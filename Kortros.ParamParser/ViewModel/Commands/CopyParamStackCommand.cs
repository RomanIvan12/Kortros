using Kortros.ParamParser.Model;
using System;
using System.Windows.Input;

namespace Kortros.ParamParser.ViewModel.Commands
{
    public class CopyParamStackCommand : ICommand
    {
        public ParamStackVM VM { get; set; }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public CopyParamStackCommand(ParamStackVM vm)
        {
            VM = vm;
        }

        public bool CanExecute(object parameter)
        {
            ParamStack selectedParamStack = parameter as ParamStack;
            if (selectedParamStack != null)
                return true;

            return false;
        }

        public void Execute(object parameter)
        {
            ParamStack selectedParamStack = parameter as ParamStack;
            VM.CopyParamStack(selectedParamStack);
        }
    }
}
