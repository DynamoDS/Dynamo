using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Runtime;

namespace Revit.GeometryConversion
{
    [SupressImportIntoVM]
    public static class UnitConversion
    {
        public static double ProtoToHostFactor = 1;
        public static double HostToProtoFactor = 1;

        /// <summary>
        /// Convert from Feet (Revit API internal units) to whatever the user facing units are
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public static T ConvertToUserFacingUnits<T>(this T geometry)
            where T : Autodesk.DesignScript.Geometry.Geometry
        {
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            return (T)geometry.Scale(ProtoToHostFactor);
        }

        /// <summary>
        /// Convert from user facing units to Feet (Revit API internal units)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public static T ConvertToRevitInternalUnits<T>(this T geometry)
            where T : Autodesk.DesignScript.Geometry.Geometry
        {
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            return (T)geometry.Scale(HostToProtoFactor);
        }
    }

}
