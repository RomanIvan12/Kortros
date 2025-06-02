using Kortros.General.UpdateParameters.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Kortros.General.UpdateParameters
{
    public class UpdateParametersViewModel : INotifyPropertyChanged
    {
        private Handler handler;


        #region Параметры значения чекбоксов в UI
        public bool SelectedKolichestvo
        {
            get { return Config.SelectedKolichestvo; }
            set 
            {
                Config.SelectedKolichestvo = value;
                OnPropertyChanged();
            }
        }

        public bool SelectedIzmer
        {
            get { return Config.SelectedIzmer; }
            set
            {
                Config.SelectedIzmer = value;
                OnPropertyChanged();
            }
        }

        public bool SelectedGroup
        {
            get { return Config.SelectedGroup; }
            set
            {
                Config.SelectedGroup = value;
                OnPropertyChanged();
            }
        }

        public bool SelectedWorkset
        {
            get { return Config.SelectedWorkset; }
            set
            {
                Config.SelectedWorkset = value;
                OnPropertyChanged();
            }
        }

        public bool SelectedElementId
        {
            get { return Config.SelectedElementId; }
            set
            {
                Config.SelectedElementId = value;
                OnPropertyChanged();
            }
        }

        public bool UpdateOnlySelected
        {
            get { return Config.UpdateOnlySelected; }
            set
            {
                Config.UpdateOnlySelected = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;


        public UpdateParametersViewModel(Handler handler)
        {
            this.handler = handler;
        }

        public void Run()
        {
            handler.Run(UpdateOnlySelected);
        }

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
