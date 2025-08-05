using System;
using System.ComponentModel;
using System.Linq;
using Autodesk.Revit.DB;

namespace ApartmentsProject.Models
{
    public class PluginSettings : INotifyPropertyChanged
    {

        private static readonly Lazy<PluginSettings> _instance = new Lazy<PluginSettings>(() => new PluginSettings());

        private PluginSettings() { }
        public static PluginSettings Instance => _instance.Value;

        private Document revitDoc;
        public Document RevitDoc
        {
            get => revitDoc;
            set => revitDoc = value;
        }

        private FilteredElementCollector _roomCollector;

        public FilteredElementCollector RoomCollector
        {
            get => _roomCollector;
            set => _roomCollector = value;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
