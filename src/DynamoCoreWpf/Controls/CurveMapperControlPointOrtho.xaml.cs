using Dynamo.Wpf.Controls.SubControls;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Dynamo.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for CurveMapperControlPointOrtho.xaml
    /// </summary>
    public partial class CurveMapperControlPointOrtho : Thumb, INotifyPropertyChanged
    {
        private const double offsetValue = 6; // thumb size * 0.5
        private Point point;

        public double LimitWidth { get; set; }
        public double LimitHeight { get; set; }
        public bool IsVertical { get; set; }

        // Define dependency properties for control point limits (Min/Max X, Y) and canvas size,
        // with a common change handler for dynamic updates.
        public static readonly DependencyProperty MinLimitXPropertyOrtho = RegisterProperty(nameof(MinLimitX), 0.0);
        public static readonly DependencyProperty MaxLimitXPropertyOrtho = RegisterProperty(nameof(MaxLimitX), 1.0);
        public static readonly DependencyProperty MinLimitYPropertyOrtho = RegisterProperty(nameof(MinLimitY), 0.0);
        public static readonly DependencyProperty MaxLimitYPropertyOrtho = RegisterProperty(nameof(MaxLimitY), 1.0);
        public static readonly DependencyProperty DynamicCanvasSizeProperty = RegisterProperty(nameof(DynamicCanvasSizeProperty), 240.0);

        /// <summary>Defines the minimum X limit of the control point.</summary>
        public double MinLimitX
        {
            get => (double)GetValue(MinLimitXPropertyOrtho);
            set => SetValue(MinLimitXPropertyOrtho, value);
        }

        /// <summary>Defines the maximum X limit of the control point.</summary>
        public double MaxLimitX
        {
            get => (double)GetValue(MaxLimitXPropertyOrtho);
            set => SetValue(MaxLimitXPropertyOrtho, value);
        }

        /// <summary>Defines the minimum Y limit of the control point.</summary>
        public double MinLimitY
        {
            get => (double)GetValue(MinLimitYPropertyOrtho);
            set => SetValue(MinLimitYPropertyOrtho, value);
        }

        /// <summary>Defines the maximum Y limit of the control point.</summary>
        public double MaxLimitY
        {
            get => (double)GetValue(MaxLimitYPropertyOrtho);
            set => SetValue(MaxLimitYPropertyOrtho, value);
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
                    //RaisePropertyChanged(nameof(ScaledCoordinates));
                }
            }
        }

        /// <summary>
        /// Gets or sets the associated bezier curve for the control point.
        /// </summary>
        public CurveBezier CurveBezier { get; set; }

        /// <summary>
        /// Gets or sets the associated control curve for the control point.
        /// </summary>
        public ControlLine ControlLineBezier { get; set; }

        /// <summary>
        /// Initializes an orthogonal control point with a specific position, movement boundaries, and a high z-index for canvas rendering.
        /// </summary>
        public CurveMapperControlPointOrtho(Point p, bool isVertical, double limitWidth, double limitHeight,
            double minLimitX, double maxLimitX, double minLimitY, double maxLimitY, double canvasSize)
        {
            InitializeComponent();
            DataContext = this;

            Point = p;
            IsVertical = isVertical;
            LimitWidth = limitWidth;
            LimitHeight = limitHeight;
            MinLimitX = minLimitX;
            MaxLimitX = maxLimitX;
            MinLimitY = minLimitY;
            MaxLimitY = maxLimitY;
            CanvasSize = canvasSize;

            Canvas.SetLeft(this, p.X - offsetValue);
            Canvas.SetTop(this, p.Y - offsetValue);
            Canvas.SetZIndex(this, 25);
        }

        /// <summary>
        /// Helper method to register dependency properties with a common property changed callback.
        /// </summary>
        /// <returns></returns>
        private static DependencyProperty RegisterProperty(string name, double defaultValue)
        {
            return DependencyProperty.Register(name, typeof(double), typeof(CurveMapperControlPointOrtho),
                new PropertyMetadata(defaultValue, OnPropertyUpdated));
        }

        /// <summary>
        /// Common property changed handler to raise updates for ScaledCoordinates.
        /// </summary>
        private static void OnPropertyUpdated(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CurveMapperControlPointOrtho controlPoint)
            {
                controlPoint.RaisePropertyChanged(nameof(ScaledCoordinates));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            // Calculate new positions for X and Y based on drag changes
            double newX = Canvas.GetLeft(this) + (IsVertical ? 0.0 : e.HorizontalChange) + offsetValue;
            double newY = Canvas.GetTop(this) + (IsVertical ? e.VerticalChange : 0.0) + offsetValue;

            // Clamp within canvas boundaries
            newX = Math.Max(0, Math.Min(newX, LimitWidth));
            newY = Math.Max(0, Math.Min(newY, LimitHeight));

            // Update the logical position
            Point = new Point(newX, newY);

            // Update the visual position on the canvas
            Canvas.SetLeft(this, newX - offsetValue);
            Canvas.SetTop(this, newY - offsetValue);

            // Regenerate associated linear curve if applicable
            if (ControlLineBezier != null)
            {
                ControlLineBezier.Regenerate(this);
            }
            if (CurveBezier != null)
            {
                CurveBezier.Regenerate(this);
                //double maxv = 0.0;
                //double minv = 0.0;
                //CurveBezier.GetMaximumMinimumOrdinates(LimitHeight, out minv, out maxv);
            }

            // Raise property change notifications
            RaisePropertyChanged(nameof(CanvasSize));
            RaisePropertyChanged(nameof(MinLimitX));
            RaisePropertyChanged(nameof(MinLimitY));
            RaisePropertyChanged(nameof(ScaledCoordinates));
        }

        private void Thumb_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
        }

        private void Thumb_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
        }

        public override string ToString()
        {
            return Point.X.ToString() + "," + Point.Y.ToString();
        }
    }
}
