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
    public static class RevitToProtoFace
    {

        public static Autodesk.DesignScript.Geometry.Surface ToProtoType(this Autodesk.Revit.DB.Face crv)
        {
            dynamic dyCrv = crv;
            return RevitToProtoFace.Convert(dyCrv);
        }

        public static Autodesk.DesignScript.Geometry.Surface ConvertToProtoSurface(Revit.GeometryObjects.Face face)
        {
            if (face.InternalFace is PlanarFace)
            {
                return Convert(face.InternalFace as PlanarFace);
            }

            return null;
        }

        private static Autodesk.DesignScript.Geometry.Surface Convert(Autodesk.Revit.DB.PlanarFace face)
        {
            var edgeLoops = EdgeLoopPolyCurves(face).ToList();
            var untrimmedSrf = GetUnderlyingSurface(face, edgeLoops);
            return untrimmedSrf.TrimWithEdgeLoops(face, edgeLoops);
        }

        private static Autodesk.DesignScript.Geometry.Surface GetUnderlyingSurface(Autodesk.Revit.DB.PlanarFace face, 
            IEnumerable<PolyCurve> edgeLoops )
        {
            edgeLoops = edgeLoops.ToList();

            var x = face.get_Vector(0).ToVector();
            var y = face.get_Vector(1).ToVector();

            // Construct planar surface larger than the biggest edge loop
            var or = edgeLoops.First().StartPoint; // don't use origin provided by revit, could be anywhere
            var maxLength = edgeLoops.Max(pc => pc.Length);
            var bigx = x.Scale(maxLength * 2);
            var bigy = y.Scale(maxLength * 2);

            return Surface.ByPerimeterPoints(new[] { or.Subtract(bigx).Subtract(bigy), 
                                                                    or.Add(bigx).Subtract(bigy), 
                                                                    or.Add(bigx).Add(bigy),
                                                                    or.Subtract(bigx).Add(bigy) });
        }

        private static Surface TrimWithEdgeLoops(this Surface surface, Autodesk.Revit.DB.Face face,
            IEnumerable<PolyCurve> loops)
        {
            var cutSurface = surface;

            // now trim underlyingPlane using the pcLoops
            foreach (var pc in loops)
            {
                var subSurfaces = cutSurface.Split(pc).Cast<Surface>();

                foreach (var srf in subSurfaces)
                {
                    if (IsValidTrim(pc, srf, face))
                    {
                        cutSurface = srf;
                        break;
                    }
                }
            }

            return cutSurface;
        }

        private static IEnumerable<PolyCurve> EdgeLoopPolyCurves(Autodesk.Revit.DB.Face face)
        {
            foreach (var ea in face.EdgeLoops.Cast<EdgeArray>())
            {
                var edges = ea.Cast<Autodesk.Revit.DB.Edge>();
                var pcrvs = edges.Select(t => t.AsCurveFollowingFace(face).ToProtoType()).ToArray();
                yield return PolyCurve.ByJoinedCurves(pcrvs);
            }
        }

        private static bool IsValidTrim(PolyCurve pc, Surface srf, Autodesk.Revit.DB.Face face)
        {
            var pts = pc.DivideByLength(pc.NumberOfCurves * 2 + 1);

            var ptsLeft = new List<Autodesk.DesignScript.Geometry.Point>();
            var ptsRight = new List<Autodesk.DesignScript.Geometry.Point>();

            foreach (var pt in pts)
            {
                var srfnorm = srf.NormalAtPoint(pt).Normalized();
                var crvtan = pc.TangentAtPoint(pt).Normalized();

                var offsetDir = crvtan.Cross(srfnorm).Scale(0.001);
                var revoffsetDir = offsetDir.Reverse().Scale(0.001);

                ptsLeft.Add(pt.Add(offsetDir));
                ptsRight.Add(pt.Add(revoffsetDir));
            }

            // A valid trim will have all of these points entirely inside the face and the surface
            return (face.AllPointsAreInside(ptsLeft) || face.AllPointsAreInside(ptsRight))
                   && (srf.AllPointsAreInside(ptsLeft) || srf.AllPointsAreInside(ptsRight));
        }

        private static bool AllPointsAreInside(this Autodesk.Revit.DB.Face face, IEnumerable<Autodesk.DesignScript.Geometry.Point> pts)
        {
            return pts.All(x =>
            {
                var proj = face.Project(x.ToXyz());
                if (proj == null) return false;

                return face.IsInside(proj.UVPoint);
            });
        }

        private static bool AllPointsAreInside(this Autodesk.DesignScript.Geometry.Surface surface, IEnumerable<Autodesk.DesignScript.Geometry.Point> pts, double tolerance = 1e-5)
        {
            return pts.All(x => surface.DistanceTo(x) < tolerance);
        }

        private static Vector TangentAtPoint(this PolyCurve pc, Autodesk.DesignScript.Geometry.Point pt)
        {
            Vector vec = null;
            foreach (var crv in pc.Curves())
            {
                var closestPt = crv.GetClosestPoint(pt);

                if (pt.DistanceTo(closestPt) > 1e-6) continue;

                var u = crv.ParameterAtPoint(closestPt);
                vec = crv.TangentAtParameter(u);
            }

            return vec;
        }

    }
}
