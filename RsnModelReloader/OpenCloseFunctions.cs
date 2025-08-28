using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using RsnModelReloader.Helpers;
using System;
using System.Collections.Generic;

namespace RsnModelReloader
{
    public class OpenCloseFunctions
    {
        public OpenCloseFunctions(Application app, Dictionary<Model, string> selectedDictionary)
        {
            if (selectedDictionary != null)
            {
                foreach (string singlePath in selectedDictionary.Values)
                {
                    try
                    {
                        Document doc = OpenFile(app, singlePath);
                        CloseOpenedFile(doc);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log.Error($"Model {singlePath} has not been opened. Error" +
                            $"{ex} message. Need to fix");
                    }
                }
            }
        }

        private Document OpenFile(Application app, string path)
        {
            ModelPath modelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(path);
            OpenOptions options = new OpenOptions
            {
                Audit = true,
                IgnoreExtensibleStorageSchemaConflict = true,
                OpenForeignOption = OpenForeignOption.Open
            };

            IList<WorksetPreview> worksets = WorksharingUtils.GetUserWorksetInfo(modelPath);
            List<WorksetId> wsToOpen = new List<WorksetId>();
            WorksetConfiguration worksetConfiguration = new WorksetConfiguration(WorksetConfigurationOption.CloseAllWorksets);

            worksetConfiguration.Open(wsToOpen);
            options.SetOpenWorksetsConfiguration(worksetConfiguration);
            Document doc = app.OpenDocumentFile(modelPath, options);
            if (doc == null)
                Logger.Log.Error($"Model {path} opening error");
            Logger.Log.Info($"Model {doc.Title} has been opened");
            return doc;
        }

        private void CloseOpenedFile(Document doc)
        {
            RelinquishOptions linkOptions = new RelinquishOptions(true)
            {
                StandardWorksets = true,
                ViewWorksets = true,
                FamilyWorksets = true,
                UserWorksets = true,
                CheckedOutElements = true
            };

            SynchronizeWithCentralOptions cyncOptions = new SynchronizeWithCentralOptions()
            {
                Comment = "Переоткрытие документа с очисткой кэша",
                Compact = true,
            };

            cyncOptions.SetRelinquishOptions(linkOptions);


            TransactWithCentralOptions transOptions = new TransactWithCentralOptions();

            try
            {
                doc.SynchronizeWithCentral(transOptions, cyncOptions);
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex.Message);
            }
            Logger.Log.Info($"Document {doc.Title} closed");
            doc.Close();
        }
    }
}
