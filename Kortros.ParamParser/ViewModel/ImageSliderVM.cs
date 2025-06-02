using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Kortros.ParamParser.ViewModel
{
    public class ImageSliderVM
    {
        public IList<int> Slides { get; } = new List<int>
        {
            0, 1, 2
        };

        public int SelectedIndex { get; set; }
    }
    public class IndexToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int index && parameter is BitmapImage[] images && index >= 0 && index < images.Length)
            {
                return images[index];
            }

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
