using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using ApartmentsProject.Models;
using ApartmentsProject.AuxiliaryСlasses;
using Settings = ApartmentsProject.Models.Settings;
using ApartmentsProject.ViewModel.Commands;
using ApartmentsProject.View;

namespace ApartmentsProject.ViewModel
{
    public class ConfigurationSettingsVm : INotifyPropertyChanged
    {
        private string _valueFunc = "Квартира";
        public string ValueFunc
        {
            get => _valueFunc;
            set 
            { 
                _valueFunc = value;
                OnPropertyChanged(nameof(ValueFunc));
            }
        }

        private ObservableCollection<Configuration> _configurations;
        public ObservableCollection<Configuration> Configurations
        {
            get => _configurations;
            set
            {
                if (_configurations != value)
                {
                    _configurations = value;
                    OnPropertyChanged(nameof(Configurations));
                }
            }
        }

        private Configuration _selectedConfiguration;
        public Configuration SelectedConfiguration
        {
            get => _selectedConfiguration;
            set
            {
                if (_selectedConfiguration != value)
                {
                    _selectedConfiguration = value;
                    UpdateSelectedValues();
                    OnPropertyChanged(nameof(SelectedConfiguration));

                    // Устанавливаем IsSelected = true для выбранного элемента, а для остальных = false
                    foreach (var config in Configurations)
                        config.IsSelected = config == _selectedConfiguration;
                    ConfigurationService.Instance.SaveConfiguration(RunCommand.ApartmentsProjectLayout);

                    // Уведомляем через Mediator об изменении
                    Mediator.NotifySelectedConfigurationChanged(_selectedConfiguration);
                }
            }
        }

        public AddConfig AddConfig { get; set; }
        public DeleteConfig DeleteConfig { get; set; }
        public RenameConfig RenameConfig { get; set; }


        #region Блок привязки параметров конфигурации
        public ObservableCollection<string> ClearCommonParametersBeforeCalculationOptions { get; }
        private string _selectedClearCommonParametersBeforeCalculation;
        public string SelectedClearCommonParametersBeforeCalculation
        {
            get => _selectedClearCommonParametersBeforeCalculation;
            set
            {
                if (_selectedClearCommonParametersBeforeCalculation != value)
                {
                    _selectedClearCommonParametersBeforeCalculation = value;
                    _selectedConfiguration.Settings.ClearCommonParametersBeforeCalculation =
                        EnumExtensions
                        .GetEnumValueDescription<ClearCommonParametersBeforeCalculation>(_selectedClearCommonParametersBeforeCalculation);

                    OnPropertyChanged(nameof(SelectedClearCommonParametersBeforeCalculation));
                    ConfigurationService.Instance.SaveConfiguration(RunCommand.ApartmentsProjectLayout);
                }
            }
        }
        public ObservableCollection<string> AreaRoundTypeOptions { get; }
        private string _selectedAreaRoundType;
        public string SelectedAreaRoundType
        {
            get => _selectedAreaRoundType;
            set
            {
                if (_selectedAreaRoundType != value)
                {
                    _selectedAreaRoundType = value;

                    _selectedConfiguration.Settings.AreaRoundType =
                        EnumExtensions.GetEnumValueDescription<AreaRoundType>(_selectedAreaRoundType);

                    OnPropertyChanged(nameof(SelectedAreaRoundType));
                    ConfigurationService.Instance.SaveConfiguration(RunCommand.ApartmentsProjectLayout);
                }
            }
        }
        public ObservableCollection<string> SourceAreaForFactorOptions { get; }
        private string _selectedSourceAreaForFactor;
        public string SelectedSourceAreaForFactor
        {
            get => _selectedSourceAreaForFactor;
            set
            {
                if (_selectedSourceAreaForFactor != value)
                {
                    _selectedSourceAreaForFactor = value;
                    _selectedConfiguration.Settings.SourceAreaForFactor =
                        EnumExtensions.GetEnumValueDescription<SourceAreaForFactor>(_selectedSourceAreaForFactor);

                    OnPropertyChanged(nameof(SelectedSourceAreaForFactor));
                    ConfigurationService.Instance.SaveConfiguration(RunCommand.ApartmentsProjectLayout);
                }
            }
        }
        public ObservableCollection<string> ComputeLivingAreaOptions { get; }
        private string _selectedLivingArea;
        public string SelectedLivingArea
        {
            get => _selectedLivingArea;
            set
            {
                if (_selectedLivingArea != value)
                {
                    _selectedLivingArea = value;
                    _selectedConfiguration.Settings.ComputeAreaSettings.LivingArea =
                        EnumExtensions.GetEnumValueDescription<LivingArea>(_selectedLivingArea);
                    OnPropertyChanged(nameof(SelectedLivingArea));
                    ConfigurationService.Instance.SaveConfiguration(RunCommand.ApartmentsProjectLayout);
                }
            }
        }
        public ObservableCollection<string> ComputeFlatAreaOptions { get; }
        private string _selectedFlatArea;
        public string SelectedFlatArea
        {
            get => _selectedFlatArea;
            set
            {
                if (_selectedFlatArea != value)
                {
                    _selectedFlatArea = value;
                    _selectedConfiguration.Settings.ComputeAreaSettings.FlatArea =
                        EnumExtensions.GetEnumValueDescription<FlatArea>(_selectedFlatArea);
                    OnPropertyChanged(nameof(SelectedFlatArea));
                    ConfigurationService.Instance.SaveConfiguration(RunCommand.ApartmentsProjectLayout);
                }
            }
        }
        #endregion


        public ConfigurationSettingsVm()
        {
            AddConfig = new AddConfig(this);
            DeleteConfig = new DeleteConfig(this);
            RenameConfig = new RenameConfig(this);

            var settings = PluginSettings.Instance;
            settings.RevitDoc = RunCommand.Doc;

            Configurations = new ObservableCollection<Configuration>(RunCommand.ApartmentsProjectLayout.ConfigurationList);
            SelectedConfiguration = RunCommand.ApartmentsProjectLayout.ConfigurationList.First(
                i => i.IsSelected == true);

            // Заполнение списков возможных значений
            ClearCommonParametersBeforeCalculationOptions = new ObservableCollection<string>(
                Enum.GetValues(typeof(ClearCommonParametersBeforeCalculation))
                .Cast<ClearCommonParametersBeforeCalculation>()
                .Select(x => x.GetDescription())
                );
            AreaRoundTypeOptions = new ObservableCollection<string>(
                Enum.GetValues(typeof(AreaRoundType))
                .Cast<AreaRoundType>()
                .Select(x => x.GetDescription())
                );
            SourceAreaForFactorOptions = new ObservableCollection<string>(
                Enum.GetValues(typeof(SourceAreaForFactor))
                .Cast<SourceAreaForFactor>()
                .Select(x => x.GetDescription())
                );
            ComputeLivingAreaOptions = new ObservableCollection<string>(
                Enum.GetValues(typeof(LivingArea))
                .Cast<LivingArea>()
                .Select(x=> x.GetDescription())
                );
            ComputeFlatAreaOptions = new ObservableCollection<string>(
                Enum.GetValues(typeof(FlatArea))
                .Cast<FlatArea>()
                .Select(x=>x.GetDescription())
                );
            UpdateSelectedValues();
        }

        public void AddConfiguration()
        {
            var popup = new AddConfigPopUpWindow();

            popup.Top = RunCommand.MainWindow.Top + (RunCommand.MainWindow.Height - popup.Height) / 2;
            popup.Left = RunCommand.MainWindow.Left + (RunCommand.MainWindow.Width - popup.Width) / 2;
            if (popup.ShowDialog() == true)
            {
                if (!string.IsNullOrWhiteSpace(popup.ConfigName))
                {
                    var newConfig = new Configuration()
                    {
                        Name = popup.ConfigName,
                        IsSelected = true,
                        Settings = new Settings()
                        {
                            ClearCommonParametersBeforeCalculation = ClearCommonParametersBeforeCalculation.ClearAll.ToString(),
                            AreaRoundType = AreaRoundType.Tenth.ToString(),
                            SourceAreaForFactor = SourceAreaForFactor.RoundedArea.ToString(),
                            ComputeAreaSettings = new ComputeAreaSettings()
                            {
                                LivingArea = LivingArea.RoundedAreaWithFactor.ToString(),
                                FlatArea = FlatArea.RoundedAreaWithFactor.ToString()
                            }
                        },
                        RoomMatrix = new RoomMatrix(),
                        ApartmentType = new ApartmentType(),
                        NumberingSettings = new NumberingSettings()
                        {
                            NumberingDirection = NumberingDirection.Сlockwise.ToString(),
                            NumberingStart = NumberingStart.Top.ToString(),
                            InitNumber = 1,
                            ResetNumberForEachLevel = false,
                            AddPrefix = false,
                            FixToNumber = "кв. №",
                            IsPrefix = true
                        }
                    };

                    foreach (var configItem in Configurations)
                        configItem.IsSelected = false;
                    Configurations.Add(newConfig);

                    SelectedConfiguration = newConfig;

                    RunCommand.ApartmentsProjectLayout.ConfigurationList = Configurations.ToList();
                    ConfigurationService.Instance.SaveConfiguration(RunCommand.ApartmentsProjectLayout);
                }
            }
        }

        public void RenameConfiguration()
        {
            var popup = new RenameConfigPopUpWindow(SelectedConfiguration.Name);
            popup.Top = RunCommand.MainWindow.Top + (RunCommand.MainWindow.Height - popup.Height) / 2;
            popup.Left = RunCommand.MainWindow.Left + (RunCommand.MainWindow.Width - popup.Width) / 2;

            if (popup.ShowDialog() == true)
            {
                if (!string.IsNullOrWhiteSpace(popup.ConfigName))
                {
                    SelectedConfiguration.Name = popup.ConfigName;

                    RunCommand.ApartmentsProjectLayout.ConfigurationList = Configurations.ToList();
                    ConfigurationService.Instance.SaveConfiguration(RunCommand.ApartmentsProjectLayout);
                }
            }
        }

        public void DeleteConfiguration()
        {
            if (Configurations == null || SelectedConfiguration == null)
                return;

            if (Configurations.Count == 1)
                return;

            var index = Configurations.IndexOf(SelectedConfiguration);

            if (index >= 0)
            {
                if (index == 0)
                    SelectedConfiguration = Configurations.Count > 1 ? Configurations[1] : default;
                else
                    SelectedConfiguration = Configurations[index - 1];

                Configurations.RemoveAt(index);
            }
            ConfigurationService.Instance.SaveConfiguration(RunCommand.ApartmentsProjectLayout);
        }

        private void UpdateSelectedValues()
        {
            if (_selectedConfiguration != null)
            {
                SelectedClearCommonParametersBeforeCalculation =
                    EnumExtensions.GetEnumDescription(
                        _selectedConfiguration.Settings.ClearCommonParametersBeforeCalculation,
                        ClearCommonParametersBeforeCalculation.ClearAll);
                SelectedAreaRoundType =
                    EnumExtensions.GetEnumDescription(
                        _selectedConfiguration.Settings.AreaRoundType,
                        AreaRoundType.Tenth);
                SelectedSourceAreaForFactor =
                    EnumExtensions.GetEnumDescription(
                        _selectedConfiguration.Settings.SourceAreaForFactor, SourceAreaForFactor.RoundedArea);
                SelectedLivingArea =
                    EnumExtensions.GetEnumDescription(
                        _selectedConfiguration.Settings.ComputeAreaSettings.LivingArea, LivingArea.RoundedAreaWithFactor);
                SelectedFlatArea =
                    EnumExtensions.GetEnumDescription(
                        _selectedConfiguration.Settings.ComputeAreaSettings.FlatArea, FlatArea.RoundedAreaWithFactor);
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
