using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApartmentsProject.ViewModel;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace ApartmentsProject.Models
{
    public class ApartmentModel
    {
        public List<Room> RoomsOfApartment { get; set; }
        public int AptId { get; set; }
        public string Level { get; set; }
        public string Function { get; set; } // По умолчанию - "Квартира". Параметр для возможной обработки МО
        public int NumberOfRooms { get; set; }
        public int NumberOfLivingRooms { get; set; }
        public string TypeName { get; set; } // Тип квартиры, типа 1С, 1Е, 1К, 2К, 2Е, 3К и т.д
        public double LivingRoomsSumArea { get; set; } // Жилая площадь
        public double RoomsSumArea { get; set; } // Площадь квартиры
        public double AllRoomsSumArea { get; set; } // Площадь общая
        public double AllRoomsSumAreaNoKof { get; set; } // Площадь общая без коэффициента
        public double AllNonHeatRoomsSumArea { get; set; } // Площадь неотапл. помещений с коэфф.

        public string ErrorMsg { get; set; }

        private List<RoomMatrixEntry> _roomMatrixEntries = RunCommand.ApartmentsProjectLayout.ConfigurationList.First(
                i => i.IsSelected == true).RoomMatrix.Entries.ToList();
        private List<ApartmentTypeEntry> _apartmentTypeEntries = RunCommand.ApartmentsProjectLayout.ConfigurationList.First(
                i => i.IsSelected == true).ApartmentType.Entries.ToList();

        public ApartmentModel(List<Room> roomsOfApartment, int id)
        {
            RoomsOfApartment = roomsOfApartment;
            AptId = id;
            Function = "Квартира";
            NumberOfRooms = RoomsOfApartment.Count;

            SetAptId();
            SetType();
            SetRatio();
            SetFinishing();
            SetNumberOfRoom();
            SetLevel();
        }

        /// <summary>
        /// Записывает в KRT_ID Квартиры или его значение ID
        /// </summary>
        private void SetAptId()
        {
            Guid idGuid = ApartmentParameterMappingVm.MappingModel
                .First(item => item.DataOrigin.Name == "KRT_ID Квартиры")
                .ParameterToMatch.Guid;
            foreach (Room room in RoomsOfApartment)
            {
                room.get_Parameter(idGuid).Set(AptId);
            }
        }

        private void SetLevel()
        {
            var listOfLevel = RoomsOfApartment.Select(room => room.Level.Name).Distinct();
            if (listOfLevel.Count() == 1)
                Level = listOfLevel.FirstOrDefault();
            else if (listOfLevel.Count() > 1)
            {
                var sb = new StringBuilder();
                foreach (var lvl in listOfLevel)
                {
                    sb.Append($"{lvl} ");
                }
                Level = sb.ToString();
            }
        }

        /// <summary>
        /// Записывает в KRT_Тип помещения - жилое/нежилое
        /// </summary>
        private void SetType()
        {
            Guid typeGuid = ApartmentParameterMappingVm.MappingModel
                .First(item => item.DataOrigin.Name == "KRT_Тип помещения")
                .ParameterToMatch.Guid;

            int numberOfLivingRooms = 0;

            foreach (Room room in RoomsOfApartment)
            {
                string typeName = _roomMatrixEntries
                    .Where(item => item.Name ==
                                   room.get_Parameter(BuiltInParameter.ROOM_NAME).AsString())
                    .Select(i => i.RoomType).First();
                room.get_Parameter(typeGuid).Set(typeName);
                if (typeName == "Living")
                    numberOfLivingRooms++;
            }

            this.NumberOfLivingRooms = numberOfLivingRooms;

            Guid numGuid = ApartmentParameterMappingVm.MappingModel
                .First(item => item.DataOrigin.Name == "KRT_Число жилых комнат")
                .ParameterToMatch.Guid;
            foreach (Room room in RoomsOfApartment)
            {
                room.get_Parameter(numGuid).Set(numberOfLivingRooms);
            }

            // Сюда впихиваю запись типа 2К, 2Е и т.п.
            Guid typeNameGuid = ApartmentParameterMappingVm.MappingModel
                .First(item => item.DataOrigin.Name == "KRT_Тип Квартиры")
                .ParameterToMatch.Guid;

            var apartmentTypeEntries = _apartmentTypeEntries
                .Where(item => item.LivingRoomCount == numberOfLivingRooms).ToList(); // отфильтрованные варианты по числу жилых комнат

            foreach (var model in apartmentTypeEntries)
            {
                var existing = model.ContainRooms;
                var nonExisting = model.NonContainRooms;

                var roomNames = RoomsOfApartment.Select(room => room.get_Parameter(BuiltInParameter.ROOM_NAME).AsString())
                    .ToList(); // Все имена помещений
                if (ValidateRooms(roomNames, ParseCellToList(existing), ParseCellToList(nonExisting)))
                {
                    this.TypeName = model.ApartmentType;
                    foreach (var room in RoomsOfApartment)
                    {
                        room.get_Parameter(typeNameGuid).Set(model.ApartmentType);
                    }
                }
            }
        }

        private bool ValidateRooms(List<string> roomNames, List<string> existingRooms, List<string> nonExistingRooms)
        {
            bool allExistingRoomsPresent = existingRooms.All(room => roomNames.Contains(room));
            bool noNonExistingRoomsPresent = nonExistingRooms.All(room => !roomNames.Contains(room));
            return allExistingRoomsPresent && noNonExistingRoomsPresent;
        }


        // Функция, которая в строке с перечеслением через запятую получает List<string> этих значений
        private static List<string> ParseCellToList(string cellValue)
        {
            if (string.IsNullOrWhiteSpace(cellValue))
                return new List<string>();
            return cellValue.Split(',').Select(item => item.Trim())
                .Where(item => !string.IsNullOrEmpty(item)).ToList();
        }

        /// <summary>
        /// Записывает в KRT_Коэффициент площади - понижающий для балконов/лоджий - берётся из таблички
        /// </summary>
        private void SetRatio()
        {
            Guid ratioGuid = ApartmentParameterMappingVm.MappingModel
                .First(item => item.DataOrigin.Name == "KRT_Коэффициент площади")
                .ParameterToMatch.Guid;
            foreach (Room room in RoomsOfApartment)
            {
                double ratio = _roomMatrixEntries
                    .Where(item => item.Name ==
                                   room.get_Parameter(BuiltInParameter.ROOM_NAME).AsString())
                    .Select(i => i.AreaFactor).First();
                room.get_Parameter(ratioGuid).Set(ratio);
            }
        }
        /// <summary>
        /// Записывает в KRT_Толщина черновой отделки значение из таблички
        /// </summary>
        private void SetFinishing()
        {
            Guid finishingGuid = ApartmentParameterMappingVm.MappingModel
                .First(item => item.DataOrigin.Name == "KRT_Толщина черновой отделки")
                .ParameterToMatch.Guid;
            foreach (Room room in RoomsOfApartment)
            {
                int finishing = _roomMatrixEntries
                    .Where(item => item.Name ==
                                   room.get_Parameter(BuiltInParameter.ROOM_NAME).AsString())
                    .Select(i => i.FinishingThickness).First();
                room.get_Parameter(finishingGuid).Set(finishing);
            }
        }

        /// <summary>
        /// Записывает в помещения порядковый номер. Сначала нумерация по приоритету, затем по площади
        /// </summary>
        private void SetNumberOfRoom()
        {
            Guid roomNumberGuid = ApartmentParameterMappingVm.MappingModel
                .First(item => item.DataOrigin.Name == "KRT_Номер помещения")
                .ParameterToMatch.Guid;

            var sortModel = _roomMatrixEntries
                .OrderBy(i => i.NumberPriority)
                .ThenBy(i => i.Name).ToList();


            int counter = 1;
            foreach (var model in sortModel)
            {
                var matchingRooms = RoomsOfApartment
                    .Where(r => r.get_Parameter(BuiltInParameter.ROOM_NAME).AsString() == model.Name)
                    .OrderByDescending(r => r.Area).ToList();
                foreach (var room in matchingRooms)
                {
                    room.get_Parameter(roomNumberGuid).Set(counter);
                    counter++;
                }
            }
        }
    }

    public class ApartmentRepository
    {
        private static readonly ApartmentRepository _instance = new ApartmentRepository();

        private ApartmentRepository() { }

        public static ApartmentRepository Instance => _instance;
        public ObservableCollection<ApartmentModel> Apartments { get; } = new ObservableCollection<ApartmentModel>();

        public void AddApartment(ApartmentModel apartment)
        {
            Apartments.Add(apartment);
        }

        public void Initialize(IEnumerable<List<Room>> allRoomsList)
        {
            Apartments.Clear();
            int counter = 1;
            foreach (List<Room> roomList in allRoomsList)
            {
                var apartment = new ApartmentModel(roomList, counter);
                AddApartment(apartment);
                counter++;
            }
        }
    }
}
