using Autodesk.Revit.DB;
using log4net;
using System;
using System.Collections.Generic;
using Kortros.Utilities;

namespace Kortros.Updaters
{
    internal class GroupMEPUpdater : IUpdater
    {
        static UpdaterId updaterId;

        public GroupMEPUpdater(AddInId id)
        {
            updaterId = new UpdaterId(id, new Guid("888A2A08-0501-4BC0-BA19-445A140CCD66"));
            RegisterUpdater();
            RegisterTriggers();
        }

        public void RegisterUpdater()
        {
            if (UpdaterRegistry.IsUpdaterRegistered(updaterId))
            {
                UpdaterRegistry.IsUpdaterRegistered(updaterId);
                UpdaterRegistry.UnregisterUpdater(updaterId);
            }

            UpdaterRegistry.RegisterUpdater(this, true);
        }

        public void RegisterTriggers()
        {
            ElementMulticategoryFilter groupMEPFilter = new ElementMulticategoryFilter(new List<BuiltInCategory>()
            {
                BuiltInCategory.OST_DuctAccessory,
                BuiltInCategory.OST_DuctSystem,
                BuiltInCategory.OST_DuctCurves,
                BuiltInCategory.OST_DuctLinings,
                BuiltInCategory.OST_DuctInsulations,
                BuiltInCategory.OST_DuctTerminal,
                BuiltInCategory.OST_FlexDuctCurves,
                BuiltInCategory.OST_DuctFitting,

                BuiltInCategory.OST_PipeAccessory,
                BuiltInCategory.OST_PipingSystem,
                BuiltInCategory.OST_PipeCurves,
                BuiltInCategory.OST_PipeInsulations,
                BuiltInCategory.OST_FlexPipeCurves,
                BuiltInCategory.OST_PipeFitting,
                BuiltInCategory.OST_PlumbingFixtures,
                BuiltInCategory.OST_Sprinklers,

                BuiltInCategory.OST_GenericModel,
                BuiltInCategory.OST_MechanicalEquipment,
                BuiltInCategory.OST_ElectricalEquipment,
            });

            if (updaterId != null && UpdaterRegistry.IsUpdaterRegistered(updaterId))
            {
                UpdaterRegistry.RemoveAllTriggers(updaterId);

                UpdaterRegistry.AddTrigger(updaterId,
                    groupMEPFilter,
                    Element.GetChangeTypeElementAddition());
            }
        }

        public void Execute(UpdaterData data)
        {
            Document doc = data.GetDocument();
            Guid KRTRS_Group = new Guid("663814f3-e83a-447f-a1d5-866c8c082409");

            IList<Element> parElems = new FilteredElementCollector(doc).OfClass(typeof(ParameterElement)).ToElements();
            List<string> existedParams = new List<string>();

            foreach (Element parElem in parElems)
            {
                if (parElem is SharedParameterElement sharedParameter)
                {
                    existedParams.Add(sharedParameter.Name);
                }
            }
            foreach (ElementId addedElements in data.GetAddedElementIds())
            {
                Element element = doc.GetElement(addedElements);

                if (element.GroupId.IntegerValue == -1)
                {
                    try
                    {
                        if (existedParams.Contains("KRTRS_Группирование"))
                        {
                            if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_MechanicalEquipment
                            || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_GenericModel
                            || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Sprinklers
                            || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_ElectricalEquipment
                            || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PlumbingFixtures
                            || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_FireAlarmDevices
                            || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_SecurityDevices
                            || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_NurseCallDevices)
                            {
                                element.get_Parameter(KRTRS_Group).Set("0.Оборудование/Обобщ.модели/Спринклеры");
                            }
                            else if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DuctAccessory
                                || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeAccessory)
                            {
                                element.get_Parameter(KRTRS_Group).Set("1.Арматура воздуховодов/трубопроводов");
                            }
                            else if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeCurves
                                || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DuctCurves
                                || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_FlexDuctCurves
                                || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_FlexPipeCurves
                                || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_CableTray
                                || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Conduit)
                            {
                                element.get_Parameter(KRTRS_Group).Set("2.Воздуховоды/Трубы/Кабельные лотки/Короба");
                            }
                            else if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DuctTerminal
                                || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_LightingDevices
                                || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_LightingFixtures
                                || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_ElectricalFixtures)
                            {
                                element.get_Parameter(KRTRS_Group).Set("3.Воздухораспределители/Эл.Приборы");
                            }
                            else if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DuctInsulations
                                || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeInsulations
                                || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DuctLinings)
                            {
                                element.get_Parameter(KRTRS_Group).Set("4.Материалы изоляции");
                            }
                            else if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DuctFitting
                                || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting
                                || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_CableTrayFitting
                                || element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_ConduitFitting)
                            {
                                element.get_Parameter(KRTRS_Group).Set("5.Соеденительные детали воздуховодов/трубопроводов/кабельных лотков");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log.Error($"ElementID {addedElements.IntegerValue} KRTRS_Группирование error: {ex.Message}");
                    }
                }
            }
        }

        public string GetAdditionalInformation()
        {
            return "GroupMEP Updater Additional Information";
        }
        public ChangePriority GetChangePriority()
        {
            return ChangePriority.MEPAccessoriesFittingsSegmentsWires;
        }
        public UpdaterId GetUpdaterId()
        {
            return updaterId;
        }
        public string GetUpdaterName()
        {
            return "GroupMEP Updater";
        }
    }
}
