using System;
using System.Collections.Generic;
using System.IO;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using eTransmitForRevitDB;
using ExporterFromRs.Classes;
using ExporterFromRs.Helpers;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using Application = Autodesk.Revit.ApplicationServices.Application;
using File = System.IO.File;
using ExporterFromRs.View;
using System.Linq;

namespace ExporterFromRs
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class DetachFile
    {
        // JSONConverter converter = new JSONConverter("F:\\18. BIM\\BIM_DATA\\05_Ресурсы\\01_Настройки плагинов\\ModelsExporter"); // создать JSON. СЮДА ПУТЬ СОХРАНЕНИЯ ФАЙЛА JSON в скобках
        private const string JsonConfigPath = @"F:\18. BIM\BIM_DATA\05_Ресурсы\01_Настройки плагинов\ModelsExporter";
        private const string JsonConfigPathCommon = @"\\FPS03MSK.corp.rsg.grp\Common$\18. BIM\BIM_DATA\05_Ресурсы\01_Настройки плагинов\ModelsExporter";
        private Application _app;
        private readonly Dictionary<Model, string> _selectedDictionary;
        private readonly string _projectName;
        private readonly string _revitVersion;
        private readonly bool _convertIsChecked;
        private readonly bool _rebarIsChecked;
        private readonly JSONConverter _converter;

        public  DetachFile(Application app, Dictionary<Model, string> selectedDictionary, string projectName, string revitVersion, bool convertIsChecked, bool rebarIsChecked)  //словарь выбранных Модель - путь
        {
            _app = app;
            _selectedDictionary = selectedDictionary;
            _projectName = projectName;
            _revitVersion = revitVersion;
            _convertIsChecked = convertIsChecked;
            _rebarIsChecked = rebarIsChecked;

            _converter = new JSONConverter(JsonConfigPathCommon);

            ProcessFiles();
        }

        private void ProcessFiles()
        {
            if (_selectedDictionary == null) return;

            string jsonExport = _converter.ExpPath;
            string folderPath = _converter.GetJsonValue(jsonExport, _revitVersion, _projectName);

            if (Directory.Exists(folderPath))
                ProcessFilesWithPath(folderPath);
            else
                HandleMissingExportPath(jsonExport);
        }

        private void ProcessFilesWithPath(string folderPath)
        {
            string dateFolder = CreateDateSubFolder(folderPath);
            foreach (string singlePath in _selectedDictionary.Values)
                ProcessSingleFile(singlePath, dateFolder);
        }
        private void ProcessSingleFile(string filePath, string targetFolder)
        {
            Document doc = OpenDetachFile(_app, filePath);
            SaveOpenedFile(doc, targetFolder);

            if (_rebarIsChecked && _convertIsChecked)
                HideCategories(doc);

            if (_convertIsChecked)
                CreateNwcFile(doc, targetFolder);

            CloseOpenedFile(doc, targetFolder);
        }

        private string CreateDateSubFolder(string basePath)
        {
            string formattedDate = DateTime.Now.ToString("yyyy-MM-dd");
            string pathDate = Path.Combine(basePath, formattedDate);
            Directory.CreateDirectory(pathDate);
            return pathDate;
        }

        private void HandleMissingExportPath(string jsonExport)
        {
            var dialog = CreatePathErrorDialog();

            switch (dialog.Show())
            {
                case TaskDialogResult.CommandLink1:
                    HandleJsonUpdateOption(jsonExport);
                    break;
                case TaskDialogResult.CommandLink2:
                    HandleTemporaryFolderOption();
                    break;
                default: break;
            }
        }

        private TaskDialog CreatePathErrorDialog()
        {
            TaskDialog dialog = new TaskDialog("Ошибка пути экспорта файлов")
            {
                MainInstruction = "Путь к папке экспорта указан неверно",
                MainContent = "Файл настройки путей в F:\\18. BIM\\BIM_DATA\\05_Ресурсы\\01_Настройки плагинов\\ModelsExporter не содержит пути для данного проекта." +
                                      "\n Необходимо выгрузить проект с обновлением json или вручную указать путь экспорта",
                CommonButtons = TaskDialogCommonButtons.Cancel
            };
            dialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1,
                "Указать папку для сохранения и обновить JSON");
            dialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2,
                "Указать папку для сохранения для данного сеанса (Без обновления JSON)");
            return dialog;
        }

        private void HandleJsonUpdateOption(string jsonExport)
        {
            var jsonDialog = CreateFolderSelectionDialog("Выберите папку для сохранения");      
            if (jsonDialog.ShowDialog() == true)
            {
                string selectedFolder = Path.GetDirectoryName(jsonDialog.FileName);
                UpdateJsonExportPath(jsonExport, selectedFolder);
                ProcessFilesWithPath(selectedFolder);
            }
        }

        private void HandleTemporaryFolderOption()
        {
            var folderDialog = CreateFolderSelectionDialog("Выберите папку для сохранения");

            if (folderDialog.ShowDialog() == true)
            {
                string selectedFolder = Path.GetDirectoryName(folderDialog.FileName);
                ProcessFilesWithPath(selectedFolder);
            }
        }

        private OpenFileDialog CreateFolderSelectionDialog(string title)
        {
            return new OpenFileDialog()
            {
                Title = title,
                CheckFileExists = false,
                FileName = "Папка"
            };
        }

        private void UpdateJsonExportPath(string jsonExportPath, string newPath)
        {
            string filePath = Path.Combine(jsonExportPath, "pathesToExport.json");
            string json = File.ReadAllText(jsonExportPath);
            JObject jsonObject = JObject.Parse(json);

            jsonObject[_revitVersion][_projectName] = newPath;
            File.WriteAllText(jsonExportPath, jsonObject.ToString());
        }

        private Document OpenDetachFile(Application app, string path)
        {
            ModelPath modelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(path);
            OpenOptions options = new OpenOptions
            {
                DetachFromCentralOption = DetachFromCentralOption.DetachAndPreserveWorksets
            };

            IList<WorksetPreview> worksets = WorksharingUtils.GetUserWorksetInfo(modelPath);
            WorksetConfiguration worksetConfiguration = new WorksetConfiguration(WorksetConfigurationOption.CloseAllWorksets);
            List<WorksetId> wsToOpen = worksets
                .Where(workset => !workset.Name.Contains("Link") &&
                                    !workset.Name.Contains("#") &&
                                    !workset.Name.Contains("Связь"))
                .Select(workset => workset.Id)
                .ToList();
            worksetConfiguration.Open(wsToOpen);

            options.SetOpenWorksetsConfiguration(worksetConfiguration);
            Document doc = app.OpenDocumentFile(modelPath, options);
            if (doc == null)
                Logger.Log.Error($"Model {path} opening error");
            Logger.Log.Info($"Model {doc.Title} has been opened");
            return doc;
        }

        private void CreateNwcFile(Document doc, string folderPath)
        {
            View3D element3dView = FindNavisworksView(doc);
            if (element3dView == null)
            {
                Logger.Log.Error($"В проекте {doc.Title} отсутствует 3д вид с названием Navisworks");
                return;
            }

            NavisworksExportOptions options = new NavisworksExportOptions
            {
                ExportScope = NavisworksExportScope.View,
                ViewId = element3dView.Id,
                ExportLinks = false,
                ExportRoomGeometry = false,
                ExportUrls = false,
                Coordinates = NavisworksCoordinates.Shared
            };
            //string fileName = doc.Title.Substring(0, doc.Title.LastIndexOf('_'));
            //string fileName = doc.Title.Contains('_') ? doc.Title.Substring(0, doc.Title.LastIndexOf('_')) : doc.Title;
            string fileName = doc.Title;
            doc.Export(folderPath,
                fileName,
                options);
            Logger.Log.Info($"{doc.Title} NWC был создан");
        }

        private void SaveOpenedFile(Document doc, string folderPath)
        {
            if (MenuWindow.PurgeElementsEnabled)
                Purge(_app, doc);

            string newName = Path.Combine(folderPath, $"{doc.Title.Substring(0, doc.Title.LastIndexOf('_'))}.rvt");

            SaveAsOptions saveOptions = new SaveAsOptions
            {
                OverwriteExistingFile = true,
                Compact = true,
                MaximumBackups = 1,
            };
            saveOptions.SetWorksharingOptions(new WorksharingSaveAsOptions { SaveAsCentral = true });

            doc.SaveAs(newName, saveOptions);
            Logger.Log.Info($"Model {doc.Title} has been saved. Model path: {newName}");
        }

        private void CloseOpenedFile(Document doc, string folderPath)
        {
            var backupFolderName = doc.Title + "_backup";
            doc.Close();
            // Блок удаления папки бэкапа
            DeleteBackupFolder(folderPath, backupFolderName);
            Logger.Log.Info("Document closed");
        }

        private View3D FindNavisworksView(Document doc)
        {
            return new FilteredElementCollector(doc)
                .OfClass(typeof(View3D))
                .WhereElementIsNotElementType()
                .Cast<View3D>()
                .FirstOrDefault(v => v.Name.Contains("Navisworks") && !v.IsTemplate);
        }

        private void DeleteBackupFolder(string basePath, string folderName)
        {
            try
            {
                string fullPath = Path.Combine(basePath, folderName); // "C:\\Users\\IvannikovRV\\Desktop\\Exporter//2025-02-13" +
                if (Directory.Exists(fullPath))
                {
                    Directory.Delete(fullPath, true);
                    Logger.Log.Info($"Папка {folderName} успешно удалена.");
                }
                else
                    Logger.Log.Info($"Папка {folderName} не найдена.");
            }
            catch (Exception ex)
            {
                Logger.Log.Info($"Ошибка при удалении папки: {ex.Message}");
            }
        }

        private void HideCategories(Document doc)
        {
            View3D view = FindNavisworksView(doc);
            if (view == null)
            {
                Logger.Log.Error($"В проекте {doc.Title} отсутствует 3д вид с названием Navisworks");
                return;
            }

            List<Category> categories = new List<BuiltInCategory>()
            {
                BuiltInCategory.OST_AreaRein,
                BuiltInCategory.OST_PathRein,
                BuiltInCategory.OST_Coupler,
                BuiltInCategory.OST_Rebar
            }.Select(bic => Category.GetCategory(doc, bic)).ToList();

            using (Transaction tr = new Transaction(doc, "Hide Rebar"))
            {
                tr.Start();
                if (view.ViewTemplateId.IntegerValue == -1)
                {
                    OverrideGraphicSettings overrideSettings = new OverrideGraphicSettings();
                    foreach (var category in categories)
                    {
                        // Получение настроек графики для вида
                        view.SetCategoryHidden(category.Id, true);
                    }
                    HideCategoriesInView(view, categories);
                }
                else
                {
                    OverrideGraphicSettings overrideSettings = new OverrideGraphicSettings();
                    View3D template = doc.GetElement(view.ViewTemplateId) as View3D;
                    HideCategoriesInView(template, categories);
                }
                tr.Commit();
            }
        }

        private void HideCategoriesInView(View3D view, IEnumerable<Category> categories)
        {
            foreach (var category in categories)
            {
                try
                {
                    view.SetCategoryHidden(category.Id, true);
                }
                catch (Exception ex)
                {
                    Logger.Log.Error($"Error: {ex.Message}. Category: {category.Name}");
                }
            }
        }

        private bool Purge(Application app, Document doc)
        {
            eTransmitUpgradeOMatic eTransmitUpgradeOMatic = new eTransmitUpgradeOMatic(app);
            UpgradeFailureType result = eTransmitUpgradeOMatic.purgeUnused(doc);
            return (result == UpgradeFailureType.UpgradeSucceeded);
        }
    }
}
