using System.IO;
using System.Linq;
using Dynamo.Models;
using NUnit.Framework;

namespace Dynamo.Tests
{
    public class CustomNodeWorkspaceOpening : DynamoUnitTest
    {

        [Test]
        public void CanOpenCustomNodeWorkspace()
        {
            Assert.Inconclusive("Porting : Formula");

            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\combine", "Sequence2.dyf");
            model.Open(examplePath);

            var nodeWorkspace = model.Workspaces.FirstOrDefault(x => x is CustomNodeWorkspaceModel);
            Assert.IsNotNull(nodeWorkspace);
            Assert.AreEqual( model.CurrentWorkspace.Name, "Sequence2");
        }

        [Test]
        public void CustomNodeWorkspaceIsAddedToSearchOnOpening()
        {
            Assert.Inconclusive("Porting : Formula");

            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\combine", "Sequence2.dyf");
            model.Open(examplePath);
            
            Controller.SearchViewModel.SearchAndUpdateResultsSync("Sequence2");
            Assert.AreEqual(1, Controller.SearchViewModel.SearchResults.Count);
            Assert.AreEqual("Sequence2", Controller.SearchViewModel.SearchResults[0].Name);
        }
    }

}
