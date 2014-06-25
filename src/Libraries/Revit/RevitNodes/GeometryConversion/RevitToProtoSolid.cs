using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;

namespace Revit.GeometryConversion
{
    [IsVisibleInDynamoLibrary(false)]
    [SupressImportIntoVM]
    public static class RevitToProtoSolid
    {
        public static Autodesk.DesignScript.Geometry.Solid ToProtoType(this Autodesk.Revit.DB.Solid solid, 
            bool performHostUnitConversion = true)
        {
            var srfs = solid.Faces.Cast<Autodesk.Revit.DB.Face>().Select(x => x.ToProtoType(false));
            var converted = Solid.ByJoinedSurfaces( srfs );

            return performHostUnitConversion ? converted.InDynamoUnits() : converted;
        }
    }
}
