using ApartmentsProject.AuxiliaryСlasses;
using ApartmentsProject.Models;
using ApartmentsProject.ViewModel.Commands;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace ApartmentsProject.ViewModel
{
    public partial class SourceDataVm
    {
        private ObservableCollection<ApartmentTypeEntry> _apartmentTypeEntries;
        public ObservableCollection<ApartmentTypeEntry> ApartmentTypeEntries
        {
            get => _apartmentTypeEntries;
            set
            {
                if (_apartmentTypeEntries != value)
                {
                    if (_apartmentTypeEntries != null)
                        foreach (var entry in _apartmentTypeEntries)
                            entry.ApartmentTypeEntryChanged -= OnApartmentTypeEntryChanged;

                    _apartmentTypeEntries = value;

                    if (_apartmentTypeEntries != value)
                        foreach (var entry in _apartmentTypeEntries)
                            entry.ApartmentTypeEntryChanged += OnApartmentTypeEntryChanged;

                    OnPropertyChanged(nameof(ApartmentTypeEntries));
                    ConfigurationService.Instance.SaveConfiguration(RunCommand.ApartmentsProjectLayout);
                }
            }
        }

        private ApartmentTypeEntry _selectedApartmentTypeEntry;
        public ApartmentTypeEntry SelectedApartmentTypeEntry
        {
            get => _selectedApartmentTypeEntry;
            set
            {
                if (_selectedApartmentTypeEntry != value)
                {
                    _selectedApartmentTypeEntry = value;
                    OnPropertyChanged(nameof(SelectedApartmentTypeEntry));
                }
            }
        }

        private void OnApartmentTypeEntriesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (ApartmentTypeEntry newItem in e.NewItems)
                    newItem.ApartmentTypeEntryChanged += OnApartmentTypeEntryChanged;
            }
            if (e.OldItems != null)
            {
                foreach (ApartmentTypeEntry oldItem in e.OldItems)
                    oldItem.ApartmentTypeEntryChanged -= OnApartmentTypeEntryChanged;
            }
        }
        private void GetApartmentTypeEntries()
        {
            ApartmentTypeEntries.Clear();
            if (_selectedConfiguration.ApartmentType.Entries != null)
            {
                foreach (var singleEntry in _selectedConfiguration.ApartmentType.Entries)
                    ApartmentTypeEntries.Add(singleEntry);
            }
        }

        public AddApartmentTypeEntry AddApartmentTypeEntry { get; set; }
        public DeleteApartmentTypeEntry DeleteApartmentTypeEntry { get; set; }
        public void AddApartmentType()
        {
            var newApartmentTypeEntry = new ApartmentTypeEntry()
            {
                ApartmentType = "New Type",
                LivingRoomCount = 0,
                ContainRooms = "Кухня",
                NonContainRooms = "Кухня-столовая"
            };

            newApartmentTypeEntry.ApartmentTypeEntryChanged += OnApartmentTypeEntryChanged;

            ApartmentTypeEntries.Add(newApartmentTypeEntry);

            RunCommand.ApartmentsProjectLayout.ConfigurationList.First(
                i => i.IsSelected == true)
                .ApartmentType.Entries = ApartmentTypeEntries.ToList();
            ConfigurationService.Instance.SaveConfiguration(RunCommand.ApartmentsProjectLayout);
        }

        public void DeleteApartmentType()
        {
            if (_selectedApartmentTypeEntry != null)
            {
                ApartmentTypeEntries.Remove(SelectedApartmentTypeEntry);
                RunCommand.ApartmentsProjectLayout.ConfigurationList.First(
                    i => i.IsSelected == true)
                    .ApartmentType.Entries = ApartmentTypeEntries.ToList();
                ConfigurationService.Instance.SaveConfiguration(RunCommand.ApartmentsProjectLayout);
            }
        }

        private void OnApartmentTypeEntryChanged()
        {
            // Сохраняем конфигурацию при изменении элемента
            ConfigurationService.Instance.SaveConfiguration(RunCommand.ApartmentsProjectLayout);
        }
    }
}
