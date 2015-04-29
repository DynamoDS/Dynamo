using System;
using System.Linq;

using NUnit.Framework;

namespace Dynamo.Tests
{
    class PackageManagerClientTests : DynamoModelTestBase
    {
        // All of these tests do not require authentication

        #region DownloadAndInstall

        [Test]
        public void DownloadAndInstallSucceedsForValidPackage()
        {
            Assert.Inconclusive("Finish me");
        }

        [Test]
        public void DownloadAndInstallFailsForInvalidPackage()
        {
            Assert.Inconclusive("Finish me");
        }
        #endregion

        [Test]
        public void SearchWithEmptyQueryReturnsAllPackagesInSortedOrder()
        {
            Assert.Inconclusive("Finish me");
        }


        [Test]
        public void ListAllReturnsAllPackages()
        {
            var elements = this.CurrentDynamoModel.PackageManagerClient.ListAll();
            Assert.AreNotEqual(0, elements.Count());
            Console.WriteLine(elements.Count());
        }

        [Test]
        public void SearchReturnsValidResults()
        {
            Assert.Inconclusive("Finish me");
        }

        [Test]
        public void CannotDownloadInvalidPackage()
        {
            Assert.Inconclusive("Finish me");
        }

        [Test]
        public void DownloadPackageHeaderSucceedsForValidPackage()
        {
            Assert.Inconclusive("Finish me");
        }

        [Test]
        public void DownloadPackageHeaderFailsForForInvalidPackage()
        {
            Assert.Inconclusive("Finish me");
        }

    }
}