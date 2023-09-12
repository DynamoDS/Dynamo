using Dynamo;
using Dynamo.PackageManager;
using NUnit.Framework;

namespace DynamoCoreWpfTests.PackageManager
{
    [TestFixture]
    public class PackageManagerUnitTests : UnitTestBase
    {
        #region PackageManager error translation

        [Test]
        public void ReturnsTranslationForUserIsNotAMaintainerError()
        {
            var message = "The user sending the new package version, DynamoTeam, is not a maintainer of the package Clockwork for Dynamo 1.x";
            var actual = PublishPackageViewModel.TranslatePackageManagerError(message);
            var expected = "The current user, 'DynamoTeam', is not a maintainer of the package 'Clockwork for Dynamo 1.x'.";
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ReturnsTranslationForPackageAlreadyExistsError()
        {
            var message = "A package with the given name and engine already exists.";
            var actual = PublishPackageViewModel.TranslatePackageManagerError(message);
            var expected = "A package with the given name already exists.";
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ReturnsSameAsInputForUnknownMessages()
        {
            var message = "Unknown error";
            var actual = PublishPackageViewModel.TranslatePackageManagerError(message);
            Assert.AreEqual(message, actual);
        }

        #endregion
    }
}
