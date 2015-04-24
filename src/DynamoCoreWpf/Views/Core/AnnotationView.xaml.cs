using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Dynamo.UI;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using HelixToolkit.Wpf.SharpDX;
using DynCmd = Dynamo.Models.DynamoModel;
using Dynamo.Selection;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
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
            this.GroupTextBlock.SizeChanged +=GroupTextBlock_SizeChanged;          
        }
     
        private void AnnotationView_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel = this.DataContext as AnnotationViewModel;
            if (ViewModel != null)
            {               
                //Set the height of Textblock based on the content.
                if (!ViewModel.AnnotationModel.loadFromXML)
                {
                    ViewModel.AnnotationModel.TextBlockHeight = this.GroupTextBlock.ActualHeight;
                }
            }
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

        private void OnDeleteAnnotation(object sender, RoutedEventArgs e)
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
            if (GroupTextBlock.IsVisible)
            {
                var annotationGuid = this.ViewModel.AnnotationModel.GUID;
                ViewModel.WorkspaceViewModel.DynamoViewModel.ExecuteCommand(
                    new DynCmd.SelectModelCommand(annotationGuid, Keyboard.Modifiers.AsDynamoType()));

                //Select all the models inside the group - This avoids some performance bottleneck 
                //with many nodes selected at the same time - which makes moving the group very slow
                DynamoSelection.Instance.Selection.AddRange(ViewModel.AnnotationModel.SelectedModels);

                foreach (var models in this.ViewModel.AnnotationModel.SelectedModels)
                {
                    //Make sure that models have the selection border inside a group when selected
                    models.IsSelected = true;                    
                }
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
            if (ViewModel != null && e.HeightChanged)
            {
                ViewModel.AnnotationModel.TextBlockHeight = GroupTextBlock.ActualHeight;                
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
            if (textbox != null && textbox.Visibility == Visibility.Visible)
            {
                textbox.Focus();
                if (textbox.Text.Equals(Dynamo.Properties.Resources.GroupDefaultText))
                {
                    textbox.SelectAll();  
                }
                             
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

        private void GroupTextBlock_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (ViewModel != null && (bool) e.NewValue)
            {
                ViewModel.WorkspaceViewModel.DynamoViewModel.ExecuteCommand(
                    new DynCmd.UpdateModelValueCommand(
                        System.Guid.Empty, this.ViewModel.AnnotationModel.GUID, "TextBlockText",
                        GroupTextBox.Text));

                ViewModel.WorkspaceViewModel.DynamoViewModel.UndoCommand.RaiseCanExecuteChanged();
                ViewModel.WorkspaceViewModel.DynamoViewModel.RedoCommand.RaiseCanExecuteChanged();
            }

        }
    }
}