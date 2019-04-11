using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;
namespace ProtoTest.TD
{
    class DefectRegress : ProtoTestBase
    {
        string importPath = "..\\..\\..\\test\\Engine\\ProtoTest\\ImportFiles\\";

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void Regress_1455158()
        {
            string code = @"
import(""FFITarget.dll"");         
def Modify : TestPoint(p : DummyPoint2D)
{
    tempX = p.X + 1;
    tempY = p.Y + 1;
    return = DummyPoint2D.ByCoordinates(tempX, tempY);
}          
pt1 = DummyPoint2D.ByCoordinates(1, 2);
pt2 = Modify(pt1);
x2 = pt2.X;
y2 = pt2.Y;
pt3 = Modify(pt2);
x3 = pt3.X;
y3 = pt3.Y;
	
	";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x3", 3);
            thisTest.Verify("y3", 4);
        }

        [Test]
        [Category("SmokeTest")]
        public void Regress_1455738()
        {
            string code = @"
b;
[Associative]
{
    a = 3;
    b = a * 2;
    a = 4;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b", 8);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void Regress_1455276()
        {
            string code = @"
import(""FFITarget.dll"");
def SquaredDistance : double (pt : DummyPoint, otherPt : DummyPoint)
{
    distx = (otherPt.X - pt.X);
    distx = distx * distx;
   
    disty = otherPt.Y -pt.Y;
        
    distz = otherPt.Z -pt.Z;
                
    return = distx + disty + distz;
}

pt1 = DummyPoint.ByCoordinates(0,0,0);
pt2 = DummyPoint.ByCoordinates(10.0, 0 ,0 );
dist = SquaredDistance(pt1, pt2);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("dist", 100.0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Regress_1455568()
        {
            //Assert.Fail("1467188 - Sprint24 : rev 3170: REGRESSION : instantiating a class more than once with same argument is causing DS to go into infinite loop!");
            string code = @"
import(""FFITarget.dll"");
C0 = DummyTuple4.XYZH(1.0,0,0,0);
C1 = DummyTuple4.XYZH(0,1.0,0,0);
C2 = DummyTuple4.XYZH(0,0,1.0,0);
C3 = DummyTuple4.XYZH(0,0,0,1.0);
t = DummyTuple4.XYZH(1,1,1,1);
tx = DummyTuple4.XYZH(C0.X, C1.X, C2.X, C3.X);
RX = tx.Multiply(t);
ty = DummyTuple4.XYZH(C0.Y, C1.Y, C2.Y, C3.Y);
RY = ty.Multiply(t);
tz = DummyTuple4.XYZH(C0.Z, C1.Z, C2.Z, C3.Z);
RZ = tz.Multiply(t);
th = DummyTuple4.XYZH(C0.H, C1.H, C2.H, C3.H);
RH = th.Multiply(t);
       
result1 =  DummyTuple4.XYZH(RX, RY, RZ, RH);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("RX", 1.0);
            thisTest.Verify("RY", 1.0);
            thisTest.Verify("RZ", 1.0);
            thisTest.Verify("RH", 1.0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Regress_1454075()
        {
            string dscode = @"
import(""FFITarget.dll"");
v = DummyVector.ByCoordinates(1,2,3);";
            ExecutionMirror mirror = thisTest.RunScriptSource(dscode);
            //Compilation test. 
        }

        [Test]
        public void Regress_1454723()
        {
            string dscode = @"
def sqrt : double  (val : double )
{
    result = 0.0;
    result = [Imperative]
             {
                return = 10.0 * val;
             }
    return = result;
}
ten=[Imperative]
{
    val = 10;
    ten = sqrt(val);
    return ten;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(dscode);
            thisTest.Verify("ten", 100.0);
        }

        [Test]
        public void Regress_1457064()
        {
            string dscode = @"
def Scale (arr : double[], scalingFactor : double)
{
    scaledArr = [Imperative]
    {
        counter = 0;
        for(val in arr)
        {
            arr[counter] = scalingFactor * val;
            counter = counter + 1;
        }
        return = arr;
    }
    return = scaledArr;
}
xCount =3;
dummy = 1;
rangeExpression = 0.0..(180*dummy)..#xCount;
result = Scale(rangeExpression, 2);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(dscode);
            object[] expectedResult = { 0.0, 180.0, 360.0 };
            thisTest.Verify("result", expectedResult, 0);
        }

        [Test]
        public void Regress_1456921()
        {
            string dscode = @"
b = 10.0;
a = 0.0;
d = a..b..c;";
            // Assert.Fail("1456921 - Sprint 20: Rev 2088: (negative), null expected when using an undefined variable ranged expression"); 
            string errmsg = "DNL-1467454 Rev 4596 : Regression : CompilerInternalException coming from undefined variable used in range expression";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(dscode, errmsg);
            object expectedResultc = null;
            thisTest.Verify("d", expectedResultc);
        }

        [Test]
        [Category("SmokeTest")]
        public void Regress_1454697()
        {
            String code =
            @"    def foo : double (array : double[])
    {
        return = 1.0 ;
    }
    
    arr = [1,2,3];
    arr2 = foo(arr);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification
            object arr2 = 1.0;
            thisTest.Verify("arr2", arr2, 0);
        }

        [Test]
        public void Regress_1457179()
        {
            string code = @"
import (""TestImport.ds"");
//external (""libmath"") def dc_sin : double (val : double);
def Sin : double (val : double)
{
    return = dc_sin(val);
}
result1 = Sin(90);
result2 = Sin(90.0);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "", importPath);
            //Verification
            object result1 = 180.0;
            object result2 = 180.0;
            thisTest.Verify("result1", result1, 0);
            thisTest.Verify("result2", result2, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void Regress_1458785()
        {
            string code = @"
def foo ( i )
{
	return = i;
}

	
a2 = foo();
a3 = 2;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a3", 2, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void Regress_1458785_2()
        {
            string code = @"
def foo ( i:int[])
{
return = i;
}
x =  1;
a1 = foo(x);
a2 = 3;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedResult = new Object[] { 1 };
            thisTest.Verify("a1", expectedResult, 0);
            thisTest.Verify("a2", 3, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void Regress_1458785_3()
        {
            string code = @"
import(""FFITarget.dll"");
a1 = DummyPoint.ByCoordinates(1,1,1);
x1 = a1.X;
y1 = a1.yy;
z1 = 2;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object expectedResult = null;
            thisTest.Verify("y1", expectedResult, 0);
            thisTest.Verify("z1", 2, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void Regress_1454692()
        {
            string code = @"
x;
y;
i=[Imperative]
{
	x = 0;
	b = 0..3; //{ 0, 1, 2, 3 }
	for( y in b )
	{
		x = y + x;
	}
	return [x,y];
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("i", new object[] {6, null});
        }

        [Test]
        [Category("SmokeTest")]
        public void Regress_1454692_2()
        {
            string code = @"
def length : int (pts : double[])
{
    numPts = [Imperative]
    {
        counter = 0;
        for(pt in pts)
        {
            counter = counter + 1;
        }
        
        return = counter;
    }
    return = numPts;
}
    
arr = 0.0..3.0;//{0.0,1.0,2.0,3.0};
num = length(arr);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] expectedresult = new Object[] { 0.0, 1.0, 2.0, 3.0 };
            thisTest.Verify("num", 4, 0);
            thisTest.Verify("arr", expectedresult, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void Regress_1455935()
        {
            string code = @"
b;
c;
d;
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
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("i", new[] {1, 1, 3});
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void Regress_1457862()
        {
            string code = @"
def Create : var(a : int[])
{
	return = a;
}
		
def foo(a : int)
{
	return = a;
}

def foo1(a : int)
{
	return = a;
}
def foo2(a : int[])
{
	return = a[2];
}
a3;
a4;
i=[Imperative]
{
	x1 = [ 1, 2, 3, 4 ];
	a = Create(x1);
	a2 = foo(x1);	
	a3 = foo1(x1[0]);
	a4 = foo2(x1);
    return [x1,a,a2,a3,a4];
}";
            thisTest.RunScriptSource(code);
            var x1 = new Object[] { 1.0, 2.0, 3.0, 4.0 };
            thisTest.Verify("i", new object[] {x1, null, x1, 1, 3});
        }

        [Test]
        [Category("SmokeTest")]
        public void Regress_1457885()
        {
            string code = @"
c = 5..7..#1;
a = 0.2..0.3..~0.2;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] c1 = new Object[] { 5 };
            // Object[] c2 = new Object[] {0.2,0.3};
            thisTest.Verify("c", c1, 0);
            //               thisTest.Verify("a",c2, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Regress_1454966()
        {
            string code = @"
import(""FFITarget.dll"");
a1 = [Associative]
{	
	return = ClassFunctionality.ClassFunctionality(1).IntVal;	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Assert.Fail("1460965 - Sprint 18 : Rev 1700 : Design Issue : Accessing properties from collection of class instances using the dot operator should yield a collection of the class properties");
            //thisTest.Verify("a1", new Object[] { new Object[] { 1, 1, 1 }, new Object[] { 2, 2, 2 }, new Object[] { 3, 3, 3 } });
            thisTest.Verify("a1", 1);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void Regress_1454966_2()
        {
            string code = @"
import(""FFITarget.dll"");
def create:A( b )
{
    a = ClassFunctionality.ClassFunctionality(b);
	return = a;
}
x = create(3).IntVal;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 3, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Dot Op")]
        [Category("Failure")]
        public void Regress_1454966_3()
        {
            string errmsg = "MAGN-4092 Replication should not be supported in Imperative scope";
            string src = @"
import(""FFITarget.dll"");
x;  
a1; 
a2;
t1;
t2;
t3;
a3;
[Imperative]
{
	x  = [ 1, 2, 3 ];
	a1 = ArrayMember.Ctor( x ).X;
	a2 = ArrayMember.Ctor( x );
	t1 = a2[0].X[0];
	t2 = a2[1].X[1];
	t3 = a2[2].X[2];
	a3 = a2[0].X[0] + a2[1].X[1] +a2[2].X[2];
	
}";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(src, errmsg);
            Object[] x = new Object[] { 1, 2, 3 };
            Object[][] a1 = new Object[][] { new object[] { 1, 1, 1 }, new object[] { 2, 2, 2 }, new object[] { 3, 3, 3 } };
            thisTest.Verify("x", x);
            thisTest.Verify("a1", a1);
            thisTest.Verify("t1", 1);
            thisTest.Verify("t2", 2);
            thisTest.Verify("t3", 3);
            thisTest.Verify("a3", 6);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void Regress_1454966_4()
        {
            string code = @"
import(""FFITarget.dll"");
 value = ArrayMember.Ctor([1,3,5]);
 value2 = ArrayMember.Ctor(1.3);
 getval= value.X;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] x = new Object[] { 1, 3, 5 };
            object c = null;
            thisTest.Verify("getval", x, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Regress_1454966_5()
        {
            string code = @"
import(""FFITarget.dll"");
value = ArrayMember.Ctor([1,3,5]);
getval= value.X;
getval2= value.X[0];
b=1;
getval3= value.X[b];
b=2;
getval4= value.X[b];
b=-1;
getval5= value.X[b];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] x = new Object[] { 1, 3, 5 };
            //Assert.Fail("1454966 - Sprint15: Rev 720 : [Geometry Porting] Assignment operation where the right had side is Class.Constructor.Property is not working");
            thisTest.Verify("getval", x, 0);
            thisTest.Verify("getval2", 1, 0);
            thisTest.Verify("getval3", 5, 0);
            thisTest.Verify("getval4", 5, 0);
            thisTest.Verify("getval5", 5, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void Regress_1454966_6()
        {
            string code = @"
import(""FFITarget.dll"");

 value = ClassFunctionality.ClassFunctionality(1);
def call:int(b:ClassFunctionality)
{
 getval= b.IntVal;
 return= getval;
 }
c= call(value);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", 1, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void Regress_1454966_7()
        {
            var x = new Object[] { 1.0, 2.0, 3.0 };
            string code = @"
import(""FFITarget.dll"");
a1=[Imperative]
{	
	d = [ 1,2,3 ];	
	val=[0,0,0];
	j = 0;	
	for( i in d )	
	{		
	    val[j]=ClassFunctionality.ClassFunctionality(i).IntVal;
	    j = j + 1;	
	}	
	a1 = val;	
	return a1;
}";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a1", x);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Regress_1454966_10()
        {
            string code = @"
def foo : var[] ( b : int )
{
    return  = [ b, b, b ];
}
t1 = [Imperative]
{
	x = [ 1, 2, 3 ];
	a2 = foo( x );
	return = a2[0][0];
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t1", 1, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void Regress_1456895()
        {
            string code = @"
def foo : int[]( b : int)
{
    return = [ b , b];
}
                
def ret_col (a)
{
    return = a;
}

d = [Associative]
{
    c1 = foo( 3 );
    return = ret_col(c1);
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] x = new Object[] { 3, 3 };
            thisTest.Verify("d", x, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void Regress_1456895_2()
        {
            string code = @"                              
def create : var[]..[]( b : int[]..[])                
{
	return =  b;                
}                                
def ret_col(a: int[]..[])                
{
	return = a[0];                
}
c;
d;
[Associative]
{                
    c = [ 3, 3 ];
	c1 = create( c );                
	d = ret_col(c1);
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] x = new Object[] { 3, 3 };
            thisTest.Verify("c", x, 0);
            thisTest.Verify("d", 3, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void Regress_1456895_3()
        {
            string code = @"
def length ( pts : double[] )
{
	numPts = [Imperative]
	{
		counter = 0;
		for(pt in pts)
		{
			counter = counter + 1;
		}
		return = counter;
	}
	return = numPts;
}  
arr = [0.0,1.0,2.0,3.0];
num = length(arr); 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] arr = new Object[] { 0.0, 1.0, 2.0, 3.0 };
            thisTest.Verify("arr", arr, 0);
            thisTest.Verify("num", 4, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void Regress_1456713()
        {
            string code = @"
a = 2.3;
b = a * 3;
c = 2.32;
d = c * 3;
e1=0.31;
f=3*e1;
g=1.1;
h=g*a;
i=0.99999;
j=2*i;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 2.3, 0);
            thisTest.Verify("b", 6.9, 0);
            thisTest.Verify("c", 2.32, 0);
            thisTest.Verify("d", 6.96, 0);
            thisTest.Verify("e1", 0.31, 0);
            thisTest.Verify("f", 0.93, 0);
            thisTest.Verify("g", 1.1, 0);
            thisTest.Verify("h", 2.53, 0);
            thisTest.Verify("i", 0.99999, 0);
            thisTest.Verify("j", 1.99998, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void Regress_1454511()
        {
            string code = @"
x=[Imperative]
{
	x = 0;
	
	for ( i in b )
	{
		x = x + 1;
	}
    return x;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 0, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void Regress_1456758()
        {
            string code = @"
a = true && true ? -1 : 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", -1, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void Regress_1459175()
        {
            string code = @"
import(""FFITarget.dll"");
p1 = DummyPoint2D.ByCoordinates( 5.0, 10.0);
a1 = p1.create(4.0,5.0);
a2 = a1.X; // expected null here!!
a3 = a1.Y; // expected null here!!
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object expectedResultc = null;
            thisTest.Verify("a1", expectedResultc, 0);
            thisTest.Verify("a2", expectedResultc, 0);
            thisTest.Verify("a3", expectedResultc, 0);
        }
        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        public void Regress_1459175_2()
        {
            Assert.Inconclusive();
            string code = @"
import(""FFITarget.dll"");
def length : ClassFunctionality[] (pts : ClassFunctionality[])
{
    numPts = [Imperative]
    {
        counter = 0;
        for(pt in pts)
        {
            counter = counter + 1;
        }        
        return = counter;
    }
    return = pts;
}
pt1 = ClassFunctionality.ClassFunctionality( 1 );
pt2 = ClassFunctionality.ClassFunctionality( 10 );
pts = [pt1, pt2];
numpts = length(pts);
test = numpts[0].IntVal;
test2 = numpts[1].IntVal;
	
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("test", 1, 0);
            thisTest.Verify("test2", 10, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void Regress_1457903()
        {
            string code = @"
a = 1..7..#2.5;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] {1, 7}, 0);
        }

        [Test]
        [Category("Type System")]
        public void Regress_1454918_1()
        {
            //Assert.Fail("1467156 - Sprint 25 - Rev 3026 type checking of return types at run time ");
            string code = @"
d;
	 def Divide : double (a:int, b:int)
	 {
	  return = a/b;
	 }
[Associative] // expected 2.5
{
	 d = Divide (5,2);
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("d", 2.5);
        }

        [Test]
        [Category("SmokeTest")]
        public void Regress_1454918_2()
        {
            string code = @"
d;
	 def foo : int (a:double)
	 {
		  return = a;
	 }
[Associative] // expected error
{
	 d = foo (5.5);
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("d", 6, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void Regress_1454918_3()
        {
            string code = @"
d;
	 def foo : double (a:double)
	 {
		  return = a;
	 }
[Associative] // expected d = 5.0
{
	 d = foo (5.0);
} ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("d", 5.0, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void Regress_1454918_4()
        {
            string code = @"
d;
	 def foo : double (a:bool)
	 {
		  return = a;
	 }
[Associative] // expected error
{
	 d = foo (true);
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object c = null;
            thisTest.Verify("d", c, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        public void Regress_1454918_5()
        {
            //Assert.Fail("1467156 - Sprint 25 - Rev 3026 type checking of return types at run time ");
            string code = @"
import(""FFITarget.dll"");
def foo : int (a : ClassFunctionality)
{
    return = a;
}

d = [Associative] 
{
	 a1 = ClassFunctionality.ClassFunctionality(1);
	 return = foo (a1);
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object c = null;
            thisTest.Verify("d", c, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void Regress_1454918_6()
        {
            string code = @"
     def foo : double ()
	 {
		  return = 5;
	 }
	 d = foo ();
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("d", 5.0, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void Regress_1456611()
        {
            string code = @"
import(""FFITarget.dll"");
def create( b )
{
    a = ClassFunctionality.ClassFunctionality(b);
	return = a;
}
x = create(3);
y = x.IntVal;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("y", 3, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void Regress_1456611_2()
        {
            string code = @"
import(""FFITarget.dll"");
    def length : int (pts : ClassFunctionality[])
    {
        numPts = [Imperative]
            {
                counter = 0;
                for(pt in pts)
                {
                    counter = counter + 1;
                }        
                return = counter;
            }
        return = numPts;
    }
numpts = [Imperative]
{
    pt1 = ClassFunctionality.ClassFunctionality( 0 );
    pt2 = ClassFunctionality.ClassFunctionality( 10 );
    pts = [pt1, pt2];
    return = length(pts); 
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("numpts", 2, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void Regress_1456611_3()
        {
            string code = @"
// function test -return class array, argument as class array 
import(""FFITarget.dll"");
def length : ClassFunctionality[] (pts : ClassFunctionality[])
{
    numPts = [Imperative]
    {
        counter = 0;
        for(pt in pts)
        {
            counter = counter + 1;
        }        
        return = counter;
    }
    return = pts;
}
pt1 = ClassFunctionality.ClassFunctionality( 0 );
pt2 = ClassFunctionality.ClassFunctionality( 10 );
pts = [pt1, pt2];
numpts = length(pts); // getting null
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void Regress_1456611_4()
        {
            string code = @"
//  function test return int , multiple arguments 
import(""FFITarget.dll"");
def length : int (pts : ClassFunctionality[],num:int)
{
    numPts = [Imperative]
    {
        counter = 0;
        for(pt in pts)
        {
            counter = counter + 1;
        }        
        return = counter;
    }
    return = num;
}
pt1 = ClassFunctionality.ClassFunctionality( 0 );
pt2 = ClassFunctionality.ClassFunctionality( 10 );
pts = [pt1, pt2];
numpts = length(pts,5); // getting null";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("numpts", 5, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void Regress_1456611_5()
        {
            string code = @"
// function test pass an item in hte array as argument , no return type specified
import(""FFITarget.dll"");
def length(pts : ClassFunctionality[],num:int )
{
    numPts = [Imperative]
    {
        counter = 0;
        for(pt in pts)
        {
            counter = counter + 1;
        }        
        return = counter;
    }
    return = num;
}
pt1 = ClassFunctionality.ClassFunctionality( 0 );
pt2 = ClassFunctionality.ClassFunctionality( 10 );
pts = [pt1, pt2];
a=[1,2,3];
numpts = length(pts,a[0]);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("numpts", 1, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void Regress_1456611_6()
        {
            string code = @"
// function test pass an item in the array as argument , no return type specified
import(""FFITarget.dll"");
def length(pts : ClassFunctionality[],num:int )
{
    numPts = [Imperative]
    {
        counter = 0;
        for(pt in pts)
        {
            counter = counter + 1;
        }        
        return = counter;
    }
    return = null;
}
pt1 = ClassFunctionality.ClassFunctionality( 0 );
pt2 = ClassFunctionality.ClassFunctionality( 10 );
pts = [pt1, pt2];
a=[1,2,3];
numpts = length(pts,a[0]);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object c = null;
            thisTest.Verify("numpts", c, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void Regress_1456611_7()
        {
            string code = @"
// no return type specified ad no return statement 
import(""FFITarget.dll"");
def length(pts : ClassFunctionality[],num:int )
{
    numPts = [Imperative]
    {
        counter = 0;
        for(pt in pts)
        {
            counter = counter + 1;
        }        
        return = counter;
    }
//    return = null;
}
pt1 = ClassFunctionality.ClassFunctionality( 0 );
pt2 = ClassFunctionality.ClassFunctionality( 10 );
pts = [pt1, pt2];
a=[1,2,3];
numpts = length(pts,a[0]);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object c = null;
            thisTest.Verify("numpts", c, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Regress_1456611_8()
        {
            //Assert.Fail("Sub-recursion calls with auto promotion on jagged arrays is not working");
            string code = @"
// function test pass an item in the array as argument , no return type specified
import(""FFITarget.dll"");
def length :ClassFunctionality[](pts : ClassFunctionality[])
{
   
    return = pts;
}
def nested(pts:ClassFunctionality[] )
{
    pt1 = ClassFunctionality.ClassFunctionality( 5 );
    pts2=[pts,pt1];
    return =length(pts2);
}
gpt1 = ClassFunctionality.ClassFunctionality( 0 );
gpt2 = ClassFunctionality.ClassFunctionality( 10 );
gpts = [gpt1, gpt2];
a=[1,2,3];
numpts = nested(gpts);
t1 = numpts[0][0].IntVal;
t2 = numpts[0][1].IntVal;
t3 = numpts[1][0].IntVal;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t1", 0, 0);
            thisTest.Verify("t2", 10, 0);
            thisTest.Verify("t3", 5, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        public void Regress_1456611_9()
        {
            //Assert.Fail("DNL-1467208 Auto-upcasting of int -> int[] is not happening on function return");
            string code = @"
// test rank of return type 
import(""FFITarget.dll"");
def length : ClassFunctionality[](pts : ClassFunctionality[])
{
   
    return = pts;
}
def nested:ClassFunctionality[][](pts:ClassFunctionality[] )//return type 2 dimensional
{
    pt1 = ClassFunctionality.ClassFunctionality( 5 );
    pt2 = ClassFunctionality.ClassFunctionality( 5 );
    return =length(pts); // returned array 1 dimensional
}
gpt1 = ClassFunctionality.ClassFunctionality( 0 );
gpt2 = ClassFunctionality.ClassFunctionality( 10 );
gpts = [gpt1, gpt2];
a=[1,2,3];
res = nested(gpts);
numpts=res[0][0].IntVal;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("numpts", 0, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void Regress_1459372()
        {
            string code = @"
collection = [ 2, 2, 2 ];
collection[1] = 3;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] x = new Object[] { 2, 3, 2 };
            thisTest.Verify("collection", x, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void Regress_1459512()
        {
            string code = @"
def length : int (pts : int[])
{
    numPts = [Imperative]
    {
        counter = 0;
        for(pt in pts)
        {
            counter = counter + 1;
        }        
        return = counter;
    }
    return = numPts;
}
z=length([1,2]);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void Regress_1459171_1()
        {
            string code = @"
import(""FFITarget.dll"");
def create( X, Y)
{	    
	return = DummyPoint2D.ByCoordinates( X, Y );		
}	

p1 = DummyPoint2D.ByCoordinates( 5.0, 10.0);
t1 = p1.X;
t2 = p1.Y;
a1 = create(p1.X,p1.Y);
a2 = a1.X;
a3 = a1.Y;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("SmokeTest")]
        public void Regress_1459171_2()
        {
            string code = @"
e1;
	def even : int (a : int) 
	{	
        return = [Imperative]{
		if(( a % 2 ) > 0 )
			return = a + 1;
		
		else 
			return = a;
        }
	}
e1=[Imperative]
{
	x = 1..3..1;
	y = 1..9..2;
	z = 11..19..2;
	c = even(x);
	d = even(x)..even(c)..(even(0)+0.5);
	e1 = even(y)..even(z)..1;
	f = even(e1[0])..even(e1[1]); 
	g = even(y)..even(z)..f[0][1]; 
    return e1;
}
";
            thisTest.RunScriptSource(code);
            object[] e1 = {  new object[] {2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12},
                               new object[] {4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14},
                               new object[] {6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16},
                               new object[] {8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18},
                               new object[] {10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20}
                             };
            thisTest.Verify("e1", e1);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void Regress_1458916()
        {
            string code = @"
import(""FFITarget.dll"");
a1 =ClassFunctionality.ClassFunctionality(5);
x1 = a1.IntVal;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
        }


        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void Regress_1459584()
        {
            string code = @"
import(""FFITarget.dll"");
def length : ClassFunctionality[] (pts : ClassFunctionality[])
{
    numPts = [Imperative]
    {
        counter = 0;
        for(pt in pts)
        {
            counter = counter + 1;
        }        
        return = counter;
    }
    return = pts;
}
pt1 = ClassFunctionality.ClassFunctionality( 0 );
pt2 = ClassFunctionality.ClassFunctionality( 10 );
pts = [pt1, pt2];
numpts = length(pts);
test=numpts[0].IntVal;
test2= numpts[1].IntVal;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] numpts = new object[] { 0, 10 };
            thisTest.Verify("test", 0, 0);
            thisTest.Verify("test2", 10, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        public void Regress_1459584_1()
        {
            string code = @"
//return type class and return an array of class-
import(""FFITarget.dll"");
def length : ClassFunctionality[] (pts : ClassFunctionality[])
{
    [Imperative]
    {
        counter = 0;
        for(pt in pts)
        {
            counter = counter + 1;
        }        
        return = counter;
    }
    return = pts;
}
pt1 = ClassFunctionality.ClassFunctionality( 0 );
pt2 = ClassFunctionality.ClassFunctionality( 10 );
pts = [pt1, pt2];
numpts = length(pts); 
a=numpts[0].IntVal;
b=numpts[1].IntVal;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 0, 0);
            thisTest.Verify("b", 10, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        public void Regress_1459584_2()
        {
            //Assert.Fail("1467196 Sprint 25 - Rev 3216 - [Design Issue] when rank of return type does not match the value returned what is the expected result ");
            string code = @"
//return type class and return an array of class-
import(""FFITarget.dll"");
def length : ClassFunctionality[] (pts : ClassFunctionality[])
{
    numPts = [Imperative]
    {
        counter = 0;
        for(pt in pts)
        {
            counter = counter + 1;
        }        
        return = counter;
    }
    return = pts[0];
}
pt1 = ClassFunctionality.ClassFunctionality( 0 );
pt2 = ClassFunctionality.ClassFunctionality( 10 );
pts = [pt1, pt2];
numpts = length(pts); 
a=numpts[0].IntVal;
b=numpts[1].IntVal;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 0, 0);
            thisTest.Verify("b", null, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Type System")]
        public void Regress_1459584_3()
        {
            //Assert.Fail("1467196 Sprint 25 - Rev 3216 - [Design Issue] when rank of return type does not match the value returned what is the expected result ");
            string code = @"
//return type class and return a double
import(""FFITarget.dll"");
def length : ClassFunctionality (pts : ClassFunctionality[])
{
    numPts = [Imperative]
    {
        counter = 0;
        for(pt in pts)
        {
            counter = counter + 1;
        }        
        return = counter;
    }
    return = 1.0;
}
pt1 = ClassFunctionality.ClassFunctionality( 0 );
pt2 = ClassFunctionality.ClassFunctionality( 10 );
pts = [pt1, pt2];
numpts = length(pts); 
a=numpts[0].IntVal;
b=numpts[1].IntVal;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //   thisTest.Verify("a", 0, 0);
            //  thisTest.Verify("b", 10, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void Regress_1459584_4()
        {
            string code = @"
//return type int and return a double
import(""FFITarget.dll"");
def length : int (pts : ClassFunctionality[])
{
    numPts = [Imperative]
    {
        counter = 0;
        for(pt in pts)
        {
            counter = counter + 1;
        }        
        return = counter;
    }
    return = 1.0;
}
pt1 = ClassFunctionality.ClassFunctionality(0);
pt2 = ClassFunctionality.ClassFunctionality(10);
pts = [pt1, pt2];
numpts = length(pts);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("numpts", 1, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void Regress_1459584_5()
        {
            string code = @"
//return type int and return a double
import(""FFITarget.dll"");
def length : double (pts : ClassFunctionality[])
{
    numPts = [Imperative]
    {
        counter = 0;
        for(pt in pts)
        {
            counter = counter + 1;
        }        
        return = counter;
    }
    return = 1;
}
pt1 = ClassFunctionality.ClassFunctionality(0);
pt2 = ClassFunctionality.ClassFunctionality(10);
pts = [pt1, pt2];
numpts = length(pts); ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("numpts", 1.0, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void Regress_1459584_6()
        {
            string code = @"
//no return type defined
import(""FFITarget.dll"");
def length  (pts : ClassFunctionality[])
{
    numPts = [Imperative]
    {
        counter = 0;
        for(pt in pts)
        {
            counter = counter + 1;
        }        
        return = counter;
    }
    return = pts;
}
pt1 = ClassFunctionality.ClassFunctionality(0);
pt2 = ClassFunctionality.ClassFunctionality(10);
pts = [pt1, pt2];
numpts = length(pts); 
test=numpts[0].IntVal;
test2=numpts[1].IntVal;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] numpts = new object[] { 0, 10 };
            thisTest.Verify("test", 0, 0);
            thisTest.Verify("test2", 10, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void Regress_1459584_7()
        {
            string code = @"
//no return type defined and return null
import(""FFITarget.dll"");
def length  (pts : ClassFunctionality[])
{
    numPts = [Imperative]
    {
        counter = 0;
        for(pt in pts)
        {
            counter = counter + 1;
        }        
        return = counter;
    }
    return = null;
}
pt1 = ClassFunctionality.ClassFunctionality(0);
pt2 = ClassFunctionality.ClassFunctionality(10);
pts = [pt1, pt2];
numpts = length(pts); ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object c = null;
            thisTest.Verify("numpts", c, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void Regress_1459584_8()
        {
            string code = @"
//no return statement
import(""FFITarget.dll"");
def length  (pts : ClassFunctionality[])
{
    numPts = [Imperative]
    {
        counter = 0;
        for(pt in pts)
        {
            counter = counter + 1;
        }        
        return = counter;
    }
   // return = null;
}
pt1 = ClassFunctionality.ClassFunctionality(0);
pt2 = ClassFunctionality.ClassFunctionality(10);
pts = [pt1, pt2];
numpts = length(pts); ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object c = null;
            thisTest.Verify("numpts", c, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void Regress_1458475()
        {
            string code = @"
a = [ 1,2 ];
b1 = a[-1];//b1=2";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b1", 2, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void Regress_1458475_2()
        {
            string code = @"
a = [ [1,2],[3,4,5]];
b1 = a[0][-1];// b1=2";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b1", 2, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void Regress_1458187()
        {
            string code = @"
//b=true;
  //              x = (b == 0) ? b : b+1;
def foo1 ( b  )
{
                x = (b == 0) ? b : b+1;
                return = x;
}
a=foo1(5.0);
b=foo1(5);
c=foo1(0);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 6.0, 0);
            thisTest.Verify("b", 6, 0);
            thisTest.Verify("c", 0, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void Regress_1458187_2()
        {
            string code = @"
def foo1 ( b )
{
x = (b == 0) ? b : b+1;
return = x;
}
a=foo1(true); ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("SmokeTest")]
        public void Regress_1458187_3()
        {
            string code = @"
def foo1 ( b )
{
x = (b == 0) ? b : b+1;
return = x;
}
a=foo1(null); ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object c = null;
            thisTest.Verify("a", c, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void Regress_1454926()
        {
            string code = @"
result;
result2;
[Imperative]
{	 
	 d1 = null;
	 d2 = 0.5;	 
	 result = d1 * d2; 
	 result2 = d1 + d2; 
 
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object result = null;
            thisTest.Verify("result", result, 0);
            thisTest.Verify("result2", result, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void Regress_1459900()
        {
            string code = @"
a:int = 1.3;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void Regress_1459762()
        {
            string code = @"
import(""FFITarget.dll"");
r1 = ClassFunctionality.ClassFunctionality(5);
r2 = r1+1;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object r2 = null;
            thisTest.Verify("r2", r2, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void Regress_1452951()
        {
            string code = @"
x;
[Associative]
{
	a = [ 4,5 ];
   
	x=[Imperative]
	{
	       //a = { 4,5 }; // works fine
		x = 0;
		for( y in a )
		{
			x = x + y;
		}
        return x;
	}
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 9, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void Regress_1457023()
        {
            string code = @"
def length : int (pts : double[])
{
    numPts = [Imperative]
    {
        counter = 0;
        for(pt in pts)
        {
            counter = counter + 1;
        }
            
        return = counter;
    }
    return = numPts;
}
arr = [0.0,1.0,2.0,3.0];
num = length(arr);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] arr = new object[] { 0.0, 1.0, 2.0, 3.0 };
            thisTest.Verify("arr", arr, 0);
            thisTest.Verify("num", 4, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void Regress_1457023_1()
        {
            string code = @"
def length : int (C : double[])
{
    counter = 0;
		
	return [Imperative]
    {          
        for(pt in C)
        {
            counter = counter + 1;
        } 
        return counter;          
    }
    
}
arr = [ 0.0, 1.0, 2.0, 3.0 ];
num = length(arr);
";
            thisTest.RunScriptSource(code);
            var arr = new object[] { 0.0, 1.0, 2.0, 3.0 };
            thisTest.Verify("arr", arr);
            thisTest.Verify("num", 4);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void Regress_1457023_2()
        {
            string code = @"
def add_2:double[](b : double[])
{
	j = 0;
	x = [Imperative]
	{
		for ( i in b )
		{
			b[j] = b[j] + 1;
			j = j + 1 ;
		}
		return = b;
	}
		
	return = x;
}
c = [ 1.0, 2, 3 ];
b2 = add_2(c);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] b2 = new object[] { 2.0, 3.0, 4.0 };
            thisTest.Verify("b2", b2, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void Regress_1457023_4()
        {
            //Assert.Fail("1463372 - Sprint 20 : Rev 2088 : Imperative code is not allowed in class constructor "); 
            string code = @"
	def create: var[]..[](i:int)
	{
		return = [Imperative]
		{
			if( i == 1 )
			{
				return = [ [ 1,2,3 ] , [ 4,5,6 ] ];
			}
			return = [ [ 1,2,3 ] , [ 1,2,3 ] ];	
        }
	}
	
A1 = create(1);
a1 = A1[0][0];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a1", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void myTest()
        {
            string code = @"
def length : int (pts : int[])
{
    numPts = [Imperative]
    {
        counter = 0;
        for(pt in pts)
        {
            counter = counter + 1;
        }        
        return = counter;
    }
    return = numPts;
}
z=length([1,2]);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
        }

        [Test]
        [Category("Type System")]
        public void Regress_1462308()
        {
            string code = @"
import(TestData from ""FFITarget.dll"");
f = TestData.IncrementByte(101); 
F = TestData.ToUpper(f);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("f", 102);
            thisTest.Verify("F", null);
        }

        [Test]
        public void Regress_1467091()
        {
            string code = @"
def foo(x:int)
{
    return =  x + 1;
}
y1 = test.foo(2);
y2 = ding().foo(3);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.IdUnboundIdentifier);
            if (!core.Options.SuppressFunctionResolutionWarning)
            {
                TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.FunctionNotFound);
            }
            thisTest.Verify("y1", null);
            thisTest.Verify("y2", null);
        }

        [Test]
        public void Regress_1467094_1()
        {
            string code = @"
t = [];
x = t[3];
t[2] = 1;
y = t[3];
z = t[-1];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", null);
            thisTest.Verify("y", null);
            thisTest.Verify("z", 1);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Regress_1467094_2()
        {
            string code = @"
import(""FFITarget.dll"");
c = [ ClassFunctionality.ClassFunctionality(0), ClassFunctionality.ClassFunctionality(1) ];
p = [];
d = [Imperative]
{
    if(c[0].IntVal == 0 )
    {
        c[0] = 0;
        p[0] = 0;
    }
    if(c[0].IntVal == 0 )
    {
        p[1] = 1;
    }
    return = 0;
}
t1 = c[0];
t2 = c[1].IntVal;
t3=p[0];
t4=p[1];";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t4", null);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Regress_1467104()
        {
            string code = @"
import(""FFITarget.dll"");
pts = ClassFunctionality.ClassFunctionality( [ 1, 2] );
aa = pts[null].IntVal;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("aa", null);
        }

        [Test]
        public void Regress_1467107()
        {
            string code = @"
def foo(x:int)
{
    return =  x + 1;
}
m=null;
y = m.foo(2);";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("y", null);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.Runtime.WarningID.DereferencingNonPointer);
        }

        [Test]
        public void Regress_1467117()
        {
            string code = @"
/*
/*
*/
a = 1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1);
            thisTest.VerifyBuildWarningCount(0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void Regress_1467318()
        {
            string code = @"
import(""FFITarget.dll"");
a = ArrayMember.Ctor([2, 3]);
t = a.X;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t", new object[] { 2, 3 });
        }
    }
}
