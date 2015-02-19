using System;
using System.Collections.Generic;

using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

namespace SampleLibraryZeroTouch
{
    [IsVisibleInDynamoLibrary(false)]
    public class PointField : IDisposable, IGraphicItem
    {
        private List<Point> points = new List<Point>();

        private PointField(double t)
        {
            for (double x = -5; x <= 5; x += 1)
            {
                for (double y = -5; y <= 5; y += 1)
                {
                    double z = Math.Sin(Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)) - t);
                    points.Add(Point.ByCoordinates(x, y, z));
                }
            }
        }
        public static PointField ByParameter(double t)
        {
            return new PointField(t);
        }

        public void Dispose()
        {
            foreach (Point pt in points)
            {
                pt.Dispose();
            }
        }

        public void Tessellate(IRenderPackage package, double tol = -1, int maxGridLines = 512)
        {
            foreach (var pt in points)
            {
                package.PushPointVertex(pt.X, pt.Y, pt.Z);
            }
        }
    }
}
