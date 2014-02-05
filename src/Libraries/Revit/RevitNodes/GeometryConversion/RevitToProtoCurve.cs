using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;

namespace Revit.GeometryConversion
{
    [Browsable(false)]
    public static class RevitToProtoCurve
    {

        /// <summary>
        /// An extension method to convert a Revit Curve to a ProtoGeometry Curve
        /// </summary>
        /// <param name="crv"></param>
        /// <returns></returns>
        public static Autodesk.DesignScript.Geometry.Curve ToProtoType(this Autodesk.Revit.DB.Curve crv)
        {
            dynamic dyCrv = crv;
            return RevitToProtoCurve.Convert(dyCrv);
        }


        /// <summary>
        /// Convert a Revit NurbSpline to a ProtoGeometry curve
        /// </summary>
        /// <param name="crv"></param>
        /// <returns></returns>
        private static Autodesk.DesignScript.Geometry.NurbsCurve Convert(Autodesk.Revit.DB.NurbSpline crv)
        {
            // TODO: there is no conversion routine for rational curves in the current ProtoGeometry interface
            if (crv.isRational)
            {
                throw new Exception("No conversion for rational NURBS curves");
            }

            return NurbsCurve.ByControlVertices(crv.CtrlPoints.Select(x => x.ToPoint()).ToArray(), crv.Degree);
        }

        /// <summary>
        /// Convert a Revit HermiteSpline to a ProtoGeometry BSplineCurve
        /// </summary>
        /// <param name="crv"></param>
        /// <returns></returns>
        private static Autodesk.DesignScript.Geometry.NurbsCurve Convert(Autodesk.Revit.DB.HermiteSpline crv)
        {
            return NurbsCurve.ByPointsTangents(crv.ControlPoints.Select(x => x.ToPoint()).ToArray(),
                crv.Tangents.First().ToVector(), crv.Tangents.Last().ToVector());
        }

        /// <summary>
        /// Convert a Revit Line to a ProtoGeometry Line
        /// </summary>
        /// <param name="crv"></param>
        /// <returns></returns>
        private static Autodesk.DesignScript.Geometry.Line Convert(Autodesk.Revit.DB.Line crv)
        {
            return Autodesk.DesignScript.Geometry.Line.ByStartPointEndPoint(
                 crv.get_EndPoint(0).ToPoint(), crv.get_EndPoint(1).ToPoint());
        }

        /// <summary>
        /// Convert a Revit Arc to a ProtoGeometry Arc
        /// </summary>
        /// <param name="crv"></param>
        /// <returns></returns>
        private static Autodesk.DesignScript.Geometry.Arc Convert(Autodesk.Revit.DB.Arc crv)
        {
            return Arc.ByThreePoints(crv.get_EndPoint(0).ToPoint(), crv.get_EndPoint(1).ToPoint(), crv.Evaluate(0.5, true).ToPoint());
        }

        /// <summary>
        /// Convert a Revit PolyLine to a degree 1 ProtoGeometry BSplineCurve
        /// </summary>
        /// <param name="crv"></param>
        /// <returns></returns>
        private static Autodesk.DesignScript.Geometry.NurbsCurve Convert(Autodesk.Revit.DB.PolyLine crv)
        {
            return
                Autodesk.DesignScript.Geometry.NurbsCurve.ByControlVertices(
                    crv.GetCoordinates().Select(x => x.ToPoint()).ToArray(), 1);
        }

        /// <summary>
        /// Convert a Revit Ellipse to a ProtoGeometry curve
        /// </summary>
        /// <param name="crv"></param>
        /// <returns></returns>
        private static Autodesk.DesignScript.Geometry.Curve Convert(Autodesk.Revit.DB.Ellipse crv)
        {
            var unitArc = Autodesk.DesignScript.Geometry.Arc.ByCenterPointRadiusAngle(crv.Center.ToPoint(), 1,
               crv.GetEndParameter(0), crv.GetEndParameter(1) - crv.GetEndParameter(0), crv.Normal.ToVector());

            var nonUniScale = CoordinateSystem.ByOriginVectors(unitArc.CenterPoint,
                unitArc.ContextCoordinateSystem.XAxis.Scale(crv.RadiusX),
                unitArc.ContextCoordinateSystem.YAxis.Scale(crv.RadiusY));

            var trf = (Curve) unitArc.Transform(unitArc.ContextCoordinateSystem, nonUniScale);

            return trf;
        }

        /// <summary>
        /// Convert a Revit Ellipse to a degree 1 ProtoGeometry BSplineCurve
        /// </summary>
        /// <param name="crv"></param>
        /// <returns></returns>
        private static Autodesk.DesignScript.Geometry.Curve Convert(Autodesk.Revit.DB.CylindricalHelix crv)
        {
            // for now, we omit the implementation until we have weights
            throw new NotImplementedException();

            var b = crv.BasePoint;
            var r = crv.Radius;
            var p = crv.Pitch;
            var z = crv.ZVector;
            var y = crv.YVector;
            var x = crv.XVector;
            var ir = crv.IsRightHanded;

        }
    }
}
