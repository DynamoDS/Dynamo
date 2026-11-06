using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
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
        private Grid aiChangeOutline = null;
        private AIChangePopover aiChangePopover = null;
        private DispatcherTimer aiChangeOpenTimer = null;

        // The workspace whose undo stack the open popover is watching, or null while it is closed
        private Graph.Workspaces.WorkspaceModel aiChangeWorkspace = null;

        // What WorkspaceModel raises when its undo stack changes (see UpdateUndoRedoStack)
        private const string UndoRedoStackChangedPropertyName = "CanUndoRedoCommand";

        private bool _useLevelSpinnerInit = false;

        // The changed-input outline: purple dots along the edge of the port background
        private const double AIChangeOutlineThickness = 1.5;
        private const double PortCornerRadius = 11;

        // Hover delay before the change popover opens, so it does not flash up while the pointer
        // only passes over the port (for example while drawing a wire)
        private static readonly TimeSpan AIChangeOpenDelay = TimeSpan.FromMilliseconds(400);

        // Static resources mostly from DynamoModern themes but some from DynamoColorsAndBrushes.xaml
        private static SolidColorBrush _primaryCharcoal200Brush = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["PrimaryCharcoal200Brush"] as SolidColorBrush;
        private static SolidColorBrush _chevronHighlightOverlayBackground = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["ChevronHighlightOverlayBackground"] as SolidColorBrush;
        private static SolidColorBrush _portMouseOverColor = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["PortMouseOverColor"] as SolidColorBrush;
        private static SolidColorBrush _nodeTransientOverlayColor = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["NodeTransientOverlayColor"] as SolidColorBrush;
        private static SolidColorBrush _darkGrey = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["DarkGreyBrush"] as SolidColorBrush;
        private static SolidColorBrush _aiChangedInputOutlineBrush = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["NodeAIChangedInputOutlineBrush"] as SolidColorBrush;

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
            _aiChangedInputOutlineBrush.Freeze();
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
                Padding = new Thickness(0),
                Width = 20,
                Height = 20,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
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
            MainGrid.PreviewMouseRightButtonUp += OnMainGridPreviewMouseRightButtonUp;
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
            viewModel.RequestShowAIChange += OnRequestShowAIChange;

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

            // The input may already carry an AI change, for example when the node view is rebuilt
            UpdateAIChange();
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
                case nameof(InPortViewModel.AIInputChange):
                    UpdateAIChange();
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
            viewModel.RequestShowAIChange -= OnRequestShowAIChange;
            aiChangeOpenTimer?.Stop();
            aiChangePopover?.Close();
            MainGrid.MouseEnter -= OnMouseEnterAIChange;
            MainGrid.MouseLeave -= OnMouseLeaveAIChange;
            GotKeyboardFocus -= OnGotKeyboardFocusAIChange;
            LostKeyboardFocus -= OnLostKeyboardFocusAIChange;
            PreviewKeyDown -= OnPreviewKeyDownAIChange;

            CleanupUITriggers();

            PortSnapping.MouseEnter -= OnMouseEnterSnapping;
            PortSnapping.MouseLeave -= OnMouseLeaveSnapping;
            PortBackgroundBorder.MouseEnter -= OnMouseEnterBackground;
            PortBackgroundBorder.MouseLeave -= OnMouseLeaveBackground;
            NodeAutoCompleteHover.MouseEnter -= OnMouseEnterHover;
            NodeAutoCompleteHover.MouseLeave -= OnMouseLeaveHover;
            MainGrid.PreviewMouseRightButtonUp -= OnMainGridPreviewMouseRightButtonUp;

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
            MainGrid.MouseEnter += OnMouseEnterAIChange;
            MainGrid.MouseLeave += OnMouseLeaveAIChange;
            GotKeyboardFocus += OnGotKeyboardFocusAIChange;
            LostKeyboardFocus += OnLostKeyboardFocusAIChange;
            PreviewKeyDown += OnPreviewKeyDownAIChange;

            Loaded -= OnPortViewLoaded;
        }

        #region AI change review

        /// <summary>
        /// The dotted outline marking an input the AI Assistant changed, or null if this input has
        /// never needed one. Exposed for tests.
        /// </summary>
        internal FrameworkElement AIChangeOutline => aiChangeOutline;

        /// <summary>
        /// The input's change popover, or null if it has never been opened. Exposed for tests.
        /// </summary>
        internal AIChangePopover AIChangePopover => aiChangePopover;

        /// <summary>
        /// Shows or hides the changed-input outline to match the view model, and makes the port
        /// focusable while it has a change to review, so the popover can be reached by keyboard.
        /// </summary>
        private void UpdateAIChange()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(UpdateAIChange));
                return;
            }

            if (viewModel?.AIInputChange == null)
            {
                if (aiChangeOutline != null)
                {
                    aiChangeOutline.Visibility = Visibility.Collapsed;
                }

                aiChangeOpenTimer?.Stop();
                aiChangePopover?.Close();
                ToolTipService.SetIsEnabled(PortBackgroundBorder, true);
                Focusable = false;
                return;
            }

            if (aiChangeOutline == null)
            {
                aiChangeOutline = CreateAIChangeOutline();
                MainGrid.Children.Add(aiChangeOutline);
            }

            aiChangeOutline.Visibility = Visibility.Visible;

            // The popover takes the place of the port's own tooltip while there is a change to review
            ToolTipService.SetIsEnabled(PortBackgroundBorder, false);
            Focusable = true;

            // An open popover follows the change, for example a later edit in the same turn
            if (aiChangePopover?.IsOpen == true)
            {
                ShowAIChangePopover();
            }
        }

        /// <summary>
        /// Opens the change popover for this input, filled in from the view model.
        /// </summary>
        internal void ShowAIChangePopover()
        {
            var change = viewModel?.AIInputChange;
            if (change == null) return;

            if (aiChangePopover == null)
            {
                aiChangePopover = new AIChangePopover(this);
                aiChangePopover.UndoRequested += OnAIChangeUndoRequested;
                aiChangePopover.NextRequested += OnAIChangeNextRequested;
                aiChangePopover.Opened += OnAIChangePopoverOpened;
                aiChangePopover.Closed += OnAIChangePopoverClosed;
            }

            var node = viewModel.NodeViewModel;
            aiChangePopover.Show(
                viewModel.PortName,
                change,
                node.CanUndoAIChange,
                node.GetNextAIChangedInput(viewModel) != null);
        }

        /// <summary>
        /// Re-checks the open popover's "Undo this change" and "Next change ›", so they do not go
        /// stale when an edit lands on the undo stack while the popover is open.
        /// </summary>
        private void UpdateAIChangePopoverActions()
        {
            if (aiChangePopover?.IsOpen != true || viewModel?.AIInputChange == null) return;

            var node = viewModel.NodeViewModel;
            aiChangePopover.UpdateActions(
                node.CanUndoAIChange,
                node.GetNextAIChangedInput(viewModel) != null);
        }

        // Only listen to the undo stack while the popover is open, so idle ports cost nothing
        private void OnAIChangePopoverOpened()
        {
            if (aiChangeWorkspace != null) return;

            aiChangeWorkspace = viewModel?.NodeViewModel?.WorkspaceViewModel?.Model;
            if (aiChangeWorkspace != null)
            {
                aiChangeWorkspace.PropertyChanged += OnAIChangeWorkspacePropertyChanged;
            }
        }

        private void OnAIChangePopoverClosed()
        {
            if (aiChangeWorkspace == null) return;

            aiChangeWorkspace.PropertyChanged -= OnAIChangeWorkspacePropertyChanged;
            aiChangeWorkspace = null;
        }

        private void OnAIChangeWorkspacePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Raised whenever an undo group is recorded (WorkspaceModel.UpdateUndoRedoStack)
            if (e.PropertyName != UndoRedoStackChangedPropertyName) return;

            if (Dispatcher.CheckAccess())
            {
                UpdateAIChangePopoverActions();
            }
            else
            {
                Dispatcher.BeginInvoke(new Action(UpdateAIChangePopoverActions));
            }
        }

        private void OnAIChangeUndoRequested()
        {
            aiChangePopover.Close();
            viewModel.NodeViewModel.UndoAIChange();
        }

        private void OnAIChangeNextRequested()
        {
            aiChangePopover.Close();
            viewModel.NodeViewModel.ShowNextAIChange(viewModel);
        }

        /// <summary>
        /// "Next change" moved the review to this input: open its popover with keyboard focus on
        /// "Next change", so repeated Enter keeps walking through the turn's changes.
        /// </summary>
        private void OnRequestShowAIChange()
        {
            // Wait for the canvas to finish centring on the node, so the popover opens in place
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (viewModel?.AIInputChange == null) return;

                Keyboard.Focus(this);
                ShowAIChangePopover();
                aiChangePopover.FocusContent(preferNext: true);
            }), DispatcherPriority.Loaded);
        }

        private void OnMouseEnterAIChange(object sender, MouseEventArgs e)
        {
            if (viewModel?.AIInputChange == null) return;

            if (aiChangePopover?.IsOpen == true)
            {
                // Back over the port before the popover closed: keep it open
                ShowAIChangePopover();
                return;
            }

            if (aiChangeOpenTimer == null)
            {
                aiChangeOpenTimer = new DispatcherTimer(DispatcherPriority.Input, Dispatcher) { Interval = AIChangeOpenDelay };
                aiChangeOpenTimer.Tick += OnAIChangeOpenTimerTick;
            }

            aiChangeOpenTimer.Start();
        }

        private void OnAIChangeOpenTimerTick(object sender, EventArgs e)
        {
            aiChangeOpenTimer.Stop();

            if (IsMouseOver)
            {
                ShowAIChangePopover();
            }
        }

        private void OnMouseLeaveAIChange(object sender, MouseEventArgs e)
        {
            aiChangeOpenTimer?.Stop();
            aiChangePopover?.CloseSoon();
        }

        private void OnGotKeyboardFocusAIChange(object sender, KeyboardFocusChangedEventArgs e)
        {
            // Only the port itself, not a control inside it such as the list level spinner. Focus
            // handed back by Escape leaves the popover closed.
            if (!ReferenceEquals(e.NewFocus, this) || aiChangePopover?.IsReturningFocus == true) return;

            ShowAIChangePopover();
        }

        private void OnLostKeyboardFocusAIChange(object sender, KeyboardFocusChangedEventArgs e)
        {
            aiChangePopover?.CloseSoon();
        }

        private void OnPreviewKeyDownAIChange(object sender, KeyEventArgs e)
        {
            // Enter or Down moves from the focused port into its popover
            if (!ReferenceEquals(e.OriginalSource, this) || (e.Key != Key.Enter && e.Key != Key.Down)) return;
            if (viewModel?.AIInputChange == null) return;

            ShowAIChangePopover();
            aiChangePopover.FocusContent(preferNext: false);
            e.Handled = true;
        }

        private static Grid CreateAIChangeOutline()
        {
            var path = new Path
            {
                Stroke = _aiChangedInputOutlineBrush,
                StrokeThickness = AIChangeOutlineThickness,
                // Zero-length dashes with round caps draw dots, one every two stroke widths
                StrokeDashArray = new DoubleCollection { 0, 2 },
                StrokeDashCap = PenLineCap.Round
            };

            // Same place and shape as the port background, and never takes mouse clicks
            var outline = new Grid
            {
                Name = "aiChangeOutline",
                Height = 29,
                VerticalAlignment = VerticalAlignment.Center,
                IsHitTestVisible = false
            };
            outline.Children.Add(path);
            outline.SizeChanged += (s, e) => path.Data = CreateAIChangeOutlineGeometry(e.NewSize);

            Grid.SetColumn(outline, 1);
            Grid.SetColumnSpan(outline, 6);
            Canvas.SetZIndex(outline, 1);

            return outline;
        }

        /// <summary>
        /// The port background's shape: square on the node side, rounded on the right. Inset by half
        /// the stroke, so the dots stay inside the port.
        /// </summary>
        private static Geometry CreateAIChangeOutlineGeometry(Size size)
        {
            var inset = AIChangeOutlineThickness / 2;
            var left = inset;
            var top = inset;
            var right = Math.Max(left, size.Width - inset);
            var bottom = Math.Max(top, size.Height - inset);
            var radius = Math.Max(0, Math.Min(PortCornerRadius, Math.Min((bottom - top) / 2, right - left)));
            var corner = new Size(radius, radius);

            var geometry = new StreamGeometry();
            using (var context = geometry.Open())
            {
                context.BeginFigure(new Point(left, top), false, true);
                context.LineTo(new Point(right - radius, top), true, false);
                context.ArcTo(new Point(right, top + radius), corner, 0, false, SweepDirection.Clockwise, true, false);
                context.LineTo(new Point(right, bottom - radius), true, false);
                context.ArcTo(new Point(right - radius, bottom), corner, 0, false, SweepDirection.Clockwise, true, false);
                context.LineTo(new Point(left, bottom), true, false);
            }

            geometry.Freeze();
            return geometry;
        }

        #endregion

        private void OnMainGridPreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}
