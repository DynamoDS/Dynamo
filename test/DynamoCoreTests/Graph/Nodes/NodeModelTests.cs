using System.IO;
using Dynamo.Graph.Nodes;
using Dynamo.Selection;
using NUnit.Framework;
using Revit.Elements;

namespace Dynamo.Tests
{
    /// <summary>
    /// Class containing tests for the NodeModel class
    /// </summary>
    [TestFixture]
    class NodeModelTests : DynamoModelTestBase
    {
        NodeModel testNodeModel;

        [SetUp]
        public void Init()
        {
            CurrentDynamoModel.NodeFactory.CreateNodeFromTypeName("CoreNodeModels.Input.DoubleInput", out testNodeModel);
        }

        [Test]
        [Category("UnitTests")]
        public void CreationNameTest()
        {
            testNodeModel.Category = "Category";

            var result = testNodeModel.ConstructDictionaryLinkFromLibrary(CurrentDynamoModel.LibraryServices);
            Assert.AreEqual("http://dictionary.dynamobim.com/2/#/Category/Action/Number", result);
        }
        
        [Test]
        [Category("UnitTests")]
        public void CategoryTest()
        {
            testNodeModel.Category = null; //Set Category to null so that GetCategoryStringFromAttributes() gets called in the property getter
            var category = testNodeModel.Category;

            Assert.AreEqual("Core.Input", category);
        }

        [Test]
        [Category("UnitTests")]
        public void DictionaryLinkTest()
        {
            testNodeModel.DictionaryLink = null; //Set DictionaryLink to null to cover the setter and to test the Configuration property
            var dictLink = testNodeModel.DictionaryLink;

            Assert.AreEqual("http://dictionary.dynamobim.com/2/", dictLink);
        }

        [Test]
        [Category("UnitTests")]
        public void SelectNeighborsTest()
        {
            string listTestFolder  = Path.Combine(TestDirectory, "core");
            string testFilePath = Path.Combine(listTestFolder, "Angle.dyn");
            
            RunModel(testFilePath);

            NodeModel node = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("dcd9c6c6-6350-4292-a553-c57a764504b4");
            
            var countBefore = DynamoSelection.Instance.Selection.Count;
            Assert.AreEqual(0, countBefore);

            node.SelectNeighbors();

            var countAfter = DynamoSelection.Instance.Selection.Count;
            Assert.AreEqual(2, countAfter);
        }
    }
}