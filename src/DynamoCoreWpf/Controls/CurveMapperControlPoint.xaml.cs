using Dynamo.Wpf.Controls.SubControls;
using Newtonsoft.Json;
using System.Globalization;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Dynamo.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for CurveMapperControlPoint.xaml
    /// </summary>
    public partial class CurveMapperControlPoint : Thumb
    {
        private const double offsetValue = 6; // thumb size * 0.5
        public CrossHair CrossHairHorizontal { get; set; }
        public CrossHair CrossHairVertical { get; set; }
        public UVCoordText uvText { get; set; }

        public double LimitWidth { get; set; }
        public double LimitHeight { get; set; }

        private Point point;
        [JsonIgnore]
        public Point Point
        {
            get { return point; }
            set
            {
                if (point != value)
                {
                    point = value;
                    Canvas.SetLeft(this, point.X - offsetValue);
                    Canvas.SetTop(this, point.Y - offsetValue);
                }
            }
        }

        public CurveLinear CurveLinear { get; set; }
        // add Bezier here...

        /// <summary>
        /// Initializes a control point with a specific position, movement boundaries, and a high z-index for canvas rendering.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="limitWidth"></param>
        /// <param name="limitHeight"></param>
        public CurveMapperControlPoint(Point position, double limitWidth, double limitHeight)
        {
            InitializeComponent();

            Point = position;

            Canvas.SetLeft(this, position.X - offsetValue);
            Canvas.SetTop(this, position.Y - offsetValue);

            LimitWidth = limitWidth;
            LimitHeight = limitHeight;

            Canvas.SetZIndex(this, 1000);
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            // Calculate new positions for X and Y based on drag changes
            double newX = Canvas.GetLeft(this) + e.HorizontalChange + offsetValue;
            double newY = Canvas.GetTop(this) + e.VerticalChange + offsetValue;

            // Clamp X position within boundaries
            if (newX < 0)
            {
                newX = 0;
            }
            else if (newX > LimitWidth)
            {
                newX = LimitWidth;
            }

            // Clamp Y position within boundaries
            if (newY < 0)
            {
                newY = 0;
            }
            else if (newY > LimitHeight)
            {
                newY = LimitHeight;
            }

            // Update the logical position
            Point = new Point(newX, newY);

            // Update the visual position on the canvas
            Canvas.SetLeft(this, newX - offsetValue);
            Canvas.SetTop(this, newY - offsetValue);

            // Regenerate associated linear curve if applicable
            if (CurveLinear != null)
            {
                CurveLinear.Regenerate();
            }

            //// Additional regenerations (commented out for now)
            //if (clinebez != null)
            //{
            //    clinebez.Regenerate(this);
            //}
            //if (curvebez != null)
            //{
            //    curvebez.Regenerate(this);
            //}

            // Regenerate associated horizontal and vertical crossHairs
            if (CrossHairHorizontal != null)
            {
                CrossHairHorizontal.Regenerate(this);
            }
            if (CrossHairVertical != null)
            {
                CrossHairVertical.Regenerate(this);
            }

            // Regenerate associated UV text display
            if (uvText != null)
            {
                uvText.Regenerate(Point);
            }
        }

        private void Thumb_DragStarted(object sender, DragStartedEventArgs e)
        {
        }

        private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
        }

        public override string ToString()
        {
            return Point.X.ToString() + "," + Point.Y.ToString();
        }
    }

    public class HalfWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double width)
            {
                return -width / 2; // Negative to offset correctly
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
