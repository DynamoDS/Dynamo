using System;
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

                    object start = null;
                    object end = null;

                    if (startMirror == null)
                    {
                        start = DSCore.Color.ByARGB(255, 192, 192, 192);
                    }
                    else
                    {
                        if (startMirror.GetData().IsCollection)
                        {
                            start = startMirror.GetData().GetElements().
                                Select(x => x.Data).FirstOrDefault();
                        }
                        else
                        {
                            start = startMirror.GetData().Data;
                        }
                    }

                    if (endMirror == null)
                    {
                        end = DSCore.Color.ByARGB(255, 64, 64, 64);
                    }
                    else
                    {
                        if (endMirror.GetData().IsCollection)
                        {
                            end = endMirror.GetData().GetElements().
                                Select(x => x.Data).FirstOrDefault();
                        }
                        else
                        {
                            end = endMirror.GetData().Data;
                        }
                    }

                    DSCore.Color startColor = start as DSCore.Color;
                    DSCore.Color endColor = end as DSCore.Color;
                    if (null != startColor && null != endColor)
                    {
                        WriteableBitmap bmp = CompleteColorScale(startColor, endColor);
                        drawPlane.Source = bmp;
                    }
                });
            };
        }

        public void Dispose() {}

        //http://gaggerostechnicalnotes.blogspot.com/2012/01/wpf-colors-scale.html
        private WriteableBitmap CompleteColorScale(DSCore.Color start, Color end)
        {
            const int size = 64;

            const int width = 1;
            const int height = size;

            var bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            var pixels = new uint[width * height];

            for (int i = 0; i < size; i++)
            {
                var newRed = start.Red + ((end.Red - start.Red) / size) * i;
                var newGreen = start.Green + ((end.Green - start.Green) / size) * i;
                var newBlue = start.Blue + ((end.Blue - start.Blue) / size) * i;

                pixels[i] = (uint)((255 << 24) + (newRed << 16) + (newGreen << 8) + newBlue);

            }
            bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 4, 0);

            return bitmap;
        }

    }
}
