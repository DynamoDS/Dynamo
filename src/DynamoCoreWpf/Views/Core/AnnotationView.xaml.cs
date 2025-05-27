using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Linq;
using Dynamo.Selection;
using Dynamo.UI;
using Dynamo.UI.Prompts;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using DynCmd = Dynamo.Models.DynamoModel;
using TextBox = System.Windows.Controls.TextBox;
using Dynamo.Wpf.Utilities;
using Dynamo.Graph.Annotations;
using Dynamo.Logging;
using Dynamo.Configuration;
using Dynamo.Controls;
using Dynamo.Views;
using Thickness = System.Windows.Thickness;
using System.Windows.Media.Imaging;
using Dynamo.Microsoft.Xaml.Behaviors;
using static Dynamo.ViewModels.SearchViewModel;

namespace Dynamo.Nodes
{
    /// <summary>
    /// Interaction logic for AnnotationView.xaml
    /// </summary>
    public partial class AnnotationView : IViewModelView<AnnotationViewModel>
    {
        //public Grid annotationGrid;
        private Grid frozenButtonZoomedOutGrid;

        //Converters
        private static ZoomToVisibilityCollapsedConverter _zoomToVisibilityCollapsedConverter = new ZoomToVisibilityCollapsedConverter();

        //Styles
        private static Style _createGenericToolTipLightStyle = CreateGenericToolTipLightStyle();

        //Images
        private static readonly BitmapImage _frozenDarkImage = new BitmapImage(new Uri("pack://application:,,,/DynamoCoreWpf;component/UI/Images/Annotations/frozen-dark-64px.png"));
        private static readonly BitmapImage _frozenHoverImage = new BitmapImage(new Uri("pack://application:,,,/DynamoCoreWpf;component/UI/Images/Annotations/frozen-hover-64px.png"));

        public AnnotationViewModel ViewModel { get; private set; }
        public static DependencyProperty SelectAllTextOnFocus;
        static AnnotationView()
        {
            // Freeze the bitmaps to improve performance
            _frozenDarkImage.Freeze();
            _frozenHoverImage.Freeze();
        }
        public AnnotationView()
        {
            Resources.MergedDictionaries.Add(SharedDictionaryManager.DynamoModernDictionary);
            Resources.MergedDictionaries.Add(SharedDictionaryManager.DynamoColorsAndBrushesDictionary);
            Resources.MergedDictionaries.Add(SharedDictionaryManager.DataTemplatesDictionary);
            Resources.MergedDictionaries.Add(SharedDictionaryManager.DynamoConvertersDictionary);

            InitializeComponent();
            InitializeUI();
            //// Create the Grid
            //annotationGrid = new Grid
            //{
            //    Name = "AnnotationGrid",
            //    Height = Double.NaN, // "Auto" in XAML is Double.NaN in C#
            //    IsHitTestVisible = true
            //};

            //// Add RowDefinitions
            //annotationGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            //annotationGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            Unloaded += AnnotationView_Unloaded;
            Loaded += AnnotationView_Loaded;
            DataContextChanged += AnnotationView_DataContextChanged;
            this.GroupTextBlock.SizeChanged += GroupTextBlock_SizeChanged;

            // Because the size of the CollapsedAnnotationRectangle doesn't necessarily change 
            // when going from Visible to collapse (and other way around), we need to also listen
            // to IsVisibleChanged. Both of these handlers will set the ModelAreaHeight on the ViewModel
            this.CollapsedAnnotationRectangle.SizeChanged += CollapsedAnnotationRectangle_SizeChanged;
            this.CollapsedAnnotationRectangle.IsVisibleChanged += CollapsedAnnotationRectangle_IsVisibleChanged;
        }

        private void InitializeUI()
        {
            AnnotationGrid.Children.Add(CreateFrozenButtonZoomedOutGrid());
        }

        private void AnnotationView_Unloaded(object sender, RoutedEventArgs e)
        {
            Loaded -= AnnotationView_Loaded;
            DataContextChanged -= AnnotationView_DataContextChanged;
            this.GroupTextBlock.SizeChanged -= GroupTextBlock_SizeChanged;
            this.CollapsedAnnotationRectangle.SizeChanged -= CollapsedAnnotationRectangle_SizeChanged;
            this.CollapsedAnnotationRectangle.IsVisibleChanged -= CollapsedAnnotationRectangle_IsVisibleChanged;
        }

        private void AnnotationView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (ViewModel != null ||
                !(this.DataContext is AnnotationViewModel viewModel))
            {
                return;
            }

            ViewModel = viewModel;
        }

        private void AnnotationView_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel = this.DataContext as AnnotationViewModel;
            if (ViewModel != null)
            {
                //Set the height and width of Textblock based on the content.
                if (!ViewModel.AnnotationModel.loadFromXML)
                {
                    SetTextMaxWidth();
                    SetTextHeight();
                }
                ViewModel.UpdateProxyPortsPosition();

                if (frozenButtonZoomedOutGrid != null)
                {
                    var frozenButton = frozenButtonZoomedOutGrid.Children.OfType<Button>().FirstOrDefault();
                    if (frozenButton != null)
                    {
                        // Create the trigger for the button click
                        var mouseLeftButtonDownTrigger = new Dynamo.UI.Views.HandlingEventTrigger()
                        {
                            EventName = "Click"
                        };

                        // Create the command action
                        var mouseLeftButtonDownAction = new InvokeCommandAction()
                        {
                            Command = ViewModel.ToggleIsFrozenGroupCommand
                        };

                        // Add the action to the trigger
                        mouseLeftButtonDownTrigger.Actions.Add(mouseLeftButtonDownAction);

                        // Add the trigger to the button
                        Dynamo.Microsoft.Xaml.Behaviors.Interaction.GetTriggers(frozenButton).Add(mouseLeftButtonDownTrigger);

                        // Set the button template
                        frozenButton.Template = (ControlTemplate)FindResource("FrozenButtonZoomedOutTemplate");
                    }
                }
            }
        }

        private Grid CreateFrozenButtonZoomedOutGrid()
        {
            var grid = new Grid
            {
                Name = "FrozenButtonGrid"
            };
            Grid.SetRow(grid, 0);
            Grid.SetRowSpan(grid, 2);
            Grid.SetColumn(grid, 0);
            Grid.SetColumnSpan(grid, 4);
            Panel.SetZIndex(grid, 1);

            // Create the Style for the Grid
            var gridStyle = new Style(typeof(Grid));
            gridStyle.Setters.Add(new Setter(UIElement.VisibilityProperty, Visibility.Collapsed));

            // MultiDataTrigger
            var multiTrigger = new MultiDataTrigger();

            // Condition 1: DataContext.AnnotationModel.IsFrozen == true
            multiTrigger.Conditions.Add(new Condition
            {
                Binding = new Binding("DataContext.AnnotationModel.IsFrozen")
                {
                    RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(UserControl), 1)
                },
                Value = true
            });

            // Condition 2: DataContext.IsExpanded == false
            multiTrigger.Conditions.Add(new Condition
            {
                Binding = new Binding("DataContext.IsExpanded")
                {
                    RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(UserControl), 1)
                },
                Value = false
            });

            // Condition 3: DataContext.Zoom (with converter) == Visible
            multiTrigger.Conditions.Add(new Condition
            {
                Binding = new Binding("DataContext.Zoom")
                {
                    RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(WorkspaceView), 1),
                    Converter = _zoomToVisibilityCollapsedConverter
                },
                Value = Visibility.Visible
            });

            // Setter for the trigger
            multiTrigger.Setters.Add(new Setter(UIElement.VisibilityProperty, Visibility.Visible));

            // Add the trigger to the style
            gridStyle.Triggers.Add(multiTrigger);

            // Assign the style to the grid
            grid.Style = gridStyle;

            // Create the Button
            var button = new Button
            {
                Width = 64,
                Height = 64,
                Template = CreateFrozenButtonZoomedOutTemplate(),
            };

            // Create the ToolTip
            var toolTip = new ToolTip
            {
                Style = _createGenericToolTipLightStyle,
                Content = new TextBlock
                {
                    Text = Wpf.Properties.Resources.GroupFrozenButtonToolTip
                }
            };
            button.ToolTip = toolTip;

            // Add the Button to the Grid
            grid.Children.Add(button);
            frozenButtonZoomedOutGrid = grid;
            return frozenButtonZoomedOutGrid;
        }
        private static Style CreateGenericToolTipLightStyle()
        {
            // Main Style for ToolTip
            var style = new Style(typeof(ToolTip));
            style.Setters.Add(new Setter(System.Windows.Controls.ToolTip.OverridesDefaultStyleProperty, true));
            style.Setters.Add(new Setter(System.Windows.Controls.ToolTip.MaxWidthProperty, 300.0));

            // ControlTemplate for ToolTip
            var template = new ControlTemplate(typeof(ToolTip));
            var popupGrid = new FrameworkElementFactory(typeof(Grid));
            popupGrid.Name = "PopupGrid";

            var shadowBackground = new FrameworkElementFactory(typeof(Grid));
            shadowBackground.Name = "ShadowBackground";
            shadowBackground.SetValue(Grid.BackgroundProperty, Brushes.Transparent);

            // Path (pointer)
            var pointerPath = new FrameworkElementFactory(typeof(Path));
            pointerPath.SetValue(Path.WidthProperty, 20.0);
            pointerPath.SetValue(Path.HeightProperty, 6.0);
            pointerPath.SetValue(Path.MarginProperty, new Thickness(5, 0, 0, 0));
            pointerPath.SetValue(Path.HorizontalAlignmentProperty, HorizontalAlignment.Left);
            pointerPath.SetValue(Path.VerticalAlignmentProperty, VerticalAlignment.Top);
            pointerPath.SetValue(Path.DataProperty, Geometry.Parse("M0,6 L6,0 12,6Z"));
            pointerPath.SetValue(Path.FillProperty, Brushes.White);
            pointerPath.SetValue(Path.StretchProperty, Stretch.None);
            pointerPath.SetValue(Path.StrokeProperty, Brushes.Gray);

            // Main Border
            var mainBorder = new FrameworkElementFactory(typeof(Border));
            mainBorder.SetValue(Border.MarginProperty, new Thickness(0, 5, 7, 7));
            mainBorder.SetValue(Border.PaddingProperty, new Thickness(10, 8, 10, 8));
            mainBorder.SetValue(Border.BackgroundProperty, Brushes.White);
            mainBorder.SetValue(Border.BorderBrushProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#999999")));
            mainBorder.SetValue(Border.BorderThicknessProperty, new Thickness(1, 0, 1, 1));
            mainBorder.SetValue(Border.CornerRadiusProperty, new CornerRadius(3));
            mainBorder.AppendChild(new FrameworkElementFactory(typeof(ContentPresenter)));

            // Top Border
            var topBorder = new FrameworkElementFactory(typeof(Border));
            topBorder.SetValue(Border.HeightProperty, 7.0);
            topBorder.SetValue(Border.MarginProperty, new Thickness(16, 5, 9, 0));
            topBorder.SetValue(Border.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
            topBorder.SetValue(Border.VerticalAlignmentProperty, VerticalAlignment.Top);
            topBorder.SetValue(Border.BorderBrushProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#999999")));
            topBorder.SetValue(Border.BorderThicknessProperty, new Thickness(0, 1, 0, 0));
            topBorder.SetValue(Border.CornerRadiusProperty, new CornerRadius(0, 0, 3, 0));

            // Left Border
            var leftBorder = new FrameworkElementFactory(typeof(Border));
            leftBorder.SetValue(Border.WidthProperty, 6.0);
            leftBorder.SetValue(Border.HeightProperty, 7.0);
            leftBorder.SetValue(Border.MarginProperty, new Thickness(0, 5, 0, 0));
            leftBorder.SetValue(Border.HorizontalAlignmentProperty, HorizontalAlignment.Left);
            leftBorder.SetValue(Border.VerticalAlignmentProperty, VerticalAlignment.Top);
            leftBorder.SetValue(Border.BorderBrushProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#999999")));
            leftBorder.SetValue(Border.BorderThicknessProperty, new Thickness(0, 1, 0, 0));
            leftBorder.SetValue(Border.CornerRadiusProperty, new CornerRadius(3, 0, 0, 0));

            // Compose visual tree
            shadowBackground.AppendChild(pointerPath);
            shadowBackground.AppendChild(mainBorder);
            shadowBackground.AppendChild(topBorder);
            shadowBackground.AppendChild(leftBorder);
            popupGrid.AppendChild(shadowBackground);
            template.VisualTree = popupGrid;

            style.Setters.Add(new Setter(System.Windows.Controls.ToolTip.TemplateProperty, template));

            // TextBlock style for ContentPresenter
            var textBlockStyle = new Style(typeof(TextBlock));
            textBlockStyle.Setters.Add(new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap));
            textBlockStyle.Setters.Add(new Setter(TextBlock.FontFamilyProperty, new FontFamily("Artifakt Element Regular"))); // Adjust as needed
            textBlockStyle.Setters.Add(new Setter(TextBlock.FontSizeProperty, 12.0));
            textBlockStyle.Setters.Add(new Setter(TextBlock.ForegroundProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#232323"))));

            //var contentPresenterStyle = new Style(typeof(ContentPresenter));
            //contentPresenterStyle.Resources.Add(typeof(TextBlock), textBlockStyle);

            //style.Resources.Add(typeof(ContentPresenter), contentPresenterStyle);

            return style;
        }
        private ControlTemplate CreateFrozenButtonZoomedOutTemplate()
        {
            var template = new ControlTemplate(typeof(Button));

            var imageFactory = new FrameworkElementFactory(typeof(Image));
            imageFactory.Name = "FrozenImageZoomedOut";
            imageFactory.SetValue(Image.WidthProperty, new TemplateBindingExtension(Button.WidthProperty));
            imageFactory.SetValue(Image.HeightProperty, new TemplateBindingExtension(Button.HeightProperty));
            imageFactory.SetValue(Image.SourceProperty, _frozenDarkImage);

            template.VisualTree = imageFactory;

            var multiTrigger = new MultiDataTrigger();
            multiTrigger.Conditions.Add(new Condition
            {
                Binding = new Binding("IsMouseOver")
                {
                    RelativeSource = new RelativeSource(RelativeSourceMode.Self)
                },
                Value = true
            });

            multiTrigger.Setters.Add(new Setter
            {
                TargetName = "FrozenImageZoomedOut",
                Property = Image.SourceProperty,
                Value = _frozenHoverImage
            });

            template.Triggers.Add(multiTrigger);

            return template;
        }

        //Set the max width of text area based on the width of the longest word in the text
        private void SetTextMaxWidth()
        {
            var words = this.ViewModel.AnnotationText.Split(' ');
            var maxLength = 0;
            string longestWord = words[0];

            foreach (var w in words)
            {
                if (w.Length > maxLength)
                {
                    longestWord = w;
                    maxLength = w.Length;
                }
            }

            var formattedText = new FormattedText(
                longestWord,
                System.Globalization.CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface(this.GroupTextBlock.FontFamily, this.GroupTextBlock.FontStyle, this.GroupTextBlock.FontWeight, this.GroupTextBlock.FontStretch),
                this.GroupTextBlock.FontSize,
                Brushes.Black);

            var margin = this.TextBlockGrid.Margin.Right + this.TextBlockGrid.Margin.Left;

            this.ViewModel.AnnotationModel.Width = formattedText.Width + margin;
            this.ViewModel.AnnotationModel.TextMaxWidth = formattedText.Width + margin;
        }

        private void OnNodeColorSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems == null || (e.AddedItems.Count <= 0))
                return;

            //store the old one
            if (e.RemovedItems != null || e.RemovedItems.Count > 0)
            {
                var orectangle = e.AddedItems[0] as Rectangle;
                if (orectangle != null)
                {
                    var brush = orectangle.Fill as SolidColorBrush;
                    if (brush != null)
                    {
                        ViewModel.WorkspaceViewModel.DynamoViewModel.ExecuteCommand(
                         new DynCmd.UpdateModelValueCommand(
                         System.Guid.Empty, ViewModel.AnnotationModel.GUID, "Background", brush.Color.ToString()));
                    }

                }
            }

            var rectangle = e.AddedItems[0] as Rectangle;
            if (rectangle != null)
            {
                var brush = rectangle.Fill as SolidColorBrush;
                if (brush != null)
                    ViewModel.Background = brush.Color;
            }
        }
        /// <summary>
        /// This function will clear the selection and then select only the annotation node to delete it for ungrouping.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnUngroupAnnotation(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                DynamoSelection.Instance.ClearSelection();
                Guid annotationGuid = this.ViewModel.AnnotationModel.GUID;

                // Expand the group before deleting it
                // otherwise collapsed content will be "lost" 
                if (!this.ViewModel.IsExpanded)
                {
                    this.ViewModel.IsExpanded = true;
                }
                 
                ViewModel.WorkspaceViewModel.DynamoViewModel.ExecuteCommand(
                   new DynCmd.SelectModelCommand(annotationGuid, Keyboard.Modifiers.AsDynamoType()));
                ViewModel.WorkspaceViewModel.DynamoViewModel.DeleteCommand.Execute(null);
                ViewModel.WorkspaceViewModel.HasUnsavedChanges = true;

                Analytics.TrackEvent(Actions.Ungroup, Categories.GroupOperations);
            }
        }

        /// <summary>
        /// Handles the OnMouseLeftButtonDown event of the AnnotationView control.
        /// Selects the models inside the group
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void AnnotationView_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (GroupTextBlock.IsVisible ||
                (!GroupTextBlock.IsVisible && !GroupTextBox.IsVisible))
            {
                ViewModel.SelectAll();
            }

            //When Textbox is visible,clear the selection. That way, models will not be added to
            //dragged nodes one more time. Ref: MAGN-7321
            if (GroupTextBlock.IsVisible && e.ClickCount >= 2)
            {
                DynamoSelection.Instance.ClearSelection();
                //Set the panning mode to false if a group is in editing mode.
                if (ViewModel.WorkspaceViewModel.IsPanning)
                {
                    ViewModel.WorkspaceViewModel.DynamoViewModel.BackgroundPreviewViewModel.TogglePan(null);
                }
                e.Handled = true;
            }

            //When the Zoom * Fontsized factor is less than 7, then
            //show the edit window
            if (!GroupTextBlock.IsVisible && e.ClickCount >= 2)
            {
                var editWindow = new EditWindow(ViewModel.WorkspaceViewModel.DynamoViewModel, true)
                {
                    Title = Dynamo.Wpf.Properties.Resources.EditAnnotationTitle
                };
                editWindow.BindToProperty(DataContext, new Binding("AnnotationText")
                {
                    Mode = BindingMode.TwoWay,
                    Source = (DataContext as AnnotationViewModel),
                    UpdateSourceTrigger = UpdateSourceTrigger.Explicit
                });

                editWindow.ShowDialog();
                e.Handled = true;
            }
        }

        private void AnnotationView_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ViewModel.SelectAll();                      
        }

        /// <summary>
        /// Handles the OnTextChanged event of the GroupTextBox control.
        /// Calculates the height of a Group based on the height of textblock
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        private void GroupTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (ViewModel is null || !IsLoaded) return;

            SetTextMaxWidth();
            SetTextHeight();
            ViewModel.WorkspaceViewModel.HasUnsavedChanges = true;
        }


        /// <summary>
        /// Handles the SizeChanged event of the GroupTextBlock control.
        /// This function calculates the height of a group based on font size
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SizeChangedEventArgs"/> instance containing the event data.</param>
        private void GroupTextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ViewModel != null && (e.HeightChanged || e.WidthChanged))
            {
                SetTextMaxWidth();
                SetTextHeight();
            }

            ViewModel.UpdateProxyPortsPosition();
        }


        /// <summary>
        /// Select the text in textbox
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private void GroupTextBox_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var textbox = sender as TextBox;
            if (textbox == null || textbox.Visibility != Visibility.Visible) return;

            //Record the value here, this is useful when title is popped from stack during undo
            ViewModel.WorkspaceViewModel.DynamoViewModel.ExecuteCommand(
                new DynCmd.UpdateModelValueCommand(
                    Guid.Empty, ViewModel.AnnotationModel.GUID, "TextBlockText",
                    GroupTextBox.Text));

            ViewModel.WorkspaceViewModel.DynamoViewModel.RaiseCanExecuteUndoRedo();

            textbox.Focus();
            if (textbox.Text.Equals(Properties.Resources.GroupNameDefaultText))
            {
                textbox.SelectAll();
            }
        }

        /// <summary>
        /// Set the Mouse caret at the end
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void GroupTextBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            this.GroupTextBox.CaretIndex = Int32.MaxValue;
        }

        /// <summary>
        /// This function will delete the group with modes
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnDeleteAnnotation(object sender, RoutedEventArgs e)
        {
            //Select the group and the models within that group
            if (ViewModel != null)
            {
                DynamoSelection.Instance.ClearSelection();
                ViewModel.SelectAll();
                ViewModel.WorkspaceViewModel.DynamoViewModel.DeleteCommand.Execute(null);

                Analytics.TrackEvent(Actions.Delete, Categories.GroupOperations);
            }
        }

        /// <summary>
        /// This function will run graph layout algorithm to the nodes inside the selected group.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnGraphLayoutAnnotation(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                DynamoSelection.Instance.ClearSelection();
                ViewModel.SelectAll();
                ViewModel.WorkspaceViewModel.DynamoViewModel.GraphAutoLayoutCommand.Execute(null);
            }
        }

        private void GroupDescriptionTextBox_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var textbox = sender as TextBox;
            if (textbox == null || textbox.Visibility != Visibility.Visible) return;

            //Record the value here, this is useful when title is popped from stack during undo
            ViewModel.WorkspaceViewModel.DynamoViewModel.ExecuteCommand(
                new DynCmd.UpdateModelValueCommand(
                    Guid.Empty, ViewModel.AnnotationModel.GUID, nameof(AnnotationModel.AnnotationDescriptionText),
                    GroupDescriptionTextBox.Text));

            ViewModel.WorkspaceViewModel.DynamoViewModel.RaiseCanExecuteUndoRedo();

            textbox.Focus();
            if (textbox.Text.Equals(Properties.Resources.GroupDefaultText))
            {
                textbox.SelectAll();
            }
        }

        private void GroupDescriptionTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            this.GroupDescriptionTextBox.CaretIndex = Int32.MaxValue;
        }

        private void GroupDescriptionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ViewModel is null || !IsLoaded) return;

            SetTextHeight();
            ViewModel.WorkspaceViewModel.HasUnsavedChanges = true;

        }

        private void SetTextHeight()
        {
            if (GroupDescriptionTextBlock is null || GroupTextBlock is null || ViewModel is null)
            {
                return;
            }

            // Use the DesiredSize and not the Actual height. Because when Textblock is collapsed,
            // Actual height is same as previous size.
            ViewModel.AnnotationModel.TextBlockHeight = 
                this.GroupDescriptionControls.DesiredSize.Height + 
                this.GroupNameControl.DesiredSize.Height;
        }

        private void CollapsedAnnotationRectangle_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetModelAreaHeight();
        }

        private void CollapsedAnnotationRectangle_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetModelAreaHeight();
        }

        private void SetModelAreaHeight()
        {
            // We only want to change the ModelAreaHeight
            // if the CollapsedAnnotationRectangle is visible,
            // as if its not it will be equal to the height of the
            // contained nodes + the TextBlockHeight
            if (ViewModel is null || !this.CollapsedAnnotationRectangle.IsVisible) return;
            ViewModel.ModelAreaHeight = this.CollapsedAnnotationRectangle.ActualHeight;
            ViewModel.AnnotationModel.UpdateBoundaryFromSelection();
        }

        private void contextMenu_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectAll();
            this.AnnotationGrid.ContextMenu.DataContext = ViewModel;
            this.AnnotationGrid.ContextMenu.IsOpen = true;
        }

        private void AnnotationRectangleThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var xAdjust = (ViewModel.Width) + e.HorizontalChange;
            var yAdjust = (ViewModel.Height) + e.VerticalChange;

            if (xAdjust >= ViewModel.Width - ViewModel.AnnotationModel.WidthAdjustment)
            {
                ViewModel.AnnotationModel.WidthAdjustment += e.HorizontalChange;
                ViewModel.WorkspaceViewModel.HasUnsavedChanges = true;
            }

            if (yAdjust >= ViewModel.Height - ViewModel.AnnotationModel.HeightAdjustment)
            {
                ViewModel.AnnotationModel.HeightAdjustment += e.VerticalChange;
                ViewModel.WorkspaceViewModel.HasUnsavedChanges = true;

            }
        }

        private void Thumb_MouseEnter(object sender, MouseEventArgs e)
        {
            ViewModel.WorkspaceViewModel.CurrentCursor = CursorLibrary.GetCursor(CursorSet.ResizeDiagonal);
        }

        private void Thumb_MouseLeave(object sender, MouseEventArgs e)
        {
            ViewModel.WorkspaceViewModel.CurrentCursor = CursorLibrary.GetCursor(CursorSet.Pointer);
        }

        private void GroupDescriptionTextBlock_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetTextHeight();
        }

        private void GroupDescriptionControls_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetTextHeight();
        }

        /// <summary>
        /// According to the current GroupStyle selected (or not selected) in the ContextMenu several actions can be executed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GroupStyleCheckmark_Click(object sender, RoutedEventArgs e)
        {
            var menuItemSelected = sender as MenuItem;
            if (menuItemSelected == null) return;

            var groupStyleItemSelected = menuItemSelected.DataContext as GroupStyleItem;
            if (groupStyleItemSelected == null) return;

            ViewModel.UpdateGroupStyle(groupStyleItemSelected);
            // Tracking selecting group style item and if it is a default style by Dynamo
            Logging.Analytics.TrackEvent(Actions.Select, Categories.GroupStyleOperations, nameof(GroupStyleItem), groupStyleItemSelected.IsDefault ? 1 : 0);
        }

        /// <summary>
        /// When the GroupStyle Submenu is opened then we need to re-load the GroupStyles in the ContextMenu (in case more Styles were added in Preferences panel).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GroupStyleAnnotation_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            ViewModel.ReloadGroupStyles();
            // Tracking loading group style items
            Logging.Analytics.TrackEvent(Actions.Load, Categories.GroupStyleOperations, nameof(GroupStyleItem) + "s");
        }
    }
}
