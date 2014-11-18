using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;

namespace Revit.GeometryConversion
{
    [SupressImportIntoVM]
    public static class RevitToProtoMesh
    {
        public static Autodesk.DesignScript.Geometry.Mesh ToProtoType(this Autodesk.Revit.DB.Mesh mesh, 
            bool performHostUnitConversion = true)
        {
            var pts = mesh.Vertices.Select(x => x.ToPoint(performHostUnitConversion));

            var tris = Enumerable.Range(0, mesh.NumTriangles)
                .Select(mesh.get_Triangle)
                .Select(tri => IndexGroup.ByIndices(tri.get_Index(0), tri.get_Index(1), tri.get_Index(2)));

            return Autodesk.DesignScript.Geometry.Mesh.ByPointsFaceIndices(pts, tris);

        }

        public static Autodesk.DesignScript.Geometry.Mesh[] ToProtoType(this Autodesk.Revit.DB.MeshArray meshArray,
            bool performHostUnitConversion = true)
        {
            return meshArray.Cast<Autodesk.Revit.DB.Mesh>().Select(x => x.ToProtoType(performHostUnitConversion)).ToArray();
        }
    }
}
