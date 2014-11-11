using System.IO;
using System.Linq;

using Dynamo.Nodes;
using Dynamo.Tests;

using NUnit.Framework;

using RTF.Framework;

namespace RevitSystemTests
{
    [TestFixture]
    class AdaptiveComponentTests : SystemTest
    {
        [Test, Category("Failure")]
        [TestModel(@".\AdaptiveComponent\AdaptiveComponentByFace.rfa")]
        public void AdaptiveComponentByFace()
        {
            var model = ViewModel.Model;

            string testFilePath = Path.Combine(workingDirectory, @".\AdaptiveComponent\AdaptiveComponentByFace.dyn");
            string testPath = Path.GetFullPath(testFilePath);

            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(11, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(12, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());

            // TODO:(Ritesh)Need to add more verification. 
            // Tracking ID http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3983

        }

        [Test]
        [TestModel(@".\AdaptiveComponent\AdaptiveComponentByCurve.rfa")]
        public void AdaptiveComponentByCurve()
        {
            var model = ViewModel.Model;

            string testFilePath = Path.Combine(workingDirectory, @".\AdaptiveComponent\AdaptiveComponentByCurve.dyn");
            string testPath = Path.GetFullPath(testFilePath);

            ViewModel.OpenCommand.Execute(testPath);

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
            var model = ViewModel.Model;

            string testFilePath = Path.Combine(workingDirectory, @".\AdaptiveComponent\AdaptiveComponent.dyn");
            string testPath = Path.GetFullPath(testFilePath);

            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            ViewModel.Model.RunExpression();

            const string adaptiveComponentNodeId = "ac5bd8f9-fcf5-46db-b795-3590044edb56";
            AssertPreviewCount(adaptiveComponentNodeId, 5);

            var acNode = ViewModel.Model.Nodes.FirstOrDefault(x => x.GUID.ToString() == adaptiveComponentNodeId);
            Assert.NotNull(acNode);

            var adaptiveComponent = GetPreviewValueAtIndex(adaptiveComponentNodeId, 3) as Revit.Elements.AdaptiveComponent;
            Assert.IsNotNull(adaptiveComponent);

            // change slider value and re-evaluate graph
            var slider = model.CurrentWorkspace.NodeFromWorkspace
                ("91b7e7ef-9db3-4aa2-8762-6a863188e7ec") as DoubleSlider;
            slider.Value = 3;

            RunCurrentModel();
            AssertPreviewCount(adaptiveComponentNodeId, 3);

        }
    }
}
