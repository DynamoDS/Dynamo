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
using Revit.GeometryReferences;

namespace Revit.GeometryConversion
{
    [IsVisibleInDynamoLibrary(false)]
    [SupressImportIntoVM]
    public static class RevitToProtoFace
    {
        public static Surface ToProtoType(this Autodesk.Revit.DB.Face revitFace, 
            bool performHostUnitConversion = true)
        {
            if (revitFace == null) return null;

            var dyFace = (dynamic) revitFace;
            List<PolyCurve> edgeLoops = EdgeLoopsAsPolyCurves(dyFace);
            Surface untrimmedSrf = SurfaceExtractor.ExtractSurface(dyFace, edgeLoops);
            var converted = untrimmedSrf != null ? untrimmedSrf.TrimWithEdgeLoops(edgeLoops.ToArray()) : null;

            if (converted == null) return null;

            converted = performHostUnitConversion ? converted.InDynamoUnits() : converted;

            var revitRef = revitFace.Reference;
            return revitRef != null ? ElementFaceReference.AddTag(converted, revitRef) : converted;
        }

        internal static List<PolyCurve> EdgeLoopsAsPolyCurves(Autodesk.Revit.DB.Face face)
        {
            return face.EdgeLoops.Cast<EdgeArray>()
                .Select(x => x.Cast<Autodesk.Revit.DB.Edge>())
                .Select(x => x.Select(t => t.AsCurveFollowingFace(face).ToProtoType(false)).ToArray())
                .Select(PolyCurve.ByJoinedCurves)
                .ToList();
        }
    }
}
