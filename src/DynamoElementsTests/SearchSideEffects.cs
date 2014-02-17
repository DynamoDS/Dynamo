using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dynamo.Models;
using NUnit.Framework;

namespace Dynamo.Tests
{
    public class SearchSideEffects : DynamoUnitTest
    {
        [Test]
        public void WhenStartingDynamoInputAndOutputNodesAreMissingFromSearch()
        {
            Assert.IsAssignableFrom( typeof(HomeWorkspaceModel), Controller.DynamoModel.CurrentWorkspace );

            // search and results are correct
            Controller.SearchViewModel.SearchAndUpdateResults("Input");
            Assert.AreEqual(0, Controller.SearchViewModel.SearchResults.Count(x => x.Name == "Input"));

            Controller.SearchViewModel.SearchAndUpdateResults("Output");
            Assert.AreEqual(0, Controller.SearchViewModel.SearchResults.Count(x => x.Name == "Output"));
        }

        [Test]
        public void WhenHomeWorkspaceIsFocusedInputAndOutputNodesAreMissingFromSearch()
        {
            // goto custom node workspace
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\combine", "Sequence2.dyf");
            model.Open(examplePath);

            Assert.AreEqual(model.CurrentWorkspace.Name, "Sequence2");

            // go to homeworkspace
            Controller.DynamoModel.CurrentWorkspace =
                Controller.DynamoModel.Workspaces.OfType<HomeWorkspaceModel>().First();

            Assert.AreEqual(model.CurrentWorkspace.Name, "Home");

            // search and results are correct
            Controller.SearchViewModel.SearchAndUpdateResultsSync("Input");
            Assert.AreEqual(0, Controller.SearchViewModel.SearchResults.Count(x => x.Name == "Input"));

            Controller.SearchViewModel.SearchAndUpdateResultsSync("Output");
            Assert.AreEqual(0, Controller.SearchViewModel.SearchResults.Count(x => x.Name == "Output"));
        }

        [Test]
        public void WhenCustomNodeWorkspaceIsFocusedInputAndOutputNodesArePresentInSearch()
        {
            // goto custom node workspace
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\combine", "Sequence2.dyf");
            model.Open(examplePath);

            Assert.AreEqual(model.CurrentWorkspace.Name, "Sequence2");

            // search and results are correct
            Controller.SearchViewModel.SearchAndUpdateResultsSync("Input");
            Assert.AreEqual(1, Controller.SearchViewModel.SearchResults.Count(x => x.Name == "Input"));
            Assert.AreEqual("Input", Controller.SearchViewModel.SearchResults[0].Name);

            Controller.SearchViewModel.SearchAndUpdateResultsSync("Output");
            Assert.AreEqual(1, Controller.SearchViewModel.SearchResults.Count(x => x.Name == "Output"));
            Assert.AreEqual("Output", Controller.SearchViewModel.SearchResults[0].Name);

        }
    }
}
