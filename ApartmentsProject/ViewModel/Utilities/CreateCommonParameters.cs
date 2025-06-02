using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using ApartmentsProject.Models;
using System.IO;
using System.Reflection;

namespace ApartmentsProject.ViewModel.Utilities
{
    public class CreateCommonParameters
    {
        public static void CreateCommon(Document doc, Application app, List<string> parameterToCreate)
        {
            // Get current SharedParameterFile path
            string filePath = app.SharedParametersFilename;

            string targetPath = ExtractFileToTemporaryFile("Apts_KRTRS.txt");

            LoadSharedParameterFile(app, targetPath);

            var categories = new List<BuiltInCategory>()
            {
                BuiltInCategory.OST_Rooms
            };

            List<Definition> externalDef = new List<Definition>();
                //GetExternalDefinition(app, "1. Квартирография", "KRT_Функциональное назначение"),
                //GetExternalDefinition(app, "1. Квартирография", "KRT_Тип Квартиры"),
                //GetExternalDefinition(app, "1. Квартирография", "KRT_№ Квартиры"),
                //GetExternalDefinition(app, "1. Квартирография", "KRT_Номер помещения"),
                //GetExternalDefinition(app, "1. Квартирография", "KRT_ID Квартиры"),
                //GetExternalDefinition(app, "1. Квартирография", "KRT_Тип помещения"),
                //GetExternalDefinition(app, "1. Квартирография", "KRT_Число жилых комнат"),
                //GetExternalDefinition(app, "1. Квартирография", "KRT_Площадь округлённая"),
                //GetExternalDefinition(app, "1. Квартирография", "KRT_Площадь с коэффициентом"),
                //GetExternalDefinition(app, "1. Квартирография", "KRT_Коэффициент площади"),
                //GetExternalDefinition(app, "1. Квартирография", "KRT_Толщина черновой отделки"),
                //GetExternalDefinition(app, "1. Квартирография", "KRT_Площадь жилая"),
                //GetExternalDefinition(app, "1. Квартирография", "KRT_Площадь квартиры"),
                //GetExternalDefinition(app, "1. Квартирография", "KRT_Площадь общая"),
                //GetExternalDefinition(app, "1. Квартирография", "KRT_Площадь общая без коэфф."),
                //GetExternalDefinition(app, "1. Квартирография", "KRT_Площадь неотапл. помещений с коэфф."),
                //GetExternalDefinition(app, "1. Квартирография", "KRT_Метка квартиры"),

            foreach (var item in parameterToCreate)
            {
                externalDef.Add(GetExternalDefinition(app, "1. Квартирография", item));
            }

            Binding binding = CreateParameterBinding(doc, categories);

            using (Transaction t = new Transaction(doc, "sss"))
            {
                t.Start();
                foreach (Definition definition in externalDef)
                {
                    CreateProjectParameter(doc, definition, binding, BuiltInParameterGroup.INVALID);
                }
                t.Commit();
            }
            LoadSharedParameterFile(app, filePath);
        }

        public static string ExtractFileToTemporaryFile(string fileName)
        {
            var tempFile = Path.Combine(Path.GetTempPath(), fileName);

            // Get current assembly
            Assembly assembly = Assembly.GetExecutingAssembly();

            // Resource name
            string resourceName = $"ApartmentsProject.Resources.{fileName}";

            using (Stream resourceStream = assembly.GetManifestResourceStream(resourceName))
            {
                if (resourceStream == null)
                {
                    throw new Exception("Embedded resource not found." + resourceName);
                }
                using (FileStream filestream = new FileStream(tempFile, FileMode.Create, FileAccess.ReadWrite))
                {
                    resourceStream.CopyTo(filestream);
                }
            }
            return tempFile;
        }
        
        public static void LoadSharedParameterFile(Application app, string path)
        {
            app.SharedParametersFilename = path;
        }

        public static Definition GetExternalDefinition(Application app, string groupName, string defName)
        {
            return app.OpenSharedParameterFile().Groups.get_Item(groupName).Definitions.get_Item(defName);
        }

        // Create Parameter Binding
        public static dynamic CreateParameterBinding(Document doc, List<BuiltInCategory> categories, bool IsTypeBinding = false)
        {
            Application app = doc.Application;
            CategorySet categorySet = app.Create.NewCategorySet();

            foreach (BuiltInCategory category in categories)
            {
                Category cat = Category.GetCategory(doc, category);
                categorySet.Insert(cat);
            }
            if (IsTypeBinding)
            {
                return app.Create.NewTypeBinding(categorySet);
            }
            return app.Create.NewInstanceBinding(categorySet);
        }

        // Create Project Parameter
        public static Definition CreateProjectParameter(Document doc, Definition definition, Binding binding, BuiltInParameterGroup group)
        {
            if (doc.ParameterBindings.Insert(definition, binding, group))
            {
                var iterator = doc.ParameterBindings.ForwardIterator();
                while (iterator.MoveNext())
                {
                    var internalDefinition = iterator.Key;
                    if (internalDefinition.Name == definition.Name)
                        return internalDefinition;
                }
            }
            return null;
        }
    }
}
