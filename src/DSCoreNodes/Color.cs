﻿using System;
using System.Windows.Controls;
using System.Xml;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DSCoreNodes
{
    public class DSColor // TODO(Ben): Rename after namespace support is done :)
    {
        private System.Drawing.Color color = System.Drawing.Color.FromArgb(255, 0, 0, 0);

        private DSColor(int a, int r, int g, int b)
        {
            this.color = System.Drawing.Color.FromArgb(a, r, g, b);
        }

        public static DSColor ByARGB(int a, int r, int g, int b)
        {
            return new DSColor(a, r, g, b);
        }
    }

#if false
    public class ColorBrightness
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

    public class ColorBrightness
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

    class ColorSaturation
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

    class ColorComponents
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

    class ColorHue
    {
        public ColorHue()
        {
            InPortData.Add(new PortData("c", "The color", typeof(Value.Container)));
            OutPortData.Add(new PortData("hue", "The hue of the color as a number between 0 and 1", typeof(Value.Number)));
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var c = (System.Drawing.Color)((Value.Container)args[0]).Item;

            return Value.NewNumber(c.GetHue());
        }
    }

    class ColorRange
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
    }
#endif
}
