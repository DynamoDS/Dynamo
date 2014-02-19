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
        private static Autodesk.Revit.DB.Curve Convert(Autodesk.DesignScript.Geometry.NurbsCurve crv)
        {
            // TODO PB: degree elevation algorithm

            if (crv.Degree < 2)
            {
                throw new Exception("No conversion");
            }

            // presumably checking if the curve is circular is quite expensive, we don't do it
            return Autodesk.Revit.DB.NurbSpline.Create(crv.ControlPoints().ToXyzs(),
               Enumerable.Repeat(1.0, crv.ControlPoints().Count()).ToList(),
                crv.Knots(),
                crv.Degree,
                crv.IsClosed,
                false);
        }

        /// <summary>
        /// Convert a DS Arc to a Revit arc
        /// </summary>
        /// <param name="arc"></param>
        /// <returns></returns>
        private static Autodesk.Revit.DB.Arc Convert(Autodesk.DesignScript.Geometry.Arc arc)
        {
            // This is not the way to do this, but ProtoGeometry is so broken it's the only thing that works
            var p0 = arc.PointAtParameter(0);
            var p05 = arc.PointAtParameter(0.5);
            var p1 = arc.PointAtParameter(1);

            return Autodesk.Revit.DB.Arc.Create(p0.ToXyz(), p1.ToXyz(), p05.ToXyz());

            //var center = arc.CenterPoint;
            //var xaxis = arc.ContextCoordinateSystem.XAxis;
            //var yaxis = arc.ContextCoordinateSystem.YAxis;
            //var plane = new Autodesk.Revit.DB.Plane(xaxis.ToXyz(), yaxis.ToXyz(), center.ToXyz());

            //return Autodesk.Revit.DB.Arc.Create(plane, arc.Radius, arc.StartAngle, arc.EndAngle);
        }

        /// <summary>
        /// Convert a DS Circle to a Revit Arc
        /// </summary>
        /// <param name="circ"></param>
        /// <returns></returns>
        private static Autodesk.Revit.DB.Arc Convert(Autodesk.DesignScript.Geometry.Circle circ)
        {
            // This is a mess, but ProtoGeometry is so broken it's the only thing that works
            var p0 = circ.PointAtParameter(0);
            var p075 = circ.PointAtParameter(0.75);
            var p025 = circ.PointAtParameter(0.25);
            var p05 = circ.PointAtParameter(0.5);

            var xaxis = Vector.ByTwoPoints(p0, p05).Normalized();
            var yaxis = Vector.ByTwoPoints(p025, p075).Normalized();

            return Autodesk.Revit.DB.Arc.Create(circ.CenterPoint.ToXyz(), circ.Radius, 0, 2*Math.PI, xaxis.ToXyz(),
                yaxis.ToXyz());

            //var center = circ.CenterPoint;
            //var xaxis = circ.ContextCoordinateSystem.XAxis;
            //var yaxis = circ.ContextCoordinateSystem.YAxis;
            //var plane = new Autodesk.Revit.DB.Plane(xaxis.ToXyz(), yaxis.ToXyz(), center.ToXyz());
            return Autodesk.Revit.DB.Arc.Create(circ.PointAtParameter(0.0).ToXyz(),
                circ.PointAtParameter(0.5).ToXyz(), circ.PointAtParameter(1.0).ToXyz());
            
            //return Autodesk.Revit.DB.Arc.Create(plane, circ.Radius, 0, 2*System.Math.PI);
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

    }
}
