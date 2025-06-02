using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitServerExporter.CoreFunc
{
    class RevitIntegration
    {
        public const string REVIT_APP_INITIALIZED_OK = "ok";
        public const string COMMON_SETTINGS_NAME = "CommonSettings";

        private static readonly string RevitRegistryFolder = @"SOFTWARE\Autodesk\Revit";
        private static readonly string ProductNameValueName = @"ProductName";
        private static readonly string ProductName = @"Autodesk Revit";
        private static readonly string InstallationLocationValueName = @"InstallationLocation";
        private static readonly string VersionValueName = @"Version";
        private static readonly string RevitExecutableName = @"Revit.exe";

        private static readonly string AddinManifestFormat =
            @"<?xml version = ""1.0"" encoding = ""utf-8"" ?>
<RevitAddIns>
    <AddIn Type = ""Application"">
        <Name>Revit To Ifc Converter</Name>
        <Assembly>RevitServerExporter.Core.dll</Assembly>
        <FullClassName>RevitServerExporter.Core.ExternalApplication</FullClassName>
        <AddInId>1B06E09D-5069-4B89-96DF-460EA78CB887</AddInId>
        <VendorId>KORTROS</VendorId>
        <VendorDescription>KORTROS2</VendorDescription>
    </AddIn>
</RevitAddIns>";


        #region
        public static VersionInfo GetVersionInfo(string targetVersion)
        {
            VersionInfo versionInfo;

            using (var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(RevitRegistryFolder, false))
            {
                versionInfo = GetVersionInfo(key, targetVersion);
            }

            if (string.IsNullOrEmpty(versionInfo.ExecutablePath))
                throw new Exception("Revit installation has not been found.");

            if (versionInfo.Version == 0)
                throw new Exception("Unknown Revit version.");

            return versionInfo;
        }

        private static VersionInfo GetVersionInfo(RegistryKey registryKey, string targetVersion)
        {
            var path = string.Empty;
            uint outVersion = 0;
            var subKeyNames = registryKey.GetSubKeyNames();
            foreach (var subKeyName in subKeyNames)
            {
                using (var subKey = registryKey.OpenSubKey(subKeyName))
                {
                    if (subKey == null)
                        continue;

                    string productName = subKey.GetValue(ProductNameValueName) as string;
                    var installationPath = subKey.GetValue(InstallationLocationValueName) as string;
                    if (productName != null && productName.StartsWith(ProductName) && installationPath != null)
                    {
                        var version = TryGetVersion(subKey);
                        if (version != ParseVersion(targetVersion))
                            continue;

                        outVersion = version;
                        path = Path.Combine(installationPath, RevitExecutableName);
                    }

                    var pathFromSubKey = GetVersionInfo(subKey, targetVersion);
                    if (!string.IsNullOrEmpty(pathFromSubKey.ExecutablePath))
                    {
                        path = pathFromSubKey.ExecutablePath;
                        outVersion = pathFromSubKey.Version;
                    }
                }
            }
            return new VersionInfo(path, outVersion);
        }
        private static uint TryGetVersion(RegistryKey key)
        {
            var versionStr = key.GetValue(VersionValueName) as string;
            if (string.IsNullOrEmpty(versionStr))
                return 0;

            var versionDigitsStr = versionStr.Split(' ').FirstOrDefault();
            return uint.TryParse(versionDigitsStr, out var version) ? version : 0;
        }

        private static uint ParseVersion(string version)
        {
            if (uint.TryParse(version, out var parsedVersion))
                return parsedVersion;
            return 0;
        }
        #endregion

        private static string GetAddinsFolder(uint revitVersion)
        {
            string programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            return Path.Combine(programData, "Autodesk", "Revit", "Addins", revitVersion.ToString());
        }

        public static void InitRevitAddinManifest(uint revitVersion)
        {
            string revitAddinFolder = GetAddinsFolder(revitVersion);
            string addinFilePath = Path.Combine(revitAddinFolder, "RevitServerExporter.addin");
            var assemblyPath = Assembly.GetExecutingAssembly().Location;
            var addinManifest = string.Format(AddinManifestFormat, assemblyPath);

            File.WriteAllText(addinFilePath, addinManifest);
        }
    }

    public class VersionInfo
    {
        public string ExecutablePath { get; }
        public uint Version { get; }

        public VersionInfo(string executablePath, uint version)
        {
            ExecutablePath = executablePath;
            Version = version;
        }
    }
}
