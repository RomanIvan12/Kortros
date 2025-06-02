using ApartmentsProject.AuxiliaryСlasses;
using ApartmentsProject.Models;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApartmentsProject.ViewModel
{
    public class VerificationVm : INotifyPropertyChanged
    {
        
        public FilteredElementCollector RoomCollector;
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

            RoomCollector = new FilteredElementCollector(PluginSettings.Instance.RevitDoc).OfCategory(BuiltInCategory.OST_Rooms);

            GetRoomsForSelectedParameter();
        }

        private void OnRoomMatrixEntitiesChanged(object sender, EventArgs e)
        {
            Refresh();
        }


        // Получаю помещения с заполненным значением "Квартира" в прописанном в ресурсах имени параметра
        private void GetRoomsForSelectedParameter()
        {
            Rooms.Clear();
            var allRooms = RoomCollector.Where(element => element.LookupParameter(Properties.Resources.ParameterOfGroup)
                    .AsString() == "Квартира")
                .Cast<Room>().ToList();
            foreach (var room in allRooms)
            {
                Rooms.Add(room);
            }
            IncorrectRooms.Clear();
            GetUnplacedRooms();
            GetUnboundRooms();
            GetMultipleRooms();
            FindIncorrectNamesRooms();
        }

        private void Refresh()
        {
            ListOfAllowedNames.Clear();
            ListOfAllowedNames = SelectedConfiguration.RoomMatrix.Entries
                .Select(i => i.Name).ToList();

            IncorrectRooms.Clear();
            GetUnplacedRooms();
            GetUnboundRooms();
            GetMultipleRooms();
            FindIncorrectNamesRooms();
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

        // 2d: Помещение размещено, но его не окружают стены - Error
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


        private void OnSelectedConfigurationChanged(Configuration newConfiguration)
        {
            // Обновляем локальное свойство при изменении
            SelectedConfiguration = newConfiguration;
            ListOfAllowedNames = SelectedConfiguration.RoomMatrix.Entries
                .Select(i => i.Name).ToList();
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
    /// Относится к помещениям с заполненным значением "Квартира"
    /// 1. Помещение не размещено - Warning
    /// 2. Помещение размещено, но его не окружают стены - Error
    /// 3. Несколько помещений в одном контуре - Error
    /// 4. Имя помещения нет в матрице помещений
    /// 
    
}
