using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using DynamoUnits;

namespace Revit.GeometryConversion
{
    [SupressImportIntoVM]
    public static class UnitConverter
    {
        public static double DynamoToHostFactor
        {
            get { return Length.FromDouble(1.0).ConvertToHostUnits(); }
        }

        public static double HostToDynamoFactor
        {
            get { return 1 / DynamoToHostFactor; }
        }

        /// <summary>
        /// Convert from Feet (Revit API internal units) to whatever the user facing units are
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public static T ConvertToDynamoUnits<T>(this T geometry)
            where T : Autodesk.DesignScript.Geometry.Geometry
        {
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            return (T)geometry.Scale(HostToDynamoFactor);
        }

        /// <summary>
        /// Convert from user facing units to Feet (Revit API internal units)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public static T ConvertToHostUnits<T>(this T geometry)
            where T : Autodesk.DesignScript.Geometry.Geometry
        {
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            return (T)geometry.Scale(DynamoToHostFactor);
        }
    }

}
