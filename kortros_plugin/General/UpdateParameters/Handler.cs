using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Kortros.General.UpdateParameters.Properties;
using System.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Kortros.Utilities;
using System.Diagnostics;
using Kortros.General.UpdateParameters.Data;
using System.Collections.ObjectModel;

namespace Kortros.General.UpdateParameters
{
    public class Handler
    {
        private readonly UIApplication _uiapp;
        private readonly Document _doc;

        // Для ElementID
        private List<BuiltInCategory> catElementId = new List<BuiltInCategory> {
            BuiltInCategory.OST_DuctAccessory,
            BuiltInCategory.OST_DuctSystem,
            BuiltInCategory.OST_DuctCurves,
            BuiltInCategory.OST_DuctLinings,
            BuiltInCategory.OST_DuctInsulations,
            BuiltInCategory.OST_DuctTerminal,
            BuiltInCategory.OST_FlexDuctCurves,
            BuiltInCategory.OST_DuctFitting,

            BuiltInCategory.OST_PipeAccessory,
            BuiltInCategory.OST_PipingSystem,
            BuiltInCategory.OST_PipeCurves,
            BuiltInCategory.OST_PipeInsulations,
            BuiltInCategory.OST_FlexPipeCurves,
            BuiltInCategory.OST_PipeFitting,
            BuiltInCategory.OST_PlumbingFixtures,
            BuiltInCategory.OST_Sprinklers,

            BuiltInCategory.OST_GenericModel,
            BuiltInCategory.OST_MechanicalEquipment,
            BuiltInCategory.OST_ElectricalEquipment,

            BuiltInCategory.OST_CableTray,
            BuiltInCategory.OST_CableTrayFitting,
            BuiltInCategory.OST_Conduit,
            BuiltInCategory.OST_ConduitFitting,

            BuiltInCategory.OST_ElectricalFixtures,
            BuiltInCategory.OST_LightingFixtures,
            BuiltInCategory.OST_DataDevices,
            BuiltInCategory.OST_FireAlarmDevices,
            BuiltInCategory.OST_LightingDevices,
            BuiltInCategory.OST_CommunicationDevices,
            BuiltInCategory.OST_SecurityDevices,
            BuiltInCategory.OST_NurseCallDevices,

            BuiltInCategory.OST_CurtaSystem, // Витражные системы
            BuiltInCategory.OST_Doors,
            BuiltInCategory.OST_Roads, // Дорожки
            BuiltInCategory.OST_CurtainWallMullions, // Импосты витража
            BuiltInCategory.OST_Columns, // Колонны
            BuiltInCategory.OST_ImportObjectStyles, // Комплекты мебели
            BuiltInCategory.OST_Roofs,
            BuiltInCategory.OST_Stairs, // Лестницы
            //BuiltInCategory.OST_Materials, // Материалы
            BuiltInCategory.OST_gbXML_SlabOnGrade, // Мебель
            BuiltInCategory.OST_MedicalEquipment, // Медицинское оборудование
            BuiltInCategory.OST_FoodServiceEquipment, // Оборуд. для предприятий общ.пита
            BuiltInCategory.OST_StairsRailing, // Ограждения
            BuiltInCategory.OST_MechanicalEquipment, // Озеленение
            BuiltInCategory.OST_Windows,
            BuiltInCategory.OST_Ramps, // Пандус
            BuiltInCategory.OST_CurtainWallPanels, // Панель витража
            BuiltInCategory.OST_Parking, // Парковка
            BuiltInCategory.OST_Floors, // Перекрытия
            BuiltInCategory.OST_PlumbingFixtures, // Сантехнические приборы
            BuiltInCategory.OST_FireProtection, // Системы противопожарной оммун
            BuiltInCategory.OST_SpecialityEquipment, // Специальное оборудование
            BuiltInCategory.OST_Walls, // Стены
            BuiltInCategory.OST_TelephoneDevices, // Топография
            BuiltInCategory.OST_Casework, // Шкафы


            BuiltInCategory.OST_StructuralFramingSystem, // Балочные системы
            BuiltInCategory.OST_StructuralFraming, // Каркас несущий
            BuiltInCategory.OST_Rebar, // Несущая арматура
            BuiltInCategory.OST_StructuralColumns, // Несущие колонны
            BuiltInCategory.OST_FabricAreas, // Обл. раскладки арматурных сеток 
            BuiltInCategory.OST_IsolatedFoundationAnalytical, // Фундамент несущей констр
        };
        //public List<Category> CatElementId = catElementId.Select(bc => Category.GetCategory(_doc, bc))
        //                                                 .ToList();

        //Общий
        private Dictionary<BuiltInCategory, string> dictCategoriesIzmer = new Dictionary<BuiltInCategory, string>
        {
            { BuiltInCategory.OST_DuctInsulations, "м²" },
            { BuiltInCategory.OST_PipeAccessory, "шт." },
            { BuiltInCategory.OST_DuctAccessory, "шт." },
            { BuiltInCategory.OST_MechanicalEquipment, "шт." },
            { BuiltInCategory.OST_PlumbingFixtures, "шт." },
            { BuiltInCategory.OST_PipeFitting, "шт." },
            { BuiltInCategory.OST_DuctFitting, "шт." },
            { BuiltInCategory.OST_DuctTerminal, "шт." },
            { BuiltInCategory.OST_Sprinklers, "шт." },
            { BuiltInCategory.OST_DuctCurves, "м" },
            { BuiltInCategory.OST_FlexDuctCurves, "м" },
            { BuiltInCategory.OST_PipeCurves, "м" },
            { BuiltInCategory.OST_FlexPipeCurves, "м" }
        };

        private Dictionary<BuiltInCategory, string> dictCategoriesKol = new Dictionary<BuiltInCategory, string>
        {
            { BuiltInCategory.OST_DuctInsulations, "площадь основы" },
            { BuiltInCategory.OST_PipeAccessory, "один" },
            { BuiltInCategory.OST_DuctAccessory, "один" },
            { BuiltInCategory.OST_MechanicalEquipment, "один" },
            { BuiltInCategory.OST_PlumbingFixtures, "один" },
            { BuiltInCategory.OST_DuctFitting, "один" },
            { BuiltInCategory.OST_PipeFitting, "один" },
            { BuiltInCategory.OST_DuctTerminal, "один" },
            { BuiltInCategory.OST_Sprinklers, "один" },
            { BuiltInCategory.OST_DuctCurves, "длина" },
            { BuiltInCategory.OST_FlexDuctCurves, "длина" },
            { BuiltInCategory.OST_PipeCurves, "длина" },
            { BuiltInCategory.OST_FlexPipeCurves, "длина" },

        };


        private HashSet<int> updatedElements = new HashSet<int>();


        private bool ParameterSelectedElementId => Config.SelectedElementId;
        private bool ParameterSelectedGroup => Config.SelectedGroup;
        private bool ParameterSelectedIzmer => Config.SelectedIzmer;
        private bool ParameterSelectedKolichestvo => Config.SelectedKolichestvo;



        public ObservableCollection<DataItem> DataItemList { get; set; }

        public Handler(UIApplication uiapp)
        {
            this._uiapp = uiapp;
            _doc = uiapp.ActiveUIDocument.Document;
            DataItemList = new ObservableCollection<DataItem>();
        }


        public DataItem SetElementId(Element element)
        {
            DataItem item = new DataItem()
            {
                ElementId = element.Id.IntegerValue,
                ElementName = element.get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM).AsValueString(),
                ParameterName = "Element_ID",
            };
            item.OldValue = element.get_Parameter(UpdateParameter.Default.ParameterElementId).AsString();

            var bc = (BuiltInCategory)element.Category.Id.IntegerValue;

            if (catElementId.Contains(bc) && element.GroupId.IntegerValue == -1)
            {
                if ((string)item.OldValue != element.Id.IntegerValue.ToString())
                {
                    element.get_Parameter(UpdateParameter.Default.ParameterElementId).Set(element.Id.IntegerValue.ToString());
                    item.NewValue = element.Id.IntegerValue.ToString();
                    item.Status = ItemStatus.Done;
                }
                else
                {
                    item.Status = ItemStatus.Cancelled;
                    item.Msg = "Исходное значение корректоно";
                }
            }
            return item;
        }

        public DataItem SetGroupMEP(Element element)
        {
            DataItem item = new DataItem()
            {
                ElementId = element.Id.IntegerValue,
                ElementName = element.get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM).AsValueString(),
                ParameterName = "KRTRS_Группирование",
            };
            item.OldValue = element.get_Parameter(UpdateParameter.Default.ParameterGroup).AsString();

            if (element.GroupId.IntegerValue == -1)
            {
                if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_MechanicalEquipment
                || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_GenericModel
                || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Sprinklers
                || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_ElectricalEquipment
                || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PlumbingFixtures
                || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_FireAlarmDevices
                || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_SecurityDevices
                || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_NurseCallDevices)
                {
                    if ((string)item.OldValue != "0.Оборудование/Обобщ.модели/Спринклеры")
                    {
                        element.get_Parameter(UpdateParameter.Default.ParameterGroup).Set("0.Оборудование/Обобщ.модели/Спринклеры");
                        item.NewValue = "0.Оборудование/Обобщ.модели/Спринклеры";
                        item.Status = ItemStatus.Done;
                    }
                    else
                    {
                        item.Status = ItemStatus.Cancelled;
                        item.Msg = "Исходное значение корректоно";
                    }   
                }

                else if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DuctAccessory
                    || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeAccessory)
                {

                    if ((string)item.OldValue != "1.Арматура воздуховодов/трубопроводов")
                    {
                        element.get_Parameter(UpdateParameter.Default.ParameterGroup).Set("1.Арматура воздуховодов/трубопроводов");
                        item.NewValue = "1.Арматура воздуховодов/трубопроводов";
                        item.Status = ItemStatus.Done;
                    }
                    else
                    {
                        item.Status = ItemStatus.Cancelled;
                        item.Msg = "Исходное значение корректоно";
                    }   
                }

                else if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeCurves
                    || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DuctCurves
                    || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_FlexDuctCurves
                    || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_FlexPipeCurves
                    || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_CableTray
                    || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Conduit)
                {
                    if ((string)item.OldValue != "2.Воздуховоды/Трубы/Кабельные лотки/Короба")
                    {
                        element.get_Parameter(UpdateParameter.Default.ParameterGroup).Set("2.Воздуховоды/Трубы/Кабельные лотки/Короба");
                        item.NewValue = "2.Воздуховоды/Трубы/Кабельные лотки/Короба";
                        item.Status = ItemStatus.Done;
                    }
                    else
                    {
                        item.Status = ItemStatus.Cancelled;
                        item.Msg = "Исходное значение корректоно";
                    }
                }

                else if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DuctTerminal
                    || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_LightingDevices
                    || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_LightingFixtures
                    || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_ElectricalFixtures)
                {

                    if ((string)item.OldValue != "3.Воздухораспределители/Эл.Приборы")
                    {
                        element.get_Parameter(UpdateParameter.Default.ParameterGroup).Set("3.Воздухораспределители/Эл.Приборы");
                        item.NewValue = "3.Воздухораспределители/Эл.Приборы";
                        item.Status = ItemStatus.Done;
                    }
                    else
                    {
                        item.Status = ItemStatus.Cancelled;
                        item.Msg = "Исходное значение корректоно";
                    }
                }

                else if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DuctInsulations
                    || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeInsulations
                    || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DuctLinings)
                {
                    if ((string)item.OldValue != "4.Материалы изоляции")
                    {
                        element.get_Parameter(UpdateParameter.Default.ParameterGroup).Set("4.Материалы изоляции");
                        item.NewValue = "4.Материалы изоляции";
                        item.Status = ItemStatus.Done;
                    }
                    else
                    {
                        item.Status = ItemStatus.Cancelled;
                        item.Msg = "Исходное значение корректоно";
                    }
                }
                else if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DuctFitting
                    || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting
                    || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_CableTrayFitting
                    || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_ConduitFitting)
                {
                    if ((string)item.OldValue != "5.Соеденительные детали воздуховодов/трубопроводов/кабельных лотков")
                    {
                        element.get_Parameter(UpdateParameter.Default.ParameterGroup).Set("5.Соеденительные детали воздуховодов/трубопроводов/кабельных лотков");
                        item.NewValue = "5.Соеденительные детали воздуховодов/трубопроводов/кабельных лотков";
                        item.Status = ItemStatus.Done;
                    }
                    else
                    {
                        item.Status = ItemStatus.Cancelled;
                        item.Msg = "Исходное значение корректоно";
                    }
                }
            }
            return item;
        }

        public DataItem SetGroupEOM(Element element)
        {
            DataItem item = new DataItem()
            {
                ElementId = element.Id.IntegerValue,
                ElementName = element.get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM).AsValueString(),
                ParameterName = "KRTRS_Группирование",
            };
            item.OldValue = element.get_Parameter(UpdateParameter.Default.ParameterGroup).AsString();

            if (element.GroupId.IntegerValue == -1)
            {
                if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_MechanicalEquipment
                || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_ElectricalEquipment)
                {
                    if ((string)item.OldValue != "1.Электрооборудование")
                    {
                        element.get_Parameter(UpdateParameter.Default.ParameterGroup).Set("1.Электрооборудование");
                        item.NewValue = "1.Электрооборудование";
                        item.Status = ItemStatus.Done;
                    }
                    else
                    {
                        item.Status = ItemStatus.Cancelled;
                        item.Msg = "Исходное значение корректоно";
                    }
                }

                else if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_LightingFixtures)
                {
                    if ((string)item.OldValue != "2.Осветительные приборы")
                    {
                        element.get_Parameter(UpdateParameter.Default.ParameterGroup).Set("2.Осветительные приборы");
                        item.NewValue = "2.Осветительные приборы";
                        item.Status = ItemStatus.Done;
                    }
                    else
                    {
                        item.Status = ItemStatus.Cancelled;
                        item.Msg = "Исходное значение корректоно";
                    }
                }
                else if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_CableTray
                    || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Conduit
                    || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_CableTrayFitting
                    || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_ConduitFitting)
                {
                    if ((string)item.OldValue != "3.Кабеленесущие системы")
                    {
                        element.get_Parameter(UpdateParameter.Default.ParameterGroup).Set("3.Кабеленесущие системы");
                        item.NewValue = "3.Кабеленесущие системы";
                        item.Status = ItemStatus.Done;
                    }
                    else
                    {
                        item.Status = ItemStatus.Cancelled;
                        item.Msg = "Исходное значение корректоно";
                    }
                }

                else if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_LightingDevices
                    || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DataDevices
                    || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_ElectricalFixtures)
                {
                    if ((string)item.OldValue != "4.Электроустановочные изделия")
                    {
                        element.get_Parameter(UpdateParameter.Default.ParameterGroup).Set("4.Электроустановочные изделия");
                        item.NewValue = "4.Электроустановочные изделия";
                        item.Status = ItemStatus.Done;
                    }
                    else
                    {
                        item.Status = ItemStatus.Cancelled;
                        item.Msg = "Исходное значение корректоно";
                    } 
                }
                else if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_ElectricalCircuit)
                {
                    if ((string)item.OldValue != "5.Кабельная продукция")
                    {
                        element.get_Parameter(UpdateParameter.Default.ParameterGroup).Set("5.Кабельная продукция");
                        item.NewValue = "5.Кабельная продукция";
                        item.Status = ItemStatus.Done;
                    }
                    else
                    {
                        item.Status = ItemStatus.Cancelled;
                        item.Msg = "Исходное значение корректоно";
                    }
                    
                }
                else if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_GenericModel)
                {
                    if ((string)item.OldValue != "6.Прочие")
                    {
                        element.get_Parameter(UpdateParameter.Default.ParameterGroup).Set("6.Прочие");
                        item.NewValue = "6.Прочие";
                        item.Status = ItemStatus.Done;
                    }
                    else
                    {
                        item.Status = ItemStatus.Cancelled;
                        item.Msg = "Исходное значение корректоно";
                    }
                }
            }
            return item;
        }

        public DataItem SetIzmer(Element element)
        {
            DataItem item = new DataItem()
            {
                ElementId = element.Id.IntegerValue,
                ElementName = element.get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM).AsValueString(),
                ParameterName = "KRTRS_Единица измерения",
            };
            var bc = (BuiltInCategory)element.Category.Id.IntegerValue;

            item.OldValue = element.get_Parameter(UpdateParameter.Default.ParameterIzmer).AsString();
            
            if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeInsulations)
            {
                InsulationLiningBase lineIns = element as InsulationLiningBase;
                Element hostElement = _doc.GetElement(lineIns.HostElementId);
                Parameter diameter = hostElement.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM);

                double convDiam = UnitUtils.ConvertFromInternalUnits(
                    diameter.AsDouble(),
                    _doc.GetUnits()
                    .GetFormatOptions(SpecTypeId.Area)
                    .GetUnitTypeId());


                if (convDiam <= 25)
                {
                    if (item.OldValue.ToString() != "м")
                    {
                        element.get_Parameter(UpdateParameter.Default.ParameterIzmer).Set("м");
                        item.NewValue = "м";
                        item.Status = ItemStatus.Done;
                    }
                    else
                    {
                        item.Status = ItemStatus.Cancelled;
                        item.Msg = "Исходное значение корректоно";
                    }
                }
                else
                {
                    if (item.OldValue.ToString() != "м²")
                    {
                        element.get_Parameter(UpdateParameter.Default.ParameterIzmer).Set("м²");
                        item.NewValue = "м²";
                        item.Status = ItemStatus.Done;
                    }
                    else
                    {
                        item.Status = ItemStatus.Cancelled;
                        item.Msg = "Исходное значение корректоно";
                    }
                }
            }
            else
            {
                if (dictCategoriesIzmer.ContainsKey(bc))
                {
                    if (item.OldValue.ToString() != dictCategoriesIzmer[bc])
                    {
                        element.get_Parameter(UpdateParameter.Default.ParameterIzmer).Set(dictCategoriesIzmer[bc]);
                        item.NewValue = dictCategoriesIzmer[bc];
                        item.Status = ItemStatus.Done;
                    }
                    else
                    {
                        item.Status = ItemStatus.Cancelled;
                        item.Msg = "Исходное значение корректоно";
                    }
                }
            }
            return item;
        }

        public DataItem SetKolichestvo(Element element)
        {
            DataItem item = new DataItem()
            {
                ElementId = element.Id.IntegerValue,
                ElementName = element.get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM).AsValueString(),
                ParameterName = "KRTRS_Количество",
            };

            item.OldValue = element.get_Parameter(UpdateParameter.Default.ParameterIzmer).AsDouble();

            var bc = (BuiltInCategory)element.Category.Id.IntegerValue;
            ForgeTypeId displayUnitsLength = _doc.GetUnits().GetFormatOptions(SpecTypeId.Length).GetUnitTypeId();
            ForgeTypeId displayUnitsArea = _doc.GetUnits().GetFormatOptions(SpecTypeId.Area).GetUnitTypeId();

            if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeInsulations)
            {
                //"площадь основы ИЛИ п.м."
                InsulationLiningBase lineIns = element as InsulationLiningBase;
                Element hostElem = _doc.GetElement(lineIns.HostElementId);

                if (hostElem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeCurves)
                {
                    double pipeDiam = hostElem.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).AsDouble();
                    var convDiam = UnitUtils.ConvertFromInternalUnits(pipeDiam, displayUnitsLength);

                    double zapasValue = 1;
                    if (UtilFunctions.GetGlobalParameterDoubleValueByName(_doc, "Запас изоляции") != null)
                        zapasValue = (double)UtilFunctions.GetGlobalParameterDoubleValueByName(_doc, "Запас изоляции");

                    if (convDiam > 26)
                    {
                        //m2
                        Parameter AreaIns = element.get_Parameter(BuiltInParameter.RBS_CURVE_SURFACE_AREA);
                        double ConvArea = UnitUtils.ConvertFromInternalUnits(AreaIns.AsDouble(), displayUnitsArea);
                        
                        if ((double)item.OldValue != (ConvArea * zapasValue))
                        {
                            element.get_Parameter(UpdateParameter.Default.ParameterKolichestvo).Set(ConvArea * zapasValue);
                            item.NewValue = ConvArea * zapasValue;
                            item.Status = ItemStatus.Done;
                        }
                        else
                        {
                            item.Status = ItemStatus.Cancelled;
                            item.Msg = "Исходное значение корректоно";
                        }
                    }
                    else
                    {
                        //mp
                        Parameter lengthIns = element.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH);
                        double ConvLength = UnitUtils.ConvertFromInternalUnits(lengthIns.AsDouble(), displayUnitsLength);
                        element.get_Parameter(UpdateParameter.Default.ParameterKolichestvo).Set(ConvLength * zapasValue / 1000);

                        if ((double)item.OldValue != (ConvLength * zapasValue))
                        {
                            element.get_Parameter(UpdateParameter.Default.ParameterKolichestvo).Set(ConvLength * zapasValue);
                            item.NewValue = ConvLength * zapasValue;
                            item.Status = ItemStatus.Done;
                        }
                        else
                        {
                            item.Status = ItemStatus.Cancelled;
                            item.Msg = "Исходное значение корректоно";
                        }
                    }
                }
            }
            else
            {
                if (dictCategoriesKol[bc] == "один")
                {
                    if ((double)item.OldValue != 1)
                    {
                        element.get_Parameter(UpdateParameter.Default.ParameterKolichestvo).Set(1);
                        item.NewValue = 1;
                        item.Status = ItemStatus.Done;
                    }
                    else
                    {
                        item.Status = ItemStatus.Cancelled;
                        item.Msg = "Исходное значение корректоно";
                    }
                }
                else if (dictCategoriesKol[bc] == "длина")
                {
                    double zapasValue = 1;
                    if (UtilFunctions.GetGlobalParameterDoubleValueByName(_doc, "Запас") != null)
                        zapasValue = (double)UtilFunctions.GetGlobalParameterDoubleValueByName(_doc, "Запас");
                    double LengthValue = element.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble();
                    double ConvValue = UnitUtils.ConvertFromInternalUnits(LengthValue, displayUnitsLength);

                    if ((double)item.OldValue != (ConvValue * zapasValue / 1000))
                    {
                        element.get_Parameter(UpdateParameter.Default.ParameterKolichestvo).Set(ConvValue * zapasValue / 1000);
                        item.NewValue = ConvValue * zapasValue / 1000;
                        item.Status = ItemStatus.Done;
                    }
                    else
                    {
                        item.Status = ItemStatus.Cancelled;
                        item.Msg = "Исходное значение корректоно";
                    }                   
                }
                else if (dictCategoriesKol[bc] == "площадь основы")
                {
                    InsulationLiningBase lineIns = element as InsulationLiningBase;
                    Element hostElement = _doc.GetElement(lineIns.HostElementId);

                    if (hostElement.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DuctCurves)
                    {
                        double zapasValue = 1;
                        if (UtilFunctions.GetGlobalParameterDoubleValueByName(_doc, "Запас изоляции") != null)
                            zapasValue = (double)UtilFunctions.GetGlobalParameterDoubleValueByName(_doc, "Запас");
                        Parameter areaIns = element.get_Parameter(BuiltInParameter.RBS_CURVE_SURFACE_AREA);
                        double ConvArea = UnitUtils.ConvertFromInternalUnits(areaIns.AsDouble(), displayUnitsArea);
                        if ((double)item.OldValue != (ConvArea * zapasValue))
                        {
                            element.get_Parameter(UpdateParameter.Default.ParameterKolichestvo).Set(ConvArea * zapasValue);
                            item.NewValue = ConvArea * zapasValue / 1000;
                            item.Status = ItemStatus.Done;
                        }
                        else
                        {
                            item.Status = ItemStatus.Cancelled;
                            item.Msg = "Исходное значение корректоно";
                        }
                    }
                }
            }

            return item;
        }


        public void Run(bool updateOnlySelected)
        {
            updatedElements.Clear();

            Stopwatch sw = new Stopwatch();
            sw.Start();
            TimeSpan time;

            using (Transaction t = new Transaction(_doc, "SetParameter"))
            {
                t.Start();

                var elements = CollectElements(!updateOnlySelected);

                foreach (Element e in elements)
                {
                    
                    if (ParameterSelectedElementId)
                    {
                        try
                        {
                            DataItem dataItem = SetElementId(e);
                            DataItemList.Add(dataItem);
                        }
                        catch { }
                    }
                    if (ParameterSelectedGroup)
                    {
                        try
                        {
                            if (_doc.Title.Contains("_EOM") || _doc.Title.Contains("_ЕОМ"))
                            {
                                DataItem dataItem = SetGroupEOM(e);
                                DataItemList.Add(dataItem);
                            }
                            if (_doc.Title.Contains("_OV") || _doc.Title.Contains("_ОВ") || _doc.Title.Contains("_VK") || _doc.Title.Contains("_ВК"))
                            {
                                DataItem dataItem = SetGroupMEP(e);
                                DataItemList.Add(dataItem);
                            }
                        }
                        catch { }
                    }
                    if (ParameterSelectedKolichestvo)
                    {
                        try
                        {
                            DataItem dataItem = SetKolichestvo(e);
                            DataItemList.Add(dataItem);
                        }
                        catch { }
                    }
                    if (ParameterSelectedIzmer)
                    {
                        try
                        {
                            //SetIzmer(e);
                            DataItem dataItem = SetIzmer(e);
                            DataItemList.Add(dataItem);
                        }
                        catch { }
                    }
                    updatedElements.Add(e.Id.IntegerValue);
                }
                t.Commit();
                sw.Stop();
                time = sw.Elapsed;
                Console.WriteLine($"Время выполнения - {time}");
            }
        }

        private FilteredElementCollector CollectElements(bool all)
        {
            FilteredElementCollector collector = Common.Collector.Create(_uiapp, all);

            collector.WhereElementIsNotElementType();

            ElementMulticategoryFilter catFilter = new ElementMulticategoryFilter(catElementId);
            collector.WherePasses(catFilter);

            return collector;
        }
    }
    public class DataItem
    {
        public int ElementId { get; set; }
        public string ElementName { get; set; }
        public string ParameterName { get; set; }
        public object OldValue { get; set; }
        public object NewValue { get; set; }
        public ItemStatus Status { get; set; }
        public string Msg { get; set; }
    }
    public enum ItemStatus
    {
        Done,
        Cancelled,
        NotCompleted,
        Error
    }
}