using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTest.TD;
using ProtoTestFx.TD;
namespace ProtoTest.Associative
{
    class MethodResolution : ProtoTestBase
    {
        [Test]
        public void SimpleCtorResolution01()
        {
            String code =
@"	class f	{		fx : var;		fy : var;		constructor f()		{			fx = 123;			fy = 345;		}	}// Construct class 'f'	cf = f.f();	x = cf.fx;	y = cf.fy;";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 123);
            Assert.IsTrue((Int64)mirror.GetValue("y").Payload == 345);
        }

        [Test]
        public void SimpleCtorResolution02()
        {
            String code =
@"	class f	{		fx : var;		fy : var;		constructor f()		{			fx = 123;			fy = 345;		}        constructor f(x : int, y : int)        {            fx = x;            fy = y;        }	}    // Construct class 'f'	cf = f.f();	x = cf.fx;	y = cf.fy;    cy = f.f(1,2);    x2 = cy.fx;    y2 = cy.fy;";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 123);
            Assert.IsTrue((Int64)mirror.GetValue("y").Payload == 345);
            Assert.IsTrue((Int64)mirror.GetValue("x2").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("y2").Payload == 2);
        }

        [Test]
        public void SimpleCtorResolution03()
        {
            String code =
@"	class vector2D	{		mx : var;		my : var;				constructor vector2D(px : int, py : int)		{			mx = px; 			my = py; 		}        def scale : int()		{			mx = mx * 2; 			my = my * 2;             return = 0;		}        def scale : int(s: int)		{			mx = mx * s; 			my = my * s;             return = 0;		}	}	p = vector2D.vector2D(10,40);	x = p.mx;	y = p.my;    	n = p.scale();	n = p.scale(10);";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 200);
            Assert.IsTrue((Int64)mirror.GetValue("y").Payload == 800);
        }

        [Test]
        public void SimpleCtorResolution04()
        {
            String code =
@"class Sample{    mx : var;    constructor Create()    {}            constructor Create(intval : int)    {}            constructor Create(doubleval : double)    {        mx = doubleval;    }            constructor Create(intval : int, doubleval : double)    {}}    //    default ctors1 = Sample.Create();    //    ctor with ints2 = Sample.Create(1);    //    ctor with doubles3 = Sample.Create(1.0);    //    ctor with int and doubles4 = Sample.Create(1, 1.0);d = s3.mx;";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            Assert.IsTrue((double)mirror.GetValue("d").Payload == 1);
        }

        [Test]
        public void TestMethodOverload1()
        {
            string code =
                @"                class A                {	                def execute(a : A)	                {		                return = 1;	                }                }                class B extends A                {	                def execute(b : B)	                {		                return = 2;	                }                }                a = A.A();                b = B.B();                val = b.execute(a);                ";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            Assert.IsTrue((Int64)mirror.GetValue("val").Payload == 1);
            Assert.IsTrue(core.BuildStatus.WarningCount == 0);
        }

        [Test]
        public void TestMethodOverload2()
        {
            string code =
                @"                class A                {	                def execute(a : A)	                {		                return = 1;	                }                }                class B extends A                {	                def execute(b : B)	                {		                return = 2;	                }                }                class C extends A                {                }                b = B.B();                c = C.C();                val = b.execute(c);                ";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            Assert.IsTrue((Int64)mirror.GetValue("val").Payload == 1);
            Assert.IsTrue(core.BuildStatus.WarningCount == 0);
        }

        [Test]
        public void TestMethodOverload3()
        {
            string code =
                @"                class A                {	                def execute(a : A)	                {		                return = 1;	                }                }                class B extends A                {	                def execute(b : B)	                {		                return = 2;	                }                }                class C extends A                {                }                c = C.C();                val = c.execute(c);                ";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            Assert.IsTrue((Int64)mirror.GetValue("val").Payload == 1);
            Assert.IsTrue(core.BuildStatus.WarningCount == 0);
        }

        [Test]
        public void TestMethodOverload4()
        {
            string code =
                @"                class A                {	                def execute(a : A)	                {		                return = 1;	                }                }                class B extends A                {	                def execute(b : B)	                {		                return = 2;	                }                }                class C extends B                {                }                c = C.C();                val = c.execute(c);                ";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            Assert.IsTrue((Int64)mirror.GetValue("val").Payload == 2);
            Assert.IsTrue(core.BuildStatus.WarningCount == 0);
        }

        [Test]
        public void TestMethodResolutionOverInheritance()
        {
            string code =
                @"                class A                {	                def execute(a : A)	                {		                return = 1;	                }                }                class B extends A                {                }                class C extends B                {                }                c = C.C();                val = c.execute(c);                ";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            Assert.IsTrue((Int64)mirror.GetValue("val").Payload == 1);
            Assert.IsTrue(core.BuildStatus.WarningCount == 0);
        }

        [Test]
        public void TestMethodOverlaodAndArrayInput1()
        {
            string code =
                @"                class A                {                    def execute(a : A)                    {                         return = -1;                     }                }                class B extends A                {                    def execute(arr : B[])                    {                        return = 2;                    }                }                b = B.B();                arr = {B.B(), B.B(), B.B()};                val = b.execute(arr);                ";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            Assert.IsTrue((Int64)mirror.GetValue("val").Payload == 2);
            Assert.IsTrue(core.BuildStatus.WarningCount == 0);
        }

        [Test]
        public void TestMethodOverlaodAndArrayInput2()
        {
            string code =
                @"                class A                {                    def execute(a : A)                    {                         return = -1;                     }                }                class B extends A                {                    def execute(arr : B[])                    {                        return = 2;                    }                }                class C extends B                {                }                b = B.B();                arr = {C.C(), B.B(), C.C()};                val = b.execute(arr);                ";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("val", 2);
            thisTest.VerifyBuildWarningCount(0);
        }

        [Test]
        public void TestMethodOverlaodAndArrayInput3()
        {
            string code =
                @"                class A                {                }                class B extends A                {                    def execute(b : B)                    {                         return = -1;                     }                    def execute(arr : B[])                    {                        return = 2;                    }                }                class C extends B                {                }                b = B.B();                arr = {C.C(), B.B(), C.C()};                val = b.execute(arr);                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("val", 2);
            thisTest.VerifyBuildWarningCount(0);
        }

        [Test]
        public void TestMethodOverlaodAndArrayInput4()
        {
            string code =
                @"                class A                {                }                class B extends A                {                    static def execute(b : B)                    {                         return = -1;                     }                    def execute(arr : B[])                    {                        return = 2;                    }                }                class C extends B                {                }                arr = {C.C(), B.B(), C.C()};                val = B.execute(arr);                val1 = val[0];                val2 = val[1];                val3 = val[2];                ";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            Assert.IsTrue((Int64)mirror.GetValue("val1").Payload == -1);
            Assert.IsTrue((Int64)mirror.GetValue("val2").Payload == -1);
            Assert.IsTrue((Int64)mirror.GetValue("val3").Payload == -1);
            Assert.IsTrue(core.BuildStatus.WarningCount == 0);
        }

        [Test]
        public void TestMethodOverlaodAndArrayInput4Min()
        {
            string code =
                @"                class A                {                }                class B extends A                {                    static def execute(b : B)                    {                         return = -1;                     }                    def execute(arr : B[])                    {                        return = 2;                    }                }                arr = {B.B(), B.B()};                val = B.execute(arr);                val1 = val[0];                val2 = val[1];                ";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            Assert.IsTrue((Int64)mirror.GetValue("val1").Payload == -1);
            Assert.IsTrue((Int64)mirror.GetValue("val2").Payload == -1);
            Assert.IsTrue(core.BuildStatus.WarningCount == 0);
        }

        [Test]
        [Category("Method Resolution")]
        public void TestStaticDispatchOnArray()
        {
            //Recorded as defect: DNL-1467146
            string code =
                @"class A{static def execute(b : A){ return = 100; }}arr = {A.A()};v = A.execute(arr);val = v[0];                ";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            Assert.IsTrue((Int64)mirror.GetValue("val").Payload == 100);
            Assert.IsTrue(core.BuildStatus.WarningCount == 0);
        }

        [Test]
        [Category("Method Resolution")]
        [Category("Escalate")]
        public void TestStaticDispatchOnEmptyArray()
        {
            string code =
                @"class A{static def execute(b : A){ return = 100; }}arr = {};v = A.execute(arr);val = v[0];                ";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            //Assert.IsTrue((Int64)mirror.GetValue("val").Payload == 100);
            Assert.IsTrue(core.BuildStatus.WarningCount == 0);
        }

        [Test]
        public void TestMethodOverlaodAndArrayInput5()
        {
            string code =
                @"                class A                {}                class B extends A                {                    def execute(b : B)                    {                         return = -1;                     }                    static def execute(arr : B[])                    {                        return = 2;                    }                }                class C extends B                {}                class D extends B                {}                arr = {C.C(), D.D(), C.C()};                val = B.execute(arr);                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("val", 2);
            thisTest.VerifyBuildWarningCount(0);
        }

        [Test]
        public void TestMethodOverlaodAndArrayInput6()
        {
            string code =
                @"                class A                {                    def execute(a : A)                    {                         return = -1;                     }                }                class B extends A                {                    static def execute(arr : B[], i : int)                    {                        return = 2;                    }                }                class C extends B                {                }                arr = {C.C(), B.B(), C.C()};                val = B.execute(arr, 1);                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("val", 2);
            thisTest.VerifyBuildWarningCount(0);
        }

        [Test]
        public void TestMethodWithArrayInput1()
        {
            string code = @"                            class A                            {                            }                            class B extends A                            {                            }                            def Test(arr : A[])                            {                                    return = 123;                            }                            a = {B.B(), B.B(), B.B()};                            val = Test(a);                            ";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            Assert.IsTrue((Int64)mirror.GetValue("val").Payload == 123);
            Assert.IsTrue(core.BuildStatus.WarningCount == 0);
        }

        [Test]
        public void TestMethodWithArrayInput2()
        {
            string code = @"                            class A                            {                            }                            class B extends A                            {                            }                            def Test(arr : A[])                            {                                    return = 123;                            }                            a = {B.B(), A.A(), B.B()};                            val = Test(a);                            ";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            Assert.IsTrue((Int64)mirror.GetValue("val").Payload == 123);
            Assert.IsTrue(core.BuildStatus.WarningCount == 0);
        }

        [Test]
        [Category("Method Resolution")]
        public void TestMethodWithArrayInputOverload()
        {
            string code = @"                            class A                            {	                            def foo(x : double)                                { return = 1; }                                def foo(x : double[]) 	                            { return = 2; }	                            def foo(x : double[][]) 	                            { return = 3; }                            }                            arr = 1..20..2;                            val = A.A().foo(arr);                            ";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            Assert.IsTrue((Int64)mirror.GetValue("val").Payload == 2);
            Assert.IsTrue(core.BuildStatus.WarningCount == 0);
        }

        [Test]
        [Category("Method Resolution")]
        public void TestMethodWithArrayInputOverloadDirectType()
        {
            string code = @"                            class A                            {	                            def foo(x : int)                                { return = 1; }                                def foo(x : int[]) 	                            { return = 2; }	                            def foo(x : int[][]) 	                            { return = 3; }                            }                            arr = 1..20..2;                            val = A.A().foo(arr);                            ";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            Assert.IsTrue((Int64)mirror.GetValue("val").Payload == 2);
            Assert.IsTrue(core.BuildStatus.WarningCount == 0);
        }

        [Test]
        public void TestMethodWithOverrides()
        {
            string code = @"                            class A                            {	                            def foo(x : double)                                { return = 1; }                            }                            class B extends A                            {                                def foo(x : double)                                { return = 2; }                            }                                                        a = A.A();                            val1 = a.foo(0.0);                                                      //  b = B.B();                                                      //  val2 =b.foo(0.0);                            ";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            Assert.IsTrue((Int64)mirror.GetValue("val1").Payload == 1);
            //Assert.IsTrue((Int64)mirror.GetValue("val2").Payload == 2);
        }

        [Test]
        public void TestOverridenMethod()
        {
            string code = @"                            class A                            {	                            def foo(x : double)                                { return = 1; }                            }                            class B extends A                            {                                def foo(x : double)                                { return = 2; }                            }                                                      //  a = A.A();                          //  val1 = a.foo(0.0);                                                      b = B.B();                                                      val2 =b.foo(0.0);                            ";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ExecutionMirror mirror = fsr.Execute(code, core);
            //Assert.IsTrue((Int64)mirror.GetValue("val1").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("val2").Payload == 2);
        }

        [Test]
        [Category("Failure")]
        public void TestMethodResolutionForSingleton()
        {
            // Tracked by: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4116
            string code = @"def foo(x:var[]..[]){    return = 2;}def foo(x:var[]){    return = 1;}d = foo(2);";
            string error = "MAGN-4116 DesignIssue: Method resolution - When a single value is passed to overloads with different rank, which one is chosen";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("d", 2);
        }

        [Test]
        public void TestMethodResolutionForArray()
        {
            string code = @"def foo(val : bool){    return = 1;}def foo(val : var){    return = 2;}arr1 = { true, null, false };r1 = foo(arr1);   // r1 = {1, 1, 1}arr2 = { null, true, false };r2 = foo(arr2);   // r2 = {2, 2, 2}";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r1", new object[] { 1, 1, 1 });
            thisTest.Verify("r2", new object[] { 2, 2, 2 });
        }

        [Test]
        [Category("Failure")]
        public void TestMethodResolutionForInforLoss()
        {
            // Tracked by: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4114
            string code = @"def foo(x1 : int, x2: int, x3: int, x4:int){    return = x1 + x2 + x3 + x4;}def foo(x1 : double, x2:double, x3:double, x4:double){    return = x1 * x2 * x3 * x4;}r = foo(3, 4, 5, 4.7);   ";
            string error = "MAGN-4114 Method resolution issue - type promotion should be preferred to type demotion";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("r", 282);

        }

        [Test]
        public void TestMethodResultionForNonExistType()
        {
            string code = @"def foo(x : NonExistClass[][]){    return = x;}z = foo({ 1, 2 });  ";
            string error = "";
            thisTest.VerifyRunScriptSource(code, error);
            thisTest.Verify("z", null);
        }
    }
}
