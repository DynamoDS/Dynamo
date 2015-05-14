using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Selection;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using NUnit.Framework;
using DynCmd = Dynamo.Models.DynamoModel;

namespace Dynamo.Tests
{
    internal class AnnotationModelTest : DynamoModelTestBase
    {
        [Test]
        [Category("UnitTests")]
        public void CanAddAnnotation()
        {
            //Add a Node
            var model = CurrentDynamoModel;
            var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));
            model.CurrentWorkspace.AddNode(addNode, false);
            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count, 1);

            //Add a Note 
            Guid id = Guid.NewGuid();
            var addNote = model.CurrentWorkspace.AddNote(false, 200, 200, "This is a test note", id);
            Assert.AreEqual(model.CurrentWorkspace.Notes.Count, 1);

            //Select the node and notes
            DynamoSelection.Instance.Selection.Add(addNode);
            DynamoSelection.Instance.Selection.Add(addNote);

            //create the group around selected nodes and notes
            Guid groupid = Guid.NewGuid();
            var annotation = model.CurrentWorkspace.AddAnnotation("This is a test group", groupid);
            Assert.AreEqual(model.CurrentWorkspace.Annotations.Count, 1);
            Assert.AreNotEqual(0, annotation.Width);
        }

        [Test]
        [Category("UnitTests")]
        public void UndoAnnotationText()
        {
            //Add a Node
            var model = CurrentDynamoModel;
            var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));
            model.CurrentWorkspace.AddNode(addNode, false);
            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count, 1);

            //Add a Note 
            Guid id = Guid.NewGuid();
            var addNote = model.CurrentWorkspace.AddNote(false, 200, 200, "This is a test note", id);
            Assert.AreEqual(model.CurrentWorkspace.Notes.Count, 1);

            //Select the node and notes
            DynamoSelection.Instance.Selection.Add(addNode);
            DynamoSelection.Instance.Selection.Add(addNote);

            //create the group around selected nodes and notes
            Guid groupid = Guid.NewGuid();
            var annotation = model.CurrentWorkspace.AddAnnotation("This is a test group", groupid);
            Assert.AreEqual(model.CurrentWorkspace.Annotations.Count, 1);
            Assert.AreNotEqual(0, annotation.Width);

            //Update the Annotation Text
            model.ExecuteCommand(
                    new DynCmd.UpdateModelValueCommand(
                        System.Guid.Empty, annotation.GUID, "TextBlockText",
                        "This is a unit test"));
            Assert.AreEqual("This is a unit test", annotation.AnnotationText);

            //Undo Annotation text
            model.CurrentWorkspace.Undo();
            
            //Title should be changed now.
            Assert.AreEqual("This is a test group", annotation.AnnotationText);
        }

        [Test]
        [Category("UnitTests")]
        public void UndoAModelDeleteShouldGetTheModelInThatGroup()
        {
            //Add a Node
            var model = CurrentDynamoModel;
            var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));
            model.CurrentWorkspace.AddNode(addNode, false);
            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count, 1);

            //Add a Note 
            Guid id = Guid.NewGuid();
            var addNote = model.CurrentWorkspace.AddNote(false, 200, 200, "This is a test note", id);
            Assert.AreEqual(model.CurrentWorkspace.Notes.Count, 1);

            //Select the node and notes
            DynamoSelection.Instance.Selection.Add(addNode);
            DynamoSelection.Instance.Selection.Add(addNote);

            //create the group around selected nodes and notes
            Guid groupid = Guid.NewGuid();
            var annotation = model.CurrentWorkspace.AddAnnotation("This is a test group", groupid);
            Assert.AreEqual(model.CurrentWorkspace.Annotations.Count, 1);
            Assert.AreNotEqual(0, annotation.Width);

            var modelToDelete = new List<ModelBase>();
            modelToDelete.Add(addNode);

            //Delete the model
            model.DeleteModelInternal(modelToDelete);

            //Check for the model count now
            Assert.AreEqual(1, annotation.SelectedModels.Count());

            //Undo the operation
            model.CurrentWorkspace.Undo();

            //Check for the model count now
            Assert.AreEqual(2, annotation.SelectedModels.Count());

        }

        [Test]
        [Category("UnitTests")]
        public void UndoDeleteAllTheModelsShouldBringTheModelsAndGroupBack()
        {
            //Add a Node
            var model = CurrentDynamoModel;
            var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));
            model.CurrentWorkspace.AddNode(addNode, false);
            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count, 1);

            //Add a Note 
            Guid id = Guid.NewGuid();
            var addNote = model.CurrentWorkspace.AddNote(false, 200, 200, "This is a test note", id);
            Assert.AreEqual(model.CurrentWorkspace.Notes.Count, 1);

            //Select the node and notes
            DynamoSelection.Instance.Selection.Add(addNode);
            DynamoSelection.Instance.Selection.Add(addNote);

            //create the group around selected nodes and notes
            Guid groupid = Guid.NewGuid();
            var annotation = model.CurrentWorkspace.AddAnnotation("This is a test group", groupid);
            Assert.AreEqual(model.CurrentWorkspace.Annotations.Count, 1);
            Assert.AreNotEqual(0, annotation.Width);

            var modelsToDelete = new List<ModelBase>();
            modelsToDelete.Add(addNote);
            modelsToDelete.Add(addNode);

            //Delete the models
            model.DeleteModelInternal(modelsToDelete);

            //Group should be deleted
            Assert.AreEqual(null, model.CurrentWorkspace.Annotations.FirstOrDefault());

            //Undo the operation
            model.CurrentWorkspace.Undo();

            //Check for the annotation count 
            Assert.AreEqual(1, model.CurrentWorkspace.Annotations.Count());
           
            //Check for the model count 
            annotation = model.CurrentWorkspace.Annotations.FirstOrDefault();
            Assert.AreNotEqual(null,annotation);            
            Assert.AreEqual(2, annotation.SelectedModels.Count());     
        }

        [Test]
        [Category("UnitTests")]
        public void UngroupAModelDeleteShouldGetTheModelInThatGroup()
        {
            //Add a Node
            var model = CurrentDynamoModel;
            var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));
            model.CurrentWorkspace.AddNode(addNode, false);
            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count, 1);

            //Add a Note 
            Guid id = Guid.NewGuid();
            var addNote = model.CurrentWorkspace.AddNote(false, 200, 200, "This is a test note", id);
            Assert.AreEqual(model.CurrentWorkspace.Notes.Count, 1);

            //Select the node and notes
            DynamoSelection.Instance.Selection.Add(addNode);
            DynamoSelection.Instance.Selection.Add(addNote);

            //create the group around selected nodes and notes
            Guid groupid = Guid.NewGuid();
            var annotation = model.CurrentWorkspace.AddAnnotation("This is a test group", groupid);
            Assert.AreEqual(model.CurrentWorkspace.Annotations.Count, 1);
            Assert.AreNotEqual(0, annotation.Width);

            var modelToUngroup = new List<ModelBase>();
            modelToUngroup.Add(addNode);

            //Delete the model
            model.UngroupModel(modelToUngroup);

            //Check for the model count now
            Assert.AreEqual(1, annotation.SelectedModels.Count());

            //Undo the operation
            model.CurrentWorkspace.Undo();

            //Check for the model count now
            Assert.AreEqual(2, annotation.SelectedModels.Count());

        }

        [Test]
        [Category("UnitTests")]
        public void UngroupAllTheModelsShouldDeleteTheGroup()
        {
            //Add a Node
            var model = CurrentDynamoModel;
            var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));
            model.CurrentWorkspace.AddNode(addNode, false);
            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count, 1);

            //Add a Note 
            Guid id = Guid.NewGuid();
            var addNote = model.CurrentWorkspace.AddNote(false, 200, 200, "This is a test note", id);
            Assert.AreEqual(model.CurrentWorkspace.Notes.Count, 1);

            //Select the node and notes
            DynamoSelection.Instance.Selection.Add(addNode);
            DynamoSelection.Instance.Selection.Add(addNote);

            //create the group around selected nodes and notes
            Guid groupid = Guid.NewGuid();
            var annotation = model.CurrentWorkspace.AddAnnotation("This is a test group", groupid);
            Assert.AreEqual(model.CurrentWorkspace.Annotations.Count, 1);
            Assert.AreNotEqual(0, annotation.Width);

            var modelsToUngroup = new List<ModelBase>();
            modelsToUngroup.Add(addNote);
            modelsToUngroup.Add(addNode);

            //Delete the models
            model.UngroupModel(modelsToUngroup);
           
            //Group should be deleted
            Assert.AreEqual(null, model.CurrentWorkspace.Annotations.FirstOrDefault());           
        }

        [Test]
        [Category("UnitTests")]
        public void UndoUngroupAllTheModelShouldGetTheGroupWithModels()
        {
            //Add a Node
            var model = CurrentDynamoModel;
            var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));
            model.CurrentWorkspace.AddNode(addNode, false);
            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count, 1);

            //Add a Note 
            Guid id = Guid.NewGuid();
            var addNote = model.CurrentWorkspace.AddNote(false, 200, 200, "This is a test note", id);
            Assert.AreEqual(model.CurrentWorkspace.Notes.Count, 1);

            //Select the node and notes
            DynamoSelection.Instance.Selection.Add(addNode);
            DynamoSelection.Instance.Selection.Add(addNote);

            //create the group around selected nodes and notes
            Guid groupid = Guid.NewGuid();
            var annotation = model.CurrentWorkspace.AddAnnotation("This is a test group", groupid);
            Assert.AreEqual(model.CurrentWorkspace.Annotations.Count, 1);
            Assert.AreNotEqual(0, annotation.Width);

            var modelToUngroup = new List<ModelBase>();
            var modelsToUngroup = new List<ModelBase>();
            modelsToUngroup.Add(addNote);
            modelsToUngroup.Add(addNode);

            //Delete the models
            model.UngroupModel(modelsToUngroup);

            //Group should be deleted
            Assert.AreEqual(null, model.CurrentWorkspace.Annotations.FirstOrDefault());
    
            //Undo the Group Deletion
            model.CurrentWorkspace.Undo();

            //This should get the group back
            Assert.AreEqual(1, model.CurrentWorkspace.Annotations.Count());

            //Undo again should get the first model into the group
            model.CurrentWorkspace.Undo();
            annotation = model.CurrentWorkspace.Annotations.FirstOrDefault();
            Assert.AreEqual(2, annotation.SelectedModels.Count());           
        }

        [Test]
        [Category("UnitTests")]
        public void AddModelToAnExistingGroup()
        {
            //Add a Node
            var model = CurrentDynamoModel;
            var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));
            model.CurrentWorkspace.AddNode(addNode, false);
            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count, 1);

            //Add a Note 
            Guid id = Guid.NewGuid();
            var addNote = model.CurrentWorkspace.AddNote(false, 200, 200, "This is a test note", id);
            Assert.AreEqual(model.CurrentWorkspace.Notes.Count, 1);

            //Select the node and notes
            DynamoSelection.Instance.Selection.Add(addNode);
            DynamoSelection.Instance.Selection.Add(addNote);

            //create the group around selected nodes and notes
            Guid groupid = Guid.NewGuid();
            var annotation = model.CurrentWorkspace.AddAnnotation("This is a test group", groupid);
            Assert.AreEqual(model.CurrentWorkspace.Annotations.Count, 1);
            Assert.AreNotEqual(0, annotation.Width);

            //Create Another node
            //Add a Node            
            var secondNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));
            model.CurrentWorkspace.AddNode(secondNode, false);
            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count, 2);

            DynamoSelection.Instance.ClearSelection();

            //Select the group and newly created node
            DynamoSelection.Instance.Selection.Add(annotation);
            DynamoSelection.Instance.Selection.Add(secondNode);

            var modelsToAdd = new List<ModelBase>();
            modelsToAdd.Add(secondNode);
           
            //Add the model to group
            model.AddToGroup(modelsToAdd);

            //Group should have the new node added 
            Assert.AreEqual(3, annotation.SelectedModels.Count());

            DynamoSelection.Instance.ClearSelection();

            //Add a new note
            id = Guid.NewGuid();
            var secondNote = model.CurrentWorkspace.AddNote(false, 200, 200, "This is a test note", id);
            Assert.AreEqual(model.CurrentWorkspace.Notes.Count, 2);

            //Select the group and newly created note
            DynamoSelection.Instance.Selection.Add(annotation);
            DynamoSelection.Instance.Selection.Add(secondNote);

            modelsToAdd.Clear();
            modelsToAdd.Add(secondNote);

            //Add the model to group
            model.AddToGroup(modelsToAdd);

            //Group should have the new note added 
            Assert.AreEqual(4, annotation.SelectedModels.Count());
        }

        [Test]
        [Category("UnitTests")]
        public void TestDefaultColorForAGroup()
        {
            //Add a Node
            var model = CurrentDynamoModel;
            var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));
            model.CurrentWorkspace.AddNode(addNode, false);
            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count, 1);

            //Add a Note 
            Guid id = Guid.NewGuid();
            var addNote = model.CurrentWorkspace.AddNote(false, 200, 200, "This is a test note", id);
            Assert.AreEqual(model.CurrentWorkspace.Notes.Count, 1);

            //Select the node and notes
            DynamoSelection.Instance.Selection.Add(addNode);
            DynamoSelection.Instance.Selection.Add(addNote);

            //create the group around selected nodes and notes
            Guid groupid = Guid.NewGuid();
            var annotation = model.CurrentWorkspace.AddAnnotation("This is a test group", groupid);
            Assert.AreEqual(model.CurrentWorkspace.Annotations.Count, 1);
            Assert.AreNotEqual(0, annotation.Width);

            //Check the default color - it should be green
            Assert.AreEqual(annotation.GroupBackground, annotation.Background);
        }

        [Test]
        [Category("UnitTests")]
        public void ChangeTheBackgroundForAGroup()
        {
            //Add a Node
            var model = CurrentDynamoModel;
            var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));
            model.CurrentWorkspace.AddNode(addNode, false);
            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count, 1);

            //Add a Note 
            Guid id = Guid.NewGuid();
            var addNote = model.CurrentWorkspace.AddNote(false, 200, 200, "This is a test note", id);
            Assert.AreEqual(model.CurrentWorkspace.Notes.Count, 1);

            //Select the node and notes
            DynamoSelection.Instance.Selection.Add(addNode);
            DynamoSelection.Instance.Selection.Add(addNote);

            //create the group around selected nodes and notes
            Guid groupid = Guid.NewGuid();
            var annotation = model.CurrentWorkspace.AddAnnotation("This is a test group", groupid);
            Assert.AreEqual(model.CurrentWorkspace.Annotations.Count, 1);
            Assert.AreNotEqual(0, annotation.Width);

            //Check the default color - it should be green
            Assert.AreEqual(annotation.GroupBackground, annotation.Background);

            //change the color
            annotation.Background = "#ff7bac";

            //Check the  color - it should be ff7bac
            Assert.AreEqual("#ff7bac", annotation.Background);
        }

        [Test]
        [Category("UnitTests")]
        public void CopyPasteTextWithCarriageIntoAGroup()
        {
            //Add a Node
            var model = CurrentDynamoModel;
            var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));
            model.CurrentWorkspace.AddNode(addNode, false);
            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count, 1);

            //Add a Note 
            Guid id = Guid.NewGuid();
            var addNote = model.CurrentWorkspace.AddNote(false, 200, 200, "This is a test note", id);
            Assert.AreEqual(model.CurrentWorkspace.Notes.Count, 1);

            //Select the node and notes
            DynamoSelection.Instance.Selection.Add(addNode);
            DynamoSelection.Instance.Selection.Add(addNote);

            //create the group around selected nodes and notes
            Guid groupid = Guid.NewGuid();
            var annotation = model.CurrentWorkspace.AddAnnotation("This is a test group", groupid);
            Assert.AreEqual(model.CurrentWorkspace.Annotations.Count, 1);
            Assert.AreNotEqual(0, annotation.Width);

            //now change the text
            annotation.AnnotationText = "This is a test\r\n\r\nfor carriage return";
            Assert.AreEqual(annotation.AnnotationText, "This is a test\r\n\r\nfor carriage return");                   
        }

        [Test]
        [Category("UnitTests")]
        public void TestDefaultFontSizeForAGroup()
        {
            //Add a Node
            var model = CurrentDynamoModel;
            var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));
            model.CurrentWorkspace.AddNode(addNode, false);
            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count, 1);

            //Add a Note 
            Guid id = Guid.NewGuid();
            var addNote = model.CurrentWorkspace.AddNote(false, 200, 200, "This is a test note", id);
            Assert.AreEqual(model.CurrentWorkspace.Notes.Count, 1);

            //Select the node and notes
            DynamoSelection.Instance.Selection.Add(addNode);
            DynamoSelection.Instance.Selection.Add(addNote);

            //create the group around selected nodes and notes
            Guid groupid = Guid.NewGuid();
            var annotation = model.CurrentWorkspace.AddAnnotation("This is a test group", groupid);
            Assert.AreEqual(model.CurrentWorkspace.Annotations.Count, 1);
            Assert.AreNotEqual(0, annotation.Width);

            //Check the default font size
            Assert.AreEqual(annotation.FontSize, 14.0);
        }

        [Test]
        [Category("UnitTests")]
        public void ChangeFontSizeForAGroup()
        {
            //Add a Node
            var model = CurrentDynamoModel;
            var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));
            model.CurrentWorkspace.AddNode(addNode, false);
            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count, 1);

            //Add a Note 
            Guid id = Guid.NewGuid();
            var addNote = model.CurrentWorkspace.AddNote(false, 200, 200, "This is a test note", id);
            Assert.AreEqual(model.CurrentWorkspace.Notes.Count, 1);

            //Select the node and notes
            DynamoSelection.Instance.Selection.Add(addNode);
            DynamoSelection.Instance.Selection.Add(addNote);

            //create the group around selected nodes and notes
            Guid groupid = Guid.NewGuid();
            var annotation = model.CurrentWorkspace.AddAnnotation("This is a test group", groupid);
            Assert.AreEqual(model.CurrentWorkspace.Annotations.Count, 1);
            Assert.AreNotEqual(0, annotation.Width);

            //Check the default font size
            Assert.AreEqual(annotation.FontSize, 14.0);

            //Change the font size
            annotation.FontSize = 30;
            Assert.AreEqual(annotation.FontSize, 30.0);
        }
    }
}
