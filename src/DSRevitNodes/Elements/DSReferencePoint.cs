using System;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.Revit.DB;
using DSNodeServices;
using DSRevitNodes.Elements;
using DSRevitNodes.GeometryConversion;
using DSRevitNodes.GeometryObjects;
using DSRevitNodes.References;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Edge = Autodesk.DesignScript.Geometry.Edge;
using Plane = Autodesk.DesignScript.Geometry.Plane;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace DSRevitNodes.Elements
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
        private DSReferencePoint( Reference curveReference, double parameter )
        {
            //Phase 1 - Check to see if the object exists and should be rebound
            var oldRefPt = 
                ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.ReferencePoint>(Document);

            //There was a point, rebind to that, and adjust its position
            if (oldRefPt != null)
            {
                InternalSetReferencePoint(oldRefPt);
                // TODO: extend to support other types of parameterization, curve reversal
                InternalSetPointOnEdge(curveReference, parameter, PointOnCurveMeasurementType.NormalizedCurveParameter, PointOnCurveMeasureFrom.Beginning );
                return;
            }

            //Phase 2- There was no existing point, create one
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            // TODO: extend to support other types of parameterization, curve reversal
            var plc = new PointLocationOnCurve(PointOnCurveMeasurementType.NormalizedCurveParameter, parameter, PointOnCurveMeasureFrom.Beginning);
            var edgePoint = Document.Application.Create.NewPointOnEdge(curveReference, plc);
            InternalSetReferencePoint(Document.FamilyCreate.NewReferencePoint(edgePoint));

            TransactionManager.GetInstance().TransactionTaskDone();

            ElementBinder.SetElementForTrace(this.InternalElementId);
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
                InternalSetReferencePoint(oldRefPt);
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

        private void InternalSetPointOnEdge(Reference curveReference, double parameter, PointOnCurveMeasurementType measurementType, PointOnCurveMeasureFrom measureFrom)
        {
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            var plc = new PointLocationOnCurve(measurementType, parameter, measureFrom);
            var edgePoint = Document.Application.Create.NewPointOnEdge(curveReference, plc);
            InternalReferencePoint.SetPointElementReference(edgePoint);

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
            if (!Document.IsFamilyDocument)
            {
                throw new Exception("ReferencePoint Elements can only be created in a Family Document");
            }
            return new DSReferencePoint(x, y, z);
        }

        /// <summary>
        /// Create a Reference Point from a point.
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public static DSReferencePoint ByPoint(Point pt)
        {
            if (pt == null)
            {
                throw new ArgumentNullException("pt");
            }

            if (!Document.IsFamilyDocument)
            {
                throw new Exception("ReferencePoint Elements can only be created in a Family Document");
            }

            return new DSReferencePoint(pt.X, pt.Y, pt.Z);
        }

        /// <summary>
        /// Create a Reference Point Element offset from a point along a vector.
        /// </summary>
        /// <param name="basePoint"></param>
        /// <param name="direction"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static DSReferencePoint ByPointVectorDistance(Point basePoint, Vector direction, double distance)
        {
            if (!Document.IsFamilyDocument)
            {
                throw new Exception("ReferencePoint Elements can only be created in a Family Document");
            }

            if (basePoint == null)
            {
                throw new ArgumentNullException("basePoint");
            }

            if (direction == null)
            {
                throw new ArgumentNullException("direction");
            }

            var pt = basePoint.ToXyz() + direction.ToXyz() * distance;

            return new DSReferencePoint(pt.X, pt.Y, pt.Z);

        }

        /// <summary>
        /// Create a Reference Point at a parameter on an edge.
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static DSReferencePoint ByPointOnEdge(DSCurveReference edge, double parameter)
        {
            if (edge == null)
            {
                throw new ArgumentNullException("edge");
            }

            return new DSReferencePoint(edge.InternalReference, parameter);

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

        #region Incomplete static constructors

        /// <summary>
        /// Create a Reference Point by UV coordinates on a face.
        /// </summary>
        /// <param name="f"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        static DSReferencePoint ByPointOnFace(DSFaceReference f, Autodesk.DesignScript.Geometry.Vector v)
        {
            throw new NotImplementedException();
        }


        #endregion
    }
}

