using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Instance.StartUp();
            var pts = new[]
            {
                Point.ByCoordinates(10,2,3)
                , Point.ByCoordinates(0,2,2)
                , Point.ByCoordinates(10,4,8)
                , Point.ByCoordinates(10,2,8)
                , Point.ByCoordinates(5,5,5)
            };

            var bspline = BSplineCurve.ByControlVertices(pts, 3);

            Console.WriteLine(bspline.Degree);
            Console.WriteLine(bspline.ControlVertices);

            var pt = bspline.ClosestPointTo(pts[0]);
            Console.WriteLine(pt == null);
        }
    }
}
