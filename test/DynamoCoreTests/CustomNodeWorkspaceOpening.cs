using System;
using System.IO;
using System.Linq;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.ViewModels;

using NUnit.Framework;

namespace Dynamo.Tests
{
    public class CustomNodeWorkspaceOpening : DynamoUnitTest
    {
        [Test]
        public void CanOpenCustomNodeWorkspace()
        {
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\combine", "Sequence2.dyf");
            Controller.DynamoViewModel.OpenCommand.Execute(examplePath);

            var nodeWorkspace = model.Workspaces.FirstOrDefault(x => x is CustomNodeWorkspaceModel);
            Assert.IsNotNull(nodeWorkspace);
            Assert.AreEqual( model.CurrentWorkspace.Name, "Sequence2");
        }

        [Test]
        public void CustomNodeWorkspaceIsAddedToSearchOnOpening()
        {
            var examplePath = Path.Combine(GetTestDirectory(), @"core\combine", "Sequence2.dyf");
            Controller.DynamoViewModel.OpenCommand.Execute(examplePath);
            
            Controller.SearchViewModel.SearchAndUpdateResultsSync("Sequence2");
            Assert.AreEqual(1, Controller.SearchViewModel.SearchResults.Count);
            Assert.AreEqual("Sequence2", Controller.SearchViewModel.SearchResults[0].Name);
        }
    }

}
