using System.Windows;
using System.Windows.Controls;
using Res = Dynamo.Wpf.Properties.Resources;

namespace Dynamo.Prompts
{
    /// <summary>
    /// Interaction logic for ChangeScaleFactorPrompt.xaml
    /// </summary>
    public partial class ChangeScaleFactorPrompt : Window
    {
        // Maximum and minimum scale factor values, log 10
        private readonly int MaxLogValue = 8;
        private readonly int MinLogValue = -8;

        // The number of rows to be highlighted
        private readonly int SpanLogValue = 9;

        public ChangeScaleFactorPrompt(int sliderValue = 0)
        {
            InitializeComponent();

            this.UnitComboBox.Items.Add(Res.ChangeScaleFactorPromptUnitMm);
            this.UnitComboBox.Items.Add(Res.ChangeScaleFactorPromptUnitCm);
            this.UnitComboBox.Items.Add(Res.ChangeScaleFactorPromptUnitM);
            this.UnitComboBox.SelectedIndex = 0; // default to mm
            RefreshUnits();

            this.UnitsSlider.Minimum = MinLogValue + SpanLogValue / 2;
            this.UnitsSlider.Maximum = MaxLogValue - SpanLogValue / 2;
            this.UnitsSlider.Height = (MaxLogValue - MinLogValue + 1) * 20;
            this.UnitsSlider.Value = sliderValue;
            RefreshHighlight();
        }

        void OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        void Slider_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            RefreshHighlight();
        }

        void UnitComboBox_Changed(object sender, SelectionChangedEventArgs e)
        {
            RefreshUnits();
            RefreshHighlight();
        }

        private void RefreshUnits()
        {
            var conversionPow = 0;
            var fmt = "";
            switch (UnitComboBox.SelectedIndex)
            {
                case 0:
                    conversionPow = 0;
                    fmt = Res.ChangeScaleFactorPromptUnitsNumberFormatMm;
                    break;
                case 1:
                    conversionPow = -1;
                    fmt = Res.ChangeScaleFactorPromptUnitsNumberFormatCm;
                    break;
                case 2:
                    conversionPow = -3; fmt = Res.ChangeScaleFactorPromptUnitsNumberFormatM; break;
            }

            this.UnitsList.Children.Clear();
            for (int pow = MaxLogValue; pow >= MinLogValue; --pow)
            {
                var num = System.Math.Pow(10, pow + conversionPow);
                this.UnitsList.Children.Add(new TextBlock()
                {
                    Text = string.Format(fmt, num)
                });
            }
        }

        private void RefreshHighlight()
        {
            var val = this.UnitsSlider.Value;

            this.DescriptionScaleRange.Text = 
                (val > 0) ? Res.ChangeScaleFactorPromptDescriptionContent0 :
                (val < 0) ? Res.ChangeScaleFactorPromptDescriptionContent2 :
                Res.ChangeScaleFactorPromptDescriptionContent1;

            this.DescriptionDefaultSetting.Text =
                (val > 0) ? string.Empty :
                (val < 0) ? string.Empty :
                Res.ChangeScaleFactorPromptDescriptionDefaultSetting;
        }

        public int SliderValue
        {
            get { return (int)this.UnitsSlider.Value; }
        }
    }
}
