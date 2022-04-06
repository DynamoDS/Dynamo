using System;
using System.Collections.Generic;
using System.Linq;
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
        [Test]
        [Category("UnitTests")]
        public static void GoldenRatio()
        {
            Assert.AreEqual(1.61803398875, DSCore.Math.GoldenRatio);

        }
        [Test]
        [Category("UnitTests")]
        public static void Log()
        {
            Assert.AreEqual(13.815510557964274, DSCore.Math.Log(1000000));
        }
        [Test]
        [Category("UnitTests")]
        public static void Log_Infinity()
        {
            Assert.IsTrue(System.Double.IsInfinity( DSCore.Math.Log(0)));
        }
        [Test]
        [Category("UnitTests")]
        public static void Log_NegativeInput()
        {
            Assert.IsTrue(System.Double.IsNaN(DSCore.Math.Log(-1)));
        }
        [Test]
        [Category("UnitTests")]
        public static void Test_RemapRange()
        {
            List<double> input = new List<double> { 0, 1, 2, 3, 4 };
            List<double> expectedResults = new List<double> { -10, -2.5, 5, 12.5, 20 };
            List<double> results = (List<double>)DSCore.Math.RemapRange(input, -10, 20);
            for(int i = 0; i < expectedResults.Count; i++)
            {
                Assert.AreEqual(expectedResults.ElementAt(i), results.ElementAt(i));
            }
        }
        [Test]
        [Category("UnitTests")]
        public static void Tan_NaN()
        {
            Assert.IsTrue(System.Double.IsNaN(DSCore.Math.Tan(90)));
        }

        [Test]
        [Category ("UnitTests")]
        public static void Factorial()
        {
            Assert.AreEqual(2432902008176640000, DSCore.Math.Factorial(20));
            Assert.Throws<ArgumentException>(() => DSCore.Math.Factorial(-1));
            Assert.Throws<System.OverflowException>(() => DSCore.Math.Factorial(25));
            Assert.Throws<System.OverflowException>(() => DSCore.Math.Factorial(100));

        }
        
        [Test]
        [Category("UnitTests")]
        public static void Map()
        {
            Assert.AreEqual(0.25, DSCore.Math.Map(1, 5, 2));
            Assert.AreEqual(0, DSCore.Math.Map(2, 4, 1));
        }

        [Test]
        [Category("UnitTests")]
        public static void MapTo()
        {
            Assert.AreEqual(2.8, DSCore.Math.MapTo(1, 3, 1.4, 1, 10));
            Assert.AreEqual(10, DSCore.Math.MapTo(2, 4, 5, 1, 10));
        }
    }
}
