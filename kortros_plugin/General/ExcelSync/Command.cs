using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.ComponentModel;

namespace Kortros.General.ExcelSync
{
    [DisplayName("Excel Sync")]
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            try
            {
                var handler = new Handler(uiapp);
                var viewModel = new ExcelSyncViewModel(handler);
                var window = new ExcelSyncWindow(viewModel);
                window.ShowDialog();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                new Common.ErrorWindow(this.GetType().Name, e, uiapp).ShowDialog();
            }

            return Result.Succeeded;
        }
    }
}
