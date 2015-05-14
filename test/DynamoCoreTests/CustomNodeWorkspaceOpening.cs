using System.IO;
using System.Linq;

using Dynamo.Models;
using Dynamo.Nodes;

using NUnit.Framework;

namespace Dynamo.Tests
{
    internal class CustomNodeWorkspaceOpening : DynamoModelTestBase
    {
        public void OpenTestFile(string folder, string fileName)
        {
            var examplePath = Path.Combine(TestDirectory, folder, fileName);
            OpenModel(examplePath);
        }

        [Test]
        public void CanOpenWorkspaceWithMissingCustomNodeThenFixByOpeningNeededCustomNodeWorkspace()
        {
            // a file with a missing custom node definition is opened
            OpenTestFile(@"core\CustomNodes", "noro.dyn");

            var homeWorkspace = CurrentDynamoModel.CurrentWorkspace as HomeWorkspaceModel;
            Assert.NotNull(homeWorkspace);

            var funcNode = homeWorkspace.Nodes.OfType<Function>().First();
            Assert.IsTrue( funcNode.Definition.IsProxy );

            // the required custom node is opened
            OpenTestFile(@"core\CustomNodes\files", "ro.dyf");
            Assert.IsFalse( funcNode.Definition.IsProxy );
            
            homeWorkspace.Run();

            CurrentDynamoModel.CurrentWorkspace = homeWorkspace;

            Assert.AreEqual(12.0, GetPreviewValue(funcNode.GUID));
        }

        [Test]
        public void CanOpenCustomNodeWorkspace()
        {
            OpenTestFile(@"core\combine", "Sequence2.dyf");

            var nodeWorkspace = CurrentDynamoModel.Workspaces.FirstOrDefault(x => x is CustomNodeWorkspaceModel);
            Assert.IsNotNull(nodeWorkspace);
            Assert.AreEqual(CurrentDynamoModel.CurrentWorkspace.Name, "Sequence2");
        }

        [Test]
        public void CustomNodeWorkspaceIsAddedToSearchOnOpening()
        {
            OpenTestFile(@"core\combine", "Sequence2.dyf");
            
            var res = CurrentDynamoModel.SearchModel.Search("Sequence2");
            Assert.AreEqual(1, res.Count());
            Assert.AreEqual("Sequence2", res.First().Name);
        }
    }

}
