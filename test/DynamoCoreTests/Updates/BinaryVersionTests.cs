using Dynamo.Updates;
using NUnit.Framework;

namespace Dynamo.Tests
{
    /// <summary>
    /// Test class to test the BinaryVersion class
    /// </summary>
    [TestFixture]
    public class BinaryVersionTests : DynamoModelTestBase
    {
        /// <summary>
        /// FromString method test
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void FromStringTest()
        {
            //If the string is not valid, it should return null
            Assert.IsNull(BinaryVersion.FromString(null));
            Assert.IsNull(BinaryVersion.FromString(""));

            Assert.IsNull(BinaryVersion.FromString("."));
            Assert.IsNull(BinaryVersion.FromString("1.1"));

            Assert.IsNull(BinaryVersion.FromString("a.1.1.1"));
            Assert.IsNull(BinaryVersion.FromString("1.a.1.1"));
            Assert.IsNull(BinaryVersion.FromString("1.1.a.1"));
            Assert.IsNull(BinaryVersion.FromString("1.1.1.a"));

            //If the string is valid, check that the properties are filled correctly
            BinaryVersion version = BinaryVersion.FromString("1.2.3.4");

            Assert.IsNotNull(version);
            Assert.AreEqual(1, version.FileMajor);
            Assert.AreEqual(2, version.FileMinor);
            Assert.AreEqual(3, version.FileBuild);
            Assert.AreEqual(4, version.FilePrivate);
        }

        /// <summary>
        /// GetHashCode method test
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void GetHashCodeTest()
        {
            BinaryVersion bin = BinaryVersion.FromString("1.1.1.1");
            Assert.IsNotNull(bin.GetHashCode());
        }

        /// <summary>
        /// GetHashCode method test
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void EqualsTest()
        {
            BinaryVersion firstBin = BinaryVersion.FromString("1.1.1.1");
            BinaryVersion secondBin = firstBin;

            Assert.IsTrue(firstBin.Equals(secondBin));
        }

        /// <summary>
        /// "Less than" operator test
        /// </summary>
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

        /// <summary>
        /// "Less than or equal" operator test
        /// </summary>
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

        /// <summary>
        /// "Greater than" operator test
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void GreaterThanOperatorTest()
        {
            BinaryVersion firstBin = BinaryVersion.FromString("2.2.2.2");

            BinaryVersion secondBin = firstBin;
            Assert.IsFalse(firstBin > secondBin);

            secondBin = null;
            Assert.IsFalse(firstBin > secondBin);

            secondBin = BinaryVersion.FromString("1.1.1.1");
            Assert.IsTrue(firstBin > secondBin);
        }

        /// <summary>
        /// "Greater than or equal" operator test
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void GreaterOrEqualOperatorTest()   
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

        /// <summary>
        /// "Equal" operator test
        /// </summary>
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

        /// <summary>
        /// "Not equal" operator test
        /// </summary>
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