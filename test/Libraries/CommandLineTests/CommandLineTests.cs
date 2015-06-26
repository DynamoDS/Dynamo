using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NUnit.Framework;
using DynamoSandbox;
using System.Text.RegularExpressions;
using System.Xml;
using Dynamo;
using Dynamo.Applications.StartupUtils;
using DynamoCLI;

namespace Dynamo.Tests
{
   
    internal class CommandLineTests : DynamoModelTestBase
    {

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("VMDataBridge.dll");  // Required for Watch node.
            libraries.Add("ProtoGeometry.dll"); // Required for Surface.
            libraries.Add("DSCoreNodes.dll");   // Required for built-in nodes.
            libraries.Add("DSIronPython.dll");  // Required for Python tests.
            libraries.Add("FunctionObject.ds"); // Required for partially applied nodes.
            base.GetLibrariesToPreload(libraries);
        }

       /// <summary>
       /// asserts that specific output port has the correct string value foreach evaluation
       /// </summary>
       /// <param name="guid">guid of node to check values of</param>
       /// <param name="values">tuple matching output port index and string value of that port</param>
       /// <param name="doc">the xml doc containing the saved output from the command line /v command</param>

        protected static void AssertOutputValuesForGuid(string guid, List<Tuple<int, string>> values, XmlDocument doc)
        {
            var index = 0;
            foreach (XmlElement evaluation in doc.DocumentElement.ChildNodes)
            {

                foreach (XmlElement node in evaluation.ChildNodes)
                {
                    if (node.GetAttribute("guid") == guid)
                    {
                        Assert.AreEqual(values[index].Item2, ((node.ChildNodes[values[index].Item1]) as XmlElement).GetAttribute("value"));
                    }
                }
                index++;
            }
        }

        protected static CommandLineArguments commandstringToArgs(string commandstring)
        {
            var argarray = commandstringToStringArray(commandstring);
            var args = CommandLineArguments.FromArguments(argarray);
            return args;
        }

        protected static string[] commandstringToStringArray(string commandstring)
        {
            //convert string to commandlineargs, 
            var m = Regex.Matches(commandstring, "([^\"]\\S*|\".+?\")\\s*");
            var list = m.Cast<Match>().Select(match => match.Value).ToList();

            //strip trailing whitespace
            var argarray = list.Select(x => x.Trim()).ToArray();
            return argarray.ToArray();
        }

        [Test]
        public void CanOpenAndRunDynamoModelWithCommandLineRunner()
        {
            string openPath = Path.Combine(TestDirectory, @"core\math\Add.dyn");
            var runner = new CommandLineRunner(this.CurrentDynamoModel);
            string commandstring = "/o" + " " + openPath;

            runner.Run(commandstringToArgs(commandstring));
            AssertPreviewValue("4c5889ac-7b91-4fb5-aaad-a2128b533279", 4.0);
        }

        [Test]
        public void CanOpenAndSetSpecifcStateInDynWithCommandLineRunner()
        {

            string openPath = Path.Combine(TestDirectory, @"core\commandline\addmultwithstatesindyn.dyn");
            var runner = new CommandLineRunner(this.CurrentDynamoModel);
            string commandstring = "/o" + " " + openPath + " " + "/s" + " " + "state2";

            runner.Run(commandstringToArgs(commandstring));
            AssertPreviewValue("6e2d89d1-d5e8-4030-b065-f9d98b3adb45", 31);
        }

        [Test]
        public void CanOpenAndSetSpecifcStateFromImportedPresetsFileWithCommandLineRunner()
        {

            string openPath = Path.Combine(TestDirectory, @"core\commandline\addmultwithoutstates.dyn");
            string presetsPath = Path.Combine(TestDirectory, @"core\commandline\statesOnly.xml");
            var runner = new CommandLineRunner(this.CurrentDynamoModel);
            string commandstring = "/o" + " " + openPath + " " + "/s" + " " + "state2" + " " + "/p" + " " + presetsPath;

            runner.Run(commandstringToArgs(commandstring));
            AssertPreviewValue("6e2d89d1-d5e8-4030-b065-f9d98b3adb45", 31);
        }

        [Test]
        public void CanOpenAndRunSpecifcStateAndOutputToFileWithCommandLineRunner()
        {
            string openPath = Path.Combine(TestDirectory, @"core\commandline\addmultwithstatesindyn.dyn");
            var newPath = GetNewFileNameOnTempPath("xml");

            var runner = new CommandLineRunner(this.CurrentDynamoModel);
            string commandstring = "/o" + " " + openPath + " " + "/s" + " " + "state2" + " " + "/v" + " " + newPath;
            runner.Run(commandstringToArgs(commandstring));

            var output = new XmlDocument();
            output.Load(newPath);
            AssertOutputValuesForGuid("6e2d89d1-d5e8-4030-b065-f9d98b3adb45", new List<Tuple<int, string>> { Tuple.Create(0, "31")}, output);

        }

        [Test]
        public void CanOpenAndRunAllImportedStatesWithCommandLineRunner()
        {
            string openPath = Path.Combine(TestDirectory, @"core\commandline\addmultwithoutstates.dyn");
            string presetsPath = Path.Combine(TestDirectory, @"core\commandline\statesOnly.xml");
            var newPath = GetNewFileNameOnTempPath("xml");

            var runner = new CommandLineRunner(this.CurrentDynamoModel);
            string commandstring = "/o" + " " + openPath + " " + "/s" + " " + "all" + " " + "/p" + " " + presetsPath + " " + "/v" + " " + newPath;
            runner.Run(commandstringToArgs(commandstring));

            var output = new XmlDocument();
            output.Load(newPath);
            AssertOutputValuesForGuid("6e2d89d1-d5e8-4030-b065-f9d98b3adb45", new List<Tuple<int, string>> { Tuple.Create(0, "10"), Tuple.Create(0, "31"), Tuple.Create(0, "20") }, output);

        }

        [Test]
        public void CanOpenAndRunAllImportedStatesFromDynamoCLIexe()
        {
            string openPath = Path.Combine(TestDirectory, @"core\commandline\addmultwithoutstates.dyn");
            string presetsPath = Path.Combine(TestDirectory, @"core\commandline\statesOnly.xml");
            var newPath = GetNewFileNameOnTempPath("xml");
            string commandstring = "/o" + " " + openPath + " " + "/s" + " " + "all" + " " + "/p" + " " + presetsPath + " " + "/v" + " " + newPath;

            DynamoCLI.Program.Main(commandstringToStringArray(commandstring));
            var output = new XmlDocument();
            output.Load(newPath);
            AssertOutputValuesForGuid("6e2d89d1-d5e8-4030-b065-f9d98b3adb45", new List<Tuple<int, string>> { Tuple.Create(0, "10"), Tuple.Create(0, "31"), Tuple.Create(0, "20") }, output);

        }

        [Test]
        public void CanOpenAndRunSpecifcStateAndOutputToFileFromDynamoCLIexe()
        {
            string openPath = Path.Combine(TestDirectory, @"core\commandline\addmultwithstatesindyn.dyn");
            var newPath = GetNewFileNameOnTempPath("xml");
            string commandstring = "/o" + " " + openPath + " " + "/s" + " " + "state2" + " " + "/v" + " " + newPath;

            DynamoCLI.Program.Main(commandstringToStringArray(commandstring));
            var output = new XmlDocument();
            output.Load(newPath);
            AssertOutputValuesForGuid("6e2d89d1-d5e8-4030-b065-f9d98b3adb45", new List<Tuple<int, string>> { Tuple.Create(0, "31") }, output);

        }

    }
}
