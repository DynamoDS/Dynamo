using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;

namespace Revit.GeometryConversion
{
    [SupressImportIntoVM]
    [IsVisibleInDynamoLibrary(false)]
    public static class SurfaceTrimmer
    {
        public static Surface TrimWithEdgeLoops(Surface surface, Autodesk.Revit.DB.Face face,
            IEnumerable<PolyCurve> loops)
        {
            var cutSurface = surface;

            // now trim underlyingPlane using the pcLoops
            foreach (var pc in loops)
            {
                var intersectingCurves = cutSurface.Intersect(pc)
                    .Where(x => x is Curve).Cast<Curve>().ToArray();

                var pcIntersecting = PolyCurve.ByJoinedCurves(intersectingCurves);
                var subSurfaces = cutSurface.Split(pcIntersecting).Cast<Surface>();

                foreach (var srf in subSurfaces)
                {
                    if (TrimValidator.IsValidTrim(pc, srf, face))
                    {
                        cutSurface = srf;
                        break;
                    }
                }
            }

            return cutSurface;
        }

    }
}
