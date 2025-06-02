using RsnModelReloader.Helpers;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Application = Autodesk.Revit.ApplicationServices.Application;
using System.Windows.Forms;
using Autodesk.Revit.UI.Events;

namespace RsnModelReloader
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SelectFilesCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Logger.Log.Info("--- Команда Reloader запущена ---");
            Application app = commandData.Application.Application;
            UIApplication uiApp = commandData.Application;
            uiApp.DialogBoxShowing += HandleDialogBoxShowing;

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
            uiApp.DialogBoxShowing -= HandleDialogBoxShowing;
            return Result.Succeeded;
        }


        private void HandleDialogBoxShowing(object sender, DialogBoxShowingEventArgs args)
        {
            string DialogId = args.DialogId;
            //if (DialogId == "Dialog_Revit_DocWarnDialog")
            //{
            //    args.OverrideResult(0);
            //    Logger.Log.Info("FILE HAS BEEN CANCELLED");
            //}
            // B_INT_AR_RD_R22

            try
            {
                if (DialogId != null)
                {
                    args.OverrideResult(1);
                    Logger.Log.Info($"DialogId - {DialogId}. ");
                    Logger.Log.Warn("FILE HAS BEEN OK");
                }
            }
            catch 
            {
                Logger.Log.Info($"ERROR DialogId - {DialogId}. ");
                Logger.Log.Error("FILE HAS BEEN CANCELLED");
            }
        }
    }
}
