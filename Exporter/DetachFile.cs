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
        private Application _app;
        public  DetachFile(Application app, Dictionary<Model, string> selectedDictionary, string projectName, string revitVersion, bool convertIsChecked, bool rebarIsChecked)  //словарь выбранных Модель - путь
        {
            _app = app;
            JSONConverter converter = new JSONConverter("F:\\18. BIM\\BIM_DATA\\05_Ресурсы\\01_Настройки плагинов\\ModelsExporter"); // создать JSON. СЮДА ПУТЬ СОХРАНЕНИЯ ФАЙЛА JSON в скобках

            string jsonExport = converter.ExpPath;
            string folderPath = converter.GetJsonValue(jsonExport, revitVersion, projectName);

            if (selectedDictionary != null)
            {
                if (Directory.Exists(folderPath))
                {
                    foreach (string singlePath in selectedDictionary.Values)
                    {
                        Document doc = OpenDetachFile(app, singlePath);

                        string formattedDate = DateTime.Now.ToString("yyyy-MM-dd");
                        string pathDate = folderPath + "//" + formattedDate;
                        Directory.CreateDirectory(pathDate);
                        SaveOpenedFile(doc, pathDate);

                        if(rebarIsChecked && convertIsChecked)
                            HideCategories(doc);
                        
                        if (convertIsChecked)
                            CreateNwcFile(doc, pathDate);

                        CloseOpenedFile(doc, pathDate);
                    }
                }
                else
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
                    switch (dialog.Show())
                    {
                        case TaskDialogResult.CommandLink1:

                            OpenFileDialog jsonDialog = new OpenFileDialog()
                            {
                                Title = "Выберите папку для сохранения",
                                CheckFileExists = false,
                                FileName = "Папка, которая перезапишет значение в JSON"
                            };
                            if (jsonDialog.ShowDialog() == true)
                            {
                                string selectedFolder = Path.GetDirectoryName(jsonDialog.FileName);
                                string filePath = Path.Combine(jsonExport, "pathesToExport.json");
                                string json = File.ReadAllText(jsonExport);
                                JObject jsonObject = JObject.Parse(json);
                                // Имя ключа, который изменится
                                jsonObject[revitVersion][projectName] = selectedFolder;
                                File.WriteAllText(jsonExport, jsonObject.ToString());

                                foreach (string singlePath in selectedDictionary.Values)
                                {
                                    Document doc = OpenDetachFile(app, singlePath);

                                    string formattedDate = DateTime.Now.ToString("yyyy-MM-dd");
                                    string pathDate = selectedFolder + "//" + formattedDate;
                                    Directory.CreateDirectory(pathDate);

                                    SaveOpenedFile(doc, pathDate);

                                    if (rebarIsChecked && convertIsChecked)
                                        HideCategories(doc);
                                    if (convertIsChecked)
                                        CreateNwcFile(doc, pathDate);

                                    CloseOpenedFile(doc, pathDate);
                                }
                            }
                            break;
                        case TaskDialogResult.CommandLink2:

                            OpenFileDialog folderDialog = new OpenFileDialog()
                            {
                                Title = "Выберите папку для сохранения",
                                CheckFileExists = false,
                                FileName = "Папка"
                            };
                            if (folderDialog.ShowDialog() == true)
                            {
                                string selectedFolder = Path.GetDirectoryName(folderDialog.FileName);

                                foreach (string singlePath in selectedDictionary.Values)
                                {
                                    Document doc = OpenDetachFile(app, singlePath);

                                    string formattedDate = DateTime.Now.ToString("yyyy-MM-dd");
                                    string pathDate = selectedFolder + "//" + formattedDate;
                                    Directory.CreateDirectory(pathDate);

                                    SaveOpenedFile(doc, pathDate);
                                    if (rebarIsChecked && convertIsChecked)
                                        HideCategories(doc);
                                    if (convertIsChecked)
                                        CreateNwcFile(doc, pathDate);
                                    CloseOpenedFile(doc, pathDate);
                                }
                            }
                            break;
                        default: break;
                    }
                }
            }
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
                if (!workset.Name.Contains("Link") && !workset.Name.Contains("#") && !workset.Name.Contains("Связь"))
                {
                    wsToOpen.Add(workset.Id);
                }
            }
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
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IList<Element> element3dViews = collector.OfClass(typeof(View3D)).WhereElementIsNotElementType().Where(v => v.Name.Contains("Navisworks")).ToList();

            View3D view = null;

            foreach (Element item in element3dViews)
            {
                View3D temp = item as View3D;
                if (temp.IsTemplate == false)
                {
                    view = temp;
                    break;
                }
            }

            if (view != null)
            {
                NavisworksExportOptions options = new NavisworksExportOptions
                {
                    ExportScope = NavisworksExportScope.View,
                    ViewId = view.Id,
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
            }
        }


        private void SaveOpenedFile(Document doc, string folderPath)
        {
            if (MenuWindow.PurgeEl)
                Purge(_app, doc);
            
            string combinedPath = Path.Combine(folderPath, $"{doc.Title}");
            SaveAsOptions saveOptions = new SaveAsOptions
            {
                OverwriteExistingFile = true,
                Compact = true,
                MaximumBackups = 1,
            };
            saveOptions.SetWorksharingOptions(new WorksharingSaveAsOptions { SaveAsCentral = true });

            string newName = combinedPath.Substring(0, combinedPath.LastIndexOf('_')) + ".rvt";

            doc.SaveAs(newName, saveOptions);
            Logger.Log.Info($"Model {doc.Title} has been saved. Model path: {newName}");
        }

        private void CloseOpenedFile(Document doc, string folderPath)
        {
            var aaa = doc.Title;
            var folderName = doc.Title + "_backup";
            doc.Close();

            // Блок удаления папки бэкапа
            try
            {
                string fullPath = Path.Combine(folderPath, folderName); // "C:\\Users\\IvannikovRV\\Desktop\\Exporter//2025-02-13" +
                // A_24_KR_RD_R22_backup
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
            Logger.Log.Info("Document closed");
        }

        private void HideCategories(Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IList<Element> element3dViews = collector.OfClass(typeof(View3D)).WhereElementIsNotElementType().Where(v => v.Name.Contains("Navisworks")).ToList();

            View3D view = null;

            foreach (Element item in element3dViews)
            {
                View3D temp = item as View3D;
                if (temp.IsTemplate == false)
                {
                    view = temp;
                    break;
                }
            }


            List<BuiltInCategory> listOfBc = new List<BuiltInCategory>()
            {
                BuiltInCategory.OST_AreaRein,
                BuiltInCategory.OST_PathRein,
                BuiltInCategory.OST_Coupler,
                BuiltInCategory.OST_Rebar
            };

            List<Category> categories = listOfBc.Select(bic => Category.GetCategory(doc, bic)).ToList();


            ElementId viewTemplateId = view.ViewTemplateId;

            using (Transaction tr = new Transaction(doc, "Hide Rebar"))
            {
                tr.Start();

                if (viewTemplateId.IntegerValue == -1)
                {
                    OverrideGraphicSettings overrideSettings = new OverrideGraphicSettings();
                    foreach (var category in categories)
                    {
                        // Получение настроек графики для вида
                        view.SetCategoryHidden(category.Id, true);
                    }
                }
                else
                {
                    OverrideGraphicSettings overrideSettings = new OverrideGraphicSettings();
                    foreach (var category in categories)
                    {
                        View3D template = doc.GetElement(viewTemplateId) as View3D;
                        template.SetCategoryHidden(category.Id, true);
                    }
                }
                tr.Commit();
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
