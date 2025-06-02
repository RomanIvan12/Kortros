using Kortros.General.UpdateParameters.Data;
using System.Windows;

namespace Kortros.General.UpdateParameters
{
    /// <summary>
    /// Логика взаимодействия для UpdateParametersWindow.xaml
    /// </summary>
    public partial class UpdateParametersWindow : Window
    {
        private readonly UpdateParametersViewModel viewModel;
        public UpdateParametersWindow(UpdateParametersViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            DataContext = this.viewModel;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: action
            Close();
            viewModel.Run();


            var dataviewModel = new ShowDataViewModel(MainCommand.Handler.DataItemList);
            var Datawindow = new ShowDataWindow(dataviewModel);
            Datawindow.ShowDialog();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
