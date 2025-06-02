using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace Kortros.General.UpdateParameters.Data
{
    /// <summary>
    /// Логика взаимодействия для ShowDataWindow.xaml
    /// </summary>
    public partial class ShowDataWindow : Window
    {
        private readonly ShowDataViewModel viewModel;

        public ShowDataWindow(ShowDataViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            DataContext = this.viewModel;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class ShowDataViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<DataItem> DataList { get; set; }

        public ShowDataViewModel(ObservableCollection<DataItem> data)
        {
            DataList = data;
        }



        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
