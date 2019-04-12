using System;
using System.Diagnostics;
using Autodesk.DesignScript.Geometry;
using NUnit.Framework;
using TestServices;

namespace DSCoreNodesTests
{
    [TestFixture]
    class ThreadTests : GeometricTestBase
    {
        [Test]
        [Category("Failure")]
        public void Thread_Pause()
        {
            var sw = new Stopwatch();
            sw.Start();

            Circle cir =(Circle) DSCore.Thread.Pause(Circle.ByCenterPointRadius(Point.ByCoordinates(0, 0), 10), 1000);
            sw.Stop();
            Assert.IsTrue(Math.Abs(sw.ElapsedMilliseconds - 1000) <= 1.0);
            Assert.NotNull(cir);
        }
    }
}
