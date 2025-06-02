using System;
using System.Windows.Input;

namespace ApartmentsProject.ViewModel.Commands
{
    public class RenameConfig : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public ConfigurationSettingsVm Vm { get; set; }
        public RenameConfig(ConfigurationSettingsVm vm)
        {
            Vm = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            Vm.RenameConfiguration();            
        }
    }
}
