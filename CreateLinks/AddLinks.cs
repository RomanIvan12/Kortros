using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CreateLinks.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows;

namespace CreateLinks
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class AddLinks
    {
        public AddLinks(Document doc, Dictionary<Model, string> selectedDictionary)
        {
            if (selectedDictionary != null)
            {
                AddRevitLinkTypes(doc, selectedDictionary.Values.ToList());     
            }
        }

        public void AddRevitLinkTypes(Document doc, List<string> modelPathes)
        {
            //Получить раздел документа (АР или КР или тп)
            string userName = doc.Application.Username;
            string docTitle = doc.Title.Replace("_" + userName, "").TrimEnd();

            //string chapter = docTitle.Substring(docTitle.LastIndexOf('_'));

            //блок добавления РН
            try
            {
                //SetWorksetNamesFromFileToStack(doc, Properties.Resources.WorksetToImport_LINK, chapter);
                SetWorksetNamesFromFileToStack(doc, Properties.Resources.WorksetToImport_LINK);
            }
            catch { }

            using (Transaction t = new Transaction(doc, "Add RevitTypes"))
            {
                t.Start();
                foreach (string singlePath in modelPathes)
                {
                    WorksetConfiguration worksetConfiguration = new WorksetConfiguration(WorksetConfigurationOption.CloseAllWorksets);

                    //Получить раб наборы вгружаемого файла
                    IList<WorksetPreview> wsPreviews = WorksharingUtils.GetUserWorksetInfo(ModelPathUtils.ConvertUserVisiblePathToModelPath(singlePath));

                    IList<WorksetId> idToOpen = new List<WorksetId>();
                    foreach (var singlePreview in wsPreviews)
                    {
                        if (!singlePreview.Name.Contains("Общие уровни"))
                        {
                            idToOpen.Add(singlePreview.Id);
                        }
                    }
                    worksetConfiguration.Open(idToOpen);

                    RevitLinkOptions revitLinkOptions = new RevitLinkOptions(false, worksetConfiguration);

                    LinkLoadResult loadResult = RevitLinkType.Create(doc,
                        ModelPathUtils.ConvertUserVisiblePathToModelPath(singlePath),
                        revitLinkOptions);

                    RevitLinkInstance revitLinkInstance;
                    try
                    {
                        revitLinkInstance = RevitLinkInstance.Create(doc,
                            loadResult.ElementId,
                            ImportPlacement.Shared);

                    }
                    catch (Autodesk.Revit.Exceptions.InvalidOperationException ex)
                    {
                        revitLinkInstance = RevitLinkInstance.Create(doc,
                            loadResult.ElementId,
                            ImportPlacement.Origin);
                        Logger.Log.Error($"Связь {singlePath} загружена по базовой точке из-за ");
                    }

                    var revitName = revitLinkInstance.get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM)
                        .AsValueString();
                    foreach (var workset in new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset)
                                 .ToWorksets())
                    {
                        if (revitName.Contains("_AR") && workset.Name.Contains("_AR"))
                        {
                            revitLinkInstance.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM)
                                .Set(workset.Id.IntegerValue);
                            doc.GetElement(revitLinkInstance.GetTypeId())
                                .get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(workset.Id.IntegerValue);
                        }

                        if (revitName.Contains("_KR") && workset.Name.Contains("_KR"))
                        {
                            revitLinkInstance.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM)
                                .Set(workset.Id.IntegerValue);
                            doc.GetElement(revitLinkInstance.GetTypeId())
                                .get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(workset.Id.IntegerValue);
                        }

                        if (revitName.Contains("_OV") && workset.Name.Contains("_OV"))
                        {
                            revitLinkInstance.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM)
                                .Set(workset.Id.IntegerValue);
                            doc.GetElement(revitLinkInstance.GetTypeId())
                                .get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(workset.Id.IntegerValue);
                        }

                        if (revitName.Contains("_VK") && workset.Name.Contains("_VK"))
                        {
                            revitLinkInstance.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM)
                                .Set(workset.Id.IntegerValue);
                            doc.GetElement(revitLinkInstance.GetTypeId())
                                .get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(workset.Id.IntegerValue);
                        }

                        if (revitName.Contains("_EOM") && workset.Name.Contains("_EOM"))
                        {
                            revitLinkInstance.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM)
                                .Set(workset.Id.IntegerValue);
                            doc.GetElement(revitLinkInstance.GetTypeId())
                                .get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(workset.Id.IntegerValue);
                        }

                        if (revitName.Contains("_SS") && workset.Name.Contains("_SS"))
                        {
                            revitLinkInstance.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM)
                                .Set(workset.Id.IntegerValue);
                            doc.GetElement(revitLinkInstance.GetTypeId())
                                .get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(workset.Id.IntegerValue);
                        }
                    }
                }
                t.Commit();
            }
        }

        //public void SetWorksetNamesFromFileToStack(Document doc, string myFile, string chapter)
        public void SetWorksetNamesFromFileToStack(Document doc, string myFile)
        {
            string[] lines = myFile.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            ICollection<Workset> existWorksets = new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset).ToWorksets();

            using (Transaction t = new Transaction(doc, "Создание рабочих наборов"))
            {
                t.Start();
                foreach (var name in lines)
                {
                    if (!existWorksets.Select(ws => ws.Name).ToList().Contains(name))
                    {
                        Workset.Create(doc, name);
                    }
                }
                t.Commit();
            }
        }


        public static void AddRevitLink(Document doc, string[] modelPathes)
        {
            using (Transaction t = new Transaction(doc, "Add RevitTypes"))
            {
                t.Start();
                foreach (string singlePath in modelPathes)
                {
                    RevitLinkOptions revitLinkOptions;
                    try
                    {
                        Logger.Log.Info($"Cвязь по пути: {singlePath} - является файлом хранилища");
                        WorksetConfiguration worksetConfiguration = new WorksetConfiguration(WorksetConfigurationOption.CloseAllWorksets);
                        //Получить раб наборы вгружаемого файла
                        IList<WorksetPreview> wsPreviews = WorksharingUtils.GetUserWorksetInfo(ModelPathUtils.ConvertUserVisiblePathToModelPath(singlePath));

                        IList<WorksetId> idToOpen = new List<WorksetId>();
                        foreach (var singlePreview in wsPreviews)
                        {
                            if (!singlePreview.Name.Contains("Общие уровни"))
                            {
                                idToOpen.Add(singlePreview.Id);
                            }
                        }
                        worksetConfiguration.Open(idToOpen);
                        revitLinkOptions = new RevitLinkOptions(false, worksetConfiguration);

                    }
                    catch
                    {
                        Logger.Log.Info($"Cвязь по пути: {singlePath} - НЕ является файлом хранилища");
                        revitLinkOptions = new RevitLinkOptions(false);
                    }


                    LinkLoadResult loadResult = RevitLinkType.Create(doc,
                        ModelPathUtils.ConvertUserVisiblePathToModelPath(singlePath),
                        revitLinkOptions);

                    RevitLinkInstance revitLinkInstance;
                    try
                    {
                        revitLinkInstance = RevitLinkInstance.Create(doc,
                            loadResult.ElementId,
                            ImportPlacement.Shared);
                        Logger.Log.Error($"Связь {singlePath} загружена по общим координатам");

                    }
                    catch (Autodesk.Revit.Exceptions.InvalidOperationException ex)
                    {
                        revitLinkInstance = RevitLinkInstance.Create(doc,
                            loadResult.ElementId,
                            ImportPlacement.Origin);
                        Logger.Log.Error($"Связь {singlePath} загружена по базовой точке");
                    }
                }
                t.Commit();
            }
        }



    }
}
