using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NUnit.Framework;
using DynamoSandbox;
using System.Text.RegularExpressions;
using System.Xml;
using Dynamo;
using Dynamo.Applications;
using DynamoCLI;

namespace Dynamo.Tests
{
   
    internal class CommandLineTests : DynamoModelTestBase
    {

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
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

        protected static StartupUtils.CommandLineArguments CommandstringToArgs(string commandString)
        {
            var argarray = CommandStringToStringArray(commandString);
            var args = StartupUtils.CommandLineArguments.Parse(argarray);
            return args;
        }

        protected static string[] CommandStringToStringArray(string commandString)
        {
            //convert string to commandlineargs, 
            var m = Regex.Matches(commandString, "([^\"]\\S*|\".+?\")\\s*");
            var list = m.Cast<Match>().Select(match => match.Value).ToList();

            //strip trailing whitespace
            var argarray = list.Select(x => x.Trim()).ToArray();
            return argarray.ToArray();
        }

        [Test]
        public void CanOpenAndRunDynamoModelWithCommandLineRunner()
        {
            string openpath = Path.Combine(TestDirectory, @"core\math\Add.dyn");
            var runner = new CommandLineRunner(this.CurrentDynamoModel);
            string commandstring = "/o" + " " + openpath;

            runner.Run(CommandstringToArgs(commandstring));
            AssertPreviewValue("4c5889ac-7b91-4fb5-aaad-a2128b533279", 4.0);
        }

        [Test]
        public void CanOpenAndSetSpecifcStateInDynWithCommandLineRunner()
        {

            string openpath = Path.Combine(TestDirectory, @"core\commandline\addmultwithstatesindyn.dyn");
            var runner = new CommandLineRunner(this.CurrentDynamoModel);
            string commandstring = "/o" + " " + openpath + " " + "/s" + " " + "state2";

            runner.Run(CommandstringToArgs(commandstring));
            AssertPreviewValue("6e2d89d1-d5e8-4030-b065-f9d98b3adb45", 31);
        }

        [Test]
        public void CanOpenAndSetSpecifcStateFromImportedPresetsFileWithCommandLineRunner()
        {

            string openpath = Path.Combine(TestDirectory, @"core\commandline\addmultwithoutstates.dyn");
            string presetspath = Path.Combine(TestDirectory, @"core\commandline\statesOnly.xml");
            var runner = new CommandLineRunner(this.CurrentDynamoModel);
            string commandstring = "/o" + " " + openpath + " " + "/s" + " " + "state2" + " " + "/p" + " " + presetspath;

            runner.Run(CommandstringToArgs(commandstring));
            AssertPreviewValue("6e2d89d1-d5e8-4030-b065-f9d98b3adb45", 31);
        }

        [Test]
        public void CanOpenAndRunSpecifcStateAndOutputToFileWithCommandLineRunner()
        {
            string openpath = Path.Combine(TestDirectory, @"core\commandline\addmultwithstatesindyn.dyn");
            var newPath = GetNewFileNameOnTempPath("xml");

            var runner = new CommandLineRunner(this.CurrentDynamoModel);
            string commandstring = "/o" + " " + openpath + " " + "/s" + " " + "state2" + " " + "/v" + " " + newPath;
            runner.Run(CommandstringToArgs(commandstring));

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
            runner.Run(CommandstringToArgs(commandstring));

            var output = new XmlDocument();
            output.Load(newPath);
            AssertOutputValuesForGuid("6e2d89d1-d5e8-4030-b065-f9d98b3adb45", new List<Tuple<int, string>> { Tuple.Create(0, "10"), Tuple.Create(0, "31"), Tuple.Create(0, "20") }, output);

        }

        [Test]
        public void CanOpenAndRunAllImportedStatesFromDynamoCLIexe()
        {
            string openpath = Path.Combine(TestDirectory, @"core\commandline\addmultwithoutstates.dyn");
            string presetspath = Path.Combine(TestDirectory, @"core\commandline\statesOnly.xml");
            var newPath = GetNewFileNameOnTempPath("xml");
            string commandstring = "/o" + " " + openpath + " " + "/s" + " " + "all" + " " + "/p" + " " + presetspath + " " + "/v" + " " + newPath;

            DynamoCLI.Program.Main(CommandStringToStringArray(commandstring));
            var output = new XmlDocument();
            output.Load(newPath);
            AssertOutputValuesForGuid("6e2d89d1-d5e8-4030-b065-f9d98b3adb45", new List<Tuple<int, string>> { Tuple.Create(0, "10"), Tuple.Create(0, "31"), Tuple.Create(0, "20") }, output);

        }

        [Test]
        public void CanOpenAndRunSpecifcStateAndOutputToFileFromDynamoCLIexe()
        {
            string openpath = Path.Combine(TestDirectory, @"core\commandline\addmultwithstatesindyn.dyn");
            var newpath = GetNewFileNameOnTempPath("xml");
            string commandstring = "/o" + " " + openpath + " " + "/s" + " " + "state2" + " " + "/v" + " " + newpath;

            DynamoCLI.Program.Main(CommandStringToStringArray(commandstring));
            var output = new XmlDocument();
            output.Load(newpath);
            AssertOutputValuesForGuid("6e2d89d1-d5e8-4030-b065-f9d98b3adb45", new List<Tuple<int, string>> { Tuple.Create(0, "31") }, output);

        }

        [Test]
        public void CanOpenAndRunFileWihtListsCorrectlyToOutputFileFromDynamoCLIexe()
        {
            string openpath = Path.Combine(TestDirectory, @"core\commandline\simplelists.dyn");
            var newpath = GetNewFileNameOnTempPath("xml");
            string commandstring = "/o" + " " + openpath + " " + "/v" + " " + newpath;

            DynamoCLI.Program.Main(CommandStringToStringArray(commandstring));
            var output = new XmlDocument();
            output.Load(newpath);
            AssertOutputValuesForGuid("47b78c9b-98b3-4852-935f-0d03f52a65b3", new List<Tuple<int, string>> { Tuple.Create(0, "{1000,2,3,{1,2,3}}") }, output);
            AssertOutputValuesForGuid("8229dec7-b4ae-463b-a7ac-e36671fefef0", new List<Tuple<int, string>> { Tuple.Create(0, "{Surface,Surface,Surface,Surface,Surface,Surface}") }, output);

        }
    }
}
