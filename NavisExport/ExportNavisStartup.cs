using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using System.Diagnostics;
using Autodesk.Revit.DB.Events;
using Application = Autodesk.Revit.ApplicationServices.Application;
using System.Threading;
using System.Windows;

namespace NavisExport
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ExportStartUp : IExternalApplication
    {
        Result IExternalApplication.OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        Result IExternalApplication.OnStartup(UIControlledApplication application)
        {
            application.ControlledApplication.ApplicationInitialized += ApplicationInitialized;
            return Result.Succeeded;
        }
        private void ApplicationInitialized(object sender, ApplicationInitializedEventArgs e)
        {
            var command = new NavisExport();
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
    public class NavisExport : IExternalCommand
    {
        private EventHandler<DocumentSynchronizedWithCentralEventArgs> _SyncHandler;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            return Execute(commandData.Application);
        }
        public Result Execute(UIApplication uiapp)
        {
            Application app = uiapp.Application;

            // code here
            if (OptionalFunctionalityUtils.IsNavisworksExporterAvailable())
            {
                try
                {
                    //Создали обработчик события
                    _SyncHandler = new EventHandler<DocumentSynchronizedWithCentralEventArgs>(ExportNWC);
                    // подписались на событие
                    app.DocumentSynchronizedWithCentral += _SyncHandler;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Экспортер не установлен");
            }
            return Result.Succeeded;
        }

        public void ExportNWC(object sender, DocumentSynchronizedWithCentralEventArgs eventarg)
        {
            Document doc = eventarg.Document;
            Application app = sender as Application;
            Thread.Sleep(1000);
            string path = string.Empty;
            Parameter parameter = doc.ProjectInformation.LookupParameter("Путь экспорта NWC");
            string title = string.Empty;


            if (doc.IsWorkshared && doc.GetWorksharingCentralModelPath().ServerPath && parameter.AsString().Length > 3)
            {
                path = parameter.AsString();
                title = doc.Title.Replace(app.Username, "").Substring(0, doc.Title.Replace(app.Username, "").Length - 1);
            }
            else
            {
                path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                title = doc.Title.Replace(app.Username, "");
            }
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IList<Element> element3dViews = collector.OfClass(typeof(View3D)).WhereElementIsNotElementType().Where(v => v.Name.Contains("Navisworks")).ToList();

            View3D view = null;

            foreach (Element item in element3dViews)
            {
                View3D temp = item as View3D;
                if (temp.IsTemplate == false)
                {
                    view = temp;
                    break;
                }
            }

            NavisworksExportOptions options = new NavisworksExportOptions
            {
                ExportScope = NavisworksExportScope.View,
                ViewId = view.Id,
                ExportLinks = false,
                ExportRoomGeometry = false,
                ExportUrls = false,
                Coordinates = NavisworksCoordinates.Shared
            };

            doc.Export(path,
                    title,
                    options);
            UIApplication uiapp = sender as UIApplication;
            RevitCommandId commanid = RevitCommandId.LookupPostableCommandId(PostableCommand.RelinquishAllMine);
            uiapp.PostCommand(commanid);
        }
    }
}
