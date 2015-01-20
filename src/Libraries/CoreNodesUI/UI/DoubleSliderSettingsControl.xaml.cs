using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Dynamo.Controls;

namespace DSCoreNodesUI
{
    /// <summary>
    /// Interaction logic for SliderSettingsControl.xaml
    /// </summary>
    public partial class DoubleSliderSettingsControl : UserControl
    {
        public DoubleSliderSettingsControl()
        {
            InitializeComponent();
            this.Loaded += DoubleSliderSettingsControl_Loaded;
        }

        void DoubleSliderSettingsControl_Loaded(object sender, RoutedEventArgs e)
        {
            MaxTb.BindToProperty(
                new Binding("Max")
                {
                    Mode = BindingMode.TwoWay,
                    Converter = new DoubleDisplay(),
                    UpdateSourceTrigger = UpdateSourceTrigger.Explicit
                });

            MinTb.BindToProperty(
                new Binding("Min")
                {
                    Mode = BindingMode.TwoWay,
                    Converter = new DoubleDisplay(),
                    UpdateSourceTrigger = UpdateSourceTrigger.Explicit
                });
        }
    }
}
