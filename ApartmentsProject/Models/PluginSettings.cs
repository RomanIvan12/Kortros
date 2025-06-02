using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace ApartmentsProject.Models
{
    public class PluginSettings : INotifyPropertyChanged
    {

        private static readonly Lazy<PluginSettings> _instance = new Lazy<PluginSettings>(() => new PluginSettings());

        private PluginSettings() { }
        public static PluginSettings Instance => _instance.Value;


        // Все ли Общие параметры созданы или замэппены
        private bool _areCommonParametersMapped;
        public bool AreCommonParametersMapped
        {
            get => _areCommonParametersMapped;
            set
            {
                _areCommonParametersMapped = value;
                OnPropertyChanged(nameof(AreCommonParametersMapped));
            }
        }



        // Видимость вкладки Сбор квартир
        private bool _apartmentCreationEnabled;
        public bool ApartmentCreationEnabled 
        {
            get => _apartmentCreationEnabled;
            set
            {
                _apartmentCreationEnabled = value;
                OnPropertyChanged(nameof(ApartmentCreationEnabled));
            }
        }


        // Показывает, собраны ли квартиры
        private bool _areApartmentCreated;
        public bool AreApartmentCreated
        {
            get => _areApartmentCreated;
            set
            {
                _areApartmentCreated = value;
                OnPropertyChanged(nameof(AreApartmentCreated));
            }
        }


        private Document revitDoc;
        public Document RevitDoc
        {
            get => revitDoc;
            set => revitDoc = value;
        }


        /// Критерии:
        /// 1-й критерий - Таблица общих параметров смэпплена (если FALSE - то расчёт должен быть заблокирован)
        ///     Вкладка "Источники" всегда включена
        /// Матрица помещений должна быть заполнена. Внутри проверка:
        ///     - Если отсутствуют имена помещений, которые есть в проекте, то по кнопке нужно добавить
        ///     2-й критерий. (если FALSE, то вкладка  "Исходная проверка" заблокирована)
        /// Матрица типов - неважно. Но если какой-то тип не найдётся во время расчета, то это надо выделить цветом
        /// 


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
