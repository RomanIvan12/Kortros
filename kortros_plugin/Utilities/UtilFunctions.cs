using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Kortros.Utilities
{
    public class UtilFunctions
    {
        public static double? GetGlobalParameterDoubleValueByName(Document doc, string name)
        {
            try
            {
                GlobalParameter g_param = doc.GetElement(GlobalParametersManager.FindByName(doc, name)) as GlobalParameter;
                ParameterValue g_parValue = g_param.GetValue();
                DoubleParameterValue total = g_parValue as DoubleParameterValue;
                return total.Value;
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        // Установка Рабочего набора воздуховодов и фиттингов в зависимости от комментария в типе системы
        public static void CommentBasedDuctWorkset(Document doc, Element element)
        {
            Parameter SystemDuct = element.get_Parameter(BuiltInParameter.RBS_DUCT_SYSTEM_TYPE_PARAM);
            if (SystemDuct != null && SystemDuct.AsElementId().IntegerValue != -1)
            {
                MechanicalSystemType ductSystem = (MechanicalSystemType)doc.GetElement(SystemDuct.AsElementId());
                if (ductSystem != null)
                {
                    string ductSystemPosition = ductSystem.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS).AsValueString();
                    foreach (Workset workset in new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset))
                    {
                        if (ductSystemPosition.PadLeft(2, '0') == workset.Name.Split('.')[0])
                        {
                            element.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(workset.Id.IntegerValue);
                        }
                    }
                }
            }
        }

        // Установка Рабочего набора труб и фиттингов в зависимости от комментария в типе системы
        public static void CommentBasedPipeWorkset(Document doc, Element element)
        {
            if (element is FamilyInstance familyInstance)
            {
                if (familyInstance.Symbol.Family.get_Parameter(BuiltInParameter.FAMILY_SHARED).AsInteger() == 0)
                {
                    Parameter SystemPipe = familyInstance.get_Parameter(BuiltInParameter.RBS_PIPING_SYSTEM_TYPE_PARAM);
                    if (SystemPipe != null && SystemPipe.AsElementId().IntegerValue != -1)
                    {
                        PipingSystemType pipeSystem = (PipingSystemType)doc.GetElement(SystemPipe.AsElementId());
                        if (pipeSystem != null)
                        {
                            string pipeSystemPosition = pipeSystem.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS).AsValueString();
                            foreach (Workset workset in new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset))
                            {
                                if (pipeSystemPosition.PadLeft(2, '0') == workset.Name.Split('.')[0])
                                {
                                    familyInstance.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(workset.Id.IntegerValue);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                Parameter SystemPipe = element.get_Parameter(BuiltInParameter.RBS_PIPING_SYSTEM_TYPE_PARAM);
                if (SystemPipe != null && SystemPipe.AsElementId().IntegerValue != -1)
                {
                    PipingSystemType pipeSystem = (PipingSystemType)doc.GetElement(SystemPipe.AsElementId());
                    if (pipeSystem != null)
                    {
                        string pipeSystemPosition = pipeSystem.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS).AsValueString();
                        foreach (Workset workset in new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset))
                        {
                            if (pipeSystemPosition.PadLeft(2, '0') == workset.Name.Split('.')[0])
                            {
                                element.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM).Set(workset.Id.IntegerValue);
                            }
                        }
                    }
                }
            }
        }

        public static void ShowList<T>(IEnumerable<T> items)
        {
            string message = string.Join("\n", items);
            MessageBox.Show(message, "Список элементов");
        }

        public static void ShowDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
        {
            StringBuilder messageBuilder = new StringBuilder();

            foreach (KeyValuePair<TKey, TValue> pair in dictionary)
            {
                string keyString = pair.Key.ToString();
                string valueString = GetFormattedValue(pair.Value);

                string pairString = $"{keyString}: {valueString}";
                messageBuilder.AppendLine(pairString);
            }

            string message = messageBuilder.ToString();
            MessageBox.Show(message, "Элементы словаря", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        public static string GetFormattedValue<T>(T value)
        {
            if (value is Dictionary<object, object> nestedDictionary)
            {
                StringBuilder nestedBuilder = new StringBuilder();
                foreach (KeyValuePair<object, object> nestedPair in nestedDictionary)
                {
                    string nestedKeyString = nestedPair.Key.ToString();
                    string nestedValueString = GetFormattedValue(nestedPair.Value);
                    string nestedPairString = $"{nestedKeyString}: {nestedValueString}";
                    nestedBuilder.AppendLine(nestedPairString);
                }
                return nestedBuilder.ToString().TrimEnd();
            }
            return value.ToString();
        }
    }
}
