using System.Linq;
using CoreNodeModels.Input;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Models;
using Dynamo.Selection;
using Dynamo.Tests;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    class PresetViewModelTest :  DynamoViewModelUnitTest
    {
        [Test]
        public void TogglePresetOptionVisibility()
        {
            //Create a Node
            var addNode = new DSFunction(ViewModel.Model.LibraryServices.GetFunctionDescriptor("+"));
            ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(addNode, false);

            //verify the node was created
            Assert.AreEqual(1, ViewModel.Model.CurrentWorkspace.Nodes.Count());

            //Check the Preset option visibility.
            Assert.AreEqual(false, ViewModel.EnablePresetOptions);

            DynamoSelection.Instance.Selection.Add(addNode);

            var ids = DynamoSelection.Instance.Selection.OfType<NodeModel>().Select(x => x.GUID).ToList();
            //create the preset from 2 nodes
            ViewModel.Model.CurrentWorkspace.AddPreset(
                "state1",
                "a state with 2 numbers", ids);
            //assert that the preset has been created
            Assert.AreEqual(ViewModel.Model.CurrentWorkspace.Presets.Count(), 1);
            Assert.AreEqual(ViewModel.Model.CurrentWorkspace.Presets.First().Nodes.Count(), 1);

            //Check the Preset option visibility.
            Assert.AreEqual(true, ViewModel.EnablePresetOptions);

            //Delete the preset
            //delete state
            var state = ViewModel.Model.CurrentWorkspace.Presets.First();
            ViewModel.Model.CurrentWorkspace.RemovePreset(state);

            Assert.AreEqual(ViewModel.Model.CurrentWorkspace.Presets.Count(), 0);

            //Check the Preset option visibility.
            Assert.AreEqual(false, ViewModel.EnablePresetOptions);
        }

        [Test]
        public void TooglePresetVisibilityWithUndoRedo()
        {
            //Create a Node
            var addNode = new DSFunction(ViewModel.Model.LibraryServices.GetFunctionDescriptor("+"));
            ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(addNode, false);

            //verify the node was created
            Assert.AreEqual(1, ViewModel.Model.CurrentWorkspace.Nodes.Count());

            //Check the Preset option visibility.
            Assert.AreEqual(false, ViewModel.EnablePresetOptions);

            DynamoSelection.Instance.Selection.Add(addNode);

            var ids = DynamoSelection.Instance.Selection.OfType<NodeModel>().Select(x => x.GUID).ToList();
            //create the preset from 2 nodes
            ViewModel.Model.ExecuteCommand(new DynamoModel.AddPresetCommand("state1", "a state with 2 numbers", ids));
            
            //assert that the preset has been created
            Assert.AreEqual(ViewModel.Model.CurrentWorkspace.Presets.Count(), 1);
            Assert.AreEqual(ViewModel.Model.CurrentWorkspace.Presets.First().Nodes.Count(), 1);

            //Check the Preset option visibility.
            Assert.AreEqual(true, ViewModel.EnablePresetOptions);

            //undo the preset creation
            ViewModel.CurrentSpace.UndoRecorder.Undo();

            Assert.AreEqual(ViewModel.Model.CurrentWorkspace.Presets.Count(), 0);

            //Check the Preset option visibility.
            Assert.AreEqual(false, ViewModel.EnablePresetOptions);

            //redo the preset creation
            ViewModel.CurrentSpace.UndoRecorder.Redo();

            Assert.AreEqual(ViewModel.Model.CurrentWorkspace.Presets.Count(), 1);

            //Check the Preset option visibility.
            Assert.AreEqual(true, ViewModel.EnablePresetOptions);

        }


        [Test]
        public void CanCreatePreset()
        {
            //Create a Node
            var numberNode = new DoubleInput();
            numberNode.Value = "1";
            ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(numberNode, false);

            //verify the node was created
            Assert.AreEqual(1, ViewModel.Model.CurrentWorkspace.Nodes.Count());
          
            DynamoSelection.Instance.Selection.Add(numberNode);

            //Check for input nodes
            Assert.AreEqual(true, ViewModel.GetInputNodesFromSelectionForPresets().Any());

            var addNode = new DSFunction(ViewModel.Model.LibraryServices.GetFunctionDescriptor("+"));
            ViewModel.Model.CurrentWorkspace.AddAndRegisterNode(addNode, false);

            DynamoSelection.Instance.ClearSelection();

            DynamoSelection.Instance.Selection.Add(addNode);

            Assert.AreEqual(false, ViewModel.GetInputNodesFromSelectionForPresets().Any());

            DynamoSelection.Instance.Selection.Add(numberNode);

            //Check for input nodes
            Assert.AreEqual(true, ViewModel.GetInputNodesFromSelectionForPresets().Any());

        }
    }
}
