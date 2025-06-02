using ApartmentsProject.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ApartmentsProject.ViewModel.Utilities.Converters
{
    class ApartmentNameToBrushMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return new SolidColorBrush(Colors.Transparent);

<<<<<<< HEAD
            if (values[0] is MatrixApartmentModel model && values[1] is List<string> roomNameList) // 
=======
            if (values[0] is MatrixApartmentModel model && values[1] is List<string> roomNameList)
>>>>>>> 048b79eed65eae5a9f8add08e5b25a2bb0a8e3c8
            {
                if (roomNameList.Contains(model.ApartmentName))
                    return new SolidColorBrush(Colors.LightGreen);
                else
                    return new SolidColorBrush(Colors.LightGray);
            }
            return new SolidColorBrush(Colors.Transparent);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
