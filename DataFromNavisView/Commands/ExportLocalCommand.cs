using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Autodesk.Revit.DB;
using DataFromNavisView.Helpers;
using DataFromNavisView.Models;
using SQLite;

namespace DataFromNavisView.Commands
{
    public class ExportLocalCommand
    {
        public static void ExportCsv(Document doc)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            StringBuilder sb = new StringBuilder();
            StringBuilder sbExport = new StringBuilder();
            // получил уникальные экземпляры
            var linkInstances = new FilteredElementCollector(doc)
                .OfClass(typeof(RevitLinkInstance))
                .Cast<RevitLinkInstance>()
                .GroupBy(item => item.GetTypeId().IntegerValue)
                .Select(group => group.First())
                .ToList();

            var dataObject = new List<object>();

            foreach (var linkInstance in linkInstances)
            {
                var linkDoc = linkInstance.GetLinkDocument();
                if (linkDoc == null) continue;

                // Получаем 3д вид Navisworks
                View view = new FilteredElementCollector(linkDoc).OfClass(typeof(View3D))
                    .Cast<View>()
                    .Where(i => !i.IsTemplate)
                    .FirstOrDefault(view3d => view3d.Name == "Navisworks");

                if (view == null)
                {
                    sb.AppendLine($"{linkInstance.GetType().Name}: - проект не содержит вида Navisworks");
                    continue;
                }

                var linkedFileElementsNav = new FilteredElementCollector(linkDoc, view.Id)
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Where(x => x.Category != null);

                foreach (Element element in linkedFileElementsNav)
                {
                    if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Cameras)
                        continue;

                    ParameterSet parameterSet = element.Parameters;
                    foreach (Parameter parameter in parameterSet)
                    {
                        if (parameter.Id.IntegerValue == (int)BuiltInParameter.ELEM_CATEGORY_PARAM_MT)
                            continue;

                        List<object> myList = new List<object>(4);
                        myList.Add(linkInstance.Name.Substring(0, linkInstance.Name.IndexOf('.')));
                        myList.Add(element.Id.IntegerValue);
                        myList.Add(parameter.Definition.Name);
                        string parameterValue = "<none>";

                        switch (parameter.StorageType)
                        {
                            case StorageType.String:
                                parameterValue = string.IsNullOrEmpty(parameter.AsString()) ? "<null>" : parameter.AsString();
                                myList.Add(parameterValue);
                                dataObject.Add(myList);
                                break;

                            case StorageType.Double:
                                    
                                var rounding = UtilityFunctions.GetParameterAccuracy(doc, parameter);
                                int decimalPlaces = (int)Math.Abs(Math.Log10(rounding));
                                parameterValue = Math.Round(UnitUtils.ConvertFromInternalUnits(parameter.AsDouble(), parameter.GetUnitTypeId()), decimalPlaces).ToString();

                                myList.Add(parameterValue);
                                dataObject.Add(myList);
                                break;

                            case StorageType.ElementId:
                                parameterValue = parameter.AsValueString();
                                myList.Add(parameterValue);
                                dataObject.Add(myList);
                                break;

                            case StorageType.Integer:
                                parameterValue = parameter.AsInteger().ToString();
                                myList.Add(parameterValue);
                                dataObject.Add(myList);
                                break;
                            default:
                                myList.Add(parameterValue);
                                dataObject.Add(myList);
                                break;
                        }
                    }
                }
            }
            foreach (var item in dataObject)
            {
                if (item is List<object> row)
                {
                    sbExport.AppendLine(string.Join("\t", row));
                }
            }

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            File.WriteAllText(Path.Combine(desktopPath, "elements_data.csv"), sbExport.ToString(), Encoding.UTF8);

            stopwatch.Stop();
            // Получаем прошедшее время
            TimeSpan elapsedTime = stopwatch.Elapsed;

            sb.AppendLine($"Операция выполнена за {elapsedTime.TotalSeconds} секунд.");
            sb.AppendLine("Файл elements_data.csv сохранён на рабочий стол");
            MessageBox.Show(sb.ToString());
        }

        public static void ExportDb3(Document doc)
        {
            // db
            Stopwatch stopwatch = new Stopwatch();
            // Запускаем таймер
            stopwatch.Start();

            DatabaseHelper.DeleteTable<ParameterData>();
            DatabaseHelper.Read<ParameterData>();

            StringBuilder sb = new StringBuilder();
            // получил уникальные экземпляры
            var linkInstances = new FilteredElementCollector(doc)
                .OfClass(typeof(RevitLinkInstance))
                .Cast<RevitLinkInstance>()
                .GroupBy(item => item.GetTypeId().IntegerValue)
                .Select(group => group.First());

            using (SQLiteConnection conn = new SQLiteConnection(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "elements_data.db3")))
            {

                conn.BeginTransaction();

                foreach (var linkInstance in linkInstances)
                {
                    var linkDoc = linkInstance.GetLinkDocument();
                    if (linkDoc == null) continue;

                    // Получаем 3д вид Navisworks
                    View view = new FilteredElementCollector(linkDoc).OfClass(typeof(View3D))
                        .Cast<View>()
                        .Where(i => !i.IsTemplate)
                        .FirstOrDefault(view3d => view3d.Name == "Navisworks");

                    if (view == null)
                    {
                        sb.AppendLine($"{linkInstance.GetType().Name}: - проект не содержит вида Navisworks");
                        continue;
                    }

                    var linkedFileElementsNav = new FilteredElementCollector(linkDoc, view.Id)
                        .WhereElementIsNotElementType()
                        .ToElements()
                        .Where(x => x.Category != null);

                    foreach (Element element in linkedFileElementsNav)
                    {
                        if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Cameras)
                            continue;

                        ParameterSet parameterSet = element.Parameters;
                        foreach (Parameter parameter in parameterSet)
                        {
                            if (parameter.Id.IntegerValue == (int)BuiltInParameter.ELEM_CATEGORY_PARAM_MT)
                                continue;

                            ParameterData dataRaw = new ParameterData()
                            {
                                LinkName = linkInstance.Name.Substring(0, linkInstance.Name.IndexOf('.')),
                                ElementId = element.Id.IntegerValue,
                                ParameterName = parameter.Definition.Name
                            };

                            switch (parameter.StorageType)
                            {
                                case StorageType.String:
                                    dataRaw.ParameterValue = string.IsNullOrEmpty(parameter.AsString()) ? "<null>" : parameter.AsString();
                                    break;

                                case StorageType.Double:
                                    var rounding = UtilityFunctions.GetParameterAccuracy(doc, parameter);
                                    int decimalPlaces = (int)Math.Abs(Math.Log10(rounding));
                                    dataRaw.ParameterValue = Math.Round(UnitUtils.ConvertFromInternalUnits(parameter.AsDouble(), parameter.GetUnitTypeId()), decimalPlaces).ToString();
                                    break;

                                case StorageType.ElementId:
                                    dataRaw.ParameterValue = parameter.AsValueString();
                                    break;

                                case StorageType.Integer:
                                    dataRaw.ParameterValue = parameter.AsInteger().ToString();
                                    break;
                                default:
                                    dataRaw.ParameterValue = "<none>";
                                    break;
                            }
                            conn.Insert(dataRaw);
                        }
                    }
                }
                conn.Commit();
            }
            stopwatch.Stop();
            TimeSpan elapsedTime = stopwatch.Elapsed;

            sb.AppendLine($"Операция выполнена за {elapsedTime.TotalSeconds} секунд.");
            sb.AppendLine("Файл elements_data.db3 сохранён на рабочий стол");
            MessageBox.Show(sb.ToString());
        }
    }
}
