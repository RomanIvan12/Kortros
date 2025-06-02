using Autodesk.Revit.DB;
using Kortros.ParamParser.ViewModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Kortros.ParamParser.View
{
    /// <summary>
    /// Логика взаимодействия для SelectCategoryWindow.xaml
    /// </summary>
    public partial class SelectCategoryWindow : Window
    {
        public static List<BuiltInCategory> categories = new List<BuiltInCategory> {
            BuiltInCategory.OST_Walls, // +
            BuiltInCategory.OST_DuctAccessory, // +
            BuiltInCategory.OST_PipeAccessory, // +
            BuiltInCategory.OST_DuctCurves, // +
            BuiltInCategory.OST_PlaceHolderDucts, // +
            BuiltInCategory.OST_DuctTerminal, // +
            BuiltInCategory.OST_LightingDevices, // +
            BuiltInCategory.OST_FlexDuctCurves, // +
            BuiltInCategory.OST_FlexPipeCurves, // +
            BuiltInCategory.OST_DataDevices, // +
            BuiltInCategory.OST_Areas, // +
            BuiltInCategory.OST_HVAC_Zones,  // +????
            BuiltInCategory.OST_CableTray,  // +
            BuiltInCategory.OST_Conduit, // +
            BuiltInCategory.OST_DuctLinings, // +
            BuiltInCategory.OST_DuctInsulations, // +
            BuiltInCategory.OST_PipeInsulations, // +
            BuiltInCategory.OST_GenericModel, // +
            BuiltInCategory.OST_MechanicalEquipment, // +
            BuiltInCategory.OST_Windows, // +
            BuiltInCategory.OST_Doors, // +
            BuiltInCategory.OST_LightingFixtures, // +
            BuiltInCategory.OST_SecurityDevices,
            BuiltInCategory.OST_FireAlarmDevices,
            BuiltInCategory.OST_Rooms,  // + цепи и комплект мебели
            BuiltInCategory.OST_Wire, // +
            BuiltInCategory.OST_MEPSpaces, // +
            BuiltInCategory.OST_PlumbingFixtures, // +
            BuiltInCategory.OST_DuctSystem,
            BuiltInCategory.OST_DuctFitting, // +
            BuiltInCategory.OST_CableTrayFitting, // +
            BuiltInCategory.OST_ConduitFitting, // +
            BuiltInCategory.OST_PipeFitting, // +
            BuiltInCategory.OST_Sprinklers, // +
            BuiltInCategory.OST_TelephoneDevices, // +
            BuiltInCategory.OST_PlaceHolderPipes, // +
            BuiltInCategory.OST_PipingSystem, // +
            BuiltInCategory.OST_PipeCurves, // +
            BuiltInCategory.OST_NurseCallDevices, // +
            BuiltInCategory.OST_CommunicationDevices, // +
            BuiltInCategory.OST_CableTrayRun,
            BuiltInCategory.OST_Mass, // +
            BuiltInCategory.OST_Parts, // +
            BuiltInCategory.OST_ElectricalFixtures, // +
            BuiltInCategory.OST_ElectricalCircuit,
            BuiltInCategory.OST_ElectricalEquipment, // +
            BuiltInCategory.OST_DetailComponents,
            BuiltInCategory.OST_Furniture, // +
            BuiltInCategory.OST_FurnitureSystems, // +
            BuiltInCategory.OST_FireAlarmDevices, // +
            BuiltInCategory.OST_SecurityDevices, // +
        };

        public ParamStackVM VM { get; set; }

        public SelectCategoryWindow(ParamStackVM vm)
        {
            Assembly.LoadFrom(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "MaterialDesignThemes.Wpf.dll"));
            Assembly.LoadFrom(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "MaterialDesignColors.dll"));
            InitializeComponent();

            VM = vm;

            var catItems = VM.CategoryItems.Select(x => x.Name);

            List<CategoryName> categoryNames = new List<CategoryName>();
            foreach (BuiltInCategory categoryName in categories)
            {
                if (catItems.Contains(categoryName.ToString()))
                {
                    CategoryName i = new CategoryName()
                    {
                        BnCategory = categoryName.ToString(),
                        CatItemName = Category.GetCategory(RunCommand.Doc, categoryName).Name,
                        IsDisabled = true,
                    };
                    categoryNames.Add(i);
                }
                else
                {
                    CategoryName i = new CategoryName()
                    {
                        BnCategory = categoryName.ToString(),
                        CatItemName = Category.GetCategory(RunCommand.Doc, categoryName).Name,
                        IsDisabled = false,
                    };
                    categoryNames.Add(i);
                }
            }
            BinCat.ItemsSource = categoryNames;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BinCat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BinCat.SelectedItem != null)
            {
                CategoryName selectedValue = BinCat.SelectedItem as CategoryName;
                DialogResult = true;
                Tag = selectedValue.BnCategory;
                Close();
            }
        }
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
    }
    public class CategoryName
    {
        public string BnCategory { get; set; }
        public string CatItemName { get; set; }
        public bool IsDisabled { get; set; }
    }
}
