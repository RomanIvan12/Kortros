using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Kortros.ParamParser.View;

namespace Kortros.ParamParser.ViewModel.Commands
{
    public class ExportDataCsvCommand : ICommand
    {
        public ParamStackVM VM { get; set; }
        public event EventHandler CanExecuteChanged;

        public ExportDataCsvCommand(ParamStackVM vm)
        {
            VM = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            VM.ExportToCsv(VM.DataItems, DataTableWindow.Instance);
        }
    }
}
