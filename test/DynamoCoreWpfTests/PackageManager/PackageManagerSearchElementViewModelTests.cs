using System;
using System.Collections.Generic;
using System.Linq;
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
        /// A mock-up version of the compatibility map received via Greg
        /// </summary>
        private static Dictionary<string, Dictionary<string, string>> compatibilityMap = new Dictionary<string, Dictionary<string, string>>
        {
            { "Revit", new Dictionary<string, string> {
                {"2016", "1.3.2"}, {"2017", "2.0.2"}, {"2018", "2.0.2"}, {"2019", "2.0.2"},
                {"2020", "2.1.0"}, {"2020.1", "2.2.1"}, {"2020.2", "2.3.0"}, {"2021", "2.5.2"},
                {"2021.1", "2.6.1"}, {"2022", "2.10.1"}, {"2022.1", "2.12.0"}, {"2023", "2.13.1"},
                {"2023.1", "2.16.1"}, {"2023.1.3", "2.16.2"}, {"2024", "2.17.0"}, {"2024.1", "2.18.1"},
                {"2024.2", "2.19.3"}, {"2025", "3.0.3"}, {"2025.1", "3.0.3"}, {"2025.2", "3.2.1"}
            }},
            { "Civil3D", new Dictionary<string, string> {
                {"2020", "2.1.1"}, {"2020.1", "2.2.0"}, {"2020.2", "2.4.1"}, {"2021", "2.5.2"},
                {"2022", "2.10.1"}, {"2023", "2.13.1"}, {"2024", "2.17.1"}, {"2024.1", "2.18.1"},
                {"2024.2", "2.18.1"}, {"2024.3", "2.19"}, {"2025", "3.0.3"}, {"2025.1", "3.2.2"}
            }}
        };

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
        /// This unit test will validate that after we set the hostName filters in the package search, we will get an intersection of the results (instead of a union)
        /// </summary>
        [Test]
        public void PackageSearchDialogSearchIntersectAgainstHostFilters()
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
                new FilterEntry(advSteelFilterName, Dynamo.Wpf.Properties.Resources.PackageFilterByHost, Dynamo.Wpf.Properties.Resources.PackageHostDependencyFilterContextItem, packageManagerSearchViewModel) { OnChecked = true },
                new FilterEntry(formItFilterName, Dynamo.Wpf.Properties.Resources.PackageFilterByHost, Dynamo.Wpf.Properties.Resources.PackageHostDependencyFilterContextItem, packageManagerSearchViewModel) { OnChecked = true },
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
                packageManagerSearchViewModel.LastSync.Add(result.SearchElementModel);
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

        /// <summary>
        /// This unit test will validate the correctness of the Status filter, where
        /// `New` and `Updated` filters are mutually exclusive
        /// `Deprecated` results are excluded, unless the filter is turned on
        /// </summary>
        [Test]
        public void PackageSearchDialogSearchTestStatusFilters()
        {
            //Arrange
            int numberOfPackages = 9;
            string packageId = "c5ecd20a-d41c-4e0c-8e11-8ddfb953d77f";
            string packageVersionNumber = "1.0.0.0";
            string newAndUpdatedPackageCreatedDateString = DateTime.Now.ToString("yyyy - MM - ddTHH:mm:ss.ffffff K");
            string activePackageCreatedDateString = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).ToString("yyyy - MM - ddTHH:mm:ss.ffffff K");
            string advSteelFilterName = "Advance Steel";
            string formItFilterName = "FormIt";

            //New Packages
            List<string> newPackagesName = new List<string> { "DynamoIronPython2.7", "dynamo", "Celery for Dynamo 2.5" };
            //Updated Packages
            List<string> updatedPackagesName = new List<string> { "DynamoIronPython2.7", "dynamo", "mise en barre", "Test-PackageDependencyFilter" };
            //Deprecated Packages
            List<string> deprecatedPackagesName = new List<string> { "DynamoTestPackage1", "DynamoTestPackage2" };

            var mockGreg = new Mock<IGregClient>();
            var clientmock = new Mock<PackageManagerClient>(mockGreg.Object, MockMaker.Empty<IPackageUploadBuilder>(), string.Empty);
            var pmCVM = new Mock<PackageManagerClientViewModel>(ViewModel, clientmock.Object) { CallBase = true };

            var packageManagerSearchViewModel = new PackageManagerSearchViewModel(pmCVM.Object);
            packageManagerSearchViewModel.RegisterTransientHandlers();

            //Adds the NonHost filters 
            packageManagerSearchViewModel.NonHostFilter = new List<FilterEntry>
            {
                new FilterEntry(Dynamo.Wpf.Properties.Resources.PackageManagerPackageNew,
                                Dynamo.Wpf.Properties.Resources.PackageFilterByStatus,
                                Dynamo.Wpf.Properties.Resources.PackageFilterNewTooltip,
                                packageManagerSearchViewModel) { OnChecked = false },
                new FilterEntry(Dynamo.Wpf.Properties.Resources.PackageManagerPackageUpdated,
                                Dynamo.Wpf.Properties.Resources.PackageFilterByStatus,
                                Dynamo.Wpf.Properties.Resources.PackageFilterUpdatedTooltip,
                                packageManagerSearchViewModel) { OnChecked = false },
                new FilterEntry(Dynamo.Wpf.Properties.Resources.PackageManagerPackageDeprecated,
                                Dynamo.Wpf.Properties.Resources.PackageFilterByStatus,
                                Dynamo.Wpf.Properties.Resources.PackageFilterDeprecatedTooltip,
                                packageManagerSearchViewModel) { OnChecked = false },
                new FilterEntry(Dynamo.Wpf.Properties.Resources.PackageSearchViewContextMenuFilterDependencies,
                                Dynamo.Wpf.Properties.Resources.PackageFilterByDependency,
                                Dynamo.Wpf.Properties.Resources.PackageFilterHasDependenciesTooltip,
                                packageManagerSearchViewModel) { OnChecked = false },
                new FilterEntry(Dynamo.Wpf.Properties.Resources.PackageSearchViewContextMenuFilterNoDependencies,
                                Dynamo.Wpf.Properties.Resources.PackageFilterByDependency,
                                Dynamo.Wpf.Properties.Resources.PackageFilterHasNoDependenciesTooltip,
                                packageManagerSearchViewModel) { OnChecked = false },
            };

            //Adding new packages
            foreach (var package in newPackagesName)
            {
                var tmpPackageVersion = new PackageVersion { version = packageVersionNumber, host_dependencies = new List<string> { formItFilterName }, created = newAndUpdatedPackageCreatedDateString };
                var tmpPackage = new PackageManagerSearchElementViewModel(new PackageManagerSearchElement(new PackageHeader()
                {
                    _id = packageId,
                    name = package,
                    num_versions = 1,
                    deprecated = false,
                    versions = new List<PackageVersion> { tmpPackageVersion },
                    host_dependencies = new List<string> { formItFilterName },
                }), false);
                packageManagerSearchViewModel.AddToSearchResults(tmpPackage);
            }

            //Adding updated packages
            foreach (var package in updatedPackagesName)
            {
                var tmpPackageVersion = new PackageVersion { version = packageVersionNumber, host_dependencies = new List<string> { advSteelFilterName }, created = activePackageCreatedDateString };
                var tmpPackageUpdatedVersion = new PackageVersion { version = packageVersionNumber.Replace('1','2'), host_dependencies = new List<string> { advSteelFilterName }, created = newAndUpdatedPackageCreatedDateString };
                var tmpPackage = new PackageManagerSearchElementViewModel(new PackageManagerSearchElement(new PackageHeader()
                {
                    _id = packageId,
                    name = package,
                    num_versions = 2,
                    deprecated = false,
                    versions = new List<PackageVersion> { tmpPackageVersion, tmpPackageUpdatedVersion },
                    host_dependencies = new List<string> { advSteelFilterName },
                }), false);
                packageManagerSearchViewModel.AddToSearchResults(tmpPackage);
            }

            //Adding deprecated
            foreach (var package in deprecatedPackagesName)
            {
                var tmpPackageVersion = new PackageVersion { version = packageVersionNumber, host_dependencies = new List<string> { advSteelFilterName, formItFilterName }, created = activePackageCreatedDateString };
                var tmpPackage = new PackageManagerSearchElementViewModel(new PackageManagerSearchElement(new PackageHeader()
                {
                    _id = packageId,
                    name = package,
                    num_versions = 1,
                    deprecated = true,
                    versions = new List<PackageVersion> { tmpPackageVersion },
                    host_dependencies = new List<string> { advSteelFilterName, formItFilterName },
                }), false);
                packageManagerSearchViewModel.AddToSearchResults(tmpPackage);
            }

            //We need to add the PackageManagerSearchElementViewModel because otherwise the search will crash
            packageManagerSearchViewModel.LastSync = new List<PackageManagerSearchElement>();
            foreach (var result in packageManagerSearchViewModel.SearchResults)
            {
                packageManagerSearchViewModel.LastSync.Add(result.SearchElementModel);
            }

            //Validate the total added packages match
            Assert.That(packageManagerSearchViewModel.SearchResults.Count == numberOfPackages);

            //Act
            //Check the Deprecated filter
            packageManagerSearchViewModel.NonHostFilter[2].OnChecked = true;
            packageManagerSearchViewModel.NonHostFilter[2].FilterCommand.Execute(string.Empty);
            Assert.IsNotNull(packageManagerSearchViewModel.SearchResults, "There was no results");
            Assert.That(packageManagerSearchViewModel.SearchResults.Count > 0, "There was no results");
            Assert.That(packageManagerSearchViewModel.SearchResults.Count == deprecatedPackagesName.Count, "The search results are not getting the deprecated packages");

            packageManagerSearchViewModel.NonHostFilter[2].OnChecked = false;
            packageManagerSearchViewModel.NonHostFilter[2].FilterCommand.Execute(string.Empty);
            Assert.IsNotNull(packageManagerSearchViewModel.SearchResults, "There was no results");
            Assert.That(packageManagerSearchViewModel.SearchResults.Count > 0, "There was no results");
            Assert.That(packageManagerSearchViewModel.SearchResults.Count == numberOfPackages - deprecatedPackagesName.Count, "The search results are not getting the deprecated packages");

            //Check the New Filter
            packageManagerSearchViewModel.NonHostFilter[0].OnChecked = true;
            packageManagerSearchViewModel.NonHostFilter[0].FilterCommand.Execute(string.Empty);
            Assert.IsNotNull(packageManagerSearchViewModel.SearchResults, "There was no results");
            Assert.That(packageManagerSearchViewModel.SearchResults.Count > 0, "There was no results");
            Assert.That(packageManagerSearchViewModel.SearchResults.Count == newPackagesName.Count, "The search results are not getting the new packages");

            //Check the Updated filter
            packageManagerSearchViewModel.NonHostFilter[0].OnChecked = false;
            packageManagerSearchViewModel.NonHostFilter[1].OnChecked = true;
            packageManagerSearchViewModel.NonHostFilter[1].FilterCommand.Execute(string.Empty);
            Assert.IsNotNull(packageManagerSearchViewModel.SearchResults, "There was no results");
            Assert.That(packageManagerSearchViewModel.SearchResults.Count > 0, "There was no results");
            Assert.That(packageManagerSearchViewModel.SearchResults.Count == updatedPackagesName.Count, "The search results are not getting the updated packages");

            //Check the exclusivity of the New and Updated filters
            packageManagerSearchViewModel.NonHostFilter[0].OnChecked = true;
            packageManagerSearchViewModel.NonHostFilter[1].OnChecked = true;
            packageManagerSearchViewModel.NonHostFilter[1].FilterCommand.Execute(string.Empty);
            Assert.That(packageManagerSearchViewModel.SearchResults.Count == 0, "There was some results incorrectly going through past the filters");
        }


        /// <summary>
        /// This unit test will validate the correctness of the Dependency filter, where
        /// `Has Dependency` and `Has No Dependency` filters are mutually exclusive
        /// </summary>
        [Test]
        public void PackageSearchDialogSearchTestDependencyFilters()
        {
            //Arrange
            int numberOfPackages = 7;
            string packageId = "c5ecd20a-d41c-4e0c-8e11-8ddfb953d77f";
            string packageVersionNumber = "1.0.0.0";
            string packageCreatedDateString = "2016 - 12 - 02T13:13:20.135000 + 00:00";
            string advSteelFilterName = "Advance Steel";
            string formItFilterName = "FormIt";

            //Dependency Packages
            List<string> dependencyPackagesName = new List<string> { "DynamoIronPython2.7", "dynamo", "Celery for Dynamo 2.5" };
            //No dependency Packages
            List<string> noDependencyPackagesName = new List<string> { "DynamoIronPython2.7", "dynamo", "mise en barre", "Test-PackageDependencyFilter" };

            var mockGreg = new Mock<IGregClient>();
            var clientmock = new Mock<PackageManagerClient>(mockGreg.Object, MockMaker.Empty<IPackageUploadBuilder>(), string.Empty);
            var pmCVM = new Mock<PackageManagerClientViewModel>(ViewModel, clientmock.Object) { CallBase = true };

            var packageManagerSearchViewModel = new PackageManagerSearchViewModel(pmCVM.Object);
            packageManagerSearchViewModel.RegisterTransientHandlers();

            //Adds the NonHost filters 
            packageManagerSearchViewModel.NonHostFilter = new List<FilterEntry>
            {
                new FilterEntry(Dynamo.Wpf.Properties.Resources.PackageManagerPackageNew,
                                Dynamo.Wpf.Properties.Resources.PackageFilterByStatus,
                                Dynamo.Wpf.Properties.Resources.PackageFilterNewTooltip,
                                packageManagerSearchViewModel) { OnChecked = false },
                new FilterEntry(Dynamo.Wpf.Properties.Resources.PackageManagerPackageUpdated,
                                Dynamo.Wpf.Properties.Resources.PackageFilterByStatus,
                                Dynamo.Wpf.Properties.Resources.PackageFilterUpdatedTooltip,
                                packageManagerSearchViewModel) { OnChecked = false },
                new FilterEntry(Dynamo.Wpf.Properties.Resources.PackageManagerPackageDeprecated,
                                Dynamo.Wpf.Properties.Resources.PackageFilterByStatus,
                                Dynamo.Wpf.Properties.Resources.PackageFilterDeprecatedTooltip,
                                packageManagerSearchViewModel) { OnChecked = false },
                new FilterEntry(Dynamo.Wpf.Properties.Resources.PackageSearchViewContextMenuFilterDependencies,
                                Dynamo.Wpf.Properties.Resources.PackageFilterByDependency,
                                Dynamo.Wpf.Properties.Resources.PackageFilterHasDependenciesTooltip,
                                packageManagerSearchViewModel) { OnChecked = false },
                new FilterEntry(Dynamo.Wpf.Properties.Resources.PackageSearchViewContextMenuFilterNoDependencies,
                                Dynamo.Wpf.Properties.Resources.PackageFilterByDependency,
                                Dynamo.Wpf.Properties.Resources.PackageFilterHasNoDependenciesTooltip,
                                packageManagerSearchViewModel) { OnChecked = false },
            };


            packageManagerSearchViewModel.NonHostFilter.ForEach(f => f.PropertyChanged += packageManagerSearchViewModel.filter_PropertyChanged);

            //Adding packages with no dependencies
            foreach (var package in noDependencyPackagesName)
            {
                var tmpPackageVersion = new PackageVersion { version = packageVersionNumber,
                    host_dependencies = new List<string> { formItFilterName },
                    created = packageCreatedDateString,
                    direct_dependency_ids = new List<Dependency> { new Dependency() }
                };
                var tmpPackage = new PackageManagerSearchElementViewModel(new PackageManagerSearchElement(new PackageHeader()
                {
                    _id = packageId,
                    name = package,
                    num_dependents = 1,
                    versions = new List<PackageVersion> { tmpPackageVersion },
                    host_dependencies = new List<string> { formItFilterName },
                }), false);
                packageManagerSearchViewModel.AddToSearchResults(tmpPackage);
            }

            //Adding packages with dependencies
            foreach (var package in dependencyPackagesName)
            {
                var tmpPackageVersion = new PackageVersion { version = packageVersionNumber,
                    host_dependencies = new List<string> { advSteelFilterName },
                    created = packageCreatedDateString,
                    direct_dependency_ids = new List<Dependency> { new Dependency(), new Dependency() }
                };
                var tmpPackage = new PackageManagerSearchElementViewModel(new PackageManagerSearchElement(new PackageHeader()
                {
                    _id = packageId,
                    name = package,
                    num_dependents = 2,
                    versions = new List<PackageVersion> { tmpPackageVersion },
                    host_dependencies = new List<string> { advSteelFilterName },
                }), false);
                packageManagerSearchViewModel.AddToSearchResults(tmpPackage);
            }

            //We need to add the PackageManagerSearchElementViewModel because otherwise the search will crash
            packageManagerSearchViewModel.LastSync = new List<PackageManagerSearchElement>();
            foreach (var result in packageManagerSearchViewModel.SearchResults)
            {
                packageManagerSearchViewModel.LastSync.Add(result.SearchElementModel);
            }

            //Validate the total added packages match
            Assert.That(packageManagerSearchViewModel.SearchResults.Count == numberOfPackages);

            //Act
            //Check the Dependency filter
            packageManagerSearchViewModel.NonHostFilter[3].OnChecked = true;
            packageManagerSearchViewModel.NonHostFilter[3].FilterCommand.Execute(string.Empty);
            Assert.IsNotNull(packageManagerSearchViewModel.SearchResults, "There was no results");
            Assert.That(packageManagerSearchViewModel.SearchResults.Count > 0, "There was no results");
            Assert.That(packageManagerSearchViewModel.SearchResults.Count == dependencyPackagesName.Count, "The search results are not getting the dependent packages");

            //Check the NoDependency filter
            packageManagerSearchViewModel.NonHostFilter[4].OnChecked = true;
            packageManagerSearchViewModel.NonHostFilter[4].FilterCommand.Execute(string.Empty);
            Assert.IsNotNull(packageManagerSearchViewModel.SearchResults, "There was no results");
            Assert.That(packageManagerSearchViewModel.SearchResults.Count > 0, "There was no results");
            Assert.That(packageManagerSearchViewModel.SearchResults.Count == noDependencyPackagesName.Count, "The search results are not getting the non-dependent packages");

            //Check that the two filters cannot be 'ON' at the same time
            Assert.IsFalse(packageManagerSearchViewModel.NonHostFilter[3].OnChecked);
            packageManagerSearchViewModel.NonHostFilter[3].OnChecked = true;
            packageManagerSearchViewModel.NonHostFilter[3].FilterCommand.Execute(string.Empty);
            Assert.IsFalse(packageManagerSearchViewModel.NonHostFilter[4].OnChecked);
        }

        /// <summary>
        /// This unit test will validate that we can search packages in different languages and they will be found.
        /// </summary>
        [Test]
        public void PackageSearchDialogSearchDifferentLanguage()
        {
            //Arrange
            string packageId = "c5ecd20a-d41c-4e0c-8e11-8ddfb953d77f";
            string packageVersionNumber = "1.0.0.0";
            string packageCreatedDateString = "2016 - 12 - 02T13:13:20.135000 + 00:00";
            string formItFilterName = "FormIt";
            var packageMaintainer = new User(){ username = "DynamoTest", _id = "90-63-17" };

            //Packages list
            List<string> packagesNameDifferentLanguages = new List<string> { "paquete", "упаковка", "包裹" };

            List<PackageHeader> packageHeaders = new List<PackageHeader>();
            var mockGreg = new Mock<IGregClient>();

            var clientmock = new Mock<PackageManagerClient>(mockGreg.Object, MockMaker.Empty<IPackageUploadBuilder>(), string.Empty);
            var pmCVM = new Mock<PackageManagerClientViewModel>(ViewModel, clientmock.Object) { CallBase = true };
            List<PackageManagerSearchElement> cachedPackages = new List<PackageManagerSearchElement>();
            foreach (var packageName in packagesNameDifferentLanguages)
            {
                var tmpPackageVersion = new PackageVersion { version = packageVersionNumber, host_dependencies = new List<string> { formItFilterName }, created = packageCreatedDateString };
                cachedPackages.Add(new PackageManagerSearchElement(new PackageHeader() { name = packageName, versions = new List<PackageVersion> { tmpPackageVersion } }));
            }
            pmCVM.SetupProperty(p => p.CachedPackageList, cachedPackages);

            var packageManagerSearchViewModel = new PackageManagerSearchViewModel(pmCVM.Object);
            packageManagerSearchViewModel.RegisterTransientHandlers();

            //Adding packages
            foreach (var package in packagesNameDifferentLanguages)
            {
                var tmpPackageVersion = new PackageVersion { version = packageVersionNumber, host_dependencies = new List<string> { formItFilterName }, created = packageCreatedDateString };
                var tmpPackage = new PackageManagerSearchElementViewModel(new PackageManagerSearchElement(new PackageHeader()
                {
                    _id = packageId,
                    name = package,
                    versions = new List<PackageVersion> { tmpPackageVersion },
                    host_dependencies = new List<string> { formItFilterName },
                    maintainers = new List<User> { packageMaintainer },
                }), false);
                packageManagerSearchViewModel.AddToSearchResults(tmpPackage);
            }

            foreach (var package in packageManagerSearchViewModel.SearchResults)
            {
                var iDoc = packageManagerSearchViewModel.LuceneUtility.InitializeIndexDocumentForPackages();
                packageManagerSearchViewModel.AddPackageToSearchIndex(package.SearchElementModel, iDoc);
            }

            packageManagerSearchViewModel.LuceneUtility.CommitWriterChanges();

            //Act - Searching for packages
            var resultingNodesChinese = packageManagerSearchViewModel.Search("包裹", true);
            var resultingNodesRussian = packageManagerSearchViewModel.Search("упаковка", true);
            var resultingNodesSpanish = packageManagerSearchViewModel.Search("paquete", true);

            //Assert
            //Validates that the packages were found
            Assert.That(resultingNodesChinese.Count(), Is.EqualTo(1), "There was no results");
            Assert.That(resultingNodesRussian.Count(), Is.EqualTo(1), "There was no results");
            Assert.That(resultingNodesSpanish.Count(), Is.EqualTo(1), "There was no results");
        }

        /// <summary>
        /// This unit test will validate that the search order will not reset on search text clear.
        /// </summary>
        [Test]
        public void PackageSearchOrderAfterTextReset()
        {
            //Arrange
            string packageId = "c5ecd20a-d41c-4e0c-8e11-8ddfb953d77f";
            string packageVersionNumber = "1.0.0.0";
            string packageCreatedDateString = "2016 - 12 - 02T13:13:20.135000 + 00:00";
            string formItFilterName = "FormIt";

            //Packages list
            List<string> packageNames = new List<string> { "package 1", "package 2", "package 3", "package 4" };
            List<int> packagesDownloads = new List<int> { 100, 400, 300, 200 };
            List<int> packagesVotes = new List<int> { 50, 60, 90, 40 };

            var mockGreg = new Mock<IGregClient>();
            var clientmock = new Mock<PackageManagerClient>(mockGreg.Object, MockMaker.Empty<IPackageUploadBuilder>(), string.Empty);
            var pmCVM = new Mock<PackageManagerClientViewModel>(ViewModel, clientmock.Object) { CallBase = true };   
            var packageManagerSearchVM = new PackageManagerSearchViewModel(pmCVM.Object);
            packageManagerSearchVM.RegisterTransientHandlers();

            //Add packages
            for (int i = 0; i < packageNames.Count; i++)
            {
                var tmpPackageVersion = new PackageVersion
                {
                    version = packageVersionNumber,
                    host_dependencies = new List<string> { formItFilterName },
                    created = packageCreatedDateString
                };
                var tmpPackage = new PackageManagerSearchElementViewModel(new PackageManagerSearchElement(new PackageHeader()
                {
                    _id = packageId,
                    name = packageNames[i],
                    versions = new List<PackageVersion> { tmpPackageVersion },
                    votes = packagesVotes[i],
                    downloads = packagesDownloads[i]
                }), false);
                packageManagerSearchVM.AddToSearchResults(tmpPackage);
            }

            //We need to add the PackageManagerSearchElementViewModel because otherwise the search will crash
            packageManagerSearchVM.LastSync = new List<PackageManagerSearchElement>();
            foreach (var result in packageManagerSearchVM.SearchResults)
            {
                packageManagerSearchVM.LastSync.Add(result.SearchElementModel);
            }

            //Act - Sort packages by Downloads in descending order
            packageManagerSearchVM.SortingKey = PackageSortingKey.Downloads;
            packageManagerSearchVM.SortingDirection = PackageSortingDirection.Descending;
            packageManagerSearchVM.Sort();

            //Set search text to a value and then reset
            packageManagerSearchVM.SearchText = "package";
            packageManagerSearchVM.SearchAndUpdateResults();
            packageManagerSearchVM.SearchText = string.Empty;
            packageManagerSearchVM.SearchAndUpdateResults();

            bool isOrderedByDownloads = true;

            for (int i = 0; i < packageManagerSearchVM.SearchResults.Count - 1; i++)
            {
                if (packageManagerSearchVM.SearchResults[i].Downloads < packageManagerSearchVM.SearchResults[i + 1].Downloads)
                {
                    isOrderedByDownloads = false; break;
                }
            }

            //Assert - validate order by Downloads
            Assert.IsTrue(isOrderedByDownloads && packageManagerSearchVM.SearchResults.Count != 0);

            //Act - Sort packages by Votes in ascending order
            packageManagerSearchVM.SortingKey = PackageSortingKey.Votes;
            packageManagerSearchVM.SortingDirection = PackageSortingDirection.Ascending;
            packageManagerSearchVM.Sort();

            //Set search text to a value and then reset
            packageManagerSearchVM.SearchText = "package";
            packageManagerSearchVM.SearchAndUpdateResults();
            packageManagerSearchVM.SearchText = string.Empty;
            packageManagerSearchVM.SearchAndUpdateResults();

            bool isOrderedByVotes = true;

            for (int i = 0; i < packageManagerSearchVM.SearchResults.Count - 1; i++)
            {
                if (packageManagerSearchVM.SearchResults[i].Votes > packageManagerSearchVM.SearchResults[i + 1].Votes)
                {
                    isOrderedByVotes = false; break;
                }
            }

            //Assert - validate order by Votes
            Assert.IsTrue(isOrderedByVotes && packageManagerSearchVM.SearchResults.Count != 0);
        }

        /// <summary>
        /// This unit test will validate that the search for package with whitespace in the name.
        /// </summary>
        [Test]
        public void PackageSearchWithWhitespaceInName()
        {
            var packageToBeSearched = "Dynamo Samples";
            var packagesListNames =  new List<string> { packageToBeSearched, "archi-lab.net", "LunchBox for Dynamo", "DynamoSap", "TuneUp" };
            string packageId = "c5ecd20a-d41c-4e0c-8e11-8ddfb953d77f";
            string packageVersionNumber = "1.0.0.0";
            string packageCreatedDateString = "2024 - 10 - 02T13:13:20.135000 + 00:00";
            var packageMaintainer = new User() { username = "DynamoTest", _id = "90-63-17" };

            List<PackageHeader> packageHeaders = new List<PackageHeader>();
            var mockGreg = new Mock<IGregClient>();

            var clientmock = new Mock<PackageManagerClient>(mockGreg.Object, MockMaker.Empty<IPackageUploadBuilder>(), string.Empty);
            var pmCVM = new Mock<PackageManagerClientViewModel>(ViewModel, clientmock.Object) { CallBase = true };
            List<PackageManagerSearchElement> cachedPackages = new List<PackageManagerSearchElement>();
            foreach (var packageName in packagesListNames)
            {
                var tmpPackageVersion = new PackageVersion { version = packageVersionNumber, host_dependencies = new List<string> { "FormIt" }, created = packageCreatedDateString };
                cachedPackages.Add(new PackageManagerSearchElement(new PackageHeader() { name = packageName, versions = new List<PackageVersion> { tmpPackageVersion } }));
            }
            pmCVM.SetupProperty(p => p.CachedPackageList, cachedPackages);

            var packageManagerSearchVM = new PackageManagerSearchViewModel(pmCVM.Object);
            packageManagerSearchVM.RegisterTransientHandlers();

            //Adding packages
            foreach (var package in packagesListNames)
            {
                var tmpPackageVersion = new PackageVersion
                {
                    version = packageVersionNumber,
                    created = packageCreatedDateString
                };
                var tmpPackage = new PackageManagerSearchElementViewModel(new PackageManagerSearchElement(new PackageHeader()
                {
                    _id = packageId,
                    name = package,
                    versions = new List<PackageVersion> { tmpPackageVersion },
                    maintainers = new List<User> { packageMaintainer },
                }), false);
                packageManagerSearchVM.AddToSearchResults(tmpPackage);
            }

            foreach (var package in packageManagerSearchVM.SearchResults)
            {
                var iDoc = packageManagerSearchVM.LuceneUtility.InitializeIndexDocumentForPackages();
                packageManagerSearchVM.AddPackageToSearchIndex(package.SearchElementModel, iDoc);
            }

            packageManagerSearchVM.LuceneUtility.CommitWriterChanges();

            var packagesSearchResult = packageManagerSearchVM.Search(packageToBeSearched, true);

            //Validates that the Search returned results and that the first one is "Dynamo Samples"
            Assert.IsTrue(packagesSearchResult != null);
            Assert.IsTrue(packagesSearchResult.Count() == 1);
            Assert.IsTrue(packagesSearchResult.FirstOrDefault().Name == packageToBeSearched);
        }

        [Test]
        public void TestComputeVersionCompatibility()
        {
            //Arrange
            var compatibilityMatrix = new List<Greg.Responses.Compatibility>
            {
                new Greg.Responses.Compatibility { name = "Dynamo", min = "2.0", max = "2.5" },
                new Greg.Responses.Compatibility { name = "Host", min = "2020", max = "2025" }
            };

            var compatibilityMatrixNoHost = new List<Greg.Responses.Compatibility>
            {
                new Greg.Responses.Compatibility { name = "Dynamo", min = "2.0", max = "2.5" },
            };

            var customDynamoVersion = new Version("2.3");
            var customHostVersion = new Version("2023.0");

            // Act
            var result = PackageManagerSearchElement.CalculateCompatibility(compatibilityMatrix, customDynamoVersion, compatibilityMap, customHostVersion);
            var resultNoHost = PackageManagerSearchElement.CalculateCompatibility(compatibilityMatrixNoHost, customDynamoVersion, compatibilityMap, customHostVersion);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(resultNoHost);
        }

        [Test]
        public void TestComputeIncompatibleVersionCompatibility()
        {
            //Arrange
            var compatibilityMatrixIncompatibleDynamo = new List<Greg.Responses.Compatibility>
            {
                new Greg.Responses.Compatibility { name = "Dynamo", min = "3.0", max = "3.5" },
                new Greg.Responses.Compatibility { name = "Host", min = "2020.0", max = "2025.0" }
            };

            var compatibilityMatrixIncompatibleHost = new List<Greg.Responses.Compatibility>
            {
                new Greg.Responses.Compatibility { name = "Dynamo", min = "2.0", max = "3.5" },
                new Greg.Responses.Compatibility { name = "Host", min = "2020.0", max = "2022.0" }
            };

            var customDynamoVersion = new Version("2.9");
            var customHostVersion = new Version("2023.0");
            var customHostName = "Host";

            // Act
            var resultIncompatibleDynamo = PackageManagerSearchElement.CalculateCompatibility(compatibilityMatrixIncompatibleDynamo, customDynamoVersion, compatibilityMap, customHostVersion, customHostName);
            var resultIncompatibleHost = PackageManagerSearchElement.CalculateCompatibility(compatibilityMatrixIncompatibleHost, customDynamoVersion, compatibilityMap, customHostVersion, customHostName);

            // Assert
            Assert.IsFalse(resultIncompatibleDynamo);
            Assert.IsFalse(resultIncompatibleHost);
        }

        [Test]
        public void TestComputeVersionNoDynamoCompatibility()
        {
            //Arrange
            var compatibilityMatrix = new List<Greg.Responses.Compatibility>
            {
                new Greg.Responses.Compatibility { name = "Revit", min = "2020", max = "2025" }
            };

            var minValueCompatibilityMatrix = new List<Greg.Responses.Compatibility>
            {
                new Greg.Responses.Compatibility { name = "Revit", min = "2020",  }
            };


            var incompleteCompatibilityMatrix = new List<Greg.Responses.Compatibility>
            {
                new Greg.Responses.Compatibility { name = "Revit" }
            };

            var customIncompatibleDynamoVersion = new Version("2.0.2"); // Matches Revit 2019
            var customCompatibleDynamoVersion = new Version("2.13.1");  // Dynamo 2.13.1 matches Revit 2023.0
            var customHostVersion = new Version("2023.0");  // Revit 2023.0 matches Dynamo 2.13.1 
            var hostName = "Revit";

            // Act
            // True - No dynamo compatibility provided, we extract Dynamo versions from Host and check against the current Dynamo Version
            var resultNoDynamoCompatibilityUnderDynamo = PackageManagerSearchElement.CalculateCompatibility(compatibilityMatrix, customCompatibleDynamoVersion, compatibilityMap);
            // True - No dynamo compatibility provided, but we are under a Host
            var resultNoDynamoCompatibilityUnderHost = PackageManagerSearchElement.CalculateCompatibility(compatibilityMatrix, customCompatibleDynamoVersion, compatibilityMap, customHostVersion, hostName);
            // True - We will assume that, if we have 'min' value but no 'max' value, anything above that is OK
            var resultMinValue = PackageManagerSearchElement.CalculateCompatibility(minValueCompatibilityMatrix, customCompatibleDynamoVersion, compatibilityMap);
            // False - looking for support for Dynamo 2.0.2 (matching Revit 2019), but the min Revit version is 2020
            var resultIncompatible = PackageManagerSearchElement.CalculateCompatibility(compatibilityMatrix, customIncompatibleDynamoVersion, compatibilityMap);
            // Null - Not enough information provided to extract DynamoCompatibility, we return 'null' for unknown compatibility
            var resultIncomplete = PackageManagerSearchElement.CalculateCompatibility(incompleteCompatibilityMatrix, customIncompatibleDynamoVersion, compatibilityMap);

            // Assert
            Assert.IsTrue(resultNoDynamoCompatibilityUnderDynamo);
            Assert.IsTrue(resultNoDynamoCompatibilityUnderHost);
            Assert.IsTrue(resultMinValue);
            Assert.IsFalse(resultIncompatible);
        }

        [Test]
        public void TestReverseDynamoCompatibilityFromHost()
        {
            //Arrange
            var compatibilityMatrix = new List<Greg.Responses.Compatibility>
            {
                new Greg.Responses.Compatibility { name = "Revit", min = "2020", max = "2025", versions = new List<string>() { "2016", "2018" } }
            };

            var expectedDynamoCompatibility = new Compatibility()
            {
                name = "Dynamo",
                min = "2.1.0",
                max = "3.0.3",
                versions = new List<string>() { "1.3.2", "2.0.2" }
            };

            var result = PackageManagerSearchElement.GetDynamoCompatibilityFromHost(compatibilityMatrix, compatibilityMap);

            Assert.That(result.name, Is.EqualTo(expectedDynamoCompatibility.name));
            Assert.That(result.min, Is.EqualTo(expectedDynamoCompatibility.min));
            Assert.That(result.max, Is.EqualTo(expectedDynamoCompatibility.max));
            Assert.That(result.versions, Is.EqualTo(expectedDynamoCompatibility.versions));
        }

        [Test]
        public void TestNullCompatibilityMap()
        {
            // Arrange
            var compatibilityMatrix = new List<Greg.Responses.Compatibility>
            {
                new Greg.Responses.Compatibility { name = "Revit", min = "2020", max = "2025", versions = new List<string>() { "2016", "2018" } }
            };

            // Act & Assert: Check that an InvalidOperationException is thrown
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                var result = PackageManagerSearchElement.GetDynamoCompatibilityFromHost(compatibilityMatrix, null);
            });

            // Assert the exception message if needed
            Assert.AreEqual("The compatibility map is not initialized.", exception.Message);
        }
    }
}
