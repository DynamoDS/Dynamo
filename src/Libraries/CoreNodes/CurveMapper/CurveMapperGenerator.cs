using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Runtime;

namespace DSCore.CurveMapper
{
    [IsVisibleInDynamoLibrary(false)]
    public class CurveMapperGenerator
    {
        private static int rounding = 10;
        private static List<List<double>> cachedValues = null;

        [IsVisibleInDynamoLibrary(false)]
        private static List<List<double>> CalculateValues(
            List<double>
            controlPoints,
            double canvasSize,
            [ArbitraryDimensionArrayImport] object minX,
            [ArbitraryDimensionArrayImport] object maxX,
            [ArbitraryDimensionArrayImport] object minY,
            [ArbitraryDimensionArrayImport] object maxY,
            List<double> pointsCount,
            string graphType
            )
        {
            var xValues = new List<double>() { double.NaN };
            var yValues = new List<double>() { double.NaN };

            // Block input replication and force user to supply scalar values
            if (
                minX is IEnumerable ||
                maxX is IEnumerable ||
                minY is IEnumerable ||
                maxY is IEnumerable)
            {
                // Ensure nulls so node returns [null, null]
                cachedValues[0] = null;
                cachedValues[1] = null;

                throw new ArgumentException("Expects argument type(s): double");
            }

            // Safety checks
            // TODO: There are similar checks in CurveMapperNodeModel.
            // Review and see if they can be removed.
            if (pointsCount == null || pointsCount.Count == 0 || (pointsCount.Count == 1 && pointsCount[0] < 2))
                return new List<List<double>> { yValues, xValues };

            if (minX == maxX || minY == maxX)
                return new List<List<double>> { yValues, xValues };
            
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

            if (curve is CurveBase dynamicCurve)
            {
                xValues = MapValues(dynamicCurve.GetCurveXValues(pointsCount), Convert.ToDouble(minX), Convert.ToDouble(maxX), canvasSize);
                yValues = MapValues(dynamicCurve.GetCurveYValues(pointsCount), Convert.ToDouble(minY), Convert.ToDouble(maxY), canvasSize);
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

        public static List<double> CalculateValuesX(
            List<double>
            controlPoints,
            double canvasSize,
            [ArbitraryDimensionArrayImport] object minX,
            [ArbitraryDimensionArrayImport] object maxX,
            [ArbitraryDimensionArrayImport] object minY,
            [ArbitraryDimensionArrayImport] object maxY,
            List<double> pointsCount,
            string graphType
            )
        {
            // X values must always be calculated first to initialize the cache.
            // CalculateValuesY() depends on this to avoid redundant calculation.
            cachedValues = CalculateValues(controlPoints, canvasSize, minX, maxX, minY, maxY, pointsCount, graphType);
            return cachedValues?[0];
        }

        public static List<double> CalculateValuesY(
            List<double>
            controlPoints,
            double canvasSize,
            [ArbitraryDimensionArrayImport] object minX,
            [ArbitraryDimensionArrayImport] object maxX,
            [ArbitraryDimensionArrayImport] object minY,
            [ArbitraryDimensionArrayImport] object maxY,
            List<double> pointsCount,
            string graphType
            )
        {
            if (cachedValues == null)
            {
                cachedValues = CalculateValues(controlPoints, canvasSize, minX, maxX, minY, maxY, pointsCount, graphType);
            }
            return cachedValues?[1];
        }
    }
}
