using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;
namespace ProtoTest.TD.Imperative
{
    class BreakContinueTest : ProtoTestBase
    {
        [Test]
        [Category("SmokeTest")]
        public void T01_WhileBreakContinue()
        {
            string src = @"x;
y;
[Imperative]
{
    x = 0;
    y = 0;
    while (true) 
    {
        x = x + 1;
        if (x > 10)
            break;
        
        if ((x == 1) || (x == 3) || (x == 5) || (x == 7) || (x == 9))
            continue;
        
        y = y + 1;
    }
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 11);
            Assert.IsTrue((Int64)mirror.GetValue("y").Payload == 5);
        }

        [Test]
        [Category("SmokeTest")]
        public void T02_WhileBreakContinue()
        {
            string src = @"sum;
[Imperative]
{
    x = 0;
    sum = 0;
    while (x <= 10) 
    {
        x = x + 1;
        if (x >= 5)
            break;
        
        y = 0;
        while (true) 
        {
            y = y + 1;
            if (y >= 10)
                break;
        }
        // y == 10 
        sum = sum + y;
    }
    // sum == 40 
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("sum").Payload == 40);
        }

        [Test]
        [Category("SmokeTest")]
        public void T03_ForLoopBreakContinue()
        {
            string src = @"sum;
[Imperative]
{
    sum = 0;
    for (x in {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13})
    {
        if (x >= 11)
            break;
        sum = sum + x;
    }
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("sum").Payload == 55);
        }

        [Test]
        [Category("SmokeTest")]
        public void T04_ForLoopBreakContinue()
        {
            string src = @"sum;
[Imperative]
{
    sum = 0;
    for (x in {1, 2, 3, 4, 5, 6, 7, 8, 9, 10})
    {
        sum = sum + x;
        if (x <= 5)
            continue;
        sum = sum + 1;
    }
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("sum").Payload == 60);
        }

        [Test]
        [Category("SmokeTest")]
        public void T05_FunctionBreakContinue()
        {
            string src = @"a;
b;
c;
d;
[Imperative]
{
    def ding:int(x:int)
    {
        if (x >= 5)
            break;
        return = 2 * x;
    }
    def dong:int(x: int)
    {
        if (x >= 5)
            continue;
        return = 2 * x;
    }
    a = ding(1);
    b = ding(6);
    c = dong(2);
    d = dong(7);
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kFunctionAbnormalExit);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 2);
            Assert.IsTrue(mirror.GetValue("b").DsasmValue.IsNull);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 4);
            Assert.IsTrue(mirror.GetValue("d").DsasmValue.IsNull);
        }
    }
}
