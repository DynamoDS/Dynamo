using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Dynamo.Extensions;
using Dynamo.Models;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Dynamo.Tests.ModelsTest
{
    [TestFixture]
    class RecordableCommandsTest
    {
        Mock<IExtension> extMock;
        ICommandExecutive executive;
        int cmdExecutionState = -1;

        private void ExtensionReadyCallback(ReadyParams ready)
        {
            executive = ready.CommandExecutive;
            Assert.IsTrue(ready.WorkspaceModels.Any());
            Assert.IsNotNull(ready.CurrentWorkspaceModel);
        }

        [SetUp]
        public void Init()
        {
            extMock = new Mock<IExtension>();
            extMock.Setup(ext => ext.Ready(It.IsAny<ReadyParams>())).Callback((ReadyParams r) => ExtensionReadyCallback(r));
            cmdExecutionState = -1;

        }

        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestRecordableCommand()
        {
            var cmd = new Mock<DynamoModel.RecordableCommand>(null);

            var pausePlaybackCommand = new DynamoModel.PausePlaybackCommand(20);

            var xmlElements = pausePlaybackCommand.Serialize();
    
            Assert.IsNotNull(JToken.Parse(xmlElements));

            Assert.Throws<System.Reflection.TargetInvocationException>(() => executive.ExecuteCommand(cmd.Object, "TestRecordable", "ExtensionTests"));
            Assert.AreEqual(-1, cmdExecutionState);

        }

        [Test]
        [Category("UnitTests")]
        public void TestRecordableCommandDeserialize()
        {
            var cmd = new Mock<DynamoModel.RecordableCommand>(null);

            var pausePlaybackCommand = new DynamoModel.AddPresetCommand("PresetName","Description");

            var xmlElements = pausePlaybackCommand.Serialize();

            // Serialize the command into an XmlElement.
            XmlDocument xmlDocument = new XmlDocument();
            XmlElement element = pausePlaybackCommand.Serialize(xmlDocument);
            Assert.IsNotNull(element);

            // Deserialized the XmlElement into a new instance of the command.
            var duplicate = DynamoModel.RecordableCommand.Deserialize(element);

            Assert.IsNotNull(JToken.Parse(xmlElements));

            Assert.Throws<System.Reflection.TargetInvocationException>(() => executive.ExecuteCommand(cmd.Object, "TestRecordable", "ExtensionTests"));
            Assert.AreEqual(-1, cmdExecutionState);

        }
    }
}
