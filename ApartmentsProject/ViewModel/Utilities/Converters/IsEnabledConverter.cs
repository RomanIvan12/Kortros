using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Autodesk.Revit.DB;

namespace ApartmentsProject.ViewModel.Utilities.Converters
{
    public class IsEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Parameter currentParameter))
                return true;
            var selectedItems = parameter as List<Parameter>;
            if (selectedItems == null)
                return true;
            return !selectedItems.Contains(currentParameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
