using CoreNodeModels;
using Dynamo.Wpf.Properties;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Shapes;

namespace Dynamo.Wpf.CurveMapper
{
    /// <summary>
    /// Interaction logic for CurveMapperControlPoint.xaml
    /// </summary>
    public partial class CurveMapperControlPoint : Thumb, INotifyPropertyChanged
    {
        private CurveMapperNodeModel curveMapperNodeModel;
        private const double offsetValue = 6;
        private bool isMoveEnabled = true;
        private bool isOrthogonal;
        private bool isVertical;
        private ControlPointData controlPointData;

        /// <summary>
        /// Represents the dynamic canvas size for the control point.
        /// </summary>
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
                double scaledX = curveMapperNodeModel.MinLimitX +
                                 (controlPointData.X / CanvasSize) *
                                 (curveMapperNodeModel.MaxLimitX - curveMapperNodeModel.MinLimitX);

                double scaledY = curveMapperNodeModel.MinLimitY +
                                 (1 - (controlPointData.Y / CanvasSize)) *
                                 (curveMapperNodeModel.MaxLimitY - curveMapperNodeModel.MinLimitY);

                return $"{CoreNodeModelWpfResources.CurveMapperControlPointCoordinatesLabel}: ({scaledX:F2}, {scaledY:F2})";
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the control point can be moved by the user.
        /// </summary>
        [JsonIgnore]
        public bool IsMoveEnabled
        {
            get => isMoveEnabled;
            set
            {
                if (isMoveEnabled != value)
                {
                    isMoveEnabled = value;
                    UpdateCursor();
                }
            }
        }

        /// <summary>
        /// Identifies the DynamicCanvasSize dependency property, which represents the dynamic canvas size 
        /// used for scaling and rendering the control point.
        /// </summary>
        public static readonly DependencyProperty DynamicCanvasSizeProperty = RegisterProperty(nameof(DynamicCanvasSizeProperty), 240.0);

        /// <summary>
        /// Gets or sets the action invoked when the control point is moved.
        /// </summary>
        public Action OnControlPointMoved { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

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

            controlPointData = controlPoint;
            CanvasSize = canvasSize;
            curveMapperNodeModel = model;
            OnControlPointMoved = updateCurve;
            this.isOrthogonal = isOrthogonal;
            this.isVertical = isVertical;            

            Canvas.SetLeft(this, controlPoint.X - offsetValue);
            Canvas.SetTop(this, controlPoint.Y - offsetValue);
            Canvas.SetZIndex(this, 25);

            UpdateCursor();

            curveMapperNodeModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(curveMapperNodeModel.MinLimitX) ||
                    e.PropertyName == nameof(curveMapperNodeModel.MaxLimitX) ||
                    e.PropertyName == nameof(curveMapperNodeModel.MinLimitY) ||
                    e.PropertyName == nameof(curveMapperNodeModel.MaxLimitY))
                {
                    RaisePropertyChanged(nameof(ScaledCoordinates));
                }
            };

            // Find the tooltip once it's loaded
            this.Loaded += (s, e) =>
            {
                if (this.Template.FindName("ControlPointEllipse", this) is Ellipse ellipse && ellipse.ToolTip is ToolTip tooltip)
                {
                    tooltip.Opened += (sender, args) =>
                    {
                        CenterTooltip(tooltip);
                    };
                }
            };
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (!IsMoveEnabled) return;

            double newCanvasSize = curveMapperNodeModel.DynamicCanvasSize;

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
            double newX = Canvas.GetLeft(this) + (isOrthogonal && isVertical ? 0.0 : deltaX) + offsetValue;
            double newY = Canvas.GetTop(this) + (isOrthogonal && !isVertical ? 0.0 : deltaY) + offsetValue;

            controlPointData.X = newX;
            controlPointData.Y = newY;
            Canvas.SetLeft(this, newX - offsetValue);
            Canvas.SetTop(this, newY - offsetValue);

            // Handle Gaussian curve control points
            string tag = controlPointData.Tag;

            if (tag.Contains("GaussianCurveControlPointData"))
            {
                curveMapperNodeModel.UpdateGaussianCurveControlPoints(deltaX, tag);
            }

            // Refresh scaled coordinates in tooltip
            RaisePropertyChanged(nameof(ScaledCoordinates));

            // Notify the mode and UI to update the curve
            curveMapperNodeModel.GenerateRenderValues();
            OnControlPointMoved?.Invoke();
        }

        private void UpdateCursor()
        {
            this.Cursor = IsMoveEnabled ? Cursors.Hand : Cursors.Arrow;
        }

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
        }

        private void CenterTooltip(ToolTip tooltip)
        {
            tooltip.Dispatcher.BeginInvoke(new Action(() =>
            {
                tooltip.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                double actualWidth = tooltip.DesiredSize.Width;
                tooltip.HorizontalOffset = -actualWidth / 2.0;
            }),
            System.Windows.Threading.DispatcherPriority.Loaded);
        }
    }
}
