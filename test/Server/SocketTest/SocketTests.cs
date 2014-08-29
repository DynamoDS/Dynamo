using System.IO;
using System.Linq;
using Dynamo.Nodes;
using DynamoWebServer;
using DynamoWebServer.Messages;

using Moq;

using NUnit.Framework;

using SuperSocket.SocketBase.Config;

namespace Dynamo.Tests
{
    class SocketTests : DynamoViewModelUnitTest
    {
        private const string GUID = "b43c1f0e-d88f-bfd7-8dd8-dc5536c18390";
        private WebServer webServer;
        private Mock<IWebSocket> mock;

        [SetUp]
        public override void Init()
        {
            base.Init();

            mock = new Mock<IWebSocket>();
            mock.Setup(ws => ws.Setup(It.IsAny<IRootConfig>(), It.IsAny<IServerConfig>())).Returns(true);
            mock.Setup(ws => ws.Start()).Returns(true);

            webServer = new WebServer(ViewModel, mock.Object);
            webServer.Start();
        }

        [Test]
        public void CanDeserialize()
        {
            var testDir = Path.Combine(GetTestDirectory(), @"core\commands");
            var commandPaths = Directory.GetFiles(testDir, "*.txt");
            var messageHandler = new MessageHandler(ViewModel);
            Assert.NotNull(commandPaths);
            Assert.Greater(commandPaths.Length, 0);
            foreach (var path in commandPaths)
            {
                var text = File.ReadAllText(path);
                var message = messageHandler.DeserializeMessage(text);
                Assert.NotNull(message);
            }
        }

        [Test]
        public void CanExecuteCreateCommand()
        {
            var model = ViewModel.Model;
            string commandPath = Path.Combine(GetTestDirectory(), @"core\commands\createNode.txt");
            string createCommand = File.ReadAllText(commandPath);

            webServer.ExecuteMessageFromSocket(createCommand, "");

            Assert.IsTrue(model.Nodes.Any(node => node.GUID.ToString() == GUID));
        }
        
        [Test]
        public void CanExecuteUpdateCommand()
        {
            var model = ViewModel.Model;
            CanExecuteCreateCommand();

            string commandPath = Path.Combine(GetTestDirectory(), @"core\commands\updateNode.txt");
            string updateCommand = File.ReadAllText(commandPath);

            webServer.ExecuteMessageFromSocket(updateCommand, "");

            var doubleInput = model.Nodes.First(
                    node => node.GUID.ToString() == GUID) as DoubleInput;

            Assert.NotNull(doubleInput);

            Assert.AreEqual(doubleInput.Value, "17.6");
        }

        [Test]
        public void CanExecuteDeleteCommand()
        {
            var model = ViewModel.Model;
            CanExecuteCreateCommand();

            string commandPath = Path.Combine(GetTestDirectory(), @"core\commands\deleteNode.txt");
            string deleteCommand = File.ReadAllText(commandPath);

            webServer.ExecuteMessageFromSocket(deleteCommand, "");
            Assert.IsFalse(model.Nodes.Any(node => node.GUID.ToString() == GUID));
        }

        [Test]
        public void CanSetupServer()
        {
            mock.Verify(m => m.Setup(It.IsAny<IRootConfig>(),It.IsAny<IServerConfig>()));
        }

        [Test]
        public void CanStartServer()
        {
            mock.Verify(m => m.Start());
        }

        [Test]
        public void CanProcessResponse()
        {
            var response = Mock.Of<Response>();

            webServer.SendResponse(response, "TestID");

            mock.Verify(m => m.GetAppSessionByID("TestID"));
        }
    }
}
