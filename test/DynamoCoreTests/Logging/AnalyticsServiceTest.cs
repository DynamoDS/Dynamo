using Dynamo.Configuration;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Graph.Workspaces;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Scheduler;
using DynamoShapeManager;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SystemTestServices;
using TestServices;

namespace Dynamo.Tests.Loggings
{
    [TestFixture]
    class AnalyticsServiceTest : DynamoModelTestBase
    {
        //We need to override this function because the one in DynamoModelTestBase is setting StartInTestMode = true
        protected override DynamoModel.IStartConfiguration CreateStartConfiguration(IPreferences settings)
        {
            return new DynamoModel.DefaultStartConfiguration()
            {
                PathResolver = pathResolver,
                StartInTestMode = false,
                GeometryFactoryPath = preloader.GeometryFactoryPath,
                Preferences = settings,
                ProcessMode = TaskProcessMode.Synchronous
            };
        }

        /// <summary>
        /// This test method will validate that the AnalyticsService.OnWorkspaceAdded (CustomNodeWorkspaceModel) is executed
        /// </summary>
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
