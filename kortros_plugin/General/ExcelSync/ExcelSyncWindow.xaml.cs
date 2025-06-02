using System.Windows;

namespace Kortros.General.ExcelSync
{
    /// <summary>
    /// Логика взаимодействия для ExcelSyncWindow.xaml
    /// </summary>
    public partial class ExcelSyncWindow : Window
    {
        private readonly ExcelSyncViewModel viewModel;

        public ExcelSyncWindow(ExcelSyncViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            DataContext = this.viewModel;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
            viewModel.Run();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.GetData();
        }
    }
}
