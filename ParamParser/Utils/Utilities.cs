using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;

namespace ParamParser
{
    internal class Utilities
    {
        public static void ShowList<T>(IEnumerable<T> items)
        {
            string message = string.Join("\n", items);
            MessageBox.Show(message, "Список элементов");
        }

        public static void ShowDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
        {
            StringBuilder messageBuilder = new StringBuilder();

            foreach (KeyValuePair<TKey, TValue> pair in dictionary)
            {
                string keyString = pair.Key.ToString();
                string valueString = GetFormattedValue(pair.Value);

                string pairString = $"{keyString}: {valueString}";
                messageBuilder.AppendLine(pairString);
            }

            string message = messageBuilder.ToString();
            MessageBox.Show(message, "Элементы словаря", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static string GetFormattedValue<T>(T value)
        {
            if (value is Dictionary<object, object> nestedDictionary)
            {
                StringBuilder nestedBuilder = new StringBuilder();
                foreach (KeyValuePair<object, object> nestedPair in nestedDictionary)
                {
                    string nestedKeyString = nestedPair.Key.ToString();
                    string nestedValueString = GetFormattedValue(nestedPair.Value);
                    string nestedPairString = $"{nestedKeyString}: {nestedValueString}";
                    nestedBuilder.AppendLine(nestedPairString);
                }
                return nestedBuilder.ToString().TrimEnd();
            }

            return value.ToString();
        }

    }
}
