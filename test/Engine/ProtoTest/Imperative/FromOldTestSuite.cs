using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
namespace ProtoTest.Imperative
{
    [TestFixture]
    class FromOldTestSuite : ProtoTestBase
    {
        [Test]
        public void TestConds01()
        {
            String code =
@"
f0;f1;f2;f3;t0;t1;t2;t3;t4;t5;t6;t7;
i = [Imperative]
{
	f0 = 5 > 6;
    f1 = (5 > 6);
    f2 = 5 >= 6;
    f3 = (5 >= 6);    
    t0 = 5 == 5;
    t1 = (5 == 5);
    t2 = 5 < 6;
    t3 = (5 < 6);
    
    t4 = 5 <= 6;
    t5 = (5 <= 6);
    t6 = 5 <= 5;
    t7 = (5 <= 5);
    return [f0, f1, f2, f3, t0, t1, t2, t3, t4, t5, t6, t7];
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("i", new[] {false, false, false, false, true, true, true, true, true, true, true, true});
        }

        [Test]
        public void TestConds02()
        {
            String code =
@"
f0;f1;f2;f3;t0;t1;t2;t3;t4;t5;t6;t7;
i = [Imperative]
{
	f0 = 0 > 1;
    f1 = (0 > 1);
    f2 = 0 >= 1;
    f3 = (0 >= 1);    
    t0 = 0 == 0;
    t1 = (0 == 0);
    t2 = 0 < 1;
    t3 = (0 < 1);
    
    t4 = 0 <= 1;
    t5 = (0 <= 1);
    t6 = 0 <= 0;
    t7 = (0 <= 0);

    return [f0, f1, f2, f3, t0, t1, t2, t3, t4, t5, t6, t7];
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("i", new[] { false, false, false, false, true, true, true, true, true, true, true, true });
        }

        [Test]
        public void TestConds03()
        {
            String code =
@"
f0;f1;f2;f3;t0;t1;t2;t3;t4;t5;t6;t7;
i = [Imperative]
{
	f0 = -1 > 0;
    f1 = (-1 > 0);
    f2 = -1 >= 0;
    f3 = (-1 >= 0);    
    t0 = -1 == -1;
    t1 = (-1 == -1);
    t2 = -1 < 0;
    t3 = (-1 < 0);
    
    t4 = -1 <= 0;
    t5 = (-1 <= 0);
    t6 = -1 <= -1;
    t7 = (-1 <= -1);
    return [f0, f1, f2, f3, t0, t1, t2, t3, t4, t5, t6, t7];
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("i", new[] { false, false, false, false, true, true, true, true, true, true, true, true });
        }
    }
}
