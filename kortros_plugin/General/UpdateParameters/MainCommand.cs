using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Kortros.General.ExcelSync;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kortros.General.UpdateParameters
{
    [DisplayName("ИМЯ ДИСП")]
    [Transaction(TransactionMode.Manual)]
    public class MainCommand : IExternalCommand
    {
        public static Handler Handler { get; set; }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            try
            {
                var handler = new Handler(uiapp);
                Handler = handler;
                var vm = new UpdateParametersViewModel(handler);
                var window = new UpdateParametersWindow(vm);
                window.ShowDialog();
            }
            catch (Exception ex)
            {

            }

            return Result.Succeeded;
        }
    }
}
