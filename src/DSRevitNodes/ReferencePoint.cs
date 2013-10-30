using System;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;

namespace DSRevitNodes
{
    public class ReferencePoint
    {
        private Autodesk.Revit.DB.ReferencePoint _refPt;

        public Plane XYPlane
        {
            get
            {
                var cs = _refPt.GetCoordinateSystem();
                var xy = new Autodesk.Revit.DB.Plane(cs.BasisX, cs.BasisY);
                return xy.ToPlane();
            }
        }

        public Plane YZPlane
        {
            get
            {
                var cs = _refPt.GetCoordinateSystem();
                var yz = new Autodesk.Revit.DB.Plane(cs.BasisY, cs.BasisZ);
                return yz.ToPlane();
            }
        }

        public Plane XZPlane
        {
            get
            {
                var cs = _refPt.GetCoordinateSystem();
                var xz = new Autodesk.Revit.DB.Plane(cs.BasisX, cs.BasisZ);
                return xz.ToPlane();
            }
        }

        /// <summary>
        /// Create a Reference Point by x,y, and z coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        static ReferencePoint ByCoords(double x, double y, double z)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create a Reference Point from a point.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        static ReferencePoint ByPt(Point p)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create a Reference Point by UV coordinates on a face.
        /// </summary>
        /// <param name="f"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        static ReferencePoint ByPtOnFace(Face f, Vector v)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create a Reference Point at a parameter on an edge.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        static ReferencePoint ByPointOnEdge(Edge e, double t)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create a Reference Point offset from a point along a vector.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="normal"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        static ReferencePoint ByPtVectorDistance(Point p, Vector vec, double distance)
        {
            throw new NotImplementedException();
        }

    }
}
