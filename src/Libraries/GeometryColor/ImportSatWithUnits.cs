using DynamoUnits;
using System;
using System.Collections.Generic;
using System.IO;
using Autodesk.DesignScript.Runtime;
using Autodesk.DesignScript.Geometry;

namespace Autodesk.DesignScript.Geometry
{

    public class GeometryImportHelpers
    {
        [IsVisibleInDynamoLibrary(false)]
        public GeometryImportHelpers() { }

        public IEnumerable<Geometry> ImportFromSatByUnits(string filePath, [DefaultArgument("null")] DynamoUnits.Unit dynamoUnit)
        {
            double mm_per_unit = -1;
            if (dynamoUnit != null)
            {
                const string milimeters = "autodesk.unit.unit:millimeters";
                var mm = Unit.ByTypeID($"{milimeters}-1.0.1");
                mm_per_unit = Utilities.ConvertByUnits(1, dynamoUnit, mm);
            }
            return null;
            //return Geometry.ImportFromSAT(filePath, mm_per_unit);
        }
        public IEnumerable<Geometry> ImportFromSatByUnits(FileInfo file, [DefaultArgument("null")] DynamoUnits.Unit dynamoUnit)
        {
            double mm_per_unit = -1;
            if (dynamoUnit != null)
            {
                const string milimeters = "autodesk.unit.unit:millimeters";
                var mm = Unit.ByTypeID($"{milimeters}-1.0.1");
                mm_per_unit = Utilities.ConvertByUnits(1, dynamoUnit, mm);
            }
            return null;
            //return Geometry.ImportFromSAT(file, mm_per_unit);
        }
    }
}
