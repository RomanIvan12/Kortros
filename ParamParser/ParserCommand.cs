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
using System.Text;
using Application = Autodesk.Revit.ApplicationServices.Application;
using Microsoft.Win32;
using Autodesk.Revit.DB.Electrical;
using ParamParser.WPF;
using ParamParser.Log4Net;
using log4net;
using System.Windows.Media;

namespace ParamParser
{
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class ParserCommand : IExternalCommand
    {

        public static ILog _logger;
        public static ILog _loggerShow;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                _logger = Logger.InitLog();
                _logger.Info($"{nameof(ParserCommand)}: initializing");
                _loggerShow = Logger.InitLog2();
                _loggerShow.Info($"{nameof(ParserCommand)}: initializing");
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _loggerShow.Error(ex);
                throw;
            }

            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;



            TestWindow window = new TestWindow(doc, uidoc);
            window.ShowDialog();



            return Result.Succeeded;
        }
    }
}