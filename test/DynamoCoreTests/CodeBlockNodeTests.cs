using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Dynamo.DSEngine.CodeCompletion;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.UI;
using Dynamo.Utilities;
using NUnit.Framework;

using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using ProtoCore.Mirror;
using ProtoCore.Utils;

using DynCmd = Dynamo.Models.DynamoModel;

namespace Dynamo.Tests
{
    class CodeBlockNodeTests : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DSCoreNodes.dll");

            base.GetLibrariesToPreload(libraries);
        }

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
        protected double tolerance = 1e-4;

        [Test]
        [Category("UnitTests")]
        public void TestDefenitionLineIndexMap()
        {
            var codeBlockNodeOne = CreateCodeBlockNode();

            var indexMap = CodeBlockUtils.GetDefinitionLineIndexMap(codeBlockNodeOne.CodeStatements);

            Assert.IsNotNull(indexMap);
            Assert.IsEmpty(indexMap);

            UpdateCodeBlockNodeContent(codeBlockNodeOne, "a = 0;");

            indexMap = CodeBlockUtils.GetDefinitionLineIndexMap(codeBlockNodeOne.CodeStatements);
            Assert.AreEqual("a", indexMap.ElementAt(0).Key);
            Assert.AreEqual(1, indexMap.ElementAt(0).Value);

            UpdateCodeBlockNodeContent(codeBlockNodeOne, "a = 0; \n a = 1;");

            indexMap = CodeBlockUtils.GetDefinitionLineIndexMap(codeBlockNodeOne.CodeStatements);
            Assert.AreEqual("a", indexMap.ElementAt(0).Key);
            Assert.AreEqual(2, indexMap.ElementAt(0).Value);

            UpdateCodeBlockNodeContent(codeBlockNodeOne, "a = 0; \n b = 1; \n a = 2;");

            indexMap = CodeBlockUtils.GetDefinitionLineIndexMap(codeBlockNodeOne.CodeStatements);
            Assert.AreEqual("b", indexMap.ElementAt(0).Key);
            Assert.AreEqual(2, indexMap.ElementAt(0).Value);
            Assert.AreEqual("a", indexMap.ElementAt(1).Key);
            Assert.AreEqual(3, indexMap.ElementAt(1).Value);

        }

        [Test]
        [Category("RegressionTests")]
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
        [Category("RegressionTests")]
        public void Defect_MAGN_4024()
        {
            // Create the initial code block node.
            var codeBlockNodeOne = CreateCodeBlockNode();
            UpdateCodeBlockNodeContent(codeBlockNodeOne, "arr = 20 .. 29;");

            // We should have one code block node by now.
            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            // Copy and paste the code block node.
            CurrentDynamoModel.AddToSelection(codeBlockNodeOne);
            CurrentDynamoModel.Copy(); // Copy the selected node.
            CurrentDynamoModel.Paste(); // Paste the copied node.

            // After pasting, we should have two nodes.
            Assert.AreEqual(2, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            // Make sure we are able to get the second code block node.
            var codeBlockNodeTwo = CurrentDynamoModel.CurrentWorkspace.Nodes[1] as CodeBlockNodeModel;
            Assert.IsNotNull(codeBlockNodeTwo);

            // The preview identifier should be named as "arr_GUID" (the prefix 
            // "arr" is derived from the named variable in the code block node).
            // 
            var guid = codeBlockNodeTwo.GUID.ToString();
            var expectedIdentifier = "arr_" + guid.Replace("-", string.Empty);
            Assert.AreEqual(expectedIdentifier, codeBlockNodeTwo.AstIdentifierBase);
        }

        [Test]
        public void TestOutportConnectors_OnAssigningVariables_ToRetainConnections()
        {
            string openPath = Path.Combine(TestDirectory,
                @"core\dsevaluation\TestOutportConnectors_OnAssigningVariables_ToRetainConnections.dyn");
            OpenModel(openPath);
            Assert.AreEqual(2, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);

            BeginRun();

            var result = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<DSFunction>(
                Guid.Parse("76b2289a-a814-44fc-97b1-397f8abea296"));

            Assert.AreEqual(-10, result.CachedValue.Data);

            var cbn = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<CodeBlockNodeModel>(
                Guid.Parse("3cd6cdb7-9e5c-4b61-bc8b-630e48b52fc0"));

            Assert.IsNotNull(cbn);
            string code = "a=10;20;";
            UpdateCodeBlockNodeContent(cbn, code);

            BeginRun();

            Assert.AreEqual(-10, result.CachedValue.Data);
        }

        [Test]
        [Category("RegressionTests")]
        public void Defect_MAGN_4946()
        {
            int value = 10;
            string codeInCBN = "a = " + value;

            // Create the initial code block node.
            var codeBlockNodeOne = CreateCodeBlockNode();
            UpdateCodeBlockNodeContent(codeBlockNodeOne, codeInCBN);

            // We should have one code block node by now.
            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            // Run 
            BeginRun();

            // Get preview data given AstIdentifierBase
            var core = CurrentDynamoModel.EngineController.LiveRunnerRuntimeCore;
            var runtimeMirror = new RuntimeMirror(codeBlockNodeOne.AstIdentifierBase, 0, core);
            MirrorData mirrorData = runtimeMirror.GetData();
            Assert.AreEqual(mirrorData.Data, value);

            // Copy and paste the code block node.
            CurrentDynamoModel.AddToSelection(codeBlockNodeOne);
            CurrentDynamoModel.Copy(); // Copy the selected node.
            CurrentDynamoModel.Paste(); // Paste the copied node.

            // After pasting, we should have two nodes.
            Assert.AreEqual(2, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            // Make sure we are able to get the second code block node.
            var codeBlockNodeTwo = CurrentDynamoModel.CurrentWorkspace.Nodes[1] as CodeBlockNodeModel;
            Assert.IsNotNull(codeBlockNodeTwo);

            // Run 
            BeginRun();

            // Get preview data given AstIdentifierBase
            runtimeMirror = new RuntimeMirror(codeBlockNodeTwo.AstIdentifierBase, 0, core);
            mirrorData = runtimeMirror.GetData();
            Assert.AreEqual(mirrorData.Data, value);
        }

        [Test]
        [Category("RegressionTests")]
        public void Defect_MAGN_784()
        {
            string openPath = Path.Combine(TestDirectory, @"core\dsevaluation\Defect_MAGN_784.dyn");
            OpenModel(openPath);

            Assert.IsFalse(CurrentDynamoModel.CurrentWorkspace.CanUndo);
            Assert.IsFalse(CurrentDynamoModel.CurrentWorkspace.CanRedo);
        }

        [Test]
        [Category("RegressionTests")]
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
        [Category("RegressionTests")]
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
            ConnectorModel.Make(codeBlockNode1, codeBlockNode0, 0, 0);

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
        [Category("RegressionTests")]
        public void Defect_MAGN_3580()
        {
            // Create the initial code block node.
            var codeBlockNode0 = CreateCodeBlockNode();
            UpdateCodeBlockNodeContent(codeBlockNode0, @"true;");

            // Create the watch node.
            var watch = new Watch();
            var command = new DynCmd.CreateNodeCommand(
                watch, 0, 0, true, false);

            CurrentDynamoModel.ExecuteCommand(command);
            var workspace = CurrentDynamoModel.CurrentWorkspace;

            // Connect the two nodes
            ConnectorModel.Make(codeBlockNode0, watch, 0, 0);

            // Run
            Assert.DoesNotThrow(BeginRun);

            // Update the code block node
            UpdateCodeBlockNodeContent(codeBlockNode0, @"truuuue;");

            // Check
            Assert.AreEqual(1, codeBlockNode0.InPortData.Count);

            // Create the second code block node
            var codeBlockNode1 = CreateCodeBlockNode();
            UpdateCodeBlockNodeContent(codeBlockNode1, @"false;");

            // Connect the two code block nodes
            ConnectorModel.Make(codeBlockNode1, codeBlockNode0, 0, 0);

            // Run
            Assert.DoesNotThrow(BeginRun);

            UpdateCodeBlockNodeContent(codeBlockNode0, @"true;");

            // Check
            Assert.AreEqual(0, codeBlockNode0.InPortData.Count);

            // Run
            Assert.DoesNotThrow(BeginRun);

            // Delete the first code block node
            var nodes = new List<ModelBase> { codeBlockNode0 };
            CurrentDynamoModel.DeleteModelInternal(nodes);

            // Undo
            workspace.Undo();
        }

        [Test]
        [Category("RegressionTests")]
        public void Defect_MAGN_3599()
        {
            // Create the initial code block node.
            var codeBlockNode = CreateCodeBlockNode();
            UpdateCodeBlockNodeContent(codeBlockNode, @"Circle.ByCenterPointRadius(pt,5)");

            // Create the Point.Origin node.
            var pointOriginNode =
                new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("Point.Origin"));

            var command = new DynCmd.CreateNodeCommand(pointOriginNode, 0, 0, true, false);

            CurrentDynamoModel.ExecuteCommand(command);

            // Connect the two nodes
            ConnectorModel.Make(pointOriginNode, codeBlockNode, 0, 0);

            Assert.AreEqual(1, codeBlockNode.InPortData.Count);

            // Update the code block node
            UpdateCodeBlockNodeContent(codeBlockNode, "pt = Point.ByCoordinates(0,0,0);\nCircle.ByCenterPointRadius(pt,5)");

            Assert.AreEqual(0, codeBlockNode.InPortData.Count);
        }

        [Test]
        [Category("RegressionTests")]
        public void OutPort_WithCommentNonAssignment_Alignment()
        {
            // Create the initial code block node.
            var codeBlockNode = CreateCodeBlockNode();
            string code = "// comment \n // comment \n a+b;";

            UpdateCodeBlockNodeContent(codeBlockNode, code);

            Assert.AreEqual(2, codeBlockNode.InPortData.Count);
            Assert.AreEqual(1, codeBlockNode.OutPortData.Count);

            Assert.AreEqual(2 * Configurations.CodeBlockPortHeightInPixels, codeBlockNode.OutPortData[0].VerticalMargin, tolerance);
            
            code = "c+ \n d; \n /* comment \n */ \n a+b;";
            UpdateCodeBlockNodeContent(codeBlockNode, code);

            Assert.AreEqual(4, codeBlockNode.InPortData.Count);
            Assert.AreEqual(2, codeBlockNode.OutPortData.Count);

            // The first output port should be at the first line
            Assert.AreEqual(0 * Configurations.CodeBlockPortHeightInPixels, codeBlockNode.OutPortData[0].VerticalMargin, tolerance);

            // The second output port should be at the 4th line, which is also 3 lines below the first
            Assert.AreEqual(3 * Configurations.CodeBlockPortHeightInPixels, codeBlockNode.OutPortData[1].VerticalMargin, tolerance);
            
            code = "/*comment \n */ \n a[0]+b;";
            UpdateCodeBlockNodeContent(codeBlockNode, code);

            Assert.AreEqual(2, codeBlockNode.InPortData.Count);
            Assert.AreEqual(1, codeBlockNode.OutPortData.Count);

            Assert.AreEqual(2 * Configurations.CodeBlockPortHeightInPixels, codeBlockNode.OutPortData[0].VerticalMargin, tolerance);

        }

        [Test]
        [Category("RegressionTests")]
        public void InPort_WithInlineConditionNonAssignment_Creation()
        {
            // Create the initial code block node.
            var codeBlockNode = CreateCodeBlockNode();
            string code = "c + d; \n z = 2;";

            UpdateCodeBlockNodeContent(codeBlockNode, code);

            Assert.AreEqual(2, codeBlockNode.InPortData.Count);
            Assert.AreEqual(2, codeBlockNode.OutPortData.Count);

            code = "x%2 == 0 ? x : -x; \n y = a+b;";
            UpdateCodeBlockNodeContent(codeBlockNode, code);

            Assert.AreEqual(3, codeBlockNode.InPortData.Count);
            Assert.AreEqual(2, codeBlockNode.OutPortData.Count);

            code = "f(x); \n y = a+b;";
            UpdateCodeBlockNodeContent(codeBlockNode, code);

            Assert.AreEqual(3, codeBlockNode.InPortData.Count);
            Assert.AreEqual(2, codeBlockNode.OutPortData.Count);
        }

        [Test]
        [Category("RegressionTests")]
        public void Parse_NonAssignmentFollowedByAssignment()
        {
            // Create the initial code block node.
            var codeBlockNode = CreateCodeBlockNode();
            string code = "List.IsEmpty(result)?result:List.FirstItem(result);";

            UpdateCodeBlockNodeContent(codeBlockNode, code);

            Assert.AreEqual(1, codeBlockNode.InPortData.Count);
            Assert.AreEqual(1, codeBlockNode.OutPortData.Count);

            code = "x%2 == 0 ? x : -x;";
            UpdateCodeBlockNodeContent(codeBlockNode, code);

            Assert.AreEqual(1, codeBlockNode.InPortData.Count);
            Assert.AreEqual(1, codeBlockNode.OutPortData.Count);
        }

        [Test]
        [Category("RegressionTests")]
        public void Parse_TypedVariableDeclaration()
        {
            // Create the initial code block node.
            var codeBlockNode = CreateCodeBlockNode();
            string code = "a : int;";

            UpdateCodeBlockNodeContent(codeBlockNode, code);

            Assert.AreEqual(1, codeBlockNode.InPortData.Count);
            Assert.AreEqual(1, codeBlockNode.OutPortData.Count);

            code = "a : int  = 2;";
            UpdateCodeBlockNodeContent(codeBlockNode, code);

            Assert.AreEqual(0, codeBlockNode.InPortData.Count);
            Assert.AreEqual(1, codeBlockNode.OutPortData.Count);
        }

        [Test]
        public void Defect_MAGN_6723()
        {
            var codeBlockNode = CreateCodeBlockNode();
            string code = @"x + ""anyString"";";

            UpdateCodeBlockNodeContent(codeBlockNode, code);
            Assert.AreEqual(1, codeBlockNode.InPortData.Count);
        }

        #region CodeBlockUtils Specific Tests

        [Test]
        [Category("UnitTests")]
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
        [Category("UnitTests")]
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
        [Category("UnitTests")]
        public void TextFormat_CurlyBraces_SemiColonAddedAutomatically()
        {
            var before = "{1,2,3}";
            var after = CodeBlockUtils.FormatUserText(before);
            Assert.AreEqual("{1,2,3};", after);

            before = "{1,2,3};";
            after = CodeBlockUtils.FormatUserText(before);
            Assert.AreEqual("{1,2,3};", after);
        }

        [Test]
        [Category("UnitTests")]
        public void TextFormat_SingleLineComment_NoSemiColonAdded()
        {
            var before = "//comment";
            var after = CodeBlockUtils.FormatUserText(before);
            Assert.AreEqual("//comment", after);
        }

        [Test]
        [Category("UnitTests")]
        public void GenerateInputPortData00()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                // Null as argument will cause exception.
                CodeBlockUtils.GenerateInputPortData(null);
            });
        }

        [Test]
        [Category("UnitTests")]
        public void GenerateInputPortData01()
        {
            // Empty list of input should return empty result.
            var unboundIdentifiers = new List<string>();
            var data = CodeBlockUtils.GenerateInputPortData(unboundIdentifiers);
            Assert.IsNotNull(data);
            Assert.AreEqual(0, data.Count());
        }

        [Test]
        [Category("UnitTests")]
        public void GenerateInputPortData02()
        {
            var unboundIdentifiers = new List<string>
            {
                "ShortVarName",
                "LongerVariableNameThatWillGetTruncated"
            };

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
        [Category("UnitTests")]
        public void GetStatementVariables00()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                // Null as argument will cause exception.
                CodeBlockUtils.GetStatementVariables(null, true);
            });
        }

        [Test]
        [Category("UnitTests")]
        public void GetStatementVariables01()
        {
            // Create a statement of "Value = 1234".
            var leftNode = new IdentifierNode("Value");
            var rightNode = new IntNode(1234);
            var binExprNode = new BinaryExpressionNode(
                leftNode, rightNode, Operator.assign);

            var statements = new List<Statement>
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
        [Category("UnitTests")]
        public void StatementRequiresOutputPort00()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                // Null as argument will cause exception.
                CodeBlockUtils.DoesStatementRequireOutputPort(null, 0);
            });

            // Create a list of another empty list.
            var svs = new List<List<string>> { new List<string>() };

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
        [Category("UnitTests")]
        public void StatementRequiresOutputPort01()
        {
            var svs = new List<List<string>>(); // An empty list should return false.
            Assert.IsFalse(CodeBlockUtils.DoesStatementRequireOutputPort(svs, 0));
        }

        [Test]
        public void StatementRequiresOutputPort02()
        {
            var svs = new List<List<string>>
            {
                new List<string>
                {
                    "Apple", "Orange"
                },

                new List<string>
                {
                    "Watermelon", "Grape", "HoneyDew"
                },

                new List<string>
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
        public void TypedIdentifier_AssignedToDifferentType_ThrowsWarning()
        {
            var model = CurrentDynamoModel;
            string codeInCBN = @"a : int = Point.ByCoordinates();";

            // Create the initial code block node.
            var codeBlockNodeOne = CreateCodeBlockNode();
            UpdateCodeBlockNodeContent(codeBlockNodeOne, codeInCBN);

            // We should have one code block node by now.
            Assert.AreEqual(1, model.CurrentWorkspace.Nodes.Count());

            // Run 
            BeginRun();

            // Get preview data given AstIdentifierBase
            var core = CurrentDynamoModel.EngineController.LiveRunnerRuntimeCore;
            var runtimeMirror = new RuntimeMirror(codeBlockNodeOne.AstIdentifierBase, 0, core);
            MirrorData mirrorData = runtimeMirror.GetData();
            Assert.AreEqual(mirrorData.Data, null);

            // Assert that node throws type mismatch warning
            Assert.IsTrue(codeBlockNodeOne.ToolTipText.Contains(
                ProtoCore.Properties.Resources.kConvertNonConvertibleTypes));
        }

        [Test]
        public void TypedIdentifier_AssignedToDifferentType_ThrowsWarning2()
        {
            string openPath = Path.Combine(TestDirectory,
                @"core\dsevaluation\typedIdentifier_warning.dyn");
            OpenModel(openPath);
            Assert.AreEqual(2, CurrentDynamoModel.CurrentWorkspace.Nodes.Count);

            BeginRun();

            var node = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<CodeBlockNodeModel>(
                Guid.Parse("17d2f866-dc5a-43ef-b3c5-ac7474d16467"));

            Assert.IsNotNull(node);
            Assert.AreEqual(null, node.CachedValue.Data);

            // Assert that node throws type mismatch warning
            Assert.IsTrue(node.ToolTipText.Contains(
                ProtoCore.Properties.Resources.kConvertNonConvertibleTypes));
        }

        #endregion

        #region Codeblock Namespace Resolution Tests

        [Test]
        public void Resolve_NamespaceConflict_LoadLibrary()
        {
            string code = "Point.ByCoordinates(10,0,0);";

            var cbn = CreateCodeBlockNode();

            UpdateCodeBlockNodeContent(cbn, code);
            Assert.AreEqual(1, cbn.OutPortData.Count);

            // FFITarget introduces conflicts with Point class in
            // FFITarget.Dummy.Point, FFITarget.Dynamo.Point
            const string libraryPath = "FFITarget.dll";

            CompilerUtils.TryLoadAssemblyIntoCore(
                CurrentDynamoModel.LibraryServices.LibraryManagementCore, libraryPath);

            code = "Point.ByCoordinates(0,0,0);";
            UpdateCodeBlockNodeContent(cbn, code);
            Assert.AreEqual(0, CurrentDynamoModel.LibraryServices.LibraryManagementCore.BuildStatus.Warnings.Count());
        }

        #endregion


        private CodeBlockNodeModel CreateCodeBlockNode()
        {
            var cbn = new CodeBlockNodeModel(CurrentDynamoModel.LibraryServices);
            var command = new DynCmd.CreateNodeCommand(cbn, 0, 0, true, false);

            CurrentDynamoModel.ExecuteCommand(command);

            Assert.IsNotNull(cbn);
            return cbn;
        }

        private void UpdateCodeBlockNodeContent(CodeBlockNodeModel cbn, string value)
        {
            var command = new DynCmd.UpdateModelValueCommand(Guid.Empty, cbn.GUID, "Code", value);
            CurrentDynamoModel.ExecuteCommand(command);
        }
    }


    public class CodeBlockCompletionTests
    {
        private ProtoCore.Core libraryServicesCore;

        [SetUp]
        public void Init()
        {
            string libraryPath = "FFITarget.dll";

            var options = new ProtoCore.Options();
            options.RootModulePathName = string.Empty;

            libraryServicesCore = new ProtoCore.Core(options);
            libraryServicesCore.Compilers.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Compiler(libraryServicesCore));
            libraryServicesCore.Compilers.Add(ProtoCore.Language.kImperative, new ProtoImperative.Compiler(libraryServicesCore));

            CompilerUtils.TryLoadAssemblyIntoCore(libraryServicesCore, libraryPath);
        }

        [TearDown]
        public void Cleanup()
        {
            if (libraryServicesCore != null)
            {
                libraryServicesCore.Cleanup();
                libraryServicesCore = null;
            }
        }

        [Test]
        [Category("UnitTests")]
        public void TestClassMemberCompletion()
        {
            string ffiTargetClass = "CodeCompletionClass";

            // Assert that the class name is indeed a class
            ClassMirror type = null;
            Assert.DoesNotThrow(() => type = new ClassMirror(ffiTargetClass, libraryServicesCore));

            var members = type.GetMembers();

            var expected = new[] { "CodeCompletionClass", "StaticFunction", "StaticProp" };
            AssertCompletions(members, expected);
        }

        [Test]
        [Category("UnitTests")]
        public void TestInstanceMemberCompletion()
        {
            string ffiTargetClass = "CodeCompletionClass";

            // Assert that the class name is indeed a class
            ClassMirror type = null;
            Assert.DoesNotThrow(() => type = new ClassMirror(ffiTargetClass, libraryServicesCore));

            var members = type.GetInstanceMembers();

            var expected = new[] { "AddWithValueContainer", "ClassProperty", 
                "IntVal", "IsEqualTo", "OverloadedAdd" };
            AssertCompletions(members, expected);
        }

        [Test]
        [Category("UnitTests")]
        public void TestCodeCompletionParser()
        {
            string code = @"x[y[z.foo()].goo()].bar";
            string actual = CodeCompletionParser.GetStringToComplete(code);
            string expected = "x[y[z.foo()].goo()].bar";
            Assert.AreEqual(expected, actual);

            code = @"abc.X[xyz.foo().Y";
            actual = CodeCompletionParser.GetStringToComplete(code);
            expected = "xyz.foo().Y";
            Assert.AreEqual(expected, actual);

            code = @"pnt[9][0] = abc.X[{xyz.b.foo((abc";
            actual = CodeCompletionParser.GetStringToComplete(code);
            expected = "abc";
            Assert.AreEqual(expected, actual);

            code = @"pnt[9][0] = abc.X[{xyz.b.foo((abc*x";
            actual = CodeCompletionParser.GetStringToComplete(code);
            expected = "x";
            Assert.AreEqual(expected, actual);

            code = @"w = abc; w = xyz; w = xyz.b; w = xyz.b.foo; w = xyz.b.foo.Y";
            actual = CodeCompletionParser.GetStringToComplete(code);
            expected = "xyz.b.foo.Y";
            Assert.AreEqual(expected, actual);
        }

        [Test]
        [Category("UnitTests")]
        public void TestCodeCompletionParserForFunctions()
        {
            string code = @"x[y[z.foo()].goo(";
            string functionName;
            string functionPrefix;
            CodeCompletionParser.GetFunctionToComplete(code, out functionName, out functionPrefix);
            Assert.AreEqual("goo", functionName);
            Assert.AreEqual("y[z.foo()]", functionPrefix);

            code = @"abc.X[xyz.foo(";
            CodeCompletionParser.GetFunctionToComplete(code, out functionName, out functionPrefix);
            Assert.AreEqual("foo", functionName);
            Assert.AreEqual("xyz", functionPrefix);

            code = @"pnt[9][0] = abc.X[{xyz.b.foo(";
            CodeCompletionParser.GetFunctionToComplete(code, out functionName, out functionPrefix);
            Assert.AreEqual("foo", functionName);
            Assert.AreEqual("xyz.b", functionPrefix);

            code = @"foo(";
            CodeCompletionParser.GetFunctionToComplete(code, out functionName, out functionPrefix);
            Assert.AreEqual("foo", functionName);
            Assert.AreEqual("", functionPrefix);

        }

        [Test]
        [Category("UnitTests")]
        public void TestCodeCompletionParserForVariableType()
        {
            string code = "a : Point;";
            string variableName = "a";

            Assert.AreEqual("Point", CodeCompletionParser.GetVariableType(code, variableName));

            code = @"a : Point = Point.ByCoordinates();";
            Assert.AreEqual("Point", CodeCompletionParser.GetVariableType(code, variableName));
        }

        [Test]
        [Category("UnitTests")]
        public void TestCodeCompletionForFullyQualifiedVariableType()
        {
            string code = "a : FFITarget.FirstNamespace.ClassWithNameConflict;";
            string variableName = "a";

            string type1 = CodeCompletionParser.GetVariableType(code, variableName);
            Assert.AreEqual("FFITarget.FirstNamespace.ClassWithNameConflict", type1);

            // Assert that the class name is indeed a class
            ClassMirror type = null;
            Assert.DoesNotThrow(() => type = new ClassMirror(type1, libraryServicesCore));

            var members = type.GetInstanceMembers();

            var expected = new[] { "PropertyA", "PropertyB", "PropertyC" };
            AssertCompletions(members, expected);

            code = @"b : FFITarget.SecondNamespace.ClassWithNameConflict;";
            variableName = "b";
            string type2 = CodeCompletionParser.GetVariableType(code, variableName);
            Assert.AreEqual("FFITarget.SecondNamespace.ClassWithNameConflict", type2);

            // Assert that the class name is indeed a class
            Assert.DoesNotThrow(() => type = new ClassMirror(type2, libraryServicesCore));

            members = type.GetInstanceMembers();

            expected = new[] { "PropertyD", "PropertyE", "PropertyF" };
            AssertCompletions(members, expected);
        }


        private void AssertCompletions(IEnumerable<StaticMirror> members, string[] expected)
        {
            var actual = members.OrderBy(n => n.Name).Select(x => x.Name).ToArray();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        [Category("UnitTests")]
        public void TestCtorSignatureCompletion()
        {
            string ffiTargetClass = "CodeCompletionClass";
            string functionName = "CodeCompletionClass";

            var codeCompletionServices = new CodeCompletionServices(libraryServicesCore);

            string code = "";
            var overloads = codeCompletionServices.GetFunctionSignatures(code, functionName, ffiTargetClass);

            // Expected 3 "CodeCompletionClass" ctor overloads
            Assert.AreEqual(3, overloads.Count());

            foreach (var overload in overloads)
            {
                Assert.AreEqual(functionName, overload.Text);
            }
            Assert.AreEqual("CodeCompletionClass (i1 : int, i2 : int, i3 : int)", overloads.ElementAt(2).Stub);
        }

        [Test]
        [Category("UnitTests")]
        public void TestMethodSignatureCompletion()
        {
            var codeCompletionServices = new CodeCompletionServices(libraryServicesCore);

            string functionPrefix = "a";
            string ffiTargetClass = "CodeCompletionClass";
            string functionName = "OverloadedAdd";

            string code = string.Format("{0} : {1};", functionPrefix, ffiTargetClass);
            var overloads = codeCompletionServices.GetFunctionSignatures(code, functionName, functionPrefix);

            // Expected 2 "OverloadedAdd" method overloads
            Assert.AreEqual(2, overloads.Count());

            foreach (var overload in overloads)
            {
                Assert.AreEqual(functionName, overload.Text);
            }
            Assert.AreEqual("OverloadedAdd : int (cf : ClassFunctionality)", overloads.ElementAt(0).Stub);
        }

        [Test]
        [Category("UnitTests")]
        public void TestMethodSignatureReturnTypeCompletion()
        {
            var codeCompletionServices = new CodeCompletionServices(libraryServicesCore);

            string functionPrefix = "a";
            string ffiTargetClass = "CodeCompletionClass";
            string functionName = "AddWithValueContainer";

            string code = string.Format("{0} : {1};", functionPrefix, ffiTargetClass);
            var overloads = codeCompletionServices.GetFunctionSignatures(code, functionName, functionPrefix);

            // Expected 1 "AddWithValueContainer" method overloads
            Assert.AreEqual(1, overloads.Count());

            foreach (var overload in overloads)
            {
                Assert.AreEqual(functionName, overload.Text);
            }
            var expected = "AddWithValueContainer : ValueContainer[] (valueContainer : ValueContainer)";
            Assert.AreEqual(expected, overloads.ElementAt(0).Stub);
        }

        [Test]
        [Category("UnitTests")]
        public void TestBuiltInMethodSignatureCompletion()
        {
            var codeCompletionServices = new CodeCompletionServices(libraryServicesCore);

            string functionPrefix = "";
            string functionName = "Count";

            string code = "";
            var overloads = codeCompletionServices.GetFunctionSignatures(code, functionName, functionPrefix);

            // Expected 1 "AddWithValueContainer" method overloads
            Assert.AreEqual(1, overloads.Count());

            foreach (var overload in overloads)
            {
                Assert.AreEqual(functionName, overload.Text);
            }
            Assert.AreEqual("Count : int (list : [])", overloads.ElementAt(0).Stub);
        }

        [Test]
        [Category("UnitTests")]
        public void TestStaticMethodSignatureCompletion()
        {
            var codeCompletionServices = new CodeCompletionServices(libraryServicesCore);

            string ffiTargetClass = "CodeCompletionClass";
            string functionName = "StaticFunction";

            string code = "";
            var overloads = codeCompletionServices.GetFunctionSignatures(code, functionName, ffiTargetClass);

            // Expected 1 "StaticFunction" method overload
            Assert.AreEqual(1, overloads.Count());

            foreach (var overload in overloads)
            {
                Assert.AreEqual(functionName, overload.Text);
            }
            Assert.AreEqual("StaticFunction : int ()", overloads.ElementAt(0).Stub);
        }

        [Test]
        [Category("UnitTests")]
        public void TestCompletionWhenTyping()
        {
            var codeCompletionServices = new CodeCompletionServices(libraryServicesCore);
            string code = "Poi";
            var completions = codeCompletionServices.SearchCompletions(code, Guid.Empty);

            // Expected 4 completion items
            Assert.AreEqual(4, completions.Count());

            string[] expectedValues = {"DummyPoint", "DesignScript.Point",
                                    "Dynamo.Point", "UnknownPoint"};
            var expected = expectedValues.OrderBy(x => x);
            var actual = completions.Select(x => x.Text).OrderBy(x => x);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        [Category("UnitTests")]
        public void TestMethodKeywordCompletionWhenTyping()
        {
            var codeCompletionServices = new CodeCompletionServices(libraryServicesCore);
            string code = "im";
            var completions = codeCompletionServices.SearchCompletions(code, Guid.Empty);

            // Expected 5 completion items
            Assert.AreEqual(5, completions.Count());

            string[] expected = { "Decimal", "Imperative", "ImportFromCSV", "Minimal", "MinimalTracedClass" };
            var actual = completions.Select(x => x.Text).OrderBy(x => x);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        [Category("UnitTests")]
        public void TestHiddenClassCompletionWhenTyping()
        {
            var codeCompletionServices = new CodeCompletionServices(libraryServicesCore);

            // "SampleClassB" defined in FFITarget library with "IsVisibleInDynamoLibrary" attribute
            // is set to false. We verify that this class does not appear in code completion results
            string code = "sam";
            var completions = codeCompletionServices.SearchCompletions(code, Guid.Empty);

            // Expected 2 completion items
            Assert.AreEqual(2, completions.Count());

            string[] expected = { "SampleClassA", "SampleClassC" };
            var actual = completions.Select(x => x.Text).OrderBy(x => x);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        [Category("UnitTests")]
        public void TestHiddenConflictingClassCompletionWhenTyping()
        {
            var codeCompletionServices = new CodeCompletionServices(libraryServicesCore);

            // "SecondNamespace.AnotherClassWithNameConflict" is defined in FFITarget library with "IsVisibleInDynamoLibrary" 
            // attribute set to false. We verify that this class does not appear in code completion results
            // and that only "FirstNamespace.AnotherClassWithNameConflict" appears in code completion results with
            // fully qualified name so that it can be resolved against "SecondNamespace.AnotherClassWithNameConflict" 
            string code = "ano";
            var completions = codeCompletionServices.SearchCompletions(code, Guid.Empty);

            // Expected 1 completion items
            Assert.AreEqual(1, completions.Count());

            string[] expected = { "AnotherClassWithNameConflict" };
            var actual = completions.Select(x => x.Text).OrderBy(x => x);

            Assert.AreEqual(expected, actual);

            // Assert that the class name is indeed a class
            ClassMirror type = null;
            Assert.DoesNotThrow(() => type = new ClassMirror("FirstNamespace.AnotherClassWithNameConflict", libraryServicesCore));

            var members = type.GetMembers();

            expected = new[] { "AnotherClassWithNameConflict", "PropertyA", "PropertyB", "PropertyC" };
            AssertCompletions(members, expected);
        }

        [Test]
        [Category("UnitTests")]
        public void TestCodeCompletionParserForMultiLineCommentContext()
        {
            string code = "abc = { /* first entry */ 1, 2, /* last entry */ 3 };";

            // find the context at the caret position inside the second multiline-comments
            int caretPos = 45;
            Assert.IsTrue(CodeCompletionParser.IsInsideCommentOrString(code, caretPos));

            caretPos = 28;
            Assert.IsFalse(CodeCompletionParser.IsInsideCommentOrString(code, caretPos));
        }

        [Test]
        [Category("UnitTests")]
        public void TestCodeCompletionParserForSingleLineCommentContext()
        {
            string code = "abc = { // first entry " + "\n" + " 1, 2, 3 };";

            int caretPos = 15;
            Assert.IsTrue(CodeCompletionParser.IsInsideCommentOrString(code, caretPos));

            caretPos = 30;
            Assert.IsFalse(CodeCompletionParser.IsInsideCommentOrString(code, caretPos));
        }

        [Test]
        [Category("UnitTests")]
        public void TestCodeCompletionParserForStringContext()
        {
            string code = @"abc = { ""first entry"", 1, 2, 3 };";

            int caretPos = 15;
            Assert.IsTrue(CodeCompletionParser.IsInsideCommentOrString(code, caretPos));

            caretPos = 30;
            Assert.IsFalse(CodeCompletionParser.IsInsideCommentOrString(code, caretPos));
        }

        [Test]
        [Category("UnitTests")]
        public void TestCodeCompletionParserForCharContext()
        {
            string code = @"abc = { 'c', 1, 2, 3 };";

            int caretPos = 9;
            Assert.IsTrue(CodeCompletionParser.IsInsideCommentOrString(code, caretPos));

            caretPos = 15;
            Assert.IsFalse(CodeCompletionParser.IsInsideCommentOrString(code, caretPos));
        }

        [Test]
        [Category("UnitTests")]
        public void TestCodeCompletionParserForMultiLineStringContext()
        {
            string code = @"class test
{
    x=1;
}
a;b;c;d;e1;f;g;
[Imperative]
{
    a:double[][]= {1}; 

    b:int[][] =  {1.1}; 
    c:string[][]={""aparajit""}; 
    d:char [][]= {'c'};
    x1= test.test();
    e:test [][]= {x1};
    e1=e.x;
    f:bool [][]= {true};
    g [][]={null};
}";
            // Find the caret position inside the string in the above text block
            int caretPos = code.IndexOf('"');
            caretPos += 4;
            Assert.IsTrue(CodeCompletionParser.IsInsideCommentOrString(code, caretPos));

            // Advance the caret so that it moves outside the string
            caretPos += 6;
            Assert.IsFalse(CodeCompletionParser.IsInsideCommentOrString(code, caretPos));
        }

        [Test]
        [Category("UnitTests")]
        public void TestCodeCompletionParserForMultiLineCodeCommentContext()
        {
            string code = @"class test
{
    x=1;
}
a;b;c;d;e1;f;g;
[Imperative]
{
    a:double[][]= {1}; 

    b:int[][] =  {1.1}; 
    c:string[][]={/*aparajit*/}; 
    d:char [][]= {'c'};
    x1= test.test();
    e:test [][]= {x1};
    e1=e.x;
    f:bool [][]= {true};
    g [][]={null};
}";
            // Find the caret position inside the comment in the above text block
            int caretPos = code.IndexOf('*');
            caretPos += 4;
            Assert.IsTrue(CodeCompletionParser.IsInsideCommentOrString(code, caretPos));

            // Advance the caret so that it moves outside the comment
            caretPos += 6;
            Assert.IsFalse(CodeCompletionParser.IsInsideCommentOrString(code, caretPos));
        }

        [Test]
        [Category("UnitTests")]
        public void TestCodeCompletionParserForMultiLineSingleCommentContext()
        {
            string code = @"class test
{
    x=1;
}
a;b;c;d;e1;f;g;
[Imperative]
{
    a:double[][]= {1}; 

    b:int[][] =  {1.1}; 
    c:string[][]={1, 2, 3}; 
    d:char [][]= {'c'};
    x1= test.test();
    e:test [][]= {x1}; // comments 
    e1=e.x;
    f:bool [][]= {true};
    g [][]={null};
}";
            // Find the caret position inside the comment in the above text block
            int caretPos = code.IndexOf('/');
            caretPos += 4;
            Assert.IsTrue(CodeCompletionParser.IsInsideCommentOrString(code, caretPos));

            // Advance the caret so that it moves to the next line and outside the comment
            caretPos += 15;
            Assert.IsFalse(CodeCompletionParser.IsInsideCommentOrString(code, caretPos));
        }

        [Test]
        [Category("UnitTests")]
        public void TestCodeCompletionParserForAllMultiLineContexts()
        {
            string code = @"class test
{
    x=1;
}
a;b;c;d;e1;f;g;
[Imperative]
{
    a:double[][]= {1}; // comments 

    b:int[][] =  {1.1}; 
    c:string[][]={""Aparajit""}; 
    /*d:char [][]= {'c'};
    x1= test.test();*/
    e:test [][]= {x1}; 
    e1=e.x;
    f:bool [][]= {true};
    g [][]={null};
}";
            // Find the caret position within the first single line comment in the above text block
            int caretPos = code.IndexOf('/');
            caretPos += 4;
            Assert.IsTrue(CodeCompletionParser.IsInsideCommentOrString(code, caretPos));

            // Find the caret position inside the string 
            caretPos = code.IndexOf('"');
            caretPos += 4;
            Assert.IsTrue(CodeCompletionParser.IsInsideCommentOrString(code, caretPos));

            // Advance the caret so that it moves inside the multi-line comment
            caretPos = code.IndexOf('*');
            caretPos += 4;
            Assert.IsTrue(CodeCompletionParser.IsInsideCommentOrString(code, caretPos));

            // Advance the caret so that it moves to the next line and outside the multi-line comment
            caretPos += 40;
            Assert.IsFalse(CodeCompletionParser.IsInsideCommentOrString(code, caretPos));
        }

        [Test]
        [Category("UnitTests")]
        public void TestCompletionForPrimitiveTypes()
        {
            // Unit test for CodeCommpletionServices.SearchTypes() which should
            // include primitive types as well.
            var codeCompletionServices = new CodeCompletionServices(libraryServicesCore);

            string code = "boo";
            var completions = codeCompletionServices.SearchTypes(code);
            Assert.AreEqual(1, completions.Count());

            string[] expected = { "bool" };
            var actual = completions.Select(x => x.Text).OrderBy(x => x);
            Assert.AreEqual(expected, actual);
        }
    }
}
