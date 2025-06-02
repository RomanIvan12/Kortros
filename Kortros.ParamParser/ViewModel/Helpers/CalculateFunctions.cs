using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Kortros.ParamParser.Model;
using Kortros.ParamParser.ViewModel.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;


namespace Kortros.ParamParser.ViewModel.Helpers
{
    public class CalculateFunctions
    {
        #region Выборка элементов
        //Функиция сворачивает окно и даёт выбрать ээлементы
        public static List<Element> SelectElements(UIDocument uidoc, Document doc, List<BuiltInCategory> categories, Window window)
        {
            List<Element> selectedElements = new List<Element>();

            ElementSelectionFilter selectionFilter = new ElementSelectionFilter(categories);
            //СКРЫТЬ ОКНО

            window.Hide();
            try
            {
                IList<Reference> references = uidoc.Selection.PickObjects(ObjectType.Element, selectionFilter);
                foreach (Reference reference in references)
                {
                    selectedElements.Add(doc.GetElement(reference.ElementId));
                }
                window.Show();
                return selectedElements;

            }
            catch (Exception ex)
            {
                window.Show();
                return null;
            }
        }


        // Получить выбранные ДО запуска приложения элементы
        public static List<Element> GetSelectedElements(UIDocument uidoc, List<BuiltInCategory> allowedCategories)
        {
            List<Element> selectedElements = new List<Element>();

            ICollection<ElementId> selectedIds = uidoc.Selection.GetElementIds();

            Document doc = uidoc.Document;

            List<int> allowedCategoriesIds = allowedCategories.Select(i => (int)i).ToList();

            foreach (ElementId element in selectedIds)
            {
                if (allowedCategoriesIds.Contains(doc.GetElement(element).Category.Id.IntegerValue))
                {
                    selectedElements.Add(doc.GetElement(element));
                }
            }
            return selectedElements;
        }



        // Получить элементы на текущем 3Двиде, если 3Д вид не выбран - идет выбор элементов на Navisworks, если и его нет, то выскакивает инфокно
        public static List<Element> GetElementsOnActiveView(Document doc, List<BuiltInCategory> allowedCategories)
        {
            List<Element> selectedElements = new List<Element>();

            Autodesk.Revit.DB.View activeView = doc.ActiveView;

            if (activeView is View3D)
            {
                foreach (var category in allowedCategories)
                {
                    foreach (var element in new FilteredElementCollector(doc, activeView.Id).OfCategory(category).WhereElementIsNotElementType().ToElements().ToList())
                    {
                        selectedElements.Add(element);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите 3D вид");
                return null;
            }
            return selectedElements;
        }

        public static List<Element> GetAllElements(Document doc, List<BuiltInCategory> allowedCategories)
        {
            List<Element> selectedElements = new List<Element>();

            foreach (var category in allowedCategories)
            {
                foreach (var element in new FilteredElementCollector(doc).OfCategory(category).WhereElementIsNotElementType().ToElements().ToList())
                {
                    selectedElements.Add(element);
                }
            }
            return selectedElements;
        }
        #endregion



        public static void SetValue(Document doc, List<Element> elements, Config currentConfig, out List<DataItem> itemList) // входные - CategoryItem, список ParameterStack
        {
            itemList = new List<DataItem>();
            using (Transaction transaction = new Transaction(doc, "ELEM"))
            {
                transaction.Start();
                // Получаем сгруппированный словарь <BiuiltInCategory, List<Element>
                Dictionary<BuiltInCategory, List<Element>> dictionaryToParse = elements.GroupBy(e => (BuiltInCategory)e.Category.Id.IntegerValue)
                .ToDictionary(g => g.Key, g => g.ToList());

                foreach (KeyValuePair<BuiltInCategory, List<Element>> pair in dictionaryToParse)
                {
                    string name = pair.Key.ToString();
                    CategoryItem categoryItem = DatabaseHelper.Read<CategoryItem>().FirstOrDefault(item => item.ConfigId == currentConfig.Id && item.Name == name);
                    List<ParamStack> stacks = DatabaseHelper.Read<ParamStack>()
                        .Where(i => i.CategoryItemId == categoryItem.Id && i.IsCorrect)
                        .ToList();

                    foreach (Element element in pair.Value)
                    {
                        Element elementType = doc.GetElement(element.GetTypeId());
                        foreach (ParamStack paramStack in stacks)
                        {
                            DataItem item = new DataItem()
                            {
                                ElementId = element.Id.IntegerValue,
                                ElementName = element.get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM).AsValueString(),
                                InitParameterName = paramStack.NameInit,
                                TargParameterName = paramStack.NameTarg,
                            };

                            DataItem updatedItem = new DataItem();

                            if (paramStack.IsTypeInit)
                            {
                                if (paramStack.IsTypeTarg)
                                {
                                    // элемент для инит - тип, для тарг - тип 
                                    // элемент для гетсет - тип
                                    Parameter parInit = GetInitParameterFromStack(elementType, paramStack);
                                    Parameter parTarg = GetTargParameterFromStack(elementType, paramStack);
                                    if (element.GroupId.IntegerValue == -1)
                                    {
                                        GetSetParameter(elementType, parInit, parTarg, out updatedItem);
                                        item.InitValue = updatedItem.InitValue;
                                        item.TargValue = updatedItem.TargValue;
                                        item.Status = updatedItem.Status;
                                    }
                                    else
                                    {
                                        item.Status = ItemStatus.NotCompleted;
                                        item.Message = $"Элемент в группе: {doc.GetElement(element.GroupId).Name}";
                                    }
                                }
                                else if (!paramStack.IsTypeTarg)
                                {
                                    // элемент для инит - тип, для тарг - экз 
                                    // элемент для гетсет - экз
                                    Parameter parInit = GetInitParameterFromStack(elementType, paramStack);
                                    Parameter parTarg = GetTargParameterFromStack(element, paramStack);
                                    if (element.GroupId.IntegerValue == -1)
                                    {
                                        GetSetParameter(element, parInit, parTarg, out updatedItem);
                                        item.InitValue = updatedItem.InitValue;
                                        item.TargValue = updatedItem.TargValue;
                                        item.Status = updatedItem.Status;
                                    }
                                    else
                                    {
                                        item.Status = ItemStatus.NotCompleted;
                                        item.Message = $"Элемент в группе: {doc.GetElement(element.GroupId).Name}";
                                    }
                                }
                            }
                            else
                            {
                                // элемент для инит - экз, для тарг - экз
                                // элемент для гетсет - экз
                                Parameter parInit = GetInitParameterFromStack(element, paramStack);
                                Parameter parTarg = GetTargParameterFromStack(element, paramStack);
                                if (element.GroupId.IntegerValue == -1)
                                {
                                    GetSetParameter(element, parInit, parTarg, out updatedItem);
                                    item.InitValue = updatedItem.InitValue;
                                    item.TargValue = updatedItem.TargValue;
                                    item.Status = updatedItem.Status;
                                }
                                else
                                {
                                    item.Status = ItemStatus.NotCompleted;
                                    item.Message = $"Элемент в группе: {doc.GetElement(element.GroupId).Name}";
                                }
                            }
                            itemList.Add(item);
                        }
                    }
                }
                transaction.Commit();
                if (transaction.GetStatus() != TransactionStatus.Committed)
                {
                    Debug.WriteLine("Error");
                }
            }
        }

        // Получить параметр конкретного элемента
        public static Parameter GetInitParameterFromStack(Element element, ParamStack stack)
        {

            if (!string.IsNullOrEmpty(stack.GuidInit))
            {
                //значит параметр общий
                return element.get_Parameter(new Guid(stack.GuidInit));
            }
            else if (!string.IsNullOrEmpty(stack.DefinitionInit))
            {
                BuiltInParameter builtInParameter = (BuiltInParameter)Enum.Parse(typeof(BuiltInParameter), stack.DefinitionInit); // ПРОВЕРИТЬ
                return element.get_Parameter(builtInParameter);
            }
            return null;
        }

        public static Parameter GetTargParameterFromStack(Element element, ParamStack stack)
        {

            if (!string.IsNullOrEmpty(stack.GuidTarg))
            {
                //значит параметр общий
                return element.get_Parameter(new Guid(stack.GuidTarg));
            }
            else if (!string.IsNullOrEmpty(stack.DefinitionTarg))
            {
                BuiltInParameter builtInParameter = (BuiltInParameter)Enum.Parse(typeof(BuiltInParameter), stack.DefinitionTarg); // ПРОВЕРИТЬ
                return element.get_Parameter(builtInParameter);
            }
            return null;
        }

        // Вставить значение в зависимости от типа данных параметра
        public static void GetSetParameter(Element element, Parameter initParameter, Parameter targParameter, out DataItem item)
        {
            item = new DataItem();

            //using (Transaction t = new Transaction(RunCommand.Doc, "Set parameter"))
            //{
            //    t.Start();
                // INIT - ELEMENT_ID
            if (initParameter.StorageType == StorageType.ElementId)
            {
                int value = initParameter.AsElementId().IntegerValue;
                item.InitValue = value;

                string strValue = initParameter.AsValueString();

                if (targParameter.StorageType == StorageType.String)
                {
                    var status = element.get_Parameter(targParameter.GUID).Set(strValue);
                    item.TargValue = strValue;
                    item.Status = status ? ItemStatus.Done : ItemStatus.Cancelled;
                }
                else if (targParameter.StorageType == StorageType.Double)
                {
                    var status = element.get_Parameter(targParameter.GUID).Set((double)value);
                    item.TargValue = (double)value;
                    item.Status = status ? ItemStatus.Done : ItemStatus.Cancelled;
                }
                else if (targParameter.StorageType == StorageType.Integer)
                {
                    var status = element.get_Parameter(targParameter.GUID).Set(value);
                    item.TargValue = value;
                    item.Status = status ? ItemStatus.Done : ItemStatus.Cancelled;
                }
            }
            // INIT - INTEGER
            else if (initParameter.StorageType == StorageType.Integer)
            {
                int value = initParameter.AsInteger();
                item.InitValue = value;

                if (targParameter.StorageType == StorageType.Integer)
                {
                    var status = element.get_Parameter(targParameter.GUID).Set(value);
                    item.TargValue = value;
                    item.Status = status ? ItemStatus.Done : ItemStatus.Cancelled;
                }
                else if (targParameter.StorageType == StorageType.Double)
                {
                    var status = element.get_Parameter(targParameter.GUID).Set((double)value);
                    item.TargValue = (double)value;
                    item.Status = status ? ItemStatus.Done : ItemStatus.Cancelled;
                }
                else if (targParameter.StorageType == StorageType.String)
                {
                    var status = element.get_Parameter(targParameter.GUID).Set(value.ToString());
                    item.TargValue = value.ToString();
                    item.Status = status ? ItemStatus.Done : ItemStatus.Cancelled;
                }
            }

            //INIT - DOUBLE
            else if (initParameter.StorageType == StorageType.Double)
            {
                double value = initParameter.AsDouble();
                double convValue = UnitUtils.ConvertFromInternalUnits(value, initParameter.GetUnitTypeId());

                item.InitValue = convValue;
                if (targParameter.StorageType == StorageType.Integer && convValue % 1 == 0)
                {
                    var status = element.get_Parameter(targParameter.GUID).Set((int)convValue);
                    item.TargValue = (int)convValue;
                    item.Status = status ? ItemStatus.Done : ItemStatus.Cancelled;
                }

                else if (targParameter.StorageType == StorageType.String)
                {
                    var status = element.get_Parameter(targParameter.GUID).Set(initParameter.AsValueString());
                    item.TargValue = initParameter.AsValueString();
                    item.Status = status ? ItemStatus.Done : ItemStatus.Cancelled;
                }

                else if (targParameter.StorageType == StorageType.Double)
                {
                    var status = element.get_Parameter(targParameter.GUID).Set(convValue);
                    item.TargValue = convValue;
                    item.Status = status ? ItemStatus.Done : ItemStatus.Cancelled;
                }
            }
            //INIT - STRING
            else if (initParameter.StorageType == StorageType.String)
            {
                string value = initParameter.AsString();
                item.InitValue = value;

                NumberStyles style = NumberStyles.Number | NumberStyles.AllowCurrencySymbol | NumberStyles.AllowDecimalPoint;
                CultureInfo culture = CultureInfo.CurrentCulture;

                if (targParameter.StorageType == StorageType.String)
                {
                    var status = element.get_Parameter(targParameter.GUID).Set(value);
                    item.TargValue = value;
                    item.Status = status ? ItemStatus.Done : ItemStatus.Cancelled;
                }
                else if (targParameter.StorageType == StorageType.Integer && int.TryParse(value, out _))
                {
                    var status = element.get_Parameter(targParameter.GUID).Set(int.Parse(value));
                    item.TargValue = int.Parse(value);
                    item.Status = status ? ItemStatus.Done : ItemStatus.Cancelled;
                }
                else if (targParameter.StorageType == StorageType.Double && double.TryParse(value, style, culture, out _))
                {
                    var status = element.get_Parameter(targParameter.GUID).Set(double.Parse(value));
                    item.TargValue = double.Parse(value);
                    item.Status = status ? ItemStatus.Done : ItemStatus.Cancelled;
                }
                
                //t.Commit();
                //if (t.GetStatus() == TransactionStatus.Committed)
                //{
                //    Debug.WriteLine("Error");
                //}
            }
        }

        public static void GetSetParameter2222222(Element element, Parameter initParameter, Parameter targParameter)
        {
            using (Transaction t = new Transaction(RunCommand.Doc, "Set parameter"))
            {
                t.Start();
                // INIT - ELEMENT_ID
                if (initParameter.StorageType == StorageType.ElementId)
                {
                    int value = initParameter.AsElementId().IntegerValue;
                    string strValue = initParameter.Definition.Name;
                    if (targParameter.StorageType == StorageType.String)
                        element.get_Parameter(targParameter.GUID).Set(strValue);
                    else if (targParameter.StorageType == StorageType.Double)
                        element.get_Parameter(targParameter.GUID).Set((double)value);
                    else if (targParameter.StorageType == StorageType.Integer)
                        element.get_Parameter(targParameter.GUID).Set(value);
                }


                // INIT - INTEGER
                else if (initParameter.StorageType == StorageType.Integer)
                {
                    int value = initParameter.AsInteger();

                    if (targParameter.StorageType == StorageType.Integer)
                        element.get_Parameter(targParameter.GUID).Set(value);
                    else if (targParameter.StorageType == StorageType.Double)
                        element.get_Parameter(targParameter.GUID).Set((double)value);
                    else if (targParameter.StorageType == StorageType.String)
                        element.get_Parameter(targParameter.GUID).Set(value.ToString());
                }

                //INIT - DOUBLE
                else if (initParameter.StorageType == StorageType.Double)
                {
                    double value = initParameter.AsDouble();
                    double convValue = UnitUtils.ConvertFromInternalUnits(value, initParameter.GetUnitTypeId());
                    if (targParameter.StorageType == StorageType.Integer && convValue % 1 == 0)
                        element.get_Parameter(targParameter.GUID).Set((int)convValue);

                    else if (targParameter.StorageType == StorageType.String)
                        element.get_Parameter(targParameter.GUID).Set(initParameter.AsValueString());


                    else if (targParameter.StorageType == StorageType.Double)
                    {
                        element.get_Parameter(targParameter.GUID).Set(convValue);
                    }
                }
                //INIT - STRING
                else if (initParameter.StorageType == StorageType.String)
                {
                    string value = initParameter.AsString();

                    NumberStyles style = NumberStyles.Number | NumberStyles.AllowCurrencySymbol | NumberStyles.AllowDecimalPoint;
                    CultureInfo culture = CultureInfo.CurrentCulture;

                    if (targParameter.StorageType == StorageType.String)
                        element.get_Parameter(targParameter.GUID).Set(value);
                    else if (targParameter.StorageType == StorageType.Integer && int.TryParse(value, out _))
                        element.get_Parameter(targParameter.GUID).Set(int.Parse(value));
                    else if (targParameter.StorageType == StorageType.Double && double.TryParse(value, style, culture, out _))
                        element.get_Parameter(targParameter.GUID).Set(double.Parse(value));
                }

                t.Commit();
                if (t.GetStatus() == TransactionStatus.Committed)
                {
                    Debug.WriteLine("Error");
                }
            }
        }
    }
}
