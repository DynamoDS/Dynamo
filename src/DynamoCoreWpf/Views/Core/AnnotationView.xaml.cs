using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Dynamo.Selection;
using Dynamo.UI;
using Dynamo.UI.Prompts;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using DynCmd = Dynamo.Models.DynamoModel;

namespace Dynamo.Controls
{
    /// <summary>
    /// Interaction logic for AnnotationView.xaml
    /// </summary>
    public partial class AnnotationView : UserControl
    {
        private AnnotationViewModel annotationViewModel
        {
            get { return this.DataContext as AnnotationViewModel; }
        }
        public AnnotationView()
        {
            InitializeComponent();
            Loaded += AnnotationView_Loaded;
        }

        private void AnnotationView_Loaded(object sender, RoutedEventArgs e)
        {
            annotationViewModel.MakeTextBoxVisible = Visibility.Collapsed;
            annotationViewModel.MakeTextBlockVisible = Visibility.Visible;
        }

        private void AnnotationView_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //annotationViewModel.MakeTextBoxVisible = Visibility.Visible;   
            var view = sender as AnnotationView;
            if (view != null) Panel.SetZIndex(view, 9999);
        }

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                annotationViewModel.MakeTextBoxVisible = Visibility.Visible;
                annotationViewModel.MakeTextBlockVisible = Visibility.Collapsed;
            }
            else
            {
                annotationViewModel.MakeTextBoxVisible = Visibility.Collapsed;
                annotationViewModel.MakeTextBlockVisible = Visibility.Visible;
            }
        }

        private void UIElement_OnMouseLeave(object sender, MouseEventArgs e)
        {
            annotationViewModel.MakeTextBlockVisible = Visibility.Visible;
            annotationViewModel.MakeTextBoxVisible = Visibility.Collapsed;
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
                    annotationViewModel.BackGroundColor = brush.Color;
            }
        }

        private void OnDeleteAnnotation(object sender, RoutedEventArgs e)
        {

            if (annotationViewModel != null)
                annotationViewModel.WorkspaceViewModel.DynamoViewModel.DeleteCommand.Execute(null);
        }

        private void AnnotationView_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Guid annotationGuid = this.annotationViewModel.AnnotationModel.GUID;
            annotationViewModel.WorkspaceViewModel.DynamoViewModel.ExecuteCommand(
               new DynCmd.SelectModelCommand(annotationGuid, Keyboard.Modifiers.AsDynamoType()));
        }
    }
}