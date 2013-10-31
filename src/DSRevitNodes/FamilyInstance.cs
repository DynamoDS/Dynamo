using System;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using Point = Autodesk.DesignScript.Geometry.Point;
using Curve = Autodesk.DesignScript.Geometry.Curve;
using Face = Autodesk.DesignScript.Geometry.Face;

namespace DSRevitNodes
{
    class FamilyInstance
    {
        public Point Location { get; set; }

        static FamilyInstance ByPt(FamilySymbol fs, Point p)
        {
            throw new NotImplementedException();
        }

        static FamilyInstance ByCurve(FamilySymbol fs, Curve c)
        {
            throw new NotImplementedException();
        }

        static FamilyInstance ByCoords(FamilySymbol fs, double x, double y, double z)
        {
            throw new NotImplementedException();
        }

        static FamilyInstance ByUVPtOnFace(FamilySymbol fs, Vector uv, Face f)
        {
            throw new NotImplementedException();
        }

        static FamilyInstance ByPtAndLevel(Point p, Level l)
        {
            throw new NotImplementedException();
        }
    }
}
