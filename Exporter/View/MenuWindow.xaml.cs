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
using Newtonsoft.Json;
using System.Windows.Media.Imaging;
using ExporterFromRs.Classes;
using Application = Autodesk.Revit.ApplicationServices.Application;
using ExporterFromRs.Helpers;
using Autodesk.Revit.DB;
using File = System.IO.File;

namespace ExporterFromRs.View
{
    public partial class MenuWindow : Window
    {
        #region Свойства
        private readonly Application _app;
        private readonly BitmapImage _folderIcon;
        private readonly BitmapImage _fileIcon;

        private static string _revitVersion;
        private static string _serverIp;
        private string _folderPathName;

        private static List<Folder> _folders = new List<Folder>();
        private List<Model> _modelList = new List<Model>();
        private Dictionary<Model, string> _dictionary = new Dictionary<Model, string>();
        public static bool PurgeElementsEnabled { get; set; }
        #endregion

        public MenuWindow(Application app, string versionNumber)
        {
            InitializeComponent();
            
            _app = app;
            _revitVersion = versionNumber;

            // Загружаем иконки из ресурсов
            _fileIcon = LoadEmbeddedImage("ExporterFromRs.Images.RevitFile.png");
            _folderIcon = LoadEmbeddedImage("ExporterFromRs.Images.Folder.png");

            if (OptionalFunctionalityUtils.IsNavisworksExporterAvailable())
            {
                try
                {
                    Convert.IsEnabled = true;
                    Convert.IsChecked = true;
                    Rebar.IsEnabled = true;
                    Rebar.IsChecked = true;
                }
                catch (Exception ex)
                {
                    Logger.Log.Info(ex.Message);
                }
            }
            else
                Logger.Log.Info("Navisworks Exporter для данной версии ревит не установлен. Функция создания .nwc была заблокирована");

            ServerIPs.ItemsSource = GetServerIps(versionNumber);
            ProjectName.ItemsSource = string.Empty;

            Logger.Log.Info("Exporter Initialized");
        }

        private BitmapImage LoadEmbeddedImage(string resourcePath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(resourcePath))
            {
                if (stream == null) return null;

                var icon = new BitmapImage();
                icon.BeginInit();
                icon.StreamSource = stream;
                icon.EndInit();
                return icon;
            }
        }

        private string[] GetServerIps(string versionNumber)
        {
            string configFilePath = $@"C:\ProgramData\Autodesk\Revit Server {versionNumber}\Config\RSN.ini";
            if (!File.Exists(configFilePath))
            {
                Logger.Log.Error("Конфигурационный файл RSN.ini не найден");
                throw new FileNotFoundException($"Конфигурационный файл не найден: {configFilePath}");
            }
            return File.ReadAllLines(configFilePath);
        }

        // Событие нажатия кнопки. Получаем список папок проектов. Присваиваем значения IP & RevitVersion переменным для дальнейшего обращения к ним
        private void ServerIPs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ServerIPs.SelectedItem == null) return;

            _serverIp = ServerIPs.SelectedItem.ToString();
            string url = $"http://{_serverIp}/RevitServerAdminRESTService{_revitVersion}/AdminRESTService.svc/";

            Task.Run(() => RSFolderContent(url)).Wait();

            ProjectName.IsEnabled = true;
            ProjectName.ItemsSource = _folders;
        }
        
        private static HttpClient CreateHttpClient(string baseAdress)
        {
            var client = new HttpClient()
            {
                BaseAddress = new Uri(baseAdress)
            };
            client.DefaultRequestHeaders.Add("User-Name", Environment.UserName);
            client.DefaultRequestHeaders.Add("User-Machine-Name", Environment.MachineName);
            client.DefaultRequestHeaders.Add("Operation-GUID", Guid.NewGuid().ToString());
            return client;
        }

        //Функция получения списка корневых папок по гетзапросу (папки проектов)
        private static void RSFolderContent(string uri)
        {
            using (HttpClient client = CreateHttpClient(uri))
            {
                try
                {
                    HttpResponseMessage response = client.GetAsync("|/contents").GetAwaiter().GetResult();

                    if (response.IsSuccessStatusCode)
                    {
                        string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        _folders = JsonConvert.DeserializeObject<RootContent>(result).Folders;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log.Info(ex.Message);
                }
            }
        }

        private void PickProject_Click(object sender, RoutedEventArgs e)
        {
            trvContent.Items.Clear();

            if (!(ProjectName.SelectedItem is Folder selectedFolder))
            {
                Logger.Log.Error("Проект не выбран");
                return;
            }

            _folderPathName = selectedFolder.Name;
            Logger.Log.Info( $"Выбран проект - {_folderPathName}");

            TreeViewItem root = new TreeViewItem { Header = _folderPathName };
            trvContent.Items.Add(root);

            _modelList = new List<Model>(); // Получаем список отмеченных моделей
            AddContentToTreeView(root, _folderPathName, _folderPathName, _modelList);

            // Получил полный словарь Model: путь для послудующего сопоставления отмеченных Model с этим словарем
            _dictionary = new Dictionary<Model, string>();
            GetModelsDict(_dictionary, _folderPathName);

            Select.IsEnabled = true;
        }
        
        #region Создание и работа с TreeView        
        // Переделка AddContent
        private void AddContentToTreeView(TreeViewItem parentItem, string folderPath, string folderName, List<Model> modelList)
        {
            var headerPanel = CreateTreeViewHeader(folderName, _folderIcon);
            parentItem.Header = headerPanel;

            var content = GetContent(folderPath);
            var rootContent = JsonConvert.DeserializeObject<RootContent>(content);
            AddFoldersToTreeView(parentItem, folderPath, rootContent.Folders, modelList);
            AddModelsToTreeView(parentItem, rootContent.Models, modelList);
        }

        private void AddFoldersToTreeView(TreeViewItem parentItem, string parentPath, IEnumerable<Folder> folders, List<Model> modelList)
        {
            foreach (var folder in folders)
            {
                var folderItem = CreateTreeViewHeader(folder.Name, _folderIcon);
                folderItem.IsExpanded = false;
                parentItem.Items.Add(folderItem);

                AddContentToTreeView(folderItem, $"{parentPath}|{folder.Name}", folder.Name, modelList);
            }
        }

        private void AddModelsToTreeView(TreeViewItem parentItem, IEnumerable<Model> models, List<Model> modelList)
        {
            foreach (var model in models)
            {
                modelList.Add(model);
                var modelItem = CreateTreeViewHeader(model.Name, _fileIcon);
                modelItem.Tag = "Model";
                modelItem.Margin = new Thickness(30, 0, 0, 0);
                parentItem.Items.Add(modelItem);
            }
        }

        private TreeViewItem CreateTreeViewHeader(string text, BitmapImage icon)
        {
            var treeViewItem = new TreeViewItem();

            var checkBox = new CheckBox()
            {
                IsChecked = false,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 0, 5, 0),
                Tag = treeViewItem // Сохраняем ссылку на TreeViewItem в Tag чекбокса
            };

            checkBox.Checked += CheckBox_CheckedChanged;
            checkBox.Unchecked += CheckBox_CheckedChanged;

            var stackPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    checkBox,
                    new Image { Source = icon, Width = 16, Height = 16, Margin = new Thickness(5, 0, 5, 0) },
                    new TextBlock { Text = text }
                }
            };

            treeViewItem.Header = stackPanel;
            return treeViewItem;
        }

        // Рекурсивная функция добавления элементов в существующее TreeView
        // folderPath - название Папки проекта (напр. "TEST", "TEST|AR", "TEST|AR|Folder1")
        // folderName - имя папки проекта (напр. "TEST", "AR", "Folder1")
        // Получаем содержимое в выбранной папке. Возвращает значение JSON типа для указанного пути
        private static string GetContent(string path)
        {
            string url = $"http://{_serverIp}/RevitServerAdminRESTService{_revitVersion}/AdminRESTService.svc/";

            using (HttpClient client = CreateHttpClient(url))
            {
                try
                {
                    HttpResponseMessage response = client.GetAsync(path + "/contents").GetAwaiter().GetResult();
                    if (response.IsSuccessStatusCode)
                        return response.Content.ReadAsStringAsync().GetAwaiter().GetResult(); //Получили папки
                }
                catch (Exception ex)
                {
                    Logger.Log.Error(ex.ToString());
                }
                return string.Empty;
            }
        }

        private void CheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Tag is TreeViewItem item)
                SetChildrenCheckedState(item, checkBox.IsChecked == true);
        }

        private void SetChildrenCheckedState(TreeViewItem item, bool isChecked)
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
                        SetChildrenCheckedState(secondLevel, isChecked);
                    }
                }
            }
        }
        #endregion

        private RootContent GetRootContent(string folder)
        {
            //Получить все возможные пути
            var content = GetContent(folder); // Получаем содержимое в выбранной папке. Возвращает значение JSON типа для указанного пути
            return JsonConvert.DeserializeObject<RootContent>(content);
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

            LogOptionsState();

            List<StackPanel> selectedPanels = new List<StackPanel>();
            var selectedModels = GetSelectedModels();
            //Получить слооварь Model : Path к всем выбранным файлам
            List<string> selectedModelNames = selectedModels.Select(x => x.Name).ToList();

            Dictionary<Model, string> newDictionary = _dictionary
                .Where(pair => selectedModelNames.Contains(pair.Key.Name))
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            LogSelectedModels(newDictionary);

            // Запуск кода с отделением и сохранением
            DetachFile makeDetach = new DetachFile(
                _app,
                newDictionary,
                _folderPathName,
                _revitVersion,
                Convert.IsChecked.Value,
                Rebar.IsChecked.Value
            );
            Close();
        }

        private void LogOptionsState()
        {
            PurgeElementsEnabled = PurgeElements.IsChecked == true;
            Logger.Log.Info($"Режим очистки модели: {(PurgeElementsEnabled ? "включен" : "выключен")}");
            Logger.Log.Info($"Функция создания .nwc: {(Convert.IsChecked == true ? "включена" : "выключена")}");
        }

        private void LogSelectedModels(Dictionary<Model, string> selectedModels)
        {
            foreach (var i in selectedModels)
                Logger.Log.Info($"Модель {i.Key.Name}, Путь на сервере: {i.Value}");
        }

        // Получил отмеченные пользователем в TreeView элементы. Возвращает список stackPanel и список Model
        private List<Model> GetSelectedModels()
        {
            var selectedModels = new List<Model>();
            foreach (TreeViewItem item in trvContent.Items)
                FindSelectedModels(item, selectedModels);

            return selectedModels;
        }

        private void FindSelectedModels(ItemsControl parentItem, List<Model> selectedModels)
        {
            foreach (var item in parentItem.Items)
            {
                if (!(item is TreeViewItem treeViewItem)) continue;

                if (treeViewItem.Header is StackPanel panel)
                {
                    CheckBox checkBox = panel.Children.OfType<CheckBox>().FirstOrDefault();
                    if (checkBox?.IsChecked == true)
                    {
                        var label = panel.Children.OfType<TextBlock>().FirstOrDefault();
                        if (label != null)
                        {
                            Model model = _modelList.FirstOrDefault(x => x.Name == label.Text.ToString());
                            if (model != null)
                                selectedModels.Add(model);
                        }
                    }
                }
                FindSelectedModels(treeViewItem, selectedModels);
            }
        }

        private void SetAllCheckboxesState(bool isChecked)
        {
            foreach (TreeViewItem item in trvContent.Items)
                SetCheckboxStateRecursive(item, isChecked);
        }

        private void SetCheckboxStateRecursive(ItemsControl parentItem, bool isChecked)
        {
            foreach (var item in parentItem.Items)
            {
                if (!(item is TreeViewItem treeViewItem)) continue;

                if (treeViewItem.Header is StackPanel panel)
                {
                    foreach (var checkBox in panel.Children.OfType<CheckBox>())
                    {
                        if (checkBox.IsChecked != isChecked)
                            checkBox.IsChecked = isChecked;
                    }
                }
                SetCheckboxStateRecursive(treeViewItem, isChecked);
            }
        } 

        private void checkBoxConvert_CheckedChanged(object sender, EventArgs e)
        {
            // Проверяем, включён ли Convert
            Rebar.IsEnabled = Convert.IsChecked == true;
            if (!Rebar.IsEnabled)
                Rebar.IsChecked = false;
        }

        private void CheckAllChains(object sender, RoutedEventArgs e)
        {
            SetAllCheckboxesState(true);
        }
        private void UnCheckAllChains(object sender, RoutedEventArgs e)
        {
            SetAllCheckboxesState(false);
        }
    }
}
