using System;
using System.IO;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;
namespace ProtoTest.TD.MultiLangTests
{
    class TestUpdate : ProtoTestBase
    {
        ProtoScript.Config.RunConfiguration runnerConfig;
        string testPath = "..\\..\\..\\test\\Engine\\ProtoTest\\ImportFiles\\";
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
        public void T01_Update_Variable_Across_Language_Scope()
        {
            string code = @"
a;b;c;d;
[Associative]
{
    a = 0;
	d = a + 1;
    [Imperative]
    {
		b = 2 + a;
		a = 1.5;
		
    }
	c = 2;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1.5, 0);
            thisTest.Verify("b", 2, 0);
            thisTest.Verify("c", 2, 0);
            thisTest.Verify("d", 2.5, 0);

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
[Imperative]
{
    a = 2.5;
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
[Imperative]
{
    a = 2.5;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 2.5, 0);
            thisTest.Verify("c", 3.5, 0);
            thisTest.Verify("b", 5, 0);
        }


        [Test]
        [Category("SmokeTest")]
        public void T04_Update_Class_Instance_Argument()
        {
            string code = @"
class A
{
    a : int;
	constructor A ( x : int )
	{
	    a = x;
	}
	def add ( x : int )
	{
	    a = a + x;
		return = a;
	}
}
t1 = 1;
a1 = A.A(t1);
b1 = a1.add(t1);
[Imperative]
{
	t1 = 2;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b1", 4, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T05_Update_Class_Instance_Argument()
        {
            string code = @"
class A
{
    a : int;
	constructor A ( x : int )
	{
	    a = x;
	}
	def add ( x : int )
	{
	    a = a + x;
		return = A.A(a);
	}
}
t1 = 1;
a1 = A.A(t1);
b1 = a1.add(t1);
t2 = b1.a;
//t1 = 2;
[Imperative]
{
	t1 = 2;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t2", 4, 0);
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
            Object[] v1 = new Object[] { 2, 3, 4 };
            thisTest.Verify("a", v1, 0);
            thisTest.Verify("b", v1, 0);
            thisTest.Verify("c", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T08_Update_Array_Variable()
        {
            //Assert.Fail("1467194 - Sprint 25 - rev 3207[Regression] Regressions created by array copy constructions ");
            string code = @"
a = 1..3;
c = a;
b = [ Imperative ]
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
e = b;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.Runtime.WarningID.kCyclicDependency);
            /*
            Object[] v2 = new Object[] { 3, 4, 5 };
            Object[] v1 = new Object[] { 2, 3, 4 };
            thisTest.Verify("a", v2, 0);
            thisTest.Verify("e", v1, 0);
            thisTest.Verify("c", v2, 0);
            thisTest.Verify("d", v2, 0);
            */
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
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.Runtime.WarningID.kCyclicDependency);

            /*
            thisTest.Verify("d", 3, 0);
            thisTest.Verify("b", 3, 0);
            thisTest.Verify("e", 2, 0);
            thisTest.Verify("f", 3, 0);
            */
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
    x = { 10, a[1], a[2] };
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
            Object[] v1 = new Object[] { 10, 20, 3 };
            Object[] v2 = new Object[] { 10, 2, 3 };
            thisTest.Verify("d", v1, 0);
            thisTest.Verify("b", v1, 0);
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
            thisTest.Verify("b", 12, 0);
            thisTest.Verify("f", 16, 0);
        }

        [Test]
        public void T14_Defect_1461209()
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
            //Assert.Fail("1467186 - sprint24 : REGRESSION: rev 3172 : Cyclic dependency detected in update cases");
            thisTest.Verify("a1", 5.0, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T14_Defect_1461209_2()
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
x = [Imperative]
{
    return = 5;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("a1", 5.0, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T14_Defect_1461209_3()
        {
            string code = @"
class A
{
    a : var = foo (1);
	
	def foo (a1 : var)
	{
	    return = a1 + 1;
	}
}
y = A.A( );
a1 = y.a;
a2 = y.foo(1);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("a1", 2, 0);
            thisTest.Verify("a2", 2, 0);
        }

        [Test]

        public void T14_Defect_1461209_4()
        {
            string code = @"
class A
{
    a : var ;
	b = a + 1;
	
	constructor A (a1 : var)
	{
	    a = a1 + 1;
	}
}
y = A.A( x );
a1 = y.a;
b1 = y.b;
x = [Imperative]
{
    return = 3;
}
x = 2;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a1", 3, 0);
            thisTest.Verify("b1", 4, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T15_Defect_1460935()
        {
            string code = @"
class B
{ 
	x3 : int ;
		
	constructor B(a) 
	{	
		x3 = a;
	}
	
}
def foo ( b1 : B )
{
    return = b1.x3;
}
b1 = B.B( 1 );
x = b1.x3;
b1 = 1;
y = x; // expected : null; recieved : 1
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object v1 = null;
            thisTest.Verify("x", v1, 0);
            thisTest.Verify("y", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T15_Defect_1460935_2()
        {
            string code = @"
class B
{ 
	x3 : int ;
		
	constructor B(a) 
	{	
		x3 = a;
	}
	
}
def foo ( b1 : B )
{
    return = b1.x3;
}
b1 = B.B( 1 );
x = b1.x3;
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
        [Category("SmokeTest")]
        public void T15_Defect_1460935_4()
        {
            string code = @"
class B
{ 
	x3 : int ;
		
	constructor B(a) 
	{	
		x3 = a;
	}
	
}
class A
{ 
	x3 : int ;
		
	constructor A(a) 
	{	
		x3 = a;
	}
	
}
def foo ( b1 : B )
{
    return = b1.x3;
}
b1 = B.B( 1 );
x = b1.x3;
y = foo ( b1 );
[Imperative]
{
	b1 = A.A( 2 );	
}
b2 = B.B( 2 );
x2 = b2.x3;
y2 = foo ( b2 );
[Imperative]
{
	b2 = null;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object v1 = null;
            thisTest.Verify("x", 2, 0);
            thisTest.Verify("x2", v1, 0);
            thisTest.Verify("y", v1, 0);
            thisTest.Verify("y2", v1, 0);
        }

        [Test]
        [Category("Replication")]

        public void T15_Defect_1460935_5()
        {
            string code = @"
class B
{ 
	x3 : int ;
		
	constructor B(a) 
	{	
		x3 = a;
	}
	
}
b3 = B.B( 2 );
x3 = b3.x3;
[Imperative]
{
	b3 = { B.B( 1 ), B.B( 2 ) } ;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 1, 2 };
            thisTest.Verify("x3", v1, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T15_Defect_1460935_6()
        {
            string code = @"
class B
{ 
	x3 : int ;
		
	constructor B(a) 
	{	
		x3 = a;
	}
	
}
def foo ( b : B )
{
    return = { b.x3, b.x3 + 1 };
}
b1 = B.B( 2 );
x1 = b1.x3;
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
a1 = { 1.0, 2.0};
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
[Imperative]
{
    x = 4;
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
[Imperative]
{
    x = 2;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("y", 7, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T16_Defect_1460623_4()
        {
            string code = @"
class A
{
	x : var;
	constructor A ( a )
	{
    	x = a;		
		x = a + 1;
		x = a + 2;
	}
	
	def foo ()
	{
	    x = 4;
		x = 5;
		return = 5;
	}
}
x1 = 1;
a1 = A.A( x1 );
y1 = a1.x;
z1 = a1.foo();
[Imperative]
{
    x1 = 2;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("y1", 5, 0);
            thisTest.Verify("z1", 5, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T17_Defect_1459759()
        {
            string code = @"
class B
{
    b1 : var;
	constructor B ( )
	{
	    b1 = 3;
	}
}
p1 = 1;
p2 = p1 * 2;
p1 = true;
x1 = 3;
y1 = x1 + 1;
x1 = B.B();
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
a1 = { 1, 2 };
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
            fsr.LoadAndPreStart(src, runnerConfig);
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
[Imperative]
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
}
b = c + 3;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", 4);
            thisTest.Verify("b", 7);
            thisTest.Verify("x", null);
            thisTest.Verify("d", 3);

        }

        [Test]
        [Category("Update")]
        public void T19_Update_Class_Properties_Thru_Methods()
        {
            string code = @"
class A
{
    a : int = 0;
	
	constructor A ()
	{
	    a = 1;
	}
	
	def Update ()
	{
	    a = 2;
		return = true;
	}
}
a1 = A.A();
b1 = a1.a;
x1 = a1.Update();
b2 = b1;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b2", 2);
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
[Imperative]
{
    a = 2.5;
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
c = { a, a };
[Imperative]
{
    a = 2.5;
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
[Imperative]
{
   a = foo(2);
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", 4.0);

        }

        [Test]
        [Category("Update")]
        [Category("Failure")]
        public void T20_Defect_1461391_4()
        {
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4085
            string err = "MAGN-4085: Updating a class property using a class method from an imperative scope is not working now";
            string code = @"class A
{
    a : int;
	constructor A ( a1 : int )
	{
	    a = a1;		
	}
	
	def update ( a2 : int )
	{
	    a = a2;
		return = true;
	}
}
x = { 1, 2 };
y1 = A.A(x);
y2 = { y1[0].a, y1[1].a };
[Imperative]
{ 
	for ( count in 0..1)
	{
	    temp = y1[count].update(0);	
	}
}
t1 = y2[0];
t2 = y2[1];
";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, err);
            Object[] v = new Object[] { 0, 0 };
            thisTest.Verify("y2", v);
            thisTest.Verify("t1", 1);
            thisTest.Verify("t2", 2);
        }

        [Test]
        [Category("Update")]
        [Category("Failure")]
        public void T20_Defect_1461391_5()
        {
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4086
            string errmsg = "MAGN-4086: Update of class instance by updating its property is not propagating the proper update";
            string code = @"class A
{
    a : int;
	constructor A ( a1 : int )
	{
	    a = a1;		
	}
	
	def update ( a2 : int )
	{
	    a = a2;
		return = true;
	}
}
def foo ( a : A) 
{
    return = a.a;
}
x = { 1, 2 };
y1 = A.A(x);
y2 = foo ( y1);
[Imperative]
{ 
	count = 0;
	for ( i in y1)
	{
	    temp = y1[count].update(0);	
        count = count + 1;		
	}
}
t1 = y2[0];
t2 = y2[1];
";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("t1", 0);
            thisTest.Verify("t2", 0);
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
y1 = { 1, 2 };
y2 = foo ( y1);
[Imperative]
{ 
	count = 0;
	for ( i in y1)
	{
	    y1[count] = y1[count] + 1;	
        count = count + 1;		
	}
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
    [Imperative]
    {
       b = 2 + a;
       a = 1.5;
              
    }
    c = a + 2; // fail : runtime assertion 
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", 3.5);
        }


        [Test]
        [Category("SmokeTest")]
        public void T22_Update_Class_Instance()
        {
            //Assert.Fail("1463700 - Sprint 20 : rev 2147 : Update is not happening when a collection property of a class instance is updated using a class method ");

            string code = @"
class A
{
    a : int[];
	constructor A ( a1 : int[] )
	{
	    a = a1;		
	}
	
	def update ( a2 : int, i:int )
	{
	    a[i] = a2;
		return = true;
	}
}
y1 = { 1, 2 };
y2 = { 3, 4 };
x = { A.A (y1), A.A(y2) };
t1 = x[0].a[0];
t2 = x[1].a[1];
dummy = 0;
[Imperative]
{ 
	count = 0;
	for ( i in y1)
	{
	    y1[count] = y1[count] + 1;	      
		count = count + 1;		
	}
}
dummy=1;
[Imperative]
{ 
	count = 0;
	for ( i in y2)
	{
	    y2[count] = y2[count] + 1;	      
		count = count + 1;		
	}
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("t1", 2);
            thisTest.Verify("t2", 5);
        }

        [Test]
        [Category("SmokeTest")]
        public void T23_Update_Class_Instance_Using_Set_Method()
        {
            string error = "1467385 Sprint 27 - rev 4219 - valid update testcase throws cyclic dependancy error ";
            string code = @"class A
{
    a : int;	
}
a1 = A.A();
a1.a = 1;
b = a1.a;
a1.a = 2;
";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("b", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T23_Update_Class_Instance_Using_Set_Method_2()
        {
            string error = "1467385 Sprint 27 - rev 4219 - valid update testcase throws cyclic dependancy error ";
            string code = @"class A
{
    a : int[];	
}
a1 = A.A();
a1.a = {1,2};
b = a1.a;
a1.a = {2,3};
";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("b", new Object[] { 2, 3 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T23_Update_Class_Instance_Using_Set_Method_3()
        {
            //string errmsg = "1467318 - Cannot return an array from a function whose return type is var with undefined rank (-2)";
            string error = "1467385 Sprint 27 - rev 4219 - valid update testcase throws cyclic dependancy error ";
            string code = @"class A
{
    a : int[];	
}
a1 = A.A();
a1.a = {1,2};
b = a1.a;
a1.a = null;
";
            thisTest.VerifyRunScriptSource(code, error);
            Object n1 = null;
            thisTest.Verify("b", n1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T23_Update_Class_Instance_Using_Set_Method_4()
        {
            string error = "1467385 Sprint 27 - rev 4219 - valid update testcase throws cyclic dependancy error ";
            string code = @"class A
{
    a : int[];	
}
a1 = A.A();
a1.a = {1,2};
b = a1.a;
a1.a = 3.5;
";
            thisTest.VerifyRunScriptSource(code, error);

            thisTest.Verify("b", new Object[] { 4 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T23_Update_Class_Instance_Using_Set_Method_5()
        {
            string error = "1467385 Sprint 27 - rev 4219 - valid update testcase throws cyclic dependancy error ";
            string code = @"class A
{
    a : int[];	
}
a1 = A.A();
a1.a = {1,2};
b = a1.a;
a1.a = true;
";
            thisTest.VerifyRunScriptSource(code, error);
            Object n1 = null;
            thisTest.Verify("b", n1);
        }

        [Test]
        [Category("SmokeTest")]
        [Category("Failure")]
        public void T23_Update_Class_Instance_Using_Set_Method_6()
        {
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1553
            string errmsg = "MAGN-1553: [Design Issue] update of instance , between property update and by method what ist he expected answer ";//1467187 - Sprint24: REGRESSION : rev 3177: When a class collection property is updated, the value if not reflected";
            string code = @"class A
{
    a : int[];	
}
def foo ( x1 : A)
{
    x1.a = { 0, 0};
    x1.a[3] = -1;
    return = x1;
}
a1 = A.A();
a1.a = {1,2}; // Having this line means not testing the property modification in foo. This is because this line will get re-executed as a1.a is modified in foo
b = a1.a;
a1 = foo ( a1);
";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            thisTest.Verify("b", new Object[] { 0, 0, n1, -1 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T23_Update_Class_Instance_Using_Set_Method_7()
        {
            string errmsg = "";
            string code = @"class A
{
    a : int[];	
}
def foo ( x1 : A)
{
    x1.a = { 0, 0};
    x1.a[3] = -1;
    return = true;
}
a1 = A.A();
// a1.a = {1,2}; // Having this line means not testing the property modification in foo. This is because this line will get re-executed as a1.a is modified in foo
b = a1.a;
dummy = foo ( a1);
";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            thisTest.Verify("b", new Object[] { 0, 0, n1, -1 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T24_Update_Variable_Type()
        {
            //Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            //{
            //Assert.Fail("1463327 - Sprint 20 : Rev 2086 : Update issue : When a variable is updated to a different type using itself DS is throwing System.NullReference exception");

            string code = @"
class A
{	Pt : double;
	constructor A (pt : double)	
	{		
	    Pt = pt;	
	}
}
c = 1.0;
c = A.A( c );
x = c.Pt;";
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
            Assert.IsTrue(mirror.GetValue("p2", 0).DsasmValue.IsNull);
        }

        [Test]
        [Category("SmokeTest")]
        public void T25_Defect_1459759_2()
        {
            string code = @"
a1 = { 1, 2 };
y = a1[1] + 1;
a1[1] = 3;
a1 = 5;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue(mirror.GetValue("y", 0).DsasmValue.IsNull);
        }

        [Test]
        [Category("SmokeTest")]
        public void T25_Defect_1459759_3()
        {
            string code = @"
a = { 2 , b ,3 };
b = 3;
c = a[1] + 2;
d = c + 1;
b = { 1,2 };";
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
		b = { 2, 3 };
		c = b[1] + 1;
		b = 2;
		return = c;
	}
	a = x;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue(mirror.GetValue("a").DsasmValue.IsNull);
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
		return = { a , b };
	}
e;
[Imperative]
{
	c = 3;
	d = 4;
	e = foo( c , d );
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v = new Object[] { null, true };
            thisTest.Verify("e", v);
        }

        [Test]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T26_Defect_1463663()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
a = {
  1;
  +1;
  } 
  
b = a + 1;
c = [Imperative]
{
  a1 = {
  1;
  +1;
  } 
  
  b1 = a1 + 1;
}
";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });

        }

        [Test]
        [Category("SmokeTest")]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T27_Modifier_Stack_With_Right_Assignment()
        {
            string code = @"
v = 1;
a = 
{
  1 => a1;
  +1 => a2;
  +v => a3;
} 
b = a + 1;
c = a1 + 1;
d = a2 + 1;
f = a3 + 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", 4);
            thisTest.Verify("c", 2);
            thisTest.Verify("d", 3);
            thisTest.Verify("f", 4);
        }

        [Test]
        [Category("SmokeTest")]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T27_Modifier_Stack_With_Function_Call()
        {
            string code = @"
def foo ( a : int, b : double )
{
    return = a + b + 3;
}
v = 1;
a = {
  1 => a1;
  +1 => a2;
  +foo(a2,a1)-3 => a3;
  } 
  
b = a + 1;
c = a1 + 1;
d = a2 + 1;
f = a3 + 1;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("b", 6.0);
            thisTest.Verify("c", 2);
            thisTest.Verify("d", 3);
            thisTest.Verify("f", 6.0);
        }

        [Test]
        [Category("SmokeTest")]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T27_Modifier_Stack_With_Function_Call_2()
        {
            string code = @"
def foo ( a : int, b : double )
{
    return = a + b + 3;
}
x = [Associative]
{
	v = 1;
	a = {
	  1 => a1;
	  +1 => a2;
	  +foo(a2,a1)-3 => a3;
	  } 
	  
	b = a + 1;
	c = a1 + 1;
	d = a2 + 1;
    f = a3 + 1;
	return = { b, c, d, f };
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 6.0, 2, 3, 6.0 };
            thisTest.Verify("x", v1);
        }

        [Test]
        [Category("SmokeTest")]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T27_Modifier_Stack_With_Different_Types()
        {
            string code = @"
    class A
	{ 
	    x : int;
	    constructor A ( y )
		{
		    x = y;
		}
	}
	a = {
		  1 => a1;
		  0.5 => a2;
		  null => a3;
		  false => a4;
		  { 1,2 } => a5;
		  { null, null } => a6;
		  A.A(1) => a7;	 
          A.A(1).x => a8;			  
	  } 
	  
	
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object v1 = null;
            Object[] v2 = new Object[] { 1, 2 };
            Object[] v3 = new Object[] { null, null };
            thisTest.Verify("a1", 1);
            thisTest.Verify("a2", 0.5);
            thisTest.Verify("a3", v1);
            thisTest.Verify("a4", false);
            thisTest.Verify("a5", v2);
            thisTest.Verify("a6", v3);
            thisTest.Verify("a8", 1);
        }

        [Test]
        [Category("SmokeTest")]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T27_Modifier_Stack_With_Different_Types_2()
        {
            string code = @"
    class A
	{ 
	    x : int;
	    constructor A ( y )
		{
		    x = y;
		}
	}
	a = {
		  1 => a1;
		  a1 - 0.5 => a2;
		  a2 * null => a3;
		  a1 > 10 ? true : false => a4;
		  a1..2 => a5;
		  { a3, a3 } => a6;
		  A.A(a1) => a7;	 
          A.A(a1).x => a8;			  
	  } 
	  
	
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object v1 = null;
            Object[] v2 = new Object[] { 1, 2 };
            Object[] v3 = new Object[] { null, null };
            thisTest.Verify("a1", 1);
            thisTest.Verify("a2", 0.5);
            thisTest.Verify("a3", v1);
            thisTest.Verify("a4", false);
            thisTest.Verify("a5", v2);
            thisTest.Verify("a6", v3);
            thisTest.Verify("a8", 1);

        }

        [Test]
        [Category("SmokeTest")]
        [Category("Failure")]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T27_Modifier_Stack_Inside_Function()
        {
            // Tracked by: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4385
            string code = @"
class A
{ 
    x : int;
    constructor A ( y )
	{
	    x = y;
	}
}
def foo ( ) 
{
    
	a = {
		  1 => a1;
		  a1 - 0.5 => a2;
		  a2 * null => a3;
		  a1 > 10 ? true : false => a4;
		  a1..2 => a5;
		  { a3, a3 } => a6;
		  A.A(a1) => a7;	 
          A.A(a1).x => a8;			  
	  }
    return = { a1, a2, a3, a4, a5, a6, a8 };
}
x = foo ();	
	  
	
";
            string err = "MAGN-4385 Regression with Modifier blocks used inside function definition";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, err);
            Object v1 = null;
            Object[] v2 = new Object[] { 1, 2 };
            Object[] v3 = new Object[] { null, null };
            Object[] v4 = new[] { 1, 0.5, v1, false, v2, v3, 1 };
            thisTest.Verify("x", v4);

        }

        [Test]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T27_Modifier_Stack_Inside_Class()
        {
            string errmsg = "";//1465231 - Sprint 21 : rev 2298 : Modifier stacks are now being allowed in class constructors ";
            string code = @"class B
{
    x : var;
    constructor B ( y )
    {
        x = y;
    }
}
class A
{ 
    a : var;
	a1 : var;
	a2  :var;
	a3 : var;
	a4 : var;
	a5 : var[];
	a6 : var[];
	a7 : var;
	a8 : var;
	
    constructor A ( x : var )
	{
	    a = {
		  x => a1;
		  a1 - 0.5 => a2;
		  a2 * null => a3;
		  a1 > 10 ? true : false => a4;
		  a1..2 => a5;
		  { a3, a3 } => a6;
		  B.B(a1) => a7;	 
          B.B(a1).x => a8;			  
     }
	}
}
a1 = A.A ( 1 );	
x = { a1.a1, a1.a2, a1.a3, a1.a4, a1.a5, a1.a6, a1.a8 };
y = a1.a;
";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object v1 = null;
            Object[] v2 = new Object[] { 1, 2 };
            Object[] v3 = new Object[] { null, null };
            Object[] v4 = new Object[] { 1, 0.5, v1, false, v2, v3, 1 };
            thisTest.Verify("x", v4);
            thisTest.Verify("y", 1);

        }

        [Test]
        [Category("Failure")]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T27_Modifier_Stack_Inside_Class_2()
        {

            // Tracked by: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4385
            string errmsg = "MAGN-4385 Modifier block inside function definition seems to corrupt input argument";
            string code = @"class B
{
    x : var;
    constructor B ( y )
    {
        x = y;
    }
}
class A
{ 
    a : var;
	a1 : var;
	a2  :var;
	a3 : var;
	a4 : var;
	a5 : var[];
	a6 : var[];
	a7 : var;
	a8 : var;
	
    constructor A ( x : var )
	{
	    a = x;			  
	}
	def update ( x : var )
	{
	    a = {
		  x => a1;
		  a1 - 0.5 => a2;
		  a2 * null => a3;
		  a1 > 10 ? true : false => a4;
		  a1..2 => a5;
		  { a3, a3 } => a6;
		  B.B(a1) => a7;	 
          B.B(a1).x => a8;			  
	  }
	  return = x;
	}
}
a1 = A.A ( 0 );	
x = a1.update(1);
y = { a1.a1, a1.a2, a1.a3, a1.a4, a1.a5, a1.a6, a1.a8 };
";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object v1 = null;
            Object[] v2 = new Object[] { 1, 2 };
            Object[] v3 = new Object[] { null, null };
            Object[] v4 = new Object[] { 1, 0.5, v1, false, v2, v3, 1 };
            thisTest.Verify("x", 1);
            thisTest.Verify("y", v4);
        }

        [Test]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T27_Modifier_Stack_Inside_Class_3()
        {
            string errmsg = "";//1465231 - Sprint 21 : rev 2298 : Modifier stacks are now being allowed in class constructors"; 
            string code = @"class B
{
    x : var;
    constructor B ( y )
    {
        x = y;
    }    
    def bfoo(a : double)
    {
        return = a*98;
    }
}
class C
{
    x : int;
    
    constructor C(a : int)
    {
        x = a;
    }    
    def bfoo(a : double)
    {
        return = a*3;
    }
}
class A
{ 
    a : var;
    a1 : var;
    a2 :var;
    a60 : var;
    a61 : var;
    a7 : var;
    a8 : var;
    a9 : var;
    constructor A ( x : var )
    {
        a =
        {
            x => a1;    
            - 0.5 => a2;
            C.C(a1) => a60;
             bfoo(a2) => a61;
             B.B(a1) => a7;
             bfoo(a2) => a8;
             B.B(19890).x => a9; 
        }
}
    
    // Expanding modifier block in foo - foo(1) is found to give the exact same results as A.A(1)
    def foo(x : int)
    {
        a1 = x;            // a1 should be 1
        a = a1;            
        a2 = a1 - 0.5;     // a2 should be 0.5
        a = a2;
        a60 = C.C(a1);    // a60 should be C(x = 1)
        a = a60;
        a61 = a60.bfoo(a2);    // a61 should be 1.5
        a = a61;    
        a7 = B.B(a1);        // a7 should be B(x = 1)
        a = a7;    
        a8 = a7.bfoo(a2);    // a8 should be 49.0
        a = a8;
        a9 = B.B(19890).x;    // a9 should be 19890
        a = a9;                // a should be 19890 in the end
    
        return = { a, a1, a2, a61, a8, a9 };
    }
}
ax = A.A();
//ax = A.A(1);    // this gives the same results as ax.foo(1)
res = ax.foo(1);
// ax should be A(a = 19890, a1 = 1, a2 = 0.5, a60 = C(x = 1), a61 = 1.5, a7 = B(x = 1), a8 = 49.0, a9 = 19890)
xa = ax.a;
xa1 = ax.a1;
xa2 = ax.a2;
xa60 = ax.a60;
xa61 = ax.a61;
xa7 = ax.a7;
xa8 = ax.a8;
xa9 = ax.a9;
";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] v4 = new Object[] { 19890, 1, 0.5, 1.5, 49.0, 19890 };

            thisTest.Verify("res", v4);
        }

        [Test]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T27_Modifier_Stack_Inside_Class_4()
        {
            string errmsg = "";//1465231 - Sprint 21 : rev 2298 : Modifier stacks are now being allowed in class constructors"; 
            string code = @"class B
{
    x : var;
    constructor B ( y )
    {
        x = y;
    }    
    def bfoo(a : double)
    {
        return = a*98;
    }
}
class C
{
    x : int;
    
    constructor C(a : int)
    {
        x = a;
    }    
    def bfoo(a : double)
    {
        return = a*3;
    }
}
class A
{ 
    a : var;
    a1 : var;
    a2 :var;
    a60 : var;
    a61 : var;
    a7 : var;
    a8 : var;
    a9 : var;
    def foo(x : int)
    {
       a =
        {
            x => a1;    
            - 0.5 => a2;
            C.C(a1) => a60;
             bfoo(a2) => a61;
             B.B(a1) => a7;
             bfoo(a2) => a8;
             B.B(19890).x => a9; 
        }
        return = { a, a1, a2, a61, a8, a9 };
    }
}
ax = A.A();
//ax = A.A(1);    // this gives the same results as ax.foo(1)
res = ax.foo(1);
";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] v4 = new Object[] { 19890, 1, 0.5, 1.5, 49.0, 19890 };
            thisTest.Verify("res", v4);
        }

        [Test]
        [Category("SmokeTest")]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T27_Modifier_Stack_Cross_Reference()
        {
            //Assert.Fail("1465319 - sprint 21 : rev 2301 : update issue with modifier stack ");

            string code = @"
a = {
		  1 => a1;
		  b1 + a1 => a2;		  		  
    };
b = {
		  2 => b1;		  		  
    };	  
	  
	
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("a", 3);

        }

        [Test]
        [Category("SmokeTest")]
        [Category("ModifierBlock")] [Category("Failure")]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T27_Modifier_Stack_Update()
        {
            string src = string.Format("{0}{1}", testPath, "T27_Modifier_Stack_Update.ds");
            fsr.LoadAndPreStart(src, runnerConfig);
            ProtoCore.CodeModel.CodePoint cp1 = new ProtoCore.CodeModel.CodePoint
            {
                CharNo = 8,
                LineNo = 9,
                SourceLocation = new ProtoCore.CodeModel.CodeFile
                {
                    FilePath = Path.GetFullPath(src)
                }
            };
            ProtoCore.CodeModel.CodePoint cp2 = new ProtoCore.CodeModel.CodePoint
            {
                CharNo = 2,
                LineNo = 10,
                SourceLocation = new ProtoCore.CodeModel.CodeFile
                {
                    FilePath = Path.GetFullPath(src)
                }
            };
            ProtoCore.CodeModel.CodePoint cp3 = new ProtoCore.CodeModel.CodePoint
            {
                CharNo = 2,
                LineNo = 11,
                SourceLocation = new ProtoCore.CodeModel.CodeFile
                {
                    FilePath = Path.GetFullPath(src)
                }
            };
            fsr.ToggleBreakpoint(cp1);
            ProtoScript.Runners.DebugRunner.VMState vms = fsr.Run();
            thisTest.DebugModeVerification(vms.mirror, "a", 4);

            fsr.ToggleBreakpoint(cp2);
            fsr.Run();
            thisTest.DebugModeVerification(vms.mirror, "a", 5);
            fsr.ToggleBreakpoint(cp3);
            fsr.Run();
            Object n1 = null;
            thisTest.DebugModeVerification(vms.mirror, "a", n1);

            fsr.Run();
        }

        [Test]
        [Category("SmokeTest")]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T27_Modifier_Stack_Update_2()
        {
            //Assert.Fail("1465319 - sprint 21 : rev 2301 : update issue with modifier stack "); 

            string code = @"
a = {
		  1 => a1;
		  a1 + b1 => a2;		  		  
    };
b1 = 2;	
  
	  
	
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 3);
        }

        [Test]
        [Category("SmokeTest")]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T27_Modifier_Stack_Update_3()
        {
            string code = @"
class B
{
    y : var;
    constructor B ( x : var )
    {
        y = x;
    } 
}
class A
{
    x : var;
    constructor A ( y : var )
    {
        x = y;
    }
    
    def foo1 ( )
    {
        return = A.A(x + 1);
    }
    
    def foo2 ( y : var )
    {
        return = B.B( y );
    }
    
    def foo3 ( x1 : A, x2 : B )
    {
        return = A.A ( x1.x + x2.y );
    }
}
y = 1;
a = {
		  A.A(1) => a1;
		  a1.foo1() => a2;
		  a1.foo2( y) => a3;
                  a1.foo3(a2,a3);                 		  
    }
    
z = a.x;
  
	  
	
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("z", 3);
        }

        [Test]
        [Category("SmokeTest")]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T27_Modifier_Stack_Update_4()
        {
            //Assert.Fail("1465319 - sprint 21 : rev 2301 : update issue with modifier stack "); 

            string code = @"
y = 3;
a =
	{
		y + 1 => a1;
		 +1 => a2;
	}
	
b1 = a1;
b2 = a2;
y = 4;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 6);
        }

        [Test]
        [Category("SmokeTest")]
        public void T27_Modifier_Stack_With_Array()
        {
            string code = @"
a =
	{
		{1,2,3,4} => a1;
		a1[0] + 1 => a2; 
	}
	
b1 = a1;
b2 = a;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 1, 2, 3, 4 };

            thisTest.Verify("b1", v1);
            thisTest.Verify("b2", 2);
        }

        [Test]
        [Category("SmokeTest")]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T27_Modifier_Stack_With_Array_2()
        {
            //Assert.Fail("1465319 - sprint 21 : rev 2301 : update issue with modifier stack "); 

            string code = @"
a =
	{
		{ 1, 2, 3, 4} => a1;
		 a [ 0] + 1 => a2;
	}
	
b1 = a1;
b2 = a2;
a = 4;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

        }

        [Test]
        [Category("SmokeTest")]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T27_Modifier_Stack_With_Array_3()
        {
            string code = @"
a =
	{
		{1,2,3,4} => a1; // identify the initial state to modify
		{a1[0]+3, a1[-1]};  // modify selected members [question: can we refer to 'a@initial' in this way 
				     // inside the modifier block where the right assigned variable was created?]
		//+10 // this modification applies to the whole collection
	}
	
b = a;
c = { a1[0], a[-1] };
x = [Imperative]
{
    return = { a[0], a1[1] };
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 4, 4 };
            Object[] v2 = new Object[] { 1, 4 };
            Object[] v3 = new Object[] { 4, 2 };
            thisTest.Verify("b", v1);
            thisTest.Verify("c", v2);
            thisTest.Verify("x", v3);
        }

        [Test]
        [Category("SmokeTest")]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T27_Modifier_Stack_Update_5()
        {
            string code = @"
def foo ( x )
{
    return = x * 2;
}
y = 3;
a =
	{
		foo( y ) + 1 => a1;
		 * 3 => a2;
	}
y = 2;	
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            //Assert.Fail("1465319 - sprint 21 : rev 2301 : update issue with modifier stack ");
            thisTest.Verify("a", 15);
        }

        [Test]
        [Category("SmokeTest")]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T27_Modifier_Stack_With_Array_4()
        {
            //Assert.Fail("1465127 - Sprint 21 : [Design Issue] rev 2294 : Modifier stack syntax not supported in Imperative block ");

            string code = @"
a = { 1, 2, 3, 4 };
a[0] =
	{
		a[0] + 1 => a1;
		a1 * 2 => a2;  
		{ a2, a2};
	}
/*[Imperative]
{
    a[1] =
	{
		a[1] + 1 => a1;
		a1 * 2 => a2;  
		{ a2, a2};
	}
}*/
b = a;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 4, 4 };
            //Object[] v2 = new Object[] { 6, 6 };
            //Object[] v3 = new Object[] { v1, v2, 3, 4 };
            Object[] v3 = new Object[] { v1, 2, 3, 4 };

            thisTest.Verify("b", v3);

        }

        [Test]
        [Category("SmokeTest")]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T27_Modifier_Stack_Update_6()
        {
            string code = @"
class A
{
    x : var;
    constructor A ( y)
    {
        x = y;
    }
}
def foo ( a : A )
{
    return = a.x * 2;
}
y = 3;
a =
	{
		A.A ( y ) => a1;
		foo ( a1 ) => a2;
	}
b = foo ( a1 );
y = 2;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1465319 - sprint 21 : rev 2301 : update issue with modifier stack ");
            thisTest.Verify("a", 4);
            thisTest.Verify("b", 4);
        }

        [Test]
        [Category("SmokeTest")]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T27_Modifier_Stack_With_Inline_Condition()
        {
            string code = @"
class B
{
    b : var;
    constructor B ( y )
    {
        b = y;
    }
}
class A
{ 
    a : var;
    constructor A ( x : var )
    {
        a = x;
    }
}
x1 = 1;
x2 = 0.1;
x3 = true;
x4 = A.A(10);
a = 
{
	x1 > x2 ? true : false => a1;
	x4.a == B.B(10).b ? true : false => a2;
	x1 == x2 ? true : x3 => a3;
	x1 != x2 ? B.B(2).b : A.A( 1).a => a4;		  
};	  
x = a == 2 ? true : false;
	
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1465293 - Sprint 22 - rev 2301 - 323010 check against not equal to null does not give correct results ");
            thisTest.Verify("a1", true);
            thisTest.Verify("a2", true);
            thisTest.Verify("a3", true);
            thisTest.Verify("a4", 2);
            thisTest.Verify("x", true);
        }


        [Test]
        [Category("SmokeTest")]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T27_Modifier_Stack_With_Range_Expr()
        {
            string code = @"
def foo ( x : int[])
{
    return = { x[1], x[0] };
}
class A
{ 
    a : var;
    constructor A ( x : var )
    {
        a = x;
    }
}
x1 = 1;
x2 = 0.1;
x3 = true;
x4 = A.A(10);
a = 
{
	x1 .. x2  => a1;
	x2 .. x1 => a4;
	1 .. x4.a .. 2 => a2;
	a2[0]..a2[2]..#5 => a5;
	x1 == x2 ? false : x1..2 => a3;
        foo ( a3) ;	
};	  
b = 0.0..a[0]..0.5;
	
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { };
            Object[] v2 = new Object[] { 0.1 };
            Object[] v3 = new Object[] { 1, 3, 5, 7, 9 };
            Object[] v4 = new Object[] { 1, 2 };
            Object[] v5 = new Object[] { 2, 1 };
            Object[] v6 = new Object[] { 1, 2, 3, 4, 5 };
            Object[] v7 = new Object[] { 0.0, 0.5, 1.0, 1.5, 2.0 };
            thisTest.Verify("a1", new object[] { 1.0 });
            thisTest.Verify("a4", v2);
            thisTest.Verify("a2", v3);
            thisTest.Verify("a3", v4);
            thisTest.Verify("a5", v6);
            thisTest.Verify("a", v5);
            thisTest.Verify("b", v7);
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
        [Category("SmokeTest")]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T27_Modifier_Stack_With_Self_Updates()
        {
            string code = @"
a = {
    1 => a1;
    +1 => a2;
    +1 => a3;
    +a2 => a4;
    +a3 => a5;
}
	
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 8);
        }

        [Test]
        public void T30_Update_Global_Variables_Class()
        {
            // Assert.Fail("1465812 - Sprint 22 : rev 2362 : Global variables cannot be accessed from class scope ");

            string code = @"
x  = 1;
class A
{
    static y : int;
    constructor A ( x )
    {
        y = x;
    }
    constructor A2 ( x1 )
    {
        y = x + x1;
    }
}
y = A.A(2);
z = y.y;
y1 = A.A2(2);
z1 = y1.y;
x = 3;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("z", 2);
            thisTest.Verify("z1", 3);
        }

        [Test]
        [Category("SmokeTest")]
        public void T30_Update_Global_Variables_Function()
        {
            string code = @"
x  = 1;
class A
{
   x : double;
   constructor A ( x1 )
   {
       x = x1;
   }
   def getx ( )
   {
       x = x + 1;
       return = x ;
   }
}
y = A.A(2);
z1 = y.x;
z2 = y.getx();
z3 = x;
z4 = y.x;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("z1", 3.0);
            thisTest.Verify("z2", 3.0);
            thisTest.Verify("z3", 1);
            thisTest.Verify("z4", 3.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T30_Update_Global_Variables_Imperative_Scope()
        {
            string code = @"
x  = {0,0,0,0};
count = 0;
i = 0;
sum  = 0;
test = sum;
[Imperative]
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
}
y = x;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 0, 1, 2, 3 };

            thisTest.Verify("test", 6);
            thisTest.Verify("x", v1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T31_Defect_1459777()
        {
            string code = @"
class A 
{
    a : var;
	constructor A ( x)
	{
	    a = x;
	}
}
x = 3;
a1 = A.A(x);
b1 = a1.a;
x = 4;
c1 = b1;
// expected : c1 = 4;
// recieved : c1 = 3
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("c1", 4);
        }

        [Test]
        [Category("SmokeTest")]
        public void T31_Defect_1459777_2()
        {
            string err = "1467385 Sprint 27 - rev 4219 - valid update testcase throws cyclic dependancy error";
            string code = @"class A
{
    a : int;	
}
a1 = A.A();
a1.a = 1;
b = a1.a;
a1.a = 2; // expected b = 2; received : b = 1;
";
            thisTest.VerifyRunScriptSource(code, err);
            thisTest.Verify("b", 2);
        }

        [Test]
        [Category("Update")]
        public void T31_Defect_1459777_3()
        {

            string code = @"
class A
{
    a : int;	
}
a1 = A.A();
x = a1.a;
c = [Imperative]
{
    a1.a = 1;
    b = a1.a;
    a1.a = 3; 
	return = b;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            //Assert.Fail("1466071 - Sprint 22 : rev 2396 : Update issue : when a property is updated in imperative scope, it does not update the outer associative scope variable ");

            thisTest.Verify("x", 3);
            thisTest.Verify("c", 1);
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
        [Category("Failure")]
        [Category("Update")]
        public void T31_Defect_1459777_5()
        {
            string code = @"
class A
{
    a : int;	
}
class B 
{
    b : var;
    constructor B ( a : A )
    { 
	    a.a = 2;
		b = a.a + 2;
    }	
	
}
y = A.A();
z = y.a;
x = B.B(y);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1466076 - Sprint 22 : rev 2396 : Update issue : when an instance property is updated inside function/method scope, it does not update the outer associative scope variable ");
            thisTest.Verify("z", 2);
        }

        [Test]
        [Category("Failure")]
        [Category("Update")]
        public void T31_Defect_1459777_6()
        {
            string code = @"
	
class A
{
    a : int;	
}
class B 
{
    b : var;
    constructor B ( a : A )
    { 
	    a.a = 3;
    }	
	def foo ( a : A )
	{
	    a.a = 3;
		return = true;
	}
	
}
y = A.A();
z = y.a;
x1 = B.B( y );
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1466076 - Sprint 22 : rev 2396 : Update issue : when an instance property is updated inside function/method scope, it does not update the outer associative scope variable ");
            thisTest.Verify("z", 3);
        }

        [Test]
        [Category("Update")]
        public void T31_Defect_1459777_7()
        {
            //Assert.Fail("1466085 - Sprint 22 : rev 2396 : Update issue : update not working with range expressions ");

            string code = @"
class A
{
    a : int;	
}
x = A.A();
y = x.a;
y1 = 0..y;
y2 = y > 1 ? true : false;
y3 = 1 < x.a ? true : false;
x.a = 2;
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
        [Category("SmokeTest")]
        public void T31_Defect_1459777_8()
        {
            string code = @"
class A
{
    a : int;	
}
def foo ( x ) 
{
    return  = x + 1;
}
x1 = A.A();
y1 = foo( x1.a );
x1.a = 2;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("y1", 3);

        }

        [Test]
        [Category("SmokeTest")]
        public void T31_Defect_1459777_9()
        {
            string code = @"
class A
{
    a : int;	
}
def foo ( x ) 
{
    return  = x + 1;
}
x1 =  { A.A(), A.A() };
a1 = A.A();
x2 =  { a1.a, a1.a };
y2 = foo ( x2[0] );
y1 = foo ( x1[1].a );
x1[1].a = 2;
a1.a = 2; 
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

        [Test]
        [Category("SmokeTest")]
        public void T33_Defect_1466107()
        {
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
x1 =  { A.A(), A.A() };
a1 = A.A();
x2 =  { a1.a, a1.a };
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
        [Category("Update")]
        public void T33_Defect_1466107_2()
        {
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
x1 =  { A.A(), A.A() };
a1 = A.A();
x2 =  { a1.a, a1.a };
y2 = foo ( x2[0] );
y1 = foo ( x1[1].b );
def foo1 ( t1 : A )
{
    t1.b = 2;
	return = null;
}
def foo2 ( t1 : A )
{
    t1.a = 2;
	return = null;
}
dummy1 = foo1 ( x1[1] );
dummy2 = foo2 ( a1 );
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1459478 - Sprint 17 : Rev 1459 : Issue with Update mechanism when updating a collection inside a function using pass-by-reference");
            thisTest.Verify("y1", 3);
            thisTest.Verify("y2", 3);
        }

        [Test]
        [Category("SmokeTest")]
        public void T34_Defect_DNL_1463327()
        {
            string code = @"
class A
{        
    Pt : double;        
    constructor A (pt : double)            
    {                        
        Pt = pt;            
    }
}
c = 1.0;
x = [Imperative]
{
	c = A.A( c );
	x = c.Pt;
	return = x;
}
// expected : c = A ( Pt = 1.0 ); x = 1.0
// received : System.NullReferenceException: Object reference not set to an instance of an object.
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
	c = A.A( {c[0], c[0]} );
	x = c.Pt;
	return = x;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 0.0, 0.0 };
            Object v2 = null;

            thisTest.Verify("x", v1);
            thisTest.Verify("y", v2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T34_Defect_DNL_1463327_3()
        {
            string error = "1467416 Count returns null if the input argument is single value ";
            string code = @"class A
{        
    Pt : double;        
    constructor A (pt : double)            
    {                        
        Pt = pt;            
    }
}
t = 0.0..3.0;
c = A.A ( t );
c1 = Count ( c );
x = [Imperative]
{
	c = A.A( c[0].Pt );
	x = c.Pt;
	return = x;
}
t = 0.0..2.0;
";
            thisTest.VerifyRunScriptSource(code, error);

            thisTest.Verify("x", 0.0);
            thisTest.Verify("c1", 1);
        }

        [Test]
        [Category("Update")]
        public void T34_Defect_DNL_1463327_4()
        {
            //Assert.Fail("1467245 - Sprint25 : rev 3420 : cyclic dependency with update cases");

            string code = @"
class A
{        
    Pt : double;        
    constructor A (pt : double)            
    {                        
        Pt = pt;            
    }
}
t = 0.0..3.0;
c = A.A ( t );
c = A.A ( c[0].Pt );
c = A.A ( c.Pt );
x = c.Pt;
t = 0.0..1.0;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 0.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T35_Defect_DNL_1463700()
        {
            string errmsg = " 1467318 - Cannot return an array from a function whose return type is var with undefined rank (-2) ";
            string code = @"class A
{        
    x = {1,2,3};        
    def foo()        
    {                
        x[0] = 100;        
    }
}
a = A.A();
t = a.x;
x = a.foo();
";
            thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] v1 = new Object[] { 100, 2, 3 };
            Object v2 = null;
            thisTest.Verify("t", v1);
            thisTest.Verify("x", v2);
        }

        [Test]
        public void T35_Defect_DNL_1463700_2()
        {
            //Assert.Fail("1467194 - Sprint 25 - rev 3207[Regression] Regressions created by array copy constructions ");

            Object[] v1 = new Object[] { 2, 4, 6 };
            string errmsg = "DNL-1467318 Variable declared without type or rank cannot be assigned a collection any more";
            string code = @"class A
{        
    x = {1,2,3};        
    count = 3;
    def foo()        
    {                
       count = 0;
       [Imperative]
       {
           for ( i in x )
	   {
	       x[count] = i*2;
	       count = count + 1;
	   }
        } 
        return = null;        
    }
}
a = A.A();
t1 = a.x;
t2 = a.count;
dummy = a.foo();
t3 = t1;";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);

            //Assert.Fail("1467187 - Sprint24: REGRESSION : rev 3177: When a class collection property is updated, the value if not reflected");

            thisTest.Verify("t1", v1);
            thisTest.Verify("t2", 3);
        }

        [Test]
        [Category("SmokeTest")]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T36_Modifier_Block_Multiple_Updates()
        {
            string code = @"
a = 1;
b = a + 1;
a = { 2 => a1;
      a1 + 1 => a2;
      a2 * 2 ;
    }
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", 7);
        }

        [Test]
        [Category("SmokeTest")]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T36_Modifier_Block_Multiple_Updates_2()
        {
            string code = @"
a = 1;
b = a + 1;
a = { 2 => a1;
      a1 + 1 => a2;
      a2 * 2 ;
    }
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", 7);
        }

        [Test]
        [Category("Update")]
        public void T37_Modify_Collections_Referencing_Each_Other()
        {
            string code = @"
a = {1,2,3};
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
        [Category("SmokeTest")]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T38_Defect_1467059_Modifier_Stack_With_Undefined_Variable()
        {
            string code = @"
a = {
          1 => a1;
          a1 + b1 => a2;    
          +2;                
    };
b1 = 2;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 5);

        }

        [Test]
        [Category("SmokeTest")]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T38_Defect_1467059_Modifier_Stack_With_Undefined_Variable_2()
        {
            string code = @"
def foo ( x )
{
    return = x;
}
a = {
          1 => a1;
          a1 + foo(b1) => a2;    
          +2;                
    };
b1 = 2;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 5);
        }


        [Test]
        [Category("SmokeTest")]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T39_Defect_1465319_Modifier_Stack_Update_Issue()
        {
            string code = @"
a = {
1 => a1; 
a1 + b1 + x=> a2; 
};
b1 = 2;
y1 = 2;
x = {
1 => x1;
x1 + y1 => x2; 
};
y1 = 5;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 9);
        }

        [Test]
        [Category("SmokeTest")]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T40_Defect_1467057_Modifier_Stack_Cross_Update_Issue()
        {
            string code = @"
a = {
          1 => a1;
          b1 + a1 => a2;                    
    };
b = {
          2 => b1;                    
    };";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 3);
        }

        [Test]
        [Category("Update")]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T40_Defect_1467057_Modifier_Stack_Cross_Update_Issue_3()
        {
            //Assert.Fail("1467088 - Sprint23 : rev 2681 : Cross updates across 2 modifier stacks going into infinite loop");

            string code = @"
a = {
          1 => a1;
          b1 + a1 => a2;                    
    };
b = {
          a2 + 2 => b1;                    
    };";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object n1 = null;

            thisTest.Verify("b", n1);
        }

        [Test]
        [Category("Update")]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T40_Defect_1467088_Modifier_Stack_Cross_Update_Issue()
        {
            //Assert.Fail("1467088 - Sprint23 : rev 2681 : Cross updates across 2 modifier stacks going into infinite loop");
            string code = @"
a = {
          1 => a1;
          b1 + a1 => a2;                    
    };
b = {
          a1 + 2 => b1;                    
    };
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 4);
        }

        [Test]
        [Category("Update")]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T40_Defect_1467088_Modifier_Stack_Cross_Update_Issue_2()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
             {
                 string code = @"
a = {
          1 => a1;
          b1 + a1 => a2;                    
    };
[Imperative]
{
	b = {
		  a1 + 2 => b1;                    
	    };
	
}
";
                 ExecutionMirror mirror = thisTest.RunScriptSource(code);
                 thisTest.Verify("a", 4);
             });
        }

        [Test]
        [Category("Update")]
        [Category("Failure")]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T40_Defect_1467088_Modifier_Stack_Cross_Update_Issue_3()
        {
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4087
            string code = @"
a = {
          1 => a1;
          b1 + a1 => a2;                    
    };
b;
[Associative]
{
	b = {
		  a1 + 2 => b1;                    
	    };
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object n1 = null;
            thisTest.Verify("a", 4);
            thisTest.Verify("b", 3);
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
        public void T41_Defect_1467072_Class_Update_2()
        {
            string code = @"
class A 
{
    a : int;
    constructor A ( a1:int)
    {
        b1 = foo(a1) + 1;
        a = b1+1;
	b1 = foo(a1);
    }
}
def foo ( a1 : int)
{
    a = b + 1;
    b = a1;
    return  = a ;
}
x = A.A(2);
y = x.a;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("y", 4);
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
[Associative]
{
def foo : int ( a : int, b : int )
{
	a = a + b;
	b = 2 * b;
	return = a + b;
}
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
        [Category("Update")]
        public void T44_Defect_1457029()
        {
            string code = @"
class A
{
    Pt : double;
    constructor A (pt : double)
    {
        Pt = pt;
    }
    
    
}
    
c1 = { { 1.0, 2.0}, 3.0 };
c1 = A.A( c1[0] );
x = c1.Pt;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", new Object[] { 1.0, 2.0 });
        }

        [Test]
        [Category("Update")]
        public void T44_Defect_1457029_2()
        {
            string code = @"
class A
{
    Pt : double;
    constructor A (pt : double)
    {
        Pt = pt;
    }
    
    
}
    
c1 = { { 1.0, 2.0}, 3.0 };
c1 = A.A( c1[0][0] );
x = c1.Pt;
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
a1 = { 1, 2};
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
a1 = { 1, 2};
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
a1 = { 1, 2 };
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
A = {1,2,3,4};         // redefine A as a collection
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
        [Category("Update")]
        public void T46_Defect_1467245_2()
        {
            String code =
 @"
class A
{        
    Pt : double;        
    constructor A (pt : double)            
    {                        
        Pt = pt;            
    }
}
t = 0.0..3.0;
c = A.A ( t );
x = c.Pt;
c = A.A ( c[0].Pt );
c = A.A ( c.Pt ); 
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
a = {0,1,2};
t = {10,11,12};
a[0] = t[0];
t[1] = a[1]; 
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", new Object[] { 10, 1, 2 });
        }

        [Test]
        [Category("Update")]
        public void T46_Defect_1467275_2()
        {
            String code =
 @"
class A
{
    a : int[];
    t : int[];
    constructor A()
    {
        a = {0,1,2};
        t = {10,11,12};
        a[0] = t[0];
        t[1] = a[1]; 
    }
}
a1 = A.A();
a = a1.a;
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", new Object[] { 10, 1, 2 });
        }

        [Test]
        [Category("Update")]
        public void T46_Defect_1467275_3()
        {
            String code =
 @"
class A
{
    a : int[];
    t : int[];
    constructor A()
    {
        a = {0,1,2};
        t = {10,11,12};        
    }
    def foo ()
    {
        a[0] = t[0];
        t[1] = a[1]; 
        return = null;
    }
}
a1 = A.A();
dummy = a1.foo();
a = a1.a;
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", new Object[] { 10, 1, 2 });
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
x = {1,2,3};
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
a = {10,11,12};
t = 0;
i = a[t];
t = 2; // expected i = 12
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("i", 12);
        }

        [Test]
        [Category("SmokeTest")]
        public void T49_Defect_1461985()
        {
            String code =
 @"
class A
{
x:int = 3; 
}
a = A.A();
b1 = a.x;
c = [Imperative]
{
a.x = 4;
return = a.x;
} 
";
            string errmsg = "";//1467385: Sprint 27 - rev 4219 - valid update testcase throws cyclic dependancy error ";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("c", 4);
            thisTest.Verify("b1", 4);
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
        [Category("SmokeTest")]
        public void T49_Defect_1461985_3()
        {
            String code =
 @"
class A
{
    x:int = 3; 
}
a = A.A();
totalLength = a.x;
i = 0;
[Imperative]
{
    while (a.x < 5) 
    {
        [Associative] 
        {
            a.x = a.x + 1;
            i = i + 1;
        }                              
    }
}
";
            //Assert.Fail("Suspected of crashing NUnit");
            string errmsg = "";//1461985 - Sprint 19 : Rev 1880 : Cross Language Update issue : When class properties are updated in different blocks the update is not happening as expected";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("totalLength", 5);
            thisTest.Verify("i", 2);

        }

        [Test]
        [Category("SmokeTest")]
        public void T49_Defect_1461985_4()
        {
            String code =
 @"
class A
{
}
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
			[Imperative] // within the associative model start some imperative scripting
			{
				for (j in a) 
				{
					if(j%2==0) 
					{
						a[j] = 0;
					}
				}
			}		
			b = a + 1;
			totalLength  = totalLength +1;
		}
		i = i + 1; 
	}
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
        [Test, Ignore]
        [Category("SmokeTest")]
        [Category("Failure")]
        public void T51_Defect_1461388()
        {
            // Tracked by: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4094
            String code =
 @"
b = 2;
a = { 0, 1, 2 };
c = 0;
[Imperative]
{
    d = a + 1;
    [Associative]
    {
        b = a  + 1;
        a = { c, c } ;
        [Imperative]
        {
            for ( i in a )
            {
                a[c] = i + 1;
                c = c + 1;
            }            
        }
    }
}
c = 10;
";
            string errmsg = "MAGN-4094 Runtime Cyclic Dependency not detected";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.Runtime.WarningID.kCyclicDependency);
            
        }

        [Test]
        [Category("SmokeTest")]
        public void T52_Defect_1459478()
        {
            String code =
@"
class A
{
    x :var[];
    constructor A (x1:var[])
    {
        x1 = x1 + 1;
        x = x1;
    }   
}
a = 0..4..1;
x = A.A(a);
test = x.x;
";
            string errmsg = "1467309 - rev 3786 : Warning:Couldn't decide which function to execute... coming from valid code";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test", new Object[] { 1, 2, 3, 4, 5 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T52_Defect_1459478_2()
        {
            String code =
@"
class A
{
    x :var[];
    constructor A (x1:var[])
    {
        x1 = x1 + 1;
        x = x1;
    }
    def foo ( a : var[] ) 
    {
        a[0] = a[0] + 1;
        return = a;
    }
}
a = 0..4..1;
x = A.A(a);
test = x.x;
y = x.foo (a );
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", new Object[] { 0, 1, 2, 3, 4 });
            thisTest.Verify("test", new Object[] { 1, 2, 3, 4, 5 });
            thisTest.Verify("y", new Object[] { 1, 1, 2, 3, 4 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T53_Defect_1467086()
        {
            String code =
@"
class A
{
    x :var[];
    constructor A (x1:var[])
    {
        
        x = x1;
    }
}
a = 0..4..1;
x = A.A(a);
test1 = x.x;
test2 = x.x;
test1 = test1 + 1;
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test2", new Object[] { 0, 1, 2, 3, 4 });
            thisTest.Verify("test1", new Object[] { 1, 2, 3, 4, 5 });

        }

        [Test]
        [Category("SmokeTest")]
        public void T53_Defect_1467086_2()
        {
            String code =
@"
class A
{
    x :var[];
    constructor A (x1:var[])
    {
        
        x = x1;
    }
}
a = 0..4..1;
x = A.A(a);
test1 = x.x;
a = a + 1;
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("test1", new Object[] { 1, 2, 3, 4, 5 });
        }

        [Test]
        [Category("SmokeTest")]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T54_Defect_1467185_Modifier_Stack()
        {
            string errmsg = "";
            string code = @"class B
{
    x : var;
    constructor B ( y )
    {
        x = y;
    }    
    def bfoo(a : double)
    {
        return = a*98;
    }
}
class C
{
    x : int;
    
    constructor C(a : int)
    {
        x = a;
    }    
    def bfoo(a : double)
    {
        return = a*3;
    }
}
x = 1;
a =
{
    x => a1;
    - 0.5 => a2; // equivalent to a1 - 0.5 or in general (previous state) - 0.5
    * 4 => a3; // equivalent to a2 * 4 or in general (previous state)times 4
    a1 > 10 ? true : false => a4;
    a1..2 => a5;
    { a3, a3 } => a6;
     C.C(a1);
     bfoo(a2) => a61; // bfoo method of class C called
     B.B(a1) => a7;
     bfoo(a2) => a8; // bfoo method of class B called
     B.B(a1).x => a9; 
}";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a1", 1);
            thisTest.Verify("a2", 0.5);
            thisTest.Verify("a3", 2.0);
            thisTest.Verify("a4", false);
            thisTest.Verify("a5", new Object[] { 1, 2 });
            thisTest.Verify("a6", new Object[] { 2.0, 2.0 });
            thisTest.Verify("a8", 49.0);
            thisTest.Verify("a9", 1);
            thisTest.Verify("a", 1);
        }

        [Test]
        [Category("SmokeTest")]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T54_Defect_1467185_Modifier_Stack_2()
        {
            string errmsg = "DNL-1467375 Sprint 27 - Rev 4127 - in the atatched example the result is expected to be zipped .";
            string code = @"class B
{
    x : var;
    constructor B ( y )
    {
        x = y;
    }    
    def bfoo(a : double)
    {
        return = a+1;
    }
}
def foo ( x : double)
{
    return = x + 1;
}
x = 1;
a =
{
    {0,1,2,3 } => a1;
    + 0.5 => a2; 
    0..2 => a3; 
    foo ( a1[a3] ) => a4;
	B.B(a1).bfoo(a4) => a5;
    B.B(a1).bfoo(foo ( a1[a3] ) ) => a6;
}
a7 = B.B(a1).bfoo(a4); // works fine
a8 = B.B(a1).bfoo(foo ( a1[a3] ) ); // works fine";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a1", new Object[] { 0, 1, 2, 3 });
            thisTest.Verify("a2", new Object[] { 0.5, 1.5, 2.5, 3.5 });
            thisTest.Verify("a3", new Object[] { 0, 1, 2 });
            thisTest.Verify("a4", new Object[] { 1.0, 2.0, 3.0 });
            thisTest.Verify("a5", new Object[] { 2.0, 3.0, 4.0 });
            thisTest.Verify("a6", new Object[] { 2.0, 3.0, 4.0 });
            thisTest.Verify("a7", new Object[] { 2.0, 3.0, 4.0 });
            thisTest.Verify("a8", new Object[] { 2.0, 3.0, 4.0 });

        }

        [Test]
        [Category("SmokeTest")]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T54_Defect_1467185_Modifier_Stack_3()
        {
            string errmsg = "";//DNL-1467185 Modifier Blocks: Support to be added for exclusion of explicit variable prefixing of instance method/property/operator calls";
            string code = @"class B
{
    x : var;
    constructor B ( y )
    {
        x = y;
    }    
    def bfoo(a : double)
    {
        return = a+1;
    }
}
def foo ( x : int, y: int)
{
    return = x + y;
}
x = { 1,2,3,4};
a =
{
    {0,1 } => a1;
	2..3 => a2;
    foo( a1<1>, a2<2> ) => a3; 
    x[a2] => a5;
	+x[a1] => a6;
	(0..2) => a4; 
     + 1;	
}
";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a3", new Object[] { new Object[] { 2, 3 }, new Object[] { 3, 4 } });
            thisTest.Verify("a4", new Object[] { 0, 1, 2 });
            thisTest.Verify("a5", new Object[] { 3, 4 });
            thisTest.Verify("a6", new Object[] { 4, 6 });
            thisTest.Verify("a", new Object[] { 1, 2, 3 });
        }

        [Test]
        [Category("SmokeTest")]
        [Category("ModifierBlock")] [Category("Failure")]
        public void T54_Defect_1467185_CrossLinked_Modifier_Blocks()
        {
            String code = @"
            a1 = 
            {
                    5 => a1@initial;
                     + b1@initial => a1@subsequent;
                     + 2;
            }
            b1 = 
            {
			        a1@initial => b1@initial;
                     + a1@subsequent => b1@subsequent;
                     + 3;
            }";
            string errmsg = "DNL-1467185 Modifier Blocks: Support to be added for exclusion of explicit variable prefixing of instance method/property/operator calls";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a1@initial", 5);
            thisTest.Verify("a1@subsequent", 10);
            thisTest.Verify("b1@initial", 5);
            thisTest.Verify("a1", 12);
            thisTest.Verify("b1@subsequent", 15);
            thisTest.Verify("b1", 18);
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
points = {p1,p2};
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
a = { 1, 2};
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
a1.a = {1,2};
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
a1.a = {1,2};
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
a1.a = {1,2};
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
def foo()
{
    a = 1;
    return = a + 1;
}
a = 2;
b = a + 1;
c = foo();
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            thisTest.Verify("c", 2);
            thisTest.Verify("b", 2);

        }

        [Test]
        [Category("SmokeTest")]
        public void T59_Defect_1467418_Update_Across_Language_Blocks()
        {
            string errmsg = "";
            string code = @"class A
{
    def _Dispose()
    {
        return = ""A.Dispose()"";
    }
}
i = 0;
[Imperative]
{
    while (i < 2)
    {
        [Associative]
        {
            as = {A.A(), A.A()};
            [Imperative]
            {
                c = 0;
                k = null;
                tmp = as;
                while (c < 2)
                {
                   k = as[c];
                   as[c] = A.A();
                   c = c + 1;
                }
            }
        }
		i = i + 1;
    }
}";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);

            thisTest.Verify("i", 2);

        }

        [Test]
        [Category("SmokeTest")]
        public void T60_Defect_14672007_Update_In_Class_From_Imperative_Scope()
        {
            String code = @"
class A
{
    a : int;
    constructor A ( a1 : int )
    {
    a = a1; 
}
}
x = { 1, 2 };
y1 = A.A(x);
y2 = { y1[0].a, y1[1].a };
[Imperative]
{
    count = 0;
    for ( i in 0..1)
    {
        //i.a = 0;
        x[count] = x[count] + 1;
        count = count + 1;
    }
}
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);

            thisTest.Verify("y2", new Object[] { 2, 3 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T60_Defect_14672007_Update_In_Class_From_Imperative_Scope_2()
        {
            String code = @"
class A
{
    a : int;
    constructor A ( a1 : int )
    {
    a = a1; 
}
}
x = { 1, 2 };
y1 = A.A(x);
y2 = { y1[0].a, y1[1].a };
[Imperative]
{
    for ( i in y1)
    {
        i.a = 0;        // Modifying  the array y1 will re-execute y2
    }
}
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("y2", new Object[] { 0, 0 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T60_Defect_14672007_Update_In_Class_From_Imperative_Scope_3()
        {
            String code = @"
class A
{
    a : int;
    constructor A ( a1 : int )
    {
    a = a1; 
    }
    def update ( a2 : int )
    {
        a = a2;
        return = true;
    }
}
x = { 1, 2 };
y1 = A.A(x);
y2 = { y1[0].a, y1[1].a };
[Imperative]
{ 
    count = 0;
    for ( i in y1)
    {
        temp = y1[count].update(0); 
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
        public void T61_Defect_1467410_Update_In_Class_Properties()
        {
            String code = @"
class Point
{
    X : double;
    Y : double;
    Z : double;    
    constructor ByCartesianCoordinates( x : double, y : double, z : double )
    {
        X = x;
		Y = y;
		Z = z;
    }     
    def Translate( x1 : double, y1 : double, z1 : double )
    {
    	return = Point.ByCartesianCoordinates( X + x1, Y + y1, Z + z1 );
    }
}
a = Point.ByCartesianCoordinates(3, 4, 0);
x1 = a.X;
y1 = a.Y;
z1 = a.Z;
a = a.Translate(0, 0, 5);
x2 = a.X;
y2 = a.Y;
z2 = a.Z;
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("x1", 3.0);
            thisTest.Verify("y1", 4.0);
            thisTest.Verify("z1", 5.0);
            thisTest.Verify("x2", 3.0);
            thisTest.Verify("y2", 4.0);
            thisTest.Verify("z2", 5.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T61_Defect_1467410_Update_In_Class_Properties_2()
        {
            String code = @"
class Point
{
    X : double;
    Y : double;
    Z : double;    
    constructor ByCartesianCoordinates( x : double, y : double, z : double )
    {
        X = x;
		Y = y;
		Z = z;
    }     
    def Translate( x1 : double, y1 : double, z1 : double )
    {
    	X = X + x1;
        Y = Y + y1;
        Z = Z + z1;
        return = null;
    }
}
a = Point.ByCartesianCoordinates(3, 4, 0);
x1 = a.X;
y1 = a.Y;
z1 = a.Z;
dummy = a.Translate(0, 0, 5);
x2 = a.X;
y2 = a.Y;
z2 = a.Z;
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("x1", 3.0);
            thisTest.Verify("y1", 4.0);
            thisTest.Verify("z1", 5.0);
            thisTest.Verify("x2", 3.0);
            thisTest.Verify("y2", 4.0);
            thisTest.Verify("z2", 5.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T62_update_wrongsequnce_1467484()
        {
            String code = @"
x = 1;
g = 10;
def f()
{
    g = g + 1;
    return = g;
}
a = x; 
c = f(); 
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
    
def foo()
{
a = 1;
b = 2;
c = 3;
m = a + b + c;
    return =true;
}
z = foo();
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
    
def foo()
{
a = 1;
b = a;
c = b;
    return =true;
}
z = foo();
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
a = {  1, 2,3 };
a = {  0, 2,4 };
b = { 1, 2, 3 };
c = 1;
a = b[c] + 1;
c = {1,2};
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
def foo()
{
    g = g + 1;
    return = g;
}
g = 1;
c = foo();
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
        [Category("SmokeTest")]
        public void T62_update_wrongsequnce_1467484_9()
        {
            String code = @"
class Point
{
    X : double;
    Y : double;
    Z : double;
    constructor ByCoordinates(x : double, y : double, z : double)
    {
        X = x;
        Y = y;
        Z = z;
    }
}
c = 0;
x = c > 5 ? 1 : 2;
[Imperative]{ c = 10; }
a = Point.ByCoordinates(10, 20, 30);
b = a;
c = Print(b);
[Imperative]{ a = { 1, 2, 3, 4, { 5, { 6, { 7, { 8.9 } } } } }; }
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
        public void T62_update_wrongsequnce_1467484_10()
        {
            String code = @"
a;
b;
c;
m;
n;
def foo()
{
a = 1;
b = a;
c = b;
return =true;
}
z = foo();
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", 1);
            thisTest.Verify("b", 1);
            thisTest.Verify("c", 1);
            thisTest.Verify("z", true);

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
def foo()
{
    a = 1;
    b = a;
    c = b;
    m = a + b + c;
    n = m;
    a = 2;
    return =true;
}
z = foo();
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", 2);
            thisTest.Verify("b", 2);
            thisTest.Verify("c", 2);
            thisTest.Verify("m", 6);
            thisTest.Verify("n", 6);
            thisTest.Verify("z", true);
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
        [Category("SmokeTest")]
        public void T62_update_wrongsequnce_1467484_13()
        {
            String code = @"
class test
{
    a;
    b;
    c;
    m;
    n;
    def foo()
    {
        a = 1;
        b = a;
        c = b;
        m = a + b + c;
        n = m;
        a = 2;
        return =true;
    }
}
    z = test.test();
    y = z.foo();
    a = z.a;
    b = z.b;
    c = z.c;
    m = z.m;
    n = z.n;
   
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", 2);
            thisTest.Verify("b", 2);
            thisTest.Verify("c", 2);
            thisTest.Verify("m", 6);
            thisTest.Verify("n", 6);
            thisTest.Verify("y", true);
        }

        [Test]
        [Category("SmokeTest")]
        public void T62_update_wrongsequnce_1467484_14()
        {
            String code = @"
a;
b;
c;
m;
n;
def foo()
{
    a = 1;
    b = a;
    c = b;
    m = a + b + c;
    n = m;
    a = 2;
    return =true;
}
z = foo();
   
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
        public void T62_update_wrongsequnce_1467484_15()
        {
            String code = @"
                        a;
                        b;
                        c;
                        m;
                        n;
                        def foo()
                        {
                            a = 1;
                            b = a;
                            c = b;
                            m = a + b + c;
                            n = m;
                            a = 2;
                            return =true;
                        }
                        z = foo();
   
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
b;
[Imperative]
{
    a = 1;
    [Associative]
    {
        b = a;
        [Imperative]
        {
            c = a;
            a = {2};
        }
    }
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
b;
[Imperative]
{
    a = 1;
    [Associative]
    {
        b = a;
        [Imperative]
        {
            c = a;
            a = {2};
        }
    }
}
";
            string errmsg = "1467501- Infinite loop for this script";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", new object[] { 2 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T63_NoInfiniteLoop_1467501_2()
        {
            String code = @"
[Imperative]
{
    a = 1;
    [Associative]
    {
        b = a;
        [Imperative]
        {
            c = a;
            d = b;
            a = { 2 };
        }
    }
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
        [Category("SmokeTest")]
        public void T64_1467161_Update_ssie_with_class_member_call_1()
        {
            String code = @"
class A
{
        fx:int;
        def foo(x:var)
        {
                fx = x +10;
                return = fx;
        }
}
class B extends A
{
        fy:var;
        constructor B(): base.A()
        {
                fx = 10;
                fy = 20;
        }
}
b = B.B();
r1 = b.fx;
r2 = b.fy; 
r3 = b.foo(1);//after boo is called, r2 is updated, which is not expected
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("r1", 11);
            thisTest.Verify("r3", 11);
        }

        [Test]
        [Category("SmokeTest")]
        public void T64_1467161_Update_issue_with_class_member_call_2()
        {
            String code = @"
class A
{
        fx:int;
        def foo(x:var)
        {
                fx = x + 10;
                return = fx;
        }
}
class B extends A
{
        fy:var;
        constructor B(): base.A()
        {
                fx = 10;
                fy = 20;
        }
}
b = B.B();
r1 = b.fx;
r2 = b.fy; 
r3 = b.foo(1);//after boo is called, r2 is updated, which is not expected
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("r1", 11);
            thisTest.Verify("r3", 11);
        }

        [Test]
        [Category("SmokeTest")]
        public void T65_1467495_FalseCyclicDependancy()
        {
            String code = @"
class Point
{
    X : double;
    Y : double;
    NextPoint: Point;
        
    constructor Point( p : Point)
    {
        NextPoint = p;
    }
    
    def XPlusY()
    {
        x = NextPoint.X;
        y = NextPoint.Y;
        temp = x + y;
        return = temp;
    }
}
pt1 = Point.Point();
pt2 = Point.Point(pt1);
pt3 = pt2.XPlusY();
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("pt3", 0.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T65_1467495_FalseCyclicDependancy_2()
        {
            String code = @"
class Point
{
    X : double;
    Y : double;
    NextPoint: Point;
        
    constructor Point( p : Point)
    {
        NextPoint = p;
    }
    
    def XPlusY()
    {
        x = NextPoint.X;
        y = NextPoint.Y;
        temp = x + y;
        return = temp;
    }
}
pt1;
pt2;
pt3;
[Imperative]
{
pt1 = Point.Point();
pt2 = Point.Point(pt1);
pt3 = pt2.XPlusY();
}
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("pt3", 0.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T65_1467495_FalseCyclicDependancy_3()
        {
            String code = @"
class Point
{
    X : double;
    Y : double;
    NextPoint: Point;
        
  
    
    def XPlusY(p:Point)
    {
        NextPoint = p;
        x = NextPoint.X;
        y = NextPoint.Y;
        temp = x + y;
        return = temp;
    }
}
pt1;
pt3;
[Imperative]
{
pt1 = Point.Point();
pt3 = pt1.XPlusY(pt1);
}
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("pt3", 0.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T65_1467495_FalseCyclicDependancy_4()
        {
            String code = @"
class Point
{
    X : double;
    Y : double;
    NextPoint: Point;
        
   
    
    def XPlusY(p:Point)
    {
        NextPoint = p;
        x = NextPoint.X;
        y = NextPoint.Y;
        temp = x + y;
        return = temp;
    }
}
pt1;
pt3;
pt1 = Point.Point();
pt3 = pt1.XPlusY(pt1);
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("pt3", 0.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T65_1467495_FalseCyclicDependancy_5()
        {
            String code = @"
class Point
{
    X : double;
    Y : double;
    NextPoint: Point;
        
    
    def XPlusY(p:Point)
    {
        NextPoint = p;
        x = NextPoint.X;
        y = NextPoint.Y;
        temp = x + y;
        return = temp;
    }
}
pt1;
pt3;
pt1 = Point.Point();
pt2 = Point.Point();
pt3 = pt1.XPlusY(pt1);
pt3 = pt1.XPlusY(pt2);
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("pt3", 0.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T66_1467483_CyclicDependancy()
        {
            String code = @"
                a = 1;
                b = a;
                def foo(x)
                {
                a = b+1;
                return = a;
                }
                r = foo(b);
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);

            thisTest.VerifyRuntimeWarningCount(1);

        }

        [Test]
        [Category("SmokeTest")]
        public void T66_1467483_CyclicDependancy_2()
        {
            String code = @"
            [Imperative]
            {
                a = 1;
                b = a;
                def foo(x)
                {
                a = b+1;
                return = a;
                }
                r = foo(b);
            }
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);

        }

        [Test]
        [Category("SmokeTest")]
        public void T66_1467512_RighthandsideUpdate_imperative()
        {
            String code = @"
            a = 1 ;
                b;
                [Imperative]
                {
                   c = 3;
                   i = 0;
                    c = { 1, 2, 3 };
                    for(i in c)
                    {
                        b = a;
                    }
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
                b;
                [Imperative]
                {
                   c = 3;
                   i = 0;
    
                   while(i < c)
                    {
                        b = a;
                        i = i + 1;
                    }
                  
                }
                a = 2;
                ";
            string errmsg = "1467512 - a variable declared in associative must trigger update when modified in imperative , it doesnt if inside for or while ";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("b", 2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T66_1467512_RighthandsideUpdate_imperative_3()
        {
            String code = @"
          a = 1 ;
            b;
            [Imperative]
            {
               c = 3;
               i = 0;
    
            def foo(a:int)
            {
                    b = a;
                    i = i + 1;
            }
                c = foo(a);
 
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
            b;
            [Imperative]
            {
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
            b;
            [Imperative]
            {
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
            b;
            [Imperative]
            {
               c = 3;
               i = 0;
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
 [Imperative]
 {
    d = a;
   
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
        public void T69_Cyclic_globalvariable_1467439()
        {
            String code = @"
a;
b;
q;
[Imperative]
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
        [Category("SmokeTest")]
        public void T71_Cyclic_globalvariable_1467439()
        {
            String code = @"
class test
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
}
a = 1;
b = 0;
q = a;
s = test.test();
r = s.bar();
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
b;
[Imperative]
{
    b = a;
    [Associative]
    {
           a = 2;
        // expect b = 2, but b = 1
    }
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
b;
[Imperative]
{
    [Associative]
    {
           b = a;
    }
}
[Associative]
{
    [Imperative]
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
        [Category("Failure")]
        public void T74_TestUpdate_1467533()
        {
            String code = @"
class A
{
    x;
    constructor A(i) { x = i;}
    def modify(i)  { x = i; }
}
a = A.A(17);
t = a.x;
[Imperative]
{
    r = a.modify(41);
}
";
            //Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4085
            string errmsg = "MAGN-4085: when Property of class is modified using a method in Imperative, does not trigger update of the variable where it is used";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("t", 41);
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
a;b;
[Imperative]
{
a = 1;
b = 2;
    b = b + a;
    a = 10;
    //expected 13 , received - 12
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
[Imperative]
{
    b = b + a;
  
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
[Imperative]
{
    b = b + a;
  
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
        [Category("SmokeTest")]
        public void T75_TestUpdate_1467536()
        {
            String code = @"
class C {
    x=0;
    def f(p : C)
    {
        return = p.x+1;
    }
    def g()
    {
        return = C.C();
    }
};
p1 = C.C();
p2 = C.C();
j=0;
i = [Imperative]
{
    j = j + 1;
    return = p1.f(p2);
}
p2 = p2.g();
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
            [Imperative]
            {
               b = a;
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
[Imperative]
{
 
    [Associative]
    {
        b = a;
    }
 
        if (d)
    {
        z = 1;
    }
}
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
[Imperative]
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
        [Category("Failure")]
        public void T84_TestUpdate_Crosslangauge_1467513_3()
        {
            String code = @"
a = 1;
b;
d;
[Imperative]
{
    b = a;
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
        [Category("Failure")]
        public void T84_TestUpdate_Crosslangauge_1467513_4()
        {
            String code = @"
a = 1;
b;
d;
[Imperative]
{
    b = a;
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
        [Category("SmokeTest")]
        public void T85_1467452_update_using_class_methods()
        {
            String code = @"
class A
{
    x : var;
    def foo : int ( a : int )
    {
        x = a ;
        return = x;
    }
    
}
a1 = A.A();
test = a1.x;
x1 = 3;
y = a1.foo( x1 ); 
[Imperative]
{
    x1 = 4;
}
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("y", 4);
            thisTest.Verify("test", 4);
        }

        [Test]
        [Category("SmokeTest")]
        public void T85_1467452_update_using_class_methods_2()
        {
            String code = @"
class A
{
    x : var;
    def foo : int ( a : int )
    {
        x = a ;
        return = x;
    }
}
x1 = 3;
y = 0;
test = 0;
[Imperative]
{
    a1 = A.A();    
    y = a1.foo( x1 ); 
    test = a1.x;
}
x1 = 4;
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("y", 4);
            thisTest.Verify("test", 4);
        }

        [Test]
        [Category("SmokeTest")]
        public void T85_1467452_update_using_class_methods_3()
        {
            String code = @"
class A
{
    x : var;
    def foo : int ( a : int )
    {
        x = a ;
        return = x;
    }
}
x1 = 3;
y = 0;
[Associative]
{
    a1 = A.A();
    test = a1.x;
    y = a1.foo( x1 ); 
}
x1 = 4;
";
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1506
            string errmsg = "MAGN-1506 [Design issue]Update on the inner associatve block is not triggered";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("y", 4);
        }

        [Test]
        [Category("SmokeTest")]
        [Category("Failure")]
        public void T85_1467452_update_using_class_methods_4()
        {
            String code = @"
class A
{
    x : var;
    def foo : int ( a : int )
    {
        x = a ;
        return = x;
    }
}
y;
[Associative]
{
    x1 = 3;
    y = x1;
    [Imperative]
    {
        a1 = A.A();
        test = a1.x;
        y = a1.foo( x1 ); 
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
        [Category("SmokeTest")]
        public void T85_1467452_update_using_class_methods_5()
        {
            String code = @"
class A
{
    x : var;
    def foo : int ( a : int )
    {
        x = a ;
        return = x;
    }
}
y;
x1 = 3;
[Imperative]
{
    a1 = A.A();
    if( x1 == 3)
    {
        y = a1.foo(x1);
    }
    else
    {
        y = a1.foo(x1+1);
    }
}
x1 = 4;
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("y", 5);
        }

        [Test]
        [Category("SmokeTest")]
        public void T85_1467452_update_using_class_methods_6()
        {
            String code = @"
class A
{
    x : var;
    def foo : int ( a : int )
    {
        x = a ;
        return = x;
    }
}
y= 0;
x1 = 3;
[Imperative]
{
    a1 = A.A();
    for(i in 0..1)
    {
        y = y + a1.foo(x1);
    }
}
x1 = 4;
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("y", 14);
        }

        [Test]
        [Category("SmokeTest")]
        public void T85_1467452_update_using_class_methods_7()
        {
            String code = @"
class A
{
    x : var;
    def foo : int ( a : int )
    {
        x = a ;
        return = x;
    }
}
y= 0;
x1 = 3;
[Imperative]
{
    a1 = A.A();
    i = 1;
    while(i <= 2)
    {
        y = y + a1.foo(x1);
        i = i+1;
    }
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
 
            a = { 10, 20, 30 };
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
      	[Imperative]
      	{
			b = 10;
			[Associative]
			{
          		x = a;
			}
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
        public void T88_1461985_Update_In_Nested_Blocks_2()
        {
            string code = @"
class B
{
    y;
    constructor B ( y1)
    {
        y = y1;
    }
}
class A
{
    x;
    def foo ( y:double )
    {
        return = B.B(y);
    }
}
a1  = null; // this kind of declaration is causing b1 = null. However, if I keep it as 'a1;', and 'b1;', I am getting expected output for 'b1'
b1 : B[] = null; // 
i = 0;
[Imperative]
{
    while ( i <= 2 )
    {
        [Associative] // within that loop build an associative model
        {
            a1 = A.A();
            b1 = a1.foo(0..i); // received : null
        }
        i = i + 1;
    }
}
test = b1.y;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] b = new Object[] { 0.0, 1.0, 2.0 };
            thisTest.Verify("test", b);
        }

        [Test]
        [Category("SmokeTest")]
        public void T89_1467414_cyclic()
        {
            String code = @"
 
         a; x;
            a = 1;
            b = a + 1;
            d;
            [Imperative]
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
            }
            f = a + b;
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.Runtime.WarningID.kCyclicDependency);
        }

        [Test]
        [Category("SmokeTest")]
        public void T90_1467510_cyclic()
        {
            String code = @"
 
         class A
            {
                x1;
                y1;
                z1;
                constructor A(x, y, z)
                {
                    x1 = x;
                    y1 = y;
                    z1 = z;        
                }
            }
            class B
            {
                x1;
   
                constructor B(x:A)
                {
                    x1 = x;
        
              
                }
            }
            a = 1;
            b = a + 1;
            c = a + b;
            n = A.A( a, b, c );
            m = B.B(n);
            a = 3;
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.VerifyRuntimeWarningCount(0);
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
        [Category("Failure")]
        public void T91_1467547()
        {
            String code = @"
 
        def foo()
        {
            return = a + 7;
        }
        def bar()
        {
            return = 3;
        }
        def ding()
        {
            return = a < 100? foo(): bar();
        }
        a = 10;
        t = ding();
        a = 50;
";
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1502
            string errmsg = "MAGN-1502: Function pointer doesn't get update";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("t", 57);
        }

        [Test]
        [Category("SmokeTest")]
        [Category("Failure")]
        public void T91_1467547_2()
        {
            String code = @"
 
        def foo()
        {
            return = a + 7;
        }
        def bar()
        {
            return = 3;
        }
        def ding()
        {
            return = a < 100? foo: bar;
        }
        a = 10;
        t = ding();
        z=t()
        a = 50;
";
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1502
            string errmsg = "MAGN-1502: Function pointer doesn't get update";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("t", 57);
        }

        [Test]
        [Category("SmokeTest")]
        public void T91_1467547_3()
        {
            String code = @"
 
        def foo()
        {
            return = a + 7;
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
        z=t();
        a = 50;
";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code);
            thisTest.Verify("z", 57);
        }

        [Test]
        [Category("SmokeTest")]
        public void T91_1467547_4()
        {
            String code = @"
 
        class B
            { 
	            x3 : int ;
		
	            constructor B(a) 
	            {	
		            x3 = a;
	            }
	
            }
            def foo ( b1 : B )
            {
                return = b1.x3;
            }
            b1 = B.B( 1 );
            d1 = foo;
            e1 = d1(b1);
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("e1", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T91_1467547_5()
        {
            String code = @"
 
        class B
            { 
	            x3 : int ;
		
	            constructor B(a) 
	            {	
		            x3 = a;
	            }
	
            }
            def foo ( b1 : B )
            {
                return = b1.x3;
            }
            b1 = B.B( 1 );
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
            return = a + 7;
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
        [Imperative]
        {
            a = 10;
            t = ding(a);
            z=t();
            a = 50;
        }
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("z", 17);
        }

        [Test]
        [Category("SmokeTest")]
        public void T91_1467547_7()
        {
            String code = @"
 
        def foo()
        {
            return = a + 7;
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
            z=t();
            a = 50;
        }
";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("z", 57);
        }

        [Test]
        [Category("SmokeTest")]
        public void T92_Test_Update_Propagation_In_Self_Update_Thru_Inline_Condition()
        {
            String code = @"
class A
{
    static X : int;
}
c = 0;
A.X = 1;
b = A.X + 1;
c = c + b;
A.X = false ? 43 : A.X;
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
