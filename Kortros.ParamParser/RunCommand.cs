using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Kortros.ParamParser.Model;
using Kortros.ParamParser.View;
using Kortros.ParamParser.ViewModel.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Kortros.ParamParser
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RunCommand : IExternalCommand
    {
        public static Document Doc { get; set; }
        public static UIApplication UIapp { get; set; }
        public static UIDocument UIdoc { get; set; }

        public static MainWindow MainWindow { get; set; }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Doc = commandData.Application.ActiveUIDocument.Document;
            UIapp = commandData.Application;
            UIdoc = commandData.Application.ActiveUIDocument;

            DatabaseHelper.DeleteTable<ParameterItemCommon>(); // Удаляет существующую таблицу с классом ParameterItemCommon

            // Создание элемента класса ParameterItemCommon для общих параметров для категорий из имеющихся в конфигах
            var existedCategories = DatabaseHelper.Read<CategoryItem>();
            List<string> existedNames = existedCategories.Select(i => i.Name).Distinct().ToList();
            foreach (var categoryName in existedNames)
            {
                ParameterItemHelper.CreateParameterItemCommon(Doc, categoryName);
            }

            MainWindow window = new MainWindow();
            MainWindow = window;
            window.ShowDialog();

            return Result.Succeeded;
        }
    }
}
