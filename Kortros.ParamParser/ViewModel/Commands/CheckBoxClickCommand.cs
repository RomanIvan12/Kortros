using Kortros.ParamParser.Model;
using Kortros.ParamParser.ViewModel.Helpers;
using System;
using System.Windows.Input;

namespace Kortros.ParamParser.ViewModel.Commands
{
    public class CheckBoxClickCommand : ICommand
    {
        public ParamStackVM VM { get; set; }
        public event EventHandler CanExecuteChanged;

        public CheckBoxClickCommand(ParamStackVM vm)
        {
            VM = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            CategoryItem catItem = parameter as CategoryItem;
            DatabaseHelper.Update(catItem);
        }
    }
}
