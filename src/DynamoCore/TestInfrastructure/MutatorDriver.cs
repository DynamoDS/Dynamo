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

namespace Dynamo.TestInfrastructure
{
    /// <summary>
    /// Class to handle driving test mutations into the graph
    /// </summary>
    public class MutatorDriver
    {
        private DynamoController dynamoController;

        public MutatorDriver(DynamoController dynamoController)
        {
            this.dynamoController = dynamoController;
        }

        internal void RunMutationTests()
        {
            Random rand = new Random(1);
            //DebugSettings.VerboseLogging = true;

            String logTarget = dynSettings.DynamoLogger.LogPath + "MutationLog.log";

            StreamWriter writer = new StreamWriter(logTarget);

            writer.WriteLine("MutateTest Internal activate");

            System.Diagnostics.Debug.WriteLine("MutateTest Internal activate");

            new Thread(() =>
                {
                    try
                    {
                        CodeBlockNodeTest(writer);
                        ConnectorTest(writer);
                        IntegerSliderTest(writer);
                        DoubleSliderTest(writer);
                        DirectoryPathTest(writer);
                        FilePathTest(writer);
                        NumberInputTest(writer);
                        StringInputTest(writer);
                        NumberSequenceTest(writer);
                        NumberRangeTest(writer);
                        DeleteNodeTest(writer);
                    }
                    finally
                    {
                        dynSettings.DynamoLogger.Log("Fuzz testing finished.");

                        writer.Flush();
                        writer.Close();
                        writer.Dispose();
                    }

                }).
                Start();
        }

        private void DeleteNodeTest(StreamWriter writer)
        {
            bool passed = false;
            try
            {
                Random rand = new Random(1);

                for (int i = 0; i < 1000; i++)
                {
                    writer.WriteLine("##### - Beginning run: " + i);

                    var nodes = dynamoController.DynamoModel.Nodes.ToList();
                    int nodesCountBeforeDelete = nodes.Count;

                    writer.WriteLine("### - Beginning eval");


                    dynamoController.UIDispatcher.Invoke(new Action(() =>
                    {
                        DynamoViewModel.RunCancelCommand runCancel =
                            new DynamoViewModel.RunCancelCommand(false, false);
                        dynamoController.DynamoViewModel.ExecuteCommand(runCancel);

                    }));

                    while (dynamoController.DynamoViewModel.Controller.Runner.Running)
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
                                    new DeleteNodeMutator(rand)
                                };


                    AbstractMutator mutator = mutators[rand.Next(mutators.Count)];

                    int numberOfUndosNeeded = mutator.Mutate();


                    Thread.Sleep(100);

                    writer.WriteLine("### - delete complete");
                    writer.Flush();
                    writer.WriteLine("### - Beginning re-exec");


                    dynamoController.UIDispatcher.Invoke(new Action(() =>
                    {
                        DynamoViewModel.RunCancelCommand runCancel =
                            new DynamoViewModel.RunCancelCommand(false, false);
                        dynamoController.DynamoViewModel.ExecuteCommand(runCancel);

                    }));

                    Thread.Sleep(100);

                    writer.WriteLine("### - re-exec complete");
                    writer.Flush();
                    writer.WriteLine("### - Beginning undo");

                    int nodesCountAfterDelete = dynamoController.DynamoModel.Nodes.Count;

                    if (nodesCountBeforeDelete > nodesCountAfterDelete)
                    {
                        for (int iUndo = 0; iUndo < numberOfUndosNeeded; iUndo++)
                        {
                            dynamoController.UIDispatcher.Invoke(new Action(() =>
                            {
                                DynamoViewModel.UndoRedoCommand undoCommand =
                                    new DynamoViewModel.UndoRedoCommand(
                                        DynamoViewModel.UndoRedoCommand.Operation.Undo);
                                dynamoController.DynamoViewModel.ExecuteCommand(undoCommand);

                            }));

                            Thread.Sleep(100);
                        }

                        writer.WriteLine("### - undo complete");
                        writer.Flush();
                        writer.WriteLine("### - Beginning re-exec");

                        dynamoController.UIDispatcher.Invoke(new Action(() =>
                        {
                            DynamoViewModel.RunCancelCommand runCancel =
                                new DynamoViewModel.RunCancelCommand(false, false);

                            dynamoController.DynamoViewModel.ExecuteCommand(runCancel);

                        }));
                        Thread.Sleep(10);

                        while (dynamoController.DynamoViewModel.Controller.Runner.Running)
                        {
                            Thread.Sleep(10);
                        }

                        writer.WriteLine("### - re-exec complete");
                        writer.Flush();

                        nodesCountAfterDelete = dynamoController.DynamoModel.Nodes.Count;
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
                dynSettings.DynamoLogger.Log("CodeBlockNodeTest : " + (passed ? "pass" : "FAIL"));
            }
        }

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

                    var nodes = dynamoController.DynamoModel.Nodes.Where(t => t.GetType() == typeof(CodeBlockNodeModel)).ToList();

                            writer.WriteLine("### - Beginning eval");


                            dynamoController.UIDispatcher.Invoke(new Action(() =>
                                {
                                    DynamoViewModel.RunCancelCommand runCancel =
                                        new DynamoViewModel.RunCancelCommand(false, false);
                                    dynamoController.DynamoViewModel.ExecuteCommand(runCancel);

                                }));

                            while (dynamoController.DynamoViewModel.Controller.Runner.Running)
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
                                    new CodeBlockNodeMutator(rand), new DeleteNodeMutator(rand)
                                };


                            AbstractMutator mutator = mutators[rand.Next(mutators.Count)];
                            
                            int numberOfUndosNeeded = mutator.Mutate();


                            Thread.Sleep(100);

                            writer.WriteLine("### - delete complete");
                            writer.Flush();
                            writer.WriteLine("### - Beginning re-exec");


                            dynamoController.UIDispatcher.Invoke(new Action(() =>
                                {
                                    DynamoViewModel.RunCancelCommand runCancel =
                                        new DynamoViewModel.RunCancelCommand(false, false);
                                    dynamoController.DynamoViewModel.ExecuteCommand(runCancel);

                                }));

                            Thread.Sleep(100);

                            writer.WriteLine("### - re-exec complete");
                            writer.Flush();
                            writer.WriteLine("### - Beginning undo");


                            for (int iUndo = 0; iUndo < numberOfUndosNeeded; iUndo++)
                            {
                                dynamoController.UIDispatcher.Invoke(new Action(() =>
                                    {
                                        DynamoViewModel.UndoRedoCommand undoCommand =
                                            new DynamoViewModel.UndoRedoCommand(
                                                DynamoViewModel.UndoRedoCommand.Operation.Undo);
                                        dynamoController.DynamoViewModel.ExecuteCommand(undoCommand);

                                    }));

                                Thread.Sleep(100);

                            }


                            writer.WriteLine("### - undo complete");
                            writer.Flush();
                            writer.WriteLine("### - Beginning re-exec");

                            dynamoController.UIDispatcher.Invoke(new Action(() =>
                                {
                                    DynamoViewModel.RunCancelCommand runCancel =
                                        new DynamoViewModel.RunCancelCommand(false, false);

                                    dynamoController.DynamoViewModel.ExecuteCommand(runCancel);

                                }));
                            Thread.Sleep(10);

                            while (dynamoController.DynamoViewModel.Controller.Runner.Running)
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

                            /*
                        UIDispatcher.Invoke(new Action(() =>
                        {
                            DynamoViewModel.ForceRunCancelCommand runCancelForce =
    new DynamoViewModel.ForceRunCancelCommand(false,
                                              false);
                        DynamoViewModel.ExecuteCommand(runCancelForce);
    
                            }));
                        while (DynamoViewModel.Controller.Runner.Running)
                        {
                            Thread.Sleep(10);
                        }
                        foreach (NodeModel n in nodes)
                        {
                            if (n.OutPorts.Count > 0)
                            {
                                String valmap = valueMap[n.GUID].ToString();
                                String nodeVal = n.GetValue(0).Data.ToString();

                                if (valmap != nodeVal)
                                {
                                    Debug.WriteLine("==========> Failure on run: " + i);
                                    Debug.WriteLine("Lookup map failed to agree");
                                    Validity.Assert(false);

                                }
                            }
                        }
                        */


                }

                passed = true;
            }
            finally
            {
                dynSettings.DynamoLogger.Log("CodeBlockNodeTest : " + (passed ? "pass" : "FAIL"));
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

                    List<NodeModel> nodes = dynamoController.DynamoModel.Nodes;
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

                            dynSettings.Controller.UIDispatcher.Invoke(new Action(() =>
                            {
                                DynamoViewModel.DeleteModelCommand delCommand =
                                    new DynamoViewModel.DeleteModelCommand(node.GUID);

                                dynamoController.DynamoViewModel.ExecuteCommand(delCommand);

                            }));

                            if (node.OutPorts[startIndex].IsConnected)
                                writer.WriteLine("### - Connector wasn't deleted");
                            else
                                writer.WriteLine("### - Connector was deleted");

                            dynamoController.UIDispatcher.Invoke(new Action(() =>
                            {
                                DynamoViewModel.UndoRedoCommand undoCommand =
                                    new DynamoViewModel.UndoRedoCommand(
                                        DynamoViewModel.UndoRedoCommand.Operation.Undo);
                                dynamoController.DynamoViewModel.ExecuteCommand(undoCommand);

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
                dynSettings.DynamoLogger.Log("ConnectorTest : " + (passed ? "pass" : "FAIL"));
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
                        List<NodeModel> nodes = dynamoController.DynamoModel.Nodes.Where(t => t.GetType() == type).ToList();

                        writer.WriteLine("### - Beginning eval");
                        dynamoController.UIDispatcher.Invoke(new Action(() =>
                        {
                            DynamoViewModel.RunCancelCommand runCancel =
                                new DynamoViewModel.RunCancelCommand(false, false);
                            dynamoController.DynamoViewModel.ExecuteCommand(runCancel);
                        }));
                        while (dynamoController.DynamoViewModel.Controller.Runner.Running)
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
                        new IntegerSliderMutator(rand)
                    };
                        AbstractMutator mutator = mutators[rand.Next(mutators.Count)];
                        int numberOfUndosNeeded = mutator.Mutate();
                        Thread.Sleep(100);

                        writer.WriteLine("### - Beginning undo");
                        for (int iUndo = 0; iUndo < numberOfUndosNeeded; iUndo++)
                        {
                            dynamoController.UIDispatcher.Invoke(new Action(() =>
                            {
                                DynamoViewModel.UndoRedoCommand undoCommand =
                                    new DynamoViewModel.UndoRedoCommand(
                                        DynamoViewModel.UndoRedoCommand.Operation.Undo);
                                dynamoController.DynamoViewModel.ExecuteCommand(undoCommand);
                            }));
                            Thread.Sleep(100);
                        }
                        writer.WriteLine("### - undo complete");
                        writer.Flush();
                        writer.WriteLine("### - Beginning re-exec");

                        dynamoController.UIDispatcher.Invoke(new Action(() =>
                        {
                            DynamoViewModel.RunCancelCommand runCancel =
                                new DynamoViewModel.RunCancelCommand(false, false);

                            dynamoController.DynamoViewModel.ExecuteCommand(runCancel);

                        }));
                        Thread.Sleep(10);

                        while (dynamoController.DynamoViewModel.Controller.Runner.Running)
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
                dynSettings.DynamoLogger.Log("IntegerSliderTest : " + (passed ? "pass" : "FAIL"));
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
                        List<NodeModel> nodes = dynamoController.DynamoModel.Nodes.Where(t => t.GetType() == type).ToList();

                        writer.WriteLine("### - Beginning eval");
                        dynamoController.UIDispatcher.Invoke(new Action(() =>
                        {
                            DynamoViewModel.RunCancelCommand runCancel =
                                new DynamoViewModel.RunCancelCommand(false, false);
                            dynamoController.DynamoViewModel.ExecuteCommand(runCancel);
                        }));
                        while (dynamoController.DynamoViewModel.Controller.Runner.Running)
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
                        new DoubleSliderMutator(rand)
                    };
                        AbstractMutator mutator = mutators[rand.Next(mutators.Count)];
                        int numberOfUndosNeeded = mutator.Mutate();
                        Thread.Sleep(100);

                        writer.WriteLine("### - Beginning undo");
                        for (int iUndo = 0; iUndo < numberOfUndosNeeded; iUndo++)
                        {
                            dynamoController.UIDispatcher.Invoke(new Action(() =>
                            {
                                DynamoViewModel.UndoRedoCommand undoCommand =
                                    new DynamoViewModel.UndoRedoCommand(
                                        DynamoViewModel.UndoRedoCommand.Operation.Undo);
                                dynamoController.DynamoViewModel.ExecuteCommand(undoCommand);
                            }));
                            Thread.Sleep(100);
                        }
                        writer.WriteLine("### - undo complete");
                        writer.Flush();

                        writer.WriteLine("### - undo complete");
                        writer.Flush();
                        writer.WriteLine("### - Beginning re-exec");

                        dynamoController.UIDispatcher.Invoke(new Action(() =>
                        {
                            DynamoViewModel.RunCancelCommand runCancel =
                                new DynamoViewModel.RunCancelCommand(false, false);

                            dynamoController.DynamoViewModel.ExecuteCommand(runCancel);

                        }));
                        Thread.Sleep(10);

                        while (dynamoController.DynamoViewModel.Controller.Runner.Running)
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
                dynSettings.DynamoLogger.Log("DoubleSliderTest : " + (passed ? "pass" : "FAIL"));
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
                        nodes = dynamoController.DynamoModel.Nodes.Where(t => t.GetType() == type).ToList();

                    if (nodes.Count == 0)
                        return;

                    writer.WriteLine("### - Beginning eval");
                    dynamoController.UIDispatcher.Invoke(new Action(() =>
                    {
                        DynamoViewModel.RunCancelCommand runCancel =
                            new DynamoViewModel.RunCancelCommand(false, false);
                        dynamoController.DynamoViewModel.ExecuteCommand(runCancel);
                    }));
                    while (dynamoController.DynamoViewModel.Controller.Runner.Running)
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
                        new DirectoryPathMutator(rand)
                    };
                    AbstractMutator mutator = mutators[rand.Next(mutators.Count)];
                    int numberOfUndosNeeded = mutator.Mutate();
                    Thread.Sleep(100);

                    writer.WriteLine("### - Beginning undo");
                    for (int iUndo = 0; iUndo < numberOfUndosNeeded; iUndo++)
                    {
                        dynamoController.UIDispatcher.Invoke(new Action(() =>
                        {
                            DynamoViewModel.UndoRedoCommand undoCommand =
                                new DynamoViewModel.UndoRedoCommand(
                                    DynamoViewModel.UndoRedoCommand.Operation.Undo);
                            dynamoController.DynamoViewModel.ExecuteCommand(undoCommand);
                        }));
                        Thread.Sleep(100);
                    }
                    writer.WriteLine("### - undo complete");
                    writer.Flush();
                    writer.WriteLine("### - Beginning re-exec");

                    dynamoController.UIDispatcher.Invoke(new Action(() =>
                    {
                        DynamoViewModel.RunCancelCommand runCancel =
                            new DynamoViewModel.RunCancelCommand(false, false);

                        dynamoController.DynamoViewModel.ExecuteCommand(runCancel);

                    }));
                    Thread.Sleep(10);

                    while (dynamoController.DynamoViewModel.Controller.Runner.Running)
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
                dynSettings.DynamoLogger.Log("DirectoryPathTest : " + (passed ? "pass" : "FAIL"));
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
                        nodes = dynamoController.DynamoModel.Nodes.Where(t => t.GetType() == type).ToList();

                    if (nodes.Count == 0)
                        return;

                    writer.WriteLine("### - Beginning eval");
                    dynamoController.UIDispatcher.Invoke(new Action(() =>
                    {
                        DynamoViewModel.RunCancelCommand runCancel =
                            new DynamoViewModel.RunCancelCommand(false, false);
                        dynamoController.DynamoViewModel.ExecuteCommand(runCancel);
                    }));
                    while (dynamoController.DynamoViewModel.Controller.Runner.Running)
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
                        new FilePathMutator(rand)
                    };
                    AbstractMutator mutator = mutators[rand.Next(mutators.Count)];
                    int numberOfUndosNeeded = mutator.Mutate();
                    Thread.Sleep(100);

                    writer.WriteLine("### - Beginning undo");
                    for (int iUndo = 0; iUndo < numberOfUndosNeeded; iUndo++)
                    {
                        dynamoController.UIDispatcher.Invoke(new Action(() =>
                        {
                            DynamoViewModel.UndoRedoCommand undoCommand =
                                new DynamoViewModel.UndoRedoCommand(
                                    DynamoViewModel.UndoRedoCommand.Operation.Undo);
                            dynamoController.DynamoViewModel.ExecuteCommand(undoCommand);
                        }));
                        Thread.Sleep(100);
                    }                    
                    writer.WriteLine("### - undo complete");
                    writer.Flush();
                    writer.WriteLine("### - Beginning re-exec");

                    dynamoController.UIDispatcher.Invoke(new Action(() =>
                    {
                        DynamoViewModel.RunCancelCommand runCancel =
                            new DynamoViewModel.RunCancelCommand(false, false);

                        dynamoController.DynamoViewModel.ExecuteCommand(runCancel);

                    }));
                    Thread.Sleep(10);

                    while (dynamoController.DynamoViewModel.Controller.Runner.Running)
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
                dynSettings.DynamoLogger.Log("FilePathTest : " + (passed ? "pass" : "FAIL"));
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
                    List<NodeModel> nodes = dynamoController.DynamoModel.Nodes.Where(t => t.GetType() == typeof(DoubleInput)).ToList();

                    if (nodes.Count == 0)
                        return;

                    writer.WriteLine("### - Beginning eval");
                    dynamoController.UIDispatcher.Invoke(new Action(() =>
                    {
                        DynamoViewModel.RunCancelCommand runCancel =
                            new DynamoViewModel.RunCancelCommand(false, false);
                        dynamoController.DynamoViewModel.ExecuteCommand(runCancel);
                    }));
                    while (dynamoController.DynamoViewModel.Controller.Runner.Running)
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
                        new NumberInputMutator(rand)
                    };
                    AbstractMutator mutator = mutators[rand.Next(mutators.Count)];
                    int numberOfUndosNeeded = mutator.Mutate();
                    Thread.Sleep(100);

                    writer.WriteLine("### - Beginning undo");
                    for (int iUndo = 0; iUndo < numberOfUndosNeeded; iUndo++)
                    {
                        dynamoController.UIDispatcher.Invoke(new Action(() =>
                        {
                            DynamoViewModel.UndoRedoCommand undoCommand =
                                new DynamoViewModel.UndoRedoCommand(
                                    DynamoViewModel.UndoRedoCommand.Operation.Undo);
                            dynamoController.DynamoViewModel.ExecuteCommand(undoCommand);
                        }));
                        Thread.Sleep(100);
                    }
                    writer.WriteLine("### - undo complete");
                    writer.Flush();
                    writer.WriteLine("### - Beginning re-exec");

                    dynamoController.UIDispatcher.Invoke(new Action(() =>
                    {
                        DynamoViewModel.RunCancelCommand runCancel =
                            new DynamoViewModel.RunCancelCommand(false, false);

                        dynamoController.DynamoViewModel.ExecuteCommand(runCancel);

                    }));
                    Thread.Sleep(10);

                    while (dynamoController.DynamoViewModel.Controller.Runner.Running)
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
                dynSettings.DynamoLogger.Log("NumberTest : " + (passed ? "pass" : "FAIL"));
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
                    List<NodeModel> nodes = dynamoController.DynamoModel.Nodes.Where(t => t.GetType() == typeof(StringInput)).ToList();

                    if (nodes.Count == 0)
                        return;

                    writer.WriteLine("### - Beginning eval");
                    dynamoController.UIDispatcher.Invoke(new Action(() =>
                    {
                        DynamoViewModel.RunCancelCommand runCancel =
                            new DynamoViewModel.RunCancelCommand(false, false);
                        dynamoController.DynamoViewModel.ExecuteCommand(runCancel);
                    }));
                    while (dynamoController.DynamoViewModel.Controller.Runner.Running)
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
                        new StringInputMutator(rand)
                    };
                    AbstractMutator mutator = mutators[rand.Next(mutators.Count)];
                    int numberOfUndosNeeded = mutator.Mutate();
                    Thread.Sleep(100);

                    writer.WriteLine("### - Beginning undo");
                    for (int iUndo = 0; iUndo < numberOfUndosNeeded; iUndo++)
                    {
                        dynamoController.UIDispatcher.Invoke(new Action(() =>
                        {
                            DynamoViewModel.UndoRedoCommand undoCommand =
                                new DynamoViewModel.UndoRedoCommand(
                                    DynamoViewModel.UndoRedoCommand.Operation.Undo);
                            dynamoController.DynamoViewModel.ExecuteCommand(undoCommand);
                        }));
                        Thread.Sleep(100);
                    }
                    writer.WriteLine("### - undo complete");
                    writer.Flush();

                    writer.WriteLine("### - undo complete");
                    writer.Flush();
                    writer.WriteLine("### - Beginning re-exec");

                    dynamoController.UIDispatcher.Invoke(new Action(() =>
                    {
                        DynamoViewModel.RunCancelCommand runCancel =
                            new DynamoViewModel.RunCancelCommand(false, false);

                        dynamoController.DynamoViewModel.ExecuteCommand(runCancel);

                    }));
                    Thread.Sleep(10);

                    while (dynamoController.DynamoViewModel.Controller.Runner.Running)
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
                dynSettings.DynamoLogger.Log("StringTest : " + (passed ? "pass" : "FAIL"));
            }
        }

        #endregion

        #region Sequence/Range Tests

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
                    nodes = dynamoController.DynamoModel.Nodes.Where(t => t.GetType() == type).ToList();

                if (nodes.Count == 0)
                    return;

                foreach (NodeModel n in nodes)
                {
                    try
                    {
                        Random rand = new Random(1);

                        Guid guidNumber1 = Guid.Parse("fa532273-cf1d-4f41-874e-6146f634e2d3"); //Guid of node "Number" that connect to node "Number Sequence" on Start
                        Guid guidNumber2 = Guid.Parse("788dfa62-dbb2-4556-ad13-ce20ccc5ec0d"); //Guid of node "Number" that connect to node "Number Sequence" on Amount
                        Guid guidNumber3 = Guid.Parse("7bfb0b00-3dbc-4ab4-ba6b-f7743b72bbc5"); //Guid of node "Number" that connect to node "Number Sequence" on Step

                        dynSettings.Controller.UIDispatcher.Invoke(new Action(() =>
                        {
                            //coordinates for additional nodes
                            double coordinatesX = 120;
                            double coordinatesY = 180;

                            //make nodes
                            DynamoViewModel.CreateNodeCommand createNodeNumber1 =
                                new DynamoViewModel.CreateNodeCommand(guidNumber1, "Number", coordinatesX, coordinatesY, false, true);
                            DynamoViewModel.CreateNodeCommand createNodeNumber2 =
                                new DynamoViewModel.CreateNodeCommand(guidNumber2, "Number", coordinatesX, coordinatesY + 100, false, true);
                            DynamoViewModel.CreateNodeCommand createNodeNumber3 =
                                new DynamoViewModel.CreateNodeCommand(guidNumber3, "Number", coordinatesX, coordinatesY + 200, false, true);

                            //execute commands
                            dynamoController.DynamoViewModel.ExecuteCommand(createNodeNumber1); //create node "Number" that connect to node "Number Sequence" on Start
                            dynamoController.DynamoViewModel.ExecuteCommand(createNodeNumber2); //create node "Number" that connect to node "Number Sequence" on Amount
                            dynamoController.DynamoViewModel.ExecuteCommand(createNodeNumber3); //create node "Number" that connect to node "Number Sequence" on Step                    
                        }));

                        for (int i = 0; i < 1000; i++)
                        {
                            writer.WriteLine("##### - Beginning run: " + i);

                            writer.WriteLine("### - Beginning eval");
                            dynamoController.UIDispatcher.Invoke(new Action(() =>
                            {
                                DynamoViewModel.RunCancelCommand runCancel =
                                    new DynamoViewModel.RunCancelCommand(false, false);
                                dynamoController.DynamoViewModel.ExecuteCommand(runCancel);
                            }));
                            while (dynamoController.DynamoViewModel.Controller.Runner.Running)
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
                                    Object data = connector.Start.Owner.GetValue(0).Data;
                                    String val = data != null ? data.ToString() : "null";
                                    valueMap.Add(guid, val);
                                    writer.WriteLine(guid + " :: " + val);
                                    writer.Flush();
                                }
                            }

                            List<AbstractMutator> mutators = new List<AbstractMutator>()
                            {
                                new NumberSequenceMutator(rand)
                            };
                            AbstractMutator mutator = mutators[rand.Next(mutators.Count)];
                            int numberOfUndosNeeded = mutator.Mutate();
                            Thread.Sleep(100);

                            writer.WriteLine("### - Beginning undo");
                            for (int iUndo = 0; iUndo < numberOfUndosNeeded; iUndo++)
                            {
                                List<NodeModel> numberSeqNodes = new List<NodeModel>();
                                numberSeqNodes = dynamoController.DynamoModel.Nodes.Where(t => t.GetType() == type).ToList();

                                dynamoController.UIDispatcher.Invoke(new Action(() =>
                                {
                                    for (int j = 0; j < 3 * numberSeqNodes.Count; j++)
                                    {
                                        DynamoViewModel.UndoRedoCommand undoCommand =
                                            new DynamoViewModel.UndoRedoCommand(DynamoViewModel.UndoRedoCommand.Operation.Undo);
                                        dynamoController.DynamoViewModel.ExecuteCommand(undoCommand);
                                    }
                                }));

                                Thread.Sleep(100);
                            }
                            writer.WriteLine("### - undo complete");
                            writer.Flush();

                            dynamoController.UIDispatcher.Invoke(new Action(() =>
                            {
                                DynamoViewModel.RunCancelCommand runCancel =
                                    new DynamoViewModel.RunCancelCommand(false, false);
                                dynamoController.DynamoViewModel.ExecuteCommand(runCancel);
                            }));
                            while (dynamoController.DynamoViewModel.Controller.Runner.Running)
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
                        dynamoController.UIDispatcher.Invoke(new Action(() =>
                        {
                            DynamoViewModel.DeleteModelCommand delNumberCommand1 =
                                new DynamoViewModel.DeleteModelCommand(Guid.Parse("fa532273-cf1d-4f41-874e-6146f634e2d3"));
                            DynamoViewModel.DeleteModelCommand delNumberCommand2 =
                                new DynamoViewModel.DeleteModelCommand(Guid.Parse("788dfa62-dbb2-4556-ad13-ce20ccc5ec0d"));
                            DynamoViewModel.DeleteModelCommand delNumberCommand3 =
                                new DynamoViewModel.DeleteModelCommand(Guid.Parse("7bfb0b00-3dbc-4ab4-ba6b-f7743b72bbc5"));

                            dynamoController.DynamoViewModel.ExecuteCommand(delNumberCommand1);
                            dynamoController.DynamoViewModel.ExecuteCommand(delNumberCommand2);
                            dynamoController.DynamoViewModel.ExecuteCommand(delNumberCommand3);
                        }));

                    }
                }
            }
            finally
            {
                dynSettings.DynamoLogger.Log("NumberSequenceTest : " + (passed ? "pass" : "FAIL"));
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
                    nodes = dynamoController.DynamoModel.Nodes.Where(t => t.GetType() == type).ToList();

                if (nodes.Count == 0)
                    return;

                foreach (NodeModel n in nodes)
                {
                    try
                    {
                        Random rand = new Random(1);

                        Guid guidNumber1 = Guid.Parse("fa532273-cf1d-4f41-874e-6146f634e2d3"); //Guid of node "Number" that connect to node "Number Range" on Start
                        Guid guidNumber2 = Guid.Parse("788dfa62-dbb2-4556-ad13-ce20ccc5ec0d"); //Guid of node "Number" that connect to node "Number Range" on Amount
                        Guid guidNumber3 = Guid.Parse("7bfb0b00-3dbc-4ab4-ba6b-f7743b72bbc5"); //Guid of node "Number" that connect to node "Number Range" on Step

                        dynSettings.Controller.UIDispatcher.Invoke(new Action(() =>
                        {
                            //coordinates for additional nodes
                            double coordinatesX = 120;
                            double coordinatesY = 180;

                            //make nodes
                            DynamoViewModel.CreateNodeCommand createNodeNumber1 =
                                new DynamoViewModel.CreateNodeCommand(guidNumber1, "Number", coordinatesX, coordinatesY, false, true);
                            DynamoViewModel.CreateNodeCommand createNodeNumber2 =
                                new DynamoViewModel.CreateNodeCommand(guidNumber2, "Number", coordinatesX, coordinatesY + 100, false, true);
                            DynamoViewModel.CreateNodeCommand createNodeNumber3 =
                                new DynamoViewModel.CreateNodeCommand(guidNumber3, "Number", coordinatesX, coordinatesY + 200, false, true);

                            //create commands
                            dynamoController.DynamoViewModel.ExecuteCommand(createNodeNumber1); //create node "Number" that connect to node "Number Range" on Start
                            dynamoController.DynamoViewModel.ExecuteCommand(createNodeNumber2); //create node "Number" that connect to node "Number Range" on End
                            dynamoController.DynamoViewModel.ExecuteCommand(createNodeNumber3); //create node "Number" that connect to node "Number Range" on Step                    
                        }));

                        for (int i = 0; i < 1000; i++)
                        {
                            writer.WriteLine("##### - Beginning run: " + i);

                            writer.WriteLine("### - Beginning eval");
                            dynamoController.UIDispatcher.Invoke(new Action(() =>
                            {
                                DynamoViewModel.RunCancelCommand runCancel =
                                    new DynamoViewModel.RunCancelCommand(false, false);
                                dynamoController.DynamoViewModel.ExecuteCommand(runCancel);
                            }));
                            while (dynamoController.DynamoViewModel.Controller.Runner.Running)
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
                                    Object data = connector.Start.Owner.GetValue(0).Data;
                                    String val = data != null ? data.ToString() : "null";
                                    valueMap.Add(guid, val);
                                    writer.WriteLine(guid + " :: " + val);
                                    writer.Flush();
                                }
                            }

                            List<AbstractMutator> mutators = new List<AbstractMutator>()
                            {
                                new NumberRangeMutator(rand)
                            };
                            AbstractMutator mutator = mutators[rand.Next(mutators.Count)];
                            int numberOfUndosNeeded = mutator.Mutate();
                            Thread.Sleep(100);

                            writer.WriteLine("### - Beginning undo");
                            for (int iUndo = 0; iUndo < numberOfUndosNeeded; iUndo++)
                            {
                                List<NodeModel> numberRangeNodes = new List<NodeModel>();
                                numberRangeNodes = dynamoController.DynamoModel.Nodes.Where(t => t.GetType() == type).ToList();

                                dynamoController.UIDispatcher.Invoke(new Action(() =>
                                {
                                    for (int j = 0; j < 3 * numberRangeNodes.Count; j++)
                                    {
                                        DynamoViewModel.UndoRedoCommand undoCommand =
                                            new DynamoViewModel.UndoRedoCommand(DynamoViewModel.UndoRedoCommand.Operation.Undo);
                                        dynamoController.DynamoViewModel.ExecuteCommand(undoCommand);
                                    }
                                }));

                                Thread.Sleep(100);
                            }
                            writer.WriteLine("### - undo complete");
                            writer.Flush();

                            dynamoController.UIDispatcher.Invoke(new Action(() =>
                            {
                                DynamoViewModel.RunCancelCommand runCancel =
                                    new DynamoViewModel.RunCancelCommand(false, false);
                                dynamoController.DynamoViewModel.ExecuteCommand(runCancel);
                            }));
                            while (dynamoController.DynamoViewModel.Controller.Runner.Running)
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
                        dynamoController.UIDispatcher.Invoke(new Action(() =>
                        {
                            DynamoViewModel.DeleteModelCommand delNumberCommand1 =
                                new DynamoViewModel.DeleteModelCommand(Guid.Parse("fa532273-cf1d-4f41-874e-6146f634e2d3"));
                            DynamoViewModel.DeleteModelCommand delNumberCommand2 =
                                new DynamoViewModel.DeleteModelCommand(Guid.Parse("788dfa62-dbb2-4556-ad13-ce20ccc5ec0d"));
                            DynamoViewModel.DeleteModelCommand delNumberCommand3 =
                                new DynamoViewModel.DeleteModelCommand(Guid.Parse("7bfb0b00-3dbc-4ab4-ba6b-f7743b72bbc5"));

                            dynamoController.DynamoViewModel.ExecuteCommand(delNumberCommand1);
                            dynamoController.DynamoViewModel.ExecuteCommand(delNumberCommand2);
                            dynamoController.DynamoViewModel.ExecuteCommand(delNumberCommand3);
                        }));

                        dynSettings.DynamoLogger.Log("NumberRangeTest : " + (passed ? "pass" : "FAIL"));
                    }
                }
            }

            finally
            {
                dynSettings.DynamoLogger.Log("NumberRangeTest : " + (passed ? "pass" : "FAIL"));
            }
        }

        #endregion
    }
}