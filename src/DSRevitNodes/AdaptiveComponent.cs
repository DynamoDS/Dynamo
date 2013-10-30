using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;

namespace Dynamo.Revit
{
    public class AdaptiveComponent
    {
        /// <summary>
        /// Create an adaptive component from a list of points.
        /// </summary>
        /// <param name="pts"></param>
        /// <returns></returns>
        static AdaptiveComponent ByPoints(List<Point> pts)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create an adaptive component by uv points on a face.
        /// </summary>
        /// <param name="pts"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        static AdaptiveComponent ByPointsOnFace(List<Vector> pts, Face f)
        {
            throw new NotImplementedException();
        }
    }
}
