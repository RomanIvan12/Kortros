using ApartmentsProject.AuxiliaryСlasses;
using ApartmentsProject.Models;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB;
using System.Windows.Controls;
using System.Windows.Media;
using Autodesk.Revit.UI.Selection;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Shapes;
using Line = Autodesk.Revit.DB.Line;

namespace ApartmentsProject.ViewModel
{
    public class ApartmentCreationCalculationVm : INotifyPropertyChanged
    {
        public List<Room> RoomsToCalculate { get; set; } = new List<Room>();
        public List<Level> LevelsToCalcualte { get; set; } = new List<Level>();

        private static Configuration _selectedConfiguration;
        public Configuration SelectedConfiguration
        {
            get => _selectedConfiguration;
            set
            {
                if (_selectedConfiguration != value)
                {
                    _selectedConfiguration = value;
                    OnPropertyChanged(nameof(SelectedConfiguration));
                }
            }
        }

        private ObservableCollection<ApartmentModel> _apartmentsOfObservableCollection;
        public ObservableCollection<ApartmentModel> ApartmentsOfObservableCollection
        {
            get => _apartmentsOfObservableCollection;
            set
            {
                _apartmentsOfObservableCollection = value;
                OnPropertyChanged(nameof(ApartmentsOfObservableCollection));
            }
        }


        private ObservableCollection<string> _viewOrProject { get; set; } = new ObservableCollection<string>
        {
            "На открытом виде",
            "Во всём проекте"
        };

        public ObservableCollection<string> ViewOrProject
        {
            get => _viewOrProject;
            set
            {
                if (_viewOrProject != value)
                {
                    _viewOrProject = value;
                    OnPropertyChanged(nameof(ViewOrProject));
                }
            }
        }

        private string _viewOrProjectPick;
        public string ViewOrProjectPick
        {
            get => _viewOrProjectPick;
            set => SetProperty(ref _viewOrProjectPick, value);
        }

        private RelayCommand _mainCalculation;
        private RelayCommand _selectRooms;

        private readonly ExternalEventHandler _externalEventHandler;
        private readonly ExternalEvent _externalEvent;

        public ApartmentCreationCalculationVm()
        {
            _externalEventHandler = new ExternalEventHandler();
            _externalEvent = ExternalEvent.Create(_externalEventHandler);

            ViewOrProjectPick = "На открытом виде";

            ApartmentsOfObservableCollection = new ObservableCollection<ApartmentModel>();

            _selectedConfiguration = RunCommand.ApartmentsProjectLayout.ConfigurationList.First(
                i => i.IsSelected == true);
            Mediator.SelectedConfigurationChanged += OnSelectedConfigurationChanged;
        }
        private void OnSelectedConfigurationChanged(Configuration newConfiguration)
        {
            SelectedConfiguration = newConfiguration;
        }

        public RelayCommand MainCalculationCommand
        {
            get
            {
                return _mainCalculation ??
                       (_mainCalculation = new RelayCommand(obj =>
                       {

                           // BCGHFDBNM исправить
                           RoomsToCalculate.Clear();
                           LevelsToCalcualte.Clear();
                           try
                           {
                               _externalEventHandler.SetAction(Calculate);
                               _externalEvent.Raise();
                           }
                           catch (Exception ex)
                           {
                               MessageBox.Show($"ошибка: {ex.Message}");
                           }
                       }));
            }
        }

        public RelayCommand SelectRoomsCommand
        {
            get
            {
                return _selectRooms ??
                       (_selectRooms = new RelayCommand(obj =>
                       {
                           RoomsToCalculate.Clear();
                           LevelsToCalcualte.Clear();
                           try
                           {
                               _externalEventHandler.SetAction(SelectRooms);
                               _externalEvent.Raise();
                           }
                           catch (Exception ex)
                           {
                               MessageBox.Show($"ошибка: {ex.Message}");
                           }
                       }));
            }
        }

        // ИСПРАВИТЬ
        private void Calculate()
        {
            ApartmentsOfObservableCollection.Clear();

            // Тут заполняются уровни и помещения, в зависимости от выбора
            // TODO: Здесь нет условия проверки, остались ли некорректные помещения, тк этот блок должен быть добавлен в класс VerificationVM
            switch (ViewOrProjectPick)
            {
                case "На открытом виде":
                    // Get currently opened view
                    var activeView = RunCommand.Doc.ActiveView;
                    if (activeView is View3D || activeView is ViewSection || activeView is ViewSheet || activeView is ViewDrafting || activeView is ViewSchedule)
                    {
                        MessageBox.Show("Откройте план этажа");
                        break;
                    }
                    else
                    {
                        LevelsToCalcualte.Add(activeView.GenLevel);
                        RoomsToCalculate = VerificationVm.Rooms.Where(i => i.Level.Id.IntegerValue == activeView.GenLevel.Id.IntegerValue).ToList();
                    }
                    break;
                case "Во всём проекте":
                    LevelsToCalcualte = new FilteredElementCollector(RunCommand.Doc).OfClass(typeof(Level)).ToElements().Cast<Level>().ToList();
                    RoomsToCalculate = VerificationVm.Rooms.ToList();
                    break;
            }

            var clearCommonParameters = (ClearCommonParametersBeforeCalculation)Enum.Parse(typeof(ClearCommonParametersBeforeCalculation), _selectedConfiguration.Settings.ClearCommonParametersBeforeCalculation, true);
            switch (clearCommonParameters)
            {
                case ClearCommonParametersBeforeCalculation.ClearAll:
                    ClearParametersOfRooms(RoomsToCalculate);
                    break;
                case ClearCommonParametersBeforeCalculation.NotClear:
                    break;
            }

            // Собираем квартиры
            Dictionary<Room, Solid> dictOfRoomsSolids = new Dictionary<Room, Solid>();
            foreach (Room room in RoomsToCalculate)
            {
                var el = (Element)room;
                var pair = GeometryStackUtils.GetRoomSolid(RunCommand.Doc, el);
                dictOfRoomsSolids.Add(pair.Key, pair.Value);
            }

            // Создал стаки для Разд Линий
            var geometryStacksOfSeparator = GeometryStackUtils.CreateGeomStackOfSeparator(RunCommand.Doc, dictOfRoomsSolids);

            List<Element> doorList = new List<Element>();
            foreach (Level level in LevelsToCalcualte)
            {
                doorList.AddRange(new FilteredElementCollector(RunCommand.Doc).OfCategory(BuiltInCategory.OST_Doors)
                    .OfClass(typeof(FamilyInstance))
                    .WhereElementIsNotElementType().ToElements()
                    .Where(item => item.LevelId.IntegerValue == level.Id.IntegerValue)
                    .Cast<FamilyInstance>()
                    .Where(elemInst => elemInst?.SuperComponent == null)
                    .Cast<Element>()
                    .ToList());
            }

            Dictionary<Element, Solid> dictOfDoorsSolids = new Dictionary<Element, Solid>();
            foreach (Element door in doorList)
            {
                var pair = GeometryStackUtils.GetDoorSolid(RunCommand.Doc, door);
                dictOfDoorsSolids.Add(pair.Key, pair.Value);
            }

            var geometryStacksOfDoor =
                GeometryStackUtils.CreateGeomStackOfDoor(RunCommand.Doc, dictOfDoorsSolids, dictOfRoomsSolids);

            geometryStacksOfSeparator.AddRange(geometryStacksOfDoor);
            var geometryStackApartments = ApartmentSearchAlgorithm.MakeApartments(geometryStacksOfSeparator); // Собранная коллекция стаков

            // Список квартир типа List<Room>. Корректный
            var rooms = ApartmentSearchAlgorithm.ConvertToRoomList(geometryStackApartments);

            using (Transaction t = new Transaction(RunCommand.Doc, "CreateApartments"))
            {
                t.Start();
                ApartmentRepository.Instance.Initialize(rooms);
                ApartmentsOfObservableCollection = ApartmentRepository.Instance.Apartments;
                t.Commit();
            }
        }

        private void SelectRooms()
        {
            IList<Element> selected = new List<Element>();
            
            ISelectionFilter selFilter = new RoomSelectionFilter();
            IList<Reference> references = RunCommand.UiDoc.Selection.PickObjects(
                ObjectType.Element, selFilter);

            foreach (Reference item in references)
            {
                selected.Add(RunCommand.Doc.GetElement(item.ElementId));
            }
            RoomsToCalculate.Clear();
            LevelsToCalcualte.Clear();

            foreach (var room in selected)
                RoomsToCalculate.Add(room as Room);

            var clearCommonParameters = (ClearCommonParametersBeforeCalculation)Enum.Parse(typeof(ClearCommonParametersBeforeCalculation), _selectedConfiguration.Settings.ClearCommonParametersBeforeCalculation, true);
            switch (clearCommonParameters)
            {
                case ClearCommonParametersBeforeCalculation.ClearAll:
                    ClearParametersOfRooms(RoomsToCalculate);
                    break;
                case ClearCommonParametersBeforeCalculation.NotClear:
                    break;
            }

            var activeView = RunCommand.Doc.ActiveView;
            if (activeView is View3D || activeView is ViewSection || activeView is ViewSheet || activeView is ViewDrafting || activeView is ViewSchedule)
            {
                MessageBox.Show("Откройте план этажа");
            }
            else
            {
                LevelsToCalcualte.Add(activeView.GenLevel);
            }

            // Собираем квартиры
            Dictionary<Room, Solid> dictOfRoomsSolids = new Dictionary<Room, Solid>();
            foreach (Room room in RoomsToCalculate)
            {
                var el = (Element)room;
                var pair = GeometryStackUtils.GetRoomSolid(RunCommand.Doc, el);
                dictOfRoomsSolids.Add(pair.Key, pair.Value);
            }

            // Создал стаки для Разд Линий
            var geometryStacksOfSeparator = GeometryStackUtils.CreateGeomStackOfSeparator(RunCommand.Doc, dictOfRoomsSolids);

            List<Element> doorList = new List<Element>();
            foreach (Level level in LevelsToCalcualte)
            {
                doorList.AddRange(new FilteredElementCollector(RunCommand.Doc).OfCategory(BuiltInCategory.OST_Doors)
                    .OfClass(typeof(FamilyInstance))
                    .WhereElementIsNotElementType().ToElements()
                    .Where(item => item.LevelId.IntegerValue == level.Id.IntegerValue)
                    .Cast<FamilyInstance>()
                    .Where(elemInst => elemInst?.SuperComponent == null)
                    .Cast<Element>()
                    .ToList());
            }

            Dictionary<Element, Solid> dictOfDoorsSolids = new Dictionary<Element, Solid>();
            foreach (Element door in doorList)
            {
                var pair = GeometryStackUtils.GetDoorSolid(RunCommand.Doc, door);
                dictOfDoorsSolids.Add(pair.Key, pair.Value);
            }

            var geometryStacksOfDoor =
                GeometryStackUtils.CreateGeomStackOfDoor(RunCommand.Doc, dictOfDoorsSolids, dictOfRoomsSolids);

            geometryStacksOfSeparator.AddRange(geometryStacksOfDoor);
            var geometryStackApartments = ApartmentSearchAlgorithm.MakeApartments(geometryStacksOfSeparator); // Собранная коллекция стаков

            // Список квартир типа List<Room>. Корректный
            var rooms = ApartmentSearchAlgorithm.ConvertToRoomList(geometryStackApartments);

            using (Transaction t = new Transaction(RunCommand.Doc, "CreateApartments"))
            {
                t.Start();
                ApartmentRepository.Instance.Initialize(rooms);
                ApartmentsOfObservableCollection = ApartmentRepository.Instance.Apartments;
                t.Commit();
            }
            CalculateSelectedApartments();
        }


        private RelayCommand _calculationCommand;
        public RelayCommand CalculationCommand
        {
            get
            {
                return _calculationCommand ??
                       (_calculationCommand = new RelayCommand(obj =>
                       {
                           try
                           {
                               _externalEventHandler.SetAction(CalculateSelectedApartments);
                               _externalEvent.Raise();
                           }
                           catch (Exception ex)
                           {
                               MessageBox.Show($"ошибка ");
                           }
                       }));
            }
        }

        private void ClearParametersOfRooms(List<Room> roomsToClear)
        {
            using (Transaction t = new Transaction(RunCommand.Doc, "Clear Parameters"))
            {
                t.Start();
                foreach (var room in roomsToClear)
                {
                    foreach (var model in ApartmentParameterMappingVm.MappingModel)
                    {
                        // if (model.DataOrigin.Name == "KRT_Функциональное назначение")
                        if (model.DataOrigin.Name == "ПО_Функц. назначение")
                            continue;

                        var guid = model.ParameterToMatch.Guid;
                        var parameter = room.get_Parameter(guid);

                        if (parameter == null)
                            continue;

                        switch (parameter.StorageType)
                        {
                            case StorageType.String:
                                parameter.Set("");
                                break;
                            case StorageType.Integer:
                                parameter.Set(0);
                                break;
                            case StorageType.Double:
                                parameter.Set(0);
                                break;
                        }
                    }
                }
                t.Commit();
            }
        }

        private void CalculateSelectedApartments()
        {
            using (Transaction t = new Transaction(PluginSettings.Instance.RevitDoc, "SetAreas"))
            {
                t.Start();
                foreach (var apartmentModel in ApartmentRepository.Instance.Apartments)
                {
                    CalculateAreas(apartmentModel);
                }
                t.Commit();
            }
        }

        private void CalculateAreas(ApartmentModel apartmentModel)
        {
            ForgeTypeId areaUnits = PluginSettings.Instance.RevitDoc.GetUnits().GetFormatOptions(SpecTypeId.Area)
                .GetUnitTypeId();

            // Заполняем "Площадь округлённая" и "KRT_Площадь с коэффициентом"
            Guid areaGuid = ApartmentParameterMappingVm.MappingModel
                .First(item => item.DataOrigin.Name == "KRT_Площадь округлённая")
                .ParameterToMatch.Guid;

            Guid areaKfGuid = ApartmentParameterMappingVm.MappingModel
                .First(item => item.DataOrigin.Name == "KRT_Площадь с коэффициентом")
                .ParameterToMatch.Guid;

            Guid kfGuid = ApartmentParameterMappingVm.MappingModel
                .First(item => item.DataOrigin.Name == "KRT_Коэффициент площади")
                .ParameterToMatch.Guid;

            double areaLiving = 0; // S жилая
            Guid areaLivingGuid = ApartmentParameterMappingVm.MappingModel
                .First(item => item.DataOrigin.Name == "KRT_Площадь жилая")
                .ParameterToMatch.Guid;

            double areaOfApartment = 0; // S квартиры
            Guid areaOfApartmentGuid = ApartmentParameterMappingVm.MappingModel
                .First(item => item.DataOrigin.Name == "KRT_Площадь квартиры")
                .ParameterToMatch.Guid;

            double areaTotal = 0; // S общая
            Guid areaTotalGuid = ApartmentParameterMappingVm.MappingModel
                .First(item => item.DataOrigin.Name == "KRT_Площадь общая")
                .ParameterToMatch.Guid;

            double areaTotalNoK = 0; // S общая без коэффициентов
            Guid areaTotalNoKGuid = ApartmentParameterMappingVm.MappingModel
                .First(item => item.DataOrigin.Name == "KRT_Площадь общая без коэфф.")
                .ParameterToMatch.Guid;

            double areaNonHeat = 0; // KRT_Площадь неотапл. помещений с коэфф.
            Guid areaNonHeatGuid = ApartmentParameterMappingVm.MappingModel
                .First(item => item.DataOrigin.Name == "KRT_Площадь неотапл. помещений с коэфф.")
                .ParameterToMatch.Guid;

            foreach (var room in apartmentModel.RoomsOfApartment)
            {
                var area = room.get_Parameter(BuiltInParameter.ROOM_AREA).AsDouble();

                Guid finishingGuid = ApartmentParameterMappingVm.MappingModel
                .First(item => item.DataOrigin.Name == "KRT_Толщина черновой отделки")
                .ParameterToMatch.Guid;
                var finishingThickness = room.get_Parameter(finishingGuid).AsDouble();

                if (finishingThickness != 0)
                {
                    area = AreaWithOffset(room, finishingThickness);
                }
                
                var convertedArea = UnitUtils.ConvertFromInternalUnits(area, areaUnits); // Площать в ревите

                var roundingValue = EnumExtensions.GetEnumDescription<AreaRoundType>(_selectedConfiguration.Settings.AreaRoundType, AreaRoundType.Units);

                var selectedRounding = double.Parse(roundingValue, CultureInfo.InvariantCulture);

                var roundedValue = RoundTo(
                    convertedArea,
                    selectedRounding);

                var convertBackValue = UnitUtils.ConvertToInternalUnits(roundedValue, areaUnits);
                var ratioValue = room.get_Parameter(kfGuid).AsDouble();

                room.get_Parameter(areaGuid).Set(convertBackValue); // Площадь округлённая
                room.get_Parameter(areaKfGuid).Set(convertBackValue * ratioValue); // KRT_Площадь с коэффициентом

                Guid typeGuid = ApartmentParameterMappingVm.MappingModel
                    .First(item => item.DataOrigin.Name == "KRT_Тип помещения")
                    .ParameterToMatch.Guid;
                var typeName = room.get_Parameter(typeGuid).AsString();

                if (typeName == "Living")
                    areaLiving += convertBackValue;
                if (ratioValue >= 1)
                    areaOfApartment += (convertBackValue * ratioValue);
                if (ratioValue < 1)
                    areaNonHeat += (convertBackValue * ratioValue);
                areaTotal += convertBackValue * ratioValue;
                areaTotalNoK += convertBackValue;
            }

            apartmentModel.RoomsOfApartment.ForEach(room => room.get_Parameter(areaLivingGuid).Set(areaLiving));
            apartmentModel.LivingRoomsSumArea = areaLiving;

            apartmentModel.RoomsOfApartment.ForEach(
                room => room.get_Parameter(areaOfApartmentGuid).Set(areaOfApartment));
            apartmentModel.RoomsSumArea = areaOfApartment;

            apartmentModel.RoomsOfApartment.ForEach(room => room.get_Parameter(areaTotalGuid).Set(areaTotal));
            apartmentModel.AllRoomsSumArea = areaTotal;

            apartmentModel.RoomsOfApartment.ForEach(room => room.get_Parameter(areaTotalNoKGuid).Set(areaTotalNoK));
            apartmentModel.AllRoomsSumAreaNoKof = areaTotalNoK;

            apartmentModel.RoomsOfApartment.ForEach(room => room.get_Parameter(areaNonHeatGuid).Set(areaNonHeat));
            apartmentModel.AllNonHeatRoomsSumArea = areaNonHeat;
        }

        #region Функции округлений
        private double RoundTo(double value, double step)
        {
            int decimalPlaces = GetDecimalPlaces(step);

            // Округляем сначала на 1 знак больше, чем требуется
            int targetDec = decimalPlaces + 1;
            double targetValue = Math.Round(value, targetDec, MidpointRounding.AwayFromZero);

            // Окончательное округление
            double roundedValue = Math.Round(targetValue / step) * step;
            return roundedValue;
        }
        static int GetDecimalPlaces(double step)
        {
            int decimalPlaces = 0;

            while (step < 1)
            {
                step *= 10;
                decimalPlaces++;
            }
            return decimalPlaces;
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private bool SetProperty<T>(ref T storage, T value, string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        private double AreaWithOffset(Room room, double offsetValue)
        {
            IList<IList<BoundarySegment>> boundaries = room.GetBoundarySegments(new SpatialElementBoundaryOptions());

            IList<BoundarySegment> mainBoundary = boundaries[0];

            // Получаем список точек
            List<XYZ> contourPoints = new List<XYZ>();
            foreach (var seg in mainBoundary)
            {
                // Добавляем начало сегмента (конец - будет добавлен как начало следующего сегмента)
                contourPoints.Add(seg.GetCurve().GetEndPoint(0));
            }
            PolyLine polyline = PolyLine.Create(contourPoints);
            double offsetFt = UnitUtils.ConvertToInternalUnits(offsetValue, UnitTypeId.Millimeters);

            // Для смещения используем метод CurveLoop.CreateViaOffset
            CurveLoop originalLoop = new CurveLoop();
            foreach (var seg in mainBoundary)
            {
                originalLoop.Append(seg.GetCurve());
            }
            XYZ point = room.Location is LocationPoint lp ? lp.Point.Z > 0 ? XYZ.BasisZ : -XYZ.BasisZ : XYZ.BasisZ;

            var offsetLoops = CurveLoop.CreateViaOffset(
                originalLoop, -offsetFt, point);

            //if (offsetLoops.Count() == 0)
            //{
            //    // ДОБАВИТЬ В ИСКЛЮЧЕНИЯ
            //    return 0;
            //}
            //return UnitUtils.ConvertFromInternalUnits(internalArea, UnitTypeId.SquareMeters);
            return Math.Abs(GetCurveLoopArea(offsetLoops));
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

    public class RoomSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Rooms)
                return true;
            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}