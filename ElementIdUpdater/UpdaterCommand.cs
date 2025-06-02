using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using System;


namespace ElementIdUpdater
{
    public class UpdaterCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            return Execute(commandData.Application);
        }
        public Result Execute(UIApplication uiapp)
        {
            Application app = uiapp.Application;
            try { uiapp.ViewActivated += new EventHandler<ViewActivatedEventArgs>(ViewActivated); }  catch { }

            return Result.Succeeded;
        }
        public static void ViewActivated(object sender, ViewActivatedEventArgs args)
        {
            Document doc = args.Document;
            AddInId id = args.Document.Application.ActiveAddInId;

            if (!doc.IsFamilyDocument)
            {
                ElementIdUpdater idUpdater = new ElementIdUpdater(id);
            }
        }
    }
}
