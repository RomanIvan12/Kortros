using RevitServerExporter.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RevitServerExporter.CoreFunc
{
    public class OpenRevit
    {
        private string _revitVersion;
        private string _revitExecutablePath;

        private Process _revitProcess;
        private readonly ManualResetEvent _revitProcessInitialized = new ManualResetEvent(false);

        public OpenRevit(string revitVersion)
        {
            _revitVersion = revitVersion;
            Initialize();
        }

        public void Initialize()
        {
            var versionInfo = RevitIntegration.GetVersionInfo(_revitVersion);
            _revitExecutablePath = versionInfo.ExecutablePath;
            RevitIntegration.InitRevitAddinManifest(versionInfo.Version);
            InitRevitConnection();
        }

        private void InitRevitConnection()
        {
            //TODO if stat

            _revitProcessInitialized.Reset();
            _revitProcess = new Process
            {
                StartInfo = { FileName = _revitExecutablePath }
            };
            _revitProcess.Start();
        }
    }
}
