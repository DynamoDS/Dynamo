using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using NUnit.Framework;

namespace Dynamo.Tests.UI
{
    [TestFixture]
    public class RecordedTests
    {
        // For access within test cases.
        private Exception exception = null;
        private DynamoController controller = null;

        [SetUp]
        public void Start()
        {
        }

        [TearDown]
        public void Exit()
        {
            this.controller = null;
            this.exception = null;
        }

        [Test, RequiresSTA]
        public void TestCreateNodes()
        {
            RunCommandsFromFile("CreateNodesAndConnectors.xml");
            WorkspaceModel workspace = controller.DynamoModel.CurrentWorkspace;
            Assert.AreEqual(5, workspace.Nodes.Count);
        }

        [Test, RequiresSTA]
        public void TestCreateConnectors()
        {
            RunCommandsFromFile("CreateNodesAndConnectors.xml");
            WorkspaceModel workspace = controller.DynamoModel.CurrentWorkspace;
            Assert.AreEqual(4, workspace.Connectors.Count);
        }

        private void RunCommandsFromFile(string commandFileName)
        {
            string commandFilePath = DynamoTestUI.GetTestDirectory();
            commandFilePath = Path.Combine(commandFilePath, @"core\recorded\");
            commandFilePath = Path.Combine(commandFilePath, commandFileName);

            // Create the controller to run alongside the view.
            controller = DynamoController.MakeSandbox(commandFilePath);

            // Create the view.
            var dynamoView = new DynamoView();
            dynamoView.DataContext = controller.DynamoViewModel;
            controller.UIDispatcher = dynamoView.Dispatcher;
            dynamoView.ShowDialog();

            Assert.IsNotNull(controller);
            Assert.IsNotNull(controller.DynamoModel);
            Assert.IsNotNull(controller.DynamoModel.CurrentWorkspace);
        }
    }
}
