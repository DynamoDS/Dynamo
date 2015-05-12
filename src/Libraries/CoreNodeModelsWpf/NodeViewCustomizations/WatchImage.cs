using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Image = System.Windows.Controls.Image;

namespace Dynamo.Wpf.Nodes
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
        }

        private void NodeModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != "IsUpdated") 
                return;

            var data = nodeModel.CachedValue;
            if (data == null)
                return;

            // There is a pending memory leak issue with this bitmap object and is being
            // tracked by: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-7305
            var bitmap = data.Data as Bitmap;
            if (bitmap != null)
            {
                nodeView.Dispatcher.BeginInvoke(new Action<Bitmap>(SetImageSource), new object[] { bitmap });
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
