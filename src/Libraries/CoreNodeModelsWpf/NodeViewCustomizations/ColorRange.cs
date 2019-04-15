
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CoreNodeModels;
using Dynamo.Configuration;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Scheduler;
using Dynamo.ViewModels;
using Dynamo.Wpf;

namespace CoreNodeModelsWpf.Nodes
{
    public class ColorRangeNodeViewCustomization : INodeViewCustomization<ColorRange>
    {
        private DynamoViewModel dynamoViewModel;
        private DispatcherSynchronizationContext syncContext;
        private ColorRange colorRangeNode;
        private DynamoModel dynamoModel;
        private System.Windows.Controls.Image gradientImage;
        private DSCore.ColorRange1D colorRange;

        public void CustomizeView(ColorRange model, NodeView nodeView)
        {
            dynamoModel = nodeView.ViewModel.DynamoViewModel.Model;
            dynamoViewModel = nodeView.ViewModel.DynamoViewModel;
            syncContext = new DispatcherSynchronizationContext(nodeView.Dispatcher);
            colorRangeNode = model;

            gradientImage = new Image
            {
                Stretch = Stretch.Fill,
                Width = 200,
                Height = Configurations.PortHeightInPixels * 3
            };

            nodeView.inputGrid.Children.Add(gradientImage);

            colorRangeNode.RequestChangeColorRange += UpdateColorRange;

            UpdateColorRange();
        }

        private void UpdateColorRange()
        {
            var s = dynamoViewModel.Model.Scheduler;

            // prevent data race by running on scheduler
            var t = new DelegateBasedAsyncTask(s, () =>
            {
                colorRange = colorRangeNode.ComputeColorRange(dynamoModel.EngineController);
            });

            // then update on the ui thread
            t.ThenSend((_) =>
            {
                var bmp = CreateColorRangeBitmap(colorRange);
                gradientImage.Source = bmp;
            }, syncContext);

            s.ScheduleForExecution(t);
        }

        public void Dispose() {}

        //http://gaggerostechnicalnotes.blogspot.com/2012/01/wpf-colors-scale.html
        private WriteableBitmap CreateColorRangeBitmap(DSCore.ColorRange1D cRange)
        {
            if (cRange == null) return null;

            const int width = 64;
            const int height = 1;

            var bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            var pixels = new uint[width * height];

            for (var i = 1; i <= width; i++)
            {
                var t = (double)i / width;
                var newColor = DSCore.ColorRange1D.GetColorAtParameter(cRange,t);
                pixels[i-1] = (uint)((255 << 24) + (newColor.Red << 16) + (newColor.Green << 8) + newColor.Blue);

            }
            bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 4, 0);

            return bitmap;
        }
    }
}
