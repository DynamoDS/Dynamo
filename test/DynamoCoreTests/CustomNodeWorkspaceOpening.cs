using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.Search;
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
        public void CanLoadReadOnlyNode()
        {
            // Open a read-only custom node
            var pathInTestsDir = @"core\CustomNodes\add_Read_only.dyf";
            var filePath = Path.Combine(TestDirectory, pathInTestsDir);
            FileInfo fInfo = new FileInfo(filePath);
            fInfo.IsReadOnly = true;
            Assert.IsTrue(DynamoUtilities.PathHelper.IsReadOnlyPath(filePath));

            OpenTestFile(@"core\CustomNodes", "add_Read_only.dyf");
            var nodeWorkspace = CurrentDynamoModel.Workspaces.FirstOrDefault(x => x is CustomNodeWorkspaceModel);
            Assert.IsNotNull(nodeWorkspace);

            // a file with a read-only custom node definition is opened
            OpenTestFile(@"core\CustomNodes", "TestAdd.dyn");
            var homeWorkspace = CurrentDynamoModel.CurrentWorkspace as HomeWorkspaceModel;
            Assert.NotNull(homeWorkspace);
            homeWorkspace.Run();

            var funcNode = homeWorkspace.Nodes.OfType<Function>().First();
            Assert.AreEqual(2.0, GetPreviewValue(funcNode.GUID));
        }

        [Test]
        public void CanOpenXmlCustomNodeWorkspace()
        {
            OpenTestFile(@"core\combine", "Sequence2.dyf");

            var nodeWorkspace = CurrentDynamoModel.Workspaces.FirstOrDefault(x => x is CustomNodeWorkspaceModel);
            Assert.IsNotNull(nodeWorkspace);
            Assert.AreEqual(CurrentDynamoModel.CurrentWorkspace.Name, "Sequence2");
        }

        [Test]
        public void XmlCustomNodeWorkspaceIsAddedToSearchOnOpening()
        {
            OpenTestFile(@"core\combine", "Sequence2.dyf");
            
            var res = CurrentDynamoModel.SearchModel.Search("Sequence2", CurrentDynamoModel.LuceneSearchUtility);
            Assert.AreEqual("Sequence2", res.First().Name);
        }

        [Test]
        public void CanOpenJsonCustomNodeWorkspace()
        {
            OpenTestFile(@"core\combine", "Sequence_Json.dyf");

            var nodeWorkspace = CurrentDynamoModel.Workspaces.FirstOrDefault(x => x is CustomNodeWorkspaceModel);
            Assert.IsNotNull(nodeWorkspace);
            Assert.AreEqual(CurrentDynamoModel.CurrentWorkspace.Name, "Sequence2");
        }

        [Test]
        public void JsonCustomNodeWorkspaceIsAddedToSearchOnOpeningDyf()
        {
            OpenTestFile(@"core\combine", "Sequence_Json.dyf");


            var res = CurrentDynamoModel.SearchModel.Search("Sequence2", CurrentDynamoModel.LuceneSearchUtility);
            Assert.AreEqual("Sequence2", res.First().Name);
        }

        [Test]
        public void JsonCustomNodeWorkspaceIsAddedToSearchOnOpeningDyn()
        {
            // Opening a dyn should automatically add any dyf in the same
            // folder as CustomNodeWorkspace and maintain the saved category
            OpenTestFile(@"core\combine", "combine-with-three.dyn");

            var res = CurrentDynamoModel.SearchModel.Search("Sequence2", CurrentDynamoModel.LuceneSearchUtility);
            Assert.AreEqual("Sequence2", res.First().Name);
            Assert.AreEqual("Misc", res.First().FullCategoryName);
        }

        [Test]
        [Category("UnitTests")]
        public void TestOnWorkspaceAdded()
        {
            //Arrange
            // Open/Run XML test graph
            string openPath = Path.Combine(TestDirectory, @"core\Angle.dyn");
            RunModel(openPath);
            int InitialNodesCount = CurrentDynamoModel.CurrentWorkspace.Nodes.Count();

            // Convert a DSFunction node Line.ByPointDirectionLength to custom node.
            var workspace = CurrentDynamoModel.CurrentWorkspace;
            var node = workspace.Nodes.OfType<DSFunction>().First();

            List<NodeModel> selectionSet = new List<NodeModel>() { node };
            var customWorkspace = CurrentDynamoModel.CustomNodeManager.Collapse(
                selectionSet.AsEnumerable(),
                Enumerable.Empty<Dynamo.Graph.Notes.NoteModel>(),
                CurrentDynamoModel.CurrentWorkspace,
                true,
                new FunctionNamePromptEventArgs
                {
                    Category = "Testing",
                    Description = "",
                    Name = "__AnalyticsServiceTest__",
                    Success = true
                }) as CustomNodeWorkspaceModel;

            //Act
            //This will execute the custom workspace assigment and trigger the added workspace assigment event
            CurrentDynamoModel.OpenCustomNodeWorkspace(customWorkspace.CustomNodeId);

            //This will add a new custom node to the workspace
            var addNode = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+"));
            var ws = CurrentDynamoModel.CustomNodeManager.CreateCustomNode("someNode", "someCategory", "");
            var csid = (ws as CustomNodeWorkspaceModel).CustomNodeId;
            var customNode = CurrentDynamoModel.CustomNodeManager.CreateCustomNodeInstance(csid);

            CurrentDynamoModel.AddNodeToCurrentWorkspace(customNode, false);
            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(addNode, false);

            //Assert
            //At the begining the CurrentWorkspace.Nodes has 4 nodes but two new nodes were added, then verify we have 5 nodes.
            Assert.AreEqual(CurrentDynamoModel.CurrentWorkspace.Nodes.Count(), InitialNodesCount + 2);
        }
    }

}
