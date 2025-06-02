using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace TechnoPark.Kvart
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class MainCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;


            ElementSelectionFilter selectionFilter = new ElementSelectionFilter(BuiltInCategory.OST_Rooms);
            IList<Reference> references = uiDoc.Selection.PickObjects(ObjectType.Element, selectionFilter); // Выбрать помещения

            List<Element> selectedRooms = new List<Element>();

            foreach (Reference reference in references)
            {
                selectedRooms.Add(doc.GetElement(reference.ElementId));
            }


            List<string> apartNumbers = new List<string>();
            List<List<double>> aparts = new List<List<double>>();
            List<double> roomsAreaCoeff = new List<double>();
            List<double> roomsAreaMultipliedByCoeff = new List<double>();
            List<double> roomsArea = new List<double>();
            List<object> outRooms = new List<object>();


            foreach (var room in selectedRooms)
            {
                var aptNumber = room.get_Parameter(new Guid("10fb72de-237e-4b9c-915b-8849b8907695")).AsString(); // ADSK_Номер квартиры

                double areaNonConv = room.get_Parameter(BuiltInParameter.ROOM_AREA).AsDouble();

                var xxx = Math.Round(areaNonConv * 0.09290304, 6);


                double area = Math.Round(Math.Round(areaNonConv * 0.09290304, 3), 2); // Площадь
                double koeff = 1; // Коэффициент
                double karea = area; // Площадь комнаты с коэффициентом
                double nokarea = area; // Площадь комнаты без коэффициента

                if (area > 0)
                {
                    var aptTip = room.get_Parameter(new Guid("56eb1705-f327-4774-b212-ef9ad2c860b0")).AsInteger(); // ADSK_Тип помещения

                    if (aptTip == 5)
                        koeff = 1;
                    else if (aptTip == 3)
                        koeff = 0.5;
                    else if (aptTip == 4)
                        koeff = 0.3;

                    int contains = apartNumbers.IndexOf(aptNumber);
                    if (contains > -1)
                    {
                        if (aptTip == 1)
                        {
                            aparts[contains][0] += 1; // Добавляем комнату
                            aparts[contains][2] += area; // Добавляем в жилую площадь
                            aparts[contains][3] += area; // Добавляем в полную площадь
                        }
                        else if (aptTip == 2)
                        {
                            aparts[contains][3] += area;
                        }
                        karea = Math.Round(koeff * area, 2);
                        aparts[contains][1] += karea;
                        nokarea = Math.Round(area, 2);
                        aparts[contains][4] += nokarea;
                    }
                    else
                    {
                        apartNumbers.Add(aptNumber);

                        double aptRoomsCount = 0;
                        double uarea = 0;
                        double apartarea = 0;

                        if (aptTip == 1)
                        {
                            aptRoomsCount = 1;
                            uarea = area;
                            apartarea = area;
                            nokarea = area;
                        }
                        else if (aptTip == 2)
                        {
                            apartarea = area;
                        }

                        karea = Math.Round(koeff * area, 2);
                        nokarea = Math.Round(area, 2);

                        aparts.Add(new List<double> { aptRoomsCount, karea, uarea, apartarea, nokarea });
                    }
                }
                roomsAreaCoeff.Add(koeff);
                roomsAreaMultipliedByCoeff.Add(karea);
                roomsArea.Add(area);
            }

            StringBuilder sb = new StringBuilder();

            int i = 0;
            foreach (var room in selectedRooms)
            {
                var aptNumber = room.get_Parameter(new Guid("10fb72de-237e-4b9c-915b-8849b8907695")).AsString(); // ADSK_Номер квартиры
                int aptPos = apartNumbers.IndexOf(aptNumber);
                var aptTip = room.get_Parameter(new Guid("56eb1705-f327-4774-b212-ef9ad2c860b0")).AsInteger(); // ADSK_Тип помещения

                double areaNonConv = room.get_Parameter(BuiltInParameter.ROOM_AREA).AsDouble();
                double area = Math.Round(Math.Round(areaNonConv * 0.09290304, 6), 2); // Площадь


                if (aptPos > -1 && area > 0)
                {
                    var apt = aparts[aptPos];

                    outRooms.Add(new List<object>
                    {
                        room, // Комната
                        $"{aptNumber}_{aptTip}", // Категория
                        apt[0], // Кол-во комнат
                        apt[1], // Общая площадь квартиры
                        apt[2], // Жилая площадь
                        apt[3], // Полная площадь
                        roomsAreaCoeff[i], // Коэффициент площади комнаты
                        roomsAreaMultipliedByCoeff[i], // Площадь с коэффициентом
                        apt[4], // Общая площадь без коэффициента
                        roomsArea[i] // Площадь комнаты без коэффициента
                    });

                    ForgeTypeId areaUnits = doc.GetUnits().GetFormatOptions(SpecTypeId.Area).GetUnitTypeId();
                    try
                    {

                        var apt1 = UnitUtils.ConvertToInternalUnits(apt[1], areaUnits);
                        var apt2 = UnitUtils.ConvertToInternalUnits(apt[2], areaUnits);
                        var apt3 = UnitUtils.ConvertToInternalUnits(apt[3], areaUnits);
                        var roomsAreaMultipliedByCoefff = UnitUtils.ConvertToInternalUnits(roomsAreaMultipliedByCoeff[i], areaUnits);
                        var roomsAreaff = UnitUtils.ConvertToInternalUnits(apt[4], areaUnits);


                        using (Transaction t = new Transaction(doc, $"Записать изменения #{i}"))
                        {
                            t.Start();
                            room.LookupParameter("ADSK_Количество комнат").Set(apt[0]);
                            room.LookupParameter("ADSK_Площадь квартиры общая").Set(apt1);
                            room.LookupParameter("ADSK_Площадь квартиры жилая").Set(apt2);
                            room.LookupParameter("ADSK_Площадь квартиры").Set(apt3);
                            room.LookupParameter("ADSK_Индекс помещения").Set($"{aptNumber}_{aptTip}");
                            room.LookupParameter("ADSK_Коэффициент площади").Set(roomsAreaCoeff[i]);
                            room.LookupParameter("ADSK_Площадь с коэффициентом").Set(roomsAreaMultipliedByCoefff);
                            room.LookupParameter("KRTRS_Общая площадь без К с летними помещениями").Set(roomsAreaff);
                            t.Commit();
                        }
                    }
                    catch (Exception ex)
                    {
                        sb.AppendLine(ex.Message);
                    }

                    room.LookupParameter("ADSK_Количество комнат").Set(apt[0]);
                }
                i++;
            }

            var zzz = outRooms;
            if (sb.Length > 0)
            {
                MessageBox.Show(sb.ToString());
            }
            else
            {
                MessageBox.Show("всё ок");
            }

            return Result.Succeeded;
        }

        public static void ShowList<T>(IEnumerable<T> items)
        {
            string message = string.Join("\n", items);
            MessageBox.Show(message, "Список элементов");
        }
    }



    public class ElementSelectionFilter : ISelectionFilter
    {
        private readonly BuiltInCategory _allowedCategory;
        public ElementSelectionFilter(BuiltInCategory allowedCategory)
        {
            _allowedCategory = allowedCategory;
        }

        public bool AllowElement(Element elem)
        {
            if (_allowedCategory == (BuiltInCategory)elem.Category.Id.IntegerValue)
            {
                return true;
            }
            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
