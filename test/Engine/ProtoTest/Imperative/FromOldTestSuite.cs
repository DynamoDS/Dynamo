using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTest.TD;
using ProtoTestFx.TD;
namespace ProtoTest.Imperative
{
    [TestFixture]
    class FromOldTestSuite : ProtoTestBase
    {
        [Test]
        public void TestConds01()
        {
            String code =
@"f0;f1;f2;f3;t0;t1;t2;t3;t4;t5;t6;t7;[Imperative]{	f0 = 5 > 6;    f1 = (5 > 6);    f2 = 5 >= 6;    f3 = (5 >= 6);        t0 = 5 == 5;    t1 = (5 == 5);    t2 = 5 < 6;    t3 = (5 < 6);        t4 = 5 <= 6;    t5 = (5 <= 6);    t6 = 5 <= 5;    t7 = (5 <= 5);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("f0", false);
            thisTest.Verify("f1", false);
            thisTest.Verify("f2", false);
            thisTest.Verify("f3", false);
            thisTest.Verify("t0", true);
            thisTest.Verify("t1", true);
            thisTest.Verify("t2", true);
            thisTest.Verify("t3", true);
            thisTest.Verify("t4", true);
            thisTest.Verify("t5", true);
            thisTest.Verify("t6", true);
            thisTest.Verify("t7", true);
        }

        [Test]
        public void TestConds02()
        {
            String code =
@"f0;f1;f2;f3;t0;t1;t2;t3;t4;t5;t6;t7;[Imperative]{	f0 = 0 > 1;    f1 = (0 > 1);    f2 = 0 >= 1;    f3 = (0 >= 1);        t0 = 0 == 0;    t1 = (0 == 0);    t2 = 0 < 1;    t3 = (0 < 1);        t4 = 0 <= 1;    t5 = (0 <= 1);    t6 = 0 <= 0;    t7 = (0 <= 0);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("f0", false);
            thisTest.Verify("f1", false);
            thisTest.Verify("f2", false);
            thisTest.Verify("f3", false);
            thisTest.Verify("t0", true);
            thisTest.Verify("t1", true);
            thisTest.Verify("t2", true);
            thisTest.Verify("t3", true);
            thisTest.Verify("t4", true);
            thisTest.Verify("t5", true);
            thisTest.Verify("t6", true);
            thisTest.Verify("t7", true);
        }

        [Test]
        public void TestConds03()
        {
            String code =
@"f0;f1;f2;f3;t0;t1;t2;t3;t4;t5;t6;t7;[Imperative]{	f0 = -1 > 0;    f1 = (-1 > 0);    f2 = -1 >= 0;    f3 = (-1 >= 0);        t0 = -1 == -1;    t1 = (-1 == -1);    t2 = -1 < 0;    t3 = (-1 < 0);        t4 = -1 <= 0;    t5 = (-1 <= 0);    t6 = -1 <= -1;    t7 = (-1 <= -1);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("f0", false);
            thisTest.Verify("f1", false);
            thisTest.Verify("f2", false);
            thisTest.Verify("f3", false);
            thisTest.Verify("t0", true);
            thisTest.Verify("t1", true);
            thisTest.Verify("t2", true);
            thisTest.Verify("t3", true);
            thisTest.Verify("t4", true);
            thisTest.Verify("t5", true);
            thisTest.Verify("t6", true);
            thisTest.Verify("t7", true);
        }

        [Test]
        public void FuncWithDec()
        {
            String code =
@"y;[Imperative]{        def sum:int( x:int, y:int )        {	        s = x + y;	        return = s;        }        	    y = 57;}";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            Assert.IsTrue((Int64)mirror.GetValue("y").Payload == 57);
        }


    }
}
