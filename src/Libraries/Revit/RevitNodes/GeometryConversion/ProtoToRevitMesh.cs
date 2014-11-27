using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Dynamo.DSEngine;

using RevitServices.Materials;
using RevitServices.Persistence;

namespace Revit.GeometryConversion
{
    [IsVisibleInDynamoLibrary(false)]
    [SupressImportIntoVM]
    public static class ProtoToRevitMesh
    {
        public static IList<GeometryObject> ToRevitType(this Autodesk.DesignScript.Geometry.Surface srf,
            bool performHostUnitConversion = true)
        {
            var rp = new RenderPackage();
            if (performHostUnitConversion)
            {
                var newSrf = srf.InHostUnits();
                newSrf.Tessellate(rp);
                newSrf.Dispose();
            }
            else
            {
                srf.Tessellate(rp);
            }

            var tsb = new TessellatedShapeBuilder();
            tsb.OpenConnectedFaceSet(false);

            var v = rp.TriangleVertices;

            for (int i = 0; i < v.Count; i += 9)
            {
                var a = new XYZ(v[i],       v[i + 1],   v[ i + 2]);
                var b = new XYZ(v[i + 3],   v[i + 4],   v[i + 5]);
                var c = new XYZ(v[i + 6],   v[i + 7],   v[i + 8]);

                var face = new TessellatedFace(new List<XYZ>(){a,b,c}, MaterialsManager.Instance.DynamoMaterialId);
                tsb.AddFace(face);
            }

            tsb.CloseConnectedFaceSet();

            var result = tsb.Build(TessellatedShapeBuilderTarget.Mesh, TessellatedShapeBuilderFallback.Salvage, ElementId.InvalidElementId);
            return result.GetGeometricalObjects();
        }

        public static IList<GeometryObject> ToRevitType(
            this Autodesk.DesignScript.Geometry.Solid solid, bool performHostUnitConversion = true)
        {
            var rp = new RenderPackage();
            if (performHostUnitConversion)
            {
                var newSolid = solid.InHostUnits();
                newSolid.Tessellate(rp);
                newSolid.Dispose();
            }
            else
            {
                solid.Tessellate(rp);
            }

            var tsb = new TessellatedShapeBuilder();
            tsb.OpenConnectedFaceSet(false);

            var v = rp.TriangleVertices;

            for (int i = 0; i < v.Count; i += 9)
            {
                var a = new XYZ(v[i], v[i + 1], v[i + 2]);
                var b = new XYZ(v[i + 3], v[i + 4], v[i + 5]);
                var c = new XYZ(v[i + 6], v[i + 7], v[i + 8]);

                var face = new TessellatedFace(new List<XYZ>() { a, b, c }, MaterialsManager.Instance.DynamoMaterialId);
                tsb.AddFace(face);
            }

            tsb.CloseConnectedFaceSet();
            var result = tsb.Build(TessellatedShapeBuilderTarget.Mesh, TessellatedShapeBuilderFallback.Salvage, ElementId.InvalidElementId);
            return result.GetGeometricalObjects();
        }
    }
}
