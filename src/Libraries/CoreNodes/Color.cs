using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;

namespace DSCore
{
    public class Color
    {
        private System.Drawing.Color color = System.Drawing.Color.FromArgb(255, 0, 0, 0);

        // Exposed only for unit test purposes.
        internal System.Drawing.Color InternalColor { get { return this.color; } }

        /// <summary>
        ///     Find the red component of a color, 0 to 255.
        /// </summary>
        /// <returns name="val">Value of the red component.</returns>
        /// <search>red</search>
        public byte Red
        {
            get { return color.R; }
        }

        /// <summary>
        ///     Find the green component of a color, 0 to 255.
        /// </summary>
        /// <returns name="val">Value of the green component.</returns>
        /// <search>green</search>
        public byte Green
        {
            get { return color.G; }
        }

        /// <summary>
        ///     Find the blue component of a color, 0 to 255.
        /// </summary>
        /// <returns name="val">Value of the blue component.</returns>
        /// <search>blue</search>
        public byte Blue
        {
            get { return color.B; }
        }

        /// <summary>
        ///     Find the alpha component of a color, 0 to 255.
        /// </summary>
        /// <returns name="val">Value of the alpha component.</returns>
        /// <search>alpha</search>
        public byte Alpha
        {
            get { return color.A; }
        }

        private Color(int a, int r, int g, int b)
        {
            this.color = System.Drawing.Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        ///     Construct a color by alpha, red, green, and blue components.
        /// </summary>
        /// <param name="a">The alpha value.</param>
        /// <param name="r">The red value.</param>
        /// <param name="g">The green value.</param>
        /// <param name="b">The blue value.</param>
        /// <returns name="color">Color.</returns>
        /// <search>color</search>
        public static Color ByARGB(int a, int r, int g, int b)
        {
            return new Color(a, r, g, b);
        }

        // This fails "GraphUtilities.PreloadAssembly", fix later.
        // After fixing, restore "TestConstructorBySystemColor" test case.
        // 
#if false
        public static Color BySystemColor(System.Drawing.Color c)
        {
            return new Color(c.A, c.R, c.G, c.B);
        }
#endif

        /// <summary>
        ///     Gets the birghtness value for this color.
        /// </summary>
        /// <returns name="val">Brightness value for the color.</returns>
        /// <search>brightness</search>
        public static float Brightness(Color c)
        {
            return c.color.GetBrightness();
        }

        /// <summary>
        ///     Gets the saturation value for this color.
        /// </summary>
        /// <returns name="val">Saturation value for the color.</returns>
        /// <search>saturation</search>
        public static float Saturation(Color c)
        {
            return c.color.GetSaturation();
        }

        /// <summary>
        ///     Gets the hue value for this color.
        /// </summary>
        /// <returns name="val">Hue value for the color.</returns>
        /// <search>hue</search>
        public static float Hue(Color c)
        {
            return c.color.GetHue();
        }

        /// <summary>
        ///     Lists the components for the color in the order: alpha, red, green, blue.
        /// </summary>
        /// <returns name="val">Saturation value for the color.</returns>
        /// <search>components,alpha,red,green,blue</search>
        [MultiReturn(new string[] {"a", "r", "g", "b"})]
        public static Dictionary<string, byte> Components(Color c)
        {
            return new Dictionary<string, byte>
            {
                {"a", c.color.A}, 
                {"r", c.color.R},
                {"g", c.color.G},
                {"b", c.color.B}, 
            };
        }

        /// <summary>
        ///     Get a color from a color gradient between a start color and an end color.
        /// </summary>
        /// <param name="start">The starting color of the range.</param>
        /// <param name="end">The end color of the range.</param>
        /// <param name="value">The value between 0 and 1 along the range for which you would like to sample the color.</param>
        /// <returns name="color">Color in the given range.</returns>
        /// <search>color,range,gradient</search>
        [IsVisibleInDynamoLibrary(false)]
        public static Color BuildColorFromRange(Color start, Color end, double value)
        {
            var selRed = (int)(start.Red + (end.Red - start.Red) * value);
            var selGreen = (int)(start.Green + (end.Green - start.Green) * value);
            var selBlue = (int)(start.Blue + (end.Blue - start.Blue) * value);

            return ByARGB(255, selRed, selGreen, selBlue);
        }

        public override string ToString()
        {
            return string.Format("Color: Red={0}, Green={1}, Blue={2}, Alpha={3}", Red, Green, Blue, Alpha);
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
