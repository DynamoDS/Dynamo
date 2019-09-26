using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using FFITarget.DesignScript;

namespace FFITarget
{

    public class TestDefaultArgumentAttributes
    {
        public static double ComputeCircle([DefaultArgument("FFITarget.DesignScript.Point.ByCoordinates(0, 0, 0)")]Point centerPoint, double radius = 1)
        {
            return radius * radius * Math.PI;
        }

        public static double ComputeCircleConflict([DefaultArgument("Point.ByCoordinates(0, 0, 0)")]Point centerPoint, double radius = 1)
        {
            return radius * radius * Math.PI;
        }

        public static IEnumerable<IEnumerable<Point>> ComputeFlattenedPoints([DefaultArgument("FFITarget.DesignScript.Point.ByCoordinates((1..2)<1>, (2..3)<2>, 0)")]IEnumerable<IEnumerable<Point>> points)
        {
            return points;
        }
    }
}
