using System;
using System.Windows.Input;

namespace TestSql.ViewModel.Commands
{
    public class DeleteConfigCommand : ICommand
    {
        public ConfigVM VM { get; set; }
        public event EventHandler CanExecuteChanged;

        public DeleteConfigCommand(ConfigVM vm)
        {
            VM = vm;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            //TODO: Deliting configuration
        }
    }
}
