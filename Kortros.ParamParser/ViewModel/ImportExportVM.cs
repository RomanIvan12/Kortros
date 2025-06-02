using Kortros.ParamParser.Model;
using Kortros.ParamParser.ViewModel.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Kortros.ParamParser.ViewModel
{
    public partial class ParamStackVM
    {

        public void Export()
        {
            // Все ПарамСтаки для выбранного CategoryItem
            ObservableCollection<ParamStack> result = new ObservableCollection<ParamStack>(
                DatabaseHelper.Read<ParamStack>().Where(p =>
                CategoryItems.Select(x => x.Id).ToList()
                .Contains(p.CategoryItemId)).ToList()
                );

            ExportedData data = new ExportedData
            {
                ExportedConfigs = SelectedConfig,
                ExportedCategoryItems = CategoryItems,
                ExportedParamStacks = result,
            };

            string jsonConfig = JsonConvert.SerializeObject(data, Formatting.Indented);

            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Title = "Сохранить файл как",
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;
                try
                {
                    File.WriteAllText(filePath, jsonConfig);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public void Import()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Открыть файл настроек",
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                FilterIndex = 1,
                RestoreDirectory = true,
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                try
                {
                    string jsonFromFile = File.ReadAllText(filePath);
                    ExportedData importedData = JsonConvert.DeserializeObject<ExportedData>(jsonFromFile);
                    AddDataToDataBase(importedData);
                    GetConfigs();
                    GetCategoryItems();
                    GetParamStacks();
                    SelectedConfig = Configs.Last();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при чтении файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public static void AddDataToDataBase(ExportedData data)
        {
            Config configToImport = data.ExportedConfigs;
            ObservableCollection<CategoryItem> categoryItemsToImport = data.ExportedCategoryItems;
            ObservableCollection<ParamStack> paramStacksToImport = data.ExportedParamStacks;

            DatabaseHelper.Insert(configToImport);

            foreach (var categoryItem in categoryItemsToImport)
            {
                int idBefore = categoryItem.Id;
                categoryItem.ConfigId = configToImport.Id;
                DatabaseHelper.Insert(categoryItem);
                foreach (var param in paramStacksToImport)
                {
                    if (param.CategoryItemId == idBefore)
                    {
                        param.CategoryItemId = categoryItem.Id;
                        DatabaseHelper.Insert(param);
                    }
                }
            }
        }
    }
    public class ExportedData
    {
        public Config ExportedConfigs { get; set; }
        public ObservableCollection<CategoryItem> ExportedCategoryItems { get; set; }
        public ObservableCollection<ParamStack> ExportedParamStacks { get; set; }
    }
}
