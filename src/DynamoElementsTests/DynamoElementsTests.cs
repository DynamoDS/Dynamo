using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Input;
using System.Diagnostics;

using Dynamo.Controls;
using Dynamo.Commands;
using Dynamo.Utilities;
using Dynamo.Nodes;

using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class DynamoElementsTests
    {
        [SetUp]
        public void Init()
        {
            StartDynamo();
        }

        private static void StartDynamo()
        {
            string tempPath = Path.GetTempPath();
            string logPath = Path.Combine(tempPath, "dynamoLog.txt");

            TextWriter tw = new StreamWriter(logPath);
            tw.WriteLine("Dynamo log started " + DateTime.Now.ToString());
            dynSettings.Writer = tw;

            //create a new instance of the controller
            DynamoController controller = new DynamoController();
            controller.Bench.Show();
        }

        [TearDown]
        public void Cleanup()
        {
            dynSettings.Controller.Bench.Close();
        }

        [Test]
        public void CanAddANote()
        {
            //create some test note data
            Dictionary<string, object> inputs = new Dictionary<string, object>();
            inputs.Add("x", 200.0);
            inputs.Add("y", 200.0);
            inputs.Add("text", "This is a test note.");
            inputs.Add("workspace", dynSettings.Controller.CurrentSpace);

            dynSettings.Workbench.UpdateLayout();

            if (DynamoCommands.AddNoteCmd.CanExecute(inputs))
            {
                DynamoCommands.AddNoteCmd.Execute(inputs);
            }

            Assert.AreEqual(dynSettings.Controller.CurrentSpace.Notes.Count, 1);
        }

        [Test]
        public void CanAddANodeByName()
        {
            if (DynamoCommands.CreateNodeCmd.CanExecute("+"))
            {
                DynamoCommands.CreateNodeCmd.Execute("+");
            }
            Assert.AreEqual(dynSettings.Controller.CurrentSpace.Nodes.Count, 1);
        }

        [Test]
        public void CanSumTwoNumbers()
        {
            
            dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.CreateNodeCmd, "+"));
            dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.CreateNodeCmd, "Number"));
            dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.CreateNodeCmd, "Number"));
            dynSettings.Controller.ProcessCommandQueue();

            //update the layout so the following
            //connectors have visuals to transform to
            //we were experiencing a problem with TransfromToAncestor
            //calls not being valid because entities weren't in the tree.
            dynSettings.Bench.Dispatcher.Invoke(
            new Action(delegate
            {
                dynSettings.Controller.Bench.UpdateLayout();
            }), DispatcherPriority.Render, null);

            ArrayList connectionData1 = new ArrayList();
            connectionData1.Add(dynSettings.Controller.Nodes[1].NodeUI);    //first number node
            connectionData1.Add(dynSettings.Controller.Nodes[0].NodeUI);    //+ node
            connectionData1.Add(0);  //first output
            connectionData1.Add(0);  //first input

            dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.CreateConnectionCmd, connectionData1));

            ArrayList connectionData2 = new ArrayList();
            connectionData2.Add(dynSettings.Controller.Nodes[2].NodeUI);    //first number node
            connectionData2.Add(dynSettings.Controller.Nodes[0].NodeUI);    //+ node
            connectionData2.Add(0);  //first output
            connectionData2.Add(1);  //second input

            dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.CreateConnectionCmd, connectionData2));
            dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.RunExpressionCmd, null));
            dynSettings.Controller.ProcessCommandQueue();

            dynSettings.Bench.Dispatcher.Invoke(
            new Action(delegate
            {
                dynSettings.Controller.Bench.UpdateLayout();
            }), DispatcherPriority.Render, null);
            
            Assert.AreEqual(dynSettings.Controller.Nodes.Count, 3);

            System.Threading.Thread.Sleep(5000);
        }
    }
}
