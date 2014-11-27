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
        /// Convert from Revit API internal units (feet) to Dynamo internal units (meters) 
        /// 
        /// Can be used simply as geometry.InDynamoUnits() as the type is constrained
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public static T InDynamoUnits<T>(this T geometry)
            where T : Autodesk.DesignScript.Geometry.Geometry
        {
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            var result = (T)geometry.Scale(HostToDynamoFactor);
            return result;
        }

        /// <summary>
        /// Convert from Dynamo internal units (meters) to Revit API internal units (feet)
        /// 
        /// Can be used simply as geometry.InHostUnits()
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public static T InHostUnits<T>(this T geometry)
            where T : Autodesk.DesignScript.Geometry.Geometry
        {
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            var result = (T)geometry.Scale(DynamoToHostFactor);
            return result;
        }
        
        /// <summary>
        /// Convert the geometry to Dynamo units if convert is true.
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="convert"></param>
        /// <returns></returns>
        public static T ConvertToDynamoUnits<T>(ref T geometry, bool convert = true)
            where T : Autodesk.DesignScript.Geometry.Geometry
        {
            if (convert)
            {
                var result = geometry.InDynamoUnits();
                geometry.Dispose();
                geometry = result;
                return result;
            }
            else
            {
                return geometry;
            }
        }

        /// <summary>
        /// Convert the geometry to host units if convert is true.
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public static T ConvertToHostUnits<T>(ref T geometry, bool convert = true)
            where T : Autodesk.DesignScript.Geometry.Geometry
        {
            if (convert)
            {
                var result = geometry.InHostUnits();
                geometry.Dispose();
                geometry = result;
                return result;
            }
            else
            {
                return geometry;
            }
        }
    }

}
