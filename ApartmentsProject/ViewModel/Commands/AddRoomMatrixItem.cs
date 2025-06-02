using System;
using System.Windows.Input;

namespace ApartmentsProject.ViewModel.Commands
{
    public class AddRoomMatrixItem : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public SourceDataVm Vm { get; set; }
        public AddRoomMatrixItem(SourceDataVm vm)
        {
            Vm = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            Vm.AddRoomMatrix();
        }
    }
}
