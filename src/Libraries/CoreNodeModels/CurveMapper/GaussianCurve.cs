using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreNodeModels.CurveMapper
{
    public class GaussianCurve
    {
        private double CanvasSize;
        private double ControlPoint1X;
        private double ControlPoint1Y;
        private double ControlPoint2X;
        private double ControlPoint2Y;
        private double ControlPoint3X;
        private double ControlPoint3Y;
        private double ControlPoint4X;
        private double ControlPoint4Y;
        private const double renderIncrementX = 1.0; // ADD THIS BASE CLASS ?

        private double lastControlPoint2X;
        private double previousHorizontalOffset;

        /// <summary>
        /// Indicates whether the node is currently being resized, preventing unintended control point updates.
        /// </summary>
        public bool IsResizing { get; set; } = false;

        public GaussianCurve(double cp1X, double cp1Y, double cp2X, double cp2Y, double cp3X, double cp3Y, double cp4X, double cp4Y, double canvasSize)
        {
            CanvasSize = canvasSize;
            ControlPoint1X = cp1X;
            ControlPoint1Y = cp1Y;
            ControlPoint2X = cp2X;
            ControlPoint2Y = cp2Y;
            ControlPoint1X = cp3X;
            ControlPoint1Y = cp3Y;
            ControlPoint2X = cp4X;
            ControlPoint2Y = cp4Y;
        }






        //private void ControlPoint2_Changed(object sender, PropertyChangedEventArgs e)
        //{
        //    if (IsResizing) return;

        //    if (e.PropertyName == nameof(CurveMapperControlPoint.Point))
        //    {
        //        double deltaX = controlPoint2.Point.X - lastControlPoint2X;

        //        if (deltaX == 0) return;

        //        // Temporarily unsubscribe events to prevent recursion
        //        controlPoint3.PropertyChanged -= ControlPoint3_Changed;
        //        controlPoint4.PropertyChanged -= ControlPoint4_Changed;

        //        controlPoint3.Point = new Point(controlPoint3.Point.X + deltaX, controlPoint3.Point.Y);
        //        controlPoint4.Point = new Point(controlPoint4.Point.X + deltaX, controlPoint4.Point.Y);
        //        lastControlPoint2X = controlPoint2.Point.X;

        //        // Re-subscribe events
        //        controlPoint3.PropertyChanged += ControlPoint3_Changed;
        //        controlPoint4.PropertyChanged += ControlPoint4_Changed;
        //    }
        //}

        //private void ControlPoint3_Changed(object sender, PropertyChangedEventArgs e)
        //{
        //    if (IsResizing) return;
        //    if (e.PropertyName == nameof(CurveMapperControlPoint.Point))
        //    {
        //        var horizontalOffset = controlPoint2.Point.X - controlPoint3.Point.X;
        //        if (horizontalOffset == previousHorizontalOffset) return;

        //        // Temporarily unsubscribe events to prevent recursion
        //        controlPoint2.PropertyChanged -= ControlPoint2_Changed;
        //        controlPoint4.PropertyChanged -= ControlPoint4_Changed;

        //        controlPoint4.Point = new Point(controlPoint2.Point.X + horizontalOffset, controlPoint4.Point.Y);
        //        previousHorizontalOffset = horizontalOffset;

        //        // Re-subscribe events
        //        controlPoint2.PropertyChanged += ControlPoint2_Changed;
        //        controlPoint4.PropertyChanged += ControlPoint4_Changed;
        //    }
        //}
        //private void ControlPoint4_Changed(object sender, PropertyChangedEventArgs e)
        //{
        //    if (IsResizing) return;

        //    if (e.PropertyName == nameof(CurveMapperControlPoint.Point))
        //    {
        //        var horizontalOffset = controlPoint2.Point.X - controlPoint4.Point.X;
        //        if (horizontalOffset == previousHorizontalOffset) return;

        //        // Temporarily unsubscribe events to prevent recursion
        //        controlPoint2.PropertyChanged -= ControlPoint2_Changed;
        //        controlPoint3.PropertyChanged -= ControlPoint3_Changed;

        //        controlPoint3.Point = new Point(controlPoint2.Point.X + horizontalOffset, controlPoint3.Point.Y);
        //        previousHorizontalOffset = horizontalOffset;

        //        // Re-subscribe events
        //        controlPoint2.PropertyChanged += ControlPoint2_Changed;
        //        controlPoint3.PropertyChanged += ControlPoint3_Changed;
        //    }
        //}

        private List<double>[] GenerateGaussianCurve()
        {
            bool peakX = false;
            double xLeft = double.NaN, yLeft = 0;
            double xRight = double.NaN, yRight = 0;


            var valuesXleft = new List<double> ();
            var valuesYleft = new List<double> ();


            //polySegmentLeft ??= new PolyLineSegment { IsStroked = true };
            //polySegmentRight ??= new PolyLineSegment { IsStroked = true };
            //polySegmentLeft.Points.Clear();
            //polySegmentRight.Points.Clear();

            // Compute Gaussian parameters
            double A = (CanvasSize - ControlPoint1Y) * 4;
            double mu = ControlPoint2X;
            double sigma = Math.Abs(ControlPoint4X - ControlPoint3X) / 4;
            if (sigma < 1) sigma = 1;

            if (ControlPoint2X > 0)
            {
                xLeft = (A <= CanvasSize)
                    ? ControlPoint2X
                    : mu - Math.Sqrt(-2 * Math.Pow(sigma, 2) * Math.Log(CanvasSize / A));

                // Ensure xLeft stays within canvas bounds
                xLeft = Math.Max(xLeft, 0);
                yLeft = ComputeGaussianY_(xLeft, A, mu, sigma);

                // PathFigure.StartPoint = new Point(0, ComputeGaussianY(0, A, mu, sigma));
                valuesXleft.Add(0);
                valuesYleft.Add(ComputeGaussianY_(0, A, mu, sigma));

                for (double x = 0; x <= ControlPoint2X; x += renderIncrementX)
                {
                    double y = ComputeGaussianY_(x, A, mu, sigma);

                    if (y >= 0 && y <= CanvasSize)
                    {
                        if (!peakX)
                        {
                            // polySegmentLeft.Points.Add(new Point(x, y));
                            valuesXleft.Add(x);
                            valuesYleft.Add(y);
                        }

                    }
                    else
                    {
                        peakX = true;
                    }
                }

                // polySegmentLeft.Points.Add(new Point(xLeft, yLeft));
                valuesXleft.Add(xLeft);
                valuesYleft.Add(yLeft);
            }

            // Calculate Right Path
            if (ControlPoint2X < CanvasSize)
            {
                xRight = (A <= CanvasSize)
                    ? ControlPoint2X
                    : mu + Math.Sqrt(-2 * Math.Pow(sigma, 2) * Math.Log(CanvasSize / A));

                // Ensure xRight stays within canvas bounds
                xRight = Math.Min(xRight, CanvasSize);
                yRight = ComputeGaussianY_(xRight, A, mu, sigma);

                // PathFigure2.StartPoint = new Point(xRight, yRight);
                var valuesXright = new List<double> { xRight };
                var valuesYright = new List<double> { yRight };

                for (double x = ControlPoint2X; x <= CanvasSize; x += renderIncrementX)
                {
                    double y = ComputeGaussianY_(x, A, mu, sigma);

                    if (y >= 0 && y <= CanvasSize)
                    {
                        //polySegmentRight.Points.Add(new Point(x, y));
                        valuesXright.Add(x);
                        valuesYright.Add(y);
                    }
                }
                // polySegmentRight.Points.Add(new Point(MaxWidth, ComputeGaussianY(MaxWidth, A, mu, sigma)));
                valuesXright.Add(CanvasSize);
                valuesYright.Add(ComputeGaussianY_(CanvasSize, A, mu, sigma));
            }


            return [valuesXleft, valuesYleft];
        }

        private double ComputeGaussianY_(double x, double A, double mu, double sigma)
        {
            double exponent = -Math.Pow(x - mu, 2) / (2 * Math.Pow(sigma, 2));
            double normalizedY = A * Math.Exp(exponent);

            return CanvasSize - normalizedY; // Convert to canvas coordinates
        }






        public List<double> GetCurveXValues(int pointsCount, bool isRender = false)
        {
            //return new List<double> { 10, 20, 30 };
            var result = GenerateGaussianCurve()[0];

            return result;
        }

        public List<double> GetCurveYValues(int pointsCount, bool isRender = false)
        {
            //return new List<double> { 10, 20, 30 };
            var result = GenerateGaussianCurve()[1];

            return result;
        }
    }
}
