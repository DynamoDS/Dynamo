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
            libraries.Add("VMDataBridge.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("ProtoGeometry.dll");
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
            string category = "Core.Input.";

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

        /// <summary>
        /// This test validates several cases using Search Category Based (like "category.node")
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void LuceneValidateCategoryBasedSearch()
        {
            Assert.IsAssignableFrom(typeof(HomeWorkspaceModel), ViewModel.Model.CurrentWorkspace);
            string category = "FileSystem";
            string nodeName = "F";
            string searchTerm = category + "." + nodeName;

            // Search and check that the results are correct based in the node name provided for the searchTerm
            var nodesResult = ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.Search(searchTerm);

            //Take the first 5 elements in the results
            var topFourResults = nodesResult.Take(5);
            //Validate that the top 4 elements in the results start with "F"
            Assert.That(topFourResults.Where(x => x.Name.StartsWith(nodeName)).Count() == 4, Is.True);
            //Validate that the top 5 elements in the results belong to the FileSystem category
            Assert.That(topFourResults.Where(x => x.Class.Equals(category)).Count() == 5);

            nodeName = "Append";
            searchTerm = category + "." + nodeName;
            nodesResult = ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.Search(searchTerm);
            //Validate that the first in the node is AppendText
            Assert.That(nodesResult.Take(1).First().Name.StartsWith(nodeName), Is.True);
            //Validate that the first result belong to the FileSystem category
            Assert.That(nodesResult.Take(1).First().Class == category, Is.True);

            searchTerm = ".";
            //This search should not return results since we are searching just for the "." char
            nodesResult = ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.Search(searchTerm);
            Assert.That(nodesResult.Count() == 0, Is.True);
        }

        //This test will validate that resulting nodes have a specific order
        [Test]
        [Category("UnitTests")]
        public void LuceneSearchNodesOrderingValidation()
        {
            Assert.IsAssignableFrom(typeof(HomeWorkspaceModel), ViewModel.Model.CurrentWorkspace);
            string searchTerm = "number";
            List<string> expectedSearchResults1 = new List<string> { "number", "number slider", "numberofcurves" };

            string searchTerm2 = "list.join";
            List<string> expectedSearchResults2 = new List<string> { "join", "list create", "range" };

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

        //This test will validate that resulting nodes have a specific order when having T-Spline nodes in the nodes list.
        [Test]
        public void LuceneSearchTSplineNodesOrderingValidation()
        {
            Assert.IsAssignableFrom(typeof(HomeWorkspaceModel), ViewModel.Model.CurrentWorkspace);
            string[] searchTerms = { "sphere", "cone" };

            // Search for "sphere" and check that the results are correct based in the node name provided for the searchTerms
            var nodesResult = ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.Search(searchTerms[0]);

            Assert.IsNotNull(nodesResult);
            Assert.That(nodesResult.Count(), Is.GreaterThan(0));
            var firstTSpline = nodesResult.Take(15).ToList().FindIndex(x => x.Class.ToLower().Contains("tspline"));

            //Take the first 5 elements, get the ones that belong to the expected category and finally get the index in the list
            var firstCatExpectedNode = nodesResult.Take(5).ToList().FindIndex(x => x.Class.ToLower().Contains(searchTerms[0]));

            //Validate that the normal node (category Sphere) will be at index 0 (first place) and the TSpline node at index 3 (3 > 0)
            Assert.That(firstTSpline > firstCatExpectedNode);

            // Search for "cone "and check that the results are correct based in the node name provided for the searchTerms
            nodesResult = ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.Search(searchTerms[1]);

            Assert.IsNotNull(nodesResult);
            Assert.That(nodesResult.Count(), Is.GreaterThan(0));
            firstTSpline = nodesResult.Take(5).ToList().FindIndex(x => x.Class.ToLower().Contains("tspline"));

            //Take the first 5 elements, get the ones that belong to the expected category and finally get the index in the list
            firstCatExpectedNode = nodesResult.Take(5).ToList().FindIndex(x => x.Class.ToLower().Contains(searchTerms[1]));

            //Validate that T-Spline nodes are not found in the top 5 results
            Assert.That(firstTSpline == -1);

            //Validate that the normal node (category Cone) will be at the first top 5 positions
            Assert.That(firstCatExpectedNode >= 0 && firstCatExpectedNode < 5);

        }


        //This test will validate that File Path node is found when using the criteria "file path"
        [Test]
        [Category("UnitTests")]
        public void LuceneSearchFilePathValidation()
        {
            Assert.IsAssignableFrom(typeof(HomeWorkspaceModel), ViewModel.Model.CurrentWorkspace);
            string searchTerm = "file path";

            // Search and check that the results are correct based in the node name provided for the searchTerm
            var nodesResult = ViewModel.CurrentSpaceViewModel.InCanvasSearchViewModel.Search(searchTerm);
            Assert.IsNotNull(nodesResult);
            Assert.That(nodesResult.Count(), Is.GreaterThan(0));

            //Validate that the file path node is in the first 5 elements of the resulting list
            var nodesNamesList = nodesResult.Take(5).Select(x => x.Name.ToLower());
            Assert.IsTrue(nodesNamesList.Contains(searchTerm));
        }
    }
}
