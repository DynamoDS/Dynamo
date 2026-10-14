using System;
using System.Collections.Generic;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using NUnit.Framework;

namespace Dynamo.Tests.ModelsTest
{
    [TestFixture]
    class HomeWorkspacePendingChangesTests : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenGraphEditedInManualModeAfterRunThenHasPendingChangesIsTrue()
        {
            var home = CurrentHomeWorkspace();
            var codeBlock = CreateCodeBlock("x = 1;");
            home.RunSettings.RunType = RunType.Manual;
            BeginRun();
            Assert.IsFalse(home.HasPendingChanges, "Run did not clear the pending changes.");

            UpdateCode(codeBlock, "x = 2;");

            Assert.IsTrue(home.HasPendingChanges);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenManualRunCompletesThenHasPendingChangesIsFalse()
        {
            var home = CurrentHomeWorkspace();
            home.RunSettings.RunType = RunType.Manual;
            CreateCodeBlock("x = 1;");
            Assert.IsTrue(home.HasPendingChanges, "Edit in Manual mode did not set pending changes.");

            BeginRun();

            Assert.IsFalse(home.HasPendingChanges);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenGraphEditedInAutomaticModeThenHasPendingChangesIsFalse()
        {
            var home = CurrentHomeWorkspace();
            Assert.AreEqual(RunType.Automatic, home.RunSettings.RunType);

            CreateCodeBlock("x = 1;");

            Assert.IsFalse(home.HasPendingChanges);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenNodeDeletedInManualModeThenHasPendingChangesIsTrue()
        {
            var home = CurrentHomeWorkspace();
            var codeBlock = CreateCodeBlock("x = 1;");
            home.RunSettings.RunType = RunType.Manual;
            BeginRun();

            CurrentDynamoModel.ExecuteCommand(new DynamoModel.DeleteModelCommand(codeBlock.GUID));

            Assert.IsTrue(home.HasPendingChanges);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenWorkspaceClearedThenHasPendingChangesIsTrue()
        {
            var home = CurrentHomeWorkspace();
            CreateCodeBlock("x = 1;");
            Assert.IsFalse(home.HasPendingChanges, "Automatic run did not clear the pending changes.");

            home.Clear();

            Assert.IsTrue(home.HasPendingChanges);
        }

        private HomeWorkspaceModel CurrentHomeWorkspace()
        {
            return (HomeWorkspaceModel)CurrentDynamoModel.CurrentWorkspace;
        }

        private CodeBlockNodeModel CreateCodeBlock(string code)
        {
            var cbn = new CodeBlockNodeModel(CurrentDynamoModel.LibraryServices);
            CurrentDynamoModel.ExecuteCommand(new DynamoModel.CreateNodeCommand(cbn, 0, 0, true, false));
            UpdateCode(cbn, code);
            return cbn;
        }

        private void UpdateCode(CodeBlockNodeModel cbn, string code)
        {
            CurrentDynamoModel.ExecuteCommand(
                new DynamoModel.UpdateModelValueCommand(Guid.Empty, cbn.GUID, "Code", code));
        }
    }
}
