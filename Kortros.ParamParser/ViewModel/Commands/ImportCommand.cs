using System;
using System.Windows.Input;

namespace Kortros.ParamParser.ViewModel.Commands
{
    public class ImportCommand : ICommand
    {
        public ParamStackVM VM { get; set; }
        public event EventHandler CanExecuteChanged;

        public ImportCommand(ParamStackVM vm)
        {
            VM = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            VM.Import();
        }
    }
}
