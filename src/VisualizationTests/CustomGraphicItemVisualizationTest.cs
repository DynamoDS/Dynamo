using System.Collections.Generic;
using System.Linq;
using Dynamo.Graph.Nodes;
using NUnit.Framework;

namespace WpfVisualizationTests
{
    [TestFixture]
    public class CustomGraphicItemVisualizationTest : VisualizationTest
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            base.GetLibrariesToPreload(libraries);
            libraries.Add("FFITarget.dll");
        }

        [Test]
        public void TestInvalidGeometryIsFilteredOut()
        {
            OpenVisualizationTest("NaN.dyn");

            // Only the default scene items should be present
            Assert.AreEqual(3, BackgroundPreviewGeometry.Count());

            // Nodes should be set to warning status with an appropriate message
            var nanNode = Model.CurrentWorkspace.Nodes.First(n => n.Name == "NaN");
            Assert.AreEqual(ElementState.Warning, nanNode.State);
            StringAssert.Contains("'NaN' is being cast to 'Double'", nanNode.ToolTipText);
            var negInfNode = Model.CurrentWorkspace.Nodes.First(n => n.Name == "NegativeInfinity");
            Assert.AreEqual(ElementState.Warning, negInfNode.State);
            StringAssert.Contains("'-∞' is being cast to 'Double'", negInfNode.ToolTipText);
            var posInfNode = Model.CurrentWorkspace.Nodes.First(n => n.Name == "PositiveInfinity");
            Assert.AreEqual(ElementState.Warning, posInfNode.State);
            StringAssert.Contains("'∞' is being cast to 'Double'", posInfNode.ToolTipText);
        }
    }
}
