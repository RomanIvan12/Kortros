using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ApartmentsProject.View;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;

namespace ApartmentsProject
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class MainApplication : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
        public Result OnStartup(UIControlledApplication application)
        {
            RibbonPanel panelArch = RibbonPanel(application, "Архитектура");
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
            try
            {
                PushButtonData btnApt = new PushButtonData("ApartmentsProject", "Квартирография", thisAssemblyPath, "ApartmentsProject.RunCommand")
                {
                    ToolTip = "Квартирография",
                    LargeImage = IcoImageSource("ApartmentsProject.Icons.ApartmentsIcon.ico")
                };
                PushButton _ = panelArch.AddItem(btnApt) as PushButton;
            }
            catch { }

            application.ControlledApplication.ApplicationInitialized += ApplicationInitialized;

            return Result.Succeeded;
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
            catch { }

            try
            {
                a.CreateRibbonPanel("KORTROS", name);
            }
            catch { }

            List<RibbonPanel> panels = a.GetRibbonPanels("KORTROS");
            foreach (RibbonPanel panel in panels.Where(p => p.Name == name))
            {
                ribbonPanel = panel;
            }
            return ribbonPanel;
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
