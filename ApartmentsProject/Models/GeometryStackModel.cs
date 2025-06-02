using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace ApartmentsProject.Models
{
    public class GeometryStackModel
    {
        public Room FirstRoom { get; set; }
        public Solid FirstSolid { get; set; }
        public Room LastRoom { get; set; }
        public Solid LastSolid { get; set; }
        public object Separator { get; set; }

        private Document _doc;


        public GeometryStackModel(Document doc, Room firstRoom, Room lastRoom, object separator)
        {
            _doc = doc;
            FirstRoom = firstRoom;
            LastRoom = lastRoom;
            Separator = separator ?? throw new ArgumentNullException(nameof(separator));
        }

        public GeometryStackModel(Document doc, Room room, object separator)
        {
            _doc = doc;
            FirstRoom = room;
            LastRoom = null;
            Separator = separator ?? throw new ArgumentNullException(nameof(separator));
        }

        public override bool Equals(object obj)
        {
            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }
            GeometryStackModel other = (GeometryStackModel)obj;
            bool roomsMath =
                (FirstRoom == other.FirstRoom && LastRoom == other.LastRoom) ||
                (FirstRoom == other.LastRoom && LastRoom == other.FirstRoom) ||
                (FirstRoom != null && FirstRoom == other.FirstRoom && LastRoom == null && other.LastRoom == null) ||
                (LastRoom != null && LastRoom == other.FirstRoom && FirstRoom == null && other.FirstRoom == null);
            return roomsMath;
        }

        public override int GetHashCode()
        {
            int hashFirstRoom = FirstRoom == null ? 0 : FirstRoom.GetHashCode();
            int hashLastRoom = LastRoom == null ? 0 : LastRoom.GetHashCode();

            return hashFirstRoom ^ hashLastRoom;
        }
    }

}

