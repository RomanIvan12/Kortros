using ApartmentsProject.AuxiliaryСlasses;
using ApartmentsProject.Models;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace ApartmentsProject.ViewModel
{
    public class VerificationVm : INotifyPropertyChanged
    {

        private FilteredElementCollector _roomCollector = PluginSettings.Instance.RoomCollector;
        public static ObservableCollection<Room> Rooms { get; set; } = new ObservableCollection<Room>(); // MAIN

        public static ObservableCollection<IncorrectRoom> IncorrectRooms { get; set; } = new ObservableCollection<IncorrectRoom>();

        private IncorrectRoom _selectedRoom;
        public IncorrectRoom SelectedRoom
        {
            get => _selectedRoom;
            set
            {
                if (_selectedRoom != value)
                    _selectedRoom = value;
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
                    OnPropertyChanged(nameof(SelectedConfiguration));
                }
            }
        }

        public List<string> ListOfAllowedNames { get; set; }


        private RelayCommand _refreshRooms;
        public RelayCommand RefreshRooms
        {
            get
            {
                return _refreshRooms ??
                       (_refreshRooms = new RelayCommand(obj =>
                       {
                           Refresh();
                       }));
            }
        }

        public VerificationVm()
        {
            _selectedConfiguration = RunCommand.ApartmentsProjectLayout.ConfigurationList.First(
                i => i.IsSelected == true);
            Mediator.SelectedConfigurationChanged += OnSelectedConfigurationChanged;
            SourceDataVm.RoomMatrixEntitiesChanged += OnRoomMatrixEntitiesChanged;

            ListOfAllowedNames = SelectedConfiguration.RoomMatrix.Entries
                .Select(i => i.Name).ToList();
            EventAggregator.Instance.SubscribeAllCorrect(OnAllCorrect);

            GetRoomsForSelectedParameter();
        }

        private void OnRoomMatrixEntitiesChanged(object sender, EventArgs e)
        {
            Refresh();
        }
        private void OnAllCorrect(object sender, EventArgs e)
        {
            GetRoomsForSelectedParameter();
        }

        // Получаю помещения с заполненным значением "Квартира" в прописанном в ресурсах имени параметра
        private void GetRoomsForSelectedParameter()
        {
            Rooms.Clear();
            try
            {
                Guid function = ApartmentParameterMappingVm.MappingModel
                    .First(item => item.DataOrigin.Name == "KRT_Функциональное назначение")
                    .ParameterToMatch.Guid;
                var allRooms = _roomCollector.Where(element => element.get_Parameter(function).AsString() == "Квартира")
                    .Cast<Room>().ToList();

                foreach (var room in allRooms)
                {
                    Rooms.Add(room);
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex.Message);
            }
            IncorrectRooms.Clear();
            GetUnplacedRooms();
            GetUnboundRooms();
            GetMultipleRooms();
            FindIncorrectNamesRooms();
            DoesRoomHasBoundaries();
        }

        private void Refresh()
        {
            ListOfAllowedNames.Clear();
            //ListOfAllowedNames = SelectedConfiguration.RoomMatrix.Entries
            //    .Select(i => i.Name).ToList();
            if (SelectedConfiguration != null &&
                SelectedConfiguration.RoomMatrix != null &&
                SelectedConfiguration.RoomMatrix.Entries != null)
            {
                var entries = SelectedConfiguration.RoomMatrix.Entries;
                if (entries.Any())
                {
                    ListOfAllowedNames = entries
                        .Where(i => i != null && i.Name != null)
                        .Select(i => i.Name)
                        .ToList();
                }
                else
                {
                    ListOfAllowedNames = new List<string>();
                }
            }
            IncorrectRooms.Clear();
            GetUnplacedRooms();
            GetUnboundRooms();
            GetMultipleRooms();
            FindIncorrectNamesRooms();
            DoesRoomHasBoundaries();
        }

        // 1st:
        private void GetUnplacedRooms()
        {
            foreach (var room in Rooms)
            {               
                if (room.Location == null && room.Area == 0)
                {
                    var newIncorrectRoom = new IncorrectRoom(room)
                    {
                        ErrorMsg = "Помещение не размещено",
                        ErrorStatus = "Warning"
                    };
                    IncorrectRooms.Add(newIncorrectRoom);
                }
            }
        }

        // 2d: Помещение размещено, но его не окружают стены - Error
        private void GetUnboundRooms()
        {
            foreach (var room in Rooms)
            {
                bool isSolidEmpty = false;
                foreach (var geomElement in room.ClosedShell)
                {
                    if (geomElement is Solid)
                    {
                        if ((geomElement as Solid).Volume == 0)
                        {
                            isSolidEmpty = true;
                            break;
                        }
                    }
                }
                if (room.Location != null && room.Area == 0 && isSolidEmpty == true)
                {
                    var newIncorrectRoom = new IncorrectRoom(room)
                    {
                        ErrorMsg = "Помещение размещено, но не окружено",
                        ErrorStatus = "Error"
                    };
                    IncorrectRooms.Add(newIncorrectRoom);
                }
            }
        }

        // 3d: 
        private void GetMultipleRooms()
        {
            foreach (var room in Rooms)
            {
                bool isSolidEmpty = true;
                foreach (var geomElement in room.ClosedShell)
                {
                    if (geomElement is Solid)
                    {
                        if ((geomElement as Solid).Volume != 0)
                        {
                            isSolidEmpty = false;
                            break;
                        }
                    }
                }
                if (room.Location != null && room.Area == 0 && isSolidEmpty == false)
                {
                    var newIncorrectRoom = new IncorrectRoom(room)
                    {
                        ErrorMsg = "Помещение избыточно",
                        ErrorStatus = "Error"
                    };
                    IncorrectRooms.Add(newIncorrectRoom);
                }
            }
        }

        private void FindIncorrectNamesRooms()
        {
            foreach (var room in Rooms)
            {
                string name = room.get_Parameter(BuiltInParameter.ROOM_NAME).AsString();
                if (!ListOfAllowedNames.Contains(name))
                {
                    var newIncorrectRoom = new IncorrectRoom(room)
                    {
                        ErrorMsg = "Помещения с таким именем нет в матрице помещений",
                        ErrorStatus = "Error"
                    };
                    IncorrectRooms.Add(newIncorrectRoom);
                }
            }
        }

        private void DoesRoomHasBoundaries()
        {
            foreach (var room in Rooms)
            {
                IList<IList<BoundarySegment>> boundaries = room.GetBoundarySegments(new SpatialElementBoundaryOptions());
                if (boundaries == null || boundaries.Count == 0)
                {
                    var newIncorrectRoom = new IncorrectRoom(room)
                    {
                        ErrorMsg = "Ошибка с границами",
                        ErrorStatus = "Error"
                    };
                    IncorrectRooms.Add(newIncorrectRoom);
                }
            }
        }

        private void OnSelectedConfigurationChanged(Configuration newConfiguration)
        {
            // Обновляем локальное свойство при изменении
            SelectedConfiguration = newConfiguration;
            //ListOfAllowedNames = SelectedConfiguration.RoomMatrix.Entries
            //    .Select(i => i.Name).ToList();
            if (SelectedConfiguration != null &&
                SelectedConfiguration.RoomMatrix != null &&
                SelectedConfiguration.RoomMatrix.Entries != null)
            {
                var entries = SelectedConfiguration.RoomMatrix.Entries;
                if (entries.Any())
                {
                    ListOfAllowedNames = entries
                        .Where(i => i != null && i.Name != null)
                        .Select(i => i.Name)
                        .ToList();
                }
                else
                {
                    ListOfAllowedNames = new List<string>();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class IncorrectRoom
    {
        public Room Room { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string LevelName { get; set; }
        public string ErrorMsg { get; set; }
        public string ErrorStatus { get; set; }

        public IncorrectRoom(Room room)
        {
            Room = room;
            Id = room.Id.IntegerValue;
            Name = room.get_Parameter(BuiltInParameter.ROOM_NAME).AsString();
            LevelName = room.Level.Name;
        }
    }
}
    /// Относится к помещениям с заполненным значением "Квартира"
    /// 1. Помещение не размещено - Warning
    /// 2. Помещение размещено, но его не окружают стены - Error
    /// 3. Несколько помещений в одном контуре - Error
    /// 4. Имя помещения нет в матрице помещений