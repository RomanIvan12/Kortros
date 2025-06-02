using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApartmentsProject.Models;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace ApartmentsProject.AuxiliaryСlasses
{
    public class ApartmentSearchAlgorithm
    {
        public static List<List<GeometryStackModel>> MakeApartments(List<GeometryStackModel> listOfModelElements)
        {
            var result = new List<List<GeometryStackModel>>();

            while (listOfModelElements.Count > 0)
            {
                List<GeometryStackModel> newList = new List<GeometryStackModel>();
                GeometryStackModel initialElement = listOfModelElements.First();
                listOfModelElements.RemoveAt(0);
                newList.Add(initialElement);

                bool hasMatches = true;
                while (hasMatches)
                {
                    hasMatches = false;
                    HashSet<ElementId> currentRoomIds = newList
                        .SelectMany(item => new List<ElementId> { item.FirstRoom?.Id, item.LastRoom?.Id })
                        .Where(id => id != null)
                        .ToHashSet();

                    for (int i = 0; i < listOfModelElements.Count; i++)
                    {
                        GeometryStackModel element = listOfModelElements[i];
                        if (element == null) continue;

                        ElementId firstRoomId = element.FirstRoom?.Id;
                        ElementId lastRoomId = element.LastRoom?.Id;

                        if (currentRoomIds.Contains(firstRoomId) || currentRoomIds.Contains(lastRoomId))
                        {
                            newList.Add(element);
                            listOfModelElements.RemoveAt(i);
                            hasMatches = true;
                            break;
                        }
                    }
                }
                result.Add(newList);
            }
            return result;
        }

        public static List<List<Room>> ConvertToRoomList(List<List<GeometryStackModel>> groupedGeometryModels)
        {
            List<List<Room>> result = new List<List<Room>>();
            foreach (List<GeometryStackModel> geometryModelList in groupedGeometryModels)
            {
                HashSet<Room> uniqueRooms = new HashSet<Room>(new RoomComparer());

                foreach (var model in geometryModelList)
                {
                    if (model.FirstRoom != null)
                        uniqueRooms.Add(model.FirstRoom);
                    if (model.LastRoom != null)
                        uniqueRooms.Add(model.LastRoom);
                }
                result.Add(uniqueRooms.ToList());
            }
            return result;
        }
    }

    public class RoomComparer : IEqualityComparer<Room>
    {
        public bool Equals(Room x, Room y)
        {
            return x.Id.Equals(y.Id);
        }

        public int GetHashCode(Room obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
