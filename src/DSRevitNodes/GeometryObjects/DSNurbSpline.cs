using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Interfaces;
using Autodesk.Revit.DB;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace DSRevitNodes.GeometryObjects
{

    /// <summary>
    /// Class for making a Revit NurbsSpline.
    /// </summary>
    public class DSNurbSpline : DSCurve
    {

        #region Private constructors

        /// <summary>
        /// Internal constructor DSNurbsSpline
        /// </summary>
        /// <param name="controlPoints"></param>
        /// <param name="weights"></param>
        /// <param name="knots"></param>
        /// <param name="degree"></param>
        /// <param name="closed"></param>
        /// <param name="rational"></param>
        internal DSNurbSpline(Autodesk.DesignScript.Geometry.Point[] controlPoints, double[] weights, double[] knots, int degree, bool closed, bool rational)
        {
            var c = NurbSpline.Create(controlPoints.ToXyzs(), weights, knots, degree, closed, rational);
            this.InternalCurve = c;
        }

       /// <summary>
       /// Internal constructor for DSNurbsSpline
       /// </summary>
       /// <param name="controlPoints"></param>
       /// <param name="weights"></param>
        internal DSNurbSpline(Autodesk.DesignScript.Geometry.Point[] controlPoints, double[] weights)
        {
           var c = NurbSpline.Create(controlPoints.ToXyzs(), weights);
           this.InternalCurve = c;
        }

        #endregion

        #region Static constructors

        /// <summary>
        /// Create a DSNurbsSpline
        /// </summary>
        /// <param name="controlPoints"></param>
        /// <param name="weights"></param>
        /// <param name="knots"></param>
        /// <param name="degree"></param>
        /// <param name="closed"></param>
        /// <param name="rational"></param>
        /// <returns></returns>
        public static DSNurbSpline ByControlPointsWeightsKnotsDegreeClosedAndRationality(Autodesk.DesignScript.Geometry.Point[] controlPoints, double[] weights, double[] knots, int degree, bool closed, bool rational)
        {
            return new DSNurbSpline(controlPoints, weights, knots, degree, closed, rational);
        }

        /// <summary>
        /// Create a DSNurbsSpline assuming that the curve has equally spaced
        /// knots, is degree 3, is not closed, and is rational
        /// </summary>
        /// <param name="controlPoints"></param>
        /// <param name="weights"></param>
        /// <returns></returns>
        public static DSNurbSpline ByControlPointsAndWeights(Autodesk.DesignScript.Geometry.Point[] controlPoints, double[] weights)
        {
            return new DSNurbSpline(controlPoints, weights);
        }

        #endregion

    }
}
