using Dynamo.Engine.Profiling;
using Dynamo.Graph.Nodes;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using DynCmd = Dynamo.Models.DynamoModel;

namespace Dynamo.Tests.Engine.Profiling
{
    [TestFixture]
    class ProfilingDataTest : DynamoModelTestBase
    {
        [Test]
        [Category("UnitTests")]
        public void ProfilingDataRegisterUnRegisterNodesTest()
        {
            //Arrange
            var nodesList = new List<NodeModel>();
            var profiling = new ProfilingData();
            var cbn = CreateCodeBlockNode();

            //Act
            profiling.RegisterNode(cbn);
            var nodeData = profiling.NodeProfilingData;

            //Assert
            Assert.IsNotNull(nodeData);
            Assert.AreEqual(nodeData.Count, 1);

            //Act
            profiling.UnregisterNode(nodeData.First().Key);

            //Assert
            Assert.AreEqual(nodeData.Count, 0);

            var cbn2 = CreateCodeBlockNode();
            nodesList.Add(cbn2);
           
            //Act
            profiling.UnregisterDeletedNodes(nodesList);
            nodeData = profiling.NodeProfilingData;

            //Assert
            Assert.AreEqual(nodeData.Count, 0);
        }

        [Test]
        [Category("UnitTests")]
        public void NodeProfilingDataDispose()
        {
            //Arrange
            var profiling = new ProfilingData();
            var cbn = CreateCodeBlockNode();
            profiling.RegisterNode(cbn);
            var nodeData = profiling.NodeProfilingData;

            //Act
            var timeSpan = nodeData.First().Value.ExecutionTime;
            nodeData.First().Value.Dispose();

            //Assert
            Assert.IsNull(timeSpan);
            Assert.IsNotNull(nodeData);
            Assert.AreEqual(nodeData.Count, 1);

          
        }

        private CodeBlockNodeModel CreateCodeBlockNode()
        {
            var cbn = new CodeBlockNodeModel(CurrentDynamoModel.LibraryServices);
            var command = new DynCmd.CreateNodeCommand(cbn, 0, 0, true, false);

            CurrentDynamoModel.ExecuteCommand(command);

            Assert.IsNotNull(cbn);
            return cbn;
        }
    }
}
