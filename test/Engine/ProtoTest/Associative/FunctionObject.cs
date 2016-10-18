using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTest.TD;
using ProtoTestFx.TD;
namespace ProtoTest.Associative
{
    class FunctionObjectTest : ProtoTestBase
    {
        [Test]
        public void LoopWhile01()
        {
            string code =
    @"
import (""FunctionObject.ds"");
def add1(x) { return = x + 1; }
def lt10(x) { return = x < 10; }
add1fo = __CreateFunctionObject(add1, 1, {}, {null}, true);
lt10fo = __CreateFunctionObject(lt10, 1, {}, {null}, true);
r = LoopWhile(0, lt10fo, add1fo);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 10);
        }

        [Test]
        public void TestApply01()
        {
            string code =
    @"
import (""FunctionObject.ds"");
def add(x,y) { return = x + y;}

// fo = add(?, 42);
fo = __CreateFunctionObject(add, 2, {1}, {null, 42}, true);
r = __Apply(fo, 3);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 45);
        }

        [Test]
        public void TestApply02()
        {
            string code =
    @"
import (""FunctionObject.ds"");
def add(x,y,z) { return = x + y + z;}

// foo1 = add(?, 42, ?);
fo1 = __CreateFunctionObject(add, 3, {1}, {null, 42, null}, true);

// foo2 = add(100, 42, ?);
fo2 = __Apply(fo1, 100);

r = __Apply(fo2, 3);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 145);
        }

        [Test]
        public void TestApply03()
        {
            string code =
    @"
import (""FunctionObject.ds"");
def add(x,y) { return = x + y;}

// fo = add(?, {100, 200});
fo = __CreateFunctionObject(add, 2, {1}, {null, {100, 200}}, true);
r = __Apply(fo, {1, 2});
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", new object[] { 101, 202 });
        }

        [Test]
        public void TestApply04()
        {
            string code =
    @"
import (""FunctionObject.ds"");
def add(x,y) { return = x + y;}

// fo = add(?, {100, 200});
fo = __CreateFunctionObject(add, 2, {1}, {null, {100, 200}}, true);
r = __Apply(fo, 1);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", new object[] { 101, 201 });
        }

        [Test]
        public void TestApply05()
        {
            string code =
    @"
import (""FunctionObject.ds"");
def add(x,y) { return = x + y;}

// fo = add(?, {100, 200});
fo = __CreateFunctionObject(add, 2, {1}, {null, {100, 200}}, true);
r = __Apply(fo, {1});
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", new object[] { 101 });
        }

        [Test]
        public void TestApply06()
        {
            // Return a function object from function
            string code =
    @"
import (""FunctionObject.ds"");
def add(x,y) { return = x + y;}

def getFunctionObject()
{
    return = __CreateFunctionObject(add, 2, {1}, {null, 100}, true);
}

fo = getFunctionObject();

r = __Apply(fo, 42);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 142);
        }


        [Test]
        public void TestApply07()
        {
            // Return a function object from function
            string code =
    @"
import (""FunctionObject.ds"");
def add(x,y) { return = x + y;}
def mul(x,y) { return = x * y;}
def getFunctionObject(f:function)
{
    return = __CreateFunctionObject(f, 2, {1}, {null, 100}, true);
}

fo1 = getFunctionObject(add);
r1 = __Apply(fo1, 42);
fo2 = getFunctionObject(mul);
r2 = __Apply(fo2, 3);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 142);
            thisTest.Verify("r2", 300);
        }

        [Test]
        public void TestApply08()
        {
            // Return a function object from function
            string code =
    @"
import (""FunctionObject.ds"");
def add(x,y) { return = x + y;}
def mul(x,y) { return = x * y;}
def getFunctionObject(f:function)
{
    return = __CreateFunctionObject(f, 2, {1}, {null, 100}, true);
}

fo1 = getFunctionObject(add);
fo2 = getFunctionObject(mul);
r1 = __Apply(fo1, 3);
r2 = __Apply(fo2, 3);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 103);
            thisTest.Verify("r2", 300);
        }

        [Test]
        public void TestCompose01()
        {
            string code =
    @"
import (""FunctionObject.ds"");
def add(x,y) { return = x + y;}
def mul(x, y) { return = x * y;}

fo1 = __CreateFunctionObject(add, 2, {1}, {null, 100}, true);
fo2 = __CreateFunctionObject(mul, 2, {0}, {3, null}, true);
fo3 = __CreateComposedFunctionObject({fo1, fo2});

// r = 2 * 3 + 100
r = __Apply(fo3, 2);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 106);
        }

        [Test]
        public void TestCompose02()
        {
            string code =
    @"
import (""FunctionObject.ds"");
def add(x,y) { return = x + y;}
def mul(x, y) { return = x * y;}

fo1 = __CreateFunctionObject(add, 2, {1}, {null, 100}, true);
fo2 = __CreateComposedFunctionObject({fo1, fo1});

// r = 42 + 100 + 100
r = __Apply(fo2, 42);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 242);
        }

        [Test]
        public void TestCompose03()
        {
            // Tracked by: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4037
            string err = "MAGN-4037 Defects with FunctionObject tests";
            string code =
    @"
import (""FunctionObject.ds"");
def add(x, y) { return = x + y; }

def mul(x, y) { return = x * y; }

fo1 = __CreateFunctionObject(add, 2, { 1 }, { null, 3}, true);
fo2 = __CreateFunctionObject(mul, 2, { 0 }, { 5, null }, true);

r1 = __Apply(fo1, 7);     // 3 + 7
r2 = __Apply(fo2, 11);    // 5 * 11

comp1 = __Compose({ fo1, fo2 }); 
r3 = __Apply(comp1, 11);  // (5 * 11) + 3

comp2 = __Compose({ fo2, fo1 });
r4 = __Apply(comp2, 7);         // 5 * (3 + 7)

comp3 = __Compose({ comp1, fo1, fo2 });
r5 = __Apply(comp3, 9);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 10);
            thisTest.Verify("r2", 55);
            thisTest.Verify("r3", 58);
            thisTest.Verify("r4", 50);
            thisTest.Verify("r5", 243);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestApplyOnFunction01()
        {
            string code =
    @"
import (""FunctionObject.ds"");
def foo(x, y)
{
    return = x + y;
}

fo = __CreateFunctionObject(foo, 2, { 1 }, { null, 100 }, true);
r = __Apply(fo, 3);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 103);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestApplyOnFunction02()
        {
            string code =
    @"
import (""FunctionObject.ds"");

def Foo(x, y)
{
    return = x + y;
}


c = __CreateFunctionObject(Foo, 2, { 1 }, { null, 100 }, true);
f = __Apply(c, 3);
r = f;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 103);
        }

        [Test]
        public void TestFilter()
        {
            string code =
    @"
import (""FunctionObject.ds"");
def odd(x)
{
    return = x % 2 == 1;
}

pred = __CreateFunctionObject(odd, 1, { }, { }, true);
r1 = __Filter(1..10, pred);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", new object[] { new object[] { 1, 3, 5, 7, 9 }, new object[] { 2, 4, 6, 8, 10 } });
        }

        [Test]
        public void TestFilter2()
        {
            string code =
    @"
import (""FunctionObject.ds"");
def odd(x)
{
    return = x % 2 == 1;
}

pred = __CreateFunctionObject(odd, 1, { }, { }, true);
r1 = __Filter({}, pred);

r2 = r1[0];
r3 = r1[1];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r2", new object[] { });
            thisTest.Verify("r3", new object[] { });
        }

        [Test]
        public void TestReduce()
        {
            string code =
    @"
import (""FunctionObject.ds"");
def mul(x, y)
{
    return = x * y;
}

def sum(x, y)
{
    return = x + y;
}

acc1 = __CreateFunctionObject(mul, 2, { }, { }, true);
acc2 = __CreateFunctionObject(sum, 2, { }, { }, true);

v1 = __Reduce(acc1, 1, 1..10);
v2 = __Reduce(acc2, 0, 1..10);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("v1", 3628800);
            thisTest.Verify("v2", 55);
        }
    }
}