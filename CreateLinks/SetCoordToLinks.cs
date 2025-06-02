using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Application = Autodesk.Revit.ApplicationServices.Application;

namespace CreateLinks
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SetCoordToLinks : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Application app = commandData.Application.Application;
            Document doc = commandData.Application.ActiveUIDocument.Document;

            var linkType = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_RvtLinks).WhereElementIsElementType().ToElements().First();

            RevitLinkType revitLinkType = linkType as RevitLinkType;

            bool hasSaveablePos = revitLinkType.HasSaveablePositions();
            //var zzz = revitLinkType.SavePositions();
            ProjectLocationSet locationSet = doc.ProjectLocations;


            var linkInst = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_RvtLinks).WhereElementIsNotElementType().ToElements().First();
            RevitLinkInstance revitLinkInstance = linkInst as RevitLinkInstance;

            var iii = revitLinkInstance.Document.ActiveProjectLocation.Id;

            using (Transaction t = new Transaction(doc, "dde"))
            {
                t.Start();
                doc.PublishCoordinates(new LinkElementId(linkInst.Id, iii));

                t.Commit();
            }
            //MessageBox.Show(iii.ToString());
            
            return Result.Succeeded;
        }
    }

    public class InstCallback : ISaveSharedCoordinatesCallback
    {
        public SaveModifiedLinksOptions GetSaveModifiedLinksOption(RevitLinkType link)
        {
            return SaveModifiedLinksOptions.SaveLinks;
        }
    }
}
