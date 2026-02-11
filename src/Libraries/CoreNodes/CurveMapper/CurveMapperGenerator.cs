using Autodesk.DesignScript.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DSCore.CurveMapper
{
    [IsVisibleInDynamoLibrary(false)]
    public class CurveMapperGenerator
    {
        private static int rounding = 10;
        private static List<List<double>> cachedValues = null;

        private static List<List<double>> CalculateValues(
            List<double> controlPoints,
            double canvasSize,
            [ArbitraryDimensionArrayImport] object minX,
            [ArbitraryDimensionArrayImport] object maxX,
            [ArbitraryDimensionArrayImport] object minY,
            [ArbitraryDimensionArrayImport] object maxY,
            [ArbitraryDimensionArrayImport] object pointsCount,
            string graphType
            )
        {
            // Unpack the control points
            double cp1x = GetCP(controlPoints, 0), cp1y = GetCP(controlPoints, 1);
            double cp2x = GetCP(controlPoints, 2), cp2y = GetCP(controlPoints, 3);
            double cp3x = GetCP(controlPoints, 4), cp3y = GetCP(controlPoints, 5);
            double cp4x = GetCP(controlPoints, 6), cp4y = GetCP(controlPoints, 7);

            //      Validation
            var errors = new List<string>();

            // Check if min/max are equal (invalid range)
            bool isMinMaxEqual = false;
            try
            {
                double minXVal = Convert.ToDouble(minX);
                double maxXVal = Convert.ToDouble(maxX);
                isMinMaxEqual = Math.Abs(minXVal - maxXVal) < 1e-9;
            }
            catch { }

            try
            {
                if (!isMinMaxEqual)
                {
                    double minYVal = Convert.ToDouble(minY);
                    double maxYVal = Convert.ToDouble(maxY);
                    isMinMaxEqual = Math.Abs(minYVal - maxYVal) < 1e-9;
                }
            }
            catch { }

            if (isMinMaxEqual)
                errors.Add(Properties.Resources.CurveMapperEqualMinMaxWarning);

            // Parse and validate pointsCount
            var countValue = new List<double>();
            bool isCountInvalid = false;            

            try
            {
                if (pointsCount is IEnumerable enumerable && !(pointsCount is string))
                {
                    foreach (var item in enumerable)
                    {
                        if (double.TryParse(item?.ToString(), out var val))
                            countValue.Add(val);
                    }
                }
                else if (double.TryParse(pointsCount?.ToString(), out var singleVal))
                {
                    countValue.Add(singleVal);
                }

                if (countValue.Count == 0 || (countValue.Count == 1 && countValue[0] < 2))
                    isCountInvalid = true;
            }
            catch
            {
                isCountInvalid = true;
            }

            if (isCountInvalid)
                errors.Add(Properties.Resources.CurveMapperInvalidCountWarning);

            // Ensure X/Y limits are scalar (not lists)
            bool isXYFormatInvalid = minX is IEnumerable ||
                maxX is IEnumerable ||
                minY is IEnumerable ||
                maxY is IEnumerable;

            if (isXYFormatInvalid)
                errors.Add(Properties.Resources.CurveMapperInvalidXYFormatWarning);

            // Validate control point values based on curve type
            bool isCurveInvalid = graphType switch
            {
                "LinearCurve" => cp1x == cp2x,
                "SineWave" => cp1x == cp2x,
                "CosineWave" => cp1x == cp2x,
                "ParabolicCurve" => cp1x == cp2x,
                "PowerCurve" => cp1x <= 0 || cp1y <= 0 || cp1x >= canvasSize || cp1y >= canvasSize,
                _ => false
            };

            if (isCurveInvalid)
                errors.Add(Properties.Resources.CurveMapperInvalidCurveWarning);

            //      Early exit if validation fails
            if (errors.Count > 0)
            {
                cachedValues = new List<List<double>> { null, null };
                throw new Exception(string.Join("\n", errors));
            }

            //      Curve Calculation if all check pass
            object curve = graphType switch
            {
                "LinearCurve" => new LinearCurve(cp1x, cp1y, cp2x, cp2y, canvasSize),
                "BezierCurve" => new BezierCurve(cp1x, cp1y, cp2x, cp2y, cp3x, cp3y, cp4x, cp4y, canvasSize),
                "SineWave" => new SineWave(cp1x, cp1y, cp2x, cp2y, canvasSize),
                "CosineWave" => new SineWave(cp1x, cp1y, cp2x, cp2y, canvasSize),
                "ParabolicCurve" => new ParabolicCurve(cp1x, cp1y, cp2x, cp2y, canvasSize),
                "PerlinNoiseCurve" => new PerlinNoiseCurve(cp1x, cp1y, cp2x, cp2y, cp3x, cp3y, canvasSize),
                "PowerCurve" => new PowerCurve(cp1x, cp1y, canvasSize),
                "SquareRootCurve" => new SquareRootCurve(cp1x, cp1y, cp2x, cp2y, canvasSize),
                "GaussianCurve" => new GaussianCurve(cp1x, cp1y, cp2x, cp2y, cp3x, cp3y, cp4x, cp4y, canvasSize),
                _ => null
            };

            var xValues = new List<double>();
            var yValues = new List<double>();

            if (curve is CurveBase dynamicCurve)
            {
                xValues = MapValues(dynamicCurve.GetCurveXValues(countValue), Convert.ToDouble(minX), Convert.ToDouble(maxX), canvasSize);
                yValues = MapValues(dynamicCurve.GetCurveYValues(countValue), Convert.ToDouble(minY), Convert.ToDouble(maxY), canvasSize);
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

        [Obsolete("Use CalculateValuesForX with object pointsCount instead.")]
        public static List<double> CalculateValuesX(
            List<double> controlPoints,
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

        [Obsolete("Use CalculateValuesForY with object pointsCount instead.")]
        public static List<double> CalculateValuesY(
            List<double> controlPoints,
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

        public static List<double> CalculateValuesForX(
            List<double> controlPoints,
            double canvasSize,
            [ArbitraryDimensionArrayImport] object minX,
            [ArbitraryDimensionArrayImport] object maxX,
            [ArbitraryDimensionArrayImport] object minY,
            [ArbitraryDimensionArrayImport] object maxY,
            [ArbitraryDimensionArrayImport] object pointsCount,
            string graphType
            )
        {
            // X values must always be calculated first to initialize the cache.
            // CalculateValuesY() depends on this to avoid redundant calculation.
            cachedValues = CalculateValues(controlPoints, canvasSize, minX, maxX, minY, maxY, pointsCount, graphType);
            return cachedValues?[0];
        }

        public static List<double> CalculateValuesForY(
            List<double> controlPoints,
            double canvasSize,
            [ArbitraryDimensionArrayImport] object minX,
            [ArbitraryDimensionArrayImport] object maxX,
            [ArbitraryDimensionArrayImport] object minY,
            [ArbitraryDimensionArrayImport] object maxY,
            [ArbitraryDimensionArrayImport] object pointsCount,
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
