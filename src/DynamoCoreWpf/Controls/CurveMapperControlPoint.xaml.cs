using Dynamo.Wpf.Controls.SubControls;
using Newtonsoft.Json;
using System.Globalization;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.ComponentModel;
using System.Diagnostics;

namespace Dynamo.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for CurveMapperControlPoint.xaml
    /// </summary>
    public partial class CurveMapperControlPoint : Thumb, INotifyPropertyChanged
    {
        private const double offsetValue = 6; // thumb size * 0.5
        private Point point;

        public CrossHair CrossHairHorizontal { get; set; }
        public CrossHair CrossHairVertical { get; set; }
        public UVCoordText uvText { get; set; }
        public double LimitWidth { get; set; }
        public double LimitHeight { get; set; }


        // Define dependency properties for control point limits (Min/Max X, Y) and canvas size,
        // with a common change handler for dynamic updates.
        public static readonly DependencyProperty MinLimitXProperty = RegisterProperty(nameof(MinLimitX), 0.0);
        public static readonly DependencyProperty MaxLimitXProperty = RegisterProperty(nameof(MaxLimitX), 1.0);
        public static readonly DependencyProperty MinLimitYProperty = RegisterProperty(nameof(MinLimitY), 0.0);
        public static readonly DependencyProperty MaxLimitYProperty = RegisterProperty(nameof(MaxLimitY), 1.0);
        public static readonly DependencyProperty DynamicCanvasSizeProperty = RegisterProperty(nameof(DynamicCanvasSizeProperty), 240.0);

        /// <summary>Defines the minimum X limit of the control point.</summary>
        public double MinLimitX
        {
            get => (double)GetValue(MinLimitXProperty);
            set => SetValue(MinLimitXProperty, value);
        }

        /// <summary>Defines the maximum X limit of the control point.</summary>
        public double MaxLimitX
        {
            get => (double)GetValue(MaxLimitXProperty);
            set => SetValue(MaxLimitXProperty, value);
        }

        /// <summary>Defines the minimum Y limit of the control point.</summary>
        public double MinLimitY
        {
            get => (double)GetValue(MinLimitYProperty);
            set => SetValue(MinLimitYProperty, value);
        }

        /// <summary>Defines the maximum Y limit of the control point.</summary>
        public double MaxLimitY
        {
            get => (double)GetValue(MaxLimitYProperty);
            set => SetValue(MaxLimitYProperty, value);
        }

        /// <summary>Represents the dynamic canvas size for the control point.</summary>
        public double CanvasSize
        {
            get => (double)GetValue(DynamicCanvasSizeProperty);
            set => SetValue(DynamicCanvasSizeProperty, value);
        }

        /// <summary>
        /// Gets the scaled coordinates of the control point, mapped to user-defined limits and formatted for display.
        /// </summary>
        public string ScaledCoordinates
        {
            get
            {
                string formatNumber(double value)
                {
                    return value % 1 == 0 ? value.ToString("F0") : value.ToString("F2");
                }

                double scaledX = MinLimitX + (Point.X / CanvasSize) * (MaxLimitX - MinLimitX);
                double scaledY = MinLimitY + (1 - (Point.Y / CanvasSize)) * (MaxLimitY - MinLimitY);

                return $"Control Point ({formatNumber(scaledX)}, {formatNumber(scaledY)})";
            }
        }        
        
        /// <summary>
        /// Gets or sets the position of the control point on the canvas.
        /// </summary>
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

                    RaisePropertyChanged(nameof(Point));
                    RaisePropertyChanged(nameof(ScaledCoordinates));
                }
            }
        }

        /// <summary>
        /// Gets or sets the associated linear curve for the control point.
        /// </summary>
        public CurveLinear CurveLinear { get; set; }

        // add Bezier here...

        /// <summary>
        /// Initializes a control point with a specific position, movement boundaries, and a high z-index for canvas rendering.
        /// </summary>
        public CurveMapperControlPoint(Point position, double limitWidth, double limitHeight,
            double minLimitX, double maxLimitX, double minLimitY, double maxLimitY, double canvasSize)
        {
            InitializeComponent();
            DataContext = this;

            Point = position;
            LimitWidth = limitWidth;
            LimitHeight = limitHeight;
            MinLimitX = minLimitX;
            MaxLimitX = maxLimitX;
            MinLimitY = minLimitY;
            MaxLimitY = maxLimitY;
            CanvasSize = canvasSize;

            Canvas.SetLeft(this, position.X - offsetValue);
            Canvas.SetTop(this, position.Y - offsetValue);
            Canvas.SetZIndex(this, 25);

            DataContext = this;
        }

        /// <summary>
        /// Helper method to register dependency properties with a common property changed callback.
        /// </summary>
        /// <returns></returns>
        private static DependencyProperty RegisterProperty(string name, double defaultValue)
        {
            return DependencyProperty.Register(name, typeof(double), typeof(CurveMapperControlPoint),
                new PropertyMetadata(defaultValue, OnPropertyUpdated));
        }

        /// <summary>
        /// Common property changed handler to raise updates for ScaledCoordinates.
        /// </summary>
        private static void OnPropertyUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CurveMapperControlPoint controlPoint)
            {
                controlPoint.RaisePropertyChanged(nameof(ScaledCoordinates));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
