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
    /// Interaction logic for InPorts.xaml
    /// </summary>
    public partial class InPorts : UserControl
    {
        private InPortViewModel viewModel = null;
        private Grid MainGrid = null;
        private Border PortBackgroundBorder = null;
        private Grid NodeAutoCompleteHover = null;
        private Border nodeAutoCompleteMarker = null;
        private bool UseLevelSpinnerInit = false;

        private static SolidColorBrush primaryCharcoal200Brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DCDCDC"));
        private static SolidColorBrush chevronHighlightOverlayBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5E2DE"));
        private static SolidColorBrush portMouseOverColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#636363")); //This is the equivolent direct color vs with opacity
        private static SolidColorBrush nodeTransientOverlayColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D5BCF7"));

        private static BooleanToVisibilityConverter booleanToVisibilityConverter = new BooleanToVisibilityConverter();
        private static FontFamily artifactElementReg = SharedDictionaryManager.DynamoModernDictionary["ArtifaktElementRegular"] as FontFamily;

        //Try to unify the Port Borders
        private Brush cachedPortBackgroundColor = PortViewModel.PortBackgroundColorDefault;

        public InPorts()
        {
            InitializeComponent();

            this.MainGrid = new Grid()
            {
                Name = "MainGrid",
                Height = 34,
                Background = Brushes.Transparent,
                IsHitTestVisible = true
            };

            MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Name = "PortSnappingColumn", Width = new GridLength(25) });
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Name = "ValueMarkerColumn", Width = new GridLength(5) });
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Name = "GapBetweenValueMarkerAndPortName", Width = new GridLength(6) });
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Name = "PortNameColumn", Width = new GridLength(1, GridUnitType.Star) });
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Name = "GapBetweenPortNameAndUseLevelSpinner", Width = new GridLength(6) });
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Name = "UseLevelSpinnerColumn", Width = new GridLength(0) });
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Name = "ChevronColumn", Width = new GridLength(0) });

            var PortSnapping = new Rectangle()
            {
                Name = "PortSnapping",
                Fill = Brushes.Transparent,
                SnapsToDevicePixels = true,
            };

            Grid.SetColumn(PortSnapping, 0);
            Grid.SetColumnSpan(PortSnapping, 7);
            Canvas.SetZIndex(PortSnapping, 7);

            PortSnapping.SetBinding(Rectangle.IsHitTestVisibleProperty, new Binding("IsHitTestVisible"));

            //TODO change to trigger handler?
            //TODO deregister event handler
            PortSnapping.MouseEnter += (s, e) => viewModel.MouseEnterCommand.Execute(DataContext);
            PortSnapping.MouseLeave += (s, e) => viewModel.MouseLeaveCommand.Execute(DataContext);

            PortBackgroundBorder = new Border()
            {
                Name = "PortBackgroundBorder",
                Height = 29,
                BorderThickness = new Thickness(0, 1, 1, 1),
                CornerRadius = new CornerRadius(0, 11, 11, 0),
                IsHitTestVisible = true,
                SnapsToDevicePixels = true,
            };

            // Bind BorderBrush property
            // Might need to move this to the property handler in OnDataContextChanged
            PortBackgroundBorder.SetBinding(Border.BorderBrushProperty, new Binding("PortBorderBrushColor")
            {
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

            Grid.SetColumn(PortBackgroundBorder, 1);
            Grid.SetColumnSpan(PortBackgroundBorder, 6);

            var PortValueMarker = new Rectangle()
            {
                Name = "PortValueMarker",
                Height = 29,
                Width = 5,
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
                Margin = new Thickness(0, 0, 1, 1),
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
            PortBackgroundBorder.ToolTip = dynamoToolTip;

            NodeAutoCompleteHover = new Grid()
            {
                Name = "NodeAutoCompleteHover",
                Margin = new Thickness(-18, 0, 0, 0),
                Background = Brushes.Transparent
            };

            Grid.SetColumn(NodeAutoCompleteHover, 0);

            nodeAutoCompleteMarker = new Border
            {
                Name = "NodeAutoCompleteMarker",
                Cursor = Cursors.Hand,
                CornerRadius = new CornerRadius(10),
                Height = 20,
                Width = 20,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
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

            MainGrid.Children.Add(PortSnapping);
            MainGrid.Children.Add(PortBackgroundBorder);
            MainGrid.Children.Add(PortValueMarker);
            MainGrid.Children.Add(PortDefaultValueMarker);
            MainGrid.Children.Add(PortNameTextBox);
            MainGrid.Children.Add(NodeAutoCompleteHover);

            this.Content = MainGrid;

            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (null != viewModel) return;

            viewModel = e.NewValue as InPortViewModel;

            MainGrid.BeginInit();

            //Intialize the PortBackgroundColor
            PortBackgroundBorder.Background = viewModel.PortBackgroundColor;

            //Todo deregister event
            viewModel.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case "PortBackgroundColor":
                        if (PortBackgroundBorder.Dispatcher.CheckAccess())
                        {
                            PortBackgroundBorder.Background = viewModel.PortBackgroundColor;
                        }
                        else
                        {
                            PortBackgroundBorder.Dispatcher.Invoke(() =>
                            {
                                PortBackgroundBorder.Background = viewModel.PortBackgroundColor;
                            });
                        }
                        break;
                    case "Highlight":
                        if (viewModel.Highlight == Visibility.Visible)
                        {
                            if (PortBackgroundBorder.Dispatcher.CheckAccess())
                            {
                                PortBackgroundBorder.BorderBrush = nodeTransientOverlayColor;
                                PortBackgroundBorder.BorderThickness = new Thickness(3, 3, 3, 3);
                            }
                            else
                            {
                                PortBackgroundBorder.Dispatcher.Invoke(() =>
                                {
                                    PortBackgroundBorder.BorderBrush = nodeTransientOverlayColor;
                                    PortBackgroundBorder.BorderThickness = new Thickness(3, 3, 3, 3);
                                });
                            }
                        }
                        else
                        {
                            if (PortBackgroundBorder.Dispatcher.CheckAccess())
                            {
                                PortBackgroundBorder.BorderBrush = viewModel.PortBorderBrushColor;
                                PortBackgroundBorder.BorderThickness = new Thickness(1, 1, 1, 1);
                            }
                            else
                            {
                                PortBackgroundBorder.Dispatcher.Invoke(() =>
                                {
                                    PortBackgroundBorder.BorderBrush = viewModel.PortBorderBrushColor;
                                    PortBackgroundBorder.BorderThickness = new Thickness(1, 1, 1, 1);
                                });
                            }
                        }
                        break;
                    case "UseLevels":

                        //Add the spinner if it was not added in the constructor
                        if (viewModel.UseLevels == true)
                        {
                            if (!UseLevelSpinnerInit)
                            {
                                var useLevelControl = new UseLevelSpinner()
                                {
                                    Name = "useLevelControl",
                                    Width = 50,
                                    Height = 25,
                                    VerticalAlignment = VerticalAlignment.Center,
                                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4A4A4A")), //DarkGrey
                                };

                                DockPanel.SetDock(useLevelControl, Dock.Right);
                                Grid.SetColumn(useLevelControl, 5);

                                useLevelControl.SetBinding(UseLevelSpinner.KeepListStructureProperty, new Binding("ShouldKeepListStructure"));
                                useLevelControl.SetBinding(UseLevelSpinner.LevelProperty, new Binding("Level") { Mode = BindingMode.TwoWay });
                                useLevelControl.SetBinding(Border.VisibilityProperty, new Binding("UseLevels")
                                {
                                    Converter = booleanToVisibilityConverter
                                });

                                if (MainGrid.Dispatcher.CheckAccess())
                                {
                                    MainGrid.Children.Add(useLevelControl);
                                }
                                else
                                {
                                    MainGrid.Dispatcher.Invoke(() =>
                                    {
                                        MainGrid.Children.Add(useLevelControl);
                                    });
                                }
                            }
                            
                        }
                        break;
                }
            };

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
            // Event handlers for mouse enter and leave
            PortBackgroundBorder.MouseEnter += (s, e) =>
            {
                //Todo add check for KeepListStructure
                PortBackgroundBorder.Background = portMouseOverColor;
                if (viewModel.NodeAutoCompleteMarkerEnabled)
                    nodeAutoCompleteMarker.Visibility = Visibility.Visible;
            };
            PortBackgroundBorder.MouseLeave += (s, e) =>
            {
                PortBackgroundBorder.Background = viewModel.PortBackgroundColor;
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

            if (viewModel.UseLevelVisibility == Visibility.Visible)
            {
                var chevron = new TextBlock()
                {
                    Name = "Chevron",
                    Width = 20,
                    Padding = new Thickness(0, 1, 1, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 16,
                    Foreground = primaryCharcoal200Brush,
                    IsHitTestVisible = false,
                    Text = ">",
                    TextAlignment = TextAlignment.Center
                };

                Grid.SetColumn(chevron, 6);

                // Create the Border
                Border chevronHighlightOverlay = new Border
                {
                    Name = "ChevronHighlightOverlay",
                    Width = 20,
                    Height = 28,
                    CornerRadius = new CornerRadius(0, 11, 11, 0),
                    IsHitTestVisible = true,
                    Background = chevronHighlightOverlayBackground,
                    Opacity = 0.0 // Initial opacity
                };

                Grid.SetColumn(chevronHighlightOverlay, 6);

                // InputBindings for MouseBinding
                MouseBinding mouseBinding = new MouseBinding
                {
                    Command = viewModel.NodePortContextMenuCommand,
                    MouseAction = MouseAction.LeftClick
                };
                chevronHighlightOverlay.InputBindings.Add(mouseBinding);

                chevronHighlightOverlay.MouseEnter += (s, e) => chevronHighlightOverlay.Opacity = 0.3;
                chevronHighlightOverlay.MouseLeave += (s, e) => chevronHighlightOverlay.Opacity = 0.0;

                MainGrid.ColumnDefinitions[6].Width = new GridLength(20);
                MainGrid.Children.Add(chevron);
                MainGrid.Children.Add(chevronHighlightOverlay);
                MainGrid.ColumnDefinitions[5].Width = new GridLength(50);

                // Add the UseLevelSpinner if visable in the nodes initial state
                if (viewModel.UseLevels == true)
                { 
                    var useLevelControl = new UseLevelSpinner()
                    {
                        Name = "useLevelControl",
                        Width = 50,
                        Height = 25,
                        VerticalAlignment = VerticalAlignment.Center,
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4A4A4A")), //DarkGrey
                    };

                    DockPanel.SetDock(useLevelControl, Dock.Right);
                    Grid.SetColumn(useLevelControl, 5);

                    useLevelControl.SetBinding(UseLevelSpinner.KeepListStructureProperty, new Binding("ShouldKeepListStructure"));
                    useLevelControl.SetBinding(UseLevelSpinner.LevelProperty, new Binding("Level") { Mode = BindingMode.TwoWay });
                    useLevelControl.SetBinding(Border.VisibilityProperty, new Binding("UseLevels")
                    {
                        Converter = booleanToVisibilityConverter
                    });

                    MainGrid.Children.Add(useLevelControl);
                }
            }

            MainGrid.EndInit();
        }
    }
}
