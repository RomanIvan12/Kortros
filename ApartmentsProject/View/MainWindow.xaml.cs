using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ApartmentsProject.Models;
using ApartmentsProject.ViewModel;
using Autodesk.Revit.DB;
using ComboBox = Autodesk.Revit.UI.ComboBox;

namespace ApartmentsProject.View
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ApartmentParameterMappingVm _viewModel;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = PluginSettings.Instance;
            _viewModel = (ApartmentParameterMappingVm)FindResource("vm_mapping");
        }


        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is System.Windows.Controls.ComboBox comboBox)
            {
                if (e.RemovedItems.Count > 0)
                {
                    foreach (var item in e.RemovedItems)
                    {
                        if (item != null)
                            _viewModel.SelectedParameters.Remove(item as Parameter);
                    }
                }

                if (e.AddedItems.Count > 0)
                {
                    foreach (var item in e.AddedItems)
                    {
                        if (item != null)
                            _viewModel.SelectedParameters.Add(item as Parameter);
                    }
                }
            }
        }
        private void ComboBox_DropDownOpened(object sender, EventArgs e)
        {
            if (sender is System.Windows.Controls.ComboBox comboBox)
            {
                UpdateComboBoxState(comboBox);
                comboBox.ItemContainerGenerator.StatusChanged += OnStartedChanged;
            }
        }

        private void UpdateComboBoxState(System.Windows.Controls.ComboBox comboBox)
        {
            foreach (var item in comboBox.Items)
            {
                ComboBoxItem container = comboBox.ItemContainerGenerator.ContainerFromItem(item) as ComboBoxItem;
                if (container != null)
                {
                    if (item == null)
                    {
                        // Пустой элемент всегда оставляем видимым
                        container.IsEnabled = true;
                    }
                    else
                    {
                        // Отключаем элемента, если он уже выбран
                        container.IsEnabled = !_viewModel.SelectedParameters.Contains(item as Parameter);
                    }
                }
            }
        }

        private void OnStartedChanged(object sender, EventArgs e)
        {
            var generator = sender as ItemContainerGenerator;
            if (generator.Status == GeneratorStatus.ContainersGenerated)
            {
                UpdateComboBoxItems(generator);
                generator.StatusChanged -= OnStartedChanged;
            }
        }

        private void UpdateComboBoxItems(ItemContainerGenerator generator)
        {
            foreach (var item in generator.Items)
            {
                ComboBoxItem container = generator.ContainerFromItem(item) as ComboBoxItem;
                if (container != null)
                {
                    if (item == null)
                    {
                        // Пустой элемент всегда оставляем видимым
                        container.IsEnabled = true;
                    }
                    else
                    {
                        // Отключаем элемента, если он уже выбран
                        container.IsEnabled = !_viewModel.SelectedParameters.Contains(item as Parameter);
                    }
                }
            }
        }

        private void ThicknessTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            var typeTag = textBox?.Tag as string;
            e.Handled = !IsTextAllowed(e.Text, typeTag);
        }

        private void ThicknessTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var textBox = sender as TextBox;
            var typeTag = textBox?.Tag as string;

            if (typeTag == "double")
            {
                // Блокируем ввод некоторых символов
                if (e.Key == Key.Space || e.Key == Key.OemMinus ||
                    e.Key == Key.Subtract || e.Key == Key.OemComma)
                {
                    e.Handled = true;
                }
            }
            else
            {
                // Блокируем ввод некоторых символов
                if (e.Key == Key.Space || e.Key == Key.OemMinus ||
                    e.Key == Key.Subtract || e.Key == Key.OemComma ||
                    e.Key == Key.OemPeriod
                    )
                {
                    e.Handled = true;
                }
            }

        }

        private bool IsTextAllowed(string text, string typeTag)
        {
            if (typeTag == "double")
            {
                // Разрешено: числа и точка (запятая если используете CultureInfo с запятой)
                return double.TryParse(text, out _) || text == ".";
            }
            // Only numbers
            return int.TryParse(text, out _);
        }

    }
}
