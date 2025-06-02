using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataFromNavisView.Helpers
{
    public class UtilityFunctions
    {
        public static double GetParameterAccuracy(Document doc, Parameter parameter)
        {
            ForgeTypeId specTypeId = parameter.Definition.GetDataType();
            try
            {
                // Получаем глобальные единицы из документа
                Units projectUnits = doc.GetUnits();
                FormatOptions formatOptions = projectUnits.GetFormatOptions(specTypeId);

                if (formatOptions != null)
                {
                    return formatOptions.Accuracy;
                }
                return 0;
            }
            catch { }
            return 0;
        }
    }
}
