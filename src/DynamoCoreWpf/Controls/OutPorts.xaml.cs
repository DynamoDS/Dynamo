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
    /// Interaction logic for OutPorts.xaml
    /// </summary>
    public partial class OutPorts : UserControl
    {
        //View components that are accessed between constructor and OnDataContextChanged
        private OutPortViewModel viewModel = null;
        private Grid MainGrid = null;
        private Rectangle PortSnapping = null;
        private Border PortBackgroundBorder = null;
        private Grid PortNameGrid = null;
        private TextBlock PortNameTextBox = null;
        private Border nodeAutoCompleteMarker = null;
        private Grid NodeAutoCompleteHover = null;

        // Static resources mostly from DynamoModern themes but some from DynamoColorsAndBrushes.xaml
        private static BoolToVisibilityCollapsedConverter _boolToVisibilityCollapsedConverter = new BoolToVisibilityCollapsedConverter();
        private static FontFamily _artifactElementReg = SharedDictionaryManager.DynamoModernDictionary["ArtifaktElementRegular"] as FontFamily;
        private static SolidColorBrush _primaryCharcoal200Brush = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["PrimaryCharcoal200Brush"] as SolidColorBrush;
        private static SolidColorBrush _midGrey = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["MidGreyBrush"] as SolidColorBrush;
        private static SolidColorBrush _nodeTransientOverlayColor = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["NodeTransientOverlayColor"] as SolidColorBrush;
        private static SolidColorBrush _portMouseOverColor = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["PortMouseOverColor"] as SolidColorBrush;
        private static SolidColorBrush _portValueMarkerColor = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["UnSelectedLayoutForeground"] as SolidColorBrush;
        private static readonly ZoomToInverseVisibilityCollapsedConverter _zoomToInverseVisibilityCollapsedConverter = new ZoomToInverseVisibilityCollapsedConverter();

        //Hold the instance color for the port Background Color.  This is so it can be set differently for CodeBlock
        private SolidColorBrush portBackGroundColor = PortViewModel.PortBackgroundColorDefault;

        static OutPorts()
        {
            _primaryCharcoal200Brush.Freeze();
            _midGrey.Freeze();
            _portMouseOverColor.Freeze();
            _nodeTransientOverlayColor.Freeze();
            _portValueMarkerColor.Freeze();
        }

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
            MainGrid.ContextMenuOpening += (s, e) =>
            {
                e.Handled = true; // Suppress the default context menu
            };

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
                Fill = _portValueMarkerColor
            };

            PortValueMarker.SetBinding(Rectangle.VisibilityProperty, new Binding("PortDefaultValueMarkerVisible")
            {
                Converter = _boolToVisibilityCollapsedConverter
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
                FontFamily = _artifactElementReg,
                FontSize = 12,
                Foreground = _primaryCharcoal200Brush,
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
            MainGrid.Children.Add(PortNameGrid);
            MainGrid.Children.Add(NodeAutoCompleteHover);

            this.Content = MainGrid;

            DataContextChanged += OnDataContextChanged;
            Loaded += OnPortViewLoaded;
            Unloaded += OnPortViewUnloaded;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (viewModel != null) return;

            if (e.OldValue != null)
            {
                CleanupUITriggers();
            }

            viewModel = e.NewValue as OutPortViewModel;
            viewModel.PropertyChanged += OnPropertyChanged;

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

            // Event handlers for mouse enter and leave
            PortBackgroundBorder.MouseEnter += OnMouseEnterBackground;
            PortBackgroundBorder.MouseLeave += OnMouseLeaveBackground;
            NodeAutoCompleteHover.MouseEnter += OnMouseEnterHover;
            NodeAutoCompleteHover.MouseLeave += OnMouseLeaveHover;

            //Adjust the properties for the CodeBlock OutPort
            if (viewModel.IsPortCondensed)
            {
                MainGrid.Height = 14;
                MainGrid.Margin = new Thickness(0, 3, 0, 0);

                PortBackgroundBorder.CornerRadius = new CornerRadius(4, 0, 0, 4);
                PortBackgroundBorder.Height = 13;

                PortNameGrid.Height = 14;
                PortNameGrid.Margin = new Thickness(0, 2, 2, 0);
                PortNameTextBox.Margin = new Thickness(9, 0, 0, 1);
                PortNameTextBox.FontSize = 9;
                PortNameTextBox.MaxWidth = 100;
                PortNameTextBox.TextTrimming = TextTrimming.CharacterEllipsis;
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

        private void OnMouseEnterBackground(object sender, MouseEventArgs args)
        {
            if (PortBackgroundBorder.Dispatcher.CheckAccess())
            {
                HandleMouseHover(_portMouseOverColor, viewModel.NodeAutoCompleteMarkerEnabled);
            }
            else
            {
                PortBackgroundBorder.Dispatcher.Invoke(() =>
                {
                    HandleMouseHover(_portMouseOverColor, viewModel.NodeAutoCompleteMarkerEnabled);
                });
            }
        }
        private void OnMouseLeaveBackground(object sender, MouseEventArgs args)
        {
            if (PortBackgroundBorder.Dispatcher.CheckAccess())
            {
                HandleMouseHover(portBackGroundColor);
            }
            else
            {
                PortBackgroundBorder.Dispatcher.Invoke(() =>
                {
                    HandleMouseHover(portBackGroundColor);
                });
            }
        }

        private void HandleMouseHover(SolidColorBrush background, bool isVisible = false)
        {
            PortBackgroundBorder.Background = background;
            if (isVisible)
            {
                nodeAutoCompleteMarker.Visibility = Visibility.Visible;
            }
            else
            {
                nodeAutoCompleteMarker.Visibility = Visibility.Collapsed;
            }
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

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
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
                            PortBackgroundBorder.BorderBrush = PortViewModel.PortBorderBrushColorDefault;
                            PortBackgroundBorder.BorderThickness = new Thickness(1, 1, 1, 1);
                        }
                        else
                        {
                            PortBackgroundBorder.Dispatcher.Invoke(() =>
                            {
                                PortBackgroundBorder.BorderBrush = PortViewModel.PortBorderBrushColorDefault;
                                PortBackgroundBorder.BorderThickness = new Thickness(1, 1, 1, 1);
                            });
                        }
                    }
                    break;
            }
        }

        private void OnPortViewUnloaded(object sender, RoutedEventArgs e)
        {
            viewModel.PropertyChanged -= OnPropertyChanged;

            CleanupUITriggers();

            PortBackgroundBorder.MouseEnter -= OnMouseEnterBackground;
            PortBackgroundBorder.MouseLeave -= OnMouseLeaveBackground;
            NodeAutoCompleteHover.MouseEnter -= OnMouseEnterHover;
            NodeAutoCompleteHover.MouseLeave -= OnMouseLeaveHover;

            DataContextChanged -= OnDataContextChanged;
            Unloaded -= OnPortViewUnloaded;
        }
        private void OnPortViewLoaded(object sender, RoutedEventArgs e)
        {
            viewModel.PropertyChanged += OnPropertyChanged;
            Loaded -= OnPortViewLoaded;
        }
    }
}
