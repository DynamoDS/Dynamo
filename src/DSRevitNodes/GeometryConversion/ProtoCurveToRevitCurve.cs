﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB.Structure;

namespace DSRevitNodes.GeometryConversion
{
    [Browsable(false)]
    internal static class ProtoCurveToRevitCurve
    {

        /// <summary>
        /// An extension method for DesignScript.Geometry.Curve to convert to the analogous revit type
        /// </summary>
        /// <param name="crv"></param>
        /// <returns></returns>
        internal static Autodesk.Revit.DB.Curve ToRevitType(this Autodesk.DesignScript.Geometry.Curve crv)
        {
            dynamic dyCrv = crv;
            return ProtoCurveToRevitCurve.Convert(dyCrv);
        }

        /// <summary>
        /// Convert a DS BSplineCurve to a Revit NurbSpline
        /// </summary>
        /// <param name="crv"></param>
        /// <returns></returns>
        private static Autodesk.Revit.DB.Curve Convert(Autodesk.DesignScript.Geometry.BSplineCurve crv)
        {
            // TODO: degree elevation algorithm

            if (crv.Degree < 2)
            {
                throw new Exception("No conversion");
            }

            // presumably checking if the curve is circular is quite expensive, we don't do it

            return Autodesk.Revit.DB.NurbSpline.Create(crv.ControlVertices.ToXyzs(),
                Enumerable.Repeat( 1.0, crv.ControlVertices.Count() ).ToList(),
                crv.Knots.ToList(),
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Convert a DS Circle to a Revit Arc
        /// </summary>
        /// <param name="arc"></param>
        /// <returns></returns>
        private static Autodesk.Revit.DB.Arc Convert(Autodesk.DesignScript.Geometry.Circle arc)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Convert a DS Line to a Revit line
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private static Autodesk.Revit.DB.Line Convert(Autodesk.DesignScript.Geometry.Line line)
        {
            throw new NotImplementedException();
        }


    }
}
