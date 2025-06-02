using Kortros.ParamParser.Model;
using System;
using System.Windows.Input;

namespace Kortros.ParamParser.ViewModel.Commands
{
    public class ExportCommand : ICommand
    {
        public ParamStackVM VM { get; set; }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public ExportCommand(ParamStackVM vm)
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
            VM.Export();
        }
    }
}
