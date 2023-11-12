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
    /// These methods are further wrapped up by NodeModel nodes so we can get the naming and library location 
    /// exactly right.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public static class ImportHelpers
    {
        /// <summary>
        /// Imports geometry from SAT filepath. Set the dynamoUnit input to match how you are 
        /// interperting the other numbers in your Dynamo file.
        /// </summary>
        /// <param name="filePath">string file path to a .SAT file.</param>
        /// <param name="dynamoUnit">a forge unit length, if left null, sat file will be imported as unitless</param>
        /// <returns></returns>
        [AllowRankReduction]
        public static IEnumerable<Geometry> ImportFromSATWithUnits(string filePath, [DefaultArgument("null")] DynamoUnits.Unit dynamoUnit)
        {
            var mm_per_unit = CalculateMillimeterPerUnit(dynamoUnit);
            return Geometry.ImportFromSAT(filePath, mm_per_unit);
        }

        /// <summary>
        /// Imports geometry from SAT filepath. Set the dynamoUnit input to match how you are 
        /// interperting the other numbers in your Dynamo file.
        /// </summary>
        /// <param name="file">file object pointing to a .SAT file.</param>
        /// <param name="dynamoUnit">a forge unit length, if left null, sat file will be imported as unitless</param>
        /// <returns></returns>
        [AllowRankReduction]
        public static IEnumerable<Geometry> ImportFromSATWithUnits(FileInfo file, [DefaultArgument("null")] DynamoUnits.Unit dynamoUnit)
        {
            var mm_per_unit = CalculateMillimeterPerUnit(dynamoUnit);
            return Geometry.ImportFromSAT(file, mm_per_unit);
        }


        /// <summary>
        /// Imports geometry from SAB byte array. Set the dynamoUnit input to match how you are 
        /// interperting the other numbers in your Dynamo file.
        /// </summary>
        /// <param name="buffer">SAB byte array</param>
        /// <param name="dynamoUnit">a forge unit length, if left null, sat file will be imported as unitless</param>
        /// <returns></returns>
        [AllowRankReduction]
        public static IEnumerable<Geometry> DeserializeFromSABWithUnits(byte[] buffer, [DefaultArgument("null")] DynamoUnits.Unit dynamoUnit)
        {
            var mm_per_unit = CalculateMillimeterPerUnit(dynamoUnit);
            return Geometry.DeserializeFromSAB(buffer, mm_per_unit);
        }

        private static double CalculateMillimeterPerUnit(Unit dynamoUnit)
        {
            if (dynamoUnit != null)
            {
                const string millimeters = "autodesk.unit.unit:millimeters";
                var mm = Unit.ByTypeID($"{millimeters}-1.0.1");

                if (!dynamoUnit.ConvertibleUnits.Contains(mm))
                {
                    throw new Exception($"{dynamoUnit.Name} was not convertible to mm");
                }
                return Utilities.ConvertByUnits(1, dynamoUnit, mm);
            }
            return -1;
        }

    }
}
