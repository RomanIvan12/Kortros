using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Kortros.General.ExcelSync.Common
{
    public class Collector
    {
        public static FilteredElementCollector Create(UIApplication uiapp, bool all = false)
        {
            var doc = uiapp.ActiveUIDocument.Document;

            if (all)
            {
                return new FilteredElementCollector(doc);
            }

            var selected = uiapp.ActiveUIDocument.Selection.GetElementIds();
            if (selected.Count > 0)
            {
                return new FilteredElementCollector(doc, selected);
            }

            return new FilteredElementCollector(doc, uiapp.ActiveUIDocument.ActiveView.Id);
        }
    }
}
