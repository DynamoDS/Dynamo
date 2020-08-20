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

namespace CoreNodeModelsTests
{
    [TestFixture]
    public class SelectionTests : DynamoModelTestBase
    {
        private SelectionConcrete selection;
        private NodeModel testNode;
        private Mock<IModelSelectionHelper<ModelBase>> selectionHelperMock;

        [SetUp]
        public void TestsSetup()
        {
            selectionHelperMock = new Mock<IModelSelectionHelper<ModelBase>>(MockBehavior.Strict);
            selection = new SelectionConcrete(SelectionType.Many, SelectionObjectType.Element, "testMessage", "testPrefix", selectionHelperMock.Object);
            testNode = CreateCodeBlockNode();
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
            var cant = selection.SelectionResults.Count();
            Assert.AreEqual(1, cant);

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
            Assert.AreEqual("testPrefix : Dynamo.Graph.Nodes.CodeBlockNodeModel", toStringResult);

            selectionHelperMock.VerifyAll();
        }

        [TestCase(SelectionObjectType.Edge, SelectionType.One, "Curve")]
        [TestCase(SelectionObjectType.Edge, SelectionType.Many, "Curves")]
        [TestCase(SelectionObjectType.Face, SelectionType.One, "Surface")]
        [TestCase(SelectionObjectType.Face, SelectionType.Many, "Surfaces")]
        [TestCase(SelectionObjectType.PointOnFace, SelectionType.One, "Point")]
        [TestCase(SelectionObjectType.PointOnFace, SelectionType.Many, "Points")]
        [TestCase(SelectionObjectType.None, SelectionType.One, "Element")]
        [TestCase(SelectionObjectType.None, SelectionType.Many, "Elements")]
        public void GetSelectionOutputTest(SelectionObjectType selectionObjectType, SelectionType selectinType, string expectedResult)
        {
            selection = new SelectionConcrete(selectinType, selectionObjectType, "testMessage", "testPrefix");

            var result = selection.GetOutputPortName();

            Assert.AreEqual(expectedResult, result);
        }

        /// <summary>
        /// Class created in order to test protected methods in SelectionBase parent
        /// </summary>
        private class SelectionConcrete : SelectionBase<ModelBase, ModelBase>
        {
            public override IModelSelectionHelper<ModelBase> SelectionHelper { get; }

            public void SerializeCore(XmlElement nodeElement, SaveContext context) =>
                base.SerializeCore(nodeElement, context);

            public void DeserializeCore(XmlElement nodeElement, SaveContext context) =>
                base.DeserializeCore(nodeElement, context);

            public bool UpdateValueCore(UpdateValueParams updateValueParams) =>
                base.UpdateValueCore(updateValueParams);

            public string GetOutputPortName() =>
                 base.GetOutputPortName();

            //Implemented this way for testing so the selection suffers no modifications
            protected override IEnumerable<ModelBase> ExtractSelectionResults(ModelBase selections)
            {
                return new List<ModelBase> { selections };
            }

            protected override string GetIdentifierFromModelObject(ModelBase modelObject)
            {
                throw new NotImplementedException();
            }

            protected override ModelBase GetModelObjectFromIdentifer(string id)
            {
                throw new NotImplementedException();
            }

            //This constructor is only used during testing to allow the initalization of the SelectionHelper property
            public SelectionConcrete(
                SelectionType selectionType,
                SelectionObjectType selectionObjectType,
                string message,
                string prefix,
                IModelSelectionHelper<ModelBase> selectionHelper)
                : base(
                    selectionType,
                    selectionObjectType,
                    message,
                    prefix)
            {
                SelectionHelper = selectionHelper;
            }

            public SelectionConcrete(
                SelectionType selectionType,
                SelectionObjectType selectionObjectType,
                string message,
                string prefix)
                    : base(
                        selectionType,
                        selectionObjectType,
                        message,
                        prefix)
            {
            }

            public SelectionConcrete(
                SelectionType selectionType,
                SelectionObjectType selectionObjectType,
                string message,
                string prefix,
                IEnumerable<string> selectionIdentifier,
                IEnumerable<PortModel> inPorts,
                IEnumerable<PortModel> outPorts)
                    : base(
                        selectionType,
                        selectionObjectType,
                        message,
                        prefix,
                        selectionIdentifier,
                        inPorts,
                        outPorts)
            {
            }
        }

        //Creates a NodeModel to use for testing
        private CodeBlockNodeModel CreateCodeBlockNode()
        {
            var cbn = new CodeBlockNodeModel(CurrentDynamoModel.LibraryServices);
            var command = new DynamoModel.CreateNodeCommand(cbn, 0, 0, true, false);

            CurrentDynamoModel.ExecuteCommand(command);

            Assert.IsNotNull(cbn);
            return cbn;
        }
    }
}
