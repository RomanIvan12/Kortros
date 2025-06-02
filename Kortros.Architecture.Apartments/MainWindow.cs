using System;
using System.IO;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Events;
using Application = Autodesk.Revit.ApplicationServices.Application;
using log4net;
using log4net.Config;
using System.Reflection;
using System.Web.UI.WebControls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Kortros.Architecture.Apartments.Utilities;
using System.Resources;


namespace Kortros.Architecture.Apartments
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class MainWindow : IExternalApplication
    {
        public static string FilePath;
        private static readonly ILog _logger = LogManager.GetLogger("ZoneCalculation");

        public Result OnShutdown(UIControlledApplication application)
        {
            UtilFunctions.CreateLogFolderAndCopy();
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            #region LOG
            
            //Cоздание папки для логера и лога с именем пользователя и сегодняшней датой
            string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Logger");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            string userName = Environment.UserName;
            string dateTime = DateTime.Today.ToString("yyyy_MM_dd");
            string logName = dateTime + " " + userName + " " + "ZoneCalculation";

            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "Logger",
                    $"{logName}.log");

            FilePath = filePath;

            var configStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Kortros.Architecture.Apartments.log4net.config");
            XmlConfigurator.Configure(configStream);
            UtilFunctions.RenamePath(filePath, "ZoneCalculation");

            //application.ControlledApplication.ApplicationInitialized += ApplicationInitialized;
            application.ControlledApplication.DocumentOpened += DocumentOpened;
            
            #endregion

            #region Resources

            ResourceManager resourceManager = new ResourceManager(GetType());
            ResourceManager defaultManager = new ResourceManager(typeof(Properties.Resources));

            #endregion


            RibbonPanel panelAR = RibbonPanel(application, "Архитектура");
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

            try
            {
                PushButtonData btnCMD1 = new PushButtonData("ZoneCalculation", "Расчет зон", thisAssemblyPath, "Kortros.Architecture.Apartments.Commands.RunPlugin")
                {
                    ToolTip = "Перенос скрипта для жанны в кнопку",
                    LongDescription = "Плагин для вставки таблицы на вид или лист. Таблица отображает коэффициенты для этажа по заданным" +
                    "площадям",
                    Image = PngImageSource("Kortros.Architecture.Apartments.Resources.Icons.Table16.ico")
                };

                PushButtonData btnCMD2 = new PushButtonData("ZoneRefresh", "Обновление таблиц", thisAssemblyPath, "Kortros.Architecture.Apartments.Commands.RefreshTables")
                {
                    ToolTip = "Обновление таблиц из скрипта Расчет зон после изменения площадей",
                    Image = PngImageSource("Kortros.Architecture.Apartments.Resources.Icons.Refresh16.ico")
                };

                IList<RibbonItem> stackOne = panelAR.AddStackedItems(btnCMD1, btnCMD2);

            }
            catch (Exception e)
            {
            }
            return Result.Succeeded;
        }

        private ImageSource PngImageSource(string embeddedPath) // Лучше использовать ICO
        {
            Stream stream = GetType().Assembly.GetManifestResourceStream(embeddedPath);
            var zxzz = BitmapFrame.Create(stream, BitmapCreateOptions.IgnoreImageCache, BitmapCacheOption.OnLoad);
            return zxzz;
        }

        public ImageSource GetResourceImage(Assembly assembly, string imageName, int size)
        {
            try
            {
                Stream resourse = assembly.GetManifestResourceStream(imageName);
                if (resourse != null)
                {
                    BitmapFrame sourceFrame = BitmapFrame.Create(resourse);
                    if (sourceFrame != null)
                    {
                        TransformedBitmap transform = new TransformedBitmap(sourceFrame, new ScaleTransform(
                            (double)size / sourceFrame.PixelWidth,
                            (double)size / sourceFrame.PixelHeight));
                        return transform;
                    }
                }
            }
            catch
            {
            }
            return null;
        }

        public RibbonPanel RibbonPanel(UIControlledApplication a, string name)
        {
            RibbonPanel ribbonPanel = null;
            try
            {
                a.CreateRibbonPanel("KORTROS", name);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            List<RibbonPanel> panels = a.GetRibbonPanels("KORTROS");
            foreach (RibbonPanel panel in panels.Where(p => p.Name == name))
            {
                ribbonPanel = panel;
            }
            return ribbonPanel;
        }


        // Блок для добавления команд с событиями, по нажатию кнопки без нажатия кнопки
        private void ApplicationInitialized(object sender, ApplicationInitializedEventArgs e)
        {
        }
        private void DocumentOpened(object sender, DocumentOpenedEventArgs e)
        {
            Document doc = e.Document;
            _logger.Info($"{doc.Title} opened");
        }
    }
}
