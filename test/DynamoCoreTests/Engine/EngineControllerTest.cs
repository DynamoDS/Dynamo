using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Dynamo.Engine;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using NUnit.Framework;
using ProtoCore.AST.AssociativeAST;
using ProtoScript.Runners;
using DynCmd = Dynamo.Models.DynamoModel;

namespace Dynamo.Tests.Engine
{
    [TestFixture]
    class EngineControllerTest : DynamoModelTestBase
    {
        //This flag is to indicate that the EngineController.AstBuilt event was executed 
        private bool compiledAST = false;

        /// <summary>
        /// This test method will execute the next method from the EngineController class:
        /// GraphSyncData ComputeSyncData(IEnumerable<NodeModel> nodes, IEnumerable<NodeModel> updatedNodes, bool verboseLogging)
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void EngineControllerComputeSyncDataTest()
        {
            //Arrange
            var openPath = Path.Combine(TestDirectory, @"core\nodeLocationTest.dyn");
            RunModel(openPath);

            //Act
            //There is a code section in ComputeSyncData method where the only way to be reached is by passing updatedNodes = null
            var syncData = CurrentDynamoModel.EngineController.ComputeSyncData(null, null, false);

            //Assert
            //Validating that the method ComputeSyncData after passing the parameter updatedNodes = null
            Assert.IsNull(syncData);
            Assert.IsFalse(CurrentDynamoModel.EngineController.HasPendingGraphSyncData);

        }

        /// <summary>
        /// This test method will execute the next method from the EngineController class:
        /// PreviewGraphSyncData(IEnumerable<NodeModel> updatedNodes, bool verboseLogging)
        /// </summary>

        [Test]
        [Category("UnitTests")]
        public void EngineControllerPreviewGraphSyncDataTest()
        {
            //Arrange
            var openPath = Path.Combine(TestDirectory, @"core\nodeLocationTest.dyn");
            RunModel(openPath);
            //I created a list with the first node from the workspace
            List<NodeModel> nodesList = new List<NodeModel>()
            { CurrentDynamoModel.CurrentWorkspace.Nodes.First() };

            //Act
            var syncData = CurrentDynamoModel.EngineController.PreviewGraphSyncData(nodesList, false);

            //There is a code section in PreviewGraphSyncData method where the only way to be reached is by passing updatedNodes = null
            var syncData2 = CurrentDynamoModel.EngineController.PreviewGraphSyncData(null, false);

            //Assert        
            Assert.IsNotNull(syncData);
            // Validating that the method PreviewGraphSyncData after passing the parameter updatedNodes = null
            Assert.IsNull(syncData2);
        }

        /// <summary>
        /// This test method will execute the void ReconcileTraceDataAndNotify() method from the EngineController class
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void EngineControllerReconcileTraceDataAndNotify()
        {
            //Arrange
            CurrentDynamoModel.EngineController.AstBuilt += EngineController_AstBuilt;

            //Act
            var codeBlockNodeOne = CreateCodeBlockNode();
            CurrentDynamoModel.AddNodeToCurrentWorkspace(codeBlockNodeOne, true);
            codeBlockNodeOne.UpdateValue(new UpdateValueParams("Code", "10;"));
            CurrentDynamoModel.EngineController.AstBuilt -= EngineController_AstBuilt;

            //Assert
            //Validates that the AstBuilt event was fired 
            Assert.IsTrue(compiledAST);
        }


        /// <summary>
        /// This test method will execute the void ReconcileTraceDataAndNotify() method from the EngineController class
        /// But it will execute section where it raises an exception
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void EngineControllerReconcileTraceDataAndNotifyException()
        {
            //Arrange
            CurrentDynamoModel.EngineController.Dispose();

            //Assert
            //Because the object was already disposed then when calling the ReconcileTraceDataAndNotify() method will raise an exception
            Assert.Throws<ObjectDisposedException>( () => CurrentDynamoModel.EngineController.ReconcileTraceDataAndNotify());

        }

        //This method will be executed when the EngineController.AstBuilt event is raised
        private void EngineController_AstBuilt(object sender, Dynamo.Engine.CodeGeneration.CompiledEventArgs e)
        {
            compiledAST = true;
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
