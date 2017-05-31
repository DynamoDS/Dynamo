using System.IO;
using Dynamo.Graph;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;

using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    public class WorkspaceOpeningTests : DynamoViewModelUnitTest
    {
        [Test]
        public void OpeningWorkspaceSetsPosition()
        {
            var ws = OpenWorkspaceFromSampleFile();
            Assert.AreEqual(ws.Name, "Home");
            Assert.AreEqual(ws.X, -3732.93, .1);
            Assert.AreEqual(ws.Y, -827.405442288027 , .1);
        }

        [Test]
        public void OpeningWorkspaceSetsZoom()
        {
            var ws = OpenWorkspaceFromSampleFile();
            var wvm = ViewModel.CurrentSpaceViewModel;
            Assert.AreEqual(wvm.Zoom, 1.3, .1);
        }

        [Test]
        public void ClearingWorkspaceResetsPositionAndZoom()
        {
            var ws = OpenWorkspaceFromSampleFile();
            ws.Clear();
            var wvm = ViewModel.CurrentSpaceViewModel;
            Assert.AreEqual(0, wvm.X);
            Assert.AreEqual(0, wvm.Y);
            Assert.AreEqual(1, wvm.Zoom);
        }

        [Test]
        // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-6384
        public void ManipulatingWorkspaceDoesNotAffectZoom()
        {
            var ws = OpenWorkspaceFromSampleFile();
         
            // Pan 
            ViewModel.PanCommand.Execute("Left");

            // Zoom can't be simulated without the view.
            //ViewModel.ZoomInCommand.Execute(null);

            ws.Clear();

            ws = OpenWorkspaceFromSampleFile();
            var wvm = ViewModel.CurrentSpaceViewModel;
            Assert.AreEqual(wvm.Zoom, 1.3, .1);
        }

        private WorkspaceModel OpenWorkspaceFromSampleFile()
        {
            var examplePath = Path.Combine(SampleDirectory, @"en-US\Basics\Basics_Basic01.dyn");
            ViewModel.Model.OpenFileFromPath(examplePath);
            return ViewModel.Model.CurrentWorkspace;
        }

    }
}
