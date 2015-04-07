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
                    var colorsNode = model.InPorts[0].Connectors[0].Start.Owner;
                    var colorsIndex = model.InPorts[0].Connectors[0].Start.Index;
                    var valuesNode = model.InPorts[1].Connectors[0].Start.Owner;
                    var valuesIndex = model.InPorts[1].Connectors[0].Start.Index;

                    var startId = colorsNode.GetAstIdentifierForOutputIndex(colorsIndex).Name;
                    var endId = valuesNode.GetAstIdentifierForOutputIndex(valuesIndex).Name;

                    var colorsMirror = dm.EngineController.GetMirror(startId);
                    var valuesMirror = dm.EngineController.GetMirror(endId);

                    List<Color> colors = new List<Color>();
                    List<double> values = new List<double>();

                    if(colorsMirror != null && colorsMirror.GetData() != null)
                    {
                        var data = colorsMirror.GetData();
                        if (data.IsCollection)
                        {
                            colors.AddRange(data.GetElements().Select(e => e.Data).Cast<Color>());
                        }
                        else
                        {
                            var color = data.Data as Color;
                            if (color != null)
                                colors.Add(color);
                        }
                    }

                    if(valuesMirror != null && valuesMirror.GetData() != null)
                    {
                        var data = valuesMirror.GetData();
                        if (data.IsCollection)
                        {
                            values.AddRange(data.
                                GetElements().
                                Select(e => e.Data).
                                Select(d=>Convert.ToDouble((object)d,CultureInfo.InvariantCulture)));
                        }
                        else
                        {
                            var value = Convert.ToDouble(data.Data, CultureInfo.InvariantCulture);
                            values.Add(value);
                        }
                    }

                    var bmp = CreateColorRangeBitmap(colors, values);
                    drawPlane.Source = bmp;

                });
            };
        }

        public void Dispose() {}

        //http://gaggerostechnicalnotes.blogspot.com/2012/01/wpf-colors-scale.html
        private WriteableBitmap CreateColorRangeBitmap(List<Color> colors, List<double> parameters)
        {
            const int width = 64;
            const int height = 1;

            var bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            var pixels = new uint[width * height];

            var colorRange = DSCore.ColorRange1D.ByColorsAndParameters(colors, parameters);

            for (var i = 1; i <= width; i++)
            {
                var t = (double)i / width;
                var newColor = colorRange.GetColorAtParameter(t);
                pixels[i-1] = (uint)((255 << 24) + (newColor.Red << 16) + (newColor.Green << 8) + newColor.Blue);

            }
            bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 4, 0);

            return bitmap;
        }
    }
}
