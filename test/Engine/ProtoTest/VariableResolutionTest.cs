using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProtoCore.DSASM;
using ProtoTestFx.TD;
using ProtoTest;

namespace ProtoTest
{
    [TestFixture]
    class VariableResolutionTest : ProtoTestBase
    {
        public void TestVariableResolution03()
        {
            string code = @"
def foo()
{
    x1 = 1;
    x2 = 20;
    x3 = 300;

    v1 = [Imperative]
    {
        v2 = [Associative]
        {
            return = x3;
        }
        return = x2 + v2;
    }

    return = x1 + v1;
}

r = foo();
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 321);
        }

        public void TestVariableResolutionNegative01()
        {
            string code = @"
x0 = 21;
def foo()
{
    return = [Imperative]
    {
        return = x0;
    }
}

r = foo();
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", null);
        }

        public void TestVariableResolutionNegative02()
        {
            string code = @"
x0 = 21;
def foo()
{
    return = x0;
}

r = foo();
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", null);
        }

        public void TestVariableResolutionNegative03()
        {
            string code = @"
x0 = 21;
def foo()
{
    return = [Imperative]
    {
        return = [Associative]
        {
            return = x0;
        }
    }
}

r = foo();
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", null);
        }
    }
}
