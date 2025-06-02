using Autodesk.Revit.DB;
using Kortros.Utilities;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kortros.Updaters
{
    public class DuctPipeAccessoryUpdater : IUpdater
    {
        static UpdaterId updaterId;

        public DuctPipeAccessoryUpdater(AddInId id)
        {
            //_logger.Info($"Duct-Pipe Accessory Updater started: {doc.ActiveView.Name}");
            updaterId = new UpdaterId(id, new Guid("1FD85C92-A351-4796-A67A-5F0488A261BB"));
            RegisterUpdater();
            RegisterTriggers();
        }

        public void RegisterUpdater()
        {
            if (UpdaterRegistry.IsUpdaterRegistered(updaterId))
            {
                UpdaterRegistry.IsUpdaterRegistered(updaterId);
                UpdaterRegistry.UnregisterUpdater(updaterId);
            }

            UpdaterRegistry.RegisterUpdater(this, true);
        }

        public void RegisterTriggers()
        {
            // Фильтр типов Duct & FlexDuct
            ElementMulticategoryFilter accessoryCategoryFilter = new ElementMulticategoryFilter(new List<BuiltInCategory>()
            {
                BuiltInCategory.OST_PipeAccessory,
                BuiltInCategory.OST_DuctAccessory
            });

            if (updaterId != null && UpdaterRegistry.IsUpdaterRegistered(updaterId))
            {
                UpdaterRegistry.RemoveAllTriggers(updaterId);

                UpdaterRegistry.AddTrigger(updaterId,
                    accessoryCategoryFilter,
                    Element.GetChangeTypeParameter(new ElementId(BuiltInParameter.RBS_SYSTEM_NAME_PARAM)));
                UpdaterRegistry.AddTrigger(updaterId,
                    accessoryCategoryFilter,
                    Element.GetChangeTypeElementAddition());
                UpdaterRegistry.AddTrigger(updaterId,
                    accessoryCategoryFilter,
                    Element.GetChangeTypeGeometry());
            }
        }

        public void Execute(UpdaterData data)
        {
            Document doc = data.GetDocument();
            Guid KRTRS_Count = new Guid("495d66c9-c5b5-493e-9b0f-73afc67e4216");
            Guid KRTRS_Izm = new Guid("7621a737-0095-43fe-93fd-ee4a267a6cab");
            ForgeTypeId displayUnits = doc.GetUnits().GetFormatOptions(SpecTypeId.Length).GetUnitTypeId();

            IEnumerable<ElementId> combinedCollection = data.GetAddedElementIds().Concat(data.GetModifiedElementIds());

            foreach (ElementId elementId in combinedCollection)
            {
                Element element = doc.GetElement(elementId);

                if (doc.IsWorkshared)
                {
                    if (element.GroupId.IntegerValue == -1)
                    {
                        // Обновление Рабочего Набора OV
                        try
                        {
                            if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DuctAccessory && doc.Title.Contains("_OV"))
                            {
                                Element typeEl = doc.GetElement(element.GetTypeId());
                                string naimen = typeEl.get_Parameter(new Guid("e6e0f5cd-3e26-485b-9342-23882b20eb43")).AsString();
                                if (naimen.Contains("НЗ") || naimen.Contains("НО"))
                                {
                                    List<int> id_ozk = new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset).ToWorksets()
                                        .Where(workset => workset.Name == "09.ОЗК").Select(workset => workset.Id.IntegerValue)
                                        .ToList();
                                    element.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(id_ozk[0]);
                                }
                                else
                                    UtilFunctions.CommentBasedDuctWorkset(doc, element);
                            }
                            else if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeAccessory && doc.Title.Contains("_OV"))
                                UtilFunctions.CommentBasedPipeWorkset(doc, element);
                        }
                        catch (Exception ex)
                        {
                            Logger.Log.Error($"ElementID {elementId.IntegerValue} Workset error: {ex.Message}");
                        }

                        // Обновление Рабочего Набора VK
                        try
                        {
                            if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeAccessory && doc.Title.Contains("_VK"))
                                UtilFunctions.CommentBasedPipeWorkset(doc, element);
                        }
                        catch (Exception ex)
                        {
                            Logger.Log.Error($"ElementID {elementId.IntegerValue} Workset error: {ex.Message}");
                        }
                    }
                }

                // Заполнение KRTRS_Количество
                try
                {
                    element.get_Parameter(KRTRS_Count).Set(1);
                }
                catch (Exception ex)
                {
                    Logger.Log.Error($"ElementID {elementId.IntegerValue} KRTRS_Количество error: {ex.Message}");
                }

                // Заполнение ИмяСистемы
                try
                {
                    string SystemName = element.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM).AsValueString();
                    element.LookupParameter("ИмяСистемы").Set(SystemName);
                }
                catch (Exception ex)
                {
                    Logger.Log.Error($"ElementID {elementId.IntegerValue} ИмяСистемы error: {ex.Message}");
                }

                // Заполнение KRTRS_Единица измерения
                try
                {
                    element.get_Parameter(KRTRS_Izm).Set("шт.");
                }
                catch (Exception ex)
                {
                    Logger.Log.Error($"ElementID {elementId.IntegerValue}: KRTRS_Единица измерения error: {ex.Message}");
                }
            }
        }

        public string GetAdditionalInformation()
        {
            return "Duct-Pipe Accessory Updater Additional Information";
        }
        public ChangePriority GetChangePriority()
        {
            return ChangePriority.MEPAccessoriesFittingsSegmentsWires;
        }
        public UpdaterId GetUpdaterId()
        {
            return updaterId;
        }
        public string GetUpdaterName()
        {
            return "Duct-Pipe Accessory Updater";
        }
    }
}
