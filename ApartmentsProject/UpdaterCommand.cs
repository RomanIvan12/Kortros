using Autodesk.Revit.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApartmentsProject.Updaters;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Events;

namespace ApartmentsProject
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class UpdaterCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            return Execute(commandData.Application);
        }

        public Result Execute(UIApplication uiapp)
        {
            Application app = uiapp.Application;
            try
            {
                uiapp.ViewActivated += new EventHandler<ViewActivatedEventArgs>(ViewActivated);
            }
            catch (Exception ex)
            {
            }
            return Result.Succeeded;
        }

        public static void ViewActivated(object sender, ViewActivatedEventArgs args)
        {
            Document doc = args.Document;
            AddInId id = args.Document.Application.ActiveAddInId;

            if (!doc.IsFamilyDocument)
            {
                _ = new RoomUpdater(id);
            }
        }
    }
}
