using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;

namespace Revit.GeometryConversion
{
    [SupressImportIntoVM]
    public static class NurbsUtils
    {
        /// <summary>
        /// Elevate the degree of a Bezier curve (represented in NURBS form) to a given degree
        /// without changing the shape
        /// </summary>
        /// <param name="crv">The curve</param>
        /// <param name="finalDegree">The requested degree</param>
        /// <returns></returns>
        public static NurbsCurve ElevateBezierDegree(NurbsCurve crv, int finalDegree)
        {
            if (crv.ControlPoints().Count() != crv.Degree + 1 || crv.IsRational)
            {
                throw new Exception("The supplied curve for degree elevation must be a bezier curve");
            }

            if (crv.Degree > finalDegree)
            {
                throw new Exception("The supplied curve for degree elevation must have a " +
                    "lesser degree than that required");
            }

            while (crv.Degree < finalDegree)
            {
                crv = ElevateBezierDegreeBy1(crv);
            }

            return crv;
        }

        #region Helper methods

        private static NurbsCurve ElevateBezierDegreeBy1(NurbsCurve crv)
        {
            var cpts = crv.ControlPoints();
            var n = crv.Degree + 1;

            var cptsFinal = new List<Point>();

            cptsFinal.Add(cpts.First());

            for (var i = 1; i < cpts.Length; i++)
            {
                cptsFinal.Add(Interp(cpts[i - 1], cpts[i], (double)i / n));
            }

            cptsFinal.Add(cpts.Last());

            var oldKnots = crv.Knots();

            var knots = HermiteToNurbs.Clamp(new[] { oldKnots.First(), oldKnots.Last() }, n);
            var weights = Enumerable.Repeat(1.0, cptsFinal.Count).ToArray();

            return NurbsCurve.ByControlPointsWeightsKnots(cptsFinal, weights, knots, n);
        }

        private static Point Interp(Point p0,
                                      Point p1,
                                      double dist)
        {
            var v0 = p0.AsVector().Scale(dist);
            var v1 = p1.AsVector().Scale(1 - dist);

            return v0.Add(v1).AsPoint();
        }

        #endregion

    }

}
