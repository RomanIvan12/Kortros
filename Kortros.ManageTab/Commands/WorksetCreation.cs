using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;

namespace Kortros.ManageTab.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class WorksetCreation : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            if (doc.IsWorkshared)
            {
                ICollection<Workset> existWorksets = new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset).ToWorksets();

                WorksetCreationWindow wsWindow = new WorksetCreationWindow(doc, existWorksets);
                wsWindow.ShowDialog();
            }
            else
            {
                TaskDialog.Show("Ошибка", "Данный файл не является файлом совместной работы", TaskDialogCommonButtons.Close);
            }
            return Result.Succeeded;
        }
    }
}
