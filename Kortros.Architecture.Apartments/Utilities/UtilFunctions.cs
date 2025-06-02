using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using log4net;
using log4net.Appender;
using log4net.Repository.Hierarchy;
using System.Linq;

namespace Kortros.Architecture.Apartments.Utilities
{

    public class UtilFunctions
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

        //Создание папки на сервере и копирование туда какого-л файла
        public static void CreateLogFolderAndCopy()
        {
            string sourseFilePath = MainWindow.FilePath;
            string targetFolderPath = Path.Combine(@"F:\18. BIM\BIM_DATA\06_Logs",
                Environment.UserName);

            string targetFilePath = Path.Combine(targetFolderPath, Path.GetFileName(sourseFilePath));
            try
            {
                if (!Directory.Exists(targetFolderPath))
                    Directory.CreateDirectory(targetFolderPath);
            }
            catch { }


            if (File.Exists(sourseFilePath))
                File.Copy(sourseFilePath, targetFilePath, true);
            else
                LogManager.GetLogger("ZoneCalculation").Error($"Не удалось скопировать файл лога на внешний сервер");
        }

        public static void RenamePath(string newValue, string appenderName = "BaseValue")
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
            IAppender appender = hierarchy.GetAppenders().First(a => a.Name == appenderName);
            if (appender is FileAppender fileAppender)
            {
                fileAppender.File = newValue;
                fileAppender.ActivateOptions();
            }
        }
    }
}
