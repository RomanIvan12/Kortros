using System;
using System.Windows.Input;
using TestSql.Model;

namespace TestSql.ViewModel.Commands
{
    public class NewParamStackCommand : ICommand
    {
        public ParamStackVM VM { get; set; }
        public event EventHandler CanExecuteChanged;

        public NewParamStackCommand(ParamStackVM vm)
        {
            VM = vm;
        }
        public bool CanExecute(object parameter)
        {
            CategoryItem selectedItem = parameter as CategoryItem;
            if (selectedItem != null)
                return true;

            return false;
        }

        public void Execute(object parameter)
        {
            CategoryItem selectedItem = parameter as CategoryItem;
            //TODO: Create new note
            VM.CreateParamStack(selectedItem.Id);
        }
    }
}
