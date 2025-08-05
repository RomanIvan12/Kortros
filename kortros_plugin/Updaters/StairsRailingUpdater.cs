using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using Kortros.Utilities;

namespace Kortros.Updaters
{
    public class StairsRailingUpdater : IUpdater
    {
        static UpdaterId updaterId;

        public StairsRailingUpdater(AddInId id)
        {
            updaterId = new UpdaterId(id, new Guid("B89AB1A3-82D3-4693-A25D-971208E71AFE"));
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
            // Фильтр категория "Ограждения"
            ElementCategoryFilter railingFilter = new ElementCategoryFilter(BuiltInCategory.OST_StairsRailing);

            if (updaterId != null && UpdaterRegistry.IsUpdaterRegistered(updaterId))
            {
                UpdaterRegistry.RemoveAllTriggers(updaterId);

                UpdaterRegistry.AddTrigger(updaterId,
                    railingFilter,
                    Element.GetChangeTypeElementAddition());
                UpdaterRegistry.AddTrigger(updaterId,
                    railingFilter,
                    Element.GetChangeTypeGeometry());
            }
        }

        public void Execute(UpdaterData data)
        {
            Document doc = data.GetDocument();

            IEnumerable<ElementId> combinedCollection = data.GetAddedElementIds().Concat(data.GetModifiedElementIds());

            // Изменение рабочего набора в р-н с названием, содержащим название категории
            if (doc.IsWorkshared)
            {
                foreach (ElementId elementId in combinedCollection)
                {
                    Element element = doc.GetElement(elementId);

                    if (element.GroupId.IntegerValue == -1)
                    {
                        foreach (Workset workset in new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset))
                        {
                            try
                            {
                                if (workset.Name.Contains("Ограждения"))
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

        public string GetAdditionalInformation()
        {
            return "Stairs Railing Updater Additional Information";
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
            return "Stairs Railing Updater";
        }
    }
}

