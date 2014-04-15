using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Revit.GeometryConversion;
using Revit.GeometryObjects;
using Revit.Graphics;
using UV = Autodesk.Revit.DB.UV;

namespace Revit.GeometryObjects
{
    /// <summary>
    /// A Revit Face
    /// 
    /// Note: This class is required as there is no known way to robustly convert
    /// a Revit Face into its ProtoGeometry equivalent.
    /// </summary>
    public class Face : GeometryObject
    {

        internal Autodesk.Revit.DB.Face InternalFace
        {
            get; private set;
        }

        internal Face(Autodesk.Revit.DB.Face face)
        {
            InternalSetFace(face);
        }

        private void InternalSetFace(Autodesk.Revit.DB.Face face)
        {
            this.InternalFace = face;
        }

        protected override Autodesk.Revit.DB.GeometryObject InternalGeometryObject
        {
            get { return InternalFace; }
        }

        #region Public methods

        public override object[] Explode()
        {
            return this.Edges;
        }

        #endregion

        #region Public properties

        public bool IsTwoSided
        {
            get
            {
                return InternalFace.IsTwoSided;
            }
        }

        /// <summary>
        /// Get the Edges of the Face
        /// </summary>
        public Edge[] Edges
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Get the Surface Area of the face
        /// </summary>
        public double SurfaceArea
        {
            get
            {
                return InternalFace.Area;
            }
        }

        #endregion

        /// <summary>
        /// Evaluate a point on a Face given it's parameters
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public Autodesk.DesignScript.Geometry.Point PointAtParameter(double u, double v)
        {
            return InternalFace.Evaluate(new UV(u, v)).ToPoint();
        }

        /// <summary>
        /// Project a Point onto a Face
        /// </summary>
        /// <param name="point">The point to project onto the Face</param>
        /// <returns name="point">The nearest Point to the projected Point on the Face</returns>
        /// <returns name="uv">The UV coordinates of the nearest Point on the Face</returns>
        /// <returns name="dist">The distance from the Point to the Face</returns>
        /// <returns name="edge">The edge if projected Point is near an Edge</returns>
        /// <returns name="edgeParm">The parameter on the Edge if the point is near an Edge</returns>
        [MultiReturn(new []{"point", "uv", "dist", "edge", "edgeParm"})]
        public Dictionary<string, object> Project(Autodesk.DesignScript.Geometry.Point point)
        {
            try
            {
                var result = InternalFace.Project(point.ToXyz());
                if (result == null) return null;

                return new Dictionary<string, object>()
                {
                    {"point", result.XYZPoint != null ? result.XYZPoint.ToPoint(): null},
                    {"uv", result.UVPoint != null ? result.UVPoint.ToProtoType() : null},
                    {"dist", result.Distance},
                    {"edge", result.EdgeObject != null ? result.EdgeObject.Wrap() : null},
                    {"edgeParm", result.EdgeParameter }
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Calculate the intersection of a Curve and a Revit Face
        /// </summary>
        /// <param name="curve"></param>
        /// <returns name="point">The nearest Point to the projected Point on the Face</returns>
        /// <returns name="uv">The UV coordinates of the nearest Point on the Face</returns>
        /// <returns name="dist">The distance from the Point to the Face</returns>
        /// <returns name="edge">The edge if projected Point is near an Edge</returns>
        /// <returns name="edgeParm">The parameter on the Edge if the point is near an Edge</returns>
        [MultiReturn(new[] { "xyz", "uv", "parm", "edge", "edgeParm" })]
        public Dictionary<string, object> Intersect(Autodesk.DesignScript.Geometry.Curve curve)
        {
            if (curve == null)
            {
                throw new System.ArgumentNullException("curve");
            }

            var revitFace = this.InternalFace;
            Autodesk.Revit.DB.IntersectionResultArray xsects = null;

            var result = revitFace.Intersect(curve.ToRevitType(), out xsects);

            var pts = new List<Autodesk.DesignScript.Geometry.Point>();
            var uvs = new List<Autodesk.DesignScript.Geometry.UV>();
            var parms = new List<double>();
            var edges = new List<Edge>();
            var edgeParms = new List<double>();

            if (xsects != null)
            {
                foreach (IntersectionResult ir in xsects)
                {
                    try
                    {
                        edgeParms.Add(ir.EdgeParameter);
                    }
                    catch
                    {
                        edgeParms.Add(0);
                    }

                    edges.Add(ir.EdgeObject != null ? Edge.FromExisting(ir.EdgeObject) : null);

                    parms.Add(ir.Parameter);

                    uvs.Add(Autodesk.DesignScript.Geometry.UV.ByCoordinates(ir.UVPoint.U, ir.UVPoint.V));
                    pts.Add(ir.XYZPoint.ToPoint());
                }
            }

            return new Dictionary<string, object>()
                {
                    {"point", pts.ToArray() },
                    {"uv", uvs.ToArray() },
                    {"parm", parms.ToArray() },
                    {"edge", edges.ToArray() },
                    {"edgeParm", edgeParms.ToArray() }
                };
        }

        /// <summary>
        /// Calculate the intersection of two Faces, returning a list of Curves
        /// </summary>
        /// <param name="face1"></param>
        /// <param name="face2"></param>
        /// <returns>A list of curves or an empty list if there is no intersection</returns>
        public Autodesk.DesignScript.Geometry.Curve[] Intersect(Face face2)
        {
            var face1 = this;

            if (face2 == null)
            {
                throw new System.ArgumentNullException("face2");
            }

            var revitFace1 = face1.InternalFace;
            var revitFace2 = face2.InternalFace;

            Autodesk.Revit.DB.Curve curve;
            var rez = revitFace1.Intersect(revitFace2, out curve);

            if (rez == FaceIntersectionFaceResult.Intersecting)
            {
                return new[] { curve.ToProtoType() };
            }

            return new Autodesk.DesignScript.Geometry.Curve[] { };
        }

        /// <summary>
        /// Evaluate the normal on a Face given it's parameters
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public Autodesk.DesignScript.Geometry.Vector NormalAtParameter(double u, double v)
        {
            return InternalFace.ComputeNormal(new UV(u, v)).ToVector();
        }

        /// <summary>
        /// Obtain the derivatives at a location on the Face as a CoordinateSystem.  The X and Y axis of the
        /// CoordinateSystem are aligned with the U and V directions of the Face at that point and the Z axis
        /// is normal to the Face.  The X and Y Axis is scaled to the magnitude of the derivatives.
        /// </summary>
        /// <param name="u">The U parameter</param>
        /// <param name="v">The V parameter</param>
        /// <returns>The CoordinateSystem</returns>
        public Autodesk.DesignScript.Geometry.CoordinateSystem DerivativesAtParameter(double u, double v)
        {
            return InternalFace.ComputeDerivatives(new UV(u, v)).ToCoordinateSystem();
        }

        /// <summary>
        /// Obtain the derivatives at a location on the Face as a CoordinateSystem.  The X and Y axis of the
        /// CoordinateSystem are aligned with the U and V directions of the Face at that point and the Z axis
        /// is normal to the Face.  The X and Y axis is normalized.
        /// </summary>
        /// <param name="u">The U parameter</param>
        /// <param name="v">The V parameter</param>
        /// <returns>The CoordinateSystem</returns>
        public Autodesk.DesignScript.Geometry.CoordinateSystem CoordinateSystemAtParameter(double u, double v)
        {
            var t = InternalFace.ComputeDerivatives(new UV(u, v));
            t.BasisX = t.BasisX.Normalize();
            t.BasisZ = t.BasisZ.Normalize();
            t.BasisY = t.BasisX.CrossProduct(t.BasisZ);

            return t.ToCoordinateSystem();
        }

        internal static Face FromExisting(Autodesk.Revit.DB.Face f)
        {
            return new Face(f);
        }

        #region Tesselation

        public override void Tessellate(IRenderPackage package, double tol, int gridLines)
        {
            var mesh = this.InternalFace.Triangulate(GraphicsManager.TesselationLevelOfDetail);
            GraphicsManager.PushMesh(mesh, package);
        }

        #endregion
    }
}
