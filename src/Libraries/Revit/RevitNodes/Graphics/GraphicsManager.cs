using System.ComponentModel;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

namespace Revit.Graphics
{
    [SupressImportIntoVM]
    public class GraphicsManager
    {
        /// <summary>
        /// Defines the global level of detail setting for 
        /// object tesselation
        /// </summary>
        private static double tesselationLevelOfDetail = 1.0;
        public static double TesselationLevelOfDetail
        {
            get
            {
                return tesselationLevelOfDetail;
            }
            set
            {
                if (value < 0)
                {
                    tesselationLevelOfDetail = 0;
                    return;
                }

                if (value > 1)
                {
                    tesselationLevelOfDetail = 1;
                    return;
                }

                tesselationLevelOfDetail = value;
            }
        }

        public static void PushMesh(Autodesk.Revit.DB.Mesh mesh, IRenderPackage package)
        {
            for (var i = 0; i < mesh.NumTriangles; i++)
            {
                var triangle = mesh.get_Triangle(i);
                for (var j = 0; j < 3; j++)
                {
                    var xyz = triangle.get_Vertex(j);
                    package.PushTriangleVertex(xyz.X, xyz.Y, xyz.Z);
                }

                var a = mesh.get_Triangle(i).get_Vertex(1).Subtract(mesh.get_Triangle(i).get_Vertex(0)).Normalize();
                var b = mesh.get_Triangle(i).get_Vertex(2).Subtract(mesh.get_Triangle(i).get_Vertex(0)).Normalize();
                var norm = a.CrossProduct(b);
                package.PushTriangleVertexNormal(norm.X, norm.Y, norm.Z);
                package.PushTriangleVertexNormal(norm.X, norm.Y, norm.Z);
                package.PushTriangleVertexNormal(norm.X, norm.Y, norm.Z);
            }
        }
    }
}
