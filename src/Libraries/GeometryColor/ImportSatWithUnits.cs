using DynamoUnits;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Autodesk.DesignScript.Runtime;

//TODO at a major host release verion - 2024 for example, rename this assembly to something like
//ZeroTouchGeometryNodes and use type forward attribute to retain API compat.
namespace Autodesk.DesignScript.Geometry
{
    /// <summary>
    /// Geometry Import nodes that have dependencies we don't want to introduce into Protogeometry.
    /// </summary>
    public static class GeometryImportHelpers
    {
        /// <summary>
        /// Imports geometry from SAT filepath. Set the dynamoUnit input to match how you are 
        /// interperting the other numbers in your Dynamo file.
        /// </summary>
        /// <param name="filePath">string file path to a .SAT file.</param>
        /// <param name="dynamoUnit">a forge unit length, if left null, sat file will be imported as unitless</param>
        /// <returns></returns>
        public static IEnumerable<Geometry> ImportFromSatByUnits(string filePath, [DefaultArgument("null")] DynamoUnits.Unit dynamoUnit)
        {
            double mm_per_unit = -1;
            if (dynamoUnit != null )
            {
                const string milimeters = "autodesk.unit.unit:millimeters";
                var mm = Unit.ByTypeID($"{milimeters}-1.0.1");

                if (!dynamoUnit.ConvertibleUnits.Contains(mm)){
                    throw new Exception($"{dynamoUnit.Name} was not convertible to mm");
                }
                mm_per_unit = Utilities.ConvertByUnits(1, dynamoUnit, mm);
            }
            return Geometry.ImportFromSAT(filePath, mm_per_unit);
        }

        /// <summary>
        /// Imports geometry from SAT filepath. Set the dynamoUnit input to match how you are 
        /// interperting the other numbers in your Dynamo file.
        /// </summary>
        /// <param name="file">file object pointing to a .SAT file.</param>
        /// <param name="dynamoUnit">a forge unit length, if left null, sat file will be imported as unitless</param>
        /// <returns></returns>
        public static IEnumerable<Geometry> ImportFromSatByUnits(FileInfo file, [DefaultArgument("null")] DynamoUnits.Unit dynamoUnit)
        {
            double mm_per_unit = -1;
            if (dynamoUnit != null)
            {
                const string milimeters = "autodesk.unit.unit:millimeters";
                var mm = Unit.ByTypeID($"{milimeters}-1.0.1");

                if (!dynamoUnit.ConvertibleUnits.Contains(mm)){
                    throw new Exception($"{dynamoUnit.Name} was not convertible to mm");
                }
                mm_per_unit = Utilities.ConvertByUnits(1, dynamoUnit, mm);
            }
            return Geometry.ImportFromSAT(file, mm_per_unit);
        }
    }
}
