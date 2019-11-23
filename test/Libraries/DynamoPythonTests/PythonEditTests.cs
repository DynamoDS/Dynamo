using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using NUnit.Framework;
using PythonNodeModels;
using Dynamo.Graph.Nodes.CustomNodes;
using DynCmd = Dynamo.Models.DynamoModel;

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
            base.GetLibrariesToPreload(libraries);
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
                "845d532fdf874d939f2ed66509413ea6",
                "cb037a9debd54ce79a4007b6ea11de25",
                "a9bb1b12fbbd4aa19299f0d30c9f99b2",
                "b6bd3049034f488a9bed0373f05fd021"
            };

            // get test nodes
            var allNodes = model.CurrentWorkspace.Nodes;

            foreach(NodeModel node in allNodes) {
                var guid = node.GUID.ToString();

                // if node is a test node, verify truth value
                if (testingNodeGUIDS.Contains(guid) ) {
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

            AssertPreviewValue(guid, new Dictionary<string, int> { { "abc", 123 }, { "def", 345 } });
        }

        [Test]
        public void InputDynamoDictionary_AsPythonDictionary()
        {
            // open test graph
            var examplePath = Path.Combine(TestDirectory, @"core\python", "python_dict2.dyn");
            ViewModel.OpenCommand.Execute(examplePath);

            var guid = "490a8d54d0fa4782ae18c81f6eef8306";

            AssertPreviewValue(guid,
                new List<object> {new Dictionary<string, int> {{"abcd", 123}}, new List<int> {1, 2, 3, 4, 5, 6, 7, 8, 9}});
        }

        [Test]
        public void ReturnIronPythonDictionary_AsDynamoDictionary()
        {
            // open test graph
            var examplePath = Path.Combine(TestDirectory, @"core\python", "netDict_from_python.dyn");
            ViewModel.OpenCommand.Execute(examplePath);

            var guid = "490a8d54d0fa4782ae18c81f6eef8306";

            AssertPreviewValue(guid, new Dictionary<string, int> {{"abc", 123}, {"def", 10}});
        }

        private void UpdatePythonNodeContent(ModelBase pythonNode, string value)
        {
            var command = new DynCmd.UpdateModelValueCommand(
                System.Guid.Empty, pythonNode.GUID, "ScriptContent", value);

            ViewModel.ExecuteCommand(command);
        }

        [Test]
        public void BigInteger_CanBeMarshaledAsInt64()
        {
            // open test graph
            var examplePath = Path.Combine(TestDirectory, @"core\python", "BigIntegerToLong.dyn");
            ViewModel.OpenCommand.Execute(examplePath);

            var guid = "23088248d7b1441abbc5ada07fcdf154";

            AssertPreviewValue(guid,
                new[] {"System.Int64", "System.Double", "System.Int64", "System.Int64", "System.Numerics.BigInteger"});
        }
    }
}
