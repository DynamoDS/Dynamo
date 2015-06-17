using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SystemTestServices;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Selection;
using Dynamo.Tests;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    class NodePreviewTests : SystemTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            base.GetLibrariesToPreload(libraries);
        }

        private Watch3DView BackgroundPreview
        {
            get
            {
                return (Watch3DView)View.background_grid.FindName("background_preview");
            }
        }

        [Test]
        public void TurnGlobalPreviewOnAndOff()
        {
            var testDirectory = GetTestDirectory(ExecutingDirectory);
            var openPath = Path.Combine(testDirectory, @"core\visualization\PreviewGlobal.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var workspace = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;
            workspace.RunSettings.RunType = RunType.Automatic;

            // Identifiers for all nodes in the workspace.
            var nodeIds = new[]
            {
                Guid.Parse("0750d269-26f3-47d7-bb25-79c762a540ff"), // point0 ( visible )
                Guid.Parse("33b03fa4-50e7-4d08-a5d8-ae7f168c2624"), // point1 ( invisible )
                Guid.Parse("fa226a63-1a31-4f13-8b93-1286dac2c695"), // point2 ( visible )
                Guid.Parse("e7b0f340-a0bc-46b2-a05d-7dd71c72e27c"), // originCodeBlock (visible)
            };

            // Ensure we do have all the nodes we expected here.
            var nodes = nodeIds.Select(workspace.NodeFromWorkspace).ToList();
            Assert.IsFalse(nodes.Any(n => n == null)); // Nothing should be null.

            // Ensure that the initial visibility is correct
            Assert.IsTrue(nodes.Select(n => n.IsVisible).SequenceEqual(new[]
            {
                true, false, true, true
            }));
            // Ensure that visulations match our expectations
            Assert.AreEqual(2, BackgroundPreview.Points.Positions.Count);

            // Now turn off the preview of all the nodes
            ViewModel.CurrentSpaceViewModel.SelectAllCommand.Execute(null);
            ViewModel.CurrentSpaceViewModel.ShowHideAllGeometryPreviewCommand.Execute("false");

            // Check that all node visibility is off and nothing is shown
            Assert.IsTrue(nodes.Select(n => n.IsVisible).SequenceEqual(new[]
            {
                false, false, false, false
            }));
            Assert.Null(BackgroundPreview.Points); // There should be no point left.

            // Now turn on the preview of all the nodes
            ViewModel.CurrentSpaceViewModel.SelectAllCommand.Execute(null);
            ViewModel.CurrentSpaceViewModel.ShowHideAllGeometryPreviewCommand.Execute("true");
            
            Assert.IsTrue(nodes.Select(n => n.IsUpstreamVisible).SequenceEqual(new[]
            {
                true, true, true, true
            }));
            Assert.AreEqual(3, BackgroundPreview.Points.Positions.Count);
        }

        [Test]
        public void SettingGlobalLacingStrategy()
        {
            var testDirectory = GetTestDirectory(ExecutingDirectory);
            var openPath = Path.Combine(testDirectory, @"core\visualization\LacingStrategyGlobal.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var workspace = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;
            workspace.RunSettings.RunType = RunType.Automatic;

            // Identifiers for all nodes in the workspace.
            var nodeIds = new[]
            {
                Guid.Parse("0750d269-26f3-47d7-bb25-79c762a540ff"), // point0
                Guid.Parse("33b03fa4-50e7-4d08-a5d8-ae7f168c2624"), // point1
                Guid.Parse("fa226a63-1a31-4f13-8b93-1286dac2c695"), // point2
                Guid.Parse("e7b0f340-a0bc-46b2-a05d-7dd71c72e27c"), // arrays codeblock [1..5] and [11..12]
            };

            // Ensure we do have all the nodes we expected here.
            var nodes = nodeIds.Select(workspace.NodeFromWorkspace).ToList();
            Assert.IsFalse(nodes.Any(n => n == null)); // Nothing should be null.

            // Ensure that visulations match our expectations
            // Initially, all nodes has shortest lacing strategy
            // 5 points for point0 node, 2 points for point1 and point2 node
            Assert.AreEqual(9, BackgroundPreview.Points.Positions.Count);

            // Modify lacing strategy
            ViewModel.CurrentSpaceViewModel.SelectAllCommand.Execute(null);
            ViewModel.CurrentSpaceViewModel.SetArgumentLacingCommand.Execute(LacingStrategy.Longest.ToString());

            Assert.AreEqual(12, BackgroundPreview.Points.Positions.Count);

            // Modify lacing strategy again
            ViewModel.CurrentSpaceViewModel.SelectAllCommand.Execute(null);
            ViewModel.CurrentSpaceViewModel.SetArgumentLacingCommand.Execute(LacingStrategy.CrossProduct.ToString());

            Assert.AreEqual(17, BackgroundPreview.Points.Positions.Count);

            // Change lacing back to original state
            ViewModel.CurrentSpaceViewModel.SelectAllCommand.Execute(null);
            ViewModel.CurrentSpaceViewModel.SetArgumentLacingCommand.Execute(LacingStrategy.Shortest.ToString());
            Assert.AreEqual(9, BackgroundPreview.Points.Positions.Count);
        }
    }
}
