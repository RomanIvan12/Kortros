using System;
using System.Collections.Generic;
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

namespace ApartmentsProject.View
{
    /// <summary>
    /// Логика взаимодействия для AddConfigPopUpWindow.xaml
    /// </summary>
    public partial class AddConfigPopUpWindow : Window
    {
        public string ConfigName { get; private set; }
        public AddConfigPopUpWindow()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            ConfigName = NameTextBox.Text;
            DialogResult = true;
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
