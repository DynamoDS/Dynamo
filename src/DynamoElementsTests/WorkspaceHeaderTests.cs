using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dynamo.Models;
using NUnit.Framework;

namespace Dynamo.Tests
{
    class WorkspaceHeaderTests : UnitTestBase
    {
        [Test]
        public void CanRecognizeCustomNodeWorkspace()
        {
            var examplePath = Path.Combine(GetTestDirectory(), @"core\combine", "Sequence2.dyf");
            var workspaceInfo = WorkspaceHeader.FromPath(examplePath);

            Assert.AreEqual(workspaceInfo.Name, "Sequence2");
            Assert.AreEqual(workspaceInfo.ID, "6aecda57-7679-4afb-aa02-05a75cc3433e");
            Assert.IsTrue(workspaceInfo.IsCustomNodeWorkspace());
        }

        [Test]
        public void CanRecognizeHomeWorkspace()
        {
            var examplePath = Path.Combine(GetTestDirectory(), @"core\combine", "combine-with-three.dyn");
            var workspaceInfo = WorkspaceHeader.FromPath(examplePath);

            Assert.AreEqual("Home", workspaceInfo.Name);
            Assert.IsTrue( String.IsNullOrEmpty(workspaceInfo.ID) );
            Assert.IsFalse(workspaceInfo.IsCustomNodeWorkspace());
        }
    }
}
