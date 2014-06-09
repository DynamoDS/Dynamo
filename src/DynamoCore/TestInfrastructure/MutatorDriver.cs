using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using DSNodeServices;
using Dynamo.Models;
using Dynamo.Utilities;
using Dynamo.ViewModels;

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
                    bool passed = false;

                    try
                    {

                        for (int i = 0; i < 1000; i++)
                        {
                            writer.WriteLine("##### - Beginning run: " + i);

                            var nodes = dynamoController.DynamoModel.Nodes;

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
                        dynSettings.DynamoLogger.Log("Fuzz testing: " + (passed ? "pass" : "FAIL"));

                        writer.Flush();
                        writer.Close();
                        writer.Dispose();
                    }


                }).
                Start();

        }
    }
}