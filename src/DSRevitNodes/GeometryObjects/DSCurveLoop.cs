using System;
using System.Collections.Generic;
using System.Data;
using System.Deployment.Internal;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;

namespace DSRevitNodes.GeometryObjects
{
    /// <summary>
    /// A class representing a Revit CurveLoop
    /// </summary>
    public class DSCurveLoop
    {
        internal Autodesk.Revit.DB.CurveLoop InternalCurveLoop
        {
            get; private set;
        }

        /// <summary>
        /// Internal constructor for the CurveLoop type
        /// </summary>
        /// <param name="loop"></param>
        private DSCurveLoop(Autodesk.Revit.DB.CurveLoop loop)
        {
            this.InternalCurveLoop = loop;
        }

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

        /// <summary>
        /// Build a CurveLoop from a set of curves. The curves should be in order
        /// and linked such that their endpoints are coincident with the next curves
        /// start point
        /// </summary>
        /// <param name="curves"></param>
        /// <returns></returns>
        public static DSCurveLoop ByCurves(DSCurve[] curves)
        {
            if (curves == null)
            {
                throw new ArgumentNullException("curves");
            }

            var loop = new Autodesk.Revit.DB.CurveLoop();
            curves.ForEach(x => loop.Append(x.InternalCurve)); 
            return new DSCurveLoop(loop);
        }

    }

}
