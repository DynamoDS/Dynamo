using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using DynamoServices;
using NUnit.Framework;
using ProtoCore;
using DynCmd = Dynamo.Models.DynamoModel;

namespace Dynamo.Tests
{
    /// <summary>
    /// Tests for versioned info messages that are suppressed based on workspace version
    /// </summary>
    [Category("InfoMessage")]
    class VersionedInfoMessageTests : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("FFITarget.dll");
            base.GetLibrariesToPreload(libraries);
        }

        #region Helper Methods

        private CodeBlockNodeModel CreateCodeBlockNode()
        {
            var cbn = new CodeBlockNodeModel(CurrentDynamoModel.LibraryServices);
            var command = new DynCmd.CreateNodeCommand(cbn, 0, 0, true, false);

            CurrentDynamoModel.ExecuteCommand(command);

            Assert.IsNotNull(cbn);
            return cbn;
        }

        private void UpdateCodeBlockNodeContent(CodeBlockNodeModel cbn, string value)
        {
            var command = new DynCmd.UpdateModelValueCommand(Guid.Empty, cbn.GUID, "Code", value);
            CurrentDynamoModel.ExecuteCommand(command);
        }

        #endregion

        /// <summary>
        /// Test: Empty FileName (new workspace) and non-null IntroducedInVersion → suppresses info messages
        /// </summary>
        [Test]
        public void NewWorkspace_WithVersionedInfoMessage_SuppressesMessage()
        {
            // Create a new workspace (empty FileName)
            Assert.IsTrue(string.IsNullOrEmpty(CurrentDynamoModel.CurrentWorkspace.FileName));

            // Create a code block that calls a method with versioned info message
            var cbn = CreateCodeBlockNode();
            UpdateCodeBlockNodeContent(cbn, "FFITarget.TestVersionedInfoMessage.WithVersion();");

            // Run the graph
            BeginRun();

            // Verify that the info message was suppressed (new workspace should suppress versioned messages)
            RuntimeCore runtimeCore = CurrentDynamoModel.EngineController.LiveRunnerRuntimeCore;
            var infos = runtimeCore.RuntimeStatus.Infos.Where(x => x.Message.Contains(FFITarget.TestVersionedInfoMessage.InfoMessageText));
            Assert.AreEqual(0, infos.Count(), "Versioned info messages should be suppressed in new workspaces");
        }

        /// <summary>
        /// Test: Empty FileName (new workspace) and no IntroducedInVersion → log info messages
        /// </summary>
        [Test]
        public void NewWorkspace_WithoutVersionedInfoMessage_LogsMessage()
        {
            // Create a new workspace (empty FileName)
            Assert.IsTrue(string.IsNullOrEmpty(CurrentDynamoModel.CurrentWorkspace.FileName));

            // Create a code block that calls a method without versioned info message
            var cbn = CreateCodeBlockNode();
            UpdateCodeBlockNodeContent(cbn, "FFITarget.TestVersionedInfoMessage.WithoutVersion();");

            // Run the graph
            BeginRun();

            // Verify that the info message was logged
            RuntimeCore runtimeCore = CurrentDynamoModel.EngineController.LiveRunnerRuntimeCore;
            var infos = runtimeCore.RuntimeStatus.Infos.Where(x => x.Message.Contains(FFITarget.TestVersionedInfoMessage.InfoMessageWithoutVersionText));
            Assert.AreEqual(1, infos.Count(), "Info messages without version should always be logged");
        }

        /// <summary>
        /// Test: No IntroducedInVersion → always logs info messages
        /// </summary>
        [Test]
        public void OpenedWorkspace_WithoutVersionedInfoMessage_AlwaysLogsMessage()
        {
            // Open a workspace with version 2.0.0 (older than the message introduction)
            string openPath = Path.Combine(TestDirectory, @"core\versionedinfomessage\workspace_v2.0.0.dyn");
            OpenModel(openPath);

            // Verify workspace version
            Assert.AreEqual(new Version(2, 0, 0, 0), CurrentDynamoModel.EngineController.CurrentWorkspaceVersion);

            // Create a code block that calls a method without versioned info message
            var cbn = CreateCodeBlockNode();
            UpdateCodeBlockNodeContent(cbn, "FFITarget.TestVersionedInfoMessage.WithoutVersion();");

            // Run the graph
            BeginRun();

            // Verify that the info message was logged
            RuntimeCore runtimeCore = CurrentDynamoModel.EngineController.LiveRunnerRuntimeCore;
            var infos = runtimeCore.RuntimeStatus.Infos.Where(x => x.Message.Contains(FFITarget.TestVersionedInfoMessage.InfoMessageWithoutVersionText));
            Assert.AreEqual(1, infos.Count(), "Info messages without version should always be logged");
        }

        /// <summary>
        /// Test: workspaceVersion &lt; introducedInVersion → logs info messages
        /// </summary>
        [Test]
        public void OlderWorkspace_WithVersionedInfoMessage_LogsMessage()
        {
            // Open a workspace with version 2.0.0 (older than the message introduction at 3.0.0)
            string openPath = Path.Combine(TestDirectory, @"core\versionedinfomessage\workspace_v2.0.0.dyn");
            OpenModel(openPath);

            // Verify workspace version
            Assert.AreEqual(new Version(2, 0, 0, 0), CurrentDynamoModel.EngineController.CurrentWorkspaceVersion);

            // Create a code block that calls a method with versioned info message (introduced in 3.0.0)
            var cbn = CreateCodeBlockNode();
            UpdateCodeBlockNodeContent(cbn, "FFITarget.TestVersionedInfoMessage.WithVersion();");

            // Run the graph
            BeginRun();

            // Verify that the info message was logged (workspace is older than introduction version)
            RuntimeCore runtimeCore = CurrentDynamoModel.EngineController.LiveRunnerRuntimeCore;
            var infos = runtimeCore.RuntimeStatus.Infos.Where(x => x.Message.Contains(FFITarget.TestVersionedInfoMessage.InfoMessageText));
            Assert.AreEqual(1, infos.Count(), "Versioned info messages should be logged when workspace version < introduction version");
        }

        /// <summary>
        /// Test: workspaceVersion >= introducedInVersion → suppresses info messages
        /// </summary>
        [Test]
        public void NewerWorkspace_WithVersionedInfoMessage_SuppressesMessage()
        {
            // Open a workspace with version 3.0.0 (same as the message introduction)
            string openPath = Path.Combine(TestDirectory, @"core\versionedinfomessage\workspace_v3.0.0.dyn");
            OpenModel(openPath);

            // Verify workspace version
            Assert.AreEqual(new Version(3, 0, 0, 0), CurrentDynamoModel.EngineController.CurrentWorkspaceVersion);

            // Create a code block that calls a method with versioned info message (introduced in 3.0.0)
            var cbn = CreateCodeBlockNode();
            UpdateCodeBlockNodeContent(cbn, "FFITarget.TestVersionedInfoMessage.WithVersion();");

            // Run the graph
            BeginRun();

            // Verify that the info message was suppressed (workspace version >= introduction version)
            RuntimeCore runtimeCore = CurrentDynamoModel.EngineController.LiveRunnerRuntimeCore;
            var infos = runtimeCore.RuntimeStatus.Infos.Where(x => x.Message.Contains(FFITarget.TestVersionedInfoMessage.InfoMessageText));
            Assert.AreEqual(0, infos.Count(), "Versioned info messages should be suppressed when workspace version >= introduction version");
        }

        /// <summary>
        /// Test: workspaceVersion > introducedInVersion → suppresses info messages
        /// </summary>
        [Test]
        public void EvenNewerWorkspace_WithVersionedInfoMessage_SuppressesMessage()
        {
            // Open a workspace with version 4.0.0 (newer than the message introduction at 3.0.0)
            string openPath = Path.Combine(TestDirectory, @"core\versionedinfomessage\workspace_v4.0.0.dyn");
            OpenModel(openPath);

            // Verify workspace version
            Assert.AreEqual(new Version(4, 0, 0, 0), CurrentDynamoModel.EngineController.CurrentWorkspaceVersion);

            // Create a code block that calls a method with versioned info message (introduced in 3.0.0)
            var cbn = CreateCodeBlockNode();
            UpdateCodeBlockNodeContent(cbn, "FFITarget.TestVersionedInfoMessage.WithVersion();");

            // Run the graph
            BeginRun();

            // Verify that the info message was suppressed (workspace version > introduction version)
            RuntimeCore runtimeCore = CurrentDynamoModel.EngineController.LiveRunnerRuntimeCore;
            var infos = runtimeCore.RuntimeStatus.Infos.Where(x => x.Message.Contains(FFITarget.TestVersionedInfoMessage.InfoMessageText));
            Assert.AreEqual(0, infos.Count(), "Versioned info messages should be suppressed when workspace version > introduction version");
        }
    }
}
