using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Dynamo.Controls;
using Dynamo.Wpf;

using Color = DSCore.Color;

namespace DSCoreNodesUI
{
    public class ColorRangeNodeViewCustomization : INodeViewCustomization<ColorRange>
    {
        public void CustomizeView(ColorRange model, NodeView nodeView)
        {
            var drawPlane = new Image
            {
                Stretch = Stretch.Fill,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Width = 100,
                Height = 200
            };

            var dm = model.Workspace.DynamoModel;

            nodeView.inputGrid.Children.Add(drawPlane);

            model.RequestChangeColorRange += delegate
            {
                model.DispatchOnUIThread(delegate
                {
                    var colorStartNode = model.InPorts[0].Connectors[0].Start.Owner;
                    var startIndex = model.InPorts[0].Connectors[0].Start.Index;
                    var colorEndNode = model.InPorts[1].Connectors[0].Start.Owner;
                    var endIndex = model.InPorts[1].Connectors[0].Start.Index;

                    var startId = colorStartNode.GetAstIdentifierForOutputIndex(startIndex).Name;
                    var endId = colorEndNode.GetAstIdentifierForOutputIndex(endIndex).Name;

                    var startMirror = dm.EngineController.GetMirror(startId);
                    var endMirror = dm.EngineController.GetMirror(endId);

                    IEnumerable<Color> colors = null;
                    IEnumerable<double> values = null;

                    if (startMirror == null)
                    {
                        colors = new List<Color>{DSCore.Color.ByARGB(255, 192, 192, 192)};
                    }
                    else
                    {
                        colors = startMirror.GetData().GetElements().Select(e=>e.Data).Cast<Color>();
                    }

                    if (endMirror == null)
                    {
                        values = new List<double>{0.0};
                    }
                    else
                    {
                        values = endMirror.GetData().GetElements().Select(e => e.Data).Cast<double>();
                    }

                    WriteableBitmap bmp = CompleteColorScale(colors.ToList(), values.ToList());
                    drawPlane.Source = bmp;

                });
            };
        }

        public void Dispose() {}

        //http://gaggerostechnicalnotes.blogspot.com/2012/01/wpf-colors-scale.html
        private WriteableBitmap CompleteColorScale(IList<Color> colors, IList<double> values)
        {
            const int size = 64;

            const int width = 1;
            const int height = size;

            var bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            var pixels = new uint[width * height];

            for (int i = 0; i < size; i++)
            {
                double t = (double)i/size;
                var newColor = Color.BuildColorFrom1DRange(colors, values, t);

                pixels[i] = (uint)((255 << 24) + (newColor.Red << 16) + (newColor.Green << 8) + newColor.Blue);

            }
            bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 4, 0);

            return bitmap;
        }

    }
}
