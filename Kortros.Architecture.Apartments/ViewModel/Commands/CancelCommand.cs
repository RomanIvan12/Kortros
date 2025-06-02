using Kortros.Architecture.Apartments.Commands;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Kortros.Architecture.Apartments.ViewModel.Commands
{
    public class CancelCommand : ICommand
    {
        private static readonly ILog _logger = LogManager.GetLogger("ZoneCalculation");

        public event EventHandler CanExecuteChanged;
        public TableInstanceVM VM { get; set; }

        public CancelCommand(TableInstanceVM vm)
        {
            VM = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            try { _logger.Info("Cancel Command executed"); } catch { _logger.Error("Cancel Command Error"); }
            RunPlugin.TableInstanceWindow.Close();
        }
    }
}
