using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
using ProtoTest.TD;
using ProtoTestFx.TD;
using ProtoScript.Runners;
namespace ProtoTest.Macroblocks
{
    public class MicroFeatureTests : ProtoTestBase
    {
        private ProtoScript.Runners.LiveRunner liveRunner = null;

        public override void Setup()
        {
            base.Setup();
            liveRunner = new ProtoScript.Runners.LiveRunner();
            liveRunner.ResetVMAndResyncGraph(new List<string> { "FFITarget.dll" });
            runtimeCore = liveRunner.RuntimeCore;
        }

        public override void TearDown()
        {
            base.TearDown();
            liveRunner.Dispose();
        }
        [Test]
        public void TestSimpleParallel01()
        {
            const string code = @"
a = 1;
b = 2;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1);
            thisTest.Verify("b", 2);
        }

        [Test]
        public void TestSimpleParallel02()
        {
            const string code = @"
a = 1;
b = 2;

x = 3;
y = x;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1);
            thisTest.Verify("b", 2);
            thisTest.Verify("x", 3);
            thisTest.Verify("y", 3);
        }

        [Test]
        public void TestInputblock01()
        {
            const string code = @"
a = 1;
b = 2;
c = a + b;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1);
            thisTest.Verify("b", 2);
            thisTest.Verify("c", 3);
        }


        [Test]
        public void TestDeferredInput01()
        {
            List<string> codes = new List<string>() 
            {
                "c = a + b;",
                "a = 1;",
                "b = 2;",
            };
            Guid guid1 = System.Guid.NewGuid();
            Guid guid2 = System.Guid.NewGuid();
            Guid guid3 = System.Guid.NewGuid();

            // Create CBN
            List<Subtree> added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid1, codes[0]));
            var syncData = new GraphSyncData(null, added, null);
            liveRunner.UpdateGraph(syncData);
            //AssertValue("c", null);

            added = new List<Subtree>();
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid2, codes[1]));
            added.Add(ProtoTestFx.TD.TestFrameWork.CreateSubTreeFromCode(guid3, codes[2]));
            syncData = new GraphSyncData(null, added, null);
            liveRunner.UpdateGraph(syncData);
            //AssertValue("c", 3);
        }
    }
}