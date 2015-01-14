using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Models;
using DynamoWebServer;
using DynamoWebServer.Messages;
using DynamoWebServer.Responses;
using Moq;
using NUnit.Framework;
using SuperSocket.SocketBase.Config;

namespace Dynamo.Tests
{
    class SaveUploadTests : DynamoViewModelUnitTest
    {
        protected WebServer webServer;
        protected bool proxyResponseExpected;

        [SetUp]
        public override void Init()
        {
            base.Init();

            var mock = new Mock<IWebSocket>();
            mock.Setup(ws => ws.Setup(It.IsAny<IRootConfig>(), It.IsAny<IServerConfig>())).Returns(true);
            mock.Setup(ws => ws.Start()).Returns(true);

            webServer = new WebServer(Model, mock.Object);
            webServer.Start();
        }

        #region Tests for preparation of workspace

        [Test]
        public void CanCreateWorkspaceWithData()
        {
            string commandPath = GetCommandPathByFileName("CreateWorkspaceWithDataMessage");
            string createWorkspaceCommand = File.ReadAllText(commandPath);

            webServer.ExecuteMessageFromSocket(createWorkspaceCommand, "", false);

            CheckHomeContent();
        }

        [Test]
        public void CanCreateCustomNodeWorkspace()
        {
            string commandPath = GetCommandPathByFileName("createCustomNodeMessage");
            string createCustomNodeCommand = File.ReadAllText(commandPath);

            webServer.ExecuteMessageFromSocket(createCustomNodeCommand, "", false);

            var guid = Guid.Parse(NodeGuids.CustomNodeWorkspace);
            Assert.IsTrue(Model.CustomNodeManager.LoadedCustomNodes.Contains(guid),
                "Workspace of custom" + WebServerErrorMessages.NodeWasNotCreated);
        }

        [Test]
        public void CanFillCustomNodeWithDataWorkspace()
        {
            CanCreateCustomNodeWorkspace();

            string commandPath = GetCommandPathByFileName("FillCustomNodeWithDataMessage");
            string fillCustomNodeCommand = File.ReadAllText(commandPath);

            webServer.ExecuteMessageFromSocket(fillCustomNodeCommand, "", false);

            CheckCustomNodeContent();
        }

        #endregion

        #region Tests for save functionality

        [Test]
        public void CanSaveFileFromWeb()
        {
            CanCreateWorkspaceWithData();
            CheckExecutingSaveMessage("ExpectedResultFile");
        }

        [Test]
        public void CanSaveFileFromNWK()
        {
            CanCreateWorkspaceWithData();

            var commandPath = GetCommandPathByFileName("SaveFileMessage");
            var pathToSave = GetCommandPathByFileName("temp", "dyn");
            string saveCommand = GetCommandWithPathAndGuid(File.ReadAllText(commandPath), path: pathToSave);

            webServer.ExecuteMessageFromSocket(saveCommand, "", false);

            Assert.IsTrue(File.Exists(pathToSave), WebServerErrorMessages.NoSaving);

            var actualResult = File.ReadAllText(pathToSave);
            File.Delete(pathToSave);

            var resultPath = GetCommandPathByFileName("ExpectedResultFile", "dyn");
            var expectedResult = File.ReadAllText(resultPath);

            Assert.AreEqual(expectedResult, actualResult, WebServerErrorMessages.WrongSavedFileContent);
        }

        [Test]
        public void CanSaveCustomNodeFileFromWeb()
        {
            CanFillCustomNodeWithDataWorkspace();
            var messageHandler = new MessageHandler(Model);
            string actualResult = null;

            messageHandler.ResultReady += (s, args) =>
            {
                Assert.IsInstanceOf<SavedFileResponse>(args.Response, WebServerErrorMessages.WrongResponse);
                var response = args.Response as SavedFileResponse;
                actualResult = System.Text.Encoding.Default.GetString(response.FileContent.ToArray());
            };

            var commandPath = GetCommandPathByFileName("SaveFileMessage");
            string saveCommand = GetCommandWithPathAndGuid(File.ReadAllText(commandPath),
                guid: NodeGuids.CustomNodeWorkspace);

            var message = messageHandler.DeserializeMessage(saveCommand);
            messageHandler.Execute(message, "");

            var resultPath = GetCommandPathByFileName("ExpectedResultCustomNodeFile", "dyf");
            var expectedResult = File.ReadAllText(resultPath);

            Assert.NotNull(actualResult, WebServerErrorMessages.NoSaving);
            Assert.AreEqual(expectedResult, actualResult, WebServerErrorMessages.WrongSavedFileContent);
        }

        [Test]
        public void CanSaveCustomNodeFileFromNWK()
        {
            CanFillCustomNodeWithDataWorkspace();

            var commandPath = GetCommandPathByFileName("SaveFileMessage");
            var pathToSave = GetCommandPathByFileName("Circle", "dyf");
            string saveCommand = GetCommandWithPathAndGuid(File.ReadAllText(commandPath),
                path: pathToSave, guid: NodeGuids.CustomNodeWorkspace);

            webServer.ExecuteMessageFromSocket(saveCommand, "", false);

            Assert.IsTrue(File.Exists(pathToSave), WebServerErrorMessages.NoSaving);

            var actualResult = File.ReadAllText(pathToSave);
            File.Delete(pathToSave);

            var resultPath = GetCommandPathByFileName("ExpectedResultCustomNodeFile", "dyf");
            var expectedResult = File.ReadAllText(resultPath);

            Assert.AreEqual(expectedResult, actualResult, WebServerErrorMessages.WrongSavedFileContent);
        }

        #endregion

        #region Tests for upload functionality

        [Test]
        public void CanUploadFileFromWeb()
        {
            var fileToUploadPath = GetCommandPathByFileName("TestHome", "dyn");
            var fileToUploadContent = File.ReadAllBytes(fileToUploadPath);

            CheckExecutingUploadMessage(fileToUploadContent, 7);
            
            CheckHomeContent();
        }

        [Test]
        public void CanOpenFileFromNWK()
        {
            var uploadCommandPath = GetCommandPathByFileName("UploadFileMessageEmpty");
            var fileToUploadPath = GetCommandPathByFileName("TestHome", "dyn");

            var uploadCommand = GetCommandWithPathAndGuid(File.ReadAllText(uploadCommandPath),
                path: fileToUploadPath);

            CheckExecutingUploadMessageNWK(uploadCommand, fileToUploadPath, 7);

            CheckHomeContent();
        }

        [Test]
        public void CanUploadCustomNodeFileFromWeb()
        {
            var fileToUploadPath = GetCommandPathByFileName("TestCustomNode", "dyf");
            var fileToUploadContent = File.ReadAllBytes(fileToUploadPath);

            CheckExecutingUploadMessage(fileToUploadContent, 5);
            
            CheckCustomNodeContent();
        }

        [Test]
        public void CanOpenCustomNodeFileFromNWK()
        {
            var uploadCommandPath = GetCommandPathByFileName("UploadFileMessageEmpty");
            var fileToUploadPath = GetCommandPathByFileName("TestCustomNode", "dyf");

            var uploadCommand = GetCommandWithPathAndGuid(File.ReadAllText(uploadCommandPath),
                path: fileToUploadPath);

            CheckExecutingUploadMessageNWK(uploadCommand, fileToUploadPath, 5);

            CheckCustomNodeContent();
        }

        [Test]
        public void CanUploadFileWithCustomNodeFromWeb()
        {
            var fileToUploadPath = GetCommandPathByFileName("TestHomeWithCustomNode", "dyn");
            var fileToUploadContent = File.ReadAllBytes(fileToUploadPath);

            CheckExecutingUploadMessage(fileToUploadContent, 2);

            Assert.IsTrue(Model.Nodes.Any(node => node.GUID.ToString() == NodeGuids.CustomNodeInstance),
                "Proxy custom" + WebServerErrorMessages.NodeWasNotCreated);
        }

        [Test]
        public void CanUpdateProxyNodeByUploadCustomNode()
        {
            CanUploadFileWithCustomNodeFromWeb();
            proxyResponseExpected = true;
            CanUploadCustomNodeFileFromWeb();
            Model.Home(null);
            var customNodeInstance = Model.Nodes.First(node => node.GUID.ToString() == NodeGuids.CustomNodeInstance);
            Assert.NotNull(customNodeInstance.CachedValue, WebServerErrorMessages.ProxyNodeWasNotUpdated);
            Assert.NotNull(customNodeInstance.CachedValue.Data, WebServerErrorMessages.ProxyNodeWasNotUpdated);
        }

        [Test]
        public void CanUploadAndSaveTheSameFile()
        {
            CanUploadFileFromWeb();
            CheckExecutingSaveMessage("TestHome");
        }

        #endregion

        #region Private helping methods

        string GetCommandWithPathAndGuid(string commandText, string path = "", string guid = "")
        {
            var result = commandText;
            if (!string.IsNullOrWhiteSpace(path))
            {
                result = result.Replace("\"filePath\":\"\"",
                "\"filePath\":\"" + path.Replace("\\", "\\\\") + "\"");

                result = result.Replace("\"path\":\"\"",
                "\"path\":\"" + path.Replace("\\", "\\\\") + "\"");
            }

            if (!string.IsNullOrWhiteSpace(guid))
            {
                result = result.Replace("\"guid\":\"\"", "\"guid\":\"" + guid + "\"");
            }

            return result;
        }

        void CheckHomeContent()
        {
            Assert.IsTrue(Model.Nodes.Any(node => node.GUID.ToString() == NodeGuids.CodeBlock),
                "Code block" + WebServerErrorMessages.NodeWasNotCreated);

            Assert.IsTrue(Model.Nodes.Any(node => node.GUID.ToString() == NodeGuids.Boolean),
                "Boolean" + WebServerErrorMessages.NodeWasNotCreated);

            Assert.IsTrue(Model.Nodes.Any(node => node.GUID.ToString() == NodeGuids.If),
                "If" + WebServerErrorMessages.NodeWasNotCreated);

            Assert.IsTrue(Model.Nodes.First(node => node.GUID.ToString() == NodeGuids.CodeBlock).OutPorts.Count == 1,
                WebServerErrorMessages.WrongCBNOutputPorts);
        }

        void CheckCustomNodeContent()
        {
            Assert.IsInstanceOf<CustomNodeWorkspaceModel>(Model.CurrentWorkspace);

            Assert.IsTrue(Model.Nodes.Any(node => node.GUID.ToString() == NodeGuids.Point),
                "Point" + WebServerErrorMessages.NodeWasNotCreated);

            Assert.IsTrue(Model.Nodes.Any(node => node.GUID.ToString() == NodeGuids.Circle),
                "Circle" + WebServerErrorMessages.NodeWasNotCreated);

            Assert.IsTrue(Model.Nodes.Any(node => node.GUID.ToString() == NodeGuids.Surface),
                "Surface" + WebServerErrorMessages.NodeWasNotCreated);
        }

        void CheckExecutingSaveMessage(string expectedFileName)
        {
            var messageHandler = new MessageHandler(Model);
            string actualResult = null;

            messageHandler.ResultReady += (s, args) =>
            {
                Assert.IsInstanceOf<SavedFileResponse>(args.Response, WebServerErrorMessages.WrongResponse);
                var response = args.Response as SavedFileResponse;
                actualResult = System.Text.Encoding.Default.GetString(response.FileContent.ToArray());
            };

            var commandPath = GetCommandPathByFileName("SaveFileMessage");
            string saveCommand = File.ReadAllText(commandPath);

            var message = messageHandler.DeserializeMessage(saveCommand);
            messageHandler.Execute(message, "");

            var resultPath = GetCommandPathByFileName(expectedFileName, "dyn");
            var expectedResult = File.ReadAllText(resultPath);

            Assert.NotNull(actualResult, WebServerErrorMessages.NoSaving);
            Assert.AreEqual(expectedResult, actualResult, WebServerErrorMessages.WrongSavedFileContent);
        }

        void CheckExecutingUploadMessage(byte[] fileContent, int nodesNumber)
        {
            var messageHandler = new MessageHandler(Model);
            bool proxyResponseTookPlace = false;

            messageHandler.ResultReady += (s, args) =>
            {
                if (args.Response is UpdateProxyNodesResponse)
                {
                    proxyResponseTookPlace = true;
                }
                else
                {
                    Assert.IsInstanceOf<NodeCreationDataResponse>(args.Response, WebServerErrorMessages.WrongResponse);
                    var response = args.Response as NodeCreationDataResponse;
                    Assert.NotNull(response.Nodes, "No nodes in NodeCreationResponse");
                    Assert.AreEqual(response.Nodes.Count(), nodesNumber, WebServerErrorMessages.WrongNodesInResponse);
                }
            };

            var message = new UploadFileMessage(fileContent);
            messageHandler.Execute(message, "");
            
            Assert.AreEqual(proxyResponseExpected, proxyResponseTookPlace, 
                "UpdateProxyNodesResponse" + (proxyResponseTookPlace ? "took" : "didn't take") + "place");
            // reset for reusing
            proxyResponseExpected = false;
        }

        void CheckExecutingUploadMessageNWK(string uploadCommand, string expectedResultPath, int nodesNumber)
        {
            var messageHandler = new MessageHandler(Model);

            messageHandler.ResultReady += (s, args) =>
            {
                if (args.Response is WorkspacePathResponse)
                {
                    var wpResponse = args.Response as WorkspacePathResponse;
                    Assert.AreEqual(wpResponse.Path, expectedResultPath, WebServerErrorMessages.WrongPathInResponse);
                }
                else if (args.Response is NodeCreationDataResponse)
                {
                    var ncdResponse = args.Response as NodeCreationDataResponse;
                    Assert.NotNull(ncdResponse.Nodes, "No nodes in NodeCreationResponse");
                    Assert.AreEqual(ncdResponse.Nodes.Count(), nodesNumber, WebServerErrorMessages.WrongNodesInResponse);
                }
                else
                {
                    Assert.IsTrue(false, WebServerErrorMessages.WrongResponse);
                }
            };

            var message = messageHandler.DeserializeMessage(uploadCommand);
            messageHandler.Execute(message, "");
        }

        #endregion
    }
}
