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
using Dynamo.Models;
using ProtoCore.Mirror;
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

            model.RequestChangeColorRange += delegate { UpdateColorRange(model, dm, drawPlane); };

            UpdateColorRange(model, dm, drawPlane);
        }

        private void UpdateColorRange(ColorRange model, DynamoModel dm, Image drawPlane)
        {
            model.DispatchOnUIThread(delegate
            {
                if (!model.InPorts[0].Connectors.Any() ||
                    !model.InPorts[1].Connectors.Any())
                {
                    return;
                }

                var colorsNode = model.InPorts[0].Connectors[0].Start.Owner;
                var colorsIndex = model.InPorts[0].Connectors[0].Start.Index;
                var startId = colorsNode.GetAstIdentifierForOutputIndex(colorsIndex).Name;
                var colorsMirror = dm.EngineController.GetMirror(startId);
                var colors = GetColorsFromMirrorData(colorsMirror);

                var valuesNode = model.InPorts[1].Connectors[0].Start.Owner;
                var valuesIndex = model.InPorts[1].Connectors[0].Start.Index;
                var endId = valuesNode.GetAstIdentifierForOutputIndex(valuesIndex).Name;
                var valuesMirror = dm.EngineController.GetMirror(endId);
                var values = GetValuesFromMirrorData(valuesMirror);

                var bmp = CreateColorRangeBitmap(colors, values);
                drawPlane.Source = bmp;

            });
        }

        private static List<double> GetValuesFromMirrorData(RuntimeMirror valuesMirror)
        {
            var values = new List<double>();

            if (valuesMirror == null || valuesMirror.GetData() == null) return values;

            var data = valuesMirror.GetData();
            if (data.IsCollection)
            {
                var elements = data.GetElements().Select(e => e.Data);
                foreach (var element in elements)
                {
                    double parsed;
                    if (TryConvertToDouble(element, out parsed))
                        values.Add(parsed);
                }
            }
            else
            {
                double parsed;
                if (TryConvertToDouble(data.Data, out parsed))
                    values.Add(parsed);
            }
            return values;
        }

        private static List<Color> GetColorsFromMirrorData(RuntimeMirror colorsMirror)
        {
            var colors = new List<Color>();

            if (colorsMirror == null || colorsMirror.GetData() == null) return colors;

            var data = colorsMirror.GetData();
            if (data != null)
            {
                if (data.IsCollection)
                {
                    colors.AddRange(data.GetElements().Select(e => e.Data).OfType<Color>());
                }
                else
                {
                    var color = data.Data as Color;
                    if (color != null)
                        colors.Add(color);
                }
            }

            return colors;
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

        private static bool TryConvertToDouble(object value, out double parsed)
        {
            parsed = default(double);

            try
            {
                parsed = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
