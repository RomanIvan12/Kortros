using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using System.IO;
using System.Reflection;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;
using Path = System.IO.Path;
using System.Text;
using Application = Autodesk.Revit.ApplicationServices.Application;
using Microsoft.Win32;
using Autodesk.Revit.DB.Electrical;

namespace Kortros.Commands.ModelData
{
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    internal class DataExport : IExternalCommand
    {


        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            Application app = doc.Application;

            System.Windows.Forms.Application.EnableVisualStyles();


            string versionNumber = app.VersionNumber;
            string RSNPath = Environment.GetEnvironmentVariable("programdata") + "\\Autodesk\\Revit Server " + versionNumber + "\\Config\\RSN.ini";
            string[] RevitServer = File.ReadAllLines(RSNPath);
            // cписок айпишников
            //MessageBox.Show(RevitServer[0]);


            RSNfilesWindow window = new RSNfilesWindow(doc, RevitServer);

            //window.FilesPathSelected += CreateLinksInstances;
            //window.FilesPathSelected += Test;
            window.ShowDialog();
            return Result.Succeeded;
        }
    }
}
