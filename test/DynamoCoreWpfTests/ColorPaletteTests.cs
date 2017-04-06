using SystemTestServices;
using CoreNodeModels.Input;
using Dynamo.Graph;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;


using Dynamo.Graph;
using Dynamo.Graph.Nodes;

using ProtoCore.AST.AssociativeAST;
using CoreNodeModels.Properties;

using DSColor = DSCore.Color;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    class ColorPaletteTests : SystemTestBase
    {
        [Test]
        public void ColorPalette_AddToHomespaceAndRun_NoException()
        {
            var homespace = Model.CurrentWorkspace as HomeWorkspaceModel;
            Assert.NotNull(homespace, "The current workspace is not a HomeWorkspaceModel");
            var colorPalette = new ColorPalette();
            Model.AddNodeToCurrentWorkspace(colorPalette, true);
            homespace.Run();           
            Assert.DoesNotThrow(DispatcherUtil.DoEvents);
            Assert.AreEqual(colorPalette.DsColor, DSColor.ByARGB(255,0,0,0));
            Assert.Pass();
        }
    }
}
