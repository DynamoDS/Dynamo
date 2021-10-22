using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Graph;
using Dynamo.Graph.Annotations;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Notes;
using NUnit.Framework;

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

            annotationModel.AddToSelectedModels(nodeModel, true);

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

            annotationModel.AddToSelectedModels(node1);
            annotationModel.AddToSelectedModels(node2);

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

            annotationModel.AddToSelectedModels(newGroup);

            // Assert
            Assert.That(annotationModel.Nodes.OfType<AnnotationModel>().Any());
            Assert.That(annotationModel.Nodes.Select(x=>x.GUID).Contains(newGroup.GUID));
            Assert.That(annotationModel.Nodes.Count() == groupedModelsBefore.Count() + 1);
        }


        [Test]
        public void GroupAdaptsToSizeAdjustments()
        {
            // Arrange
            // we need to add something to the group
            // otherwise it wont trigger an update
            annotationModel.AddToSelectedModels(new DummyNode());

            var initialWidth = annotationModel.Width;
            var initialHeight = annotationModel.Height;

            var widthAdjustment = 100;
            var heightAdjustment = 100;

            // Act
            annotationModel.WidthAdjustment = widthAdjustment;
            annotationModel.HeightAdjustment = heightAdjustment;

            // Assert
            Assert.That(annotationModel.Width == initialWidth + widthAdjustment);
            Assert.That(annotationModel.Height == initialHeight + heightAdjustment);
        }
    }
}