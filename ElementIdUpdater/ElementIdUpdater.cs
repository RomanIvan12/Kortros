using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;

namespace ElementIdUpdater
{
    public class ElementIdUpdater : IUpdater
    {
        static UpdaterId updaterId;
        public ElementIdUpdater(AddInId id)
        {
            updaterId = new UpdaterId(id, new Guid("2B12BCC7-2832-41E2-BDCA-FE81F6468BFF"));
            RegisterUpdater();
            RegisterTriggers();
        }

        public void Execute(UpdaterData data)
        {
            Document doc = data.GetDocument();
            Guid ElementId = new Guid("495ac66d-09f7-466e-b85a-59f70db13553");

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
                        if (existedParams.Contains("Element_ID"))
                        {
                            element.get_Parameter(ElementId).Set(addedElements.IntegerValue.ToString());
                        }
                    }
                    catch { }
                }
            }
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
            ElementMulticategoryFilter categoryIdFilter = new ElementMulticategoryFilter(new List<BuiltInCategory>()
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

                BuiltInCategory.OST_CableTray,
                BuiltInCategory.OST_CableTrayFitting,
                BuiltInCategory.OST_Conduit,
                BuiltInCategory.OST_ConduitFitting,

                BuiltInCategory.OST_ElectricalFixtures,
                BuiltInCategory.OST_LightingFixtures,
                BuiltInCategory.OST_DataDevices,
                BuiltInCategory.OST_FireAlarmDevices,
                BuiltInCategory.OST_LightingDevices,
                BuiltInCategory.OST_CommunicationDevices,
                BuiltInCategory.OST_SecurityDevices,
                BuiltInCategory.OST_NurseCallDevices,

                BuiltInCategory.OST_CurtaSystem, // Витражные системы
                BuiltInCategory.OST_Doors,
                BuiltInCategory.OST_Roads, // Дорожки
                BuiltInCategory.OST_CurtainWallMullions, // Импосты витража
                BuiltInCategory.OST_Columns, // Колонны
                BuiltInCategory.OST_ImportObjectStyles, // Комплекты мебели
                BuiltInCategory.OST_Roofs,
                BuiltInCategory.OST_Stairs, // Лестницы
                BuiltInCategory.OST_Materials, // Материалы
                BuiltInCategory.OST_gbXML_SlabOnGrade, // Мебель
                BuiltInCategory.OST_MedicalEquipment, // Медицинское оборудование
                BuiltInCategory.OST_FoodServiceEquipment, // Оборуд. для предприятий общ.пита
                BuiltInCategory.OST_StairsRailing, // Ограждения
                BuiltInCategory.OST_MechanicalEquipment, // Озеленение
                BuiltInCategory.OST_Windows,
                BuiltInCategory.OST_Ramps, // Пандус
                BuiltInCategory.OST_CurtainWallPanels, // Панель витража
                BuiltInCategory.OST_Parking, // Парковка
                BuiltInCategory.OST_Floors, // Перекрытия
                BuiltInCategory.OST_PlumbingFixtures, // Сантехнические приборы
                BuiltInCategory.OST_FireProtection, // Системы противопожарной оммун
                BuiltInCategory.OST_SpecialityEquipment, // Специальное оборудование
                BuiltInCategory.OST_Walls, // Стены
                BuiltInCategory.OST_TelephoneDevices, // Топография
                BuiltInCategory.OST_Casework, // Шкафы


                BuiltInCategory.OST_StructuralFramingSystem, // Балочные системы
                BuiltInCategory.OST_StructuralFraming, // Каркас несущий
                BuiltInCategory.OST_Rebar, // Несущая арматура
                BuiltInCategory.OST_StructuralColumns, // Несущие колонны
                BuiltInCategory.OST_FabricAreas, // Обл. раскладки арматурных сеток 
                BuiltInCategory.OST_IsolatedFoundationAnalytical, // Фундамент несущей констр
            });

            if (updaterId != null && UpdaterRegistry.IsUpdaterRegistered(updaterId))
            {
                UpdaterRegistry.RemoveAllTriggers(updaterId);
                //TODO elementFilter

                UpdaterRegistry.AddTrigger(updaterId,
                    categoryIdFilter,
                    Element.GetChangeTypeElementAddition());
            }
        }


        public string GetAdditionalInformation()
        {
            return "Update ElementId Additional Information";
        }

        public ChangePriority GetChangePriority()
        {
            return ChangePriority.FreeStandingComponents;
        }

        public UpdaterId GetUpdaterId()
        {
            return updaterId;
        }

        public string GetUpdaterName()
        {
            return "ElementId Updater";
        }
    }
}
