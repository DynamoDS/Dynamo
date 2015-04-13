using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.ViewModels;
using Dynamo.Wpf.Controls;

using DynamoDelcam;

namespace Dynamo.Wpf.NodeViewCustomizations
{
    public class DelcamViewerNodeViewCustomization : INodeViewCustomization<DelcamViewer>
    {
        private NodeModel nodeModel;
        private DelcamViewerControl delcamViewerControl;
        private NodeViewModel nodeViewModel;
        private DelcamViewer viewerModel;
        private DelcamViewerViewModel viewerViewModel;

        public void CustomizeView(DelcamViewer model, NodeView nodeView)
        {
            nodeModel = nodeView.ViewModel.NodeModel;
            nodeViewModel = nodeView.ViewModel;
            viewerModel = model;

            delcamViewerControl = new DelcamViewerControl(model, nodeView)
            {
                DataContext = new DelcamViewerViewModel(model, nodeView),
            };

            viewerViewModel = delcamViewerControl.DataContext as DelcamViewerViewModel;
            nodeView.PresentationGrid.Children.Add(delcamViewerControl);
            nodeView.PresentationGrid.Visibility = Visibility.Visible;
            delcamViewerControl.Loaded += converterControl_Loaded;
        }

        private void converterControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
        }

        public void Dispose()
        {
            viewerViewModel.Dispose();
        }
    }
}
