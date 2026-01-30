using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using MIConvexHull;
using Tessellation.Adapters;

namespace Tessellation
{
    /// <summary>
    ///     Functions for creating convex hulls.
    /// </summary>
    public static class ConvexHull
    {
        /// <summary>
        ///     Creates a convex hull of a set of points.
        /// </summary>
        /// <param name="points">A set of points.</param>
        public static IEnumerable<Curve> ByPoints(IEnumerable<Point> points)
        {
            yield break;
        }
    }
}
