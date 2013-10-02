using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dynamo.Models;
using Dynamo.Tests;
using NUnit.Framework;

namespace Dynamo
{
    class CustomNodeWorkspaceOpening : DynamoUnitTest
    {

        [Test]
        public void CanOpenCustomNodeWorkspace()
        {
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\combine", "Sequence2.dyf");
            model.Open(examplePath);

            var nodeWorkspace = model.Workspaces.FirstOrDefault(x => x is FuncWorkspace);
            Assert.IsNotNull(nodeWorkspace);
            Assert.AreEqual( model.CurrentWorkspace.Name, "Sequence2");
        }
    }

}
