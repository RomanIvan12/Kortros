using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Architecture;

namespace ApartmentsProject.Models
{
    public interface IApartmentService
    {
        ObservableCollection<ApartmentModel> GetAllApartments();
        void InitializeApartments(List<List<Room>> roomList);
    }

    public class ApartmentService : IApartmentService
    {
        private ObservableCollection<ApartmentModel> _apartmentModels = new ObservableCollection<ApartmentModel>();
        private int _id;
        public ObservableCollection<ApartmentModel> GetAllApartments()
        {
            return _apartmentModels;
        }

        public void InitializeApartments(List<List<Room>> roomList)
        {
            _apartmentModels.Clear();
            foreach (var room in roomList)
            {
                _apartmentModels.Add(new ApartmentModel(room, _id));
            }
        }
    }
}
