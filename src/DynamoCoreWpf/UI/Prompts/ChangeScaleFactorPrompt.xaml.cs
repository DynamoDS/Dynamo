using System;
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
        public ChangeScaleFactorPrompt(int scaleValue = 0)
        {
            InitializeComponent();

            var col = (scaleValue / 2) + 1;
            var tb = (ToggleButton)this.ScaleButtonsGrid.Children
                .Cast<UIElement>()
                .First(e => Grid.GetColumn(e) == col);
            tb.IsChecked = true;
            RefreshDescription(col);
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
            RefreshDescription(col);
        }

        void Apply_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void RefreshDescription(int val)
        {
            this.DescriptionScaleRange.Text =
                (val == 0) ? Res.ChangeScaleFactorPromptDescriptionContent0 :
                (val == 1) ? Res.ChangeScaleFactorPromptDescriptionContent1 :
                (val == 2) ? Res.ChangeScaleFactorPromptDescriptionContent2 :
                Res.ChangeScaleFactorPromptDescriptionContent3;
            this.DescriptionDefaultSetting.Text =
               (val != 1) ? string.Empty : Res.ChangeScaleFactorPromptDescriptionDefaultSetting;
        }

        private int getScaleValue()
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
            get { return getScaleValue(); }
        }
    }
}
