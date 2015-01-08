using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;

using Element = Revit.Elements.Element;
using Face = Autodesk.Revit.DB.Face;
using Surface = Autodesk.DesignScript.Geometry.Surface;

namespace Revit.GeometryConversion
{
#if DEBUG

    internal static class SolidDebugging
    {
        public static IEnumerable<Autodesk.Revit.DB.Solid> GetRevitSolids(Element ele)
        {
            return ele.InternalGeometry().OfType<Autodesk.Revit.DB.Solid>().ToArray();
        }

        public static IEnumerable<Surface> GetTrimmedSurfacesFromSolid(Autodesk.Revit.DB.Solid geom)
        {
            return geom.Faces.Cast<Autodesk.Revit.DB.Face>().SelectMany(x => x.ToProtoType(false));
        }

        public static IEnumerable<Surface> GetTrimmedSurfacesFromFace(Autodesk.Revit.DB.Face geom)
        {
            return geom.ToProtoType(false);
        }

        public static IEnumerable<Autodesk.Revit.DB.Face> GetRevitFaces(Autodesk.Revit.DB.Solid geom)
        {
            return geom.Faces.Cast<Autodesk.Revit.DB.Face>();
        }

        public static IEnumerable<IEnumerable<Autodesk.Revit.DB.Edge>> GetEdgeLoopsFromRevitFace(Autodesk.Revit.DB.Face face)
        {
            return face.EdgeLoops.Cast<EdgeArray>()
                .Select(x => x.Cast<Autodesk.Revit.DB.Edge>());
        }

        public static Surface GetUntrimmedSurfaceFromRevitFace(Face geom,
            IEnumerable<PolyCurve> edgeLoops)
        {
            var dyFace = (dynamic)geom;
            return (Surface)SurfaceExtractor.ExtractSurface(dyFace, edgeLoops);
        }

        public static List<PolyCurve> GetEdgeLoopsFromRevitFaceAsPolyCurves(Autodesk.Revit.DB.Face face)
        {
            return face.EdgeLoops.Cast<EdgeArray>()
                .Select(x => x.Cast<Autodesk.Revit.DB.Edge>())
                .Select(x => x.Select(t => t.AsCurveFollowingFace(face).ToProtoType(false)))
                .Select(PolyCurve.ByJoinedCurves)
                .ToList();
        }

    }

#endif
}
