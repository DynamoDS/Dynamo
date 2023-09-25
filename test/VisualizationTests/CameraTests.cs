using System.IO;
using System.Windows.Media.Media3D;
using Dynamo.Controls;
using Dynamo.Wpf.ViewModels.Watch3D;
using NUnit.Framework;

namespace WpfVisualizationTests
{
    [TestFixture]
    public class CameraTests : VisualizationTest
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
        public void Camera_NaNData_ResetsToDefault()
        {
            var openPath = Path.Combine(
                GetTestDirectory(ExecutingDirectory),
                @"core\camera\nanCameraData.dyn");
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
            // This test validates camera loading AND, MAGN-7958. 
            // The camera in the test file is located below the XY plane.

            var openPath = Path.Combine(
                GetTestDirectory(ExecutingDirectory),
                @"core\camera\CameraData.dyn");
            OpenDynamoDefinition(openPath);

            var cam = ((HelixWatch3DViewModel)ViewModel.BackgroundPreviewViewModel).Camera;

            //<Cameras>
            //    <Camera Name="Background 3D Preview" eyeX="-12.3667749293475" eyeY="-10.6603353890713" eyeZ="28.1556199871808" 
            //        lookX="12.0340153661115" lookY="17.1294198216205" lookZ="-27.0288410500148" 
            //        upX="-0.369622383590052" upY="0.417338454481821" upZ="0.830185466001387" />
            //</Cameras>

            Assert.AreEqual(cam.Position.X, -12.3667749293475, 1e-6);
            Assert.AreEqual(cam.Position.Y, -10.6603353890713, 1e-6);
            Assert.AreEqual(cam.Position.Z, 28.1556199871808, 1e-6);
            Assert.AreEqual(cam.LookDirection.X, 12.0340153661115, 1e-6);
            Assert.AreEqual(cam.LookDirection.Y, 17.1294198216205, 1e-6);
            Assert.AreEqual(cam.LookDirection.Z, -27.0288410500148, 1e-6);
            Assert.AreEqual(cam.UpDirection.X, -0.369622383590052, 1e-6);
            Assert.AreEqual(cam.UpDirection.Y, 0.417338454481821, 1e-6);
            Assert.AreEqual(cam.UpDirection.Z, 0.830185466001387, 1e-6);
        }

        [Test]
        public void Camera_GoodSaveData_JSONLoadsCorrectly()
        {
            // This test validates camera loading from JSON matches XML. 
            // The camera position should match the Camera_GoodSaveData_LoadsCorrectly test above

            var openPath = Path.Combine(
                GetTestDirectory(ExecutingDirectory),
                @"core\camera\CameraData.dyn");
            OpenDynamoDefinition(openPath);
            var tempFileName = Path.GetTempPath() + "CameraDataJson.dyn";
            ViewModel.SaveAs(tempFileName);
            OpenDynamoDefinition(tempFileName);

            var cam = ((HelixWatch3DViewModel)ViewModel.BackgroundPreviewViewModel).Camera;

            Assert.AreEqual(cam.Position.X, -12.3667749293475, 1e-6);
            Assert.AreEqual(cam.Position.Y, -10.6603353890713, 1e-6);
            Assert.AreEqual(cam.Position.Z, 28.1556199871808, 1e-6);
            Assert.AreEqual(cam.LookDirection.X, 12.0340153661115, 1e-6);
            Assert.AreEqual(cam.LookDirection.Y, 17.1294198216205, 1e-6);
            Assert.AreEqual(cam.LookDirection.Z, -27.0288410500148, 1e-6);
            Assert.AreEqual(cam.UpDirection.X, -0.369622383590052, 1e-6);
            Assert.AreEqual(cam.UpDirection.Y, 0.417338454481821, 1e-6);
            Assert.AreEqual(cam.UpDirection.Z, 0.830185466001387, 1e-6);

            File.Delete(tempFileName);
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

            var cam = ((HelixWatch3DViewModel)ViewModel.BackgroundPreviewViewModel).Camera;
            var testPos = new Point3D(5, 0, 0);
            var testLook = new Vector3D(-1, 0, 0);
            cam.Position = testPos;
            cam.LookDirection = testLook;

            var tempFileName = Path.GetTempPath() + ".dyn";
            ViewModel.SaveAs(tempFileName);

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
            var defaultData = new CameraData();
            var cam = ((HelixWatch3DViewModel)ViewModel.BackgroundPreviewViewModel).Camera;
            return cam.Position == defaultData.EyePosition &&
                cam.LookDirection == defaultData.LookDirection &&
                cam.UpDirection == defaultData.UpDirection;
        }
    }
}
