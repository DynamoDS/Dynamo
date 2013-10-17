using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.ViewModels;
using NUnit.Framework;

namespace Dynamo.Tests.UI
{
    [TestFixture]
    public class RecordedTests
    {
        // For access within test cases.
        private DynamoController controller = null;

        [SetUp]
        public void Start()
        {
        }

        [TearDown]
        public void Exit()
        {
        }

        [Test]
        public void TestCreateNodesAndConnectors()
        {
            RunCommandsFromFile("CreateNodesAndConnectors.xml");

            WorkspaceModel workspace = controller.DynamoModel.CurrentWorkspace;
            Assert.IsNotNull(workspace, "Current workspace must not be 'null'");
            Assert.AreEqual(5, workspace.Nodes.Count);
            Assert.AreEqual(4, workspace.Connectors.Count);
        }

        private void RunCommandsFromFile(string commandFileName)
        {
            string commandFilePath = DynamoTestUI.GetTestDirectory();
            commandFilePath = Path.Combine(commandFilePath, @"core\recorded\");
            commandFilePath = Path.Combine(commandFilePath, commandFileName);

            Thread worker = new Thread(LaunchDynamoWithCommandFile);
            worker.SetApartmentState(ApartmentState.STA);
            worker.Name = "RunCommandThread";
            worker.Start(commandFilePath);
            worker.Join();
        }

        private void LaunchDynamoWithCommandFile(object parameter)
        {
            string commandFilePath = parameter as string;
            this.controller = DynamoView.MakeSandboxForUnitTest(commandFilePath);
        }
    }
}
