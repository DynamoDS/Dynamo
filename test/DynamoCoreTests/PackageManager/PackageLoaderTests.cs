using System.IO;
using System.Linq;

using Dynamo.PackageManager;

using NUnit.Framework;

namespace Dynamo.Tests
{
    class PackageLoaderTests : DynamoViewModelUnitTest
    {
        public string PackagesDirectory { get { return Path.Combine(this.GetTestDirectory(), "pkgs"); } }

        [Test]
        public void ScanPackageDirectoryReturnsPackageForValidDirectory()
        {
            var pkgDir = Path.Combine(PackagesDirectory, "Custom Rounding");
            var loader = ViewModel.Model.Loader.PackageLoader;
            var pkg = loader.ScanPackageDirectory(pkgDir);

            Assert.IsNotNull(pkg);
            Assert.AreEqual("CAAD_RWTH", pkg.Group);
            Assert.AreEqual("Custom Rounding", pkg.Name);
            Assert.AreEqual("0.1.4", pkg.VersionName);
            Assert.AreEqual("This collection of nodes allows rounding, rounding up and rounding down to a specified precision.", pkg.Description);
            Assert.AreEqual("Round Up To Precision - Rounds a number *up* to a specified precision, Round Down To Precision - " 
                + "Rounds a number *down* to a specified precision, Round To Precision - Rounds a number to a specified precision", pkg.Contents);
            Assert.AreEqual("0.5.2.10107", pkg.EngineVersion);
            pkg.LoadIntoDynamo(ViewModel.Model.Loader, ViewModel.Model.Logger, ViewModel.Model.EngineController.LibraryServices);

            Assert.AreEqual(3, pkg.LoadedCustomNodes.Count);
        }

        [Test]
        public void ScanPackageDirectoryReturnsNullForInvalidDirectory()
        {
            var pkgDir = "";
            var loader = ViewModel.Model.Loader.PackageLoader;
            var pkg = loader.ScanPackageDirectory(pkgDir);
        }

        [Test]
        [Category("Failure")]
        public void LoadPackagesReturnsAllValidPackagesInValidDirectory()
        {
            var loader = new PackageLoader(ViewModel.Model.Loader, ViewModel.Model.Logger);
            loader.LoadPackagesIntoDynamo(ViewModel.Model.PreferenceSettings, ViewModel.Model.EngineController.LibraryServices);

            Assert.AreEqual(1, loader.LocalPackages.Count);
        }

        [Test]
        public void LoadPackagesReturnsNoPackagesForInvalidDirectory()
        {
            var pkgDir = Path.Combine(PackagesDirectory, "No directory");
            var loader = new PackageLoader(ViewModel.Model.Loader, ViewModel.Model.Logger, pkgDir);
            loader.LoadPackagesIntoDynamo(ViewModel.Model.PreferenceSettings, ViewModel.Model.EngineController.LibraryServices);
            Assert.AreEqual(0, loader.LocalPackages.Count);
        }

        [Test]
        public void GetOwnerPackageReturnsPackageForValidFunctionDefinition()
        {
            //Assert.Inconclusive("Porting : Formula");

            var loader = new PackageLoader(ViewModel.Model.Loader, ViewModel.Model.Logger, PackagesDirectory);
            loader.LoadPackagesIntoDynamo(ViewModel.Model.PreferenceSettings, ViewModel.Model.EngineController.LibraryServices);
            var pkg = loader.LocalPackages.FirstOrDefault(x => x.Name == "Custom Rounding");
            Assert.AreEqual(3, pkg.LoadedCustomNodes.Count);

            foreach (var nodeInfo in pkg.LoadedCustomNodes)
            {
                var funcDef = ViewModel.Model.CustomNodeManager.GetFunctionDefinition(nodeInfo.FunctionId);
                Assert.IsNotNull(funcDef);

                var foundPkg = loader.GetOwnerPackage(funcDef);

                Assert.IsNotNull(foundPkg);
                Assert.AreEqual(pkg.Name, foundPkg.Name);
                Assert.IsTrue(pkg.Name == foundPkg.Name);
            }
        
        }

        [Test]
        public void GetOwnerPackageReturnsNullForInvalidFunction()
        {
            var loader = new PackageLoader(ViewModel.Model.Loader, ViewModel.Model.Logger, PackagesDirectory);

            var info = ViewModel.Model.CustomNodeManager.AddFileToPath(
                Path.Combine(new string[] {GetTestDirectory(), "core", "combine", "combine2.dyf"}));

            var funcDef = ViewModel.Model.CustomNodeManager.GetFunctionDefinition(info.FunctionId);
            Assert.IsNotNull(funcDef);
            var foundPkg = loader.GetOwnerPackage(funcDef);
            Assert.IsNull(foundPkg);
        }

        [Test]
        public void IsUnderPackageControlIsCorrectForValidFunctionDefinition()
        {
            Assert.Inconclusive("Finish me");

        }

        [Test]
        public void IsUnderPackageControlIsCorrectForValidType()
        {
            Assert.Inconclusive("Finish me");

        }

        [Test]
        public void IsUnderPackageControlIsCorrectForValidPath()
        {
            Assert.Inconclusive("Finish me");

        }

        [Test]
        public void CanGetPackageFromRootReturnsPackageForValidDirectory()
        {
            Assert.Inconclusive("Finish me");

        }

        [Test]
        public void CanGetPackageFromRootReturnsNullForInvalidDirectory()
        {
            Assert.Inconclusive("Finish me");

        }
    }
}