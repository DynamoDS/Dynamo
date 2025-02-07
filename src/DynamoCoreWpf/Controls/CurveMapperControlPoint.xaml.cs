using Dynamo.Wpf.Controls.SubControls;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Dynamo.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for CurveMapperControlPoint.xaml
    /// </summary>
    public partial class CurveMapperControlPoint : Thumb, INotifyPropertyChanged
    {
        private const double offsetValue = 6; // thumb size * 0.5
        private Point point;
        private bool isEnabled = true;

        public bool IsOrthogonal { get; set; }
        public bool IsVertical { get; set; }
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
                    UpdateBounds(); // Call to update visibility
                }
            }
        }
        
        [JsonIgnore]
        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if (isEnabled != value)
                {
                    isEnabled = value;
                    UpdateCursor();
                }
            }
        }

        /// <summary>
        /// Dependency property that indicates whether the control point is within the canvas bounds.
        /// </summary>
        public static readonly DependencyProperty IsWithinBoundsProperty =
            DependencyProperty.Register(
                nameof(IsWithinBounds), typeof(bool), typeof(CurveMapperControlPoint), new PropertyMetadata(true)
            );

        /// <summary>
        /// Gets or sets a value indicating whether the control point is within the canvas bounds.
        /// </summary>
        public bool IsWithinBounds
        {
            get => (bool)GetValue(IsWithinBoundsProperty);
            set => SetValue(IsWithinBoundsProperty, value);
        }

        /// <summary>
        /// Gets or sets the associated linear curve for the control point.
        /// </summary>
        public LinearCurve CurveLinear { get; set; }

        /// <summary>
        /// Gets or sets the associated bezier curve for the control point.
        /// </summary>
        public BezierCurve CurveBezier { get; set; }

        /// <summary>
        /// Gets or sets the associated sine curve for the control point.
        /// </summary>
        public SineCurve CurveSine { get; set; }

        /// <summary>
        /// Gets or sets the associated cosine curve for the control point.
        /// </summary>
         // TODO: check if we should have separate Cosine Curve class
        public SineCurve CurveCosine { get; set; }

        /// <summary>
        /// Gets or sets the associated parabolic curve for the control point.
        /// </summary>
        public ParabolicCurve CurveParabolic { get; set; }

        /// <summary>
        /// Gets or sets the associated perlin curve for the control point.
        /// </summary>
        public PerlinCurve CurvePerlin { get; set; }

        /// <summary>
        /// Gets or sets the associated power curve for the control point.
        /// </summary>
        public PowerCurve CurvePower { get; set; }

        /// <summary>
        /// Gets or sets the associated square root curve for the control point.
        /// </summary>
        public SquareRootCurve SquareRootCurve { get; set; }

        /// <summary>
        /// Gets or sets the associated gaussian curve for the control point.
        /// </summary>
        public GaussianCurve GaussianCurve { get; set; }

        /// <summary>
        /// Gets or sets the associated control curve for the control point.
        /// </summary>
        public ControlLine ControlLineBezier { get; set; }

        /// <summary>
        /// Initializes a control point with a specific position, movement boundaries, and a high z-index for canvas rendering.
        /// </summary>
        public CurveMapperControlPoint(Point position, double limitWidth, double limitHeight,
            double minLimitX, double maxLimitX, double minLimitY, double maxLimitY, double canvasSize, bool isOrthogonal = false, bool isVertical = false)
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
            IsOrthogonal = isOrthogonal;
            IsVertical = isVertical;

            Canvas.SetLeft(this, position.X - offsetValue);
            Canvas.SetTop(this, position.Y - offsetValue);
            Canvas.SetZIndex(this, 25);

            UpdateCursor();
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
            if (!IsEnabled) return;

            // Calculate new positions for X and Y based on drag changes
            double newX = Canvas.GetLeft(this) + (IsOrthogonal && IsVertical ? 0.0 : e.HorizontalChange) + offsetValue;
            double newY = Canvas.GetTop(this) + (IsOrthogonal && !IsVertical ? 0.0 : e.VerticalChange) + offsetValue;


            // Clamp within canvas boundaries
            newX = Math.Max(0, Math.Min(newX, LimitWidth));
            newY = Math.Max(0, Math.Min(newY, LimitHeight));

            // Update the logical position
            Point = new Point(newX, newY);

            // Update the visual position on the canvas
            Canvas.SetLeft(this, newX - offsetValue);
            Canvas.SetTop(this, newY - offsetValue);

            // Regenerate associated elements
            CurveLinear?.Regenerate();
            ControlLineBezier?.Regenerate(this); 
            CurveBezier?.Regenerate(this);
            CurveSine?.Regenerate();
            CurveCosine?.Regenerate();
            CurveParabolic?.Regenerate(this);
            CurvePerlin?.Regenerate();
            CurvePower?.Regenerate();
            SquareRootCurve?.Regenerate();
            GaussianCurve?.Regenerate();

        }

        private void Thumb_DragStarted(object sender, DragStartedEventArgs e)
        {
        }

        private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
        }

        private void UpdateCursor()
        {
            this.Cursor = IsEnabled ? Cursors.Hand : Cursors.Arrow;
        }

        private void UpdateBounds()
        {
            IsWithinBounds = Point.X >= 0 && Point.X <= LimitWidth;
            RaisePropertyChanged(nameof(IsWithinBounds));
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
