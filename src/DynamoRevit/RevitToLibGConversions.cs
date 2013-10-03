using Autodesk.LibG;
using Autodesk.Revit.DB;
using Point = Autodesk.LibG.Point;

namespace Dynamo.Applications
{
    public static class RevitToLibGConversions
    {
        public static Point XyzToPoint(XYZ xyz)
        {
            return Point.by_coordinates(xyz.X, xyz.Y, xyz.Z);
        }

        public static XYZ PointToXyz(Point pt)
        {
            return new XYZ(pt.x(), pt.y(), pt.z());
        }
    }
}
