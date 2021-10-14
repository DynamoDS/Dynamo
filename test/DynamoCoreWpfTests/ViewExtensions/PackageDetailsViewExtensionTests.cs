using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Dynamo.Graph.Workspaces;
using Dynamo.PackageDetails;
using Dynamo.PackageManager;
using Dynamo.PackageManager.UI;
using Dynamo.WorkspaceDependency;
using Dynamo.Wpf.Extensions;
using Greg.Responses;
using NUnit.Framework;
using PythonNodeModels;

namespace DynamoCoreWpfTests
{
    public class PackageDetailsViewExtensionTests : DynamoTestUIBase
    {
        private PackageDetailsViewExtension packageDetailsViewExtension = new PackageDetailsViewExtension();
        
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
            List<string> hostNames = new List<string> { hostName1, hostName2 };

            PackageVersion packageVersionNoDependencies = new PackageVersion
            {
                host_dependencies = new List<string>(),
                full_dependency_ids = new List<Dependency>()
            };
            PackageVersion packageVersionWithPython2Dependency = new PackageVersion
            {
                host_dependencies = new List<string> { PythonEngineVersion.IronPython2.ToString() },
                full_dependency_ids = new List<Dependency>()
            };
            PackageVersion packageVersionWithPython3Dependency = new PackageVersion
            {
                host_dependencies = new List<string> { PythonEngineVersion.CPython3.ToString() },
                full_dependency_ids = new List<Dependency>()
            };
            PackageVersion packageVersionWithHostDependency = new PackageVersion
            {
                host_dependencies = new List<string> { hostName1 },
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
            Assert.AreEqual(PythonEngineVersion.IronPython2.ToString(), packageDetailWithPython2Dependency.PythonVersion);

            Assert.AreEqual(Dynamo.Properties.Resources.NoneString, packageDetailWithPython3Dependency.Hosts);
            Assert.AreEqual(PythonEngineVersion.CPython3.ToString(), packageDetailWithPython3Dependency.PythonVersion);

            Assert.AreEqual(hostName1, packageDetailWithHostDependency.Hosts);
            Assert.AreEqual(Dynamo.Properties.Resources.NoneString, packageDetailWithHostDependency.PythonVersion);

            Assert.AreEqual(string.Join(", ", hostNames), packageDetailWithMultipleHostDependencies.Hosts);
            Assert.AreEqual(Dynamo.Properties.Resources.NoneString, packageDetailWithMultipleHostDependencies.PythonVersion);
        }

        /// <summary>
        /// Tests whether the PackageDetailsView's DataGrid displays package versions properly.
        /// </summary>
        [Test]
        public void TestVersionDisplay()
        {
            // Arrange
            PackageVersion packageVersion1 = new PackageVersion { full_dependency_ids = new List<Dependency>() };
            PackageVersion packageVersion2 = new PackageVersion { full_dependency_ids = new List<Dependency>() };
            List<PackageVersion> packageVersions = new List<PackageVersion> { packageVersion1, packageVersion2 };

            PackageHeader packageHeader = new PackageHeader { versions = packageVersions };
            PackageManagerSearchElement packageManagerSearchElement = new PackageManagerSearchElement(packageHeader)
            {
                Height = 0,
                Visibility = true,
                IsSelected = false,
                IsExpanded = false,
                Items = null,
                Parent = null,
                OldParent = null,
                FullCategoryName = null,
                Votes = 0,
                Weight = 0,
                Guid = default,
                Keywords = null
            };

            PackageManagerExtension packageManagerExtension = new PackageManagerExtension();
            packageDetailsViewExtension.PackageManagerExtension = packageManagerExtension;

            // Act
            packageDetailsViewExtension.OpenPackageDetails(packageManagerSearchElement);
            PackageDetailsView packageDetailsView = packageDetailsViewExtension.PackageDetailsView;

            // Assert
            Assert.IsNotNull(packageDetailsView.VersionsDataGrid);
            Assert.AreEqual(packageVersions.Count, packageDetailsView.VersionsDataGrid.Items.Count);
        }

        [Test]
        public void DownloadSpecifiedVersionOfPackageTest()
        {
            RaiseLoadedEvent(this.View);
            var extensionManager = View.viewExtensionManager;
            extensionManager.Add(packageDetailsViewExtension);

            Open(@"pkgs\Dynamo Samples\extra\CustomRenderExample.dyn");

            var loadedParams = new ViewLoadedParams(View, ViewModel);
            var currentWorkspace = ViewModel.Model.CurrentWorkspace;

            //// This is equivalent to uninstall the package
            //var package = packageDetailsViewExtension..PackageLoader.LocalPackages.Where(x => x.Name == "Dynamo Samples").FirstOrDefault();
            //package.LoadState.SetScheduledForDeletion();

            //// Once choosing to install the specified version, info.State should reflect RequireRestart
            //viewExtension.DependencyView.DependencyRegen(currentWorkspace);

            //// Restart banner should display immediately
            //Assert.AreEqual(Visibility.Visible, viewExtension.DependencyView.RestartBanner.Visibility);
            //Assert.AreEqual(1, viewExtension.DependencyView.PackageDependencyTable.Items.Count);
            //var newInfo = viewExtension.DependencyView.dataRows.FirstOrDefault().DependencyInfo;

            //// Local loaded version was 2.0.0, but now will be update to date with dyn
            //Assert.AreEqual("2.0.1", newInfo.Version.ToString());
            //Assert.AreEqual(1, newInfo.Nodes.Count);
            //Assert.AreEqual(newInfo.State, PackageDependencyState.RequiresRestart);
        }

       
    }
}
