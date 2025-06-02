using System;
using Autodesk.Revit.DB;
using Kortros.ParamParser.Model;

using System.Linq;

namespace Kortros.ParamParser.ViewModel.Helpers
{
    public class ParameterItemHelper
    {
        /// <summary>
        /// Функция добовления ParameterItemCommon для одной категории
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static void CreateParameterItemCommon(Document doc, string builtInCategoryString)
        {
            // Перебираю параметры в документе,
            DefinitionBindingMapIterator map = doc.ParameterBindings.ForwardIterator();
            while (map.MoveNext())
            {
                InternalDefinition intDef = (InternalDefinition)map.Key;

                ElementBinding xxx = map.Current as ElementBinding;
                CategorySet set = xxx.Categories;

                string dataType = intDef.GetDataType().TypeId;

                foreach (Category c in set)
                {
                    BuiltInCategory builtIn = (BuiltInCategory)c.Id.IntegerValue;
                    if (builtInCategoryString == builtIn.ToString()) // добавить параметрстак, если его категория есть в списке категорий у уже созданных конфигов
                    {
                        Element element = doc.GetElement(intDef.Id); // элемент параметра
                        // Определяю общий или нет
                        if (element is SharedParameterElement) 
                        {
                            SharedParameterElement sharedParEl = element as SharedParameterElement;
                            ParameterItemCommon item = new ParameterItemCommon()
                            {
                                Category = builtIn.ToString(),
                                IsBuiltIn = false,
                                Guid = sharedParEl.GuidValue.ToString(),
                                Name = sharedParEl.Name,
                                Definition = string.Empty,
                                ParameterType = intDef.GetDataType().TypeId,
                                //StorageType вносится при создании CategoryItem,
                            };

                            if (dataType.Contains("string"))
                            {
                                item.StorageType = "String";
                            }
                            else if (dataType.Contains("bool") || dataType.Contains("int"))
                            {
                                item.StorageType = "Integer";
                            }
                            else
                            {
                                item.StorageType = "Double";
                            }

                            //Определяю тип или экземпляр
                            if (map.Current is TypeBinding)
                            {
                                item.IsType = true;
                            }
                            else if (map.Current is InstanceBinding)
                            {
                                item.IsType = false;
                            }
                            DatabaseHelper.Insert(item);
                        }
                    }
                }
            }
        }

        // Функция для создания таблицы ParameterItem, не используется в работе, только для первоначального создания таблицы
        public static void CreateParameterItem(Document doc, BuiltInCategory reference)
        {
            Category category = Category.GetCategory(doc, reference);

            Element instance = new FilteredElementCollector(doc).OfCategoryId(category.Id).WhereElementIsNotElementType().ToElements().FirstOrDefault();
            if (instance != null)
            {
                ParameterSet parameterset = instance.Parameters;
                foreach (Parameter parameter in parameterset)
                {
                    if (parameter.Id.IntegerValue < 0)
                    {
                        var IntDef = parameter.Definition as InternalDefinition;
                        ParameterItem item = new ParameterItem()
                        {
                            Category = reference.ToString(),
                            IsBuiltIn = true,
                            Guid = string.Empty,
                            Name = parameter.Definition.Name,
                            Definition = IntDef.BuiltInParameter.ToString(),
                            ParameterType = parameter.Definition.GetDataType().TypeId,
                            StorageType = parameter.StorageType.ToString(),
                            IsType = false,
                            IsReadOnly = parameter.IsReadOnly,
                        };
                        DatabaseHelper.Insert(item);
                    }
                }
            }
            var a = DatabaseHelper.Read<ParameterItem>().Select(i => i.Definition);

            Element type = new FilteredElementCollector(doc).OfCategoryId(category.Id).WhereElementIsElementType().ToElements().FirstOrDefault();
            if (type != null)
            {
                ParameterSet parametersetType = type.Parameters;
                foreach (Parameter parameter in parametersetType)
                {
                    if (parameter.Id.IntegerValue < 0)
                    {
                        var IntDef = parameter.Definition as InternalDefinition;
                        if (!a.Contains(IntDef.BuiltInParameter.ToString()))
                        {
                            ParameterItem item = new ParameterItem()
                            {
                                Category = reference.ToString(),
                                IsBuiltIn = true,
                                Guid = string.Empty,
                                Name = parameter.Definition.Name,
                                Definition = IntDef.BuiltInParameter.ToString(),
                                ParameterType = parameter.Definition.GetDataType().TypeId,
                                StorageType = parameter.StorageType.ToString(),
                                IsType = true,
                                IsReadOnly = parameter.IsReadOnly,
                            };

                            DatabaseHelper.Insert(item);
                        }
                    }
                }
            }
        }

        public static bool DoesSharedParameterExists(Document doc, string category, string guid)
        {
            // Перебираю параметры в документе,
            DefinitionBindingMapIterator map = doc.ParameterBindings.ForwardIterator();
            while (map.MoveNext())
            {
                InternalDefinition intDef = (InternalDefinition)map.Key;

                ElementBinding xxx = map.Current as ElementBinding;
                CategorySet set = xxx.Categories;


                foreach (Category c in set)
                {
                    BuiltInCategory builtInCategory;
                    if (Enum.TryParse(category, out builtInCategory))
                    {
                        Category cat = Category.GetCategory(doc, builtInCategory);
                        if (cat.Id == c.Id)
                        {
                            BuiltInCategory builtIn = (BuiltInCategory)c.Id.IntegerValue;
                            if (category == builtIn.ToString()) // добавить параметрстак, если его категория есть в списке категорий у уже созданных конфигов
                            {
                                Element element = doc.GetElement(intDef.Id); // элемент параметра
                                // Определяю общий или нет
                                if (element is SharedParameterElement)
                                {
                                    SharedParameterElement sharedParEl = element as SharedParameterElement;

                                    if (sharedParEl.GuidValue == new Guid(guid))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}
