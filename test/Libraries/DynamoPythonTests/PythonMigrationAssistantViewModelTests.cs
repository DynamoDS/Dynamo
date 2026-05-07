using System;
using System.IO;
using Dynamo;
using Dynamo.Graph.Workspaces;
using Dynamo.PythonMigration.Differ;
using Dynamo.PythonMigration.MigrationAssistant;
using NUnit.Framework;
using PythonNodeModels;

namespace DynamoPythonTests
{
    [TestFixture]
    public class PythonMigrationAssistantViewModelTests : DynamoModelTestBase
    {
        /// <summary>
        /// Regression test: when Python.Runtime is an incompatible version (e.g. pythonnet 2.x loaded
        /// instead of 3.x), Py.CreateScope() throws TypeLoadException because PyModule does not exist
        /// in that assembly. The ViewModel must catch this and show an error state rather than crashing.
        /// </summary>
        [Test]
        public void WhenMigrationThrowsTypeLoadExceptionViewModelShowsErrorState()
        {
            // Arrange
            var pyNode = new PythonNode();
            var workspace = CurrentDynamoModel.CurrentWorkspace as WorkspaceModel;
            var pathManager = CurrentDynamoModel.PathManager;

            Func<string, string> brokenMigrator = _ =>
                throw new TypeLoadException("Could not load type 'Python.Runtime.PyModule' from assembly 'Python.Runtime, Version=2.5.2.12086'");

            // Act
            PythonMigrationAssistantViewModel viewModel = null;
            Assert.DoesNotThrow(() =>
            {
                viewModel = new PythonMigrationAssistantViewModel(
                    pyNode, workspace, pathManager, new Version(3, 0), brokenMigrator);
            });

            // Assert
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(State.Error, viewModel.CurrentViewModel.DiffState);
        }

        /// <summary>
        /// Verifies that a MissingMethodException (e.g. missing member in incompatible Python.Runtime)
        /// during migration is handled gracefully and shows an error state.
        /// </summary>
        [Test]
        public void WhenMigrationThrowsMissingMethodExceptionViewModelShowsErrorState()
        {
            var pyNode = new PythonNode();
            var workspace = CurrentDynamoModel.CurrentWorkspace as WorkspaceModel;
            var pathManager = CurrentDynamoModel.PathManager;

            Func<string, string> brokenMigrator = _ =>
                throw new MissingMethodException("Python.Runtime.Py", "CreateScope");

            PythonMigrationAssistantViewModel viewModel = null;
            Assert.DoesNotThrow(() =>
            {
                viewModel = new PythonMigrationAssistantViewModel(
                    pyNode, workspace, pathManager, new Version(3, 0), brokenMigrator);
            });

            Assert.IsNotNull(viewModel);
            Assert.AreEqual(State.Error, viewModel.CurrentViewModel.DiffState);
        }

        /// <summary>
        /// Verifies that a FileLoadException (e.g. assembly version conflict) during migration
        /// is handled gracefully and shows an error state.
        /// </summary>
        [Test]
        public void WhenMigrationThrowsFileLoadExceptionViewModelShowsErrorState()
        {
            var pyNode = new PythonNode();
            var workspace = CurrentDynamoModel.CurrentWorkspace as WorkspaceModel;
            var pathManager = CurrentDynamoModel.PathManager;

            Func<string, string> brokenMigrator = _ =>
                throw new FileLoadException("Could not load file or assembly 'Python.Runtime'");

            PythonMigrationAssistantViewModel viewModel = null;
            Assert.DoesNotThrow(() =>
            {
                viewModel = new PythonMigrationAssistantViewModel(
                    pyNode, workspace, pathManager, new Version(3, 0), brokenMigrator);
            });

            Assert.IsNotNull(viewModel);
            Assert.AreEqual(State.Error, viewModel.CurrentViewModel.DiffState);
        }

        /// <summary>
        /// Verifies that a BadImageFormatException (e.g. 32/64-bit or .NET target mismatch in
        /// Python.Runtime) during migration is handled gracefully and shows an error state.
        /// </summary>
        [Test]
        public void WhenMigrationThrowsBadImageFormatExceptionViewModelShowsErrorState()
        {
            var pyNode = new PythonNode();
            var workspace = CurrentDynamoModel.CurrentWorkspace as WorkspaceModel;
            var pathManager = CurrentDynamoModel.PathManager;

            Func<string, string> brokenMigrator = _ =>
                throw new BadImageFormatException("The format of the file 'Python.Runtime' is invalid.");

            PythonMigrationAssistantViewModel viewModel = null;
            Assert.DoesNotThrow(() =>
            {
                viewModel = new PythonMigrationAssistantViewModel(
                    pyNode, workspace, pathManager, new Version(3, 0), brokenMigrator);
            });

            Assert.IsNotNull(viewModel);
            Assert.AreEqual(State.Error, viewModel.CurrentViewModel.DiffState);
        }
    }
}
