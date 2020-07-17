using System;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Graph.Workspaces;
using Dynamo.Selection;
using NUnit.Framework;

namespace Dynamo.Tests
{
    /// <summary>
    /// Class containing tests for the WorkspaceModel class
    /// </summary>
    [TestFixture]
    class WorkspaceModelTests : DynamoModelTestBase
    {
        public void OpenTestFile(string folder, string fileName)
        {
            var examplePath = Path.Combine(TestDirectory, folder, fileName);
            OpenModel(examplePath);
        }

        [Test]
        [Category("UnitTests")]
        public void CustomNodeDependenciesPropertyGetTest()
        {
            OpenTestFile(@"core\CustomNodes", "add_Read_only.dyf");
            var customWorkspace = CurrentDynamoModel.Workspaces.FirstOrDefault(x => x is CustomNodeWorkspaceModel);
            Assert.IsNotNull(customWorkspace);

            var dependencyCount = customWorkspace.Dependencies.ToList().Count;
            Assert.AreEqual(0, dependencyCount);

            //Loads an e customWorkspace to use as dependency
            OpenTestFile(@"core\CustomNodes", "Centroid.dyf");
            var extraCustomWorkspace = CurrentDynamoModel.Workspaces.LastOrDefault(x => x is CustomNodeWorkspaceModel);
            Assert.IsNotNull(extraCustomWorkspace);
            var cnId = (extraCustomWorkspace as CustomNodeWorkspaceModel).CustomNodeId;
            var customNode = CurrentDynamoModel.CustomNodeManager.CreateCustomNodeInstance(cnId);

            //Adds a custom node to customWorkspace to generate a dependency
            CurrentDynamoModel.CurrentWorkspace = customWorkspace;
            CurrentDynamoModel.AddNodeToCurrentWorkspace(customNode, false);
            dependencyCount = customWorkspace.Dependencies.ToList().Count;
            Assert.AreEqual(1, dependencyCount);

            //Adds the node again but should not increase dependencies
            CurrentDynamoModel.AddNodeToCurrentWorkspace(customNode, false);
            dependencyCount = customWorkspace.Dependencies.ToList().Count;
            Assert.AreEqual(1, dependencyCount);

            //assert that the guid we have stored is the custom nodes functionID
            Assert.AreEqual(customNode.FunctionSignature, CurrentDynamoModel.CurrentWorkspace.Dependencies.First());
        }

        [Test]
        [Category("UnitTests")]
        public void LoadLegacyNotes_AllNewNotesTest()
        {
            //Generating tests data
            var mockViewBlock = new ExtraWorkspaceViewInfo();
            mockViewBlock.Notes = new[] {
                new ExtraNoteViewInfo()
                { Id = Guid.NewGuid().ToString(),
                    Text = "Test data text",
                    X = 1,
                    Y = 1
                },
                new ExtraNoteViewInfo()
                { Id = Guid.NewGuid().ToString(),
                    Text = "Test data text",
                    X = 1,
                    Y = 1
                }
            };

            //Checks no notes exited before
            Assert.AreEqual(0, this.CurrentDynamoModel.CurrentWorkspace.Notes.Count());

            //Adds and checks if notes where added
            this.CurrentDynamoModel.CurrentWorkspace.UpdateWithExtraWorkspaceViewInfo(mockViewBlock);
            Assert.AreEqual(2, this.CurrentDynamoModel.CurrentWorkspace.Notes.Count());
        }

        [Test]
        [Category("UnitTests")]
        public void LoadLegacyNotes_DuplicatedNoteIdTest()
        {
            Guid toBeDuplicatedId = Guid.NewGuid();

            //Generating tests data
            var mockViewBlock = new ExtraWorkspaceViewInfo();
            var firstNote = new ExtraNoteViewInfo()
            {
                Id = toBeDuplicatedId.ToString(),
                Text = "First test note data",
                X = 1,
                Y = 1
            };
            mockViewBlock.Notes = new[] { firstNote };

            //Checks no notes exited before
            Assert.AreEqual(0, this.CurrentDynamoModel.CurrentWorkspace.Notes.Count());

            //Adds and checks that the note was added
            this.CurrentDynamoModel.CurrentWorkspace.UpdateWithExtraWorkspaceViewInfo(mockViewBlock);
            Assert.AreEqual(1, this.CurrentDynamoModel.CurrentWorkspace.Notes.Count());

            //Loads a second note with the same id
            mockViewBlock.Notes = new[] {
                new ExtraNoteViewInfo()
                { Id = toBeDuplicatedId.ToString(),
                    Text = "Second test note data",
                    X = 1,
                    Y = 1
                }
            };
            
            this.CurrentDynamoModel.CurrentWorkspace.UpdateWithExtraWorkspaceViewInfo(mockViewBlock);
            
            //Checks that there is still only one note
            Assert.AreEqual(1, this.CurrentDynamoModel.CurrentWorkspace.Notes.Count());

            //Verifies that the remaining note is in fact the first one added
            Assert.AreEqual(firstNote.Text, this.CurrentDynamoModel.CurrentWorkspace.Notes.FirstOrDefault().Text);
        }

        [Test]
        [Category("UnitTests")]
        public void CheckIfModelExistsInSameGroup_True_ShouldNotCreateNewAnnotation()
        {
            //Add a Node
            var addNode = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+"));
            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(addNode, false);
            Assert.AreEqual(CurrentDynamoModel.CurrentWorkspace.Nodes.Count(), 1);

            //Add a Note 
            Guid id = Guid.NewGuid();
            var addNote = CurrentDynamoModel.CurrentWorkspace.AddNote(false, 200, 200, "This is a test note", id);
            Assert.AreEqual(CurrentDynamoModel.CurrentWorkspace.Notes.Count(), 1);

            //Select the node and notes
            DynamoSelection.Instance.Selection.Add(addNode);
            DynamoSelection.Instance.Selection.Add(addNote);

            //Create the group around selected node and note
            Guid groupId = Guid.NewGuid();
            CurrentDynamoModel.CurrentWorkspace.AddAnnotation("This is a test group", groupId);
            Assert.AreEqual(CurrentDynamoModel.CurrentWorkspace.Annotations.Count(), 1);

            //Tries to create new annotation with the same selection
            var result = CurrentDynamoModel.CurrentWorkspace.AddAnnotation("This is a test group", Guid.NewGuid());
            Assert.IsNull(result);

            //Adds additional node
            var extraNode = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+"));
            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(extraNode);
            Assert.AreEqual(2, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            //Adds new node to selection
            DynamoSelection.Instance.Selection.Add(extraNode);

            //Tries to create new annotation with selected nodes.
            result = CurrentDynamoModel.CurrentWorkspace.AddAnnotation("This is a test group", Guid.NewGuid());
            Assert.IsNull(result);
        }

        [Test]
        [Category("UnitTests")]
        public void CheckIfModelExistsInSameGroup_False_ShouldCreateNewAnnotation()
        {
            //Add a Node
            var addNode = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+"));
            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(addNode, false);
            Assert.AreEqual(CurrentDynamoModel.CurrentWorkspace.Nodes.Count(), 1);

            //Add a Note 
            Guid id = Guid.NewGuid();
            var addNote = CurrentDynamoModel.CurrentWorkspace.AddNote(false, 200, 200, "This is a test note", id);
            Assert.AreEqual(CurrentDynamoModel.CurrentWorkspace.Notes.Count(), 1);

            //Select the node and notes
            DynamoSelection.Instance.Selection.Add(addNode);
            DynamoSelection.Instance.Selection.Add(addNote);

            //Create the group around selected node and note
            Guid groupId = Guid.NewGuid();
            CurrentDynamoModel.CurrentWorkspace.AddAnnotation("This is a test group", groupId);
            Assert.AreEqual(CurrentDynamoModel.CurrentWorkspace.Annotations.Count(), 1);

            //Adds additional node
            var extraNode = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+"));
            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(extraNode);
            Assert.AreEqual(2, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            //Clears previous selection
            DynamoSelection.Instance.ClearSelection();

            //Selects new node
            DynamoSelection.Instance.Selection.Add(extraNode);

            //Create new annotation with selected nodes.
            var result = CurrentDynamoModel.CurrentWorkspace.AddAnnotation("This is a test group", Guid.NewGuid());
            Assert.IsNotNull(result);
        }
    }
}
