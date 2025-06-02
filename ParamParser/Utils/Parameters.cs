using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ParamParser
{
    internal class Parameters
    {
        public static dynamic GetParameterValue(Document doc, Parameter parameter)
        {
            if (parameter.GetType() == typeof(Parameter))
            {
                StorageType storageType = parameter.StorageType;
                if (storageType == StorageType.Integer)
                {
                    return parameter.AsInteger();
                }
                else if (storageType == StorageType.Double)
                {
                    int y = 0;
                    var x = parameter.AsValueString().Split('.');
                    try
                    {
                        y = (Regex.Replace(x[1], "[^0-9]", "")).Count();
                    }
                    catch { }
                    //return UnitUtils.ConvertFromInternalUnits(parameter.AsDouble(), doc.GetUnits().GetFormatOptions(SpecTypeId.Length).GetUnitTypeId());
                    var a = UnitUtils.ConvertFromInternalUnits(parameter.AsDouble(), parameter.GetUnitTypeId());
                    if (y > 0)
                    {
                        return Math.Round(a, y);
                    }
                    else
                    {
                        return Math.Round(a, 0);
                    }
                    //return UnitUtils.ConvertFromInternalUnits(parameter.AsDouble(), parameter.GetUnitTypeId());
                }
                else if (storageType == StorageType.String)
                {
                    return parameter.AsString();
                }
                else if (storageType == StorageType.ElementId)
                {
                    return parameter.AsElementId();
                }
            }
            return null;
        }
    }
}
