using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Kortros.Utilities;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kortros.Updaters
{
    internal class PipeInsulationUpdater : IUpdater
    {
        static UpdaterId updaterId;

        public PipeInsulationUpdater(AddInId id)
        {
            updaterId = new UpdaterId(id, new Guid("98847DF1-9F7B-4D31-8365-BB1DBECF68A5"));
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
            // Фильтр типа PipeInsulation
            ElementClassFilter pipeInsFilter = new ElementClassFilter(typeof(PipeInsulation));
            if (updaterId != null && UpdaterRegistry.IsUpdaterRegistered(updaterId))
            {
                UpdaterRegistry.RemoveAllTriggers(updaterId);

                UpdaterRegistry.AddTrigger(updaterId,
                    pipeInsFilter,
                    Element.GetChangeTypeParameter(new ElementId(BuiltInParameter.RBS_SYSTEM_NAME_PARAM)));
                UpdaterRegistry.AddTrigger(updaterId,
                    pipeInsFilter,
                    Element.GetChangeTypeElementAddition());
                UpdaterRegistry.AddTrigger(updaterId,
                    pipeInsFilter,
                    Element.GetChangeTypeGeometry());
            }
        }
        public void Execute(UpdaterData data)
        {
            Document doc = data.GetDocument();
            Guid KRTRS_Count = new Guid("495d66c9-c5b5-493e-9b0f-73afc67e4216");
            Guid ADSK_Izm = new Guid("4289cb19-9517-45de-9c02-5a74ebf5c86d"); //адск_единица измерения
            Guid KRTRS_Izm = new Guid("7621a737-0095-43fe-93fd-ee4a267a6cab");

            ForgeTypeId displayUnitsLength = doc.GetUnits().GetFormatOptions(SpecTypeId.Length).GetUnitTypeId();
            ForgeTypeId displayUnitsArea = doc.GetUnits().GetFormatOptions(SpecTypeId.Area).GetUnitTypeId();

            IEnumerable<ElementId> combinedCollection = data.GetAddedElementIds().Concat(data.GetModifiedElementIds());

            foreach (ElementId elementId in combinedCollection)
            {
                Element element = doc.GetElement(elementId);

                if (element.GroupId.IntegerValue == -1)
                {
                    // Заполнение KRTRS_Количество
                    try
                    {
                        InsulationLiningBase lineIns = element as InsulationLiningBase;
                        Element hostElem = doc.GetElement(lineIns.HostElementId);
                        if (hostElem == null)
                            continue;

                        if (hostElem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeCurves)
                        {
                            //Parameter diamValue = hostElem.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM);
                            //double ConvValue = UnitUtils.ConvertFromInternalUnits(diamValue.AsDouble(), displayUnits);
                            ElementId ElementTypeId = element.GetTypeId();
                            Element ElementType = doc.GetElement(ElementTypeId);
                            Parameter Izmer = ElementType.get_Parameter(ADSK_Izm);
                            double zapasValue = 1;
                            if (UtilFunctions.GetGlobalParameterDoubleValueByName(doc, "Запас изоляции") != null)
                                zapasValue = (double)UtilFunctions.GetGlobalParameterDoubleValueByName(doc, "Запас изоляции");

                            if (Izmer.AsString() == "кв.м" || Izmer.AsString() == "м²")
                            {
                                Parameter lengthIns = element.get_Parameter(BuiltInParameter.RBS_CURVE_SURFACE_AREA);
                                double ConvArea = UnitUtils.ConvertFromInternalUnits(lengthIns.AsDouble(), displayUnitsArea);
                                element.get_Parameter(KRTRS_Count).Set(ConvArea * zapasValue);
                            }
                            else
                            {
                                Parameter lengthIns = element.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH);
                                double ConvLength = UnitUtils.ConvertFromInternalUnits(lengthIns.AsDouble(), displayUnitsLength);
                                element.get_Parameter(KRTRS_Count).Set(ConvLength * zapasValue / 1000);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log.Error($"ElementID {elementId.IntegerValue} KRTRS_Количество error: {ex.Message}");
                    }

                    // Заполнение ИмяСистемы
                    try
                    {
                        string SystemName = element.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM).AsValueString();
                        element.LookupParameter("ИмяСистемы").Set(SystemName);
                    }
                    catch (Exception ex) 
                    {
                        Logger.Log.Error($"ElementID {elementId.IntegerValue} ИмяСистемы error: {ex.Message}");
                    }

                    // Заполнение KRTRS_Единица измерения
                    try
                    {
                        element.get_Parameter(KRTRS_Izm).Set("м²");
                    }
                    catch (Exception ex)
                    {
                        Logger.Log.Error($"ElementID {elementId.IntegerValue}: KRTRS_Единица измерения error: {ex.Message}");
                    }
                }
            }
        }

        public string GetAdditionalInformation()
        {
            return "PipeInsulation Updater error";
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
            return "PipeInsulation Updater";
        }
    }
}
