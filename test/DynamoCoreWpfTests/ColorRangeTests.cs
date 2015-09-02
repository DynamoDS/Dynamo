using SystemTestServices;
using DSCoreNodesUI;
using Dynamo.Models;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    class ColorRangeTests : SystemTestBase
    {
        [Test]
        public void ColorRange_AddToHomespaceAndRun_NoException()
        {
            var homespace = Model.CurrentWorkspace as HomeWorkspaceModel;
            Assert.NotNull(homespace, "The current workspace is not a HomeWorkspaceModel");
            var colorRange = new ColorRange();
            Model.AddNodeToCurrentWorkspace(colorRange, true);
            homespace.Run();
            Assert.DoesNotThrow(DispatcherUtil.DoEvents);
            Assert.Pass();
        }
    }
}
