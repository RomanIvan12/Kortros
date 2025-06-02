using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ApartmentsProject.ViewModel.Utilities.Converters
{
    public class DataTypeToTypeCollectionsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dataType = value as string;
            if (dataType == "TEXT")
                return ApartmentParameterMappingVm.TextParameters;
            if (dataType == "AREA")
                return ApartmentParameterMappingVm.AreaParameters;
            if (dataType == "CURRENCY")
                return ApartmentParameterMappingVm.CurrencyParameters;
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
