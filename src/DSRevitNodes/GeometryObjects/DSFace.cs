using Autodesk.DesignScript.Interfaces;
using Autodesk.Revit.DB;
using DSRevitNodes.GeometryConversion;
using DSRevitNodes.Graphics;

namespace DSRevitNodes.GeometryObjects
{
    public class DSFace : AbstractGeometryObject
    {

        internal Autodesk.Revit.DB.Face InternalFace
        {
            get; private set;
        }

        internal DSFace(Autodesk.Revit.DB.Face face)
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
        public DSEdge[] Edges
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
        public Autodesk.DesignScript.Geometry.Point Evaluate(double u, double v)
        {
            return InternalFace.Evaluate(new UV(u, v)).ToPoint();
        }

        public static DSFace FromExisting(Autodesk.Revit.DB.Face f)
        {
            return new DSFace(f);
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
