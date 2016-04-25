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
        [Test]
        public void TestVariableResolutionInClass01()
        {
            string code = @"
class Foo
{
    x1 = 1;
    static x2 = 20;

    def foo()
    {
        x3 = 300;
        return = [Imperative]
        {
            x4 = 4000;
            return = [Associative]
            {
                x5 = 50000;
                return = x1 + x2 + x3 + x4 + x5;
            }
        }
    }
}

f = Foo();
r = f.foo();
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 54321);
        }

        [Test]
        public void TestVariableResolutionInClass02()
        {
            // Global variables should not be accessible in class
            string code = @"
x0 = 21;
class Foo
{
    x1 = x0;
    static x2 = x0;
    x3 = x0;

    def foo1()
    {
        return = x0;
    }

    def foo2()
    {
        return = [Imperative]
        {
            return = x0;
        }
    }
}

f = Foo();
r1 = f.foo1();
r2 = f.foo2();
r3 = f.x2;
r4 = f.x3;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", null);
            thisTest.Verify("r2", null);
            thisTest.Verify("r3", null);
            thisTest.Verify("r4", null);
        }

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
