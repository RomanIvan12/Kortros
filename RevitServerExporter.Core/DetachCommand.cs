using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Newtonsoft.Json;

namespace RevitServerExporter.Core
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class DetachCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            return Execute(commandData.Application);
        }
        //public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        public Result Execute(UIApplication uiapp)
        {

            //Dictionary<Model, string> selectedModels = MainWindow.SelectedDictionary;

            //Application app = commandData.Application.Application;
            Application app = uiapp.Application;

            string jsonFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ModelsToExport.json");
            if (!File.Exists(jsonFilePath))
            {
                Console.WriteLine("JSON not found");
                return Result.Cancelled;
            }

            string json = File.ReadAllText(jsonFilePath);
            Dictionary<string, string> selectedDictionaries = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            foreach (string md in selectedDictionaries.Values) // ДОБАВИТЬ JSON с выбором
            {
                Document doc = OpenDetachFile(app, md);
                SaveOpenedFile(doc);
            }

            //catch (Exception ex)
            //{
            //    Console.WriteLine($"Произошла ошибка: {ex.Message}");
            //}

            return Result.Succeeded;
        }

        private Document OpenDetachFile(Application app, string path)
        {
            ModelPath modelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(path);
            OpenOptions options = new OpenOptions
            {
                DetachFromCentralOption = DetachFromCentralOption.DetachAndPreserveWorksets
            };

            IList<WorksetPreview> worksets = WorksharingUtils.GetUserWorksetInfo(modelPath);
            List<WorksetId> wsToOpen = new List<WorksetId>();
            WorksetConfiguration worksetConfiguration = new WorksetConfiguration(WorksetConfigurationOption.CloseAllWorksets);
            foreach (WorksetPreview workset in worksets)
            {
                if (!workset.Name.Contains("Link"))
                {
                    wsToOpen.Add(workset.Id);
                }
            }
            worksetConfiguration.Open(wsToOpen);

            options.SetOpenWorksetsConfiguration(worksetConfiguration);
            Document doc = app.OpenDocumentFile(modelPath, options);
            return doc;
        }

        private void SaveOpenedFile(Document doc)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string combinedPath = Path.Combine(desktopPath, $"{doc.Title}.rvt");

            SaveAsOptions saveOptions = new SaveAsOptions
            {
                OverwriteExistingFile = true,
                Compact = true,
                MaximumBackups = 1,
            };
            saveOptions.SetWorksharingOptions(new WorksharingSaveAsOptions { SaveAsCentral = true });

            doc.SaveAs(combinedPath, saveOptions);

            doc.Close();
        }
    }
}
