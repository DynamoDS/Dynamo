using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Controls;

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
        [STAThread]
        [SetUp]
        public void Init()
        {
            string tempPath = Path.GetTempPath();
            string logPath = Path.Combine(tempPath, "dynamoLog.txt");

            TextWriter tw = new StreamWriter(logPath);
            tw.WriteLine("Dynamo log started " + DateTime.Now.ToString());
            dynSettings.Writer = tw;

            //create a new instance of the controller
            DynamoController controller = new DynamoController();
        }

        [STAThread]
        [TearDown]
        public void Cleanup()
        {
            //TODO: Anything to clean up here?
        }

        [STAThread]
        [Test]
        public void CanAddANote()
        {
            //create some test note data
            Dictionary<string, object> inputs = new Dictionary<string, object>();
            inputs.Add("x", 200.0);
            inputs.Add("y", 200.0);
            inputs.Add("text", "This is a test note.");
            inputs.Add("workspace", dynSettings.Controller.CurrentSpace);

            if (DynamoCommands.AddNoteCmd.CanExecute(inputs))
            {
                DynamoCommands.AddNoteCmd.Execute(inputs);
            }

            Assert.AreEqual(dynSettings.Controller.CurrentSpace.Notes.Count, 1);
        }


    }
}
