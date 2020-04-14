using System;
using Dynamo.Updates;
using NUnit.Framework;

namespace Dynamo.Tests.Core
{
    [TestFixture]
    public class BinaryVersionTests : DynamoModelTestBase
    {
        [Test]
        [Category("UnitTests")]
        public void FromStringTest()
        {
            Assert.IsNull(BinaryVersion.FromString(null));
            Assert.IsNull(BinaryVersion.FromString(""));

            Assert.IsNull(BinaryVersion.FromString("."));
            Assert.IsNull(BinaryVersion.FromString("1.1"));

            Assert.IsNull(BinaryVersion.FromString("a.1.1.1"));
            Assert.IsNull(BinaryVersion.FromString("1.a.1.1"));
            Assert.IsNull(BinaryVersion.FromString("1.1.a.1"));
            Assert.IsNull(BinaryVersion.FromString("1.1.1.a"));
        }

        [Test]
        [Category("UnitTests")]
        public void GetHashCodeTest()
        {
            BinaryVersion bin = BinaryVersion.FromString("1.1.1.1");
                Console.WriteLine(bin.GetHashCode());

            Assert.IsNotNull(bin.GetHashCode());
        }

        [Test]
        [Category("UnitTests")]
        public void EqualsTest()
        {
            BinaryVersion firstBin = BinaryVersion.FromString("1.1.1.1");
            BinaryVersion secondBin = firstBin;

            Assert.IsTrue(firstBin.Equals(secondBin));
        }

        [Test]
        [Category("UnitTests")]
        public void LessThanOperatorTest()
        {
            BinaryVersion firstBin = BinaryVersion.FromString("1.1.1.1");

            BinaryVersion secondBin = firstBin;
            Assert.IsFalse(firstBin < secondBin);

            secondBin = null;
            Assert.IsFalse(firstBin < secondBin);

            secondBin = BinaryVersion.FromString("2.2.2.2");
            Assert.IsTrue(firstBin < secondBin);
        }

        [Test]
        [Category("UnitTests")]
        public void LessOrEqualOperatorTest()
        {
            BinaryVersion bin = BinaryVersion.FromString("1.1.1.1");

            BinaryVersion secondBin = bin;
            Assert.IsFalse(bin <= secondBin);

            secondBin = null;
            Assert.IsFalse(bin <= secondBin);

            secondBin = BinaryVersion.FromString("2.2.2.2");
            Assert.IsTrue(bin <= secondBin);

            secondBin = BinaryVersion.FromString("1.1.1.1");
            Assert.IsTrue(bin <= secondBin);
        }

        [Test]
        [Category("UnitTests")]
        public void MoreThanOperatorTest()
        {
            BinaryVersion firstBin = BinaryVersion.FromString("2.2.2.2");

            BinaryVersion secondBin = firstBin;
            Assert.IsFalse(firstBin > secondBin);

            secondBin = null;
            Assert.IsFalse(firstBin > secondBin);

            secondBin = BinaryVersion.FromString("1.1.1.1");
            Assert.IsTrue(firstBin > secondBin);
        }

        [Test]
        [Category("UnitTests")]
        public void MoreOrEqualOperatorTest()   
        {
            BinaryVersion firstBin = BinaryVersion.FromString("2.2.2.2");

            BinaryVersion secondBin = firstBin;
            Assert.IsFalse(firstBin >= secondBin);

            secondBin = null;
            Assert.IsFalse(firstBin >= secondBin);

            secondBin = BinaryVersion.FromString("1.1.1.1");
            Assert.IsTrue(firstBin >= secondBin);

            secondBin = BinaryVersion.FromString("2.2.2.2");
            Assert.IsTrue(firstBin >= secondBin);
        }

        [Test]
        [Category("UnitTests")]
        public void EqualOperatorTestTest()
        {
            BinaryVersion bin = BinaryVersion.FromString("1.1.1.1");

            BinaryVersion secondBin = bin;
            Assert.IsTrue(bin == secondBin);

            secondBin = null;
            Assert.IsFalse(bin == secondBin);

            secondBin = BinaryVersion.FromString("1.1.1.1");
            Assert.IsTrue(bin == secondBin);
        }

        [Test]
        [Category("UnitTests")]
        public void NotEqualOperatorTestTest()
        {
            BinaryVersion bin = BinaryVersion.FromString("1.1.1.1");

            BinaryVersion secondBin = bin;
            Assert.IsFalse(bin != secondBin);

            secondBin = null;
            Assert.IsTrue(bin != secondBin);

            secondBin = BinaryVersion.FromString("1.1.1.1");
            Assert.IsFalse(bin != secondBin);

            secondBin = BinaryVersion.FromString("2.2.2.2");
            Assert.IsTrue(bin != secondBin);
        }
    }
}