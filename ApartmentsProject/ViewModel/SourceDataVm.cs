using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using ApartmentsProject.Models;
using ApartmentsProject.AuxiliaryСlasses;
using System.Collections.Specialized;
using Autodesk.Revit.DB.Architecture;
using ApartmentsProject.ViewModel.Commands;
using System.Text;
using System.Reflection;

namespace ApartmentsProject.ViewModel
{
    public partial class SourceDataVm : INotifyPropertyChanged
    {
        // Комбобок жилое-нежило
        public ObservableCollection<string> SelectedRoomTypeOptions { get; }

        private bool _isAddFromRoomsEnabled;
        public bool IsAddFromRoomsEnabled
        {
            get => _isAddFromRoomsEnabled;
            set 
            { 
                _isAddFromRoomsEnabled = value;
                OnPropertyChanged(nameof(IsAddFromRoomsEnabled));
            }
        }

        private ObservableCollection<string> _roomNames;
        public ObservableCollection<string> RoomNames 
        { 
            get => _roomNames;
            set
            {
                if (_roomNames != value)
                {
                    _roomNames = value;
                    OnPropertyChanged(nameof(SelectedRoomType));
                }
            }
        }

        private string _infoText;
        public string InfoText
        {
            get => _infoText;
            set 
            { 
                _infoText = value;
                OnPropertyChanged(nameof(InfoText));
            }
        }

        private string _isOk;
        public string IsOk
        {
            get { return _isOk; }
            set 
            { 
                _isOk = value;
                OnPropertyChanged(nameof(IsOk));
            }
        }

        private string _selectedRoomType;
        public string SelectedRoomType
        {
            get => _selectedRoomType;
            set
            {
                if (_selectedRoomType != value)
                {
                    _selectedRoomType = value;
                    _selectedRoomMatrixEntry.RoomType =
                        EnumExtensions
                        .GetEnumValueDescription<RoomType>(_selectedRoomType);

                    OnPropertyChanged(nameof(SelectedRoomType));
                    ConfigurationService.Instance.SaveConfiguration(RunCommand.ApartmentsProjectLayout);
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
                    OnPropertyChanged(nameof(SelectedConfiguration));
                    GetRoomMatrixEntries();
                    GetApartmentTypeEntries();
                }
            }
        }

        private static ObservableCollection<RoomMatrixEntry> _roomMatrixEnties;
        public ObservableCollection<RoomMatrixEntry> RoomMatrixEnties
        {
            get => _roomMatrixEnties;
            set
            {
                if (_roomMatrixEnties != value)
                {
                    if (_roomMatrixEnties != null)
                    {
                        foreach (var entry in _roomMatrixEnties)
                            entry.RoomMatrixEntryChanged -= OnRoomMatrixEntryChanged;

                        _roomMatrixEnties.CollectionChanged -= OnRoomMatrixEntriesChanged;
                    }
                    _roomMatrixEnties = value;

                    if (_roomMatrixEnties != null)
                    {
                        // Подписаться на изменения в новой коллекции
                        foreach (var entry in _roomMatrixEnties)
                            entry.RoomMatrixEntryChanged += OnRoomMatrixEntryChanged;

                        _roomMatrixEnties.CollectionChanged += OnRoomMatrixEntriesChanged;
                    }
                    
                    OnPropertyChanged(nameof(RoomMatrixEnties));
                    ConfigurationService.Instance.SaveConfiguration(RunCommand.ApartmentsProjectLayout);
                }
            }
        }

        private RoomMatrixEntry _selectedRoomMatrixEntry;
        public RoomMatrixEntry SelectedRoomMatrixEntry
        {
            get => _selectedRoomMatrixEntry;
            set
            {
                if (_selectedRoomMatrixEntry != value)
                {
                    if (_selectedRoomMatrixEntry != null)
                        _selectedRoomMatrixEntry.PropertyChanged -= RoomMatrixEntry_PropertyChanged;

                    _selectedRoomMatrixEntry = value;

                    if (_selectedRoomMatrixEntry != null)
                        _selectedRoomMatrixEntry.PropertyChanged += RoomMatrixEntry_PropertyChanged;

                    OnPropertyChanged(nameof(SelectedRoomMatrixEntry));
                    RaiseRoomMatrixEntitiesChanged();
                }
            }
        }

        private RelayCommand _addRoomMatrixEntryFromRooms;
        private RelayCommand _plusIntCommand;
        private RelayCommand _minusIntCommand;

        public AddRoomMatrixItem AddRoomMatrixItem { get; set; }
        public DeleteRoomMatrixItem DeleteRoomMatrixItem { get; set; }

        public SourceDataVm()
        {
            AddRoomMatrixItem = new AddRoomMatrixItem(this);
            DeleteRoomMatrixItem = new DeleteRoomMatrixItem(this);
            AddApartmentTypeEntry = new AddApartmentTypeEntry(this);
            DeleteApartmentTypeEntry = new DeleteApartmentTypeEntry(this);

            RoomNames = new ObservableCollection<string>();
            RoomMatrixEnties = new ObservableCollection<RoomMatrixEntry>();
            RoomMatrixEnties.CollectionChanged += OnRoomMatrixEntriesChanged;

            ApartmentTypeEntries = new ObservableCollection<ApartmentTypeEntry>();
            ApartmentTypeEntries.CollectionChanged += OnApartmentTypeEntriesChanged;

            _selectedConfiguration = RunCommand.ApartmentsProjectLayout.ConfigurationList.First(
                i => i.IsSelected == true);
            Mediator.SelectedConfigurationChanged += OnSelectedConfigurationChanged;

            GetRoomMatrixEntries();
            GetApartmentTypeEntries();

            SelectedRoomTypeOptions = new ObservableCollection<string>(
                Enum.GetValues(typeof(RoomType))
                .Cast<RoomType>()
                .Select(x => x.GetDescription())
                );

            // ЭТОТ БЛОК НАДО УПРОСТИТЬ
            var allRooms = new FilteredElementCollector(PluginSettings.Instance.RevitDoc).OfCategory(BuiltInCategory.OST_Rooms)
                .Where(element => element.LookupParameter(Properties.Resources.ParameterOfGroup)
                    .AsString() == "Квартира").Cast<Room>().ToList();

            foreach (var roomName in allRooms.Select(room => room.get_Parameter(BuiltInParameter.ROOM_NAME)
                .AsString()).Distinct().ToList())
            {
                RoomNames.Add(roomName);
            }
            CheckRoomsToAdd();
        }
        private void OnRoomMatrixEntriesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (RoomMatrixEntry newItem in e.NewItems)
                    newItem.RoomMatrixEntryChanged += OnRoomMatrixEntryChanged;    
            if (e.OldItems != null)
                foreach (RoomMatrixEntry oldItem in e.OldItems)
                    oldItem.RoomMatrixEntryChanged -= OnRoomMatrixEntryChanged;

            CheckRoomsToAdd();
            CheckOk();
            RaiseRoomMatrixEntitiesChanged();
        }

        private void RoomMatrixEntry_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Уведомляем, что SelectedRoomMatrixEntry изменился (любое его внутреннее поле)
            CheckOk();
            OnPropertyChanged(nameof(SelectedRoomMatrixEntry));
        }


        private void GetRoomMatrixEntries()
        {
            RoomMatrixEnties.Clear();
            if (_selectedConfiguration.RoomMatrix.Entries != null)
            {
                foreach (var singleEntry in _selectedConfiguration.RoomMatrix.Entries)
                {
                    RoomMatrixEnties.Add(singleEntry);
                }
            }
        }

        public void AddRoomMatrix()
        {
            var newRoomMatrix = new RoomMatrixEntry()
            {
                Name = "New Matrix Item",
                FinishingThickness = 0,
                //AreaFactor = 1,
                //NumberPriority = 1
            };

            newRoomMatrix.RoomMatrixEntryChanged += OnRoomMatrixEntryChanged;

            RoomMatrixEnties.Add(newRoomMatrix);

            RunCommand.ApartmentsProjectLayout.ConfigurationList.First(
                i => i.IsSelected == true)
                .RoomMatrix.Entries = RoomMatrixEnties.ToList();
            ConfigurationService.Instance.SaveConfiguration(RunCommand.ApartmentsProjectLayout);
        }

        public void DeleteRoomMatrix()
        {
            if (SelectedRoomMatrixEntry != null)
            {
                RoomMatrixEnties.Remove(SelectedRoomMatrixEntry);
                RunCommand.ApartmentsProjectLayout.ConfigurationList.First(
                    i => i.IsSelected == true)
                    .RoomMatrix.Entries = RoomMatrixEnties.ToList();
                ConfigurationService.Instance.SaveConfiguration(RunCommand.ApartmentsProjectLayout);
            }
        }

        public RelayCommand AddRoomMatrixEntryFromRooms
        {
            get
            {
                return _addRoomMatrixEntryFromRooms ??
                       (_addRoomMatrixEntryFromRooms = new RelayCommand(obj =>
                       {
                           RoomMatrixEntryFromRooms();
                       },
                       canExecute: obj =>
                       {
                           return IsAddFromRoomsEnabled;
                       }
                       
                       ));
            }
        }

        public RelayCommand PlusIntCommand
        {
            get
            {
                return _plusIntCommand ??
                       (_plusIntCommand = new RelayCommand(obj =>
                       {
                           if (SelectedRoomMatrixEntry != null)
                               ++SelectedRoomMatrixEntry.NumberPriority;
                           if (SelectedApartmentTypeEntry != null)
                               ++SelectedApartmentTypeEntry.LivingRoomCount;
                       }));
            }
        }
        public RelayCommand MinusIntCommand
        {
            get
            {
                return _minusIntCommand ??
                       (_minusIntCommand = new RelayCommand(obj =>
                       {
                           if (SelectedRoomMatrixEntry != null)
                               --SelectedRoomMatrixEntry.NumberPriority;
                           if (SelectedApartmentTypeEntry != null)
                               --SelectedApartmentTypeEntry.LivingRoomCount;
                       }));
            }
        }

        private void RoomMatrixEntryFromRooms()
        {
            List<Room> listOfRooms = VerificationVm.Rooms.ToList();

            foreach (var room in listOfRooms)
            {
                List<string> namesOfMatrix = RoomMatrixEnties.Select(i => i.Name).ToList();
                string roomName = room.get_Parameter(BuiltInParameter.ROOM_NAME).AsString();
                if (!namesOfMatrix.Contains(roomName))
                {
                    var newRoomMatrix = new RoomMatrixEntry()
                    {
                        Name = roomName,
                        FinishingThickness = 0,
                    };
                    if (roomName.Equals("спальня", StringComparison.OrdinalIgnoreCase) ||
                        roomName.Equals("гостиная", StringComparison.OrdinalIgnoreCase) ||
                        roomName.Equals("жилая комната", StringComparison.OrdinalIgnoreCase))
                    {
                        newRoomMatrix.RoomType = EnumExtensions.GetDescription(RoomType.Living);
                    }

                    newRoomMatrix.RoomMatrixEntryChanged += OnRoomMatrixEntryChanged;

                    RoomMatrixEnties.Add(newRoomMatrix);

                    RunCommand.ApartmentsProjectLayout.ConfigurationList.First(
                        i => i.IsSelected == true)
                        .RoomMatrix.Entries = RoomMatrixEnties.ToList();
                    ConfigurationService.Instance.SaveConfiguration(RunCommand.ApartmentsProjectLayout);
                }
            }
        }

        private void CheckRoomsToAdd()
        {
            List<string> namesOfMatrix = RoomMatrixEnties.Select(i => i.Name).ToList(); // Имена из матрицы
            List<string> roomsNotInMatrix = new List<string>();

            foreach (var roomName in RoomNames)
            {
                if (!namesOfMatrix.Contains(roomName))
                {
                    roomsNotInMatrix.Add(roomName);
                }
            }
            if (roomsNotInMatrix.Count == 0)
                _isAddFromRoomsEnabled = false;
            else
                _isAddFromRoomsEnabled = true;
            if (roomsNotInMatrix.Count < 5)
            {
                InfoText = $"В матрице отсутствуют строки ({roomsNotInMatrix.Count} шт.), соответствующие помещениям: {string.Join(", ", roomsNotInMatrix)}";
            }
            else
                InfoText = "МНОГА";
        }

        public static bool HasAnyNullProperty(object obj)
        {
            if (obj == null) return true;

            var type = obj.GetType();
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            return properties.Any(prop => prop.CanRead && prop.GetValue(obj) == null);
        }

        private void CheckOk()
        {
            IsOk = "OK";
            foreach (var entry in RoomMatrixEnties)
            {
                bool hasNull = HasAnyNullProperty(entry);

                if (hasNull)
                {
                    IsOk = "NEOK";
                    break;
                }
            }
        }

        public static event EventHandler RoomMatrixEntitiesChanged;
        private void RaiseRoomMatrixEntitiesChanged()
        {
            RoomMatrixEntitiesChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnRoomMatrixEntryChanged()
        {
            //RaiseRoomMatrixEntitiesChanged();
            // Сохраняем конфигурацию при изменении элемента
            ConfigurationService.Instance.SaveConfiguration(RunCommand.ApartmentsProjectLayout);
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
