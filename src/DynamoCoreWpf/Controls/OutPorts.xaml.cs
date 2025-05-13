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
        private Border nodeAutoCompleteMarker = null;
        private Grid NodeAutoCompleteHover = null;

        private static BoolToVisibilityCollapsedConverter boolToVisibilityCollapsedConverter = new BoolToVisibilityCollapsedConverter();
        private static FontFamily artifactElementReg = SharedDictionaryManager.DynamoModernDictionary["ArtifaktElementRegular"] as FontFamily;
        private static SolidColorBrush primaryCharcoal200Brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DCDCDC"));
        private static SolidColorBrush midGrey = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666"));
        private static SolidColorBrush nodeTransientOverlayColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D5BCF7"));
        private static SolidColorBrush portMouseOverColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#636363")); //This is the equivolent direct color vs with opacity
        private static SolidColorBrush portValueMarkerColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#999999"));

        //Hold the instance color for the port Backroudn Color.  This is so it can be set differently for CodeBlock
        private SolidColorBrush portBackGroundColor = PortViewModel.PortBackgroundColorDefault;

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
                IsHitTestVisible = true,
                SnapsToDevicePixels = true,
                Background = portBackGroundColor,
                BorderBrush = PortViewModel.PortBorderBrushColorDefault
            };

            Grid.SetColumn(PortBackgroundBorder, 0);
            Grid.SetColumnSpan(PortBackgroundBorder, 2);

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

            //Unclear if we ever disable ports
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

            DynamoToolTip dynamoToolTip = new DynamoToolTip
            {
                AttachmentSide = DynamoToolTip.Side.Top,
                OverridesDefaultStyle = true,
                HasDropShadow = false,
                Style = NodeView.DynamoToolTipTopStyle
            };

            TextBlock textBlock = new TextBlock
            {
                MaxWidth = 320,
                TextWrapping = TextWrapping.Wrap
            };

            textBlock.SetBinding(TextBlock.TextProperty, new Binding("ToolTipContent"));
            dynamoToolTip.Content = textBlock;
            PortBackgroundBorder.ToolTip = dynamoToolTip;

            NodeAutoCompleteHover = new Grid()
            {
                Name = "NodeAutoCompleteHover",
                Margin = new Thickness(0, 0, -18, 0),
                Background = Brushes.Transparent
            };

            Grid.SetColumn(NodeAutoCompleteHover, 2);

            nodeAutoCompleteMarker = new Border
            {
                Name = "NodeAutoCompleteMarker",
                Cursor = Cursors.Hand,
                CornerRadius = new CornerRadius(10),
                Height = 20,
                Width = 20,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right,
                Background = nodeTransientOverlayColor,
                SnapsToDevicePixels = true,
                Visibility = Visibility.Collapsed
            };

            var nodeAutoCompleteMarkerLabel = new Label
            {
                Name = "NodeAutoCompleteMarkerLabel",
                FontSize = 12,
                Width = 25,
                Height = 25,
                Margin = new Thickness(-3, -3, 0, 0),
                Content = "✨"
            };

            nodeAutoCompleteMarker.Child = nodeAutoCompleteMarkerLabel; 
            NodeAutoCompleteHover.Children.Add(nodeAutoCompleteMarker);

            //TODO Finish NodeAutoCompletHover Tooltip

            //TODO Finish PortBoderHighlight move it to changeing the PortBackgroundBorder

            MainGrid.Children.Add(PortSnapping);
            MainGrid.Children.Add(PortBackgroundBorder);
            MainGrid.Children.Add(PortValueMarker);
            MainGrid.Children.Add(PortNameGrid);
            MainGrid.Children.Add(NodeAutoCompleteHover);

            this.Content = MainGrid;

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

            var previewMouseLeftDownTrigger = new Dynamo.UI.Views.HandlingEventTrigger()
            {
                EventName = "PreviewMouseLeftButtonDown",
            };
            var previewMouseLeftDownAction = new InvokeCommandAction()
            {
                Command = viewModel.NodeAutoCompleteCommand,
                PassEventArgsToCommand = true
            };

            previewMouseLeftDownTrigger.Actions.Add(previewMouseLeftDownAction);
            Dynamo.Microsoft.Xaml.Behaviors.Interaction.GetTriggers(nodeAutoCompleteMarker).Add(previewMouseLeftDownTrigger);

            //todo dispatch on UI thead
            //Todo move to method so can unregister
            PortBackgroundBorder.MouseEnter += (s, e) =>
            {
                PortBackgroundBorder.Background = portMouseOverColor;
                if(viewModel.NodeAutoCompleteMarkerEnabled)
                    nodeAutoCompleteMarker.Visibility = Visibility.Visible;
            };
            PortBackgroundBorder.MouseLeave += (s, e) =>
            {
                PortBackgroundBorder.Background = portBackGroundColor;
                nodeAutoCompleteMarker.Visibility = Visibility.Collapsed;
            };

            NodeAutoCompleteHover.MouseEnter += (s, e) =>
            {
                if (viewModel.NodeAutoCompleteMarkerEnabled)
                    nodeAutoCompleteMarker.Visibility = Visibility.Visible;
            };
            NodeAutoCompleteHover.MouseLeave += (s, e) =>
            {
                nodeAutoCompleteMarker.Visibility = Visibility.Collapsed;
            };

            if (viewModel.IsPortCondensed)
            {
                MainGrid.Height = 14;
                MainGrid.Margin = new Thickness(0,3,0,0);

                PortBackgroundBorder.CornerRadius = new CornerRadius(0);
                PortBackgroundBorder.BorderThickness = new Thickness(0);
                PortBackgroundBorder.Height = 14;
                PortBackgroundBorder.Width = 20;
                PortBackgroundBorder.Background = midGrey;
                portBackGroundColor = midGrey;
                PortBackgroundBorder.BorderBrush = Brushes.Transparent;
                PortNameTextBox.Margin = new Thickness(12,1,0,0);
                PortNameGrid.Height = 14;
                PortNameGrid.Margin = new Thickness(0, 1, 2, 0);
            }

        }
    }
}
