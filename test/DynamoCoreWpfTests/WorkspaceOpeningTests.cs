using System;
using System.IO;
using Dynamo.Events;
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
            Assert.AreEqual(ws.Name, "Basics_Basic01");
            Assert.AreEqual(ws.X, -3732.93, .1);
            Assert.AreEqual(ws.Y, -827.405442288027 , .1);
        }

        [Test]
        public void VerifyRegisteredHomeWorkspace()
        {
            // This test verifies the HomeWorkspace was registered with the EvaluationCompleted
            // and RefreshCompleted events upon deserialization in both xml and json

            // Load an xml test file
            string dynFilePath = Path.Combine(Dynamo.UnitTestBase.TestDirectory, @"core\serialization\serialization.dyn");
            string testPath = Path.GetFullPath(dynFilePath);
            ViewModel.OpenCommand.Execute(testPath);

            // Verify homeWorkspace is not null
            var homeWorkspace = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;
            Assert.NotNull(homeWorkspace);
            
            // Verify events handlers have been registered
            bool openXmlFired = false;
            EventHandler<EvaluationCompletedEventArgs> testXmlEvent = (sender, e) => { openXmlFired = true; };
            ViewModel.Model.EvaluationCompleted += testXmlEvent;
            RunCurrentModel();
            Assert.IsTrue(openXmlFired);

            //Unsubscribe
            ViewModel.Model.EvaluationCompleted -= testXmlEvent;

            // Save to json in temp location
            string tempPath = Path.Combine(Dynamo.UnitTestBase.TestDirectory, @"core\serialization\serialization_temp.dyn");
            ViewModel.SaveAsCommand.Execute(tempPath);
            
            // Close workspace
            Assert.IsTrue(ViewModel.CloseHomeWorkspaceCommand.CanExecute(null));
            ViewModel.CloseHomeWorkspaceCommand.Execute(null);
            
            // Open json temp file
            testPath = Path.GetFullPath(tempPath);
            ViewModel.OpenCommand.Execute(testPath);

            // Verify homeWorkspace is not null
            homeWorkspace = ViewModel.Model.CurrentWorkspace as HomeWorkspaceModel;
            Assert.NotNull(homeWorkspace);

            // Verify events handlers have been registered
            bool openJsonFired = false;
            EventHandler<EvaluationCompletedEventArgs> testJsonEvent = (sender, e) => { openJsonFired = true; };
            ViewModel.Model.EvaluationCompleted += testJsonEvent;
            RunCurrentModel();
            Assert.IsTrue(openJsonFired);

            // Close workspace
            Assert.IsTrue(ViewModel.CloseHomeWorkspaceCommand.CanExecute(null));
            ViewModel.CloseHomeWorkspaceCommand.Execute(null);

            // Delete temp file
            File.Delete(tempPath);

            //Unsubscribe
            ViewModel.Model.EvaluationCompleted -= testJsonEvent;
        }

        [Test]
        public void OpeningWorkspaceSetsZoom()
        {
            var ws = OpenWorkspaceFromSampleFile();
            var wvm = ViewModel.CurrentSpaceViewModel;
            Assert.AreEqual(wvm.Zoom, 1.3, .1);
        }

        [Test]
        public void OpeningWorkspaceDoNotStartWithUnSavedChanges()
        {
            var ws = OpenWorkspaceFromSampleFile();
            Assert.AreEqual(ws.HasUnsavedChanges, false);
        }

        [Test, Ignore]
        public void OpeningWorkspaceWithManualRunState()
        {
            var ws = (HomeWorkspaceModel)OpenWorkspaceInManualModeFromSampleFile(true);
            Assert.AreEqual(ws.RunSettings.RunType, RunType.Manual);
            // TODO - this should be coming back false see QNTM-2839
            Assert.IsFalse(ws.HasRunWithoutCrash);
        }

        [Test]
        public void OpeningWorkspaceWithAutoRunState()
        {
            var ws = (HomeWorkspaceModel)OpenWorkspaceInManualModeFromSampleFile(false);
            Assert.AreEqual(ws.RunSettings.RunType, RunType.Automatic);
            Assert.IsTrue(ws.HasRunWithoutCrash);
        }

        [Test]
        public void OpeningXMLWorkspaceShouldSetDeterministicId()
        {
            var ws = OpenWorkspaceFromSampleFile();
            Assert.AreEqual(ws.Guid.ToString(), "3c9d0464-8643-5ffe-96e5-ab1769818209");
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
            ViewModel.OpenCommand.Execute(examplePath);
            return ViewModel.Model.CurrentWorkspace;
        }

        private WorkspaceModel OpenWorkspaceInManualModeFromSampleFile(bool forceManualMode)
        {
            // The sample is saved in auto mode, this function opens it in ForceMannual mode
            var examplePath = Path.Combine(SampleDirectory, @"en-US\Basics\Basics_Basic03.dyn");
            var openParams = Tuple.Create(examplePath, forceManualMode);
            // TODO HasRunWithoutCrash comes back true but should be false
            // See QNTM-2839 and OpeningWorksapceWithManualRunState test above
            ViewModel.OpenCommand.Execute(openParams);
            return ViewModel.Model.CurrentWorkspace;
        }
    }
}
