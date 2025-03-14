using System;
using System.Collections.Generic;

namespace DSCore.CurveMapper
{
    public class CurveMapperGenerator
    {
        private static int rounding = 10;

        public static List<List<double>> CalculateValues(
            List<double> controlPoints, double canvasSize,
            double minX, double maxX, double minY, double maxY,
            int pointsCount, string graphType
            )
        {
            var xValues = new List<double>() { double.NaN };
            var yValues = new List<double>() { double.NaN };

            if (minX != maxX && minY != maxY && pointsCount >= 2)
            {
                // Unpack the control points
                double cp1x = GetCP(controlPoints, 0), cp1y = GetCP(controlPoints, 1);
                double cp2x = GetCP(controlPoints, 2), cp2y = GetCP(controlPoints, 3);
                double cp3x = GetCP(controlPoints, 4), cp3y = GetCP(controlPoints, 5);
                double cp4x = GetCP(controlPoints, 6), cp4y = GetCP(controlPoints, 7);

                object curve = null;

                switch (graphType)
                {
                    case "LinearCurve":
                        curve = new LinearCurve(cp1x, cp1y, cp2x, cp2y, canvasSize);
                        break;
                    case "BezierCurve":
                        curve = new BezierCurve(cp1x, cp1y, cp2x, cp2y, cp3x, cp3y, cp4x, cp4y, canvasSize);
                        break;
                    case "SineWave":
                        curve = new SineWave(cp1x, cp1y, cp2x, cp2y, canvasSize);
                        break;
                    case "CosineWave":
                        curve = new SineWave(cp1x, cp1y, cp2x, cp2y, canvasSize);
                        break;
                    case "ParabolicCurve":
                        curve = new ParabolicCurve(cp1x, cp1y, cp2x, cp2y, canvasSize);
                        break;
                    case "PerlinNoiseCurve":
                        curve = new PerlinNoiseCurve(cp1x, cp1y, cp2x, cp2y, cp3x, cp3y, canvasSize);
                        break;
                    case "PowerCurve":
                        curve = new PowerCurve(cp1x, cp1y, canvasSize);
                        break;
                    case "SquareRootCurve":
                        curve = new SquareRootCurve(cp1x, cp1y, cp2x, cp2y, canvasSize);
                        break;
                    case "GaussianCurve":
                        curve = new GaussianCurve(cp1x, cp1y, cp2x, cp2y, cp3x, cp3y, cp4x, cp4y, canvasSize);
                        break;
                }

                if (curve != null)
                {
                    dynamic dynamicCurve = curve;

                    xValues = MapValues(dynamicCurve.GetCurveXValues(pointsCount), minX, maxX, canvasSize);
                    yValues = MapValues(dynamicCurve.GetCurveYValues(pointsCount), minY, maxY, canvasSize);
                }
            }

            return new List<List<double>> { yValues, xValues };
        }

        private static double GetCP(List<double> controlPoints, int index)
        {
            return index < controlPoints.Count ? controlPoints[index] : 0;
        }

        private static List<double> MapValues(List<double> rawValues, double minLimit, double maxLimit, double canvasSize)
        {
            var mappedValues = new List<double>();

            foreach (var value in rawValues)
            {
                mappedValues.Add(Math.Round(minLimit + value / canvasSize * (maxLimit - minLimit), rounding));
            }
            return mappedValues;
        }
    }
}
