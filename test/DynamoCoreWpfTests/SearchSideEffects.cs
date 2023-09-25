using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamo.Graph.Workspaces;
using NUnit.Framework;

namespace Dynamo.Tests
{
    public class SearchSideEffects : DynamoViewModelUnitTest
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("DSCoreNodes.dll");
        }

        [SetUp]
        public void Setup()
        {
            ViewModel.Model.AddZeroTouchNodesToSearch(ViewModel.Model.LibraryServices.GetAllFunctionGroups());
        }


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

        /// <summary>
        /// This test will validate that the nodes "Input", "Output", "And", "Or", "Not", "+", "-"  appear in the InCanvasSearch results
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void WhenStartingDynamoOperatorNodesNolongerMissingFromSearch()
        {
            Assert.IsAssignableFrom(typeof(HomeWorkspaceModel), ViewModel.Model.CurrentWorkspace);

            List<string> nodesList = new List<string>() { "Input", "Output", "And", "Or", "Not", "+", "-" };

            foreach (var node in nodesList)
            {
                // search and check that the results are correct based in the node name provided for the searchTerm
                ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.SearchAndUpdateResults(node);
                var filteredResults = ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.FilteredResults;
                Assert.AreEqual(1, filteredResults.Count(x => x.Model.Name == node), "Non matching results for " + node);
            }
        }

        //This test will validate that all the nodes read by Dynamo in test mode can be found using LuceneSearch
        [Test]
        [Category("UnitTests")]
        public void LuceneSearchAllNodesValidation()
        {
            Assert.IsAssignableFrom(typeof(HomeWorkspaceModel), ViewModel.Model.CurrentWorkspace);
            List<string> nodesList = ViewModel.Model.SearchModel.Entries.Select(entry => entry.Name).ToList();

            foreach (var node in nodesList)
            {
                // Search and check that the results are correct based in the node name provided for the searchTerm
                ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.SearchAndUpdateResults(node);
                var filteredResults = ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.FilteredResults;

                //There are overloaded nodes
                Assert.GreaterOrEqual(filteredResults.Count(x => x.Model.Name == node), 1);
            }
        }

        //This test will validate that when the search term is a category it will found the nodes under that specific category
        [Test]
        [Category("UnitTests")]
        public void LuceneSearchNodesByCategoryValidation()
        {
            Assert.IsAssignableFrom(typeof(HomeWorkspaceModel), ViewModel.Model.CurrentWorkspace);
            string category = "Core.Input";

            // Search and check that the results are correct based in the node name provided for the searchTerm
            var nodesResult = ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.Search(category);

            //Check that we got at least 1 result from the Lucene Search
            Assert.That(nodesResult.Count(), Is.GreaterThan(0));

            //We take the first 10 nodes from the result
            foreach (var node in nodesResult.Take(10))
            {
                //All the resulting nodes will have the Category = Core and Class = Input so we check against the first splitted value (by . char) of "Core.Input"
                Assert.IsTrue(node.Category.Contains(category.Split('.')[0]) || node.Category.Contains(category.Split('.')[1]));
            }
        }

        //This test will validate that resulting nodes have a specific order
        [Test]
        [Category("UnitTests")]
        public void LuceneSearchNodesOrderingValidation()
        {
            Assert.IsAssignableFrom(typeof(HomeWorkspaceModel), ViewModel.Model.CurrentWorkspace);
            string searchTerm = "number";
            List<string> expectedSearchResults1 = new List<string> { "number", "number slider", "round" };

            string searchTerm2 = "list.join";
            List<string> expectedSearchResults2 = new List<string> { "join", "list create", "list.map" };

            // Search and check that the results are correct based in the node name provided for the searchTerm
            var nodesResult = ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.Search(searchTerm);
            Assert.IsNotNull(nodesResult);
            Assert.That(nodesResult.Count(), Is.GreaterThan(0));
            var nodesNamesList = nodesResult.Select(x => x.Name.ToLower());


            for(int i = 0; i < 3; i++)
            {
                //Check that the result match the expected position
                Assert.AreEqual(nodesNamesList.ElementAt(i), expectedSearchResults1.ElementAt(i));
            }
            
            // Search and check that the results are correct based in the node name provided for the searchTerm
            nodesResult = ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.Search(searchTerm2);
            Assert.IsNotNull(nodesResult);
            Assert.That(nodesResult.Count(), Is.GreaterThan(0));
            nodesNamesList = nodesResult.Select(x => x.Name.ToLower());

            for (int i = 0; i < 3; i++)
            {
                //Check that the result match the expected position
                Assert.AreEqual(nodesNamesList.ElementAt(i), expectedSearchResults2.ElementAt(i));
            }
        }
    }
}
