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

                    string assemblyPass = Environment.CurrentDirectory + "\\nodes\\DSCoreNodesUI.dll";
                    Assembly assembly = Assembly.LoadFile(assemblyPass);
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
                                    else
                                    {
                                        writer.WriteLine("### - test of IntegerSlider is passed");
                                        writer.Flush();
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

                    string assemblyPass = Environment.CurrentDirectory + "\\nodes\\DSCoreNodesUI.dll";
                    Assembly assembly = Assembly.LoadFile(assemblyPass);
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
                                    else
                                    {
                                        writer.WriteLine("### - test of DoubleSlider is passed");
                                        writer.Flush();
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
                    string assemblyPass = Environment.CurrentDirectory + "\\nodes\\DSCoreNodesUI.dll";
                    Assembly assembly = Assembly.LoadFile(assemblyPass);
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
                                else
                                {
                                    writer.WriteLine("### - test of DirectoryPath is passed");
                                    writer.Flush();
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
                    string assemblyPass = Environment.CurrentDirectory + "\\nodes\\DSCoreNodesUI.dll";
                    Assembly assembly = Assembly.LoadFile(assemblyPass);
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
                                else
                                {
                                    writer.WriteLine("### - test of FilePath is passed");
                                    writer.Flush();
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
                                else
                                {
                                    writer.WriteLine("### - test of Number is passed");
                                    writer.Flush();
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
                                else
                                {
                                    writer.WriteLine("### - test of String is passed");
                                    writer.Flush();
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
    }
}