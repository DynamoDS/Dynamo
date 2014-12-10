using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.Utilities
{
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
