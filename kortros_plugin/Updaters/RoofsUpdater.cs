using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using Kortros.Utilities;

namespace Kortros.Updaters
{
    public class RoofsUpdater : IUpdater
    {
        static UpdaterId updaterId;

        public RoofsUpdater(AddInId id)
        {
            updaterId = new UpdaterId(id, new Guid("9B432793-03B5-49B7-B764-A3E057151483"));
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
            ElementCategoryFilter roofsFilter = new ElementCategoryFilter(BuiltInCategory.OST_Roofs);

            if (updaterId != null && UpdaterRegistry.IsUpdaterRegistered(updaterId))
            {
                UpdaterRegistry.RemoveAllTriggers(updaterId);

                UpdaterRegistry.AddTrigger(updaterId,
                    roofsFilter,
                    Element.GetChangeTypeElementAddition());
                UpdaterRegistry.AddTrigger(updaterId,
                    roofsFilter,
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
                        RoofBase roof = element as RoofBase;

                        foreach (Workset workset in new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset))
                        {
                            try
                            {
                                if (workset.Name.Contains("Кровля"))
                                    element.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(workset.Id.IntegerValue);
                            }
                            catch (Exception ex)
                            {
                                Logger.Log.Error($"ElementID {elementId.IntegerValue} Workset error: {ex.Message}");
                            }
                        }

                        // Заполнение KRTRS_Единица измерения
                        try
                        {
                            element.get_Parameter(KRTRS_Izm).Set("м\u00B2");
                        }
                        catch (Exception ex)
                        {
                            Logger.Log.Error($"ElementID {elementId.IntegerValue}: KRTRS_Единица измерения error: {ex.Message}");
                        }
                    }
                }
            }
        }

        public string GetAdditionalInformation()
        {
            return "Roofs Updater Additional Information";
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
            return "Roofs Updater";
        }
    }
}
