using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using CoreNodeModels.Input;
using Dynamo.Configuration;
using Dynamo.Graph.Nodes;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Scheduler;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    class PackageManagerExtensionLoadingTests : DynamoTestUIBase
    {
        private string PackagesDirectory { get { return Path.Combine(GetTestDirectory(this.ExecutingDirectory), "pkgs"); } }

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        protected override DynamoModel.IStartConfiguration CreateStartConfiguration(IPathResolver pathResolver)
        {
            return new DynamoModel.DefaultStartConfiguration()
            {
                PathResolver = pathResolver,
                StartInTestMode = true,
                GeometryFactoryPath = preloader.GeometryFactoryPath,
                ProcessMode = TaskProcessMode.Synchronous,
                Preferences = new PreferenceSettings() { CustomPackageFolders = new List<string>() { this.PackagesDirectory } }
            };
        }

        [Test]
        public void PackageManagerLoadsAndAddsExtension()
        {
         
            Assert.That(this.Model.ExtensionManager.Extensions.Select(x => x.Name),
                Is.EquivalentTo (new List<string> { "DynamoPackageManager", "testExtension" }));
        }
        [Test]
        public void PackageManagerLoadsExtensionAndItWorks()
        {
         
            dynamic sampleExtension = this.Model.ExtensionManager.Extensions.Where(x => x.Name == "testExtension").FirstOrDefault();
            var node = new DoubleInput();
            this.Model.CurrentWorkspace.AddAndRegisterNode(node);
            //assert the extension was keeping track of the node events in the workspace.
            Assert.AreEqual(1, (sampleExtension.nodes as List<NodeModel>).Count());
        }
        [Test]
        public void PackageManagerLoadsAndAddsViewExtension()
        {
            Assert.That(this.View.viewExtensionManager.ViewExtensions.Select(x => x.Name),
                Is.EquivalentTo((new List<string> { "DynamoManipulationExtension","LibraryUI","NotificationsExtension", "Workspace References", "Sample View Extension","PackageManagerViewExtension" })));
        }

        [Test]
        public void PackageManagerLoadsViewExtensionAndItWorks()
        {
            dynamic sampleExtension = this.View.viewExtensionManager.ViewExtensions.Where(x => x.Name == "Sample View Extension").FirstOrDefault();
            var node = new DoubleInput();
            this.Model.CurrentWorkspace.AddAndRegisterNode(node);

            //assert a menu item was added with the correct header.
            var mi = this.View.viewMenu.Items.Cast<MenuItem>().Where
                (x => (string)x.Header == "Show View Extension Sample Window").FirstOrDefault();
            Assert.IsNotNull(mi);
        }
    }
}
