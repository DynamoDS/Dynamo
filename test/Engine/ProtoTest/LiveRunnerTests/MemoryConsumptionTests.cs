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
        ProtoCore.Diagnostics.Runtime runtimeDiagnostics = null;
        private ProtoScript.Runners.LiveRunner liverunner = null;

        public override void Setup()
        {
            base.Setup();
            liverunner = new ProtoScript.Runners.LiveRunner(); 
            runtimeDiagnostics = new ProtoCore.Diagnostics.Runtime(liverunner.RuntimeCore);
        }

        public override void TearDown()
        {
            liverunner.Dispose();
            base.TearDown();
        }


        [Test]
        [Category("Failure")]
        public void TestInstructionStreamMemory_SimpleWorkflow01()
        {
            List<string> codes = new List<string>() 
            {
                "a = 1;",  
                "a = 2;"      
            };

            Guid guid = System.Guid.NewGuid();

            // First run
            // a = 1
            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            liverunner.UpdateGraph(syncData);
            instrStreamStart = runtimeDiagnostics.GetExecutableInstructionCount();

            // Modify 
            // a = 2
            List<Subtree> modified = new List<Subtree>();
            modified.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid, codes[1]));
            syncData = new GraphSyncData(null, null, modified);
            liverunner.UpdateGraph(syncData);
            instrStreamEnd = runtimeDiagnostics.GetExecutableInstructionCount();

            Assert.AreEqual(instrStreamStart, instrStreamEnd);
        }
    }
}

