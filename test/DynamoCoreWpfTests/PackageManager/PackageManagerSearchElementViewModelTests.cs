using System.Collections.Generic;
using Dynamo.Controls;
using Dynamo.PackageManager.ViewModels;
using Dynamo.Tests;
using Dynamo.ViewModels;
using Greg;
using Greg.Responses;
using Moq;
using NUnit.Framework;
using SystemTestServices;
using static Dynamo.PackageManager.PackageManagerSearchViewModel;

namespace Dynamo.PackageManager.Wpf.Tests
{
    class PackageManagerSearchElementViewModelTests : SystemTestBase
    {
        /// <summary>
        /// A test to ensure the CanInstall property of a package updates correctly.
        /// </summary>
        [Test]
        public void TestPackageManagerSearchElementCanInstall()
        {
            var name1 = "non-duplicate";
            var name2 = "duplicate";
            var version = "1.0.0";
            string packageCreatedDateString = "2016 - 12 - 02T13:13:20.135000 + 00:00";
            string formItFilterName = "FormIt";

            var mockGreg = new Mock<IGregClient>();
            var clientmock = new Mock<PackageManagerClient>(mockGreg.Object, MockMaker.Empty<IPackageUploadBuilder>(), string.Empty);
            var pmCVM = new Mock<PackageManagerClientViewModel>(ViewModel, clientmock.Object) { CallBase = true }; ;

            var ext = Model.GetPackageManagerExtension();
            var loader = ext.PackageLoader;
            loader.Add(new Package("", name2, version, ""));

            var packageManagerSearchViewModel = new PackageManagerSearchViewModel(pmCVM.Object);
            packageManagerSearchViewModel.RegisterTransientHandlers();

            var tmpPackageVersion = new PackageVersion { version = version, host_dependencies = new List<string> { formItFilterName }, created = packageCreatedDateString };
            var newSE1 = new PackageManagerSearchElementViewModel(new PackageManagerSearchElement(new PackageHeader()
            {
                name = name1,
                versions = new List<PackageVersion> { tmpPackageVersion },
            }), false);

            var newSE2 = new PackageManagerSearchElementViewModel(new PackageManagerSearchElement(new PackageHeader()
            {
                name = name2,
                versions = new List<PackageVersion> { tmpPackageVersion },
            }), false);

            packageManagerSearchViewModel.AddToSearchResults(newSE1);
            packageManagerSearchViewModel.AddToSearchResults(newSE2);

            // Default CanInstall should be true
            Assert.AreEqual(true, newSE1.CanInstall);
            Assert.AreEqual(true, newSE2.CanInstall);

            var dHandle1 = new PackageDownloadHandle()
            {
                Id = name1,
                VersionName = version,
                Name = name1
            };

            var dHandle2 = new PackageDownloadHandle()
            {
                Id = name2,
                VersionName = version,
                Name = name2
            };

            pmCVM.Object.Downloads.Add(dHandle1);
            pmCVM.Object.Downloads.Add(dHandle2);

            Assert.AreEqual(true, newSE1.CanInstall);
            Assert.AreEqual(true, newSE2.CanInstall);

            dHandle1.DownloadState = PackageDownloadHandle.State.Downloading;
            Assert.AreEqual(false, newSE1.CanInstall);

            dHandle1.DownloadState = PackageDownloadHandle.State.Downloaded;
            Assert.AreEqual(false, newSE1.CanInstall);

            dHandle1.DownloadState = PackageDownloadHandle.State.Error;
            dHandle2.DownloadState = PackageDownloadHandle.State.Downloading;

            Assert.AreEqual(true, newSE1.CanInstall);
            Assert.AreEqual(false, newSE2.CanInstall);

            dHandle1.DownloadState = PackageDownloadHandle.State.Installing;
            dHandle2.DownloadState = PackageDownloadHandle.State.Downloaded;
            Assert.AreEqual(false, newSE1.CanInstall);
            Assert.AreEqual(false, newSE2.CanInstall);

            // Simulate that the package corresponding to name1 was added successfully
            var package1 = new Package("", name1, version, "") { };
            package1.SetAsLoaded();
            loader.Add(package1);

            dHandle1.DownloadState = PackageDownloadHandle.State.Installed;
            dHandle2.DownloadState = PackageDownloadHandle.State.Installed;

            Assert.AreEqual(false, newSE1.CanInstall);
            Assert.AreEqual(false, newSE2.CanInstall);

            packageManagerSearchViewModel.ClearSearchResults();
        }


        /// <summary>
        /// A test to ensure that the label converted is equal the resources when localized
        /// </summary>
        [Test]
        public void TestPackageManagerInstallStatusByResourceName()
        {
            var name1 = "package";
            var version = "1.0.0";         

            var dHandle1 = new PackageDownloadHandle()
            {
                Id = name1,
                VersionName = version,
                Name = name1
            };

            dHandle1.DownloadState = PackageDownloadHandle.State.Installed;

            var download1State = new PackageDownloadStateToStringConverter().Convert(dHandle1.DownloadState, null, null, null).ToString();
            Assert.IsTrue(download1State.Equals(Dynamo.Wpf.Properties.Resources.PackageDownloadStateInstalled));

            dHandle1.DownloadState = PackageDownloadHandle.State.Installing;

            download1State = new PackageDownloadStateToStringConverter().Convert(dHandle1.DownloadState, null, null, null).ToString();
            Assert.IsTrue(download1State.Equals(Dynamo.Wpf.Properties.Resources.PackageDownloadStateInstalling));

            dHandle1.DownloadState = PackageDownloadHandle.State.Downloaded;

            download1State = new PackageDownloadStateToStringConverter().Convert(dHandle1.DownloadState, null, null, null).ToString();
            Assert.IsTrue(download1State.Equals(Dynamo.Wpf.Properties.Resources.PackageDownloadStateDownloaded));
            
            dHandle1.DownloadState = PackageDownloadHandle.State.Downloading;

            download1State = new PackageDownloadStateToStringConverter().Convert(dHandle1.DownloadState, null, null, null).ToString();
            Assert.IsTrue(download1State.Equals(Dynamo.Wpf.Properties.Resources.PackageDownloadStateDownloading));

            dHandle1.DownloadState = PackageDownloadHandle.State.Error;

            download1State = new PackageDownloadStateToStringConverter().Convert(dHandle1.DownloadState, null, null, null).ToString();
            Assert.IsTrue(download1State.Equals(Dynamo.Wpf.Properties.Resources.PackageDownloadStateError));

            dHandle1.DownloadState = PackageDownloadHandle.State.Uninitialized;

            download1State = new PackageDownloadStateToStringConverter().Convert(dHandle1.DownloadState, null, null, null).ToString();
            Assert.IsTrue(download1State.Equals(Dynamo.Wpf.Properties.Resources.PackageDownloadStateStarting));

            download1State = new PackageDownloadStateToStringConverter().Convert(null, null, null, null).ToString();
            Assert.IsTrue(download1State.Equals(Dynamo.Wpf.Properties.Resources.PackageStateUnknown));
        }

        /// <summary>
        /// This unit test will validate that after we set the filters in the package search, we will get an intersection of the results (instead of a union)
        /// </summary>
        [Test]
        public void PackageSearchDialogSearchIntersectAgainstFilters()
        {
            //Arrange
            int numberOfPackages = 9;
            string packageId = "c5ecd20a-d41c-4e0c-8e11-8ddfb953d77f";
            string packageVersionNumber = "1.0.0.0";
            string packageCreatedDateString = "2016 - 12 - 02T13:13:20.135000 + 00:00";
            string advSteelFilterName = "Advance Steel";
            string formItFilterName = "FormIt";

            //Formit Packages
            List<string> formItPackagesName = new List<string> { "DynamoIronPython2.7", "dynamo", "Celery for Dynamo 2.5" };
            //Advance Steel Packages
            List<string> advanceSteelPackagesName = new List<string> { "DynamoIronPython2.7", "dynamo", "mise en barre", "Test-PackageDependencyFilter" };
            //Advance Steel Packages & Formit
            List<string> intersectionPackagesName = new List<string> { "DynamoTestPackage1", "DynamoTestPackage2" };

            var mockGreg = new Mock<IGregClient>();
            var clientmock = new Mock<PackageManagerClient>(mockGreg.Object, MockMaker.Empty<IPackageUploadBuilder>(), string.Empty);
            var pmCVM = new Mock<PackageManagerClientViewModel>(ViewModel, clientmock.Object) {CallBase=true };

            var packageManagerSearchViewModel = new PackageManagerSearchViewModel(pmCVM.Object);
            packageManagerSearchViewModel.RegisterTransientHandlers();

            //Adds the filters for FormIt and Advance Steel
            packageManagerSearchViewModel.HostFilter = new List<FilterEntry>
            {
                new FilterEntry(advSteelFilterName, packageManagerSearchViewModel) { OnChecked = true },
                new FilterEntry(formItFilterName, packageManagerSearchViewModel) { OnChecked = true },
            };

            //Adding FormIt packages
            foreach (var package in formItPackagesName)
            {
                var tmpPackageVersion = new PackageVersion { version = packageVersionNumber, host_dependencies = new List<string> { formItFilterName }, created = packageCreatedDateString };
                var tmpPackage = new PackageManagerSearchElementViewModel(new PackageManagerSearchElement(new PackageHeader()
                {
                    _id = packageId,
                    name = package,
                    versions = new List<PackageVersion> { tmpPackageVersion },
                    host_dependencies = new List<string> { formItFilterName },
                }), false);
                packageManagerSearchViewModel.AddToSearchResults(tmpPackage);
            }

            //Adding Advance Steel packages
            foreach (var package in advanceSteelPackagesName)
            {
                var tmpPackageVersion = new PackageVersion { version = packageVersionNumber, host_dependencies = new List<string> { advSteelFilterName }, created = packageCreatedDateString };
                var tmpPackage = new PackageManagerSearchElementViewModel(new PackageManagerSearchElement(new PackageHeader()
                {
                    _id = packageId,
                    name = package,
                    versions = new List<PackageVersion> { tmpPackageVersion },
                    host_dependencies = new List<string> { advSteelFilterName },
                }), false);
                packageManagerSearchViewModel.AddToSearchResults(tmpPackage);
            }

            //Adding packages that belong to FormIt and Advance Steel (intersection packages)
            foreach (var package in intersectionPackagesName)
            {
                var tmpPackageVersion = new PackageVersion { version = packageVersionNumber, host_dependencies = new List<string> { advSteelFilterName, formItFilterName }, created = packageCreatedDateString };
                var tmpPackage = new PackageManagerSearchElementViewModel(new PackageManagerSearchElement(new PackageHeader()
                {
                    _id = packageId,
                    name = package,
                    versions = new List<PackageVersion> { tmpPackageVersion },
                    host_dependencies = new List<string> { advSteelFilterName, formItFilterName },
                }), false);
                packageManagerSearchViewModel.AddToSearchResults(tmpPackage);
            }

            //We need to add the PackageManagerSearchElementViewModel because otherwise the search will crash
            packageManagerSearchViewModel.LastSync = new List<PackageManagerSearchElement>();
            foreach (var result in packageManagerSearchViewModel.SearchResults)
            {
                packageManagerSearchViewModel.LastSync.Add(result.Model);
            }

            //Validate the total added packages match
            Assert.That(packageManagerSearchViewModel.SearchResults.Count == numberOfPackages);

            //Act
            //Check the Advance Steel filter
            packageManagerSearchViewModel.HostFilter[0].FilterCommand.Execute(advSteelFilterName);
            //Check the FormIt filter
            packageManagerSearchViewModel.HostFilter[1].FilterCommand.Execute(formItFilterName);

            //Assert
            //Validates that we have results and that the result match the expected 2 packages (intersection)
            Assert.IsNotNull(packageManagerSearchViewModel.SearchResults, "There was no results");
            Assert.That(packageManagerSearchViewModel.SearchResults.Count > 0, "There was no results");
            Assert.That(packageManagerSearchViewModel.SearchResults.Count == intersectionPackagesName.Count, "The search results are not getting the packages intersected");
        }
    }
}
