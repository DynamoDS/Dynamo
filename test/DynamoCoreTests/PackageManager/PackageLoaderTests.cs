using System.IO;
using System.Linq;

using NUnit.Framework;

namespace Dynamo.PackageManager.Tests
{
    class PackageLoaderTests : DynamoModelTestBase
    {
        public string PackagesDirectory { get { return Path.Combine(this.TestDirectory, "pkgs"); } }

        [Test]
        public void ScanPackageDirectoryReturnsPackageForValidDirectory()
        {
            var pkgDir = Path.Combine(PackagesDirectory, "Custom Rounding");
            var loader = this.CurrentDynamoModel.PackageLoader;
            var pkg = loader.ScanPackageDirectory(pkgDir);

            Assert.IsNotNull(pkg);
            Assert.AreEqual("CAAD_RWTH", pkg.Group);
            Assert.AreEqual("Custom Rounding", pkg.Name);
            Assert.AreEqual("0.1.4", pkg.VersionName);
            Assert.AreEqual("This collection of nodes allows rounding, rounding up and rounding down to a specified precision.", pkg.Description);
            Assert.AreEqual("Round Up To Precision - Rounds a number *up* to a specified precision, Round Down To Precision - " 
                + "Rounds a number *down* to a specified precision, Round To Precision - Rounds a number to a specified precision", pkg.Contents);
            Assert.AreEqual("0.5.2.10107", pkg.EngineVersion);

            loader.Load(pkg);

            Assert.AreEqual(3, pkg.LoadedCustomNodes.Count);
        }

        [Test]
        public void ScanPackageDirectoryReturnsNullForInvalidDirectory()
        {
            var pkgDir = "";
            var loader = this.CurrentDynamoModel.PackageLoader;
            Assert.IsNull(loader.ScanPackageDirectory(pkgDir));
        }

        [Test]
        public void LoadPackagesReturnsAllValidPackagesInValidDirectory()
        {
            var loader = new PackageLoader(PackagesDirectory);
            loader.LoadAll(new LoadPackageParams
            {
                Preferences = this.CurrentDynamoModel.PreferenceSettings
            });

            // There are four packages in "GitHub\Dynamo\test\pkgs"
            Assert.AreEqual(4, loader.LocalPackages.Count());
        }

        [Test]
        public void LoadPackagesReturnsNoPackagesForInvalidDirectory()
        {
            var pkgDir = Path.Combine(PackagesDirectory, "No directory");
            var loader = new PackageLoader(pkgDir);
            loader.LoadAll(new LoadPackageParams
            {
                Preferences = this.CurrentDynamoModel.PreferenceSettings           
            });

            Assert.AreEqual(0, loader.LocalPackages.Count());
        }

        [Test]
        public void GetOwnerPackageReturnsPackageForValidFunctionDefinition()
        {
            var loader = new PackageLoader(PackagesDirectory);
            loader.RequestLoadCustomNodeDirectory +=
                (dir) => this.CurrentDynamoModel.CustomNodeManager.AddUninitializedCustomNodesInPath(dir, true);

            loader.LoadAll(new LoadPackageParams
            {
                Preferences = this.CurrentDynamoModel.PreferenceSettings
            });

            var pkg = loader.LocalPackages.FirstOrDefault(x => x.Name == "Custom Rounding");
            Assert.AreEqual(3, pkg.LoadedCustomNodes.Count);

            foreach (var nodeInfo in pkg.LoadedCustomNodes)
            {
                CustomNodeDefinition funcDef;
                Assert.IsTrue(this.CurrentDynamoModel.CustomNodeManager.TryGetFunctionDefinition(nodeInfo.FunctionId, true, out funcDef));
                Assert.IsNotNull(funcDef);

                var foundPkg = loader.GetOwnerPackage(nodeInfo);

                Assert.IsNotNull(foundPkg);
                Assert.AreEqual(pkg.Name, foundPkg.Name);
                Assert.IsTrue(pkg.Name == foundPkg.Name);
            }
        }

        [Test]
        public void GetOwnerPackageReturnsNullForInvalidFunction()
        {
            var loader = new PackageLoader(PackagesDirectory);

            CustomNodeInfo info;
            Assert.IsTrue(
                this.CurrentDynamoModel.CustomNodeManager.AddUninitializedCustomNode(
                    Path.Combine(new string[] { TestDirectory, "core", "combine", "combine2.dyf" }),
                    true,
                    out info));

            CustomNodeDefinition funcDef;
            Assert.IsTrue(this.CurrentDynamoModel.CustomNodeManager.TryGetFunctionDefinition(info.FunctionId, true, out funcDef));
            var foundPkg = loader.GetOwnerPackage(info);
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
