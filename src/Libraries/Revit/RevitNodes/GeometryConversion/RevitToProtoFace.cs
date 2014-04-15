using System;
using System.CodeDom;
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
    [SupressImportIntoVM]
    public static class RevitToProtoFace
    {

        public static Autodesk.DesignScript.Geometry.Surface ToProtoType(this Autodesk.Revit.DB.Face crv)
        {
            dynamic dyCrv = crv;
            return RevitToProtoFace.Convert(dyCrv);
        }

        private static Autodesk.DesignScript.Geometry.Surface Convert(Autodesk.Revit.DB.PlanarFace face)
        {
            // get underlying planar representation
            var o = face.Origin.ToPoint();
            var n = face.Normal.ToVector();
            var x = face.get_Vector(0).ToVector();
            var y = face.get_Vector(1).ToVector();

            var pl = Autodesk.DesignScript.Geometry.Plane.ByOriginXAxisYAxis(o, x, y);
           
            // get trimming curves as polycurves
            EdgeArrayArray eaa = face.EdgeLoops;
            var pcLoops = new List<PolyCurve>();

            foreach (var ea in eaa.Cast<EdgeArray>())
            {
                var edges = ea.Cast<Autodesk.Revit.DB.Edge>();
                var pcrvs = edges.Select(t => t.AsCurveFollowingFace(face).ToProtoType()).ToArray();
                pcLoops.Add(PolyCurve.ByJoinedCurves(pcrvs));
            }

            var bb = BoundingBox.ByGeometryCoordinateSystem(pcLoops.ToArray(), CoordinateSystem.ByOriginVectors(o, x, y));

            var pmax = (Autodesk.DesignScript.Geometry.Point) bb.MaxPoint.Project(pl, n)[0];
            var pmin = (Autodesk.DesignScript.Geometry.Point) bb.MinPoint.Project(pl, n)[0];
            
            // construct rectangle

            var v = pmin.Subtract(pmax.AsVector()).AsVector();

            var xl = x.Scale(v.Dot(x));
            var yl = y.Scale(v.Dot(y));

            var boundingRec = Rectangle.ByCornerPoints(pmin, pmin.Add(xl), pmax, pmin.Add(yl));
            var underlyingPlane = boundingRec.Patch()[0];

            // now trim underlyingPlane using the pcLoops
            foreach (var pc in pcLoops)
            {

                //underlyingPlane.Trim();

            }

            return null;

        }

    }
}
