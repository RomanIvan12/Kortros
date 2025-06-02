using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

namespace DataFromNavisView
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
            RibbonPanel panelArch = RibbonPanel(application, "Manage");
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
            try
            {
                PushButtonData btnApt = new PushButtonData("DataFromNavisView", "Экспорт данных", thisAssemblyPath, "DataFromNavisView.RunCommand")
                {
                    ToolTip = "Экспорт всех параметров связей",
                    LongDescription = "Выгрузка данных из всех связей в базу данных PostgreSql" +
                                      "\n- Выгрузка выходит ТОЛЬКО с вида Navisworks" +
                                      "\n- Если такой вид отсутствует, выгрузки для этой связи не будет",
                    LargeImage = IcoImageSource("DataFromNavisView.ExportButtonLogo.ico")
                };
                PushButton _ = panelArch.AddItem(btnApt) as PushButton;
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
