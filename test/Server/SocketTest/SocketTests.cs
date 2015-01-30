using System;
using System.IO;
using System.Linq;

using Dynamo.Nodes;

using DynamoWebServer;
using DynamoWebServer.Messages;
using DynamoWebServer.Responses;

using Moq;

using NUnit.Framework;

using SuperSocket.SocketBase.Config;

namespace Dynamo.Tests
{
    class SocketTests : DynamoViewModelUnitTest
    {
        WebServer webServer;
        Mock<IWebSocket> mock;

        [SetUp]
        public override void Init()
        {
            base.Init();

            mock = new Mock<IWebSocket>();
            mock.Setup(ws => ws.Setup(It.IsAny<IRootConfig>(), It.IsAny<IServerConfig>())).Returns(true);
            mock.Setup(ws => ws.Start()).Returns(true);

            webServer = new WebServer(Model, mock.Object);
            webServer.Start();
        }

        [Test]
        public void CanDeserialize()
        {
            var testDir = Path.Combine(GetTestDirectory(), @"core\commands");
            var commandPaths = Directory.GetFiles(testDir, "*Message.txt");
            var messageHandler = new MessageHandler(Model);
            Assert.NotNull(commandPaths);
            Assert.Greater(commandPaths.Length, 0);
            foreach (var path in commandPaths)
            {
                var text = File.ReadAllText(path);
                var message = messageHandler.DeserializeMessage(text);
                Assert.NotNull(message);
            }

            Assert.Throws<ArgumentException>(() => messageHandler.DeserializeMessage("Wrong data"));
        }

        [Test]
        public void CanSerialize()
        {
            var messageHandler = new MessageHandler(Model);
            var message = new RunCommandsMessage();

            var json = messageHandler.Serialize(message);
            Assert.NotNull(json);
            json = messageHandler.Serialize(null);
            Assert.Null(json);
        }

        [Test]
        public void CanExecuteCreateCommand()
        {
            string commandPath = GetCommandPathByFileName("createNodeMessage");
            string createCommand = File.ReadAllText(commandPath);

            webServer.ExecuteMessageFromSocket(createCommand, "", false);

            Assert.IsTrue(Model.Nodes.Any(node => node.GUID.ToString() == NodeGuids.Number));
        }

        [Test]
        public void CanExecuteUpdateCommand()
        {
            CanExecuteCreateCommand();

            string commandPath = GetCommandPathByFileName("updateNodeMessage");
            string updateCommand = File.ReadAllText(commandPath);

            webServer.ExecuteMessageFromSocket(updateCommand, "", false);

            var doubleInput = Model.Nodes.First(
                    node => node.GUID.ToString() == NodeGuids.Number) as DoubleInput;

            Assert.NotNull(doubleInput);

            Assert.AreEqual(doubleInput.Value, "17.6");
        }

        [Test]
        public void CanExecuteDeleteCommand()
        {
            CanExecuteCreateCommand();

            string commandPath = GetCommandPathByFileName("deleteNodeMessage");
            string deleteCommand = File.ReadAllText(commandPath);

            webServer.ExecuteMessageFromSocket(deleteCommand, "", false);
            Assert.IsFalse(Model.Nodes.Any(node => node.GUID.ToString() == NodeGuids.Number));
        }

        [Test]
        public void CanClearWorkspace()
        {
            CanExecuteCreateCommand();

            Assert.IsTrue(Model.Nodes.Any());

            string commandPath = GetCommandPathByFileName("clearWorkspaceMessage");
            string clearWorkspace = File.ReadAllText(commandPath);

            webServer.ExecuteMessageFromSocket(clearWorkspace, "", false);

            Assert.IsFalse(Model.Nodes.Any());
        }

        [Test]
        public void CanExecuteCreateCommandQueue()
        {
            string commandPath = GetCommandPathByFileName("createNodeMessage");
            string createCommand = File.ReadAllText(commandPath);

            webServer.ExecuteMessageFromSocket(createCommand, "", true);

            Assert.IsFalse(Model.Nodes.Any());
        }

        [Test]
        public void CanSetupServer()
        {
            mock.Verify(m => m.Setup(It.IsAny<IRootConfig>(), It.IsAny<IServerConfig>()));
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

            webServer.SendResponse(response, "TestId");

            mock.Verify(m => m.GetAppSessionById("TestId"));
        }

        [Test]
        public void CanCreateCodeBlockNode() 
        {
            string commandPath = GetCommandPathByFileName("codeBlockMessage");
            string createCommand = File.ReadAllText(commandPath);

            webServer.ExecuteMessageFromSocket(createCommand, "", false);

            Assert.IsTrue(Model.Nodes.Any(node => node.GUID.ToString() == NodeGuids.CodeBlock));
        }

        [Test]
        public void CanSendCodeBlockNodeUpdate()
        {
            var commandPath = GetCommandPathByFileName("codeBlockMessage");
            var createCommand = File.ReadAllText(commandPath);
            var messageHandler = new MessageHandler(Model);
            string actualResult = null;
            messageHandler.ResultReady += (s, args) =>
            {
                Assert.IsInstanceOf<CodeBlockDataResponse>(args.Response, WebServerErrorMessages.WrongResponse);
                var response = args.Response as CodeBlockDataResponse;
                actualResult = response.Data;
            };

            Message message = messageHandler.DeserializeMessage(createCommand);
            messageHandler.Execute(message, "");

            var resultPath = GetCommandPathByFileName("codeBlockExpectedResult");
            var expectedResult = File.ReadAllText(resultPath);

            Assert.AreEqual(expectedResult, actualResult, "Code block data is wrong");
        }
    }
}
