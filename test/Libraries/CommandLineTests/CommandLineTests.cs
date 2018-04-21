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
using Dynamo.ViewModels;
using DynamoCLI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
            var viewModel = DynamoViewModel.Start(
                new DynamoViewModel.StartConfiguration()
                {
                    DynamoModel = this.CurrentDynamoModel
                });

            var runner = new CommandLineRunner(viewModel);
            string commandstring = "/o" + " " + openpath;

            runner.Run(CommandstringToArgs(commandstring));
            AssertPreviewValue("4c5889ac-7b91-4fb5-aaad-a2128b533279", 4.0);
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

        [Test]
        public void CanOpenAndRunFileWithDictionaryCorrectlyToOutputFileFromDynamoCLIexe()
        {
            string openpath = Path.Combine(TestDirectory, @"core\commandline\simpleDict.dyn");
            var newpath = GetNewFileNameOnTempPath("xml");
            string commandstring = "/o" + " " + openpath + " " + "/v" + " " + newpath;

            DynamoCLI.Program.Main(CommandStringToStringArray(commandstring));
            var output = new XmlDocument();
            output.Load(newpath);
            AssertOutputValuesForGuid("36c30251-c867-4d73-9a3b-24f3e9ab00e5", new List<Tuple<int, string>> { Tuple.Create(0, "{e : 6, d : {4, 5}, a : 1, c : {bar : 999, foo : 99}, b : 2}") }, output);
            AssertOutputValuesForGuid("6a5bcff0-ce40-4773-aee6-88d99104b4a7", new List<Tuple<int, string>> { Tuple.Create(0, "{a,b,c,d,e}"), Tuple.Create(1, "{1,2,{bar : 999, foo : 99},{4,5},6}") }, output);

        }
        [Test]
        public void CanOpenAndRunFileWithCustomNodeAndOutputGeometryFromDynamoCLIexe()
        {
            string openpath = Path.Combine(TestDirectory, @"core\commandline\GeometryTest.dyn");
            var newpath = GetNewFileNameOnTempPath("json");
            string geometrypath = Path.Combine(TestDirectory, @"core\commandline\GeometryTest.json");
            string commandstring = "/o" + " " + openpath + " " + "/g" + " " + newpath;

            DynamoCLI.Program.Main(CommandStringToStringArray(commandstring));

            using (StreamReader geometryFile = new StreamReader(geometrypath))
            {
                using (StreamReader newFile = new StreamReader(newpath))
                {
                    Assert.IsNotNull(geometryFile);
                    var geometry = geometryFile.ReadToEnd();

                    Assert.IsNotNull(newFile);
                    var newGeometry = newFile.ReadToEnd();

                    Assert.IsNotNullOrEmpty(geometry);
                    var geometryJson = JsonConvert.DeserializeObject(geometry) as JArray;

                    Assert.IsNotNullOrEmpty(newGeometry);
                    var newGeometryJson = JsonConvert.DeserializeObject(newGeometry) as JArray;

                    Assert.IsNotNull(geometryJson);
                    Assert.IsNotNull(newGeometryJson);

                    Assert.IsTrue(JToken.DeepEquals(geometryJson, newGeometryJson));
                }
            }
        }

    }
}
