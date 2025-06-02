using Kortros.ParamParser.Model;
using System.Windows;
using System.Windows.Controls;

namespace Kortros.ParamParser.View.Controls
{
    /// <summary>
    /// Логика взаимодействия для ParamInitControl.xaml
    /// </summary>
    public partial class ParamInitControl : UserControl
    {
        public ParamStack ParamStack
        {
            get { return (ParamStack)GetValue(ParamStackProperty); }
            set { SetValue(ParamStackProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ParamStack.  This enables animation, styling, binding, etc...

        //new ParamStack() { NameInit = "Name", IsTypeInit = true, StorageTypeInit = "Storage" }
        public static readonly DependencyProperty ParamStackProperty =
            DependencyProperty.Register("ParamStack", typeof(ParamStack), typeof(ParamInitControl), new PropertyMetadata(null, SetValue));

        private static void SetValue(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ParamInitControl control = d as ParamInitControl;

            // ДРУГОЙ ТЕКСТ НАДО
            if (control != null)
            {
                control.paramName.Text = (e.NewValue as ParamStack).NameInit;
                control.paramType.Text =
                    (e.NewValue as ParamStack).IsTypeInit ? "Параметр типа" : "Параметр экземпляра";

                string storageType = (e.NewValue as ParamStack).StorageTypeInit;
                switch (storageType)
                {
                    case "Integer":
                        control.paramStorage.Text = "Тип данных: Целое";
                        break;
                    case "String":
                        control.paramStorage.Text = "Тип данных: Строка";
                        break;
                    case "Double":
                        control.paramStorage.Text = "Тип данных: Дробное";
                        break;
                    case "ElementId":
                        control.paramStorage.Text = "Тип данных: ElementId";
                        break;
                    default:
                        control.paramStorage.Text = (e.NewValue as ParamStack).StorageTypeInit;
                        break;
                }
            }
        }
        public ParamInitControl()
        {
            InitializeComponent();
        }
    }
}
