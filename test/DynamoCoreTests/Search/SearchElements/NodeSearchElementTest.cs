using System;
using System.Collections.Generic;
using System.IO;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Search.SearchElements;
using Moq;
using NUnit.Framework;

namespace Dynamo.Tests.Search.SearchElements
{
    /// <summary>
    /// This test class was created with the purpose of calling the next methods in the base class(NodeSearchElement)
    /// IEnumerable<Tuple<string, string>> GenerateInputParameters()
    /// IEnumerable<string> GenerateOutputParameters()
    /// </summary>
    public class CustomNodeSearchElementTest2 : NodeSearchElement
    {
        private readonly ICustomNodeSource customNodeManager;
        private string path;

        public Guid ID { get; private set; }

        public override string CreationName { get { return this.ID.ToString(); } }

        public string Path
        {
            get { return path; }
            private set
            {
                if (value == path) return;
                path = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomNodeSearchElement"/> class.
        /// </summary>
        /// <param name="customNodeManager">Custom node manager</param>
        /// <param name="info">Custom node info</param>
        public CustomNodeSearchElementTest2(ICustomNodeSource customNodeManager, CustomNodeInfo info)
        {
            this.customNodeManager = customNodeManager;
            inputParameters = new List<Tuple<string, string>>();
            outputParameters = new List<string>();
            SyncWithCustomNodeInfo(info);
        }

        /// <summary>
        ///     Updates the properties of this search element.
        /// </summary>
        /// <param name="info">Actual data of custom node</param>        
        public void SyncWithCustomNodeInfo(CustomNodeInfo info)
        {
            ID = info.FunctionId;
            Name = info.Name;
            FullCategoryName = info.Category;
            Description = info.Description;
            Path = info.Path;
            iconName = ID.ToString();

            ElementType = ElementTypes.CustomNode;
            if (info.IsPackageMember)
                ElementType |= ElementTypes.Packaged; // Add one more flag.
        }

        protected override NodeModel ConstructNewNodeModel()
        {
            return customNodeManager.CreateCustomNodeInstance(ID);
        }

        protected override IEnumerable<Tuple<string, string>> GenerateInputParameters()
        {
            return base.GenerateInputParameters();
        }

        protected override IEnumerable<string> GenerateOutputParameters()
        {
            return base.GenerateOutputParameters();
        }
    }
    [TestFixture]
    class NodeSearchElementTest : DynamoModelTestBase
    {
        /// <summary>
        /// This test method will execute the next methods/properties in NodeSearchElement class
        /// protected virtual void OnItemProduced(NodeModel obj)
        /// public IEnumerable<Tuple<string, string>> InputParameters
        /// public IEnumerable<string> OutputParameters
        /// protected virtual IEnumerable<string> GenerateOutputParameters()
        /// protected virtual IEnumerable<Tuple<string, string>> GenerateInputParameters()
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestNodeSearchElement()
        {
            //Arrange
            string path = Path.Combine(TestDirectory, @"core\CustomNodes\CNDefault.dyf");
            const string nodeName = "TheNoodle";
            const string catName = "TheCat";
            const string descr = "TheCat";

            var guid1 = Guid.NewGuid();
            var dummyInfo1 = new CustomNodeInfo(guid1, nodeName, catName, descr, path);
            var moq = new Mock<ICustomNodeSource>();
            var dummySearch1 = new CustomNodeSearchElementTest2(moq.Object, dummyInfo1);

            //Act
            //Execute the GenerateInputParameters() method
            List<Tuple<string, string>> inputParameters = dummySearch1.InputParameters as List<Tuple<string, string>>;

            //Execute the GenerateOutputParameters() method
            List<string> outputParameters = dummySearch1.OutputParameters as List<string>;

            dummySearch1.ProduceNode();

            //Assert
            //It just validates that the parameters is not null and has at least one element in the list
            Assert.IsNotNull(inputParameters);
            Assert.Greater(inputParameters.Count, 0);
            Assert.IsNotNull(outputParameters);
            Assert.Greater(outputParameters.Count, 0);

            //This will execute the Get of the IsVisibleInSearch property
            Assert.IsTrue(dummySearch1.IsVisibleInSearch);
        }

        /// <summary>
        /// This test method was created with the purpose of executing the next methods/properties
        /// public NodeSearchElement SearchElement
        /// public DragDropNodeSearchElementInfo(NodeSearchElement searchElement)
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestDragDropNodeSearchElement()
        {
            //Arrange
            string path = Path.Combine(TestDirectory, @"core\CustomNodes\CNDefault.dyf");
            const string nodeName = "TheNoodle";
            const string catName = "TheCat";
            const string descr = "TheCat";

            var guid1 = Guid.NewGuid();
            var dummyInfo1 = new CustomNodeInfo(guid1, nodeName, catName, descr, path);
            var moq = new Mock<ICustomNodeSource>();
            var dummySearch1 = new CustomNodeSearchElementTest2(moq.Object, dummyInfo1);

            //Act
            //This will execute the DragDropNodeSearchElementInfo constructor
            var dragAndDropElement = new DragDropNodeSearchElementInfo(dummySearch1);

            //Assert
            //This will execute the Get method of the SearchElement property
            Assert.IsNotNull(dragAndDropElement.SearchElement);
        }
    }
}
