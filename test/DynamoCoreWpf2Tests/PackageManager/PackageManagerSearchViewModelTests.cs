using System;
using System.Collections.Generic;
using Greg.Responses;
using NUnit.Framework;

namespace Dynamo.PackageManager.Wpf.Tests
{
    class PackageManagerSearchViewModelTests
    {
        [Test]
        public void FormatPackageVersionList_CorrectlyFormatsASingleElement()
        {
            IEnumerable<Tuple<PackageHeader, PackageVersion>> l = new[]
            {
                new Tuple<PackageHeader, PackageVersion>(
                    new PackageHeader()
                    {
                        name = "Foo"
                    },
                    new PackageVersion()
                    {
                        version = "0.1.0"
                    })
            };

            var e =
@"Foo 0.1.0";

            Assert.AreEqual(e, PackageManagerSearchViewModel.FormatPackageVersionList(l));
        }

        [Test]
        public void FormatPackageVersionList_CorrectlyFormatsMultipleElements()
        {
            IEnumerable<Tuple<PackageHeader, PackageVersion>> l = new []
            {
                new Tuple<PackageHeader, PackageVersion>(
                    new PackageHeader()
                    {
                        name = "Foo"
                    },
                    new PackageVersion()
                    {
                        version = "0.1.0"
                    }),
                new Tuple<PackageHeader, PackageVersion>(
                    new PackageHeader()
                    {
                        name = "Bar"
                    },
                    new PackageVersion()
                    {
                        version = "1.2.3"
                    })
            };

            var e =
@"Foo 0.1.0
Bar 1.2.3";

            Assert.AreEqual(e, PackageManagerSearchViewModel.FormatPackageVersionList(l));
        }

    }
}
