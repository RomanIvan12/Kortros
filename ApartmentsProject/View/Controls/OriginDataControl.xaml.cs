using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ApartmentsProject.Models;

namespace ApartmentsProject.View.Controls
{
    /// <summary>
    /// Логика взаимодействия для OriginDataControl.xaml
    /// </summary>
    public partial class OriginDataControl : UserControl
    {

        public ParameterMappingModel Data
        {
            get { return (ParameterMappingModel)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Data.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(ParameterMappingModel), typeof(OriginDataControl), new PropertyMetadata(null, SetValue));


        private static void SetValue(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            OriginDataControl control = d as OriginDataControl;

            if (control != null)
            {
                control.DataName.Text = (e.NewValue as ParameterMappingModel).DataOrigin.Name;
                control.DataDescription.Text = (e.NewValue as ParameterMappingModel).DataOrigin.Description;

                string dataType = (e.NewValue as ParameterMappingModel).DataOrigin.DataType;
                switch (dataType)
                {
                    case "AREA":
                        control.DataType.Text = "[Площадь]";
                        break;
                    case "TEXT":
                        control.DataType.Text = "[Текст]";
                        break;
                    case "CURRENCY":
                        control.DataType.Text = "[Денежная единица]";
                        break;
                }
            }
        }
        public OriginDataControl()
        {
            InitializeComponent();
        }
    }
}
