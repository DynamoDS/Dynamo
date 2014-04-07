using NUnit.Framework;
using DSCore;

namespace DSCoreNodesTests
{
    /// <summary>
    /// PB:  This is just a starting point, just to check that ProtoGeometry is properly working in Dynamo.
    /// 
    /// It should probably get refactored out at some point.
    /// </summary>
    [TestFixture]
    public class ColorTests
    {
        [Test]
        public void TestConstructorByARGB()
        {
            Color color = Color.ByARGB(10, 20, 30, 40);
            Assert.AreEqual(10, color.InternalColor.A);
            Assert.AreEqual(20, color.InternalColor.R);
            Assert.AreEqual(30, color.InternalColor.G);
            Assert.AreEqual(40, color.InternalColor.B);
        }

        // [Test]
        // public void TestConstructorBySystemColor()
        // {
        //     System.Drawing.Color original = System.Drawing.Color.FromArgb(10, 20, 30, 40);
        //     Color color = Color.BySystemColor(original);
        //     Assert.AreEqual(10, color.InternalColor.A);
        //     Assert.AreEqual(20, color.InternalColor.R);
        //     Assert.AreEqual(30, color.InternalColor.G);
        //     Assert.AreEqual(40, color.InternalColor.B);
        // }

        [Test]
        public void TestBrightness()
        {
            Color color = Color.ByARGB(128, 64, 160, 255);
            Assert.AreEqual(0.625490189, Color.Brightness(color), 0.000001);
        }

        [Test]
        public void TestSaturation()
        {
            Color color = Color.ByARGB(128, 64, 160, 255);
            Assert.AreEqual(1.0, Color.Saturation(color), 0.000001);
        }

        [Test]
        public void TestHue()
        {
            Color color = Color.ByARGB(128, 64, 160, 255);
            Assert.AreEqual(209.842926, Color.Hue(color), 0.000001);
        }

        [Test]
        public void TestComponents()
        {
            Color color = Color.ByARGB(128, 64, 160, 255);
            var components = Color.Components(color);

            Assert.AreEqual(128, components["a"]);
            Assert.AreEqual(64,  components["r"]);
            Assert.AreEqual(160, components["g"]);
            Assert.AreEqual(255, components["b"]);
        }
    }
}
