using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Data;
using System.Windows.Input;
using Dynamo.UI;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using DynCmd = Dynamo.Models.DynamoModel;
using Dynamo.UI.Prompts;

namespace Dynamo.Nodes
{
    /// <summary>
    /// Interaction logic for AnnotationView.xaml
    /// </summary>
    public partial class AnnotationView : IViewModelView<AnnotationViewModel>
    {
        public AnnotationViewModel ViewModel { get; private set; }

        public AnnotationView()
        {
            InitializeComponent();
            Loaded += AnnotationView_Loaded;
            BindingErrorTraceListener.SetTrace();
        }

        private void AnnotationView_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel = this.DataContext as AnnotationViewModel;
            //ViewModel.MakeTextBoxVisible = Visibility.Collapsed;
            //ViewModel.MakeTextBlockVisible = Visibility.Visible;
        }

        private void AnnotationView_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //annotationViewModel.MakeTextBoxVisible = Visibility.Visible;   
            var view = sender as AnnotationView;
            if (view != null) Panel.SetZIndex(view, 9999);
        }

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                OnEditItemClick(this, null);
                e.Handled = true;
            }           
        }

        private void OnEditItemClick(object sender, RoutedEventArgs e)
        {
            // Setup a binding with the edit window's text field
            var dynamoViewModel = ViewModel.WorkspaceViewModel.DynamoViewModel;
            var editWindow = new EditWindow(dynamoViewModel, true);
            editWindow.BindToProperty(DataContext, new Binding("AnnotationText")
            {
                Mode = BindingMode.TwoWay,
                Source = (DataContext as AnnotationViewModel),
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            });

            editWindow.ShowDialog();
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
                ViewModel.WorkspaceViewModel.DynamoViewModel.DeleteCommand.Execute(null);
        }

        private void AnnotationView_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Guid annotationGuid = this.ViewModel.AnnotationModel.GUID;
            ViewModel.WorkspaceViewModel.DynamoViewModel.ExecuteCommand(
               new DynCmd.SelectModelCommand(annotationGuid, Keyboard.Modifiers.AsDynamoType()));
        }      
    }
}