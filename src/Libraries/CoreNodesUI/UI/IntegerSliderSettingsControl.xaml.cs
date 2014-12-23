using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Dynamo.Controls;

namespace DSCoreNodesUI
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class IntegerSliderSettingsControl : UserControl
    {
        public IntegerSliderSettingsControl()
        {
            InitializeComponent();
            this.Loaded += IntegerSliderSettingsControl_Loaded;
        }

        void IntegerSliderSettingsControl_Loaded(object sender, RoutedEventArgs e)
        {
            MaxTb.BindToProperty(
                new Binding("Max")
                {
                    Mode = BindingMode.TwoWay,
                    Converter = new IntegerDisplay(),
                    UpdateSourceTrigger = UpdateSourceTrigger.Explicit
                });

            MinTb.BindToProperty(
                new Binding("Min")
                {
                    Mode = BindingMode.TwoWay,
                    Converter = new IntegerDisplay(),
                    UpdateSourceTrigger = UpdateSourceTrigger.Explicit
                });
        }
    }
}
