using Autodesk.Revit.DB;
using Kortros.General.MVVM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Excel = Microsoft.Office.Interop.Excel;

namespace Kortros.General.ExcelSync
{
    public class ExcelSyncViewModel : ViewModelBase
    {
        private Handler handler;
        private Dictionary<string, Dictionary<string, object>> data;

        public string FileName
        {
            get { return Config.FilePath; }
            set
            {
                Config.FilePath = value;
                OnPropertyChanged();
            }
        }

        public string MarkParName
        {
            get { return Config.MarkParName; }
            set
            {
                Config.MarkParName = value;
                OnPropertyChanged();
            }
        }

        public string TypeMarkParName
        {
            get { return Config.TypeMarkParName; }
            set
            {
                Config.TypeMarkParName = value;
                OnPropertyChanged();
            }
        }

        private string warningMessage = "";
        public string WarningMessage
        {
            get { return warningMessage; }
            set
            {
                warningMessage = value;
                OnPropertyChanged();
            }
        }

        private bool ready = false;
        public bool Ready
        {
            get { return ready; }
            set
            {
                ready = value;
                OnPropertyChanged();
            }
        }


        public bool UpdateOnlySelected
        {
            get { return Config.UpdateOnlySelected; }
            set
            {
                Config.UpdateOnlySelected = value;
                OnPropertyChanged();
            }
        }

        public List<Category> Categories => handler.Categories;

        private Category selectedCategory;
        public Category SelectedCategory
        {
            get { return selectedCategory; }
            set
            {
                selectedCategory = value;
                OnPropertyChanged();
            }
        }

        public ExcelSyncViewModel(Handler handler)
        {
            this.handler = handler;

            if (!string.IsNullOrEmpty(FileName) && File.Exists(FileName))
            {
                data = LoadDataFromExcel(FileName);
                Ready = true;
            }
            else
            {
                FileName = null;
            }
        }

        public void Run()
        {
            if (data == null)
            {
                return;
            }

            handler.Run(data, UpdateOnlySelected, SelectedCategory);
        }

        public void GetData()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel files (*.xls;*.xlsx)|*.xls;*.xlsx";
            if (openFileDialog.ShowDialog() == true)
            {
                FileName = openFileDialog.FileName;
                data = LoadDataFromExcel(openFileDialog.FileName);
                Ready = true;
            }
        }

        private Dictionary<string, Dictionary<string, object>> LoadDataFromExcel(string filePath)
        {
            var data = new Dictionary<string, Dictionary<string, object>>();
            Excel.Application app = new Excel.Application();
            Excel.Workbook workbook = app.Workbooks.Open(filePath);
            Excel._Worksheet worksheet = workbook.Sheets[1];
            Excel.Range range = worksheet.UsedRange;

            int rowCount = range.Rows.Count;
            int colCount = range.Columns.Count;

            int paramCount = 0;
            for (int j = 1; j <= colCount; j++)
            {
                string parName = range.Cells[1, j].Value2?.ToString() as string;
                if (j == 1)
                {
                    MarkParName = parName;
                }
                if (parName == null || j == 1) continue;
                paramCount++;
            }

            var values = range.Value2;

            var uniqueMarks = new HashSet<string>();
            var duplicatedmarks = new List<(int, string)>();
            var emptyMarksIndeces = new List<int>();

            for (int i = 3; i <= rowCount; i++)
            {
                var mark = values[i, 1]?.ToString() as string;
                if (string.IsNullOrEmpty(mark))
                {
                    emptyMarksIndeces.Add(i);
                    Console.WriteLine($"Пустая марка! Строка: {i}");
                    continue;
                }
                if (uniqueMarks.Contains(mark))
                {
                    Console.WriteLine($"Дубликат марки! {mark}");
                    duplicatedmarks.Add((i, mark));
                    continue;
                }
                uniqueMarks.Add(mark);

                data[mark] = new Dictionary<string, object>();

                int idx = 0;
                for (int j = 1; j <= colCount; j++)
                {
                    var parName = values[1, j]?.ToString() as string;
                    if (string.IsNullOrEmpty(parName) || j == 1) continue;

                    var parValue = values[i, j]?.ToString() as string;

                    data[mark][parName] = parValue;
                    idx++;
                }
            }

            if (duplicatedmarks.Count > 0)
            {
                WarningMessage += $"Дублирующиеся марки (номер/марка): {string.Join(", ", duplicatedmarks.Select(m => $"{m.Item1}/{m.Item2}"))}\n";
            }
            if (emptyMarksIndeces.Count > 0)
            {
                WarningMessage += $"Пустые марки (номера строк): {string.Join(", ", emptyMarksIndeces)}\n";
            }

            //cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();

            //release com objects to fully kill excel process from running in the background
            Marshal.ReleaseComObject(range);
            Marshal.ReleaseComObject(worksheet);

            //close and release
            workbook.Close();
            Marshal.ReleaseComObject(workbook);

            //quit and release
            app.Quit();
            Marshal.ReleaseComObject(app);

            return data;
        }
    }
}
