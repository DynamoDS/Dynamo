using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Dynamo.Engine;
using Dynamo.Engine.NodeToCode;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Models;
using Dynamo.Scheduler;
using Dynamo.Selection;
using NUnit.Framework;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Tests
{
    [Category("NodeToCode")]
    class NodeToCodeTest : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void TestParitition1()
        {
            // Select some nodes, and return a group of nodes. Each group could
            // be converted to code.
            //
            // 1 -> + -> 2
            OpenModel(@"core\node2code\partition1.dyn");
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<CodeBlockNodeModel>();
            var groups = NodeToCodeCompiler.GetCliques(nodes);
            Assert.AreEqual(2, groups.Count);
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
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<CodeBlockNodeModel>();
            var groups = NodeToCodeCompiler.GetCliques(nodes);
            Assert.IsTrue(groups.Count == 2);
            foreach (var group in groups)
            {
                if (group.Count == 2)
                {
                    Assert.IsNotNull(group.Find(n => n.Name == "2"));
                    Assert.IsNotNull(group.Find(n => n.Name == "3"));
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
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<CodeBlockNodeModel>();
            var groups = NodeToCodeCompiler.GetCliques(nodes);
            Assert.AreEqual(2, groups.Count);
            var group = groups.Where(g => g.Count == 2).First();
            Assert.IsNotNull(group.Find(n => n.Name == "2"));
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
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<CodeBlockNodeModel>();
            var groups = NodeToCodeCompiler.GetCliques(nodes);
            Assert.AreEqual(1, groups.Count);
            var group = groups.First();
            Assert.IsNotNull(group.Find(n => n.Name == "1"));
            Assert.IsNotNull(group.Find(n => n.Name == "2"));
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
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes.Where(n => n.Name != "X");
            var groups = NodeToCodeCompiler.GetCliques(nodes);
            Assert.AreEqual(2, groups.Count);

            var group1 = groups.Where(g => g.Count == 3).FirstOrDefault();
            Assert.IsNotNull(group1);

            var group2 = groups.Where(g => g.Count == 2).FirstOrDefault();
            Assert.IsNotNull(group2);

            var names = group1.Select(n => Int32.Parse(n.Name)).ToList();
            names.Sort();
            Assert.IsTrue(names.SequenceEqual(new[] { 1, 2, 3 }));

            names = group2.Select(n => Int32.Parse(n.Name)).ToList();
            names.Sort();
            Assert.IsTrue(names.SequenceEqual(new[] { 4, 5 }));
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
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            var engine = CurrentDynamoModel.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            // We should get 3 ast nodes, but their order is not important. 
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.AstNodes);
            Assert.AreEqual(3, result.AstNodes.Count());
            Assert.True(result.AstNodes.All(n => n is BinaryExpressionNode));

            var exprs = result.AstNodes.Cast<BinaryExpressionNode>();
            Assert.True(exprs.All(e => e.LeftNode is IdentifierNode && e.RightNode is IntNode));

            var idents = exprs.Select(e => (e.LeftNode as IdentifierNode).Value);
            var vals = exprs.Select(e => (e.RightNode as IntNode).Value);

            Assert.IsTrue(new[] { "a", "a1", "a2" }.All(x => idents.Contains(x)));
            Assert.IsTrue(new[] { 1, 2, 3 }.All(x => vals.Contains(x)));
        }

        [Test]
        public void TestVariableConfliction2()
        {
            // Test same name variables will be renamed in node to code
            //
            // Code blocks:
            //  
            //    a = 1;
            //
            //    a = 2;
            OpenModel(@"core\node2code\sameNames2.dyn");
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            var engine = CurrentDynamoModel.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            // We should get 2 ast nodes, but their order is not important. 
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.AstNodes);
            Assert.AreEqual(2, result.AstNodes.Count());
            Assert.True(result.AstNodes.All(n => n is BinaryExpressionNode));

            var exprs = String.Concat(result.AstNodes
                                            .Cast<BinaryExpressionNode>()
                                            .Select(e => e.ToString().Replace(" ", String.Empty)));

            Assert.IsTrue(((exprs.Contains("a1=1") && exprs.Contains("a=2"))
                || ((exprs.Contains("a=1") && exprs.Contains("a1=2")))));
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
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            var engine = CurrentDynamoModel.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.AstNodes);
            Assert.AreEqual(3, result.AstNodes.Count());
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
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            var engine = CurrentDynamoModel.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.AstNodes);
            Assert.AreEqual(6, result.AstNodes.Count());
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
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            var engine = CurrentDynamoModel.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.AstNodes);
            Assert.AreEqual(3, result.AstNodes.Count());
            Assert.True(result.AstNodes.All(n => n is BinaryExpressionNode));

            var exprs = String.Concat(result.AstNodes
                                            .Cast<BinaryExpressionNode>()
                                            .Select(e => e.ToString().Replace(" ", String.Empty)));

            // It totally depends on which code block node is compiled firstly.
            // Variables in the first one won't be renamed.
            Assert.IsTrue(exprs.Contains("x=1") && exprs.Contains("x1=x") && exprs.Contains("a[x1][x1]=2"));
        }

        [Test]
        public void TestVariableConfliction6()
        {
            // Test same name variables will be renamed in node to code
            //
            //   x = 1; --> Point.ByCoordinate()
            //          --> Point.ByCoordinate()
            OpenModel(@"core\node2code\sameNames5.dyn");
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            var engine = CurrentDynamoModel.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            NodeToCodeCompiler.ReplaceWithShortestQualifiedName(engine.LibraryServices.LibraryManagementCore.ClassTable, result.AstNodes);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.AstNodes);
            Assert.AreEqual(3, result.AstNodes.Count());
            Assert.True(result.AstNodes.All(n => n is BinaryExpressionNode));

            var rhs = result.AstNodes.Cast<BinaryExpressionNode>()
                                     .Skip(1)
                                     .Select(n => n.RightNode.ToString())
                                     .ToList();

            Assert.AreEqual("Point.ByCoordinates(x, 0)", rhs[0]);
            Assert.AreEqual("Point.ByCoordinates(x, 0)", rhs[1]);
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
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            var engine = CurrentDynamoModel.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.AstNodes);
            Assert.AreEqual(4, result.AstNodes.Count());
            Assert.True(result.AstNodes.All(n => n is BinaryExpressionNode));

            var lhs = result.AstNodes.Cast<BinaryExpressionNode>()
                                     .Select(expr => expr.LeftNode as IdentifierNode);

            // It totally depends on which code block node is compiled firstly.
            // Variables in the first one won't be renamed.
            Assert.IsNotNull(lhs.FirstOrDefault(x => x.Value.Equals("t1")));
            Assert.IsNotNull(lhs.FirstOrDefault(x => x.Value.Equals("t2")));
            Assert.IsNotNull(lhs.FirstOrDefault(x => x.Value.Equals("t3")));
            Assert.IsNotNull(lhs.FirstOrDefault(x => x.Value.Equals("t4")));
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
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            var engine = CurrentDynamoModel.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.AstNodes);
            Assert.True(result.AstNodes.All(n => n is BinaryExpressionNode));

            var lhs = result.AstNodes.Cast<BinaryExpressionNode>()
                                     .Select(expr => expr.LeftNode as IdentifierNode);

            // It totally depends on which code block node is compiled firstly.
            // Variables in the first one won't be renamed.
            Assert.IsNotNull(lhs.FirstOrDefault(x => x.Value.Equals("b")));
            Assert.IsNotNull(lhs.FirstOrDefault(x => x.Value.Equals("a")));
            Assert.IsNotNull(lhs.FirstOrDefault(x => x.Value.Equals("t1")));
            Assert.IsNotNull(lhs.FirstOrDefault(x => x.Value.Equals("t2")));
            Assert.IsNotNull(lhs.FirstOrDefault(x => x.Value.Equals("t3")));
        }

        [Test]
        public void TestTemporaryVariableRenaming3()
        {
            // 1 x 
            // 2 y
            OpenModel(@"core\node2code\tempVariable3.dyn");
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            var engine = CurrentDynamoModel.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            result = NodeToCodeCompiler.ConstantPropagationForTemp(result, Enumerable.Empty<string>());
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.AstNodes);
            Assert.AreEqual(4, result.AstNodes.Count());
            Assert.True(result.AstNodes.All(n => n is BinaryExpressionNode));
        }

        [Test]
        public void TestTemporaryVariableRenaming4()
        {
            OpenModel(@"core\node2code\tempVariable4.dyn");
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            var engine = CurrentDynamoModel.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            result = NodeToCodeCompiler.ConstantPropagationForTemp(result, Enumerable.Empty<string>());
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.AstNodes);

            NodeToCodeCompiler.ReplaceWithShortestQualifiedName(engine.LibraryServices.LibraryManagementCore.ClassTable, result.AstNodes);
            Assert.AreEqual(2, result.AstNodes.Count());
            Assert.True(result.AstNodes.All(n => n is BinaryExpressionNode));
            var rhs = result.AstNodes.Cast<BinaryExpressionNode>().Select(n => n.RightNode.ToString());

            Assert.AreEqual("Point.ByCoordinates(1, 2)", rhs.First());
            Assert.AreEqual("Point.ByCoordinates(1, 3)", rhs.Last());
        }

        [Test]
        public void TestShortestQualifiedNameReplacer1()
        {
            string libraryPath = "FFITarget.dll";
            if (!CurrentDynamoModel.EngineController.LibraryServices.IsLibraryLoaded(libraryPath))
            {
                CurrentDynamoModel.EngineController.ImportLibrary(libraryPath);
            }

            var functionCall = AstFactory.BuildFunctionCall(
                "Autodesk.DesignScript.Geometry.Point",
                "ByCoordinates",
                new List<AssociativeNode> { new IntNode(1), new IntNode(2) });
            var lhs = AstFactory.BuildIdentifier("lhs");
            var ast = AstFactory.BuildBinaryExpression(lhs, functionCall, ProtoCore.DSASM.Operator.assign);

            NodeToCodeCompiler.ReplaceWithShortestQualifiedName(
                CurrentDynamoModel.EngineController.LibraryServices.LibraryManagementCore.ClassTable,
                new[] { ast });

            // Since there is a conflict with FFITarget.DesignScript.Point and FFITarget.Dynamo.Point,
            // node to code generates the shortest unique name, which in this case will be
            // Autodesk.Point for Autodesk.DesignScript.Geometry.Point
            Assert.AreEqual("Autodesk.Point.ByCoordinates(1, 2)", ast.RightNode.ToString());
        }

        [Test]
        public void TestShortestQualifiedNameReplacer2()
        {
            string libraryPath = "FFITarget.dll";
            if (!CurrentDynamoModel.EngineController.LibraryServices.IsLibraryLoaded(libraryPath))
            {
                CurrentDynamoModel.EngineController.LibraryServices.ImportLibrary(libraryPath);
            }

            // Point.ByCoordinates(1,2); 
            OpenModel(@"core\node2code\unqualifiedName1.dyn");
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            var engine = CurrentDynamoModel.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            result = NodeToCodeCompiler.ConstantPropagationForTemp(result, Enumerable.Empty<string>());
            NodeToCodeCompiler.ReplaceWithShortestQualifiedName(engine.LibraryServices.LibraryManagementCore.ClassTable, result.AstNodes);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.AstNodes);

            var expr = result.AstNodes.Last() as BinaryExpressionNode;
            Assert.IsNotNull(expr);

            // Since there is a conflict with FFITarget.DesignScript.Point and FFITarget.Dynamo.Point,
            // node to code generates the shortest unique name, which in this case will be
            // Autodesk.Point for Autodesk.DesignScript.Geometry.Point
            Assert.AreEqual("Autodesk.Point.ByCoordinates(1, 2)", expr.RightNode.ToString());
        }

        [Test]
        public void TestShortestQualifiedNameReplacer3()
        {
            string libraryPath = "FFITarget.dll";
            if (!CurrentDynamoModel.EngineController.LibraryServices.IsLibraryLoaded(libraryPath))
            {
                CurrentDynamoModel.EngineController.LibraryServices.ImportLibrary(libraryPath);
            }

            // 1 -> Point.ByCoordinates(x, y); 
            OpenModel(@"core\node2code\unqualifiedName2.dyn");
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            var engine = CurrentDynamoModel.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            result = NodeToCodeCompiler.ConstantPropagationForTemp(result, Enumerable.Empty<string>());
            NodeToCodeCompiler.ReplaceWithShortestQualifiedName(engine.LibraryServices.LibraryManagementCore.ClassTable, result.AstNodes);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.AstNodes); ;

            var expr = result.AstNodes.Last() as BinaryExpressionNode;
            Assert.IsNotNull(expr);

            // Since there is a conflict with FFITarget.DesignScript.Point and FFITarget.Dynamo.Point,
            // node to code generates the shortest unique name, which in this case will be
            // Autodesk.Point for Autodesk.DesignScript.Geometry.Point
            Assert.AreEqual("Autodesk.Point.ByCoordinates(x, x)", expr.RightNode.ToString());
        }

        [Test]
        public void TestShortestQualifiedNameReplacer4()
        {
            string libraryPath = "FFITarget.dll";
            if (!CurrentDynamoModel.EngineController.LibraryServices.IsLibraryLoaded(libraryPath))
            {
                CurrentDynamoModel.EngineController.LibraryServices.ImportLibrary(libraryPath);
            }

            // 1 -> Autodesk.DesignScript.Geometry.Point.ByCoordinates(x, x); 
            OpenModel(@"core\node2code\unqualifiedName3.dyn");
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            var engine = CurrentDynamoModel.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            result = NodeToCodeCompiler.ConstantPropagationForTemp(result, Enumerable.Empty<string>());
            NodeToCodeCompiler.ReplaceWithShortestQualifiedName(engine.LibraryServices.LibraryManagementCore.ClassTable, result.AstNodes);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.AstNodes);

            var expr = result.AstNodes.Last() as BinaryExpressionNode;
            Assert.IsNotNull(expr);

            // Since there is a conflict with FFITarget.DesignScript.Point and FFITarget.Dynamo.Point,
            // node to code generates the shortest unique name, which in this case will be
            // Autodesk.Point for Autodesk.DesignScript.Geometry.Point
            Assert.AreEqual("Autodesk.Point.ByCoordinates(x, x)", expr.RightNode.ToString());
        }

        [Test]
        public void TestShortestQualifiedNameReplacer5()
        {
            string libraryPath = "FFITarget.dll";
            if (!CurrentDynamoModel.EngineController.LibraryServices.IsLibraryLoaded(libraryPath))
            {
                CurrentDynamoModel.EngineController.LibraryServices.ImportLibrary(libraryPath);
            }

            // 1 -> Autodesk.DesignScript.Geometry.Point.ByCoordinates(x, x); 
            OpenModel(@"core\node2code\unqualifiedName4.dyn");
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            var engine = CurrentDynamoModel.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            result = NodeToCodeCompiler.ConstantPropagationForTemp(result, Enumerable.Empty<string>());
            NodeToCodeCompiler.ReplaceWithShortestQualifiedName(engine.LibraryServices.LibraryManagementCore.ClassTable, result.AstNodes);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.AstNodes);

            var expr1 = result.AstNodes.First() as BinaryExpressionNode;
            var expr2 = result.AstNodes.Last() as BinaryExpressionNode;

            Assert.IsNotNull(expr1);
            Assert.IsNotNull(expr2);

            // Since there is a conflict with FFITarget.DesignScript.Point and FFITarget.Dynamo.Point,
            // node to code generates the shortest unique name, which in this case will be
            // Autodesk.Point for Autodesk.DesignScript.Geometry.Point
            Assert.AreEqual("Autodesk.Point.ByCoordinates(0, 0)", expr1.RightNode.ToString());
            Assert.AreEqual("Autodesk.Point.ByCoordinates(0, 0)", expr2.RightNode.ToString());
        }

        [Test]
        public void TestShortestQualifiedNameReplacer6()
        {
            OpenModel(@"core\node2code\unqualifiedName5.dyn");
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            var engine = CurrentDynamoModel.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            result = NodeToCodeCompiler.ConstantPropagationForTemp(result, Enumerable.Empty<string>());
            NodeToCodeCompiler.ReplaceWithShortestQualifiedName(engine.LibraryServices.LibraryManagementCore.ClassTable, result.AstNodes);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.AstNodes);

            var expr = result.AstNodes.Last() as BinaryExpressionNode;
            Assert.IsNotNull(expr);
            Assert.AreEqual("Geometry.DistanceTo(t1, t2)", expr.RightNode.ToString());
        }

        [Test]
        public void TestShortestQualifiedNameReplacer7()
        {
            string libraryPath = "FFITarget.dll";
            if (!CurrentDynamoModel.EngineController.LibraryServices.IsLibraryLoaded(libraryPath))
            {
                CurrentDynamoModel.EngineController.LibraryServices.ImportLibrary(libraryPath);
            }

            // Point.ByCoordinates(1,2,3);
            // Point.ByCoordinates(1,2,3);
            OpenModel(@"core\node2code\unqualifiedName6.dyn");
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            var engine = CurrentDynamoModel.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            result = NodeToCodeCompiler.ConstantPropagationForTemp(result, Enumerable.Empty<string>());
            NodeToCodeCompiler.ReplaceWithShortestQualifiedName(engine.LibraryServices.LibraryManagementCore.ClassTable, result.AstNodes);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.AstNodes);

            var expr1 = result.AstNodes.First() as BinaryExpressionNode;
            var expr2 = result.AstNodes.Last() as BinaryExpressionNode;

            Assert.IsNotNull(expr1);
            Assert.IsNotNull(expr2);

            // Since there is a conflict with FFITarget.DesignScript.Point and FFITarget.Dynamo.Point,
            // node to code generates the shortest unique name, which in this case will be
            // Autodesk.Point for Autodesk.DesignScript.Geometry.Point
            Assert.AreEqual("Autodesk.Point.ByCoordinates(1, 2, 3)", expr1.RightNode.ToString());
            Assert.AreEqual("Autodesk.Point.ByCoordinates(1, 2, 3)", expr2.RightNode.ToString());
        }

        [Test]
        public void TestShortestQualifiedNameReplacerWithClassConflicts()
        {
            string libraryPath = "FFITarget.dll";
            if (!CurrentDynamoModel.EngineController.LibraryServices.IsLibraryLoaded(libraryPath))
            {
                CurrentDynamoModel.EngineController.LibraryServices.ImportLibrary(libraryPath);
            }

            OpenModel(@"core\node2code\ShortenNodeNameWithClassConflicts.dyn");
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            var engine = CurrentDynamoModel.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            result = NodeToCodeCompiler.ConstantPropagationForTemp(result, Enumerable.Empty<string>());
            NodeToCodeCompiler.ReplaceWithShortestQualifiedName(engine.LibraryServices.LibraryManagementCore.ClassTable, result.AstNodes);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.AstNodes);

            var asts = result.AstNodes.ToList();
            Assert.AreEqual(6, asts.Count);

            Assert.IsTrue(asts.Any(x => (x as BinaryExpressionNode).RightNode.ToString() == "Revit.Elements.Parameter.Parameter()"));
            Assert.IsTrue(asts.Any(x => (x as BinaryExpressionNode).RightNode.ToString() == "RevitFamily.Parameter.Parameter()"));
            Assert.IsTrue(asts.Any(x => (x as BinaryExpressionNode).RightNode.ToString() == "archilab.Parameter.Parameter()"));
            Assert.IsTrue(asts.Any(x => (x as BinaryExpressionNode).RightNode.ToString() == "RevitProject.Parameter.Parameter()"));
            Assert.IsTrue(asts.Any(x => (x as BinaryExpressionNode).RightNode.ToString() == "Revit.Elements.Category.Category()"));
            Assert.IsTrue(asts.Any(x => (x as BinaryExpressionNode).RightNode.ToString() == "Rhythm.Category.Category()"));
        }

        [Test]
        public void TestShortestQualifiedNameReplacerTypedIdentiferFFITarget()
        {
            string libraryPath = "FFITarget.dll";
            if (!CurrentDynamoModel.EngineController.LibraryServices.IsLibraryLoaded(libraryPath))
            {
                CurrentDynamoModel.EngineController.LibraryServices.ImportLibrary(libraryPath);
            }
            var typedIdNode = new TypedIdentifierNode
            {
                Name = "aName",
                Value = "aName",
                datatype = new ProtoCore.Type("FFITarget.Base", 0),
                TypeAlias = "FFITarget.Base"
            };
            var oldTypeName = typedIdNode.datatype.Name;

            var engine = CurrentDynamoModel.EngineController;
            NodeToCodeCompiler.ReplaceWithShortestQualifiedName(engine.LibraryServices.LibraryManagementCore.ClassTable, new List<AssociativeNode>() { typedIdNode });

            Assert.AreEqual("Base", typedIdNode.TypeAlias);
            Assert.AreEqual(oldTypeName, typedIdNode.datatype.Name);
            Assert.AreEqual("aName", typedIdNode.Name);
            Assert.AreEqual("aName", typedIdNode.Value);
        }

        [Test]
        public void TestShortestQualifiedNameReplacerTypedIdentifer()
        {
            var typedIdNode = new TypedIdentifierNode
            {
                Name = "aName",
                Value = "aName",
                datatype = new ProtoCore.Type("Autodesk.DesignScript.Geometry.Geometry", 0),
                TypeAlias = "Autodesk.DesignScript.Geometry.Geometry"
            };
            var oldTypeName = typedIdNode.datatype.Name;

            var engine = CurrentDynamoModel.EngineController;
            NodeToCodeCompiler.ReplaceWithShortestQualifiedName(engine.LibraryServices.LibraryManagementCore.ClassTable, new List<AssociativeNode>() { typedIdNode });

            Assert.AreEqual("Geometry", typedIdNode.TypeAlias);
            Assert.AreEqual(oldTypeName, typedIdNode.datatype.Name);
            Assert.AreEqual("aName", typedIdNode.Name);
            Assert.AreEqual("aName", typedIdNode.Value);
        }

        [Test]
        public void TestShortestQualifiedNameReplacerTypedIdentifer_WithConflict()
        {
            string libraryPath = "FFITarget.dll";
            if (!CurrentDynamoModel.EngineController.LibraryServices.IsLibraryLoaded(libraryPath))
            {
                CurrentDynamoModel.EngineController.LibraryServices.ImportLibrary(libraryPath);
            }

            var typedIdNode = new TypedIdentifierNode
            {
                Name = "aName",
                Value = "aName",
                datatype = new ProtoCore.Type("Autodesk.DesignScript.Geometry.Point", 0),
                TypeAlias = "Autodesk.DesignScript.Geometry.Point"
            };
            var oldTypeName = typedIdNode.datatype.Name;

            var engine = CurrentDynamoModel.EngineController;
            NodeToCodeCompiler.ReplaceWithShortestQualifiedName(engine.LibraryServices.LibraryManagementCore.ClassTable, new List<AssociativeNode>() { typedIdNode });

            Assert.AreEqual("Autodesk.Point", typedIdNode.TypeAlias);
            Assert.AreEqual(oldTypeName, typedIdNode.datatype.Name);
            Assert.AreEqual("aName", typedIdNode.Name);
            Assert.AreEqual("aName", typedIdNode.Value);
        }

        [Test]
        public void TestShortestQualifiedNameReplacerWithDefaultArgument()
        {
            string libraryPath = "FFITarget.dll";
            if (!CurrentDynamoModel.EngineController.LibraryServices.IsLibraryLoaded(libraryPath))
            {
                CurrentDynamoModel.EngineController.LibraryServices.ImportLibrary(libraryPath);
            }

            OpenModel(@"core\node2code\SphereDefaultArg.dyn");
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            var engine = CurrentDynamoModel.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            result = NodeToCodeCompiler.ConstantPropagationForTemp(result, Enumerable.Empty<string>());
            NodeToCodeCompiler.ReplaceWithShortestQualifiedName(engine.LibraryServices.LibraryManagementCore.ClassTable, result.AstNodes);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.AstNodes);

            var expr1 = result.AstNodes.First() as BinaryExpressionNode;

            Assert.IsNotNull(expr1);

            // Since there is a conflict with FFITarget.DesignScript.Point and FFITarget.Dynamo.Point,
            // node to code generates the shortest unique name, which in this case will be
            // Autodesk.Point for Autodesk.DesignScript.Geometry.Point
            Assert.AreEqual("Sphere.ByCenterPointRadius(Autodesk.Point.ByCoordinates(0, 0, 0), 1)", expr1.RightNode.ToString());
        }

        [Test]
        public void TestShortestQualifiedNameReplacerWithDefaultArgument2()
        {
            string libraryPath = "FFITarget.dll";
            if (!CurrentDynamoModel.EngineController.LibraryServices.IsLibraryLoaded(libraryPath))
            {
                CurrentDynamoModel.EngineController.LibraryServices.ImportLibrary(libraryPath);
            }

            OpenModel(@"core\node2code\ShortenNodeNameWithDefaultArg.dyn");
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            var engine = CurrentDynamoModel.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            result = NodeToCodeCompiler.ConstantPropagationForTemp(result, Enumerable.Empty<string>());
            NodeToCodeCompiler.ReplaceWithShortestQualifiedName(engine.LibraryServices.LibraryManagementCore.ClassTable, result.AstNodes);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.AstNodes);

            var expr1 = result.AstNodes.First() as BinaryExpressionNode;

            Assert.IsNotNull(expr1);

            // Since there is a conflict with FFITarget.DesignScript.Point and FFITarget.Dynamo.Point,
            // node to code generates the shortest unique name, which in this case will be
            // Autodesk.Point for Autodesk.DesignScript.Geometry.Point
            Assert.AreEqual("ElementResolverTarget.StaticMethod(ElementResolverTarget.Create().StaticProperty)", expr1.RightNode.ToString());
        }

        [Test]
        public void TestShortestQualifiedNameReplacerWithStaticPropertyInDefaultArgument()
        {
            string libraryPath = "FFITarget.dll";
            if (!CurrentDynamoModel.EngineController.LibraryServices.IsLibraryLoaded(libraryPath))
            {
                CurrentDynamoModel.EngineController.LibraryServices.ImportLibrary(libraryPath);
            }

            OpenModel(@"core\node2code\ShortestQualifiedNameReplacerWithStaticPropertyInDefaultArgument.dyn");
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            var engine = CurrentDynamoModel.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            result = NodeToCodeCompiler.ConstantPropagationForTemp(result, Enumerable.Empty<string>());
            NodeToCodeCompiler.ReplaceWithShortestQualifiedName(engine.LibraryServices.LibraryManagementCore.ClassTable, result.AstNodes);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.AstNodes);

            var expr1 = result.AstNodes.First() as BinaryExpressionNode;

            Assert.IsNotNull(expr1);
            Assert.AreEqual(
                "ElementResolverTarget.StaticMethod2(ElementResolverTarget.StaticProperty.StaticProperty2.Method(ElementResolverTarget.Create()))",
                expr1.RightNode.ToString());
        }

        [Test]
        public void TestShortestQualifiedNameReplacerWithStaticProperty()
        {
            string libraryPath = "FFITarget.dll";
            if (!CurrentDynamoModel.EngineController.LibraryServices.IsLibraryLoaded(libraryPath))
            {
                CurrentDynamoModel.EngineController.LibraryServices.ImportLibrary(libraryPath);
            }

            OpenModel(@"core\node2code\ShortenNodeNameWithStaticProperty.dyn");
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            var engine = CurrentDynamoModel.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            result = NodeToCodeCompiler.ConstantPropagationForTemp(result, Enumerable.Empty<string>());
            NodeToCodeCompiler.ReplaceWithShortestQualifiedName(engine.LibraryServices.LibraryManagementCore.ClassTable, result.AstNodes);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.AstNodes);

            var expr1 = result.AstNodes.First() as BinaryExpressionNode;

            Assert.IsNotNull(expr1);

            Assert.AreEqual("ElementResolverTarget.StaticProperty", expr1.RightNode.ToString());
        }

        [Test]
        public void TestShortestQualifiedNameReplacerWithFFITargetListClass()
        {
            string libraryPath = "FFITarget.dll";
            if (!CurrentDynamoModel.EngineController.LibraryServices.IsLibraryLoaded(libraryPath))
            {
                CurrentDynamoModel.EngineController.LibraryServices.ImportLibrary(libraryPath);
            }

            OpenModel(@"core\node2code\FFITarget.DSCore.List.dyn");
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            var engine = CurrentDynamoModel.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            result = NodeToCodeCompiler.ConstantPropagationForTemp(result, Enumerable.Empty<string>());
            NodeToCodeCompiler.ReplaceWithShortestQualifiedName(engine.LibraryServices.LibraryManagementCore.ClassTable, result.AstNodes);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.AstNodes);

            var asts = result.AstNodes.ToList();
            Assert.AreEqual(2, asts.Count);

            var expr = asts[1] as BinaryExpressionNode;

            Assert.IsNotNull(expr);

            Assert.AreEqual("FFITarget.List.Count(l)", expr.RightNode.ToString());

        }

        [Test]
        public void TestShortestQualifiedNameReplacerWithGlobalClass()
        {
            string libraryPath = "FFITarget.dll";
            if (!CurrentDynamoModel.EngineController.LibraryServices.IsLibraryLoaded(libraryPath))
            {
                CurrentDynamoModel.EngineController.LibraryServices.ImportLibrary(libraryPath);
            }

            OpenModel(@"core\node2code\ShortenNodeNameWithGlobalClass.dyn");
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            var engine = CurrentDynamoModel.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            result = NodeToCodeCompiler.ConstantPropagationForTemp(result, Enumerable.Empty<string>());
            NodeToCodeCompiler.ReplaceWithShortestQualifiedName(engine.LibraryServices.LibraryManagementCore.ClassTable, result.AstNodes);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.AstNodes);

            var asts = result.AstNodes.ToList();
            Assert.AreEqual(2, asts.Count);

            var expr1 = asts[0] as BinaryExpressionNode;
            var expr2 = asts[1] as BinaryExpressionNode;

            Assert.IsNotNull(expr1);
            Assert.IsNotNull(expr2);

            Assert.AreEqual("DupTargetTest.DupTargetTest()", expr1.RightNode.ToString());
            Assert.AreEqual("DupTargetTest.Foo(t1)", expr2.RightNode.ToString());

        }

        [Test]
        public void TestShortestQualifiedNameReplacerWithConflictingClass()
        {
            string libraryPath = "FFITarget.dll";
            if (!CurrentDynamoModel.EngineController.LibraryServices.IsLibraryLoaded(libraryPath))
            {
                CurrentDynamoModel.EngineController.LibraryServices.ImportLibrary(libraryPath);
            }

            // This file has a node from FFITarget.C.B.DupTargetTest namespace.
            // Since this class is marked as hidden from library but is inherited by
            // global class DupTargetTest, performing a Node2Code on this node, should
            // emit the global class name in a CBN.
            OpenModel(@"core\node2code\ShortenNodeNameWithConflictingClass.dyn");
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            var engine = CurrentDynamoModel.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            result = NodeToCodeCompiler.ConstantPropagationForTemp(result, Enumerable.Empty<string>());
            NodeToCodeCompiler.ReplaceWithShortestQualifiedName(engine.LibraryServices.LibraryManagementCore.ClassTable, result.AstNodes);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.AstNodes);

            var asts = result.AstNodes.ToList();
            Assert.AreEqual(2, asts.Count);

            var expr1 = asts[0] as BinaryExpressionNode;
            var expr2 = asts[1] as BinaryExpressionNode;

            Assert.IsNotNull(expr1);
            Assert.IsNotNull(expr2);

            Assert.AreEqual("DupTargetTest.DupTargetTest()", expr1.RightNode.ToString());
            Assert.AreEqual("DupTargetTest.Prop(t1)", expr2.RightNode.ToString());

        }

        [Test]
        public void TestBasicNode2CodeWorkFlow1()
        {
            // 1 -> a -> x
            OpenModel(@"core\node2code\workflow1.dyn");
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            var engine = CurrentDynamoModel.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            result = NodeToCodeCompiler.ConstantPropagationForTemp(result, Enumerable.Empty<string>());
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.AstNodes);

            var expr1 = result.AstNodes.First() as BinaryExpressionNode;
            var expr2 = result.AstNodes.Last() as BinaryExpressionNode;

            Assert.IsNotNull(expr1);
            Assert.IsNotNull(expr2);

            Assert.IsTrue(expr1.ToString().StartsWith("a = 1;"));
            Assert.IsTrue(expr2.ToString().StartsWith("x = a;"));

            var undo = new DynamoModel.UndoRedoCommand(DynamoModel.UndoRedoCommand.Operation.Undo);
            CurrentDynamoModel.ExecuteCommand(undo);

            Assert.AreEqual(2, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(3, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
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
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;

            SelectAll(nodes);
            var command = new DynamoModel.ConvertNodesToCodeCommand();
            CurrentDynamoModel.ExecuteCommand(command);

            Assert.AreEqual(0, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            var undo = new DynamoModel.UndoRedoCommand(DynamoModel.UndoRedoCommand.Operation.Undo);
            CurrentDynamoModel.ExecuteCommand(undo);

            Assert.AreEqual(6, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(6, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
        }

        [Test]
        public void TestBasicNode2CodeWorkFlow3()
        {
            OpenModel(@"core\node2code\workflow3.dyn");
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;

            SelectAll(nodes);
            var command = new DynamoModel.ConvertNodesToCodeCommand();
            CurrentDynamoModel.ExecuteCommand(command);

            Assert.AreEqual(3, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(3, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            var undo = new DynamoModel.UndoRedoCommand(DynamoModel.UndoRedoCommand.Operation.Undo);
            CurrentDynamoModel.ExecuteCommand(undo);

            Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(5, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
        }

        [Test]
        public void TestBasicNode2CodeWorkFlow4()
        {
            OpenModel(@"core\node2code\workflow4.dyn");
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;

            SelectAll(nodes);
            var command = new DynamoModel.ConvertNodesToCodeCommand();
            CurrentDynamoModel.ExecuteCommand(command);

            Assert.AreEqual(3, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(3, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            var undo = new DynamoModel.UndoRedoCommand(DynamoModel.UndoRedoCommand.Operation.Undo);
            CurrentDynamoModel.ExecuteCommand(undo);

            Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
        }

        [Test]
        public void TestShortName1()
        {
            string libraryPath = "FFITarget.dll";
            if (!CurrentDynamoModel.EngineController.LibraryServices.IsLibraryLoaded(libraryPath))
            {
                CurrentDynamoModel.EngineController.LibraryServices.ImportLibrary(libraryPath);
            }

            OpenModel(@"core\node2code\shortName1.dyn");
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            var engine = CurrentDynamoModel.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            result = NodeToCodeCompiler.ConstantPropagationForTemp(result, Enumerable.Empty<string>());
            NodeToCodeCompiler.ReplaceWithShortestQualifiedName(engine.LibraryServices.LibraryManagementCore.ClassTable, result.AstNodes);
            Assert.IsTrue(result != null && result.AstNodes != null);

            var expr = result.AstNodes.Last() as BinaryExpressionNode;

            Assert.IsNotNull(expr);

            // Since there is a conflict with FFITarget.DesignScript.Point and FFITarget.Dynamo.Point,
            // node to code generates the shortest unique name, which in this case will be
            // Autodesk.Point for Autodesk.DesignScript.Geometry.Point
            Assert.AreEqual("Autodesk.Point.ByCoordinates(t1, 0)", expr.RightNode.ToString());
        }

        [Test]
        public void TestPropertyWontBeReplaced1()
        {
            // Point.X; Point.X;
            OpenModel(@"core\node2code\property.dyn");
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            var engine = CurrentDynamoModel.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            result = NodeToCodeCompiler.ConstantPropagationForTemp(result, Enumerable.Empty<string>());
            NodeToCodeCompiler.ReplaceWithShortestQualifiedName(engine.LibraryServices.LibraryManagementCore.ClassTable, result.AstNodes);
            Assert.IsTrue(result != null && result.AstNodes != null);

            var rhs = result.AstNodes.Skip(1).Select(b => (b as BinaryExpressionNode).RightNode.ToString().Contains("X"));
            Assert.IsTrue(rhs.All(r => r));
        }

        [Test]
        public void TestPropertyWontBeReplaced2()
        {
            string libraryPath = "FFITarget.dll";
            if (!CurrentDynamoModel.EngineController.LibraryServices.IsLibraryLoaded(libraryPath))
            {
                CurrentDynamoModel.EngineController.LibraryServices.ImportLibrary(libraryPath);
            }

            OpenModel(@"core\node2code\staticproperty.dyn");
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            var engine = CurrentDynamoModel.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            result = NodeToCodeCompiler.ConstantPropagationForTemp(result, Enumerable.Empty<string>());
            NodeToCodeCompiler.ReplaceWithShortestQualifiedName(engine.LibraryServices.LibraryManagementCore.ClassTable, result.AstNodes);
            Assert.IsTrue(result != null && result.AstNodes != null);

            var rhs = result.AstNodes.Skip(1).Select(b => (b as BinaryExpressionNode).RightNode.ToString().EndsWith("ElementResolverTarget.StaticProperty"));
            Assert.IsTrue(rhs.All(r => r));
        }

        [Test]
        public void TestPropertyToStaticPropertyInvocation()
        {
            string libraryPath = "FFITarget.dll";
            if (!CurrentDynamoModel.EngineController.LibraryServices.IsLibraryLoaded(libraryPath))
            {
                CurrentDynamoModel.EngineController.LibraryServices.ImportLibrary(libraryPath);
            }

            OpenModel(@"core\node2code\tostaticproperty.dyn");
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            var engine = CurrentDynamoModel.EngineController;

            var result = engine.ConvertNodesToCode(nodes, nodes);
            result = NodeToCodeCompiler.ConstantPropagationForTemp(result, Enumerable.Empty<string>());
            NodeToCodeCompiler.ReplaceWithShortestQualifiedName(engine.LibraryServices.LibraryManagementCore.ClassTable, result.AstNodes);
            Assert.IsTrue(result != null && result.AstNodes != null);

            var rhs = result.AstNodes.Skip(1).Select(b => (b as BinaryExpressionNode).RightNode.ToString().Contains("ValueContainer.SomeValue"));
            Assert.IsTrue(rhs.All(r => r));
        }

        [Test]
        public void UniqueNamespace_on_node2code_noConflictWarning()
        {
            string libraryPath = "FFITarget.dll";
            if (!CurrentDynamoModel.EngineController.LibraryServices.IsLibraryLoaded(libraryPath))
            {
                CurrentDynamoModel.EngineController.LibraryServices.ImportLibrary(libraryPath);
            }

            // this graph contains a single "FFITarget.B.DupTargetTest" node that should not conflict with namespace "FFITarget.C.B.DupTargetTest"
            OpenModel(@"core\node2code\UniqueNamespace_on_node2code_noConflictWarning.dyn");
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            SelectAll(nodes);
            var command = new DynamoModel.ConvertNodesToCodeCommand();
            CurrentDynamoModel.ExecuteCommand(command);

            nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            var cbn = nodes.OfType<CodeBlockNodeModel>().FirstOrDefault();
            Assert.IsNotNull(cbn);

            Assert.False(cbn.IsInErrorState);
        }


        private void TestNodeToCodeUndoBase(string filePath)
        {
            // Verify after undo all nodes and connectors should be resotored back
            // to original state.
            OpenModel(filePath);

            var wp = CurrentDynamoModel.CurrentWorkspace;
            var nodeGuids = wp.Nodes.Select(n => n.GUID).ToList();
            nodeGuids.Sort();

            var connectorGuids = wp.Connectors.Select(c => c.GUID).ToList();
            connectorGuids.Sort();

            SelectAll(wp.Nodes);
            var command = new DynamoModel.ConvertNodesToCodeCommand();
            CurrentDynamoModel.ExecuteCommand(command);

            var undo = new DynamoModel.UndoRedoCommand(DynamoModel.UndoRedoCommand.Operation.Undo);
            CurrentDynamoModel.ExecuteCommand(undo);

            var curNodeGuids = wp.Nodes.Select(n => n.GUID).ToList();
            curNodeGuids.Sort();

            var curConnectorGuids = wp.Connectors.Select(c => c.GUID).ToList();
            curConnectorGuids.Sort();

            Assert.AreEqual(curNodeGuids.Count, nodeGuids.Count);
            Assert.IsTrue(nodeGuids.SequenceEqual(curNodeGuids));

            Assert.AreEqual(curConnectorGuids.Count, connectorGuids.Count);
            Assert.IsTrue(connectorGuids.SequenceEqual(curConnectorGuids));
        }

        [Test]
        public void TestNodeToCodeUndo1()
        {
            TestNodeToCodeUndoBase(@"core\node2code\undo1.dyn");
        }

        [Test]
        public void TestNodeToCodeUndo2()
        {
            TestNodeToCodeUndoBase(@"core\node2code\undo2.dyn");
        }


        [Test]
        [Category("UnitTests")]
        public void TestNodeToCodeUndoRecorder()
        {
            NodeToCodeUndoHelper recorder = new NodeToCodeUndoHelper();
            var dummyModel = new DummyModel(1, 10);
            recorder.RecordCreation(dummyModel);
            recorder.RecordDeletion(dummyModel);
            Assert.AreEqual(0, recorder.ActionCount());
        }

        [Test]
        public void TestUINode_String()
        {
            OpenModel(@"core\node2code\stringNode.dyn");
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            var engine = CurrentDynamoModel.EngineController;
            var result = engine.ConvertNodesToCode(nodes, nodes);
            Assert.IsNotNull(result.AstNodes);

            var assignment = result.AstNodes.FirstOrDefault();
            Assert.IsNotNull(assignment);

            var binaryExpr = assignment as BinaryExpressionNode;
            Assert.IsNotNull(binaryExpr);

            Assert.AreEqual("\"42\"", binaryExpr.RightNode.ToString());
        }

        [Test]
        public void TestUINode_IntegerSlider()
        {
            OpenModel(@"core\node2code\integerSlider.dyn");
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            var engine = CurrentDynamoModel.EngineController;
            var result = engine.ConvertNodesToCode(nodes, nodes);
            Assert.IsNotNull(result.AstNodes);

            var assignment = result.AstNodes.FirstOrDefault();
            Assert.IsNotNull(assignment);

            var binaryExpr = assignment as BinaryExpressionNode;
            Assert.IsNotNull(binaryExpr);

            Assert.AreEqual("42", binaryExpr.RightNode.ToString());
        }

        [Test]
        public void TestUINode_NumberSlider()
        {
            OpenModel(@"core\node2code\numberSlider.dyn");
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            var engine = CurrentDynamoModel.EngineController;
            var result = engine.ConvertNodesToCode(nodes, nodes);
            Assert.IsNotNull(result.AstNodes);

            var assignment = result.AstNodes.FirstOrDefault();
            Assert.IsNotNull(assignment);

            var binaryExpr = assignment as BinaryExpressionNode;
            Assert.IsNotNull(binaryExpr);

            Assert.AreEqual("42", binaryExpr.RightNode.ToString());
        }

        [Test]
        public void TestUINode_BoolSelector()
        {
            OpenModel(@"core\node2code\boolSelector.dyn");
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            var engine = CurrentDynamoModel.EngineController;
            var result = engine.ConvertNodesToCode(nodes, nodes);
            Assert.IsNotNull(result.AstNodes);

            var assignment = result.AstNodes.FirstOrDefault();
            Assert.IsNotNull(assignment);

            var binaryExpr = assignment as BinaryExpressionNode;
            Assert.IsNotNull(binaryExpr);

            Assert.AreEqual("true", binaryExpr.RightNode.ToString());
        }

        [Test]
        public void TestUINode_CreateList()
        {
            OpenModel(@"core\node2code\CreateList.dyn");
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            var engine = CurrentDynamoModel.EngineController;
            var result = engine.ConvertNodesToCode(nodes, nodes);
            Assert.IsNotNull(result.AstNodes);

            Assert.AreEqual(3, result.AstNodes.Count());
            var assignment = result.AstNodes.LastOrDefault();
            Assert.IsNotNull(assignment);

            var binaryExpr = assignment as BinaryExpressionNode;
            Assert.IsNotNull(binaryExpr);

            Assert.AreEqual("[t1, t2]", binaryExpr.RightNode.ToString());
        }

        [Test]
        public void TestUINode_NumberSequence()
        {
            OpenModel(@"core\node2code\numberSequence.dyn");
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            var engine = CurrentDynamoModel.EngineController;
            var result = engine.ConvertNodesToCode(nodes, nodes);
            Assert.IsNotNull(result.AstNodes);

            var assignment = result.AstNodes.LastOrDefault();
            Assert.IsNotNull(assignment);

            var binaryExpr = assignment as BinaryExpressionNode;
            Assert.IsNotNull(binaryExpr);

            Assert.IsTrue(binaryExpr.RightNode is RangeExprNode);
        }

        [Test]
        public void TestUINode_NumberRange()
        {
            OpenModel(@"core\node2code\numberRange.dyn");
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            var engine = CurrentDynamoModel.EngineController;
            var result = engine.ConvertNodesToCode(nodes, nodes);
            Assert.IsNotNull(result.AstNodes);

            var assignment = result.AstNodes.LastOrDefault();
            Assert.IsNotNull(assignment);

            var binaryExpr = assignment as BinaryExpressionNode;
            Assert.IsNotNull(binaryExpr);

            Assert.IsTrue(binaryExpr.RightNode is RangeExprNode); ;
        }

        [Test]
        public void TestFilePathToCode()
        {
            OpenModel(@"core\node2code\filepath.dyn");
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;

            SelectAll(nodes);
            var command = new DynamoModel.ConvertNodesToCodeCommand();
            CurrentDynamoModel.ExecuteCommand(command);

            var cbn = CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<CodeBlockNodeModel>().FirstOrDefault();
            Assert.IsNotNull(cbn);

            Assert.IsTrue(cbn.Code.Contains(@"D:\\foo\\bar"));
        }

        [Test]
        public void TestDirectoryToCode()
        {
            OpenModel(@"core\node2code\directory.dyn");
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;

            SelectAll(nodes);
            var command = new DynamoModel.ConvertNodesToCodeCommand();
            CurrentDynamoModel.ExecuteCommand(command);

            var cbn = CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<CodeBlockNodeModel>().FirstOrDefault();
            Assert.IsNotNull(cbn);

            Assert.IsTrue(cbn.Code.Contains(@"D:\\foo\\bar"));
        }

        [Test]
        public void TestNodeWithVarArgument()
        {
            OpenModel(@"core\node2code\splitstring.dyn");
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            SelectAll(nodes);

            var command = new DynamoModel.ConvertNodesToCodeCommand();
            CurrentDynamoModel.ExecuteCommand(command);
            CurrentDynamoModel.ForceRun();

            var cbn = CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<CodeBlockNodeModel>().FirstOrDefault();
            Assert.IsNotNull(cbn);

            var guid = cbn.GUID.ToString();
            AssertPreviewValue(guid, new[] { "foo", "bar", "qux" });
        }

        [Test]
        [Category("RegressionTests")]
        public void TestMultioutputNode()
        {
            // Regression MAGN-8009 
            OpenModel(@"core\node2code\multipleoutput.dyn");
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            SelectAll(nodes);

            var functionNode = CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<DSFunction>().FirstOrDefault();
            var guid = functionNode.AstIdentifierGuid;

            var command = new DynamoModel.ConvertNodesToCodeCommand();
            CurrentDynamoModel.ExecuteCommand(command);
            CurrentDynamoModel.ForceRun();

            var cbn = CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<CodeBlockNodeModel>().FirstOrDefault();
            Assert.IsNotNull(cbn);
            Assert.IsFalse(cbn.Code.Contains(guid));
        }

        [Test]
        [Category("UnitTests")]
        public void TestNameProvider()
        {
            var core = CurrentDynamoModel.EngineController.LibraryServices.LibraryManagementCore;
            var libraryServices = new LibraryCustomizationServices(CurrentDynamoModel.PathManager);
            var nameProvider = new NamingProvider(core, libraryServices);

            ProtoCore.Type t;
            string name = string.Empty;
            int typeID = -1;

            t = ProtoCore.TypeSystem.BuildPrimitiveTypeObject(ProtoCore.PrimitiveType.Integer);
            name = nameProvider.GetTypeDependentName(t);
            Assert.AreEqual("num", name);

            t = ProtoCore.TypeSystem.BuildPrimitiveTypeObject(ProtoCore.PrimitiveType.Double);
            name = nameProvider.GetTypeDependentName(t);
            Assert.AreEqual("num", name);

            t = ProtoCore.TypeSystem.BuildPrimitiveTypeObject(ProtoCore.PrimitiveType.String);
            name = nameProvider.GetTypeDependentName(t);
            Assert.AreEqual("str", name);

            typeID = core.TypeSystem.GetType("Autodesk.DesignScript.Geometry.Point");
            t = core.TypeSystem.BuildTypeObject(typeID);
            name = nameProvider.GetTypeDependentName(t);
            Assert.AreEqual("point", name);

            typeID = core.TypeSystem.GetType("Autodesk.DesignScript.Geometry.BoundingBox");
            t = core.TypeSystem.BuildTypeObject(typeID);
            name = nameProvider.GetTypeDependentName(t);
            Assert.AreEqual("boundingBox", name);

            t = new ProtoCore.Type();
            t.Name = "DummyClassForTest";
            t.UID = -1;
            name = nameProvider.GetTypeDependentName(t);
            Assert.IsTrue(string.IsNullOrEmpty(name));
        }

        private void SelectAll(IEnumerable<NodeModel> nodes)
        {
            DynamoSelection.Instance.ClearSelection();
            nodes.ToList().ForEach((ele) => DynamoSelection.Instance.Selection.Add(ele));
        }

        [Test]
        [Category("RegressionTests")]
        public void TestDoubleValueInDifferentCulture()
        {
            var frCulture = CultureInfo.CreateSpecificCulture("fr-FR");

            var currentCulture = Thread.CurrentThread.CurrentCulture;
            var currentUICulture = Thread.CurrentThread.CurrentUICulture;

            Thread.CurrentThread.CurrentCulture = frCulture;
            Thread.CurrentThread.CurrentUICulture = frCulture;

            // manually verified s="1,234";
            double d = 1.234;
            string s = d.ToString();

            DoubleNode d1 = new DoubleNode(1.234);
            string s1 = d1.ToString();
            Assert.AreEqual(s1, "1.234");

            ProtoCore.AST.ImperativeAST.DoubleNode d2 = new ProtoCore.AST.ImperativeAST.DoubleNode(1.234);
            string s2 = d2.ToString();
            Assert.AreEqual(s2, "1.234");

            Thread.CurrentThread.CurrentCulture = currentCulture;
            Thread.CurrentThread.CurrentUICulture = currentUICulture;
        }

        [Test]
        [Category("UnitTests")]
        public void TestUsingTypeDependentVariableName01()
        {
            OpenModel(@"core\node2code\string.dyn");
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            SelectAll(nodes);

            var command = new DynamoModel.ConvertNodesToCodeCommand();
            CurrentDynamoModel.ExecuteCommand(command);

            var cbn = CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<CodeBlockNodeModel>().FirstOrDefault();
            Assert.IsNotNull(cbn);
            Assert.IsTrue(cbn.GetAstIdentifierForOutputIndex(0).Value.StartsWith("str"));
        }

        [Test]
        [Category("UnitTests")]
        public void TestUsingTypeDependentVariableName02()
        {
            OpenModel(@"core\node2code\num.dyn");
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            SelectAll(nodes);

            var command = new DynamoModel.ConvertNodesToCodeCommand();
            CurrentDynamoModel.ExecuteCommand(command);

            var cbn = CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<CodeBlockNodeModel>().FirstOrDefault();
            Assert.IsNotNull(cbn);
            Assert.IsTrue(cbn.GetAstIdentifierForOutputIndex(0).Value.StartsWith("num"));
        }

        [Test]
        public void TestNodeToCodeWithPropertyInCodeBlock()
        {
            OpenModel(@"core\node2code\NodeToCode_CodeBlock.dyn");
            var nodes = CurrentDynamoModel.CurrentWorkspace.Nodes;
            SelectAll(nodes);

            var command = new DynamoModel.ConvertNodesToCodeCommand();
            CurrentDynamoModel.ExecuteCommand(command);

            var cbn = CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<CodeBlockNodeModel>().FirstOrDefault();
            Assert.IsNotNull(cbn);

            AssertPreviewValue(cbn.GUID.ToString(), false);
        }
    }

    [Category("NodeToCode")]
    class NodeToCodeSystemTest : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            base.GetLibrariesToPreload(libraries);
        }

        private void SelectAll(IEnumerable<Guid> nodes)
        {
            DynamoSelection.Instance.ClearSelection();
            foreach (var node in CurrentDynamoModel.CurrentWorkspace.Nodes)
            {
                if (nodes.Contains(node.GUID))
                {
                    DynamoSelection.Instance.Selection.Add(node);
                }
            }
        }

        private void SelectAllExcept(Guid excludedNode)
        {
            DynamoSelection.Instance.ClearSelection();
            foreach (var node in CurrentDynamoModel.CurrentWorkspace.Nodes)
            {
                if (node.GUID != excludedNode)
                {
                    DynamoSelection.Instance.Selection.Add(node);
                }
            }
        }

        private string GetStringData(Guid guid)
        {
            var varName = GetVarName(guid);
            var mirror = GetRuntimeMirror(varName);
            Assert.IsNotNull(mirror);
            return mirror.GetStringData();
        }

        private static string[] GetDynFiles(string folder)
        {
            var dir = Path.Combine(TestDirectory, @"core\node2code\" + folder);
            var files = Directory.GetFiles(dir, "*.dyn");
            return files;
        }

        private static string[] GetFilesForMutation()
        {
            return GetDynFiles("mutation");
        }

        /// <summary>
        /// Run the dyn file and get all preview values in string representation. 
        /// Then, iterate all nodes, for each iteration, choose a node and convert 
        /// the remaining nodes to code, and compare the preview value of this 
        /// node against with its original value; then undo, run and compare the
        /// preview values of all nodes with original values.
        /// </summary>
        /// <param name="dynFilePath"></param>
        protected void MutationTest(string dynFilePath)
        {
            CurrentDynamoModel.Scheduler.ProcessMode = TaskProcessMode.Asynchronous;

            RunModel(dynFilePath);
            // Block until all tasks are executed
            while (CurrentDynamoModel.Scheduler.HasPendingTasks) ;

            var allNodes = CurrentDynamoModel.CurrentWorkspace.Nodes.Select(n => n.GUID).ToList();
            int nodeCount = allNodes.Count();
            var previewMap = allNodes.ToDictionary(n => n, n => GetStringData(n));

            var convertibleNodes = CurrentDynamoModel.CurrentWorkspace.Nodes
                                                                      .Where(node => node.IsConvertible)
                                                                      .Select(n => n.GUID).ToList();
            int connectorCount = CurrentDynamoModel.CurrentWorkspace.Connectors.Count();

            for (int i = 1; i <= Math.Min(convertibleNodes.Count(), 10); ++i)
            {
                var toBeConvertedNodes = convertibleNodes.Take(i);
                var otherNodes = allNodes.Except(toBeConvertedNodes);

                SelectAll(toBeConvertedNodes);

                var command = new DynamoModel.ConvertNodesToCodeCommand();
                CurrentDynamoModel.ExecuteCommand(command);
                // Block until all tasks are executed
                while (CurrentDynamoModel.Scheduler.HasPendingTasks) ;

                foreach (var node in otherNodes)
                {
                    // Verify after converting remaining nodes to code, the value
                    // of node that is not converted should remain same.
                    var preValue = previewMap[node];
                    var currentValue = GetStringData(node);
                    Assert.AreEqual(preValue, currentValue);
                }

                var undo = new DynamoModel.UndoRedoCommand(DynamoModel.UndoRedoCommand.Operation.Undo);
                CurrentDynamoModel.ExecuteCommand(undo);
                // Block until all tasks are executed
                while (CurrentDynamoModel.Scheduler.HasPendingTasks) ;

                // Verify after undo everything is OK
                Assert.AreEqual(nodeCount, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
                Assert.AreEqual(connectorCount, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

                foreach (var node in CurrentDynamoModel.CurrentWorkspace.Nodes)
                {
                    Assert.IsTrue(previewMap.ContainsKey(node.GUID));
                    var preValue = previewMap[node.GUID];
                    var currentValue = GetStringData(node.GUID);
                    Assert.AreEqual(preValue, currentValue);
                }
            }
        }
        /*
        TODO these tests were disabled because they are not stable and 
        their random nature makes them randmoly fail.
        [Test, TestCaseSource("GetFilesForMutation")]
        public void TestMutation(string fileName)
        {
            MutationTest(fileName);
        }
        
     */
    }
}
