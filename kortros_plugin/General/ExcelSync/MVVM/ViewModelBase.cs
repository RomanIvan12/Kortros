using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Kortros.General.MVVM
{
    public class ViewModelBase : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;
        public static void OnStaticPropertyChanged([CallerMemberName] string prop = "")
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(prop));
        }
    }
}
