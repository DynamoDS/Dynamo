using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
using ProtoTest.TD;
using ProtoTestFx.TD;
namespace ProtoTest.Associative
{
    class MicroFeatureTests : ProtoTestBase
    {
        readonly string testCasePath = Path.GetFullPath(@"..\..\..\Scripts\Associative\MicroFeatureTests\");

        [Test]
        public void TestAssignment01()
        {

            String code =
@"foo;[Associative]{	foo = 5;}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Obj o = mirror.GetValue("foo");
            Assert.IsTrue((Int64)o.Payload == 5);
        }

        [Test]
        public void TestAssignment02()
        {
            String code =
@"boo;moo;scoo;[Associative]{	boo = 5;    moo = 7.2;    scoo = 11;}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Obj o = mirror.GetValue("foo");
            Assert.IsTrue((Int64)(mirror.GetValue("boo")).Payload == 5);
            Assert.IsTrue((Double)(mirror.GetValue("moo")).Payload == 7.2);
            Assert.IsTrue((Int64)(mirror.GetValue("scoo")).Payload == 11);
        }

        [Test]
        public void TestNull01()
        {
            String code =
@"x;y;a;b;c;[Associative]{	x = null;    y = x;    a = null;    b = a + 2;    c = 2 + a * x;}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue(mirror.GetValue("x").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("y").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("a").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("b").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
            Assert.IsTrue(mirror.GetValue("c").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
        }

        [Test]
        public void TestNull02()
        {
            String code =
@"c;[Associative]{    def foo : int ( a : int )    {        b = a + 1;    }	     c = foo(1);	}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue(mirror.GetValue("c").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
        }

        [Test]
        public void TestFunctions01()
        {
            String code =
@"test;test2;test3;[Associative]{    def mult : int( s : int )		{		return = s * 2;	}    test = mult(5);    test2 = mult(2);    test3 = mult(mult(5));}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("test").Payload == 10);
            Assert.IsTrue((Int64)mirror.GetValue("test2").Payload == 4);
            Assert.IsTrue((Int64)mirror.GetValue("test3").Payload == 20);
        }

        [Test]
        public void TestFunctions02()
        {
            String code =
@"        temp;[Associative]{     def test2 : int(b : int)    {        return = b;    }                    def test : int(a : int)    {        return = a + test2(5);    }                   temp = test(2);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("temp").Payload == 7);
        }

        [Test]
        [Category("Failure")]
        public void TestDuplicateFunctionParams()
        {
            const string code = @"
def test : int(a : int, a : int)
{
    return = a + a;
}

temp = test(1, 2);
";
            thisTest.RunScriptSource(code);
            thisTest.VerifyBuildWarningCount(1);
            thisTest.Verify("temp", null);
        }

        [Test]
        public void TestFunctionsOverload01()
        {
            String code =
@"test1;test2;[Associative]{    def m1 : int( s : int )		{		return = s * 2;	}    def m1 : int( s: int, y : int )    {        return = s * y;    }    test1 = m1(5);    test2 = m1(5, 10);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("test1").Payload == 10);
            Assert.IsTrue((Int64)mirror.GetValue("test2").Payload == 50);
        }

        [Test]
        public void TestFunctionsOverload02()
        {
            String code =
@"i;j;[Associative]{    def f : int( p1 : int )    {	    x = p1 * 10;	    return = x;    }    def f : int( p1 : int, p2 : int )    {	    return = p1 + p2;    }    a = 2;    b = 20;    // Pasing variables to function overloads    i = f(a + 10);    j = f(a, b);}   ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("i").Payload == 120);
            Assert.IsTrue((Int64)mirror.GetValue("j").Payload == 22);
        }

        [Test]
        public void TestFunctionsOverload03()
        {
            String code =
            @"class A            {	            b : int;	            	            def foo(a : int)	            {		            b = 1;		            return = a;	            }	            def foo(a : int[])	            {                    b = 2;		            return = a;	            }            }            x = A.A();            c = {1,2,3,4};            d = x.foo(c);            y = x.b;            e = x.foo({5,6,7,8});            z = x.b;                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Obj o = mirror.GetValue("d");
            List<Obj> os = mirror.GetArrayElements(o);
            Assert.IsTrue(os.Count == 4);
            Assert.IsTrue((Int64)os[0].Payload == 1);
            Assert.IsTrue((Int64)os[1].Payload == 2);
            Assert.IsTrue((Int64)os[2].Payload == 3);
            Assert.IsTrue((Int64)os[3].Payload == 4);
            Assert.IsTrue((Int64)mirror.GetValue("y").Payload == 2);
            o = mirror.GetValue("e");
            os = mirror.GetArrayElements(o);
            Assert.IsTrue(os.Count == 4);
            Assert.IsTrue((Int64)os[0].Payload == 5);
            Assert.IsTrue((Int64)os[1].Payload == 6);
            Assert.IsTrue((Int64)os[2].Payload == 7);
            Assert.IsTrue((Int64)os[3].Payload == 8);
            Assert.IsTrue((Int64)mirror.GetValue("z").Payload == 2);
        }

        [Test]
        public void TestDynamicDispatch01()
        {
            String code =
@"    class A    {        x:int;        constructor A(i)        {            x = i;        }    }        def foo(a:A)    {        return = a.x;    }    def foo(x:int)    {        return = x;    }    def ding(x:int)    {        return = (x < 0) ? 2 * x : A.A(x);    }    t1 = foo(ding(-1));    t2 = foo(ding(2));    arr = {1, 2};    arr[1] = A.A(100);    t3 = foo(arr[1]);    ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("t1").Payload == -2);
            Assert.IsTrue((Int64)mirror.GetValue("t2").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("t3").Payload == 100);
        }

        [Test]
        [Category("Failure")]
        [Category("Class")]
        public void TestClasses01()
        {
            String code =
@"	class f	{		fx : var;		fy : var;		constructor f()		{			fx = 123;			fy = 345;		}	}		class g	{		gx : var;		gy : var;		constructor g()		{			// Construct a class within a class			gx = f.f();			gy = 678;		}	}	p = f.f();    a = p.fx;    b = p.fy;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 123);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 345);
        }

        [Test]
        public void TestClasses02()
        {
            String code =
@"	class vector2D	{		mx : var;		my : var;				constructor vector2D(px : int, py : int)		{			mx = px; 			// Copy mx to my with px's value			my = mx; 		}	}	v1 = vector2D.vector2D(100,20);	x = v1.mx;	y = v1.my;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 100);
            Assert.IsTrue((Int64)mirror.GetValue("y").Payload == 100);
        }

        [Test]
        public void TestClasses03()
        {
            String code =
@"    class A    {        x : var;        constructor A()        {            x = 0;        }	    def Get : int()        {            return = 10;        }    }    class B extends A    {        constructor B()        {        }    }    p = B.B();    x = p.Get();";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 10);
        }

        [Test]
        public void TestClasses04()
        {
            String code =
@"    class A    {        x : var;        constructor A()        {            x = 1;        }	    def Get : int()        {            return = 10;        }    }        class B extends A    {        constructor B()        {            x = 2;        }    }    ptrA = A.A();    ax = ptrA.x;    ptrB = B.B();    bx = ptrB.x;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("ax").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("bx").Payload == 2);
        }

        [Test]
        public void TestClasses05()
        {
            String code =
@"      def sum : double (p : double)    {           return = p + 10.0;    }    class Obj    {        val : var;		mx : var;		my : var;		mz : var;        constructor Obj(xx : double, yy : double, zz : double)        {            mx = xx;            my = yy;            mz = zz;            val = sum(zz);        }    }    p = Obj.Obj(0.0, 1.0, 2.0);    x = p.val;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((double)mirror.GetValue("x").Payload == 12);
        }

        [Test]
        public void TestClasses06()
        {
            String code =
@"  class Point{    mx : var;    my : var;    mz : var;    constructor ByCoordinates(x : int, y : int, z : int)    {        mx = x;        my = y;        mz = z;    }}class BSplineCurve{    mpts : var[];    constructor ByPoints(ptsOnCurve : Point[])    {        mpts = ptsOnCurve;    }}pt1 = Point.ByCoordinates(1,2,3);pt2 = Point.ByCoordinates(4,5,6);pt3 = Point.ByCoordinates(7,8,9);pt4 = Point.ByCoordinates(10,11,12);pt5 = Point.ByCoordinates(15,16,17);pts = {pt1, pt2, pt3, pt4, pt5};p = BSplineCurve.ByPoints(pts);a1 = p.mpts[0].mx;a2 = p.mpts[1].my;a3 = p.mpts[2].mz;a4 = p.mpts[3].mx;a5 = p.mpts[4].my;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("a1").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("a2").Payload == 5);
            Assert.IsTrue((Int64)mirror.GetValue("a3").Payload == 9);
            Assert.IsTrue((Int64)mirror.GetValue("a4").Payload == 10);
            Assert.IsTrue((Int64)mirror.GetValue("a5").Payload == 16);
        }

        [Test]
        public void TestClasses07()
        {
            String code =
@"  class vector2D{	mx : var[];	constructor vector2D()	{		mx = {10,20}; 	}    def ModifyMe : int()    {        mx[1] = 64;        return = mx[1];    }}p = vector2D.vector2D();x = p.ModifyMe();";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 64);
        }

        [Test]
        public void TestClasses08()
        {
            String code =
@"  class A{    public x = 1;}class B extends A{    private x = 2;}b = B.B();t = b.x;";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        public void TestClassFunction01()
        {
            String code =
@"	class complex	{		mx : var;		my : var;		constructor complex(px : int, py : int)		{			mx = px; 			my = py; 		}		def scale : int(s : int)		{			mx = mx * s; 			my = my * s; 			return = 0;		}	}	p = complex.complex(8,16);	i = p.mx;	j = p.my;	// Calling a member function of class complex that mutates its properties 	k1 = p.scale(2); 	// Scale 'p' further	k2 = p.scale(10); ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("i", 160);
            thisTest.Verify("j", 320);
        }

        [Test]
        public void TestClassHeirarchy01()
        {
            String code =
@"class A
{
	constructor A()
	{
	}
	
	def foo : int()
	{
		return = 1;
	}
}

class B extends A
{

	constructor B()
	{
	}
}

class C extends B
{
	constructor C()
	{
	}
}

class D
{
	def foo:int(a:A)
	{
		return = a.foo();
	}
}

c = C.C();
d = D.D();
x = d.foo(c);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 1);
        }

        [Test]
        public void TestClassHeirarchy02()
        {
            String code =
@"class A
{
	constructor A()
	{
	}
	
	def foo : int()
	{
		return = 1;
	}
}

class C extends B
{
	constructor C()
	{
	}
}

class B extends A
{

	constructor B()
	{
	}
}

class D
{
	def foo:int(a:A)
	{
		return = a.foo();
	}
}

c = C.C();
d = D.D();
x = d.foo(c);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 1);
        }


        [Test]
        public void TestClassFunction02()
        {
            String code =
@"    class Obj    {        val : var;	    def sum : int (p : int)        {            return = p + 10;        }        constructor Obj()        {            val = sum(2);        }    }    p = Obj.Obj();    x = p.val;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 12);
        }

        [Test]
        public void TestClassFunction03()
        {
            String code =
@"	class Vector	{		mx : var;		my : var;				def init : int ()		{            my = 522;              return = 22;		}				constructor Vector(x : int)		{			mx = x;            aa = init(); // A local function called within the constructor		}	}	a = Vector.Vector(1);	b = a.mx;    c = a.my;    d = a.init();";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 522);
        }

        [Test]
        public void TestClassFunction04()
        {
            String code =
@"    class Sample    {        _val : var;                constructor Sample()        {            _val = 5.0;        }                constructor Sample(val : double)        {            _val = val;        }                def get_Val : double ()        {            return = _val;        }    }        def function1 : double (s : Sample )    {        return = s.get_Val();    }        s1 = Sample.Sample();    s2 = Sample.Sample(100.0);        one = function1(s1);    two = function1(s2);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((double)mirror.GetValue("one").Payload == 5);
            Assert.IsTrue((double)mirror.GetValue("two").Payload == 100);
        }

        [Test]
        public void TestClassFunction05()
        {
            String code =
@"class Point{    _x : var;    _y : var;    _z : var;                                    constructor Point(xx : double, yy : double, zz : double)    {        _x = xx;        _y = yy;        _z = zz;    }                                    def get_X : double ()     {        return = _x;    }    def get_Y : double ()     {        return = _y;    }    def get_Z : double ()     {        return = _z;    }}                    class Line                 {    _sp : var;    _ep : var;                        constructor Line(startPoint : Point, endPoint : Point)    {        _sp = startPoint;         _ep = endPoint;                        }    def get_StartPoint : Point ()    {                                      return = _sp;    }                                                    def get_EndPoint : Point ()     {        return = _ep;    }          }                pt1 = Point.Point(3.0,2.0,1.0);pt2 = Point.Point(31.0,21.0,11.0);  myline = Line.Line(pt1, pt2);v1 = myline._sp.get_X();v2 = myline._sp._x;v3 = myline.get_StartPoint().get_X();v4 = myline.get_StartPoint().get_Y();v5 = myline.get_StartPoint().get_Z();v6 = myline.get_EndPoint().get_X();v7 = myline.get_EndPoint().get_Y();v8 = myline.get_EndPoint().get_Z();";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((double)mirror.GetValue("v1").Payload == 3);
            Assert.IsTrue((double)mirror.GetValue("v2").Payload == 3);
            Assert.IsTrue((double)mirror.GetValue("v3").Payload == 3);
            Assert.IsTrue((double)mirror.GetValue("v4").Payload == 2);
            Assert.IsTrue((double)mirror.GetValue("v5").Payload == 1);
            Assert.IsTrue((double)mirror.GetValue("v6").Payload == 31);
            Assert.IsTrue((double)mirror.GetValue("v7").Payload == 21);
            Assert.IsTrue((double)mirror.GetValue("v8").Payload == 11);
        }

        [Test]
        public void TestClassFunction06()
        {
            String code =
@"class Point {    mx : var;    my : var;    mz : var;    constructor ByCoordinates(xx : double, yy : double, zz : double)    {    mx = xx;    my = yy;    mz = zz;    }}    class Circle{    _cp : var;        constructor ByCenterPointRadius(centerPt : Point, rad : double)    {        _cp = centerPt;    }        def get_CenterPoint : Point ()    {                return = _cp;    }        }e;f;g;[Associative]{    pt = Point.ByCoordinates(10.0,20.0,30.0);    rad = 25.0;        c = Circle.ByCenterPointRadius(pt, rad);        d = c.get_CenterPoint();    e = d.mx;    f = d.my;    g = d.mz;}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((double)mirror.GetValue("e", 0).Payload == 10);
            Assert.IsTrue((double)mirror.GetValue("f", 0).Payload == 20);
            Assert.IsTrue((double)mirror.GetValue("g", 0).Payload == 30);
        }

        [Test]
        public void TestClassFunction07()
        {
            String code =
@"class MyPoint{	X : double;	Y : double;	Z : double;                                    constructor MyPoint (x : double, y : double, z : double)    {		X = x;		Y = y;		Z = z;    }					def Get_X : double()	{		return = X;	}			def Get_Y : double()	{		return = Y;	}	def Get_Z : double()	{		return = Z;	}}	def GetPointValue : double (pt : MyPoint){	return = pt.Get_X() + pt.Get_Y()+ pt.Get_Z(); }	p = MyPoint.MyPoint (10.0, 20.0, 30.0);val = GetPointValue(p);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((double)mirror.GetValue("val").Payload == 60);
        }

        [Test]
        public void TestClassFunction08()
        {
            String code =
@"class Point{    _x : var;    _y : var;    _z : var;                                    constructor Point(xx : double, yy : double, zz : double)    {        _x = xx;        _y = yy;        _z = zz;    }                                    def get_X : double ()     {        return = _x;    }    def get_Y : double ()     {        return = _y;    }    def get_Z : double ()     {        return = _z;    }}                    class Line      {    _sp : var;    _ep : var;                        constructor Line(startPoint : Point, endPoint : Point)    {        _sp = startPoint;         _ep = endPoint;                        }    def get_StartPoint : Point ()    {                                      return = _sp;    }                                                    def get_EndPoint : Point ()     {        return = _ep;    }                                             }                pt1 = Point.Point(3.0,2.0,1.0);pt2 = Point.Point(30.1, 20.1, 10.1);l = Line.Line(pt1, pt2);                l_sp = l.get_StartPoint();l_ep = l.get_EndPoint();      l_sp_x = l_sp.get_X();l_ep_x = l_ep.get_X();      l_sp_y = l_sp.get_Y();l_ep_y = l_ep.get_Y();l_sp_z = l_sp.get_Z();l_ep_z = l_ep.get_Z();";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((double)mirror.GetValue("l_sp_x").Payload == 3);
            Assert.IsTrue((double)mirror.GetValue("l_ep_x").Payload == 30.1);
            Assert.IsTrue((double)mirror.GetValue("l_sp_y").Payload == 2);
            Assert.IsTrue((double)mirror.GetValue("l_ep_y").Payload == 20.1);
            Assert.IsTrue((double)mirror.GetValue("l_sp_z").Payload == 1);
            Assert.IsTrue((double)mirror.GetValue("l_ep_z").Payload == 10.1);
        }

        [Test]
        public void TestClassFunction09()
        {
            String code =
@"class MyPoint{	mX : double;	mY : double;	mZ : double;                                constructor ByXY (x : double, y : double)    {		mX = x;		mY = y;		mZ = 0.0;    }			constructor ByYZ (y : double, z : double)    {		mX = 0.0;		mY = y;		mZ = z;    }}    p = MyPoint.ByYZ (100.0,200.0);	x = p.mX;	y = p.mY;	z = p.mZ;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((double)mirror.GetValue("x").Payload == 0);
            Assert.IsTrue((double)mirror.GetValue("y").Payload == 100);
            Assert.IsTrue((double)mirror.GetValue("z").Payload == 200);
        }

        [Test]
        public void TestClassFunction10()
        {
            String code =
@"class A  {       x : int;    constructor Create(p : B)    {       x = p.a;     }}    class B{    a : int;    constructor Create(p : A)    {        a = p.x;    }}    aa = 2;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
        }

        [Test]
        public void TestClassFunction11()
        {
            String code =
@"class Point{    context : var;    x : var;    constructor Create(cs : CoordinateSystem, xx : double)    {        context= cs;        x = xx;    }}class CoordinateSystem{    origin : var;    constructor Create(orig : Point)    {        origin = orig;    }}cs = null;p = Point.Create(cs, 10.0);cs2 = CoordinateSystem.Create(p);xval = cs2.origin.x;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((double)mirror.GetValue("xval").Payload == 10);
        }

        [Test]
        public void TestClassFunction12()
        {
            String code =
@"class Tuple4{    X : var;    Y : var;    Z : var;    H : var;        constructor XYZH(xValue : double, yValue : double, zValue : double, hValue : double)    {        X = xValue;        Y = yValue;        Z = zValue;        H = hValue;            }}class Transform{    public C0 : var;     public C1 : var;     public C2 : var;     public C3 : var;             public constructor ByTuples(t0 : Tuple4, t1 : Tuple4, t2 : Tuple4, t3 : Tuple4)    {        C0 = t0;        C1 = t1;        C2 = t2;        C3 = t3;    }            public def ApplyTransform : Tuple4 (t : Tuple4)    {        return = Tuple4.XYZH(0.0, 0.0, 0.0, 0.0);    }            public def NativeMultiply : Transform(other : Transform)    {                      tc0 = ApplyTransform(other.C0); // Test member functions having same local var names        tc1 = ApplyTransform(other.C1);        tc2 = ApplyTransform(other.C2);        tc3 = ApplyTransform(other.C3);        return = Transform.ByTuples(tc0, tc1, tc2, tc3);    }        public def NativePreMultiply : Transform (other : Transform)    {             tc0 = other.ApplyTransform(C0); // Test member functions having same local var names        tc1 = other.ApplyTransform(C1);        tc2 = other.ApplyTransform(C2);        tc3 = other.ApplyTransform(C3);        return = Transform.ByTuples(tc0, tc1, tc2, tc3);    }}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
        }

        [Test]
        public void TestClassFunction13()
        {
            String code =
@"class Tuple4{    X : var;    Y : var;    Z : var;    H : var;        constructor XYZH(xValue : double, yValue : double, zValue : double, hValue : double)    {        X = xValue;        Y = yValue;        Z = zValue;        H = hValue;            }    constructor ByCoordinates4(coordinates : double[] )    {        X = coordinates[0];        Y = coordinates[1];        Z = coordinates[2];        H = coordinates[3];        }        public def Multiply : double (other : Tuple4)    {        //return = (X * other.X) + (Y * other.Y) + (Z * other.Z) + (H * other.H);        return = 100.1;    }}class Vector{    X : var;    Y : var;    Z : var;        public constructor ByCoordinates(xx : double, yy : double, zz : double)    {        X = xx;        Y = yy;        Z = zz;    }}class Transform{    public C0 : Tuple4;     public C1 : Tuple4;     public C2 : Tuple4;     public C3 : Tuple4;             public constructor ByData(data : double[][])    {        C0 = Tuple4.ByCoordinates4(data[0]);        C1 = Tuple4.ByCoordinates4(data[1]);        C2 = Tuple4.ByCoordinates4(data[2]);        C3 = Tuple4.ByCoordinates4(data[3]);    }        public def ApplyTransform : Tuple4 (t : Tuple4)    {        tx = Tuple4.XYZH(C0.X, C1.X, C2.X, C3.X);        return = t;    }        public def TransformVector : Vector (p: Vector)    {            tpa = Tuple4.XYZH(p.X, p.Y, p.Z, 0.0);        tpcv = ApplyTransform(tpa);        return = Vector.ByCoordinates(tpcv.X, tpcv.Y, tpcv.Z);        }}data = {                {1.0, 0.0, 0.0, 0.0},            {0.0, 1.0, 0.0, 0.0},            {0.0, 0.0, 1.0, 0.0},            {0.0, 0.0, 0.0, 1.0}        };        xform = Transform.ByData(data);vec111 = Vector.ByCoordinates(1.0,1.0,1.0);tempTuple = Tuple4.XYZH(vec111.X, vec111.Y, vec111.Z, 0.0);tempcv = xform.ApplyTransform(tempTuple);x = tempcv.X;y = tempcv.Y;z = tempcv.Z;h = tempcv.H;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((double)mirror.GetValue("x").Payload == 1);
            Assert.IsTrue((double)mirror.GetValue("y").Payload == 1);
            Assert.IsTrue((double)mirror.GetValue("z").Payload == 1);
            Assert.IsTrue((double)mirror.GetValue("h").Payload == 0);
        }

        [Test]
        [Category("JunToFix")]
        public void TestClassFunction14()
        {
            String code =
@"class TestClass        {            X: var;    Y: var;        constructor CreateByXY (x : double, y : double)        {                X = x;        Y = y;                }            def AddByOne : TestClass ()        {                tempX = X;        tempY = Y;                    temp = TestClass.CreateByXY(tempX + 1, tempY + 1);        return = temp;        }               }            myInstance = TestClass.CreateByXY(10.0, 10.0);    myNewInstance = myInstance.AddByOne();    x = myNewInstance.X;    y = myNewInstance.Y;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((double)mirror.GetValue("x").Payload == 11);
            Assert.IsTrue((double)mirror.GetValue("y").Payload == 11);
        }

        [Test]
        public void TestClassFunction15()
        {
            String code =
@"class Point{    x : var;    y : var;    z : var;        constructor Create(xx : double, yy : double, zz : double)    {        x = xx;        y = yy;        z = zz;    }        def Offset : Point (delx : double, dely : double, delz : double)    {        return = Point.Create(x + delx, y + dely, z + delz);    }        def OffsetByArray : Point( deltas : double[] )    {        return = Offset(deltas[0], deltas[1], deltas[2]);    }}x;y;z;[Associative]{    pt = Point.Create(10,10,10);    a = {10.0,20.0,30.0};    pt2 = pt.OffsetByArray(a);    x = pt2.x;    y = pt2.y;    z = pt2.z;}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((double)mirror.GetValue("x", 0).Payload == 20);
            Assert.IsTrue((double)mirror.GetValue("y", 0).Payload == 30);
            Assert.IsTrue((double)mirror.GetValue("z", 0).Payload == 40);
        }

        [Test]
        public void TestClassFunction16()
        {
            String code =
@"class CoordinateSystem{}class Vector{    public GlobalCoordinates : var;    public X : var;    public Y : var;    public Z : var;    public Length : var;    public Normalized : var;    public ParentCoordinateSystem : var;    public XLocal : var;    public YLocal : var;    public ZLocal : var;    public constructor ByCoordinates(x : double, y : double, z : double)    {        X = x;        Y = y;        Z = z;    }        public constructor ByCoordinates(cs: CoordinateSystem, xLocal : double, yLocal : double, zLocal : double )    {        ParentCoordinateSystem = cs;        XLocal = xLocal;        YLocal = yLocal;        ZLocal = zLocal;    }    public constructor ByCoordinateArray(coordinates : double[])    {        X = coordinates[0];        Y = coordinates[1];        Z = coordinates[2];        }    public def Cross : Vector (otherVector : Vector)    {        return = Vector.ByCoordinates(            Y*otherVector.Z - Z*otherVector.Y,            Z*otherVector.X - X*otherVector.Z,            X*otherVector.Y - Y*otherVector.X);    }}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
        }

        [Test]
        public void TestClassFunction17()
        {
            String code =
@"class A{    constructor A() {}    def foo() { return = 1; }    def foo(i:int) { return = 10; }}class B{    constructor B() {}    def foo() { return = 2; }    def foo(i:int) { return = 20; }}p = B.B();a = p.foo();b = p.foo(1);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("a", 0).Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("b", 0).Payload == 20);
        }

        [Test]
        public void TestStaticUpdate01()
        {
            string code = @"class Base{    static x : int = 1;}t = Base.x;Base.x = 10;                 ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("t", 0).Payload == 10);
        }

        [Test]
        public void TestStaticUpdate02()
        {
            string code = @"class Base{    static x : int[];}t = Base.x;Base.x = { 1, 2 };                   ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t", new object[] { 1, 2 });
        }

        [Test]
        public void TestStaticProperty01()
        {
            string code = @"class A{    static x:int;    static def foo(i)    {        return = 2 * i;    }}a = A.A();a.x = 3;t1 = a.x;b = A.A();t2 = b.x;                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("t1", 0).Payload == 3);
            Assert.IsTrue((Int64)mirror.GetValue("t2", 0).Payload == 3);
        }

        [Test]
        public void TestStaticProperty02()
        {
            string code = @"class S{	public static a : int;}class C{    public x : int;    constructor C()    {        S.a = 2;    }}p = C.C();b = S.a;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("b", 0).Payload == 2);
        }


        [Test]
        public void TestStaticMethodResolution()
        {
            string code = @"            class A            {	            b : int;	            static z : int;	            def foo(a : int)	            {		            b = 1;		            return = a;	            }	            static def foo(a : int[])	            {		            z = 2;		            return = 9;	            }            }            x = A.A();            c = {1,2,3,4};            d = A.foo(c);            y = x.b;            v = x.z;            w = A.z;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Obj o = mirror.GetValue("c");
            List<Obj> os = mirror.GetArrayElements(o);
            Assert.IsTrue(os.Count == 4);
            Assert.IsTrue((Int64)os[0].Payload == 1);
            Assert.IsTrue((Int64)os[1].Payload == 2);
            Assert.IsTrue((Int64)os[2].Payload == 3);
            Assert.IsTrue((Int64)os[3].Payload == 4);
            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 9);
            Assert.IsTrue((Int64)mirror.GetValue("y").Payload == 0);
            Assert.IsTrue((Int64)mirror.GetValue("v").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("w").Payload == 2);
        }

        [Test]
        public void TestClassNegative01()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                thisTest.RunScriptSource(
    @"class TestClass{    X: var;    constructor Create(x : double)    {        X = x;        return = x + y;    }}p = TestClass.Create(10.0);"
                );
            });
        }

        [Test]
        public void TestTemporaryArrayIndexing01()
        {
            string code = @"t = {1,2,3,4}[3]; ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("t", 4);
        }

        [Test]
        public void TestTemporaryArrayIndexing02()
        {
            string code = @"    t = {{1,2}, {3,4}}[1][1];";
            thisTest.RunScriptSource(code);
            thisTest.Verify("t", 4);
        }

        [Test]
        public void TestTemporaryArrayIndexing03()
        {
            string code = @"    t = ({{1,2}, {3,4}})[1][1];";
            thisTest.RunScriptSource(code);
            thisTest.Verify("t", 4);
        }

        [Test]
        public void TestTemporaryArrayIndexing04()
        {
            string code = @"    t = ({{1,2}, {3,4}}[1])[1];";
            thisTest.RunScriptSource(code);
            thisTest.Verify("t", 4);
        }

        [Test]
        public void TestTemporaryArrayIndexing05()
        {
            string code = @"    t = {1,2,3,4,5}[1..3];";
            thisTest.RunScriptSource(code);
            thisTest.Verify("t", new object[] { 2, 3, 4 });
        }

        [Test]
        public void TestTemporaryArrayIndexing06()
        {
            string code = @"    t = (1..5)[1..3];";
            thisTest.RunScriptSource(code);
            thisTest.Verify("t", new object[] { 2, 3, 4 });
        }

        [Test]
        public void TestTemporaryArrayIndexing07()
        {
            string code = @"    t = ({1,2,3} + {4, 5, 6})[1];";
            thisTest.RunScriptSource(code);
            thisTest.Verify("t", 7);
        }

        [Test]
        public void TestArray001()
        {
            String code =
@"x;y;a;[Associative]{	a = {1001,1002};    x = a[0];    y = a[1];    a[0] = 23;}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 23);
            Assert.IsTrue((Int64)mirror.GetValue("y").Payload == 1002);
            Obj o = mirror.GetValue("a");
            ProtoCore.DSASM.Mirror.DsasmArray arr = (ProtoCore.DSASM.Mirror.DsasmArray)o.Payload;
            Assert.IsTrue((Int64)arr.members[0].Payload == 23);
            Assert.IsTrue((Int64)arr.members[1].Payload == 1002);
        }

        [Test]
        public void TestArray002()
        {
            String code =
@"b;[Associative]{     def foo : int (a : int[])    {                   return = a[0];    }                arr = {100, 200};                b = foo(arr);}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 100);
        }

        [Test]
        public void TestArray003()
        {
            String code =
@"a = {0,1,2};t = {10,11,12};a[0] = t[0];t[1] = a[1];";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Obj o = mirror.GetValue("a");
            ProtoCore.DSASM.Mirror.DsasmArray arr = (ProtoCore.DSASM.Mirror.DsasmArray)o.Payload;
            Assert.IsTrue((Int64)arr.members[0].Payload == 10);
            Assert.IsTrue((Int64)arr.members[1].Payload == 1);
            Assert.IsTrue((Int64)arr.members[2].Payload == 2);
        }

        [Test]
        public void TestIndexingIntoArray01()
        {
            String code =
@"class A{    fx :var;    constructor A(x : var)    {        fx = x;    }}fa = A.A(10..12);r1 = fa.fx;r2 = fa[0].fx; // 10r3 = fa.fx[0]; // 10";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r2", 10);
            thisTest.Verify("r3", 10);
        }

        [Test]
        public void TestIndexingIntoArray02()
        {
            String code =
@"x=[Imperative]{    def ding()    {        return = {{1,2,3}, {4,5,6}};    }    return = ding()[1][1];}class A{    a:int;    constructor A(i:int)    {        a = i;    }}def foo(){    return = {A.A(1), A.A(2), A.A(3)};}y = foo()[1].a;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 5);
            thisTest.Verify("y", 2);
        }

        [Test]
        public void TestArrayOverIndexing01()
        {
            string code = @"[Imperative]{    arr1 = {true, false};    arr2 = {1, 2, 3};    arr3 = {false, true};    t = arr2[1][0];}";
            thisTest.RunScriptSource(code);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.Runtime.WarningID.kOverIndexing);
        }

        [Test]
        public void TestDynamicArray001()
        {
            String code =
@"a = {10,20};a[2] = 100;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Obj o = mirror.GetValue("a");
            ProtoCore.DSASM.Mirror.DsasmArray arr = (ProtoCore.DSASM.Mirror.DsasmArray)o.Payload;
            Assert.IsTrue((Int64)arr.members[0].Payload == 10);
            Assert.IsTrue((Int64)arr.members[1].Payload == 20);
            Assert.IsTrue((Int64)arr.members[2].Payload == 100);
        }

        [Test]
        public void TestDynamicArray002()
        {
            String code =
@"t = {};t[0] = 100;t[1] = 200;t[2] = 300;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Obj o = mirror.GetValue("t");
            ProtoCore.DSASM.Mirror.DsasmArray arr = (ProtoCore.DSASM.Mirror.DsasmArray)o.Payload;
            Assert.IsTrue((Int64)arr.members[0].Payload == 100);
            Assert.IsTrue((Int64)arr.members[1].Payload == 200);
            Assert.IsTrue((Int64)arr.members[2].Payload == 300);
        }

        [Test]
        public void TestDynamicArray003()
        {
            String code =
@"t = {};t[0][0] = 1;t[0][1] = 2;a = t[0][0];b = t[0][1];";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 2);
        }

        [Test]
        public void TestDynamicArray004()
        {
            String code =
@"t = {};t[0][0] = 1;t[0][1] = 2;t[1][0] = 10;t[1][1] = 20;a = t[1][0];b = t[1][1];";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 10);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 20);
        }

        [Test]
        public void TestDynamicArray005()
        {
            String code =
@"t = {0,{20,30}};t[1][1] = {40,50};a = t[1][1][0];";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 40);
        }

        [Test]
        public void TestDynamicArray006()
        {
            String code =
@"a;b;c;d;[Imperative]{    t = {};    t[0][0] = 1;    t[0][1] = 2;    t[1][0] = 3;    t[1][1] = 4;    a = t[0][0];    b = t[0][1];    c = t[1][0];    d = t[1][1];}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 3);
            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 4);
        }

        [Test]
        public void TestDynamicArray007()
        {
            String code = @"a[3] = 3;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { null, null, null, 3 });
        }

        [Test]
        public void TestDynamicArray008()
        {
            String code = @"a[0] = false;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { false });
        }

        [Test]
        public void TestDynamicArray009()
        {
            String code = @"a = false;a[3] = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { false, null, null, 1 });
        }

        [Test]
        public void TestDynamicArray010()
        {
            String code = @"a = false;a[1][1] = {3};";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { false, new object[] { null, new object[] { 3 } } });
        }

        [Test]
        public void TestDynamicArray011()
        {
            String code = @"a[0] = 1;a[0][1] = 2;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { new object[] { 1, 2 } });
        }

        [Test]
        public void TestDynamicArray012()
        {
            String code = @"a = 1;a[-1] = 2;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { 2 });
        }

        [Test]
        public void TestDynamicArray013()
        {
            String code = @"a = 1;a[-3] = 2;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { 2, null, 1 });
        }

        [Test]
        public void TestDynamicArray014()
        {
            String code = @"a = {1, 2};a[3] = 3;a[-5] = 100;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { 100, 1, 2, null, 3 });
        }

        [Test]
        public void TestDynamicArray015()
        {
            String code = @"a = 1;a[-2][-1] = 3;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { new object[] { 3 }, 1 });
        }

        [Test]
        public void TestDictionary01()
        {
            // Using string as a key
            String code = @"
a = {1, 2, 3};
a[""x""] = 42;
r = a [""x""];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 42);
        }

        [Test]
        public void TestDictionary02()
        {
            // Double value can't be used as a key
            String code = @"
a = {1, 2, 3};
a[1.234] = 42;
r = a [1.3];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 42);
        }

        [Test]
        public void TestDictionary03()
        {
            // Using boolean value as a key
            String code = @"
a = {};
a[true] = 42;
a[false] = 41;
r1 = a [1 == 1];
r2 = a [0 == 1];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 42);
            thisTest.Verify("r2", 41);
        }

        [Test]
        public void TestDictionary04()
        {
            // Using character value as a key
            String code = @"
a = {};
a['x'] = 42;
r1 = a['x'];
r2 = a[""x""];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 42);
            thisTest.Verify("r2", null);
        }

        [Test]
        public void TestDictionary05()
        {
            // Using class instance as a key 
            String code = @"
class A
{
}
a = A();
arr = {};
arr[a] = 41;
arr[a] = 42;
r = arr[a];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 42);
        }

        [Test]
        public void TestDictionary06()
        {
            // Test replication on array indexing on LHS
            // using character as a key
            String code = @"
strs = {""x"", true, 'b'};
arr = {};
arr[strs] = {11, 13, 17};
r1 = arr[""x""];
r2 = arr[true];
r3 = arr['b'];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 11);
            thisTest.Verify("r2", 13);
            thisTest.Verify("r3", 17);
        }

        [Test]
        public void TestDictionary07()
        {
            // Test replication on array indexing on RHS
            String code = @"
strs = {""x"", true, 'b'};
arr = {};
arr[strs] = {11, 13, 17};
values = arr[strs];
r1 = values[0];
r2 = values[1];
r3 = values[2];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r1", 11);
            thisTest.Verify("r2", 13);
            thisTest.Verify("r3", 17);
        }

        [Test]
        public void TestDictionary08()
        {
            // Test for 2D array
            String code = @"
arr = {{1, 2}, {3, 4}};
arr[1][""xyz""] = 42;
arr[1][true] = 42;
r1 = arr[1][""xyzxyzxyz""];
r2 = arr[1][true];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r1", null);
            thisTest.Verify("r2", 42);
        }

        [Test]
        public void TestDictionary09()
        {
            // Copy array should also copy key-value pairs
            String code = @"
a = {};
a[""xyz""] = 42;
b = a;
r = b[""xyz""];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 42);
        }

        [Test]
        public void TestDictionary10()
        {
            // Key-value shouldn't be disposed after scope
            String code = @"
a = [Imperative]
{
    b = {};
    b[""xyz""] = 42;
    return = b;
}

r = a[""xyz""];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 42);
        }

        [Test]
        public void TestDictionary11()
        {
            // Copy array should also copy key-value pairs
            String code = @"
a = {};
a[true] = 42;
b = a;
r = b[true];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 42);
        }

        [Test]
        public void TestDictionary14()
        {
            // Copy array should also copy key-value pairs
            String code = @"
a = {};
a[true] = 42;
def foo(x: var[]..[])
{
    return = x[true];
}
r = foo(a);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 42);
        }

        [Test]
        public void TestDictionary12()
        {
            // Type conversion applied to values as well
            String code = @"
a:int[] = {1.1, 2.2, 3.3};
a[true] = 42.4;
r = a[true];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 42);
        }

        [Test]
        public void TestDictionary13()
        {
            // Type conversion applied to values as well
            String code = @"
a = {1.1, 2.2, 3.3};
a[true] = 42.4;
b:int[] = a;
r = b[true];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 42);
        }

        [Test]
        public void TestDictionary15()
        {
            // Test for-loop to get values
            String code = @"
a = {1, 2, 3};
a[true] = 42;
r = [Imperative]
{
    x = null;
    for (v in a)
    {
        x = v;
    }
    return = x;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 42);
        }

        [Test]
        public void TestDictionary16()
        {
            // Test replication for function call
            String code = @"
a = {1, 2, 3};
a[true] = 42;

def foo(x) { return = x; }
r1 = foo(a);
r2 = r1[3];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r2", 42);
        }

        [Test]
        public void TestDictionary17()
        {
            // Test replication for function call
            String code = @"
a = {1, 2, 3};
a[true] = 21;
b = {1, 2, 3};
b[false] = 21;

def foo(x, y) { return = x + y; }
sum = foo(a, b);
r = sum[3];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 42);
        }

        [Test]
        public void TestDictionary18()
        {
            // Test replication for array indexing
            String code = @"
a = {1, 2, 3};
a[true] = 42;
b = {};
b[a] = 1;
b[42] = 42;
c = b[a];
r = c[3];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 42);
        }

        [Test]
        public void TestDictionary19()
        {
            // Test builtin functions GetKeys() for array
            String code = @"
        a = {1, 2, 3};
a[true] = 41;
a[""x""] = ""foo"";
r = Count(GetKeys(a));
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 5);
        }

        [Test]
        public void TestDictionary20()
        {
            // Test builtin functions GetValues() for array
            String code = @"
        a = {1, 2, 3};
a[true] = 41;
a[""x""] = ""foo"";
r = Count(GetValues(a));
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 5);
        }

        [Test]
        public void TestDictionary21()
        {
            // Test builtin functions ContainsKey() for array
            String code = @"
        a = {1, 2, 3};
a[true] = 41;
a[""x""] = ""foo"";
r1 = ContainsKey(a, ""x"");
r2 = ContainsKey(a, true);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r1", true);
            thisTest.Verify("r2", true);
        }

        [Test]
        [Category("Failure")]
        public void TestDictionary22()
        {
            // Test builtin functions RemoveKey() for array
            String code = @"
        a = {1, 2, 3};
a[true] = 41;
a[""x""] = ""foo"";
r1 = RemoveKey(a, ""x"");
r2 = RemoveKey(a, true);
r3 = ContainsKey(a, ""x"");
r4 = ContainsKey(a, true);
";
            // Tracked in:http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4155
            string errmsg = "MAGN-4155 : ContainsKey returns wrong value";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, errmsg);
            thisTest.Verify("r1", true);
            thisTest.Verify("r2", true);
            thisTest.Verify("r3", false);
            thisTest.Verify("r4", true);
        }

        [Test]
        public void TestDictionary23()
        {
            // Test for-loop
            String code = @"
r = [Imperative]
{
    a = {1, 5, 7};
    a[""x""] = 9;
    a[true] = 11;
    x = 0; 
    for (v in a) 
    {
        x = x + v;
    }
    return = x;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 33);
        }

        [Test]
        public void TestDictionary24()
        {
            // Test for-loop
            String code = @"
r = [Imperative]
{
    a = {};
    x = 0;
    for (v in a) 
    {
        x = x + v;
    }
    return = x;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 0);
        }

        [Test]
        public void TestDictionaryRegressMAGN337()
        {
            string code = @"
     a = { 1, 2, 3 };
            b = {""x"",""y""};
                
def foo(a1 : var[], b1 : var[])
            {

                a1[b1] = true;
                return =a1;
            }
z1 = foo(a, b);
r1=z1[""x""];
r2=z1[""y""];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r1", true);
            thisTest.Verify("r2", true);
        }


        [Test]
        public void TestDictionaryRegressMAGN619()
        {
            string code = @"
a[null]=5;
c=Count(a);
r = a[null];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", 0);
            thisTest.Verify("r", 5);
        }

        [Test]
        public void TestDictionary25()
        {
            string code = @"
a = {};
x = ""key"";
a[x] = 42;

y = ""key"";
a[y] = 24;

z = ""key"";
a[z] = 12;

r1 = a[x];
r2 = a[y];
r3 = a[z];
r4 = a[""key""];

a[""key""] = 1;
r5 = a[""key""];
r6 = a[x];
r7 = a[y];
r8 = a[z];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r5", 1);
            thisTest.Verify("r6", 1);
            thisTest.Verify("r7", 1);
            thisTest.Verify("r8", 1);
        }

        [Test]
        public void TestArrayCopyAssignment01()
        {
            String code = @"a = {1, 2, 3};b[1] = a;b[1][1] = 100;z = a[1];";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("z", 2);
        }

        [Test]
        public void TestArrayCopyAssignment02()
        {
            String code = @"a = {1, 2, 3};b = {1, 2, 3};b[1] = a;b[1][1] = 100;z = a[1];";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("z", 2);
        }

        [Test]
        public void TestDynamicArray016()
        {
            String code = @"a = {{1, 2}, {3, 4}};a[-3][-1] = 5;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { new object[] { 5 }, new object[] { 1, 2 }, new object[] { 3, 4 } });
        }

        [Test]
        public void TestArrayIndexReplication01()
        {
            string code = @"a = 1;a[1..2] = 2;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { 1, 2, 2 });
        }

        [Test]
        public void TestArrayIndexReplication02()
        {
            string code = @"a = {1, 2, 3};b = a[1..2];";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", new object[] { 2, 3 });
        }

        [Test]
        public void TestDynamicArrayNegative01()
        {
            string code = @"x = (null)[0];";
            thisTest.RunScriptSource(code, "");
            thisTest.Verify("x", null);
        }

        [Test]
        public void TestDynamicArrayNegative02()
        {
            string code = @"x;[Imperative]{x = (null)[0];}";
            thisTest.RunScriptSource(code, "");
            thisTest.Verify("x", null);
        }

        [Test]
        public void TestDynamicArrayNegative03()
        {
            // Test calling a function on an empty array
            string code = @"p = {};x = p.f();";
            thisTest.RunScriptSource(code, "");
            thisTest.Verify("x", null);
        }


        [Test]
        public void TestReplicationGuidesOnFunctions01()
        {
            string code = @"def f(){    return = { 1, 2 };}def g(){    return = { 3, 4 };}x = f()<1> + g()<2>;a = x[0];b = x[1];";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { 4, 5 });
            thisTest.Verify("b", new object[] { 5, 6 });
        }



        [Test]
        public void TestReplicationGuideWithLongestProperty01()
        {
            String code =
@"
def f(i:int, j:int)
{
    return = i + j;
}

a = 1..3;
b = 2..3;
c = f(a<1L>,b<2>);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", new Object[] { new object[] { 3, 4 }, new object[] { 4, 5 }, new object[] { 5, 6 } });
        }

        [Test]
        public void TestReplicationGuidesOnDotOps01()
        {
            string code = @"class A{    x : var[];    constructor A()    {        x = {1,2};    }}a = A.A();b = A.A();c = a.x<1> + b.x<2>;x = c[0];y = c[1];";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", new object[] { 2, 3 });
            thisTest.Verify("y", new object[] { 3, 4 });
        }

        [Test]
        public void TestReplicationGuidesOnDotOps02()
        {
            string code = @"class C{    def f(a:int,b:int)    {        return = 1;    }}p = C.C();x = {1,2};y = {3,4};a = p.f(x<1>, y<2>);b = a[0];";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", new object[] { 1, 1 });
        }

        [Test]
        public void TestReplicationGuidesOnDotOps03()
        {
            string code = @"class C{    def f(a:int,b:int)    {        return = 1;    }}p = C.C();a = p.f({1,2}<1>, {3,4}<2>);b = a[0];";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", new object[] { 1, 1 });
        }

        [Test]
        public void TestReplicationGuidesOnDotOps04()
        {
            string code = @"class C{    def f(a : int)    {        return = 10;    }}p = {C.C(), C.C()};x = p<1>.f({1,2}<2>);y = x[0];z = x[1];";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("y", new object[] { 10, 10 });
            thisTest.Verify("z", new object[] { 10, 10 });
        }

        [Test]
        public void TestReplicationGuidesOnDotOps05()
        {
            string code = @"class A{        x;        constructor A ( x1 )        {            x = x1;        }        def foo ( y )        {            return = x + y;        }}a = A.A (0..1);b = 2..3;x = a<1>.foo(b<2>);y = x[0];z = x[1];";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("y", new object[] { 2, 3 });
            thisTest.Verify("z", new object[] { 3, 4 });
        }

        [Test]
        public void TestTypeArrayAssign4()
        {
            string code = @"a:int[] = {1, 2, 3};a[0] = false;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] { null, 2, 3 });
        }

        [Test]
        public void TestTypeArrayAssign5()
        {
            string code = @"a = {false, 2, true};b:int[] = a;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", new object[] { null, 2, null });
        }

        [Test]
        public void TestTypeArrayAssign6()
        {
            string code = @"a:int = 2;a[1] = 3;;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", null);
        }

        [Test]
        public void TestTypeArrayAssign_1467462()
        {
            string code = @"x:int[] = 1..4;x[{2,3}] = {1, 2};";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", new object[] { 1, 2, 1, 2 });
        }

        [Test]
        public void NestedBlocks001()
        {
            String code =
@"a;[Associative]{    a = 4;    b = a*2;                    [Imperative]    {        i=0;        temp=1;        //if(i<=a)        //{            //temp=temp+1;        //}    }    a = 1;}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Obj o = mirror.GetValue("a");
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 1);
        }
        [Ignore]
        public void BitwiseOp001()
        {
            String code =
                        @"                        [Associative]                        {	                        a = 2;	                        b = 3;                            c = a & b;                        }                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 2);
        }
        [Ignore]
        public void BitwiseOp002()
        {
            String code =
                        @"                        [Associative]                        {	                        a = 2;	                        b = 3;                            c = a | b;                        }                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 3);
        }
        [Ignore]
        public void BitwiseOp003()
        {
            String code =
                        @"                        [Associative]                        {	                        a = 2;	                        b = ~a;                        }                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == -3);
        }
        [Ignore]
        public void BitwiseOp004()
        {
            String code =
                        @"                        [Associative]                        {	                        a = true;                            b = false;	                        c = a^b;                        }                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", null);
        }

        [Test]
        public void LogicalOp001()
        {
            String code =
                        @"e;                        [Associative]                        {	                        a = true;	                        b = false;                            c = 1;                            d = a && b;                            e = c && d;                        }                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("e", false);
        }

        [Test]
        public void LogicalOp002()
        {
            String code =
                        @"e;                        [Associative]                        {	                        a = true;	                        b = false;                            c = 1;                            d = a || b;                            e = c || d;                        }                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("e", true);
        }

        [Test]
        public void LogicalOp003()
        {
            String code =
                        @"c;                        [Associative]                        {	                        a = true;	                        b = false;                            c = !(a || !b);                        }                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", false);
        }

        [Test]
        public void DoubleOp()
        {
            String code =
                        @"b;                        [Associative]                        {	                        a = 1 + 2;                            b = 2.0;                            b = a + b;                         }                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Double)mirror.GetValue("b").Payload == 5.0);
        }

        [Test]
        public void TestEq()
        {
            string code= @"
class A {}
a = A();
b = 42;
c = a == b;
            ";

            thisTest.RunScriptSource(code);
            thisTest.Verify("c", false);
        }

        [Test]
        public void RangeExpr001()
        {
            String code =
                        @"b;c;d;e;f;                        [Associative]                        {	                        a = 1..5;                            b = a[0];	                        c = a[1];	                        d = a[2];	                        e = a[3];                             f = a[4];                        }                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 3);
            Assert.IsTrue((Int64)mirror.GetValue("e").Payload == 4);
            Assert.IsTrue((Int64)mirror.GetValue("f").Payload == 5);
        }

        [Test]
        public void RangeExpr002()
        {
            String code =
                        @"b;c;d;e;                        [Associative]                        {	                        a = 1.5..5..1.1;                            b = a[0];	                        c = a[1];	                        d = a[2];	                        e = a[3];                                                     }                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Double)mirror.GetValue("b").Payload == 1.5);
            Assert.IsTrue((Double)mirror.GetValue("c").Payload == 2.6);
            Assert.IsTrue((Double)mirror.GetValue("d").Payload == 3.7);
            Assert.IsTrue((Double)mirror.GetValue("e").Payload == 4.8);
        }

        [Test]
        public void RangeExpr003()
        {
            String code =
                        @"b;c;d;e;                        [Associative]                        {	                        a = 15..10..-1.5;                            b = a[0];	                        c = a[1];	                        d = a[2];	                        e = a[3];                                                     }                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Double)mirror.GetValue("b").Payload == 15.0);
            Assert.IsTrue((Double)mirror.GetValue("c").Payload == 13.5);
            Assert.IsTrue((Double)mirror.GetValue("d").Payload == 12.0);
            Assert.IsTrue((Double)mirror.GetValue("e").Payload == 10.5);
        }

        [Test]
        public void RangeExpr004()
        {
            String code =
                        @"b;c;d;e;f;                        [Associative]                        {	                        a = 0..15..#5;                            b = a[0];	                        c = a[1];	                        d = a[2];	                        e = a[3];                             f = a[4];                                                    }                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Double)mirror.GetValue("b").Payload == 0);
            Assert.IsTrue((Double)mirror.GetValue("c").Payload == 3.75);
            Assert.IsTrue((Double)mirror.GetValue("d").Payload == 7.5);
            Assert.IsTrue((Double)mirror.GetValue("e").Payload == 11.25);
            Assert.IsTrue((Double)mirror.GetValue("f").Payload == 15);
        }

        [Test]
        public void RangeExpr005()
        {
            String code =
                        @"b;c;d;e;f;                        [Associative]                        {	                        a = 0..15..~4;                            b = a[0];	                        c = a[1];	                        d = a[2];	                        e = a[3];                              f = a[4];                                                   }                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Double)mirror.GetValue("b").Payload == 0);
            Assert.IsTrue((Double)mirror.GetValue("c").Payload == 3.75);
            Assert.IsTrue((Double)mirror.GetValue("d").Payload == 7.5);
            Assert.IsTrue((Double)mirror.GetValue("e").Payload == 11.25);
            Assert.IsTrue((Double)mirror.GetValue("f").Payload == 15);
        }

        [Test]
        public void RangeExpr06()
        {
            string code = @"
x1 = 0..#(-1)..5;
x2 = 0..#0..5;
x3 = 0..#1..10;
x4 = 0..#5..10;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x1", null);
            thisTest.Verify("x2", new object[] { });
            thisTest.Verify("x3", new object[] { 0 });
            thisTest.Verify("x4", new object[] { 0, 10, 20, 30, 40 });
        }

        [Test]
        public void FunctionWithinConstr001()
        {
            String code =
                        @"                                                class Dummy                        {		                    x : var;                            def init : bool ()                            {       			                    x = 5;			                    return=false;                            }                                    constructor Create()                            {                                dummy = init();			                            }                        }a;                        [Associative]                        {                                d = Dummy.Create();		                        a = d.x;                        }                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("a", 0).Payload == 5);
        }

        [Test]
        public void InlineCondition001()
        {
            String code =
                        @"	                        a = 10;                            b = 20;                            c = a < b ? a : b;                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 10);
        }

        [Test]
        public void InlineCondition002()
        {
            String code =
                        @"		                        a = 10;			                b = 20;                            c = a > b ? a : a == b ? 0 : b;                         ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 20);
        }

        [Test]
        public void InlineCondition003()
        {
            String code =
                        @"a = {11,12,10};t = 10;b = a > t ? 2 : 1;x = b[0];y = b[1];z = b[2];                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("y").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("z").Payload == 1);
        }

        [Test]
        public void InlineCondition004()
        {
            String code =
                        @"def f(i : int){    return = i + 1;}def g(){    return = 1;}a = {10,0,10};t = 1;b = a > t ? f(10) : g();x = b[0];y = b[1];z = b[2];                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 11);
            Assert.IsTrue((Int64)mirror.GetValue("y").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("z").Payload == 11);
        }
        [Ignore]
        public void PrePostFix001()
        {
            String code =
                        @"	                        a = 5;                            b = ++a;                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 6);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 6);
        }
        [Ignore]
        public void PrePostFix002()
        {
            String code =
                        @"	                        a = 5;                            b = a++;                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 6);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 5);
        }
        [Ignore]
        public void PrePostFix003()
        {
            String code =
                        @"	                        a = 5;			//a=5;                            b = ++a;		//b =6; a =6;                            a++;			//a=7;                            c = a++;		//c = 7; a = 8;                        ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 8);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 6);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 7);
        }

        [Test]
        public void Modulo001()
        {
            String code =
                @"                    a = 10 % 4; // 2                    b = 5 % a; // 1                    c = b + 11 % a * 3 - 4; // 0                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 0);
        }

        [Test]
        public void Modulo002()
        {
            String code =
               @"                    a = 10 % 4; // 2                    b = 5 % a; // 1                    c = 11 % a == 2 ? 11 % 2 : 11 % 3; // 2                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 2);
        }

        [Test]
        public void Modulo003()
        {
            string code = @"a = 5.2 % 2.3;";
            var mirror = thisTest.RunScriptSource(code);
            mirror.Verify("a", 0.6);
        }

        [Test]
        public void ModuloByZero()
        {
            string code =
@"
a = 2 % 0;
b = 2.1 % 0;
";

            thisTest.RunScriptSource(code);
            thisTest.Verify("a", null);
            thisTest.Verify("b", double.NaN);

            var warnings = thisTest.GetTestRuntimeCore().RuntimeStatus.Warnings;
            Assert.IsTrue(warnings.Any(w => w.ID == ProtoCore.Runtime.WarningID.kModuloByZero));
        }

        [Test]
        public void NegativeIndexOnCollection001()
        {
            String code =
                @"                    a = {1, 2, 3, 4};                    b = a[-2]; // 3                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 3);
        }

        [Test]
        public void NegativeIndexOnCollection002()
        {
            String code =
                @"                    a = { { 1, 2 }, { 3, 4 } };                    b = a[-1][-2]; // 3                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 3);
        }

        [Test]
        public void NegativeIndexOnCollection003()
        {
            String code =
                @"                    class A                    {	                    x : var[];		                    constructor A()	                    {		                    x = { B.B(), B.B(), B.B() };	                    }                    }                    class B                    {	                    x : var[]..[];		                    constructor B()	                    {		                    x = { { 1, 2 }, { 3, 4 },  { 5, 6 } };			                    }                    }                    a = { A.A(), A.A(), A.A() };                    b = a[-2].x[-3].x[-2][-1]; // 4                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 4);
        }

        [Test]
        public void PopListWithDimension()
        {
            String code =
                @"class A{	y : var;	z : var[];		constructor A()	{		y = 10;		z = { B.B(40, 50), B.B(60, 70) };	}}class B{	m : var;	n : var;	constructor B(i : int, j : int)	{		m = i;		n = j;	}}a = A.A();b = B.B(1, 2);a.z[1] = b;watch1 = a.z[1].m; watch2 = a.z[1].n; a.z[1].m = 10;                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("watch1", 10);
            thisTest.Verify("watch2", 2);
        }

        [Test]
        public void TestUpdate01()
        {
            String code =
                @"                    a = 1;                    b = a;                    a = 10;                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 10);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 10);
        }

        [Test]
        public void TestUpdate02()
        {
            String code =
                @"                    a = 1;                    b = b + a;                    b = 2;                    a = 10;                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 2);
        }

        [Test]
        public void TestUpdate03()
        {
            String code =
                @"def f : int(p : int){    a = 10;    b = a;    a = p;    return = b;}x = 20;y = f(x);x = 40;                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 40);
            Assert.IsTrue((Int64)mirror.GetValue("y").Payload == 40);
        }

        [Test]
        public void TestUpdateRedefinition01()
        {
            String code =
                @"a = 1;c = 2;b = a + 1;b = c + 1;a = 3;                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 3);
        }

        [Test]
        public void TestUpdateRedefinition02()
        {
            String code =
                @"                    a = 1;                    a = a + 1;                    a = 10;                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 10);
        }

        [Test]
        public void TestArrayUpdate01()
        {
            String code =
                @"a = {10,11,12};t = 0;i = a[t];t = 2;                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("i").Payload == 12);
        }

        [Test]
        public void TestFunctionUpdate01()
        {
            String code =
                @"class C{    x : int;    constructor C()    {        x = 1;    }}def f(a : C){    a.x = 10;    return = 0;}p = C.C();i = p.x;t = f(p);                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("i").Payload == 10);
        }

        [Test]
        [Category("JunToFix")]
        public void TestFunctionUpdate02()
        {
            String code =
                @"class A {    a : int;    constructor A ()    {        b = 1;        a = b;        b = 10;    }}x = A.A();y = x.a;                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("y").Payload == 10);
        }

        [Test]
        public void TestNoUpdate01()
        {
            String code =
                @"class Line{    x : int;    constructor Line(i : int)    {        x = i;    }    def Trim()    {        return = Line.Line(x - 1);           }}myline = Line.Line(10);myline = myline.Trim();myline = myline.Trim();length = myline.x;                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("length").Payload == 8);
        }

        [Test]
        public void TestPropertyUpdate01()
        {
            String code =
                @"class A{    x : int;	    constructor A()    {        x = 0;    }}p = A.A();a = p.x;p.x = 2;                 ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 2);
        }
        // Comment Jun: Investigate how replicating setters have affected this update

        [Test]
        public void TestPropertyUpdate02()
        {
            String code =
                @"class A{    x : int;	    constructor A()    {        x = 0;    }}p = A.A();b = 2;p.x = b;b = 10;t = p.x;                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1467249 - Sprint25: rev 3468 : REGRESSION: class property update is not propagating");
            Assert.IsTrue((Int64)mirror.GetValue("t").Payload == 10);
        }

        [Test]
        public void TestPropertyUpdate03()
        {
            String code =
                @"class A{    x : int;	    constructor A()    {        x = 1;    }}class B{    m : var;	    constructor B()    {        m = A.A();    }}p = B.B();a = p.m.x;p.m.x = 2;                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 2);
        }

        [Test]
        public void TestPropertyUpdate04()
        {
            String code =
                @"class A{    x : int;	    constructor A()    {        x = 1;    }}class B{    m : var;	    constructor B()    {        m = A.A();    }}p = B.B();b = 2;p.m.x = b;b = 10;t = p.m.x;                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("t").Payload == 10);
        }

        [Test]
        public void TestPropertyUpdate05()
        {
            String code =
                @"class f{	x : var;	y : var;	constructor f()	{		x = 1;			y = 2;		}}p = f.f();i = p.x;p.y = 1000;                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetFirstValue("i").Payload == 1);
        }

        [Test]
        public void TestPropertyUpdate06()
        {
            String code =
                @"class C{    x :var;    constructor C()    {        x = 10;    }}p = C.C();p.x = p.x + 1;p.x = p.x + 1;t = p.x;                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetFirstValue("t").Payload == 12);
        }


        [Test]
        public void TestPropertyUpdate07()
        {
            String code =
                @"class C{    x : int;    constructor C(i:int)    {        x = i;    }}i = 10;a = C.C(i);a.x = 15;v = a.x;i = 7;                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetFirstValue("v").Payload == 15);
        }

        [Test]
        public void TestLHSUpdate01()
        {
            String code =
                @"class C{    x : var;    constructor C(i : int)    {        x = D.D(i);    }}class D{    y : int = 0;    constructor D(i : int)    {        y = i;    }}a = C.C();// must reexecute a.x.y = 10 because a.x was modifieda.x.y = 10;i = a.x.y;a.x = D.D(2);j = a.x.y;                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetFirstValue("i").Payload == 10);
            Assert.IsTrue((Int64)mirror.GetFirstValue("j").Payload == 10);
        }

        [Test]
        public void TestLHSUpdate02()
        {
            String code =
                @"class A{    a : int;    }a1 = A.A();a1.a = 1;b = a1.a; // Should only update oncea1.a = 10;                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetFirstValue("b").Payload == 10);
        }

        [Test]
        public void TestPropertyModificationInMethodUpdate01()
        {
            String code =
                @"class C{    mx : var;    constructor C ()	{	    mx = 1; 	}	def f()	{		mx = 10;		return = 0; 	}}p = C.C();x = p.mx; a = p.f();                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetFirstValue("x").Payload == 10);
        }

        [Test]
        public void TestPropertyModificationInMethodUpdate02()
        {
            String code =
                @"class C{    mx : var;	my : var;     constructor C ()	{	    mx = 1; 	    my = 2; 	}	def f()	{		mx = 10;		my = 20;		return = 0; 	}}p = C.C();x = p.mx; y = p.my; a = p.f();                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetFirstValue("x").Payload == 10);
            Assert.IsTrue((Int64)mirror.GetFirstValue("y").Payload == 20);
        }

        [Test]
        public void TestXLangUpdate01()
        {
            String code =
                @"a;b;[Associative]{    a = 1;    b = a;    [Imperative]    {        a = a + 1;    }}                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 2);
        }

        [Test]
        public void TestXLangUpdate02()
        {
            String code =
                @"a;b;[Associative]{    a = 1;    b = a;    a = 10;    [Imperative]    {        a = a + 1;    }}                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 11);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 11);
        }

        [Test]
        public void TestXLangUpdate03()
        {
            String code =
                @"a;b;c;d;[Associative]{    a = 1;    b = a;    c = 100;    d = c;    [Imperative]    {        a = a + 1;        c = 10;    }}                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("a").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("b").Payload == 2);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 10);
            Assert.IsTrue((Int64)mirror.GetValue("d").Payload == 10);
        }

        [Test]
        public void TestXLangUpdate04()
        {
            String code =
                @"def f(p : int){    return = p;}a = 1;i = [Imperative]{    return = f(a);}a = 10;                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetValue("i").Payload == 10);
        }

        [Test]
        public void TestXLangUpdate_AssociativeTriggersAssociative01()
        {
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4585
            String code =
                @"a = 1;
x = [Associative]
{
    return = a + 10;
}
a = 2;                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            string err = "MAGN-4585: Failure to trigger update in an inner associative block";
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 12, err);
        }

        [Test]
        public void TestXLangUpdate_AssociativeTriggersAssociative02()
        {
            // Tracked in: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4585
            String code =
                @"a = 1;
x = [Associative]
{
    return = a + 100;
}


y = [Associative]
{
    return = a + 200;
}
a = 10;
                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            string err = "MAGN-4585: Failure to trigger update in an inner associative block";
            Assert.IsTrue((Int64)mirror.GetValue("x").Payload == 110, err);
            Assert.IsTrue((Int64)mirror.GetValue("y").Payload == 210, err);
        }


        [Test]
        public void TestGCRefCount()
        {
            String code =
                @"class point{    x : var;    y : var;    constructor point()    {        x = 10;        y = 20;    }    def _Dispose : int()    {        x = 100;        return = null;    }}def f : int(){    p = point.point();    p2 = p;    return = p.x;}i = f();n = point.point();                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
        }

        [Test]
        [Ignore]
        [Category("ProtoGeometry")]
        [Category("PortToCodeBlocks")]
        public void TestGCFFI001()
        {
            String code =
                @"def foo : int(){	p = Point.ByCoordinates(10, 20, 30);	p2 = Point.ByCoordinates(12, 22, 32);	p3 = Point.ByCoordinates(14, 24, 34);	return = 10;}p = Point.ByCoordinates(15, 25, 35);x = p.X;y = foo();                ";
            code = string.Format("{0}\n{1}", "import(\"ProtoGeometry.dll\");", code);
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
        }

        [Test]
        [Ignore]
        [Category("ProtoGeometry")]
        [Category("PortToCodeBlocks")]
        public void TestGCRefCount002()
        {
            String code =
                @"def CreatePoint : Point(x : int, y : int, z : int){	return = Point.ByCoordinates(x, y, z);}def getx : double(p : Point){	return = p.X;}p = CreatePoint(5, 6, 7);x = getx(p);                ";
            code = string.Format("{0}\n{1}", "import(\"ProtoGeometry.dll\");", code);
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Obj o = mirror.GetFirstValue("x");
            Assert.IsTrue((Double)o.Payload == 5.0);
        }

        [Test]
        public void TestGlobalVariable()
        {
            String code =
                @"                    gx = 100;                    def f : int()                    {                        return = gx;                    }                    i = f();                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue((Int64)mirror.GetFirstValue("gx").Payload == 100);
        }

        [Test]
        [Ignore]
        [Category("ProtoGeometry")]
        [Category("PortToCodeBlocks")]
        public void TestNullFFI()
        {
            String code =
                @"class Test{    X : int;    constructor Test(x : int)    {        X = x;    }        def Equals : bool (other : Test)    {        return = (other.X == this.X);    }}x = {1001,2001};t = Point.ByCoordinates(x, 0, 0);s = t;s[1] = null;check = s.Equals(t);value = check[1];Print(check);                ";
            code = string.Format("{0}\n{1}", "import(\"ProtoGeometry.dll\");", code);
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Assert.IsTrue(mirror.GetFirstValue("value").DsasmValue.optype == ProtoCore.DSASM.AddressType.Null);
        }

        [Test]
        public void TestAttributeOnClass()
        {
            string src = @"class TestAttribute
{
	constructor TestAttribute()
	{}
}
class VisibilityAttribute
{
	x : var;
	constructor VisibilityAttribute(_x : var)
	{
		x = _x;
	}
}
[Test, Visibility(1)]
class Point
{
	
[Test]
	public x : var;
	[Visibility(2)]
	public y : var;
	
	[Test, Visibility(1)]
	constructor Point()
	{
		x = 10; y = 10;
	}
	
[Test]
	public static def foo : int()
	{
		return = 10;
	}
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.VerifyBuildWarningCount(0);
        }

        [Test]
        public void TestAttributeOnGlobalFunction()
        {
            string src = @"class TestAttribute
{
	constructor TestAttribute()
	{}
}
class VisibilityAttribute
{
	x : var;
	constructor VisibilityAttribute(_x : var)
	{
		x = _x;
	}
}
[Test, Visibility(1)]
class Point
{
	
[Test]
	public x : var;
	[Visibility(2)]
	public y : var;
	
	[Test, Visibility(1)]
	constructor Point()
	{
		x = 10; y = 10;
	}
	
[Test]
	public static def foo : int()
	{
		return = 10;
	}
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.VerifyBuildWarningCount(0);
        }

        [Test]
        public void TestAttributeOnLanguageBlock()
        {
            string src = @"class TestAttribute
{
	constructor TestAttribute()
	{}
}
class VisibilityAttribute
{
	x : var;
	constructor VisibilityAttribute(_x : var)
	{
		x = _x;
	}
}
[Imperative, version=""###"", Visibility(11), fingerprint=""FS54"", Test] 
{
	a = 19;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.VerifyBuildWarningCount(0);
        }

        [Test]
        public void TestAttributeWithLanguageBlockAndArrayExpression()
        {
            string src = @"class TestAttribute
{
	constructor TestAttribute()
	{}
}
class VisibilityAttribute
{
	x : var;
	constructor VisibilityAttribute(_x : var)
	{
		x = _x;
	}
}
def foo : int[]..[](p : var[]..[])
{
	a = { 1, { 2, 3 }, 4 };
	return = a[1];
}
[Associative, version=""###"", Visibility(11), fingerprint=""FS54"", Test] 
{
	a = {1, 2, 3};
	b = a[1];
	c = a[0];
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.VerifyBuildWarningCount(0);
        }

        [Test]
        public void TestBasicArrayMethods()
        {
            string src = @"a = { 1, 2, { 3, 4, 5, { 6, 7, { 8, 9, { { 11 } } } } }, { 12, 13 } };
c = Count(a);
r = Rank(a);
a2 = Flatten(a);";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("c").Payload == 4);
            Assert.IsTrue((Int64)mirror.GetValue("r").Payload == 6);
            mirror.CompareArrays("a2", new List<Object> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, typeof(Int64));
        }

        [Test]
        public void TestStringConcatenation01()
        {
            string src = @"s1='a';
s2=""bcd"";
s3=s1+s2;

s4=""abc"";
s5='d';
s6=s4+s5;

s7=""ab"";
s8=""cd"";
s9=s7+s8;";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("s3", "abcd");
            thisTest.Verify("s6", "abcd");
            thisTest.Verify("s9", "abcd");
        }


        [Test]
        public void TestStringOperations()
        {
            string src = @"
class A{}
s = ""ab"";
r1 = s + 3;
r2 = s + false;
r4 = !s;
r44 = !A.A();
r444 = !1;
r5 = s == ""ab"";
r6 = s == s;
r7 = ""ab"" == ""ab"";
ns = s;
ns[0] = 1;
r9 = s != ""ab"";
ss = ""abc"";
m = ss;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("r1", "ab3");
            thisTest.Verify("r2", "abfalse");
            thisTest.Verify("r4", false);
            thisTest.Verify("r44", false);
            thisTest.Verify("r444", false);
            thisTest.Verify("r5", true);
            thisTest.Verify("r6", true);
            thisTest.Verify("r7", true);
            thisTest.Verify("r9", false);
            thisTest.Verify("ss", "abc");
        }

        [Test]
        public void TestStringTypeConversion()
        {
            //Assert.Fail("DNL-1467239 Sprint 26 - Rev 3425 type conversion - string to bool conversion failing");
            string src = @"def foo:bool(x:bool)
{
    return=x;
}
r1 = foo('h');
r2 = 'h' && true;
r3 = 'h' + 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("r1", true);
            thisTest.Verify("r2", true);
            thisTest.Verify("r3", null);
        }

        [Test]
        public void TestTypeArrayAssign()
        {
            String code =
@"t:int[] = {1,2,3};";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t", new Object[] { 1, 2, 3 });
        }

        [Test]
        public void TestTypeArrayAssign2()
        {
            String code =
@"t:int[];t = {1,2,3};";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t", new Object[] { 1, 2, 3 });
        }

        [Test]
        public void TestTypeArrayAssign3()
        {
            String code =
@"class A {    t:int[];    def foo() {        t = {1,2,3};        return = t;    }}a = A.A();b = a.foo();ret = a.t;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", new Object[] { 1, 2, 3 });
            thisTest.Verify("ret", new Object[] { 1, 2, 3 });
        }

        [Test]
        public void TestTypedAssignment01()
        {
            String code =
@"class A{    x:int;    def foo()    {        x:double = 4.5;        return = null;    }}a = A.A();t = a.foo();x = a.x;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 5);
        }

        [Test]
        public void TestTypedAssignment02()
        {
            string code =
@" t1:int = 1;t1 = 3.5;t2:var = 2;t2 = 4.3;t3 = false;t3 = 4.9;t4 = 1;t4:int = 3.9;t4:var = 5.1;t4 = 6.1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t1", 4);
            thisTest.Verify("t2", 4.3);
            thisTest.Verify("t3", 4.9);
            thisTest.Verify("t4", 6.1);
        }

        [Test]
        public void TestTypedAssignment03()
        {
            string code =
@" t1;t2;t3;t4;[Imperative]{    t1:int = 1;    t1 = 3.5;    t2:var = 2;    t2 = 4.3;    t3 = false;    t3 = 4.9;    t4 = 1;    t4:int = 3.9;    t4:var = 5.1;    t4 = 6.1;}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t1", 4);
            thisTest.Verify("t2", 4.3);
            thisTest.Verify("t3", 4.9);
            thisTest.Verify("t4", 6.1);
        }

        [Test]
        public void TestTypedAssignment04()
        {
            string code =
@"class A{    x:int = 1;}t:A = A.A();r1 = t;t = 3;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r1", null);
        }

        [Test]
        public void TestTypedAssignment05()
        {
            string code =
@"class A{    x:int = 1;    def foo()    {        p:int = 3;        p = false;        return = p;    }}a = A.A();r = a.foo();";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", null);
        }

        [Test]
        public void TestTypedAssignment06()
        {
            string code =
@"x:int = 3.5;x:bool;y:int = 0;y:bool;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", true);
            thisTest.Verify("y", false);
        }

        [Test]
        [Category("Escalate")]
        [Category("ToFixJun")]
        public void TestPropAssignWithReplication()
        {
            //Assert.Fail("DNL-1467241 Sprint25: rev 3420 : Property assignments using replication is not working");
            string code =
@"class A{    x : int;    t : int;    constructor A( y)    {        x = y;    }} a1 = { A.A(1), A.A(2) };a1.t = 5;testx = a1.x;test = a1.t;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("testx", new Object[] { 1, 2 });
            thisTest.Verify("test", new Object[] { 5, 5 });
        }

        [Test]
        public void TestPropAssignWithReplication02()
        {
            string code =
@"class A {    x : int;    constructor A(i : int)    {        x = i;    }}a = {A.A(10), A.A(20)};a.x = 5;t = a.x;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t", new Object[] { 5, 5 });
        }

        [Test]
        public void TestGlobalFunctionRecursion100()
        {
            string code =
@"def f(i : int){    loc = [Imperative]    {        a = 0;        if (i > 1)        {            return = f(i - 1) + i + a;        }        return = i;    }    return = loc;}x = 100;y = f(x);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("y", 5050);
        }

        [Test]
        public void TestGlobalFunctionRecursion100_GlobalIncrement()
        {
            string code =
@"global = 0;def f(i : int){    loc = [Imperative]    {        a = 0;        if (i > 1)        {            return = f(i - 1) + i + a;        }        return = i;    }    global = global + 1;    return = loc;}x = 100;y = f(x);z = global;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("y", 5050);
            thisTest.Verify("z", 100);
        }

        [Test]
        public void TestGlobalFunctionRecursion100_GlobalIncrementInFunction01()
        {
            string code =
@"global = 0;def g(){    global = global + 1;    return = 0;}def f(i : int){    loc = [Imperative]    {        a = 0;        if (i > 1)        {            return = f(i - 1) + i + a;        }        return = i;    }    t = g();    return = loc;}x = 100;y = f(x);z = global;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("y", 5050);
            thisTest.Verify("z", 100);
        }

        [Test]
        public void TestGlobalFunctionRecursion100_GlobalIncrementInFunction02()
        {
            string code =
@"global = 0;def g(){    global = global + 1;    return = 0;}def h(){    global = global + 1;    return = 0;}def f(i : int){    loc = [Imperative]    {        a = 0;        if (i > 1)        {            return = f(i - 1) + i + a;        }        return = i;    }    s = g();    t = h();    return = loc;}x = 100;y = f(x);z = global;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("y", 5050);
            thisTest.Verify("z", 200);
        }

        [Test]
        public void TestGlobalFunctionRecursionReplication()
        {
            string code =
@"def f(i : int){    loc = [Imperative]    {        xx = 0;        if (i > 1)        {            return = f(i - 1) + i + xx;        }        return = i;    }    return = loc;}x = {100,200,300};y = f(x);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("y", new Object[] { 5050, 20100, 45150 });
        }

        [Test]
        public void TestrecusionWithNestedFunction01()
        {
            string code =
@"def if_1(x)
{
    return = 1;
}

def if_2(x)
{
    v1 = x - 1;
    v2 = foo(v1);
    v3 = x * v2;
    return = v3;
}

def foo(x)
{
    c = x <= 1;

    v = [Imperative]
    {
        if (c)
        {
            return = if_1(x);
        }
        return = if_2(x);
    }

    return = v;
}

r = foo(3);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 6);
        }

        [Test]
        public void TestContextInject01()
        {
            ProtoScript.Runners.ProtoRunner runner = new ProtoScript.Runners.ProtoRunner();
            string code =
@"[Associative]{	a = x;     b = y;    c = a + b;        x = 10;    y = 20;}";
            // TODO Jun: Move this test to the existing location context inject testcases
            // Add state verification
            Dictionary<string, Object> context = new Dictionary<string, object>();
            context.Add("x", 1);
            context.Add("y", 2);
            ProtoScript.Runners.ProtoRunner.ProtoVMState vmstate = runner.PreStart(code, context);
            runner.RunToNextBreakpoint(vmstate);
        }

        [Test]
        public void TestBasicFFIReplicate()
        {
            string code =
@"a = {25, 36, 49};r = Minimal.Sqrt(a);";
            code = string.Format("{0}\n{1}", "import(\"FFITarget.dll\");", code);
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", new Object[] { 5.0, 6.0, 7.0 });
        }

        [Test]
        public void Test_Compare_Node_01()
        {
            string s1 = "a = 1;";
            string s2 = "a=(1);";

            ProtoCore.AST.Node s1Root = ProtoCore.Utils.ParserUtils.Parse(s1);
            ProtoCore.AST.Node s2Root = ProtoCore.Utils.ParserUtils.Parse(s2);
            bool areEqual = s1Root.Equals(s2Root);
            Assert.AreEqual(areEqual, true);
        }

        [Test]
        public void Test_Compare_Node_02()
        {
            string s1 = "a = 1; b=2;";
            string s2 = "a=(1) ; b = (2);";
            ProtoCore.AST.Node s1Root = ProtoCore.Utils.ParserUtils.Parse(s1);
            ProtoCore.AST.Node s2Root = ProtoCore.Utils.ParserUtils.Parse(s2);
            bool areEqual = s1Root.Equals(s2Root);
            Assert.AreEqual(areEqual, true);
        }

        [Test]
        public void Test_Compare_Node_03()
        {
            string s1 = "a     =   1;  c = a+1;";
            string s2 = "a = 1; c=a +    1;";
            ProtoCore.AST.Node s1Root = ProtoCore.Utils.ParserUtils.Parse(s1);
            ProtoCore.AST.Node s2Root = ProtoCore.Utils.ParserUtils.Parse(s2);
            bool areEqual = s1Root.Equals(s2Root);
            Assert.AreEqual(areEqual, true);
        }

        [Test]
        public void ParseTypedIdentifier_AstNode()
        {
            string s1 = "a : A;";
            string s2 = "a : A = null;";
            string s3 = "a : A.B.C;";
            string s4 = "a : A.B.C = null;";
            
            var s1Root = ProtoCore.Utils.ParserUtils.Parse(s1) as CodeBlockNode;
            Assert.IsNotNull(s1Root);
            var typedNode = s1Root.Body[0] as TypedIdentifierNode;
            Assert.IsNotNull(typedNode);
            Assert.AreEqual("A", typedNode.datatype.Name);

            s1Root = ProtoCore.Utils.ParserUtils.Parse(s2) as CodeBlockNode;
            Assert.IsNotNull(s1Root);
            var ben = s1Root.Body[0] as BinaryExpressionNode;
            Assert.IsNotNull(ben);
            typedNode = ben.LeftNode as TypedIdentifierNode;
            Assert.IsNotNull(typedNode);
            Assert.AreEqual("A", typedNode.datatype.Name);

            s1Root = ProtoCore.Utils.ParserUtils.Parse(s3) as CodeBlockNode;
            Assert.IsNotNull(s1Root);
            typedNode = s1Root.Body[0] as TypedIdentifierNode;
            Assert.IsNotNull(typedNode);
            Assert.AreEqual("A.B.C", typedNode.datatype.Name);

            s1Root = ProtoCore.Utils.ParserUtils.Parse(s4) as CodeBlockNode;
            Assert.IsNotNull(s1Root);
            ben = s1Root.Body[0] as BinaryExpressionNode;
            Assert.IsNotNull(ben);
            typedNode = ben.LeftNode as TypedIdentifierNode;
            Assert.IsNotNull(typedNode);
            Assert.AreEqual("A.B.C", typedNode.datatype.Name);
        }

        [Test]
        public void ParseTypedIdentifierWithRank_AstNode()
        {
            string s1 = "a : A[];";
            string s2 = "a : A[] = null;";
            string s3 = "a : A.B.C[];";
            string s4 = "a : A.B.C[] = null;";

            string s5 = "a : A[][];";
            string s6 = "a : A[][] = null;";
            string s7 = "a : A.B.C[][];";
            string s8 = "a : A.B.C[][] = null;";

            var root = ProtoCore.Utils.ParserUtils.Parse(s1) as CodeBlockNode;
            Assert.IsNotNull(root);
            var typedNode = root.Body[0] as TypedIdentifierNode;
            Assert.IsNotNull(typedNode);
            Assert.AreEqual("A", typedNode.datatype.Name);
            Assert.AreEqual(1, typedNode.datatype.rank);

            root = ProtoCore.Utils.ParserUtils.Parse(s2) as CodeBlockNode;
            Assert.IsNotNull(root);
            var ben = root.Body[0] as BinaryExpressionNode;
            Assert.IsNotNull(ben);
            typedNode = ben.LeftNode as TypedIdentifierNode;
            Assert.IsNotNull(typedNode);
            Assert.AreEqual("A", typedNode.datatype.Name);
            Assert.AreEqual(1, typedNode.datatype.rank);

            root = ProtoCore.Utils.ParserUtils.Parse(s3) as CodeBlockNode;
            Assert.IsNotNull(root);
            typedNode = root.Body[0] as TypedIdentifierNode;
            Assert.IsNotNull(typedNode);
            Assert.AreEqual("A.B.C", typedNode.datatype.Name);
            Assert.AreEqual(1, typedNode.datatype.rank);

            root = ProtoCore.Utils.ParserUtils.Parse(s4) as CodeBlockNode;
            Assert.IsNotNull(root);
            ben = root.Body[0] as BinaryExpressionNode;
            Assert.IsNotNull(ben);
            typedNode = ben.LeftNode as TypedIdentifierNode;
            Assert.IsNotNull(typedNode);
            Assert.AreEqual("A.B.C", typedNode.datatype.Name);
            Assert.AreEqual(1, typedNode.datatype.rank);

            root = ProtoCore.Utils.ParserUtils.Parse(s5) as CodeBlockNode;
            Assert.IsNotNull(root);
            typedNode = root.Body[0] as TypedIdentifierNode;
            Assert.IsNotNull(typedNode);
            Assert.AreEqual("A", typedNode.datatype.Name);
            Assert.AreEqual(2, typedNode.datatype.rank);

            root = ProtoCore.Utils.ParserUtils.Parse(s6) as CodeBlockNode;
            Assert.IsNotNull(root);
            ben = root.Body[0] as BinaryExpressionNode;
            Assert.IsNotNull(ben);
            typedNode = ben.LeftNode as TypedIdentifierNode;
            Assert.IsNotNull(typedNode);
            Assert.AreEqual("A", typedNode.datatype.Name);
            Assert.AreEqual(2, typedNode.datatype.rank);

            root = ProtoCore.Utils.ParserUtils.Parse(s7) as CodeBlockNode;
            Assert.IsNotNull(root);
            typedNode = root.Body[0] as TypedIdentifierNode;
            Assert.IsNotNull(typedNode);
            Assert.AreEqual("A.B.C", typedNode.datatype.Name);
            Assert.AreEqual(2, typedNode.datatype.rank);

            root = ProtoCore.Utils.ParserUtils.Parse(s8) as CodeBlockNode;
            Assert.IsNotNull(root);
            ben = root.Body[0] as BinaryExpressionNode;
            Assert.IsNotNull(ben);
            typedNode = ben.LeftNode as TypedIdentifierNode;
            Assert.IsNotNull(typedNode);
            Assert.AreEqual("A.B.C", typedNode.datatype.Name);
            Assert.AreEqual(2, typedNode.datatype.rank);
        }


        [Test]
        public void TestLHSUndefinedArrayIndex01()
        {
            string code = 
@"
a[i] = 10;     
b = a[0];       
            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("b", null);
        }

        [Test]
        public void TestLHSUndefinedArrayIndex02()
        {
            string code =
@"
a = {1,2,3};
a[i] = 10;     
b = a[0];       
            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("b", 1);
        }

        [Test]
        public void TestLocalKeywordDeclaration01()
        {
            string code =
@"
a : local = 1;       
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1);
        }


        [Test]
        public void TestLocalKeywordDeclaration02()
        {
            string code =
@"
a : local = 1;       
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1);
        }

        [Test]
        public void TestLocalKeywordDeclaration03()
        {
            string code =
@"
a : local int = 1;       
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1);
        }

        [Test]
        public void TestLocalKeywordDeclaration04()
        {
            string code =
@"
a : local = 1;      
b : local int = 2;       
c = a + b;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("c", 3);
        }

        [Test]
        public void TestLocalKeywordDeclaration05()
        {
            string code =
@"
a : local int = 1;      
b : local int = 2;       
c = a + b;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("c", 3);
        }

        [Test]
        public void TestLocalKeywordDeclaration06()
        {
            string code =
@"
a : local int = 1;      
b : local int = 2;       
c : local int = a + b;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("c", 3);
        }

        [Test]
        public void TestLocalKeywordDeclaration07()
        {
            string code =
@"
i = [Associative]
{
    a : local int = 1;      
    return = a;
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("i", 1);
        }

        [Test]
        public void TestLocalKeywordDeclaration08()
        {
            string code =
@"
i = [Associative]
{
    a : local int = 1;      
    b : local int = 2;       
    return = a + b;
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("i", 3);
        }

        [Test]
        public void TestLocalKeywordDeclarationNegativ01()
        {
            String code =
@"  
a : int local = 1;   // 'local' should come before any type specifier";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }


        [Test]
        public void TestLocalKeywordDeclarationNegativ02()
        {
            String code =
@"  
local = 1;   // 'local' is a reserved keyword";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        public void TestLocalKeywordDeclarationNegativ03()
        {
            String code =
@"  
local : int = 1;   // 'local' is a reserved keyword";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        public void TestLocalKeywordDeclarationNegativ04()
        {
            String code =
@"  
local = [Imperative] { return = 1; };   // 'local' is a reserved keyword";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }


        [Test]
        public void TestLocalKeywordFromLanguageBlock01()
        {
            string code =
@"
a = 1;
b = [Associative]
{
    a : local = 2;
    return = a;
}

c = a;
d = b;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("c", 1);
            thisTest.Verify("d", 2);
        }

        [Test]
        public void TestLocalKeywordFromLanguageBlock02()
        {
            string code =
@"
a = 1;
b = [Associative]
{
    a : local = 2;
    x : local = a;
    return = x;
}

c = a;
d = b;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("c", 1);
            thisTest.Verify("d", 2);
        }

        [Test]
        public void TestLocalKeywordFromFunction01()
        {
            string code =
@"
a = 1;
def f()
{
    a : local = 2;
    return = a;
}

p = f();
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("p", 2);
        }

        [Test]
        public void TestLocalKeywordFromFunction02()
        {
            string code =
@"
a = 1;
def f()
{
    a : local = 2;
    x : local = a;
    return = x;
}

p = f();
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("p", 2);
        }

        [Test]
        public void TestLocalFromMemberFunction01()
        {
            string code =
@"
class C
{
    a : int = 1;
    def f()
    {
        a : local = 2;
        return = a;
    }
}
p = C.C();
x = p.a;
y = p.f();
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 1);
            thisTest.Verify("y", 2);
        }

        [Test]
        public void TestLocalFromMemberFunction02()
        {
            string code =
@"
class C
{
    a : int = 1;
    def f()
    {
        a : local = 2;
        return = a;
    }


    def g()
    {
        a : local = 3;
        return = a;
    }
}
p = C.C();
x = p.a;
y = p.f();
z = p.g();
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 1);
            thisTest.Verify("y", 2);
            thisTest.Verify("z", 3);
        }


        [Test]
        public void TestLocalVariableUpdate01()
        {
            string code =
@"
i = [Associative]
{
    a : local = 1; 
    b : local = a;
    a = 2;
    return = b;
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("i", 2);
        }

        [Test]
        public void TestLocalVariableUpdate02()
        {
            string code =
@"
i = [Associative]
{
    a : local = 1; 
    b : local = a + 10;
    a = 2;
    return = b;
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("i", 12);
        }

        [Test]
        public void TestLocalVariableUpdate03()
        {
            string code =
@"
a = 10;
b = a;
i = [Associative]
{
    a : local = 1; 
    b : local = a;
    a = 2;          // Update local 'a'
    return = b;
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 10);
            thisTest.Verify("b", 10);
            thisTest.Verify("i", 2);
        }

        [Test]
        public void TestLocalVariableUpdate04()
        {
            string code =
@"
a = 10;
b = a;
i = [Associative]
{
    return = [Imperative]
    {
        return = [Associative]
        {
            a : local = 1; 
            b : local = a;
            a = 2;          // Update local 'a'
            return = b;
        }
    }
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("b", 10);
            thisTest.Verify("i", 2);
        }

        [Test]
        public void TestLocalVariableNoUpdate01()
        {
            string code =
@"
a = 1;
b = a;
c = [Associative]
{
    a : local = 2; // Updating local 'a' should not update global 'a'
    return = a;
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1);
            thisTest.Verify("b", 1);
            thisTest.Verify("c", 2);
        }

        [Test]
        public void TestLocalVariableNoUpdate02()
        {
            string code =
@"
a = 1;
b = a;
c = [Associative]
{
    a : local; 
    a = 2;      // Updating local 'a' should not update global 'a'
    return = a;
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1);
            thisTest.Verify("b", 1);
            thisTest.Verify("c", 2);
        }


        [Test]
        public void TestLocalVariableNoUpdate03()
        {
            string code =
@"
a : local = 1;      // Tagging a variable local at the global scope has no semantic effect
b : local = a;
c = [Associative]
{
    a : local; 
    a = 2;      // Updating local 'a' should not update global 'a'
    return = 1;
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1);
            thisTest.Verify("b", 1);
            thisTest.Verify("c", 1);
        }


        [Test]
        public void TestNullsOnExpression01()
        {
            string code =
@"
        a = 1 + null;
        b = """" == null;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", null);
            thisTest.Verify("b", false);
        }

        [Test]
        public void TestAddNullToString()
        {
            string code =
@"
a = ""hello"" + null;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", null);
        }
        
        [Test]
        public void TestUndefinedTypedIdentifier()
        {
            string code =
@"
        a : UndefinedType;
        b : UndefinedType2 = 2;
        c : UndefinedType3 = null;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", null);
            thisTest.Verify("b", null);
            thisTest.Verify("c", null);

            thisTest.VerifyRuntimeWarningCount(3);
        }
    }
}