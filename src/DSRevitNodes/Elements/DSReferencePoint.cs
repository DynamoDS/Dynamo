using System;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.Revit.DB;
using DSNodeServices;
using DSRevitNodes.Elements;
using DSRevitNodes.GeometryConversion;
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
    public class DSReferencePoint : AbstractElement, IGraphicItem
    {

        #region Internal properties

        /// <summary>
        /// Internal variable containing the wrapped Revit object
        /// </summary>
        internal Autodesk.Revit.DB.ReferencePoint InternalReferencePoint
        {
            get; private set;
        }

        /// <summary>
        /// Reference to the Element
        /// </summary>
        internal override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalReferencePoint; }
        }

        #endregion

        #region Private constructors

        /// <summary>
        /// Internal constructor for wrapping a ReferencePoint. The returned
        /// object is Revit owned
        /// </summary>
        /// <param name="pt"></param>
        private DSReferencePoint(Autodesk.Revit.DB.ReferencePoint refPt)
        {
            InternalSetReferencePoint(refPt);
        }

        /// <summary>
        /// Internal constructor for the ReferencePoint
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        private DSReferencePoint(double x, double y, double z)
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

            InternalSetReferencePoint(Document.FamilyCreate.NewReferencePoint(new XYZ(x, y, z)));

            TransactionManager.GetInstance().TransactionTaskDone();

            ElementBinder.SetElementForTrace(this.InternalElementId);
        }

        #endregion

        #region Private mutators

        private void InternalSetPosition(XYZ xyz)
        {
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            InternalReferencePoint.Position = xyz;

            TransactionManager.GetInstance().TransactionTaskDone();
        }

        private void InternalSetReferencePoint(ReferencePoint p)
        {

            InternalReferencePoint = p;
            this.InternalElementId = InternalReferencePoint.Id;
            this.InternalUniqueId = InternalReferencePoint.UniqueId;
        }

        #endregion

        #region Public properties

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

        #endregion

        #region Static constructors

        /// <summary>
        /// Create a Reference Point by x, y, and z coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static DSReferencePoint ByCoordinates(double x, double y, double z)
        {
            return new DSReferencePoint(x, y, z);
        }

        /// <summary>
        /// Create a Reference Point from a point.
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public static DSReferencePoint ByPoint(Point pt)
        {
            return new DSReferencePoint(pt.X, pt.Y, pt.Z);
        }

        /// <summary>
        /// Create a Reference Point by UV coordinates on a face.
        /// </summary>
        /// <param name="f"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        static DSReferencePoint ByPointOnFace(DSFace f, Vector v)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create a Reference Point at a parameter on an edge.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        static DSReferencePoint ByPointOnEdge(Edge e, double t)
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
        static DSReferencePoint ByPointVectorDistance(Point p, Vector vec, double distance)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Internal static constructors 

        /// <summary>
        /// Create a Reference Point from a user selected Element.
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        internal static DSReferencePoint FromExisting(Autodesk.Revit.DB.ReferencePoint pt, bool isRevitOwned)
        {
            return new DSReferencePoint(pt)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion

        #region Tesselation

        /// <summary>
        /// Tessellate Reference Point to render package for visualization.
        /// </summary>
        /// <param name="package"></param>
        void IGraphicItem.Tessellate(IRenderPackage package)
        {
            package.PushPointVertex(this.X, this.Y, this.Z);
        }

        #endregion

    }
}
