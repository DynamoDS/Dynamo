using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Models;
using Dynamo.Selection;
using Dynamo.Tests;
using Dynamo.Utilities;
using Dynamo.ViewModels;
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

            //verify if the second node was created
            Assert.AreEqual(2, ViewModel.Model.CurrentWorkspace.Nodes.Count());

            //Select the node 
            DynamoSelection.Instance.Selection.Add(addNode);
          
            //Check whether group can be ungrouped
            Assert.AreEqual(true, ViewModel.CanUngroupModel(null));
        }


        [Test]
        [Category("DynamoUI")]
        public void CanUngroupNodeFromAGroupIfGroupContainsNote()
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

            //create a note.
            ViewModel.AddNoteCommand.Execute(null);
            var note = ViewModel.Model.CurrentWorkspace.Notes.FirstOrDefault();
            Assert.IsNotNull(note);

            //Select the note 
            DynamoSelection.Instance.Selection.Add(note);
            DynamoSelection.Instance.Selection.Add(annotation);

            ViewModel.AddModelsToGroupModelCommand.Execute(null);

            //Clear the selection
            DynamoSelection.Instance.ClearSelection();
            //Select the node 
            DynamoSelection.Instance.Selection.Add(addNode);

            Assert.AreEqual(2, annotation.Nodes.Count());
            //remove it
            Assert.DoesNotThrow(() =>
            {
                ViewModel.UngroupModelCommand.Execute(null);
              
            });
            Assert.AreEqual(1, annotation.Nodes.Count());
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
            vm.SelectAll();
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
            vm.SelectAll();
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
            vm.SelectAll();
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
        public void CheckEmptySelectionListAfterUndos()
        {
            //Add a Node
            var model = GetModel();
            var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));
            model.CurrentWorkspace.AddAndRegisterNode(addNode, false);
            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count(), 1);

            //Add a Note 
            Guid id = Guid.NewGuid();
            var addNote = model.CurrentWorkspace.AddNote(false, 200, 200, "This is a test note", id);
            Assert.AreEqual(model.CurrentWorkspace.Notes.Count(), 1);

            //Select the node and notes
            DynamoSelection.Instance.Selection.Add(addNode);
            DynamoSelection.Instance.Selection.Add(addNote);

            //create the group around selected nodes and notes
            Guid groupid = Guid.NewGuid();
            var annotation = model.CurrentWorkspace.AddAnnotation("This is a test group", groupid);
            Assert.AreEqual(model.CurrentWorkspace.Annotations.Count(), 1);
            Assert.AreNotEqual(0, annotation.Width);
            Assert.AreEqual(string.Empty, annotation.AnnotationText);

            //Update the Annotation Text
            model.ExecuteCommand(
                    new DynamoModel.UpdateModelValueCommand(
                        Guid.Empty, annotation.GUID, "TextBlockText",
                        "This is a unit test"));

            var annotationCenterPoint = new Point2D(addNode.CenterX, addNode.CenterY);

            //Deselects the group
            ViewModel.ExecuteCommand(
                    new DynamoModel.SelectModelCommand(Guid.Empty, Dynamo.Utilities.ModifierKeys.None));

            //Selects the group
            DynamoSelection.Instance.Selection.Add(addNode);
            ViewModel.ExecuteCommand(
             new DynamoModel.DragSelectionCommand(annotationCenterPoint, DynamoModel.DragSelectionCommand.Operation.BeginDrag));

            //Deselects the group
            ViewModel.ExecuteCommand(
                    new DynamoModel.SelectModelCommand(Guid.Empty, Dynamo.Utilities.ModifierKeys.None));

            //Selects the group
            DynamoSelection.Instance.Selection.Add(addNode);
            ViewModel.ExecuteCommand(
              new DynamoModel.DragSelectionCommand(annotationCenterPoint, DynamoModel.DragSelectionCommand.Operation.BeginDrag));

            //Deselects the group
            ViewModel.ExecuteCommand(
                    new DynamoModel.SelectModelCommand(Guid.Empty, Dynamo.Utilities.ModifierKeys.None));

            model.CurrentWorkspace.Undo();
            model.CurrentWorkspace.Undo();

            Assert.IsTrue(DynamoSelection.Instance.Selection.Any(x => x.IsSelected));
            Assert.IsTrue(model.CurrentWorkspace.Nodes.Any(x => x.IsSelected));

            model.CurrentWorkspace.Undo();
            model.CurrentWorkspace.Undo();
            model.CurrentWorkspace.Undo();
            model.CurrentWorkspace.Undo();

            //Deselects the group
            ViewModel.ExecuteCommand(
                 new DynamoModel.SelectModelCommand(Guid.Empty, Dynamo.Utilities.ModifierKeys.None));

            Assert.IsFalse(model.CurrentWorkspace.Nodes.Any(x => x.IsSelected));
            Assert.IsFalse(DynamoSelection.Instance.Selection.Any(x => x.IsSelected));
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

        [Test]
        public void CanAddGroupAndGroupedNodesToSelection()
        {
            OpenModel("core\\AddGroupToSelection.dyn");

            var ws = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace("032c6f2c2867454b856a65293f0c70c2"); ;
            var vm = ViewModel.CurrentSpaceViewModel.Annotations.FirstOrDefault();

            // Count before anything is selected
            var countBefore = DynamoSelection.Instance.Selection.Count;
            Assert.AreEqual(0, countBefore);

            // Select first the node
            ws.Select();

            // Add group and nodes to selection
            vm.AddGroupAndGroupedNodesToSelection();

            var countAfter = DynamoSelection.Instance.Selection.Count;
            Assert.AreEqual(3, countAfter);

        }


        [Test]
        public void CanAddGroupToOtherGroup()
        {
            // Arrange
            var group1Name = "Group1";
            var group2Name = "Group2";

            OpenModel(@"core\annotationViewModelTests\groupsTestFile.dyn");

            //Create 3rd group
            var dummyNode = new DummyNode();
            var command = new DynamoModel.CreateNodeCommand(dummyNode, 0, 0, true, false);
            ViewModel.Model.ExecuteCommand(command);

            ViewModel.ExecuteCommand(
                new DynamoModel.SelectModelCommand(dummyNode.GUID, Keyboard.Modifiers.AsDynamoType()));
            ViewModel.AddAnnotationCommand.Execute(null);

            var group1ViewModel = ViewModel.CurrentSpaceViewModel.Annotations.FirstOrDefault(x => x.AnnotationText == group1Name);
            var group2ViewModel = ViewModel.CurrentSpaceViewModel.Annotations.FirstOrDefault(x => x.AnnotationText == group2Name);
            var group3ViewModel = ViewModel.CurrentSpaceViewModel.Annotations.FirstOrDefault(x => x.AnnotationText != group2Name && x.AnnotationText != group1Name);

            var group1ContentBefore = group1ViewModel.Nodes.ToList();

            // Act
            //Select both groups
            var modelGuids = new List<Guid>
            {
                group1ViewModel.AnnotationModel.GUID,
                group2ViewModel.AnnotationModel.GUID,
                group3ViewModel.AnnotationModel.GUID
            };

            ViewModel.ExecuteCommand(
                new DynamoModel.SelectModelCommand(modelGuids, Keyboard.Modifiers.AsDynamoType()));

            Assert.That(
                DynamoSelection.Instance.Selection.Contains(group1ViewModel.AnnotationModel) &&
                DynamoSelection.Instance.Selection.Contains(group2ViewModel.AnnotationModel) &&
                DynamoSelection.Instance.Selection.Contains(group3ViewModel.AnnotationModel)
            );

            group1ViewModel.AddGroupToGroupCommand.Execute(null);

            // Assert
            Assert.That(group1ContentBefore.Count != group1ViewModel.Nodes.Count());
            Assert.That(group1ViewModel.Nodes.Contains(group2ViewModel.AnnotationModel));
            Assert.That(group1ViewModel.Nodes.Contains(group3ViewModel.AnnotationModel));

            //Assert HasUnsavedChanges is true
            Assert.That(ViewModel.CurrentSpaceViewModel.HasUnsavedChanges);

            ViewModel.CurrentSpace.Undo();

            Assert.That(group1ContentBefore.Count == group1ViewModel.Nodes.Count());
            Assert.That(!group1ViewModel.Nodes.Contains(group2ViewModel.AnnotationModel));
            Assert.That(!group1ViewModel.Nodes.Contains(group3ViewModel.AnnotationModel));

            //Assert HasUnsavedChanges is true
            Assert.AreEqual(true, ViewModel.CurrentSpaceViewModel.HasUnsavedChanges);
        }

        [Test]
        public void CannotAddGroupToOtherGroupIfTheGroupAlreadyBelongsToAGroup()
        {
            // Arrange
            var group1Name = "Group1";
            var group2Name = "Group2";

            OpenModel(@"core\annotationViewModelTests\groupsTestFile.dyn");

            var group1ViewModel = ViewModel.CurrentSpaceViewModel.Annotations.FirstOrDefault(x => x.AnnotationText == group1Name);
            var group2ViewModel = ViewModel.CurrentSpaceViewModel.Annotations.FirstOrDefault(x => x.AnnotationText == group2Name);

            var group1ContentBefore = group1ViewModel.Nodes.ToList();

            // Act
            // Add group2 to group1
            var modelGuids = new List<Guid>
            {
                group1ViewModel.AnnotationModel.GUID,
                group2ViewModel.AnnotationModel.GUID
            };

            ViewModel.ExecuteCommand(
                new DynamoModel.SelectModelCommand(modelGuids, Keyboard.Modifiers.AsDynamoType()));

            group1ViewModel.AddGroupToGroupCommand.Execute(null);

            //Create 3rd group
            var dummyNode = new DummyNode();
            var command = new DynamoModel.CreateNodeCommand(dummyNode, 0, 0, true, false);
            ViewModel.Model.ExecuteCommand(command);

            ViewModel.ExecuteCommand(
                new DynamoModel.SelectModelCommand(dummyNode.GUID, Keyboard.Modifiers.AsDynamoType()));
            ViewModel.AddAnnotationCommand.Execute(null);

            var group3ViewModel = ViewModel.CurrentSpaceViewModel.Annotations
                .FirstOrDefault(x => x.AnnotationText != group2Name && x.AnnotationText != group1Name);

            // now select group1 and group 3 and assert that 
            // group3 cannot execute AddGroupToGroupCommand 
            ViewModel.ExecuteCommand(
                new DynamoModel.SelectModelCommand(
                    new List<Guid>
                    {
                        group3ViewModel.AnnotationModel.GUID,
                        group1ViewModel.AnnotationModel.GUID
                    }, 
                    Keyboard.Modifiers.AsDynamoType()));

            // Assert
            Assert.IsFalse(group3ViewModel.AddGroupToGroupCommand.CanExecute(null));
        }


        [Test]
        public void CollapsingGroupWillCreateInportAndOuportCollections()
        {
            // Arrange
            var groupName = "GroupToCollapse";
            OpenModel(@"core\annotationViewModelTests\groupsTestFile.dyn");

            var group1ViewModel = ViewModel.CurrentSpaceViewModel.Annotations.FirstOrDefault(x => x.AnnotationText == groupName);
            var inPortsBefore = group1ViewModel.InPorts.ToList();
            var outPortsBefore = group1ViewModel.OutPorts.ToList();

            var expectedInPortNames = new List<string>
            {
                "codeBlock1Input",
                "codeBlock2Input",
                "item0",
                "item1",
                "item2"
            };

            var expectedOutPortNames = new List<string>
            {
                string.Empty,
                string.Empty,
                "list"
            };

            // Act
            group1ViewModel.IsExpanded = false;

            // Assert
            Assert.That(inPortsBefore.Count != group1ViewModel.InPorts.Count);
            Assert.That(outPortsBefore.Count != group1ViewModel.OutPorts.Count);
            CollectionAssert.AreEquivalent(expectedInPortNames, group1ViewModel.InPorts.Select(x => x.PortModel.Name));
            CollectionAssert.AreEquivalent(expectedOutPortNames, group1ViewModel.OutPorts.Select(x => x.PortModel.Name));
            Assert.That(group1ViewModel.NodeContentCount == 5);
        }


        [Test]
        public void CollapsingAGroupWillAlsoCollapsAllOfItsNodes()
        {
            // Arrange
            var groupName = "GroupWithGroupedGroup";

            OpenModel(@"core\annotationViewModelTests\groupsTestFile.dyn");
            var groupViewModel = ViewModel.CurrentSpaceViewModel.Annotations.FirstOrDefault(x => x.AnnotationText == groupName);

            var groupNodesCollapsedStatusBefore = groupViewModel.ViewModelBases
                .Select(x => x.IsCollapsed)
                .ToList();

            // Act
            groupViewModel.IsExpanded = false;
            var groupNodesCollapsedStatusAfter = groupViewModel.ViewModelBases.Select(x => x.IsCollapsed);

            // Assert
            CollectionAssert.AreNotEquivalent(groupNodesCollapsedStatusAfter, groupNodesCollapsedStatusBefore);
            Assert.That(groupNodesCollapsedStatusAfter.All(x => x == true));
        }

        /// <summary>
        /// Expands parent group, child group is expanded already, so wire coming out of nested group node should be hidden == false.
        /// </summary>
        [Test]
        public void NestedCollapsedGroupWiresDisplayCorrectlyWhenParentGroupExpands()
        {
            //Arrange
            var outerGroupName = "OuterGroupCollapsed";
            var innerGroupName = "InnerGroupExpanded";

            OpenModel(@"core\annotationViewModelTests\groupsTestFile.dyn");
            var outerGroup = ViewModel.CurrentSpaceViewModel.Annotations.FirstOrDefault(x => x.AnnotationText == outerGroupName);
            var innerGroup = ViewModel.CurrentSpaceViewModel.Annotations.FirstOrDefault(x => x.AnnotationText == innerGroupName);

            var innerGroupWiresBefore = innerGroup.Nodes.OfType<NodeModel>()
              .SelectMany(x => x.OutPorts.SelectMany(c => c.Connectors));

            // Act
            outerGroup.IsExpanded = true;

            var innerGroupWiresAfter = innerGroup.Nodes.OfType<NodeModel>()
                .SelectMany(x => x.OutPorts.SelectMany(c => c.Connectors));

            // Assert
            CollectionAssert.AreEquivalent(innerGroupWiresBefore, innerGroupWiresAfter);
            Assert.That(innerGroupWiresAfter.All(x => !x.IsHidden));
        }

        /// <summary>
        /// Expands parent group, child group is collapsed, but wire coming out of proxy port for nested group should still be hidden == false.
        /// </summary>
        [Test]
        public void NestedExpandedGroupWiresDisplayCorrectlyWhenParentGroupExpands()
        {
            //Arrange
            var outerGroupName = "OuterGroupCollapsed2";
            var innerGroupName = "InnerGroupCollapsed";

            OpenModel(@"core\annotationViewModelTests\groupsTestFile.dyn");
            var outerGroup = ViewModel.CurrentSpaceViewModel.Annotations.FirstOrDefault(x => x.AnnotationText == outerGroupName);
            var innerGroup = ViewModel.CurrentSpaceViewModel.Annotations.FirstOrDefault(x => x.AnnotationText == innerGroupName);

            var innerGroupWiresBefore = innerGroup.Nodes.OfType<NodeModel>()
                .SelectMany(x => x.OutPorts.SelectMany(c => c.Connectors));

            // Act
            outerGroup.IsExpanded = true;

            var innerGroupWiresAfter = innerGroup.Nodes.OfType<NodeModel>()
                .SelectMany(x => x.OutPorts.SelectMany(c => c.Connectors));

            // Assert
            CollectionAssert.AreEquivalent(innerGroupWiresBefore, innerGroupWiresAfter);
            Assert.That(innerGroupWiresAfter.All(x => !x.IsHidden));
        }


        [Test]
        public void ChangingIsExpandedMarksGraphAsModified()
        {
            // Arrange
            //Create a Node
            var addNode = new DSFunction(ViewModel.Model.LibraryServices.GetFunctionDescriptor("+"));
            ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(addNode, false);

            //verify the node was created
            Assert.AreEqual(1, ViewModel.Model.CurrentWorkspace.Nodes.Count());

            //Select the node for group
            DynamoSelection.Instance.Selection.Add(addNode);

            //Create a Group around that node
            ViewModel.AddAnnotationCommand.Execute(null);
            var annotationViewModel = ViewModel.CurrentSpaceViewModel.Annotations.FirstOrDefault();

            // Act
            // Set workspace changes to false
            ViewModel.CurrentSpaceViewModel.HasUnsavedChanges = false;

            // Change annotationViewModel IsExpandedState
            annotationViewModel.IsExpanded = !annotationViewModel.IsExpanded;
            var workspaceStateAfterChangingIsExpandedFirst = ViewModel.CurrentSpaceViewModel.HasUnsavedChanges;

            // Set workspace changes to false
            ViewModel.CurrentSpaceViewModel.HasUnsavedChanges = false;

            // Change annotationViewModel IsExpandedState
            annotationViewModel.IsExpanded = !annotationViewModel.IsExpanded;
            var workspaceStateAfterChangingIsExpandedSecond = ViewModel.CurrentSpaceViewModel.HasUnsavedChanges;

            // Assert
            Assert.IsTrue(workspaceStateAfterChangingIsExpandedFirst);
            Assert.IsTrue(workspaceStateAfterChangingIsExpandedSecond);
            Assert.IsTrue(ViewModel.CurrentSpaceViewModel.HasUnsavedChanges);
        }

        [Test]
        public void AddingNodeToGroupWithNestedGroupsWillAddNodeToParentGroup()
        {
            // Arrange
            var parentGroupName = "GroupWithGroupedGroup";
            var nestedGroupName = "GroupInsideOtherGroup";

            OpenModel(@"core\annotationViewModelTests\groupsTestFile.dyn");
            var parentGroupViewModel = ViewModel.CurrentSpaceViewModel.Annotations.FirstOrDefault(x => x.AnnotationText == parentGroupName);
            var parentGroupModel = ViewModel.Model.CurrentWorkspace.Annotations.FirstOrDefault(x => x.GUID == parentGroupViewModel.AnnotationModel.GUID);

            var nestedGroupModel = ViewModel.Model.CurrentWorkspace.Annotations.FirstOrDefault(x => x.AnnotationText == nestedGroupName);

            // Act
            //Create a Node
            var addNode = new DSFunction(ViewModel.Model.LibraryServices.GetFunctionDescriptor("+"));
            ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(addNode, false);

            //verify the node was created
            Assert.That(ViewModel.Model.CurrentWorkspace.Nodes.Contains(addNode));

            var addNodeViewModel = ViewModel.CurrentSpaceViewModel.Nodes
                .FirstOrDefault(x => x.Id == addNode.GUID);

            DynamoSelection.Instance.Selection.Clear();
            DynamoSelection.Instance.Selection.Add(addNode);
            DynamoSelection.Instance.Selection.Add(parentGroupModel);

            addNodeViewModel.AddToGroupCommand.Execute(null);

            // Assert
            Assert.IsTrue(parentGroupModel.ContainsModel(addNode));
            Assert.IsFalse(nestedGroupModel.ContainsModel(addNode));
        }

        [Test]
        public void ConnectorPinsGetsAddedToTheGroup()
        {
            // Arrange
            OpenModel(@"core\annotationViewModelTests\groupsTestFile.dyn");
            var pinNode1Name = "PinNode1";
            var pinNode2Name = "PinNode2";

            var nodesToGroup = ViewModel.CurrentSpace.Nodes.Where(x => x.Name == pinNode1Name || x.Name == pinNode2Name);

            // Act
            DynamoSelection.Instance.ClearSelection();
            DynamoSelection.Instance.Selection.AddRange(nodesToGroup);

            Guid groupid = Guid.NewGuid();
            var annotation = ViewModel.Model.CurrentWorkspace.AddAnnotation("This is a test group", "Group that contains connector pins", groupid);

            // Assert
            Assert.That(annotation.Nodes.OfType<ConnectorPinModel>().Any());
        }

        [Test]
        public void NodesCantBeAddedToCollapsedGroups()
        {
            // Arrange
            var groupName = "CollapsedGroup";

            OpenModel(@"core\annotationViewModelTests\groupsTestFile.dyn");
            var groupViewModel = ViewModel.CurrentSpaceViewModel.Annotations.FirstOrDefault(x => x.AnnotationText == groupName);
            var groupModel = ViewModel.Model.CurrentWorkspace.Annotations.FirstOrDefault(x => x.GUID == groupViewModel.AnnotationModel.GUID);

            // Act
            //Create a Node
            var addNode = new DSFunction(ViewModel.Model.LibraryServices.GetFunctionDescriptor("+"));
            ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(addNode, false);

            //verify the node was created
            Assert.That(ViewModel.Model.CurrentWorkspace.Nodes.Contains(addNode));

            var addNodeViewModel = ViewModel.CurrentSpaceViewModel.Nodes
                .FirstOrDefault(x => x.Id == addNode.GUID);

            DynamoSelection.Instance.Selection.Clear();
            DynamoSelection.Instance.Selection.Add(addNode);
            DynamoSelection.Instance.Selection.Add(groupModel);

            // Assert
            Assert.IsFalse(addNodeViewModel.AddToGroupCommand.CanExecute(null));
        }

        /// <summary>
        /// Tests that a collapsed group with a node containing a warning will display a warning icon in its header.
        /// </summary>
        [Test]
        [Category("DynamoUI")]
        public void CollapsedGroupDisplaysWarningIfNodeInWarningState()
        {
            // Adding a dummy node to the workspace
            var dummyNode = new DummyNode();
            DynamoModel model = GetModel();
            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(dummyNode, 0, 0, true, true));

            NodeViewModel dummyNodeViewModel = ViewModel.CurrentSpaceViewModel.Nodes
                .FirstOrDefault(x => x.NodeModel.GUID == dummyNode.GUID);
            
            ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(dummyNode, false);

            //verify the node was created
            Assert.AreEqual(1, ViewModel.Model.CurrentWorkspace.Nodes.Count());

            //Select the node for group
            DynamoSelection.Instance.Selection.Add(dummyNode);

            //Create a Group around that node
            ViewModel.AddAnnotationCommand.Execute(null);
            var annotation = ViewModel.Model.CurrentWorkspace.Annotations.FirstOrDefault();
            var annotationViewModel = ViewModel.CurrentSpaceViewModel.Annotations.FirstOrDefault();

            //Check if the group is created
            Assert.IsNotNull(annotation);

            //Clear the selection
            DynamoSelection.Instance.ClearSelection();

            // Collapses the group
            annotationViewModel.IsExpanded = false;

            NodeModel dummyNodeModel = dummyNodeViewModel.NodeModel;

            var topLeft = new Point(dummyNodeViewModel.X, dummyNodeViewModel.Y);
            var botRight = new Point(dummyNodeViewModel.X + dummyNodeModel.Width, dummyNodeViewModel.Y + dummyNodeModel.Height);

            if (dummyNodeViewModel.ErrorBubble == null)
            {
                dummyNodeViewModel.ErrorBubble = new InfoBubbleViewModel(ViewModel);
            }

            InfoBubbleViewModel infoBubbleViewModel = dummyNodeViewModel.ErrorBubble;

            // The collection of messages the node receives
            ObservableCollection<InfoBubbleDataPacket> nodeMessages = infoBubbleViewModel.NodeMessages;
            nodeMessages.Add(new InfoBubbleDataPacket(InfoBubbleViewModel.Style.Warning, topLeft, botRight, "Warning", InfoBubbleViewModel.Direction.Top));

            Assert.AreEqual(ElementState.Warning, annotationViewModel.AnnotationModel.GroupState);
        }

        [Test]
        public void CollapsedGroupsUnhidesContentBeforeBeingUngrouped()
        {
            // Arrange
            var groupName = "CollapsedGroup";

            OpenModel(@"core\annotationViewModelTests\groupsTestFile.dyn");
            var groupViewModel = ViewModel.CurrentSpaceViewModel.Annotations.FirstOrDefault(x => x.AnnotationText == groupName);
            var groupModel = ViewModel.Model.CurrentWorkspace.Annotations.FirstOrDefault(x => x.GUID == groupViewModel.AnnotationModel.GUID);

            var groupIsExpandedBefore = groupViewModel.IsExpanded;
            var collapsedStateBefore = groupViewModel.ViewModelBases
                .Select(x => x.IsCollapsed)
                .ToList();

            // Act
            DynamoSelection.Instance.Selection.Clear();
            DynamoSelection.Instance.Selection.Add(groupModel);

            ViewModel.UngroupAnnotationCommand.Execute(null);

            var collapsedStateAfter = groupViewModel.ViewModelBases.Select(x => x.IsCollapsed);

            // Assert
            Assert.IsFalse(groupIsExpandedBefore);
            Assert.That(collapsedStateBefore.All(x => x is true));
            Assert.That(collapsedStateAfter.All(x => x is false));
        }

        [Test]
        public void SelectAllCommandSelectGroupTest()
        {
            // Arrange
            var groupName = "CollapsedGroup";

            // Graph contains collapsed group as well as nodes, notes, connector pins outside of group
            OpenModel(@"core\annotationViewModelTests\groupsTestFile.dyn");
            var groupViewModel = ViewModel.CurrentSpaceViewModel.Annotations.FirstOrDefault(x => x.AnnotationText == groupName);
            var groupModel = ViewModel.Model.CurrentWorkspace.Annotations.FirstOrDefault(x => x.GUID == groupViewModel.AnnotationModel.GUID);

            // Assert that select all command should include target group
            ViewModel.CurrentSpaceViewModel.SelectAllCommand.Execute(null);
            Assert.IsTrue(DynamoSelection.Instance.Selection.Contains(groupModel));
        }

        [Test]
        public void ShowGroupContentsTest()
        {
            // Arrange
            var groupName = "GroupWithGroupedGroup2";

            // Graph contains collapsed group as well as nodes, notes, connector pins outside of group
            OpenModel(@"core\annotationViewModelTests\groupsTestFile.dyn");
            var groupViewModel = ViewModel.CurrentSpaceViewModel.Annotations.FirstOrDefault(x => x.AnnotationText == groupName);
            groupViewModel.ShowGroupContents();

            // Get target connector
            var connectorViewModel = ViewModel.CurrentSpaceViewModel.Connectors
                    .Where(x => x.ConnectorModel.GUID == new Guid("a3d71f022eb94d9e9e1b1a91cb587989"))
                    .FirstOrDefault();

            Assert.IsTrue(groupViewModel.ViewModelBases.OfType<AnnotationViewModel>().FirstOrDefault().IsCollapsed);
            Assert.IsTrue(connectorViewModel.IsCollapsed);
        }

        [Test]
        public void CanToggleVisibilityOfAllNodesInAGroup()
        {
            // Arrange
            var parentGroupName = "GroupWithGroupedGroup";
            var nestedGroupName = "GroupInsideOtherGroup";

            OpenModel(@"core\annotationViewModelTests\groupsTestFile.dyn");

            var parentGroupNameViewModel = ViewModel.CurrentSpaceViewModel.Annotations.FirstOrDefault(x => x.AnnotationText == parentGroupName);
            var nestedGroupNameViewModel = ViewModel.CurrentSpaceViewModel.Annotations.FirstOrDefault(x => x.AnnotationText == nestedGroupName);

            //No. of nodes in parent group with preview enabled before the toggle.
            var parentGroupContentBefore = parentGroupNameViewModel.Nodes.OfType<NodeModel>().Where(x => x.IsVisible == true).ToList();
            var nestedGroupContentBefore = nestedGroupNameViewModel.Nodes.OfType<NodeModel>().Where(x => x.IsVisible == true).ToList();

            Assert.AreEqual(2, parentGroupContentBefore.Count);
            Assert.AreEqual(2, nestedGroupContentBefore.Count);

            //Assert HasUnsavedChanges is false
            Assert.AreEqual(false, ViewModel.CurrentSpaceViewModel.HasUnsavedChanges);

            // Act
            parentGroupNameViewModel.ToggleIsVisibleGroupCommand.Execute(null);

            // Assert
            // Only the nodes in parent group are affected
            var parentGroupContentAfter = parentGroupNameViewModel.Nodes.OfType<NodeModel>().Where(x => x.IsVisible == true).ToList();
            var nestedGroupContentAfter = nestedGroupNameViewModel.Nodes.OfType<NodeModel>().Where(x => x.IsVisible == true).ToList();
            Assert.AreEqual(0, parentGroupContentAfter.Count);
            Assert.AreEqual(2, nestedGroupContentAfter.Count);

            // Now both the groups are affected
            nestedGroupNameViewModel.ToggleIsVisibleGroupCommand.Execute(null);
            nestedGroupContentAfter = nestedGroupNameViewModel.Nodes.OfType<NodeModel>().Where(x => x.IsVisible == true).ToList();
            Assert.AreEqual(0, nestedGroupContentAfter.Count);

            //Assert HasUnsavedChanges is false
            Assert.AreEqual(true, ViewModel.CurrentSpaceViewModel.HasUnsavedChanges);
        }

        #endregion
    }
}
