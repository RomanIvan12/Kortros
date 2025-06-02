using ApartmentsProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApartmentsProject.AuxiliaryСlasses
{
    public class Mediator
    {
        public static event Action<Configuration> SelectedConfigurationChanged;
        public static void NotifySelectedConfigurationChanged(Configuration newConfiguration)
        {
            SelectedConfigurationChanged?.Invoke(newConfiguration);
        }
    }
}
