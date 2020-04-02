using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Dynamo.Graph.Nodes;
using Dynamo.Migration;
using NUnit.Framework;

namespace Dynamo.Tests.Migrations
{
    [TestFixture]
    public class MigrationTests : DynamoModelTestBase
    {

        #region Migration
        /// <summary>
        /// This method will check the class Migration (The class just stores the version and the action)
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestInternalMigration()
        {
            //Arrange
            var version = new Version(224, 4, 0);

            //The Action is a delegate parameterless and return void
            var dynamoAction = new Action(() =>
            {
                Console.WriteLine("Upgrading Version Workspace");
            });

            //Act
            //The Migration class receives as parameters the version and the action, then they are assigned to internal properties
            Migration.Migration migrationTest = new Migration.Migration(version, dynamoAction);
            //Assert
            //Just checking that the values send as parameters match the 
            Assert.AreEqual(migrationTest.Version, version);
            Assert.AreEqual(migrationTest.Upgrade, dynamoAction);
        }
        #endregion

        #region MigrationManager
        /// <summary>
        /// This test method will check the function MigrationManager.CreateFunctionNode method
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestCreateFunctionNode()
        {
            //Arrange
            string documentDynPath = Path.Combine(TestDirectory, @"core\Angle.dyn");
            XmlDocument xmlDoc = new XmlDocument();
            XmlDocument xmlOldNodeDoc = new XmlDocument();
            xmlOldNodeDoc.LoadXml("<Dynamo.Nodes.DSFunction type=\"Dynamo.Nodes.DSFunction\" x=\"203.6\" y=\"22.8\"/>");
            XmlElement xmlOldNode = xmlOldNodeDoc.DocumentElement;
            XmlElement xmlResult = null;

            //Act
            if (File.Exists(documentDynPath))
            {
                xmlDoc.Load(documentDynPath);
                xmlResult = MigrationManager.CreateFunctionNode(xmlDoc, xmlOldNode, 0, "DSCoreNodes.dll", "Math.DegreesToRadians", "DSCore.Math.DegreesToRadians@double");
            }

            //Assert
            //Check that all the attibutes where generated correctly
            Assert.IsNotNull(xmlResult);
            Assert.AreEqual(xmlResult.GetAttribute("type"), "Dynamo.Graph.Nodes.ZeroTouch.DSFunction");
            Assert.AreEqual(xmlResult.GetAttribute("assembly"), "DSCoreNodes.dll");
            Assert.AreEqual(xmlResult.GetAttribute("nickname"), "Math.DegreesToRadians");
            Assert.AreEqual(xmlResult.GetAttribute("function"), "DSCore.Math.DegreesToRadians@double");
        }

        /// <summary>
        /// This test method will check the MigrationManager.CreateVarArgFunctionNode method
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestCreateVarArgFunctionNode()
        {
            //Arrange
            string documentDynPath = Path.Combine(TestDirectory, @"core\dsevaluation\Defect_MAGN_2375_3487.dyn");
            XmlDocument xmlDoc = new XmlDocument();
            XmlDocument xmlOldNodeDoc = new XmlDocument();
            xmlOldNodeDoc.LoadXml("<Dynamo.Graph.Nodes.ZeroTouch.DSVarArgFunction type=\"Dynamo.Graph.Nodes.ZeroTouch.DSVarArgFunction\" x=\"203.6\" y=\"220\"/>");
            XmlElement xmlOldNode = xmlOldNodeDoc.DocumentElement;
            XmlElement xmlResult = null;

            //Act
            if (File.Exists(documentDynPath))
            {
                xmlDoc.Load(documentDynPath);
                xmlResult = MigrationManager.CreateVarArgFunctionNode(xmlDoc, xmlOldNode, 0, "DSCoreNodes.dll", "String.Split", "DSCore.String.Split@string,string[]", "2");
            }

            //Assert
            //Check that all the attibutes where generated correctly
            Assert.IsNotNull(xmlResult);
            Assert.AreEqual(xmlResult.GetAttribute("type"), "Dynamo.Graph.Nodes.ZeroTouch.DSVarArgFunction");
            Assert.AreEqual(xmlResult.GetAttribute("assembly"), "DSCoreNodes.dll");
            Assert.AreEqual(xmlResult.GetAttribute("nickname"), "String.Split");
            Assert.AreEqual(xmlResult.GetAttribute("function"), "DSCore.String.Split@string,string[]");
        }

        /// <summary>
        /// This test method will check the MigrationManager.CreateCodeBlockNodeModelNode method
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestCreateCodeBlockNodeModelNode()
        {
            //Arrange
            string documentDynPath = Path.Combine(TestDirectory, @"core\NodeStates.dyn");
            XmlDocument xmlDoc = new XmlDocument();
            XmlDocument xmlOldNodeDoc = new XmlDocument();
            xmlOldNodeDoc.LoadXml("<Dynamo.Graph.Nodes.CodeBlockNodeModel type=\"Dynamo.Graph.Nodes.CodeBlockNodeModel\"  x=\"203.6\" y=\"220\"/>");
            XmlElement xmlOldNode = xmlOldNodeDoc.DocumentElement;
            XmlElement xmlResult = null;

            //Act
            if (File.Exists(documentDynPath))
            {
                xmlDoc.Load(documentDynPath);
                xmlResult = MigrationManager.CreateCodeBlockNodeModelNode(xmlDoc, xmlOldNode, 0, "1");
            }

            //Assert
            //Check that all the attibutes where generated correctly
            Assert.IsNotNull(xmlResult);
            Assert.AreEqual(xmlResult.GetAttribute("type"), "Dynamo.Graph.Nodes.CodeBlockNodeModel");
            Assert.AreEqual(xmlResult.GetAttribute("nickname"), "Code Block");
            Assert.AreEqual(xmlResult.GetAttribute("CodeText"), "1");
        }

        /// <summary>
        /// This test method will check the MigrationManager.CreateNode method
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestCreateNode()
        {
            //Arrange
            string documentDynPath = Path.Combine(TestDirectory, @"core\NodeStates.dyn");
            XmlDocument xmlDoc = new XmlDocument();
            XmlDocument xmlOldNodeDoc = new XmlDocument();
            xmlOldNodeDoc.LoadXml("<Dynamo.Graph.Nodes.CodeBlockNodeModel type=\"Dynamo.Graph.Nodes.CodeBlockNodeModel\"  x=\"203.6\" y=\"220\"/>");
            XmlElement xmlOldNode = xmlOldNodeDoc.DocumentElement;
            XmlElement xmlResult = null;

            //Act
            if (File.Exists(documentDynPath))
            {
                xmlDoc.Load(documentDynPath);
                xmlResult = MigrationManager.CreateNode(xmlDoc, xmlOldNode, 0, "Dynamo.Graph.Nodes.CodeBlockNodeModel", "CodeBlock1");
            }

            //Assert
            //Check that all the attibutes where generated correctly
            Assert.IsNotNull(xmlResult);
            Assert.AreEqual(xmlResult.GetAttribute("type"), "Dynamo.Graph.Nodes.CodeBlockNodeModel");
            Assert.AreEqual(xmlResult.GetAttribute("nickname"), "CodeBlock1");
        }

        /// <summary>
        /// This test method will check the MigrationManager.CreateCustomNodeFrom method
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestCreateCustomNodeFrom()
        {
            //Arrange
            string documentDynPath = Path.Combine(TestDirectory, @"core\NodeStates.dyn");
            XmlDocument xmlDoc = new XmlDocument();
            XmlDocument xmlOldNodeDoc = new XmlDocument();
            xmlOldNodeDoc.LoadXml("<Dynamo.Graph.Nodes.CodeBlockNodeModel type=\"Dynamo.Graph.Nodes.CodeBlockNodeModel\"  x=\"203.6\" y=\"220\"/>");
            XmlElement xmlOldNode = xmlOldNodeDoc.DocumentElement;
            XmlElement xmlResult = null;

            //Act
            if (File.Exists(documentDynPath))
            {
                List<string> nodeInputs = new List<string>() { "input1", "input2" };
                List<string> nodeOutputs = new List<string>() { "output1", "output2" };

                xmlDoc.Load(documentDynPath);
                xmlResult = MigrationManager.CreateCustomNodeFrom(xmlDoc, xmlOldNode, "TestCodeBlock","CodeBlock1","Just creating a code block for testing functions", nodeInputs, nodeOutputs);
            }

            //Assert
            //Check that all the attibutes where generated correctly
            Assert.IsNotNull(xmlResult);
            Assert.AreEqual(xmlResult.ChildNodes.Item(0).Attributes.Item(0).Value, "TestCodeBlock");
            Assert.AreEqual(xmlResult.ChildNodes.Item(1).Attributes.Item(0).Value, "CodeBlock1");
            Assert.AreEqual(xmlResult.GetAttribute("type"), "Dynamo.Graph.Nodes.CustomNodes.Function");    
        }
        #endregion

        #region NodeMigrationData
        /// <summary>
        /// This test method will check the NodeMigrationData.FindConnector method
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestFindConnector()
        {
            //Arrange
            NodeMigrationData migrationDataTest = null; 
            string documentDynPath = Path.Combine(TestDirectory, @"core\NodeStates.dyn");
            XmlDocument xmlDoc = new XmlDocument();           
            string startGuid = "0db97f86-fbd8-4195-b2a0-aa0e78ca1d72";
            string endGuid = "50219c24-e583-4b85-887c-409fb062da6e";

            //Act
            xmlDoc.Load(documentDynPath);
            migrationDataTest = new NodeMigrationData(xmlDoc);
            PortId startPort = new PortId(startGuid, 0, PortType.Input);
            PortId endPort = new PortId(endGuid, 1, PortType.Input); ;
            //This will find a existing connector inside the NodeStates.dyn file
            var xmlNodeConnector = migrationDataTest.FindConnector(startPort, endPort);

            //Assert
            //Just check that the new node created is not null
            Assert.IsNotNull(xmlNodeConnector);
        }

        /// <summary>
        /// This test method will check the NodeMigrationData.FindFirstConnector method
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestFindFirstConnector()
        {
            //Arrange
            NodeMigrationData migrationDataTest = null;
            string documentDynPath = Path.Combine(TestDirectory, @"core\Angle.dyn");
            XmlDocument xmlDoc = new XmlDocument();
            string endGuid = "dcd9c6c6-6350-4292-a553-c57a764504b4";

            //Act
            xmlDoc.Load(documentDynPath);
            migrationDataTest = new NodeMigrationData(xmlDoc);
            PortId portId = new PortId(endGuid, 0, PortType.Input);

            //This method finds the first connector inside the Angle.dyn file
            var xmlNodeConnector = migrationDataTest.FindFirstConnector(portId);

            //Assert
            Assert.IsNotNull(xmlNodeConnector);
        }

        /// <summary>
        /// This test method will check the NodeMigrationData.RemoveFirstConnector method
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestRemoveFirstConnector()
        {
            //Arrange
            NodeMigrationData migrationDataTest = null;
            string documentDynPath = Path.Combine(TestDirectory, @"core\Angle.dyn");
            XmlDocument xmlDoc = new XmlDocument();

            //It will delete the connector with the next 'end' (the one that owns the connector)
            string endGuid = "dcd9c6c6-6350-4292-a553-c57a764504b4";

            //Act
            xmlDoc.Load(documentDynPath);
            migrationDataTest = new NodeMigrationData(xmlDoc);
            PortId portId = new PortId(endGuid, 0, PortType.Input);

            migrationDataTest.RemoveFirstConnector(portId);//Removes with one that ends with dcd9c6c6-6350-4292-a553-c57a764504b4

            //This connector won't be found because it was removed previosly - search the connector that ends with dcd9c6c6-6350-4292-a553-c57a764504b4
            var xmlDeletedConnectorNode = xmlDoc.SelectSingleNode("descendant::Dynamo.Graph.Connectors.ConnectorModel[@end='dcd9c6c6-6350-4292-a553-c57a764504b4']");

            //This connect might be found because already existed inside the document - search the connector that ends with 67f40bd6-cdee-470a-9aa5-2c9d6f6c87bc
            var xmlExixtingConnectorNode = xmlDoc.SelectSingleNode("descendant::Dynamo.Graph.Connectors.ConnectorModel[@end='67f40bd6-cdee-470a-9aa5-2c9d6f6c87bc']");

            //Assert
            Assert.IsNull(xmlDeletedConnectorNode);//Check that the xmlDeletedConnectorNode is null (was not found inside Angle.dyn)
            Assert.IsNotNull(xmlExixtingConnectorNode);//Check that the xmlExixtingConnectorNode is not null (was found inside Angle.dyn)
        }

        /// <summary>
        /// This test method will check the NodeMigrationData.FindConnectors method
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestFindConnectors()
        {
            //Arrange
            NodeMigrationData migrationDataTest = null;
            string documentDynPath = Path.Combine(TestDirectory, @"core\Angle.dyn");
            XmlDocument xmlDoc = new XmlDocument();
            string endGuid = "67f40bd6-cdee-470a-9aa5-2c9d6f6c87bc";

            //Act
            xmlDoc.Load(documentDynPath);
            migrationDataTest = new NodeMigrationData(xmlDoc);
            PortId portId = new PortId(endGuid, 0, PortType.Input);

            //This call will find several connectors in the Angle.dyn file
            var connectorsFound = migrationDataTest.FindConnectors(portId);

            //Assert
            Assert.IsNotNull(connectorsFound); //It means that at least one connector was found

            //For the file Angle.dyn will find just one connector then it will validate that the end property is valid
            foreach (var connector in connectorsFound)
            {
                Assert.AreEqual(connector.GetAttribute("end"), "67f40bd6-cdee-470a-9aa5-2c9d6f6c87bc");
            }
            
        }

        /// <summary>
        /// This test method will check the NodeMigrationData.ReconnectToPort method
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestReconnectToPort()
        {
            //At the beginning this the port setup
            //connector1 start="dcd9c6c6-6350-4292-a553-c57a764504b4" start_index="0" end="67f40bd6-cdee-470a-9aa5-2c9d6f6c87bc"
            //connector2 start="c1d6f9b4-3f45-4502-b4b8-27fd03491388" start_index="0" end="dcd9c6c6-6350-4292-a553-c57a764504b4"

            //Arrange
            NodeMigrationData migrationDataTest = null;
            string documentDynPath = Path.Combine(TestDirectory, @"core\Angle.dyn");
            XmlDocument xmlDoc = new XmlDocument();
            string endGuidFind = "67f40bd6-cdee-470a-9aa5-2c9d6f6c87bc";
           
            PortId portIdFind = new PortId(endGuidFind, 0, PortType.Input);
            string endGuidReconnect = "c1d6f9b4-3f45-4502-b4b8-27fd03491388";
            PortId portIdReconnect = new PortId(endGuidReconnect, 0, PortType.Input);

            //Act
            xmlDoc.Load(documentDynPath);
            migrationDataTest = new NodeMigrationData(xmlDoc);
            //We have to search for the connector1 end="67f40bd6-cdee-470a-9aa5-2c9d6f6c87bc", inside de xml file.
            var xmlNodeConnector = migrationDataTest.FindFirstConnector(portIdFind);

            //The function will reconnect the connector1 from end-67f40bd6-cdee-470a-9aa5-2c9d6f6c87bc to end-c1d6f9b4-3f45-4502-b4b8-27fd03491388
            migrationDataTest.ReconnectToPort(xmlNodeConnector, portIdReconnect);

            //After the port reconnection this the port setup
            //connector1 start="dcd9c6c6-6350-4292-a553-c57a764504b4" start_index="0" end="c1d6f9b4-3f45-4502-b4b8-27fd03491388"
            //connector2 start="c1d6f9b4-3f45-4502-b4b8-27fd03491388" start_index="0" end="dcd9c6c6-6350-4292-a553-c57a764504b4"

            //Assert 
            //The connector1 (xmlNodeConnector) should have now as end = c1d6f9b4-3f45-4502-b4b8-27fd03491388
            Assert.AreEqual(xmlNodeConnector.GetAttribute("end"), "c1d6f9b4-3f45-4502-b4b8-27fd03491388");
        }

        /// <summary>
        /// This test method will check the NodeMigrationData.CreateConnector method
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestCreateConnector()
        {
            //Arrange
            //In the file Angle.dyn we have only two connectors
            NodeMigrationData migrationDataTest = null;
            string documentDynPath = Path.Combine(TestDirectory, @"core\Angle.dyn");
            XmlDocument xmlDoc = new XmlDocument();

            //Act
            xmlDoc.Load(documentDynPath);
            //Find the node1 inside the xml file - guid="dcd9c6c6-6350-4292-a553-c57a764504b4"
            XmlElement node1 = (XmlElement)xmlDoc.SelectSingleNode("//Dynamo.Graph.Nodes.ZeroTouch.DSFunction");
            //Fin the node2 inside the xml file - guid="c1d6f9b4-3f45-4502-b4b8-27fd03491388"
            XmlElement node2 = (XmlElement)xmlDoc.SelectSingleNode("//CoreNodeModels.Input.DoubleInput");
            migrationDataTest = new NodeMigrationData(xmlDoc);
            //Creates a new connector from node1 to node2
            migrationDataTest.CreateConnector(node1,0,node2,0);

            //Assert
            //Verify that now we have three connectors (one is Dynamo.Models.ConnectorModel)
            Assert.AreEqual(xmlDoc.SelectNodes("//Connectors/*").Count, 3);

            //Verify that the new connector is found inside the xml document
            var connectorCreated = xmlDoc.SelectSingleNode("//Connectors/Dynamo.Models.ConnectorModel[@start='dcd9c6c6-6350-4292-a553-c57a764504b4'][@end='c1d6f9b4-3f45-4502-b4b8-27fd03491388']");
            Assert.IsNotNull(connectorCreated);
        }

        /// <summary>
        /// This test method will check the NodeMigrationData.CreateConnector method (an overloaded method)
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestCreateConnector2()
        {
            //Arrange
            //In the file Angle.dyn we have only two connectors
            NodeMigrationData migrationDataTest = null;
            string documentDynPath = Path.Combine(TestDirectory, @"core\Angle.dyn");
            XmlDocument xmlDoc = new XmlDocument();
            string newStartGuid = Guid.NewGuid().ToString();
            string endStartGuid = Guid.NewGuid().ToString();

            //Act
            xmlDoc.Load(documentDynPath);
            //Find the a connector inside the xml document
            XmlElement node1 = xmlDoc.CreateElement("Dynamo.Graph.Connectors.ConnectorModel");
          
            node1.SetAttribute("start", newStartGuid);
            node1.SetAttribute("end", endStartGuid);
            node1.SetAttribute("start_index", "0");
            node1.SetAttribute("end_index", "0");
            node1.SetAttribute("portType", "0");

            migrationDataTest = new NodeMigrationData(xmlDoc);
            //Creates a new connector 
            migrationDataTest.CreateConnector(node1);

            //Assert
            //Verify that now we have three Connectors
            Assert.AreEqual(xmlDoc.SelectNodes("//Connectors/*").Count, 3);

            //Verify that the new node created exists inside the xml document(start = newConnectorGuid)
            var connectorCreated = xmlDoc.SelectSingleNode("//Connectors/Dynamo.Graph.Connectors.ConnectorModel[@start='"+ newStartGuid + "']");
            Assert.IsNotNull(connectorCreated);
        }

        /// <summary>
        /// This test method will check the NodeMigrationData.CreateConnectorFromId method  
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestCreateConnectorFromId()
        {

            //Arrange
            //In the file Angle.dyn we have only two connectors
            NodeMigrationData migrationDataTest = null;
            string documentDynPath = Path.Combine(TestDirectory, @"core\Angle.dyn");
            XmlDocument xmlDoc = new XmlDocument();
            string startNodeId = Guid.NewGuid().ToString();
            string endNodeId = Guid.NewGuid().ToString();

            //Act
            xmlDoc.Load(documentDynPath);           

            migrationDataTest = new NodeMigrationData(xmlDoc);
            //Creates a connector from node1 to node2 (based in new generated id's)
            migrationDataTest.CreateConnectorFromId(startNodeId, 0, endNodeId, 0);

            //Assert
            //Verify that now we have three connectors (one is Dynamo.Models.ConnectorModel)
            Assert.AreEqual(xmlDoc.SelectNodes("//Connectors/*").Count, 3);

            //Verify that the node exists inside the Angle.dyn file
            var connectorCreated = xmlDoc.SelectSingleNode("//Connectors/Dynamo.Models.ConnectorModel[@start='"+ startNodeId + "'][@end='"+ endNodeId + "']");
            Assert.IsNotNull(connectorCreated);
        }
        #endregion

        #region WorkspaceMigrationAttribute
        /// <summary>
        /// This test method will create a new instance of the WorkspaceMigrationAttribute class
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestWorkspaceMigrationAttribute()
        {
            //Arrange
            Version inicial = new Version(225, 0, 0);
            Version final = new Version(226, 0, 0);

            //Act
            var migrationAttribute = new WorkspaceMigrationAttribute(inicial.ToString(), final.ToString());

            //Assert
            //Verify that the attributes were stored correctly in the class
            Assert.IsNotNull(migrationAttribute.From);
            Assert.IsNotNull(migrationAttribute.To);
        }
        #endregion

    }//class
}//namespace