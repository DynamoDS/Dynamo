using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dynamo.Graph;
using Dynamo.Graph.Annotations;
using Dynamo.Graph.Nodes;
using NUnit.Framework;
using ProtoCore.AST;
using Revit.Elements;

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

    }
}