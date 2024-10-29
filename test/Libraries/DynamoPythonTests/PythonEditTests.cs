using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using DSCPython;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using PythonNodeModels;
using Dynamo.PythonServices;
using DynCmd = Dynamo.Models.DynamoModel;
using System.Threading;
using PythonNodeModelsWpf;

namespace Dynamo.Tests
{
    [RequiresThread(ApartmentState.STA)]
    public class PythonEditTests : DynamoViewModelUnitTest
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DSCPython.dll");
            base.GetLibrariesToPreload(libraries);
        }

        /// <summary>
        ///    Returns a list of python engines from the PythonEngineVersion Enum. 
        /// </summary>
        private IEnumerable<string> GetPythonEnginesList()
        {
            return new List<string>() { PythonEngineManager.CPython3EngineName };
        }

        /// <summary>
        ///    Updates Engine property for a single python node. 
        /// </summary>
        private void UpdatePythonEngineAndRun(PythonNode pythonNode, string pythonEngineVersion)
        {
            pythonNode.EngineName = pythonEngineVersion;
            //to kick off a run node modified must be called
            pythonNode.OnNodeModified();
        }

        /// <summary>
        ///    Updates Engine property for a list of python nodes. 
        /// </summary>
        private void UpdateEngineAndRunForAllPythonNodes(List<PythonNode> list, string pythonEngineVersion)
        {
            foreach (var pyNode in list)
            {
                pyNode.EngineName = pythonEngineVersion;
                pyNode.OnNodeModified();
            }
        }

        private void UpdatePythonNodeContent(ModelBase pythonNode, string value)
        {
            var command = new DynCmd.UpdateModelValueCommand(
                System.Guid.Empty, pythonNode.GUID, "ScriptContent", value);

            ViewModel.ExecuteCommand(command);
        }

        /// <summary>
        ///     Counts the non-overlapping occurrences of a specified substring within a given string.
        /// </summary>
        private int CountSubstrings(string code, string subscting)
        {
            int count = 0;
            int index = code.IndexOf(subscting, 0);

            while (index != -1)
            {
                count++;
                index = code.IndexOf(subscting, index + subscting.Length);
            }
            return count;
        }

        [Test]
        public void PythonScriptEdit_WorkspaceChangesReflected()
        {
            // open file
            var model = ViewModel.Model;
            var examplePath = Path.Combine(TestDirectory, @"core\python", "python.dyn");
            ViewModel.OpenCommand.Execute(examplePath);

            // get the python node
            var workspace = model.CurrentWorkspace;
            var nodeModel = workspace.NodeFromWorkspace("3bcad14e-d086-4278-9e08-ed2759ef92f3");
            var pynode = nodeModel as PythonNode;
            Assert.NotNull(pynode);

            // make changes to python script
            UpdatePythonNodeContent(pynode, @"print 'okay'");

            // workspace has changes
            Assert.IsTrue(model.CurrentWorkspace.HasUnsavedChanges);
        }

        [Test]
        public void PythonNodeEnginePropertyTest()
        {
            // open file
            var model = ViewModel.Model;
            var examplePath = Path.Combine(TestDirectory, @"core\python", "python.dyn");
            ViewModel.OpenCommand.Execute(examplePath);

            // get the python node and check Engine property
            var workspace = model.CurrentWorkspace;
            var nodeModel = workspace.NodeFromWorkspace("3bcad14e-d086-4278-9e08-ed2759ef92f3");
            var pynode = nodeModel as PythonNode;
            Assert.AreEqual(pynode.EngineName, PythonEngineManager.IronPython2EngineName);

            // workspace has no changes
            Assert.IsFalse(model.CurrentWorkspace.HasUnsavedChanges);

            // Serialize DYN and deserialize to double check check Engine field
            var path = GetNewFileNameOnTempPath();
            ViewModel.Model.CurrentWorkspace.Save(path);

            var fileContents = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(fileContents))
                return;

            JObject dynObj = JObject.Parse(fileContents);
            var pythonTokens = dynObj["Nodes"].Where(t => t.Value<string>("NodeType") == "PythonScriptNode").Select(t => t);
            Assert.IsNotNull(pythonTokens);
            Assert.IsTrue(pythonTokens.Any(t => t.Value<string>("Engine") == PythonEngineManager.IronPython2EngineName));
        }

        [Test]
        public void PythonScriptEdit_CustomNode()
        {
            // open file
            var model = ViewModel.Model;
            var examplePath = Path.Combine(TestDirectory, @"core\python", "PythonCustomNodeHomeWorkspace.dyn");
            ViewModel.OpenCommand.Execute(examplePath);
            var homeWorkspace = model.CurrentWorkspace;

            // open custom node
            var customNodeModel = homeWorkspace.NodeFromWorkspace("83c2b47d81e14226a93941f1e47dc47d") as Function;
            ViewModel.GoToWorkspaceCommand.Execute(customNodeModel.Definition.FunctionId);
            var customNodeWorkspace = model.CurrentWorkspace;

            // get the python node
            var nodeModel = customNodeWorkspace.NodeFromWorkspace("439c4e7bd4ed45f49d5786209c2ec403");
            var pynode = nodeModel as PythonNode;
            Assert.NotNull(pynode);

            // make changes to python script
            UpdatePythonNodeContent(pynode, @"OUT = IN[0] * 2");

            // custom node workspace should have changes, home workspace should not
            Assert.IsTrue(customNodeWorkspace.HasUnsavedChanges);
            Assert.IsFalse(homeWorkspace.HasUnsavedChanges);

            /* TODO: uncomment this section after undo issues are resolved
            // undo change
            ViewModel.UndoCommand.Execute(null);

            // custom node workspace should not have changes, and neither should home
            Assert.IsFalse(customNodeWorkspace.HasUnsavedChanges);
            Assert.IsFalse(homeWorkspace.HasUnsavedChanges);

            // make home workspace current
            ViewModel.HomeCommand.Execute(null);

            // make changes to python script
            UpdatePythonNodeContent(pynode, @"OUT = IN[0] * 2");

            // custom node workspace should have changes, home workspace should not
            Assert.IsTrue(customNodeWorkspace.HasUnsavedChanges);
            Assert.IsFalse(homeWorkspace.HasUnsavedChanges);
            */
        }


        [Test]
        public void PythonScriptEdit_UndoRedo()
        {
            // open file
            var model = ViewModel.Model;
            var examplePath = Path.Combine(TestDirectory, @"core\python", "python.dyn");
            ViewModel.OpenCommand.Execute(examplePath);

            // get the python node
            var pynode = model.CurrentWorkspace.Nodes.OfType<PythonNode>().First();
            Assert.NotNull(pynode);

            // save original script
            var origScript = pynode.Script;

            // make changes to python script
            var newScript = @"print 'okay'";
            UpdatePythonNodeContent(pynode, newScript);

            // workspace has changes
            Assert.IsTrue(model.CurrentWorkspace.HasUnsavedChanges);

            // undo change
            ViewModel.UndoCommand.Execute(null);

            // check value is back to original
            Assert.AreEqual(pynode.Script, origScript);

            // redo change
            ViewModel.RedoCommand.Execute(null);

            // script is edited
            Assert.AreEqual(pynode.Script, newScript);
        }

        [Test]
        public void PythonScriptEdit_ConvertTabsToSpacesButton()
        {
            // Open file and get the Python node
            var model = ViewModel.Model;
            var examplePath = Path.Combine(TestDirectory, @"core\python", "ConvertTabsToSpaces.dyn");
            ViewModel.OpenCommand.Execute(examplePath);
            var pynode = model.CurrentWorkspace.Nodes.OfType<PythonNode>().First();

            // Asset the node is loaded
            Assert.NotNull(pynode, "Python node should be loaded from the file.");

            // number of spaces is hard coded as providing a public property or changing the access
            // level of PythonIndentationStrategy.ConvertTabsToSpaces is unnecessary for this purpose only
            var spacesIndent = new string(' ', 4);
            var tabIndent = "\t";

            // Assert initial conditions : 17 tab indents and no space indents
            Assert.IsTrue(pynode.Script.Count(c => c == '\t') == 17);
            Assert.IsTrue(CountSubstrings(pynode.Script, spacesIndent) == 0);

            // Convert tabs to spaces
            var convertedString = PythonIndentationStrategy.ConvertTabsToSpaces(pynode.Script);
            pynode.Script = convertedString;

            // Assert the tab indents are converted to space indents
            Assert.IsTrue(pynode.Script.Count(c => c == '\t') == 0);
            Assert.IsTrue(CountSubstrings(pynode.Script, spacesIndent) == 17);
        }

        [Test]
        public void VarInPythonScriptEdit_WorkspaceChangesReflected()
        {
            // open file
            var model = ViewModel.Model;
            var examplePath = Path.Combine(TestDirectory, @"core\python", "varinpython.dyn");
            ViewModel.OpenCommand.Execute(examplePath);

            // get the python node
            var pynode = model.CurrentWorkspace.Nodes.OfType<PythonNode>().First();
            Assert.NotNull(pynode);

            // make changes to python script
            UpdatePythonNodeContent(pynode, @"print 'okay'");

            // workspace has changes
            Assert.IsTrue(model.CurrentWorkspace.HasUnsavedChanges);
        }

        [Test]
        public void VarInPythonScriptEdit_UndoRedo()
        {
            // open file
            var model = ViewModel.Model;
            var examplePath = Path.Combine(TestDirectory, @"core\python", "varinpython.dyn");
            ViewModel.OpenCommand.Execute(examplePath);

            // get the python node
            var pynode = model.CurrentWorkspace.Nodes.OfType<PythonNode>().First();
            Assert.NotNull(pynode);

            // save original script
            var origScript = pynode.Script;

            // make changes to python script
            var newScript = @"print 'okay'";
            UpdatePythonNodeContent(pynode, newScript);

            // workspace has changes
            Assert.IsTrue(model.CurrentWorkspace.HasUnsavedChanges);

            // undo change
            ViewModel.UndoCommand.Execute(null);

            // check value is back to original
            Assert.AreEqual(pynode.Script, origScript);

            // redo change
            ViewModel.RedoCommand.Execute(null);

            // script is edited
            Assert.AreEqual(pynode.Script, newScript);
        }

        [Test]
        public void ReturnPythonDictionary_AsDynamoDictionary()
        {
            // open test graph
            var examplePath = Path.Combine(TestDirectory, @"core\python", "python_dict.dyn");
            ViewModel.OpenCommand.Execute(examplePath);

            var guid = "490a8d54d0fa4782ae18c81f6eef8306";

            var nodeModel = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace(guid);
            var pynode = nodeModel as PythonNode;

            var count = 0;
            foreach (var pythonEngine in GetPythonEnginesList())
            {
                UpdatePythonEngineAndRun(pynode, pythonEngine);

                ViewModel.HomeSpace.Run();
                count++;
                Assert.AreEqual(count, (GetModel().CurrentWorkspace as HomeWorkspaceModel).EvaluationCount);
                AssertPreviewValue(guid, new Dictionary<string, int> { { "abc", 123 }, { "def", 345 } });
            }
        }

        [Test]
        public void InputDynamoDictionary_AsPythonDictionary()
        {
            // open test graph
            var examplePath = Path.Combine(TestDirectory, @"core\python", "python_dict2.dyn");
            ViewModel.OpenCommand.Execute(examplePath);

            var guid = "490a8d54d0fa4782ae18c81f6eef8306";

            var nodeModel = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace(guid);
            var pynode = nodeModel as PythonNode;

            var count = 0;
            foreach (var pythonEngine in GetPythonEnginesList())
            {
                UpdatePythonEngineAndRun(pynode, pythonEngine);

                ViewModel.HomeSpace.Run();
                count++;
                Assert.AreEqual(count, (GetModel().CurrentWorkspace as HomeWorkspaceModel).EvaluationCount);

                AssertPreviewValue(guid,
                      new List<object> { new Dictionary<string, int> { { "abcd", 123 } }, new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 } });
            }
        }

        [Test]
        public void PythonGeometryTest()
        {
            // open test graph
            var examplePath = Path.Combine(TestDirectory, @"core\python", "PythonGeometry.dyn");
            ViewModel.OpenCommand.Execute(examplePath);

            var pythonGUID = "3bcad14ed08642789e08ed2759ef92f3";
            var lineGUID = "cac9ff74bed14ff285294878fa849cd0";
            var circleCenterPointGUID = "6bb5d21d7815467db1d2ccfc77713337";
            var circleRadiusGUID = "88746807ba6d4c81a37dbfb187acca81";

            var nodeModel = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace(pythonGUID);
            var pynode = nodeModel as PythonNode;

            var count = 0;
            foreach (var pythonEngine in GetPythonEnginesList())
            {
                UpdatePythonEngineAndRun(pynode, pythonEngine);
                ViewModel.HomeSpace.Run();
                count++;
                Assert.AreEqual(count, (GetModel().CurrentWorkspace as HomeWorkspaceModel).EvaluationCount);

                var line = GetPreviewValue(lineGUID) as Line;
                Assert.AreEqual(line.Length, 5);

                AssertPreviewValue(circleCenterPointGUID, Point.ByCoordinates(0, 0, 0));
                AssertPreviewValue(circleRadiusGUID, 5);
            }
        }

        [Test]
        public void PythonNodeEnginePropertyChangeTest()
        {
            // open test graph
            var examplePath = Path.Combine(TestDirectory, @"core\python", "pythonEngineTest.dyn");
            ViewModel.OpenCommand.Execute(examplePath);
            var pythonGUID = "83a5b1d2-dc58-4d8f-823f-861ac7d565f1";

            var nodeModel = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace(pythonGUID);
            var pynode = nodeModel as PythonNode;

            var count = 0;
            foreach (var pythonEngine in GetPythonEnginesList())
            {
                UpdatePythonEngineAndRun(pynode, pythonEngine);
                ViewModel.HomeSpace.Run();
                count++;
                Assert.AreEqual(count, (GetModel().CurrentWorkspace as HomeWorkspaceModel).EvaluationCount);

                var nodeValue = GetPreviewValue(pythonGUID);

                if (pythonEngine == PythonEngineManager.IronPython2EngineName)
                {
                    Assert.AreEqual("2.7.9", nodeValue);
                }
                else if (pythonEngine == PythonEngineManager.CPython3EngineName)
                {
                    Assert.AreEqual("3.9.12", nodeValue);
                }
            }
        }

        [Test]
        public void ReturnIronPythonDictionary_AsDynamoDictionary()
        {
            // open test graph
            var examplePath = Path.Combine(TestDirectory, @"core\python", "netDict_from_python.dyn");
            ViewModel.OpenCommand.Execute(examplePath);

            var guid = "490a8d54d0fa4782ae18c81f6eef8306";

            var nodeModel = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace(guid);
            var pynode = nodeModel as PythonNode;

            var count = 0;
            foreach (var pythonEngine in GetPythonEnginesList())
            {
                UpdatePythonEngineAndRun(pynode, pythonEngine);
                ViewModel.HomeSpace.Run();
                count++;
                Assert.AreEqual(count, (GetModel().CurrentWorkspace as HomeWorkspaceModel).EvaluationCount);

                AssertPreviewValue(guid, new Dictionary<string, int> { { "abc", 123 }, { "def", 10 } });
            }
        }

        [Test]
        public void BigInteger_CanBeMarshaledAsInt64()
        {
            // open test graph
            var examplePath = Path.Combine(TestDirectory, @"core\python", "BigIntegerToLong.dyn");
            ViewModel.OpenCommand.Execute(examplePath);

            var guid = "23088248d7b1441abbc5ada07fcdf154";
            var pythonGUID = "547d1dd9203746bbaa2b7bd448c8124a";

            var nodeModel = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace(pythonGUID);
            var pynode = nodeModel as PythonNode;
            var count = 0;
            foreach (var pythonEngine in GetPythonEnginesList())
            {
                UpdatePythonEngineAndRun(pynode, pythonEngine);

                ViewModel.HomeSpace.Run();
                count++;
                Assert.AreEqual(count, (GetModel().CurrentWorkspace as HomeWorkspaceModel).EvaluationCount);

                AssertPreviewValue(guid,
                         new[] { "System.Int64", "System.Double", "System.Int64", "System.Int64", "System.Numerics.BigInteger" });
            }
        }

        [Test]
        public void TestWorkspaceWithMultiplePythonEngines()
        {
            // open test graph
            var examplePath = Path.Combine(TestDirectory, @"core\python", "WorkspaceWithMultiplePythonEngines.dyn");
            ViewModel.OpenCommand.Execute(examplePath);

            var nodeModels = ViewModel.Model.CurrentWorkspace.Nodes.Where(n => n.NodeType == "PythonScriptNode");
            List<PythonNode> pythonNodes = nodeModels.Cast<PythonNode>().ToList();

            var pynode1 = pythonNodes.ElementAt(0);
            var pynode2 = pythonNodes.ElementAt(1);

            Assert.IsTrue(PythonEngineManager.Instance.AvailableEngines.Any(x => x.Name == PythonEngineManager.CPython3EngineName));

            // Error when running IronPython2 script while IronPython2 engine is not installed
            AssertPreviewValue(pynode1.GUID.ToString("N"), null);
            AssertPreviewValue(pynode2.GUID.ToString("N"), null);

            UpdatePythonEngineAndRun(pynode1, PythonEngineManager.CPython3EngineName);
            Assert.IsTrue(ViewModel.Model.CurrentWorkspace.HasUnsavedChanges);
            AssertPreviewValue(pynode1.GUID.ToString("N"), "3.9.12");

            UpdatePythonEngineAndRun(pynode2, PythonEngineManager.CPython3EngineName);
            Assert.IsTrue(ViewModel.Model.CurrentWorkspace.HasUnsavedChanges);
            AssertPreviewValue(pynode2.GUID.ToString("N"), new List<string> { "3.9.12", "3.9.12" });
        }

        [Test]
        public void Python_CanReferenceDynamoServicesExecutionSession()
        {
            // open test graph
            var examplePath = Path.Combine(TestDirectory, @"core\python", "python_refDynamoServices.dyn");
            ViewModel.OpenCommand.Execute(examplePath);

            var guid = "296e339254e845b695caa1a116500be0";

            var nodeModel = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace(guid);
            var pynode = nodeModel as PythonNode;
            var count = 0;
            foreach (var pythonEngine in GetPythonEnginesList())
            {
                UpdatePythonEngineAndRun(pynode, pythonEngine);

                ViewModel.HomeSpace.Run();
                count++;
                Assert.AreEqual(count, (GetModel().CurrentWorkspace as HomeWorkspaceModel).EvaluationCount);

                // Python script returns list of paths contained in PathManager.PackageDirectories
                AssertPreviewCount(guid, 3);
            }
        }

        [Test]
        public void CPythonClassCanBeUsedInDownStreamNode()
        {
            // open test graph
            var examplePath = Path.Combine(TestDirectory, @"core\python", "cpythoncustomclass.dyn");
            ViewModel.OpenCommand.Execute(examplePath);

            var downstream1 = ViewModel.Model.CurrentWorkspace.Nodes.First(x => x.Name == "downstream1");
            var downstream2 = ViewModel.Model.CurrentWorkspace.Nodes.First(x => x.Name == "downstream2");

            ViewModel.HomeSpace.Run();
            AssertPreviewValue(downstream1.GUID.ToString(), "firstName");
            AssertPreviewValue(downstream2.GUID.ToString(), "firstNamelastname");

        }
        [Test]
        public void CPythonClassCanBeModifiedInDownStreamNode()
        {
            // open test graph
            var examplePath = Path.Combine(TestDirectory, @"core\python", "cpythoncustomclass_modified.dyn");
            ViewModel.OpenCommand.Execute(examplePath);

            var downstream1 = ViewModel.Model.CurrentWorkspace.Nodes.First(x => x.Name == "downstream1");
            var downstream2 = ViewModel.Model.CurrentWorkspace.Nodes.First(x => x.Name == "downstream2");

            ViewModel.HomeSpace.Run();
            AssertPreviewValue(downstream2.GUID.ToString(), "joe");
        }

        [Test]
        public void TwoCPythonHandlesReturnedFromSameNodeHaveSameHandleID()
        {
            // open test graph
            var examplePath = Path.Combine(TestDirectory, @"core\python", "cpythoncustomclass_returnManyInstances.dyn");
            ViewModel.OpenCommand.Execute(examplePath);

            var classdef = ViewModel.Model.CurrentWorkspace.Nodes.First(x => x.Name == "classdef");

            ViewModel.HomeSpace.Run();
            var handles = classdef.CachedValue.GetElements().Select(x => x.Data).Cast<DynamoCPythonHandle>().ToList<DynamoCPythonHandle>();
            var firstMemLoc = handles.First().PythonObjectID;
            Assert.IsTrue(handles.All(x => x.PythonObjectID == firstMemLoc));
        }

        [Test]
        public void TwoCPythonHandlesReturnedFromDifferentNodesHaveSameHandleID()
        {
            // open test graph
            var examplePath = Path.Combine(TestDirectory, @"core\python", "cpythoncustomclass_returnManyInstancesFromManyNodes.dyn");
            ViewModel.OpenCommand.Execute(examplePath);

            var classdef = ViewModel.Model.CurrentWorkspace.Nodes.First(x => x.Name == "classdef");
            var downstream1 = ViewModel.Model.CurrentWorkspace.Nodes.First(x => x.Name == "downstream1");

            ViewModel.HomeSpace.Run();
            var handles = classdef.CachedValue.GetElements().Select(x => x.Data);
            handles = handles.Concat(downstream1.CachedValue.GetElements().Select(x => x.Data));
            var dynamoHandles = handles.Cast<DynamoCPythonHandle>().ToList();

            var firstMemLoc = dynamoHandles.First().PythonObjectID;
            Assert.IsTrue(dynamoHandles.All(x => x.PythonObjectID == firstMemLoc));
        }

        [Test]
        public void CPythonClassCanBeReturnedAndSafelyDisposedInDownStreamNode()
        {
            // open test graph
            var examplePath = Path.Combine(TestDirectory, @"core\python", "cpythoncustomclass_modified.dyn");
            ViewModel.OpenCommand.Execute(examplePath);

            var classdef = ViewModel.Model.CurrentWorkspace.Nodes.First(x => x.Name == "classdef");
            var downstream1 = ViewModel.Model.CurrentWorkspace.Nodes.First(x => x.Name == "downstream1");
            var downstream2 = ViewModel.Model.CurrentWorkspace.Nodes.First(x => x.Name == "downstream2");

            ViewModel.HomeSpace.Run();
            AssertPreviewValue(downstream2.GUID.ToString(), "joe");
            Assert.AreEqual(2, DynamoCPythonHandle.HandleCountMap.First(x => x.ToString().Contains("myClass")).Value);


            ViewModel.Model.CurrentWorkspace.Nodes.OfType<CodeBlockNodeModel>().First().UpdateValue(new UpdateValueParams("Code", "\"foo\";"));

            ViewModel.HomeSpace.Run();
            AssertPreviewValue(downstream2.GUID.ToString(), "foo");
            Assert.AreEqual(2, DynamoCPythonHandle.HandleCountMap.First(x => x.ToString().Contains("myClass")).Value);

            ViewModel.Model.CurrentWorkspace.Nodes.OfType<CodeBlockNodeModel>().First().UpdateValue(new UpdateValueParams("Code", "\"bar\";"));

            ViewModel.HomeSpace.Run();
            AssertPreviewValue(downstream2.GUID.ToString(), "bar");

            var deleteCmd = new DynamoModel.DeleteModelCommand(downstream1.GUID);
            ViewModel.Model.ExecuteCommand(deleteCmd);

            Assert.AreEqual(1, DynamoCPythonHandle.HandleCountMap.First(x => x.ToString().Contains("myClass")).Value);

            var deleteCmd2 = new DynamoModel.DeleteModelCommand(classdef.GUID);
            ViewModel.Model.ExecuteCommand(deleteCmd2);

            Assert.IsEmpty(DynamoCPythonHandle.HandleCountMap.Where(x => x.ToString().Contains("myClass")));
        }

        [Test]
        public void VerifySysPathValueForCPythonEngine()
        {
            // open test graph
            var examplePath = Path.Combine(TestDirectory, @"core\python", "CPythonSysPath.dyn");
            ViewModel.OpenCommand.Execute(examplePath);

            var firstPythonNodeGUID = "07ea2d39-811e-4df7-a38c-8c784d5079b0";
            var secondPythonNodeGUID = "a3058920-586a-43d9-9806-7d41c36803bb";

            var sysPathList = GetFlattenedPreviewValues(firstPythonNodeGUID);

            // Verify that the custom path is added to the 'sys.path'.
            Assert.AreEqual(sysPathList.Count(), 4);
            Assert.AreEqual(sysPathList.Last(), "C:\\Program Files\\dotnet");

            // Change the python engine for the 2nd node and verify that the custom path is not reflected in the 2nd node. 
            // Only the default python paths would be present in 'sys.path' when a python node is evaluated.
            var nodeModel = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace(secondPythonNodeGUID);
            var pynode = nodeModel as PythonNode;
            UpdatePythonEngineAndRun(pynode, PythonEngineManager.CPython3EngineName);
            sysPathList = GetFlattenedPreviewValues(secondPythonNodeGUID);
            Assert.AreEqual(sysPathList.Count(), 3);
            Assert.AreNotEqual(sysPathList.Last(), "C:\\Program Files\\dotnet");
        }

        [Test]
        public void Test_Hidden_Properties_Python()
        {
            RunModel(@"core\python\HiddenProperties_Python.dyn");

            var getItemAtIndex2 = "ad1f2ed7-6373-4381-aa93-df707c5e6339";
            AssertPreviewValue(getItemAtIndex2, new string[] { "TSplineVertex", "TSplineVertex" });
        }

        [Test]
        public void CpythonRestart_ReloadsModules()
        {
            var modName = "reload_test2";
            (ViewModel.CurrentSpace as HomeWorkspaceModel).RunSettings.RunType = RunType.Manual;
            var tempPath = Path.Combine(TempFolder, $"{modName}.py");

            //clear file.
            File.WriteAllText(tempPath, "value ='Hello World!'\n");

            //we have to shutdown python before this test to make sure we're starting in a clean state.
            this.ViewModel.Model.OnRequestPythonReset(PythonEngineManager.CPython3EngineName);
            try
            {
                var script = $@"import sys
sys.path.append(r'{Path.GetDirectoryName(tempPath)}')
import {modName}
OUT = {modName}.value";


                var pythonNode = new PythonNode();
                ViewModel.CurrentSpace.AddAndRegisterNode(pythonNode);
                pythonNode.EngineName = PythonEngineManager.CPython3EngineName;
                UpdatePythonNodeContent(pythonNode, script);
                RunCurrentModel();
                AssertPreviewValue(pythonNode.GUID.ToString(), "Hello World!");

                //now modify the file.
                File.AppendAllLines(tempPath, new string[] { "value ='bye'" });

                //user restarts manually, this will cause a dynamo and python engine reset
                this.ViewModel.Model.OnRequestPythonReset(PythonEngineManager.CPython3EngineName);

                RunCurrentModel();
                AssertPreviewValue(pythonNode.GUID.ToString(), "bye");

            }
            finally
            {
                File.Delete(tempPath);
            }
        }

        /// <summary>
        /// Currently unsupported use case - if a python module imported, and then moved on disk, reload will fail unless it is manually removed from
        /// sys.modules as well. This should be rare.
        /// </summary>
        [Test]
        [Category("Failure")]
        [Category("TechDebt")]
        public void CpythonRestart_ReloadModuleFromDifferentLocationFails()
        {
            var modName = "reload_test3";
            (ViewModel.CurrentSpace as HomeWorkspaceModel).RunSettings.RunType = RunType.Manual;
            var tempPath = Path.Combine(TempFolder, $"{modName}.py");

            //clear file.
            File.WriteAllText(tempPath, "value ='Hello World!'\n");
            try
            {
                var script = $@"import sys
sys.path.append(r'{Path.GetDirectoryName(tempPath)}')
import {modName}
OUT = {modName}.value";


                var pythonNode = new PythonNode();
                ViewModel.CurrentSpace.AddAndRegisterNode(pythonNode);
                pythonNode.EngineName = PythonEngineManager.CPython3EngineName;
                UpdatePythonNodeContent(pythonNode, script);
                RunCurrentModel();
                AssertPreviewValue(pythonNode.GUID.ToString(), "Hello World!");

                //delete the .py file, and create a new module with the same name, but a new location
                File.Delete(tempPath);
                var dynamoTemp = new DirectoryInfo(TempFolder).Parent.FullName;
                tempPath = Path.Combine(dynamoTemp, Guid.NewGuid().ToString("N"), $"{modName}.py");
                var modfile = new System.IO.FileInfo(tempPath);
                modfile.Directory.Create(); 
                File.WriteAllText(modfile.FullName, "value ='bye'\n");

                //update script to point to new location
                script = $@"import sys
sys.path.append(r'{Path.GetDirectoryName(tempPath)}')
import {modName}
OUT = {modName}.value";
                UpdatePythonNodeContent(pythonNode, script);

                //user restarts manually, this will cause a dynamo and python reset
                this.ViewModel.Model.OnRequestPythonReset(PythonEngineManager.CPython3EngineName);

                RunCurrentModel();
                //this failure is currently expected.
                AssertPreviewValue(pythonNode.GUID.ToString(), "bye");

            }
            finally
            {
                File.Delete(tempPath);
            }
        }

        //This test creates some instances with a class defined in a loaded module
        //then calls a method on these instances - then reloads the module and runs the graph again.
        [Test]
        public void Cpython_reloaded_class_instances()
        {
           
            RunModel(@"core\python\cpython_reloaded_class_instances.dyn");
            var leafPythonNode = "27af4862d5e7446babea7ff42f5bc80c";
            AssertPreviewValue(leafPythonNode, new string[] { "initial", "initial" });

            var modulePath = Path.Combine(TestDirectory, "core", "python", "module_reload", "reloaded_class.py");

            var originalContents = File.ReadAllText(modulePath);
            try
            {
                //now we modify the module and force reload.
                var newContent =
@"class reloaded_class:
    def __init__(self):
        self.data = 'reloaded'

    def get_data(self):
        return self.data";
                File.WriteAllText(modulePath, newContent);

                this.ViewModel.Model.OnRequestPythonReset(PythonEngineManager.CPython3EngineName);
                RunCurrentModel();
                AssertPreviewValue(leafPythonNode, new string[] { "reloaded", "reloaded" });
                //after a second run - the old instance shoud have been disposed
                //only the new one should remain.
                Assert.AreEqual(1, DynamoCPythonHandle.HandleCountMap.First(x => x.ToString().Contains("reloaded_class")).Value);

            }
            finally
            {
                File.WriteAllText(modulePath, originalContents);
            }
        }

        [Test]
        public void Cpython_reloaded_class_instances_AUTO()
        {
            this.ViewModel.Model.OnRequestPythonReset(PythonEngineManager.CPython3EngineName);
            RunModel(@"core\python\cpython_reloaded_class_instances.dyn");
            var leafPythonNode = "27af4862d5e7446babea7ff42f5bc80c";
            AssertPreviewValue(leafPythonNode, new string[] { "initial", "initial" });

            var modulePath = Path.Combine(TestDirectory, "core", "python", "module_reload", "reloaded_class.py");

            var originalContents = File.ReadAllText(modulePath);
            try
            {
                //now we modify the module and force reload.
                var newContent =
@"class reloaded_class:
    def __init__(self):
        self.data = 'reloaded'

    def get_data(self):
        return self.data";
                File.WriteAllText(modulePath, newContent);

                (ViewModel.CurrentSpace as HomeWorkspaceModel).RunSettings.RunType = RunType.Automatic;
                this.ViewModel.Model.OnRequestPythonReset(PythonEngineManager.CPython3EngineName);
                
                AssertPreviewValue(leafPythonNode, new string[] { "reloaded", "reloaded" });
                //after a second run - the old instance shoud have been disposed
                //only the new one should remain.
                Assert.AreEqual(1, DynamoCPythonHandle.HandleCountMap.First(x => x.ToString().Contains("reloaded_class")).Value);

            }
            finally
            {
                File.WriteAllText(modulePath, originalContents);
            }
        }
    }
}
