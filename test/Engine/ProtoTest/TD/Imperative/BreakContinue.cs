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
            string src = @"
i = [Imperative]
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
    return [x, y];
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("i", new[] {11, 5});
        }

        [Test]
        [Category("SmokeTest")]
        public void T02_WhileBreakContinue()
        {
            string src = @"
sum = [Imperative]
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
    return sum;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("sum", 40);
        }

        [Test]
        [Category("SmokeTest")]
        public void T03_ForLoopBreakContinue()
        {
            string src = @"
sum = [Imperative]
{
    sum = 0;
    for (x in [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13])
    {
        if (x >= 11)
            break;
        sum = sum + x;
    }
    return sum;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("sum", 55);
        }

        [Test]
        [Category("SmokeTest")]
        public void T04_ForLoopBreakContinue()
        {
            string src = @"
sum = [Imperative]
{
    sum = 0;
    for (x in [1, 2, 3, 4, 5, 6, 7, 8, 9, 10])
    {
        sum = sum + x;
        if (x <= 5)
            continue;
        sum = sum + 1;
    }
    return sum;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("sum", 60);
        }

        [Test]
        [Category("SmokeTest")]
        public void T05_FunctionBreakContinue()
        {
            string src = @"
    def ding:int(x:int)
    {
        return [Imperative] 
        {
            if (x >= 5)
                break;
            return 2 * x;
        }
    }
    def dong:int(x: int)
    {
        return [Imperative] 
        {
            if (x >= 5)
                continue;
            return 2 * x;
        }
    }
    i = [Imperative]
    {
        a = ding(1);
        b = ding(6);
        c = dong(2);
        d = dong(7);
        return [a, b, c, d];
    }
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.FunctionAbnormalExit);
            thisTest.Verify("i", new object[] {2, null, 4, null});
        }
    }
}
