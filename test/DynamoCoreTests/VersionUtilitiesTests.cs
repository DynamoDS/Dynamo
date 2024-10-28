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

        [Test]
        public void ParseVersionSafely_NullInput_ReturnsNull()
        {
            // Arrange
            string version = null;

            // Act
            Version result = VersionUtilities.Parse(version);

            // Assert
            Assert.IsNull(result, "Expected null when input is null.");
        }

        [Test]
        public void ParseVersionSafely_JibberishInput_ReturnsNull()
        {
            // Arrange
            string version = "not.a.version";

            // Act
            Version result = VersionUtilities.Parse(version);

            // Assert
            Assert.IsNull(result, "Expected null when input is an invalid version string.");
        }

        [Test]
        public void ParseVersionSafely_ShortVersion_ReturnsPaddedVersion()
        {
            // Arrange
            string version = "2019";

            // Act
            Version result = VersionUtilities.Parse(version);

            // Assert
            Assert.AreEqual(new Version(2019, 0, 0), result, "Expected version '2019.0.0' when input is '2019'.");
        }

        [Test]
        public void ParseVersionSafely_FullVersion_ReturnsParsedVersion()
        {
            // Arrange
            string version = "2019.1.2";

            // Act
            Version result = VersionUtilities.Parse(version);

            // Assert
            Assert.AreEqual(new Version(2019, 1, 2), result, "Expected version '2019.1.2' when input is '2019.1.2'.");
        }

    }

}
