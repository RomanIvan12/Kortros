using Autodesk.Revit.DB;
using Kortros.ParamParser.Model;
using Kortros.ParamParser.ViewModel.Helpers;
using System.Linq;
using System.Windows.Controls;
using Kortros.ParamParser.ViewModel.Commands;
using static System.Net.Mime.MediaTypeNames;

namespace Kortros.ParamParser.ViewModel
{
    public partial class ParamStackVM
    {
        #region Config Parameters

        private Config selectedConfig;
        public Config SelectedConfig
        {
            get { return selectedConfig; }
            set
            {
                selectedConfig = value;
                OnPropertyChanged("SelectedConfig");
                GetCategoryItems();
            }
        }

        private string configName;
        public string ConfigName
        {
            get { return configName; }
            set
            {
                configName = value;
                OnPropertyChanged("ConfigName");
            }
        }
        #endregion

        #region CategoryItem Parameters

        private CategoryItem selectedCategoryItem;
        public CategoryItem SelectedCategoryItem
        {
            get { return selectedCategoryItem; }
            set
            {
                selectedCategoryItem = value;
                OnPropertyChanged("SelectedCategoryItem");
                GetParamStacks();
                GetParameterItemsForCategory();
            }
        }
        #endregion
        
        private string _noteText;

        public string NoteText
        {
            get { return _noteText; }
            set
            {
                _noteText = value;
                OnPropertyChanged("NoteText");
            }
        }

        #region ParamStack Parameters

        private bool _suppressOnPropertyChanged;
        private ParamStack selectedParamStack;
        public ParamStack SelectedParamStack
        {
            get { return selectedParamStack; }
            set
            {
                selectedParamStack = value;
                if (!_suppressOnPropertyChanged)
                {
                    OnPropertyChanged("SelectedParamStack");
                    OnSel();
                }
            }
        }

        #endregion

        private void OnSel()
        {
            SelectedParameterItemInit = null;
            SelectedParameterItemTarg = null;
        }


        #region Config Functions
        private void GetConfigs()
        {
            var configs = DatabaseHelper.Read<Config>();

            Configs.Clear();

            foreach (var config in configs)
            {
                Configs.Add(config);
            }
        }

        public void CreateConfig(string cName)
        {
            Config newConfig = new Config()
            {
                Name = cName,
            };

            DatabaseHelper.Insert(newConfig);
            ConfigName = string.Empty;
            GetConfigs();
            SelectedConfig = Configs.Last();
        }

        public void DeleteConfig()
        {
            if (SelectedConfig != null)
            {
                var categoryItems = DatabaseHelper.Read<CategoryItem>().Where(n => n.ConfigId == SelectedConfig.Id).ToList();
                CategoryItems.Clear();
                ParamStacks.Clear();
                foreach (var categoryItem in categoryItems)
                {
                    var paramStacks = DatabaseHelper.Read<ParamStack>().Where(i => i.CategoryItemId == categoryItem.Id).ToList();
                    foreach (var paramStack in paramStacks)
                    {
                        DatabaseHelper.Delete(paramStack);
                    }
                    DatabaseHelper.Delete(categoryItem);
                }
                DatabaseHelper.Delete(SelectedConfig);
                GetConfigs();
                if (Configs.Count > 0)
                    SelectedConfig = Configs[0];
            }
        }
        #endregion

        #region CategoryItem Functions

        public void CreateCategoryItem(string categoryName, int configId)
        {
            CategoryItem newCategoryItem = new CategoryItem()
            {
                Name = categoryName,
                ConfigId = configId
            };

            DatabaseHelper.Insert(newCategoryItem);

            // Функция, которая проверяет новый вставленный CategoryItem (его Name), есть ли в списке ParameterItemCommon, если нет - добавляет параметры
            // для только что созданной категории
            var categoriesParameterItemCommon = DatabaseHelper.Read<ParameterItemCommon>().Select(n => n.Category).ToList();
            if (!categoriesParameterItemCommon.Contains(categoryName))
            {
                ParameterItemHelper.CreateParameterItemCommon(RunCommand.Doc, categoryName);
            }
            GetCategoryItems();
        }

        public void GetCategoryItems() // TODO: СЮДА ВСТАВЛЯТЬ БЛОКИРОВКУ В СЛУЧАЕ ОТСУТСТВИЯ ПАРАМЕТРОВ ДЛЯ КАТЕГОРИИ
        {
            if (SelectedConfig != null)
            {
                BlockCategoryItem(SelectedConfig);
                var categotyItems = DatabaseHelper.Read<CategoryItem>().Where(n => n.ConfigId == SelectedConfig.Id).ToList();

                CategoryItems.Clear();
                ParamStacks.Clear();
                foreach (var categoryItem in categotyItems)
                {
                    CategoryItems.Add(categoryItem);
                }

            }
        }

        public void DeleteCategoryItem()
        {
            if (SelectedCategoryItem != null)
            {
                var paramStacks = DatabaseHelper.Read<ParamStack>().Where(n => n.CategoryItemId == SelectedCategoryItem.Id).ToList();
                ParamStacks.Clear();
                foreach (var paramStack in paramStacks)
                {
                    DatabaseHelper.Delete(paramStack);
                }
            }

            DatabaseHelper.Delete(SelectedCategoryItem);
            GetCategoryItems();
        }
        #endregion

        #region ParamStack Functions
        public void CreateParamStack(int categoryItemId)
        {
            ParamStack newStack = new ParamStack()
            {
                CategoryItemId = categoryItemId,

            };
            DatabaseHelper.Insert(newStack);

            GetParamStacks();
        }
        
        private void GetParamStacks()
        {
            NoteText = "";

            if (SelectedCategoryItem != null)
            {
                var paramStacks = DatabaseHelper.Read<ParamStack>().Where(n => n.CategoryItemId == SelectedCategoryItem.Id).ToList();

                var targetParameters = paramStacks.Select(i => i.GuidTarg).ToList();
                var duplicateStacks = targetParameters.GroupBy(x => x)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();
                ParamStacks.Clear();

                if (duplicateStacks.Any())
                {
                    NoteText += $"Удалите повторяющиеся элементы \n";
                }

                foreach (var paramStack in paramStacks)
                {
                    // TODO: Проверка параметров общих в стаке на наличие такого общего параметра в проекте
                    var catName = SelectedCategoryItem.Name;
                    var guidInit = paramStack.GuidInit;
                    var guidTarg = paramStack.GuidTarg;

                    //IsEnabledCategory = true;
                    if (!string.IsNullOrEmpty(guidInit))
                    {
                        if (!ParameterItemHelper.DoesSharedParameterExists(RunCommand.Doc, catName, guidInit))
                        {
                            NoteText += $"Параметр {paramStack.NameInit} Отсутствует в проекте \n";
                        }

                    }
                    if (!string.IsNullOrEmpty(guidTarg))
                    {
                        if (!ParameterItemHelper.DoesSharedParameterExists(RunCommand.Doc, catName, guidTarg))
                        {
                            NoteText += $"Параметр {paramStack.NameTarg} Отсутствует в проекте \n";
                        }
                    }
                    ParamStacks.Add(paramStack);
                }

                if (string.IsNullOrEmpty(NoteText))
                {
                    SelectedCategoryItem.IsEnabled = true;
                    DatabaseHelper.Update(SelectedCategoryItem);
                }
            }
        }

        private void BlockCategoryItem(Config selectedConfig)
        {
            if (selectedConfig != null)
            {
                var categotyItems = DatabaseHelper.Read<CategoryItem>().Where(n => n.ConfigId == selectedConfig.Id).ToList();
                foreach (CategoryItem categoryItem in categotyItems)
                {
                    if (categoryItem != null)
                    {
                        var paramStacks = DatabaseHelper.Read<ParamStack>().Where(n => n.CategoryItemId == categoryItem.Id).ToList();

                        var targetParameters = paramStacks.Select(i => i.GuidTarg).ToList();
                        var duplicateStacks = targetParameters.GroupBy(x => x)
                            .Where(g => g.Count() > 1)
                            .Select(g => g.Key)
                            .ToList();

                        int counter = 0;
                        foreach (var paramStack in paramStacks)
                        {
                            var catName = categoryItem.Name;
                            var guidInit = paramStack.GuidInit;
                            var guidTarg = paramStack.GuidTarg;
                            if (!string.IsNullOrEmpty(guidInit))
                            {
                                if (!ParameterItemHelper.DoesSharedParameterExists(RunCommand.Doc, catName, guidInit))
                                {
                                    categoryItem.IsEnabled = false;
                                    categoryItem.IsChecked = false;
                                    DatabaseHelper.Update(categoryItem);
                                    counter++;
                                }
                            }
                            if (!string.IsNullOrEmpty(guidTarg))
                            {
                                if (!ParameterItemHelper.DoesSharedParameterExists(RunCommand.Doc, catName, guidTarg))
                                {
                                    categoryItem.IsEnabled = false;
                                    categoryItem.IsChecked = false;
                                    DatabaseHelper.Update(categoryItem);
                                    counter++;
                                }
                            }
                            if (duplicateStacks.Any())
                            {
                                categoryItem.IsEnabled = false;
                                categoryItem.IsChecked = false;
                                DatabaseHelper.Update(categoryItem);
                                counter++;
                            }
                        }
                        if (counter == 0)
                        {
                            categoryItem.IsEnabled = true;
                            DatabaseHelper.Update(categoryItem);
                        }
                    }
                }
            }
        }


        // Нужно сделать так, чтобы SelectedParamStack сохранился
        private void GetParamStacks2()
        {
            _suppressOnPropertyChanged = true;
            if (SelectedCategoryItem != null)
            {
                ParamStack paramStack1 = SelectedParamStack;

                var paramStacks = DatabaseHelper.Read<ParamStack>().Where(n => n.CategoryItemId == SelectedCategoryItem.Id).ToList();

                ParamStacks.Clear();
                foreach (var paramStack in paramStacks)
                {
                    ParamStacks.Add(paramStack);
                }
                SelectedParamStack = paramStack1;
            }
            _suppressOnPropertyChanged = false;
        }

        public void DeleteParamStack()
        {
            var tempCategory = SelectedCategoryItem;
            DatabaseHelper.Delete(SelectedParamStack);
            GetCategoryItems();
            GetParamStacks();
            SelectedCategoryItem = tempCategory;
        }
        #endregion
    }
}
