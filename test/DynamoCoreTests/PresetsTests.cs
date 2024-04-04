using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CoreNodeModels.Input;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.Selection;
using NUnit.Framework;
using DynCmd = Dynamo.Models.DynamoModel;

namespace Dynamo.Tests
{
    internal class PresetsTests : Dynamo.DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("VMDataBridge.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        private List<NodeModel> SetupNumberNodesAndPresets()
        {
            var model = CurrentDynamoModel;
            //create some numbers
            var numberNode1 = new DoubleInput();
            numberNode1.Value = "1";
            var numberNode2 = new DoubleInput();
            numberNode2.Value = "2";
            var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));

            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(numberNode1,0,0,true,false));
            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(numberNode2, 0, 0, true, false));
            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(addNode, 0, 0, true, false));

            //connect them up
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(numberNode1.GUID,0,PortType.Output,DynCmd.MakeConnectionCommand.Mode.Begin));
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(addNode.GUID,0,PortType.Input,DynCmd.MakeConnectionCommand.Mode.End));

            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(numberNode2.GUID,0,PortType.Output,DynCmd.MakeConnectionCommand.Mode.Begin));
            model.ExecuteCommand(new DynamoModel.MakeConnectionCommand(addNode.GUID,1,PortType.Input,DynCmd.MakeConnectionCommand.Mode.End));

            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count(), 3);
            Assert.AreEqual(model.CurrentWorkspace.Connectors.Count(), 2);

            DynamoSelection.Instance.ClearSelection();
            //create the first state with the numbers selected
            DynamoSelection.Instance.Selection.Add(numberNode1);
            DynamoSelection.Instance.Selection.Add(numberNode2);
            var ids = DynamoSelection.Instance.Selection.OfType<NodeModel>().Select(x => x.GUID).ToList();
            //create the preset from 2 nodes
            model.ExecuteCommand(new DynamoModel.AddPresetCommand("state1", "3", ids));

            //change values
            numberNode1.Value = "2";
            numberNode2.Value = "3";

            DynamoSelection.Instance.ClearSelection();
            DynamoSelection.Instance.Selection.Add(numberNode1);
            DynamoSelection.Instance.Selection.Add(numberNode2);
            ids = DynamoSelection.Instance.Selection.OfType<NodeModel>().Select(x => x.GUID).ToList();

            model.ExecuteCommand(new DynamoModel.AddPresetCommand("state2", "5", ids));

            return new List<NodeModel>() { numberNode1, numberNode2,addNode };
        }

        [Test]
        [Category("UnitTests")]
        public void CanSavePinState()
        {
            var model = CurrentDynamoModel;
            var cbn = new CodeBlockNodeModel(model.LibraryServices);
            var command = new DynamoModel.CreateNodeCommand(cbn, 0, 0, true, false);

            CurrentDynamoModel.ExecuteCommand(command);

            UpdateCodeBlockNodeContent(cbn, "42");
            cbn.PreviewPinned = true;

            DynamoSelection.Instance.Selection.Add(cbn);
            var ids = DynamoSelection.Instance.Selection.OfType<NodeModel>().Select(x => x.GUID).ToList();
            model.ExecuteCommand(new DynamoModel.AddPresetCommand("state1", "3", ids));

            UpdateCodeBlockNodeContent(cbn, "146");
            DynamoSelection.Instance.Selection.Remove(cbn);

            model.CurrentWorkspace.ApplyPreset(model.CurrentWorkspace.Presets.Where(
               x => x.Name == "state1").First());

            RunCurrentModel();

            Assert.IsTrue(cbn.PreviewPinned);
        }

        private void UpdateCodeBlockNodeContent(CodeBlockNodeModel cbn, string value)
        {
            var command = new DynCmd.UpdateModelValueCommand(Guid.Empty, cbn.GUID, "Code", value);
            CurrentDynamoModel.ExecuteCommand(command);
        }


        [Test]
        [Category("UnitTests")]
        public void CanAddPresetState()
        {
            var nodes = SetupNumberNodesAndPresets();
            var model = CurrentDynamoModel;

            Assert.AreEqual(model.CurrentWorkspace.Presets.Count(), 2);
            Assert.AreEqual(model.CurrentWorkspace.Presets.First().Nodes.Count(), 2);
        }

        [Test]
        [Category("UnitTests")]
        public void CanDeletePresetState()
        {
            var nodes = SetupNumberNodesAndPresets();
            var model = CurrentDynamoModel;

            Assert.AreEqual(model.CurrentWorkspace.Presets.Count(), 2);
            Assert.AreEqual(model.CurrentWorkspace.Presets.First().Nodes.Count(), 2);

            //delete state
            var state = model.CurrentWorkspace.Presets.First();
            model.CurrentWorkspace.RemovePreset(state);

           Assert.AreEqual(model.CurrentWorkspace.Presets.Count(), 1);

        }


        [Test]
        [Category("UnitTests")]
        public void CanAddAndRestoreState()
        {
            var nodes = SetupNumberNodesAndPresets();
            var model = CurrentDynamoModel;
            var addNode = nodes[2];

            //now restore state to state 1
           model.CurrentWorkspace.ApplyPreset(model.CurrentWorkspace.Presets.Where(
               x => x.Name == "state1").First());

           RunCurrentModel();
           Thread.Sleep(250);
              //assert that the value of the add node is 3
           AssertPreviewValue(addNode.GUID.ToString(), 3);

           //now restore state to state 2
           model.CurrentWorkspace.ApplyPreset(model.CurrentWorkspace.Presets.Where(
               x => x.Name == "state2").First());

           RunCurrentModel();
           Thread.Sleep(250);
           //assert that the value of the add node is 5
           AssertPreviewValue(nodes[2].GUID.ToString(), 5);

        }

        [Test]
        public void CanRestoreStateAndUndo()
        {
            var nodes = SetupNumberNodesAndPresets();
            var model = CurrentDynamoModel;
            var addNode = nodes[2];


            //now restore state to state 1
            model.CurrentWorkspace.ApplyPreset(model.CurrentWorkspace.Presets.Where(
                x => x.Name == "state1").First());

            RunCurrentModel();
            Thread.Sleep(250);
            //assert that the value of the add node is 3
            AssertPreviewValue(addNode.GUID.ToString(), 3);

            //now restore state to state 2
            model.CurrentWorkspace.ApplyPreset(model.CurrentWorkspace.Presets.Where(
                x => x.Name == "state2").First());

            RunCurrentModel();
            Thread.Sleep(250);
            //assert that the value of the add node is 5
            AssertPreviewValue(addNode.GUID.ToString(), 5);

            //now undo the first setWorkSpace call
            var undoOperation = DynCmd.UndoRedoCommand.Operation.Undo;
            model.ExecuteCommand(new DynCmd.UndoRedoCommand(undoOperation));

            RunCurrentModel();
            Thread.Sleep(250);
            //assert that the value of the add node is 3, back to the first state
            AssertPreviewValue(addNode.GUID.ToString(), 3);

        }

        [Test]
        public void CanRestoreStateAndUndoAndRedo()
        {
            var nodes = SetupNumberNodesAndPresets();
            var model = CurrentDynamoModel;

            var addNode = nodes[2];

            //now restore state to state 1
            model.CurrentWorkspace.ApplyPreset(model.CurrentWorkspace.Presets.Where(
                x => x.Name == "state1").First());

            RunCurrentModel();
            Thread.Sleep(250);
            //assert that the value of the add node is 3
            AssertPreviewValue(addNode.GUID.ToString(), 3);

            //now restore state to state 2
            model.CurrentWorkspace.ApplyPreset(model.CurrentWorkspace.Presets.Where(
                x => x.Name == "state2").First());

            RunCurrentModel();
            Thread.Sleep(250);
            //assert that the value of the add node is 5
            AssertPreviewValue(addNode.GUID.ToString(), 5);

            //now undo the first setWorkSpace call
            var undoOperation = DynCmd.UndoRedoCommand.Operation.Undo;
            model.ExecuteCommand(new DynCmd.UndoRedoCommand(undoOperation));

            RunCurrentModel();
            Thread.Sleep(250);
            //assert that the value of the add node is 3, back to the first state
            AssertPreviewValue(addNode.GUID.ToString(), 3);

            //now redo the second setWorkSpace call
            var redoOperation = DynCmd.UndoRedoCommand.Operation.Redo;
            model.ExecuteCommand(new DynCmd.UndoRedoCommand(redoOperation));

            RunCurrentModel();
            Thread.Sleep(250);
            //assert that the value of the add node is 5, back to the second state
            AssertPreviewValue(addNode.GUID.ToString(), 5);

        }

        [Test]
        public void CanLoadFileWithGoodPresetsObject()
        {
            var model = CurrentDynamoModel;
            string openPath = Path.Combine(TestDirectory, @"core\PresetStates\GoodStates.dyn");
            OpenModel(openPath);
           
            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(model.CurrentWorkspace.Presets.Count(), 5);

        }

        [Test]
        public void CanLoadFileWithNoPresetsObject()
        {
            var model = CurrentDynamoModel;
            string openPath = Path.Combine(TestDirectory, @"core\PresetStates\NoStates.dyn");
            OpenModel(openPath);
           
            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(model.CurrentWorkspace.Presets.Count(), 0);

        }

        //this attempts to load some good states and some states with bad GUIDS, 
        [Test]
        public void CanLoadFileWithMalformedPresetsObject()
        {
            var model = CurrentDynamoModel;
            string openPath = Path.Combine(TestDirectory, @"core\PresetStates\MalformedStates.dyn");
            Assert.DoesNotThrow(() => {OpenModel(openPath);});

            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count());
              //assert all the states are loadead even if the GUID and nodetypes are malformed
            Assert.AreEqual(model.CurrentWorkspace.Presets.Count(), 5);

        }

        [Test]
        public void CanLoadFromDynWithMissingNodes()
          {
              var model = CurrentDynamoModel;
              string openPath = Path.Combine(TestDirectory, @"core\PresetStates\MissingNodesInGraph.dyn");
              Assert.DoesNotThrow(() => { OpenModel(openPath); });

              Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count());
              Assert.AreEqual(model.CurrentWorkspace.Presets.Count(), 5);

              var stateWithMissingNodes = model.CurrentWorkspace.Presets.Where(x => x.Name == "4/8/2015 2:57:02 PM").First();
            //get all the serialized nodes that were loaded when the state was loaded
             var nodesInState = stateWithMissingNodes.SerializedNodes.Select(x=>Guid.Parse(x.GetAttribute("guid")));
            //now assert that one or more of the ids in the state is missing from the ids in the graph
             var nodesInGraph = model.CurrentWorkspace.Nodes.Select(x => x.GUID);
             var nodesInStateAndGraph = nodesInState.Intersect(nodesInGraph);
           
              Assert.IsTrue(nodesInState.Count() > nodesInStateAndGraph.Count());

              Assert.DoesNotThrow(() => { model.CurrentWorkspace.ApplyPreset(stateWithMissingNodes); });

          }

        [Test]
        public void CanRestoreStateInGraphThatIsMissingNodes()
        {
            var nodes = SetupNumberNodesAndPresets();
            var model = CurrentDynamoModel;
            var numberNode1 = nodes[0];
            DoubleInput numberNode2 = nodes[1] as DoubleInput;
            
            model.CurrentWorkspace.RemoveAndDisposeNode(numberNode1);

            //now restore state to state 1
            Assert.DoesNotThrow(() =>
            {
                model.CurrentWorkspace.ApplyPreset(model.CurrentWorkspace.Presets.Where(
                    x => x.Name == "state1").First());
            });

            //now check that numbernode2 has been set to correct value in state 1
            Assert.AreEqual(numberNode2.Value, "2");
            //check that node 1 has actually been deleted
            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count(), 2);
        }

        [Test]
        public void CanCreateStatesAndSave()
          {
              var nodes = SetupNumberNodesAndPresets();
              var model = CurrentDynamoModel;

              //save these states
              var newPath = GetNewFileNameOnTempPath("dyn");
              model.CurrentWorkspace.Save(newPath);

              Assert.IsTrue(File.Exists(newPath));
          }

        [Test, Ignore("unknown reason")]
        public void CanSaveAndLoadStateWithMissingNodesWithoutLosingThem()
        {
            var model = CurrentDynamoModel;
            string openPath = Path.Combine(TestDirectory, @"core\PresetStates\GoodStates.dyn");
            OpenModel(openPath);

            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(model.CurrentWorkspace.Presets.Count(), 5);

            var allSerializedNodesInAllStates = model.CurrentWorkspace.Presets.SelectMany(x => x.SerializedNodes);

            //now delete a node that is included in the state
            var num1 = model.CurrentWorkspace.Nodes.OfType<DoubleInput>().First();
            model.CurrentWorkspace.RemoveAndDisposeNode(num1);
            //then save this dyn
            
            var newPath = GetNewFileNameOnTempPath("dyn");
            model.CurrentWorkspace.Save(newPath);

            Assert.IsTrue(File.Exists(newPath));
            //and open this new dyn
            OpenModel(newPath);
            //now only 4 nodes
            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(model.CurrentWorkspace.Presets.Count(), 5);
            
            //make sure we have the same number of serialized nodes in this newly loaded presets state
            var allSerializedNodesInAllStates2 = model.CurrentWorkspace.Presets.SelectMany(x => x.SerializedNodes);
            Assert.AreEqual(allSerializedNodesInAllStates.Count(), allSerializedNodesInAllStates2.Count());
        }

        [Test]
        public void CanRestoreStateAndNodesDoNotMove()
        {
            var nodes = SetupNumberNodesAndPresets();
            var model = CurrentDynamoModel;
            var numberNode1 = nodes[0];
            var numberNode2 = nodes[1];
            

            Assert.AreEqual(numberNode1.X, 0);
            Assert.AreEqual(numberNode1.Y, 0);
            Assert.AreEqual(numberNode2.X, 0);
            Assert.AreEqual(numberNode2.Y, 0);

            // move the nodes
            numberNode1.X = 10;
            numberNode1.Y = 10;

            numberNode2.X = 20;
            numberNode2.Y = 20;
            //set the state back before 
            model.CurrentWorkspace.ApplyPreset(model.CurrentWorkspace.Presets.Where(
                   x => x.Name == "state1").First());

            Assert.AreEqual(numberNode1.X, 10);
            Assert.AreEqual(numberNode1.Y, 10);
            Assert.AreEqual(numberNode2.X, 20);
            Assert.AreEqual(numberNode2.Y, 20);
        }

        [Test]
        public void CloseWorkspaceShouldClearPresets()
        {
            var nodes = SetupNumberNodesAndPresets();
            var model = CurrentDynamoModel;

            //Clear the current workspace
            model.ClearCurrentWorkspace();
            
            //Clearing the workspace should clear the presets
           Assert.AreEqual(0, model.CurrentWorkspace.Presets.Count());
        }

        [Test]
        [Category("UnitTests")]
        public void AddPresetShouldSetDirtyFlag()

        {
            var model = CurrentDynamoModel;
            //create some numbers
            var numberNode1 = new DoubleInput();
            numberNode1.Value = "1";
            var numberNode2 = new DoubleInput();
            numberNode2.Value = "2";

            var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));
           
            //Check for Dirty flag
            Assert.AreEqual(model.CurrentWorkspace.HasUnsavedChanges,false);

            //add the nodes
            model.CurrentWorkspace.AddAndRegisterNode(numberNode1, false);
            model.CurrentWorkspace.AddAndRegisterNode(numberNode2, false);
            model.CurrentWorkspace.AddAndRegisterNode(addNode, false);

            //Check for Dirty flag
            Assert.AreEqual(model.CurrentWorkspace.HasUnsavedChanges, true);

            //Set the dirty flag to false. Mocking the save.
            model.CurrentWorkspace.HasUnsavedChanges = false;
            Assert.AreEqual(model.CurrentWorkspace.HasUnsavedChanges, false);
            
            //connect them up
            ConnectorModel.Make(numberNode1, addNode, 0, 0);
            ConnectorModel.Make(numberNode2, addNode, 0, 1);

            //Check for Dirty flag - After the connection the dirty flag should be set.
            Assert.AreEqual(model.CurrentWorkspace.HasUnsavedChanges, true);

            //Set the dirty flag to false. Mocking the save.
            model.CurrentWorkspace.HasUnsavedChanges = false;
            Assert.AreEqual(model.CurrentWorkspace.HasUnsavedChanges, false);

            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count(), 3);
            Assert.AreEqual(model.CurrentWorkspace.Connectors.Count(), 2);

            //create the first state with the numbers selected
            DynamoSelection.Instance.Selection.Add(numberNode1);
            DynamoSelection.Instance.Selection.Add(numberNode2);
            var ids = DynamoSelection.Instance.Selection.OfType<NodeModel>().Select(x => x.GUID).ToList();
            //create the preset from 2 nodes
            model.CurrentWorkspace.AddPreset(
                 "state1",
                 "3", ids);

            Assert.AreEqual(1, model.CurrentWorkspace.Presets.Count());
          
            //change values
            numberNode1.Value = "2";
            numberNode2.Value = "3";

            DynamoSelection.Instance.ClearSelection();
            DynamoSelection.Instance.Selection.Add(numberNode1);
            DynamoSelection.Instance.Selection.Add(numberNode2);
            ids = DynamoSelection.Instance.Selection.OfType<NodeModel>().Select(x => x.GUID).ToList();

            model.CurrentWorkspace.AddPreset(
            "state2",
            "5", ids);

            //Check for Dirty flag - After the Preset the dirty flag should be set.
            Assert.AreEqual(model.CurrentWorkspace.HasUnsavedChanges, true);
        }

        [Test]
        public void CanAddStateAndUndoAndRedo()
        {
            var nodes = SetupNumberNodesAndPresets();
            var model = CurrentDynamoModel;

            //now undo the preset creation call
            var undoOperation = DynCmd.UndoRedoCommand.Operation.Undo;
            model.ExecuteCommand(new DynCmd.UndoRedoCommand(undoOperation));
            
            //assert that 1 preset exists
            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Presets.Count());

            //now redo the presets creation
            var redoOperation = DynCmd.UndoRedoCommand.Operation.Redo;
            model.ExecuteCommand(new DynCmd.UndoRedoCommand(redoOperation));

            //assert that 2 presets exist
            Assert.AreEqual(2,CurrentDynamoModel.CurrentWorkspace.Presets.Count());

        }

        [Test]
        public void CanDeleteStateAndUndoAndRedo()
        {

            var nodes = SetupNumberNodesAndPresets();
            var model = CurrentDynamoModel;

            var preset = CurrentDynamoModel.CurrentWorkspace.Presets.Where(x => x.Name == "state1").First();
            var presetguid = preset.GUID;

            //delete the preset
            model.ExecuteCommand(new DynamoModel.DeleteModelCommand(presetguid));

            //now undo the preset deletion
            var undoOperation = DynCmd.UndoRedoCommand.Operation.Undo;
            model.ExecuteCommand(new DynCmd.UndoRedoCommand(undoOperation));

            //assert that 2 presets exist
            Assert.AreEqual(2, CurrentDynamoModel.CurrentWorkspace.Presets.Count());
            //assert that it has the same GUID as the old preset
            Assert.AreEqual(presetguid, CurrentDynamoModel.CurrentWorkspace.Presets.Where(x=>x.Name == "state1").First().GUID);

            //now redo the presets deletion
            var redoOperation = DynCmd.UndoRedoCommand.Operation.Redo;
            model.ExecuteCommand(new DynCmd.UndoRedoCommand(redoOperation));

            //assert that 1 preset exists
            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Presets.Count());

        }

        [Test]
        public void CanCreateAndApplyPresetWithUndoRedo()
        {
            var nodes = SetupNumberNodesAndPresets();
            var model = CurrentDynamoModel;
            //now restore state to state 1
            model.CurrentWorkspace.ApplyPreset(model.CurrentWorkspace.Presets.Where(
                x => x.Name == "state1").First());

            //now undo the preset application
            model.CurrentWorkspace.UndoRecorder.Undo();

            //assert that 2 presets exist
            Assert.AreEqual(2, CurrentDynamoModel.CurrentWorkspace.Presets.Count());

            model.CurrentWorkspace.UndoRecorder.Undo();
            //assert that 1 presets exist
            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Presets.Count());
            
            model.CurrentWorkspace.UndoRecorder.Undo();

            //assert that 0 presets exist
            Assert.AreEqual(0, CurrentDynamoModel.CurrentWorkspace.Presets.Count());

            //now redo the presets creation
            var redoOperation = DynCmd.UndoRedoCommand.Operation.Redo;
            model.ExecuteCommand(new DynCmd.UndoRedoCommand(redoOperation));

            //now redo the presets creation
            model.ExecuteCommand(new DynCmd.UndoRedoCommand(redoOperation));

            //now redo the presets application
            model.ExecuteCommand(new DynCmd.UndoRedoCommand(redoOperation));

            //assert that 2 presets exist
            Assert.AreEqual(2, CurrentDynamoModel.CurrentWorkspace.Presets.Count());

            //now restore state to state 2
            Assert.DoesNotThrow(() => { model.CurrentWorkspace.ApplyPreset(model.CurrentWorkspace.Presets.Where(
                x => x.Name == "state2").First());});

            //assert that the value of the add node is 5
            AssertPreviewValue(nodes[2].GUID.ToString(), 5);

        }

        [Test]
        public void CanApplyPresetFromUndoRedoNodes()
        {

            var nodes = SetupNumberNodesAndPresets();
            var model = CurrentDynamoModel;

            //presets
            model.CurrentWorkspace.UndoRecorder.Undo();
            model.CurrentWorkspace.UndoRecorder.Undo();
            //nodes
            model.CurrentWorkspace.UndoRecorder.Undo();
            model.CurrentWorkspace.UndoRecorder.Undo();
            model.CurrentWorkspace.UndoRecorder.Undo();
            //connections
            model.CurrentWorkspace.UndoRecorder.Undo();
            model.CurrentWorkspace.UndoRecorder.Undo();
            model.CurrentWorkspace.UndoRecorder.Undo();
            model.CurrentWorkspace.UndoRecorder.Undo();


            //assert that 0 nodes and 0 presets exist
            Assert.AreEqual(0, CurrentDynamoModel.CurrentWorkspace.Presets.Count());
            Assert.AreEqual(0, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            //now redo the connector creation
            model.CurrentWorkspace.UndoRecorder.Redo();
            model.CurrentWorkspace.UndoRecorder.Redo();
            model.CurrentWorkspace.UndoRecorder.Redo();
            model.CurrentWorkspace.UndoRecorder.Redo();

            //now redo the node creation
            model.CurrentWorkspace.UndoRecorder.Redo();
            model.CurrentWorkspace.UndoRecorder.Redo();
            model.CurrentWorkspace.UndoRecorder.Redo();

            //now redo the presets creation
            model.CurrentWorkspace.UndoRecorder.Redo();
            model.CurrentWorkspace.UndoRecorder.Redo();



            //assert that 2 presets exist
            Assert.AreEqual(2, CurrentDynamoModel.CurrentWorkspace.Presets.Count());
            //assert that there are 3 nodes
            Assert.AreEqual(3, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            //make sure we recreated the connectors as well during redo
            Assert.AreEqual(2, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            //assert that a preset contains 2 nodes
            Assert.AreEqual(2, CurrentDynamoModel.CurrentWorkspace.Presets.Where(x=>x.Name == "state1").First().Nodes.Count());
            //assert that a preset contains 2 nodes
            Assert.AreEqual(2, CurrentDynamoModel.CurrentWorkspace.Presets.Where(x => x.Name == "state2").First().Nodes.Count());

            //now restore state to state 2
            Assert.DoesNotThrow(() =>
            {
                model.CurrentWorkspace.ApplyPreset(model.CurrentWorkspace.Presets.Where(
                    x => x.Name == "state2").First());
            });

            RunCurrentModel();
            Thread.Sleep(250);
            //assert that the value of the numbeer node is 2
            AssertPreviewValue(nodes[0].GUID.ToString(), 2);

            //assert that the value of the add node is 5
            AssertPreviewValue(nodes[2].GUID.ToString(),5);
           
            //now restore state to state 1
            Assert.DoesNotThrow(() =>
            {
                model.CurrentWorkspace.ApplyPreset(model.CurrentWorkspace.Presets.Where(
                    x => x.Name == "state1").First());
            });

            RunCurrentModel();
            Thread.Sleep(250);

            AssertPreviewValue(nodes[2].GUID.ToString(), 3);

        }
    }
}
