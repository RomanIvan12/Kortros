using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Kortros.Utilities;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kortros.Updaters
{
    public class PipeUpdater : IUpdater
    {
        static UpdaterId updaterId;

        public PipeUpdater(AddInId id)
        {
            //_logger.Info($"PipeUpdater started: {doc.ActiveView.Name}");
            updaterId = new UpdaterId(id, new Guid("2D89A647-55E2-4570-AFC7-563A957784C0"));
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
            // Фильтр типов Pipe & FlexPipe
            ElementMulticlassFilter pipeFilter = new ElementMulticlassFilter(new List<Type>()
            {
                typeof(Pipe),
                typeof(FlexPipe),
            });

            if (updaterId != null && UpdaterRegistry.IsUpdaterRegistered(updaterId))
            {
                UpdaterRegistry.RemoveAllTriggers(updaterId);

                UpdaterRegistry.AddTrigger(updaterId,
                    pipeFilter,
                    Element.GetChangeTypeParameter(new ElementId(BuiltInParameter.RBS_SYSTEM_NAME_PARAM)));
                UpdaterRegistry.AddTrigger(updaterId,
                    pipeFilter,
                    Element.GetChangeTypeElementAddition());
                UpdaterRegistry.AddTrigger(updaterId,
                    pipeFilter,
                    Element.GetChangeTypeGeometry());
            }
        }

        public void Execute(UpdaterData data)
        {
            Document doc = data.GetDocument();
            Guid KRTRS_Count = new Guid("495d66c9-c5b5-493e-9b0f-73afc67e4216");
            Guid KRTRS_Izm = new Guid("7621a737-0095-43fe-93fd-ee4a267a6cab");

            ForgeTypeId displayUnits = doc.GetUnits().GetFormatOptions(SpecTypeId.Length).GetUnitTypeId();

            IEnumerable<ElementId> combinedCollection = data.GetAddedElementIds().Concat(data.GetModifiedElementIds());

            foreach (ElementId elementId in combinedCollection)
            {
                Element element = doc.GetElement(elementId);
                
                if (element.GroupId.IntegerValue == -1)
                {
                    if (doc.IsWorkshared)
                    {
                        // Обновление рабочего набора для ОВ
                        //try
                        //{
                        //    UtilFunctions.CommentBasedPipeWorkset(doc, element);
                        //}
                        //catch (Exception ex)
                        //{
                        //    _logger.Error($"ElementID {elementId.IntegerValue} Workset error: {ex.Message}");
                        //}

                        // Обновление рабочего набора для ВК
                        try
                        {
                            UtilFunctions.CommentBasedPipeWorkset(doc, element);
                        }
                        catch (Exception ex)
                        {
                            Logger.Log.Error($"ElementID {elementId.IntegerValue} Workset error: {ex.Message}");
                        }
                    }

                    // Заполнение KRTRS_Количество
                    try
                    {
                        double zapasValue = 1;
                        if (UtilFunctions.GetGlobalParameterDoubleValueByName(doc, "Запас") != null)
                            zapasValue = (double)UtilFunctions.GetGlobalParameterDoubleValueByName(doc, "Запас");
                        double LengthValue = element.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble();
                        double ConvValue = UnitUtils.ConvertFromInternalUnits(LengthValue, displayUnits);
                        element.get_Parameter(KRTRS_Count).Set(ConvValue * zapasValue / 1000);
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
                        element.get_Parameter(KRTRS_Izm).Set("м");
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
            return "Pipe Updater Additional Information";
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
            return "Pipe Updater";
        }
    }
}
