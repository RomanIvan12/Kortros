using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB;

namespace ApartmentsProject.ViewModel.Utilities
{
    public class Functions
    {
        /*
        Функция, в которая обрабатывает элементы, находящиеся в группе
        Входные параметры: Элемент, параметр, значение для записи
        */
        public static void SetParameter(Document doc, Element element, string parameterName, string stringValue)
        {
            // Проверка, находится ли элемент в группе
            if (element.GroupId.IntegerValue == -1)
            {
                element.LookupParameter(parameterName).Set(stringValue);
            }
            else
            {
                Group group = doc.GetElement(element.GroupId) as Group;
                Type groupType = group.GetType();
                MessageBox.Show(group.Name);
            }
        }

        public static void ShowProperties(object obj)
        {
            if (obj == null)
            {
                MessageBox.Show("Object is null");
                return;
            }

            Type type = obj.GetType();
            PropertyInfo[] properties = type.GetProperties();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Properties if {type.Name}:");

            foreach (PropertyInfo prop in properties)
            {
                try
                {
                    var value = prop.GetValue(obj, null);
                    sb.AppendLine($"{prop.Name}: {value}");
                }
                catch (TargetInvocationException ex)
                {
                    sb.AppendLine($"{prop.Name}: Error retrieving value ({ex.InnerException.Message})");
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"{prop.Name}: Error retrieving value ({ex.Message})");
                }
            }
            MessageBox.Show(sb.ToString());
        }
    }
}
