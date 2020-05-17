using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CoreNodeModels.Input;
using Dynamo.Extensions;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using Dynamo.Selection;
using Dynamo.Utilities;
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

        /// <summary>
        /// This method will be executed as callback when Dynamo is Ready
        /// </summary>
        /// <param name="ready"></param>
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

        /// <summary>
        /// This Test Method will execute the next methods in the RecordableCommands class
        /// protected RecordableCommand(string tag)
        /// internal string Serialize()
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestRecordableCommand()
        {
            //Arrange
            //The tag passed as parameter is null, then it will raise an exception later
            var cmd = new Mock<DynamoModel.RecordableCommand>(null);
            var pausePlaybackCommand = new DynamoModel.PausePlaybackCommand(20);

            //Act
            var xmlElements = pausePlaybackCommand.Serialize();
    
            //Assert
            Assert.IsNotNull(JToken.Parse(xmlElements));//Verify that the json serialized is valid

            //This will execute the exception section in RecordableCommand(string tag), because tag is null
            Assert.Throws<System.Reflection.TargetInvocationException>(() => executive.ExecuteCommand(cmd.Object, "TestRecordable", "ExtensionTests"));
            Assert.AreEqual(-1, cmdExecutionState);//It means that the command was not executed successfully
        }

        /// <summary>
        /// This Test Method will execute the neext methods in RecordableCommands class
        /// static RecordableCommand Deserialize(string jsonString)
        /// internal virtual void TrackAnalytics() 
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestRecordableCommandDeserialize()
        {
            //Arrange
            var numberNode1 = new DoubleInput();
            numberNode1.Value = "1";
            var numberNode2 = new DoubleInput();
            numberNode2.Value = "2";

            DynamoSelection.Instance.ClearSelection();
            //create the first state with the numbers selected
            DynamoSelection.Instance.Selection.Add(numberNode1);
            DynamoSelection.Instance.Selection.Add(numberNode2);

            var ids = DynamoSelection.Instance.Selection.OfType<NodeModel>().Select(x => x.GUID).ToList();
            var AddPresetCommand = new DynamoModel.AddPresetCommand("PresetName","Description",ids);
            
            // Serialize the command into an XmlElement.
            XmlDocument xmlDocument = new XmlDocument();
            XmlElement elementAddPresetCommand = AddPresetCommand.Serialize(xmlDocument);

            var jsonAddPresetCommand = AddPresetCommand.Serialize();


            var helper = new XmlElementHelper(elementAddPresetCommand);
            Guid gWorkspace = Guid.NewGuid();
            Guid gState = Guid.NewGuid();
            var ApplyPresetCommand = new DynamoModel.ApplyPresetCommand(gWorkspace, gState);

            //This will execute the empty method TrackAnalytics()
            ApplyPresetCommand.TrackAnalytics();
            XmlElement elementApplyPresetCommand = ApplyPresetCommand.Serialize(xmlDocument);

            //Act
            // Deserialized the XmlElement into a new instance of the command.
            var deserializedAddPresetCommand = DynamoModel.RecordableCommand.Deserialize(elementAddPresetCommand);
            var deserializedApplyPresetCommand = DynamoModel.RecordableCommand.Deserialize(elementApplyPresetCommand);

            //This will execute the overloaded Deserialize method (the one receiving a string as parameter)
            var deserializedFromJson = DynamoModel.RecordableCommand.Deserialize(jsonAddPresetCommand);

            //This will generate a invalid json string, so when it's deserialized will raise an exception
            jsonAddPresetCommand = jsonAddPresetCommand.Replace('{', '<');

            //This is a fake command so when it's deserialized will raise an exception
            XmlElement elemTest = xmlDocument.CreateElement("TestCommand");
            Assert.Throws<ArgumentException>(() => DynamoModel.RecordableCommand.Deserialize(elemTest));

            //Assert
            //This will check that the Deserialized commands are valid
            Assert.IsNotNull(deserializedAddPresetCommand);
            Assert.IsNotNull(deserializedApplyPresetCommand);
            Assert.IsNotNull(deserializedFromJson);
            Assert.Throws<ApplicationException>(() => DynamoModel.RecordableCommand.Deserialize(jsonAddPresetCommand));


        }
    }
}
