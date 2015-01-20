using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Tests;

using NUnit.Framework;

namespace DataBridgeTests
{
    public class DataBridgeTests : DSEvaluationViewModelUnitTest
    {
        [Test]
        [Category("Failure")]
        public void CanUseWatchInCustomNode()
        {
            var examplesPath = Path.Combine(GetTestDirectory(), @"core\watch");
            var model = ViewModel.Model;

            RunModel(Path.Combine(examplesPath, "watchdatabridge.dyn"));

            var customNodeId =
                model.CurrentWorkspace.FirstNodeFromWorkspace<Function>().Definition.FunctionId;

            CustomNodeWorkspaceModel customNodeWorkspace;
            Assert.IsTrue(model.CustomNodeManager.TryGetFunctionWorkspace(customNodeId, true, out customNodeWorkspace));

            var innerWatch = customNodeWorkspace.FirstNodeFromWorkspace<Watch>();
            var outerWatch = model.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();

            Assert.AreEqual(10, (int)innerWatch.CachedValue);
            Assert.AreEqual(10, (int)outerWatch.CachedValue);
        }
    }
}
