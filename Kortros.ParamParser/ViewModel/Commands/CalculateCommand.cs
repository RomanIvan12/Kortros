using System;
using System.Windows.Input;
using Kortros.ParamParser.Model;

namespace Kortros.ParamParser.ViewModel.Commands
{
    public class CalculateCommand : ICommand
    {
        public ParamStackVM VM { get; set; }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public CalculateCommand(ParamStackVM vm)
        {
            VM = vm;
        }

        public bool CanExecute(object parameter)
        {
            string selectedPickElements = (string)parameter;
            if (!string.IsNullOrEmpty(selectedPickElements))
                return true;

            return false;
        }


        public void Execute(object parameter)
        {
            VM.Calculate();
        }
    }
}

