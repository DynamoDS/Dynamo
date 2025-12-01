using Dynamo;
using Dynamo.Graph.Workspaces;
using Dynamo.PythonServices;
using Dynamo.PythonServices.EventHandlers;
using Dynamo.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using PythonNodeModels;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using static Dynamo.Models.DynamoModel;

namespace DynamoPythonTests
{
    [TestFixture]
    class PythonEngineSelectorTests : DynamoModelTestBase
    {
        private void UpdatePythonEngineAndRun(PythonNode pythonNode, string pythonEngineVersion)
        {
            pythonNode.EngineName = pythonEngineVersion;
            //to kick off a run node modified must be called
            pythonNode.OnNodeModified();
        }

        /// <summary>
        /// This test will cover the use case of the API to query certain Python engine ability for evaluation
        /// </summary>
        [Test]
        public void TestEngineSelectorInitialization()
        {
            Assert.AreEqual(true, PythonEngineManager.Instance.AvailableEngines.Any(x => x.Name == PythonEngineManager.PythonNet3EngineName));
            Assert.AreEqual(false, PythonEngineManager.Instance.AvailableEngines.Any(x => x.Name == PythonEngineManager.IronPython2EngineName));
        }

        [Test]
        public void PythonEnginePackageDependencyIsCollectedAndSerialized()
        {
            // Load JSON file graph
            string path = Path.Combine(TestDirectory, @"core\packageDependencyTests\PythonDependency.dyn");

            // Assert package dependency is not already serialized to .dyn
            using (StreamReader file = new StreamReader(path))
            {
                var data = file.ReadToEnd();
                var json = (JObject)JsonConvert.DeserializeObject(data);
                Assert.IsEmpty(json[WorkspaceReadConverter.NodeLibraryDependenciesPropString]);
            }

            string packageDirectory = Path.Combine(TestDirectory, @"core\packageDependencyTests\PythonEnginePackage");
            LoadPackage(packageDirectory);

            OpenModel(path);

            //TO-DO: Force load binaries or mock the python engine instead of loading a package
            //assert that default python engine was selected, and 2 different engines are loaded
            var currentws = CurrentDynamoModel.CurrentWorkspace;
            var pyNode = currentws.Nodes.OfType<PythonNode>().FirstOrDefault();
            Assert.IsNotNull(pyNode);
            Assert.AreEqual(pyNode.EngineName, PythonEngineManager.PythonNet3EngineName);
            Assert.AreEqual(PythonEngineManager.Instance.AvailableEngines.Count, 2);

            currentws.ForceComputeWorkspaceReferences = true;
            var packageDependencies = currentws.NodeLibraryDependencies;
            // PythonNet3 is loaded as a default package, ww should have a single package dependency.
            Assert.AreEqual(1, packageDependencies.Count);

            // Change engine to IronPython2, which is loaded as a package.
            UpdatePythonEngineAndRun(pyNode, "IronPython2");
            currentws.ForceComputeWorkspaceReferences = true;

            //assert that python engine imported from a package gets added to NodeLibraryDependencies
            packageDependencies = currentws.NodeLibraryDependencies;
            Assert.AreEqual(1, packageDependencies.Count);
            var package = packageDependencies.First();
            Assert.AreEqual(new PackageDependencyInfo("DynamoIronPython2.7", new Version("3.2.1")), package);
            Assert.AreEqual(1, package.Nodes.Count);

            Assert.IsTrue(package.IsLoaded);
            if (package is PackageDependencyInfo)
            {
                var packageDependencyState = ((PackageDependencyInfo)package).State;
                Assert.AreEqual(PackageDependencyState.Loaded, packageDependencyState);
            }

            // Assert package dependency is serialized
            var ToJson = currentws.ToJson(CurrentDynamoModel.EngineController);
            var JObject = (JObject)JsonConvert.DeserializeObject(ToJson);
            var deserializedPackageDependencies = JObject[WorkspaceReadConverter.NodeLibraryDependenciesPropString];
            Assert.AreEqual(1, deserializedPackageDependencies.Count());
            var name = deserializedPackageDependencies.First()[NodeLibraryDependencyConverter.NamePropString].Value<string>();
            Assert.AreEqual(package.Name, name);
            var version = deserializedPackageDependencies.First()[NodeLibraryDependencyConverter.VersionPropString].Value<string>();
            Assert.AreEqual(package.Version.ToString(), version);
            var nodes = deserializedPackageDependencies.First()[NodeLibraryDependencyConverter.NodesPropString].Values<string>();
            Assert.AreEqual(package.Nodes.Select(n => n.ToString("N")), nodes);
        }

        [Test]
        public void CanCopydAndPasteAndUndoPythonEngine()
        {
            var pyNode = new PythonNode();

            CurrentDynamoModel.ExecuteCommand(new Dynamo.Models.DynamoModel.CreateNodeCommand(pyNode, 0, 0, false, false));
            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            pyNode.EngineName = PythonEngineManager.PythonNet3EngineName;
            CurrentDynamoModel.AddToSelection(pyNode);
           
            CurrentDynamoModel.Copy();
            Assert.AreEqual(1, CurrentDynamoModel.ClipBoard.Count);

            CurrentDynamoModel.Paste();
            Assert.AreEqual(2, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            Assert.IsTrue(CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<PythonNode>().All(x => x.EngineName == PythonEngineManager.PythonNet3EngineName));

            CurrentDynamoModel.ExecuteCommand(new UndoRedoCommand(UndoRedoCommand.Operation.Undo));
            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            pyNode.EngineName = PythonEngineManager.IronPython2EngineName;

            CurrentDynamoModel.ExecuteCommand(
                 new UpdateModelValueCommand(
                     Guid.Empty, pyNode.GUID, nameof(PythonNode.EngineName), PythonEngineManager.PythonNet3EngineName));
            Assert.AreEqual(pyNode.EngineName, PythonEngineManager.PythonNet3EngineName);

            CurrentDynamoModel.ExecuteCommand(new UndoRedoCommand(UndoRedoCommand.Operation.Undo));

            Assert.AreEqual(pyNode.EngineName, PythonEngineManager.IronPython2EngineName);
        }

        [Test]
        public void CPytonEngineManagerAPITest()
        {
            var pythonNet3Eng = PythonEngineManager.Instance.AvailableEngines.FirstOrDefault(x => x.Name == PythonEngineManager.PythonNet3EngineName);

            Assert.IsNotNull(pythonNet3Eng);

            EvaluationStartedEventHandler start1 = ((code, bindings, scopeSet) => { scopeSet("IN", new ArrayList { " ", "  " }); });
            pythonNet3Eng.EvaluationStarted += start1;

            int counter = 0;
            EvaluationFinishedEventHandler end = ((state, code, bindings, scopeGet) => { counter++; });
            pythonNet3Eng.EvaluationFinished += end;

            var inputM = pythonNet3Eng.InputDataMarshaler as DataMarshaler;
            inputM.RegisterMarshaler((string s) => s.Length);

            var output = DSPythonNet3.DSPythonNet3Evaluator.EvaluatePythonScript(
                "OUT = sum(IN)", new ArrayList(), new ArrayList());

            inputM.UnregisterMarshalerOfType<string>();

            Assert.AreEqual(3, output);

            var outputM = pythonNet3Eng.OutputDataMarshaler as DataMarshaler;
            outputM.RegisterMarshaler((string s) => s.Length);

            EvaluationStartedEventHandler start2 = ((code, bindings, scopeSet) => { scopeSet("TEST", new ArrayList { "", " ", "  " }); });
            pythonNet3Eng.EvaluationStarted += start2;

            output = DSPythonNet3.DSPythonNet3Evaluator.EvaluatePythonScript(
                "OUT = TEST",
                new ArrayList(),
                new ArrayList());

            outputM.UnregisterMarshalerOfType<string>();

            Assert.AreEqual(new[] { 0, 1, 2 }, output);
            Assert.AreEqual(2, counter);

            pythonNet3Eng.EvaluationStarted -= start1;
            pythonNet3Eng.EvaluationStarted -= start2;
            pythonNet3Eng.EvaluationFinished -= end;
        }
    }
}
