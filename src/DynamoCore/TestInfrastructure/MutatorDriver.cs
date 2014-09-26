using Dynamo.Models;
using Dynamo.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Data;

namespace Dynamo.TestInfrastructure
{
    /// <summary>
    /// Class to handle driving test mutations into the graph
    /// </summary>
    public class MutatorDriver
    {
        private DynamoViewModel dynamoViewModel;

        public MutatorDriver(DynamoViewModel dynamoViewModel)
        {
            this.dynamoViewModel = dynamoViewModel;
        }

        private static CompositeCollection collection = null;

        public CompositeCollection Collection
        {
            get
            {
                if (collection == null)
                    collection = GetCompositeCollection();

                return collection;
            }
        }

        internal CompositeCollection GetCompositeCollection()
        {
            var _oc = new CompositeCollection();

            var runAllTests = new RunAllTests(dynamoViewModel);
            var cc = new CollectionContainer();
            cc.Collection = new ObservableCollection<RunAllTests>() { runAllTests };
            _oc.Add(cc);

            var mutators = new CollectionContainer();
            mutators.Collection = ObtainMutationTests();
            _oc.Add(mutators);

            return _oc;
        }

        private ObservableCollection<AbstractMutator> ObtainMutationTests()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            var mutators = new ObservableCollection<AbstractMutator>();

            foreach (Type type in assembly.GetTypes())
            {
                var attribs = Attribute.GetCustomAttribute(type, typeof(MutationTestAttribute));
                var attribute = attribs as MutationTestAttribute;
                if (attribute != null)
                {
                    object[] args = new object[] { dynamoViewModel };
                    var classInstance = Activator.CreateInstance(type, args);
                    mutators.Add((AbstractMutator)classInstance);
                }
            }
            return mutators;
        }

        internal void RunMutationTests()
        {
            String logTarget = dynamoViewModel.Model.Logger.LogPath + "MutationLog.log";

            StreamWriter writer = new StreamWriter(logTarget);

            writer.WriteLine("MutateTest Internal activate");

            System.Diagnostics.Debug.WriteLine("MutateTest Internal activate");

            var mutators = new List<AbstractMutator>();
            foreach (CollectionContainer container in collection)
            {
                var objs = container.Collection.OfType<AbstractMutator>();
                if (objs != null)
                    mutators.AddRange(objs);
            }

            new Thread(() =>
            {
                try
                {
                    foreach (var mutator in mutators)
                    {
                        if (mutator.IsSelected)
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

            }).
                Start();
        }

        private object locker = new object();

        internal void RunOnSelectedFolder()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            String logTarget = dynamoViewModel.Model.Logger.LogPath + "MutationLog.log";
            StreamWriter writer = new StreamWriter(logTarget);

            string[] filePaths = Directory.GetFiles(dialog.SelectedPath, "*.xml");

            IEnumerable<AbstractMutator> mutators = null;
            foreach (CollectionContainer container in Collection)
            {
                mutators = container.Collection.OfType<AbstractMutator>();
            }

            foreach (string path in filePaths)
            {
                new Thread(() =>
                {
                    lock (locker)
                    {
                        dynamoViewModel.UIDispatcher.Invoke(new Action(() =>
                        {
                            dynamoViewModel.ExecuteCommand(new DynamoViewModel.OpenFileCommand(path));

                            foreach (AbstractMutator mutator in mutators)
                            {
                                if (mutator.IsSelected)
                                    InvokeTest(mutator, writer);
                            }
                        }));
                    }
                }).Start();
            }
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