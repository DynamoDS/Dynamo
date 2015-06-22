using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTest.TD;
using ProtoCore.Lang.Replication;
using ProtoTestFx.TD;
namespace ProtoTest.Associative
{
    class MethodsFocusTeam : ProtoTestBase
    {
        [Test]
        public void SimpleCtorResolution01()
        {
            String code =
@"	class f	{		fx : var;		fy : var;		constructor f()		{			fx = 123;			fy = 345;		}	}// Construct class 'f'	cf = f.f();	x = cf.fx;	y = cf.fy;";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 123);
            thisTest.Verify("y", 345);
        }

        [Test]
        public void T_upgrade()
        {
            String code =
@"def foo(x:var[]){    return = x;}a = foo(1);";
            thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 1 };
            thisTest.Verify("a", v1);

        }

        [Test]
        public void T001_DotOp_DefautConstructor_01()
        {
            String code =
@"	class C	{		fx : int;		fy : int;	}	c = C.C();	x = c.fx;	y = c.fy;";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 0);
            thisTest.Verify("y", 0);
        }

        [Test]
        public void T002_DotOp_DefautConstructor_02()
        {
            String code =
@"	class C	{		fx : double;		fy : var;	}	c = C.C();	x = c.fx;	y = c.fy;";
            thisTest.RunScriptSource(code);
            //Assert.Fail("0.0 should not be evaluated to be the same as 'null' in verification");
            thisTest.Verify("x", 0.0);
            thisTest.Verify("y", null);
        }

        [Test]
        public void T003_DotOp_DefautConstructor_03()
        {
            String code =
@"	class C	{		fx : double;		fy : double;	}	c = C.C();	x = c.fx;	y = c.fy;";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 0.0);
            thisTest.Verify("y", 0.0);
        }

        [Test]
        public void T004_DotOp_DefautConstructor_04()
        {
            String code =
@"	class C	{		fx : var;		fy : var;	}	c = C.C();	x = c.fx;	y = c.fy;";
            thisTest.RunScriptSource(code);
            Object v1 = new Object();
            v1 = null;
            thisTest.Verify("x", v1);
            thisTest.Verify("y", v1);
        }

        [Test]
        public void T005_DotOp_DefautConstructor_05()
        {
            String code =
@"	class C	{		fx : var[];		fy : var[];	}	c = C.C();	x = c.fx;	y = c.fy;";
            thisTest.RunScriptSource(code);
            Object v1 = new Object();
            v1 = null;
            thisTest.Verify("x", v1);
            thisTest.Verify("y", v1);
        }

        [Test]
        public void T006_DotOp_SelfDefinedConstructor_01()
        {
            String code =
@"	class C	{		fx : int;		fy : int;		constructor C()		{			fx = 10;			fy = 20;		}    	}	c = C.C();	x = c.fx;	y = c.fy;";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 10);
            thisTest.Verify("y", 20);
        }

        [Test]
        public void T007_DotOp_SelfDefinedConstructor_02()
        {
            String code =
@"	class C	{		fx : double;		fy : double;		constructor C()		{			fx = 10;			fy = 20;		}    	}	c = C.C();	x = c.fx;	y = c.fy;";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 10.0);
            thisTest.Verify("y", 20.0);
        }

        [Test]
        public void TV1467134_intToDouble_1()
        {
            String code =
@"	def foo : double(x : double){    return = x + 1;}a = 1;r = foo(a);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 2.0);
        }

        [Test]
        public void TV1467134_intToDouble_2()
        {
            String code =
@"	def foo : double(x : double){    return = x + 3.1415926;}a = 1;r = foo(a);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 4.1415926);
        }

        [Test]
        public void TV1467134_intToDouble_dotOp()
        {
            String code =
@"class A{    fx : double = 1;    constructor A(x : int)    {        fx = this.foo(x);    }    def foo : double(y : int)    {        fx = y+fx;        return = fx;    }}a = A.A(1.1);r1 = a.foo(2.0);r2 = a.fx;";
            thisTest.RunScriptSource(code);
            //thisTest.Verify("r1", );
        }

        [Test]
        public void T008_DotOp_MultiConstructor_01()
        {
            String code =
@"	class C	{		fx : var;		fy : var;		constructor C1()		{			fx = 1;			fy = 2;		} 		constructor C2()		{			fx = 3;			fy = 4;		}   	}	c1 = C.C1();	x1 = c1.fx;	y1 = c1.fy;	c2 = C.C2();	x2 = c2.fx;	y2 = c2.fy;";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x1", 1);
            thisTest.Verify("y1", 2);
            thisTest.Verify("x2", 3);
            thisTest.Verify("y2", 4);
        }

        [Test]
        public void T009_DotOp_FuncCall()
        {
            String code =
@"	class C	{		fx : var;		fy : var;		constructor C1()		{			fx = 1;			fy = 2;		} 		def foo()        {            fx = 3;            fy = 4;        }	}	c1 = C.C1();	x1 = c1.fx;	y1 = c1.fy;	 m = c1.foo();    x2 = c1.fx;    y2 = c1.fy;    ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x1", 3);
            thisTest.Verify("y1", 4);
            thisTest.Verify("x2", 3);
            thisTest.Verify("y2", 4);
        }

        [Test]
        public void T010_DotOp_Property()
        {
            String code =
@"class C{	fx : var[];	//fy : var[];	constructor C()	{		fx = {0,1,2};	}}fc = C.C();m = fc.fx;n = fc.fx + 1;	";
            thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 0, 1, 2 };
            Object[] v2 = new Object[] { 1, 2, 3 };
            thisTest.Verify("m", v1);
            thisTest.Verify("n", v2);
        }

        [Test]
        public void T011_DotOp_Property_2()
        {
            String code =
@"class C{	fx : var[];	constructor C()	{		fx = {0,1,2};	}        def foo(fz:int)    {        return = fx + fz;    }}fc = C.C();m = fc.fx;n = fc.foo(1);	";
            thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 0, 1, 2 };
            Object[] v2 = new Object[] { 1, 2, 3 };
            thisTest.Verify("m", v1);
            thisTest.Verify("n", v2);
        }

        [Test]
        public void T012_DotOp_UserDefinedClass_01()
        {
            String code =
@"class A{	fx :var;	fb : B;	constructor A(x : var)	{		fx = x;		fb = B.B(x);		}}class B{	fy: var;	constructor B(y : var)	{		fy = y;	}}fa = A.A(3);r1 = fa.fx;//3r2 = fa.fb.fy;//3fb = B.B(5);r3 = fb.fy;//5	";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 3);
            thisTest.Verify("r2", 3);
            thisTest.Verify("r3", 5);
        }

        [Test]
        public void T013_DotOp_UserDefinedClass_02()
        {
            String code =
@"class A{	fx :var;	fb : B;	constructor A(x : var)	{		fx = x;		fb = B.B(x);		}}class B{	fy: var;	constructor B(y : var)	{		fy = y;	}}fa = A.A(1..3);r1 = fa.fx;//1..3r2 = fa.fb.fy;//1..3fb = B.B(1..5);r3 = fb.fy;//1..5";
            thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 1, 2, 3 };
            Object[] v2 = new Object[] { 1, 2, 3 };
            Object[] v3 = new Object[] { 1, 2, 3, 4, 5 };
            thisTest.Verify("r1", v1);
            thisTest.Verify("r2", v2);
            thisTest.Verify("r3", v3);
        }

        [Test]
        public void T014_DotOp_UserDefinedClass_03()
        {
            String code =
@"class A{	fx :var;	fb : B;	constructor A(x : var)	{		fx = x;		fb = B.B({x,10});		}}class B{	fy: var[];	constructor B(y : var[])	{		fy = y;			}	def foob()			{				fy = fy +1;				return = fy;			}}fa = A.A(1..3);r1 = fa.fx;//1..3r2 = fa.fb.fy;//{{1,10},{2,10},{3,10}}->{{2,11},{3,11},{4,11}}r3 = fa.fb.foob();//{{2,11},{3,11},{4,11}}";
            thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 1, 2, 3 };
            //Object[] v2 = new Object[] { 1, 10 };
            //Object[] v3 = new Object[] { 2, 10 };
            //Object[] v4 = new Object[] { 3, 10 };
            //Object[] v5 = new Object[] { v2, v3, v4 };
            Object[] v6 = new Object[] { 2, 11 };
            Object[] v7 = new Object[] { 3, 11 };
            Object[] v8 = new Object[] { 4, 11 };
            Object[] v9 = new Object[] { v6, v7, v8 };
            thisTest.Verify("r1", v1);
            thisTest.Verify("r2", v9);
            thisTest.Verify("r3", v9);
            //Assert.Fail("1467135 - Sprint 24 - Rev 2941: non-deterministic dispatch warning message thrown out when using replication with dot opration");
            thisTest.VerifyBuildWarningCount(0);
        }

        [Test]
        [Category("Failure")]
        public void TV1467135_DotOp_Replication_1()
        {
            String code =
@"class A{    fx:int;    fb : B;     def foo()    {           return = 1;    }}class B{    fy :int;    def foo()    {        return  =2 ;    }}a = A.A();b = B.B();arr = {a,b};arr2 = {a.fb,b};r1 = arr.foo();r2 = arr2.foo();";
            // Tracked by: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1693
            string str = "MAGN-1693 Regression : Dot Operation using Replication on heterogenous array of instances is yielding wrong output";
            thisTest.VerifyRunScriptSource(code, str);
            Object[] v1 = new Object[] { 1, 2 };
            Object[] v2 = new Object[] { null, 2 };
            thisTest.Verify("r1", v1);
            thisTest.Verify("r2", v2);
            thisTest.VerifyBuildWarningCount(0);
        }

        [Test]
        public void TV1467135_DotOp_Replication_2()
        {
            String code =
@"class A{    fx:int;    fb : B;    constructor A(x :int)    {        fx = x;        fb = B.B(x);    }      def foo()    {           return = 1;    }}class B{    fy :int;    constructor B(y:int)    {        fy = y;    }    def foo()    {        return  =2 ;    }}a = A.A({1,1});b = B.B({2,2});rfx = a.fx;raby = a.fb.fy;rfy = b.fy;";
            thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 1, 1 };
            Object[] v2 = new Object[] { 2, 2 };
            thisTest.Verify("rfx", v1);
            thisTest.Verify("raby", v1);
            thisTest.Verify("rfy", v2);
            thisTest.VerifyBuildWarningCount(0);
        }

        [Test]
        [Category("Failure")]
        public void TV1467135_DotOp_Replication_3()
        {
            String code =
@"class A{    fx:int;    fb : B;    constructor A(x :int)    {        fx = x;        fb = B.B(x);    }      def foo()    {           return = 1;    }}class B{    fy :int;    constructor B(y:int)    {        fy = y;    }    def foo()    {        return  =2 ;    }}a = A.A({1,1});b = B.B({2,2});rfoo = {a,b}.foo();";
            // Tracked by: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1693
            string str = "MAGN-1693 Regression : Dot Operation using Replication on heterogenous array of instances is yielding wrong output";
            thisTest.VerifyRunScriptSource(code, str);
            Object[] v1 = new Object[] { 1, 1 };
            Object[] v2 = new Object[] { 2, 2 };
            Object[] v3 = new Object[] { v1, v2 };
            thisTest.Verify("rfoo", v3);
            thisTest.VerifyBuildWarningCount(0);
        }


        [Test]
        public void TV1467135_CallingFuncInSameScope()
        {
            String code =
@"class C{    fz:int;    constructor C(z:int)    {        fz = foo(z);    }    def foo(z:int)    {        return = z+1;    }}c = C.C({3,3});rc = c.fz;";
            thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 4, 4 };
            thisTest.Verify("rc", v1);
            thisTest.VerifyBuildWarningCount(0);
        }

        [Test]
        public void TV1467135_CallingFuncInSameScope_this()
        {
            String code =
@"class C{    fz:int;    constructor C(z:int)    {        fz = this.foo(z);    }    def foo(z:int)    {        return = z+1;    }}c = C.C({3,3});rc = c.fz;";
            thisTest.RunScriptSource(code);
            thisTest.SetErrorMessage("1467372 - Sprint 27 - Rev 4107:\"this\" keyword doesn't return correct answer when using with replication");
            Object[] v1 = new Object[] { 4, 4 };
            thisTest.Verify("rc", v1);
            thisTest.VerifyBuildWarningCount(0);
        }

        [Test]
        public void TV1467372_ThisKeyword()
        {
            String code =
@"class C{    fz:int;    constructor C(z:int)    {        fz = this.foo(z);    }    def foo(z:int)    {        this.fz = z +1;        return = fz;    }}c = C.C({3,3});rc = c.fz;";
            thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 4, 4 };
            thisTest.Verify("rc", v1);
        }

        [Test]
        public void TV1467372_ThisKeyword_InMemberFunction_Replication()
        {
            String code =
@"class C{    fz:int;    constructor C(z:int)    {        fz = this.foo(z);    }    def foo(z:int)    {        this.fz = this.foo();        return = fz;    }    def foo()    {        return = this.fz+1;    }}c = C.C({3,3});rc = c.fz;";
            thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 1, 1 };
            thisTest.Verify("rc", v1);
        }

        [Test]
        public void TV1467372_ThisKeyword_InMemberFunction_Replication_2()
        {
            String code =
@"class C{    fz:int;    constructor C(z:int)    {        fz = this.foo(z);    }    def foo(z:int)    {        this.fz = this.foo() +z;        return = fz;    }    def foo()    {        return = this.fz+1;    }}c = C.C({3,3});r1 = c.foo();";
            thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 5, 5 };
            thisTest.Verify("r1", v1);
        }

        [Test]
        public void TV1467372_ThisKeyword_InMemberFunction_Replication_3()
        {
            String code =
@"class C{    fz:int;    constructor C(z:int)    {        fz = this.foo(z);    }    def foo(z:int)    {        this.fz = this.foo() +z;        return = fz;    }    def foo()    {        return = this.fz+1;    }}c = C.C({3,3});r1 = c.foo(10);";
            string str = "";
            thisTest.RunScriptSource(code, str);
            Object[] v1 = new Object[] { 15, 15 };
            thisTest.Verify("r1", v1);
        }

        [Test]
        public void TV1467372_ThisKeyword_2()
        {
            String code =
@"class A {    fx:int;    constructor A(x)    {        this.fx = this.fx +x;    }}a = A.A(1);ra = a.fx;";
            thisTest.RunScriptSource(code);
            thisTest.Verify("ra", 1);
        }

        [Test]
        public void TV1467372_ThisKeyword_2_Replication()
        {
            String code =
@"class A {    fx:int;    constructor A(x)    {        this.fx = this.fx +x;    }}a = A.A({1,1});ra = a.fx;";
            thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 1, 1 };
            thisTest.Verify("ra", v1);
        }

        [Test]
        public void TV1467372_ThisKeyword_3()
        {
            String code =
@"class A {    fx:int;    constructor A(x:int)    {        this.fx = this.fx +x;    }}class B extends A{    constructor B(y:int)    {        this.fx = this.fx + y;    }}a = A.A({1,1});b = B.B({2,2});ra = a.fx;rb = b.fx;";
            thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 1, 1 };
            Object[] v2 = new Object[] { 2, 2 };
            thisTest.Verify("ra", v1);
            thisTest.Verify("rb", v2);
        }

        [Test]
        public void TV1467372_ThisKeyword_InMemberFunction_1()
        {
            String code =
@"class C{    fz:int = 0;    constructor C(z:int)    {        fz = this.foo(z);    }    def foo(z:int)    {        this.fz = this.foo();        return = fz;    }    def foo()    {        return = this.fz+1;    }}c = C.C(3);rc = c.fz;";
            thisTest.RunScriptSource(code);
            thisTest.Verify("rc", 1);
        }

        [Test]
        public void TV1467135_DotOp_Replication_4()
        {
            String code =
@"class A{    fx:int;    fb : B;    constructor A(x :int)    {        fx = x;        fb = B.B(x);    }      def foo()    {           return = 1;    }}class B{    fy :int;    fc:C;    constructor B(y:int)    {        fy = y;        fc = C.C(y);    }    def foo()    {        return  =2 ;    }}class C{    fz:int;    constructor C(z:int)    {        fz = foo(z);    }    def foo(z:int)    {        return = z+1;    }}t1 = A.A({1,1}).fb.fc.foo(10);t2 = A.A({1,1}).fb.fc.fz;t3 = A.A({1,1}).fb.foo();";
            thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 11, 11 };
            Object[] v2 = new Object[] { 2, 2 };
            thisTest.Verify("t1", v1);
            thisTest.Verify("t2", v2);
            thisTest.Verify("t3", v2);
            thisTest.VerifyBuildWarningCount(0);
        }

        [Test]
        public void T015_DotOp_Collection_01()
        {
            String code =
@"class A{	fx :var;	constructor A(x : var)	{		fx = x;	}}fa = A.A(1..3);r1 = fa.fx[0]==fa[0].fx? true:false;r2 = fa.fx[0];";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", true);
        }

        [Test]
        public void T015_DotOp_Collection_01a()
        {
            String code =
@"class A{	fx :int;	constructor A(x : int)	{		fx = x;	}}fa = A.A(1..3);r0a = fa.fx;r1 = (fa.fx[0] == r0a[0]);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", true);
        }

        [Test]
        public void T016_DotOp_Collection_02()
        {
            String code =
@"	class A{	fx :var[][];	constructor A(x : var[][])	{		fx = x;		}}a = {{0},{1},{2}};fa = A.A(a);r1 = fa.fx;//";
            thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 0 };
            Object[] v2 = new Object[] { 1 };
            Object[] v3 = new Object[] { 2 };
            Object[] v4 = new Object[] { v1, v2, v3 };
            thisTest.Verify("r1", v4);
        }

        [Test]
        public void T017_DotOp_Collection_03()
        {
            String code =
@"	class A{	fx :var[][];	constructor A(x : var[][])	{		fx = x;		}}a = {{0},{1},{2}};fa = A.A(a);r1 = fa.fx[1][0];//";
            thisTest.RunScriptSource(code);

            thisTest.Verify("r1", 1);
        }

        [Test]
        public void T018_DotOp_Collection_04()
        {
            String code =
                    @"                    class A                     {                                    fx: var;                                    fb: B[];                                                    constructor A(x :var)                                    {                                                    fx = x;                                                    fb = B.B({10,11});                                                  }                    }                    class B                    {                                    fy : var;                                    constructor B(y : var)                                    {                                                    fy = y;                                    }                    }                    a = {1,2};                    va = A.A(a);                    r1 = va.fb.fy;                    ";
            thisTest.RunScriptSource(code);
            //Assert.Fail("1467136 - Sprint 24 - Rev 2941:resolution failure when using dot operation to get 2D array property ");
            Object[] v1 = { 10, 11 };
            Object[] v2 = { v1, v1 };
            thisTest.Verify("r1", v2);

        }

        [Test]
        public void TV018_DotOp_Collection_04_1()
        {
            String code =
@"class A {                fx: var;                fb: B[];                                constructor A(x :var)                {                                fx = x;                                fb = B.B({10,11});                              }}class B{                fy : var;                constructor B(y : var)                {                                fy = y;                }}a = {1,2};va = A.A(a);r1 = va.fb.fy;";
            thisTest.RunScriptSource(code);
            Object v1 = new Object[] { 10, 11 };
            Object v2 = new Object[] { v1, v1 };
            thisTest.Verify("r1", v2);
        }

        [Test]
        public void TV018_DotOp_Collection_04_2()
        {
            String code =
@"def foo(x:int){    return = x;}r:int = foo({1,2});";
            thisTest.RunScriptSource(code);
            Object v1 = null;
            thisTest.Verify("r", v1);
        }

        [Test]
        public void TV018_DotOp_Collection_04_3()
        {
            String code =
                    @"                    class A                     {                                    fx: var;                        fb : B[];                                        constructor A(x : var)                                    {                                                    fx = x;                                                    fb = B.B({10,11});                                                  }                    }                    class B                    {                                    fy : var;                                    constructor B(y : var)                                    {                                                    fy = y;                                    }                    }                    a = {1,2};                    va = { A.A(a),  A.A(a + 1)  };                    r1 = va.fb.fy;                    ";
            thisTest.RunScriptSource(code);
            Object v1 = new Object[] { 10, 11 };
            Object v2 = new Object[] { new object[] { v1, v1 }, new object[] { v1, v1 } };
            thisTest.Verify("r1", v2);
        }

        [Test]
        public void TV018_DotOp_Collection_04_4()
        {
            String code =
                    @"                    class A                         {                                        fx: var;                            fb : B[];                            fc : var[];                                            constructor A(x : var)                                        {                                                        fx = x;                                            fb = B.B({ 10, 11 });                                            fc = fb.fy;                                        }                        }                        class B                        {                                        fy : var;                                        constructor B(y : var)                                        {                                                        fy = y;                                        }                        }                        a = {1,2};                        va = A.A(a) ;                        r1 = va.fc;                    ";
            thisTest.RunScriptSource(code);
            Object v1 = new Object[] { 10, 11 };
            Object v2 = new Object[] { v1, v1 };
            thisTest.Verify("r1", v2);
        }


        [Test]
        public void T020_Replication_Var()
        {
            String code =
@"def foo(x:var){	return = x+1;}a = {1,2};r = foo(a);";
            thisTest.RunScriptSource(code);
            Object v1 = new Object[] { 2, 3 };
            thisTest.Verify("r", v1);
        }

        [Test]
        public void T019_DotOp_Collection_05()
        {
            String code =
@"class A {	fx: int;	fb: B;		constructor A(x :int)	{		fx = x;		fb = B.B({10,11});		}}class B{	fy : int;	constructor B(y : int)	{		fy = y;	}}a = {1,2};va = A.A(a);r1 = va[0].fb.fy;r2 = va.fb[0].fy;r3 = va.fb.fy[0];";
            thisTest.RunScriptSource(code);
            thisTest.SetErrorMessage("1467333 - Sprint 27 - Rev 3959: when initializing class member, array is converted to not indexable type, which gives wrong result");
            Object v1 = null;
            thisTest.Verify("r1", v1);
            thisTest.Verify("r2", v1);
            thisTest.Verify("r3", v1);

        }

        [Test]
        public void T021_DotOp_Nested_01()
        {
            String code =
@"class A {	fx: var;	fb: B[];		constructor A(x :var)	{		fx = x;		fb = B.B({10,11});		}}class B{	fy : var;	fc: C[];	constructor B(y : var)	{		fy = y;		fc = C.C({100,200});	}}class C{	fz:var;	constructor C(z :var)	{		fz= z;	}}a = {1,2};va = A.A(a);r = va[0].fb[0].fc[0].fz;";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 100);


        }

        [Test]
        public void T021_DotOp_Nested_02()
        {
            String code =
@"class A {    fx: int;    fc: C[];        constructor A(x :int)    {        fx = x;        fc = C.C({10,11});        }}class B extends A{    constructor B(y : int): base.A(y)    {    }}class C {    fz : int;    constructor C(z : int)    {        fz = z;    }}b = {1,2};vb = B.B(b);t1 = vb.fc;r1 = vb[0].fc.fz;r2 = vb.fc[0].fz;r3 = vb.fc.fz[0];r4 = vb.fx;";
            thisTest.RunScriptSource(code);
            thisTest.SetErrorMessage("1467137 - Sprint 24 - Rev 2941: wrong result when using dot opration to get property for more than two collections");
            Object[] v1 = new Object[] { 10, 11 };
            Object[] v2 = new Object[] { 1, 2 };
            thisTest.Verify("r1", v1);
            thisTest.Verify("r2", v1);
            thisTest.Verify("r3", v1);
            thisTest.Verify("r4", v2);
        }

        [Test]
        public void TV1467137_DotOp_Indexing_1()
        {
            String code =
@"class A {	fx: int;	fb: B[];		constructor A(x :int)	{		fx = x;		fb = B.B({10,11});		}}class B{	fy : int;	constructor B(y : int)	{		fy = y;	}    def foo()    {        fy = 100;        return = fy;    }}a = {1,2};va = A.A(a);r1 = va[0][0].fb.foo();r2 = va.fb[0][0].foo();r3 = va.fb.foo()[0][0];r4 = va[0].fb.foo()[0];r5 = va.fb[0].foo()[0];r6 = va.fb.foo()[0][0];";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", null);
            thisTest.Verify("r2", 100);
            thisTest.Verify("r3", 100);
            thisTest.Verify("r4", 100);
            thisTest.Verify("r5", 100);
            thisTest.Verify("r6", 100);
        }

        [Test]
        public void TV1467137_1_DotOp_Update()
        {
            String code =
@"class A {	fx: int;	fb: B[];		constructor A(x :int)	{		fx = x;		fb = B.B({10,11});		}}class B{	fy : int;	constructor B(y : int)	{		fy = y;	}    def foo()    {        fy = 100;        return = fy;    }}a = {1,2};va = A.A(a);r1 = va[0].fb.fy;r2 = va.fb[0].fy;r3 = va.fb.fy[0];r4 = va[0].fb.foo()[0];r5 = va.fb[0].foo()[0];r6 = va.fb.foo()[0][0];";
            thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 100, 100 };
            thisTest.SetErrorMessage("1467137 - Sprint 24 - Rev 2941: wrong result when using dot opration to get property for more than two collections");
            thisTest.Verify("r1", v1);
            thisTest.Verify("r2", v1);
            thisTest.Verify("r3", v1);
            thisTest.Verify("r4", 100);
            thisTest.Verify("r5", 100);
            thisTest.Verify("r6", 100);
        }

        [Test]
        [Category("Failure")]
        public void T021_DotOp_Nested_03()
        {
            String code =
@"class A {         fb: B =B.B({10,11}) ;      } class B {         constructor B(y : int)         {        }} va = A.A();s = va.fb;m:B = B.B({10,11});r = m==s;";
            // Tracked by: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4045
            thisTest.SetErrorMessage("MAGN-4045 Initializing class member at class level with array of objects, causes crash");

            thisTest.RunScriptSource(code);  
            Object v1 = null;
            thisTest.Verify("m", v1);
            thisTest.Verify("s", v1);
            thisTest.Verify("r", true);
        }

        [Test]
        public void TV1467333()
        {
            String code =
@"class A {         fb: B  ;      constructor B()    {        fb = B.B({10,11});    }    } class B {         constructor B(y : int)         {        }} va = A.A();s = va.fb;n = a.fb;m:B = B.B({10,11});r = m==n;";
            thisTest.RunScriptSource(code);
            thisTest.SetErrorMessage("1467333 - Sprint 27 - Rev 3959: when initializing class member, array is converted to not indexable type, which gives wrong result");
            Object v1 = null;
            //thisTest.Verify("va", v1); (as the constuction of the object is valid even if the assignment fails)
            thisTest.Verify("m", v1);
            thisTest.Verify("s", v1);
            thisTest.Verify("r", true);
        }

        [Test]
        public void T022_DotOp_CallFunc_01()
        {
            String code =
@"class A{	fb : B;	def fooa(x:var)	{			fb = B.B();        return = fb.foob(x);			}}class B{    x : var;	def foob(x : var)	{		this.x = x; 	return = this.x;	}}a = A.A();r1 =  a.fooa(1);r2 = a.fb.foob(2);b = B.B();r3 = b.foob(3);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 1);
            thisTest.Verify("r2", 2);
            thisTest.Verify("r3", 3);
        }

        [Test]
        [Category("Failure")]
        public void T023_DotOp_FuncCall_02()
        {
            // Tracked by: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4048
            String code =
@"class A{	fb : B;	def fooa(x:var)	{		fb = B.B();        return = fb.foob(x);	}}class B{	fa :A;	def foob(x : var)	{		fa = A.A();        return = fa.fooa(x);	}}a = A.A();r = a.fooa(1);";
            Assert.Fail("MAGN-4048 Cyclic dependency undetected resulting in StackOverflowException");
            thisTest.RunScriptSource(code);

        }

        [Test]
        public void T024_DotOp_FuncCall_03()
        {
            String code =
@"class A{	fb : B;	def fooa(x:var)	{		fb = B.B();        return = fb.foob(x);	}}class B{	fc :C;	def foob(x : var)	{		fc = C.C();		return = fc.fooc(x); 	}}class C{	def fooc(x :var)	{		return = {x,true};	}}a = A.A();r = a.fooa(1);";

            thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 1, true };
            thisTest.Verify("r", v1);
        }

        [Test]
        public void T025_DotOp_FuncCall_04()
        {
            String code =
@"class A{	fb : B;	x: int;constructor A(x:int){		this.x = x;}}a = A.A({0,1});r = a.x;";
            thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 0, 1 };
            thisTest.Verify("r", v1);

        }

        [Test]
        public void TV025_1467140_1()
        {
            String code =
@"class A{	fb : B;	x: int;constructor A(x:int){		this.x = x;        this.fb = B.B(x);}}class B{    b:int;    constructor B(x:int)    {        this.b = x;    }}a = A.A({0,1});r = a.x;r2 = a.fb.b;";
            thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 0, 1 };
            Object[] v2 = new Object[] { 0, 1 };
            thisTest.Verify("r", v1);
            thisTest.Verify("r2", v2);
        }

        [Test]
        public void TV025_1467140_2()
        {
            String code =
@"class A{	fb : B;	x: int;constructor A(x:int){		this.x = x;        this.fb = B.B(x);}}class B extends A{    b:int;    constructor B(x:int)    {        this.x = x+10;        this.b = x;    }}a = A.A({0,1});r = a.x;r2 = a.fb.x;";
            thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 0, 1 };
            Object[] v2 = new Object[] { 10, 11 };
            thisTest.Verify("r", v1);
            thisTest.Verify("r2", v2);
        }

        [Test]
        public void T026_DotOp_FuncCall_05()
        {
            String code =
@"class A{	fb : B;	fx: int;constructor A(x:int){		fx = x;}	def fooa(y:int)	{		fb = B.B();        return = fb.foob({fx,y});	}}class B{	def foob(x : int)	{		return = x;	}}a = A.A({0,1});m = a.fx;r1 = a.fooa({10,11});";
            string str = "";
            thisTest.VerifyRunScriptSource(code, str);
            Object[] v1 = new Object[] { 0, 10 };
            Object[] v2 = new Object[] { 1, 11 };
            Object[] v3 = new Object[] { v1, v2 };
            thisTest.Verify("r1", v3);
        }

        [Test]
        public void T027_DotOp_FuncCall_06()
        {
            String code =
@"class A{	fb : B;	fx: int;constructor A(x:int){		fx = x;}	def fooa(y:int)	{		fb = B.B();        return = fb.foob({fx,y});	}}class B{	def foob(x : int)	{		return = x;	}}a = A.A({0,1});m = a.fx;r2 = a[0].fooa({20,21});";
            thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 0, 20 };
            Object[] v2 = new Object[] { 0, 21 };
            Object[] v3 = new Object[] { v1, v2 };

            thisTest.Verify("r2", v3);
        }
        //////Inheritance

        [Test]
        public void T028_Inheritance_Property()
        {
            String code =
@"class A{	fx: int;	constructor A(x:int)	{		fx = x;	}}class B extends A{	fx : int;	fy : int;	constructor  B(y :int):base.A(x)	{		fy = y;	}}a = A.A(1);b = B.B(2);r1 = a.fx;";
            // Assert.Fail("1467141 - Sprint 24 - Rev 2954: declare a property in subclass with the same name as the property in super class will cause Nunit crash");
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
           {
               thisTest.RunScriptSource(code);
           });

            //thisTest.Verify("r1", 1);

        }

        [Test]
        [Category("Class")]
        public void T029_Inheritance_Property_1()
        {
            String code =
@"class A{	fx: int;	constructor A(x:int)	{		fx = x;	}}class B extends A{		fy : int;	constructor  B(y :int)	{		fy = y;	}}a = A.A(1);b = B.B(2);r1 = a.fx;r2 = b.fx;r3 = b.fy;";

            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 1);
            thisTest.Verify("r2", 0);
            thisTest.Verify("r3", 2);
            
        }

        [Test]
        public void T030_Inheritance_Property_2()
        {
            String code =
@"class A {	fx : int;	constructor A1(x : int)	{		fx = x;	}	constructor A2(x : int)	{		fx = x *2;	}	constructor A()	{		fx = 10;	}}class B extends A{	fy : int;}a1 = A.A1(1);a2 = A.A2(1);b1 = B.B();r = b1.fx;";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 10);
        }


        [Test]
        public void T031_Inheritance_Property_3()
        {
            String code =
@"class A{	fx :int[];	constructor A(x:int[])	{		fx = x +1;	}}class B extends A{	fy :int[];	constructor B(y: int[],x:int[]): base.A(x)	{		fy = y +2;	}}a = A.A({0,1});r1 = a.fx;b = B.B({10,11},{0,1});r2 = b.fx;r3 = b.fy;r4 = a.fy;";
            thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 1, 2 };
            Object[] v2 = new Object[] { 12, 13 };
            thisTest.Verify("r1", v1);
            thisTest.Verify("r2", v1);
            thisTest.Verify("r3", v2);
            thisTest.Verify("r4", null);
        }

        [Test]
        public void T032_ReservationCheck_rangeExp()
        {
            String code =
@"class R {    RangeExpression :int;    constructor R()    {        RangeExpression = 1;    }}r = R.R();r1 = r.RangeExpression;";
            thisTest.RunScriptSource(code);

            thisTest.Verify("r1", 1);
        }

        [Test]
        public void T032_Defect_ReservationCheck_rangeExp()
        {
            String code =
@"class R {    //RangeExpression :int;    constructor R()    {        //RangeExpression = 1;    }    def RangeExpression()    {        return = 1;    }}class RangeExpression{};r = R.R();r1 = RangeExpression.RangeExpression();r2 = r.RangeExpression();";
            thisTest.RunScriptSource(code);
            //thisTest.Verify("r1", 1);
            thisTest.Verify("r2", 1);
        }

        [Test]
        public void T033_PushThroughCasting()
        {
            String code =
@"def foo(x:var){	return = bar(x) + 1;}def bar(x:int){    return = x + 1;}a = 1;r = foo(a);";
            thisTest.RunScriptSource(code);
            Object r = 3;
            thisTest.Verify("r", r);
        }

        [Test]
        public void T033_PushThroughCasting_UserDefinedType()
        {
            String code =
@"def foo(x:AB){	return = bar(x) + 1;}def bar(x:Ric){    return = x.fx +1 ;}class AB{	fx : var;	constructor AB()	{ 		fx = 1;	}}class Ric extends AB{	constructor Ric() : base.AB()	{	}}ric = Ric.Ric();r = foo(ric);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 3);
        }

        [Test]
        public void T034_PushThroughCastingWithReplication()
        {
            //DNL-1467147 When arguments are up-converted to a var during replication, the type of the value is changed, not the type of the reference
            String code =
@"def foo(x:var){	return = bar(x) + 1;}def bar(x:int){    return = x + 1;}a = {0,1};r = foo(a);";
            thisTest.RunScriptSource(code);
            Object r = new Object[]             {                2,                3            }
            ;
            thisTest.Verify("r", r);
        }

        [Test]
        public void TV1467147_PushThroughCastingWithReplication_1()
        {
            String code =
@"def foo(x:var){	return = bar(x) + 1;}def bar(x:int){    return = x + 1;}a = {0,1.0};r = foo(a);";
            thisTest.RunScriptSource(code);
            Object r = new Object[]             {                2,                3            }
            ;
            thisTest.Verify("r", r);
        }

        [Test]
        public void TV1467147_PushThroughCastingWithReplication_2_constructor()
        {
            String code =
@"class A{    fx:bool;    constructor A(x:var)    {        fx = bar(x);    }    def bar(x:bool)    {        return = x;    }}a = A.A({0,1.1,null});r = a.fx;";
            thisTest.RunScriptSource(code);
            Object r = new Object[]             {                false,                true,                null            }
            ;
            thisTest.Verify("r", r);
        }

        [Test]
        public void T034_PushThroughCastingWithReplication_UserDefinedType()
        {
            String code =
@"def foo(x:AB){	return = bar(x) + 1;}def bar(x:Ric){    return = x.fx +1 ;}class AB{	fx : var;	constructor AB()	{ 		fx = 1;	}}class Ric extends AB{	constructor Ric() : base.AB()	{	}}ric1 = Ric.Ric();ric2 = Ric.Ric();rics = {ric1, ric2};r = foo(rics);";
            thisTest.RunScriptSource(code);
            Object[] rs = new Object[] { 3, 3 };
            thisTest.Verify("r", rs);
        }

        [Test]
        public void T035_PushThroughIntWithReplication()
        {
            String code =
@"def foo(x:int){	return = bar(x) + 1;}def bar(x:int){    return = x + 1;}a = {0,1};r = foo(a);";
            thisTest.RunScriptSource(code);
            Object r = new Object[]             {                2,                3            }
            ;
            thisTest.Verify("r", r);
        }

        [Test]
        public void T036_Replication_ArrayDimensionDispatch_SubTest_0D()
        {
            String code =
@"a = 0;def foo(x:int){	return = 7;}va = foo(a);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("va", 7);
        }

        [Test]
        public void T036_Replication_ArrayDimensionDispatch_SubTest_1D()
        {
            String code =
@"a = { 0 };def foo(x:int){	return = 7;}va = foo(a);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("va", new object[] { 7 });
        }

        [Test]
        public void T036_Replication_ArrayDimensionDispatch_SubTest_1D2()
        {
            String code =
@"a = { 0, 0 };def foo(x:int){	return = 7;}va = foo(a);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("va", new object[] { 7, 7 });
        }

        [Test]
        public void T036_Replication_ArrayDimensionDispatch_SubTest_2D()
        {
            String code =
@"a = { { 0 } };def foo(x:int){	return = 7;}va = foo(a);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("va", new object[] { new object[] { 7 } });
        }

        [Test]
        public void T036_Replication_ArrayDimensionDispatch_SubTest_2D2()
        {
            String code =
@"a = { { 0 }, {0} };def foo(x:int){	return = 7;}va = foo(a);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("va", new object[] { new object[] { 7 }, new object[] { 7 } });
        }

        [Test]
        public void T036_Replication_ArrayDimensionDispatch_SubTest_3D()
        {
            String code =
@"a = { { { 0 } } };def foo(x:int){	return = 7;}va = foo(a);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("va", new object[] { new object[] { new object[] { 7 } } });
        }

        [Test]
        public void T036_Replication_ArrayDimensionDispatch_SubTest_4D()
        {
            String code =
@"a = { { { { 0 } } } };def foo(x:int){	return = 7;}va = foo(a);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("va", new object[] { new object[] { new object[] { new object[] { 7 } } } });
        }


        [Test]
        public void T036_Replication_ArrayDimensionDispatch_SubTest()
        {
            String code =
@"c = {{1}};d = {{{1}}};def foo(x:int){	return = 7;}vc = foo(c);vd = foo(d);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("vc", new object[]                                              {                                                  new object[] {7 }                                              });
            thisTest.Verify("vd", new object[]                                              {                                                  new object[]                                                      {                                                          new object[] {7 }                                                      }                                              });
        }

        [Test]
        public void T036_Replication_ArrayDimensionDispatch()
        {
            String code =
@"z = 0;a = {};b = {1};c = {{1}};d = {{{1}}};def foo(x:int){	return = 7;}vz = foo(z);va = foo(a);vb = foo(b);vc = foo(c);vd = foo(d);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("vz", 7);
            thisTest.Verify("va", new object[0]);
            thisTest.Verify("vb", new object[] { 7 });
            thisTest.Verify("vc", new object[]                                              {                                                  new object[] {7 }                                              });
            thisTest.Verify("vd", new object[]                                              {                                                  new object[]                                                      {                                                          new object[] {7 }                                                      }                                              });
        }

        [Test]
        public void T037_Replication_ArrayDimensionDispatch_Var()
        {
            String code =
@"z = 0;a = {};b = {1};c = {{1}};d = {{{1}}};def foo(x:var){	return = 7;}vz = foo(z);va = foo(a);vb = foo(b);vc = foo(c);vd = foo(d);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("vz", 7);
            thisTest.Verify("va", new object[0]);
            thisTest.Verify("vb", new object[] { 7 });
            thisTest.Verify("vc", new object[]                                              {                                                  new object[] {7 }                                              });
            thisTest.Verify("vd", new object[]                                              {                                                  new object[]                                                      {                                                          new object[] {7 }                                                      }                                              });
        }

        [Test]
        public void T038_Replication_HigherArrayDimensionDispatch()
        {
            String code =
@"z = 0;a = {};b = {1};c = {{1}};d = {{{1}}};def foo(x:var){	return = 3;}def foo(x:var[]){	return = 7;}vz = foo(z);va = foo(a);vb = foo(b);vc = foo(c);vd = foo(d);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("vz", 3);
            thisTest.Verify("va", 7);
            thisTest.Verify("vb", 7);
            thisTest.Verify("vc",
                                                  new object[] { 7 }
                                              );
            thisTest.Verify("vd",
                                                  new object[]                                                      {                                                          new object[] {7 }                                                      }
                                              );
        }

        [Test]
        public void Z001_ReplicationGuides_Minimal_01()
        {
            String code =
@"a = 1..3;b = 1..3;def foo(i:int, j:int){	return = 3;}r = foo(a<1>, b<1>);";
            thisTest.RunScriptSource(code);
            Object[] r = new object[] { 3, 3, 3 };
            thisTest.Verify("r", r);
        }

        [Test]
        public void Z002_ReplicationGuides_Minimal_02()
        {
            String code =
@"a = 1..3;b = 1..3;def foo(i:int, j:int){	return = 3;}r = foo(a<1>, b<2>);";
            thisTest.RunScriptSource(code);
            Object[] r = new object[]                             {                                 new object[] {3, 3, 3},                                 new object[] {3, 3, 3},                                 new object[] {3, 3, 3}                             };
            thisTest.Verify("r", r);
        }

        [Test]
        public void Z003_ReplicationGuides_MultipleGuides_01_ExecAtAllCheck()
        {
            String code =
@"a = 1..(1..3);b = 1..3;def foo(i:int, j:int){	return = 3;}r = foo(a<1><3>, b<2>);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        public void R001_Replicator_Internal_ReductionsToGuides_SimpleCartesian()
        {

            List<int> reductionC0 = new List<int>() { 1 };
            List<ReplicationInstruction> instructionsC0 = Replicator.ReductionToInstructions(reductionC0);
            Assert.IsTrue(instructionsC0.Count == 1);
            Assert.IsTrue(instructionsC0[0].CartesianIndex == 0);
            Assert.IsTrue(instructionsC0[0].Zipped == false);
        }

        [Test]
        public void R001_Replicator_Internal_ReductionsToGuides_SimpleCartesian2()
        {
            List<int> reductionC0 = new List<int>() { 2 };
            List<ReplicationInstruction> instructionsC0 = Replicator.ReductionToInstructions(reductionC0);
            Assert.IsTrue(instructionsC0.Count == 2);
            Assert.IsTrue(instructionsC0[0].CartesianIndex == 0);
            Assert.IsTrue(instructionsC0[0].Zipped == false);
            Assert.IsTrue(instructionsC0[1].CartesianIndex == 0);
            Assert.IsTrue(instructionsC0[1].Zipped == false);

        }

        [Test]
        public void R002_Replicator_Internal_ReductionsToGuides_SimpleZipped()
        {
            List<int> reductionZ01 = new List<int>() { 1, 1 };
            List<ReplicationInstruction> instructionsZ01 = Replicator.ReductionToInstructions(reductionZ01);
            Assert.IsTrue(instructionsZ01.Count == 1);
            Assert.IsTrue(instructionsZ01[0].ZipIndecies.Count == 2);
            Assert.IsTrue(instructionsZ01[0].ZipIndecies.Contains(0));
            Assert.IsTrue(instructionsZ01[0].ZipIndecies.Contains(1));
            Assert.IsTrue(instructionsZ01[0].Zipped == true);
        }

        [Test]
        public void R002_Replicator_Internal_ReductionsToGuides_SimpleZipped2()
        {
            List<int> reductionZ01 = new List<int>() { 2, 2 };
            List<ReplicationInstruction> instructionsZ01 = Replicator.ReductionToInstructions(reductionZ01);
            Assert.IsTrue(instructionsZ01.Count == 2);
            Assert.IsTrue(instructionsZ01[0].ZipIndecies.Count == 2);
            Assert.IsTrue(instructionsZ01[0].ZipIndecies.Contains(0));
            Assert.IsTrue(instructionsZ01[0].ZipIndecies.Contains(1));
            Assert.IsTrue(instructionsZ01[0].Zipped == true);
            Assert.IsTrue(instructionsZ01[1].ZipIndecies.Contains(0));
            Assert.IsTrue(instructionsZ01[1].ZipIndecies.Contains(1));
            Assert.IsTrue(instructionsZ01[1].Zipped == true);
        }
        /*
[Test]        public void T039_Inheritance_()        {            String code =@"class A extends var{    fx = 0;    constructor A() : base var();    {        fx = 1;    }}a = A.A();b = a.fx;";            thisTest.RunScriptSource(code);            thisTest.Verify("fx", 1);        }*/

        [Test]
        public void T039_Inheritance_Method_1()
        {
            String code =
@"class A{	fx:int = 1;	constructor A()	{		fx = 2;	}	def foo(x:var)	{		fx = x +10;		return = fx;	}}class B extends A{	fy:var;	constructor B(): base.A()	{		fx = 10;		fy = 20;	}}b = B.B();r1 = b.fx;r2 = b.fy; r3 = b.foo(1);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 11);
            thisTest.Verify("r2", 20);
            thisTest.Verify("r3", 11);
        }

        [Test]
        public void TV1467161_Inheritance_Update_1()
        {
            String code =
@"class A{	fx:int = 1;	constructor A()	{		fx = 2;	}	def foo(x:var)	{		fx = x +10;		return = fx;	}}class B extends A{	fy:var;	constructor B(): base.A()	{		fx = 10;		fy = 20;	}    def foo(x:var)	{		fx = x +100;        fy = fx; 		return = fx;	}}b = B.B();r1 = b.fx;r2 = b.fy; r3 = b.foo(1);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 101);
            thisTest.Verify("r2", 101);
            thisTest.Verify("r3", 101);
        }

        [Test]
        public void TV1467161_Inheritance_Update_2()
        {
            String code =
@"class A{	fx:int = 1;	constructor A()	{		fx = 2;	}	def foo(x:var)	{		fx = x +10;		return = fx;	}}class B extends A{	fy:var;	constructor B(): base.A()	{		fx = 10;		fy = 20;	}    def foo(x:var)	{		fx = x +100;		return = fx;	}}class C extends B{    fz:int = 100;}c:A = C.C();r1 = c.fx;r2 = c.fy; r3 = c.fz;r4 = c.foo(1);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 101);
            thisTest.Verify("r2", 20);
            thisTest.Verify("r3", 100);
            thisTest.Verify("r4", 101);
        }

        [Test]
        public void T040_Inheritance_Dynamic_Typing_1()
        {
            String code =
@"class A{	fx:int = 1;	constructor A()	{		fx = 2;	}	def foo(x:var)	{		fx = x +10;		return = fx;	}}class B extends A{	fy:var;	constructor B(): base.A()	{		fx = 10;		fy = 20;	}}b : B = B.B();r1 = b.fx;r2 = b.fy; r3 = b.foo(1);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 11);
            thisTest.Verify("r2", 20);
            thisTest.Verify("r3", 11);
        }

        [Test]
        public void T041_Inheritance_Dynamic_Typing_2()
        {
            String code =
@"class A{	fx:int = 1;	constructor A()	{		fx = 2;	}	def foo(x:var)	{		fx = x +10;		return = fx;	}}class B extends A{	fy:var;	constructor B(): base.A()	{		fx = 10;		fy = 20;	} }a = A.A();r1 = a.fx;r2 = a.fy; r3 = a.foo(1);";
            thisTest.RunScriptSource(code);
            Object v1 = new Object();
            v1 = null;
            thisTest.Verify("r1", 11);
            thisTest.Verify("r2", v1);
            thisTest.Verify("r3", 11);
        }

        [Test]
        public void T042_Inheritance_Dynamic_Typing_3()
        {
            String code =
@"class A{	fx:int = 1;	constructor A()	{		fx = 2;	}	def foo(x:var)	{		fx = x +10;		return = fx;	}}class B extends A{	fy:var;	constructor B(): base.A()	{		fx = 10;		fy = 20;	} }a:A = A.A();r1 = a.fx;r2 = a.fy; r3 = a.foo(1);";
            thisTest.RunScriptSource(code);
            Object v1 = new Object();
            v1 = null;
            thisTest.Verify("r1", 11);
            thisTest.Verify("r2", v1);
            thisTest.Verify("r3", 11);
        }

        [Test]
        public void T044_Function_Overriding_NoArgs()
        {
            String code =
@"class A{	fx:int = 1;	constructor A()	{		fx = 2;	}	def foo()	{		return = 100;	}}class B extends A{	fy:var;	constructor B(): base.A()	{		fx = 10;		fy = 20;	} 		def foo()	{		return = 200;	}}a = A.A();b = B.B();r1 = a.foo();r2 = b.foo();";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 100);
            thisTest.Verify("r2", 200);

        }

        [Test]
        public void T043_Function_Overriding_1()
        {
            String code =
@"class A{	fx:int = 1;	constructor A()	{		fx = 2;	}	def foo(x:var)	{		fx = x +10;		return = fx;	}}class B extends A{	fy:var;	constructor B(): base.A()	{		fx = 10;		fy = 20;	} 		def foo(x:var)	{		fx = x;		return = fx;	}}a = A.A();b = B.B();r1 = a.foo(1);r2 = b.foo(100);r3 = a.fx;r4 = b.fx;";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 11);
            thisTest.Verify("r2", 100);
            thisTest.Verify("r3", 11);
            thisTest.Verify("r4", 100);

        }

        [Test]
        public void T043_Function_Overriding_2()
        {
            String code =
@"class A{	fx:int = 1;	constructor A()	{		fx = 2;	}	def foo(x:int)	{		fx = x +10;		return = fx;	}}class B extends A{	fy:var;	constructor B(): base.A()	{		fx = 10;		fy = 20;	} 		def foo(x:int)	{		fx = x;		return = fx;	}}a = A.A();b = B.B();r1 = a.foo(1);r2 = b.foo(100);r3 = a.fx;r4 = b.fx;";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 11);
            thisTest.Verify("r2", 100);
            thisTest.Verify("r3", 11);
            thisTest.Verify("r4", 100);

        }

        [Test]
        [Category("Failure")]
        public void TV1467063_Function_Overriding()
        {
            // Tracked by: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4054
            string err = "";
            String code =
@"class A{	fx:int = 1;	constructor A()	{		fx = 2;	}	def foo(x:int)	{		fx = x +10;		return = fx;	}}class B extends A{	fy:var;	constructor B(): base.A()	{		fx = 10;		fy = 20;	} 		def foo(x:var)	{		fx = x;		return = fx;	}}a = A.A();b = B.B();r1 = a.foo(1);r2 = b.foo(100);r3 = a.fx;r4 = b.fx;";
            thisTest.RunScriptSource(code, err);
            thisTest.SetErrorMessage("MAGN-4054: Function overriding: when using function overriding, wrong function is called");
            thisTest.Verify("r1", 11);
            thisTest.Verify("r2", 100);
            thisTest.Verify("r3", 11);
            thisTest.Verify("r4", 100);
        }

        [Test]
        public void T045_Inheritance_Method_02()
        {
            String code =
@"class A{	fx:int;	constructor A()	{		fx = 0;	}	def foo(x:double)	{		fx = x;		return = fx;	}}class B extends A{	fy:var;	constructor B(): base.A()	{		fx = 10;		fy = 20;	} 		def foo(x:int)	{		fx = x +10;		return = fx;	}}b = B.B();r1 = b.foo(1.0);r2 = b.foo(1);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 1);
            thisTest.Verify("r2", 11);
        }

        [Test]
        public void T046_Inheritance_Method_03()
        {
            String code =
@"class A{	fx:int;	constructor A()	{		fx = 0;	}	def foo(x:int)	{		fx = x;		return = fx;	}}class B extends A{	fy:var;	constructor B(): base.A()	{		fx = 10;		fy = 20;	} 		def foo(x:double)	{		fx = x +10;		return = fx;	}}b = B.B();r1 = b.foo(1.0);r2 = b.foo(1);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 11);
            thisTest.Verify("r2", 1);
        }

        [Test]
        public void T047_Inheritance_Method_04()
        {
            String code =
@"class A{	fx:int;	constructor A()	{		fx = 0;	}	def foo(x:int)	{		fx = x;		return = fx;	}}class B extends A{	fy:var;	constructor B(): base.A()	{		fx = 10;		fy = 20;	} 		def foo(x:var)	{		fx = x +10;		return = fx;	}}b = B.B();r1 = b.foo(1.0);r2 = b.foo(1);";
            thisTest.RunScriptSource(code);
            //Assert.Fail("1467175 - sprint 25 - Rev 3150: [design issue] :cast distance to a var comparing with cast distance to a double ");
            thisTest.Verify("r1", 11);
            thisTest.Verify("r2", 1);
        }

        [Test]
        public void TV1467175_1()
        {
            String code =
@"def foo(x : double){    return = x ;}def foo(x : var){    return = x ;}r = foo(1);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 1.0);
        }

        [Test]
        public void TV1467175_2()
        {
            String code =
@"class A{def foo(x : double){    return = x ;}def foo(x : var){    return = x ;}}a = A.A();r = a.foo(1);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 1.0);
        }

        [Test]
        public void TV1467175_3()
        {
            String code =
@"class A{def foo(x : double){    return = x ;}}class B extends A{def foo(x : var){    return = x ;}}a = A.A();b = B.B();ra = a.foo(1);rb = b.foo(1);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("ra", 1.0);
            thisTest.Verify("rb", 1.0);
        }

        [Test]
        public void TV1467175_4()
        {
            String code =
@"class A{def foo(x : var){    return = x ;}}class B extends A{def foo(x : double){    return = x ;}}a = A.A();b = B.B();ra = a.foo(1);rb = b.foo(1);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("ra", 1);
            thisTest.Verify("rb", 1.0);
        }

        [Test]
        public void T049_Inheritance_Update_01()
        {
            String code =
@"    class A{    fx : int = 1;    def foo()    {        fx = 11;        return = fx;    }    }class B extends A{    def foo()    {        fx = 22;        return = fx;    }}a = A.A();b = B.B();r1 = a.fx;r2 = b.fx;r3 = a.foo();r4 = b.foo();";
            thisTest.RunScriptSource(code);
            // Assert.Fail("1467167 - Sprint 25 - Rev 3132: class member doesn't get updated correctly");
            thisTest.Verify("r1", 11);
            thisTest.Verify("r2", 22);
            thisTest.Verify("r3", 11);
            thisTest.Verify("r4", 22);
        }

        [Test]
        public void T049_Inheritance_Update_02()
        {
            String code =
@"class A{    fx : int = 1;    def foo()    {        fx = 11;        return = fx;    }    }class B extends A{    def foo()    {        fx = 22;        return = fx;    }}class C extends A{    def foo()    {        fx = 33;        return = fx;    }}a = A.A();b = B.B();c = C.C();r1 = a.fx;r2 = b.fx;r3 = c.fx;r4 = a.foo();r5 = b.foo();r6 = c.foo();";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 11);
            thisTest.Verify("r2", 22);
            thisTest.Verify("r3", 33);
            thisTest.Verify("r4", 11);
            thisTest.Verify("r5", 22);
            thisTest.Verify("r6", 33);
        }

        [Test]
        public void T049_Inheritance_Update_03()
        {
            String code =
@"class A{    fx : int = 1;    def foo()    {        fx = 11;        return = fx;    }    }class B extends A{    fy:int;    def foo()    {        fx = 22;        return = fx;    }}class C extends B{    def foo()    {        fy = 33;        return = fy;    }}a = A.A();b = B.B();c = C.C();r1 = a.fx;r2 = b.fx;r3 = c.fy;r4 = a.foo();r5 = b.foo();r6 = c.foo();";
            thisTest.RunScriptSource(code);
            thisTest.SetErrorMessage("1467167 - Sprint 25 - Rev 3132: class member doesn't get updated correctly");
            thisTest.Verify("r1", 11);
            thisTest.Verify("r2", 22);
            thisTest.Verify("r3", 33);
            thisTest.Verify("r4", 11);
            thisTest.Verify("r5", 22);
            thisTest.Verify("r6", 33);
        }

        [Test]
        public void TV1467167()
        {
            String code =
@"class A{    fx : int = 1;    def foo()    {        fx = 11;        return = fx;    }    }class B extends A{    def foo()    {        fx = 22;        return = fx;    }    def foo(x:int)    {        fx = x;        return = fx;    }    def fooB()    {        return = fx;    }}class C extends B{    def foo()    {        fx = 33;        return = fx;    }}a = A.A();b:A = B.B();c = C.C();r1 = a.fx;r2 = b.fx;r3 = c.fx;r4 = a.foo();r5 = b.foo();r6 = c.foo();r7 = c.fooB();a.fx = 111;b.fx = 222;c.fx = 333;r8 = c.foo(3);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 111);
            thisTest.Verify("r2", 222);
            thisTest.Verify("r3", 333); // c.fx = 333 will re-execute as it is lhs dependent
            thisTest.Verify("r4", 11);
            thisTest.Verify("r5", 22);
            thisTest.Verify("r6", 33);
            thisTest.Verify("r7", 33);
            thisTest.Verify("r8", 3);
        }


        [Test]
        public void T050_Transitive_Inheritance_01()
        {
            String code =
@"class A{    fx : int = 1;    def foo()    {        fx = 11;        return = fx;    }    }class B extends A{    fy : int = 2;}class C extends B{    fz : int = 3;}c = C.C();r1 = c.fx;r2 = c.fy;r3 = c.fz;r4 = c.foo();";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 11);
            thisTest.Verify("r2", 2);
            thisTest.Verify("r3", 3);
            thisTest.Verify("r4", 11);
        }

        [Test]
        public void T050_Transitive_Inheritance_02()
        {
            String code =
@"class A{    fx : int = 1;    def foo()    {        fx = 11;        return = fx;    }    }class B extends A{    fy : int = 2;    def foo()    {        fy = 22;        return = fy;    }}class C extends B{    fz : int = 3;}c = C.C();r1 = c.fx;r2 = c.fy;r3 = c.fz;r4 = c.foo();";
            thisTest.RunScriptSource(code);
            thisTest.SetErrorMessage("1467167 - Sprint 25 - Rev 3132: class member doesn't get updated correctly");
            thisTest.Verify("r1", 1);
            thisTest.Verify("r2", 22);
            thisTest.Verify("r3", 3);
            thisTest.Verify("r4", 22);
        }

        [Test]
        public void T050_Inheritance_Multi_Constructor_01()
        {
            String code =
@"class A{    fx : int;    constructor A()    {        fx = 0;    }            constructor A(x:var)    {        fx = x+1;    }    constructor A(x:int)    {        fx = x+2;    }         constructor A2(x:var)    {        fx = x+3;    }    }class B extends A{    constructor B() : base.A() { }    constructor B(x : var) : base.A(x) { }    constructor B(x : int) : base.A(x) { }    constructor B(x : var) : base.A2(x) { }    constructor B(x : int) : base.A2(x) { }}b1 = B.B();r1 = b1.fx;b2 = B.B(0);r2 = b2.fx;b3 = B.B(0.0);r3 = b3.fx;b4 = B.B(A.A()); //nullr4 = b4.fx;    ";
            thisTest.RunScriptSource(code);
            Object v1 = null;
            thisTest.Verify("r1", 0);
            thisTest.Verify("r2", 1);
            thisTest.Verify("r3", 1);
            thisTest.Verify("r4", v1);
        }

        [Test]
        [Category("ToFixJun")]
        [Category("Failure")]
        [Category("Class")]
        public void T051_TransitiveInheritance_Constructor()
        {
            String code =
@"class A{    fx : int;    constructor A(x : int)    {        fx = x+1;    }    constructor A(x : double)    {        fx = x+2;    }}class B extends A{    constructor B(x : int) : base.A(x) { }    constructor B(x : double) : base.A(x) { }}class C extends B{    constructor C(x : int) : base.B(x) { }    constructor C(x : double) : base.B(x) { }}c1 = C.C();c2 = C.C(1);c3 = C.C(1.0);r1 = c1.fx;r2 = c2.fx;r3 = c3.fx;";
            // Tracked by: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4050
            string err = "MAGN-4050 Transitive inheritance base constructor causes method resolution failure";
            thisTest.RunScriptSource(code, err);
            thisTest.Verify("r1", 0);
            thisTest.Verify("r2", 2);
            thisTest.Verify("r3", 3.0); // Actual result is 2 as C.C(1.0) instead of calling double overload of ctor A calls its int counterpart instead??
        }

        [Test]
        public void T050_Inheritance_Multi_Constructor_02()
        {
            String code =
@"class A{    fx : int;    constructor A()    {        fx = 0;    }            constructor A(x:var)    {        fx = x+1;    }    constructor A(x:int)    {        fx = x+2;    }         constructor A2(x:var)    {        fx = x+3;    }    }class B extends A{    constructor B() : base.A() { }    constructor B(x : var) : base.A(x) { }    constructor B(x : int) : base.A(x) { }    constructor B2(x : var) : base.A2(x) { }    constructor B2(x : int) : base.A2(x) { }}b1 = B.B();r1 = b1.fx;b2 = B.B(0);r2 = b2.fx;b3 = B.B2(0.0);r3 = b3.fx;b4 = B.B2(A.A()); //nullr4 = b4.fx;b5 = B.B2(0);r5 = b5.fx;    ";
            thisTest.RunScriptSource(code);
            //Assert.Fail("1467179 - Sprint25 : rev 3152 : multiple inheritance base constructor causes method resolution");
            Object v1 = null;
            thisTest.Verify("r1", 0);
            thisTest.Verify("r2", 1);
            thisTest.Verify("r3", 3);
            thisTest.Verify("r4", v1);
        }

        [Test]
        public void T052_Defect_ReplicationMethodOverloading()
        {
            String code =
@"class A                                {                                }                                a1 = A.A();                                def foo(val : int[])                                {                                    return = 1;                                }                                def foo(val : var)                                {                                    return = 2;                                }                                                                arr = { 3, a1, 5 } ;                                r = foo(arr);    ";
            thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 2, 2, 2 };
            thisTest.Verify("r", v1);
        }

        [Test]
        [Category("Method Resolution")]
        [Category("Failure")]
        public void T052_Defect_ReplicationMethodOverloading_2()
        {
            String code =
@"class A{}a1 = A.A();def foo(val : int[]){    return = 1;}def foo(val : var){    return = 2;}def foo(val: var[]){	return = 3;}                                arr = { {3}, a1, 5,{a1} } ;//1,2,2,3r = foo(arr);    ";
            // Tracked by: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4052
            thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 1, 2, 2, 3 };
            thisTest.Verify("r", v1);
        }

        [Test]
        [Category("Method Resolution")]
        [Category("Failure")]
        public void TV052_Defect_ReplicationMethodOverloading_01()
        {
            String code =
@"class A{}a1 = A.A();def foo(val : int[]){    return = 1;}def foo(val : var){    return = 2;}def foo(val: var[]){	return = 3;}                                arr = { {3}, a1, 5,{{a1}} } ;//1,2,2,nullr = foo(arr);";
            // Tracked by: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4052
            string err = "MAGN-4052 Replication and Method overload issue, resolving to wrong method";
            thisTest.RunScriptSource(code, err);
            Object[] v1 = new Object[] { 1, 2, 2, null };
            thisTest.Verify("r", v1);
        }

        [Test]
        [Category("Method Resolution")]
        public void TV052_Defect_ReplicationMethodOverloading_02()
        {
            String code =
@"class A{}a1 = A.A();def foo(val : int[]){    return = 1;}def foo(val : var){    return = 2;}def foo(val: var[]..[]){	return = 3;}def foo(val: var[]){	return = 4;}                                arr = { {3}, a1, 5,{{a1}} } ;//3r = foo(arr);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 3);
        }

        [Test]
        [Category("Method Resolution")]
        [Category("Failure")]
        public void TV052_Defect_ReplicationMethodOverloading_03()
        {
            String code =
@"class A{}a1 = A.A();def foo(val : int[]){    return = 1;}def foo(val : var){    return = 2;}def foo(val: var[]){	return = 3;}                                arr = { {3}, a1, 5.0,{{a1}} } ;//1,2,2,nullr = foo(arr);";
            // Tracked by: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4052
            string err = "MAGN-4052 Replication and Method overload issue, resolving to wrong method";
            thisTest.RunScriptSource(code, err);
            Object[] v1 = new Object[] { 1, 2, 2, null };
            thisTest.Verify("r", v1);
        }

        [Test]
        [Category("Method Resolution")]
        [Category("Failure")]
        public void TV052_Defect_ReplicationMethodOverloading_InUserDefinedClass()
        {
            String code =
@"class A{        def foo(val : int[])    {        return = 11;    }    def foo(val : var)    {        return = 22;    }    def foo(val: var[])    {	    return = 33;    }}a1 = A.A();a2 = A.A();def foo(val : int[]){    return = 1;}def foo(val : var){    return = 2;}def foo(val: var[]){	return = 3;}                                arr = { {3}, a1, 5.0,{{a1}} } ;//1,2,2,nullr = foo(arr);r2= a2.foo(arr);";
            // Tracked by: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4052
            string err = "MAGN-4052 Replication and Method overload issue, resolving to wrong method";
            thisTest.RunScriptSource(code, err);
            Object[] v1 = new Object[] { 1, 2, 2, null };
            Object[] v2 = new Object[] { 11, 22, 22, null };
            thisTest.Verify("r", v1);
            thisTest.Verify("r2", v2);
        }
        /*                
[Test]                public void T050_Inheritance_Multi_Construc()                {                    String code =        @"        class A        {            fx : int;            constructor A()            {                fx = 0;            }                    constructor A(x:var)            {                fx = x+1;            }            constructor A(x:int)            {                fx = x+2;            }                 constructor A2(x:var)            {                fx = x+3;            }            }        class B extends A        {            constructor B() : base.A() { }            constructor B(x : var) : base.A(x) { }            constructor B(x : int) : base.A(x) { }            constructor B2(x : var) : base.A2(x) { }            constructor B2(x : int) : base.A2(x) { }        }        b1 = B.B();        r1 = b1.fx;        b2 = B.B(0);        r2 = b2.fx;        b3 = B.B2(0.0);        r3 = b3.fx;        b4 = B.B2(A.A()); //null        r4 = b4.fx;        b5 = B.B2(0);        r5 = b5.fx;            ";                    thisTest.RunScriptSource(code);                    Assert.Fail("1467179 - Sprint25 : rev 3152 : multiple inheritance base constructor causes method resolution");                    Object v1 = null;                    thisTest.Verify("r1", 0);                    thisTest.Verify("r2", 1);                    thisTest.Verify("r3", 1);                    thisTest.Verify("r4", v1);                }                */

        [Test]
        [Category("Failure")]
        public void T053_ReplicationWithDiffTypesInArr()
        {
            String code =
@"class A{    def foo()    {        return = 1;    }}class B{    def foo()    {        return = 2;    }}m = { A.A(), B.B(), A.A() };n = { B.B(), A.A(), B.B() };r1 = m.foo();r2 = n.foo();";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1693
            string str = "MAGN-1693 Regression : Dot Operation using Replication on heterogenous array of instances is yielding wrong output";
            thisTest.VerifyRunScriptSource(code, str);

            Object[] v1 = new Object[] { 1, 2, 1 };
            Object[] v2 = new Object[] { 2, 1, 2 };
            thisTest.Verify("r1", v1);
            thisTest.Verify("r2", v2);
        }

        [Test]
        [Category("Failure")]
        public void T054_ReplicationWithInvalidTypesInArr()
        {
            String code =
@"class A{    def foo()    {        return = 1;    }}class B{    def foo()    {        return = 2;    }}m = { A.A(), B.B(), null };n = { B.B(), A.A(), c, false };r1 = m.foo();r2 = n.foo();";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1693
            string str = "MAGN-1693 Regression : Dot Operation using Replication on heterogenous array of instances is yielding wrong output";
            thisTest.RunScriptSource(code, str);
            Object v3 = null;
            Object[] v1 = new Object[] { 1, 2, v3 };
            Object[] v2 = new Object[] { 2, 1, v3, v3 };

            thisTest.Verify("r1", v1);
            thisTest.Verify("r2", v2);
        }
        
        [Test]
        public void T055_ReplicationWithDiffTypesInArr_UserDefined_Simpler()
        {
            String code =
@"class A{def foo(a:A){    return = 1;}}class B extends A{def foo(b:B){    return = 2;}}m = {A.A(),B.B(),A.A()};a = A.A();r1a = a.foo(m);b = B.B();r1b = b.foo(m);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1a", new object[] { 1, 1, 1 });
            thisTest.Verify("r1b", new object[] { 1, 2, 1 });
        }

        [Test]
        public void ReplicationSubDispatch()
        {
            String code =
@"def foo (i : int){ return=1;}def foo (d: double){ return=2;}m = {4, 5.0, 56, 6.3};ret = foo(m);";
            thisTest.RunScriptSource(code);

            thisTest.Verify("ret", new object[] { 1, 2, 1, 2 });
        }

        [Test]
        public void Test()
        {
            String code =
@"class A{        def Test(b : B)        { return = 1; }}class B extends A{        def Test(a : A)        { return = 2; }} a = A.A();b = B.B();r1 = a.Test(a);//nullr2 = b.Test(b);//2";
            thisTest.RunScriptSource(code);
        }

        [Test]
        public void T056_nonmatchingclass_1467162()
        {
            String code =
            @"                class A                {                    fa = 1;                }                a:M = A.A();//            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", null);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.Runtime.WarningID.kConversionNotPossible);
        }

        [Test]
        public void T057_nonmatchingclass_1467162_2()
        {
            String code =
            @"                class A                {                    fa = 1;                }                class M {};                a:M = A.A();            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", null);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.Runtime.WarningID.kConversionNotPossible);
        }

        [Test]
        public void T058_nonmatchingclass_1467162_3()
        {
            String code =
            @"                class A                {                    fa = 1;                }                class M extends A{};                a:M = A.A();            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", null);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.Runtime.WarningID.kConversionNotPossible);
        }

        [Test]
        public void T059_Polymphism()
        {
            String code =
            @"class A{    private def bar()    {        return = ""A.bar()"";    }    public def foo()    {        return = bar();    }}class B extends A{}b = B.B();ret = b.foo();            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("ret", "A.bar()");
        }

        [Test]
        public void T059_Polymphism_2()
        {
            String code =
            @"class A{    private def bar()    {        return = ""A.bar()"";    }    public def foo()    {        return = bar();    }}class B extends A{    private def bar()    {        return = ""B.bar()"";    }}b = B.B();ret = b.foo();            ";
            string error = "1467150 Sprint 25 - Rev 3026 - Polymorphism in design script broken when nested function calls between the different class ";
            thisTest.RunScriptSource(code, error);
            thisTest.Verify("ret", "B.bar()");
        }

        [Test]
        public void T059_Polymphism_3()
        {
            String code =
            @"class A{    private def bar()    {        return = ""A.bar()"";    }}class B extends A{    public def foo()    {        return = bar();    }}b = B.B();ret = b.foo();            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("ret", null);
        }

        [Test]
        public void T059_Polymphism_4()
        {
            String code =
            @"class A{    public def foo()    {        return = bar();    }}class B extends A{    private def bar()    {        return = ""B.bar()"";    }}b = B.B();ret = b.foo();            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("ret", null);
        }

        [Test]
        public void T059_Polymphism_5()
        {
            String code =
            @"                class Geometry                {                        def Clone()                        {                            return = CreateNew();                        }                        def CreateNew()                        {                             return=0;                        }                }                class Curve extends Geometry                {                }                class Arc extends Curve                {                        def CreateNew()                        {                             return = 100;                        }                }                a=Arc.Arc();                g=a.Clone();             ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("g", 100);
        }

        [Test]
        public void T060_DispatchOnArrayLevel()
        {
            String code =
            @"                def foo(x:var[]..[]){return = 2;}def foo(x:var[]){return = 1;}d = foo({1,2});            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("d", 1);
        }

        [Test]
        public void T060_DispatchOnArrayLevel_1()
        {
            String code =
            @"                def foo(x:var[]..[]){return = 2;}def foo(x:var[]){return = 1;}d = foo({ { 1 } , { 2} });            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("d", 2);
        }

        [Test]
        public void T060_DispatchOnArrayLevel_2()
        {
            String code =
            @"def foo(x:int[]..[]){return = 2;}def foo(x:int[]){return = 1;}d = foo({1,2});            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("d", 1);
        }


        [Test]
        public void RepoTests_MAGN3177()
        {
            String code =
                @"def foo(str: string, para : string[], arg : var[]){    Print(str);    Print(para);    Print(arg);}" +   "\n s = \"str\";\n" +   "paras = {\"a\",\"b\"}; \n" +   "args = {{2,5},{5}}; \n"+"foo(s, paras, args);";
            thisTest.RunScriptSource(code);
            //thisTest.Verify("d", 1);
        }

        /// <summary>
        /// As member function is overloaded with %thisptr as the first
        /// parameter, this test case tries to verify that method resolution
        /// work properly for overloaded member function and non-overloaded
        /// member function which has same signature. E.g.,
        /// 
        ///     void foo(x: X, y: X);
        ///     void foo(%thisptr:X, x:X);
        ///     
        /// </summary>
        [Test]
        public void TestMethodResolutionForThisPtrs1()
        {
            string code = @"
class A
{
    def foo()
    {
        return = 41;
    }

    def foo(x : A)
    {
        return = 42;
    }

    def foo(x : A, y: A)
    {
        return = 43;
    }

    def foo(x : A, y: A, z:A)
    {
        return = 44;
    }
}

a = A();
r1 = a.foo();
r2 = a.foo(a);
r3 = a.foo(a,a);
r4 = a.foo(a,a,a);
";

            thisTest.RunScriptSource(code);
            thisTest.VerifyBuildWarningCount(0);
            thisTest.Verify("r1", 41);
            thisTest.Verify("r2", 42);
            thisTest.Verify("r3", 43);
            thisTest.Verify("r4", 44);
        }

        [Test]
        public void TestMethodResolutionForThisPtrs2()
        {
            string code = @"
class A
{
    def foo(x: int)
    {
        return = 41;
    }

    static def foo(x : A, y: int)
    {
        return = 42;
    }
}

a = A();
r1 = a.foo(1);
r2 = a.foo(a, 1);
";

            thisTest.RunScriptSource(code);
            thisTest.VerifyBuildWarningCount(0);
            thisTest.Verify("r1", 41);
            thisTest.Verify("r2", 42);
        }

        [Test]
        public void TestMethodResolutionForThisPtrs3()
        {
            string code = @"
class A
{
    def foo(x: int)
    {
        return = 41;
    }

    static def foo(x : A, y: int)
    {
        return = 42;
    }
}

a = A();
r1 = a.foo({1});
r2 = a.foo(a, {1});
";

            thisTest.RunScriptSource(code);
            thisTest.VerifyBuildWarningCount(0);
            thisTest.Verify("r1", new object[] {41});
            thisTest.Verify("r2", new object[] {42});
        }
    }
}

