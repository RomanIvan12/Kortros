using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autodesk.Revit.Creation;
using Npgsql;
using Document = Autodesk.Revit.DB.Document;
using DataFromNavisView.Helpers;
using System.Diagnostics;
using System.Data;
using static Autodesk.Revit.DB.SpecTypeId;
using System.Reflection;
using System.Xml.Linq;
using System.Collections.Generic;
using System;
using Autodesk.Revit.DB.Architecture;

namespace DataFromNavisView
{
    /// <summary>
    /// Логика взаимодействия для PostgreSqlWindow.xaml
    /// </summary>
    public partial class PostgreSqlWindow : Window
    {
        private readonly Document _doc;

        private readonly string _host = "172.30.20.4";
        private readonly string _port = "5432";
        private readonly string _user = "bimuser";
        private readonly string _database = "Projects_parameters";
        private readonly string _password = "I3LFYyVEB9xa?cXtAeQb1Lid";

        public PostgreSqlWindow(Document document)
        {
            InitializeComponent();
            _doc = document;

            // Connection to postgresql
            LoadProjects();

        }

        private void LoadProjects()
        {
            string connString = $"Host={_host};Port={_port};Username={_user};Password={_password};Database={_database}";

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
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            StringBuilder sb = new StringBuilder();
            
            string connectionString = $"Host={_host};Port={_port};Username={_user};Password={_password};Database={_database}";

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                Logger.Log.Info("Соединение установлено.");
                sb.AppendLine("Соединение установлено.");

                // Step 1. Add row to export table
                int newExportId = LocalWindow.AddExportByProjectName(connection, ProjectsList.Text);

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
                    LocalWindow.CreateSubProjectDataTable(connection);

                    // Step 3. Get new name of table
                    string fileName = linkInstance.Name.IndexOf(".rvt") != -1 ? linkInstance.Name.Substring(0, linkInstance.Name.IndexOf(".rvt")) : linkInstance.Name;

                    var listOfData = LocalWindow.GetDataFromLink(linkDoc, view, fileName);

                    // Step 4. Add data at the table
                    LocalWindow.InsertDataIntoSubprojectTable(connection, newExportId, listOfData);

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
        
        private void OkMaterials_Click(object sender, RoutedEventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            StringBuilder sb = new StringBuilder();

            string connectionString = $"Host={_host};Port={_port};Username={_user};Password={_password};Database={_database}";

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                Logger.Log.Info("Соединение установлено.");
                sb.AppendLine("Соединение установлено.");

                // Step 1. Add row to export table
                int newExportId = LocalWindow.AddExportByProjectName(connection, ProjectsList.Text);
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

                    // Step 3. Create new table if not exists
                    LocalWindow.CreateSubProjectMaterialDataTable(connection);


                    // Step 2. Get fileName
                    string fileName = linkInstance.Name.IndexOf(".rvt") != -1 ? linkInstance.Name.Substring(0, linkInstance.Name.IndexOf(".rvt")) : linkInstance.Name;

                    var listOfData = LocalWindow.GetMaterialDataFromLink(linkDoc, view, fileName);
                    listOfData = listOfData.Where(i => i != null).ToList();

                    // Step 4. Add data at the table
                    LocalWindow.InsertMaterialDataIntoSubprojectTable(connection, newExportId, listOfData);
                    Logger.Log.Info($"Materials - количество строк - {listOfData.Count}.");
                }

                Logger.Log.Info("Все операции успешно выполнены.");
                stopwatch.Stop();
                TimeSpan elapsedTime = stopwatch.Elapsed;

                sb.AppendLine($"Операция выполнена за {elapsedTime.TotalSeconds} секунд.");
                Logger.Log.Info($"Операция выполнена за {elapsedTime.TotalSeconds} секунд.");
                MessageBox.Show(sb.ToString());
            }
        }

        private void Both_Click(object sender, RoutedEventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            StringBuilder sb = new StringBuilder();


            string connectionString = $"Host={_host};Port={_port};Username={_user};Password={_password};Database={_database}";

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                Logger.Log.Info("Соединение установлено.");
                sb.AppendLine("Соединение установлено.");

                // Step 1. Add row to export table
                int newExportId = LocalWindow.AddExportByProjectName(connection, ProjectsList.Text);

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
                    LocalWindow.CreateSubProjectDataTable(connection);

                    // Step 3. Get fileName
                    string fileName = linkInstance.Name.IndexOf(".rvt") != -1 ? linkInstance.Name.Substring(0, linkInstance.Name.IndexOf(".rvt")) : linkInstance.Name;

                    var listOfData = LocalWindow.GetDataFromLink(linkDoc, view, fileName);
                    
                    // Step 4. Add data at the table
                    LocalWindow.InsertDataIntoSubprojectTable(connection, newExportId, listOfData);
                    Logger.Log.Info($"Element_params - количество строк - {listOfData.Count}.");


                    // Step 2.1. Create new table if not exists
                    LocalWindow.CreateSubProjectMaterialDataTable(connection);

                    var listOfDataMaterials = LocalWindow.GetMaterialDataFromLink(linkDoc, view, fileName);
                    listOfDataMaterials = listOfDataMaterials.Where(i => i != null).ToList();

                    // Step 2.4. Add data at the table
                    LocalWindow.InsertMaterialDataIntoSubprojectTable(connection, newExportId, listOfDataMaterials);
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
    }
}
