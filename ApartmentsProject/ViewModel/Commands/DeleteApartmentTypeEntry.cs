using ApartmentsProject.Models;
using System;
using System.Windows.Input;

namespace ApartmentsProject.ViewModel.Commands
{
    public class DeleteApartmentTypeEntry : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public SourceDataVm Vm { get; set; }

        public DeleteApartmentTypeEntry(SourceDataVm vm)
        {
            Vm = vm;
        }
        public bool CanExecute(object parameter)
        {
            ApartmentTypeEntry selectedApartmentTypeEntry = parameter as ApartmentTypeEntry;
            if (selectedApartmentTypeEntry != null)
                return true;
            return false;
        }

        public void Execute(object parameter)
        {
            Vm.DeleteApartmentType();
        }
    }
}
