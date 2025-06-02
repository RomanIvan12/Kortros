using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autodesk.Revit.DB;
using CreateLinks.Helpers;
using Microsoft.Win32;

namespace CreateLinks.View
{
    /// <summary>
    /// Логика взаимодействия для FolderWindow.xaml
    /// </summary>
    public partial class FolderWindow : Window
    {
        private Document _doc;
        public ObservableCollection<string> FileNames { get; set; }
        public FolderWindow(string[] fileNames, Document doc)
        {
            InitializeComponent();
            _doc = doc;
            FileNames = new ObservableCollection<string>(fileNames);
            this.DataContext = this;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), // Папка "Мои документы"
                Filter = "Revit Files (*.rvt)|*.rvt", // Фильтр файлов
                Title = "Выберите файлы Revit",
                Multiselect = true // Разрешить выбор нескольких файлов
            };
            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                string[] files = openFileDialog.FileNames;
                foreach (var file in files)
                {
                    if (!FileNames.Contains(file))
                        FileNames.Add(file);
                }
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем выделенные файлы из ListBox
            var selectedItems = MyListBox.SelectedItems.Cast<string>().ToList(); // Замените myListBox на имя вашего ListBox

            // Удаляем каждый выделенный элемент из коллекции FileNames
            foreach (var item in selectedItems)
            {
                FileNames.Remove(item);
            }
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            AddLinks.AddRevitLink(_doc, FileNames.ToArray());

            stopwatch.Stop();
            TimeSpan elapsedTime = stopwatch.Elapsed;
            string result = string.Format("{0} мин. {1} сек.",
                elapsedTime.Minutes, elapsedTime.Seconds);
            
            MessageBox.Show(result);
            Close();
        }
    }
}
