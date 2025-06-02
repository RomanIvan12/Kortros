using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Kortros.Architecture.Apartments.Utilities;
using Kortros.Architecture.Apartments.View;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Application = Autodesk.Revit.ApplicationServices.Application;

namespace Kortros.Architecture.Apartments.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RunPlugin : IExternalCommand
    {
        public static Level Level;
        public static Document Document;
        public static UIApplication Application;
        public static TableInstanceWindow TableInstanceWindow;

        private static readonly ILog _logger = LogManager.GetLogger("ZoneCalculation");

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            Application app = commandData.Application.Application;
            var activeView = doc.ActiveView;
            ViewType viewType = doc.ActiveView.ViewType;

            List<Room> roomsInLevel = RevitFunc.GetRoomsInLevel(doc, activeView.GenLevel);

            if (viewType == ViewType.FloorPlan && roomsInLevel.Count() > 0)
            {
                try { _logger.Info("Plugin opened"); } catch { _logger.Error("Plugin opened Error"); }

                Document = doc;
                Level = activeView.GenLevel;
                Application = commandData.Application;

                TableInstanceWindow window = new TableInstanceWindow();
                TableInstanceWindow = window;
                window.ShowDialog();
                return Result.Succeeded;
            }
            else if (roomsInLevel.Count() == 0)
            {
                try { _logger.Info("На данном уровне нет помещений"); } catch { _logger.Error("Plugin opened Error"); }
                MessageBox.Show("На данном уровне нет помещений");
                return Result.Cancelled;
            }
            else
            {
                try { _logger.Info("Необходимо открыть план этажа"); } catch { _logger.Error("Plugin opened Error"); }
                MessageBox.Show("Необходимо открыть план этажа");
                return Result.Cancelled;
            }
        }
    }
}
