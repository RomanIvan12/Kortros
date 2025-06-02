using Autodesk.Revit.DB;
using Kortros.Architecture.Apartments.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Kortros.Architecture.Apartments.Extensions
{
    public class ISymbolUpdater : IUpdater
    {
        static UpdaterId updaterId;
        public ISymbolUpdater(AddInId addInId)
        {
            updaterId = new UpdaterId(addInId, new Guid("5EF25DE9-C8B3-4541-9575-F6E37B858056"));
            RegisterUpdater();
            RegisterTriggers();
        }

        public void Execute(UpdaterData data)
        {
            Document doc = data.GetDocument();

            foreach (ElementId addedElements in data.GetAddedElementIds())
            {
                Element element = doc.GetElement(addedElements);
                if (element != null && element.Name == "Шаблон таблицы")
                {
                    MessageBox.Show(element.Id.IntegerValue.ToString());
                    // СЮДА ФУНКЦИОНАЛ ЗАПОЛНЕНИЯ
                    TableInstanceVM tvm = TableInstanceVM.VM; // Окно ViewModel

                    if (!tvm.HideGNS)
                    {
                        element.LookupParameter("Значения ГНС корпуса").Set(1);
                    }
                    else
                    {
                        element.LookupParameter("Значения ГНС корпуса").Set(0);
                    }

                    if (!tvm.HideKoff)
                    {
                        element.LookupParameter("Видимость коэффициентов").Set(1);
                    }
                    else
                    {
                        element.LookupParameter("Видимость коэффициентов").Set(0);
                    }

                    var instance = TableInstanceVM.tableInstance;

                    if (instance.AreaOfLivingSpace != 0)
                        element.LookupParameter("S общ на этаже").Set(instance.AreaOfLivingSpace);
                    else
                        element.LookupParameter("S общ на этаже").Set(11);

                    if (instance.AreaOfSpace != 0)
                        element.LookupParameter("S общ всех помещений на этаже").Set(instance.AreaOfSpace);
                    else
                        element.LookupParameter("S общ всех помещений на этаже").Set(11);

                    if (instance.IsAreaVNSCorrect)
                        element.LookupParameter("S этажа ВНС").Set(instance.AreaVNS);
                    else
                        element.LookupParameter("S этажа ВНС").Set(11);

                    if (instance.IsAreaGNS1Correct)
                        element.LookupParameter("S этажа ГНС1").Set(instance.AreaGNS1);
                    else
                        element.LookupParameter("S этажа ГНС1").Set(11);

                    if (instance.IsAreaGNS2Correct)
                        element.LookupParameter("S этажа ГНС2").Set(instance.AreaGNS2);
                    else
                        element.LookupParameter("S этажа ГНС2").Set(11);

                    if (instance.AreaSectionGNS1 == 0)
                        element.LookupParameter("S всего корпуса ГНС1").Set(instance.AreaSectionGNS1);
                    else
                        element.LookupParameter("S всего корпуса ГНС1").Set(11);

                    if (instance.AreaSectionGNS2 == 0)
                        element.LookupParameter("S всего корпуса ГНС2").Set(instance.AreaSectionGNS1);
                    else
                        element.LookupParameter("S всего корпуса ГНС2").Set(11);

                    element.LookupParameter("Этаж").Set(instance.Level);

                    if (instance.IsVersionsAvailable)
                        element.LookupParameter("Номер варианта").Set(instance.Version);
                    else
                        element.LookupParameter("Номер варианта").Set("");
                }
            }
            UpdaterRegistry.DisableUpdater(updaterId);
        }
        public void RegisterUpdater()
        {
            if (UpdaterRegistry.IsUpdaterRegistered(updaterId))
            {
                UpdaterRegistry.UnregisterUpdater(updaterId);
            }
            UpdaterRegistry.RegisterUpdater(this, true);
        }
        public void RegisterTriggers()
        {
            ElementCategoryFilter categoryFilter = new ElementCategoryFilter(BuiltInCategory.OST_GenericAnnotation);

            if (updaterId != null && UpdaterRegistry.IsUpdaterRegistered(updaterId))
            {
                UpdaterRegistry.RemoveAllTriggers(updaterId);

                UpdaterRegistry.AddTrigger(updaterId, categoryFilter, Element.GetChangeTypeElementAddition());
            }
        }


        public string GetAdditionalInformation()
        {
            return "ISymbolUpdater Additional Information";
        }

        public ChangePriority GetChangePriority()
        {
            return ChangePriority.Annotations;
        }

        public UpdaterId GetUpdaterId()
        {
            return updaterId;
        }

        public string GetUpdaterName()
        {
            return "ISymbolUpdater Name"; ;
        }
    }
}
