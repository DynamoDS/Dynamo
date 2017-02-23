using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
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
            Small = 0,
            Medium = 1,
            Large = 2,
            ExtraLarge = 3
        }

        public Size size { get; set; }

        public Tuple<string, string> ScaleRange {
            get
            {
                return scaleRanges[size];
            }
        }

        private Dictionary<Size, Tuple<string, string>> scaleRanges = new Dictionary<Size, Tuple<string, string>>
        {
            {Size.Small, new Tuple<string, string>("0.000,001", "100")},
            {Size.Medium, new Tuple<string, string>("0.0001", "10,000")},
            {Size.Large, new Tuple<string, string>("0.01", "1000,000")},
            {Size.ExtraLarge, new Tuple<string, string>("1", "100,000,000")}
        };

        public ChangeScaleFactorPrompt(int scaleValue = 0)
        {
            InitializeComponent();

            var col = (scaleValue / 2) + 1;

            var tb = (ToggleButton)this.ScaleButtonsGrid.Children
                .Cast<UIElement>()
                .First(e => Grid.GetColumn(e) == col);
            tb.IsChecked = true;

            Update((Size)col);
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
            int col = (int)toggleButton.GetValue(Grid.ColumnProperty);

            Update((Size)col);
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
                scaleRanges[highlightedSize].Item1, scaleRanges[highlightedSize].Item2);
            this.DescriptionDefaultSetting.Text =
               (highlightedSize == Size.Medium) ? Res.ChangeScaleFactorPromptDescriptionDefaultSetting : String.Empty;
            size = highlightedSize;
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
