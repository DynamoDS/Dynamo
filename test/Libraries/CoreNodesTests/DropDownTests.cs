using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoreNodeModels;
using Dynamo.PackageManager;
using NUnit.Framework;

namespace DSCoreNodesTests
{
    [TestFixture]
    public class DropDownTests : Dynamo.DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }
        [Test]
        public void OpenXmlDYNwithSelectionNode()
        {
            // Define package loading reference path
            var dir = TestDirectory;
            var pkgDir = Path.Combine(dir, "pkgs\\Dynamo Samples");
            var pkgMan = this.CurrentDynamoModel.GetPackageManagerExtension();
            var loader = pkgMan.PackageLoader;
            var pkg = loader.ScanPackageDirectory(pkgDir);

            // Load the sample package
            loader.LoadPackages(new List<Package> { pkg });
            // Assert expected package was loaded
            //TODO_NET6 following package cannot be loaded as depends on wpf.
            Assert.AreEqual(pkg.Name, "Dynamo Samples");

            // Run the graph with correct info serialized, node should deserialize to correct selection
            string path = Path.Combine(TestDirectory, "pkgs", "Dynamo Samples", "extra", "DropDownSample_1Dot3.dyn");
            RunModel(path);

            var node = CurrentDynamoModel.CurrentWorkspace.Nodes.FirstOrDefault();
            // Second selection selected
            Assert.AreEqual(1, node.GetType().GetProperty("SelectedIndex").GetValue(node));
            Assert.AreEqual("Cersei", node.GetType().GetProperty("SelectedString").GetValue(node));
        }

        /// <summary>
        /// This test will make sure when opening an XML DYN containing dropdown
        /// with invalid SelectedIndex but valid SelectedString can still be opened correctly
        /// </summary>
        [Test]
        public void OpenXmlDYNwithInvalidSelectedIndex()
        {
            // Define package loading reference path
            var dir = TestDirectory;
            var pkgDir = Path.Combine(dir, "pkgs\\Dynamo Samples");
            var pkgMan = this.CurrentDynamoModel.GetPackageManagerExtension();
            var loader = pkgMan.PackageLoader;
            var pkg = loader.ScanPackageDirectory(pkgDir);

            // Load the sample package
            loader.LoadPackages(new List<Package> { pkg });
            // Assert expected package was loaded
            //TODO_NET6 following package cannot be loaded as depends on wpf.
            Assert.AreEqual(pkg.Name, "Dynamo Samples");

            // Run the graph with correct info serialized, node should deserialize to correct selection
            string path = Path.Combine(TestDirectory, "pkgs", "Dynamo Samples", "extra", "DropDownSample_1Dot3_InvalidSelectedIndex.dyn");
            RunModel(path);

            var node = CurrentDynamoModel.CurrentWorkspace.Nodes.FirstOrDefault();
            // Second selection selected
            Assert.AreEqual(1, node.GetType().GetProperty("SelectedIndex").GetValue(node));
            Assert.AreEqual("Cersei", node.GetType().GetProperty("SelectedString").GetValue(node));
        }
        /// <summary>
        /// This test will make sure when opening an XML DYN containing dropdown
        /// with negative SelectedIndex can still be opened correctly
        /// </summary>
        [Test]
        public void OpenXmlDYNwithInvalidSelectedIndex2()
        {
            // Define package loading reference path
            var dir = TestDirectory;
            var pkgDir = Path.Combine(dir, "pkgs\\Dynamo Samples");
            var pkgMan = this.CurrentDynamoModel.GetPackageManagerExtension();
            var loader = pkgMan.PackageLoader;
            var pkg = loader.ScanPackageDirectory(pkgDir);

            // Load the sample package
            loader.LoadPackages(new List<Package> { pkg });
            // Assert expected package was loaded
            Assert.AreEqual(pkg.Name, "Dynamo Samples");

            // Run the graph with correct info serialized, node should deserialize to correct selection
            string path = Path.Combine(TestDirectory, "pkgs", "Dynamo Samples", "extra", "DropDownSample_1Dot3_InvalidSelectedIndex2.dyn");
            RunModel(path);

            var node = CurrentDynamoModel.CurrentWorkspace.Nodes.FirstOrDefault();
            // No selection selected
            //TODO_NET6 following package cannot be loaded as depends on wpf.
            Assert.AreEqual(-1, node.GetType().GetProperty("SelectedIndex").GetValue(node));
            Assert.AreEqual(String.Empty, node.GetType().GetProperty("SelectedString").GetValue(node));
        }

        /// <summary>
        /// This test will make sure when opening an XML DYN containing dropdown
        /// with SelectedIndex larger than item count can still be opened correctly
        /// </summary>
        [Test]
        public void OpenXmlDYNwithInvalidSelectedIndex3()
        {
            // Define package loading reference path
            var dir = TestDirectory;
            var pkgDir = Path.Combine(dir, "pkgs\\Dynamo Samples");
            var pkgMan = this.CurrentDynamoModel.GetPackageManagerExtension();
            var loader = pkgMan.PackageLoader;
            var pkg = loader.ScanPackageDirectory(pkgDir);

            // Load the sample package
            loader.LoadPackages(new List<Package> { pkg });
            // Assert expected package was loaded
            Assert.AreEqual(pkg.Name, "Dynamo Samples");

            // Run the graph with correct info serialized, node should deserialize to correct selection
            //TODO_NET6 following package cannot be loaded as depends on wpf.
            string path = Path.Combine(TestDirectory, "pkgs", "Dynamo Samples", "extra", "DropDownSample_1Dot3_InvalidSelectedIndex3.dyn");
            RunModel(path);

            var node = CurrentDynamoModel.CurrentWorkspace.Nodes.FirstOrDefault();
            // No selection selected
            Assert.AreEqual(-1, node.GetType().GetProperty("SelectedIndex").GetValue(node));
            Assert.AreEqual(String.Empty, node.GetType().GetProperty("SelectedString").GetValue(node));
        }
    }
}
