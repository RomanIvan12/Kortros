using Autodesk.Revit.DB;
using log4net;
using System;
using System.Collections.Generic;
using Kortros.Utilities;

namespace Kortros.Updaters
{
    internal class DoorsUpdater : IUpdater
    {
        static UpdaterId updaterId;

        public DoorsUpdater(AddInId id)
        {
            updaterId = new UpdaterId(id, new Guid("EAD2D03D-5B4E-4C7F-8289-0D18551D1D6F"));
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
            // Фильтр типов Duct & FlexDuct
            LogicalAndFilter arFamilyInstances = new LogicalAndFilter(
                new ElementClassFilter(typeof(FamilyInstance)),
                new ElementCategoryFilter(BuiltInCategory.OST_Doors)
                );

            if (updaterId != null && UpdaterRegistry.IsUpdaterRegistered(updaterId))
            {
                UpdaterRegistry.RemoveAllTriggers(updaterId);

                UpdaterRegistry.AddTrigger(updaterId,
                    arFamilyInstances,
                    Element.GetChangeTypeElementAddition());
            }
        }

        public void Execute(UpdaterData data)
        {
            Document doc = data.GetDocument();

            ICollection<ElementId> addedElements = data.GetAddedElementIds();

            foreach (ElementId elementId in addedElements)
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
                                    elemInst.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(workset.Id.IntegerValue);
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
            return "Doors Updater Additional Information";
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
            return "Door updater";
        }
    }
}
