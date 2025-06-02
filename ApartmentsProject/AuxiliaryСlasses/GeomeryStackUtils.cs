using System;
using System.Collections.Generic;
using ApartmentsProject.Models;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace ApartmentsProject.AuxiliaryСlasses
{
    public class GeometryStackUtils
    {
        // Основной принцип алгоритма. На вход подаются List<Solid> помещений, List<Solid> дверей, List<Разд.линии>
        //public static Dictionary<Element, GeometryElement> GetRoomGeometry(Element room)
        public static KeyValuePair<Room, Solid> GetRoomSolid(Document doc, Element room)
        {
            // TODO: Добавить фильтр, чтобы убрать неразмещенные помещения или избыточные
            SpatialElementGeometryCalculator calculator = new SpatialElementGeometryCalculator(doc);
            SpatialElementGeometryResults results = calculator.CalculateSpatialElementGeometry((SpatialElement)room);

            Solid solid = results.GetGeometry();
            return new KeyValuePair<Room, Solid>((Room)room, solid);
        }

        public static Solid GetRoomGeometry(Document doc, Element room)
        {
            // TODO: Добавить фильтр, чтобы убрать неразмещенные помещения или избыточные
            SpatialElementGeometryCalculator calculator = new SpatialElementGeometryCalculator(doc);
            SpatialElementGeometryResults results = calculator.CalculateSpatialElementGeometry((SpatialElement)room);

            Solid solid = results.GetGeometry();
            return solid;
        }

        public static KeyValuePair<Element, Solid> GetDoorSolid(Document doc, Element door)
        {
            DoorSolids doorSolidInstance = new DoorSolids(door, doc);
            return new KeyValuePair<Element, Solid>(door, doorSolidInstance.Solid);
        }

        public static List<GeometryStackModel> CreateGeomStackOfDoor(Document doc, Dictionary<Element, Solid> dictOfDoorSolids, Dictionary<Room, Solid> dictOfRoomsSolids)
        {
            List<GeometryStackModel> result = new List<GeometryStackModel>();
            foreach (KeyValuePair<Element, Solid> doorEntry in dictOfDoorSolids)
            {
                Element separator = doorEntry.Key;
                Solid doorSolid = doorEntry.Value;

                Room firstRoom = null;
                Room lastRoom = null;
                Solid firstSolid = null;
                Solid lastSolid = null;

                // Перебираем каждое помещение Solid в словаре dictOfRoomsSolids для проверки пересечения
                foreach (KeyValuePair<Room, Solid> roomEntry in dictOfRoomsSolids)
                {
                    Room room = roomEntry.Key;
                    Solid roomSolid = roomEntry.Value;

                    Solid intersection =
                        BooleanOperationsUtils.ExecuteBooleanOperation(doorSolid, roomSolid,
                            BooleanOperationsType.Intersect);

                    if (intersection != null && intersection.Volume > 0)
                    {
                        if (firstRoom == null)
                        {
                            firstRoom = room;
                            firstSolid = roomSolid;
                        }
                        else
                        {
                            lastRoom = room;
                            lastSolid = roomSolid;
                        }
                    }
                }
                GeometryStackModel model = new GeometryStackModel(doc, firstRoom, lastRoom, doorEntry)
                {
                    FirstSolid = firstSolid,
                    LastSolid = lastSolid,
                };
                if (model.FirstRoom != null && model.FirstSolid != null)
                {
                    result.Add(model);
                }
            }
            return result;
        }

        /// <summary>
        /// Функция, создающая Стак между помещениями, разделёнными разделительными линиями
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="dictOfRoomsSolids"></param>
        /// <returns></returns>
        public static List<GeometryStackModel> CreateGeomStackOfSeparator(Document doc, Dictionary<Room, Solid> dictOfRoomsSolids)
        {
            List<GeometryStackModel> result = new List<GeometryStackModel>();

            var keys = new List<Room>(dictOfRoomsSolids.Keys);

            for (int i = 0; i < keys.Count; ++i)
            {
                Room firstRoom = keys[i];
                Solid firstSolid = dictOfRoomsSolids[firstRoom];

                for (int j = i + 1; j < keys.Count; ++j)
                {
                    Room lastRoom = keys[j];
                    Solid lastSolid = dictOfRoomsSolids[lastRoom];

                    // Check intersection TEST
                    if (AreSolidsAlignedOnAnyFace(firstSolid, lastSolid))
                    {
                        result.Add(new GeometryStackModel(doc, firstRoom, lastRoom, "Line")
                        {
                            FirstSolid = firstSolid,
                            LastSolid = lastSolid,
                        });
                    }
                }
            }
            return result;
        }

        private static bool CheckIntersection(Solid solid1, Solid solid2)
        {
            Solid newSolid = BooleanOperationsUtils.ExecuteBooleanOperation(
                solid1, solid2, BooleanOperationsType.Intersect);
            return !newSolid.Volume.Equals(0.0);
        }

        private static bool AreSolidsAlignedOnAnyFace(Solid solid1, Solid solid2)
        {
            // Перебор граней 1го солида
            foreach (Face face1 in solid1.Faces)
            {
                PlanarFace planarFace1 = face1 as PlanarFace;
                if (planarFace1 == null) continue;

                // перебор граней второго солида
                foreach (Face face2 in solid2.Faces)
                {
                    PlanarFace planarFace2 = face2 as PlanarFace;
                    if (planarFace2 == null) continue;

                    // Проверка, совпадают ли нормали граней
                    if (!planarFace1.FaceNormal.IsAlmostEqualTo(planarFace2.FaceNormal))
                        continue;
                    // Проверка, лежал ли грани в одной плоскости
                    if (AreFacesInSamePlane(planarFace1, planarFace2) && DoFacesTouch(planarFace1, planarFace2))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private static bool AreFacesInSamePlane(PlanarFace face1, PlanarFace face2)
        {
            XYZ point1 = face1.Origin;
            XYZ point2 = face2.Origin;

            // Нормаль первой грани
            XYZ normal1 = face1.FaceNormal;

            // Проверяем, лежат ли глазами точек начало второй грани на плоскости первой грани
            double tolerance = 1e-6;
            double distance = normal1.DotProduct(point2 - point1);

            // Проверка параллельности плоскостей по нормалям
            XYZ normal2 = face2.FaceNormal;
            bool areNormalsParallel = normal1.IsAlmostEqualTo(normal2) || normal1.IsAlmostEqualTo(-normal2);

            bool result = Math.Abs(distance) < tolerance && areNormalsParallel;

            return result;
        }

        private static bool DoFacesTouch(PlanarFace face1, PlanarFace face2)
        {
            EdgeArray edges1 = face1.EdgeLoops.get_Item(0);
            EdgeArray edges2 = face2.EdgeLoops.get_Item(0);

            // Перебираем все пары краев и проверяем, пересекаются ли они
            foreach (Edge edge1 in edges1)
            {
                Curve curve1 = edge1.AsCurve();
                foreach (Edge edge2 in edges2)
                {
                    Curve curve2 = edge2.AsCurve();
                    SetComparisonResult result =
                        curve1.Intersect(curve2, out IntersectionResultArray intersectionResults);

                    if (result == SetComparisonResult.Overlap)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
