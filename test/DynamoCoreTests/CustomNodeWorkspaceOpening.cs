using System;
using System.IO;
using System.Linq;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.ViewModels;

using NUnit.Framework;

namespace Dynamo.Tests
{
    public class CustomNodeWorkspaceOpening : DynamoViewModelUnitTest
    {
        [Test]
        public void CanOpenCustomNodeWorkspace()
        {
            var model = ViewModel.Model;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\combine", "Sequence2.dyf");
            ViewModel.OpenCommand.Execute(examplePath);

            var nodeWorkspace = model.Workspaces.FirstOrDefault(x => x is CustomNodeWorkspaceModel);
            Assert.IsNotNull(nodeWorkspace);
            Assert.AreEqual( model.CurrentWorkspace.Name, "Sequence2");
        }

        [Test]
        [Category("Failure")]
        public void CustomNodeWorkspaceIsAddedToSearchOnOpening()
        {
            var examplePath = Path.Combine(GetTestDirectory(), @"core\combine", "Sequence2.dyf");
            ViewModel.OpenCommand.Execute(examplePath);
            
            ViewModel.SearchViewModel.SearchAndUpdateResults("Sequence2");
            Assert.AreEqual(1, ViewModel.SearchViewModel.SearchResults.Count);
            Assert.AreEqual("Sequence2", ViewModel.SearchViewModel.SearchResults[0].Model.Name);
        }
    }

}
