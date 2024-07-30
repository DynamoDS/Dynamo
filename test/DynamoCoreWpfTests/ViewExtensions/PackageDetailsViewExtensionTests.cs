using System.Collections.Generic;
using System.Linq;
using Dynamo.PackageDetails;
using Dynamo.PackageManager;
using Dynamo.PythonServices;
using Greg.Responses;
using Moq;
using NUnit.Framework;
using PythonNodeModels;
using SystemTestServices;

namespace DynamoCoreWpfTests
{
    public class PackageDetailsViewExtensionTests : SystemTestBase
    {
        private PackageDetailsViewExtension PackageDetailsViewExtension { get; }= new PackageDetailsViewExtension
        {
            PackageManagerExtension = new PackageManagerExtension()
        };
        private static List<User> UsersList { get; } = new List<User>
        {
            new User
            {
                _id = "1",
                username = "User 1"
            },
            new User
            {
                _id = "2",
                username = "User 2"
            }
        };
        private static List<string> Hosts { get; } = new List<string> {"Revit", "Civil3D"};
        private static List<Dependency> Dependencies { get; } = new List<Dependency>
        {
            new Dependency {_id = string.Empty, name = "Dependency"},
            new Dependency {_id = string.Empty, name = "Dependency"}
        };
        private static List<PackageVersion> PackageVersions = new List<PackageVersion>
        {
            new PackageVersion
            {
                engine_version = "1.0.0",
                created = System.DateTime.Now.ToString(),
                full_dependency_versions = DependencyVersions,
                full_dependency_ids = Dependencies,
                direct_dependency_versions = DependencyVersions,
                direct_dependency_ids = Dependencies,
                host_dependencies = new List<string>(Hosts),
                version = "0.0.1",
                name = "test",
                size = "2.19 MiB",
            },
            new PackageVersion
            {
                engine_version = "1.0.0",
                created = System.DateTime.Now.ToString(),
                full_dependency_versions = DependencyVersions,
                full_dependency_ids = Dependencies,
                direct_dependency_versions = DependencyVersions,
                direct_dependency_ids = Dependencies,
                host_dependencies = new List<string>(Hosts),
                version = "0.0.2",
                name = "test",
                size = "4.19 MiB",
            },
            new PackageVersion
            {
                engine_version = "1.0.0",
                created = System.DateTime.Now.ToString(),
                full_dependency_versions = DependencyVersions,
                full_dependency_ids = Dependencies,
                direct_dependency_versions = DependencyVersions,
                direct_dependency_ids = Dependencies,
                host_dependencies = new List<string>(Hosts),
                version = "0.0.3",
                name = "test",
                size = "5.19 MiB",
            },
        };
        private static List<string> DependencyVersions { get; } = new List<string> {"1", "2", "3"};
        
        /// <summary>
        /// Tests whether a PackageDetailItem detects its dependencies and sets correponding values properly.
        /// These display in the PackageDetailsView versions DataGrid as the 'Host' and 'Python' columns.
        /// </summary>
        [Test]
        public void TestDependencyDetection()
        {
            // Arrange
            string hostName1 = "Revit";
            string hostName2 = "Civil3D";
            List<string> hostNames = new List<string> {hostName1, hostName2};

            PackageVersion packageVersionNoDependencies = new PackageVersion
            {
                host_dependencies = new List<string>(),
                full_dependency_ids = new List<Dependency>()
            };
            PackageVersion packageVersionWithPython2Dependency = new PackageVersion
            {
                host_dependencies = new List<string> { PythonEngineManager.IronPython2EngineName },
                full_dependency_ids = new List<Dependency>()
            };
            PackageVersion packageVersionWithPython3Dependency = new PackageVersion
            {
                host_dependencies = new List<string> { PythonEngineManager.CPython3EngineName },
                full_dependency_ids = new List<Dependency>()
            };
            PackageVersion packageVersionWithHostDependency = new PackageVersion
            {
                host_dependencies = new List<string> {hostName1},
                full_dependency_ids = new List<Dependency>()
            };
            PackageVersion packageVersionWithMultipleHostDependencies = new PackageVersion
            {
                host_dependencies = new List<string>
                {
                    hostName1, hostName2
                },
                full_dependency_ids = new List<Dependency>()
            };

            // Act
            PackageDetailItem packageDetailItemNoDependencies = new PackageDetailItem
            (
                string.Empty,
                packageVersionNoDependencies,
                true
            );
            PackageDetailItem packageDetailWithPython2Dependency = new PackageDetailItem
            (
                string.Empty,
                packageVersionWithPython2Dependency,
                true
            );
            PackageDetailItem packageDetailWithPython3Dependency = new PackageDetailItem
            (
                string.Empty,
                packageVersionWithPython3Dependency,
                true
            );
            PackageDetailItem packageDetailWithHostDependency = new PackageDetailItem
            (
                string.Empty,
                packageVersionWithHostDependency,
                true
            );
            PackageDetailItem packageDetailWithMultipleHostDependencies = new PackageDetailItem
            (
                string.Empty,
                packageVersionWithMultipleHostDependencies,
                true
            );

            // Assert
            Assert.AreEqual(Dynamo.Properties.Resources.NoneString, packageDetailItemNoDependencies.Hosts);
            Assert.AreEqual(Dynamo.Properties.Resources.NoneString, packageDetailItemNoDependencies.PythonVersion);

            Assert.AreEqual(Dynamo.Properties.Resources.NoneString, packageDetailWithPython2Dependency.Hosts);
            Assert.AreEqual(PythonEngineManager.IronPython2EngineName,
                packageDetailWithPython2Dependency.PythonVersion);

            Assert.AreEqual(Dynamo.Properties.Resources.NoneString, packageDetailWithPython3Dependency.Hosts);
            Assert.AreEqual(PythonEngineManager.CPython3EngineName, packageDetailWithPython3Dependency.PythonVersion);

            Assert.AreEqual(hostName1, packageDetailWithHostDependency.Hosts);
            Assert.AreEqual(Dynamo.Properties.Resources.NoneString, packageDetailWithHostDependency.PythonVersion);

            Assert.AreEqual(string.Join(", ", hostNames), packageDetailWithMultipleHostDependencies.Hosts);
            Assert.AreEqual(Dynamo.Properties.Resources.NoneString,
                packageDetailWithMultipleHostDependencies.PythonVersion);
        }

        /// <summary>
        /// Tests whether the PackageDetailsViewModel receives the package versions properly.
        /// </summary>
        [Test]
        public void TestVersionsDisplayedInView()
        {
            // Arrange
            PackageHeader packageHeader = new PackageHeader {
                _id = null,
                name = string.Empty,
                versions = PackageVersions,
                latest_version_update = System.DateTime.Now,
                num_versions = PackageVersions.Count,
                comments = null,
                num_comments = 0,
                latest_comment = null,
                votes = 0,
                downloads = 0,
                repository_url = null,
                site_url = null,
                banned = false,
                deprecated = false,
                @group = null,
                engine = null,
                license = null,
                used_by = null,
                host_dependencies = Hosts,
                num_dependents = 0,
                description = null,
                maintainers = UsersList,
                keywords = null
        };
            PackageManagerSearchElement packageManagerSearchElement = new PackageManagerSearchElement(packageHeader);

            // Act
            PackageDetailsViewExtension.OpenPackageDetails(packageManagerSearchElement);
            PackageDetailsView packageDetailsView = PackageDetailsViewExtension.PackageDetailsView;

            // Assert
            Assert.IsNotNull(packageDetailsView.VersionsDataGrid);
            Assert.IsInstanceOf<PackageDetailsViewModel>(packageDetailsView.DataContext);

            PackageDetailsViewModel packageDetailsViewModel = packageDetailsView.DataContext as PackageDetailsViewModel;
            Assert.AreEqual(PackageVersions.Count, packageDetailsViewModel.PackageDetailItems.Count);
        }

        /// <summary>
        /// Tests whether the PackageDetailsViewModel displays the package links properly.
        /// </summary>
        [Test]
        public void TestLinksDisplayedInView()
        {
            // Arrange
            string someLink = "somelink";
            PackageHeader packageHeader = new PackageHeader
            {
                _id = null,
                name = string.Empty,
                versions = PackageVersions,
                latest_version_update = System.DateTime.Now,
                num_versions = PackageVersions.Count,
                comments = null,
                num_comments = 0,
                latest_comment = null,
                votes = 0,
                downloads = 0,
                repository_url = someLink,
                site_url = someLink,
                banned = false,
                deprecated = false,
                @group = null,
                engine = null,
                license = null,
                used_by = null,
                host_dependencies = Hosts,
                num_dependents = 0,
                description = null,
                maintainers = UsersList,
                keywords = null
            };
            PackageManagerSearchElement packageManagerSearchElement = new PackageManagerSearchElement(packageHeader);

            // Act
            PackageDetailsViewExtension.OpenPackageDetails(packageManagerSearchElement);
            PackageDetailsView packageDetailsView = PackageDetailsViewExtension.PackageDetailsView;

            // Assert
            Assert.IsNotNull(packageDetailsView.PackageWebsiteLink);
            Assert.IsNotNull(packageDetailsView.PackageRepositoryLink);
            Assert.IsInstanceOf<PackageDetailsViewModel>(packageDetailsView.DataContext);

            PackageDetailsViewModel packageDetailsViewModel = packageDetailsView.DataContext as PackageDetailsViewModel;
            Assert.AreEqual(someLink, packageDetailsViewModel.PackageRepositoryURL);
            Assert.AreEqual(someLink, packageDetailsViewModel.PackageSiteURL);
        }

        /// <summary>
        /// Tests whether OpenDependencyDetails method works.
        /// This is fired when the user clicks a link to another dependency package.
        /// </summary>
        [Test]
        public void TestOpenDependencyDetails()
        {
            // Arrange
            string packageToOpen = "Sample View Extension";
            string packageAuthor = "DynamoTeam";
            string packageDescription = "Dynamo sample view extension.";

            PackageDetailsViewExtension.PackageManagerClientViewModel = ViewModel.PackageManagerClientViewModel;

            PackageHeader packageHeader = new PackageHeader
            {
                _id = null,
                name = string.Empty,
                versions = PackageVersions,
                latest_version_update = System.DateTime.Now,
                num_versions = PackageVersions.Count,
                comments = null,
                num_comments = 0,
                latest_comment = null,
                votes = 0,
                downloads = 0,
                repository_url = null,
                site_url = null,
                banned = false,
                deprecated = false,
                @group = null,
                engine = null,
                license = null,
                used_by = null,
                host_dependencies = Hosts,
                num_dependents = 0,
                description = null,
                maintainers = UsersList,
                keywords = null
            };

            var depPackageHeader = new PackageHeader
            {
                _id = null,
                name =packageToOpen,
                versions = PackageVersions,
                latest_version_update = System.DateTime.Now,
                num_versions = PackageVersions.Count,
                comments = null,
                num_comments = 0,
                latest_comment = null,
                votes = 0,
                downloads = 0,
                repository_url = null,
                site_url = null,
                banned = false,
                deprecated = false,
                @group = null,
                engine = null,
                license = null,
                used_by = null,
                host_dependencies = Hosts,
                num_dependents = 0,
                description = packageDescription,
                maintainers = new List<User> {new User() { _id = "3", username = "DynamoTeam" } },
                keywords = null
            };

            PackageManagerSearchElement packageManagerSearchElement = new PackageManagerSearchElement(packageHeader);
            PackageManagerSearchElement depPackageManagerSearchElement = new PackageManagerSearchElement(depPackageHeader);

            PackageDetailsViewExtension.OpenPackageDetails(packageManagerSearchElement);
            PackageDetailsView packageDetailsView = PackageDetailsViewExtension.PackageDetailsView;
            Assert.IsInstanceOf<PackageDetailsViewModel>(packageDetailsView.DataContext);
            var originalViewModel = packageDetailsView.DataContext as PackageDetailsViewModel;

            var mockViewModel = new Mock<PackageDetailsViewModel>(MockBehavior.Default, PackageDetailsViewExtension, packageManagerSearchElement);
            mockViewModel.CallBase = true;
            mockViewModel.Setup(x => x.GetPackageByName(It.IsAny<string>()))
                .Returns<string>(
                (s) => {
                    return depPackageManagerSearchElement;
                } );
            packageDetailsView.DataContext = mockViewModel;


            // Act
            mockViewModel.Object.OpenDependencyDetails(packageToOpen);

            // Assert
            PackageDetailsView newPackageDetailsView = PackageDetailsViewExtension.PackageDetailsView;
            PackageDetailsViewModel newPackageDetailsViewModel = newPackageDetailsView.DataContext as PackageDetailsViewModel;

            Assert.AreEqual(packageToOpen, newPackageDetailsViewModel.PackageName);
            Assert.AreEqual(packageAuthor, newPackageDetailsViewModel.PackageAuthorName);
            Assert.AreEqual(packageDescription, newPackageDetailsViewModel.PackageDescription);
        }

        /// <summary>
        /// Tests whether the extension opens and display package details when a user
        /// clicks on the 'View Details' button in the package manager searh view.
        /// </summary>
        [Test]
        public void TestViewPackageDetailsCommand()
        {
            // Arrange
            string packageToOpen = "Sample View Extension";
            List<User> packageAuthor = new List<User> { new User{ _id = "1", username = "DynamoTeam" }};
            string packageDescription = "Dynamo sample view extension.";

            PackageDetailsViewExtension.PackageManagerClientViewModel = ViewModel.PackageManagerClientViewModel;

            PackageHeader packageHeader = new PackageHeader
            {
                _id = null,
                name = packageToOpen,
                versions = PackageVersions,
                latest_version_update = System.DateTime.Now,
                num_versions = PackageVersions.Count,
                comments = null,
                num_comments = 0,
                latest_comment = null,
                votes = 0,
                downloads = 0,
                repository_url = null,
                site_url = null,
                banned = false,
                deprecated = false,
                @group = null,
                engine = null,
                license = null,
                used_by = null,
                host_dependencies = Hosts,
                num_dependents = 0,
                description = packageDescription,
                maintainers = packageAuthor,
                keywords = null
            };
            PackageManagerSearchElement packageManagerSearchElement = new PackageManagerSearchElement(packageHeader);
            PackageDetailsViewExtension.OpenPackageDetails(packageManagerSearchElement);
            PackageDetailsView packageDetailsView = PackageDetailsViewExtension.PackageDetailsView;
            Assert.IsInstanceOf<PackageDetailsViewModel>(packageDetailsView.DataContext);
            PackageDetailsViewModel packageDetailsViewModel = packageDetailsView.DataContext as PackageDetailsViewModel;

            // Act
            PackageDetailsViewExtension.PackageManagerClientViewModel
                .DynamoViewModel
                .OnViewExtensionOpenWithParameterRequest("Package Details", packageManagerSearchElement);

            // Assert
            Assert.AreEqual(packageToOpen, packageDetailsViewModel.PackageName);
            Assert.AreEqual(packageAuthor.First().username, packageDetailsViewModel.PackageAuthorName);
            Assert.AreEqual(packageDescription, packageDetailsViewModel.PackageDescription);
            Assert.AreEqual(false, string.IsNullOrEmpty(packageDetailsViewModel.PackageDetailItems.FirstOrDefault().PackageSize));
        }
    }
}
