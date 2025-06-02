using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using ApartmentsProject.Models;

namespace ApartmentsProject.AuxiliaryСlasses
{
    class ConfigurationService
    {
        private readonly string _configFilePath;

        private static readonly Lazy<ConfigurationService> _instance =
            new Lazy<ConfigurationService>(() => new ConfigurationService());

        private  ConfigurationService()
        {
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "KortrosPluginData"
                );
            Directory.CreateDirectory(appDataPath);
            _configFilePath = Path.Combine(appDataPath, "krtrsApartmentsProjectLayout.xml");
        }
        public static ConfigurationService Instance => _instance.Value;
        // Метод для сохранения
        public void SaveConfiguration(ApartmentsProjectLayout config)
        {
            var serializer = new XmlSerializer(typeof(ApartmentsProjectLayout));
            using (var writer = new StreamWriter(_configFilePath))
            {
                serializer.Serialize(writer, config);
            }
        }

        public ApartmentsProjectLayout LoadConfiguration()
        {
            if (!File.Exists(_configFilePath))
            {
                return new ApartmentsProjectLayout()
                {
                    ConfigurationList = new List<Configuration>()
                    {
                        new Configuration()
                        {
                            Name = "Конфигурация 1",
                            IsSelected = true,
                            Settings = new Settings()
                            {
                                ClearCommonParametersBeforeCalculation = ClearCommonParametersBeforeCalculation.ClearAll.ToString(),
                                AreaRoundType = AreaRoundType.Tenth.ToString(),
                                SourceAreaForFactor = SourceAreaForFactor.RoundedArea.ToString(),
                                ComputeAreaSettings = new ComputeAreaSettings()
                                {
                                    LivingArea = LivingArea.RoundedAreaWithFactor.ToString(),
                                    FlatArea = FlatArea.RoundedAreaWithFactor.ToString()
                                }
                            },
                            RoomMatrix = new RoomMatrix()
                            {
                                Entries = new List<RoomMatrixEntry>()
                                {
                                    new RoomMatrixEntry()
                                    {
                                        Name = "Спальня",
                                        FinishingThickness = 0,
                                        RoomType = RoomType.Living.ToString(),
                                        AreaFactor = 1,
                                        NumberPriority = 3
                                    },
                                    new RoomMatrixEntry()
                                    {
                                        Name = "Гостиная",
                                        FinishingThickness = 0,
                                        RoomType = RoomType.Living.ToString(),
                                        AreaFactor = 1,
                                        NumberPriority = 3
                                    },
                                    new RoomMatrixEntry()
                                    {
                                        Name = "Жилая комната",
                                        FinishingThickness = 0,
                                        RoomType = RoomType.Living.ToString(),
                                        AreaFactor = 1,
                                        NumberPriority = 3
                                    },
                                    new RoomMatrixEntry()
                                    {
                                        Name = "С/У",
                                        FinishingThickness = 0,
                                        RoomType = RoomType.NonLiving.ToString(),
                                        AreaFactor = 1,
                                        NumberPriority = 6
                                    },
                                    new RoomMatrixEntry()
                                    {
                                        Name = "Кухня-ниша",
                                        FinishingThickness = 0,
                                        RoomType = RoomType.NonLiving.ToString(),
                                        AreaFactor = 1,
                                        NumberPriority = 2
                                    },
                                    new RoomMatrixEntry()
                                    {
                                        Name = "Кухня-столовая",
                                        FinishingThickness = 0,
                                        RoomType = RoomType.NonLiving.ToString(),
                                        AreaFactor = 1,
                                        NumberPriority = 2
                                    },
                                    new RoomMatrixEntry()
                                    {
                                        Name = "Кухня",
                                        FinishingThickness = 0,
                                        RoomType = RoomType.NonLiving.ToString(),
                                        AreaFactor = 1,
                                        NumberPriority = 2
                                    },
                                    new RoomMatrixEntry()
                                    {
                                        Name = "Прихожая",
                                        FinishingThickness = 0,
                                        RoomType = RoomType.NonLiving.ToString(),
                                        AreaFactor = 1,
                                        NumberPriority = 1
                                    },
                                    new RoomMatrixEntry()
                                    {
                                        Name = "Гардеробная",
                                        FinishingThickness = 0,
                                        RoomType = RoomType.NonLiving.ToString(),
                                        AreaFactor = 1,
                                        NumberPriority = 5
                                    },
                                    new RoomMatrixEntry()
                                    {
                                        Name = "Коридор",
                                        FinishingThickness = 0,
                                        RoomType = RoomType.NonLiving.ToString(),
                                        AreaFactor = 1,
                                        NumberPriority = 4
                                    },
                                    new RoomMatrixEntry()
                                    {
                                        Name = "Кладовая",
                                        FinishingThickness = 0,
                                        RoomType = RoomType.NonLiving.ToString(),
                                        AreaFactor = 1,
                                        NumberPriority = 5
                                    },
                                    new RoomMatrixEntry()
                                    {
                                        Name = "Балкон",
                                        FinishingThickness = 0,
                                        RoomType = RoomType.NonLiving.ToString(),
                                        AreaFactor = 1,
                                        NumberPriority = 7
                                    },
                                    new RoomMatrixEntry()
                                    {
                                        Name = "Лоджия",
                                        FinishingThickness = 0,
                                        RoomType = RoomType.NonLiving.ToString(),
                                        AreaFactor = 1,
                                        NumberPriority = 7
                                    },
                                    new RoomMatrixEntry()
                                    {
                                        Name = "Терраса",
                                        FinishingThickness = 0,
                                        RoomType = RoomType.NonLiving.ToString(),
                                        AreaFactor = 1,
                                        NumberPriority = 7
                                    }
                                }
                            },
                            ApartmentType = new ApartmentType()
                            {
                                Entries = new List<ApartmentTypeEntry>()
                                        {
                                            new ApartmentTypeEntry()
                                            {
                                                ApartmentType = "1С",
                                                LivingRoomCount = 1,
                                                ContainRooms = "Кухня-ниша",
                                                NonContainRooms = "Кухня, Кухня-столовая"
                                            },
                                            new ApartmentTypeEntry()
                                            {
                                                ApartmentType = "1К",
                                                LivingRoomCount = 1,
                                                ContainRooms = "Кухня",
                                                NonContainRooms = "Кухня-столовая"
                                            },
                                            new ApartmentTypeEntry()
                                            {
                                                ApartmentType = "1Е",
                                                LivingRoomCount = 1,
                                                ContainRooms = "Кухня-столовая",
                                                NonContainRooms = "Кухня"
                                            },
                                            new ApartmentTypeEntry()
                                            {
                                                ApartmentType = "2К",
                                                LivingRoomCount = 2,
                                                ContainRooms = "Кухня",
                                                NonContainRooms = "Кухня-столовая"
                                            },
                                            new ApartmentTypeEntry()
                                            {
                                                ApartmentType = "2Е",
                                                LivingRoomCount = 2,
                                                ContainRooms = "Кухня-столовая",
                                                NonContainRooms = "Кухня"
                                            },
                                            new ApartmentTypeEntry()
                                            {
                                                ApartmentType = "3К",
                                                LivingRoomCount = 3,
                                                ContainRooms = "Кухня",
                                                NonContainRooms = "Кухня-столовая"
                                            },
                                            new ApartmentTypeEntry()
                                            {
                                                ApartmentType = "3Е",
                                                LivingRoomCount = 3,
                                                ContainRooms = "Кухня-столовая",
                                                NonContainRooms = "Кухня"
                                            },
                                            new ApartmentTypeEntry()
                                            {
                                                ApartmentType = "4К",
                                                LivingRoomCount = 4,
                                                ContainRooms = "Кухня",
                                                NonContainRooms = "Кухня-столовая"
                                            },
                                            new ApartmentTypeEntry()
                                            {
                                                ApartmentType = "4Е",
                                                LivingRoomCount = 4,
                                                ContainRooms = "Кухня-столовая",
                                                NonContainRooms = "Кухня"
                                            },
                                            new ApartmentTypeEntry()
                                            {
                                                ApartmentType = "5К",
                                                LivingRoomCount = 5,
                                                ContainRooms = "Кухня",
                                                NonContainRooms = "Кухня-столовая"
                                            },
                                            new ApartmentTypeEntry()
                                            {
                                                ApartmentType = "5Е",
                                                LivingRoomCount = 5,
                                                ContainRooms = "Кухня-столовая",
                                                NonContainRooms = "Кухня"
                                            },
                                        }
                            }
                        }
                    }
                };
            }
            var serializer = new XmlSerializer(typeof(ApartmentsProjectLayout));
            using (var reader = new StreamReader(_configFilePath))
            {
                return (ApartmentsProjectLayout)serializer.Deserialize(reader);
            }
        }
    }
}
