using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Kortros.ManageTab
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class MainWindow : IExternalApplication
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
                PushButtonData btnWorksetCreationData = new PushButtonData("WorksetCreation", "Добавить РН", thisAssemblyPath, "Kortros.ManageTab.Commands.WorksetCreation")
                {
                    ToolTip = "Создание рабочих наборов",
                    LongDescription = "В зависимости от имени файла открывается окно выбора рабочих наборов",
                    LargeImage = IcoImageSource("Kortros.ManageTab.WorksetCreation.ico")
                };
                _ = panelManage.AddItem(btnWorksetCreationData) as PushButton;
            }
            catch (Exception e)
            {
            }
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
            string tab = "KORTROS";
            RibbonPanel ribbonPanel = null;
            try
            {
                a.CreateRibbonTab(tab);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            try
            {
                a.CreateRibbonPanel(tab, name);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            List<RibbonPanel> panels = a.GetRibbonPanels(tab);
            foreach (RibbonPanel panel in panels.Where(p => p.Name == name))
            {
                ribbonPanel = panel;
            }
            return ribbonPanel;
        }
    }
}
