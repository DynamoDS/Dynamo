using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTest.TD;
using ProtoTestFx.TD;
namespace ProtoTest.MultiLangTests
{
    class FunctionPointerTest : ProtoTestBase
    {
        [Test]
        public void T01_BasicGlobalFunction()
        {
            string code = @"
def foo:int(x:int)
{
	return = x;
}
a = foo;
b = foo(3); //b=3;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object b = 3;
            thisTest.Verify("b", b);
        }

        [Test]
        public void T02_GlobalFunctionWithDefaultArg()
        {
            string code = @"
def foo:double(x:int, y:double = 2.0)
{
	return = x + y;
}
a = foo;
b = foo(3); //b=5.0;
c = foo(2, 4.0); //c = 6.0";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object b = 5.0;
            object c = 6.0;
            thisTest.Verify("b", b);
            thisTest.Verify("c", c);
        }

        [Test]
        public void T03_GlobalFunctionInAssocBlk()
        {
            string code = @"
a;
b;
c;
[Associative]
{
	def foo:double(x:int, y:double = 2.0)
	{
		return = x + y;
	}
	a = foo;
	b = foo(3); //b=5.0;
	c = foo(2, 4.0); //c = 6.0
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object b = 5.0;
            object c = 6.0;
            thisTest.Verify("b", b);
            thisTest.Verify("c", c);
        }

        [Test]
        public void T04_GlobalFunctionInImperBlk()
        {
            string code = @"
a;
b;
c;
[Imperative]
{
	def foo:double(x:int, y:double = 2.0)
	{
		return = x + y;
	}
	a = foo;
	b = foo(3); //b=5.0;
	c = foo(2, 4.0); //c = 6.0
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object b = 5.0;
            object c = 6.0;
            thisTest.Verify("b", b);
            thisTest.Verify("c", c);
        }

        [Test]
        public void T05_ClassMemerVarAsFunctionPointer()
        {
            string code = @"
class A
{
	x:var;
	constructor A()
	{
		x = foo;
	}
}
def foo:double(x:int, y:double = 2.0)
{
	return = x + y;
}
a = A.A();
b = a.x(3,2.0);	//b=5.0;
c = a.x(2, 4.0);	//c = 6.0";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object b = 5.0;
            object c = 6.0;
            thisTest.Verify("b", b);
            thisTest.Verify("c", c);
        }

        [Test]
        public void T05_ClassMemerVarAsFunctionPointerDefaultArg()
        {
            string code = @"
class A
{
	x:var;
	constructor A()
	{
		x = foo;
	}
}
def foo:double(x:int, y:double = 2.0)
{
	return = x + y;
}
a = A.A();
b = a.x(3,2.0);	//b=5.0;
c = a.x(2, 4.0);	//c = 6.0";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object b = 5.0;
            thisTest.Verify("b", b);
        }

        [Test]
        public void T06_ClassMemerVarAsFunctionPointerAssocBlk()
        {
            string code = @"
class A
{
	x:var;
	constructor A()
	{
		x = foo;
	}
}
def foo:double(x:int, y:double = 2.0)
{
	return = x + y;
}
a;
b;
c;
[Associative]
{
	a = A.A();
	b = a.x(3,2.0);	//b=5.0;
	c = a.x(2, 4.0);	//c = 6.0
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object b = 5.0;
            object c = 6.0;
            thisTest.Verify("b", b);
            thisTest.Verify("c", c);
        }

        [Test]
        public void T07_ClassMemerVarAsFunctionPointerImperBlk()
        {
            string code = @"
class A
{
	x:var;
	constructor A()
	{
		x = foo;
	}
}
def foo:double(x:int, y:double = 2.0)
{
	return = x + y;
}
a;
b;
c;
[Imperative]
{
	a = A.A();
	b = a.x(3);	//b=5.0;
	c = a.x(2, 4.0);	//c = 6.0
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object b = 5.0;
            object c = 6.0;
            thisTest.Verify("b", b);
            thisTest.Verify("c", c);
        }

        [Test]
        public void T08_FunctionPointerUpdateTest()
        {
            string code = @"
def foo1:int(x:int)
{
	return = x;
}
def foo2:double(x:int, y:double = 2.0)
{
	return = x + y;
}
a = foo1;
b = a(3);
a = foo2;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object b = 5.0;
            thisTest.Verify("b", b);
        }

        [Test]
        public void T09_NegativeTest_Non_FunctionPointer()
        {
            string code = @"
def foo:int(x:int)
{
	return = x;
}
a = 2;
b = a();";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object b = null;
            thisTest.Verify("b", b);
        }

        [Test]
        public void T10_NegativeTest_UsingFunctionNameAsVarName_Global()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
def foo:int(x:int)
{
	return = x;
}
foo = 3;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        public void T11_NegativeTest_UsingFunctionNameAsVarName_Global_ImperBlk()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
[Imperative]
{
	def foo:int(x:int)
	{
		return = x;
	}
	foo = 3;
}";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        public void T12_NegativeTest_UsingGlobalFunctionNameAsMemVarName_Class()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
def foo:int(x:int)
{
	return = x;
}
class A
{
	foo : var;
}";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        public void T13_NegativeTest_UsingMemFunctionNameAsMemVarName_Class()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
def foo:int(x:int)
{
	return = x;
}
class A
{
	memFoo : var;
	def memFoo()
	{
		return = 2;
	}
}";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("Negative")]
        [Category("Failure")]
        public void T14_NegativeTest_UsingFunctionNameInNonAssignBinaryExpr()
        {
            // Tracked by: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4038
            string err = "MAGN-4038 Compilation error for use of function pointer as a variable not reported";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
def foo:int(x:int)
{
	return = x;
}
a = foo + 2;";
                ExecutionMirror mirror = thisTest.RunScriptSource(code, err);
            });
        }

        [Test]
        public void T15_NegativeTest_UsingFunctionNameInNonAssignBinaryExpr_Global_ImperBlk()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
[Imperative]
{
	def foo:int(x:int)
	{
		return = x;
	}
	a = foo + 2;
}";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        public void T16_NegativeTest_UsingMemFunctionAsFunctionPtr()
        {
            string code = @"
def foo:int(x:int)
{
	return = x;
}
class A
{
	x : function; 
	y: function;
	constructor A()
	{
		x = foo;
		y = memFoo;
	}
	def memFoo(xx:int)
	{
		return = xx;
	}
}
a = A.A();
x = a.x(2);
y = a.y(2);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object x = 2;
            object y = null;
            thisTest.Verify("x", x);
            thisTest.Verify("y", y);
        }

        [Test]
        public void T17_PassFunctionPointerAsArg()
        {
            string code = @"
def foo:int(x:int)
{
	return = x;
}
def foo1:int(f:function, x:int)
{
	return = f(x);
}
a = foo1(foo, 2);
b = foo;
c = foo1(b, 3);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object a = 2;
            object c = 3;
            thisTest.Verify("a", a);
            thisTest.Verify("c", c);
        }

        [Test]
        public void T18_FunctionPointerAsReturnVal()
        {
            string code = @"
def foo:int(x:int)
{
	return = x;
}
def foo1:int(f : function, x:int)
{
	return = f(x);
}
def foo2:function()
{
	return = foo;
}
a = foo2();
b = a(2);
c = foo1(a, 3);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object b = 2;
            object c = 3;
            thisTest.Verify("b", b);
            thisTest.Verify("c", c);
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void T19_NegativeTest_PassingFunctionPtrAsArg_CSFFI()
        {
            string code = @"
import (""ProtoGeometry.dll"");
def foo : CoordinateSystem()
{
	return = CoordinateSystem.Identity();
}
a = Point.ByCartesianCoordinates(foo, 1.0, 2.0, 3.0);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object a = null;
            thisTest.Verify("a", a);
        }

        [Test]
        public void T20_FunctionPtrUpdateOnMemVar_1()
        {
            string code = @"
def foo1:int(x:int)
{
	return = x;
}
class A
{
	x:function;
	constructor A(xx:function)
	{
		x = xx;
	}
}
a = A.A(foo1);
b = a.x(3);    //b = 3";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object b = 3;
            thisTest.Verify("b", b);
        }

        [Test]
        [Category("Method Resolution")]
        [Category("Failure")]
        public void T21_FunctionPtrUpdateOnMemVar_2()
        {
            string err = "MAGN-4039 Update does not work well with function pointers";
            string code = @"
def foo1:int(x:int)
{
	return = x;
}
def foo2:double(x:int, y:double = 2.0)
{
	return = x + y;
}
class A
{
	x:var;
}
a = A.A();
a.x = foo1;
b = a.x(2); //b = 2;
a.x = foo2; //b = 4.0";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, err);
            object b = 4.0;
            thisTest.Verify("b", b);
        }

        [Test]
        public void T22_FunctionPointer_Update()
        {
            string code = @"
class A
{
x;
}
def foo(x)
{
    return = 2 * x;
}
def bar(x, f)
{
    return = f(x);
}
x = 100;
a = A.A();
a.x = x;
x = bar(x, foo);
t = a.x;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t", 200);
        }

        [Test]
        [Category("Failure")]
        public void T22_FunctionPointerArray()
        {
            string err = "MAGN-4040 Array indexing on array of function pointers causes crash";
            string code = @"
def foo(x)
{
    return = 2 * x;
}
fs = {foo, foo};
r = fs[0](100);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, err);
            thisTest.Verify("r", 200);
        }

        [Test]
        public void T23_FunctionPointerAsReturnValue()
        {
            string code = @"
def foo(x)
{
    return = 2 * x;
}
def bar(i)
{
    return = foo;
}
fs = bar(0..1);
f = fs[0];
r = f(100);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 200);
        }

        [Test]
        public void T24_FunctionPointerAsReturnValue2()
        {
            string code = @"
def foo(x)
{
    return = 2 * x;
}
def bar:function[]()
{
    return = {foo, foo};
}
fs = bar();
f = fs[0];
r = f(100);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 200);
        }

        [Test]
        public void T25_FunctionPointerTypeConversion()
        {
            string code = @"
def foo:int(x)
{
    return = 2 * x;
}
def bar:var[]()
{
    return = {foo, foo};
}
fs = bar();
f = fs[0];
r = f(100);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 200);
        }

        [Test]
        public void T26_NestedFunctionPointer()
        {
            string code = @"
def foo(x)
{
    return = 2 * x;
}
def bar(x)
{
    return = 3 * x;
}
def ding(x, f1:var, f2:var)
{
    return = f1(f2(x));
}
x = 1;
r = ding(x, foo, bar);
x = 2;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 12);
        }

        [Test]
        public void T27_FunctionPointerDefaultParameter()
        {
            string code = @"
def foo(x, y = 10, z = 100)
{
    return = x + y + z;
}
def bar(x, f)
{
    return = f(x);
}
r = bar(1, foo);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 111);
        }

        [Test]
        public void T28_FunctionPointerInInlineCond()
        {
            string code = @"
def foo(x, y = 10, z = 100)
{
    return = x + y + z;
}
def bar(x, y = 2, z = 3)
{
    return = x * y * z;
}
def ding(i, f, b)
{
    return = (i > 0) ? f(i) : b(i);
}
r1 = ding(1, foo, bar);
r2 = ding(-1, foo, bar);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 111);
            thisTest.Verify("r2", -6);
        }

        [Test]
        public void T29_FunctionPointerInInlineCond()
        {
            string code = @"
def foo(x, y = 10, z = 100)
{
    return = x + y + z;
}
def ding(i, f)
{
    return = (i > 0) ? f(i) : f;
}
r1 = ding(1, foo);
r2 = ding(-1, 100);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 111);
            thisTest.Verify("r2", 100);
        }

        [Test]
        public void T30_TypeConversion()
        {
            string code = @"
def foo()
{
    return = null;
}
t1:int = foo;
t2:int[] = foo;
t3:char = foo;
t4:string = foo;
t5:bool = foo; 
t6:function = foo;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t1", null);
            thisTest.Verify("t2", new object[] { null });
            thisTest.Verify("t3", null);
            thisTest.Verify("t4", null);
            thisTest.Verify("t5", null);
        }

        [Test]
        [Category("Failure")]
        public void T31_UsedAsMemberVariable()
        {
            string err = "MAGN-4039 Update does not work well with function pointers";
            string code = @"
class A
{
    f;
    x;
    constructor A(_x, _f)
    {
        x = _x;
        f = _f;
    }
    def update()
    {
        x = f(x);
        return = null;
    }
}
def foo(x)
{
    return = 2 * x;
}
def bar(x)
{
    return = 3 * x;
}
a = A.A(2, foo);
r = a.update1();
a.f = bar;
r = a.x;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, err);
            thisTest.Verify("r", 12);
        }

        [Test]
        public void T32_UseStaticMemberFunction()
        {
            string code = @"
class Foo
{
    static def foo(x:int, y:int)
    {
        return = x + y;
    }
}

f = Foo.foo;
r = f(3, 4);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 7);
        }

        [Test]
        public void T33_UseStaticMemberFunction()
        {
            string code = @"
class Foo
{
    static def foo(x:int, y:int)
    {
        return = x + y;
    }
}

f = Foo.foo;
r = f({3, 5}, {7, 9});
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", new object[] {10, 14});
        }

        [Test]
        public void T34_UseStaticMemberFunction()
        {
            string code = @"
class Foo 
{ 
    static def foo(x : int, y : int) 
    {
        return = x + y; 
    } 
} 
f = Foo.foo;
a = { 3, 5 };
b = { 7, 9 };
r = f(a<1>, b<2>);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", new object[] { new object[] {10, 12}, new object[] {12, 14} });
        }

        [Test]
        public void T35_UseConstructor()
        {
            string code = @"
class Foo 
{ 
    i;
    constructor Foo(x, y)
    {
        i = x + y;
    } 
} 
c = Foo.Foo;
f = c(2, 3);
r = f.i;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 5);
        }


        [Test]
        public void T36_UseConstructor()
        {
            string code = @"
class Foo 
{ 
    i;
    constructor Foo(x, y)
    {
        i = x + y;
    } 
} 
c = Foo.Foo;
f = c({3,5}, {7, 9});
r = f.i;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", new object[] {10, 14});
        }

        [Test]
        public void T37_FunctionPointerToProperty()
        {
            string code = @"
class Foo 
{ 
    X;
    constructor Foo(x)
    {
        X = x;
    } 
} 
c = Foo.Foo(42);
fx = Foo.X;
r = fx(c);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 42);
        }

        [Test]
        public void T38_FunctionPointerToProperty()
        {
            string code = @"
class Foo 
{ 
    X;
    constructor Foo(x)
    {
        X = x;
    } 
} 
cs = Foo.Foo(42..43);
fx = Foo.X;
r = fx(cs);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", new object[] {42, 43});
        }

        [Test]
        public void T39_FunctionPointerToMemberFunction()
        {
            string code = @"
class Foo 
{ 
    X;
    constructor Foo(x)
    {
        X = x;
    } 

    def foo(x)
    {
        return = X + x;
    }
} 
c = Foo.Foo(40);
fx = Foo.foo;
r = fx(c, 2);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 42);
        }

        [Test]
        public void T40_FunctionPointerToMemberFunction()
        {
            string code = @"
class Foo 
{ 
    X;
    constructor Foo(x)
    {
        X = x;
    } 

    def foo(x)
    {
        return = X + x;
    }
} 
cs = Foo.Foo({100, 200});
fx = Foo.foo;
vs = {42, 43};
r = fx(cs, vs);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", new object[] {142, 243});
        }

        [Test]
        public void T41_FunctionPointerToMemberFunction()
        {
            string code = @"
class Foo 
{ 
    X;
    constructor Foo(x)
    {
        X = x;
    } 

    def foo(x)
    {
        return = X + x;
    }
} 
cs = Foo.Foo({100, 200});
fx = Foo.foo;
vs = {42, 43};
r = fx(cs<1>, vs<2>);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", new object[] { new object[] {142, 143},
                                                new object[] {242, 243}});
        }

        [Test]
        public void T42_FunctionPointerToProperty()
        {
            string code = @"
class Foo 
{ 
    X;
    constructor Foo(x)
    {
        X = x;
    } 
} 
c = Foo.Foo(42);
fx = Foo.X;
r = Evaluate(fx, {c}, true);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 42);
        }

        [Test]
        public void T43_FunctionPointerToProperty()
        {
            string code = @"
class Foo 
{ 
    X;
    constructor Foo(x)
    {
        X = x;
    } 
} 
cs = Foo.Foo(42..43);
fx = Foo.X;
r = Evaluate(fx, {cs}, true);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", new object[] {42, 43});
        }

        [Test]
        public void T44_FunctionPointerToMemberFunction()
        {
            string code = @"
class Foo 
{ 
    X;
    constructor Foo(x)
    {
        X = x;
    } 

    def foo(x)
    {
        return = X + x;
    }
} 
c = Foo.Foo(42);
fx = Foo.foo;
r = Evaluate(fx, {c, 100}, true);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 142);
        }

        [Test]
        public void T45_FunctionPointerToMemberFunction()
        {
            string code = @"
class Foo 
{ 
    X;
    constructor Foo(x)
    {
        X = x;
    } 

    def foo(x)
    {
        return = X + x;
    }
} 
cs = Foo.Foo(42..43);
fx = Foo.foo;
r = Evaluate(fx, {cs, {100,200}}, true);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", new object[] {142, 243});
        }
    }
}
