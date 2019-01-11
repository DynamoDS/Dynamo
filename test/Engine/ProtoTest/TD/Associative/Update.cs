using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoFFI;
using ProtoScript.Runners;
using ProtoTestFx.TD;
namespace ProtoTest.TD.Associative
{
    class Update : ProtoTestBase
    {
        ProtoScript.Runners.DebugRunner fsr;
        string importPath = "..\\..\\..\\test\\Engine\\ProtoTest\\ImportFiles\\";

        public override void Setup()
        {
            base.Setup();
           
            fsr = new DebugRunner(core);
            DLLFFIHandler.Register(FFILanguage.CSharp, new CSModuleHelper());
            DLLFFIHandler.Register(FFILanguage.CPlusPlus, new ProtoFFI.PInvokeModuleHelper());
            CLRModuleType.ClearTypes();
        }

        public override void TearDown()
        {
            base.TearDown();
            fsr = null;
        }

        [Test]
        [Category("SmokeTest")]
        public void T001_Simple_Update()
        {
            string code = @"
a = 1;
b = a + 1;
a = 2;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification
            thisTest.Verify("a", 2, 0);
            thisTest.Verify("b", 3, 0);
        }

        [Test]
        [Category("Update")]
        public void T002_Update_Collection()
        {
            string code = @"
a = 0..4..1;
b = a;
c = b[2];
a = 10..14..1;
b[2] = b[2] + 1;
a[2] = a[2] + 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1459470 - Sprint17 : Rev 1459 : Associative Update not working properly with collection elements"); 
            //Verification
            thisTest.Verify("c", 14, 0);

        }

        [Test]
        [Category("SmokeTest")]
        public void T003_Update_In_Function_Call()
        {
            string code = @"
def foo1 ( a : int ) 
{
    return = a + 1;
}
def foo2 ( a : int[] ) 
{
    a[0] = a[1] + 1;
	return = a;
}
def foo3 ( a : int[] ) 
{
    b = a;
	b[0] = b[1] + 1;
	return = b;
}
a = 0..4..1;
b = a[0];
c = foo1(b);
d = foo2(a);
e1 = foo3(a);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification
            Object[] v0 = new Object[] { 0, 1, 2, 3, 4 };
            Object[] v1 = new Object[] { 2, 1, 2, 3, 4 };
            thisTest.Verify("a", v0, 0);
            thisTest.Verify("d", v1, 0);
            thisTest.Verify("e1", v1, 0);
        }

        [Test]
        [Category("Update")]
        public void T004_Update_In_Function_Call_2()
        {
            string errmsg = "";//1467302 - rev 3778 : invalid cyclic dependency detected";
            string src = @"
def foo1 ( a : int ) 
{
    return = a + 1;
}
def foo3 ( a : int[] ) 
{
    b = a;
	b[0] = b[1] + 1;
	return = b;
}
a = 0..4..1;
b = a[0];
c = foo1(b);
e1 = foo3(a);
a = 10..14..1;
a[1] = a[1] + 1;";
            thisTest.VerifyRunScriptSource(src, errmsg);
            //Assert.Fail("1459470 - Sprint17 : Rev 1459 : Associative Update not working properly with collection elements"); 

            //Verification
            Object[] v1 = new Object[] { 13, 12, 12, 13, 14 };
            Object[] v2 = new Object[] { 14, 13, 13, 14, 15 };
            Object[] v3 = new Object[] { 10, 12, 12, 13, 14 };
            thisTest.Verify("a", v3);
            thisTest.Verify("e1", v1);
            thisTest.Verify("b", 10);
            thisTest.Verify("c", 11);
        }

        [Test]
        [Category("Update")]
        public void T005_Update_In_collection()
        {
            string code = @"
a=1;
b=2;
c=4;
collection = [a,b,c];
collection[1] = collection[1] + 0.5;
d = collection[1];
d = d + 0.1;    // updates the result of accessing the collection - Redefinition
                // 'd' now only depends on any changes to 'd'
b = b + 0.1;    // updates the source member of the collection";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1459470 - Sprint17 : Rev 1459 : Associative Update not working properly with collection elements"); 

            //Verification
            Object[] v1 = new Object[] { 1, 2.6, 4 };
            thisTest.Verify("collection", v1, 0);
            thisTest.Verify("b", 2.1, 0);
            thisTest.Verify("d", 2.6, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T006_Update_In_Class()
        {
            string code = @"
import(""FFITarget.dll"");
startPt = DummyPoint.ByCoordinates(1, 1, 0);
endPt   = DummyPoint.ByCoordinates(1, 5, 0);
line_0  = DummyLine.ByStartPointEndPoint(startPt, endPt); 	// create line_0
startPt2 = [Imperative]
{
    x2 = 1..5..1;
	p2 = 0..0..#5;
	c2 = 0;
	for (i in x2 )
	{
	    p2[c2] = DummyPoint.ByCoordinates(i, 1, 0);		
		c2 = c2 + 1;
	}
	return = p2;
}
endPt2 = [Imperative]
{
    x2 = 11..15..1;
	p2 = 0..0..#5;
	c2 = 0;
	for (i in x2 )
	{
	    p2[c2] = DummyPoint.ByCoordinates(i, 5, 0);		
		c2 = c2 + 1;
	}
	return = p2;
}
line_0 = [Imperative]
{    
	p2 = 0..0..#25;
	c2 = 0;
	for (i in startPt2 )
	{
	    for ( j in endPt2 )
		{
		    p2[c2] = DummyLine.ByStartPointEndPoint(i, j);
			c2 = c2 + 1;
		}
			
	}
	return = p2;
}
x1_start = line_0[0].Start.X;
x1_end = line_0[0].End.X;
x5_start = line_0[4].Start.X;
x5_end = line_0[4].End.X;
line_0 = [Imperative]
{    
	p2 = 0..0..#5;
	c2 = 0;
	for (i in startPt2 )
	{
	    p2[c2] = DummyLine.ByStartPointEndPoint(startPt2[c2], endPt2[c2]);
		c2 = c2 + 1;
			
	}
	return = p2;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification         
            thisTest.Verify("x1_start", 1.0, 0);
            thisTest.Verify("x1_end", 11.0, 0);
            thisTest.Verify("x5_start", 5.0, 0);
            thisTest.Verify("x5_end", 15.0, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T008_Update_Of_Variables()
        {
            string code = @"
a = 1;
b = a + 1;
a = 2;
t1 = [ 1, 2 ];
t2 = t1 [0] + 1;
t1 = 5.5;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification
            thisTest.Verify("b", 3, 0);
            thisTest.Verify("t2", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void T009_Update_Of_Undefined_Variables()
        {
            string code = @"
u1 = u2;
u2 = 3;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification
            thisTest.Verify("u1", 3, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T010_Update_Of_Singleton_To_Collection()
        {
            string code = @"
s1 = 3;
s2 = s1 -1;
s1 = [ 3, 4 ] ;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification
            Object[] v1 = new Object[] { 2, 3 };
            thisTest.Verify("s2", v1, 0);
        }

        [Test]
        [Category("Update")]
        public void T011_Update_Of_Variable_To_Null()
        {
            string src = @"x = 1;
y = 2/x;
x = 0;
v1 = 2;
v2 = v1 * 3;
v1 = null;";
            thisTest.VerifyRunScriptSource(src);
            TestFrameWork.AssertInfinity("y");
            thisTest.Verify("v2", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void T012_Update_Of_Variables_To_Bool()
        {
            string code = @"
p1 = 1;
p2 = p1 * 2;
p1 = false;
q1 = -3.5;
q2 = q1 * 2;
q1 = true;
s1 = 1.0;
s2 = s1 * 2;
s1 = false;
t1 = -1;
t2 = t1 * 2;
t1 = true;
r1 = 1;
r2 = r1 * 2;
r1 = true;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification         
            thisTest.Verify("p2", null);
            thisTest.Verify("q2", null);
            thisTest.Verify("s2", null);
            thisTest.Verify("t2", null);
            thisTest.Verify("r2", null);

        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T013_Update_Of_Variables_To_User_Defined_Class()
        {
            string code = @"
import(""FFITarget.dll"");
r1 = 2.0;
r2 = r1+1;
r1 = TestObjectA.TestObjectA(5);
t1 = [ 1, 2 ];
t2 = t1 [0] + 1;
t1 = TestObjectA.TestObjectA(5);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification      
            thisTest.Verify("t2", null);
            thisTest.Verify("r2", null);

        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T014_Update_Of_Class_Properties()
        {
            string code = @"
import(""FFITarget.dll"");
x = 3;
a1 = TestObjectA.TestObjectA(x);
b1 = a1.a;
x = 4;
c1 = b1;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification      
            thisTest.Verify("c1", 4, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T015_Update_Of_Class_Properties()
        {
            string code = @"
import(""FFITarget.dll"");
x = [ 3, 4 ] ;
a1 = TestObjectA.TestObjectA(x);
b1 = a1.a;
x[0] = x [0] + 1;
c1 = b1;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification   
            Object[] v1 = new Object[] { 4, 4 };
            thisTest.Verify("c1", v1, 0);

        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T016_Update_Of_Variable_Types()
        {
            string code = @"
import(""FFITarget.dll"");
x = [ 3, 4 ] ;
y = x[0] + 1;
x =  [ 3.5, 4.5 ] ;
x =  [ TestObjectA.TestObjectA(1).a, TestObjectA.TestObjectA(2).a ] ;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification   
            thisTest.Verify("y", 2, 0);
        }

        [Test]
        [Category("Update")]
        public void T019_Update_General()
        {
            //Assert.Fail("1460139 - Sprint 18 : Rev 1580 : Update going into infinite loop for valid case"); 

            string code = @"
X = 1;
Y = X + 1;
X = X + 1;
X = X + 1;
//Y = X + 1;
//X  = X + 1;
test = X + Y;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification  
            thisTest.Verify("test", 7, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T022_Defect_1459905()
        {
            string code = @"
import(""FFITarget.dll"");
p = TestObjectA.TestObjectA(1);
x = p.a;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification   
            thisTest.Verify("x", 1, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T022_Defect_1459905_3()
        {
            string code = @"
import(""FFITarget.dll"");
def foo ( b1 : TestObjectA )
{
    return = b1.a;
}
b1 = TestObjectA.TestObjectA( 1 );
b1 = foo(b1); 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification   
            thisTest.Verify("b1", 1, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T022_Defect_1459905_4()
        {
            string code = @"
import(""FFITarget.dll"");
def foo ( b1 : TestObjectA )
{
    return = b1.a;
}
b1 = TestObjectA.TestObjectA( 1 );
x = b1.a;
b1 = TestObjectA.TestObjectA( 2 );
y = x;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification   
            thisTest.Verify("y", 2, 0);
        }


        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T023_Defect_1459789()
        {
            string code = @"
import(""FFITarget.dll"");
p = TestObjectA.TestObjectA(1.0);			        
x = p.a;
p = TestObjectA.TestObjectA(2.0);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification   
            thisTest.Verify("x", 2.0, 0);
        }

        [Test]
        [Category("Update")]
        public void T024_Defect_1459470()
        {
            string code = @"
a = 0..4..1;
b = a;
c = b[2];
a = 10..14..1;
b[2] = b[2] + 1;
a[2] = a[2] + 1;
x = a;
					        
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification   
            //Assert.Fail("1459470 - Sprint17 : Rev 1459 : Associative Update not working properly with collection elements"); 
            thisTest.Verify("c", 14);

        }

        [Test]
        [Category("Update")]
        public void T024_Defect_1459470_3()
        {
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4020
            string errmsg = "MAGN-4020 Update of global variables from inside function call is not happening as expected";
            string src = @"
a = 0..4..1;
b = a;
c = b[2];
a = 10..14..1;
b[2] = b[2] + 1;
a[2] = a[2] + 1;
";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(src, errmsg);
            //Verification   
            thisTest.Verify("c", 14);
        }

        [Test]
        [Category("Update")]
        public void T024_Defect_1459470_4()
        {
            string errmsg = "";//DNL-1467337 Rev 3971 : Update of global variables from inside function call is not happening as expectedr";
            string src = @"a = [1,2,3,4];
b = a;
c = b[2];
d = a[2];
a[0..1] = [1, 2];
b[2..3] = 5;
	
";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(src, errmsg);
            //Verification   
            thisTest.Verify("c", 5);
            thisTest.Verify("d", 3);
        }

        [Test]
        [Category("Update")]
        public void T025_Defect_1459704()
        {
            string err = "1467186 - sprint24 : REGRESSION: rev 3172 : Cyclic dependency detected in update cases";
            string src = @"a = b;
b = 3;
c = a;
					        
";
            thisTest.VerifyRunScriptSource(src, err);

            //Verification   
            thisTest.Verify("a", 3, 0);
            thisTest.Verify("c", 3, 0);
        }

        [Test]
        [Category("Update")]
        public void T029_Defect_1460139_Update_In_Class()
        {
            string code = @"
X = 1;
Y = X + 1;
X = X + 1;
X = X + 1; // this line causing the problem
test = X + Y;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1460139 - Sprint 18 : Rev 1580 : Update going into infinite loop for valid case");

            thisTest.Verify("test", 7);
        }

        [Test]
        [Category("Update")]
        public void T031_Defect_1467302()
        {
            String code =
@"
def foo ( a : int[] ) 
{
    b = a;
    b[0] = b[1] + 1;
    return = b;
}
a = [ 0, 1, 2];
e1 = foo(a);
a = [ 1, 2];
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("e1", new Object[] { 3, 2 });
        }

        [Test]
        [Category("Update")]
        public void T031_Defect_1467302_2()
        {
            String code =
@"
def foo ( a : int[] ) 
{
    b = a;
    b[0] = b[1] + 1;
    return = b;
}
i = 1..2;
a = [ 0, 1, 2, 3];
e1 = foo(a[i]);
i = 0..2;
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("e1", new Object[] { 2, 1, 2 });
        }

        [Test]
        [Category("Update")]
        public void T033_Defect_1467187_Update_In_class_collection_property_2()
        {
            String code =
@"
class B
{    
    a1 : int;
    a2 : double[];
    constructor B (a:int, b : double[])    
    {        
        a1 = a;
        a2 = b;
    }
}
b1 = B.B ( 1, [1.0, 2.0] );
test1 = b1.a2[0];
b1.a2[0] = b1.a2[1];
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test1", 2.0);
        }

        [Test]
        [Category("Update")]
        public void T035_FalseCyclicDependency()
        {
            string code = @"
def foo()
{
    a = b;
    return = null;
}
def bar()
{
    b = a;
    return = null;
}
a = 1;
b = 0;
r = bar();
q = a;
";
            string errmsg = "DNL-1467336 Rev 3971 :global and local scope identifiers of same name causing cyclic dependency issue";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("q", 1);
        }

        [Test]
        public void TestCyclicDependency01()
        {
            string code = @"
b = 1;
a = b + 1;
b = a;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.InvalidStaticCyclicDependency);
            Object n1 = null;
            thisTest.Verify("a", n1);
            thisTest.Verify("b", n1);
        }

        [Test]
        public void TestCyclicDependency02()
        {

            string code = @"


a1;
[Imperative]
{
	a = [];
	b = a;
	a[0] = b;
	c = Count(a);
}
[Associative]
{
	a1 = [0];
	b1 = a1;
	a1[0] = b1;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.InvalidStaticCyclicDependency);
            Object n1 = null;
            thisTest.Verify("a1", n1);
        }

        [Test]
        public void TestStringIndexing()
        {

            string code = @"

x = 1;
y = 2;
a = {""x"" : x, ""y"" : y};
x = 3;
y = 4;
r1 = a[""x""];
r2 = a[""y""];
";
            var mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 3);
            thisTest.Verify("r2", 4);
        }
    }
}
