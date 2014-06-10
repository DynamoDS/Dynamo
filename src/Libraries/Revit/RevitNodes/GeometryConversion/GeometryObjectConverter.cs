﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Microsoft.CSharp.RuntimeBinder;
using Revit.GeometryConversion;
using Revit.GeometryReferences;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace Revit.GeometryConversion
{
    [IsVisibleInDynamoLibrary(false)]
    public static class GeometryObjectConverter
    {
        /// <summary>
        /// Convert a GeometryObject to an applicable ProtoGeometry type.
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="reference"></param>
        /// <returns>A Geometry type.  Null if there's no suitable conversion.</returns>
        public static object Convert(this Autodesk.Revit.DB.GeometryObject geom, Autodesk.Revit.DB.Reference reference = null)
        {
            if (geom == null) return null;

            dynamic dynGeom = geom;
            try
            {
                return Tag( InternalConvert(dynGeom), reference);
            }
            catch (RuntimeBinderException e)
            {
                return null; 
            }

        }

        #region Tagging

        private static Autodesk.DesignScript.Geometry.Curve Tag(Autodesk.DesignScript.Geometry.Curve curve,
            Autodesk.Revit.DB.Reference reference)
        {
            return reference != null ? ElementCurveReference.AddTag(curve, reference) : curve;
        }

        private static Autodesk.DesignScript.Geometry.Surface Tag(Autodesk.DesignScript.Geometry.Surface srf,
    Autodesk.Revit.DB.Reference reference)
        {
            return reference != null ? ElementFaceReference.AddTag(srf, reference) : srf;
        }

        private static Autodesk.DesignScript.Geometry.Geometry Tag(Autodesk.DesignScript.Geometry.Geometry geo,
    Autodesk.Revit.DB.Reference reference)
        {
            return geo;
        }

        private static object Tag(object obj, Autodesk.Revit.DB.Reference reference)
        {
            return obj;
        }

        #endregion

        #region Converter methods

        private static Autodesk.DesignScript.Geometry.Curve InternalConvert(Autodesk.Revit.DB.Edge geom)
        {
            return (Autodesk.DesignScript.Geometry.Curve) geom.AsCurve().Convert();
        }

        private static Autodesk.DesignScript.Geometry.Surface InternalConvert(Autodesk.Revit.DB.Face geom)
        {
            return geom.ToProtoType();
        }

        private static Autodesk.DesignScript.Geometry.Solid InternalConvert(Autodesk.Revit.DB.Solid geom)
        {
            return geom.ToProtoType();
        }

        private static Autodesk.DesignScript.Geometry.Point InternalConvert(Autodesk.Revit.DB.Point geom)
        {
            return geom.ToProtoType();
        }

        private static Autodesk.DesignScript.Geometry.Curve InternalConvert(Autodesk.Revit.DB.Curve geom)
        {
            return geom.ToProtoType();
        }

        public static Autodesk.DesignScript.Geometry.PolyCurve InternalConvert(Autodesk.Revit.DB.PolyLine geom)
        {
            return geom.ToProtoType();
        }

        private static Autodesk.DesignScript.Geometry.PolyCurve InternalConvert(Autodesk.Revit.DB.Profile geom)
        {
            return geom.Curves.ToProtoType();
        }

        private static Autodesk.DesignScript.Geometry.Mesh InternalConvert(Autodesk.Revit.DB.Mesh geom)
        {
            return geom.ToProtoType();
        }

        #endregion

    }
}
