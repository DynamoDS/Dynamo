using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Dynamo.Applications;
using Dynamo.ViewModels;
using Dynamo.Wpf.ViewModels.Watch3D;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Dynamo.Tests
{
    internal class CommandLineTests : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("VMDataBridge.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("FunctionObject.ds");
            libraries.Add("BuiltIn.ds");
            libraries.Add("ProtoGeometry.dll");
          
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
                        Assert.AreEqual(values[index].Item2,
                            ((node.ChildNodes[values[index].Item1]) as XmlElement).GetAttribute("value"));
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

        //Only for use during testing. Windows or Posix will handle this when invoking commands.
        protected static string[] CommandStringToStringArray(string commandString)
        {
            //convert string to commandlineargs, 
            var m = Regex.Matches(commandString, "([^\"]\\S*|\".+?\")\\s*");
            var list = m.Cast<Match>().Select(match => match.Value);
            //trim off leading and trailing "'s - this is handy when paths are passed as args with quotes around them.
            list = list.Select(arg => arg.Trim(new[] { '"' })).ToList();
            //strip trailing whitespace
            var argarray = list.Select(x => x.Trim()).ToArray();
            return argarray.ToArray();
        }

        //
        // DynamoCLI Tests
        //
        [Test]
        public void CanOpenAndRunDynamoModelWithCommandLineRunner()
        {
            string openpath = Path.Combine(TestDirectory, @"core\math\Add.dyn");

            var runner = new DynamoCLI.CommandLineRunner(this.CurrentDynamoModel);
            string commandstring = "/o" + " " + openpath;

            runner.Run(CommandstringToArgs(commandstring));
            AssertPreviewValue("4c5889ac-7b91-4fb5-aaad-a2128b533279", 4.0);
        }

        [Test]
        public void ImportingAnAssemblyDoesNotEffectCustomNodePaths()
        {
            //load a graph which requires first loading FFITarget.dll
            var openpath = Path.Combine(TestDirectory, @"core\commandLine\FFITarget.dyn");
            var importPath = Path.Combine(ExecutingDirectory, "FFITarget.dll");

            var runner = new DynamoCLI.CommandLineRunner(this.CurrentDynamoModel);
            string commandstring = $"/o {openpath} /i {importPath}";

            runner.Run(CommandstringToArgs(commandstring));
            //assert that the FFITarget dummy point node is created with correct properties.
            AssertPreviewValue("fba565f89c8948e290bbb423177fa7bb", 2.0);
            AssertPreviewValue("5a1fae3e13ce4ccfba737ec75057907b", 2.0);

            //the custom package paths should not be effected by running the CLI.
            this.CurrentDynamoModel.PreferenceSettings.CustomPackageFolders.ForEach(packagePath =>
            {
                Assert.IsFalse(packagePath.Contains(importPath) || packagePath == importPath);
            });

        }

        [Test]
        public void CanImportDependencyBeforeRunningGraph()
        {
            //load a graph which requires first loading FFITarget.dll
            var openpath = Path.Combine(TestDirectory, @"core\commandLine\FFITarget.dyn");
            var importPath = Path.Combine(ExecutingDirectory, "FFITarget.dll");

            var runner = new DynamoCLI.CommandLineRunner(this.CurrentDynamoModel);
            string commandstring = $"/o {openpath} /i {importPath}";

            runner.Run(CommandstringToArgs(commandstring));
            //assert that the FFITarget dummy point node is created with correct properties.
            AssertPreviewValue("fba565f89c8948e290bbb423177fa7bb", 2.0);
            AssertPreviewValue("5a1fae3e13ce4ccfba737ec75057907b", 2.0);

        }

        [Test]
        public void CanImportMultipleDepsBeforeRunningGraph()
        {
            //load a graph which requires first loading FFITarget.dll and SampleLibraryZeroTouch.dll
            var openpath = Path.Combine(TestDirectory, @"core\commandLine\multipleDeps.dyn");
            var importPath1 = Path.Combine(ExecutingDirectory, "FFITarget.dll");
            var importPath2 = Path.Combine(TestDirectory,"pkgs", "Dynamo Samples","bin" ,"SampleLibraryZeroTouch.dll");

            var runner = new DynamoCLI.CommandLineRunner(this.CurrentDynamoModel);
            string commandstring = $"/o {openpath} /i {importPath1} /i \"{importPath2}\"";

            runner.Run(CommandstringToArgs(commandstring));
            //assert that this node from the samples ZT dll produces a correct result.
            AssertPreviewValue("04c1531865e34ecb80a265c7822450aa",  "Point(X = 4.000, Y = 2.000, Z = 2.000)" );

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
            AssertOutputValuesForGuid("47b78c9b-98b3-4852-935f-0d03f52a65b3",
                new List<Tuple<int, string>> {Tuple.Create(0, "{1000,2,3,{1,2,3}}")}, output);
            AssertOutputValuesForGuid("8229dec7-b4ae-463b-a7ac-e36671fefef0",
                new List<Tuple<int, string>> {Tuple.Create(0, "{Surface,Surface,Surface,Surface,Surface,Surface}")},
                output);
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
            AssertOutputValuesForGuid("36c30251-c867-4d73-9a3b-24f3e9ab00e5",
                new List<Tuple<int, string>>
                {
                    Tuple.Create(0, "{e : 6, d : {4, 5}, a : 1, c : {bar : 999, foo : 99}, b : 2}")
                }, output);
            AssertOutputValuesForGuid("6a5bcff0-ce40-4773-aee6-88d99104b4a7",
                new List<Tuple<int, string>>
                {
                    Tuple.Create(0, "{a,b,c,d,e}"),
                    Tuple.Create(1, "{1,2,{bar : 999, foo : 99},{4,5},6}")
                }, output);
        }

        //
        // DynamoWPFCLI Tests
        //
        [Test]
        public void CanOpenAndRunDynamoModelWithWPFCommandLineRunner()
        {
            string openpath = Path.Combine(TestDirectory, @"core\math\Add.dyn");
            var viewModel = DynamoViewModel.Start(
                new DynamoViewModel.StartConfiguration()
                {
                    DynamoModel = this.CurrentDynamoModel,
                    Watch3DViewModel =
                        new DefaultWatch3DViewModel(null, new Watch3DViewModelStartupParams(this.CurrentDynamoModel))
                        {
                            Active = false,
                            CanBeActivated = false
                        }
                });

            var runner = new DynamoWPFCLI.CommandLineRunnerWPF(viewModel);
            string commandstring = "/o" + " " + openpath;

            runner.Run(CommandstringToArgs(commandstring));
            AssertPreviewValue("4c5889ac-7b91-4fb5-aaad-a2128b533279", 4.0);
        }

        [Test]
        public void CanOpenAndRunFileWihtListsCorrectlyToOutputFileFromDynamoWPFCLIexe()
        {
            string openpath = Path.Combine(TestDirectory, @"core\commandline\simplelists.dyn");
            var newpath = GetNewFileNameOnTempPath("xml");
            string commandstring = "/o" + " " + openpath + " " + "/v" + " " + newpath;

            DynamoWPFCLI.Program.Main(CommandStringToStringArray(commandstring));
            var output = new XmlDocument();
            output.Load(newpath);
            AssertOutputValuesForGuid("47b78c9b-98b3-4852-935f-0d03f52a65b3",
                new List<Tuple<int, string>> {Tuple.Create(0, "{1000,2,3,{1,2,3}}")}, output);
            AssertOutputValuesForGuid("8229dec7-b4ae-463b-a7ac-e36671fefef0",
                new List<Tuple<int, string>> {Tuple.Create(0, "{Surface,Surface,Surface,Surface,Surface,Surface}")},
                output);
        }

        [Test]
        public void CanOpenAndRunFileWithDictionaryCorrectlyToOutputFileFromDynamoWPFCLIexe()
        {
            string openpath = Path.Combine(TestDirectory, @"core\commandline\simpleDict.dyn");
            var newpath = GetNewFileNameOnTempPath("xml");
            string commandstring = "/o" + " " + openpath + " " + "/v" + " " + newpath;

            DynamoWPFCLI.Program.Main(CommandStringToStringArray(commandstring));
            var output = new XmlDocument();
            output.Load(newpath);
            AssertOutputValuesForGuid("36c30251-c867-4d73-9a3b-24f3e9ab00e5",
                new List<Tuple<int, string>>
                {
                    Tuple.Create(0, "{e : 6, d : {4, 5}, a : 1, c : {bar : 999, foo : 99}, b : 2}")
                }, output);
            AssertOutputValuesForGuid("6a5bcff0-ce40-4773-aee6-88d99104b4a7",
                new List<Tuple<int, string>>
                {
                    Tuple.Create(0, "{a,b,c,d,e}"),
                    Tuple.Create(1, "{1,2,{bar : 999, foo : 99},{4,5},6}")
                }, output);
        }

        [Test]
        public void CanOpenAndRunFileWithCustomNodeAndOutputGeometryFromDynamoWPFCLIexe()
        {
            string openpath = Path.Combine(TestDirectory, @"core\commandline\GeometryTest.dyn");
            var newpath = GetNewFileNameOnTempPath("json");
            string geometrypath = Path.Combine(TestDirectory, @"core\commandline\GeometryTest.json");
            string commandstring = "/o" + " " + openpath + " " + "/g" + " " + newpath;

            DynamoWPFCLI.Program.Main(CommandStringToStringArray(commandstring));

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