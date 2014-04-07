using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using NUnit.Framework;
using List = DSCore.List;

namespace DSCoreNodesTests
{
    /// <summary>
    /// PB:  This is just a starting point, just to check that ProtoGeometry is properly working in Dynamo.
    /// 
    /// It should probably get refactored out at some point.
    /// </summary>
    [TestFixture]
    internal static class ProtoNodesTests
    {
        [Test]
        [Category("Failing")]
        public static void CanDoSimpleLoft()
        {
            HostFactory.Instance.StartUp();

            var points0 = new Point[10];
            var points1 = new Point[10];

            for (int i = 0; i < 10; ++i)
            {
                points0[i] = Point.ByCoordinates(i, 0, 0);
                points1[i] = Point.ByCoordinates(i, 10, 10);
            }

            var crv0 = NurbsCurve.ByControlPoints(points0);
            var crv1 = NurbsCurve.ByControlPoints(points1);

            var srf = Surface.ByLoft(new[] { crv0, crv1 });
            Assert.NotNull(srf);

            Console.WriteLine(srf.PointAtParameter(0.5,0.5));

            HostFactory.Instance.ShutDown();
        }

    }
}
