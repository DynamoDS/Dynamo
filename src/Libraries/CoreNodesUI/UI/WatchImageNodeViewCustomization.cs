using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Wpf;

using Image = System.Windows.Controls.Image;

namespace Dynamo.Wpf
{
    internal class WatchImageNodeViewCustomization : INodeViewCustomization<Dynamo.Nodes.WatchImageCore>
    {
        private Image image;
        private NodeModel nodeModel;
        private NodeView nodeView;

        public void CustomizeView(Nodes.WatchImageCore nodeModel, NodeView nodeView)
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
            if (args.PropertyName != "IsUpdated") return;
            var im = GetImageFromMirror();
            nodeView.Dispatcher.BeginInvoke(new Action<Bitmap>(SetImageSource), new object[] { im });
        }

        private void SetImageSource(System.Drawing.Bitmap bmp)
        {
            if (bmp == null)
                return;

            // how to convert a bitmap to an imagesource http://blog.laranjee.com/how-to-convert-winforms-bitmap-to-wpf-imagesource/ 
            // TODO - watch out for memory leaks using system.drawing.bitmaps in managed code, see here http://social.msdn.microsoft.com/Forums/en/csharpgeneral/thread/4e213af5-d546-4cc1-a8f0-462720e5fcde
            // need to call Dispose manually somewhere, or perhaps use a WPF native structure instead of bitmap?

            var hbitmap = bmp.GetHbitmap();
            var imageSource = Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(bmp.Width, bmp.Height));
            image.Source = imageSource;
        }

        private Bitmap GetImageFromMirror()
        {
            if (this.nodeModel.InPorts[0].Connectors.Count == 0) return null;

            var mirror = this.nodeModel.Workspace.DynamoModel.EngineController.GetMirror(this.nodeModel.AstIdentifierForPreview.Name);

            if (null == mirror)
                return null;

            var data = mirror.GetData();

            if (data == null || data.IsNull) return null;
            if (data.Data is System.Drawing.Bitmap) return data.Data as System.Drawing.Bitmap;
            return null;
        }

        public void Dispose()
        {
            nodeModel.PropertyChanged -= NodeModelOnPropertyChanged;
        }
    }
}
