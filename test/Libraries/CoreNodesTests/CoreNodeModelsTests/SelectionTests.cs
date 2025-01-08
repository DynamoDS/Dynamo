using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CoreNodeModels;
using CoreNodeModels.Properties;
using Dynamo;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using NUnit.Framework;
using Moq;
using TestUINodes;

namespace DSCoreNodesTests
{
    [TestFixture]
    public class SelectionTests : DynamoModelTestBase
    {
        private SelectionConcrete selection;
        private NodeModel testNode;
        private Mock<IModelSelectionHelper<ModelBase>> selectionHelperMock;
        
        //Creates a NodeModel to use for testing
        private CodeBlockNodeModel CreateCodeBlockNode()
        {
            var cbn = new CodeBlockNodeModel(CurrentDynamoModel.LibraryServices);
            var command = new DynamoModel.CreateNodeCommand(cbn, 0, 0, true, false);

            CurrentDynamoModel.ExecuteCommand(command);

            Assert.IsNotNull(cbn);
            return cbn;
        }

        [SetUp]
        public void TestsSetup()
        {
            selectionHelperMock = new Mock<IModelSelectionHelper<ModelBase>>(MockBehavior.Strict);
            selection = new SelectionConcrete(SelectionType.Many, SelectionObjectType.Element, "testMessage", "testPrefix", selectionHelperMock.Object);
            selection.Name = "selectionTestName";
            testNode = CreateCodeBlockNode();
        }

        [Test]
        public void ConstructorWithPortsTest()
        {
            //Ports for testing
            PortModel inPort = new PortModel(PortType.Input, testNode,new PortData("input", "input port"));
            PortModel outPort = new PortModel(PortType.Output, testNode,new PortData("output", "output port"));

            IEnumerable<PortModel> inPorts = new List<PortModel>{inPort};
            IEnumerable<PortModel> outPorts = new List<PortModel>{outPort};

            //Creates the new selection
            selection = new SelectionConcrete(
                SelectionType.Many,
                SelectionObjectType.Element,
                "testMessage",
                "testPrefix",
                new List<string>{"selection Identifier"},
                inPorts,
                outPorts);

            Assert.AreEqual(inPort.GUID, selection.InPorts.FirstOrDefault().GUID);
            Assert.AreEqual(outPort.GUID, selection.OutPorts.FirstOrDefault().GUID);
        }

        [Test]
        public void SelectionIdentifierTest()
        {
            //Selects the test node
            selectionHelperMock
                .Setup(x => x.RequestSelectionOfType("testMessage", SelectionType.Many, SelectionObjectType.Element))
                .Returns(new List<ModelBase> { testNode });
            selection.Select(testNode);

            var expectedResult = new List<string> {testNode.GUID.ToString()};
            var result = selection.SelectionIdentifier;

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void SelectTest()
        {
            //Checks there is no elements selected
            var startingAmount = selection.SelectionResults.Count();
            Assert.AreEqual(0, startingAmount);

            //Selects the test node
            selectionHelperMock
                .Setup(x => x.RequestSelectionOfType("testMessage", SelectionType.Many, SelectionObjectType.Element))
                .Returns(new List<ModelBase> { testNode });
            selection.Select(testNode);

            //checks one node is selected
            var result = selection.SelectionResults.Count();
            Assert.AreEqual(1, result);

            selectionHelperMock.VerifyAll();
        }

        [Test]
        public void ClearSelectionTest()
        {
            //Selects the test node
            selectionHelperMock
                .Setup(x => x.RequestSelectionOfType("testMessage", SelectionType.Many, SelectionObjectType.Element))
                .Returns(new List<ModelBase> { testNode });
            selection.Select(testNode);

            //checks one node is selected
            var count = selection.SelectionResults.Count();
            Assert.AreEqual(1, count);

            selection.ClearSelections();
            //Checks no nodes are selected
            var result = selection.SelectionResults.Count();
            Assert.AreEqual(0, result);

            selectionHelperMock.VerifyAll();
        }

        [Test]
        public void ToStringTest()
        {
            //Checks ToString result for an empty selection
            var toStringResult = selection.ToString();
            Assert.AreEqual(Resources.SelectionNodeNothingSelected, toStringResult);

            //Selects the test node
            selectionHelperMock
                .Setup(x => x.RequestSelectionOfType("testMessage", SelectionType.Many, SelectionObjectType.Element))
                .Returns(new List<ModelBase> { testNode });
            selection.Select(testNode);

            //Checks ToString result for the selected element
            toStringResult = selection.ToString();
            string nodeType = testNode.GetType().ToString();
            string expected = $"testPrefix: {nodeType}";
            Assert.AreEqual(expected, toStringResult);

            selectionHelperMock.VerifyAll();
        }

        [Test]
        public void GetSelectionOutputTest()
        {
            //Case ObjectType: Edge
            selection = new SelectionConcrete(SelectionType.One, SelectionObjectType.Edge, "testMessage", "testPrefix");
            var expectedResult = "Curve";
            var result = selection.GetOutputPortName();
            Assert.AreEqual(expectedResult, result);

            selection = new SelectionConcrete(SelectionType.Many, SelectionObjectType.Edge, "testMessage", "testPrefix");
            expectedResult = "Curves"; 
            result = selection.GetOutputPortName();
            Assert.AreEqual(expectedResult, result);

            //Case ObjectType: Face
            selection = new SelectionConcrete(SelectionType.One, SelectionObjectType.Face, "testMessage", "testPrefix");
            expectedResult = "Surface";
            result = selection.GetOutputPortName();
            Assert.AreEqual(expectedResult, result);

            selection = new SelectionConcrete(SelectionType.Many, SelectionObjectType.Face, "testMessage", "testPrefix");
            expectedResult = "Surfaces";
            result = selection.GetOutputPortName();
            Assert.AreEqual(expectedResult, result);

            //Case ObjectType: PointOnFace
            selection = new SelectionConcrete(SelectionType.One, SelectionObjectType.PointOnFace, "testMessage", "testPrefix");
            expectedResult = "Point";
            result = selection.GetOutputPortName();
            Assert.AreEqual(expectedResult, result);

            selection = new SelectionConcrete(SelectionType.Many, SelectionObjectType.PointOnFace, "testMessage", "testPrefix");
            expectedResult = "Points";
            result = selection.GetOutputPortName();
            Assert.AreEqual(expectedResult, result);

            //Case ObjectType: None
            selection = new SelectionConcrete(SelectionType.One, SelectionObjectType.None, "testMessage", "testPrefix");
            expectedResult = "Element";
            result = selection.GetOutputPortName();
            Assert.AreEqual(expectedResult, result);

            selection = new SelectionConcrete(SelectionType.Many, SelectionObjectType.None, "testMessage", "testPrefix");
            expectedResult = "Elements";
            result = selection.GetOutputPortName();
            Assert.AreEqual(expectedResult, result);

        }

        [Test]
        public void SerializeCoreTest()
        {
            //Selects the test node
            selectionHelperMock
                .Setup(x => x.RequestSelectionOfType("testMessage", SelectionType.Many, SelectionObjectType.Element))
                .Returns(new List<ModelBase> { testNode });
            selection.Select(testNode);

            //Serializes selection into xml
            var testDocument = new XmlDocument();
            testDocument.LoadXml("<ElementTag/>");
            var testElement = testDocument.DocumentElement;
            selection.SerializeCore(testElement, SaveContext.None);

            string expectedInnerXML = String.Format("<instance id=\"{0}\" />",
                testNode.GUID.ToString());
            Assert.AreEqual(expectedInnerXML, testElement.InnerXml);
        }

        [Test]
        public void DeserializeCoreTest()
        {
            selection.testNode = testNode;

            //Selects the test node
            selectionHelperMock
                .Setup(x => x.RequestSelectionOfType("testMessage", SelectionType.Many, SelectionObjectType.Element))
                .Returns(new List<ModelBase> { testNode });
            selection.Select(testNode);
            var count = selection.SelectionResults.Count();
            Assert.AreEqual(1, count);


            //Serializes selection into xml
            var testDocument = new XmlDocument();
            testDocument.LoadXml("<ElementTag/>");
            var testElement = testDocument.DocumentElement;
            selection.SerializeCore(testElement, SaveContext.None);

            //Clears selection
            selection.ClearSelections();
            count = selection.SelectionResults.Count();
            Assert.AreEqual(0, count);

            //Recovers selection from xml
            selection.DeserializeCore(testElement, SaveContext.None);
            count = selection.SelectionResults.Count();
            Assert.AreEqual(1, count);
            var selectedNode = selection.SelectionResults.FirstOrDefault();
            Assert.AreEqual(testNode.GUID, selectedNode.GUID);
        }

        [Test]
        public void UpdateValueCoreTest()
        {
            //Sets property to be updated
            selection.Name = "Starting Name";

            //Updates Name property with new value
            var updatedValue = new UpdateValueParams("Name", "UpdatedName");
            bool response = selection.UpdateValueCore(updatedValue);
            Assert.IsTrue(response);
            Assert.AreEqual(updatedValue.PropertyValue, selection.Name);

            //Case: Update "Value" property
            //Load node for testing
            testNode.GUID = Guid.NewGuid();
            selection.testNode = testNode;

            //Updates selection value
            updatedValue = new UpdateValueParams("Value", testNode.GUID.ToString());
            response = selection.UpdateValueCore(updatedValue);

            var count = selection.SelectionResults.Count();
            Assert.AreEqual(1, count);
            var selectedNode = selection.SelectionResults.FirstOrDefault();
            Assert.AreEqual(testNode.GUID, selectedNode.GUID);
        }
    }
}
