using System;
using System.Windows.Controls;
using System.Xml;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DSCore
{
    public class DSColor // TODO(Ben): Rename after namespace support is done :)
    {
        private System.Drawing.Color color = System.Drawing.Color.FromArgb(255, 0, 0, 0);

        // Exposed only for unit test purposes.
        internal System.Drawing.Color InternalColor { get { return this.color; } }

        private DSColor(int a, int r, int g, int b)
        {
            this.color = System.Drawing.Color.FromArgb(a, r, g, b);
        }

        public static DSColor ByARGB(int a, int r, int g, int b)
        {
            return new DSColor(a, r, g, b);
        }

        // This fails "GraphUtilities.PreloadAssembly", fix later.
        // After fixing, restore "TestConstructorBySystemColor" test case.
        // 
#if false
        public static DSColor BySystemColor(System.Drawing.Color c)
        {
            return new DSColor(c.A, c.R, c.G, c.B);
        }
#endif

        public static float Brightness(DSColor c)
        {
            return c.color.GetBrightness();
        }

        public static float Saturation(DSColor c)
        {
            return c.color.GetSaturation();
        }

        public static float Hue(DSColor c)
        {
            return c.color.GetHue();
        }

        public static byte[] Components(DSColor c)
        {
            return new byte[] { c.color.A, c.color.R, c.color.G, c.color.B };
        }
    }

#if false

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
