using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using ApartmentsProject.AuxiliaryСlasses;
using ApartmentsProject.Models;
using ApartmentsProject.ViewModel.Utilities;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Newtonsoft.Json;

namespace ApartmentsProject.ViewModel
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ApartmentParameterMappingVm : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ParameterMappingModel _selectedModelItem;
        public ParameterMappingModel SelectedModelItem
        {
            get => _selectedModelItem;
            set => SetProperty(ref _selectedModelItem, value);
        }

        private static Dictionary<string, string> SchemaDictionary { get; set; }
        public ObservableCollection<Parameter> TextAreaCurrencyParameters { get; set; } = 
            new ObservableCollection<Parameter>();

        public List<Parameter> SelectedParameters { get; set; } =
            new List<Parameter>();
        

        public static ObservableCollection<ParameterMappingModel> MappingModel { get; set; } =
            new ObservableCollection<ParameterMappingModel>(); // Main Collection

        public ObservableCollection<Data> ParameterOrigins { get; set; }

        private RelayCommand _createCommonParameters;
        private RelayCommand _saveSettings;
        private readonly ExternalEventHandler _externalEventHandler;
        private readonly ExternalEvent _externalEvent;

        public ApartmentParameterMappingVm()
        {
            using (Transaction t = new Transaction(RunCommand.Doc, "Initialize Schema"))
            {
                t.Start();
                ParameterFolderSchema schema = new ParameterFolderSchema(RunCommand.Doc);
                if (ParameterFolderSchema.CurrentValue != null)
                    SchemaDictionary = schema.GetValueFromDataStorage(ParameterFolderSchema.CurrentValue);
                t.Commit();
            }
            _externalEventHandler = new ExternalEventHandler();
            _externalEvent = ExternalEvent.Create(_externalEventHandler);

            CreateComboBoxCollections(); // Заполняем коллекции выше
            
            ParameterOrigins =  new ObservableCollection<Data>(ParameterMappingFunctions.Parse("Apts_KRTRS.txt"));
            CreateMappingModel(TextAreaCurrencyParameters);

            AreAllMappingModelsSet();
        }

        private void CreateMappingModel(ObservableCollection<Parameter> allParamsCollection)
        {
            MappingModel.Clear();
            var parameterComparer = new ParameterEqualityComparer();

            var savedParameterNames = SchemaDictionary
                .Where(pair => !pair.Value.Contains("00000000"))
                .Select(pair => pair.Key).ToList();

            foreach (Data parameterData in ParameterOrigins)
            {
                var collectionViewSource = new CollectionViewSource { Source = allParamsCollection };
                collectionViewSource.Filter += (s, e) =>
                {
                    if (e.Item is Parameter parameter)
                    {
                        e.Accepted = FilterParameters(parameter, parameterData);
                    };
                };
                collectionViewSource.SortDescriptions.Add(new SortDescription(nameof(Parameter.Definition.Name), ListSortDirection.Ascending));
                ((ListCollectionView)collectionViewSource.View).CustomSort = new CustomDataNamesComparer(parameterData.Name);

                var paramItem = new ParameterMappingModel()
                {
                    DataOrigin = parameterData,
                    FilteredParametersView = collectionViewSource
                };

                string parameterName = parameterData.Name;
                if (savedParameterNames.Contains(parameterName))
                {
                    var guid = SchemaDictionary[parameterName]; // это гуид

                    foreach (Parameter param in allParamsCollection)
                    {
                        if (param.GUID.ToString() == guid)
                        {
                            if (allParamsCollection.FirstOrDefault(p => parameterComparer.Equals(p, param)) is Parameter matchingParameter)
                            {
                                paramItem.Status = true;
                                paramItem.ParameterToMatch.Parameter = matchingParameter;
                                paramItem.ParameterToMatch.Name = matchingParameter.Definition.Name;
                                paramItem.ParameterToMatch.Guid = matchingParameter.GUID;
                                paramItem.ParameterToMatch.DataType =
                                    LabelUtils.GetLabelForSpec(matchingParameter.Definition.GetDataType());
                                paramItem.ParameterToMatch.UserModifiable = matchingParameter.UserModifiable;
                            }
                        }
                    }
                }
                else
                {
                    foreach (Parameter param in allParamsCollection)
                    {
                        if (param.Definition.Name == parameterName)
                        {
                            if (allParamsCollection.FirstOrDefault(p => parameterComparer.Equals(p, param)) is Parameter matchingParameter)
                            {
                                paramItem.Status = true;
                                paramItem.ParameterToMatch.Parameter = matchingParameter;
                                paramItem.ParameterToMatch.Name = matchingParameter.Definition.Name;
                                paramItem.ParameterToMatch.Guid = matchingParameter.GUID;
                                paramItem.ParameterToMatch.DataType =
                                    LabelUtils.GetLabelForSpec(matchingParameter.Definition.GetDataType());
                                paramItem.ParameterToMatch.UserModifiable = matchingParameter.UserModifiable;
                            }
                        }
                    }
                }
                MappingModel.Add(paramItem);
            }
        }
        
        private bool FilterParameters(Parameter parameter, Data parameterData)
        {
            if (GetEnglishLabel(parameter.Definition.GetDataType())
                == parameterData.DataType && !parameter.Definition.Name.StartsWith("KRT_"))
            {
                return true;
            }

            if (parameter.Definition.Name == parameterData.Name)
                return true;
            return false;
        }

        private string GetEnglishLabel(ForgeTypeId specType)
        {
            string originalLabel = LabelUtils.GetLabelForSpec(specType);
            if (SpecLabelMapping.TryGetValue(originalLabel, out var engLabel))
                return engLabel;
            return originalLabel;
        }

        private static readonly Dictionary<string, string> SpecLabelMapping = new Dictionary<string, string>()
        {
            { "Площадь", "AREA" },
            { "Текст", "TEXT" },
            { "Денежная единица", "CURRENCY" }
        };
        
        public RelayCommand CreateCommonParameters
        {
            get
            {
                return _createCommonParameters ??
                   (_createCommonParameters = new RelayCommand(obj =>
                   {
                       try
                       {
                           _externalEventHandler.SetAction(CreateParams);
                           _externalEvent.Raise();
                       }
                       catch (Exception ex)
                       {
                           MessageBox.Show($"ошибка загрузки параметров: {ex.Message}");
                       }
                   }));
            }
        }

        public RelayCommand SaveSettingsCommand
        {
            get
            {
                return _saveSettings ??
                   (_saveSettings = new RelayCommand(obj =>
                   {
                       try
                       {
                           _externalEventHandler.SetAction(SaveSettings);
                           _externalEvent.Raise();
                       }
                       catch (Exception ex)
                       {
                           MessageBox.Show(ex.Message);
                       }
                   }));
            }
        }

        public void SaveSettings()
        {
            using (Transaction t = new Transaction(RunCommand.Doc, "werwer"))
            {
                t.Start();
                new ParameterFolderSchema(RunCommand.Doc).DeleteDataStorages();
                foreach (var parameterMappingModel in MappingModel)
                {
                    //if (parameterMappingModel.DataOrigin.Name != parameterMappingModel.ParameterToMatch.Name)
                    if (parameterMappingModel.ParameterToMatch.Name != null)
                    {
                        var guid = parameterMappingModel.Parameter.GUID.ToString();
                        SchemaDictionary[parameterMappingModel.DataOrigin.Name] = guid;
                    }
                }
                ParameterFolderSchema schema = new ParameterFolderSchema(RunCommand.Doc);
                schema.SetDataStorageField(JsonConvert.SerializeObject(SchemaDictionary
                    .Select(kv => new Dictionary<string, string> { { kv.Key, kv.Value } }).ToList(), Formatting.Indented));
                t.Commit();
            }
        }

        public void CreateParams()
        {
            List<string> paramsToCreate = MappingModel.Where(p => p.ParameterToMatch?.Name == null)
                .Select(p => p.DataOrigin.Name).ToList();
            Utilities.CreateCommonParameters.CreateCommon(RunCommand.Doc, RunCommand.App, paramsToCreate);

            RefreshComboBoxCollections();
            AreAllMappingModelsSet();
            MessageBox.Show("Общие параметры созданы!");
        }

        private void CreateComboBoxCollections()
        {
            TextAreaCurrencyParameters.Clear();
            var roomElement = new FilteredElementCollector(RunCommand.Doc)
                .OfCategory(BuiltInCategory.OST_Rooms).FirstElement();
            if (roomElement == null) return;

            foreach (Parameter param in roomElement.Parameters)
            {
                if (param.IsShared)
                {
                    var parType = param.Definition.GetDataType();
                    if (parType == SpecTypeId.Area || parType == SpecTypeId.String.Text || parType == SpecTypeId.Currency)
                    {
                        TextAreaCurrencyParameters.Add(param);
                    }
                }
            }
        }

        private void RefreshComboBoxCollections()
        {
            CreateComboBoxCollections();
            CreateMappingModel(TextAreaCurrencyParameters);
        }
        
        private void AreAllMappingModelsSet()
        {
            var settings = PluginSettings.Instance;
            foreach (ParameterMappingModel mappingModel in MappingModel)
            {
                if (mappingModel.Status)
                {
                    settings.AreCommonParametersMapped = true;
                }
                else if (!mappingModel.Status)
                {
                    settings.AreCommonParametersMapped = false;
                    break;
                }
            }
        }
        
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private bool SetProperty<T>(ref T storage, T value, string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    // Создание компоратора для того, чтобы внутренний параметр был вначале!
    public class CustomDataNamesComparer : IComparer
    {
        private readonly string originName;

        public CustomDataNamesComparer(string originName)
        {
            this.originName = originName;
        }
        public int Compare(object x, object y)
        {
            var parameter1 = x as Parameter;
            var parameter2 = y as Parameter;

            if (parameter1 == null || parameter2 == null) return 0;

            // Если один из элементов - "Иван", то он идет первым
            if (parameter1.Definition.Name == originName && parameter2.Definition.Name != originName)
                return -1;
            if (parameter1.Definition.Name != originName && parameter2.Definition.Name == originName)
                return 1;

            // В противном случае сортировка по алфавиту
            return string.Compare(parameter1.Definition.Name, parameter2.Definition.Name, StringComparison.Ordinal);
        }
    }

    public class ParameterEqualityComparer : IEqualityComparer<Parameter>
    {
        public bool Equals(Parameter x, Parameter y)
        {
            if (x == null || y == null) return false;
            return x.GUID == y.GUID && x.Definition.Name.Equals(y.Definition.Name);
        }
        public int GetHashCode(Parameter obj)
        {
            if (obj == null) return 0;

            int hash = 17;
            hash = hash * 23 + obj.GUID.GetHashCode();
            hash = hash * 23 + (obj.Definition.Name != null ? obj.Definition.Name.GetHashCode() : 0);
            return hash;
        }
    }
}
