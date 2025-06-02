using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kortros.General.ExcelSync
{
    public class Handler
    {
        private readonly UIApplication uiapp;
        private readonly Document doc;

        private string MarkParName => Config.MarkParName;
        private string TypeMarkParName => Config.TypeMarkParName;

        private Dictionary<string, List<int>> invalidMarks = new Dictionary<string, List<int>>();
        private HashSet<int> updatedElements = new HashSet<int>();
        private Dictionary<string, Dictionary<string, object>> data;

        private List<BuiltInCategory> categories = new List<BuiltInCategory> {
            BuiltInCategory.OST_Walls,
            BuiltInCategory.OST_DuctAccessory,
            BuiltInCategory.OST_PipeAccessory,
            BuiltInCategory.OST_DuctCurves,
            BuiltInCategory.OST_PlaceHolderDucts,
            BuiltInCategory.OST_DuctTerminal,
            BuiltInCategory.OST_LightingDevices,
            BuiltInCategory.OST_FlexDuctCurves,
            BuiltInCategory.OST_FlexPipeCurves,
            BuiltInCategory.OST_DataDevices,
            BuiltInCategory.OST_Areas,
            BuiltInCategory.OST_HVAC_Zones,
            BuiltInCategory.OST_CableTray,
            BuiltInCategory.OST_ConduitRun,
            BuiltInCategory.OST_Conduit,
            BuiltInCategory.OST_DuctLinings,
            BuiltInCategory.OST_DuctInsulations,
            BuiltInCategory.OST_PipeInsulations,
            BuiltInCategory.OST_MechanicalEquipmentSet,
            BuiltInCategory.OST_GenericModel,
            BuiltInCategory.OST_MechanicalEquipment,
            BuiltInCategory.OST_LightingFixtures,
            BuiltInCategory.OST_SecurityDevices,
            BuiltInCategory.OST_FabricationHangers,
            BuiltInCategory.OST_FireAlarmDevices,
            BuiltInCategory.OST_Rooms,
            BuiltInCategory.OST_Wire,
            BuiltInCategory.OST_MEPSpaces,
            BuiltInCategory.OST_PlumbingFixtures,
            BuiltInCategory.OST_PipeSegments,
            BuiltInCategory.OST_SwitchSystem,
            BuiltInCategory.OST_DuctSystem,
            BuiltInCategory.OST_DuctFitting,
            BuiltInCategory.OST_CableTrayFitting,
            BuiltInCategory.OST_ConduitFitting,
            BuiltInCategory.OST_PipeFitting,
            BuiltInCategory.OST_Sprinklers,
            BuiltInCategory.OST_TelephoneDevices,
            BuiltInCategory.OST_PlaceHolderPipes,
            BuiltInCategory.OST_PipingSystem,
            BuiltInCategory.OST_PipeCurves,
            BuiltInCategory.OST_FabricationPipework,
            BuiltInCategory.OST_NurseCallDevices,
            BuiltInCategory.OST_CommunicationDevices,
            BuiltInCategory.OST_CableTrayRun,
            BuiltInCategory.OST_Mass,
            BuiltInCategory.OST_Parts,
            BuiltInCategory.OST_ElectricalFixtures,
            BuiltInCategory.OST_ElectricalInternalCircuits,
            BuiltInCategory.OST_ElectricalCircuit,
            BuiltInCategory.OST_ElectricalEquipment,
            BuiltInCategory.OST_FabricationDuctwork,
            BuiltInCategory.OST_FabricationContainment,
            BuiltInCategory.OST_DetailComponents,

        };
        public List<Category> Categories => categories.Select(bc => Category.GetCategory(doc, bc)).ToList();

        public Handler(UIApplication uiapp)
        {
            this.uiapp = uiapp;
            doc = uiapp.ActiveUIDocument.Document;
        }

        public void Run(Dictionary<string, Dictionary<string, object>> data, bool updateOnlySelected, Category selectedCategory)
        {
            updatedElements.Clear();
            invalidMarks.Clear();

            Stopwatch sw = new Stopwatch();
            sw.Start();
            TimeSpan time;

            string modeMessage = "Будут обновлены ";
            if (updateOnlySelected)
            {
                modeMessage += uiapp.ActiveUIDocument.Selection.GetElementIds().Count > 0 ? "выбранные" : $"видимые на виде {uiapp.ActiveUIDocument.ActiveView.Name}";
            }
            else
            {
                modeMessage += "все";
            }
            modeMessage += " элементы";
            if (selectedCategory != null)
            {
                modeMessage += $" категории {selectedCategory.Name}";
            }

            Console.WriteLine(modeMessage);

            this.data = data;
            using (Transaction tx = new Transaction(doc))
            {
                try
                {
                    tx.Start("Notion Sync");

                    var elements = CollectElements(!updateOnlySelected);
                    if (selectedCategory != null)
                    {
                        elements.OfCategory((BuiltInCategory)selectedCategory.Id.IntegerValue);
                    }
                    foreach (Element e in elements)
                    {
                        SetInstanceParameterValues(e);
                    }

                    tx.Commit();
                    sw.Stop();
                    time = sw.Elapsed;
                    Console.WriteLine($"Время выполнения - {time}");
                }
                catch (Exception e)
                {
                    tx.RollBack();
                    sw.Stop();
                    time = sw.Elapsed;
                    throw e;
                }
            }
        }

        private FilteredElementCollector CollectElements(bool all)
        {
            FilteredElementCollector collector = Common.Collector.Create(uiapp, all);

            collector.WhereElementIsNotElementType();

            ElementMulticategoryFilter catFilter = new ElementMulticategoryFilter(categories);
            collector.WherePasses(catFilter);

            return collector;
        }

        private void SetInstanceParameterValues(Element e)
        {
            string mark = GetMark(e);
            if (string.IsNullOrEmpty(mark))
            {
                //Console.WriteLine($"#{e.Id}: нет марки");
                return;
            }

            SetMark(e, mark);
            Console.WriteLine($"#{e.Id}: {mark}");

            SetElementParameterValues(e, mark, data);
        }


        private void SetElementParameterValues(Element e, string mark, Dictionary<string, Dictionary<string, object>> data)
        {
            if (!data.ContainsKey(mark))
            {
                if (invalidMarks.ContainsKey(mark))
                {
                    invalidMarks[mark].Add(e.Id.IntegerValue);
                }
                else
                {
                    invalidMarks[mark] = new List<int> { e.Id.IntegerValue };
                    Console.WriteLine($"#{e.Id}: Марки '{mark}' нет в таблице.");
                }
                return;
            }

            Dictionary<string, object> elementData = data[mark];

            foreach (string parName in elementData.Keys)
            {
                Parameter par = e.LookupParameter(parName);

                if (par == null)
                {
                    Console.WriteLine($"#{e.Id}: нет параметра '{parName}', параметр будет пропущен.");
                    continue;
                }

                // Skip mark
                if (par.IsShared && par.Definition.Name == MarkParName)
                {
                    continue;
                }

                if (par.IsReadOnly)
                {
                    Console.WriteLine($"#{e.Id}: Параметр '{parName}' только для чтения");
                    continue;
                }

                bool isString = par.StorageType == StorageType.String;

                bool isSimpleNumber = par.Definition.GetDataType() == SpecTypeId.Number || par.StorageType == StorageType.Integer;

                //bool isSimpleNumber = par.Definition.UnitType == UnitType.UT_Number || par.StorageType == StorageType.Integer;
                if (!isString && !isSimpleNumber)
                {
                    Console.WriteLine($"#{e.Id}: Параметр '{parName}' не поддерживаемого типа");
                    continue;
                }

                string rawValue = elementData[parName] as string;

                try
                {

                    // NULL
                    if (rawValue == null && par.HasValue)
                    {
                        if (par.StorageType == StorageType.Double || par.StorageType == StorageType.Integer)
                        {
                            par.Set(0);
                        }
                        else if (par.StorageType == StorageType.String)
                        {
                            par.Set("");
                        }
                        continue;
                    }


                    // STRING
                    if (par.StorageType == StorageType.String)
                    {
                        string value = rawValue;
                        string currentValue = par.AsString();
                        if (string.IsNullOrEmpty(currentValue) && string.IsNullOrEmpty(value))
                        {
                            continue;
                        }
                        if (currentValue != value)
                        {
                            par.Set(value);
                        }
                        continue;
                    }

                    // NUMBER, INTEGER
                    if (par.Definition.GetDataType() == SpecTypeId.Number)
                    //if (par.Definition.UnitType == UnitType.UT_Number)
                    {
                        double value;
                        try
                        {
                            value = Convert.ToDouble(rawValue.Replace(",", "."), CultureInfo.InvariantCulture);
                        }
                        catch (Exception)
                        {
                            Console.WriteLine($"#{e.Id}: Некорректное значение '{rawValue}' для параметра '{parName}'");
                            continue;
                        }
                        double currentValue = par.AsDouble();
                        if (currentValue != value)
                        {
                            par.Set(value);
                        }
                    }
                    else if (par.StorageType == StorageType.Integer)
                    {
                        int value;
                        bool res = int.TryParse(rawValue, out value);
                        if (!res)
                        {
                            Console.WriteLine($"#{e.Id}: Некорректное значение '{rawValue}' для параметра '{parName}'");
                            continue;
                        }
                        int currentValue = par.AsInteger();
                        if (currentValue != value)
                        {
                            par.Set(value);
                        }
                    }

                }
                catch (Exception error)
                {
                    Console.WriteLine($"#{e.Id}: {error.Message}");
                }
            }

            updatedElements.Add(e.Id.IntegerValue);
        }

        private string GetMark(Element e)
        {
            //  1. пробуем получить у экземпляра ARO_OBJ_Марка
            Parameter markPar = e.LookupParameter(MarkParName); // TODO use GUID
            if (markPar == null)
            {
                // 2. Если такого параметра вообще нет, пропускаем элемент
                Console.WriteLine($"#{e.Id}: нет параметра марки '{MarkParName}'.");
                return null;
            }
            if (markPar.IsReadOnly)
            {
                // 3. Если readonly значит назначен через семейсвто, возвращаем его значение
                return markPar.AsString();
            }

            // 4. Берем тип
            Element type = doc.GetElement(e.GetTypeId());
            if (type == null)
            {
                return markPar.AsString();
            }

            // 5. Если параметр ARO_OBJ_Марка в типе, пропускаем элемент
            if (type.LookupParameter(MarkParName) != null)
            {
                Console.WriteLine($"#{e.Id}: параметр марки '{MarkParName}' в типе");
                return null;
            }

            // 6. Берем из типа параметр 
            Parameter typeMarkPar = type.LookupParameter(TypeMarkParName); // TODO use GUID

            // 7. Если его нет возвращаем значение ARO_OBJ_Марка экземпляра
            if (typeMarkPar == null)
            {
                return markPar.AsString();
            }

            string typeMark = typeMarkPar.AsString();

            // 8. Если нет значения ARO_GEN_Тип_Марка тоже возвращаем значение ARO_OBJ_Марка экземпляра
            if (String.IsNullOrEmpty(typeMark))
            {
                return markPar.AsString();
            }

            // 9. Возвращаем значение ARO_GEN_Тип_Марка
            return typeMark;
        }

        private void SetMark(Element e, string mark)
        {
            Parameter markPar = e.LookupParameter(MarkParName);
            if (markPar.AsString() != mark)
            {
                markPar.Set(mark);
            }
        }
    }
}
