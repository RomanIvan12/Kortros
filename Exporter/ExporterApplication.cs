using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ExporterFromRs
{
    public class Availability : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
        {
            return true;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ExporterApplication : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            RibbonPanel panelManage = RibbonPanel(application, "Manage");
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
            try
            {
                PushButtonData buttonTest =
                    new PushButtonData("Export", "ModelsExport", thisAssemblyPath, "ExporterFromRs.SelectProjects")
                    {
                        ToolTip = "Экспорт моделей из revit сервера",
                        LongDescription = "Экспорт моделей куда-либо",
                        LargeImage = IcoImageSource("ExporterFromRs.Images.ExportLogo.ico"),
                        AvailabilityClassName = "ExporterFromRs.Availability"
                    };
                panelManage.AddItem(buttonTest);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            // Register DialogBoxShowing event handler, which allow you to 
            // do some work before the dialog shows (close TaskDialog_Missing_Third_Party_Updater)
            application.DialogBoxShowing += DialogShowErrorMessage;

            return Result.Succeeded;
        }

        void DialogShowErrorMessage(object sender, DialogBoxShowingEventArgs args)
        {
            TaskDialogShowingEventArgs args2 = args as TaskDialogShowingEventArgs;

            string DialogId = args2.DialogId;
            if (DialogId == "TaskDialog_Missing_Third_Party_Updater" || DialogId == "TaskDialog_Missing_Third_Party_Updaters")
            {
                args2.OverrideResult((int)TaskDialogResult.CommandLink1);
            }
        }

        private ImageSource IcoImageSource(string embeddedPath)
        {
            Stream stream = this.GetType().Assembly.GetManifestResourceStream(embeddedPath);
            var img = BitmapFrame.Create(stream, BitmapCreateOptions.IgnoreImageCache, BitmapCacheOption.OnLoad);
            return img;
        }

        public RibbonPanel RibbonPanel(UIControlledApplication a, string name)
        {
            RibbonPanel ribbonPanel = null;
            try
            {
                a.CreateRibbonTab("KORTROS");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            try
            {
                a.CreateRibbonPanel("KORTROS", name);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            List<RibbonPanel> panels = a.GetRibbonPanels("KORTROS");
            return panels.First(p => p.Name == name);
        }
    }
}
