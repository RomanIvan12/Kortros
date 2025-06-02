using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ProgressBar
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ProgressBarCommand : IExternalCommand
    {
        public static Document Doc { get; set; }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Doc = commandData.Application.ActiveUIDocument.Document;

            List<Element> listOfWalls = new FilteredElementCollector(Doc).OfCategory(BuiltInCategory.OST_Walls)
                .WhereElementIsNotElementType().ToElements().ToList();

            try
            {
                BarWindow window = new BarWindow();
                window.Show();
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                return Result.Failed;
            }
        }
    }
}
