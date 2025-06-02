using System;
using System.Windows.Input;

namespace ApartmentsProject.ViewModel.Commands
{
    public class AddConfig : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public ConfigurationSettingsVm Vm { get; set; }

        public AddConfig(ConfigurationSettingsVm vm)
        {
            Vm = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            Vm.AddConfiguration();
        }
    }
}
