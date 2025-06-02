using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Newtonsoft.Json;

namespace ApartmentsProject.ViewModel.Utilities
{
    public class ParameterFolderSchema
    {
        private readonly Document _doc;

        private static Schema _schema;

        private Entity _storageEntity;
        private DataStorage _dataStorage;

        public static string CurrentValue { get; set; }

        public ParameterFolderSchema(Document doc)
        {
            _doc = doc;

            GetDataStorageMembers(); // Создал схему!
            GetDataStorageValue(); // Посмотрел что было

            // Если DS не было, он его создаёт с пустыми значениями
            if (CurrentValue == null || string.IsNullOrEmpty(CurrentValue))
            {
                var objectList = _baseValue.Select(kv => new Dictionary<string, string> { { kv.Key, kv.Value } }).ToList();
                SetDataStorageField(JsonConvert.SerializeObject(objectList, Formatting.Indented));
            }
        }
        private static void GetSchema()
        {
            if (_schema != null)
                return;
            Guid schemaGuid = new Guid("C2DF5362-0EAD-4269-ABB0-9079A028F8A7");

            _schema = Schema.Lookup(schemaGuid);
            if (_schema == null)
            {
                SchemaBuilder schemaBuilder = new SchemaBuilder(schemaGuid);
                schemaBuilder.SetSchemaName("CommonParametersKRT");
                schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
                schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);
                schemaBuilder.AddSimpleField("Data", typeof(string));

                _schema = schemaBuilder.Finish();
            }
        }

        private void GetDataStorageMembers()
        {
            GetSchema();
            var dataStorages = new FilteredElementCollector(_doc).OfClass(typeof(DataStorage));

            foreach (var dataStorage in dataStorages)
            {
                Entity entity = dataStorage.GetEntity(_schema);

                if (entity.IsValid() && entity.Schema.SchemaName == "CommonParametersKRT")
                {
                    _storageEntity = entity;
                    _dataStorage = dataStorage as DataStorage;
                    return;
                }
            }
        }

        public string GetDataStorageValue()
        {
            // Определение Schema и Entity
            var dataStorages = new FilteredElementCollector(_doc).OfClass(typeof(DataStorage));

            foreach (var dataStorage in dataStorages)
            {
                Entity entity = dataStorage.GetEntity(_schema);
                Schema schema = entity.Schema;

                if (schema != null && schema.SchemaName == "CommonParametersKRT" && entity.IsValid())
                {
                    CurrentValue = entity.Get<string>("Data");
                    return CurrentValue;
                }
            }
            return null;
        }

        // Вставить значение по умолчанию
        public void SetDataStorageField(string data = null)
        {
            if (_dataStorage == null)
            {
                _dataStorage = DataStorage.Create(_doc);
                _storageEntity = new Entity(_schema);
            }

            if (data != null)
                _storageEntity.Set("Data", data);

            _dataStorage.SetEntity(_storageEntity);
        }

        public void DeleteDataStorages()
        {
            var dataStorages = new FilteredElementCollector(_doc).OfClass(typeof(DataStorage));

            List<ElementId> elementsToDelete = new List<ElementId>();
            foreach (var elem in dataStorages)
            {
                DataStorage dataStorage = elem as DataStorage;
                if (dataStorage != null)
                {
                    Entity entity = dataStorage.GetEntity(_schema);
                    Schema schema = entity.Schema;

                    if (schema != null && schema.SchemaName == "CommonParametersKRT")
                        elementsToDelete.Add(elem.Id);
                }
            }
            if (elementsToDelete.Count > 0)
            {
                _doc.Delete(elementsToDelete);
            }
        }

        private Dictionary<string, string> _baseValue = new Dictionary<string, string>()
        {
            { "KRT_Площадь с коэффициентом", "00000000-0000-0000-0000-000000000000" },
            { "KRT_Число жилых комнат", "00000000-0000-0000-0000-000000000000" },
            { "KRT_№ Квартиры","00000000-0000-0000-0000-000000000000" },
            { "KRT_Метка квартиры","00000000-0000-0000-0000-000000000000" },
            { "KRT_ID Квартиры","00000000-0000-0000-0000-000000000000" },
            { "KRT_Тип помещения","00000000-0000-0000-0000-000000000000" },
            { "KRT_Площадь округлённая","00000000-0000-0000-0000-000000000000" },
            { "KRT_Тип Квартиры","00000000-0000-0000-0000-000000000000" },
            { "KRT_Толщина черновой отделки","00000000-0000-0000-0000-000000000000" },
            { "KRT_Коэффициент площади","00000000-0000-0000-0000-000000000000" },
            { "KRT_Площадь квартиры","00000000-0000-0000-0000-000000000000" },
            { "KRT_Площадь общая без коэфф.","00000000-0000-0000-0000-000000000000" },
            { "KRT_Площадь жилая","00000000-0000-0000-0000-000000000000" },
            { "KRT_Функциональное назначение","00000000-0000-0000-0000-000000000000" },
            { "KRT_Площадь общая","00000000-0000-0000-0000-000000000000" },
            { "KRT_Номер помещения","00000000-0000-0000-0000-000000000000" },
            { "KRT_Площадь неотапл. помещений с коэфф.","00000000-0000-0000-0000-000000000000" },
        };

        public Dictionary<string, string> GetValueFromDataStorage(string json)
        {
            var objectList = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(json);
            return objectList.SelectMany(dict => dict).ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }
}
