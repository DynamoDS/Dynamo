using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;
using Dynamo.UI;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using GraphLayout;
using DynCmd = Dynamo.Models.DynamoModel;
using Dynamo.Selection;
using Dynamo.Models;

namespace Dynamo.Nodes
{
    /// <summary>
    /// Interaction logic for AnnotationView.xaml
    /// </summary>
    public partial class AnnotationView : IViewModelView<AnnotationViewModel>
    {
        public AnnotationViewModel ViewModel { get; private set; }
        private bool CanMoveGroup;
        double xAnnotationViewPos, xCanvasPos, yAnnotationViewPos, yCanvasPos;
        private bool canRepositionNode;
        
        public AnnotationView()
        {
            Resources.MergedDictionaries.Add(SharedDictionaryManager.DynamoModernDictionary);
            Resources.MergedDictionaries.Add(SharedDictionaryManager.DynamoColorsAndBrushesDictionary);
            Resources.MergedDictionaries.Add(SharedDictionaryManager.DataTemplatesDictionary);
            Resources.MergedDictionaries.Add(SharedDictionaryManager.DynamoConvertersDictionary);
            Resources.MergedDictionaries.Add(SharedDictionaryManager.PortsDictionary);

            InitializeComponent();
            Loaded += AnnotationView_Loaded;
            BindingErrorTraceListener.SetTrace();                      
        }

        private void AnnotationView_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel = this.DataContext as AnnotationViewModel;
            //Set the height of Textblock based on the content.
            if (ViewModel != null)
            {
                ViewModel.AnnotationModel.TextBlockHeight = this.GroupTextBlock.ActualHeight;
            }
        }

        private void OnNodeColorSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems == null || (e.AddedItems.Count <= 0))
                return;

            var rectangle = e.AddedItems[0] as Rectangle;
            if (rectangle != null)
            {
                var brush = rectangle.Fill as SolidColorBrush;
                if (brush != null)
                    ViewModel.BackGroundColor = brush.Color;
            }
        }

        private void OnDeleteAnnotation(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.WorkspaceViewModel.DynamoViewModel.DeleteCommand.Execute(null);               
            }
        }
     
        private void AnnotationView_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        { 
            DynamoSelection.Instance.ClearSelection();
            System.Guid annotationGuid = this.ViewModel.AnnotationModel.GUID;
            ViewModel.WorkspaceViewModel.DynamoViewModel.ExecuteCommand(
                new DynCmd.SelectModelCommand(annotationGuid, Keyboard.Modifiers.AsDynamoType()));

            foreach (var models in this.ViewModel.AnnotationModel.SelectedModels)
            {
                ViewModel.WorkspaceViewModel.DynamoViewModel.ExecuteCommand(
                    new DynCmd.SelectModelCommand(models.GUID, Dynamo.Utilities.ModifierKeys.Shift));
            }
        }
     
        private void AnnotationView_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            DynamoSelection.Instance.ClearSelection();
            System.Guid annotationGuid = this.ViewModel.AnnotationModel.GUID;
            ViewModel.WorkspaceViewModel.DynamoViewModel.ExecuteCommand(
               new DynCmd.SelectModelCommand(annotationGuid, Keyboard.Modifiers.AsDynamoType()));
        }

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.GroupTextBox.Visibility = Visibility.Visible;
            this.GroupTextBlock.Visibility = Visibility.Collapsed;
            e.Handled = true;
        }

        private void GroupTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {           
            if (ViewModel != null)
            {
                ViewModel.AnnotationModel.TextBlockHeight = GroupTextBox.ActualHeight;
            }           
        }

        private void AnnotationView_OnMouseLeave(object sender, MouseEventArgs e)
        {
            this.GroupTextBox.Visibility = Visibility.Collapsed;
            this.GroupTextBlock.Visibility = Visibility.Visible; 
        }
    }
}