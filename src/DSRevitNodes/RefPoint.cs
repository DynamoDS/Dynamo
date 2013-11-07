using System;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.Revit.DB;
using DSNodeServices;
using RevitServices.Persistence;
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
    [ShortName("refPt")]
    public class RefPoint : AbstractGeometry, IGraphicItem
    {
        /// <summary>
        /// Internal variable containing the wrapped Revit object
        /// </summary>
        public Autodesk.Revit.DB.ReferencePoint InternalReferencePoint
        {
            get; set;
        }

        /// <summary>
        /// Internal constructor for the ReferencePoint
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        private RefPoint(double x, double y, double z)
        {
            //Phase 1 - Check to see if the object exists and should be rebound
            var oldRefPt = 
                ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.ReferencePoint>(Document);

            //There was a point, rebind to that, and adjust its position
            if (oldRefPt != null)
            {
                InternalReferencePoint = oldRefPt;
                InternalSetPosition(new XYZ(x, y, z));
                return;
            }

            //Phase 2- There was no existing point, create one
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            InternalReferencePoint = Document.FamilyCreate.NewReferencePoint(new XYZ(x, y, z));
            this.InternalID = InternalReferencePoint.Id;

            TransactionManager.GetInstance().TransactionTaskDone();

            //ElementBinder.SetElementForTrace(this.InternalID);
        }

        private void InternalSetPosition(XYZ xyz)
        {
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            InternalReferencePoint.Position = xyz;

            TransactionManager.GetInstance().TransactionTaskDone();
        }

        public double X
        {
            get { return InternalReferencePoint.Position.X; }
            set { InternalSetPosition(new XYZ(value, Y, Z)); }
        }


        public double Y
        {
            get { return InternalReferencePoint.Position.Y; }
            set { InternalSetPosition(new XYZ(X, value, Z)); }
        }


        public double Z
        {
            get { return InternalReferencePoint.Position.Z; }
            set { InternalSetPosition(new XYZ(X, Y, value)); }
        }

        
        public Plane XYPlane
        {
            get
            {
                var cs = InternalReferencePoint.GetCoordinateSystem();
                var xy = new Autodesk.Revit.DB.Plane(cs.BasisX, cs.BasisY);
                return xy.ToPlane();
            }
        }

        public Plane YZPlane
        {
            get
            {
                var cs = InternalReferencePoint.GetCoordinateSystem();
                var yz = new Autodesk.Revit.DB.Plane(cs.BasisY, cs.BasisZ);
                return yz.ToPlane();
            }
        }

        public Plane XZPlane
        {
            get
            {
                var cs = InternalReferencePoint.GetCoordinateSystem();
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
        public static RefPoint ByCoordinates(double x, double y, double z)
        {
            try
            {
                var pt = new RefPoint(x, y, z);
                return pt;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        /// <summary>
        /// Create a Reference Point from a point.
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        static RefPoint ByPoint(Point pt)
        {
            return new RefPoint(pt.X, pt.Y, pt.Z);
        }

        /// <summary>
        /// Create a Reference Point by UV coordinates on a face.
        /// </summary>
        /// <param name="f"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        static RefPoint ByPointOnFace(Face f, Vector v)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create a Reference Point at a parameter on an edge.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        static RefPoint ByPointOnEdge(Edge e, double t)
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
        static RefPoint ByPointVectorDistance(Point p, Vector vec, double distance)
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
