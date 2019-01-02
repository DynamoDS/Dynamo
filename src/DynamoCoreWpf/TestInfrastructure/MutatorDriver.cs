using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using Dynamo.ViewModels;

namespace Dynamo.TestInfrastructure
{
    /// <summary>
    /// Class to handle driving test mutations into the graph
    /// </summary>
    public class MutatorDriver
    {
        /// <summary>
        /// Number of mutations to run
        /// </summary>
        private const int NUMBER_OF_CYCLES = 1000;

        private readonly DynamoViewModel dynamoViewModel;

        public MutatorDriver(DynamoViewModel dynamoViewModel)
        {
            this.dynamoViewModel = dynamoViewModel;
        }

        internal void RunMutationTests()
        {
            String logTarget = dynamoViewModel.Model.Logger.LogPath + "MutationLog.log";

            StreamWriter writer = new StreamWriter(logTarget);

            writer.WriteLine("MutateTest Internal activate");

            System.Diagnostics.Debug.WriteLine("MutateTest Internal activate");

            new Thread(() =>
            {
                Random rand = new Random(1);

                try
                {
                    Assembly assembly = Assembly.GetExecutingAssembly();

                    var mutators = new List<AbstractMutator>();

                    foreach (Type type in assembly.GetTypes())
                    {
                        var attribute = Attribute.GetCustomAttribute(type, typeof(MutationTestAttribute));
                        if (attribute != null)
                        {
                            object[] args = new object[] { dynamoViewModel };
                            var classInstance = Activator.CreateInstance(type, args);
                            mutators.Add((AbstractMutator)classInstance);
                        }
                    }

                    System.Diagnostics.Debug.WriteLine("Mutators list: " + mutators.ToList());

                    for (int cycleCounter = 0; cycleCounter < NUMBER_OF_CYCLES; cycleCounter++)
                    {
                        int mutatorIndex = rand.Next(0, mutators.Count);
                        var mutator = mutators[mutatorIndex];
                        InvokeTest(mutator, writer);
                    }
                }
                finally
                {
                    dynamoViewModel.Model.Logger.Log("Fuzz testing finished.");

                    writer.Flush();
                    writer.Close();
                    writer.Dispose();
                }

            }).Start();
        }

        private void InvokeTest(AbstractMutator mutator, StreamWriter writer)
        {
            bool passed = false;

            Type type = mutator.GetNodeType();
            if (type == null)
                return;

            var att = (MutationTestAttribute)Attribute.GetCustomAttribute(mutator.GetType(), 
                typeof(MutationTestAttribute));

            var nodes = dynamoViewModel.Model.CurrentWorkspace.Nodes.ToList();
            if (type != typeof(NodeModel))
                nodes = nodes.Where(t => t.GetType() == type).ToList();

            if (nodes.Count == 0)
            {
                writer.WriteLine("No nodes for mutator: " + mutator.GetType());
                return;

            }

            try
            {
                Random rand = new Random(1);

                for (int i = 0; i < mutator.NumberOfLaunches; i++)
                {
                    NodeModel node = nodes[rand.Next(nodes.Count)];

                    writer.WriteLine("##### - Beginning run " + att.Caption + ": " + i);
                    writer.WriteLine("### - Beginning eval");

                    dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                    {
                        DynamoModel.RunCancelCommand runCancel =
                            new DynamoModel.RunCancelCommand(false, false);

                        dynamoViewModel.ExecuteCommand(runCancel);
                    }));
                    while (!dynamoViewModel.HomeSpaceViewModel.RunSettingsViewModel.RunButtonEnabled)
                    {
                        Thread.Sleep(10);
                    }

                    writer.WriteLine("### - Eval complete");
                    writer.Flush();

                    var engineController =
                        dynamoViewModel.EngineController;

                    passed = mutator.RunTest(node, engineController, writer);

                    if (!passed)
                    {
                        writer.WriteLine("### - Test failed");
                        break;
                    }
                }
            }
            catch(Exception ex)
            {
                dynamoViewModel.Model.Logger.Log(att.Caption + ": FAILED: " + ex.Message);
            }
            finally
            {
                dynamoViewModel.Model.Logger.Log(att.Caption + ": " + (passed ? "pass" : "FAIL"));
            }
        }
    }
}