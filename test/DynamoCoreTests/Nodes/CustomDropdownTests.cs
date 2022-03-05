using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using CoreNodeModels;
using CoreNodeModels.Input;
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
            string path = Path.Combine(TestDirectory, "pkgs", "Dynamo Samples", "extra", "CustomDropdownMenuNodeSample.dyn");
            RunModel(path);

            var node = CurrentDynamoModel.CurrentWorkspace.Nodes.FirstOrDefault();

            object itemsAsObject = node.GetType().GetProperty(nameof(CustomSelectionNodeModel.Items), typeof(ObservableCollection<CustomSelectionItemViewModel>)).GetValue(node);
            Assert.NotNull(itemsAsObject);
            var items = (ObservableCollection<CustomSelectionItemViewModel>)(itemsAsObject);
            Assert.AreEqual(3, items.Count);
            Assert.AreEqual("One", items[0].Name);
        }



    }
}
