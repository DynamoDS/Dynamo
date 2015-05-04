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

    }
}
