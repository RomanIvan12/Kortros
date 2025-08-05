using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using Kortros.Utilities;

namespace Kortros.Updaters
{
    public class EquipmentUpdater : IUpdater
    {
        static UpdaterId updaterId;

        public EquipmentUpdater(AddInId id)
        {
            updaterId = new UpdaterId(id, new Guid("AE23C0FF-A071-49E3-AA97-D1518C7879E9"));
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
            LogicalAndFilter logicalEquipmentFilter = new LogicalAndFilter(
                new ElementClassFilter(typeof(FamilyInstance)),
                new ElementMulticategoryFilter(new List<BuiltInCategory>()
                    {
                        BuiltInCategory.OST_MechanicalEquipment,
                        BuiltInCategory.OST_PlumbingFixtures
                    })
                );

            if (updaterId != null && UpdaterRegistry.IsUpdaterRegistered(updaterId))
            {
                UpdaterRegistry.RemoveAllTriggers(updaterId);

                UpdaterRegistry.AddTrigger(updaterId,
                     logicalEquipmentFilter,
                     Element.GetChangeTypeParameter(new ElementId(BuiltInParameter.RBS_SYSTEM_NAME_PARAM)));
                UpdaterRegistry.AddTrigger(updaterId,
                    logicalEquipmentFilter,
                    Element.GetChangeTypeElementAddition());
                UpdaterRegistry.AddTrigger(updaterId,
                    logicalEquipmentFilter,
                    Element.GetChangeTypeGeometry());
            }
        }

        public void Execute(UpdaterData data)
        {
            Guid KRTRS_Count = new Guid("495d66c9-c5b5-493e-9b0f-73afc67e4216"); //KRTRS_Количество
            Guid KRTRS_Izm = new Guid("7621a737-0095-43fe-93fd-ee4a267a6cab");

            Document doc = data.GetDocument();

            ICollection<ElementId> addedElements = data.GetAddedElementIds();
            ICollection<ElementId> modifiedElements = data.GetModifiedElementIds();

            IEnumerable<ElementId> combinedCollection = addedElements.Concat(modifiedElements);
    
            // Заполнение KRTRS_Количество
            foreach (ElementId addedElement in addedElements)
            {
                Element element = doc.GetElement(addedElement);
                if (element.GroupId.IntegerValue == -1)
                {
                    try
                    {
                        element.get_Parameter(KRTRS_Count).Set(1);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log.Error($"ElementID {addedElement.IntegerValue} KRTRS_Количество error: {ex.Message}");
                    }

                    // Заполнение KRTRS_Единица измерения
                    try
                    {
                        element.get_Parameter(KRTRS_Izm).Set("шт.");
                    }
                    catch (Exception ex)
                    {
                        Logger.Log.Error($"ElementID {addedElement.IntegerValue}: KRTRS_Единица измерения error: {ex.Message}");
                    }
                }
            }
            // Заполнение ИмяСистемы
            foreach (ElementId elementId in combinedCollection)
            {
                Element element = doc.GetElement(elementId);
                if (element.GroupId.IntegerValue == -1)
                {
                    try
                    {
                        string SystemName = element.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM).AsValueString();
                        element.LookupParameter("ИмяСистемы").Set(SystemName);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log.Error($"ElementID {elementId.IntegerValue} ИмяСистемы error: {ex.Message}");
                    }
                }
            }

            // Изменение рабочего набора в р-н с названием, содержащим название категории
            if (doc.IsWorkshared)
            {
                foreach (ElementId elementId in combinedCollection)
                {
                    Element element = doc.GetElement(elementId);
                    if (element.GroupId.IntegerValue == -1)
                    {
                        FamilyInstance elemInst = element as FamilyInstance;

                        if (elemInst.SuperComponent == null)
                        {
                            foreach (Workset workset in new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset))
                            {
                                try
                                {
                                    if (workset.Name.Contains(elemInst.Category.Name))
                                    {
                                        elemInst.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(workset.Id.IntegerValue);
                                    }
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
            return "Equipment Updater Additional Information";
        }
        public ChangePriority GetChangePriority()
        {
            return ChangePriority.FreeStandingComponents;
        }
        public UpdaterId GetUpdaterId()
        {
            return updaterId;
        }
        public string GetUpdaterName()
        {
            return "Equipment Updater";
        }
    }
}
