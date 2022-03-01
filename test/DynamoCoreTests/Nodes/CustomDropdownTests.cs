using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoreNodeModels;
using Dynamo.PackageManager;
using NUnit.Framework;
using SystemTestServices;

namespace Dynamo.Tests.Nodes
{
    [TestFixture]
    public class CustomDropDownTests : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void OpenJsonDYNwithSelectionNode()
        {
            // Define package loading reference path
            var dir = SystemTestBase.GetTestDirectory(ExecutingDirectory);
            var pkgDir = Path.Combine(dir, "pkgs\\Dynamo Samples");
            var pkgMan = this.CurrentDynamoModel.GetPackageManagerExtension();
            var loader = pkgMan.PackageLoader;
            var pkg = loader.ScanPackageDirectory(pkgDir);

            // Load the sample package
            loader.LoadPackages(new List<Package> {pkg});
            // Assert expected package was loaded
            Assert.AreEqual(pkg.Name, "Dynamo Samples");

            // Run the graph with correct info serialized, node should deserialize to correct selection
            string path = Path.Combine(TestDirectory, "pkgs", "Dynamo Samples", "extra", "DropDownSample.dyn");
            RunModel(path);

            var node = CurrentDynamoModel.CurrentWorkspace.Nodes.FirstOrDefault();
            // Second selection selected
            Assert.AreEqual(1, node.GetType().GetProperty("SelectedIndex").GetValue(node));
            Assert.AreEqual("Cersei", node.GetType().GetProperty("SelectedString").GetValue(node));
        }



    }
}
