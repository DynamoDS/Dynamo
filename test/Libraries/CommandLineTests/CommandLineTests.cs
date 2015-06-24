using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NUnit.Framework;
using DynamoSandbox;
using System.Text.RegularExpressions;
using System.Xml;
using Dynamo;

namespace Dynamo.Tests
{
    [TestFixture]
    class CommandLineTests : DynamoModelTestBase
    {
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
            //convert string to commandlineargs, 
            var m = Regex.Matches(commandstring, "([^\"]\\S*|\".+?\")\\s*");
            var list = m.Cast<Match>().Select(match => match.Value).ToList();

            //strip trailing whitespace
            var argarray = list.Select(x => x.Trim()).ToArray();
            var args = CommandLineArguments.FromArguments(argarray);
            return args;
        }

        [Test]
        public void CanOpenAndRunDynamoModelFromCLI()
        {
            string openPath = Path.Combine(TestDirectory, @"core\math\Add.dyn");
            var runner = new CommandLineRunner(this.CurrentDynamoModel);
            string commandstring = "/o" + " " + openPath;

            runner.Run(commandstringToArgs(commandstring));
            AssertPreviewValue("4c5889ac-7b91-4fb5-aaad-a2128b533279", 4.0);
        }

        [Test]
        public void CanOpenAndSetSpecifcStateInDynFromCLI()
        {

            string openPath = Path.Combine(TestDirectory, @"core\commandline\addmultwithstatesindyn.dyn");
            var runner = new CommandLineRunner(this.CurrentDynamoModel);
            string commandstring = "/o" + " " + openPath + " " + "/s" + " " + "state2";

            runner.Run(commandstringToArgs(commandstring));
            AssertPreviewValue("6e2d89d1-d5e8-4030-b065-f9d98b3adb45", 31);
        }

        [Test]
        public void CanOpenAndSetSpecifcStateFromImportedPresetsFileFromCLI()
        {

            string openPath = Path.Combine(TestDirectory, @"core\commandline\addmultwithoutstates.dyn");
            string presetsPath = Path.Combine(TestDirectory, @"core\commandline\statesOnly.xml");
            var runner = new CommandLineRunner(this.CurrentDynamoModel);
            string commandstring = "/o" + " " + openPath + " " + "/s" + " " + "state2" + " " + "/p" + " " + presetsPath;

            runner.Run(commandstringToArgs(commandstring));
            AssertPreviewValue("6e2d89d1-d5e8-4030-b065-f9d98b3adb45", 31);
        }

        [Test]
        public void CanOpenAndRunSpecifcStateAndOutputToFileFromCLI()
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
        public void CanOpenAndRunAllImportedStatesFromCLI()
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


    }
}
