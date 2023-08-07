using System.IO;
using System.Linq;
using System.Windows.Annotations;
using CoreNodeModels.Input;
using Dynamo.Graph.Annotations;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Notes;
using Dynamo.Models;
using Dynamo.Selection;
using Dynamo.Tests;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    class CustomNodeTests :  DynamoViewModelUnitTest
    {
        [Test]
        public void TestNewNodeFromSelectionCommandState()
        {
            Assert.IsFalse(ViewModel.CurrentSpaceViewModel.NodeFromSelectionCommand.CanExecute(null));

            var node = new DoubleInput();
            ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(node, false);
            ViewModel.Model.AddToSelection(node);
            Assert.IsTrue(ViewModel.CurrentSpaceViewModel.NodeFromSelectionCommand.CanExecute(null));

            var ws = ViewModel.Model.CustomNodeManager.Collapse(
                DynamoSelection.Instance.Selection.OfType<NodeModel>(),
                Enumerable.Empty<NoteModel>(),
                ViewModel.Model.CurrentWorkspace,
                true,
                new FunctionNamePromptEventArgs
                {
                    Category = "Testing",
                    Description = "",
                    Name = "__CollapseTest2__",
                    Success = true
                });

            Assert.IsFalse(ViewModel.CurrentSpaceViewModel.NodeFromSelectionCommand.CanExecute(null));
        }

        [Test]
        public void TestNestedGroupsOnCustomNodes()
        {
            string openPath = Path.Combine(TestDirectory, @"core\collapse\collapse-group.dyn");
            OpenModel(openPath);

            foreach (var node in ViewModel.CurrentSpaceViewModel.Nodes)
            {
                DynamoSelection.Instance.Selection.Add(node.NodeModel);
            }
            foreach (var note in ViewModel.CurrentSpaceViewModel.Notes)
            {
                DynamoSelection.Instance.Selection.Add(note.Model);
            }
            foreach (var grp in ViewModel.CurrentSpaceViewModel.Annotations)
            {
                DynamoSelection.Instance.Selection.Add(grp.AnnotationModel);
            }

            //assert that everything is selected
            var groupsSelected = DynamoSelection.Instance.Selection.OfType<AnnotationModel>().Count();
            var nodesSelected = DynamoSelection.Instance.Selection.OfType<NodeModel>().Count();
            var notesSelected = DynamoSelection.Instance.Selection.OfType<NoteModel>().Count();
            Assert.AreEqual(ViewModel.CurrentSpaceViewModel.Annotations.Count(), groupsSelected); // 3 groups
            Assert.AreEqual(ViewModel.CurrentSpaceViewModel.Nodes.Count(), nodesSelected); //6 nodes
            Assert.AreEqual(ViewModel.CurrentSpaceViewModel.Notes.Count(), notesSelected); // 2 notes

            var ws = ViewModel.Model.CustomNodeManager.Collapse(
                DynamoSelection.Instance.Selection.OfType<NodeModel>(),
                DynamoSelection.Instance.Selection.OfType<NoteModel>(),
                ViewModel.Model.CurrentWorkspace,
                true,
                new FunctionNamePromptEventArgs
                {
                    Category = "Testing",
                    Description = "",
                    Name = "__CollapseTest3__",
                    Success = true
                });

            ViewModel.Model.AddCustomNodeWorkspace(ws);
            ViewModel.Model.OpenCustomNodeWorkspace(ws.CustomNodeId);
            var customNodeWorkspace = ViewModel.Model.CurrentWorkspace;

            Assert.AreEqual(groupsSelected, customNodeWorkspace.Annotations.Count());
            Assert.AreEqual(nodesSelected, customNodeWorkspace.Nodes.Count() - 1); //Subtract 1 for the Output Node
            Assert.AreEqual(notesSelected, customNodeWorkspace.Notes.Count());

            //assert that note and groups are still associated
            var maingroup = customNodeWorkspace.Annotations.First(x => x.AnnotationText.Equals("Test"));
            Assert.AreEqual(2, maingroup.Nodes.OfType<AnnotationModel>().Count()); // 2 groups inside the main group
            Assert.AreEqual(1, maingroup.Nodes.OfType<NoteModel>().Count()); // 1 note inside the main group
        }
    }
}
