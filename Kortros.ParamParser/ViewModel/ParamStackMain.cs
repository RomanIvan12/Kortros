using Autodesk.Revit.DB;
using Kortros.ParamParser.Model;
using Kortros.ParamParser.ViewModel.Commands;
using Kortros.ParamParser.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Kortros.ParamParser.ViewModel
{
    public partial class ParamStackVM : INotifyPropertyChanged
    {

        private ParameterItem selectedParameterItemInit;
        public ParameterItem SelectedParameterItemInit
        {
            get { return selectedParameterItemInit; }
            set
            {
                selectedParameterItemInit = value;
                OnPropertyChanged("SelectedParameterItemInit");
                if (selectedParameterItemInit != null)
                {
                    SetPropertyValuesForParameterStackInit();
                    IsParameterStackCorrect();
                    GetParamStacks2();
                }
            }
        }

        private ParameterItemCommon selectedParameterItemTarg;
        public ParameterItemCommon SelectedParameterItemTarg
        {
            get { return selectedParameterItemTarg; }
            set
            {
                selectedParameterItemTarg = value;
                OnPropertyChanged("SelectedParameterItemTarg");
                if (selectedParameterItemTarg != null)
                {
                    SetPropertyValuesForParameterStackTarg();
                    IsParameterStackCorrect();
                    GetParamStacks2();
                }
            }
        }

        private string chosenPickElements;

        public string ChosenPickElements
        {
            get { return chosenPickElements; }
            set
            {
                chosenPickElements = value;
                OnPropertyChanged("ChosenPickElements");
            }
        }


        public ObservableCollection<Config> Configs { get; set; }
        public ObservableCollection<CategoryItem> CategoryItems { get; set; }
        public ObservableCollection<ParamStack> ParamStacks { get; set; }
        public ObservableCollection<ParameterItem> ParamItemsInit { get; set; }
        public ObservableCollection<ParameterItemCommon> ParamItemsTarget { get; set; } // Отфильтрованные элементы без тех, чей StorageType - ElementID
        public ObservableCollection<string> ChoosePick { get; set; }
        public ObservableCollection<Element> ElementsToParse { get; set; }
        public ObservableCollection<DataItem> DataItems { get; set; }


        public NewConfigCommand NewConfigCommand { get; set; }
        public NewCategoryItemCommand NewCategoryItemCommand { get; set; }
        public NewParamStackCommand NewParamStackCommand { get; set; }
        public DeleteConfigCommand DeleteConfigCommand { get; set; }
        public DeleteCategoryItemCommand DeleteCategoryItemCommand { get; set; }
        public DeleteParamStackCommand DeleteParamStackCommand { get; set; }

        public CheckAllCategoryItemsCommand CheckAllCategoryItemsCommand { get; set; }
        public UnCheckAllCategoryItemsCommand UnCheckAllCategoryItemsCommand { get; set; }

        public CopyParamStackCommand CopyParamStackCommand { get; set; }

        public CheckBoxClickCommand CheckBoxClickCommand { get; set; }
        public ExportDataCsvCommand ExportDataCsvCommand { get; set; }

        public CalculateCommand CalculateCommand { get; set; }

        public ExportCommand ExportCommand { get; set; }
        public ImportCommand ImportCommand { get; set; }

        #region Изображения для описания

        private ObservableCollection<string> _images;
        public ObservableCollection<string> Images
        {
            get { return _images; }
            set
            {
                _images = value;
                OnPropertyChanged("Images");
            }
        }

        private string _currentImage;
        public string CurrentImage
        {
            get { return _currentImage; }
            set
            {
                _currentImage = value;
                OnPropertyChanged("CurrentImage");
            }
        }

        #endregion


        public event PropertyChangedEventHandler PropertyChanged;

        public ParamStackVM()
        {
            NewConfigCommand = new NewConfigCommand(this);
            DeleteConfigCommand = new DeleteConfigCommand(this);

            NewCategoryItemCommand = new NewCategoryItemCommand(this);
            DeleteCategoryItemCommand = new DeleteCategoryItemCommand(this);

            NewParamStackCommand = new NewParamStackCommand(this);
            DeleteParamStackCommand = new DeleteParamStackCommand(this);
            CopyParamStackCommand = new CopyParamStackCommand(this);

            CheckAllCategoryItemsCommand = new CheckAllCategoryItemsCommand(this);
            UnCheckAllCategoryItemsCommand = new UnCheckAllCategoryItemsCommand(this);

            CheckBoxClickCommand = new CheckBoxClickCommand(this);
            CalculateCommand = new CalculateCommand(this);
            ExportCommand = new ExportCommand(this);
            ImportCommand = new ImportCommand(this);

            ExportDataCsvCommand = new ExportDataCsvCommand(this);
            // ____________________________

            Configs = new ObservableCollection<Config>();
            CategoryItems = new ObservableCollection<CategoryItem>();
            ParamStacks = new ObservableCollection<ParamStack>();

            ParamItemsInit = new ObservableCollection<ParameterItem>();
            ParamItemsTarget = new ObservableCollection<ParameterItemCommon>();
            ElementsToParse = new ObservableCollection<Element>();
            DataItems = new ObservableCollection<DataItem>();

            ChoosePick = new ObservableCollection<string>
            {
                "Обработка всех элементов в документе",
                "Обработка элементов на текущем 3D виде",
                "Обработка выбранных элементов",
            };

            Images = new ObservableCollection<string>()
            {
                "pack://application:,,,/Kortros.ParamParser;component/Icons/sl1.png",
                "pack://application:,,,/Kortros.ParamParser;component/Icons/sl2.png",
                "pack://application:,,,/Kortros.ParamParser;component/Icons/sl3.png",
            };
            GetConfigs();
            GetCategoryItems();
            GetParamStacks();
            if (Configs.Count > 0)
                SelectedConfig = Configs[0];
        }

        /// <summary>
        /// Получение списков ParamItemsInit и ParamItemsTarget для источника данных комбобоксов
        /// </summary>
        public void GetParameterItemsForCategory()
        {
            if (SelectedCategoryItem != null)
            {
                // Получаю категорию
                string builtInSelectedCategory = SelectedCategoryItem.Name;
                ParamItemsInit.Clear();
                ParamItemsTarget.Clear();

                var temp = DatabaseHelper.ExtractDatabaseToTemporaryFile(RunCommand.Doc.Application.VersionNumber);

                // Получил список ParameterItem (встроенные) для категории 
                List<ParameterItem> selectedParItemsInit = DatabaseHelper.Read<ParameterItem>(temp).Where(i => i.Category == builtInSelectedCategory).OrderBy(i => i.Name).ToList();

                List<ParameterItemCommon> selectedParItemsTarg = DatabaseHelper.Read<ParameterItemCommon>().Where(i => i.Category == builtInSelectedCategory).OrderBy(i => i.Name).ToList(); // TARG COMMON

                // Создание списка, чтобы к ParamItemsInit добавить тип ParameterItemCommon
                List<ParameterItem> commonItemsToAdd = new List<ParameterItem>(selectedParItemsTarg.Select(
                    i => new ParameterItem
                    {
                        Category = i.Category,
                        IsBuiltIn = i.IsBuiltIn,
                        Guid = i.Guid,
                        Name = i.Name,
                        Definition = i.Definition,
                        ParameterType = i.ParameterType,
                        StorageType = i.StorageType,
                        IsType = i.IsType,
                        IsReadOnly = i.IsReadOnly,
                    }
                    )).OrderBy(i => i.Name).ToList();

                foreach (ParameterItem parameterItem in selectedParItemsInit)
                {
                    ParamItemsInit.Add(parameterItem);
                }
                foreach (ParameterItem parameterItem in commonItemsToAdd)
                {
                    ParamItemsInit.Add(parameterItem);
                }
                //List<ParameterItem> sorted = unsorted.OrderBy(i => i.Name).ToList();
                //foreach (ParameterItem parameterItem in sorted)
                //{
                //    ParamItemsInit.Add(parameterItem);
                //}

                foreach (ParameterItemCommon parameterItemCommon in selectedParItemsTarg)
                {
                    ParamItemsTarget.Add(parameterItemCommon);
                }
            }
        }


        public void SetPropertyValuesForParameterStackInit()
        {
            //if (SelectedParameterItemInit != null && SelectedParamStack != null)
            if (SelectedParamStack != null)
            {
                SelectedParamStack.IsBuiltInInit = SelectedParameterItemInit.IsBuiltIn;
                SelectedParamStack.GuidInit = selectedParameterItemInit.Guid;
                SelectedParamStack.NameInit = selectedParameterItemInit.Name;
                SelectedParamStack.DefinitionInit = selectedParameterItemInit.Definition;
                SelectedParamStack.ParameterTypeInit = selectedParameterItemInit.ParameterType;
                SelectedParamStack.StorageTypeInit = selectedParameterItemInit.StorageType;
                SelectedParamStack.IsTypeInit = selectedParameterItemInit.IsType;
            }
            DatabaseHelper.Update(SelectedParamStack);
        }

        public void SetPropertyValuesForParameterStackTarg()
        {
            if (SelectedParameterItemTarg != null && SelectedParamStack != null)
            {
                SelectedParamStack.IsBuiltInTarg = SelectedParameterItemTarg.IsBuiltIn;
                SelectedParamStack.GuidTarg = selectedParameterItemTarg.Guid;
                SelectedParamStack.NameTarg = selectedParameterItemTarg.Name;
                SelectedParamStack.DefinitionTarg = selectedParameterItemTarg.Definition;
                SelectedParamStack.ParameterTypeTarg = selectedParameterItemTarg.ParameterType;
                SelectedParamStack.StorageTypeTarg = selectedParameterItemTarg.StorageType;
                SelectedParamStack.IsTypeTarg = selectedParameterItemTarg.IsType;
            }
            DatabaseHelper.Update(SelectedParamStack);
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}