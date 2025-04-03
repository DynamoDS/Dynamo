using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Runtime;

namespace DSCore.CurveMapper
{
    public class CurveMapperGenerator
    {
        private static int rounding = 10;
        private static List<List<double>> cachedValues = null;

        [IsVisibleInDynamoLibrary(false)]
        public static List<List<double>> CalculateValues(
            List<double>
            controlPoints,
            double canvasSize,
            [ArbitraryDimensionArrayImport] object minX,
            [ArbitraryDimensionArrayImport] object maxX,
            [ArbitraryDimensionArrayImport] object minY,
            [ArbitraryDimensionArrayImport] object maxY,
            object pointsCount,
            string graphType
            )
        {
            if (
                minX is IEnumerable ||
                maxX is IEnumerable ||
                minY is IEnumerable ||
                maxY is IEnumerable)
            {
                throw new ArgumentException("LIST INPUT IS UNSUPPORTED");
            }

            var xValues = new List<double>() { double.NaN };
            var yValues = new List<double>() { double.NaN };

            var pointsCountAsList = new List<double>();

            if (pointsCount is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (item is double d)
                        pointsCountAsList.Add(d);
                    else if (item is int i)
                        pointsCountAsList.Add(Convert.ToDouble(i));
                    else if (item is long l)
                        pointsCountAsList.Add(Convert.ToDouble(l));
                }
            }
            else if (pointsCount is double d)
            {
                pointsCountAsList.Add(d);
            }
            else if (pointsCount is int i)
            {
                pointsCountAsList.Add(Convert.ToDouble(i));
            }
            else if (pointsCount is long l)
            {
                pointsCountAsList.Add(Convert.ToDouble(l));
            }


            var e1 = pointsCountAsList; // RAISE WARNING IF IT'S A SINGLE DOUBLE VALUE


            if (minX != maxX && minY != maxY) // pointsCount >= 2
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

                    //int pointsCountInt;
                    //try
                    //{
                    //    pointsCountInt = checked((int)(long)pointsCount);
                    //}
                    //catch (OverflowException)
                    //{
                    //    throw new ArgumentOutOfRangeException(nameof(pointsCount), "pointsCount is out of range for an int.");
                    //}

                    // THROW THE WARNING IF COUNT IS SINGLE DOUBLE VALUE ?
                    // CLEAR THE WARNING WHEN THE VALUE IN THE CODE BLOCK HAS CHANGED (ON NODE COLLECTION CHANGED?)
                    // CHECK WHY THE OTHER WARNING ALSO APPEARS

                    var minXConverted = Convert.ToDouble(minX);
                    var maxXConverted = Convert.ToDouble(maxX);
                    var minYConverted = Convert.ToDouble(minY);
                    var maxYConverted = Convert.ToDouble(maxY);

                    var pointsCountConverted = Convert.ToDouble(pointsCount);

                    var d1 = dynamicCurve.GetCurveXValues(pointsCountAsList);
                    var d2 = dynamicCurve.GetCurveYValues(pointsCountAsList);

                    xValues = MapValues(dynamicCurve.GetCurveXValues(pointsCountAsList), minXConverted, maxXConverted, canvasSize);
                    yValues = MapValues(dynamicCurve.GetCurveYValues(pointsCountAsList), minYConverted, maxYConverted, canvasSize);
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

        //SafeCalculateValues
        public static List<double> CalculateValuesX(
            List<double>
            controlPoints,
            double canvasSize,
            [ArbitraryDimensionArrayImport] object minX,
            [ArbitraryDimensionArrayImport] object maxX,
            [ArbitraryDimensionArrayImport] object minY,
            [ArbitraryDimensionArrayImport] object maxY,
            object pointsCount,
            string graphType
            )
        {
            // Run safe version to prevent list replication
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
            object pointsCount,
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
