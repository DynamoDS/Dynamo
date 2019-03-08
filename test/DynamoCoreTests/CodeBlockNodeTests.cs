using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoreNodeModels;
using DesignScript.Builtin;
using Dynamo.Engine.CodeCompletion;
using Dynamo.Graph;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using NUnit.Framework;

using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using ProtoCore.Mirror;
using ProtoCore.Utils;

using DynCmd = Dynamo.Models.DynamoModel;
using Dynamo.Models;
using Dynamo.Graph.Workspaces;
using Dynamo.Properties;

namespace Dynamo.Tests
{
    class CodeBlockNodeTests : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DesignScriptBuiltin.dll");
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
        public void TestVarRedefinitionInFunctionDef()
        {
            string openPath = Path.Combine(TestDirectory,
                @"core\dsevaluation\TestVarRedefinitionInFunctionDef.dyn");
            OpenModel(openPath);
            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            var guid = "bbf7919d-d578-4b54-90b1-7df8f01483c6";
            var cbn = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<CodeBlockNodeModel>(
                Guid.Parse(guid));
            

            Assert.IsNotNull(cbn);
            Assert.AreEqual(ElementState.PersistentWarning, cbn.State);

            Assert.IsTrue(cbn.CodeStatements.Any());

            Assert.AreEqual(0, cbn.InPorts.Count);
            Assert.AreEqual(1, cbn.OutPorts.Count);
            Assert.AreEqual(0, cbn.AllConnectors.Count());
            AssertPreviewValue(guid, 3);
        }

        [Test]
        [Category("UnitTests")]
        public void TestVarRecursiveDepInFunctionDef()
        {
            string openPath = Path.Combine(TestDirectory,
                @"core\dsevaluation\TestVarRecursiveDepInFunctionDef.dyn");
            OpenModel(openPath);
            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            var guid = "bbf7919d-d578-4b54-90b1-7df8f01483c6";
            var cbn = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<CodeBlockNodeModel>(
                Guid.Parse(guid));


            Assert.IsNotNull(cbn);
            Assert.AreEqual(ElementState.PersistentWarning, cbn.State);

            Assert.IsTrue(cbn.CodeStatements.Any());

            Assert.AreEqual(0, cbn.InPorts.Count);
            Assert.AreEqual(1, cbn.OutPorts.Count);
            Assert.AreEqual(0, cbn.AllConnectors.Count());
            AssertPreviewValue(guid, 2);
        }

        [Test]
        [Category("UnitTests")]
        public void TestVarRecursiveDepInAssocBlock()
        {
            string openPath = Path.Combine(TestDirectory,
                @"core\dsevaluation\TestVarRecursiveDepInAssocBlock.dyn");
            OpenModel(openPath);
            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            var guid = "bbf7919d-d578-4b54-90b1-7df8f01483c6";
            var cbn = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<CodeBlockNodeModel>(
                Guid.Parse(guid));


            Assert.IsNotNull(cbn);
            Assert.AreEqual(ElementState.PersistentWarning, cbn.State);

            Assert.IsTrue(cbn.CodeStatements.Any());

            Assert.AreEqual(0, cbn.InPorts.Count);
            Assert.AreEqual(1, cbn.OutPorts.Count);
            Assert.AreEqual(0, cbn.AllConnectors.Count());
            AssertPreviewValue(guid, null);
        }

        [Test]
        [Category("UnitTests")]
        public void TestVarRedefinitionInAssocBlock()
        {
            string openPath = Path.Combine(TestDirectory,
                @"core\dsevaluation\TestVarRedefinitionInAssocBlock.dyn");
            OpenModel(openPath);
            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            var guid = "6fbbc611-c805-43c6-809e-69b9ab317a9b";
            var cbn = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<CodeBlockNodeModel>(
                Guid.Parse(guid));


            Assert.IsNotNull(cbn);
            Assert.AreEqual(ElementState.PersistentWarning, cbn.State);

            Assert.IsTrue(cbn.CodeStatements.Any());

            Assert.AreEqual(0, cbn.InPorts.Count);
            Assert.AreEqual(1, cbn.OutPorts.Count);
            Assert.AreEqual(0, cbn.AllConnectors.Count());
            AssertPreviewValue(guid, 3);
        }

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

            UpdateCodeBlockNodeContent(codeBlockNodeOne, "a = 0; \n b = 1;");

            indexMap = CodeBlockUtils.GetDefinitionLineIndexMap(codeBlockNodeOne.CodeStatements);
            Assert.AreEqual("a", indexMap.ElementAt(0).Key);
            Assert.AreEqual(1, indexMap.ElementAt(0).Value);
            Assert.AreEqual("b", indexMap.ElementAt(1).Key);
            Assert.AreEqual(2, indexMap.ElementAt(1).Value);

            UpdateCodeBlockNodeContent(codeBlockNodeOne, "b = 0; \n a = 1; \n c = 2;");

            indexMap = CodeBlockUtils.GetDefinitionLineIndexMap(codeBlockNodeOne.CodeStatements);
            Assert.AreEqual("b", indexMap.ElementAt(0).Key);
            Assert.AreEqual(1, indexMap.ElementAt(0).Value);
            Assert.AreEqual("a", indexMap.ElementAt(1).Key);
            Assert.AreEqual(2, indexMap.ElementAt(1).Value);
            Assert.AreEqual("c", indexMap.ElementAt(2).Key);
            Assert.AreEqual(3, indexMap.ElementAt(2).Value);

        }

        [Test]
        [Category("RegressionTests")]
        public void Defect_MAGN_1045()
        {
            // Create the initial code block node.
            var codeBlockNode = CreateCodeBlockNode();

            // Before code changes, there should be no in/out ports.
            Assert.AreEqual(0, codeBlockNode.InPorts.Count);
            Assert.AreEqual(0, codeBlockNode.OutPorts.Count);

            // After code changes, there should be two output ports.
            UpdateCodeBlockNodeContent(codeBlockNode, "a = 1..6;\na[2]=a[2] + 1;");
            Assert.AreEqual(0, codeBlockNode.InPorts.Count);
            Assert.AreEqual(2, codeBlockNode.OutPorts.Count);
        }

        [Test]
        public void CodeBlockConnectionsRemainAfterUndo()
        {
            RunCurrentModel();
            // Create the initial code block node.
            var codeBlockNode1 = CreateCodeBlockNode();
            UpdateCodeBlockNodeContent(codeBlockNode1,"3;");

            // Create the second code block node.
            var codeBlockNode2 = CreateCodeBlockNode();
            UpdateCodeBlockNodeContent(codeBlockNode2, "x;");

            //connect them
            this.CurrentDynamoModel.ExecuteCommand(
                new DynamoModel.MakeConnectionCommand(codeBlockNode1.GUID, 0, PortType.Output,
             DynamoModel.MakeConnectionCommand.Mode.Begin));
            this.CurrentDynamoModel.ExecuteCommand(
                new DynamoModel.MakeConnectionCommand(codeBlockNode2.GUID, 0, PortType.Input,
                DynamoModel.MakeConnectionCommand.Mode.End));
            RunCurrentModel();

            var oldPos = codeBlockNode2.X;
            var oldinputPortGuid = codeBlockNode2.InPorts[0].GUID;
            var oldConnectorGuid = codeBlockNode2.InPorts[0].Connectors[0].GUID;

            //move the second node.
            this.CurrentDynamoModel.ExecuteCommand(
                new DynamoModel.UpdateModelValueCommand(codeBlockNode2.GUID, "Position", "300.0;300.0"));
            Assert.AreEqual(300, codeBlockNode2.X);
            //undo it.
            this.CurrentDynamoModel.CurrentWorkspace.Undo();
            Assert.AreEqual(oldPos, codeBlockNode2.X);

            //assert that after undo the port and connector have the same GUIDs
            //Assert.AreEqual(oldinputPortGuid, codeBlockNode2.InPorts[0].GUID);
            Assert.AreEqual(oldConnectorGuid, codeBlockNode2.InPorts[0].Connectors[0].GUID);


            RunCurrentModel();
            //assert the connectors still exist and the node has the same value
            Assert.IsTrue(codeBlockNode2.InPorts[0].Connectors.Count > 0);
            Assert.IsTrue(codeBlockNode1.OutPorts[0].Connectors.Count > 0);
            Assert.AreEqual(3, codeBlockNode2.CachedValue.Data);

        }
        [Test]
        public void UndoRedoCodeBlockErrorStateDoesNotCrashOutputs()
        {
            RunCurrentModel();
            // Create the initial code block node.
            var codeBlockNode1 = CreateCodeBlockNode();
            UpdateCodeBlockNodeContent(codeBlockNode1, "1;");
            RunCurrentModel();

            UpdateCodeBlockNodeContent(codeBlockNode1, "{1};");
            RunCurrentModel();

            Assert.IsTrue(codeBlockNode1.IsInErrorState);

            UpdateCodeBlockNodeContent(codeBlockNode1, "{1];");
            RunCurrentModel();

            Assert.DoesNotThrow(() =>
            {
                CurrentDynamoModel.CurrentWorkspace.Undo();
                RunCurrentModel();
                CurrentDynamoModel.CurrentWorkspace.Redo();
                RunCurrentModel();
                CurrentDynamoModel.CurrentWorkspace.Undo();
                RunCurrentModel();
            });

            Assert.IsTrue(codeBlockNode1.IsInErrorState);
        }

        [Test]
        public void UndoRedoCodeBlockErrorStateDoesNotCrashInputs()
        {
            RunCurrentModel();
            // Create the initial code block node.
            var codeBlockNode1 = CreateCodeBlockNode();
            UpdateCodeBlockNodeContent(codeBlockNode1, "x;");
            RunCurrentModel();

            UpdateCodeBlockNodeContent(codeBlockNode1, "{x};");
            RunCurrentModel();

            Assert.IsTrue(codeBlockNode1.IsInErrorState);

            UpdateCodeBlockNodeContent(codeBlockNode1, "{x];");
            RunCurrentModel();

            Assert.DoesNotThrow(() =>
            {
                CurrentDynamoModel.CurrentWorkspace.Undo();
                RunCurrentModel();
                CurrentDynamoModel.CurrentWorkspace.Redo();
                RunCurrentModel();
                CurrentDynamoModel.CurrentWorkspace.Undo();
                RunCurrentModel();
            });

            Assert.IsTrue(codeBlockNode1.IsInErrorState);
        }

        [Test]
        public void UndoRedoCodeBlockDeletionDoesNotCrash()
        {
            RunCurrentModel();
            // Create the initial code block node.
            var codeBlockNode1 = CreateCodeBlockNode();
            UpdateCodeBlockNodeContent(codeBlockNode1, "3;");

            // Create the second code block node.
            var codeBlockNode2 = CreateCodeBlockNode();
            UpdateCodeBlockNodeContent(codeBlockNode2, "x;");

            //connect them
            this.CurrentDynamoModel.ExecuteCommand(
                new DynamoModel.MakeConnectionCommand(codeBlockNode1.GUID, 0, PortType.Output,
             DynamoModel.MakeConnectionCommand.Mode.Begin));
            this.CurrentDynamoModel.ExecuteCommand(
                new DynamoModel.MakeConnectionCommand(codeBlockNode2.GUID, 0, PortType.Input,
                DynamoModel.MakeConnectionCommand.Mode.End));
            RunCurrentModel();

            var oldConnectorGuid = codeBlockNode2.InPorts[0].Connectors[0].GUID;

            //record the codeblock for undo - this is the issue as undoing this will deserialize the codeblock
            WorkspaceModel.RecordModelsForModification(new List<ModelBase> { codeBlockNode2 }, CurrentDynamoModel.CurrentWorkspace.UndoRecorder);

            //delete it
            this.CurrentDynamoModel.ExecuteCommand(
                new DynamoModel.DeleteModelCommand(codeBlockNode2.GUID));

            //undo the deletion.
            this.CurrentDynamoModel.CurrentWorkspace.Undo();
            Assert.AreEqual(2, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            //get the new codeblock instance
            codeBlockNode2 = CurrentDynamoModel.CurrentWorkspace.Nodes.Where(x => x.GUID == codeBlockNode2.GUID).FirstOrDefault() as CodeBlockNodeModel;

            Assert.AreEqual(oldConnectorGuid, codeBlockNode2.InPorts[0].Connectors[0].GUID);

            //undo the initial recording...
            this.CurrentDynamoModel.CurrentWorkspace.Undo();
            //assert nothing changed
            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(oldConnectorGuid, codeBlockNode2.InPorts[0].Connectors[0].GUID);

            //redo the deletion
            Assert.DoesNotThrow(() =>
            {
                this.CurrentDynamoModel.CurrentWorkspace.Redo();
                this.CurrentDynamoModel.CurrentWorkspace.Redo();
                this.CurrentDynamoModel.CurrentWorkspace.Redo();
                this.CurrentDynamoModel.CurrentWorkspace.Redo();
            });
            
            //undo deletion again
            this.CurrentDynamoModel.CurrentWorkspace.Undo();
            //get the new codeblock instance
            codeBlockNode2 = CurrentDynamoModel.CurrentWorkspace.Nodes.Where(x => x.GUID == codeBlockNode2.GUID).FirstOrDefault() as CodeBlockNodeModel;
            Assert.AreEqual(2, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(oldConnectorGuid, codeBlockNode2.InPorts[0].Connectors[0].GUID);
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
            var codeBlockNodeTwo = CurrentDynamoModel.CurrentWorkspace.Nodes.ElementAt(1) as CodeBlockNodeModel;
            Assert.IsNotNull(codeBlockNodeTwo);

            // The preview identifier should be named as "arr_GUID" (the prefix 
            // "arr" is derived from the named variable in the code block node).
            // 
            var guid = codeBlockNodeTwo.GUID.ToString();
            var expectedIdentifier = "arr_" + guid.Replace("-", string.Empty);
            Assert.AreEqual(expectedIdentifier, codeBlockNodeTwo.AstIdentifierBase);
        }

        [Test]
        public void TestInOutPorts_ForChainedExpression()
        {
            // Create the initial code block node.
            var codeBlockNode = CreateCodeBlockNode();

            // Before code changes, there should be no in/out ports.
            Assert.AreEqual(0, codeBlockNode.InPorts.Count);
            Assert.AreEqual(0, codeBlockNode.OutPorts.Count);

            // After code changes, there should be input & output ports.
            UpdateCodeBlockNodeContent(codeBlockNode, "Flatten(l.Explode()).Area;");
            Assert.AreEqual(1, codeBlockNode.InPorts.Count);
            Assert.AreEqual(1, codeBlockNode.OutPorts.Count);
        }

        [Test]
        public void TestOutportConnectors_OnAssigningVariables_ToRetainConnections()
        {
            string openPath = Path.Combine(TestDirectory,
                @"core\dsevaluation\TestOutportConnectors_OnAssigningVariables_ToRetainConnections.dyn");
            OpenModel(openPath);
            Assert.AreEqual(2, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

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
        public void Test_InportOutportConnections_RetainedForCodeBlockErrors()
        {
            string openPath = Path.Combine(TestDirectory,
                @"core\dsevaluation\Test_InportOutportConnections_RetainedForCodeBlockErrors.dyn");
            OpenModel(openPath);
            Assert.AreEqual(3, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            var cbn = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<CodeBlockNodeModel>(
                Guid.Parse("50edf3c7-7e0d-4e2d-8344-f7380eddd827"));

            Assert.IsNotNull(cbn);
            Assert.AreEqual(ElementState.Active, cbn.State);
            Assert.IsTrue(cbn.CodeStatements.Any());

            Assert.AreEqual(5, cbn.InPorts.Count);
            Assert.AreEqual(3, cbn.OutPorts.Count);
            Assert.AreEqual(8, cbn.AllConnectors.Count());

            // add syntax error in cbn code and update
            string codeInCBN = @"x = [a$,b,c,d,e];x[1];x[3];";
            UpdateCodeBlockNodeContent(cbn, codeInCBN);

            // Verify that cbn is in error state and number of input, output ports remains the same
            Assert.IsNotNull(cbn);
            Assert.AreEqual(ElementState.Error, cbn.State);
            Assert.IsTrue(!cbn.CodeStatements.Any());
            
            Assert.AreEqual(5, cbn.InPorts.Count);
            Assert.AreEqual(3, cbn.OutPorts.Count);
            Assert.AreEqual(8, cbn.AllConnectors.Count());
        }

        [Test]
        public void Test_InportOutportConnections_RetainedForCodeBlockErrorsInFile()
        {
            string openPath = Path.Combine(TestDirectory,
                @"core\dsevaluation\Test_InportOutportConnections_RetainedForCodeBlockErrorsInFile.dyn");
            OpenModel(openPath);
            Assert.AreEqual(3, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            var cbn = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<CodeBlockNodeModel>(
                Guid.Parse("50edf3c7-7e0d-4e2d-8344-f7380eddd827"));

            // Verify that cbn is in error state and input, output ports exist
            Assert.IsNotNull(cbn);
            Assert.AreEqual(ElementState.Error, cbn.State);
            Assert.IsTrue(!cbn.CodeStatements.Any());

            Assert.AreEqual(5, cbn.InPorts.Count);
            Assert.AreEqual(3, cbn.OutPorts.Count);
            Assert.AreEqual(8, cbn.AllConnectors.Count());
        }

        [Test]
        public void Test_PortErrorBehavior_CodeBlockErrorsInFile()
        {
            // ---------------------------------------------------------------
            // STEP 1: Load a file with the code block node in an error state
            //         and verify the ports, port line indexes, and connections 

            string openPath = Path.Combine(TestDirectory,
                @"core\dsevaluation\Test_PortErrorBehavior_CodeBlockErrorsInFile.dyn");
            OpenModel(openPath);
            Assert.AreEqual(3, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            var cbn = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<CodeBlockNodeModel>(
                Guid.Parse("dad587d1-acee-445c-890d-98500b408ec6"));

            // Verify that the code block node is in an error state
            Assert.IsNotNull(cbn);
            Assert.AreEqual(ElementState.Error, cbn.State);
            Assert.IsTrue(!cbn.CodeStatements.Any());

            // Verify that input ports, output ports, and any expected connections exist
            Assert.AreEqual(1, cbn.InPorts.Count);
            Assert.AreEqual(3, cbn.OutPorts.Count);
            Assert.AreEqual(2, cbn.AllConnectors.Count());

            // NOTE: Input ports are matched by name instead of index, so there
            //       is no checking of the input line indexes here or below
      
            // Verify the output port line indexes here
            Assert.AreEqual(0, cbn.OutPorts[0].LineIndex);
            Assert.AreEqual(1, cbn.OutPorts[1].LineIndex);
            Assert.AreEqual(2, cbn.OutPorts[2].LineIndex);

            // ---------------------------------------------------------------
            // STEP 2: Fix the code block node error and verify the ports, 
            //         connections, and port line indexes  

            // Fix the code block node error
            var brokenCode = cbn.Code;
            var fixedCode = brokenCode.Replace("{val};", "{};");
            UpdateCodeBlockNodeContent(cbn, fixedCode);

            // Verify that the code block node is no longer in an error state
            Assert.AreEqual(ElementState.Active, cbn.State);
            Assert.IsTrue(cbn.CodeStatements.Any());

            // Verify that input ports, output ports, and any expected connections exist
            // and that the number of each has not changed
            Assert.AreEqual(1, cbn.InPorts.Count);
            Assert.AreEqual(3, cbn.OutPorts.Count);
            Assert.AreEqual(2, cbn.AllConnectors.Count());

            // Verify the output port line indexes here
            Assert.AreEqual(0, cbn.OutPorts[0].LineIndex);
            Assert.AreEqual(5, cbn.OutPorts[1].LineIndex);
            Assert.AreEqual(6, cbn.OutPorts[2].LineIndex);
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
            var codeBlockNodeTwo = CurrentDynamoModel.CurrentWorkspace.Nodes.ElementAt(1) as CodeBlockNodeModel;
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
            Assert.AreEqual(1, codeBlockNode.InPorts.Count);

            // Update the code block node
            UpdateCodeBlockNodeContent(codeBlockNode, @"Point.ByCoordinates(0,0,0);");

            // Check
            Assert.AreEqual(0, codeBlockNode.InPorts.Count);
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
            Assert.AreEqual(1, codeBlockNode0.InPorts.Count);

            // Update the first code block node to have y defined
            UpdateCodeBlockNodeContent(codeBlockNode0, "y=1;\nx=y;");

            // Check
            Assert.AreEqual(0, codeBlockNode0.InPorts.Count);
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
            Assert.AreEqual(1, codeBlockNode0.InPorts.Count);

            // Create the second code block node
            var codeBlockNode1 = CreateCodeBlockNode();
            UpdateCodeBlockNodeContent(codeBlockNode1, @"false;");

            // Connect the two code block nodes
            ConnectorModel.Make(codeBlockNode1, codeBlockNode0, 0, 0);

            // Run
            Assert.DoesNotThrow(BeginRun);

            UpdateCodeBlockNodeContent(codeBlockNode0, @"true;");

            // Check
            Assert.AreEqual(0, codeBlockNode0.InPorts.Count);

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

            Assert.AreEqual(1, codeBlockNode.InPorts.Count);

            // Update the code block node
            UpdateCodeBlockNodeContent(codeBlockNode, "pt = Point.ByCoordinates(0,0,0);\nCircle.ByCenterPointRadius(pt,5)");

            Assert.AreEqual(0, codeBlockNode.InPorts.Count);
        }

        [Test]
        [Category("RegressionTests")]
        public void OutPort_WithCommentNonAssignment_Alignment()
        {
            // Create the initial code block node.
            var codeBlockNode = CreateCodeBlockNode();
            string code = "// comment \n // comment \n a+b;";

            UpdateCodeBlockNodeContent(codeBlockNode, code);

            Assert.AreEqual(2, codeBlockNode.InPorts.Count);
            Assert.AreEqual(1, codeBlockNode.OutPorts.Count);

            Assert.AreEqual(2, codeBlockNode.OutPorts[0].LineIndex);

            code = "c+ \n d; \n /* comment \n */ \n a+b;";
            UpdateCodeBlockNodeContent(codeBlockNode, code);

            Assert.AreEqual(4, codeBlockNode.InPorts.Count);
            Assert.AreEqual(2, codeBlockNode.OutPorts.Count);

            // The first output port should be at the first line
            Assert.AreEqual(0, codeBlockNode.OutPorts[0].LineIndex);

            // The second output port should be at the 4th line, which is also 3 lines below the first
            Assert.AreEqual(4, codeBlockNode.OutPorts[1].LineIndex);

            code = "/*comment \n */ \n a[0]+b;";
            UpdateCodeBlockNodeContent(codeBlockNode, code);

            Assert.AreEqual(2, codeBlockNode.InPorts.Count);
            Assert.AreEqual(1, codeBlockNode.OutPorts.Count);

            Assert.AreEqual(2, codeBlockNode.OutPorts[0].LineIndex);
        }

        [Test]
        [Category("RegressionTests")]
        public void PortIndicesShouldProduceCorrectConnectorOffsets()
        {
            var code =
@"var00 = a;

var01 = b;
var02 = c;


var03 = d;
var04 = e;
var05 = f;



var06 = g;
";

            var codeBlockNode = CreateCodeBlockNode();
            UpdateCodeBlockNodeContent(codeBlockNode, code);

            Assert.AreEqual(7, codeBlockNode.InPorts.Count);
            Assert.AreEqual(7, codeBlockNode.OutPorts.Count);

            // Input ports are regular ports that do not depend on LineIndex.
            Assert.AreEqual(-1, codeBlockNode.InPorts[0].LineIndex);
            Assert.AreEqual(-1, codeBlockNode.InPorts[1].LineIndex);
            Assert.AreEqual(-1, codeBlockNode.InPorts[2].LineIndex);
            Assert.AreEqual(-1, codeBlockNode.InPorts[3].LineIndex);
            Assert.AreEqual(-1, codeBlockNode.InPorts[4].LineIndex);
            Assert.AreEqual(-1, codeBlockNode.InPorts[5].LineIndex);
            Assert.AreEqual(-1, codeBlockNode.InPorts[6].LineIndex);

            // Output ports are smaller ports that depend on LineIndex.
            Assert.AreEqual(0, codeBlockNode.OutPorts[0].LineIndex);
            Assert.AreEqual(2, codeBlockNode.OutPorts[1].LineIndex);
            Assert.AreEqual(3, codeBlockNode.OutPorts[2].LineIndex);
            Assert.AreEqual(6, codeBlockNode.OutPorts[3].LineIndex);
            Assert.AreEqual(7, codeBlockNode.OutPorts[4].LineIndex);
            Assert.AreEqual(8, codeBlockNode.OutPorts[5].LineIndex);
            Assert.AreEqual(12, codeBlockNode.OutPorts[6].LineIndex);

            // Ensure that "NodeModel.GetPortVerticalOffset" does not regress.
            // This is the way connector position is calculated.
            var verticalOffset = 2.9;
            var lastOutPort = codeBlockNode.OutPorts[6];
            var expectedOffset = verticalOffset + lastOutPort.LineIndex * lastOutPort.Height;
            var actualOffset = codeBlockNode.GetPortVerticalOffset(lastOutPort);
            Assert.AreEqual(expectedOffset, actualOffset);

            // Input ports are regular portst that should depend on "Index" instead.
            var lastInPort = codeBlockNode.InPorts[6];
            expectedOffset = verticalOffset + lastInPort.Index * lastInPort.Height;
            actualOffset = codeBlockNode.GetPortVerticalOffset(lastInPort);
            Assert.AreEqual(6, lastInPort.Index);
            Assert.AreEqual(expectedOffset, actualOffset);
        }

        [Test]
        [Category("RegressionTests")]
        public void InPort_WithInlineConditionNonAssignment_Creation()
        {
            // Create the initial code block node.
            var codeBlockNode = CreateCodeBlockNode();
            string code = "c + d; \n z = 2;";

            UpdateCodeBlockNodeContent(codeBlockNode, code);

            Assert.AreEqual(2, codeBlockNode.InPorts.Count);
            Assert.AreEqual(2, codeBlockNode.OutPorts.Count);

            code = "x%2 == 0 ? x : -x; \n y = a+b;";
            UpdateCodeBlockNodeContent(codeBlockNode, code);

            Assert.AreEqual(3, codeBlockNode.InPorts.Count);
            Assert.AreEqual(2, codeBlockNode.OutPorts.Count);

            code = "f(x); \n y = a+b;";
            UpdateCodeBlockNodeContent(codeBlockNode, code);

            Assert.AreEqual(3, codeBlockNode.InPorts.Count);
            Assert.AreEqual(2, codeBlockNode.OutPorts.Count);
        }

        [Test]
        [Category("RegressionTests")]
        public void Parse_NonAssignmentFollowedByAssignment()
        {
            // Create the initial code block node.
            var codeBlockNode = CreateCodeBlockNode();
            string code = "List.IsEmpty(result)?result:List.FirstItem(result);";

            UpdateCodeBlockNodeContent(codeBlockNode, code);

            Assert.AreEqual(1, codeBlockNode.InPorts.Count);
            Assert.AreEqual(1, codeBlockNode.OutPorts.Count);

            code = "x%2 == 0 ? x : -x;";
            UpdateCodeBlockNodeContent(codeBlockNode, code);

            Assert.AreEqual(1, codeBlockNode.InPorts.Count);
            Assert.AreEqual(1, codeBlockNode.OutPorts.Count);
        }

        [Test]
        [Category("RegressionTests")]
        public void Parse_TypedVariableDeclaration()
        {
            // Create the initial code block node.
            var codeBlockNode = CreateCodeBlockNode();
            string code = "a : int;";

            UpdateCodeBlockNodeContent(codeBlockNode, code);

            Assert.AreEqual(1, codeBlockNode.InPorts.Count);
            Assert.AreEqual(1, codeBlockNode.OutPorts.Count);

            code = "a : int  = 2;";
            UpdateCodeBlockNodeContent(codeBlockNode, code);

            Assert.AreEqual(0, codeBlockNode.InPorts.Count);
            Assert.AreEqual(1, codeBlockNode.OutPorts.Count);
        }

        [Test]
        public void Defect_MAGN_6723()
        {
            var codeBlockNode = CreateCodeBlockNode();
            string code = @"x + ""anyString"";";

            UpdateCodeBlockNodeContent(codeBlockNode, code);
            Assert.AreEqual(1, codeBlockNode.InPorts.Count);
        }

        [Test]
        public void TestEscapeSequenceAtEnd()
        {
            string openPath = Path.Combine(TestDirectory, @"core\dsevaluation\CBN_escapeseq.dyn");
            OpenModel(openPath);
            Assert.AreEqual(4, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            BeginRun();

            AssertPreviewValue("a26a599a-e608-4d6f-8de8-309f7fb0972d", "hello\\");

            var helloBrokenNode = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<CodeBlockNodeModel>(
                Guid.Parse("060f6309-80b1-4a15-bc8d-accb2dd24d22"));
            int outportCount = helloBrokenNode.OutPorts.Count;
            Assert.IsFalse(outportCount > 0);

            var helloWorldNode = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<CodeBlockNodeModel>( 
                Guid.Parse("90109553-56c2-4afd-ab3d-27356e5c2f07"));
            AssertPreviewValue("90109553-56c2-4afd-ab3d-27356e5c2f07", "world");

            var helloWorldBrokenNode = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<CodeBlockNodeModel>(
                Guid.Parse("2e28c888-77b4-4bd5-a946-d8588368e014"));
            int outportBrokenNodeCount = helloBrokenNode.OutPorts.Count;
            Assert.IsFalse(outportCount > 0);
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
            Assert.AreEqual(unboundIdentifiers[0], data0.Name);
            Assert.AreEqual(unboundIdentifiers[0], data0.ToolTipString);

            var data1 = data.ElementAt(1);
            Assert.AreEqual("LongerVariableNameTha...", data1.Name);
            Assert.AreEqual(unboundIdentifiers[1], data1.ToolTipString);
        }

        [Test]
        [Category("UnitTests")]
        public void GetStatementVariables00()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                // Null as argument will cause exception.
                CodeBlockUtils.GetStatementVariables(null);
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

            var vars = CodeBlockUtils.GetStatementVariables(statements);
            Assert.IsNotNull(vars);
            Assert.AreEqual(1, vars.Count());

            var variables = vars.ElementAt(0);
            Assert.IsNotNull(variables);
            Assert.AreEqual(1, variables.Count());
            Assert.AreEqual("Value", variables.ElementAt(0));
        }

        [Test]
        [Category("UnitTests")]
        public void GetStatementVariablesForOutports00()
        {
            var cb = ParserUtils.Parse(@"a[0] = 1234; a[1] = ""abcd"";");

            var binExprNode = cb.Body[0] as BinaryExpressionNode;

            var statements = new List<Statement>
            {
                Statement.CreateInstance(binExprNode)
            };

            var vars = CodeBlockUtils.GetStatementVariablesForOutports(statements);
            Assert.IsNotNull(vars);
            Assert.AreEqual(1, vars.Count());

            var variables = vars.ElementAt(0);
            Assert.IsNotNull(variables);
            Assert.AreEqual(1, variables.Count());
            Assert.AreEqual("a[0]", variables.ElementAt(0));

            binExprNode = cb.Body[1] as BinaryExpressionNode;

            statements = new List<Statement>
            {
                Statement.CreateInstance(binExprNode)
            };

            vars = CodeBlockUtils.GetStatementVariablesForOutports(statements);
            Assert.IsNotNull(vars);
            Assert.AreEqual(1, vars.Count());

            variables = vars.ElementAt(0);
            Assert.IsNotNull(variables);
            Assert.AreEqual(1, variables.Count());
            Assert.AreEqual("a[1]", variables.ElementAt(0));
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

            Assert.Throws<System.IndexOutOfRangeException>(() =>
            {
                // -1 as index argument will cause exception.
                CodeBlockUtils.DoesStatementRequireOutputPort(svs, -1);
            });

            Assert.Throws<System.IndexOutOfRangeException>(() =>
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
            Assert.AreEqual(2, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            BeginRun();

            var node = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<CodeBlockNodeModel>(
                Guid.Parse("17d2f866-dc5a-43ef-b3c5-ac7474d16467"));

            Assert.IsNotNull(node);
            Assert.AreEqual(null, node.CachedValue.Data);

            // Assert that node throws type mismatch warning
            Assert.IsTrue(node.ToolTipText.Contains(
                ProtoCore.Properties.Resources.kConvertNonConvertibleTypes));
        }

        [Test]
        // This test case is specific to the "ExportCSV node, needs to be updated whenever there is a change to the node
        public void MethodDeprecated_LogsWarning()
        {
            string openPath = Path.Combine(TestDirectory, @"core\dsevaluation\MigrateCBN.dyn");
            RunModel(openPath);

            Assert.AreEqual(3, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(2, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            var node1 = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace
                ("eff3e874-cfc3-455c-8275-741ab5b42ebd");

            Assert.IsTrue(node1.ToolTipText.Contains(
                "Method 'DSCore.IO.CSV.WriteToFile' has been deprecated, please use method 'DSOffice.Data.ExportCSV' instead"));
        }

        [Test]
        public void ChangeListSyntax_CodeBlockInErrorState()
        {
            string openPath = Path.Combine(TestDirectory, @"core\dsevaluation\colorRangeBad.dyn");
            RunModel(openPath);

            Assert.AreEqual(13, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(19, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            var cbn = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<CodeBlockNodeModel>
                ("d2401c4d-840a-4100-9789-6083193b9a24");

            Assert.IsNotNull(cbn);
            string code = "[red,green,blue];{0,num,1};";
            UpdateCodeBlockNodeContent(cbn, code);

            RunCurrentModel();

            Assert.IsTrue(cbn.IsInErrorState);
            Assert.IsTrue(cbn.ToolTipText.Contains(
                ProtoCore.Properties.Resources.DeprecatedListInitializationSyntax));
        }

        #endregion

        #region Codeblock Namespace Resolution Tests

        [Test]
        public void Resolve_NamespaceConflict_LoadLibrary()
        {
            string code = "Point.ByCoordinates(10,0,0);";

            var cbn = CreateCodeBlockNode();

            UpdateCodeBlockNodeContent(cbn, code);
            Assert.AreEqual(1, cbn.OutPorts.Count);

            // FFITarget introduces conflicts with Point class in
            // FFITarget.Dummy.Point, FFITarget.Dynamo.Point
            const string libraryPath = "FFITarget.dll";

            CompilerUtils.TryLoadAssemblyIntoCore(
                CurrentDynamoModel.LibraryServices.LibraryManagementCore, libraryPath);

            code = "Point.ByCoordinates(0,0,0);";
            UpdateCodeBlockNodeContent(cbn, code);
            Assert.AreEqual(0, CurrentDynamoModel.LibraryServices.LibraryManagementCore.BuildStatus.Warnings.Count());
        }


        [Test]
        [Category("RegressionTests")]
        public void TestMultipleIndexingInCodeBlockNode()
        {
            string openPath = Path.Combine(TestDirectory, @"core\dsevaluation\TestVariableIndexingInCBN.dyn");
            RunModel(openPath);

            AssertPreviewValue("7cbc2d85-d837-4969-8eba-36f672c77d6f", 6);
            AssertPreviewValue("ebb49227-2e2b-4861-b824-1574ba89b455", 6);
        }

        [Test]
        public void TestWarningsWithListMethods()
        {
            string openPath = Path.Combine(TestDirectory, @"core\sorting\sorting.dyn");
            OpenModel(openPath);

            var node1 = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace
                ("14fae78b-b009-4503-afe9-b714e08db1ec");
            var node2 = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace
                ("9e2c84e6-b9b8-4bdf-b82e-868b2436b865");

            Assert.IsTrue(string.IsNullOrEmpty(node1.ToolTipText));
            Assert.IsTrue(string.IsNullOrEmpty(node2.ToolTipText));

            BeginRun();

            Assert.IsTrue(string.IsNullOrEmpty(node1.ToolTipText));
            Assert.IsTrue(string.IsNullOrEmpty(node2.ToolTipText));
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

    [TestFixture]
    public class CodeBlockNodeRenamingTests : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("FFITarget.dll");

            base.GetLibrariesToPreload(libraries);
        }
        [Test]
        public void TestReplicationGuidesWithASTRewrite()
        {
            string openPath = Path.Combine(TestDirectory, @"core\cbn_renaming\TestReplicationGuidesWithASTRewrite.dyn");
            RunModel(openPath);
            var data1 = new object[] {new[] {5, 6, 7}, new[] {6, 7, 8}, new[] {7, 8, 9}};
            var data2 = new object[] { new[] { 11, 21, 31 }, new[] { 12, 22, 32 }, new[] { 13, 23, 33 } };
            AssertPreviewValue("345a236b-6919-4075-b64c-81568c892bb2", data1);
            AssertPreviewValue("49f2bd4a-6b88-4bf7-bf61-5c6f8d407478", data2);
        }

        [Test]
        public void TestImperativePropertyAccessing()
        {
            string openPath = Path.Combine(TestDirectory, @"core\cbn_renaming\TestImperativePropertyAccessing.dyn");
            RunModel(openPath);
            AssertPreviewValue("39c65660-8575-43bc-8af7-f24225a6bd5b", 21);
        }

        [Test]
        [Ignore("Test Loops Forever. Danger.")]
        public void TestImperativeLanguageBlock()
        {
            // TODO pratapa: Return to fix this test - result of difference in indexing behavior after ValueAtIndex
            string openPath = Path.Combine(TestDirectory, @"core\cbn_renaming\TestImperativeInCBN.dyn");
            RunModel(openPath);
            AssertPreviewValue("27fba61c-ba19-4575-90a7-f856f74b4887", 49);
        }

        [Test]
        public void TestImperativeBinaryExpression()
        {
            string openPath = Path.Combine(TestDirectory, @"core\cbn_renaming\TestImperativeBinaryExpression.dyn");
            RunModel(openPath);
            AssertPreviewValue("0959d53a-76f9-4bd2-b407-5e409856b55e", 63);
        }

        [Test]
        public void TestImperativeArrayIndexing()
        {
            string openPath = Path.Combine(TestDirectory, @"core\cbn_renaming\TestImperativeArrayIndexing.dyn");
            RunModel(openPath);
            AssertPreviewValue("a09b25ca-435b-4dec-b18a-e51ed6aed489", 6);
        }

        [Test]
        public void TestImperativeExpressionList()
        {
            string openPath = Path.Combine(TestDirectory, @"core\cbn_renaming\TestImperativeExpressionList.dyn");
            RunModel(openPath);
            AssertPreviewValue("ab1ddd1b-67cd-4c27-a49d-53baca0c6e95", new object[] { 5, 11, 17, 21 } );
        }

        [Test]
        public void TestImperativeFunctionParameter()
        {
            string openPath = Path.Combine(TestDirectory, @"core\cbn_renaming\TestImperativeFunctionParameter.dyn");
            RunModel(openPath);
            AssertPreviewValue("a9ba09c4-54a3-4c79-b5c0-ab98afbd1f9c", 630);
        }

        [Test]
        public void TestImperativeRangeExpression()
        {
            string openPath = Path.Combine(TestDirectory, @"core\cbn_renaming\TestImperativeRangeExpression.dyn");
            RunModel(openPath);
            AssertPreviewValue("4356b0c6-636e-4729-9e7a-34ab69fd0feb", new object[] { 2, 5, 8 });
        }

        [Test]
        public void TestImperativeIf()
        {
            string openPath = Path.Combine(TestDirectory, @"core\cbn_renaming\TestImperativeIf.dyn");
            RunModel(openPath);
            AssertPreviewValue("2822649d-ce8c-41df-b3e3-f3e97d43f5e5", 21);
        }

        [Test]
        public void TestImperativeForLoop()
        {
            string openPath = Path.Combine(TestDirectory, @"core\cbn_renaming\TestImperativeForLoop.dyn");
            RunModel(openPath);
            AssertPreviewValue("c031668c-226f-4b99-8e54-011c2199b8c6", 55);
        }

        [Test]
        public void TestImperativeWhileLoop()
        {
            string openPath = Path.Combine(TestDirectory, @"core\cbn_renaming\TestImperativeWhileLoop.dyn");
            RunModel(openPath);
            AssertPreviewValue("bfcedc08-4812-452c-a3e4-192c6de99530", 55);
        }

        [Test]
        public void TestImperativeUnaryExpression()
        {
            string openPath = Path.Combine(TestDirectory, @"core\cbn_renaming\TestImperativeUnaryExpression.dyn");
            RunModel(openPath);
            AssertPreviewValue("a17ce868-60c4-41cf-ad12-fdbb6e79284e", -100);
        }

        [Test]
        public void TestPropertyAccessing()
        {
            string openPath = Path.Combine(TestDirectory, @"core\cbn_renaming\TestPropertyAccessing.dyn");
            RunModel(openPath);
            AssertPreviewValue("30c24391-9361-4b53-834f-912e9faf9586", 2);
        }

        [Test]
        public void TestDictionaryAndListInitializationSyntax()
        {
            string openPath = Path.Combine(TestDirectory, @"core\cbn\TestDictionaryListSyntax.dyn");
            RunModel(openPath);
            AssertPreviewValue("3ad6f95c-3bc4-4740-b9ae-0cd3a8577b4a", DesignScript.Builtin.Dictionary.ByKeysValues(new List<string>(), new List<object>()));
            AssertPreviewValue("499afbf5-416b-4f13-bdd6-e232130f644d", new int[] { });
            AssertPreviewValue("31373889-0b81-42cf-82d1-463af2e4a200", DesignScript.Builtin.Dictionary.ByKeysValues(new List<string>() { "foo" }, new List<object>() { "bar" }));
            AssertPreviewValue("6ce105e2-fa09-453e-9418-a57dff7e62c1", DesignScript.Builtin.Dictionary.ByKeysValues(new List<string>() { "foo" }, new List<object>() { "bar" }));
            AssertPreviewValue("0dfac919-8996-4dd3-b5ab-5c1f3034510a", new [] { 1, 2 });
            AssertPreviewValue("5486ba2d-af43-4f9f-9ed0-7e080bca49fd", new [] { 1, 2 });
            AssertPreviewValue("6fe8b8b3-3c81-4210-b58b-df60cc778fb0", null);
            AssertPreviewValue("24f7cbca-a101-44df-a751-8ed264096c20", null);
            AssertPreviewValue("2afe34da-e7ae-43c1-a43a-fa233a7e1906", DesignScript.Builtin.Dictionary.ByKeysValues(new List<string>() { "foo" }, new List<object>() { "bar" }));

            AssertError("0cb2f07a-95ab-49ed-bd7e-3e21281b87a3"); // uses identifier as dictionary key
            AssertError("a2b3ac31-98f0-46b0-906e-8617821d0a51"); // uses old syntax {1,2}
        }
        
        [Test]
        public void TestDeprecatedListSyntaxMigration()
        {
            string openPath = Path.Combine(TestDirectory, @"core\migration\CodeBlockWithArray.dyn");
            RunModel(openPath);
            AssertPreviewValue("b80e0c94-4a98-4f98-a197-f426a0a96db3", new object[] { 1, 2, 3 });
        }

        [Test]
        public void CodeBlockNodeModelOutputPortLabels()
        {
            string openPath = Path.Combine(TestDirectory, @"core\cbn\OutputPortLabels.dyn");
            RunModel(openPath);

            var cbn = GetModel().Workspaces.First().Nodes.First() as CodeBlockNodeModel;
            Assert.AreEqual(string.Format(Resources.CodeBlockTempIdentifierOutputLabel, 1), cbn.OutPorts.First().ToolTip); 
            Assert.AreEqual(string.Format(Resources.CodeBlockTempIdentifierOutputLabel, 2), cbn.OutPorts.ElementAt(1).ToolTip); 
            Assert.AreEqual(string.Format(Resources.CodeBlockTempIdentifierOutputLabel, 4), cbn.OutPorts.ElementAt(2).ToolTip); 
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
            libraryServicesCore.Compilers.Add(ProtoCore.Language.Associative, new ProtoAssociative.Compiler(libraryServicesCore));
            libraryServicesCore.Compilers.Add(ProtoCore.Language.Imperative, new ProtoImperative.Compiler(libraryServicesCore));

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

            var expected = new[] { "AddWithValueContainer", "CodeCompletionClass", "IsEqualTo", "OverloadedAdd", "StaticFunction", "StaticProp"};
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
            const string libraryPath = "BuiltIn.ds";

            CompilerUtils.TryLoadAssemblyIntoCore(libraryServicesCore, libraryPath);

            var codeCompletionServices = new CodeCompletionServices(libraryServicesCore);

            string functionPrefix = "List";
            string functionName = "RemoveIfNot";

            string code = "";
            var overloads = codeCompletionServices.GetFunctionSignatures(code, functionName, functionPrefix);

            // Expected 1 "AddWithValueContainer" method overloads
            Assert.AreEqual(1, overloads.Count());

            foreach (var overload in overloads)
            {
                Assert.AreEqual(functionName, overload.Text);
            }
            Assert.AreEqual("RemoveIfNot : [] (list : [], type : string)", overloads.ElementAt(0).Stub);
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
            Assert.AreEqual(8, completions.Count());

            string[] expectedValues = {"DummyPoint", "DesignScript.Point",
                                    "Dynamo.Point", "UnknownPoint", "DummyPoint2D", "Point_1D", "Point_2D", "Point_3D"};
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

            // Expected 3 completion items
            Assert.AreEqual(3, completions.Count());

            string[] expected = { "Imperative", "Minimal", "MinimalTracedClass" };
            var actual = completions.Select(x => x.Text).OrderBy(x => x);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        [Category("UnitTests")]
        public void TestCompletionOnType1()
        {
            var codeCompletionServices = new CodeCompletionServices(libraryServicesCore);
            var completions = codeCompletionServices.GetCompletionsOnType("", "CodeCompletionClass");
            Assert.AreEqual(6, completions.Count());
        }

        [Test]
        [Category("UnitTests")]
        public void TestCompletionOnType2()
        {
            var codeCompletionServices = new CodeCompletionServices(libraryServicesCore);
            var completions = codeCompletionServices.GetCompletionsOnType("x : CodeCompletionClass", "x");
            Assert.AreEqual(5, completions.Count());
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
            Assert.AreEqual(3, completions.Count());

            string[] expected = { "SampleClassA", "SampleClassC", "TestSamePropertyName" };
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
            Assert.AreEqual(2, completions.Count());

            string[] expected = { "bool", "BooleanMember" };
            var actual = completions.Select(x => x.Text).OrderBy(x => x);
            Assert.AreEqual(expected, actual);
        }
    }
}
