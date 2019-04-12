using System;
using NUnit.Framework;
using ProtoTestFx.TD;
namespace ProtoTest.Associative
{
    class MethodsFocusTeam : ProtoTestBase
    {
        [Test]
        public void SimpleCtorResolution01()
        {
            String code =
@"    import(""FFITarget.dll"");	p = TestObjectA.TestObjectA();    p.Set(2);	x = p.a;";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 2);
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
        public void SimpleCtorResolution02()
        {
            String code =
@"    import(""FFITarget.dll"");	p = TestObjectA.TestObjectA(2);	x = p.a;";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 2);
        }

        [Test]
        public void T002_DotOp_DefautConstructor_IntProperty()
        {
            String code =
@"    import(""FFITarget.dll"");	c = ClassFunctionality.ClassFunctionality(1);	x = c.IntVal;";
            thisTest.RunScriptSource(code);
            //Assert.Fail("0.0 should not be evaluated to be the same as 'null' in verification");
            thisTest.Verify("x", 1);
        }

        [Test]
        public void T002_DotOp_DefautConstructor_DoubleProperty()
        {
            String code =
@"    import(""FFITarget.dll"");	p = DummyPoint.ByCoordinates(1.0, 2.0, 3.0);    x = p.X;";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 1.0);
        }

        [Test]
        public void T002_DotOp_DefautConstructor_ArrayProperty()
        {
            String code =
@"    import(""FFITarget.dll"");	p = ArrayMember.Ctor([1,2,3,4,5]);    x = p.X;";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", new object[] { 1, 2, 3, 4, 5 });
        }

        [Test]
        public void T006_DotOp_SelfDefinedConstructor_01()
        {
            String code =
@"    import(""FFITarget.dll"");	p = ClassFunctionality.ClassFunctionality(10);	x = p.IntVal;";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 10);
        }

        [Test]
        [Category("Class")]
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
        public void T008_DotOp_MultiConstructor_01()
        {
            String code =
@"	    import(""FFITarget.dll"");	p1 = TestObjectA.TestObjectA();    p1.Set(1);	x = p1.a;	p2 = TestObjectA.TestObjectA(2);	y = p2.a;";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 1);
            thisTest.Verify("y", 2);
        }

        [Test]
        public void T009_DotOp_FuncCall()
        {
            String code =
@"    import(""FFITarget.dll"");	p1 = DummyPoint.ByCoordinates(1.0, 2.0, 3.0);	p2 = p1.Translate(1.0, 1.0, 1.0);    x = p2.X;    y = p2.Y;    z = p2.Z;    ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 2.0);
            thisTest.Verify("y", 3.0);
            thisTest.Verify("z", 4.0);
        }

        [Test]
        public void T010_DotOp_Property()
        {
            String code =
@"    import(""FFITarget.dll"");	p1 = DummyPoint.ByCoordinates(1.0, 2.0, 3.0);	p2 = p1.Translate(1.0, 1.0, 1.0);    x = p2.X;    y = p2.Y;    z = p2.Z + 1;	";
            thisTest.RunScriptSource(code);
            thisTest.Verify("z", 5.0);
        }

        [Test]
        public void ArrayInFunction()
        {
            String code =
@"def foo(fz:int){    return =  [0,1,2] + fz;}n = foo(1);	";
            thisTest.RunScriptSource(code);
            Object[] v2 = new Object[] { 1, 2, 3 };
            thisTest.Verify("n", v2);
        }

        [Test]
        [Category("Class")]
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
        [Category("Class")]
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
        [Category("Class")]
        public void TV1467372_ThisKeyword_2()
        {
            String code =
@"class A {    fx:int;    constructor A(x)    {        this.fx = 0;        this.fx = this.fx +x;    }}a = A.A(1);ra = a.fx;";
            thisTest.RunScriptSource(code);
            thisTest.Verify("ra", 1);
        }

        [Test]
        [Category("Class")]
        public void TV1467372_ThisKeyword_2_Replication()
        {
            String code =
@"class A {    fx:int;    constructor A(x)    {        this.fx = 0;        this.fx = this.fx +x;    }}a = A.A([1,1]);ra = a.fx;";
            thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 1, 1 };
            thisTest.Verify("ra", v1);
        }

        [Test]
        [Category("Class")]
        public void TV1467372_ThisKeyword_3()
        {
            String code =
@"class A {    fx:int;    constructor A(x:int)    {        this.fx = 0;        this.fx = this.fx +x;    }}class B extends A{    constructor B(y:int)    {        this.fx = 0;        this.fx = this.fx + y;    }}a = A.A([1,1]);b = B.B([2,2]);ra = a.fx;rb = b.fx;";
            thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 1, 1 };
            Object[] v2 = new Object[] { 2, 2 };
            thisTest.Verify("ra", v1);
            thisTest.Verify("rb", v2);
        }

        [Test]
        public void T015_DotOp_Collection_01()
        {
            String code =
@"
import(""FFITarget.dll"");p = DummyPoint.ByCoordinates(1..3, 20, 30);a = p.X[0];b = p[0].X;r1 = p.X[0] == p[0].X ? true : false;";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", true);
        }

        [Test]
        public void T015_DotOp_Collection_01a()
        {
            String code =
@"import(""FFITarget.dll"");p = DummyPoint.ByCoordinates(1..3, 20, 30);r1 = (p.X[0] == p[0].X);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", true);
        }

        [Test]
        public void T016_Collection_02()
        {
            String code =
@"def A(x : var[][]){	return = x;	}a = [[0],[1],[2]];fa = A(a);r1 = fa;";
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
@"	def A(x : var[][]){	return = x;	}a = [[0],[1],[2]];fa = A(a);r1 = fa[1][0];";
            thisTest.RunScriptSource(code);

            thisTest.Verify("r1", 1);
        }

        [Test]
        public void T018_DotOp_Collection_04()
        {
            String code =
                    @"                     import(""FFITarget.dll"");	c = ClassFunctionality.ClassFunctionality([1,2]);	x = c.IntVal;                    ";
            thisTest.RunScriptSource(code);
            Object[] v = { 1, 2 };
            thisTest.Verify("x", v);

        }

        [Test]
        public void TV018_DotOp_Collection_04_1()
        {
            String code =
@"import(""FFITarget.dll"");	c = ClassFunctionality.ClassFunctionality([1,2] + 1);	x = c.IntVal;";
            thisTest.RunScriptSource(code);
            Object[] v = { 2, 3 };
            thisTest.Verify("x", v);
        }

        [Test]
        public void TV018_DotOp_Collection_04_2()
        {
            String code =
@"def foo(x:int){    return = x;}r:int = foo([1,2]);";
            thisTest.RunScriptSource(code);
            Object v1 = null;
            thisTest.Verify("r", v1);
        }

        [Test]
        public void TV018_DotOp_Collection_04_3()
        {
            String code =
                    @"                   import(""FFITarget.dll"");	c = [ClassFunctionality.ClassFunctionality(1), ClassFunctionality.ClassFunctionality(2)];	x = c.IntVal;                    ";
            thisTest.RunScriptSource(code);
            Object[] v = { 1, 2 };
            thisTest.Verify("x", v);
        }

        [Test]
        public void TV018_DotOp_Collection_04_4()
        {
            String code =
                    @"    import(""FFITarget.dll"");	c = [ClassFunctionality.ClassFunctionality(1), ClassFunctionality.ClassFunctionality(2)];	x = c.IntVal + 1;                    ";
            thisTest.RunScriptSource(code);
            Object[] v = { 2, 3 };
            thisTest.Verify("x", v);                    
        }

        [Test]
        public void DotCallOnEmptyList()
        {
            string code = @"x = [[], []]; y = x.foo();";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", new object [] { new object [] { }, new object [] { } });
        }

        [Test]
        public void T020_Replication_Var()
        {
            String code =
@"def foo(x:var){	return = x+1;}a = [1,2];r = foo(a);";
            thisTest.RunScriptSource(code);
            Object v1 = new Object[] { 2, 3 };
            thisTest.Verify("r", v1);
        }

        [Test]
        public void T019_DotOp_Collection_05()
        {
            String code =
@" import(""FFITarget.dll"");	c = [ClassFunctionality.ClassFunctionality(1), ClassFunctionality.ClassFunctionality(2)];	x = c.IntVal[0];";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 1);

        }

        [Test]
        public void T019_DotOp_Collection_06()
        {
            String code =

@" import(""FFITarget.dll"");	c = [ClassFunctionality.ClassFunctionality([1,2]), ClassFunctionality.ClassFunctionality([3,4])];	x = c.IntVal[0][1];";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 2);


        }


        [Test]
        public void T019_DotOp_Collection_07()
        {
            String code =

@" import(""FFITarget.dll"");	c = [ClassFunctionality.ClassFunctionality([1,2]), ClassFunctionality.ClassFunctionality([3,4])];	x = c.IntVal[1][0];";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 3);


        }

        [Test]
        [Category("Class")]
        public void TV1467137_1_DotOp_Update()
        {
            String code =
@"class A {	fx: int;	fb: B[];		constructor A(x :int)	{		fx = x;		fb = B.B([10,11]);		}}class B{	fy : int;	constructor B(y : int)	{		fy = y;	}}a = [1,2];va = A.A(a);r1 = va[0].fb.fy;r2 = va.fb[0].fy;r3 = va.fb.fy[0];";
            thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 10, 11 };
            thisTest.Verify("r1", v1);
            thisTest.Verify("r2", v1);
            thisTest.Verify("r3", v1);
        }

        [Test]
        [Category("Class")]
        public void TV1467333()
        {
            String code =
@"class A {         fb: B  ;      constructor B()    {        fb = B.B([10,11]);    }    } class B {         constructor B(y : int)         {        }} va = A.A();s = va.fb;n = a.fb;m:B = B.B([10,11]);r = m==n;";
            thisTest.RunScriptSource(code);
            thisTest.SetErrorMessage("1467333 - Sprint 27 - Rev 3959: when initializing class member, array is converted to not indexable type, which gives wrong result");
            Object v1 = null;
            //thisTest.Verify("va", v1); (as the constuction of the object is valid even if the assignment fails)
            thisTest.Verify("m", v1);
            thisTest.Verify("s", v1);
            thisTest.Verify("r", true);
        }

        [Test]
        public void T025_DotOp_FuncCall_04()
        {
            String code =
@"import(""FFITarget.dll"");p = DummyPoint.ByCoordinates(1..3, 20, 30);r = p.X;";
            thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 1.0, 2.0, 3.0 };
            thisTest.Verify("r", v1);

        }

        [Test]
        [Category("Class")]
        public void TV025_1467140_1()
        {
            String code =
@"class A{	fb : B;	x: int;constructor A(x:int){		this.x = x;        this.fb = B.B(x);}}class B{    b:int;    constructor B(x:int)    {        this.b = x;    }}a = A.A([0,1]);r = a.x;r2 = a.fb.b;";
            thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 0, 1 };
            Object[] v2 = new Object[] { 0, 1 };
            thisTest.Verify("r", v1);
            thisTest.Verify("r2", v2);
        }

        [Test]
        [Category("Class")]
        public void TV025_1467140_2()
        {
            String code =
@"class A{	fb : B;	x: int;constructor A(x:int){		this.x = x;        this.fb = B.B(x);}}class B extends A{    b:int;    constructor B(x:int)    {        this.x = x+10;        this.b = x;    }}a = A.A([0,1]);r = a.x;r2 = a.fb.x;";
            thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 0, 1 };
            Object[] v2 = new Object[] { 10, 11 };
            thisTest.Verify("r", v1);
            thisTest.Verify("r2", v2);
        }

        [Test]
        [Category("Class")]
        public void T028_Inheritance_Property()
        {
            String code =
@"class A{	fx: int;	constructor A(x:int)	{		fx = x;	}}class B extends A{	fx : int;	fy : int;	constructor  B(y :int):base.A(x)	{		fy = y;	}}a = A.A(1);b = B.B(2);r1 = a.fx;";
            // Assert.Fail("1467141 - Sprint 24 - Rev 2954: declare a property in subclass with the same name as the property in super class will cause Nunit crash");
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
           {
               thisTest.RunScriptSource(code);
           });
        }

        [Test]
        [Category("Class")]
        public void T029_Inheritance_Property_1()
        {
            String code =
@"class A{	fx: int;	constructor A(x:int)	{		fx = x;	}}class B extends A{		fy : int;	constructor  B(y :int)	{		fy = y;	}}a = A.A(1);b = B.B(2);r1 = a.fx;r2 = b.fx;r3 = b.fy;";

            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 1);
            thisTest.Verify("r2", null);
            thisTest.Verify("r3", 2);
            
        }

        [Test]
        [Category("Class")]
        public void T030_Inheritance_Property_2()
        {
            String code =
@"class A {	fx : int;	constructor A1(x : int)	{		fx = x;	}	constructor A2(x : int)	{		fx = x *2;	}	constructor A()	{		fx = 10;	}}class B extends A{	fy : int;}a1 = A.A1(1);a2 = A.A2(1);b1 = B.B();r = b1.fx;";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 10);
        }


        [Test]
        [Category("Class")]
        public void T031_Inheritance_Property_3()
        {
            String code =
@"class A{	fx :int[];	constructor A(x:int[])	{		fx = x +1;	}}class B extends A{	fy :int[];	constructor B(y: int[],x:int[]): base.A(x)	{		fy = y +2;	}}a = A.A([0,1]);r1 = a.fx;b = B.B([10,11],[0,1]);r2 = b.fx;r3 = b.fy;r4 = a.fy;";
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
@"def RangeExpression : int () { return = 1;}r1 = RangeExpression();";
            thisTest.RunScriptSource(code);

            thisTest.Verify("r1", 1);
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
        [Category("Class")]
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
@"def foo(x:var){	return = bar(x) + 1;}def bar(x:int){    return = x + 1;}a = [0,1];r = foo(a);";
            thisTest.RunScriptSource(code);
            Object r = new Object[]             {                2,                3            }
            ;
            thisTest.Verify("r", r);
        }

        [Test]
        public void TV1467147_PushThroughCastingWithReplication_1()
        {
            String code =
@"def foo(x:var){	return = bar(x) + 1;}def bar(x:int){    return = x + 1;}a = [0,1.0];r = foo(a);";
            thisTest.RunScriptSource(code);
            Object r = new Object[]             {                2,                3            }
            ;
            thisTest.Verify("r", r);
        }

        [Test]
        public void TV1467147_PushThroughCastingWithReplication()
        {
            String code =
@"def A(x:var){    return = bar(x);}def bar(x:bool){    return = x;}a = A([0,1.1,null]);r = a;";
            thisTest.RunScriptSource(code);
            Object r = new Object[]             {                false,                true,                null            }
            ;
            thisTest.Verify("r", r);
        }

        [Test]
        [Category("Class")]
        public void T034_PushThroughCastingWithReplication_UserDefinedType()
        {
            String code =
@"def foo(x:AB){	return = bar(x) + 1;}def bar(x:Ric){    return = x.fx +1 ;}class AB{	fx : var;	constructor AB()	{ 		fx = 1;	}}class Ric extends AB{	constructor Ric() : base.AB()	{	}}ric1 = Ric.Ric();ric2 = Ric.Ric();rics = [ric1, ric2];r = foo(rics);";
            thisTest.RunScriptSource(code);
            Object[] rs = new Object[] { 3, 3 };
            thisTest.Verify("r", rs);
        }

        [Test]
        public void T035_PushThroughIntWithReplication()
        {
            String code =
@"def foo(x:int){	return = bar(x) + 1;}def bar(x:int){    return = x + 1;}a = [0,1];r = foo(a);";
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
@"a = [ 0 ];def foo(x:int){	return = 7;}va = foo(a);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("va", new object[] { 7 });
        }

        [Test]
        public void T036_Replication_ArrayDimensionDispatch_SubTest_1D2()
        {
            String code =
@"a = [ 0, 0 ];def foo(x:int){	return = 7;}va = foo(a);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("va", new object[] { 7, 7 });
        }

        [Test]
        public void T036_Replication_ArrayDimensionDispatch_SubTest_2D()
        {
            String code =
@"a = [ [ 0 ] ];def foo(x:int){	return = 7;}va = foo(a);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("va", new object[] { new object[] { 7 } });
        }

        [Test]
        public void T036_Replication_ArrayDimensionDispatch_SubTest_2D2()
        {
            String code =
@"a = [ [ 0 ], [0] ];def foo(x:int){	return = 7;}va = foo(a);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("va", new object[] { new object[] { 7 }, new object[] { 7 } });
        }

        [Test]
        public void T036_Replication_ArrayDimensionDispatch_SubTest_3D()
        {
            String code =
@"a = [ [ [ 0 ] ] ];def foo(x:int){	return = 7;}va = foo(a);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("va", new object[] { new object[] { new object[] { 7 } } });
        }

        [Test]
        public void T036_Replication_ArrayDimensionDispatch_SubTest_4D()
        {
            String code =
@"a = [ [ [ [ 0 ] ] ] ];def foo(x:int){	return = 7;}va = foo(a);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("va", new object[] { new object[] { new object[] { new object[] { 7 } } } });
        }


        [Test]
        public void T036_Replication_ArrayDimensionDispatch_SubTest()
        {
            String code =
@"c = [[1]];d = [[[1]]];def foo(x:int){	return = 7;}vc = foo(c);vd = foo(d);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("vc", new object[]                                              {                                                  new object[] {7 }                                              });
            thisTest.Verify("vd", new object[]                                              {                                                  new object[]                                                      {                                                          new object[] {7 }                                                      }                                              });
        }

        [Test]
        public void T036_Replication_ArrayDimensionDispatch()
        {
            String code =
@"z = 0;a = [];b = [1];c = [[1]];d = [[[1]]];def foo(x:int){	return = 7;}vz = foo(z);va = foo(a);vb = foo(b);vc = foo(c);vd = foo(d);";
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
@"z = 0;a = [];b = [1];c = [[1]];d = [[[1]]];def foo(x:var){	return = 7;}vz = foo(z);va = foo(a);vb = foo(b);vc = foo(c);vd = foo(d);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("vz", 7);
            thisTest.Verify("va", new object[0]);
            thisTest.Verify("vb", new object[] { 7 });
            thisTest.Verify("vc", new object[]                                              {                                                  new object[] {7 }                                              });
            thisTest.Verify("vd", new object[]                                              {                                                  new object[]                                                      {                                                          new object[] {7 }                                                      }                                              });
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
        [Category("Class")]
        public void T039_Inheritance_Method_1()
        {
            String code =
@"class A{	fx;	constructor A()	{		fx = 2;	}}class B extends A{	fy;	constructor B(): base.A()	{		fx = 10;		fy = 20;	}}b = B.B();r1 = b.fx;r2 = b.fy; ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 10);
            thisTest.Verify("r2", 20);
        }

        [Test]
        public void TV1467063_Function_Overriding()
        {
            // Tracked by: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4054
            string err = "";
            String code =
@"	def foo(x:int)	{        return = 100;	}		def foo(x:var)	{		return = 200;	}    r = foo(100);";
            thisTest.RunScriptSource(code, err);
            thisTest.SetErrorMessage("MAGN-4054: Function overriding: when using function overriding, wrong function is called");
            thisTest.Verify("r", 100);
        }

        [Test]
        public void T045_Inheritance_Method_02()
        {
            String code =
@"	def foo(x:int)	{        return = 100;	}		def foo(x: double)	{		return = 200;	}    r1 = foo(1.5);    r2 = foo(1);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 200);
            thisTest.Verify("r2", 100);
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
        [Category("DSDefinedClass_Ported")]
        public void TV1467175_2()
        {
            String code =
@"def foo(x : double){    return = x ;}def foo(x : var){    return = x ;}r = foo(1);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", 1.0);
        }

        [Test]
        public void TV1467175_3()
        {
            String code =
@"import (""FFITarget.dll"");a = TestOverloadA.TestOverloadA();b = TestOverloadB.TestOverloadB();ra = a.foo(1.1);rb = b.foo(1.1);";
            thisTest.RunScriptSource(code);
            thisTest.Verify("ra", 100);
            thisTest.Verify("rb", 100);
        }


        [Test]
        public void FunctionOverload()
        {
            String code =
@"    def foo()    {        fx = 11;        return = fx;    }        def fooB()    {        fx = 22;        return = fx;    }r4 = foo();r7 = fooB();";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r4", 11);
            thisTest.Verify("r7", 22);
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
@"import(""FFITarget.dll"");p = DummyPoint.ByCoordinates(10, 20, 30);def foo(val : int[]){    return = 1;}def foo(val : var){    return = 2;}                                arr = [ 3, p, 5 ] ;r = foo(arr);    ";
            thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 2, 2, 2 };
            thisTest.Verify("r", v1);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Method Resolution")]
        [Category("Failure")]
        public void T052_Defect_ReplicationMethodOverloading_2()
        {
            String code =
@"
import(""FFITarget.dll"");a1 = DummyPoint.ByCoordinates(10, 20, 30);def foo(val : int[]){    return = 1;}def foo(val : var){    return = 2;}def foo(val: var[]){	return = 3;}                                arr = [ [3], a1, 5,[a1] ] ;//1,2,2,3r = foo(arr);    ";
            // Tracked by: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4052
            thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 1, 2, 2, 3 };
            thisTest.Verify("r", v1);
        }

        [Test]
        [Category("Method Resolution")]
        [Category("Failure")]
        [Category("DSDefinedClass_Ported")]
        public void TV052_Defect_ReplicationMethodOverloading_01()
        {
            String code =
@"
import(""FFITarget.dll"");a1 = DummyPoint.ByCoordinates(10, 20, 30);def foo(val : int[]){    return = 1;}def foo(val : var){    return = 2;}def foo(val: var[]){	return = 3;}                                arr = [ [3], a1, 5,[[a1]] ] ;//1,2,2,nullr = foo(arr);";
            // Tracked by: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4052
            string err = "MAGN-4052 Replication and Method overload issue, resolving to wrong method";
            thisTest.RunScriptSource(code, err);
            Object[] v1 = new Object[] { 1, 2, 2, null };
            thisTest.Verify("r", v1);
        }

        [Test]
        [Category("Method Resolution")]
        [Category("Failure")]
        [Category("DSDefinedClass_Ported")]
        public void TV052_Defect_ReplicationMethodOverloading_03()
        {
            String code =
@"
import(""FFITarget.dll"");a1 = DummyPoint.ByCoordinates(10, 20, 30);def foo(val : int[]){    return = 1;}def foo(val : var){    return = 2;}def foo(val: var[]){	return = 3;}                                arr = [ [3], a1, 5.0,[[a1]] ] ;//1,2,2,nullr = foo(arr);";
            // Tracked by: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4052
            string err = "MAGN-4052 Replication and Method overload issue, resolving to wrong method";
            thisTest.RunScriptSource(code, err);
            Object[] v1 = new Object[] { 1, 2, 2, null };
            thisTest.Verify("r", v1);
        }

        [Test]
        [Category("Method Resolution")]
        [Category("Failure")]
        [Category("DSDefinedClass_Ported")]
        public void TV052_Defect_ReplicationMethodOverloading_InUserDefinedClass()
        {
            String code =
@"import(""FFITarget.dll"");a1 = DummyPoint.ByCoordinates(10, 20, 30);def foo(val : int[]){    return = 1;}def foo(val : var){    return = 2;}def foo(val: var[]){	return = 3;}                                arr = [ [3], a1, 5.0,[[a1]] ] ;//1,2,2,nullr = foo(arr);";
            // Tracked by: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4052
            string err = "MAGN-4052 Replication and Method overload issue, resolving to wrong method";
            thisTest.RunScriptSource(code, err);
            Object[] v1 = new Object[] { 1, 2, 2, null };
            thisTest.Verify("r", v1);
        }


        [Test]
        public void ReplicationSubDispatch()
        {
            String code =
@"def foo (i : int){ return=1;}def foo (d: double){ return=2;}m = [4, 5.0, 56, 6.3];ret = foo(m);";
            thisTest.RunScriptSource(code);

            thisTest.Verify("ret", new object[] { 1, 2, 1, 2 });
        }

        [Test]
        public void Test()
        {
            String code =
@"class A{}class B extends A{}def TestA(b : B){ return = 1; }def TestB(a : A){ return = 2; }a = A.A();b = B.B();r1 = TestA(a);//nullr2 = TestB(b);//2";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T056_nonmatchingclass_1467162()
        {
            String code =
            @"                import(""FFITarget.dll"");                a : M = DummyPoint.ByCoordinates(10, 20, 30);            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", null);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.Runtime.WarningID.ConversionNotPossible);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T057_nonmatchingclass_1467162_2()
        {
            String code =
            @"                                import(""FFITarget.dll"");                a : DummyVector = DummyPoint.ByCoordinates(10, 20, 30);            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", null);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.Runtime.WarningID.ConversionNotPossible);
        }

        [Test]
        public void RepoTests_MAGN3177()
        {
            String code =
                @"def foo(str: string, para : string[], arg : var[]){    Print(str);    Print(para);    Print(arg);}" +   "\n s = \"str\";\n" +   "paras = [\"a\",\"b\"]; \n" +   "args = [[2,5],[5]]; \n"+"foo(s, paras, args);";
            thisTest.RunScriptSource(code);
            //thisTest.Verify("d", 1);
        }


        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestMethodResolution01()
        {
            string code = @"

import(""FFITarget.dll"");

    def foo()
    {
        return = 41;
    }

    def foo(x : DummyPoint)
    {
        return = 42;
    }

    def foo(x : DummyPoint, y: DummyPoint)
    {
        return = 43;
    }

    def foo(x : DummyPoint, y: DummyPoint, z:DummyPoint)
    {
        return = 44;
    }

a = DummyPoint.ByCoordinates(10, 20, 30);
r1 = foo();
r2 = foo(a);
r3 = foo(a,a);
r4 = foo(a,a,a);
";

            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 41);
            thisTest.Verify("r2", 42);
            thisTest.Verify("r3", 43);
            thisTest.Verify("r4", 44);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestMethodResolution02()
        {
            string code = @"

import(""FFITarget.dll"");

def foo(x: int)
{
    return = 41;
}

def foo(x : DummyPoint, y: int)
{
    return = 42;
}
a = DummyPoint.ByCoordinates(10, 20, 30);
r1 = foo(1);
r2 = foo(a, 1);
";

            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 41);
            thisTest.Verify("r2", 42);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void TestMethodResolution03()
        {
            string code = @"
import(""FFITarget.dll"");

def foo(x: int)
{
    return = 41;
}

def foo(x : DummyPoint, y: int)
{
    return = 42;
}
a = DummyPoint.ByCoordinates(10, 20, 30);
r1 = foo([1]);
r2 = foo(a, [1]);
";

            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", new object[] {41});
            thisTest.Verify("r2", new object[] {42});
        }

        [Test]
        public void TestMethodResolutionOnHeterogeneousArray01()
        {
            string code = @"
def foo(x : int)
{
    return = x;
}

r = foo([""foo"", ""bar"", 33.9]);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", new object[] { null, null, 34 });
        }

        [Test]
        public void TestMethodResolutionOnHeterogeneousArray02()
        {
            string code = @"
def foo(x: int, y : string)
{
    return = x;
}

xs = [""foo"", 10, ""bar""];
ys = [ 12, ""ding"", ""dang""];
r = foo(xs, ys);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", new object[] { null, 10, null});
        }

        [Test]
        public void TestMethodResolutionOnHeterogeneousArray03()
        {
            string code = @"
def foo(x: int, y : string)
{
    return = x;
}

xs = [""foo"", 10, ""bar""];
ys = [ 12, ""ding"", ""dang""];
r = foo(xs<1>, ys<1>);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r", new object[] { null, 10, null });
        }

        [Test]
        public void NonStaticPropertyLookupOnClassName_DoesNotCrash()
        {
            var code = @"
import(""FFITarget.dll"");

a = DummyPoint.X;
d = DummyPoint.ByCoordinates(89,0,0);
b = a(d);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("b", 89);
        }
        
        [Test]
        public void StaticMethodPropertyLookupsOnClassName()
        {
            var code = @"
import(""FFITarget.dll"");

a = ClassFunctionality.get_StaticProperty;

c = ClassFunctionality.ClassFunctionality(78);
b = ClassFunctionality.get_Property;
d = b(c);

e = ClassFunctionality.get_StaticMethod();

f = ClassFunctionality.get_StaticMethod;
g = f();

h = ClassFunctionality.get_Method;
i = h(c);

j = ClassFunctionality.get_Method(c);

m = ClassFunctionality.IntVal;
k = m(c);

l = ValueContainer.SomeStaticProperty;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 99);
            thisTest.Verify("d", 78);
            thisTest.Verify("e", 99);
            thisTest.Verify("g", 99);
            thisTest.Verify("i", 78);
            thisTest.Verify("j", 78);
            thisTest.Verify("k", 78);
            thisTest.Verify("l", 123);
        }
    }
}

