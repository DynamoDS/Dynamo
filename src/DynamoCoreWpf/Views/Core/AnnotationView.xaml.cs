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

namespace Dynamo.Nodes
{
    /// <summary>
    /// Interaction logic for AnnotationView.xaml
    /// </summary>
    public partial class AnnotationView : IViewModelView<AnnotationViewModel>
    {
        public AnnotationViewModel ViewModel { get; private set; }
        public static DependencyProperty SelectAllTextOnFocus;
        public AnnotationView()
        {
            Resources.MergedDictionaries.Add(SharedDictionaryManager.DynamoModernDictionary);
            Resources.MergedDictionaries.Add(SharedDictionaryManager.DynamoColorsAndBrushesDictionary);
            Resources.MergedDictionaries.Add(SharedDictionaryManager.DataTemplatesDictionary);
            Resources.MergedDictionaries.Add(SharedDictionaryManager.DynamoConvertersDictionary);
            Resources.MergedDictionaries.Add(SharedDictionaryManager.InPortsDictionary);
            Resources.MergedDictionaries.Add(SharedDictionaryManager.OutPortsDictionary);

            InitializeComponent();
            Unloaded += AnnotationView_Unloaded;
            Loaded += AnnotationView_Loaded;
            DataContextChanged += AnnotationView_DataContextChanged;
            this.GroupTextBlock.SizeChanged += GroupTextBlock_SizeChanged;

            // Because the size of the CollapsedAnnotationRectangle doesn't necessarily change 
            // when going from Visible to collapse (and other way around), we need to also listen
            // to IsVisibleChanged. Both of these handlers will set the ModelAreaHeight on the ViewModel
            this.CollapsedAnnotationRectangle.IsVisibleChanged += CollapsedAnnotationRectangle_IsVisibleChanged;
        }

        private void AnnotationView_Unloaded(object sender, RoutedEventArgs e)
        {
            Loaded -= AnnotationView_Loaded;
            DataContextChanged -= AnnotationView_DataContextChanged;
            this.GroupTextBlock.SizeChanged -= GroupTextBlock_SizeChanged;
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
            }
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

        private void CollapsedAnnotationRectangle_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var model = ViewModel.AnnotationModel;
            if (!model.IsExpanded)
            {
                // Measure port widths and update min width
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    double maxInPortWidth = MeasureMaxPortWidth(inputPortControl);
                    double maxOutPortWidth = MeasureMaxPortWidth(outputPortControl);
                    model.MinWidthOnCollapsed = maxInPortWidth + maxOutPortWidth + 10;

                    double totalInPortHeight = MeasureCombinedPortHeight(inputPortControl);
                    double totalOutPortHeight = MeasureCombinedPortHeight(outputPortControl);
                    model.MinCollapsedPortAreaHeight = Math.Max(totalInPortHeight, totalOutPortHeight);

                    model.UpdateBoundaryFromSelection();
                }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            }
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

        private void CollapsedAnnotationRectangleThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            // Mark the group as being resized while collapsed
            ViewModel.AnnotationModel.IsResizedWhileCollapsed = true;

            var model = ViewModel.AnnotationModel;
            var xAdjust = ViewModel.Width + e.HorizontalChange;
            var yAdjust = ViewModel.Height + e.VerticalChange;

            double minHeight = model.MinCollapsedPortAreaHeight + model.TextBlockHeight + AnnotationModel.CollapsedContentHeight;

            if (xAdjust >= Math.Max(model.TextMaxWidth, model.MinWidthOnCollapsed))
            {
                model.WidthAdjustment += e.HorizontalChange;
                ViewModel.WorkspaceViewModel.HasUnsavedChanges = true;
            }

            if (yAdjust >= minHeight)
            {
                model.HeightAdjustment += e.VerticalChange;
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

        private double MeasureMaxPortWidth(ItemsControl portControl)
        {
            portControl.UpdateLayout();

            double max = 0;
            foreach (var item in portControl.Items)
            {
                if (portControl.ItemContainerGenerator.ContainerFromItem(item) is FrameworkElement container)
                {
                    container.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    max = Math.Max(max, container.DesiredSize.Width);
                }
            }
            return max;
        }

        private double MeasureCombinedPortHeight(ItemsControl portControl)
        {
            portControl.UpdateLayout();

            double total = 0;
            foreach (var item in portControl.Items)
            {
                if (portControl.ItemContainerGenerator.ContainerFromItem(item) is FrameworkElement container && container.IsVisible)
                {
                    container.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    total += container.DesiredSize.Height;
                }
            }
            return total;
        }
    }
}
