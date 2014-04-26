using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;

namespace Revit.GeometryConversion
{
    public static class RevitToProtoFace
    {
        // PB: should be phased out eventually
        public static Surface ToSurface(this Revit.GeometryObjects.Face revitFace)
        {
            if (revitFace == null) return null;

            return revitFace.InternalFace.ToSurface();
        }

        public static Surface ToSurface(this Autodesk.Revit.DB.Face face)
        {
            if (face == null) return null;

            dynamic dyFace = face;
            List<PolyCurve> edgeLoops = EdgeLoopsAsPolyCurves(dyFace);
            Surface untrimmedSrf = SurfaceExtractor.ExtractSurface(dyFace, edgeLoops);
            return untrimmedSrf != null ? untrimmedSrf.TrimWithEdgeLoops(edgeLoops.ToArray()) : null;
        }

        public static Surface ToUntrimmedSurface(this Revit.GeometryObjects.Face revitFace)
        {
            if (revitFace == null) return null;

            return revitFace.InternalFace.ToUntrimmedSurface();
        }

        public static Surface ToUntrimmedSurface(this Autodesk.Revit.DB.Face face)
        {
            if (face == null) return null;

            dynamic dyFace = face;
            List<PolyCurve> edgeLoops = EdgeLoopsAsPolyCurves(dyFace);
            return SurfaceExtractor.ExtractSurface(dyFace, edgeLoops);
        }

        public static Autodesk.DesignScript.Geometry.Solid ToSolid(Surface[] surfaces)
        {
            return Autodesk.DesignScript.Geometry.Solid.ByJoinedSurfaces(surfaces);
        }

        public static PolyCurve[] EdgeLoops(this Revit.GeometryObjects.Face revitFace)
        {
            if (revitFace == null) return null;

            return revitFace.InternalFace.EdgeLoops();
        }

        public static PolyCurve[] EdgeLoops(this Autodesk.Revit.DB.Face face)
        {
            if (face == null) return null;

            return EdgeLoopsAsPolyCurves(face).ToArray();
        }

        private static List<PolyCurve> EdgeLoopsAsPolyCurves(Autodesk.Revit.DB.Face face)
        {
            return face.EdgeLoops.Cast<EdgeArray>()
                .Select(x => x.Cast<Autodesk.Revit.DB.Edge>())
                .Select(x => x.Select(t => t.AsCurveFollowingFace(face).ToProtoType()).ToArray())
                .Select(PolyCurve.ByJoinedCurves)
                .ToList();
        }

    }
}
