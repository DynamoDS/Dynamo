using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Selection;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using NUnit.Framework;
using DynCmd = Dynamo.Models.DynamoModel;

namespace Dynamo.Tests
{
    internal class PresetStatesTests : DynamoViewModelUnitTest
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("VMDataBridge.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void CanRestoreStateAndUndo()
        {
            var model = ViewModel.Model;
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

            Assert.AreEqual(ViewModel.CurrentSpace.Nodes.Count, 3);
            Assert.AreEqual(ViewModel.CurrentSpace.Connectors.Count(), 2);

            //create the first state with the numbers selected
            DynamoSelection.Instance.Selection.Add(numberNode1);
            DynamoSelection.Instance.Selection.Add(numberNode2);
            var IDS = DynamoSelection.Instance.Selection.OfType<NodeModel>().Select(x => x.GUID).ToList();
            //create the preset from 2 nodes
            ViewModel.Model.CurrentWorkspace.CreatePresetStateFromSelection(
                "state1",
                "3", IDS);

            //change values
            numberNode1.Value = "2";
            numberNode2.Value = "3";

            DynamoSelection.Instance.ClearSelection();
            DynamoSelection.Instance.Selection.Add(numberNode1);
            DynamoSelection.Instance.Selection.Add(numberNode2);
            IDS = DynamoSelection.Instance.Selection.OfType<NodeModel>().Select(x => x.GUID).ToList();

            ViewModel.Model.CurrentWorkspace.CreatePresetStateFromSelection(
            "state2",
            "5", IDS);

            //now restore state to state 1
            ViewModel.CurrentSpace.SetWorkspaceToState(ViewModel.CurrentSpace.PresetsCollection.DesignStates.Where(
                x => x.Name == "state1").First());

            ViewModel.HomeSpace.Run();
            Thread.Sleep(250);
            //assert that the value of the add node is 3
            Assert.AreEqual(addNode.CachedValue.Data, 3);

            //now restore state to state 2
            ViewModel.CurrentSpace.SetWorkspaceToState(ViewModel.CurrentSpace.PresetsCollection.DesignStates.Where(
                x => x.Name == "state2").First());

            ViewModel.HomeSpace.Run();
            Thread.Sleep(250);
            //assert that the value of the add node is 5
            Assert.AreEqual(addNode.CachedValue.Data, 5);

            //now undo the first setWorkSpace call
            var undoOperation = DynCmd.UndoRedoCommand.Operation.Undo;
            ViewModel.ExecuteCommand(new DynCmd.UndoRedoCommand(undoOperation));

            ViewModel.HomeSpace.Run();
            Thread.Sleep(250);
            //assert that the value of the add node is 3, back to the first state
            Assert.AreEqual(addNode.CachedValue.Data, 3);

        }

        [Test]
        public void CanLoadFileWithGoodPresetsObject()
        {
            string openPath = Path.Combine(TestDirectory, @"core\PresetStates\GoodStates.dyn");
            ViewModel.OpenCommand.Execute(openPath);
           
            Assert.AreEqual(5, ViewModel.CurrentSpace.Nodes.Count);
            Assert.AreEqual(ViewModel.HomeSpace.PresetsCollection.DesignStates.Count(), 5);

        }
         [Test]
        public void CanLoadFileWithNoPresetsObject()
        {
            string openPath = Path.Combine(TestDirectory, @"core\PresetStates\NoStates.dyn");
            ViewModel.OpenCommand.Execute(openPath);
           
            Assert.AreEqual(5, ViewModel.CurrentSpace.Nodes.Count);
            Assert.AreEqual(ViewModel.HomeSpace.PresetsCollection.DesignStates.Count(), 0);

        }

          [Test]
          [Category("Failure")]
        //TODO fix implementaion in loader
        //this attempts to load some good states and some states with bad GUIDS, currently Dynamo just refuses to load
        // the entire graph when one GUID is bad in a state, should just load anyway...
        public void CanLoadFileWithMalformedPresetsObject()
        {
            string openPath = Path.Combine(TestDirectory, @"core\PresetStates\MalformedStates.dyn");
            Assert.DoesNotThrow(() => {ViewModel.OpenCommand.Execute(openPath);});
           
            Assert.AreEqual(5, ViewModel.CurrentSpace.Nodes.Count);
              //assert all the states are loadead even if the GUID and nodetypes are malformed
            Assert.AreEqual(ViewModel.HomeSpace.PresetsCollection.DesignStates.Count(), 5);

        }

        [Test]
          public void CanLoadFromDynWithMissingNodes()
          {
              string openPath = Path.Combine(TestDirectory, @"core\PresetStates\MissingNodesInGraph.dyn");
              Assert.DoesNotThrow(() => { ViewModel.OpenCommand.Execute(openPath); });

              Assert.AreEqual(4, ViewModel.CurrentSpace.Nodes.Count);
              Assert.AreEqual(ViewModel.HomeSpace.PresetsCollection.DesignStates.Count(), 5);

             var stateWithMissingNodes = ViewModel.CurrentSpace.PresetsCollection.DesignStates.Where(x=>x.Name == "4/8/2015 2:57:02 PM").First();
            //get all the serialized nodes that were loaded when the state was loaded
             var nodesInState = stateWithMissingNodes.SerializedNodes.Select(x=>Guid.Parse(x.GetAttribute("guid")));
            //now assert that one or more of the IDS in the state is missing from the ids in the graph
             var nodesInGraph = ViewModel.CurrentSpace.Nodes.Select(x => x.GUID);
             var nodesInStateAndGraph = nodesInState.Intersect(nodesInGraph);
           
              Assert.IsTrue(nodesInState.Count() > nodesInStateAndGraph.Count());

              Assert.DoesNotThrow(() => { ViewModel.CurrentSpace.SetWorkspaceToState(stateWithMissingNodes); });

          }

        [Test]
        public void CanRestoreStateInGraphThatIsMissingNodes()
        {
            var model = ViewModel.Model;
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

            Assert.AreEqual(ViewModel.CurrentSpace.Nodes.Count, 3);
            Assert.AreEqual(ViewModel.CurrentSpace.Connectors.Count(), 2);

            //create the first state with the numbers selected
            DynamoSelection.Instance.Selection.Add(numberNode1);
            DynamoSelection.Instance.Selection.Add(numberNode2);
            var IDS = DynamoSelection.Instance.Selection.OfType<NodeModel>().Select(x => x.GUID).ToList();
            //create the preset from 2 nodes
            ViewModel.Model.CurrentWorkspace.CreatePresetStateFromSelection(
                "state1",
                "3", IDS);

            //change values
            numberNode1.Value = "2";
            numberNode2.Value = "3";

           //now delete numberNode1;
            ViewModel.CurrentSpace.RemoveNode(numberNode1);


            //now restore state to state 1
            Assert.DoesNotThrow(() =>
            {
                ViewModel.CurrentSpace.SetWorkspaceToState(ViewModel.CurrentSpace.PresetsCollection.DesignStates.Where(
                    x => x.Name == "state1").First());
            });

            //now check that numbernode2 has been set to correct value in state 1
            Assert.AreEqual(numberNode2.Value, "2");
            //check that node 1 has actually been deleted
            Assert.AreEqual(ViewModel.CurrentSpace.Nodes.Count, 2);
        }

        [Test]
       public void CanCreateStatesAndSave()
          {
              var model = ViewModel.Model;
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

              Assert.AreEqual(ViewModel.CurrentSpace.Nodes.Count, 3);
              Assert.AreEqual(ViewModel.CurrentSpace.Connectors.Count(), 2);

              //create the first state with the numbers selected
              DynamoSelection.Instance.Selection.Add(numberNode1);
              DynamoSelection.Instance.Selection.Add(numberNode2);
              var IDS = DynamoSelection.Instance.Selection.OfType<NodeModel>().Select(x => x.GUID).ToList();
              //create the preset from 2 nodes
              ViewModel.Model.CurrentWorkspace.CreatePresetStateFromSelection(
                  "state1",
                  "3", IDS);

              //change values
              numberNode1.Value = "2";
              numberNode2.Value = "3";

              DynamoSelection.Instance.ClearSelection();
              DynamoSelection.Instance.Selection.Add(numberNode1);
              DynamoSelection.Instance.Selection.Add(numberNode2);
              IDS = DynamoSelection.Instance.Selection.OfType<NodeModel>().Select(x => x.GUID).ToList();

              ViewModel.Model.CurrentWorkspace.CreatePresetStateFromSelection(
              "state2",
              "5", IDS);

              //save these states
              var newPath = GetNewFileNameOnTempPath("dyn");
              var res = ViewModel.Model.CurrentWorkspace.SaveAs(newPath, ViewModel.Model.EngineController.LiveRunnerCore);

              Assert.IsTrue(res);
              Assert.IsTrue(File.Exists(newPath));
         

          }
        
        [Test]
        public void CanSaveAndLoadStateWithMissingNodesWithoutLosingThem()
        {
            string openPath = Path.Combine(TestDirectory, @"core\PresetStates\GoodStates.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            Assert.AreEqual(5, ViewModel.CurrentSpace.Nodes.Count);
            Assert.AreEqual(ViewModel.HomeSpace.PresetsCollection.DesignStates.Count(), 5);

            var allSerializedNodesInAllStates = ViewModel.CurrentSpace.PresetsCollection.DesignStates.SelectMany(x => x.SerializedNodes);

            //now delete a node that is included in the state
            var num1 = ViewModel.HomeSpace.Nodes.OfType<DoubleInput>().First();
            ViewModel.HomeSpace.RemoveNode(num1);
            //then save this dyn
            
            var newPath = GetNewFileNameOnTempPath("dyn");
            var res = ViewModel.Model.CurrentWorkspace.SaveAs(newPath, ViewModel.Model.EngineController.LiveRunnerCore);

            Assert.IsTrue(res);
            Assert.IsTrue(File.Exists(newPath));
            //and open this new dyn
            ViewModel.OpenCommand.Execute(newPath);
            //now only 4 nodes
            Assert.AreEqual(4, ViewModel.CurrentSpace.Nodes.Count);
            Assert.AreEqual(ViewModel.HomeSpace.PresetsCollection.DesignStates.Count(), 5);
            
            //make sure we have the same number of serialized nodes in this newly loaded presets state
            var allSerializedNodesInAllStates2 = ViewModel.CurrentSpace.PresetsCollection.DesignStates.SelectMany(x => x.SerializedNodes);
            Assert.AreEqual(allSerializedNodesInAllStates.Count(), allSerializedNodesInAllStates2.Count());
        }
    }
}
