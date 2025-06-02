using System;
using System.Windows.Input;

namespace TestSql.ViewModel.Commands
{
    public class NewCategoryItemCommand : ICommand
    {
        public ParamStackVM VM { get; set; }
        public event EventHandler CanExecuteChanged;

        public NewCategoryItemCommand(ParamStackVM vm)
        {
            VM = vm;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            //TODO: Create new CategoryItem
            VM.CreateCategoryItem();
        }
    }
}
