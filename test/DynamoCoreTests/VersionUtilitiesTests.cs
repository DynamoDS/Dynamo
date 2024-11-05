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
        public void ParseVersionSafely_TooManyComponents_ReturnsNull()
        {
            // Arrange
            string version = "1.2.3.4.5";

            // Act
            Version result = VersionUtilities.Parse(version);

            // Assert
            Assert.IsNull(result, "Expected null when input has too many components '1.2.3.4.5'.");
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

        [Test]
        public void ParseVersionSafely_LongVersion_ReturnsParsedVersion()
        {
            // Arrange
            string version = "2019.1.2.0";

            // Act
            Version result = VersionUtilities.Parse(version);

            // Assert
            Assert.AreEqual(new Version(2019, 1, 2, 0), result, "Expected version '2019.1.2.0' when input is '2019.1.2.0'.");
        }

        [Test]
        public void ParseWildCard_ValidVersion_ReturnsNull()
        {
            // Arrange
            string version = "2019.1.2";

            // Act
            Version result = VersionUtilities.WildCardParse(version);

            // Assert
            Assert.IsNull(result, "Expected null when input does not end with a wildcard symbol.");
        }

        [Test]
        public void ParseWildCard_InvalidWildcardVersion1_ReturnsNull()
        {
            // Arrange
            string version = "2019.1.2.*";

            // Act
            Version result = VersionUtilities.WildCardParse(version);

            // Assert
            Assert.IsNull(result, "Expected null when input is an invalid version string.");
        }

        [Test]
        public void ParseWildCard_InvalidWildcardVersion2_ReturnsNull()
        {
            // Arrange
            string version = "2019.1*";

            // Act
            Version result = VersionUtilities.WildCardParse(version);

            // Assert
            Assert.IsNull(result, "Expected null when input is an invalid version string.");
        }

        [Test]
        public void ParseWildCard_InvalidWildcardVersion3_ReturnsNull()
        {
            // Arrange
            string version = "2019.*.1";

            // Act
            Version result = VersionUtilities.WildCardParse(version);

            // Assert
            Assert.IsNull(result, "Expected null when input is an invalid version string.");
        }

        [Test]
        public void ParseWildCard_InvalidCharacters1_ReturnsNull()
        {
            // Arrange
            string version = "a.*";

            // Act
            Version result = VersionUtilities.WildCardParse(version);

            // Assert
            Assert.IsNull(result, "Expected null when input is an invalid version string.");
        }

        [Test]
        public void ParseWildCard_InvalidCharacters2_ReturnsNull()
        {
            // Arrange
            string version = "a.1.*";

            // Act
            Version result = VersionUtilities.WildCardParse(version);

            // Assert
            Assert.IsNull(result, "Expected null when input is an invalid version string.");
        }

        [Test]
        public void ParseWildCard_InvalidCharacters3_ReturnsNull()
        {
            // Arrange
            string version = "2016.a.*";

            // Act
            Version result = VersionUtilities.WildCardParse(version);

            // Assert
            Assert.IsNull(result, "Expected null when input is an invalid version string.");
        }

        [Test]
        public void ParseWildCard_ValidString1_ReturnsParsedVersion()
        {
            // Arrange
            string version = "2019.1.*";

            // Act
            Version result = VersionUtilities.WildCardParse(version);

            // Assert
            Assert.AreEqual(new Version(2019, 1, 2147483647), result, "Expected version '2019.1.2147483647' when input is '2019.1.*'.");
        }

        [Test]
        public void ParseWildCard_ValidString2_ReturnsParsedVersion()
        {
            // Arrange
            string version = "2019.*";

            // Act
            Version result = VersionUtilities.WildCardParse(version);

            // Assert
            Assert.AreEqual(new Version(2019, 2147483647, 0), result, "Expected version '2019.2147483647.0' when input is '2019.*'.");
        }

        [Test]
        public void ParseWildCard_TwoAndThreePartVersionConsistency_ReturnsParsedVersions()
        {
            // Arrange & Act
            Version twoPartResult = VersionUtilities.WildCardParse("2021.*");
            Version threePartResult = VersionUtilities.WildCardParse("2021.2.*");

            // Assert
            Assert.AreEqual(new Version(2021, 2147483647, 0), twoPartResult, "Expected version '2021.2147483647.0' for input '2021.*'.");
            Assert.AreEqual(new Version(2021, 2, 2147483647), threePartResult, "Expected version '2021.2.2147483647' for input '2021.2.*'.");
        }

        [Test]
        public void ParseWildCard_TooManyComponents_ReturnsNull()
        {
            // Arrange
            string version = "1.2.3.4.*";

            // Act
            Version result = VersionUtilities.WildCardParse(version);

            // Assert
            Assert.IsNull(result, "Expected null when input has too many components '1.2.3.4.*'.");
        }

    }

}
