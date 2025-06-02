using Autodesk.Revit.DB;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using Kortros.Utilities;

namespace Kortros.Updaters
{
    public class WallsUpdater : IUpdater
    {
        static UpdaterId updaterId;

        public WallsUpdater(AddInId id)
        {
            updaterId = new UpdaterId(id, new Guid("A33E70D9-8827-4D2E-9353-199C7F417CC8"));
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
            // Фильтр категория "Стена"
            ElementCategoryFilter wallsFilter = new ElementCategoryFilter(BuiltInCategory.OST_Walls);

            if (updaterId != null && UpdaterRegistry.IsUpdaterRegistered(updaterId))
            {
                UpdaterRegistry.RemoveAllTriggers(updaterId);

                UpdaterRegistry.AddTrigger(updaterId,
                     wallsFilter,
                     Element.GetChangeTypeParameter(new ElementId(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM))); // ПРОВЕРИТЬ, ВОЗМОЖНО УБРАТЬ
                UpdaterRegistry.AddTrigger(updaterId,
                    wallsFilter,
                    Element.GetChangeTypeElementAddition());
                UpdaterRegistry.AddTrigger(updaterId,
                    wallsFilter,
                    Element.GetChangeTypeGeometry());
            }
        }

        public void Execute(UpdaterData data)
        { 
            Document doc = data.GetDocument();

            IEnumerable<ElementId> combinedCollection = data.GetAddedElementIds().Concat(data.GetModifiedElementIds());

            // Изменение рабочего набора
            if (doc.IsWorkshared)
            {
                foreach (ElementId elementId in combinedCollection)
                {
                    Element element = doc.GetElement(elementId);

                    if (element.GroupId.IntegerValue == -1)
                    {
                        Wall wall = element as Wall;

                        ElementId materialId = wall.WallType.get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM).AsElementId();

                        Material material = materialId != null ? doc.GetElement(materialId) as Material : null;
                        string matName = material != null ? doc.GetElement(materialId).Name : null;

                        // Условия, завязанные на материал
                        if (material != null && matName != null)
                        {
                            if ((matName.Contains("идроизоляция") || matName.Contains("еотекстиль") || matName.Contains("мембран") || matName.Contains("руберо") || matName.Contains("битум")) && matName != null)
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
                            else if (matName.IndexOf("Фасад", StringComparison.OrdinalIgnoreCase) >= 0)            
                            {
                                foreach (Workset workset in new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset))
                                {
                                    try
                                    {
                                        if (workset.Name.Contains("11.АР_Фасад"))
                                            element.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(workset.Id.IntegerValue);
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.Log.Error($"ElementID {elementId.IntegerValue} Workset error: {ex.Message}");
                                    }
                                }
                            }
                        }
                        // условия, завязанные на параметре "внешние"
                        else if (wall.WallType.Function == WallFunction.Exterior)
                        {
                            foreach (Workset workset in new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset))
                            {
                                try
                                {
                                    if (workset.Name.Contains("Кладка наружная"))
                                        element.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(workset.Id.IntegerValue);
                                }
                                catch (Exception ex)
                                {
                                    Logger.Log.Error($"ElementID {elementId.IntegerValue} Workset error: {ex.Message}");
                                }
                            }
                        }
                        // условия для кладки
                        else if (wall.WallType.Function == WallFunction.Interior && material.MaterialClass == "Кладка")
                        {
                            foreach (Workset workset in new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset))
                            {
                                try
                                {
                                    if (workset.Name.Contains("06"))
                                        element.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(workset.Id.IntegerValue);
                                }
                                catch (Exception ex)
                                {
                                    Logger.Log.Error($"ElementID {elementId.IntegerValue} Workset error: {ex.Message}");
                                }
                            }
                        }
                        else if (wall.WallType.get_Parameter(BuiltInParameter.ALL_MODEL_DESCRIPTION).AsString().Contains("тделка")
                            || wall.WallType.get_Parameter(BuiltInParameter.ALL_MODEL_MODEL).AsString().Contains("тделка"))
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
            return "Walls Updater Additional Information";
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
            return "Walls Updater";
        }
    }
}
