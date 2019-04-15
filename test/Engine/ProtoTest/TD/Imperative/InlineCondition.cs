using System;
using System.Collections.Generic;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
namespace ProtoTest.TD.Imperative
{
    class InlineCondition : ProtoTestBase
    {
        [Test]
        [Category("SmokeTest")]
        public void T001_Inline_Using_Function_Call()
        {
            string src = @"
	def fo1 : int(a1 : int)
	{
		return a1 * a1;
	}
i = [Imperative]
{
	a	=	10;				
	b	=	20;
				
	smallest1   =   a	<   b   ?   a	:	b;
	largest1	=   a	>   b   ?   a	:	b;
	d = fo1(a);
	smallest2   =   (fo1(a))	<   (fo1(b))  ?   (fo1(a))	:	(fo1(a));	//100
	largest2	=   (fo1(a)) >   (fo1(b))  ?   (fo1(a))	:	(fo1(b)); //400
    return [smallest2, largest2];
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("i", new[] {100, 400});
        }

        [Ignore]
        [Category("SmokeTest")]
        public void T002_Inline_Using_Math_Lib_Functions()
        {
            string src = @"[Imperative]
{
	external (""libmath"") def dc_sqrt : double (val : double);
def sqrt : double (val : double)
{
     return = dc_sqrt(val);
}
def fo1 (a1 : int)  = a1 * a1 ;
a    =    10;                
b    =    20;
                   
smallest1   =   a  <   b   ?   a :    b; //10
largest1  =   a     >   b   ?   a :    b; //20
d = fo1(a);
smallest2   =   sqrt(fo1(a)) <   sqrt(fo1(b))  ?   sqrt(fo1(a))    :     sqrt(fo1(a)); //10.0
largest2  =   sqrt(fo1(a)) >   sqrt(fo1(b))  ?   sqrt(fo1(a))  :     sqrt(fo1(b)); //20.0
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            // expected "StatementUsedInAssignment" warning
            thisTest.Verify("smallest2", 10.0);
            thisTest.Verify("largest2", 20.0);
        }
        [Ignore]
        public void T003_Inline_Using_Collection()
        {
            string src = @"[Imperative]
{
	Passed = 1;
	Failed = 0;
	Einstein = 56;
	BenBarnes = 90;
	BenGoh = 5;
	Rameshwar = 80;
	Jun = 68;
	Roham = 50;
	Smartness = [ BenBarnes, BenGoh, Jun, Rameshwar, Roham ]; // { 1, 0, 1, 1, 0 }
	Results = Smartness > Einstein ? Passed : Failed;
	
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            // expected "StatementUsedInAssignment" warning
            List<Object> result = new List<object> { 1, 0, 1, 1, 0, };
            thisTest.Verify("Results", result);
        }
        [Ignore]
        public void T005_Inline_Using_2_Collections_In_Condition()
        {
            string src = @"[Imperative]
{
	a1 	=  1..3..1; 
	b1 	=  4..6..1; 
	a2 	=  1..3..1; 
	b2 	=  4..7..1; 
	a3 	=  1..4..1; 
	b3 	=  4..6..1; 
	c1 = a1 > b1 ? true : false; // { false, false, false }
	c2 = a2 > b2 ? true : false; // { false, false, false }
	c3 = a3 > b3 ? true : false; // { false, false, false, null }
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            // expected "StatementUsedInAssignment" warning
            List<Object> c1 = new List<object> { false, false, false };
            List<Object> c2 = new List<object> { false, false, false };
            List<Object> c3 = new List<object> { false, false, false, null };
            thisTest.Verify("c1", c1);
            thisTest.Verify("c2", c2);
            thisTest.Verify("c3", c3);
        }

        [Ignore]
        public void T006_Inline_Using_Different_Sized_1_Dim_Collections()
        {
            string src = @"[Imperative]
{
	a = 10 ;
	b = ((a - a / 2 * 2) > 0)? a : a+1 ; //11
	c = 5; 
	d = ((c - c / 2 * 2) > 0)? c : c+1 ; //5 
	e1 = ((b>(d-b+d))) ? d : (d+1); //5
	//inline conditional, returning different sized collections
	c1 = [1,2,3];
	c2 = [1,2];
	a1 = [1, 2, 3, 4];
	b1 = a1>3?true:a1; // expected : {1, 2, 3, true}
	b2 = a1>3?true:c1; // expected : {1, 2, 3}
	b3 = a1>3?c1:c2;   // expected : {1, 2}
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            // expected "StatementUsedInAssignment" warning
            List<Object> b1 = new List<object> { 1, 2, 3, true };
            List<Object> b2 = new List<object> { 1, 2, 3 };
            List<Object> b3 = new List<object> { 1, 2 };
            thisTest.Verify("b1", b1);
            thisTest.Verify("b2", b2);
            thisTest.Verify("b3", b3);
        }
        [Ignore]
        public void T007_Inline_Using_Collections_And_Replication()
        {
            string src = @"[Imperative]
{
	def even : int(a : int)
	{
		return = a * 2;
	}
	a =1..10..1 ; //{1,2,3,4,5,6,7,8,9,10}
	i = 1..5; 
	b = ((a[i] % 2) > 0)? even(a[i]) : a ;  // { 1, 6, 3, 10, 5 }	
	c = ((a[0] % 2) > 0)? even(a[i]) : a ; // { 4, 6, 8, `0, `2 }
	d = ((a[-2] % 2) == 0)? even(a[i]) : a ; // { 1, 2,..10}
	e1 = (a[-2] == d[9])? 9 : a[1..2]; // { 2, 3 }
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            // expected "StatementUsedInAssignment" warning
            List<Object> b = new List<object> { 1, 6, 3, 10, 5 };
            List<Object> c = new List<object> { 4, 6, 8, 0, 2 };
            List<Object> d = new List<object> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            List<Object> e1 = new List<object> { 2, 3 };
            thisTest.Verify("b", b);
            thisTest.Verify("c", c);
            thisTest.Verify("d", d);
            thisTest.Verify("e1", e1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T008_Inline_Returing_Different_Ranks()
        {
            string src = @"
x = [Imperative]
{
	a = [ 0, 1, 2, 4];
	x = a > 1 ? 0 : [1,1]; // { 1, 1} ? 
	x_0 = x[0];
	x_1 = x[1];
    return x;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            List<Object> x = new List<object> { 1, 1 };
            thisTest.Verify("x", x);
        }
        [Ignore]
        public void T009_Inline_Using_Function_Call_And_Collection_And_Replication()
        {
            string src = @"[Imperative]
{
	def even(a : int)
	{
		return = a * 2;
	}
	def odd(a : int ) 
	{
	return = a* 2 + 1;
	}
	x = 1..3;
	a = ((even(5) > odd(3)))? even(5) : even(3); //10
	b = ((even(x) > odd(x+1)))?odd(x+1):even(x) ; // {2,4,6}
	c = odd(even(3)); // 13
	d = ((a > c))?even(odd(c)) : odd(even(c)); //53
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            // expected "StatementUsedInAssignment" warning
            List<Object> b = new List<object> { 2, 4, 6 };
            thisTest.Verify("b", b);
            thisTest.Verify("a", 10);
            thisTest.Verify("c", 13);
            thisTest.Verify("d", 53);
        }

        [Test]
        [Category("SmokeTest")]
        public void T010_Inline_Using_Literal_Values()
        {
            string src = @"
i = [Imperative]
{
	a = 1 > 2.5 ? false: 1;
	b = 0.55 == 1 ? true : false;
	c = (( 1 + 0.5 ) / 2 ) <= (200/10) ? (8/2) : (6/3);
	d = true ? true : false;
	e = false ? true : false;
	f = true == true ? 1 : 0.5;
	g = (1/3.0) > 0 ? (1/3.0) : (4/3);
	h = (1/3.0) < 0 ? (1/3.0) : (4/3);
    return [a, b, c, d, e, f, g, h];
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("i", new object[] {1, false, 4, true, false, 1, 0.33333333333333331, 1});
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T011_Inline_Using_Variables()
        {
            string code = @"
import(""FFITarget.dll"");
i = [Imperative]
{
	a = 1;
	b = 0.5;
	c = -1;
	d = true;
	f = null;
	g = false;
	h = ClassFunctionality.ClassFunctionality(1);
	i = h.IntVal;
	
	x1 = a > b ? c : d;
	x2 = a <= b ? c : d;
	
	x3 = f == g ? h : i;
	x4 = f != g ? h : i;
    x5 = f != g ? h : h.IntVal;	
	
	temp = x3.IntVal;
    return [x1, x2, x4, x5, x3, temp];
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("i", new object[] {-1, true, 1, 1, 1, null });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T012_Inline_Using_Fun_Calls()
        {
            string code = @"
import(""FFITarget.dll"");
def power ( a )
{
    return = a * a ;
}
a = power(1);
b = power(0.5);
c = -1;
d = true;
f = null;
g = false;
h = ClassFunctionality.ClassFunctionality(1);
i = h.IntVal;
x1 = power(power(2)) > power(2) ? power(1) : power(0);
x2 = power(power(2)) < power(2) ? power(1) : power(0);
x3 = power(c) < b ? power(1) : power(0);
x4 = power(f) >= power(1) ? power(1) : power(0);
x5 = power(f) < power(1) ? power(1) : power(0);
x6 = power(i) >= power(h.IntVal) ? power(1) : power(0);
x7 = power(f) >= power(i) ? power(1) : power(0);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object n1 = null;
            thisTest.SetErrorMessage("1467231 - Sprint 26 - Rev 3393 null to bool conversion should not be allowed");
            thisTest.Verify("x1", 1);
            thisTest.Verify("x2", 0);
            thisTest.Verify("x3", 0);
            thisTest.Verify("x4", 0);
            thisTest.Verify("x5", 0);
            thisTest.Verify("x6", 1);
            thisTest.Verify("x7", 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T013_Inline_Using_Class()
        {
            string code = @"

def foo ( a, b )
{
	return = a * b ;
}
	

def power ( a )
{
    return = a * a ;
}
a = -1;
x1 = a <  foo(a, 2) ? a : foo(a, 2);
x2 = a >= foo(a, 2) ? a : foo(a, 2);
x3 = foo(a, power(3)) <  power(foo(0, 3)) ? foo(a, power(3)) : power(foo(0, 3));
x4 = foo(a, power(3)) >= power(foo(0, 3)) ? foo(a, power(3)) : power(foo(0, 3));
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x1", -2, 0);
            thisTest.Verify("x2", -1, 0);
            thisTest.Verify("x3", -9, 0);
            thisTest.Verify("x4", 0, 0);
        }

        [Test]
        [Category("Replication")]
        [Category("Failure")]
        public void T014_Inline_Using_Collections()
        {

            string err = " comparison of collection with singleton should yield null in imperative scope";
            string src = @"t1;t2;t3;t4;t5;t7;
c1;c2;c3;c4;
[Imperative]
{
	a = [ 0, 1, 2];
	b = [ 3, 11 ];
	c = 5;
	d = [ 6, 7, 8, 9];
	e = [ 10 ];
	x1 = a < 5 ? b : 5;
	t1 = x1[0];
	t2 = x1[1];
	c1 = 0;
	for (i in x1)
	{
		c1 = c1 + 1;
	}
	
	x2 = 5 > b ? b : 5;
	t3 = x2[0];
	t4 = x2[1];
	c2 = 0;
	for (i in x2)
	{
		c2 = c2 + 1;
	}
	
	x3 = b < d ? b : e;
	t5 = x3[0];
	c3 = 0;
	for (i in x3)
	{
		c3 = c3 + 1;
	}
	
	x4 = b > e ? d : [ 0, 1];
	t7 = x4[0];	
	c4 = 0;
	for (i in x4)
	{
		c4 = c4 + 1;
	}	
}
";
            thisTest.VerifyRunScriptSource(src, err);


            thisTest.Verify("t1", 3);
            thisTest.Verify("t2", 11);
            thisTest.Verify("c1", 2);
            thisTest.Verify("t3", 3);
            thisTest.Verify("t4", 5);
            thisTest.Verify("c2", 2);
            thisTest.Verify("t5", 3);
            thisTest.Verify("c3", 1);
            thisTest.Verify("t7", 0);
            thisTest.Verify("c4", 1);
        }

        [Test]
        [Category("Replication")]
        public void T016_Inline_Using_Operators()
        {
            //Assert.Fail("1456751 - Sprint16 : Rev 990 : Inline conditions not working with replication over collections "); 

            string code = @"
def foo (a:int )
{
	 return = a;   
}
a = 1+2 > 3*4 ? 5-9 : 10/2;
b = a > -a ? 1 : 0;
c = 2> 1 && 4>3 ? 1 : 0;
d = 1 == 1 || (1 == 0) ? 1 : 0;
e1 = a > b && c > d ? 1 : 0;
f = a <= b || c <= d ? 1 : 0;
g = foo([ 1, 2 ]) > 3+ foo([4,5,6]) ?  1 : 3+ foo([4,5,6]);
i = [1,3] > 2 ? 1: 0;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] array2 = { 0, 1 };
            thisTest.Verify("a", 5.0, 0);
            thisTest.Verify("b", 1, 0);
            thisTest.Verify("c", 1, 0);
            thisTest.Verify("d", 1, 0);
            thisTest.Verify("e1", 0, 0);
            thisTest.Verify("f", 1, 0);
            thisTest.Verify("g", new [] {7, 8}, 0);
            thisTest.Verify("i", array2, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T017_Inline_In_Function_Scope()
        {
            string code = @"
def foo1 ( b )
{
	return = b == 0 ? b : b+1;
	
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
	return = y + x1;
}
a1 = foo1(4);
a2 = foo2(3);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a1", 5, 0);
            thisTest.Verify("a2", 8, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T019_Defect_1456758()
        {
            string code = @"
b = true;
a1 = b && true ? -1 : 1;
a2 = [Imperative]
{
	return b && true ? -1 : 1;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a1", -1);
            thisTest.Verify("a2", -1);

        }

        [Test]
        [Category("SmokeTest")]
        public void T020_Nested_And_With_Range_Expr()
        {
            string code = @"
a1 =  1 > 2 ? true : 2 > 1 ? 2 : 1;
a2 =  1 > 2 ? true : 0..3;
b = [0,1,2,3];
a3 = 1 > 2 ? true : b;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] ExpectedRes_1 = { 0, 1, 2, 3 };
            Object[] ExpectedRes_2 = { 0, 1, 2, 3 };
            thisTest.Verify("a1", 2, 0);
            thisTest.Verify("a2", ExpectedRes_1, 0);
            thisTest.Verify("b", ExpectedRes_2, 0);
            thisTest.Verify("a3", ExpectedRes_2, 0);
        }

        [Test]
        [Category("Replication")]
        public void T22_Defect_1467166()
        {
            String code =
@"xx = [Imperative] 
{
    a = [ 0, 1, 2]; 
    return a < 1 ? 1 : 0;
}
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("xx", 0);
        }

        [Test]
        [Category("Replication")]
        public void T22_Defect_1467166_2()
        {
            String code =
@"xx = [Imperative] 
{
    a = [ 0, 1, 2]; 
    return 2 > 1 ? a : 0;
}
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("xx", new Object[] { 0, 1, 2 });
        }

        [Test]
        [Category("Replication")]
        public void T22_Defect_1467166_3()
        {
            String code =
@"
def foo()
{
    return null;
}
i = [Imperative] 
{
   x1 = null == null ? 1 : 0;
   x2 = null != null ? 1 : 0;
   x3 = null == a ? 1 : 0;
   x4 = foo2(1) == a ? 1 : 0;
   x5 = foo() == null ? 1 : 0;
   return [x1, x2, x3, x4, x5];
}
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("i", new[] {1, 0, 1, 1, 1});
        }


        [Test]
        [Category("Replication")]
        public void T23_1467403_inline_null()
        {
            String code =
@"a = null;
d2 = (a!=null)? 1 : 0;
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("d2", 0);
            thisTest.VerifyBuildWarningCount(0);
        }
    }
}
