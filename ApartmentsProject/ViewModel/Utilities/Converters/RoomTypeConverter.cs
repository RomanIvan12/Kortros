using ApartmentsProject.AuxiliaryСlasses;
using ApartmentsProject.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ApartmentsProject.ViewModel.Utilities.Converters
{
    public class RoomTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string enumValue)
                return EnumExtensions.GetEnumDescription(enumValue, RoomType.Living);
            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string description)
                return EnumExtensions.GetEnumValueDescription<RoomType>(description);
            return value;
        }
    }
}
