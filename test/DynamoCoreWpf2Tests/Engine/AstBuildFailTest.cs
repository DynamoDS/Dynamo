using System.IO;
using Dynamo.Graph.Nodes;
using NUnit.Framework;

namespace Dynamo.Tests
{
    /// <summary>
    /// This is a WPF test because NodeWithFailingASTOutput is a node that is defined in the TestUINodes assembly
    /// </summary>
    [TestFixture]
    internal class AstBuildFailTest : DynamoModelTestBase
    {
        [Test]
        public void TestAstBuildException()
        {
            // This dyn file contains a node which will throw an exception 
            // when it is compiled to AST node. Verify the exception won't
            // crash Dynamo, and the state of node should be AstBuildBroken
            string openPath = Path.Combine(TestDirectory, @"core\astbuilder\buildAstException.dyn");
            RunModel(openPath);

            var node = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("c0e4b4ef-49f2-4bbc-9cbe-a8cc651ac17e");
            Assert.AreEqual(node.State, ElementState.AstBuildBroken);
            AssertPreviewValue("c0e4b4ef-49f2-4bbc-9cbe-a8cc651ac17e", null);

            Assert.IsTrue(node.Infos.Any(x => x.Message.Contains(Properties.Resources.NodeProblemEncountered)));
            Assert.IsTrue(node.Infos.Any(x => x.Message.Contains("Dummy error message.")));
        }
    }
}
