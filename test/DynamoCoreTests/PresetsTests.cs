using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Selection;
using Dynamo.Utilities;
using NUnit.Framework;
using DynCmd = Dynamo.Models.DynamoModel;

namespace Dynamo.Tests
{
    internal class PresetsTests : Dynamo.DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("VMDataBridge.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        [Category("UnitTests")]
        public void CanAddPresetState()
        {
            var model = CurrentDynamoModel;
            //create some numbers
            var numberNode1 = new DoubleInput();
            numberNode1.Value = "1";
            var numberNode2 = new DoubleInput();
            numberNode2.Value = "2";

            model.CurrentWorkspace.AddNode(numberNode1, false);
            model.CurrentWorkspace.AddNode(numberNode2, false);
       
            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count, 2);

            DynamoSelection.Instance.Selection.Add(numberNode1);
            DynamoSelection.Instance.Selection.Add(numberNode2);
            var IDS = DynamoSelection.Instance.Selection.OfType<NodeModel>().Select(x => x.GUID).ToList();
            //create the preset from 2 nodes
            model.CurrentWorkspace.AddPreset(
                "state1",
                "a state with 2 numbers", IDS);
            //assert that the preset has been created
            Assert.AreEqual(model.CurrentWorkspace.Presets.Count(), 1);
            Assert.AreEqual(model.CurrentWorkspace.Presets.First().Nodes.Count(), 2);
        }

        [Test]
        [Category("UnitTests")]
        public void CanDeletePresetState()
        {
            var model = CurrentDynamoModel;
            //create some numbers
            var numberNode1 = new DoubleInput();
            numberNode1.Value = "1";
            var numberNode2 = new DoubleInput();
            numberNode2.Value = "2";

            model.CurrentWorkspace.AddNode(numberNode1, false);
            model.CurrentWorkspace.AddNode(numberNode2, false);

            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count, 2);

            DynamoSelection.Instance.Selection.Add(numberNode1);
            DynamoSelection.Instance.Selection.Add(numberNode2);
            var IDS = DynamoSelection.Instance.Selection.OfType<NodeModel>().Select(x => x.GUID).ToList();
            //create the preset from 2 nodes
            model.CurrentWorkspace.AddPreset(
                "state1",
                "a state with 2 numbers", IDS);
            //assert that the preset has been created
            Assert.AreEqual(model.CurrentWorkspace.Presets.Count(), 1);
            Assert.AreEqual(model.CurrentWorkspace.Presets.First().Nodes.Count(), 2);

            //delete state
            var state = model.CurrentWorkspace.Presets.First();
           model.CurrentWorkspace.RemoveState(state);

           Assert.AreEqual(model.CurrentWorkspace.Presets.Count(), 0);

        }


        [Test]
        [Category("UnitTests")]
        public void CanAddAndRestoreState()
        {
            var model = CurrentDynamoModel;
            //create some numbers
            var numberNode1 = new DoubleInput();
            numberNode1.Value = "1";
            var numberNode2 = new DoubleInput();
            numberNode2.Value = "2";
            var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));

            //add the nodes
            model.CurrentWorkspace.AddNode(numberNode1, false);
            model.CurrentWorkspace.AddNode(numberNode2, false);
            model.CurrentWorkspace.AddNode(addNode, false);

           //connect them up
           ConnectorModel.Make(numberNode1,addNode, 0, 0);
           ConnectorModel.Make(numberNode2,addNode, 0, 1);

           Assert.AreEqual(model.CurrentWorkspace.Nodes.Count, 3);
           Assert.AreEqual(model.CurrentWorkspace.Connectors.Count(), 2);

            //create the first state with the numbers selected
           DynamoSelection.Instance.Selection.Add(numberNode1);
           DynamoSelection.Instance.Selection.Add(numberNode2);
           var IDS = DynamoSelection.Instance.Selection.OfType<NodeModel>().Select(x => x.GUID).ToList();
           //create the preset from 2 nodes
           model.CurrentWorkspace.AddPreset(
               "state1",
               "3", IDS);

            //change values
           numberNode1.Value = "2";
           numberNode2.Value = "3";

           DynamoSelection.Instance.ClearSelection();
           DynamoSelection.Instance.Selection.Add(numberNode1);
           DynamoSelection.Instance.Selection.Add(numberNode2);
           IDS = DynamoSelection.Instance.Selection.OfType<NodeModel>().Select(x => x.GUID).ToList();

           model.CurrentWorkspace.AddPreset(
           "state2",
           "5", IDS);

            //now restore state to state 1
           model.CurrentWorkspace.ApplyPreset(model.CurrentWorkspace.Presets.Where(
               x => x.Name == "state1").First());

           RunCurrentModel();
           Thread.Sleep(250);
              //assert that the value of the add node is 3
           Assert.AreEqual(addNode.CachedValue.Data, 3);

           //now restore state to state 2
           model.CurrentWorkspace.ApplyPreset(model.CurrentWorkspace.Presets.Where(
               x => x.Name == "state2").First());

           RunCurrentModel();
           Thread.Sleep(250);
           //assert that the value of the add node is 5
           Assert.AreEqual(addNode.CachedValue.Data, 5);

        }

        [Test]
        public void CanRestoreStateAndUndo()
        {
            var model = CurrentDynamoModel;
            //create some numbers
            var numberNode1 = new DoubleInput();
            numberNode1.Value = "1";
            var numberNode2 = new DoubleInput();
            numberNode2.Value = "2";
            var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));

            //add the nodes
            model.CurrentWorkspace.AddNode(numberNode1, false);
            model.CurrentWorkspace.AddNode(numberNode2, false);
            model.CurrentWorkspace.AddNode(addNode, false);

            //connect them up
            ConnectorModel.Make(numberNode1, addNode, 0, 0);
            ConnectorModel.Make(numberNode2, addNode, 0, 1);

            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count, 3);
            Assert.AreEqual(model.CurrentWorkspace.Connectors.Count(), 2);

            //create the first state with the numbers selected
            DynamoSelection.Instance.Selection.Add(numberNode1);
            DynamoSelection.Instance.Selection.Add(numberNode2);
            var IDS = DynamoSelection.Instance.Selection.OfType<NodeModel>().Select(x => x.GUID).ToList();
            //create the preset from 2 nodes
           model.CurrentWorkspace.AddPreset(
                "state1",
                "3", IDS);

            //change values
            numberNode1.Value = "2";
            numberNode2.Value = "3";

            DynamoSelection.Instance.ClearSelection();
            DynamoSelection.Instance.Selection.Add(numberNode1);
            DynamoSelection.Instance.Selection.Add(numberNode2);
            IDS = DynamoSelection.Instance.Selection.OfType<NodeModel>().Select(x => x.GUID).ToList();

            model.CurrentWorkspace.AddPreset(
            "state2",
            "5", IDS);

            //now restore state to state 1
            model.CurrentWorkspace.ApplyPreset(model.CurrentWorkspace.Presets.Where(
                x => x.Name == "state1").First());

            RunCurrentModel();
            Thread.Sleep(250);
            //assert that the value of the add node is 3
            Assert.AreEqual(addNode.CachedValue.Data, 3);

            //now restore state to state 2
            model.CurrentWorkspace.ApplyPreset(model.CurrentWorkspace.Presets.Where(
                x => x.Name == "state2").First());

            RunCurrentModel();
            Thread.Sleep(250);
            //assert that the value of the add node is 5
            Assert.AreEqual(addNode.CachedValue.Data, 5);

            //now undo the first setWorkSpace call
            var undoOperation = DynCmd.UndoRedoCommand.Operation.Undo;
            model.ExecuteCommand(new DynCmd.UndoRedoCommand(undoOperation));

            RunCurrentModel();
            Thread.Sleep(250);
            //assert that the value of the add node is 3, back to the first state
            Assert.AreEqual(addNode.CachedValue.Data, 3);

        }

        [Test]
        public void CanRestoreStateAndUndoAndRedo()
        {
            var model = CurrentDynamoModel;
            //create some numbers
            var numberNode1 = new DoubleInput();
            numberNode1.Value = "1";
            var numberNode2 = new DoubleInput();
            numberNode2.Value = "2";
            var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));

            //add the nodes
            model.CurrentWorkspace.AddNode(numberNode1, false);
            model.CurrentWorkspace.AddNode(numberNode2, false);
            model.CurrentWorkspace.AddNode(addNode, false);

            //connect them up
            ConnectorModel.Make(numberNode1, addNode, 0, 0);
            ConnectorModel.Make(numberNode2, addNode, 0, 1);

            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count, 3);
            Assert.AreEqual(model.CurrentWorkspace.Connectors.Count(), 2);

            //create the first state with the numbers selected
            DynamoSelection.Instance.Selection.Add(numberNode1);
            DynamoSelection.Instance.Selection.Add(numberNode2);
            var IDS = DynamoSelection.Instance.Selection.OfType<NodeModel>().Select(x => x.GUID).ToList();
            //create the preset from 2 nodes
            model.CurrentWorkspace.AddPreset(
                 "state1",
                 "3", IDS);

            //change values
            numberNode1.Value = "2";
            numberNode2.Value = "3";

            DynamoSelection.Instance.ClearSelection();
            DynamoSelection.Instance.Selection.Add(numberNode1);
            DynamoSelection.Instance.Selection.Add(numberNode2);
            IDS = DynamoSelection.Instance.Selection.OfType<NodeModel>().Select(x => x.GUID).ToList();

            model.CurrentWorkspace.AddPreset(
            "state2",
            "5", IDS);

            //now restore state to state 1
            model.CurrentWorkspace.ApplyPreset(model.CurrentWorkspace.Presets.Where(
                x => x.Name == "state1").First());

            RunCurrentModel();
            Thread.Sleep(250);
            //assert that the value of the add node is 3
            Assert.AreEqual(addNode.CachedValue.Data, 3);

            //now restore state to state 2
            model.CurrentWorkspace.ApplyPreset(model.CurrentWorkspace.Presets.Where(
                x => x.Name == "state2").First());

            RunCurrentModel();
            Thread.Sleep(250);
            //assert that the value of the add node is 5
            Assert.AreEqual(addNode.CachedValue.Data, 5);

            //now undo the first setWorkSpace call
            var undoOperation = DynCmd.UndoRedoCommand.Operation.Undo;
            model.ExecuteCommand(new DynCmd.UndoRedoCommand(undoOperation));

            RunCurrentModel();
            Thread.Sleep(250);
            //assert that the value of the add node is 3, back to the first state
            Assert.AreEqual(addNode.CachedValue.Data, 3);

            //now redo the second setWorkSpace call
            var redoOperation = DynCmd.UndoRedoCommand.Operation.Redo;
            model.ExecuteCommand(new DynCmd.UndoRedoCommand(redoOperation));

            RunCurrentModel();
            Thread.Sleep(250);
            //assert that the value of the add node is 5, back to the second state
            Assert.AreEqual(addNode.CachedValue.Data, 5);

        }

        [Test]
        public void CanLoadFileWithGoodPresetsObject()
        {
            var model = CurrentDynamoModel;
            string openPath = Path.Combine(TestDirectory, @"core\PresetStates\GoodStates.dyn");
            OpenModel(openPath);
           
            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(model.CurrentWorkspace.Presets.Count(), 5);

        }

        [Test]
        public void CanLoadFileWithNoPresetsObject()
        {
            var model = CurrentDynamoModel;
            string openPath = Path.Combine(TestDirectory, @"core\PresetStates\NoStates.dyn");
            OpenModel(openPath);
           
            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(model.CurrentWorkspace.Presets.Count(), 0);

        }

        //this attempts to load some good states and some states with bad GUIDS, 
        [Test]
        public void CanLoadFileWithMalformedPresetsObject()
        {
            var model = CurrentDynamoModel;
            string openPath = Path.Combine(TestDirectory, @"core\PresetStates\MalformedStates.dyn");
            Assert.DoesNotThrow(() => {OpenModel(openPath);});

            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count);
              //assert all the states are loadead even if the GUID and nodetypes are malformed
            Assert.AreEqual(model.CurrentWorkspace.Presets.Count(), 5);

        }

        [Test]
        public void CanLoadFromDynWithMissingNodes()
          {
              var model = CurrentDynamoModel;
              string openPath = Path.Combine(TestDirectory, @"core\PresetStates\MissingNodesInGraph.dyn");
              Assert.DoesNotThrow(() => { OpenModel(openPath); });

              Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
              Assert.AreEqual(model.CurrentWorkspace.Presets.Count(), 5);

              var stateWithMissingNodes = model.CurrentWorkspace.Presets.Where(x => x.Name == "4/8/2015 2:57:02 PM").First();
            //get all the serialized nodes that were loaded when the state was loaded
             var nodesInState = stateWithMissingNodes.SerializedNodes.Select(x=>Guid.Parse(x.GetAttribute("guid")));
            //now assert that one or more of the IDS in the state is missing from the ids in the graph
             var nodesInGraph = model.CurrentWorkspace.Nodes.Select(x => x.GUID);
             var nodesInStateAndGraph = nodesInState.Intersect(nodesInGraph);
           
              Assert.IsTrue(nodesInState.Count() > nodesInStateAndGraph.Count());

              Assert.DoesNotThrow(() => { model.CurrentWorkspace.ApplyPreset(stateWithMissingNodes); });

          }

        [Test]
        public void CanRestoreStateInGraphThatIsMissingNodes()
        {
            var model = CurrentDynamoModel;
            //create some numbers
            var numberNode1 = new DoubleInput();
            numberNode1.Value = "1";
            var numberNode2 = new DoubleInput();
            numberNode2.Value = "2";
            var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));

            //add the nodes
            model.CurrentWorkspace.AddNode(numberNode1, false);
            model.CurrentWorkspace.AddNode(numberNode2, false);
            model.CurrentWorkspace.AddNode(addNode, false);

            //connect them up
            ConnectorModel.Make(numberNode1, addNode, 0, 0);
            ConnectorModel.Make(numberNode2, addNode, 0, 1);

            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count, 3);
            Assert.AreEqual(model.CurrentWorkspace.Connectors.Count(), 2);

            //create the first state with the numbers selected
            DynamoSelection.Instance.Selection.Add(numberNode1);
            DynamoSelection.Instance.Selection.Add(numberNode2);
            var IDS = DynamoSelection.Instance.Selection.OfType<NodeModel>().Select(x => x.GUID).ToList();
            //create the preset from 2 nodes
            model.CurrentWorkspace.AddPreset(
                "state1",
                "3", IDS);

            //change values
            numberNode1.Value = "2";
            numberNode2.Value = "3";

           //now delete numberNode1;
            model.CurrentWorkspace.RemoveNode(numberNode1);


            //now restore state to state 1
            Assert.DoesNotThrow(() =>
            {
                model.CurrentWorkspace.ApplyPreset(model.CurrentWorkspace.Presets.Where(
                    x => x.Name == "state1").First());
            });

            //now check that numbernode2 has been set to correct value in state 1
            Assert.AreEqual(numberNode2.Value, "2");
            //check that node 1 has actually been deleted
            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count, 2);
        }

        [Test]
        public void CanCreateStatesAndSave()
          {
              var model = CurrentDynamoModel;
              //create some numbers
              var numberNode1 = new DoubleInput();
              numberNode1.Value = "1";
              var numberNode2 = new DoubleInput();
              numberNode2.Value = "2";
              var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));
            
              //add the nodes
              model.CurrentWorkspace.AddNode(numberNode1, false);
              model.CurrentWorkspace.AddNode(numberNode2, false);
              model.CurrentWorkspace.AddNode(addNode, false);

              //connect them up
              ConnectorModel.Make(numberNode1, addNode, 0, 0);
              ConnectorModel.Make(numberNode2, addNode, 0, 1);

              Assert.AreEqual(model.CurrentWorkspace.Nodes.Count, 3);
              Assert.AreEqual(model.CurrentWorkspace.Connectors.Count(), 2);

              //create the first state with the numbers selected
              DynamoSelection.Instance.Selection.Add(numberNode1);
              DynamoSelection.Instance.Selection.Add(numberNode2);
              var IDS = DynamoSelection.Instance.Selection.OfType<NodeModel>().Select(x => x.GUID).ToList();
              //create the preset from 2 nodes
              model.CurrentWorkspace.AddPreset(
                  "state1",
                  "3", IDS);

              //change values
              numberNode1.Value = "2";
              numberNode2.Value = "3";

              DynamoSelection.Instance.ClearSelection();
              DynamoSelection.Instance.Selection.Add(numberNode1);
              DynamoSelection.Instance.Selection.Add(numberNode2);
              IDS = DynamoSelection.Instance.Selection.OfType<NodeModel>().Select(x => x.GUID).ToList();

              model.CurrentWorkspace.AddPreset(
              "state2",
              "5", IDS);

              //save these states
              var newPath = GetNewFileNameOnTempPath("dyn");
              var res = model.CurrentWorkspace.SaveAs(newPath, model.EngineController.LiveRunnerRuntimeCore);

              Assert.IsTrue(res);
              Assert.IsTrue(File.Exists(newPath));
         

          }
        
        [Test]
        public void CanSaveAndLoadStateWithMissingNodesWithoutLosingThem()
        {
            var model = CurrentDynamoModel;
            string openPath = Path.Combine(TestDirectory, @"core\PresetStates\GoodStates.dyn");
            OpenModel(openPath);

            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(model.CurrentWorkspace.Presets.Count(), 5);

            var allSerializedNodesInAllStates = model.CurrentWorkspace.Presets.SelectMany(x => x.SerializedNodes);

            //now delete a node that is included in the state
            var num1 = model.CurrentWorkspace.Nodes.OfType<DoubleInput>().First();
            model.CurrentWorkspace.RemoveNode(num1);
            //then save this dyn
            
            var newPath = GetNewFileNameOnTempPath("dyn");
            var res = model.CurrentWorkspace.SaveAs(newPath, model.EngineController.LiveRunnerRuntimeCore);

            Assert.IsTrue(res);
            Assert.IsTrue(File.Exists(newPath));
            //and open this new dyn
            OpenModel(newPath);
            //now only 4 nodes
            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(model.CurrentWorkspace.Presets.Count(), 5);
            
            //make sure we have the same number of serialized nodes in this newly loaded presets state
            var allSerializedNodesInAllStates2 = model.CurrentWorkspace.Presets.SelectMany(x => x.SerializedNodes);
            Assert.AreEqual(allSerializedNodesInAllStates.Count(), allSerializedNodesInAllStates2.Count());
        }

        [Test]
        public void CanRestoreStateAndNodesDoNotMove()
        {
            var model = CurrentDynamoModel;
            //create some numbers
            var numberNode1 = new DoubleInput();
           
            numberNode1.Value = "1";
            var numberNode2 = new DoubleInput();
            numberNode2.Value = "2";
            
            var addNode = new DSFunction(model.LibraryServices.GetFunctionDescriptor("+"));
             
            //add the nodes
            model.CurrentWorkspace.AddNode(numberNode1, false);
            model.CurrentWorkspace.AddNode(numberNode2, false);
            model.CurrentWorkspace.AddNode(addNode, false);
            //connect them up
            ConnectorModel.Make(numberNode1, addNode, 0, 0);
            ConnectorModel.Make(numberNode2, addNode, 0, 1);

            Assert.AreEqual(model.CurrentWorkspace.Nodes.Count, 3);
            Assert.AreEqual(model.CurrentWorkspace.Connectors.Count(), 2);

            //create the first state with the numbers selected
            DynamoSelection.Instance.Selection.Add(numberNode1);
            DynamoSelection.Instance.Selection.Add(numberNode2);
            var IDS = DynamoSelection.Instance.Selection.OfType<NodeModel>().Select(x => x.GUID).ToList();
            //create the preset from 2 nodes
            model.CurrentWorkspace.AddPreset(
                 "state1",
                 "3", IDS);

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
    }
}
