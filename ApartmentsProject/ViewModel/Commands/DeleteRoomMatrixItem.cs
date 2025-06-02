using ApartmentsProject.Models;
using System;
using System.Windows.Input;

namespace ApartmentsProject.ViewModel.Commands
{
    public class DeleteRoomMatrixItem : ICommand
    {
        public SourceDataVm Vm { get; set; }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public DeleteRoomMatrixItem(SourceDataVm vm)
        {
            Vm = vm;
        }
        public bool CanExecute(object parameter)
        {
            RoomMatrixEntry selectedRoomMatrixEntry = parameter as RoomMatrixEntry;
            if (selectedRoomMatrixEntry != null)
                return true;
            return false;
        }

        public void Execute(object parameter)
        {
            Vm.DeleteRoomMatrix();
        }
    }
}
