using Dynamo.Graph.Nodes;
using Dynamo.Search.SearchElements;
using NUnit.Framework;

namespace Dynamo.Tests.Search.SearchElements
{
    
    [TestFixture]
    class CodeBlockNodeSearchElementTest : DynamoModelTestBase
    {
        /// <summary>
        /// This test method will execute the next method in CodeBlockNodeSearchElement 
        /// protected override NodeModel ConstructNewNodeModel()
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestCodeBlockNodeSearchNewNodeModel()
        {
            //Arrange
            var cbnData = new TypeLoadData(typeof(CodeBlockNodeModel));
            var codeBlockSearch = new CodeBlockNodeSearchElement(cbnData, CurrentDynamoModel.LibraryServices);
            int initialCount = CurrentDynamoModel.SearchModel.NumElements;

            CurrentDynamoModel.SearchModel.Add(codeBlockSearch);

            //Act
            //This will call the method CodeBlockNodeSearchElement.ConstructNewNodeModel()
            codeBlockSearch.ProduceNode();

            //Assert
            //Just validate that the new CodeBlockNodeSearchElement was added correctly
            Assert.AreEqual(initialCount + 1, CurrentDynamoModel.SearchModel.NumElements);
        }

    }
}
