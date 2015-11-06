using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Selection;
using NUnit.Framework;

using DynCmd = Dynamo.Models.DynamoModel;

namespace Dynamo.Tests
{
    internal class NodeExecutionTest : DynamoModelTestBase
    {
        [Test]
        [Category("UnitTests")]
        public void FreezeANodeTest()
        {
            //Add a Node
            var model = CurrentDynamoModel;
            var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));
            model.CurrentWorkspace.AddAndRegisterNode(addNode, false);

            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count(), 1);            
            Assert.AreEqual(addNode.IsFrozen, false);

            addNode.IsFrozen = true;
            model.CurrentWorkspace.ComputeRunStateOfTheNodes(addNode);

            Assert.AreEqual(addNode.IsFrozen, true);
        }

        [Test]
        [Category("UnitTests")]
        public void ParentNodeFreezeTest()
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

            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count(), 3);
            Assert.AreEqual(model.CurrentWorkspace.Connectors.Count(), 2);

            //Check the value
            AssertPreviewValue(addNode.GUID.ToString(),3);

            //Freeeze the numbernode1 and compute the run state of other nodes.           
            numberNode1.IsFrozen = true;
            model.CurrentWorkspace.ComputeRunStateOfTheNodes(numberNode1);

            //change the value of number node1.
            numberNode1.Value = "3.0";

            // the number node must be frozen and not executing state
            Assert.AreEqual(numberNode1.IsFrozen, true);
            Assert.AreEqual(numberNode1.CanExecute, false);

            //the add node must be frozen and can execute state
            Assert.AreEqual(addNode.IsFrozen,true);
            Assert.AreEqual(addNode.CanExecute, true);

            //Since the nodes are frozen, the value  of add node should not change.            
            AssertPreviewValue(addNode.GUID.ToString(), 3);
        }

        [Test]
        [Category("UnitTests")]
        public void ParentNodeEntersTemporaryRunStateTest()
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

            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count(), 3);
            Assert.AreEqual(model.CurrentWorkspace.Connectors.Count(), 2);
            
            //Check the value
            AssertPreviewValue(addNode.GUID.ToString(),3);

            //freeze add node
            addNode.IsFrozen = true;
            model.CurrentWorkspace.ComputeRunStateOfTheNodes(addNode);
            
            // the add node must be frozen and not executing state
            Assert.AreEqual(addNode.IsFrozen, true);
            Assert.AreEqual(addNode.CanExecute, false);

            //freeze number node.
            numberNode1.IsFrozen = true;
            model.CurrentWorkspace.ComputeRunStateOfTheNodes(numberNode1);

            //change the value on number node 1
            numberNode1.Value = "3.0";

            // the number node must be frozen and not executing state
            Assert.AreEqual(numberNode1.IsFrozen, true);
            Assert.AreEqual(numberNode1.CanExecute, false);

            // the add node must be frozen and in a temporary state
            Assert.AreEqual(addNode.IsFrozen, true);
            Assert.AreEqual(addNode.CanExecute, null);

            //Since the add node is frozen, the value should not change
            AssertPreviewValue(addNode.GUID.ToString(), 3);
        }

        [Test]
        [Category("UnitTests")]
        public void UnFreezeParentNodeUnfreezesChildNodes()
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

            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count(), 3);
            Assert.AreEqual(model.CurrentWorkspace.Connectors.Count(), 2);

            //check the value
            AssertPreviewValue(addNode.GUID.ToString(), 3);

            //Add the number node to selection and call run state on the workspace.
            //this should freeze the number node.
            numberNode1.IsFrozen = true;
            model.CurrentWorkspace.ComputeRunStateOfTheNodes(numberNode1);

            // the number node must be frozen and not executing state
            Assert.AreEqual(numberNode1.IsFrozen, true);
            Assert.AreEqual(numberNode1.CanExecute, false);

            //the add node must be frozen and can execute state
            Assert.AreEqual(addNode.IsFrozen, true);
            Assert.AreEqual(addNode.CanExecute, true);

            //check the value
            AssertPreviewValue(addNode.GUID.ToString(), 3);

            //now the number node1 is frozen. change the value.
            numberNode1.Value = "3.0";

            //check the value of add node. it should not change.
            AssertPreviewValue(addNode.GUID.ToString(), 3);

            //unfreeze the input node
            numberNode1.IsFrozen = false;
            model.CurrentWorkspace.ComputeRunStateOfTheNodes(numberNode1);

            Assert.AreEqual(numberNode1.IsFrozen, false);
            
            //Now the add node should be in unfreeze state
            Assert.AreEqual(addNode.IsFrozen, false);

            //Now the add node should get the value
            AssertPreviewValue(addNode.GUID.ToString(), 5);
        }

        [Test]
        [Category("UnitTests")]
        public void UnfreezeParentNodeMakesATemporaryStateNodeSwitchBackToPreviousState()
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

            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count(), 3);
            Assert.AreEqual(model.CurrentWorkspace.Connectors.Count(), 2);

            //check the value
            AssertPreviewValue(addNode.GUID.ToString(), 3);

            //freeze add node
            addNode.IsFrozen = true;
            model.CurrentWorkspace.ComputeRunStateOfTheNodes(addNode);

            // the add node must be frozen and not executing state
            Assert.AreEqual(addNode.IsFrozen, true);
            Assert.AreEqual(addNode.CanExecute, false);

            //freeze number node.
            numberNode1.IsFrozen = true;
            model.CurrentWorkspace.ComputeRunStateOfTheNodes(numberNode1);

            // the number node must be frozen and not executing state
            Assert.AreEqual(numberNode1.IsFrozen, true);
            Assert.AreEqual(numberNode1.CanExecute, false);

            //change the value on number node 1
            numberNode1.Value = "3.0";

            // the add node must be frozen and in a temporary state
            Assert.AreEqual(addNode.IsFrozen, true);
            Assert.AreEqual(addNode.CanExecute, null);

            //check the value on add node.
            AssertPreviewValue(addNode.GUID.ToString(), 3);

            //now unfreeze number node.
            numberNode1.IsFrozen = false;
            model.CurrentWorkspace.ComputeRunStateOfTheNodes(numberNode1);

            //now number node is not frozen
            Assert.AreEqual(numberNode1.IsFrozen, false);

            //this causes the add node to switch back from temporary state
            //to frozen and not executing state
            Assert.AreEqual(addNode.IsFrozen, true);
            Assert.AreEqual(addNode.CanExecute, false);

            //check the value on add node. 
            AssertPreviewValue(addNode.GUID.ToString(), 5);
        }

        [Test]
        [Category("UnitTests")]
        public void NodeAttachedTThatHasParentsShouldNotUnfreezeUntilAllParentsUnfreeze()
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

            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count(), 3);
            Assert.AreEqual(model.CurrentWorkspace.Connectors.Count(), 2);

            //check the value.
            AssertPreviewValue(addNode.GUID.ToString(), 3);

            //now freeze both the number nodes.
            numberNode1.IsFrozen = true;
            model.CurrentWorkspace.ComputeRunStateOfTheNodes(numberNode1);

            numberNode2.IsFrozen = true;
            model.CurrentWorkspace.ComputeRunStateOfTheNodes(numberNode2);

            //Number nodes in frozen and not executing state
            Assert.AreEqual(numberNode1.IsFrozen , true);
            Assert.AreEqual(numberNode1.CanExecute, false);

            Assert.AreEqual(numberNode2.IsFrozen, true);
            Assert.AreEqual(numberNode2.CanExecute, false);

            //change the value of number nodes
            numberNode1.Value = "3.0";
            numberNode2.Value = "3.0";

            //add node in frozen and can execute state
            Assert.AreEqual(addNode.IsFrozen, true);
            Assert.AreEqual(addNode.CanExecute, true);

            //addnode should not change the value.
            AssertPreviewValue(addNode.GUID.ToString(),3);
            
            //unfreeze one of the number node          
            numberNode1.IsFrozen = false;
            model.CurrentWorkspace.ComputeRunStateOfTheNodes(numberNode1);

            //Number nodes in frozen and not executing state
            Assert.AreEqual(numberNode1.IsFrozen, false);
            Assert.AreEqual(numberNode1.CanExecute, true);

            Assert.AreEqual(numberNode2.IsFrozen, true);
            Assert.AreEqual(numberNode2.CanExecute, false);

            //add node should still be in frozen and can execute state
            Assert.AreEqual(addNode.IsFrozen, true);
            Assert.AreEqual(addNode.CanExecute, true);

            //addnode should change the value.
            AssertPreviewValue(addNode.GUID.ToString(), 5);

            //now unfreeze the other node.
            numberNode2.IsFrozen = false;
            model.CurrentWorkspace.ComputeRunStateOfTheNodes(numberNode2);

            Assert.AreEqual(numberNode2.IsFrozen, false);
            Assert.AreEqual(numberNode2.CanExecute, true);

            //now the add node should not be in frozen state
            Assert.AreEqual(addNode.IsFrozen, false);
            Assert.AreEqual(addNode.CanExecute, true);

            //addnode should change the value now.
            AssertPreviewValue(addNode.GUID.ToString(), 6);
        }

        [Test]
        [Category("UnitTests")]
        public void UndoFreezeOnParentNode()
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

            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count(), 3);
            Assert.AreEqual(model.CurrentWorkspace.Connectors.Count(), 2);

            //check the value
            AssertPreviewValue(addNode.GUID.ToString(), 3);
           
            //Record for undo.
            model.ExecuteCommand(
                    new DynCmd.UpdateModelValueCommand(
                        Guid.Empty, numberNode1.GUID, "IsFrozen",
                         numberNode1.IsFrozen.ToString()));
            numberNode1.IsFrozen = true;
           
            model.CurrentWorkspace.ComputeRunStateOfTheNodes(numberNode1);

            //Number nodes in frozen and not executing state
            Assert.AreEqual(numberNode1.IsFrozen, true);
            Assert.AreEqual(numberNode1.CanExecute, false);

            //add node in frozen and can execute state
            Assert.AreEqual(addNode.IsFrozen, true);
            Assert.AreEqual(addNode.CanExecute, true);

            //check the value
            AssertPreviewValue(addNode.GUID.ToString(), 3);
           
            //undo the freeze on numbernode1
            model.CurrentWorkspace.Undo();
            //since the undo is on model here, calling compute run state
            model.CurrentWorkspace.ComputeRunStateOfTheNodes(numberNode1);
            
            //Now change the value on number node1
            numberNode1.Value = "3.0";
            
            //now the first number node unfreeze mode.
            Assert.AreEqual(numberNode1.IsFrozen, false);
            Assert.AreEqual(numberNode1.CanExecute, true);

            //add node in normal state
            Assert.AreEqual(addNode.IsFrozen, false);
            Assert.AreEqual(addNode.CanExecute, true);

            //check the value
            AssertPreviewValue(addNode.GUID.ToString(), 5);

        }
      
    }
}
