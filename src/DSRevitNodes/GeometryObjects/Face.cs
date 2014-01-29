using Autodesk.DesignScript.Interfaces;
using Autodesk.Revit.DB;
using Revit.GeometryConversion;
using Revit.GeometryObjects;
using Revit.Graphics;

namespace Revit.GeometryObjects
{
    public class Face : AbstractGeometryObject
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

        protected override GeometryObject InternalGeometryObject
        {
            get { return InternalFace; }
        }

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
        /// Project a point onto a face
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public Autodesk.DesignScript.Geometry.Point Project(Autodesk.DesignScript.Geometry.Point point)
        {
            var result = InternalFace.Project(point.ToXyz());
            try
            {
                return result.XYZPoint.ToPoint();
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

        internal static Face FromExisting(Autodesk.Revit.DB.Face f)
        {
            return new Face(f);
        }

        #region Tesselation

        public override void Tessellate(IRenderPackage package)
        {
            var mesh = this.InternalFace.Triangulate(GraphicsManager.TesselationLevelOfDetail);

            for (var i = 0; i < mesh.NumTriangles; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    var xyz = mesh.get_Triangle(i).get_Vertex(i);
                    package.PushTriangleVertex(xyz.X, xyz.Y, xyz.Z);
                }
            }

        }

        #endregion
    }
}
