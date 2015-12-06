using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using DSCoreNodesUI;
using DSCoreNodesUI.Input;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Selection;
using NUnit.Framework;

using DynCmd = Dynamo.Models.DynamoModel;

namespace Dynamo.Tests
{
    internal class NodeExecutionTest : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("VMDataBridge.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        [Category("UnitTests")]
        public void Freeze_ANode_Test()
        {           
            var model = CurrentDynamoModel;
            var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));
            model.CurrentWorkspace.AddAndRegisterNode(addNode, false);

            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count(), 1);
            Assert.AreEqual(addNode.IsFrozen, false);

            addNode.IsFrozen = true;          
            Assert.AreEqual(addNode.IsFrozen, true);
        }

        [Test]
        [Category("UnitTests")]
        public void Freeze_Test_On_WatchNode()
        {
            var model = CurrentDynamoModel;
            //create some numbers
            var numberNode1 = new DoubleInput();
            numberNode1.Value = "1";
            var numberNode2 = new DoubleInput();
            numberNode2.Value = "2";
            var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));
           
            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(numberNode1, 0, 0, true, false));
            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(numberNode2, 0, 0, true, false));
            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(addNode, 0, 0, true, false));

            //connect them up
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(numberNode1.GUID, 0, PortType.Output, DynCmd.MakeConnectionCommand.Mode.Begin));
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(addNode.GUID, 0, PortType.Input, DynCmd.MakeConnectionCommand.Mode.End));

            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(numberNode2.GUID, 0, PortType.Output, DynCmd.MakeConnectionCommand.Mode.Begin));
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(addNode.GUID, 1, PortType.Input, DynCmd.MakeConnectionCommand.Mode.End));

            //add  a watch node
            var watchNode = new Watch();
            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(watchNode, 0, 0, true, false));

            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(addNode.GUID, 0, PortType.Output, DynCmd.MakeConnectionCommand.Mode.Begin));
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(watchNode.GUID, 0, PortType.Input, DynCmd.MakeConnectionCommand.Mode.End));

            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count(), 4);
            Assert.AreEqual(model.CurrentWorkspace.Connectors.Count(), 3);

            //check the value
            AssertPreviewValue(addNode.GUID.ToString(), 3);
        
            //check the watch node value
            AssertPreviewValue(watchNode.GUID.ToString(), 3);
        }


        [Test]
        [Category("UnitTests")]
        public void ParentNode_Freeze_Test()
        {
            var model = CurrentDynamoModel;
            //create some numbers
            var numberNode1 = new DoubleInput();
            numberNode1.Value = "1";
            var numberNode2 = new DoubleInput();
            numberNode2.Value = "2";
            var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));

            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(numberNode1, 0, 0, true, false));
            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(numberNode2, 0, 0, true, false));
            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(addNode, 0, 0, true, false));

            //connect them up
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(numberNode1.GUID, 0, PortType.Output, DynCmd.MakeConnectionCommand.Mode.Begin));
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(addNode.GUID, 0, PortType.Input, DynCmd.MakeConnectionCommand.Mode.End));

            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(numberNode2.GUID, 0, PortType.Output, DynCmd.MakeConnectionCommand.Mode.Begin));
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(addNode.GUID, 1, PortType.Input, DynCmd.MakeConnectionCommand.Mode.End));

            //add  a watch node
            var watchNode = new Watch();
            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(watchNode, 0, 0, true, false));

            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(addNode.GUID, 0, PortType.Output, DynCmd.MakeConnectionCommand.Mode.Begin));
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(watchNode.GUID, 0, PortType.Input, DynCmd.MakeConnectionCommand.Mode.End));

            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count(), 4);
            Assert.AreEqual(model.CurrentWorkspace.Connectors.Count(), 3);

            //Check the value
            AssertPreviewValue(addNode.GUID.ToString(), 3);

            //Freeeze the numbernode1 and compute the Freeze state of other nodes.           
            numberNode1.IsFrozen = true;          

            //change the value of number node1.
            numberNode1.Value = "3.0";

            // the number node must be frozen and not executing state
            Assert.AreEqual(numberNode1.IsFrozen, true);
             
            //the add node must be frozen and can execute state
            Assert.AreEqual(addNode.IsFrozen, true);
            
            //Since the nodes are frozen, the value  of add node should not change.            
            AssertPreviewValue(addNode.GUID.ToString(), 3);

            //check the watch node value
            AssertPreviewValue(watchNode.GUID.ToString(), 3);
        }

        [Test]
        [Category("UnitTests")]
        public void ParentNode_Enters_TemporaryState_Test()
        {
            var model = CurrentDynamoModel;
            //create some numbers
            var numberNode1 = new DoubleInput();
            numberNode1.Value = "1";
            var numberNode2 = new DoubleInput();
            numberNode2.Value = "2";
            var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));

            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(numberNode1, 0, 0, true, false));
            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(numberNode2, 0, 0, true, false));
            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(addNode, 0, 0, true, false));

            //connect them up
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(numberNode1.GUID, 0, PortType.Output, DynCmd.MakeConnectionCommand.Mode.Begin));
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(addNode.GUID, 0, PortType.Input, DynCmd.MakeConnectionCommand.Mode.End));

            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(numberNode2.GUID, 0, PortType.Output, DynCmd.MakeConnectionCommand.Mode.Begin));
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(addNode.GUID, 1, PortType.Input, DynCmd.MakeConnectionCommand.Mode.End));

            //add  a watch node
            var watchNode = new Watch();
            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(watchNode, 0, 0, true, false));

            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(addNode.GUID, 0, PortType.Output, DynCmd.MakeConnectionCommand.Mode.Begin));
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(watchNode.GUID, 0, PortType.Input, DynCmd.MakeConnectionCommand.Mode.End));

            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count(), 4);
            Assert.AreEqual(model.CurrentWorkspace.Connectors.Count(), 3);

            //Check the value
            AssertPreviewValue(addNode.GUID.ToString(), 3);

            //freeze add node
            addNode.IsFrozen = true;
            
            // the add node must be frozen and not executing state
            Assert.AreEqual(addNode.IsFrozen, true);
           
            //freeze number node.
            numberNode1.IsFrozen = true;
           
            //change the value on number node 1
            numberNode1.Value = "3.0";

            // the number node must be frozen and not executing state
            Assert.AreEqual(numberNode1.IsFrozen, true);
            
            // the add node must be frozen and in a temporary state
            Assert.AreEqual(addNode.IsFrozen, true);
            
            //Since the add node is frozen, the value should not change
            AssertPreviewValue(addNode.GUID.ToString(), 3);

            //check the watch node value
            AssertPreviewValue(watchNode.GUID.ToString(), 3);
        }

        [Test]
        [Category("UnitTests")]
        public void UnFreeze_ParentNode_UnfreezesChildNodes_Test()
        {
            var model = CurrentDynamoModel;
            //create some numbers
            var numberNode1 = new DoubleInput();
            numberNode1.Value = "1";
            var numberNode2 = new DoubleInput();
            numberNode2.Value = "2";
            var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));

            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(numberNode1, 0, 0, true, false));
            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(numberNode2, 0, 0, true, false));
            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(addNode, 0, 0, true, false));

            //connect them up
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(numberNode1.GUID, 0, PortType.Output, DynCmd.MakeConnectionCommand.Mode.Begin));
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(addNode.GUID, 0, PortType.Input, DynCmd.MakeConnectionCommand.Mode.End));

            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(numberNode2.GUID, 0, PortType.Output, DynCmd.MakeConnectionCommand.Mode.Begin));
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(addNode.GUID, 1, PortType.Input, DynCmd.MakeConnectionCommand.Mode.End));

            //add  a watch node
            var watchNode = new Watch();
            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(watchNode, 0, 0, true, false));

            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(addNode.GUID, 0, PortType.Output, DynCmd.MakeConnectionCommand.Mode.Begin));
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(watchNode.GUID, 0, PortType.Input, DynCmd.MakeConnectionCommand.Mode.End));

            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count(), 4);
            Assert.AreEqual(model.CurrentWorkspace.Connectors.Count(), 3);

            BeginRun();

            //check the value
            AssertPreviewValue(addNode.GUID.ToString(), 3);

            //Add the number node to selection and call Freeze state on the workspace.
            //this should freeze the number node.
            numberNode1.IsFrozen = true;
            
            // the number node must be frozen and not executing state
            Assert.AreEqual(numberNode1.IsFrozen, true);
            
            //the add node must be frozen and can execute state
            Assert.AreEqual(addNode.IsFrozen, true);
            
            //check the value
            AssertPreviewValue(addNode.GUID.ToString(), 3);

            //check the watch node value
            AssertPreviewValue(watchNode.GUID.ToString(), 3);

            //now the number node1 is frozen. change the value.
            numberNode1.Value = "3.0";

            //check the value of add node. it should not change.
            AssertPreviewValue(addNode.GUID.ToString(), 3);

            //unfreeze the input node
            numberNode1.IsFrozen = false;
            
            Assert.AreEqual(numberNode1.IsFrozen, false);

            //Now the add node should be in unfreeze state
            Assert.AreEqual(addNode.IsFrozen, false);

            //Now the add node should get the value
            AssertPreviewValue(addNode.GUID.ToString(), 5);

            //check the watch node value
            AssertPreviewValue(watchNode.GUID.ToString(), 5);
        }

        [Test]
        [Category("UnitTests")]
        public void Unfreeze_ParentNode_MakesATemporaryStateNode_SwitchBackToPreviousState_Test()
        {
            var model = CurrentDynamoModel;
            //create some numbers
            var numberNode1 = new DoubleInput();
            numberNode1.Value = "1";
            var numberNode2 = new DoubleInput();
            numberNode2.Value = "2";
            var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));

            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(numberNode1, 0, 0, true, false));
            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(numberNode2, 0, 0, true, false));
            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(addNode, 0, 0, true, false));

            //connect them up
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(numberNode1.GUID, 0, PortType.Output, DynCmd.MakeConnectionCommand.Mode.Begin));
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(addNode.GUID, 0, PortType.Input, DynCmd.MakeConnectionCommand.Mode.End));

            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(numberNode2.GUID, 0, PortType.Output, DynCmd.MakeConnectionCommand.Mode.Begin));
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(addNode.GUID, 1, PortType.Input, DynCmd.MakeConnectionCommand.Mode.End));

            //add  a watch node
            var watchNode = new Watch() {X = 100, Y = 300};
            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(watchNode, 0, 0, true, false));
            
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(addNode.GUID, 0, PortType.Output, DynCmd.MakeConnectionCommand.Mode.Begin));
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(watchNode.GUID, 0, PortType.Input, DynCmd.MakeConnectionCommand.Mode.End));

            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count(), 4);
            Assert.AreEqual(model.CurrentWorkspace.Connectors.Count(), 3);

            //check the value
            AssertPreviewValue(addNode.GUID.ToString(), 3);

            //freeze add node
            addNode.IsFrozen = true;
           
            // the add node must be frozen and not executing state
            Assert.AreEqual(addNode.IsFrozen, true);
           
            //freeze number node.
            numberNode1.IsFrozen = true;
            
            // the number node must be frozen and not executing state
            Assert.AreEqual(numberNode1.IsFrozen, true);
            
            //change the value on number node 1
            numberNode1.Value = "3.0";

            // the add node must be frozen and in a temporary state
            Assert.AreEqual(addNode.IsFrozen, true);
           
            //check the value on add node.
            AssertPreviewValue(addNode.GUID.ToString(), 3);

            //now unfreeze number node.
            numberNode1.IsFrozen = false;
           
            //now number node is not frozen
            Assert.AreEqual(numberNode1.IsFrozen, false);

            //this causes the add node to switch back from temporary state
            //to frozen and not executing state
            Assert.AreEqual(addNode.IsFrozen, true);
            
            //check the value on add node. Add node is not executed in the run,
            // becuase the frozen nodes are removed from AST. So the value of add node
            // should be 0. But the cached value should be 3, which is from the previous execution.
            AssertPreviewValue(addNode.GUID.ToString(), 0);
            Assert.IsNotNull(addNode.CachedValue.Data);
            Assert.AreEqual(Convert.ToInt32(addNode.CachedValue.Data),3);
            Assert.AreEqual(Convert.ToInt32(watchNode.CachedValue), 3);
        }

        [Test]
        [Category("UnitTests")]
        public void Node_AttachedTThatHasParents_ShouldNotUnfreeze_UntilAllParentsUnfreeze_Test()
        {
            var model = CurrentDynamoModel;
            //create some numbers
            var numberNode1 = new DoubleInput();
            numberNode1.Value = "1";
            var numberNode2 = new DoubleInput();
            numberNode2.Value = "2";
            var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));

            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(numberNode1, 0, 0, true, false));
            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(numberNode2, 0, 0, true, false));
            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(addNode, 0, 0, true, false));

            //connect them up
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(numberNode1.GUID, 0, PortType.Output, DynCmd.MakeConnectionCommand.Mode.Begin));
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(addNode.GUID, 0, PortType.Input, DynCmd.MakeConnectionCommand.Mode.End));

            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(numberNode2.GUID, 0, PortType.Output, DynCmd.MakeConnectionCommand.Mode.Begin));
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(addNode.GUID, 1, PortType.Input, DynCmd.MakeConnectionCommand.Mode.End));

            //add  a watch node
            var watchNode = new Watch() { X = 100, Y = 300 };
            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(watchNode, 0, 0, true, false));

            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(addNode.GUID, 0, PortType.Output, DynCmd.MakeConnectionCommand.Mode.Begin));
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(watchNode.GUID, 0, PortType.Input, DynCmd.MakeConnectionCommand.Mode.End));

            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count(), 4);
            Assert.AreEqual(model.CurrentWorkspace.Connectors.Count(), 3);

            //check the value.
            AssertPreviewValue(addNode.GUID.ToString(), 3);

            //now freeze both the number nodes.
            numberNode1.IsFrozen = true;
          
            numberNode2.IsFrozen = true;
           
            //Number nodes in frozen and not executing state
            Assert.AreEqual(numberNode1.IsFrozen, true);
            
            Assert.AreEqual(numberNode2.IsFrozen, true);
           
            //change the value of number nodes
            numberNode1.Value = "3.0";
            numberNode2.Value = "3.0";

            //add node in frozen and can execute state
            Assert.AreEqual(addNode.IsFrozen, true);
            
            //addnode should not change the value.
            AssertPreviewValue(addNode.GUID.ToString(), 3);
            Assert.AreEqual(Convert.ToInt32(watchNode.CachedValue),3);

            //unfreeze one of the number node          
            numberNode1.IsFrozen = false;
           
            //Number nodes in frozen and not executing state
            Assert.AreEqual(numberNode1.IsFrozen, false);
            
            Assert.AreEqual(numberNode2.IsFrozen, true);
            
            //add node should still be in frozen and can execute state
            Assert.AreEqual(addNode.IsFrozen, true);
                      
            //now unfreeze the other node.
            numberNode2.IsFrozen = false;
            
            Assert.AreEqual(numberNode2.IsFrozen, false);
           
            //now the add node should not be in frozen state
            Assert.AreEqual(addNode.IsFrozen, false);
            
            //addnode should change the value now.
            AssertPreviewValue(addNode.GUID.ToString(), 6);
            AssertPreviewValue(watchNode.GUID.ToString(), 6);
        }

        [Test]
        [Category("UnitTests")]
        public void Undo_Freeze_OnParentNode_Test()
        {
            var model = CurrentDynamoModel;
            //create some numbers
            var numberNode1 = new DoubleInput();
            numberNode1.Value = "1";
            var numberNode2 = new DoubleInput();
            numberNode2.Value = "2";
            var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));

            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(numberNode1, 0, 0, true, false));
            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(numberNode2, 0, 0, true, false));
            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(addNode, 0, 0, true, false));

            //connect them up
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(numberNode1.GUID, 0, PortType.Output, DynCmd.MakeConnectionCommand.Mode.Begin));
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(addNode.GUID, 0, PortType.Input, DynCmd.MakeConnectionCommand.Mode.End));

            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(numberNode2.GUID, 0, PortType.Output, DynCmd.MakeConnectionCommand.Mode.Begin));
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(addNode.GUID, 1, PortType.Input, DynCmd.MakeConnectionCommand.Mode.End));
            
            //add  a watch node
            var watchNode = new Watch() { X = 100, Y = 300 };
            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(watchNode, 0, 0, true, false));

            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(addNode.GUID, 0, PortType.Output, DynCmd.MakeConnectionCommand.Mode.Begin));
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(watchNode.GUID, 0, PortType.Input, DynCmd.MakeConnectionCommand.Mode.End));

            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count(), 4);
            Assert.AreEqual(model.CurrentWorkspace.Connectors.Count(), 3);

            //check the value
            AssertPreviewValue(addNode.GUID.ToString(), 3);

            //Record for undo.
            model.ExecuteCommand(
                    new DynCmd.UpdateModelValueCommand(
                        Guid.Empty, numberNode1.GUID, "IsFrozen",
                         numberNode1.IsFrozen.ToString()));

            numberNode1.IsFrozen = true;
           
            //Number nodes in frozen and not executing state
            Assert.AreEqual(numberNode1.IsFrozen, true);
           
            //add node in frozen and can execute state
            Assert.AreEqual(addNode.IsFrozen, true);
            
            //check the value
            AssertPreviewValue(addNode.GUID.ToString(), 3);
            Assert.AreEqual(Convert.ToInt32(watchNode.CachedValue), 3);

            //undo the freeze on numbernode1
            model.CurrentWorkspace.Undo();
          
            //Now change the value on number node1
            numberNode1.Value = "3.0";

            //now the first number node unfreeze mode.
            Assert.AreEqual(numberNode1.IsFrozen, false);
           
            //add node in normal state
            Assert.AreEqual(addNode.IsFrozen, false);
           
            //check the value
            AssertPreviewValue(addNode.GUID.ToString(), 5);
            AssertPreviewValue(watchNode.GUID.ToString(), 5);
        }

        [Test]
        [Category("UnitTests")]
        public void File_open_with_freezeNodes_test()
        {
            string openPath = Path.Combine(TestDirectory, @"core\FreezeNodes\TestFrozenState.dyn");
            RunModel(openPath);

            //check the upstream node is explicitly frozen
            var inputNode = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<DoubleInput>("13bd151a-f5b6-4af7-ac20-a7121cc0d830");
            Assert.AreEqual(true, inputNode.IsFrozen);

            //because the upstream node is frozen, the downstream node should be in frozen state
            var add = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<DSFunction>("44f77917-ce7a-404f-bf70-6972c9276c02");
            Assert.AreEqual(true, add.IsFrozen);
        }
    }
}
