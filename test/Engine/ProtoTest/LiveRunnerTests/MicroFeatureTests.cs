﻿using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
using ProtoTest.TD;
using ProtoScript.Runners;
using ProtoTestFx.TD;
using System.Linq;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using ProtoCore.Mirror;
using System.Collections;

namespace ProtoTest.LiveRunner
{
    class MicroFeatureTests : ProtoTestBase
    {
        private ProtoScript.Runners.LiveRunner astLiveRunner = null;
        private Random randomGen = new Random();

        public override void Setup()
        {
            base.Setup();
            astLiveRunner = new ProtoScript.Runners.LiveRunner();
            astLiveRunner.ResetVMAndResyncGraph(new List<string> { "FFITarget.dll" });
        }

        public override void TearDown()
        {
            base.TearDown();
            astLiveRunner.Dispose();
        }

        [Test]
        public void GraphILTest_Assign01()
        {
            // Build the AST trees
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.IntNode(10),
                ProtoCore.DSASM.Operator.assign);
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign);

            // Instantiate GraphSyncData
            List<Subtree> addedList = new List<Subtree>();
            addedList.Add(new Subtree(astList, System.Guid.NewGuid()));
            GraphSyncData syncData = new GraphSyncData(null, addedList, null);

            // emit the DS code from the AST tree
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();
            liveRunner.UpdateGraph(syncData);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 10);
        }

        [Test]
        public void GraphILTest_Assign01_AstInput()
        {
            // Build the AST trees
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.IntNode(10),
                ProtoCore.DSASM.Operator.assign);
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign);

            // Update graph using AST node input
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();
            liveRunner.UpdateGraph(assign);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 10);
        }



        [Test]
        public void GraphILTest_Assign01a()
        {
            ////////////////////////////////////////////////////////////////////
            // Adds a node => a = 10;
            // Creates Subtree, Deletes the node,
            // Creates Subtree and sync data and executes it via delta execution
            ////////////////////////////////////////////////////////////////////

            // Build the AST trees
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.IntNode(10),
                ProtoCore.DSASM.Operator.assign);
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign);

            // Instantiate GraphSyncData
            List<Subtree> addedList = new List<Subtree>();
            System.Guid guid1 = System.Guid.NewGuid();
            addedList.Add(new Subtree(astList, guid1));
            GraphSyncData syncData = new GraphSyncData(null, addedList, null);

            // emit the DS code from the AST tree
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();
            liveRunner.UpdateGraph(syncData);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 10);

            astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign);
            List<Subtree> deletedList = new List<Subtree>();
            deletedList.Add(new Subtree(astList, guid1));

            syncData = new GraphSyncData(deletedList, null, null);
            // emit the DS code from the AST tree
            liveRunner.UpdateGraph(syncData);

            mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue(mirror.GetData().IsNull);
        }

        [Test]
        public void GraphILTest_Assign02()
        {
            ////////////////////////////////////////////////////////////////////
            // Adds a node => a = 10 + 20;
            // Creates Subtree and sync data and executes it via delta execution
            ////////////////////////////////////////////////////////////////////

            // Build the AST tree
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.IntNode(10),
                    new ProtoCore.AST.AssociativeAST.IntNode(20),
                    ProtoCore.DSASM.Operator.add),
                ProtoCore.DSASM.Operator.assign);
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign);

            // Instantiate GraphSyncData
            List<Subtree> addedList = new List<Subtree>();
            addedList.Add(new Subtree(astList, System.Guid.NewGuid()));
            GraphSyncData syncData = new GraphSyncData(null, addedList, null);

            // emit the DS code from the AST tree
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();
            liveRunner.UpdateGraph(syncData);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 30);
        }

        [Test]
        public void GraphILTest_Assign02_AstInput()
        {
            ////////////////////////////////////////////////////////////////////
            // Adds a node => a = 10 + 20;
            // Creates Subtree and sync data and executes it via delta execution
            ////////////////////////////////////////////////////////////////////

            // Build the AST tree
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.IntNode(10),
                    new ProtoCore.AST.AssociativeAST.IntNode(20),
                    ProtoCore.DSASM.Operator.add),
                ProtoCore.DSASM.Operator.assign);

            // emit the DS code from the AST tree
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();
            liveRunner.UpdateGraph(assign);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 30);
        }

        [Test]
        public void GraphILTest_Assign03_astInput()
        {
            ////////////////////////////////////////////////////////////////////
            // Adds nodes => a = 10; c = 20; b = a + c;
            // Creates 3 separate Subtrees 
            ////////////////////////////////////////////////////////////////////

            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();

            // Build the AST trees
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign1 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.IntNode(10),
                ProtoCore.DSASM.Operator.assign);

            astList.Add(assign1);

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign2 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                new ProtoCore.AST.AssociativeAST.IntNode(20),
                ProtoCore.DSASM.Operator.assign);

            astList.Add(assign2);

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign3 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                    ProtoCore.DSASM.Operator.add),
                ProtoCore.DSASM.Operator.assign);

            astList.Add(assign3);

            // update graph with ast input
            CodeBlockNode cNode = new CodeBlockNode();
            cNode.Body = astList;
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();
            liveRunner.UpdateGraph(cNode);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("b");
            Assert.IsTrue((Int64)mirror.GetData().Data == 30);
        }

        [Test]
        public void GraphILTest_Assign04_astInput()
        {
            ////////////////////////////////////////////////////////////////////
            // Adds nodes => a = 10; 
            // executes it
            // Adds node => c = 20;
            // executes it
            // Adds node => b = a + c;
            // executes it
            ////////////////////////////////////////////////////////////////////

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            // Build the AST trees
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign1 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.IntNode(10),
                ProtoCore.DSASM.Operator.assign);

            // update graph
            liveRunner.UpdateGraph(assign1);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 10);

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign2 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                new ProtoCore.AST.AssociativeAST.IntNode(20),
                ProtoCore.DSASM.Operator.assign);

            // update graph
            liveRunner.UpdateGraph(assign2);

            mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 10);
            mirror = liveRunner.InspectNodeValue("c");
            Assert.IsTrue((Int64)mirror.GetData().Data == 20);

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign3 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                    ProtoCore.DSASM.Operator.add),
                ProtoCore.DSASM.Operator.assign);

            // update graph
            liveRunner.UpdateGraph(assign3);

            mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 10);
            mirror = liveRunner.InspectNodeValue("c");
            Assert.IsTrue((Int64)mirror.GetData().Data == 20);
            mirror = liveRunner.InspectNodeValue("b");
            Assert.IsTrue((Int64)mirror.GetData().Data == 30);
        }

        [Test]
        public void GraphILTest_Assign05()
        {
            ////////////////////////////////////////////////////////////////////
            // Adds nodes => a = 10; 
            // executes it
            // Adds node => c = 20;
            // executes it
            // Adds node => b = a + c;
            // executes it
            // deletes node => c = 20;
            // executes updated graph
            ////////////////////////////////////////////////////////////////////

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();

            // Build the AST trees
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign1 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.IntNode(10),
                ProtoCore.DSASM.Operator.assign);

            astList.Add(assign1);
            List<Subtree> addedList = new List<Subtree>();
            addedList.Add(new Subtree(astList, System.Guid.NewGuid()));

            // Instantiate GraphSyncData
            GraphSyncData syncData = new GraphSyncData(null, addedList, null);
            // emit the DS code from the AST tree
            liveRunner.UpdateGraph(syncData);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 10);

            //string o = liveRunner.GetCoreDump();

            astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign2 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                new ProtoCore.AST.AssociativeAST.IntNode(20),
                ProtoCore.DSASM.Operator.assign);

            astList.Add(assign2);
            addedList = new List<Subtree>();
            System.Guid guid1 = System.Guid.NewGuid();
            addedList.Add(new Subtree(astList, guid1));

            syncData = new GraphSyncData(null, addedList, null);
            // emit the DS code from the AST tree
            liveRunner.UpdateGraph(syncData);

            mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 10);
            mirror = liveRunner.InspectNodeValue("c");
            Assert.IsTrue((Int64)mirror.GetData().Data == 20);

            //string o = liveRunner.GetCoreDump();

            astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign3 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                    new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                    ProtoCore.DSASM.Operator.add),
                ProtoCore.DSASM.Operator.assign);

            astList.Add(assign3);
            addedList = new List<Subtree>();
            addedList.Add(new Subtree(astList, System.Guid.NewGuid()));

            syncData = new GraphSyncData(null, addedList, null);
            // emit the DS code from the AST tree
            liveRunner.UpdateGraph(syncData);

            mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 10);
            mirror = liveRunner.InspectNodeValue("c");
            Assert.IsTrue((Int64)mirror.GetData().Data == 20);
            mirror = liveRunner.InspectNodeValue("b");
            Assert.IsTrue((Int64)mirror.GetData().Data == 30);

            //o = liveRunner.GetCoreDump();

            astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign2);
            List<Subtree> deletedList = new List<Subtree>();
            deletedList.Add(new Subtree(astList, guid1));

            syncData = new GraphSyncData(deletedList, null, null);
            // emit the DS code from the AST tree
            liveRunner.UpdateGraph(syncData);

            mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 10);
            mirror = liveRunner.InspectNodeValue("c");
            Assert.IsTrue(mirror.GetData().IsNull);
            mirror = liveRunner.InspectNodeValue("b");
            Assert.IsTrue(mirror.GetData().IsNull);

            //o = liveRunner.GetCoreDump();
        }

        [Test]
        public void GraphILTest_ModifiedNode01()
        {
            ////////////////////////////////////////////////////////////////////
            // Adds nodes => c = 78; d = a;
            // Create subtree, execute
            // Adds nodes => a = 10; 
            // Adds node => b = a;
            // Create subtree, execute
            // Modify subtree => a = b;
            // execute updated graph (cylcic dependency should not occur)
            ////////////////////////////////////////////////////////////////////

            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign0 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("c"),
                new ProtoCore.AST.AssociativeAST.IntNode(78),
                ProtoCore.DSASM.Operator.assign);

            astList.Add(assign0);

            assign0 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("d"),
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                ProtoCore.DSASM.Operator.assign);

            astList.Add(assign0);

            List<Subtree> addedList = new List<Subtree>();
            System.Guid guid0 = System.Guid.NewGuid();
            addedList.Add(new Subtree(astList, guid0));

            GraphSyncData syncData = new GraphSyncData(null, addedList, null);
            // emit the DS code from the AST tree
            liveRunner.UpdateGraph(syncData);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("c");
            Assert.IsTrue((Int64)mirror.GetData().Data == 78);

            mirror = liveRunner.InspectNodeValue("d");
            Assert.IsTrue(mirror.GetData().IsNull);

            // Build the AST trees
            astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign1 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.IntNode(10),
                ProtoCore.DSASM.Operator.assign);

            astList.Add(assign1);

            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign2 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                ProtoCore.DSASM.Operator.assign);

            astList.Add(assign2);
            addedList = new List<Subtree>();
            System.Guid guid1 = System.Guid.NewGuid();
            addedList.Add(new Subtree(astList, guid1));

            syncData = new GraphSyncData(null, addedList, null);
            // emit the DS code from the AST tree
            liveRunner.UpdateGraph(syncData);

            mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 10);
            mirror = liveRunner.InspectNodeValue("b");
            Assert.IsTrue((Int64)mirror.GetData().Data == 10);
            mirror = liveRunner.InspectNodeValue("c");
            Assert.IsTrue((Int64)mirror.GetData().Data == 78);
            mirror = liveRunner.InspectNodeValue("d");
            Assert.IsTrue((Int64)mirror.GetData().Data == 10);

            astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign3 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                ProtoCore.DSASM.Operator.assign);

            astList.Add(assign3);
            List<Subtree> modifiedList = new List<Subtree>();
            modifiedList.Add(new Subtree(astList, guid1));

            syncData = new GraphSyncData(null, null, modifiedList);
            // emit the DS code from the AST tree
            liveRunner.UpdateGraph(syncData);

            mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 10);

            mirror = liveRunner.InspectNodeValue("b");
            Assert.IsTrue((Int64)mirror.GetData().Data == 10);

            mirror = liveRunner.InspectNodeValue("c");
            Assert.IsTrue((Int64)mirror.GetData().Data == 78);

            mirror = liveRunner.InspectNodeValue("d");
            Assert.IsTrue((Int64)mirror.GetData().Data == 10);
        }
     

        [Test]
        public void TestDeltaExpression_01()
        {
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            // emit the DS code from the AST tree
            liveRunner.UpdateCmdLineInterpreter("a=10;");

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 10);

            //string o = liveRunner.GetCoreDump();

            // emit the DS code from the AST tree
            liveRunner.UpdateCmdLineInterpreter("c=20;");

            mirror = liveRunner.InspectNodeValue("c");
            Assert.IsTrue((Int64)mirror.GetData().Data == 20);
            mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 10);

            //string o = liveRunner.GetCoreDump();

            // emit the DS code from the AST tree
            liveRunner.UpdateCmdLineInterpreter("b = a+c;");

            mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 10);
            mirror = liveRunner.InspectNodeValue("c");
            Assert.IsTrue((Int64)mirror.GetData().Data == 20);
            mirror = liveRunner.InspectNodeValue("b");
            Assert.IsTrue((Int64)mirror.GetData().Data == 30);

            //o = liveRunner.GetCoreDump();

            // emit the DS code from the AST tree
            liveRunner.UpdateCmdLineInterpreter("c= 30;");

            mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 10);
            mirror = liveRunner.InspectNodeValue("c");
            Assert.IsTrue((Int64)mirror.GetData().Data == 30);
            mirror = liveRunner.InspectNodeValue("b");
            Assert.IsTrue((Int64)mirror.GetData().Data == 40);

            //o = liveRunner.GetCoreDump();
        }

        [Test]
        public void TestDeltaExpression_02()
        {
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            // emit the DS code from the AST tree
            liveRunner.UpdateCmdLineInterpreter("x=99;");

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("x");
            Assert.IsTrue((Int64)mirror.GetData().Data == 99);

            //string o = liveRunner.GetCoreDump();

            // emit the DS code from the AST tree
            liveRunner.UpdateCmdLineInterpreter("y=x;");

            mirror = liveRunner.InspectNodeValue("y");
            Assert.IsTrue((Int64)mirror.GetData().Data == 99);
            mirror = liveRunner.InspectNodeValue("x");
            Assert.IsTrue((Int64)mirror.GetData().Data == 99);

            //string o = liveRunner.GetCoreDump();

            // emit the DS code from the AST tree
            liveRunner.UpdateCmdLineInterpreter("x = 100;");

            mirror = liveRunner.InspectNodeValue("x");
            Assert.IsTrue((Int64)mirror.GetData().Data == 100);
            mirror = liveRunner.InspectNodeValue("y");
            Assert.IsTrue((Int64)mirror.GetData().Data == 100);
        }

        [Test]
        [Category("PortToCodeBlocks")]
        public void TestDeltaExpressionFFI_01()
        {
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            liveRunner.UpdateCmdLineInterpreter(@"import (""FFITarget.dll"");");
            liveRunner.UpdateCmdLineInterpreter("p = DummyPoint.ByCoordinates(10,10,10);");

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("p");

            //==============================================
            // Translate the point
            // newPoint = p.Translate(1,2,3);
            //==============================================

            liveRunner.UpdateCmdLineInterpreter("newPoint = p.Translate(1,2,3);");
            mirror = liveRunner.InspectNodeValue("newPoint");

            //==============================================
            // Build a binary expression to retirieve the x property
            // xval = newPoint.X
            //==============================================
            liveRunner.UpdateCmdLineInterpreter("xval = newPoint.X;");
            mirror = liveRunner.InspectNodeValue("xval");

            //==============================================
            //
            // import ("ProtoGeometry.dll");
            // p = Point.Bycoordinates(10.0, 10.0, 10.0);
            // newPoint = p.Translate(1.0,2.0,3.0);
            // xval = newPoint.X;
            //
            //==============================================
            Assert.IsTrue((double)mirror.GetData().Data == 11.0);

        }

        [Test]
        [Category("PortToCodeBlocks")]
        public void TestDeltaExpressionFFI_02()
        {
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();

            //string code = @"class Point{ X : double; constructor ByCoordinates(x : double, y : double, z : double){X = x;} def Translate(x : double, y : double, z : double){return = Point.ByCoordinates(11,12,13);} }";

            //liveRunner.UpdateCmdLineInterpreter(code);
            liveRunner.UpdateCmdLineInterpreter(@"import (""FFITarget.dll"");");
            liveRunner.UpdateCmdLineInterpreter("p = DummyPoint.ByCoordinates(10,10,10);");

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("p");

            //==============================================
            // Build a binary expression to retirieve the x property
            // xval = newPoint.X
            //==============================================
            liveRunner.UpdateCmdLineInterpreter("xval = p.X;");
            mirror = liveRunner.InspectNodeValue("xval");

            //==============================================
            //
            // import ("ProtoGeometry.dll");
            // p = Point.Bycoordinates(10.0, 10.0, 10.0);
            // newPoint = p.Translate(1.0,2.0,3.0);
            // xval = newPoint.X;
            //
            //==============================================
            Assert.IsTrue((double)mirror.GetData().Data == 10.0);

            //==============================================
            // Translate the point
            // newPoint = p.Translate(1,2,3);
            //==============================================

            liveRunner.UpdateCmdLineInterpreter("p = p.Translate(1,2,3);");

            mirror = liveRunner.InspectNodeValue("p");

            mirror = liveRunner.InspectNodeValue("xval");

            //==============================================
            //
            // import ("ProtoGeometry.dll");
            // p = Point.Bycoordinates(10.0, 10.0, 10.0);
            // newPoint = p.Translate(1.0,2.0,3.0);
            // xval = newPoint.X;
            //
            //==============================================
            Assert.IsTrue((double)mirror.GetData().Data == 11.0);

        }

        [Test]
        public void GraphILTest_ComplexWatch01()
        {
            // Build the AST trees
            // x = 1..10;
            ProtoCore.AST.AssociativeAST.RangeExprNode rangeExpr = new ProtoCore.AST.AssociativeAST.RangeExprNode();
            rangeExpr.FromNode = new ProtoCore.AST.AssociativeAST.IntNode(0);
            rangeExpr.ToNode = new ProtoCore.AST.AssociativeAST.IntNode(5);
            rangeExpr.StepNode = new ProtoCore.AST.AssociativeAST.IntNode(1);
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                rangeExpr,
                ProtoCore.DSASM.Operator.assign);
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign);

            // Instantiate GraphSyncData
            List<Subtree> addedList = new List<Subtree>();
            addedList.Add(new Subtree(astList, System.Guid.NewGuid()));
            GraphSyncData syncData = new GraphSyncData(null, addedList, null);

            // emit the DS code from the AST tree
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();
            liveRunner.UpdateGraph(syncData);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("a");
            var collection = mirror.GetData().GetElements();
            Assert.IsTrue((Int64)collection[1].Data == 1);
        }


        [Test]
        public void GraphILTest_DeletedNode01()
        {
            //====================================
            // Create a = 10 
            // Execute and verify a = 10
            // Delete a = 10
            // Create b = a
            // Execute and verify b = null
            //====================================

            // Create a = 10 
            // Execute and verify a = 10
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                new ProtoCore.AST.AssociativeAST.IntNode(10),
                ProtoCore.DSASM.Operator.assign);
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign);

            Guid guid = System.Guid.NewGuid();

            // Instantiate GraphSyncData
            List<Subtree> addedList = new List<Subtree>();
            addedList.Add(new Subtree(astList, guid));
            GraphSyncData syncData = new GraphSyncData(null, addedList, null);

            // emit the DS code from the AST tree
            ProtoScript.Runners.ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();
            liveRunner.UpdateGraph(syncData);

            ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 10);




            // Delete a = 10
            List<Subtree> deletedList = new List<Subtree>();
            deletedList.Add(new Subtree(null, guid));
            syncData = new GraphSyncData(deletedList, null, null);
            liveRunner.UpdateGraph(syncData);



            // Create b = a 
            // Execute and verify b = null
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode assign2 = new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(
                new ProtoCore.AST.AssociativeAST.IdentifierNode("b"),
                new ProtoCore.AST.AssociativeAST.IdentifierNode("a"),
                ProtoCore.DSASM.Operator.assign);
            astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            astList.Add(assign2);

            guid = System.Guid.NewGuid();

            // Instantiate GraphSyncData
            addedList = new List<Subtree>();
            addedList.Add(new Subtree(astList, guid));
            syncData = new GraphSyncData(null, addedList, null);

            // emit the DS code from the AST tree
            liveRunner.UpdateGraph(syncData);

            mirror = liveRunner.InspectNodeValue("b");
            Assert.IsTrue(mirror.GetData().IsNull);

        }

        private void AssertValue(string varname, object value)
        {
            var mirror = astLiveRunner.InspectNodeValue(varname);
            MirrorData data = mirror.GetData();
            object svValue = data.Data;
            if (value is double)
            {
                Assert.AreEqual((double)svValue, Convert.ToDouble(value));
            }
            else if (value is int)
            {
                Assert.AreEqual((Int64)svValue, Convert.ToInt64(value));
            }
            else if (value is bool)
            {
                Assert.AreEqual((bool)svValue, Convert.ToBoolean(value));
            }
            else if (value is IEnumerable<int>)
            {
                Assert.IsTrue(data.IsCollection);
                var values = (value as IEnumerable<int>).ToList().Select(v => (object)v).ToList();
                Assert.IsTrue(mirror.GetUtils().CompareArrays(varname, values, typeof(Int64)));
            }
            else if (value is IEnumerable<double>)
            {
                Assert.IsTrue(data.IsCollection);
                var values = (value as IEnumerable<double>).ToList().Select(v => (object)v).ToList();
                Assert.IsTrue(mirror.GetUtils().CompareArrays(varname, values, typeof(double)));
            }
        }

        [Test]
        [Category("PortToCodeBlocks")]
        public void TestAdd01()
        {
            List<string> codes = new List<string>() 
            {
                "a = 1;",
                "x = a; y = a; z = a; p = DummyPoint.ByCoordinates(x, y, z); px = p.X;",
            };
            List<Guid> guids = Enumerable.Range(0, codes.Count).Select(_ => System.Guid.NewGuid()).ToList();
            IEnumerable<int> index = Enumerable.Range(0, codes.Count);

            int shuffleCount = codes.Count;

            // in which add order, LiveRunner should get the same result.
            for (int i = 0; i < shuffleCount; ++i)
            {
                ILiveRunner liveRunner = new ProtoScript.Runners.LiveRunner();
                liveRunner.ResetVMAndResyncGraph(new List<string> { "FFITarget.dll" });

                index = index.OrderBy(_ => randomGen.Next());
                var added = index.Select(idx => ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guids[idx], codes[idx])).ToList();

                var syncData = new GraphSyncData(null, added, null);
                liveRunner.UpdateGraph(syncData);

                ProtoCore.Mirror.RuntimeMirror mirror = liveRunner.InspectNodeValue("px");
                var value = (double)mirror.GetData().Data;
                Assert.AreEqual(value, 1);
            }
        }


        [Test]
        [Category("PortToCodeBlocks")]
        public void TestModify01()
        {
            List<string> codes = new List<string>() 
            {
                "a = 1;",
                "x = a; y = a; z = a; p = DummyPoint.ByCoordinates(x, y, z); px = p.X;",
            };
            List<Guid> guids = Enumerable.Range(0, codes.Count).Select(_ => System.Guid.NewGuid()).ToList();

            // add two nodes
            IEnumerable<int> index = Enumerable.Range(0, codes.Count);
            var added = index.Select(idx => ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guids[idx], codes[idx])).ToList();

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            ProtoCore.Mirror.RuntimeMirror mirror = astLiveRunner.InspectNodeValue("px");
            var value = (double)mirror.GetData().Data;
            Assert.AreEqual(value, 1);

            for (int i = 0; i < 10; ++i)
            {
                codes[0] = "a = " + i.ToString() + ";";
                var modified = index.Select(idx => ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guids[idx], codes[idx])).ToList();

                syncData = new GraphSyncData(null, null, modified);
                astLiveRunner.UpdateGraph(syncData);

                mirror = astLiveRunner.InspectNodeValue("px");
                value = (double)mirror.GetData().Data;
                Assert.AreEqual(value, i);
            }
        }

        [Test]
        public void RegressMAGN747()
        {
            List<string> codes = new List<string>() 
            {
                "a = 1;",
            };
            Guid guid = System.Guid.NewGuid();

            // add two nodes
            IEnumerable<int> index = Enumerable.Range(0, codes.Count);
            var added = index.Select(idx => ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[idx])).ToList();

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            ProtoCore.Mirror.RuntimeMirror mirror = astLiveRunner.InspectNodeValue("a");
            var value = (Int64)mirror.GetData().Data;
            Assert.AreEqual(value, 1);

            // Simulate delete a = 1 and add CBN a = 2
            int newval = 2;
            codes[0] = "a = " + newval.ToString() + ";";
            var modified = index.Select(idx => ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[idx])).ToList();

            List<Subtree> deletedList = new List<Subtree>();
            deletedList.Add(new Subtree(null, guid));

            syncData = new GraphSyncData(deletedList, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            Console.WriteLine("a = " + astLiveRunner.InspectNodeValue("a").GetStringData());
            AssertValue("a", newval);

        }

        [Test]
        public void RegressMAGN750()
        {
            List<string> codes = new List<string>() 
            {
                "a = 1;",
                "b = a; c = b + 1;",
            };
            List<Guid> guids = Enumerable.Range(0, codes.Count).Select(_ => System.Guid.NewGuid()).ToList();

            // add two nodes
            IEnumerable<int> index = Enumerable.Range(0, codes.Count);
            var added = index.Select(idx => ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guids[idx], codes[idx])).ToList();

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            ProtoCore.Mirror.RuntimeMirror mirror = astLiveRunner.InspectNodeValue("c");
            var value = (Int64)mirror.GetData().Data;
            Assert.AreEqual(value, 2);

            for (int i = 0; i < 10; ++i)
            {
                codes[0] = "a = " + i.ToString() + ";";
                var modified = index.Select(idx => ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guids[idx], codes[idx])).ToList();

                syncData = new GraphSyncData(null, null, modified);
                astLiveRunner.UpdateGraph(syncData);

                Console.WriteLine("c = " + astLiveRunner.InspectNodeValue("c").GetStringData());
                AssertValue("c", i + 1);
            }
        }



        [Test]
        public void RegressMAGN750_1()
        {
            List<string> codes = new List<string>() 
            {
                "a = 1;",
                "b = a; c = b + 1;",
            };
            List<Guid> guids = Enumerable.Range(0, codes.Count).Select(_ => System.Guid.NewGuid()).ToList();

            // add two nodes
            IEnumerable<int> index = Enumerable.Range(0, codes.Count);
            var added = index.Select(idx => ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guids[idx], codes[idx])).ToList();

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            ProtoCore.Mirror.RuntimeMirror mirror = astLiveRunner.InspectNodeValue("c");
            var value = (Int64)mirror.GetData().Data;
            Assert.AreEqual(value, 2);

            int newval = 2;
            codes[0] = "a = " + newval.ToString() + ";";
            var modified = index.Select(idx => ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guids[idx], codes[idx])).ToList();

            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            Console.WriteLine("c = " + astLiveRunner.InspectNodeValue("c").GetStringData());
            AssertValue("c", newval + 1);

        }

        [Test]
        [Category("PortToCodeBlocks")]
        public void RegressMAGN753()
        {
            List<string> codes = new List<string>() 
            {
                "t = 1..2;",
                "x = t; a = x;",
                "z = a; pts = DummyPoint.ByCoordinates(z, 10, 2); ptsx = pts.X;"
            };
            List<Guid> guids = Enumerable.Range(0, codes.Count).Select(_ => System.Guid.NewGuid()).ToList();

            // add two nodes
            IEnumerable<int> index = Enumerable.Range(0, codes.Count);
            var added = index.Select(idx => ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guids[idx], codes[idx])).ToList();

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            for (int i = 1; i <= 10; ++i)
            {
                codes[0] = "t = 0.." + i.ToString() + ";";
                var modified = index.Select(idx => ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guids[idx], codes[idx])).ToList();

                syncData = new GraphSyncData(null, null, modified);
                astLiveRunner.UpdateGraph(syncData);

                Console.WriteLine("a = " + astLiveRunner.InspectNodeValue("a").GetStringData());
                AssertValue("a", Enumerable.Range(0, i + 1));

                Console.WriteLine("ptsx = " + astLiveRunner.InspectNodeValue("ptsx").GetStringData());
                AssertValue("ptsx", Enumerable.Range(0, i + 1));

                Console.WriteLine("pts = " + astLiveRunner.InspectNodeValue("pts").GetStringData());
                Assert.IsTrue(!string.IsNullOrEmpty(astLiveRunner.InspectNodeValue("pts").GetStringData()));
            }
        }

        [Test]
        public void RegressMAGN765()
        {
            List<string> codes = new List<string>() 
            {
                "a=10;b=20;c=30;",
                "var1=DummyPoint.ByCoordinates(a,b,c);",
                "var2=DummyPoint.ByCoordinates(a,a,c);"
            };
            List<Guid> guids = Enumerable.Range(0, codes.Count).Select(_ => System.Guid.NewGuid()).ToList();

            // add two nodes
            IEnumerable<int> index = Enumerable.Range(0, codes.Count);
            var added = index.Select(idx => ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guids[idx], codes[idx])).ToList();

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            for (int i = 1; i <= 10; ++i)
            {
                codes[0] = "a=10;b=20;c=" + i.ToString() + ";";
                var modified = index.Select(idx => ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guids[idx], codes[idx])).ToList();

                syncData = new GraphSyncData(null, null, modified);
                astLiveRunner.UpdateGraph(syncData);

                var var1_value = astLiveRunner.InspectNodeValue("var1").GetStringData();
                Console.WriteLine("var1 = " + var1_value);
                Assert.IsTrue(!string.IsNullOrEmpty(var1_value));

                var var2_value = astLiveRunner.InspectNodeValue("var2").GetStringData();
                Console.WriteLine("var2 = " + var2_value);
                Assert.IsTrue(!string.IsNullOrEmpty(var2_value));
            }
        }

        [Test]
        public void RegressMAGN773()
        {
            List<string> codes = new List<string>() 
            {
                "h=1;",
                "k=h;ll=k+2;",
                "v=ll;hf=v+2;",
                "a45=hf;vv=DummyPoint.ByCoordinates(a45, 3, 1);"
            };
            List<Guid> guids = Enumerable.Range(0, codes.Count).Select(_ => System.Guid.NewGuid()).ToList();

            // add two nodes
            IEnumerable<int> index = Enumerable.Range(0, codes.Count);
            var added = index.Select(idx => ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guids[idx], codes[idx])).ToList();

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            for (int i = 1; i <= 10; ++i)
            {
                codes[0] = "h=1.." + i.ToString() + ";";

                index = Enumerable.Range(0, 2);
                var modified = index.Select(idx => ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guids[idx], codes[idx])).ToList();

                syncData = new GraphSyncData(null, null, modified);
                astLiveRunner.UpdateGraph(syncData);

                var strValue = astLiveRunner.InspectNodeValue("vv").GetStringData();
                Console.WriteLine("vv = " + strValue);
                Assert.IsTrue(!string.IsNullOrEmpty(strValue));

                strValue = astLiveRunner.InspectNodeValue("hf").GetStringData();
                Console.WriteLine("hf = " + strValue);
                Assert.IsTrue(!string.IsNullOrEmpty(strValue));
            }
        }

        [Test]
        public void TestFunctionDefinition01()
        {
            List<string> codes = new List<string>() 
            {
                "def f() { return = 1; } x = f();"
            };

            Guid guid = System.Guid.NewGuid();

            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[0]));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("x", 1);
        }

        [Test]
        public void TestFunctionDefinitionWithLanguageblocks01()
        {
            List<string> codes = new List<string>() 
            {
                @"
                def f(x:int) 
                { 
                    return = [Imperative]
                    {
                        return = x + 1; 
                    }
                } 
                y = f(1);"
            };

            Guid guid = System.Guid.NewGuid();

            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[0]));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("y", 2);
        }


        [Test]
        public void TestFunctionModification01()
        {
            List<string> codes = new List<string>() 
            {
                "def f() { t = 41; return = t;} x = f();",
                "def f() { t1 = 41; t2 = 42; return = {t1, t2}; } x = f();",
                "def f() { t1 = 41; t2 = 42; t3 = 43; return = {t1, t2, t3};} x = f();"
            };

            Guid guid = System.Guid.NewGuid();

            {
                List<Subtree> added = new List<Subtree>();
                added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[0]));

                var syncData = new GraphSyncData(null, added, null);
                astLiveRunner.UpdateGraph(syncData);

                AssertValue("x", 41);
            }

            {
                // Modify the function and verify
                List<Subtree> modified = new List<Subtree>();
                modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[1]));

                var syncData = new GraphSyncData(null, null, modified);
                astLiveRunner.UpdateGraph(syncData);

                AssertValue("x", new int[] { 41, 42 });
            }

            {
                // Modify the function and verify
                List<Subtree> modified = new List<Subtree>();
                modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[2]));

                var syncData = new GraphSyncData(null, null, modified);
                astLiveRunner.UpdateGraph(syncData);

                AssertValue("x", new int[] { 41, 42, 43 });
            }
        }

        [Test]
        public void TestFunctionModification02()
        {
            // Test function re-defintion but without parameters
            List<string> codes = new List<string>() 
            {
                "def f() { t = 41; return = t;} x = f(); r = Equals(x, 41);",
                "def f() { t1 = 41; t2 = 42; return = {t1, t2}; } x = f(); r = Equals(x, {41, 42});",
                "def f() { t1 = 41; t2 = 42; t3 = 43; return = {t1, t2, t3};} x = f(); r = Equals(x, {41, 42, 43});",
                "def f() { t1 = 43; t2 = 42; t3 = 41; return = {t1, t2, t3};} x = f(); r = Equals(x, {43, 42, 41});",
                "def f() { t1 = t2 + t3; return = t;} x = f(); r = Equals(x, null);",
                "def f() { t1 = 2; t2 = 5; t3 = t1..t2; return = t3;} x = f(); r = Equals(x, {2, 3, 4, 5});",
                "def f() { t1 = 2; t2 = 5; t3 = t1..t2..#2; return = t3;} x = f(); r = Equals(x, {2, 5});",
                "def f() { a = 1; b = 2; c = (a == b) ? 3 : 4; return = c; } x = f(); r = Equals(x, 4);",
                "def f() { a = 2; b = 2; c = (a == b) ? 3 : 4; return = c; } x = f(); r = Equals(x, 3);",
            };

            Guid guid = System.Guid.NewGuid();

            {
                List<Subtree> added = new List<Subtree>();
                added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[0]));

                var syncData = new GraphSyncData(null, added, null);
                astLiveRunner.UpdateGraph(syncData);

                AssertValue("r", true);
            }

            IEnumerable<int> indexes = Enumerable.Range(0, codes.Count);
            int shuffleCount = codes.Count;

            for (int i = 0; i < shuffleCount; ++i)
            {
                indexes = indexes.OrderBy(_ => randomGen.Next());

                foreach (var index in indexes)
                {
                    List<Subtree> modified = new List<Subtree>();
                    modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[index]));

                    var syncData = new GraphSyncData(null, null, modified);
                    astLiveRunner.UpdateGraph(syncData);

                    AssertValue("r", true);
                }
            }
        }

        [Test]
        public void TestFunctionModification03()
        {
            // Test function re-defintion but without parameters
            List<string> codes = new List<string>() 
            {
                @"
def f() 
{ 
    t = 41; 
    return = t;
} 
x = f(); 
r = Equals(x, 41);
",
                @"
def f() 
{ 
    t1 = 41; 
    t2 = 42; 
    return = {t1, t2}; 
} 
x = f(); 
r = Equals(x, {41, 42});
",
            };

            Guid guid = System.Guid.NewGuid();

            // Create CBN
            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("r", true);

            // Modify CBN
            List<Subtree> modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[1]));
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("r", true);
        }


        [Test]
        public void TestFunctionModification04()
        {
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4161
            // AST node to string is causing a crash

            // Test function re-define and should remove the old function
            List<string> codes = new List<string>() 
            {
                "def f() { return = 41; } x = f(); r1 = Equals(x, 41); y = f(0); r2 = Equals(y, null); r = r1 && r2;",
                "def f(x) { return = 42;} x = f(0); r1 = Equals(x, 42); y = f(); r2 = Equals(y, null); r = r1 && r2;",
            };

            Guid guid = System.Guid.NewGuid();

            {
                List<Subtree> added = new List<Subtree>();
                added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[0]));

                var syncData = new GraphSyncData(null, added, null);
                astLiveRunner.UpdateGraph(syncData);

                AssertValue("r", true);
            }


            IEnumerable<int> indexes = Enumerable.Range(0, codes.Count);
            int shuffleCount = codes.Count;

            for (int i = 0; i < shuffleCount; ++i)
            {
                indexes = indexes.OrderBy(_ => randomGen.Next());

                foreach (var index in indexes)
                {
                    List<Subtree> modified = new List<Subtree>();
                    modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[index]));

                    var syncData = new GraphSyncData(null, null, modified);
                    astLiveRunner.UpdateGraph(syncData);

                    AssertValue("r", true);
                }
            }
        }

        [Test]
        public void TestSimpleFunctionRedefinition01()
        {
            List<string> codes = new List<string>() 
            {
                "def f() { return = 5;} x = f();",
                "def f() { return = 10; } x = f();"
            };

            Guid guid = System.Guid.NewGuid();

            {
                List<Subtree> added = new List<Subtree>();
                added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[0]));

                var syncData = new GraphSyncData(null, added, null);
                astLiveRunner.UpdateGraph(syncData);

                AssertValue("x", 5);
            }

            {
                // Modify the function and verify
                List<Subtree> modified = new List<Subtree>();
                modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[1]));

                var syncData = new GraphSyncData(null, null, modified);
                astLiveRunner.UpdateGraph(syncData);

                AssertValue("x", 10);
            }
        }

        [Test]
        public void TestSimpleFunctionRedefinition02()
        {
            List<string> codes = new List<string>() 
            {
                "def f() { return = null;}",
                "def f() { return = 10; }",
                "x = f();"
            };

            Guid guid1 = System.Guid.NewGuid();
            Guid guid2 = System.Guid.NewGuid();

            {
                List<Subtree> added = new List<Subtree>();
                added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));

                var syncData = new GraphSyncData(null, added, null);
                astLiveRunner.UpdateGraph(syncData);
            }

            {
                // Modify the function 
                List<Subtree> modified = new List<Subtree>();
                modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[1]));

                var syncData = new GraphSyncData(null, null, modified);
                astLiveRunner.UpdateGraph(syncData);
            }


            {
                // Call the function
                List<Subtree> added = new List<Subtree>();
                added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[2]));

                var syncData = new GraphSyncData(null, added, null);
                astLiveRunner.UpdateGraph(syncData);
                AssertValue("x", 10);
            }
        }

        [Test]
        public void TestSimpleFunctionRedefinition03()
        {
            List<string> codes = new List<string>() 
            {
                "def f() { return = null;}",
                "def f() { return = 10; }",
                "x = f();",
                "def f() { return = 5; }",
            };

            Guid guid1 = System.Guid.NewGuid();
            Guid guid2 = System.Guid.NewGuid();

            {
                List<Subtree> added = new List<Subtree>();
                added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));

                var syncData = new GraphSyncData(null, added, null);
                astLiveRunner.UpdateGraph(syncData);
            }

            {
                // Modify the function 
                List<Subtree> modified = new List<Subtree>();
                modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[1]));

                var syncData = new GraphSyncData(null, null, modified);
                astLiveRunner.UpdateGraph(syncData);
            }


            {
                // Call the function
                List<Subtree> added = new List<Subtree>();
                added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[2]));

                var syncData = new GraphSyncData(null, added, null);
                astLiveRunner.UpdateGraph(syncData);
                AssertValue("x", 10);
            }

            {
                // Modify the function 
                List<Subtree> modified = new List<Subtree>();
                modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[3]));

                var syncData = new GraphSyncData(null, null, modified);
                astLiveRunner.UpdateGraph(syncData);
                AssertValue("x", 5);
            }
        }

        [Test]
        public void TestSimpleFunctionRedefinition04()
        {
            List<string> codes = new List<string>() 
            {                    
                "def f(){y = 1; return = 2;} x = f();",
                "def f(){return = 1;} x = f();"
            };

            Guid guid = System.Guid.NewGuid();
            List<Subtree> added = new List<Subtree>();

            // Create CBNs
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("x", 2);

            // Modify CBN2 - remove the last line
            List<Subtree> modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[1]));
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("x", 1);
        }

        [Test]
        public void TestSimpleFunctionRedefinition05()
        {
            List<string> codes = new List<string>() 
            {                    
                "def f(x){return = x + 2;} p = f(10);",
                "def f(x){return = x - 2;} p = f(10);",
            };

            Guid guid = System.Guid.NewGuid();
            List<Subtree> added = new List<Subtree>();

            // Create CBN
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("p", 12);

            // Modify function in CBN
            List<Subtree> modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[1]));
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("p", 8);
        }

        [Test]
        public void TestFunctionRedefinitionOnNewNode01()
        {
            List<string> codes = new List<string>() 
            {
                "def f() { return = 5;} x = f();",
                "def f() { return = 10; } y = f();"
            };

            Guid guid = System.Guid.NewGuid();
            List<Subtree> added = new List<Subtree>();

            {
                added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[0]));

                var syncData = new GraphSyncData(null, added, null);
                astLiveRunner.UpdateGraph(syncData);

                AssertValue("x", 5);
            }

            // Simualate creating a new CBN for a function definition
            // This should still yield the same result as the codegen will not add the new function.
            // This error will have been handled at the front-end (UI)
            guid = System.Guid.NewGuid();
            {
                added = new List<Subtree>();
                added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[1]));

                var syncData = new GraphSyncData(null, added, null);
                astLiveRunner.UpdateGraph(syncData);

                AssertValue("y", 5);
            }
        }


        [Test]
        public void TestFunctionRedefinitionOnExistingNode01()
        {
            List<string> codes = new List<string>() 
            {
                "def f() { return = 5;}",
                "x = f();",
                "def f() { return = 10; }"
            };

            // Create 2 CBNs 

            List<Subtree> added = new List<Subtree>();


            // A CBN with function def f
            Guid guid_func = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid_func, codes[0]));

            // A new CBN that uses function f
            Guid guid = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[1]));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("x", 5);


            // Redefine the CBN
            List<Subtree> modified = new List<Subtree>();

            // Mark the CBN that uses f as modified
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[1]));

            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid_func, codes[2]));

            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that x must have automatically re-executed
            AssertValue("x", 10);

        }

        [Test]
        public void TestFunctionRedefinitionOnUnmodifiedNode01()
        {
            List<string> codes = new List<string>() 
            {
                "def f() { return = 5;}",
                "x = f();",
                "def f() { return = 10; }"
            };

            // Create 2 CBNs 

            List<Subtree> added = new List<Subtree>();


            // A CBN with function def f
            Guid guid_func = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid_func, codes[0]));

            // A new CBN that uses function f
            Guid guid = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[1]));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("x", 5);


            // Redefine the CBN
            List<Subtree> modified = new List<Subtree>();

            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid_func, codes[2]));

            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that x must have automatically re-executed
            AssertValue("x", 10);

        }

        [Test]
        public void TestFunctionRedefinitionOnUnmodifiedNode02()
        {
            List<string> codes = new List<string>() 
            {
                "def f() { return = 5;}",
                "x = f();",
                "y = f();",
                "def f() { return = 10; }"
            };

            // Create 2 CBNs 
            List<Subtree> added = new List<Subtree>();


            // A CBN with function def f
            Guid guid_func = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid_func, codes[0]));

            // A new CBN that calls function f
            Guid guid = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[1]));

            // Create another CBN that calls function d
            guid = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[2]));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("x", 5);


            // Redefine the CBN
            List<Subtree> modified = new List<Subtree>();

            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid_func, codes[3]));

            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that x must have automatically re-executed
            AssertValue("x", 10);
            AssertValue("y", 10);

        }

        [Test]
        public void TestFunctionRedefinitionWithLanguageblocks01()
        {
            List<string> codes = new List<string>() 
            {
                @"
                def f(x:int) 
                { 
                    return = [Imperative]
                    {
                        return = x + 1; 
                    }
                }",
 
                @"y = f(1);",

                @"def f(x:int) 
                { 
                    return = [Imperative]
                    {
                        return = x + 10; 
                    }
                }"
            };

            // Create 2 CBNs 
            List<Subtree> added = new List<Subtree>();


            // A CBN with function def f
            Guid guid_func = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid_func, codes[0]));

            // A new CBN that calls function f
            Guid guid = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[1]));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Verify
            AssertValue("y", 2);

            // Redefine the function
            List<Subtree> modified = new List<Subtree>();

            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid_func, codes[2]));

            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that x must have automatically re-executed
            AssertValue("y", 11);

        }

        [Test]
        public void TestFunctionRedefinitionWithLanguageblocks02()
        {
            List<string> codes = new List<string>() 
            {
                @"
                def f(x:int) 
                { 
                    return = x + 1; 
                }",
 
                @"y = f(1);",

                @"def f(x:int) 
                { 
                    return = [Imperative]
                    {
                        return = x + 10; 
                    }
                }"
            };

            // Create 2 CBNs 
            List<Subtree> added = new List<Subtree>();


            // A CBN with function def f
            Guid guid_func = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid_func, codes[0]));

            // A new CBN that calls function f
            Guid guid = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[1]));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Verify
            AssertValue("y", 2);

            // Redefine the function
            List<Subtree> modified = new List<Subtree>();

            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid_func, codes[2]));

            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that x must have automatically re-executed
            AssertValue("y", 11);

        }

        [Test]
        public void TestFunctionOverloadRedefinitionOnUnmodifiedNode01()
        {
            List<string> codes = new List<string>() 
            {
                "global = 0;",
                "def f() { global = global + 1; return = global;}",
                "def f(i : int) { return = i + 10;}",
                "x = f();",
                "y = f(2);",
                "def f(i : int) { return = i + 100; }"  // redefine the overload, it should only re-execute the overload call f(2)
            };

            List<Subtree> added = new List<Subtree>();


            // A new CBN for a global
            Guid guid = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[0]));

            // A CBN with function def f
            Guid guid_func1 = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid_func1, codes[1]));


            // A CBN with function overload def f(i)
            Guid guid_func2 = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid_func2, codes[2]));

            // CBN for calling function f
            guid = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[3]));


            // CBN for calling overload function f(i)
            guid = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[4]));


            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("x", 1);
            AssertValue("y", 12);


            // Redefine the CBN
            List<Subtree> modified = new List<Subtree>();

            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid_func2, codes[5]));

            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that the call to the function f has not re-executed
            AssertValue("x", 1);

            // Verify that the call to the overload function f(i) has re-executed
            AssertValue("y", 102);

        }

        [Test]
        public void TestFunctionOverloadRedefinitionOnUnmodifiedNode02()
        {
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4229
            List<string> codes = new List<string>() 
            {
                "global = 0;",
                "def f() { global = global + 1; return = global;}",
                "def f(i : int) { return = i + 10;}",
                "x = f();",
                "y = f(2);",
                "def f() { global = global + 1; return = global + 10;}",
                "def f(i : int) { return = i + 100; }"
            };

            List<Subtree> added = new List<Subtree>();


            // A new CBN for a global
            Guid guid = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[0]));

            // A CBN with function def f
            Guid guid_func1 = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid_func1, codes[1]));


            // A CBN with function overload def f(i)
            Guid guid_func2 = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid_func2, codes[2]));

            // CBN for calling function f
            guid = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[3]));


            // CBN for calling overload function f(i)
            guid = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[4]));


            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("x", 1);
            AssertValue("y", 12);


            // Redefine both functions
            List<Subtree> modified = new List<Subtree>();

            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid_func1, codes[5]));
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid_func2, codes[6]));

            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that the call to the function f has not re-executed
            AssertValue("x", 12);

            // Verify that the call to the overload function f(i) has re-executed
            AssertValue("y", 102);

        }

        [Test]
        [Category("Failure")]
        public void TestFunctionOverloadRedefinitionOnUnmodifiedNode03()
        {
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-5370
            List<string> codes = new List<string>() 
            {
                "def foo(i:int) { return = i;}",
                "def bar : double(x:int) { return = x;}",
                "r = foo(bar(2));",
                "def foo(d:double) { return = d;}"
            };

            Guid guid1 = System.Guid.NewGuid();
            Guid guid2 = System.Guid.NewGuid();
            Guid guid3 = System.Guid.NewGuid();
            Guid guid4 = System.Guid.NewGuid();

            // Create function foo, bar and a statement that uses them
            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[1]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, codes[2]));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("r", 2);


            // Add overload foo
            added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid4, codes[3]));
            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("r", 2.0);

        }


        public void TestFunctionOverloadOnNewNode01()
        {
            List<string> codes = new List<string>() 
            {
                "def f() { return = 5;} x = f();",
                "def f(i : int) { return = i + 1; } y = f(5);"
            };

            Guid guid = System.Guid.NewGuid();
            List<Subtree> added = new List<Subtree>();

            {
                added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[0]));

                var syncData = new GraphSyncData(null, added, null);
                astLiveRunner.UpdateGraph(syncData);

                AssertValue("x", 5);
            }

            // Simualate creating a new CBN for a function definition
            // This should still yield the same result as the codegen will not add the new function.
            // This error will have been handled at the front-end (UI)
            guid = System.Guid.NewGuid();
            {
                added = new List<Subtree>();
                added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[1]));

                var syncData = new GraphSyncData(null, added, null);
                astLiveRunner.UpdateGraph(syncData);

                AssertValue("y", 6);
            }
        }

        [Test]
        public void TestFunctionRedefinitionOfArguments01()
        {
            List<string> codes = new List<string>() 
            {
                "def f() { return = 1;}",
                "x = f();",
                "def f(a : int) { return = 1; }"
            };

            // Create 2 CBNs 
            List<Subtree> added = new List<Subtree>();


            // A CBN with function def f
            Guid guid_func = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid_func, codes[0]));

            // A new CBN that uses function f
            Guid guid = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[1]));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("x", 1);


            // Redefine the CBN
            List<Subtree> modified = new List<Subtree>();

            // Mark the CBN that uses f as modified
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[1]));

            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid_func, codes[2]));

            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that x must have automatically re-executed and that it can no loger find the function
            RuntimeMirror mirror = astLiveRunner.InspectNodeValue("x");
            Assert.IsTrue(mirror.GetData().IsNull);

        }

        [Test]
        public void TestFunctionRedefinitionOfArguments02()
        {
            List<string> codes = new List<string>() 
            {
                "def f(a) { return = a;}",
                "x = f(1);",
                "def f(a, b) { return = a + b; }"
            };

            // Create 2 CBNs 
            List<Subtree> added = new List<Subtree>();


            // A CBN with function def f
            Guid guid_func = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid_func, codes[0]));

            // A new CBN that uses function f
            Guid guid = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[1]));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("x", 1);


            // Redefine the CBN
            List<Subtree> modified = new List<Subtree>();

            // Mark the CBN that uses f as modified
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[1]));

            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid_func, codes[2]));

            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that x must have automatically re-executed and that it can no longer find the function 'f'
            RuntimeMirror mirror = astLiveRunner.InspectNodeValue("x");
            Assert.IsTrue(mirror.GetData().IsNull);

        }

        [Test]
        public void TestRedefineFunctionName01()
        {
            List<string> codes = new List<string>() 
            {
                "def f() { return = 1;}",
                "x = f();",
                "def g() { return = 1;}",
            };

            // Create 2 CBNs 
            List<Subtree> added = new List<Subtree>();


            // A CBN with function def f
            Guid guid_func = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid_func, codes[0]));

            // A new CBN that uses function f
            Guid guid = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[1]));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("x", 1);


            // Redefine the CBN
            List<Subtree> modified = new List<Subtree>();

            // Mark the CBN that uses f as modified
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[1]));

            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid_func, codes[2]));

            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that x must have automatically re-executed and that it can no longer find the function 'f'
            RuntimeMirror mirror = astLiveRunner.InspectNodeValue("x");
            Assert.IsTrue(mirror.GetData().IsNull);

        }

        [Test]
        public void TestRedefineFunctionName02()
        {
            List<string> codes = new List<string>() 
            {
                "def f() { return = 1;} def f(i:int) { return = i;}",
                "x = f();y = f(2);",
                "def g() { return = 1;} def f(i:int) { return = i;}",
            };

            // Create 2 CBNs 
            List<Subtree> added = new List<Subtree>();


            // A CBN with function def f
            Guid guid_func = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid_func, codes[0]));

            // A new CBN that uses function f
            Guid guid = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[1]));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("x", 1);
            AssertValue("y", 2);

            // Redefine the CBN
            List<Subtree> modified = new List<Subtree>();

            // Mark the CBN that uses f as modified
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[1]));

            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid_func, codes[2]));

            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that x must have automatically re-executed and that it can no longer find the function 'f'
            RuntimeMirror mirror = astLiveRunner.InspectNodeValue("x");
            Assert.IsTrue(mirror.GetData().IsNull);

            // Verify that 'y' was not affected and retains its old value
            AssertValue("y", 2);

        }

        [Test]
        public void TestFunctionObjectInApply()
        {
            astLiveRunner = new ProtoScript.Runners.LiveRunner();
            astLiveRunner.ResetVMAndResyncGraph(new List<string> { "FunctionObject.ds" });
            string code = @"
 def foo(x,y ) { return = x + y; }
 f = _SingleFunctionObject(foo, 2, {1}, {null, 42}, true); r = __Apply(f, 3);
 ";

            Guid guid = System.Guid.NewGuid();
            List<Subtree> added = new List<Subtree>();
            {
                added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, code));
                var syncData = new GraphSyncData(null, added, null);
                astLiveRunner.UpdateGraph(syncData);
                AssertValue("r", 45);
            }
        }

        [Test]
        public void TestCodeblockModification01()
        {
            List<string> codes = new List<string>() 
            {
                "g = 0;",
                "def f() { g = g + 1; return = 1;}",
                "x = f(); a = 10;",  // CBN 1
                "x = f(); a = 11;"   // Simulate modifiying CBN 1
            };

            List<Subtree> added = new List<Subtree>();

            // CBN for global
            Guid guid_global = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid_global, codes[0]));

            // A CBN with function def f
            Guid guid_func = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid_func, codes[1]));

            // A new CBN that uses function f
            Guid guid = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[2]));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("g", 1);
            AssertValue("a", 10);


            // Redefine the CBN
            List<Subtree> modified = new List<Subtree>();

            // Mark the CBN that uses f as modified
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[3]));

            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            // Verify that x must have automatically re-executed
            AssertValue("g", 1);    // This should not increment
            AssertValue("a", 11);

        }

        [Test]
        public void TestCodeblockModification02()
        {
            List<string> codes = new List<string>() 
            {
                "a = 1;",        // guid1
                "b = 2;",        // guid2
                "c = 3;",        // guid3
                "d = a + b;",    // guid4
                "d = a + c;",    // guid4
            };

            Guid guid1 = System.Guid.NewGuid();
            Guid guid2 = System.Guid.NewGuid();
            Guid guid3 = System.Guid.NewGuid();
            Guid guid4 = System.Guid.NewGuid();

            List<Subtree> added = new List<Subtree>();

            // Create a, b, c CBNs
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[1]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, codes[2]));

            // Connect a and b to  d = a + b
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid4, codes[3]));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("d", 3);

            // Delete b
            List<Subtree> deleted = new List<Subtree>();
            deleted.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[1]));
            syncData = new GraphSyncData(deleted, null, null);
            astLiveRunner.UpdateGraph(syncData);

            // Connect a and c to d = a + c
            List<Subtree> modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid4, codes[4]));

            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("d", 4);
        }


        [Test]
        public void TestCodeblockModification03()
        {
            List<string> codes = new List<string>() 
            {
                "a = 1;",
                "b = 2;",
                "c = a;",
                "c = b;",
                "d = c + 10;",
            };

            List<Subtree> added = new List<Subtree>();

            Guid guid1 = System.Guid.NewGuid();
            Guid guid2 = System.Guid.NewGuid();
            Guid guid3 = System.Guid.NewGuid();
            Guid guid4 = System.Guid.NewGuid();

            // Create a and b
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[1]));

            // Connect a to c 
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, codes[2]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("c", 1);


            // Connect b to c 
            List<Subtree> modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, codes[3]));

            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("c", 2);

            // Delete first node
            List<Subtree> deleted = new List<Subtree>();
            deleted.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            syncData = new GraphSyncData(deleted, null, null);
            astLiveRunner.UpdateGraph(syncData);


            // Add new node d = c + 10;
            added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid4, codes[4]));
            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("d", 12);

        }

        [Test]
        public void TestCodeblockModification04()
        {
            List<string> codes = new List<string>() 
            {
                "a = 1;",   // g1

                "y = a; x = y;",   // g2

                "c = a; b = c;",   // g3
                
                "y = b; x = y;",   // g2
                
            };

            List<Subtree> added = new List<Subtree>();

            Guid guid1 = System.Guid.NewGuid();
            Guid guid2 = System.Guid.NewGuid();
            Guid guid3 = System.Guid.NewGuid();

            // Create 2 CBNs 
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[1]));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("x", 1);


            // Create new CBN
            added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, codes[2]));

            // Reconnect g2 
            List<Subtree> modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[3]));

            syncData = new GraphSyncData(null, added, modified);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("b", 1);


        }

        [Test]
        public void TestCodeblockModification05()
        {
            List<string> codes = new List<string>() 
            {
                @"import(""FFITarget.dll""); p = DummyPoint.ByCoordinates(0.0, 0.0, 0.0); x = p.X;",
                "p = DummyPoint.ByCoordinates(0.0, 0.0, 0.0); a = p.X;",
                "p = DummyPoint.ByCoordinates(1.0, 0.0, 0.0); a = p.X;"
            };

            List<Subtree> added = new List<Subtree>();

            // Create CBN
            Guid guid = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("x", 0.0);


            // Modify the 2nd statement to a = p.X 
            List<Subtree> modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[1]));
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("a", 0.0);

            // Modify the 1st statement to p = Point.ByCoordinates(1,0,0)
            modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[2]));
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("a", 1.0);
        }

        [Test]
        public void TestPersistentValuesOnUpdate()
        {
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3789

            List<string> codes = new List<string>() 
            {
                @"import(""FFITarget.dll"");", 
                "a = 10; b = 20;", 
                "t = TestPersistentNodeValues(); t = t.Add(a,b); p = t.Sum;",
                "a = 15; b = 25;"
            };

            List<Subtree> added = new List<Subtree>();

            // Create CBN's
            Guid guid = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[0]));
            Guid guid1 = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[1]));
            Guid guid2 = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[2]));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("p", 30);


            // Modify the 4th line to a = 15; b = 25;
            List<Subtree> modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[3]));
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            // Expected value of Sum is 70 (30 + 15 +25)
            AssertValue("p", 70);

        }

        [Test]
        public void TestCodeblockModification06()
        {
            List<string> codes = new List<string>() 
            {
                "a = b = 1;",
                "b = 1; a = b;"
            };

            List<Subtree> added = new List<Subtree>();
            Guid guid = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("a", 1);


            // Modify the CBN
            List<Subtree> modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[1]));
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("a", 1);

        }

        [Test]
        public void TestCodeblockModification07()
        {
            List<string> codes = new List<string>() 
            {
                "a = b = 1;",
                "b = 2; a = b;"
            };

            List<Subtree> added = new List<Subtree>();
            Guid guid = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("a", 1);


            // Modify the CBN
            List<Subtree> modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[1]));
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("a", 2);

        }

        [Test]
        [Category("Failure")]
        public void TestCodeblockModification08()
        {
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4160

            List<string> codes = new List<string>() 
            {
                "a = 1;",
                "x = a; x = x + 1;",
                "a = 2;"
            };

            Guid guid1 = System.Guid.NewGuid();
            Guid guid2 = System.Guid.NewGuid();

            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[1]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("x", 2);


            // Modify the CBN
            List<Subtree> modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[2]));
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("x", 3);
        }

        [Test]
        public void TestCodeblockModification09()
        {
            List<string> codes = new List<string>() 
            {
                @"import(""FFITarget.dll"");",
                "x = 1; p = DummyPoint.ByCoordinates(x, 0.0, 0.0); p = p.Translate(0,5,0);",
                "x = 1; p = DummyPoint.ByCoordinates(x, 0.0, 0.0); p = p.Translate(0,5,0); b = p.Y;"
            };

            List<Subtree> added = new List<Subtree>();

            // Create CBN and run import stmt
            Guid guid = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Add new line
            List<Subtree> modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[1]));
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            // Add new line
            modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[2]));
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("b", 5.0);
        }

        [Test]
        public void TestCodeblockModification10()
        {
            List<string> codes = new List<string>() 
            {
                @"import(""FFITarget.dll"");",
                "x = 1; p = DummyPoint.ByCoordinates(x, 0.0, 0.0);",
                "x = 1; p = DummyPoint.ByCoordinates(x, 0.0, 0.0); p = p.Translate(0,5,0);",
                "x = 1; p = DummyPoint.ByCoordinates(x, 0.0, 0.0); p = p.Translate(0,5,0); b = p.Y;"
            };

            List<Subtree> added = new List<Subtree>();

            // Create CBN and run import stmt
            Guid guid = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Add new line
            List<Subtree> modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[1]));
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            // Add new line
            modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[2]));
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);


            modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[3]));
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("b", 5.0);
        }


        [Test]
        public void TestCodeblockModification11()
        {
            List<string> codes = new List<string>() 
            {
                @"import(""FFITarget.dll"");",
                "x = 0..2; p = DummyPoint.ByCoordinates(x, 0.0, 0.0); p[0] = p[0].Translate(0,5,0);",
                "x = 0..2; p = DummyPoint.ByCoordinates(x, 0.0, 0.0); p[0] = p[0].Translate(0,5,0); b = p[0].Y;"
            };

            List<Subtree> added = new List<Subtree>();

            // Create CBN and run import stmt
            Guid guid = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Add new line
            List<Subtree> modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[1]));
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            // Add new line
            modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[2]));
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("b", 5.0);
        }

        [Test]
        public void TestCodeblockModification12()
        {
            List<string> codes = new List<string>() 
            {
                @"import(""FFITarget.dll"");",
                "x = 0..2; p = DummyPoint.ByCoordinates(x, 0.0, 0.0);",
                "x = 0..2; p = DummyPoint.ByCoordinates(x, 0.0, 0.0); p[0] = p[0].Translate(0,5,0);",
                "x = 0..2; p = DummyPoint.ByCoordinates(x, 0.0, 0.0); p[0] = p[0].Translate(0,5,0); b = p[0].Y;"
            };

            List<Subtree> added = new List<Subtree>();

            // Create CBN and run import stmt
            Guid guid = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Add new line
            List<Subtree> modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[1]));
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            // Add new line
            modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[2]));
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);


            modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[3]));
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("b", 5.0);
        }

        [Test]
        public void TestCodeblockModification13()
        {
            List<string> codes = new List<string>() 
            {
                @"import(""FFITarget.dll"");",
                "a = 1;",
                "a = 10;",
                "p = DummyPoint.ByCoordinates(x, 0.0, 0.0); i = p.X;",
                "x = a; p = DummyPoint.ByCoordinates(x, 0.0, 0.0); i = p.X;",
            };

            List<Subtree> added = new List<Subtree>();

            Guid guid1 = System.Guid.NewGuid();
            Guid guid2 = System.Guid.NewGuid();
            Guid guid3 = System.Guid.NewGuid();

            // Create CBN and run import stmt
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Add new lines
            added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[1]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, codes[3]));
            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Connect Point to 'a'
            List<Subtree> modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, codes[4]));
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            // Modify x
            modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[2]));
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("i", 10.0);
        }


        [Test]
        public void TestCodeblockModification14()
        {
            List<string> codes = new List<string>() 
            {
                @"import(""FFITarget.dll"");",
                "a = 1;",
                "a = 10;",
                "p = DummyPoint.ByCoordinates(x, 0.0, 0.0); i = p.X;",
                "p = DummyPoint.ByCoordinates(x, 0.0, 0.0); i = p.X; x = a;",
            };

            List<Subtree> added = new List<Subtree>();

            Guid guid1 = System.Guid.NewGuid();
            Guid guid2 = System.Guid.NewGuid();
            Guid guid3 = System.Guid.NewGuid();

            // Create CBN and run import stmt
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Add new lines
            added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[1]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, codes[3]));
            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Connect Point to 'a'
            List<Subtree> modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, codes[4]));
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            // Modify x
            modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[2]));
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("i", 10.0);
        }

        [Test]
        public void TestCodeblockModification15()
        {
            List<string> codes = new List<string>() 
            {
                "a = 1;",
                "x = a;",
                "a = 2;"
            };

            Guid guid1 = System.Guid.NewGuid();
            Guid guid2 = System.Guid.NewGuid();

            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[1]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("x", 1);


            // Modify the CBN
            List<Subtree> modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[2]));
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("x", 2);
        }

        [Test]
        public void TestCodeblockModification16()
        {
            List<string> codes = new List<string>() 
            {
                @"import(""FFITarget.dll"");",
                "y = 10.0;",
                "p = DummyPoint.ByCoordinates(0.0, 0.0, 0.0);",
                "i = p.Y;",
                "p = DummyPoint.ByCoordinates(0.0, y, 0.0);"
            };

            Guid guid1 = System.Guid.NewGuid();
            Guid guid2 = System.Guid.NewGuid();
            Guid guid3 = System.Guid.NewGuid();
            Guid guid4 = System.Guid.NewGuid();

            List<Subtree> added = new List<Subtree>();

            // Create CBN and run import stmt
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);


            // Create a CBN with a point
            added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[2]));

            // Create a CBN that checks the value of p.Y
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, codes[3]));

            // Execute
            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("i", 0.0);


            // Create a CBN defining 'y'
            added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid4, codes[1]));
            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("y", 10.0);

            // Connect CBN to point
            List<Subtree> modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[4]));
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("i", 10.0);

            // Disconnect point
            modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[2]));
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("i", 0.0);

            // Delete CBN
            List<Subtree> deleted = new List<Subtree>();
            deleted.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid4, codes[1]));
            syncData = new GraphSyncData(deleted, null, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("i", 0.0);
        }

        [Test]
        [Category("Failure")]
        public void TestCodeBlockDeleteLine01()
        {
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4159
            List<string> codes = new List<string>() 
            {                    
                "a = 2;",
                "def f(x){return = x;} b = a; p = f(b);",
                "def f(x){return = x;}"
            };

            Guid guid1 = System.Guid.NewGuid();
            Guid guid2 = System.Guid.NewGuid();
            List<Subtree> added = new List<Subtree>();

            // Create CBNs
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[1]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("p", 2);

            // Modify CBN2 - Remove the line that calls the function
            List<Subtree> modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[2]));
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            var mirror = astLiveRunner.InspectNodeValue("p");

            Assert.IsTrue(mirror.GetData().IsNull);
        }

        [Test]
        public void TestEmptyCodeblock01()
        {
            List<string> codes = new List<string>() 
            {
                "a = b = 1;",
                "b = 1; a = b;"
            };

            // Simulate an empty codeblock
            List<Subtree> added = new List<Subtree>();
            Guid guid = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, ""));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Modify the CBN
            List<Subtree> modified = new List<Subtree>();
            modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[0]));
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("a", 1);

            // Modify the CBN
            modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[1]));
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("a", 1);

        }

        [Test]
        public void TestEmptyCodeblock02()
        {
            List<string> codes = new List<string>() 
            {
                "a = b = 1;",
                "b = 2; a = b;"
            };

            // Simulate an empty codeblock
            List<Subtree> added = new List<Subtree>();
            Guid guid = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, ""));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Modify the CBN
            List<Subtree> modified = new List<Subtree>();
            modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[0]));
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("a", 1);

            // Modify the CBN
            modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[1]));
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("a", 2);

        }

        [Test]
        public void TestDeleteNode02()
        {
            List<string> codes = new List<string>() 
            {
                "a = 1;",
                "b = 2;",
                "c = a;",
                "c = b;",
            };

            List<Subtree> added = new List<Subtree>();

            Guid guid1 = System.Guid.NewGuid();
            Guid guid2 = System.Guid.NewGuid();
            Guid guid3 = System.Guid.NewGuid();

            // Create a and b
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[1]));

            // Connect a to c 
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, codes[2]));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("c", 1);

            // Connect b to c 
            List<Subtree> modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, codes[3]));

            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("c", 2);

            // Delete first node
            List<Subtree> deleted = new List<Subtree>();

            // Mark the CBN that uses f as modified
            deleted.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));

            syncData = new GraphSyncData(deleted, null, null);
            astLiveRunner.UpdateGraph(syncData);

            // c should not have changed
            AssertValue("c", 2);

        }

        [Test]
        public void TestDeleteNode03()
        {
            List<string> codes = new List<string>() 
            {
                "p = DummyPoint.ByCoordinates(0,0,0);"
            };

            List<Subtree> added = new List<Subtree>();

            // Create a node
            Guid guid = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[0]));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);


            // Delete the node
            List<Subtree> deleted = new List<Subtree>();
            deleted.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[0]));
            syncData = new GraphSyncData(deleted, null, null);
            astLiveRunner.UpdateGraph(syncData);

            var mirror = astLiveRunner.InspectNodeValue("p");
            Assert.IsTrue(mirror.GetData().IsNull);

        }

        [Test]
        public void TestCachingSSA01()
        {
            List<string> codes = new List<string>() 
            {
                "a = 1;",
                "a = a + 1;"
            };

            Guid guid1 = System.Guid.NewGuid();
            Guid guid2 = System.Guid.NewGuid();

            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[1]));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("a", 2);

        }

        [Test]
        [Category("Failure")]
        public void TestCachingSSA02()
        {
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4158
            // Executing function SSA statement that was not modified


            List<string> codes = new List<string>() 
            {
                "global = 0; def f(i:int) { return = i; } def g(j:int) { global = global + 1; return = j; }",
                "x = 1;",               // Simulate input to function in this CBN
                "z = 10;",              // Simulate another input to the function
                "a = f(x) + g(2);",     // CBN with function call 
                "a = f(z) + g(2);"      // Same CBN with function call where the input to function 'f' was changed
            };

            Guid guid1 = System.Guid.NewGuid();
            Guid guid2 = System.Guid.NewGuid();
            Guid guid3 = System.Guid.NewGuid();
            Guid guid4 = System.Guid.NewGuid();
            Guid guid5 = System.Guid.NewGuid();

            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[1]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, codes[2]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid4, codes[3]));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("a", 3);
            AssertValue("global", 1);


            // Modify the function call CBN so it connects to the input 'z'
            List<Subtree> modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid5, codes[4]));

            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("a", 12);

            // Verify that function 'g' was not re-executed
            AssertValue("global", 1);
        }

        [Test]
        public void TestExecution()
        {
            List<string> codes = new List<string>() 
            {
                "class JPoint{ X:int; Y:int; Z:int; constructor ByCoord(a:int,b:int,c:int){X = a; Y = b; Z = c;}}",
                "a=10;b=20;c=30;",
                "p = JPoint.ByCoord(a,b,c);",
                "x = p.X; "
            };

            Guid guid1 = System.Guid.NewGuid();
            Guid guid2 = System.Guid.NewGuid();
            Guid guid3 = System.Guid.NewGuid();
            Guid guid4 = System.Guid.NewGuid();

            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[1]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, codes[2]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid4, codes[3]));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("x", 10);
        }



        [Test]
        public void RegressMAGN747_01()
        {
            List<string> codes = new List<string>() 
            {
                "a = b = 42;",
                "b = a = 24;"
            };

            Guid guid = System.Guid.NewGuid();
            List<Subtree> added = new List<Subtree>();
            {
                added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[0]));

                var syncData = new GraphSyncData(null, added, null);
                astLiveRunner.UpdateGraph(syncData);

                AssertValue("a", 42);
                AssertValue("b", 42);
            }

            List<Subtree> modified = new List<Subtree>();
            {
                modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[1]));

                var syncData = new GraphSyncData(null, null, modified);
                astLiveRunner.UpdateGraph(syncData);

                AssertValue("a", 24);
                AssertValue("b", 24);
            }

            modified = new List<Subtree>();
            {
                modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[0]));

                var syncData = new GraphSyncData(null, null, modified);
                astLiveRunner.UpdateGraph(syncData);

                AssertValue("a", 42);
                AssertValue("b", 42);
            }

            modified = new List<Subtree>();
            {
                modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[1]));

                var syncData = new GraphSyncData(null, null, modified);
                astLiveRunner.UpdateGraph(syncData);

                AssertValue("a", 24);
                AssertValue("b", 24);
            }
        }

        [Test]
        public void RegressMAGN747_02()
        {
            List<string> codes = new List<string>() 
            {
                "a1 = b1 = 42;",
                "b2 = a2 = 24;",
                "a2 = b2 = 42;"
            };

            // Add CBN1 a1 = b1 = 42;
            Guid guid1 = System.Guid.NewGuid();
            List<Subtree> added = new List<Subtree>();
            {
                added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));

                var syncData = new GraphSyncData(null, added, null);
                astLiveRunner.UpdateGraph(syncData);

                AssertValue("a1", 42);
                AssertValue("b1", 42);
            }


            // Add CBN2 a2 = b2 = 24;
            Guid guid2 = System.Guid.NewGuid();
            added = new List<Subtree>();
            {
                added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[1]));

                var syncData = new GraphSyncData(null, added, null);
                astLiveRunner.UpdateGraph(syncData);

                AssertValue("a2", 24);
                AssertValue("b2", 24);
            }

            // Modify CBN2 to a2 = b2 = 42;
            List<Subtree> modified = new List<Subtree>();
            {
                modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[2]));

                var syncData = new GraphSyncData(null, null, modified);
                astLiveRunner.UpdateGraph(syncData);

                AssertValue("a2", 42);
                AssertValue("b2", 42);
            }

            // Modify CBN2 a2 = b2 = 24;
            modified = new List<Subtree>();
            {
                modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[1]));

                var syncData = new GraphSyncData(null, null, modified);
                astLiveRunner.UpdateGraph(syncData);

                AssertValue("a2", 24);
                AssertValue("b2", 24);
            }
        }

        [Test]
        public void TestDoubleAssignment01()
        {
            List<string> codes = new List<string>() 
            {
                "a = b = 1;",
                "b = a = 2;"
            };

            // Add CBN1 a1 = b1 = 42;
            Guid guid = System.Guid.NewGuid();
            List<Subtree> added = new List<Subtree>();
            {
                added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[0]));

                var syncData = new GraphSyncData(null, added, null);
                astLiveRunner.UpdateGraph(syncData);

                AssertValue("a", 1);
                AssertValue("b", 1);
            }



            // Modify CBN2 to a2 = b2 = 42;
            List<Subtree> modified = new List<Subtree>();
            {
                modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[1]));

                var syncData = new GraphSyncData(null, null, modified);
                astLiveRunner.UpdateGraph(syncData);

                AssertValue("a", 2);
                AssertValue("b", 2);
            }
        }

        [Test]
        public void TestPythonCodeExecution()
        {
            List<string> codes = new List<string>() 
            {
                @"import(""DSIronPython.dll"");",
                @"x = IronPythonEvaluator.EvaluateIronPythonScript(""# Default imports

#The inputs to this node will be stored as a list in the IN variable.
dataEnteringNode = IN

#Assign your output to the OUT variable
OUT = 1"", {""IN""}, {{}}); x = x;",
                            @"x = IronPythonEvaluator.EvaluateIronPythonScript(""# Default imports

#The inputs to this node will be stored as a list in the IN variable.
dataEnteringNode = IN

#Assign your output to the OUT variable
OUT = 100"", {""IN""}, {{}}); x = x;"
            };

            Guid guid1 = System.Guid.NewGuid();
            Guid guid2 = System.Guid.NewGuid();

            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[1]));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("x", 1);


            List<Subtree> modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[2]));

            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("x", 100);
        }

        [Test]
        public void TestEmptyArray()
        {
            List<string> codes = new List<string>() 
            {
                @"def foo(i:int, j : var[]..[]) { return = i; }",
                @"x = 1; p = foo(x, {{}});",
                @"x = 10;"
            };

            Guid guid1 = System.Guid.NewGuid();
            Guid guid2 = System.Guid.NewGuid();

            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[1]));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);



            List<Subtree> modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[2]));

            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);
        }

        [Test]
        public void TestNodeMapping()
        {
            List<string> codes = new List<string>()
            {
                @"def foo(x) { return = x + 42; }",
                @"x = 1; y = foo(x);",
            };

            Guid guid1 = System.Guid.NewGuid();
            Guid guid2 = System.Guid.NewGuid();

            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[1]));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Graph UI node -> ASTs
            var astNodes = astLiveRunner.Core.DSExecutable.RuntimeData.CachedSSANodes;
            bool foundCallsite = false;
            Guid callsiteId = Guid.Empty;

            // AST -> CallSite
            foreach (var ast in astNodes)
            {
                ProtoCore.CallSite callsite;
                if (astLiveRunner.Core.DSExecutable.RuntimeData.ASTToCallSiteMap.TryGetValue(ast.ID, out callsite))
                {
                    callsiteId = callsite.CallSiteID;
                    foundCallsite = true;
                    break;
                }
            }


            // CallSite -> Graph UI node
            Assert.IsTrue(foundCallsite);
            Assert.AreEqual(guid2, astLiveRunner.Core.DSExecutable.RuntimeData.CallSiteToNodeMap[callsiteId]);
        }

        [Test]
        public void TestReExecute01()
        {
            List<string> codes = new List<string>() 
            {
                @"import(""FFITarget.dll"");TestUpdateCount.Reset();", 
                "p = TestUpdateCount.Ctor(10,20);",
                "a = p.UpdateCount;"
            };

            List<Subtree> added = new List<Subtree>();

            // Create CBN1 for import
            Guid guid1 = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Create CBN2 to use TestCount
            Guid guid2 = System.Guid.NewGuid();
            added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[1]));


            // Create CBN3 to check value of TestCount
            Guid guid3 = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, codes[2]));
            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("a", 1);


            // Modify CBN2 with same contents with ForceExecution flag set
            List<Subtree> modified = new List<Subtree>();
            Subtree subtree = ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[1]);
            subtree.ForceExecution = true;
            modified.Add(subtree);
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("a", 2);
        }

        [Test]
        public void TestReExecute02()
        {
            List<string> codes = new List<string>() 
            {
                @"a = 1;", 
                @"a = 2;"
            };

            List<Subtree> added = new List<Subtree>();

            Guid guid = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("a", 1);

            // Modify
            List<Subtree> modified = new List<Subtree>();
            Subtree subtree = ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[1]);
            modified.Add(subtree);
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("a", 2);
        }

        [Test]
        public void TestReExecuteOnModifiedNode01()
        {
            List<string> codes = new List<string>() 
            {
                @"import(""FFITarget.dll""); TestUpdateCount.Reset();", 
                "p = TestUpdateCount.Ctor(10,20);",
                "a = p.UpdateCount + p.Val;",
                "p = TestUpdateCount.Ctor(10,30);"
            };

            List<Subtree> added = new List<Subtree>();

            // Create CBN1 for import
            Guid guid1 = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Create CBN2 to use TestCount
            Guid guid2 = System.Guid.NewGuid();
            added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[1]));


            // Create CBN3 to check value of TestCount
            Guid guid3 = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, codes[2]));
            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("a", 31);


            // Modify CBN2 with new contents with ForceExecution flag set
            // This incremenets the count from the FFI lib. 
            List<Subtree> modified = new List<Subtree>();
            Subtree subtree = ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[3]);
            subtree.ForceExecution = true;
            modified.Add(subtree);
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("a", 42);
        }

        [Test]
        public void ReproMAGN3551()
        {
            List<string> codes = new List<string>() 
            {
                @"import(""FFITarget.dll"");", 
                "a = DummyPoint.ByCoordinates(0,1,2);",
                "b = DummyPoint.ByCoordinates(1,2,3);",
                "a_in = a; b_in = b; c = DummyLine.ByStartPointEndPoint(a,b);"
            };

            List<Subtree> added = new List<Subtree>();

            // Create CBN1 for import
            Guid guid1 = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Create CBN2 to create a = 
            Guid guid2 = System.Guid.NewGuid();
            added = new List<Subtree>();

            Subtree cbn2 = ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[1]);
            added.Add(cbn2);

            // Create CBN3 to create b = 
            Guid guid3 = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, codes[2]));

            // Create CBN4 to create c = 
            Guid guid4 = System.Guid.NewGuid();
            Subtree cbn4 = ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid4, codes[3]);
            added.Add(cbn4);

            //Run
            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            var mirror = astLiveRunner.InspectNodeValue("a");
            MirrorData data = mirror.GetData();

            Assert.IsTrue(data.Data.GetType() == typeof(FFITarget.DummyPoint));

            //Delete a =
            syncData = new GraphSyncData(new List<Subtree>() { cbn2 }, null, null);
            //Run            
            astLiveRunner.UpdateGraph(syncData);


            //Create a = 
            syncData = new GraphSyncData(null, new List<Subtree>() { cbn2 }, null);

            //Run
            astLiveRunner.UpdateGraph(syncData);

            //Delete a_in = ...
            syncData = new GraphSyncData(new List<Subtree>() { cbn4 }, null, null);

            //Run
            astLiveRunner.UpdateGraph(syncData);

            //The output of A should still be a point, but it isn't it's now the stack pointer
            mirror = astLiveRunner.InspectNodeValue("a");
            data = mirror.GetData();
            Assert.IsTrue(data.Data.GetType() == typeof(FFITarget.DummyPoint));


        }

        [Test]
        public void ReproMAGN3576()
        {
            List<string> codes = new List<string>() 
            {
                @"import(""FFITarget.dll"");", 
                "t_0_a = DummyPoint.ByCoordinates(0,1,8);",
                "t_0_b = DummyPoint.ByCoordinates(0,1,2);",
                "a_6 = t_0_b; b_6 = t_0_a; t_0_7 = DummyLine.ByStartPointEndPoint(a_6,b_6);"
            };

            List<Subtree> added = new List<Subtree>();

            // Create CBN1 for import
            Guid guid1 = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Create CBN2 to create a = 
            Guid guid2 = System.Guid.NewGuid();
            added = new List<Subtree>();

            Subtree cbn2 = ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[1]);
            added.Add(cbn2);

            // Create CBN3 to create b = 
            Guid guid3 = System.Guid.NewGuid();
            Subtree cbn3 = ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, codes[2]);
            added.Add(cbn3);

            // Create CBN4 to create c = 
            Guid guid4 = System.Guid.NewGuid();
            Subtree cbn4 = ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid4, codes[3]);
            added.Add(cbn4);

            //Run
            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            var mirror = astLiveRunner.InspectNodeValue("t_0_a");
            MirrorData data = mirror.GetData();

            Assert.IsTrue(data.Data.GetType() == typeof(FFITarget.DummyPoint));


            //Delete CBN4
            syncData = new GraphSyncData(new List<Subtree>() { cbn4 }, null, null);
            //Run            
            astLiveRunner.UpdateGraph(syncData);


            //Create a = 
            syncData = new GraphSyncData(null, new List<Subtree>() { cbn4 }, null);

            //Run
            astLiveRunner.UpdateGraph(syncData);



            //Delete CBN3
            //Modify CBN4 to set in arg from CBN 3 to be null
            Subtree newCBN4 = ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid4, "a_6 = null; b_6 = t_0_a; t_0_7 = DummyLine.ByStartPointEndPoint(a_6,b_6);");
            syncData = new GraphSyncData(new List<Subtree>() { cbn3 }, null, new List<Subtree> { newCBN4 });

            //Run
            astLiveRunner.UpdateGraph(syncData);



            //Recreate CBN3
            //Set CBN4 back to what it was
            syncData = new GraphSyncData(null, new List<Subtree> { cbn3 }, new List<Subtree> { cbn4 });
            astLiveRunner.UpdateGraph(syncData);


            //The output of A should still be a point, but it isn't it's now the stack pointer
            mirror = astLiveRunner.InspectNodeValue("t_0_a");
            data = mirror.GetData();
            Assert.IsTrue(data.Data.GetType() == typeof(FFITarget.DummyPoint));

            mirror = astLiveRunner.InspectNodeValue("t_0_b");
            data = mirror.GetData();
            Assert.IsTrue(data.Data.GetType() == typeof(FFITarget.DummyPoint));


            mirror = astLiveRunner.InspectNodeValue("t_0_7");
            data = mirror.GetData();
            Assert.IsTrue(data.Data.GetType() == typeof(FFITarget.DummyLine));
        }

        [Test]
        public void ReproMAGN3576Ext()
        {
            List<string> codes = new List<string>() 
            {
                @"import(""FFITarget.dll"");", 
                "t_0_a = DummyPoint.ByCoordinates(0,1,8);",
                "t_0_e = DummyPoint.ByCoordinates(0,1,2);",
                "a_6 = t_0_e; b_6 = t_0_a; t_0_6 = DummyLine.ByStartPointEndPoint(a_6,b_6);"
            };

            List<Subtree> added = new List<Subtree>();

            // Create CBN1 for import
            Guid guid1 = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Create CBN2 to create a = 
            Guid guid2 = System.Guid.NewGuid();
            added = new List<Subtree>();

            Subtree cbn2 = ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[1]);
            added.Add(cbn2);

            // Create CBN3 to create b = 
            Guid guid3 = System.Guid.NewGuid();
            Subtree cbn3 = ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, codes[2]);
            added.Add(cbn3);

            // Create CBN4 to create c = 
            Guid guid4 = System.Guid.NewGuid();
            Subtree cbn4 = ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid4, codes[3]);
            added.Add(cbn4);

            //Run
            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            var mirror = astLiveRunner.InspectNodeValue("t_0_a");
            MirrorData data = mirror.GetData();

            Assert.IsTrue(data.Data.GetType() == typeof(FFITarget.DummyPoint));


            //Delete CBN4
            syncData = new GraphSyncData(new List<Subtree>() { cbn4 }, null, null);
            //Run            
            astLiveRunner.UpdateGraph(syncData);


            //Create a = 
            syncData = new GraphSyncData(null, null, new List<Subtree>() { cbn4 });

            //Run
            astLiveRunner.UpdateGraph(syncData);



            //Delete CBN3
            //Modify CBN4 to set in arg from CBN 3 to be null
            Subtree newCBN4 = ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid4, "a_6 = null; b_6 = t_0_a; t_0_6 = DummyLine.ByStartPointEndPoint(a_6,b_6);");
            syncData = new GraphSyncData(new List<Subtree>() { cbn3 }, null, new List<Subtree> { newCBN4 });

            //Run
            astLiveRunner.UpdateGraph(syncData);




            //Recreate CBN3
            //Set CBN4 back to what it was
            syncData = new GraphSyncData(null, null, new List<Subtree> { cbn3, cbn4 });
            astLiveRunner.UpdateGraph(syncData);


            //Delete CBN4
            syncData = new GraphSyncData(new List<Subtree>() { cbn4 }, null, null);
            //Run            
            astLiveRunner.UpdateGraph(syncData);


            //Create a = 
            syncData = new GraphSyncData(null, null, new List<Subtree>() { cbn4 });

            //Run
            astLiveRunner.UpdateGraph(syncData);



            //The output of A should still be a point, but it isn't it's now the stack pointer
            mirror = astLiveRunner.InspectNodeValue("t_0_a");
            data = mirror.GetData();
            Console.WriteLine(data);
            Assert.IsTrue(data.Data.GetType() == typeof(FFITarget.DummyPoint));

            mirror = astLiveRunner.InspectNodeValue("t_0_e");
            data = mirror.GetData();
            Console.WriteLine(data);

            Assert.IsTrue(data.Data.GetType() == typeof(FFITarget.DummyPoint));


            mirror = astLiveRunner.InspectNodeValue("t_0_6");
            data = mirror.GetData();
            Console.WriteLine(data);

            Assert.IsTrue(data.Data.GetType() == typeof(FFITarget.DummyLine));
        }

        [Test]
        public void ReproMAGN3551Ext()
        {
            List<string> codes = new List<string>() 
            {
                @"import(""FFITarget.dll"");", 
                "a = DummyPoint.ByCoordinates(0,1,2);",
                "b = DummyPoint.ByCoordinates(1,2,3);",
                "a_in = a; b_in = b; c = DummyLine.ByStartPointEndPoint(a,b);"
            };

            List<Subtree> added = new List<Subtree>();

            // Create CBN1 for import
            Guid guid1 = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Create CBN2 to create a = 
            Guid guid2 = System.Guid.NewGuid();
            added = new List<Subtree>();

            Subtree cbn2 = ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[1]);
            added.Add(cbn2);

            // Create CBN3 to create b = 
            Guid guid3 = System.Guid.NewGuid();
            Subtree cbn3 = ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, codes[2]);
            added.Add(cbn3);

            // Create CBN4 to create c = 
            Guid guid4 = System.Guid.NewGuid();
            Subtree cbn4 = ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid4, codes[3]);
            added.Add(cbn4);

            //Run
            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            var mirror = astLiveRunner.InspectNodeValue("a");
            MirrorData data = mirror.GetData();

            astLiveRunner.InspectNodeValue("a").GetData();
            astLiveRunner.InspectNodeValue("b").GetData();
            astLiveRunner.InspectNodeValue("c").GetData();


            Assert.IsTrue(data.Data.GetType() == typeof(FFITarget.DummyPoint));

            //Delete c =
            syncData = new GraphSyncData(new List<Subtree>() { cbn4 }, null, null);
            //Run            
            astLiveRunner.UpdateGraph(syncData);

            astLiveRunner.InspectNodeValue("a").GetData();
            astLiveRunner.InspectNodeValue("b").GetData();


            //Create c = 
            syncData = new GraphSyncData(null, null, new List<Subtree>() { cbn4 });

            //Run
            astLiveRunner.UpdateGraph(syncData);

            astLiveRunner.InspectNodeValue("a").GetData();
            astLiveRunner.InspectNodeValue("b").GetData();
            astLiveRunner.InspectNodeValue("c").GetData();

            //Delete b_in = ...
            Subtree cbn4New = ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid4,
                                                    "a_in = a; b_in = null; c = DummyLine.ByStartPointEndPoint(a,b);");
            syncData = new GraphSyncData(new List<Subtree>() { cbn3 }, null, new List<Subtree>() { cbn4New });

            //Run
            astLiveRunner.UpdateGraph(syncData);

            astLiveRunner.InspectNodeValue("a").GetData();
            astLiveRunner.InspectNodeValue("c").GetData();

            //Reset CBN 3 and 4
            syncData = new GraphSyncData(null, null, new List<Subtree>() { cbn3, cbn4 });

            //Run
            astLiveRunner.UpdateGraph(syncData);

            astLiveRunner.InspectNodeValue("a").GetData();
            astLiveRunner.InspectNodeValue("b").GetData();
            astLiveRunner.InspectNodeValue("c").GetData();

            //Delete c =
            syncData = new GraphSyncData(new List<Subtree>() { cbn4 }, null, null);
            //Run            
            astLiveRunner.UpdateGraph(syncData);

            astLiveRunner.InspectNodeValue("a").GetData();
            astLiveRunner.InspectNodeValue("b").GetData();

            //Create c = 
            syncData = new GraphSyncData(null, null, new List<Subtree>() { cbn4 });

            //Run
            astLiveRunner.UpdateGraph(syncData);

            astLiveRunner.InspectNodeValue("a").GetData();
            astLiveRunner.InspectNodeValue("b").GetData();
            astLiveRunner.InspectNodeValue("c").GetData();

            //The output of A should still be a point, but it isn't it's now the stack pointer
            mirror = astLiveRunner.InspectNodeValue("a");
            data = mirror.GetData();
            Assert.IsTrue(data.Data.GetType() == typeof(FFITarget.DummyPoint));

            mirror = astLiveRunner.InspectNodeValue("c");
            data = mirror.GetData();
            Assert.IsTrue(data.Data.GetType() == typeof(FFITarget.DummyLine));

        }


        [Test]
        public void ReproMAGNXXX()
        {
            List<string> codes = new List<string>()
                {
                    @"import(""FFITarget.dll""); 
                    import(""FunctionObject.ds"");",
                    "t0 = 0;",
                    "v0 = FFITarget.DummyPoint.ByCoordinates(t0, t0, t0);"
                };

            List<Subtree> added = new List<Subtree>();

            // Create CBN0 for import
            Guid guid0 = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid0, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            //astLiveRunner.UpdateGraph(syncData);

            // Create cbnNum to create t0 = 0
            Guid guid1 = System.Guid.NewGuid();
            //added = new List<Subtree>();

            Subtree cbnNum = ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[1]);
            added.Add(cbnNum);

            // Create CBN2 to create v0 = 
            Guid guid2 = System.Guid.NewGuid();
            Subtree cbnPt = ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[2]);
            Subtree cbnDel = ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2,
                "v0 = _SingleFunctionObject(FFITarget.DummyPoint.ByCoordinates, 3, {}, {null, null, null}, true);");

            added.Add(cbnPt);

            //Run
            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            Assert.IsTrue((long)astLiveRunner.InspectNodeValue("t0").GetData().Data == 0);
            Assert.IsTrue(astLiveRunner.InspectNodeValue("v0").GetData().Data.GetType() == typeof(FFITarget.DummyPoint));


            //Del and reset the cbnPt
            syncData = new GraphSyncData(new List<Subtree>() { cbnPt }, null, null);
            astLiveRunner.UpdateGraph(syncData);

            syncData = new GraphSyncData(null, null, new List<Subtree>() { cbnPt });
            astLiveRunner.UpdateGraph(syncData);


            //Del and reset the cbnNum
            syncData = new GraphSyncData(new List<Subtree>() { cbnNum }, null, new List<Subtree>() { cbnDel });
            astLiveRunner.UpdateGraph(syncData);

            syncData = new GraphSyncData(null, null, new List<Subtree>() { cbnNum, cbnPt });
            astLiveRunner.UpdateGraph(syncData);


            //Del and reset the cbnPt
            syncData = new GraphSyncData(new List<Subtree>() { cbnPt }, null, null);
            astLiveRunner.UpdateGraph(syncData);

            syncData = new GraphSyncData(null, null, new List<Subtree>() { cbnPt });
            astLiveRunner.UpdateGraph(syncData);


            //Del and reset the cbnPt
            syncData = new GraphSyncData(new List<Subtree>() { cbnPt }, null, null);
            astLiveRunner.UpdateGraph(syncData);

            syncData = new GraphSyncData(null, null, new List<Subtree>() { cbnPt });
            astLiveRunner.UpdateGraph(syncData);



            //Del and reset the cbnPt
            syncData = new GraphSyncData(new List<Subtree>() { cbnPt }, null, null);
            astLiveRunner.UpdateGraph(syncData);

            syncData = new GraphSyncData(null, null, new List<Subtree>() { cbnPt });
            astLiveRunner.UpdateGraph(syncData);




            //Del and reset the cbnNum
            syncData = new GraphSyncData(new List<Subtree>() { cbnNum }, null, new List<Subtree>() { cbnDel });
            astLiveRunner.UpdateGraph(syncData);

            syncData = new GraphSyncData(null, null, new List<Subtree>() { cbnNum, cbnPt });
            astLiveRunner.UpdateGraph(syncData);




            //Del and reset the cbnPt
            syncData = new GraphSyncData(new List<Subtree>() { cbnPt }, null, null);
            astLiveRunner.UpdateGraph(syncData);

            syncData = new GraphSyncData(null, null, new List<Subtree>() { cbnPt });
            astLiveRunner.UpdateGraph(syncData);




            //Del and reset the cbnPt
            syncData = new GraphSyncData(new List<Subtree>() { cbnPt }, null, null);
            astLiveRunner.UpdateGraph(syncData);

            syncData = new GraphSyncData(null, null, new List<Subtree>() { cbnPt });
            astLiveRunner.UpdateGraph(syncData);

            Assert.IsTrue((long)astLiveRunner.InspectNodeValue("t0").GetData().Data == 0);
            Assert.IsTrue(astLiveRunner.InspectNodeValue("v0").GetData().Data.GetType() == typeof(FFITarget.DummyPoint));


        }

        [Test]
        public void ReproMAG3600()
        {
            List<string> codes = new List<string>()
                {
                    @"import(""FFITarget.dll""); 
                    import(""FunctionObject.ds"");",
                    "t0 = 0;",
                    "v0 = FFITarget.DummyPoint.ByCoordinates(t0, t0, t0);"
                };

            List<Subtree> added = new List<Subtree>();

            // Create CBN0 for import
            Guid guid0 = System.Guid.NewGuid();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid0, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            //astLiveRunner.UpdateGraph(syncData);

            // Create cbnNum to create t0 = 0
            Guid guid1 = System.Guid.NewGuid();
            //added = new List<Subtree>();

            Subtree cbnNum = ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[1]);
            added.Add(cbnNum);

            // Create CBN2 to create v0 = 
            Guid guid2 = System.Guid.NewGuid();
            Subtree cbnPt = ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[2]);
            Subtree cbnDel = ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2,
                "v0 = _SingleFunctionObject(FFITarget.DummyPoint.ByCoordinates, 3, {}, {null, null, null}, true);");

            added.Add(cbnPt);

            //Run
            syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            Assert.IsTrue((long)astLiveRunner.InspectNodeValue("t0").GetData().Data == 0);
            Assert.IsTrue(astLiveRunner.InspectNodeValue("v0").GetData().Data.GetType() == typeof(FFITarget.DummyPoint));


            Random rand = new Random(876);

            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine(i);

                if (rand.NextDouble() < 0.5)
                {
                    Console.WriteLine("CBN Pt");

                    //Del and reset the cbnPt
                    syncData = new GraphSyncData(new List<Subtree>() { cbnPt }, null, null);
                    astLiveRunner.UpdateGraph(syncData);

                    syncData = new GraphSyncData(null, null, new List<Subtree>() { cbnPt });
                    astLiveRunner.UpdateGraph(syncData);
                }
                else
                {
                    Console.WriteLine("CBN Num");


                    //Del and reset the cbnNum
                    syncData = new GraphSyncData(new List<Subtree>() { cbnNum }, null, new List<Subtree>() { cbnDel });
                    astLiveRunner.UpdateGraph(syncData);

                    syncData = new GraphSyncData(null, null, new List<Subtree>() { cbnNum, cbnPt });
                    astLiveRunner.UpdateGraph(syncData);

                }



                Assert.IsTrue(astLiveRunner.InspectNodeValue("t0").GetData().Data != null);
                Assert.IsTrue((long)astLiveRunner.InspectNodeValue("t0").GetData().Data == 0);
                Assert.IsTrue(astLiveRunner.InspectNodeValue("v0").GetData().Data.GetType() ==
                              typeof(FFITarget.DummyPoint));

            }

        }

        [Test]
        public void TestImperativeExecution01()
        {
            List<string> codes = new List<string>() 
            {
                "i = [Imperative] {a = 1;b = a;a = 10;return = b;}"
            };
            Guid guid = System.Guid.NewGuid();

            // add two nodes
            IEnumerable<int> index = Enumerable.Range(0, codes.Count);
            var added = index.Select(idx => ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[idx])).ToList();

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            ProtoCore.Mirror.RuntimeMirror mirror = astLiveRunner.InspectNodeValue("i");
            StackValue value = mirror.GetData().GetStackValue();
            Assert.AreEqual(value.opdata, 1);
        }

        [Test]
        public void TestImperativeExecution02()
        {
            List<string> codes = new List<string>() 
            {
               @"
a = [Imperative]
{
    x = 2;
    return = x;
}
"
            };

            Guid guid1 = System.Guid.NewGuid();

            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("a", 2);
        }

        [Test]
        public void TestImperativeExecution03()
        {
            List<string> codes = new List<string>() 
            {
               @"
a = [Imperative]
{
    x = 2;
    if (x > 2)
    {
        x = 10;
    }
    return = x;
}
"
            };

            Guid guid1 = System.Guid.NewGuid();

            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("a", 2);
        }

        [Test]
        public void TestImperativeExecution04()
        {
            List<string> codes = new List<string>() 
            {
               @"
a = [Imperative]
{
    x = 2;
    if (x > 2)
    {
        x = 10;
    }
    else
    {
        x = 11;
    }
    return = x;
}
"
            };

            Guid guid1 = System.Guid.NewGuid();

            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("a", 11);
        }

        [Test]
        public void TestImperativeExecution05()
        {
            List<string> codes = new List<string>() 
            {
               @"
a = [Imperative]
{
    x = 2;
    if (x > 1)
    {
        x = 10;
    }
    return = x;
}
"
            };

            Guid guid1 = System.Guid.NewGuid();

            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("a", 10);
        }

        [Test]
        public void TestImperativeExecution06()
        {
            List<string> codes = new List<string>() 
            {
               @"
a = [Imperative]
{
    x = 0;
    n = 1..5;
    for (i in n)
    {
        x = x + i;
    }
    return = x;
}
"
            };

            Guid guid1 = System.Guid.NewGuid();

            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("a", 15);
        }

        [Test]
        public void TestImperativeExecution07()
        {
            List<string> codes = new List<string>() 
            {
               @"
a = [Imperative]
{
    x = 0;
    n = 1..5;
    for (i in n)
    {
        x = x + i;
        break;
    }
    return = x;
}
"
            };

            Guid guid1 = System.Guid.NewGuid();

            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("a", 1);
        }

        [Test]
        public void TestImperativeExecution08()
        {
            List<string> codes = new List<string>() 
            {
               @"
x = 10;
a = [Imperative]
{
    return = x + 1;
}
"
            };

            Guid guid1 = System.Guid.NewGuid();

            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("a", 11);
        }


        [Test]
        public void TestImperativeDeltaExecution01()
        {
            List<string> codes = new List<string>() 
            {
               
@"
a = [Imperative]
{
    return = 1;
}
", 

@"
a = [Imperative]
{
    return = 2;
}
"
            };

            Guid guid = System.Guid.NewGuid();

            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("a", 1);

            // Modify the language block and verify
            List<Subtree> modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[1]));

            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("a", 2);
        }

        [Test]
        public void TestImperativeDeltaExecution02()
        {
            List<string> codes = new List<string>() 
            {
               
@"
a = [Imperative]
{
    x = 1;
    if (x > 1)
    {
        x = 10;
    }
    return = x;
}
", 

@"
a = [Imperative]
{
    x = 2;
    if (x > 1)
    {
        x = 10;
    }
    return = x;
}
"
            };

            Guid guid = System.Guid.NewGuid();

            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("a", 1);

            // Modify the language block and verify
            List<Subtree> modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[1]));

            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("a", 10);
        }

        [Test]
        public void TestImperativeDeltaExecution03()
        {
            List<string> codes = new List<string>() 
            {
               
@"
a = [Imperative]
{
    x = 1;
    if (x > 1)
    {
        x = 10;
    }
    return = x;
}
", 

@"
a = [Imperative]
{
    x = 1;
    if (x > 1)
    {
        x = 10;
    }
    else
    {
        x = 100;
    }
    return = x;
}
"
            };

            Guid guid = System.Guid.NewGuid();

            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("a", 1);

            // Modify the language block and verify
            List<Subtree> modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[1]));

            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("a", 100);
        }

        [Test]
        public void TestImperativeDeltaExecution04()
        {
            List<string> codes = new List<string>() 
            {
               
@"
a = [Imperative]
{
    x = 1;
    if (x > 0)
    {
        x = 10;
    }
    return = x;
}
", 

@"
a = [Imperative]
{
    x = 1;
    if (x > 0)
    {
        x = 100;
    }
    return = x;
}
"
            };

            Guid guid = System.Guid.NewGuid();

            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("a", 10);

            // Modify the language block and verify
            List<Subtree> modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[1]));

            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("a", 100);
        }

        [Test]
        public void TestNestedLanguageBlockExecution01()
        {
            List<string> codes = new List<string>() 
            {
@"
r = [Imperative]
{
    if (true)
    {
        return = [Associative] { return = 42; }
    }
    return = null;
}
"
            };

            Guid guid1 = System.Guid.NewGuid();

            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("r", 42);
        }

        [Test]
        public void TestNestedLanguageBlockExecution02()
        {
            List<string> codes = new List<string>() 
            {
@"
def foo()
{
    return = [Associative]
    {
        return = [Imperative]
        {
            return = [Associative]
            {
                t = 1;
                return = t;
            }
        }
    }
}

a = foo();
"
            };

            Guid guid1 = System.Guid.NewGuid();

            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("a", 1);
        }

        [Test]
        public void TestNestedLanguageBlockExecution03()
        {
            List<string> codes = new List<string>() 
            {
@"
def func()
{
    v = [Associative]
    {
        return = [Imperative]
        {
            t = false;
            if(true)
            {
                return = 1;
            }

            return = 2;
        }
    }
    return = v;
}

a = func();
"
            };

            Guid guid1 = System.Guid.NewGuid();

            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("a", 1);
        }

        [Test]
        public void TestNestedLanguageBlockReExecution01()
        {
            List<string> codes = new List<string>() 
            {
@"
r = [Associative]
{
    return = [Imperative] 
    { 
        return = 1; 
    }
}
"
,
@"
r = [Associative]
{
    return = [Imperative] 
    { 
        return = 2; 
    }
}
"
            };

            Guid guid1 = System.Guid.NewGuid();

            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("r", 1);

            List<Subtree> modified = new List<Subtree>();
            Subtree subtree = ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[1]);
            modified.Add(subtree);
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("r", 2);
        }

        [Test]
        public void TestNestedLanguageBlockReExecution02()
        {
            List<string> codes = new List<string>() 
            {
               @"r = [Imperative]
               {
                   if (true)
                   {
                       return = [Associative] { return = 42; }
                   }
                   return = null;
               }",

               @"r = [Imperative]
               {
                   if (true)
                   {
                       return = [Associative] { return = 45; }
                   }
                   return = null;
               }"
            };

            Guid guid1 = System.Guid.NewGuid();

            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("r", 42);

            List<Subtree> modified = new List<Subtree>();
            Subtree subtree = ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[1]);
            modified.Add(subtree);
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("r", 45);
        }

        [Test]
        public void TestNestedLanguageBlockReExecution03()
        {
            List<string> codes = new List<string>() 
            {
@"
a = [Associative]
{
    b = [Imperative]
    {
        return = 1 + 2; //  Modifying this line should re-execute the entire language block 'a = [Associative]{...}'
    }
    c = b + 10;
    return = c;
}
"
,
@"
a = [Associative]
{
    b = [Imperative]
    {
        return = 2 + 2; //  Modifying this line should re-execute the entire language block 'a = [Associative]{...}'
    }
    c = b + 10;
    return = c;
}
"
            };

            Guid guid1 = System.Guid.NewGuid();

            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("a", 13);

            List<Subtree> modified = new List<Subtree>();
            Subtree subtree = ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[1]);
            modified.Add(subtree);
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("a", 14);
        }
        [Test]
        public void TestNestedLanguageBlockReExecution04()
        {
            string code = @"
def func_1: var[]..[](x1 : var[]..[])
{
    x2 = x1;
    var_1 = [Associative]
    {
        return = [Imperative]
        {
            if (false)
            {
                var_2 = [Associative]
                {
                    return = [Imperative]
                    {
                        if(true)
                        {
                            return = x2;
                        }
                        else
                        {
                            x3 = 42;
                            return = x3;
                        }
                    }
                }

                return = var_2;
            }
            else
            {
                x4 = 1024;
                return = x4;
            }
        }
    }
    return = var_1;
}

x = 5;
r = func_1(x);
";

            Guid guid1 = System.Guid.NewGuid();
            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, code));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("r", 1024);
        }

        [Test]
        public void TestNestedLanguageBlockReExecution05()
        {
            string code = @"
def foo()
{
    x2 = 5;
    v1 = [Associative]
    {
        return = [Imperative]
        {
            if (false)
            { 
                v2 = [Associative]
                {
                    return = [Imperative]
                    {
                        if(true)
                        {
                            return = 10;
                        }
                        else
                        {
                            return = 15;
                        }
                    }
                }
                return = v2;
            }
            else
            {
                return = 20;
            }
        }
    }
    return = v1;
}

x = foo();
";

            Guid guid1 = System.Guid.NewGuid();
            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, code));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("x", 20);
        }


        [Test]
        public void TestNestedLanguageBlockReExecution06()
        {
            string code = @"

def foo()
{
    aa = 8;
    bb = aa;

    cc = [Associative]
    {
        return = [Imperative]
        {
            if(false)
            {
                return = [Associative]
                {
                    return = [Imperative]
                    {
                        return = bb;
                    }
                }
            }
        }
    }
    return = 16;
}

r = foo();
";

            Guid guid1 = System.Guid.NewGuid();
            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, code));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("r", 16);
        }

        [Test]
        public void TestNestedLanguageBlockReExecution07()
        {
            string code = @"
def func_11546f565974453bae527393c546bbff: var[]..[](x1 : var[]..[])
{
    tssa1 = x1;
    x1f = tssa1;
    tssa2 = x1f;
    x3 = tssa2;
    tssa3 = x3;
    x24 = tssa3;
    tssa3 = x24;
    x34 = tssa3;

    var_42 = [Associative]
    {
        return = [Imperative]
        {
            t1 = false;
            if(t1)
            {
                v1 = [Associative]
                {
                    return = [Imperative]
                    {
                        t2 = true;
                        if(t2)
                        {
                            x2 = x3;
                            x4 = x2;
                            return = x4;

                        }
                        else
                        {
                            x5 = 42;
                            return = x5;

                        }

                    }
                    ;

                }
                r1 = v1;
                x7 = r1;
                return = x7;

            }
            else
            {
                x8 = 1024;
                x9 = x8;
                x10 = x9;
                return = x10;
            }
        }
    }
    tssa4 = var_42;
    var_d = tssa4;
    tssa5 = var_d;
    return = tssa5;
}

x = 5;
r = func_11546f565974453bae527393c546bbff(x);
";

            Guid guid1 = System.Guid.NewGuid();
            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, code));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("r", 1024);
        }

        [Test]
        public void TestNestedLanguageBlockReExecution08()
        {
            string code = @"
def f(x)
{
    aa = [Imperative]
    {
        if(x <= 1)
        {
            return = [Associative]
            {
                bb = 1;
                return = bb;
            }
        }
        else
        {
            return = [Associative]
            {
                cc = f(x - 1) + x;   
                return = cc;
            }
        }
    }
    return = aa;
}

t = f(3);
";

            Guid guid1 = System.Guid.NewGuid();
            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, code));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("t", 6);
        }

        [Test]
        public void TestNestedLanguageBlockReExecution09()
        {
            string code = @"
def func_f417d5607cc14ef8bde1b821bada91da: var[]..[](x : var[]..[])
{
    t1 = 1;
    v1 = (x) <= (t1);
    v2 = [Imperative]
    {
        if(v1)
        {
            return = [Associative]
            {
                t2 = 1;
                return = t2;

            }
        }
        else
        {
            return = [Associative]
            {
                t3 = 1;
                v3 = (x) - (t3);
                v4 = func_f417d5607cc14ef8bde1b821bada91da(v3);
                v5 = (x) * (v4);
                return = v5;

            }
        }
    }
    v6 = v2;
    return = v6;
}

r = 4;
t = func_f417d5607cc14ef8bde1b821bada91da(r);
";

            Guid guid1 = System.Guid.NewGuid();
            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, code));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("t", 24);
        }

        [Test]
        public void TestNestedLanguageBlockReExecution10()
        {
            string code = @"
def foo: var[]..[](x : var[]..[])
{
    t0 = 1;
    cond= (x) <= (t0);
    v0 = [Imperative]
    {
        if(cond)
        {
            return = [Associative]
            {
                t1 = 1;
                return = t1;
            }
        }
        else
        {
            return = [Associative]
            {
                t2 = 1;
                v2 = (x) - (t2);
                v3 = foo(v2);
                v4 = (x) * (v3);
                return = v4;

            }
        }
    }
    v1 = v0;
    return = v1;
}

t = 5;
v = foo(t);
";

            Guid guid1 = System.Guid.NewGuid();
            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, code));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("v", 120);
        }


        [Test]
        public void TestNestedLanguageBlockReExecution11()
        {
            List<string> codes = new List<string>() 
            {
@"
    a = [Imperative]
    {
        return = 10;
    }

    b = [Imperative]
    {
    
        return = 20;
    }

    c = [Imperative]
    {
        d = 30;
        if (d == 0)
        {
            e = 40;
        }
        return = 50;
    }

    f = 60;
"
,
@"
    a = [Imperative]
    {
        return = 10;
    }

    b = [Imperative]
    {
    
        return = 20;
    }


    c = [Imperative]
    {
        d = 30;
        if (d == 0)
        {
            e = 40;
        }
        return = 50;
    }
",

@"
    f = 60;
"
            };

            Guid guid1 = System.Guid.NewGuid();
            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("f", 60);

            // Modify the CBN to remove the last line
            List<Subtree> modified = new List<Subtree>();
            Subtree subtree = ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[1]);
            modified.Add(subtree);

            // Create a new CBN to add the removed line
            Guid guid2 = System.Guid.NewGuid();
            added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[2]));
            syncData = new GraphSyncData(null, added, modified);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("f", 60);

        }

        [Test]
        public void RegressMagn4659()
        {
            List<string> codes = new List<string>() 
            {
                @"import(""FFITarget.dll""); p = DummyPoint.ByCoordinates(0.0, 0.0, 0.0);",
                "x = p.X;",
                "r = x;",
                "x = p.X();",
            };

            List<Subtree> added = new List<Subtree>();
            Guid guid1 = System.Guid.NewGuid();
            Guid guid2 = System.Guid.NewGuid();
            Guid guid3 = System.Guid.NewGuid();

            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[1]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, codes[2]));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            Assert.AreEqual(0, astLiveRunner.RuntimeCore.RuntimeStatus.WarningCount);

            List<Subtree> modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[3]));

            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);
            Assert.AreEqual(1, astLiveRunner.RuntimeCore.RuntimeStatus.WarningCount);
            Assert.AreEqual(guid2, astLiveRunner.RuntimeCore.RuntimeStatus.Warnings.First().GraphNodeGuid);
        }


        [Test]
        public void TestTransactionUpdate01()
        {
            string code = @"
import(""FFITarget.dll""); 
TestUpdateCount.Reset();
x = 1;
y = 2;
p = TestUpdateCount.Ctor(x,y);
x = 10;
y = 20;
a = p.UpdateCount;
";
            Guid guid1 = System.Guid.NewGuid();
            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, code));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Update should only have happened twice
            AssertValue("a", 2);
        }

        [Test]
        public void TestTransactionUpdate02()
        {
            string code = @"
import(""FFITarget.dll"");
TestUpdateCount.Reset();
x = 1;
y = 2;
p = TestUpdateCount.Ctor(x,y);
x = 10;
y = 20;
x = 30;
y = 40;
a = p.UpdateCount;
";
            Guid guid1 = System.Guid.NewGuid();
            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, code));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Update should only have happened twice
            AssertValue("a", 2);
        }

        [Test]
        public void TestTransactionUpdate03()
        {
            List<string> codes = new List<string>() 
            {
                @"import(""FFITarget.dll""); TestUpdateCount.Reset();", 
                "x = 1; y = 2;",
                "p = TestUpdateCount.Ctor(x,y);",
                "a = p.UpdateCount;",
                "x = 10; y = 20;",
            };

            List<Subtree> added = new List<Subtree>();

            // Create CBN1 for import
            Guid guid1 = System.Guid.NewGuid();
            Guid guid2 = System.Guid.NewGuid();
            Guid guid3 = System.Guid.NewGuid();
            Guid guid4 = System.Guid.NewGuid();

            // Create CBN1 for import
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            // Create CBN2 for x and y
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[1]));
            // Create CBN3 for TestCount constructor
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, codes[2]));
            // Create CBN4 for UpdateCount
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid4, codes[3]));

            // Verify that UpateCount is only called once
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("a", 1);


            // Modify CBN2 with same contents with ForceExecution flag set
            Subtree subtree = ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[4]);
            List<Subtree> modified = new List<Subtree>();
            modified.Add(subtree);
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("a", 2);
        }

        [Test]
        public void TestTransactionUpdate04()
        {
            List<string> codes = new List<string>() 
            {
                @"import(""FFITarget.dll""); TestUpdateCount.Reset();", 
                "x = 1;",
                "p = TestUpdateCount.Ctor(x,0);",
                "a = p.UpdateCount;",
                "x = 10;",
            };

            List<Subtree> added = new List<Subtree>();

            // Create CBN1 for import
            Guid guid1 = System.Guid.NewGuid();
            Guid guid2 = System.Guid.NewGuid();
            Guid guid3 = System.Guid.NewGuid();
            Guid guid4 = System.Guid.NewGuid();

            // Create CBN1 for import
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            // Create CBN2 for x and y
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[1]));
            // Create CBN3 for TestCount constructor
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, codes[2]));
            // Create CBN4 for UpdateCount
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid4, codes[3]));

            // Verify that UpateCount is only called once
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("a", 1);


            // Modify CBN2 
            Subtree subtree = ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[4]);
            List<Subtree> modified = new List<Subtree>();
            modified.Add(subtree);

            // Modify CBN3 with same contents with ForceExecution flag set
            subtree = ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, codes[2]);
            subtree.ForceExecution = true;
            modified.Add(subtree);

            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("a", 2);
        }

        [Test]
        public void TestUnboundVariableWarning01()
        {
            // Test that there are no warnings because the unbound variable is resolved downstream
            string code = 
            @"
            a = b; 
            b = 1;
            ";
        
            Guid guid = System.Guid.NewGuid();

            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, code));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            Assert.AreEqual(0, astLiveRunner.RuntimeCore.RuntimeStatus.WarningCount);
        }

        [Test]
        public void TestUnboundVariableWarning02()
        {
            // Test that there are no warnings because the unbound variable is resolved downstream
            string code =
            @"
            a = b; 
            b = c; 
            c = 1;
            ";

            Guid guid = System.Guid.NewGuid();

            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, code));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            Assert.AreEqual(0, astLiveRunner.RuntimeCore.RuntimeStatus.WarningCount);
        }

        [Test]
        public void RegressMAGN5353()
        {
            // This test case tries to verify that when a FFI object is deleted, 
            // the corresponding _Dispose() should be invoked.
            //
            // It is for defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-5353
            var added = new List<Subtree>();

            var guid1 = Guid.NewGuid();
            var code1 = @"import(""FFITarget.dll""); x = DisposeTracer(); DisposeTracer.DisposeCount = 0;";
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, code1));

            var guid2 = Guid.NewGuid();
            var code2 = "y = DisposeTracer.DisposeCount;";
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, code2));

            // Verify that UpateCount is only called once
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("y", 0);

            // Modify CBN2 
            Subtree subtree = new Subtree(new List<AssociativeNode>{}, guid1);
            List<Subtree> deleted = new List<Subtree>();
            deleted.Add(subtree);

            syncData = new GraphSyncData(deleted, null, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("y", 1);
        }

        [Test]
        public void TestAssociativeupdateWithinFunction01()
        {
            // Test that there are no warnings because the unbound variable is resolved downstream
            string code =
            @"
def f()
{
	a = 1;
	b = a;
	a = 10;
	return = b;
}
x = f();
            ";

            Guid guid = System.Guid.NewGuid();

            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, code));

            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            AssertValue("x", 10);
        }

        [Test]
        public void TestReExecuteScopeIf01()
        {
            List<string> codes = new List<string>() 
            {

@"
a = false;
",

 @"
a = true;
",

@"

d = [Imperative]
{
    if(a)
    {
        return = [Associative]
        {
            e = true;
            return = e;

        }
    }
    else
    {
        return = [Associative]
        {
            f = false;
            return = f;

        }
    }
}
"
            };

            List<Subtree> added = new List<Subtree>();
            Guid guid1 = System.Guid.NewGuid();
            Guid guid2 = System.Guid.NewGuid();

            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[2]));

            // Execute All
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);

            // Modify 'a 'to true
            List<Subtree> modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[1]));
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            // Modify 'a 'to false
            modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

        }

        [Test]
        public void TestFalseCyclicExecution01()
        {
            List<string> codes = new List<string>() 
            { 
@"
i = {1}; 
j = i[0];
"
,

@"
j = 999;
i = {};
i[0] = j;
"
,
@"
i = {100}; 
j = i[0];
"
            };

            Guid guid1 = System.Guid.NewGuid();

            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("j", 1);

            // Modify guid3
            // Disconnect input
            List<Subtree> modified = new List<Subtree>();
            Subtree subtree = ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[1]);
            modified.Add(subtree);
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("j", 999);

            // Modify guid3
            // Reconnect input
            modified = new List<Subtree>();
            subtree = ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[2]);
            modified.Add(subtree);
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("j", 100);
        }

        [Test]
        public void TestFalseCyclicExecution02()
        {
            List<string> codes = new List<string>() 
            { 
@"
i = {1,2}; 
j = i[0];
k = i[1];
"
,

@"
j = 999;
k = 999;
i = {};
i[0] = j;
i[1] = k;
"
,
@"
i = {10,20}; 
j = i[0];
k = i[1];
"
            };

            Guid guid1 = System.Guid.NewGuid();

            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("j", 1);
            AssertValue("k", 2);

            // Modify guid3
            // Disconnect input
            List<Subtree> modified = new List<Subtree>();
            Subtree subtree = ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[1]);
            modified.Add(subtree);
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("j", 999);
            AssertValue("k", 999);

            // Modify guid3
            // Reconnect input
            modified = new List<Subtree>();
            subtree = ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[2]);
            modified.Add(subtree);
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("j", 10);
            AssertValue("k", 20);
        }

        [Test]
        public void TestMAGN5477()
        {
            List<string> codes = new List<string>() 
            {
@"

import(""FunctionObject.ds"");
def foosa: var[]..[](a1 : var[]..[], a2 : var[]..[])
{
    p = a2;
    q = a1;
    r = {};
    r[""b""] = q;
    r[""a""] = p;
    return = r;
}
"
,

@"
x = 1;
y = 2;
z = 3;
"

, 

@"
i = foosa(x, y);
j = __TryGetValueFromNestedDictionaries(i, ""b"");
k = __TryGetValueFromNestedDictionaries(i, ""a"");
"
,

@"
partialVar = _SingleFunctionObject(foosa, 2, {0}, {x, null}, true);
j = _SingleFunctionObject(__ComposeBuffered, 3, {0, 1}, {{_SingleFunctionObject(__GetOutput, 2, {1}, {null, ""b""}, true), partialVar}, 1, null}, true);
i_out1 = _SingleFunctionObject(__ComposeBuffered, 3, {0, 1}, {{_SingleFunctionObject(__GetOutput, 2, {1}, {null, ""a""}, true), partialVar}, 1, null}, true);
i = {};
i[""b""] = j;
i[""a""] = k;
"
,
@"
i = foosa(x, z);
j = __TryGetValueFromNestedDictionaries(i, ""b"");
k = __TryGetValueFromNestedDictionaries(i, ""a"");
"
            };

            Guid guid1 = System.Guid.NewGuid();
            Guid guid2 = System.Guid.NewGuid();
            Guid guid3 = System.Guid.NewGuid();

            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[1]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, codes[2]));
            var syncData = new GraphSyncData(null, added, null);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("j", 1);
            AssertValue("k", 2);

            // Modify guid3
            // Disconnect input
            List<Subtree> modified = new List<Subtree>();
            Subtree subtree = ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, codes[3]);
            modified.Add(subtree);
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);

            // Modify guid3
            // Reconnect input
            modified = new List<Subtree>();
            subtree = ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, codes[4]);
            modified.Add(subtree);
            syncData = new GraphSyncData(null, null, modified);
            astLiveRunner.UpdateGraph(syncData);
            AssertValue("j", 1);
            AssertValue("k", 3);
        }
     
    }

}

