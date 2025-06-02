using Autodesk.Revit.DB;
using Kortros.ParamParser.Model;
using Kortros.ParamParser.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Kortros.ParamParser.View;

namespace Kortros.ParamParser.ViewModel
{
    public partial class ParamStackVM
    {
        // Вспомогательные функции, реализующие действия UI
        public void SelectAllCategories()
        {
            foreach (var item in CategoryItems)
            {
                item.IsChecked = true;
                DatabaseHelper.Update(item);
            }
            GetCategoryItems();
        }
        public void DeSelectAllCategories()
        {
            foreach (var item in CategoryItems)
            {
                item.IsChecked = false;
                DatabaseHelper.Update(item);
            }
            GetCategoryItems();
        }

        public void CopyParamStack(ParamStack originalInstance)
        {
            ParamStack newInstanse = (ParamStack)originalInstance.Clone();
            DatabaseHelper.Insert(newInstanse);

            GetParamStacks();
        }

        public void IsParameterStackCorrect()
        {
            if (SelectedParamStack != null)
            {
                if (!SelectedParamStack.IsTypeInit && SelectedParamStack.IsTypeTarg) //Исх экз И целевой тип
                {
                    SelectedParamStack.IsCorrect = false;
                    DatabaseHelper.Update(SelectedParamStack);
                }
                else if (SelectedParamStack.StorageTypeTarg == "ElementId" || SelectedParamStack.StorageTypeTarg == null)
                {
                    SelectedParamStack.IsCorrect = false;
                    DatabaseHelper.Update(SelectedParamStack);
                }
                else
                {
                    SelectedParamStack.IsCorrect = true;
                    DatabaseHelper.Update(SelectedParamStack);
                }
            }
        }



        public void Calculate()
        {
            var listOfBuiltInCategories = BuiltInCategories();
            List<DataItem> dataItems = new List<DataItem>();

            if (ChosenPickElements == "Обработка всех элементов в документе")
            {
                // Получаем список категорий, которые отмечены
                List<Element> allElementsInProject = CalculateFunctions.GetAllElements(RunCommand.Doc, listOfBuiltInCategories);

                CalculateFunctions.SetValue(RunCommand.Doc, allElementsInProject, SelectedConfig, out dataItems);
            }
            else if (ChosenPickElements == "Обработка элементов на текущем 3D виде")
            {
                List<Element> elementsOnActiveView = CalculateFunctions.GetElementsOnActiveView(RunCommand.Doc, listOfBuiltInCategories);

                CalculateFunctions.SetValue(RunCommand.Doc, elementsOnActiveView, SelectedConfig, out dataItems);
            }
            else if (ChosenPickElements == "Обработка выбранных элементов")
            {
                List<Element> selectedElements = CalculateFunctions.GetSelectedElements(RunCommand.UIdoc, listOfBuiltInCategories);

                if (selectedElements.Count != 0)
                    selectedElements = CalculateFunctions.SelectElements(RunCommand.UIdoc, RunCommand.Doc, listOfBuiltInCategories, RunCommand.MainWindow);

                CalculateFunctions.SetValue(RunCommand.Doc, selectedElements, SelectedConfig, out dataItems);
            }
            DataItems.Clear();
            foreach (DataItem item in dataItems)
            {
                DataItems.Add(item);
            }

            DataTableWindow dataWindow = new DataTableWindow()
            {
                DataContext = this,
            };
            dataWindow.ShowDialog();
        }




        // Получил ПарамСтэки для отмеченных категорий
        public List<ParamStack> GetParStacksForCalculation()
        {
            List<ParamStack> paramStacks = new List<ParamStack>();

            List<CategoryItem> allCatItems = DatabaseHelper.Read<CategoryItem>()
                .Where(i => i.ConfigId == SelectedConfig.Id && i.IsChecked)
                .ToList();
            foreach (CategoryItem catItem in allCatItems)
            {
                List<ParamStack> stacks = DatabaseHelper.Read<ParamStack>()
                    .Where(i => i.CategoryItemId == catItem.Id && i.IsCorrect)
                    .ToList();
                paramStacks.AddRange(stacks);
            }
            return paramStacks;
        }

        public List<BuiltInCategory> BuiltInCategories()
        {
            List<CategoryItem> allCatItems = DatabaseHelper.Read<CategoryItem>()
                .Where(i => i.ConfigId == SelectedConfig.Id && i.IsChecked)
                .ToList();

            List<BuiltInCategory> categories = allCatItems
                .Select(catItem => (BuiltInCategory)Enum.Parse(typeof(BuiltInCategory), catItem.Name))
                .ToList();
            return categories;
        }
    }
}
