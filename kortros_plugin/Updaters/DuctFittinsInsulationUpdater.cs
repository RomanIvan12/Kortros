using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Kortros.Utilities;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kortros.Updaters
{
    internal class DuctFittinsInsulationUpdater : IUpdater
    {
        static UpdaterId updaterId;

        public DuctFittinsInsulationUpdater(AddInId id, Document doc)
        {
            updaterId = new UpdaterId(id, new Guid("F45B2C4A-C703-4353-A5A0-0CE84A02E893"));
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
            // Фильтр типов DuctInsulation
            ElementClassFilter ductFitInsFilter = new ElementClassFilter(typeof(DuctInsulation));

            if (updaterId != null && UpdaterRegistry.IsUpdaterRegistered(updaterId))
            {
                UpdaterRegistry.RemoveAllTriggers(updaterId);

                UpdaterRegistry.AddTrigger(updaterId,
                    ductFitInsFilter,
                    Element.GetChangeTypeParameter(new ElementId(BuiltInParameter.RBS_SYSTEM_NAME_PARAM)));
                UpdaterRegistry.AddTrigger(updaterId,
                    ductFitInsFilter,
                    Element.GetChangeTypeElementAddition());
                UpdaterRegistry.AddTrigger(updaterId,
                    ductFitInsFilter,
                    Element.GetChangeTypeGeometry());
            }
        }

        public void Execute(UpdaterData data)
        {
            Document doc = data.GetDocument();
            Guid KRTRS_Count = new Guid("495d66c9-c5b5-493e-9b0f-73afc67e4216");
            Guid KRTRS_Izm = new Guid("7621a737-0095-43fe-93fd-ee4a267a6cab");
            Guid ADSK_area = new Guid("b6a46386-70e9-4b1f-9fdb-8e1e3f18a673"); //адск_площадь
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
                        DuctInsulation ductIns = element as DuctInsulation;
                        Element hostElem = doc.GetElement(ductIns.HostElementId);
                        if (hostElem == null)
                        {
                            continue;
                        }
                        if (hostElem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DuctFitting)
                        {
                            double zapasValue = 1;
                            if (UtilFunctions.GetGlobalParameterDoubleValueByName(doc, "Запас изоляции") != null)
                                zapasValue = (double)UtilFunctions.GetGlobalParameterDoubleValueByName(doc, "Запас изоляции");
                            Parameter areaIns = hostElem.get_Parameter(ADSK_area);
                            double ConvArea = UnitUtils.ConvertFromInternalUnits(areaIns.AsDouble(), displayUnits);
                            element.get_Parameter(KRTRS_Count).Set(ConvArea * zapasValue);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log.Error($"ElementID {elementId.IntegerValue} KRTRS_Количество error: {ex.Message}");
                    }
                    // Заполнение ед. изм.
                    try
                    {
                        element.get_Parameter(KRTRS_Izm).Set("шт.");
                    }
                    catch (Exception ex)
                    {
                        Logger.Log.Error($"ElementID {elementId.IntegerValue}:  KRTRS_Единица измерения - error: {ex.Message}");
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
                }
            }
        }

        public string GetAdditionalInformation()
        {
            return "Duct & Fittings Insulation Updater Additional Information";
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
            return "Duct & Fittings Insulation Updater";
        }
    }
}
