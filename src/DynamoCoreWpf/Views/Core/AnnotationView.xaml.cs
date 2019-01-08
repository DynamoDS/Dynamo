using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Dynamo.Selection;
using Dynamo.UI;
using Dynamo.UI.Prompts;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using DynCmd = Dynamo.Models.DynamoModel;
using TextBox = System.Windows.Controls.TextBox;

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
            Resources.MergedDictionaries.Add(SharedDictionaryManager.PortsDictionary);

            InitializeComponent();
            Loaded += AnnotationView_Loaded;
            this.GroupTextBlock.SizeChanged += GroupTextBlock_SizeChanged;
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
                    ViewModel.AnnotationModel.TextBlockHeight = this.GroupTextBlock.ActualHeight;
                }
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

            this.ViewModel.AnnotationModel.Width = formattedText.Width;
            this.ViewModel.AnnotationModel.TextMaxWidth = formattedText.Width;
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

        private void OnUngroupAnnotation(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.WorkspaceViewModel.DynamoViewModel.DeleteCommand.Execute(null);
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
                ViewModel.Select();
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
            DynamoSelection.Instance.ClearSelection();
            System.Guid annotationGuid = this.ViewModel.AnnotationModel.GUID;
            ViewModel.WorkspaceViewModel.DynamoViewModel.ExecuteCommand(
               new DynCmd.SelectModelCommand(annotationGuid, Keyboard.Modifiers.AsDynamoType()));
        }

        /// <summary>
        /// Handles the OnTextChanged event of the GroupTextBox control.
        /// Calculates the height of a Group based on the height of textblock
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        private void GroupTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (ViewModel != null)
            {
                SetTextMaxWidth();
                ViewModel.AnnotationModel.TextBlockHeight = GroupTextBox.ActualHeight;
                ViewModel.WorkspaceViewModel.HasUnsavedChanges = true;
            }
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
                //Use the DesiredSize and not the Actual height. Because when Textblock is collapsed,
                //Actual height is same as previous size. used when the Font size changed during zoom
                ViewModel.AnnotationModel.TextBlockHeight = GroupTextBlock.DesiredSize.Height;
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
                    GroupTextBox.Text));

            ViewModel.WorkspaceViewModel.DynamoViewModel.RaiseCanExecuteUndoRedo();

            textbox.Focus();
            if (textbox.Text.Equals(Properties.Resources.GroupDefaultText))
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
                ViewModel.Select();
                ViewModel.WorkspaceViewModel.DynamoViewModel.DeleteCommand.Execute(null);
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
                ViewModel.Select();
                ViewModel.WorkspaceViewModel.DynamoViewModel.GraphAutoLayoutCommand.Execute(null);
            }
        }

    }
}
