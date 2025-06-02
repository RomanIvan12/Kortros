using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Kortros.ParamParser.ViewModel
{
    public class DataItem
    {
        public int ElementId { get; set; }
        public string ElementName { get; set; }
        public string InitParameterName { get; set; }
        public object InitValue { get; set; }
        public string TargParameterName { get; set; }
        public object TargValue { get; set; }
        public ItemStatus Status { get; set; }
        public string Message { get; set; }
    }
    public enum ItemStatus
    {
        Done,
        Cancelled,
        NotCompleted,
        Error
    }
    public partial class ParamStackVM
    {
        public void ExportToCsv(ObservableCollection<DataItem> dataItems, ICloseable window)
        {
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string name = DateTime.Now.ToString("yy-MM-dd_HH_mm_ss") + ".csv";
            var filePath = Path.Combine(desktopPath,"data_" + name);

            using (var writer = new StreamWriter(filePath, false, Encoding.Unicode))
            {
                foreach (var item in dataItems)
                {
                    writer.WriteLine($"{item.ElementId};{item.ElementName};" +
                                     $"{item.InitParameterName};{item.InitValue};{item.TargParameterName};" +
                                     $"{item.TargValue};{item.Status};{item.Message}");
                }
            }
            window?.CloseWnd();
        }
    }
    public interface ICloseable
    {
        void CloseWnd();
    }
}
