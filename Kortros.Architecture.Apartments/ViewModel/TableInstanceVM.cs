using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Kortros.Architecture.Apartments.Commands;
using Kortros.Architecture.Apartments.Model;
using Kortros.Architecture.Apartments.Utilities;
using Kortros.Architecture.Apartments.ViewModel.Commands;
using log4net;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;

namespace Kortros.Architecture.Apartments.ViewModel
{
    public class TableInstanceVM : INotifyPropertyChanged
    {
        private static readonly ILog _logger = LogManager.GetLogger("ZoneCalculation");

        private static Document _doc;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        #region Флажок скрытия ГНС корпуса
        private bool hideGNS;
        public bool HideGNS
        {
            get { return hideGNS; }
            set
            {
                hideGNS = value;
                OnPropertyChanged(nameof(HideGNS));
            }
        }
        #endregion
        #region Флажок скрытия коэффициентов
        private bool hideKoff;
        public bool HideKoff
        {
            get { return hideKoff; }
            set 
            { 
                hideKoff = value; 
                OnPropertyChanged(nameof(HideKoff));
            }
        }
        #endregion
        #region Флажок добавления таблиц
        private bool addTableView;
        public bool AddTableView
        {
            get { return addTableView; }
            set 
            { 
                addTableView = value;
                OnPropertyChanged(nameof(AddTableView));
            }
        }
        #endregion
        #region TextBox с состоянием заполнения исх данных
        private static string label1Text;
        public string Label1Text
        {
            get { return label1Text; }
            set 
            {
                label1Text = value;
                OnPropertyChanged(nameof(Label1Text));
            }
        }
        #endregion
        #region Выбранный вариант из списка

        private string selectedVar;
        public string SelectedVar
        {
            get { return selectedVar; }
            set 
            {
                selectedVar = value; 
                OnPropertyChanged(nameof(SelectedVar));
                ZoneVNS = RevitFunc.CheckVns(_doc, tableInstance, selectedVar);
                ZoneGNS1 = RevitFunc.CheckGns1(_doc, tableInstance, selectedVar);
                ZoneGNS2 = RevitFunc.CheckGns2(_doc, tableInstance, selectedVar);
                SetVariant();
                tableInstance.Rooms = RefreshRooms();
            }
        }
        #endregion
        #region Статус зоны ВНС
        private ItemZone zoneVNS;
        public ItemZone ZoneVNS
        {
            get { return zoneVNS; }
            set 
            { 
                zoneVNS = value; 
                OnPropertyChanged(nameof(ZoneVNS));
            }
        }
        #endregion
        #region Статус зоны ГНС1
        private ItemZone zoneGNS1;
        public ItemZone ZoneGNS1
        {
            get { return zoneGNS1; }
            set 
            { 
                zoneGNS1 = value;
                OnPropertyChanged(nameof(ZoneGNS1));
            }
        }
        #endregion
        #region Статус зоны ГНС2
        private ItemZone zoneGNS2;
        public ItemZone ZoneGNS2
        {
            get { return zoneGNS2; }
            set 
            { 
                zoneGNS2 = value;
                OnPropertyChanged(nameof(ZoneGNS2));
            }
        }
        #endregion
        #region Доступен ли Комбобокс (варианты)
        private bool isComboBoxEnabled;

        public bool IsComboBoxEnabled
        {
            get { return isComboBoxEnabled; }
            set
            { 
                isComboBoxEnabled = value; 
                OnPropertyChanged(nameof(IsComboBoxEnabled));
            }
        }


        #endregion


        public ObservableCollection<string> Variants { get; set; }

        // КОМАНДЫ
        public OKButtonCommand OKButtonCommand { get; set; }
        public CancelCommand CancelCommand { get; set; }


        public static TableInstance tableInstance { get; set; }

        public static TableInstanceVM VM { get; set; }

        public TableInstanceVM()
        {
            _doc = RunPlugin.Document;
            tableInstance = new TableInstance()
            {
                Rooms = RevitFunc.GetRoomsInLevel(_doc, RunPlugin.Level),
                Level = RunPlugin.Level.Name,
            };
            
            OKButtonCommand = new OKButtonCommand(this);
            CancelCommand = new CancelCommand(this);

            Variants = new ObservableCollection<string>();
            VM = this;
            CheckExistedCommonParameters(
                _doc,
                new Dictionary<string, string> () 
                {
                    { "ADSK_Номер квартиры", "10fb72de-237e-4b9c-915b-8849b8907695" }
                });
            CheckVariantAvailabilityAndSet(tableInstance);
            if (string.IsNullOrWhiteSpace(tableInstance.Version))
            {
                ZoneVNS = RevitFunc.CheckVns(_doc, tableInstance);
                ZoneGNS1 = RevitFunc.CheckGns1(_doc, tableInstance);
                ZoneGNS2 = RevitFunc.CheckGns2(_doc, tableInstance);
            }
        }


        // Проверяет, есть ли в помещениях на этаже заполнение поля Комментарий по шаблону "Вар НН" или "Вариант НН"
        private bool CheckVariantAvailabilityAndSet(TableInstance tabInstance)
        {
            List<string> comments = new List<string>();
            foreach (Room room in tabInstance.Rooms)
            {
                string comment = room.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString();

                string pattern = @"^(Вар(\s+)?(иант)?\s+)?0*([1-9]\d*)$";
                Regex regex = new Regex(pattern);
                Match match = regex.Match(comment);


                if (!comments.Contains(comment) && match.Success)
                    comments.Add(comment);
            }
            if (comments.Count > 0)
            {
                label1Text += "Выберите вариант";
                tabInstance.IsVersionsAvailable = true;
                foreach (string version in comments)
                {
                    Variants.Add(version);
                }
                IsComboBoxEnabled = true;
                return true;
            }
            else
            {
                label1Text += "Нет доступных вариантов";
                tabInstance.IsVersionsAvailable = false;
                IsComboBoxEnabled = false;
                return false;
            }
        }

        //Проверка наличия параметров
        public static void CheckExistedCommonParameters(Document doc, Dictionary<string, string> parameterSet)
        {
            //  Dictionary <NAME, GUID>
            DefinitionBindingMapIterator mapIterator = doc.ParameterBindings.ForwardIterator();

            StringBuilder sb = new StringBuilder();

            // перебираю параметры
            List<string> namesInternalDefinitions = new List<string>();

            while (mapIterator.MoveNext())
            {
                InternalDefinition internalDefinition = (InternalDefinition)mapIterator.Key;
                namesInternalDefinitions.Add(internalDefinition.Name);

                if (parameterSet.ContainsKey(internalDefinition.Name))
                {
                    //TODO: Проверяю, что он инстанс и назначен помещению
                    ElementBinding elementBinding = mapIterator.Current as ElementBinding;
                    CategorySet categorySet = elementBinding.Categories;

                    if (mapIterator.Current is InstanceBinding && categorySet.Contains(Category.GetCategory(doc, BuiltInCategory.OST_Rooms)))
                    {
                        label1Text = internalDefinition.Name + " есть в списке параметров - OK" + "\n\n";
                        continue;
                    }
                    else
                    {
                        //TODO: Обработать ошибку
                        label1Text = internalDefinition.Name + " есть в списке параметров, но он либо относится к типу, либо не содержит категории Помещения - ERROR" + "\n";
                    }
                }
            }
            foreach (string item in parameterSet.Keys)
            {
                if (!namesInternalDefinitions.Contains(item))
                {
                    label1Text = item + " ОТСУТСТВУЕТ - ERROR" + "\n";
                }
            }
        }

        // Запись варианта, если он присутствует
        public void SetVariant()
        {
            if (tableInstance.IsVersionsAvailable)
            {
                tableInstance.Version = selectedVar.ToString();
            }
        }

        // Получение S общая квартир на этаже (м2)
        public static void SetLivingArea(Document doc, TableInstance tableInstance)
        {
            tableInstance.AreaOfLivingSpace = 0;

            ForgeTypeId displayUnitsArea = doc.GetUnits().GetFormatOptions(SpecTypeId.Area).GetUnitTypeId();
            double sum = 0;

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
            tableInstance.AreaOfLivingSpace = sum;
        }

        // Получение S общая на этаже (м2)
        public static void SetArea(Document doc, TableInstance tableInstance)
        {
            tableInstance.AreaOfSpace = 0;

            ForgeTypeId displayUnitsArea = doc.GetUnits().GetFormatOptions(SpecTypeId.Area).GetUnitTypeId();
            double sum = 0;

            foreach (Room room in tableInstance.Rooms)
            {
                double AreaOfApartments = room.get_Parameter(BuiltInParameter.ROOM_AREA).AsDouble();
                double ConvArea = UnitUtils.ConvertFromInternalUnits(AreaOfApartments, displayUnitsArea); // часть первого значения
                sum += Math.Round(ConvArea, 1);
            }

            tableInstance.AreaOfSpace = sum;
        }

        // Получение S VNS
        public static void SetAreasZone(Document doc, TableInstance tableInstance)
        {
            ForgeTypeId displayUnitsArea = doc.GetUnits().GetFormatOptions(SpecTypeId.Area).GetUnitTypeId();

            List<Element> areas = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Areas).WhereElementIsNotElementType().ToList();
            foreach (Element element in areas)
            {
                Area area = element as Area;
                string levelName = element.get_Parameter(BuiltInParameter.LEVEL_NAME).AsString();
                string schemeName = area.AreaScheme.Name;
                string name = element.get_Parameter(BuiltInParameter.ROOM_NAME).AsString();
                if (levelName == tableInstance.Level &&
                    tableInstance.IsAreaVNSCorrect &&
                    schemeName.Contains("ВНС") &&
                    name.Contains(tableInstance.Version))
                {
                    double Area = area.get_Parameter(BuiltInParameter.ROOM_AREA).AsDouble();
                    tableInstance.AreaVNS = UnitUtils.ConvertFromInternalUnits(Area, displayUnitsArea);
                }
                if (levelName == tableInstance.Level &&
                    tableInstance.IsAreaGNS1Correct &&
                    schemeName.Contains("Всего") &&
                    name.Contains(tableInstance.Version))
                {
                    double Area = area.get_Parameter(BuiltInParameter.ROOM_AREA).AsDouble();
                    tableInstance.AreaGNS1 = UnitUtils.ConvertFromInternalUnits(Area, displayUnitsArea);
                }
                if (levelName == tableInstance.Level &&
                    tableInstance.IsAreaGNS2Correct &&
                    schemeName.Contains("370") &&
                    name.Contains(tableInstance.Version))
                {
                    double Area = area.get_Parameter(BuiltInParameter.ROOM_AREA).AsDouble();
                    tableInstance.AreaGNS2 = UnitUtils.ConvertFromInternalUnits(Area, displayUnitsArea);
                }
            }
        }

        /// <summary>
        /// Обновляет список помещений, если выбран вариант
        /// </summary>
        /// <param name="rooms"></param>
        /// <returns></returns>
        private List<Room> RefreshRooms()
        {
            //tableInstance.Rooms.Clear();
            List<Room> newRooms = new List<Room>();
            if (tableInstance.IsVersionsAvailable)
            {
                foreach (Room room in RevitFunc.GetRoomsInLevel(_doc, RunPlugin.Level))
                {
                    string comment = room.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString();
                    if (comment == tableInstance.Version)
                        newRooms.Add(room);
                }
            }
            return newRooms;
        }

    }
    public class ItemZone
    {
        public string ZoneName { get; set; }
        public string Status { get; set; }
    }

}
