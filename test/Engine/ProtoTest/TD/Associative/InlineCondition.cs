using System;
using System.Collections.Generic;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
namespace ProtoTest.TD.Associative
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
		return = a1 * a1;
	}
	a	=	10;				
	b	=	20;
				
	smallest1   =   a	<   b   ?   a	:	b;
	largest1	=   a	>   b   ?   a	:	b;
	d = fo1(a);
	smallest2   =   (fo1(a))	<   (fo1(b))  ?   (fo1(a))	:	(fo1(a));	//100
	largest2	=   (fo1(a)) >   (fo1(b))  ?   (fo1(a))	:	(fo1(b)); //400
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            // expected "StatementUsedInAssignment" warning
            thisTest.Verify("smallest2", 100);
            thisTest.Verify("largest2", 400);
        }

        [Test]
        [Category("Replication")]
        public void T003_Inline_Using_Collection()
        {
            //Assert.Fail("1456751 - Sprint16 : Rev 990 : Inline conditions not working with replication over collections"); 
            string src = @"
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
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            // expected "StatementUsedInAssignment" warning
            List<Object> result = new List<object>() { 1, 0, 1, 1, 0, };
            thisTest.Verify("Results", result);
        }


        [Test]
        [Category("Replication")]
        public void T005_Inline_Using_2_Collections_In_Condition()
        {
            //Assert.Fail("1456751 - Sprint16 : Rev 990 : Inline conditions not working with replication over collections"); 
            string src = @"
	a1 	=  1..3..1; 
	b1 	=  4..6..1; 
	a2 	=  1..3..1; 
	b2 	=  4..7..1; 
	a3 	=  1..4..1; 
	b3 	=  4..6..1; 
	c1 = a1 > b1 ? true : false; // { false, false, false }
	c2 = a2 > b2 ? true : false; // { false, false, false }
	c3 = a3 > b3 ? true : false; // { false, false, false, null }
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            // expected "StatementUsedInAssignment" warning
            List<Object> c1 = new List<object>() { false, false, false };
            List<Object> c2 = new List<object>() { false, false, false };
            List<Object> c3 = new List<object>() { false, false, false };
            thisTest.Verify("c1", c1);
            thisTest.Verify("c2", c2);
            thisTest.Verify("c3", c3);
        }


        [Test]
        [Category("Replication")]
        public void T006_Inline_Using_Different_Sized_1_Dim_Collections()
        {
            //Assert.Fail("1456751 - Sprint16 : Rev 990 : Inline conditions not working with replication over collections"); 
            string src = @"
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
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("b1", new object[] {1, 2, 3,true});
            thisTest.Verify("b2", new object[] {1, 2, 3});
            thisTest.Verify("b3", new object[] {1, 2});
        }


        [Test]
        [Category("Replication")]
        public void T007_Inline_Using_Collections_And_Replication()
        {
            string src = @"
	def even : int(a : int)
	{
		return = a * 2;
	}
	a =1..10..1 ; //{1,2,3,4,5,6,7,8,9,10}
	i = 1..5; 
	b = ((a[i] % 2) > 0)? even(a[i]) : a ;  // { 1, 6, 3, 10, 5 }	
	c = ((a[0] % 2) > 0)? even(a[i]) : a ; // { 4, 6, 8, 10, 12 }
	d = ((a[-2] % 2) == 0)? even(a[i]) : a ; // { 1, 2,..10}
	e1 = (a[-2] == d[9])? 9 : a[1..2]; // { 2, 3 }
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            List<Object> d = new List<object>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            thisTest.Verify("c", new object[] { 4, 6, 8, 10, 12 });
            thisTest.Verify("d", new object[] { 1, 2, 3, 4, 5 });
            thisTest.Verify("e1", new object[] { 2, 3});

        }


        [Test]
        [Category("Replication")]
        public void T008_Inline_Returing_Different_Ranks()
        {
            //Assert.Fail("1456751 - Sprint16 : Rev 990 : Inline conditions not working with replication over collections"); 
            string src = @"
	a = [ 0, 1, 2, 4];
	x = a > 1 ? 0 : [1,1]; // { 1, 1} ? 
	x_0 = x[0];
	x_1 = x[1];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("x", new object[] {1, 1});
        }

        [Test]
        [Category("Replication")]
        public void T009_Inline_Using_Function_Call_And_Collection_And_Replication()
        {
            String errmsg = "1467166 - Sprint24 : rev 3133 : Regression : comparison of collection with singleton should yield null in imperative scope";
            string src = @"
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
";
            thisTest.VerifyRunScriptSource(src, errmsg);
            Object n1 = null;
            thisTest.Verify("b", new object[] {2, 4, 6});
            thisTest.Verify("a", 10);
            thisTest.Verify("c", 13);
            thisTest.Verify("d", 53);
        }


        [Test]
        public void T010_Defect_1456751_replication_issue()
        {
            String errmsg = "1467166 - VALIDATION NEEDED - Sprint24 : rev 3133 : Regression : comparison of collection with singleton should yield null in imperative scope";
            string src = @"xx;
x1;
x2;
x3;
x4;
i=[Imperative]
{
	a = [ 0, 1, 2];
	b = [ 3, 11 ];
	c = 5;
	d = [ 6, 7, 8, 9];
	e = [ 10 ];
	xx = 1 < a ? a : 5; // expected:5 
        yy = 0;
	if( 1 < a )
	    yy = a;
	else
	    yy = 5;
	x1 = a < 5 ? b : 5; // expected:5 
	t1 = x1[0];
	t2 = x1[1];
	c1 = 0;
	for (i in x1)
	{
		c1 = c1 + 1;
	}
	x2 = 5 > b ? b : 5; // expected:5
	t3 = x2[0];
	t4 = x2[1];
	c2 = 0;
	for (i in x2)
	{
		c2 = c2 + 1;
	}
	x3 = b < d ? b : e; // expected: {10}
	t5 = x3[0];
	c3 = 0;
	for (i in x3)
	{
		c3 = c3 + 1;
	}
	x4 = b > e ? d : [ 0, 1]; // expected {0,1}
	t7 = x4[0]; 
	c4 = 0;
	for (i in x4)
	{
		c4 = c4 + 1;
	}
    return [xx,x1,x2,x3,x4];
}
/*
Expected : 
result1 = { 5, 5, 2 };
thisTest.Verification(mirror, ""xx"", result1, 1);
thisTest.Verification(mirror, ""t1"", 3, 1);
thisTest.Verification(mirror, ""t2"", 11, 1);
thisTest.Verification(mirror, ""c1"", 2, 1);
thisTest.Verification(mirror, ""t3"", 3, 1);
thisTest.Verification(mirror, ""t4"", 5, 1);
thisTest.Verification(mirror, ""c2"", 2, 1);
thisTest.Verification(mirror, ""t5"", 3, 1); 
thisTest.Verification(mirror, ""c3"", 1, 1);
thisTest.Verification(mirror, ""t7"", 0, 1);
thisTest.Verification(mirror, ""c4"", 1, 1);*/
";
            thisTest.VerifyRunScriptSource(src, errmsg);
            thisTest.Verify("i", new object[] {5, 5, 5, new object[] {10}, new object[] {0, 1}});
        }


        [Test]
        [Category("SmokeTest")]
        public void T010_Defect_1456751_execution_on_both_true_and_false_path_issue()
        {
            // Tracked by: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4026
            string src = @"
a = 0;
def foo (a)
{
    a = a + 1;
    return = a;
}
x = 1 > 2 ? foo(a) + 1 : foo(a) + 2;
	
";
            string err = "MAGN-4026 Execution of both true and false statements in Associative inline condition";
            ExecutionMirror mirror = thisTest.RunScriptSource(src, err);
            thisTest.Verify("x", 3);
        }


        [Test]
        [Category("SmokeTest")]
        [Category("Failure")]
        public void T011_Defect_1467281_conditionals()
        {
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3941
            string code =
               @"x = 2 == [ ]; 
                 y = []==null;
                 z = [[1]]==[1];
                 z2 = [ [ 1 ] ] == 1;
                 z3=1==[[1]];
                 z4=[1]==[[1]];";
            thisTest.RunScriptSource(code);
            thisTest.SetErrorMessage("MAGN-3941 [Design Issue] Errors with conditionals with empty arrays and ararys with different ranks");
            thisTest.Verify("x", false); //WAITING FOR DESIGN DECISION
            thisTest.Verify("y", false);//WAITING FOR DESIGN DECISION
            thisTest.Verify("z", null);
            thisTest.Verify("z2", new object[] { new object[] { true } });
            thisTest.Verify("z3", new object[] { new object[] { true } });
            thisTest.Verify("z4", null);
        }


        [Test]
        public void T012_Defect_1467288()
        {
            string code =
@"c = 0;
a = [ 0, 1, 2 ];
a = c > 1 ? a : a + 1;
//expected : { 1, 2, 3 }";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", new Object[] { 1, 2, 3 });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestInlineArrayInFunction()
        {
            string code =
@"
def foo (c)
{
    a = c == 4 ? c : c + 1;
    return = a;
}

test = foo([1,2]);
";
            string errmsg = "";

            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new Object[] { 2, 3 });
        }

        [Test]
        public void T013_Defect_1467290()
        {
            string code =
@"c = 0;
x = 10;
x = c > 1 ? 3 : 4;
[Imperative]
{
    c = 3;            
}
test = x;";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("x", 4);
        }


        [Test]
        public void T013_Defect_1467290_2()
        {
            string code =
@"c = 0;
x = 10;
a = 1;
b = [1,2];
x = c > 1 ? a : b;
[Imperative]
{
    c = 3; 
    a = 2;           
}
test = x;";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("x", new object[] {1, 2});
        }


        [Test]
        public void T013_Defect_1467290_3()
        {
            string code =
@"c = 0;
x = 10;
a = 1;
b = [1,2];
x = c > 1 ? a : b;
[Imperative]
{
    c = 3; 
    a = 2;
    [Associative]
    {
        c = -1;
        b = [ 2,3];
    }           
}
test = x;";
            string errmsg = "";//1467290 sprint25: rev 3731 : REGRESSION : Update with inline condition across multiple language blocks is not working as expected";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("x", new Object[] { 2, 3 });
        }


        [Test]
        public void T013_Defect_1467290_4()
        {
            string code =
@"c = 0;
x = 10;
a = [1,2];
x = c > 1 ? a : a +1;
[Imperative]
{
    c = 3; 
    a = [2,3];
    [Associative]
    {
        c = -1;
        a = [ 0,1];
    }           
}
test = x;";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("x", new Object[] { 2, 3 });
        }


        [Test]
        public void T014_InlineConditionContainUndefinedType()
        {
            string code = @"
x = C ? 1 : 0;
";
            string errmsg = "";//DNL-1467449 Rev 4596 : REGRESSION : Undefined variables in inline condition causes  Compiler Error";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("x", 0);
        }


        [Test]
        public void T014_InlineConditionContainUndefinedType_2()
        {
            string code = @"
def foo ()
{
    x = C ? 1 : 0;
    return  = x;
}
test = foo();
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", 0);
        }


        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T014_InlineConditionContainUndefinedType_3()
        {
            string code = @"

def foo ()
{
    x = C==1 ? 1 : 0;
    C = 1;
    return  = x;
}

test = foo();
";
            string errmsg = "1467449 - Rev 4596 : REGRESSION : Undefined variables in inline condition causes Compiler Error";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", 1);
        }


        [Test]
        public void T014_InlineConditionContainUndefinedType_4()
        {
            string code = @"
def foo ()
{
    x = C == 1? 1 : 0;
    C = 1;
    return  = x;
}
test = foo();
";
            string errmsg = "1467449 - Rev 4596 : REGRESSION : Undefined variables in inline condition causes Compiler Error";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", 1);
        }


        [Test]
        public void T014_InlineConditionContainUndefinedType_5()
        {
            string code = @"
x = C==1 ? 1 : 0;
C = 1;
";
            string errmsg = "1467449 - Rev 4596 : REGRESSION : Undefined variables in inline condition causes Compiler Error";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("x", 1);
        }


        [Test]
        public void T015_conditionequalto_1467482()
        {
            string code = @"
c = true;
d = c== true;
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("d", true);
            thisTest.VerifyRuntimeWarningCount(0);
        }


        [Test]
        public void T016_conditionnotequalto_1467482_2()
        {
            string code = @"
c = false;
d = c!= true;
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("d", true);
            thisTest.VerifyRuntimeWarningCount(0);
        }


        [Test]
        public void T017_conditionequalto_1467482_3()
        {
            string code = @"
d=[Imperative]
{
    c = true;
    d = c== true;
    return d;
}
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("d", true);
            thisTest.VerifyRuntimeWarningCount(0);
        }


        [Test]
        public void T018_conditionequalto_1467482_4()
        {
            string code = @"
            d=[Imperative]
            {
                c = false;
                d = c!= true;
                return d;
            }
            ";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("d", true);
            thisTest.VerifyRuntimeWarningCount(0);
        }


        [Test]
        [Category("Failure")]
        public void T018_conditionequalto_1467503_5()
        {
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1699
            string code = @"
                d;
                c = false;
                d = c!= null;
            
        ";
            string errmsg = "MAGN-1699 null comparison returns null, it should return true or false";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("d", true);
            thisTest.VerifyRuntimeWarningCount(0);
        }


        [Test]
        [Category("Failure")]
        public void T018_conditionequalto_1467503_6()
        {
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1699
            string code = @"
                d;
                c = false;
                d = c== null;
            
        ";
            string errmsg = "MAGN-1699 null comparison returns null, it should return true or false";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("d", false);
            thisTest.VerifyRuntimeWarningCount(0);
        }


        [Test]
        [Category("Failure")]
        public void T018_conditionequalto_1467503_7()
        {
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1699
            string code = @"
            d;
            [Imperative]
            {
                c = false;
                d = c== null;
            }
        ";
            string errmsg = "MAGN-1699 null comparison returns null, it should return true or false";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("d", false);
            thisTest.VerifyRuntimeWarningCount(0);
        }


        [Test]
        [Category("Failure")]
        public void T018_conditionequalto_1467503_8()
        {
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1699
            string code = @"
            d;
            [Imperative]
            {
                c = false;
                d = c!= null;
            }
        ";
            string errmsg = "MAGN-1699 null comparison returns null, it should return true or false";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("d", true);
            thisTest.VerifyRuntimeWarningCount(0);
        }


        [Test]
        public void T018_conditionequalto_1467403_9()
        {
            string code = @"
            d;
                c = null;
                d = c== null;
        ";
            string errmsg = "1467403 -in conditional it throws error could not decide which function to execute , for valid code a,d gives the correct result as well ";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("d", true);
            thisTest.VerifyRuntimeWarningCount(0);
        }


        [Test]
        public void T018_conditionequalto_1467403_10()
        {
            string code = @"
            d;
                c = null;
                d = c!= null;
        ";
            string errmsg = "1467403- in conditional it throws error could not decide which function to execute , for valid code a,d gives the correct result as well ";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("d", false);
            thisTest.VerifyRuntimeWarningCount(0);
        }


        [Test]
        public void T018_conditionequalto_1467403_11()
        {
            string code = @"
            d=[Imperative]
            {
                c = null;
                d = c== null;
                return d;
            }
        ";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("d", true);
            thisTest.VerifyRuntimeWarningCount(0);
        }


        [Test]
        public void T018_conditionequalto_1467403_12()
        {
            string code = @"
            d=[Imperative]
            {
                c = null;
                d = c!= null;
                return d;
            }
        ";
            string errmsg = "1467403 -in conditional it throws error could not decide which function to execute , for valid code a,d gives the correct result as well ";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("d", false);
            thisTest.VerifyRuntimeWarningCount(0);
        }


        [Test]
        public void T018_conditionequalto_1467403_13()
        {
            string code = @"
            e;
                c : int = 1;
                d : int[] = [ 1, 0 ];
                e = c == d;
        ";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("e", new object[] { true, false });
            thisTest.VerifyRuntimeWarningCount(0);
        }


        [Test]
        public void T018_conditionequalto_1467504_14()
        {
            string code = @"
            e=[Imperative]
            {
                c : int = 1;
                d : int[] = [ 1, 0 ];
                e = c == d;
                return e;
            }
        ";
            string errmsg = "1467504-conditonal - comparison with an single vs array returns null in imperative block ";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("e", false);
            thisTest.VerifyRuntimeWarningCount(0);
        }


        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestObjectEquality()
        {
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1692
            string code = @"
            import(""FFITarget.dll"");
            a = DummyPoint.ByCoordinates(1,2,3);
            b = DummyVector.ByCoordinates(4,5,6);
            c = a == b;
        ";
            thisTest.VerifyRunScriptSource(code);
            thisTest.Verify("c", false);
            thisTest.VerifyRuntimeWarningCount(0);
        }


        [Test]
        public void T020_1467442()
        {
            string code = @"
z = 1;
z = (z > 0) ? 1 : 2;
z = 3;
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("z", 3);
        }


        [Test]
        public void T021_1467442()
        {
            string code = @"
b = 0;
a = b;
z = 0;
z = z + ((a > 0) ? a : 0);
b = 5;
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("z", 5);
        }


        [Test]
        public void T021_1467442_2()
        {
            string code = @"
z = 0;
z = z + ((a > 0) ? a : 0);
a = 1;
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("z", 1);
        }


        [Test]
        public void T021_1467442_3()
        {
            string code = @"
def foo ()
{
    a;
    z =  (a > 0) ? a : 0;
    a = 1;
    return = z;
}
z = foo();
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("z", 1);
        }

        [Test]
        public void T021_1467442_5()
        {
            string code = @"
a = 1;
b = 1;
c = 1;
z = a > 0 ? (b > 1 ? 0 : 1 ) : ( c > 1 ? 2 : 3);
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("z", 1);
        }


        [Test]
        public void T021_1467442_6()
        {
            string code = @"
def foo ( x : double)
{
    return = 0;
}
def foo ( x : bool)
{
    return = 1;
}
a = 1;
z = foo ( a > 0 ? 1.4 : false  );
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("z", 0);
        }


        [Test]
        public void T021_1467442_7()
        {
            string code = @"
def foo ( x : double)
{
    return = 0;
}
def foo ( x : bool)
{
    return = 1;
}
a = [-1, 1];
z = foo ( a > 0 ? 1.4 : false  );
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("z", new Object[] { 1, 0 });
        }


        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T021_1467442_8()
        {
            string code = @"

def foo ( x : double)
{
    return = 0;
}
def foo ( x : bool)
{
    return = 1;
}

a = [-1, 1];
z = foo ( a > 0 ? 1.4 : false );
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("z", new Object[] { 1, 0 });
        }


        [Test]
        public void T021_1467442_9()
        {
            string code = @"
def foo ( x )
{
    return = x;
}
def foo2 ( x )
{
    return = x;
}
a = [-1, 1];
z = foo2 ( a > 0 ? foo(1) : foo(2)  );
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("z", new Object[] { 2, 1 });
        }


        [Test]
        public void T021_1467442_10()
        {
            string code = @"
def foo ( x )
{
    return = x;
}
def foo2 ( x )
{
    return = x;
}
a = [-1, 1];
z;
i=[Imperative]
{
    for ( i in 0..1 )
    {
        z[i] = foo2 ( a[i] > 0 ? foo(1) : foo(2)  );
    }
    return z;
}
";
            string errmsg = "";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("i", new Object[] { 2, 1 });
        }
    }
}
