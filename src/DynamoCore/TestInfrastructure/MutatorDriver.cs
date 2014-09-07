using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Collections.Generic;
using DSNodeServices;
using Dynamo.Core;
using Dynamo.Nodes;
using Dynamo.Models;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using DynamoUtilities;
using Autodesk.DesignScript.Runtime;

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
            String logTarget = dynamoViewModel.Model.Logger.LogPath + "MutationLog.log";

            StreamWriter writer = new StreamWriter(logTarget);

            writer.WriteLine("MutateTest Internal activate");

            System.Diagnostics.Debug.WriteLine("MutateTest Internal activate");

            new Thread(() =>
            {
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

                    foreach (var mutator in mutators)
                        InvokeTest(mutator, writer);
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

            var nodes = dynamoViewModel.Model.Nodes;
            if (type != typeof(NodeModel))
                nodes = dynamoViewModel.Model.Nodes.Where(t => t.GetType() == type).ToList();

            if (nodes.Count == 0)
                return;

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

                    passed = mutator.RunTest(node, writer);

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