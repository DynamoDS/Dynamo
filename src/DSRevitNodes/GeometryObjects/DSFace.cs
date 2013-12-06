using System;
using Autodesk.DesignScript.Interfaces;
using Autodesk.Revit.DB;
using DSRevitNodes.GeometryObjects;
using DSRevitNodes.Graphics;

namespace DSRevitNodes.GeometryObjects
{
    public class DSFace : IGeometryObject
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

        #region Tesselation

        public void Tessellate(IRenderPackage package)
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
