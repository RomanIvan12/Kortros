using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Kortros.Architecture.Apartments.Commands;
using Kortros.Architecture.Apartments.Model;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;

namespace Kortros.Architecture.Apartments.ViewModel
{
    public class TableInstanceVM_ИСХ : INotifyPropertyChanged
    {
        public TableInstance tableInstance;


        private bool _isLevelCheched;
        public bool IsLevelCheched
        {
            get { return _isLevelCheched; }
            set 
            {
                if (_isLevelCheched != value)
                {
                    _isLevelCheched = value;
                    OnPropertyChanged(nameof(IsLevelCheched));

                    // Off 2d rbutton
                    if (IsLevelCheched)
                        IsAdskChecked = false;
                }
            }
        }

        private bool _isAdskChecked;
        public bool IsAdskChecked
        {
            get { return _isAdskChecked; }
            set 
            { 
                if( _isAdskChecked != value)
                {
                    _isAdskChecked = value;
                    OnPropertyChanged(nameof(IsAdskChecked));

                    // Off 1st rbutton
                    if (IsAdskChecked )
                        IsLevelCheched = false;
                }
            }
        }

        private string selectedAdskValue;

        public string SelectedAdskValue
        {
            get { return selectedAdskValue; }
            set 
            { 
                selectedAdskValue = value;
                OnPropertyChanged(nameof(SelectedAdskValue));
            }
        }

        private string selectedVariant;

        public string SelectedVariant
        {
            get { return selectedVariant; }
            set
            {
                selectedVariant = value;
                OnPropertyChanged(nameof(SelectedVariant));
            }
        }



        public ObservableCollection<string> Variants { get; set; }
        public ObservableCollection<string> AdskLevels { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand ToggleLevelRadioButtonCommand { get; }
        public ICommand ToggleAdskRadioButtonCommand { get; }

        public TableInstanceVM()
        {
            this.tableInstance = new TableInstance()
            {
                Rooms = GetRoomsInLevel(),
                Level = ExternalCommand3.Level.Name,
            };
            tableInstance.IsVersionAvailable = CheckVariantAvailability();

            ToggleLevelRadioButtonCommand = new DelegateCommand(ToggleLevelButton);
            ToggleAdskRadioButtonCommand = new DelegateCommand(ToggleAdskButton);

            Variants = new ObservableCollection<string>();
            //GetRoomsInAdsk();

            if (tableInstance.IsVersionAvailable == true)
            {
                SetVariantsAvailable();
            }
        }

        private void ToggleLevelButton()
        {
            IsLevelCheched = true;
        }
        private void ToggleAdskButton()
        {
            IsAdskChecked = true;
        }

        private List<Room> GetRoomsInLevel()
        {
            return new FilteredElementCollector(ExternalCommand3.Document).OfCategory(BuiltInCategory.OST_Rooms).ToElements()
                .Where(room => room.LevelId == ExternalCommand3.Level.Id
                && (room as SpatialElement).Area > 0).Select(item => item as Room).ToList();
        }
        private void GetRoomsInAdsk()
        {
            List<Element> allRooms = new FilteredElementCollector(ExternalCommand3.Document).OfCategory(BuiltInCategory.OST_Rooms).ToElements()
                .Where(room => (room as SpatialElement).Area > 0).ToList();
            foreach (var room in allRooms)
            {
                string adsk = room.get_Parameter(new Guid("9eabf56c-a6cd-4b5c-a9d0-e9223e19ea3f")).AsString();
                if (!AdskLevels.Contains(adsk))
                    AdskLevels.Add(adsk);
            }
        }

        private bool CheckVariantAvailability()
        {
            List<string> comments = new List<string>();
            foreach (Room room in tableInstance.Rooms)
            {
                string comment = room.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString();
                if(!comments.Contains(comment) && comment.StartsWith("Вар"))
                    comments.Add(comment);
            }
            if (comments.Count > 0)
                return true;
            return false;
        }

        private void SetVariantsAvailable()
        {
            List<string> variants = new List<string>();
            foreach (Room room in tableInstance.Rooms)
            {
                string comment = room.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString();
                if (!variants.Contains(comment) && comment.StartsWith("Вар"))
                    variants.Add(comment);
            }
            foreach (string var in variants)
            {
                Variants.Add(var);
            }
        }


        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}



public static void SetArea(TableInstance tableInstance)
{
    tableInstance.AreaOfSpace = 0;


    ForgeTypeId displayUnitsArea = _doc.GetUnits().GetFormatOptions(SpecTypeId.Area).GetUnitTypeId();
    double sum = 0;

    //if (tableInstance.IsVersionsAvailable)
    //{
    //    foreach (Room room in tableInstance.Rooms)
    //    {
    //        string comment = room.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString();
    //        if (comment != null && comment == tableInstance.Version)
    //        {
    //            double AreaOfApartments = room.get_Parameter(BuiltInParameter.ROOM_AREA).AsDouble();
    //            double ConvArea = UnitUtils.ConvertFromInternalUnits(AreaOfApartments, displayUnitsArea); // часть первого значения
    //            sum += Math.Round(ConvArea, 1);
    //        }
    //    }
    //}
    //else
    //{
    foreach (Room room in tableInstance.Rooms)
    {

        double AreaOfApartments = room.get_Parameter(BuiltInParameter.ROOM_AREA).AsDouble();
        double ConvArea = UnitUtils.ConvertFromInternalUnits(AreaOfApartments, displayUnitsArea); // часть первого значения
        sum += Math.Round(ConvArea, 1);
    }
    //}
    tableInstance.AreaOfSpace = sum;
}

public static void SetLivingArea(TableInstance tableInstance)
{
    tableInstance.AreaOfLivingSpace = 0;

    ForgeTypeId displayUnitsArea = _doc.GetUnits().GetFormatOptions(SpecTypeId.Area).GetUnitTypeId();
    double sum = 0;

    if (tableInstance.IsVersionsAvailable)
    {
        foreach (Room room in tableInstance.Rooms)
        {
            string comment = room.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString();
            string adskNomer = room.get_Parameter(new Guid("10fb72de-237e-4b9c-915b-8849b8907695")).AsString();
            if (!string.IsNullOrWhiteSpace(adskNomer) && comment != null && comment == tableInstance.Version)
            {
                double AreaOfApartments = room.get_Parameter(BuiltInParameter.ROOM_AREA).AsDouble();
                double ConvArea = UnitUtils.ConvertFromInternalUnits(AreaOfApartments, displayUnitsArea); // часть первого значения
                sum += Math.Round(ConvArea, 1);
            }
        }
    }
    else
    {
        foreach (Room room in tableInstance.Rooms)
        {
            string adskNomer = room.get_Parameter(new Guid("10fb72de-237e-4b9c-915b-8849b8907695")).AsString();
            if (!string.IsNullOrWhiteSpace(adskNomer))
            {
                double AreaOfApartments = room.get_Parameter(BuiltInParameter.ROOM_AREA).AsDouble();
                double ConvArea = UnitUtils.ConvertFromInternalUnits(AreaOfApartments, displayUnitsArea); // часть первого значения
                sum += Math.Round(ConvArea, 1);
            }
        }
    }
    tableInstance.AreaOfLivingSpace = sum;
}