using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Application = Autodesk.Revit.ApplicationServices.Application;

namespace RevitServerExporter.Core
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ExternalApplication : IExternalApplication
    {
        // IN LOGGER

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
 
            //application.DialogBoxShowing += DialogBoxShowing;
            application.ControlledApplication.ApplicationInitialized += ApplicationInitialized;

            return Result.Succeeded;
        }

        private void DialogBoxShowing(object sender, DialogBoxShowingEventArgs e)
        {
            if (e.DialogId == "Dialog_Revit_DocWarnDialog")
                e.OverrideResult(2); //Cancel
        }
        public void ApplicationInitialized(object sender, ApplicationInitializedEventArgs e)
        {
            MessageBox.Show("13");
            var command = new DetachCommand();
            if (sender is UIApplication)
            {
                command.Execute(sender as UIApplication);
            }
            else
            {
                command.Execute(new UIApplication(sender as Application));
            }
        }
    }
}
