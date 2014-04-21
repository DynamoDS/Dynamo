using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;

namespace Revit.GeometryConversion
{
    internal static class HermiteToNurbs
    {
        /// <summary>
        /// Convert a Revit HermiteSpline exactly to a NurbsCurve equivalent
        /// </summary>
        /// <param name="crv"></param>
        /// <returns></returns>
        public static NurbsCurve ConvertExact(Autodesk.Revit.DB.HermiteSpline crv)
        {
            var knots = GetNurbsKnots(crv);
            var points = GetNurbsPoints(crv, knots);

            // the resultant nurbs curve is not rational - i.e. it's weights are 1
            var weights = Enumerable.Repeat(1.0, points.Length).ToArray();

            return NurbsCurve.ByControlPointsWeightsKnots(points, weights, knots, 3);
        }

        /// <summary>
        /// Build a NURBS knot structure from a HermiteSpline
        /// </summary>
        /// <param name="curve"></param>
        /// <returns></returns>
        private static double[] GetNurbsKnots(Autodesk.Revit.DB.HermiteSpline curve)
        {
            var parms = curve.Parameters.Cast<double>().ToList();
            var knots = new List<double>();

            for (int ii = 0; ii < parms.Count; ii++)
            {
                if (ii == 0 || ii == parms.Count - 1)
                {
                    for (int jj = 4; jj-- > 0; )
                        knots.Add(parms[ii]);
                }
                else
                {
                    knots.Add(parms[ii]);
                }
            }

            return knots.ToArray();
        }

        /// <summary>
        /// Obtain the Nurbs control points from a Hermite Spline
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="nurbsKnots"></param>
        /// <returns></returns>
        private static Autodesk.DesignScript.Geometry.Point[] GetNurbsPoints(Autodesk.Revit.DB.HermiteSpline curve, double[] nurbsKnots)
        {

            int numKnots = nurbsKnots.Length;
            int numPoints = numKnots - _cubicOrder;
            var pPoints = new XYZ[numPoints];

            for (int ii = 0; ii < numPoints; ii++)
            {
                double t = 0.5 * (nurbsKnots[ii] + nurbsKnots[ii + 1]);

                var del = new double[_cubicOrder];
                for (int jj = 1; jj <= _cubicDegree; jj++)
                {
                    del[jj] = nurbsKnots[ii + jj] - t;
                }

                var s = Symmetric(del);

                var psi = new double[_cubicOrder];
                for (int k = 0; k <= _cubicDegree; k++)
                {
                    double top = _power1[k] * _factorial[k] * s[_cubicDegree - k];
                    psi[k] = top / _factorial[_cubicDegree];
                }

                pPoints[ii] = XYZ.Zero;

                var derivTransform = curve.ComputeDerivatives(t, false);

                // this is the expected rep
                var derivs = new[]
                {
                    derivTransform.Origin,  
                    derivTransform.BasisX, // 1st deriv
                    derivTransform.BasisY, // 2nd deriv
                    null
                };

                // calculate 3-th derivative
                int low = 0;
                int high = numPoints + 1;
                int mid = (low + high) / 2;
                while (t < nurbsKnots[mid] || t >= nurbsKnots[mid + 1])
                {
                    if (t < nurbsKnots[mid])
                        high = mid;
                    else
                        low = mid;
                    mid = (low + high) / 2;
                }

                // find the span containing t
                double p1 = nurbsKnots[mid];
                double p2 = nurbsKnots[mid + 1];

                // evaluate the point and tangent at the two end points of the span
                var P1R1 = curve.ComputeDerivatives(p1, false);
                var P1 = P1R1.Origin;
                var R1 = P1R1.BasisX;

                var P4R4 = curve.ComputeDerivatives(p2, false);
                var P4 = P4R4.Origin;
                var R4 = P4R4.BasisX;

                // the chord vector from p1 to p2
                var P14 = P1 - P4;
                var R14 = R1 + R4;

                // the span of the region is tk
                double tk = p2 - p1;
                if (tk < 1e-13)
                    tk = 1.0;  

                // an approximation of the third derivative
                derivs[3] = P14 * (12.0 / (tk * tk * tk)) + R14 * (6.0 / (tk * tk));

                for (int r = 0; r <= _cubicDegree; r++)
                {
                    int codegree = _cubicDegree - r;
                    double mul = _power1[codegree] * psi[codegree];

                    pPoints[ii] = pPoints[ii] + mul * derivs[r];
                }
            }

            return pPoints.Select(x => x.ToPoint()).ToArray();
        }

        #region Helper methods

        private static double[] Symmetric(double[] pX)
        {
            const int degree = 3;
            var stemp = new double[degree + 2, degree + 2];

            for (int ii = 0; ii <= degree; ii++)
                stemp[ii, 0] = 1.0;

            for (int ii = 0; ii <= degree; ii++)
                stemp[ii, ii + 1] = 0.0;

            for (int ii = 1; ii <= degree; ii++)
            {
                for (int jj = 1; jj <= ii; jj++)
                {
                    stemp[ii, jj] = pX[ii] * stemp[ii - 1, jj - 1] + stemp[ii - 1, jj];
                }
            }

            var pS = new double[_cubicOrder];
            for (int jj = 0; jj <= degree; jj++)
                pS[jj] = stemp[degree, jj];

            return pS;
        }

        private static int _cubicDegree = 3;
        private static int _cubicOrder = _cubicDegree + 1;
        private static double[] _factorial = { 1, 1, 2, 6 };
        private static double[] _power1 = { 1, -1, 1, -1 };

        #endregion

    }
}
