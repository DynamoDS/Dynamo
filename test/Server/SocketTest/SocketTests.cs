using System.IO;
using System.Linq;

using Dynamo.Nodes;
using Dynamo.Utilities;

using DynamoWebServer;
using DynamoWebServer.Responses;

using Moq;

using NUnit.Framework;

namespace Dynamo.Tests
{
    class SocketTests : DynamoUnitTest
    {
        private const string GUID = "514e751f-106a-bed2-9025-163962ca7abc";
        private Mock<IServer> mock;

        [SetUp]
        public override void Init()
        {
            base.Init();

            mock = new Mock<IServer>();
            dynSettings.EnableServer(mock.Object);
        }

        [Test]
        public void CanExecuteCreateCommand()
        {
            string commandPath = Path.Combine(GetTestDirectory(), @"core\commands\createNode.txt");
            string createCommand = File.ReadAllLines(commandPath)[0];

            dynSettings.ExecuteMessageFromSocket(createCommand, "");

            Assert.IsTrue(Controller.DynamoModel.Nodes.Any(node => node.GUID.ToString() == GUID));
        }

        [Test]
        public void CanExecuteUpdateCommand()
        {
            CanExecuteCreateCommand();

            string commandPath = Path.Combine(GetTestDirectory(), @"core\commands\updateNode.txt");
            string updateCommand = File.ReadAllLines(commandPath)[0];

            dynSettings.ExecuteMessageFromSocket(updateCommand, "");

            var doubleInput = Controller.DynamoModel.Nodes.First(
                    node => node.GUID.ToString() == GUID) as DoubleInput;

            Assert.NotNull(doubleInput);

            Assert.AreEqual(doubleInput.Value, "17.6");
        }

        [Test]
        public void CanExecuteDeleteCommand()
        {
            CanExecuteCreateCommand();

            string commandPath = Path.Combine(GetTestDirectory(), @"core\commands\deleteNode.txt");
            string deleteCommand = File.ReadAllLines(commandPath)[0];

            dynSettings.ExecuteMessageFromSocket(deleteCommand, "");
            Assert.IsFalse(Controller.DynamoModel.Nodes.Any(node => node.GUID.ToString() == GUID));
        }

        [Test]
        public void CanStartServer()
        {
            mock.Verify(m => m.Start());
        }

        [Test]
        public void CanSendResponse()
        {
            dynSettings.SendAnswerToWebSocket("Test message", "Test Id");

            mock.Verify(m => m.SendResponse(It.IsAny<Response>(), It.IsAny<string>()));
        }
    }
}
