using System;
using System.IO;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Events;
using Kortros.Utilities;
using Application = Autodesk.Revit.ApplicationServices.Application;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Kortros
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class MainWindow : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            Logger.CreateLogFolderAndCopy();
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            application.ControlledApplication.ApplicationInitialized += ApplicationInitialized;

            RibbonPanel panelMEP = RibbonPanel(application, "MEP");
            RibbonPanel panelGeneral = RibbonPanel(application, "Общие");
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

            try
            {
                PushButtonData btnNameSystemData = new PushButtonData("Set Name System", "KRTRS_Имя системы", thisAssemblyPath, "Kortros.MEP.NameSystem.NameSystemCommand")
                {
                    ToolTip = "Заполнение параметра KRTRS_Имя системы",
                    LargeImage = IcoImageSource("Kortros.Resources.Icons.NameSystem.ico")
                };
                _ = panelMEP.AddItem(btnNameSystemData) as PushButton;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            try
            {
                PushButtonData btnDuctThicknessCommandData = new PushButtonData("ADSK_Толщина стенки", "Толщина стенок", thisAssemblyPath, "Kortros.MEP.DuctThickness.DuctThicknessCommand")
                {
                    ToolTip = "Заполнение параметра ADSK_Толщина стенки по СП 60.13330.2020 Прил К",
                    LargeImage = IcoImageSource("Kortros.Resources.Icons.DuctThickness.ico")
                };
                _ = panelMEP.AddItem(btnDuctThicknessCommandData) as PushButton;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            try
            {
                PushButtonData ExcelSyncBtnData = new PushButtonData("ExcelSync", "ExcelSync", thisAssemblyPath, "Kortros.General.ExcelSync.Command")
                {
                    ToolTip = "Связь значений параметров в Excel и Revit",
                    LongDescription = "Скрипт АРО по связи заполнения параметров в Excel и Revit" +
                    "\nДля успешной работы должны быть выполнены следующие условия оформления Excel" +
                    "\n- Первая строчка содержит названия параметров" +
                    "\n- Вторая строчка пустая" +
                    "\n- Значения параметра в первой колонке являются ключом и должны быть уникальными",
                    LargeImage = IcoImageSource("Kortros.Resources.Icons.ExcelSync.ico")
                };
                _ = panelGeneral.AddItem(ExcelSyncBtnData) as PushButton;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
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

        // Блок для добавления команд с событиями, по нажатию кнопки без нажатия кнопки
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
