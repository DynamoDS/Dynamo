using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Graph;
using Dynamo.Graph.Annotations;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Notes;
using NUnit.Framework;
using DynCmd = Dynamo.Models.DynamoModel;

namespace Dynamo.Tests
{
    /// <summary>
    /// Class containing tests for the AnnotationModel class
    /// </summary>
    [TestFixture]
    public class AnnotationModelTests : DynamoModelTestBase
    {
        private AnnotationModel annotationModel;

        [SetUp]
        public void Init()
        {
            var groupId = Guid.NewGuid();
            annotationModel = CurrentDynamoModel.CurrentWorkspace.AddAnnotation("This is a test group", groupId);
        }

        [Test]
        [Category("UnitTests")]
        public void TextPropertyTest()
        {
            var value = "Test Text Value";
            var initialValue = annotationModel.Text;

            annotationModel.Text = value;

            var result = annotationModel.Text;
            Assert.AreEqual(value, result);
        }

        [Test]
        [Category("UnitTests")]
        public void UpdateValueCoreTest()
        {
            string propName;
            string propValue;
            UpdateValueParams parameters;

            //FontSize property update case
            propName = "FontSize";
            propValue = "15";

            parameters = new UpdateValueParams(propName, propValue);
            annotationModel.UpdateValue(parameters);

            var expectedCase1 = Convert.ToDouble(propValue);
            var resultCase1 = annotationModel.FontSize;

            Assert.AreEqual(expectedCase1, resultCase1);

            //Background property update case
            propName = "Background";
            propValue = "#ff7bac";

            parameters = new UpdateValueParams(propName, propValue);
            annotationModel.UpdateValue(parameters);

            var expectedCase2 = propValue;
            var resultCase2 = annotationModel.Background;

            Assert.AreEqual(expectedCase2, resultCase2);

            //TextBlockText property update case
            propName = "TextBlockText";
            propValue = "Test text";

            parameters = new UpdateValueParams(propName, propValue);
            annotationModel.UpdateValue(parameters);

            var expectedCase3 = propValue;
            var resultCase3 = annotationModel.AnnotationText;

            Assert.AreEqual(expectedCase3, resultCase3);
        }

        [Test]
        [Category("UnitTests")]
        public void AddToSelectedModels_NotInAnnotationArea_ShouldNotAddTheNodeToTheAnnotation()
        {
            NodeModel nodeModel;
            CurrentDynamoModel.NodeFactory.CreateNodeFromTypeName("CoreNodeModels.Input.DoubleInput", out nodeModel);

            annotationModel.AddToTargetAnnotationModel(nodeModel, true);

            Assert.IsFalse(annotationModel.Nodes.Contains(nodeModel));
        }


        [Test]
        public void CanChangeGroupDescriptionText()
        {
            // Arrange
            var newValue = "This is some new description";
            var initialValue = annotationModel.AnnotationDescriptionText;

            // Act
            annotationModel.AnnotationDescriptionText = newValue;

            // Assert
            Assert.That(annotationModel.AnnotationDescriptionText != initialValue);
            Assert.That(annotationModel.AnnotationDescriptionText == newValue);
        }


        [Test]
        public void ModelRectEqualsWidthAndHeight()
        {
            // Arrange
            var node1 = new DummyNode();
            var node2 = new DummyNode();

            // Act
            var node1Posistion = new Utilities.Point2D(node1.Width / 2, node1.Height / 2);
            node1.X = node1Posistion.X;
            node1.Y = node1Posistion.Y;

            var node2Posistion = new Utilities.Point2D(1000 - (node2.Width / 2), 1000 - (node2.Height / 2));
            node2.X = node2Posistion.X;
            node2.Y = node2Posistion.Y;

            annotationModel.AddToTargetAnnotationModel(node1);
            annotationModel.AddToTargetAnnotationModel(node2);

            // Assert
            Assert.That(annotationModel.Width == annotationModel.Rect.Width);
            Assert.That(annotationModel.Height == annotationModel.Rect.Height);
            Assert.That(annotationModel.X == annotationModel.Rect.TopLeft.X);
            Assert.That(annotationModel.Y == annotationModel.Rect.TopLeft.Y);
            Assert.That(annotationModel.ModelAreaHeight == annotationModel.Height - annotationModel.TextBlockHeight);
        }


        [Test]
        public void CanAddAnotherAnnotationModelToAnnotaionModel()
        {
            // Arrange
            var groupedModelsBefore = annotationModel.Nodes;

            // Act
            var nodeCollection = new List<NodeModel>() { new DummyNode() };
            var newGroup = new AnnotationModel(nodeCollection, new List<NoteModel>());

            annotationModel.AddToTargetAnnotationModel(newGroup);

            // Assert
            Assert.That(annotationModel.Nodes.OfType<AnnotationModel>().Any());
            Assert.That(annotationModel.Nodes.Select(x=>x.GUID).Contains(newGroup.GUID));
            Assert.That(annotationModel.Nodes.Count() == groupedModelsBefore.Count() + 1);
        }

        [Test]
        public void UndoAddAnnotationModelsToAnnotaionModel()
        {
            // Make sure the parent group is not empty, otherwise undo in test will not work
            // This does not happen when launching Dynamo with UI because from UI, it is not possible
            // to create an empty annotation yet
            annotationModel.AddToTargetAnnotationModel(new DummyNode());
            // Baseline
            Assert.AreEqual(0, annotationModel.Nodes.OfType<AnnotationModel>().Count());

            // Create two groups as candidates
            var nodeCollection = new List<NodeModel>() { new DummyNode() };
            var nodeCollection2 = new List<NodeModel>() { new DummyNode() };
            var newGroup = new AnnotationModel(nodeCollection, new List<NoteModel>());
            var newGroup2 = new AnnotationModel(nodeCollection2, new List<NoteModel>());
            CurrentDynamoModel.CurrentWorkspace.AddAnnotation(newGroup);
            CurrentDynamoModel.CurrentWorkspace.AddAnnotation(newGroup2);

            CurrentDynamoModel.ExecuteCommand(
                new DynCmd.AddGroupToGroupCommand(
                    new List<Guid>() { newGroup.GUID, newGroup2.GUID },
                    annotationModel.GUID));

            // Assert
            Assert.AreEqual(2, annotationModel.Nodes.OfType<AnnotationModel>().Count());
            Assert.That(CurrentDynamoModel.CurrentWorkspace.CanUndo);

            // Check undo result
            // Undo selecting the host group
            CurrentDynamoModel.CurrentWorkspace.Undo();
            Assert.AreEqual(0, annotationModel.Nodes.OfType<AnnotationModel>().Count());
        }


        [Test]
        public void GroupAdaptsToSizeAdjustments()
        {
            // Arrange
            // we need to add something to the group
            // otherwise it wont trigger an update
            annotationModel.AddToTargetAnnotationModel(new DummyNode());

            var initialWidth = annotationModel.Width;
            var initialHeight = annotationModel.Height;

            var widthAdjustment = 100;
            var heightAdjustment = 100;

            // Act
            annotationModel.IsThumbResizing = true;
            annotationModel.WidthAdjustment = widthAdjustment;
            annotationModel.HeightAdjustment = heightAdjustment;
            annotationModel.IsThumbResizing = false;

            // Assert
            Assert.That(annotationModel.Width == initialWidth + widthAdjustment);
            Assert.That(annotationModel.Height == initialHeight + heightAdjustment);
        }

        [Test]
        [Category("UnitTests")]
        public void GroupDoesNotExpandBeyondUserSetSizeWhenNodeIsInside()
        {
            // Arrange
            // Add the first node to initialize the group
            var firstNode = new DummyNode();
            annotationModel.AddToTargetAnnotationModel(firstNode);

            var originalWidth = annotationModel.Width;
            var originalHeight = annotationModel.Height;
            var originalRect = annotationModel.Rect;

            // Simulate user resizing with thumb
            annotationModel.UserSetWidth = originalWidth + 200;
            annotationModel.UserSetHeight = originalHeight + 200;
            annotationModel.UpdateBoundaryFromSelection();

            var userResizedWidth = annotationModel.Width;
            var userResizedHeight = annotationModel.Height;

            Assert.AreEqual(1, annotationModel.Nodes.Count());
            Assert.IsTrue(userResizedWidth > originalWidth);
            Assert.IsTrue(userResizedHeight > originalHeight);

            // Create a second node that sits within the resized area but outside the original bounds
            var secondNode = new DummyNode { X = 100, Y = 50 };

            // Confirm the node exceeds the original boundary
            Assert.IsTrue(secondNode.Rect.Right > originalRect.Right);
            Assert.IsTrue(secondNode.Rect.Bottom > originalRect.Bottom);

            // Act
            annotationModel.AddToTargetAnnotationModel(secondNode);
            annotationModel.UpdateBoundaryFromSelection();

            // Assert: group size should remain at user-set dimensions
            Assert.AreEqual(2, annotationModel.Nodes.Count());
            Assert.AreEqual(userResizedWidth, annotationModel.Width);
            Assert.AreEqual(userResizedHeight, annotationModel.Height);
        }

        [Test]
        [Category("UnitTests")]
        public void GroupExpandsWhenNodeExceedsUserSetSize_AndShrinksBackOnRemoval()
        {
            // Arrange: Add an initial node to initialize the group
            var firstNode = new DummyNode();
            annotationModel.AddToTargetAnnotationModel(firstNode);

            var initialWidth = annotationModel.Width;
            var initialHeight = annotationModel.Height;

            // Simulate manual resize by the user
            annotationModel.UserSetWidth = initialWidth + 200;
            annotationModel.UserSetHeight = initialHeight + 200;
            annotationModel.UpdateBoundaryFromSelection();

            var resizedWidth = annotationModel.Width;
            var resizedHeight = annotationModel.Height;
            var resizedRect = annotationModel.Rect;

            // Assert: Manual resize increased size
            Assert.Greater(resizedWidth, initialWidth);
            Assert.Greater(resizedHeight, initialHeight);

            // Arrange: Create a second node that is fully outside the resized boundary
            var secondNode = new DummyNode { X = 1000, Y = 500 };

            // Verify that the new node falls outside the resized group bounds
            Assert.Greater(secondNode.Rect.Left, resizedRect.Right);
            Assert.Greater(secondNode.Rect.Top, resizedRect.Bottom);

            // Act: Add the second node to the group
            annotationModel.AddToTargetAnnotationModel(secondNode);
            annotationModel.UpdateBoundaryFromSelection();

            // Assert: Group expanded to accommodate second node
            Assert.AreEqual(2, annotationModel.Nodes.Count());
            Assert.Greater(annotationModel.Width, resizedWidth);
            Assert.Greater(annotationModel.Height, resizedHeight);

            // Act: Remove the second node
            var updatedNodes = annotationModel.Nodes.Where(n => n != secondNode).ToList();
            annotationModel.Nodes = updatedNodes;
            annotationModel.UpdateBoundaryFromSelection();

            // Assert: Group size reverts to user-defined dimensions
            Assert.AreEqual(1, annotationModel.Nodes.Count());
            Assert.AreEqual(resizedWidth, annotationModel.Width);
            Assert.AreEqual(resizedHeight, annotationModel.Height);
        }
    }
}