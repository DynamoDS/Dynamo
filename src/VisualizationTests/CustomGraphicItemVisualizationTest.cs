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
            Assert.True(nanNode.Infos.Any(x => x.Message.Contains("'NaN' is being cast to 'Double'") && x.State == ElementState.Warning));

            var negInfNode = Model.CurrentWorkspace.Nodes.First(n => n.Name == "NegativeInfinity");
            Assert.AreEqual(ElementState.Warning, negInfNode.State);
            Assert.True(negInfNode.Infos.Any(x => x.Message.Contains("'-∞' is being cast to 'Double'") && x.State == ElementState.Warning));

            var posInfNode = Model.CurrentWorkspace.Nodes.First(n => n.Name == "PositiveInfinity");
            Assert.AreEqual(ElementState.Warning, posInfNode.State);
            Assert.True(posInfNode.Infos.Any(x => x.Message.Contains("'∞' is being cast to 'Double'") && x.State == ElementState.Warning));
        }
    }
}
