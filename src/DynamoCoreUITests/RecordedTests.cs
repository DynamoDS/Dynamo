using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Dynamo.Controls;
using NUnit.Framework;

namespace Dynamo.Tests.UI
{
    [TestFixture]
    public class RecordedTests
    {
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
            DynamoView.MakeSandboxAndRun(commandFilePath);
        }
    }
}
