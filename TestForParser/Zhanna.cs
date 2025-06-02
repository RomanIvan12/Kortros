using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TestForParser
{
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class Zhanna : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            var ParElemsShared = new FilteredElementCollector(doc).OfClass(typeof(SharedParameterElement));

            List<Element> fams = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_GenericAnnotation).WhereElementIsElementType().ToList();
            List<Element> rooms = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Rooms).WhereElementIsNotElementType().ToList();
            List<Element> areas = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Areas).WhereElementIsNotElementType().ToList();
            
            ForgeTypeId displayUnitsArea = doc.GetUnits().GetFormatOptions(SpecTypeId.Area).GetUnitTypeId();

            using (Transaction t = new Transaction(doc, "333"))
            {
                t.Start();
                foreach (Element element in fams)
                {
                    if (element.Name.Contains("Тип"))
                    {
                        string variant = string.Concat(element.Name.Where(char.IsNumber));

                        List<double> Snaetazhe = new List<double>(); 
                
                        List<double> Sobshaya = new List<double>();
                
                        List<double> Sgvns1Sum = new List<double>();             

                        List<double> Sgvns2Sum = new List<double>();

                        foreach (Element room in rooms)
                        {
                            string comment = room.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString();

                            string adskNomer = room.get_Parameter(new Guid("10fb72de-237e-4b9c-915b-8849b8907695")).AsString();

                            if (comment != null && comment.Contains(variant) && !string.IsNullOrWhiteSpace(adskNomer))
                            {
                                double AreaOfApartments = room.get_Parameter(BuiltInParameter.ROOM_AREA).AsDouble();
                                double ConvArea = UnitUtils.ConvertFromInternalUnits(AreaOfApartments, displayUnitsArea); // часть первого значения
                                Snaetazhe.Add(Math.Round(ConvArea, 1));
                            }

                            if (comment != null && comment.Contains(variant))
                            {
                                double AreaOfApartments = room.get_Parameter(BuiltInParameter.ROOM_AREA).AsDouble();
                                double ConvArea = UnitUtils.ConvertFromInternalUnits(AreaOfApartments, displayUnitsArea); // часть второго значения
                                Sobshaya.Add(Math.Round(ConvArea,1));
                            }
                        }

                        foreach (Element area in areas)
                        {
                            Area xx = area as Area;
                            string schemeName = xx.AreaScheme.Name;
                            string name = area.get_Parameter(BuiltInParameter.ROOM_NAME).AsString();

                            if (name.Contains(variant) && schemeName.Contains("ВНС"))
                            {
                                double Area = area.get_Parameter(BuiltInParameter.ROOM_AREA).AsDouble();
                                double Svns = UnitUtils.ConvertFromInternalUnits(Area, displayUnitsArea); // 3
                                element.LookupParameter("S этажа ВНС").Set(Math.Round(Svns, 2));
                            }

                            if (name.Contains(variant) && schemeName.Contains("Всего"))
                            {
                                double Area = area.get_Parameter(BuiltInParameter.ROOM_AREA).AsDouble();
                                double Sgvns1 = UnitUtils.ConvertFromInternalUnits(Area, displayUnitsArea); // 4
                                element.LookupParameter("S этажа ГНС1").Set(Math.Round(Sgvns1, 2));
                            }

                            if (name.Contains(variant) && schemeName.Contains("370"))
                            {
                                double Area = area.get_Parameter(BuiltInParameter.ROOM_AREA).AsDouble();
                                double Sgvns2 = UnitUtils.ConvertFromInternalUnits(Area, displayUnitsArea); // 5
                                element.LookupParameter("S этажа ГНС2").Set(Math.Round(Sgvns2, 2));
                            }

                            if (schemeName.Contains("Всего"))
                            {
                                double Area = area.get_Parameter(BuiltInParameter.ROOM_AREA).AsDouble();
                                double Sgvns1 = UnitUtils.ConvertFromInternalUnits(Area, displayUnitsArea); 
                                Sgvns1Sum.Add(Sgvns1); // шестое значение ГНС1
                            }

                            if (schemeName.Contains("370"))
                            {
                                double Area = area.get_Parameter(BuiltInParameter.ROOM_AREA).AsDouble();
                                double Sgvns2 = UnitUtils.ConvertFromInternalUnits(Area, displayUnitsArea); // седьмое значение ГНС2
                                Sgvns2Sum.Add(Sgvns2); // шестое значение ГНС1
                            }
                        }

                        double Snaetazhe_1 = Snaetazhe.Sum(); // 1
                        double Sobshaya_2 = Sobshaya.Sum(); // 2
                        double Sgvns1Sum_6 = Sgvns1Sum.Sum(); // 6
                        double Sgvns2Sum_7 = Sgvns2Sum.Sum(); // 7

                        element.LookupParameter("S общ на этаже").Set(Math.Round(Snaetazhe_1, 2));
                        element.LookupParameter("S общ всех помещений на этаже").Set(Math.Round(Sobshaya_2, 2));
                
                        element.LookupParameter("S всего корпуса ГНС1").Set(Math.Round(Sgvns1Sum_6, 2));
                        element.LookupParameter("S всего корпуса ГНС2").Set(Math.Round(Sgvns2Sum_7, 2));
                    }

                }

                t.Commit();
            }

            return Result.Succeeded;
        }
    }
}
