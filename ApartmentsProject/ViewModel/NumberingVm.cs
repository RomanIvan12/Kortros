using ApartmentsProject.AuxiliaryСlasses;
using ApartmentsProject.Models;
using Autodesk.Revit.DB;
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

namespace ApartmentsProject.ViewModel
{
    public class NumberingVm : INotifyPropertyChanged
    {
        private string _parameterName;
        public string ParameterName
        {
            get => _parameterName;
            set
            {
                if (_parameterName != value)
                {
                    _parameterName = value;
                    OnPropertyChanged(nameof(ParameterName));
                }
            }
        }

        private static Configuration _selectedConfiguration;
        public Configuration SelectedConfiguration
        {
            get => _selectedConfiguration;
            set
            {
                if (_selectedConfiguration != value)
                {
                    _selectedConfiguration = value;
                    UpdateNumberingSettings();
                    OnPropertyChanged(nameof(SelectedConfiguration));
                }
            }
        }

        public ObservableCollection<string> NumberingDirectionOptions { get; set; }
        private string _selectedNumberingDirection;
        public string SelectedNumberingDirection
        {
            get => _selectedNumberingDirection;
            set
            {
                if (_selectedNumberingDirection != value)
                {
                    _selectedNumberingDirection = value;
                    _selectedConfiguration.NumberingSettings.NumberingDirection =
                        EnumExtensions.GetEnumValueDescription<NumberingDirection>(_selectedNumberingDirection);
                    OnPropertyChanged(nameof(SelectedNumberingDirection));
                    ConfigurationService.Instance.SaveConfiguration(RunCommand.ApartmentsProjectLayout);
                }
            }
        }

        private int _initNumber;
        public int InitNumber
        {
            get => _initNumber;
            set
            {
                if (_initNumber != value)
                {
                    _initNumber = value;
                    _selectedConfiguration.NumberingSettings.InitNumber = _initNumber;
                    OnPropertyChanged(nameof(InitNumber));
                    ConfigurationService.Instance.SaveConfiguration(RunCommand.ApartmentsProjectLayout);
                }
            }
        }

        public ObservableCollection<string> NumberingStartOptions { get; set; }
        private string _selectedNumberingStart;
        public string SelectedNumberingStart
        {
            get => _selectedNumberingStart;
            set
            {
                if (_selectedNumberingStart != value)
                {
                    _selectedNumberingStart = value;
                    _selectedConfiguration.NumberingSettings.NumberingStart =
                        EnumExtensions.GetEnumValueDescription<NumberingStart>(_selectedNumberingStart);
                    OnPropertyChanged(nameof(SelectedNumberingStart));
                    ConfigurationService.Instance.SaveConfiguration(RunCommand.ApartmentsProjectLayout);
                }
            }
        }

        private bool _isEnumForEachLevel;
        public bool IsEnumForEachLevel
        {
            get => _isEnumForEachLevel;
            set
            {
                if (_isEnumForEachLevel != value)
                {
                    _isEnumForEachLevel = value;
                    _selectedConfiguration.NumberingSettings.ResetNumberForEachLevel = _isEnumForEachLevel;
                    OnPropertyChanged(nameof(IsEnumForEachLevel));
                    ConfigurationService.Instance.SaveConfiguration(RunCommand.ApartmentsProjectLayout);
                }
            }
        }

        private bool _addPrefix;
        public bool AddPrefix
        {
            get => _addPrefix;
            set
            {
                if (_addPrefix != value)
                {
                    _addPrefix = value;
                    _selectedConfiguration.NumberingSettings.AddPrefix = _addPrefix;
                    OnPropertyChanged(nameof(AddPrefix));
                    ConfigurationService.Instance.SaveConfiguration(RunCommand.ApartmentsProjectLayout);
                }
            }
        }

        private string _prefixValue;
        public string PrefixValue
        {
            get => _prefixValue;
            set
            {
                if (_prefixValue != value)
                {
                    _prefixValue = value;
                    _selectedConfiguration.NumberingSettings.FixToNumber = _prefixValue;
                    OnPropertyChanged(nameof(PrefixValue));
                    ConfigurationService.Instance.SaveConfiguration(RunCommand.ApartmentsProjectLayout);
                }
            }
        }

        private bool _isPrefix;
        public bool IsPrefix
        {
            get => _isPrefix;
            set
            {
                if (_isPrefix != value)
                {
                    _isPrefix = value;
                    _selectedConfiguration.NumberingSettings.IsPrefix = _isPrefix;
                    OnPropertyChanged(nameof(IsPrefix));
                    ConfigurationService.Instance.SaveConfiguration(RunCommand.ApartmentsProjectLayout);
                }
            }
        }

        private Document _doc = RunCommand.Doc;


        private FilteredElementCollector _roomCollector = PluginSettings.Instance.RoomCollector;
        private readonly ExternalEventHandler _externalEventHandler;
        private readonly ExternalEvent _externalEvent;

        private RelayCommand _createNumOnView;
        public RelayCommand CreateNumOnView
        {
            get
            {
                return _createNumOnView ??
                       (_createNumOnView = new RelayCommand(obj =>
                       {
                           try
                           {
                               _externalEventHandler.SetAction(CreateNumberingCurrentLevel);
                               _externalEvent.Raise();
                           }
                           catch (Exception ex)
                           {
                               MessageBox.Show($"ошибка: {ex.Message}");
                           }
                       }));
            }
        }

        public NumberingVm()
        {
            GetNumberingParameterName();
            EventAggregator.Instance.SubscribeAllCorrect(OnAllCorrect);

            NumberingDirectionOptions = new ObservableCollection<string>(
                Enum.GetValues(typeof(NumberingDirection))
                .Cast<NumberingDirection>()
                .Select(x => x.GetDescription())
                );
            NumberingStartOptions = new ObservableCollection<string>(
                Enum.GetValues(typeof(NumberingStart))
                .Cast<NumberingStart>()
                .Select(x => x.GetDescription())
                );

            _selectedConfiguration = RunCommand.ApartmentsProjectLayout.ConfigurationList.First(
                i => i.IsSelected == true);
            Mediator.SelectedConfigurationChanged += OnSelectedConfigurationChanged;
            UpdateNumberingSettings();

            _externalEventHandler = new ExternalEventHandler();
            _externalEvent = ExternalEvent.Create(_externalEventHandler);
        }
        private void OnAllCorrect(object sender, EventArgs e)
        {
            GetNumberingParameterName();
        }
        private void GetNumberingParameterName()
        {
            try
            {
                Guid numberDefaultValue = ApartmentParameterMappingVm.MappingModel
                    .First(item => item.DataOrigin.Name == "KRT_№ Квартиры")
                    .ParameterToMatch.Guid;
                ParameterName = _roomCollector.First().get_Parameter(numberDefaultValue).Definition.Name;
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex.Message);
            }
        }

        private void CreateNumberingCurrentLevel()
        {
            NumberingDirection direction = EnumExtensions.GetEnumValueByDescription<NumberingDirection>(_selectedNumberingDirection, NumberingDirection.Сlockwise);
            NumberingStart option = EnumExtensions.GetEnumValueByDescription<NumberingStart>(_selectedNumberingStart, NumberingStart.Top);

            ////////////////
            List<XYZ> points = FindMopCenterPoints();
            foreach (var singlePoint in points)
                PlaceMark(singlePoint);

            // Получаю список центров квартир -> множество точек, обозначающее центры квартир на ВСЕХ УРОВНЯХ
            Dictionary<Level, List<ApartmentModel>> apartmentsByLevel = ApartmentRepository.Instance.Apartments
                .GroupBy(apt => apt.LevelElement, new LevelComparer())
                .ToDictionary(
                    group => group.Key,
                    group => group.ToList(),
                    new LevelComparer()
                );

            int initNumber = InitNumber;
            foreach (Level level in apartmentsByLevel.Keys.OrderBy(item => item.Elevation))
            {
                XYZ mopPoint = FindMopCenterPointByLevelName(level.Name);

                List<ApartmentModel> apartmentsCurrentLevel = apartmentsByLevel[level];

                foreach (var apartment in apartmentsCurrentLevel)
                {
                    PlaceMark(apartment.GeomCenterPoint);
                }

                // Получаю список центров квартир -> множество точек, обозначающее центры квартир
                List<XYZ> apartmentPointsCurrentLevel = apartmentsCurrentLevel.Select(item => item.GeomCenterPoint).ToList();

                // Нужно создать прямоугольник, который вписывает точки выше. Он нужен будет для вычисления отклонения от центра МОПов
                List<XYZ> pointsOfRectangleLevel = RectangleByPoints.CreateRectangle(apartmentPointsCurrentLevel);

                XYZ finishPointRectangle = RectangleSolver.GetFinishPoint(pointsOfRectangleLevel, mopPoint, option); // Это точка середины отрезка,
                                                                                                                     // на которую падает луч по направлению
                
                using (Transaction tr = new Transaction(_doc, "Numerate"))
                {
                    tr.Start();
                    Plane plane = Plane.CreateByNormalAndOrigin(_doc.ActiveView.ViewDirection, _doc.ActiveView.Origin);
                    // Создаю линии
                    SketchPlane sketchPlane = SketchPlane.Create(_doc, plane);
                    _doc.Create.NewDetailCurve(_doc.ActiveView, Line.CreateBound(pointsOfRectangleLevel[0], pointsOfRectangleLevel[1]));
                    _doc.Create.NewDetailCurve(_doc.ActiveView, Line.CreateBound(pointsOfRectangleLevel[1], pointsOfRectangleLevel[2]));
                    _doc.Create.NewDetailCurve(_doc.ActiveView, Line.CreateBound(pointsOfRectangleLevel[2], pointsOfRectangleLevel[3]));
                    _doc.Create.NewDetailCurve(_doc.ActiveView, Line.CreateBound(pointsOfRectangleLevel[3], pointsOfRectangleLevel[0]));
                    //
                    var outNumber = CreateNumbersOutNumber(apartmentPointsCurrentLevel, mopPoint, finishPointRectangle, direction, initNumber);
                    _doc.Create.NewDetailCurve(_doc.ActiveView, Line.CreateBound(mopPoint, finishPointRectangle));

                    initNumber = outNumber + 1;

                    tr.Commit();
                }
            }
        }

        private XYZ FindMopCenterPointByLevelName(string level)
        {
            Guid function = ApartmentParameterMappingVm.MappingModel
                .First(item => item.DataOrigin.Name == "KRT_Функциональное назначение")
                .ParameterToMatch.Guid;

            List<Room> allRooms = _roomCollector.Where(element => element.get_Parameter(function)
                    .AsString() == "МОП").Where(i => _doc.GetElement(i.LevelId).Name == level)
                .Cast<Room>().ToList();

            //Нахожу геометрический центр МОПов
            List<XYZ> mopPts = allRooms.Select(room => NumberingExtention.GetRoomCentroid(room)).ToList();
            return NumberingExtention.GetGeometricCenter(mopPts);
        }

        private List<XYZ> FindMopCenterPoints() // получаю список центров мопов для всех уровней
        {
            Guid function = ApartmentParameterMappingVm.MappingModel
                .First(item => item.DataOrigin.Name == "KRT_Функциональное назначение")
                .ParameterToMatch.Guid;

            List<Room> allRooms = _roomCollector.Where(element => element.get_Parameter(function)
                    .AsString() == "МОП")
                .Cast<Room>().OrderBy(room => room.Level.Elevation).ToList();

            //Нахожу геометрический центр МОПов
            List<XYZ> mopPtsByLevel = allRooms.GroupBy(room => room.Level.Elevation)
                .Select(grp =>
                    NumberingExtention.GetGeometricCenter(
                        grp.Select(room => NumberingExtention.GetRoomCentroid(room)).ToList()
                    )
                ).ToList();
            return mopPtsByLevel;
        }

        // Марка для контроля. Скрыть при надобности после теста
        private void PlaceMark(XYZ point)
        {
            var doc = PluginSettings.Instance.RevitDoc;
            using (Transaction tr = new Transaction(doc, "test"))
            {
                tr.Start();
                FamilySymbol symbol = doc.GetElement(new ElementId(6629955)) as FamilySymbol;
                doc.Create.NewFamilyInstance(point, symbol, doc.ActiveView);
                tr.Commit();
            }
        }

        // Записать число - вернуть последнее значение
        private int CreateNumbersOutNumber(List<XYZ> points, XYZ start, XYZ finish, NumberingDirection dir, int startNumber)
        {
            XYZ mainVec = (finish - start).Normalize();
            XYZ up = XYZ.BasisZ; // Ось Z за "вертикаль", если работаем в XY-плоскости

            // Структура хранит угол и точку
            var pointsWithAngles = points.Select(p =>
            {
                XYZ vec = (p - start).Normalize();
                // Векторное произведение для определения знака угла (по/против часовой)
                XYZ cross = mainVec.CrossProduct(vec);
                double dot = mainVec.DotProduct(vec);
                double angle = 0;
                switch (dir)
                {
                    case NumberingDirection.Сlockwise:
                        angle = Math.Atan2(-cross.DotProduct(up), dot); // ПО ЧАСОВОМУ с минусом
                        break;
                    case NumberingDirection.Sunwise:
                        angle = Math.Atan2(cross.DotProduct(up), dot); // ПРОТИВ ЧАСОВОГО с плюсом
                        break;
                }
                // angle в диапазоне [-π, π], по часовой - отрицательные углы, их превращаем в положительные
                double angleCW = angle < 0 ? 2 * Math.PI + angle : angle;
                return new { Point = p, Angle = angleCW };
            }).OrderBy(pa => pa.Angle).ToList();

            var xxx = pointsWithAngles;

            Guid numGuid = ApartmentParameterMappingVm.MappingModel
            .First(item => item.DataOrigin.Name == "KRT_№ Квартиры")
            .ParameterToMatch.Guid;


            int outNumber = 0;
            for (int i = 0; i < pointsWithAngles.Count; i++)
            {
                XYZ p = pointsWithAngles[i].Point;
                int number = i + startNumber;
                ApartmentModel selectedModel = ApartmentRepository.Instance.Apartments.Where(z => z.GeomCenterPoint == p).First();

                if (selectedModel != null)
                {
                    foreach (var room in selectedModel.RoomsOfApartment)
                    {
                        room.get_Parameter(numGuid).Set(number.ToString());
                    }
                }
                outNumber = number;
            }
            return outNumber;
        }


        private void UpdateNumberingSettings()
        {
            if (_selectedConfiguration != null)
            {
                SelectedNumberingDirection =
                    EnumExtensions.GetEnumDescription(
                        _selectedConfiguration.NumberingSettings.NumberingDirection,
                        NumberingDirection.Сlockwise);
                SelectedNumberingStart =
                    EnumExtensions.GetEnumDescription(
                        _selectedConfiguration.NumberingSettings.NumberingStart,
                        NumberingStart.Top);
                InitNumber = _selectedConfiguration.NumberingSettings.InitNumber;
                IsEnumForEachLevel = _selectedConfiguration.NumberingSettings.ResetNumberForEachLevel;
                AddPrefix = _selectedConfiguration.NumberingSettings.AddPrefix;
                PrefixValue = _selectedConfiguration.NumberingSettings.FixToNumber;
                IsPrefix = _selectedConfiguration.NumberingSettings.IsPrefix;
            }
        }

        private void OnSelectedConfigurationChanged(Configuration newConfiguration)
        {
            // Обновляем локальное свойство при изменении
            SelectedConfiguration = newConfiguration;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
