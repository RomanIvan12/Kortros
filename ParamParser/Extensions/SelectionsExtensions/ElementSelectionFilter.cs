using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParamParser.Extensions.SelectionsExtensions
{
    public class ElementSelectionFilter : ISelectionFilter
    {
        private Func<Element, bool> _validateElement;
        public ElementSelectionFilter(Func<Element, bool> validateElement)
        {
            _validateElement = validateElement;
        }


        public bool AllowElement(Element elem)
        {
            return _validateElement(elem);
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }
}
