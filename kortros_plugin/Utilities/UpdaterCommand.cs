using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Kortros.Updaters;
using System;

namespace Kortros.Utilities
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class UpdaterCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            return Execute(commandData.Application);
        }

        public Result Execute(UIApplication uiapp)
        {
            Application app = uiapp.Application;
            try
            {
                uiapp.ViewActivated += new EventHandler<ViewActivatedEventArgs>(ViewActivated);
            }
            catch (Exception ex)
            {
                Logger.Log.Info($"Error ViewActivated Event: {ex.Message}");
            }
            return Result.Succeeded;
        }

        public static void ViewActivated(object sender, ViewActivatedEventArgs args)
        {
            Document doc = args.Document;
            AddInId id = args.Document.Application.ActiveAddInId;

            if (!doc.IsFamilyDocument)
            {
                _ = new DuctFittinsInsulationUpdater(id, doc);

                _ = new DuctInsulationsUpdater(id);
                _ = new DuctPipeAccessoryUpdater(id);
                _ = new ElementIdUpdater(id);
                _ = new PipeDuctFittingsUpdater(id);
                _ = new PipeInsulationUpdater(id);

                if (doc.Title.Contains("_EOM") || doc.Title.Contains("_ЕОМ"))
                {
                    GroupEOMUpdater groupEOMUpdater = new GroupEOMUpdater(id);
                }
                if (doc.Title.Contains("_OV") || doc.Title.Contains("_ОВ"))
                {
                    GroupMEPUpdater groupMEPUpdater = new GroupMEPUpdater(id);
                    DuctUpdater ductUpdater = new DuctUpdater(id);
                    EquipmentUpdater equipmentUpdater = new EquipmentUpdater(id);
                    PipeUpdater pipeUpdater = new PipeUpdater(id);
                    SprinklerDuctTerminalUpdater sprinklerDuctTerminalUpdater = new SprinklerDuctTerminalUpdater(id);
                }
                if (doc.Title.Contains("_VK") || doc.Title.Contains("_ВК"))
                {
                    GroupMEPUpdater groupMEPUpdater = new GroupMEPUpdater(id);
                    EquipmentUpdater equipmentUpdater = new EquipmentUpdater(id);
                    PipeUpdater pipeUpdater = new PipeUpdater(id);
                    SprinklerDuctTerminalUpdater sprinklerDuctTerminalUpdater = new SprinklerDuctTerminalUpdater(id);
                }
                if (doc.Title.Contains("_AR") || doc.Title.Contains("_АР"))
                {
                    WallsUpdater wallsUpdater = new WallsUpdater(id);
                    FloorsUpdater floorsUpdater = new FloorsUpdater(id);
                    DoorsUpdater doorsUpdater = new DoorsUpdater(id);
                    RoofsUpdater roofsUpdater = new RoofsUpdater(id);
                    CeilingUpdater ceilingUpdater = new CeilingUpdater(id);
                    FamilyInstanceClassUpdater familyInstanceClassUpdater = new FamilyInstanceClassUpdater(id);
                    StairsRailingUpdater stairsRailing = new StairsRailingUpdater(id);
                }
            }
        }
    }
}
