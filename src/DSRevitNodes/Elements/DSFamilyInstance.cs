using System;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using DSRevitNodes.Elements;
using Point = Autodesk.DesignScript.Geometry.Point;
using Curve = Autodesk.DesignScript.Geometry.Curve;
using Face = Autodesk.DesignScript.Geometry.Face;

namespace DSRevitNodes
{
    public class DSFamilyInstance
    {
        public Point Location { get; set; }

        private DSFamilyInstance()
        {
            
        }

        static DSFamilyInstance ByPoint(DSFamilySymbol fs, Point p)
        {
            throw new NotImplementedException();
        }

        static DSFamilyInstance ByCurve(DSFamilySymbol fs, DSCurve c)
        {
            throw new NotImplementedException();
        }

        static DSFamilyInstance ByCoordinates(DSFamilySymbol fs, double x, double y, double z)
        {
            throw new NotImplementedException();
        }

        static DSFamilyInstance ByUvsOnFace(DSFamilySymbol fs, Vector uv, DSFace f)
        {
            throw new NotImplementedException();
        }

        static DSFamilyInstance ByPointAndLevel(Point p, DSLevel l)
        {
            throw new NotImplementedException();
        }
    }
}
