using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using CreateLinks.Helpers;
using System.IO;
using System.Reflection;
using System.Net.Http;
using Newtonsoft.Json;
using System.Diagnostics;
using Autodesk.Revit.DB;

namespace CreateLinks.View
{
    public partial class MainWindow : Window
    {
        private readonly Document _doc;
        private readonly Application _app;


        private static string _revitVersion;
        private static string _serverIp;
        private string _folderPathName;

        private static List<Folder> _folders = new List<Folder>();

        private readonly BitmapImage _folderIcon;
        private readonly BitmapImage _fileIcon;

        private List<Model> _modelList;


        private Dictionary<Model, string> _dictionary;

        public MainWindow(string versionNumber, Document doc)
        {
            InitializeComponent();
            _doc = doc;
            _revitVersion = versionNumber;

            // Получаем ссылки на иконки
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("CreateLinks.Resources.Icons.RevitFile.png"))
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
            using (Stream stream = assembly.GetManifestResourceStream("CreateLinks.Resources.Icons.Folder.png"))
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
            //PickProjectFunc();

            ServerIPs.ItemsSource = GetServerIps(versionNumber);
            ProjectName.ItemsSource = "";
            Logger.Log.Info("Exporter Initialized");
        }

        private string[] GetServerIps(string versionNumber)
        {
            string configFilePath = $@"C:\ProgramData\Autodesk\Revit Server {versionNumber}\Config\RSN.ini";
            if (!System.IO.File.Exists(configFilePath))
            {
                Logger.Log.Error("Конфигурационный файл RSN.ini не найден");
                throw new FileNotFoundException($"Конфигурационный файл не найден: {configFilePath}");
            }
            string[] serverIps = System.IO.File.ReadAllLines(configFilePath);

            return serverIps;
        }


        //Событие нажатия кнопки. Получаем список папок проектов. Присваиваем значения IP & RevitVersion переменным для дальнейшего обращения к ним
        // Активируем Комбобокс с списком папок
        private void ServerIPs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string ip = ServerIPs.SelectedItem.ToString();
            _serverIp = ip;
            string url = "http://" + ip + "/RevitServerAdminRESTService" + _revitVersion + "/AdminRESTService.svc/";

            Task.Run(() => RSFolderContent(url)).Wait();

            ProjectName.IsEnabled = true;
            ProjectName.ItemsSource = _folders;
        }

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
                    Logger.Log.Info(ex.Message);
                }
            }
        }

        private void PickProject_Click(object sender, RoutedEventArgs e)
        {
            trvContent.Items.Clear();

            if (ProjectName.SelectedItem != null)
            {
                Folder folder = ProjectName.SelectedItem as Folder;
                string foldPathName = folder.Name;
                _folderPathName = foldPathName;
                Logger.Log.Info($"Выбран проект - {foldPathName}");

                TreeViewItem root = new TreeViewItem
                {
                    Header = _folderPathName
                };
                trvContent.Items.Add(root);

                List<Model> modelList = new List<Model>();
                AddContent(root, _folderPathName, _folderPathName, modelList);
                _modelList = modelList; // Получаем список отмеченных моделей

                // Получил полный словарь Model: путь для последующего сопоставления отмеченных Model с этим словарем
                Dictionary<Model, string> dictionary = new Dictionary<Model, string>();
                GetModelsDict(dictionary, _folderPathName);
                _dictionary = dictionary;
                Select.IsEnabled = true;
            }
            else
            {
                Logger.Log.Error("Проект не выбран");
            }
        }
        private void PickProjectFunc()
        {
            trvContent.Items.Clear();

            _folderPathName = "УГА|003_Конструктор планировок";
            TreeViewItem root = new TreeViewItem
            {
                Header = _folderPathName
            };
            trvContent.Items.Add(root);

            List<Model> modelList = new List<Model>();
            AddContent(root, _folderPathName, "003_Конструктор планировок", modelList);
            _modelList = modelList; // Получаем список отмеченных моделей

            // Получил полный словарь Model: путь для послудующего сопоставления отмеченных Model с этим словарем
            Dictionary<Model, string> dictionary = new Dictionary<Model, string>();
            GetModelsDict(dictionary, _folderPathName);
            _dictionary = dictionary;
            Select.IsEnabled = true;
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
            string url = "http://" + _serverIp + "/RevitServerAdminRESTService" + _revitVersion + "/AdminRESTService.svc/";
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
            TreeViewItem item = new TreeViewItem
            {
                IsExpanded = true
            };

            //create stackpanel
            StackPanel stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            //create image
            Image image = new Image
            {
                Source = bitImage,
                Width = 16,
                Height = 16
            };

            //create label
            Label label = new Label
            {
                Content = text
            };

            //create checkbox
            CheckBox checkBox = new CheckBox
            {
                IsChecked = false,
                VerticalAlignment = VerticalAlignment.Center
            };

            stackPanel.Children.Add(image);
            stackPanel.Children.Add(label);
            stackPanel.Children.Add(checkBox);

            //assign stack
            item.Header = stackPanel;
            return item;
        }
        #endregion

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
            string ip = ServerIPs.SelectedItem.ToString();
            string rsn = "RSN://" + ip + "/";
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
            Logger.Log.Info($"Button {Select.Content} was pressed");

            List<StackPanel> selectedPanels = new List<StackPanel>();
            List<Model> selectedmodels = new List<Model>();

            foreach (TreeViewItem item in trvContent.Items)
            {
                GetSelectedStackPanels(item, selectedPanels, selectedmodels);
            }

            //Получить слооварь Model : Path к всем выбранным файлам
            List<string> selectedModelNames = selectedmodels.Select(x => x.Name).ToList();
            Dictionary<Model, string> newDictionary = _dictionary.Where(pair => selectedModelNames.Contains(pair.Key.Name)).ToDictionary(pair => pair.Key, pair => pair.Value);
            
            // Запуск кода с отделением и сохранением
            AddLinks makeDetach = new AddLinks(_doc, newDictionary);



            Close();
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


        private void CheckUncheck(ItemsControl parentItem, bool? state = null)
        {
            foreach (var item in parentItem.Items)
            {
                if (item is TreeViewItem treeViewItem)
                {
                    if (treeViewItem.Header.GetType() == typeof(string))
                    {
                        CheckUncheck(treeViewItem, state);
                    }
                    else if (treeViewItem.Header.GetType() == typeof(StackPanel))
                    {
                        StackPanel stack = treeViewItem.Header as StackPanel;
                        foreach (var child in stack.Children)
                        {
                            if (child is CheckBox checkBox && state != null)
                            {
                                checkBox.IsChecked = state;
                            }
                            else if (child is CheckBox xxx && state == null)
                            {
                                if (xxx.IsChecked == true)
                                {
                                    xxx.IsChecked = false;
                                }
                                else if (xxx.IsChecked == false)
                                {
                                    xxx.IsChecked = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CheckAllChains(object sender, RoutedEventArgs e)
        {
            CheckUncheck(trvContent, true);
        }
        private void UnCheckAllChains(object sender, RoutedEventArgs e)
        {
            CheckUncheck(trvContent, false);
        }
        private void ToggleAllChains(object sender, RoutedEventArgs e)
        {
            CheckUncheck(trvContent);
        }
    }
}
