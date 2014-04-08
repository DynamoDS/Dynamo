using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Runtime;
using Microsoft.CSharp.RuntimeBinder;

namespace Revit.GeometryObjects
{
    [IsVisibleInDynamoLibrary(false)]
    public static class GeometryObjectWrapper
    {
        /// <summary>
        /// If possible, wrap the geometry object in a DS type.
        /// </summary>
        /// <param name="geom"></param>
        /// <returns></returns>
        public static GeometryObject Wrap(this Autodesk.Revit.DB.GeometryObject geom)
        {
            if (geom == null) return null;

            dynamic dynGeom = geom;
            try
            {
                return InternalWrap(dynGeom);
            }
            catch (RuntimeBinderException e)
            {
                // There's no InternalWrap method
                return null;
            }
        }

        private static GeometryObjects.Face InternalWrap(Autodesk.Revit.DB.Face geom)
        {
            return Face.FromExisting(geom);
        }

        private static GeometryObjects.Solid InternalWrap(Autodesk.Revit.DB.Solid geom)
        {
            return Solid.FromExisting(geom);
        }

        private static GeometryObjects.Edge InternalWrap(Autodesk.Revit.DB.Edge geom)
        {
            return Edge.FromExisting(geom);
        }

    }
}
