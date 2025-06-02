using Kortros.General.ExcelSync.Properties;

namespace Kortros.General.ExcelSync
{
    public class Config
    {
        public static string FilePath
        {
            get
            {
                return Settings.Default.ExcelSyncFilePath;
            }
            set
            {
                Settings.Default.ExcelSyncFilePath = value;
                Settings.Default.Save();
            }
        }

        public static string MarkParName
        {
            get
            {
                return Settings.Default.ExcelSyncMarkParName;
            }
            set
            {
                Settings.Default.ExcelSyncMarkParName = value;
                Settings.Default.Save();
            }
        }

        public static string TypeMarkParName
        {
            get
            {
                return Settings.Default.ExcelSyncTypeMarkParName;
            }
            set
            {
                Settings.Default.ExcelSyncTypeMarkParName = value;
                Settings.Default.Save();
            }
        }

        public static bool UpdateOnlySelected
        {
            get
            {
                return Settings.Default.ExcelSyncUpdateOnlySelected;
            }
            set
            {
                Settings.Default.ExcelSyncUpdateOnlySelected = value;
                Settings.Default.Save();
            }
        }
    }
}
