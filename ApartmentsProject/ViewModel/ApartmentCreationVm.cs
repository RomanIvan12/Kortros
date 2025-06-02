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

namespace ApartmentsProject.ViewModel
{
    public class ApartmentCreationVm : INotifyPropertyChanged
    {
        public List<Room> AllRooms { get; set; } = new List<Room>();
        public List<Level> AllLevel { get; set; } = new List<Level>();


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

        //public static ObservableCollection<ApartmentModel> ApartmentsOfObservableCollection { get; set; } = new ObservableCollection<ApartmentModel>();

        public event PropertyChangedEventHandler PropertyChanged;

        private string _clearRoomsPick;
        public string ClearRoomsPick
        {
            get => _clearRoomsPick;
            set => SetProperty(ref _clearRoomsPick, value);
        }

        private string _viewOrProjectPick;
        public string ViewOrProjectPick
        {
            get => _viewOrProjectPick;
            set => SetProperty(ref _viewOrProjectPick, value);
        }
        public static ObservableCollection<string> ClearRoomsCollection { get; set; } = new ObservableCollection<string>
        {
            "Очищать параметры",
            "Не очищать параметры (не рекоменд.)"
        };

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


        private RelayCommand _mainCalculation;



        private readonly ExternalEventHandler _externalEventHandler;
        private readonly ExternalEvent _externalEvent;

        public ApartmentCreationVm()
        {
            _externalEventHandler = new ExternalEventHandler();
            _externalEvent = ExternalEvent.Create(_externalEventHandler);

            ViewOrProjectPick = "На открытом виде";
        }

        public RelayCommand MainCalculationCommand
        {
            get
            {
                return _mainCalculation ??
                       (_mainCalculation = new RelayCommand(obj =>
                       {
                           AllRooms.Clear();
                           AllLevel.Clear();
                           GetRooms();
                           try
                           {
                               _externalEventHandler.SetAction(Calculate);
                               _externalEvent.Raise();
                           }
                           catch (Exception ex)
                           {
                               MessageBox.Show($"ошибка загрузки параметров: {ex.Message}");
                           }
                       }));
            }
        }


        private void Calculate()
        {
            //GetRooms();
            switch (ViewOrProjectPick)
            {
                case "На открытом виде":
                    // Get currently opened view
                    var activeView = RunCommand.Doc.ActiveView;
                    if (activeView is View3D || activeView is ViewSection || activeView is ViewSheet || activeView is ViewDrafting)
                    {
                        MessageBox.Show("Откройте план этажа");
                        break;
                    }
                    else
                    {
                        var roomsInView = new FilteredElementCollector(RunCommand.Doc, activeView.Id)
                            .OfCategory(BuiltInCategory.OST_Rooms).Cast<Room>().ToList();
                        List<Room> roomsView = new List<Room>(); // Нужные помещения
                        foreach (var room in roomsInView)
                        {
                            List<int> RoomsId = ApartmentCalculationVm.Rooms.Select(r => r.Id.IntegerValue).ToList();
                            if (RoomsId.Contains(room.Id.IntegerValue))
                                roomsView.Add(room);
                        }
                        if (roomsView.Count > 0)
                        {
                            AllRooms = roomsView;
                            AllLevel.Add(roomsView.First().Level);
                        }
                    }
                    break;

                case "Во всём проекте":
                    AllLevel = new FilteredElementCollector(RunCommand.Doc).OfClass(typeof(Level)).ToElements().Cast<Level>().ToList();
                    break;
            }
            
            switch (ClearRoomsPick)
            {
                case "Очищать параметры":
                    using (Transaction t = new Transaction(RunCommand.Doc, "Clear Parameters"))
                    {
                        t.Start();

                        foreach (var room in AllRooms)
                        {
                            foreach (var model in ApartmentParameterMappingVm.MappingModel)
                            {
                                var guid = model.ParameterToMatch.Guid;
                                //room.get_Parameter(guid).Set("");
                                try
                                {
                                    room.get_Parameter(guid).Set("");
                                }
                                catch { }

                                //room.get_Parameter(guid).Set(0);
                                try
                                {
                                    room.get_Parameter(guid).Set(0);
                                }
                                catch { }
                            }
                        }

                        t.Commit();
                    }
                    break;

                case "Не очищать параметры (не рекоменд.)":
                    break;
            }

            // Собираем квартиры
            Dictionary<Room, Solid> dictOfRoomsSolids = new Dictionary<Room, Solid>();
            foreach (Room room in AllRooms)
            {
                var el = (Element)room;
                var pair = GeometryStackUtils.GetRoomSolid(RunCommand.Doc, el);
                dictOfRoomsSolids.Add(pair.Key, pair.Value);
            }

            // Создал стаки для Разд Линий
            var geometryStacksOfSeparator = GeometryStackUtils.CreateGeomStackOfSeparator(RunCommand.Doc, dictOfRoomsSolids);

            List<Element> doorList = new List<Element>();

            foreach (Level level in AllLevel)
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

        /// <summary>
        /// логика заполнения некоторых параметров после нажатия кнопки "Собрать"
        /// 
        /// исходные данные - список квартир (точнее их ID)
        /// заполняемые параметры:
        /// 
        /// KRT_ID - заполняется функцией на кнопке "Собрать"
        /// 
        /// KRT_Тип помещения - берётся их таблицы матрицы в зависимости от имени
        /// KRT_Число жилых комнат - сумма помещений в квартире с типом "Жилое"
        /// KRT_Коэффициент площади - берётся из матрицы
        /// KRT_Толщина черновой отделки - берётся из матрицы
        /// </summary>


        public void GetRooms()
        {
            if (ApartmentCalculationVm.UnplacedRooms.Count != 0
                || ApartmentCalculationVm.InvalidRooms.Count != 0)
            {
                throw new InvalidOperationException();
            }
            else
            {
                foreach (var room in ApartmentCalculationVm.Rooms)
                {
                    AllRooms.Add(room);
                }
            }
        }


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
    }
}
