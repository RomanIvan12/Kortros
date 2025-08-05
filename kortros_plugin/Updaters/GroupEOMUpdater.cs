using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using Kortros.Utilities;

namespace Kortros.Updaters
{
    internal class GroupEOMUpdater : IUpdater
    {
        static UpdaterId updaterId;
        public GroupEOMUpdater(AddInId id)
        {
            updaterId = new UpdaterId(id, new Guid("FA38FF1E-7169-4F18-A8BF-31D59BE254E0"));
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
            ElementMulticategoryFilter categoryEOMFilter = new ElementMulticategoryFilter(new List<BuiltInCategory>()
            {
                BuiltInCategory.OST_MechanicalEquipment,
                BuiltInCategory.OST_ElectricalEquipment,
                BuiltInCategory.OST_LightingFixtures,
                BuiltInCategory.OST_CableTray,
                BuiltInCategory.OST_Conduit,
                BuiltInCategory.OST_CableTrayFitting,
                BuiltInCategory.OST_ConduitFitting,
                BuiltInCategory.OST_LightingDevices,
                BuiltInCategory.OST_DataDevices,
                BuiltInCategory.OST_ElectricalFixtures,
                BuiltInCategory.OST_ElectricalCircuit,
                BuiltInCategory.OST_GenericModel
            });

            if (updaterId != null && UpdaterRegistry.IsUpdaterRegistered(updaterId))
            {
                UpdaterRegistry.RemoveAllTriggers(updaterId);

                UpdaterRegistry.AddTrigger(updaterId,
                    categoryEOMFilter,
                    Element.GetChangeTypeElementAddition());
            }
        }

        public void Execute(UpdaterData data)
        {
            Document doc = data.GetDocument();
            Guid KRTRS_Group = new Guid("663814f3-e83a-447f-a1d5-866c8c082409");

            IList<Element> parElems = new FilteredElementCollector(doc).OfClass(typeof(ParameterElement)).ToElements();
            List<string> existedParams = new List<string>();

            foreach (Element parElem in parElems)
            {
                if (parElem is SharedParameterElement sharedParameterElement)
                {
                    existedParams.Add(sharedParameterElement.Name);
                }
            }
            foreach (ElementId addedEl in data.GetAddedElementIds())
            {
                Element element = doc.GetElement(addedEl);

                if (element.GroupId.IntegerValue == -1)
                {
                    try
                    {
                        if (existedParams.Contains("KRTRS_Группирование"))
                        {
                            if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_MechanicalEquipment
                            || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_ElectricalEquipment)
                            {
                                element.get_Parameter(KRTRS_Group).Set("1.Электрооборудование");
                            }
                            else if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_LightingFixtures)
                            {
                                element.get_Parameter(KRTRS_Group).Set("2.Осветительные приборы");
                            }
                            else if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_CableTray
                                || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Conduit
                                || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_CableTrayFitting
                                || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_ConduitFitting)
                            {
                                element.get_Parameter(KRTRS_Group).Set("3.Кабеленесущие системы");
                            }
                            else if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_LightingDevices
                                || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DataDevices
                                || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_ElectricalFixtures)
                            {
                                element.get_Parameter(KRTRS_Group).Set("4.Электроустановочные изделия");
                            }
                            else if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_ElectricalCircuit)
                            {
                                element.get_Parameter(KRTRS_Group).Set("5.Кабельная продукция");
                            }
                            else if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_GenericModel)
                            {
                                element.get_Parameter(KRTRS_Group).Set("6.Прочие");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log.Error($"ElementID {addedEl.IntegerValue} KRTRS_Группирование error: {ex.Message}");
                    }
                }
            }
        }
        public string GetAdditionalInformation()
        {
            return "EOMGroup Updater Additional Information";
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
            return "EOMGroup Updater";
        }
    }
}
