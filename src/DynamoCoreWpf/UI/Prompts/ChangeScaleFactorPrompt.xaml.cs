using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Res = Dynamo.Wpf.Properties.Resources;

namespace Dynamo.Prompts
{
    /// <summary>
    /// Interaction logic for ChangeScaleFactorPrompt.xaml
    /// </summary>
    public partial class ChangeScaleFactorPrompt : Window
    {
        public enum Size
        {
            Medium,
            Small,
            Large,
            ExtraLarge
        }

        public Size ScaleSize { get; set; }

        public Tuple<string, string, string> ScaleRange {
            get
            {
                return scaleRanges[ScaleSize];
            }
        }

        private Dictionary<Size, Tuple<string, string, string>> scaleRanges = new Dictionary<Size, Tuple<string, string, string>>
        {
            {Size.Medium, new Tuple<string, string, string>("medium", "0.0001", "10,000")},
            {Size.Small, new Tuple<string, string, string>("small", "0.000,001", "100")},
            {Size.Large, new Tuple<string, string, string>("large", "0.01", "1,000,000")},
            {Size.ExtraLarge, new Tuple<string, string, string>("extra large", "1", "100,000,000")}
        };

        public ChangeScaleFactorPrompt(int scaleValue = 0)
        {
            InitializeComponent();

            var col = (scaleValue / 2) + 1;

            var toggleButton = (ToggleButton)this.ScaleButtonsGrid.Children
                .Cast<UIElement>()
                .First(e => Grid.GetColumn(e) == col);
            toggleButton.IsChecked = true;

            Update((Size)toggleButton.Tag);
        }

        void ScaleButton_click(object sender, RoutedEventArgs e)
        {
            foreach(Control c in this.ScaleButtonsGrid.Children)
            {
                if(c.GetType() == typeof(ToggleButton))
                {
                    ToggleButton tb = (ToggleButton)c;
                    tb.IsChecked = false;
                }
            }

            var toggleButton = sender as ToggleButton;
            toggleButton.IsChecked = true;

            Update((Size)toggleButton.Tag);
        }

        void Apply_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Update(Size highlightedSize)
        {
            this.DescriptionScaleRange.Text = String.Format(Res.ChangeScaleFactorPromptDescriptionContent, 
                scaleRanges[highlightedSize].Item2, scaleRanges[highlightedSize].Item3);
            this.DescriptionDefaultSetting.Text =
               (highlightedSize == Size.Medium) ? Res.ChangeScaleFactorPromptDescriptionDefaultSetting : String.Empty;
            ScaleSize = highlightedSize;
        }

        private int GetScaleValue()
        {
            foreach (Control c in this.ScaleButtonsGrid.Children)
            {
                if (c.GetType() == typeof(ToggleButton))
                {
                    ToggleButton tb = (ToggleButton)c;
                    if((bool)tb.IsChecked)
                    {
                        return ((int)tb.GetValue(Grid.ColumnProperty) -1) * 2;
                    }
                }
            }
            return 0;
        }

        public int ScaleValue
        {
            get { return GetScaleValue(); }
        }
    }
}
