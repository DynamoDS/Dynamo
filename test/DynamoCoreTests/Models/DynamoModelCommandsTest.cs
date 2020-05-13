using System;
using System.IO;
using System.Xml;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using Dynamo.Utilities;
using NUnit.Framework;
using DynCmd = Dynamo.Models.DynamoModel;

namespace Dynamo.Tests.ModelsTest
{
    /// <summary>
    /// This test class will contain test method for executing methods/properties in the DynamoModel class
    /// </summary>
    [TestFixture]
    class DynamoModelCommandsTest : DynamoModelTestBase
    {
        /// <summary>
        /// This test method will execute the DynamoModel_CreateAndConnectNodeImpl method from the DynamoModel class
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void DynamoModel_CreateAndConnectNodeImpl()
        {
            //Arrange
            Guid newNodeGuid = Guid.NewGuid();
            Guid existingNodeGuid = new Guid("81c94fd0-35a0-4680-8535-00aff41192d3");
            double x = 100;
            double y = 190;

            string openPath = Path.Combine(TestDirectory, @"core\DetailedPreviewMargin_Test.dyn");
            RunModel(openPath);

            //This command is created with CreateAsDownstreamNode flas as true
            var cmdOne = new DynamoModel.CreateAndConnectNodeCommand(newNodeGuid, existingNodeGuid,
                "CoreNodeModels.CreateList", 0, 0, x, y, true, false);

            //This command is created with CreateAsDownstreamNode flas as false
            var cmdTwo = new DynamoModel.CreateAndConnectNodeCommand(newNodeGuid, existingNodeGuid,
                "CoreNodeModels.CreateList", 0, 0, x, y, false, false);

            //Act
            //This will execute the method CreateAndConnectNodeImpl for both commands
            CurrentDynamoModel.ExecuteCommand(cmdOne);
            CurrentDynamoModel.ExecuteCommand(cmdTwo);

            //Assert
            Assert.IsNotNull(cmdOne);
            Assert.IsNotNull(cmdTwo);
        }

        /// <summary>
        /// This test method will execute the method GetNodeFromCommand from the DynamoModel class
        /// The method has several flows then this test method will follow several flows
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void DynamoModel_GetNodeFromCommand()
        {
            //Arrange
            Guid newNodeGuid = Guid.NewGuid();

            var command = new DynCmd.CreateNodeCommand(newNodeGuid.ToString(), "NodeName1", 0, 0, true, true);
            var command2 = new DynCmd.CreateNodeCommand(newNodeGuid.ToString(), "CoreNodeModels.Watch", 0, 0, true, true);

            //This will execute the proxy section code in the GetNodeFromCommand() method
            var commandProxy = new DynCmd.CreateProxyNodeCommand(newNodeGuid.ToString(), "CoreNodeModels.Watch", 0, 0, true, true,"ProxyCommand1",0,0);

            //Act//Assert
            //Calling the ExecuteCommand internally executes the GetNodeFromCommand() method
            Assert.Throws<Exception>( () => CurrentDynamoModel.ExecuteCommand(command));
            CurrentDynamoModel.ExecuteCommand(command2);
            Assert.Throws<Exception>(() => CurrentDynamoModel.ExecuteCommand(commandProxy));
            
        }

        /// <summary>
        /// This test method will execute the method GetNodeFromCommand from the DynamoModel class
        /// It will follow the flow for when there is duplicate guid/name in the workspace
        /// </summary> 
        [Test]
        [Category("UnitTests")]
        public void DynamoModel_GetNodeFromCommand_DuplicatedGuids()
        {
            //Arrange
            //This guid already exist in the .dyn file (List.Create), then it will execute a specific code in GetNodeFromCommand method
            Guid existingNodeGuid = new Guid("81c94fd0-35a0-4680-8535-00aff41192d3");

            string openPath = Path.Combine(TestDirectory, @"core\DetailedPreviewMargin_Test.dyn");
            RunModel(openPath);

            var command = new DynCmd.CreateNodeCommand(existingNodeGuid.ToString(), "List.Create", 0, 0, true, true);  

            //Act
            CurrentDynamoModel.ExecuteCommand(command);

            //Assert
            //Validating that the NodeCommand was successfully created.
            Assert.IsNotNull(command);
            Assert.AreEqual(command.Name, "List.Create");
            Assert.AreEqual(command.ModelGuid.ToString(), existingNodeGuid.ToString());
        }

        /// <summary>
        /// This test method will execute the method GetNodeFromCommand from the DynamoModel class
        /// It will follow the flow for when the node provided is a XMl Element with one child
        /// </summary> 
        [Test]
        [Category("UnitTests")]
        public void DynamoModel_GetNodeFromCommand_XML()
        {
            //Arrange
            XmlDocument xmlDocument = new XmlDocument();
            XmlElement elemTest = xmlDocument.CreateElement("TestCommand");
            var helper = new XmlElementHelper(elemTest);
            //DeserializeCore method is looking for the attributes ShowErrors and CancelRun, then we need to set them up before calling the method.
            helper.SetAttribute("X", 100);
            helper.SetAttribute("Y", 100);
            helper.SetAttribute("DefaultPosition", true);
            helper.SetAttribute("TransformCoordinates", true);
            helper.SetAttribute("NodeName", "TestNode");
            helper.SetAttribute("type", "Dynamo.Graph.Nodes.CodeBlockNodeModel");


            XmlElement elemTestChild = xmlDocument.CreateElement("TestCommandChild");
            var helper2 = new XmlElementHelper(elemTestChild);
            helper2.SetAttribute("type", "Dynamo.Graph.Nodes.CodeBlockNodeModel");
            helper2.SetAttribute("ShouldFocus", true);
            helper2.SetAttribute("CodeText", "text");
            elemTest.AppendChild(elemTestChild);

            //Act
            var createNodeCommand = DynCmd.CreateNodeCommand.DeserializeCore(elemTest);
            //Calling Execute method will call internally the GetNodeFromCommand method
            createNodeCommand.Execute(CurrentDynamoModel);

            //Assert
            //Validating that the creageNodeCommand was successful
            Assert.IsNotNull(createNodeCommand);
            Assert.AreEqual(createNodeCommand.NodeXml.Name, "TestCommandChild");
        }

        
        [Test]
        [Category("UnitTests")]
        public void DynamoModel_SelectModelImpl()
        {
            //Arrange
            //This guid already exist in the .dyn file (List.Create), then it will execute a specific code in GetNodeFromCommand method
            Guid existingNodeGuid = new Guid("81c94fd0-35a0-4680-8535-00aff41192d3");

            string openPath = Path.Combine(TestDirectory, @"core\DetailedPreviewMargin_Test.dyn");
            RunModel(openPath);

            var command = new DynCmd.CreateNodeCommand(existingNodeGuid.ToString(), "List.Create", 0, 0, true, true);

            //Act
            CurrentDynamoModel.ExecuteCommand(command);

            //Assert
            //Validating that the NodeCommand was successfully created.
            Assert.IsNotNull(command);
            Assert.AreEqual(command.Name, "List.Create");
            Assert.AreEqual(command.ModelGuid.ToString(), existingNodeGuid.ToString());
        }

    }
}
