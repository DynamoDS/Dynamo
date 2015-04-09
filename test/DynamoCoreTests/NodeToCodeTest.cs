using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Dynamo.Nodes;
using Dynamo.DSEngine;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Tests
{
    [Category("NodeToCode")]
    class NodeToCodeTest : DynamoViewModelUnitTest
    {
        public void OpenModel(string relativeFilePath)
        {
            string openPath = Path.Combine(TestDirectory, relativeFilePath);
            ViewModel.OpenCommand.Execute(openPath);
        }

        [Test]
        public void TestVariableConfliction1()
        {
            // Test same name variables will be renamed in node to code
            //
            // Three code block:
            //  
            //    a = 1;
            // 
            //    a = 2;
            //
            //    a = 3;
            OpenModel(@"core\node2code\sameNames1.dyn");
            var nodes = ViewModel.CurrentSpaceViewModel.Model.Nodes;
            var engine = ViewModel.Model.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            // We should get 3 ast nodes, but their order is not important. 
            Assert.True(result != null && result.AstNodes != null && result.AstNodes.Count() == 3);
            Assert.True(result.AstNodes.All(n => n is BinaryExpressionNode));

            var exprs = result.AstNodes.Cast<BinaryExpressionNode>();
            Assert.True(exprs.All(e => e.LeftNode is IdentifierNode && e.RightNode is IntNode));

            var idents = exprs.Select(e => (e.LeftNode as IdentifierNode).Value);
            var vals = exprs.Select(e => (e.RightNode as IntNode).Value);

            Assert.IsTrue(new [] {"a", "a1", "a2"}.All(x => idents.Contains(x)));
            Assert.IsTrue(new [] {1, 2, 3}.All(x => vals.Contains(x)));
        }

        [Test]
        public void TestVariableConfliction2()
        {
            // Test same name variables will be renamed in node to code
            //
            // Code blocks:
            //  
            //    a = 1;
            //    a = 2;
            //
            //    a = 3;
            OpenModel(@"core\node2code\sameNames2.dyn");
            var nodes = ViewModel.CurrentSpaceViewModel.Model.Nodes;
            var engine = ViewModel.Model.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            // We should get 3 ast nodes, but their order is not important. 
            Assert.True(result != null && result.AstNodes != null && result.AstNodes.Count() == 3);
            Assert.True(result.AstNodes.All(n => n is BinaryExpressionNode));

            var exprs = String.Concat(result.AstNodes
                                            .Cast<BinaryExpressionNode>()
                                            .Select(e => e.ToString().Replace(" ", String.Empty)));

            Assert.IsTrue(((exprs.Contains("a1=1") && exprs.Contains("a1=2") && exprs.Contains("a=3"))
                || ((exprs.Contains("a=1") && exprs.Contains("a=2") && exprs.Contains("a1=3")))));
        }

        [Test]
        public void TestVariableConfliction3()
        {
            // Test same name variables will be renamed in node to code
            //
            // Code blocks
            //  
            //    a = 1;
            //    b = a + a;
            //
            //    a = 2;
            OpenModel(@"core\node2code\sameNames3.dyn");
            var nodes = ViewModel.CurrentSpaceViewModel.Model.Nodes;
            var engine = ViewModel.Model.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            Assert.True(result != null && result.AstNodes != null && result.AstNodes.Count() == 3);
            Assert.True(result.AstNodes.All(n => n is BinaryExpressionNode));

            var exprs = String.Concat(result.AstNodes
                                            .Cast<BinaryExpressionNode>()
                                            .Select(e => e.ToString().Replace(" ", String.Empty)));

            Assert.IsTrue(((exprs.Contains("a1=1") && exprs.Contains("b=a+a") && exprs.Contains("a=2"))
                || ((exprs.Contains("a1=1") && exprs.Contains("b=a1+a1") && exprs.Contains("a=2")))));
        }

        [Test]
        public void TestVariableConfliction4()
        {
            // Test same name variables will be renamed in node to code
            //
            // Code blocks
            //  
            //    a = 1;
            //    a1 = 2;
            //    a3 = a + a1;
            //
            //    a = 3;
            //    a1 = 4;
            //    a2 = a + a1;
            OpenModel(@"core\node2code\sameNames4.dyn");
            var nodes = ViewModel.CurrentSpaceViewModel.Model.Nodes;
            var engine = ViewModel.Model.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            Assert.True(result != null && result.AstNodes != null && result.AstNodes.Count() == 6);
            Assert.True(result.AstNodes.All(n => n is BinaryExpressionNode));

            var exprs = String.Concat(result.AstNodes
                                            .Cast<BinaryExpressionNode>()
                                            .Select(e => e.ToString().Replace(" ", String.Empty)));

            // It totally depends on which code block node is compiled firstly.
            // Variables in the first one won't be renamed.
            Assert.IsTrue(
                ((exprs.Contains("a=1") && exprs.Contains("a1=2") && exprs.Contains("a3=a+a1") 
                && exprs.Contains("a2=3") && exprs.Contains("a11=4") && exprs.Contains("a21=a2+a11"))

             || ((exprs.Contains("a=3") && exprs.Contains("a1=4") && exprs.Contains("a2=a+a1") 
                && exprs.Contains("a3=1") && exprs.Contains("a11=2") && exprs.Contains("a31=a3+a11")))));
        }

        [Test]
        public void TestTemporaryVariableRenaming1()
        {
            // Test temporary variables should be renamed
            //
            // Code blocks
            //  
            //    1;
            //    t1 = 2;
            //
            //    3 
            //    t2 = 4;
            OpenModel(@"core\node2code\tempVariable1.dyn");
            var nodes = ViewModel.CurrentSpaceViewModel.Model.Nodes;
            var engine = ViewModel.Model.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            Assert.True(result != null && result.AstNodes != null && result.AstNodes.Count() == 4);
            Assert.True(result.AstNodes.All(n => n is BinaryExpressionNode));

            var exprs = String.Concat(result.AstNodes
                                            .Cast<BinaryExpressionNode>()
                                            .Select(e => e.ToString().Replace(" ", String.Empty)));

            // It totally depends on which code block node is compiled firstly.
            // Variables in the first one won't be renamed.
            Assert.IsTrue(
               (exprs.Contains("t3=1") && exprs.Contains("t1=2") && exprs.Contains("t4=3") && exprs.Contains("t2=4"))
             || (exprs.Contains("t3=3") && exprs.Contains("t2=4") && exprs.Contains("t4=1") && exprs.Contains("t1=2")));
        }

        [Test]
        public void TestTemporaryVariableRenaming2()
        {
            // Test temporary variables should be renamed
            //
            // Code blocks
            //  
            //    1;
            //    2;  ---> b | a = b;
            //    3;
            //    4;
            OpenModel(@"core\node2code\tempVariable2.dyn");
            var nodes = ViewModel.CurrentSpaceViewModel.Model.Nodes;
            var engine = ViewModel.Model.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            Assert.True(result != null && result.AstNodes != null);
            Assert.True(result.AstNodes.All(n => n is BinaryExpressionNode));

            var exprs = String.Concat(result.AstNodes
                                            .Cast<BinaryExpressionNode>()
                                            .Select(e => e.ToString().Replace(" ", String.Empty)));

            // It totally depends on which code block node is compiled firstly.
            // Variables in the first one won't be renamed.
            Assert.IsTrue(exprs.Contains("b=t2") && exprs.Contains("t2=2"));
        }
    }
}
