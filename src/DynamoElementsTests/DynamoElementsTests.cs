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

            dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.AddNoteCmd, inputs));
            dynSettings.Controller.ProcessCommandQueue();

            Assert.AreEqual(dynSettings.Controller.CurrentSpace.Notes.Count, 1);
        }

        [Test]
        public void CanAddANodeByName()
        {
            Dictionary<string, object> sumData = new Dictionary<string, object>();
            sumData.Add("x", 400.0);
            sumData.Add("y", 100.0);
            sumData.Add("name", "+");
            dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.CreateNodeCmd, sumData));
            dynSettings.Controller.ProcessCommandQueue();

            Assert.AreEqual(dynSettings.Controller.CurrentSpace.Nodes.Count, 1);
        }

        [Test]
        public void CanSumTwoNumbers()
        {

            Dictionary<string, object> sumData = new Dictionary<string, object>();
            Dictionary<string, object> numData1 = new Dictionary<string, object>();
            Dictionary<string, object> numData2 = new Dictionary<string, object>();

            sumData.Add("x", 400.0);
            sumData.Add("y", 100.0);
            sumData.Add("name", "+");

            numData1.Add("x", 100.0);
            numData1.Add("y", 100.0);
            numData1.Add("name", "Number");

            numData2.Add("x", 100.0);
            numData2.Add("y", 300.0);
            numData2.Add("name", "Number");

            dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.CreateNodeCmd, sumData));
            dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.CreateNodeCmd, numData1));
            dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.CreateNodeCmd, numData2));
            dynSettings.Controller.ProcessCommandQueue();

            //update the layout so the following
            //connectors have visuals to transform to
            //we were experiencing a problem in tests with TransfromToAncestor
            //calls not being valid because entities weren't in the tree yet.
            dynSettings.Bench.Dispatcher.Invoke(
            new Action(delegate
            {
                dynSettings.Controller.Bench.UpdateLayout();
            }), DispatcherPriority.Render, null);


            dynDoubleInput num1 = dynSettings.Controller.Nodes[1] as dynDoubleInput;
            num1.Value = 2;
            dynDoubleInput num2 = dynSettings.Controller.Nodes[2] as dynDoubleInput;
            num2.Value = 2;

            Dictionary<string, object> cd1 = new Dictionary<string, object>();
            cd1.Add("start", dynSettings.Controller.Nodes[1].NodeUI);
            cd1.Add("end", dynSettings.Controller.Nodes[0].NodeUI);
            cd1.Add("port_start", 0);
            cd1.Add("port_end", 0);

            dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.CreateConnectionCmd, cd1));
            Dictionary<string,object> cd2 = new Dictionary<string,object>();
            cd2.Add("start", dynSettings.Controller.Nodes[2].NodeUI);    //first number node
            cd2.Add("end", dynSettings.Controller.Nodes[0].NodeUI);    //+ node
            cd2.Add("port_start",0);  //first output
            cd2.Add("port_end",1);  //second input

            dynSettings.Bench.LogText = "";

            dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.CreateConnectionCmd, cd2));
            dynSettings.Controller.CommandQueue.Add(Tuple.Create<object, object>(DynamoCommands.RunExpressionCmd, null));
            dynSettings.Controller.ProcessCommandQueue();

            //validate that the expression for addition is as expected
            Assert.AreEqual((dynSettings.Controller.Nodes[0] as dynNode).PrintExpression(), "(+ 2 2)");

            dynSettings.Bench.Dispatcher.Invoke(
            new Action(delegate
            {
                dynSettings.Controller.Bench.UpdateLayout();
            }), DispatcherPriority.Render, null);
            
            Assert.AreEqual(dynSettings.Controller.Nodes.Count, 3);
        }

        [Test]
        public void CanCreateACustomFunctionEnterItAndLeave()
        {
            throw new NotImplementedException();
        }
    }
}
