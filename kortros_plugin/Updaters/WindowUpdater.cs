using Autodesk.Revit.DB;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kortros.Updaters
{
    public class WindowUpdater : IUpdater
    {
        static UpdaterId updaterId;
        private static readonly ILog _logger = LogManager.GetLogger("Updater");
        public WindowUpdater(AddInId id, Document doc)
        {
            updaterId = new UpdaterId(id, new Guid("605309A5-7509-4849-816A-1CFA9E47EC8E"));
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
            // Фильтр "экземпляры семейств" И "ОКНА"
            LogicalAndFilter logicalWindowFilter = new LogicalAndFilter(
                new ElementClassFilter(typeof(FamilyInstance)),
                new ElementCategoryFilter(BuiltInCategory.OST_Windows)
                );

            if (updaterId != null && UpdaterRegistry.IsUpdaterRegistered(updaterId))
            {
                UpdaterRegistry.RemoveAllTriggers(updaterId);

                UpdaterRegistry.AddTrigger(updaterId,
                     logicalWindowFilter,
                     Element.GetChangeTypeParameter(new ElementId(BuiltInParameter.UNIFORMAT_CODE)));
                UpdaterRegistry.AddTrigger(updaterId,
                    logicalWindowFilter,
                    Element.GetChangeTypeElementAddition());
                UpdaterRegistry.AddTrigger(updaterId,
                    logicalWindowFilter,
                    Element.GetChangeTypeGeometry());
            }
        }

        public void Execute(UpdaterData data)
        {
            Document doc = data.GetDocument();

            IEnumerable<ElementId> combinedCollection = data.GetAddedElementIds().Concat(data.GetModifiedElementIds()); //новые и измененные

            if (doc.IsWorkshared)
            {
                foreach (ElementId elementId in combinedCollection)
                {
                    Element element = doc.GetElement(elementId);
                    FamilyInstance elemInst = element as FamilyInstance;
                    ElementId elemSymbol = element.GetTypeId();
                    Element elementType = doc.GetElement(elemSymbol);

                    if (elemInst.SuperComponent == null)
                    {
                        foreach (Workset workset in new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset))
                        {
                            try
                            {
                                if (elementType.get_Parameter(BuiltInParameter.UNIFORMAT_CODE).AsString() == "RAR.04.08" && workset.Name.Contains("Отверстия"))
                                {
                                    elemInst.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(workset.Id.IntegerValue);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.Error($"ElementID {elementId.IntegerValue} Workset error: {ex.Message}");
                            }
                        }
                    }
                }
            }
        }

        public string GetAdditionalInformation()
        {
            return "Window Updater Additional Information";
        }

        public ChangePriority GetChangePriority()
        {
            return ChangePriority.DoorsOpeningsWindows;
        }

        public UpdaterId GetUpdaterId()
        {
            return updaterId;
        }

        public string GetUpdaterName()
        {
            return "Window Updater";
        }
    }
}
