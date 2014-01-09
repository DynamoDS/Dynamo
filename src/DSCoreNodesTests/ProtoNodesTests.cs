using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using NUnit.Framework;
using List = DSCoreNodes.List;

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
        public static void CanStartupAsmAndCreateSimpleCircle()
        {
            HostFactory.Instance.StartUp();

            var x = Point.ByCoordinates(0, 0, 0);
            var y = Point.ByCoordinates(1, 0, 0);
            var z = Point.ByCoordinates(2, 2, 0);

            var c = Circle.ByPointsOnCurve(x, y, z);

            HostFactory.Instance.ShutDown();
        }

    }
}
