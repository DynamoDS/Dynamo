using System.IO;

using Dynamo;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Tests;

using NUnit.Framework;

namespace DataBridgeTests
{
    public class DataBridgeTests : DynamoModelTestBase
    {
        [Test]
        [Category("Failure")]
        public void CanUseWatchInCustomNode()
        {
            var examplesPath = Path.Combine(TestDirectory, @"core\watch");

            RunModel(Path.Combine(examplesPath, "watchdatabridge.dyn"));

            var customNodeId =
                CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Function>().Definition.FunctionId;

            CustomNodeWorkspaceModel customNodeWorkspace;
            Assert.IsTrue(CurrentDynamoModel.CustomNodeManager.TryGetFunctionWorkspace(customNodeId, true, out customNodeWorkspace));

            var innerWatch = customNodeWorkspace.FirstNodeFromWorkspace<Watch>();
            var outerWatch = CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();

            Assert.AreEqual(10, innerWatch.CachedValue);
            Assert.AreEqual(10, outerWatch.CachedValue);
        }
    }
}
