using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Microsoft.CSharp.RuntimeBinder;
using Revit.GeometryConversion;
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
        /// <returns>A Geometry type.  Null if there's no suitable conversion.</returns>
        public static Autodesk.DesignScript.Geometry.Geometry Convert(this Autodesk.Revit.DB.GeometryObject geom)
        {
            if (geom == null) return null;

            dynamic dynGeom = geom;
            try
            {
                return InternalConvert(dynGeom);
            }
            catch (RuntimeBinderException e)
            {
                // There's no InternalConvert method
                return null;
            }
        }

        private static Autodesk.DesignScript.Geometry.Point InternalConvert(Autodesk.Revit.DB.Point geom)
        {
            return Point.ByCoordinates(geom.Coord.X, geom.Coord.Y, geom.Coord.Z);
        }

        private static Autodesk.DesignScript.Geometry.Curve InternalConvert(Autodesk.Revit.DB.Curve geom)
        {
            return geom.ToProtoType();
        }

        private static Autodesk.DesignScript.Geometry.PolyCurve InternalConvert(Autodesk.Revit.DB.PolyLine geom)
        {
            return PolyCurve.ByPoints(geom.GetCoordinates().Select(x => Point.ByCoordinates(x.X, x.Y, x.Z)).ToArray());
        }

        private static Autodesk.DesignScript.Geometry.PolyCurve InternalConvert(Autodesk.Revit.DB.Profile geom)
        {
            return geom.Curves.ToProtoTypes();
        }

        private static Autodesk.DesignScript.Geometry.Mesh InternalConvert(Autodesk.Revit.DB.Mesh geom)
        {
            return geom.ToProtoType();
        }

    }
}
