using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using CoreNodeModels;
using Dynamo.Controls;
using Dynamo.Graph.Nodes;
using Dynamo.Utilities;
using Dynamo.Wpf;
using Image = System.Windows.Controls.Image;

namespace CoreNodeModelsWpf.Nodes
{
    internal class WatchImageNodeViewCustomization : INodeViewCustomization<WatchImageCore>
    {
        private Image image;
        private NodeModel nodeModel;
        private NodeView nodeView;

        public void CustomizeView(WatchImageCore nodeModel, NodeView nodeView)
        {
            this.nodeModel = nodeModel;
            this.nodeView = nodeView;

            image = new Image
            {
                MaxWidth = 400,
                MaxHeight = 400,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                Name = "image1",
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };

            // Update the image when the property is updated
            // TODO(Peter): This is a hack written a long time ago, should be cleaned up
            nodeModel.PropertyChanged += NodeModelOnPropertyChanged;

            nodeView.PresentationGrid.Children.Add(image);
            nodeView.PresentationGrid.Visibility = Visibility.Visible;

            HandleMirrorData();
        }

        private void NodeModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != "CachedValue") 
                return;

            HandleMirrorData();
        }

        private void HandleMirrorData()
        {
            var data = nodeModel.CachedValue;
            if (data == null)
                return;

            var bitmap = data.Data as Bitmap;
            if (bitmap != null)
            {
                nodeView.Dispatcher.Invoke(new Action<Bitmap>(SetImageSource), new object[] { bitmap });
            }
        }

        private void SetImageSource(Bitmap bmp)
        {
            image.Source = ResourceUtilities.ConvertToImageSource(bmp);
        }

        public void Dispose()
        {
            nodeModel.PropertyChanged -= NodeModelOnPropertyChanged;
        }
    }
}
