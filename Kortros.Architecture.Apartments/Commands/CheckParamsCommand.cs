using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Kortros.Architecture.Apartments.Extensions;
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
    public class CheckParamsCommand : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            Application app = commandData.Application.Application;
            //ADSK_Наименование и номер цвета

            List<Category> xxx = new List<Category>()
            {
                Category.GetCategory(doc, BuiltInCategory.OST_Rooms),
            };
            //Category zxc = Category.GetCategory(doc, BuiltInCategory.OST_Rooms);

            using (Transaction t = new Transaction(doc, "de"))
            {
                t.Start();

                CheckCommonParameters.AddSharedParameters(app, doc, xxx, "ADSK_Наименование и номер цвета", true);
                t.Commit();
            }

            CheckCommonParameters.CheckExistedCommonParameters(doc);



            MessageBox.Show("CMD1");
            return Result.Succeeded;
        }
    }
}
