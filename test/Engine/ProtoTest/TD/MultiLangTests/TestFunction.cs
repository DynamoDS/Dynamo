using System;
using System.IO;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;
namespace ProtoTest.TD.MultiLangTests
{
    class TestFunction : ProtoTestBase
    {
        string testPath = "..\\..\\..\\test\\Engine\\ProtoTest\\ImportFiles\\";
        ProtoScript.Config.RunConfiguration runnerConfig;
        ProtoScript.Runners.DebugRunner fsr;

        public override void Setup()
        {
            base.Setup();
            runnerConfig = new ProtoScript.Config.RunConfiguration();
            runnerConfig.IsParrallel = false;
            fsr = new ProtoScript.Runners.DebugRunner(core);
        }

        public override void TearDown()
        {
            base.TearDown();
            fsr = null;
        }

        [Test]
        [Category("SmokeTest")]
        public void T01_Function_In_Assoc_Scope()
        {
            string code = @"
a;
[Associative]
{
    def foo : int( a:int )
    {
	   return = a * 10;
	}
	
    a = foo( 2 );
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 20);

        }

        [Test]
        [Category("SmokeTest")]
        public void T02_Function_In_Imp_Scope()
        {
            string code = @"
a;
[Imperative]
{
    def foo : double( a:double, b : int )
    {
	   return = a * b;
	}
	
    a = foo( 2.5, 2 );
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((double)mirror.GetValue("a").Payload == 5.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T03_Function_In_Nested_Scope()
        {
            string code = @"
a;b;
[Imperative]
{
    def foo : double( a:double, b : int )
    {
	   return = a * b;
	}
	a = 3;
	[Associative]
	{
		a = foo( 2.5, 1 );
	}
	b = 
	[Associative]
	{
		a = foo( 2.5, 1 );
		return = a;
	}
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((double)mirror.GetValue("a").Payload == 2.5);
            Assert.IsTrue((double)mirror.GetValue("b").Payload == 2.5);
        }

        [Test]
        [Category("SmokeTest")]
        public void T04_Function_In_Nested_Scope()
        {
            string code = @"
a;b;
[Associative]
{
    def foo : int( a:int, b : int )
    {
	   return = a * b;
	}
	a = 3.5;
	[Imperative]
	{
		a = foo( 2, 1 );
	}
	b = 
	[Imperative]
	{
		a = foo( 2, 1 );
		return = a;
	}
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T05_Function_outside_Any_Block()
        {
            string code = @"
def foo : int( a:int, b : int )
{
    return = a * b;
}
a = 3.5;
b = 3.5;
[Associative]
{
	a = 3.5;
	[Imperative]
	{
		a = foo( 2, 1 );
	}
	b = 
	[Imperative]
	{
		a = foo( 2, 1 );
		return = a;
	}
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 2);
        }

        [Test]
        [Category("Smoke Test")]
        public void T06_Function_Imp_Inside_Assoc()
        {
            string code = @"
a;b;
[Associative]
{
	def foo : int( a:int, b : int )
	{
		return = a * b;
	}
	a = 3.5;
	b = 3.5;

	[Imperative]
	{
		a = foo( 2, 1 );
	}
	b = 
	[Imperative]
	{
		c = foo( 2, 1 );
		return = c;
	}
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 2);

        }

        [Test]
        [Category("SmokeTest")]
        public void T07_Function_Assoc_Inside_Imp()
        {
            string src = @"a;b;
[Imperative]
{
	def foo : int( a:int, b : int )
	{
		return = a * b;
	}
	a = 3.5;
	b = 3.5;
	[Associative]
	{
		a = foo( 2, 1 );
	}
	b = 
	[Associative]
	{
		c = foo( 2, 1 );
		return = c;
	}
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);

            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T08_Function_From_Inside_Class_Constructor()
        {
            string code = @"
def add_1 : int( a:int )
{
	return = a + 1;
}
class A
{
	a : var;
	constructor CreateA ( a1 : int )
	{
		a = add_1(a1);
	}
}
a;b;
[Imperative]
{
	arg = 1;
	b = 1;
	
    [Associative]
	{
		A_inst = A.CreateA( arg );
		a = A_inst.a;
	}
    [Associative]
    {
        b = [Imperative]
        {
            A_inst = A.CreateA( arg );
            return = A_inst.a;
        }
    }
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetFirstValue("a").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetFirstValue("b").Payload == 2);

        }

        [Test]
        [Category("SmokeTest")]
        public void T09_Function_From_Inside_Class_Constructor()
        {
            string code = @"
def add_1 : int( a:int )
{
	return = a + 1;
}
class A
{
	a : var;
	constructor CreateA ( a1 : int )
	{
		a = add_1(a1);
	}
}
a;b;
[Associative]
{
	arg = 1;
	b = 1;
    [Imperative]
    {
        [Associative]
        {
            A_inst = A.CreateA( arg  );
            a = A_inst.a;
        }
    }
	b = 
	[Imperative]
	{
		A_inst = A.CreateA( arg  );
		return = A_inst.a;
	}
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetFirstValue("a").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetFirstValue("b").Payload == 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T10_Function_From_Inside_Class_Method()
        {
            string code = @"
def add_1 : int( a:int )
{
	return = a + 1;
}
class A
{
	a : var;
	constructor CreateA ( a1 : int )
	{
		a = add_1(a1);
	}
	def add1 : int(  )
    {
	    return = add_1(a);
    }
}
a;b;
[Associative]
{
	arg = 1;
	b = 1;
    [Imperative]
    {
        [Associative]
        {
            A_inst = A.CreateA( arg  );
            a = A_inst.add1();
        }
    }
	b = 
	[Imperative]
	{
		A_inst = A.CreateA( arg  );
		return = A_inst.add1();
	}
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetFirstValue("a").Payload == 3);
            Assert.IsTrue((Int64)mirror.GetFirstValue("b").Payload == 3);
        }

        [Test]
        [Category("SmokeTest")]
        public void T11_Function_From_Inside_Class_Method()
        {
            string code = @"
def add_1 : int( a:int )
{
	return = a + 1;
}
class A
{
	a : var;
	constructor CreateA ( a1 : int )
	{
		a = add_1(a1);
	}
	def add1 : int(  )
    {
	    return = add_1(a);
    }
}
a;b;
[Imperative]
{
	arg = 1;
	b = 1;
	[Associative]
	{
		A_inst = A.CreateA( arg );
		a = A_inst.add1();
	}
    b = [Associative]
    {
        return = [Imperative]
        {
            A_inst = A.CreateA( arg );
            return = A_inst.add1();
        }
    }
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetFirstValue("a").Payload == 3);
            Assert.IsTrue((Int64)mirror.GetFirstValue("b").Payload == 3);
        }

        [Test]
        [Category("SmokeTest")]
        public void T12_Function_From_Inside_Function()
        {
            string code = @"
def add_1 : double( a:double )
{
	return = a + 1;
}
a;b;
[Associative]
{
	def add_2 : double( a:double )
	{
		return = add_1( a ) + 1;
	}
	
	a = 1.5;
	b = add_2 (a );
	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((double)mirror.GetValue("a").Payload == 1.5);
            Assert.IsTrue((double)mirror.GetValue("b").Payload == 3.5);
        }

        [Test]
        [Category("SmokeTest")]
        public void T13_Function_From_Inside_Function()
        {
            string code = @"
a;b;
def add_1 : double( a:double )
{
	return = a + 1;
}
[Imperative]
{
	def add_2 : double( a:double )
	{
		return = add_1( a ) + 1;
	}
	
	a = 1.5;
	b = add_2 (a );
	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((double)mirror.GetValue("a").Payload == 1.5);
            Assert.IsTrue((double)mirror.GetValue("b").Payload == 3.5);
        }

        [Test]
        [Category("SmokeTest")]
        public void T14_Function_Recursive_imperative()
        {
            string code = @"
a;b;
[Imperative]
{
	def factorial : int( n : int )
	{
		if ( n > 1 ) 
		{
		    return = n * factorial ( n - 1 );
		}
		else 
		{
		    return = 1;
		}		
	}
	
	a = 3;
	b = factorial (a );
	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 3);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 6);
        }

        [Test]
        [Category("SmokeTest")]
        public void T18_Function_Recursive_associative()
        {
            string code = @"
a;b;
[Imperative]
{
	def factorial : int( n : int )
	{
		if ( n > 1 ) 
		{
		    return = n * factorial ( n - 1 );
		}
		else 
		{
		    return = 1;
		}		
	}
	
	a = 3;
	b = factorial (a );
	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 3);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 6);
        }

        [Test]
        [Category("SmokeTest")]
        public void T15_Function_From_Parallel_Blocks()
        {
            //Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            //{
            string code = @"
a;b;
[Imperative]
{
	def factorial : int( n : int )
	{
		if ( n > 1 ) 
		{
		    return = n * factorial ( n - 1 );
		}
		else 
		{
		    return = 1;
		}		
	}
	
	
	
}
[Imperative]
{
	a = 3;
	b = factorial (a );
	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue(mirror.GetFirstValue("b").DsasmValue.IsNull);
            //});

        }

        [Test]
        [Category("SmokeTest")]
        public void T16_Function_From_Parallel_Blocks()
        {
            //Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            //{
            string code = @"
a;b;
[Imperative]
{
	def factorial : int( n : int )
	{
		if ( n > 1 ) 
		{
		    return = n * factorial ( n - 1 );
		}
		else 
		{
		    return = 1;
		}		
	}
	
	
	
}
[Associative]
{
	a = 3;
	b = factorial (a );
	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue(mirror.GetFirstValue("b").DsasmValue.IsNull);
            //});
        }

        [Test]
        [Category("SmokeTest")]
        public void T17_Function_From_Parallel_Blocks()
        {
            //Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            //{
            string code = @"
a;b;
[Associative]
{
	def foo : int( n : int )
	{
		return = n * n;	
	}
	
	
	
}
[Associative]
{
	a = 3;
	b = foo (a );
	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue(mirror.GetFirstValue("b").DsasmValue.IsNull);
            //});
        }

        [Test]
        [Category("SmokeTest")]
        public void T19_Function_From_Imperative_While_And_For_Loops()
        {
            string code = @"
x;y;
[Imperative]
{
	def foo : int( n : int )
	{
		return = n * n;	
	}
	
	a = { 0, 1, 2, 3, 4, 5 };
	x = 0;
	for ( i in a )
	{
	    x = x + foo ( i );
	}
	
	y = 0;
	j = 0;
	while ( a[j] <= 4 )
	{
	    y = y + foo ( a[j] );
		j = j + 1;
	}
	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 55);
            Assert.IsTrue((Int64)mirror.GetValue("y").Payload == 30);
        }


        [Test]
        [Category("SmokeTest")]
        public void T20_Function_From_Imperative_If_Block()
        {
            string code = @"
x;y;z;
[Associative]
{
	def foo : int( n : int )
	{
		return = n * n;	
	}
	
	[Imperative]
	{
	
		a = { 0, 1, 2, 3, 4, 5 };
		x = 0;
		for ( i in a )
		{
			x = x + foo ( i );
		}
		
		y = 0;
		j = 0;
		while ( a[j] <= 4 )
		{
			y = y + foo ( a[j] );
			j = j + 1;
		}
		
		z = 0;
		
		if( x == 55 )
		{
		    x = foo (x);
		}
		
		if ( x == 50 )
		{
		    x = 2;
		}
		elseif ( y == 30 )
		{
		    y = foo ( y );
		}
		
		if ( x == 50 )
		{
		    x = 2;
		}
		elseif ( y == 35 )
		{
		    x = 3; 
		}
		else
		{
		    z = foo (5);
		}
	}
	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 3025);
            Assert.IsTrue((Int64)mirror.GetValue("y").Payload == 900);
            Assert.IsTrue((Int64)mirror.GetValue("z").Payload == 25);
        }

        [Test]
        [Category("SmokeTest")]
        public void T21_Function_From_Nested_Imperative_Loops()
        {
            string code = @"
x;y;
[Imperative]
{
	def foo : int( n : int )
	{
		return = n ;	
	}
	
	a = { 0, 1, 2, 3, 4, 5 };
	x = 0;
	for ( i in a )
	{
	    for ( j in a )
		{
		    x = x + foo ( j );
		}
	}
	
	y = 0;
	j = 0;
	while ( j <= 4 )
	{
	    p = 0;
		while ( p <= 4)
		{
		    y = y + foo ( a[p] );
			p = p + 1;
		}
		j = j + 1;
	}
	
	if( x < 100 )
	{
	    if ( x > 20 )
		{
		    x = x + foo (x );
		}
	}
	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 180, 0);
            thisTest.Verify("y", 50, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T22_Function_Call_As_Instance_Arguments()
        {
            string code = @"
A1;A2;
class A
{
	a : var;
	constructor CreateA ( a1 : int )
	{
		a = a1;
	}
	
	constructor CreateB ( a1 : double )
	{
		a = a1;
	}
}
[Associative]
{
	def foo : int( n : int )
	{
		return = n ;	
	}
	
	def foo2 : double( n : double )
	{
		return = n ;	
	}
	
	A1 = A.CreateA(foo(foo(1))).a;	
	A2 = A.CreateB(foo2(foo(1))).a;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("A1", 1);
            thisTest.Verify("A2", 1.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T23_Function_Call_As_Function_Call_Arguments()
        {
            string code = @"
c1;
[Associative]
{
	def foo : double ( a : double , b :double )
	{
		return = a + b ;	
	}
	
	def foo2 : double ( a : double , b :double )
	{
		return = foo ( a , b ) + foo ( a, b );	
	}
	
	a1 = 2;
	b1 = 4;
	c1 = foo2( foo (a1, b1 ), foo ( a1, foo (a1, b1 ) ) );
	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c1", 28.0, 0);

        }

        [Test]
        [Category("SmokeTest")]
        public void T24_Function_Call_In_Range_Expression()
        {
            // Assert.Fail("1463472 - Sprint 20 : rev 2112 : Function calls are not working inside range expressions in Associative scope ");
            string code = @"
a1;a2;a3;a4;
[Associative]
{
	def foo : double ( a : double , b :double )
	{
		return = a + b ;	
	}
	
	a1 = 1..foo(2,3)..foo(1,1);
	a2 = 1..foo(2,3)..#foo(1,1);
	a3 = 1..foo(2,3)..~foo(1,1);
	a4 = { foo(1,0), foo(1,1), 3 };
	
}
[Imperative]
{
	def foo_2 : double ( a : double , b :double )
	{
		return = a + b ;	
	}
	
	a1 = 1..foo_2(2,3)..foo_2(1,1);
	a2 = 1..foo_2(2,3)..#foo_2(1,1);
	a3 = 1..foo_2(2,3)..~foo_2(1,1);
	a4 = { foo_2(1,0), foo_2(1,1), 3 };
	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedResult1 = { 1.0, 2.0, 3.0 };
            object[] expectedResult2 = { 1.0, 3.0, 5.0 };
            object[] expectedResult3 = { 1.0, 5.0 };
            thisTest.Verify("a1", expectedResult2);
            thisTest.Verify("a2", expectedResult3);
            thisTest.Verify("a3", expectedResult2);
            thisTest.Verify("a4", new object[] { 1.0, 2.0, 3 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T25_Function_Call_In_Mathematical_And_Logical_Expr()
        {
            string code = @"
a1;a2;a3;a4;
[Associative]
{
	def foo : double ( a : double , b :double )
	{
		return = a + b ;	
	}
	
	a1 = 1 + foo(2,3);
	a2 = 2.0 / foo(2,3);
	a3 = 1 && foo(2,2);	
}
[Imperative]
{
	def foo_2 : double( a : double , b :double )
	{
		return = a + b ;	
	}
	
	a1 = 1 + foo_2(2,3);
	a2 = 2.0 / foo_2(2,3);
	a3 = 1 && foo_2(2,2);
	a4 = 0;
	
	if( foo_2(1,2) > foo_2(1,0) )
	{
	    a4 = 1;
	}
	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a1", 6.0);
            thisTest.Verify("a2", 0.4);
            thisTest.Verify("a3", true);
            thisTest.Verify("a4", 1);

        }

        [Test]
        [Category("SmokeTest")]
        public void T26_Function_Call_In_Mathematical_And_Logical_Expr()
        {
            string code = @"
x;a4;
[Imperative]
{
	def foo_2 : double ( a : double , b :double )
	{
		return = a + b ;	
	}
	a4 = 0;
	if( foo_2(1,2) < foo_2(1,0) )
	{
	    a4 = 1;
	}
	elseif( foo_2(1,2) && foo_2(1,0) )
	{
	    a4 = 2;
	}
	
	x = 0;	
	while (x < foo_2(1,2))
	{
	    x = x + 1;
	}
	
	c = { 0, 1, 2 };
	for (i in c )
	{
		if( foo_2(1,2) > foo_2(1,0) )
		{
		    x = x + 1;
		}
	}
	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 6, 0);
            thisTest.Verify("a4", 2, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T27_Function_Call_Before_Declaration()
        {
            string code = @"
[Associative]
{ 
	def Level1 : int (a : int) 
	{  
		return = Level2(a+1); 
	}  
	def Level2 : int (a : int) 
	{  
		return = a + 1; 
	} 
	input = 3; 
	result = Level1(input); 
}
[Imperative]
{ 
	a = foo(1); 
	def foo : int (a : int)
	{
		return = a + 1; 
	}
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

        }

        [Test]
        [Category("SmokeTest")]
        public void T29_Function_With_Different_Arguments()
        {
            string code = @"
y;
[Imperative]
{ 
	 def foo : double ( a : int, b : double, c : bool, d : int[], e : double[][], f:int[]..[], g : bool[] )
	 {
	     x = -1;
		 if ( c == true && g[0] == true)
		 {
		     x = x + a + b + d[0] + e[0][0];
		 }
		 else
		 {
		     x = 0;
		 }
         return  = x;
	 }
	 
	 a = 1;
	 b = 1;
	 c = true;
	 d = { 1, 2 };
	 e = { { 1, 1 }, {2, 2 } } ;
	 f = { {0, 1}, 2 };
	 g = { true, false };
	 
	 y = foo ( a, b, c, d, e, f, g );
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("y", 3.0, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T30_Function_With_Mismatching_Return_Type()
        {
            //Assert.Throws(typeof(ProtoCore.Exceptions.RuntimeException), () =>
            //{
            string code = @"
b1;
[Imperative]
{ 
	 def foo1 : double ( a : double )
	 {
	    return = true;
	 }
	 
	 dummyArg = 1.5;
	
	b1 = foo1 ( dummyArg );
	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue(mirror.GetValue("b1").DsasmValue.IsNull);
            //});
        }

        [Test]
        [Category("SmokeTest")]
        public void T31_Function_With_Mismatching_Return_Type()
        {
            string code = @"
b2;
[Imperative]
{ 
	 def foo2 : double ( a : double )
	 {
	    return = 5;
	 }
	 
	dummyArg = 1.5;
	
	b2 = foo2 ( dummyArg );
	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b2", 5.0, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T32_Function_With_Mismatching_Return_Type()
        {
            //Assert.Throws(typeof(ProtoCore.Exceptions.RuntimeException), () =>
            //{
            string code = @"
b2;
[Imperative]
{ 
	 def foo3 : int ( a : double )
	 {
	    return = 5.5;
	 }
	 
	dummyArg = 1.5;
	
	b2 = foo3 ( dummyArg );
	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b2", 6, 0);
            //});
        }

        [Test]
        [Category("SmokeTest")]
        public void T33_Function_With_Mismatching_Return_Type()
        {
            //Assert.Throws(typeof(ProtoCore.Exceptions.RuntimeException), () =>
            //{
            //Assert.Fail("1467156 - Sprint 25 - Rev 3026 type checking of return types at run time ");
            string code = @"
class A
{
    a : int;
	constructor A1(a1 : int)
	{
	    a = a1;
	}
}
b2;
[Imperative]
{ 
	 def foo3 : int ( a : double )
	 {
	    temp = A.A1(1);
		return = temp;
	 }
	 
	dummyArg = 1.5;
	
	b2 = foo3 ( dummyArg );	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue(mirror.GetValue("b2").DsasmValue.IsNull);
            //});
        }

        [Test]
        [Category("SmokeTest")]
        public void T34_Function_With_Mismatching_Return_Type()
        {
            //Assert.Throws(typeof(ProtoCore.Exceptions.RuntimeException), () =>
            //{
            string code = @"
b1;
[Associative]
{ 
	 def foo1 : double ( a : double )
	 {
	    return = true;
	 }
	 
	 dummyArg = 1.5;
	
	b1 = foo1 ( dummyArg );
	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue(mirror.GetValue("b1").DsasmValue.IsNull);
            //});
        }

        [Test]
        [Category("SmokeTest")]
        public void T35_Function_With_Mismatching_Return_Type()
        {
            string code = @"
b2;
[Associative]
{ 
	 def foo2 : double ( a : double )
	 {
	    return = 5;
	 }
	 
	dummyArg = 1.5;
	
	b2 = foo2 ( dummyArg );
	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b2", 5.0, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T36_Function_With_Mismatching_Return_Type()
        {
            //Assert.Throws(typeof(ProtoCore.Exceptions.RuntimeException), () =>
            //{
            string code = @"
b2;
[Associative]
{ 
	 def foo3 : int ( a : double )
	 {
	    return = 5.5;
	 }
	 
	dummyArg = 1.5;
	
	b2 = foo3 ( dummyArg );
	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b2", 6, 0);
            //});
        }

        [Test]
        [Category("SmokeTest")]
        public void T37_Function_With_Mismatching_Return_Type()
        {
            //Assert.Fail("DNL-1467208 Auto-upcasting of int -> int[] is not happening on function return");
            //Assert.Throws(typeof(ProtoCore.Exceptions.RuntimeException), () =>
            //{
            //Assert.Fail("1467156 - Sprint 25 - Rev 3026 type checking of return types at run time ");
            string code = @"
class A
{
    a : int;
	constructor A1(a1 : int)
	{
	    a = a1;
	}
}
b2;
[Associative]
{ 
	 def foo3 : int ( a : double )
	 {
	    temp = A.A1(1);
		return = temp;
	 }
	 
	dummyArg = 1.5;
	
	b2 = foo3 ( dummyArg );	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue(mirror.GetValue("b2").DsasmValue.IsNull);
            //});
        }

        [Test]
        public void T38_Function_With_Mismatching_Return_Type()
        {
            //Assert.Throws(typeof(ProtoCore.Exceptions.RuntimeException), () =>
            //{
            //Assert.Fail("1467156 - Sprint 25 - Rev 3026 type checking of return types at run time ");
            string code = @"
b2;
[Associative]
{ 
	 def foo3 : int[] ( a : double )
	 {
	    return = a;
	 }
	 
	dummyArg = 1.5;
	
	b2 = foo3 ( dummyArg );	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b2", new Object[] { 2 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T39_Function_With_Mismatching_Return_Type()
        {
            //Assert.Fail("1467156 - Sprint 25 - Rev 3026 type checking of return types at run time ");
            string code = @"
b2;
[Associative]
{ 
	 def foo3 : int ( a : double )
	 {
	    return = {1, 2};
	 }
	 
	dummyArg = 1.5;
	
	b2 = foo3 ( dummyArg );	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object v1 = null;
            thisTest.Verify("b2", v1);
        }

        [Test]
        [Category("Type System")]
        public void T40_Function_With_Mismatching_Return_Type()
        {
            string code = @"
[Associative]
{ 
	 def foo3 : int[][] ( a : double )
	 {
	    return = { {2.5}, {3.5}};
	 }
	 
	dummyArg = 1.5;
	
	b2 = foo3 ( dummyArg );	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

        }

        [Test]
        [Category("Type System")]
        public void T41_Function_With_Mismatching_Return_Type()
        {
            string code = @"
[Associative]
{ 
	 def foo3 : int[][] ( a : double )
	 {
	    return = { {2.5}, 3};
	 }
	 
	dummyArg = 1.5;
	
	b2 = foo3 ( dummyArg );	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

        }

        [Test]
        [Category("SmokeTest")]
        public void T42_Function_With_Mismatching_Return_Type()
        {
            string code = @"
b2;
[Associative]
{ 
	 def foo3 : bool[]..[] ( a : double )
	 {
	    return = { {2}, 3};
	 }
	 
	dummyArg = 1.5;
	
	b2 = foo3 ( dummyArg );	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b2", new object[] { new object[] { true }, true });
        }

        [Test]
        [Category("SmokeTest")]
        public void T43_Function_With_Matching_Return_Type()
        {
            string code = @"
b2;
[Associative]
{ 
	 def foo3 : int[]..[] ( a : double )
	 {
	    return = { { 0, 2 }, { 1 } };
	 }
	 
	dummyArg = 1.5;
	
	b2 = foo3 ( dummyArg )[0][0];	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b2", 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T44_Function_With_Null_Argument()
        {
            string code = @"
b2;
[Imperative]
{ 
	 def foo : double ( a : double )
	 {
	    return = 1.5;
     }
	
	 b2 = foo ( null );	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b2", 1.5, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T45_Function_With_Mismatching_Argument_Type()
        {
            string code = @"
b2;
[Associative]
{ 
	 def foo : double ( a : double )
	 {
	    return = 1.5;
     }
	
	 b2 = foo ( 1 );	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b2", 1.5, 0);

        }

        [Test]
        [Category("SmokeTest")]
        public void T46_Function_With_Mismatching_Argument_Type()
        {
            string code = @"
c;
[Imperative]
{ 
	 def foo : double ( a : int )
	 {
	    return = 1.5;
     }
	
	 b2 = foo ( 1.5);
     c = 3;	 
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", 3, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T47_Function_With_Mismatching_Argument_Type()
        {
            string code = @"
c;
[Associative]
{ 
	 def foo : double ( a : double )
	 {
	    return = 1.5;
     }
	
	 b2 = foo ( true);	
	 c = 3;	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", 3, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T48_Function_With_Mismatching_Argument_Type()
        {
            string code = @"
c;
class A
{
    a : int;
	constructor A1(a1 : int)
	{
	    a = a1;
	}
}
[Associative]
{ 
	 def foo : double ( a : int )
	 {
	    return = 1.5;
     }
	 a = A.A1(1);
	 b2 = foo ( a);	
	 c = 3;	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", 3, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T49_Function_With_Matching_Return_Type()
        {
            string code = @"
c;
class A
{
    a : int;
	constructor A1(a1 : int)
	{
	    a = a1;
	}
}
[Associative]
{ 
	 def foo : A ( x : A )
	 {
	    return = x;
     }
	 aa = A.A1(1);
	 b2 = foo ( aa).a;
	 c = 3;	
	 
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", 3, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T50_Function_With_Mismatching_Argument_Type()
        {
            string code = @"
c;
[Associative]
{ 
	 def foo : double ( a : int[] )
	 {
	    return = 1.5;
     }
	 aa = { };
	 b2 = foo ( aa );	
	 c = 3;	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", 3, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T51_Function_With_Mismatching_Argument_Type()
        {
            string code = @"
c;
[Associative]
{ 
	 def foo : double ( a : double[] )
	 {
	    return = 1.5;
     }
	 aa = {1, 2 };
	 b2 = foo ( aa );	
	 c = 3;	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", 3, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T52_Function_With_Mismatching_Argument_Type()
        {
            string code = @"
c;
[Associative]
{ 
	 def foo : double ( a : double[] )
	 {
	    return = 1.5;
     }
	 aa = 1.5;
	 b2 = foo ( aa );	
	 c = 3;	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", 3, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T53_Function_Updating_Argument_Values()
        {
            string code = @"
aa;b2;c;
[Associative]
{ 
	 def foo : double ( a : double )
	 {
	    a = 4.5;
		return = a;
     }
	 aa = 1.5;
	 b2 = foo ( aa );	
	 c = 3;	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aa", 1.5, 0);
            thisTest.Verify("b2", 4.5, 0);
            thisTest.Verify("c", 3, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T54_Function_Updating_Argument_Values()
        {
            string code = @"
aa;b2;c;
[Imperative]
{ 
	 def foo : double ( a : double )
	 {
	    a = 4.5;
		return = a;
     }
	 aa = 1.5;
	 b2 = foo ( aa );	
	 c = 3;	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aa", 1.5, 0);
            thisTest.Verify("b2", 4.5, 0);
            thisTest.Verify("c", 3, 0);
        }

        [Test]
        [Category("Type System")]
        public void T55_Function_Updating_Argument_Values()
        {

            string code = @"
aa;b2;c;
[Imperative]
{ 
	 def foo : int ( a : double )
	 {
	    a = 5;
		return = a;
     }
	 aa = 5.0;
	 b2 = foo ( aa );	
	 c = 3;	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aa", 5.0, 0);
            thisTest.Verify("b2", 5, 0);
            thisTest.Verify("c", 3, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T56_Function_Updating_Argument_Values()
        {
            string code = @"
aa;bb;c;
[Associative]
{ 
	 def foo : int[] ( a : int[] )
	 {
	    a[0] = 0;
		return = a;
     }
	 aa = { 1, 2 };
	 bb = foo ( aa );	
	 
	 c = 3;	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedResult1 = { 1, 2 };
            object[] expectedResult2 = { 0, 2 };
            thisTest.Verify("aa", expectedResult1, 0);
            thisTest.Verify("bb", expectedResult2, 0);
            thisTest.Verify("c", 3, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T57_Function_Using_Local_Var_As_Same_Name_As_Arg()
        {
            string code = @"
aa;bb;c;
[Associative]
{ 
	 def foo : int ( a : int )
	 {
	    a = 3;
		b = a + 1;
		return = b;
     }
	 
	 aa = 1;
	 bb = foo ( aa );
     c = 3;	 
	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aa", 1, 0);
            thisTest.Verify("bb", 4, 0);
            thisTest.Verify("c", 3, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T58_Function_With_No_Argument()
        {
            string code = @"
c1;c2;
     def foo : int (  )
	 {
	    return = 3;
     }
	 
	 [Associative]
	 { 
		c1 = foo();	
     }
	 
	 [Imperative]
	 { 
		c2 = foo();	
     }
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("c1", 3);
            thisTest.Verify("c2", 3);
        }

        [Test]
        [Category("SmokeTest")]
        public void T59_Function_With_No_Return_Stmt()
        {
            string code = @"
c;
     def foo : int ( a : int )
	 {
	    b = a + 1;
     }
	 
	 [Associative]
	 { 
		c = foo(1);	
     }
	 
	 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue(mirror.GetValue("c").DsasmValue.IsNull);
        }

        [Test]
        [Category("SmokeTest")]
        public void T60_Function_With_No_Return_Stmt()
        {
            string code = @"
c;
     def foo : int ( a : int )
	 {
	    b = a + 1;
     }
	 
	 [Imperative]
	 { 
		c = foo(1);	
     }
	 
	 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue(mirror.GetValue("c").DsasmValue.IsNull);

        }

        [Test]
        [Category("SmokeTest")]
        public void T61_Function_With_Void_Return_Stmt()
        {
            //Assert.Fail("1463474 - Sprint 20 : rev 2112 : negative case: when user tries to create a void function DS throws ArgumentOutOfRangeException ");

            string code = @"
[Associative]
{
	 a = 1;
	 [Imperative]
	 {
		def foo : void  ( )
		{
			a = 2;		
		}
		foo();
        b = a;	    
	 }
}
	 
	 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

        }

        [Test]
        [Category("SmokeTest")]
        public void T62_Function_Modifying_Globals_Values()
        {
            //Assert.Fail("1461363 - Sprint 19 : rev 1800 : Global variable support is not there in function scope ");
            string code = @"
a;b;
[Associative]
{
	 a = 1;
	 [Imperative]
	 {
		def foo : int  ( )
		{
			c = a;
			a = 2;	
                        return = c + 1;			
		}
		b = foo();
            
	 }
}
x = 1;
def foo2 : int  ( )
{
    y = x;
    x = 2;	
    return = y + 1;			
}
z = foo2();
	 
	 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 2);
            thisTest.Verify("b", 2);
            thisTest.Verify("x", 2);
            thisTest.Verify("z", 3);
        }

        [Test]
        [Category("SmokeTest")]
        public void T63_Function_Modifying_Globals_Values()
        {
            //Assert.Fail("1465794 - Sprint 22 : rev 2359 : Global variable support is not there in purely Imperative scope"); 

            string code = @"
x = [Imperative]
{
	a = 1;
	def foo : int  ( )
	{
		c = a;
		a = 2;	
                return = c + 1;			
	}
	b = foo();
        return = { a, b };    
}
	 
	 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 2, 2 };
            thisTest.Verify("x", v1);

        }

        [Test]
        [Category("SmokeTest")]
        public void T64_Function_Modifying_Globals_Values_Negative()
        {
            string code = @"
a;b;c;
[Imperative]
{
	def foo : int  ( )
	{
		c = a;
		a = 2;	
                return = c + 1;			
	}
	a = 1;
	b = foo();
	c = 3;
            
}
	 
	 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1465794 - Sprint 22 : rev 2359 : Global variable support is not there in purely Imperative scope"); 
            thisTest.Verify("a", 2);
            thisTest.Verify("b", 2);
            thisTest.Verify("c", 3);


        }

        [Test]
        [Category("SmokeTest")]
        public void T65_Function_With_No_Return_Type()
        {
            string code = @"
a;b;
[Imperative]
{
	def foo (  )
	{
		return = true;			
	}
	
	a = foo();
	b = 3;
            
}
	 
	 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", true, 0);
            thisTest.Verify("b", 3, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T66_Function_Returning_Null()
        {
            string code = @"
a;b;c;
[Imperative]
{
	def foo : int ( a : int )
	{
		c = d + a;
        return = c;		
	}
	
	a = 1;
	b = foo(a);
	c = b + 2;
            
}
	 
	 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1, 0);
            Assert.IsTrue(mirror.GetValue("b").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("c").DsasmValue.IsNull);
        }

        [Test]
        [Category("SmokeTest")]
        public void T67_Function_Returning_Collection()
        {
            string code = @"
a;b;
[Imperative]
{
	def foo : int[] ( a : int )
	{
		c = { a + 1, a + 2.5 };
        return = c;		
	}
	
	a = 1;
	b = foo(a);
            
}
	 
	 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            object[] expectedResult = { 2, 4 };
            thisTest.Verify("b", expectedResult, 0);
            thisTest.Verify("a", 1, 0);

        }

        [Test]
        [Category("SmokeTest")]
        public void T68_Function_Returning_Null()
        {
            string code = @"
a;b;
[Imperative]
{
	def foo : int[] ( a : int )
	{
		c = { a + 1, a + 2.5 };
        return = null;		
	}
	
	a = 1;
	b = foo(a);
            
}
	 
	 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue(mirror.GetValue("b").DsasmValue.IsNull);
            thisTest.Verify("a", 1, 0);
        }

        [Test] //Fuqiang: this is changed due to the implementation of function pointer
        [Category("SmokeTest")]
        public void T69_Function_Name_Checking()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
[Imperative]
{
	def foo : int ( a : int )
	{
		foo = 3;
		return  = a + foo;
	}
	
	a = 1;
	foo = 2;
	b = foo(a);           
}
	 
	 
";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
                //thisTest.Verify("a", 1, 0);
                //thisTest.Verify("a", 4, 0);
                //thisTest.Verify("foo", 2, 0);
            });
        }

        [Test] //Fuqiang: this is changed due to the implementation of function pointer
        [Category("SmokeTest")]
        public void T70_Function_Name_Checking()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
[Associative]
{
	def foo : int ( a : int )
	{
		foo = 4;
		return  = a + foo;
	}
    foo = 1;
	[Imperative]
	{
		def foo : int ( a : int )
		{
			foo = 3;
			return  = a + foo;
		}
		
		a = 1;
		foo = 2;
		b = foo(a);           
	}
}
	 
	 
";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
                //thisTest.Verify("a", 1, 0);
                //thisTest.Verify("b", 4, 0);
                //thisTest.Verify("foo", 2, 0);
            });
        }

        [Test] //Fuqiang: this is changed due to the implementation of function pointer
        [Category("SmokeTest")]
        public void T71_Function_Name_Checking()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
[Associative]
{
    foo = 1;
}
[Imperative]
{
	def foo : int ( a : int )
	{
		foo = 3;
		return  = a + foo;
	}
		
	a = 1;
	foo = 2;
	b = foo(a);  
    c = foo + 1;	
}
	 
	 
";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
                //thisTest.Verify("a", 1, 0);
                //thisTest.Verify("foo", 2, 0);
                //thisTest.Verify("c", 3, 0);
                //thisTest.Verify("b", 4, 0);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T72_Function_Name_Checking()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
class foo
{
    a : int;
	constructor foo ( b : int )
	{
	    a = b;
	}
}
[Associative]
{
    def foo : int ( a : int )
	{
		foo = 3;
		return  = a + foo;
	}
	foo = 1;
	x = foo(foo);
	y = foo.foo(foo);
	z = y.a;
}
	 
	 
";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T73_Function_Name_Checking_Negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
[Associative]
{
    def _foo : int ( a : int )
	{
		return  = a + 1;
	}
	
	x = _foo(1);
	
}
[Associative]
{
    def 12foo : int ( a : int )
	{
		return  = a + 1;
	}
	
	x = 12foo(1);
	
}
	 
	 
";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void T74_Function_With_Simple_Replication_Imperative()
        {
            string code = @"
x;y;
[Imperative]
{
    def foo : int ( a : int )
	{
		return  = a + 1;
	}
	
	x = { 1, 2, 3 };
	y = foo(x);
	
}	 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedResult1 = { 1, 2, 3 };
            object[] expectedResult2 = { 2, 3, 4 };
            thisTest.Verify("y", expectedResult2);
            thisTest.Verify("x", expectedResult1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T74_Function_With_Simple_Replication_Associative()
        {
            string code = @"
x;y;
[Associative]
{
    def foo : int ( a : int )
	{
		return  = a + 1;
	}
	
	x = { 1, 2, 3 };
	y = foo(x);
	
}
	 
	 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedResult1 = { 1, 2, 3 };
            object[] expectedResult2 = { 2, 3, 4 };
            thisTest.Verify("y", expectedResult2);
            thisTest.Verify("x", expectedResult1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T75_Function_With_Replication_In_Two_Args()
        {
            string code = @"
y;
[Associative]
{
    def foo : int ( a : int, b : int )
	{
		return  = a + b;
	}
	
	x1 = { 1, 2, 3 };
	x2 = { 1, 2, 3 };
	
	y = foo ( x1, x2 );
	
}
	 
	 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedResult = { 2, 4, 6 };
            thisTest.Verify("y", expectedResult);
        }

        [Test]
        [Category("SmokeTest")]
        public void T76_Function_With_Replication_In_One_Arg()
        {
            string code = @"
y;
[Associative]
{
    def foo : int ( a : int, b : int )
	{
		return  = a + b;
	}
	
	x1 = { 1, 2, 3 };
	x2 = 1;
	
	y = foo ( x1, x2 );
	
}
	 
	 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedResult = { 2, 3, 4 };
            thisTest.Verify("y", expectedResult);
        }

        [Test]
        [Category("Replication")]
        public void T77_Function_With_Simple_Replication_Guide()
        {
            //Assert.Fail("1463477 - Sprint 20 : rev 2112 : replication guides throwing MethodResolutionException ");

            string code = @"
a1;a2;a3;a4;
[Associative]
{
    def foo : int ( a : int, b : int )
	{
		return  = a + b;
	}
	
	x1 = { 1, 2 };
	x2 = { 1, 2 };
	y = foo( x1<1> , x2<2> );
	a1 = y[0][0];
	a2 = y[0][1];
	a3 = y[1][0];
	a4 = y[1][1];
	
}
	 
	 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("a1", 2, 0);
            thisTest.Verify("a2", 3, 0);
            thisTest.Verify("a3", 3, 0);
            thisTest.Verify("a4", 4, 0);

        }

        [Test]
        [Category("Update")]
        public void T78_Function_call_By_Reference()
        {
            string code = @"
c;d;
[Associative]
{
    def foo : int ( a : int, b : int )
	{
		a = a + b;
		b = 2;
		return  = a + b;
	}
	
	a = 1;
	b = 2;
	c = foo (a, b );
	d = a + b;
	
}
	 
	 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1463498 - Sprint 20 : rev 8112 : Updating variables inside function call is returning the wrong value");
            thisTest.Verify("c", 5);
            thisTest.Verify("d", 3);

        }

        [Test]
        [Category("SmokeTest")]
        public void T79_Function_call_By_Reference()
        {
            string code = @"
c;d;
[Imperative]
{
    def foo : int ( a : int, b : int )
	{
		a = a + b;
		b = 2 * a;
		return  = a + b;
	}
	
	a = 1;
	b = 2;
	c = foo (a, b );
	d = a + b;
	
}
	 
	 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("c", 9, 0);
            thisTest.Verify("d", 3, 0);

        }

        [Test]
        [Category("SmokeTest")]
        public void T80_Function_call_By_Reference()
        {
            string code = @"
[Associative]
{
    def foo : int ( a : int, b : int )
	{
		c = [Imperative]
		{
		    d = 0;
			if( a > b )
				d = 1;
			return = d;	
		}
		a = a + c;
		b = b + c;
		return  = a + b;
	}
	
	a = 2;
	b = 1;
	c = foo (a, b );
	d = a + b;
	
}
	 
	 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kInvalidStaticCyclicDependency);
        }

        [Test]
        [Category("SmokeTest")]
        public void T81_Function_Calling_Imp_From_Assoc()
        {
            string code = @"
b;
[Associative]
{
    def foo : int ( a : int )
	{
		c = [Imperative]
		{
		    d = 0;
			if( a > 1 )
			{
				d = 1;
			}
			return = d;	
		}
		return  = a + c;
	}
	
	a = 2;
	b = foo (a );	
}
	 
	 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", 3, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T82_Function_Calling_Assoc_From_Imp()
        {
            string code = @"
b;
[Imperative]
{
    def foo : int ( a : int )
	{
		d = 0;
		if( a > 1 )
	    {
		     d = [Associative]
			 {
			     return = a + 1;
			 }
		}
		else
		{
		    d = a + 2;
		}
		return  = a + d;
	}
	
	a = 2;
	b = foo (a );	
}
	 
	 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", 5, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T83_Function_With_Null_Arg()
        {
            string code = @"
b;
def foo:int ( a : int )
{	
	return = a;
}
[Associative]
{
	b = foo( null );
}
	";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue(mirror.GetValue("b").DsasmValue.IsNull);
        }

        [Test]
        [Category("SmokeTest")]
        public void T84_Function_With_User_Defined_Class()
        {
            string code = @"
class A
{
    i : int;
	constructor A ( x : int )
	{
	    i = x;
	}
}
def foo : A ( a : A )
{
    a1 = a.i;
	b1 = A.A(a1+1);
	return = b1;
}
x = A.A(1);
y = foo (x);
z = y.i;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("z", 2, 0);
        }

        [Test]
        [Category("Replication")]
        public void T85_Function_With_No_Type()
        {
            // Assert.Fail("1467080 - sprint23 : rev 2681 : method overload issue over functions with same name and signature in multiple language blocks");

            string code = @"
a11;a21;a31;
a12;a22;a32;
[Imperative]
{
	def foo( a )
	{
		a = a + 1;
		return = a;
	}
	c = { 1,2,3 };
	d = foo ( c ) ;
		
	a11 = d[0];
	a21 = d[1];
	a31 = d[2];
}
[Associative]
{
	def foo( a )
	{
		a = a + 1;
		return = a;
	}
	c = { 1,2,3 };
	d = foo ( c ) ;
		
	a12 = d[0];
	a22 = d[1];
	a32 = d[2];
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("a11", 2);
            thisTest.Verify("a21", 3);
            thisTest.Verify("a31", 4);
            thisTest.Verify("a12", 2);
            thisTest.Verify("a22", 3);
            thisTest.Verify("a32", 4);
        }

        [Test]
        [Category("SmokeTest")]
        public void T86_Function_With_For_Loop()
        {
            string code = @"
a1;a2;a3;
[Imperative]
{	
	def foo: int( a : int )
	{
		for( i in a )		
		{					
		}		
		return = a;	
	}	
	d = { 1,2,3 };	
	//c = foo( d );	
	j = 0;	
	for( i in d )	
	{		
		d[j] = i + 1;		
		j = j + 1;	
	}	
	a1 = d[0];	
	a2 = d[1];	
	a3 = d[2];
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a1", 2, 0);
            thisTest.Verify("a2", 3, 0);
            thisTest.Verify("a3", 4, 0);
        }

        [Test]
        public void T87_Function_Returning_From_Imp_Block_Inside_Assoc()
        {
            string code = @"
def foo : int( a : int)
{
  temp = [Imperative]
  {
      if ( a == 0 )
      {
          return = 0; 
      }
    return = a ;
  } 
  return = temp;
}
x = foo( 0 );
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 0, 0);

        }


        [Test]
        [Category("SmokeTest")]
        public void T88_Function_With_Collection_Argument()
        {
            string code = @"
def foo : double (arr : double[])
{
    return = 0;
}
arr = {1,2,3,4};
sum = foo(arr);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("sum", 0.0, 0);

        }

        [Test]
        [Category("Replication")]
        public void T89_Function_With_Replication()
        {
            //Assert.Fail("1456115 - Sprint16: Rev 889 : Replication over a collection is not working as expected");
            //Assert.Fail("1467075 - Sprint23 : rev 2660 : replication with nested array is not working as expected");
            string error = "1467355 Sprint 27 Rev 4014 , it replicates with maximum combination where as its expected to zipped ";
            string code = @"def foo : double (arr1 : double[], arr2 : double[] )
{
    return  = arr1[0] + arr2[0];
}
arr = {  {2.5,3}, {1.5,2} };
two = foo (arr, arr);
t1 = two[0];
t2 = two[1];
//arr1 = {2.5,3};
//arr2 = {1.5,2};
//two = foo(arr1, arr2);
";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("t1", 5.0, 0);
            thisTest.Verify("t2", 3.0, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T90_Function_PassingNullToUserDefinedType()
        {
            string code = @"
class Point
{
	length : int;
	constructor Create(l:int)
	{
		length = l;
	}
	def GetLength : int()
	{
		return = length;
	}
}
def GetPointLength : int(p:Point)
{
	return = p.GetLength();
}
p = Point.Create(2);
testP = GetPointLength(p);
p2 = null;
testNull = GetPointLength(p2);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification
            object testP = 2;
            object testNull = null;
            thisTest.Verify("testP", testP, 0);
            thisTest.Verify("testNull", testNull, 0);
        }

        [Test]
        public void T91_Function_With_Default_Arg()
        {
            string code = @"
def foo:double(x:int = 2, y:double = 5.0)
{
	return = x + y;
}
a = foo();
b = foo(1, 3.0);
c = foo();
d;e;f;
[Imperative]
{
	d = foo();
	e = foo(1, 3.0);
	f = foo();
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification
            object a = 7.0;
            object b = 4.0;
            object c = 7.0;
            object d = 7.0;
            object e = 4.0;
            object f = 7.0;
            thisTest.Verify("a", a);
            thisTest.Verify("b", b);
            thisTest.Verify("c", c);
            thisTest.Verify("d", d);
            thisTest.Verify("e", e);
            thisTest.Verify("f", f);
        }

        [Test]
        [Category("Type System")]
        public void T92_Function_With_Default_Arg_Overload()
        {
            //Assert.Fail("DNL-1467202 - Argument type casting is not happening in some cases with function calls");
            string code = @"
def foo:double()
{
	return = 0.0;
}
def foo:double(x : int = 1, y : double = 2.0)
{
	return = x + y;
}
a = foo();
b = foo(3);
c = foo(3.4); // not found, null
d = foo(3, 4.0);
e = foo(1, 2.0, 3); // not found, null
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification
            object a = 0.0;
            object b = 5.0;
            object c = 5.0;
            object d = 7.0;
            object e = null;
            thisTest.Verify("a", a);
            thisTest.Verify("b", b);
            thisTest.Verify("c", c);
            thisTest.Verify("d", d);
            thisTest.Verify("e", e);
        }

        [Test]
        public void T93_Function_With_Default_Arg_In_Class()
        {
            string str = "";
            string code = @"class Test
{
	a : int = 1;
	b : int = 2;
	c : int = 3;
	constructor Test(k:int, m:int = 4)
	{
		a = k;
		b = m;
	}
	def foo:double(x:int = 2, y:double = 5.0)
	{
		return = x + y + a + b + c;
	}
}
i;j;k;a;b;c;
[Associative]
{
	t = Test.Test(2); // a = 2, b = 4, c = 3
	i = t.foo(); // i = 16.0
	j = t.foo(1, 3.0); // j = 13.0
	k = t.foo(); // k = 16.0
}
[Imperative]
{
	t = Test.Test(2); // a = 2, b = 4, c = 3
	a = t.foo(); // a = 16.0
	b = t.foo(1, 3.0); // b = 13.0
	c = t.foo(); // c = 16.0
}
";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, str);
            //Verification
            object i = 16.0;
            object j = 13.0;
            object k = 16.0;
            object a = 16.0;
            object b = 13.0;
            object c = 16.0;
            thisTest.Verify("i", i);
            thisTest.Verify("j", j);
            thisTest.Verify("k", k);
            thisTest.Verify("a", a);
            thisTest.Verify("b", b);
            thisTest.Verify("c", c);
        }


        [Test]
        [Category("SmokeTest")]
        public void TV00_Function_With_If_Statements()
        {
            string src = @"a;
[Imperative]
{
	def foo:int( a:int, b:int, c:int )
	{
		if( a < b )
		{
			if( a < c ) 
				return = a;
		}
	
		else if ( b < c ) 
			return = b;
		
		else if (a == b && b == c ) 
			return  = 1;
			
		return = c;
	}
	
	a = foo( -9,3,-7 );
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == -9);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV01_Function_With_While_Statements()
        {
            string src = @"d;
[Imperative]
{
	def foo:int( a:int, b:int)
	{	
		c = 1;
		
		while( b > 0 )
		{
			c = c * a;
			b = b - 1;
		}
	
		return = c;
	}
	
	d = foo( 2,3 );
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 8);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV02_Function_With_For_Statements()
        {
            string src = @"d;
[Imperative]
{
	def foo:int( a:int )
	{	
		c = { 1,2,3,4,5 };
		temp = 0;
		
		for( i in c )
		{
			if( i == a )
			temp = 1;
			
		}
	
		if(temp) 
			return = a;
		
		return = 0;
	
	}
	
	d = foo( 6 );
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV03_Function_With_Recursion()
        {
            string src = @"x;
[Imperative]
{
	def factorial: int( a:int )
	{	
		if(!a)
			return = 1;
		
		if( a < 0 )
			return = 0;
			
		else 
			return = a * factorial( a - 1 );
	}
		x = factorial(5);
}
		";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 120);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV04_Function_With_RangeExpression()
        {
            string src = @"d;
[Imperative]
{
	def foo:double( a:int )
	{
		b = 2..4..#3;
		
		sum = b[0] + b[1] + b[2];
		
		return = sum + 0.5;
	}
	d = foo( 1 );
}
		";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((double)mirror.GetValue("d").Payload == 9.5);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV05_Function_With_RangeExpression_2()
        {
            string src = @"a;d;
[Imperative]
{
	def foo: int( a:int[] )
	{
		sum = 0;
		
		for( i in a )
		{
			sum = sum + i ;
		}
		return = sum;
	}
	a = 2..4..#3;
	
	d = foo( a );
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 9);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV06_Function_With_Logical_Operators()
        {
            string src = @"a;
[Imperative]
{
	def foo:int( a:int , b:int, c:int )
	{
		if(a > 0 || b > 0 || c > 0)
		{
			if((a > 0 && b < 0)||(a > 0 && c < 0))
				return = a;
			else
				return = 0;
		}
		
		return = 1;
	}
	
	a = foo( 2,3,4 );
}
			
			";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV07_Function_With_Math_Operators()
        {
            string code = @"a;
[Imperative]
{
	def math: double(a:double, b:double, c:int)
	{
		if( c == 1 )
		{
			return = a + b;
		}
			if( c == 2 )
		{
			return = a - b;
		}
		
		if( c == 3 )
		{
			return = a * b;
		}
		
		else
			return = a / b;
		
			
	}
	a = 18;
	b = 2;
	c = 1;
	
	a = math(a,b,2+1);
}";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 36.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV08_Function_With_Outer_Function_Calls()
        {
            string src = @"x;y;
[Imperative]
{
	def is_negative:int(a :int)
	{
		if( a < 0 )
			return = 1;
			
			return = 0;
	}
	
	def make_negative:int(a :int)
	{
		return = a * -1;
	}
	
	def absolute:int(a :int)
	{
		if(is_negative(a))
			a = make_negative(a);
		
		return = a;
	}
	x = -7;
	x = absolute(x);
	y = absolute(11);
}	
		";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 7);
            Assert.IsTrue((Int64)mirror.GetValue("y").Payload == 11);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV09_Function_With_Argument_Update_Imperative()
        {
            string src = @"e;f;
[Imperative]
{
	def update:int( a:int, b:int )
	{
		a = a + 1;
		b = b + 1;
		return = a + b;
	}
	
	c = 5;
	d = 5;
	e = update(c,d);
	e = c;
	f = d;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("e").Payload == 5);
            Assert.IsTrue((Int64)mirror.GetValue("f").Payload == 5);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV10_Function_With_Class_Instances()
        {
            string src = @"b;
class A
{
	a : var;
	constructor CreateA ( a1 : int )
	{
		a = a1;
	}
	def add1 : int(  )
    {
	    return = a + 1;
    }
}
	def foo:int( a : int )
	{
		A1 = A.CreateA( a );
		return = A1.add1();
	}
[Imperative]
{
	b = foo(1);
}
	
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV11_Function_Update_Local_Variables()
        {
            string src = @"r;
[Imperative]
{
	def foo:int ( a : int )
	{
		b = a + 2.5;
		c = true;
		c = a;
		d = c + b;
		return = d;
	}
	
	r = foo(1);
	r = b;
}
	";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue(mirror.GetValue("r").DsasmValue.IsNull);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV12_Function_With_Argument_Update_Associative()
        {
            string src = @"e;f;
[Associative]
{
	def update:int( a:int, b:int )
	{
		a = a + 1;
		b = b + 1;
		return = a + b;
	}
	
	c = 5;
	d = 5;
	e = update(c,d);
	e = c;
	f = d;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("e").Payload == 5);
            Assert.IsTrue((Int64)mirror.GetValue("f").Payload == 5);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV13_Empty_Functions_Imperative()
        {
            string src = @"b;
[Imperative]
{
	def foo:int ( a : int )
	{
	}
	
	b = foo( 1 );
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue(mirror.GetValue("b").DsasmValue.IsNull);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV14_Empty_Functions_Associative()
        {
            string src = @"b;
[Associative]
{
	def foo:int ( a : int )
	{
	}
	
	b = foo( 1 );
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue(mirror.GetValue("b").DsasmValue.IsNull);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV15_Function_No_Brackets()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string src = @"b;
[Imperative]
{
	def foo:int ( a : int ) 
	return = a + 1;
	
	b = foo(1);
}";
                ExecutionMirror mirror = thisTest.RunScriptSource(src);
                Assert.IsTrue(mirror.GetValue("b").DsasmValue.IsNull);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void TV16_Function_With_No_Return_Statement()
        {
            string src = @"b;
	def foo : int( a : int )
	{
		a = a + 1;
	}
	[Imperative]
	{
		b = foo( 1 );
	}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue(mirror.GetValue("b").DsasmValue.IsNull);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV17_Function_Access_Local_Variables_Outside()
        {
            string src = @"
	def foo: int ( a : int )
	{
		b = a + 1;
		c = a * 2;
		return = a;
	}
e;f;
[Imperative]
{	
	d = foo( 1 );
	e = b;
	f = c;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue(mirror.GetValue("e").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("f").DsasmValue.IsNull);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV18_Function_Access_Global_Variables_Inside()
        {
            //Assert.Fail("1461363 - Sprint 19 : rev 1800 : Global variable support is not there in function scope ");
            string code = @"
	global = 5;
	global2 = 6;
	d;
	
	def foo: int ( a : int )
	{
		b = a + global;
		c = a * 2 * global2;
		return = b + c;
	}
[Imperative]
{	
	d = foo( 1 );
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("d", 18);


        }

        [Test]
        [Category("SmokeTest")]
        public void TV19_Function_Modify_Global_Variables_Inside()
        {
            //Assert.Fail("1461363 - Sprint 19 : rev 1800 : Global variable support is not there in function scope "); 
            string code = @"
	global = 5;
	e;
	
	def foo: int ( a : int )
	{
		global = a + global;
		
		return = a;
	}
[Imperative]
{	
	d = foo( 1 );
	e = global;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("e", 6);

        }

        [Test]
        [Category("SmokeTest")]
        public void TV20_Function_With_Illegal_Syntax_1()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string src = @"b;
[Imperative]
{
	def foo:int(a:int)
	{
		return = a + 1:
	}
	
	b = foo( 9 );
}";
                ExecutionMirror mirror = thisTest.RunScriptSource(src);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void TV21_Function_With_Illegal_Syntax_2()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string src = @"b;
[Imperative]
{
	def foo : int(a ; int)
	{
		return = a + 1;
	}
	
	b = foo( 9 );
}";
                ExecutionMirror mirror = thisTest.RunScriptSource(src);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void TV22_Function_With_Class_Object_As_Argument()
        {
            string src = @"b;
class A
{
	a : var;
	constructor CreateA ( a1 : int )
	{
		a = a1;
	}
	def add1 : int(  )
    {
	    return = a + 1;
    }
}
def foo:int ( A_Inst : A )
{	
	return = A_Inst.add1();
}
[Associative]
{
	b = foo( A.CreateA( 1 ) );
}
	";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV23_Defect_1455152()
        {
            string code = @"
c;
d;
def foo : int ( a : int )
{
    b = a + 1;
}	 
[Associative]
{
     c = foo(1);
}
[Imperative]
{
     d = foo(1);
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue(mirror.GetValue("c").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("d").DsasmValue.IsNull);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV24_Defect_1454958()
        {
            //Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            //{
            string code = @"
def foo: int( a : int , b : int)
{
	return = a + b;
}
b;c;
[Associative]
{
	b = Foo( 1,2 );
}
[Imperative]
{
	c = foo( 1 );
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue(mirror.GetValue("b").DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("c").DsasmValue.IsNull);
            //});
        }

        [Test]
        [Category("SmokeTest")]
        public void TV25_Defect_1454923()
        {
            string code = @"
class Temp
{
	a : var;
	
	constructor CreateTemp ( a1 : int )
	{
		a = a1;
	}
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV26_Defect_1454923_2()
        {
            string code = @"
def function1: int ( a : int, b : int )
{
	return = -1 * (a * b );
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV27_Defect_1454688()
        {
            //Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            //{
            string code = @"
a;
[Associative]
{
	a = function1(1,2,3);
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue(mirror.GetFirstValue("a").DsasmValue.IsNull);
            //});
        }

        [Test]
        [Category("SmokeTest")]
        public void TV28_Defect_1454688_2()
        {
            //Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            //{
            string code = @"
a;
[Imperative]
{
	a = function(1,2,3);
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue(mirror.GetFirstValue("a").DsasmValue.IsNull);
            //});
        }

        [Test]
        [Category("SmokeTest")]
        public void TV29_Overloading_Different_Number_Of_Parameters()
        {
            string code = @"
def foo:int ( a : int )
{
	return = a + 1;
}
def foo:int ( a : int, b : int, c: int )
{
	return = a + b + c ;
}
c = foo( 1 );
d = foo( 3, 2, 0 );
a;
[Imperative]
{
	a = foo( 1, 2, 3 );
}	
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", 2);
            thisTest.Verify("d", 5);
            thisTest.Verify("a", 6);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV30_Overloading_Different_Parameter_Types()
        {
            string code = @"
def foo:int ( a : int )
{
	return = 2 * a;
}
def foo:int ( a : double )
{
	return = 2;
}
	b = foo( 2 );
	c = foo(3.4);
d;
[Imperative]
{
	d = foo(-2.4);
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", 4);
            thisTest.Verify("c", 2);
            thisTest.Verify("d", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV31_Overloading_Different_Return_Types()
        {
            string code = @"
def foo: int( a: int )
{
	return = 1;
}
// This is the same definition regardless of return type
def foo: double( a : int )
{
	return = 2.3;
}
b = foo ( 1 );";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
        }

        [Test]
        public void TV32_Function_With_Default_Argument_Value()
        {
            //Assert.Fail("1455742 - Sprint15 : Rev 836 : Function with default values not allowed"); 
            string code = @"
def foo : int ( a = 5, b = 5 )
{
	return =  a +  b;
}
c1;c2;c3;c4;
[Imperative]
{
	c1 = foo (  );
	c2 = foo ( 1 );
	c3 = foo ( 1, 2 );
	c4 = foo ( 1, 2, 3 );
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object v1 = null;
            thisTest.Verify("c1", 10);
            thisTest.Verify("c2", 6);
            thisTest.Verify("c3", 3);
            thisTest.Verify("c4", v1);

        }

        [Test]
        public void TV32_Function_With_Default_Argument_Value_2()
        {
            //Assert.Fail("1455742 - Sprint15 : Rev 836 : Function with default values not allowed"); 
            string code = @"
def foo  ( a : int = 5, b : double = 5.5, c : bool = true )
{
	return = x = c == true ? a  : b;
}
def foo  ( a : double = 5, b : double = 5.5, c : bool = true )
{
	return = x = c == true ? a  : b;
}
c1;c2;c3;c4;
[Imperative]
{
	c1 = foo (  );
	c2 = foo ( 1 );
	c3 = foo ( 1.5, 2 );
	c4 = foo ( 1, 2, 0 );
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c1", 5);
            thisTest.Verify("c2", 1);
            thisTest.Verify("c3", 1.5);
            thisTest.Verify("c4", 2.0);

        }

        [Test]
        public void TV32_Function_With_Default_Argument_Value_3()
        {
            //Assert.Fail("1455742 - Sprint15 : Rev 836 : Function with default values not allowed"); 
            string code = @"
def foo  ( a : int, b : double = 5, c : bool = true)
{
	return = x = c == true ? a  : b;
}
def foo2  ( a , b = 5, c = true)
{
	return = x = c == true ? a  : b;
}
c1;c3;c3;c4;
d1 = foo2 (  );
d2 = foo2 ( 1 );
d3 = foo2 ( 2, 3 );
d4 = foo2 ( 4, 5, false );
d5 = 
[Imperative]
{
	c1 = foo (  );
	c2 = foo ( 1 );
	c3 = foo ( 2, 3 );
	c4 = foo ( 4, 5, false );
	return = { c1, c2, c3, c4 };
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { null, 1, 2, 5.0 };
            thisTest.Verify("d5", v1);
            thisTest.Verify("d2", 1);
            thisTest.Verify("d3", 2);
            thisTest.Verify("d4", 5);
        }

        [Test]
        [Category("Method Resolution")]
        public void TV33_Function_Overloading()
        {
            string code = @"
def foo  ( a : int = 5, b : double = 5.5, c : bool = true )
{
	return = x = c == true ? a  : b;
}
def foo  ( a : double = 6, b : double = 5.5, c : bool = true )
{
	return = x = c == true ? a  : b;
}
c1;c2;c3;c4;
[Imperative]
{
	c1 = foo (  );
	c2 = foo ( 1 );
	c3 = foo ( 1.5, 2 );
	c4 = foo ( 1, 2, 0 );
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            // Assert.Fail("1467176 - sprint24: Regression : rev 3152 : inline condition causing wrong output in function with default arguments");
            thisTest.Verify("c1", 5);
            thisTest.Verify("c2", 1);
            thisTest.Verify("c3", 1.5);
            thisTest.Verify("c4", 2.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV33_Function_Overloading_2()
        {
            string code = @"
def foo  ( a : int , b : double , c : bool  )
{
	return = a;
}
def foo  ( a : double, b : double , c : int  )
{
	return = b;
}
c4;c5;c6;
[Imperative]
{
	c4 = foo ( 1, 2, 0 );
	c5 = foo ( 1.5, 2.5, 0 );
	c6 = foo ( 1.5, 2.5, true );
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c4", 2.0);
            thisTest.Verify("c5", 2.5);
            thisTest.Verify("c6", 2);
        }


        [Test]
        [Category("SmokeTest")]
        public void TV33_Overloading_Different_Order_Of_Parameters()
        {
            string code = @"
def foo : int ( a :int, b : double )
{
	return = 2;
}
def foo : int( c : double, d : int )
{
	return = 3;
}
c = foo( 1,2.5 );
d = foo ( 2.5, 1 );
//e = foo ( 2.5, 2.5 );
f = foo ( 1, 2 );
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("f", 2, 0);
            thisTest.Verify("c", 2, 0);
            thisTest.Verify("d", 3, 0);
        }


        [Test]
        [Category("SmokeTest")]
        public void TV34_Implicit_Conversion_Int_To_Bool()
        {
            string code = @"
b;c;d;
[Imperative]
{
	def foo:int ( a : bool )
	{
		if(a)
			return = 1;
		else
			return = 0;
	}
	
	b = foo( 1 );
	c = foo( 1.5 );
	d = 0;
	if(1.5 == true ) 
	{
	    d = 3;
	}
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", 1);
            thisTest.Verify("c", 1);
            thisTest.Verify("d", 3);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV35_Implicit_Conversion_Int_To_Double()
        {
            string code = @"
def foo : double ( a: double )
{
	return = a + 2.5;
}
	b = foo( 2 );
	c;
[Imperative]
{
	c = foo( 3 );
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("b", 4.5);
            thisTest.Verify("c", 5.5);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV36_Implicit_Conversion_Return_Type()
        {
            string code = @"
def foo: bool ( a : double, b : double )
{
	return = 0.5;
}
c = foo ( 2.3 , 3 );";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", true);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV37_Overloading_With_Type_Conversion()
        {
            string code = @"
def foo : int ( a : double, b : double )
{
	return = 1;
}
def foo : int ( a : int, b : int )
{
	return = 2;
}
def foo : int ( a : int, b : double )
{
	return = 3;
}
a = foo ( 1,2 );
b = foo ( 2,2 );
c = foo ( 1, 2.3 );
d = foo ( 2.3, 2 );
 ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 2, 0);
            thisTest.Verify("b", 2, 0);
            thisTest.Verify("c", 3, 0);
            thisTest.Verify("d", 1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV38_Defect_1449956()
        {
            string code = @"
x;
y;
[Imperative]
{
	def factorial: int( a:int )
	{	
		if(!a)
			return = 1;
		
		if( a < 0 )
			return = 0;
			
		else 
			return = a * factorial( a - 1 );
	}
		x = factorial(5);
		y = factorial(7);
		}
	";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 120, 0);
            thisTest.Verify("y", 5040, 0);

        }

        [Test]
        [Category("SmokeTest")]
        [Category("Failure")]
        public void TV39_Defect_1449956_2()
        {
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4012
            string code = @"
[Imperative]
{
def recursion: int ( a : int )
{

	if ( a ==0 || a < 0)
		return = 0;
	
	 
		return = a + recursion(a - 1);

}
	x = recursion( 10 );
	y = recursion( -1 );
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 55, 0);
            thisTest.Verify("y", 0, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV40_Defect_1449956_3()
        {
            string code = @"
x;y;
[Associative]
{
	def recursion  : int( a : int)
	{
		temp = [Imperative]
		{
			if ( a ==0 || a < 0)
			{
				return = 0;	
			}
			return = a + recursion( a - 1 );
		}		 
		return = temp;
	}
	x = recursion( 4 );
	y = recursion( -1 );
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 10, 0);
            thisTest.Verify("y", 0, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV41_Defect_1454959()
        {
            string code = @"
result;
a;
[Associative]
{
 def Level1 : int (a : int)
 {
  return = Level2(a+1);
 }
 
 def Level2 : int (a : int)
 {
  return = a + 1;
 }
 input = 3;
 result = Level1(input); 
}
[Imperative]
{
 
 b = 1;
 a = foo(b);
 def foo : int (a : int)
 {
     return = a + 1;
 }
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("result", 5);
            thisTest.Verify("a", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV42_Defect_1454959()
        {
            string code = @"
a;b;c;
def Level1 : int (a : int)
{
    return = Level2(a+1);
}
 
def Level2 : int (a : int)
{
    return = a + 1;
}
input = 3;
result = Level1(input); 
[Associative]
{
    a = Level1(4);
	b = foo (a);
	c = [Imperative]
	{
	    return = foo2( foo (a ) );
	}
}
def foo ( a )
{
    return = a + foo2(a);
}
def foo2 ( a ) 
{
    return = a;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 6);
            thisTest.Verify("b", 12);
            thisTest.Verify("c", 12);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV43_Defect_1455143()
        {
            string code = @"
a1;b1;c1;
a2;b2;c2;
[Associative]
{ 
	 def foo1 : int[] ( a : int[] )
	 {
	    a[0] = 0;
            return = a;
         }
	 aa = { 1, 2 };
	 bb = foo1 ( aa );	
	 a1 = aa[0];
	 b1 = bb[0];
	 cc = [Imperative]
	 {
	     return = foo1(aa);
	 };	
	 c1 = cc[0];
}
[Imperative]
{ 
	 def foo  ( a : int[] )
	 {
	    a[0] = 0;
            return = a;
         }
	 aa = { 1, 2 };
	 bb = foo ( aa );	
	 a2 = aa[0];
	 b2 = bb[0];
	 c2 = 3;	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a1", 1);
            thisTest.Verify("b1", 0);
            thisTest.Verify("c1", 0);
            thisTest.Verify("a2", 1);
            thisTest.Verify("b2", 0);
            thisTest.Verify("c2", 3);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV44_Defect_1455245()
        {
            string code = @"
a;b;
[Associative]
{
	a = 1;
	[Imperative]
	{
		def foo : int ( x : int )
	    {
	        return = a + x;
        }
	 
	    b = foo(1) ;	
	}
	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1, 0);
            thisTest.Verify("b", 2, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV45_Defect_1455278()
        {
            string code = @"
b1;b2;
[Associative]
{
    def foo : int ( a : int )
	{
		c = [Imperative]
		{
		    d = 0;
			if( a > 1 )
			{
				d = 1;
			}
			return = d;	
		}
		return  = a + c;
	}
	
	a = 2;
	b1 = foo (a );	
}
[Imperative]
{
    def foo2 : int ( a : int )
	{
		
		c = [Associative]
		{
            return = [Imperative]
            { 
                d = 0;
                if( a > 1 )
                {
                    d = 1;
                }
                return = d;	
            }
        }
		return  = a + c;
		
	}
	
	a = 2;
	b2 = foo2 (a );	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b1", 3);
            thisTest.Verify("b2", 3);
        }

        [Test]
        public void TV46_Defect_1455278()
        {
            string code = @"
class A
{
    a : var;
    constructor A ( i )
	{
	    a = i;
	}
	def foo : int ( a1 : int )
	{
		c = [Imperative]
		{
		    d = 0;
			if( a1 > 1 )
			{
				d = 1;
			}
			return = d;	
		}
		return  = a + c;
	}
}
	
a1 = A.A(1);
b1 = a1.foo(2);
b2 = a1.foo(0);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b1", 2, 0);
            thisTest.Verify("b2", 1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV47_Defect_1456087()
        {
            string code = @"
def foo : int ( a : double, b : double )
{
	return = 2;
}
def foo : int ( a : int, b : int )
{
	return = 1;
}
def foo : int ( a : int, b : double )
{
	return = 3;
}
a;b;c;d;
[Imperative]
{
	a = foo ( 1, 2 );
	b = foo ( 2.5, 2.5 );
	c = foo ( 1, 2.3 );
	d = foo ( 2.3, 2 );
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1);
            thisTest.Verify("b", 2);
            thisTest.Verify("c", 3);
            thisTest.Verify("d", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV48_Defect_1456110()
        {
            string code = @"
def foo : int( a : int)
{
  temp = [Imperative]
  {
      if ( a == 0 )
      {
          return = 0; 
      }
      return = a ;
  } 
  return = temp;
}
x = foo( 0 );
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 0, 0);

        }
        [Test]
        [Category("SmokeTest")]
        public void TV49_Defect_1456110()
        {
            string code = @"
def recursion : int(a : int)
{
    loc = [Imperative]
    {
        if (a <= 0)
        {
            return = 0; 
        }
        return = a + recursion(a - 1);
    }
    return = loc;
}
a = 10;
x;
[Imperative]
{
	x = recursion(a); 
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 55);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV50_Defect_1456108()
        {
            string code = @"
c;
[Imperative]
{
	def collection:int[](a : int[] )
	{
		j = 0;
		for ( i in a )
		{
			a[j] = a[j] + 1;
			j = j + 1;
		}
		
		return = a;
	}
	
	[Associative]
	{
		c = { 1,2,3 };
		c = collection( c );
	}
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedResult = { 2, 3, 4 };
            thisTest.Verify("c", expectedResult);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV51_Defect_1456108_2()
        {
            string code = @"
c;
[Imperative]
{
	def collection:double[](a : double[] )
	{
		j = 0;
		for ( i in a )
		{
			a[j] = a[j] + 1;
			j = j + 1;
		}
		
		return = a;
	}
	
	c = { 1.0,2.0,3.0 };
	c = collection( c );
	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedResult = { 2.0, 3.0, 4.0 };
            thisTest.Verify("c", expectedResult, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV51_Defect_1456108_3()
        {
            // object[] expectedResult1 = { 2.0, 3, 4 };
            object expectedResult2 = null;

            string code = @"
class A
{
    b : double[];
	
	constructor A (x : double[])
	{
		b = x;
	}
	def add_1:double[](a: double[] )
	{
		j = 0;
		a = [Imperative]
		{
			for ( i in a )
			{
				a[j] = a[j] + 1;
				j = j + 1;
			}
			return = a;
		}
		
		return = a;
	}
	
	def add_2:double[]( )
	{
		j = 0;
		x = [Imperative]
		{
			for ( i in b )
			{
				b[j] = b[j] + 1;
				j = j + 1;
			}
			return = b;
		}
		
		return = x;
	}
}
c = { 1.0, 2.0, 3.0 };
a1 = A.A( c );
c = a1.add_1( c );
b2 = a1.add_2( );";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            // b1 is updated as well. 
            thisTest.Verify("a1", expectedResult2, 0);
            thisTest.Verify("b2", expectedResult2, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV51_Defect_1456108_4()
        {
            object[] expectedResult1 = { 1.0, 2.0, 3.0 };
            object[] expectedResult2 = { 2.0, 3.0, 4.0 };
            string code = @"
class A
{
    b : double[];
	
	constructor A (x : double[])
	{
		b = x;
	}
	def add_1:double[](a: double[] )
	{
		j = 0;
		a = [Imperative]
		{
			for ( i in a )
			{
				a[j] = a[j] + 1;
				j = j + 1;
			}
			return = a;
		}
		
		return = a;
	}
	
	def add_2:double[]( )
	{
		j = 0;
		x = [Imperative]
		{
			for ( i in b )
			{
				b[j] = b[j] + 1;
				j = j + 1;
			}
			return = b;
		}
		
		return = x;
	}
}
c = { 1.0, 2.0, 3.0 };
a1 = A.A( c );
t1 = a1.b;
t2 = a1.add_2();
t=c;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("t", expectedResult1, 0);
            thisTest.Verify("t2", expectedResult2, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV51_Defect_1456108_5()
        {
            object[] expectedResult1 = { 0.0, 0.0, 0.0 };
            string code = @"
class A
{
    b : double[];
	
	constructor A (x : double[])
	{
		b = x;
	}
	def add_1:double[](a: double[] )
	{
		j = 0;
		a = [Imperative]
		{
			for ( i in a )
			{
				a[j] = a[j] + 1;
				j = j + 1;
			}
			return = a;
		}
		
		return = a;
	}
	
	def add_2:double[]( )
	{
		j = 0;
		x = [Imperative]
		{
			for ( i in b )
			{
				b[j] = b[j] + 1;
				j = j + 1;
			}
			return = b;
		}
		
		return = x;
	}
}
c = { 1.0, 2.0, 3.0 };
a1 = A.A( c );
b1 = a1.add_1( c );
b2 = a1.add_2( );
t = a1.b;
c = { -1, -1, -1 };";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1467238 - Sprint25: rev 3420 : REGRESSION : Update issue when class collection property is updated");
            thisTest.Verify("t", expectedResult1, 0);
            thisTest.Verify("b2", expectedResult1, 0);
            thisTest.Verify("b1", expectedResult1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV52_Defect_1456397()
        {
            string code = @"
class A
{
	a : var;
	
	constructor CreateA ( a1 : int )
	{	
		a = a1 ;
	}		
	
	def CreateNewVal ( )
	{
		y = [Imperative]
		{
			if ( a  < 10 )			
			{
				return = a + 10;			
			}
			return = a;
 		}
		return = y + a;
	}
}
b1;
[Associative]
{
    a1 = A.CreateA(1);
	b1 = a1.CreateNewVal();
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b1", 12);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV53_Defect_1456397_2()
        {
            string code = @"
class A
{
	a : var;
	
	constructor CreateA ( a1 : int )
	{	
		a = a1 ;
	}		
	
	def CreateNewVal ( )
	{
		y = [Associative]
		{
			return = a;
 		}
		return = y + a;
	}
}
b1;
[Imperative]
{
    a1 = A.CreateA(1);
	b1 = a1.CreateNewVal();
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b1", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV54_Defect_1456397_3()
        {
            string code = @"
b1;
[Imperative]
{
	def CreateVal ( a )
	{
		x = 1;
		y = [Associative]
		{
			return = a;
		}
		return = x + y;
	}
	b1 = CreateVal( 1 );
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b1", 2, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV55_Defect_1456571()
        {
            string code = @"
def foo(arr)
{
    retArr = 3;	
	[Imperative]
    {
		retArr = 5;
	}
    return = retArr;
}
	x = 0.5;
	x = foo(x);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 5, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV56_Defect_1456571_2()
        {
            string code = @"
def foo(arr)
{
    retArr = 3;	
	[Associative]
    {
		retArr = 5;
	}
    return = retArr;
}
x;
	[Imperative]
	{
		x = 0.5;
		x = foo(x);
	}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 5);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV56_Defect_1456571_3()
        {
            string code = @"
class A
{
	a : var;
	
	constructor A(b)
	{
		a = b;
		[Imperative]
		{
			if(a > 3 ) 
			{
				a = 3 ;
			}
		}
	}
	def foo (b)
	{
		[Imperative]
		{
			if(a == 3 ) 
			{
				a = 3 + b;
			}		
		}
		return = a;
	}
}
f1;f2;f3;f4;
[Associative]
{
	a1 = A.A(5);
	a2 = A.A(1);
	f1 = a1.foo(2);
	f2 = a2.foo(2);
	
}
[Imperative]
{
	a1 = A.A(5);
	a2 = A.A(1);
	f3 = a1.foo(2);
	f4 = a2.foo(2);	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("f1", 5);
            thisTest.Verify("f2", 1);
            thisTest.Verify("f3", 5);
            thisTest.Verify("f4", 1);

        }

        [Test] //Fuqiang: this is changed due to the implementation of function pointer
        [Category("SmokeTest")]
        public void TV57_Defect_1454932()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
[Associative]
{
	def Foo : int ()
	{
		return = 4;
	}
	Foo = 5;
	b = Foo();
	c = Foo;
}";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
                //thisTest.Verify("b", 4, 0);
                //thisTest.Verify("c", 5, 0);
            });
        }

        [Test]
        [Category("SmokeTest")]
        public void TV58_Defect_1455278()
        {
            string code = @"
class A
{
    a : var;
    constructor A ( i )
	{
		a = i;
	}
	
	def foo : int ( a1 : int )
	{
		c = [Imperative]
		{
			d = 0;
			if( a1 > 1 )
		{
			d = 1;
		}
		return = d;	
		}
	return  = a + c;
	}
}
	a1 = A.A(1);
	b1 = a1.foo(2); 
	b2 = a1.foo(0); ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b1", 2, 0);
            thisTest.Verify("b2", 1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV59_Defect_1455278_2()
        {
            string code = @"
def multiply : double[] (a : double[])
{    
	temp = [Imperative]
    { 
		b = {0, 10};
		counter = 0; 
		
		for( y in a ) 
		{              
			b[counter] = y * y;   
			counter = counter + 1;           
		}                
        
		return = b;    
	}   
	return = temp;
}
	
	x = {2.5,10.0};
	x_squared = multiply( x );";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] ExpectedResult = { 6.25, 100.0 };
            thisTest.Verify("x_squared", ExpectedResult, 0);

        }

        [Test]
        [Category("SmokeTest")]
        public void TV60_Defect_1455278_3()
        {
            string code = @"
    def power(num : double, exponent : int)
    {
		temp = [Imperative]
        {
			result = 1.0;
            counter = 0;            
			while(counter < exponent )            
			{
				result = result * num;                
				counter =  counter + 1;            
			}            
			return = result;        
		}       
		return = temp;
	}    
	
	x = power(3.0, 2);
	
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 9.0, 0);

        }

        [Test]
        [Category("SmokeTest")]
        public void TV61_Defect_1456100()
        {
            string code = @"
d;
[Imperative]
{
	def foo: int( a : int )
	{
		for( i in a )
		{
		}
		return = a;
	}
	
	d = { 1,2,3 };
	j = 0;
	
	for( i in d )
	{
		d[j] = i + 1;
		j = j + 1;
	}
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] ExpectedResult = { 2, 3, 4 };
            thisTest.Verify("d", ExpectedResult, 0);

        }

        [Test]
        [Category("SmokeTest")]
        public void TV62_Defect_1455090()
        {
            string code = @"
	def foo:int[]..[] ( a:int[]..[] )
	{
		a[0][0] = 1;
		return = a;
	}
	b = { {0,2,3}, {4,5,6} };
	d = foo( b );
	c = d[0];";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedResult2 = { 1, 2, 3 };
            thisTest.Verify("c", expectedResult2, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV63_Defect_1455090_2()
        {
            string code = @"
	def foo ( a : double[]..[] )
	{
		a[0][0] = 2.5;
		return = a;
	}
	
	a = { {2.3,3.5},{4.5,5.5} };
	
	a = foo( a );
	c = a[0];
	d;
	[Imperative]
	{
		b = { {2.3}, {2.5} };
		b = foo( b );
		d = b[0];
	}
	
	
	";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedResult2 = { 2.5, 3.5 };
            object[] expectedResult = { 2.5 };
            thisTest.Verify("c", expectedResult2);
            thisTest.Verify("d", expectedResult);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV64_Defect_1455090_3()
        {
            string code = @"
c;d;
[Imperative]
{
	def foo ( a : double[]..[] )
	{
		a[0][0] = 2.5;
		return = a;
	}
	
	a = { {2.3,3.5},{4.5,5.5} };
	
	a = foo( a );
	c = a[0];
	
	[Associative]
	{
		b = { {2.3}, {2.5} };
		b = foo( b );
		d = b[0];
	}
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedResult2 = { 2.5, 3.5 };
            object[] expectedResult = { 2.5 };
            thisTest.Verify("c", expectedResult2);
            thisTest.Verify("d", expectedResult);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV65_Defect_1455090_4()
        {
            string code = @"
class A
{
	a : var[]..[];
	
	constructor Create( a1 : int[]..[])
	{
		a = a1;
	}
	
	def foo: int( )
	{
		return = a[0][0];
	}	
		
}
	
	b = {{1,2},{3,4}};
	A1 = A.Create( b );
	a = A1.foo();
	
		
	";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV66_Defect_1455090_5()
        {
            string code = @"
class A
{
	a : var;
	
	constructor Create( a1 :int)
	{
		a = a1;
	}
	
	def foo: int( )
	{
		return = a;
	}	
		
}
	def objarray:A ( arr : A[]..[] )
	{
		return = arr[1][0];
	}
	
	A1 = A.Create( 1 );
	A2 = A.Create( 3 );
	A3 = A.Create( 5 );
	A4 = A.Create( 7 );
	
	B = { { A1,A2 },{ A3,A4} };
	
	b = objarray( B );
	
	c = b.foo();
	
	
	
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", 5, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV67_Defect_1455090_6()
        {
            string code = @"
class A
{
	a : var;
	
	constructor Create( a1 :int)
	{
		a = a1;
	}
	
	def foo: int( )
	{
		return = a;
	}	
		
}
c;
	[Imperative]
	{
		def objarray:A ( arr : A[]..[] )
		{
			return = arr[1][0];
		}
	
		A1 = A.Create( 1 );
		A2 = A.Create( 3 );
		A3 = A.Create( 5 );
		A4 = A.Create( 7 );
	
		B = { { A1,A2 },{ A3,A4} };
		
		b = objarray( B );
	
		c = b.foo();
	}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", 5);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV68_Defect_1455090_7()
        {
            //Assert.Fail("1463372 - Sprint 20 : Rev 2088 : Imperative code is not allowed in class constructor ");

            string code = @"
class A
{
	a : var;
	
	constructor create(i:int)
	{
		[Imperative]
		{
			if( i == 1 )
			{
				a = { { 1,2,3 } , { 4,5,6 } };
			}
			else
			{
			    a = { { 1,2,3 } , { 1,2,3 } };
			}
		}
	
	}
	
	def compare:int ( b : int[]..[], i : int, j : int )
	{
		temp = [Imperative]
		{
		    if( b[i][j] == a[i][j] )
		        return = 1;
			return = 0;
		}
		return = temp ;
	}
}
b1 = { {1, 2, 3},{ 1, 2, 3} };
A1 = A.create(1);
a1 = A1.compare( b1, 0, 0 );
a2 = A1.compare( b1, 1, 1 );
	";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a1", 1);
            thisTest.Verify("a2", 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV69_Defect_1456799()
        {
            string code = @"
class Point
{
    X : var;
    
    constructor ByCoordinates(x : double)
    {
        X = x;
        
    }
}
class BSplineCurve
{
    P : var;
	
	constructor ByPoints(ptOnCurve : Point)
    {
	    P = ptOnCurve;
    }
}
pt1 = Point.ByCoordinates(0);
pt2 = Point.ByCoordinates(5);
pts = {pt1, pt2};
bcurve = BSplineCurve.ByPoints(pts[1]);
bcurvePt = bcurve.P;
bcurvePtX = bcurvePt.X;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("bcurvePtX", 5.0, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV69_Defect_1456799_2()
        {
            string code = @"
class Point
{
    X : var;
    
    constructor ByCoordinates(x : double)
    {
        X = x;
        
    }
}
class BSplineCurve
{
    P : var[];
	
	constructor ByPoints(ptOnCurve : Point[])
    {
	    P = ptOnCurve;
    }
}
pt1 = Point.ByCoordinates(0);
pt2 = Point.ByCoordinates(5);
pts = {pt1, pt2};
bcurve = BSplineCurve.ByPoints(pts);
bcurvePtX = bcurve.P[1].X;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("bcurvePtX", 5.0, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV69_Defect_1456799_3()
        {
            string code = @"
class Point
{
    X : var;
    
    constructor ByCoordinates(x : double)
    {
        X = x;
        
    }
}
class BSplineCurve
{
    P : var[];
	
	constructor ByPoints(ptOnCurve : Point[])
    {
	    P = ptOnCurve;
    }
}
bcurvePtX;
[Imperative]
{
	pt1 = Point.ByCoordinates(0);
	pt2 = Point.ByCoordinates(5);
	pts = {pt1, pt2};
	bcurve = BSplineCurve.ByPoints(pts);
	bcurvePtX = bcurve.P[1].X;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("bcurvePtX", 5.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV70_Defect_1456798()
        {
            string code = @"
class Point
{
    X : var;
    Y : var;
    Z : var;
    constructor ByCoordinates(x : double, y : double, z : double)
    {
        X = x;
        Y = y;
        Z = z;
    }
}
class BSplineCurve
{
    Pts : var[];
	
	constructor ByPoints(ptsOnCurve : Point[])
    {
	    Pts = ptsOnCurve;
    }
}
pt1 = Point.ByCoordinates(0,0,0);
pt2 = Point.ByCoordinates(5,0,0);
pt3 = Point.ByCoordinates(10,0,0);
pt4 = Point.ByCoordinates(15,0,0);
pt5 = Point.ByCoordinates(20,0,0);
pts = {pt1, pt2, pt3, pt4, pt5};
bcurve = BSplineCurve.ByPoints(pts);
p = bcurve.Pts[2];
X = p.X;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("X", 10.0, 0);

        }

        [Test]
        [Category("SmokeTest")]
        public void TV71_Defect_1456108()
        {
            string code = @"
c;
[Imperative]
{
	def collectioninc: int[]( a : int[] )
	{
		j = 0;
		for( i in a )
		{
			a[j] = a[j] + 1;
			j = j + 1;
		}
		return = a;
	}
	d = { 1,2,3 };
	c = collectioninc( d );
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedResult = { 2, 3, 4 };
            thisTest.Verify("c", expectedResult, 0);
        }


        [Test]
        [Category("SmokeTest")]
        public void TV71_Defect_1456108_2()
        {
            string code = @"
        def collectioninc: int[]( a : int[] )
	{
		j = 0;
		a = [Imperative]
		{
			for( i in a )
			{
				a[j] = a[j] + 1;
				j = j + 1;
			}
			return = a;
		}
		return = a;
	}
	d = { 1,2,3 };
	c = collectioninc( d );
	b;
        [Imperative]
	{
		b = collectioninc( d );
	}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedResult1 = { 1, 2, 3 };
            object[] expectedResult2 = { 2, 3, 4 };
            thisTest.Verify("d", expectedResult1);
            thisTest.Verify("b", expectedResult2);
            thisTest.Verify("c", expectedResult2);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV72_Defect_1454541()
        {
            string code = @"
d1;d2;
[Associative]
{
    def singleLine1 : int( a:int ) = a * 10;
    d1 = singleLine1( 2 );
}
[Imperative]
{
    def singleLine2 : int( a:int ) = a * 10;
    d2 = singleLine2( 2 );
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("d1", 20);
            thisTest.Verify("d2", 20);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV72_Defect_1454541_1()
        {
            string code = @"
d1;d2;
[Associative]
{
    def singleLine1 : int( a:int ) { return = a * 10; } 
    d1 = singleLine1( 2 );
}
[Imperative]
{
    def singleLine2 : int( a:int ) { return = a * 10; } 
    d2 = singleLine2( 2 );
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("d1", 20);
            thisTest.Verify("d2", 20);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV72_Defect_1454541_2()
        {
            string code = @"
def singleLine1 : int( a:int[] ) = a[0] ;
d = singleLine1( {20,20} );
def singleLine2 : int[]( a:int[] ) = a ;
d1 = singleLine2( {20,20} );
d2 = d1[1];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("d", 20);
            thisTest.Verify("d2", 20);
        }


        [Test]
        [Category("SmokeTest")]
        public void TV73_Defect_1451831()
        {
            string code = @"
y;
[Associative]
{
  
	a = 1;
	b = 1;
	
	def test:int( a:int, b:int, c : int, d : int )
	{
		 
	    y = [Imperative]
		{
			if( a == b ) 
			{
				return = 1;
			}		
			else
			{
				return = 0;
			}
		}
		
		return = y + c + d;
	}
	
	c = 1;
	d = 1;
	
	y = test ( a , b, c, d);
	
		
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("y", 3, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV74_Defect_1456426()
        {
            string code = @"
def foo1 : int ( a : int )
{
	c = [Imperative]
	{
		d = 0;
		if( a > 1 )
		{
			d = 1;
		}
		return = d;	
	}
	return  = a + c;
}
b1 = foo1(3);
b2;b3;
	
[Associative]
{
    def foo : int ( a : int )
	{
		c = [Imperative]
		{
		    d = 0;
			if( a > 1 )
			{
				d = 1;
			}
			return = d;	
		}
		return  = a + c;
	}
	
	b2 = foo (2 );	
}
[Imperative]
{
    def foo2 : int ( a : int )
	{
		
		d = 0;
		if( a > 1 )
		{
			d = 1;
		}
			
		return = a + d;
	}
	
	
	b3 = foo2 (4 );	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b1", 4);
            thisTest.Verify("b2", 3);
            thisTest.Verify("b3", 5);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV75_Defect_1456870()
        {
            string code = @"
def foo1 ( b )
{
	return = b == 0 ? 0 : b+1;
	
}
def foo2 ( x )
{
	y = [Imperative]
	{
	    if(x > 0)
		{
		   return = x >=foo1(x) ? x : foo1(x);
		}
		return = x >=2 ? x : 2;
	}
	x1 = y == 0 ? 0 : y;
	return = x1 + y;
}
a1 = foo1(4);
a2 = foo2(3);
//thisTest.Verification(mirror, ""a1"", 5, 0); // fails";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a1", 5, 0);
            thisTest.Verify("a2", 8, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV76_Defect_1456112()
        {
            string code = @"
def foo1 : double (arr : double[])
{
    return = 0;
}
arr = {1,2,3,4};
sum = foo1(arr);
def foo2 : double (arr : double)
{
    return = 0;
}
arr1 = {1.0,2.0,3.0,4.0};
sum1 = foo2(arr1);
sum2 = foo1(arr);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] sum1 = new Object[] { 0.0, 0.0, 0.0, 0.0 };
            thisTest.Verify("sum", 0.0, 0);
            thisTest.Verify("sum1", sum1, 0);
            thisTest.Verify("sum2", 0.0, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV76_Defect_1456112_2()
        {
            string code = @"
def foo1 : double (arr : double[][])
{
    return = arr[0][0];
}
sum1;sum2;
[Imperative]
{
	arr1 = { {1, 2.0}, {true, 4} };
	sum1 = foo1(arr);
	x = 1;
	arr2 = { {1, 2.0}, {x, 4} };
	sum2 = foo1(arr2);
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object n1 = null;
            thisTest.Verify("sum1", n1);
            thisTest.Verify("sum2", 1.0);

        }

        [Test]
        [Category("Method Resolution")]
        public void TV77_Defect_1455259()
        {

            //Assert.Fail("1463703 - Sprint 20 : rev 2147 : regression : ambiguous warning and wrong output for cases where the class method name and global method name is same ");
            // Assert.Fail("1467080 - sprint23 : rev 2681 : method overload issue over functions with same name and signature in multiple language blocks");

            string code = @"
def foo2 : int ( a : int )
{
	return  = a + 1;
}
class A
{
    a1: var;
	constructor A ( a)
	{
	    a1 = a;
	}
	def foo2  ( a : int )
	{
	    return  = a + a1;
	}
    
}
x1;y11;y21;
x2;y12;y22;
b;
[Associative]
{
    def foo : int ( a : int )
	{
		return  = a + 1;
	}
	
	def foo1  ( a  )
	{
		return  = a + 1;
	}
	
	x1 = 1;
	y11 = foo(x1);
	y21 = foo1(x1);
	
}
[Imperative]
{
    def foo : int ( a : int )
	{
		return  = a + 1;
	}
	
	def foo1  ( a  )
	{
		return  = a + 1;
	}
	
	x2 = 1;	
	y12 = foo(x2);
	y22 = foo1(x2);
	a = A.A(1);
	b = a.a1;
	c = a.foo2(1);	
	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x1", 1);
            thisTest.Verify("y11", 2);
            thisTest.Verify("y21", 2);
            thisTest.Verify("x2", 1);
            thisTest.Verify("y12", 2);
            thisTest.Verify("y22", 2);
            thisTest.Verify("b", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV77_Defect_1455259_2()
        {
            // Assert.Fail("1463703 - Sprint 20 : rev 2147 : regression : ambiguous warning and wrong output for cases where the class method name and global method name is same ");

            string code = @"
def foo2 : int ( a : int )
{
	return  = a + 1;
}
class A
{
    a1: var;
	constructor A ( a)
	{
	    a1 = a;
	}
	def foo2  ( a : int )
	{
	    return  = a + a1;
	}
    
}
b;c;d;
[Imperative]
{
    
	a = A.A(1);
	b = a.a1;
	c = a.foo2(1);	
	d = foo2(1);
	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", 1, 0);
            thisTest.Verify("d", 2, 0);
            thisTest.Verify("c", 2, 0);
        }

        [Test]
        [Category("Method Resolution")]
        [Category("Failure")]
        public void TV78_Defect_1460866()
        {
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4013
            string code = @"
def foo( a:int )
{
    b = a + 1;
}
y = foo ( 1 );
x = [Imperative]
{
	def foo2(x:int)
	{
	    if (x > 0)
		      return = 1;
	}
	t = foo2(-1);
	return  = t;
}
z1 = [Imperative]
{
	a = 1;
	if ( a > 2 ) 
	    return = 2;
}
z2 = [Imperative]
{
	def foo2(x:int)
	{
	    if (x > 0)
		      return = 1;
	}
	t = foo2(1);
	return  = t;	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object v1 = null;
            thisTest.Verify("x", v1);
            thisTest.Verify("y", v1);
            thisTest.Verify("z1", v1);
            thisTest.Verify("z2", 1);

            thisTest.VerifyRuntimeWarningCount(0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV78_Defect_1460866_2()
        {
            string code = @"
class A
{
    a : int;
    constructor A ( a1 : int )
    {
        a = [Imperative]
	{
	    if(a1 > 2 )
	        return = 2;
	}
    }
    def foo ()
    {
        x = [Imperative]
	{
	    if(a == 2 )
	        return = true;
	}
	return  = x;
    }
}
x = A.A( 3 );
y = x.a;
z = x.foo();
x2 = A.A( 1 );
y2 = x2.a;
z2 = x2.foo();
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object v1 = null;
            thisTest.Verify("y", 2, 0);
            thisTest.Verify("z", true, 0);
            thisTest.Verify("y2", v1, 0);
            thisTest.Verify("z2", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV79_Defect_1462300()
        {
            string code = @"
def testcall(a:test) 
{ 
return ={a.ImNotDefined(),a.ImNotDefined()}; 
} 
class test 
{ 
p1 : var; 
constructor test() 
{ 
p1=1; 
} 
}; 
a= test.test(); 
b=testcall(a); 
aa = b[0]; 
bb = b[1]; 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object v1 = null;
            Object[] v2 = new Object[] { null, null };
            thisTest.Verify("aa", v1);
            thisTest.Verify("bb", v1);
            thisTest.Verify("b", v2);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV79_Defect_1462300_2()
        {
            string code = @"
def testcall(a:test) 
{ 
return ={a.ImNotDefined(),a.ImNotDefined()}; 
} 
class test 
{ 
p1 : var; 
constructor test() 
{ 
p1=1; 
} 
}; 
a= null; 
b=testcall(a); 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v2 = new Object[] { null, null };
            thisTest.Verify("b", v2);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV79_Defect_1462300_3()
        {
            //Assert.Fail("1462300 - sprint 19 - rev 1948-316037 - if return an array of functions as properties then it does not return all of them");

            string code = @"
def testcall2 :test[] () 
{ 	
	return ={test.test(), test.test()}; 
} 
def testcall3 :int[] () 
{ 	
	return ={test.test().x, test.test().y}; 
} 
def testcall4 :test[] () 
{ 	
	return ={null, null}; 
} 
class test 
{ 
	x : var;
	y : var;
}; 
b = testcall2(); 
//c = b.x; // expected : { null, null }
d = testcall3(); // expected : { null, null }
e1 = testcall4(); // expected : { null, null }
//f = e1.y; // expected : { null, null }
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v2 = new Object[] { null, null };
            thisTest.Verify("d", v2);
            thisTest.Verify("e1", v2);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV80_Function_Name_Negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
def function ( a : int)
{
    return = a + 1;
}
x = function(2);";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });

        }


        [Test]
        [Category("SmokeTest")]
        public void TV81_Defect_1458187()
        {
            string code = @"
def foo ( a )
{
	x = [Imperative]
	{
		if ( a == 0 )
		return = a;
		else
		return = a + 1;
	}
	return = x;
}
a = foo( 2 );
b = foo(false);
c = foo(true);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 3, 0);
            thisTest.Verify("b", false, 0);
            Assert.IsTrue(mirror.GetValue("c", 0).DsasmValue.IsNull);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV81_Defect_1458187_2()
        {
            string code = @"
def foo ( a )
{
	x = (a == 1)?a:0;
	return = x + a;
}
a = foo( 2 );
b = foo(false);
c = foo(true);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 2, 0);
            Assert.IsTrue(mirror.GetValue("b", 0).DsasmValue.IsNull);
            Assert.IsTrue(mirror.GetValue("c", 0).DsasmValue.IsNull);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV82_Defect_1460892()
        {
            string code = @"
def foo ( a : int )
{
    return  = a + 1;
}
def foo2 ( b : int, f1 : function )
{
    return = f1( b );
}
a = foo2 ( 10, foo );
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 11);

        }

        [Test]
        [Category("SmokeTest")]
        public void TV83_Function_Pointer()
        {
            string code = @"
def foo ( a : bool )
{
    return  = a ;
}
def foo2 ( b : int, f1 : function )
{
    return = f1( b );
}
a = foo2 ( 0, foo );
def poo ( a : int )
{
    return  = a ;
}
def poo2 ( b : bool, f1 : function )
{
    return = f1( b );
}
a2 = poo2 ( false, poo );
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object n1 = null;
            thisTest.Verify("a", false);
            thisTest.Verify("a2", n1);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV83_Function_Pointer_Collection()
        {
            string code = @"
def count ( a : bool[]..[] )
{
    c = 0;
	c = [Imperative]
	{
	    for ( i in a )
		{
		    c = c + 1;
		}
		return = c;
	}
	return  = c ;
}
def foo ( b : bool[]..[], f1 : function )
{
    return = count( b );
}
a = foo ( { true, false, { true, true } },  count );
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 3);
        }

        [Test]
        [Category("Type System")]
        public void TV84_Function_Pointer_Implicit_Conversion()
        {

            string code = @"
def count ( a : int[] )
{
    c = 0;
	x = [Imperative]
	{
	    for ( i in a )
		{
		    c = c + 1;
		}
		return = c;
	}
	return  = x ;
}
def foo ( b : double[], f1 : function )
{
    return = count( b );
}
a = foo ( { 1.0, 2.6 },  count );
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("a", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV84_Function_Pointer_Implicit_Conversion_2()
        {
            string code = @"
def count ( a : double[]..[] )
{
    c = 0;
	x = [Imperative]
	{
	    for ( i in a )
		{
		    c = c + 1;
		}
		return = c;
	}
	return  = x ;
}
def foo ( b : int[]..[], f1 : function )
{
    return = count( b );
}
a = foo ( { 1, 2 , {3, 4} },  count );
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("a", 3);
        }

        [Test]
        [Category("Failure")]
        public void TV84_Function_Pointer_Implicit_Conversion_3()
        {
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4748
            string code = @"
def count ( a : double[] )
{
    c = 0;
	x = [Imperative]
	{
	    for ( i in a )
		{
		    c = c + 1;
		}
		return = c;
	}
	return  = x ;
}
def foo ( b : int[], f1 : function )
{
    return = f1( b );
}
a = foo ( { 1, 2,  { 3, 4 } },  count );
d = foo ( { 2, 2.5, { 1, 1.5 }, 1 , false},  count );

";
            string err = "MAGN-4748: Replication Unboxing error";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, err);
            thisTest.Verify("a", new object[] { 1, 1, 2 });
            thisTest.Verify("d", new object[] { 1, 1, 2, 1, 1 });
        }

        [Test]
        [Category("SmokeTest")]
        [Category("Failure")]
        public void TV84_Function_Pointer_Implicit_Conversion_4()
        {
            // Tracked by: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4014
            string code = @"
def count ( a : double[] )
{
    c = 0;
	x = [Imperative]
	{
	    for ( i in a )
		{
		    c = c + 1;
		}
		return = c;
	}
	return  = x ;
}
def foo ( b : int[], f1 : function )
{
    return = f1( b );
}
[Imperative]
{
	a = foo ( { 1, 2,  { 3, 4 } },  count );
	d = foo ( { 2, 2.5, { 1, 1.5 }, 1 , false},  count );
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] {1,1,2});
            thisTest.Verify("d", new object[] {1,1,2,1,1});
        }

        [Test]
        [Category("SmokeTest")]
        public void TV84_Function_Pointer_Using_2_Functions()
        {
            string code = @"
def greater ( a , b )
{
    c = [Imperative]
	{
	    if ( a > b )
		    return = a;
		else
		   return = b;
	}
	return  = c ;
}
def greatest ( a : double[], greater : function )
{
    c = a[0];
	[Imperative]
	{
	    for ( i in a )
		{
		    c = greater( i, c );
		}	
	}
	return  = c ;
}
def foo ( a : double[], greatest : function , greater : function)
{
    return  = greatest ( a, greater );
}
a;
[Imperative]
{
	a = foo ( { 1.5, 6, 3, -1, 0 }, greatest, greater );
	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 6.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV84_Function_Pointer_Negative()
        {
            string error = "1467379 Sprint 27 - Rev 4193 - after throwing warning / error in the attached code it should execute rest of the code ";
            string code = @"def greater ( a , b )
{
    c = [Imperative]
	{
	    if ( a > b )
		    return = a;
		else
		   return = b;
	}
	return  = c ;
}
def greatest ( a : double[], f : function )
{
    c = a[0];
	[Imperative]
	{
	    for ( i in a )
		{
		    c = f( i, c );
		}	
	}
	return  = c ;
}
a;
[Imperative]
{
	a = greatest ( { 1.5, 6, 3, -1, 0 }, greater2 );
	
}
";
            thisTest.VerifyRunScriptSource(code, error);
            Object v = null;
            thisTest.Verify("a", v);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV85_Function_Return_Type_Var_User_Defined_Type_Conversion()
        {
            string code = @"
class A
{
    x : int;
}
def goo : var()
{
    return = A.A();
}
def foo : A ()
{
    return = goo();
}
a = foo();
b = a.x;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("b", 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV86_Defect_1456728()
        {
            string code = @"
def f1 (arr :  double[] )
{
    return = arr;
}
def f2 (arr :  double[] )
{
    return = { arr[0], arr[1] };
}
a = f1( { null, null } );
b = f2( { null, null } );
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v2 = new Object[] { null, null };
            thisTest.Verify("a", v2);
            thisTest.Verify("b", v2);
        }


        [Test]
        [Category("SmokeTest")]
        public void TV87_Defect_1464027()
        {
            string code = @"
class A
{
    x : double;
	y : int;
	z : bool;
	a : int[];
	b : B;
}
class B
{
    b : double;
	
}
def goo : var()
{
    return = A.A();
}
def foo : A ()
{
    return = goo();
}
a = foo();
t1 = a.x;
t2 = a.y;
t3 = a.z;
t4 = a.a;
t5 = a.b.b;
t11;t12;t13;t14;t15;
[Imperative]
{
    t11 = a.x;
	t12 = a.y;
	t13 = a.z;
	t14 = a.a;
	t15 = a.b.b;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object v1 = null;
            thisTest.Verify("t1", 0.0);
            thisTest.Verify("t2", 0);
            thisTest.Verify("t3", false);
            object a = new object[] { };
            thisTest.Verify("t4", a);
            thisTest.Verify("t5", v1);
            thisTest.Verify("t11", 0.0);
            thisTest.Verify("t12", 0);
            thisTest.Verify("t13", false);
            thisTest.Verify("t14", a);
            thisTest.Verify("t15", v1);

        }

        [Test]
        [Category("SmokeTest")]
        public void TV87_Defect_1464027_2()
        {
            string code = @"
class A
{
    x : double;
	y : int;
	z : bool;
	a : int[];
	b : B;
}
class B
{
    b : double;
	
}
def goo : var[]()
{
    return = {A.A(), A.A()};
}
def foo : A[] ()
{
    return = {A.A(), A.A()};
}
a1 = foo();
a = a1[1];
t1 = a.x;
t2 = a.y;
t3 = a.z;
t4 = a.a;
t5 = a.b.b;
t11;t12;t13;t14;t15;
[Imperative]
{
    t11 = a.x;
	t12 = a.y;
	t13 = a.z;
	t14 = a.a;
	t15 = a.b.b;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t1", 0.0);
            thisTest.Verify("t2", 0);
            thisTest.Verify("t3", false);
            object a = new object[] { };
            thisTest.Verify("t4", a);
            thisTest.Verify("t5", null);
            thisTest.Verify("t11", 0.0);
            thisTest.Verify("t12", 0);
            thisTest.Verify("t13", false);
            thisTest.Verify("t14", a);
            thisTest.Verify("t15", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV87_Defect_1464027_3()
        {
            string code = @"
class A
{
    x : double;
	y : int;
	z : bool;
	a : int[];
	b : B;
	
	def goo : var()
	{
		return = A.A();
	}
}
class B
{
    b : double;
	
}
def foo : A (x:A)
{
    return = x.goo();
}
a1 = A.A();
a = foo ( a1 );
t1 = a.x;
t2 = a.y;
t3 = a.z;
t4 = a.a;
t5 = a.b.b;
t11;t12;t13;t14;t15;
[Imperative]
{
    t11 = a.x;
	t12 = a.y;
	t13 = a.z;
	t14 = a.a;
	t15 = a.b.b;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t1", 0.0);
            thisTest.Verify("t2", 0);
            thisTest.Verify("t3", false);
            object a = new object[] { };
            thisTest.Verify("t4", a);
            thisTest.Verify("t5", null);
            thisTest.Verify("t11", 0.0);
            thisTest.Verify("t12", 0);
            thisTest.Verify("t13", false);
            thisTest.Verify("t14", a);
            thisTest.Verify("t15", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV87_Defect_1464027_4()
        {
            string code = @"
class A
{
    x : double;
	y : int;
	z : bool;
	a : int[];
	b : B;
	
	def goo : A()
	{
		return = A.A();
	}
}
class B
{
    b : double;
	
}
def foo : var (x:A)
{
    return = x.goo();
}
a1 = A.A();
a = foo ( a1 );
t1 = a.x;
t2 = a.y;
t3 = a.z;
t4 = a.a;
t5 = a.b.b;
t11;t12;t13;t14;t15;
[Imperative]
{
    t11 = a.x;
	t12 = a.y;
	t13 = a.z;
	t14 = a.a;
	t15 = a.b.b;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t1", 0.0);
            thisTest.Verify("t2", 0);
            thisTest.Verify("t3", false);
            object a = new object[] { };
            thisTest.Verify("t4", a);
            thisTest.Verify("t5", null);
            thisTest.Verify("t11", 0.0);
            thisTest.Verify("t12", 0);
            thisTest.Verify("t13", false);
            thisTest.Verify("t14", a);
            thisTest.Verify("t15", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV87_Defect_1464027_5()
        {
            string code = @"
class A
{
    x : double;
	y : int;
	z : bool;
	a : int[];
	b : B;
	
	def goo :var()
	{
		return = A.A();
	}
}
class B
{
    b : double;
	
}
def foo : var (x:A)
{
    return = x.goo();
}
a1 = A.A();
a = foo ( a1 );
t1 = a.x;
t2 = a.y;
t3 = a.z;
t4 = a.a;
t5 = a.b.b;
t11;t12;t13;t14;t15;
[Imperative]
{
    t11 = a.x;
	t12 = a.y;
	t13 = a.z;
	t14 = a.a;
	t15 = a.b.b;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t1", 0.0);
            thisTest.Verify("t2", 0);
            thisTest.Verify("t3", false);
            object a = new object[] { };
            thisTest.Verify("t4", a);
            thisTest.Verify("t5", null);
            thisTest.Verify("t11", 0.0);
            thisTest.Verify("t12", 0);
            thisTest.Verify("t13", false);
            thisTest.Verify("t14", a);
            thisTest.Verify("t15", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV88_Defect_1463489()
        {
            string code = @"
def foo: bool (  )
{
	return = 0.24;
}
c = foo ( ); //expected true, received 
d = [Imperative]
{
    return = foo();
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", true);
            thisTest.Verify("d", true);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV88_Defect_1463489_2()
        {
            string code = @"
def foo: bool ( x : bool )
{
	return = x && true;
}
c = foo ( 0.6 ); 
c1 = foo ( 0.0 ); 
d = [Imperative]
{
    return = foo(-3.5);
}
d1 = [Imperative]
{
    return = foo(0.0);
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", true);
            thisTest.Verify("d", true);
            thisTest.Verify("c1", false);
            thisTest.Verify("d1", false);
        }

        [Test]
        [Category("Design Issue")]
        [Category("Update")]
        [Category("Failure")]
        public void TV88_Defect_1463489_3()
        {
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1510
            string src = string.Format("{0}{1}", testPath, "TV88_Defect_1463489_3.ds");
            fsr.LoadAndPreStart(src, runnerConfig);
            ProtoCore.CodeModel.CodePoint cp = new ProtoCore.CodeModel.CodePoint
            {
                CharNo = 8,
                LineNo = 35,
                SourceLocation = new ProtoCore.CodeModel.CodeFile
                {
                    FilePath = Path.GetFullPath(src)
                }
            };
            fsr.ToggleBreakpoint(cp);
            ProtoScript.Runners.DebugRunner.VMState vms = fsr.Run();
            thisTest.DebugModeVerification(vms.mirror, "y1", true);
            thisTest.DebugModeVerification(vms.mirror, "y3", true);
            thisTest.DebugModeVerification(vms.mirror, "y2", false);
            thisTest.DebugModeVerification(vms.mirror, "y4", false);
            fsr.Run();
            thisTest.DebugModeVerification(vms.mirror, "y1", false);
            thisTest.DebugModeVerification(vms.mirror, "y3", false);
            thisTest.DebugModeVerification(vms.mirror, "y2", true);
            thisTest.DebugModeVerification(vms.mirror, "y4", true);

        }

        [Test]
        [Category("SmokeTest")]
        public void TV89_Implicit_Type_Conversion_1()
        {
            string code = @"
class A
{
    y : bool;
	constructor A ( x : bool )
	{
	    y = x && true;
	}
	def foo: bool ( x : bool )
	{
		[Imperative]
		{
		    if( x == false )
			    y = true;
			else
			    y = false;
		}		
		return = y;
	}
}
c = A.A ( 6 );
y1 = c.foo( 6 );
c1 = A.A ( 0 );
y2 = c1.foo( 0 ); 
d = [Imperative]
{
    return = A.A ( -3 );
}
y3 = d.foo ( -3 );
d1 = [Imperative]
{
    return = A.A ( 0 );
}
y4 = d1.foo ( 0 );
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("y1", false);
            thisTest.Verify("y3", false);
            thisTest.Verify("y2", true);
            thisTest.Verify("y4", true);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV90_Defect_1463474()
        {
            //Assert.Fail("1461363 - Sprint 19 : rev 1800 : Global variable support is not there in function scope ");

            string code = @"
a;b;
[Associative]
{
	 a = 1;
	 [Imperative]
	 {
		def foo : void  ( )
		{
			a = 2;		
		}
		foo();
        b = a;	    
	 }
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 2);
            thisTest.Verify("b", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV90_Defect_1463474_2()
        {
            string code = @"
a = 3;
def foo : void  ( )
{
	a = 2;		
}
foo();
b1 = a;	
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b1", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV90_Defect_1463474_3()
        {
            //Assert.Fail("1461363 - Sprint 19 : rev 1800 : Global variable support is not there in function scope "); 
            //Assert.Fail("1467156 - Sprint 25 - Rev 3026 type checking of return types at run time ");
            string code = @"
a = 3;
def foo : void  (  )
{
	a = 2;
    return = -3;	
}
c1 = foo();
b1 = a;	
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object v1 = null;
            thisTest.Verify("c1", v1);
            thisTest.Verify("b1", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV90_Defect_1463474_4()
        {
            string code = @"
class A
{
    a :int = 3;
	def foo : void  ( )
	{
		a = 2;		
	}
}
c1 = A.A();
b1 = c1.foo();
d1 = c1.a;	
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object v1 = null;
            thisTest.Verify("b1", v1);
            thisTest.Verify("d1", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV91_Defect_1463703()
        {
            string code = @"
def foo2 : int ( a : int )
{
	return  = a + 1;
}
class A
{
    a1: var;
	constructor A ( a)
	{
	    a1 = a;
	}
	def foo2  ( a : int )
	{
	    return  = a + a1;
	}
    
}
b;c;d;
[Imperative]
{
	a = A.A(1);
	b = a.a1;
	c = a.foo2(1);
	d = foo2(1);	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("c", 2);
            thisTest.Verify("d", 2);
            thisTest.Verify("b", 1);
        }

        [Test]

        public void TV91_Defect_1463703_2()
        {
            //This failure is no longer related to this defect. Back to TD.
            //Assert.Fail("1466269 - Sprint 22 - rev 2418 - Regression : DS throws warning Multiple type+pattern match parameters found, non-deterministic dispatch ");
            string error = "1467080 sprint23 : rev 2681 : method overload issue over functions with same name and signature in multiple language blocks";
            string code = @"def foo2 : int ( a : int )
{
	return  = a + 1;
}
class A
{
    a1: var;
	constructor A ( a)
	{
	    a1 = a;
	}
	def foo2  ( a : int )
	{
	    return  = a + a1;
	}
    
}
y1;y2;y3;y4;
[Associative]
{
    def foo : int ( a : int )
	{
		return  = a + 1;
	}
	
	def foo1  ( a  )
	{
		return  = a + 1;
	}
	
	x = 1;
	y1 = foo(x);
	y2 = foo1(x); //warning : (-1,-1) Warning:Ambiguous method dispatch.
	
}
[Imperative]
{
    def foo : int ( a : int )
	{
		return  = a + 1;
	}
	
	def foo1  ( a  )
	{
		return  = a + 1;
	}
	
	x = 1;	
	y3 = foo(x);
	y4 = foo1(x); //warning : (-1,-1) Warning:Ambiguous method dispatch.
	a = A.A(1);
	b = a.a1;
	c = a.foo2(1);	
	
}
";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("y1", 2);
            thisTest.Verify("y2", 2);
            thisTest.Verify("y3", 2);
            thisTest.Verify("y4", 2);
        }

        [Test]
        [Category("Method Resolution")]
        [Category("Failure")]
        public void TV91_Defect_1463703_3()
        {
            // Tracked  by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4013
            string error = "MAGN-4013 Method overload issue over functions with same name and signature in multiple language blocks";
            string code = @"def foo2 : int[] ( a : int[] )
{
	return  = a;
}
class A
{
    a1: var[];
	constructor A ( a: var[])
	{
	    a1 = a;
	}
	def foo2  ( a : int[] )
	{
	    a1 = a;
		return  = a1 ;
	}
    
}
x1 = 
[Associative]
{
    def foo : int[] ( a : int[] )
	{
		a[0] = 0;
		return  = a;
	}
	
	def foo1  ( a : var[]  )
	{
		a[0] = 1;
		return  = a;
	}
	
	x = {9,9};
	y1 = foo(x);
	y2 = foo1(x); //warning : (-1,-1) Warning:Ambiguous method dispatch.
	return = { y1, y2};
}
x2 = 
[Imperative]
{
    def foo : int[] ( a : int[] )
	{
		a[0] = 2;
		return  = a ;
	}
	
	def foo1  ( a : var [] )
	{
		a[0] = 4;
		return  = a ;
	}
	
	x = { 9, 9 };	
	y3 = foo(x);
	y4 = foo1(x); //warning : (-1,-1) Warning:Ambiguous method dispatch.
	a = A.A({1,2});
	b = a.a1;
	c = a.foo2({3,4});	
	return = { y3, y4, b, c };
	
}
";
            thisTest.VerifyRunScriptSource(code, error);

            Object[] v1 = new Object[] { new Object[] { 0, 9 }, new Object[] { 1, 9 } };
            Object[] v2 = new Object[] { new Object[] { 2, 9 }, new Object[] { 4, 9 }, new Object[] { 1, 2 }, new Object[] { 3, 4 } };
            thisTest.Verify("x1", v1);
            thisTest.Verify("x2", v2);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV92_Accessing_Variables_Declared_Inside_Function_Body()
        {
            string code = @"
class A
{
    a : var;
    constructor A ( y )
    {
        a = y;
    }
}
def foo ( )
{
    a = { 1, 2, 3};
    b = A.A(10);
    return = {a,b};
}
x = foo ();
a = x[0][0]; // expected 1, received 1
b = x[1];
c = b.a; // expected 10, received 10";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("a", 1);
            thisTest.Verify("c", 10);
        }

        [Test]
        public void TV93_Modifying_Global_Var_In_Func_Call()
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
	def foo ( p: Point)
	{
	    return = Point.Point( (p.X), (p.Y), (p.Z) );
	}
}
def func1(pts : Point[]) 
{
  pts[1] = pts[0];
  return = null;
}
p1 = Point.Point ( 0,0,0);
p2 = Point.Point ( 1, 1, 1 );
p = { p1, p2 }; 
xx = p[1].X;
yy = p[1].Y;
zz = p[1].Z;
dummy = func1(p);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            // Assert.Fail("1466768 - Sprint 23 : rev 2479 : global variables are not getting updated through function calls");
            thisTest.Verify("xx", 1);
            thisTest.Verify("yy", 1);
            thisTest.Verify("zz", 1);
        }

        [Test]
        public void TV93_Modifying_Global_Var_In_Func_Call_2()
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
	def foo ( p: Point)
	{
	    return = Point.Point( (p.X), (p.Y), (p.Z) );
	}
}
def func1(pts : Point[]) 
{
  pts[1] = pts[0];
  return = null;
}
p1 = 0;
p2 = 1;
p = { p1, p2 }; 
xx = p[1];
yy = p[1];
zz = p[1];
dummy = func1(p);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            // Assert.Fail("1466768 - Sprint 23 : rev 2479 : global variables are not getting updated through function calls");

            // This test case doesn't make sense. Should be updated or deleted. -- Yu Ke
            thisTest.Verify("xx", 1);
            thisTest.Verify("yy", 1);
            thisTest.Verify("zz", 1);
        }

        [Test]
        public void TV93_Modifying_Global_Var_In_Func_Call_3()
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
	def foo ( p: Point)
	{
	    return = Point.Point( (p.X), (p.Y), (p.Z) );
	}
}
def func1(p1 : Point, p2 : Point) 
{
  p1 = p1.foo(p2);
  return = null;
}
p1 = Point.Point ( 0,0,0);
p2 = Point.Point ( 1, 1, 1 );
dummy = func1(p1, p2);
xx = p1.X; 
yy = p1.Y; 
zz = p1.Z; 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            // Assert.Fail("1466768 - Sprint 23 : rev 2479 : global variables are not getting updated through function calls");
            thisTest.Verify("xx", 0);
            thisTest.Verify("yy", 0);
            thisTest.Verify("zz", 0);
        }

        [Test]
        public void TV93_Modifying_Global_Var_In_Func_Call_4()
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
	def foo ( p: Point)
	{
	    return = Point.Point( (p.X+X), (p.Y+Y), (p.Z+Z) );
	}
}
def UnionBox(combined : Point, arr : Point[], index : int) {
  
  // Nothing changed for combined outside Unionbox()
  combined = combined.foo(arr[index]);
  [Imperative]
  {
	  if(index != 0 )
	  {
		  combined = UnionBox(combined, arr, index - 1);
	  }	  
  }
  return = null;
}
points = { Point.Point ( 0,0,0), Point.Point ( 1, 1, 1 ), Point.Point ( 2, 2, 2 ), Point.Point ( 3, 3, 3 ) };
top_index = Count( points ) - 2;
base_index = Count ( points ) -1;
s = points[base_index];
xx = s.X;
yy = s.Y;
zz = s.Z;
s1 = UnionBox(s, points, top_index);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            // Assert.Fail("1466768 - Sprint 23 : rev 2479 : global variables are not getting updated through function calls");
            thisTest.Verify("xx", 3);
            thisTest.Verify("yy", 3);
            thisTest.Verify("zz", 3);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV95_Method_Resolution_Derived_Class_Arguments()
        {

            string code = @"
class A
{
}
class B extends A
{
}
def Test(arr : A[])
{
        return = 123;
}
a = {B.B(), B.B(), B.B()};
t = Test(a);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("DNL-1467027 - Sprint23 : rev 2529 : Method resolution issue over derived class arguments");

            thisTest.Verify("t", 123);

        }

        [Test]
        [Category("SmokeTest")]
        public void TV96_Defect_DNL_1465794()
        {

            string code = @"
x = [Imperative]
{    
    a = 1;    
	def foo : int  ( )    
	{        
	    c = a;        
		a = 2;                    
		return = c + 1;                
	}    
	b = foo();        
	return = { a, b };    
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 2, 2 };

            thisTest.Verify("x", v1);


        }

        [Test]
        [Category("SmokeTest")]
        public void TV96_Defect_DNL_1465794_2()
        {
            string code = @"
x = [Imperative]
{    
        a = 2;    
	def foo : int  ( )    
	{        
	    c = 0;
	    if ( a > 1 )
	    {
	        for ( i in 0..1 )
                {
		    a = a + i;
                }
                c = 1;                                    
	    }
	    return = c;                
	}    
	b = foo();        
	return = { a, b };    
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 3, 1 };
            thisTest.Verify("x", v1);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV96_Defect_DNL_1465794_3()
        {
            string code = @"
x = [Imperative]
{    
        a = { { 1, 2 } , { 3, 4 } };    
	def foo : int  ( )    
	{        
	    c = 0;
	    if ( a[0][0] == 1 )
	    {
	        for ( i in 0..1 )
                {
		    a[0][0] = a[0][0] + i;
                }
                c = 1;                                    
	    }
	    return = c;                
	}    
	b = foo();        
	return = { a, b };    
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { new Object[] { 2, 2 }, new Object[] { 3, 4 } };
            Object[] v2 = new Object[] { v1, 1 };
            thisTest.Verify("x", v2);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV97_Heterogenous_Objects_As_Function_Arguments_No_Replication()
        {
            string code = @"
class A
{
}
a1 = A.A();
def foo ( x : double[])
{
    return = x;
}
a1 = { 2.5, 4, 3*2 };
b1 = foo ( a1 );
a2 = { 2, 4, 3.5 };
b2 = foo ( a2 );
a3 = { 2, 4, 3 };
b3 = foo ( a3 );
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            Object[] v1 = new Object[] { 2.5, 4.0, 6.0 };
            Object[] v2 = new Object[] { 2.0, 4.0, 3.5 };
            Object[] v3 = new Object[] { 2.0, 4.0, 3.0 };
            thisTest.Verify("b1", v1);
            thisTest.Verify("b2", v2);
            thisTest.Verify("b3", v3);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV97_Heterogenous_Objects_As_Function_Arguments_No_Replication_2()
        {
            string code = @"
class A
{
    t1 : var;
    constructor A ( t2 :  var )
    {
        t1 = t2;
    }
}
a = A.A(1);
def foo ( x : var[])
{
    return = 1;
}
a1 = { 2.5, null, 3, a, ""sar"" };
b1 = foo ( a1 );
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("b1", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV97_Heterogenous_Objects_As_Function_Arguments_With_Replication()
        {
            string code = @"
def foo ( x : double)
{
    return = x;
}
a1 = { 2.5, 4 };
b1 = foo ( a1 );
a2 = { 3, 4, 2.5 };
b2 = foo ( a2 );
a3 = { 3, 4, 2 };
b3 = foo ( a3 );
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            Object[] v1 = new Object[] { 2.5, 4.0 };
            Object[] v2 = new Object[] { 3.0, 4.0, 2.5 };
            Object[] v3 = new Object[] { 3.0, 4.0, 2.0 };
            thisTest.Verify("b1", v1);
            thisTest.Verify("b2", v2);
            thisTest.Verify("b3", v3);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV97_Heterogenous_Objects_As_Function_Arguments_With_Replication_2()
        {
            string code = @"
class A
{
}
a1 = A.A();
def foo ( x : var)
{
    return = 1;
}
a1 = { 2.5, null, 3, a1, ""sar"" };
b1 = foo ( a1 );";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 1, 1, 1, 1, 1 };
            thisTest.Verify("b1", v1);
        }

        [Test]
        [Category("Method Resolution")]
        public void TV98_Method_Overload_Over_Rank_Of_Array()
        {
            string code = @"
def foo(x : int[])
{ 
    return = 1;
}
def foo(x : int[]..[])
{ 
    return = 2;
}
def foo(x : int[][])
{ 
    return = 0;
}
    
x = foo ( { { 0,1}, {2, 3} } );
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("x", 0);
        }

        [Test]
        public void TV89_typeConversion_FunctionArguments_1467060()
        {
            //Assert.Fail("1467060 - Sprint 23 : rev 2570 : Method resolution fails when passed a heterogenous array of double and int to a function which expects double ");
            string code = @"
def foo ( x : double[])
{
    return = x;
}
a2 = { 2, 4, 3.5 };
b2 = foo (a2);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] b2 = new Object[] { 2.0, 4.0, 3.5 };
            thisTest.Verify("b2", b2);
        }

        [Test]
        public void TV89_typeConversion_FunctionArguments_1467060_2()
        {
            //Assert.Fail("1467060 - Sprint 23 : rev 2570 : Method resolution fails when passed a heterogenous array of double and int to a function which expects double ");
            string code = @"
def foo ( x : double[])
{
    return = x;
}
a2 = { 2, 4, 3};
b2 = foo ( a2 );";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] b2 = new Object[] { 2.0, 4.0, 3.0 };
            thisTest.Verify("b2", b2);
        }

        [Test]
        [Category("Type System")]
        public void TV89_typeConversion_FunctionArguments_1467060_3()
        {
            //This failure is no longer related to this defect. Back to TD.
            //New defect - no converting type arguments correctly
            //Assert.Fail("1467060 - Sprint 23 : rev 2570 : Method resolution fails when passed a heterogenous array of double and int to a function which expects double ");
            string code = @"
def foo ( x : int[])
{
    return = x;
}
a1 = { 2, 4.1, 3.5};
b1 = foo ( a1 );";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] b1 = new Object[] { 2, 4, 4 };
            thisTest.Verify("b1", b1);
        }

        [Test]
        [Category("Type System")]
        public void TV89_typeConversion_FunctionArguments_1467060_4()
        {
            //This failure is no longer related to this defect. Back to TD.
            //New defect - no converting type arguments correctly
            //Assert.Fail("1467060 - Sprint 23 : rev 2570 : Method resolution fails when passed a heterogenous array of double and int to a function which expects double ");
            string code = @"
def foo ( x : int)
{
    return = x;
}
a1 = { 2, 4.1, false};
b1 = foo ( a1 );";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] b1 = new Object[] { 2, 4, null };
            thisTest.Verify("b1", b1);
        }

        [Test]
        [Category("Type System")]
        public void TV89_typeConversion_FunctionArguments_1467060_5()
        {
            //This failure is no longer related to this defect. Back to TD.
            //New defect - no converting type arguments correctly
            //Assert.Fail("1467060 - Sprint 23 : rev 2570 : Method resolution fails when passed a heterogenous array of double and int to a function which expects double ");
            string code = @"
def foo ( x : int[])
{
    return = x;
}
a1 = { 2, 4.1, {1,2}};
b1 = foo ( a1 );";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] b1 = new Object[] { new object[] { 2 }, new Object[] { 4 }, new object[] { 1, 2 } };
            thisTest.Verify("b1", b1);
        }

        [Test]
        [Category("Failure")]
        public void TV89_typeConversion_FunctionArguments_1467060_6()
        {
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1668

            //This failure is no longer related to this defect. Back to TD.
            //New defect - no converting type arguments correctly
            //Assert.Fail("DNL-1467060 - Sprint 23 : rev 2570 : Method resolution fails when passed a heterogenous array of double and int to a function which expects double ");
            string error = "MAGN-1668 Sprint 27 - Rev 3905 when there is rank mismatch for function , array upagrades to 1 dimension higer than expected";
            string code = @"def foo ( x : int[])
{
    return = x;
}
a1 = { null, 5, 6.0};
b1 = foo ( a1 );";
            thisTest.VerifyRunScriptSource(code, error);
            Object[] b1 = new Object[] { null, 5, 6 };
            thisTest.Verify("b1", b1);
        }


        [Test]
        public void TV89_typeConversion_FunctionArguments_1467060_9()
        {
            //This failure is no longer related to this defect. Back to TD.
            //New defect - no converting type arguments correctly
            //Assert.Fail("DNL-1467060 - Sprint 23 : rev 2570 : Method resolution fails when passed a heterogenous array of double and int to a function which expects double ");
            string code = @"
def foo ( x : int[])
{
    return = x;
}
a1 = { 1, null, 6.0};
b1 = foo ( a1 );";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] b1 = new Object[] { 1, null, 6 };
            thisTest.Verify("b1", b1);
        }

        [Test]
        public void TV89_typeConversion_FunctionArguments_1467060_7()
        {
            string code = @"
def foo ( x : int[])
{
    return = x;
}
a1 = { null, null, null};
b1 = foo ( a1 );";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] b1 = new Object[] { null, null, null };
            thisTest.Verify("b1", b1);
        }

        [Test]
        [Category("Type System")]
        public void TV89_typeConversion_FunctionArguments_1467060_8()
        {
            //Assert.Fail("DNL-1467202 Argument type casting is not happening in some cases with function calls");
            string code = @"
def foo:int[]( x : int[])
{
    return = x;
}
a1 = {1.1,2.0,3};
b1 = foo ( a1 );";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] b1 = new Object[] { 1, 2, 3 };
            //Assert.Fail("1463268 - Sprint 20 : [Design Issue] Rev 1822 : Method resolution fails when implicit type conversion of double to int is expected");
            thisTest.Verify("b1", b1);

            //string dsFullPathName = testPath + "TV89_typeConversion_FunctionArguments_1467060_8.ds";
            //ExecutionMirror mirror = thisTest.RunScript(dsFullPathName);
            //Assert.IsTrue(core.BuildStatus.ContainsWarning(ProtoCore.BuildData.WarningID.kFunctionNotFound));
        }


        [Test]
        public void TV99_Defect_1463456_Array_By_Reference_Issue()
        {
            string errmsg = " 1467318 -  Cannot return an array from a function whose return type is var with undefined rank (-2)  ";
            string code = @"class A
{
a = {1,2,3};
}
a = A.A();
val = a.a;
val[0] = 100;
t = a.a[0]; //expected 100; received 1";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("val", new object[] { 100, 2, 3 });
            thisTest.Verify("t", 1);
        }

        [Test]
        public void TV99_Defect_1463456_Array_By_Reference_Issue_2()
        {
            string code = @"
def A (a: int [])
{
return = a;
}
val = {1,2,3};
b = A(val);
b[0] = 100; 
t = val[0]; //expected 100, received 1";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t", 1);
        }

        [Test]
        public void T100_Class_inheritance_replication()
        {
            string code = @"
class A
{
        def Test : int[] (a : A)
        { return = {2}; }
}
class B extends A
{
        def Test : int (b : B)
        { return = 5; }
}
class C extends B
{
}
 
c = C.C();
b = c.Test(c);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", 5);
        }

        [Test]
        public void T100_Class_inheritance_replication_2()
        {
            // Assert.Fail("1467131- Sprint 24 - Rev 2910 method overload with replication , throws error WARNING: Multiple type+pattern match parameters found, non-deterministic dispatch" );
            string code = @"
class A
{
        def Test : int(b : B)
        { return = 2; }
}
class B extends A
{
        def Test : int[] (a : A)
        { return = {5}; }
}
class C extends B
{
}
 
c = C.C();
result = c.Test(c);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("result", 2);
        }

        [Test]
        [Category("Method Resolution")]
        public void T100_Defect_Class_inheritance_dispatch()
        {
            String code =
 @"
class A
{
        def Test(b : B)
        { return = 1; }
}
class B extends A
{
        def Test(a : A)
        { return = 2; }
}
 
a = A.A();
b = B.B();
r1 = a.Test(a);//null
r2 = b.Test(b);//1
";
            thisTest.RunScriptSource(code);
            //Assert.Fail("1467131- Sprint 24 - Rev 2910 method overload with replication , throws error WARNING: Multiple type+pattern match parameters found, non-deterministic dispatch\nunecpected result" );
            thisTest.SetErrorMessage("1467307 - Sprint 25 - Rev 3784 : Method resolution failure on member function when actual parameter is the subtype of the formal parameter type");
            Object v1 = null;
            thisTest.Verify("r1", v1);
            thisTest.Verify("r2", 1);
        }

        [Test]
        [Category("Method Resolution")]
        public void T100_Defect_Class_inheritance_dispatch_a()
        {
            String code =
 @"
class A
{
        def Test(b : B)
        { return = 1; }
}
class B extends A
{
        def Test(a : A)
        { return = 2; }
}
 
b = B.B();
r2 = b.Test(b);//1
";
            thisTest.RunScriptSource(code);
            //Assert.Fail("1467131- Sprint 24 - Rev 2910 method overload with replication , throws error WARNING: Multiple type+pattern match parameters found, non-deterministic dispatch\nunecpected result" );
            thisTest.SetErrorMessage("1467307 - Sprint 25 - Rev 3784 : Method resolution failure on member function when actual parameter is the subtype of the formal parameter type");
            thisTest.Verify("r2", 1);
        }

        [Test]
        public void T100_Defect_Class_inheritance_replication_1()
        {
            String code =
 @"
class A
{
        def Test(b : B)
        { return = 1; }
}
class B extends A
{
        def Test(a : A)
        { return = 2; }
}
class C extends B
{
    def Test(c:C)
    {return = 3;}
}
 
a = A.A();
b = B.B();
c = C.C();
r1 = a.Test(a);//null
r2 = b.Test(b);//1
r3 = c.Test(c);//3
r4 = a.Test(b);//1
r5 = a.Test(c);//1
r6 = b.Test(a);//2
r7 = b.Test(c);//1
r8 = c.Test(a);//2
r9 = c.Test(b);//1
";
            thisTest.RunScriptSource(code);
            //Assert.Fail("1467131- Sprint 24 - Rev 2910 method overload with replication , throws error WARNING: Multiple type+pattern match parameters found, non-deterministic dispatch\nunecpected result");
            thisTest.SetErrorMessage("1467307 - Sprint 25 - Rev 3784 : Method resolution failure on member function when actual parameter is the subtype of the formal parameter type");
            Object v1 = null;
            thisTest.Verify("r1", v1);
            thisTest.Verify("r2", 1);
            thisTest.Verify("r3", 3);
            thisTest.Verify("r4", 1);
            thisTest.Verify("r5", 1);
            thisTest.Verify("r6", 2);
            thisTest.Verify("r7", 1);
            thisTest.Verify("r8", 2);
            thisTest.Verify("r9", 1);
        }

        [Test]
        public void TV101_Indexing_IntoArray_InFunctionCall_1463234()
        {
            // Assert.Fail("1467131- Sprint 24 - Rev 2910 method overload with replication , throws error WARNING: Multiple type+pattern match parameters found, non-deterministic dispatch" );
            string code = @"
def foo()
{
return = {1,2};
}
t = foo()[0];";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t", 1);
        }

        [Test]
        public void TV101_Indexing_IntoArray_InFunctionCall_1463234_2()
        {
            // Assert.Fail("1467131- Sprint 24 - Rev 2910 method overload with replication , throws error WARNING: Multiple type+pattern match parameters found, non-deterministic dispatch" );
            string code = @"
t;
[Imperative]
{
def foo()
{
return = {1,2};
}
t = foo()[0];
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t", 1);
        }

        [Test]
        public void TV101_Indexing_Intosingle_InFunctionCall_1463234_2()
        {
            // Assert.Fail("1467131- Sprint 24 - Rev 2910 method overload with replication , throws error WARNING: Multiple type+pattern match parameters found, non-deterministic dispatch" );
            string code = @"
def foo()
{
return = {1};
}
t = foo()[0];";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t", 1);
        }

        [Test]
        public void TV101_Indexing_Intoemptyarray_InFunctionCall_1463234_3()
        {
            // Assert.Fail("1467131- Sprint 24 - Rev 2910 method overload with replication , throws error WARNING: Multiple type+pattern match parameters found, non-deterministic dispatch" );
            string code = @"
def foo()
{
return = {};
}
t = foo()[0];";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object t = null;
            thisTest.Verify("t", t);
        }

        [Test]
        public void TV101_Indexing_Intovariablenotarray_InFunctionCall_1463234_4()
        {
            // Assert.Fail("1467131- Sprint 24 - Rev 2910 method overload with replication , throws error WARNING: Multiple type+pattern match parameters found, non-deterministic dispatch" );
            string code = @"
def foo()
{
return = 1;
}
t = foo()[0];";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object t = null;
            thisTest.Verify("t", t);
        }

        [Test]
        public void TV101_Indexing_IntoNested_FunctionCall_1463234_5()
        {
            // Assert.Fail("1467131- Sprint 24 - Rev 2910 method overload with replication , throws error WARNING: Multiple type+pattern match parameters found, non-deterministic dispatch" );
            string code = @"
	def foo()
	{
		return = {foo2()[0],foo2()[1]};
	}
def foo2()
{
return = {1,2};
}
a=test.test()[0];
t = foo()[0];";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("t", 1);
        }

        [Test]
        public void TV101_Indexing_Into_classCall_1463234_6()
        {
            // Assert.Fail("1467131- Sprint 24 - Rev 2910 method overload with replication , throws error WARNING: Multiple type+pattern match parameters found, non-deterministic dispatch" );
            string code = @"
class test{
	constructor test()
	{
	}
	def foo()
	{
		return = {1,2};
	}
}
a=test.test();
t = a.foo()[0];";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("t", 1);
        }

        [Test]
        public void TV101_Indexing_Into_classCall_1463234_7()
        {
            // Assert.Fail("1467131- Sprint 24 - Rev 2910 method overload with replication , throws error WARNING: Multiple type+pattern match parameters found, non-deterministic dispatch" );
            string code = @"
class test{
	constructor test()
	{
	}
	def foo()
	{
		return = {1,2};
	}
}
t;
[Imperative]
{
a=test.test();
t = a.foo()[0];
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t", 1);
        }

        [Test]
        public void TV102_GlobalVariable_Function_1466768()
        {
            // Assert.Fail("1467131- Sprint 24 - Rev 2910 method overload with replication , throws error WARNING: Multiple type+pattern match parameters found, non-deterministic dispatch" );
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
	def foo ( p: Point)
	{
		return = Point.Point( (p.X), (p.Y), (p.Z) );
	}
}
def func1(p1 : Point, p2 : Point)
{
	p1 = p1.foo(p2);
	return = null;
}
p1 = Point.Point ( 0,0,0);
p2 = Point.Point ( 1, 1, 1 );
dummy = func1(p1, p2);
xx = p1.X; // expected 0, received 0
yy = p1.Y; // expected 0, received 0
zz = p1.Z; // expected 0, received 0
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("xx", 0);
            thisTest.Verify("yy", 0);
            thisTest.Verify("zz", 0);
        }

        [Test]
        public void TV102_GlobalVariable_Function_1466768_1()
        {
            // Assert.Fail("1467131- Sprint 24 - Rev 2910 method overload with replication , throws error WARNING: Multiple type+pattern match parameters found, non-deterministic dispatch" );
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
	def foo ( p: Point)
	{
		return = Point.Point( (p.X), (p.Y), (p.Z) );
	}
}
def func1()
{
	p1 = p1.foo(p2);
	return = null;
}
p1 = Point.Point ( 0,0,0);
p2 = Point.Point ( 1, 1, 1 );
dummy = func1();
xx = p1.X; // expected 0, received 0
yy = p1.Y; // expected 0, received 0
zz = p1.Z; // expected 0, received 0
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("xx", 1);
            thisTest.Verify("yy", 1);
            thisTest.Verify("zz", 1);
        }


        [Test]
        public void TV102_GlobalVariable_Function_1466768_2()
        {
            // Assert.Fail("1467131- Sprint 24 - Rev 2910 method overload with replication , throws error WARNING: Multiple type+pattern match parameters found, non-deterministic dispatch" );
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
def foo ( p: Point)
{
return = Point.Point( (p.X), (p.Y), (p.Z) );
}
}
def func1(pts : Point[])
{
pts[1] = pts[0];
return = null;
}
p1 = Point.Point ( 0,0,0);
p2 = Point.Point ( 1, 1, 1 );
p = { p1, p2 };
xx = p[1].X;
yy = p[1].Y;
zz = p[1].Z;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("xx", 1);
            thisTest.Verify("yy", 1);
            thisTest.Verify("zz", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV103_Defect_1467149()
        {
            string code = @"
class surf
{
	a:double;
	constructor surf(c:double)
	{
		a=1;
	}
}
convert={surf.surf(1..2), surf.surf(3)};
def prop(test:surf)
{
	return= test.a;
}
b=prop(convert);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1467183 - Sprint24: rev 3163 : replication on nested array is outputting extra brackets in some cases");
            thisTest.Verify("b", new Object[] { new Object[] { 1.0, 1.0 }, 1.0 });

        }

        [Test]
        [Category("SmokeTest")]
        public void TV103_Defect_1455090_Rank_Of_Arg()
        {
            String code =
@"
b;
[Associative]
{
def foo : int ( a : double[][] )
{
return = a[0][0] ; 
}
a = { { 0, 1}, {2, 3} };
b = foo ( a );
}
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV103_Defect_1455090_Rank_Of_Arg_2()
        {
            String code =
@"class A
{
    X : var[][][];
    constructor A ( b : double[]..[] )
    {
        X = b;
    }
    def foo : var[][][] (  )
    {
        return = X ; 
    }
}
a = { { { 0, 1} }, { {2, 3} } };
b = A.A ( a );
c = b.foo();";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", new Object[] { new Object[] { new Object[] { 0.0, 1.0 } }, new Object[] { new Object[] { 2.0, 3.0 } } });
        }

        [Test]
        [Category("SmokeTest")]
        public void TV103_Defect_1455090_Rank_Of_Arg_3()
        {
            String code =
@"class A
{
    X : var[][][];
    constructor A ( b : double[]..[] )
    {
        X = b;
    }
    def foo : var[][][] (  )
    {
        return = X ; 
    }
}
a = { { { 0, 1} },  {2, 3}  };
b = A.A ( a );
c = b.foo();";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            //Assert.Fail("1467183 - Sprint24: rev 3163 : replication on nested array is outputting extra brackets in some cases");
            thisTest.Verify("c", new Object[] { new Object[] { new Object[] { 0.0, 1.0 } }, new Object[] { new object[] { 2.0 }, new object[] { 3.0 } } });
        }

        [Test]
        [Category("SmokeTest")]
        public void TV103_Defect_1455090_Rank_Of_Arg_4()
        {
            String code =
@"  class A
{
    X : var[][];
    constructor A ( b : double[][] )
    {
        X = b;
    }
    def foo : var[][][] (  )
    {
        return = X ; 
    }
}
a = { { 0, 1} ,  2  };
b = A.A ( a );
c = b.foo();";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1467183 - Sprint24: rev 3163 : replication on nested array is outputting extra brackets in some cases");

            thisTest.Verify("c", new Object[] { new Object[] { new object[] { 0.0 }, new object[] { 1.0 } }, new object[] { new object[] { 2.0 } } });
        }

        [Test]
        [Category("SmokeTest")]
        public void TV103_Defect_1455090_Rank_Of_Arg_4a()
        {
            String code =
@"
def foo : var[] (  )
{
    return = 1 ; 
}
c = foo();";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1467183 - Sprint24: rev 3163 : replication on nested array is outputting extra brackets in some cases");
            thisTest.Verify("c", new Object[] { 1 });
        }

        [Test]
        [Category("SmokeTest")]
        public void TV103_Defect_1455090_Rank_Of_Arg_4b()
        {
            String code =
@"
def foo : int[] (  )
{
    return = 1 ; 
}
c = foo();";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1467183 - Sprint24: rev 3163 : replication on nested array is outputting extra brackets in some cases");
            thisTest.Verify("c", new Object[] { 1 });
        }


        [Test]
        [Category("SmokeTest")]
        public void TV103_Defect_1455090_Rank_Of_Arg_5()
        {
            String code =
                @"class A
                {
                    X : var[][];
                    constructor A ( b : double[][] )
                    {
                        X = b;
                    }
                    def foo : var[][][] (  )
                    {
                        return = X ; 
                    }
                }
                a = { 3, { 0, 1} ,  2  };
                b = A.A ( a );
                c = b.foo();";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1467183 - Sprint24: rev 3163 : replication on nested array is outputting extra brackets in some cases");

            thisTest.Verify("c", new Object[] { new object[] { new object[] { 3.0 } }, new Object[] { new object[] { 0.0 }, new object[] { 1.0 } }, new object[] { new object[] { 2.0 } } });
        }

        [Test]
        [Category("SmokeTest")]
        public void TV103_Defect_1455090_Rank_Of_Arg_6()
        {
            String code =
@"
def foo  ( x : int[][] )
{
        return = x ; 
}
a = { 3, { 0, 1} ,  2  };
b = foo ( a );";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1467183 - Sprint24: rev 3163 : replication on nested array is outputting extra brackets in some cases");

            thisTest.Verify("b", new Object[] { new object[] { 3 }, new Object[] { 0, 1 }, new object[] { 2 } });
        }

        [Test]
        [Category("SmokeTest")]
        public void TV104_Defect_1467112()
        {
            String code =
@"class A
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
class C extends B
{
    public def foo1 (a)
    {
        return = 3;
    }         
}
b = C.C();
b1 = b.foo1(1);
test = b1;";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("test", 3);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV105_Defect_1467244()
        {
            String code =
@"def foo(x:int = 2.3)
{
return = x;
}
d = foo();";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("d", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV105_Defect_1467244_2()
        {
            String code =
@"def foo(x:double = 2)
{
return = x;
}
d = foo();";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("d", 2.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV105_Defect_1467244_3()
        {
            String code =
@"def foo(x:int = 2)
{
return = x;
}
d = foo(1.5);";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("d", 2);
        }

        [Test]
        [Category("Failure")]
        [Category("SmokeTest")]
        public void TV106_Defect_1467132()
        {
            String code =
@"def foo : double (x :var[])
{
    
    return = Average(x);
}
a = {1,2,2,1};
b = {1,{}};
c = Average(a);
c1= foo(a);
c2 = foo(b);
c3 = Average({});
result = {foo(a),foo(b)};";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4171
            string errmsg = "MAGN-4171: Replication method resolution";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("result", new Object[] { 1.5, new Object[] { 1.0, 0.0 } });
        }

        [Test]
        [Category("SmokeTest")]
        public void TV107_Defect_1467273_Function_Resolution_Over_Argument_Rank()
        {
            String code =
@"def foo(x:var[]..[])
{
return = 2;
}
def foo(x:var[])
{
return = 1;
}
d = foo({1,2});";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("d", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV107_Defect_1467273_Function_Resolution_Over_Argument_Rank_2()
        {
            String code =
@"def foo(x:int[]..[])
{
return = 2;
}
def foo(x:int[])
{
return = 1;
}
d = foo({1,2});";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("d", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV107_Defect_1467273_Function_Resolution_Over_Argument_Rank_3()
        {
            String code =
@"def foo(x:double[]..[])
{
return = 2;
}
def foo(x:int[])
{
return = 1;
}
d = foo({1.5,2.5});";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("d", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV107_Defect_1467273_Function_Resolution_Over_Argument_Rank_4()
        {
            String code =
@"
class A
{
    def foo(x:int[]..[])
    {
        return = 2;
    }
    def foo(x:int[])
    {
        return = 1;
    }
}
a = A.A();
d = a.foo({1,2});";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("d", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV107_Defect_1467273_Function_Resolution_Over_Argument_Rank_4a()
        {
            String code =
@"
class A
{
    def foo(x:int[]..[])
    {
        return = 2;
    }
    def foo(x:int[])
    {
        return = 1;
    }
}
a = A.A();
d = a.foo({1.0,2.2});";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("d", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV104_Defect_1467379()
        {
            String code =
@"
def foo (f:function)
{   
return = true;
}      
a = foo(test);
b = 1;
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            string err = "1467379 Sprint 27 - Rev 4193 - after throwing warning / error in the attached code it should execute rest of the code ";
            thisTest.RunScriptSource(code, err);
            thisTest.Verify("b", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV105_Defect_1467409()
        {
            String code =
@"
def foo ( x1: int, y1 : int )
{
    return = x1;
}
class A {}
a=A.A();
r=a.foo(); // calling a non-exist function shouldn't get a warning at complie time
b = foo1();
d = foo(2, A.A() );
f = foo3();
r1;b1;d1;
[Imperative]
{
    r1=a.foo(); 
    b1 = foo1();
    d1 = foo(2, A.A() );
}
def foo3()
{
    r2=a.foo(); 
    b2 = foo1();
    d2 = foo(2, A.A() );
    return = { r2, b2, d2 };
}
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            string err = "DNL-1467409 Please disable static analysis warnings for function resolution"; ;
            thisTest.RunScriptSource(code, err);
            Object n1 = null;
            thisTest.Verify("r", n1);
            thisTest.Verify("b", n1);
            thisTest.Verify("d", n1);
            thisTest.Verify("r1", n1);
            thisTest.Verify("b1", n1);
            thisTest.Verify("d1", n1);
            thisTest.Verify("f", new Object[] { n1, n1, n1 });

        }

        [Test]
        [Category("SmokeTest")]
        public void TV106_1467455()
        {
            String code =
                @"
                def foo ()
                {
                    return 2;
                }
                a= foo()
                ";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            string err = ""; ;
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                thisTest.RunScriptSource(code, err);
            });


        }

        [Test]
        [Category("Method Resolution")]
        public void T28_Function_Arguments_Declared_Before_Function_Def()
        {
            string code = @"
result = [Associative]
{ 
	a = 0;
	b = 1;
	def foo : int (a : int, b: int)
	{
		return = a + b; 
	}
	
	result = foo( a, b); 
	return = result;
}
result2 = 
[Imperative]
{ 
	a = 3;
	b = 4;
	def foo : int (a : int, b: int)
	{
		return = a + b; 
	}
	
	result2 = foo( a, b); 
	return = result2;
}
result3 = 
[Associative]
{ 
	a = 5;
	b = 6;
	result3 = [Imperative]
	{
		def foo : int (a : int, b: int)
		{
			return = a + b; 
		}
		
		result3 = foo( a, b); 
		return = result3;
	}
	return = result3;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            // Assert.Fail("1467080 - sprint23 : rev 2681 : method overload issue over functions with same name and signature in multiple language blocks");
            thisTest.Verify("result", 1);
            thisTest.Verify("result2", 7);
            thisTest.Verify("result3", 11);
        }

        [Test]
        [Category("SmokeTest")]
        public void T63_Function_notDeclared()
        {
            String code = @"
            import(""DSCoreNodes.dll"");
            def foo : double(arg : double) = arg + 1;
            a = foo(""a""); ";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", null);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.RuntimeData.WarningID.kMethodResolutionFailure);
        }

        [Test]
        [Category("SmokeTest")]
        public void T64_Function_notDeclared_2()
        {
            String code = @"
x = foo(); // null
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("x", null);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kFunctionNotFound);
        }

        [Test]
        [Category("SmokeTest")]
        public void T64_Function_notDeclared_3()
        {
            String code = @"
import(""DSCoreNodes.dll"");
c = Math.Floor(3.0);
d = Floor(3);
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("d", null);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kFunctionNotFound);
        }

        [Test]
        [Category("SmokeTest")]
        public void T64_Function_notDeclared_imperative_4()
        {
            String code = @"
import(""DSCoreNodes.dll"");
c;d;
[Imperative]
{
    c = Math.Floor(3.0);
    d = Floor(3);
}
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("d", null);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kFunctionNotFound);
        }

        [Test]
        [Category("SmokeTest")]
        public void T65_1467115_warning_on_function_resolution_1()
        {
            String code = @"
x = { 1,2};
t = 0;
y = x[null];
p = y + 1;
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("p", null);
            thisTest.VerifyBuildWarningCount(0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T65_1467115_warning_on_function_resolution_2()
        {
            String code = @"
a ;
b ;
[Associative]
{
    a = 10;
    b = a * 2;
    a = { 5, 10, 15 };
    
    a = 5..15..2;
}
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", new Object[] { 10, 14, 18, 22, 26, 30 });
            thisTest.VerifyBuildWarningCount(0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T65_1467115_warning_on_function_resolution_3()
        {
            String code = @"
def foo (a : int)
{
    return = 1;
}
def foo(b : double)
{
    return = 2;
}
def foo(b : double[])
{
    return = 3;
}
x = { 1.0, 5, 2.4};
p = foo(x);
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("p", 3);
            thisTest.VerifyBuildWarningCount(0);
        }

        [Test]
        public void TestMultiOverLoadWithDefaultArg()
        {
            string code = @"
class Bar
{
}

class Foo
{
    def foo(x = 0, y = 0, z = 0)
    {
        return = 41;
    }
    
    def foo(x : Bar)
    {
        return = 42;
    }
}

b = Bar();
f = Foo();
r = f.foo(b); // shoudn't be resolved to foo(x = 0, y = 0, z = 0)
";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, "");
            thisTest.Verify("r", 42);
        }

        [Test]
        [Category("Failure")]
        public void TestFunctionOverloadFromNestedLanguageBlock01()
        {
            string code = @"
def f()
{
    return = 1;
}

[Imperative]
{
    def f()
	{
		return = 2;
	}
}

a = f();
";
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3987
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code);
            thisTest.Verify("a", 1);
        }
    }
}
