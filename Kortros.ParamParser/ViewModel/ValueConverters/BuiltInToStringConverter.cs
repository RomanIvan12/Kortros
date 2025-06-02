using Autodesk.Revit.DB;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Kortros.ParamParser.ViewModel.ValueConverters
{
    public class BuiltInToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string category = (string)value;

            try
            {
                Category cat = Category.GetCategory(RunCommand.Doc, (BuiltInCategory)Enum.Parse(typeof(BuiltInCategory), category));
                return cat.Name;
            }
            catch (Exception)
            {
                return category + "_Ошибка конвертации имени";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
