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
        private const double renderIncrementX = 2.0; // ADD THIS BASE CLASS ?

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
            ControlPoint3X = cp3X;
            ControlPoint3Y = cp3Y;
            ControlPoint4X = cp4X;
            ControlPoint4Y = cp4Y;
        }

        private List<double>[] GenerateGaussianCurve()
        {
            bool peakX = false;
            double xLeft = double.NaN, yLeft = 0;
            double xRight = double.NaN, yRight = 0;


            var valuesXleft = new List<double> ();
            var valuesYleft = new List<double> ();
            var valuesXright = new List<double>();
            var valuesYright = new List<double>();
            var valuesX = new List<double>();
            var valuesY = new List<double>();

            // Compute Gaussian parameters
            //double A = (CanvasSize - ControlPoint1Y) * 4;
            double A = ControlPoint1Y * 4;
            double mu = ControlPoint2X;
            double sigma = Math.Abs(ControlPoint4X - ControlPoint3X) / 4;
            if (sigma < 1) sigma = 1;


            for (double x = 0; x <= ControlPoint2X; x += renderIncrementX)
            {
                double y = CanvasSize - ComputeGaussianY(x, A, mu, sigma);

                if (y <= CanvasSize)
                {
                    valuesX.Add(x);
                    valuesY.Add(y);
                }
                else
                {
                    // Find the x value for which y == CanvasSize
                    double xAtCanvasSize = mu - Math.Sqrt(-2 * Math.Pow(sigma, 2) * Math.Log(CanvasSize / A));
                    xAtCanvasSize = Math.Max(xAtCanvasSize, 0);

                    if (!valuesX.Contains(xAtCanvasSize))
                    {
                        valuesX.Add(xAtCanvasSize);
                        valuesY.Add(CanvasSize);
                    }
                    break;
                }
            }
            for (double x = ControlPoint2X; x <= CanvasSize; x += renderIncrementX)
            {
                double y = CanvasSize - ComputeGaussianY(x, A, mu, sigma);

                if (y <= CanvasSize)
                {
                    valuesX.Add(x);
                    valuesY.Add(y);
                }
                else
                {
                    // Find the x value for which y == CanvasSize
                    double xAtCanvasSize = mu + Math.Sqrt(-2 * Math.Pow(sigma, 2) * Math.Log(CanvasSize / A));
                    xAtCanvasSize = Math.Max(xAtCanvasSize, 0);

                    if (!valuesX.Contains(xAtCanvasSize))
                    {
                        valuesX.Add(xAtCanvasSize);
                        valuesY.Add(CanvasSize);
                    }
                }
            }

            return [valuesX, valuesY];
        }

        private double ComputeGaussianY(double x, double A, double mu, double sigma)
        {
            double exponent = -Math.Pow(x - mu, 2) / (2 * Math.Pow(sigma, 2));
            double normalizedY = A * Math.Exp(exponent);

            return CanvasSize - normalizedY; // Convert to canvas coordinates
        }






        public List<double> GetCurveXValues(int pointsCount, bool isRender = false)
        {
            //return new List<double> { 10, 20, 30 };
            var result = GenerateGaussianCurve()[0];
//            var result = new List<double>
//            {
//120,
//122,
//124,
//126,
//128,
//130,
//132,
//134,
//136,
//138,
//140,
//142,
//144,
//146,
//148,
//150,
//152,
//154,
//156,
//158,
//160,
//162,
//164,
//166,
//168,
//170,
//172,
//174,
//176,
//178,
//180,
//182,
//184,
//186,
//188,
//190,
//192,
//194,
//196,
//198,
//200,
//202,
//204,
//206,
//208,
//210,
//212,
//214,
//216,
//218,
//220,
//222,
//224,
//226,
//228,
//230,
//232,
//234,
//236,
//238,
//240,
//240

//            };

            return result;
        }

        public List<double> GetCurveYValues(int pointsCount, bool isRender = false)
        {
            //return new List<double> { 10, 20, 30 };
            var result = GenerateGaussianCurve()[1];

//            var result = new List<double>
//            {
//48,
//50.64823358516807,
//58.37578196990103,
//70.56059470375766,
//86.25841863997286,
//104.32353065131849,
//123.54611333517438,
//142.7835616035647,
//161.06644022262,
//177.66672626719685,
//192.12437591475913,
//204.23607097004142,
//214.01562561857037,
//221.63818659215258,
//227.38012250562616,
//231.56410874430577,
//234.51542384936633,
//236.531901423402,
//237.86707266465748,
//238.72415783371366,
//239.2577433332212,
//239.5800017053089,
//239.76885888099298,
//239.87627907445082,
//239.9355911754427,
//239.9673875181002,
//239.9839395173319,
//239.99230746290056,
//239.9964164219302,
//239.998376320134,
//239.99928448259095,
//239.9996933264865,
//239.99987215945978,
//239.99994816809274,
//239.99997956088689,
//239.9999921609548,
//239.99999707584388,
//239.9999989391006,
//239.9999996256446,
//239.99999987152154,
//239.99999995711428,
//239.99999998607706,
//239.9999999956037,
//239.99999999864986,
//239.99999999959672,
//239.99999999988285,
//239.9999999999669,
//239.9999999999909,
//239.99999999999756,
//239.99999999999937,
//239.99999999999983,
//239.99999999999997,
//240,
//240,
//240,
//240,
//240,
//240,
//240,
//240,
//240,
//240

//            };

            return result;
        }
    }
}
