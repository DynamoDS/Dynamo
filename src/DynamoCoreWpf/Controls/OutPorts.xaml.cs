using Dynamo.Configuration;
using Dynamo.Controls;
using Dynamo.Microsoft.Xaml.Behaviors;
using Dynamo.ViewModels;
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
    /// Interaction logic for OutPorts.xaml
    /// </summary>
    public partial class OutPorts : UserControl
    {
        private OutPortViewModel viewModel = null;
        private Grid MainGrid = null;
        private Rectangle PortSnapping = null;
        private Border PortBackgroundBorder = null;
        private Grid PortNameGrid = null;
        private TextBlock PortNameTextBox = null;
        private Border BorderHighlightOverlay = null;

        private static BoolToVisibilityCollapsedConverter boolToVisibilityCollapsedConverter = new BoolToVisibilityCollapsedConverter();
        private static FontFamily artifactElementReg = SharedDictionaryManager.DynamoModernDictionary["ArtifaktElementRegular"] as FontFamily;
        private static SolidColorBrush primaryCharcoal200Brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DCDCDC"));
        private static SolidColorBrush midGrey = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666"));

        public OutPorts()
        {
            InitializeComponent();

            this.MainGrid = new Grid()
            {
                Name = "MainGrid",
                Height = 34,
                Background = Brushes.Transparent,
                HorizontalAlignment = HorizontalAlignment.Right,
                IsHitTestVisible = true,
            };

            MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Name = "PortNameColumn", Width = new GridLength(1, GridUnitType.Star) });
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Name = "ValueMarkerColumn", Width = new GridLength(5) });
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Name = "PortSnappingColumn", Width = new GridLength(25) });

            //TODO IsPortCondensed setting in OnDataContextChanged

            PortSnapping = new Rectangle()
            {
                Name = "PortSnapping",
                Margin = new Thickness(0, 0, -25, 0),
                Fill = Brushes.Transparent,
                SnapsToDevicePixels = true,
            };

            Grid.SetColumn(PortSnapping, 0);
            Grid.SetColumnSpan(PortSnapping, 3);
            Canvas.SetZIndex(PortSnapping, 1);

            PortSnapping.SetBinding(Rectangle.IsHitTestVisibleProperty, new Binding("IsHitTestVisible"));

            PortBackgroundBorder = new Border()
            {
                Name = "PortBackgroundBorder",
                Height = 29,
                BorderThickness = new Thickness(1, 1, 0, 1),
                CornerRadius = new CornerRadius(11, 0, 0, 11),
                IsHitTestVisible = false,
                SnapsToDevicePixels = true
            };

            PortBackgroundBorder.SetBinding(Border.BackgroundProperty, new Binding("PortBackgroundColor") { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            PortBackgroundBorder.SetBinding(Border.BorderBrushProperty, new Binding("PortBorderBrushColor") { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged }); //DO we need this?

            Grid.SetColumn(PortBackgroundBorder, 0);
            Grid.SetColumnSpan(PortBackgroundBorder, 2);
            //TODO IsPortCondensed setting in OnDataContextChanged for PortBackgroundBorder

            var portValueMarkerColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#999999"));

            var PortValueMarker = new Rectangle()
            {
                Name = "PortValueMarker",
                Height = 27,
                Width = 5,
                VerticalAlignment = VerticalAlignment.Center,
                IsHitTestVisible = false,
                SnapsToDevicePixels = true,
                Fill = portValueMarkerColor
            };

            PortValueMarker.SetBinding(Rectangle.VisibilityProperty, new Binding("PortDefaultValueMarkerVisible")
            {
                Converter = boolToVisibilityCollapsedConverter
            });

            Grid.SetColumn(PortValueMarker, 1);

            this.PortNameGrid = new Grid()
            {
                Name = "PortNameGrid",
                Height = 34,
                VerticalAlignment = VerticalAlignment.Center,
                IsHitTestVisible = true,
            };

            Grid.SetColumn(PortNameGrid, 0);

            PortNameGrid.SetBinding(TextBlock.IsEnabledProperty, new Binding("IsEnabled"));

            this.PortNameTextBox = new TextBlock
            {
                Name = "PortNameTextBox",
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = artifactElementReg,
                FontSize = 12,
                Foreground = primaryCharcoal200Brush,
                IsHitTestVisible = false,
                Margin = new Thickness(13, 3, 10, 0)
            };

            PortNameTextBox.SetBinding(TextBlock.TextProperty, new Binding("PortName"));
            PortNameGrid.Children.Add(PortNameTextBox);
            //TODO IsPortCondensed setting in OnDataContextChanged for PortNameGrid & PortNameTextBox

            BorderHighlightOverlay = new Border()
            {
                Name = "BorderHighlightOverlay",
                BorderBrush = Brushes.Transparent,
                Opacity = 0.2,
                SnapsToDevicePixels = true,
                Height = 29,
                CornerRadius = new CornerRadius(11, 0, 0, 11),
                BorderThickness = new Thickness(1, 1, 0, 1),
                Background = Brushes.Transparent
            };

            Grid.SetColumn(BorderHighlightOverlay, 0);
            Grid.SetColumnSpan(BorderHighlightOverlay, 2);

            BorderHighlightOverlay.MouseEnter += (s, e) =>
            {
                PortValueMarker.Fill = Brushes.White;
                BorderHighlightOverlay.Background = Brushes.White;
            };

            BorderHighlightOverlay.MouseLeave += (s, e) =>
            {
                PortValueMarker.Fill = portValueMarkerColor;
                BorderHighlightOverlay.Background = Brushes.Transparent;
            };

            DynamoToolTip dynamoToolTip = new DynamoToolTip
            {
                AttachmentSide = DynamoToolTip.Side.Top,
                OverridesDefaultStyle = true,
                HasDropShadow = false,
                Style = Dynamo.Controls.NodeView.DynamoToolTipTopStyle
            };

            TextBlock textBlock = new TextBlock
            {
                MaxWidth = 320,
                TextWrapping = TextWrapping.Wrap
            };

            textBlock.SetBinding(TextBlock.TextProperty, new Binding("ToolTipContent"));
            dynamoToolTip.Content = textBlock;
            BorderHighlightOverlay.ToolTip = dynamoToolTip;

            var NodeAutoCompleteHover = new Border()
            {
                Name = "NodeAutoCompleteHover",
                Margin = new Thickness(0, 0, -18, 0),
                Background = Brushes.Transparent
            };

            //TODO Finish NodeAutoCompletHover

            //TODO Finish PortBoderHighlight

            Grid.SetColumn(NodeAutoCompleteHover, 2);

            MainGrid.Children.Add(PortSnapping);
            MainGrid.Children.Add(PortBackgroundBorder);
            MainGrid.Children.Add(PortValueMarker);
            MainGrid.Children.Add(PortNameGrid);
            MainGrid.Children.Add(BorderHighlightOverlay);
            MainGrid.Children.Add(NodeAutoCompleteHover);

            this.Content = MainGrid;

            //TODO unregister
            DataContextChanged += OnDataContextChanged;

        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (null != viewModel) return;

            viewModel = e.NewValue as OutPortViewModel;

            var mouseLeftButtonDownTrigger = new Dynamo.UI.Views.HandlingEventTrigger()
            {
                EventName = "MouseLeftButtonDown",
            };
            var mouseLeftButtonDownAction = new InvokeCommandAction()
            {
                Command = viewModel.ConnectCommand,
            };

            mouseLeftButtonDownTrigger.Actions.Add(mouseLeftButtonDownAction);
            Dynamo.Microsoft.Xaml.Behaviors.Interaction.GetTriggers(MainGrid).Add(mouseLeftButtonDownTrigger);

            var mouseRightButtonDownTrigger = new Dynamo.UI.Views.HandlingEventTrigger()
            {
                EventName = "MouseRightButtonDown",
            };
            var mouseRightButtonDownAction = new InvokeCommandAction()
            {
                Command = viewModel.NodePortContextMenuCommand,
                CommandParameter = viewModel
            };

            mouseRightButtonDownTrigger.Actions.Add(mouseRightButtonDownAction);
            Dynamo.Microsoft.Xaml.Behaviors.Interaction.GetTriggers(MainGrid).Add(mouseRightButtonDownTrigger);

            var mouseEnterTrigger = new Dynamo.UI.Views.HandlingEventTrigger()
            {
                EventName = "MouseEnter",
            };
            var mouseEnterAction = new InvokeCommandAction()
            {
                Command = viewModel.MouseEnterCommand,
                CommandParameter = viewModel
            };

            mouseEnterTrigger.Actions.Add(mouseEnterAction);
            Dynamo.Microsoft.Xaml.Behaviors.Interaction.GetTriggers(PortSnapping).Add(mouseEnterTrigger);

            var mouseLeaveTrigger = new Dynamo.UI.Views.HandlingEventTrigger()
            {
                EventName = "MouseLeave",
            };
            var mouseLeaveAction = new InvokeCommandAction()
            {
                Command = viewModel.MouseLeaveCommand,
                CommandParameter = viewModel
            };

            mouseLeaveTrigger.Actions.Add(mouseLeaveAction);
            Dynamo.Microsoft.Xaml.Behaviors.Interaction.GetTriggers(PortSnapping).Add(mouseLeaveTrigger);

            if (viewModel.IsPortCondensed)
            {
                MainGrid.Height = 14;
                MainGrid.Margin = new Thickness(0,3,0,0);

                PortBackgroundBorder.CornerRadius = new CornerRadius(0);
                PortBackgroundBorder.BorderThickness = new Thickness(0);
                PortBackgroundBorder.Height = 14;
                PortBackgroundBorder.Width = 20;
                PortBackgroundBorder.Background = midGrey;
                PortBackgroundBorder.BorderBrush = Brushes.Transparent;
                PortNameTextBox.Margin = new Thickness(12,1,0,0);
                PortNameGrid.Height = 14;
                PortNameGrid.Margin = new Thickness(0, 1, 2, 0);
                BorderHighlightOverlay.CornerRadius = new CornerRadius(0);
                BorderHighlightOverlay.BorderThickness = new Thickness(0);
                BorderHighlightOverlay.Height = Configurations.CodeBlockOutputPortHeightInPixels;
                BorderHighlightOverlay.Width = 20;
                BorderHighlightOverlay.Margin = new Thickness(5,0,0,0);
            }

        }
    }
}
