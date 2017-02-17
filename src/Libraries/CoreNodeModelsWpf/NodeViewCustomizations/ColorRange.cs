using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DSCore;
using Dynamo.Controls;
using Dynamo.Scheduler;

using CoreNodeModels;
using Dynamo.Configuration;
using Dynamo.Engine;
using Dynamo.Wpf;
using Dynamo.Graph.Workspaces;

namespace CoreNodeModelsWpf.Nodes
{
    public class ColorRangeNodeViewCustomization : INodeViewCustomization<ColorRange>
    {
        private IScheduler scheduler;
        private EngineController engineController;
        private DispatcherSynchronizationContext syncContext;
        private ColorRange colorRangeNode;
        private System.Windows.Controls.Image gradientImage;
        private ColorRange1D colorRange;

        public void CustomizeView(ColorRange model, NodeView nodeView)
        {
            var dynamoModel = nodeView.ViewModel.DynamoViewModel.Model;
            var currentWorkspace = dynamoModel.CurrentWorkspace as IHomeWorkspaceModel;

            // if not a home workspace, we will not use the scheduler or enginecontroller
            if (currentWorkspace != null)
            {
                this.scheduler = currentWorkspace.Scheduler;
                this.engineController = currentWorkspace.EngineController;
            }

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
            if (this.scheduler == null) return;

            // prevent data race by running on scheduler
            var t = new DelegateBasedAsyncTask(this.scheduler, () =>
            {
                colorRange = colorRangeNode.ComputeColorRange(this.engineController);
            });

            // then update on the ui thread
            t.ThenSend((_) =>
            {
                var bmp = CreateColorRangeBitmap(colorRange);
                gradientImage.Source = bmp;
            }, syncContext);

            this.scheduler.ScheduleForExecution(t);
        }

        public void Dispose() {}

        //http://gaggerostechnicalnotes.blogspot.com/2012/01/wpf-colors-scale.html
        private WriteableBitmap CreateColorRangeBitmap(ColorRange1D colorRange)
        {
            const int width = 64;
            const int height = 1;

            var bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            var pixels = new uint[width * height];

            for (var i = 1; i <= width; i++)
            {
                var t = (double)i / width;
                var newColor = ColorRange1D.GetColorAtParameter(colorRange,t);
                pixels[i-1] = (uint)((255 << 24) + (newColor.Red << 16) + (newColor.Green << 8) + newColor.Blue);

            }
            bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 4, 0);

            return bitmap;
        }
    }
}
