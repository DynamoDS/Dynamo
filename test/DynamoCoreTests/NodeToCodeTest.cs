using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Dynamo.Nodes;
using Dynamo.DSEngine;
using ProtoCore.AST.AssociativeAST;
using Dynamo.Models;

namespace Dynamo.Tests
{
    [Category("NodeToCode")]
    class NodeToCodeTest : DynamoViewModelUnitTest
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            base.GetLibrariesToPreload(libraries);
        }

        public void OpenModel(string relativeFilePath)
        {
            string openPath = Path.Combine(TestDirectory, relativeFilePath);
            ViewModel.OpenCommand.Execute(openPath);
        }

        [Test]
        public void TestParitition1()
        {
            // Select some nodes, and return a group of nodes. Each group could
            // be converted to code.
            //
            // 1 -> + -> 2
            OpenModel(@"core\node2code\partition1.dyn");
            var nodes = ViewModel.CurrentSpaceViewModel.Model.Nodes.OfType<CodeBlockNodeModel>();
            var groups = NodeToCodeUtils.GetCliques(nodes);
            Assert.IsTrue(groups.Count == 2);
        }

        [Test]
        public void TestParitition2()
        {
            // Select some nodes, and return a group of nodes. Each group could
            // be converted to code.
            //
            // 1 -> + -> 2 -> 3
            // |              ^
            // +--------------+
            OpenModel(@"core\node2code\partition2.dyn");
            var nodes = ViewModel.CurrentSpaceViewModel.Model.Nodes.OfType<CodeBlockNodeModel>();
            var groups = NodeToCodeUtils.GetCliques(nodes);
            Assert.IsTrue(groups.Count == 2);
            foreach (var group in groups)
            {
               if (group.Count == 2)
               {
                   Assert.IsTrue(group.Find(n => n.NickName == "2") != null &&
                                 group.Find(n => n.NickName == "3") != null);
               } 
            }
        }

        [Test]
        public void TestParitition3()
        {
            // Select some nodes, and return a group of nodes. Each group could
            // be converted to code.
            //
            // 1 -> + -> 3
            // |         ^
            // +--> 2----+
            OpenModel(@"core\node2code\partition3.dyn");
            var nodes = ViewModel.CurrentSpaceViewModel.Model.Nodes.OfType<CodeBlockNodeModel>();
            var groups = NodeToCodeUtils.GetCliques(nodes);
            Assert.IsTrue(groups.Count == 2);
            var group = groups.Where(g => g.Count == 2).First();
            Assert.IsTrue(group.Find(n => n.NickName == "2") != null);
        }

        [Test]
        public void TestParitition4()
        {
            // Select some nodes, and return a group of nodes. Each group could
            // be converted to code. circular
            //
            // 1 --> x --> 2
            // ^           |
            // +-----------+
            OpenModel(@"core\node2code\partition4.dyn");
            var nodes = ViewModel.CurrentSpaceViewModel.Model.Nodes.OfType<CodeBlockNodeModel>();
            var groups = NodeToCodeUtils.GetCliques(nodes);
            Assert.IsTrue(groups.Count == 1);
            var group = groups.First();
            Assert.IsTrue(group.Find(n => n.NickName == "1") != null &&
                          group.Find(n => n.NickName == "2") != null);
        }

        [Test]
        public void TestParitition5()
        {
            // Select some nodes, and return a group of nodes. Each group could
            // be converted to code. 
            //
            // 1             4
            // 2 ----> x --> 5  
            // 3
            OpenModel(@"core\node2code\partition5.dyn");
            var nodes = ViewModel.CurrentSpaceViewModel.Model.Nodes.Where(n => n.NickName != "X");
            var groups = NodeToCodeUtils.GetCliques(nodes);
            Assert.IsTrue(groups.Count == 2);

            var group1 = groups.Where(g => g.Count == 3).FirstOrDefault();
            Assert.IsNotNull(group1);

            var group2 = groups.Where(g => g.Count == 2).FirstOrDefault();
            Assert.IsNotNull(group2);

            var nickNames = group1.Select(n => Int32.Parse(n.NickName)).ToList();
            nickNames.Sort();
            Assert.IsTrue(nickNames.SequenceEqual(new[] { 1, 2, 3 }));

            nickNames = group2.Select(n => Int32.Parse(n.NickName)).ToList();
            nickNames.Sort();
            Assert.IsTrue(nickNames.SequenceEqual(new[] { 4, 5 }));
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
        public void TestVariableConfliction5()
        {
            // Test same name variables will be renamed in node to code
            //
            // Code blocks
            //  
            //   x = 1; --> a[x][x] = 2;
            OpenModel(@"core\node2code\sameName5.dyn");
            var nodes = ViewModel.CurrentSpaceViewModel.Model.Nodes;
            var engine = ViewModel.Model.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            Assert.True(result != null && result.AstNodes != null && result.AstNodes.Count() == 3);
            Assert.True(result.AstNodes.All(n => n is BinaryExpressionNode));

            var exprs = String.Concat(result.AstNodes
                                            .Cast<BinaryExpressionNode>()
                                            .Select(e => e.ToString().Replace(" ", String.Empty)));

            // It totally depends on which code block node is compiled firstly.
            // Variables in the first one won't be renamed.
            Assert.IsTrue(exprs.Contains("x=1") && exprs.Contains("x1=x") && exprs.Contains("a[x1][x1]=2"));
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

        [Test]
        public void TestTemporaryVariableRenaming3()
        {
            // 1 x 
            // 2 y
            OpenModel(@"core\node2code\tempVariable3.dyn");
            var nodes = ViewModel.CurrentSpaceViewModel.Model.Nodes;
            var engine = ViewModel.Model.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            result = NodeToCodeUtils.ConstantPropagationForTemp(result, Enumerable.Empty<string>());
            Assert.True(result != null && result.AstNodes != null && result.AstNodes.Count() == 4);
            Assert.True(result.AstNodes.All(n => n is BinaryExpressionNode));
        }

        [Test]
        public void TestUnqualifiedNameReplacer1()
        {
            var functionCall = AstFactory.BuildFunctionCall(
                "Autodesk.DesignScript.Geometry.Point", 
                "ByCoordinates", 
                new List<AssociativeNode> { new IntNode(1), new IntNode(2)});
            var lhs = AstFactory.BuildIdentifier("lhs");
            var ast = AstFactory.BuildBinaryExpression(lhs, functionCall, ProtoCore.DSASM.Operator.assign);

            NodeToCodeUtils.ReplaceWithUnqualifiedName(
                ViewModel.Model.EngineController.LibraryServices.LibraryManagementCore, 
                new [] { ast });

            Assert.IsTrue(ast.RightNode.ToString().Equals("Point.ByCoordinates(1, 2)"));
        }

        [Test]
        public void TestUnqualifiedNameReplacer2()
        {
            // Point.ByCoordinates(1,2); 
            OpenModel(@"core\node2code\unqualifiedName1.dyn");
            var nodes = ViewModel.CurrentSpaceViewModel.Model.Nodes;
            var engine = ViewModel.Model.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            result = NodeToCodeUtils.ConstantPropagationForTemp(result, Enumerable.Empty<string>());
            NodeToCodeUtils.ReplaceWithUnqualifiedName(engine.LibraryServices.LibraryManagementCore, result.AstNodes);
            Assert.True(result != null && result.AstNodes != null);

            var expr = result.AstNodes.Last() as BinaryExpressionNode;
            Assert.IsTrue(expr != null);

            Assert.IsTrue(expr.RightNode.ToString().Equals("Point.ByCoordinates(1, 2)"));
        }

        [Test]
        public void TestUnqualifiedNameReplacer3()
        {
            // 1 -> Point.ByCoordinates(x, y); 
            OpenModel(@"core\node2code\unqualifiedName2.dyn");
            var nodes = ViewModel.CurrentSpaceViewModel.Model.Nodes;
            var engine = ViewModel.Model.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            result = NodeToCodeUtils.ConstantPropagationForTemp(result, Enumerable.Empty<string>());
            NodeToCodeUtils.ReplaceWithUnqualifiedName(engine.LibraryServices.LibraryManagementCore, result.AstNodes);
            Assert.True(result != null && result.AstNodes != null);

            var expr = result.AstNodes.Last() as BinaryExpressionNode;
            Assert.IsTrue(expr != null);

            Assert.IsTrue(expr.RightNode.ToString().Equals("Point.ByCoordinates(x, x)"));
        }

        [Test]
        public void TestUnqualifiedNameReplacer4()
        {
            // 1 -> Autodesk.DesignScript.Geometry.Point.ByCoordinates(x, x); 
            OpenModel(@"core\node2code\unqualifiedName3.dyn");
            var nodes = ViewModel.CurrentSpaceViewModel.Model.Nodes;
            var engine = ViewModel.Model.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            result = NodeToCodeUtils.ConstantPropagationForTemp(result, Enumerable.Empty<string>());
            NodeToCodeUtils.ReplaceWithUnqualifiedName(engine.LibraryServices.LibraryManagementCore, result.AstNodes);
            Assert.True(result != null && result.AstNodes != null);

            var expr = result.AstNodes.Last() as BinaryExpressionNode;
            Assert.IsTrue(expr != null);

            Assert.IsTrue(expr.RightNode.ToString().Equals("Point.ByCoordinates(x, x)"));
        }

        [Test]
        public void TestUnqualifiedNameReplacer5()
        {
            // 1 -> Autodesk.DesignScript.Geometry.Point.ByCoordinates(x, x); 
            OpenModel(@"core\node2code\unqualifiedName4.dyn");
            var nodes = ViewModel.CurrentSpaceViewModel.Model.Nodes;
            var engine = ViewModel.Model.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            result = NodeToCodeUtils.ConstantPropagationForTemp(result, Enumerable.Empty<string>());
            NodeToCodeUtils.ReplaceWithUnqualifiedName(engine.LibraryServices.LibraryManagementCore, result.AstNodes);
            Assert.True(result != null && result.AstNodes != null);

            var expr1 = result.AstNodes.First() as BinaryExpressionNode;
            var expr2 = result.AstNodes.Last() as BinaryExpressionNode;

            Assert.IsNotNull(expr1);
            Assert.IsNotNull(expr2);

            Assert.IsTrue(expr1.RightNode.ToString().Equals("Point.ByCoordinates(0, 0)"));
            Assert.IsTrue(expr2.RightNode.ToString().Equals("Point.ByCoordinates(0, 0)"));
        }

        [Test]
        public void TestUnqualifiedNameReplacer6()
        {
            OpenModel(@"core\node2code\unqualifiedName5.dyn");
            var nodes = ViewModel.CurrentSpaceViewModel.Model.Nodes;
            var engine = ViewModel.Model.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            result = NodeToCodeUtils.ConstantPropagationForTemp(result, Enumerable.Empty<string>());
            NodeToCodeUtils.ReplaceWithUnqualifiedName(engine.LibraryServices.LibraryManagementCore, result.AstNodes);
            Assert.True(result != null && result.AstNodes != null);

            var expr = result.AstNodes.Last() as BinaryExpressionNode;

            Assert.IsNotNull(expr);
            Assert.IsTrue(expr.RightNode.ToString().Equals("t1.DistanceTo(t2)"));
        }

        [Test]
        public void TestBasicNode2CodeWorkFlow1()
        {
            // 1 -> a -> x
            OpenModel(@"core\node2code\workflow1.dyn");
            var nodes = ViewModel.CurrentSpaceViewModel.Model.Nodes;
            var engine = ViewModel.Model.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            result = NodeToCodeUtils.ConstantPropagationForTemp(result, Enumerable.Empty<string>());
            Assert.IsTrue(result != null && result.AstNodes != null);

            var expr1 = result.AstNodes.First() as BinaryExpressionNode;
            var expr2 = result.AstNodes.Last() as BinaryExpressionNode;

            Assert.IsNotNull(expr1);
            Assert.IsNotNull(expr2);

            Assert.IsTrue(expr1.ToString().StartsWith("a = 1;"));
            Assert.IsTrue(expr2.ToString().StartsWith("x = a;"));
        }

        [Test]
        public void TestBasicNode2CodeWorkFlow2()
        {
            // 1 -> Point.ByCoordinates ---+
            //                             |---------------------------+ 
            //                             +-- List.Create (index0)    |-> Line.ByStartPointEndPoint
            //                             +--> List.Create (index1)   |
            //                             |---------------------------+ 
            // 2 -> Point.ByCoordinates ---+
            OpenModel(@"core\node2code\workflow2.dyn");
            var nodes = ViewModel.CurrentSpaceViewModel.Model.Nodes;
            var engine = ViewModel.Model.EngineController;

            ViewModel.SelectAll(null);
            var command = new DynamoModel.ConvertNodesToCodeCommand();
            ViewModel.ExecuteCommand(command);

            Assert.IsTrue(ViewModel.CurrentSpace.Connectors.Count() == 2);
            Assert.IsTrue(ViewModel.CurrentSpace.Nodes.Count == 2);

            var undo = new DynamoModel.UndoRedoCommand(DynamoModel.UndoRedoCommand.Operation.Undo);
            ViewModel.ExecuteCommand(undo);

            Assert.IsTrue(ViewModel.CurrentSpace.Connectors.Count() == 6);
            Assert.IsTrue(ViewModel.CurrentSpace.Nodes.Count == 6);
        }

        [Test]
        public void TestBasicNode2CodeWorkFlow3()
        {
            OpenModel(@"core\node2code\workflow3.dyn");
            var nodes = ViewModel.CurrentSpaceViewModel.Model.Nodes;
            var engine = ViewModel.Model.EngineController;

            ViewModel.SelectAll(null);
            var command = new DynamoModel.ConvertNodesToCodeCommand();
            ViewModel.ExecuteCommand(command);

            Assert.IsTrue(ViewModel.CurrentSpace.Connectors.Count() == 3);
            Assert.IsTrue(ViewModel.CurrentSpace.Nodes.Count == 3);
        }

        [Test]
        public void TestBasicNode2CodeWorkFlow4()
        {
            OpenModel(@"core\node2code\workflow4.dyn");
            var nodes = ViewModel.CurrentSpaceViewModel.Model.Nodes;
            var engine = ViewModel.Model.EngineController;

            ViewModel.SelectAll(null);
            var command = new DynamoModel.ConvertNodesToCodeCommand();
            ViewModel.ExecuteCommand(command);

            Assert.IsTrue(ViewModel.CurrentSpace.Connectors.Count() == 3);
            Assert.IsTrue(ViewModel.CurrentSpace.Nodes.Count == 3);
        }

        [Test]
        public void TestShortName1()
        {
            OpenModel(@"core\node2code\shortName1.dyn");
            var nodes = ViewModel.CurrentSpaceViewModel.Model.Nodes;
            var engine = ViewModel.Model.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            result = NodeToCodeUtils.ConstantPropagationForTemp(result, Enumerable.Empty<string>());
            NodeToCodeUtils.ReplaceWithUnqualifiedName(engine.LibraryServices.LibraryManagementCore, result.AstNodes);
            Assert.IsTrue(result != null && result.AstNodes != null);

            var expr = result.AstNodes.Last() as BinaryExpressionNode;

            Assert.IsNotNull(expr);
            Assert.IsTrue(expr.RightNode.ToString().Equals("Point.ByCoordinates(t1, 0)"));
        }
    }
}
