using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Dynamo.Controls;
using Dynamo.Graph.Annotations;
using Dynamo.Graph.Nodes;
using Dynamo.Logging;
using Dynamo.Selection;
using Dynamo.UI;
using Dynamo.UI.Controls;
using Dynamo.UI.Prompts;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Views;
using Dynamo.Wpf.Utilities;
using DynCmd = Dynamo.Models.DynamoModel;
using EventTrigger = System.Windows.EventTrigger;
using TextBox = System.Windows.Controls.TextBox;
using Thickness = System.Windows.Thickness;
using ModifierKeys = System.Windows.Input.ModifierKeys;
using System.ComponentModel;
using System.Collections.Specialized;

namespace Dynamo.Nodes
{
    /// <summary>
    /// Interaction logic for AnnotationView.xaml
    /// </summary>
    public partial class AnnotationView : IViewModelView<AnnotationViewModel>
    {
        internal Grid AnnotationGrid;
        internal Popup GroupContextMenuPopup;
        internal Grid GroupStyleSelectorGrid;
        private Grid frozenButtonZoomedOutGrid;
        private Grid textBlockGrid;
        private TextBlock groupTextBlock;
        private TextBlock groupDescriptionTextBlock;
        private TextBox groupTextBox;
        private TextBox groupDescriptionTextBox;
        private ContentControl groupNameControl;
        private ContentControl groupDescriptionControls;
        private Border collapsedAnnotationRectangle;
        private Expander groupExpander;
        private ItemsControl inputPortControl;
        private ItemsControl outputPortControl;
        private Thumb mainGroupThumb;
        private StackPanel groupPopupPanel;
        private InCanvasSearchControl searchBar;

        private bool _isUpdatingLayout = false;
        private bool isSearchFromGroupContext;

        //Converters
        private static readonly ZoomToVisibilityCollapsedConverter _zoomToVisibilityCollapsedConverter = new ZoomToVisibilityCollapsedConverter();
        private static readonly ListHasMoreThanNItemsToVisibilityConverter _listHasMoreThanNItemsToVisibilityConverter = new ListHasMoreThanNItemsToVisibilityConverter();
        private static readonly ConnectionStateToBrushConverter _connectionStateToBrushConverter = SharedDictionaryManager.DynamoConvertersDictionary["ConnectionStateToBrushConverter"] as ConnectionStateToBrushConverter;
        private static readonly ConnectionStateToVisibilityCollapsedConverter _connectionStateToVisibilityCollapsedConverter = new ConnectionStateToVisibilityCollapsedConverter();
        private static readonly BoolToVisibilityCollapsedConverter _boolToVisibilityCollapsedConverter = new BoolToVisibilityCollapsedConverter();
        private static readonly AnnotationTextConverter _annotationTextConverter = SharedDictionaryManager.DynamoConvertersDictionary["AnnotationTextConverter"] as AnnotationTextConverter;
        private static readonly TextForegroundSaturationColorConverter _textForegroundSaturationColorConverter = SharedDictionaryManager.DynamoConvertersDictionary["TextForegroundSaturationColorConverter"] as TextForegroundSaturationColorConverter;
        private static readonly GroupTitleVisibilityConverter _groupTitleVisibilityConverter = SharedDictionaryManager.DynamoConvertersDictionary["GroupTitleVisibilityConverter"] as GroupTitleVisibilityConverter;
        private static readonly InverseBooleanToVisibilityCollapsedConverter _inverseBooleanToVisibilityCollapsedConverter = new InverseBooleanToVisibilityCollapsedConverter();
        private static readonly InverseBooleanConverter _inverseBooleanConverter = new InverseBooleanConverter();
        private static readonly NestedGroupsLabelConverter _nestedGroupsLabelConverter = new NestedGroupsLabelConverter();
        private static readonly CollectionHasMoreThanNItemsToBoolConverter _collectionHasMoreThanNItemsToBoolConverter = new CollectionHasMoreThanNItemsToBoolConverter();
        private static readonly BackgroundConditionEvaluator _backgroundConditionEvaluator = new BackgroundConditionEvaluator();
        private static readonly ColorToSolidColorBrushConverter _colorToSolidColorBrushConverter = new ColorToSolidColorBrushConverter();
        private static readonly StringToBrushColorConverter _stringToBrushColorConverter = new StringToBrushColorConverter();

        //Styles
        private static readonly Style _createGenericToolTipLightStyle = CreateGenericToolTipLightStyle();
        private static readonly Style _dynamoToolTipTopStyle = GetDynamoToolTipTopStyle();
        private static readonly Style _groupResizeThumbStyle = CreateGroupResizeThumbStyle();

        //Font
        private static FontFamily _artifaktElementRegular = SharedDictionaryManager.DynamoModernDictionary["ArtifaktElementRegular"] as FontFamily;
        private static FontFamily _artifaktElementBold = SharedDictionaryManager.DynamoModernDictionary["ArtifaktElementBold"] as FontFamily;

        //Brushes
        private static SolidColorBrush _primaryCharcoal300 = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["PrimaryCharcoal300Brush"] as SolidColorBrush;
        private static SolidColorBrush _midGreyBrush = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["MidGreyBrush"] as SolidColorBrush;
        private static SolidColorBrush _blue300Brush = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["Blue300Brush"] as SolidColorBrush;
        private static SolidColorBrush _preferencesWindowButtonColor = SharedDictionaryManager.DynamoModernDictionary["PreferencesWindowButtonColor"] as SolidColorBrush;
        private static SolidColorBrush _primaryCharcoal100 = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["PrimaryCharcoal100Brush"] as SolidColorBrush;
        private static SolidColorBrush _nodeContextMenuSeparatorColor = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["NodeContextMenuSeparatorColor"] as SolidColorBrush;
        private static SolidColorBrush _nodeContextMenuBackgroundHighlight = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["NodeContextMenuBackgroundHighlight"] as SolidColorBrush;
        private static SolidColorBrush _nodeContextMenuBackground = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["NodeContextMenuBackground"] as SolidColorBrush;
        private static SolidColorBrush _nodeContextMenuForeground = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["NodeContextMenuForeground"] as SolidColorBrush;

        private static CornerRadius _cornerRadius = new CornerRadius(10, 10, 0, 0);

        //Images
        private static readonly BitmapImage _frozenLightImage = new BitmapImage(new Uri("pack://application:,,,/DynamoCoreWpf;component/UI/Images/Annotations/frozen-light-64px.png"));
        private static readonly BitmapImage _frozenDarkImage = new BitmapImage(new Uri("pack://application:,,,/DynamoCoreWpf;component/UI/Images/Annotations/frozen-dark-64px.png"));
        private static readonly BitmapImage _frozenHoverImage = new BitmapImage(new Uri("pack://application:,,,/DynamoCoreWpf;component/UI/Images/Annotations/frozen-hover-64px.png"));
        private static readonly BitmapImage _caretDownGreyImage = new BitmapImage(new Uri("pack://application:,,,/DynamoCoreWpf;component/UI/Images/caret_down_grey_48px.png"));
        private static readonly BitmapImage _caretDownWhiteImage = new BitmapImage(new Uri("pack://application:,,,/DynamoCoreWpf;component/UI/Images/caret_down_white_48px.png"));
        private static readonly BitmapImage _caretDownHoverImage = new BitmapImage(new Uri("pack://application:,,,/DynamoCoreWpf;component/UI/Images/caret_down_hover_48px.png"));
        private static readonly BitmapImage _caretUpGreyImage = new BitmapImage(new Uri("pack://application:,,,/DynamoCoreWpf;component/UI/Images/caret_up_grey_48px.png"));
        private static readonly BitmapImage _caretUpWhiteImage = new BitmapImage(new Uri("pack://application:,,,/DynamoCoreWpf;component/UI/Images/caret_up_white_48px.png"));
        private static readonly BitmapImage _caretUpHoverImage = new BitmapImage(new Uri("pack://application:,,,/DynamoCoreWpf;component/UI/Images/caret_up_hover_48px.png"));

        private static readonly BitmapImage _warningImage = new BitmapImage(new Uri("pack://application:,,,/DynamoCoreWpf;component/UI/Images/warning.png"));
        private static readonly BitmapImage _errorImage = new BitmapImage(new Uri("pack://application:,,,/DynamoCoreWpf;component/UI/Images/error.png"));
        private static readonly BitmapImage _menuGreyImage = new BitmapImage(new Uri("pack://application:,,,/DynamoCoreWpf;component/UI/Images/menu_grey_48px.png"));
        private static readonly BitmapImage _menuWhiteImage = new BitmapImage(new Uri("pack://application:,,,/DynamoCoreWpf;component/UI/Images/menu_white_48px.png"));
        private static readonly BitmapImage _menuHoverImage = new BitmapImage(new Uri("pack://application:,,,/DynamoCoreWpf;component/UI/Images/menu_hover_48px.png"));

        private EventHandler _groupContextMenuClosedHandler;

        public AnnotationViewModel ViewModel { get; private set; }
        public static DependencyProperty SelectAllTextOnFocus;
        static AnnotationView()
        {
            // Freeze the bitmaps and brushes to improve performance
            _frozenDarkImage.Freeze();
            _frozenLightImage.Freeze();
            _frozenHoverImage.Freeze();
            _caretDownGreyImage.Freeze();
            _caretDownWhiteImage.Freeze();
            _caretDownHoverImage.Freeze();
            _caretUpGreyImage.Freeze();
            _caretUpWhiteImage.Freeze();
            _caretUpHoverImage.Freeze();

            _primaryCharcoal300.Freeze();
            _midGreyBrush.Freeze();
            _blue300Brush.Freeze();
            _preferencesWindowButtonColor.Freeze();
            _primaryCharcoal100.Freeze();
            _nodeContextMenuSeparatorColor.Freeze();
            _nodeContextMenuBackgroundHighlight.Freeze();
            _nodeContextMenuBackground.Freeze();
            _nodeContextMenuForeground.Freeze();
        }
        public AnnotationView()
        {
            InitializeComponent();
            InitializeUI();

            Unloaded += AnnotationView_Unloaded;
            Loaded += AnnotationView_Loaded;
            DataContextChanged += AnnotationView_DataContextChanged;
            this.groupTextBlock.SizeChanged += GroupTextBlock_SizeChanged;
            PreviewMouseDoubleClick += OnAnnotationDoubleClick;

            // Because the size of the collapsedAnnotationRectangle doesn't necessarily change 
            // when going from Visible to collapse (and other way around), we need to also listen
            // to IsVisibleChanged. Both of these handlers will set the ModelAreaHeight on the ViewModel
            this.collapsedAnnotationRectangle.SizeChanged += CollapsedAnnotationRectangle_SizeChanged;
            this.collapsedAnnotationRectangle.IsVisibleChanged += CollapsedAnnotationRectangle_IsVisibleChanged;
        }

        private void InitializeUI()
        {
            // Create a new NameScope for this view if one doesn't exist
            if (NameScope.GetNameScope(this) == null)
            {
                NameScope.SetNameScope(this, new NameScope());
            }

            // Create the Grid
            AnnotationGrid = new Grid
            {
                Name = "AnnotationGrid",
                Height = Double.NaN,
                IsHitTestVisible = true
            };

            AnnotationGrid.SetBinding(VisibilityProperty, new Binding("IsCollapsed")
            {
                Converter = _inverseBooleanToVisibilityCollapsedConverter
            });

            // Add RowDefinitions
            AnnotationGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            AnnotationGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Create and apply the implicit style
            CreateExpanderStyle();

            AnnotationGrid.Children.Add(CreateFrozenButtonZoomedOutGrid());
            AnnotationGrid.Children.Add(CreatePersistentBorder());
            AnnotationGrid.Children.Add(CreateSelectionBorder());
            AnnotationGrid.Children.Add(CreateNodeHoverBorder());
            AnnotationGrid.Children.Add(CreateGroupExpander());
            AnnotationGrid.Children.Add(CreateCollapsedAnnotationRectangle());

            this.RegisterName(groupTextBlock.Name, groupTextBlock);
            this.RegisterName(groupTextBox.Name, groupTextBox);
            this.RegisterName(groupNameControl.Name, groupNameControl);
            this.RegisterName(groupDescriptionTextBlock.Name, groupDescriptionTextBlock);
            this.RegisterName(groupDescriptionTextBox.Name, groupDescriptionTextBox);
            this.RegisterName(groupDescriptionControls.Name, groupDescriptionControls);
            this.RegisterName(groupExpander.Name, groupExpander);

            this.Content = AnnotationGrid;
        }

        #region Events
        private void AnnotationView_Unloaded(object sender, RoutedEventArgs e)
        {
            Loaded -= AnnotationView_Loaded;
            DataContextChanged -= AnnotationView_DataContextChanged;
            PreviewMouseDoubleClick -= OnAnnotationDoubleClick;
            ViewModel.WorkspaceViewModel.InCanvasSearchViewModel.PropertyChanged -= OnSearchViewModelPropertyChanged;
            ViewModel.WorkspaceViewModel.Nodes.CollectionChanged -= OnWorkspaceNodesChanged;
            if (_groupContextMenuClosedHandler != null)
                GroupContextMenuPopup.Closed -= _groupContextMenuClosedHandler;

            if (groupTextBlock != null)
                groupTextBlock.SizeChanged -= GroupTextBlock_SizeChanged;
            if (collapsedAnnotationRectangle != null)
            {
                collapsedAnnotationRectangle.SizeChanged -= CollapsedAnnotationRectangle_SizeChanged;
                collapsedAnnotationRectangle.IsVisibleChanged -= CollapsedAnnotationRectangle_IsVisibleChanged;
            }
            if (groupTextBox != null)
            {
                groupTextBox.IsVisibleChanged -= GroupTextBox_OnIsVisibleChanged;
                groupTextBox.GotFocus -= GroupTextBox_OnGotFocus;
                groupTextBox.TextChanged -= GroupTextBox_OnTextChanged;
            }
            
            if (groupDescriptionTextBox != null)
            {
                groupDescriptionTextBox.IsVisibleChanged -= GroupDescriptionTextBox_OnIsVisibleChanged;
                groupDescriptionTextBox.GotFocus -= GroupDescriptionTextBox_GotFocus;
                groupDescriptionTextBox.TextChanged -= GroupDescriptionTextBox_TextChanged;
            }
            if (mainGroupThumb != null)
            {
                mainGroupThumb.DragDelta -= AnnotationRectangleThumb_DragDelta;
                mainGroupThumb.MouseEnter -= Thumb_MouseEnter;
                mainGroupThumb.MouseLeave -= Thumb_MouseLeave;
            }
            UnregisterNamesFromScope();
        }
        private void UnregisterNamesFromScope()
        {
            try
            {
                if (groupTextBlock?.Name != null)
                    this.UnregisterName(groupTextBlock.Name);

                if (groupTextBox?.Name != null)
                    this.UnregisterName(groupTextBox.Name);

                if (groupNameControl?.Name != null)
                    this.UnregisterName(groupNameControl.Name);

                if (groupDescriptionTextBlock?.Name != null)
                    this.UnregisterName(groupDescriptionTextBlock.Name);

                if (groupDescriptionTextBox?.Name != null)
                    this.UnregisterName(groupDescriptionTextBox.Name);

                if (groupDescriptionControls?.Name != null)
                    this.UnregisterName(groupDescriptionControls.Name);

                if (groupExpander?.Name != null)
                    this.UnregisterName(groupExpander.Name);
            }
            catch (ArgumentException)
            {
                // Name was already unregistered or never registered
                // This can happen during rapid load/unload cycles
            }
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

                // Create and attach the popup menu for group annotations
                CreateAndAttachAnnotationPopup();

                // Subscribe to search box activity to close the other popup items when typing
                var searchVM = ViewModel.WorkspaceViewModel.InCanvasSearchViewModel;
                searchVM.PropertyChanged += OnSearchViewModelPropertyChanged;

                // Add new nodes to group when search is triggered from group
                ViewModel.WorkspaceViewModel.Nodes.CollectionChanged += OnWorkspaceNodesChanged;

                // Reset group context flag when popup closes
                _groupContextMenuClosedHandler = (s, e) => isSearchFromGroupContext = false;
                GroupContextMenuPopup.Closed += _groupContextMenuClosedHandler;
            }
        }

        private void OnAnnotationDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Shift || Keyboard.Modifiers == ModifierKeys.Control)
                return;

            var workspace = WpfUtilities.FindParent<WorkspaceView>(this);
            if (workspace == null)
                return;

            var clickPosition = e.GetPosition(workspace.WorkspaceElements);
            var annotationModel = ViewModel.AnnotationModel;

            // Define the area below the text block where nodes reside
            var annoRectArea = new Rect(annotationModel.X, annotationModel.Y + annotationModel.TextBlockHeight, annotationModel.Width, annotationModel.ModelAreaHeight);

            // Only create CBN if click is in model area (not in the title/text area)
            if (!annoRectArea.Contains(clickPosition))
                return;

            workspace.ViewModel?.HandleAnnotationDoubleClick(clickPosition, annotationModel);
            e.Handled = true;
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
            if (groupTextBlock.IsVisible ||
                (!groupTextBlock.IsVisible && !groupTextBox.IsVisible))
            {
                ViewModel.SelectAll();
            }

            //When Textbox is visible,clear the selection. That way, models will not be added to
            //dragged nodes one more time. Ref: MAGN-7321
            if (groupTextBlock.IsVisible && e.ClickCount >= 2)
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
            if (!groupTextBlock.IsVisible && e.ClickCount >= 2)
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
        /// Handles the OnMouseRightButtonU event of the AnnotationView control - opens the context menu.
        /// </summary>
        private void AnnotationView_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!GroupContextMenuPopup.IsOpen)
            {
                OpenContextMenuAtMouse();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Handles the OnTextChanged event of the groupTextBox control.
        /// Calculates the height of a Group based on the height of textblock
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        private void GroupTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (ViewModel is null || !IsLoaded || _isUpdatingLayout) return;

            _isUpdatingLayout = true;

            // Use Dispatcher.BeginInvoke to batch layout updates
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    SetTextMaxWidth();
                    SetTextHeight();
                }
                finally
                {
                    _isUpdatingLayout = false;
                }
            }), DispatcherPriority.Background);

            if (groupTextBox.ActualHeight > 0 && groupTextBox.ActualWidth > 0)
            {
                ViewModel.WorkspaceViewModel.HasUnsavedChanges = true;
            }
        }

        /// <summary>
        /// Handles the SizeChanged event of the groupTextBlock control.
        /// This function calculates the height of a group based on font size
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SizeChangedEventArgs"/> instance containing the event data.</param>
        private void GroupTextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ViewModel != null && (e.HeightChanged || e.WidthChanged) && !_isUpdatingLayout)
            {
                _isUpdatingLayout = true;
                
                // Use Dispatcher.BeginInvoke to batch layout updates
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        SetTextMaxWidth();
                        SetTextHeight();
                        ViewModel.UpdateProxyPortsPosition();
                    }
                    finally
                    {
                        _isUpdatingLayout = false;
                    }
                }), DispatcherPriority.Background);
            }
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
                    groupTextBox.Text));

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
            this.groupTextBox.CaretIndex = Int32.MaxValue;
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
                    groupDescriptionTextBox.Text));

            ViewModel.WorkspaceViewModel.DynamoViewModel.RaiseCanExecuteUndoRedo();

            textbox.Focus();
            if (textbox.Text.Equals(Properties.Resources.GroupDefaultText))
            {
                textbox.SelectAll();
            }
        }

        private void GroupDescriptionTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            this.groupDescriptionTextBox.CaretIndex = Int32.MaxValue;
        }

        private void GroupDescriptionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ViewModel is null || !IsLoaded) return;

            SetTextHeight();
            if (groupDescriptionTextBox.ActualHeight > 0 && groupDescriptionTextBox.ActualWidth > 0)
            {
                ViewModel.WorkspaceViewModel.HasUnsavedChanges = true;
            }

        }

        private void CollapsedAnnotationRectangle_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetModelAreaHeight();
        }

        private void CollapsedAnnotationRectangle_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetModelAreaHeight();
        }

        private void contextMenu_Click(object sender, RoutedEventArgs e) 
        {
            ViewModel.SelectAll();
            OpenContextMenuAtMouse();
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

        #region Context Menu Logic

        private void OpenContextMenuAtMouse()
        {
            var workspaceView = WpfUtilities.FindParent<WorkspaceView>(this);
            if (workspaceView == null) return;

            var outerCanvas = workspaceView.OuterCanvas;
            if (outerCanvas == null) return;

            // Firstly, close the workspace context menu if it's open            
            if (workspaceView.ContextMenuPopup != null)
            {
                workspaceView.ContextMenuPopup.IsOpen = false;
            }

            isSearchFromGroupContext = true;

            var mousePosInOuter = Mouse.GetPosition(outerCanvas);

            // Convert mouse position from outerCanvas to WorkspaceElements
            var workspaceElements = workspaceView.FindName("WorkspaceElements") as UIElement;
            if (workspaceElements == null) return;

            var transform = outerCanvas.TransformToDescendant(workspaceElements);
            var mousePosInWorkspace = transform.Transform(mousePosInOuter);

            // Save correct position and reset the text
            var searchViewModel = ViewModel.WorkspaceViewModel.InCanvasSearchViewModel;
            searchViewModel.InCanvasSearchPosition = mousePosInWorkspace;
            searchViewModel.SearchText = string.Empty;

            // Rebuild content for dynamic state
            var border = new Border
            {
                Background = _midGreyBrush,
                Child = CreatePopupPanel()
            };
            border.PreviewKeyDown += GroupContextMenu_KeyDown;

            GroupContextMenuPopup.Child = border;
            GroupContextMenuPopup.PlacementTarget = outerCanvas;
            GroupContextMenuPopup.Placement = PlacementMode.MousePoint;
            GroupContextMenuPopup.IsOpen = true;
        }

        private void OnSearchViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(SearchViewModel.SearchText)) return;

            var searchText = ViewModel.WorkspaceViewModel.InCanvasSearchViewModel.SearchText;

            bool isSearching = !string.IsNullOrWhiteSpace(searchText);

            // Collapse/hide other popup items based on search
            foreach (var child in groupPopupPanel.Children)
            {
                // Skip the search box itself
                if (child is InCanvasSearchControl)
                    continue;

                if (child is UIElement element)
                    element.Visibility = isSearching ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private void OnWorkspaceNodesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!isSearchFromGroupContext) return;

            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems?.Count > 0)
            {
                if (e.NewItems[0] is NodeViewModel nodeViewModel)
                {
                    ViewModel.AnnotationModel.AddToTargetAnnotationModel(nodeViewModel.NodeModel);
                }
            }
        }

        private void OnNodeColorRectangleClicked(object sender, MouseButtonEventArgs e)
        {
            if (sender is Label label)
            {
                if (label.Background is SolidColorBrush brush)
                {
                    // Update the model background color
                    ViewModel.WorkspaceViewModel.DynamoViewModel.ExecuteCommand(
                        new DynCmd.UpdateModelValueCommand(
                            Guid.Empty,
                            ViewModel.AnnotationModel.GUID,
                            "Background",
                            brush.Color.ToString()));

                    // Update the ViewModel color
                    ViewModel.Background = brush.Color;

                    GroupContextMenuPopup.IsOpen = false;
                }
            }
        }

        private void GroupContextMenu_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.O && Keyboard.Modifiers == System.Windows.Input.ModifierKeys.None)
            {
                if (searchBar != null && searchBar.IsKeyboardFocusWithin) return;

                OnUngroupAnnotation(this, new RoutedEventArgs());
                e.Handled = true;
                GroupContextMenuPopup.IsOpen = false;
            }
        }

        #endregion

        #endregion

        #region Control Templates

        private ControlTemplate CreateGroupExpanderTemplate()
        {
            var template = new ControlTemplate(typeof(ToggleButton));

            // Create Border
            var borderFactory = new FrameworkElementFactory(typeof(Border), "expanderBorder");
            borderFactory.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Control.BackgroundProperty));
            borderFactory.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(Control.BorderBrushProperty));
            borderFactory.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(Control.BorderThicknessProperty));
            borderFactory.SetValue(Border.PaddingProperty, new TemplateBindingExtension(Control.PaddingProperty));

            // Create Grid
            var gridFactory = new FrameworkElementFactory(typeof(Grid));

            // Add ColumnDefinition
            var columnDef = new FrameworkElementFactory(typeof(ColumnDefinition));
            columnDef.SetValue(ColumnDefinition.WidthProperty, GridLength.Auto);
            gridFactory.AppendChild(columnDef);

            // Add Rectangle
            var rectangleFactory = new FrameworkElementFactory(typeof(Rectangle));
            rectangleFactory.SetValue(Rectangle.FillProperty, Brushes.Transparent);
            rectangleFactory.SetValue(Grid.ColumnSpanProperty, 1);
            gridFactory.AppendChild(rectangleFactory);

            // Add Image
            var imageFactory = new FrameworkElementFactory(typeof(Image), "sign");
            imageFactory.SetValue(Image.SourceProperty, _caretDownGreyImage);
            imageFactory.SetValue(FrameworkElement.WidthProperty, 16.0);
            imageFactory.SetValue(FrameworkElement.HeightProperty, 16.0);
            gridFactory.AppendChild(imageFactory);

            // Set Grid as Border's child
            borderFactory.AppendChild(gridFactory);

            // Set Border as template's visual tree
            template.VisualTree = borderFactory;

            // Add triggers
            AddExpanderTriggers(template);

            return template;
        }

        private void AddExpanderTriggers(ControlTemplate template)
        {
            // First MultiDataTrigger
            var trigger1 = new MultiDataTrigger();
            trigger1.Conditions.Add(new Condition(
                new Binding("IsMouseOver") { RelativeSource = RelativeSource.Self },
                false));
            trigger1.Conditions.Add(new Condition(
                new Binding("IsChecked") { RelativeSource = RelativeSource.Self },
                false));
            trigger1.Conditions.Add(new Condition(
                new Binding("Background") { Converter = _backgroundConditionEvaluator },
                false));
            trigger1.Setters.Add(new Setter(Image.SourceProperty, _caretDownGreyImage, "sign"));

            // Second MultiDataTrigger
            var trigger2 = new MultiDataTrigger();
            trigger2.Conditions.Add(new Condition(
                new Binding("IsMouseOver") { RelativeSource = RelativeSource.Self },
                false));
            trigger2.Conditions.Add(new Condition(
                new Binding("IsChecked") { RelativeSource = RelativeSource.Self },
                false));
            trigger2.Conditions.Add(new Condition(
                new Binding("Background") { Converter = _backgroundConditionEvaluator },
                true));
            trigger2.Setters.Add(new Setter(Image.SourceProperty, _caretDownWhiteImage, "sign"));

            // Third MultiDataTrigger
            var trigger3 = new MultiDataTrigger();
            trigger3.Conditions.Add(new Condition(
                new Binding("IsMouseOver") { RelativeSource = RelativeSource.Self },
                true));
            trigger3.Conditions.Add(new Condition(
                new Binding("IsChecked") { RelativeSource = RelativeSource.Self },
                false));
            trigger3.Setters.Add(new Setter(Image.SourceProperty, _caretDownHoverImage, "sign"));

            // Fourth MultiDataTrigger
            var trigger4 = new MultiDataTrigger();
            trigger4.Conditions.Add(new Condition(
                new Binding("IsMouseOver") { RelativeSource = RelativeSource.Self },
                false));
            trigger4.Conditions.Add(new Condition(
                new Binding("IsChecked") { RelativeSource = RelativeSource.Self },
                true));
            trigger4.Conditions.Add(new Condition(
                new Binding("Background") { Converter = _backgroundConditionEvaluator },
                false));
            trigger4.Setters.Add(new Setter(Image.SourceProperty, _caretUpGreyImage, "sign"));

            // Fifth MultiDataTrigger
            var trigger5 = new MultiDataTrigger();
            trigger5.Conditions.Add(new Condition(
                new Binding("IsMouseOver") { RelativeSource = RelativeSource.Self },
                false));
            trigger5.Conditions.Add(new Condition(
                new Binding("IsChecked") { RelativeSource = RelativeSource.Self },
                true));
            trigger5.Conditions.Add(new Condition(
                new Binding("Background") { Converter = _backgroundConditionEvaluator },
                true));
            trigger5.Setters.Add(new Setter(Image.SourceProperty, _caretUpWhiteImage, "sign"));

            // Sixth MultiDataTrigger
            var trigger6 = new MultiDataTrigger();
            trigger6.Conditions.Add(new Condition(
                new Binding("IsMouseOver") { RelativeSource = RelativeSource.Self },
                true));
            trigger6.Conditions.Add(new Condition(
                new Binding("IsChecked") { RelativeSource = RelativeSource.Self },
                true));
            trigger6.Setters.Add(new Setter(Image.SourceProperty, _caretUpHoverImage, "sign"));

            // Add all triggers to template
            template.Triggers.Add(trigger1);
            template.Triggers.Add(trigger2);
            template.Triggers.Add(trigger3);
            template.Triggers.Add(trigger4);
            template.Triggers.Add(trigger5);
            template.Triggers.Add(trigger6);
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

        private ControlTemplate CreateFrozenButtonZoomedInTemplate()
        {
            var template = new ControlTemplate(typeof(Button));

            // Create the Image
            var imageFactory = new FrameworkElementFactory(typeof(Image));
            imageFactory.Name = "FrozenImageZoomedIn";
            imageFactory.SetValue(Image.SourceProperty, _frozenDarkImage);

            template.VisualTree = imageFactory;

            // Create MouseOver trigger
            var mouseOverTrigger = new MultiDataTrigger();
            mouseOverTrigger.Conditions.Add(new Condition
            {
                Binding = new Binding("IsMouseOver")
                {
                    RelativeSource = new RelativeSource(RelativeSourceMode.Self)
                },
                Value = true
            });
            mouseOverTrigger.Setters.Add(new Setter(Image.SourceProperty, _frozenHoverImage, "FrozenImageZoomedIn"));

            // Create Background condition trigger
            var backgroundTrigger = new MultiDataTrigger();
            backgroundTrigger.Conditions.Add(new Condition
            {
                Binding = new Binding("DataContext.Background")
                {
                    RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(UserControl), 1),
                    Converter = _backgroundConditionEvaluator
                },
                Value = true
            });
            backgroundTrigger.Conditions.Add(new Condition
            {
                Binding = new Binding("IsMouseOver")
                {
                    RelativeSource = new RelativeSource(RelativeSourceMode.Self)
                },
                Value = false
            });
            backgroundTrigger.Setters.Add(new Setter(Image.SourceProperty, _frozenLightImage, "FrozenImageZoomedIn"));

            // Add triggers to template
            template.Triggers.Add(mouseOverTrigger);
            template.Triggers.Add(backgroundTrigger);

            return template;
        }
        #endregion

        #region Setup UI

        #region Frozen Zoom Button

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
            button.SetBinding(Button.CommandProperty, new Binding("ToggleIsFrozenGroupCommand"));

            // Add the Button to the Grid
            grid.Children.Add(button);
            frozenButtonZoomedOutGrid = grid;
            return frozenButtonZoomedOutGrid;
        }

        #endregion

        #region Borders
        /// <summary>
        /// Creates a persistent border that will always be visible around the annotation.
        /// </summary>
        /// <returns></returns>
        private Border CreatePersistentBorder()
        {
            var persistentBorder = new Border
            {
                Name = "persistentBorder",
                BorderThickness = new Thickness(2),
                CornerRadius = _cornerRadius,
                IsHitTestVisible = false,
                Margin = new Thickness(-1)
            };

            // Set Grid properties
            Grid.SetRowSpan(persistentBorder, 2);
            Canvas.SetZIndex(persistentBorder, 41);

            persistentBorder.SetBinding(Border.BorderBrushProperty, new Binding("Background"));
            persistentBorder.SetBinding(Border.VisibilityProperty, new Binding("Nodes")
            {
                Converter = _listHasMoreThanNItemsToVisibilityConverter
            });

            return persistentBorder;
        }

        /// <summary>
        /// Creates a selection border that will be visible when the annotation is selected.
        /// </summary>
        /// <returns></returns>
        private Border CreateSelectionBorder()
        {
            var selectionBorder = new Border
            {
                Name = "selectionBorder",
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(2),
                CornerRadius = _cornerRadius,
                IsHitTestVisible = false,
                Margin = new Thickness(-1)
            };

            Grid.SetRowSpan(selectionBorder, 2);
            Canvas.SetZIndex(selectionBorder, 41);

            selectionBorder.SetBinding(Border.BorderBrushProperty, new Binding("PreviewState")
            {
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Mode = BindingMode.OneWay,
                Converter = _connectionStateToBrushConverter
            });

            selectionBorder.SetBinding(Border.VisibilityProperty, new Binding("PreviewState")
            {
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Mode = BindingMode.OneWay,
                Converter = _connectionStateToVisibilityCollapsedConverter
            });

            return selectionBorder;
        }

        /// <summary>
        /// Creates a border that will be visible when the node is being hovered over.
        /// If a node is dragged over this group (and that node does not already belong to another group)
        /// this border is activated, indicating that the node can be dropped into the group.
        /// The visibility of this is controlled by the AnnotationViewModel property 'NodeHoveringState'
        /// which is set in the StateMachine.
        /// </summary>
        /// <returns></returns>
        private Border CreateNodeHoverBorder()
        {
            var nodeHoverBorder = new Border
            {
                Name = "nodeHoveringStateBorder",
                Background = Brushes.Transparent,
                BorderBrush = _primaryCharcoal300,
                BorderThickness = new Thickness(6),
                CornerRadius = _cornerRadius,
                IsHitTestVisible = false,
                Margin = new Thickness(-4)
            };

            Grid.SetRowSpan(nodeHoverBorder, 2);
            Canvas.SetZIndex(nodeHoverBorder, 41);

            nodeHoverBorder.SetBinding(Border.VisibilityProperty, new Binding("NodeHoveringState")
            {
                Converter = _boolToVisibilityCollapsedConverter
            });

            return nodeHoverBorder;
        }
        #endregion

        #region Group Expander

        private Expander CreateGroupExpander()
        {
            groupExpander = new Expander
            {
                Name = "GroupExpander"
            };

            Grid.SetRow(groupExpander, 0);

            groupExpander.SetBinding(FrameworkElement.WidthProperty, new Binding("Width"));
            groupExpander.SetBinding(Expander.IsExpandedProperty, new Binding("IsExpanded")
            {
                Mode = BindingMode.TwoWay
            });

            textBlockGrid = new Grid
            {
                Name = "textBlockGrid",
                Height = double.NaN,  // "Auto" in XAML
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(5, 0, 5, 0)
            };

            Grid.SetRow(textBlockGrid, 0);

            textBlockGrid.SetBinding(FrameworkElement.MaxWidthProperty, new Binding("Width"));

            // Add RowDefinitions
            textBlockGrid.RowDefinitions.Add(new RowDefinition
            {
                Name = "GroupNameRow",
                Height = GridLength.Auto
            });
            textBlockGrid.RowDefinitions.Add(new RowDefinition
            {
                Name = "GroupDescriptionRow",
                Height = GridLength.Auto
            });

            textBlockGrid.Children.Add(CreateGroupNameControl());
            textBlockGrid.Children.Add(CreateGroupDescriptionControls());
            SetupGridTriggers();

            groupExpander.Header = textBlockGrid;
            groupExpander.Content = CreateAnnotationGrid();

            return groupExpander;
        }
        private ContentControl CreateGroupNameControl()
        {
            groupNameControl = new ContentControl
            {
                Name = "groupNameControl",
                MinHeight = 20,
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid.SetRow(groupNameControl, 0);
            groupNameControl.SetBinding(FrameworkElement.WidthProperty, new Binding("Width"));

            var grid = new Grid
            {
                Margin = new Thickness(0, 0, 20, 0)
            };

            //Group name text controls
            groupTextBlock = new TextBlock
            {
                Name = "groupTextBlock",
                FontFamily = new FontFamily("Trebuchet"),
                LineStackingStrategy = LineStackingStrategy.BlockLineHeight,
                TextWrapping = TextWrapping.WrapWithOverflow,
                Visibility = Visibility.Visible
            };
            groupTextBlock.SetBinding(TextBlock.TextProperty, new Binding("AnnotationText")
            {
                Converter = _annotationTextConverter,
                ConverterParameter = "TextBlock"
            });
            groupTextBlock.SetBinding(TextBlock.ForegroundProperty, new Binding("Background")
            {
                Converter = _textForegroundSaturationColorConverter
            });
            groupTextBlock.SetBinding(TextBlock.FontSizeProperty, new Binding("FontSize"));

            groupTextBox = new TextBox
            {
                Name = "groupTextBox",
                FontFamily = new FontFamily("Trebuchet"),
                TextWrapping = TextWrapping.WrapWithOverflow,
                Visibility = Visibility.Collapsed,
                AcceptsReturn = true,
                AcceptsTab = true
            };
            Grid.SetColumn(groupTextBox, 0);
            groupTextBox.SetBinding(TextBox.TextProperty, new Binding("AnnotationText")
            {
                Converter = _annotationTextConverter,
                ConverterParameter = "TextBox"
            });
            groupTextBox.SetBinding(TextBox.FontSizeProperty, new Binding("FontSize"));
            groupTextBox.IsVisibleChanged += GroupTextBox_OnIsVisibleChanged;
            groupTextBox.GotFocus += GroupTextBox_OnGotFocus;
            groupTextBox.TextChanged += GroupTextBox_OnTextChanged;

            // Add controls to the grid
            grid.Children.Add(groupTextBlock);
            grid.Children.Add(groupTextBox);

            groupNameControl.Content = grid;

            return groupNameControl;
        }
        private ContentControl CreateGroupDescriptionControls()
        {
            groupDescriptionControls = new ContentControl
            {
                Name = "groupDescriptionControls",
                MinHeight = 20,
                Margin = new Thickness(0, -10, 30, 0)
            };

            Grid.SetRow(groupDescriptionControls, 1);

            // Add event handlers
            groupDescriptionControls.SizeChanged += GroupDescriptionControls_SizeChanged;
            groupDescriptionControls.IsVisibleChanged += GroupDescriptionTextBlock_IsVisibleChanged;

            // Create the inner Grid
            var grid = new Grid();

            groupDescriptionTextBlock = new TextBlock
            {
                Name = "groupDescriptionTextBlock",
                FontFamily = new FontFamily("Trebuchet"),
                FontSize = 12,
                LineStackingStrategy = LineStackingStrategy.BlockLineHeight,
                TextWrapping = TextWrapping.Wrap,
                Visibility = Visibility.Visible
            };
            groupDescriptionTextBlock.SetBinding(TextBlock.TextProperty, new Binding("AnnotationDescriptionText")
            {
                Converter = _annotationTextConverter
            });
            groupDescriptionTextBlock.SetBinding(TextBlock.ForegroundProperty, new Binding("Background")
            {
                Converter = _textForegroundSaturationColorConverter
            });

            groupDescriptionTextBox = new TextBox
            {
                Name = "groupDescriptionTextBox",
                FontFamily = new FontFamily("Trebuchet"),
                FontSize = 12,
                TextWrapping = TextWrapping.Wrap,
                Visibility = Visibility.Collapsed,
                AcceptsReturn = true,
                AcceptsTab = true
            };
            groupDescriptionTextBox.SetBinding(TextBox.TextProperty, new Binding("AnnotationDescriptionText")
            {
                Converter = _annotationTextConverter
            });
            groupDescriptionTextBox.IsVisibleChanged += GroupDescriptionTextBox_OnIsVisibleChanged;
            groupDescriptionTextBox.GotFocus += GroupDescriptionTextBox_GotFocus;
            groupDescriptionTextBox.TextChanged += GroupDescriptionTextBox_TextChanged;

            var style = new Style(typeof(ContentControl));

            // Add default visibility setter
            style.Setters.Add(new Setter(UIElement.VisibilityProperty, Visibility.Collapsed));

            // Create DataTrigger for GroupExpander.IsExpanded
            var dataTrigger = new DataTrigger
            {
                Binding = new Binding("IsExpanded") { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Expander), 1) },
                Value = true
            };
            dataTrigger.Setters.Add(new Setter(UIElement.VisibilityProperty, Visibility.Visible));
            style.Triggers.Add(dataTrigger);

            // Add controls to the grid
            grid.Children.Add(groupDescriptionTextBlock);
            grid.Children.Add(groupDescriptionTextBox);

            groupDescriptionControls.Content = grid;
            groupDescriptionControls.Style = style;

            return groupDescriptionControls;
        }

        private void SetupGridTriggers()
        {
            // Add double-click triggers
            AddTrigger(ContentControl.MouseDoubleClickEvent, "groupNameControl", "groupTextBlock", "groupTextBox", true);
            AddTrigger(ContentControl.MouseDoubleClickEvent, "groupDescriptionControls", "groupDescriptionTextBlock", "groupDescriptionTextBox", true);

            // Add lost focus triggers
            AddTrigger(TextBox.LostKeyboardFocusEvent, "groupTextBox", "groupTextBlock", "groupTextBox", false);
            AddTrigger(TextBox.LostKeyboardFocusEvent, "groupDescriptionTextBox", "groupDescriptionTextBlock", "groupDescriptionTextBox", false);
        }
        private Storyboard CreateStoryboard(string textBlockName, string textBoxName, bool isDoubleClick)
        {
            var storyboard = new Storyboard();

            // Add animations
            AddAnimation(storyboard, textBlockName, "TextBlock", isDoubleClick, true);
            AddAnimation(storyboard, textBoxName, "TextBox", isDoubleClick, true);
            AddAnimation(storyboard, textBoxName, "TextBox", isDoubleClick, false);

            return storyboard;
        }
        private void AddTrigger(RoutedEvent routedEvent, string sourceName, string textBlockName, string textBoxName, bool isDoubleClick)
        {
            var trigger = new EventTrigger
            {
                RoutedEvent = routedEvent,
                SourceName = sourceName
            };
            var storyboard = CreateStoryboard(textBlockName, textBoxName, isDoubleClick);
            trigger.Actions.Add(new BeginStoryboard { Storyboard = storyboard });
            textBlockGrid.Triggers.Add(trigger);
        }
        private void AddAnimation(Storyboard storyboard, string targetName, string targetType, bool isDoubleClick, bool isVisibility)
        {
            if (isVisibility)
            {
                var visibilityAnimation = new ObjectAnimationUsingKeyFrames();
                Storyboard.SetTargetName(visibilityAnimation, targetName);
                Storyboard.SetTargetProperty(visibilityAnimation, new PropertyPath($"({targetType}.Visibility)"));
                visibilityAnimation.Duration = TimeSpan.Zero;

                var keyFrame = new DiscreteObjectKeyFrame
                {
                    KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero)
                };

                if (isDoubleClick)
                {
                    // Use the static converter instance
                    var visibility = targetType == "TextBlock" ?
                        _groupTitleVisibilityConverter.Convert(Visibility.Visible, typeof(Visibility), "FlipTextblock", null) :
                        _groupTitleVisibilityConverter.Convert(Visibility.Visible, typeof(Visibility), "FlipTextbox", null);

                    keyFrame.Value = visibility;
                }
                else
                {
                    // For lost focus, we use direct Visibility values
                    keyFrame.Value = targetType == "TextBlock" ? Visibility.Visible : Visibility.Collapsed;
                }

                visibilityAnimation.KeyFrames.Add(keyFrame);
                storyboard.Children.Add(visibilityAnimation);
            }
            else
            {
                var focusableAnimation = new BooleanAnimationUsingKeyFrames();
                Storyboard.SetTargetName(focusableAnimation, targetName);
                Storyboard.SetTargetProperty(focusableAnimation, new PropertyPath("(TextBox.Focusable)"));

                var focusableKeyFrame = new DiscreteBooleanKeyFrame
                {
                    KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
                    Value = true
                };

                focusableAnimation.KeyFrames.Add(focusableKeyFrame);
                storyboard.Children.Add(focusableAnimation);
            }
        }

        #endregion

        #region Main Grid

        private Grid CreateAnnotationGrid()
        {
            var grid = new Grid();

            // Add the Canvas
            var annotationRectangle = CreateAnnotationRectangle();
            grid.Children.Add(annotationRectangle);

            // Add the Resize Thumb
            mainGroupThumb = CreateResizeThumb();
            grid.Children.Add(mainGroupThumb);

            return grid;
        }

        private Canvas CreateAnnotationRectangle()
        {
            var canvas = new Canvas
            {
                Name = "AnnotationRectangle",
                IsHitTestVisible = true,
                Background = Brushes.Transparent,
                Opacity = 0.5
            };

            // Set bindings
            canvas.SetBinding(FrameworkElement.WidthProperty, new Binding("Width"));
            canvas.SetBinding(FrameworkElement.HeightProperty, new Binding("ModelAreaHeight"));
            canvas.SetBinding(UIElement.VisibilityProperty, new Binding("IsExpanded")
            {
                Converter = _boolToVisibilityCollapsedConverter
            });

            // Create and add the Path
            var path = new Path
            {
                Stroke = Brushes.Transparent,
                StrokeThickness = 0
            };

            var brush = new SolidColorBrush();
            BindingOperations.SetBinding(brush, SolidColorBrush.ColorProperty,
                new Binding("Background"));
            path.Fill = brush;

            var combinedGeometry = new CombinedGeometry
            {
                GeometryCombineMode = GeometryCombineMode.Exclude
            };

            var rectGeometry = new RectangleGeometry();
            BindingOperations.SetBinding(rectGeometry, RectangleGeometry.RectProperty,
                new Binding("ModelAreaRect"));
            combinedGeometry.Geometry1 = rectGeometry;

            var geometryGroup = new GeometryGroup { FillRule = FillRule.EvenOdd };

            BindingOperations.SetBinding(geometryGroup, GeometryGroup.ChildrenProperty,
                new Binding("NestedGroupsGeometries"));

            combinedGeometry.Geometry2 = geometryGroup;

            path.Data = combinedGeometry;

            canvas.Children.Add(path);

            return canvas;
        }

        private Thumb CreateResizeThumb()
        {
            var thumb = new Thumb();
            thumb.Name = "ResizeThumb";
            thumb.DragDelta += AnnotationRectangleThumb_DragDelta;
            

            thumb.Style = _groupResizeThumbStyle;
            thumb.MouseEnter += Thumb_MouseEnter;
            thumb.MouseLeave += Thumb_MouseLeave;

            return thumb;
        }

        private static Style CreateGroupResizeThumbStyle()
        {
            var style = new Style(typeof(Thumb));

            // Add basic property setters
            style.Setters.Add(new Setter(UIElement.SnapsToDevicePixelsProperty, true));
            style.Setters.Add(new Setter(FrameworkElement.MarginProperty, new Thickness(0, 0, 2.5, 2.5)));
            style.Setters.Add(new Setter(FrameworkElement.WidthProperty, 10.0));
            style.Setters.Add(new Setter(FrameworkElement.HeightProperty, 10.0));
            style.Setters.Add(new Setter(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Right));
            style.Setters.Add(new Setter(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Bottom));

            // Create the control template
            var template = new ControlTemplate(typeof(Thumb));

            // Create the Polygon
            var polygonFactory = new FrameworkElementFactory(typeof(Polygon));
            
            // Set Points collection
            var points = new PointCollection
            {
                new Point(0, 8),
                new Point(8, 8),
                new Point(8, 0)
            };
            polygonFactory.SetValue(Polygon.PointsProperty, points);

            // Set Fill binding
            polygonFactory.SetBinding(Polygon.FillProperty, new Binding("DataContext.Background")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(UserControl), 1),
                Converter = _textForegroundSaturationColorConverter,
                ConverterParameter = "Thumb"
            });

            // Set the template's visual tree
            template.VisualTree = polygonFactory;

            // Add the template setter to the style
            style.Setters.Add(new Setter(Control.TemplateProperty, template));

            return style;
        }

        #endregion

        #region Collapsed controls

        private Border CreateCollapsedAnnotationRectangle()
        {
            var border = new Border
            {
                Name = "collapsedAnnotationRectangle"
            };

            // Set bindings
            border.SetBinding(UIElement.VisibilityProperty, new Binding("IsExpanded")
            {
                ElementName = "GroupExpander",
                Converter = _inverseBooleanToVisibilityCollapsedConverter
            });

            // Set Grid.Row
            Grid.SetRow(border, 1);

            // Set Background
            var backgroundBrush = new SolidColorBrush();
            BindingOperations.SetBinding(backgroundBrush, SolidColorBrush.ColorProperty,
                new Binding("Background"));
            backgroundBrush.Opacity = 0.5;
            border.Background = backgroundBrush;

            // Create and set content
            border.Child = CreateCollapsedMainGrid();

            collapsedAnnotationRectangle = border;

            return collapsedAnnotationRectangle;
        }

        private Grid CreateCollapsedMainGrid()
        {
            var grid = new Grid();

            // Add column definitions
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Add row definitions
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Add children
            inputPortControl = CreateInputPortControl();
            outputPortControl = CreateOutputPortControl();
            var groupContent = CreateGroupContent();

            grid.Children.Add(inputPortControl);
            grid.Children.Add(outputPortControl);
            grid.Children.Add(groupContent);

            return grid;
        }

        private ItemsControl CreateInputPortControl()
        {
            var itemsControl = new ItemsControl
            {
                Name = "inputPortControl",
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(-25, 10, -25, 10)
            };

            Panel.SetZIndex(itemsControl, 20);
            Grid.SetColumn(itemsControl, 0);
            Grid.SetRow(itemsControl, 0);

            itemsControl.SetBinding(ItemsControl.ItemsSourceProperty, new Binding("InPorts"));

            return itemsControl;
        }

        private ItemsControl CreateOutputPortControl()
        {
            var itemsControl = new ItemsControl
            {
                Name = "outputPortControl",
                HorizontalContentAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(-25, 10, -25, 10)
            };

            Panel.SetZIndex(itemsControl, 20);
            Grid.SetColumn(itemsControl, 2);
            Grid.SetRow(itemsControl, 0);

            itemsControl.SetBinding(ItemsControl.ItemsSourceProperty, new Binding("OutPorts"));

            return itemsControl;
        }

        private Grid CreateGroupContent()
        {
            var grid = new Grid
            {
                Name = "GroupContent"
            };

            Grid.SetRow(grid, 1);
            Grid.SetColumnSpan(grid, 3);

            // Add column definitions
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Add children
            var nestedGroupsLabel = CreateNestedGroupsLabel();
            var nodeCountBorder = CreateNodeCountBorder();

            grid.Children.Add(nestedGroupsLabel);
            grid.Children.Add(nodeCountBorder);

            return grid;
        }

        private Label CreateNestedGroupsLabel()
        {
            var label = new Label
            {
                Name = "NestedGroups",
                Margin = new Thickness(0),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            Grid.SetColumn(label, 0);

            label.SetBinding(Label.ContentProperty, new Binding("NestedGroups")
            {
                Converter = _nestedGroupsLabelConverter
            });

            ToolTipService.SetShowOnDisabled(label, false);
            label.SetBinding(ToolTipService.IsEnabledProperty, new Binding("NestedGroups")
            {
                Converter = _collectionHasMoreThanNItemsToBoolConverter
            });

            // Create and set tooltip
            label.ToolTip = CreateNestedGroupsTooltip();

            return label;
        }

        private DynamoToolTip CreateNestedGroupsTooltip()
        {
            var tooltip = new DynamoToolTip
            {
                AttachmentSide = DynamoToolTip.Side.Bottom,
                Style = _dynamoToolTipTopStyle,
                Margin = new Thickness(5, 0, 0, 0)
            };

            var listView = new ListView();
            listView.SetBinding(ListView.ItemsSourceProperty, new Binding("NestedGroups"));

            // Create and set ListView style
            var listViewStyle = new Style(typeof(ListView));
            listViewStyle.Setters.Add(new Setter(ListView.BackgroundProperty, Brushes.Transparent));
            listViewStyle.Setters.Add(new Setter(ListView.BorderThicknessProperty, new Thickness(0)));

            // Create data trigger
            var dataTrigger = new DataTrigger
            {
                Binding = new Binding("NestedGroups")
                {
                    Converter = _collectionHasMoreThanNItemsToBoolConverter
                },
                Value = true
            };

            // Create data template
            var dataTemplate = new DataTemplate(typeof(AnnotationViewModel));
            var textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
            textBlockFactory.SetBinding(TextBlock.TextProperty, new Binding("AnnotationText"));
            dataTemplate.VisualTree = textBlockFactory;

            dataTrigger.Setters.Add(new Setter(ListView.ItemTemplateProperty, dataTemplate));
            listViewStyle.Triggers.Add(dataTrigger);

            listView.Style = listViewStyle;
            tooltip.Content = listView;

            return tooltip;
        }

        private Border CreateNodeCountBorder()
        {
            var border = new Border
            {
                Name = "NodeCount",
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                Background = new SolidColorBrush(Color.FromRgb(238, 238, 238)),
                MinHeight = 32,
                MinWidth = 32,
                Width = Double.NaN,
                CornerRadius = new CornerRadius(32),
                Margin = new Thickness(10),
                VerticalAlignment = VerticalAlignment.Bottom,
            };

            Grid.SetColumn(border, 1);

            // Create and add TextBlock
            var textBlock = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 14
            };

            textBlock.SetBinding(TextBlock.TextProperty, new Binding("NodeContentCount")
            {
                Mode = BindingMode.OneWay,
                StringFormat = "+{0}"
            });

            border.Child = textBlock;

            return border;
        }

        #endregion

        #region Context Menu

        private void CreateAndAttachAnnotationPopup()
        {
            GroupContextMenuPopup = new Popup
            {
                Name = "AnnotationContextPopup",
                Placement = PlacementMode.MousePoint,
                StaysOpen = false,
                AllowsTransparency = true,
                PopupAnimation = PopupAnimation.Fade
            };

            // Wrap in border to apply outer padding
            var border = new Border
            {
                Background = _midGreyBrush,
                Child = CreatePopupPanel()
            };

            GroupContextMenuPopup.Child = border;
        }

        private StackPanel CreatePopupPanel()
        {
            groupPopupPanel = new StackPanel
            {
                Background = _midGreyBrush,
                Orientation = Orientation.Vertical
            };

            // Show search box only if the group is expanded
            if (ViewModel.IsExpanded)
            {
                groupPopupPanel.Children.Add(CreateSearchBox());
            }            

            // Add "Delete Group" menu item with extra top margin to separate it visually from the search box
            var deleteGroupItem = CreatePopupItem(
                Wpf.Properties.Resources.GroupContextMenuDeleteGroup,
                () => OnDeleteAnnotation(this, new RoutedEventArgs()));
            var deleteGroupWrapper = new Border()
            {
                Margin = new Thickness(0, 10, 0, 0),
                Child = deleteGroupItem
            };
            groupPopupPanel.Children.Add(deleteGroupWrapper);

            groupPopupPanel.Children.Add(CreatePopupItem(
                Wpf.Properties.Resources.GroupContextMenuFreezeGroup,
                () => ViewModel.ToggleIsFrozenGroupCommand?.Execute(null),
                () => ViewModel.AnnotationModel.IsFrozen));

            groupPopupPanel.Children.Add(CreatePopupItem(
                Wpf.Properties.Resources.GroupContextMenuUngroup,
                () => OnUngroupAnnotation(this, new RoutedEventArgs())));

            groupPopupPanel.Children.Add(CreatePopupItem(
                Wpf.Properties.Resources.GroupContextMenuPreview,
                () => ViewModel.ToggleIsVisibleGroupCommand?.Execute(null),
                () => ViewModel.AnnotationModel.IsVisible));

            groupPopupPanel.Children.Add(CreatePopupItem(
                Wpf.Properties.Resources.ContextUnGroupFromSelection,
                () => ViewModel.RemoveGroupFromGroupCommand?.Execute(null),
                isEnabled: ViewModel.CanUngroupGroup(ViewModel.AnnotationModel)));

            groupPopupPanel.Children.Add(CreatePopupItem(
                Wpf.Properties.Resources.GroupContextMenuAddGroupToGroup,
                () => ViewModel.AddGroupToGroupCommand?.Execute(null),
                isEnabled: ViewModel.CanAddGroupToGroup(ViewModel.AnnotationModel)));

            groupPopupPanel.Children.Add(CreatePopupItem(
                Wpf.Properties.Resources.GroupContextMenuGraphLayout,
                () => OnGraphLayoutAnnotation(this, new RoutedEventArgs())));

            GroupStyleSelectorGrid = CreateSubmenuItem(
                Wpf.Properties.Resources.GroupStyleContextAnnotation,
                CreateGroupStyleSelector);
            groupPopupPanel.Children.Add(GroupStyleSelectorGrid);

            groupPopupPanel.Children.Add(CreateSubmenuItem(
                Wpf.Properties.Resources.GroupContextMenuColor,
                CreateColorSelector));

            // Add "Delete Group" menu item with extra bottom margin
            var fontSizeItem = CreateSubmenuItem(
                Wpf.Properties.Resources.GroupContextMenuFont,
                CreateFontSizeSelector);
            var fontSizeWrapper = new Border()
            {
                Margin = new Thickness(0, 0, 0, 10),
                Child = fontSizeItem
            };
            groupPopupPanel.Children.Add(fontSizeWrapper);

            return groupPopupPanel;
        }

        private UIElement CreateSearchBox()
        {
            searchBar = new InCanvasSearchControl
            {
                DataContext = ViewModel.WorkspaceViewModel.InCanvasSearchViewModel,
                Width = 230
            };

            // Close the group context menu when the search box requests to hide itself
            searchBar.RequestShowInCanvasSearch += flags =>
            {
                if (flags.HasFlag(ShowHideFlags.Hide))
                {
                    GroupContextMenuPopup.IsOpen = false;
                }
            };

            return searchBar;
        }

        private UIElement CreatePopupItem(string label, Action onClick, Func<bool> isChecked = null, bool isEnabled = true)
        {
            bool checkedState = isChecked?.Invoke() ?? false;

            // Optional checkmark
            var check = new TextBlock
            {
                Text = checkedState ? "✓" : "",
                FontSize = 11,
                FontFamily = _artifaktElementRegular,
                Foreground = checkedState ? _blue300Brush : Brushes.Transparent,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(-7, 0, 0, 0),
                Width = 10, // reserve space so layout doesn't shift
            };

            // Main label
            var text = new AccessText
            {
                Text = label,
                FontSize = 13,
                FontFamily = checkedState ? _artifaktElementBold : _artifaktElementRegular,
                Foreground = _nodeContextMenuForeground,
                VerticalAlignment = VerticalAlignment.Center,
                Opacity = isEnabled ? 1.0 : 0.5
            };

            var rowGrid = new Grid();
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10) });
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            Grid.SetColumn(check, 0);
            Grid.SetColumn(text, 1);
            rowGrid.Children.Add(check);
            rowGrid.Children.Add(text);

            var border = WrapWithMenuBorder(rowGrid, () =>
            {
                if (isEnabled)
                {
                    onClick?.Invoke();

                    // Refresh visual if it's checkable
                    if (isChecked != null)
                    {
                        bool nowChecked = isChecked();
                        check.Text = nowChecked ? "✓" : "";
                        check.Foreground = nowChecked ? _blue300Brush : Brushes.Transparent;
                        text.FontFamily = nowChecked ? _artifaktElementBold : _artifaktElementRegular;
                    }
                }
            }, isEnabled);

            border.Focusable = true;
            border.FocusVisualStyle = null;

            border.MouseEnter += (s, e) =>
            {
                if (s is UIElement element) element.Focus();
            };

            return border;
        }

        private Grid CreateSubmenuItem(string label, Func<UIElement> submenuContentFactory)
        {
            var popup = new Popup
            {
                Placement = PlacementMode.Right,
                AllowsTransparency = true,
                StaysOpen = true,
                PopupAnimation = PopupAnimation.Fade
            };

            var arrow = new TextBlock
            {
                Text = ">",
                FontSize = 13,
                FontFamily = _artifaktElementRegular,
                Foreground = _blue300Brush,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right,
                RenderTransform = new ScaleTransform(1, 1.5)
            };

            var text = new TextBlock
            {
                Text = label,
                FontSize = 13,
                FontFamily = _artifaktElementRegular,
                Foreground = _nodeContextMenuForeground,
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
            };

            var layoutGrid = new Grid();
            layoutGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            layoutGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(25) });

            Grid.SetColumn(text, 0);
            Grid.SetColumn(arrow, 1);
            layoutGrid.Children.Add(text);
            layoutGrid.Children.Add(arrow);

            var border = WrapWithMenuBorder(layoutGrid);

            border.MouseEnter += (s, e) =>
            {
                popup.Child = submenuContentFactory.Invoke();
                popup.PlacementTarget = border;
                popup.IsOpen = true;
                border.Background = _nodeContextMenuBackgroundHighlight;
            };

            border.MouseLeave += (s, e) =>
            {
                if (!popup.IsMouseOver)
                {
                    popup.IsOpen = false;
                    border.Background = Brushes.Transparent;
                }
            };

            return new Grid { Children = { border, popup } };
        }

        private UIElement CreateColorSelector()
        {
            var panel = new UniformGrid
            {
                Rows = 2,
                Columns = 8,
                Margin = new Thickness(5)
            };

            foreach (var color in new[]
            {
                "#d4b6db", "#ffb8d8", "#ffc999", "#e8f7ad", "#b9f9e1", "#a4e1ff", "#b5b5b5", "#FFFFFF",
                "#bb87c6", "#ff7bac", "#ffaa45", "#c1d676", "#71c6a8", "#48b9ff", "#848484", "#d8d8d8"
            })
            {
                var rect = CreateColorSwatch(color);
                rect.MouseLeftButtonUp += OnNodeColorRectangleClicked;
                panel.Children.Add(rect);
            }

            return new Border
            {
                Background = _midGreyBrush,
                Padding = new Thickness(10, 5, 10, 5),
                Child = panel
            };
        }

        private UIElement CreateFontSizeSelector()
        {
            var stack = new StackPanel { Orientation = Orientation.Vertical };
            foreach (var size in new[] { 14, 18, 24, 30, 36, 48, 60, 72, 96 })
            {
                var item = CreatePopupItem(
                    size.ToString(),
                    () => ViewModel.ChangeFontSize?.Execute(size.ToString()),
                    () => ViewModel.FontSize == size,
                    isEnabled: true
                    );

                // Override width for each item individually
                if (item is Border border)
                {
                    border.MinWidth = 150;
                }

                stack.Children.Add(item);
            }

            return new Border
            {
                Background = _midGreyBrush,
                Padding = new Thickness(0, 5, 0, 5),
                Child = stack
            };
        }

        private UIElement CreateGroupStyleSelector()
        {
            ViewModel.ReloadGroupStyles();

            var stack = new StackPanel { Orientation = Orientation.Vertical };

            foreach (var groupStyle in ViewModel.GroupStyleList)
            {

                // Handle separator
                if (groupStyle is GroupStyleSeparator)
                {
                    var separator = new Border
                    {
                        Height = 1,
                        Margin = new Thickness(0, 5, 0, 5),
                        Background = Brushes.Gray
                    };
                    stack.Children.Add(separator);
                    continue;
                }

                var rect = CreateColorSwatch("#" + groupStyle.HexColorString);

                var label = new TextBlock
                {
                    Text = groupStyle.Name,
                    FontFamily = _artifaktElementRegular,
                    FontSize = 13,
                    Margin = new Thickness(5, 0, 0, 0),
                    Foreground = _nodeContextMenuForeground,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var innerStack = new StackPanel { Orientation = Orientation.Horizontal };
                innerStack.Children.Add(rect);
                innerStack.Children.Add(label);

                var border = new Border
                {
                    Background = Brushes.Transparent,
                    MinWidth = 150,
                    Child = innerStack
                };

                border.MouseEnter += (s, e) => border.Background = _nodeContextMenuBackgroundHighlight;
                border.MouseLeave += (s, e) => border.Background = Brushes.Transparent;

                border.MouseLeftButtonUp += (s, e) =>
                {
                    ViewModel.AnnotationModel.GroupStyleId = groupStyle.GroupStyleId;
                    ViewModel.GroupStyleId = groupStyle.GroupStyleId;

                    // Apply background color
                    if (!string.IsNullOrEmpty(groupStyle.HexColorString))
                    {
                        ViewModel.WorkspaceViewModel.DynamoViewModel.ExecuteCommand(
                            new DynCmd.UpdateModelValueCommand(
                                Guid.Empty,
                                ViewModel.AnnotationModel.GUID,
                                "Background",
                                "#" + groupStyle.HexColorString));

                        ViewModel.Background = (Color)ColorConverter.ConvertFromString("#" + groupStyle.HexColorString);
                    }

                    GroupContextMenuPopup.IsOpen = false;
                };

                stack.Children.Add(border);
            }

            return new Border
            {
                Background = _midGreyBrush,
                Padding = new Thickness(10),
                Child = stack
            };
        }

        private Border WrapWithMenuBorder(UIElement content, Action onClick = null, bool isEnabled = true)
        {
            var border = new Border
            {
                Padding = new Thickness(15, 5, 15, 5),
                Background = Brushes.Transparent,
                MinWidth = 250,
                Child = content
            };

            if (isEnabled)
            {
                border.MouseLeftButtonUp += (s, e) =>
                {
                    onClick?.Invoke();
                    GroupContextMenuPopup.IsOpen = false;
                };

                border.MouseEnter += (s, e) => border.Background = _nodeContextMenuBackgroundHighlight;
                border.MouseLeave += (s, e) => border.Background = Brushes.Transparent;
            }

            return border;
        }

        private Label CreateColorSwatch(string hexColor)
        {
            var name = "C" + hexColor.Replace("#", "");
            var rect = new Label
            {
                Width = 13,
                Height = 13,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hexColor)),
                Margin = new Thickness(3),
                Cursor = Cursors.Hand,
                Name = name,
            };

            rect.SetValue(System.Windows.Automation.AutomationProperties.NameProperty, name);
            
            return rect;
        }

        #endregion

        #endregion

        #region Setup Styles

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

            return style;
        }

        private static Style GetDynamoToolTipTopStyle()
        {
            var infoBubbleEdgeNormalBrush = SharedDictionaryManager.DynamoColorsAndBrushesDictionary["UnSelectedLayoutForeground"] as SolidColorBrush;
            var infoBubbleBackNormalBrush = Brushes.White;

            Style customTooltipStyle = new Style(typeof(DynamoToolTip));

            // Create the ControlTemplate
            ControlTemplate toolTipTemplate = new ControlTemplate(typeof(DynamoToolTip));
            FrameworkElementFactory gridFactory = new FrameworkElementFactory(typeof(Grid));

            // Add RowDefinitions to the Grid
            FrameworkElementFactory rowDef0 = new FrameworkElementFactory(typeof(RowDefinition));
            rowDef0.SetValue(RowDefinition.HeightProperty, new GridLength(1, GridUnitType.Auto));
            gridFactory.AppendChild(rowDef0);

            FrameworkElementFactory rowDef1 = new FrameworkElementFactory(typeof(RowDefinition));
            rowDef1.SetValue(RowDefinition.HeightProperty, new GridLength(6));
            gridFactory.AppendChild(rowDef1);

            FrameworkElementFactory columnDef0 = new FrameworkElementFactory(typeof(ColumnDefinition));
            columnDef0.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Auto));
            gridFactory.AppendChild(columnDef0);

            FrameworkElementFactory borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.SetValue(Grid.RowProperty, 0);
            borderFactory.SetValue(Border.MarginProperty, new Thickness(0, 0, 0, -1));
            borderFactory.SetValue(Border.BackgroundProperty, infoBubbleBackNormalBrush);
            borderFactory.SetValue(Border.BorderBrushProperty, infoBubbleEdgeNormalBrush);
            borderFactory.SetValue(Border.BorderThicknessProperty, new Thickness(1));
            borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(2));
            gridFactory.AppendChild(borderFactory);

            var polylineSegment = new PolyLineSegment()
            {
                Points = new PointCollection(new List<Point>()
                {
                    new Point(0,0),
                    new Point(5,6),
                    new Point(10,0)
                })
            };

            var tooltipPathFigure = new PathFigure()
            {
                IsClosed = false,
                StartPoint = new Point(0, 0)
            };

            tooltipPathFigure.Segments.Add(polylineSegment);

            var tooltipGeometry = new PathGeometry();
            tooltipGeometry.Figures.Add(tooltipPathFigure);

            FrameworkElementFactory pathFactory = new FrameworkElementFactory(typeof(Path));
            pathFactory.SetValue(Path.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            pathFactory.SetValue(Path.VerticalAlignmentProperty, VerticalAlignment.Center);
            pathFactory.SetValue(Path.FillProperty, infoBubbleBackNormalBrush);
            pathFactory.SetValue(Path.StrokeProperty, infoBubbleEdgeNormalBrush);
            pathFactory.SetValue(Path.StrokeThicknessProperty, 1.0);
            pathFactory.SetValue(Path.DataProperty, tooltipGeometry);
            pathFactory.SetValue(Grid.RowProperty, 1);
            gridFactory.AppendChild(pathFactory);

            FrameworkElementFactory contentPresenterFactory = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenterFactory.SetValue(ContentPresenter.MarginProperty, new Thickness(4));
            contentPresenterFactory.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Left);
            contentPresenterFactory.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Top);
            contentPresenterFactory.SetValue(TextBlock.FontSizeProperty, 14.0);
            gridFactory.AppendChild(contentPresenterFactory);

            toolTipTemplate.VisualTree = gridFactory;
            customTooltipStyle.Setters.Add(new Setter(DynamoToolTip.TemplateProperty, toolTipTemplate));

            return customTooltipStyle;
        }

        #endregion

        #region Expander Control Template

        private void CreateExpanderStyle()
        {
            var style = new Style(typeof(Expander));
            style.Setters.Add(new Setter(Control.TemplateProperty, CreateExpanderControlTemplate()));
            this.Resources.Add(typeof(Expander), style);
        }

        private ControlTemplate CreateExpanderControlTemplate()
        {
            var template = new ControlTemplate(typeof(Expander));

            // Create main Grid
            var mainGridFactory = new FrameworkElementFactory(typeof(Grid));

            // Add RowDefinitions correctly
            var autoRow = new FrameworkElementFactory(typeof(RowDefinition));
            autoRow.SetValue(RowDefinition.HeightProperty, GridLength.Auto);
            mainGridFactory.AppendChild(autoRow);

            var contentRow = new FrameworkElementFactory(typeof(RowDefinition));
            contentRow.Name = "ContentRow";
            contentRow.SetValue(RowDefinition.HeightProperty, new GridLength(0));
            mainGridFactory.AppendChild(contentRow);

            var headerBorderFactory = CreateHeaderBorder();
            headerBorderFactory.SetValue(Grid.RowProperty, 0);
            mainGridFactory.AppendChild(headerBorderFactory);

            var contentBorderFactory = new FrameworkElementFactory(typeof(Border));
            contentBorderFactory.Name = "Content";
            contentBorderFactory.SetValue(Grid.RowProperty, 1);

            var contentPresenterFactory = new FrameworkElementFactory(typeof(ContentPresenter));
            contentBorderFactory.AppendChild(contentPresenterFactory);
            mainGridFactory.AppendChild(contentBorderFactory);

            template.VisualTree = mainGridFactory;

            // Add triggers with correct DependencyProperty
            var expandedTrigger = new Trigger
            {
                Property = Expander.IsExpandedProperty,
                Value = true
            };
            expandedTrigger.Setters.Add(new Setter(RowDefinition.HeightProperty, new Binding("Height")
            {
                ElementName = "Content"
            }, "ContentRow"));
            template.Triggers.Add(expandedTrigger);

            return template;
        }

        private FrameworkElementFactory CreateHeaderBorder()
        {
            var headerBorderFactory = new FrameworkElementFactory(typeof(Border));
            headerBorderFactory.Name = "headerBorder";
            headerBorderFactory.SetValue(Border.CornerRadiusProperty, _cornerRadius);
            headerBorderFactory.SetValue(Border.PaddingProperty, new Thickness(0));
            headerBorderFactory.SetValue(Border.MarginProperty, new Thickness(0));

            // Set Background binding
            headerBorderFactory.SetBinding(Border.BackgroundProperty, new Binding("DataContext.Background")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(UserControl), 1),
                Converter = _colorToSolidColorBrushConverter
            });

            // Set IsEnabled binding for ToolTipService
            headerBorderFactory.SetValue(ToolTipService.IsEnabledProperty,
                new Binding("IsExpanded")
                {
                    RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent),
                    Converter = _inverseBooleanConverter
                });

            // Set ToolTip
            headerBorderFactory.SetValue(Border.ToolTipProperty, CreateToolTip());

            // Add the main grid for header content
            var headerGridFactory = CreateHeaderGrid();
            headerBorderFactory.AppendChild(headerGridFactory);

            return headerBorderFactory;
        }

        private FrameworkElementFactory CreateHeaderGrid()
        {
            var gridFactory = new FrameworkElementFactory(typeof(Grid));

            // Add column definitions directly
            var col1 = new FrameworkElementFactory(typeof(ColumnDefinition));
            col1.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));
            gridFactory.AppendChild(col1);

            var col2 = new FrameworkElementFactory(typeof(ColumnDefinition));
            col2.SetValue(ColumnDefinition.WidthProperty, GridLength.Auto);
            gridFactory.AppendChild(col2);

            var col3 = new FrameworkElementFactory(typeof(ColumnDefinition));
            col3.SetValue(ColumnDefinition.WidthProperty, GridLength.Auto);
            gridFactory.AppendChild(col3);

            var col4 = new FrameworkElementFactory(typeof(ColumnDefinition));
            col4.SetValue(ColumnDefinition.WidthProperty, GridLength.Auto);
            gridFactory.AppendChild(col4);

            // Add ContentPresenter
            var contentPresenterFactory = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenterFactory.SetValue(Grid.ColumnSpanProperty, 4);
            contentPresenterFactory.SetValue(FrameworkElement.MarginProperty, new Thickness(4));
            contentPresenterFactory.SetValue(ContentPresenter.ContentSourceProperty, "Header");
            contentPresenterFactory.SetValue(ContentPresenter.RecognizesAccessKeyProperty, true);
            contentPresenterFactory.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Left);
            gridFactory.AppendChild(contentPresenterFactory);

            // Add Warning/Error Icon
            var warningIcon = CreateWarningErrorIcon();
            warningIcon.SetValue(Grid.ColumnProperty, 0);
            warningIcon.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Right);
            gridFactory.AppendChild(warningIcon);

            // Add Frozen Button Grid
            var frozenButtonGrid = CreateFrozenButtonGrid();
            frozenButtonGrid.SetValue(Grid.ColumnProperty, 1);
            gridFactory.AppendChild(frozenButtonGrid);

            // Add Expander Toggle Button
            var expanderToggle = CreateExpanderToggleButton();
            expanderToggle.SetValue(Grid.ColumnProperty, 2);
            gridFactory.AppendChild(expanderToggle);

            // Add Context Menu Button
            var contextMenuButton = CreateContextMenuButton();
            contextMenuButton.SetValue(Grid.ColumnProperty, 3);
            gridFactory.AppendChild(contextMenuButton);

            return gridFactory;
        }

        private FrameworkElementFactory CreateWarningErrorIcon()
        {
            var imageFactory = new FrameworkElementFactory(typeof(Image));
            imageFactory.SetValue(Grid.ColumnProperty, 0);
            imageFactory.SetValue(FrameworkElement.WidthProperty, 16.0);
            imageFactory.SetValue(FrameworkElement.HeightProperty, 16.0);
            imageFactory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Bottom);
            imageFactory.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Right);
            imageFactory.SetValue(FrameworkElement.MarginProperty, new Thickness(0, 0, 2.5, 2.5));
            imageFactory.SetValue(System.Windows.Automation.AutomationProperties.NameProperty, "WarningErrorIcon");
            imageFactory.SetValue(System.Windows.Automation.AutomationProperties.AutomationIdProperty, "WarningErrorIcon");

            var imageStyle = new Style(typeof(Image));

            // Warning trigger
            var warningTrigger = new DataTrigger
            {
                Binding = new Binding("AnnotationModel.GroupState"),
                Value = ElementState.Warning
            };
            warningTrigger.Setters.Add(new Setter(Image.SourceProperty, _warningImage));
            imageStyle.Triggers.Add(warningTrigger);

            // Error trigger
            var errorTrigger = new DataTrigger
            {
                Binding = new Binding("AnnotationModel.GroupState"),
                Value = ElementState.Error
            };
            errorTrigger.Setters.Add(new Setter(Image.SourceProperty, _errorImage));
            imageStyle.Triggers.Add(errorTrigger);

            imageFactory.SetValue(FrameworkElement.StyleProperty, imageStyle);

            return imageFactory;
        }

        private ToolTip CreateToolTip()
        {
            var groupText = new TextBlock()
            {
                Margin = new Thickness(0, 0, 0, 10),
            };
            groupText.SetBinding(TextBlock.TextProperty, new Binding("AnnotationText"));

            var groupDescription = new TextBlock()
            {
                TextWrapping = TextWrapping.WrapWithOverflow,
            };
            groupDescription.SetBinding(TextBlock.TextProperty, new Binding("AnnotationDescriptionText"));

            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                MaxWidth = 320
            };
            stackPanel.Children.Add(groupText);
            stackPanel.Children.Add(groupDescription);

            var toolTip = new DynamoToolTip
            {
                AttachmentSide = DynamoToolTip.Side.Top,
                Style = _dynamoToolTipTopStyle,
                Margin = new Thickness(5, 0, 0, 0),
                Content = stackPanel
            };

            return toolTip;
        }

        private FrameworkElementFactory CreateFrozenButtonGrid()
        {
            var gridFactory = new FrameworkElementFactory(typeof(Grid));

            // Create grid style with visibility trigger
            var gridStyle = new Style(typeof(Grid));
            gridStyle.Setters.Add(new Setter(UIElement.VisibilityProperty, Visibility.Visible));

            var visibilityTrigger = new DataTrigger
            {
                Binding = new Binding("DataContext.Zoom")
                {
                    RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(WorkspaceView), 1),
                    Converter = _zoomToVisibilityCollapsedConverter
                },
                Value = Visibility.Visible
            };
            visibilityTrigger.Setters.Add(new Setter(UIElement.VisibilityProperty, Visibility.Collapsed));
            gridStyle.Triggers.Add(visibilityTrigger);
            gridFactory.SetValue(FrameworkElement.StyleProperty, gridStyle);

            // Create frozen button
            var buttonFactory = new FrameworkElementFactory(typeof(Button));
            buttonFactory.SetValue(System.Windows.Automation.AutomationProperties.NameProperty, "FrezzeButton");
            buttonFactory.SetValue(System.Windows.Automation.AutomationProperties.AutomationIdProperty, "FrezzeButton");
            buttonFactory.SetValue(FrameworkElement.WidthProperty, 16.0);
            buttonFactory.SetValue(FrameworkElement.HeightProperty, 16.0);
            buttonFactory.SetValue(FrameworkElement.MarginProperty, new Thickness(0, 0, 3.5, 3));
            buttonFactory.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Right);
            buttonFactory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Bottom);
            buttonFactory.SetValue(Button.CommandProperty, new Binding("ToggleIsFrozenGroupCommand"));
            buttonFactory.SetValue(Button.TemplateProperty, CreateFrozenButtonZoomedInTemplate());
            buttonFactory.SetBinding(UIElement.VisibilityProperty, new Binding("DataContext.AnnotationModel.IsFrozen")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(UserControl), 1),
                Converter = _boolToVisibilityCollapsedConverter
            });

            // Create button tooltip
            var tooltip = new ToolTip
            {
                Style = _createGenericToolTipLightStyle,
                Content = new TextBlock
                {
                    Text = Wpf.Properties.Resources.GroupFrozenButtonToolTip
                }
            };
            buttonFactory.SetValue(Button.ToolTipProperty, tooltip);

            gridFactory.AppendChild(buttonFactory);
            return gridFactory;
        }

        private FrameworkElementFactory CreateExpanderToggleButton()
        {
            var expanderTemplate = CreateGroupExpanderTemplate();
            var toggleButtonFactory = new FrameworkElementFactory(typeof(ToggleButton));
            toggleButtonFactory.SetValue(Grid.ColumnProperty, 2);
            toggleButtonFactory.SetValue(FrameworkElement.OverridesDefaultStyleProperty, true);
            toggleButtonFactory.SetValue(FrameworkElement.MarginProperty, new Thickness(0, 0, 0, 2.5));
            toggleButtonFactory.SetValue(Control.TemplateProperty, expanderTemplate);
            toggleButtonFactory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Bottom);
            toggleButtonFactory.SetValue(System.Windows.Automation.AutomationProperties.NameProperty, "ExpanderToggle");
            toggleButtonFactory.SetValue(System.Windows.Automation.AutomationProperties.AutomationIdProperty, "ExpanderToggle");

            toggleButtonFactory.SetBinding(ToggleButton.IsCheckedProperty, new Binding("IsExpanded")
            {
                Mode = BindingMode.TwoWay,
                RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent)
            });

            return toggleButtonFactory;
        }

        private FrameworkElementFactory CreateContextMenuButton()
        {
            var buttonFactory = new FrameworkElementFactory(typeof(Button));
            buttonFactory.Name = "contextMenu";
            buttonFactory.SetValue(Grid.ColumnProperty, 3);
            buttonFactory.SetValue(Button.BackgroundProperty, Brushes.Transparent);
            buttonFactory.SetValue(Button.BorderBrushProperty, Brushes.Transparent);
            buttonFactory.SetValue(Button.BorderThicknessProperty, new Thickness(0));
            buttonFactory.SetValue(FrameworkElement.MarginProperty, new Thickness(0, 2.5, 0, 2.5));
            buttonFactory.SetValue(FrameworkElement.HeightProperty, 16.0);
            buttonFactory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Bottom);
            buttonFactory.AddHandler(Button.ClickEvent, new RoutedEventHandler(contextMenu_Click));
            buttonFactory.SetValue(System.Windows.Automation.AutomationProperties.NameProperty, "ContextMenu");
            buttonFactory.SetValue(System.Windows.Automation.AutomationProperties.AutomationIdProperty, "ContextMenu");
            // Create button style
            var buttonStyle = new Style(typeof(Button));
            buttonStyle.Setters.Add(new Setter(Button.OverridesDefaultStyleProperty, true));

            // Create button template
            var buttonTemplate = new ControlTemplate(typeof(Button));
            var borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.Name = "border";
            borderFactory.SetValue(Border.BorderThicknessProperty, new Thickness(0));
            borderFactory.SetValue(Border.BackgroundProperty, new Binding()
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent),
                Path = new PropertyPath(Button.BackgroundProperty)
            });

            var imageFactory = new FrameworkElementFactory(typeof(Image));
            imageFactory.Name = "menuIcon";
            imageFactory.SetValue(FrameworkElement.WidthProperty, 16.0);
            imageFactory.SetValue(FrameworkElement.HeightProperty, 16.0);
            borderFactory.AppendChild(imageFactory);

            buttonTemplate.VisualTree = borderFactory;

            // Add triggers for menu icon
            var notHoverDarkTrigger = new MultiDataTrigger();
            notHoverDarkTrigger.Conditions.Add(new Condition(new Binding("IsMouseOver") { RelativeSource = new RelativeSource(RelativeSourceMode.Self) }, false));
            notHoverDarkTrigger.Conditions.Add(new Condition(new Binding("Background") { Converter = _backgroundConditionEvaluator }, false));
            notHoverDarkTrigger.Setters.Add(new Setter(Image.SourceProperty, _menuGreyImage, "menuIcon"));

            var notHoverLightTrigger = new MultiDataTrigger();
            notHoverLightTrigger.Conditions.Add(new Condition(new Binding("IsMouseOver") { RelativeSource = new RelativeSource(RelativeSourceMode.Self) }, false));
            notHoverLightTrigger.Conditions.Add(new Condition(new Binding("Background") { Converter = _backgroundConditionEvaluator }, true));
            notHoverLightTrigger.Setters.Add(new Setter(Image.SourceProperty, _menuWhiteImage, "menuIcon"));

            var hoverTrigger = new MultiDataTrigger();
            hoverTrigger.Conditions.Add(new Condition(new Binding("IsMouseOver") { RelativeSource = new RelativeSource(RelativeSourceMode.Self) }, true));
            hoverTrigger.Setters.Add(new Setter(Image.SourceProperty, _menuHoverImage, "menuIcon"));

            buttonTemplate.Triggers.Add(notHoverDarkTrigger);
            buttonTemplate.Triggers.Add(notHoverLightTrigger);
            buttonTemplate.Triggers.Add(hoverTrigger);

            buttonStyle.Setters.Add(new Setter(Button.TemplateProperty, buttonTemplate));
            buttonFactory.SetValue(FrameworkElement.StyleProperty, buttonStyle);

            return buttonFactory;
        }

        #endregion

        private void SetModelAreaHeight()
        {
            // We only want to change the ModelAreaHeight
            // if the collapsedAnnotationRectangle is visible,
            // as if its not it will be equal to the height of the
            // contained nodes + the TextBlockHeight
            if (ViewModel is null || !this.collapsedAnnotationRectangle.IsVisible) return;
            ViewModel.ModelAreaHeight = this.collapsedAnnotationRectangle.ActualHeight;
            ViewModel.AnnotationModel.UpdateBoundaryFromSelection();
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
                new Typeface(this.groupTextBlock.FontFamily, this.groupTextBlock.FontStyle, this.groupTextBlock.FontWeight, this.groupTextBlock.FontStretch),
                this.groupTextBlock.FontSize,
                Brushes.Black);

            var margin = this.textBlockGrid.Margin.Right + this.textBlockGrid.Margin.Left;

            this.ViewModel.AnnotationModel.Width = formattedText.Width + margin;
            this.ViewModel.AnnotationModel.TextMaxWidth = formattedText.Width + margin;
        }

        private void SetTextHeight()
        {
            if (this.groupDescriptionTextBlock is null || this.groupTextBlock is null || ViewModel is null)
            {
                return;
            }

            // Use the DesiredSize and not the Actual height. Because when Textblock is collapsed,
            // Actual height is same as previous size.
            ViewModel.AnnotationModel.TextBlockHeight =
                this.groupDescriptionControls.DesiredSize.Height +
                this.groupNameControl.DesiredSize.Height;
        }
    }
}
