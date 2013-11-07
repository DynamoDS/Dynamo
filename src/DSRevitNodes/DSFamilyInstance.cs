using System;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using Point = Autodesk.DesignScript.Geometry.Point;
using Curve = Autodesk.DesignScript.Geometry.Curve;
using Face = Autodesk.DesignScript.Geometry.Face;

namespace DSRevitNodes
{
    class DSFamilyInstance
    {
        public Point Location { get; set; }

        static DSFamilyInstance ByPt(DSFamilySymbol fs, Point p)
        {
            throw new NotImplementedException();
        }

        static DSFamilyInstance ByCurve(DSFamilySymbol fs, DSCurve c)
        {
            throw new NotImplementedException();
        }

        static DSFamilyInstance ByCoords(DSFamilySymbol fs, double x, double y, double z)
        {
            throw new NotImplementedException();
        }

        static DSFamilyInstance ByUVPtOnFace(DSFamilySymbol fs, Vector uv, DSFace f)
        {
            throw new NotImplementedException();
        }

        static DSFamilyInstance ByPtAndLevel(Point p, DSLevel l)
        {
            throw new NotImplementedException();
        }
    }
}
