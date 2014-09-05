using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DSCoreNodesTests
{
    [TestFixture]
    public class MathTests
    {

        [Test]
        [Category("UnitTests")]
        public static void Sum()
        {

            Assert.AreEqual(6.0,
                DSCore.Math.Sum(new List<double> {1.0, 2.0, 3.0}));

            Assert.AreEqual(0.0,
                DSCore.Math.Sum(new List<double> {  }));

            Assert.AreEqual(0.0,
                DSCore.Math.Sum(new List<double> {0.0, 0.0, 0.0 }));

            Assert.AreEqual(0.0,
                DSCore.Math.Sum(new List<double> { -1.0, 1.0}));

            Assert.AreEqual(21.0,
                DSCore.Math.Sum(new List<double> { -1.0, 0.0, 2.0, 4.0, 6.0, 10.0 }));

        }

    }
}
