using Autodesk.Revit.DB;
using Kortros.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kortros.Updaters
{
    public class SprinklerDuctTerminalUpdater : IUpdater
    {
        static UpdaterId updaterId;

        public SprinklerDuctTerminalUpdater(AddInId id)
        {
            updaterId = new UpdaterId(id, new Guid("57D7F9BD-EF49-4813-B9E0-E1BF701697D6"));
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
            // Фильтр "экземпляры семейств" И "мультикатегории"
            LogicalAndFilter logicalSprDuctFilter = new LogicalAndFilter(
                new ElementClassFilter(typeof(FamilyInstance)),
                new ElementMulticategoryFilter(new List<BuiltInCategory>()
                    {
                        BuiltInCategory.OST_DuctTerminal,
                        BuiltInCategory.OST_Sprinklers
                    })
                );

            if (updaterId != null && UpdaterRegistry.IsUpdaterRegistered(updaterId))
            {
                UpdaterRegistry.RemoveAllTriggers(updaterId);

                UpdaterRegistry.AddTrigger(updaterId,
                    logicalSprDuctFilter,
                    Element.GetChangeTypeAny());
            }
        }

        public void Execute(UpdaterData data)
        {
            Document doc = data.GetDocument();
            Guid KRTRS_Count = new Guid("495d66c9-c5b5-493e-9b0f-73afc67e4216");
            Guid KRTRS_Izm = new Guid("7621a737-0095-43fe-93fd-ee4a267a6cab");

            IEnumerable<ElementId> combinedCollection = data.GetAddedElementIds().Concat(data.GetModifiedElementIds());

            foreach (ElementId elementId in combinedCollection)
            {
                Element element = doc.GetElement(elementId);

                if (element.GroupId.IntegerValue == -1)
                {
                    if (doc.IsWorkshared)
                    {
                        // Обновление Рабочего Набора OV
                        try
                        {
                            Parameter TypeName = element.get_Parameter(BuiltInParameter.RBS_DUCT_SYSTEM_TYPE_PARAM);

                            int wsId = new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset).ToWorksets()
                                .FirstOrDefault(x => x.Name.Contains("Оборудование")).Id.IntegerValue;

                            if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DuctTerminal)
                            {
                                if (TypeName.AsValueString() == "Не определено" || TypeName.AsValueString() == "Undefined")
                                    element.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(wsId);
                                else
                                    UtilFunctions.CommentBasedDuctWorkset(doc, element);
                            }
                            else if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Sprinklers)
                            {
                                if (TypeName.AsValueString() == "Не определено" || TypeName.AsValueString() == "Undefined")
                                    element.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(wsId);
                                else
                                    UtilFunctions.CommentBasedPipeWorkset(doc, element);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Log.Error($"ElementID {elementId.IntegerValue} Workset OV error: {ex.Message}");
                        }

                        // Обновление Рабочего Набора VK
                        try
                        {
                            Parameter TypeName = element.get_Parameter(BuiltInParameter.RBS_DUCT_SYSTEM_TYPE_PARAM);
                            if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Sprinklers &&
                                TypeName != null)
                            {
                                UtilFunctions.CommentBasedPipeWorkset(doc, element);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Log.Error($"ElementID {elementId.IntegerValue} Workset VK error: {ex.Message}");
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
        }

        public string GetAdditionalInformation()
        {
            return "Sprinkler & DuctTerminal Updater Additional Information";
        }
        public ChangePriority GetChangePriority()
        {
            return ChangePriority.MEPFixtures;
        }
        public UpdaterId GetUpdaterId()
        {
            return updaterId;
        }
        public string GetUpdaterName()
        {
            return "Sprinkler & DuctTerminal Updater";
        }
    }
}
