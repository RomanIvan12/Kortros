using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ExporterFromRs.Helpers;
using ExporterFromRs.View;
using System.Windows;
using Application = Autodesk.Revit.ApplicationServices.Application;

namespace ExporterFromRs
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class SelectProjects : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Logger.Log.Info("--- Команда Exporter запущена ---");
            Application app = commandData.Application.Application;

            string versionNumber = app.VersionNumber;
            Logger.Log.Info($"Версия revit - {versionNumber}");

            string configFilePath = $@"C:\ProgramData\Autodesk\Revit Server {versionNumber}\Config\RSN.ini";
            if (!System.IO.File.Exists(configFilePath))
            {
                Logger.Log.Error("Конфигурационный файл RSN.ini не найден");
                MessageBox.Show("Конфигурационный файл RSN.ini не найден");
                Logger.Log.Info("--EXIT--");
                return Result.Cancelled;
            }

            MenuWindow window = new MenuWindow(app, versionNumber);
            window.ShowDialog();
            Logger.Log.Info("------");

            Logger.CreateLogFolderAndCopy();
            return Result.Succeeded;
        }
    }
}
