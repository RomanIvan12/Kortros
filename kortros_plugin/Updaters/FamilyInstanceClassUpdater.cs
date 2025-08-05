using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using Kortros.Utilities;

namespace Kortros.Updaters
{
    public class FamilyInstanceClassUpdater : IUpdater
    {
        static UpdaterId updaterId;
        public FamilyInstanceClassUpdater(AddInId id)
        {
            updaterId = new UpdaterId(id, new Guid("14CCD816-232B-4A27-A379-9F629303F011"));
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
            // Фильтр "экземпляры семейств"
            ElementClassFilter logicalPlumbingFixturesFilter = new ElementClassFilter(typeof(FamilyInstance));

            if (updaterId != null && UpdaterRegistry.IsUpdaterRegistered(updaterId))
            {
                UpdaterRegistry.RemoveAllTriggers(updaterId);

                UpdaterRegistry.AddTrigger(updaterId,
                    logicalPlumbingFixturesFilter,
                    Element.GetChangeTypeElementAddition());
            }
        }

        public void Execute(UpdaterData data)
        {
            Document doc = data.GetDocument();
            Guid KRTRS_Izm = new Guid("7621a737-0095-43fe-93fd-ee4a267a6cab");
            ICollection<ElementId> addedCollection = data.GetAddedElementIds(); //новые

            // Изменение рабочего набора в р-н с названием, содержащим название категории
            if (doc.IsWorkshared)
            {
                foreach (ElementId elementId in addedCollection)
                {
                    Element element = doc.GetElement(elementId);

                    if (element.GroupId.IntegerValue == -1)
                    {
                        FamilyInstance elemInst = element as FamilyInstance;

                        ElementId elemSymbol = element.GetTypeId();
                        Element elementType = doc.GetElement(elemSymbol); // Тип

                        // Заполнение KRTRS_Единица измерения
                        try
                        {
                            element.get_Parameter(KRTRS_Izm).Set("шт.");
                        }
                        catch (Exception ex)
                        {
                            Logger.Log.Error($"ElementID {elementId.IntegerValue}: KRTRS_Единица измерения error: {ex.Message}");
                        }

                        if (elemInst.SuperComponent == null)
                        {
                            foreach (Workset workset in new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset))
                            {
                                try
                                {
                                    if (workset.Name.Contains("Технология") && elemInst.Name.IndexOf("паркинг", StringComparison.OrdinalIgnoreCase) >= 0)
                                    {
                                        elemInst.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(workset.Id.IntegerValue);
                                        break;
                                    }
                                    else if (elementType.get_Parameter(BuiltInParameter.UNIFORMAT_CODE).AsString() == "RAR.04.08" && workset.Name.Contains("Отверстия"))
                                    {
                                        elemInst.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(workset.Id.IntegerValue);
                                        break;
                                    }
                                    else if (workset.Name.Contains(elemInst.Category.Name) && elemInst.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PlumbingFixtures)
                                    {
                                        elemInst.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(workset.Id.IntegerValue);
                                        break;
                                    }

                                    else if (workset.Name.Contains("Лотки") && elemInst.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PlumbingFixtures
                                        && elemInst.Name.IndexOf("лоток", StringComparison.OrdinalIgnoreCase) >= 0)
                                    {
                                        elemInst.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(workset.Id.IntegerValue);
                                        break;
                                    }

                                    else if (workset.Name.IndexOf(elemInst.Category.Name, StringComparison.OrdinalIgnoreCase) >= 0 &&
                                        (elemInst.Category.Id.IntegerValue == (int)BuiltInCategory.OST_MechanicalEquipment ||
                                        elemInst.Category.Id.IntegerValue == (int)BuiltInCategory.OST_ElectricalEquipment))
                                    {
                                        elemInst.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(workset.Id.IntegerValue);
                                        break;
                                    }
                                    else if (workset.Name.Contains("Зоны проезда") && elemInst.Category.Id.IntegerValue == (int)BuiltInCategory.OST_GenericModel &&
                                        elemInst.Name.IndexOf("Зона проезда", StringComparison.OrdinalIgnoreCase) >= 0)
                                    {
                                        elemInst.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(workset.Id.IntegerValue);
                                        break;
                                    }
                                    else if (elemInst.Name.Contains("155") && workset.Name == "16.АР_Перемычки")
                                    {
                                        elemInst.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(workset.Id.IntegerValue);
                                        break;
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
            return "Family Instance Class Updater Additional Information";
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
            return "Family Instance Class Updater";
        }
    }
}
