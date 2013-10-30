using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Dynamo;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.PackageManager;
using Dynamo.PackageManager.UI;
using Dynamo.Search;
using Dynamo.FSchemeInterop;
using Dynamo.ViewModels;
using Microsoft.FSharp.Collections;
using NUnit.Framework;

namespace Dynamo.Tests
{
    internal class PackageDependencyTests : DynamoUnitTest
    {
        [Test]
        public void CanDiscoverDependenciesForFunctionDefinitionOpenFromFile()
        {
            var vm = Controller.DynamoViewModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\custom_node_dep_test\");

            string openPath = Path.Combine(examplePath, "custom_node_dep_test.dyn");
            Controller.DynamoModel.Open(openPath);
            var funcRootNode = vm.CurrentSpace.NodeFromWorkspace<Function>("333ed3ad-c786-4064-8203-e79ce7cb109f");

            var dirDeps = funcRootNode.Definition.DirectDependencies;
            Assert.AreEqual(2, dirDeps.Count() );

            var allDeps = funcRootNode.Definition.Dependencies;
            Assert.AreEqual(7, allDeps.Count());

            var packageRoot = new PackageItemRootViewModel(funcRootNode.Definition);
            packageRoot.BuildDependencies(new HashSet<object>());

            Assert.AreEqual(2, packageRoot.Items.Count);

            Assert.AreEqual(2, packageRoot.Items[0].Items.Count);
            Assert.AreEqual(3, packageRoot.Items[1].Items.Count);
        }
    }
}
