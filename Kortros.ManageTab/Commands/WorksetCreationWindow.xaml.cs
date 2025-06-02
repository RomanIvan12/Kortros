using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Kortros.ManageTab.Commands
{
    /// <summary>
    /// Логика взаимодействия для WorksetCreationWindow.xaml
    /// </summary>
    public partial class WorksetCreationWindow : Window
    {
        Document _doc;
        List<ItemWorkset> _existedWorksets;
        public WorksetCreationWindow(Document doc, ICollection<Workset> worksets)
        {
            InitializeComponent();
            _doc = doc;
            List<ItemWorkset> worksetsCollection = new List<ItemWorkset>();
            foreach (Workset workset in worksets)
            {
                worksetsCollection.Add(new ItemWorkset
                {
                    NameValue = workset.Name,
                    Workset = workset,
                });
            }
            _existedWorksets = worksetsCollection;
            CreatedWorksets.ItemsSource = worksetsCollection;
            if (doc.Title.Contains("_AR") || doc.Title.Contains("_АР"))
            {
                Title = "Создать рабочие наборы для АР";
                SetWorksetNamesFromFiles(Properties.Resources.WorksetToImport_LINK, Properties.Resources.WorksetToImport_AR);
            }
            else if (doc.Title.Contains("_OV") || doc.Title.Contains("_ОВ"))
            {
                Title = "Создать рабочие наборы для ОВ";
                SetWorksetNamesFromFiles(Properties.Resources.WorksetToImport_LINK, Properties.Resources.WorksetToImport_OV);
            }
            else if (doc.Title.Contains("_VK") || doc.Title.Contains("_ВК"))
            {
                Title = "Создать рабочие наборы для ВК";
                SetWorksetNamesFromFiles(Properties.Resources.WorksetToImport_LINK, Properties.Resources.WorksetToImport_VK);
            }
            else if (doc.Title.Contains("_SS") || doc.Title.Contains("_СС"))
            {
                Title = "Создать рабочие наборы для СС";
                SetWorksetNamesFromFiles(Properties.Resources.WorksetToImport_LINK, Properties.Resources.WorksetToImport_SS);
            }
            else if (doc.Title.Contains("_КР") || doc.Title.Contains("_KR"))
            {
                Title = "Создать рабочие наборы для КР";
                SetWorksetNamesFromFiles(Properties.Resources.WorksetToImport_LINK, Properties.Resources.WorksetToImport_SS);
            }
        }

        public void SetWorksetNamesFromFiles(string myFileLink, string myFile)
        {
            string[] linesLink = myFileLink.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string[] lines = myFile.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string[] total = linesLink.Concat(lines).ToArray();
            foreach (string line in total)
            {
                CheckBox item = new CheckBox();
                item.Content = line;
                item.Height = 20;
                WorksetList.Children.Add(item);
            }
        }

        public void SetWorksetNamesFromFileToStack(string myFile)
        {
            string[] lines = myFile.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                CheckBox item = new CheckBox();
                item.Content = line;
                item.Height = 20;
                WorksetList.Children.Add(item);
            }
        }

        private void CheckAll_Checked(object sender, RoutedEventArgs e)
        {
            UIElementCollection checkBoxes = WorksetList.Children;
            foreach (UIElement item in checkBoxes)
            {
                CheckBox checkBox = item as CheckBox;
                checkBox.IsChecked = true;
            }
        }
        private void CheckAll_Unchecked(object sender, RoutedEventArgs e)
        {
            UIElementCollection checkBoxes = WorksetList.Children;
            foreach (UIElement item in checkBoxes)
            {
                CheckBox checkBox = item as CheckBox;
                checkBox.IsChecked = false;
            }
        }

        private void btn_CreateWorksets(object sender, RoutedEventArgs e)
        {
            UIElementCollection worksetNames = WorksetList.Children;

            using (Transaction t = new Transaction(_doc, "Создание рабочих наборов"))
            {
                t.Start();
                foreach(UIElement item in worksetNames)
                {
                    CheckBox workset = item as CheckBox;
                    string name = workset.Content.ToString();
                    if ((bool)workset.IsChecked && !_existedWorksets.Select(ws => ws.NameValue).ToList().Contains(name))
                    {
                        Workset.Create(_doc, name);
                    }
                }
                t.Commit();
                if (t.GetStatus() == TransactionStatus.Committed)
                {
                    TaskDialog.Show("Оповещение", "Рабочие наборы созданы", TaskDialogCommonButtons.Ok);
                }
            }
            Close();
        }
    }
    public class ItemWorkset
    {
        public string NameValue { get; set; }
        public Workset Workset { get; set; }
    }
}
