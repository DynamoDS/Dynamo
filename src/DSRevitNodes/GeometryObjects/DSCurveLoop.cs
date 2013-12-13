using System;
using System.Collections.Generic;
using System.Data;
using System.Deployment.Internal;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.Revit.DB;
using DSRevitNodes.GeometryConversion;

namespace DSRevitNodes.GeometryObjects
{
    /// <summary>
    /// A class representing a Revit CurveLoop
    /// </summary>
    public class DSCurveLoop
    {
        #region Internal properties

        internal Autodesk.Revit.DB.CurveLoop InternalCurveLoop
        {
            get; private set;
        }

        #endregion

        #region Private constructors

        /// <summary>
        /// Internal constructor for the CurveLoop type
        /// </summary>
        /// <param name="loop"></param>
        private DSCurveLoop(Autodesk.Revit.DB.CurveLoop loop)
        {
            this.InternalCurveLoop = loop;
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Identifies if the Curveloop is closed
        /// </summary>
        public bool IsClosed
        {
            get
            {
                return !InternalCurveLoop.IsOpen();
            }
        }

        /// <summary>
        /// Identifies if the curves making up the loop are planar
        /// </summary>
        public bool IsPlanar
        {
            get
            {
                return InternalCurveLoop.HasPlane();
            }
        }

        #endregion

        #region Static constructors

        /// <summary>
        /// Build a CurveLoop from a set of curves. The curves should be in order
        /// and linked such that their endpoints are coincident with the next curves
        /// start point
        /// </summary>
        /// <param name="curves"></param>
        /// <returns></returns>
        public static DSCurveLoop ByCurves(Autodesk.DesignScript.Geometry.Curve[] curves)
        {
            if (curves == null)
            {
                throw new ArgumentNullException("curves");
            }

            var loop = new Autodesk.Revit.DB.CurveLoop();
            curves.ForEach(x => loop.Append(x.ToRevitType())); 
            return new DSCurveLoop(loop);
        }

        #endregion

    }

}
