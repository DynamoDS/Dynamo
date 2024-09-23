using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamo.Search;
using Dynamo.Search.SearchElements;
using Dynamo.Utilities;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    internal class SearchModelHostIntegrationTests : DynamoModelTestBase
    {

        private static NodeSearchModel search;

        [SetUp]
        public void Init()
        {
            search = new NodeSearchModel();
        }


        /// <summary>
        /// Validate that the "SetParameterByName" node is found in the top 5 results from the indexed Revit 2025 nodes
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void ValidateRevitSetParameterNodeInSearch()
        {
            string expectedNode = "SetParameterByName";
            string[] searchTerms = { "set parameter", "setparameter", "element.setparameter" };
            const int nodeResultsToTake = 5;
            const int expectedNodeIndex = 0;

            string fullJsonNodesPath = Path.Combine(TestDirectory, @"DynamoCoreTests\NodesJsonDatasets\LuceneIndexedNodesRevit.json");
            UpdateIndexedNodesFromJason(fullJsonNodesPath);

            foreach(var searchTerm in searchTerms)
            {
                var results = search.Search(searchTerm, CurrentDynamoModel.LuceneUtility);
                var nameResults = results.Select(node => node.Name);
                // Validates that the Expected Node is found in the Top 5 Search results
                Assert.IsTrue(nameResults.Take(nodeResultsToTake).Contains(expectedNode));

                //Validates that the expected node is at first place
                Assert.IsTrue(nameResults.IndexOf(expectedNode) == expectedNodeIndex, string.Format("The node:{0} is expected at position:{1} but was found in position:{2}",expectedNode, expectedNodeIndex, nameResults.IndexOf(expectedNode)));
            }          
        }

        /// <summary>
        /// Validate that the "GetParameterValueByName" node is found in the top 5 results from the indexed Revit 2025 nodes
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void ValidateRevitGetParameterNodeInSearch()
        {
            string expectedNode = "GetParameterValueByName";
            string[] searchTerms = { "getparameter", "get parameter", "element.getparameter" };
            const int nodeResultsToTake = 5;
            const int expectedNodeIndex = 0;

            string fullJsonNodesPath = Path.Combine(TestDirectory, @"DynamoCoreTests\NodesJsonDatasets\LuceneIndexedNodesRevit.json");
            UpdateIndexedNodesFromJason(fullJsonNodesPath);

            foreach (var searchTerm in searchTerms)
            {
                var results = search.Search(searchTerm, CurrentDynamoModel.LuceneUtility);
                var nameResults = results.Select(node => node.Name);
                // Validates that the Expected Node is found in the Top 5 Search results
                Assert.IsTrue(nameResults.Take(nodeResultsToTake).Contains(expectedNode));

                //Validates that the expected node is at first place
                Assert.IsTrue(nameResults.IndexOf(expectedNode) == expectedNodeIndex, string.Format("The node: {0} is expected at position: {1} but was found in position: {2}", expectedNode, expectedNodeIndex, nameResults.IndexOf(expectedNode)));
            }
        }

        /// <summary>
        /// Validate that several Revit nodes are found in the top 5 results in Revit and also validate that are in the first position
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void ValidateSeveralRevitNodesInSearch()
        {
            string[] expectedNodes = { "PointAtParameter", "Select Model Element", "FamilyName", "List.Combine", "String",};
            string[] searchTerms = { "point at parameter", "select model element", "family name", "combine", "string" };
            const int nodeResultsToTake = 5;
            const int expectedNodeIndex = 0;

            string fullJsonNodesPath = Path.Combine(TestDirectory, @"DynamoCoreTests\NodesJsonDatasets\LuceneIndexedNodesRevit.json");
            UpdateIndexedNodesFromJason(fullJsonNodesPath);

            int index = 0;
            foreach (var searchTerm in searchTerms)
            {
                var results = search.Search(searchTerm, CurrentDynamoModel.LuceneUtility);
                var nameResults = results.Select(node => node.Name);
                var expectedNode = expectedNodes[index];

                // Validates that the Expected Node is found in the Top 5 Search results
                Assert.IsTrue(nameResults.Take(nodeResultsToTake).Contains(expectedNode));

                //Validates that the expected node is at first place
                Assert.IsTrue(nameResults.IndexOf(expectedNode) == expectedNodeIndex, string.Format("The node: {0} is expected at position: {1} but was found in position: {2}", expectedNode, expectedNodeIndex, nameResults.IndexOf(expectedNode)));
                index++;
            }
        }

        /// <summary>
        /// Validate that several Revit nodes are found in the top 5 results in Revit and also validate that are in the first position
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void ValidateListCreateNodeInSearch()
        {
            string expectedNode = "List Create";
            string[] searchTerms = { "list.create", "list create"};
            const int nodeResultsToTake = 5;
            int[] expectedNodeIndex = { 0, 1 };

            string fullJsonNodesPath = Path.Combine(TestDirectory, @"DynamoCoreTests\NodesJsonDatasets\LuceneIndexedNodesRevit.json");
            UpdateIndexedNodesFromJason(fullJsonNodesPath);

            int index = 0;
            foreach (var searchTerm in searchTerms)
            {
                var results = search.Search(searchTerm, CurrentDynamoModel.LuceneUtility);
                var nameResults = results.Select(node => node.Name);

                // Validates that the Expected Node is found in the Top 5 Search results
                Assert.IsTrue(nameResults.Take(nodeResultsToTake).Contains(expectedNode));

                //Validates that the expected node is at first place
                Assert.IsTrue(nameResults.IndexOf(expectedNode) == expectedNodeIndex[index], string.Format("The node: {0} is expected at position: {1} but was found in position: {2}", expectedNode, expectedNodeIndex[index], nameResults.IndexOf(expectedNode)));
                index++;
            }
        }

        /// <summary>
        /// Validate that several Revit nodes are found in the top 5 results in Revit and also validate that are in the first position
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void ValidateTranslateNodeInSearch()
        {
            string expectedNode = "Translate";
            string searchTerm = "translate" ;
            const int nodeResultsToTake = 5;
            const int expectedNodeIndex = 0;

            string fullJsonNodesPath = Path.Combine(TestDirectory, @"DynamoCoreTests\NodesJsonDatasets\LuceneIndexedNodesRevit.json");
            UpdateIndexedNodesFromJason(fullJsonNodesPath);
  
            var results = search.Search(searchTerm, CurrentDynamoModel.LuceneUtility);
            var nameResults = results.Select(node => node.Name);

            // Validates that the Expected Node is found in the Top 5 Search results
            Assert.IsTrue(nameResults.Contains(expectedNode));

            foreach (var result in nameResults.Take(nodeResultsToTake))
            {
                //Validates that the first 5 nodes in the result start with the word "Translate"
                Assert.That(result.Contains("Translate"), Is.True);
            }
            //Validates that the expected node is at first place
            Assert.IsTrue(nameResults.IndexOf(expectedNode) == expectedNodeIndex, string.Format("The node: {0} is expected at position: {1} but was found in position: {2}", expectedNode, expectedNodeIndex, nameResults.IndexOf(expectedNode)));
        }


        /// <summary>
        /// Validate that the "Choose Text Style" node is found in the top 5 results from the indexed Civil3D 2025.1 nodes
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void ValidateCivil3DChooseTextStyleNodesInSearch()
        {
            string expectedNode = "Choose Text Style";
            string[] searchTerms = { "choose text", "choose text style" };
            const int nodeResultsToTake = 5;

            int[] expectedNodeIndex = [1, 0];

            string fullJsonNodesPath = Path.Combine(TestDirectory, @"DynamoCoreTests\NodesJsonDatasets\LuceneIndexedInfoC3D.json");
            UpdateIndexedNodesFromJason(fullJsonNodesPath);

            int index = 0;
            foreach (var searchTerm in searchTerms)
            {
                var results = search.Search(searchTerm, CurrentDynamoModel.LuceneUtility);
                var nameResults = results.Select(node => node.Name);
                //Validates that the Expected Node is found in the Top 5 Search results
                Assert.IsTrue(nameResults.Take(nodeResultsToTake).Contains(expectedNode));

                //Validates that the expected node is at specific place defined in the expectedNodeIndex array
                int expectedIndex = expectedNodeIndex[index];
                Assert.IsTrue(nameResults.IndexOf(expectedNode) == expectedIndex, string.Format("The node: {0} is expected at position: {1} but was found in position: {2}", expectedNode, expectedIndex, nameResults.IndexOf(expectedNode)));
                index++;
            }
        }

        /// <summary>
        /// Validate that several T-Spline nodes are found in the top 10 results from the nodes indexed in DynamoSandbox
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void ValidateTSplineNodesInSearch()
        {
            string[] expectedNodes = {"BySphereFourPoints", "ByConePointsRadius", "ByCylinderPointsRadius", "Plane"};
            string[] searchTerms = { "sphere", "cone", "cylinder", "plane"};

            //TO-DO Update this array according to the results after the Search algorithm is updated/refactored
            int[] expectedNodeIndex = [6, 4, 5, 0];

            //This value will need to be updated to 5 after fixing the Search Algorithm when we are getting better results 
            const int nodeResultsToTake = 10;

            string fullJsonNodesPath = Path.Combine(TestDirectory, @"DynamoCoreTests\NodesJsonDatasets\LuceneIndexedInfoSandboxTSplines.json");
            UpdateIndexedNodesFromJason(fullJsonNodesPath);

            int index = 0;
            foreach (var searchTerm in searchTerms)
            {
                var results = search.Search(searchTerm, CurrentDynamoModel.LuceneUtility);
                var nameResults = results.Select(node => node.Name);
                //Validates that the Expected Node is found in the Top 5 Search results
                Assert.IsTrue(nameResults.Take(nodeResultsToTake).Contains(expectedNodes[searchTerms.IndexOf(searchTerm)]));

                //Validates that the expected node is at specific place defined in the expectedNodeIndex array
                var expectedNode = expectedNodes[searchTerms.IndexOf(searchTerm)];
                int expectedIndex = expectedNodeIndex[index];
                Assert.IsTrue(nameResults.IndexOf(expectedNode) == expectedIndex, string.Format("The node: {0} is expected at position: {1} but was found in position: {2}", expectedNode, expectedIndex, nameResults.IndexOf(expectedNode)));
                index++;
            }
        }

        #region Utility Methods
        /// <summary>
        /// Read a json array and convert it to a list of nodes which can be indexed for Lucene Search
        /// </summary>
        /// <param name="jsonFileFullPath">Full path (including file name) to the json file containing an array of nodes info</param>
        /// <returns></returns>
        private List<NodeSearchElement> GetNodesFromJson(string jsonFileFullPath)
        {
            using StreamReader reader = new(jsonFileFullPath);
            var json = reader.ReadToEnd();
            var jsonArray = JArray.Parse(json);
            List<NodeSearchElement> nodesList = new List<NodeSearchElement>();
            foreach (var data in jsonArray)
            {
                var name = (string)data["Name"];
                var category = (string)data["FullCategoryName"];
                var description = (string)data["Description"];
                var element = new CustomNodeSearchElement(null, new CustomNodeInfo(Guid.NewGuid(), name, category, description, ""));
                nodesList.Add(element);
            }
            return nodesList;
        }

        private void UpdateIndexedNodesFromJason(string fullJsonNodesPath)
        {
            var nodesList = GetNodesFromJson(fullJsonNodesPath);

            foreach (var node in nodesList)
            {
                search.Add(node);
                CurrentDynamoModel.SearchModel.Add(node);
            }
            
            CurrentDynamoModel.LuceneUtility.UpdateIndexedNodesInfo(nodesList);
        }
        #endregion
    }
}
