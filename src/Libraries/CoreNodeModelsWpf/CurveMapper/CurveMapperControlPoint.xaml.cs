using CoreNodeModels;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Dynamo.Wpf.CurveMapper
{
    /// <summary>
    /// Interaction logic for CurveMapperControlPoint.xaml
    /// </summary>
    public partial class CurveMapperControlPoint : Thumb, INotifyPropertyChanged
    {
        public CurveMapperNodeModel AssociatedModel { get; set; }
        public Action OnControlPointMoved { get; set; } // Notify the curve to update

        public ControlPointData ControlPointData { get; private set; }


        private const double offsetValue = 6; // thumb size * 0.5
        private Point point;
        private bool isEnabled = true;

        public bool IsOrthogonal { get; set; }
        public bool IsVertical { get; set; }

        public static readonly DependencyProperty DynamicCanvasSizeProperty = RegisterProperty(nameof(DynamicCanvasSizeProperty), 240.0);

        /// <summary>Represents the dynamic canvas size for the control point.</summary>
        public double CanvasSize
        {
            get => (double)GetValue(DynamicCanvasSizeProperty);
            set => SetValue(DynamicCanvasSizeProperty, value);
        }

        /// <summary>
        /// Gets the scaled coordinates of the control point, normalized based on Min/Max limits.
        /// </summary>
        [JsonIgnore]
        public string ScaledCoordinates
        {
            get
            {
                double scaledX = AssociatedModel.MinLimitX +
                                 (ControlPointData.X / CanvasSize) *
                                 (AssociatedModel.MaxLimitX - AssociatedModel.MinLimitX);

                double scaledY = AssociatedModel.MinLimitY +
                                 (1 - (ControlPointData.Y / CanvasSize)) *
                                 (AssociatedModel.MaxLimitY - AssociatedModel.MinLimitY);

                return $"Coordinates: ({scaledX:F2}, {scaledY:F2})";
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

        public CurveMapperControlPoint(
            ControlPointData controlPoint,
            double canvasSize,
            CurveMapperNodeModel model,
            Action updateCurve,
            bool isOrthogonal = false, bool isVertical = false
        )        
        {
            InitializeComponent();
            DataContext = this;

            ControlPointData = controlPoint;
            CanvasSize = canvasSize;
            AssociatedModel = model;
            OnControlPointMoved = updateCurve;
            IsOrthogonal = isOrthogonal;
            IsVertical = isVertical;            

            Canvas.SetLeft(this, controlPoint.X - offsetValue);
            Canvas.SetTop(this, controlPoint.Y - offsetValue);
            Canvas.SetZIndex(this, 25);

            UpdateCursor();

            AssociatedModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(AssociatedModel.MinLimitX) ||
                    e.PropertyName == nameof(AssociatedModel.MaxLimitX) ||
                    e.PropertyName == nameof(AssociatedModel.MinLimitY) ||
                    e.PropertyName == nameof(AssociatedModel.MaxLimitY))
                {
                    RaisePropertyChanged(nameof(ScaledCoordinates));
                }
            };
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
                //controlPoint.RaisePropertyChanged(nameof(ScaledCoordinates));
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

            double newCanvasSize = AssociatedModel.DynamicCanvasSize;

            // Calculate horizontal and vertical deltas within canvas boundaries
            // Requires for Gaussian control points.
            var deltaL = Canvas.GetLeft(this) + offsetValue;
            var deltaR = newCanvasSize - Canvas.GetLeft(this) - offsetValue;
            var deltaX = e.HorizontalChange > 0 ?
                Math.Min(e.HorizontalChange, deltaR) :
                Math.Max(e.HorizontalChange, -deltaL);

            var deltaT = Canvas.GetTop(this) + offsetValue;
            var deltaB = newCanvasSize - Canvas.GetTop(this) - offsetValue;
            var deltaY = e.VerticalChange > 0 ?
                Math.Min(e.VerticalChange, deltaB) :
                Math.Max(e.VerticalChange, -deltaT);

            // Calculate new positions for X and Y based on drag changes
            double newX = Canvas.GetLeft(this) + (IsOrthogonal && IsVertical ? 0.0 : deltaX) + offsetValue;
            double newY = Canvas.GetTop(this) + (IsOrthogonal && !IsVertical ? 0.0 : deltaY) + offsetValue;

            // Update ControlPointData with new relative position
            ControlPointData.X = newX;
            ControlPointData.Y = newY;
            Canvas.SetLeft(this, newX - offsetValue);
            Canvas.SetTop(this, newY - offsetValue);

            // Handle Gaussian curve control points
            string tag = ControlPointData.Tag;

            if (tag.Contains("GaussianCurveControlPointData"))
            {
                AssociatedModel.UpdateGaussianCurveControlPoints(deltaX, tag);
            }            

            // Refresh scaled coordinates in tooltip
            RaisePropertyChanged(nameof(ScaledCoordinates));

            // Notify the mode and UI to update the curve
            AssociatedModel.GenerateOutputValues();
            OnControlPointMoved?.Invoke();
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
