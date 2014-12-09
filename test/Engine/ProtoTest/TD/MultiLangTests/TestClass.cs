using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;
namespace ProtoTest.TD.MultiLangTests
{
    class TestClass : ProtoTestBase
    {
        [Test]
        [Category("SmokeTest")]
        public void T01_Class_In_Various_Scopes()
        {
            string code = @"
class Dummy
{ 
	x : var;  
	constructor Dummy () 
	{	
		x = 2;	 
	}
}
obj1 = [Imperative]
{
	a = Dummy.Dummy();
	a1 = a.x;
	return = a;
}
getX1 = obj1.x;	
obj2 = [Associative]
{
	b = Dummy.Dummy();
	b1 = b.x;
	return = b1;
}
getX2 = obj2.x;	";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

        }

        [Test]
        [Category("SmokeTest")]
        public void T02_Class_In_Various_Nested_Scopes()
        {
            string code = @"
class Dummy
{ 
	x : var;  
	constructor Dummy () 
	{	
		x = 2;	 
	}
}
c1 = [Imperative]
{
	a = Dummy.Dummy();
	b = [Associative]
	{
	    return = Dummy.Dummy();
	}
	c = a.x + b.x;
	return = c;
}
c2 = [Associative]
{
	a = Dummy.Dummy();
	b = [Imperative]
	{
	    return = Dummy.Dummy();
	}
	c = a.x + b.x;
	return = c;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("SmokeTest")]
        public void T03_Class_In_Various_Scopes()
        {
            string code = @"
class Dummy
{ 
	x : var;  
	constructor Dummy () 
	{	
		x = 2;	 
	}
}
a = Dummy.Dummy();
c1 = [Imperative]
{
	b = [Associative]
	{
	    return = a;
	}
	c = a.x + b.x;
	return = c;
}
c2 = [Associative]
{
	b = [Imperative]
	{
	    return = a;
	}
	c = a.x + b.x;
	return = c;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("SmokeTest")]
        public void T04_Class_Properties()
        {
            Object[] t1 = new Object[] { 1, 2 };
            object t6 = null;
            string code = @"
class A
{ 
	x1 : var[];  
	x2 : int ;
	x3 : double;
	x4 : bool;
	x5 : B;
	constructor A () 
	{	
		x1  = { 1, 2 };  
		x2  = 1;
		x3  = 1.5;
		x4  = true;
		x5  = B.B(1);
	}
}
class B
{ 
	y : int;
	constructor B (i : int) 
	{	
		y = i;
	}
}
a = A.A();
t1 = a.x1;
t2 = a.x2;
t3 = a.x3;
t4 = a.x4;
t5 = a.x5;
t6 = t5[0].y;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t1", t1, 0);
            thisTest.Verify("t2", 1, 0);
            thisTest.Verify("t3", 1.5, 0);
            thisTest.Verify("t4", true, 0);
            thisTest.Verify("t6", t6, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T05_Class_Properties()
        {
            Object[] t1 = new Object[] { new Object[] { 1 }, new Object[] { 2 } };
            Object[] t2 = new Object[] { 1, 2 };
            Object[] t3 = new Object[] { 1.5, 2.5 };
            Object[] t4 = new Object[] { true, false };
            string code = @"
class A
{ 
	x1 : var[][];  
	x2 : int[] ;
	x3 : double[]..[];
	x4 : bool[];
	x5 : B[];
	constructor A () 
	{	
		x1  = { 1, 2 };  
		x2  = { 1, 2 };
		x3  = { 1.5, 2.5 };
		x4  = { true, false };
		x5  = { B.B(1), B.B(2) };
	}
}
class B
{ 
	y : int;
	constructor B (i : int) 
	{	
		y = i;
	}
}
a = A.A();
t1 = a.x1;
t2 = a.x2;
t3 = a.x3;
t4 = a.x4;
t5 = a.x5;
t6 = t5[0].y;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t1", t1, 0);
            thisTest.Verify("t2", t2, 0);
            thisTest.Verify("t3", t3, 0);
            thisTest.Verify("t4", t4, 0);
            thisTest.Verify("t6", 1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T06_Class_Properties()
        {
            string code = @"
class A
{ 
	x1 : int = 5;
	constructor A () 
	{	
		
	}
}
a = A.A();
t1 = a.x1;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t1", 5, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T07_Class_Properties()
        {
            string code = @"
class A
{ 
	static x1 : int;
	constructor A () 
	{	
		x1 = 5;
	}
}
a = A.A();
t1 = a.x1;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("SmokeTest")]
        public void T08_Class_Properties()
        {
            string code = @"
class A
{ 
	x1 : int ;
	x2 : double[][];
	
	constructor A () 
	{	
		x1  = { true, 2.5 };  
		x2 = B.B();		
	}
}
class B
{ 
	x1 : int ;
		
	constructor B () 
	{	
		x1 = 1;
	}
}
a = A.A();
t1 = a.x1;
t2 = a.x2[1].x1;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("SmokeTest")]
        public void T09_Class_Inheritance()
        {
            string code = @"
class B
{ 
	x3 : int ;
		
	constructor B () 
	{	
		x3 = 2;
	}
}
class A extends B
{ 
	x1 : int ;
	x2 : double;
	
	constructor A () : base.B ()
	{	
		x1 = 1; 
		x2 = 1.5;		
	}
}
a1 = A.A();
b1 = a1.x1;
b2 = a1.x2;
b3 = a1.x3;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b1", 1, 0);
            thisTest.Verify("b2", 1.5, 0);
            thisTest.Verify("b3", 2, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T10_Class_Inheritance()
        {
            string code = @"
class B
{ 
	x3 : int ;
		
	constructor () 
	{	
		x3 = 2;
	}
}
class A extends B
{ 
	x1 : int ;
	x2 : double;
	
	constructor () : base ()
	{	
		x1 = 1; 
		x2 = 1.5;		
	}
}
a1 = A();
b1 = a1.x1;
b2 = a1.x2;
b3 = a1.x3;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b1", 1);
            thisTest.Verify("b2", 1.5);
            thisTest.Verify("b3", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T11_Class_Inheritance()
        {
            string code = @"
class B
{ 
	x3 : int ;
		
	constructor B(a) 
	{	
		x3 = a;
	}
	def foo ( )
	{
		return = x3;
	}
	def foo ( a : int)
	{
		return = x3 + a;
	}
	
	def foo2 ( a : int)
	{
		return = x3 + a;
	}
}
class A extends B
{ 
	x1 : int ;
	x2 : double;
	
	constructor A(a1,a2,a3) : base.B(a3)
	{	
		x1 = a1; 
		x2 = a2;		
	}
	def foo ( )
	{
		return = {x1, x2};
	}
}
a1 = A.A( 1, 1.5, 0 );
b1 = a1.foo();
b2 = a1.foo2(1);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b1", new object[] { 1, 1.5 });
            thisTest.Verify("b2", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T12_Class_CheckPropertyWhenCreatedInImperativeCode()
        {
            String code =
            @"class Dummy
{
 x : var;
 
 constructor Dummy ()
 {
	x = 2;	
 }
}
obj = [Imperative]{
return = Dummy.Dummy();
}
getX = obj.x;;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification
            object getX = 2;
            thisTest.Verify("getX", getX, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T13_Class_Default_Constructors()
        {
            string code = @"
class A
{ 
	x : int = 5 + 4 ;
	y : var;
	z : bool;
	w : B;
	p : int [];
	
}
class B
{ 
	x : int  ;	
	
}
a1 = A.A();
x1 = a1.x;
y1 = a1.y;
z1 = a1.z;
w1 = a1.w;
p1 = a1.p;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("x1", 9, 0);
            thisTest.Verify("z1", false, 0);
            Object a = new object[] { };
            thisTest.Verify("p1", a);
            thisTest.Verify("y1", null);
            thisTest.Verify("w1", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void T14_Class_Named_Constructors()
        {
            string code = @"
class A
{ 
	x : int;
	
	constructor A ()
	{
		x = 1;		
	}
	
	constructor A (i)
	{
		x = i;		
	}
	
	constructor A1 (i)
	{
		x = i;		
	}
	
	
}
xx = [Imperative]
{
	a1 = A.A();
	a2 = A.A(2);
	a3 = A.A(3);
	return = { a1, a2, a3 };
}
x1 = xx[0].x;
x2 = xx[1].x;
x3 = xx[2].x;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x1", 1, 0);
            thisTest.Verify("x2", 2, 0);
            thisTest.Verify("x3", 3, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T15_Class_Constructor_Negative()
        {
            string code = @"
class A
{ 
	x : int[];
	
	constructor A (i:int[])
	{
		x = i;		
	}
	
	
}
xx = [Imperative]
{
	y = 1;
	a1 = A.A(y);
	return = a1;
}
x1 = xx.x;
x2 = 3;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x1", new object[]{1});
            thisTest.AssertPointer("xx");
            thisTest.Verify("x2", 3);
        }

        [Test] 
        [Category("SmokeTest")]
        public void T15_Class_Constructor_Negative_1467598()
        {
            string code = @"
class A
{ 
	x : int[];
	
	constructor A (i:int[])
	{
		x = i;		
	}
	
	
}
xx = [Imperative]
{
	y = 1;
	a1 = A.A(y);
	return = a1;
}
x1 = xx.x;
x2 = 3;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x1", new object[]{1});
            thisTest.AssertPointer("xx");
            thisTest.Verify("x2", 3);
        }

        [Test]
        [Category("SmokeTest")]
        public void T16_Class_Constructor_Negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
class A
{ 
	x : int;
	
	constructor A (i)
	{
		x = i;		
		return = { x, i};
	}
	
	
}
a1 = A.A(1);
";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T17_Class_Constructor_Negative()
        {
            string code = @"
class A
{ 
	x : int;
	
	constructor A (i)
	{
		x = i;				
	}
	
	
}
class B
{ 
	x : int;
	
	constructor B (i)
	{
		x = i;		
		
	}
	
}
b1 = B.B(1);
a1 = A.A(b1);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("SmokeTest")]
        public void T18_Class_Constructor_Empty()
        {
            string src = @"class A
{ 
	x : int[] = {1,2};
	
	constructor A ()
	{
		y = 2;		
	}	
}
a1 = A.A();
x = a1.x;
x1 = x[0];
x2 = x[1];
x3 = a1.y;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);

            thisTest.Verify("x1", 1, 0);
            thisTest.Verify("x2", 2, 0);
            Assert.IsTrue(mirror.GetValue("x3").DsasmValue.IsNull);
        }

        [Test]
        [Category("SmokeTest")]
        public void T19_Class_Constructor_Test_Default_Property_Values()
        {
            string code = @"
class A
{ 
	x : var ;
	y : int;
	z : bool;
	u : double;
	v : B;
	w1 : int[];
	w2 : double[];
	w3 : bool[];
	w4 : B[][];
	
	constructor A ()
	{
		      	
	}	
}
a1 = A.A();
x1 = a1.x;
x2 = a1.y;
x3 = a1.z;
x4 = a1.u;
x5 = a1.v;
x6 = a1.w1;
x7 = a1.w2;
x8 = a1.w3;
x9 = a1.w4;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x1", null);
            thisTest.Verify("x2", 0);
            thisTest.Verify("x3", false);
            thisTest.Verify("x4", 0.0);
            thisTest.Verify("x5", null);
            Object a = new object[] { };
            thisTest.Verify("x6", a);
            thisTest.Verify("x7", a);
            thisTest.Verify("x8", a);
            thisTest.Verify("x9", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void T20_Class_Constructor_Fails()
        {
            string src = @"class A
{ 
	x : var ;	
	
	constructor A ()
	{
		 x = w;     	
	}	
}
a1 = A.A();
b1 = a1.x;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kIdUnboundIdentifier);
            Assert.IsTrue(mirror.GetValue("b1").DsasmValue.IsNull);
        }

        [Test]
        [Category("SmokeTest")]
        public void T21_Class_Constructor_Calling_Base_Constructor()
        {
            string code = @"
class A
{ 
	x : var ;	
	
	constructor A ()
	{
		 x = 1;     	
	}	
}
class B extends A
{ 
	y : var ;	
	
	constructor B () : base.A()
	{
		 y = 2;     	
	}	
}
class C extends B
{ 
	z : var ;	
	
	constructor C () : base.B()
	{
		 z = 3;     	
	}	
}
c = C.C();
c1 = c.x;
c2 = c.y;
c3 = c.z;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c1", 1, 0);
            thisTest.Verify("c2", 2, 0);
            thisTest.Verify("c3", 3, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T22_Class_Constructor_Not_Calling_Base_Constructor()
        {
            string code = @"
class A
{ 
	x : var = 0 ;	
	
	constructor A ()
	{
		 x = 1;     	
	}	
}
class C extends A
{ 
	y : var ;	
	
	constructor C () 
	{
		 y = 2;
         x = 2;		 
	}	
}
c = C.C();
c1 = c.x;
c2 = c.y;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c1", 2, 0);
            thisTest.Verify("c2", 2, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T23_Class_Constructor_Base_Constructor_Same_Name()
        {
            string code = @"
class A
{ 
	x : var = 0 ;	
	
	constructor A ()
	{
		 x = 1;     	
	}	
}
class C extends A
{ 
	y : var ;	
	
	constructor A () : base.A()
	{
		 y = 2;
         x = 2;		 
	}
	
}
class B extends A
{ 
	y : var ;	
	
	constructor A () : base.A()
	{
		 y = 2;
         
	}
	
}
class D extends A
{ 
	y : var ;	
	
	constructor A () : base.A() 
	{
		 y = 2;
         
	}
	
}
c = C.A();
c1 = c.x;
c2 = c.y;
b = B.A();
b1 = b.x;
b2 = b.y;
d = D.A();
d1 = d.x;
d2 = d.y;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c1", 2, 0);
            thisTest.Verify("c2", 2, 0);
            thisTest.Verify("b1", 1, 0);
            thisTest.Verify("b2", 2, 0);
            thisTest.Verify("d1", 1, 0);
            thisTest.Verify("d2", 2, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T24_Class_Constructor_Calling_Base_Methods()
        {
            string code = @"
class A
{ 
	x : var = 0 ;	
	
	constructor A ()
	{
		 x = 1;     	
	}
	def foo ()
	{
	    return = x + 1;
	}	
}
class C extends A
{ 
	y : var ;	
	
	constructor C () : base.A()
	{
		 y = foo();
         	 
	}
	
}
c = C.C();
c1 = c.x;
c2 = c.y;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c1", 1, 0);
            thisTest.Verify("c2", 2, 0);

        }

        [Test]
        [Category("SmokeTest")]
        public void T25_Class_Properties_Modifiers()
        {
            // Assert.Fail("1467247 - Sprint25: rev 3448 : REGRESSION : Runtime exception thrown when setting a private property of a class instance");
            string code = @"
class A
{ 
	public x : var ;	
	private y : var ;
	//protected z : var = 0 ;
	constructor A ()
	{
		   	
	}
	public def foo1 (a)
	{
	    x = a;
		y = a + 1;
		return = x + y;
	} 
	private def foo2 (a)
	{
	    x = a;
		y = a + 1;
		return = x + y;
	}	
}
a = A.A();
a1 = a.foo1(1);
a2 = a.foo2(1);
a.x = 4;
a.y = 5;
a3 = a.x;
a4 = a.y;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a1", 3, 0);
            thisTest.Verify("a3", 4, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T26_Class_Properties_Access()
        {
            string code = @"
class A
{ 
	public x : var ;	
	private y : var ;
	//protected z : var = 0 ;
	constructor A ()
	{
		   	
	}
	public def foo1 (a) 
	{
	    x = a;
		return = x + a;
	} 
	private def foo2 (a)
	{
	    x = a;
		y = a + 1;
		return = x + y;
	}	
	public def foo3 (a)
	{
	    x = a;
		return = x + foo2(a);
	}
}
a11;a12;a2;a3;a4;a5;
[Imperative]
{
    a2 = [Associative]
	{
	    a1 = [Imperative]
		{
		    a = A.A();
			return = a;
		}
		a11 = a1.foo1(1);
		a12 = a1.x;
		return = a11+a12;
	}
	ax = A.A();
	a3 = ax.foo1(1);
	a4 = ax.foo3(1);
	a5 = ax.x;
	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("a11", 2);
            thisTest.Verify("a12", 1);
            thisTest.Verify("a2", 3);
            thisTest.Verify("a3", 2);
            thisTest.Verify("a4", 4);
            thisTest.Verify("a5", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T27_Class_Properties_Access()
        {
            string code = @"
class A
{ 
	public x : var ;	
	private y : var ;
	//protected z : var = 0 ;
	constructor A (i)
	{
		x = i;
        y = i;		
	}
	public def foo1 (a) 
	{
	    x = a;
		return = x + a;
	} 
	private def foo2 (a)
	{
	    x = a;
		y = a + 1;
		return = x + y;
	}	
	public def foo3 (a)
	{
	    x = a;
		return = x + foo2(a);
	}
}
aa;a2;
[Imperative]
{
    aa = 0;
	a2 = [Associative]
	{
	    a1 = [Imperative]
		{
		    x = { 1, 2};
			add = 0;
			for ( i in x )
			{
			    ax = A.A(i); 
                add = add + ax.foo1(1) + ax.foo3(1);				
			}
			return = add;
		}
		a2 = A.A(3);
		return = a1 +a2.x;
	}
	if(a2 > 0 )
	{
	    x2 = A.A(2);
		aa = x2.foo3(2);
	}
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aa", 7);
            thisTest.Verify("a2", 15);
        }

        [Test]
        [Category("Method Resolution")]
        public void T28_Class_Properties_Access()
        {
            // Assert.Fail("1467080 - sprint23 : rev 2681 : method overload issue over functions with same name and signature in multiple language blocks");

            string code = @"
class A
{ 
	public x : var ;	
	private y : var ;
	constructor A (i)
	{
		x = i;
        y = i;		
	}
 
	private def foo2 (a)
	{
	    x = a;
		y = a + 1;
		return = x + y;
	}	
	public def foo3 (a)
	{
	    x = a;
		return = x + foo2(a);
	}
}
class B
{ 
	public x : var ;	
	public y : A ;
	constructor B (i)
	{
		x = i;
        y = A.A(i);		
	}
 
	public def foo3 (a)
	{
	    x = a;
		y = A.A(a);
		return = x + y.x;
	}	
	
}
def foo (a1 : A, b1 :B )
{
    x = a1.x + b1.x;
	return = x;
}

f1 = null;
v1 = [Imperative]
{
    def foo2 (a1 : A, b1 :B )
	{
		x = a1.x + b1.x;
		y = a1.foo3(1) + b1.foo3(1);
		return = x + y;
	}
    a1 = A.A(1);
	b1 = B.B(1);
	
	f1 = foo2(a1,b1);
	x = { 1, 2 };
	add = 0;
	for ( i in x )
	{
	    ax = A.A(i); 
        bx = B.B(i);
		add = add + foo2(ax, bx);				
	}
	if(add > 0 )
	{
	    add = add + foo2(a1,b1);		
	}
	return = add;
}
a2 = A.A(2);
b2 = B.B(2);
f2 = foo(a2,b2);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("f1", 8);
            thisTest.Verify("v1", 26);
            thisTest.Verify("f2", 4);
        }

        [Test]
        public void T29_Class_Method_Chaining()
        {
            //Assert.Fail("1454966 - Sprint15: Rev 720 : [Geometry Porting] Assignment operation where the right had side is Class.Constructor.Property is not working ");

            string code = @"
class A
{ 
	x : int[];	
	
	constructor A ( i :int[])
	{
		x = i;
       	
	}
 
	public def foo ()
	{
	    return = x;
	}
}
class B
{ 
	public x : A ;	
	
	constructor B (i:A)
	{
		x = i;
       
	}
 
	public def foo ()
	{
	    return = x;
	}	
	
}
x = { 1, 2, 3 };
y = { 4, 5, 6 };
a1 = A.A(x);
a2 = A.A(y);
b1 = B.B({a1,a2});
t1 = b1[0].x.x[1];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("t1", 2, 0);

        }



        [Test]
        [Category("SmokeTest")]
        public void T30_Class_Property_Update_Negative()
        {
            string code = @"
class A
{ 
	x : int[];		
	constructor A ( i :int[])
	{
		x = i;       	
	} 
	public def foo ()
	{
	    x = true;
		return = x;
	}
}
x = { 1, 2, 3 };
a1 = A.A(x);
x1 = a1.x;
x2 = a1.foo();
x3 = 3;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            TestFrameWork.Verify(mirror, "x1", null);
            TestFrameWork.Verify(mirror, "x2", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void T31_Class_By_Composition()
        {
            string code = @"
class Point
{
    X : double;
	Y : double;
	Z : double;
	
	constructor ByCoordinates( x : double, y : double, z : double )
	{
	    X = x;
		Y = y;
		Z = z;		
	}
}
class Line
{
    P1 : Point;
	P2 : Point;
	
	constructor ByStartPointEndPoint( p1 : Point, p2 : Point )
	{
	    P1 = p1;
		P2 = p2;
	}
	
	def PointAtParameter (p : double )
	{
	
	    t1 = P1.X + ( p * (P2.X - P1.X) );
		//t2 = 
		return = Point.ByCoordinates( t1, P1.Y, P1.Z);
	    
	}
	
}
class MyLineByComposition 
{
	BaseLine : Line; // line property
	MidPoint : Point; // midPoint property
	
	public constructor ByPoints(start : Point, end : Point)
	{
		BaseLine = Line.ByStartPointEndPoint(start, end);
		MidPoint = BaseLine.PointAtParameter(0.5);
	}
}
p1 = Point.ByCoordinates( 5.0, 0.0, 0.0 );
p2 = Point.ByCoordinates( 10.0, 0.0, 0.0 );
myLineInstance = MyLineByComposition.ByPoints(p1, p2);
testP = myLineInstance.MidPoint;
x1 = testP.X;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x1", 7.5, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T32_Class_ReDeclaration()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
class Point
{
    X : double;
	Y : double;
	Z : double;
	
	constructor ByCoordinates( x : double, y : double, z : double )
	{
	    X = x;
		Y = y;
		Z = z;		
	}
}
class Line
{
    P1 : Point;
	P2 : Point;
	
	constructor ByStartPointEndPoint( p1 : Point, p2 : Point )
	{
	    P1 = p1;
		P2 = p2;
	}
	
	def PointAtParameter (p : double )
	{
	
	    t1 = P1.X + ( p * (P2.X - P1.X) );
		//t2 = 
		return = Point.ByCoordinates( t1, P1.Y, P1.Z);
	    
	}
	
}
class Point
{
    X : double;
	
	
	constructor ByCoordinates( x : double )
	{
	    X = x;
			
	}
}
class MyLineByComposition 
{
	BaseLine : Line; // line property
	MidPoint : Point; // midPoint property
	
	public constructor ByPoints(start : Point, end : Point)
	{
		BaseLine = Line.ByStartPointEndPoint(start, end);
		MidPoint = BaseLine.PointAtParameter(0.5);
	}
}
p1 = Point.ByCoordinates( 5.0, 0.0, 0.0 );
p2 = Point.ByCoordinates( 10.0, 0.0, 0.0 );
myLineInstance = MyLineByComposition.ByPoints(p1, p2);
testP = myLineInstance.MidPoint;
x1 = testP.X;
";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });

        }

        [Test]
        [Category("SmokeTest")]
        public void T33_Class_Methods()
        {
            string src = @"class Point
{
    X : double;
	Y : double;
	Z : double;
	
	constructor ByCoordinates( x : double, y : double, z : double )
	{
	    X = x;
		Y = y;
		Z = z;		
	}
	
	def addP1( x : int )
	{
	    X = x;
		return = X;
		
	}
	
	def addP2( x : int )
	{
	    W = x;
		return  = W;
		
	}
}
p1 = Point.ByCoordinates( 5.0, 0.0, 0.0 );
x1 = p1.X;
x2 = p1.x;
x3 = p1.addP1(1);
x4 = p1.addP2(1);
x5 = p1.W;
x6 = p1.X;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue(mirror.GetValue("x2").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("x5").DsasmValue.IsNull);
            thisTest.Verify("x1", 1.0, 0);
            thisTest.Verify("x3", 1.0, 0);
            thisTest.Verify("x4", 1, 0);
            thisTest.Verify("x6", 1.0, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T34_Class_Static_Methods()
        {
            string code = @"
class Point
{
    static X : double;
	Y : double;
	constructor ByCoordinates( x : double, y: double )
	{
	    X = x;
		Y = y;
			
	}
	
	static def add1( )
	{
	    X = X + 1;
		Y = Y + 1;
		return = X + Y;
		
	}
	
	def add2( )
	{
	    X = X + 1;
		Y = Y + 1;
		return = X + Y;		
	}
}
p1 = Point.ByCoordinates( 5.0, 0.0);
x1 = p1.X;
x2 = p1.Y;
x3 = p1.add1();
x4 = p1.add2();
x5 = 1;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object v = null;
            thisTest.Verify("x3", v);
            thisTest.Verify("x4", 8.0);
            thisTest.Verify("x5", 1);

        }

        [Test]
        [Category("SmokeTest")]
        public void T35_Class_Method_Overloading()
        {
            string code = @"
class Point
{
    X : double;
	Y : double;
	constructor ByCoordinates( x : double, y: double )
	{
	    X = x;
		Y = y;
			
	}
	
	def add1( )
	{
	    X = X + 1;
		Y = Y + 1;
		return = X + Y;
		
	}
	
}
class Point2 extends Point
{
    a : double;
	b : double;
	constructor ByCoordinates( x : double, y: double )
	{
	    X = x;
		Y = y;
		a = X;
		b = Y;			
	}
	
	def add1( )
	{	    
		return = X + Y;		
	}
	
}
p1 = Point.ByCoordinates( 5.0, 10.0);
p2 = Point2.ByCoordinates( 15.0, 20.0);
a1 = p1.add1();
a2 = p2.add1();
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a1", 17.0, 0);
            thisTest.Verify("a2", 35.0, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T36_Class_Method_Calling_Constructor()
        {
            string code = @"
class Point
{
    X : double;
	Y : double;
	constructor ByCoordinates( x : double, y: double )
	{
	    X = x;
		Y = y;
			
	}
	
	def create( )
	{
	    X = X + 1;
		Y = Y + 1;
		return = Point.ByCoordinates( X, Y );
		
	}
	
}
class Point2 extends Point
{
    a : double;
	b : double;
	constructor ByCoordinates( x : double, y: double )
	{
	    X = x;
		Y = y;
		a = X;
		b = Y;			
	}
	
	def create( )
	{
	    X = X + X;
		b = b + b;
		return = Point2.ByCoordinates( X, b );
		
	}
	
}
p1 = Point.ByCoordinates( 5.0, 10.0);
p2 = Point2.ByCoordinates( 15.0, 20.0);
a1 = p1.create();
a2 = a1.X;
a3 = a1.Y;
b1 = p2.create();
b2 = b1.X;
b3 = b1.Y;
b4 = b1.a;
b5 = b1.b;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a2", 6.0, 0);
            thisTest.Verify("a3", 11.0, 0);
            thisTest.Verify("b2", 30.0, 0);
            thisTest.Verify("b3", 40.0, 0);
            thisTest.Verify("b4", 30.0, 0);
            thisTest.Verify("b5", 40.0, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T37_Class_Method_Using_This()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
class Point
{
    X : double;
	Y : double;
	constructor ByCoordinates( x : double, y: double )
	{
	    X = x;
		Y = y;
			
	}
	
	def add(this : Point, d : double )
	{
	  	this.X = this.X + d;
		this.Y = this.Y + d;
		return = this.X + this.Y;
		
	}
	
}
p1 = Point.ByCoordinates( 5.0, 10.0);
a1 = p1.add(4.0);
a2 = p1.X;
a3 = p1.Y;
";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
                thisTest.Verify("a2", 5.0, 0);
                thisTest.Verify("a3", 10.0, 0);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T38_Class_Method_Using_This()
        {
            string code = @"
class Point
{
    X : double;
	Y : double;
	constructor ByCoordinates( x : double, y: double )
	{
	    X = x;
		Y = y;
			
	}
	
	def add( d : double )
	{
	  	this.X = this.X + d;
		this.Y = this.Y + d;
		return = this.X + this.Y;
		
	}
	
}
p1 = Point.ByCoordinates( 5.0, 10.0);
a1 = p1.add(4.0);
a2 = p1.X;
a3 = p1.Y;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a2", 9.0, 0);
            thisTest.Verify("a3", 14.0, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T39_Class_Method_Returning_Collection()
        {
            string code = @"
class Point
{
    X : double;
	Y : double;
	constructor ByCoordinates( x : double, y: double )
	{
	    X = x;
		Y = y;
			
	}
	
	def add( d : double )
	{
	  	return = { X+d, Y+d };
		
	}
	
}
p1 = Point.ByCoordinates( 5.0, 10.0);
a1 = p1.add(4.0);
a2 = a1[0];
a3 = a1[1];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a2", 9.0, 0);
            thisTest.Verify("a3", 14.0, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T40_Class_Property_Initialization_With_Another_Class()
        {
            string code = @"
class Point
{
    X : double;
		
	public constructor ByCoordinates( xValue : double  )
    {
		X = xValue; 			
	}
}
class MyPoint 
{
	inner  : Point = Point.ByCoordinates(3);	
}
p1 = MyPoint.MyPoint();
t1 = p1.inner.X;
	
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t1", 3.0, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T41_Test_Static_Properties()
        {
            string code = @"
class A
{
    static x:int;
}
a = A.A();
a.x = 3;
A.x = 3;
t1 = a.x;
t2 = A.x;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t1", 3, 0);
            thisTest.Verify("t2", 3, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T41_Test_Static_Properties_2()
        {
            string code = @"
class A
{
    static x:int = 3;
	
}
a = A.A();
b = [Imperative]
{
	a.x = 4;
	A.x = 4;
	t1 = a.x;
	t2 = A.x;
	return = { t1, t2 };
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 4, 4 };
            thisTest.Verify("b", v1, 0);
        }


        [Test]
        [Category("SmokeTest")]
        public void T42_Defect_1461717()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
             {
                 string code = @"
class A
{
    a : var;
	constructor A ()
	{
	    a = 1;
	}
}
class B extends A
{
    b : var;
	constructor B ()
	{
	    b = 1;
	}
}
A = B.B();
";
                 ExecutionMirror mirror = thisTest.RunScriptSource(code);
             });
        }


        [Test]
        [Category("SmokeTest")]
        public void T43_Defect_1461479()
        {
            string code = @"
class A
{
    static x:int=1;	
}
x2 = A.x;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x2", 1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T43_Defect_1461479_2()
        {
            string code = @"
class A
{ 
	static x1 : int;
	constructor A () 
	{	
		x1 = 5;
	}
}
a = A.A();
t1 = a.x1;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t1", 5, 0);
        }

        [Test]
        [Category("Update")]
        [Category("Failure")]
        public void T43_Defect_1461479_3()
        {
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3949

            string code = @"
class A
{
    static x:int=1;	
	def foo1 ()
	{
	    x = 6;
		return = x;
	}
}
class B extends A
{
    static y:int=1;
    constructor B ( )
    {
	    y = 4;
    } 
    def foo2 ()
	{
	    x = 7;
		y = 8;
		return = { x, y } ;
	}	
}
x2 = A.x;
x3 = B.B();
t1 = x3.y;
t2 = x3.x;
x3.y = 2;
x3.x = 3;
t3 = x3.y;
t4 = x3.x;
t5 = x3.foo1();
t6 = x3.foo2();
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            Object[] v1 = new Object[] { 7, 8 };

            thisTest.Verify("x2", 7, 0);
            thisTest.Verify("t2", 7, 0);
            thisTest.Verify("t3", 8, 0);
            thisTest.Verify("t4", 7, 0);
            thisTest.Verify("t5", 6, 0);
            thisTest.Verify("t6", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T43_Defect_1461479_4()
        {
            string code = @"
class A
{
    static x:int=1;
    def foo ()
    {
	    x = 4;
		return = x;
    }	
	
}
def foo2( a : int)
{
    A.x = 3;
	x = A.x + a;
	return = x;
}
b = foo2( 1 ) ;
a = A.A();
c = a.x;
d = a.foo();
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("b", 4, 0);
            thisTest.Verify("c", 4, 0);
            thisTest.Verify("d", 4, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T44_Defect_1461860()
        {
            string code = @"
class A
{
    static x:int = 3;
	
}
b;
[Imperative]
{
	a = A.A();
	b = [Associative]
	{
	    a.x = 4;
		return = a.x;
	}
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", 4);
        }

        [Test]
        [Category("SmokeTest")]
        public void T44_Defect_1461860_2()
        {
            string code = @"
class A
{
    static x:int = 3;
	
}
a1 = A.A();
a2 = A.A();
y = [Imperative]
{
	x = { a1, a2 };
	count = 0;
	
	for ( i in x )
	{
	    i.x = count;
        count = count + 1;		
	}
	
	return = x;
}
c = y.x;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 1, 1 };
            thisTest.Verify("c", v1);
        }


        [Test]
        [Category("SmokeTest")]
        public void T46_Defect_1461716()
        {
            string code = @"
class A
{
    a : int;	
	constructor A ()
	{
	    a = 1;
	}	
}
a1 = A.A();
b1 = a1.A();
c1 = b1.a;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object v1 = null;

            thisTest.Verify("b1", v1);
            thisTest.Verify("c1", v1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T46_Defect_1461716_2()
        {
            string code = @"
class A
{
    a : int;	
	constructor A ()
	{
	    a = 1;
	}	
}
def foo ( )
{
    a1 = A.A();
	b1 = a1.A();
	c1 = b1.a;
	return = { b1, c1 };
}
x = foo ();
b1;c1;
[Imperative]
{
	a1 = A.A();
	b1 = a1.A();
	c1 = b1.a;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object v1 = null;
            Object[] v2 = new Object[] { null, null };
            thisTest.Verify("b1", v1);
            thisTest.Verify("c1", v1);
            thisTest.Verify("x", v2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T47_Calling_Imperative_Code_From_Conctructor()
        {
            string code = @"
class A
{
	a : int;
	
	constructor A( i:int )
	{
		[Imperative]
		{
		    a = i;
		}
	}
	
}
A1 = A.A( 1 );
a1 = A1.a;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1463372 - Sprint 20 : Rev 2088 : Imperative code is not allowed in class constructor ");
            thisTest.Verify("a1", 1);

        }

        [Test]
        [Category("SmokeTest")]
        public void T48_Defect_1460027()
        {
            string code = @"
class A
{
	a : int[][];
	
	constructor create(i:int)
	{
		
				a = { { 1,2,3 } , { 4,5,6 } };
	//a=1;
	
	}
	
}
A1 = A.create(1);
a1 = A1.a;
b1=a1[1][0];";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b1", 4);
        }

        [Test]
        [Category("SmokeTest")]
        public void T48_Defect_1460027_2()
        {
            string code = @"
class A
{
	a : var[]..[];
	
	constructor create(i:int)
	{
		
				a = { { 1,2,3 } , { 4,5,6 } };
	
	}
	
}
A1 = A.create(1);
a1 = A1.a;
b1=a1[1][0];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b1", 4);
        }

        [Test]
        [Category("SmokeTest")]
        public void T48_Defect_1460027_3()
        {
            //Assert.Fail("1463700 - Sprint 20 : rev 2147 : Update is not happening when a collection property of a class instance is updated using a class method ");
            string code = @"
class A
{
	a : var[]..[];
	
	constructor create()
	{
		
				a = { { 1,2,3 } , { 4,5,6 } };
	
	}
	
}
	test1 = A.create();
	
	a1 = test1.a;
	test1.a[0] = { 4,5,6 };
	b = test1.a[0];
	c = test1.a[0][0];";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] expected = new Object[] { 4, 5, 6 };
            thisTest.Verify("b", expected, 0);
            thisTest.Verify("c", 4, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T48_Defect_1460027_4()
        {
            string code = @"
class A
{
	a : var[]..[];
	
	constructor create()
	{
		
				a = { { 1,2,3 } , { 4,5,6 } };
	
	}
	def foo ( x )
	{
		a[1][2] = x;
		return = a;
	}
}
	
	test1 = A.create();
	a1 = test1.a;
	b = test1.foo( a1[0][1] );
	
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.RuntimeData.WarningID.kCyclicDependency);
        }

        [Test]
        [Category("SmokeTest")]
        public void T49_Defect_1460505()
        {
            string code = @"
class Parent
{
A : var;
B : var;
C : var;
constructor Create( x:int, y:int, z:int )
{
A = x;	
B = y;
C = z;
}
}
class Child extends Parent
{
constructor Create( x:int, y:int, z:int )
{
A = x;	
B = y;
C = z;
}
}
def modify(oldPoint : Parent)
{
return=true;
}
oldPoint = Parent.Create( 1, 2, 3 );
derivedpoint = Child.Create( 7,8,9 );
basePoint = modify( oldPoint );
derivedPoint2 = modify( derivedpoint );";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("basePoint", true, 0);
            thisTest.Verify("derivedPoint2", true, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T49_Defect_1460505_2()
        {
            string code = @"
class Parent
{
A : var;
B : var;
C : var;
constructor Create( x:int, y:int, z:int )
{
A = x;	
B = y;
C = z;
}
}
class Child extends Parent
{
constructor Create( x:int, y:int, z:int )
{
A = x;	
B = y;
C = z;
}
}
def modify(oldPoint : Child)
{
return=true;
}
oldPoint = Parent.Create( 1, 2, 3 );
derivedpoint = Child.Create( 7,8,9 );
basePoint = modify( oldPoint );
derivedPoint2 = modify( derivedpoint );";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object n1 = null;
            thisTest.Verify("basePoint", n1, 0);
            thisTest.Verify("derivedPoint2", true, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T50_Defect_1460510()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
                {
                    string code = @"
class TestPoint
{
            A : var;
                                
       constructor Create()
        {
	    A = 10; 
        }
	
}
class derived extends TestPoint
{
	    A : var;
     constructor Create()
        {
	    A = 20; 
     }
	def Modify()
		
	{
	   A = A +1;
	    return=true;
	}   
}
	oldPoint = TestPoint.Create();
    derivedpoint=derived.Create();
	derivedPoint2 = derivedpoint.Modify();
	xPoint1 = oldPoint.A;
    xPoint2 = derivedpoint.A;";
                    ExecutionMirror mirror = thisTest.RunScriptSource(code);
                });
        }

        [Test]
        [Category("SmokeTest")]
        public void T50_Defect_1460510_2()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
class Base
{
	A : int;
	constructor Create()
	{
		A = 5;
	}
}
class Derived extends Base
{
	A : var;
	constructor Create1()
	{
		A = 10;
	}
}
	def Modify( object : Base )
	{
		object.A = object.A + 1;
		return = object.A;
	}
	B = Base.Create();
	D = Derived.Create1();
	a = Modify( B );
	";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T51_Defect_1461399()
        {
            string code = @"
class Arc
{
constructor Arc()
{
}
def get_StartPoint()
{
	return = 1;
}
}
def CurveProperties(curve : Arc)
{
 return = {
	curve.get_StartPoint(),
	curve.get_StartPoint(),
	curve.get_StartPoint()
	
 };
}
test = CurveProperties(null);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] ExpectedResult = new Object[] { null, null, null };
            thisTest.Verify("test", ExpectedResult);
        }

        [Test]
        [Category("SmokeTest")]
        public void T51_Defect_1461399_2()
        {
            string code = @"
class Arc
{
    constructor Arc()
    {
        
    }
    def get_StartPoint()
    {
        return = 1;
    }
    
    
    def CurveProperties(curve : Arc)
    {
        
        return =
        {
            curve.get_StartPoint(),
            curve.get_StartPoint(),
            curve.get_StartPoint()
        };
        
    }
} 
   
	Arc1 = Arc.Arc();
	test = Arc1.CurveProperties(null);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] ExpectedResult = new Object[] { null, null, null };
            thisTest.Verify("test", ExpectedResult);
        }

        [Test]
        [Category("SmokeTest")]
        public void T51_Defect_1461399_3()
        {
            string code = @"
class Arc
{
    constructor Arc()
    {
        
    }
    def get_StartPoint()
    {
        return = 1;
    }
    
    
    def CurveProperties(curve : Arc)
    {
        
        return =
        {
            curve.get_StartPoint(),
            curve.get_StartPoint(),
            curve.get_StartPoint()
        };
        
    }
} 
test;
[Imperative]
{   
	Arc1 = Arc.Arc();
	test = Arc1.CurveProperties(null);
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] ExpectedResult = new Object[] { null, null, null };
            thisTest.Verify("test", ExpectedResult);
        }


        [Test]
        [Category("SmokeTest")]
        public void T52_Defect_1461479()
        {
            string code = @"
class A
{
	x : int = 2;
	
	def foo : int()
	{
		return = 2;
	}
}
a = A.foo(); 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue(mirror.GetValue("a", 0).DsasmValue.IsNull);
        }

        [Test]
        [Category("SmokeTest")]
        public void T52_Defect_1461479_2()
        {
            string code = @"
class Sample
{
	a : var = 2;
	
	static def ret_a ()
	{
		return = a;
	}
}
test1 = Sample.ret_a(); 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue(mirror.GetValue("test1", 0).DsasmValue.IsNull);
        }

        [Test]
        [Category("SmokeTest")]
        public void T52_Defect_1461479_3()
        {
            string code = @"
class Sample
{
	static a : var;
	constructor Sample()
	{
		a = 3;
	}
	
	static def ret_a ( b )
	{
		a = b;
		return = a;
	}
}
	Sample1 = Sample.Sample();
	
	test1 = Sample1.ret_a( 2 ); 
	test2 = Sample1.a;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("test1", 2, 0);
            thisTest.Verify("test2", 2, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T52_Defect_1461479_4()
        {
            // Note now is valid to return non-static member as function 
            // pointer

            // Assert.Fail("1463741 Sprint 20 : Rev 2150 : Error: Non-Static members are accessible via static calls");
            string code = @"
class Sample
{
	a : var = 2;
	
	def ret_a ()
	{
		return = B.a;
	}
	
}
class B
{
   a : int;
}
test1;test2;test5;
[Imperative]
{
	S = Sample.Sample();
	test1 = Sample.ret_a();
	test5 = S.ret_a();
}
S2 = Sample.Sample();
test3 = Sample.ret_a();
test6 = S2.ret_a();";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object v1 = null;
        }

        [Test]
        [Category("SmokeTest")]
        public void T52_Defect_1461479_5()
        {
            string code = @"
class Sample
{
	a : var = 2;
	
	def ret_a ()
	{
		return = a;
	}
}
test3 = Sample.ret_a();
test4 = Sample.a;
def fun1 ()
{
    return = { Sample.ret_a()};
}
def fun2 ()
{
    return = [Imperative]
	{
	    return = { Sample.ret_a()};
	}
}
test5 = fun1();
test6 = fun2();
test1;test2;test7;test8;
[Imperative]
{
	test1 = Sample.ret_a();
	test7 = fun1();
    test8 = fun2();
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object v1 = null;
            Object[] v2 = new Object[] { null };
            thisTest.Verify("test1", v1, 0);
            thisTest.Verify("test3", v1, 0);
            thisTest.Verify("test5", v2, 0);
            thisTest.Verify("test6", v2, 0);
            thisTest.Verify("test7", v2, 0);
            thisTest.Verify("test8", v2, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T53_Undefined_Class_As_Parameter()
        {
            string code = @"
class Parent
{
	A : var;
	B : var;
	C : var;
	constructor Create( x:int, y:int, z:int )
	{
		A = x;	
		B = y;
		C = z;
	}
}
class Child extends Parent
{
	constructor Create( x:int, y:int, z:int )
	{
		A = x;	
		B = y;
		C = z;
	}
}
def modify(oldPoint1 : TestPoint)
{
    oldPoint1.A = oldPoint1.A +1;
	oldPoint1.B = oldPoint1.B +1;
	oldPoint1.C = oldPoint1.C +1;
	return=true;
}
oldPoint = Parent.Create( 1, 2, 3 );
derivedpoint = Child.Create( 7,8,9 );
basePoint = modify( oldPoint );
derivedPoint2 = modify( derivedpoint );
x1 = oldPoint.A;
x2 = derivedpoint.B;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x1", 1);
            thisTest.Verify("x2", 8);
            Assert.IsTrue(mirror.GetValue("basePoint", 0).DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("derivedPoint2", 0).DsasmValue.IsNull);
        }

        [Test]
        [Category("SmokeTest")]
        public void T53_Undefined_Class_As_Parameter_1463738()
        {
            string code = @"
class Parent
{
A : var;
B : var;
C : var;
constructor Create( x:int, y:int, z:int )
{
A = x;
B = y;
C = z;
}
}
class Child extends Parent
{
constructor Create( x:int, y:int, z:int )
{
A = x;
B = y;
C = z;
}
}
def modify(oldPoint : TestPoint)
{
oldPoint.A = oldPoint.A +1;
oldPoint.B = oldPoint.B +1;
oldPoint.C = oldPoint.C +1;
return=true;
}
oldPoint = Parent.Create( 1, 2, 3 );
derivedpoint = Child.Create( 7,8,9 );
basePoint = modify( oldPoint );
derivedPoint2 = modify( derivedpoint );
x1=oldPoint.A;
x2=derivedpoint.B; ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x1", 1);
            thisTest.Verify("x2", 8);
            Assert.IsTrue(mirror.GetValue("basePoint", 0).DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("derivedPoint2", 0).DsasmValue.IsNull);
        }

        [Test]
        [Category("SmokeTest")]
        public void T53_Undefined_Class_As_Parameter_1463738_2()
        {
            string code = @"
class Parent
{
A : var;
B : var;
C : var;
constructor Create( x:int, y:int, z:int )
{
A = x;
B = y;
C = z;
}
}
class Child extends Parent
{
constructor Create( x:int, y:int, z:int )
{
A = x;
B = y;
C = z;
}
}
def modify(oldPoint : TestPoint[]) // array of class as argumment and class is not defined
{
oldPoint.A = oldPoint.A +1;
oldPoint.B = oldPoint.B +1;
oldPoint.C = oldPoint.C +1;
return=true;
}
oldPoint = Parent.Create( 1, 2, 3 );
derivedpoint = Child.Create( 7,8,9 );
basePoint = modify( oldPoint );
derivedPoint2 = modify( derivedpoint );
x1 = oldPoint.A;
x2 = derivedpoint.B;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x1", 1);
            thisTest.Verify("x2", 8);
            Assert.IsTrue(mirror.GetValue("basePoint", 0).DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("derivedPoint2", 0).DsasmValue.IsNull);
        }

        [Test]
        [Category("SmokeTest")]
        public void T53_Undefined_Class_As_Parameter_1463738_3()
        {
            string code = @"
class Parent
{
A : var;
B : var;
C : var;
constructor Create( x:int, y:int, z:int )
{
A = x;
B = y;
C = z;
}
}
class Child extends Parent
{
constructor Create( x:int, y:int, z:int )
{
A = x;
B = y;
C = z;
}
}
def modify(tmp : Child) // definition with inherited class
{
tmp.A = tmp.A +1;
tmp.B = tmp.B +1;
tmp.C = tmp.C +1;
return=true;
}
oldPoint = Parent.Create( 1, 2, 3 );
derivedpoint = Child.Create( 7,8,9 );
test1 = modify( oldPoint ); // call function with object of parent class
test2 = modify( derivedpoint );
x1 = oldPoint.A;
x2 = derivedpoint.B;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("test1", null);
            thisTest.Verify("x1", 1);
            thisTest.Verify("x2", 9);

        }

        [Test]
        [Category("SmokeTest")]
        public void T53_Undefined_Class_As_Parameter_1463738_4()
        {
            //Assert.Fail("1467236 - Sprint25: rev 3418 : REGRESSION : Cyclic dependency detected in updated on class instances inside function calls");
            string code = @"
class Parent
{
A : var;
B : var;
C : var;
constructor Create( x:int, y:int, z:int )
{
A = x;
B = y;
C = z;
}
}
class Child extends Parent
{
constructor Create( x:int, y:int, z:int )
{
A = x;
B = y;
C = z;
}
}
def modify(oldPoint : Parent)
{
oldPoint.A = oldPoint.A +1;
oldPoint.B = oldPoint.B +1;
oldPoint.C = oldPoint.C +1;
return=true;
}
oldPoint = Parent.Create( 1, 2, 3 );
derivedpoint = Child.Create( 7,8,9 );
basePoint = modify( oldPoint );
derivedPoint2 = modify( derivedpoint );
x1 = oldPoint.A;
x2 = derivedpoint.B;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("x1", 2);
            thisTest.Verify("x2", 9);

        }

        [Test]
        [Category("SmokeTest")]
        public void T53_Undefined_Class_As_Parameter_1463738_6()
        {
            //Assert.Fail("1467236 - Sprint25: rev 3418 : REGRESSION : Cyclic dependency detected in updated on class instances inside function calls");

            string code = @"
class Parent
{
A : var;
B : var;
C : var;
constructor Create( x:int, y:int, z:int )
{
A = x;
B = y;
C = z;
}
}
class Child extends Parent
{
constructor Create( x:int, y:int, z:int )
{
A = x;
B = y;
C = z;
}
}
def modify(oldPoint1 : Parent) // two different function in one of them has a known class type as argument 
{
oldPoint1.A = oldPoint1.A +1;
oldPoint1.B = oldPoint1.B +1;
oldPoint1.C = oldPoint1.C +1;
return=true;
}
def modify(oldPoint1 : TestPoint)
{
oldPoint1.A = oldPoint1.A +1;
oldPoint1.B = oldPoint1.B +1;
oldPoint1.C = oldPoint1.C +1;
return=true;
}
oldPoint = Parent.Create( 1, 2, 3 );
derivedpoint = Child.Create( 7,8,9 );
basePoint = modify( oldPoint );
derivedPoint2 = modify( derivedpoint );
x1 = oldPoint.A; 
x2 = derivedpoint.B; 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("x1", 2);
            thisTest.Verify("x2", 9);
        }

        [Test]
        public void T53_Undefined_Class_As_Parameter_1463738_7()
        {
            string code = @"
class Parent
{
A : var;
B : var;
C : var;
constructor Create( x:int, y:int, z:int )
{
A = x;
B = y;
C = z;
}
}
class Child extends Parent
{
constructor Create( x:int, y:int, z:int )
{
A = x;
B = y;
C = z;
}
}
def modify(oldPoint1 : Parent) // two different function in one of them has a known class type as argument 
{
oldPoint1.A = oldPoint1.A +1;
oldPoint1.B = oldPoint1.B +1;
oldPoint1.C = oldPoint1.C +1;
return=true;
}

x1;
x2;
[Imperative]
{
 def modify:void()
{
oldPoint1.A = oldPoint1.A +1;
oldPoint1.B = oldPoint1.B +1;
oldPoint1.C = oldPoint1.C +1;
}
oldPoint = Parent.Create( 1, 2, 3 );
derivedpoint = Child.Create( 7,8,9 );
basePoint = modify( oldPoint );
derivedPoint2 = modify( derivedpoint );
x1 = oldPoint.A; // expected 1, received : 2
x2 = derivedpoint.B; // expected 8; received : 9
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("x1", 2);
            thisTest.Verify("x2", 9);
        }

        [Test]
        [Category("SmokeTest")]
        public void T53_Undefined_Class_As_Parameter_1463738_8()
        {
            //Assert.Fail("1467236 - Sprint25: rev 3418 : REGRESSION : Cyclic dependency detected in updated on class instances inside function calls");

            string code = @"
class Parent
{
A : var;
B : var;
C : var;
constructor Create( x:int, y:int, z:int )
{
A = x;
B = y;
C = z;
}
}
class Child extends Parent
{
constructor Create( x:int, y:int, z:int )
{
A = x;
B = y;
C = z;
}
}
def modify(a:int,oldPoint1 : Parent) // two different function in one of them has a known class type as argument 
{
oldPoint1.A = oldPoint1.A +1;
oldPoint1.B = oldPoint1.B +1;
oldPoint1.C = oldPoint1.C +1;
return=true;
}
def modify(a:int,oldPoint1 : TestPoint)
{
oldPoint1.A = oldPoint1.A +1;
oldPoint1.B = oldPoint1.B +1;
oldPoint1.C = oldPoint1.C +1;
return =a ;
}
oldPoint = Parent.Create( 1, 2, 3 );
derivedpoint = Child.Create( 7,8,9 );
basePoint = modify(1, oldPoint );
derivedPoint2 = modify(1, derivedpoint );
x1 = oldPoint.A; 
x2 = derivedpoint.B; 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("x1", 2);
            thisTest.Verify("x2", 9);
        }

        [Test]
        [Category("SmokeTest")]
        public void T53_Undefined_Class_As_Parameter_imperative_1463738_9()
        {
            string code = @"
class Parent
{
A : var;
B : var;
C : var;
constructor Create( x:int, y:int, z:int )
{
A = x;
B = y;
C = z;
}
}
class Child extends Parent
{
constructor Create( x:int, y:int, z:int )
{
A = x;
B = y;
C = z;
}
}
x1;x2;
[Imperative] // in imperative , object type not defined 
{
def modify(a:int,oldPoint1 : TestPoint)
{
oldPoint1.A = oldPoint1.A +1;
oldPoint1.B = oldPoint1.B +1;
oldPoint1.C = oldPoint1.C +1;
return =a ;
}
oldPoint = Parent.Create( 1, 2, 3 );
derivedpoint = Child.Create( 7,8,9 );
basePoint = modify(1, oldPoint );
derivedPoint2 = modify(1, derivedpoint );
x1 = oldPoint.A; 
x2 = derivedpoint.B;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x1", 1);
            thisTest.Verify("x2", 8);
        }

        [Test]
        [Category("SmokeTest")]
        public void T53_Undefined_Class_negative_1467107_10()
        {
            string code = @"
def foo(x:int)
{
return = x + 1;
}
//y1 = test.foo(2);
m=null;
y2 = m.foo(2);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object a = null;
            thisTest.Verify("y2", a);
        }

        [Test]
        [Category("SmokeTest")]
        public void T53_Undefined_Class_negative_imperative_1467107_11()
        {
            string code = @"
y2;
[Imperative]
{
	def foo(x:int)
	{
		return = x + 1;
	}
	//y1 = test.foo(2);
	m=null;
	y2 = m.foo(2);
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object a = null;
            thisTest.Verify("y2", a);
        }

        [Test]
        [Category("SmokeTest")]
        public void T53_Undefined_Class_negative_imperative_1467091_12()
        {
            string code = @"
y;
[Imperative]
{
	def foo ( x : int)
	{
		return = x + 1;
	}
	y = test.foo (1);
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object a = null;
            thisTest.Verify("y", a);
        }

        [Test]
        [Category("SmokeTest")]
        public void T53_Undefined_Class_negative_associative_1467091_13()
        {
            string code = @"
def foo ( x : int)
{
return = x + 1;
}
y = test.foo (1);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object a = null;
            thisTest.Verify("y", a);
        }

        // Jun: To address on dot op defect

        [Test]
        public void T55_Defect_1460616()
        {
            string code = @"
class A
{ 
	x : var ;
	constructor A ()
	{
		      	
	}	
}
class B extends A
{
	constructor B()
	{
	    x=this.B();
	}
}
class C
{ 
	x : var ;
	
	
	constructor C ()
	{
		  x = this.C();
	}
        constructor C_1 ()
	{
		  x = this.C_1();
	}
}
a1 = B.B();
x1 = a1.x;
a2 = C.C();
x2 = a2.x;
a3 = C.C_1();
x3 = a3.x;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object v1 = null;
            thisTest.Verify("x1", v1);
            thisTest.Verify("x2", v1);
            thisTest.Verify("x3", v1);

        }

        [Test]
        [Category("Failure")]
        public void TV55_Defect_1460616_1()
        {
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3952
            Assert.Fail("MAGN-3952 DS goes into infinite loop with class having 'this' pointer");
            string code = @"
class A
{ 
	x : var ;
	constructor A ()
	{
		      	
	}	
}
class B extends A
{
	constructor B()
	{
	    x=this.B();
	}
}
class C
{ 
	x : var ;
	
	
	constructor C ()
	{
		  x = this.C_1();
	}
        constructor C_1 ()
	{
		  x = this.C();
	}
}
a1 = B.B();
x1 = a1.x;
a2 = C.C();
x2 = a2.x;
a3 = C.C_1();
x3 = a3.x;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object v1 = null;
            thisTest.Verify("x1", v1);
            thisTest.Verify("x2", v1);
            thisTest.Verify("x3", v1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T55_Defect_1460616_2()
        {
            string code = @"
class C
{ 
	x : var ;
	
    constructor C_1 ()
	{
		  x = C_1();
	}
}
a3 = C.C_1();
x3 = a3.x;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object n1 = null;
            thisTest.Verify("x3", n1);
        }


        [Test]
        [Category("SmokeTest")]
        public void T56_Local_Class_method_Same_Name_As_Global_Function()
        {
            string code = @"
def dong()
{
    return = A.A();
}
class A
{
    def foo()
    {
        return = 100;
    }
}
class B
{
    def foo()
    {
        return = 1000;
    }
    def ding()
    {
        a = dong();
        return = a.foo();
    }
}
b = B.B();
x = b.ding();  // expect 100 but got 1000
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 100);
        }

        [Test]
        [Category("Method Resolution")]
        public void T58_Defect_1462445()
        {
            //Assert.Fail("1462445 - Sprint 19 : [Design Issue] Geometry Porting : Rev 1989 : Static method overload issue ");

            string code = @"
class A
            {
                   b : int;
                   static z : int;
                   def foo(a : int)
                   {
                          b = 1;
                          return = a;
                   }
                   static def foo(a : int[])
                   {
                          z = 2;
                          return = 9;
                   }
            }
            x = A.A();
            c = {1,2,3,4};
            d = A.foo(c);  // expected : 9
            y = x.b; 
            v = x.z; // expected : 2
            w = A.z; // expected : 2
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("d", 9);
            thisTest.Verify("v", 2);
            thisTest.Verify("y", 0);
            thisTest.Verify("w", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T59_Defect_1466572()
        {
            string code = @"
class Point
{
    X : var;
	Y: var;
	Z: var;
	constructor Point ( x, y, z )
	{
	    X = x;
		Y = y;
		Z = z;
	}
}
x1 = 1;
p1 = Point.Point ( -x1, x1, -x1 );
xx = p1.X;// expected -1, received 1
yy = p1.Y;// expected 1, received 1
zz = p1.Z;// expected -1, received -1
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("xx", -1);
            thisTest.Verify("yy", 1);
            thisTest.Verify("zz", -1);

        }

        [Test]
        [Category("SmokeTest")]
        public void T60_Defect_1467004()
        {
            string code = @"
class TestDefect
{
        def foo(val : double)
        {
                return = val;
        }
        def foo(arr : double[])
        {
                return = -123;
        }        
}
test = TestDefect.TestDefect();
arr = 5..25;
s = test.foo(arr); //Expected output should be -123
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("s", -123);

        }

        [Test]
        [Category("SmokeTest")]
        public void T59_Defect_1466572_2()
        {
            string code = @"
class Point
{
    X : var;
	Y: var;
	Z: var;
	constructor Point ( x, y, z )
	{
	    X = x;
		Y = y;
		Z = z;
	}
}
x1 = 1;
x2 = -x1;
p1 = Point.Point ( -x1, -(x1+x2), -x1*x2 );
xx = p1.X;
yy = p1.Y;
zz = p1.Z;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("xx", -1);
            thisTest.Verify("yy", 0);
            thisTest.Verify("zz", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T59_Defect_1466572_3()
        {
            string code = @"
class Point
{
    X : var;
	Y: var;
	Z: var;
	constructor Point ( x, y, z )
	{
	    X = x;
		Y = y;
		Z = z;
	}
}
x1 = 1;
x2 = -x1;
p1 = Point.Point ( -x1, -(x1+x2), -x1*x2 );
xx = p1.X;
yy = p1.Y;
zz = p1.Z;
def foo ( a : double, b:double, c:double)
{
    return = a + b + c;
}
p2 = foo ( -x1, -(x1+x2), -x1*x2+0.5 );
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("p2", 0.5);
        }

        [Test]
        [Category("SmokeTest")]
        public void T61_Defect_1459171()
        {
            string code = @"
class Point{
X : double;
Y : double;
constructor ByCoordinates( x : double, y: double )
{
X = x;
Y = y;
}
def create( )
{
return = Point.ByCoordinates( X, Y );
}
 }
p1 = Point.ByCoordinates( 5.0, 10.0);
b1 = p1.X;
b2 = p1.Y;
a1 = p1.create();
a2 = a1.X;
a3 = a1.Y;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b1", 5.000000);
            thisTest.Verify("b2", 10.000000);
            thisTest.Verify("a2", 5.000000);
            thisTest.Verify("a3", 10.000000);
        }

        [Test]
        public void T62_class_assignment_inside_imperative()
        {
            string code = @"class point{
constructor point(x:double)
{
a = 1;
}
} 
class polygon{
constructor polygon(pt:point[])
{
poly= pt;
}
} 
pt0=point.point(1.0);
pt1=point.point(2.0);
pt2=point.point(3.0);
pt3=point.point(4.0);
pt4=point.point(5.0);
pt5=point.point(6.0);
pt6=point.point(7.0);
pt7=point.point(8.0);
pt8=point.point(9.0);
facesIndices = {{0,1,5,4},{1,2,6,5}};
pointGroup = {pt0,pt1,pt2,pt3,pt4,pt5,pt6,pt7,pt8};
z=[Imperative]
{
def buildarray(test:int[],collect:point[])
{
b= { } ;//=> Issue 2 : if we use a predetermined sized array like { 0,0,0,0} here instead, then this function returns the correct value instead of null
j=0;
for (k in test)
{
b[j] = collect[k];
j=j+1;
}
return =b;
}
controlPoly={};
for (i in 0..1)
{
c={0,1,5,4};
c=facesIndices[i];
a=buildarray(c,pointGroup);
controlPoly[i] = polygon.polygon(a);
}
return=controlPoly;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            // thisTest.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kFunctionNotFound);
        }

        [Test]
        [Category("SmokeTest")]
        public void T63_Array_inClass_Imperative_1465637()
        {
            string code = @"
class A
{
X:var;
Y:var;
Count1 :int;
constructor A ( i : int )
	{
	X = 0..i;
	[Imperative]
	{
		Y = {0,0,0,0,0};
		Count1 = 0; 
		for ( i in X ) {
			Y[Count1] = i * -1;
			Count1 = Count1 + 1;
		}
	}
}
}
p = 4;
a = A.A(p);
b1 = a.X;
 // expected { 0, 1, 2, 3, 4 }
b2 = a.Y;
 // expected {0,-1,-2,-3,-4}
b3 = a.Count1;
//received : //watch:
 b1 = {0,-1,-2,-3,-4};//watch: 
b2 = {0,0,0,0,0};
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] b1 = new Object[] { 0, -1, -2, -3, -4 };
            Object[] b2 = new Object[] { 0, 0, 0, 0, 0 };
            thisTest.Verify("b1", b1);
            thisTest.Verify("b2", b2);
        }


        [Test]
        public void T68_Inherit_Base_Constructor_1467153()
        {
            string code = @"
class PointA
{
        X : double;
        Y : double;
        Z : double;
        constructor ByCoordinates (x : double, y : double, z : double)
        {
                X = x;
                Y = y;
                Z = z;
        }
}
class Node extends PointA
{
        Fixity  : bool;
        constructor ByCoordinatesFixity (x : double, y : double, z : double, fixity : bool)
                : base.ByCoordinates (x, y, z)
        {
                Fixity = fixity;
        }
}
n = Node.ByCoordinatesFixity(1,2,3,false);
a=n.X;
b=n.Y;
c=n.Z;
d=n.Fixity;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1.0);
            thisTest.Verify("b", 2.0);
            thisTest.Verify("c", 3.0);
            thisTest.Verify("d", false);
        }

        [Test]
        public void T68_Inherit_Base_Constructor_1467153_negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
class PointA
{
        X : double;
        Y : double;
        Z : double;
        constructor ByCoordinates (x : double, y : double, z : double)
        {
                X = x;
                Y = y;
                Z = z;
        }
}
class Node extends PointA
{
        Fixity  : bool;
        constructor ByCoordinatesFixity (x : double, y : double, z : double, fixity : bool)
                : base ByCoordinates (x, y, z)
        {
                Fixity = fixity;
        }
}
n = Node.ByCoordinatesFixity(1,2,3,false);
a=n.X;
b=n.Y;
c=n.Z;
d=n.Fixity;
";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });

        }


        [Test]
        public void T63_Class_methodresolution_1457172()
        {
            string code = @"
class A
{
a : var;
b : var;
constructor A( a1:int[], a2:int[])
{
a = a1;
b = a2;
}
def foo :int[]..[]( a : int[], b:int[])
{
y = [Imperative]
{
x = { { 0,0,0 }, {0,0,0} , {0,0,0} };
c1 = 0;
for ( i in a)
{
c2 = 0;
for ( j in b )
{
x[c1][c2] = i + j ;
c2 = c2+ 1;
}
c1 = c1 + 1;
}
return = x;
}
return = y;
}
}
a = { 0, 1, 2 };
b = { 3, 4, 5 };
x = A.A( a, b);
y = x.foo ();
p1 = y[0][0];
p2 = y[0][0];
p3 = y[0][2];
p4 = y[1][0];
p5 = y[1][1];
p6 = y[1][2];
p7 = y[2][0];
p8 = y[2][1];
p8 = y[2][2];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.RuntimeData.WarningID.kMethodResolutionFailure);
            Object a = null;
            thisTest.Verify("p1", a);
            thisTest.Verify("p2", a);
            thisTest.Verify("p3", a);
            thisTest.Verify("p4", a);
            thisTest.Verify("p5", a);
            thisTest.Verify("p6", a);
            thisTest.Verify("p7", a);
            thisTest.Verify("p8", a);
        }

        [Test]
        public void T63_Class_methodresolution_1457172_2()
        {
            string code = @"
class A
{
a : var;
b : var;
c : var;
constructor A( a1:int[], a2:int[], a3:int[])
{
a = a1;
b = a2;
c = a3;
}
def foo :int[]..[]( a : int[], b:int[], c :int[])
{
y = [Imperative]
{
x = { { { 0, 0} , { 0, 0} }, { { 0, 0 }, { 0, 0} }};
c1 = 0;
for ( i in a)
{
c2 = 0;
for ( j in b )
{
c3 = 0;
for ( k in c )
{
x[c1][c2][c3] = i + j + k;
c3 = c3 + 1;
}
c2 = c2+ 1;
}
c1 = c1 + 1;
}
return = x;
}
return = y;
}
}
a = { 0, 1 };
b = { 2, 3};
c = { 4, 5 };
x = A.A( a, b , c);
y = x.foo ();
p1 = y[0][0][0];
p2 = y[0][0][1];
p3 = y[0][1][0];
p4 = y[0][1][1];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.RuntimeData.WarningID.kMethodResolutionFailure);
            Object a = null;
            thisTest.Verify("p1", a);
            thisTest.Verify("p2", a);
            thisTest.Verify("p3", a);
            thisTest.Verify("p4", a);

        }

        [Test]
        public void T63_Class_methodresolution_1457172_3()
        {
            string code = @"
class A
{
def foo :int( a : int)
{
return =1;
}
}
x = A.A( );
y = x.foo ();
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.RuntimeData.WarningID.kMethodResolutionFailure);
            Object a = null;
            thisTest.Verify("y", a);

        }

        [Test]
        public void T69_Redefine_property_inherited_1467141()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
class A
{
	fx: int;
	constructor A(x:int)
	{
		fx = x;
	}
}
class B extends A
{
	fx : int;
	fy : int;
	constructor  B(y :int):base.A(x)
	{
		fy = y;
	}
}
a = A.A(1);
b = B.B(2);
r1 = a.fx;
r2 = a.fy;
";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        public void T70_Defect_1467112_Method_Overloading_Issue()
        {
            string code = @"
class A
{ 
    public x : var ;    
    
    public def foo1 (a)
    {
      return = 1;
    } 
    
}
class B extends A
{
    public def foo1 (a)
    {
        return = 2;
    }  
        
        
}
b = B.B();
b1 = b.foo1(1);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1467112 - Sprint 23 : rev 2797 : Method resolution issue : Derived class method of same name cannot be accessed");
            thisTest.Verify("b1", 2);
        }

        [Test]
        public void T71_class_inherit_arg_var_1467157()
        {
            // Assert.Fail("1467157 - Sprint 25 - rev 3047 class inheritance if the argument is var and the same declared both in base calss and inherited class it throws error ");
            string code = @"
class Geometry
{
        def Clone : Geometry()
        {
                return = CreateNew(1);
        }
        def CreateNew : Geometry(a:var)
        { 
                return = null; 
        }
}
class Curve extends Geometry
{
}
class Arc extends Curve
{
        def CreateNew : Geometry(a:var)
        { 
                return = Arc.Arc(); 
        }
}
a=Arc.Arc();
b=a.CreateNew(1);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            // not possible to add verification this testcase as there is not method to verify that the warning are gone ! 

        }

        [Test]
        public void T72_class_inherit_1467097_1()
        {
            //Assert.Fail("1467097 Sprint 24 - Rev 2761 - if var is used as a argument to function and call function with defined class it goes into a loop and hangs DS ");
            string code = @"
class Parent
{
	A : var;
	B : var;
	C : var;
constructor Create( x:int, y:int, z:int )
{
	A = x;
	B = y;
	C = z;
}
}
class Child extends Parent
{
	constructor Create( x:int, y:int, z:int )
	{
		A = x;
		B = y;
		C = z;
	}
}
def modify(oldPoint : var)
{
	
oldPoint.A = oldPoint.A +1;
	oldPoint.B = oldPoint.B +1;
	oldPoint.C = oldPoint.C +1;
	return=true;
}
oldPoint = Parent.Create( 1, 2, 3 );
t = modify( oldPoint );
a=oldPoint.A;
b=oldPoint.B;
c=oldPoint.C;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 2);
            thisTest.Verify("b", 3);
            thisTest.Verify("c", 4);

        }

        [Test]
        public void T72_class_inherit_1467097_2()
        {
            //Assert.Fail("1467097 Sprint 24 - Rev 2761 - if var is used as a argument to function and call function with defined class it goes into a loop and hangs DS ");
            string code = @"
class Parent
{
	A : var;
	B : var;
	C : var;
constructor Create( x:int, y:int, z:int )
{
	A = x;
	B = y;
	C = z;
}
}
class Child extends Parent
{
	constructor Create( x:int, y:int, z:int )
	{
		
A = x;
		B = y;
		C = z;
	}
}
def modify(oldPoint : var)
{
	
oldPoint.A = oldPoint.A +1;
	oldPoint.B = oldPoint.B +1;
	oldPoint.C = oldPoint.C +1;
	return=true;
}
derivedpoint = Child.Create( 7,8,9 );
t = modify( derivedpoint );
a=derivedpoint.A;
b=derivedpoint.B;
c=derivedpoint.C;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 8);
            thisTest.Verify("b", 9);
            thisTest.Verify("c", 10);

        }

        [Test]
        public void TV_1467097_class_inherit()
        {
            String code =
@"
class Parent
{
    A : var;
    B: var;
    constructor Create( x:int,y:int)
    {
        A = x;
        B = y;
    }
}
def modify(oldPoint : var)
{
oldPoint.A = oldPoint.A +1;
oldPoint.B = oldPoint.B +1;
return = oldPoint;
}
oldPoint = Parent.Create( 1,2 );
oldPoint = modify( oldPoint );
ra = oldPoint.A;
rb = oldPoint.B;
    
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("ra", 2);
            thisTest.Verify("rb", 3);
        }


        [Test]
        [Category("SmokeTest")]
        public void T73_Defect_1467210_Expected_Warning()
        {
            String code =
@"
class Test
{
    def DoSomething()
    {
        return = 5;
    }
}
t = Test.Test();
a = Test.DoSomething(t); //no warning is thrown and returned value is null
a = Test.DoSomething(); //wrong warning is thrown: 
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object n1 = null;
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kFunctionNotFound);
            thisTest.Verify("a", n1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T74_Defect_1469099_Access_Property()
        {
            string code = @"
class Point
{
    X:double;
    constructor ByCoordinates()
        { X=1234.5678; }
}
class brick
{
    cent:int;    ang:int;    
    constructor createBrick(point1: Point)
    {
        cent=brickCenter(point1); //constructor calling a function inside class
        ang=brickAngle(point1); //constructor calling a function inside class
    }
def brickAngle(point1: Point)
    { xx1 = true;    return = 0; }
def brickCenter(point1: Point)
    { xx2 = true;    return = 1;}
}
p1 = Point.ByCoordinates();
b1 = brick.createBrick(p1);
t1 = b1.cent;
t2 = b1.ang;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t1", 1);
            thisTest.Verify("t2", 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T74_Defect_1469099_Access_Property_2()
        {
            string code = @"
class Point
{
    X:double;
    constructor ByCoordinates()
        { X=1234.5678; }
}
class Point2
{
    X:double;
    constructor ByCoordinates()
        { X=1234.5678; }
}
class brick extends Point2
{
    cent:int;    ang:int;    
    constructor createBrick(point1: Point)
    {
        cent=brickCenter(point1); //constructor calling a function inside class
        ang=brickAngle(point1); //constructor calling a function inside class
    }
def brickAngle(point1: Point)
    { xx1 = true;    return = 0; }
def brickCenter(point1: Point)
    { xx2 = true;    return = 1;}
}
t1;t2;
[Imperative]
{
	p1 = Point.ByCoordinates();
	b1 = brick.createBrick(p1);
	t1 = b1.cent;
	t2 = b1.ang;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t1", 1);
            thisTest.Verify("t2", 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T75_Defect_1467188_Class_Instantiation()
        {
            string code = @"
class A
{
    X : var;   
    constructor A(xValue : double)
    {
        X = xValue;               
    }    
    public def foo  (other : A)
    {
        return = X * other.X ;
    }
}
C0 = A.A(1.0);
t = A.A(1);
tx = A.A(C0.X);
RX = tx.foo(t);
ty = A.A(C0.X);
RY = ty.foo(t);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("RX", 1.0);
            thisTest.Verify("RY", 1.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T75_Defect_1467188_Class_Instantiation_2()
        {
            string code = @"
class B
{
    Y : var;   
    constructor B(yValue : double)
    {
        Y = yValue;               
    }    
    public def foo  (other : A)
    {
        return = Y  + other.X + other.Y;
    }
}
class A extends B
{
    X : var;   
    constructor A(xValue : double, yValue : double)
    {
        X = xValue; 
        Y = yValue;
    }  
    
}
a = A.A( 1.0, 2.0 );
t = A.A( 3, 4);
tx = A.A( a.X, a.Y );
RX = tx.foo( t );
ty = A.A( a.X, a.Y );
RY = ty.foo( t );";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("RX", 9.0);
            thisTest.Verify("RY", 9.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T76_Defect_1467186_Class_Update()
        {
            string code = @"
class A
{
        a : var;
    constructor A ( a1 : double)
    {
        a = a1;
    }
}
y = A.A( x);
a1 = y.a;
x = 3;
x = 5;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a1", 5.0);
        }

    

        [Test]
        public void T77_Defect_1460274_Class_Update_2()
        {
            string code = @"
class Point
{
  X : var;
  constructor ByCartesianCoordinates( x : var)
  {
      X = x;
  }
  def Translate ( y : var )
  {
      return = Point.ByCartesianCoordinates( X + y ) ;
  }
}
pt2 = Point.ByCartesianCoordinates(1);
pt3 = Point.ByCartesianCoordinates(2);
pointGroup2 = {pt2,pt3};
pointGroup2[0] = {
                    pointGroup2[0].Translate(3) => t2;		    
		}
test2 = pointGroup2.X;
pt0 = Point.ByCartesianCoordinates(1);
pt1 = Point.ByCartesianCoordinates(2);
pointGroup = {pt0,pt1};
t = pointGroup[0].Translate(3);
pointGroup[0] = t;
test = pointGroup.X;
pt4 = Point.ByCartesianCoordinates(1);
pt5 = Point.ByCartesianCoordinates(2);
pointGroup3 = {pt4,pt5};
pointGroup3[0] = pointGroup3[0].Translate(3);
test3 = pointGroup3.X;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1460139 - Sprint 18 : Rev 1580 : Update causing cyclic dependency for valid case");
            Object n1 = null;
            thisTest.Verify("test2", new Object[] { 4, 2 });
            thisTest.Verify("test", n1);
            thisTest.Verify("test3", new Object[] { 4, 2 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T77_Defect_1460274_Class_Update_3()
        {
            string code = @"
class Point
{
  X : var;
  constructor ByCartesianCoordinates( x : var)
  {
      X = x;
  }
  def Translate ( y : var )
  {
      return = Point.ByCartesianCoordinates( X + y ) ;
  }
}
pt0 = Point.ByCartesianCoordinates(1);
pt1 = Point.ByCartesianCoordinates(2);
pointGroup = {pt0,pt1};
pointGroup[0] = {
                    pointGroup[0].Translate(3) => t;		    
		}
test = pointGroup.X;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("test", new Object[] { 4, 2 });
        }

        [Test]
        public void T77_Defect_1460274_Class_Update_5()
        {
            string code = @"
geometry1 = {};
geometry1[0] = 5;
geometry1[1] = 10;
geometry1[2] = {
                  geometry1[0]+ geometry1[1];
              }
geometry1[4] = {
                  geometry1[1]+ 1;
              }
test1 = geometry1;
geometry = {};
geometry[0] = 5;
geometry[1] = 10;
geometry[2] = geometry[0]+ geometry[1];
geometry[4] = geometry[1]+1;
test = geometry;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1460139 - Sprint 18 : Rev 1580 : Update causing cyclic dependency for valid case");
            thisTest.Verify("test", new Object[] { 5, 10, 15, null, 11 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T78_Defect_1467146_Class_Update_With_Replication()
        {
            string code = @"
class A
{
static def execute(b : A)
 { 
  return = 100; 
 }
}
arr = {A.A()};
v = A.execute(arr);
val = v[0];";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("val", 100);
        }

        [Test]
        [Category("SmokeTest")]
        public void T78_Defect_1467146_Class_Update_With_Replication_2()
        {
            string code = @"
class A
{
static def execute(b : A)
 { 
  return = 100; 
 }
}
arr = {};
v = A.execute(arr);
val = v[0];";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object n1 = null;
            thisTest.Verify("val", n1);
        }

        [Test]
        [Category("Failure")]
        public void T78_Defect_1467146_Class_Update_With_Replication_3()
        {
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1660
            string err = "MAGN-1660 Sprint25: rev 3352: Type conversion - method dispatch over heterogeneous array is not correct";
            string code = @"class A
{
static def execute(b : A)
 { 
  return = 100; 
 }
}
arr = {A.A(), null, 3};
v = A.execute(arr);
";
            thisTest.VerifyRunScriptSource(code, err);
            //Assert.Fail("1467224 - Sprint25: rev 3352: method dispatch over heterogeneous array is not correct");
            Object n1 = null;
            thisTest.Verify("v", new Object[] { 100, 100, n1 });
        }

        [Test]
        [Category("SmokeTest")]
        [Category("Failure")]
        public void T78_Defect_1467146_Class_Update_With_Replication_4()
        {
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1693
            string str = "MAGN-1693 Regression : Dot Operation using Replication on heterogenous array of instances is yielding wrong output";
            string code = @"class A
{
static def execute(b : A)
 { 
  return = 100; 
 }
}
arr = {A.A(), null, 3};
v = A.execute(arr);
";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, str);
            Object n1 = null;
            thisTest.Verify("v", new Object[] { 100, n1, n1 });
        }

        [Test]
        public void T78_Defect_1467146_Class_Update_With_Replication_5()
        {
            string code = @"
class A
{
	static def execute1( a : B)
	{ 
	  return = 100; 
	}
	
	static def execute2( a : B[])
	{ 
	  return = 200; 
	}
	
	
}
class B extends A
{
	
}
arr = {};
arr2 : B[] = null;
v1 = B.execute1(arr);
v = v1[0];
v2 = B.execute2(arr);
p1 = B.execute1(arr2);
p = p1[0];
p2 = B.execute2(arr2);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1467224 - Sprint25: rev 3352: method dispatch over heterogeneous array is not correct"); 
            Object n1 = null;
            thisTest.Verify("v", n1);
            thisTest.Verify("v2", 200);
            thisTest.Verify("p", n1);
            thisTest.Verify("p2", 200);
        }

        [Test]
        public void T79_Defect_1458581_Unnamed_Constructor()
        {
            string code = @"
class A 
{ 
	x1 : int ;
	x2 : double;
	
	constructor () 
	{	
		x1 = 1; 
		x2 = 1.5;		
	}
}
a1 = A.A();
b1 = a1.x1;
b2 = a1.x2;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b1", 1);
            thisTest.Verify("b2", 1.5);
        }

        [Test]
        [Category("Replication")]
        public void T80_Defect_1444246_Replication()
        {
            string code = @"
class Point
{
	x : var;
	constructor Create(xx : int)
	{
		x = xx;
	}
}
xs;
[Associative]
{
	coords = {0,1,2,3,4,5,6,7,8,9};
	pts = Point.Create(coords);
	xs = pts.x;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("xs", new Object[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
        }

        [Test]
        [Category("Replication")]
        public void T80_Defect_1444246_Replication_2()
        {
            string code = @"
class MyPoint
{ 
	X: var;
	Y: var;
	constructor CreateXY(x : double, y : double)
	{
		X = x;
		Y = y;
	} 
}
p2 = MyPoint.CreateXY(-20.0,-30.0).X;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("p2", -20.0);
        }

        [Test]
        public void T81_Defect_1467246_derived_class_setter()
        {
            string code = @"
class A
{ 
	x : var ;	
	constructor A ()
	{
	    x = 4;	          	
	}	
}
class B extends A
{ 
	z : var;	
	constructor B () : base.A()
	{
	    z = 6;      	
	}	
}
a1 = B.B();
x1 = a1.x;
z1 = a1.z;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1467246 - sprint25: rev 3445 : REGRESSION : Property setter/getter not working as expected for derived classes");

            thisTest.Verify("x1", 4);
            thisTest.Verify("z1", 6);
        }

        [Test]
        public void T82_Defect_1467174()
        {
            string code = @"
class Point
{
    X : double;    
    constructor ByCartesianCoordinates( x : double )
    {
        X = x;
    
    }      
}
class Line
{
    StartPoint : Point;        
    constructor ByStartPointEndPoint( p1 : Point )
    {
        StartPoint = p1;    
    }    
    def PointAtParameter(  v : double )
    {
        start_x = StartPoint.X * v;// this is the culprit
    return = start_x;
    }     
}
p1 = Point.ByCartesianCoordinates(1.5);
l1 = Line.ByStartPointEndPoint(p1);
t1 = l1.StartPoint.X;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("t1", 1.5);

        }

        [Test]
        [Category("Replication")]
        public void T83_Defect_1463232()
        {
            String code =
@"class A
{
}
a = A.A();
t = a.x;
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            thisTest.Verify("t", n1);
        }

        [Test]
        [Category("Replication")]
        public void T83_Defect_1463232_2()
        {
            String code =
@"class A
{
}
a;t;
[Imperative]
{
a = A.A();
t = a.x;
}
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            thisTest.Verify("t", n1);
        }

        [Test]
        [Category("Replication")]
        [Category("Failure")]
        public void T83_Defect_1463232_3()
        {
            String code =
@"class A
{
}
[Associative]
{
a = {A.A(),A.A()};
t = a.x;
}
";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1694
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            string errmsg = "MAGN-1694 Regression : Dot Operation on instances using replication returns single null where multiple nulls are expected";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            thisTest.Verify("t", new Object[] { n1, n1 });
        }

        [Test]
        public void T84_ClassNameAsPropertyName_01()
        {
            string code = @"
class A
{
    x:int;
    static y:int;
    constructor A(_x:int)
    {
        x = _x;
    }
}
class B
{
    A : A;
    constructor B()
    {
        A = A.A(100);
    }
    def getx()
    {
        return = A.x;
    }
    def modifyx()
    {
        A.x = 200;
        return = null;
    }
}
b = B.B();
x1 = b.getx();
r = b.modifyx();
x2 = b.getx();
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x1", 100);
            thisTest.Verify("x2", 200);
            thisTest.VerifyBuildWarningCount(0);
        }

        [Test]
        public void T85_Defect_1467247()
        {
            string code = @"
class A
{ 
    public x : var ;    
    private y : var ;
    //protected z : var = 0 ;
    constructor A ()
    {
               
    }
    public def foo1 (a)
    {
        x = a;
        y = a + 1;
        return = x + y;
    } 
    private def foo2 (a)
    {
        x = a;
        y = a + 1;
        return = x + y;
    }    
}
a = A.A();
a1 = a.foo1(1);
a2 = a.foo2(1);
a.x = 4;
a.y = 5;
t1 = a.x;
t2 = a.y;
";
            thisTest.RunScriptSource(code);
            Object n1 = null;
            thisTest.Verify("t1", 4);
            thisTest.Verify("t2", n1);

        }


        [Test]
        public void T85_Defect_1467247_2()
        {
            string code = @"
class A
{ 
    public x : var ;    
    private y : var ;
    constructor A ()
    {
        x = 10;
        y = 20;   
    }
    public def foo1 ()
    {
       return = foo2();
    } 
    private def foo2 ()
    {
        x = 1;
        y = 2;
        return = x + y;
    }    
}
a = A.A();
a1 = a.foo1();
a2 = a.foo2();
t1 = a.x;
t2 = a.y;
a.x = 4;
a.y = 5;
";
            thisTest.RunScriptSource(code);
            Object n1 = null;
            thisTest.Verify("t1", 4);
            thisTest.Verify("t2", n1);
            thisTest.Verify("a1", 3);
            thisTest.Verify("a2", n1);

        }

        [Test]
        [Category("Failure")]
        public void T85_Defect_1467247_3()
        {
            string code = @"
class A
{ 
    public x : var ;   
    constructor A ()
    {
        x = 10;       
    }
    public def foo ()
    {
       return = x + 1;
    }        
}
a = A.A();
a1 = a.foo();
a.x = 4;
";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1507
            string errmsg = "1467254 - Sprint25: rev 3468 : REGRESSION: class property update is not propagating";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a1", 5);

        }

        [Test]
        public void T86_Defect_1467216()
        {
            string code = @"
class Plane
{
    x : int ;
}
class Polygon
{
    Plane : Plane;
    constructor Create(plane : Plane)
    {
        Plane = plane;
        Plane.x = 10;
    }
}
pln = Plane.Plane();
s = Polygon.Create(pln);
p = s.Plane;
test = p.x;
";
            string errmsg = "1467254 - Sprint25: rev 3468 : REGRESSION: class property update is not propagating";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", 10);
        }

        [Test]
        public void T86_Defect_1467216_2()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
class Plane
{
    x : int ;
}
Plane = Plane.Plane();
test = Plane.x;
";

                thisTest.RunScriptSource(code);
            });
        }

        [Test]
        public void T86_Defect_1467216_3()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
class Plane
{
    x : int ;
}
Plane = 10;
test = Plane;
";
                thisTest.RunScriptSource(code);
            });
        }

        [Test]
        public void T86_Defect_1467216_4()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
class Plane
{
    x : int ;
}
def foo ( ) 
{
    Plane = 10;
    return = Plane;
}
test = foo();
";
                thisTest.RunScriptSource(code);
            });
        }

        [Test]
        public void T87_Defect_1467243()
        {
            string code = @"
class A
{
    fx :var;
    constructor A(x : var)
    {
        fx = x;
    }
}
fa = A.A(1..3);
r1 = fa.fx[0]==fa[0].fx? true:false;
r2 = fa.fx[0];
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("r1", true);

        }

        [Test]
        public void T87_Defect_1467243_2()
        {
            string code = @"
class A
{
    fx :var[][];
    constructor A(x : var[][])
    {
        fx = x;
    }
}
fa = { A.A({{1},{2}}), A.A({{3},{4}}) };
r1 = fa[0].fx[0]==fa.fx[0][0]? true:false;
r2 = fa.fx[0][0];
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("r1", new Object[] { true });
        }

        [Test]
        public void T88_Runtime_MemberFunction01()
        {
            string code = @"
class C1
{
    def foo()
    {
        return = 100;
    }
}
class C2
{
    private def foo()
    {
        return = 200;
    }
}
def foo(i)
{
    return = (i > 0) ? C1.C1() : C2.C2();
}
x = foo(-1);
r = x.foo();
";
            string errmsg = "DNL-1467343 private member function can be called at runtime";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("r", null);
        }

        [Test]
        public void T89_Runtime_MemberFunction02()
        {
            string code = @"
class Base
{
    private def foo()
    {
        return = 100;
    }
}
class Derive extends Base
{
    def foo()
    {
        return = 200;
    }
}
def foo:Base()
{
    return = Derive.Derive();
}
b = foo();
r = b.foo();
";
            string errmsg = "DNL-1467344 Calling a derived class's member function always get null at runtime";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("r", 200);
        }

        [Test]
        public void T89_1467344_02()
        {
            string code = @"
class Base
{
    private def foo()
    {
        return = 100;
    }
}
class Derive extends Base
{
    def foo()
    {
        return = 200;
    }
}
def foo:Base()
{
    return = Derive.Derive();
}
b;
r;
[Imperative]
{
        b = foo();
    r = b.foo();
}
";
            string errmsg = "DNL-1467344 Calling a derived class's member function always get null at runtime";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("r", 200);
        }

        [Test]
        public void T90_Runtime_MemberFunction03()
        {
            string code = @"
class C1 
{
    def foo()
    {
        return = 100;
    }
}
class C2 extends C1
{
    def foo()
    {
        return = 200;
    }
}
class C3 extends C2
{
    def foo()
    {
        return = 300;
    }
}
def foo:C2()
{
    return = C3.C3();
}
x = foo();
r = x.foo();
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("r", 300);
        }

        [Test]
        public void T91_stackoverflow()
        {
            string code = @"
                    class test 
                    { 
                    a; 
                    constructor test(a1 : int, b1 : int, c1 : int) 
                    { 
                    a = a1; 
                    } 
                    } 
                    class Row 
                    { 
                    constructor ByPoints(yy:int, xx: int) 
                    { 
                    [Imperative] 
                    { 
                    for(j in 0..36) 
                    { 
                    tread = test.test(yy, xx, 3); 
                    rise = test.test(tread.a,xx,3); 
                    } 
                    } 
                    } 
                    } 
                    a = 0..18..1; 
                    b = 0..18..1; 
                    Rows = Row.ByPoints(a, b);
                    ";
            string errmsg = "1467365 - Sprint 27 - Rev 3058 Stack overflow detected in the attached code ";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.VerifyBuildWarningCount(0);
        }

        [Test]
        [Category("SmokeTest")]
        [Category("Failure")]
        public void T92_default_argument_1467384()
        {
            String code =
            @"class test
                {
                    t : int;
                    def test(t : int = 4)
                    {
                        return = t;
                    }
                }
                a = test.test().t;
                ";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3950
            string error = "MAGN-3950 Default arguments are not working for class member";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("a", 4, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T92_default_argument_1467402()
        {
            String code =
            @"class B extends A{ b = 2; }
                class A{
                c : A;
                f= 1;
                }
                b1 = B.B(); 
                c = b1.b;
                d = b1.f;
                ";
            string error = " 1467402 -if the class is used before its define it throws error Index was outside the bounds of the array - negative case ";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("c", 2, 0);
            thisTest.Verify("d", 1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T93_Defect_1467349_update_static_properties()
        {
            String code =
@"class Base
{
    static x : int[];
}
t = Base.x;
Base.x = { 5.2, 3.9 }; 
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("t", new Object[] { 5, 4 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T93_Defect_1467349_update_static_properties_2()
        {
            String code =
@"class A
{
static x:int = 3; 
}
a = A.A();
a.x = 2;
b1 = a.x;
a.x = 3; 
b2 = b1;  
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("b2", 3);
        }

        [Test]
        [Category("SmokeTest")]
        public void T93_Defect_1467349_update_static_properties_3()
        {
            String code =
@"class A
{
    static x:int = 3; 
    constructor A ( y1)
    {
        x = y1;
    }
}
y = 2;
a = A.A(y);
b1 = a.x;
y = 3;
b2 = b1;  
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("b2", 3);
        }

        [Test]
        [Category("SmokeTest")]
        public void T93_Defect_1467349_update_static_properties_4()
        {
            String code =
@"class A
{
    static x:int[] = {3,4}; 
    constructor A ( y1:int[])
    {
        x = y1;
    }
}
y = 2..3;
a = A.A(y);
b1 = a.x;
y = 3..4;
b2 = b1;  
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("b2", new Object[] { 3, 4 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T93_Defect_1467349_update_static_properties_5()
        {
            String code =
@"class A
{
    static x:int[] = {3,4}; 
    constructor A ( y1:int[])
    {
        x = y1;
    }
}
y = 2..3;
a = A.A(y);
b1 = a.x;
y = {4,4};
a.x[0] = 3;
b2 = b1;  
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("b2", new Object[] { 3, 4 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T94_1467343()
        {
            String code =
@"class C1
{
    def foo()
    {
        return = ""C1.foo()"";
    }
}
class C2
{
    private def foo()
    {
        return = ""C2.foo()"";
    }
}
def foo(i)
{
    return = (i > 0) ? C1.C1() : C2.C2();
}
x = foo(-1);
r = x.foo();  
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r", null);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.RuntimeData.WarningID.kMethodResolutionFailure);

        }

        [Test]
        [Category("SmokeTest")]
        public void T94_1467343_2()
        {
            String code =
@"class C1
{
    def foo()
    {
        return = ""C1.foo()"";
    }
}
class C2
{
    private def foo()
    {
        return = ""C2.foo()"";
    }
}
def foo(i)
{
    return = (i > 0) ? C1.C1() : C2.C2();
}
r;
[Imperative]
{
    x = foo(-1);
    r = x.foo();  
}
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r", null);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.RuntimeData.WarningID.kMethodResolutionFailure);
        }

        [Test]
        [Category("SmokeTest")]
        public void T94_1467344_3()
        {
            String code =
@"
class Base
{
    private def foo()
    {
        return = 100;
    }
}
class Derive extends Base
{
    def foo()
    {
        return = 200;
    }
}
def foo:Base()
{
    return = Derive.Derive();
}
b = foo();
r = b.foo();
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r", 200);

        }

        [Test]
        [Category("SmokeTest")]
        public void T94_1467344_4()
        {
            String code =
@"
class Base
{
    private def foo()
    {
        return = 100;
    }
}
class Derive extends Base
{
    def foo()
    {
        return = 200;
    }
}
def foo:Base()
{
    return = Derive.Derive();
}
r;
[Imperative]
{
b = foo();
r = b.foo();
}
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r", 200);
        }

        [Test]
        [Category("SmokeTest")]
        public void T94_1467443()
        {
            String code =
@"
class test
{
    private foo;
    
}
a = test.test();
a.foo = 1;
";
            string error = "Property 'foo' is inaccessible";
            thisTest.VerifyRunScriptSource(code, error);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.RuntimeData.WarningID.kMethodResolutionFailure);
        }

        [Test]
        [Category("SmokeTest")]
        public void T94__imperative_1467443_7()
        {
            String code =
@"
class test
{
    private foo;
    
}
a = test.test();
[Imperative]
{
    a.foo = 1;
}
";
            string error = "Property 'foo' is inaccessible";
            thisTest.VerifyRunScriptSource(code, error);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.RuntimeData.WarningID.kMethodResolutionFailure);
        }

        [Test]
        [Category("SmokeTest")]
        public void T94_1467443_2()
        {
            String code =
@"
class test{    }
a = test.test();
a.b = 1;
";
            string error = "Class 'test' does not have a property 'b'";
            thisTest.VerifyRunScriptSource(code, error);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.RuntimeData.WarningID.kMethodResolutionFailure);
        }

        [Test]
        [Category("SmokeTest")]
        public void T94_imperative_1467443_8()
        {
            String code =
@"
class test{    }
a;
[Imperative]
{
a = test.test();
a.b = 1;
}
";
            string error = "1467443 Error on incorrect set property is not helpful ";
            thisTest.VerifyRunScriptSource(code, error);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.RuntimeData.WarningID.kMethodResolutionFailure);
        }

        [Test]
        [Category("SmokeTest")]
        public void T94_1467443_3()
        {
            String code =
@"
class test
{
    private foo;
    
}
class test1 extends test
{
    
}
a = test.test();
a.foo = 1;
";
            string error = "Property 'foo' is inaccessible";
            thisTest.VerifyRunScriptSource(code, error);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.RuntimeData.WarningID.kMethodResolutionFailure);
        }

        [Test]
        [Category("SmokeTest")]
        public void T94_iperative_1467443_9()
        {
            String code =
@"
class test
{
    private foo;
    
}
class test1 extends test
{
    
}
a;
[Imperative]
{
a = test.test();
a.foo = 1;
}
";
            string error = "Property 'foo' is inaccessible";
            thisTest.VerifyRunScriptSource(code, error);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.RuntimeData.WarningID.kMethodResolutionFailure);
        }

        [Test]
        [Category("SmokeTest")]
        public void T94_1467443_4()
        {
            String code =
@"
class test
{
   
    
}
class test1 extends test
{
     private foo;
}
a = test1.test1();
a.foo = 1;
";
            string error = "Property 'foo' is inaccessible";
            thisTest.VerifyRunScriptSource(code, error);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.RuntimeData.WarningID.kMethodResolutionFailure);
        }

        [Test]
        [Category("SmokeTest")]
        public void T94_imperative_1467443_10()
        {
            String code =
@"
class test
{
   
    
}
class test1 extends test
{
     private foo;
}
a;
[Imperative]
{
a = test1.test1();
a.foo = 1;
}
";
            string error = "Property 'foo' is inaccessible";
            thisTest.VerifyRunScriptSource(code, error);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.RuntimeData.WarningID.kMethodResolutionFailure);
        }

        [Test]
        [Category("SmokeTest")]
        public void T94_1467443_6()
        {
            String code =
@"
class test
{
   
    
}
class test1 extends test
{
     private foo;
}
def foo1()
{
    return = test.test();
}
a = foo1();
a.foo = 1;
";
            string error = "Class 'test' does not have a property 'foo'";
            thisTest.VerifyRunScriptSource(code, error);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.RuntimeData.WarningID.kMethodResolutionFailure);
        }

        [Test]
        [Category("SmokeTest")]
        public void T94_imperative_1467443_11()
        {
            String code =
@"
class test
{
   
    
}
class test1 extends test
{
     private foo;
}
def foo1()
{
    return = test.test();
}
a;
[Imperative]
{
a = foo1();
a.foo = 1;
}
";
            string error = "1467443 Error on incorrect set property is not helpful ";
            thisTest.VerifyRunScriptSource(code, error);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.RuntimeData.WarningID.kMethodResolutionFailure);
        }

        [Test]
        [Category("SmokeTest")]
        public void T95_1467437_1()
        {
            String code =
            @"
          class A
            {
                a;
                constructor A(i)
                {
                    a = i;
                }
            }
            x = (A.A(1..3))[0];
            y=x.a;
            ";
            string error = "DNL-1467618 Regression : Use of the array index after replicated constructor yields complier error now";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("y", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T95_1467437_2()
        {
            String code =
            @"
          class A
            {
                a;
                constructor A(i)
                {
                    a = i;
                }
            }
            x = (A.A(1..3))[0..2];
            y=x.a;
            ";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("y", new object[] { 1, 2, 3 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T95_1467437_3()
        {
            String code =
            @"
          class A
            {
                a;
                constructor A(i)
                {
                    a = i;
                }
            }
def foo(i:int)
{
    return = A.A(i);
}
x = foo(1..3)[0];
y = x.a;
            ";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("y", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T95_1467437_4()
        {
            String code =
            @"
          class A
            {
                a;
                constructor A(i)
                {
                    a = i;
                }
            }
def foo(i:int)
{
    return = A.A(i);
}
x = foo(1..3)[0..2];
y = x.a;
            ";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("y", new object[] { 1, 2, 3 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T95_1467437_5()
        {
            String code =
            @"
          class A
            {
                a;
                constructor A(i)
                {
                    a = i;
                }
            }
            y;
            [Imperative]
            {
                x = (A.A(1..3))[0];
                y=x.a;
            }
            ";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("y", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T95_1467437_6()
        {
            String code =
            @"
          class A
            {
                a;
                constructor A(i)
                {
                    a = i;
                }
            }
            y;
            [Imperative]
            {
            x = (A.A(1..3))[0..2];
            y={x[0].a,x[1].a,x[2].a};
            }
            ";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("y", new object[] { 1, 2, 3 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T95_1467437_7()
        {
            String code =
            @"
          class A
            {
                a;
                constructor A(i)
                {
                    a = i;
                }
            }
          def foo(i:int)
          {
            return = A.A(i);
          }
          x = foo(1..3)[{1,2}];
          y = x.a;
          ";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("y", new object[] { 2, 3 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T96_1467464_1()
        {
            String code =
            @"
                class test
                {
                    f;
                    constructor test()
                    {
                    [Associative]
                        {
                            [Imperative]
                            {
                                i = 3;
                            }
                    // The value of 'i' cannot be inspected here.
                    // If this line is removed, then 'i' can be inspected.    
                            f = i;
                        }
                    }
                }
                a = test.test();
                b = a.f;
            ";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("b", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void T97_1467440_Class_Not_Defined_1()
        {
            String code =
            @"
def foo(x : NonExistClass[][])
{
    return = x;
}
z = foo({ 1, 2 });
            ";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            Object n1 = null;
            thisTest.Verify("z", n1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T97_1467440_Class_Not_Defined_2()
        {
            String code =
            @"
z;
[Imperative]
{
    def foo(x : NonExistClass[][])
    {
        return = x;
    }
    z = foo({ 1, 2 });
}
            ";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            Object n1 = null;
            thisTest.Verify("z", n1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T97_1467440_Class_Not_Defined_3
            ()
        {
            String code =
            @"
z;
[Imperative]
{
    def foo(y: f1, x : f2)
    {
        return = {y,x};
    }
    z = foo( 1, 2);
}
            ";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            Object n1 = null;
            thisTest.Verify("z", n1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T98_1467450_Class_Not_Defined_1
            ()
        {
            String code =
            @"
a1 = A.A( a, b); 
a2 = A.A( 1, 2); 
a3 = A.A({1,2}, c);
a4 = A.A({1,2}, c..d, 0..1, 5..f);
            ";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            Object n1 = null;
            thisTest.Verify("a1", n1);
            thisTest.Verify("a2", n1);
            thisTest.Verify("a3", n1);
            thisTest.Verify("a4", n1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T98_1467450_Class_Not_Defined_2
            ()
        {
            String code =
            @"
a1;a2;a3;a4;
[Imperative]
{
    a1 = A.A( a, b); 
    a2 = A.A( 1, 2); 
    a3 = A.A({1,2}, c);
    a4 = A.A({1,2}, c..d, 0..1, 5..f);
}
            ";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            Object n1 = null;
            thisTest.Verify("a1", n1);
            thisTest.Verify("a2", n1);
            thisTest.Verify("a3", n1);
            thisTest.Verify("a4", n1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T98_1467450_Class_Not_Defined_3
            ()
        {
            String code =
            @"
def foo ()
{
    a1 = A.A( a, b); 
    a2 = A.A( 1, 2); 
    a3 = A.A({1,2}, c);
    //a4 = A.A({1,2}, c..d, 0..1, 5..f);
}
a1 : int;
a2 : int;
a3 : int;
a4 : int;
            ";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            Object n1 = null;
            thisTest.Verify("a1", n1);
            thisTest.Verify("a2", n1);
            thisTest.Verify("a3", n1);
            thisTest.Verify("a4", n1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T98_1467450_Class_Not_Defined_4
            ()
        {
            String code =
            @"
class A
{
    static def foo ()
    {
        a1 = A.A( a, b); 
        a2 = A.A( 1, 2); 
        a3 = A.A({1,2}, c);
        a4 = A.A({1,2}, c..d, 0..1, 5..f);
    }
}
a1 : int;
a2 : int;
a3 : int;
a4 : int;
test = A.foo();
            ";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            Object n1 = null;
            thisTest.Verify("a1", n1);
            thisTest.Verify("a2", n1);
            thisTest.Verify("a3", n1);
            thisTest.Verify("a4", n1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T99_UnnamedConstructor01()
        {
            String code =
            @"
class A
{
    x;
    constructor(i) { x = i; }
}
a = A(41);
r = a.x; 
            ";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r", 41);
        }

        [Test]
        [Category("SmokeTest")]
        [Category("Failure")]
        public void T99_1467469
            ()
        {
            String code =
            @"
class A{ a = 1; }
class B{ a = 1; }
a = A.A();
b = B.B();
c = 1 == 2;
d = a == b;
            ";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3951
            string error = "MAGN-3951 Equality of Pointer types not working properly";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("d", false);
            thisTest.Verify("c", false);
        }

        [Test]
        [Category("SmokeTest")]
        public void T95_1467421()
        {
            String code =
@"
class A
{
    static  count : int = 0;
    constructor A()
    {
        count = count + 1;
    }   
    
}
a1 = A.A();// received - A(count=null), expected: A(count=1)
r = a1.count;
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r", 1);

        }

        [Test]
        [Category("SmokeTest")]
        public void T95_1467421_2()
        {
            String code =
@"
class A
{
    private static  count : int = 0;
    constructor A()
    {
        count = count + 1;
    }   
    
}
a1 = A.A();// received - A(count=null), expected: A(count=1)
r = a1.count;
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r", null);

        }

        [Test]
        [Category("SmokeTest")]
        public void T95_1467421_3()
        {
            String code =
@"
class A
{
   static count : int= 0;
    constructor A()
    {
        count = count + 1;
    }   
    
}
a1 = A.A(); // received - A(count=null), expected: A(count=1)
a1.count = 3;
b1 = A.A();
r = a1.count;
r2 = b1.count;
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r", 4);
            thisTest.Verify("r2", 4);

        }

        [Test]
        [Category("SmokeTest")]
        public void T95_1467421_4()
        {
            String code =
@"
class A
{
   static count : int[]= {0,1};
    constructor A()
    {
        count = count + 1;
    }   
    
}
a1 = A.A(); // received - A(count=null), expected: A(count=1)
a1.count = {3,4,5};
b1 = A.A();
r = a1.count;
r2 = b1.count;
";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3949
            string error = "MAGN-3949 Crash in language with static property update case";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r", new object[] { 4, 5, 6 });
            thisTest.Verify("r2", new object[] { 4, 5, 6 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T95_1467421_5()
        {
            String code =
@"
class A
{
    static  count : int = 0;
    constructor A()
    {
        count = count + 1;
    }   
    
}
r;
[Imperative]
{
a1 = A.A();// received - A(count=null), expected: A(count=1)
r = a1.count;
}
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T95_1467472()
        {
            String code =
@"
class A
{
    static count : int = 0;
    pt;
    constructor A()
    {
        count = count + 1;
        pt = count;
    }
}
a1 = A.A();
r=a1.count;
r2=a1.pt;
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r", 1);
            thisTest.Verify("r2", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T95_1467472_2()
        {
            String code =
@"
class A
{
static count : int = 0;
constructor A()
{
count = count + 1;
}
}
class B extends A
{
pt;
constructor B()
{
pt = count;
}
}
a1 = A.A(); // received - A(count=null), expected: A(count=1)r = a1.count;
r1=a1.count;
b1 = B.B();
r2=b1.pt;
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r1", 1);
            thisTest.Verify("r2", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T95_1467472_3()
        {
            String code =
@"
class A
{
    static count : int = 0;
    pt;
    constructor A()
    {
        count = count + 1;
        pt = count;
    }
}
r;
r2;
[Imperative]
{
a1 = A.A();
r=a1.count;
r2=a1.pt;
}
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r", 1);
            thisTest.Verify("r2", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T95_1467472_4()
        {
            String code =
@"
class A
{
static count : int = 0;
constructor A()
{
count = count + 1;
}
}
class B extends A
{
pt;
constructor B()
{
pt = count;
}
}
r1;r2;
[Imperative]
{
a1 = A.A(); // received - A(count=null), expected: A(count=1)r = a1.count;
r1=a1.count;
b1 = B.B();
r2=b1.pt;
}
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r1", 1);
            thisTest.Verify("r2", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T96_1467078_unnamed_constructor_1()
        {
            String code =
@"
class FixedNode
{
    i : int = 0;
    constructor ( x : int)
    {
        i = x;
    }
    constructor FixedNode( x : int)
    {
        i = x+x;;
    }
}
t = FixedNode.FixedNode(41);
test = t.i; 
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("test", 41);
        }

        [Test]
        [Category("SmokeTest")]
        public void T96_1467078_unnamed_constructor_2()
        {
            String code =
@"
class A
{
    i : int = 0;
    constructor ( x : int)
    {
        i = x;
    }    
}
class B extends A
{
    j : int = 0;
    constructor ( x : int) : base.A(x)
    {
        j = x;
    }    
}
t = B.B(4);
test1 = t.i; 
test2 = t.j; 
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("test1", 4);
            thisTest.Verify("test2", 4);
        }

        [Test]
        [Category("SmokeTest")]
        public void T96_1467078_unnamed_constructor_3()
        {
            String code =
@"
class A
{
    i : int = 0;
    constructor ( x : int)
    {
        i = x;
    }    
}
class B 
{
    j : A;
    constructor ( x : int) 
    {
        j = A.A(x);
    }    
}
t = B.B(4);
test1 = t.j.i; 
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("test1", 4);

        }

        [Test]
        [Category("SmokeTest")]
        public void T96_1467078_unnamed_constructor_4()
        {
            String code =
@"
class A
{
    i : int = 0;
    constructor ( x : int)
    {
        i = x;
    }    
}
class B 
{
    j : A;
    constructor ( x : int) 
    {
        j = A.A(x);
    }    
}
test1;
[Imperative]
{
    t = B.B(4);
    test1 = t.j.i;
}
 
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("test1", 4);
        }

        [Test]
        [Category("SmokeTest")]
        public void T96_1467078_unnamed_constructor_5()
        {
            String code =
@"
class A
{
    i : int = 0;
    constructor ( x : int)
    {
        i = x;
    }    
}
class B 
{
    j : A;
    constructor ( x : int) 
    {
        j = A.A(x);
    }    
}
def foo ()
{
    t = B.B(4);
    return = t.j.i;
}
test1 = foo();
 
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("test1", 4);
        }

        [Test]
        [Category("SmokeTest")]
        public void T97_1467522_Indexing_Class_Properties_1()
        {
            String code =
@"
class B
{
    x : double;
    constructor B(xx)
    {
        x = xx;
    }  
}
class A
{
    Start : B;    
    constructor A(start : B)
    {
        Start = start;
        
    }   
}
def foo(a : A[])
{ 
    result = a[0].Start.x;     
    return = result;
}
walls = { };
walls[0] = A.A(B.B(1));
walls[1] = A.A(B.B(3)); 
test = foo(walls); // received {1,3}; expected : 1
test2 = walls[0].Start.x; // received 1
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("test", 1.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T97_1467522_Indexing_Class_Properties_2()
        {
            String code =
@"
class A
{
    x : int = 0;
    constructor A(xx)
    {
        x = xx;
    }
    def func(t : A)
    {
        return = x + t.x;
    }
    
}
def foo(wall : A[])
{ 
    result = wall[0].func(wall[1]);    
    return = result;
}
wall = { };
wall[0] = A.A(1);
wall[1] = A.A(2);
test1 = foo(wall);//{ null, null} 
test2 = wall[0].func(wall[1]);  // 3; expected test1=test2
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("test1", 3);
        }

        [Test]
        [Category("Failure")]
        public void T98_Class_Static_Property_Using_Global_Variable()
        {
            String code =
@"
t1 = 3;
class A
{
    static a = t1 ;
}
test1 = A.a;
";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1512
            string error = "MAGN-1512 Update issue : when a static property is defined using a global variable, the value is  not getting updated";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("test1", 3);
        }

        [Test]
        public void T98_Class_Static_Property_Using_Other_Properties()
        {
            String code =
@"
class A
{
    b = 3;
    static a = b ;
}
test1 = A.a;
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("test1", null);
            thisTest.VerifyBuildWarningCount(1);
        }

        [Test]
        public void T98_1467571_static_nonstatic_issue()
        {
            String code =
@"
class A
{
    b = 3;
    static c = 4;
    static def a () { return = b; }
    def a2 () { return = c; }
}
test1 = A.a();
aa = A.A();
test2 = aa.a2();
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("test1", null);
            thisTest.Verify("test2", 4);
            thisTest.VerifyBuildWarningCount(1);
        }

        [Test]
        public void T99_1467578_this_imperative()
        {
            String code =
@"
class MyInt
{
    IntValue : int;
    
    constructor Default(intValue : int)
    {
        IntValue = intValue;
    }
    
    def someOperation : MyInt()
    {
        returnValue;
        [Imperative]
        {
            x = this.IntValue + 1;
            returnValue = MyInt.Default(x);
        }
        return = returnValue;
    }        
}
seed = MyInt.Default(1);
val=seed.someOperation().IntValue;
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("val", 2);
            thisTest.VerifyBuildWarningCount(0);
        }

        [Test]
        public void T100_1467578_this_imperative()
        {
            String code =
@"
class MyInt
{
    IntValue : int;
    
    constructor Default(intValue : int)
    {
        IntValue = intValue;
    }
    
    def someOperation : MyInt(array : MyInt[])
    {
        returnValue;
        [Imperative]
        {
            for(m in array)
            {
                if (m == this)
                {
                    returnValue = MyInt.Default(this.IntValue + 1);
                }
                else
                {
                    returnValue = MyInt.Default(0);
                }
            }
        }
    }        
}
startArray = MyInt.Default(1..5);
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.VerifyBuildWarningCount(0);
        }

        [Test]
        public void T110_IDE_884_Testing_Bang_Inside_Imperative()
        {
            String code =
@"
class test
{
    static def foo()
    {
        included = true;  
        return = [Imperative]
        {
            a = !included;
            return = a;                
        }
        return = true;
    }
    def foo2()
    {
        included = true;  
        return = [Imperative]
        {
            a;
            if(!included)
                a = !included;
            else
                a = !(!included);                
            return = a;
        }
        return = false;
     }
     def foo3()
     {
        included = false;  
        return = [Imperative]
        {
            a;
            while(!included)
            {
                a = !included;
                included = !included;
            }                           
            return = a;
        }
        return = false;
     }
     def foo4()
     {
        included = false;  
        return = [Imperative]
        {
            a = {0,1};
            x;
            for(i in a )
            {
                x = !included;
                included = !included;                
            }                           
            return = x;
        }
        return = false;
     }
    
}
test1 = test.foo();
t = test.test();
test2 = t.foo2();
test3 = t.foo3();
test4 = t.foo4();
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.VerifyBuildWarningCount(0);
            thisTest.Verify("test1", false);
            thisTest.Verify("test2", true);
            thisTest.Verify("test3", true);
            thisTest.Verify("test4", false);
        }

        [Test]
        public void T110_IDE_884_Testing_Bang_Inside_Imperative_2()
        {
            String code =
@"
def foo (a)
{
    x = !a;
    return = [Imperative]
    {
        y = !x;
        if(!y == true)
            return = !y;
        else
            return = !x;
    }
}
a = true;
test1 = [Imperative]
{
    return = !a;
}
test2 = [Imperative]
{
    return = [Associative]
    {
        return = [Imperative]
        {
             return = !a;
        }
    }
} 
test3 = foo(a);  
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.VerifyBuildWarningCount(0);
            thisTest.Verify("test1", false);
            thisTest.Verify("test2", false);
            thisTest.Verify("test3", true);

        }

        [Test]
        [Category("SmokeTest")]
        public void T111_Class_Constructor_Negative_1467598()
        {
            String code =
@"
class test
{
    constructor foo()
    {
    }
}
a = test.sum();
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);

            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kFunctionNotFound);
            thisTest.Verify("a", null);


        }

        [Test]
        [Category("SmokeTest")]
        public void T112_1467578_this_imperative()
        {
            String code =
@"
class MyInt
{
    IntValue : int;
    
    constructor Default(intValue : int)
    {
        IntValue = intValue;
    }
    
    def someOperation : MyInt[](array : MyInt[])
    {
        returnValue = { };
        c = 0;
        [Imperative]
        {
            for(m in array)
            {
                if (Equals ( m, this))
                {
                    returnValue[c] = MyInt.Default(this.IntValue + 1);
                }
                else
                {
                    returnValue[c] = MyInt.Default(0);
                }
                c = c + 1;
            }
        }
        return = returnValue;
    }        
}
startArray = MyInt.Default(1..5);
seed = MyInt.Default(1);
test = seed.someOperation(startArray).IntValue;
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("test", new Object[] { 2, 0, 0, 0, 0 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T113_1467599_Type_Conversion()
        {
            String code =
@"
class MyInt
{
    IntValue : int;
    
    constructor Create()
    {
        [Imperative]
        {
              IntValue = true;
        }
    } 
    constructor MyInt()
    {
        [Imperative]
        {
              IntValue = 1;
        }
    }
    def foo ()
    {
        [Imperative]
        {
            [Associative]
            {
                [Imperative]
                {
                    IntValue = true;
                    return = IntValue;
                }
            }
        }
    }
          
}
a = MyInt.Create().IntValue;
b = MyInt.MyInt().foo();
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.VerifyRuntimeWarningCount(2);
            thisTest.Verify("a", null);
            thisTest.Verify("b", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void T114_1467599_Type_Conversion()
        {
            String code =
@"
class MyInt
{
    IntValue : int[];
    
    constructor Create()
    {
        [Imperative]
        {
              IntValue = {true, true};
        }
    } 
    constructor MyInt()
    {
        [Imperative]
        {
              IntValue = {1,1};
        }
    }
    def foo ()
    {
        [Imperative]
        {
            [Associative]
            {
                [Imperative]
                {
                    IntValue = {true,true};
                    //return = IntValue;
                }
            }
        }
        return = IntValue;
    }
          
}
a = MyInt.Create().IntValue;
b = MyInt.MyInt().foo();
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.VerifyRuntimeWarningCount(4);
            thisTest.Verify("a", new Object[] { null, null });
            thisTest.Verify("b", new Object[] { null, null });
        }

        [Test]
        [Category("SmokeTest")]
        public void T115_1467599_Type_Conversion()
        {
            String code =
@"
class MyInt
{
    IntValue : int;
    
    constructor Create()
    {
        [Imperative]
        {
              IntValue = true;
        }
    } 
    def foo()
    {
        return = [Imperative]
        {
              return = MyInt.Create();
        }
    }    
          
}
a = MyInt.Create().IntValue;
b = MyInt.MyInt().foo().IntValue;
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("a", null);
            thisTest.Verify("b", null);
        }

        [Test]
        public void T116_1467599_Type_Conversion()
        {
            String code =
@"
myValue :int = 1; 
x = 1;
[Imperative]
{           
    myValue = myValue + 0.5; //output = 2, but no type conversion message 
    x = x + 0.5;  
}
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.VerifyRuntimeWarningCount(0);
            thisTest.Verify("myValue", 2);
            thisTest.Verify("x", 1.5);
        }

        [Test]
        [Category("SmokeTest")]
        public void T117_1467599_Type_Conversion()
        {
            String code =
@"
class A
{
    myValue : int = 1;
    x = 1;
    constructor A ()
    {    
        [Imperative]
        {           
            myValue = myValue + 0.5;
            x = x + 0.5;
        }
    }
}
a = A.A();
b = a.myValue;
c = a.x;
";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.VerifyRuntimeWarningCount(0);
            thisTest.Verify("b", 2);
            thisTest.Verify("c", 1.5);
        }

        [Test]
        [Category("SmokeTest")]
        public void T118_1467695_setter_inlinecondition()
        {
            String code =
@"
class curve
{
    Color ;
    Length : double;
    constructor curve(t:int)
    {
        length = t;
    }
}
curve1 : curve[]..[];
[Associative]
{
    curve1 = curve.curve(1..10);
    curve1.Color = curve1.Length > 6 ? 1 : 2;
}
";
            string error = "value assigned by a conditional on an array throws error '%set_Color()' is invoked on invalid object ";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.VerifyRuntimeWarningCount(0);

        }
    }
}
