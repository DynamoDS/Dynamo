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

using Edge = Autodesk.Revit.DB.Edge;
using Face = Autodesk.Revit.DB.Face;

namespace Revit.GeometryConversion
{
    internal static class RevitToProtoFace
    {
        internal static IEnumerable<Surface> ToProtoType(this Autodesk.Revit.DB.Face revitFace,
          bool performHostUnitConversion = true, Reference referenceOverride = null)
        {
            if (revitFace == null) throw new ArgumentNullException("revitFace");

            var revitEdgeLoops = EdgeLoopPartition.GetAllEdgeLoopsFromRevitFace(revitFace);
            var partitionedRevitEdgeLoops = EdgeLoopPartition.ByEdgeLoopsAndFace(revitFace, revitEdgeLoops);

            var listSurface = new List<Surface>();

            foreach (var edgeloopPartition in partitionedRevitEdgeLoops)
            {
                // convert the trimming curves
                var edgeLoops = EdgeLoopsAsPolyCurves(revitFace, edgeloopPartition);

                // convert the underrlying surface
                var dyFace = (dynamic)revitFace;
                Surface untrimmedSrf = SurfaceExtractor.ExtractSurface(dyFace, edgeLoops);
                if (untrimmedSrf == null) throw new Exception("Failed to extract surface");

                // trim the surface
                Surface converted = untrimmedSrf.TrimWithEdgeLoops(edgeLoops);

                // perform unit conversion if necessary
                converted = performHostUnitConversion ? converted.InDynamoUnits() : converted;

                // if possible, apply revit reference
                var revitRef = referenceOverride ?? revitFace.Reference;
                if (revitRef != null) converted = ElementFaceReference.AddTag(converted, revitRef);

                listSurface.Add(converted);
            }

            return listSurface;
        }

        private static List<PolyCurve> EdgeLoopsAsPolyCurves(Face face, 
            IEnumerable<IEnumerable<Edge>> edgeLoops)
        {
            return edgeLoops
                .Select(x => x.Select(t => t.AsCurveFollowingFace(face).ToProtoType(false)))
                .Select(PolyCurve.ByJoinedCurves)
                .ToList();
        }

    }
}
