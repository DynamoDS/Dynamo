using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.CustomNodes;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using PythonNodeModels;
using DynCmd = Dynamo.Models.DynamoModel;
using Newtonsoft.Json.Linq;

namespace Dynamo.Tests
{
    [RequiresSTA]
    public class PythonEditTests : DynamoViewModelUnitTest
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("DSIronPython.dll");
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DSCPython.dll");
            base.GetLibrariesToPreload(libraries);
        }

        /// <summary>
        ///    Returns a list of python engines from the PythonEngineVersion Enum. 
        /// </summary>
        private IEnumerable<PythonEngineVersion> GetPythonEnginesList()
        {
            return Enum.GetValues(typeof(PythonEngineVersion)).Cast<PythonEngineVersion>();
        }

        /// <summary>
        ///    Updates Engine property for a single python node. 
        /// </summary>
        private void UpdateEnginePropertyForPythonNode(PythonNode pythonNode, PythonEngineVersion pythonEngineVersion)
        {
            pythonNode.Engine = pythonEngineVersion;
        }

        /// <summary>
        ///    Updates Engine property for a list of python nodes. 
        /// </summary>
        private void UpdateEnginePropertyForAllPythonNodes(List<PythonNode> list, PythonEngineVersion pythonEngineVersion)
        {
            foreach (var pyNode in list)
            { 
                pyNode.Engine = pythonEngineVersion;
            }
        }

        private void UpdatePythonNodeContent(ModelBase pythonNode, string value)
        {
            var command = new DynCmd.UpdateModelValueCommand(
                System.Guid.Empty, pythonNode.GUID, "ScriptContent", value);

            ViewModel.ExecuteCommand(command);
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
            Assert.AreEqual(pynode.Engine, PythonEngineVersion.IronPython2);

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
            Assert.IsTrue(pythonTokens.Any(t => t.Value<string>("Engine") == PythonEngineVersion.IronPython2.ToString()));
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
        public void VerifyPythonLoadsFromCore()
        {
            // This test graphs verifies the following:
            // 1 - IronPython version 2.7.9 is loaded
            // 2 - IronPython StdLib 2.7.9 is loaded from Core location
            // 3 - StdLib modules are loaded
            // 4 - Legacy import statements are not influenced by 2.7.9 upgrade
            
            // open test graph
            var model = ViewModel.Model;
            var examplePath = Path.Combine(TestDirectory, @"core\python", "IronPythonInfo_TestGraph.dyn");
            ViewModel.OpenCommand.Execute(examplePath);

            // reference to specific testing nodes in test graph
            string[] testingNodeGUIDS = new string[]
            {
                "845d532f-df87-4d93-9f2e-d66509413ea6",
                "cb037a9d-ebd5-4ce7-9a40-07b6ea11de25",
                "a9bb1b12-fbbd-4aa1-9299-f0d30c9f99b2",
                "b6bd3049-034f-488a-9bed-0373f05fd021"
            };

            // get test nodes
            var allNodes = model.CurrentWorkspace.Nodes;

            foreach(NodeModel node in allNodes) {
                var guid = node.GUID.ToString();

                // if node is a test node, verify truth value
                if (testingNodeGUIDS.Contains(guid) )
                {
                    AssertPreviewValue(guid, true);
                }
            }

            var pynode = model.CurrentWorkspace.Nodes.OfType<PythonNode>().First();
            Assert.NotNull(pynode);
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

            foreach (var pythonEngine in GetPythonEnginesList())
            {
                UpdateEnginePropertyForPythonNode(pynode, pythonEngine);

                ViewModel.HomeSpace.Run();

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

            foreach (var pythonEngine in GetPythonEnginesList())
            {
                UpdateEnginePropertyForPythonNode(pynode, pythonEngine);

                ViewModel.HomeSpace.Run();

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

            foreach (var pythonEngine in GetPythonEnginesList())
            {
                UpdateEnginePropertyForPythonNode(pynode, pythonEngine);

                ViewModel.HomeSpace.Run();

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

            foreach (var pythonEngine in GetPythonEnginesList())
            {
                UpdateEnginePropertyForPythonNode(pynode, pythonEngine);

                ViewModel.HomeSpace.Run();

                var nodeValue = GetPreviewValue(pythonGUID);

                if (pythonEngine == PythonEngineVersion.IronPython2) {
                    Assert.AreEqual(nodeValue, "2.7.9");
                }
                else if (pythonEngine == PythonEngineVersion.CPython3){
                    Assert.AreEqual(nodeValue, "3.7.3");
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

            foreach (var pythonEngine in GetPythonEnginesList())
            {
                UpdateEnginePropertyForPythonNode(pynode, pythonEngine);

                ViewModel.HomeSpace.Run();

                AssertPreviewValue(guid, new Dictionary<string, int> { { "abc", 123 }, { "def", 10 } });
            }
        }

        [Test]
        [Category("Failure")]
        // This test is failing for CPython3 Engine. 
        // The BigIntegers which use more than int64 type, are currently not being marshalled. 
        public void BigInteger_CanBeMarshaledAsInt64()
        {
            // open test graph
            var examplePath = Path.Combine(TestDirectory, @"core\python", "BigIntegerToLong.dyn");
            ViewModel.OpenCommand.Execute(examplePath);

            var guid = "23088248d7b1441abbc5ada07fcdf154";
            var pythonGUID = "547d1dd9203746bbaa2b7bd448c8124a";

            var nodeModel = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace(pythonGUID);
            var pynode = nodeModel as PythonNode;

            foreach (var pythonEngine in GetPythonEnginesList())
            {
                UpdateEnginePropertyForPythonNode(pynode, pythonEngine);
                 
                ViewModel.HomeSpace.Run();

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

            var pythonNode1GUID = "d060e68f510f43fe8990c2c1ba7e0f80";
            var pythonNode2GUID = "4050d23e529c43e9b6140506d8adb06b";

            var nodeModels = ViewModel.Model.CurrentWorkspace.Nodes.Where(n => n.NodeType == "PythonScriptNode");
            List<PythonNode> pythonNodes = nodeModels.Cast<PythonNode>().ToList();

            var pynode1 = pythonNodes.ElementAt(0);
            var pynode2 = pythonNodes.ElementAt(1);

            AssertPreviewValue(pythonNode2GUID, new List<String> { "2.7.9", "2.7.9"});

            UpdateEnginePropertyForPythonNode(pynode1, PythonEngineVersion.CPython3);
            Assert.IsTrue(ViewModel.Model.CurrentWorkspace.HasUnsavedChanges);
            AssertPreviewValue(pythonNode2GUID, new List<String> { "3.7.3", "2.7.9" });

            UpdateEnginePropertyForPythonNode(pynode2, PythonEngineVersion.CPython3);
            Assert.IsTrue(ViewModel.Model.CurrentWorkspace.HasUnsavedChanges);
            AssertPreviewValue(pythonNode2GUID, new List<String> { "3.7.3", "3.7.3" });

            UpdateEnginePropertyForAllPythonNodes(pythonNodes, PythonEngineVersion.IronPython2);
            Assert.IsTrue(ViewModel.Model.CurrentWorkspace.HasUnsavedChanges);
            AssertPreviewValue(pythonNode2GUID, new List<String> { "2.7.9", "2.7.9" });
        }
    }
}
