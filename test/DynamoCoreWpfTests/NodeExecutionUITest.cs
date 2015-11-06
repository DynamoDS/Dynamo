using System.Linq;
using System.Security.RightsManagement;
using System.Windows.Controls;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Selection;
using Dynamo.Tests;
using NUnit.Framework;
using Dynamo.Utilities;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    public class NodeExecutionUITest : DynamoViewModelUnitTest
    {
        /// Below table shows the state of the node.
        ///  -----------------------------------------------------------
        ///  | NodeState                |  RunChecked    |  RunEnabled |
        ///  -----------------------------------------------------------  
        ///  | Frozen / Executing       |       True     |      False  |
        ///  | Frozen / Not Executing   |       False    |      True   |
        ///  | UnFrozen / Executing     |       True     |      True   |
        ///  | Frozen / Null            |       False    |      True   |
        ///  -----------------------------------------------------------
        ///  Frozen/Executing is true for all nodes that are implictly frozen.
        ///  Frozen/ not executing is true for all nodes that are explictly frozen.
        ///  Unfrozen/Executing is the default node state.
        ///  ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        ///  | Frozen / Null state is a temporary state. If a node is explictly       |  
        ///  | set to freeze, and if any other node is trying to unfreeze explcitly   |
        ///  | set node, then the node enters this temporary state. In this state,    |
        ///  | the run state of the node cannot be modified by other nodes            |
        ///  ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ 
        ///  The tests are written around testing these states. The Actual execution tests 
        ///  are written in NodeExecutionTests in DynamoCore.
        #region NodeExecution

        //case 1 : Node in Freeze and Not execute state. True for all parent nodes.
        [Test]
        [Category("DynamoUI")]
        public void NodeInFreezeNotExecuteStateTest()
        {
            //Create a Node
            var addNode = new DSFunction(ViewModel.Model.LibraryServices.GetFunctionDescriptor("+"));
            ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(addNode, false);

            //verify the node was created
            Assert.AreEqual(1, ViewModel.Model.CurrentWorkspace.Nodes.Count());

            //Select the node
            DynamoSelection.Instance.Selection.Add(addNode);

            //Freeze the node
            addNode.IsFrozen = true;
            ViewModel.CurrentSpaceViewModel.Model.ComputeRunStateOfTheNodes(addNode);

            //Check the RUN property. Assuming only one node is selected
            //this property is fetched from Nodeviewmodel. Context Menu on Workspace,
            //Context menu on Node and Edit Menu toolbar refers to the same location.
            Assert.AreEqual(ViewModel.CurrentSpaceViewModel.NodeRunChecked, false);
            Assert.AreEqual(ViewModel.CurrentSpaceViewModel.NodeRunEnabled, true);
        }

        //case 2 : Node in Freeze and execute state. True for all child nodes.
        [Test]
        [Category("DynamoUI")]
        public void NodeInFreezeExecuteStateTest()
        {
            var model = ViewModel.Model;
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
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(numberNode1.GUID, 0, PortType.Output,
                DynamoModel.MakeConnectionCommand.Mode.Begin));
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(addNode.GUID, 0, PortType.Input,
                DynamoModel.MakeConnectionCommand.Mode.End));

            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(numberNode2.GUID, 0, PortType.Output,
                DynamoModel.MakeConnectionCommand.Mode.Begin));
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(addNode.GUID, 1, PortType.Input,
                DynamoModel.MakeConnectionCommand.Mode.End));

            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count(), 3);
            Assert.AreEqual(model.CurrentWorkspace.Connectors.Count(), 2);

            //Now Freeze the NumberNode1.
            numberNode1.IsFrozen = true;
            ViewModel.CurrentSpaceViewModel.Model.ComputeRunStateOfTheNodes(numberNode1);

            //Get the ViewModel of the number node and check the RUN property.
            var numberNodevm = ViewModel.CurrentSpaceViewModel.Nodes.First(x => x.NodeLogic == numberNode1);
            Assert.IsNotNull(numberNodevm);
            Assert.AreEqual(numberNodevm.NodeRunChecked,false);
            Assert.AreEqual(numberNodevm.NodeRunEnabled, true);

            //Get the ViewModel of add node and check the RUN property. This node is a child node of numbernode1.
            //so this node should be in Frozen and Executing state.
            var addNodeVm = ViewModel.CurrentSpaceViewModel.Nodes.First(x => x.NodeLogic == addNode);
            Assert.IsNotNull(addNodeVm);
            Assert.AreEqual(addNodeVm.NodeRunChecked, true);
            Assert.AreEqual(addNodeVm.NodeRunEnabled, false);
        }

        //case 3 : Node in Freeze and temporary state. 
        [Test]
        [Category("DynamoUI")]
        public void NodeInTemporaryFreezeState()
        {
            var model = ViewModel.Model;
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
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(numberNode1.GUID, 0, PortType.Output,
                DynamoModel.MakeConnectionCommand.Mode.Begin));
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(addNode.GUID, 0, PortType.Input,
                DynamoModel.MakeConnectionCommand.Mode.End));

            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(numberNode2.GUID, 0, PortType.Output,
                DynamoModel.MakeConnectionCommand.Mode.Begin));
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(addNode.GUID, 1, PortType.Input,
                DynamoModel.MakeConnectionCommand.Mode.End));

            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count(), 3);
            Assert.AreEqual(model.CurrentWorkspace.Connectors.Count(), 2);

            //Now Freeze the add node. This node has two input nodes. Note that
            //input nodes are not frozen.
            addNode.IsFrozen = true;
            ViewModel.CurrentSpaceViewModel.Model.ComputeRunStateOfTheNodes(addNode);

            //Get the ViewModel of add node and check the RUN property.
            //This node should be in Frozen and not Executing state.
            var addNodeVm = ViewModel.CurrentSpaceViewModel.Nodes.First(x => x.NodeLogic == addNode);
            Assert.IsNotNull(addNodeVm);
            Assert.AreEqual(addNodeVm.NodeRunChecked, false);
            Assert.AreEqual(addNodeVm.NodeRunEnabled, true);

            //Now freeze NumberNode1.
            numberNode1.IsFrozen = true;
            ViewModel.CurrentSpaceViewModel.Model.ComputeRunStateOfTheNodes(numberNode1);

            //Get the ViewModel of add node and check the RUN property.
            //This node should be in Frozen and not Executing state.
            var numberNode1Vm= ViewModel.CurrentSpaceViewModel.Nodes.First(x => x.NodeLogic == numberNode1);
            Assert.IsNotNull(numberNode1Vm);
            Assert.AreEqual(numberNode1Vm.NodeRunChecked, false);
            Assert.AreEqual(numberNode1Vm.NodeRunEnabled, true);

            //Now check the add node. RUN property will be unchecked and disabled.
            Assert.AreEqual(addNodeVm.NodeRunChecked, false);
            Assert.AreEqual(addNodeVm.NodeRunEnabled, false);
        }

        [Test]
        [Category("DynamoUI")]
        public void UndoFreezeTest()
        {
            var model = ViewModel.Model;
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
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(numberNode1.GUID, 0, PortType.Output,
                DynamoModel.MakeConnectionCommand.Mode.Begin));
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(addNode.GUID, 0, PortType.Input,
                DynamoModel.MakeConnectionCommand.Mode.End));

            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(numberNode2.GUID, 0, PortType.Output,
                DynamoModel.MakeConnectionCommand.Mode.Begin));
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(addNode.GUID, 1, PortType.Input,
                DynamoModel.MakeConnectionCommand.Mode.End));

            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count(), 3);
            Assert.AreEqual(model.CurrentWorkspace.Connectors.Count(), 2);

            var addNodeVm = ViewModel.CurrentSpaceViewModel.Nodes.First(x => x.NodeLogic == addNode);
            Assert.IsNotNull(addNodeVm);

            var numberNode1Vm = ViewModel.CurrentSpaceViewModel.Nodes.First(x => x.NodeLogic == numberNode1);
            Assert.IsNotNull(numberNode1Vm);

            var numberNode2Vm = ViewModel.CurrentSpaceViewModel.Nodes.First(x => x.NodeLogic == numberNode2);
            Assert.IsNotNull(numberNode2Vm);

            //freeze number node1.            
            ViewModel.CurrentSpaceViewModel.ComputeRunStateOfTheNodeCommand.Execute(numberNode1);
           
            Assert.AreEqual(numberNode1Vm.NodeRunChecked, false);
            Assert.AreEqual(numberNode1Vm.NodeRunEnabled, true);

            //add node is in frozen executing state
            Assert.AreEqual(addNodeVm.NodeRunChecked, true);
            Assert.AreEqual(addNodeVm.NodeRunEnabled, false);

            //freeze number node2
            ViewModel.CurrentSpaceViewModel.ComputeRunStateOfTheNodeCommand.Execute(numberNode2);

            Assert.AreEqual(numberNode2Vm.NodeRunChecked, false);
            Assert.AreEqual(numberNode2Vm.NodeRunEnabled, true);

            //add node is in frozen executing state
            Assert.AreEqual(addNodeVm.NodeRunChecked, true);
            Assert.AreEqual(addNodeVm.NodeRunEnabled, false);

            ViewModel.CurrentSpace.Undo();

            //numbernode2 unfreeze
            Assert.AreEqual(numberNode2Vm.NodeRunChecked, true);
            Assert.AreEqual(numberNode2Vm.NodeRunEnabled, true);

            //add node is in frozen executing state
            Assert.AreEqual(addNodeVm.NodeRunChecked, true);
            Assert.AreEqual(addNodeVm.NodeRunEnabled, false);

            ViewModel.CurrentSpace.Undo();

            //numbernode1 unfreeze
            Assert.AreEqual(numberNode1Vm.NodeRunChecked, true);
            Assert.AreEqual(numberNode1Vm.NodeRunEnabled, true);

            //add node is in normal state
            Assert.AreEqual(addNodeVm.NodeRunChecked, true);
            Assert.AreEqual(addNodeVm.NodeRunEnabled, true);
            
        }

        #endregion
    }
}
