using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Dynamo.Wpf.Controls.SubControls
{
    public class PerlinCurve : CurveBase
    {
        private CurveMapperControlPoint orthoPoint1;
        private CurveMapperControlPoint orthoPoint2;
        private CurveMapperControlPoint controlPoint;
        private PolyLineSegment polySegment;
        private readonly List<double> randomValues;
        private readonly Random rand;

        private double maxWidth;
        private double maxHeight;

        private const int PerlinVertices = 2000;
        private readonly int perlinVerticesMask;
        private double amplitude = 1.0;
        private double scale = 1.0;

        public double MaxWidth
        {
            get => maxWidth;
            set
            {
                if (maxWidth != value)
                {
                    maxWidth = value;
                    Regenerate(orthoPoint1); // Ensure the curve regenerates if needed
                    Regenerate(orthoPoint2);
                    Regenerate(controlPoint);
                }
            }
        }
        public double MaxHeight
        {
            get => maxHeight;
            set
            {
                if (maxHeight != value)
                {
                    maxHeight = value;
                    Regenerate(orthoPoint1); // Ensure the curve regenerates if needed
                    Regenerate(orthoPoint2);
                    Regenerate(controlPoint);
                }
            }
        }

        /// <summary>
        /// Initializes a perlin noise curve with control points, dimensions, and visual properties.
        /// </summary>
        public PerlinCurve(CurveMapperControlPoint oPoint1, CurveMapperControlPoint oPoint2, CurveMapperControlPoint controlPoint, int seed, double maxWidth, double maxHeight)
        {
            this.orthoPoint1 = oPoint1;
            this.orthoPoint2 = oPoint2;
            this.controlPoint = controlPoint;
            this.maxWidth = maxWidth;
            this.maxHeight = maxHeight;

            perlinVerticesMask = PerlinVertices - 1;
            rand = new Random(seed);
            randomValues = new List<double>();

            ConfigureNoiseParameters(4.0, 0.1, 1.0, 1, seed);            

            for (int i = 0; i < PerlinVertices; i++)
            {
                randomValues.Add(rand.NextDouble() - 0.5);
            }

            PathFigure = new PathFigure();

            GeneratePerlinCurve();

            PathFigure.Segments.Add(polySegment);

            PathGeometry = new PathGeometry
            {
                Figures = { PathFigure}
            };

            PathCurve = new Path
            {
                Data = PathGeometry,
                Stroke = new SolidColorBrush(Color.FromRgb(0xB3, 0x85, 0xF2)), // Purple color
                StrokeThickness = 3,
                StrokeLineJoin = PenLineJoin.Round
            };
        }

        private Point PerlinAddPointsToCurve(double x)
        {
            // Calculate the raw point
            double rawY = GetHeight(-controlPoint.Point.X + x, 0.0) + controlPoint.Point.Y;

            // Clamp the values to ensure they stay within the canvas
            double clampedX = Math.Max(0, Math.Min(x, maxWidth));
            double clampedY = Math.Max(0, Math.Min(rawY, maxHeight));

            return new Point(clampedX, clampedY);
        }

        private void GeneratePerlinCurve()
        {
            if (polySegment == null)
            {
                polySegment = new PolyLineSegment();
                polySegment.IsStroked = true;
            }
            else
            {
                polySegment.Points.Clear();
            }

            scale = (maxWidth - orthoPoint1.Point.X) / (maxWidth * 3.0);
            amplitude = orthoPoint2.Point.Y * 1.1;
            baseFrequency = scale;

            if (PathFigure != null)
            {
                PathFigure.StartPoint = PerlinAddPointsToCurve(0.0);
            }

            if (polySegment != null)
            {
                for (double x = 1.0; x < maxWidth; x += 1.0)
                {
                    polySegment.Points.Add(PerlinAddPointsToCurve(x));
                }

                polySegment.Points.Add(PerlinAddPointsToCurve(maxWidth));
            }
        }

        /// <summary>
        /// Regenerates the curve when control points are updated.
        /// </summary>
        public void Regenerate(CurveMapperControlPoint updatedControlPoint)
        {
            if (updatedControlPoint == null)
                return;

            if (updatedControlPoint.IsOrthogonal)
            {
                if (updatedControlPoint == orthoPoint1 || updatedControlPoint == orthoPoint2)
                {
                    orthoPoint1 = (updatedControlPoint == orthoPoint1) ? updatedControlPoint : orthoPoint1;
                    orthoPoint2 = (updatedControlPoint == orthoPoint2) ? updatedControlPoint : orthoPoint2;
                }
            }
            else if (updatedControlPoint == controlPoint)
            {
                controlPoint = updatedControlPoint;
            }

            GeneratePerlinCurve();
        }

        public override List<double> GetValuesFromAssignedParameters(double lowLimit, double highLimit, int count)
        {
            if (count < 1)
                return null;

            List<double> values = new List<double>();

            int steps = count - 1;
            for (double d = 0.0; d < maxWidth; d += (maxWidth / steps))
            {
                double rawHeight = GetHeight(-controlPoint.Point.X + d, 0.0);
                double adjustedHeight = -rawHeight + (maxHeight - controlPoint.Point.Y);
                double normalizedValue = (highLimit - lowLimit) * adjustedHeight / maxHeight;
                normalizedValue += lowLimit;
                values.Add(normalizedValue);
            }

            if (values.Count < count)
            {
                double rawHeight = GetHeight(-controlPoint.Point.X + maxWidth, 0.0);
                double adjustedHeight = -rawHeight + (maxHeight - controlPoint.Point.Y);
                double normalizedValue = (highLimit - lowLimit) * adjustedHeight / maxHeight;
                normalizedValue += lowLimit;
                values.Add(normalizedValue);
            }

            return values;
        }

        // Sourced from StackOverflow
        private int numberOfOctaves;       // Number of noise layers (octaves)
        private double baseFrequency;       // Initial frequency of the noise
        private double persistenceFactor;   // Controls amplitude reduction per octave
        private int randomSeed;             // Seed for random noise generation

        private void ConfigureNoiseParameters(double persistenceFactor, double baseFrequency, double baseAmplitude, int numberOfOctaves, int seed)
        {
            this.persistenceFactor = persistenceFactor;
            randomSeed = seed;
            this.numberOfOctaves = numberOfOctaves;
            amplitude = baseAmplitude;
            this.baseFrequency = baseFrequency;
        }

        private double GetHeight(double x, double y)
        {
            return amplitude * ComputePerlinNoiseSum(x, y);
        }

        private double ComputePerlinNoiseSum(double i, double j)
        {
            double totalNoise = 0.0;
            double currentAmplitude = 1;
            double currentFrequency = baseFrequency;

            for (int k = 0; k < numberOfOctaves; k++)
            {
                totalNoise += GetValue(j * currentFrequency + randomSeed + (PerlinVertices / 2), i * currentFrequency + randomSeed + (PerlinVertices / 2)) * currentAmplitude;
                currentAmplitude *= persistenceFactor;
                currentFrequency *= 2;
            }

            return totalNoise;
        }

        private double GetValue(double x, double y)
        {
            int xInt = (int)x;
            int yInt = (int)y;
            double xFrac = x - xInt;
            double yFrac = y - yInt;

            //noise values
            double n01 = Noise(xInt - 1, yInt - 1);
            double n02 = Noise(xInt + 1, yInt - 1);
            double n03 = Noise(xInt - 1, yInt + 1);
            double n04 = Noise(xInt + 1, yInt + 1);
            double n05 = Noise(xInt - 1, yInt);
            double n06 = Noise(xInt + 1, yInt);
            double n07 = Noise(xInt, yInt - 1);
            double n08 = Noise(xInt, yInt + 1);
            double n09 = Noise(xInt, yInt);

            double n12 = Noise(xInt + 2, yInt - 1);
            double n14 = Noise(xInt + 2, yInt + 1);
            double n16 = Noise(xInt + 2, yInt);

            double n23 = Noise(xInt - 1, yInt + 2);
            double n24 = Noise(xInt + 1, yInt + 2);
            double n28 = Noise(xInt, yInt + 2);

            double n34 = Noise(xInt + 2, yInt + 2);

            //find the noise values of the four corners
            double x0y0 = 0.0625 * (n01 + n02 + n03 + n04) + 0.125 * (n05 + n06 + n07 + n08) + 0.25 * (n09);
            double x1y0 = 0.0625 * (n07 + n12 + n08 + n14) + 0.125 * (n09 + n16 + n02 + n04) + 0.25 * (n06);
            double x0y1 = 0.0625 * (n05 + n06 + n23 + n24) + 0.125 * (n03 + n04 + n09 + n28) + 0.25 * (n08);
            double x1y1 = 0.0625 * (n09 + n16 + n28 + n34) + 0.125 * (n08 + n14 + n06 + n24) + 0.25 * (n04);

            //interpolate between those values according to the x and y fractions
            double v1 = Interpolate(x0y0, x1y0, xFrac); //interpolate in x direction (y)
            double v2 = Interpolate(x0y1, x1y1, xFrac); //interpolate in x direction (y+1)
            double fin = Interpolate(v1, v2, yFrac);  //interpolate in y direction

            return fin;
        }

        private double Interpolate(double startValue, double endValue, double interpolationFactor)
        {
            double inverseFactor = 1.0 - interpolationFactor;
            double inverseFactorSquared = inverseFactor * inverseFactor;
            double weightStart = 3.0 * inverseFactorSquared - 2.0 * inverseFactorSquared * inverseFactor;

            double factorSquared = interpolationFactor * interpolationFactor;
            double weightEnd = 3.0 * factorSquared - 2.0 * (factorSquared * interpolationFactor);

            return startValue * weightStart + endValue * weightEnd;
        }

        private double Noise(int x, int y)
        {
            int n = x + y * 57;
            n = (n << 13) ^ n;
            int t = (n * (n * n * 15731 + 789221) + 1376312589) & 0x7fffffff;
            return 1.0 - (double)t * 0.93132257461547858515625e-9;
        }
    }
}
