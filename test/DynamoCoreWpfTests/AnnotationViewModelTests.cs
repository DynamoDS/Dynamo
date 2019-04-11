using System.Linq;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Models;
using Dynamo.Selection;
using Dynamo.Tests;
using Dynamo.Utilities;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    public class AnnotationViewModelTests : DynamoViewModelUnitTest
    {
        #region Annotation

        [Test]
        [Category("DynamoUI")]
        public void CreateGroupAroundNodes()
        {
            //Create a Node
            var addNode = new DSFunction(ViewModel.Model.LibraryServices.GetFunctionDescriptor("+"));
            ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(addNode, false);
           
            //verify the node was created
            Assert.AreEqual(1, ViewModel.Model.CurrentWorkspace.Nodes.Count());

            //Select the note for group
            DynamoSelection.Instance.Selection.Add(addNode);

            //Create a Group around that node
            ViewModel.AddAnnotationCommand.Execute(null);
            var annotation = ViewModel.Model.CurrentWorkspace.Annotations.FirstOrDefault();
            Assert.IsNotNull(annotation);

            //verify whether group is not empty
            Assert.AreNotEqual(0, annotation.Y);
            Assert.AreNotEqual(0, annotation.X);
            Assert.AreNotEqual(0, annotation.Width);
            Assert.AreNotEqual(0, annotation.Height);           
        }

        [Test]
        [Category("DynamoUI")]
        public void CannotCreateGroupIfNoModelsAreSelectedInTheCanvas()
        {
            //Check whether Create Group is enabled in a blank canvas.
            Assert.AreEqual(false, ViewModel.CanAddAnnotation(null));
        }


        [Test]
        [Category("DynamoUI")]
        public void CanCreateGroupIfANodeIsAlreadyInAGroup()
        {
            //Create a Node
            var addNode = new DSFunction(ViewModel.Model.LibraryServices.GetFunctionDescriptor("+"));
            ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(addNode, false);

            //verify the node was created
            Assert.AreEqual(1, ViewModel.Model.CurrentWorkspace.Nodes.Count());

            //Select the note for group
            DynamoSelection.Instance.Selection.Add(addNode);

            //Create a Group around that node
            ViewModel.AddAnnotationCommand.Execute(null);
            var annotation = ViewModel.Model.CurrentWorkspace.Annotations.FirstOrDefault();

            //Check if the group is created
            Assert.IsNotNull(annotation);

            //Clear the selection
            DynamoSelection.Instance.ClearSelection();

            //Select the node again
            DynamoSelection.Instance.Selection.Add(addNode);

            //Check whether group can be created
            Assert.AreEqual(false,ViewModel.CanAddAnnotation(null));

        }

        [Test]
        [Category("DynamoUI")]
        public void NestedGroupTestForNodes()
        {
            //Create a Node
            var addNode = new DSFunction(ViewModel.Model.LibraryServices.GetFunctionDescriptor("+"));
            ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(addNode, false);

            //verify the node was created
            Assert.AreEqual(1, ViewModel.Model.CurrentWorkspace.Nodes.Count());

            //Select the node for group
            DynamoSelection.Instance.Selection.Add(addNode);

            //Create a Group around that node
            ViewModel.AddAnnotationCommand.Execute(null);
            var annotation = ViewModel.Model.CurrentWorkspace.Annotations.FirstOrDefault();

            //Check if the group is created
            Assert.IsNotNull(annotation);

            //Clear the selection
            DynamoSelection.Instance.ClearSelection();

            var secondNode = new DSFunction(ViewModel.Model.LibraryServices.GetFunctionDescriptor("+"));
            ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(secondNode, false);

            //verify the node was created
            Assert.AreEqual(2, ViewModel.Model.CurrentWorkspace.Nodes.Count());

            //Select the nodes again - add node is in a group and secondnode is not in a group
            DynamoSelection.Instance.Selection.Add(addNode);
            DynamoSelection.Instance.Selection.Add(secondNode);

            //Check whether group can be created
            Assert.AreEqual(false, ViewModel.CanAddAnnotation(null));

        }

        [Test]
        [Category("DynamoUI")]
        public void CanUngroupNodeFromAGroup()
        {
            //Create a Node
            var addNode = new DSFunction(ViewModel.Model.LibraryServices.GetFunctionDescriptor("+"));
            ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(addNode, false);

            //verify the node was created
            Assert.AreEqual(1, ViewModel.Model.CurrentWorkspace.Nodes.Count());

            //Select the node for group
            DynamoSelection.Instance.Selection.Add(addNode);

            //Create a Group around that node
            ViewModel.AddAnnotationCommand.Execute(null);
            var annotation = ViewModel.Model.CurrentWorkspace.Annotations.FirstOrDefault();

            //Check if the group is created
            Assert.IsNotNull(annotation);

            //Clear the selection
            DynamoSelection.Instance.ClearSelection();

            var secondNode = new DSFunction(ViewModel.Model.LibraryServices.GetFunctionDescriptor("+"));
            ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(secondNode, false);

            //verify the node was created
            Assert.AreEqual(2, ViewModel.Model.CurrentWorkspace.Nodes.Count());

            //Select the node 
            DynamoSelection.Instance.Selection.Add(addNode);
          
            //Check whether group can be created
            Assert.AreEqual(true, ViewModel.CanUngroupModel(null));
        }

        [Test]
        [Category("DynamoUI")]
        public void CanUngroupNodeWhichIsNotInAGroup()
        {
            //Create a Node
            var addNode = new DSFunction(ViewModel.Model.LibraryServices.GetFunctionDescriptor("+"));
            ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(addNode, false);

            //verify the node was created
            Assert.AreEqual(1, ViewModel.Model.CurrentWorkspace.Nodes.Count());

            //Select the node for group
            DynamoSelection.Instance.Selection.Add(addNode);

            //Create a Group around that node
            ViewModel.AddAnnotationCommand.Execute(null);
            var annotation = ViewModel.Model.CurrentWorkspace.Annotations.FirstOrDefault();

            //Check if the group is created
            Assert.IsNotNull(annotation);

            //Clear the selection
            DynamoSelection.Instance.ClearSelection();

            var secondNode = new DSFunction(ViewModel.Model.LibraryServices.GetFunctionDescriptor("+"));
            ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(secondNode, false);
          
            //Select the node 
            DynamoSelection.Instance.Selection.Add(secondNode);

            //Check whether group can be created
            Assert.AreEqual(false, ViewModel.CanUngroupAnnotation(null));
        }

        [Test]
        [Category("DynamoUI")]
        public void CanAddToGroupForNodes()
        {
            //Create a Node
            var addNode = new DSFunction(ViewModel.Model.LibraryServices.GetFunctionDescriptor("+"));
            ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(addNode, false);

            //verify the node was created
            Assert.AreEqual(1, ViewModel.Model.CurrentWorkspace.Nodes.Count());

            //Select the node for group
            DynamoSelection.Instance.Selection.Add(addNode);

            //Create a Group around that node
            ViewModel.AddAnnotationCommand.Execute(null);
            var annotation = ViewModel.Model.CurrentWorkspace.Annotations.FirstOrDefault();

            //Check if the group is created
            Assert.IsNotNull(annotation);

            //Clear the selection
            DynamoSelection.Instance.ClearSelection();

            //Select the group
            DynamoSelection.Instance.Selection.Add(annotation);

            var secondNode = new DSFunction(ViewModel.Model.LibraryServices.GetFunctionDescriptor("+"));
            ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(secondNode, false);

            //Select the node 
            DynamoSelection.Instance.Selection.Add(secondNode);

            //Check whether group can be created
            Assert.AreEqual(true, ViewModel.CanAddModelsToGroup(null));
        }

        [Test]
        [Category("DynamoUI")]
        public void CanAddToGroupForNodeWhichIsAlreadyInAGroup()
        {
            //Create a Node
            var addNode = new DSFunction(ViewModel.Model.LibraryServices.GetFunctionDescriptor("+"));
            ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(addNode, false);

            //verify the node was created
            Assert.AreEqual(1, ViewModel.Model.CurrentWorkspace.Nodes.Count());

            //Select the node for group
            DynamoSelection.Instance.Selection.Add(addNode);

            //Create a Group around that node
            ViewModel.AddAnnotationCommand.Execute(null);
            var annotation = ViewModel.Model.CurrentWorkspace.Annotations.FirstOrDefault();

            //Check if the group is created
            Assert.IsNotNull(annotation);

            //Clear the selection
            DynamoSelection.Instance.ClearSelection();

            //Select the node 
            DynamoSelection.Instance.Selection.Add(addNode);

            //Check whether group can be created
            Assert.AreEqual(false, ViewModel.CanAddModelsToGroup(null));
        }


        [Test]
        [Category("DynamoUI")]
        public void CreateDeleteGroupAroundNodes()
        {
            //Create a Node
            var addNode = new DSFunction(ViewModel.Model.LibraryServices.GetFunctionDescriptor("+"));
            ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(addNode, false);
           
            //verify the node was created
            Assert.AreEqual(1, ViewModel.Model.CurrentWorkspace.Nodes.Count());

            //Select the note for group
            DynamoSelection.Instance.Selection.Add(addNode);

            //Create a Group around that node
            ViewModel.AddAnnotationCommand.Execute(null);
            var annotation = ViewModel.Model.CurrentWorkspace.Annotations.FirstOrDefault();
            Assert.IsNotNull(annotation);

            //verify that group was created
            Assert.AreEqual(1, ViewModel.Model.CurrentWorkspace.Annotations.Count());

            //select the group for deletion
            DynamoSelection.Instance.Selection.Add(annotation);
            Assert.AreEqual(1, DynamoSelection.Instance.Selection.Count);

            //delete the note
            ViewModel.DeleteCommand.Execute(null);
            Assert.AreEqual(0, ViewModel.Model.CurrentWorkspace.Annotations.Count());

            //verify only annotation was deleted and not the note
            Assert.AreEqual(1, ViewModel.Model.CurrentWorkspace.Nodes.Count());
        }

        [Test]
        [Category("DynamoUI")]
        public void CreateGroupAroundNotes()
        {
            //First, create a Note
            ViewModel.AddNoteCommand.Execute(null);
            var note = ViewModel.Model.CurrentWorkspace.Notes.FirstOrDefault();
            Assert.IsNotNull(note);

            //verify the note was created
            Assert.AreEqual(1, ViewModel.Model.CurrentWorkspace.Notes.Count());

            //Select the note for group
            DynamoSelection.Instance.Selection.Add(note);

            //Create a Group around that note
            ViewModel.AddAnnotationCommand.Execute(null);
            var annotation = ViewModel.Model.CurrentWorkspace.Annotations.FirstOrDefault();
            Assert.IsNotNull(annotation);

            //verify that group was created
            Assert.AreEqual(1, ViewModel.Model.CurrentWorkspace.Annotations.Count());

            //verify whether group is not empty
            Assert.AreNotEqual(0, annotation.Y);
            Assert.AreNotEqual(0, annotation.X);
            Assert.AreNotEqual(0, annotation.Width);
            Assert.AreNotEqual(0, annotation.Height);           
        }

        [Test]
        [Category("DynamoUI")]
        public void CreateDeleteGroupAroundNotes()
        {
            //First, create a Note
            ViewModel.AddNoteCommand.Execute(null);
            var note = ViewModel.Model.CurrentWorkspace.Notes.FirstOrDefault();
            Assert.IsNotNull(note);

            //verify the note was created
            Assert.AreEqual(1, ViewModel.Model.CurrentWorkspace.Notes.Count());

            //Create a Group around that note
            ViewModel.AddAnnotationCommand.Execute(null);
            var annotation = ViewModel.Model.CurrentWorkspace.Annotations.FirstOrDefault();
            Assert.IsNotNull(annotation);

            //verify that group was created
            Assert.AreEqual(1, ViewModel.Model.CurrentWorkspace.Annotations.Count());

            //select the group for deletion
            DynamoSelection.Instance.Selection.Add(annotation);
            Assert.AreEqual(1, DynamoSelection.Instance.Selection.Count);

            //delete the note
            ViewModel.DeleteCommand.Execute(null);
            Assert.AreEqual(0, ViewModel.Model.CurrentWorkspace.Annotations.Count());

            //verify only annotation was deleted and not the note
            Assert.AreEqual(1, ViewModel.Model.CurrentWorkspace.Notes.Count());
        }

        [Test]
        [Category("DynamoUI")]
        public void CanCreateGroupIfANoteIsAlreadyInAGroup()
        {
            //First, create a Note
            ViewModel.AddNoteCommand.Execute(null);
            var note = ViewModel.Model.CurrentWorkspace.Notes.FirstOrDefault();
            Assert.IsNotNull(note);

            //verify the note was created
            Assert.AreEqual(1, ViewModel.Model.CurrentWorkspace.Notes.Count());

            //Select the note for group
            DynamoSelection.Instance.Selection.Add(note);

            //Create a Group around that node
            ViewModel.AddAnnotationCommand.Execute(null);
            var annotation = ViewModel.Model.CurrentWorkspace.Annotations.FirstOrDefault();

            //Check if the group is created
            Assert.IsNotNull(annotation);

            //Clear the selection
            DynamoSelection.Instance.ClearSelection();

            //Select the node again
            DynamoSelection.Instance.Selection.Add(note);

            //Check whether group can be created
            Assert.AreEqual(false, ViewModel.CanAddAnnotation(null));
        }

        [Test]
        [Category("DynamoUI")]
        public void NestedGroupTestForNotes()
        {
            //First, create a Note
            ViewModel.AddNoteCommand.Execute(null);
            var note = ViewModel.Model.CurrentWorkspace.Notes.FirstOrDefault();
            Assert.IsNotNull(note);

            //verify the note was created
            Assert.AreEqual(1, ViewModel.Model.CurrentWorkspace.Notes.Count());

            //Select the node for group
            DynamoSelection.Instance.Selection.Add(note);

            //Create a Group around that node
            ViewModel.AddAnnotationCommand.Execute(null);
            var annotation = ViewModel.Model.CurrentWorkspace.Annotations.FirstOrDefault();

            //Check if the group is created
            Assert.IsNotNull(annotation);

            //Clear the selection
            DynamoSelection.Instance.ClearSelection();

            ViewModel.AddNoteCommand.Execute(null);
            var secondnote = ViewModel.Model.CurrentWorkspace.Notes.FirstOrDefault();
            Assert.IsNotNull(secondnote);


            //verify the node was created
            Assert.AreEqual(2, ViewModel.Model.CurrentWorkspace.Notes.Count());

            //Select the notes again 
            DynamoSelection.Instance.Selection.Add(note);
            DynamoSelection.Instance.Selection.Add(secondnote);

            //Check whether group can be created - Should be false.
            Assert.AreEqual(false, ViewModel.CanAddAnnotation(null));
        }

        [Test]
        [Category("DynamoUI")]
        public void CanUngroupNoteFromAGroup()
        {
            //First, create a Note
            ViewModel.AddNoteCommand.Execute(null);
            var note = ViewModel.Model.CurrentWorkspace.Notes.FirstOrDefault();
            Assert.IsNotNull(note);

            //verify the note was created
            Assert.AreEqual(1, ViewModel.Model.CurrentWorkspace.Notes.Count());

            //Select the note for group
            DynamoSelection.Instance.Selection.Add(note);

            //Create a Group around that note
            ViewModel.AddAnnotationCommand.Execute(null);
            var annotation = ViewModel.Model.CurrentWorkspace.Annotations.FirstOrDefault();

            //Check if the group is created
            Assert.IsNotNull(annotation);

            //Clear the selection
            DynamoSelection.Instance.ClearSelection();

            ViewModel.AddNoteCommand.Execute(null);
            var secondnote = ViewModel.Model.CurrentWorkspace.Notes.FirstOrDefault();
            Assert.IsNotNull(secondnote);

            //verify the node was created
            Assert.AreEqual(2, ViewModel.Model.CurrentWorkspace.Notes.Count());

            //Select the note
            DynamoSelection.Instance.Selection.Add(secondnote);

            //Check whether group can be created
            Assert.AreEqual(true, ViewModel.CanUngroupModel(null));
        }

        [Test]
        [Category("DynamoUI")]
        public void CanAddToGroupForNotes()
        {
            //First, create a Note
            ViewModel.AddNoteCommand.Execute(null);
            var note = ViewModel.Model.CurrentWorkspace.Notes.FirstOrDefault();
            Assert.IsNotNull(note);

            //verify the note was created
            Assert.AreEqual(1, ViewModel.Model.CurrentWorkspace.Notes.Count());

            //Select the note for group
            DynamoSelection.Instance.Selection.Add(note);

            //Create a Group around that note
            ViewModel.AddAnnotationCommand.Execute(null);
            var annotation = ViewModel.Model.CurrentWorkspace.Annotations.FirstOrDefault();

            //Check if the group is created
            Assert.IsNotNull(annotation);

            //Clear the selection
            DynamoSelection.Instance.ClearSelection();

            //Select the group
            DynamoSelection.Instance.Selection.Add(annotation);

            //First, create a Note
            ViewModel.AddNoteCommand.Execute(null);
            var secondnote = ViewModel.Model.CurrentWorkspace.Notes.FirstOrDefault();
            Assert.IsNotNull(secondnote);

            //Select the node 
            DynamoSelection.Instance.Selection.Add(secondnote);

            //Check whether group can be created
            Assert.AreEqual(true, ViewModel.CanAddModelsToGroup(null));
        }

        [Test]
        [Category("DynamoUI")]
        public void CanAddToGroupForNoteWhichIsAlreadyInAGroup()
        {
            //First, create a Note
            ViewModel.AddNoteCommand.Execute(null);
            var note = ViewModel.Model.CurrentWorkspace.Notes.FirstOrDefault();
            Assert.IsNotNull(note);

            //verify the note was created
            Assert.AreEqual(1, ViewModel.Model.CurrentWorkspace.Notes.Count());

            //Select the note for group
            DynamoSelection.Instance.Selection.Add(note);

            //Create a Group around that node
            ViewModel.AddAnnotationCommand.Execute(null);
            var annotation = ViewModel.Model.CurrentWorkspace.Annotations.FirstOrDefault();

            //Check if the group is created
            Assert.IsNotNull(annotation);

            //Clear the selection
            DynamoSelection.Instance.ClearSelection();

            //Select the note 
            DynamoSelection.Instance.Selection.Add(note);

            //Check whether group can be created
            Assert.AreEqual(false, ViewModel.CanAddModelsToGroup(null));
        }

        [Test]
        [Category("DynamoUI")]
        public void SelectingTheGroupShouldSelectTheModels()
        {
            //Create a Node
            var addNode = new DSFunction(ViewModel.Model.LibraryServices.GetFunctionDescriptor("+"));
            ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(addNode, false);

            //verify the node was created
            Assert.AreEqual(1, ViewModel.Model.CurrentWorkspace.Nodes.Count());

            //Select the node for group
            DynamoSelection.Instance.Selection.Add(addNode);

            //Create a Group around that node
            ViewModel.AddAnnotationCommand.Execute(null);
            var annotation = ViewModel.Model.CurrentWorkspace.Annotations.FirstOrDefault();

            //Check if the group is created
            Assert.IsNotNull(annotation);

            //Clear the selection
            DynamoSelection.Instance.ClearSelection();
           
            //Selecting the Group should select the models within that group
            var vm = ViewModel.CurrentSpaceViewModel.Annotations.FirstOrDefault();
            Assert.IsNotNull(vm);
            vm.Select();
            Assert.AreEqual(annotation.Nodes.Count(), annotation.Nodes.Count(x => x.IsSelected));
        }

        [Test]
        [Category("DynamoUI")]
        public void DeletingTheGroupShouldDeleteTheGroupAndModels()
        {
            //Create a Node
            var addNode = new DSFunction(ViewModel.Model.LibraryServices.GetFunctionDescriptor("+"));
            ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(addNode, false);

            //verify the node was created
            Assert.AreEqual(1, ViewModel.Model.CurrentWorkspace.Nodes.Count());

            //Select the node for group
            DynamoSelection.Instance.Selection.Add(addNode);

            //Create a Group around that node
            ViewModel.AddAnnotationCommand.Execute(null);
            var annotation = ViewModel.Model.CurrentWorkspace.Annotations.FirstOrDefault();

            //Check if the group is created
            Assert.IsNotNull(annotation);

            //Clear the selection
            DynamoSelection.Instance.ClearSelection();

            //Selecting the Group should select the models within that group 
            var vm = ViewModel.CurrentSpaceViewModel.Annotations.FirstOrDefault();
            Assert.IsNotNull(vm);
            vm.Select();
            Assert.AreEqual(annotation.Nodes.Count(), annotation.Nodes.Count(x => x.IsSelected));

            //Execute the delete command - This should delete the entire group and models
            ViewModel.DeleteCommand.Execute(null);
            Assert.AreEqual(null, ViewModel.Model.CurrentWorkspace.Annotations.FirstOrDefault());
        }

        [Test]
        [Category("DynamoUI")]
        public void UndoDeletingTheGroupShouldBringTheGroupAndModelsBack()
        {
            //Create a Node
            var addNode = new DSFunction(ViewModel.Model.LibraryServices.GetFunctionDescriptor("+"));
            ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(addNode, false);

            //verify the node was created
            Assert.AreEqual(1, ViewModel.Model.CurrentWorkspace.Nodes.Count());

            //Select the node for group
            DynamoSelection.Instance.Selection.Add(addNode);

            //Create a Group around that node
            ViewModel.AddAnnotationCommand.Execute(null);
            var annotation = ViewModel.Model.CurrentWorkspace.Annotations.FirstOrDefault();

            //Check if the group is created
            Assert.IsNotNull(annotation);

            //Clear the selection
            DynamoSelection.Instance.ClearSelection();

            //Selecting the Group should select the models within that group 
            var vm = ViewModel.CurrentSpaceViewModel.Annotations.FirstOrDefault();
            Assert.IsNotNull(vm);
            vm.Select();
            Assert.AreEqual(annotation.Nodes.Count(), annotation.Nodes.Count(x => x.IsSelected));

            //Execute the delete command - This should delete the entire group and models
            ViewModel.DeleteCommand.Execute(null);
            Assert.AreEqual(null, ViewModel.Model.CurrentWorkspace.Annotations.FirstOrDefault());

            //Undo the operation
            ViewModel.CurrentSpace.Undo();
            Assert.AreEqual(1, ViewModel.Model.CurrentWorkspace.Annotations.Count());

            annotation = ViewModel.Model.CurrentWorkspace.Annotations.FirstOrDefault();
            Assert.IsNotNull(annotation);
            Assert.AreEqual(1, annotation.Nodes.Count());

        }

        [Test]
        [Category("DynamoUI")]
        public void TestCrossSelectingGroups()
        {
            //Create a Node
            var addNode = new DSFunction(ViewModel.Model.LibraryServices.GetFunctionDescriptor("+"));
            ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(addNode, false);

            //verify the node was created
            Assert.AreEqual(1, ViewModel.Model.CurrentWorkspace.Nodes.Count());

            //Select the node for group
            DynamoSelection.Instance.Selection.Add(addNode);

            //Create a Group around that node
            ViewModel.AddAnnotationCommand.Execute(null);
            var annotation = ViewModel.Model.CurrentWorkspace.Annotations.FirstOrDefault();

            //Check if the group is created
            Assert.IsNotNull(annotation);

            //Clear the selection
            DynamoSelection.Instance.ClearSelection();

            //Get the Rect for the group
            var rect = annotation.Rect;
            ViewModel.CurrentSpaceViewModel.SelectInRegion(rect,true);

            //Check whether group is selected
            Assert.AreEqual(true, annotation.IsSelected);

            //Check whether the model is selected
            Assert.AreEqual(true,addNode.IsSelected);
        }

        [Test]
        public void UndoMovingNoteInsideGroup()
        {
            OpenModel(@"UI\NoteInGroupTest.dyn");

            var workspaceVm = ViewModel.CurrentSpaceViewModel;

            // check if note is loaded from the file
            var noteVm = workspaceVm.Notes.First();
            Assert.IsNotNull(noteVm);
            var noteX = noteVm.Left;
            var noteY = noteVm.Top;
            var noteCenterX = noteVm.Model.CenterX;
            var noteCenterY = noteVm.Model.CenterY;

            // check if group is loaded from the file
            var groupVm = workspaceVm.Annotations.First();
            Assert.IsNotNull(groupVm);
            var groupX = groupVm.Left;
            var groupY = groupVm.Top;

            // only note should be selected
            DynamoSelection.Instance.Selection.Add(noteVm.Model);

            var point = new Point2D(noteCenterX, noteCenterY);
            var operation = DynamoModel.DragSelectionCommand.Operation.BeginDrag;
            var command = new DynamoModel.DragSelectionCommand(point, operation);

            ViewModel.ExecuteCommand(command);

            operation = DynamoModel.DragSelectionCommand.Operation.EndDrag;
            point.X += 100;
            point.Y += 100;
            command = new DynamoModel.DragSelectionCommand(point, operation);

            ViewModel.ExecuteCommand(command);

            // Check note and annotation are moved
            Assert.AreNotEqual(groupX, groupVm.Left);
            Assert.AreNotEqual(groupY, groupVm.Top);
            Assert.AreNotEqual(noteX, noteVm.Left);
            Assert.AreNotEqual(noteY, noteVm.Top);

            ViewModel.UndoCommand.Execute(null);

            // Check annotation and note are moved back
            var msgPart = " was not moved back";
            Assert.AreEqual(groupX, groupVm.Left, "Group" + msgPart);
            Assert.AreEqual(groupY, groupVm.Top, "Group" + msgPart);
            Assert.AreEqual(noteX, noteVm.Left, "Note" + msgPart);
            Assert.AreEqual(noteY, noteVm.Top, "Note" + msgPart);
        }

        [Test]
        public void TestOpeningMalformedAnnotation()
        {
            OpenModel("core\\MalformedGroup.dyn");
            var ws = ViewModel.Model.CurrentWorkspace;
            // the file contains annotation which has a removed node
            // check if all models are loaded correctly
            Assert.AreEqual(4, ws.Nodes.Count());
            Assert.AreEqual(1, ws.Annotations.Count());
            Assert.AreEqual(4, ws.Annotations.First().Nodes.Count());
        }

        #endregion
    }
}
