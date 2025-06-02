using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Kortros.ParamParser.ViewModel.Helpers;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;

namespace TestForParser
{
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class TestCommand : IExternalCommand
    {
        private readonly List<BuiltInCategory> categories = new List<BuiltInCategory> {
            BuiltInCategory.OST_Walls, // +
            BuiltInCategory.OST_DuctAccessory, // +
            BuiltInCategory.OST_PipeAccessory, // +
            BuiltInCategory.OST_DuctCurves, // +
            BuiltInCategory.OST_PlaceHolderDucts, // +
            BuiltInCategory.OST_DuctTerminal, // +
            BuiltInCategory.OST_LightingDevices, // +
            BuiltInCategory.OST_FlexDuctCurves, // +
            BuiltInCategory.OST_FlexPipeCurves, // +
            BuiltInCategory.OST_DataDevices, // +
            BuiltInCategory.OST_Areas, // +
            BuiltInCategory.OST_HVAC_Zones,  // +????
            BuiltInCategory.OST_CableTray,  // +
            BuiltInCategory.OST_Conduit, // +
            BuiltInCategory.OST_DuctLinings, // +
            BuiltInCategory.OST_DuctInsulations, // +
            BuiltInCategory.OST_PipeInsulations, // +
            BuiltInCategory.OST_GenericModel, // +
            BuiltInCategory.OST_MechanicalEquipment, // +
            BuiltInCategory.OST_Windows, // +
            BuiltInCategory.OST_Doors, // +
            BuiltInCategory.OST_LightingFixtures, // +
            BuiltInCategory.OST_SecurityDevices,
            BuiltInCategory.OST_FireAlarmDevices,
            BuiltInCategory.OST_Rooms,  // + цепи и комплект мебели
            BuiltInCategory.OST_Wire, // +
            BuiltInCategory.OST_MEPSpaces, // +
            BuiltInCategory.OST_PlumbingFixtures, // +
            BuiltInCategory.OST_DuctSystem,
            BuiltInCategory.OST_DuctFitting, // +
            BuiltInCategory.OST_CableTrayFitting, // +
            BuiltInCategory.OST_ConduitFitting, // +
            BuiltInCategory.OST_PipeFitting, // +
            BuiltInCategory.OST_Sprinklers, // +
            BuiltInCategory.OST_TelephoneDevices, // +
            BuiltInCategory.OST_PlaceHolderPipes, // +
            BuiltInCategory.OST_PipingSystem, // +
            BuiltInCategory.OST_PipeCurves, // +
            BuiltInCategory.OST_NurseCallDevices, // +
            BuiltInCategory.OST_CommunicationDevices, // +
            BuiltInCategory.OST_CableTrayRun,
            BuiltInCategory.OST_Mass, // +
            BuiltInCategory.OST_Parts, // +
            BuiltInCategory.OST_ElectricalFixtures, // +
            BuiltInCategory.OST_ElectricalCircuit,
            BuiltInCategory.OST_ElectricalEquipment, // +
            BuiltInCategory.OST_DetailComponents,
            BuiltInCategory.OST_Furniture, // +
            BuiltInCategory.OST_FurnitureSystems, // +
            BuiltInCategory.OST_FireAlarmDevices, // +
            BuiltInCategory.OST_SecurityDevices, // +
        };

        private static StringBuilder _sb = new StringBuilder();

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            //DatabaseHelper.DeleteTable<ParameterItemCommon>("ParameterItemCommon");
            //CreateParameterItemCommon(doc);
            foreach (var i in categories)
            {
                CreateParameterItem(doc, i);
            }

            MessageBox.Show(_sb.ToString());
            return Result.Succeeded;
        }

        public static List<ParameterItemCommon> CreateParameterItemCommon(Document doc)
        {
            List<ParameterItemCommon> parameters = new List<ParameterItemCommon>();

            DefinitionBindingMapIterator map = doc.ParameterBindings.ForwardIterator();



            var existedCategories = DatabaseHelper.Read<CategoryItem>();
            List<string> existedNames = existedCategories.Select(i => i.Name).Distinct().ToList();

            //ShowList(existedNames);

            // Перебираю параметры в документе,
            while (map.MoveNext())
            {
                InternalDefinition intDef = (InternalDefinition)map.Key;

                ElementBinding xxx = map.Current as ElementBinding;
                CategorySet set = xxx.Categories;

                string dataType = intDef.GetDataType().TypeId;

                foreach (Category c in set)
                {
                    BuiltInCategory builtIn = (BuiltInCategory)c.Id.IntegerValue;

                    if (existedNames.Contains(builtIn.ToString())) // добавить параметрстак, если его категория есть в списке категорий у уже созданных конфигов
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
                            parameters.Add(item);
                        }
                    }
                }
            }
            return parameters;
        }

        
        public void CreateParameterItem(Document doc, BuiltInCategory reference)
        {
            Category category = Category.GetCategory(doc, reference);
            _sb.AppendLine($"       Категория: {category.Name}");
            Element instance = new FilteredElementCollector(doc).OfCategoryId(category.Id).WhereElementIsNotElementType().ToElements().FirstOrDefault();
            if (instance != null)
            {
                int counter = 0;
                ParameterSet parameterset = instance.Parameters;
                foreach (Parameter parameter in parameterset)
                {
                    if (parameter.Id.IntegerValue < 0)
                    {
                        counter++;

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
                _sb.AppendLine($"Instance - количество параметров = {counter.ToString()}");
            }

            Element type = new FilteredElementCollector(doc).OfCategoryId(category.Id).WhereElementIsElementType().ToElements().FirstOrDefault();
            if (type != null)
            {
                int counter = 0;
                ParameterSet parametersetType = type.Parameters;
                foreach (Parameter parameter in parametersetType)
                {
                    if (parameter.Id.IntegerValue < 0)
                    {
                        counter++;
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
                            IsType = true,
                            IsReadOnly = parameter.IsReadOnly,
                        };

                        DatabaseHelper.Insert(item);
                    }
                }
                _sb.AppendLine($"Type - количество параметров = {counter.ToString()}");
            }
            _sb.AppendLine("_______________________");
        }
        

        public static void ShowList<T>(IEnumerable<T> items)
        {
            string message = string.Join("\n", items);
            MessageBox.Show(message, "Список элементов");
        }
    }

    public class ParameterItemCommon
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public string Category { get; set; }
        public bool IsBuiltIn { get; set; }
        public string Guid { get; set; }
        public string Name { get; set; }
        public string Definition { get; set; }
        public string ParameterType { get; set; }
        public string StorageType { get; set; }
        public bool IsType { get; set; }
        public bool IsReadOnly { get; set; }
    }

    public class ParameterItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public string Category { get; set; }
        public bool IsBuiltIn { get; set; }
        public string Guid { get; set; }
        public string Name { get; set; }
        public string Definition { get; set; }
        public string ParameterType { get; set; }
        public string StorageType { get; set; }
        public bool IsType { get; set; }
        public bool IsReadOnly { get; set; }
    }
    public class CategoryItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public int ConfigId { get; set; }
        public string Name { get; set; } // Содержит имя вида "OST_{category}"
        public bool IsChecked { get; set; }
    }

}
