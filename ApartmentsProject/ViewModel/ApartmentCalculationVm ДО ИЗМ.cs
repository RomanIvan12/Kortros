using ApartmentsProject.ViewModel.Utilities;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ApartmentsProject.Models;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using ApartmentsProject.AuxiliaryСlasses;
using System.Windows;

namespace ApartmentsProject.ViewModel
{
    public partial class ApartmentCalculationVm
    {
        public FilteredElementCollector RoomCollector;

        private readonly ExternalEventHandler _externalEventHandler;
        private readonly ExternalEvent _externalEvent;



        public static ObservableCollection<Room> Rooms { get; set; } = new ObservableCollection<Room>(); // MAIN
        public static ObservableCollection<Room> UnplacedRooms { get; set; } = new ObservableCollection<Room>();
        public static ObservableCollection<InvalidRoom> InvalidRooms { get; set; } = new ObservableCollection<InvalidRoom>();


        private string _labelValue;
        public string LabelValue
        {
            get => _labelValue;
            set => SetProperty(ref _labelValue, value);
        }

        public ApartmentCalculationVm()
        {
            _externalEventHandler = new ExternalEventHandler();
            _externalEvent = ExternalEvent.Create(_externalEventHandler);

            //
            var settings = PluginSettings.Instance;
            settings.RevitDoc = RunCommand.Doc;
            settings.AreCommonParametersMapped = false;

            LoadData();
        }

        private void LoadData()
        {
            RoomCollector = new FilteredElementCollector(PluginSettings.Instance.RevitDoc).OfCategory(BuiltInCategory.OST_Rooms);
            GetRoomsForSelectedParameter();

        }
        
        // Получаю помещения с заполненным значением "Квартира" в прописанном в ресурсах имени параметра
        private void GetRoomsForSelectedParameter()
        {
            StringBuilder sb = new StringBuilder();
            Rooms.Clear();
            var allRooms = RoomCollector.Where(element => element.LookupParameter(Properties.Resources.ParameterOfGroup)
                    .AsString() == "Квартира")
                .Cast<Room>().ToList();
            foreach (var room in allRooms)
            {
                Rooms.Add(room);
            }
            sb.AppendLine($"Количество помещений квартир, отфильтрованных по параметру {Properties.Resources.ParameterOfGroup}: {Rooms.Count.ToString()}");
            sb.AppendLine($"____________");

            GetRoomsUnplaced();
            
            if (UnplacedRooms.Count > 0)
            {
                sb.AppendLine($"Количество неразмещённых помещений: {UnplacedRooms.Count.ToString()}");
                sb.AppendLine("Эти помещения будут проигнорированы при расчете");
            }

            _labelValue = sb.ToString();
        }


        private void GetRoomsUnplaced()
        {
            UnplacedRooms.Clear();
            foreach (var room in Rooms)
            {
                if (room.Location == null || room.Area == 0)
                    UnplacedRooms.Add(room);
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                OnPropertyChanged(propertyName);
            }
        }
    }
}
