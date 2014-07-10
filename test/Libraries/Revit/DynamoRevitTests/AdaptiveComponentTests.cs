using System.IO;
using Dynamo.Nodes;
using Dynamo.Utilities;
using NUnit.Framework;
using Revit.Elements;
using RTF.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class AdaptiveComponentTests:DynamoRevitUnitTestBase
    {
        [Test]
        [TestModel(@".\AdaptiveComponent\AdaptiveComponentByFace.rfa")]
        public void AdaptiveComponentByFace()
        {
            var model = dynSettings.Controller.DynamoModel;

            string testFilePath = Path.Combine(_testPath, @".\AdaptiveComponent\AdaptiveComponentByFace.dyn");
            string testPath = Path.GetFullPath(testFilePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(9, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(10, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());
        }

        [Test]
        [TestModel(@".\AdaptiveComponent\AdaptiveComponentByCurve.rfa")]
        public void AdaptiveComponentByCurve()
        {
            var model = dynSettings.Controller.DynamoModel;

            string testFilePath = Path.Combine(_testPath, @".\AdaptiveComponent\AdaptiveComponentByCurve.dyn");
            string testPath = Path.GetFullPath(testFilePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(6, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(5, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();

            // Check for number of Family Instance Creation
            var allElements = "e83c14bb-864f-4730-900f-0905dac6dcad";
            AssertPreviewCount(allElements, 10);

        }

        [Test]
        [TestModel(@".\AdaptiveComponent\AdaptiveComponent.rfa")]
        public void AdaptiveComponent()
        {
            var model = dynSettings.Controller.DynamoModel;

            string testFilePath = Path.Combine(_testPath, @".\AdaptiveComponent\AdaptiveComponent.dyn");
            string testPath = Path.GetFullPath(testFilePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(18, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(19, model.CurrentWorkspace.Connectors.Count);

            dynSettings.Controller.RunExpression();

            var refPtNodeId = "ac5bd8f9-fcf5-46db-b795-3590044edb56";
            AssertPreviewCount(refPtNodeId, 5);

            var refPt = GetPreviewValueAtIndex(refPtNodeId, 3) as Family;
            Assert.IsNotNull(refPt);

            // change slider value and re-evaluate graph
            DoubleSlider slider = model.CurrentWorkspace.NodeFromWorkspace
                ("91b7e7ef-9db3-4aa2-8762-6a863188e7ec") as DoubleSlider;
            slider.Value = 3;

            RunCurrentModel();
            AssertPreviewCount(refPtNodeId, 3);

        }
    }
}
