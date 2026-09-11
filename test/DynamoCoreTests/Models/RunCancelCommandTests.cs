using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.Scheduler;
using NUnit.Framework;

namespace Dynamo.Tests.ModelsTest
{
    [TestFixture]
    class RunCancelCommandTests : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenNoRunInProgressThenCancelIsNoOp()
        {
            var home = CurrentHomeWorkspace();
            var evaluationCount = home.EvaluationCount;
            var runtime = CurrentDynamoModel.EngineController.LiveRunnerRuntimeCore;

            Assert.DoesNotThrow(() =>
                CurrentDynamoModel.ExecuteCommand(new DynamoModel.RunCancelCommand(false, true)));

            Assert.IsFalse(home.GraphRunInProgress);
            Assert.AreEqual(evaluationCount, home.EvaluationCount);
            Assert.IsFalse(runtime.CancellationPending);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenCancelRunIsTrueThenGraphIsNotStarted()
        {
            CreateCodeBlock("x = 1;");
            var home = CurrentHomeWorkspace();
            var evaluationCount = home.EvaluationCount;

            CurrentDynamoModel.ExecuteCommand(new DynamoModel.RunCancelCommand(false, true));

            Assert.AreEqual(evaluationCount, home.EvaluationCount);
            Assert.IsFalse(home.GraphRunInProgress);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenForceRunCancelCommandCancelRunIsTrueThenGraphIsNotForceRun()
        {
            var home = CurrentHomeWorkspace();
            var evaluationCount = home.EvaluationCount;

            Assert.DoesNotThrow(() =>
                CurrentDynamoModel.ExecuteCommand(new DynamoModel.ForceRunCancelCommand(false, true)));

            Assert.AreEqual(evaluationCount, home.EvaluationCount);
            Assert.IsFalse(home.GraphRunInProgress);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenCancelCalledTwiceWhileIdleThenSecondCallIsNoOp()
        {
            Assert.DoesNotThrow(() =>
            {
                CurrentDynamoModel.ExecuteCommand(new DynamoModel.RunCancelCommand(false, true));
                CurrentDynamoModel.ExecuteCommand(new DynamoModel.RunCancelCommand(false, true));
            });

            Assert.IsFalse(CurrentDynamoModel.EngineController.LiveRunnerRuntimeCore.CancellationPending);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenRunCompletesWithoutCancelThenWasCancelledIsFalse()
        {
            EvaluationCompletedEventArgs completed = null;
            CurrentDynamoModel.EvaluationCompleted += (_, e) => completed = e;

            CreateCodeBlock("x = 1;");
            BeginRun();

            Assert.IsNotNull(completed);
            Assert.IsFalse(completed.WasCancelled);
            Assert.IsTrue(completed.EvaluationSucceeded);
        }

        [Test]
        public void WhenRunIsCancelledThenWasCancelledIsTrue()
        {
            var completed = CancelInFlightInfiniteLoop(out _);

            Assert.IsTrue(completed.WasCancelled);
            Assert.IsTrue(completed.EvaluationSucceeded);
        }

        [Test]
        public void WhenRunIsCancelledThenModifiedNodesStayDirty()
        {
            CancelInFlightInfiniteLoop(out var codeBlock);

            Assert.IsTrue(codeBlock.IsModified);
        }

        [Test]
        public void WhenCancelledRunFinishesThenNextRunCanSucceed()
        {
            CancelInFlightInfiniteLoop(out var codeBlock);

            CurrentDynamoModel.Scheduler.ProcessMode = TaskProcessMode.Synchronous;
            CurrentDynamoModel.ExecuteCommand(
                new DynamoModel.UpdateModelValueCommand(Guid.Empty, codeBlock.GUID, "Code", "x = 2;"));

            EvaluationCompletedEventArgs completed = null;
            CurrentDynamoModel.EvaluationCompleted += (_, e) => completed = e;
            BeginRun();

            Assert.IsNotNull(completed);
            Assert.IsFalse(completed.WasCancelled);
            AssertPreviewValue(codeBlock.GUID.ToString(), 2);
        }

        private HomeWorkspaceModel CurrentHomeWorkspace()
        {
            return (HomeWorkspaceModel)CurrentDynamoModel.CurrentWorkspace;
        }

        private CodeBlockNodeModel CreateCodeBlock(string code)
        {
            var cbn = new CodeBlockNodeModel(CurrentDynamoModel.LibraryServices);
            CurrentDynamoModel.ExecuteCommand(new DynamoModel.CreateNodeCommand(cbn, 0, 0, true, false));
            CurrentDynamoModel.ExecuteCommand(
                new DynamoModel.UpdateModelValueCommand(Guid.Empty, cbn.GUID, "Code", code));
            return cbn;
        }

        private EvaluationCompletedEventArgs CancelInFlightInfiniteLoop(out CodeBlockNodeModel codeBlock)
        {
            var home = CurrentHomeWorkspace();

            // Manual mode: only run when this test says so. In Automatic mode, editing
            // the code block would start a run on its own, on the test thread.
            home.RunSettings.RunType = RunType.Manual;

            // Background scheduler, set before the node exists so nothing can ever run
            // the slow graph inline on this thread.
            CurrentDynamoModel.Scheduler.ProcessMode = TaskProcessMode.Asynchronous;

            // Slow but finite. The engine checks for cancellation between instructions,
            // so this is interruptible. It must be able to end on its own: if cancelling
            // fails, an endless loop would leave the scheduler thread spinning and the
            // test run would freeze in teardown instead of failing. Tune the count so an
            // uncancelled run takes roughly ten seconds on your machine.
            const string slowLoop = @"b = [Imperative]
{
    i = 0;
    while (i < 1000000)
    {
        i = i + 1;
    }
    return = i;
};";

            codeBlock = CreateCodeBlock(slowLoop);

            EvaluationCompletedEventArgs completed = null;
            using var finished = new ManualResetEventSlim(false);
            CurrentDynamoModel.EvaluationCompleted += (_, e) =>
            {
                completed = e;
                finished.Set();
            };

            BeginRun();
            Assert.IsTrue(home.GraphRunInProgress, "Run did not start.");

            var deadline = DateTime.UtcNow.AddSeconds(30);
            while (!finished.Wait(50))
            {
                Assert.Less(DateTime.UtcNow, deadline, "Cancelled evaluation never completed.");
                CurrentDynamoModel.ExecuteCommand(new DynamoModel.RunCancelCommand(false, true));
            }

            return completed;
        }
    }
}
