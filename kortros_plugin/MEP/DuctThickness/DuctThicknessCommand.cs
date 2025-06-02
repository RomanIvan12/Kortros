using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Kortros.Utilities;

namespace Kortros.MEP.DuctThickness
{
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class DuctThicknessCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Logger.Log.Info("--- Команда Толщина стенок запущена ---");
            Document doc = commandData.Application.ActiveUIDocument.Document;
            Guid width = new Guid("381b467b-3518-42bb-b183-35169c9bdfb3");
            ForgeTypeId displayUnits = doc.GetUnits().GetFormatOptions(SpecTypeId.Length).GetUnitTypeId();

            List<BuiltInCategory> categories = new List<BuiltInCategory>()
            {
                BuiltInCategory.OST_DuctCurves,
                BuiltInCategory.OST_DuctFitting
            };
            List<Element> allElements = categories.SelectMany(category => new FilteredElementCollector(doc).OfCategory(category).WhereElementIsNotElementType().ToElements()).ToList();
            
            foreach (Element element in allElements)
            {
                if (element.GroupId.IntegerValue == -1)
                {
                    using (Transaction t = new Transaction(doc, "Set ADSK_Толщина"))
                    {
                        t.Start();
                        try
                        {
                            string size = element.get_Parameter(BuiltInParameter.RBS_CALCULATED_SIZE).AsString();
                            int maxNumber = MaxNumber(size);
                            if (size.Contains('⌀'))
                            {
                                if (maxNumber <= 200)
                                    element.get_Parameter(width).Set(UnitUtils.ConvertToInternalUnits(0.5, displayUnits));
                                else if (maxNumber <= 450)
                                    element.get_Parameter(width).Set(UnitUtils.ConvertToInternalUnits(0.6, displayUnits));
                                else if (maxNumber <= 800)
                                    element.get_Parameter(width).Set(UnitUtils.ConvertToInternalUnits(0.7, displayUnits));
                                else if (maxNumber <= 1250)
                                    element.get_Parameter(width).Set(UnitUtils.ConvertToInternalUnits(1, displayUnits));
                                else if (maxNumber <= 1600)
                                    element.get_Parameter(width).Set(UnitUtils.ConvertToInternalUnits(1.2, displayUnits));
                                else if (maxNumber <= 2000)
                                    element.get_Parameter(width).Set(UnitUtils.ConvertToInternalUnits(1.4, displayUnits));
                            }
                            else
                            {
                                if (maxNumber <= 250)
                                    element.get_Parameter(width).Set(UnitUtils.ConvertToInternalUnits(0.5, displayUnits));
                                else if (maxNumber <= 1000)
                                    element.get_Parameter(width).Set(UnitUtils.ConvertToInternalUnits(0.7, displayUnits));
                                else if (maxNumber <= 2000)
                                    element.get_Parameter(width).Set(UnitUtils.ConvertToInternalUnits(0.9, displayUnits));
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Log.Error($"Элемент {element.Id.IntegerValue} отработан с ошибкой или не отработан. {ex.Message}");
                        }
                        t.Commit();
                    }
                }
            }
            Logger.Log.Info("------");
            MessageBox.Show("Параметр ADSK_Толщина заполнен", "Выполнение плагина завершено");
            return Result.Succeeded;
        }

        private int MaxNumber(string input)
        {
            List<int> numbersList = new List<int>();
            string currentNumber = "";

            foreach (char c in input)
            {
                if (char.IsDigit(c))
                    currentNumber += c;
                else
                {
                    if (!string.IsNullOrEmpty(currentNumber))
                    {
                        numbersList.Add(int.Parse(currentNumber));
                        currentNumber = "";
                    }
                }
            }
            if (!string.IsNullOrEmpty(currentNumber))
                numbersList.Add(int.Parse(currentNumber));
            return numbersList.Max();
        }
    }
}
