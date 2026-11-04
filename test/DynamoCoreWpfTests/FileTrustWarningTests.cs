using System.IO;
using System.Linq;
using System.Windows.Media;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.ViewModels;
using Dynamo.Wpf.ViewModels.FileTrust;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

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

        /// <summary>
        /// Copies a known graph into an untrusted temp folder so the trust gate applies to it.
        /// </summary>
        private string CopyGraphToUntrustedFolder()
        {
            trustedDir = Path.Combine(TempFolder, "untrusted");
            Directory.CreateDirectory(trustedDir);
            var source = Path.Combine(GetTestDirectory(ExecutingDirectory), @"core\CustomNodes\TestAdd.dyn");
            var target = Path.Combine(trustedDir, "UntrustedGraph.dyn");
            File.Copy(source, target, overwrite: true);
            Assert.IsFalse(ViewModel.PreferenceSettings.IsTrustedLocation(trustedDir));
            return target;
        }

        [Test]
        public void WhenUntrustedFileDeclinedInPreviewPromptThenGraphIsNotLoaded()
        {
            var path = CopyGraphToUntrustedFolder();
            string promptedPath = null;
            ViewModel.TrustedLocationPrompt = p =>
            {
                promptedPath = p;
                return (DynamoViewModel.TrustedLocationPromptResult.Declined, false);
            };

            DynamoModel.IsTestMode = false;
            try
            {
                ViewModel.OpenCommand.Execute(path);
            }
            finally
            {
                DynamoModel.IsTestMode = true;
            }

            Assert.AreEqual(path, promptedPath);
            Assert.AreNotEqual(path, ViewModel.HomeSpace.FileName);
            Assert.IsEmpty(ViewModel.HomeSpace.Nodes);
            Assert.IsFalse(ViewModel.FileTrustViewModel.ShowWarningPopup);
        }

        [Test]
        public void WhenUntrustedFileAcceptedInPreviewPromptThenGraphLoadsWithoutRunBlock()
        {
            var path = CopyGraphToUntrustedFolder();
            ViewModel.TrustedLocationPrompt = _ => (DynamoViewModel.TrustedLocationPromptResult.Accepted, false);

            DynamoModel.IsTestMode = false;
            try
            {
                ViewModel.OpenCommand.Execute(path);
            }
            finally
            {
                DynamoModel.IsTestMode = true;
            }

            Assert.AreEqual(path, ViewModel.HomeSpace.FileName);
            Assert.IsNotEmpty(ViewModel.HomeSpace.Nodes);
            Assert.IsFalse(ViewModel.HomeSpace.RunSettings.ForceBlockRun);
            Assert.IsFalse(ViewModel.FileTrustViewModel.ShowWarningPopup);
            Assert.IsTrue(ViewModel.FileTrustViewModel.AllowOneTimeTrust);
            Assert.IsFalse(ViewModel.PreferenceSettings.IsTrustedLocation(trustedDir));
        }

        [Test]
        public void WhenUntrustedFileAcceptedWithTrustLocationThenLocationIsTrusted()
        {
            var path = CopyGraphToUntrustedFolder();
            ViewModel.TrustedLocationPrompt = _ => (DynamoViewModel.TrustedLocationPromptResult.Accepted, true);

            DynamoModel.IsTestMode = false;
            try
            {
                ViewModel.OpenCommand.Execute(path);
            }
            finally
            {
                DynamoModel.IsTestMode = true;
            }

            Assert.IsTrue(ViewModel.PreferenceSettings.IsTrustedLocation(trustedDir));
            Assert.AreEqual(path, ViewModel.HomeSpace.FileName);
        }

        [Test]
        public void WhenUntrustedFileDeclinedFromOpenRecentThenUserIsNotAskedToSave()
        {
            var path = CopyGraphToUntrustedFolder();
            ViewModel.TrustedLocationPrompt = _ => (DynamoViewModel.TrustedLocationPromptResult.Declined, false);
            ViewModel.HomeSpace.HasUnsavedChanges = true;
            var saveRequests = 0;
            WorkspaceSaveEventHandler handler = (o, e) => saveRequests++;
            ViewModel.RequestUserSaveWorkflow += handler;

            DynamoModel.IsTestMode = false;
            try
            {
                ViewModel.OpenRecentCommand.Execute(path);
            }
            finally
            {
                DynamoModel.IsTestMode = true;
                ViewModel.RequestUserSaveWorkflow -= handler;
            }

            Assert.AreEqual(0, saveRequests);
            Assert.IsTrue(ViewModel.HomeSpace.HasUnsavedChanges);
        }

        [TestCase("#FFC1D676", 0xC1, 0xD6, 0x76)]
        [TestCase("#A4E1FF", 0xA4, 0xE1, 0xFF)]
        public void WhenGroupColorIsHexThenPreviewUsesIt(string color, int red, int green, int blue)
        {
            var brush = (SolidColorBrush)GraphPreviewRenderer.ParseGroupBrush(color);

            Assert.AreEqual(Color.FromArgb(0x80, (byte)red, (byte)green, (byte)blue), brush.Color);
        }

        [TestCase("ContextColor file://server/share/profile.icc 1,0,0,0")]
        [TestCase("sc#1,0,0,0")]
        [TestCase("Red")]
        [TestCase("#GGGGGG")]
        [TestCase("#12345")]
        [TestCase("")]
        [TestCase(null)]
        public void WhenGroupColorIsNotLiteralHexThenPreviewUsesDefault(string color)
        {
            var fallback = (SolidColorBrush)GraphPreviewRenderer.ParseGroupBrush(null);

            var brush = (SolidColorBrush)GraphPreviewRenderer.ParseGroupBrush(color);

            Assert.AreSame(fallback, brush);
        }

        [Test]
        public void WhenDynJsonReadForPreviewThenLayoutIsReadWithoutLoadingNodes()
        {
            var json = JObject.Parse(@"{
                ""Nodes"": [
                    { ""Id"": ""a"", ""Inputs"": [], ""Outputs"": [ { ""Id"": ""a-out"" } ] },
                    { ""Id"": ""b"", ""Inputs"": [ { ""Id"": ""b-in0"" }, { ""Id"": ""b-in1"" } ], ""Outputs"": [] }
                ],
                ""Connectors"": [ { ""Start"": ""a-out"", ""End"": ""b-in1"" } ],
                ""View"": {
                    ""NodeViews"": [
                        { ""Id"": ""a"", ""Name"": ""Number"", ""X"": 10.0, ""Y"": 20.0 },
                        { ""Id"": ""b"", ""Name"": ""Add"", ""X"": 300.0, ""Y"": 40.0 }
                    ],
                    ""Annotations"": [
                        { ""Title"": ""A group"", ""Nodes"": [ ""a"", ""b"" ], ""Left"": 0.0, ""Top"": 0.0, ""Width"": 500.0, ""Height"": 200.0, ""Background"": ""#FFC1D676"" },
                        { ""Title"": ""A note"", ""Nodes"": [], ""Left"": 50.0, ""Top"": 250.0, ""Width"": 0.0, ""Height"": 0.0 }
                    ]
                }
            }");

            var preview = GraphPreview.FromJson(json);

            Assert.AreEqual(2, preview.Nodes.Count);
            Assert.AreEqual("Add", preview.Nodes[1].Name);
            Assert.AreEqual(2, preview.Nodes[1].InputCount);
            Assert.AreEqual(1, preview.Connectors.Count);
            Assert.AreEqual(0, preview.Connectors[0].StartNode);
            Assert.AreEqual(1, preview.Connectors[0].EndNode);
            Assert.AreEqual(1, preview.Connectors[0].EndPortIndex);
            Assert.AreEqual("A group", preview.Groups.Single().Title);
            Assert.AreEqual("A note", preview.Notes.Single().Text);
            Assert.IsNotNull(GraphPreviewRenderer.Render(preview, 1.0));
        }
    }
}
