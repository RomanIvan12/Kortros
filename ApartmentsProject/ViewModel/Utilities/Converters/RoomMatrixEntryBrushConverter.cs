using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;

namespace ApartmentsProject.ViewModel.Utilities.Converters
{
    public class RoomMatrixEntryBrushConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.Length != 2)
                return Brushes.Black;

            var matrixRoomName = value[0] as string;
            var allRooms = value[1] as ObservableCollection<string>;

            if (allRooms != null && !allRooms.Contains(matrixRoomName))
                return Brushes.Gray;
            return Brushes.Black;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
