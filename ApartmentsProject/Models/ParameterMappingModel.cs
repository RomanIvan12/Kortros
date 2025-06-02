using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Autodesk.Revit.DB;

namespace ApartmentsProject.Models
{
    public class ParameterMappingModel : INotifyPropertyChanged
    {

        private CollectionViewSource _filteredParametersView;
        public CollectionViewSource FilteredParametersView
        {
            get => _filteredParametersView;
            set
            {
                if (_filteredParametersView != value)
                {
                    _filteredParametersView = value;
                    OnPropertyChanged(nameof(FilteredParametersView));
                }
            }
        }
        
        private bool _status;
        public bool Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(nameof(Status));
                }
            }
        }
        public Data DataOrigin { get; set; }


        private ParameterData _parameterToMatch;
        public ParameterData ParameterToMatch
        {
            get => _parameterToMatch;
            set
            {
                if (_parameterToMatch != value)
                {
                    _parameterToMatch = value;
                    OnPropertyChanged(nameof(ParameterToMatch));
                    UpdateStatus();
                }
            }
        }
        public Parameter Parameter
        {
            get => _parameterToMatch.Parameter;
            set
            {
                if (_parameterToMatch.Parameter != value)
                {
                    _parameterToMatch.Parameter = value;
                    SetParameterToMatch(value);
                    OnPropertyChanged(nameof(Parameter));
                }
            }
        }
        private void UpdateStatus()
        {
            if (ParameterToMatch == null || ParameterToMatch.DataType == null 
                                         || ParameterToMatch.Name == null 
                                         || ParameterToMatch.Parameter == null)
                Status = false;
            else
                Status = true;
        }
        private void SetParameterToMatch(Parameter parameter)
        {
            if (parameter != null)
            {
                if (_parameterToMatch == null)
                {
                    _parameterToMatch = new ParameterData();
                }

                Status = true;  // or additional logic if necessary
                _parameterToMatch.Parameter = parameter;
                _parameterToMatch.Name = parameter.Definition.Name;
                _parameterToMatch.Guid = parameter.GUID;
                _parameterToMatch.DataType = LabelUtils.GetLabelForSpec(parameter.Definition.GetDataType());
                _parameterToMatch.UserModifiable = parameter.UserModifiable;
            }
        }

        public ParameterMappingModel()
        {
            _parameterToMatch = new ParameterData
            {
                Parameter = null
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Data
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public string Description { get; set; }
    }

    public class ParameterData
    {
        public string Name { get; set; }
        public Parameter Parameter { get; set; }
        public Guid Guid { get; set; }
        public string DataType { get; set; }
        public bool UserModifiable { get; set; }
    }
}