using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CreateLinks
{
    /// <summary>
    /// НЕ СДЕЛАНО!!!!
    /// </summary>
    public class MainApplication : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            RibbonPanel managePanel = RibbonPanel(application, "Manage");
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

            PushButtonData buttonLinks = new PushButtonData("Links", "Пакетное добав. связей", thisAssemblyPath, "CreateLinks.SelectProjects")
            {
                ToolTip = "Пакетное добавление связей",
                LongDescription = "Пакетное добавление связей",
                LargeImage = IcoImageSource("CreateLinks.Resources.Icons.linkIcon.ico")
            };
            _ = managePanel.AddItem(buttonLinks) as PushButton;

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
