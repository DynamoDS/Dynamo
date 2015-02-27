using System;
using System.Collections.Generic;

using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

namespace SampleLibraryZeroTouch
{
    [IsVisibleInDynamoLibrary(false)]
    public class PointField : IDisposable, IGraphicItem
    {
        private List<double> vertexCoords = new List<double>();

        private PointField(double t)
        {
            for (double x = -5; x <= 5; x += 0.5)
            {
                for (double y = -5; y <= 5; y += 0.5)
                {
                    double z = Math.Sin(Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)) - t);
                    vertexCoords.Add(x);
                    vertexCoords.Add(y);
                    vertexCoords.Add(z);
                }
            }
        }
        public static PointField ByParameter(double t)
        {
            return new PointField(t);
        }

        public void Dispose()
        {
        }

        public void Tessellate(IRenderPackage package, double tol = -1, int maxGridLines = 512)
        {
            package.PointVertices = vertexCoords;
        }
    }
}
