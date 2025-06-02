using System;
using System.Windows.Input;

namespace Kortros.ParamParser.ViewModel.Commands
{
    public class NewConfigCommand : ICommand
    {
        public ParamStackVM VM { get; set; }

        public event EventHandler CanExecuteChanged;
        //{
        //    add { CommandManager.RequerySuggested += value; }
        //    remove { CommandManager.RequerySuggested -= value; }
        //}

        public NewConfigCommand(ParamStackVM vm)
        {
            VM = vm;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            string configName = parameter as string;
            if (!string.IsNullOrWhiteSpace(configName))
            {
                VM.CreateConfig(configName);
            }
            else
            {
                VM.CreateConfig("configName"); // имя по умолчанию
            }

        }
    }
}
