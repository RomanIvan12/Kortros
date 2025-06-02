using Kortros.ParamParser.Model;
using Kortros.ParamParser.View;
using System;
using System.Windows.Input;

namespace Kortros.ParamParser.ViewModel.Commands
{
    public class NewCategoryItemCommand : ICommand
    {
        public ParamStackVM VM { get; set; }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public NewCategoryItemCommand(ParamStackVM vm)
        {
            VM = vm;
        }
        public bool CanExecute(object parameter)
        {
            Config selectedConfig = parameter as Config;
            if (selectedConfig != null)
                return true;
            return false;
        }

        public void Execute(object parameter)
        {
            Config selectedConfig = parameter as Config;
            SelectCategoryWindow selectCategoryWindow = new SelectCategoryWindow(VM);
            bool? dialogResult = selectCategoryWindow.ShowDialog();
            if (dialogResult == true)
            {
                string selectedCategoryName = selectCategoryWindow.Tag as string;
                VM.CreateCategoryItem(selectedCategoryName, selectedConfig.Id);
            }
        }
    }
}
