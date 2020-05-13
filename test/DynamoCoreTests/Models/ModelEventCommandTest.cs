using System;
using System.Collections.Generic;
using System.Xml;
using Dynamo.Graph.Nodes;
using NUnit.Framework;
using static Dynamo.Models.DynamoModel;
using DynCmd = Dynamo.Models.DynamoModel;

namespace Dynamo.Tests.ModelsTest
{
    [TestFixture]
    class ModelEventCommandTest : DynamoModelTestBase
    {
        /// <summary>
        /// This test method will execute the next constructors from the ModelEventCommand class
        /// public ModelEventCommand(Guid modelGuid, string eventName, int value)
        /// public ModelEventCommand(Guid modelGuid, string eventName)
        /// public ModelEventCommand(IEnumerable<Guid> modelGuid, string eventName)
        /// void SerializeCore(XmlElement element)
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void ModelEventCommand_Constructor()
        {
            //Arrange
            var cbn = CreateCodeBlockNode();
            IEnumerable<Guid> modelGuid = new[] { cbn.GUID };
            XmlDocument xmlDocument = new XmlDocument();

            //Act
            XmlElement elemTest = xmlDocument.CreateElement("TestModelEventCommand");
            var command1 = new ModelEventCommand(cbn.GUID,"testEvent1",1);
            var command2 = new ModelEventCommand(cbn.GUID, "testEvent2");
            var command3 = new ModelEventCommand(modelGuid, "testEvent3");         
            var xmlSerializedCommand = command1.Serialize(xmlDocument);

            //Assert
            //This section validates that the commands were created correctly and the Serialization was successfull
            Assert.IsNotNull(command1);
            Assert.AreEqual(command1.EventName, "testEvent1");
            Assert.IsNotNull(command2);
            Assert.AreEqual(command2.EventName, "testEvent2");
            Assert.IsNotNull(command3);
            Assert.AreEqual(command3.EventName, "testEvent3");
            Assert.IsNotNull(xmlSerializedCommand);

        }

        private CodeBlockNodeModel CreateCodeBlockNode()
        {
            var cbn = new CodeBlockNodeModel(CurrentDynamoModel.LibraryServices);
            var command = new DynCmd.CreateNodeCommand(cbn, 0, 0, true, false);

            CurrentDynamoModel.ExecuteCommand(command);

            Assert.IsNotNull(cbn);
            return cbn;
        }
    }
}
