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
using Surface = Autodesk.DesignScript.Geometry.Surface;

namespace Revit.GeometryConversion
{
    [SupressImportIntoVM]
    public static class RevitToProtoFace
    {
        public static IEnumerable<Surface> ToProtoType(this Autodesk.Revit.DB.Face revitFace,
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
                if (untrimmedSrf == null)
                {
                    edgeLoops.ForEach(x => x.Dispose());
                    edgeLoops.Clear();
                    throw new Exception("Failed to extract surface");
                }

                // trim the surface
                Surface converted;
                try
                {
                    converted = untrimmedSrf.TrimWithEdgeLoops(edgeLoops);
                }
                catch (Exception e)
                {
                    edgeLoops.ForEach(x => x.Dispose());
                    edgeLoops.Clear();
                    untrimmedSrf.Dispose();
                    throw e;
                }

                edgeLoops.ForEach(x => x.Dispose());
                edgeLoops.Clear();
                untrimmedSrf.Dispose();

                // perform unit conversion if necessary
                if (performHostUnitConversion)
                    UnitConverter.ConvertToDynamoUnits(ref converted);

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
            List<PolyCurve> result = new List<PolyCurve>();
            foreach (var edgeLoop in edgeLoops)
            {
                List<Autodesk.DesignScript.Geometry.Curve> curves = 
                    new List<Autodesk.DesignScript.Geometry.Curve>();
                foreach (var edge in edgeLoop)
                {
                    var dbCurve = edge.AsCurveFollowingFace(face);
                    curves.Add(dbCurve.ToProtoType(false));
                }
                result.Add(PolyCurve.ByJoinedCurves(curves));
                curves.ForEach(x => x.Dispose());
                curves.Clear();
            }
            return result;
        }

    }
}
