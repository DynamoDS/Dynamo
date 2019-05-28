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
        ProtoScript.Runners.DebugRunner fsr;

        public override void Setup()
        {
            base.Setup();
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
def foo : int( a:int )
{
   return = a * 10;
}

[Associative]
{
    a = foo( 2 );
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("a", 20);

        }

        [Test]
        [Category("SmokeTest")]
        public void T02_Function_In_Imp_Scope()
        {
            string code = @"
    def foo : double( a:double, b : int )
    {
	   return = a * b;
	}
a=[Imperative]
{
	
    return foo( 2.5, 2 );
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a",5.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T03_Function_In_Nested_Scope()
        {
            string code = @"
    def foo : double( a:double, b : int )
    {
	   return = a * b;
	}
i=[Imperative]
{
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
    return [a,b];
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("i", new[] { 2.5, 2.5 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T04_Function_In_Nested_Scope()
        {
            string code = @"
a=x[0];b=x[1];
def foo : int( a:int, b : int )
{
   return = a * b;
}
x=[Associative]
{
	i = [Imperative]
	{
		return foo( 2, 1 );
	}
	j = [Imperative]
	{
		return foo( 2, 1 );
	}
    return [i, j];
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 2);
            thisTest.Verify("b", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T07_Function_Assoc_Inside_Imp()
        {
            string src = @"a=i[0];b=i[1];
	def foo : int( a:int, b : int )
	{
		return = a * b;
	}
i = [Imperative]
{
	a = 3.5;
	b = 3.5;
	[Associative]
	{
		a = foo( 2, 1 );
	}
	b = [Associative]
	{
		c = foo( 2, 1 );
		return = c;
	}
    return [a,b];
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);

            thisTest.Verify("a", 2);
            thisTest.Verify("b", 2);
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
def add_2 : double( a:double )
{
    return = add_1( a ) + 1;
}
a;b;
[Associative]
{
	a = 1.5;
	b = add_2 (a );
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a",1.5);
            thisTest.Verify("b",3.5);
        }

        [Test]
        [Category("SmokeTest")]
        public void T13_Function_From_Inside_Function()
        {
            string code = @"
a=i[0];b=i[1];
def add_1 : double( a:double )
{
	return = a + 1;
}
def add_2 : double( a:double )
{
    return = add_1( a ) + 1;
}
i = [Imperative]
{
	a = 1.5;
	b = add_2 (a );
	return [a,b];
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a",1.5);
            thisTest.Verify("b",3.5);
        }

        [Test]
        [Category("SmokeTest")]
        public void T18_Function_Recursive_associative()
        {
            string code = @"
a=i[0];b=i[1];
def factorial : int( n : int )
{
    return = [Imperative]
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
i = [Imperative]
{
	a = 3;
	b = factorial (a );
    return [a,b];
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 3);
            thisTest.Verify("b", 6);
        }

        [Test]
        [Category("SmokeTest")]
        public void T17_Function_From_Parallel_Blocks()
        {
            string code = @"
def foo : int( n : int )
{
    return = n * n;	
}
b = [Associative]
{
	a = 3;
	return foo (a);
};";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", 9);
        }

        [Test]
        [Category("SmokeTest")]
        public void T19_Function_From_Imperative_While_And_For_Loops()
        {
            string code = @"
x=i[0];y=i[1];
def foo : int( n : int )
{
	return = n * n;	
}
	
i = [Imperative]
{

	a = [ 0, 1, 2, 3, 4, 5 ];
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
	return [x, y];
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 55);
            thisTest.Verify("y", 30);
        }


        [Test]
        [Category("SmokeTest")]
        public void T20_Function_From_Imperative_If_Block()
        {
            string code = @"
x = a[0];
y = a[1];
z = a[2];
def foo : int( n : int )
{
    return = n * n;	
}
a = [Associative]
{
	return [Imperative]
	{
	
		a = [ 0, 1, 2, 3, 4, 5 ];
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
        return [x, y, z];
	}
	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 3025);
            thisTest.Verify("y", 900);
            thisTest.Verify("z", 25);
        }

        [Test]
        [Category("SmokeTest")]
        public void T21_Function_From_Nested_Imperative_Loops()
        {
            string code = @"
x=i[0];y=i[1];
	def foo : int( n : int )
	{
		return = n ;	
	}
i = [Imperative]
{
	
	a = [ 0, 1, 2, 3, 4, 5 ];
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
	return [x,y];
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 180, 0);
            thisTest.Verify("y", 50, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T22_Function_Call_As_Instance_Arguments()
        {
            string code = @"
import(""FFITarget.dll"");
def foo : int( n : int )
{
    return = n ;	
}

def foo2 : double( n : double )
{
    return = n ;	
}

A1 = [Associative]
{
	return = ClassFunctionality.ClassFunctionality(foo(foo(1))).IntVal;	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("A1", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T23_Function_Call_As_Function_Call_Arguments()
        {
            string code = @"
c1;
def foo : double ( a : double , b :double )
{
    return = a + b ;	
}

def foo2 : double ( a : double , b :double )
{
    return = foo ( a , b ) + foo ( a, b );	
}

[Associative]
{
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
def foo : double ( a : double , b :double )
{
    return = a + b ;	
}

[Associative]
{
	a1 = 1..foo(2,3)..foo(1,1);
	a2 = 1..foo(2,3)..#foo(1,1);
	a3 = 1..foo(2,3)..~foo(1,1);
	a4 = [ foo(1,0), foo(1,1), 3 ];
	
}	

def foo_2 : double ( a : double , b :double )
{
    return = a + b ;	
}
[Imperative]
{
	a1 = 1..foo_2(2,3)..foo_2(1,1);
	a2 = 1..foo_2(2,3)..#foo_2(1,1);
	a3 = 1..foo_2(2,3)..~foo_2(1,1);
	a4 = [ foo_2(1,0), foo_2(1,1), 3 ];
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
a1=i[0];a2=i[1];a3=i[2];a4=i[3];

def foo_2 : double( a : double , b :double )
{
    return = a + b ;	
}
i = [Imperative]
{
	a1 = 1 + foo_2(2,3);
	a2 = 2.0 / foo_2(2,3);
	a3 = 1 && foo_2(2,2);
	a4 = 0;
	
	if( foo_2(1,2) > foo_2(1,0) )
	{
	    a4 = 1;
	}
	return [a1, a2, a3, a4];
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
x=i[0];a4=i[1];
def foo_2 : double ( a : double , b :double )
{
    return = a + b ;	
}
i = [Imperative]
{
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
	
	c = [ 0, 1, 2 ];
	for (i in c )
	{
		if( foo_2(1,2) > foo_2(1,0) )
		{
		    x = x + 1;
		}
	}
	return [x,a4];
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
def Level1 : int (a : int) 
{  
    return = Level2(a+1); 
}  
def Level2 : int (a : int) 
{  
    return = a + 1; 
} 

[Associative]
{ 
	input = 3; 
	result = Level1(input); 
}
def foo : int (a : int)
{
    return = a + 1; 
}
[Imperative]
{ 
	a = foo(1); 
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

        }

        [Test]
        [Category("SmokeTest")]
        public void T29_Function_With_Different_Arguments()
        {
            string code = @"
	 def foo : double ( a : int, b : double, c : bool, d : int[], e : double[][], f:int[]..[], g : bool[] )
	 {
        return [Imperative]
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
             return x;
        }   
	 }
y = [Imperative]
{ 
	 
	 a = 1;
	 b = 1;
	 c = true;
	 d = [ 1, 2 ];
	 e = [ [ 1, 1 ], [2, 2 ] ] ;
	 f = [ [0, 1], 2 ];
	 g = [ true, false ];
	 
	 return foo ( a, b, c, d, e, f, g );
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
	 def foo1 : double ( a : double )
	 {
	    return = true;
	 }
b1 = [Imperative]
{ 
	 dummyArg = 1.5;
	
	return foo1 ( dummyArg );
	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b1", null);
            //});
        }

        [Test]
        [Category("SmokeTest")]
        public void T31_Function_With_Mismatching_Return_Type()
        {
            string code = @"
	 def foo2 : double ( a : double )
	 {
	    return = 5;
	 }
b2 = [Imperative]
{ 
	dummyArg = 1.5;
	
	return foo2 ( dummyArg );
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
def foo3 : int ( a : double )
{
    return = 5.5;
}
	 
b2 = [Imperative]
{ 
	dummyArg = 1.5;
	
	return foo3 ( dummyArg );
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b2", 6, 0);
            //});
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T33_Function_With_Mismatching_Return_Type()
        {
            string code = @"
import(""FFITarget.dll"");
def foo3 : int ( a : double )
{
    temp = ClassFunctionality.ClassFunctionality(1);
    return = temp;
}
b2 = [Imperative]
{ 
	dummyArg = 1.5;
	
	return = foo3 ( dummyArg );	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b2", null);
            //});
        }

        [Test]
        [Category("SmokeTest")]
        public void T34_Function_With_Mismatching_Return_Type()
        {
            //Assert.Throws(typeof(ProtoCore.Exceptions.RuntimeException), () =>
            //{
            string code = @"
b1 = [Associative]
{ 
	 def foo1 : double ( a : double )
	 {
	    return = true;
	 }
	 
	 dummyArg = 1.5;
	
	return foo1 ( dummyArg );
	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b1", null);
            //});
        }

        [Test]
        [Category("SmokeTest")]
        public void T35_Function_With_Mismatching_Return_Type()
        {
            string code = @"
	 def foo2 : double ( a : double )
	 {
	    return = 5;
	 }
	 
b2 = [Associative]
{ 
	dummyArg = 1.5;
	
	return foo2 ( dummyArg );
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
	 def foo3 : int ( a : double )
	 {
	    return = 5.5;
	 }
	 
b2 = [Associative]
{ 
	dummyArg = 1.5;
	
	return foo3 ( dummyArg );
	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b2", 6, 0);
            //});
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T37_Function_With_Mismatching_Return_Type()
        {
            //Assert.Fail("DNL-1467208 Auto-upcasting of int -> int[] is not happening on function return");
            //Assert.Throws(typeof(ProtoCore.Exceptions.RuntimeException), () =>
            //{
            //Assert.Fail("1467156 - Sprint 25 - Rev 3026 type checking of return types at run time ");
            string code = @"
import(""FFITarget.dll"");
	 def foo3 : int ( a : double )
	 {
	    temp = ClassFunctionality.ClassFunctionality(1);
		return = temp;
	 }
	
b2 = [Associative]
{ 
 
	dummyArg = 1.5;
	
	return = foo3 ( dummyArg );	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b2", null);
            //});
        }

        [Test]
        public void T38_Function_With_Mismatching_Return_Type()
        {
            //Assert.Throws(typeof(ProtoCore.Exceptions.RuntimeException), () =>
            //{
            //Assert.Fail("1467156 - Sprint 25 - Rev 3026 type checking of return types at run time ");
            string code = @"
	 def foo3 : int[] ( a : double )
	 {
	    return = a;
	 }
	 
b2 = [Associative]
{ 
	dummyArg = 1.5;
	
	return foo3 ( dummyArg );	
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
	 def foo3 : int ( a : double )
	 {
	    return = [1, 2];
	 }
	 
b2 = [Associative]
{ 
	dummyArg = 1.5;
	
	return foo3 ( dummyArg );	
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
	 def foo3 : int[][] ( a : double )
	 {
	    return = [ [2.5], [3.5]];
	 }
	 
[Associative]
{ 
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
	 def foo3 : int[][] ( a : double )
	 {
	    return = [ [2.5], 3];
	 }
[Associative]
{ 
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
	 def foo3 : bool[]..[] ( a : double )
	 {
	    return = [ [2], 3];
	 }
	 
b2 = [Associative]
{ 
	dummyArg = 1.5;
	
	return foo3 ( dummyArg );	
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
	 def foo3 : int[]..[] ( a : double )
	 {
	    return = [ [ 0, 2 ], [ 1 ] ];
	 }
	 
b2 = [Associative]
{ 
	dummyArg = 1.5;
	
	return foo3 ( dummyArg )[0][0];	
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
def foo : double ( a : double )
{
    return = 1.5;
}
b2 = [Imperative]
{ 
	 return foo ( null );	
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
	 def foo : double ( a : double )
	 {
	    return = 1.5;
     }
	
b2 = [Associative]
{ 
	 return foo ( 1 );	
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
	 def foo : double ( a : int )
	 {
	    return = 1.5;
     }
c = [Imperative]
{ 
	 b2 = foo ( 1.5);
     return 3;	 
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
	 def foo : double ( a : double )
	 {
	    return = 1.5;
     }
	
c = [Associative]
{ 
	 b2 = foo ( true);	
	return 3;	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", 3, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T49_Function_With_Matching_Return_Type()
        {
            string code = @"
import(""FFITarget.dll"");
	 def foo : ClassFunctionality ( x : ClassFunctionality )
	 {
	    return = x;
     }
c = [Associative]
{ 

	 aa = ClassFunctionality.ClassFunctionality(1);
	 b2 = foo ( aa).IntVal;
	 return = 3;	
	 
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
	 def foo : double ( a : int[] )
	 {
	    return = 1.5;
     }
c = [Associative]
{ 
	 aa = [ ];
	 b2 = foo ( aa );	
	return 3;	
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
	 def foo : double ( a : double[] )
	 {
	    return = 1.5;
     }
c = [Associative]
{ 
	 aa = [1, 2 ];
	 b2 = foo ( aa );	
	 return 3;	
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
	 def foo : double ( a : double[] )
	 {
	    return = 1.5;
     }
c = [Associative]
{ 
	 aa = 1.5;
	 b2 = foo ( aa );	
	 return 3;	
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
	 def foo : double ( a : double )
	 {
	    a = 4.5;
		return = a;
     }
[Associative]
{ 
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
aa=i[0];b2=i[1];c=i[2];
	 def foo : double ( a : double )
	 {
	    a = 4.5;
		return = a;
     }  
i = [Imperative]
{ 
	 aa = 1.5;
	 b2 = foo ( aa );	
	 c = 3;	
    return [aa, b2, c];
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
aa=i[0];b2=i[1];c=i[2];
	 def foo : int ( a : double )
	 {
	    a = 5;
		return = a;
     }
i=[Imperative]
{ 
	 aa = 5.0;
	 b2 = foo ( aa );	
	 c = 3;	
return [aa, b2, c];
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
	 def foo : int[] ( a : int[] )
	 {
	    a[0] = 0;
		return = a;
     }
[Associative]
{ 
	 aa = [ 1, 2 ];
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
	 def foo : int ( a : int )
	 {
	    a = 3;
		b = a + 1;
		return = b;
     }
	 
[Associative]
{ 
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
	 
	 c2 = [Imperative]
	 { 
		return foo();	
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
            thisTest.Verify("c", null);
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
            thisTest.Verify("c", null);

        }

        [Test]
        [Category("SmokeTest")]
        public void T61_Function_With_Void_Return_Stmt()
        {
            //Assert.Fail("1463474 - Sprint 20 : rev 2112 : negative case: when user tries to create a void function DS throws ArgumentOutOfRangeException ");

            string code = @"
		def foo : void  ( )
		{
			a = 2;		
		}
[Associative]
{
	 a = 1;
	 [Imperative]
	 {
		foo();
        b = a;	    
	 }
}
	 
	 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

        }

        [Test]
        [Category("SmokeTest")]
        public void T63_Function_Modifying_Globals_Values()
        {
            //Assert.Fail("1465794 - Sprint 22 : rev 2359 : Global variable support is not there in purely Imperative scope"); 

            string code = @"
def foo : int  (a)
{
    c = a;
    a = 2;	
    return = c + 1;			
}   
x = [Imperative]
{
	a = 1;
	b = foo(a);
    return = [ a, b ];    
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 1, 3 };
            thisTest.Verify("x", v1);

        }

        [Test]
        [Category("SmokeTest")]
        public void T65_Function_With_No_Return_Type()
        {
            string code = @"
a=i[0];b=i[1];
	def foo (  )
	{
		return = true;			
	}
	
i = [Imperative]
{

	a = foo();
	b = 3;
     return [a, b];       
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
a=i[0];b=i[1];c=i[2];
	def foo : int ( a : int )
	{
		c = d + a;
        return = c;		
	}	
i=[Imperative]
{
	a = 1;
	b = foo(a);
	c = b + 2;
    return [a, b, c];      
}
	 
	 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1, 0);
            thisTest.Verify("b", null);
            thisTest.Verify("c", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void T67_Function_Returning_Collection()
        {
            string code = @"
a=i[0];b=i[1];
	def foo : int[] ( a : int )
	{
		c = [ a + 1, a + 2.5 ];
        return = c;		
	}
i=[Imperative]
{
	a = 1;
	b = foo(a);
    return [a,b];       
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
a=i[0];b=i[1];
	def foo : int[] ( a : int )
	{
		c = [ a + 1, a + 2.5 ];
        return = null;		
	}
i=[Imperative]
{
	a = 1;
	b = foo(a);
    return [a,b];      
}
	 
	 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", null);
            thisTest.Verify("a", 1, 0);
        }

        [Test] //Fuqiang: this is changed due to the implementation of function pointer
        [Category("SmokeTest")]
        public void T69_Function_Name_Checking()
        {
            string code = @"
	def foo : int ( a : int )
	{
		foo = 3;
		return  = a + foo;
	}
	
	a = 1;
	foo = 2;
	b = foo(a);           
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1, 0);
            thisTest.Verify("foo", 2, 0);
            thisTest.Verify("b", null, 0);
        }

        [Test] //Fuqiang: this is changed due to the implementation of function pointer
        [Category("SmokeTest")]
        public void T70_Function_Name_Checking()
        {
            string code = @"
		def foo : int ( a : int )
		{
			foo = 3;
			return  = a + foo;
		}
[Associative]
{
	[Imperative]
	{
		a = 1;
		b = foo(a);           
	}
}
	 
	 
";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
        }

        [Test] //Fuqiang: this is changed due to the implementation of function pointer
        [Category("SmokeTest")]
        public void T71_Function_Name_Checking()
        {
                string code = @"
[Associative]
{
    foo = 1;
}
	def foo : int ( a : int )
	{
		foo = 3;
		return  = a + foo;
	}
[Imperative]
{
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
x=i[0];y=i[1];
    def foo : int ( a : int )
	{
		return  = a + 1;
	}
i=[Imperative]
{
	x = [ 1, 2, 3 ];
	y = foo(x);
	return [x, y];
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
x=a[0];
y=a[1];
def foo : int ( a : int )
{
    return  = a + 1;
}

a=[Associative]
{
	i = [ 1, 2, 3 ];
	j = foo(i);
	return [i, j];
};
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
def foo : int ( a : int, b : int )
{
    return  = a + b;
}

[Associative]
{
	x1 = [ 1, 2, 3 ];
	x2 = [ 1, 2, 3 ];
	
	y = foo ( x1, x2 );
}
	 
	 
";
            thisTest.RunScriptSource(code);
            object[] expectedResult = { 2, 4, 6 };
            thisTest.Verify("y", expectedResult);
        }

        [Test]
        [Category("SmokeTest")]
        public void T76_Function_With_Replication_In_One_Arg()
        {
            string code = @"
y;
def foo : int ( a : int, b : int )
{
    return  = a + b;
}

[Associative]
{
	x1 = [ 1, 2, 3 ];
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
def foo : int ( a : int, b : int )
{
    return  = a + b;
}

[Associative]
{
	x1 = [ 1, 2 ];
	x2 = [ 1, 2 ];
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
def foo : int ( a : int, b : int )
{
    a = a + b;
    b = 2;
    return  = a + b;
}

[Associative]
{
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
c=i[0];d=i[1];
def foo : int ( a : int, b : int )
{
    return = [Imperative] {
    a = a + b;
    b = 2 * a;
    return  = a + b;
    }
}
i=[Imperative]
{
	a = 1;
	b = 2;
	c = foo (a, b );
	d = a + b;
	return [c,d];
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

[Associative]
{
	a = 2;
	b = 1;
	c = foo (a, b );
	d = a + b;
}
	 
	 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.InvalidStaticCyclicDependency);
        }

        [Test]
        [Category("SmokeTest")]
        public void T81_Function_Calling_Imp_From_Assoc()
        {
            string code = @"
b;
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
	
[Associative]
{
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
    def foo : int ( a : int )
	{
        return = [Imperative]
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
	}
b = [Imperative]
{
	a = 2;
	return foo (a );	
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
            thisTest.Verify("b", null);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T84_Function_With_User_Defined_Class()
        {
            string code = @"
import(""FFITarget.dll"");
def foo : ClassFunctionality ( a : ClassFunctionality )
{
    a1 = a.IntVal;
	b1 = ClassFunctionality.ClassFunctionality(a1+1);
	return = b1;
}
x = ClassFunctionality.ClassFunctionality(1);
y = foo (x);
z = y.IntVal;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("z", 2, 0);
        }

        [Test]
        [Category("Replication")]
        public void T85_Function_With_No_Type()
        {
            // Assert.Fail("1467080 - sprint23 : rev 2681 : method overload issue over functions with same name and signature in multiple language blocks");

            string code = @"
a11=i[0];a21=i[1];a31=i[2];
a12;a22;a32;
	def foo( a )
	{
		a = a + 1;
		return = a;
	}
i=[Imperative]
{
	c = [ 1,2,3 ];
	d = foo ( c ) ;
		
	a11 = d[0];
	a21 = d[1];
	a31 = d[2];
    return [a11, a21, a31];
}
[Associative]
{
	def foo( a )
	{
		a = a + 1;
		return = a;
	}
	c = [ 1,2,3 ];
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
a1=i[0];a2=i[1];a3=i[2];
def foo: int( a : int )
{
    return = [Imperative]
    {
		for( i in a )		
		{					
		}		
		return = a;	
    }
}	
i=[Imperative]
{	
	d = [ 1,2,3 ];	
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
    return [a1, a2, a3];
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
arr = [1,2,3,4];
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
arr = [  [2.5,3], [1.5,2] ];
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
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T90_Function_PassingNullToUserDefinedType()
        {
            string code = @"
import(""FFITarget.dll"");
def GetVal : int(p:ClassFunctionality)
{
	return = p.IntVal;
}
p2 = null;
testNull = GetPointLength(p2);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification
            object testNull = null;
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
d=i[0];e=i[1];f=i[2];
i=[Imperative]
{
	d = foo();
	e = foo(1, 3.0);
	f = foo();
    return [d,e,f];
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
        [Category("SmokeTest")]
        public void TV00_Function_With_If_Statements()
        {
            string src = @"
	def foo:int( a:int, b:int, c:int )
	{
        return = [Imperative]
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
	}	
a = [Imperative]
{
	return foo( -9,3,-7 );
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("a", -9);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV01_Function_With_While_Statements()
        {
            string src = @"
	def foo:int( a:int, b:int)
	{	
        return = [Imperative]
        {
		    c = 1;
		
		    while( b > 0 )
		    {
			    c = c * a;
			    b = b - 1;
		    }
	
		    return = c;
        }
	}
d = [Imperative]
{
	return foo( 2,3 );
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("d", 8);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV02_Function_With_For_Statements()
        {
            string src = @"
	def foo:int( a:int )
	{	
        return = [Imperative]
        {
		    c = [ 1,2,3,4,5 ];
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
	}
d = [Imperative]
{
	return foo( 6 );
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("d", 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV03_Function_With_Recursion()
        {
            string src = @"x;
	def factorial: int( a:int )
	{	
        return = [Imperative]
        {
		    if(!a)
			    return = 1;
		
		    if( a < 0 )
			    return = 0;
			
		    else 
			    return = a * factorial( a - 1 );
        }       
	}
x = [Imperative]
{
	return factorial(5);
}
		";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("x", 120);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV04_Function_With_RangeExpression()
        {
            string src = @"d;
	def foo:double( a:int )
	{
		b = 2..4..#3;
		
		sum = b[0] + b[1] + b[2];
		
		return = sum + 0.5;
	}   
d = [Imperative]
{
	return foo( 1 );
}
		";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("d",9.5);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV05_Function_With_RangeExpression_2()
        {
            string src = @"a;d;
	def foo: int( a:int[] )
	{
        return = [Imperative]
        {
		    sum = 0;
		
		    for( i in a )
		    {
			    sum = sum + i ;
		    }
		    return = sum;
        }
	}
d = [Imperative]
{
	a = 2..4..#3;
	
	return foo( a );
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("d", 9);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV06_Function_With_Logical_Operators()
        {
            string src = @"a;
	def foo:int( a:int , b:int, c:int )
	{
        return = [Imperative]
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
	}
a = [Imperative]
{
	return foo( 2,3,4 );
}
			
			";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("a", 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV07_Function_With_Math_Operators()
        {
            string code = @"a;
	def math: double(a:double, b:double, c:int)
	{
    return = [Imperative]{
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
	}
a = [Imperative]
{
	a = 18;
	b = 2;
	c = 1;
	
	return math(a,b,2+1);
}";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 36.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV08_Function_With_Outer_Function_Calls()
        {
            string src = @"x=i[0];y=i[1];
	def is_negative:int(a :int)
	{
        return = [Imperative]{
		    if( a < 0 )
			    return = 1;
			
			    return = 0;
        }
	}
	def make_negative:int(a :int)
	{
		return = a * -1;
	}
	def absolute:int(a :int)
	{
        return = [Imperative]{
		    if(is_negative(a))
			    a = make_negative(a);
		
		    return = a;
        }
	}
i=[Imperative]
{
	x = -7;
	x = absolute(x);
	y = absolute(11);
    return [x,y];
}	
		";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("x", 7);
            thisTest.Verify("y", 11);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV09_Function_With_Argument_Update_Imperative()
        {
            string src = @"e=i[0];f=i[1];
	def update:int( a:int, b:int )
	{
		a = a + 1;
		b = b + 1;
		return = a + b;
	}
	
i=[Imperative]
{
	c = 5;
	d = 5;
	e = update(c,d);
	e = c;
	f = d;
    return [e, f];
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("e", 5);
            thisTest.Verify("f", 5);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void TV10_Function_With_Class_Instances()
        {
            string src = @"
import(""FFITarget.dll"");
def foo:int( a : int )
{
	A1 = ClassFunctionality.ClassFunctionality( a );
	return = A1.IntVal + a;
}
b = [Imperative]
{
	return = foo(1);
}
	
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("b", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV11_Function_Update_Local_Variables()
        {
            string src = @"r;
	def foo:int ( a : int )
	{
		b = a + 2.5;
		c = true;
		c = a;
		d = c + b;
		return = d;
	}
[Imperative]
{
	r = foo(1);
	r = b;
}
	";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("r", null);
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
            thisTest.Verify("e", 5);
            thisTest.Verify("f", 5);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV13_Empty_Functions_Imperative()
        {
            string src = @"b;
	def foo:int ( a : int )
	{
	}
[Imperative]
{
	b = foo( 1 );
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("b", null);
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
            thisTest.Verify("b", null);
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
                thisTest.Verify("b", null);
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
            thisTest.Verify("b", null);
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
            thisTest.Verify("e", null);
            thisTest.Verify("f", null);
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
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void TV22_Function_With_Class_Object_As_Argument()
        {
            string src = @"
import(""FFITarget.dll"");
def foo:int ( A_Inst : ClassFunctionality )
{	
	return = A_Inst.IntVal + 1;
}
b = [Associative]
{
	return = foo( ClassFunctionality.ClassFunctionality( 1 ) );
}
	";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("b", 2);
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
            thisTest.Verify("c", null);
            thisTest.Verify("d", null);
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
            thisTest.Verify("b", null);
            thisTest.Verify("c", null);
            //});
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
            thisTest.Verify("a", null);
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
            thisTest.Verify("a", null);
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
a=
[Imperative]
{
	return foo( 1, 2, 3 );
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
d=
[Imperative]
{
	return foo(-2.4);
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
c1=i[0];c2=i[1];c3=i[2];c4=i[3];
i = [Imperative]
{
	c1 = foo (  );
	c2 = foo ( 1 );
	c3 = foo ( 1, 2 );
	c4 = foo ( 1, 2, 3 );
    return [c1, c2, c3, c4];
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
	return = c == true ? a  : b;
}
def foo  ( a : double = 5, b : double = 5.5, c : bool = true )
{
	return = c == true ? a  : b;
}
c1=i[0];c2=i[1];c3=i[2];c4=i[3];
i=[Imperative]
{
	c1 = foo (  );
	c2 = foo ( 1 );
	c3 = foo ( 1.5, 2 );
	c4 = foo ( 1, 2, 0 );
    return [c1, c2, c3, c4];
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
	return = c == true ? a  : b;
}
def foo2  ( a , b = 5, c = true)
{
	return = c == true ? a  : b;
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
	return = [ c1, c2, c3, c4 ];
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
	return = c == true ? a  : b;
}
def foo  ( a : double = 6, b : double = 5.5, c : bool = true )
{
	return = c == true ? a  : b;
}
c1=i[0];c2=i[1];c3=i[2];c4=i[3];
i=[Imperative]
{
	c1 = foo (  );
	c2 = foo ( 1 );
	c3 = foo ( 1.5, 2 );
	c4 = foo ( 1, 2, 0 );
    return = [ c1, c2, c3, c4 ];
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
c4=i[0];c5=i[1];c6=i[2];
i = [Imperative]
{
	c4 = foo ( 1, 2, 0 );
	c5 = foo ( 1.5, 2.5, 0 );
	c6 = foo ( 1.5, 2.5, true );
    return [c4, c5, c6];
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
b=i[0];c=i[1];d=i[2];
	def foo:int ( a : bool )
	{
        return = [Imperative]{
		    if(a)
			    return = 1;
		    else
			    return = 0;
        }
	}
i=[Imperative]
{
	b = foo( 1 );
	c = foo( 1.5 );
	d = 0;
	if(1.5 == true ) 
	{
	    d = 3;
	}
    return [b,c,d];
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
	c=
[Imperative]
{
	return foo( 3 );
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
x=i[0];
y=i[1];
def factorial: int( a:int )
{	
    return = [Imperative]
    {
		if(!a)
			return = 1;
		
		if( a < 0 )
			return = 0;
			
		else 
			return = a * factorial( a - 1 );
    }
}
i=[Imperative]
{
	x = factorial(5);
	y = factorial(7);
    return [x,y];
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
[Associative]
{
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
 def Level1 : int (a : int)
 {
  return = Level2(a+1);
 }
 
 def Level2 : int (a : int)
 {
  return = a + 1;
 }
[Associative]
{

 input = 3;
 result = Level1(input); 
}
 def foo : int (a : int)
 {
     return = a + 1;
 }
a = [Imperative]
{
 b = 1;
 return foo(b);
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
a2=i[0];b2=i[1];c2=i[2];
	 def foo1 : int[] ( a : int[] )
	 {
	    a[0] = 0;
        return = a;
    }
[Associative]
{ 
	 aa = [ 1, 2 ];
	 bb = foo1 ( aa );	
	 a1 = aa[0];
	 b1 = bb[0];
	 cc = [Imperative]
	 {
	     return = foo1(aa);
	 };	
	 c1 = cc[0];
}
def foo  ( a : int[] )
{
    a[0] = 0;
    return = a;
}
i = [Imperative]
{ 
	 aa = [ 1, 2 ];
	 bb = foo ( aa );	
	 a2 = aa[0];
	 b2 = bb[0];
	 c2 = 3;	
    return [a2, b2, c2];
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
def foo : int ( x : int )
{
    return = a + x;
}
[Associative]
{
	a = 1;
	[Imperative]
	{
	    b = foo(1) ;	
	}
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1);
            thisTest.Verify("b", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV45_Defect_1455278()
        {
            string code = @"
b1;
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
	
[Associative]
{
	a = 2;
	b1 = foo (a );	
}
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
b2 = [Imperative]
{
	a = 2;
	return foo2 (a );	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b1", 3);
            thisTest.Verify("b2", 3);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TV46_Defect_1455278()
        {
            string code = @"
import(""FFITarget.dll"");
def foo : int ( a1 : int, a: int)
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
	
a = 1;
b1 = foo(2, a);
b2 = foo(0, a);
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
a=i[0];b=i[1];c=i[2];d=i[3];
i=[Imperative]
{
	a = foo ( 1, 2 );
	b = foo ( 2.5, 2.5 );
	c = foo ( 1, 2.3 );
	d = foo ( 2.3, 2 );
    return [a, b, c, d];
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
x = [Imperative]
{
	return recursion(a); 
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
	def collection:int[](a : int[] )
	{
        return = [Imperative]
        {
		    j = 0;
		    for ( i in a )
		    {
			    a[j] = a[j] + 1;
			    j = j + 1;
		    }
		
		    return = a;
        }
	}
[Imperative]
{
	[Associative]
	{
		c = [ 1,2,3 ];
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
	def collection:double[](a : double[] )
	{
        return = [Imperative]
        {
		    j = 0;
		    for ( i in a )
		    {
			    a[j] = a[j] + 1;
			    j = j + 1;
		    }
		
		    return = a;
        }
	}
c = [Imperative]
{
	c = [ 1.0,2.0,3.0 ];
	c = collection( c );
	return c;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedResult = { 2.0, 3.0, 4.0 };
            thisTest.Verify("c", expectedResult, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void TV51_Defect_1456108_3()
        {

            string code = @"
import(""FFITarget.dll"");
def add_1:double[](a: double[] )
{
	j = 0;
	return = [Imperative]
	{
		for ( i in a )
		{
			a[j] = a[j] + 1;
			j = j + 1;
		}
		return = a;
	}
}
	
def add_2:double[](b : double[] )
{
	j = 0;
	return = [Imperative]
	{
		for ( i in b )
		{
			b[j] = b[j] + 1;
			j = j + 1;
		}
		return = b;
	}
}

b = [ 1.0, 2.0, 3.0 ];
c = add_1(b);
b2 = add_2( b );
";
            object[] expectedResult2 = { 2.0, 3.0, 4.0 };
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            // b1 is updated as well. 
            thisTest.Verify("c", expectedResult2, 0);
            thisTest.Verify("b2", expectedResult2, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void TV51_Defect_1456108_4()
        {
            object[] expectedResult1 = { 1.0, 2.0, 3.0 };
            object[] expectedResult2 = { 2.0, 3.0, 4.0 };
            string code = @"
import(""FFITarget.dll"");
	def add_2:double[]( b : double[] )
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

b= [ 1.0, 2.0, 3.0 ];
t2 = add_2(b);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t2", expectedResult2, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void TV51_Defect_1456108_5()
        {
            string code = @"
import(""FFITarget.dll"");
	def add_1:double[](a: double[],  b : double[])
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
	
	def add_2:double[]( b : double[])
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
c = [ 1.0, 2.0, 3.0 ];
b1 = add_1( c, c );
b2 = add_2(c);
c = [ -1, -1, -1 ];";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1467238 - Sprint25: rev 3420 : REGRESSION : Update issue when class collection property is updated");
            object[] expectedResult1 = { 0.0, 0.0, 0.0 };
            thisTest.Verify("b2", expectedResult1, 0);
            thisTest.Verify("b1", expectedResult1, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void TV52_Defect_1456397()
        {
            string code = @"
import(""FFITarget.dll"");
	def CreateNewVal ( a )
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

b1 = [Associative]
{
	return = CreateNewVal(1);
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b1", 12);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void TV53_Defect_1456397_2()
        {
            string code = @"
import(""FFITarget.dll"");
def CreateNewVal (a )
{
	y = [Associative]
	{
		return = a;
 	}
	return = y + a;
}
b1 = [Imperative]
{
	return = CreateNewVal(1);
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b1", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV54_Defect_1456397_3()
        {
            string code = @"
	def CreateVal ( a )
	{
		x = 1;
		y = [Associative]
		{
			return = a;
		}
		return = x + y;
	}
b1 = [Imperative]
{
	return CreateVal( 1 );
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
	return [Imperative]
    {
		retArr = 5;
        return retArr;
	}
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
    return retArr;
}
x = [Imperative]
{
	x = 0.5;
	return foo(x);
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 5);
        }

        [Test] 
        [Category("SmokeTest")]
        public void TV57_Defect_1454932()
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
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void TV58_Defect_1455278()
        {
            string code = @"
import(""FFITarget.dll"");
	def foo : int ( a1 : int, a )
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
	    return = a + c;
	}

	a = 1;
	b1 = foo(2, a); 
	b2 = foo(0, a); 
";
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
		b = [0, 10];
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
	
	x = [2.5,10.0];
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
	def foo: int( a : int )
	{
        return = [Imperative]
        {
		    for( i in a )
		    {
		    }
		    return = a;
        }
	}
d = [Imperative]
{
	d = [ 1,2,3 ];
	j = 0;
	
	for( i in d )
	{
		d[j] = i + 1;
		j = j + 1;
	}
    return d;
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
	b = [ [0,2,3], [4,5,6] ];
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
	
	a = [ [2.3,3.5],[4.5,5.5] ];
	
	a = foo( a );
	c = a[0];
	d = [Imperative]
	{
		b = [ [2.3], [2.5] ];
		b = foo( b );
		return b[0];
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
c = i[0];
d = i[1];
def foo ( a : double[]..[] )
{
	a[0][0] = 2.5;
	return = a;
}
i=[Imperative]
{
	a = [ [2.3,3.5],[4.5,5.5] ];
	a = foo( a );
	c = a[0];

	d = [Associative]
	{
		b = [ [2.3], [2.5] ];
		return foo( b )[0];

	}
    return [c,d];
};";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedResult2 = { 2.5, 3.5 };
            object[] expectedResult = { 2.5 };
            thisTest.Verify("c", expectedResult2);
            thisTest.Verify("d", expectedResult);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void TV65_Defect_1455090_4()
        {
            string code = @"
import(""FFITarget.dll"");
def foo: int(a : int[]..[])
{
	return = a[0][0];
}	
b = [[1,2],[3,4]];
a = foo( b );
	";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void TV66_Defect_1455090_5()
        {
            string code = @"
import(""FFITarget.dll"");
	def objarray:ClassFunctionality ( arr : ClassFunctionality[]..[] )
	{
		return = arr[1][0];
	}
	
	A1 = ClassFunctionality.ClassFunctionality( 1 );
	A2 = ClassFunctionality.ClassFunctionality( 3 );
	A3 = ClassFunctionality.ClassFunctionality( 5 );
	A4 = ClassFunctionality.ClassFunctionality( 7 );
	
	B = [ [ A1,A2 ],[ A3,A4] ];
	
	b = objarray( B );
	
	c = b.IntVal;
	
	
	
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", 5, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void TV67_Defect_1455090_6()
        {
            string code = @"
import(""FFITarget.dll"");
	def objarray:ClassFunctionality ( arr : ClassFunctionality[]..[] )
	{
		return = arr[1][0];
	}
c = [Imperative]
{
	A1 = ClassFunctionality.ClassFunctionality( 1 );
	A2 = ClassFunctionality.ClassFunctionality( 3 );
	A3 = ClassFunctionality.ClassFunctionality( 5 );
	A4 = ClassFunctionality.ClassFunctionality( 7 );
	
	B = [ [ A1,A2 ],[ A3,A4] ];
		
	b = objarray( B );
	
	return = b.IntVal;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", 5);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void TV68_Defect_1455090_7()
        {
            //Assert.Fail("1463372 - Sprint 20 : Rev 2088 : Imperative code is not allowed in class constructor ");

            string code = @"
import(""FFITarget.dll"");
def create(i:int)
{
	return = [Imperative]
	{
	    a = null;
		if( i == 1 )
		{
			a = [ [ 1,2,3 ] , [ 4,5,6 ] ];
		}
		else
		{
			a = [ [ 1,2,3 ] , [ 1,2,3 ] ];
		}
	    return = a;
	}
}
	
def compare:int ( b : int[]..[], a : int[]..[], i : int, j : int )
{
	temp = [Imperative]
	{
		if( b[i][j] == a[i][j] )
		    return = 1;
		return = 0;
	}
	return = temp ;
}

b1 = [ [1, 2, 3],[ 1, 2, 3] ];
a = create(1);
a1 = compare( b1, a, 0, 0 );
a2 = compare( b1, a, 1, 1 );
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
pts = [pt1, pt2];
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
pts = [pt1, pt2];
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
bcurvePtX = [Imperative]
{
	pt1 = Point.ByCoordinates(0);
	pt2 = Point.ByCoordinates(5);
	pts = [pt1, pt2];
	bcurve = BSplineCurve.ByPoints(pts);
	bcurvePtX = bcurve.P[1].X;
    return bcurvePtX;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("bcurvePtX", 5.0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void TV70_Defect_1456798()
        {
            string code = @"
import(""FFITarget.dll"");
pt1 = DummyPoint.ByCoordinates(0,0,0);
pt2 = DummyPoint.ByCoordinates(5,0,0);
pt3 = DummyPoint.ByCoordinates(10,0,0);
pt4 = DummyPoint.ByCoordinates(15,0,0);
pt5 = DummyPoint.ByCoordinates(20,0,0);
pts = [pt1, pt2, pt3, pt4, pt5];
p = pts[2];
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

def collectioninc: int[]( a : int[] )
{
    return = [Imperative]{
	    j = 0;
	    for( i in a )
	    {
		    a[j] = a[j] + 1;
		    j = j + 1;
	    }
	    return = a;
    }
}
c = [Imperative]
{
	d = [ 1,2,3 ];
	return collectioninc( d );
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
	d = [ 1,2,3 ];
	c = collectioninc( d );
	b = [Imperative]
	{
		return collectioninc( d );
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
        public void TV72_Defect_1454541_1()
        {
            string code = @"
d1;
def singleLine1 : int( a:int ) { return = a * 10; } 
d1 = singleLine1( 2 );
def singleLine2 : int( a:int ) { return = a * 10; } 
d2 = [Imperative]
{
     return singleLine2( 2 );
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("d1", 20);
            thisTest.Verify("d2", 20);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV73_Defect_1451831()
        {
            string code = @"
y;
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

[Associative]
{
	a = 1;
	b = 1;
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
b2;
	
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
[Associative]
{
	b2 = foo (2 );	
}
def foo2 : int ( a : int )
{
    return = [Imperative]
    {
		
		d = 0;
		if( a > 1 )
		{
			d = 1;
		}
			
		return = a + d;
    }
}
b3 = [Imperative]
{
	return foo2 (4 );	
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
arr = [1,2,3,4];
sum = foo1(arr);
def foo2 : double (arr : double)
{
    return = 0;
}
arr1 = [1.0,2.0,3.0,4.0];
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
sum1=i[0];sum2=i[1];
i = [Imperative]
{
	arr1 = [ [1, 2.0], [true, 4] ];
	sum1 = foo1(arr);
	x = 1;
	arr2 = [ [1, 2.0], [x, 4] ];
	sum2 = foo1(arr2);
    return [sum1, sum2];
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object n1 = null;
            thisTest.Verify("sum1", n1);
            thisTest.Verify("sum2", 1.0);

        }

        [Test]
        [Category("DSDefinedClass_Ported")]
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
x1;y11;y21;
x2 = i[0];y12 = i[1];y22 = i[2];
def foo : int ( a : int )
{
	return  = a + 1;
}
	
def foo1  ( a  )
{
	return  = a + 1;
}
[Associative]
{
	x1 = 1;
	y11 = foo(x1);
	y21 = foo1(x1);
	
}
i=[Imperative]
{
	x2 = 1;	
	y12 = foo(x2);
	y22 = foo1(x2);
	return [x2, y12, y22];
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x1", 1);
            thisTest.Verify("y11", 2);
            thisTest.Verify("y21", 2);
            thisTest.Verify("x2", 1);
            thisTest.Verify("y12", 2);
            thisTest.Verify("y22", 2);
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
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void TV79_Defect_1462300()
        {
            string code = @"
import(""FFITarget.dll"");
def testcall(a:ClassFunctionality) 
{ 
return =[a.ImNotDefined(),a.ImNotDefined()]; 
} 
a= ClassFunctionality.ClassFunctionality(); 
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
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void TV79_Defect_1462300_2()
        {
            string code = @"
import(""FFITarget.dll"");
def testcall(a:ClassFunctionality) 
{ 
return =[a.ImNotDefined(),a.ImNotDefined()]; 
} 
a= null; 
b=testcall(a); 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v2 = new Object[] { null, null };
            thisTest.Verify("b", v2);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void TV79_Defect_1462300_3()
        {
            //Assert.Fail("1462300 - sprint 19 - rev 1948-316037 - if return an array of functions as properties then it does not return all of them");

            string code = @"
import(""FFITarget.dll"");
def testcall2 :ClassFunctionality[] () 
{ 	
	return =[ClassFunctionality.ClassFunctionality(), ClassFunctionality.ClassFunctionality()]; 
} 
def testcall3 :int[] () 
{ 	
	return =[ClassFunctionality.ClassFunctionality(), ClassFunctionality.ClassFunctionality()]; 
} 
def testcall4 :ClassFunctionality[] () 
{ 	
	return =[null, null]; 
} 
b = testcall2(); 
d = testcall3(); 
e1 = testcall4(); 
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
            thisTest.Verify("a", 3);
            thisTest.Verify("b", false);
            thisTest.Verify("c", null);
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
            thisTest.Verify("a", 2);
            thisTest.Verify("b", null);
            thisTest.Verify("c", null);
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
a = foo ( [ true, false, [ true, true ] ],  count );
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
a = foo ( [ 1.0, 2.6 ],  count );
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
a = foo ( [ 1, 2 , [3, 4] ],  count );
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
a = foo ( [ 1, 2,  [ 3, 4 ] ],  count );
d = foo ( [ 2, 2.5, [ 1, 1.5 ], 1 , false],  count );

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
	a = foo ( [ 1, 2,  [ 3, 4 ] ],  count );
	d = foo ( [ 2, 2.5, [ 1, 1.5 ], 1 , false],  count );
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
	return c ;
}
def greatest ( a : double[], greater : function )
{
    c = a[0];
	return [Imperative]
	{
	    for ( i in a )
		{
		    c = greater( i, c );
		}
        return c;	
	}
}
def foo ( a : double[], greatest : function , greater : function)
{
    return greatest ( a, greater );
}
a = [Imperative]
{
	return foo ( [ 1.5, 6, 3, -1, 0 ], greatest, greater );
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
	a = greatest ( [ 1.5, 6, 3, -1, 0 ], greater2 );
	
}
";
            thisTest.VerifyRunScriptSource(code, error);
            Object v = null;
            thisTest.Verify("a", v);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void TV85_Function_Return_Type_Var_User_Defined_Type_Conversion()
        {
            string code = @"
import(""FFITarget.dll"");
def goo : var()
{
    return = ClassFunctionality.ClassFunctionality();
}
def foo : ClassFunctionality ()
{
    return = goo();
}
a = foo();
b = a.IntVal;";
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
    return = [ arr[0], arr[1] ];
}
a = f1( [ null, null ] );
b = f2( [ null, null ] );
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v2 = new Object[] { null, null };
            thisTest.Verify("a", v2);
            thisTest.Verify("b", v2);
        }


        [Test]
        [Category("Failure")]
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
            //members are not initialized
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
            fsr.LoadAndPreStart(src);
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
            Assert.AreEqual((bool)vms.mirror.GetDebugValue("y1").Payload, true);
            Assert.AreEqual((bool)vms.mirror.GetDebugValue("y3").Payload, true);
            Assert.AreEqual((bool)vms.mirror.GetDebugValue("y2").Payload, false);
            Assert.AreEqual((bool)vms.mirror.GetDebugValue("y4").Payload, false);
            fsr.Run();
            Assert.AreEqual((bool)vms.mirror.GetDebugValue("y1").Payload, false);
            Assert.AreEqual((bool)vms.mirror.GetDebugValue("y3").Payload, false);
            Assert.AreEqual((bool)vms.mirror.GetDebugValue("y2").Payload, true);
            Assert.AreEqual((bool)vms.mirror.GetDebugValue("y4").Payload, true);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void TV89_Implicit_Type_Conversion_1()
        {
            string code = @"
def bar ( x : bool )
{
	return = x && true;
}
def foo: bool ( x : bool, y : bool )
{
	return [Imperative]
	{
		if( x == false )
        {
			y = true;
        }
		else
        {
			y = false;
        }
        return y;
	}		
}

c = bar ( 6 );
y1 = foo( 6, c);
c1 = bar( 0 );
y2 = foo( 0, c1 ); 
d = [Imperative]
{
    return = bar ( -3 );
}
y3 = foo ( -3, d );
d1 = [Imperative]
{
    return = bar ( 0 );
}
y4 = foo ( 0, d1 );
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("y1", false);
            thisTest.Verify("y3", false);
            thisTest.Verify("y2", true);
            thisTest.Verify("y4", true);
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
            thisTest.Verify("b1", 3);
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
            thisTest.Verify("b1", 3);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void TV90_Defect_1463474_4()
        {
            string code = @"
def foo : void  ( )
{
	a = 2;		
}
b1 = foo();
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object v1 = null;
            thisTest.Verify("b1", v1);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void TV91_Defect_1463703()
        {
            string code = @"
def Create : var ( a)
{
	return = a;
}
def foo2  ( a : int, a1: var)
{
	return  = a + a1;
}
    
b=i[0];c=i[1];
i=[Imperative]
{
	b = Create(1);
	c = foo2(1, b);
    return [b,c];
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("b", 1);
            thisTest.Verify("c", 2);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void TV92_Accessing_Variables_Declared_Inside_Function_Body()
        {
            string code = @"
import(""FFITarget.dll"");
def foo ( )
{
    a = [ 1, 2, 3];
    b = ClassFunctionality.ClassFunctionality(10);
    return = [a,b];
}
x = foo ();
a = x[0][0]; // expected 1, received 1
b = x[1];
c = b.IntVal; // expected 10, received 10";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("a", 1);
            thisTest.Verify("c", 10);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TV93_Modifying_Global_Var_In_Func_Call()
        {
            string code = @"
import(""FFITarget.dll"");
def foo ( p: DummyPoint)
{
	return = DummyPoint.ByCoordinates( (p.X), (p.Y), (p.Z) );
}

def func1(pts : DummyPoint[]) 
{
  pts[1] = pts[0];
  return = null;
}
p1 = DummyPoint.ByCoordinates( 0,0,0);
p2 = DummyPoint.ByCoordinates( 1, 1, 1 );
p = [ p1, p2 ]; 
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
        [Category("DSDefinedClass_Ported")]
        public void TV93_Modifying_Global_Var_In_Func_Call_2()
        {
            string code = @"
import(""FFITarget.dll"");
def foo ( p: DummyPoint)
{
	return = DummyPoint.ByCoordinates( (p.X), (p.Y), (p.Z) );
}
def func1(pts : DummyPoint[]) 
{
  pts[1] = pts[0];
  return = null;
}
p1 = 0;
p2 = 1;
p = [ p1, p2 ]; 
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
        [Category("DSDefinedClass_Ported")]
        public void TV93_Modifying_Global_Var_In_Func_Call_3()
        {
            string code = @"
import(""FFITarget.dll"");
def foo ( p: DummyPoint)
{
	return = DummyPoint.ByCoordinates( (p.X), (p.Y), (p.Z) );
}

def func1(p1 : DummyPoint, p2 : DummyPoint) 
{
  p1 = p1.foo(p2);
  return = null;
}
p1 = DummyPoint.ByCoordinates( 0,0,0);
p2 = DummyPoint.ByCoordinates( 1, 1, 1 );
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
        [Category("DSDefinedClass_Ported")]
        public void TV93_Modifying_Global_Var_In_Func_Call_4()
        {
            string code = @"
import(""FFITarget.dll"");
def foo ( p: DummyPoint)
{
	return = DummyPoint.ByCoordinates( (p.X+X), (p.Y+Y), (p.Z+Z) );
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
points = [ DummyPoint.ByCoordinates( 0,0,0), DummyPoint.ByCoordinates( 1, 1, 1 ), DummyPoint.ByCoordinates( 2, 2, 2 ), DummyPoint.ByCoordinates( 3, 3, 3 ) ];
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
a = [B.B(), B.B(), B.B()];
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
a;
def foo : int  (a)    
{        
    c = a;        
    a = 2;                    
    return = c + 1;                
}  
x = [Imperative]
{    
    a = 1;    
	b = foo(a);        
	return = [ a, b ];    
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 1, 3 };
            thisTest.Verify("x", v1);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV96_Defect_DNL_1465794_2()
        {
            string code = @"
a;
	def foo : int  (a)    
	{        
    return = [Imperative]{
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
}
x = [Imperative]
{    
        a = 2;    
	b = foo(a);        
	return = [ a, b ];    
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 2, 1 };
            thisTest.Verify("x", v1);
        }

        [Test]
        [Category("SmokeTest")]
        public void TV96_Defect_DNL_1465794_3()
        {
            string code = @"
a;
	def foo : int  (a:var[]..[])    
	{        
    return = [Imperative]{
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
}   
x = [Imperative]
{    
    a = [ [ 1, 2 ] , [ 3, 4 ] ];    
	b = foo(a);        
	return = [ a, b ];    
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { new Object[] { 1, 2 }, new Object[] { 3, 4 } };
            Object[] v2 = new Object[] { v1, 1 };
            thisTest.Verify("x", v2);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void TV97_Heterogenous_Objects_As_Function_Arguments_No_Replication()
        {
            string code = @"
import(""FFITarget.dll"");
a1 = ClassFunctionality.ClassFunctionality();
def foo ( x : double[])
{
    return = x;
}
a1 = [ 2.5, 4, 3*2 ];
b1 = foo ( a1 );
a2 = [ 2, 4, 3.5 ];
b2 = foo ( a2 );
a3 = [ 2, 4, 3 ];
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
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void TV97_Heterogenous_Objects_As_Function_Arguments_No_Replication_2()
        {
            string code = @"
import(""FFITarget.dll"");
a = ClassFunctionality.ClassFunctionality(1);
def foo ( x : var[])
{
    return = 1;
}
a1 = [ 2.5, null, 3, a, ""sar"" ];
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
a1 = [ 2.5, 4 ];
b1 = foo ( a1 );
a2 = [ 3, 4, 2.5 ];
b2 = foo ( a2 );
a3 = [ 3, 4, 2 ];
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
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void TV97_Heterogenous_Objects_As_Function_Arguments_With_Replication_2()
        {
            string code = @"
import(""FFITarget.dll"");
a1 = ClassFunctionality.ClassFunctionality();
def foo ( x : var)
{
    return = 1;
}
a1 = [ 2.5, null, 3, a1, ""sar"" ];
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
    
x = foo ( [ [ 0,1], [2, 3] ] );
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
a2 = [ 2, 4, 3.5 ];
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
a2 = [ 2, 4, 3];
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
a1 = [ 2, 4.1, 3.5];
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
a1 = [ 2, 4.1, false];
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
a1 = [ 2, 4.1, [1,2]];
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
a1 = [ null, 5, 6.0];
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
a1 = [ 1, null, 6.0];
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
a1 = [ null, null, null];
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
a1 = [1.1,2.0,3];
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
        [Category("DSDefinedClass_Ported")]
        public void TV99_Defect_1463456_Array_By_Reference_Issue()
        {
            string errmsg = " 1467318 -  Cannot return an array from a function whose return type is var with undefined rank (-2)  ";
            string code = @"
import(""FFITarget.dll"");
a = ArrayMember.Ctor([1,2,3]);
val = a.X;
val[0] = 100;
t = a.X[0]; //expected 100; received 1";
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
val = [1,2,3];
b = A(val);
b[0] = 100; 
t = val[0]; //expected 100, received 1";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t", 1);
        }

        [Test]
        public void TV101_Indexing_IntoArray_InFunctionCall_1463234()
        {
            // Assert.Fail("1467131- Sprint 24 - Rev 2910 method overload with replication , throws error WARNING: Multiple type+pattern match parameters found, non-deterministic dispatch" );
            string code = @"
def foo()
{
return = [1,2];
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

def foo()
{
return = [1,2];
}
t=[Imperative]
{
return foo()[0];
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
return = [1];
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
return = [];
}
t = foo()[0];";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object t = null;
            thisTest.Verify("t", t);
        }

        [Test]
        public void TV101_Indexing_Intovariablenotarray_InFunctionCall_1463234_4()
        {
            string code = @"
def foo()
{
return = 1;
}
t = foo()[0];";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t", null);
        }

        [Test]
        public void TV101_Indexing_IntoNested_FunctionCall_1463234_5()
        {
            // Assert.Fail("1467131- Sprint 24 - Rev 2910 method overload with replication , throws error WARNING: Multiple type+pattern match parameters found, non-deterministic dispatch" );
            string code = @"
	def foo()
	{
		return = [foo2()[0],foo2()[1]];
	}
def foo2()
{
return = [1,2];
}
a=test.test()[0];
t = foo()[0];";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("t", 1);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TV101_Indexing_Into_classCall_1463234_6()
        {
            // Assert.Fail("1467131- Sprint 24 - Rev 2910 method overload with replication , throws error WARNING: Multiple type+pattern match parameters found, non-deterministic dispatch" );
            string code = @"
def foo()
{
	return = [1,2];
}
t = foo()[0];";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("t", 1);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TV101_Indexing_Into_classCall_1463234_7()
        {
            // Assert.Fail("1467131- Sprint 24 - Rev 2910 method overload with replication , throws error WARNING: Multiple type+pattern match parameters found, non-deterministic dispatch" );
            string code = @"
def foo()
{
	return = [1,2];
}
t = [Imperative]
{
    return = foo()[0];
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t", 1);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TV102_GlobalVariable_Function_1466768()
        {
            // Assert.Fail("1467131- Sprint 24 - Rev 2910 method overload with replication , throws error WARNING: Multiple type+pattern match parameters found, non-deterministic dispatch" );
            string code = @"
import(""FFITarget.dll"");
def foo ( p: DummyPoint)
{
	return = DummyPoint.ByCoordinates( (p.X), (p.Y), (p.Z) );
}
def func1(p1 : DummyPoint, p2 : DummyPoint)
{
	p1 = foo(p2);
	return = null;
}
p1 = DummyPoint.ByCoordinates( 0,0,0);
p2 = DummyPoint.ByCoordinates( 1, 1, 1 );
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
        [Category("DSDefinedClass_Ported")]
        public void TV102_GlobalVariable_Function_1466768_1()
        {
            // Assert.Fail("1467131- Sprint 24 - Rev 2910 method overload with replication , throws error WARNING: Multiple type+pattern match parameters found, non-deterministic dispatch" );
            string code = @"
import(""FFITarget.dll"");
def foo ( p: DummyPoint)
{
	return = DummyPoint.ByCoordinates( (p.X), (p.Y), (p.Z) );
}
def func1(p2)
{
	p1 = foo(p2);
	return = p1;
}
p1 =  DummyPoint.ByCoordinates( 0,0,0);
p2 =  DummyPoint.ByCoordinates( 1, 1, 1 );
p1 = func1(p2);
xx = p1.X; 
yy = p1.Y; 
zz = p1.Z; 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("xx", 1);
            thisTest.Verify("yy", 1);
            thisTest.Verify("zz", 1);
        }


        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TV102_GlobalVariable_Function_1466768_2()
        {
            // Assert.Fail("1467131- Sprint 24 - Rev 2910 method overload with replication , throws error WARNING: Multiple type+pattern match parameters found, non-deterministic dispatch" );
            string code = @"
import(""FFITarget.dll"");
def foo ( p: DummyPoint)
{
	return = DummyPoint.ByCoordinates( (p.X), (p.Y), (p.Z) );
}
def func1(p1 : DummyPoint, p2 : DummyPoint)
{
	p1 = foo(p2);
	return = null;
}
def func1(pts : DummyPoint[])
{
pts[1] = pts[0];
return = null;
}
p1 = DummyPoint.ByCoordinates( 0,0,0);
p2 = DummyPoint.ByCoordinates( 1, 1, 1 );
p = [ p1, p2 ];
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
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void TV103_Defect_1467149()
        {
            string code = @"
import(""FFITarget.dll"");
convert=[ClassFunctionality.ClassFunctionality(1..2), ClassFunctionality.ClassFunctionality(3)];
def prop(test:ClassFunctionality)
{
	return= test.IntVal;
}
b=prop(convert);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1467183 - Sprint24: rev 3163 : replication on nested array is outputting extra brackets in some cases");
            thisTest.Verify("b", new Object[] { new Object[] { 1.0, 2.0 }, 3.0 });

        }

        [Test]
        [Category("SmokeTest")]
        public void TV103_Defect_1455090_Rank_Of_Arg()
        {
            String code =
@"
b;
def foo : int ( a : double[][] )
{
    return = a[0][0] ; 
}
[Associative]
{
    a = [ [ 0, 1], [2, 3] ];
    b = foo ( a );
}
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void TV103_Defect_1455090_Rank_Of_Arg_2()
        {
            String code =
@"
def foo : var[][][] (X : var[][][])
{
    return = X ; 
}
a = [ [ [ 0, 1] ], [ [2, 3] ] ];
c = foo(a);";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", new Object[] { new Object[] { new Object[] { 0.0, 1.0 } }, new Object[] { new Object[] { 2.0, 3.0 } } });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void TV103_Defect_1455090_Rank_Of_Arg_4()
        {
            String code =
@" 
def Create : var[][]( b : double[][] )
{
    return = b;
}
def foo : var[][][] (  X : var[][])
{
    return = X ; 
}

a = [ [ 0, 1] ,  2  ];
b = Create( a );
c = foo(b);
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
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
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
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
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1467183 - Sprint24: rev 3163 : replication on nested array is outputting extra brackets in some cases");
            thisTest.Verify("c", new Object[] { 1 });
        }


        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void TV103_Defect_1455090_Rank_Of_Arg_5()
        {
            String code =
                @"
def Create : var[][]( b : double[][] )
{
    return = b;
}
def foo : var[][][] (  X : var[][])
{
    return = X; 
}
                a = [ 3, [ 0, 1] ,  2  ];
                b = Create ( a);
                c = foo(b);
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
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
a = [ 3, [ 0, 1] ,  2  ];
b = foo ( a );";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1467183 - Sprint24: rev 3163 : replication on nested array is outputting extra brackets in some cases");

            thisTest.Verify("b", new Object[] { new object[] { 3 }, new Object[] { 0, 1 }, new object[] { 2 } });
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
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
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
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
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
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
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
a = [1,2,2,1];
b = [1,[]];
c = Average(a);
c1= foo(a);
c2 = foo(b);
c3 = Average([]);
result = [foo(a),foo(b)];";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4171
            string errmsg = "MAGN-4171: Replication method resolution";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("result", new Object[] { 1.5, new Object[] { 1.0, 0.0 } });
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
d = foo([1.5,2.5]);";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("d", 2);
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
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            string err = "1467379 Sprint 27 - Rev 4193 - after throwing warning / error in the attached code it should execute rest of the code ";
            thisTest.RunScriptSource(code, err);
            thisTest.Verify("b", 1);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void TV105_Defect_1467409()
        {
            String code =
@"
import(""FFITarget.dll"");
def foo ( x1: int, y1 : int )
{
    return = x1;
}
a=ClassFunctionality.ClassFunctionality();
r=a.foo(); // calling a non-exist function shouldn't get a warning at complie time
b = foo1();
d = foo(2, ClassFunctionality.ClassFunctionality() );
f = foo3();
r1;b1;d1;
[Imperative]
{
    r1=a.foo(); 
    b1 = foo1();
    d1 = foo(2, ClassFunctionality.ClassFunctionality() );
}
def foo3()
{
    r2=a.foo(); 
    b2 = foo1();
    d2 = foo(2, ClassFunctionality.ClassFunctionality() );
    return = [ r2, b2, d2 ];
}
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
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
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
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
	def foo : int (a : int, b: int)
	{
		return = a + b; 
	}
	
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
            def foo : double(arg : double) { return = arg + 1 ;}
            a = foo(""a""); ";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", null);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.Runtime.WarningID.MethodResolutionFailure);
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
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.FunctionNotFound);
        }

        [Test]
        [Category("SmokeTest")]
        public void T65_1467115_warning_on_function_resolution_1()
        {
            String code = @"
x = [ 1,2];
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
    a = [ 5, 10, 15 ];
    
    a = 5..15..2;
}
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", new Object[] { 10, 14, 18, 22, 26, 30 });
            thisTest.VerifyBuildWarningCount(0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestMultiOverLoadWithDefaultArg()
        {
            string code = @"
import(""FFITarget.dll"");
def foo(x = 0, y = 0, z = 0)
{
    return = 41;
}
    
def foo(x : ClassFunctionality)
{
    return = 42;
}


b = ClassFunctionality.ClassFunctionality();
r = foo(b); // shoudn't be resolved to foo(x = 0, y = 0, z = 0)
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
