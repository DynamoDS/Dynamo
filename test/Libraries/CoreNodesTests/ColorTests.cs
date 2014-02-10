using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using DSCoreNodes;
using NUnit.Framework;
using DSCore;
using List = DSCore.List;

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
            DSColor color = DSColor.ByARGB(10, 20, 30, 40);
            Assert.AreEqual(10, color.InternalColor.A);
            Assert.AreEqual(20, color.InternalColor.R);
            Assert.AreEqual(30, color.InternalColor.G);
            Assert.AreEqual(40, color.InternalColor.B);
        }

        // [Test]
        // public void TestConstructorBySystemColor()
        // {
        //     System.Drawing.Color original = System.Drawing.Color.FromArgb(10, 20, 30, 40);
        //     DSColor color = DSColor.BySystemColor(original);
        //     Assert.AreEqual(10, color.InternalColor.A);
        //     Assert.AreEqual(20, color.InternalColor.R);
        //     Assert.AreEqual(30, color.InternalColor.G);
        //     Assert.AreEqual(40, color.InternalColor.B);
        // }

        [Test]
        public void TestBrightness()
        {
            DSColor color = DSColor.ByARGB(128, 64, 160, 255);
            Assert.AreEqual(0.625490189, DSColor.Brightness(color), 0.000001);
        }

        [Test]
        public void TestSaturation()
        {
            DSColor color = DSColor.ByARGB(128, 64, 160, 255);
            Assert.AreEqual(1.0, DSColor.Saturation(color), 0.000001);
        }

        [Test]
        public void TestHue()
        {
            DSColor color = DSColor.ByARGB(128, 64, 160, 255);
            Assert.AreEqual(209.842926, DSColor.Hue(color), 0.000001);
        }

        [Test]
        public void TestComponents()
        {
            DSColor color = DSColor.ByARGB(128, 64, 160, 255);
            byte[] components = DSColor.Components(color);

            Assert.AreEqual(128, components[0]);
            Assert.AreEqual(64,  components[1]);
            Assert.AreEqual(160, components[2]);
            Assert.AreEqual(255, components[3]);
        }
    }
}
