using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Autodesk.Revit.DB;

namespace ApartmentsProject.ViewModel.Utilities.Converters
{
    public class DataTypeFilterConverter : IValueConverter
    {
        private static readonly Dictionary<string, string> SpecLabelMapping = new Dictionary<string, string>
        {
            { "Площадь", "AREA" },
            { "Текст", "TEXT" },
            { "Денежная единица", "CURRENCY" }
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CollectionViewSource collectionViewSource && parameter is string dataType)
            {
                var expectedType = SpecLabelMapping.FirstOrDefault(x => x.Value == dataType).Key;
                collectionViewSource.View.Filter =
                    item => GetEnglishLabel((item as Parameter)?.Definition?.GetDataType()) == expectedType;
                return collectionViewSource.View;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private string GetEnglishLabel(ForgeTypeId specType)
        {
            string originalLabel = LabelUtils.GetLabelForSpec(specType);
            if (SpecLabelMapping.TryGetValue(originalLabel, out var engLabel))
                return engLabel;
            return originalLabel;
        }
    }
}
