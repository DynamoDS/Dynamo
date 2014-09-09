using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DSCore;
using NUnit.Framework;

namespace DSCoreNodesTests
{
    static class ComparisonTests
    {
        [Test]
        [Category("UnitTests")]
        public static void LessThan_Ints()
        {
            Assert.AreEqual(true, Compare.LessThan(0, 1));
            Assert.AreEqual(false, Compare.LessThan(1, 0));
            Assert.AreEqual(false, Compare.LessThan(0, 0));
        }

        [Test]
        [Category("UnitTests")]
        public static void LessThan_Doubles()
        {
            Assert.AreEqual(true, Compare.LessThan(0.0, 1.0));
            Assert.AreEqual(false, Compare.LessThan(1.0, 0.0));
            Assert.AreEqual(false, Compare.LessThan(0.0, 0.0));
        }

        [Test]
        [Category("UnitTests")]
        public static void LessThan_DoublesAndInts()
        {
            Assert.AreEqual(true, Compare.LessThan(0.0, 1));
            Assert.AreEqual(false, Compare.LessThan(1, 0.0));
            Assert.AreEqual(false, Compare.LessThan(0.0, 0));
        }

        [Test]
        [Category("UnitTests")]
        public static void LessThan_Strings()
        {
            Assert.AreEqual(true, Compare.LessThan("a", "A"));
            Assert.AreEqual(false, Compare.LessThan("c", "a"));
            Assert.AreEqual(false, Compare.LessThan("a", "a"));
        }


        [Test]
        [Category("UnitTests")]
        public static void GreaterThan_Ints()
        {
            Assert.AreEqual(false, Compare.GreaterThan(0, 1));
            Assert.AreEqual(true, Compare.GreaterThan(1, 0));
            Assert.AreEqual(false, Compare.GreaterThan(0, 0));
        }

        [Test]
        [Category("UnitTests")]
        public static void GreaterThan_Doubles()
        {
            Assert.AreEqual(false, Compare.GreaterThan(0.0, 1.0));
            Assert.AreEqual(true, Compare.GreaterThan(1.0, 0.0));
            Assert.AreEqual(false, Compare.GreaterThan(0.0, 0.0));
        }

        [Test]
        [Category("UnitTests")]
        public static void GreaterThan_DoublesAndInts()
        {
            Assert.AreEqual(false, Compare.GreaterThan(0.0, 1));
            Assert.AreEqual(true, Compare.GreaterThan(1, 0.0));
            Assert.AreEqual(false, Compare.LessThan(0.0, 0));
        }

        [Test]
        [Category("UnitTests")]
        public static void GreaterThan_Strings()
        {
            Assert.AreEqual(false, Compare.GreaterThan("a", "A"));
            Assert.AreEqual(true, Compare.GreaterThan("c", "a"));
            Assert.AreEqual(false, Compare.GreaterThan("a", "a"));
        }


        [Test]
        [Category("UnitTests")]
        public static void LessThanOrEqual_Ints()
        {
            Assert.AreEqual(true, Compare.LessThanOrEqual(0, 1));
            Assert.AreEqual(false, Compare.LessThanOrEqual(1, 0));
            Assert.AreEqual(true, Compare.LessThanOrEqual(0, 0));
        }

        [Test]
        [Category("UnitTests")]
        public static void LessThanOrEqual_Doubles()
        {
            Assert.AreEqual(true, Compare.LessThanOrEqual(0.0, 1.0));
            Assert.AreEqual(false, Compare.LessThanOrEqual(1.0, 0.0));
            Assert.AreEqual(true, Compare.LessThanOrEqual(0.0, 0.0));
        }

        [Test]
        [Category("UnitTests")]
        public static void LessThanOrEqual_DoublesAndInts()
        {
            Assert.AreEqual(true, Compare.LessThanOrEqual(0.0, 1));
            Assert.AreEqual(false, Compare.LessThanOrEqual(1, 0.0));
            Assert.AreEqual(true, Compare.LessThanOrEqual(0.0, 0));
        }

        [Test]
        [Category("UnitTests")]
        public static void LessThanOrEqual_Strings()
        {
            Assert.AreEqual(true, Compare.LessThanOrEqual("a", "A"));
            Assert.AreEqual(false, Compare.LessThanOrEqual("c", "a"));
            Assert.AreEqual(true, Compare.LessThanOrEqual("a", "a"));
        }


        [Test]
        [Category("UnitTests")]
        public static void GreaterThanOrEqual_Ints()
        {
            Assert.AreEqual(false, Compare.GreaterThanOrEqual(0, 1));
            Assert.AreEqual(true, Compare.GreaterThanOrEqual(1, 0));
            Assert.AreEqual(true, Compare.GreaterThanOrEqual(0, 0));
        }

        [Test]
        [Category("UnitTests")]
        public static void GreaterThanOrEqual_Doubles()
        {
            Assert.AreEqual(false, Compare.GreaterThanOrEqual(0.0, 1.0));
            Assert.AreEqual(true, Compare.GreaterThanOrEqual(1.0, 0.0));
            Assert.AreEqual(true, Compare.GreaterThanOrEqual(0.0, 0.0));
        }

        [Test]
        [Category("UnitTests")]
        public static void GreaterThanOrEqual_DoublesAndInts()
        {
            Assert.AreEqual(false, Compare.GreaterThanOrEqual(0.0, 1));
            Assert.AreEqual(true, Compare.GreaterThanOrEqual(1, 0.0));
            Assert.AreEqual(true, Compare.LessThanOrEqual(0.0, 0));
        }

        [Test]
        [Category("UnitTests")]
        public static void GreaterThanOrEqual_Strings()
        {
            Assert.AreEqual(false, Compare.GreaterThanOrEqual("a", "A"));
            Assert.AreEqual(true, Compare.GreaterThanOrEqual("c", "a"));
            Assert.AreEqual(true, Compare.GreaterThanOrEqual("a", "a"));
        }
    }
}
