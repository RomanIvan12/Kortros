using Autodesk.Revit.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Newtonsoft.Json;
using System.IO;
using System.Windows;
using Application = Autodesk.Revit.ApplicationServices.Application;

namespace DataFromNavisView
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RunCommandMaterials : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            Application app = commandData.Application.Application;

            List<BuiltInCategory> bic = new List<BuiltInCategory>()
            {
                BuiltInCategory.OST_Walls,
                BuiltInCategory.OST_Roofs,
                BuiltInCategory.OST_Floors
            };

            //var listOfElements =  (from item in bic
            //        from element in new FilteredElementCollector(doc, doc.ActiveView.Id)
            //            .OfCategory(item)
            //            .WhereElementIsNotElementType()
            //            .ToElements()
            //                       select element).ToList();

            StringBuilder sb = new StringBuilder();

            // Тестовая стена
            Element element = doc.GetElement(new ElementId(573284));
            sb.AppendLine("Id - 573284");
            var wall = element as Wall;
            var wallType = wall.WallType;
            sb.AppendLine("Тип стены: " + wallType.Name);
            var compoundStr = wallType.GetCompoundStructure().GetLayers()
                .OrderBy(item => item.LayerId);
            sb.AppendLine($"Количество слоёв - {wallType.GetCompoundStructure().LayerCount}");

            int counter = 0;
            foreach (var layer in compoundStr)
            {
                counter++;
                var material = doc.GetElement(layer.MaterialId).Name;
                sb.AppendLine($"Слой #{counter} - {material}");
                ForgeTypeId displayUnits = doc.GetUnits().GetFormatOptions(SpecTypeId.Length).GetUnitTypeId();
                var width = UnitUtils.ConvertFromInternalUnits(layer.Width, displayUnits);
                ForgeTypeId areaUnits = doc.GetUnits().GetFormatOptions(SpecTypeId.Area).GetUnitTypeId();
                var area = UnitUtils.ConvertFromInternalUnits(
                    element.get_Parameter(BuiltInParameter.HOST_AREA_COMPUTED).AsDouble(), areaUnits);
                var volume = area * width;
                sb.AppendLine($"Ширина: {width}" +
                              $"\n Площадь: {area}" +
                              $"\n Объём: {volume}");
            }


            MessageBox.Show(sb.ToString());
            return Result.Succeeded;
        }


        public static void ShowList<T>(IEnumerable<T> items)
        {
            string message = string.Join("\n", items);
            MessageBox.Show(message, "Список элементов");
        }
    }
}
