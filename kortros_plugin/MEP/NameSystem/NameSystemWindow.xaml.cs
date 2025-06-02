using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using Kortros.Utilities;


namespace Kortros.MEP.NameSystem
{
    public partial class NameSystemWindow : Window
    {
        List<Element> allElements;
        Document doc;
        Guid nameSys = new Guid("f5db6958-854a-4e72-b87d-6bfe54518eac");
        public NameSystemWindow(Document document, List<Element> elems)
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            InitializeComponent();
            allElements = elems;
            doc = document;
        }
        private void OKButton(object sender, RoutedEventArgs e)
        {
            if (SysName.IsChecked == true)
            {
                Logger.Log.Info("Выбрано Имя Системы");
                using (Transaction t = new Transaction(doc, "Name System"))
                {
                    t.Start();
                    foreach (Element el in allElements)
                    {
                        try
                        {
                            string Value = el.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM).AsValueString();
                            el.get_Parameter(nameSys).Set(Value);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                        }
                    }
                    t.Commit();
                }
                this.Close();
            }
            else if (SysShort.IsChecked == true)
            {
                Logger.Log.Info("Выбрано Сокращение для системы");
                using (Transaction t = new Transaction(doc, "Name Short"))
                {
                    t.Start();
                    foreach (Element el in allElements)
                    {
                        try
                        {
                            if (el.Category.Id.IntegerValue != (int)BuiltInCategory.OST_MechanicalEquipment)
                            {
                                string Value = el.get_Parameter(BuiltInParameter.RBS_DUCT_PIPE_SYSTEM_ABBREVIATION_PARAM).AsValueString();
                                el.get_Parameter(nameSys).Set(Value);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                        }
                    }
                    t.Commit();
                }
                this.Close();
            }
        }
    }
}
