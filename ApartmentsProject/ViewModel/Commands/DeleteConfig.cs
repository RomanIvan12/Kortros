using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Windows.Input;

namespace ApartmentsProject.ViewModel.Commands
{
    public class DeleteConfig : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public ConfigurationSettingsVm Vm { get; set; }

        public DeleteConfig(ConfigurationSettingsVm vm)
        {
            Vm = vm;
        }

        public bool CanExecute(object parameter)
        {
            ObservableCollection<Configuration> configs = parameter as ObservableCollection<Configuration>;
            if (configs != null)
                if (configs.Count == 1 )
                    return false;
            return true;
        }

        public void Execute(object parameter)
        {
            Vm.DeleteConfiguration();
        }
    }
}
