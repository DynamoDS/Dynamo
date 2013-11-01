using System;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.Revit.DB;
using DSNodeServices;
using RevitServices.Transactions;
using Edge = Autodesk.DesignScript.Geometry.Edge;
using Plane = Autodesk.DesignScript.Geometry.Plane;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace DSRevitNodes
{

    /// <summary>
    /// A Revit Reference Point
    /// </summary>
    [RegisterForTrace]
    public class ReferencePoint : AbstractGeometry, IGraphicItem
    {
        private Autodesk.Revit.DB.ReferencePoint internalRefPt;

        /// <summary>
        /// Internal constructor for the ReferencePoint
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        private ReferencePoint(double x, double y, double z)
        {
            var transaction = new TransactionManager().StartTransaction(Document);

            internalRefPt = Document.FamilyCreate.NewReferencePoint(new XYZ(x, y, z));
            this.InternalID = internalRefPt.Id;

            transaction.CommitTransaction();
        }

        private void SetPosition(XYZ xyz)
        {
            var transaction = new TransactionManager().StartTransaction(Document);
            internalRefPt.Position = xyz;
            transaction.CommitTransaction();
        }

        public double X { 
            get { return internalRefPt.Position.X; } 
            set { SetPosition(new XYZ(value, Y, Z)); }
        }



        public double Y
        {
            get { return internalRefPt.Position.Y; }
            set { SetPosition(new XYZ(X, value, Z)); }
        }


        public double Z
        {
            get { return internalRefPt.Position.Z; }
            set { SetPosition(new XYZ(X, Y, value)); }
        }

        
        public Plane XYPlane
        {
            get
            {
                var cs = internalRefPt.GetCoordinateSystem();
                var xy = new Autodesk.Revit.DB.Plane(cs.BasisX, cs.BasisY);
                return xy.ToPlane();
            }
        }

        public Plane YZPlane
        {
            get
            {
                var cs = internalRefPt.GetCoordinateSystem();
                var yz = new Autodesk.Revit.DB.Plane(cs.BasisY, cs.BasisZ);
                return yz.ToPlane();
            }
        }

        public Plane XZPlane
        {
            get
            {
                var cs = internalRefPt.GetCoordinateSystem();
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
        public static ReferencePoint ByCoords(double x, double y, double z)
        {
           return new ReferencePoint(x,y,z);
        }

        /// <summary>
        /// Create a Reference Point from a point.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        static ReferencePoint ByPt(Point pt)
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

        /// <summary>
        /// Tessellate Reference Point to render package for visualization.
        /// </summary>
        /// <param name="package"></param>
        void IGraphicItem.Tessellate(IRenderPackage package)
        {
            package.PushPointVertex(this.X, this.Y, this.Z);
        }
    }
}
