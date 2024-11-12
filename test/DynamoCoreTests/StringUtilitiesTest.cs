using DynamoUtilities;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class StringUtilitiesTest
    {
        [TestCase("512 MiB", ExpectedResult = "512 MB")]
        [TestCase("1 GiB", ExpectedResult = "1 GB")]
        [TestCase("128 KiB", ExpectedResult = "128 KB")]
        [TestCase("2 TiB", ExpectedResult = "2 TB")]
        [TestCase("0 PiB", ExpectedResult = "0 PB")]
        [TestCase("1024 EiB", ExpectedResult = "1024 EB")]
        public string ConvertToSIFileSize_StandardUnitsWithI_RemovesI(string input)
        {
            return StringUtilities.SimplifyFileSizeUnit (input);
        }

        [TestCase("512 MB", ExpectedResult = "512 MB")]
        [TestCase("1 GB", ExpectedResult = "1 GB")]
        [TestCase("128 KB", ExpectedResult = "128 KB")]
        public string ConvertToSIFileSize_StandardUnitsWithoutI_Unchanged(string input)
        {
            return StringUtilities.SimplifyFileSizeUnit (input);
        }

        [TestCase("Random Text", ExpectedResult = "Random Text")]
        [TestCase("128", ExpectedResult = "128")]
        [TestCase("", ExpectedResult = "")]
        [TestCase(null, ExpectedResult = null)]
        public string ConvertToSIFileSize_NonStandardInputs_Unchanged(string input)
        {
            return StringUtilities.SimplifyFileSizeUnit (input);
        }

        [Test]
        public void ConvertToSIFileSize_NullInput_ReturnsNull()
        {
            Assert.IsNull(StringUtilities.SimplifyFileSizeUnit (null));
        }

        [Test]
        public void ConvertToSIFileSize_EmptyString_ReturnsEmptyString()
        {
            Assert.AreEqual("", StringUtilities.SimplifyFileSizeUnit (""));
        }
    }
}
