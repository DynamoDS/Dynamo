using System.Xml;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using Dynamo.Utilities;
using NUnit.Framework;
using DynCmd = Dynamo.Models.DynamoModel;

namespace Dynamo.Tests.ModelsTest
{
    [TestFixture]
    class CreateNodeCommandTest : DynamoModelTestBase
    {
        /// <summary>
        /// This test method will execute the next methods in the CreateNodeCommand class
        /// void SerializeCore(XmlElement element) for a normal Node
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void CreateNodeCommand_ExecuteCore()
        {
            //Arrange
            var cbn = CreateCodeBlockNode();
            var command = new DynCmd.CreateNodeCommand(cbn,0,0,true,true);
            XmlDocument xmlDocument = new XmlDocument();

            //Act
            CurrentDynamoModel.ExecuteCommand(command);
            XmlElement elemTest = xmlDocument.CreateElement("TestCommand");

            var xmlElement = command.Serialize(xmlDocument);

            //Assert
            Assert.IsNotNull(xmlElement);
        }

        /// <summary>
        /// This test method will execute the next methods in the CreateNodeCommand class
        /// void SerializeCore(XmlElement element) for a XML Node
        /// void TrackAnalytics()
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void CreateNodeCommandXML_ExecuteCore()
        {
            //Arrange
            XmlDocument xmlDocument = new XmlDocument();

            //Act
            XmlElement elemTest = xmlDocument.CreateElement("TestCommand");
            var helper = new XmlElementHelper(elemTest);
            //DeserializeCore method is looking for the attributes ShowErrors and CancelRun, then we need to set them up before calling the method.
            helper.SetAttribute("X", 100);
            helper.SetAttribute("Y", 100);
            helper.SetAttribute("DefaultPosition", true);
            helper.SetAttribute("TransformCoordinates", true);
            helper.SetAttribute("NodeName", "TestNode"); 
           

            XmlElement elemTestChild = xmlDocument.CreateElement("TestCommandChild");
            elemTest.AppendChild(elemTestChild);

            var createNodeCommand = DynCmd.CreateNodeCommand.DeserializeCore(elemTest);
            var serializedCommand = createNodeCommand.Serialize(xmlDocument);

            createNodeCommand.TrackAnalytics();

            //Assert
            Assert.IsNotNull(createNodeCommand);
            Assert.IsNotNull(serializedCommand);
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
