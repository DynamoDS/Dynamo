using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Dynamo.Controls;
using Dynamo.UI;

using DSCoreNodesUI;

using Color = DSCore.Color;

namespace Dynamo.Wpf.Nodes
{
    public class ColorRangeNodeViewCustomization : INodeViewCustomization<ColorRange>
    {
        public void CustomizeView(ColorRange model, NodeView nodeView)
        {
            var drawPlane = new Image
            {
                Stretch = Stretch.Fill,
                Width = 200,
                Height = Configurations.PortHeightInPixels * 3
            };

            var dm = nodeView.ViewModel.DynamoViewModel.Model;

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

                    colors = startMirror == null ? 
                        new List<Color>{DSCore.Color.ByARGB(255, 192, 192, 192)} : 
                        startMirror.GetData().GetElements().Select(e=>e.Data).Cast<Color>();

                    try
                    {
                        values =
                            endMirror.GetData()
                                .GetElements()
                                .Select(e => e.Data)
                                .Select(d=>Convert.ToDouble(d,CultureInfo.InvariantCulture));
                    }
                    catch
                    {
                        values = new List<double> { 0.0 };
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
            const int width = 64;
            const int height = 1;

            var bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            var pixels = new uint[width * height];

            for (int i = 0; i < width; i++)
            {
                double t = (double)i / width;
                var newColor = Color.BuildColorFrom1DRange(colors, values, t);

                pixels[i] = (uint)((255 << 24) + (newColor.Red << 16) + (newColor.Green << 8) + newColor.Blue);

            }
            bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 4, 0);

            return bitmap;
        }

    }

    //public class ColorRange2DNodeViewCustomization : INodeViewCustomization<ColorRange2D>
    //{
    //    public void CustomizeView(ColorRange2D model, NodeView nodeView)
    //    {
    //        var drawPlane = new Image
    //        {
    //            Stretch = Stretch.Fill,
    //            Width = 200,
    //            Height = 200
    //        };

    //        var dm = model.Workspace.DynamoModel;

    //        nodeView.inputGrid.Children.Add(drawPlane);

    //        model.RequestChangeColorRange += delegate
    //        {
    //            model.DispatchOnUIThread(delegate
    //            {
    //                var colorStartNode = model.InPorts[0].Connectors[0].Start.Owner;
    //                var startIndex = model.InPorts[0].Connectors[0].Start.Index;
    //                var colorEndNode = model.InPorts[1].Connectors[0].Start.Owner;
    //                var endIndex = model.InPorts[1].Connectors[0].Start.Index;

    //                var startId = colorStartNode.GetAstIdentifierForOutputIndex(startIndex).Name;
    //                var endId = colorEndNode.GetAstIdentifierForOutputIndex(endIndex).Name;

    //                var startMirror = dm.EngineController.GetMirror(startId);
    //                var endMirror = dm.EngineController.GetMirror(endId);

    //                IEnumerable<Color> colors = null;
    //                IEnumerable<UV> values = null;

    //                colors = startMirror == null ?
    //                    new List<Color> { DSCore.Color.ByARGB(255, 192, 192, 192) } : 
    //                    startMirror.GetData().GetElements().Select(e => e.Data).Cast<Color>();

    //                values = endMirror == null ? 
    //                    new List<UV> { UV.ByCoordinates() } : 
    //                    endMirror.GetData().GetElements().Select(e => e.Data).Cast<UV>();

    //                WriteableBitmap bmp = CompleteColorScale(colors.ToList(), values.ToList());
    //                drawPlane.Source = bmp;

    //            });
    //        };
    //    }

    //    public void Dispose() { }

    //    //http://gaggerostechnicalnotes.blogspot.com/2012/01/wpf-colors-scale.html
    //    private WriteableBitmap CompleteColorScale(IList<Color> colors, IList<UV> values)
    //    {
    //        const int width = 64;
    //        const int height = 64;

    //        var bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
    //        var pixels = new uint[width * height];

    //        var colorRange = DSCore.ColorRange2D.ByColorsAndParameters(colors, values);
    //        var colorMap = colorRange.CreateColorMap(width, height);

    //        var count = 0;
    //        for (int i = 0; i < width; i++)
    //        {
    //            for (int j = 0; j < height; j++)
    //            {
    //                var newColor = colorMap[i, j];

    //                pixels[count] = (uint)((255 << 24) + (newColor.Red << 16) + (newColor.Green << 8) + newColor.Blue);
    //                count++;
    //            }
    //        }
    //        bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 4, 0);

    //        return bitmap;
    //    }

    //}
}
