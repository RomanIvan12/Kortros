using ApartmentsProject.AuxiliaryСlasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace ApartmentsProject.Models
{
    // Верхний уровень вложенности
    [XmlRoot("krtrsApartmentsProjectLayout")]
    public class ApartmentsProjectLayout
    {
        [XmlArray("Configurations")]
        [XmlArrayItem("Configuration")]
        public List<Configuration> ConfigurationList { get; set; }
    }

    // Третий уровень вложенности
    public class Configuration : INotifyPropertyChanged
    {
        private string _name;
        [XmlElement("Name")]
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        [XmlElement("Id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();


        private bool _isSelected;

        [XmlElement("IsSelected")]
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        private Settings _settings;
        [XmlElement("Settings")]
        public Settings Settings
        {
            get => _settings;
            set
            {
                if (_settings != value)
                {
                    _settings = value;
                    OnPropertyChanged(nameof(Settings));
                }
            }
        }



        [XmlElement("RoomMatrix")]
        public RoomMatrix RoomMatrix { get; set; }

        [XmlElement("ApartmentType")]
        public ApartmentType ApartmentType { get; set; }

        [XmlElement("NumberingSettings")]
        public NumberingSettings NumberingSettings { get; set; }



        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    public class Settings : INotifyPropertyChanged
    {
        [XmlElement("ClearCommonParametersBeforeCalculation")]
        public string ClearCommonParametersBeforeCalculation { get; set; }

        [XmlElement("AreaRoundType")]
        public string AreaRoundType { get; set; }

        [XmlElement("SourceAreaForFactor")]
        public string SourceAreaForFactor { get; set; }

        [XmlElement("ComputeAreaSettings")]
        public ComputeAreaSettings ComputeAreaSettings { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class NumberingSettings : INotifyPropertyChanged
    {
        [XmlElement("NumberingDirection")]
        public string NumberingDirection { get; set; }

        [XmlElement("NumberingStart")]
        public string NumberingStart { get; set; }

        [XmlElement("InitNumber")]
        public int InitNumber { get; set; }

        [XmlElement("ResetNumberForEachLevel")]
        public bool ResetNumberForEachLevel { get; set; }

        [XmlElement("AddPrefix")]
        public bool AddPrefix { get; set; }

        [XmlElement("FixToNumber")]
        public string FixToNumber { get; set; }

        [XmlElement("IsPrefix")]
        public bool IsPrefix { get; set; }



        //[XmlElement("NumerateTargetParameter")]
        //public NumerateTargetParameter NumerateTargetParameter { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    //public class NumerateTargetParameter
    //{
    //    [XmlElement("Name")]
    //    public string Name { get; set; }

    //    [XmlElement("Guid")]
    //    public string Guid { get; set; }
    //}

    public class ComputeAreaSettings
    {
        [XmlElement("LivingArea")]
        public string LivingArea { get; set; }

        [XmlElement("FlatArea")]
        public string FlatArea { get; set; }
    }


    public class RoomMatrix : INotifyPropertyChanged
    {
        [XmlArray("Entries")]
        [XmlArrayItem("RoomMatrixEntry")]
        public List<RoomMatrixEntry> Entries { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ApartmentType
    {
        [XmlArray("Entries")]
        [XmlArrayItem("ApartmentTypeEntry")]
        public List<ApartmentTypeEntry> Entries { get; set; }
    }

    public class RoomMatrixEntry : INotifyPropertyChanged
    {
        private string _name;
        [XmlElement("Name")]
        public string Name
        {
            get => _name;
            set
            {
                string newValue = value?.TrimEnd();
                if (_name != newValue)
                {
                    _name = newValue;
                    OnPropertyChanged(nameof(Name));
                    OnRoomMatrixEntryChanged();
                }
            }
        }

        private int _finishingThickness;
        [XmlElement("FinishingThickness")]
        public int FinishingThickness
        {
            get => _finishingThickness;
            set
            {
                int validateValue = Math.Max(0, value);
                if (_finishingThickness != validateValue)
                {
                    _finishingThickness = validateValue;
                    OnPropertyChanged(nameof(FinishingThickness));
                    OnRoomMatrixEntryChanged();
                }
            }
        }


        private string _roomType;
        [XmlElement("RoomType")]
        public string RoomType
        {
            get => _roomType;
            set
            {
                if (_roomType != value)
                {
                    _roomType = value;
                    OnPropertyChanged(nameof(RoomType));
                    OnRoomMatrixEntryChanged();
                }
            }
        }

        private double _areaFactor;
        [XmlElement("AreaFactor")]
        public double AreaFactor
        {
            get => _areaFactor;
            set
            {
                double validateValue = Math.Max(0, value);
                if (_areaFactor != validateValue)
                {
                    _areaFactor = validateValue;
                    OnPropertyChanged(nameof(AreaFactor));
                    OnRoomMatrixEntryChanged();
                }
            }
        }

        private int _numberPriority;
        [XmlElement("NumberPriority")]
        public int NumberPriority
        {
            get => _numberPriority;
            set
            {
                int validateValue = Math.Max(0, value);
                if (_numberPriority != validateValue)
                {
                    _numberPriority = validateValue;
                    OnPropertyChanged(nameof(NumberPriority));
                    OnRoomMatrixEntryChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        // Событие для сигнализации об изменении
        public event Action RoomMatrixEntryChanged;
        protected virtual void OnRoomMatrixEntryChanged()
        {
            RoomMatrixEntryChanged?.Invoke();
        }
    }

    public class ApartmentTypeEntry : INotifyPropertyChanged
    {
        private string _apartmentType;
        [XmlElement("ApartmentType")]
        public string ApartmentType
        {
            get => _apartmentType;
            set
            {
                if (_apartmentType != value)
                {
                    _apartmentType = value;
                    OnPropertyChanged(nameof(ApartmentType));
                    OnApartmentTypeEntryChanged();
                }
            }
        }

        private int _livingRoomCount;
        [XmlElement("LivingRoomCount")]
        public int LivingRoomCount
        {
            get => _livingRoomCount;
            set
            {
                int validateValue = Math.Max(1, value);
                if (_livingRoomCount != validateValue)
                {
                    _livingRoomCount = validateValue;
                    OnPropertyChanged(nameof(LivingRoomCount));
                    OnApartmentTypeEntryChanged();
                }
            }
        }

        private string _containRooms;
        [XmlElement("ContainRooms")]
        public string ContainRooms
        {
            get => _containRooms;
            set
            {
                if (_containRooms != value)
                {
                    _containRooms = value;
                    OnPropertyChanged(nameof(ContainRooms));
                    OnApartmentTypeEntryChanged();
                }
            }
        }
        private string _nonContainRooms;
        [XmlElement("NonContainRooms")]
        public string NonContainRooms
        {
            get => _nonContainRooms;
            set
            {
                if (_nonContainRooms != value)
                {
                    _nonContainRooms = value;
                    OnPropertyChanged(nameof(NonContainRooms));
                    OnApartmentTypeEntryChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        // Событие для сигнализации об изменении
        public event Action ApartmentTypeEntryChanged;
        protected virtual void OnApartmentTypeEntryChanged()
        {
            ApartmentTypeEntryChanged?.Invoke();
        }
    }


    public enum ClearCommonParametersBeforeCalculation
    {
        [Description("Очищать параметры")]
        ClearAll,

        [Description("Не очищать параметры (не рекоменд.)")]
        NotClear
    }

    public enum AreaRoundType
    {
        [Description("0.001")]
        Thousands,
        [Description("0.01")]
        Hundreds,
        [Description("0.1")]
        Tenth,
        [Description("1")]
        Units,
        [Description("0.005")]
        HalfHundreds,
        [Description("0.05")]
        HalfTenth,
        [Description("0.5")]
        HalfUnits
    }

    public enum SourceAreaForFactor
    {
        [Description("Округлённую площадь")]
        RoundedArea,
        [Description("Исходную площадь")]
        BaseArea
    }

    public enum LivingArea
    {
        [Description("Площадь округлённая")]
        RoundedArea,
        [Description("Площадь округлённая с коэффициентом")]
        RoundedAreaWithFactor
    }
    public enum FlatArea
    {
        [Description("Площадь округлённая")]
        RoundedArea,
        [Description("Площадь округлённая с коэффициентом")]
        RoundedAreaWithFactor
    }
    public enum RoomType
    {
        [Description("Жилое")]
        Living,
        [Description("Нежилое")]
        NonLiving
    }

    public enum NumberingDirection
    {
        [Description("По часовой стрелке")]
        Сlockwise,
        [Description("Против часовой стрелки")]
        Sunwise
    }

    public enum NumberingStart
    {
        [Description("Напротив  подъезда")]
        EntryPoint,
        [Description("Верх")]
        Top,
        [Description("Право")]
        Right,
        [Description("Низ")]
        Bottom,
        [Description("Лево")]
        Left
    }
}
