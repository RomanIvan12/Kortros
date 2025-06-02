using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementIdUpdater
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class MainApp : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            application.ControlledApplication.ApplicationInitialized += ApplicationInitialized;


            return Result.Succeeded;
        }
        private void ApplicationInitialized(object sender, ApplicationInitializedEventArgs e)
        {
            var command = new UpdaterCommand();
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
