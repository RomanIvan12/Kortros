using Autodesk.Revit.Attributes;
using System.Linq;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Windows;
using DataFromNavisView.Commands;
using DataFromNavisView.Helpers;


namespace DataFromNavisView
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RunCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Logger.Log.Info("Команда выгрузки данных сводной модели запущена");

            Document doc = commandData.Application.ActiveUIDocument.Document;
            
            #region MyRegion
            TaskDialog mainDialog = new TaskDialog("Choose an option")
            {
                MainInstruction = "Выберите настройку экспорта",
                MainContent = " - 1. Выгрузка в локальную базу данных" +
                              "\n - 2. Выгрузка на внешнюю базу postgre Kortros" +
                              "\n - 3. Сохранить в файл базы данных на рабочий стол" +
                              "\n - 4. Сохранить в текстовый файл csv на рабочий стол",
                FooterText = @"F:\18. BIM\BIM_DATA\05_Ресурсы\00_Инструкции\PostrgeSQL.pdf",
                CommonButtons = TaskDialogCommonButtons.Cancel
            };
            mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Export to local postgresql");
            mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Export to postgresql");
            mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink3, "Export to .db3 file_select storage");
            mainDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink4, "Export to .csv file_select storage");

            TaskDialogResult result = mainDialog.Show();

            switch (result)
            {
                case (TaskDialogResult.CommandLink1):
                    Logger.Log.Info("TaskDialogResult.CommandLink1 - загрузка на localhost");

                    var linkInstances = new FilteredElementCollector(doc)
                        .OfClass(typeof(RevitLinkInstance))
                        .Cast<RevitLinkInstance>()
                        .GroupBy(item => item.GetTypeId().IntegerValue)
                        .Select(group => group.First());
                    if (linkInstances.Any())
                    {
                        Logger.Log.Info("Открыто окно настройки");
                        LocalWindow localWindow = new LocalWindow(doc);
                        localWindow.ShowDialog();
                    }
                    else
                    {
                        Logger.Log.Error($"В проекте {doc.Title} нет подгруженных связей");
                        MessageBox.Show("В проекте нет подгруженных связей");
                    }
                    break;
                case (TaskDialogResult.CommandLink2):

                    Logger.Log.Info("TaskDialogResult.CommandLink2 - загрузка на postgresqlBim");

                    var linkInstances2 = new FilteredElementCollector(doc)
                        .OfClass(typeof(RevitLinkInstance))
                        .Cast<RevitLinkInstance>()
                        .GroupBy(item => item.GetTypeId().IntegerValue)
                        .Select(group => group.First());
                    if (linkInstances2.Any())
                    {
                        Logger.Log.Info("Открыто окно настройки");
                        PostgreSqlWindow postgreWindow = new PostgreSqlWindow(doc);
                        postgreWindow.ShowDialog();
                    }
                    else
                    {
                        Logger.Log.Error($"В проекте {doc.Title} нет подгруженных связей");
                        MessageBox.Show("В проекте нет подгруженных связей");
                    }
                    break;

                case (TaskDialogResult.CommandLink3):
                    Logger.Log.Info("TaskDialogResult.CommandLink3 - загрузка в файл export.db3");
                    ExportLocalCommand.ExportDb3(doc);
                    break;
                case (TaskDialogResult.CommandLink4):
                    Logger.Log.Info("TaskDialogResult.CommandLink4 - загрузка в файл export.csv");
                    ExportLocalCommand.ExportCsv(doc);
                    break;
            }
            Logger.CreateLogFolderAndCopy();
            #endregion
            return Result.Succeeded;
        }


    }

}