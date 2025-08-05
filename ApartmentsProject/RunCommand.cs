using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ApartmentsProject.AuxiliaryСlasses;
using ApartmentsProject.Models;
using ApartmentsProject.View;
using ApartmentsProject.ViewModel.Utilities;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Application = Autodesk.Revit.ApplicationServices.Application;

namespace ApartmentsProject
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RunCommand : IExternalCommand
    {
        public static Document Doc { get; set; }
        public static UIDocument UiDoc { get; set; }
        public static Application App { get; set; }
        public static MainWindow MainWindow { get; set; }

        public static ApartmentsProjectLayout ApartmentsProjectLayout { get; set; }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Logger.Log.Info("--- Плагин квартирографии запущен ---");

            Doc = commandData.Application.ActiveUIDocument.Document;
            App = commandData.Application.Application;
            UiDoc = commandData.Application.ActiveUIDocument;

            var roomElement = new FilteredElementCollector(RunCommand.Doc)
                .OfCategory(BuiltInCategory.OST_Rooms).FirstElement();
            if (roomElement == null)
            {
                MessageBox.Show("В проекте нет помещений");
                Logger.Log.Error("В проекте нет помещений");
                return Result.Failed;
            }

            try
            {
                CreateCommonParameters.ExtractFileToTemporaryFile("Apts_KRTRS.txt");
                var service = ConfigurationService.Instance;
                ApartmentsProjectLayout layout = service.LoadConfiguration();
                service.SaveConfiguration(layout);
                ApartmentsProjectLayout = layout;

                PluginSettings.Instance.RoomCollector = new FilteredElementCollector(Doc).OfCategory(BuiltInCategory.OST_Rooms);

                MainWindow = new MainWindow();
                MainWindow.Show();
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                Logger.Log.Error($"Ошибка команды: {message} ");
                return Result.Failed;
            }
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class TestCommandToTest : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            var app = commandData.Application;
            var uidoc = app.ActiveUIDocument;


            List<Element> roomsList = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Rooms)
                .Where(item => item.LevelId.IntegerValue == 1143796
                               && item.LookupParameter("ПО_Функц. назначение").AsString() == "Квартира").ToList();
            // ПО_Функц. назначение

            Dictionary<Room, Solid> dictOfRoomsSolids = new Dictionary<Room, Solid>();
            foreach (Element room in roomsList)
            {
                var pair = GeometryStackUtils.GetRoomSolid(doc, room);
                dictOfRoomsSolids.Add(pair.Key, pair.Value);
            }
            // Создал стаки для Разд Линий
            var geometryStacksOfSeparator = GeometryStackUtils.CreateGeomStackOfSeparator(doc, dictOfRoomsSolids);
            //var solids = geometryStacksOfSeparator.SelectMany(gsm => new[] {gsm.FirstSolid, gsm.LastSolid}).ToList();


            List<Element> doorList = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Doors)
                .OfClass(typeof(FamilyInstance))
                .WhereElementIsNotElementType().ToElements()
                .Where(item => item.LevelId.IntegerValue == 1143796)
                .Cast<FamilyInstance>()
                .Where(elemInst => elemInst?.SuperComponent == null)
                .Cast<Element>()
                .ToList();

            Dictionary<Element, Solid> dictOfDoorsSolids = new Dictionary<Element, Solid>();
            foreach (Element door in doorList)
            {
                var pair = GeometryStackUtils.GetDoorSolid(doc, door);
                dictOfDoorsSolids.Add(pair.Key, pair.Value);
            }


            var geometryStacksOfDoor =
                GeometryStackUtils.CreateGeomStackOfDoor(doc, dictOfDoorsSolids, dictOfRoomsSolids);


            geometryStacksOfSeparator.AddRange(geometryStacksOfDoor);

            var geometryStackApartments = ApartmentSearchAlgorithm.MakeApartments(geometryStacksOfSeparator);

            var rooms = ApartmentSearchAlgorithm.ConvertToRoomList(geometryStackApartments);

            int counter = 1;
            using (Transaction t = new Transaction(doc, "TEST"))
            {
                t.Start();
                foreach (List<Room> listRooms in rooms)
                {
                    foreach (Room room in listRooms)
                    {
                        var element = (Element)room;
                        element.LookupParameter("Тестовый для группы2").Set($"КВАРТИРА №{counter.ToString()}");
                    }
                    counter++;
                }
                t.Commit();
            }

            #region скрыть
            ////Element elementRoom = doc.GetElement(new ElementId(6626091)); // ID помещения


            //foreach (var room in roomsList)
            //{
            //    using (Transaction t = new Transaction(doc, "Create Generic Model"))
            //    {
            //        t.Start();
            //        DirectShape directShape = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));
            //        directShape.ApplicationId = "App id";
            //        directShape.ApplicationDataId = "Geom obj";
            //        directShape.SetShape(new GeometryObject[] { GeometryStackUtils.GetRoomGeometry(doc, room) });
            //        t.Commit();
            //    }
            //}
            #endregion
            return Result.Succeeded;
        }
    }


    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class TestDatStorage : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var document = commandData.Application.ActiveUIDocument.Document;
            var application = commandData.Application.Application;

            using (Transaction t = new Transaction(document, "test"))
            {
                t.Start();
                ParameterFolderSchema schema = new ParameterFolderSchema(document);
                t.Commit();
            }
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class TestCentroid : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var doc = commandData.Application.ActiveUIDocument.Document;
            var application = commandData.Application.Application;

            var point = NumberingExtention.GetRoomCentroid(doc.GetElement(new ElementId(4253421)) as Room);
            PlaceMark(point, doc);        
            return Result.Succeeded;
        }
        private void PlaceMark(XYZ point, Document doc)
        {

            using (Transaction tr = new Transaction(doc, "test"))
            {
                tr.Start();
                FamilySymbol symbol = doc.GetElement(new ElementId(6629955)) as FamilySymbol;

                doc.Create.NewFamilyInstance(point, symbol, doc.ActiveView);
                // OneLevelBased
                tr.Commit();
            }
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class TestRoomBoudaries : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var document = commandData.Application.ActiveUIDocument.Document;
            var application = commandData.Application.Application;

            Room room = document.GetElement(new ElementId(4253381)) as Room;

            IList<IList<BoundarySegment>> boundaries = room.GetBoundarySegments(new SpatialElementBoundaryOptions());

            // Работаем только с первым замкнутым контуром (основной контур)
            IList<BoundarySegment> mainBoundary = boundaries[0];

            // Получаем список точек
            List<XYZ> contourPoints = new List<XYZ>();
            foreach (var seg in mainBoundary)
            {
                // Добавляем начало сегмента (конец - будет добавлен как начало следующего сегмента)
                contourPoints.Add(seg.GetCurve().GetEndPoint(0));
            }

            // Строим полиэлайн
            PolyLine polyline = PolyLine.Create(contourPoints);

            // Смещаем контур на 100 мм внутрь
            double offsetMm = 100.0;
            double offsetFt = UnitUtils.ConvertToInternalUnits(offsetMm, UnitTypeId.Millimeters);

            // Для смещения используем метод CurveLoop.CreateViaOffset
            CurveLoop originalLoop = new CurveLoop();
            foreach (var seg in mainBoundary)
            {
                originalLoop.Append(seg.GetCurve());
            }

            XYZ point = room.Location is LocationPoint lp ? lp.Point.Z > 0 ? XYZ.BasisZ : -XYZ.BasisZ : XYZ.BasisZ;

            var offsetLoops = CurveLoop.CreateViaOffset(
                originalLoop, -offsetFt, point);

            // Берём первый смещённый контур
            //CurveLoop offsetLoop = offsetLoops[0];

            // Вычисляем площади
            double externalArea = Math.Abs(GetCurveLoopArea(originalLoop));
            double internalArea = Math.Abs(GetCurveLoopArea(offsetLoops));
            double diffArea = externalArea - internalArea;

            // Переводим площадь в м²
            double extAreaSqM = UnitUtils.ConvertFromInternalUnits(externalArea, UnitTypeId.SquareMeters);
            double intAreaSqM = UnitUtils.ConvertFromInternalUnits(internalArea, UnitTypeId.SquareMeters);
            double diffAreaSqM = UnitUtils.ConvertFromInternalUnits(diffArea, UnitTypeId.SquareMeters);

            TaskDialog.Show("Разность площадей", $"Исходная площадь: {extAreaSqM:F2} м²\n" +
                                                 $"Внутренняя (после смещения): {intAreaSqM:F2} м²\n" +
                                                 $"Разность: {diffAreaSqM:F2} м²");

            return Result.Succeeded;
        }
        private double GetCurveLoopArea(CurveLoop loop)
        {
            List<XYZ> pts = loop.Select(c => c.GetEndPoint(0)).ToList();
            // Площадь по формуле шнурков (Shoelace/Гаусса)
            double area = 0;
            for (int i = 0; i < pts.Count; i++)
            {
                XYZ p1 = pts[i];
                XYZ p2 = pts[(i + 1) % pts.Count];
                area += (p1.X * p2.Y - p2.X * p1.Y);
            }
            return Math.Abs(area) * 0.5;
        }
    }

}
