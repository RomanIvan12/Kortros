using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Application = Autodesk.Revit.ApplicationServices.Application;
using CreateLinks.View;
using CreateLinks.Helpers;
using System.Windows.Forms;

namespace CreateLinks
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SelectProjects : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Logger.Log.Info("--- Команда добавления связей запущена ---");
            Application app = commandData.Application.Application;
            Document doc = commandData.Application.ActiveUIDocument.Document;

            string versionNumber = app.VersionNumber;
            Logger.Log.Info($"Версия revit - {versionNumber}");


            TaskDialog dialog = new TaskDialog("Выберите расположение файлов связей")
            {
                MainInstruction = "Выбрать связи с revit server или с файлового хранилища",
                CommonButtons = TaskDialogCommonButtons.Cancel
            };
            dialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1,
                "Связи с Revit Server");
            dialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2,
                "Связи с файловой системы");

            switch (dialog.Show())
            {
                case TaskDialogResult.CommandLink1:
                    Logger.Log.Info("RS Link");
                    string configFilePath = $@"C:\ProgramData\Autodesk\Revit Server {versionNumber}\Config\RSN.ini";
                    if (!System.IO.File.Exists(configFilePath))
                    {
                        Logger.Log.Error("Конфигурационный файл RSN.ini не найден");
                        MessageBox.Show("Конфигурационный файл RSN.ini не найден");
                        Logger.Log.Info("--EXIT--");
                        return Result.Cancelled;
                    }

                    MainWindow window = new MainWindow(versionNumber, doc);
                    window.ShowDialog();
                    break;
                case TaskDialogResult.CommandLink2:
                    Logger.Log.Info("Folder Link");

                    OpenFileDialog openFileDialog = new OpenFileDialog
                    {
                        InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), // Папка "Мои документы"
                        Filter = "Revit Files (*.rvt)|*.rvt", // Фильтр файлов
                        Title = "Выберите файлы Revit",
                        Multiselect = true // Разрешить выбор нескольких файлов
                    };
                    DialogResult result = openFileDialog.ShowDialog();

                    string[] files;

                    if (result == DialogResult.OK)
                    {
                        files = openFileDialog.FileNames;
                    }
                    else
                        return Result.Cancelled;

                    FolderWindow fw = new FolderWindow(files, doc);
                    fw.ShowDialog();
                    break;

                default: break;
            }

            Logger.Log.Info("--EXIT--");
            return Result.Succeeded;
        }
    }
}
