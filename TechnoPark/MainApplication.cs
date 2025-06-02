using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TechnoPark
{
    public class MainApplication : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            RibbonPanel panelManage = RibbonPanel(application, "Технопарк");
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

            try
            {
                PushButtonData btnParamParser = new PushButtonData("DynamoKv", "Dynamo Квартирография", thisAssemblyPath, "TechnoPark.Kvart.MainCommand")
                {
                    ToolTip = "Квартирография (до 2х знаков) Технопарк изм S без k",
                    LargeImage = IcoImageSource("TechnoPark.Kvart.Logo.ico")
                };
                PushButton _ = panelManage.AddItem(btnParamParser) as PushButton;
            }
            catch { }

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
    }
}
