using System;
using System.IO;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;
namespace ProtoTest.TD.MultiLangTests
{
    class TestUpdate : ProtoTestBase
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
        public void T02_Update_Function_Argument_Across_Language_Scope()
        {
            string code = @"
a = 1;
def foo ( a1 : double )
{
    return = a1 + 1;
}
b = foo ( c ) ;
c = a + 1;
a = [Imperative]
{
    return 2.5;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 2.5, 0);
            thisTest.Verify("c", 3.5, 0);
            thisTest.Verify("b", 4.5, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T03_Update_Function_Argument_Across_Language_Scope()
        {
            string code = @"
a = 1;
def foo ( a1 : int )
{
    return = a1 + 1;
}
b = foo ( c ) ;
c = a + 1;
a=[Imperative]
{
    return 2.5;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 2.5, 0);
            thisTest.Verify("c", 3.5, 0);
            thisTest.Verify("b", 5, 0);
        }


        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T04_Update_Class_Instance_Argument()
        {
            string code = @"
import(""FFITarget.dll"");
t1 = 1;
a1 = ClassFunctionality.ClassFunctionality(t1);
b1 = a1.OverloadedAdd(t1);
t1=[Imperative]
{
	return 2;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b1", 4, 0);
        }


        [Test]
        [Category("SmokeTest")]
        public void T07_Update_Array_Variable()
        {
            string code = @"
a = 1..3;
c = a;
b = [ Imperative ]
{
    count = 0;
	for ( i in a )
	{
	    a[count] = i + 1;
		count = count+1;
	}
	return = a;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v0 = new Object[] { 1, 2, 3 };
            Object[] v1 = new Object[] { 2, 3, 4 };
            thisTest.Verify("a", v0, 0);
            thisTest.Verify("b", v1, 0);
            thisTest.Verify("c", v0, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T08_Update_Array_Variable()
        {
            //Assert.Fail("1467194 - Sprint 25 - rev 3207[Regression] Regressions created by array copy constructions ");
            string code = @"
a = 1..3;
c = a;
a = [ Imperative ]
{
    count = 0;
	for ( i in a )
	{
	    if ( i > 0 )
		{
		    a[count] = i + 1;
		}
		count = count+1;
	}
	return = a;
}
d = [ Imperative ]
{
    count2 = 0;
	while (count2 <= 2 ) 
	{
	    if ( a[count2] > 0 )
		{
		    a[count2] = a[count2] + 1;
		}
		count2 = count2+1;
	}
	return = a;
}
e = c;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v2 = new Object[] { 3, 4, 5 };
            Object[] v1 = new Object[] { 2, 3, 4 };
            thisTest.Verify("a", v1, 0);
            thisTest.Verify("e", v1, 0);
            thisTest.Verify("c", v1, 0);
            thisTest.Verify("d", v2, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T09_Update_Across_Multiple_Imperative_Blocks()
        {
            string code = @"
a = 1;
b = a;
c = [ Imperative ]
{
    a = 2;
	return = a;
}
d = [ Imperative ]
{
    a = 3;
	return = a;
}
e = c;
f = d;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("d", 3, 0);
            thisTest.Verify("b", 1, 0);
            thisTest.Verify("e", 2, 0);
            thisTest.Verify("f", 3, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T10_Update_Array_Across_Multiple_Imperative_Blocks()
        {
            string code = @"
a = 1..3;
b = a;
c = [Imperative ]
{
    x = [ 10, a[1], a[2] ];
	a[0] = 10;
	return = x;
}
d = [ Imperative ]
{
    a[1] = 20;
	return = a;
}
e = c;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v0 = new Object[] { 1, 2, 3 };
            Object[] v1 = new Object[] { 1, 20, 3 };
            Object[] v2 = new Object[] { 10, 2, 3 };
            thisTest.Verify("d", v1, 0);
            thisTest.Verify("b", v0, 0);
            thisTest.Verify("e", v2, 0);
        }

        [Test]
        [Category("Update")]
        public void T11_Update_Undefined_Variables()
        {
            //Assert.Fail("1461388 - Sprint 19 : rev 1808 : Cross Language Update Issue : Inner Associative block should trigger update of outer associative block variable ");

            string code = @"
b = a;
[Imperative]
{
    a = 3;
}
[Associative]
{
    a = 4;	
}
c = b;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("c", 4, 0);
        }

        [Test]
        [Category("Update")]
        [Category("Failure")]
        public void T12_Update_Undefined_Variables()
        {
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1527
            string errmsg = "MAGN-1527: Cross Language Update Issue : Inner Associative block should trigger update of outer associative block variable ";
            string code = @"
b = a;
[Imperative]
{
    a = 3;
}
[Associative]
{
    a = 4;
    d = b + 1;	
}
c = b;";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);

            thisTest.Verify("d", 5, 0);
            thisTest.Verify("c", 4, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T13_Update_Variables_Across_Blocks()
        {
            string code = @"
a = 3;
b = a * 3;
c = [Imperative]
{
    d = b + 3;
	a = 4;
	return = d;
}
f = c + 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            // There is no cycle as 'a' is only modified to a different value once
            thisTest.Verify("b", 9, 0);
            thisTest.Verify("f", 13, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T14_Defect_1461209()
        {
            string code = @"
import(""FFITarget.dll"");
y = ClassFunctionality.ClassFunctionality( x);
a1 = y.IntVal;
x = 3;
x = 5;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1467186 - sprint24 : REGRESSION: rev 3172 : Cyclic dependency detected in update cases");
            thisTest.Verify("a1", 5.0, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T14_Defect_1461209_2()
        {
            string code = @"
import(""FFITarget.dll"");
y = ClassFunctionality.ClassFunctionality( x);
a1 = y.IntVal;
x = [Imperative]
{
    return = 5;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("a1", 5.0, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T15_Defect_1460935()
        {
            string code = @"
import(""FFITarget.dll"");
def foo ( b1 : ClassFunctionality )
{
    return = b1.IntVal;
}
b1 = ClassFunctionality.ClassFunctionality( 1 );
x = b1.IntVal;
b1 = 1;
y = x; 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object v1 = null;
            thisTest.Verify("x", v1, 0);
            thisTest.Verify("y", v1, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T15_Defect_1460935_2()
        {
            string code = @"
import(""FFITarget.dll"");
def foo ( b1 : ClassFunctionality )
{
    return = b1.IntVal;
}
b1 = ClassFunctionality.ClassFunctionality( 1 );
x = b1.IntVal;
b1 = 2;
y = x; // expected : null; recieved : exception
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object v1 = null;
            thisTest.Verify("x", v1, 0);
            thisTest.Verify("y", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T15_Defect_1460935_3()
        {
            string code = @"
x = 1;
y = x;
x = true; //if x = false, the update mechanism works fine
yy = y;
x = false;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("yy", false, 0);

        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T15_Defect_1460935_4()
        {
            string code = @"
import(""FFITarget.dll"");
def foo ( b1 : ClassFunctionality )
{
    return = b1.IntVal;
}
b1 = ClassFunctionality.ClassFunctionality( 1 );
x = b1.IntVal;
y = foo ( b1 );
b1 = [Imperative]
{
	return ClassFunctionality.ClassFunctionality( 2 );	
}
b2 = ClassFunctionality.ClassFunctionality( 2 );
x2 = b2.IntVal;
y2 = foo ( b2 );
b2 = [Imperative]
{
	return null;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object v1 = null;
            thisTest.Verify("x", 2, 0);
            thisTest.Verify("y", 2, 0);
            thisTest.Verify("x2", v1, 0);
            thisTest.Verify("y2", v1, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Replication")]

        public void T15_Defect_1460935_5()
        {
            string code = @"
import(""FFITarget.dll"");
b3 = ClassFunctionality.ClassFunctionality( 2 );
x3 = b3.IntVal;
b3 = [Imperative]
{
	return [ ClassFunctionality.ClassFunctionality( 1 ), ClassFunctionality.ClassFunctionality( 2 ) ] ;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 1, 2 };
            thisTest.Verify("x3", v1, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T15_Defect_1460935_6()
        {
            string code = @"
import(""FFITarget.dll"");
def foo ( b : ClassFunctionality )
{
    return = [ b.IntVal, b.IntVal + 1 ];
}
b1 = ClassFunctionality.ClassFunctionality( 2 );
x1 = b1.IntVal;
f1 = foo ( b1);
b1 = null;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object v1 = null;
            object[] f1 = { null, null };
            thisTest.Verify("x1", v1, 0);
            thisTest.Verify("f1", f1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T16_Defect_1460623()
        {
            string code = @"
a2 = 1.0;
test2 = a2;
a2 = 3.0;
a2 = 3.3;
t2 = test2; // expected : 3.3; recieved : 3.0
a1 = [ 1.0, 2.0];
test1 = a1[1]; 
a1[1] = 3.0;
a1[1] = 3.3;
t1 = test1; // expected : 3.3; recieved : 3.0
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t1", 3.3, 0);
            thisTest.Verify("t2", 3.3, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T16_Defect_1460623_2()
        {
            string code = @"
def foo ( a )
{
    return = a;
}
x = 1;
y = foo (x );
x = 2;
x = 3;
x = [Imperative]
{
    return 4;
}
z = x;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 4, 0);
            thisTest.Verify("y", 4, 0);
            thisTest.Verify("z", 4, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T16_Defect_1460623_3()
        {
            string code = @"
def foo ( a )
{
    x = a;
	y = x + 3;
	x = a + 1;
	x = a + 2;
	return = y;
}
x = 1;
y = foo (x );
x = [Imperative]
{
    return 2;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("y", 7, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T17_Defect_1459759()
        {
            string code = @"
import(""FFITarget.dll"");
p1 = 1;
p2 = p1 * 2;
p1 = true;
x1 = 3;
y1 = x1 + 1;
x1 = ClassFunctionality.ClassFunctionality();
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object v1 = null;
            thisTest.Verify("p2", v1, 0);
            thisTest.Verify("y1", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T17_Defect_1459759_2()
        {
            string code = @"
a1 = [ 1, 2 ];
y = a1[1] + 1;
a1[1] = 3;
a1 = 5;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object v1 = null;
            thisTest.Verify("y", v1, 0);
        }
        /*
        
[Test]
        [Category ("SmokeTest")]
 public void T18_Update_Variables_In_Inner_Assoc()
        {
            string src = string.Format("{0}{1}", testPath, "T18_Update_Variables_In_Inner_Assoc.ds");
            fsr.LoadAndPreStart(src);
            ProtoCore.CodeModel.CodePoint cp = new ProtoCore.CodeModel.CodePoint
            {
                CharNo = 8,
                LineNo = 9,
                SourceLocation = new ProtoCore.CodeModel.CodeFile
                {
                    FilePath = Path.GetFullPath(src)
                }
            };
            fsr.ToggleBreakpoint(cp);
            cp = new ProtoCore.CodeModel.CodePoint
            {
                CharNo = 9,
                LineNo = 13,
                SourceLocation = new ProtoCore.CodeModel.CodeFile
                {
                    FilePath = Path.GetFullPath(src)
                }
            };
            fsr.ToggleBreakpoint(cp);
            cp = new ProtoCore.CodeModel.CodePoint
            {
                CharNo = 9,
                LineNo = 14,
                SourceLocation = new ProtoCore.CodeModel.CodeFile
                {
                    FilePath = Path.GetFullPath(src)
                }
            };
            fsr.ToggleBreakpoint(cp);
            ProtoScript.Runners.DebugRunner.VMState vms = fsr.Run();
            thisTest.DebugModeVerification(vms.mirror, "c", 1);
            thisTest.DebugModeVerification(vms.mirror, "b", 2);
            thisTest.DebugModeVerification(vms.mirror, "x", 4);
            thisTest.DebugModeVerification(vms.mirror, "d", 3);
            fsr.Run();
            thisTest.DebugModeVerification(vms.mirror, "c", 4);
            thisTest.DebugModeVerification(vms.mirror, "b", 3);
            thisTest.DebugModeVerification(vms.mirror, "x", 4);
            thisTest.DebugModeVerification(vms.mirror, "d", 6);
            fsr.Run();
            thisTest.DebugModeVerification(vms.mirror, "c", 4);
            thisTest.DebugModeVerification(vms.mirror, "b", 8);
            thisTest.DebugModeVerification(vms.mirror, "x", 8);
            thisTest.DebugModeVerification(vms.mirror, "d", 6);
            fsr.Run();
            thisTest.DebugModeVerification(vms.mirror, "c", 4);
            thisTest.DebugModeVerification(vms.mirror, "b", 7);
            thisTest.DebugModeVerification(vms.mirror, "x", 7);
            thisTest.DebugModeVerification(vms.mirror, "d", 6);
        }
        
        */
        [Test]
        [Category("SmokeTest")]
        [Category("Cyclic")]
        public void T18_Update_Variables_In_Inner_Assoc()
        {
            string code = @"
c = 2;
b = c * 2;
x = b;
d;
i=[Imperative]
{
    c = 1;
	b = c + 1;
	d = b + 1;
	y = 1;
	[Associative]
	{
	  	b = c + 2;		
		c = 4;
		z = 1;
	}
    return [b,c,d,y,z];
}
b = i[0];
c = i[1];
d = i[2];
y = i[3];
z = i[4];
b = c + 3;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", 4);
            thisTest.Verify("b", 7);
            thisTest.Verify("x", 7);
            thisTest.Verify("d", 3);

        }

        [Test]
        [Category("SmokeTest")]
        public void T20_Defect_1461391()
        {
            string code = @"
a = 1;
def foo ( a1 : double )
{
    return = a1 + 1;
}
b = foo ( c ) ;
c = a + 1;
a = [Imperative]
{
    return 2.5;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", 3.5);
            thisTest.Verify("b", 4.5);
        }

        [Test]
        [Category("SmokeTest")]
        public void T20_Defect_1461391_2()
        {
            string code = @"
a = 1;
def foo ( a1 : double[] )
{
    return = a1[0] + a1[1];
}
b = foo ( c ) ;
c = [ a, a ];
a = [Imperative]
{
    return 2.5;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", 5.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T20_Defect_1461391_3()
        {
            string code = @"
a = 1;
def foo ( a1 : double )
{
    return = a1 + 1;
}
b = foo ( a ) ;
a = [Imperative]
{
   return foo(2);
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", 4.0);

        }

        [Test]
        [Category("SmokeTest")]
        public void T20_Defect_1461391_6()
        {
            string code = @"
def foo ( a : int) 
{
    return = a;
}
y1 = [ 1, 2 ];
y2 = foo ( y1);
y1=[Imperative]
{ 
	count = 0;
	for ( i in y1)
	{
	    y1[count] = y1[count] + 1;	
        count = count + 1;		
	}
    return y1;
}
t1 = y2[0];
t2 = y2[1];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t1", 2);
            thisTest.Verify("t2", 3);
        }

        [Test]
        [Category("SmokeTest")]
        public void T21_Defect_1461390()
        {
            string code = @"
c;
[Associative]
{
    a = 0;
    d = a + 1;
    a = [Imperative]
    {
       b = 2 + a;
       return 1.5;
              
    }
    c = a + 2; // fail : runtime assertion 
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", 3.5);
        }


        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T23_Update_Class_Instance_Using_Set_Method()
        {
            string error = "1467385 Sprint 27 - rev 4219 - valid update testcase throws cyclic dependancy error ";
            string code = @"
import(""FFITarget.dll"");
a1 = ClassFunctionality.ClassFunctionality();
a1.IntVal = 1;
b = a1.IntVal;
a1.IntVal = 2;
";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("b", 2);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T23_Update_Class_Instance_Using_Set_Method_2()
        {
            string code = @"
import(""FFITarget.dll"");
a1 = ClassFunctionality.ClassFunctionality();
a1.IntVal = 1;
b = a1.IntVal;
a1.IntVal = 2;
";
            thisTest.VerifyRunScriptSource(code);
            thisTest.Verify("b", 2);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T23_Update_Class_Instance_Using_Set_Method_3()
        {
            string code = @"
import(""FFITarget.dll"");
a1 = ClassFunctionality.ClassFunctionality();
a1.IntVal = 1;
b = a1.IntVal;
a1.IntVal = null;
";
            thisTest.VerifyRunScriptSource(code);
            Object n1 = null;
            thisTest.Verify("b", 1); // int cannot be set to null
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T23_Update_Class_Instance_Using_Set_Method_4()
        {
            string error = "1467385 Sprint 27 - rev 4219 - valid update testcase throws cyclic dependancy error ";
            string code = @"
import(""FFITarget.dll"");
a1 = ClassFunctionality.ClassFunctionality();
a1.IntVal = 1;
b = a1.IntVal;
a1.IntVal = 3.5;
";
            thisTest.VerifyRunScriptSource(code, error);

            thisTest.Verify("b", 4);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T23_Update_Class_Instance_Using_Set_Method_5()
        {
            string code = @"
import(""FFITarget.dll"");
a1 = ClassFunctionality.ClassFunctionality();
a1.IntVal = 1;
b = a1.IntVal;
a1.IntVal = true; // This is not accepted as IntVal is an 'int' type
";
            thisTest.VerifyRunScriptSource(code);
            thisTest.Verify("b", 1);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T23_Update_Class_Instance_Using_Set_Method_6()
        {
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1553
            string errmsg = "MAGN-1553: [Design Issue] update of instance , between property update and by method what ist he expected answer ";//1467187 - Sprint24: REGRESSION : rev 3177: When a class collection property is updated, the value if not reflected";
            string code = @"
import(""FFITarget.dll"");
def foo ( x1 : ClassFunctionality)
{
    x1.IntVal = -1;
    return = x1;
}
a1 = ClassFunctionality.ClassFunctionality();
a1.IntVal = 10; // Having this line means not testing the property modification in foo. This is because this line will get re-executed as a1.a is modified in foo
b = a1.IntVal;
a1 = foo ( a1);
";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            thisTest.Verify("b", 10);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T23_Update_Class_Instance_Using_Set_Method_7()
        {
            string errmsg = "";
            string code = @"
import(""FFITarget.dll"");
a1 = ClassFunctionality.ClassFunctionality();
b = a1.IntVal;
a1.IntVal = -1;
";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            thisTest.Verify("b", -1);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T24_Update_Variable_Type()
        {
            //Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            //{
            //Assert.Fail("1463327 - Sprint 20 : Rev 2086 : Update issue : When a variable is updated to a different type using itself DS is throwing System.NullReference exception");

            string code = @"
import(""FFITarget.dll"");
c = 1.0;
c = ClassFunctionality.ClassFunctionality( c );
x = c.IntVal;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 1.0);

            //});
        }

        [Test]
        [Category("SmokeTest")]
        public void T25_Defect_1459759()
        {
            string code = @"
p1 = 2;
p2 = p1+2;
p1 = true;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("p2", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void T25_Defect_1459759_2()
        {
            string code = @"
a1 = [ 1, 2 ];
y = a1[1] + 1;
a1[1] = 3;
a1 = 5;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("y", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void T25_Defect_1459759_3()
        {
            string code = @"
a = [ 2 , b ,3 ];
b = 3;
c = a[1] + 2;
d = c + 1;
b = [ 1,2 ];";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v3 = new Object[] { 3, 4 };
            Object[] v4 = new Object[] { 4, 5 };
            thisTest.Verify("c", v3);
            thisTest.Verify("d", v4);
        }

        [Test]
        [Category("SmokeTest")]
        public void T25_Defect_1459759_4()
        {
            string code = @"
p2;
[Imperative]
{
	[Associative]
	{
		p1 = 2;
		p2 = p1+2;
		p1 = true;
	}
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("p2", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void T25_Defect_1459759_5()
        {
            string code = @"
a;
[Imperative]
{
	a = 2;
	x = [Associative]
	{
		b = [ 2, 3 ];
		c = b[1] + 1;
		b = 2;
		return = c;
	}
	a = x;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void T25_Defect_1459759_6()
        {
            string code = @"
	def foo ( a, b )
	{
		a = b + 1;
		b = true;
		return = [ a , b ];
	}
e = [Imperative]
{
	c = 3;
	d = 4;
	return foo( c , d );
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v = new Object[] { null, true };
            thisTest.Verify("e", v);
        }

        [Test]
        public void T28_Update_With_Inline_Condition()
        {
            string code = @"
x = 3;
a1 = 1;
a2 = 2;
a = x > 2 ? a1: a2;
a1 = 3;
a2 = 4;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1467191 - Sprint24: rev 3185 : REGRESSION: update on inline condition is not happening as expected");

            thisTest.Verify("a", 3);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T30_Update_Global_Variables_Class()
        {
            string code = @"
import(""FFITarget.dll"");
y = ClassFunctionality.ClassFunctionality();
z = y.StaticProp;
y.StaticProp = 2;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("z", 2);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T30_Update_Global_Variables_Function()
        {
            string code = @"
import(""FFITarget.dll"");
IntVal = 1;
y = ClassFunctionality.ClassFunctionality(2);
z1 = y.IntVal;
z3 = IntVal;
z4 = y.IntVal;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("z1", 2);
            thisTest.Verify("z3", 1);
            thisTest.Verify("z4", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T30_Update_Global_Variables_Imperative_Scope()
        {
            string code = @"
x  = [0,0,0,0];
count = 0;
i = 0;
sum  = 0;
imp = [Imperative]
{
    for  ( i in x ) 
    {
       x[count] = count;
       count = count + 1;       
    }
    j = 0;
    while ( j < count )
    {
        sum = x[j]+ sum;
        j = j + 1;
    }
    return [x, sum];
}
y = imp[0];
test = imp[1];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 0, 1, 2, 3 };

            thisTest.Verify("test", 6);
            thisTest.Verify("y", v1);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T31_Defect_1459777()
        {
            string code = @"
import(""FFITarget.dll"");
class A 
{
    a : var;
	constructor A ( x)
	{
	    a = x;
	}
}
x = 3;
a1 = ClassFunctionality.ClassFunctionality(x);
b1 = a1.IntVal;
x = 4;
c1 = b1;
// expected : c1 = 4;
// recieved : c1 = 3
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("c1", 4);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T31_Defect_1459777_2()
        {
            string err = "1467385 Sprint 27 - rev 4219 - valid update testcase throws cyclic dependancy error";
            string code = @"
import(""FFITarget.dll"");
a1 = ClassFunctionality.ClassFunctionality();
a1.IntVal = 1;
b = a1.IntVal;
a1.IntVal = 2; // expected b = 2; received : b = 1;
";
            thisTest.VerifyRunScriptSource(code, err);
            thisTest.Verify("b", 2);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Update")]
        public void T31_Defect_1459777_3()
        {
            string code = @"
import(""FFITarget.dll"");
a1 = ClassFunctionality.ClassFunctionality();
x = a1.IntVal;
c = [Imperative]
{
    a1.IntVal = 1;
    b = a1.IntVal;
    a1.IntVal = 3; 
	return = b;
}
";
            thisTest.RunAndVerifySemanticError(code, ProtoImperative.Properties.Resources.ImperativeSymbolsAreReadOnly);

        }

        [Test]
        [Category("Update")]
        public void T31_Defect_1459777_4()
        {
            string err = "1467385: Sprint 27 - rev 4219 - valid update testcase throws cyclic dependancy error ";
            string code = @"class A
{
    a : int;	
}
class B extends A 
{
    b : var; 
	
}
def foo ( x )
{
    x.b = 2;
	return = true;
}
y = B.B();
y.b = 1; // This is re-executed again as y.b is modified from inside foo

z = y.b;
test = foo ( y ) ;
z2 = z;
";
            thisTest.VerifyRunScriptSource(code, err);
            thisTest.Verify("z2", 1);

        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Update")]
        public void T31_Defect_1459777_7()
        {
            //Assert.Fail("1466085 - Sprint 22 : rev 2396 : Update issue : update not working with range expressions ");

            string code = @"
import(""FFITarget.dll"");
x = ClassFunctionality.ClassFunctionality();
y = x.IntVal;
y1 = 0..y;
y2 = y > 1 ? true : false;
y3 = 1 < x.IntVal ? true : false;
x.IntVal = 2;
z1 = y1;
z2 = y2;
z3 = y3;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 0, 1, 2 };

            thisTest.Verify("z1", v1);
            thisTest.Verify("z2", true);
            thisTest.Verify("z3", true);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T31_Defect_1459777_8()
        {
            string code = @"
import(""FFITarget.dll"");
def foo ( x ) 
{
    return  = x + 1;
}
x1 = ClassFunctionality.ClassFunctionality();
y1 = foo( x1.IntVal );
x1.IntVal = 2;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("y1", 3);

        }

        [Test, Category("Failure")]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T31_Defect_1459777_9()
        {
            // TODO pratapa: Regression in dotcall assignment post Dictionary changes
            string code = @"
import(""FFITarget.dll"");
def foo ( x ) 
{
    return  = x + 1;
}
x1 =  [ ClassFunctionality.ClassFunctionality(), ClassFunctionality.ClassFunctionality() ];
a1 = ClassFunctionality.ClassFunctionality();
x2 =  [ a1.IntVal, a1.IntVal ];
y2 = foo ( x2[0] );
y1 = foo ( x1[1].IntVal );
x1[1].IntVal = 2;
a1.IntVal = 2; 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1466107 - Sprint 22 : rev 2396 : Update issue with multiple updates involving instances and collections");
            thisTest.Verify("y1", 3);
            thisTest.Verify("y2", 3);
        }

        [Test]
        [Category("Update")]
        public void T32_Update_With_Range_Expr()
        {
            string code = @"
y = 1;
y1 = 0..y;
y = 2;
z1 = y1;                             
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 0, 1, 2 };
            //Assert.Fail("1466085 - Sprint 22 : rev 2396 : Update issue : update not working with range expressions ");
            thisTest.Verify("z1", v1);

        }

        [Test, Category("Failure")]
        [Category("SmokeTest")]
        public void T33_Defect_1466107()
        {
            // TODO pratapa: Regression in dotcall assignment after Dictionary changes
            string code = @"
class B
{
    b : int;	
}
class A extends B
{
    a : int;	
}
def foo ( x ) 
{
    return  = x + 1;
}
x1 =  [ A.A(), A.A() ];
a1 = A.A();
x2 =  [ a1.a, a1.a ];
y2 = foo ( x2[0] );
y1 = foo ( x1[1].b );
x1[1].b = 2;
a1.a = 2; 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("y1", 3);
            thisTest.Verify("y2", 3);

        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T34_Defect_DNL_1463327()
        {
            string code = @"
import(""FFITarget.dll"");
c = 1.0;
x = [Imperative]
{
	c = ClassFunctionality.ClassFunctionality( c );
	x = c.IntVal;
	return = x;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 1.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T34_Defect_DNL_1463327_2()
        {
            string code = @"
class A
{        
    Pt : double[];        
    constructor A (pt : double[])            
    {                        
        Pt = pt;            
    }
}
c = 0.0..3.0;
y = c[0];
x = [Imperative]
{
	c = A.A( [c[0], c[0]] );
	x = c.Pt;
	return = x;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 0.0, 0.0 };

            thisTest.Verify("x", v1);
            thisTest.Verify("y", 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T34_Defect_DNL_1463327_3()
        {
            string error = "1467416 Count returns null if the input argument is single value ";
            string code = @"
import(""FFITarget.dll"");
t = 0..3;
c = ClassFunctionality.ClassFunctionality ( t );
c1 = Count ( c );
x = [Imperative]
{
	c = ClassFunctionality.ClassFunctionality( c[0].IntVal );
	x = c.IntVal;
	return = x;
}
t = 0..2;
";
            thisTest.VerifyRunScriptSource(code, error);

            thisTest.Verify("x", 0.0);
            thisTest.Verify("c1", 3);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Update")]
        public void T34_Defect_DNL_1463327_4()
        {
            string code = @"
import(""FFITarget.dll"");
t = 0..3;
c = ClassFunctionality.ClassFunctionality ( t );
c = ClassFunctionality.ClassFunctionality ( c[0].IntVal );
c = ClassFunctionality.ClassFunctionality ( c.IntVal );
x = c.IntVal;
t = 0..1;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T35_Defect_DNL_1463700()
        {
            string errmsg = " 1467318 - Cannot return an array from a function whose return type is var with undefined rank (-2) ";
            string code = @"     
def foo()        
{   x = [1,2,3];          
    x[0] = 100;        
    return = x;
}
a = foo();
";
            thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] v1 = new Object[] { 100, 2, 3 };
            thisTest.Verify("a", v1);
        }

        [Test]
        [Category("Update")]
        public void T37_Modify_Collections_Referencing_Each_Other()
        {
            string code = @"
a = [1,2,3];
b = a;
c1 = a[0];
b[0] = 10;
c2 = a[0];
testArray = a;
testArrayMember1 = c1;
testArrayMember2 = c2;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1467086 - Sprint23 : rev 2702 : Assocciative update not working properly for collections referencing each other");
            thisTest.Verify("testArrayMember2", 1);
            thisTest.Verify("testArrayMember1", 1);
        }

        [Test]
        [Category("Update")]
        public void T41_Defect_1467072_Class_Update()
        {
            string code = @"
class A 
{
    a : int;
    constructor A ( a1:int)
    {
        a = b + 1;
        b = a1;
    }
}
def foo ( a1 : int)
{
    a = b + 1;
    b = a1;
    return  = a ;
}
ga = gb + 1;
gb = gf;
gc = foo (ga);
gd = A.A(gc);
e1 = gd.a;
gf = 2;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.SetErrorMessage("1467259 - Sprint25:rev 3541 : Issue with class property update inside constructor for undefined variables");
            thisTest.Verify("e1", 5);

        }

        [Test]
        [Category("Update")]
        public void T42_Update_Using_Variables()
        {
            string error = "1467261 Sprint25: rev 3543: False Cyclic dependency error message for valid code";

            String code =
@"a = 2;
b = 2 * a;
c = a + b;
a = 4;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, error);
            thisTest.Verify("c", 12);
        }

        [Test]
        [Category("Update")]
        public void T43_Defect_1463498()
        {
            string code = @"
c;d;
def foo : int ( a : int, b : int )
{
	a = a + b;
	b = 2 * b;
	return = a + b;
}
[Associative]
{
    a = 1;
    b = 2;
    c = foo (a, b ); // expected 9, received -3
    d = a + b;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", 9);
            thisTest.Verify("d", 3);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Update")]
        public void T44_Defect_1457029()
        {
            string code = @"
import(""FFITarget.dll"");
c1 = [ [ 1,2], 3];
c1 = ClassFunctionality.ClassFunctionality( c1[0] );
x = c1.IntVal;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", new Object[] { 1, 2 });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Update")]
        public void T44_Defect_1457029_2()
        {
            string code = @"
import(""FFITarget.dll""); 
c1 = [ [ 1.0, 2.0], 3.0 ];
c1 = ClassFunctionality.ClassFunctionality( c1[0][0] );
x = c1.IntVal;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 1.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T45_Defect_1467191_Update_Inline_Condition()
        {
            String code =
 @"x = 3;
a1 = [ 1, 2];
a2 = 3;
a = x > 2 ? a2: a1;
a2 = 5;
x = 1;
a1[0] = 0;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new Object[] { 0, 2 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T45_Defect_1467191_Update_Inline_Condition_2()
        {
            String code =
 @"
def foo ( x1 )
{
    return = x1 + 1;
}
x = 3;
a1 = [ 1, 2];
a2 = 3;
a = x > 2 ? foo(a2): foo(a1);
a2 = 5;
x = 1;
a1[0] = 0;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new Object[] { 1, 3 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T45_Defect_1467191_Update_Inline_Condition_3()
        {
            String code =
 @"
def foo ( x, a2, a1:int[] )
{
   y = x > 2 ? a2: a1;
   return = y;
}
a1 = [ 1, 2 ];
a2 = 5;
x = 3;
a = foo ( x, a2, a1 );
x = 1;
a1[0] = 0;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new Object[] { 0, 2 });
        }

        [Test]
        [Category("Update")]
        public void T46_Defect_1467245()
        {
            String code =
 @"
A = 10;              // assignment of single literal value
B = 2*A;              // expression involving previously defined variables
A = A + 1;            // expressions modifying an existing variable;
A = 15;               // redefine A, removing modifier
A = [1,2,3,4];         // redefine A as a collection
A = 1..10..2;          // redefine A as a range expression (start..end..inc)
A = 1..10..~4;         // redefine A as a range expression (start..end..approx_inc)
A = 1..10..#4;         // redefine A as a range expression (start..end..no_of_incs)
A = A + 1;         // modifying A as a range expression
A = 1..10..2;  
";
            string errmsg = "";// "DNL-1462143 Sprint 19 : Rev 1912 : Update Design Issue : Redefinition of a variable should ensure there are no more updates on changes in RHS of older definitions";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("B", new Object[] { 2, 6, 10, 14, 18 }); // prev language
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Update")]
        public void T46_Defect_1467245_2()
        {
            String code =
 @"
import(""FFITarget.dll"");
t = 0..3;
c = ClassFunctionality.ClassFunctionality ( t );
x = c.IntVal;
c = ClassFunctionality.ClassFunctionality ( c[0].IntVal );
c = ClassFunctionality.ClassFunctionality ( c.IntVal ); 
";
            string errmsg = "";// "DNL-1462143 Sprint 19 : Rev 1912 : Update Design Issue : Redefinition of a variable should ensure there are no more updates on changes in RHS of older definitions";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("x", 0.0);
        }

        [Test]
        [Category("Update")]
        public void T46_Defect_1467275()
        {
            String code =
 @"
a = [0,1,2];
t = [10,11,12];
a[0] = t[0];
t[1] = a[1]; 
";
            thisTest.RunAndVerifyBuildWarning(code, ProtoCore.BuildData.WarningID.InvalidStaticCyclicDependency);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Update")]
        public void T46_Defect_1467275_2()
        {
            String code =
 @"
a = [0,1,2];
t = [10,11,12];
a[0] = t[0];
t[1] = a[1]; 
";
            thisTest.RunAndVerifyBuildWarning(code, ProtoCore.BuildData.WarningID.InvalidStaticCyclicDependency);
        }

        [Test]
        [Category("SmokeTest")]
        public void T47_Defect_1467092()
        {
            String code =
 @"
def foo ( x )
{
    return = x + 1;
}
x = [1,2,3];
b = 0;
y1 = x[b];
y2 = x[foo(b)];
b = 1;
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("y1", 2);
            thisTest.Verify("y2", 3);
        }

        [Test]
        [Category("SmokeTest")]
        public void T48_Defect_1467276()
        {
            String code =
 @"
a = [10,11,12];
t = 0;
i = a[t];
t = 2; // expected i = 12
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("i", 12);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T49_Defect_1461985()
        {
            String code =
 @"
import(""FFITarget.dll"");
a = ClassFunctionality.ClassFunctionality();
b1 = a.IntVal;
c = [Imperative]
{
    a.IntVal = 4;
    return = a.IntVal;
} 
";
            thisTest.RunAndVerifySemanticError(code, ProtoImperative.Properties.Resources.ImperativeSymbolsAreReadOnly);
        }

        [Test]
        [Category("SmokeTest")]
        public void T49_Defect_1461985_2()
        {
            String code =
 @"
totalLength = 0;
[Imperative]
{
                while (totalLength < 20) // create a simple outer loop
                {
                                [Associative] // within that loop build an associative model
                                {
                                                totalLength  = totalLength + 1;
                                }                              
                }
}
";
            // Assert.Fail("Suspected of crashing NUnit");
            string errmsg = "";//1461985 - Sprint 19 : Rev 1880 : Cross Language Update issue : When class properties are updated in different blocks the update is not happening as expected";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("totalLength", 20);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T49_Defect_1461985_3()
        {
            String code =
 @"
import(""FFITarget.dll"");
a = ClassFunctionality.ClassFunctionality(0);
totalLength = a.IntVal;
i = 0;
[Imperative]
{
    while (a.IntVal < 5) 
    {
        [Associative] 
        {
            a.IntVal = a.IntVal + 1;
            i = i + 1;
        }                              
    }
}
";
            //Assert.Fail("Suspected of crashing NUnit");
            string errmsg = "";//1461985 - Sprint 19 : Rev 1880 : Cross Language Update issue : When class properties are updated in different blocks the update is not happening as expected";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("totalLength", 5);

        }

        [Test]
        [Category("DSDefinedClass_Ported"), Category("Failure")]
        [Category("SmokeTest")]
        public void T49_Defect_1461985_4()
        {
            String code =
 @"
totalLength = 0;
i = 4;
b = 0;
[Imperative]
{
	while (totalLength < 3) // create a simple outer loop
	{
		[Associative] // within that loop build an associative model
		{
			a = 0..i;
			a = [Imperative] // within the associative model start some imperative scripting
			{
				for (j in a) 
				{
					if(j%2==0) 
					{
						a[j] = 0;
					}
				}
                return a;
			}		
			b = a + 1;
			totalLength  = totalLength +1;
		}
		i = i + 1; 
	}
    return [b, i, totalLength];
}
";
            //Assert.Fail("Suspected of crashing NUnit");
            string errmsg = "";//1461985 - Sprint 19 : Rev 1880 : Cross Language Update issue : When class properties are updated in different blocks the update is not happening as expected";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", new Object[] { 1, 2, 1, 4, 1, 6, 1 });
            thisTest.Verify("i", 7);
            thisTest.Verify("totalLength", 3);

        }

        [Test]
        [Category("SmokeTest")]
        public void T50_Defect_1466076()
        {
            String code =
 @"
class A
{
    a : int; 
}
class B extends A 
{
    b : var; 
}
def foo ( x )
{
    x.b = 2;
    return = true;
}
y = B.B();
y.b = 1;
z = y.b;
test = foo ( y ) ;
z2 = z; 
";
            string errmsg = "1467385 - Sprint 27 - rev 4219 - valid update testcase throws cyclic dependancy error ";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("z2", 1);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T52_Defect_1459478()
        {
            String code =
@"
import(""FFITarget.dll"");
def foo (x1:var[])
{
    return = x1 + 1;
}   
a = 0..4..1;
x = foo(a);
";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code);
            thisTest.Verify("x", new Object[] { 1, 2, 3, 4, 5 });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T52_Defect_1459478_2()
        {
            String code =
@"
import(""FFITarget.dll"");
def foo (x1:var[])
{
    return = x1 + 1;
}  
def bar ( a : var[] ) 
{
    a[0] = a[0] + 1;
    return = a;
}

a = 0..4..1;
x = foo(a);
y = bar (a );
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", new Object[] { 0, 1, 2, 3, 4 });
            thisTest.Verify("y", new Object[] { 1, 1, 2, 3, 4 });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T53_Defect_1467086()
        {
            String code =
@"
a = 0..4..1;
test1 = a;
test2 = a;
test1 = test1 + 1;
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test2", new Object[] { 0, 1, 2, 3, 4 });
            thisTest.Verify("test1", new Object[] { 1, 2, 3, 4, 5 });

        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T53_Defect_1467086_2()
        {
            String code =
@"
a = 0..4..1;
test1 = a;
a = a + 1;
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test1", new Object[] { 1, 2, 3, 4, 5 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T55_Defect_1467341_Update_Causing_Cyclic_Dep()
        {
            String code = @"
def foo(arr: double)
{
	return = arr;
}
z=0;
p1 = z+1;
p2 = z+2;
points = [p1,p2];
a = foo(points);
z = 2;";
            string errmsg = "DNL-1467341 rev 3997: REGRESSION : false cyclic dependency detected";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", new Object[] { 3.0, 4.0 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T56_Defect_1467342_Inline_Condition_replication()
        {
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4088
            String code = @"
a = [ 1, 2];
x = a > 1 ? a : null; ";
            Object n1 = null;
            string errmsg = "MAGN-4088 Design issue with inline condition : only the condition being replicated";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("x", new Object[] { n1, 2 });
        }

        [Test]
        [Category("SmokeTest")]
        [Category("Failure")]
        public void T57_Defect_1467399()
        {
            String code = @"
class A
{
    a : int[];            
}
def foo ( x1 : A)
{
x1.a= -1;
    return = x1;
}
a1 = A.A();
a1.a = [1,2];
b = a1.a;
a1 = foo ( a1);
// received b = {1,2} - 10,11,12,13,11,12  - wrong 
 ";
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4088
            Object n1 = null;
            string errmsg = "MAGN-4088: Design issue with inline condition : only the condition being replicated";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", new object[] { -1 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T57_Defect_1467399_2()
        {
            String code = @"
class A
{
    a : int[];            
}
def foo ( x1 : A)
{
    x1.a= -1;
    return = x1;
}
a1 = A.A();
a1.a = [1,2];
b = a1.a;
a1.a = -1;
// b= {-1} , received - 10,11,12,13 - correct 
 ";
            Object n1 = null;
            string errmsg = "DNL-1467342 Design issue with inline condition : only the condition being replicated";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", new object[] { -1 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T57_Defect_1467399_4()
        {
            String code = @"
class A
{
    a : int[];            
}
def foo ( x1 : A)
{
    x1.a = -1;
    return = x1;
}
a1 = A.A();
a2 = A.A();
a1 = foo ( a1);
b = a1.a;
a1.a = [1,2];
// received - b- {1,2} 10,11,12,13 - correct
 ";
            Object n1 = null;
            string errmsg = "DNL-1467342 Design issue with inline condition : only the condition being replicated";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", new object[] { 1, 2 });
        }

        [Test]
        [Category("SmokeTest")]
        public void Update_In_Global_Variables()
        {
            String code = @"
b = 0;
a = 0;
def foo ()
{
    a = 1;
    b = a + 1;    
    return = a + b;
    
}
b = 1;
a = b;
test = foo();
";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code);
            thisTest.Verify("test", 3);
        }

        [Test]
        [Category("SmokeTest")]
        public void T58_Defect_1467396_Update_In_Global_Variables_2()
        {
            String code = @"
a = 2;
b = a + 1;
a = 1;
c = a + 1;
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            thisTest.Verify("c", 2);
            thisTest.Verify("b", 2);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T59_Defect_1467418_Update_Across_Language_Blocks()
        {
            string errmsg = "";
            string code = @"
import(""FFITarget.dll"");
i = 0;
i=[Imperative]
{
    while (i < 2)
    {
        [Associative]
        {
            as = [ClassFunctionality.ClassFunctionality(), ClassFunctionality.ClassFunctionality()];
            [Imperative]
            {
                c = 0;
                k = null;
                tmp = as;
                while (c < 2)
                {
                   k = as[c];
                   as[c] = ClassFunctionality.ClassFunctionality();
                   c = c + 1;
                }
            }
        }
		i = i + 1;
    }
    return i;
}";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);

            thisTest.Verify("i", 2);

        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T60_Defect_14672007_Update_In_Class_From_Imperative_Scope()
        {
            String code = @"
import(""FFITarget.dll"");
x = [ 1, 2 ];
y1 =ClassFunctionality.ClassFunctionality(x);
y2 = [ y1[0].IntVal, y1[1].IntVal ];
x=[Imperative]
{
    count = 0;
    for ( i in 0..1)
    {
        x[count] = x[count] + 1;
        count = count + 1;
    }
    return x;
}
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);

            thisTest.Verify("y2", new Object[] { 2, 3 });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T60_Defect_14672007_Update_In_Class_From_Imperative_Scope_2()
        {
            String code = @"
import(""FFITarget.dll"");
x = [ 1, 2 ];
y1 = ClassFunctionality.ClassFunctionality(x);
y2 = [ y1[0].IntVal, y1[1].IntVal ];
[Imperative]
{
    for ( i in y1)
    {
        i.IntVal = 0;        // Modifying  the array y1 will re-execute y2
    }
}
";
           thisTest.RunAndVerifySemanticError(code, ProtoImperative.Properties.Resources.ImperativeSymbolsAreReadOnly);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T60_Defect_14672007_Update_In_Class_From_Imperative_Scope_3()
        {
            String code = @"
import(""FFITarget.dll"");
x = [ 1, 2 ];
y1 = ClassFunctionality.ClassFunctionality(x);
y2 = [ y1[0].IntVal, y1[1].IntVal ];
[Imperative]
{ 
    count = 0;
    for ( i in y1)
    {
        temp = y1[count].Set(0); 
        count = count + 1; 
    }
}
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("y2", new Object[] { 1, 2 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T62_update_wrongsequnce_1467484()
        {
            String code = @"
x = 1;
g = 10;
a = x; 
g = g + 1;
c = g;
b = 2; 
t = a+c; 
c = a + b; 
x = 3;
";
            string errmsg = "1467484 - wrong execution sequence for update ";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("g", 11);

        }

        [Test]
        [Category("SmokeTest")]
        public void T62_update_wrongsequnce_1467484_2()
        {
            String code = @"
a=0;
b=0;
c=0;
m=0;
a = 1;
b = 2;
c = 3;
m = a + b + c;
";
            string errmsg = "1467484 - wrong execution sequence for update ";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("m", 6);

        }

        [Test]
        [Category("SmokeTest")]
        public void T62_update_wrongsequnce_global_1467484_3()
        {
            String code = @"
a;
b;
c;
a = 1;
b = a;
c = b;
";
            string errmsg = "1467484 - wrong execution sequence for update ";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", 1);
            thisTest.Verify("c", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T62_update_wrongsequnce_1467484_4()
        {
            String code = @"
x = 1; 
a = x + 1; 
c = 0; 
b = 2; 
t = a+c; 
c = a + b; 
x = 3;
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("t", 10);
            thisTest.Verify("a", 4);
            thisTest.Verify("c", 6);
        }

        [Test]
        [Category("SmokeTest")]
        public void T62_update_wrongsequnce_1467484_5()
        {
            String code = @"
a = [  1, 2,3 ];
a = [  0, 2,4 ];
b = [ 1, 2, 3 ];
c = 1;
a = b[c] + 1;
c = [1,2];
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", new object[] { 3, 4 });

            thisTest.Verify("c", new object[] { 1, 2 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T62_update_wrongsequnce_1467484_6()
        {
            String code = @"
c = 1;
a = 1;
c = a;
a = 1;
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("c", 1);
            thisTest.Verify("a", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T62_update_wrongsequnce_1467484_7()
        {
            String code = @"
g = 1;
g = g + 1;
c = g;
a = 1;
c = a;
a = 1;
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("c", 1);
            thisTest.Verify("a", 1);
            thisTest.Verify("g", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T62_update_wrongsequnce_1467484_8()
        {
            String code = @"
a = 7;
s = ""aa = "" + a;
c = a + 2;
s = ""cc = "" + c;
a = 10;
a = 33;
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", 33);
            thisTest.Verify("c", 35);
            thisTest.Verify("s", "cc = 35");
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T62_update_wrongsequnce_1467484_9()
        {
            String code = @"
import(""FFITarget.dll"");
c = 0;
x = c > 5 ? 1 : 2;
c = [Imperative]{ return 10; }
a = DummyPoint.ByCoordinates(10, 20, 30);
b = a;
c = Print(b);
a=[Imperative]{ return [ 1, 2, 3, 4, [ 5, [ 6, [ 7, [ 8.9 ] ] ] ] ]; }
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("x", 2);
            thisTest.Verify("a", new object[] { 1, 2, 3, 4, new object[] { 5, new object[] { 6, new object[] { 7, new object[] { 8.9 } } } } });
            thisTest.Verify("b", new object[] { 1, 2, 3, 4, new object[] { 5, new object[] { 6, new object[] { 7, new object[] { 8.9 } } } } });
            thisTest.Verify("c", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void T62_update_wrongsequnce_1467484_11()
        {
            String code = @"
a;
b;
c;
m;
n;
a = 1;
b = a;
c = b;
m = a + b + c;
n = m;
a = 2;
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", 2);
            thisTest.Verify("b", 2);
            thisTest.Verify("c", 2);
            thisTest.Verify("m", 6);
            thisTest.Verify("n", 6);
        }

        [Test]
        [Category("SmokeTest")]
        public void T62_update_wrongsequnce_1467484_12()
        {
            String code = @"
a = 2;
b = 3;
c = a + b;
dummy = c + 1;
c = a * b;
a = 3;
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", 3);
            thisTest.Verify("b", 3);
            thisTest.Verify("c", 9);
            thisTest.Verify("dummy", 10);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T62_update_wrongsequnce_1467484_13()
        {
            String code = @"
    a = 1;
    b = a;
    c = b;
    m = a + b + c;
    n = m;
    a = 2;
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", 2);
            thisTest.Verify("b", 2);
            thisTest.Verify("c", 2);
            thisTest.Verify("m", 6);
            thisTest.Verify("n", 6);
        }

        [Test]
        [Category("SmokeTest")]
        public void T62_update_wrongsequnce_1467484_16()
        {
            String code = @"
                a = 7;
                s = ""aa = "" + a;
                c = a + 2;
                s = ""cc = "" + c;
                [Imperative]
                {
                    a = 10;
                
                }
                a = 33;
   
                        ";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", 33);
            thisTest.Verify("c", 35);
            thisTest.Verify("s", "cc = 35");
        }

        [Test]
        [Category("SmokeTest")]
        public void T63_NoInfiniteLoop()
        {
            String code = @"
b = [Imperative]
{
    a = 1;
    b = null;
    [Associative]
    {
        b = a;
        a = [Imperative]
        {
            c = a;
            return [2];
        }
    }
    return b;
}
";
            string errmsg = "1467501- Infinite loop for this script";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", new object[] { 2 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T63_NoInfiniteLoop_1467501()
        {
            String code = @"
b = [Imperative]
{
    a = 1;
    b = null;
    [Associative]
    {
        b = a;
        a = [Imperative]
        {
            c = a;
            return [2];
        }
    }
    return b;
}
";
            string errmsg = "1467501- Infinite loop for this script";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", new object[] { 2 });
        }

        [Test]
        [Category("SmokeTest"), Category("Failure")]
        public void T63_NoInfiniteLoop_1467501_2()
        {
            String code = @"
b = [Imperative]
{
    a = 1;
    b = null;
    [Associative]
    {
        b = a;
        a = [Imperative]
        {
            c = a;
            d = b;
            return [ 2 ];
        }
    }
    return b;
}
";
            string errmsg = "1467501- Infinite loop for this script";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.VerifyRuntimeWarningCount(1);
        }

        [Test]
        [Category("SmokeTest")]
        [Category("Failure")]
        public void T63_NoInfiniteLoop_3_1467519()
        {
            String code = @"
[Imperative]
{
    a = 1;
    c = a;
    [Associative]
    {
        a = 2;
    }
}
";
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1702
            string errmsg = "MAGN-1702 [Design Issue ] should a variable declared in imperative, modified in an inner associative block trigger update";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", 2);
            thisTest.Verify("c", 1);
            thisTest.VerifyRuntimeWarningCount(1); // validation required, fix this after the design is verified
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T65_1467495_FalseCyclicDependancy()
        {
            String code = @"
import(""FFITarget.dll"");
def XPlusY(NextPoint : DummyPoint)
{
    x = NextPoint.X;
    y = NextPoint.Y;
    temp = x + y;
    return = temp;
}
pt1 = DummyPoint.ByCoordinates(0,0,0);
pt2 = XPlusY(pt1);
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("pt2", 0.0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T65_1467495_FalseCyclicDependancy_2()
        {
            String code = @"
import(""FFITarget.dll"");
def XPlusY(NextPoint : DummyPoint)
{
    x = NextPoint.X;
    y = NextPoint.Y;
    temp = x + y;
    return = temp;
}
pt1 = i[0];
pt2 = i[1];
i = [Imperative]
{
    pt1 = DummyPoint.ByCoordinates(0,0,0);
    pt2 = XPlusY(pt1);
    return [pt1, pt2];
}
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("pt2", 0.0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T65_1467495_FalseCyclicDependancy_5()
        {
            String code = @"
import(""FFITarget.dll"");
def XPlusY(NextPoint : DummyPoint)
{
    x = NextPoint.X;
    y = NextPoint.Y;
    temp = x + y;
    return = temp;
}
pt1 = DummyPoint.ByCoordinates(0,0,0);
pt2 = DummyPoint.ByCoordinates(0,0,0);
pt3 = XPlusY(pt1);
pt3 = XPlusY(pt2);
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("pt3", 0.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T66_1467512_RighthandsideUpdate_imperative()
        {
            String code = @"
                a = 1 ;
                b=[Imperative]
                {
                    b = null;
                    c = 3;
                    i = 0;
                    c = [ 1, 2, 3 ];
                    for(i in c)
                    {
                        b = a;
                    }
                    return b;
                }
                a = 2;
";
            string errmsg = "1467512 - a variable declared in associative must trigger update when modified in imperative , it doesnt if inside for or while ";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", 2);

        }

        [Test]
        [Category("SmokeTest")]
        public void T66_1467512_RighthandsideUpdate_imperative_2()
        {
            String code = @"
                a = 1 ;
                b=[Imperative]
                {
                   c = 3;
                   i = 0;
                   b = null;
                   while(i < c)
                    {
                        b = a;
                        i = i + 1;
                    }
                  return b;
                }
                a = 2;
                ";
            string errmsg = "1467512 - a variable declared in associative must trigger update when modified in imperative , it doesnt if inside for or while ";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T66_1467512_RighthandsideUpdate_imperative_4()
        {
            String code = @"
            a = 1 ;
            b=[Imperative]
            {
                b = null;
               c = 3;
               i = 0;
                if (d)
                {
                     b = a;
                }
                else
                {
                    b = a;
                }
                return b;
            }
            a = 2;
                ";
            string errmsg = "1467512 - a variable declared in associative must trigger update when modified in imperative , it doesnt if inside for or while ";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T66_1467512_RighthandsideUpdate_imperative_5()
        {
            String code = @"
            a = 1 ;
            b=[Imperative]
            {
                b = null;
               c = 3;
               i = 0;
                d=1;
                if (d)
                {
                     b = a;
                }
                else
                {
                    b = a;
                }
                return b;
            }
            a = 2;
                ";
            string errmsg = "1467512 - a variable declared in associative must trigger update when modified in imperative , it doesnt if inside for or while ";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T66_1467512_RighthandsideUpdate_imperative_6()
        {
            String code = @"
           a = 1 ;
            b=[Imperative]
            {
               c = 3;
               i = 0;
                b= null;
                e;
                if (d)
                {
                     b = a;
                }
                else if (e)
                {
                    b = a;
                }
                else
                {
                    b = a;
                }
                return b;
            }
            a = 2;
                ";
            string errmsg = "1467512 - a variable declared in associative must trigger update when modified in imperative , it doesnt if inside for or while ";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T66_1467513_RighthandsideUpdate_innerassociative()
        {
            String code = @"
            b;
            a = 1 ;
            [Associative]
            {
               c = 3;
               i = 0;
               b = a;
            }
            a = 2; 
                ";
            string errmsg = "MAGN-1509 [Design issue]Update on the inner associatve block is not triggered";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T67_CrossLanguage_1467520()
        {
            String code = @"
 a=1;
 d=100;
 d =[Imperative]
 {
    return a;
   
 }
 [Associative]
{
    a = 5;
}
";
            string errmsg = "1467520 [Regression] Cross language update is not correct if a variable used in imperative is modified in associative";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("d", 5);
        }



        [Test]
        [Category("SmokeTest")]
        public void T68_Cyclic_globalvariable_1467439()
        {
            String code = @"
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
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("q", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T70_Cyclic_globalvariable_1467439()
        {
            String code = @"
a;
b;
q;
[Associative]
{
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
q = a;
r = bar();
}
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("q", 1);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T71_Cyclic_globalvariable_1467439()
        {
            String code = @"
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
q = a;
r = bar();
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("q", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T72_Cyclic_globalvariable_1467439()
        {
            String code = @"
a = 1;
b = 0;
    b = a;
[Imperative]
{
    [Associative]
    {
        a = b;
    }
}
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T73_Cyclic_globalvariable_1467439()
        {
            String code = @"
a = 1;
b = 0;
    b = a;
[Imperative]
{
    [Associative]
    {
        a = b;
    }
}
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T74_TestUpdate()
        {
            String code = @"
a = 1;
c = 1;
c = a;
a = 2;
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", 2);
            thisTest.Verify("c", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T74_ExecutionSequence_1467484()
        {
            String code = @"
a = 1;
c = 1;
c = a;
a = 2;
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", 2);
            thisTest.Verify("c", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T74_TestUpdate_1467484_2()
        {
            String code = @"
a = 1;
c = 1;
c = a;
a = 2;
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", 2);
            thisTest.Verify("c", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T74_TestUpdate_1467484_3()
        {
            String code = @"
x = 1;      // 1,3 
a = x + 1;  // 2,4
c = 0;      // 0
b = 2;      // 2
t = a + c;  // 2,6,8,10
c = a + b;  // 4,6
x = 3;      // 3
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);

            thisTest.Verify("a", 4);
            thisTest.Verify("b", 2);
            thisTest.Verify("t", 10);
            thisTest.Verify("c", 6);
            thisTest.Verify("x", 3);
        }


        [Test]
        [Category("SmokeTest")]
        public void T74_TestUpdate_1467532()
        {
            String code = @"
 a = 1;
    b;
    [Imperative]
    {
        [Associative]
        {
           b = a;
        }
    }
a = 2;
// expect b = 2, but b = 1
";
            string errmsg = "1467532 - a variable used inside an inner associative within an imperative does not trigger update";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T74_TestUpdate_1467532_2()
        {
            String code = @"
a = 1;
b = 2;
[Imperative]
{
    
    [Associative]
    {
        b = b + a;
    }
    //b-expected 13 , received - 3
}
a = 10;
";
            string errmsg = "1467532 - a variable used inside an inner associative within an imperative does not trigger update";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", 13);
        }

        [Test]
        [Category("SmokeTest")]
        public void T74_TestUpdate_1467532_3()
        {
            String code = @"
a = 1;
b=[Imperative]
{
    b = a;
    [Associative]
    {
           a = 2;
        // expect b = 2, but b = 1
    }
    return b;
}
";
            string errmsg = "1467532 - a variable used inside an inner associative within an imperative does not trigger update";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", 1);
        }



        [Test]
        [Category("SmokeTest")]
        public void T74_TestUpdate_1467532_4()
        {
            String code = @"
a = 1;
b;
[Imperative]
{
    [Associative]
    {
           b = a;
    }
}
[Imperative]
{
    [Associative]
    {
        a = 2;
    }
}
";
            string errmsg = "1467532 - a variable used inside an inner associative within an imperative does not trigger update";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T74_TestUpdate_1467532_5()
        {
            String code = @"
a = 1;
b;
[Imperative]
{
    [Associative]
    {
           b = a;
    }
}
[Imperative]
{
    [Associative]
    {
        a = 2;
    }
}
";
            string errmsg = "1467532 - a variable used inside an inner associative within an imperative does not trigger update";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T74_TestUpdate_1467532_6()
        {
            String code = @"
a = 1;
b;
[Imperative]
{
    [Associative]
    {
           b = a;
    }
}
[Imperative]
{
    [Associative]
    {
        a = 2;
    }
}
";
            string errmsg = "1467532 - a variable used inside an inner associative within an imperative does not trigger update";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T74_TestUpdate_1467532_7()
        {
            String code = @"
a = 1;
b=[Imperative]
{
    b= null;
    [Associative]
    {
           b = a;
    }
    return b;
}
[Associative]
{
    a = [Imperative]
    {
        return 2;
    }
}
";
            string errmsg = "1467532 - a variable used inside an inner associative within an imperative does not trigger update";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T74_TestUpdate_1467532_8()
        {
            String code = @"
a = 1;
b;
[Imperative]
{
    c = 1;
    if (c)
    {
    [Associative]
    {
        b = a;
    }
    }
}
a = 2;
";
            string errmsg = "1467532 - a variable used inside an inner associative within an imperative does not trigger update";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T74_TestUpdate_1467532_9()
        {
            String code = @"
a = 1;
b;
[Imperative]
{
    for(c in 0..2)
    {
    [Associative]
    {
        b = a;
    }
    }
}
a = 2;
";
            string errmsg = "1467532 - a variable used inside an inner associative within an imperative does not trigger update";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T74_TestUpdate_1467532_10()
        {
            String code = @"
a = 1;
b;
[Imperative]
{
    c = 1;
    j = 1;
    while(j<2)
    {
    [Associative]
    {
        b = a;
    }
        j = j + 1;
    }
}
a = 2;
";
            string errmsg = "1467532 - a variable used inside an inner associative within an imperative does not trigger update";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T74_TestUpdate_1467531()
        {
            String code = @"
a = 1;
b = 2;
b = b + a;
a = 10;
";
            string errmsg = "1467531 - final result is wrong when self referencing the object";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", 12);
            thisTest.Verify("a", 10);
        }

        [Test]
        [Category("SmokeTest")]
        public void T74_TestUpdate_1467531_2()
        {
            String code = @"
a = 1;
b = 2;
b = b + a;
a = 10;
";
            string errmsg = "1467531 - final result is wrong when self referencing the object";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", 12);
            thisTest.Verify("a", 10);
        }

        [Test]
        [Category("SmokeTest")]
        public void T74_TestUpdate_1467531_3()
        {
            String code = @"
a=i[0];b=i[1];
i = [Imperative]
{
    a = 1;
    b = 2;
    b = b + a;
    a = 10;
    //expected 13 , received - 12
    return [a, b];
}
";
            string errmsg = "1467531 - final result is wrong when self referencing the object";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", 10);
            thisTest.Verify("b", 3);
        }

        [Test]
        [Category("SmokeTest")]
        public void T74_TestUpdate_1467531_4()
        {
            String code = @"
a = 1;
b = 2;
b = [Imperative]
{
    return b + a;
    //expected 13 , received - 12
}
  a = 10;
";
            string errmsg = "1467531 - final result is wrong when self referencing the object";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", 10);
            thisTest.Verify("b", 13);
        }

        [Test]
        [Category("SmokeTest")]
        public void T74_TestUpdate_1467531_5()
        {
            String code = @"
a = 1;
b = 2;
 b =[Imperative]
{
   return b + a;
  
    //expected 13 , received - 12
}
[Associative]
{
  a = 10;
}
";
            string errmsg = "1467531 - final result is wrong when self referencing the object";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", 10);
            thisTest.Verify("b", 13);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T75_TestUpdate_1467536()
        {
            String code = @"
import(""FFITarget.dll"");
def f(p : DummyPoint)
{
    return = p.X + 1;
}

p1 = DummyPoint.ByCoordinates(1,1,1);
p2 = DummyPoint.ByCoordinates(2,2,2);
j = 0;
j = [Imperative]
{
    j = j + 1;
    f(p2);
    return j;
}
p2 = DummyPoint.ByCoordinates(3,3,3);
";
            string errmsg = "1467536 -when class instance is used inside an imperative block and modifid outside , it does not reexecute imperative block ";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("j", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T76_TestUpdate_1467520()
        {
            String code = @"
            a = 1;
            b;
            d;
            b = [Imperative]
            {
               return a;
            }
            [Associative]
            {
               //d = b;
            }
            a = 2;
";
            string errmsg = "1467520-Cross language update is not correct if a variable used in imperative is modified in associative ";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T77_TestUpdate_Crosslangauge_1467520_2()
        {
            String code = @"
         a = 1;
b;
[Imperative]
{
    c = 1;
    if (c)
    {
    [Associative]
    {
        b = a;
    }
    }
}
[Associative]
{
    a = 2;
}
";
            string errmsg = "1467520 -Cross language update is not correct if a variable used in imperative is modified in associative ";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T78_TestUpdate_Crosslangauge_1467520_3()
        {
            String code = @"
a = 1;
b;
[Imperative]
{
    for(c in 0..2)
    {
    [Associative]
    {
        b = a;
    }
    }
}
[Associative]
{
    a = 2;
}
";
            string errmsg = "1467520 -Cross language update is not correct if a variable used in imperative is modified in associative ";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T79_TestUpdate_Crosslangauge_1467520_4()
        {
            String code = @"
d;
a = 1;
b;
z;
i=[Imperative]
{
 
    [Associative]
    {
        b = a;
    }
 
    if (d)
    {
        z = 1;
    }
    return [b,z];
}
b = i[0];
z = i[1];
[Associative]
{
    a = 2;
}
d = a;
";
            string errmsg = "1467520 -Cross language update is not correct if a variable used in imperative is modified in associative ";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("z", 1);
            thisTest.Verify("b", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T80_TestUpdate_Crosslangauge_1467520_5()
        {
            String code = @"
a = 1;
b;
d;
[Imperative]
{
    for(c in 0..2)
    {
        if (d)
        {
    [Associative]
    {
        b = a;
    }
        }
    }
}
[Associative]
{
    a = 2;
}
d = 1;
";
            string errmsg = "1467520 -Cross language update is not correct if a variable used in imperative is modified in associative ";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", 2);
            thisTest.Verify("b", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T81_TestUpdate_Crosslangauge_1467520_6()
        {
            String code = @"
a = 1;
b;
d;
[Imperative]
{
    for(c in 0..2)
    {
        if (d)
        {
    [Associative]
    {
        b = a;
    }
        }
    }
}
[Associative]
{
    a = 2;
}
[Associative]
{
    d = 1;
}
";
            string errmsg = "1467520 -Cross language update is not correct if a variable used in imperative is modified in associative ";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", 2);
            thisTest.Verify("b", 2);
            thisTest.Verify("d", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T82_TestUpdate_Crosslangauge_1467520_7()
        {
            String code = @"
a = 1;
b;
d;
b = [Imperative]
{
    for(c in 0..2)
    {
        if (d)
        {
            [Associative]
            {
                b = a;
            }
        }
    }
    return b;
}
[Associative]
{
    a = 2;
}
d = [Imperative]
{
    return 1;
}
";
            string errmsg = "1467520 -Cross language update is not correct if a variable used in imperative is modified in associative ";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", 2);
            thisTest.Verify("b", 2);
            thisTest.Verify("d", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T83_TestUpdate_Crosslangauge_1467538()
        {
            String code = @"
a = 1;
b;
d;
c;
[Imperative]
{
    b = a;
}
[Associative]
{
    c=1;
}
a = 2;
";
            string errmsg = "1467538- it does not reexecute imperative ,in the presence of a parallel associative block";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("c", 1);

        }

        [Test]
        [Category("SmokeTest")]      
        public void T84_TestUpdate_Crosslangauge_1467513_3()
        {
            String code = @"
a = 1;
b;
d;
b =[Imperative]
{
     return a;
}
[Associative]
{
    d = b;
}
a = 2;
";
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4089
            string errmsg = "MAGN-4089: it does not reexecute imperative ,in the presence of a parallel associative block";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("d", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T84_TestUpdate_Crosslangauge_1467513_4()
        {
            String code = @"
a = 1;
b;
d;
b = [Imperative]
{
    return a;
}
[Associative]
{
  
}
[Associative]
{  d = b;
}
a = 2;
";
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4089
            string errmsg = "MAGN-4089: it does not reexecute imperative ,in the presence of a parallel associative block";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("d", 2);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T85_1467452_update_using_class_methods()
        {
            String code = @"
import(""FFITarget.dll"");
a1 = ClassFunctionality.ClassFunctionality();
x1 = 3;
y = a1.SetAndReturn( x1 ); 
x1 = [Imperative]
{
    return 4;
}
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("y", 4);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T85_1467452_update_using_class_methods_2()
        {
            String code = @"
import(""FFITarget.dll"");
x1 = 3;
y = 0;
test = 0;
i = [Imperative]
{
    a1 = ClassFunctionality.ClassFunctionality();    
    y = a1.SetAndReturn( x1 ); 
    test = a1.IntVal;
    return [y, test];
}
y = i[0];
test = i[1];
x1 = 4;
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("y", 4);
            thisTest.Verify("test", 4);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T85_1467452_update_using_class_methods_3()
        {
            String code = @"
import(""FFITarget.dll"");
x1 = 3;
y = 0;
[Associative]
{
    a1 = ClassFunctionality.ClassFunctionality();
    test = a1.IntVal;
    y = a1.SetAndReturn( x1 ); 
}
x1 = 4;
";
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1506
            string errmsg = "MAGN-1506 [Design issue]Update on the inner associatve block is not triggered";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("y", 4);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        [Category("Failure")]
        public void T85_1467452_update_using_class_methods_4()
        {
            String code = @"
import(""FFITarget.dll"");
y = [Associative]
{
    x1 = 3;
    y = x1;
    return = [Imperative]
    {
        a1 = ClassFunctionality.ClassFunctionality();
        test = a1.IntVal;
        return = a1.SetAndReturn( x1 ); 
    }
}
x1 = 4;
";
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1506
            string errmsg = "MAGN-1506 [Design issue]Update on the inner associatve block is not triggered";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("y", 4);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T85_1467452_update_using_class_methods_5()
        {
            String code = @"
import(""FFITarget.dll"");
x1 = 3;
y = 0;
y = [Imperative]
{
    a1 = ClassFunctionality.ClassFunctionality();
    if( x1 == 3)
    {
        y = a1.SetAndReturn(x1);
    }
    else
    {
        y = a1.SetAndReturn(x1+1);
    }
    return y;
}
x1 = 4;
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("y", 5);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T85_1467452_update_using_class_methods_6()
        {
            String code = @"
import(""FFITarget.dll"");
x1 = 3;
y = 0;
y = [Imperative]
{
    a1 = ClassFunctionality.ClassFunctionality();
    for(i in 0..1)
    {
        y = y + a1.SetAndReturn(x1);
    }
    return y;
}
x1 = 4;
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("y", 14);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T85_1467452_update_using_class_methods_7()
        {
            String code = @"
import(""FFITarget.dll"");
y = 0;
x1 = 3;
y = 0;
y = [Imperative]
{
    a1 = ClassFunctionality.ClassFunctionality();
    i = 1;
    while(i <= 2)
    {
        y = y + a1.SetAndReturn(x1);
        i = i+1;
    }
    return y;
}
x1 = 4;
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("y", 14);
        }

        [Test]
        [Category("SmokeTest")]
        public void T86_variableupdate()
        {
            String code = @"
             x = 1;
             a = x;
             b = a;
             a = a + 1; // Redefinition 'a' no longer depends on 'x'
             x = 3;
            ";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("x", 3);
            thisTest.Verify("a", 2);
            thisTest.Verify("b", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T87_arrayupdate()
        {
            String code = @"
 
            a = [ 10, 20, 30 ];
            c = 1;
            b = a[c];
            a[c] = 100;
            c = 2;
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", 100);
            thisTest.Verify("c", 2);
            thisTest.Verify("a", new object[] { 10, 100, 100 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T87_nestedblocks()
        {
            String code = @"
 
    a; x; b;
    [Associative]
  	{
      	a = 1;
      	b = [Imperative]
      	{
			b = 10;
			[Associative]
			{
          		x = a;
			}
            return b;
      	}	
      	a = 10;
	}
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", 10);
            thisTest.Verify("b", 10);
        }


        [Test]
        [Category("SmokeTest")]
        public void T89_1467414()
        {
            String code = @"
 
            a; x;
            a = 1;
            b = a + 1;
            d;
            i = [Imperative]
            {
                a = 2;
                c = b + 1;
	            b = a + 2;
                [Associative]
                {
                   a = 1.5;
                   d = c + 1;
                   b = a + 3; 
                   a = 2.5; 	   
                }
                b = a + 4;
                a = 3;	
				return [a, b, d];
            }
            f = i[0] + i[1] + i[2];
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("f", 13.5);
        }

        [Test]
        [Category("SmokeTest")]
        public void T90_1467510_cyclic()
        {
            String code = @"
import(""FFITarget.dll"");
a = 1;
b = a + 1;
c = a + b;
n = DummyVector.ByCoordinates( a, b, c );
m = ClassFunctionality.ClassFunctionality(n);
a = 3;
x = n.X;
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("x", 3);
        }

        [Test]
        [Category("SmokeTest")]
        public void T90_1467510_cyclic_2()
        {
            String code = @"
 
        a = 1;
        b = a;
        c = b;
        m = a + b + c;
        n = m;
        a = 2;
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.VerifyRuntimeWarningCount(0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T91_1467336_cyclic_1()
        {
            String code = @"
 
        a = 1;
        b = a;
        c = b;
        m = a + b + c;
        n = m;
        a = 2;
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.VerifyRuntimeWarningCount(0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T91_1467547_3()
        {
            String code = @"
 
        def foo()
        {
            return = 7;
        }
        def bar()
        {
            return = 3;
        }
        def ding(a)
        {
            return = a < 100? foo: bar;
        }
        a = 10;
        t = ding(a);
        z = t();
        a = 200;
";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code);
            thisTest.Verify("z", 3);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T91_1467547_4()
        {
            String code = @"
import(""FFITarget.dll"");
def foo ( b1 : ClassFunctionality )
{
    return = b1.IntVal;
}
b1 = ClassFunctionality.ClassFunctionality( 1 );
d1 = foo;
e1 = d1(b1);
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("e1", 1);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T91_1467547_5()
        {
            String code = @"
import(""FFITarget.dll"");
def foo ( b1 : ClassFunctionality )
{
    return = b1.IntVal;
}
b1 = ClassFunctionality.ClassFunctionality( 1 );
d1 = foo;
e1 = d1(b1);
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("e1", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T91_1467547_6()
        {
            String code = @"
 
        def foo()
        {
            return = 7;
        }
        def bar()
        {
            return = 3;
        }
        def ding(a)
        {
            return = a < 100? foo: bar;
        }
a;
        z = [Imperative]
        {
            a = 10;
            t = ding(a);
            z = t();
            a = 200;
            return z;
        }
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("z", 7);
        }

        [Test]
        [Category("SmokeTest")]
        public void T91_1467547_7()
        {
            String code = @"
 
        def foo()
        {
            return = 7;
        }
        def bar()
        {
            return = 3;
        }
        def ding(a)
        {
            return = a < 100? foo: bar;
        }
z;
a;
        [Associative]
        {
            a = 10;
            t = ding(a);
            z = t();
            a = 200;
        }
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("z", 3);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T92_Test_Update_Propagation_In_Self_Update_Thru_Inline_Condition()
        {
            String code = @"
import(""FFITarget.dll"");
c = 0;
ClassFunctionality.StaticProp = 1;
b = ClassFunctionality.StaticProp + 1;
c = c + b;
ClassFunctionality.StaticProp = false ? 43 : ClassFunctionality.StaticProp;
";
            string errmsg = "DNL-1467636 Self update should not trigger update propagation";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("c", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T93_Test_Update_Propagation()
        {
            String code = @"
a = 0;
b = 1;
a = a + b;
b = b;
";
            string errmsg = "DNL-1467636 Self update should not trigger update propagation";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", 1);
        }
    }
}
