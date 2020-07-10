using System.IO;
using System.Linq;
using Dynamo.Graph.Workspaces;
using NUnit.Framework;

namespace Dynamo.Tests
{
    /// <summary>
    /// Class containing tests for the CustomNodeWorkspaceModel class
    /// </summary>
    [TestFixture]
    class CustomNodeWorkspaceModelTests : DynamoModelTestBase
    {
        public void OpenTestFile(string folder, string fileName)
        {
            var examplePath = Path.Combine(TestDirectory, folder, fileName);
            OpenModel(examplePath);
        }

        [Test]
        [Category("UnitTests")]
        public void OnPropertyNameChangedTest()
        {
            OpenTestFile(@"core\CustomNodes", "add_Read_only.dyf");
            var nodeWorkspace = CurrentDynamoModel.Workspaces.FirstOrDefault(x => x is CustomNodeWorkspaceModel);
            Assert.IsNotNull(nodeWorkspace);

            OpenTestFile(@"core\CustomNodes", "TestAdd.dyn");
            var homeWorkspace = CurrentDynamoModel.CurrentWorkspace as HomeWorkspaceModel;
            Assert.NotNull(homeWorkspace);

            //Changes the Name property so that the event is raised
            nodeWorkspace.Name = "NewNodeName";

            var resultNode = homeWorkspace.Nodes.FirstOrDefault(x => x.Name == "NewNodeName");
            Assert.IsNotNull(resultNode);
        }

        [Test]
        [Category("UnitTests")]
        public void GetSharedNameTest()
        {
            var fileName = "add_Read_only";
            OpenTestFile(@"core\CustomNodes", fileName+".dyf");
            var nodeWorkspace = CurrentDynamoModel.Workspaces.FirstOrDefault(x => x is CustomNodeWorkspaceModel);
            Assert.IsNotNull(nodeWorkspace);

            var result = nodeWorkspace.GetSharedName();
            Assert.AreEqual(fileName,result);

            //Set fileName to null to simulate it is not a saved file
            nodeWorkspace.FileName = null;
            var expected = nodeWorkspace.Name;

            result = nodeWorkspace.GetSharedName();

            Assert.AreEqual(expected,result);
        }
    }
}
