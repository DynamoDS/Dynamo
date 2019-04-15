using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CefSharp;
using Dynamo.Configuration;
using Dynamo.Graph.Workspaces;
using Dynamo.Interfaces;
using Dynamo.LibraryUI;
using Dynamo.Models;
using Dynamo.Scheduler;
using Dynamo.Search.SearchElements;
using Dynamo.Wpf.Extensions;
using DynamoCoreWpfTests;
using DynamoCoreWpfTests.Utility;
using Moq;
using NUnit.Framework;

namespace ViewExtensionLibraryTests
{
    class LibraryViewControllerTests : DynamoTestUIBase
    {
        private string PackagesDirectory { get { return Path.Combine(GetTestDirectory(this.ExecutingDirectory), "pkgs"); } }

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
        [Category("UnitTests")]
        public void CreatingNodeCustomNodeDefinitionOrUpdatingItShouldRaiseUpdate()
        {
            var customNodeManager = this.Model.CustomNodeManager;
            var nodeSearchModel = this.Model.SearchModel;
            var customization = new LibraryViewCustomization();
            var commandExec = new ViewExtensionCommandExecutive(this.ViewModel);
            var testCallback = new Mock<IJavascriptCallback>();

            const string libraryDataUpdated = "libraryDataUpdated";
            var resetevent = new AutoResetEvent(false);
            var refreshCount = 0;
            var timeout = 50;

            testCallback.Setup(c => c.CanExecute).Returns(true);
            testCallback.Setup(c => c.ExecuteAsync()).Callback(() => {
                refreshCount = refreshCount + 1; resetevent.Set(); });

            var controller = new LibraryViewController(this.View, commandExec, customization);
            controller.On(libraryDataUpdated, testCallback.Object);

            Assert.AreEqual(0, refreshCount);

            var path =System.IO.Path.Combine(TempFolder, "customNodeLibraryTest.dyf");


                var cnId = Guid.NewGuid();
            // lets make a real customnode, this will register the custom node and we should assert a refresh was raised so we can see it.
            this.Model.ExecuteCommand(new DynamoModel.CreateCustomNodeCommand(cnId, "customNodeLibraryTest", "tests", "", false));
         
            Assert.IsTrue(resetevent.WaitOne(timeout * 100));
            Assert.AreEqual(1, refreshCount);
            resetevent.Reset();

            //save will raise an event.
            this.Model.Workspaces.OfType<CustomNodeWorkspaceModel>().FirstOrDefault().Save(path);
            Assert.IsTrue(resetevent.WaitOne(timeout * 100));
            Assert.AreEqual(2, refreshCount);
            resetevent.Reset();

            //  get the searchElement for this custom node and update it.
            var realNodeData = nodeSearchModel.SearchEntries.OfType<CustomNodeSearchElement>().Where(x => x.ID == cnId).FirstOrDefault();
            //  updating should raise an event.
            nodeSearchModel.Update(realNodeData);

            Assert.IsTrue(resetevent.WaitOne(timeout * 100));
            Assert.AreEqual(3, refreshCount);

            //cleanup the saved custom node
            System.IO.File.Delete(path);

        }

        [Test]
        [Category("UnitTests")]
        public void InstantiatingLazyLoadedCustomNodeShouldNotRaiseUpdate()
        {


            var customNodeManager = this.Model.CustomNodeManager;
            var nodeSearchModel = this.Model.SearchModel;
            var customization = new LibraryViewCustomization();
            var commandExec = new ViewExtensionCommandExecutive(this.ViewModel);
            var testCallback = new Mock<IJavascriptCallback>();

            const string libraryDataUpdated = "libraryDataUpdated";
            var refreshCount = 0;
            var infoUpdated = false;

            testCallback.Setup(c => c.CanExecute).Returns(true);
            testCallback.Setup(c => c.ExecuteAsync()).Callback(() => {
                refreshCount = refreshCount + 1;
            });

            var controller = new LibraryViewController(this.View, commandExec, customization);
            controller.On(libraryDataUpdated, testCallback.Object);

            //lets grab a real customNode which was loaded from the package directory specified above
            var customNodeSearchEntry = nodeSearchModel.SearchEntries.OfType<CustomNodeSearchElement>().FirstOrDefault();
            var cnId = customNodeSearchEntry.ID;
            //lets attempt to create this, we should not raise an update, but we should see info get updated in the search.
            customNodeManager.InfoUpdated += (data) =>
            {
                Assert.AreEqual(data.FunctionId, cnId);
                infoUpdated = true;

            };
            controller.CreateNode(cnId.ToString());
            DispatcherUtil.DoEvents();
            Assert.AreEqual(0, refreshCount);
            Assert.IsTrue(infoUpdated);

        }
    }
}
