using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Dynamo.Controls;
using Dynamo.Microsoft.Xaml.Behaviors;
using Dynamo.ViewModels;
using Dynamo.Views;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// Interaction logic for InPorts.xaml
    /// </summary>
    public partial class InPorts : UserControl
    {
        private InPortViewModel viewModel = null;

        //View components that are accessed between constructor and OnDataContextChanged
        private Grid MainGrid = null;
        private Border PortBackgroundBorder = null;
        private Grid NodeAutoCompleteHover = null;
        private Border nodeAutoCompleteMarker = null;
        private Rectangle PortSnapping = null;
        private Border chevronHighlightOverlay = null;

        private bool _useLevelSpinnerInit = false;

        // Static resources mostly from DynamoModern themes but some from DynamoColorsAndBrushes.xaml
        private static SolidColorBrush _primaryCharcoal200Brush = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["PrimaryCharcoal200Brush"] as SolidColorBrush;
        private static SolidColorBrush _chevronHighlightOverlayBackground = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["ChevronHighlightOverlayBackground"] as SolidColorBrush;
        private static SolidColorBrush _portMouseOverColor = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["PortMouseOverColor"] as SolidColorBrush;
        private static SolidColorBrush _nodeTransientOverlayColor = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["NodeTransientOverlayColor"] as SolidColorBrush;
        private static SolidColorBrush _darkGrey = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["DarkGreyBrush"] as SolidColorBrush;

        private static BooleanToVisibilityConverter _booleanToVisibilityConverter = new BooleanToVisibilityConverter();
        private static FontFamily _artifactElementReg = SharedDictionaryManager.DynamoModernDictionary["ArtifaktElementRegular"] as FontFamily;
        private static readonly ZoomToInverseVisibilityCollapsedConverter _zoomToInverseVisibilityCollapsedConverter = new ZoomToInverseVisibilityCollapsedConverter();

        static InPorts()
        {
            _primaryCharcoal200Brush.Freeze();
            _chevronHighlightOverlayBackground.Freeze();
            _portMouseOverColor.Freeze();
            _nodeTransientOverlayColor.Freeze();
            _darkGrey.Freeze();
        }
        public InPorts()
        {
            InitializeComponent();

            this.MainGrid = new Grid()
            {
                Name = "MainGrid",
                Height = 34,
                Background = Brushes.Transparent,
                IsHitTestVisible = true,
            };

            MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Name = "PortSnappingColumn", Width = new GridLength(25) });
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Name = "ValueMarkerColumn", Width = new GridLength(5) });
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Name = "GapBetweenValueMarkerAndPortName", Width = new GridLength(6) });
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Name = "PortNameColumn", Width = new GridLength(1, GridUnitType.Star) });
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Name = "GapBetweenPortNameAndUseLevelSpinner", Width = new GridLength(6) });
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Name = "UseLevelSpinnerColumn", Width = new GridLength(0) });
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Name = "ChevronColumn", Width = new GridLength(0) });
            MainGrid.ContextMenuOpening += (s, e) =>
            {
                e.Handled = true; // Suppress the default context menu
            };

            PortSnapping = new Rectangle()
            {
                Name = "PortSnapping",
                Fill = Brushes.Transparent,
                SnapsToDevicePixels = true,
            };

            Grid.SetColumn(PortSnapping, 0);
            Grid.SetColumnSpan(PortSnapping, 7);
            Canvas.SetZIndex(PortSnapping, 7);

            PortSnapping.SetBinding(Rectangle.IsHitTestVisibleProperty, new Binding("IsHitTestVisible"));

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
                FontFamily = _artifactElementReg,
                FontSize = 12,
                Foreground = _primaryCharcoal200Brush,
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
                Background = _nodeTransientOverlayColor,
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
            NodeAutoCompleteHover.SetBinding(UIElement.VisibilityProperty, new Binding("DataContext.Zoom")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(WorkspaceView), 1),
                Converter = _zoomToInverseVisibilityCollapsedConverter
            });

            MainGrid.Children.Add(PortSnapping);
            MainGrid.Children.Add(PortBackgroundBorder);
            MainGrid.Children.Add(PortValueMarker);
            MainGrid.Children.Add(PortDefaultValueMarker);
            MainGrid.Children.Add(PortNameTextBox);
            MainGrid.Children.Add(NodeAutoCompleteHover);

            this.Content = MainGrid;

            DataContextChanged += OnDataContextChanged;
            Loaded += OnPortViewLoaded;
            Unloaded += OnPortViewUnloaded;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (null != viewModel) return;

            // Clean up old triggers if they exist
            if (e.OldValue != null)
            {
                CleanupUITriggers();
            }

            viewModel = e.NewValue as InPortViewModel;
            viewModel.PropertyChanged += OnPropertyChanged;

            //Initialize the PortBackgroundColor
            PortBackgroundBorder.Background = viewModel.PortBackgroundColor;

            var mouseLeftButtonDownTrigger = new Dynamo.UI.Views.HandlingEventTrigger()
            {
                EventName = "MouseLeftButtonDown",
            };
            var mouseLeftButtonDownAction = new InvokeCommandAction()
            {
                Command = viewModel.ConnectCommand,
                PassEventArgsToCommand = true
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

            if (viewModel.UseLevelVisibility == Visibility.Visible)
            {
                SetLevelVisibility();
            }
        }

        private void SetLevelVisibility()
        {
            var chevron = new TextBlock()
            {
                Name = "Chevron",
                Width = 20,
                Padding = new Thickness(0, 1, 1, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 16,
                Foreground = _primaryCharcoal200Brush,
                IsHitTestVisible = false,
                Text = ">",
                TextAlignment = TextAlignment.Center
            };

            Grid.SetColumn(chevron, 6);

            // Create the Border
            chevronHighlightOverlay = new Border
            {
                Name = "ChevronHighlightOverlay",
                Width = 20,
                Height = 28,
                CornerRadius = new CornerRadius(0, 11, 11, 0),
                IsHitTestVisible = true,
                Background = _chevronHighlightOverlayBackground,
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

            chevronHighlightOverlay.MouseEnter += OnMouseEnterChevron;
            chevronHighlightOverlay.MouseLeave += OnMouseLeaveChevron;

            MainGrid.ColumnDefinitions[6].Width = new GridLength(20);
            MainGrid.Children.Add(chevron);
            MainGrid.Children.Add(chevronHighlightOverlay);
            MainGrid.ColumnDefinitions[5].Width = new GridLength(50);

            // Add the UseLevelSpinner if visible in the nodes initial state
            if (viewModel.UseLevels == true)
            {
                var useLevelControl = new UseLevelSpinner()
                {
                    Name = "useLevelControl",
                    Width = 50,
                    Height = 25,
                    VerticalAlignment = VerticalAlignment.Center,
                    Background = _darkGrey
                };

                DockPanel.SetDock(useLevelControl, Dock.Right);
                Grid.SetColumn(useLevelControl, 5);

                useLevelControl.SetBinding(UseLevelSpinner.KeepListStructureProperty, new Binding("ShouldKeepListStructure"));
                useLevelControl.SetBinding(UseLevelSpinner.LevelProperty, new Binding("Level") { Mode = BindingMode.TwoWay });
                useLevelControl.SetBinding(Border.VisibilityProperty, new Binding("UseLevels")
                {
                    Converter = _booleanToVisibilityConverter
                });

                MainGrid.Children.Add(useLevelControl);
                _useLevelSpinnerInit = true;
            }
        }

        private void CleanupTriggersFromElement(FrameworkElement element)
        {
            if (element != null)
            {
                var triggers = Dynamo.Microsoft.Xaml.Behaviors.Interaction.GetTriggers(element);
                if (triggers != null && triggers.Count > 0)
                {
                    triggers.Clear();
                }
            }
        }
        private void CleanupUITriggers()
        {
            CleanupTriggersFromElement(MainGrid);
            CleanupTriggersFromElement(nodeAutoCompleteMarker);
            CleanupTriggersFromElement(PortSnapping);
        }


        private void OnMouseEnterSnapping(object sender, MouseEventArgs args)
        {
            viewModel.MouseEnterCommand.Execute(DataContext);
        }

        private void OnMouseLeaveSnapping(object sender, MouseEventArgs args)
        {
            viewModel.MouseLeaveCommand.Execute(DataContext);
        }

        //todo validate if these need to checked and dispatched on UI thread
        private void OnMouseEnterBackground(object sender, MouseEventArgs args)
        {
            //Todo add check for KeepListStructure
            PortBackgroundBorder.Background = _portMouseOverColor;
            if (viewModel.NodeAutoCompleteMarkerEnabled)
                nodeAutoCompleteMarker.Visibility = Visibility.Visible;
        }

        private void OnMouseLeaveBackground(object sender, MouseEventArgs args)
        {
            PortBackgroundBorder.Background = viewModel.PortBackgroundColor;
            nodeAutoCompleteMarker.Visibility = Visibility.Collapsed;
        }

        private void OnMouseEnterHover(object sender, MouseEventArgs args)
        {
            if (viewModel.NodeAutoCompleteMarkerEnabled)
                nodeAutoCompleteMarker.Visibility = Visibility.Visible;
        }

        private void OnMouseLeaveHover(object sender, MouseEventArgs args)
        {
            nodeAutoCompleteMarker.Visibility = Visibility.Collapsed;
        }

        private void OnMouseEnterChevron(object sender, MouseEventArgs args)
        {
            chevronHighlightOverlay.Opacity = 0.3;
        }

        private void OnMouseLeaveChevron(object sender, MouseEventArgs args)
        {
            chevronHighlightOverlay.Opacity = 0.0;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
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
                            PortBackgroundBorder.BorderBrush = _nodeTransientOverlayColor;
                            PortBackgroundBorder.BorderThickness = new Thickness(3, 3, 3, 3);
                        }
                        else
                        {
                            PortBackgroundBorder.Dispatcher.Invoke(() =>
                            {
                                PortBackgroundBorder.BorderBrush = _nodeTransientOverlayColor;
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
                        if (!_useLevelSpinnerInit)
                        {
                            var useLevelControl = new UseLevelSpinner()
                            {
                                Name = "useLevelControl",
                                Width = 50,
                                Height = 25,
                                VerticalAlignment = VerticalAlignment.Center,
                                Background = _darkGrey
                            };

                            DockPanel.SetDock(useLevelControl, Dock.Right);
                            Grid.SetColumn(useLevelControl, 5);

                            useLevelControl.SetBinding(UseLevelSpinner.KeepListStructureProperty, new Binding("ShouldKeepListStructure"));
                            useLevelControl.SetBinding(UseLevelSpinner.LevelProperty, new Binding("Level") { Mode = BindingMode.TwoWay });
                            useLevelControl.SetBinding(Border.VisibilityProperty, new Binding("UseLevels")
                            {
                                Converter = _booleanToVisibilityConverter
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
        }

        private void OnPortViewUnloaded(object sender, RoutedEventArgs e)
        {
            viewModel.PropertyChanged -= OnPropertyChanged;

            CleanupUITriggers();

            PortSnapping.MouseEnter -= OnMouseEnterSnapping;
            PortSnapping.MouseLeave -= OnMouseLeaveSnapping;
            PortBackgroundBorder.MouseEnter -= OnMouseEnterBackground;
            PortBackgroundBorder.MouseLeave -= OnMouseLeaveBackground;
            NodeAutoCompleteHover.MouseEnter -= OnMouseEnterHover;
            NodeAutoCompleteHover.MouseLeave -= OnMouseLeaveHover;

            if (chevronHighlightOverlay != null)
            {
                chevronHighlightOverlay.MouseEnter -= OnMouseEnterChevron;
                chevronHighlightOverlay.MouseLeave -= OnMouseLeaveChevron;
            }

            DataContextChanged -= OnDataContextChanged;
            Unloaded -= OnPortViewUnloaded;
        }

        private void OnPortViewLoaded(object sender, RoutedEventArgs e)
        {
            // Event handlers for mouse enter and leave
            PortSnapping.MouseEnter += OnMouseEnterSnapping;
            PortSnapping.MouseLeave += OnMouseLeaveSnapping;
            PortBackgroundBorder.MouseEnter += OnMouseEnterBackground;
            PortBackgroundBorder.MouseLeave += OnMouseLeaveBackground;
            NodeAutoCompleteHover.MouseEnter += OnMouseEnterHover;
            NodeAutoCompleteHover.MouseLeave += OnMouseLeaveHover;

            Loaded -= OnPortViewLoaded;
        }
    }
}
