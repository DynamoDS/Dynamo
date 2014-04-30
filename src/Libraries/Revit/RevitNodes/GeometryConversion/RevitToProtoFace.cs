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
    [IsVisibleInDynamoLibrary(false)]
    [SupressImportIntoVM]
    public static class RevitToProtoFace
    {
        public static Surface ToProtoType(this Revit.GeometryObjects.Face revitFace)
        {
            if (revitFace == null) return null;

            return revitFace.InternalFace.ToProtoType();
        }

        public static Surface ToProtoType(this Autodesk.Revit.DB.Face face)
        {
            if (face == null) return null;

            dynamic dyFace = face;
            List<PolyCurve> edgeLoops = EdgeLoopsAsPolyCurves(dyFace);
            Surface untrimmedSrf = SurfaceExtractor.ExtractSurface(dyFace, edgeLoops);
            return untrimmedSrf != null ? untrimmedSrf.TrimWithEdgeLoops(edgeLoops.ToArray()) : null;
        }

        internal static List<PolyCurve> EdgeLoopsAsPolyCurves(Autodesk.Revit.DB.Face face)
        {
            return face.EdgeLoops.Cast<EdgeArray>()
                .Select(x => x.Cast<Autodesk.Revit.DB.Edge>())
                .Select(x => x.Select(t => t.AsCurveFollowingFace(face).ToProtoType()).ToArray())
                .Select(PolyCurve.ByJoinedCurves)
                .ToList();
        }

    }
}
