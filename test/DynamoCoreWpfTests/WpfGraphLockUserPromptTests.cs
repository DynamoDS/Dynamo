using Dynamo.Graph.Workspaces.Locking;
using Dynamo.Wpf.Services;
using Dynamo.Wpf.UI;
using NUnit.Framework;
using System;
using System.IO;
using System.Windows.Forms;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    [Category("UnitTests")]
    public class WpfGraphLockUserPromptTests
    {
        private string tempDir;

        [SetUp]
        public void SetUp()
        {
            tempDir = Path.Combine(Path.GetTempPath(), System.Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }

        private string FilePath(string name) => Path.Combine(tempDir, name);

        private WpfGraphLockUserPrompt CreatePrompt(
            IFileSaver dialog,
            Func<string, string, MessageBoxButtons, MessageBoxIcon, DialogResult> msgBox = null)
        {
            return new WpfGraphLockUserPrompt(
                ownerProvider: () => null,
                productNameProvider: "Dynamo",
                dialogFactory: () => dialog,
                showMessageBox: msgBox ?? ((_, __, ___, ____) => DialogResult.OK));
        }

        [Test]
        public void WhenUserPicksLockedSourceFileThenSaveIsBlocked()
        {
            // Arrange
            var graphPath = FilePath("locked.dyn");
            File.WriteAllText(graphPath, "graph");
            var dialog = new MockFileSaver { FileToSelect = graphPath };
            var prompt = CreatePrompt(dialog);

            // Act
            var result = prompt.ShowSaveAsDialog(graphPath);

            // Assert
            Assert.IsNull(result, "Picking the locked source file must cancel the save");
        }

        [Test]
        public void WhenUserPicksLockedSourceFileWithDifferentCasingThenSaveIsBlocked()
        {
            // Arrange - validates the Path.GetFullPath normalisation fix
            var graphPath = FilePath("MyGraph.dyn");
            File.WriteAllText(graphPath, "graph");
            var dialog = new MockFileSaver { FileToSelect = graphPath.ToUpperInvariant() };
            var prompt = CreatePrompt(dialog);

            // Act
            var result = prompt.ShowSaveAsDialog(graphPath);

            // Assert
            Assert.IsNull(result, "Case-insensitive path match must still block the save");
        }

        [Test]
        public void WhenUserPicksNewNonExistingFileThenSaveProceeds()
        {
            // Arrange
            var graphPath = FilePath("source.dyn");
            var savePath = FilePath("new-copy.dyn");
            File.WriteAllText(graphPath, "graph");
            // savePath intentionally does not exist
            var dialog = new MockFileSaver { FileToSelect = savePath };
            var prompt = CreatePrompt(dialog);

            // Act
            var result = prompt.ShowSaveAsDialog(graphPath);

            // Assert
            Assert.AreEqual(savePath, result, "Picking a new path should proceed without any prompt");
        }

        [Test]
        public void WhenUserPicksExistingFileAndConfirmsThenSaveProceeds()
        {
            // Arrange
            var graphPath = FilePath("source.dyn");
            var existingPath = FilePath("existing.dyn");
            File.WriteAllText(graphPath, "graph");
            File.WriteAllText(existingPath, "other graph");

            var dialog = new MockFileSaver { FileToSelect = existingPath };
            var prompt = CreatePrompt(dialog, (_, __, ___, ____) => DialogResult.Yes);

            // Act
            var result = prompt.ShowSaveAsDialog(graphPath);

            // Assert
            Assert.AreEqual(existingPath, result, "Confirming overwrite should allow the save");
        }

        [Test]
        public void WhenUserPicksExistingFileAndDeclinesOverwriteThenSaveIsBlocked()
        {
            // Arrange
            var graphPath = FilePath("source.dyn");
            var existingPath = FilePath("existing.dyn");
            File.WriteAllText(graphPath, "graph");
            File.WriteAllText(existingPath, "other graph");

            var dialog = new MockFileSaver { FileToSelect = existingPath };
            var prompt = CreatePrompt(dialog, (_, __, ___, ____) => DialogResult.No);

            // Act
            var result = prompt.ShowSaveAsDialog(graphPath);

            // Assert
            Assert.IsNull(result, "Declining the overwrite confirmation must cancel the save");
        }

        [Test]
        public void WhenUserCancelsDialogThenSaveIsBlocked()
        {
            // Arrange - user presses Cancel without picking any file
            var graphPath = FilePath("source.dyn");
            File.WriteAllText(graphPath, "graph");
            var dialog = new MockFileSaver { SimulatedShowDialogResult = false };
            var prompt = CreatePrompt(dialog);

            // Act
            var result = prompt.ShowSaveAsDialog(graphPath);

            // Assert
            Assert.IsNull(result, "Cancelling the dialog should return null");
        }

        private sealed class MockFileSaver : IFileSaver
        {
            public string Filter { get; set; }
            public string DefaultExt { get; set; }
            public string FileName { get; set; }
            public bool AddExtension { get; set; }
            public string InitialDirectory { get; set; }
            public bool OverwritePrompt { get; set; }

            internal string FileToSelect { get; set; }
            internal bool? SimulatedShowDialogResult { get; set; } = true;

            private System.ComponentModel.CancelEventHandler fileOkHandler;
            public event System.ComponentModel.CancelEventHandler FileOk
            {
                add => fileOkHandler += value;
                remove => fileOkHandler -= value;
            }

            public bool? ShowDialog()
            {
                if (FileToSelect != null)
                    FileName = FileToSelect;

                var args = new System.ComponentModel.CancelEventArgs();
                fileOkHandler?.Invoke(this, args);

                return args.Cancel ? false : SimulatedShowDialogResult;
            }
        }
    }
}
