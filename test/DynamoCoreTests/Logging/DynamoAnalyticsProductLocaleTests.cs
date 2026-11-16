using System.Globalization;
using Dynamo.Logging;
using NUnit.Framework;

namespace Dynamo.Tests.Loggings
{
    /// <summary>
    /// The product locale Dynamo registers with ADP is how the Autodesk Assistant panel learns the
    /// language to show. Without it the panel follows the OS language (DYN-11035).
    /// </summary>
    [TestFixture]
    class DynamoAnalyticsProductLocaleTests
    {
        [TestCase("en-US")]
        [TestCase("fr-FR")]
        [TestCase("zh-CN")]
        [TestCase("pt-BR")]
        public void GetProductLocale_NamedCulture_ReturnsItsName(string cultureName)
        {
            Assert.AreEqual(cultureName, DynamoAnalyticsClient.GetProductLocale(new CultureInfo(cultureName)));
        }

        [Test]
        public void GetProductLocale_InvariantCulture_ReturnsNull()
        {
            Assert.IsNull(DynamoAnalyticsClient.GetProductLocale(CultureInfo.InvariantCulture));
        }

        [Test]
        public void GetProductLocale_Null_ReturnsNull()
        {
            Assert.IsNull(DynamoAnalyticsClient.GetProductLocale(null));
        }
    }
}
