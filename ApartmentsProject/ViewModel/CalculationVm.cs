using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ApartmentsProject.AuxiliaryСlasses;
using ApartmentsProject.Models;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ApartmentsProject.ViewModel
{
    public class CalculationVm : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<double> Rounding { get; set; } = new ObservableCollection<double>() 
        {
            1, 0.1, 0.5, 0.01, 0.05, 0.001, 0.005
        };

        public ObservableCollection<string> LivingArea { get; set; } = new ObservableCollection<string>()
        {
            "Площадь округлённая",
            "Площадь округлённая с коэффициентом"
        };

        public ObservableCollection<string> ApartmentArea { get; set; } = new ObservableCollection<string>()
        {
            "Площадь округлённая",
            "Площадь округлённая с коэффициентом"
        };

        public ObservableCollection<string> CalculationType { get; set; } = new ObservableCollection<string>()
        {
            "На открытом виде",
            "Во всём проекте"
        };


        private double _selectedRounding;
        public double SelectedRounding
        {
            get => _selectedRounding;
            set
            {
                SetProperty(ref _selectedRounding, value);
            }
        }

        private string _selectedLivingArea;
        public string SelectedLivingArea
        {
            get => _selectedLivingArea;
            set
            {
                SetProperty(ref _selectedLivingArea, value);
            }
        }

        private string _selectedApartmentArea;
        public string SelectedApartmentArea
        {
            get => _selectedApartmentArea;
            set
            {
                SetProperty(ref _selectedApartmentArea, value);
            }
        }


        private readonly ExternalEventHandler _externalEventHandler;
        private readonly ExternalEvent _externalEvent;

        public CalculationVm()
        {
            _externalEventHandler = new ExternalEventHandler();
            _externalEvent = ExternalEvent.Create(_externalEventHandler);


            SelectedRounding = 0.01;
            SelectedLivingArea = "Площадь округлённая с коэффициентом";
            SelectedApartmentArea = "Площадь округлённая с коэффициентом";


        }


        private RelayCommand _mainCalculation;

        public RelayCommand CalculationCommand
        {
            get
            {
                return _mainCalculation ??
                       (_mainCalculation = new RelayCommand(obj =>
                       {
                           try
                           {
                               _externalEventHandler.SetAction(Calculate);
                               _externalEvent.Raise();
                           }
                           catch (Exception ex)
                           {
                               MessageBox.Show($"ошибка ");
                           }
                       }));
            }
        }

        private void Calculate()
        {
            using (Transaction t = new Transaction(PluginSettings.Instance.RevitDoc, "SetAreas"))
            {
                t.Start();
                foreach (var apartmentModel in ApartmentRepository.Instance.Apartments)
                {
                    CalculateAreas(apartmentModel);
                }
                t.Commit();
            }
        }




        private void CalculateAreas(ApartmentModel apartmentModel)
        {
            ForgeTypeId areaUnits = PluginSettings.Instance.RevitDoc.GetUnits().GetFormatOptions(SpecTypeId.Area).GetUnitTypeId();

            // Заполняем "Площадь округлённая" и "KRT_Площадь с коэффициентом"
            Guid areaGuid = ApartmentParameterMappingVm.MappingModel
                .First(item => item.DataOrigin.Name == "KRT_Площадь округлённая")
                .ParameterToMatch.Guid;

            Guid areaKfGuid = ApartmentParameterMappingVm.MappingModel
                .First(item => item.DataOrigin.Name == "KRT_Площадь с коэффициентом")
                .ParameterToMatch.Guid;

            Guid kfGuid = ApartmentParameterMappingVm.MappingModel
                .First(item => item.DataOrigin.Name == "KRT_Коэффициент площади")
                .ParameterToMatch.Guid;


            foreach (var room in apartmentModel.RoomsOfApartment)
            {
                var area = room.get_Parameter(BuiltInParameter.ROOM_AREA).AsDouble();
                var convertedArea = UnitUtils.ConvertFromInternalUnits(area, areaUnits);
                
                var roundedValue = RoundTo(
                    convertedArea,
                    _selectedRounding);

                var convertBackValue = UnitUtils.ConvertToInternalUnits(roundedValue, areaUnits);
                room.get_Parameter(areaGuid).Set(convertBackValue);

                var ratioValue = room.get_Parameter(kfGuid).AsDouble();

                room.get_Parameter(areaKfGuid).Set(convertBackValue * ratioValue);
            }
        }

        #region Функции округлений
        private double RoundTo(double value, double step)
        {
            int decimalPlaces = GetDecimalPlaces(step);

            // Округляем сначала на 1 знак больше, чем требуется
            int targetDec = decimalPlaces + 1;
            double targetValue = Math.Round(value, targetDec, MidpointRounding.AwayFromZero);

            // Окончательное округление
            double roundedValue = Math.Round(targetValue / step) * step;
            return roundedValue;
        }
        static int GetDecimalPlaces(double step)
        {
            int decimalPlaces = 0;

            while (step < 1)
            {
                step *= 10;
                decimalPlaces++;
            }
            return decimalPlaces;
        }
        #endregion








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
}
