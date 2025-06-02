using System;
using Autodesk.Revit.UI;

namespace ApartmentsProject.AuxiliaryСlasses
{
    public class ExternalEventHandler: IExternalEventHandler
    {
        private Action _action;

        public void SetAction(Action action)
        {
            _action = action;
        }

        public void Execute(UIApplication app)
        {
            try
            {
                _action?.Invoke();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"Ошибка: {ex.Message} \n{ex.StackTrace}");
            }
        }

        public string GetName()
        {
            return "External Event Handler";
        }
    }
}
