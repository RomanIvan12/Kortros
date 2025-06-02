using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Kortros.Architecture.Apartments.Model;
using Kortros.Architecture.Apartments.ViewModel;
using System.Xml.Linq;

namespace Kortros.Architecture.Apartments.Utilities
{
    public class RevitFunc
    {
        public static List<Room> GetRoomsInLevel(Document doc, Level lvl)
        {
            return new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Rooms).ToElements()
                .Where(room => room.LevelId == lvl.Id
                && (room as SpatialElement).Area > 0).Select(item => item as Room).ToList();
        }

        // Функция проверки ВНС
        public static ItemZone CheckVns(Document doc, TableInstance tableInstance, string selectedVar = "xxxxx")
        {
            string level = tableInstance.Level;

            ItemZone zone = new ItemZone()
            {
                ZoneName = "ВНС"
            };
            List<Element> areas = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Areas)
                .WhereElementIsNotElementType()
                .ToElements()
                .Where(area => doc.GetElement(area.LevelId).Name == tableInstance.Level)
                .ToList();

            var counter = 0;
            foreach (Element element in areas)
            {
                Area area = element as Area;
                string schemeName = area.AreaScheme.Name; // Имя типоразмера
                string name = string.Empty;
                try
                {
                    name = element.get_Parameter(BuiltInParameter.ROOM_NAME).AsString();
                }
                catch { }

                if ((schemeName.Contains("ВНС") && name.Contains(selectedVar)) ||
                    (schemeName.Contains("ВНС") && !tableInstance.IsVersionsAvailable))
                {
                    counter++;
                }
            }
            if (counter == 1)
            {
                tableInstance.IsAreaVNSCorrect = true;
                zone.Status = "Зона корректна";
            }
            else if (counter == 0)
            {
                tableInstance.IsAreaVNSCorrect = false;
                zone.Status = "Зона отсутствует";
            }
            else
            {
                tableInstance.IsAreaVNSCorrect = false;
                zone.Status = "Число зон с выбранным вариантом больше одного";
            }
            return zone;
        }

        // Функция проверки ГНС1
        public static ItemZone CheckGns1(Document doc, TableInstance tableInstance, string selectedVar = "xxxxx")
        {
            ItemZone zone = new ItemZone()
            {
                ZoneName = "ГНС 250"
            };
            List<Element> areas = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Areas)
                .WhereElementIsNotElementType()
                .ToElements()
                .Where(area => doc.GetElement(area.LevelId).Name == tableInstance.Level)
                .ToList();

            var counter = 0;
            foreach (Element element in areas)
            {
                Area area = element as Area;
                string schemeName = area.AreaScheme.Name; // Имя типоразмера
                string name = string.Empty;
                try
                {
                    name = element.get_Parameter(BuiltInParameter.ROOM_NAME).AsString();
                }
                catch { }

                if ((schemeName.Contains("Всего по зданию") && name.Contains(selectedVar)) ||
                    (schemeName.Contains("Всего") && !tableInstance.IsVersionsAvailable))
                {
                    counter++;
                }
            }
            if (counter == 1)
            {
                tableInstance.IsAreaGNS1Correct = true;
                zone.Status = "Зона корректна";
            }
            else if (counter == 0)
            {
                tableInstance.IsAreaGNS1Correct = false;
                zone.Status = "Зона отсутствует";
            }
            else
            {
                tableInstance.IsAreaGNS1Correct = false;
                zone.Status = "Число зон с выбранным вариантом больше одного";
            }
            return zone;
        }

        // Функция проверки ГНС2
        public static ItemZone CheckGns2(Document doc, TableInstance tableInstance, string selectedVar = "xxxxx")
        {
            ItemZone zone = new ItemZone()
            {
                ZoneName = "ГНС 370"
            };
            List<Element> areas = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Areas)
                .WhereElementIsNotElementType()
                .ToElements()
                .Where(area => doc.GetElement(area.LevelId).Name == tableInstance.Level)
                .ToList();

            var counter = 0;
            foreach (Element element in areas)
            {
                Area area = element as Area;
                string name = string.Empty;
                string schemeName = area.AreaScheme.Name; // Имя типоразмера
                try
                {
                    name = element.get_Parameter(BuiltInParameter.ROOM_NAME).AsString();
                }
                catch { }

                if ((schemeName.Contains("ГНС_370") && name.Contains(selectedVar)) ||
                        (schemeName.Contains("ГНС_370") && !tableInstance.IsVersionsAvailable))
                {
                    counter++;
                }
            }
            if (counter == 1)
            {
                tableInstance.IsAreaGNS2Correct = true;
                zone.Status = "Зона корректна";
            }
            else if (counter == 0)
            {
                tableInstance.IsAreaGNS2Correct = false;
                zone.Status = "Зона отсутствует";
            }
            else
            {
                tableInstance.IsAreaGNS2Correct = false;
                zone.Status = "Число зон с выбранным вариантом больше одного";
            }
            return zone;
        }
    


    }
}
