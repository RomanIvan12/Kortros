using System;
using System.Windows.Input;

namespace TestSql.ViewModel.Commands
{
    public class CreateConfigCommand : ICommand
    {
        public ConfigVM VM { get; set; }
        public event EventHandler CanExecuteChanged;

        public CreateConfigCommand(ConfigVM vm)
        {
            VM = vm;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            //TODO: Creating functionality
        }
    }
}
