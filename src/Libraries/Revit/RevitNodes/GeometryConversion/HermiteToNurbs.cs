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
            var knots = Clamp(crv.Parameters.Cast<double>());
            var points = GetNurbsPoints(crv, knots);

            // the resultant nurbs curve is not rational - i.e. it's weights are 1
            var weights = Enumerable.Repeat(1.0, points.Length).ToArray();

            return NurbsCurve.ByControlPointsWeightsKnots(points, weights, knots, 3);
        }
        
        /// <summary>
        /// Clamp a collection of curve parameters by introducing knot multiplicities at each end such
        /// that each end of the knot vector has degree + 1 copies of the knot
        /// </summary>
        /// <param name="curveParameters"></param>
        /// <param name="degree"></param>
        /// <returns></returns>
        internal static double[] Clamp(IEnumerable<double> curveParameters, int degree = 3)
        {
            var parms = curveParameters.ToList();
            return
                Enumerable.Repeat(parms.First(), degree)
                    .Concat(parms)
                    .Concat(Enumerable.Repeat(parms.Last(), degree))
                    .ToArray();

        }

        /// <summary>
        /// Obtain the Nurbs control points from a Hermite Spline
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="nurbsKnots"></param>
        /// <returns></returns>
        internal static Autodesk.DesignScript.Geometry.Point[] GetNurbsPoints(Autodesk.Revit.DB.HermiteSpline curve, double[] nurbsKnots)
        {

            int numKnots = nurbsKnots.Length;
            int numPoints = numKnots - 4;
            var pPoints = new XYZ[numPoints];
            var factorial = new double[] { 1, 1, 2, 6 };
            var power = new double[] { 1, -1, 1, -1 };

            for (int ii = 0; ii < numPoints; ii++)
            {
                double t = 0.5 * (nurbsKnots[ii] + nurbsKnots[ii + 1]);

                var del = new double[4];
                for (int jj = 1; jj <= 3; jj++)
                {
                    del[jj] = nurbsKnots[ii + jj] - t;
                }

                var s = Symmetric(del);

                var psi = new double[4];
                for (int k = 0; k <= 3; k++)
                {
                    double top = power[k] * factorial[k] * s[3 - k];
                    psi[k] = top / factorial[3];
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

                for (int r = 0; r <= 3; r++)
                {
                    int codegree = 3 - r;
                    double mul = power[codegree] * psi[codegree];

                    pPoints[ii] = pPoints[ii] + mul * derivs[r];
                }
            }

            return pPoints.Select(x => x.ToPoint(false)).ToArray();
        }

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

            var pS = new double[4];
            for (int jj = 0; jj <= degree; jj++)
                pS[jj] = stemp[degree, jj];

            return pS;
        }

    }
}
