using System;
using System.Collections.Generic;
using System.Linq;

namespace DSCore.CurveMapper
{
    /// <summary>
    /// Represents a Perlin noise curve in the CurveMapper.
    /// The curve generates procedural noise based on control points and Perlin noise functions.
    /// </summary>
    public class PerlinNoiseCurve : CurveBase
    {
        private double ControlPoint1X;
        private double ControlPoint1Y;
        private double ControlPoint2X;
        private double ControlPoint2Y;
        private double ControlPoint3X;
        private double ControlPoint3Y;

        private readonly List<double> randomValues;
        private readonly Random rand;

        private const int PerlinVertices = 2000;
        private const int Seed = 1;
        private readonly int perlinVerticesMask;
        private readonly double initialWidth = 240;
        private double amplitude = 1.0;
        private double scale = 1.0;

        private int numberOfOctaves;       // Number of noise layers (octaves)
        private double baseFrequency;       // Initial frequency of the noise
        private double persistenceFactor;   // Controls amplitude reduction per octave
        private int randomSeed;             // Seed for random noise generation


        public PerlinNoiseCurve(double cp1X, double cp1Y, double cp2X, double cp2Y, double cp3X, double cp3Y, double canvasSize)
            : base(canvasSize)
        {
            //CanvasSize = canvasSize;
            ControlPoint1X = cp1X;
            ControlPoint1Y = cp1Y;
            ControlPoint2X = cp2X;
            ControlPoint2Y = cp2Y;
            ControlPoint3X = cp3X;
            ControlPoint3Y = cp3Y;

            perlinVerticesMask = PerlinVertices - 1;
            rand = new Random(Seed);
            randomValues = new List<double>();

            ConfigureNoiseParameters(4.0, 0.1, 1.0, 1, Seed);

            for (int i = 0; i < PerlinVertices; i++)
            {
                randomValues.Add(rand.NextDouble() - 0.5);
            }
        }

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

        private double ComputePerlinCurveY(double x)
        {
            // Calculate the raw point using the adjusted Y
            double y = GetHeight(-ControlPoint3X + x, 0.0) + (CanvasSize - ControlPoint3Y);

            // Flipped value to align correctly with WPF's coordinate system
            return CanvasSize - y;
        }

        /// <summary>
        /// Returns X and Y values distributed across the curve.
        /// </summary>
        protected override (List<double> XValues, List<double> YValues) GenerateCurve(List<double> pointsDomain, bool isRender = false)
        {
            var valuesX = new List<double>();
            var valuesY = new List<double>();

            double scalingFactor = CanvasSize / initialWidth;
            scale = (CanvasSize - ControlPoint1X) / (CanvasSize * 3.0);
            amplitude = (CanvasSize - ControlPoint2Y) * 1.1;
            baseFrequency = scale / scalingFactor;

            if (isRender)
            {
                for (double x = 0.0; x <= CanvasSize; x += renderIncrementX)
                {
                    var y = ComputePerlinCurveY(x);

                    if (y >= 0 && y <= CanvasSize)
                    {
                        valuesX.Add(x);
                        valuesY.Add(y);
                    }
                }

                // Add the intersection points & sort the values
                var intersectionValuesX = GetBoundaryIntersections();
                valuesX.AddRange(intersectionValuesX.XValues);
                valuesY.AddRange(intersectionValuesX.YValues);

                var sortedPairs = valuesX.Zip(valuesY, (x, y) => new { X = x, Y = y })
                    .OrderBy(pair => pair.X)
                    .ToList();
                valuesX = sortedPairs.Select(p => p.X).ToList();
                valuesY = sortedPairs.Select(p => p.Y).ToList();
            }
            else if (pointsDomain.Count == 1)
            {
                var pointsCount = pointsDomain[0];

                var step = CanvasSize / (pointsCount - 1);

                for (int i = 0; i < pointsCount; i++)
                {
                    double x = 0 + step * i;

                    valuesX.Add(x);
                    valuesY.Add(ComputePerlinCurveY(x));
                }
            }
            else
            {
                return GenerateFromDomain(pointsDomain, x => ComputePerlinCurveY(x));
            }

            return (valuesX, valuesY);
        }

        private (List<double> XValues, List<double> YValues) GetBoundaryIntersections()
        {
            var intersectionXPoints = new List<double>();
            var intersectionYPoints = new List<double>();

            double previousY = ComputePerlinCurveY(0);
            double previousX = 0;
            double step = renderIncrementX;

            for (double currentX = step; currentX <= CanvasSize; currentX += step)
            {
                double currentY = ComputePerlinCurveY(currentX);

                // Check for intersection at Y = 0
                if ((previousY < 0 && currentY >= 0) || (previousY > 0 && currentY <= 0))
                {
                    double intersectX = previousX + ((0 - previousY) * (currentX - previousX) / (currentY - previousY));
                    intersectionXPoints.Add(intersectX);
                    intersectionYPoints.Add(0);
                }

                // Check for intersection at Y = CanvasSize
                if ((previousY < CanvasSize && currentY >= CanvasSize) || (previousY > CanvasSize && currentY <= CanvasSize))
                {
                    double intersectX = previousX + ((CanvasSize - previousY) * (currentX - previousX) / (currentY - previousY));
                    intersectionXPoints.Add(intersectX);
                    intersectionYPoints.Add(CanvasSize);
                }

                previousX = currentX;
                previousY = currentY;
            }

            return (intersectionXPoints, intersectionYPoints);
        }
    }
}
