using Dynamo.Configuration;
using Dynamo.Graph.Workspaces;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Scheduler;
using NUnit.Framework;
using System.IO;

namespace Dynamo.Tests.ModelsTests
{
    [TestFixture]
    class FileTrustHeadlessTests : DynamoModelTestBase
    {
        private const string SampleDynPath = @"core\callsite\RebindingMultiDimension.dyn";

        /// <summary>
        /// Headless hosts (DynamoCLI, ExecuteCommand) run with StartInTestMode=false.
        /// Trust warnings remain enabled so we exercise the reviewer's precondition.
        /// </summary>
        protected override DynamoModel.IStartConfiguration CreateStartConfiguration(IPreferences settings)
        {
            return new DynamoModel.DefaultStartConfiguration()
            {
                PathResolver = pathResolver,
                StartInTestMode = false,
                GeometryFactoryPath = preloader.GeometryFactoryPath,
                ProcessMode = TaskProcessMode.Synchronous,
                Preferences = new PreferenceSettings()
            };
        }

        [Test]
        public void WhenOpenedViaOpenFileFromPathFromUntrustedLocationInHeadlessModeThenForceBlockRunIsFalse()
        {
            // Arrange
            AssertHeadlessTrustPreconditions();
            var untrustedPath = CopyDynToUntrustedTemp(SampleDynPath);
            AssertPathIsUntrusted(untrustedPath);

            // Act
            CurrentDynamoModel.ExecuteCommand(new DynamoModel.OpenFileCommand(untrustedPath));

            // Assert
            Assert.IsFalse(GetHomeWorkspace().RunSettings.ForceBlockRun);
        }

        [Test]
        public void WhenOpenedViaDefaultOpenFileCommandFromUntrustedLocationInHeadlessModeThenForceBlockRunIsFalse()
        {
            // Arrange
            AssertHeadlessTrustPreconditions();
            var untrustedPath = CopyDynToUntrustedTemp(SampleDynPath);
            AssertPathIsUntrusted(untrustedPath);

            // Act
            CurrentDynamoModel.OpenFileFromPath(untrustedPath, forceManualExecutionMode: true);

            // Assert
            Assert.IsFalse(GetHomeWorkspace().RunSettings.ForceBlockRun);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenOpenedHeadlesslyFromUntrustedLocationThenGraphEvaluationCompletes()
        {
            // Arrange
            AssertHeadlessTrustPreconditions();
            var untrustedPath = CopyDynToUntrustedTemp(SampleDynPath);
            AssertPathIsUntrusted(untrustedPath);

            // Act
            CurrentDynamoModel.OpenFileFromPath(untrustedPath, forceManualExecutionMode: true);
            var evaluationCompleted = RunGraphAndWaitForEvaluation();

            // Assert
            Assert.IsFalse(GetHomeWorkspace().RunSettings.ForceBlockRun);
            Assert.IsTrue(evaluationCompleted, "Headless open from an untrusted path must not silently block graph execution.");
        }

        [Test]
        [Category("UnitTests")]
        public void WhenOpenedWithExplicitForceBlockRunThenEvaluationDoesNotComplete()
        {
            // Arrange
            var filePath = Path.Combine(TestDirectory, SampleDynPath);

            // Act
            CurrentDynamoModel.ExecuteCommand(
                new DynamoModel.OpenFileCommand(
                    filePath,
                    forceManualExecutionMode: true,
                    isTemplate: false,
                    forceBlockRun: true));
            var evaluationCompleted = RunGraphAndWaitForEvaluation();

            // Assert
            Assert.IsTrue(GetHomeWorkspace().RunSettings.ForceBlockRun);
            Assert.IsFalse(evaluationCompleted, "Explicit ForceBlockRun must block graph execution until trust is acknowledged.");
        }


        private void AssertHeadlessTrustPreconditions()
        {
            Assert.IsFalse(DynamoModel.IsTestMode);
            Assert.IsFalse(CurrentDynamoModel.PreferenceSettings.DisableTrustWarnings);
        }

        private string CopyDynToUntrustedTemp(string path)
        {
            var sourcePath = Path.Combine(TestDirectory, path);
            var untrustedTempPath = Path.Combine(TempFolder, "untrusted");
            Directory.CreateDirectory(untrustedTempPath);

            var destinationPath = Path.Combine(untrustedTempPath, Path.GetFileName(sourcePath));
            File.Copy(sourcePath, destinationPath, overwrite: true);
            return destinationPath;
        }

        private void AssertPathIsUntrusted(string path)
        {
            var dirPath = Path.GetDirectoryName(path);
            Assert.IsFalse(CurrentDynamoModel.PreferenceSettings.IsTrustedLocation(dirPath));
        }

        private HomeWorkspaceModel GetHomeWorkspace()
        {
            var workspace = CurrentDynamoModel.CurrentWorkspace as HomeWorkspaceModel;
            Assert.IsNotNull(workspace);
            return workspace;
        }

        private bool RunGraphAndWaitForEvaluation()
        {
            var evaluationCompleted = false;

            void OnEvaluationCompleted(object sender, EvaluationCompletedEventArgs e)
            {
                if (e.EvaluationTookPlace)
                {
                    evaluationCompleted = true;
                }
            }

            CurrentDynamoModel.EvaluationCompleted += OnEvaluationCompleted;
            try
            {
                BeginRun();
                EmptyScheduler();
            }
            finally
            {
                CurrentDynamoModel.EvaluationCompleted -= OnEvaluationCompleted;
            }

            return evaluationCompleted;
        }
    }
}
