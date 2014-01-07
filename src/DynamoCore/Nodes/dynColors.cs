using System;
using System.Windows.Controls;
using System.Xml;
using Dynamo.Controls;
using Dynamo.Models;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;

namespace Dynamo.Nodes
{
    [NodeName("Color Brightness")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_COLOR)]
    [NodeDescription("Calculates a color's brightness.")]
    class ColorBrightness : NodeWithOneOutput
    {
        public ColorBrightness()
        {
            InPortData.Add(new PortData("c", "The color", typeof(Value.Container)));
            OutPortData.Add(new PortData("mag", "The magnitude of the color vector", typeof(Value.Number)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var c = (System.Drawing.Color)((Value.Container)args[0]).Item;

            return Value.NewNumber(c.GetBrightness());
        } 
    }

    [NodeName("Color Saturation")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_COLOR)]
    [NodeDescription("Calculates a color's saturation.")]
    class ColorSaturation : NodeWithOneOutput
    {
        public ColorSaturation()
        {
            InPortData.Add(new PortData("c", "The color", typeof(Value.Container)));
            OutPortData.Add(new PortData("sat", "The saturation of the color as a number between 0 and 1", typeof(Value.Number)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var c = (System.Drawing.Color)((Value.Container)args[0]).Item;

            return Value.NewNumber(c.GetSaturation());
        }
    }

    [NodeName("Color")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_COLOR)]
    [NodeDescription("Make a color from its alpha, red, green, and blue components.")]
    [NodeSearchTags("argb")]
    class Color : NodeWithOneOutput
    {
        public Color()
        {
            InPortData.Add(new PortData("A", "The alpha part of the color between 0 and 255", typeof(Value.Number), Value.NewNumber(255)));
            InPortData.Add(new PortData("R", "The red part of the color between 0 and 255", typeof(Value.Number), Value.NewNumber(0)));
            InPortData.Add(new PortData("G", "The green part of the color between 0 and 255", typeof(Value.Number), Value.NewNumber(0)));
            InPortData.Add(new PortData("B", "The blue part of the color between 0 and 255", typeof(Value.Number), Value.NewNumber(0)));
            OutPortData.Add(new PortData("c", "The color", typeof(Value.Container)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var a = (int) Math.Round(((Value.Number) args[0]).Item);
            var r = (int) Math.Round(((Value.Number)args[1]).Item);
            var g = (int) Math.Round(((Value.Number)args[2]).Item);
            var b = (int) Math.Round(((Value.Number)args[3]).Item);

            return Value.NewContainer(System.Drawing.Color.FromArgb(a, r, g, b));
        }
    }

    [NodeName("Color Components")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_COLOR)]
    [NodeDescription("Separate a color into its alpha, red, green, and blue components.")]
    [NodeSearchTags("argb")]
    class ColorComponents : NodeModel
    {
        private readonly PortData _alphaOut = new PortData(
            "A", "The alpha part of the color between 0 and 255", typeof(Value.Number));

        private readonly PortData _redOut = new PortData(
            "R", "The red part of the color between 0 and 255", typeof(Value.Number));

        private readonly PortData _greenOut = new PortData(
            "G", "The green part of the color between 0 and 255", typeof(Value.Number));

        private readonly PortData _blueOut = new PortData(
            "B", "The blue part of the color between 0 and 255", typeof(Value.Number));

        public ColorComponents()
        {
            InPortData.Add(new PortData("c", "The color", typeof(Value.Container)));
            OutPortData.Add(_alphaOut);
            OutPortData.Add(_redOut);
            OutPortData.Add(_greenOut);
            OutPortData.Add(_blueOut);

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override void Evaluate(FSharpList<Value> args, Dictionary<PortData, Value> outPuts)
        {
            var c = (System.Drawing.Color)((Value.Container)args[0]).Item;

            outPuts[_alphaOut] = Value.NewNumber(c.A);
            outPuts[_redOut] = Value.NewNumber(c.R);
            outPuts[_greenOut] = Value.NewNumber(c.G);
            outPuts[_blueOut] = Value.NewNumber(c.B);
        }
    }

    [NodeName("Color Hue")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_COLOR)]
    [NodeDescription("Calculates a color's hue.")]
    class ColorHue : NodeWithOneOutput
    {
        public ColorHue()
        {
            InPortData.Add(new PortData("c", "The color", typeof(Value.Container)));
            OutPortData.Add(new PortData("hue", "The hue of the color as a number between 0 and 1", typeof(Value.Number)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var c = (System.Drawing.Color)((Value.Container)args[0]).Item;

            return Value.NewNumber(c.GetHue());
        }
    }

    public class ColorRangeEventArgs : EventArgs
    {
        System.Drawing.Color Start { get; set; }
        System.Drawing.Color End { get; set; }
        public ColorRangeEventArgs(System.Drawing.Color start, System.Drawing.Color end)
        {
            Start = start;
            End = end;
        }
    }

    [NodeName("Color Range")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_COLOR)]
    [NodeDescription("Get a color given a color range.")]
    class ColorRange : NodeWithOneOutput
    {
        private System.Drawing.Color _start;
        private System.Drawing.Color _end;

        public event EventHandler RequestChangeColorRange;
        protected virtual void OnRequestChangeColorRange(object sender, EventArgs e)
        {
            if (RequestChangeColorRange != null)
                RequestChangeColorRange(sender, e);
        }

        public ColorRange()
        {
            InPortData.Add(new PortData("start", "The start color.", typeof(Value.Container)));
            InPortData.Add(new PortData("end", "The end color.", typeof(Value.Container)));
            InPortData.Add(new PortData("value", "The value between 0 and 1 of the selected color.", typeof(Value.Number)));

            OutPortData.Add(new PortData("color", "The selected color.", typeof(Value.Container)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;

            _start = System.Drawing.Color.Blue;
            _end = System.Drawing.Color.Red;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            _start = (System.Drawing.Color)((Value.Container)args[0]).Item;
            _end = (System.Drawing.Color)((Value.Container)args[1]).Item;
            var value = ((Value.Number)args[2]).Item;

            if (value > 1.0 || value < 0.0)
                throw new Exception("Please enter a value between 0.0 and 1.0");

            OnRequestChangeColorRange(this, EventArgs.Empty);

            var selRed = (int)(_start.R + (_end.R - _start.R)*value);
            var selGreen = (int)(_start.G + (_end.G - _start.G)*value);
            var selBlue = (int)(_start.B + (_end.B - _start.B)*value);

            var returnColor = System.Drawing.Color.FromArgb(selRed, selGreen, selBlue);
            return Value.NewContainer(returnColor);
        }

        public void SetupCustomUIElements(dynNodeView nodeUI)
        {
            base.InitializeUI(nodeUI);
            
            var drawPlane = new Image
                {
                    Stretch = Stretch.Fill,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Width = 100,
                    Height = 200 
                };

            nodeUI.inputGrid.Children.Add(drawPlane);

            RequestChangeColorRange += delegate
            {
                DispatchOnUIThread(delegate
                {
                    WriteableBitmap bmp = CompleteColorScale(_start, _end);
                    drawPlane.Source = bmp; 
                });
            };
        }

        //http://gaggerostechnicalnotes.blogspot.com/2012/01/wpf-colors-scale.html
        private WriteableBitmap CompleteColorScale(System.Drawing.Color start, System.Drawing.Color end)
        {
            //var drawPlane = new System.Windows.Controls.Image();

            const int size = 64;

            const int width = 1;
            const int height = size;

            var bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            var pixels = new uint[width * height];

            for (int i = 0; i < size; i++)
            {
                var newRed = start.R + ((end.R - start.R)/size)*i;
                var newGreen = start.G + ((end.G - start.G) / size) * i;
                var newBlue = start.B + ((end.B - start.B) / size) * i;

                //pixels[i] = (uint)((newBlue << 16) + (newGreen << 8) + (newRed << 0));
                pixels[i] = (uint)((255 << 24) + (newRed << 16) + (newGreen << 8) + newBlue);

            }
            bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 4, 0);

            return bitmap;
        }

        [NodeMigration("0.6.2.0","0.6.3.0")]
        public void UpdateLacability(XmlNode node)
        {
            //if the laceability has been set on this node to disabled, then set it to longest
            if (node.Attributes["lacing"].Value == "Disabled")
            {
                node.Attributes["lacing"].Value = "Longest";
            }
        }
    }
}
