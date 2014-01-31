using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Interfaces;
using Autodesk.Revit.DB;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace Revit.GeometryObjects
{

    /// <summary>
    /// Class for making a Revit NurbsSpline.
    /// </summary>
    public class NurbSpline : Curve
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
        internal NurbSpline(Autodesk.DesignScript.Geometry.Point[] controlPoints, double[] weights, double[] knots, int degree, bool closed, bool rational)
        {
            var c = Autodesk.Revit.DB.NurbSpline.Create(controlPoints.ToXyzs(), weights, knots, degree, closed, rational);
            this.InternalCurve = c;
        }

       /// <summary>
       /// Internal constructor for DSNurbsSpline
       /// </summary>
       /// <param name="controlPoints"></param>
       /// <param name="weights"></param>
        internal NurbSpline(Autodesk.DesignScript.Geometry.Point[] controlPoints, double[] weights)
        {
            TransactionManager.GetInstance().EnsureInTransaction(DocumentManager.GetInstance().CurrentDBDocument);

           var c = Autodesk.Revit.DB.NurbSpline.Create(controlPoints.ToXyzs(), weights);
           this.InternalCurve = c;

            TransactionManager.GetInstance().TransactionTaskDone();
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
        public static NurbSpline ByControlPointsWeightsKnotsDegreeClosedAndRationality(Autodesk.DesignScript.Geometry.Point[] controlPoints, double[] weights, double[] knots, int degree, bool closed, bool rational)
        {
            return new NurbSpline(controlPoints, weights, knots, degree, closed, rational);
        }

        /// <summary>
        /// Create a DSNurbsSpline assuming that the curve has equally spaced
        /// knots, is degree 3, is not closed, and is rational
        /// </summary>
        /// <param name="controlPoints"></param>
        /// <param name="weights"></param>
        /// <returns></returns>
        public static NurbSpline ByControlPointsAndWeights(Autodesk.DesignScript.Geometry.Point[] controlPoints, double[] weights)
        {
            return new NurbSpline(controlPoints, weights);
        }

        #endregion

    }
}
