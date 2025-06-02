using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kortros.General.UpdateParameters.Common
{
    public class Collector
    {
        public static FilteredElementCollector Create(UIApplication uiapp, bool all = false)
        {
            Document doc = uiapp.ActiveUIDocument.Document;

            if (all)
            {
                return new FilteredElementCollector(doc);
            }

            var selected = uiapp.ActiveUIDocument.Selection.GetElementIds();
            if (selected.Count > 0)
            {
                return new FilteredElementCollector(doc);
            }

            return new FilteredElementCollector(doc, uiapp.ActiveUIDocument.ActiveView.Id);

        }
    }
}
