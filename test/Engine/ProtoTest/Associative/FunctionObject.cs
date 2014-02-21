using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTest.TD;
using ProtoTestFx.TD;
namespace ProtoTest.Associative
{
    public class FunctionObjectTest
    {
        public TestFrameWork thisTest = new TestFrameWork();
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestApply01()
        {
            string code =
    @"
import (""FunctionObject.ds"");
def add(x,y) { return = x + y;}

// fo = add(?, 42);
fo = _SingleFunctionObject(add, 2, {1}, {null, 42});
r = Apply(fo, 3);
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
fo1 = _SingleFunctionObject(add, 3, {1}, {null, 42, null});

// foo2 = add(100, 42, ?);
fo2 = Apply(fo1, 100);

r = Apply(fo2, 3);
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
fo = _SingleFunctionObject(add, 2, {1}, {null, {100, 200}});
r = Apply(fo, {1, 2});
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
fo = _SingleFunctionObject(add, 2, {1}, {null, {100, 200}});
r = Apply(fo, 1);
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
fo = _SingleFunctionObject(add, 2, {1}, {null, {100, 200}});
r = Apply(fo, {1});
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
    return = _SingleFunctionObject(add, 2, {1}, {null, 100});
}

fo = getFunctionObject();

r = Apply(fo, 42);
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
    return = _SingleFunctionObject(f, 2, {1}, {null, 100});
}

fo1 = getFunctionObject(add);
r1 = Apply(fo1, 42);
fo2 = getFunctionObject(mul);
r2 = Apply(fo2, 3);
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
    return = _SingleFunctionObject(f, 2, {1}, {null, 100});
}

fo = getFunctionObject({add, mul});
r = Apply(fo, 3);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", new object[] {103, 300});
        }

        [Test]
        public void TestCompose01()
        {
            string code =
    @"
import (""FunctionObject.ds"");
def add(x,y) { return = x + y;}
def mul(x, y) { return = x * y;}

fo1 = _SingleFunctionObject(add, 2, {1}, {null, 100});
fo2 = _SingleFunctionObject(mul, 2, {0}, {3, null});
fo3 = _ComposedFunctionObject({fo1, fo2});

// r = 2 * 3 + 100
r = Apply(fo3, 2);
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

fo1 = _SingleFunctionObject(add, 2, {1}, {null, 100});
fo2 = _ComposedFunctionObject({fo1, fo1});

// r = 42 + 100 + 100
r = Apply(fo2, 42);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 242);
        }

        [Test]
        public void TestCompose03()
        {
            string code =
    @"
import (""FunctionObject.ds"");
def add(x, y) { return = x + y; }

def mul(x, y) { return = x * y; }

fo1 = _SingleFunctionObject(add, 2, { 1 }, { null, 3});
fo2 = _SingleFunctionObject(mul, 2, { 0 }, { 5, null });

r1 = Apply(fo1, 7);     // 3 + 7
r2 = Apply(fo2, 11);    // 5 * 11

comp1 = Compose({ fo1, fo2 }); 
r3 = Apply(comp1, 11);  // (5 * 11) + 3

comp2 = Compose({ fo2, fo1 });
r4 = Apply(comp2, 7);         // 5 * (3 + 7)

comp3 = Compose({ comp1, fo1, fo2 });
r5 = Apply(comp3, 9);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 10);
            thisTest.Verify("r2", 55);
            thisTest.Verify("r3", 58);
            thisTest.Verify("r4", 50);
            thisTest.Verify("r5", 243);
        }

        [Test]
        public void TestApplyOnStaticFunction()
        {
            string code =
    @"
import (""FunctionObject.ds"");
class Foo
{
    static def foo(x, y)
    {
        return = x + y;
    }
}

fo = _SingleFunctionObject(Foo.foo, 2, { 1 }, { null, 100 });
r = Apply(fo, 3);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 103);
        }

        [Test]
        public void TestApplyOnConstructor()
        {
            string code =
    @"
import (""FunctionObject.ds"");
class Foo
{
    i;
    constructor Foo(x, y)
    {
        i = x + y;
    }
}

c = _SingleFunctionObject(Foo.Foo, 2, { 1 }, { null, 100 });
f = Apply(c, 3);
r = f.i;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 103);
        }

        [Test]
        public void TestSortByKey()
        {
            string code =
    @"
import (""FunctionObject.ds"");
class Point
{
    constructor Point(_x, _y, _z)
    {
        x = _x;
        y = _y;
        z = _z;
    }
    
    x; y; z;
}

p1 = Point(1, 2, 3);
p2 = Point(2, 3, 4);
p3 = Point(2, 1, 3);

def getCoordinateValue(p : Point)
{
    return = p.x + p.y + p.z;
}

getPointKey = _SingleFunctionObject(getCoordinateValue, 1, { }, { });
r1 = SortByKey(null, getPointKey);
r2 = SortByKey({ }, getPointKey);

r3 = SortByKey({ p1 }, getPointKey);
t1 = Map(r3, getPointKey);

r4 = SortByKey({ p1, p1, p1 }, getPointKey);
t2 = Map(r4, getPointKey);

r5 = SortByKey({ p1, p2, p3 }, getPointKey);
t3 = Map(r5, getPointKey);

r6 = SortByKey({ p2, p1 }, getPointKey);
t4 = Map(r6, getPointKey);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("t1", new object[]{6});
            thisTest.Verify("t2", new object[] { 6, 6, 6 });
            thisTest.Verify("t3", new object[] { 6, 6, 9 });
            thisTest.Verify("t4", new object[] { 6, 9});
        }

        [Test]
        public void TestSortByComparision()
        {
            string code =
    @"
import (""FunctionObject.ds"");
class Point
{
    constructor Point(_x, _y, _z)
    {
        x = _x;
        y = _y;
        z = _z;
    }
    
    x; y; z;
}

p1 = Point(2, 1, 3);
p2 = Point(1, 3, 3);
p3 = Point(1, 2, 4);
p4 = Point(3, 1, 2);

def comparePoint(p1 : Point, p2: Point)
{
    return = [Imperative]
    {
        if (p1.x > p2.x)
        {
            return = 1;
        }
        else if (p1.x < p2.x)
        {
            return = -1;
        }
        else
        {
            if (p1.y > p2.y)
            {
                return = 1;
            }
            else if (p1.y < p2.y)
            {
                return = -1;
            }
            else
            {
                if (p1.z > p2.z)
                {
                    return = 1;
                }
                else if (p1.z < p2.z)
                {
                    return = -1;
                }
                else
                {
                    return = 0;
                }
            }
        }
    }
}

pointComparer = _SingleFunctionObject(comparePoint, 2, { }, { });

r = SortByComparsion({ p1, p2, p3, p4 }, pointComparer);
r1 = r.x;
r2 = r.y;
r3 = r.z;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", new object[] { 1, 1, 2, 3 });
            thisTest.Verify("r2", new object[] { 2, 3, 1, 1});
            thisTest.Verify("r3", new object[] { 4, 3, 3, 2 });
        }

        [Test]
        public void TestGroupByKey()
        {
            string code =
    @"
import (""FunctionObject.ds"");
class Point
{
    constructor Point(_x, _y, _z)
    {
        x = _x;
        y = _y;
        z = _z;
    }
    
    x; y; z;
}

p1 = Point(1, 2, 3);
p2 = Point(2, 3, 4);
p3 = Point(2, 1, 3);

def getCoordinateValue(p : Point)
{
    return = p.x + p.y + p.z;
}

getPointKey = _SingleFunctionObject(getCoordinateValue, 1, { }, { });
r = GroupByKey({ p1, p2, p3 }, getPointKey);
t1 = Map(r[0], getPointKey);
t2 = Map(r[1], getPointKey);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("t1", new object[] { 6, 6});
            thisTest.Verify("t2", new object[] { 9 });
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

pred = _SingleFunctionObject(odd, 1, { }, { });
r1 = Filter(1..10, pred);
r2 = FilterOut(1..10, pred);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", new object[] { 1, 3, 5, 7, 9 });
            thisTest.Verify("r2", new object[] { 2, 4, 6, 8, 10 });
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

acc1 = _SingleFunctionObject(mul, 2, { }, { });
acc2 = _SingleFunctionObject(sum, 2, { }, { });
v1 = Reduce(1..10, 1, acc1);
v2 = Reduce(1..10, 0, acc2);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("v1", 3628800);
            thisTest.Verify("v2", 55);
        }
    }
}
