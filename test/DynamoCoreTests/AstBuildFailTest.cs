using Dynamo.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using Dynamo.Models;
using Dynamo.DSEngine;

namespace Dynamo.Tests
{
    [TestFixture]
    internal class AstBuildFailTest: DSEvaluationViewModelUnitTest
    {
        [Test]
        public void TestAstBuildException()
        {
            // This dyn file contains a node which will throw an exception 
            // when it is compiled to AST node. Verify the exception won't
            // crash Dynamo, and the state of node should be AstBuildBroken
            var model = ViewModel.Model;
            string openPath = Path.Combine(TestDirectory, @"core\astbuilder\buildAstException.dyn");
            RunModel(openPath);

            var node = model.CurrentWorkspace.NodeFromWorkspace("c0e4b4ef-49f2-4bbc-9cbe-a8cc651ac17e");
            Assert.AreEqual(node.State, ElementState.AstBuildBroken);
            AssertPreviewValue("c0e4b4ef-49f2-4bbc-9cbe-a8cc651ac17e", null);

            var formatString = Properties.Resources.NodeProblemEncountered;
            var expectedToolTip = String.Format(formatString, "Dummy error message.");
            Assert.AreEqual(expectedToolTip, node.ToolTipText);
        }
    }
}
