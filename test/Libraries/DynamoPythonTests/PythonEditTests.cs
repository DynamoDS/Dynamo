using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DSIronPythonNode;
using Dynamo;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Tests;
using Dynamo.Utilities;
using NUnit.Framework;
using DynCmd = Dynamo.ViewModels.DynamoViewModel;

namespace Dynamo.Tests
{
    public class PythonEditTests : DynamoUnitTest
    {
        [Test]
        public void PythonScriptEdit_WorkspaceChangesReflected()
        {
            // open file
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\python", "python.dyn");
            model.Open(examplePath);

            // get the python node
            var workspace = model.CurrentWorkspace;
            var nodeModel = workspace.NodeFromWorkspace("3bcad14e-d086-4278-9e08-ed2759ef92f3");
            DSIronPythonNode.PythonNode pynode = nodeModel as DSIronPythonNode.PythonNode;
            Assert.NotNull(pynode);

            // make changes to python script
            UpdatePythonNodeContent(pynode, @"print 'okay'");

            // workspace has changes
            Assert.IsTrue(model.CurrentWorkspace.HasUnsavedChanges);
        }

        [Test]
        public void PythonScriptEdit_UndoRedo()
        {
            // open file
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\python", "python.dyn");
            model.Open(examplePath);

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
            Controller.DynamoViewModel.UndoCommand.Execute(null);

            // check value is back to original
            Assert.AreEqual(pynode.Script, origScript);

            // redo change
            Controller.DynamoViewModel.RedoCommand.Execute(null);

            // script is edited
            Assert.AreEqual(pynode.Script, newScript);
        }

        [Test]
        public void VarInPythonScriptEdit_WorkspaceChangesReflected()
        {
            // open file
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\python", "varinpython.dyn");
            model.Open(examplePath);

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
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\python", "varinpython.dyn");
            model.Open(examplePath);

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
            Controller.DynamoViewModel.UndoCommand.Execute(null);

            // check value is back to original
            Assert.AreEqual(pynode.Script, origScript);

            // redo change
            Controller.DynamoViewModel.RedoCommand.Execute(null);

            // script is edited
            Assert.AreEqual(pynode.Script, newScript);
        }

        private void UpdatePythonNodeContent(ModelBase pythonNode, string value)
        {
            var command = new DynCmd.UpdateModelValueCommand(
                pythonNode.GUID, "ScriptContent", value);

            dynSettings.Controller.DynamoViewModel.ExecuteCommand(command);
        }
    }
}
