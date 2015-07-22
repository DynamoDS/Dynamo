using System;
using System.IO;
using System.Windows.Media.Media3D;
using SystemTestServices;
using Dynamo.Controls;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    public class CameraTests : SystemTestBase
    {
        private Watch3DView BackgroundPreview
        {
            get { return (Watch3DView)View.background_grid.FindName("background_preview"); }
        }

        [Test]
        public void Camera_NoSavedData_ResetsToDefault()
        {
            var openPath = Path.Combine(
                GetTestDirectory(ExecutingDirectory),
                @"core\camera\NoCameraData.dyn");
            OpenDynamoDefinition(openPath);

            Assert.True(CameraHasDefaultOrientation());
        }

        [Test]
        public void Camera_BadSavedData_ResetsToDefault()
        {
            var openPath = Path.Combine(
                GetTestDirectory(ExecutingDirectory),
                @"core\camera\BadCameraData.dyn");
            OpenDynamoDefinition(openPath);
            Assert.True(CameraHasDefaultOrientation());
        }

        [Test]
        public void Camera_GoodSaveData_LoadsCorrectly()
        {
            var openPath = Path.Combine(
                GetTestDirectory(ExecutingDirectory),
                @"core\camera\CameraData.dyn");
            OpenDynamoDefinition(openPath);

            var cam = BackgroundPreview.Camera;

            //<Camera Name="background_preview" eyeX="-9.38327815723004" eyeY="0.297033715592044" eyeZ="-0.189174672105126" 
            //lookX="10.3830314479331" lookY="0.223983767894635" lookZ="0.181236488075402" />

            Assert.AreEqual(cam.Position.X, -9.38327815723004, 1e-6);
            Assert.AreEqual(cam.Position.Y, 0.297033715592044, 1e-6);
            Assert.AreEqual(cam.Position.Z, -0.189174672105126, 1e-6);
            Assert.AreEqual(cam.LookDirection.X, 10.3830314479331, 1e-6);
            Assert.AreEqual(cam.LookDirection.Y, 0.223983767894635, 1e-6);
            Assert.AreEqual(cam.LookDirection.Z, 0.181236488075402, 1e-6);
        }

        [Test]
        public void Camera_WorkspaceCleared_ResetsToDefault()
        {
            string openPath = Path.Combine(
                GetTestDirectory(ExecutingDirectory),
                @"core\camera\NoCameraData.dyn");
            OpenDynamoDefinition(openPath);

            // The view handles the WorkspaceCleared event.

            ViewModel.Model.ClearCurrentWorkspace();
            Assert.True(CameraHasDefaultOrientation());
        }

        [Test]
        public void Camera_GoodSaveData_SavesCorrectly()
        {
            ViewModel.NewHomeWorkspaceCommand.Execute(null);

            var cam = BackgroundPreview.Camera;
            var testPos = new Point3D(5, 0, 0);
            var testLook = new Vector3D(-1, 0, 0);
            cam.Position = testPos;
            cam.LookDirection = testLook;

            var tempFileName = Path.GetTempPath() + ".dyn";
            ViewModel.Model.CurrentWorkspace.SaveAs(tempFileName, ViewModel.Model.EngineController.LiveRunnerRuntimeCore);

            ViewModel.Model.ClearCurrentWorkspace();
            Assert.True(CameraHasDefaultOrientation());

            OpenDynamoDefinition(tempFileName);

            Assert.AreEqual(cam.Position.X, testPos.X, 1e-6);
            Assert.AreEqual(cam.Position.Y, testPos.Y, 1e-6);
            Assert.AreEqual(cam.Position.Z, testPos.Z, 1e-6);
            Assert.AreEqual(cam.LookDirection.X, testLook.X, 1e-6);
            Assert.AreEqual(cam.LookDirection.Y, testLook.Y, 1e-6);
            Assert.AreEqual(cam.LookDirection.Z, testLook.Z, 1e-6);

            File.Delete(tempFileName);
        }

        private bool CameraHasDefaultOrientation()
        {
            return BackgroundPreview.Camera.Position == BackgroundPreview.defaultCameraPosition &&
                BackgroundPreview.Camera.LookDirection == BackgroundPreview.defaultCameraLookDirection &&
                BackgroundPreview.Camera.UpDirection == BackgroundPreview.defaultCameraUpDirection;
        }
    }
}
