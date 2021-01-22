using System.Collections.Generic;
using System.IO;
using Dynamo.Selection;
using System.Linq;
using Dynamo.Models;
using NUnit.Framework;
using Dynamo.Graph.Workspaces;
using static Dynamo.Models.DynamoModel;
using Dynamo.Scheduler;

namespace Dynamo.Tests
{
    [TestFixture]
    class DelayedGraphExecutionTests : DynamoModelTestBase
    {
        private HomeWorkspaceModel wModel;
        private int taskCounter = 0;
        private bool activeTaskCounter = false;
        private int originalNodeCount = 0;
        private int expectedTaskModifiedNodeCount = 0;

        private void activateTaskCounter(int expected)
        {
            activeTaskCounter = true;
            taskCounter = 0;
            expectedTaskModifiedNodeCount = expected;
        }

        private void deactivateTaskCounter()
        {
            activeTaskCounter = false;
            taskCounter = 0;
        }

        private List<Graph.ModelBase> workspaceNodes()
        {
            return wModel.Nodes.Select(x => x as Graph.ModelBase).ToList();
        }

        private void taskStateHandler(DynamoScheduler sender, TaskStateChangedEventArgs args)
        {
            if (args.Task is UpdateGraphAsyncTask && args.CurrentState == TaskStateChangedEventArgs.State.Scheduled && activeTaskCounter)
            {
                Assert.AreEqual((args.Task as UpdateGraphAsyncTask).ModifiedNodes.Count(), expectedTaskModifiedNodeCount);
                taskCounter++;
            }
        }

        [SetUp]
        public void Init()
        {
            OpenModel(Path.Combine(TestDirectory, "core", "DelayedGraphExecution.dyn"));
            wModel = CurrentDynamoModel.CurrentWorkspace as HomeWorkspaceModel;
            wModel.RunSettings.RunType = RunType.Automatic;

            originalNodeCount = workspaceNodes().Count();

            CurrentDynamoModel.Scheduler.TaskStateChanged += taskStateHandler;
        }

        public override void Cleanup()
        {
            CurrentDynamoModel.Scheduler.TaskStateChanged -= taskStateHandler;
            base.Cleanup();
        }

        [Test]
        [Category("UnitTests")]
        public void DelayedExecutionOnDeletionUndoTest()
        {
            wModel.RecordAndDeleteModels(workspaceNodes());
            Assert.AreEqual(wModel.Nodes.Count(), 0);

            activateTaskCounter(originalNodeCount);
            CurrentDynamoModel.ExecuteCommand(new UndoRedoCommand(UndoRedoCommand.Operation.Undo));
            Assert.AreEqual(wModel.Nodes.Count(), originalNodeCount);
            Assert.AreEqual(taskCounter, 1);
            deactivateTaskCounter();
        }

        [Test]
        [Category("UnitTests")]
        public void DelayedExecutionOnCopyPaste()
        {
            DynamoSelection.Instance.ClearSelection();
            workspaceNodes().ForEach((ele) => DynamoSelection.Instance.Selection.Add(ele));
            CurrentDynamoModel.Copy();

            activateTaskCounter(originalNodeCount * 2);
            CurrentDynamoModel.Paste();
            Assert.AreEqual(wModel.Nodes.Count(), originalNodeCount * 2);
            Assert.AreEqual(taskCounter, 1);
            deactivateTaskCounter();
        }

        [Test]
        [Category("UnitTests")]
        public void DelayedExecutionUndoRedoTest()
        {
            DynamoSelection.Instance.ClearSelection();
            workspaceNodes().ForEach((ele) => DynamoSelection.Instance.Selection.Add(ele));
            CurrentDynamoModel.Copy();
            CurrentDynamoModel.Paste();
            Assert.AreEqual(wModel.Nodes.Count(), originalNodeCount * 2);

            CurrentDynamoModel.ExecuteCommand(new UndoRedoCommand(UndoRedoCommand.Operation.Undo));
            Assert.AreEqual(wModel.Nodes.Count(), originalNodeCount);

            activateTaskCounter(originalNodeCount);
            CurrentDynamoModel.ExecuteCommand(new UndoRedoCommand(UndoRedoCommand.Operation.Redo));
            Assert.AreEqual(wModel.Nodes.Count(), originalNodeCount * 2);
            Assert.AreEqual(taskCounter, 1);
            deactivateTaskCounter();

            CurrentDynamoModel.ExecuteCommand(new UndoRedoCommand(UndoRedoCommand.Operation.Undo));
            Assert.IsTrue(wModel.Nodes.Count() == originalNodeCount);
        }

        [Test]
        [Category("UnitTests")]
        public void DelayedExecutionNodeToCodeTest()
        {
            DynamoSelection.Instance.ClearSelection();
            workspaceNodes().ForEach((ele) => DynamoSelection.Instance.Selection.Add(ele));

            var finalNodes = 117;// includes dummy nodes
            activateTaskCounter(finalNodes);
            CurrentDynamoModel.ExecuteCommand(new ConvertNodesToCodeCommand());
            Assert.AreEqual(wModel.Nodes.Count(), finalNodes);
            Assert.AreEqual(taskCounter, 1);
            deactivateTaskCounter();

            CurrentDynamoModel.ExecuteCommand(new UndoRedoCommand(UndoRedoCommand.Operation.Undo));
        }
    }
}
