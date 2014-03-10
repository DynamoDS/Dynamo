using System;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.Revit.DB;
using DSNodeServices;
using Revit.Elements;
using Revit.GeometryConversion;
using Revit.GeometryObjects;
using Revit.References;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Edge = Autodesk.DesignScript.Geometry.Edge;
using Plane = Autodesk.DesignScript.Geometry.Plane;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace Revit.Elements
{

    /// <summary>
    /// A Revit Reference Point
    /// </summary>
    [RegisterForTrace]
    [ShortName("refPt")]
    public class ReferencePoint : AbstractElement, IGraphicItem
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
        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalReferencePoint; }
        }

        #endregion

        #region Private constructors

        /// <summary>
        /// Internal constructor for wrapping a ReferencePoint. 
        /// </summary>
        /// <param name="refPt"></param>
        private ReferencePoint(Autodesk.Revit.DB.ReferencePoint refPt)
        {
            InternalSetReferencePoint(refPt);
        }

        /// <summary>
        /// Internal constructor for ReferencePoint Elements that a persistent relationship to a Face
        /// </summary>
        /// <param name="faceReference"></param>
        /// <param name="uv"></param>
        private ReferencePoint(Autodesk.Revit.DB.Reference faceReference, Autodesk.Revit.DB.UV uv)
        {
            //Phase 1 - Check to see if the object exists and should be rebound
            var oldRefPt =
                ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.ReferencePoint>(Document);

            //There was a point, rebind to that, and adjust its position
            if (oldRefPt != null)
            {
                InternalSetReferencePoint(oldRefPt);
                InternalSetPointOnFace(faceReference, uv);
                return;
            }

            //Phase 2- There was no existing point, create one
            TransactionManager.Instance.EnsureInTransaction(Document);

            var edgePoint = Document.Application.Create.NewPointOnFace(faceReference, uv);
            InternalSetReferencePoint(Document.FamilyCreate.NewReferencePoint(edgePoint));

            TransactionManager.Instance.TransactionTaskDone();

            ElementBinder.SetElementForTrace(this.InternalElementId);
        }

        /// <summary>
        /// Internal constructor for ReferencePoint Elements that a persistent relationship to a Curve
        /// </summary>
        /// <param name="curveReference"></param>
        /// <param name="parameter"></param>
        /// <param name="measurementType"></param>
        /// <param name="measureFrom"></param>
        private ReferencePoint(Reference curveReference, double parameter, PointOnCurveMeasurementType measurementType,
            PointOnCurveMeasureFrom measureFrom)
        {
            //Phase 1 - Check to see if the object exists and should be rebound
            var oldRefPt =
                ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.ReferencePoint>(Document);

            //There was a point, rebind to that, and adjust its position
            if (oldRefPt != null)
            {
                InternalSetReferencePoint(oldRefPt);
                InternalSetPointOnCurve(curveReference, parameter, measurementType, measureFrom);
                return;
            }

            //Phase 2- There was no existing point, create one
            TransactionManager.Instance.EnsureInTransaction(Document);

            var plc = new PointLocationOnCurve(measurementType, parameter, measureFrom);
            var edgePoint = Document.Application.Create.NewPointOnEdge(curveReference, plc);
            InternalSetReferencePoint(Document.FamilyCreate.NewReferencePoint(edgePoint));

            TransactionManager.Instance.TransactionTaskDone();

            ElementBinder.SetElementForTrace(this.InternalElementId);
        }

        /// <summary>
        /// Internal constructor for the ReferencePoint
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        private ReferencePoint(double x, double y, double z)
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
            TransactionManager.Instance.EnsureInTransaction(Document);

            InternalSetReferencePoint(Document.FamilyCreate.NewReferencePoint(new XYZ(x, y, z)));

            TransactionManager.Instance.TransactionTaskDone();

            ElementBinder.SetElementForTrace(this.InternalElementId);
        }

        #endregion

        #region Private mutators

        private void InternalSetPosition(XYZ xyz)
        {
            TransactionManager.Instance.EnsureInTransaction(Document);

            InternalReferencePoint.Position = xyz;

            TransactionManager.Instance.TransactionTaskDone();
        }

        private void InternalSetPointOnFace(Reference faceReference, Autodesk.Revit.DB.UV uv)
        {
            TransactionManager.Instance.EnsureInTransaction(Document);

            var edgePoint = Document.Application.Create.NewPointOnFace(faceReference, uv);
            InternalReferencePoint.SetPointElementReference(edgePoint);

            TransactionManager.Instance.TransactionTaskDone();
        }

        private void InternalSetPointOnCurve(Reference curveReference, double parameter,
            PointOnCurveMeasurementType measurementType, PointOnCurveMeasureFrom measureFrom)
        {
            TransactionManager.Instance.EnsureInTransaction(Document);

            var plc = new PointLocationOnCurve(measurementType, parameter, measureFrom);
            var edgePoint = Document.Application.Create.NewPointOnEdge(curveReference, plc);
            InternalReferencePoint.SetPointElementReference(edgePoint);

            TransactionManager.Instance.TransactionTaskDone();
        }

        private void InternalSetReferencePoint(Autodesk.Revit.DB.ReferencePoint p)
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

        public Point Point
        {
            get
            {
                return InternalReferencePoint.Position.ToPoint();
            }
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

        public String Id { get { return InternalElementId.ToString(); } 
        }


        #endregion

        #region Public static constructors

        /// <summary>
        /// Create a Reference Point by x, y, and z coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static ReferencePoint ByCoordinates(double x, double y, double z)
        {
            if (!Document.IsFamilyDocument)
            {
                throw new Exception("ReferencePoint Elements can only be created in a Family Document");
            }
            return new ReferencePoint(x, y, z);
        }

        /// <summary>
        /// Create a Reference Point from a point.
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public static ReferencePoint ByPoint(Point pt)
        {
            if (pt == null)
            {
                throw new ArgumentNullException("pt");
            }

            if (!Document.IsFamilyDocument)
            {
                throw new Exception("ReferencePoint Elements can only be created in a Family Document");
            }

            return new ReferencePoint(pt.X, pt.Y, pt.Z);
        }

        /// <summary>
        /// Create a Reference Point Element offset from a point along a vector
        /// </summary>
        /// <param name="basePoint"></param>
        /// <param name="direction"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static ReferencePoint ByPointVectorDistance(Autodesk.DesignScript.Geometry.Point basePoint, Autodesk.DesignScript.Geometry.Vector direction, double distance)
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

            return new ReferencePoint(pt.X, pt.Y, pt.Z);

        }

        /// <summary>
        /// Create a Reference Point at a particular length along a curve
        /// </summary>
        /// <param name="curveReference"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static ReferencePoint ByLengthOnCurveReference(CurveReference curveReference, double length)
        {
            if (!Document.IsFamilyDocument)
            {
                throw new Exception("ReferencePoint Elements can only be created in a Family Document");
            }

            if (curveReference == null)
            {
                throw new ArgumentNullException("curveReference");
            }

            return new ReferencePoint(curveReference.InternalReference, length, PointOnCurveMeasurementType.SegmentLength, PointOnCurveMeasureFrom.Beginning);
        }

        /// <summary>
        /// Create a Reference Point at a parameter on an Curve.  This introduces a persistent relationship between
        /// Elements in the Revit document.
        /// </summary>
        /// <param name="curveReference"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static ReferencePoint ByParameterOnCurveReference(CurveReference curveReference, double parameter)
        {
            if (!Document.IsFamilyDocument)
            {
                throw new Exception("ReferencePoint Elements can only be created in a Family Document");
            }

            if (curveReference == null)
            {
                throw new ArgumentNullException("curveReference");
            }

            return new ReferencePoint(curveReference.InternalReference, parameter, PointOnCurveMeasurementType.NormalizedCurveParameter, PointOnCurveMeasureFrom.Beginning);
        }

        /// <summary>
        /// Create a Reference Point by UV coordinates on a Face. This introduces a persistent relationship between
        /// Elements in the Revit document.
        /// </summary>
        /// <param name="face"></param>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static ReferencePoint ByParametersOnFaceReference(FaceReference face, double u, double v)
        {
            if (!Document.IsFamilyDocument)
            {
                throw new Exception("ReferencePoint Elements can only be created in a Family Document");
            }

            if (face == null)
            {
                throw new ArgumentNullException("face");
            }

            return new ReferencePoint(face.InternalReference, new Autodesk.Revit.DB.UV(u, v));
        }

        #endregion

        #region Internal static constructors 

        /// <summary>
        /// Create a Reference Point from a user selected Element.
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        internal static ReferencePoint FromExisting(Autodesk.Revit.DB.ReferencePoint pt, bool isRevitOwned)
        {
            return new ReferencePoint(pt)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion

        public override string ToString()
        {
            return string.Format("Reference Point: Location=(X={0}, Y={1}, Z={2})", InternalReferencePoint.Position.X,
                InternalReferencePoint.Position.Y, InternalReferencePoint.Position.Z);
        }

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

