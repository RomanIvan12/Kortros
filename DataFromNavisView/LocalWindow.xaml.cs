using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using DataFromNavisView.Helpers;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Document = Autodesk.Revit.DB.Document;

namespace DataFromNavisView
{
    public partial class LocalWindow : Window
    {
        private readonly string _host = "localhost";
        private readonly string _port = "5432";
        private readonly string _user = "postgres";
        private readonly string _database = "Projects_parameters";

        private readonly Document _doc;
        private static ForgeTypeId LengthUnits;
        private static ForgeTypeId AreaUnits;
        private static ForgeTypeId VolumeUnits;

        public LocalWindow(Document document)
        {
            InitializeComponent();
            _doc = document;
            LengthUnits = _doc.GetUnits().GetFormatOptions(SpecTypeId.Length).GetUnitTypeId();
            AreaUnits = _doc.GetUnits().GetFormatOptions(SpecTypeId.Area).GetUnitTypeId();
            VolumeUnits = _doc.GetUnits().GetFormatOptions(SpecTypeId.Volume).GetUnitTypeId();
        }

        private void ConnectBtn_Click(object sender, RoutedEventArgs e)
        {
            string password = PassName.Password;
            //LoadDatabaseList(password);
            LoadProjects(password);
        }

        private void LoadProjects(string password)
        {
            string connString = $"Host={_host};Port={_port};Username={_user};Password={password};Database={_database}";
            using (var conn = new NpgsqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT project_name FROM projects";

                    using (var command = new NpgsqlCommand(query, conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            List<string> projectNames = new List<string>();
                            while (reader.Read())
                            {
                                projectNames.Add(reader.GetString(0));
                            }
                            ProjectsList.ItemsSource = projectNames;
                            Logger.Log.Info("Список проектов получен");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log.Error("Connection error: " + ex.Message);
                    MessageBox.Show("Connection error: " + ex.Message);
                }
                EnsureExportTableExists(conn);
            }
        }
        
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            StringBuilder sb = new StringBuilder();

            string connectionString = $"Host={_host};Port={_port};Username={_user};Password={PassName.Password};Database={_database}";
            
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                Logger.Log.Info("Соединение установлено.");
                sb.AppendLine("Соединение установлено.");

                // Step 1. Add row to export table
                int newExportId = AddExportByProjectName(connection, ProjectsList.Text);
                Logger.Log.Info($"Выбранная база данных: {ProjectsList.Text}.");

                // Перебор связей, и на каждый проект по таблице
                // получил уникальные экземпляры
                var linkInstances = new FilteredElementCollector(_doc)
                    .OfClass(typeof(RevitLinkInstance))
                    .Cast<RevitLinkInstance>()
                    .GroupBy(item => item.GetTypeId().IntegerValue)
                    .Select(group => group.First());

                foreach (var linkInstance in linkInstances)
                {
                    var linkDoc = linkInstance.GetLinkDocument();
                    if (linkDoc == null) continue;

                    // Получаем 3д вид Navisworks
                    View view = new FilteredElementCollector(linkDoc).OfClass(typeof(View3D))
                        .Cast<View>()
                        .Where(i => !i.IsTemplate)
                        .FirstOrDefault(view3d => view3d.Name == "Navisworks");

                    if (view == null)
                    {
                        sb.AppendLine($"{linkInstance.Name}: - проект не содержит вида Navisworks. Он будет пропущен");
                        Logger.Log.Info($"{linkInstance.Name}: - проект не содержит вида Navisworks. Он будет пропущен");
                        continue;
                    }

                    // Step 2. Create new table
                    CreateSubProjectDataTable(connection);

                    // Step 3. Get new name of fileName
                    string fileName = linkInstance.Name.IndexOf(".rvt") != -1 ? linkInstance.Name.Substring(0, linkInstance.Name.IndexOf(".rvt")) : linkInstance.Name;
                    
                    var listOfData = GetDataFromLink(linkDoc, view, fileName);

                    // Step 4. Add data at the table
                    InsertDataIntoSubprojectTable(connection, newExportId, listOfData);

                    Logger.Log.Info($"Element_params - количество строк - {listOfData.Count}.");
                }

                Logger.Log.Info("Все операции успешно выполнены.");
                stopwatch.Stop();
                TimeSpan elapsedTime = stopwatch.Elapsed;

                sb.AppendLine($"Операция выполнена за {elapsedTime.TotalSeconds} секунд.");
                Logger.Log.Info($"Операция выполнена за {elapsedTime.TotalSeconds} секунд.");
                MessageBox.Show(sb.ToString());
            }
        }

        /// <summary>
        /// Добавляет новую запись в таблицу exports, используя project_name для поиска project_id.
        /// </summary>
        public static int AddExportByProjectName(NpgsqlConnection connection, string projectName)
        {
            // Находим project_id по имени проекта
            string getProjectIdQuery = "SELECT project_id FROM projects WHERE project_name = @projectName;";
            int projectId;

            using (var getIdCommand = new NpgsqlCommand(getProjectIdQuery, connection))
            {
                getIdCommand.Parameters.AddWithValue("projectName", projectName);
                var result = getIdCommand.ExecuteScalar();

                if (result == null)
                    throw new Exception($"Проект с именем '{projectName}' не найден.");

                projectId = (int)result;
                Console.WriteLine($"Найден project_id = {projectId} для проекта '{projectName}'");
            }


            // Вставляем новую запись в таблицу exports
            string insertExportQuery = "INSERT INTO exports (project_id, export_date) VALUES (@projectId, @exportDate) RETURNING export_id;";
            using (var insertCommand = new NpgsqlCommand(insertExportQuery, connection))
            {
                insertCommand.Parameters.AddWithValue("projectId", projectId);
                insertCommand.Parameters.AddWithValue("exportDate", DateTime.UtcNow.Date);

                int exportId = (int)insertCommand.ExecuteScalar();
                Console.WriteLine($"Добавлена запись в exports: export_id = {exportId}");
                return exportId;
            }
        }

        public void EnsureExportTableExists(NpgsqlConnection connection)
        {
            var connectionString = $"Host={_host};Port={_port};Username={_user};Password={PassName.Password};Database={_database}";

            // Проверяем, есть ли таблица export
            string checkTableQuery = $@"
                SELECT EXISTS (
                    SELECT 1
                    FROM information_schema.tables
                    WHERE table_schema = 'public'
                        AND table_name = 'exports'
                );";
            bool tableExists;
            using (var checkCmd = new NpgsqlCommand(checkTableQuery, connection))
            {
                tableExists = (bool)checkCmd.ExecuteScalar();
            }
            if (!tableExists)
            {
                // Create Table
                string createTableQuery = $@"
                    CREATE TABLE public.exports (
                    export_id SERIAL PRIMARY KEY,
                    project_id INTEGER,
                    export_date DATE NOT NULL
                );";
                using (var cmd = new NpgsqlCommand(createTableQuery, connection))
                {
                    cmd.ExecuteNonQuery();
                    Logger.Log.Info($"Создана таблица exports");
                }
            }
        }


        public static void CreateSubProjectDataTable(NpgsqlConnection connection)
        {
            string query = $@"
            CREATE TABLE IF NOT EXISTS Element_params (
                id BIGSERIAL PRIMARY KEY,
                export_id INTEGER REFERENCES exports(export_id),
                element_type VARCHAR(255),
                file_name VARCHAR(255),
                element_id INTEGER,
                parameter_name VARCHAR(255),
                parameter_value VARCHAR,
                is_built_in BOOLEAN
            );";

            using (var cmd = new NpgsqlCommand(query, connection))
            {
                cmd.ExecuteNonQuery();
                Logger.Log.Info($"Создана таблица Element_params");
            }
        }

        public static List<ParameterDataTemp> GetDataFromLink(Document doc, View view, string fileName)
        {
            List<ParameterDataTemp> data = new List<ParameterDataTemp>();
            HashSet<int> processedTypeIds = new HashSet<int>();

            var linkedFileElementsNav = new FilteredElementCollector(doc, view.Id)
                .ToElements()
                .Where(x => x.Category != null);
                //.WhereElementIsNotElementType()

            foreach (Element element in linkedFileElementsNav)
            {
                if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Cameras || // Убрал камеры, Группы моделей
                        element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_IOSModelGroups)
                    continue;


                if (element.GetTypeId().IntegerValue != -1)
                {
                    var dataItemsForElement = CreateParamData(doc, element, fileName, processedTypeIds);
                    data.AddRange(dataItemsForElement);
                }
            }
            return data;
        }

        private static List<ParameterDataTemp> CreateParamData(Document doc, Element element, string fileName, HashSet<int>processedTypeIds)
        {
            List<ParameterDataTemp> result = new List<ParameterDataTemp>();

            foreach (Parameter parameter in element.Parameters)
            {
                if (parameter.Id.IntegerValue == (int)BuiltInParameter.ELEM_CATEGORY_PARAM_MT)
                    continue;

                ParameterDataTemp dataRaw = new ParameterDataTemp()
                {
                    FileName = fileName,
                    ElementId = element.Id.IntegerValue,
                    SymbolOrType = "Экземпляр",
                    ParameterName = parameter.Definition.Name,
                    ParameterValue = GetParameterValue(doc, parameter),
                    IsBuiltIn = (parameter.Definition as InternalDefinition)?.BuiltInParameter == BuiltInParameter.INVALID
                };
                result.Add(dataRaw);
            }

            ElementId typeId = element.GetTypeId();
            int typeIdint = element.GetTypeId().IntegerValue;
            if (typeId != ElementId.InvalidElementId && !processedTypeIds.Contains(typeIdint))
            {
                processedTypeIds.Add(typeIdint);
                var elementType = doc.GetElement(typeId);

                foreach (Parameter parameter in elementType.Parameters)
                {
                    if (parameter.Id.IntegerValue == (int)BuiltInParameter.ELEM_CATEGORY_PARAM_MT)
                        continue;

                    ParameterDataTemp dataRaw = new ParameterDataTemp()
                    {
                        FileName = fileName,
                        ElementId = elementType.Id.IntegerValue,
                        SymbolOrType = "Тип",
                        ParameterName = parameter.Definition.Name,
                        ParameterValue = GetParameterValue(doc, parameter),
                        IsBuiltIn = (parameter.Definition as InternalDefinition)?.BuiltInParameter == BuiltInParameter.INVALID
                    };
                    result.Add(dataRaw);
                }
            }
            return result;
        }

        private static object GetParameterValue(Document doc, Parameter parameter)
        {
            switch (parameter.StorageType)
            {
                case StorageType.String:
                    return string.IsNullOrEmpty(parameter.AsString())
                        ? null
                        : parameter.AsString();
                case StorageType.Double:
                    var rounding = UtilityFunctions.GetParameterAccuracy(doc, parameter);
                    int decimalPlaces = (int)Math.Abs(Math.Log10(rounding));
                    return
                        Math.Round(
                            UnitUtils.ConvertFromInternalUnits(parameter.AsDouble(),
                                parameter.GetUnitTypeId()), decimalPlaces);
                case StorageType.ElementId:
                    return parameter.AsValueString();

                case StorageType.Integer:
                    return parameter.AsInteger();

                default:
                    return null;
            }
        }


        public static void InsertDataIntoSubprojectTable(NpgsqlConnection connection, int exportId, List<ParameterDataTemp>dataElements)
        {
            string query = $@"
            INSERT INTO Element_params (export_id, file_name, element_id, element_type, parameter_name, parameter_value, is_built_in) 
            VALUES (@exportId, @fileName, @elementId, @elementType, @parameterName, @parameterValue, @isBuiltIn);";

            using (var transaction = connection.BeginTransaction())
            {
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.Add(new NpgsqlParameter("exportId", NpgsqlDbType.Integer));
                    cmd.Parameters.Add(new NpgsqlParameter("fileName", NpgsqlDbType.Varchar));
                    cmd.Parameters.Add(new NpgsqlParameter("elementId", NpgsqlDbType.Integer));
                    cmd.Parameters.Add(new NpgsqlParameter("elementType", NpgsqlDbType.Varchar));
                    cmd.Parameters.Add(new NpgsqlParameter("parameterName", NpgsqlDbType.Varchar));
                    cmd.Parameters.Add(new NpgsqlParameter("parameterValue", NpgsqlDbType.Varchar));
                    cmd.Parameters.Add(new NpgsqlParameter("isBuiltIn", NpgsqlDbType.Boolean));
                    
                    foreach (var data in dataElements)
                    {
                        //cmd.Parameters["fileName"].Value = string.IsNullOrEmpty(data.FileName) ? (object)DBNull.Value : data.FileName;
                        cmd.Parameters["exportId"].Value = exportId;
                        cmd.Parameters["fileName"].Value = data.FileName;
                        cmd.Parameters["elementId"].Value = data.ElementId;
                        cmd.Parameters["elementType"].Value = data.SymbolOrType;
                        cmd.Parameters["parameterName"].Value = data.ParameterName;
                        cmd.Parameters["isBuiltIn"].Value = data.IsBuiltIn;

                        //cmd.Parameters["parameterValue"].Value = data.ParameterValue;

                        if (data.ParameterValue == null)
                        {
                            cmd.Parameters["parameterValue"].Value = DBNull.Value;
                        }
                        else
                        {
                            string serializedValue = JsonConvert.SerializeObject(data.ParameterValue);
                            cmd.Parameters["parameterValue"].Value = serializedValue;
                        }

                        cmd.ExecuteNonQuery();
                    }
                }
                transaction.Commit();
            }
        }
        

        private void OkMaterials_Click(object sender, RoutedEventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            StringBuilder sb = new StringBuilder();

            string connectionString = $"Host={_host};Port={_port};Username={_user};Password={PassName.Password};Database={_database}";

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                Logger.Log.Info("Соединение установлено.");
                sb.AppendLine("Соединение установлено.");

                // Step 1. Add row to export table
                int newExportId = AddExportByProjectName(connection, ProjectsList.Text);
                Logger.Log.Info($"Выбранная база данных: {ProjectsList.Text}.");

                // Перебор связей, и на каждый проект по таблице
                // получил уникальные экземпляры
                var linkInstances = new FilteredElementCollector(_doc)
                    .OfClass(typeof(RevitLinkInstance))
                    .Cast<RevitLinkInstance>()
                    .GroupBy(item => item.GetTypeId().IntegerValue)
                    .Select(group => group.First());

                foreach (var linkInstance in linkInstances)
                {
                    var linkDoc = linkInstance.GetLinkDocument();
                    if (linkDoc == null) continue;

                    // Получаем 3д вид Navisworks
                    View view = new FilteredElementCollector(linkDoc).OfClass(typeof(View3D))
                        .Cast<View>()
                        .Where(i => !i.IsTemplate)
                        .FirstOrDefault(view3d => view3d.Name == "Navisworks");

                    if (view == null)
                    {
                        sb.AppendLine($"{linkInstance.Name}: - проект не содержит вида Navisworks. Он будет пропущен");
                        Logger.Log.Info($"{linkInstance.Name}: - проект не содержит вида Navisworks. Он будет пропущен");
                        continue;
                    }
                    // Step 2. Create new table if not exists
                    CreateSubProjectMaterialDataTable(connection);

                    // Step 2. Get fileName
                    string fileName = linkInstance.Name.IndexOf(".rvt") != -1 ? linkInstance.Name.Substring(0, linkInstance.Name.IndexOf(".rvt")) : linkInstance.Name;

                    var listOfDataMaterials = GetMaterialDataFromLink(linkDoc, view, fileName);
                    listOfDataMaterials = listOfDataMaterials.Where(i => i != null).ToList();

                    // Step 4. Add data at the table
                    InsertMaterialDataIntoSubprojectTable(connection, newExportId, listOfDataMaterials);
                    Logger.Log.Info($"Materials: - количество строк - {listOfDataMaterials.Count}.");
                }

                Logger.Log.Info("Все операции успешно выполнены.");
                stopwatch.Stop();
                TimeSpan elapsedTime = stopwatch.Elapsed;

                sb.AppendLine($"Операция выполнена за {elapsedTime.TotalSeconds} секунд.");
                Logger.Log.Info($"Операция выполнена за {elapsedTime.TotalSeconds} секунд.");
                MessageBox.Show(sb.ToString());
            }
        }

        public static void InsertMaterialDataIntoSubprojectTable(NpgsqlConnection connection, int exportId, List<MaterialData> dataElements)
        {
            string query = $@"
            INSERT INTO Materials (export_id, file_name, element_id, material_name, material_key, width, area, area_system, volume, base_element_id, base_element_category) 
            VALUES (@exportId, @fileName, @elementId, @materialName, @materialKey, @width, @area, @area_system, @volume, @baseElementId, @baseElementCategory);";


            using (var transaction = connection.BeginTransaction())
            {
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.Add(new NpgsqlParameter("exportId", NpgsqlDbType.Integer));
                    cmd.Parameters.Add(new NpgsqlParameter("fileName", NpgsqlDbType.Varchar));
                    cmd.Parameters.Add(new NpgsqlParameter("elementId", NpgsqlDbType.Integer));
                    cmd.Parameters.Add(new NpgsqlParameter("materialName", NpgsqlDbType.Varchar));
                    cmd.Parameters.Add(new NpgsqlParameter("materialKey", NpgsqlDbType.Varchar));
                    cmd.Parameters.Add(new NpgsqlParameter("width", NpgsqlDbType.Double));
                    cmd.Parameters.Add(new NpgsqlParameter("area", NpgsqlDbType.Double));
                    cmd.Parameters.Add(new NpgsqlParameter("area_system", NpgsqlDbType.Double));
                    cmd.Parameters.Add(new NpgsqlParameter("volume", NpgsqlDbType.Double));
                    cmd.Parameters.Add(new NpgsqlParameter("baseElementId", NpgsqlDbType.Integer));
                    cmd.Parameters.Add(new NpgsqlParameter("baseElementCategory", NpgsqlDbType.Varchar));

                    foreach (var data in dataElements)
                    {
                        cmd.Parameters["exportId"].Value = exportId;
                        cmd.Parameters["fileName"].Value = data.FileName;
                        cmd.Parameters["elementId"].Value = data.ElementId;
                        cmd.Parameters["materialName"].Value = data.MaterialName;
                        cmd.Parameters["materialKey"].Value = data.MaterialKey;
                        cmd.Parameters["width"].Value = data.Width;
                        cmd.Parameters["area"].Value = data.Area;
                        cmd.Parameters["area_system"].Value = data.AreaSystem;
                        cmd.Parameters["volume"].Value = data.Volume;
                        cmd.Parameters["baseElementId"].Value = data.BaseElementId;
                        cmd.Parameters["baseElementCategory"].Value = data.BaseElementCategory;

                        cmd.ExecuteNonQuery();
                    }
                }
                transaction.Commit();
            }
        }


        public static void CreateSubProjectMaterialDataTable(NpgsqlConnection connection)
        {
            string query = $@"
            CREATE TABLE IF NOT EXISTS Materials (
                id BIGSERIAL PRIMARY KEY,
                export_id INTEGER REFERENCES exports(export_id),
                file_name VARCHAR,
                element_id INTEGER,
                material_name VARCHAR,
                material_key VARCHAR,
                width DOUBLE PRECISION,
                area DOUBLE PRECISION,
                area_system DOUBLE PRECISION,
                volume DOUBLE PRECISION,
                base_element_id INTEGER,
                base_element_category VARCHAR(255)
            );";

            using (var cmd = new NpgsqlCommand(query, connection))
            {
                cmd.ExecuteNonQuery();
                Logger.Log.Info($"Создана таблица Materials");
            }
        }

        public static List<MaterialData> GetMaterialDataFromLink(Document doc, View view, string fileName)
        {
            List<MaterialData> materialData = new List<MaterialData>();

            List<BuiltInCategory> listOfCategories = new List<BuiltInCategory>()
            {
                BuiltInCategory.OST_Walls,
                BuiltInCategory.OST_Floors,
                BuiltInCategory.OST_Roofs,
                BuiltInCategory.OST_StructuralColumns,
                BuiltInCategory.OST_Ramps,
                BuiltInCategory.OST_Stairs,
            };

            foreach (var category in listOfCategories)
            {
                var linkedFileElementsNav = new FilteredElementCollector(doc, view.Id)
                    .OfCategory(category)
                    .WhereElementIsNotElementType()
                    .ToElements();
                foreach (Element element in linkedFileElementsNav)
                {
                    if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Walls
                        || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Floors
                        || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Roofs)
                    {
                        materialData.AddRange(CreateMaterialDataForLayers(doc, element, fileName)); //
                    }
                    else if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Ramps)
                    {
                        var materialIds = element.GetMaterialIds(false);
                        if (materialIds.Any())
                        {
                            var materialId = materialIds.First();

                            var item = new MaterialData()
                            {
                                FileName = fileName,
                                ElementId = materialId.IntegerValue,
                                MaterialName = doc.GetElement(materialId).Name,
                                MaterialKey = doc.GetElement(materialId).get_Parameter(BuiltInParameter.KEYNOTE_PARAM).AsString(),
                                Area =
                                    UnitUtils.ConvertFromInternalUnits(element.GetMaterialArea(materialId, false), AreaUnits),
                                Volume = UnitUtils.ConvertFromInternalUnits(element.GetMaterialVolume(materialId), VolumeUnits),
                                BaseElementId = element.Id.IntegerValue,
                                BaseElementCategory = element.Category.Name
                            };
                            materialData.Add(item);
                        }
                    }
                    else if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_StructuralColumns
                             && element.GetType() == typeof(FamilyInstance))
                    {
                        var item = FamilyInstanceData(doc, element, fileName);
                        materialData.Add(item);
                    }
                    else if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Stairs)
                    {
                        var itemList = GetMaterialDataFromStairs(doc, element, fileName);
                        materialData.AddRange(itemList);
                    }
                }
            }
            return materialData;
        }
        
        private static List<MaterialData> CreateMaterialDataForLayers(Document doc, Element element, string fileName)
        {
            var materialData = new List<MaterialData>();

            var compoundStructure = GetCompoundStructureLayers(element);

            if (element is FamilyInstance)
            {
                materialData.Add(FamilyInstanceData(doc, element, fileName));
                return materialData;
            }

            // Если структура слоёв пустая
            if (compoundStructure == null || !compoundStructure.Any())
            {
                return materialData;
            }
            // Обработка однослойных элементов
            if (compoundStructure.Count() == 1)
            {
                var layer = compoundStructure.First();
                AddMaterialData(doc, element, materialData, layer, 1, fileName);
                return materialData;
            }
            // Для многослойных элементов
            var materialIds = compoundStructure.Select(layer => layer.MaterialId.IntegerValue).ToList();
            foreach (var layer in compoundStructure)
            {
                var numberOfLayers = materialIds.Count(id => id == layer.MaterialId.IntegerValue);
                AddMaterialData(doc, element, materialData, layer, numberOfLayers, fileName);
            }
            return materialData;
        }

        private static IEnumerable<CompoundStructureLayer> GetCompoundStructureLayers(Element element)
        {
            switch (element.Category.Id.IntegerValue)
            {
                case (int)BuiltInCategory.OST_Walls:
                    var wall = element as Wall;
                    if (wall?.WallType.Kind != WallKind.Curtain)
                    {
                        return wall?.WallType.GetCompoundStructure().GetLayers().OrderBy(i => i.LayerId);
                    }
                    return null;
                case (int)BuiltInCategory.OST_Floors:
                    return (element as Floor)?.FloorType.GetCompoundStructure().GetLayers().OrderBy(i => i.LayerId);
                case (int)BuiltInCategory.OST_Roofs:
                    if (element is FootPrintRoof)
                    {
                        if ((element as FootPrintRoof)?.RoofType.GetCompoundStructure() != null)
                            return (element as FootPrintRoof)?.RoofType.GetCompoundStructure().GetLayers()
                                .OrderBy(i => i.LayerId);
                    }
                    if (element is RoofBase)
                    {
                        if((element as RoofBase)?.RoofType.GetCompoundStructure() != null)
                            return (element as RoofBase)?.RoofType.GetCompoundStructure().GetLayers()
                                .OrderBy(i => i.LayerId);
                    }
                    return null;
                default:
                    return null;
            }
        }

        private static void AddMaterialData(Document doc, Element element, List<MaterialData> materialData, CompoundStructureLayer layer, int numberOfLayers, string fileName)
        {
            var materialId = layer.MaterialId;
            var item = new MaterialData()
            {
                FileName = fileName,
                ElementId = materialId.IntegerValue != -1 ? materialId.IntegerValue : -1,
                MaterialName = materialId.IntegerValue != -1 ? doc.GetElement(materialId)?.Name : "<По категории>",
                MaterialKey = materialId.IntegerValue != -1 ? doc.GetElement(materialId).get_Parameter(BuiltInParameter.KEYNOTE_PARAM).AsString() : "",
                Width = UnitUtils.ConvertFromInternalUnits(layer.Width, LengthUnits),
                BaseElementId = element.Id.IntegerValue,
                BaseElementCategory = element.Category.Name
            };

            if (materialId.IntegerValue != -1)
            {
                // С материалом
                item.Area = UnitUtils.ConvertFromInternalUnits(element.GetMaterialArea(materialId, false), AreaUnits) /
                            numberOfLayers;
                item.Volume = UnitUtils.ConvertFromInternalUnits(element.GetMaterialVolume(materialId), VolumeUnits);
            }
            else
            {
                // Без материала
                item.Area = 0;
                item.Volume = UnitUtils.ConvertFromInternalUnits(
                    element.get_Parameter(BuiltInParameter.HOST_VOLUME_COMPUTED)?.AsDouble() ?? 0, VolumeUnits) / numberOfLayers;
            }
            item.AreaSystem = UnitUtils.ConvertFromInternalUnits(
                element.get_Parameter(BuiltInParameter.HOST_AREA_COMPUTED)?.AsDouble() ?? 0, AreaUnits);
            materialData.Add(item);
        }



        private void Both_Click(object sender, RoutedEventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            StringBuilder sb = new StringBuilder();


            string connectionString = $"Host={_host};Port={_port};Username={_user};Password={PassName.Password};Database={_database}";

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                Logger.Log.Info("Соединение установлено.");
                sb.AppendLine("Соединение установлено.");

                // Step 1. Add row to export table
                int newExportId = AddExportByProjectName(connection, ProjectsList.Text);

                Logger.Log.Info($"Выбранная база данных: {ProjectsList.Text}.");

                // Перебор связей, и на каждый проект по таблице
                // получил уникальные экземпляры
                var linkInstances = new FilteredElementCollector(_doc)
                    .OfClass(typeof(RevitLinkInstance))
                    .Cast<RevitLinkInstance>()
                    .GroupBy(item => item.GetTypeId().IntegerValue)
                    .Select(group => group.First());

                foreach (var linkInstance in linkInstances)
                {
                    var linkDoc = linkInstance.GetLinkDocument();
                    if (linkDoc == null) continue;

                    // Получаем 3д вид Navisworks
                    View view = new FilteredElementCollector(linkDoc).OfClass(typeof(View3D))
                        .Cast<View>()
                        .Where(i => !i.IsTemplate)
                        .FirstOrDefault(view3d => view3d.Name == "Navisworks");

                    if (view == null)
                    {
                        sb.AppendLine($"{linkInstance.Name}: - проект не содержит вида Navisworks. Он будет пропущен");
                        Logger.Log.Info($"{linkInstance.Name}: - проект не содержит вида Navisworks. Он будет пропущен");
                        continue;
                    }
                    // Step 2. Create new table if not exists
                    CreateSubProjectDataTable(connection);

                    // Step 3. Get fileName
                    string fileName = linkInstance.Name.IndexOf(".rvt") != -1 ? linkInstance.Name.Substring(0, linkInstance.Name.IndexOf(".rvt")) : linkInstance.Name;

                    var listOfData = GetDataFromLink(linkDoc, view, fileName);

                    // Step 4. Add data at the table
                    InsertDataIntoSubprojectTable(connection, newExportId, listOfData);
                    Logger.Log.Info($"Element_params - количество строк - {listOfData.Count}.");
                    

                    // Step 2.1. Create new table if not exists
                    CreateSubProjectMaterialDataTable(connection);

                    var listOfDataMaterials = GetMaterialDataFromLink(linkDoc, view, fileName);
                    listOfDataMaterials = listOfDataMaterials.Where(i => i != null).ToList();

                    // Step 2.4. Add data at the table
                    InsertMaterialDataIntoSubprojectTable(connection, newExportId, listOfDataMaterials);
                    Logger.Log.Info($"Materials - количество строк - {listOfDataMaterials.Count}.");
                }

                Logger.Log.Info("Все операции успешно выполнены.");
                stopwatch.Stop();
                TimeSpan elapsedTime = stopwatch.Elapsed;

                sb.AppendLine($"Операция выполнена за {elapsedTime.TotalSeconds} секунд.");
                Logger.Log.Info($"Операция выполнена за {elapsedTime.TotalSeconds} секунд.");
                MessageBox.Show(sb.ToString());
            }
        }


        public static MaterialData FamilyInstanceData(Document doc, Element element, string fileName)
        {
            var materialIds = element.GetMaterialIds(false);

            if (materialIds.Any())
            {
                var materialId = materialIds.First();

                if (materialId.IntegerValue != -1)
                {
                    var item = new MaterialData()
                    {
                        FileName = fileName,
                        ElementId = materialId.IntegerValue,
                        MaterialName = doc.GetElement(materialId).Name,
                        MaterialKey = doc.GetElement(materialId).get_Parameter(BuiltInParameter.KEYNOTE_PARAM).AsString(),
                        Area =
                            UnitUtils.ConvertFromInternalUnits(element.GetMaterialArea(materialId, false), AreaUnits),
                        Volume = UnitUtils.ConvertFromInternalUnits(element.GetMaterialVolume(materialId), VolumeUnits),
                        BaseElementId = element.Id.IntegerValue,
                        BaseElementCategory = element.Category.Name
                    };
                    return item;
                }
            }
            else // Когда материала нет: и с солиды, и пустоты
            {
                var solidsOfElement = GetSolidsFromFamilyInstance(element);

                if (solidsOfElement.Any())
                {

                    var vol = (element.get_Parameter(BuiltInParameter.HOST_VOLUME_COMPUTED) != null)
                        ? UnitUtils.ConvertFromInternalUnits(
                            element.get_Parameter(BuiltInParameter.HOST_VOLUME_COMPUTED).AsDouble(),
                            VolumeUnits)
                        : UnitUtils.ConvertFromInternalUnits(
                            solidsOfElement.Sum(solid => solid.Volume),
                            VolumeUnits);

                    var item = new MaterialData()
                        {
                            FileName = fileName,
                            ElementId = -1,
                            MaterialName = "<По категории>",
                            MaterialKey = "",
                            Volume = vol,
                            BaseElementId = element.Id.IntegerValue,
                            BaseElementCategory = element.Category.Name
                        };
                    return item;
                }
            }
            return null;
        }

        public static List<Solid> GetSolidsFromFamilyInstance(Element element)
        {
            List<Solid> solids = new List<Solid>();

            // Получаем главный GeometryElement из экземпляра семейства
            GeometryElement geometryElement = element.get_Geometry(new Options { ComputeReferences = true });

            if (geometryElement == null)
                return solids; // Если геометрии нет, возвращаем пустой список
            var zzz = geometryElement.GetEnumerator();
            foreach (var obj in geometryElement)
            {
                if (obj is GeometryInstance geomInstance)
                {
                    GeometryElement instanceGeometry = geomInstance.GetInstanceGeometry();

                    // Обрабатываем все объекты внутри экземплярной геометрии
                    foreach (GeometryObject instanceObject in instanceGeometry)
                    {
                        // Проверяем, является ли объект Solid
                        if (instanceObject is Solid solid)
                        {
                            // Добавляем Solid в список, если он содержит геометрию
                            if (solid.Faces.Size > 0 && solid.Edges.Size > 0)
                            {
                                solids.Add(solid);
                            }
                        }
                    }
                }
            }
            return solids;
        }

        public static List<MaterialData> GetMaterialDataFromStairs(Document doc, Element element, string fileName)
        {
            List<MaterialData> data = new List<MaterialData>();

            // Площадки
            if (element is Stairs stairs)
            {
                var stairsLanding = stairs.GetStairsLandings();
                foreach (var landingId in stairsLanding)
                {
                    var landingElement = doc.GetElement(landingId);
                    data.Add(FamilyInstanceData(doc, landingElement, fileName));
                }
                var stairsRuns = stairs.GetStairsRuns();
                foreach (var runId in stairsRuns)
                {
                    var runElement = doc.GetElement(runId);
                    data.Add(FamilyInstanceData(doc, runElement, fileName));
                }
            }
            return data;
        }
    }


    public class MaterialData
    {
        public string FileName { get; set; }
        public int ElementId { get; set; }
        public string MaterialName { get; set; }
        public string MaterialKey { get; set; }

        private double _width;
        public double Width
        {
            get => _width;
            set => _width = Math.Round(value, 2);
        }
        private double _area;
        public double Area
        {
            get => _area;
            set => _area = Math.Round(value, 2);
        }

        private double _areaSystem;
        public double AreaSystem
        {
            get => _areaSystem;
            set => _areaSystem = Math.Round(value, 2);
        }
        private double _volume;
        public double Volume
        {
            get => _volume;
            set => _volume = Math.Round(value, 3);
        }
        public int BaseElementId { get; set; }
        public string BaseElementCategory { get; set; }
    }
    public class ParameterDataTemp
    {
        public string FileName { get; set; }
        public int ElementId { get; set; }
        public string SymbolOrType { get; set; }
        public string ParameterName { get; set; }
        public object ParameterValue { get; set; }
        public bool IsBuiltIn { get; set; }
    }

}
