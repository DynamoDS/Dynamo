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


namespace Dynamo.Tests
{
    class CodeBlockNodeTests : DynamoUnitTest
    {
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
        public void TestStatement_FunctionDef()
        {
            string code = @"
def foo()
{
a = 5;
return = a;
}";
            Guid tempGuid = new Guid();
            CodeBlockNode commentNode;
            ProtoCore.AST.Node resultNode;
            List<string> topLevelRefVar = new List<string>();
            List<string> allRefVar = new List<string>();
            resultNode = GraphToDSCompiler.GraphUtilities.Parse(code, out commentNode);
            BinaryExpressionNode ben = (resultNode as CodeBlockNode).Body[0] as BinaryExpressionNode;
            Statement s1 = Statement.CreateInstance((resultNode as CodeBlockNode).Body[0], tempGuid);
            topLevelRefVar = Statement.GetReferencedVariableNames(s1, true);
            allRefVar = Statement.GetReferencedVariableNames(s1, false);
            Assert.AreEqual(2, s1.StartLine);
            Assert.AreEqual(6, s1.EndLine);
            Assert.AreEqual(null, s1.DefinedVariable);
            Assert.AreEqual(Statement.StatementType.FuncDeclaration, s1.CurrentType);
            Assert.AreEqual(0, topLevelRefVar.Count);
            Assert.AreEqual(1, allRefVar.Count);
            Assert.AreEqual("a", allRefVar[0]);

            code = @"def foo() = 10;";
            resultNode = GraphToDSCompiler.GraphUtilities.Parse(code, out commentNode);
            ben = (resultNode as CodeBlockNode).Body[0] as BinaryExpressionNode;
            s1 = Statement.CreateInstance((resultNode as CodeBlockNode).Body[0], tempGuid);
            topLevelRefVar = Statement.GetReferencedVariableNames(s1, true);
            allRefVar = Statement.GetReferencedVariableNames(s1, false);
            Assert.AreEqual(1, s1.StartLine);
            Assert.AreEqual(1, s1.EndLine);
            Assert.AreEqual(null, s1.DefinedVariable);
            Assert.AreEqual(Statement.StatementType.FuncDeclaration, s1.CurrentType);
            Assert.AreEqual(0, topLevelRefVar.Count);
            Assert.AreEqual(0, allRefVar.Count);
        }

        [Test]
        public void TestStatement_InlineExpression()
        {
            string code = @"
a = 
b>c+5 ? 
d-2 : 
e+f ;";
            Guid tempGuid = new Guid();
            CodeBlockNode commentNode;
            ProtoCore.AST.Node resultNode;
            List<string> refVarNames = new List<string>();
            resultNode = GraphToDSCompiler.GraphUtilities.Parse(code, out commentNode);
            BinaryExpressionNode ben = (resultNode as CodeBlockNode).Body[0] as BinaryExpressionNode;
            Statement s1 = Statement.CreateInstance((resultNode as CodeBlockNode).Body[0], tempGuid);
            refVarNames = Statement.GetReferencedVariableNames(s1, true);
            Assert.AreEqual(2, s1.StartLine);
            Assert.AreEqual(5, s1.EndLine);
            Assert.AreEqual("a", s1.DefinedVariable.Name);
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
#d;";
            Guid tempGuid = new Guid();
            CodeBlockNode commentNode;
            ProtoCore.AST.Node resultNode;
            List<string> refVarNames = new List<string>();
            resultNode = GraphToDSCompiler.GraphUtilities.Parse(code, out commentNode);
            BinaryExpressionNode ben = (resultNode as CodeBlockNode).Body[0] as BinaryExpressionNode;
            Statement s1 = Statement.CreateInstance((resultNode as CodeBlockNode).Body[0], tempGuid);
            refVarNames = Statement.GetReferencedVariableNames(s1, true);
            Assert.AreEqual(2, s1.StartLine);
            Assert.AreEqual(5, s1.EndLine);
            Assert.AreEqual("a", s1.DefinedVariable.Name);
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
            Guid tempGuid = new Guid();
            CodeBlockNode commentNode;
            ProtoCore.AST.Node resultNode;
            List<string> refVarNames = new List<string>();
            resultNode = GraphToDSCompiler.GraphUtilities.Parse(code, out commentNode);
            BinaryExpressionNode ben = (resultNode as CodeBlockNode).Body[0] as BinaryExpressionNode;
            //1
            Statement s1 = Statement.CreateInstance((resultNode as CodeBlockNode).Body[0], tempGuid);
            refVarNames = Statement.GetReferencedVariableNames(s1, true);
            Assert.AreEqual(2, s1.StartLine);
            Assert.AreEqual("w", s1.DefinedVariable.Name);
            Assert.AreEqual(1, refVarNames.Count);
            Assert.AreEqual(true, refVarNames.Contains("a"));
            //2
            Statement s2 = Statement.CreateInstance((resultNode as CodeBlockNode).Body[1], tempGuid);
            refVarNames = Statement.GetReferencedVariableNames(s2, true);
            Assert.AreEqual(3, s2.StartLine);
            Assert.AreEqual("x", s2.DefinedVariable.Name);
            Assert.AreEqual(2, refVarNames.Count);
            Assert.AreEqual(true, refVarNames.Contains("c"));
            //3
            Statement s3 = Statement.CreateInstance((resultNode as CodeBlockNode).Body[2], tempGuid);
            refVarNames = Statement.GetReferencedVariableNames(s3, true);
            Assert.AreEqual(4, s3.StartLine);
            Assert.AreEqual("y", s3.DefinedVariable.Name);
            Assert.AreEqual(4, refVarNames.Count);
            Assert.AreEqual(true, refVarNames.Contains("e"));
            //4
            Statement s4 = Statement.CreateInstance((resultNode as CodeBlockNode).Body[3], tempGuid);
            refVarNames = Statement.GetReferencedVariableNames(s4, true);
            Assert.AreEqual(5, s4.StartLine);
            Assert.AreEqual("z", s4.DefinedVariable.Name);
            Assert.AreEqual(2, refVarNames.Count);
            Assert.AreEqual(true, refVarNames.Contains("b"));
        }

        [Test]
        public void TestStatement_FunctionCall()
        {
            string code = @"
a = foo(3-b,c*d,e..f..1);";
            Guid tempGuid = new Guid();
            CodeBlockNode commentNode;
            ProtoCore.AST.Node resultNode;
            List<string> refVarNames = new List<string>();
            resultNode = GraphToDSCompiler.GraphUtilities.Parse(code, out commentNode);
            BinaryExpressionNode ben = (resultNode as CodeBlockNode).Body[0] as BinaryExpressionNode;
            Statement s1 = Statement.CreateInstance((resultNode as CodeBlockNode).Body[0], tempGuid);
            refVarNames = Statement.GetReferencedVariableNames(s1, true);
            Assert.AreEqual(2, s1.StartLine);
            Assert.AreEqual(2, s1.EndLine);
            Assert.AreEqual("a", s1.DefinedVariable.Name);
            Assert.AreEqual(Statement.StatementType.Expression, s1.CurrentType);
            Assert.AreEqual(5, refVarNames.Count);
            Assert.AreEqual(true, refVarNames.Contains("f"));
        }

        [Test]
        public void TestStatement_AssignmentTypeSpecifier()
        {
            string code = @"
a : int = 35;";
            Guid tempGuid = new Guid();
            CodeBlockNode commentNode;
            ProtoCore.AST.Node resultNode;
            List<string> refVarNames = new List<string>();
            resultNode = GraphToDSCompiler.GraphUtilities.Parse(code, out commentNode);
            BinaryExpressionNode ben = (resultNode as CodeBlockNode).Body[0] as BinaryExpressionNode;
            Statement s1 = Statement.CreateInstance((resultNode as CodeBlockNode).Body[0], tempGuid);
            refVarNames = Statement.GetReferencedVariableNames(s1, true);
            Assert.AreEqual(2, s1.StartLine);
            Assert.AreEqual(2, s1.EndLine);
            Assert.AreEqual("a", s1.DefinedVariable.Name);
            Assert.AreEqual(Statement.StatementType.Expression, s1.CurrentType);
            Assert.AreEqual(0, refVarNames.Count);
        }

        [Test]
        public void TestStatement_ArrayReference()
        {
            string code = @"
a : int[]..[] = {1,2,3};
b = c[w][x][y][z];";
            Guid tempGuid = new Guid();
            CodeBlockNode commentNode;
            ProtoCore.AST.Node resultNode;
            List<string> refVarNames = new List<string>();
            resultNode = GraphToDSCompiler.GraphUtilities.Parse(code, out commentNode);
            BinaryExpressionNode ben = (resultNode as CodeBlockNode).Body[0] as BinaryExpressionNode;
            //1
            Statement s1 = Statement.CreateInstance((resultNode as CodeBlockNode).Body[0], tempGuid);
            refVarNames = Statement.GetReferencedVariableNames(s1, true);
            Assert.AreEqual(2, s1.StartLine);
            Assert.AreEqual(2, s1.EndLine);
            Assert.AreEqual("a", s1.DefinedVariable.Name);
            Assert.AreEqual(Statement.StatementType.Expression, s1.CurrentType);
            Assert.AreEqual(0, refVarNames.Count);
            //2
            Statement s2 = Statement.CreateInstance((resultNode as CodeBlockNode).Body[1], tempGuid);
            refVarNames = Statement.GetReferencedVariableNames(s2, true);
            Assert.AreEqual(3, s2.StartLine);
            Assert.AreEqual("b", s2.DefinedVariable.Name);
            Assert.AreEqual(Statement.StatementType.Expression, s2.CurrentType);
            Assert.AreEqual(5, refVarNames.Count);
            Assert.AreEqual(true, refVarNames.Contains("c"));
            Assert.AreEqual(true, refVarNames.Contains("z"));
        }

        [Test]
        public void TestSemiColonAddition()
        {
            string userText, compilableText;
            userText = "a";
            compilableText = CodeBlockNodeModel.FormatUserText(userText);
            Assert.AreEqual("a;", compilableText);

            userText = "\na\n\n\n";
            compilableText = CodeBlockNodeModel.FormatUserText(userText);
            Assert.AreEqual("a;", compilableText);

            userText = "a = 1; \n\n b = foo( c,\nd\n,\ne)";
            compilableText = CodeBlockNodeModel.FormatUserText(userText);
            Assert.AreEqual("a = 1; \n\n b = foo( c,\nd\n,\ne);", compilableText);

            userText = "      a = b-c;\nx = 1+3;\n   ";
            compilableText = CodeBlockNodeModel.FormatUserText(userText);
            Assert.AreEqual("a = b-c;\nx = 1+3;", compilableText);

            userText = "\n   \n   \n    \n";
            compilableText = CodeBlockNodeModel.FormatUserText(userText);
            Assert.AreEqual("", compilableText);
        }

        [Test]
        [Category("DSExecution")]
        [Category("Regression")]
        public void RegressMAGN592()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\cbn\magn592.dyn");
            model.Open(openPath);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(null));

            // a = 1..10;
            // b = (1..10..1)..5;
            // v1 = Count(a);
            // v2 = Count(Flatten(b));
            // v3 = v1 + v2; 
            // 
            // To verify v3 == 45;

            var node = model.CurrentWorkspace.NodeFromWorkspace("9ee31295-11a0-450d-951d-ed61ad9ce159");
            string var = node.VariableToPreview;
            RuntimeMirror mirror = null;
            Assert.DoesNotThrow(() => mirror = EngineController.Instance.GetMirror(var));
            Assert.IsNotNull(mirror);

            StackValue value = mirror.GetData().GetStackValue();
            Assert.AreEqual(value.opdata, 45);
        }
    }
}

