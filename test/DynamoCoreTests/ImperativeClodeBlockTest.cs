using System.Collections.Generic;
using System.Linq;
using Dynamo.Scheduler;
using NUnit.Framework;
using static Dynamo.Models.DynamoModel;

namespace Dynamo.Tests
{
    [Category("ImperativeCode")]
    class ImperativeClodeBlockTest : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("DSIronPython.dll");
            libraries.Add("FunctionObject.ds");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void TestCallStaticFunction()
        {
            RunModel(@"core\imperative\call_static_function.dyn");
            AssertPreviewValue("250880ed-5d34-49e7-b92b-2a7f9336f62b", new object[] { 3, 5 });
        }

        [Test]
        public void DeletingImperativeCBNShouldNotLeadToCrash()
        {
            TaskStateChangedEventHandler evaluationDidNotFailHandler =
                (DynamoScheduler sender, TaskStateChangedEventArgs args) =>
                {
                    Assert.AreNotEqual(TaskStateChangedEventArgs.State.ExecutionFailed, args.CurrentState);
                };
            try
            {
                CurrentDynamoModel.Scheduler.TaskStateChanged += evaluationDidNotFailHandler;
                OpenModel(@"core\imperative\delete_imperative_cbn_crash.dyn");
                AssertPreviewValue("0692b19256834a9187f3bcd500d513f1", 5.86);
                CurrentDynamoModel.ExecuteCommand(new DeleteModelCommand("45f0db0f-c014-481e-9b7f-939da54f2adc"));
                CurrentDynamoModel.ExecuteCommand(new DeleteModelCommand("235f53ba-d913-45d0-986a-b9c6588cba45"));
                AssertPreviewValue("0692b19256834a9187f3bcd500d513f1", 5.86);
                Assert.AreEqual(2, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            }
            finally
            {
                CurrentDynamoModel.Scheduler.TaskStateChanged -= evaluationDidNotFailHandler;
            }
        }
    }
}
