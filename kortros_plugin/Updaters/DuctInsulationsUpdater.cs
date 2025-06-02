using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Kortros.Utilities;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kortros.Updaters
{
    internal class DuctInsulationsUpdater : IUpdater
    {
        static UpdaterId updaterId;

        public DuctInsulationsUpdater(AddInId id)
        {
            updaterId = new UpdaterId(id, new Guid("38A4E97F-83EB-490A-850E-55D471E8CC07"));
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
            // Фильтр типа DuctInsulation
            ElementClassFilter ductInsFilter = new ElementClassFilter(typeof(DuctInsulation));

            if (updaterId != null && UpdaterRegistry.IsUpdaterRegistered(updaterId))
            {
                UpdaterRegistry.RemoveAllTriggers(updaterId);

                UpdaterRegistry.AddTrigger(updaterId,
                    ductInsFilter,
                    Element.GetChangeTypeParameter(new ElementId(BuiltInParameter.RBS_SYSTEM_NAME_PARAM)));
                UpdaterRegistry.AddTrigger(updaterId,
                    ductInsFilter,
                    Element.GetChangeTypeElementAddition());
                UpdaterRegistry.AddTrigger(updaterId,
                    ductInsFilter,
                    Element.GetChangeTypeGeometry());
            }
        }

        public void Execute(UpdaterData data)
        {
            Document doc = data.GetDocument();
            Guid KRTRS_Count = new Guid("495d66c9-c5b5-493e-9b0f-73afc67e4216");
            Guid KRTRS_Izm = new Guid("7621a737-0095-43fe-93fd-ee4a267a6cab");

            ForgeTypeId displayUnits = doc.GetUnits().GetFormatOptions(SpecTypeId.Area).GetUnitTypeId();

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
                        {
                            continue;
                        }
                        if (hostElem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DuctCurves)
                        {
                            double zapasValue = 1;
                            if (UtilFunctions.GetGlobalParameterDoubleValueByName(doc, "Запас изоляции") != null)
                                zapasValue = (double)UtilFunctions.GetGlobalParameterDoubleValueByName(doc, "Запас");
                            Parameter areaIns = element.get_Parameter(BuiltInParameter.RBS_CURVE_SURFACE_AREA);
                            double ConvArea = UnitUtils.ConvertFromInternalUnits(areaIns.AsDouble(), displayUnits);
                            element.get_Parameter(KRTRS_Count).Set(ConvArea * zapasValue);
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
                        element.get_Parameter(KRTRS_Izm).Set("м\u00B2");
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
            return "DuctInsulation Updater Additional Information";
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
            return "DuctInsulation Updater";
        }
    }
}
