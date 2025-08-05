using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using Kortros.Utilities;

namespace Kortros.Updaters
{
    public class FloorsUpdater : IUpdater
    {
        static UpdaterId updaterId;
        public FloorsUpdater(AddInId id)
        {
            updaterId = new UpdaterId(id, new Guid("96A0D5C9-66E0-4A07-89FC-28BB1EF00B20"));
            RegisterUpdater();
            RegisterTriggers();
        }

        public void RegisterUpdater()
        {
            if (UpdaterRegistry.IsUpdaterRegistered(updaterId))
            {
                UpdaterRegistry.UnregisterUpdater(updaterId);
            }
            UpdaterRegistry.RegisterUpdater(this, true);
        }

        public void RegisterTriggers()
        {
            // Фильтр категория "Перекрытия"
            ElementCategoryFilter floorFilter = new ElementCategoryFilter(BuiltInCategory.OST_Floors);

            if (updaterId != null && UpdaterRegistry.IsUpdaterRegistered(updaterId))
            {
                UpdaterRegistry.RemoveAllTriggers(updaterId);

                UpdaterRegistry.AddTrigger(updaterId,
                    floorFilter,
                    Element.GetChangeTypeElementAddition());
                UpdaterRegistry.AddTrigger(updaterId,
                    floorFilter,
                    Element.GetChangeTypeGeometry());
            }
        }

        public void Execute(UpdaterData data)
        { 
            Document doc = data.GetDocument();
            Guid KRTRS_Izm = new Guid("7621a737-0095-43fe-93fd-ee4a267a6cab");

            IEnumerable<ElementId> combinedCollection = data.GetAddedElementIds().Concat(data.GetModifiedElementIds()); //новые и измененные

            // Изменение рабочего набора в р-н с названием, содержащим название категории
            if (doc.IsWorkshared)
            {
                foreach (ElementId elementId in combinedCollection)
                {
                    Element element = doc.GetElement(elementId);

                    if (element.GroupId.IntegerValue == -1)
                    {
                        Floor floor = element as Floor;
                        // Заполнение KRTRS_Единица измерения
                        try
                        {
                            element.get_Parameter(KRTRS_Izm).Set("м\u00B2");
                        }
                        catch (Exception ex)
                        {
                            Logger.Log.Error($"ElementID {elementId.IntegerValue}: KRTRS_Единица измерения error: {ex.Message}");
                        }

                        ElementId materialId = floor.FloorType.get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM).AsElementId();
                        Material material = materialId != null ? doc.GetElement(materialId) as Material : null;
                        string matName = material != null ? doc.GetElement(materialId).Name : null;

                        if (material != null && matName != null)
                        {
                            if (matName.Contains("идроизоляция") || matName.Contains("еотекстиль") || matName.Contains("мембран") || matName.Contains("руберо") || matName.Contains("битум"))
                            {
                                foreach (Workset workset in new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset))
                                {
                                    try
                                    {
                                        if (workset.Name.Contains("Гидроизоляция"))
                                            element.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(workset.Id.IntegerValue);
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.Log.Error($"ElementID {elementId.IntegerValue} Workset error: {ex.Message}");
                                    }
                                }
                            }
                            else if (matName.IndexOf("входны", StringComparison.OrdinalIgnoreCase)>=0)
                            {
                                foreach (Workset workset in new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset))
                                {
                                    try
                                    {
                                        if (workset.Name.Contains("Крыльца"))
                                            element.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(workset.Id.IntegerValue);
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.Log.Error($"ElementID {elementId.IntegerValue} Workset error: {ex.Message}");
                                    }
                                }
                            }
                        }
                        else if (floor.FloorType.get_Parameter(BuiltInParameter.ALL_MODEL_MODEL).AsString().Contains("тделка")
                            || floor.FloorType.get_Parameter(BuiltInParameter.ALL_MODEL_DESCRIPTION).AsString().Contains("тделка"))
                        {
                            foreach (Workset workset in new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset))
                            {
                                try
                                {
                                    if (workset.Name.Contains("Отделка"))
                                        element.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(workset.Id.IntegerValue);
                                }
                                catch (Exception ex)
                                {
                                    Logger.Log.Error($"ElementID {elementId.IntegerValue} Workset error: {ex.Message}");
                                }
                            }
                        }
                    }
                }
            }
        }

        public string GetAdditionalInformation()
        {
            return "Floors Updater Additional Information";
        }
        public ChangePriority GetChangePriority()
        {
            return ChangePriority.FloorsRoofsStructuralWalls;
        }
        public UpdaterId GetUpdaterId()
        {
            return updaterId;
        }
        public string GetUpdaterName()
        {
            return "Floors Updater";
        }
    }
}

