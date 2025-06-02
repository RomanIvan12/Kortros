using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Application = Autodesk.Revit.ApplicationServices.Application;
using Binding = Autodesk.Revit.DB.Binding;

namespace Kortros.Architecture.Apartments.Extensions
{
    public class CheckCommonParameters
    {
        private static Dictionary<string, string> parameterSet = new Dictionary<string, string>
        {

            {"ADSK_Этаж", "9eabf56c-a6cd-4b5c-a9d0-e9223e19ea3f" },
            {"ADSK_Номер квартиры", "10fb72de-237e-4b9c-915b-8849b8907695" },
            {"ADSK_Количество комнат", "f52108e1-0813-4ad6-8376-a38a1a23a55b" },
            {"ADSK_Тип квартиры", "78e3b89c-eb68-4600-84a7-c523de162743" },
            {"ADSK_Тип помещения", "56eb1705-f327-4774-b212-ef9ad2c860b0" },
            {"ADSK_Индекс квартиры", "a2985e5c-b28e-416a-acf6-7ab7e4ee6d86" },
            {"ADSK_Номер здания", "eaa57141-68d3-4f89-8272-246328f8e77b" },
            {"ADSK_Номер помещения квартиры", "69890ae1-d66e-4fe9-aced-024c27719f53" },
            {"ADSK_Площадь квартиры", "d3035d0f-b738-4407-a0e5-30787b92fa49" },
            {"ADSK_Площадь квартиры жилая", "178e222b-903b-48f5-8bfc-b624cd67d13c" },
            {"ADSK_Площадь квартиры общая", "af973552-3d15-48e3-aad8-121fe0dda34e" },
            {"ADSK_Коэффициент площади", "066eab6d-c348-4093-b0ca-1dfe7e78cb6e" },
            {"ADSK_Площадь с коэффициентом", "9a0c14fa-b48c-40ce-8f95-6954dfe2a399" },
            {"ADSK_Номер секции", "b59a3474-a5f4-430a-b087-a20f1a4eb57e" }
        };


        //Просто показ информации
        public static void CheckExistedCommonParameters(Document doc)
        {
            DefinitionBindingMapIterator mapIterator = doc.ParameterBindings.ForwardIterator();

            StringBuilder sb = new StringBuilder();
            // перебираю параметры

            List<string> namesInternalDefinitions = new List<string>();

            while (mapIterator.MoveNext())
            {

                InternalDefinition internalDefinition = (InternalDefinition)mapIterator.Key;
                namesInternalDefinitions.Add(internalDefinition.Name);

                if (parameterSet.ContainsKey(internalDefinition.Name))
                {
                    //TODO: Проверяю, что он инстанс и назначен помещению
                    ElementBinding elementBinding = mapIterator.Current as ElementBinding;
                    CategorySet categorySet = elementBinding.Categories;

                    if (mapIterator.Current is InstanceBinding && categorySet.Contains(Category.GetCategory(doc, BuiltInCategory.OST_Rooms)))
                    {
                        sb.AppendLine(internalDefinition.Name + " есть в списке параметров (+)");
                        continue;
                    }
                    else
                    {
                        //TODO: Обработать ошибку
                        sb.AppendLine(internalDefinition.Name + " есть в списке параметров, но он либо относится к типу, либо не содержит категории Помещения");
                    }
                }
            }
            foreach (string item in parameterSet.Keys)
            {
                if (!namesInternalDefinitions.Contains(item))
                {
                    sb.AppendLine(item + " ОТСУТСТВУЕТ");
                }
            }

            MessageBox.Show(sb.ToString(), "Warning");
        }


        public static void AddSharedParameters(Application app, Document doc, List<Category> cats, string Name, bool isInstance=true)
        {
            CategorySet categorySet = app.Create.NewCategorySet();

            foreach (Category category in cats)
            {
                categorySet.Insert(category);
            }

            DefinitionFile definitionFile = app.OpenSharedParameterFile();
            DefinitionGroup definitionGroup = definitionFile.Groups.get_Item("02 Обязательные АРХИТЕКТУРА");

            BindingMap bindingMap = doc.ParameterBindings;

            foreach (Definition paramDef in definitionGroup.Definitions)
            {
                if (paramDef.Name == Name && !bindingMap.Contains(paramDef))
                {
                    if (isInstance)
                    {
                        InstanceBinding instanceBinding = app.Create.NewInstanceBinding(categorySet);
                        bindingMap.Insert(paramDef, instanceBinding, BuiltInParameterGroup.INVALID);
                    }
                    else
                    {
                        TypeBinding typeBinding = app.Create.NewTypeBinding(categorySet);
                        bindingMap.Insert(paramDef, typeBinding, BuiltInParameterGroup.INVALID);
                    }
                }
            }
        }
    }
}
