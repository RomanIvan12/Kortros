using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using ApartmentsProject.AuxiliaryСlasses;
using ApartmentsProject.Domain;
using ApartmentsProject.Models;
using ApartmentsProject.ViewModel.Utilities;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

namespace ApartmentsProject.ViewModel
{
    public partial class ApartmentCalculationVm : INotifyPropertyChanged
    {
        private CollectionViewSource _filteredSortParameters;
        private CollectionViewSource _filteredValueSortParameters;

        private string _filterSortText;
        private string _filterValueSortText;

        private SelectableParameterModel _selectedSortParameter;
        private string _selectedValueSortParameter;

        private string _labelValue;
        private Room _selectedUnplacedRoom;

        public CollectionViewSource FilteredSortParameters
        {
            get
            {
                if (_filteredSortParameters == null)
                {
                    _filteredSortParameters = new CollectionViewSource { Source = SortParameters };
                    _filteredSortParameters.Filter += (s, e) =>
                    {
                        if (string.IsNullOrWhiteSpace(FilterSortText))
                        {
                            e.Accepted = true;
                            return;
                        }
                        if (e.Item is SelectableParameterModel item)
                        {
                            e.Accepted = item.Name.IndexOf(FilterSortText, StringComparison.OrdinalIgnoreCase) >= 0;
                        }
                        else
                        {
                            e.Accepted = false;
                        }
                    };

                    _filteredSortParameters.SortDescriptions.Add(
                        new SortDescription("Name", ListSortDirection.Ascending));
                }
                return _filteredSortParameters;
            }
        }

        public CollectionViewSource FilteredValueSortParameters
        {
            get
            {
                if (_filteredValueSortParameters == null)
                {
                    _filteredValueSortParameters = new CollectionViewSource { Source = ValueSortParameters };
                    _filteredValueSortParameters.Filter += (s, e) =>
                    {
                        if (string.IsNullOrWhiteSpace(FilterValueSortText))
                        {
                            e.Accepted = true;
                            return;
                        }

                        if (e.Item is SelectableParameterModel item)
                        {
                            e.Accepted = item.Name.IndexOf(FilterSortText, StringComparison.OrdinalIgnoreCase) >= 0;
                        }
                        else
                        {
                            e.Accepted = false;
                        }
                    };
                }
                return _filteredValueSortParameters;
            }
        }

        public string FilterSortText
        {
            get => _filterSortText;
            set
            {
                if (_filterSortText != value)
                {
                    _filterSortText = value;
                    OnPropertyChanged(nameof(FilterSortText));
                    FilteredSortParameters.View.Refresh();
                }
            }
        }
        public string FilterValueSortText
        {
            get => _filterValueSortText;
            set
            {
                if (_filterValueSortText != value)
                {
                    _filterValueSortText = value;
                    OnPropertyChanged(nameof(FilterValueSortText));
                    FilteredValueSortParameters.View.Refresh();
                }
            }
        }
        public SelectableParameterModel SelectedSortParameter
        {
            get => _selectedSortParameter;
            set
            {
                _selectedSortParameter = value;
                OnPropertyChanged(nameof(SelectedSortParameter));
                SetProperty(ref _selectedSortParameter, value);
                if (SelectedSortParameter.Name != null)
                    UpdateValidValuesSortParameters(SelectedSortParameter.Name);

            }
        }
        public string SelectedValueSortParameter
        {
            get => _selectedValueSortParameter;
            set => SetProperty(ref _selectedValueSortParameter, value);
        }
        public string LabelValue
        {
            get => _labelValue;
            set => SetProperty(ref _labelValue, value);
        }

        public Room SelectedUnplacedRoom
        {
            get => _selectedUnplacedRoom;
            set => SetProperty(ref _selectedUnplacedRoom, value);
        }


        public ObservableCollection<SelectableParameterModel> SortParameters { get; set; } = new ObservableCollection<SelectableParameterModel>(); // Параметр в поле "Параметр группирования квартир"
        public ObservableCollection<string> ValueSortParameters { get; set; } = new ObservableCollection<string>(); // Параметр в поле "Значения параметра"

        private RelayCommand _getAllRooms;
        private RelayCommand _getUnplacedRooms;
        private RelayCommand _getInvalidRooms;

        public static ObservableCollection<Room> Rooms { get; set; } = new ObservableCollection<Room>(); // MAIN
        public static ObservableCollection<Room> UnplacedRooms { get; set; } = new ObservableCollection<Room>();
        public static ObservableCollection<InvalidRoom> InvalidRooms { get; set; } = new ObservableCollection<InvalidRoom>();

        //private PluginSettings _pluginSettings;
        //public PluginSettings PluginSettings
        //{
        //    get => _pluginSettings;
        //    set => SetProperty(ref _pluginSettings, value);
        //}


        public RelayCommand GetAllRooms
        {
            get
            {
                return _getAllRooms ??
                       (_getAllRooms = new RelayCommand(obj =>
                       {
                           try
                           {
                               _externalEventHandler.SetAction(GetRoomsForSelectedValue);
                               _externalEvent.Raise();
                           }
                           catch (Exception ex)
                           {
                               MessageBox.Show($"ошибка загрузки параметров: {ex.Message}");
                           }
                       }));
            }
        }

        public RelayCommand GetUnplacedRooms
        {
            get
            {
                return _getUnplacedRooms ??
                       (_getUnplacedRooms = new RelayCommand(obj =>
                       {
                           GetRoomsUnplaced();
                       }));
            }
        }

        public RelayCommand GetInvalidRoomsCommand
        {
            get
            {
                return _getInvalidRooms ??
                       (_getInvalidRooms = new RelayCommand(obj => { GetInvalidRooms(); }));
            }
        }
    }
}
