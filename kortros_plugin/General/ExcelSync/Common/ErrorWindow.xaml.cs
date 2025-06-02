using Autodesk.Revit.UI;
using Kortros.General.MVVM;
using System;
using System.Windows;

namespace Kortros.General.ExcelSync.Common
{
    public partial class ErrorWindow : Window
    {
        private readonly ErrorViewModel viewModel;

        public ErrorWindow(string className, Exception exception, UIApplication uiapp)
        {
            InitializeComponent();
            this.viewModel = new ErrorViewModel
            {
                CommandName = className,
                Exception = exception,
                DocName = uiapp.ActiveUIDocument.Document.Title,
            };
            DataContext = this.viewModel;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class ErrorViewModel : ViewModelBase
    {
        public string CommandName { get; set; }
        public Exception Exception { get; set; }
        public string DocName { get; set; }
        public string Description { get; set; }
    }
}
