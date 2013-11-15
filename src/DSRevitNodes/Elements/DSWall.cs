using System;
using Autodesk.Revit.DB;
using Autodesk.DesignScript.Geometry;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace DSRevitNodes
{
    class DSWall
    {
        public static DSWall ByStartEndPoints(Point start, Point end, Autodesk.Revit.DB.Level bottom, Autodesk.Revit.DB.Level top)
        {
            throw new NotImplementedException();
        }

        public static DSWall ByCurve(DSCurve c, Autodesk.Revit.DB.Level bottom, Autodesk.Revit.DB.Level top)
        {
            throw new NotImplementedException();
        }
    }
}
