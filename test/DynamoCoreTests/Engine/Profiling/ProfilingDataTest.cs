using System.Collections.Generic;
using System.Linq;
using Dynamo.Engine.Profiling;
using Dynamo.Graph.Nodes;
using NUnit.Framework;
using DynCmd = Dynamo.Models.DynamoModel;

namespace Dynamo.Tests.Engine.Profiling
{
    [TestFixture]
    class ProfilingDataTest : DynamoModelTestBase
    {
        /// <summary>
        /// This test method will execute the next methods from the ProfilingData class
        /// internal Dictionary<Guid, NodeProfilingData> NodeProfilingData
        /// internal void UnregisterNode(Guid guid)
        /// void UnregisterDeletedNodes(IEnumerable<NodeModel> modelNodes)
        /// </summary>
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
            //Checking that the code block was registered correctly
            Assert.IsNotNull(nodeData);
            Assert.AreEqual(nodeData.Count, 1);

            //Act
            profiling.UnregisterNode(nodeData.First().Key);

            //Assert
            //Checking that the code block was unregistered successfully
            Assert.AreEqual(nodeData.Count, 0);

            var cbn2 = CreateCodeBlockNode();
            nodesList.Add(cbn2);
           
            //Act
            profiling.UnregisterDeletedNodes(nodesList);
            nodeData = profiling.NodeProfilingData;

            //Assert
            //Checking that the second code block was unregistered correctly
            Assert.AreEqual(nodeData.Count, 0);
        }

        /// <summary>
        /// This test method will execute the next methods from the NodeProfilingData class
        /// public void Dispose()
        /// internal TimeSpan? ExecutionTime
        /// </summary>
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
