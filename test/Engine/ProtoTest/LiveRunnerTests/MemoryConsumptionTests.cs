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
    
    class MemoryConsumptionTests : ProtoTestBase
    {
        private int instrStreamStart = 0;
        private int instrStreamEnd = 0;
        private ProtoScript.Runners.LiveRunner liverunner = null;

        public override void Setup()
        {
            base.Setup();
            liverunner = new ProtoScript.Runners.LiveRunner(); 
        }

        public override void TearDown()
        {
            liverunner.Dispose();
            base.TearDown();
        }


        [Test]
        public void TestInstructionStreamMemory_SimpleWorkflow01()
        {
            List<string> codes = new List<string>() 
            {
                "a = 1;",  
                "a = 2;"      
            };

            Guid guid = Guid.NewGuid();

            // First run
            // a = 1
            List<Subtree> added = new List<Subtree>();
            Subtree st = TestFrameWork.CreateSubTreeFromCode(guid, codes[0]);
            st.IsInput = true;
            added.Add(st);
            var syncData = new GraphSyncData(null, added, null);
            liverunner.UpdateGraph(syncData);

            RuntimeMirror mirror = liverunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 1);

            // Modify 
            // a = 2
            List<Subtree> modified = new List<Subtree>(); 
            st = TestFrameWork.CreateSubTreeFromCode(guid, codes[1]);
            st.IsInput = true;
            modified.Add(st);
            syncData = new GraphSyncData(null, null, modified);
            liverunner.UpdateGraph(syncData);

            mirror = liverunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 2);

            Assert.AreEqual(instrStreamStart, instrStreamEnd);
        }


        [Test]
        public void TestPeriodicUpdate01()
        {
            int rhs = 0;
            string code = String.Format("a = {0};", rhs.ToString());

            Guid guid = Guid.NewGuid();

            // First run
            // a = 0
            List<Subtree> added = new List<Subtree>();
            Subtree st = TestFrameWork.CreateSubTreeFromCode(guid, code);
            st.IsInput = true;
            added.Add(st);
            var syncData = new GraphSyncData(null, added, null);
            liverunner.UpdateGraph(syncData);

            RuntimeMirror mirror = liverunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 0);

            List<Subtree> modified;

            const int maxUpdate = 100;
            for (int n = 1; n <= maxUpdate; ++n)
            {
                // Modify a
                code = String.Format("a = {0};", n.ToString());
                modified = new List<Subtree>();
                st = TestFrameWork.CreateSubTreeFromCode(guid, code);
                st.IsInput = true;
                modified.Add(st);
                syncData = new GraphSyncData(null, null, modified);
                liverunner.UpdateGraph(syncData);
            }

            mirror = liverunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 100);
        }

        [Test]
        public void TestInstructionStreamMemory_FunctionRedefinition01()
        {
            List<string> codes = new List<string>() 
            {
                "def f() {return = 1;}",  
                "a = f();",  
                "def f() {return = 2;}"
            };

            Guid guid1 = System.Guid.NewGuid();
            Guid guid2 = System.Guid.NewGuid();

            // First run
            List<Subtree> added = new List<Subtree>();
            Subtree st = ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]);
            added.Add(st);

            st = ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[1]);
            added.Add(st);

            var syncData = new GraphSyncData(null, added, null);
            liverunner.UpdateGraph(syncData);

            ProtoCore.Mirror.RuntimeMirror mirror = liverunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 1);

            // Modify function
            List<Subtree> modified = new List<Subtree>();
            st = ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[2]);
            modified.Add(st);
            syncData = new GraphSyncData(null, null, modified);
            liverunner.UpdateGraph(syncData);

            mirror = liverunner.InspectNodeValue("a");
            Assert.IsTrue((Int64)mirror.GetData().Data == 2);

            Assert.AreEqual(instrStreamStart, instrStreamEnd);
        }
    }
}

