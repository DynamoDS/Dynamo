using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Dynamo.DSEngine;

using RevitServices.Elements;
using RevitServices.Persistence;

namespace Revit.GeometryConversion
{
    [IsVisibleInDynamoLibrary(false)]
    [SupressImportIntoVM]
    public static class ProtoToRevitMesh
    {
        public static ElementId StyleId { get; set; }
        public static ElementId MaterialId { get; set; }

        public static IList<GeometryObject> ToRevitType(this Autodesk.DesignScript.Geometry.Surface srf,
            bool performHostUnitConversion = true)
        {
            Element el;
            if (StyleId == null)
            {
                DocumentManager.Instance.CurrentDBDocument.TryGetElement(StyleId, out el);
                if (el == null)
                {
                   StyleId = FindDynamoGraphicsStyle(); 
                }
            }

            if (MaterialId == null)
            {
                DocumentManager.Instance.CurrentDBDocument.TryGetElement(StyleId, out el);
                if (el == null)
                {
                    MaterialId = FindDefaultRevitMaterial();
                }
            }

            srf = performHostUnitConversion ? srf.InHostUnits() : srf;

            var rp = new RenderPackage();
            srf.Tessellate(rp);

            var tsb = new TessellatedShapeBuilder();
            tsb.OpenConnectedFaceSet(false);

            var v = rp.TriangleVertices;

            for (int i = 0; i < v.Count; i += 9)
            {
                var a = new XYZ(v[i],       v[i + 1],   v[ i + 2]);
                var b = new XYZ(v[i + 3],   v[i + 4],   v[i + 5]);
                var c = new XYZ(v[i + 6],   v[i + 7],   v[i + 8]);

                var face = new TessellatedFace(new List<XYZ>(){a,b,c}, ElementId.InvalidElementId);
                tsb.AddFace(face);
            }

            tsb.CloseConnectedFaceSet();
            var result = tsb.Build(TessellatedShapeBuilderTarget.AnyGeometry, TessellatedShapeBuilderFallback.Mesh, ElementId.InvalidElementId);
            return result.GetGeometricalObjects();
        }

        private static ElementId FindDefaultRevitMaterial()
        {
            var fec = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument);
            fec.OfClass(typeof(Material));
            var materials = fec.ToElements();

            var defaultMat = materials.FirstOrDefault(mat => mat.Name == "Dynamo");
            if (defaultMat != null)
            {
                return defaultMat.Id;
            }

            defaultMat = materials.FirstOrDefault(mat => mat.Name == "Default");
            if (defaultMat != null)
            {
                return defaultMat.Id;
            }

            throw new Exception("The default material could not be found.");
        }

        private static ElementId FindDynamoGraphicsStyle()
        {
            var styles = new FilteredElementCollector(DocumentManager.Instance.CurrentUIDocument.Document);
            styles.OfClass(typeof(GraphicsStyle));

            var gStyle = styles.ToElements().FirstOrDefault(x => x.Name == "Dynamo");
            if (gStyle != null)
            {
                return gStyle.Id;
            }

            return ElementId.InvalidElementId;
        }
    }
}
