using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Kortros.Architecture.Apartments.Model;
using Kortros.Architecture.Apartments.Utilities;
using Kortros.Architecture.Apartments.View;
using Kortros.Architecture.Apartments.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using Application = Autodesk.Revit.ApplicationServices.Application;

namespace Kortros.Architecture.Apartments.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RefreshTables : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            UIApplication uiapp = commandData.Application;

            Autodesk.Revit.DB.View activeView = doc.ActiveView;
            Level level = activeView.GenLevel;
            ViewType viewType = doc.ActiveView.ViewType;

            List<Element> tables = new FilteredElementCollector(doc, activeView.Id)
                .OfCategory(BuiltInCategory.OST_GenericAnnotation)
                .WhereElementIsNotElementType()
                .Where (table => table.Name == "Шаблон таблицы")
                .ToList();


            if (viewType == ViewType.FloorPlan && tables.Count() > 0)
            {
                foreach (Element table in tables)
                {
                    using (Transaction t = new Transaction(doc, "setdata"))
                    {
                        t.Start();
                        GetTableInstance(doc, table, level);
                        t.Commit();
                    }
                }
                return Result.Succeeded;
            }
            else
            {
                MessageBox.Show("Необходимо открыть план этажа с таблицами");
                return Result.Cancelled;
            }
        }

        private TableInstance GetTableInstance(Document doc, Element table, Level lvl)
        {
            TableInstance tableInstance = new TableInstance()
            {
                Level = lvl.Name,
            };

            try
            {
                string variant = table.LookupParameter("Номер варианта").AsString();
                tableInstance.IsVersionsAvailable = true;
                tableInstance.Version = variant;
            }
            catch 
            {
                tableInstance.IsVersionsAvailable = false;
                tableInstance.Version = "";
            }
            tableInstance.Rooms = GetRoomsCommand(doc, tableInstance, lvl);


            // Заполнить сначала IS NNN Correct
            if (tableInstance.IsVersionsAvailable)
            {
                RevitFunc.CheckVns(doc, tableInstance, tableInstance.Version);
            }
            else
            {
                RevitFunc.CheckVns(doc, tableInstance);
            }

            if (tableInstance.IsVersionsAvailable)
            {
                RevitFunc.CheckGns1(doc, tableInstance, tableInstance.Version);
            }
            else
            {
                RevitFunc.CheckGns1(doc, tableInstance);
            }

            if (tableInstance.IsVersionsAvailable)
            {
                RevitFunc.CheckGns2(doc, tableInstance, tableInstance.Version);
            }
            else
            {
                RevitFunc.CheckGns2(doc, tableInstance);
            }


            TableInstanceVM.SetLivingArea(doc, tableInstance);
            TableInstanceVM.SetArea(doc, tableInstance);
            TableInstanceVM.SetAreasZone(doc, tableInstance);

            // Перезаписать значения

            if (tableInstance.AreaOfLivingSpace != 0)
                table.LookupParameter("S общ на этаже").Set(tableInstance.AreaOfLivingSpace);
            else
                table.LookupParameter("S общ на этаже").Set(11);

            if (tableInstance.AreaOfSpace != 0)
                table.LookupParameter("S общ всех помещений на этаже").Set(tableInstance.AreaOfSpace);
            else
                table.LookupParameter("S общ всех помещений на этаже").Set(11);

            if (tableInstance.IsAreaVNSCorrect)
                table.LookupParameter("S этажа ВНС").Set(tableInstance.AreaVNS);
            else
                table.LookupParameter("S этажа ВНС").Set(11);

            if (tableInstance.IsAreaGNS1Correct)
                table.LookupParameter("S этажа ГНС1").Set(tableInstance.AreaGNS1);
            else
                table.LookupParameter("S этажа ГНС1").Set(11);

            if (tableInstance.IsAreaGNS2Correct)
                table.LookupParameter("S этажа ГНС2").Set(tableInstance.AreaGNS2);
            else
                table.LookupParameter("S этажа ГНС2").Set(11);

            if (tableInstance.AreaSectionGNS1 == 0)
                table.LookupParameter("S всего корпуса ГНС1").Set(tableInstance.AreaSectionGNS1);
            else
                table.LookupParameter("S всего корпуса ГНС1").Set(11);

            if (tableInstance.AreaSectionGNS2 == 0)
                table.LookupParameter("S всего корпуса ГНС2").Set(tableInstance.AreaSectionGNS1);
            else
                table.LookupParameter("S всего корпуса ГНС2").Set(11);

            return tableInstance;
        }

        private List<Room> GetRoomsCommand(Document doc, TableInstance tableInstance, Level lvl)
        {
            List<Room> rooms = new List<Room>();
            if (tableInstance.IsVersionsAvailable)
            {
                foreach (Room room in RevitFunc.GetRoomsInLevel(doc, lvl))
                {
                    string comment = room.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString();
                    if (comment == tableInstance.Version)
                        rooms.Add(room);
                }
                return rooms;
            }
            else
            {
                return RevitFunc.GetRoomsInLevel(doc, lvl);
            }
        }

    }
}
