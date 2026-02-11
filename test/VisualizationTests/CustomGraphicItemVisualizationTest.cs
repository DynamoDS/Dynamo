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
            RunCurrentModel();

            // Only the default scene items should be present
            Assert.AreEqual(3, BackgroundPreviewGeometry.Count());

            // Nodes should be set to active status without any warning message
            var nanNode = Model.CurrentWorkspace.Nodes.First(n => n.Name == "NaN");
            Assert.AreEqual(ElementState.Active, nanNode.State);

            var objtype = Model.CurrentWorkspace.Nodes.First(n => n.Name == "Object.Type");
            Assert.AreEqual(ElementState.Active, objtype.State);
            var result = GetPreviewValue("243b31d9b3f14cc49768689864a57986").ToString();
            Assert.AreEqual("System.Double", result);

            var negInfNode = Model.CurrentWorkspace.Nodes.First(n => n.Name == "NegativeInfinity");
            Assert.AreEqual(ElementState.Active, negInfNode.State);

            var posInfNode = Model.CurrentWorkspace.Nodes.First(n => n.Name == "PositiveInfinity");
            Assert.AreEqual(ElementState.Active, posInfNode.State);
        }
    }
}
