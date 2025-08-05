using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using ExporterFromRs.Helpers;
using ExporterFromRs.View;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
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
            UIApplication uiapp = commandData.Application;

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

            commandData.Application.Application.FailuresProcessing += new EventHandler<FailuresProcessingEventArgs>(OnFailuresProcessing);
            uiapp.DialogBoxShowing += TaskDialogShowingEvent;

            MenuWindow window = new MenuWindow(app, versionNumber);
            window.ShowDialog();
            Logger.Log.Info("------");

            commandData.Application.Application.FailuresProcessing -= new EventHandler<FailuresProcessingEventArgs>(OnFailuresProcessing);
            uiapp.DialogBoxShowing -= TaskDialogShowingEvent;

            Logger.CreateLogFolderAndCopy();
            return Result.Succeeded;
        }
        private void OnFailuresProcessing(object sender, FailuresProcessingEventArgs e)
        {
            FailuresAccessor failuresAccessor = e.GetFailuresAccessor();
            String transactionName = failuresAccessor.GetTransactionName();

            IList<FailureMessageAccessor> fmas = failuresAccessor.GetFailureMessages();
            if (transactionName.Equals("etransmit purge unused"))
            {
                foreach (FailureMessageAccessor fma in fmas)
                {
                    failuresAccessor.DeleteWarning(fma);
                }
                e.SetProcessingResult(FailureProcessingResult.ProceedWithCommit);
                return;
            }
            e.SetProcessingResult(FailureProcessingResult.Continue);
        }

        public void TaskDialogShowingEvent(object sender, DialogBoxShowingEventArgs args)
        {
            if (args is TaskDialogShowingEventArgs taskArgs)
            {
                if (taskArgs.Message.Contains("printsrv"))
                    taskArgs.OverrideResult((int)TaskDialogResult.CommandLink1);
            }
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class TestGetData : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            string path = $"RSN://Srv-revit/TEST/bez_nwc_bez_armat MAIN.rvt";
            string path2 = "C:\\Users\\IvannikovRV\\Desktop\\bez_nwc_bez_armat MAIN.rvt";
            ModelPath modelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(path);

            TransmissionData transData = TransmissionData.ReadTransmissionData(modelPath);
            if (transData != null)
            {
                ICollection<ElementId> externalReferences = transData.GetAllExternalFileReferenceIds();
                foreach (ElementId refId in externalReferences)
                {
                    var extRef = transData.GetLastSavedReferenceData(refId);
                    var type = extRef.ExternalFileReferenceType;
                    if (extRef.ExternalFileReferenceType == ExternalFileReferenceType.RevitLink)
                    {
                        transData.SetDesiredReferenceData(refId, extRef.GetPath(), extRef.PathType, false);
                    }
                }
                transData.IsTransmitted = true;
                TransmissionData.WriteTransmissionData(modelPath, transData);
            }
            return Result.Succeeded;
        }
    }
}
