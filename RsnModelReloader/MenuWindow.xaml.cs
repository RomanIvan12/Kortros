using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Reflection;
using Window = System.Windows.Window;
using CheckBox = System.Windows.Controls.CheckBox;
using System.Net.Http;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using Application = Autodesk.Revit.ApplicationServices.Application;
using RsnModelReloader.Helpers;
using Newtonsoft.Json;

namespace RsnModelReloader
{
    public partial class MenuWindow : Window
    {
        private readonly Application _app;

        private static string _revitVersion;
        private static string _serverIp;
        private string _folderPathName;

        private static List<Folder> _folders = new List<Folder>();

        private readonly BitmapImage _folderIcon;
        private readonly BitmapImage _fileIcon;

        private List<Model> _modelList = new List<Model>();

        private Dictionary<Model, string> _dictionary = new Dictionary<Model, string>();

        public MenuWindow(Application app, string versionNumber)
        {
            InitializeComponent();

            _app = app;
            _revitVersion = versionNumber;

            // Получаем ссылки на иконки
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("RsnModelReloader.Images.RevitFile.png"))
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
            using (Stream stream = assembly.GetManifestResourceStream("RsnModelReloader.Images.Folder.png"))
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
            var listOfips = GetServerIps(versionNumber);
            ServerIPs.ItemsSource = listOfips;
            ServerIPs.SelectedItem = listOfips[0];
            Logger.Log.Info("Reloader Initialized");
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
                        _folders = folders; // список всех папочек
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log.Info(ex.Message);
                }
            }
        }

        //Событие нажатия кнопки. Получаем список папок проектов. Присваиваем значения IP & RevitVersion переменным для дальнейшего обращения к ним
        // Активируем Комбобокс с списком папок
        private void PickProject_Click(object sender, RoutedEventArgs e)
        {
            string ip = ServerIPs.SelectedItem.ToString();
            _serverIp = ip;
            string url = "http://" + ip + "/RevitServerAdminRESTService" + _revitVersion + "/AdminRESTService.svc/";

            Task.Run(() => RSFolderContent(url)).Wait();

            trvContent.Items.Clear();

            foreach (var folder in _folders)
            {
                string foldPathName = folder.Name;
                _folderPathName = foldPathName;

                TreeViewItem root1 = new TreeViewItem
                {
                    Header = _folderPathName
                };
                trvContent.Items.Add(root1);

                List<Model> modelList = new List<Model>();
                AddContent(root1, _folderPathName, _folderPathName, modelList);
                foreach (var model in modelList)
                {
                    _modelList.Add(model); // Получаем список отмеченных моделей текущей папки
                }

                // Получил полный словарь Model: путь для послудующего сопоставления отмеченных Model с этим словарем
                Dictionary<Model, string> dictionary = new Dictionary<Model, string>();
                GetModelsDict(dictionary, _folderPathName);

                foreach (var dict in dictionary)
                {
                    _dictionary[dict.Key] = dict.Value;
                }
            }
            Select.IsEnabled = true;
        }

        #region Создание и работа с TreeView
        //Рекурсивная функция добавления элементов в существующее TreeView
        //folderPath - название Папки проекта (напр. "TEST", "TEST|AR", "TEST|AR|Folder1")
        //folderName - имя папки проекта (напр. "TEST", "AR", "Folder1")
        private void AddContent(TreeViewItem header, string folderPath, string folderName, List<Model> modelList)
        {
            //Add Root Folders
            header.Header = GetTreeView(folderName, _folderIcon);
           
            var xxx = GetContent(folderPath);
            List<Folder> folders = JsonConvert.DeserializeObject<RootContent>(xxx).Folders;
            foreach (Folder folder in folders)
            {
                TreeViewItem folderItem = new TreeViewItem { IsExpanded = false };
                folderItem.Header = GetTreeView(folder.Name, _folderIcon);
                header.Items.Add(folderItem);

                AddContent(folderItem, folderPath + "|" + folder.Name, folder.Name, modelList);
            }

            List<Model> models = JsonConvert.DeserializeObject<RootContent>(xxx).Models;
            foreach (Model model in models)
            {
                modelList.Add(model);
                var child = GetTreeView(model.Name, _fileIcon);
                child.Tag = "Model";
                child.Margin = new Thickness(30, 0, 0, 0);
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

        // Создаёт в TreeViewItem StackPanel из чекбокс - лого - текст
        private TreeViewItem GetTreeView(string text, BitmapImage bitImage)
        {
            TreeViewItem item = new TreeViewItem { IsExpanded = true };
            StackPanel stackPanel = new StackPanel { Orientation = Orientation.Horizontal };

            CheckBox checkBox = new CheckBox
            {
                IsChecked = false,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 0, 5, 0),
                Tag = item // Сохраняем ссылку на TreeViewItem в Tag чекбокса
            };

            checkBox.Checked += CheckBox_CheckedChanged;
            checkBox.Unchecked += CheckBox_CheckedChanged;

            Image image = new Image { Source = bitImage, Width = 16, Height = 16, Margin = new Thickness(5, 0, 5, 0) };
            Label label = new Label { Content = text };

            stackPanel.Children.Add(checkBox);
            stackPanel.Children.Add(image);
            stackPanel.Children.Add(label);

            item.Header = stackPanel;
            return item;
        }

        private void CheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Tag is TreeViewItem item)
            {
                SetChildrenChecked(item, checkBox.IsChecked == true);
            }
        }


        private void SetChildrenChecked(TreeViewItem item, bool isChecked)
        {
            StackPanel header = item.Header as StackPanel;
            TreeViewItem parent = item.Parent as TreeViewItem;
            var tag = item.Tag;

            if (tag == null)
            {
                foreach (var childFolder in parent.Items)
                {
                    TreeViewItem childItem = childFolder as TreeViewItem;
                    if (childItem.Items.Count == 0)
                    {
                        if (childItem.Header is StackPanel panel)
                        {
                            foreach (var child in panel.Children)
                            {
                                if (child is CheckBox checkBox)
                                {
                                    if (checkBox.IsChecked != isChecked)
                                        checkBox.IsChecked = isChecked;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        TreeViewItem secondLevel = childItem.Header as TreeViewItem;
                        if (secondLevel.Header is StackPanel panel)
                        {
                            foreach (var child in panel.Children)
                            {
                                if (child is CheckBox checkBox)
                                {
                                    if (checkBox.IsChecked != isChecked)
                                        checkBox.IsChecked = isChecked;
                                    break;
                                }
                            }
                        }
                        SetChildrenChecked(secondLevel, isChecked);
                    }
                }
            }
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
            List<Model> selectedModels = new List<Model>();

            foreach (TreeViewItem item in trvContent.Items)
            {
                GetSelectedStackPanels(item, selectedPanels, selectedModels);
            }

            //Получить слооварь Model : Path к всем выбранным файлам
            List<string> selectedModelNames = selectedModels.Select(x => x.Name).ToList();
            Dictionary<Model, string> newDictionary = _dictionary.Where(pair => selectedModelNames.Contains(pair.Key.Name)).ToDictionary(pair => pair.Key, pair => pair.Value);

            foreach (var i in newDictionary)
                Logger.Log.Info($"Модель {i.Key.Name}, Путь на сервере: {i.Value}");

            // Запуск кода с отделением и сохранением
            OpenCloseFunctions openClose = new OpenCloseFunctions(_app, newDictionary);

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
                    if (treeViewItem.HasHeader == false)
                    {
                        continue;
                    }

                    if (treeViewItem.Header is StackPanel panel)
                    {
                        foreach (var child in panel.Children)
                        {
                            if (child is CheckBox checkBox)
                            {
                                if (checkBox.IsChecked != state)
                                    checkBox.IsChecked = state;
                                break;
                            }
                        }
                    }
                    else
                    {
                        TreeViewItem secondLevel = treeViewItem.Header as TreeViewItem;
                        if (secondLevel.Header is StackPanel panel2)
                        {
                            foreach (var child in panel2.Children)
                            {
                                if (child is CheckBox checkBox)
                                {
                                    if (checkBox.IsChecked != state)
                                        checkBox.IsChecked = state;
                                    break;
                                }
                            }
                        }
                        CheckUncheck(secondLevel, state);
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
    }
}
