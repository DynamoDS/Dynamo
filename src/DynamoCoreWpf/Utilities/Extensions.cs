namespace Dynamo.Utilities
{
    public static class ColorExtensions
    {
        /// <summary>
        /// Convert a color to a 24bit hex representation
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        public static string ToHex(this System.Windows.Media.Color col)
        {
            var red = col.R.ToString("X2");
            var green = col.G.ToString("X2");
            var blue = col.B.ToString("X2");
            return $"#{red}{green}{blue}";
        }
    }

    public static class WindowsBaseExtensions
    {
        public static System.Windows.Thickness AsWindowsType(this Dynamo.Utilities.Thickness thickness)
        {
            return new System.Windows.Thickness(
                thickness.Left,
                thickness.Top,
                thickness.Right,
                thickness.Bottom);
        }

        public static Dynamo.Utilities.Point2D AsDynamoType(this System.Windows.Point point)
        {
            return new Dynamo.Utilities.Point2D(point.X, point.Y);
        }

        public static System.Windows.Point AsWindowsType(this Dynamo.Utilities.Point2D point)
        {
            return new System.Windows.Point(point.X, point.Y);
        }

        public static Dynamo.Utilities.ModifierKeys AsDynamoType(this System.Windows.Input.ModifierKeys keys)
        {
            switch (keys)
            {
                case System.Windows.Input.ModifierKeys.Alt:
                    return ModifierKeys.Alt;
                case System.Windows.Input.ModifierKeys.Control:
                    return ModifierKeys.Control;
                case System.Windows.Input.ModifierKeys.None:
                    return ModifierKeys.None;
                case System.Windows.Input.ModifierKeys.Shift:
                    return ModifierKeys.Shift;
                case System.Windows.Input.ModifierKeys.Windows:
                    return ModifierKeys.Windows;
            }

            // This is an exhaustive enumeration, so this should never be hit unless Windows
            // changes their API
            return ModifierKeys.None;
        }
    }
}
