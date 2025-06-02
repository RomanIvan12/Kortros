using ApartmentsProject.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ApartmentsProject.ViewModel.Utilities.Converters
{
    public class IsRoomMatrixEntryInvalidConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is RoomMatrixEntry entry)
            {
                // Проверка на пустые значения
                if (string.IsNullOrWhiteSpace(entry.Name) ||
                    entry.RoomType != "Жилое" || entry.RoomType != "Нежилое" ||
                    entry.FinishingThickness < 0 ||
                    entry.AreaFactor <= 0 || entry.NumberPriority < 0)
                    return true;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
