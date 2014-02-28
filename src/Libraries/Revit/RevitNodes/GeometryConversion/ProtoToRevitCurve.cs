using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

namespace Revit.GeometryConversion
{
    [Browsable(false)]
    public static class ProtoToRevitCurve
    {

        /// <summary>
        /// An extension method for DesignScript.Geometry.Curve to convert to the analogous revit type
        /// </summary>
        /// <param name="crv"></param>
        /// <returns></returns>
        public static Autodesk.Revit.DB.Curve ToRevitType(this Autodesk.DesignScript.Geometry.Curve crv)
        {
            dynamic dyCrv = crv;

            return ProtoToRevitCurve.Convert(dyCrv);

        }

        /// <summary>
        /// Convert a DS BSplineCurve to a Revit NurbSpline
        /// </summary>
        /// <param name="crv"></param>
        /// <returns></returns>
        private static Autodesk.Revit.DB.NurbSpline Convert(Autodesk.DesignScript.Geometry.NurbsCurve crv)
        {
            if (crv.Degree <= 2)
            {
                throw new Exception("Could not convert the curve to a Revit curve");
            }

            // presumably checking if the curve is circular is quite expensive, we don't do it
            return Autodesk.Revit.DB.NurbSpline.Create(crv.ControlPoints().ToXyzs(),
                crv.Weights(),
                crv.Knots(),
                crv.Degree,
                crv.IsClosed,
                crv.IsRational );
        }

        /// <summary>
        /// Convert a DS Arc to a Revit arc
        /// </summary>
        /// <param name="arc"></param>
        /// <returns></returns>
        private static Autodesk.Revit.DB.Arc Convert(Autodesk.DesignScript.Geometry.Arc arc)
        {
            var center = arc.CenterPoint.ToXyz();
            var n = arc.Normal.ToXyz();
            n.Normalize();
            var sp = arc.StartPoint.ToXyz();
            var x = sp - center;
            x.Normalize();
            var y = n.CrossProduct(x);

            var plane = new Autodesk.Revit.DB.Plane(x, y, center);
            return Autodesk.Revit.DB.Arc.Create(plane, arc.Radius, 0, arc.SweepAngle.ToRadians());
        }

        /// <summary>
        /// Convert a DS Circle to a Revit Arc
        /// </summary>
        /// <param name="circ"></param>
        /// <returns></returns>
        private static Autodesk.Revit.DB.Arc Convert(Autodesk.DesignScript.Geometry.Circle circ)
        {
            var center = circ.CenterPoint.ToXyz();
            var n = circ.Normal.ToXyz();
            n.Normalize();
            var sp = circ.StartPoint.ToXyz();
            var x = sp - center;
            x.Normalize();
            var y = n.CrossProduct(x);

            var plane = new Autodesk.Revit.DB.Plane(x, y, center);
            return Autodesk.Revit.DB.Arc.Create(plane, circ.Radius, 0, 2 * System.Math.PI);
        }

        /// <summary>
        /// Convert a DS Line to a Revit line
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private static Autodesk.Revit.DB.Line Convert(Autodesk.DesignScript.Geometry.Line line)
        {
            return Autodesk.Revit.DB.Line.CreateBound(line.StartPoint.ToXyz(), line.EndPoint.ToXyz());
        }

        /// <summary>
        /// Convert a DS Helix to a Revit Helix
        /// </summary>
        /// <param name="crv"></param>
        /// <returns></returns>
        private static Autodesk.Revit.DB.CylindricalHelix Convert(Autodesk.DesignScript.Geometry.Helix crv)
        {
            var sp = crv.StartPoint.ToXyz();
            var ap = crv.AxisPoint.ToXyz();
            var ad = crv.AxisDirection.ToXyz();
            ad.Normalize();
            var x = sp - ap;
            x.Normalize();
            var p = crv.Pitch;
            var a = crv.Angle.ToRadians();

            return Autodesk.Revit.DB.CylindricalHelix.Create(sp, crv.Radius, x, ad, p, 0, a);
        }

        /// <summary>
        /// Convert a DS Ellipse to a Revit ellipse
        /// </summary>
        /// <param name="crv"></param>
        /// <returns></returns>
        private static Autodesk.Revit.DB.Ellipse Convert(Autodesk.DesignScript.Geometry.Ellipse crv)
        {
            var center = crv.CenterPoint.ToXyz();
            var x = crv.MajorAxis.ToXyz();
            x.Normalize();
            var y = crv.MinorAxis.ToXyz();
            y.Normalize();
            var xw = crv.MajorAxis.Length;
            var yw = crv.MinorAxis.Length;

            return Autodesk.Revit.DB.Ellipse.Create(center, xw, yw, x, y, 0, 2*Math.PI);
        }

    }
}
