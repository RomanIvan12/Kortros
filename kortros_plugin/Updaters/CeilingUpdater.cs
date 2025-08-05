using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using Kortros.Utilities;

namespace Kortros.Updaters
{
    public class CeilingUpdater : IUpdater
    {
        static UpdaterId updaterId;
        public CeilingUpdater(AddInId id)
        {
            updaterId = new UpdaterId(id, new Guid("3C434288-E840-42DA-BEBF-26A7DA994EC7"));
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
            // Фильтр категория "Потолки"
            ElementCategoryFilter ceilingFilter = new ElementCategoryFilter(BuiltInCategory.OST_Ceilings);

            if (updaterId != null && UpdaterRegistry.IsUpdaterRegistered(updaterId))
            {
                UpdaterRegistry.RemoveAllTriggers(updaterId);

                UpdaterRegistry.AddTrigger(updaterId,
                    ceilingFilter,
                    Element.GetChangeTypeElementAddition());
                UpdaterRegistry.AddTrigger(updaterId,
                    ceilingFilter,
                    Element.GetChangeTypeGeometry());
            }
        }

        public void Execute(UpdaterData data)
        { 
            Document doc = data.GetDocument();

            IEnumerable<ElementId> combinedCollection = data.GetAddedElementIds().Concat(data.GetModifiedElementIds()); //новые и измененные

            // Изменение рабочего набора в р-н с названием, содержащим название категории
            if (doc.IsWorkshared)
            {
                foreach (ElementId elementId in combinedCollection)
                {
                    if (doc.GetElement(elementId).GroupId.IntegerValue == -1)
                    {
                        Element element = doc.GetElement(elementId);

                        foreach (Workset workset in new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset))
                        {
                            try
                            {
                                if (workset.Name.Contains("тделка"))
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
            return "Ceilings Updater Additional Information";
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
            return "Ceilings Updater";
        }
    }
}

