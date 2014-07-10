using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NUnit.Framework;
using ProtoCore;
using ProtoCore.AST.AssociativeAST;
using Dynamo;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.DSEngine;
using ProtoCore.Mirror;
using ProtoCore.DSASM;
using Dynamo.Models;
using DynCmd = Dynamo.ViewModels.DynamoViewModel;

namespace Dynamo.Tests
{
    class CodeBlockNodeTests : DynamoUnitTest
    {
#if false
        [Test]
        public void TestVariableClass()
        {
            string code = "a;";
            CodeBlockNode commentNode;
            ProtoCore.AST.Node resultNode;
            resultNode = GraphToDSCompiler.GraphUtilities.Parse(code, out commentNode);
            IdentifierNode iNode = (resultNode as CodeBlockNode).Body[0] as IdentifierNode;
            Variable var1 = new Variable(iNode);
            Assert.AreEqual("a", var1.Name);
            Assert.AreEqual(1, var1.Row);
            Assert.AreEqual(1, var1.StartColumn);
            Assert.AreEqual(2, var1.EndColumn);
            iNode = null;
            Assert.Catch<ArgumentNullException>(delegate { Variable var2 = new Variable(iNode); });
        }

        [Test]
        public void TestStatement_InlineExpression()
        {
            string code = @"
a = 
b>c+5 ? 
d-2 : 
e+f ;";
            CodeBlockNode commentNode;
            ProtoCore.AST.Node resultNode;
            List<string> refVarNames = new List<string>();
            resultNode = GraphToDSCompiler.GraphUtilities.Parse(code, out commentNode);
            BinaryExpressionNode ben = (resultNode as CodeBlockNode).Body[0] as BinaryExpressionNode;
            Statement s1 = Statement.CreateInstance((resultNode as CodeBlockNode).Body[0]);
            refVarNames = Statement.GetReferencedVariableNames(s1, true);
            Assert.AreEqual(2, s1.StartLine);
            Assert.AreEqual(5, s1.EndLine);
            Assert.AreEqual("a", s1.FirstDefinedVariable.Name);
            Assert.AreEqual(Statement.StatementType.Expression, s1.CurrentType);
            Assert.AreEqual(5, refVarNames.Count);
            Assert.AreEqual(true, refVarNames.Contains("d"));
        }

        [Test]
        public void TestStatement_RangeExpression()
        {
            string code = @"
a = 
b..
c..
d;";
            CodeBlockNode commentNode;
            ProtoCore.AST.Node resultNode;
            List<string> refVarNames = new List<string>();
            resultNode = GraphToDSCompiler.GraphUtilities.Parse(code, out commentNode);
            BinaryExpressionNode ben = (resultNode as CodeBlockNode).Body[0] as BinaryExpressionNode;
            Statement s1 = Statement.CreateInstance((resultNode as CodeBlockNode).Body[0]);
            refVarNames = Statement.GetReferencedVariableNames(s1, true);
            Assert.AreEqual(2, s1.StartLine);
            Assert.AreEqual(5, s1.EndLine);
            Assert.AreEqual("a", s1.FirstDefinedVariable.Name);
            Assert.AreEqual(Statement.StatementType.Expression, s1.CurrentType);
            Assert.AreEqual(3, refVarNames.Count);
            Assert.AreEqual(true, refVarNames.Contains("d"));
        }

        [Test]
        public void TestStatement_BasicExpressions()
        {
            string code = @"
w = 1+a;
x = b*c/2;
y = a>d || a<e;
z = b &&e; ";
            CodeBlockNode commentNode;
            ProtoCore.AST.Node resultNode;
            List<string> refVarNames = new List<string>();
            resultNode = GraphToDSCompiler.GraphUtilities.Parse(code, out commentNode);
            BinaryExpressionNode ben = (resultNode as CodeBlockNode).Body[0] as BinaryExpressionNode;
            //1
            Statement s1 = Statement.CreateInstance((resultNode as CodeBlockNode).Body[0]);
            refVarNames = Statement.GetReferencedVariableNames(s1, true);
            Assert.AreEqual(2, s1.StartLine);
            Assert.AreEqual("w", s1.FirstDefinedVariable.Name);
            Assert.AreEqual(1, refVarNames.Count);
            Assert.AreEqual(true, refVarNames.Contains("a"));
            //2
            Statement s2 = Statement.CreateInstance((resultNode as CodeBlockNode).Body[1]);
            refVarNames = Statement.GetReferencedVariableNames(s2, true);
            Assert.AreEqual(3, s2.StartLine);
            Assert.AreEqual("x", s2.FirstDefinedVariable.Name);
            Assert.AreEqual(2, refVarNames.Count);
            Assert.AreEqual(true, refVarNames.Contains("c"));
            //3
            Statement s3 = Statement.CreateInstance((resultNode as CodeBlockNode).Body[2]);
            refVarNames = Statement.GetReferencedVariableNames(s3, true);
            Assert.AreEqual(4, s3.StartLine);
            Assert.AreEqual("y", s3.FirstDefinedVariable.Name);
            Assert.AreEqual(4, refVarNames.Count);
            Assert.AreEqual(true, refVarNames.Contains("e"));
            //4
            Statement s4 = Statement.CreateInstance((resultNode as CodeBlockNode).Body[3]);
            refVarNames = Statement.GetReferencedVariableNames(s4, true);
            Assert.AreEqual(5, s4.StartLine);
            Assert.AreEqual("z", s4.FirstDefinedVariable.Name);
            Assert.AreEqual(2, refVarNames.Count);
            Assert.AreEqual(true, refVarNames.Contains("b"));
        }

        [Test]
        public void TestStatement_FunctionCall()
        {
            string code = @"
a = foo(3-b,c*d,e..f..1);";
            CodeBlockNode commentNode;
            ProtoCore.AST.Node resultNode;
            List<string> refVarNames = new List<string>();
            resultNode = GraphToDSCompiler.GraphUtilities.Parse(code, out commentNode);
            BinaryExpressionNode ben = (resultNode as CodeBlockNode).Body[0] as BinaryExpressionNode;
            Statement s1 = Statement.CreateInstance((resultNode as CodeBlockNode).Body[0]);
            refVarNames = Statement.GetReferencedVariableNames(s1, true);
            Assert.AreEqual(2, s1.StartLine);
            Assert.AreEqual(2, s1.EndLine);
            Assert.AreEqual("a", s1.FirstDefinedVariable.Name);
            Assert.AreEqual(Statement.StatementType.Expression, s1.CurrentType);
            Assert.AreEqual(5, refVarNames.Count);
            Assert.AreEqual(true, refVarNames.Contains("f"));
        }

        [Test]
        public void TestStatement_AssignmentTypeSpecifier()
        {
            string code = @"
a : int = 35;";
            CodeBlockNode commentNode;
            ProtoCore.AST.Node resultNode;
            List<string> refVarNames = new List<string>();
            resultNode = GraphToDSCompiler.GraphUtilities.Parse(code, out commentNode);
            BinaryExpressionNode ben = (resultNode as CodeBlockNode).Body[0] as BinaryExpressionNode;
            Statement s1 = Statement.CreateInstance((resultNode as CodeBlockNode).Body[0]);
            refVarNames = Statement.GetReferencedVariableNames(s1, true);
            Assert.AreEqual(2, s1.StartLine);
            Assert.AreEqual(2, s1.EndLine);
            Assert.AreEqual("a", s1.FirstDefinedVariable.Name);
            Assert.AreEqual(Statement.StatementType.Expression, s1.CurrentType);
            Assert.AreEqual(0, refVarNames.Count);
        }

        [Test]
        public void TestStatement_ArrayReference()
        {
            string code = @"
a : int[]..[] = {1,2,3};
b = c[w][x][y][z];";
            CodeBlockNode commentNode;
            ProtoCore.AST.Node resultNode;
            List<string> refVarNames = new List<string>();
            resultNode = GraphToDSCompiler.GraphUtilities.Parse(code, out commentNode);
            BinaryExpressionNode ben = (resultNode as CodeBlockNode).Body[0] as BinaryExpressionNode;
            //1
            Statement s1 = Statement.CreateInstance((resultNode as CodeBlockNode).Body[0]);
            refVarNames = Statement.GetReferencedVariableNames(s1, true);
            Assert.AreEqual(2, s1.StartLine);
            Assert.AreEqual(2, s1.EndLine);
            Assert.AreEqual("a", s1.FirstDefinedVariable.Name);
            Assert.AreEqual(Statement.StatementType.Expression, s1.CurrentType);
            Assert.AreEqual(0, refVarNames.Count);
            //2
            Statement s2 = Statement.CreateInstance((resultNode as CodeBlockNode).Body[1]);
            refVarNames = Statement.GetReferencedVariableNames(s2, true);
            Assert.AreEqual(3, s2.StartLine);
            Assert.AreEqual("b", s2.FirstDefinedVariable.Name);
            Assert.AreEqual(Statement.StatementType.Expression, s2.CurrentType);
            Assert.AreEqual(5, refVarNames.Count);
            Assert.AreEqual(true, refVarNames.Contains("c"));
            Assert.AreEqual(true, refVarNames.Contains("z"));
        }
#endif
        [Test]
        public void TestSemiColonAddition()
        {
            string userText, compilableText;
            userText = "a";
            compilableText = CodeBlockUtils.FormatUserText(userText);
            Assert.AreEqual("a;", compilableText);

            userText = "\na\n\n\n";
            compilableText = CodeBlockUtils.FormatUserText(userText);
            Assert.AreEqual("a;", compilableText);

            userText = "a = 1; \n\n b = foo( c,\nd\n,\ne)";
            compilableText = CodeBlockUtils.FormatUserText(userText);
            Assert.AreEqual("a = 1;\n\n b = foo( c,\nd\n,\ne);", compilableText);

            userText = "      a = b-c;\nx = 1+3;\n   ";
            compilableText = CodeBlockUtils.FormatUserText(userText);
            Assert.AreEqual("a = b-c;\nx = 1+3;", compilableText);

            userText = "\n   \n   \n    \n";
            compilableText = CodeBlockUtils.FormatUserText(userText);
            Assert.AreEqual("", compilableText);
        }

        [Test]
        public void TestFormatTextScenarios()
        {
            var before = "1;2;";
            var after = CodeBlockUtils.FormatUserText(before);
            Assert.AreEqual("1;\n2;", after);

            before = "  \t\n  1;2;  \t\n   ";
            after = CodeBlockUtils.FormatUserText(before);
            Assert.AreEqual("1;\n2;", after);

            before = "  a = 1;    b = 2;  \n  \n  ";
            after = CodeBlockUtils.FormatUserText(before);
            Assert.AreEqual("a = 1;\n    b = 2;", after);

            before = "  a = 1;    \nb = 2;  \n  \n  ";
            after = CodeBlockUtils.FormatUserText(before);
            Assert.AreEqual("a = 1;\nb = 2;", after);
        }

        [Test]
        public void Defect_MAGN_1045()
        {
            // Create the initial code block node.
            var codeBlockNode = CreateCodeBlockNode();

            // Before code changes, there should be no in/out ports.
            Assert.AreEqual(0, codeBlockNode.InPortData.Count);
            Assert.AreEqual(0, codeBlockNode.OutPortData.Count);

            // After code changes, there should be two output ports.
            UpdateCodeBlockNodeContent(codeBlockNode, "a = 1..6;\na[2]=a[2] + 1;");
            Assert.AreEqual(0, codeBlockNode.InPortData.Count);
            Assert.AreEqual(2, codeBlockNode.OutPortData.Count);
        }

        [Test]
        public void Defect_MAGN_784()
        {
            string openPath = Path.Combine(GetTestDirectory(), @"core\dsevaluation\Defect_MAGN_784.dyn");
            Controller.DynamoViewModel.OpenCommand.Execute(openPath);

            Assert.IsFalse(Controller.DynamoModel.CurrentWorkspace.CanUndo);
            Assert.IsFalse(Controller.DynamoModel.CurrentWorkspace.CanRedo);
        }

        [Test]
        public void Defect_MAGN_3244()
        {
            // Create the initial code block node.
            var codeBlockNode = CreateCodeBlockNode();
            UpdateCodeBlockNodeContent(codeBlockNode, @"point.ByCoordinates(0,0,0);");
            
            // Check
            Assert.AreEqual(1, codeBlockNode.InPortData.Count);

            // Update the code block node
            UpdateCodeBlockNodeContent(codeBlockNode, @"Point.ByCoordinates(0,0,0);");

            // Check
            Assert.AreEqual(0, codeBlockNode.InPortData.Count);
        }

        [Test]
        public void Defect_MAGN_3244_extended()
        {
            //This is to test if the code block node has errors, the connectors are still
            //there. And if the variables in the code block node are defined, the connectors
            //will disappear.

            // Create the first code block node.
            var codeBlockNode0 = CreateCodeBlockNode();
            UpdateCodeBlockNodeContent(codeBlockNode0, @"x=y;");

            // Create the second code block node.
            var codeBlockNode1 = CreateCodeBlockNode();
            UpdateCodeBlockNodeContent(codeBlockNode1, @"1;");

            // Connect the two nodes
            var workspace = Controller.DynamoModel.CurrentWorkspace;
            ConnectorModel connector = ConnectorModel.Make(codeBlockNode1, codeBlockNode0,
                0, 0, PortType.INPUT);
            workspace.Connectors.Add(connector);

            // Update the first code block node to have errors
            UpdateCodeBlockNodeContent(codeBlockNode0, @"x=&y;");

            // Check
            Assert.AreEqual(1, codeBlockNode0.InPortData.Count);

            // Update the first code block node to have y defined
            UpdateCodeBlockNodeContent(codeBlockNode0, "y=1;\nx=y;");

            // Check
            Assert.AreEqual(0, codeBlockNode0.InPortData.Count);
        }

        [Test]
        public void Defect_MAGN_3580()
        {
            // Create the initial code block node.
            var codeBlockNode0 = CreateCodeBlockNode();
            UpdateCodeBlockNodeContent(codeBlockNode0, @"true;");

            // Create the watch node.
            var nodeGuid = Guid.NewGuid();
            var command = new DynCmd.CreateNodeCommand(
                nodeGuid, "Watch", 0, 0, true, false);

            Controller.DynamoViewModel.ExecuteCommand(command);
            var workspace = Controller.DynamoModel.CurrentWorkspace;
            var watchNode = workspace.NodeFromWorkspace<Watch>(nodeGuid);

            // Connect the two nodes
            ConnectorModel connector0 = ConnectorModel.Make(codeBlockNode0, watchNode,
                0, 0, PortType.INPUT);
            workspace.Connectors.Add(connector0);

            // Run
            Assert.DoesNotThrow(() => Controller.RunExpression(null));

            // Update the code block node
            UpdateCodeBlockNodeContent(codeBlockNode0, @"truuuue;");

            // Check
            Assert.AreEqual(1, codeBlockNode0.InPortData.Count);

            // Create the second code block node
            var codeBlockNode1 = CreateCodeBlockNode();
            UpdateCodeBlockNodeContent(codeBlockNode1, @"false;");

            // Connect the two code block nodes
            ConnectorModel connector1 = ConnectorModel.Make(codeBlockNode1, codeBlockNode0,
                0, 0, PortType.INPUT);
            workspace.Connectors.Add(connector1);

            // Run
            Assert.DoesNotThrow(() => Controller.RunExpression(null));

            UpdateCodeBlockNodeContent(codeBlockNode0, @"true;");

            // Check
            Assert.AreEqual(0, codeBlockNode0.InPortData.Count);

            // Run
            Assert.DoesNotThrow(() => Controller.RunExpression(null));

            // Delete the first code block node
            List<ModelBase> nodes = new List<ModelBase>();
            nodes.Add(codeBlockNode0);
            Controller.DynamoModel.DeleteModelInternal(nodes);

            // Undo
            workspace.Undo();
        }

        [Test]
        public void Defect_MAGN_3599()
        {
            // Create the initial code block node.
            var codeBlockNode = CreateCodeBlockNode();
            UpdateCodeBlockNodeContent(codeBlockNode, @"Circle.ByCenterPointRadius(pt,5)");

            // Create the Point.Origin node.
            var nodeGuid = Guid.NewGuid();
            var command = new DynCmd.CreateNodeCommand(
                nodeGuid, "Point.Origin", 0, 0, true, false);

            Controller.DynamoViewModel.ExecuteCommand(command);
            var workspace = Controller.DynamoModel.CurrentWorkspace;
            var pointOriginNode = workspace.NodeFromWorkspace<DSFunction>(nodeGuid);

            // Connect the two nodes
            ConnectorModel connector = ConnectorModel.Make(pointOriginNode, codeBlockNode,
                0, 0, PortType.INPUT);
            workspace.Connectors.Add(connector);

            Assert.AreEqual(1, codeBlockNode.InPortData.Count);

            // Update the code block node
            UpdateCodeBlockNodeContent(codeBlockNode, "pt = Point.ByCoordinates(0,0,0);\nCircle.ByCenterPointRadius(pt,5)");

            Assert.AreEqual(0, codeBlockNode.InPortData.Count);
        }

        #region CodeBlockUtils Specific Tests

        [Test]
        public void GenerateInputPortData00()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                // Null as argument will cause exception.
                CodeBlockUtils.GenerateInputPortData(null);
            });
        }

        [Test]
        public void GenerateInputPortData01()
        {
            // Empty list of input should return empty result.
            var unboundIdentifiers = new List<string>();
            var data = CodeBlockUtils.GenerateInputPortData(unboundIdentifiers);
            Assert.IsNotNull(data);
            Assert.AreEqual(0, data.Count());
        }

        [Test]
        public void GenerateInputPortData02()
        {
            var unboundIdentifiers = new List<string>();
            unboundIdentifiers.Add("ShortVarName");
            unboundIdentifiers.Add("LongerVariableNameThatWillGetTruncated");

            var data = CodeBlockUtils.GenerateInputPortData(unboundIdentifiers);
            Assert.IsNotNull(data);
            Assert.AreEqual(2, data.Count());

            var data0 = data.ElementAt(0);
            Assert.AreEqual(unboundIdentifiers[0], data0.NickName);
            Assert.AreEqual(unboundIdentifiers[0], data0.ToolTipString);

            var data1 = data.ElementAt(1);
            Assert.AreEqual("LongerVariableNameTha...", data1.NickName);
            Assert.AreEqual(unboundIdentifiers[1], data1.ToolTipString);
        }

        [Test]
        public void GetStatementVariables00()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                // Null as argument will cause exception.
                CodeBlockUtils.GetStatementVariables(null, true);
            });
        }

        [Test]
        public void GetStatementVariables01()
        {
            // Create a statement of "Value = 1234".
            var leftNode = new IdentifierNode("Value");
            var rightNode = new IntNode(1234);
            var binExprNode = new BinaryExpressionNode(
                leftNode, rightNode, Operator.assign);

            var statements = new List<Statement>()
            {
                Statement.CreateInstance(binExprNode)
            };

            var vars = CodeBlockUtils.GetStatementVariables(statements, true);
            Assert.IsNotNull(vars);
            Assert.AreEqual(1, vars.Count());

            var variables = vars.ElementAt(0);
            Assert.IsNotNull(variables);
            Assert.AreEqual(1, variables.Count());
            Assert.AreEqual("Value", variables.ElementAt(0));
        }

        [Test]
        public void StatementRequiresOutputPort00()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                // Null as argument will cause exception.
                CodeBlockUtils.DoesStatementRequireOutputPort(null, 0);
            });

            // Create a list of another empty list.
            var svs = new List<List<string>>() { new List<string>() };

            Assert.Throws<IndexOutOfRangeException>(() =>
            {
                // -1 as index argument will cause exception.
                CodeBlockUtils.DoesStatementRequireOutputPort(svs, -1);
            });

            Assert.Throws<IndexOutOfRangeException>(() =>
            {
                // Out-of-bound index argument will cause exception.
                CodeBlockUtils.DoesStatementRequireOutputPort(svs, 1);
            });
        }

        [Test]
        public void StatementRequiresOutputPort01()
        {
            var svs = new List<List<string>>(); // An empty list should return false.
            Assert.IsFalse(CodeBlockUtils.DoesStatementRequireOutputPort(svs, 0));
        }

        [Test]
        public void StatementRequiresOutputPort02()
        {
            var svs = new List<List<string>>()
            {
                new List<string>()
                {
                    "Apple", "Orange"
                },

                new List<string>()
                {
                    "Watermelon", "Grape", "HoneyDew"
                },

                new List<string>()
                {
                    "Lemon", "Apple"
                }
            };

            // "Apple" is redefined on the last statement, no port for first statement.
            Assert.IsFalse(CodeBlockUtils.DoesStatementRequireOutputPort(svs, 0));

            // None of the variables on statement 1 is redefined, so show output port.
            Assert.IsTrue(CodeBlockUtils.DoesStatementRequireOutputPort(svs, 1));

            // The last line will display an output port as long as it defines variable.
            Assert.IsTrue(CodeBlockUtils.DoesStatementRequireOutputPort(svs, 2));
        }

        [Test]
        public void TestMapLogicalToVisualLineIndices00()
        {
            var firstResult = CodeBlockUtils.MapLogicalToVisualLineIndices(null);
            Assert.IsNotNull(firstResult);
            Assert.AreEqual(0, firstResult.Count());

            var secondResult = CodeBlockUtils.MapLogicalToVisualLineIndices("");
            Assert.IsNotNull(secondResult);
            Assert.AreEqual(0, secondResult.Count());
        }

        [Test]
        public void TestMapLogicalToVisualLineIndices01()
        {
            var code = "point = Point.ByCoordinates(1, 2, 3);";
            var maps = CodeBlockUtils.MapLogicalToVisualLineIndices(code);

            Assert.IsNotNull(maps);
            Assert.AreEqual(1, maps.Count());
            Assert.AreEqual(0, maps.ElementAt(0));
        }

        [Test]
        public void TestMapLogicalToVisualLineIndices02()
        {
            var code = "start = Point.ByCoordinates(1, 2, 3);\n" +
                "end = Point.ByCoordinates(10, 20, 30);";

            var maps = CodeBlockUtils.MapLogicalToVisualLineIndices(code);

            Assert.IsNotNull(maps);
            Assert.AreEqual(2, maps.Count());
            Assert.AreEqual(0, maps.ElementAt(0));
            Assert.AreEqual(1, maps.ElementAt(1));
        }

        [Test]
        public void TestMapLogicalToVisualLineIndices03()
        {
            var code = "firstLine = Line.ByStartPointEndPoint(" +
                "Point.ByCoordinates(0, 0, 0), " +
                "Point.ByCoordinates(10, 20, 30));\n" +
                "\n" +
                "secondLine = Line.ByStartPointEndPoint(" +
                "Point.ByCoordinates(10, 20, 30), " +
                "Point.ByCoordinates(40, 50, 60));\n";

            var maps = CodeBlockUtils.MapLogicalToVisualLineIndices(code);

            Assert.IsNotNull(maps);
            Assert.AreEqual(4, maps.Count()); // Note the empty last line.
            Assert.AreEqual(0, maps.ElementAt(0));
            Assert.AreEqual(2, maps.ElementAt(1));
            Assert.AreEqual(3, maps.ElementAt(2));
            Assert.AreEqual(5, maps.ElementAt(3));
        }

        [Test]
        public void TestMapLogicalToVisualLineIndices04()
        {
            var code = "firstLine = Line.ByStartPointEndPoint(" +
                "Point.ByCoordinatesinates(0, 0, 0), " +
                "Point.ByCoordinatesinates(10, 20, 30));\n" +
                "\n" +
                "xCoordinates0 = 10;\n" +
                "yCoordinates0 = 20;\n" +
                "zCoordinates0 = 30;\n" +
                "\n" +
                "\n" +
                "xCoordinates1 = 40;\n" +
                "yCoordinates1 = 50;\n" +
                "zCoordinates1 = 60;\n" +
                "\n" +
                "secondLine = Line.ByStartPointEndPoint(" +
                "Point.ByCoordinatesinates(xCoordinates0, yCoordinates0, zCoordinates0), " +
                "Point.ByCoordinatesinates(xCoordinates1, yCoordinates1, zCoordinates1));\n" +
                "\n" +
                "\n" +
                "sp = firstLine.StartPoint;\n" +
                "ep = firstLine.EndPoint;\n";

            var maps = CodeBlockUtils.MapLogicalToVisualLineIndices(code);

            Assert.IsNotNull(maps);
            Assert.AreEqual(17, maps.Count()); // Note the empty last line.
            Assert.AreEqual(0, maps.ElementAt(0));
            Assert.AreEqual(2, maps.ElementAt(1));
            Assert.AreEqual(3, maps.ElementAt(2));
            Assert.AreEqual(4, maps.ElementAt(3));
            Assert.AreEqual(5, maps.ElementAt(4));
            Assert.AreEqual(6, maps.ElementAt(5));
            Assert.AreEqual(7, maps.ElementAt(6));
            Assert.AreEqual(8, maps.ElementAt(7));
            Assert.AreEqual(9, maps.ElementAt(8));
            Assert.AreEqual(10, maps.ElementAt(9));
            Assert.AreEqual(11, maps.ElementAt(10));
            Assert.AreEqual(12, maps.ElementAt(11));
            Assert.AreEqual(15, maps.ElementAt(12));
            Assert.AreEqual(16, maps.ElementAt(13));
            Assert.AreEqual(17, maps.ElementAt(14));
            Assert.AreEqual(18, maps.ElementAt(15));
        }

        #endregion

        private CodeBlockNodeModel CreateCodeBlockNode()
        {
            var nodeGuid = Guid.NewGuid();
            var command = new DynCmd.CreateNodeCommand(
                nodeGuid, "Code Block", 0, 0, true, false);

            Controller.DynamoViewModel.ExecuteCommand(command);
            var workspace = Controller.DynamoModel.CurrentWorkspace;
            var cbn = workspace.NodeFromWorkspace<CodeBlockNodeModel>(nodeGuid);

            Assert.IsNotNull(cbn);
            return cbn;
        }

        private void UpdateCodeBlockNodeContent(CodeBlockNodeModel cbn, string value)
        {
            var command = new DynCmd.UpdateModelValueCommand(cbn.GUID, "Code", value);
            Controller.DynamoViewModel.ExecuteCommand(command);
        }
    }
}
