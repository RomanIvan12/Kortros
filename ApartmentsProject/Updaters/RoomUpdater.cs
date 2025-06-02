using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace ApartmentsProject.Updaters
{
    public class RoomUpdater : IUpdater
    {
        private static UpdaterId updaterId;

        public RoomUpdater(AddInId id)
        {
            updaterId = new UpdaterId(id, new Guid("479ACCD8-62D6-41F4-91A5-9167FCC2C1D8"));
            RegisterUpdater();
            RegisterTriggers();
        }

        private void RegisterUpdater()
        {
            if (UpdaterRegistry.IsUpdaterRegistered(updaterId))
            {
                UpdaterRegistry.UnregisterUpdater(updaterId);
            }
            UpdaterRegistry.RegisterUpdater(this, true);
        }
        private void RegisterTriggers()
        {
            ElementCategoryFilter roomFilter = new ElementCategoryFilter(BuiltInCategory.OST_Rooms);
            if (updaterId != null && UpdaterRegistry.IsUpdaterRegistered(updaterId))
            {
                UpdaterRegistry.RemoveAllTriggers(updaterId);
                UpdaterRegistry.AddTrigger(updaterId, roomFilter, Element.GetChangeTypeGeometry());
            }
        }

        public void Execute(UpdaterData data)
        {
            Document doc = data.GetDocument();
            foreach (ElementId modifiedElementId in data.GetModifiedElementIds())
            {
                Element element = doc.GetElement(modifiedElementId);
            }
        }

        public string GetAdditionalInformation()
        {
            return "Room Updater Kvartirografia Information";
        }

        public ChangePriority GetChangePriority()
        {
            return ChangePriority.RoomsSpacesZones;
        }

        public UpdaterId GetUpdaterId()
        {
            return updaterId;
        }

        public string GetUpdaterName()
        {
            return "Room Updater Kvartirografia";
        }
    }
}
