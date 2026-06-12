using Dynamo.Graph.Workspaces;
using NUnit.Framework;
using System.IO;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    public class FileTrustWarningTests : DynamoTestUIBase
    {
        private string trustedDir;

        [TearDown]
        public void Cleanup()
        {
            if (trustedDir != null)
            {
                ViewModel?.PreferenceSettings?.RemoveTrustedLocation(trustedDir);

                if (Directory.Exists(trustedDir))
                {
                    Directory.Delete(trustedDir, recursive: true);
                }

                trustedDir = null;
            }
        }

        [Test]
        public void WhenTrustedFileOpenedWhileUntrustedWarningActiveThenForceBlockRunIsReset()
        {
            var trustedFileName = "TrustedFileB.dyn";
            var trustedDirName = "trusted";

            // Open any file in the test directory
            var fileAPath = Path.Combine(GetTestDirectory(ExecutingDirectory), @"core\CustomNodes\TestAdd.dyn");
            ViewModel.OpenCommand.Execute(fileAPath);

            var homeWorkspace = ViewModel.HomeSpace as HomeWorkspaceModel;
            Assert.IsNotNull(homeWorkspace);

            // Replicate the state the real untrusted file open would have produced
            // via DynamoModel.ShouldForceBlockRun + UpdateFileTrustWarningUi
            homeWorkspace.RunSettings.ForceBlockRun = true;
            ViewModel.FileTrustViewModel.ShowWarningPopup = true;
            ViewModel.FileTrustViewModel.DynFileDirectoryName = Path.GetDirectoryName(fileAPath);

            // Assert that the precondition is fully armed before simulating the race
            Assert.IsTrue(homeWorkspace.RunSettings.ForceBlockRun);
            Assert.IsTrue(ViewModel.FileTrustViewModel.ShowWarningPopup);
            Assert.IsFalse(string.IsNullOrEmpty(ViewModel.FileTrustViewModel.DynFileDirectoryName));

            // Arrange - place file B in a dedicated trusted subcategory
            trustedDir = Path.Combine(TempFolder, trustedDirName);
            Directory.CreateDirectory(trustedDir);
            var fileBPath = Path.Combine(trustedDir, trustedFileName);
            File.Copy(fileAPath, fileBPath);

            ViewModel.PreferenceSettings.AddTrustedLocation(trustedDir);

            // Assert that the trusted path was registered
            Assert.IsTrue(ViewModel.PreferenceSettings.IsTrustedLocation(trustedDir));

            // Open file B while file A's trust warning is still active
            ViewModel.OpenCommand.Execute(fileBPath);

            // Assert that file B is now the active workspace, ForceBlockRun is reset,
            // the popup for file A is dismissed and torn down
            Assert.AreEqual(trustedFileName, Path.GetFileName(ViewModel.CurrentSpace.FileName));
            Assert.IsFalse(ViewModel.HomeSpace.RunSettings.ForceBlockRun);
            Assert.IsFalse(ViewModel.FileTrustViewModel.ShowWarningPopup);
            Assert.IsTrue(string.IsNullOrEmpty(ViewModel.FileTrustViewModel.DynFileDirectoryName));
        }
    }
}
