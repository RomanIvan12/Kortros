using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using log4net;
using ParamParser.Extensions.SelectionsExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ParamParser.WPF
{
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    /// <summary>
    /// Логика взаимодействия для TestWindow.xaml
    /// </summary>
    public partial class TestWindow : Window
    {
        private SetValue _myVal;

        private readonly List<BuiltInCategory> categories = new List<BuiltInCategory> {
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
            BuiltInCategory.OST_Windows,
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
            BuiltInCategory.OST_DetailComponents
        };

        public List<Category> Categories => categories.Select(bc => Category.GetCategory(_doc, bc)).ToList();
        private static Document _doc;
        private readonly UIDocument _uidoc;

        private IList<ElementId> _selectedElementIds;
        private List<ItemPar> _selectionItemPar;

        StorageType _storageTypeInitial;
        StorageType _storageTypeTarget;

        public List<ItemCat> _items;

        private ItemCat _itemCat;
        private ItemPar _itemParInit;
        private ItemPar _itemParTarget;
        private bool _rewrite;
        private bool _showLog;

        private static readonly ILog _logger = ParserCommand._logger;
        private static readonly ILog _loggerShow = ParserCommand._loggerShow;

        public TestWindow(Document doc, UIDocument uidoc)
        {
            InitializeComponent();
            ///Значения, передаваемое в SetValue
            //_myVal = new SetValue(doc, _itemCat, _itemParInit, _itemParTarget, _rewrite, _storageTypeInitial, _storageTypeTarget, _selectedElementIds);
            _doc = doc;
            _uidoc = uidoc;
            List<ItemCat> items = new List<ItemCat>();

            CategoryPick.IsChecked = true;
            ReWrite.IsChecked = true;

            foreach (var category in Categories)
            {
                ItemCat tempItem = new ItemCat
                {
                    DisplayValue = category.Name,
                    SelectedCategory = category
                };
                items.Add(tempItem);
            }
            CategoryList.ItemsSource = items;
            _items = items;
            try
            {
                GetSelectedElementIds();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            reWrite_SelectionChanged();
            ShowLog_SelectionChanged();
            if (_showLog == true)
            {
                //если галка включена
                if (Selected.IsChecked == true)
                {
                    _loggerShow.Info(_itemCat);
                }
                else if (CategoryPick.IsChecked == true)
                {
                    _loggerShow.Info("Selected");
                }
            }
            _myVal = new SetValue(_doc, _itemCat, _itemParInit, _itemParTarget, _rewrite, _storageTypeInitial, _storageTypeTarget, _selectedElementIds);
            if (CategoryPick.IsChecked == true)
            {
                ParamParser.SetValue.SetValues();
            }
            else if (Selected.IsChecked == true)
            {
                ParamParser.SetValue.SetValuessSelected();
            }
            this.Close();
        }


        // Получить параметры для категории
        private List<Parameter> ParametersOfSelectedCategory(Category category)
        {
            try
            {
                List<Parameter> parameters = new List<Parameter>();
                Element instance = new FilteredElementCollector(_doc).OfCategoryId(category.Id).WhereElementIsNotElementType().ToElements().First();
                if (instance != null)
                {
                    ParameterSet parameterset = instance.Parameters;
                    foreach (Parameter parameter in parameterset)
                    {
                        parameters.Add(parameter);
                    }
                }

                Element type = new FilteredElementCollector(_doc).OfCategoryId(category.Id).WhereElementIsElementType().ToElements().First();
                if (type != null)
                {
                    ParameterSet parametersetType = type.Parameters;
                    foreach (Parameter parameter in parametersetType)
                    {
                        parameters.Add(parameter);
                    }
                }
                return parameters; // параметры типа и экземпляра
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            return null; // параметры типа и экземпляра
        }

        private void reWrite_SelectionChanged()
        {
            if (ReWrite.IsChecked == true)
            {
                _rewrite = true;
            }
            else if (ReWrite.IsChecked == false)
            {
                _rewrite = false;
            }
        }

        private void ShowLog_SelectionChanged()
        {
            if (ShowLog.IsChecked == true)
            {
                _showLog = true;
            }
            else if (ShowLog.IsChecked == false)
            {
                _showLog = false;
            }
        }

        private void CategoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                //получение выбранного элемента категории
                ItemCat selectedItem = CategoryList.SelectedItem as ItemCat;
                _itemCat = selectedItem; // to SetValues

                //получение параметров выбранной категории
                List<Parameter> parametersInit = ParametersOfSelectedCategory(selectedItem.SelectedCategory);

                List<Parameter> parametersTarget = new List<Parameter>();

                if (parametersInit == null)
                {
                    InitialParameters.ItemsSource = null;
                    InitialParameters.Text = "";

                    TargetParameters.ItemsSource = null;
                    TargetParameters.Text = "";
                    //возможно _selectionItemPar = нулл надо записать
                }
                //_parameters = parameters;
                List<ItemPar> parsInitial = new List<ItemPar>();
                List<ItemPar> parsTarget = new List<ItemPar>();

                foreach (Parameter parameter in parametersInit)
                {
                    ItemPar tempPar = new ItemPar
                    {
                        DisplayValue = parameter.Definition.Name,
                        SelectedParameter = parameter
                    };

                    parsInitial.Add(tempPar);
                    if (!parameter.IsReadOnly //Заполнение значений для SelectedItems
                        && parameter.CanBeAssociatedWithGlobalParameters())
                    {
                        ItemPar tempParTarg = new ItemPar
                        {
                            DisplayValue = parameter.Definition.Name,
                            SelectedParameter = parameter
                        };
                        parsTarget.Add(tempParTarg);
                    }
                }
                InitialParameters.Text = "";
                TargetParameters.Text = "";

                MyComparer comparer = new MyComparer();
                parsInitial.Sort(comparer);
                parsTarget.Sort(comparer);

                InitialParameters.ItemsSource = parsInitial;
                TargetParameters.ItemsSource = parsTarget;

                _selectionItemPar = parsInitial;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }


        private void InitialParameters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Получение выбранного параметра (для того, чтобы узнать его СтораджТайп
            if (InitialParameters.SelectedItem == null)
            {
                _itemParInit = null;
            }
            else
            {
                ItemPar initPar = InitialParameters.SelectedItem as ItemPar;
                _itemParInit = initPar; // to SetValues
                Parameter InitParam = initPar.SelectedParameter;

                _storageTypeInitial = InitParam.StorageType;
            }
        }

        private void TargetParameters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TargetParameters.SelectedItem == null)
            {
                _itemParTarget = null;
            }
            else
            {
                ItemPar targetPar = TargetParameters.SelectedItem as ItemPar;
                _itemParTarget = targetPar; //to SetValues
                Parameter TargetParam = targetPar.SelectedParameter;

                _storageTypeTarget = TargetParam.StorageType;
            }
        }

        //Получить значения выбранного параметра у всех элементов выбдранной категории
        // Получил словарь ЭлементАйди-значение

        private void radioButton_CheckedChanged(object sender, RoutedEventArgs e) // Поменять цвет надписей
        {
            ButtonPossibleSelect();
            RadioButton radioButton = (RadioButton)sender;

            List<RadioButton> listzzz = new List<RadioButton>()
            {
                CategoryPick,
                Selected
            };
            if (radioButton.IsChecked == true)
            {
                if (_selectedElementIds != null && Selected.IsChecked == true)
                {
                    SetInitTargetParam(); //THIS
                }
                else if (CategoryPick.IsChecked == true)
                {
                    InitialParameters.ItemsSource = null;
                    InitialParameters.Text = "";

                    TargetParameters.ItemsSource = null;
                    TargetParameters.Text = "";
                }
                radioButton.Foreground = System.Windows.Media.Brushes.Black;
                foreach (RadioButton r in listzzz)
                {
                    if (r.IsChecked != true)
                    {
                        r.Foreground = System.Windows.Media.Brushes.White;
                    }
                }
            }
        }

        private void ButtonPossibleSelect()
        {
            if (CategoryPick.IsChecked == true)
            {
                SelectElements.IsEnabled = false;
                CategoryList.IsEnabled = true;
            }
            else if (Selected.IsChecked == true)
            {
                SelectElements.IsEnabled = true;
                CategoryList.IsEnabled = false;
            }
        }

        private void SelectElements_Click(object sender, RoutedEventArgs e) //
        {
            IList<ElementId> selectedIds = new List<ElementId>();
            this.Hide();
            IList<Reference> references = _uidoc.Selection.PickObjects(
                ObjectType.Element, new ElementSelectionFilter(
                    el => categories.Contains(el.Category.BuiltInCategory)));

            foreach (Reference item in references)
            {
                selectedIds.Add(item.ElementId);
            }
            _selectedElementIds = selectedIds;
            SetInitTargetParam();
            this.ShowDialog();
            //Topmost = true;
        }

        private IList<Category> GetUniqueCategories()
        {
            IList<Category> categories = new List<Category>();

            IList<string> categoriesNames = new List<string>();
            IList<Element> selectedElements = _selectedElementIds.Select(item => _doc.GetElement(item)).ToList();

            foreach (Element element in selectedElements)
            {
                Category cat = element.Category;
                if (!categoriesNames.Contains(cat.Name))
                {
                    categories.Add(cat);
                    categoriesNames.Add(cat.Name);
                }
            }
            if (categories.Count > 0)
            {
                return categories;
            }
            else { return null; }
        }

        private void SetInitTargetParam()
        {
            IList<Category> uniqueCategories = GetUniqueCategories();

            if (uniqueCategories != null && uniqueCategories.Count > 0)
            {
                List<ItemPar> parsInit = new List<ItemPar>();
                List<ItemPar> parsTarg = new List<ItemPar>();

                List<Parameter> baseList = ParametersOfSelectedCategory(uniqueCategories[0]);
                List<int> baseIds = baseList.Select(c => c.Id.IntegerValue).ToList(); //id параметров первого элемента

                List<Parameter> baseListTarget = new List<Parameter>();

                if (uniqueCategories.Count() == 1)
                {
                    foreach (Parameter singlePar in baseList)
                    {
                        if (baseIds.Contains(singlePar.Id.IntegerValue))
                        {
                            ItemPar tempPar = new ItemPar
                            {
                                DisplayValue = singlePar.Definition.Name,
                                SelectedParameter = singlePar
                            };
                            parsInit.Add(tempPar);
                        }
                    }
                    InitialParameters.ItemsSource = parsInit;

                    foreach (Parameter item in baseList)
                    {
                        if (!item.IsReadOnly //Заполнение значений для SelectedItems
                            && item.CanBeAssociatedWithGlobalParameters())
                        {
                            baseListTarget.Add(item);
                        }
                    }
                    List<int> targIds = baseListTarget.Select(c => c.Id.IntegerValue).ToList(); //id параметров первого элемента

                    foreach (Parameter singlePar in baseList)
                    {
                        if (targIds.Contains(singlePar.Id.IntegerValue))
                        {
                            ItemPar tempPar = new ItemPar
                            {
                                DisplayValue = singlePar.Definition.Name,
                                SelectedParameter = singlePar
                            };
                            parsTarg.Add(tempPar);
                        }
                    }
                    TargetParameters.ItemsSource = parsTarg;
                }
                else
                {
                    for (int i = 1; i < uniqueCategories.Count(); i++)
                    {
                        List<int> baseIdsInit = new List<int>();
                        List<ItemPar> items = new List<ItemPar>();
                        List<Parameter> parameters = ParametersOfSelectedCategory(uniqueCategories[i]);

                        foreach (Parameter singlePar in parameters)
                        {
                            if (baseIds.Contains(singlePar.Id.IntegerValue))
                            {
                                ItemPar tempPar = new ItemPar
                                {
                                    DisplayValue = singlePar.Definition.Name,
                                    SelectedParameter = singlePar
                                };
                                items.Add(tempPar);

                                baseIdsInit.Add(singlePar.Id.IntegerValue);
                            }
                        }
                        baseIds = baseIdsInit;
                        parsInit = items;
                    }
                    InitialParameters.ItemsSource = parsInit; // Получил общие параметры для всех категорий в выборе

                    foreach (Parameter item in baseList)
                    {
                        if (!item.IsReadOnly //Заполнение значений для SelectedItems
                            && item.CanBeAssociatedWithGlobalParameters())
                        {
                            baseListTarget.Add(item);
                        }
                    }

                    List<int> targIds = baseListTarget.Select(c => c.Id.IntegerValue).ToList(); //id параметров первого элемента
                    for (int i = 1; i < uniqueCategories.Count(); i++)
                    {
                        List<int> baseIdsTarget = new List<int>();
                        List<ItemPar> items = new List<ItemPar>();
                        List<Parameter> parameters = ParametersOfSelectedCategory(uniqueCategories[i]);

                        foreach (Parameter singlePar in parameters)
                        {
                            if (targIds.Contains(singlePar.Id.IntegerValue))
                            {
                                ItemPar tempPar = new ItemPar
                                {
                                    DisplayValue = singlePar.Definition.Name,
                                    SelectedParameter = singlePar
                                };
                                items.Add(tempPar);
                                baseIdsTarget.Add(singlePar.Id.IntegerValue);
                            }
                        }
                        targIds = baseIdsTarget;
                        parsTarg = items;
                    }
                    TargetParameters.ItemsSource = parsTarg;
                }
            }
        }

        private IList<ElementId> GetSelectedElementIds()
        {
            Selection selection = _uidoc.Selection;
            ICollection<ElementId> selectedIds = selection.GetElementIds();

            IList<ElementId> ids = new List<ElementId>();

            foreach (ElementId id in selectedIds)
            {
                if (categories.Contains(_doc.GetElement(id).Category.BuiltInCategory))
                {
                    ids.Add(id);
                }
            }
            if (ids.Count > 0)
            {
                _selectedElementIds = ids;
                return ids;
            }
            else { return null; }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class ItemCat
    {
        public string DisplayValue { get; set; }
        public Category SelectedCategory { get; set; }
    }

    public class ItemPar
    {
        public string DisplayValue { get; set; }
        public Parameter SelectedParameter { get; set; }
    }

    public class  MyComparer : IComparer<ItemPar>
    {
        public int Compare(ItemPar x, ItemPar y)
        {
            return x.DisplayValue.CompareTo(y.DisplayValue);
        }
    }

}

