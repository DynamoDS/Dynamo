using System;
using Autodesk.Revit.DB;
using Autodesk.DesignScript.Geometry;
using Curve = Autodesk.DesignScript.Geometry.Curve;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace Dynamo.Revit
{
    class Wall
    {
        static Wall ByStartEndPoints(Point start, Point end, Autodesk.Revit.DB.Level bottom, Autodesk.Revit.DB.Level top)
        {
            throw new NotImplementedException();
        }

        static Wall ByCurve(Curve c, Autodesk.Revit.DB.Level bottom, Autodesk.Revit.DB.Level top)
        {
            throw new NotImplementedException();
        }
    }
}
