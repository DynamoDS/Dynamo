using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Dynamo.Engine.CodeGeneration;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [Category("DSExecution")]
    class ProfilingTest : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("VMDataBridge.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void TestProfilingSingleNode()
        {
            // Note: This test file is saved in manual run mode
            string openPath = Path.Combine(TestDirectory, @"core\profiling\SingleNode.dyn");
            OpenModel(openPath);

            Assert.IsNotNull(CurrentDynamoModel.EngineController.profilingSession);
            var profilingData = CurrentDynamoModel.EngineController.profilingSession.ProfilingData;
            var node = CurrentDynamoModel.CurrentWorkspace.Nodes.FirstOrDefault();

            Assert.IsNull(profilingData.TotalExecutionTime);
            Assert.IsNull(profilingData.NodeExecutionTime(node));

            BeginRun();

            Assert.IsNotNull(profilingData.TotalExecutionTime);
            Assert.Greater(profilingData.TotalExecutionTime?.Ticks, 0);
            Assert.IsNotNull(profilingData.NodeExecutionTime(node));
            Assert.Greater(profilingData.NodeExecutionTime(node)?.Ticks, 0);

            this.CurrentDynamoModel.ExecuteCommand(
                new DynamoModel.DeleteModelCommand(node.GUID));
            BeginRun();

            Assert.IsNotNull(profilingData.TotalExecutionTime);
            Assert.Greater(profilingData.TotalExecutionTime?.Ticks, 0);
            Assert.IsNull(profilingData.NodeExecutionTime(node));
        }
    }
}
