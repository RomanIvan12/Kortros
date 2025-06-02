using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;

namespace Kortros.ParamParser.ViewModel.Extensions
{
    public class ElementSelectionFilter : ISelectionFilter
    {
        private readonly List<BuiltInCategory> _allowedCategories;
        public ElementSelectionFilter(List<BuiltInCategory> allowedCategories)
        {
            _allowedCategories = allowedCategories;
        }

        public bool AllowElement(Element elem)
        {
            if (_allowedCategories.Contains((BuiltInCategory)elem.Category.Id.IntegerValue))
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
