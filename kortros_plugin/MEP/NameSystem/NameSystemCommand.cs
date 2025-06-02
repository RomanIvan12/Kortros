using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Kortros.Utilities;

namespace Kortros.MEP.NameSystem
{
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class NameSystemCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Logger.Log.Info("--- Команда Заполнение KRTRS_Имя Системы запущена ---");
            Document doc = commandData.Application.ActiveUIDocument.Document;

            List<BuiltInCategory> listOfCat = new List<BuiltInCategory>
            {
                BuiltInCategory.OST_PipeAccessory, // арматура труб
                BuiltInCategory.OST_FlexPipeCurves, // гибкие трубопроводы
                BuiltInCategory.OST_PipeFitting, // соед. детали труб
                BuiltInCategory.OST_Sprinklers, // спринклеры
                BuiltInCategory.OST_PipeCurves, // трубы
                BuiltInCategory.OST_DuctCurves, // воздуховоды
                BuiltInCategory.OST_DuctFitting, // соед. детали воздуховоды
                BuiltInCategory.OST_DuctAccessory, // арматура воздуховоды
                BuiltInCategory.OST_FlexDuctCurves, // гибкие воздуховоды
                BuiltInCategory.OST_PipeInsulations, // изоляция труб
                BuiltInCategory.OST_DuctInsulations, // изоляция воздуховодов
                BuiltInCategory.OST_DuctLinings // внутренняя изоляция воздуховодов
            };


            List<Element> allElements = new List<Element>();

            foreach (BuiltInCategory category in listOfCat)
            {
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                List<Element> elems = collector.OfCategory(category).WhereElementIsNotElementType().ToElements().ToList();
                allElements.AddRange(elems);
            }

            Logger.Log.Info($"Количество элементов: {allElements.Count.ToString()}");
            NameSystemWindow instance = new NameSystemWindow(doc, allElements);
            instance.ShowDialog();
            Logger.Log.Info("------");
            return Result.Succeeded;
        }
    }
}
