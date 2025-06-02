using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ExporterFromRs.Helpers
{
    public class JSONConverter
    {
        private string expPath;
        private string pilPath;

        public string ExpPath
        {
            get { return expPath; }
            set { expPath = value; }
        }
        public string PilPath
        {
            get { return pilPath; }
            set { pilPath = value; }
        }

        public JSONConverter(string folderPath = null)
        {
            Dictionary<string, Dictionary<string, string>> pathesToExport = new Dictionary<string, Dictionary<string, string>>
                {
                    { "2020", new Dictionary<string, string>
                        {
                            { "001_Южный порт", "T:\\Renova-SG\\Common\\Пользователи\\BIM-проектирование\\Проекты\\001 Южный порт\\09 BIM\\03 OUT" },
                            { "002_А.Власова", "T:\\Renova-SG\\Common\\Пользователи\\BIM-проектирование\\Проекты\\002 А.Власова\\09 BIM\\03 OUT" },
                            { "OLD", "value" },
                            { "Конструктор планировок", "T:\\Renova-SG\\Common\\Пользователи\\BIM-проектирование\\Проекты\\000 Конструктор планировок\\09 BIM\\03 OUT" },
                        }
                    },
                    { "2021", new Dictionary<string, string>
                        {
                            { "001_Южный порт", "T:\\Renova-SG\\Common\\Пользователи\\BIM-проектирование\\Проекты\\001 Южный порт\\09 BIM\\03 OUT" },
                            { "002_А.Власова", "T:\\Renova-SG\\Common\\Пользователи\\BIM-проектирование\\Проекты\\002 А.Власова\\09 BIM\\03 OUT" },
                            { "OLD", "value2" }
                        }
                    },
                    { "2022", new Dictionary<string, string>
                        {
                            { "001_Южный порт", "T:\\Renova-SG\\Common\\Пользователи\\BIM-проектирование\\Проекты\\001 Южный порт\\09 BIM\\03 OUT" },
                            { "002_А.Власова", "T:\\Renova-SG\\Common\\Пользователи\\BIM-проектирование\\Проекты\\002 А.Власова\\09 BIM\\03 OUT" },
                            { "003_Квартал 837", "value" },
                            { "004_ДОО ЮП", "value" },
                            { "005_СОШ ЮП", "value" },
                            { "006_Веткино", "value" },
                            { "OLD", "value" },
                            { "TEST", "C:\\Users\\IvannikovRV\\Desktop\\SaveFiles" },
                            { "Конструктор планировок", "T:\\Renova-SG\\Common\\Пользователи\\BIM-проектирование\\Проекты\\000 Конструктор планировок\\09 BIM\\03 OUT" }
                        }
                    },
                    { "2023", new Dictionary<string, string>
                        {
                            { "001_Южный порт", "T:\\Renova-SG\\Common\\Пользователи\\BIM-проектирование\\Проекты\\001 Южный порт\\09 BIM\\03 OUT" },
                            { "002_А.Власова", "T:\\Renova-SG\\Common\\Пользователи\\BIM-проектирование\\Проекты\\002 А.Власова\\09 BIM\\03 OUT" },
                            { "003_Квартал 837", "value" },
                            { "004_ДОО ЮП", "value" },
                            { "005_СОШ ЮП", "value" },
                            { "006_Веткино", "value" },
                            { "OLD", "value" },
                            { "Конструктор планировок", "T:\\Renova-SG\\Common\\Пользователи\\BIM-проектирование\\Проекты\\000 Конструктор планировок\\09 BIM\\03 OUT" },
                            { "Офис Москва", "T:\\Renova-SG\\Common\\Пользователи\\BIM-проектирование\\Проекты\\000 Офис Москва\\09 BIM\\03 OUT" }
                        }
                    },
                    { "2024", new Dictionary<string, string>
                        {
                            { "key", "value" },
                            { "key1", "value2" }
                        }
                    }
                };

            if (folderPath == null)
            {
                folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }

            expPath = Path.Combine(folderPath, "pathesToExport.json");
            
            if (!File.Exists(expPath))
            {
                CreateJson(pathesToExport, expPath);
            }
        }

        public void CreateJson(Dictionary<string, Dictionary<string, string>> dictionary, string path)
        {
            string jsonToPilot = JsonConvert.SerializeObject(dictionary, Formatting.Indented);
            File.WriteAllText(path, jsonToPilot);
        }

        public string GetJsonValue(string jsonfilePath, string revVersion, string ProjectName)
        {
            string json = File.ReadAllText(jsonfilePath);
            JObject jsonObject = JObject.Parse(json);

            string pathValue = (string)jsonObject[revVersion][ProjectName];

            return pathValue;
        }
    }
}
