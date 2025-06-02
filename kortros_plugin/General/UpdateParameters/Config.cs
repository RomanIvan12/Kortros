using Kortros.General.UpdateParameters.Properties;

namespace Kortros.General.UpdateParameters
{
    public class Config
    {
		public static bool SelectedKolichestvo
        {
			get 
			{ 
				return UpdateParameter.Default.UpdateParameterSelectedKolichestvo; 
			}
			set 
			{
                UpdateParameter.Default.UpdateParameterSelectedKolichestvo = value;
                UpdateParameter.Default.Save();
            }
		}

        public static bool SelectedIzmer
        {
            get
            {
                return UpdateParameter.Default.UpdateParameterSelectedIzmer;
            }
            set
            {
                UpdateParameter.Default.UpdateParameterSelectedIzmer = value;
                UpdateParameter.Default.Save();
            }
        }

        public static bool SelectedGroup
        {
            get
            {
                return UpdateParameter.Default.UpdateParameterSelectedGroup;
            }
            set
            {
                UpdateParameter.Default.UpdateParameterSelectedGroup = value;
                UpdateParameter.Default.Save();
            }
        }

        public static bool SelectedWorkset
        {
            get
            {
                return UpdateParameter.Default.UpdateParameterSelectedWorkset;
            }
            set
            {
                UpdateParameter.Default.UpdateParameterSelectedWorkset = value;
                UpdateParameter.Default.Save();
            }
        }

        public static bool SelectedElementId
        {
            get
            {
                return UpdateParameter.Default.UpdateParameterSelectedElementId;
            }
            set
            {
                UpdateParameter.Default.UpdateParameterSelectedElementId = value;
                UpdateParameter.Default.Save();
            }
        }

        public static bool UpdateOnlySelected
        {
            get
            {
                return UpdateParameter.Default.UpdateParameterUpdateOnlySelected;
            }
            set
            {
                UpdateParameter.Default.UpdateParameterUpdateOnlySelected = value;
                UpdateParameter.Default.Save();
            }
        }
    }
}
