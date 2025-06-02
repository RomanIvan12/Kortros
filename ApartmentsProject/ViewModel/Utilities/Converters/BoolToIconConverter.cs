using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ApartmentsProject.ViewModel.Utilities.Converters
{
    class BoolToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isTrue)
            {
                string iconPath = isTrue
                    ? "pack://application:,,,/ApartmentsProject;component/Icons/Mapping_yes.ico"
                    : "pack://application:,,,/ApartmentsProject;component/Icons/Mapping_no.ico";
                return new BitmapImage(new Uri(iconPath));
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
