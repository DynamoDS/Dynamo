using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using DSNodeServices;
using Dynamo.Models;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using System.Linq;
using Dynamo.Nodes;
using System.Reflection;
using System.Runtime.Remoting;
using DynamoUtilities;
using Autodesk.DesignScript.Runtime;
using Dynamo.Core;

namespace Dynamo.TestInfrastructure
{
    /// <summary>
    /// Class to handle driving test mutations into the graph
    /// </summary>
    public class MutatorDriver
    {
        private readonly DynamoViewModel dynamoViewModel;

        public MutatorDriver(DynamoViewModel dynamoViewModel)
        {
            this.dynamoViewModel = dynamoViewModel;
        }

        internal void RunMutationTests()
        {
            Random rand = new Random(1);
            //DebugSettings.VerboseLogging = true;

            String logTarget = dynamoViewModel.Model.Logger.LogPath + "MutationLog.log";

            StreamWriter writer = new StreamWriter(logTarget);

            writer.WriteLine("MutateTest Internal activate");

            System.Diagnostics.Debug.WriteLine("MutateTest Internal activate");

            new Thread(() =>
            {
                try
                {
                    ConnectorTest(writer);
                    CopyNodeTest(writer);
                    DeleteNodeTest(writer);

                    CodeBlockNodeTest(writer);

                    IntegerSliderTest(writer);
                    DoubleSliderTest(writer);

                    DirectoryPathTest(writer);
                    FilePathTest(writer);

                    NumberInputTest(writer);
                    StringInputTest(writer);

                    NumberSequenceTest(writer);
                    NumberRangeTest(writer);
                    ListTest(writer);

                    CustomNodeTest(writer);
                    CustomNodeCompatibilityTest(writer);
                }
                finally
                {
                    dynamoViewModel.Model.Logger.Log("Fuzz testing finished.");

                    writer.Flush();
                    writer.Close();
                    writer.Dispose();
                }

            }).
                Start();
        }

        #region Copy/Delete Tests

        private void DeleteNodeTest(StreamWriter writer)
        {
            bool passed = false;
            try
            {
                Random rand = new Random(1);

                for (int i = 0; i < 1000; i++)
                {
                    writer.WriteLine("##### - Beginning run: " + i);

                    var nodes = dynamoViewModel.Model.Nodes.ToList();
                    int nodesCountBeforeDelete = nodes.Count;

                    writer.WriteLine("### - Beginning eval");


                    dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                    {
                        DynamoViewModel.RunCancelCommand runCancel =
                            new DynamoViewModel.RunCancelCommand(false, false);
                        dynamoViewModel.ExecuteCommand(runCancel);

                    }));

                    while (dynamoViewModel.Model.Runner.Running)
                    {
                        Thread.Sleep(10);
                    }

                    writer.WriteLine("### - Eval complete");
                    writer.Flush();
                    writer.WriteLine("### - Beginning readout");

                    writer.WriteLine("### - Beginning delete");
                    NodeModel node = nodes[rand.Next(nodes.Count)];
                    writer.WriteLine("### - Deletion target: " + node.GUID);


                    List<AbstractMutator> mutators = new List<AbstractMutator>()
                                {
                                    new DeleteNodeMutator(dynamoViewModel, rand)
                                };


                    AbstractMutator mutator = mutators[rand.Next(mutators.Count)];

                    int numberOfUndosNeeded = mutator.Mutate();


                    Thread.Sleep(100);

                    writer.WriteLine("### - delete complete");
                    writer.Flush();
                    writer.WriteLine("### - Beginning re-exec");


                    dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                    {
                        DynamoViewModel.RunCancelCommand runCancel =
                            new DynamoViewModel.RunCancelCommand(false, false);
                        dynamoViewModel.ExecuteCommand(runCancel);

                    }));

                    Thread.Sleep(100);

                    writer.WriteLine("### - re-exec complete");
                    writer.Flush();
                    writer.WriteLine("### - Beginning undo");

                    int nodesCountAfterDelete = dynamoViewModel.Model.Nodes.Count;

                    if (nodesCountBeforeDelete > nodesCountAfterDelete)
                    {
                        for (int iUndo = 0; iUndo < numberOfUndosNeeded; iUndo++)
                        {
                            dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                            {
                                DynamoViewModel.UndoRedoCommand undoCommand =
                                    new DynamoViewModel.UndoRedoCommand(
                                        DynamoViewModel.UndoRedoCommand.Operation.Undo);
                                dynamoViewModel.ExecuteCommand(undoCommand);

                            }));

                            Thread.Sleep(100);
                        }

                        writer.WriteLine("### - undo complete");
                        writer.Flush();
                        writer.WriteLine("### - Beginning re-exec");

                        dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                        {
                            DynamoViewModel.RunCancelCommand runCancel =
                                new DynamoViewModel.RunCancelCommand(false, false);

                            dynamoViewModel.ExecuteCommand(runCancel);

                        }));
                        Thread.Sleep(10);

                        while (dynamoViewModel.Model.Runner.Running)
                        {
                            Thread.Sleep(10);
                        }

                        writer.WriteLine("### - re-exec complete");
                        writer.Flush();

                        nodesCountAfterDelete = dynamoViewModel.Model.Nodes.Count;
                        if (nodesCountBeforeDelete == nodesCountAfterDelete)
                            writer.WriteLine("### - Node was restored");
                        else
                            writer.WriteLine("### - Node wasn't restored");
                        writer.Flush();
                    }
                    else
                    {
                        writer.WriteLine("### - Error removing a node");
                        writer.Flush();
                    }
                }

                passed = true;
            }
            finally
            {
                dynamoViewModel.Model.Logger.Log("DeleteNodeTest : " + (passed ? "pass" : "FAIL"));
            }
        }

        private void CopyNodeTest(StreamWriter writer)
        {
            bool passed = false;
            List<NodeModel> nodes = dynamoViewModel.Model.Nodes.ToList();
            if (nodes.Count == 0)
                return;

            try
            {
                Random rand = new Random(1);

                for (int i = 0; i < 1000; i++)
                {
                    NodeModel node = nodes[rand.Next(nodes.Count)];

                    writer.WriteLine("##### - Beginning run: " + i);
                    writer.WriteLine("### - Beginning eval");
                    dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                    {
                        DynamoViewModel.RunCancelCommand runCancel =
                            new DynamoViewModel.RunCancelCommand(false, false);
                        dynamoViewModel.ExecuteCommand(runCancel);
                    }));
                    while (dynamoViewModel.Model.Runner.Running)
                    {
                        Thread.Sleep(10);
                    }
                    writer.WriteLine("### - Eval complete");
                    writer.Flush();

                    int nodesCountBeforeCopying = nodes.Count;

                    List<AbstractMutator> mutators = new List<AbstractMutator>()
                        {
                            new CopyNodeMutator(dynamoViewModel, rand)
                        };
                    AbstractMutator mutator = mutators[rand.Next(mutators.Count)];
                    int numberOfUndosNeeded = mutator.Mutate();
                    Thread.Sleep(100);

                    writer.WriteLine("### - Beginning undo");
                    for (int iUndo = 0; iUndo < numberOfUndosNeeded; iUndo++)
                    {
                        dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                        {
                            DynamoViewModel.UndoRedoCommand undoCommand =
                                new DynamoViewModel.UndoRedoCommand(DynamoViewModel.UndoRedoCommand.Operation.Undo);
                            dynamoViewModel.ExecuteCommand(undoCommand);

                        }));
                        Thread.Sleep(100);
                    }
                    writer.WriteLine("### - undo complete");
                    writer.Flush();

                    dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                    {
                        DynamoViewModel.RunCancelCommand runCancel =
                            new DynamoViewModel.RunCancelCommand(false, false);
                        dynamoViewModel.ExecuteCommand(runCancel);
                    }));
                    while (dynamoViewModel.Model.Runner.Running)
                    {
                        Thread.Sleep(10);
                    }

                    writer.WriteLine("### - Beginning test of CopyNode");
                    if (node.OutPorts.Count > 0)
                    {
                        try
                        {
                            int nodesCountAfterCopying = dynamoViewModel.Model.Nodes.ToList().Count;

                            if (nodesCountBeforeCopying != nodesCountAfterCopying)
                            {
                                writer.WriteLine("!!!!!!!!!!! - test of CopyNode is failed");
                                writer.WriteLine(node.GUID);

                                writer.WriteLine("Was: " + nodesCountAfterCopying);
                                writer.WriteLine("Should have been: " + nodesCountBeforeCopying);
                                writer.Flush();
                                return;

                                Debug.WriteLine("==========> Failure on run: " + i);
                                Debug.WriteLine("Lookup map failed to agree");
                                Validity.Assert(false);
                            }
                            else
                            {
                                writer.WriteLine("### - test of CopyNode is passed");
                                writer.Flush();
                            }

                        }
                        catch (Exception)
                        {
                            writer.WriteLine("!!!!!!!!!!! - test of CopyNode is failed");
                            writer.Flush();
                            return;
                        }
                    }
                    writer.WriteLine("### - test of CopyNode complete");
                    writer.Flush();
                }
                passed = true;
            }
            finally
            {
                dynamoViewModel.Model.Logger.Log("CopyNodeTest : " + (passed ? "pass" : "FAIL"));
            }
        }

        #endregion

        #region CodeBlockNode Test

        private void CodeBlockNodeTest(StreamWriter writer)
        {
            bool passed = false;
            try
            {
                Random rand = new Random(1);

                for (int i = 0; i < 1000; i++)
                {
                    writer.WriteLine("##### - Beginning run: " + i);

                    var nodes = dynamoViewModel.Model.Nodes.Where(t => t.GetType() == typeof(CodeBlockNodeModel)).ToList();
                    if (nodes.Count == 0)
                        return;

                    writer.WriteLine("### - Beginning eval");


                    dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                    {
                        DynamoViewModel.RunCancelCommand runCancel =
                            new DynamoViewModel.RunCancelCommand(false, false);
                        dynamoViewModel.ExecuteCommand(runCancel);

                    }));

                    while (dynamoViewModel.Model.Runner.Running)
                    {
                        Thread.Sleep(10);
                    }

                    writer.WriteLine("### - Eval complete");
                    writer.Flush();
                    writer.WriteLine("### - Beginning readout");


                    Dictionary<Guid, String> valueMap = new Dictionary<Guid, String>();

                    foreach (NodeModel n in nodes)
                    {
                        if (n.OutPorts.Count > 0)
                        {
                            Guid guid = n.GUID;
                            Object data = n.GetValue(0).Data;
                            String val = data != null ? data.ToString() : "null";
                            valueMap.Add(guid, val);
                            writer.WriteLine(guid + " :: " + val);
                            writer.Flush();

                        }
                    }

                    writer.WriteLine("### - Readout complete");
                    writer.Flush();
                    writer.WriteLine("### - Beginning delete");
                    NodeModel node = nodes[rand.Next(nodes.Count)];
                    writer.WriteLine("### - Deletion target: " + node.GUID);


                    List<AbstractMutator> mutators = new List<AbstractMutator>()
                                {
                                    new CodeBlockNodeMutator(dynamoViewModel, rand), new DeleteNodeMutator(dynamoViewModel, rand)
                                };


                    AbstractMutator mutator = mutators[rand.Next(mutators.Count)];

                    int numberOfUndosNeeded = mutator.Mutate();


                    Thread.Sleep(100);

                    writer.WriteLine("### - delete complete");
                    writer.Flush();
                    writer.WriteLine("### - Beginning re-exec");


                    dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                    {
                        DynamoViewModel.RunCancelCommand runCancel =
                            new DynamoViewModel.RunCancelCommand(false, false);
                        dynamoViewModel.ExecuteCommand(runCancel);

                    }));

                    Thread.Sleep(100);

                    writer.WriteLine("### - re-exec complete");
                    writer.Flush();
                    writer.WriteLine("### - Beginning undo");


                    for (int iUndo = 0; iUndo < numberOfUndosNeeded; iUndo++)
                    {
                        dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                        {
                            DynamoViewModel.UndoRedoCommand undoCommand =
                                new DynamoViewModel.UndoRedoCommand(
                                    DynamoViewModel.UndoRedoCommand.Operation.Undo);
                            dynamoViewModel.ExecuteCommand(undoCommand);

                        }));

                        Thread.Sleep(100);

                    }


                    writer.WriteLine("### - undo complete");
                    writer.Flush();
                    writer.WriteLine("### - Beginning re-exec");

                    dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                    {
                        DynamoViewModel.RunCancelCommand runCancel =
                            new DynamoViewModel.RunCancelCommand(false, false);

                        dynamoViewModel.ExecuteCommand(runCancel);

                    }));
                    Thread.Sleep(10);

                    while (dynamoViewModel.Model.Runner.Running)
                    {
                        Thread.Sleep(10);
                    }

                    writer.WriteLine("### - re-exec complete");
                    writer.Flush();
                    writer.WriteLine("### - Beginning readback");


                    foreach (NodeModel n in nodes)
                    {
                        if (n.OutPorts.Count > 0)
                        {
                            try
                            {
                                String valmap = valueMap[n.GUID].ToString();
                                Object data = n.GetValue(0).Data;
                                String nodeVal = data != null ? data.ToString() : "null";

                                if (valmap != nodeVal)
                                {

                                    writer.WriteLine("!!!!!!!!!!! Read-back failed");
                                    writer.WriteLine(n.GUID);


                                    writer.WriteLine("Was: " + nodeVal);
                                    writer.WriteLine("Should have been: " + valmap);
                                    writer.Flush();
                                    return;


                                    Debug.WriteLine("==========> Failure on run: " + i);
                                    Debug.WriteLine("Lookup map failed to agree");
                                    Validity.Assert(false);
                                }
                            }
                            catch (Exception)
                            {
                                writer.WriteLine("!!!!!!!!!!! Read-back failed");
                                writer.Flush();
                                return;
                            }
                        }
                    }
                }

                passed = true;
            }
            finally
            {
                dynamoViewModel.Model.Logger.Log("CodeBlockNodeTest : " + (passed ? "pass" : "FAIL"));
            }
        }

        #endregion

        #region Connector Test

        private void ConnectorTest(StreamWriter writer)
        {
            bool passed = false;

            try
            {
                writer.WriteLine("##### - Beginning run");

                Random rand = new Random(1);

                for (int i = 0; i < 1000; i++)
                {
                    writer.WriteLine("##### - Beginning run: " + i);

                    List<NodeModel> nodes = dynamoViewModel.Model.Nodes;
                    int randVal = rand.Next(nodes.Count);

                    writer.WriteLine("### - Beginning eval");

                    NodeModel node = nodes.Count > 0 ? nodes[randVal] : null;

                    if (node != null)
                    {
                        List<ConnectorModel> firstNodeConnectors = node.AllConnectors.ToList();

                        foreach (ConnectorModel connector in firstNodeConnectors)
                        {
                            //let's remember the indexes
                            int startIndex = connector.Start.Index;

                            dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                            {
                                DynamoViewModel.DeleteModelCommand delCommand =
                                    new DynamoViewModel.DeleteModelCommand(node.GUID);

                                dynamoViewModel.ExecuteCommand(delCommand);

                            }));

                            if (node.OutPorts[startIndex].IsConnected)
                                writer.WriteLine("### - Connector wasn't deleted");
                            else
                                writer.WriteLine("### - Connector was deleted");

                            dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                            {
                                DynamoViewModel.UndoRedoCommand undoCommand =
                                    new DynamoViewModel.UndoRedoCommand(
                                        DynamoViewModel.UndoRedoCommand.Operation.Undo);
                                dynamoViewModel.ExecuteCommand(undoCommand);

                            }));

                            Thread.Sleep(100);

                            if (node.OutPorts[startIndex].IsConnected)
                                writer.WriteLine("### - Connector was recreated");
                            else
                                writer.WriteLine("### - ### - Connector wasn't recreated");
                        }

                        passed = true;
                    }
                }
            }
            finally
            {
                dynamoViewModel.Model.Logger.Log("ConnectorTest : " + (passed ? "pass" : "FAIL"));
            }
        }

        #endregion

        #region Slider Tests

        private void IntegerSliderTest(StreamWriter writer)
        {
            bool passed = false;
            try
            {
                Random rand = new Random(1);

                for (int i = 0; i < 1000; i++)
                {
                    writer.WriteLine("##### - Beginning run: " + i);

                    string assemblyPath = Assembly.GetExecutingAssembly().Location;
                    string assemblyDir = Path.GetDirectoryName(assemblyPath);
                    string pathToNodesDll = assemblyDir + "\\nodes\\DSCoreNodesUI.dll";
                    Assembly assembly = Assembly.LoadFile(pathToNodesDll);

                    Type type = assembly.GetType("Dynamo.Nodes.IntegerSlider");
                    if (type != null)
                    {
                        List<NodeModel> nodes = dynamoViewModel.Model.Nodes.Where(t => t.GetType() == type).ToList();

                        writer.WriteLine("### - Beginning eval");
                        dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                        {
                            DynamoViewModel.RunCancelCommand runCancel =
                                new DynamoViewModel.RunCancelCommand(false, false);
                            dynamoViewModel.ExecuteCommand(runCancel);
                        }));
                        while (dynamoViewModel.Model.Runner.Running)
                        {
                            Thread.Sleep(10);
                        }
                        writer.WriteLine("### - Eval complete");
                        writer.Flush();

                        Dictionary<Guid, String> valueMap = new Dictionary<Guid, String>();
                        foreach (NodeModel n in nodes)
                        {
                            if (n.OutPorts.Count > 0)
                            {
                                Guid guid = n.GUID;
                                Object data = n.GetValue(0).Data;
                                String val = data != null ? data.ToString() : "null";
                                valueMap.Add(guid, val);
                                writer.WriteLine(guid + " :: " + val);
                                writer.Flush();
                            }
                        }

                        List<AbstractMutator> mutators = new List<AbstractMutator>()
                    {
                        new IntegerSliderMutator(dynamoViewModel, rand)
                    };
                        AbstractMutator mutator = mutators[rand.Next(mutators.Count)];
                        int numberOfUndosNeeded = mutator.Mutate();
                        Thread.Sleep(100);

                        writer.WriteLine("### - Beginning undo");
                        for (int iUndo = 0; iUndo < numberOfUndosNeeded; iUndo++)
                        {
                            dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                            {
                                DynamoViewModel.UndoRedoCommand undoCommand =
                                    new DynamoViewModel.UndoRedoCommand(
                                        DynamoViewModel.UndoRedoCommand.Operation.Undo);
                                dynamoViewModel.ExecuteCommand(undoCommand);
                            }));
                            Thread.Sleep(100);
                        }
                        writer.WriteLine("### - undo complete");
                        writer.Flush();
                        writer.WriteLine("### - Beginning re-exec");

                        dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                        {
                            DynamoViewModel.RunCancelCommand runCancel =
                                new DynamoViewModel.RunCancelCommand(false, false);

                            dynamoViewModel.ExecuteCommand(runCancel);

                        }));
                        Thread.Sleep(10);

                        while (dynamoViewModel.Model.Runner.Running)
                        {
                            Thread.Sleep(10);
                        }

                        writer.WriteLine("### - re-exec complete");
                        writer.Flush();
                        writer.WriteLine("### - Beginning readback");

                        writer.WriteLine("### - Beginning test of IntegerSlider");
                        foreach (NodeModel n in nodes)
                        {
                            if (n.OutPorts.Count > 0)
                            {
                                try
                                {
                                    String valmap = valueMap[n.GUID].ToString();
                                    Object data = n.GetValue(0).Data;
                                    String nodeVal = data != null ? data.ToString() : "null";

                                    if (valmap != nodeVal)
                                    {
                                        writer.WriteLine("!!!!!!!!!!! - test of IntegerSlider is failed");
                                        writer.WriteLine(n.GUID);

                                        writer.WriteLine("Was: " + nodeVal);
                                        writer.WriteLine("Should have been: " + valmap);
                                        writer.Flush();
                                        return;

                                        Debug.WriteLine("==========> Failure on run: " + i);
                                        Debug.WriteLine("Lookup map failed to agree");
                                        Validity.Assert(false);
                                    }
                                }
                                catch (Exception)
                                {
                                    writer.WriteLine("!!!!!!!!!!! - test of IntegerSlider is failed");
                                    writer.Flush();
                                    return;
                                }
                            }
                        }
                        writer.WriteLine("### - test of IntegerSlider complete");
                        writer.Flush();
                    }
                    passed = true;
                }
            }
            finally
            {
                dynamoViewModel.Model.Logger.Log("IntegerSliderTest : " + (passed ? "pass" : "FAIL"));
            }
        }

        private void DoubleSliderTest(StreamWriter writer)
        {
            bool passed = false;
            try
            {
                Random rand = new Random(1);

                for (int i = 0; i < 1000; i++)
                {
                    writer.WriteLine("##### - Beginning run: " + i);

                    string assemblyPath = Assembly.GetExecutingAssembly().Location;
                    string assemblyDir = Path.GetDirectoryName(assemblyPath);
                    string pathToNodesDll = assemblyDir + "\\nodes\\DSCoreNodesUI.dll";
                    Assembly assembly = Assembly.LoadFile(pathToNodesDll);

                    Type type = assembly.GetType("Dynamo.Nodes.DoubleSlider");
                    if (type != null)
                    {
                        List<NodeModel> nodes = dynamoViewModel.Model.Nodes.Where(t => t.GetType() == type).ToList();

                        writer.WriteLine("### - Beginning eval");
                        dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                        {
                            DynamoViewModel.RunCancelCommand runCancel =
                                new DynamoViewModel.RunCancelCommand(false, false);
                            dynamoViewModel.ExecuteCommand(runCancel);
                        }));
                        while (dynamoViewModel.Model.Runner.Running)
                        {
                            Thread.Sleep(10);
                        }
                        writer.WriteLine("### - Eval complete");
                        writer.Flush();

                        Dictionary<Guid, String> valueMap = new Dictionary<Guid, String>();
                        foreach (NodeModel n in nodes)
                        {
                            if (n.OutPorts.Count > 0)
                            {
                                Guid guid = n.GUID;
                                Object data = n.GetValue(0).Data;
                                String val = data != null ? data.ToString() : "null";
                                valueMap.Add(guid, val);
                                writer.WriteLine(guid + " :: " + val);
                                writer.Flush();
                            }
                        }

                        List<AbstractMutator> mutators = new List<AbstractMutator>()
                    {
                        new DoubleSliderMutator(dynamoViewModel, rand)
                    };
                        AbstractMutator mutator = mutators[rand.Next(mutators.Count)];
                        int numberOfUndosNeeded = mutator.Mutate();
                        Thread.Sleep(100);

                        writer.WriteLine("### - Beginning undo");
                        for (int iUndo = 0; iUndo < numberOfUndosNeeded; iUndo++)
                        {
                            dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                            {
                                DynamoViewModel.UndoRedoCommand undoCommand =
                                    new DynamoViewModel.UndoRedoCommand(
                                        DynamoViewModel.UndoRedoCommand.Operation.Undo);
                                dynamoViewModel.ExecuteCommand(undoCommand);
                            }));
                            Thread.Sleep(100);
                        }
                        writer.WriteLine("### - undo complete");
                        writer.Flush();

                        writer.WriteLine("### - undo complete");
                        writer.Flush();
                        writer.WriteLine("### - Beginning re-exec");

                        dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                        {
                            DynamoViewModel.RunCancelCommand runCancel =
                                new DynamoViewModel.RunCancelCommand(false, false);

                            dynamoViewModel.ExecuteCommand(runCancel);

                        }));
                        Thread.Sleep(10);

                        while (dynamoViewModel.Model.Runner.Running)
                        {
                            Thread.Sleep(10);
                        }
                        writer.WriteLine("### - re-exec complete");
                        writer.Flush();
                        writer.WriteLine("### - Beginning readback");

                        writer.WriteLine("### - Beginning test of DoubleSlider");
                        foreach (NodeModel n in nodes)
                        {
                            if (n.OutPorts.Count > 0)
                            {
                                try
                                {
                                    String valmap = valueMap[n.GUID].ToString();
                                    Object data = n.GetValue(0).Data;
                                    String nodeVal = data != null ? data.ToString() : "null";

                                    if (valmap != nodeVal)
                                    {
                                        writer.WriteLine("!!!!!!!!!!! - test of DoubleSlider is failed");
                                        writer.WriteLine(n.GUID);

                                        writer.WriteLine("Was: " + nodeVal);
                                        writer.WriteLine("Should have been: " + valmap);
                                        writer.Flush();
                                        return;

                                        Debug.WriteLine("==========> Failure on run: " + i);
                                        Debug.WriteLine("Lookup map failed to agree");
                                        Validity.Assert(false);
                                    }
                                }
                                catch (Exception)
                                {
                                    writer.WriteLine("!!!!!!!!!!! - test of DoubleSlider is failed");
                                    writer.Flush();
                                    return;
                                }
                            }
                        }
                        writer.WriteLine("### - test of DoubleSlider complete");
                        writer.Flush();
                    }
                    passed = true;
                }
            }
            finally
            {
                dynamoViewModel.Model.Logger.Log("DoubleSliderTest : " + (passed ? "pass" : "FAIL"));
            }
        }

        #endregion

        #region DirectoryPath/FilePath Tests

        private void DirectoryPathTest(StreamWriter writer)
        {
            bool passed = false;
            try
            {
                Random rand = new Random(1);

                for (int i = 0; i < 1000; i++)
                {
                    writer.WriteLine("##### - Beginning run: " + i);

                    string assemblyPath = Assembly.GetExecutingAssembly().Location;
                    string assemblyDir = Path.GetDirectoryName(assemblyPath);
                    string pathToNodesDll = assemblyDir + "\\nodes\\DSCoreNodesUI.dll";
                    Assembly assembly = Assembly.LoadFile(pathToNodesDll);

                    Type type = assembly.GetType("DSCore.File.Directory");
                    List<NodeModel> nodes = new List<NodeModel>();
                    if (type != null)
                        nodes = dynamoViewModel.Model.Nodes.Where(t => t.GetType() == type).ToList();

                    if (nodes.Count == 0)
                        return;

                    writer.WriteLine("### - Beginning eval");
                    dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                    {
                        DynamoViewModel.RunCancelCommand runCancel =
                            new DynamoViewModel.RunCancelCommand(false, false);
                        dynamoViewModel.ExecuteCommand(runCancel);
                    }));
                    while (dynamoViewModel.Model.Runner.Running)
                    {
                        Thread.Sleep(10);
                    }
                    writer.WriteLine("### - Eval complete");
                    writer.Flush();

                    Dictionary<Guid, String> valueMap = new Dictionary<Guid, String>();
                    foreach (NodeModel n in nodes)
                    {
                        if (n.OutPorts.Count > 0)
                        {
                            Guid guid = n.GUID;
                            Object data = n.GetValue(0).Data;
                            String val = data != null ? data.ToString() : "null";
                            valueMap.Add(guid, val);
                            writer.WriteLine(guid + " :: " + val);
                            writer.Flush();
                        }
                    }

                    List<AbstractMutator> mutators = new List<AbstractMutator>()
                    {
                        new DirectoryPathMutator(dynamoViewModel, rand)
                    };
                    AbstractMutator mutator = mutators[rand.Next(mutators.Count)];
                    int numberOfUndosNeeded = mutator.Mutate();
                    Thread.Sleep(100);

                    writer.WriteLine("### - Beginning undo");
                    for (int iUndo = 0; iUndo < numberOfUndosNeeded; iUndo++)
                    {
                        dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                        {
                            DynamoViewModel.UndoRedoCommand undoCommand =
                                new DynamoViewModel.UndoRedoCommand(
                                    DynamoViewModel.UndoRedoCommand.Operation.Undo);
                            dynamoViewModel.ExecuteCommand(undoCommand);
                        }));
                        Thread.Sleep(100);
                    }
                    writer.WriteLine("### - undo complete");
                    writer.Flush();
                    writer.WriteLine("### - Beginning re-exec");

                    dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                    {
                        DynamoViewModel.RunCancelCommand runCancel =
                            new DynamoViewModel.RunCancelCommand(false, false);

                        dynamoViewModel.ExecuteCommand(runCancel);

                    }));
                    Thread.Sleep(10);

                    while (dynamoViewModel.Model.Runner.Running)
                    {
                        Thread.Sleep(10);
                    }
                    writer.WriteLine("### - re-exec complete");
                    writer.Flush();
                    writer.WriteLine("### - Beginning readback");

                    writer.WriteLine("### - Beginning test of DirectoryPath");
                    foreach (NodeModel n in nodes)
                    {
                        if (n.OutPorts.Count > 0)
                        {
                            try
                            {
                                String valmap = valueMap[n.GUID].ToString();
                                Object data = n.GetValue(0).Data;
                                String nodeVal = data != null ? data.ToString() : "null";

                                if (valmap != nodeVal)
                                {
                                    writer.WriteLine("!!!!!!!!!!! - test of DirectoryPath is failed");
                                    writer.WriteLine(n.GUID);

                                    writer.WriteLine("Was: " + nodeVal);
                                    writer.WriteLine("Should have been: " + valmap);
                                    writer.Flush();
                                    return;

                                    Debug.WriteLine("==========> Failure on run: " + i);
                                    Debug.WriteLine("Lookup map failed to agree");
                                    Validity.Assert(false);
                                }
                            }
                            catch (Exception)
                            {
                                writer.WriteLine("!!!!!!!!!!! - test of DirectoryPath is failed");
                                writer.Flush();
                                return;
                            }
                        }
                    }
                    writer.WriteLine("### - test of DirectoryPath complete");
                    writer.Flush();
                }
                passed = true;
            }
            finally
            {
                dynamoViewModel.Model.Logger.Log("DirectoryPathTest : " + (passed ? "pass" : "FAIL"));
            }
        }

        private void FilePathTest(StreamWriter writer)
        {
            bool passed = false;
            try
            {
                Random rand = new Random(1);

                for (int i = 0; i < 1000; i++)
                {
                    writer.WriteLine("##### - Beginning run: " + i);

                    string assemblyPath = Assembly.GetExecutingAssembly().Location;
                    string assemblyDir = Path.GetDirectoryName(assemblyPath);
                    string pathToNodesDll = assemblyDir + "\\nodes\\DSCoreNodesUI.dll";
                    Assembly assembly = Assembly.LoadFile(pathToNodesDll);

                    Type type = assembly.GetType("DSCore.File.Filename");
                    List<NodeModel> nodes = new List<NodeModel>();
                    if (type != null)
                        nodes = dynamoViewModel.Model.Nodes.Where(t => t.GetType() == type).ToList();

                    if (nodes.Count == 0)
                        return;

                    writer.WriteLine("### - Beginning eval");
                    dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                    {
                        DynamoViewModel.RunCancelCommand runCancel =
                            new DynamoViewModel.RunCancelCommand(false, false);
                        dynamoViewModel.ExecuteCommand(runCancel);
                    }));
                    while (dynamoViewModel.Model.Runner.Running)
                    {
                        Thread.Sleep(10);
                    }
                    writer.WriteLine("### - Eval complete");
                    writer.Flush();

                    Dictionary<Guid, String> valueMap = new Dictionary<Guid, String>();
                    foreach (NodeModel n in nodes)
                    {
                        if (n.OutPorts.Count > 0)
                        {
                            Guid guid = n.GUID;
                            Object data = n.GetValue(0).Data;
                            String val = data != null ? data.ToString() : "null";
                            valueMap.Add(guid, val);
                            writer.WriteLine(guid + " :: " + val);
                            writer.Flush();
                        }
                    }

                    List<AbstractMutator> mutators = new List<AbstractMutator>()
                    {
                        new FilePathMutator(dynamoViewModel, rand)
                    };
                    AbstractMutator mutator = mutators[rand.Next(mutators.Count)];
                    int numberOfUndosNeeded = mutator.Mutate();
                    Thread.Sleep(100);

                    writer.WriteLine("### - Beginning undo");
                    for (int iUndo = 0; iUndo < numberOfUndosNeeded; iUndo++)
                    {
                        dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                        {
                            DynamoViewModel.UndoRedoCommand undoCommand =
                                new DynamoViewModel.UndoRedoCommand(
                                    DynamoViewModel.UndoRedoCommand.Operation.Undo);
                            dynamoViewModel.ExecuteCommand(undoCommand);
                        }));
                        Thread.Sleep(100);
                    }
                    writer.WriteLine("### - undo complete");
                    writer.Flush();
                    writer.WriteLine("### - Beginning re-exec");

                    dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                    {
                        DynamoViewModel.RunCancelCommand runCancel =
                            new DynamoViewModel.RunCancelCommand(false, false);

                        dynamoViewModel.ExecuteCommand(runCancel);

                    }));
                    Thread.Sleep(10);

                    while (dynamoViewModel.Model.Runner.Running)
                    {
                        Thread.Sleep(10);
                    }
                    writer.WriteLine("### - re-exec complete");
                    writer.Flush();
                    writer.WriteLine("### - Beginning readback");

                    writer.WriteLine("### - Beginning test of FilePath");
                    foreach (NodeModel n in nodes)
                    {
                        if (n.OutPorts.Count > 0)
                        {
                            try
                            {
                                String valmap = valueMap[n.GUID].ToString();
                                Object data = n.GetValue(0).Data;
                                String nodeVal = data != null ? data.ToString() : "null";

                                if (valmap != nodeVal)
                                {
                                    writer.WriteLine("!!!!!!!!!!! - test of FilePath is failed");
                                    writer.WriteLine(n.GUID);

                                    writer.WriteLine("Was: " + nodeVal);
                                    writer.WriteLine("Should have been: " + valmap);
                                    writer.Flush();
                                    return;

                                    Debug.WriteLine("==========> Failure on run: " + i);
                                    Debug.WriteLine("Lookup map failed to agree");
                                    Validity.Assert(false);
                                }
                            }
                            catch (Exception)
                            {
                                writer.WriteLine("!!!!!!!!!!! - test of FilePath is failed");
                                writer.Flush();
                                return;
                            }
                        }
                    }
                    writer.WriteLine("### - test of FilePath complete");
                    writer.Flush();
                }
                passed = true;
            }
            finally
            {
                dynamoViewModel.Model.Logger.Log("FilePathTest : " + (passed ? "pass" : "FAIL"));
            }
        }

        #endregion

        # region Number/String Input Tests

        private void NumberInputTest(StreamWriter writer)
        {
            bool passed = false;
            try
            {
                Random rand = new Random(1);

                for (int i = 0; i < 1000; i++)
                {
                    writer.WriteLine("##### - Beginning run: " + i);
                    List<NodeModel> nodes = dynamoViewModel.Model.Nodes.Where(t => t.GetType() == typeof(DoubleInput)).ToList();

                    if (nodes.Count == 0)
                        return;

                    writer.WriteLine("### - Beginning eval");
                    dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                    {
                        DynamoViewModel.RunCancelCommand runCancel =
                            new DynamoViewModel.RunCancelCommand(false, false);
                        dynamoViewModel.ExecuteCommand(runCancel);
                    }));
                    while (dynamoViewModel.Model.Runner.Running)
                    {
                        Thread.Sleep(10);
                    }
                    writer.WriteLine("### - Eval complete");
                    writer.Flush();

                    Dictionary<Guid, String> valueMap = new Dictionary<Guid, String>();
                    foreach (NodeModel n in nodes)
                    {
                        if (n.OutPorts.Count > 0)
                        {
                            Guid guid = n.GUID;
                            Object data = n.GetValue(0).Data;
                            String val = data != null ? data.ToString() : "null";
                            valueMap.Add(guid, val);
                            writer.WriteLine(guid + " :: " + val);
                            writer.Flush();
                        }
                    }

                    List<AbstractMutator> mutators = new List<AbstractMutator>()
                    {
                        new NumberInputMutator(dynamoViewModel, rand)
                    };
                    AbstractMutator mutator = mutators[rand.Next(mutators.Count)];
                    int numberOfUndosNeeded = mutator.Mutate();
                    Thread.Sleep(100);

                    writer.WriteLine("### - Beginning undo");
                    for (int iUndo = 0; iUndo < numberOfUndosNeeded; iUndo++)
                    {
                        dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                        {
                            DynamoViewModel.UndoRedoCommand undoCommand =
                                new DynamoViewModel.UndoRedoCommand(
                                    DynamoViewModel.UndoRedoCommand.Operation.Undo);
                            dynamoViewModel.ExecuteCommand(undoCommand);
                        }));
                        Thread.Sleep(100);
                    }
                    writer.WriteLine("### - undo complete");
                    writer.Flush();
                    writer.WriteLine("### - Beginning re-exec");

                    dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                    {
                        DynamoViewModel.RunCancelCommand runCancel =
                            new DynamoViewModel.RunCancelCommand(false, false);

                        dynamoViewModel.ExecuteCommand(runCancel);

                    }));
                    Thread.Sleep(10);

                    while (dynamoViewModel.Model.Runner.Running)
                    {
                        Thread.Sleep(10);
                    }
                    writer.WriteLine("### - re-exec complete");
                    writer.Flush();
                    writer.WriteLine("### - Beginning readback");

                    writer.WriteLine("### - Beginning test of Number");
                    foreach (NodeModel n in nodes)
                    {
                        if (n.OutPorts.Count > 0)
                        {
                            try
                            {
                                String valmap = valueMap[n.GUID].ToString();
                                Object data = n.GetValue(0).Data;
                                String nodeVal = data != null ? data.ToString() : "null";

                                if (valmap != nodeVal)
                                {
                                    writer.WriteLine("!!!!!!!!!!! - test of Number is failed");
                                    writer.WriteLine(n.GUID);

                                    writer.WriteLine("Was: " + nodeVal);
                                    writer.WriteLine("Should have been: " + valmap);
                                    writer.Flush();
                                    return;

                                    Debug.WriteLine("==========> Failure on run: " + i);
                                    Debug.WriteLine("Lookup map failed to agree");
                                    Validity.Assert(false);
                                }
                            }
                            catch (Exception)
                            {
                                writer.WriteLine("!!!!!!!!!!! - test of Number is failed");
                                writer.Flush();
                                return;
                            }
                        }
                    }
                    writer.WriteLine("### - test of Number complete");
                    writer.Flush();
                }
                passed = true;
            }
            finally
            {
                dynamoViewModel.Model.Logger.Log("NumberTest : " + (passed ? "pass" : "FAIL"));
            }
        }

        private void StringInputTest(StreamWriter writer)
        {
            bool passed = false;
            try
            {
                Random rand = new Random(1);

                for (int i = 0; i < 1000; i++)
                {
                    writer.WriteLine("##### - Beginning run: " + i);
                    List<NodeModel> nodes = dynamoViewModel.Model.Nodes.Where(t => t.GetType() == typeof(StringInput)).ToList();

                    if (nodes.Count == 0)
                        return;

                    writer.WriteLine("### - Beginning eval");
                    dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                    {
                        DynamoViewModel.RunCancelCommand runCancel =
                            new DynamoViewModel.RunCancelCommand(false, false);
                        dynamoViewModel.ExecuteCommand(runCancel);
                    }));
                    while (dynamoViewModel.Model.Runner.Running)
                    {
                        Thread.Sleep(10);
                    }
                    writer.WriteLine("### - Eval complete");
                    writer.Flush();

                    Dictionary<Guid, String> valueMap = new Dictionary<Guid, String>();
                    foreach (NodeModel n in nodes)
                    {
                        if (n.OutPorts.Count > 0)
                        {
                            Guid guid = n.GUID;
                            Object data = n.GetValue(0).Data;
                            String val = data != null ? data.ToString() : "null";
                            valueMap.Add(guid, val);
                            writer.WriteLine(guid + " :: " + val);
                            writer.Flush();
                        }
                    }

                    List<AbstractMutator> mutators = new List<AbstractMutator>()
                    {
                        new StringInputMutator(dynamoViewModel, rand)
                    };
                    AbstractMutator mutator = mutators[rand.Next(mutators.Count)];
                    int numberOfUndosNeeded = mutator.Mutate();
                    Thread.Sleep(100);

                    writer.WriteLine("### - Beginning undo");
                    for (int iUndo = 0; iUndo < numberOfUndosNeeded; iUndo++)
                    {
                        dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                        {
                            DynamoViewModel.UndoRedoCommand undoCommand =
                                new DynamoViewModel.UndoRedoCommand(
                                    DynamoViewModel.UndoRedoCommand.Operation.Undo);
                            dynamoViewModel.ExecuteCommand(undoCommand);
                        }));
                        Thread.Sleep(100);
                    }
                    writer.WriteLine("### - undo complete");
                    writer.Flush();

                    writer.WriteLine("### - undo complete");
                    writer.Flush();
                    writer.WriteLine("### - Beginning re-exec");

                    dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                    {
                        DynamoViewModel.RunCancelCommand runCancel =
                            new DynamoViewModel.RunCancelCommand(false, false);

                        dynamoViewModel.ExecuteCommand(runCancel);

                    }));
                    Thread.Sleep(10);

                    while (dynamoViewModel.Model.Runner.Running)
                    {
                        Thread.Sleep(10);
                    }
                    writer.WriteLine("### - re-exec complete");
                    writer.Flush();
                    writer.WriteLine("### - Beginning readback");

                    writer.WriteLine("### - Beginning test of String");
                    foreach (NodeModel n in nodes)
                    {
                        if (n.OutPorts.Count > 0)
                        {
                            try
                            {
                                String valmap = valueMap[n.GUID].ToString();
                                Object data = n.GetValue(0).Data;
                                String nodeVal = data != null ? data.ToString() : "null";

                                if (valmap != nodeVal)
                                {
                                    writer.WriteLine("!!!!!!!!!!! - test of String is failed");
                                    writer.WriteLine(n.GUID);

                                    writer.WriteLine("Was: " + nodeVal);
                                    writer.WriteLine("Should have been: " + valmap);
                                    writer.Flush();
                                    return;

                                    Debug.WriteLine("==========> Failure on run: " + i);
                                    Debug.WriteLine("Lookup map failed to agree");
                                    Validity.Assert(false);
                                }
                            }
                            catch (Exception)
                            {
                                writer.WriteLine("!!!!!!!!!!! - test of String is failed");
                                writer.Flush();
                                return;
                            }
                        }
                    }
                    writer.WriteLine("### - test of Number complete");
                    writer.Flush();
                }
                passed = true;
            }
            finally
            {
                dynamoViewModel.Model.Logger.Log("StringTest : " + (passed ? "pass" : "FAIL"));
            }
        }

        #endregion

        #region Sequence/Range/List Tests

        private void NumberSequenceTest(StreamWriter writer)
        {
            bool passed = false;

            try
            {
                string assemblyPath = Assembly.GetExecutingAssembly().Location;
                string assemblyDir = Path.GetDirectoryName(assemblyPath);
                string pathToNodesDll = assemblyDir + "\\nodes\\DSCoreNodesUI.dll";
                Assembly assembly = Assembly.LoadFile(pathToNodesDll);

                Type type = assembly.GetType("DSCoreNodesUI.NumberSeq");
                List<NodeModel> nodes = new List<NodeModel>();
                if (type != null)
                    nodes = dynamoViewModel.Model.Nodes.Where(t => t.GetType() == type).ToList();

                if (nodes.Count == 0)
                    return;

                foreach (NodeModel n in nodes)
                {
                    try
                    {
                        Random rand = new Random(1);

                        Guid guidNumber1 = Guid.Parse("fa532273-cf1d-4f41-874e-6146f634e2d3"); //Guid of the node "Number" to connect to the node "Number Sequence" on Start
                        Guid guidNumber2 = Guid.Parse("788dfa62-dbb2-4556-ad13-ce20ccc5ec0d"); //Guid of the node "Number" to connect to the node "Number Sequence" on Amount
                        Guid guidNumber3 = Guid.Parse("7bfb0b00-3dbc-4ab4-ba6b-f7743b72bbc5"); //Guid of the node "Number" to connect to the node "Number Sequence" on Step

                        dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                        {
                            double coordinatesX = 120;
                            double coordinatesY = 180;

                            //create nodes
                            DynamoViewModel.CreateNodeCommand createNodeNumber1 =
                                new DynamoViewModel.CreateNodeCommand(guidNumber1, "Number", coordinatesX, coordinatesY, false, true);
                            DynamoViewModel.CreateNodeCommand createNodeNumber2 =
                                new DynamoViewModel.CreateNodeCommand(guidNumber2, "Number", coordinatesX, coordinatesY + 100, false, true);
                            DynamoViewModel.CreateNodeCommand createNodeNumber3 =
                                new DynamoViewModel.CreateNodeCommand(guidNumber3, "Number", coordinatesX, coordinatesY + 200, false, true);

                            //execute commands
                            dynamoViewModel.ExecuteCommand(createNodeNumber1);
                            dynamoViewModel.ExecuteCommand(createNodeNumber2);
                            dynamoViewModel.ExecuteCommand(createNodeNumber3);
                        }));

                        for (int i = 0; i < 1000; i++)
                        {
                            writer.WriteLine("##### - Beginning run: " + i);

                            writer.WriteLine("### - Beginning eval");
                            dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                            {
                                DynamoViewModel.RunCancelCommand runCancel =
                                    new DynamoViewModel.RunCancelCommand(false, false);
                                dynamoViewModel.ExecuteCommand(runCancel);
                            }));
                            while (dynamoViewModel.Model.Runner.Running)
                            {
                                Thread.Sleep(10);
                            }
                            writer.WriteLine("### - Eval complete");
                            writer.Flush();

                            Dictionary<Guid, String> valueMap = new Dictionary<Guid, String>();
                            if (n.OutPorts.Count > 0)
                            {
                                List<ConnectorModel> firstNodeConnectors = n.AllConnectors.ToList(); //Get node connectors
                                foreach (ConnectorModel connector in firstNodeConnectors)
                                {
                                    Guid guid = connector.Start.Owner.GUID;
                                    if (!valueMap.ContainsKey(guid))
                                    {
                                        Object data = connector.Start.Owner.GetValue(0).Data;
                                        String val = data != null ? data.ToString() : "null";
                                        valueMap.Add(guid, val);
                                        writer.WriteLine(guid + " :: " + val);
                                        writer.Flush();
                                    }
                                }
                            }

                            List<AbstractMutator> mutators = new List<AbstractMutator>()
                            {
                                new NumberSequenceMutator(dynamoViewModel, rand)
                            };
                            AbstractMutator mutator = mutators[rand.Next(mutators.Count)];
                            int numberOfUndosNeeded = mutator.Mutate();
                            Thread.Sleep(100);

                            writer.WriteLine("### - Beginning undo");
                            for (int iUndo = 0; iUndo < numberOfUndosNeeded; iUndo++)
                            {
                                List<NodeModel> numberSeqNodes = new List<NodeModel>();
                                numberSeqNodes = dynamoViewModel.Model.Nodes.Where(t => t.GetType() == type).ToList();

                                dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                                {
                                    for (int j = 0; j < 3 * numberSeqNodes.Count; j++)
                                    {
                                        DynamoViewModel.UndoRedoCommand undoCommand =
                                            new DynamoViewModel.UndoRedoCommand(DynamoViewModel.UndoRedoCommand.Operation.Undo);
                                        dynamoViewModel.ExecuteCommand(undoCommand);
                                    }
                                }));

                                Thread.Sleep(100);
                            }
                            writer.WriteLine("### - undo complete");
                            writer.Flush();

                            dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                            {
                                DynamoViewModel.RunCancelCommand runCancel =
                                    new DynamoViewModel.RunCancelCommand(false, false);
                                dynamoViewModel.ExecuteCommand(runCancel);
                            }));
                            while (dynamoViewModel.Model.Runner.Running)
                            {
                                Thread.Sleep(10);
                            }

                            writer.WriteLine("### - Beginning test of NumberSequence");
                            if (n.OutPorts.Count > 0)
                            {
                                try
                                {
                                    List<ConnectorModel> firstNodeConnectors = n.AllConnectors.ToList();
                                    foreach (ConnectorModel connector in firstNodeConnectors)
                                    {
                                        String valmap = valueMap[connector.Start.Owner.GUID].ToString();
                                        Object data = connector.Start.Owner.GetValue(0).Data;
                                        String nodeVal = data != null ? data.ToString() : "null";

                                        if (valmap != nodeVal)
                                        {
                                            writer.WriteLine("!!!!!!!!!!! - test of NumberSequence is failed");
                                            writer.WriteLine(n.GUID);

                                            writer.WriteLine("Was: " + nodeVal);
                                            writer.WriteLine("Should have been: " + valmap);
                                            writer.Flush();
                                            return;

                                            Debug.WriteLine("==========> Failure on run: " + i);
                                            Debug.WriteLine("Lookup map failed to agree");
                                            Validity.Assert(false);
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                    writer.WriteLine("!!!!!!!!!!! - test of NumberSequence is failed");
                                    writer.Flush();
                                    return;
                                }
                            }

                            writer.WriteLine("### - test of NumberSequence complete");
                            writer.Flush();
                        }

                        passed = true;
                    }
                    finally
                    {
                        dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                        {
                            DynamoViewModel.DeleteModelCommand delNumberCommand1 =
                                new DynamoViewModel.DeleteModelCommand(Guid.Parse("fa532273-cf1d-4f41-874e-6146f634e2d3"));
                            DynamoViewModel.DeleteModelCommand delNumberCommand2 =
                                new DynamoViewModel.DeleteModelCommand(Guid.Parse("788dfa62-dbb2-4556-ad13-ce20ccc5ec0d"));
                            DynamoViewModel.DeleteModelCommand delNumberCommand3 =
                                new DynamoViewModel.DeleteModelCommand(Guid.Parse("7bfb0b00-3dbc-4ab4-ba6b-f7743b72bbc5"));

                            dynamoViewModel.ExecuteCommand(delNumberCommand1);
                            dynamoViewModel.ExecuteCommand(delNumberCommand2);
                            dynamoViewModel.ExecuteCommand(delNumberCommand3);
                        }));

                    }
                }
            }
            finally
            {
                dynamoViewModel.Model.Logger.Log("NumberSequenceTest : " + (passed ? "pass" : "FAIL"));
            }
        }

        private void NumberRangeTest(StreamWriter writer)
        {
            bool passed = false;

            try
            {
                string assemblyPath = Assembly.GetExecutingAssembly().Location;
                string assemblyDir = Path.GetDirectoryName(assemblyPath);
                string pathToNodesDll = assemblyDir + "\\nodes\\DSCoreNodesUI.dll";
                Assembly assembly = Assembly.LoadFile(pathToNodesDll);

                Type type = assembly.GetType("DSCoreNodesUI.NumberRange");
                List<NodeModel> nodes = new List<NodeModel>();
                if (type != null)
                    nodes = dynamoViewModel.Model.Nodes.Where(t => t.GetType() == type).ToList();

                if (nodes.Count == 0)
                    return;

                foreach (NodeModel n in nodes)
                {
                    try
                    {
                        Random rand = new Random(1);

                        Guid guidNumber1 = Guid.Parse("fa532273-cf1d-4f41-874e-6146f634e2d3"); //Guid of the node "Number" to connect to the node "Number Range" on Start
                        Guid guidNumber2 = Guid.Parse("788dfa62-dbb2-4556-ad13-ce20ccc5ec0d"); //Guid of the node "Number" to connect to the node "Number Range" on Amount
                        Guid guidNumber3 = Guid.Parse("7bfb0b00-3dbc-4ab4-ba6b-f7743b72bbc5"); //Guid of the node "Number" to connect to the node "Number Range" on Step

                        dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                        {
                            double coordinatesX = 120;
                            double coordinatesY = 180;

                            //create nodes
                            DynamoViewModel.CreateNodeCommand createNodeCmd1 =
                                new DynamoViewModel.CreateNodeCommand(guidNumber1, "Number", coordinatesX, coordinatesY, false, true);
                            DynamoViewModel.CreateNodeCommand createNodeCmd2 =
                                new DynamoViewModel.CreateNodeCommand(guidNumber2, "Number", coordinatesX, coordinatesY + 100, false, true);
                            DynamoViewModel.CreateNodeCommand createNodeCmd3 =
                                new DynamoViewModel.CreateNodeCommand(guidNumber3, "Number", coordinatesX, coordinatesY + 200, false, true);

                            //execute commands
                            dynamoViewModel.ExecuteCommand(createNodeCmd1);
                            dynamoViewModel.ExecuteCommand(createNodeCmd2);
                            dynamoViewModel.ExecuteCommand(createNodeCmd3);
                        }));

                        for (int i = 0; i < 1000; i++)
                        {
                            writer.WriteLine("##### - Beginning run: " + i);

                            writer.WriteLine("### - Beginning eval");
                            dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                            {
                                DynamoViewModel.RunCancelCommand runCancel =
                                    new DynamoViewModel.RunCancelCommand(false, false);
                                dynamoViewModel.ExecuteCommand(runCancel);
                            }));
                            while (dynamoViewModel.Model.Runner.Running)
                            {
                                Thread.Sleep(10);
                            }
                            writer.WriteLine("### - Eval complete");
                            writer.Flush();

                            Dictionary<Guid, String> valueMap = new Dictionary<Guid, String>();
                            if (n.OutPorts.Count > 0)
                            {
                                List<ConnectorModel> firstNodeConnectors = n.AllConnectors.ToList(); //Get node connectors
                                foreach (ConnectorModel connector in firstNodeConnectors)
                                {
                                    Guid guid = connector.Start.Owner.GUID;
                                    if (!valueMap.ContainsKey(guid))
                                    {
                                        Object data = connector.Start.Owner.GetValue(0).Data;
                                        String val = data != null ? data.ToString() : "null";
                                        valueMap.Add(guid, val);
                                        writer.WriteLine(guid + " :: " + val);
                                        writer.Flush();
                                    }
                                }
                            }

                            List<AbstractMutator> mutators = new List<AbstractMutator>()
                            {
                                new NumberRangeMutator(dynamoViewModel, rand)
                            };
                            AbstractMutator mutator = mutators[rand.Next(mutators.Count)];
                            int numberOfUndosNeeded = mutator.Mutate();
                            Thread.Sleep(100);

                            writer.WriteLine("### - Beginning undo");
                            for (int iUndo = 0; iUndo < numberOfUndosNeeded; iUndo++)
                            {
                                List<NodeModel> numberRangeNodes = new List<NodeModel>();
                                numberRangeNodes = dynamoViewModel.Model.Nodes.Where(t => t.GetType() == type).ToList();

                                dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                                {
                                    for (int j = 0; j < 3 * numberRangeNodes.Count; j++)
                                    {
                                        DynamoViewModel.UndoRedoCommand undoCommand =
                                            new DynamoViewModel.UndoRedoCommand(DynamoViewModel.UndoRedoCommand.Operation.Undo);
                                        dynamoViewModel.ExecuteCommand(undoCommand);
                                    }
                                }));

                                Thread.Sleep(100);
                            }
                            writer.WriteLine("### - undo complete");
                            writer.Flush();

                            dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                            {
                                DynamoViewModel.RunCancelCommand runCancel =
                                    new DynamoViewModel.RunCancelCommand(false, false);
                                dynamoViewModel.ExecuteCommand(runCancel);
                            }));
                            while (dynamoViewModel.Model.Runner.Running)
                            {
                                Thread.Sleep(10);
                            }

                            writer.WriteLine("### - Beginning test of NumberRange");
                            if (n.OutPorts.Count > 0)
                            {
                                try
                                {
                                    List<ConnectorModel> firstNodeConnectors = n.AllConnectors.ToList();
                                    foreach (ConnectorModel connector in firstNodeConnectors)
                                    {
                                        String valmap = valueMap[connector.Start.Owner.GUID].ToString();
                                        Object data = connector.Start.Owner.GetValue(0).Data;
                                        String nodeVal = data != null ? data.ToString() : "null";

                                        if (valmap != nodeVal)
                                        {
                                            writer.WriteLine("!!!!!!!!!!! - test of NumberRange is failed");
                                            writer.WriteLine(n.GUID);

                                            writer.WriteLine("Was: " + nodeVal);
                                            writer.WriteLine("Should have been: " + valmap);
                                            writer.Flush();
                                            return;

                                            Debug.WriteLine("==========> Failure on run: " + i);
                                            Debug.WriteLine("Lookup map failed to agree");
                                            Validity.Assert(false);
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                    writer.WriteLine("!!!!!!!!!!! - test of NumberRange is failed");
                                    writer.Flush();
                                    return;
                                }
                            }
                            writer.WriteLine("### - test of NumberRange complete");
                            writer.Flush();
                        }
                        passed = true;
                    }
                    finally
                    {
                        dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                        {
                            DynamoViewModel.DeleteModelCommand delNumberCommand1 =
                                new DynamoViewModel.DeleteModelCommand(Guid.Parse("fa532273-cf1d-4f41-874e-6146f634e2d3"));
                            DynamoViewModel.DeleteModelCommand delNumberCommand2 =
                                new DynamoViewModel.DeleteModelCommand(Guid.Parse("788dfa62-dbb2-4556-ad13-ce20ccc5ec0d"));
                            DynamoViewModel.DeleteModelCommand delNumberCommand3 =
                                new DynamoViewModel.DeleteModelCommand(Guid.Parse("7bfb0b00-3dbc-4ab4-ba6b-f7743b72bbc5"));

                            dynamoViewModel.ExecuteCommand(delNumberCommand1);
                            dynamoViewModel.ExecuteCommand(delNumberCommand2);
                            dynamoViewModel.ExecuteCommand(delNumberCommand3);
                        }));
                    }
                }
            }

            finally
            {
                dynamoViewModel.Model.Logger.Log("NumberRangeTest : " + (passed ? "pass" : "FAIL"));
            }
        }

        private void ListTest(StreamWriter writer)
        {
            bool passed = false;

            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string assemblyDir = Path.GetDirectoryName(assemblyPath);
            string pathToNodesDll = assemblyDir + "\\nodes\\DSCoreNodesUI.dll";
            Assembly assembly = Assembly.LoadFile(pathToNodesDll);

            Type type = assembly.GetType("DSCoreNodesUI.CreateList");
            List<NodeModel> nodes = new List<NodeModel>();
            if (type != null)
                nodes = dynamoViewModel.Model.Nodes.Where(t => t.GetType() == type).ToList();

            if (nodes.Count == 0)
                return;

            foreach (NodeModel n in nodes)
            {
                try
                {
                    Random rand = new Random(1);

                    Guid guidNumber1 = Guid.Parse("fa532273-cf1d-4f41-874e-6146f634e2d3"); //Guid of the node "Number" to coonect to the node "List" on InPorts(0)

                    dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                    {
                        double coordinatesX = 120;
                        double coordinatesY = 180;

                        //create auxiliar node
                        DynamoViewModel.CreateNodeCommand createNodeCmd =
                            new DynamoViewModel.CreateNodeCommand(guidNumber1, "Number", coordinatesX, coordinatesY, false, true);

                        //execute command
                        dynamoViewModel.ExecuteCommand(createNodeCmd);
                    }));

                    for (int i = 0; i < 1000; i++)
                    {
                        writer.WriteLine("##### - Beginning run: " + i);
                        writer.WriteLine("### - Beginning eval");
                        dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                        {
                            DynamoViewModel.RunCancelCommand runCancel =
                                new DynamoViewModel.RunCancelCommand(false, false);
                            dynamoViewModel.ExecuteCommand(runCancel);
                        }));
                        while (dynamoViewModel.Model.Runner.Running)
                        {
                            Thread.Sleep(10);
                        }
                        writer.WriteLine("### - Eval complete");
                        writer.Flush();

                        Dictionary<Guid, String> valueMap = new Dictionary<Guid, String>();
                        if (n.OutPorts.Count > 0)
                        {
                            List<ConnectorModel> firstNodeConnectors = n.AllConnectors.ToList(); //Get node connectors
                            foreach (ConnectorModel connector in firstNodeConnectors)
                            {
                                Guid guid = connector.Start.Owner.GUID;
                                if (!valueMap.ContainsKey(guid))
                                {
                                    Object data = connector.Start.Owner.GetValue(0).Data;
                                    String val = data != null ? data.ToString() : "null";
                                    valueMap.Add(guid, val);
                                    writer.WriteLine(guid + " :: " + val);
                                    writer.Flush();
                                }
                            }
                        }

                        List<AbstractMutator> mutators = new List<AbstractMutator>()
                        {
                            new ListMutator(dynamoViewModel, rand)
                        };
                        AbstractMutator mutator = mutators[rand.Next(mutators.Count)];
                        int numberOfUndosNeeded = mutator.Mutate();
                        Thread.Sleep(100);

                        writer.WriteLine("### - Beginning undo");
                        for (int iUndo = 0; iUndo < numberOfUndosNeeded; iUndo++)
                        {
                            dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                            {
                                List<NodeModel> listNodes = new List<NodeModel>();
                                listNodes = dynamoViewModel.Model.Nodes.Where(t => t.GetType() == type).ToList();

                                for (int j = 0; j < listNodes.Count; j++)
                                {
                                    DynamoViewModel.UndoRedoCommand undoCommand =
                                        new DynamoViewModel.UndoRedoCommand(DynamoViewModel.UndoRedoCommand.Operation.Undo);
                                    dynamoViewModel.ExecuteCommand(undoCommand);
                                }
                            }));
                            Thread.Sleep(100);
                        }
                        writer.WriteLine("### - undo complete");
                        writer.Flush();

                        dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                        {
                            DynamoViewModel.RunCancelCommand runCancel =
                                new DynamoViewModel.RunCancelCommand(false, false);
                            dynamoViewModel.ExecuteCommand(runCancel);
                        }));
                        while (dynamoViewModel.Model.Runner.Running)
                        {
                            Thread.Sleep(10);
                        }

                        writer.WriteLine("### - Beginning test of List");
                        if (n.OutPorts.Count > 0)
                        {
                            try
                            {
                                List<ConnectorModel> firstNodeConnectors = n.AllConnectors.ToList();
                                foreach (ConnectorModel connector in firstNodeConnectors)
                                {
                                    String valmap = valueMap[connector.Start.Owner.GUID].ToString();
                                    Object data = connector.Start.Owner.GetValue(0).Data;
                                    String nodeVal = data != null ? data.ToString() : "null";

                                    if (valmap != nodeVal)
                                    {
                                        writer.WriteLine("!!!!!!!!!!! - test of List is failed");
                                        writer.WriteLine(n.GUID);

                                        writer.WriteLine("Was: " + nodeVal);
                                        writer.WriteLine("Should have been: " + valmap);
                                        writer.Flush();
                                        return;

                                        Debug.WriteLine("==========> Failure on run: " + i);
                                        Debug.WriteLine("Lookup map failed to agree");
                                        Validity.Assert(false);
                                    }
                                    else
                                    {
                                        writer.WriteLine("### - test of List is passed");
                                        writer.Flush();
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                writer.WriteLine("!!!!!!!!!!! - test of List is failed");
                                writer.Flush();
                                return;
                            }
                        }
                        writer.WriteLine("### - test of List complete");
                        writer.Flush();
                    }
                    passed = true;
                }
                finally
                {
                    dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                    {
                        DynamoViewModel.DeleteModelCommand delNumberCmd =
                            new DynamoViewModel.DeleteModelCommand(Guid.Parse("fa532273-cf1d-4f41-874e-6146f634e2d3"));

                        dynamoViewModel.ExecuteCommand(delNumberCmd);
                    }));

                    dynamoViewModel.Model.Logger.Log("ListTest : " + (passed ? "pass" : "FAIL"));
                }
            }
        }

        #endregion

        #region Custom node tests

        private void CustomNodeCompatibilityTest(StreamWriter writer)
        {
            bool passed = false;

            List<Type> types = LoadAllTypesFromDynamoAssemblies();

            List<NodeModel> nodes = dynamoViewModel.Model.Nodes.Where(t => t.GetType() == typeof(Function)).ToList();

            if (nodes.Count == 0)
                return;

            try
            {
                Random rand = new Random(1);

                foreach (NodeModel node in nodes)
                {
                    List<ConnectorModel> firstNodeConnectors = node.AllConnectors.ToList();

                    foreach (Type type in types)
                    {
                        writer.WriteLine("##### - Beginning run for type: " + type.ToString());
                        writer.WriteLine("### - Beginning eval");

                        dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                        {
                            DynamoViewModel.RunCancelCommand runCancel =
                                new DynamoViewModel.RunCancelCommand(false, false);
                            dynamoViewModel.ExecuteCommand(runCancel);
                        }));

                        while (dynamoViewModel.Model.Runner.Running)
                        {
                            Thread.Sleep(10);
                        }

                        writer.WriteLine("### - Eval complete");
                        writer.Flush();

                        double coordinatesX = node.X;
                        double coordinatesY = node.Y;

                        string nodeName = GetName(type);

                        if (!string.IsNullOrEmpty(nodeName))
                        {
                            dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                            {
                                Guid guidNumber = Guid.NewGuid();

                                DynamoViewModel.CreateNodeCommand createCommand =
                               new DynamoViewModel.CreateNodeCommand(guidNumber, nodeName, coordinatesX, coordinatesY, false, false);
                                dynamoViewModel.ExecuteCommand(createCommand);
                            }));

                            Dictionary<Guid, String> valueMap = new Dictionary<Guid, String>();
                            foreach (ConnectorModel connector in firstNodeConnectors)
                            {
                                Guid guid = connector.Start.Owner.GUID;
                                Object data = connector.Start.Owner.GetValue(0).Data;
                                String val = data != null ? data.ToString() : "null";
                                valueMap.Add(guid, val);
                                writer.WriteLine(guid + " :: " + val);
                                writer.Flush();
                            }

                            List<AbstractMutator> mutators = new List<AbstractMutator>()
                        {
                            new CustomNodeCompatibilityMutator(dynamoViewModel, rand)
                        };

                            AbstractMutator mutator = mutators[rand.Next(mutators.Count)];
                            int numberOfUndosNeeded = mutator.Mutate();
                            Thread.Sleep(100);

                            writer.WriteLine("### - Beginning undo");
                            for (int iUndo = 0; iUndo < numberOfUndosNeeded; iUndo++)
                            {
                                dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                                {
                                    for (int j = 0; j < 3; j++)
                                    {
                                        DynamoViewModel.UndoRedoCommand undoCommand =
                                            new DynamoViewModel.UndoRedoCommand(DynamoViewModel.UndoRedoCommand.Operation.Undo);
                                        dynamoViewModel.ExecuteCommand(undoCommand);
                                    }
                                }));
                                Thread.Sleep(100);
                            }
                            writer.WriteLine("### - undo complete");
                            writer.Flush();

                            dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                            {
                                DynamoViewModel.RunCancelCommand runCancel =
                                    new DynamoViewModel.RunCancelCommand(false, false);

                                dynamoViewModel.ExecuteCommand(runCancel);
                            }));
                            while (dynamoViewModel.Model.Runner.Running)
                            {
                                Thread.Sleep(10);
                            }

                            writer.WriteLine("### - Beginning test of CustomNode");
                            if (node.OutPorts.Count > 0)
                            {
                                try
                                {
                                    NodeModel nodeAfterUndo = dynamoViewModel.Model.Nodes.ToList().FirstOrDefault(t => t.GUID == node.GUID);

                                    if (nodeAfterUndo != null)
                                    {
                                        List<ConnectorModel> firstNodeConnectorsAfterUndo = nodeAfterUndo.AllConnectors.ToList();
                                        foreach (ConnectorModel connector in firstNodeConnectors)
                                        {
                                            Guid guid = connector.Start.Owner.GUID;
                                            Object data = connector.Start.Owner.GetValue(0).Data;
                                            String val = data != null ? data.ToString() : "null";

                                            if (valueMap[guid] != val)
                                            {
                                                writer.WriteLine("!!!!!!!!!!! - test of CustomNode is failed");
                                                writer.WriteLine(node.GUID);

                                                writer.WriteLine("Was: " + val);
                                                writer.WriteLine("Should have been: " + valueMap[guid]);
                                                writer.Flush();
                                                return;

                                                Debug.WriteLine("==========> Failure on type: " + type.ToString());
                                                Debug.WriteLine("Lookup map failed to agree");
                                                Validity.Assert(false);
                                            }
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                    writer.WriteLine("!!!!!!!!!!! - test of CustomNode is failed");
                                    writer.Flush();
                                    return;
                                }
                            }
                            writer.WriteLine("### - test of CustomNode complete");
                            writer.Flush();
                        }
                    }
                }

                passed = true;
            }
            finally
            {
                dynamoViewModel.Model.Logger.Log("CustomNodeTest : " + (passed ? "pass" : "FAIL"));
            }
        }

        private void CustomNodeTest(StreamWriter writer)
        {
            bool passed = false;

            List<NodeModel> nodes = dynamoViewModel.Model.CurrentWorkspace.Nodes.Where(t => t.GetType() == typeof(Function)).ToList();

            if (nodes.Count == 0)
                return;

            try
            {
                Random rand = new Random(1);

                for (int i = 0; i < 1000; i++)
                {
                    NodeModel node = nodes[rand.Next(nodes.Count)];

                    writer.WriteLine("##### - Beginning run: " + i);
                    writer.WriteLine("### - Beginning eval");

                    dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                    {
                        DynamoViewModel.RunCancelCommand runCancel =
                            new DynamoViewModel.RunCancelCommand(false, false);
                        dynamoViewModel.ExecuteCommand(runCancel);
                    }));

                    while (dynamoViewModel.Model.Runner.Running)
                    {
                        Thread.Sleep(10);
                    }

                    writer.WriteLine("### - Eval complete");
                    writer.Flush();

                    List<ConnectorModel> firstNodeConnectors = node.AllConnectors.ToList();

                    Dictionary<Guid, String> valueMap = new Dictionary<Guid, String>();
                    foreach (ConnectorModel connector in firstNodeConnectors)
                    {
                        if (connector.End.Owner.GUID != node.GUID)
                        {
                            Guid guid = connector.Start.Owner.GUID;
                            Object data = connector.Start.Owner.GetValue(0).Data;
                            String val = data != null ? data.ToString() : "null";
                            valueMap.Add(guid, val);
                            writer.WriteLine(guid + " :: " + val);
                            writer.Flush();
                        }
                    }

                    string customNodeFilePath = string.Empty;
                    if (node is Function)
                        customNodeFilePath = ((Function)node).Definition.WorkspaceModel.FileName;

                    var workspaces = dynamoViewModel.Model.Workspaces;

                    if (File.Exists(customNodeFilePath))
                    {
                        int currentWorkspaceIndex = dynamoViewModel.CurrentWorkspaceIndex;

                        dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                        {
                            DynamoViewModel.OpenFileCommand openFile =
                                new DynamoViewModel.OpenFileCommand(customNodeFilePath);
                            dynamoViewModel.ExecuteCommand(openFile);
                            Thread.Sleep(100);
                        }));

                        List<NodeModel> nodesInCustomNodeBeforeMutation = workspaces.FirstOrDefault(t => t.Name == ((Function)node).Definition.WorkspaceModel.Name).Nodes.ToList();
                        Dictionary<Guid, String> customNodeStructureBeforeMutation = GetDictionaryOfConnectedNodes(nodesInCustomNodeBeforeMutation);

                        List<AbstractMutator> mutators = new List<AbstractMutator>()
                        {
                            new CustomNodeMutator(dynamoViewModel, currentWorkspaceIndex, rand)
                        };
                        AbstractMutator mutator = mutators[rand.Next(mutators.Count)];
                        int numberOfUndosNeeded = mutator.Mutate();
                        Thread.Sleep(100);

                        writer.WriteLine("### - Beginning undo");
                        for (int iUndo = 0; iUndo < numberOfUndosNeeded; iUndo++)
                        {
                            dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                            {
                                DynamoViewModel.UndoRedoCommand undoCommand =
                                    new DynamoViewModel.UndoRedoCommand(DynamoViewModel.UndoRedoCommand.Operation.Undo);
                                dynamoViewModel.ExecuteCommand(undoCommand);
                            }));
                            Thread.Sleep(100);
                        }
                        writer.WriteLine("### - undo complete");
                        writer.Flush();

                        dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                        {
                            DynamoViewModel.RunCancelCommand runCancel =
                                new DynamoViewModel.RunCancelCommand(false, false);

                            dynamoViewModel.ExecuteCommand(runCancel);
                        }));
                        while (dynamoViewModel.Model.Runner.Running)
                        {
                            Thread.Sleep(10);
                        }

                        dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                        {
                            DynamoViewModel.SwitchTabCommand switchCmd =
                                new DynamoViewModel.SwitchTabCommand(currentWorkspaceIndex);

                            dynamoViewModel.ExecuteCommand(switchCmd);
                            Thread.Sleep(100);
                        }));

                        List<NodeModel> nodesInCustomNodeAfterMutation = workspaces.FirstOrDefault(t => t.Name == ((Function)node).Definition.WorkspaceModel.Name).Nodes.ToList();
                        Dictionary<Guid, String> customNodeStructureAfterMutation = GetDictionaryOfConnectedNodes(nodesInCustomNodeAfterMutation);

                        writer.WriteLine("### - Beginning test of CustomNode structure");
                        if (customNodeStructureBeforeMutation.Count == customNodeStructureAfterMutation.Count)
                        {
                            foreach (var item in customNodeStructureAfterMutation)
                            {
                                if (item.Value != customNodeStructureBeforeMutation[item.Key])
                                {
                                    writer.WriteLine("!!!!!!!!!!! - test of CustomNode structure is failed");
                                    writer.Flush();
                                    return;
                                }
                            }
                        }
                        else
                        {
                            writer.WriteLine("!!!!!!!!!!! - test of CustomNode structure is failed");
                            writer.Flush();
                            return;
                        }

                        writer.WriteLine("### - Beginning test of CustomNode");
                        if (node.OutPorts.Count > 0)
                        {
                            try
                            {
                                NodeModel nodeAfterUndo = workspaces.FirstOrDefault(t => t.GetType() == node.Workspace.GetType()).Nodes.ToList().FirstOrDefault(t => t.GUID == node.GUID);

                                if (nodeAfterUndo == null)
                                {
                                    writer.WriteLine("!!!!!!!!!!! - test of CustomNode is failed");
                                    writer.Flush();
                                    return;
                                }

                                List<ConnectorModel> firstNodeConnectorsAfterUndo = nodeAfterUndo.AllConnectors.ToList();

                                foreach (ConnectorModel connector in firstNodeConnectorsAfterUndo)
                                {
                                    if (connector.End.Owner.GUID != node.GUID)
                                    {
                                        Object data = connector.Start.Owner.GetValue(0).Data;
                                        String nodeVal = data != null ? data.ToString() : "null";

                                        if (valueMap[connector.Start.Owner.GUID] != nodeVal)
                                        {
                                            writer.WriteLine("!!!!!!!!!!! - test of CustomNode is failed");
                                            writer.WriteLine(node.GUID);

                                            writer.WriteLine("Was: " + nodeVal);
                                            writer.WriteLine("Should have been: " + valueMap[connector.End.Owner.GUID]);
                                            writer.Flush();
                                            return;

                                            Debug.WriteLine("==========> Failure on run: " + i);
                                            Debug.WriteLine("Lookup map failed to agree");
                                            Validity.Assert(false);
                                        }
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                writer.WriteLine("!!!!!!!!!!! - test of CustomNode is failed");
                                writer.Flush();
                                return;
                            }
                        }

                        writer.WriteLine("### - test of CustomNode complete");
                        writer.Flush();
                    }
                    passed = true;
                }
            }
            finally
            {
                dynamoViewModel.Model.Logger.Log("CustomNodeTest : " + (passed ? "pass" : "FAIL"));
            }
        }

        #endregion

        #region Auxiliary methods

        private string GetName(Type type)
        {
            string name = string.Empty;
            List<string> excludedTypeNames = new List<string>() { "Code Block", "Custom Node", "Compose Functions", "List.ForEach", "Build Sublists", "Apply Function" };

            var attribs = type.GetCustomAttributes(typeof(NodeNameAttribute), false);
            var attrs = type.GetCustomAttributes(typeof(IsVisibleInDynamoLibraryAttribute), true);
            if (attribs.Length > 0)
            {
                if (!excludedTypeNames.Contains((attribs[0] as NodeNameAttribute).Name))
                    name = (attribs[0] as NodeNameAttribute).Name;

                if ((attrs != null) && attrs.Any())
                {
                    var isVisibleAttr = attrs[0] as IsVisibleInDynamoLibraryAttribute;
                    if (null != isVisibleAttr && isVisibleAttr.Visible == false)
                    {
                        name = string.Empty;
                    }
                }
            }

            return name;
        }

        private List<Type> LoadAllTypesFromDynamoAssemblies()
        {
            List<Type> nodeTypes = new List<Type>();
            HashSet<string> loadedAssemblyNames = new HashSet<string>();
            var allLoadedAssembliesByPath = new Dictionary<string, Assembly>();
            var allLoadedAssemblies = new Dictionary<string, Assembly>();

            // cache the loaded assembly information
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.IsDynamic)
                    continue;

                try
                {
                    allLoadedAssembliesByPath[assembly.Location] = assembly;
                    allLoadedAssemblies[assembly.FullName] = assembly;
                }
                catch { }
            }

            // find all the dlls registered in all search paths
            // and concatenate with all dlls in the current directory
            List<string> allDynamoAssemblyPaths =
                DynamoPathManager.Instance.Nodes.SelectMany(path => Directory.GetFiles(path, "*.dll", SearchOption.TopDirectoryOnly)).ToList();

            // add the core assembly to get things like code block nodes and watches.
            allDynamoAssemblyPaths.Add(Path.Combine(DynamoPathManager.Instance.MainExecPath, "DynamoCore.dll"));

            var resolver = new ResolveEventHandler(delegate(object sender, ResolveEventArgs args)
            {
                Assembly result;
                allLoadedAssemblies.TryGetValue(args.Name, out result);
                return result;
            });

            foreach (var assemblyPath in allDynamoAssemblyPaths)
            {
                var fn = Path.GetFileName(assemblyPath);

                if (fn == null)
                    continue;

                // if the assembly has already been loaded, then
                // skip it, otherwise cache it.
                if (loadedAssemblyNames.Contains(fn))
                    continue;

                loadedAssemblyNames.Add(fn);

                if (allLoadedAssembliesByPath.ContainsKey(assemblyPath))
                {
                    List<Type> types = LoadNodesFromAssembly(allLoadedAssembliesByPath[assemblyPath]);
                    nodeTypes.AddRange(types);
                }
                else
                {
                    try
                    {
                        var assembly = Assembly.LoadFrom(assemblyPath);
                        allLoadedAssemblies[assembly.GetName().Name] = assembly;
                        List<Type> types = LoadNodesFromAssembly(assembly);
                        nodeTypes.AddRange(types);
                    }
                    catch (Exception e)
                    {
                        dynamoViewModel.Model.Logger.Log(e);
                    }
                }
            }

            return nodeTypes;
        }

        private List<Type> LoadNodesFromAssembly(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            var searchViewModel = dynamoViewModel.Model.SearchModel;

            List<Type> types = new List<Type>();

            try
            {
                var loadedTypes = assembly.GetTypes();

                foreach (var t in loadedTypes)
                {
                    try
                    {
                        //only load types that are in the right namespace, are not abstract
                        //and have the elementname attribute
                        var attribs = t.GetCustomAttributes(typeof(NodeNameAttribute), false);
                        var isDeprecated = t.GetCustomAttributes(typeof(NodeDeprecatedAttribute), true).Any();
                        var isMetaNode = t.GetCustomAttributes(typeof(IsMetaNodeAttribute), false).Any();
                        var isDSCompatible = t.GetCustomAttributes(typeof(IsDesignScriptCompatibleAttribute), true).Any();

                        bool isHidden = false;
                        var attrs = t.GetCustomAttributes(typeof(IsVisibleInDynamoLibraryAttribute), true);
                        if (null != attrs && attrs.Any())
                        {
                            var isVisibleAttr = attrs[0] as IsVisibleInDynamoLibraryAttribute;
                            if (null != isVisibleAttr && isVisibleAttr.Visible == false)
                            {
                                isHidden = true;
                            }
                        }

                        if (!dynamoViewModel.Model.Loader.IsNodeSubType(t) && t.Namespace != "Dynamo.Nodes") /*&& attribs.Length > 0*/
                            continue;

                        //if we are running in revit (or any context other than NONE) use the DoNotLoadOnPlatforms attribute, 
                        //if available, to discern whether we should load this type
                        if (!dynamoViewModel.Model.Context.Equals(Context.NONE))
                        {

                            object[] platformExclusionAttribs = t.GetCustomAttributes(typeof(DoNotLoadOnPlatformsAttribute), false);
                            if (platformExclusionAttribs.Length > 0)
                            {
                                string[] exclusions = (platformExclusionAttribs[0] as DoNotLoadOnPlatformsAttribute).Values;

                                //if the attribute's values contain the context stored on the controller
                                //then skip loading this type.

                                if (exclusions.Reverse().Any(e => e.Contains(dynamoViewModel.Model.Context)))
                                    continue;
                            }
                        }

                        if (attribs.Length > 0 && !isDeprecated && !isMetaNode && isDSCompatible && !isHidden)
                        {
                            searchViewModel.Add(t);
                        }
                        types.Add(t);
                    }
                    catch (Exception e)
                    {
                        dynamoViewModel.Model.Logger.Log(e);
                    }

                }
            }
            catch (Exception e)
            {
                dynamoViewModel.Model.Logger.Log("Could not load types.");
                dynamoViewModel.Model.Logger.Log(e);
            }

            return types;
        }

        private Dictionary<Guid, String> GetDictionaryOfConnectedNodes(List<NodeModel> list)
        {
            Dictionary<Guid, String> dictionary = new Dictionary<Guid, string>();
            foreach (NodeModel node in list)
            {
                List<ConnectorModel> nodeConnectors = node.AllConnectors.ToList();
                string connectorGuids = string.Empty;

                foreach (ConnectorModel connector in nodeConnectors)
                {
                    if (connector.Start.Owner.GUID != node.GUID)
                        connectorGuids += connector.Start.Owner.GUID.ToString() + "_";

                    else if (connector.End.Owner.GUID != node.GUID)
                        connectorGuids += connector.End.Owner.GUID.ToString() + "_";

                }
                dictionary.Add(node.GUID, connectorGuids);
            }
            return dictionary;
        }

        #endregion
    }
}