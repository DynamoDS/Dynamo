using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Search.SearchElements;
using NUnit.Framework;

namespace Dynamo.Tests.Search.SearchElements
{
    /// <summary>
    /// This test class was created with the purpose of executing the protected method
    /// protected override NodeModel ConstructNewNodeModel()
    /// </summary>
    public class NodeModelSearchElementDerived : NodeModelSearchElement
    {
        internal NodeModelSearchElementDerived(TypeLoadData typeLoadData) : base(typeLoadData)
        {

        }

        protected override NodeModel ConstructNewNodeModel()
        {
            return base.ConstructNewNodeModel();//Calls the ConstructNewNodeModel() method in the base class(NodeModelSearchElement)
        }

        //Due that ConstructNewNodeModel() method is protected we need to create a public method that call it.
        public NodeModel NewModel()
        {
           return ConstructNewNodeModel();
        }
    }

    [TestFixture]
    class NodeModelSearchElementTest : DynamoModelTestBase
    {
        /// <summary>
        /// This test method will execute the NodeModel ConstructNewNodeModel() method in NodeModelSearchElement
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestNodeModelSearchElement()
        {
            //Arrange
            var symbolData = new TypeLoadData(typeof(Symbol));

            //Act
            var symbolSearchElement = new NodeModelSearchElementDerived(symbolData)
            {
                IsVisibleInSearch = CurrentDynamoModel.CurrentWorkspace is CustomNodeWorkspaceModel
            };

            //Internally this will execute the ConstructNewNodeModel() through the NodeModelSearchElementDerived class
            var model = symbolSearchElement.NewModel();

            //Assert
            //We just check that the model was successfully created
            Assert.IsNotNull(model);
        }
    }
}
