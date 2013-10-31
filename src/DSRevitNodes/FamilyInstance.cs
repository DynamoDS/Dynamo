using System;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using Point = Autodesk.DesignScript.Geometry.Point;
using Curve = Autodesk.DesignScript.Geometry.Curve;
using Face = Autodesk.DesignScript.Geometry.Face;

namespace Dynamo.Revit
{
    class FamilyInstance
    {
        public Point Location { get; set; }

        static FamilyInstance ByPoint(FamilySymbol fs, Point p)
        {
            throw new NotImplementedException();
        }

        static FamilyInstance ByCurve(FamilySymbol fs, Curve c)
        {
            throw new NotImplementedException();
        }

        static FamilyInstance ByCoordinates(FamilySymbol fs, double x, double y, double z)
        {
            throw new NotImplementedException();
        }

        static FamilyInstance ByUVPointOnFace(FamilySymbol fs, Vector uv, Face f)
        {
            throw new NotImplementedException();
        }
    }
}
