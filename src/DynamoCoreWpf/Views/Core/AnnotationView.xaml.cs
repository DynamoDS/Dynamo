using System;
using System.Collections.Generic;
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
using GraphLayout;
using DynCmd = Dynamo.Models.DynamoModel;
using Dynamo.UI.Prompts;
using Dynamo.Selection;

namespace Dynamo.Nodes
{
    /// <summary>
    /// Interaction logic for AnnotationView.xaml
    /// </summary>
    public partial class AnnotationView : IViewModelView<AnnotationViewModel>
    {
        public AnnotationViewModel ViewModel { get; private set; }
        private bool moveTheGrouping;
        double x_shape, x_canvas, y_shape, y_canvas;
        private bool canRepositionNode;
        
        public AnnotationView()
        {
            InitializeComponent();
            Loaded += AnnotationView_Loaded;
            BindingErrorTraceListener.SetTrace();
        }

        private void AnnotationView_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel = this.DataContext as AnnotationViewModel;          
        }

      
        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {       
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
     
        private void AnnotationView_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //annotationViewModel.MakeTextBoxVisible = Visibility.Visible; 
            var dataContext = this.DataContext as AnnotationViewModel;
            var view = sender as AnnotationView;
            if (view != null) Panel.SetZIndex(view, 9999);
            Mouse.Capture(view);
            dataContext.IsInDrag = true;
            
            if (e.ClickCount == 1)
            {                
                moveTheGrouping = true;
                this.CaptureMouse();
                x_shape = Canvas.GetLeft(view);
                var parentCanvas = FindChild<Canvas>(Application.Current.MainWindow, "backgroundCanvas");
                x_canvas = e.GetPosition(parentCanvas).X;
                y_shape = Canvas.GetTop(view);
                y_canvas = e.GetPosition(parentCanvas).Y;

                foreach (var nodes in dataContext.SelectedNodes)
                {
                    ViewModel.WorkspaceViewModel.DynamoViewModel.ExecuteCommand(
                          new DynCmd.SelectModelCommand(nodes.GUID, Dynamo.Utilities.ModifierKeys.Shift));
                }

                var point = e.GetPosition(parentCanvas);
                var operation = DynCmd.DragSelectionCommand.Operation.BeginDrag;
                var command = new DynCmd.DragSelectionCommand(point.AsDynamoType(), operation);
                ViewModel.WorkspaceViewModel.DynamoViewModel.ExecuteCommand(command);
            }
            if (e.ClickCount >= 2)
            {
                OnEditItemClick(this, null);
            }
            e.Handled = true;
        }

        public static T FindChild<T>(DependencyObject parent, string childName)
   where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        private void AnnotationView_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var dataContext = this.DataContext as AnnotationViewModel;
          
            var parentCanvas = FindChild<Canvas>(Application.Current.MainWindow, "backgroundCanvas");
          
            var point = e.GetPosition(parentCanvas);
            var operation = DynCmd.DragSelectionCommand.Operation.EndDrag;
            var command = new DynCmd.DragSelectionCommand(point.AsDynamoType(), operation);
            ViewModel.WorkspaceViewModel.DynamoViewModel.ExecuteCommand(command);
          
            Mouse.Capture(null);           
            moveTheGrouping = false;         
            DynamoSelection.Instance.ClearSelection();
            dataContext.IsInDrag = false;
            e.Handled = true;
        }

        private void AnnotationView_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Guid annotationGuid = this.ViewModel.AnnotationModel.GUID;
            ViewModel.WorkspaceViewModel.DynamoViewModel.ExecuteCommand(
               new DynCmd.SelectModelCommand(annotationGuid, Keyboard.Modifiers.AsDynamoType()));
        }

        private void AnnotationView_OnMouseMove(object sender, MouseEventArgs e)
        {
          
            if (moveTheGrouping)
            {
                var parentCanvas = FindChild<Canvas>(Application.Current.MainWindow, "backgroundCanvas");

                double x = e.GetPosition(parentCanvas).X;
                double y = e.GetPosition(parentCanvas).Y;
                x_shape += x - x_canvas;
                Canvas.SetLeft(this, x_shape);
                x_canvas = x;
                y_shape += y - y_canvas;
                Canvas.SetTop(this, y_shape);
                y_canvas = y;                
            }

            e.Handled = true;
        }
    }
}