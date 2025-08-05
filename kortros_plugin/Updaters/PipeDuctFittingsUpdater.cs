using Autodesk.Revit.DB;
using Kortros.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kortros.Updaters
{
    internal class PipeDuctFittingsUpdater : IUpdater
    {
        static UpdaterId updaterId;
        public PipeDuctFittingsUpdater(AddInId id)
        {
            updaterId = new UpdaterId(id, new Guid("A92AF23F-7F6F-4943-A36C-D6DCEAD35953"));
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
            LogicalAndFilter logicalFittingFilter = new LogicalAndFilter(
                new ElementClassFilter(typeof(FamilyInstance)),
                new ElementMulticategoryFilter(new List<BuiltInCategory>()
                    {
                        BuiltInCategory.OST_PipeFitting,
                        BuiltInCategory.OST_DuctFitting
                    })
                );

            if (updaterId != null && UpdaterRegistry.IsUpdaterRegistered(updaterId))
            {
                UpdaterRegistry.RemoveAllTriggers(updaterId);

                UpdaterRegistry.AddTrigger(updaterId,
                    logicalFittingFilter,
                    Element.GetChangeTypeParameter(new ElementId(BuiltInParameter.RBS_SYSTEM_NAME_PARAM)));
                UpdaterRegistry.AddTrigger(updaterId,
                    logicalFittingFilter,
                    Element.GetChangeTypeElementAddition());
                UpdaterRegistry.AddTrigger(updaterId,
                    logicalFittingFilter,
                    Element.GetChangeTypeGeometry());
            }
        }

        public void Execute(UpdaterData data)
        {
            Document doc = data.GetDocument();

            // Заполнение RBTT_Количество
            Guid KRTRS_Count = new Guid("495d66c9-c5b5-493e-9b0f-73afc67e4216"); //KRTRS_Количество
            Guid ADSK_area = new Guid("b6a46386-70e9-4b1f-9fdb-8e1e3f18a673"); //adsk_размер_площадь
            Guid KRTRS_Izm = new Guid("7621a737-0095-43fe-93fd-ee4a267a6cab");

            ForgeTypeId displayUnitsArea = doc.GetUnits().GetFormatOptions(SpecTypeId.Area).GetUnitTypeId();

            IEnumerable<ElementId> combinedCollection = data.GetAddedElementIds().Concat(data.GetModifiedElementIds());

            foreach (ElementId elementId in combinedCollection)
            {
                Element element = doc.GetElement(elementId);

                if (element.GroupId.IntegerValue == -1)
                {
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

                    // Заполнение рабочих наборов
                    if (doc.IsWorkshared)
                    {
                        if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting)
                        {
                            try
                            {
                                UtilFunctions.CommentBasedPipeWorkset(doc, element);
                            }
                            catch (Exception ex)
                            {
                                Logger.Log.Error($"ElementID {elementId.IntegerValue} Workset error: {ex.Message}");
                            }
                        }
                        else if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DuctFitting)
                        {
                            try
                            {
                                UtilFunctions.CommentBasedDuctWorkset(doc, element);
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
        public string GetAdditionalInformation()
        {
            return "Pipe-Duct Fittings Updater Additional Information";
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
            return "Pipe-Duct Fittings Updater";
        }
    }
}
