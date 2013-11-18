using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dynamo;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Tests;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo.Tests
{
    public class PythonEditTests : DynamoUnitTest
    {
        [Test]
        public void EditScriptSetsWorkspaceAsChanged()
        {
            // open file
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\python", "singlenode.dyn");
            model.Open(examplePath);

            // get the python node
            var pynode = model.CurrentWorkspace.Nodes.OfType<Python>().First();
            Assert.NotNull(pynode);

            // save original script
            var origScript = pynode.Script;

            // make changes to python script
            pynode.Script = @"print 'okay'";

            // workspace has changes
            Assert.IsTrue(model.CurrentWorkspace.HasUnsavedChanges);

            // undo change
            Controller.DynamoViewModel.UndoCommand.Execute(null);

            // check value script is restored
            Assert.AreEqual(pynode.Script, origScript);
        }

        [Test]
        public void EditScriptCanBeUndone()
        {
            // open file
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\python", "singlenode.dyn");
            model.Open(examplePath);

            // get the python node
            var pynode = model.CurrentWorkspace.Nodes.OfType<Python>().First();
            Assert.NotNull(pynode);

            // save original script
            var origScript = pynode.Script;

            // make changes to python script
            var newScript = @"print 'okay'";
            pynode.Script = newScript;

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

    }
}
