using Autodesk.DesignScript.Geometry;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestServices;

namespace DSCoreNodesTests
{
    [TestFixture]
    class ThreadTests : GeometricTestBase
    {
        [Test]
        public void Thread_Pause()
        {
            var sw = new Stopwatch();
            sw.Start();

            Circle cir =(Circle) DSCore.Thread.Pause(Circle.ByCenterPointRadius(Point.ByCoordinates(0, 0), 10), 1000);
            sw.Stop();
            Assert.GreaterOrEqual(sw.Elapsed.TotalMilliseconds, 1000);
            Assert.NotNull(cir);
        }
    }
}
