using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamo.Graph.Workspaces;
using NUnit.Framework;

namespace Dynamo.Tests
{
    public class SearchSideEffects : DynamoViewModelUnitTest
    {
        [Test]
        [Category("UnitTests")]
        public void WhenStartingDynamoInputAndOutputNodesAreNolongerMissingFromSearch()
        {
            Assert.IsAssignableFrom( typeof(HomeWorkspaceModel), ViewModel.Model.CurrentWorkspace );

            // search and results are correct
            ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.SearchAndUpdateResults("Input");
            Assert.AreEqual(1, ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.FilteredResults.Count(x => x.Model.Name == "Input"));

            ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.SearchAndUpdateResults("Output");
            Assert.AreEqual(1, ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.FilteredResults.Count(x => x.Model.Name == "Output"));
        }


        /// <summary>
        /// This test will validate that the nodes "Input", "Output", "And", "Or", "Not", "+", "-"  appear in the InCanvasSearch results
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void WhenStartingDynamoOperatorNodesNolongerMissingFromSearch()
        {
            Assert.IsAssignableFrom(typeof(HomeWorkspaceModel), ViewModel.Model.CurrentWorkspace);

            List<string> nodesList = new List<string>() { "Input", "Output", "And", "Or", "Not", "+", "-" };

            foreach(var node in nodesList)
            {
                // search and check that the results are correct based in the node name provided for the searchTerm
                ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.SearchAndUpdateResults(node);
                var filteredResults = ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.FilteredResults;
                Assert.AreEqual(1, ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.FilteredResults.Count(x => x.Model.Name == node));
            }    
        }

        [Test]
        public void WhenHomeWorkspaceIsFocusedInputAndOutputNodesAreMissingFromSearch()
        {
            // goto custom node workspace
            var model = ViewModel.Model;
            var examplePath = Path.Combine(TestDirectory, @"core\combine", "Sequence2.dyf");
            ViewModel.OpenCommand.Execute(examplePath);

            Assert.AreEqual(model.CurrentWorkspace.Name, "Sequence2");

            // go to homeworkspace
            ViewModel.Model.CurrentWorkspace =
                ViewModel.Model.Workspaces.OfType<HomeWorkspaceModel>().First();

            Assert.AreEqual(model.CurrentWorkspace.Name, "Home");

            // search and results are correct
            ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.SearchAndUpdateResults("Input");
            Assert.AreEqual(1, ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.FilteredResults.Count(x => x.Model.Name == "Input"));

            ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.SearchAndUpdateResults("Output");
            Assert.AreEqual(1, ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.FilteredResults.Count(x => x.Model.Name == "Output"));
        }

        [Test]
        [Category("Failure")]
        public void WhenCustomNodeWorkspaceIsFocusedInputAndOutputNodesArePresentInSearch()
        {
            // goto custom node workspace
            var model = ViewModel.Model;
            var examplePath = Path.Combine(TestDirectory, @"core\combine", "Sequence2.dyf");
            ViewModel.OpenCommand.Execute(examplePath);

            Assert.AreEqual(model.CurrentWorkspace.Name, "Sequence2");

            // search and results are correct
            ViewModel.SearchViewModel.SearchAndUpdateResults("Input");
            Assert.AreEqual(1, ViewModel.SearchViewModel.FilteredResults.Count(x => x.Model.Name == "Input"));
            Assert.AreEqual("Input", ViewModel.SearchViewModel.FilteredResults.ElementAt(0).Model.Name);

            ViewModel.SearchViewModel.SearchAndUpdateResults("Output");
            Assert.AreEqual(1, ViewModel.SearchViewModel.FilteredResults.Count(x => x.Model.Name == "Output"));
            Assert.AreEqual("Output", ViewModel.SearchViewModel.FilteredResults.ElementAt(0).Model.Name);

        }
    }
}
