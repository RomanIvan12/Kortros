using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MessageBox = System.Windows.Forms.MessageBox;
using System.Drawing;
using System.Net;
using System.IO;
using System.Xml;
using System.Reflection;
using Application = Autodesk.Revit.ApplicationServices.Application;
using Newtonsoft.Json;
using System.Security.Cryptography;
using Window = System.Windows.Window;
using CheckBox = System.Windows.Controls.CheckBox;

namespace Kortros.Commands.ModelData
{
    /// <summary>
    /// Логика взаимодействия для RSNfilesWindow.xaml
    /// </summary>
    public partial class RSNfilesWindow : Window
    {
        Document _doc;
        string[] IpAdresses;


        private string selectedServerValue;
        private List<string> selectedProjects;


        private List<string> shortNames;
        private List<string> longNames = new List<string>();


        List<string> FilePathes;


        public event Action<Document, List<string>> FilesPathSelected;

        public RSNfilesWindow(Document doc, string[] ipAdresses)
        {
            InitializeComponent();
            IpAdresses = ipAdresses;
            _doc = doc;
            foreach (string ip in IpAdresses)
            {
                ServerIPs.Items.Add(ip);
            }
        }


        private void Select_Click(object sender, RoutedEventArgs e)
        {
            List<string> allCheckedFiles = new List<string>(); //выбранные файлы
            UIElementCollection checkBoxes = RevitFiles.Children;
            StringBuilder sb = new StringBuilder();
            foreach (UIElement item in checkBoxes)
            {
                CheckBox checkBox = item as CheckBox;
                if (checkBox.IsChecked == true)
                {
                    //allCheckedFiles.Add(checkBox.Content.ToString());
                    string SV = checkBox.Content.ToString();
                    foreach (var i in longNames)
                    {
                        if (i.Contains(SV))
                        {
                            allCheckedFiles.Add(i);
                        }
                    }
                }
                FilePathes = allCheckedFiles;
            }
            /*
            foreach (var i in FilePathes)
            {
                sb.AppendLine(i);
            }
            MessageBox.Show(sb.ToString());
            */
            FilesPathSelected?.Invoke(_doc, FilePathes);
            this.Close();
        }
        private void Pick_Click(object sender, RoutedEventArgs e)
        {
            string ip = ServerIPs.SelectedItem.ToString();
            string pickedProject = ProjectNumber.SelectedItem.ToString();
            Application app = _doc.Application;
            string versionNumber = app.VersionNumber;
            string url = "http://" + ip + "/RevitServerAdminRESTService" + versionNumber + "/AdminRESTService.svc/";
            string rsn = "RSN://" + ip + "/";

            List<string> Projects = RSContent(url, rsn, pickedProject);
            RevitFiles.Children.Clear();
            foreach (string name in Projects)
            {
                string remove = name.Split(new[] { pickedProject }, StringSplitOptions.None)[1];
                CheckBox item = new CheckBox
                {
                    Content = remove
                };
                RevitFiles.Children.Add(item); //место добавления в stackpanel

                longNames.Add(name);
                //shortNames.Add(remove);
                //FilePathes.Add(name);
            }
        }

        private void ServerIPs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                string ip = ServerIPs.SelectedItem.ToString();
                Application app = _doc.Application;
                string versionNumber = app.VersionNumber;
                string url = "http://" + ip + "/RevitServerAdminRESTService" + versionNumber + "/AdminRESTService.svc/";
                selectedProjects = RSFolderContent(url)[0];
                ProjectNumber.Items.Clear();
                foreach (string prjct in RSFolderContent(url)[0])
                {
                    ProjectNumber.Items.Add(prjct);
                }
            }
            catch (Exception) { }
        }

        private static List<List<string>> RSFolderContent(string uri, string folder = "|")
        {
            string urlCont = uri + folder + "/contents";
            try
            {
                WebRequest request = WebRequest.Create(urlCont);
                request.Method = "GET";
                request.Headers.Add("User-Name", Environment.UserName);
                request.Headers.Add("User-Machine-Name", Environment.MachineName);
                request.Headers.Add("Operation-GUID", Guid.NewGuid().ToString());

                using (WebResponse response = request.GetResponse())
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string responseStr = reader.ReadToEnd();
                    dynamic resultjson = JsonConvert.DeserializeObject(responseStr);

                    List<string> folderNames = new List<string>();
                    List<string> filesNames = new List<string>();

                    foreach (dynamic folderObj in resultjson["Folders"])
                    {
                        folderNames.Add(folderObj["Name"].ToString());
                    }
                    foreach (dynamic fileObj in resultjson["Models"])
                    {
                        filesNames.Add(fileObj["Name"].ToString());
                    }
                    return new List<List<string>> { folderNames, filesNames };
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static List<string> RSContent(string uri, string rsi, string folder = "|")
        {
            // функция показывает список файлов по данному пути (вернее их путей). Subfolder определяет корневую папку (по умолчанию - корневаяя папка)
            string urlCont = uri + folder + "/contents";
            List<string> fileslist = new List<string>();
            try
            {
                WebRequest request = WebRequest.Create(urlCont);
                request.Method = "GET";
                request.Headers.Add("User-Name", Environment.UserName);
                request.Headers.Add("User-Machine-Name", Environment.MachineName);
                request.Headers.Add("Operation-GUID", Guid.NewGuid().ToString());

                using (WebResponse response = request.GetResponse())
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string responseStr = reader.ReadToEnd();
                    dynamic resultjson = JsonConvert.DeserializeObject(responseStr);

                    if (resultjson["Models"] != null)
                    {
                        foreach (dynamic fileObj in resultjson["Models"])
                        {
                            string filelink = folder == "|" ? fileObj["Name"] : folder.Replace("|", "/") + "/" + fileObj["Name"];
                            fileslist.Add(rsi + filelink);
                        }
                    }
                    if (resultjson["Folders"] != null)
                    {
                        foreach (dynamic folderObj in resultjson["Folders"])
                        {
                            string subfolder = folder == "|" ? folderObj["Name"] : folder + "|" + folderObj["Name"];
                            fileslist.AddRange(RSContent(uri, rsi, subfolder));
                        }
                    }
                }
            }
            catch (Exception)
            {
                fileslist.Add("!ERROR! on folder - " + folder);
            }
            return fileslist;
        }

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
    }
}
