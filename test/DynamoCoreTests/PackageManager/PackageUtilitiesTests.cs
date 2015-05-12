using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Dynamo.Utilities;

using Greg.Responses;

using NUnit.Framework;

namespace Dynamo.PackageManager.Tests
{
    [TestFixture]
    class PackageUtilitiesTests
    {
        [Test]
        [Category("UnitTests")]
        public void FilterFuturePackages_IEnumerableTuplePackageHeaderPackageVersion_ReturnsCorrectResults()
        {
            var pkgVersion063 = new PackageVersion()
            {
                engine_version = "0.6.3"
            };

            var pkgVersion080 = new PackageVersion()
            {
                engine_version = "0.8.0"
            };

            var pkg063 = new PackageHeader()
            {
                name = "063",
                versions = new List<PackageVersion>()
                {
                    pkgVersion063
                }
            };

            var pkg080 = new PackageHeader()
            {
                name = "080",
                versions = new List<PackageVersion>()
                {
                    pkgVersion080
                }
            };

            var pkgList = new List<Tuple<PackageHeader, PackageVersion>>()
            {
                new Tuple<PackageHeader, PackageVersion>(pkg063, pkgVersion063),
                new Tuple<PackageHeader, PackageVersion>(pkg080, pkgVersion080),
            };

            var dynamoVersion = Version.Parse("0.7.1");
            var filteredPkgList = pkgList.FilterFuturePackages(dynamoVersion);

            Assert.AreEqual(1, filteredPkgList.Count());
            Assert.AreEqual("080", filteredPkgList.First().Item1.name);
        }

    }
}
