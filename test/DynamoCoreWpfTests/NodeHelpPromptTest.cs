using System.Collections.Generic;
using System.IO;
using Dynamo.Graph.Workspaces;
using Dynamo.Tests;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    class NodeHelpPromptTest : DynamoTestUIBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }
        
        [Test]
        public void ConstructDictionaryLinkFromLibraryTest()
        {
            var openPath = Path.Combine(GetTestDirectory(ExecutingDirectory), @"core\nodehelpprompt\NodeHelpPrompt.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var ws = Model.CurrentWorkspace as HomeWorkspaceModel;
            string link = "";

            var vectorNode = ws.NodeFromWorkspace("e8c9d601-fdd3-400f-81e8-eb206714e250"); // Vector.ByCoordinates (x, y, z, normalized)
            link = vectorNode.ConstructDictionaryLinkFromLibrary(ViewModel.Model.LibraryServices);
            Assert.AreEqual("http://dictionary.dynamobim.com/2/#/Geometry/Vector/Create/ByCoordinates(x_double-y_double-z_double-normalized_bool)", link);

            var cuboidNode = ws.NodeFromWorkspace("d24c6e0e-4d4d-4c9f-885f-f95244b03931"); // Cuboid.ByLengths(cs, width, length, height)
            link = cuboidNode.ConstructDictionaryLinkFromLibrary(ViewModel.Model.LibraryServices);
            Assert.AreEqual("http://dictionary.dynamobim.com/2/#/Geometry/Cuboid/Create/ByLengths(cs_CoordinateSystem-width_double-length_double-height_double)", link);
            
            var hueNode = ws.NodeFromWorkspace("b2d4cff2-1a19-4da7-b22e-34756fe51a5f"); // Color.Hue
            link = hueNode.ConstructDictionaryLinkFromLibrary(ViewModel.Model.LibraryServices);
            Assert.AreEqual("http://dictionary.dynamobim.com/2/#/Core/Color/Action/Hue", link);
        }
    }
}
