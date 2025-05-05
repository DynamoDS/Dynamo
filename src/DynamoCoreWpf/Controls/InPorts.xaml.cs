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

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// Interaction logic for InPorts.xaml
    /// </summary>
    public partial class InPorts : UserControl
    {
        public InPorts()
        {
            InitializeComponent();

            var artifactElementReg = SharedDictionaryManager.DynamoModernDictionary["ArtifaktElementRegular"] as FontFamily;
            var primaryCharcoal200Brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DCDCDC"));

            var MainGrid = new Grid()
            {
                Name = "MainGrid",
                Height = 34,
                Background = new SolidColorBrush(Colors.Transparent),
                IsHitTestVisible = true
            };

            MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Name = "PortSnappingColumn", Width = new GridLength(25) });
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Name = "ValueMarkerColumn", Width = new GridLength(5) });
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Name = "GapBetweenValueMarkerAndPortName", Width = new GridLength(6) });
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Name = "PortNameColumn", Width = new GridLength(1, GridUnitType.Star) });
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Name = "GapBetweenPortNameAndUseLevelSpinner", Width = new GridLength(6) });
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Name = "UseLevelSpinnerColumn", Width = GridLength.Auto });
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Name = "ChevronColumn", Width = GridLength.Auto });

            //TODO Set up Grid Interactivity Triggers

            var PortSnapping = new System.Windows.Shapes.Rectangle()
            {
                Name = "PortSnapping",
                Fill = new SolidColorBrush(Colors.Transparent),
                SnapsToDevicePixels = true,
            };

            Grid.SetColumn(PortSnapping, 0);
            Grid.SetColumnSpan(PortSnapping, 7);
            Canvas.SetZIndex(PortSnapping, 7);

            PortSnapping.SetBinding(Rectangle.IsHitTestVisibleProperty, new Binding("IsHitTestVisible"));
            //TODO Set up Rectangle Interactivity Triggers

            var PortBackgroundBorder = new Border()
            {
                Name = "PortBackgroundBorder",
                Height = 29,
                BorderThickness = new System.Windows.Thickness(0),
                CornerRadius = new CornerRadius(0, 11, 11, 0),
                IsHitTestVisible = false,
                SnapsToDevicePixels = true
            };

            PortBackgroundBorder.SetBinding(Border.BackgroundProperty, new Binding("PortBackgroundColor") { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

            Grid.SetColumn(PortBackgroundBorder, 1);
            Grid.SetColumnSpan(PortBackgroundBorder, 6);

            var PortValueMarker = new System.Windows.Shapes.Rectangle()
            {
                Name = "PortValueMarker",
                Height = 29,
                VerticalAlignment = VerticalAlignment.Center,
                IsHitTestVisible = false,
                SnapsToDevicePixels = true,
            };

            PortValueMarker.SetBinding(Rectangle.FillProperty, new Binding("PortValueMarkerColor") { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

            Grid.SetColumn(PortValueMarker, 1);

            var PortDefaultValueMarker = new Border()
            {
                Name = "PortDefaultValueMarker",
                Width = 4,
                Height = 27,
                Margin = new Thickness(0, 0, 1, 0),
                HorizontalAlignment = HorizontalAlignment.Right,
            };

            PortDefaultValueMarker.SetBinding(Border.BackgroundProperty, new Binding("PortValueMarkerColor") { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            PortDefaultValueMarker.SetBinding(Border.VisibilityProperty, new Binding("PortDefaultValueMarkerVisible")
            {
                Converter = new BooleanToVisibilityConverter()
            });

            Grid.SetColumn(PortDefaultValueMarker, 0);

            var PortNameTextBox = new TextBlock()
            {
                Name = "PortNameTextBox",
                Width = Double.NaN,
                Margin = new Thickness(0, 1, 5, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = artifactElementReg,
                FontSize = 12,
                Foreground = primaryCharcoal200Brush,
                IsHitTestVisible = false,
            };

            PortNameTextBox.SetBinding(TextBlock.TextProperty, new Binding("PortName"));

            Grid.SetColumn(PortNameTextBox, 3);

            var Chevron = new TextBlock()
            {
                Name = "Chevron",
                Width = 20,
                Padding = new Thickness(0, 1, 1, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 16,
                Foreground = primaryCharcoal200Brush,
                IsHitTestVisible = false,
                Text = "&gt;",
                TextAlignment = TextAlignment.Center
            };

            Chevron.SetBinding(TextBlock.VisibilityProperty, new Binding("UseLevelVisibility"));

            //TODO WIP

            Grid.SetColumn(Chevron, 6);

            MainGrid.Children.Add(PortSnapping);
            MainGrid.Children.Add(PortBackgroundBorder);
            MainGrid.Children.Add(PortValueMarker);
            MainGrid.Children.Add(PortDefaultValueMarker);
            MainGrid.Children.Add(PortNameTextBox);

            this.Content = MainGrid;

        }
    }
}
