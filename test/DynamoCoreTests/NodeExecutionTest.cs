using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoreNodeModels;
using CoreNodeModels.Input;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Models;
using NUnit.Framework;

using DynCmd = Dynamo.Models.DynamoModel;

namespace Dynamo.Tests
{
    internal class NodeExecutionTest : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("VMDataBridge.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("ProtoGeometry.dll");
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
            
            // For the new model, as freezing a node is equivallent to delete
            // a node, the node will be nullified.
            //
            // On the UI, the node may choose to display the same previous
            // value as before, but its real value *should* have been changed.
            AssertPreviewValue(addNode.GUID.ToString(), 0);
            Assert.IsNull(addNode.CachedValue.Data);
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

        [Test]
        [Category("UnitTests")]
        public void File_open_with_all_nodes_Frozen()
        {
            string openPath = Path.Combine(TestDirectory, @"core\FreezeNodes\TestFrozenStateAllNodes.dyn");
            RunModel(openPath);

            //check the upstream node is explicitly frozen
            var inputNode1 = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<DoubleInput>("13bd151a-f5b6-4af7-ac20-a7121cc0d830");
            Assert.AreEqual(true, inputNode1.IsFrozen);

            var inputNode2 = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<DoubleInput>("9c75fe7e-976b-4d9b-a7ff-e5dfb4e50a4b");
            Assert.AreEqual(true, inputNode2.IsFrozen);

            //because the upstream node is frozen, the downstream node should be in frozen state
            var add = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<DSFunction>("44f77917-ce7a-404f-bf70-6972c9276c02");
            Assert.AreEqual(true, add.IsFrozen);

            //check the value on Add Node
            AssertPreviewValue(add.GUID.ToString(), null);

            //now change the property on inputnode1 and inputnode2.
            inputNode1.IsFrozen = false;
            inputNode2.IsFrozen = false;

            //now all the nodes are in unfreeze state
            Assert.AreEqual(false, inputNode1.IsFrozen);
            Assert.AreEqual(false, inputNode2.IsFrozen);
            Assert.AreEqual(false, add.IsFrozen);

            //check the value on Add Node
            AssertPreviewValue(add.GUID.ToString(), 8);
        }

        [Test]
        [Category("UnitTests")]
        public void Check_ConnectingFrozenStartNodeUpdatesEndNodeState()
        {
            CreateAndConnectNodes();
            CurrentDynamoModel.ClearCurrentWorkspace();

            // check if after clearing end node is still able to be updated 
            // by adding a frozen input connector
            CreateAndConnectNodes();
        }

        private void CreateAndConnectNodes()
        {
            var model = CurrentDynamoModel;
            //create a number
            var numberNode = new DoubleInput();
            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(numberNode, 0, 0, true, false));

            //add  a watch node
            var watchNode = new Watch();
            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(watchNode, 0, 0, true, false));

            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count(), 2);

            numberNode.IsFrozen = true;
            Assert.IsTrue(numberNode.isFrozenExplicitly);
            Assert.IsFalse(watchNode.IsFrozen);

            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(numberNode.GUID, 0, PortType.Output, DynCmd.MakeConnectionCommand.Mode.Begin));
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(watchNode.GUID, 0, PortType.Input, DynCmd.MakeConnectionCommand.Mode.End));

            Assert.AreEqual(model.CurrentWorkspace.Connectors.Count(), 1);

            // check if watch node freeze state is updated
            var msg = "End node freeze state has not been updated";
            Assert.IsTrue(watchNode.IsFrozen, msg);
            Assert.IsFalse(watchNode.isFrozenExplicitly, msg); 
        }

        [Test]
        [Category("UnitTests")]
        public void Freeze_Geometry_Test()
        {
            string openPath = Path.Combine(TestDirectory, @"core\FreezeNodes\TestFrozenOnGeometry.dyn");
            RunModel(openPath);

            //check freeze on Number node
            var numberNode = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<DoubleInput>("40296ab0-b354-46ab-a20d-3439854e6558");
            numberNode.IsFrozen = true;
            Assert.IsTrue(numberNode.isFrozenExplicitly);
            numberNode.IsFrozen = false;

            //check freeze on Cube
            var geometryNode = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("c9c7b211-2aee-4712-838e-b9aaacf72153");
            geometryNode.IsFrozen = true;
            Assert.IsTrue(geometryNode.isFrozenExplicitly);
            geometryNode.IsFrozen = false;

            //check freeze on Line
            var lineNode = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("bdb4475c-8c84-435a-a905-af6c7ee656c3");
            lineNode.IsFrozen = true;
            Assert.IsTrue(lineNode.isFrozenExplicitly);
            lineNode.IsFrozen = false;

            //check freeze on Point
            var pointNode = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("64134763-9c2a-4fa1-907f-60b70afe8883");
            pointNode.IsFrozen = true;
            Assert.IsTrue(pointNode.isFrozenExplicitly);
            pointNode.IsFrozen = false;

            //check freeze on codeblock node
            var codeblockNode = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("3ea56fe2-9be1-4d7d-ac7a-61b5275c096d");
            codeblockNode.IsFrozen = true;
            Assert.IsTrue(codeblockNode.isFrozenExplicitly);
            codeblockNode.IsFrozen = false;

            //check freeze on watch node
            var watchNode = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("8c0edf26-d7ab-4114-9fb8-43817e022c38");
            watchNode.IsFrozen = true;
            Assert.IsTrue(watchNode.isFrozenExplicitly);
            watchNode.IsFrozen = false;

            //check freeze on watch3D node
            var watch3DNode = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("79bec369-2077-495a-97a5-013ee15f1425");
            watch3DNode.IsFrozen = true;
            Assert.IsTrue(watch3DNode.isFrozenExplicitly);
            watch3DNode.IsFrozen = false;
        }

        [Test]
        [Category("UnitTests")]
        public void LoadGraphWithFreezeNodes_Defect9274()
        {
            //Load the graph.
            string openPath = Path.Combine(TestDirectory, @"core\FreezeNodes\TestFrozenNodesOnLoading.dyn");
            RunModel(openPath);

            //Check whether the geometry node is frozen
            var node = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("8163332d-21ec-4257-9a5a-0b69462db44f");
            Assert.IsTrue(node.IsFrozen);
            //Frozen nodes should not get involved in execution.
            Assert.IsFalse(node.WasInvolvedInExecution);
        }

        [Test]
        [Category("UnitTests")]
        public void DisConnectANodeRecomputesFrozenNodes_Defect9218()
        {
            //Load the graph.
            string openPath = Path.Combine(TestDirectory, @"core\FreezeNodes\TestFrozenNodesOnDisconnecting.dyn");
            RunModel(openPath);

            //Check whether the geometry node is frozen
            var node = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("ef3eaed0-7a8e-47a9-b06e-416bb30ec72f");
            Assert.IsTrue(node.IsFrozen);

            //disconnect the Slider node
            var sliderNode =
                CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("7f7418c2-598d-4957-981b-aea1761ccc03");
            Assert.IsTrue(sliderNode.IsFrozen);

            Guid start;
            Guid.TryParse("7f7418c2-598d-4957-981b-aea1761ccc03", out start);

            Guid end;
            Guid.TryParse("3f00be88-170f-4ebe-b81a-011c32ed2acb", out end);

            var connector = CurrentDynamoModel.CurrentWorkspace.Connectors.First(
                x => x.Start.Owner.GUID == start &&
                                          x.End.Owner.GUID == end);
            Assert.IsNotNull(connector);
            
            connector.Delete();  
          
            //now the geometry node should not be in freeze state
            Assert.IsFalse(node.IsFrozen);
        }
    }
}
