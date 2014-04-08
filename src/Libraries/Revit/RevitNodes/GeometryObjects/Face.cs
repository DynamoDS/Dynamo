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
                return new Dictionary<string, object>()
                {
                    {"point", result.XYZPoint.ToPoint()},
                    {"uv", result.UVPoint.ToProtoType()},
                    {"dist", result.Distance},
                    {"edge", result.EdgeObject.Wrap()},
                    {"edgeParm", result.EdgeParameter }
                };
            }
            catch
            {
                return null;
            }
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
