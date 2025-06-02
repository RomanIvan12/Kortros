using Kortros.ParamParser.Model;
using System;
using System.Windows.Input;

namespace Kortros.ParamParser.ViewModel.Commands
{
    public class NewParamStackCommand : ICommand
    {
        public ParamStackVM VM { get; set; }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public NewParamStackCommand(ParamStackVM vm)
        {
            VM = vm;
        }

        public bool CanExecute(object parameter)
        {
            CategoryItem selectedCategoryItem = parameter as CategoryItem;
            if (selectedCategoryItem != null)
                return true;

            return false;
        }

        public void Execute(object parameter)
        {
            CategoryItem selectedCategoryItem = parameter as CategoryItem;
            VM.CreateParamStack(selectedCategoryItem.Id);
        }
    }
}
