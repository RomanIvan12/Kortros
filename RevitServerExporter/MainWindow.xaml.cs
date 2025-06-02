using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.IO;
using System.Reflection;
using Window = System.Windows.Window;
using CheckBox = System.Windows.Controls.CheckBox;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.Windows.Media.Media3D;
using RevitServerExporter.Classes;
using RevitServerExporter.CoreFunc;

namespace RevitServerExporter
{
    public partial class MainWindow : Window
    {
        static private List<Folder> _folders = new List<Folder>();

        private BitmapImage _folderIcon;
        private BitmapImage _fileIcon;
        private List<Model> _modelList;

        private string _serverIp;
        private string _revitVersion;
        private string _folderPathName;

        private Dictionary<Model, string> _dictionary;

        private static Dictionary<Model, string> _SelectedDictionary;

        public static Dictionary<Model, string> SelectedDictionary
        {
            get { return _SelectedDictionary; }
            set { _SelectedDictionary = value; }
        }

        public MainWindow()
        {
            InitializeComponent();

            // Получаем ссылки на иконки
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("RvtServerExporter.Icons.RevitFile.png"))
            {
                if (stream != null)
                {
                    BitmapImage icon = new BitmapImage();
                    icon.BeginInit();
                    icon.StreamSource = stream;
                    icon.EndInit();
                    _fileIcon = icon;
                }
            }
            using (Stream stream = assembly.GetManifestResourceStream("RvtServerExporter.Icons.Folder.png"))
            {
                if (stream != null)
                {
                    BitmapImage icon = new BitmapImage();
                    icon.BeginInit();
                    icon.StreamSource = stream;
                    icon.EndInit();
                    _folderIcon = icon;
                }
            }
            using (Stream stream = assembly.GetManifestResourceStream("RvtServerExporter.Icons.Server.png"))
            {
                if (stream != null)
                {
                    BitmapImage icon = new BitmapImage();
                    icon.BeginInit();
                    icon.StreamSource = stream;
                    icon.EndInit();
                    //_serverIcon = icon;
                }
            }

            //Выбираемые версии ревита
            SelectedVersion.ItemsSource = new string[]
            {
                "2020",
                "2021",
                "2022",
                "2023",
                "2024"
            };

            ServerIPs.ItemsSource = new string[]
            {
                "Srv-revit",
                "172.20.0.53"
            };
            ProjectName.ItemsSource = "";
        }

        //Функция получения списка корневых папок по гетзапросу (папки проектов)
        private static void RSFolderContent(string uri)
        {
            //Create httpclient instance
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(uri);
                client.DefaultRequestHeaders.Add("User-Name", Environment.UserName);
                client.DefaultRequestHeaders.Add("User-Machine-Name", Environment.MachineName);
                client.DefaultRequestHeaders.Add("Operation-GUID", Guid.NewGuid().ToString());
                try
                {
                    //отправляем Гет запрос
                    HttpResponseMessage response = client.GetAsync("|/contents").GetAwaiter().GetResult();

                    if (response.IsSuccessStatusCode)
                    {
                        string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        List<Folder> folders = JsonConvert.DeserializeObject<RootContent>(result).Folders;
                        _folders = folders;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        //Событие нажатия кнопки. Получаем список папок проектов. Присваиваем значения IP & RevitVersion переменным для дальнейшего обращения к ним
        // Активируем Комбобокс с списком папок
        private void Pick_Click(object sender, RoutedEventArgs e)
        {
            string ip = ServerIPs.SelectedItem.ToString();
            string revitVersion = SelectedVersion.SelectedItem.ToString();
            string url = "http://" + ip + "/RevitServerAdminRESTService" + revitVersion + "/AdminRESTService.svc/";

            Task.Run(() => RSFolderContent(url)).Wait();

            //RSFolderContent(url);
            ProjectName.IsEnabled = true;
            ProjectName.ItemsSource = _folders;

            _serverIp = ip;
            _revitVersion = revitVersion;
            //запишем переменные в свойсва класса
        }

        #region Создание и работа с TreeView
        //Рекурсивная функция добавления элементов в существующее TreeView
        //folderPath - название Папки проекта (напр. "TEST", "TEST|AR", "TEST|AR|Folder1")
        //folderName - имя папки проекта (напр. "TEST", "AR", "Folder1")
        private void AddContent(TreeViewItem header, string folderPath, string folderName, List<Model> modelList)
        {
            //Add Root Folders
            header.Header = folderName;

            var xxx = GetContent(folderPath);

            List<Folder> folders = JsonConvert.DeserializeObject<RootContent>(xxx).Folders;
            foreach (Folder folder in folders)
            {
                var folderChild = GetTreeView(folder.Name, _folderIcon); // ПОТОМ НАСТРОИТЬ ИКОНКУ. Не добавляется для папок
                //TreeViewItem folderChild = new TreeViewItem();
                //folderChild.Header = folder.Name;
                header.Items.Add(folderChild);

                AddContent(folderChild, folderPath + "|" + folder.Name, folder.Name, modelList);
            }
            //Получили файлы
            List<Model> models = JsonConvert.DeserializeObject<RootContent>(xxx).Models;

            foreach (Model model in models)
            {
                modelList.Add(model);
                var child = GetTreeView(model.Name, _fileIcon);
                header.Items.Add(child);
            }
        }

        // Получаем содержимое в выбранной папке. Возвращает значение JSON типа для указанного пути
        private static string GetContent(string path)
        {
            string url = "http://Srv-revit/RevitServerAdminRESTService2022/AdminRESTService.svc/";
            //Create httpclient instance
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Add("User-Name", Environment.UserName);
                client.DefaultRequestHeaders.Add("User-Machine-Name", Environment.MachineName);
                client.DefaultRequestHeaders.Add("Operation-GUID", Guid.NewGuid().ToString());
                try
                {
                    //отправляем Гет запрос
                    HttpResponseMessage response = client.GetAsync(path + "/contents").GetAwaiter().GetResult();

                    if (response.IsSuccessStatusCode)
                    {
                        //Получили папки
                        string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
                return string.Empty;
            }
        }

        // Создаёт в TreeViewItem StackPanel из лого - текст - чекбокс
        private TreeViewItem GetTreeView(string text, BitmapImage bitImage)
        {
            TreeViewItem item = new TreeViewItem();
            item.IsExpanded = true;

            //create stackpanel
            StackPanel stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Horizontal;

            //create image
            Image image = new Image();
            image.Source = bitImage;
            image.Width = 16; image.Height = 16;

            //create label
            Label label = new Label();
            label.Content = text;

            //create checkbox
            CheckBox checkBox = new CheckBox();
            checkBox.IsChecked = false;
            checkBox.VerticalAlignment = VerticalAlignment.Center;


            stackPanel.Children.Add(image);
            stackPanel.Children.Add(label);
            stackPanel.Children.Add(checkBox);

            //assign stack
            item.Header = stackPanel;
            return item;
        }
        #endregion
        private void PickProject_Click(object sender, RoutedEventArgs e)
        {
            if (ProjectName.SelectedItem != null)
            {
                Folder folder = ProjectName.SelectedItem as Folder;
                string foldPathName = folder.Name;
                _folderPathName = foldPathName;
            }

            trvContent.Items.Clear();
            TreeViewItem root = new TreeViewItem();
            root.Header = _folderPathName;
            trvContent.Items.Add(root);

            List<Model> modelList = new List<Model>();
            AddContent(root, _folderPathName, _folderPathName, modelList);
            _modelList = modelList; // Получаем список отмеченных моделей

            // Получил полный словарь Model: путь для послудующего сопоставления отмеченных Model с этим словарем
            Dictionary<Model, string> dictionary = new Dictionary<Model, string>();
            GetModelsDict(dictionary, _folderPathName);
            _dictionary = dictionary;
        }
        private RootContent GetRootContent(string folder)
        {
            //Получить все возможные пути
            var xxx = GetContent(folder); // Получаем содержимое в выбранной папке. Возвращает значение JSON типа для указанного пути

            RootContent root = JsonConvert.DeserializeObject<RootContent>(xxx);
            return root;
        }
        // Получили словарь Model : Path, где path - полный путь к файлу всех файлов в папке проекта
        private void GetModelsDict(Dictionary<Model, string> dictionary, string folder)
        {
            string rsn = "RSN://" + _serverIp + "/";
            RootContent root = GetRootContent(folder);
            if (root != null)
            {
                if (root.Models != null)
                {
                    foreach (Model fileObj in root.Models)
                    {
                        string filelink = folder.Replace("|", "/") + "/" + fileObj.Name;
                        dictionary.Add(fileObj, rsn + filelink);
                    }
                }
                if (root.Folders != null)
                {
                    foreach (Folder folderObj in root.Folders)
                    {

                        string subfolder = folder + "|" + folderObj.Name;
                        GetModelsDict(dictionary, subfolder);
                    }
                }
            }
        }
        private void Select_Click(object sender, RoutedEventArgs e)
        {
            List<StackPanel> selectedPanels = new List<StackPanel>();
            List<Model> selectedmodels = new List<Model>();

            foreach (TreeViewItem item in trvContent.Items)
            {
                GetSelectedStackPanels(item, selectedPanels, selectedmodels);
            }

            //Получить слооварь Model : Path к всем выбранным файлам
            List<string> selectedModelNames = selectedmodels.Select(x => x.Name).ToList();
            Dictionary<Model, string> newDictionary = _dictionary.Where(pair => selectedModelNames.Contains(pair.Key.Name)).ToDictionary(pair => pair.Key, pair => pair.Value);
            _SelectedDictionary = newDictionary;

            //Нужно конвертировать Dictionary<Model, string> newDictionary в JSON, затем записать в мои документы
            // Затем в блоке с открытием ревита удалить этот JSON

            ConvertDataJSON(newDictionary);

            //TEST всякого
            OpenRevit dataTransfer = new OpenRevit(_revitVersion);
        }

        //Функция, которая конвертирует данные в JSON
        private void ConvertDataJSON(Dictionary<Model, string> dictionary)
        {
            try
            {
                string documentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string jsonFilePath = Path.Combine(documentPath, "ModelsToExport.json");

                Dictionary<string, string> newDictionary = dictionary.ToDictionary(
                    kvp => kvp.Key.Name, kvp => kvp.Value);

                string json = JsonConvert.SerializeObject(newDictionary, Formatting.Indented);
                System.IO.File.WriteAllText(jsonFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Произошла ошибка при сохранении словаря: " + ex.Message);
            }
        }

        // Получил отмеченные пользователем в TreeView элементы. Возвращает список stackPanel и список Model
        private void GetSelectedStackPanels(TreeViewItem item, List<StackPanel> stackPanels, List<Model> selectedModels)
        {
            if (item.Header is StackPanel stackPanel)
            {
                CheckBox checkBox = stackPanel.Children.OfType<CheckBox>().FirstOrDefault();
                if (checkBox != null && checkBox.IsChecked == true)
                {
                    stackPanels.Add(stackPanel);

                    Label label = stackPanel.Children.OfType<Label>().FirstOrDefault();
                    string modelName = label.Content.ToString();
                    Model selectedModel = _modelList.FirstOrDefault(x => x.Name == modelName);
                    if (selectedModel != null)
                    {
                        selectedModels.Add(selectedModel);
                    }
                }
            }
            foreach (TreeViewItem childItem in item.Items)
            {
                GetSelectedStackPanels(childItem, stackPanels, selectedModels);
            }
        }
    }
}

/*

private void CheckAllChains(object sender, RoutedEventArgs e)
{
    UIElementCollection checkBoxes = RevitFiles.Children;
    foreach (UIElement item in checkBoxes)
    {
        CheckBox checkBox = item as CheckBox;
        checkBox.IsChecked = true;
    }
}
private void UnCheckAllChains(object sender, RoutedEventArgs e)
{
    UIElementCollection checkBoxes = RevitFiles.Children;
    foreach (UIElement item in checkBoxes)
    {
        CheckBox checkBox = item as CheckBox;
        checkBox.IsChecked = false;
    }
}
private void ToggleAllChains(object sender, RoutedEventArgs e)
{
    UIElementCollection checkBoxes = RevitFiles.Children;
    foreach (UIElement item in checkBoxes)
    {
        CheckBox checkBox = item as CheckBox;
        if (checkBox.IsChecked == true)
        {
            checkBox.IsChecked = false;
        }
        else if (checkBox.IsChecked == false)
        {
            checkBox.IsChecked = true;
        }
    }
}
*/
