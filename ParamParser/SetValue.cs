using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;
using ParamParser.Extensions.SelectionsExtensions;
using ParamParser.WPF;
using ComboBox = System.Windows.Controls.ComboBox;
using ParamParser.Log4Net;
using log4net;


namespace ParamParser
{
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class SetValue
    {
        private static Document _doc;
        private static ItemCat _itemCat;
        private static ItemPar _itemParInit;
        private static ItemPar _itemParTarget;
        private static bool _rewrite;
        private static StorageType _storageTypeInitial;
        private static StorageType _storageTypeTarget;
        private static IList<ElementId> _selectedElementIds;


        private static readonly ILog _logger = ParserCommand._logger;
        private static readonly ILog _loggerShow = ParserCommand._loggerShow;

        private readonly static string Message = "message";

        public SetValue(Document doc, ItemCat itemCat, ItemPar itemParInit, ItemPar itemParTarget, bool rewrite,
            StorageType storageTypeInitial, StorageType storageTypeTarget, IList<ElementId> selectedElementIds)
        {
            _doc = doc;
            _itemCat = itemCat;
            _itemParInit = itemParInit;
            _itemParTarget = itemParTarget;
            _rewrite = rewrite;
            _storageTypeInitial = storageTypeInitial;
            _storageTypeTarget = storageTypeTarget;
            _selectedElementIds = selectedElementIds;
            //MessageBox.Show(doc.ToString() + "\n" + itemCat.ToString() + "\n" + itemParInit.ToString() + "\n" + itemParTarget.ToString()
               // + "\n" + _rewrite.ToString() + "\n" + _storageTypeInitial.ToString() + "\n" + _storageTypeTarget.ToString());
        }

        static dynamic GetParameterInfo(Parameter parameter)
        {
            InternalDefinition intdef = parameter.Definition as InternalDefinition;
            if (parameter.IsShared)
            {
                return parameter.GUID;
            }
            else if (intdef.BuiltInParameter != BuiltInParameter.INVALID)
            {
                return intdef.BuiltInParameter;
            }
            else
            {
                return parameter.Definition.Name;
            }
        }

        private static Dictionary<ElementId, Parameter> GetValuesOfTargetParameter(Category category)
        {
            IList<Element> InstanceElements = new FilteredElementCollector(_doc).OfCategoryId(category.Id).WhereElementIsNotElementType().ToElements();
            IList<Element> TypeElements = new FilteredElementCollector(_doc).OfCategoryId(category.Id).WhereElementIsElementType().ToElements();

            Parameter selectedParam = _itemParInit.SelectedParameter; //конкретный исходный параметр конкретного экземпляра ИЛИ типа
            Parameter targetParam = _itemParTarget.SelectedParameter; //конкретный целевой параметр конкретного экземпляра ИЛИ типа

            var info_init = GetParameterInfo(selectedParam); //Инфо по исходному
            var info = GetParameterInfo(targetParam); //Инфо по целевому

            var typeOfParameter = info.GetType();
            var typeOfInitParameter = info_init.GetType();
            Dictionary<ElementId, Parameter> ElementIdDictionaryValue = new Dictionary<ElementId, Parameter>();

            if (targetParam.Element.GetTypeId().IntegerValue != -1) // INST
            {
                try
                {
                    foreach (Element element in InstanceElements)
                    {
                        if (typeOfParameter == typeof(string))
                        {
                            Parameter aaa = element.LookupParameter(info);
                            if (typeOfInitParameter == typeof(string))
                            {
                                Parameter bbb = element.LookupParameter(info_init);
                                ElementIdDictionaryValue.Add(element.Id, aaa);
                            }
                            else if (typeOfInitParameter == typeof(Guid))
                            {
                                Parameter bbb = element.get_Parameter(info_init);
                                ElementIdDictionaryValue.Add(element.Id, aaa);
                            }
                            else if (typeOfInitParameter == typeof(BuiltInParameter))
                            {
                                Parameter bbb = element.get_Parameter(info_init);
                                ElementIdDictionaryValue.Add(element.Id, aaa);
                            }
                        }
                        else if (typeOfParameter == typeof(Guid))
                        {
                            Parameter aaa = element.get_Parameter(info);

                            if (typeOfInitParameter == typeof(string))
                            {
                                Parameter bbb = element.LookupParameter(info_init);
                                ElementIdDictionaryValue.Add(element.Id, aaa);
                            }
                            else if (typeOfInitParameter == typeof(Guid))
                            {
                                Parameter bbb = element.get_Parameter(info_init);
                                ElementIdDictionaryValue.Add(element.Id, aaa);
                            }
                            else if (typeOfInitParameter == typeof(BuiltInParameter))
                            {
                                Parameter bbb = element.get_Parameter(info_init);
                                ElementIdDictionaryValue.Add(element.Id, aaa);
                            }
                        }
                        else if (typeOfParameter == typeof(BuiltInParameter))
                        {
                            Parameter aaa = element.get_Parameter(info);
                            if (typeOfInitParameter == typeof(string))
                            {
                                Parameter bbb = element.LookupParameter(info_init);
                                ElementIdDictionaryValue.Add(element.Id, aaa);
                            }
                            else if (typeOfInitParameter == typeof(Guid))
                            {
                                Parameter bbb = element.get_Parameter(info_init);
                                ElementIdDictionaryValue.Add(element.Id, aaa);
                            }
                            else if (typeOfInitParameter == typeof(BuiltInParameter))
                            {
                                Parameter bbb = element.get_Parameter(info_init);
                                ElementIdDictionaryValue.Add(element.Id, aaa);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
                return ElementIdDictionaryValue;
            }
            else // TYPE
            {
                try
                {
                    foreach (Element element in TypeElements)
                    {
                        if (typeOfParameter == typeof(string))
                        {
                            Parameter aaa = element.LookupParameter(info);
                            if (typeOfInitParameter == typeof(string))
                            {
                                Parameter bbb = element.LookupParameter(info_init);
                                if (bbb.HasValue)
                                {
                                    ElementIdDictionaryValue.Add(element.Id, aaa);
                                }
                            }
                            else if (typeOfInitParameter == typeof(Guid))
                            {
                                Parameter bbb = element.get_Parameter(info_init);
                                if (bbb.HasValue)
                                {
                                    ElementIdDictionaryValue.Add(element.Id, aaa);
                                }
                            }
                            else if (typeOfInitParameter == typeof(BuiltInParameter))
                            {
                                Parameter bbb = element.get_Parameter(info_init);
                                if (bbb.HasValue)
                                {
                                    ElementIdDictionaryValue.Add(element.Id, aaa);
                                }
                            }
                        }
                        else if (typeOfParameter == typeof(Guid))
                        {
                            Parameter aaa = element.get_Parameter(info);
                            if (typeOfInitParameter == typeof(string))
                            {
                                Parameter bbb = element.LookupParameter(info_init);
                                if (bbb.HasValue)
                                {
                                    ElementIdDictionaryValue.Add(element.Id, aaa);
                                }
                            }
                            else if (typeOfInitParameter == typeof(Guid))
                            {
                                Parameter bbb = element.get_Parameter(info_init);
                                if (bbb.HasValue)
                                {
                                    ElementIdDictionaryValue.Add(element.Id, aaa);
                                }
                            }
                            else if (typeOfInitParameter == typeof(BuiltInParameter))
                            {
                                Parameter bbb = element.get_Parameter(info_init);
                                if (bbb.HasValue)
                                {
                                    ElementIdDictionaryValue.Add(element.Id, aaa);
                                }
                            }
                        }
                        else if (typeOfParameter == typeof(BuiltInParameter))
                        {
                            Parameter aaa = element.get_Parameter(info);
                            if (typeOfInitParameter == typeof(string))
                            {
                                Parameter bbb = element.LookupParameter(info_init);
                                if (bbb.HasValue)
                                {
                                    ElementIdDictionaryValue.Add(element.Id, aaa);
                                }
                            }
                            else if (typeOfInitParameter == typeof(Guid))
                            {
                                Parameter bbb = element.get_Parameter(info_init);
                                if (bbb.HasValue)
                                {
                                    ElementIdDictionaryValue.Add(element.Id, aaa);
                                }
                            }
                            else if (typeOfInitParameter == typeof(BuiltInParameter))
                            {
                                Parameter bbb = element.get_Parameter(info_init);
                                if (bbb.HasValue)
                                {
                                    ElementIdDictionaryValue.Add(element.Id, aaa);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
                return ElementIdDictionaryValue;
            }
        }

        private static Dictionary<ElementId, object> GetValuesOfInitParameter(Category category)
        {
            IList<Element> InstanceElements = new FilteredElementCollector(_doc).OfCategoryId(category.Id).WhereElementIsNotElementType().ToElements();
            IList<Element> TypeElements = new FilteredElementCollector(_doc).OfCategoryId(category.Id).WhereElementIsElementType().ToElements();

            Parameter selectedParam = _itemParInit.SelectedParameter; //конкретный исходный параметр конкретного экземпляра ИЛИ типа

            var info = GetParameterInfo(selectedParam); //Инфо по исходному
            Dictionary<ElementId, object> ElementIdDictionaryValue = new Dictionary<ElementId, object>();

            if (selectedParam.Element.GetTypeId().IntegerValue != -1)
            {
                //try
                //{
                foreach (Element element in InstanceElements)
                {
                    if (info.GetType() == typeof(string))
                    {
                        try
                        {
                            Parameter aaa = element.LookupParameter(info); // указать, если ААА нулл
                            if (aaa.HasValue)
                            {
                                var value = Parameters.GetParameterValue(_doc, aaa);
                                ElementIdDictionaryValue.Add(element.Id, value);
                            }
                        }
                        catch (Exception ex)
                        {
                            ElementIdDictionaryValue.Add(element.Id, null);
                            _logger.Error(ex);
                        }
                    }
                    else if (info.GetType() == typeof(BuiltInParameter))
                    {
                        try
                        {
                            Parameter aaa = element.get_Parameter(info);
                            if (aaa.HasValue)
                            {
                                var value = Parameters.GetParameterValue(_doc, aaa);
                                ElementIdDictionaryValue.Add(element.Id, value);
                            }
                        }
                        catch (Exception ex)
                        {
                            ElementIdDictionaryValue.Add(element.Id, null);
                            _logger.Error(ex);
                        }
                    }
                    else if (info.GetType() == typeof(Guid))
                    {
                        try
                        {
                            Parameter aaa = element.get_Parameter(info);
                            if (aaa.HasValue)
                            {
                                var value = Parameters.GetParameterValue(_doc, aaa);
                                ElementIdDictionaryValue.Add(element.Id, value);
                            }
                        }
                        catch (Exception ex)
                        {
                            ElementIdDictionaryValue.Add(element.Id, null);
                            _logger.Error(ex);
                        }
                    }
                }
                //}
                //catch (Exception ex)
                //{
                //    _logger.Error(ex);
                //}
                return ElementIdDictionaryValue;
            }
            else
            {
                //try
                //{
                foreach (Element element in TypeElements)
                {
                    if (info.GetType() == typeof(Guid))
                    {
                        Parameter aaa = element.get_Parameter(info);
                        if (aaa.HasValue)
                        {
                            var value = Parameters.GetParameterValue(_doc, aaa); ;
                            ElementIdDictionaryValue.Add(element.Id, value);
                        }
                    }
                    else if (info.GetType() == typeof(string))
                    {
                        Parameter aaa = element.LookupParameter(info);
                        if (aaa.HasValue)
                        {
                            var value = Parameters.GetParameterValue(_doc, aaa);
                            ElementIdDictionaryValue.Add(element.Id, value);
                        }
                    }
                    else if (info.GetType() == typeof(BuiltInParameter))
                    {
                        Parameter aaa = element.get_Parameter(info);
                        if (aaa.HasValue)
                        {
                            var value = Parameters.GetParameterValue(_doc, aaa);
                            ElementIdDictionaryValue.Add(element.Id, value);
                        }
                    }
                }
                //}
                //catch (Exception ex)
                //{
                //    _logger.Error(ex);
                //}
                return ElementIdDictionaryValue;
            }
        }

        private static Dictionary<ElementId, object> GetValuesOfSelectedInitParameters(IList<ElementId> elementIds)
        {
            Parameter selectedParam = _itemParInit.SelectedParameter;
            var info = GetParameterInfo(selectedParam);

            List<Element> elements = elementIds.Select(c => _doc.GetElement(c)).ToList();
            Dictionary<ElementId, object> ElementIdDictionaryValue = new Dictionary<ElementId, object>();

            if (selectedParam.Element.GetTypeId().IntegerValue != -1)
            {
                try
                {
                    foreach (Element element in elements)
                    {

                        if (info.GetType() == typeof(string))
                        {
                            Parameter aaa = element.LookupParameter(info);
                            if (aaa.HasValue)
                            {
                                var value = Parameters.GetParameterValue(_doc, aaa);
                                ElementIdDictionaryValue.Add(element.Id, value);
                            }
                        }
                        else if (info.GetType() == typeof(BuiltInParameter))
                        {
                            Parameter aaa = element.get_Parameter(info);
                            if (aaa.HasValue)
                            {
                                var value = Parameters.GetParameterValue(_doc, aaa);
                                ElementIdDictionaryValue.Add(element.Id, value);
                            }
                        }
                        else if (info.GetType() == typeof(Guid))
                        {
                            Parameter aaa = element.get_Parameter(info);
                            if (aaa.HasValue)
                            {
                                var value = Parameters.GetParameterValue(_doc, aaa);
                                ElementIdDictionaryValue.Add(element.Id, value);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
                return ElementIdDictionaryValue;
            }
            else
            {
                try
                {
                    foreach (Element element in elements)
                    {
                        if (info.GetType() == typeof(Guid))
                        {
                            Parameter aaa = element.get_Parameter(info);
                            if (aaa.HasValue)
                            {
                                var value = Parameters.GetParameterValue(_doc, aaa); ;
                                ElementIdDictionaryValue.Add(element.Id, value);
                            }

                        }
                        else if (info.GetType() == typeof(string))
                        {
                            Parameter aaa = element.LookupParameter(info);
                            if (aaa.HasValue)
                            {
                                var value = Parameters.GetParameterValue(_doc, aaa);
                                ElementIdDictionaryValue.Add(element.Id, value);
                            }
                        }
                        else if (info.GetType() == typeof(BuiltInParameter))
                        {
                            Parameter aaa = element.get_Parameter(info);
                            if (aaa.HasValue)
                            {
                                var value = Parameters.GetParameterValue(_doc, aaa);
                                ElementIdDictionaryValue.Add(element.Id, value);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
                return ElementIdDictionaryValue;
            }
        }

        private static Dictionary<ElementId, Parameter> GetValuesOfSelectedTargetParameter(IList<ElementId> elementIds)
        {

            Parameter selectedParam = _itemParInit.SelectedParameter; //конкретный исходный параметр конкретного экземпляра ИЛИ типа
            Parameter targetParam = _itemParTarget.SelectedParameter; //конкретный целевой параметр конкретного экземпляра ИЛИ типа

            var info_init = GetParameterInfo(selectedParam); //Инфо по исходному
            var info = GetParameterInfo(targetParam); //Инфо по целевому

            List<Element> elements = elementIds.Select(c => _doc.GetElement(c)).ToList();

            var typeOfParameter = info.GetType();
            var typeOfInitParameter = info_init.GetType();
            Dictionary<ElementId, Parameter> ElementIdDictionaryValue = new Dictionary<ElementId, Parameter>();

            if (targetParam.Element.GetTypeId().IntegerValue != -1) // INST и TYPE
            {
                try
                {
                    foreach (Element element in elements)
                    {
                        if (typeOfParameter == typeof(string))
                        {
                            Parameter aaa = element.LookupParameter(info);
                            if (typeOfInitParameter == typeof(string))
                            {
                                Parameter bbb = element.LookupParameter(info_init);
                                ElementIdDictionaryValue.Add(element.Id, aaa);
                            }
                            else if (typeOfInitParameter == typeof(Guid))
                            {
                                Parameter bbb = element.get_Parameter(info_init);
                                ElementIdDictionaryValue.Add(element.Id, aaa);
                            }
                            else if (typeOfInitParameter == typeof(BuiltInParameter))
                            {
                                Parameter bbb = element.get_Parameter(info_init);
                                ElementIdDictionaryValue.Add(element.Id, aaa);
                            }
                        }
                        else if (typeOfParameter == typeof(Guid))
                        {
                            Parameter aaa = element.get_Parameter(info);

                            if (typeOfInitParameter == typeof(string))
                            {
                                Parameter bbb = element.LookupParameter(info_init);
                                ElementIdDictionaryValue.Add(element.Id, aaa);
                            }
                            else if (typeOfInitParameter == typeof(Guid))
                            {
                                Parameter bbb = element.get_Parameter(info_init);
                                ElementIdDictionaryValue.Add(element.Id, aaa);
                            }
                            else if (typeOfInitParameter == typeof(BuiltInParameter))
                            {
                                Parameter bbb = element.get_Parameter(info_init);
                                ElementIdDictionaryValue.Add(element.Id, aaa);
                            }
                        }
                        else if (typeOfParameter == typeof(BuiltInParameter))
                        {
                            Parameter aaa = element.get_Parameter(info);
                            if (typeOfInitParameter == typeof(string))
                            {
                                Parameter bbb = element.LookupParameter(info_init);
                                ElementIdDictionaryValue.Add(element.Id, aaa);
                            }
                            else if (typeOfInitParameter == typeof(Guid))
                            {
                                Parameter bbb = element.get_Parameter(info_init);
                                ElementIdDictionaryValue.Add(element.Id, aaa);
                            }
                            else if (typeOfInitParameter == typeof(BuiltInParameter))
                            {
                                Parameter bbb = element.get_Parameter(info_init);
                                ElementIdDictionaryValue.Add(element.Id, aaa);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
                return ElementIdDictionaryValue;
            }
            else // TYPE
            {
                try
                {
                    foreach (Element element in elements)
                    {
                        if (typeOfParameter == typeof(string))
                        {
                            Parameter aaa = element.LookupParameter(info);
                            if (typeOfInitParameter == typeof(string))
                            {
                                Parameter bbb = element.LookupParameter(info_init);
                                if (bbb.HasValue)
                                {
                                    ElementIdDictionaryValue.Add(element.Id, aaa);
                                }
                            }
                            else if (typeOfInitParameter == typeof(Guid))
                            {
                                Parameter bbb = element.get_Parameter(info_init);
                                if (bbb.HasValue)
                                {
                                    ElementIdDictionaryValue.Add(element.Id, aaa);
                                }
                            }
                            else if (typeOfInitParameter == typeof(BuiltInParameter))
                            {
                                Parameter bbb = element.get_Parameter(info_init);
                                if (bbb.HasValue)
                                {
                                    ElementIdDictionaryValue.Add(element.Id, aaa);
                                }
                            }
                        }
                        else if (typeOfParameter == typeof(Guid))
                        {
                            Parameter aaa = element.get_Parameter(info);
                            if (typeOfInitParameter == typeof(string))
                            {
                                Parameter bbb = element.LookupParameter(info_init);
                                if (bbb.HasValue)
                                {
                                    ElementIdDictionaryValue.Add(element.Id, aaa);
                                }
                            }
                            else if (typeOfInitParameter == typeof(Guid))
                            {
                                Parameter bbb = element.get_Parameter(info_init);
                                if (bbb.HasValue)
                                {
                                    ElementIdDictionaryValue.Add(element.Id, aaa);
                                }
                            }
                            else if (typeOfInitParameter == typeof(BuiltInParameter))
                            {
                                Parameter bbb = element.get_Parameter(info_init);
                                if (bbb.HasValue)
                                {
                                    ElementIdDictionaryValue.Add(element.Id, aaa);
                                }
                            }
                        }
                        else if (typeOfParameter == typeof(BuiltInParameter))
                        {
                            Parameter aaa = element.get_Parameter(info);
                            if (typeOfInitParameter == typeof(string))
                            {
                                Parameter bbb = element.LookupParameter(info_init);
                                if (bbb.HasValue)
                                {
                                    ElementIdDictionaryValue.Add(element.Id, aaa);
                                }
                            }
                            else if (typeOfInitParameter == typeof(Guid))
                            {
                                Parameter bbb = element.get_Parameter(info_init);
                                if (bbb.HasValue)
                                {
                                    ElementIdDictionaryValue.Add(element.Id, aaa);
                                }
                            }
                            else if (typeOfInitParameter == typeof(BuiltInParameter))
                            {
                                Parameter bbb = element.get_Parameter(info_init);
                                if (bbb.HasValue)
                                {
                                    ElementIdDictionaryValue.Add(element.Id, aaa);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
                return ElementIdDictionaryValue;
            }
        }

        private static void SetValueFromString(StorageType parameterInitial, StorageType parameterTarget, Parameter parameterToSet, string Value)
        {
            if (parameterInitial == StorageType.String)
            {
                try
                {
                    if (parameterTarget == StorageType.String)
                    {
                        parameterToSet.Set(Value);
                    }
                    else if (parameterTarget == StorageType.Integer)
                    {
                        bool success = int.TryParse(Value, out int numberValue);
                        if (success) { parameterToSet.Set(numberValue); }
                    }
                    else if (parameterTarget == StorageType.Double)
                    {
                        bool success = Decimal.TryParse(Value, out Decimal numberValue);
                        if (success) { parameterToSet.Set((double)numberValue); }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }
        }
        private static void SetValueFromInteger(StorageType parameterInitial, StorageType parameterTarget, Parameter parameterToSet, int Value)
        {
            if (parameterInitial == StorageType.Integer)
            {
                try
                {
                    if (parameterTarget == StorageType.String)
                    {
                        parameterToSet.Set(Value.ToString());
                    }
                    else if (parameterTarget == StorageType.Integer)
                    {
                        parameterToSet.Set(Value);
                    }
                    else if (parameterTarget == StorageType.Double)
                    {
                        parameterToSet.Set(Value);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }
        }
        private static void SetValueFromDouble(StorageType parameterInitial, StorageType parameterTarget, Parameter parameterToSet, double Value)
        {
            if (parameterInitial == StorageType.Double)
            {
                try
                {
                    if (parameterTarget == StorageType.String)
                    {
                        //parameterToSet.Set(Math.Round(Value, 3).ToString());
                        parameterToSet.Set(Value.ToString());

                    }
                    else if (parameterTarget == StorageType.Integer)
                    {
                        parameterToSet.Set((int)Value);
                    }
                    else if (parameterTarget == StorageType.Double)
                    {
                        parameterToSet.Set(Value);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }
        }
        private static void SetValueFromElementId(StorageType parameterInitial, StorageType parameterTarget, Parameter parameterToSet, ElementId Value)
        {
            if (parameterInitial == StorageType.ElementId)
            {
                try
                {
                    if (parameterTarget == StorageType.String)
                    {
                        parameterToSet.Set(Value.IntegerValue.ToString());
                    }
                    else if (parameterTarget == StorageType.Integer)
                    {
                        parameterToSet.Set(Value.IntegerValue);
                    }
                    else if (parameterTarget == StorageType.Double)
                    {
                        parameterToSet.Set((double)Value.IntegerValue);
                    }
                    else if (parameterTarget == StorageType.ElementId)
                    {
                        parameterToSet.Set(Value);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }
        }


        public static void SetValues()
        {
            //получение выбранного элемента категории
            Category selectedCat = _itemCat.SelectedCategory; // выбранная категория

            // Получил словарь ЭлементАйди-таргетПараметр
            Dictionary<ElementId, Parameter> dictOfTargParams = GetValuesOfTargetParameter(selectedCat); // ТУТ ТИПЫ НЕ ЗАЛЕТАЮТ!
            // Получил список элементов ЦЕЛЕВЫХ, в которые вношу (типа или экземпляра)
            List<ElementId> listofElementsTarget = dictOfTargParams.Keys.ToList();

            // Получил словарь ЭлементАйди-значение Исходных
            //Dictionary<ElementId, object> dictOfEla = new Dictionary<ElementId, object>();

            Dictionary<ElementId, object> dictOfEl = GetValuesOfInitParameter(selectedCat);

            Parameter selectedParam = _itemParInit.SelectedParameter; //конкретный ИСХОДНЫЙ параметр конкретного экземпляра ИЛИ типа
            Parameter targetParam = _itemParTarget.SelectedParameter; //конкретный ЦЕЛЕВОЙ параметр конкретного экземпляра ИЛИ типа

            using (Transaction t = new Transaction(_doc, "SetValues"))
            {
                t.Start();
                if (targetParam.Element.GetTypeId().IntegerValue == -1) //TYPE
                {
                    if (selectedParam.Element.GetTypeId().IntegerValue == -1) //TYPE
                    {
                        int index = 0;
                        foreach (ElementId elementId in listofElementsTarget)
                        {
                            //для лога
                            string messageToSet = Message;
                            ElementType elementType = _doc.GetElement(elementId) as ElementType;
                            string name = elementType.Name + _doc.GetElement(elementId).Name;

                            index++;
                            Parameter parameterToSet = dictOfTargParams[elementId];
                            var Value = dictOfEl[elementId];

                            var isEmpty = IsTargetValueEmpty(parameterToSet);

                            if (Value.GetType() == typeof(string)
                                && (_rewrite == true
                                || (_rewrite == false && isEmpty == true)))
                            {
                                SetValueFromString(_storageTypeInitial, _storageTypeTarget, parameterToSet, Value as string);
                            }
                            else if (Value.GetType() == typeof(int)
                                && (_rewrite == true
                                || (_rewrite == false && isEmpty == true)))
                            {
                                SetValueFromInteger(_storageTypeInitial, _storageTypeTarget, parameterToSet, (int)Value);
                            }
                            else if (Value.GetType() == typeof(double)
                                && (_rewrite == true
                                || (_rewrite == false && isEmpty == true)))
                            {
                                SetValueFromDouble(_storageTypeInitial, _storageTypeTarget, parameterToSet, (double)Value);
                            }
                            else if (Value.GetType() == typeof(ElementId)
                                && (_rewrite == true
                                || (_rewrite == false && isEmpty == true)))
                            {
                                SetValueFromElementId(_storageTypeInitial, _storageTypeTarget, parameterToSet, Value as ElementId);
                            }
                            else
                            {
                                messageToSet = "пареметр имел значение";
                            }
                            //строка для лога
                            string message = $"{index} - {elementId} - {name}" +
                                $" - <{parameterToSet.Definition.Name}> - {_storageTypeTarget} - {messageToSet}";
                            _loggerShow.Info(message);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Невозможно перенести значение параметра типа в параметр экземпляра");
                        //Error
                    }
                }
                else //INSTANCE 
                {
                    int index = 0;

                    foreach (ElementId elementId in listofElementsTarget)
                    {
                        try
                        {
                            index++;
                            string messageToSet = Message;

                            Element elementToSet = _doc.GetElement(elementId); // экземпляры элементы
                            Parameter parameterToSet = dictOfTargParams[elementId]; // параметры к экземплярам элементов
                            var isEmpty = IsTargetValueEmpty(parameterToSet); //log

                            if (_doc.GetElement(dictOfEl.Keys.First()).GetTypeId().IntegerValue == -1)
                            {
                                var Value = dictOfEl[elementToSet.GetTypeId()];
                                if (Value.GetType() == typeof(string)
                                    && (_rewrite == true
                                    || (_rewrite == false && isEmpty == true)))
                                {
                                    SetValueFromString(_storageTypeInitial, _storageTypeTarget, parameterToSet, Value as string);
                                }
                                else if (Value.GetType() == typeof(int)
                                    && (_rewrite == true
                                    || (_rewrite == false && isEmpty == true)))
                                {
                                    SetValueFromInteger(_storageTypeInitial, _storageTypeTarget, parameterToSet, (int)Value);
                                }
                                else if (Value.GetType() == typeof(double)
                                    && (_rewrite == true
                                    || (_rewrite == false && isEmpty == true)))
                                {
                                    SetValueFromDouble(_storageTypeInitial, _storageTypeTarget, parameterToSet, (double)Value);
                                }
                                else if (Value.GetType() == typeof(ElementId)
                                    && (_rewrite == true
                                    || (_rewrite == false && isEmpty == true)))
                                {
                                    SetValueFromElementId(_storageTypeInitial, _storageTypeTarget, parameterToSet, Value as ElementId);
                                }
                                else
                                {
                                    messageToSet = "пареметр имел значение";
                                    //MessageBox.Show("erreer");
                                }
                                //строка для лога
                                string message = $"{index} - {elementId} - {elementToSet.Name}" +
                                    $" - <{parameterToSet.Definition.Name}> - {_storageTypeTarget} - {messageToSet}";
                                _loggerShow.Info(message);
                            }
                            else
                            {
                                try
                                {
                                    var Value = dictOfEl[elementId]; // ТУТ ОШИБКА (т.к. элементИд закидывается экземпляра, а надо - типа
                                    if (Value.GetType() == typeof(string)
                                        && (_rewrite == true
                                        || (_rewrite == false && isEmpty == true)))
                                    {
                                        SetValueFromString(_storageTypeInitial, _storageTypeTarget, parameterToSet, Value as string);
                                    }
                                    else if (Value.GetType() == typeof(int)
                                        && (_rewrite == true
                                        || (_rewrite == false && isEmpty == true)))
                                    {
                                        SetValueFromInteger(_storageTypeInitial, _storageTypeTarget, parameterToSet, (int)Value);
                                    }
                                    else if (Value.GetType() == typeof(double)
                                        && (_rewrite == true
                                        || (_rewrite == false && isEmpty == true)))
                                    {
                                        SetValueFromDouble(_storageTypeInitial, _storageTypeTarget, parameterToSet, (double)Value);
                                    }
                                    else if (Value.GetType() == typeof(ElementId)
                                        && (_rewrite == true
                                        || (_rewrite == false && isEmpty == true)))
                                    {
                                        SetValueFromElementId(_storageTypeInitial, _storageTypeTarget, parameterToSet, Value as ElementId);
                                    }
                                    else
                                    {
                                        messageToSet = "пареметр имел значение";
                                        //MessageBox.Show("erreer");
                                    }
                                    //строка для лога
                                    string message = $"{index} - {elementId} - {elementToSet.Name}" +
                                        $" - <{parameterToSet.Definition.Name}> - {_storageTypeTarget} - {messageToSet}";
                                    _loggerShow.Info(message);
                                }
                                catch (Exception ex) 
                                {
                                    _logger.Error(ex);
                                }
                            }
                        }
                        catch { }
                    }
                }
                t.Commit();
            }
        }
    
        public static void SetValuessSelected()
        {
            Parameter selectedParam = _itemParInit.SelectedParameter; //конкретный ИСХОДНЫЙ параметр конкретного экземпляра ИЛИ типа
            Parameter targetParam = _itemParTarget.SelectedParameter; //конкретный ЦЕЛЕВОЙ параметр конкретного экземпляра ИЛИ типа

            IList <ElementId> selectedElementIds = _selectedElementIds; // выделенные элемнеты

            Dictionary<ElementId, object> dictOfEl = GetValuesOfSelectedInitParameters(selectedElementIds);

            Dictionary<ElementId, Parameter> dictOfTargParams = GetValuesOfSelectedTargetParameter(selectedElementIds);

            // Получил список элементов ЦЕЛЕВЫХ, в которые вношу (типа или экземпляра)
            List<ElementId> listofElementsTarget = dictOfTargParams.Keys.ToList();

            using (Transaction t = new Transaction(_doc, "setValuesSelec"))
            {
                t.Start();
                if (targetParam.Element.GetTypeId().IntegerValue == -1) //TYPE
                {
                    if (selectedParam.Element.GetTypeId().IntegerValue == -1) //TYPE
                    {
                        int index = 0;
                        foreach (ElementId elementId in listofElementsTarget)
                        {
                            //log
                            index++;
                            string messageToSet = Message;
                            ElementType elementType = _doc.GetElement(elementId) as ElementType;
                            string name = elementType.Name + _doc.GetElement(elementId).Name;

                            Parameter parameterToSet = dictOfTargParams[elementId];
                            var Value = dictOfEl[elementId];

                            var isEmpty = IsTargetValueEmpty(parameterToSet); //log

                            if (Value.GetType() == typeof(string)
                                && (_rewrite == true
                                || (_rewrite == false && isEmpty == true)))
                            {
                                SetValueFromString(_storageTypeInitial, _storageTypeTarget, parameterToSet, Value as string);
                            }
                            else if (Value.GetType() == typeof(int)
                                && (_rewrite == true
                                || (_rewrite == false && isEmpty == true)))
                            {
                                SetValueFromInteger(_storageTypeInitial, _storageTypeTarget, parameterToSet, (int)Value);
                            }
                            else if (Value.GetType() == typeof(double)
                                && (_rewrite == true
                                || (_rewrite == false && isEmpty == true)))
                            {
                                SetValueFromDouble(_storageTypeInitial, _storageTypeTarget, parameterToSet, (double)Value);
                            }
                            else if (Value.GetType() == typeof(ElementId)
                                && (_rewrite == true
                                || (_rewrite == false && isEmpty == true)))
                            {
                                SetValueFromElementId(_storageTypeInitial, _storageTypeTarget, parameterToSet, Value as ElementId);
                            }
                            else
                            {
                                messageToSet = "пареметр имел значение";
                            }
                            //строка для лога
                            string message = $"{index} - {elementId} - {name}" +
                                $" - <{parameterToSet.Definition.Name}> - {_storageTypeTarget} - {Message}";
                            _loggerShow.Info(message);
                        }
                        //code
                    }
                    else
                    {
                        MessageBox.Show("Невозможно перенести значение параметра типа в параметр экземпляра");
                        //Error
                    }
                }
                else //INSTANCE 
                {
                    int index = 0;
                    foreach (ElementId elementId in listofElementsTarget)
                    {
                        index++;
                        string messageToSet = Message;

                        Element elementToSet = _doc.GetElement(elementId); // экземпляры элементы
                        Parameter parameterToSet = dictOfTargParams[elementId]; // параметры к экземплярам элементов

                        var isEmpty = IsTargetValueEmpty(parameterToSet); //log

                        if (_doc.GetElement(dictOfEl.Keys.First()).GetTypeId().IntegerValue == -1)
                        {
                            var Value = dictOfEl[elementToSet.GetTypeId()];
                            if (Value.GetType() == typeof(string)
                                && (_rewrite == true
                                || (_rewrite == false && isEmpty == true)))
                            {
                                SetValueFromString(_storageTypeInitial, _storageTypeTarget, parameterToSet, Value as string);
                            }
                            else if (Value.GetType() == typeof(int)
                                && (_rewrite == true
                                || (_rewrite == false && isEmpty == true)))
                            {
                                SetValueFromInteger(_storageTypeInitial, _storageTypeTarget, parameterToSet, (int)Value);
                            }
                            else if (Value.GetType() == typeof(double)
                                && (_rewrite == true
                                || (_rewrite == false && isEmpty == true)))
                            {
                                SetValueFromDouble(_storageTypeInitial, _storageTypeTarget, parameterToSet, (double)Value);
                            }
                            else if (Value.GetType() == typeof(ElementId)
                                && (_rewrite == true
                                || (_rewrite == false && isEmpty == true)))
                            {
                                SetValueFromElementId(_storageTypeInitial, _storageTypeTarget, parameterToSet, Value as ElementId);
                            }
                            else
                            {
                                messageToSet = "пареметр имел значение";
                                //MessageBox.Show("erreer");
                            }
                            //строка для лога
                            string message = $"{index} - {elementId} - {elementToSet.Name}" +
                                $" - <{parameterToSet.Definition.Name}> - {_storageTypeTarget} - {messageToSet}";
                            _loggerShow.Info(message);
                        }
                        else
                        {
                            var Value = dictOfEl[elementId]; // ТУТ ОШИБКА (т.к. элементИд закидывается экземпляра, а надо - типа
                            if (Value.GetType() == typeof(string)
                                && (_rewrite == true
                                || (_rewrite == false && isEmpty == true)))
                            {
                                SetValueFromString(_storageTypeInitial, _storageTypeTarget, parameterToSet, Value as string);
                            }
                            else if (Value.GetType() == typeof(int)
                                && (_rewrite == true
                                || (_rewrite == false && isEmpty == true)))
                            {
                                SetValueFromInteger(_storageTypeInitial, _storageTypeTarget, parameterToSet, (int)Value);
                            }
                            else if (Value.GetType() == typeof(double)
                                && (_rewrite == true
                                || (_rewrite == false && isEmpty == true)))
                            {
                                SetValueFromDouble(_storageTypeInitial, _storageTypeTarget, parameterToSet, (double)Value);
                            }
                            else if (Value.GetType() == typeof(ElementId)
                                && (_rewrite == true
                                || (_rewrite == false && isEmpty == true)))
                            {
                                SetValueFromElementId(_storageTypeInitial, _storageTypeTarget, parameterToSet, Value as ElementId);
                            }
                            else
                            {
                                messageToSet = "пареметр имел значение";
                                //MessageBox.Show("erreer");
                            }
                            //строка для лога
                            string message = $"{index} - {elementId} - {elementToSet.Name}" +
                                $" - <{parameterToSet.Definition.Name}> - {_storageTypeTarget} - {messageToSet}";
                            _loggerShow.Info(message);
                        }
                    }
                }
                t.Commit();
            }
        }

        private static bool IsTargetValueEmpty(Parameter param)
        {
            if (param.AsValueString() == null || param.AsValueString() == "" || param.AsValueString() == string.Empty)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
