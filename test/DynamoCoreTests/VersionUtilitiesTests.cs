using System;

using Dynamo.Utilities;

using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class VersionUtilitiesTests
    {
        [Test]
        [Category("UnitTests")]
        public void PartialParse_String_ReturnsCorrectResults()
        {
            Assert.IsTrue(VersionUtilities.PartialParse("0.7.0.9") > VersionUtilities.PartialParse("0.0.5"));
            Assert.IsTrue(VersionUtilities.PartialParse("0.7.0") > VersionUtilities.PartialParse("0.0.5"));
            Assert.IsTrue(VersionUtilities.PartialParse("0.7.0") > VersionUtilities.PartialParse("0.0.5"));
            Assert.IsTrue(VersionUtilities.PartialParse("0.7.0") > VersionUtilities.PartialParse("0.0.5"));
            Assert.IsTrue(VersionUtilities.PartialParse("0.7.1") > VersionUtilities.PartialParse("0.0.5"));

            Assert.IsFalse(VersionUtilities.PartialParse("0.7.0.123") > VersionUtilities.PartialParse("0.7.0.467"));
            Assert.IsFalse(VersionUtilities.PartialParse("0.7.0") > VersionUtilities.PartialParse("0.7.0"));
            Assert.IsFalse(VersionUtilities.PartialParse("0.0.5") > VersionUtilities.PartialParse("0.7.0"));
            Assert.IsFalse(VersionUtilities.PartialParse("0.7.5") > VersionUtilities.PartialParse("1.7.0"));
            Assert.IsFalse(VersionUtilities.PartialParse("0.0.5") > VersionUtilities.PartialParse("0.0.7"));
            Assert.IsFalse(VersionUtilities.PartialParse("1.0.5") > VersionUtilities.PartialParse("2.7.0"));
            Assert.IsFalse(VersionUtilities.PartialParse("0.7.0") > VersionUtilities.PartialParse("0.7.1"));
        }

        [Test]
        [Category("UnitTests")]
        public void PartialParse_String_ThrowsAnExceptionWhenPassedAMalformedString()
        {
            Assert.Throws(typeof(FormatException), () => VersionUtilities.PartialParse("0.0.a"));
            Assert.Throws(typeof(FormatException), () => VersionUtilities.PartialParse("0.0"));
            Assert.Throws(typeof(FormatException), () => VersionUtilities.PartialParse(""));
        }

    }

}
