using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;
namespace ProtoTest.TD.MultiLangTests
{
    class CollectionAssignment : ProtoTestBase
    {
        [Test]
        [Category("SmokeTest")]
        public void T01_Simple_1D_Collection_Assignment()
        {
            string code = @"
a;
b;
c;
d;
e;
[Imperative]
{
	a = { {1,2}, {3,4} };
	
	a[1] = {-1,-2,3};
	
	c = a[1][1];
	
	d = a[0];
	
	b = { 1, 2 };
	
	b[0] = {2,2};
	e = b[0];
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedResult2 = { 1, 2 };
            object[] expectedResult3 = { 2, 2 };
            thisTest.Verify("c", -2, 0);
            thisTest.Verify("d", expectedResult2, 0);
            thisTest.Verify("e", expectedResult3, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T02_Collection_Assignment_Associative()
        {
            string code = @"
a;
b;
c;
d;
e;
[Associative]
{
	a = { {1,2}, {3,4} };
	
	a[1] = {-1,-2,3};
	
	c = a[1][1];
	
	d = a[0];
	
	b = { 1, 2 };
	
	b[0] = {2,2};
	e = b[0];
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedResult2 = { 1, 2 };
            object[] expectedResult3 = { 2, 2 };
            thisTest.Verify("c", -2, 0);
            thisTest.Verify("d", expectedResult2, 0);
            thisTest.Verify("e", expectedResult3, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T03_Collection_Assignment_Nested_Block()
        {
            string code = @"
c;
d;
e;
[Associative]
{
	a = { {1,2,3},{4,5,6} };
	
	[Imperative]
	{
		c = a[0];
		d = a[1][2];
	}
	
	e = c;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedResult2 = { 1, 2, 3 };
            thisTest.Verify("c", expectedResult2);
            thisTest.Verify("d", 6);
            thisTest.Verify("e", expectedResult2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T04_Collection_Assignment_Using_Indexed_Values()
        {
            string code = @"
c;
d;
e;
[Associative]
{
	a = { {1,2,3},{4,5,6} };
	
	b = { a[0], 4 };
	
	c = b[0];
	
	d = b[1];
	
	e = { a[0][0], a[0][1], a[1][0] };
	
}
	";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedResult = { 1, 2, 3 };
            object[] expectedResult2 = { 1, 2, 4 };
            thisTest.Verify("c", expectedResult, 0);
            thisTest.Verify("e", expectedResult2, 0);
            thisTest.Verify("d", 4, 0);


        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T05_Collection_Assignment_Using_Class()
        {
            string code = @"
class collection
{
	
	public a : var[];
	
	constructor create( )
	{
		a = { 1,2,3 };
	}
	
	def ret_col ( )
	{
		return=  a;
	}
}
d;
[Imperative]
{
	c1 = collection.create(  );
	d = c1.ret_col();
}
		
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedResult2 = { 1, 2, 3 };
            thisTest.Verify("d", expectedResult2, 0);
        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T06_Collection_Assignment_Using_Class_2()
        {
            string code = @"
class collection
{
	
	public a : var[];
	
	constructor create( b )
	{
		a = { 1,2,b };
	}
	
	def modify ( c )
	{
		a[0] = c;
		return = a;
	}
}
d;
[Associative]
{
	c1 = collection.create( 3 );
	
	d = c1.modify( 4 );
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedResult2 = { 4, 2, 3 };
            thisTest.Verify("d", expectedResult2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T07_Collection_Assignment_In_Function_Scope()
        {
            string code = @"
def collection :int[] ( a :int[] , b:int , c:int )
{
	a[1] = b;
	a[2] = c;
	return= a;
}
	a = { 1,0,0 };
	[Imperative]
	{
		a = collection( a, 2, 3 );
	}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedResult2 = { 1, 2, 3 };
            thisTest.Verify("a", expectedResult2, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T08_Collection_Assignment_In_Function_Scope_2()
        {
            string code = @"
def foo ( a )
{
	return= a;
}
	a = { 1, foo( 2 ) , 3 };
	b;
	[Imperative]
	{
		b = { foo( 4 ), 5, 6 };
	}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedResult2 = { 1, 2, 3 };
            object[] expectedResult = { 4, 5, 6 };
            thisTest.Verify("a", expectedResult2);
            thisTest.Verify("b", expectedResult);
        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T09_2D_Collection_Assignment_In_Class_Scope()
        {
            string code = @"
class coll
{
	a : var[][];
	
	constructor Create ()
	{
		a = { {1,2} , {3,4} };
	}
	
	def ret ()
	{
		return= a;
	}
}
	c1 = coll.Create();
	b = c1.ret();
	c = b[1];
	d;
	[Imperative]
	{	
		c2 = coll.Create();
		b1 = c2.ret();
		d = b1[0];
	}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedResult2 = { 3, 4 };
            object[] expectedResult = { 1, 2 };
            thisTest.Verify("c", expectedResult2);
            thisTest.Verify("d", expectedResult);
        }

        [Test]
        [Category("SmokeTest")]
        public void T10_2D_Collection_Assignment_In_Function_Scope()
        {
            string code = @"
	def foo( a:int[] )
	{
		a[0][0] = 1;
		return= a;
	}
	b = { {0,2,3}, {4,5,6} };
	d = foo( b );
	c = d[0];
		
		
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedResultc = { new object[] { 1 }, 2, 3 };
            object[] expectedResultd = {new object[] {new object[] {1}, 2, 3},
                     
            new object[] {new object[] {1}, 5, 6}
        };
            thisTest.Verify("c", expectedResultc, 0);
            thisTest.Verify("d", expectedResultd, 0);

        }

        [Test]
        [Category("SmokeTest")]
        public void T11_2D_Collection_Assignment_Heterogeneous()
        {
            string code = @"
b;
c;
d;
[Imperative]
{
	a = { {1,2,3}, {4}, {5,6} };
	b = a[1];
	a[1] = 2;
	a[1] = a[1] + 1;
	a[2] = {7,8};
	c = a[1];
	d = a[2][1];
}	";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedResult2 = { 4 };
            thisTest.Verify("b", expectedResult2, 0);
            thisTest.Verify("c", 3, 0);
            thisTest.Verify("d", 8, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T12_Collection_Assignment_Block_Return_Statement()
        {
            string code = @"
c1;
c2;
[Associative]
{
	a = 3;
	
	b = [Imperative]
	{
		c = { 1,2,3 };
		if( c[1] <= 3 )
		return= c;
	}
	
	b[2] = 4;
	a = b;
	c1 = a[1];
	c2 = a[2];
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("c1", 2, 0);
            thisTest.Verify("c2", 4, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T13_2D_Collection_Assignment_Block_Return_Statement()
        {
            string code = @"
c1;
c2;
[Associative]
{
	a = 3;
	
	b = [Imperative]
	{
		c = { { 1,2,3 } , { 4,5,6 } } ;
		return= c;
	}
	
	b[0][0] = 0;
	a = b;
	c1 = a[0];
	c2 = a[1][2];
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedResult2 = { 0, 2, 3 };
            thisTest.Verify("c1", expectedResult2, 0);
            thisTest.Verify("c2", 6, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T14_2D_Collection_Assignment_Using_For_Loop()
        {
            string code = @"
pts = {{0,1,2},{0,1,2}};
x = {1,2};
y = {1,2,3};
[Imperative]
{
    c1 = 0;
	for ( i in x )
	{
		c2 = 0;
		for ( j in y )
		{
		    pts[c1][c2] = i+j;
			c2 = c2+1;
		}
		c1 = c1 + 1;
	}
	
}
p1 = pts[1][1];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("p1", 4, 0);

        }

        [Test]
        [Category("SmokeTest")]
        public void T15_2D_Collection_Assignment_Using_While_Loop()
        {
            string code = @"
p1;
[Imperative]
{
	pts = {{0,1,2},{0,1,2}};
	x = {1,2,3};
	y = {1,2,3};
    i = 0;
	while ( i < 2 )
	{		
		j = 0;
		while ( j  < 3 )
		{
		    pts[i][j] = i+j;
			j = j + 1;
		}
		i = i + 1;
	}
	p1 = pts[1][1];
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("p1", 2, 0);
        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T16_Assigning_Class_Collection_Property()
        {
            string error = "1467321 rev 3878: class property specified as an empty array with no rank is becoming null when assigned a collection to it ";
            string code = @"class A
{
    a = {1,2,3};
}
a = A.A();
val = a.a;
val[0] = 100;
t = a.a[0];         
";
            thisTest.VerifyRunScriptSource(code, error);
            // Assert.Fail("1463456 - Sprint 20 : Rev 2105 : Collection assignment is not always by reference ( when the collection is in class or function ) ");
            thisTest.Verify("t", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T17_Assigning_Collection_And_Updating()
        {
            string code = @"
a = {1, 2, 3};
b = a;
b[0] = 100;
t = a[0];       // t = 100, as expected
      
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T18_Assigning_Collection_In_Function_And_Updating()
        {
            string code = @"
def A (a: int [])
{
    return = a;
}
val = {1,2,3};
b = A(val);
t = b;
t[0] = 100;    // 
y = b[0];
z = val[0];    // val[0] is still 1
      
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            // Assert.Fail("1463456 - Sprint 20 : Rev 2105 : Collection assignment is not always by reference ( when the collection is in class or function ) ");
            thisTest.Verify("z", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T19_Assigning_Collection_In_Function_And_Updating()
        {
            string code = @"
def A (a: int [])
{
    return = a;
}
val = {1,2,3};
b = A(val);
b[0] = 100;     
z = val[0];     
      
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            // Assert.Fail("1463456 - Sprint 20 : Rev 2105 : Collection assignment is not always by reference ( when the collection is in class or function ) ");
            thisTest.Verify("z", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T20_Defect_1458567()
        {
            string code = @"
a = 1;
b = a[1];";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object v = null;

            thisTest.Verify("a", 1);
            thisTest.Verify("b", v);
        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T20_Defect_1458567_2()
        {
            string code = @"
class Point
{
    X : double;
	Y : double;
	Z : double;
	
	constructor ByCoordinates( x : double, y : double, z : double )
	{
	    X = x;
		Y = y;
		Z = z;		
	}
}
class Line
{
    P1 : Point;
	P2 : Point;
	
	constructor ByStartPointEndPoint( p1 : Point, p2 : Point )
	{
	    P1 = p1;
		P2 = p2;
	}
	
	def PointAtParameter (p : double )
	{
	
	    t1 = P1.X + ( p * (P2.X - P1.X) );
		return = Point.ByCoordinates( t1, P1.Y, P1.Z);
	    
	}
	
}
startPt = Point.ByCoordinates(1, 1, 0);
endPt   = Point.ByCoordinates(1, 5, 0);
line_0  = Line.ByStartPointEndPoint(startPt, endPt); 	
x1 = line_0[10].P1.X;
x2 = line_0[0].P1.X;
x3 = line_0.P1.X;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object v = null;
            thisTest.Verify("x1", v);
            thisTest.Verify("x2", v);
            thisTest.Verify("x3", 1.0);
        }

        [Test]
        [Category("SmokeTest")]
        public void T21_Defect_1460891()
        {
            string code = @"
a;
c;
[Imperative]
{
    b = { };
    count = 0;
    a = 1..5..2;
    for ( i in a )
    {
        b[count] = i + 1;
        count = count + 1;
    }
	c = b ;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 1, 3, 5 };
            Object[] v2 = new Object[] { 2, 4, 6 };
            thisTest.Verify("a", v1);
            thisTest.Verify("c", v2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T22_Create_Multi_Dim_Dynamic_Array()
        {
            string code = @"
test;
[Imperative]
{
    d = {{}};
    r = c = 0;
    a = { 0, 1, 2 };
	b = { 3, 4, 5 };
    for ( i in a )
    {
        c = 0;
		for ( j in b)
		{
		    d[r][c] = i + j;
			c = c + 1;
        }
		r = r + 1;
    }
	test = d;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { new Object[] { 3, 4, 5 }, new Object[] { 4, 5, 6 }, new Object[] { 5, 6, 7 } };

            thisTest.Verify("test", v1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T21_Defect_1460891_2()
        {
            // Assert.Fail("1464429 - Sprint 21 : rev 2205 : Dynamic array cannot be used in associative scope ");

            string code = @"
def CreateArray ( x : var[] , i )
{
    x[i] = i;
	return = x;
}
b = {0, 1};
count = 0..1;
b = CreateArray ( b, count );
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { new object[] { 0, 1 }, new object[] { 0, 1 } };

            thisTest.Verify("b", v1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T23_Create_Dynamic_Array_Using_Replication_In_Imperative_Scope()
        {
            string code = @"
test;
[Imperative]
{
	def CreateArray ( x : var[] , i )
	{
		x[i] = i;
		return = x;
	}
	test = { };
	test = CreateArray ( test, 0 );
	test = CreateArray ( test, 1 );
	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            Object[] v1 = new Object[] { 0, 1 };
            thisTest.Verify("test", v1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T24_Dynamic_Array_Imperative_Scope()
        {
            string code = @"
t = [Imperative]
{
    d = { { } };
    r = c = 0;
    a = { 0, 1, 2 };
    b = { 3, 4, 5 };
    for ( i in a )
    {
        c = 0;
	for ( j in b)
	{
	    d[r][c] = i + j;
	    c = c + 1;
        }
	r = r + 1;
    }
    test = d;
    return = test;
}
// expected : test = { { 3, 4, 5 }, {4, 5, 6}, {5, 6, 7} }
// received : test = { { 3, 4, 5 }, , }";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { new Object[] { 3, 4, 5 }, new Object[] { 4, 5, 6 }, new Object[] { 5, 6, 7 } };
            thisTest.Verify("t", v1);
        }


        [Test]
        [Category("SmokeTest")]
        public void T24_Dynamic_Array_Imperative_Function_Scope()
        {
            string code = @"
def createArray( p : int[] )
{  
    a = [Imperative]  
    {    
        collection = {};	
	lineCnt = 0;
	while ( lineCnt < 2 )
	{
            collection [ lineCnt ] = p [ lineCnt ] * -1;
	    lineCnt = lineCnt + 1;      
	}
	return = collection;
    }
    return = a;
}
x = createArray ( { 1, 2, 3, 4 } );
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { -1, -2 };
            thisTest.Verify("x", v1);
        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T24_Dynamic_Array_Accessing_Out_Of_Bound_Index()
        {
            // Assert.Fail("1465614 - Sprint 21: rev 2335 : Accessing out-of-bound index os dynamic array is throwing unexpected error ");

            string code = @"
class A
{
    Y : double;
    constructor A( y : double)
    {
        Y = y;
    }
}
class B
{
    A1 : A;
    A2 : A;
    constructor B( a1 : A, a2 : A)
    {
        A1 = a1;
        A2 = a2;
    }
}
def foo ( x : double)
{
    return = x + 1;
}
innerCircle2Rad = 100;
basePoint = {  };
basePoint2 = { };
nsides = 4;
a = 0..nsides - 1..1;
b = 0..nsides - 1..2;
collection = { };
[Imperative]
{
    temp1 = {  };
    temp2 = {  };
    for(i in a)
    {
        basePoint[i] = A.A( innerCircle2Rad * foo(i * 360 / nsides) );
        temp1[i] = basePoint[i].Y;
    }
    for(i in a)
    {
        if(i <= nsides-2)
        {
            basePoint2[i] = B.B(basePoint[i], basePoint[i+1]);        
            temp2[i] = { basePoint2[i].A1.Y, basePoint2[i].A2.Y };                      
        }
        basePoint2[nsides-1] = B.B(basePoint[nsides-1], basePoint[0]);
    }
    collection = { temp1, temp2 };
}   
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            // Object v1 = null;
            // thisTest.Verify("test1", 1);
            // thisTest.Verify("test2", v1);
        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T24_Dynamic_Array_Class_Scope()
        {
            string code = @"
class A
{
    X : var[];
    Y : var[];
    Count1;
    
    constructor A ( i : int )
    {
        X = 0..i;
	[Imperative]
	{
	    Count1 = 0;	    
	    y = {};
	    for ( i in X ) 
	    {
	        y[Count1] = i * -1;
		Count1 = Count1+1;
	    }          
            Y = y;	    
	}
	
    }
}
p = 3;
a = A.A(p);
b1 = a.X;
b2 = a.Y;
b3 = a.Count1;
p = 4;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 0, 1, 2, 3, 4 };
            Object[] v2 = new Object[] { 0, -1, -2, -3, -4 };

            thisTest.Verify("b1", v1);
            thisTest.Verify("b2", v2);
        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T24_Dynamic_Array_Class_Scope_2()
        {
            // Assert.Fail("1465637 - Sprint 22 : rev 2336 : Issue with populating multiple array properties of class using imperative block ");
            string error = "1467321 rev 3878: class property specified as an empty array with no rank is becoming null when assigned a collection to it ";
            string code = @"class A
{
    X = { };
    Y = { };
    Count1 :int;
    
    constructor A ( i : int )
    {
        X = 0..i;
	[Imperative]
	{
	    Count1 = 0;	    
	    
	    for ( i in X ) 
	    {
	        Y[Count1] = i * -1;
		Count1 = Count1+1;
	    }          
            	    
	}
	
    }
}
p = 4;
a = A.A(p);
b1 = a.X;
b2 = a.Y;
b3 = a.Count1;
//p = 4;
";
            thisTest.VerifyRunScriptSource(code, error);
            Object[] v1 = new Object[] { 0, 1, 2, 3, 4 };
            Object[] v2 = new Object[] { 0, -1, -2, -3, -4 };
            thisTest.Verify("b1", v1);
            thisTest.Verify("b2", v2);
        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T24_Dynamic_Array_Class_Scope_3()
        {
            // Assert.Fail("1465704 - Sprint 22: rev 2346 : Adding elements to array from inside class methods is throwing System.IndexOutOfRangeException exception ");
            //Assert.Fail("1467194 - Sprint 25 - rev Regressions created by array copy constructions");           
            string errmsg = "";//1467187 - Sprint24: REGRESSION : rev 3177: When a class collection property is updated, the value if not reflected";
            string code = @"class A
{
    X :int[] = { };
    Y :int[] = { };
    Count1 :int;
    
    constructor A ( i : int )
    {
        X = 0..i;
	Y = i..0..-1;
	Count1 = i+1;
    }
    
    def update ( )
    {
        [Imperative]
	{
	    i = 0;
	    while  ( i < Count1 )
	    {
	        temp = Y[i];
		Y[i] = X[i];
		X[i] = temp;
		i = i + 1;
	    }    
	    X[Count1] = 100;
	    Y[Count1] = 100;
	    Count1 = Count1 + 1;
	}
	return = true;
    }
}
p = 4;
a = A.A(p);
b1 = a.X;
b2 = a.Y;
b3 = a.Count1;
test = a.update();
c1 = a.X;
c2 = a.Y;
c3 = a.Count1;
test = b1;
";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] v1 = new Object[] { 4, 3, 2, 1, 0, 100 };
            Object[] v2 = new Object[] { 0, 1, 2, 3, 4, 100 };
            thisTest.Verify("b1", v1);
            thisTest.Verify("b2", v2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T24_Dynamic_Array_Inside_Function()
        {
            string code = @"
def foo ( d : var[] )
{
    [Imperative]
    {
	r = c = 0;
	a = { 0, 1, 2 };
	b1 = { 3, 4, 5 };
	for ( i in a )
	{
	    c = 0;
	    for ( j in b1)
	    {
		d[r][c] = i + j;
		c = c + 1;
	    }
	    r = r + 1;
	}	
    }
    return = d;
}
b = {};
b = foo ( b ) ;     
a = b;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { new Object[] { 3, 4, 5 }, new Object[] { 4, 5, 6 }, new Object[] { 5, 6, 7 } };
            thisTest.Verify("a", v1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T24_Dynamic_Array_Inside_Function_2()
        {
            string code = @"
def foo ( d : var[]..[] )
{
    [Imperative]
    {
	r = c = 0;
	a = { 0, 1, 2 };
	b1 = { 3, 4, 5 };
	for ( i in a )
	{
	    c = 0;
	    for ( j in b1)
	    {
		d[r][c] = i + j;
		c = c + 1;
	    }
	    r = r + 1;
	}	
    }
    return = d;
}
b = { {} };
b = foo ( b ) ;     
a = b;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { new Object[] { 3, 4, 5 }, new Object[] { 4, 5, 6 }, new Object[] { 5, 6, 7 } };
            thisTest.Verify("a", v1);
        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T24_Dynamic_Array_Passed_As_Int_Array_To_Class_Method()
        {
            // Assert.Fail("1465802 - Sprint 22: rev 2359 : Dynamic array issue : when a dynamic array is passed as a collection method/function it throws unknown datatype invalid exception");
            string code = @"
class A
{
                constructor A()
                {}
                def  foo : int(i : int[])
                {
                                return  = i[0] + i[1];
                }
}
b1;b2;b3;
[Associative]
{
                cy={};
                cy[0]=10;
                cy[1]=12;
                a=cy;
                d={cy[0],cy[1]};
                aa = A.A();              
                b1=aa.foo(d);//works
                b2=aa.foo(a); //does not work � error: Unknown Datatype Invalid
                b3=aa.foo(cy); //does not work � error: Unknown Datatype Invalid
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b1", 22);
            thisTest.Verify("b2", 22);
            thisTest.Verify("b3", 22);
        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T24_Dynamic_Array_Passed_As_Primitive_Array_To_Function()
        {
            // Assert.Fail("1465802 - Sprint 22: rev 2359 : Dynamic array issue : when a dynamic array is passed as a collection method/function it throws unknown datatype invalid exception");
            string code = @"
class A
{
                constructor A()
                {}
                def  foo : double(i : var[])
                {
                                return  = i[0] + i[1];
                }
}
b1;b2;b3;
[Associative]
{
                cy={};
                cy[0]=1;
                cy[1]=1.5;
                a=cy;
                d={cy[0],cy[1]};
                aa = A.A();              
                b1=aa.foo(d);//works
                b2=aa.foo(a); //does not work � error: Unknown Datatype Invalid
                b3=aa.foo(cy); //does not work � error: Unknown Datatype Invalid
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b1", 2.5);
            thisTest.Verify("b2", 2.5);
            thisTest.Verify("b3", 2.5);
        }

        [Test]
        [Category("SmokeTest")]
        public void T24_Dynamic_Array_Argument_Function_1465802_1()
        {
            string code = @"
	def foo : int(i:int[])
	{
		return = 1;
	}
b1;
[Associative]
{
cy={};
cy[0]=10;
cy[1]=12;
b1=foo(cy);
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b1", 1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T24_Dynamic_Array_Argument_Function_1465802_2()
        {
            string code = @"
	def foo : int(i:int[])
	{
		return = 1;
	}
b1;
[Associative]
{
cy={};
cy[0]=10;
cy[1]=null;
b1=foo(cy);
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b1", 1);

        }

        /*   
[Test]
           public void T24_Dynamic_Array_Function_1465706()
           {
               ExecutionMirror mirror = thisTest.RunScript(filePath, "T24_Dynamic_Array_Function_1465706.ds");
               Object[] test = new Object[] { 0, 1 };
               thisTest.Verify("test", test);
        
           }*/



        [Test]
        [Category("SmokeTest")]
        public void T25_Adding_Elements_To_Array()
        {
            string code = @"
a = 0..2;
a[3] = 3;
b = a;
x = { { 0, 0 } , { 1, 1 } };
x[1][2] = 1;
x[2] = {2,2,2,2};
y = x;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 0, 1, 2, 3 };
            Object[] v2 = new Object[] { 0, 0 };
            Object[] v3 = new Object[] { 1, 1, 1 };
            Object[] v4 = new Object[] { 2, 2, 2, 2 };
            Object[] v5 = new Object[] { v2, v3, v4 };
            thisTest.Verify("b", v1);
            thisTest.Verify("y", v5);

        }

        [Test]
        [Category("SmokeTest")]
        public void T25_Adding_Elements_To_Array_Function()
        {
            string code = @"
def add ( x : var[]..[] ) 
{
    x[1][2] = 1;
    x[2] = { 2, 2, 2, 2 };
    return = x;
}
x = { { 0, 0 } , { 1, 1 } };
x = add(x);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v2 = new Object[] { 0, 0 };
            Object[] v3 = new Object[] { 1, 1, 1 };
            Object[] v4 = new Object[] { 2, 2, 2, 2 };
            Object[] v5 = new Object[] { v2, v3, v4 };
            thisTest.Verify("x", v5);
        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T25_Adding_Elements_To_Array_Class()
        {
            // Assert.Fail("1465704 - Sprint 22: rev 2346 : Adding elements to array from inside class methods is throwing System.IndexOutOfRangeException exception ");

            string code = @"
class A
{
    x : var[][];
    constructor A (  )
    {
        x = { { 0, 0 } , { 1, 1 } };
    }
    def add ( ) 
    {
	x[1][2] = 1;
	x[2] = { 2, 2, 2, 2 };
	return = x;
    }
}
y = A.A();
x = y.add(); // expected { { 0,0 }, { 1, 1, 1 }, {2, 2, 2, 2} }
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v2 = new Object[] { 0, 0 };
            Object[] v3 = new Object[] { 1, 1, 1 };
            Object[] v4 = new Object[] { 2, 2, 2, 2 };
            Object[] v5 = new Object[] { v2, v3, v4 };
            thisTest.Verify("x", v5);
        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T25_Adding_elements_tomemberofclass_1465704()
        {

            string code = @"
class A
{
x : var[][];
constructor A ( )
{
x = { { 0, 0 } , { 1, 1 } };
}
def add ( )
{
x[1][2] = 1;
x[2] = { 2, 2, 2, 2 };
return = x;
}
}
y = A.A();
x = y.add(); //x = {{0,0},{1,1,1},{2,2,2,2}}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] a2 = new Object[] { 0, 0 };
            Object[] a3 = new Object[] { 1, 1, 1 };
            Object[] a4 = new Object[] { 2, 2, 2, 2 };
            Object[] a5 = new Object[] { a2, a3, a4 };
            thisTest.Verify("x", a5);
        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T25_Adding_elements_tomemberofclass_1465704_2()
        {
            string code = @"
class A
{
x : var[][];
constructor A ( )
{
x = { { 0, 0 } , { 1, 1 } };
}
}
def add ( a:A)
{
a.x[1][2] = 1;
a.x[2] = { 2, 2, 2, 2 };
return = a.x;
}
y = A.A();
x = add(y); // expected { { 0,0 }, { 1, 1, 1 }, {2, 2, 2, 2} }
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v2 = new Object[] { 0, 0 };
            Object[] v3 = new Object[] { 1, 1, 1 };
            Object[] v4 = new Object[] { 2, 2, 2, 2 };
            Object[] v5 = new Object[] { v2, v3, v4 };
            thisTest.Verify("x", v5);
        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T25_Adding_elements_tomemberofclass_1465704_3()
        {
            string code = @"
class A
{
    x : var[]..[];
    constructor A ( )
    {
       x = { { 0, 0 } , { 1, 1 } };
    }
    
    def add ( )
    {
        x[1][2] = 1;
        x[2] = { 2, false, { 2, 2 } };
        
        return = x;
    }
}
    
y = A.A();
x = y.add(); // expected { { 0,0 }, { 1, 1, 1 }, {2, false, {2, 2}} }
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v2 = new Object[] { 0, 0 };
            Object[] v3 = new Object[] { 1, 1, 1 };
            Object[] v4 = new Object[] { 2, false, new Object[] { 2, 2 } };
            Object[] v5 = new Object[] { v2, v3, v4 };
            thisTest.Verify("x", v5);
        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T25_Adding_elements_tomemberofclass_1465704_4()
        {
            string code = @"
class A
{
x : var[]..[];
a: var[]..[];
constructor A ( )
{
x = { { 0, 0 } , { 1, 1 } };
}
def add ( )
{
x[1][2] = 1;
x[2] = { 2, false,{ 2, 2} };
return = x;
}
def test( )
{
a = x;
a[3]=1;
return = a;
}
}
y = A.A();
x = y.add(); 
z=y.test();//z = {{0,0},{1,1,1},{2,false,{2,2}},1}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v2 = new Object[] { 0, 0 };
            Object[] v3 = new Object[] { 1, 1, 1 };
            Object[] v4 = new Object[] { 2, false, new Object[] { 2, 2 } };
            Object[] v5 = new Object[] { v2, v3, v4, 1 };
            thisTest.Verify("x", new object[] { v2, v3, v4 });
            thisTest.Verify("z", v5);
        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T25_Adding_elements_tomemberofclass_1465704_5()
        {
            string code = @"
class A
{
x : var[][];
a: var;
constructor A ( )
{
x = { { 0, 0 } , { 1, 1 } };
}
def remove ( )
{
x=Remove(x,1);
return = x;
}
def add( )
{
x[1] = {4,4};
return = x;
}
}
y = A.A();
x = y.remove(); //x = {{0,0},{4,4}}
z=y.add();//z = {{0,0},{4,4}}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 0, 0 };
            Object[] v2 = new Object[] { 4, 4 };
            Object[] v3 = new Object[] { v1, v2 };
            thisTest.Verify("x", new object[] { v1 });
            thisTest.Verify("z", v3);
        }



        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T25_Adding_elements_tomemberofclass_1465704_6()
        {
            string code = @"
class A
{
x : var[]..[];
constructor A ( )
{
x = { { 0, 0 } , { 1, 1 } };
}
}
class B extends A 
{
def add ( )
{
x[1][2] = 1;
x[2] = { 2, false,{ 2, 2} };
return = x;
}
}
y = B.B();
x = y.add(); // expected { { 0,0 }, { 1, 1, 1 }, {2, false, {2, 2}} }";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v2 = new Object[] { 0, 0 };
            Object[] v3 = new Object[] { 1, 1, 1 };
            Object[] v4 = new Object[] { 2, false, new Object[] { 2, 2 } };
            Object[] v5 = new Object[] { v2, v3, v4 };
            thisTest.Verify("x", v5);
        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T25_Adding_elements_tomemberofclass_1465704_7()
        {
            string code = @"
class A
{
x : var[]..[];
constructor A ( )
{
x = { { 0, 0 } , { 1, 1 } };
}
}
y = A.A();
 // expected { { 0,0 }, { 1, 1, 1 }, {2, false, {2, 2}} }
x=[Imperative]
{
def add ( )
{
y.x[1][2] = 1;
y.x[2] = { 2, false,{ 2, 2} };
return = y.x;
}
z = add();
return=z;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v2 = new Object[] { 0, 0 };
            Object[] v3 = new Object[] { 1, 1, 1 };
            Object[] v4 = new Object[] { 2, false, new Object[] { 2, 2 } };
            Object[] v5 = new Object[] { v2, v3, v4 };
            thisTest.Verify("x", v5);
        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T25_Adding_elements_MemberClass_imperative_1465704_8()
        {
            string code = @"
class A
{
x : var[];
constructor A ( )
{
x = { { 0, 0 } , { 1, 1 } };
}
}
y = A.A();
 // expected { { 0,0 }, { 1, 1, 1 }, {2, false, {2, 2}} }
a=[Imperative]
{
def add ( )
{
z=0..5;
for(i in z)
{
	y.x[i] = 1;
}
return = y.x; 
}
y = add();
return=y;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] a = new Object[] { 1, 1, 1, 1, 1, 1 };
            thisTest.Verify("a", a);
        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T25_Adding_elements_MemberClass_imperative_1465704_9()
        {
            string error = "1467309 rev 3786 : Warning:Couldn't decide which function to execute... coming from valid code ";
            string code = @"class A
{
x : var[];
constructor A ( )
{
x = { { 0, 0 } , { 1, 1 } };
}
}
y = A.A();
 // expected { { 0,0 }, { 1, 1, 1 }, {2, false, {2, 2}} }
a=[Imperative]
{
def add ( )
{
z=5;
j=0;
while ( j<=z)
{
	y.x[j] = 1;;
j=j+1;
}
return = y.x; 
}
y = add();
return=y;
}";
            thisTest.VerifyRunScriptSource(code, error);
            Object[] a = new Object[] { 1, 1, 1, 1, 1, 1 };
            thisTest.Verify("a", a);
        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T25_Adding_elements_MemberClass_imperative_1465704_10()
        {
            string code = @"
class A
{
x : var[][];
constructor A ( )
{
x = { { 0, 0 } , { 1, 1 } };
}
}
y = A.A();
 // expected { { 0,0 }, { 1, 1, 1 }, {2, false, {2, 2}} }
x=[Imperative]
{
def add ( )
{
y.x[1][2] = 1;
y.x[2] = { null, false,{ 2, 2} };
return = y.x;
}
z = add();
return=z;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v2 = new Object[] { 0, 0 };
            Object[] v3 = new Object[] { 1, 1, 1 };
            Object[] v4 = new Object[] { null, false, null };
            Object[] v5 = new Object[] { v2, v3, v4 };
            thisTest.Verify("x", v5);
        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T25_Class_Assignment_dynamic_imperative_1465637_1()
        {
            string code = @"
class A
{
X:var;
Y:var;
Count1 :int;
constructor A ( i : int )
	{
	X = 0..i;
	[Imperative]
	{
		Y = {0,0,0,0,0};
		Count1 = 0; 
		for ( i in X ) {
			Y[Count1] = i * -1;
			Count1 = Count1 + 1;
		}
	}
}
}
p = 4;
a = A.A(p);
b1 = a.X;
 // expected { 0, 1, 2, 3, 4 }
b2 = a.Y;
 // expected {0,-1,-2,-3,-4}
b3 = a.Count1;
//received : //watch:
 b1 = {0,-1,-2,-3,-4};//watch: 
b2 = {0,0,0,0,0};
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] b1 = new Object[] { 0, -1, -2, -3, -4 };
            Object[] b2 = new Object[] { 0, 0, 0, 0, 0 };
            thisTest.Verify("b1", b1);
            thisTest.Verify("b2", b2);
        }


        [Test]
        [Category("SmokeTest")]
        public void T26_Defct_DNL_1459616()
        {
            string code = @"
a=1;
a={a,2};";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 1, 2 };
            thisTest.Verify("a", v1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T26_Defct_DNL_1459616_2()
        {
            string code = @"
a={1,2};
[Imperative]
{
    a={a,2};
}
b = a;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { new Object[] { 1, 2 }, 2 };
            thisTest.Verify("b", v1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T26_Defct_DNL_1459616_3()
        {
            string code = @"
a={1,2};
[Imperative]
{
    a={a,2};
}
b = { 1, 2 };
def foo ( )
{
    b =  { b[1], b[1] };
    return = null;
}
dummy = foo ();
c = b;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 2, 2 };
            thisTest.Verify("c", v1);
        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T26_Defct_DNL_1459616_4()
        {
            string code = @"
class A
{
    x : var[]..[];
    constructor A ()
    {
        a = { a, a };
        x = a;	
    }
    def foo ()
    {
        b = { b[0], b[0], b };
	return = b;
    }
}
//a={1,2};
x1 = A.A();
c = [Imperative]
{
    //b = { 0, 1 };
    t1 = x1.x;
    t2  = x1.foo();
    return = { t1, t2 };
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { null, null };
            Object[] v2 = new Object[] { null, null, null };
            Object[] v3 = new Object[] { v1, v2 };
            thisTest.Verify("c", v3);
        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("Variable resolution")]
        [Category("Failure")]
        public void T26_Defct_DNL_1459616_5()
        {
            string error = "MAGN-1511 Sprint 22 : rev 2362 : [Design Issue ]Global variables cannot be accessed from class scope";
            string code = @"class A
{
    x : var[]..[];
    constructor A ()
    {
        a = { a, a };
        x = a;	
    }
    def foo ()
    {
        b = { b[0], b[0], b };
	return = b;
    }
}
a={1,2};
x1 = A.A();
c = [Imperative]
{
    b = { 0, 1 };
    t1 = x1.x;
    t2  = x1.foo();
    return = { t1, t2 };
}
";
            thisTest.VerifyRunScriptSource(code, error);
            Object[] v1 = new Object[] { new Object[] { 1, 2 }, new Object[] { 1, 2 } };
            Object[] v2 = new Object[] { 0, 0, new Object[] { 0, 1 } };
            Object[] v3 = new Object[] { v1, v2 };
            thisTest.Verify("c", v3);
        }

        [Test]
        [Category("SmokeTest")]
        public void T27_defect_1464429_DynamicArray()
        {
            string code = @"
def CreateArray ( x : var[] , i )
{
x[i] = i;
return = x;
}
b = { }; // Note : b = { 0, 0} works fine
count = 0..1;
t2 = CreateArray ( b, count );
t1=b;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            Object[] t1 = new Object[] { };
            Object[] t2 = new Object[] { new Object[] { 0 }, new Object[] { null, 1 } };

            thisTest.Verify("t1", t1);
            thisTest.Verify("t2", t2);
        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T27_defect_1464429_DynamicArray_inline()
        {
            string code = @"
class test
{
	def CreateArray ( x : var[] , i )
	{
		smallest1  =  i>1?i*i:i;
		x[i] = smallest1;
		return = x;
	}
}
b = { }; // Note : b = { 0, 0} works fine
count = 0..2;
a= test.test();
t2 = a.CreateArray( b, count );
t1=b;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] t1 = new Object[] { };
            Object n1 = null;
            Object[] t2 = new Object[] { new Object[] { 0 }, new Object[] { n1, 1 }, new Object[] { n1, n1, 4 } };
            thisTest.Verify("t1", t1);
            thisTest.Verify("t2", t2);
        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T27_defect_1464429_DynamicArray_class()
        {
            string code = @"
class test
{
	def CreateArray ( x : var[] , i )
	{
		smallest1  =  i>1?i*i:i;
		x[i] = smallest1;
		return = x;
	}
}
b = { }; // Note : b = { 0, 0} works fine
count = 0..2;
a= test.test();
t2 = a.CreateArray( b, count );
t1=b;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] t1 = new Object[] { };
            Object n1 = null;
            Object[] t2 = new Object[] { new Object[] { 0 }, new Object[] { n1, 1 }, new Object[] { n1, n1, 4 } };
            thisTest.Verify("t1", t1);
            thisTest.Verify("t2", t2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T27_defect_1464429_DynamicArray_update()
        {
            //Assert.Fail("1467234 - Sprint25: rev 3408 : Adding to collections using negative indices is not working"); 
            string code = @"
def CreateArray ( x : var[] , i )
{
x[i] = i;
return = x;
}
b = { }; // Note : b = { 0, 0} works fine
count = 0..1;
t2 = CreateArray ( b, count );
t1=b;
count = -2..-1;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            Object[] t1 = new Object[] { };
            Object n1 = null;
            Object[] t2 = new Object[] { new Object[] { -2, n1 }, new Object[] { -1 } };
            thisTest.Verify("t1", t1);
            thisTest.Verify("t2", t2);
        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T27_defect_1464429_DynamicArray_memberof_class()
        {
            string code = @"
class test
{
y ={};
	def CreateArray (  i :int)
	{
		y[i] = i;
		return = y;
	}
}
count = 0..2;
a= test.test();
t2 = a.CreateArray(  count );";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] count = new Object[] { 0, 1, 2 };
            Object[] t2 = new Object[] { new Object[] { 0 }, new Object[] { 0, 1 }, new Object[] { 0, 1, 2 } };
            thisTest.Verify("count", count);
            thisTest.Verify("t2", t2);
        }


        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T27_defect_1464429_DynamicArray_class_inherit()
        {

            string errmsg = " 1467318 - Cannot return an array from a function whose return type is var with undefined rank (-2) ";
            string code = @"class test
{
    y ={};
	def CreateArray (  i :int)
	{
		y[1] = i;
		return = y;
	}
}
class test1 extends test
{
   def CreateArray (  i :int)
	{
		y[i] = i*-1;
		return = y;
	}
}
count = 0..2;
a= test1.test1();
t2 = a.CreateArray(  count );
";
            thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] count = new Object[] { 0, 1, 2 };
            Object[] t2 = new Object[] { new Object[] { 0 }, new Object[] { 0, -1 }, new Object[] { 0, -1, -2 } };
            thisTest.Verify("count", count);
            thisTest.Verify("t2", t2);
        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T27_DynamicArray_Class_1465802_Argument()//not
        {
            string code = @"
class A
{
	constructor A()
	{
}
	def foo : int(i:int[])
	{
		return = i[0] + i[1];
	}
}
b1;b2;b31;
[Associative]
{
cy={};
cy[0]=10;
cy[1]=12;
a=cy;
d={cy[0],cy[1]};
aa = A.A();
b1=aa.foo(cy);
b2=aa.foo(d);
b31=aa.foo(a);
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            int b = 22;

            thisTest.Verify("b1", b);
            thisTest.Verify("b2", b);
            thisTest.Verify("b31", b);
        }

        [Category("DSDefinedClass")]
        public void T27_DynamicArray_Class_1465802_Argument_2()
        {
            string code = @"
class A
{
	constructor A()
	{
}
	def foo : int(i:int[])
	{
		return = 1;
	}
}
[Associative]
{
cy={};
cy[0]=10;
cy[1]=null;
aa = A.A();
b1=aa.foo(cy);
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            int b = 1;
            thisTest.Verify("b1", b);
        }


        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T27_DynamicArray_Class_1465802_member()//not
        {
            string code = @"
class A
{
i:int[];
	constructor A(d:int[])
	{
i=d;
}
	def foo : int()
	{
		return = i[0] + i[1];
	}
}
a1;b1;c1;
[Associative]
{
cy={};
cy[0]=10;
cy[1]=12;
a=cy;
d={cy[0],cy[1]};
aa = A.A(cy);
bb = A.A(d);
cc = A.A(a);
a1=aa.foo();
b1=bb.foo();
c1=cc.foo();
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            int b = 22;

            thisTest.Verify("a1", b);
            thisTest.Verify("b1", b);
            thisTest.Verify("c1", b);
        }

        [Test]
        public void T27_DynamicArray_Invalid_Index_1465614_1()
        {
            string code = @"
a={};
b=a[2];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object b = null;
            thisTest.Verify("b", b);
        }

        [Test]
        [Category("DSDefinedClass")]
        public void T27_DynamicArray_Invalid_Index_1465614_2()
        {
            string code = @"
class Point
{
X : double;
Y : double;
Z : double;
constructor ByCoordinates ( x1 : double, y1 : double, z1 : double ) 
{
X = x1;
Y = y1;
Z = z1;
}
}
class Line{
P1: Point;
P2: Point;
constructor ByStartPointEndPoint ( p1: Point, p2: Point )
{
P1 = p1;
P2 = p2;
}
}
baseLineCollection = { };
basePoint = { }; // replace this with ""basePoint = { 0, 0};"", and it works fine
nsides = 2;
a = 0..nsides - 1..1;
[Imperative]
{
for(i in a)
{
basePoint[i] = Point.ByCoordinates(i, i, 0);
}
for(i in a)
{
baseLineCollection[i] = Line.ByStartPointEndPoint(basePoint[i], basePoint[i+1]);
}
}
x=basePoint[0].X;
y=basePoint[0].Y;
z=basePoint[0].Z;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 0.0);
            thisTest.Verify("y", 0.0);
            thisTest.Verify("z", 0.0);
        }

        [Test]
        [Category("DSDefinedClass")]
        public void T27_DynamicArray_Invalid_Index_1467104()
        {
            string code = @"
class Point
{
	x : var;
	constructor Create(xx : double)
	{
		x = xx;
	}
}
pts = Point.Create( { 1, 2} );
aa = pts[null].x;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object aa = null;
            thisTest.Verify("aa", aa);

        }

        [Test]
        [Category("DSDefinedClass")]
        public void T27_DynamicArray_Invalid_Index_1467104_2()
        {
            string code = @"
class Point
{
x : var[];
constructor Create(xx : double)
{
	x = {xx,1};
}
}
aa;
[Imperative]
{
pts = Point.Create( { 1, 2} );
aa = pts[null].x[null];
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object aa = null;
            thisTest.Verify("aa", aa);

        }

        [Test]
        [Category("DSDefinedClass")]
        public void T27_DynamicArray_Invalid_Index_1467104_3()
        {
            string code = @"
class Point
{
x : var[];
constructor Create(xx : double)
{
	x = {xx,1};
}
}
aa;aa1;
[Imperative]
{
	pts = Point.Create( { 1, 2} );
	aa = pts[null];
	aa1 = pts[null].x[null];
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object aa = null;
            object aa1 = null;
            thisTest.Verify("aa", aa);
            thisTest.Verify("aa1", aa1);
        }
        /* 
[Test]
         [Category ("SmokeTest")]
  public void T27_defect_1464429_DynamicArray_function()
         {
             ExecutionMirror mirror = thisTest.RunScript(filePath, "T27_defect_1464429_DynamicArray_function.ds");
             Object[] t1 = new Object[] { -2, -1 };
            
             thisTest.Verify("t2", t2);
         }*/


        [Test]
        [Category("SmokeTest")]
        public void T28_defect_1465706__DynamicArray_Imperative()
        {
            string code = @"
test;
[Imperative]
{
def CreateArray ( x : var[] , i )
{
x[i] = i;
return = x;
}
test = { };
test = CreateArray ( test, 0 );
test = CreateArray ( test, 1 );
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] t = new Object[] { 0, 1 };
            thisTest.Verify("test", t);
        }

        [Test]
        [Category("SmokeTest")]
        public void T28_defect_1465706__DynamicArray_Imperative_2()
        {
            string code = @"
a;
r;
[Imperative]
{
    def test (i:int)
    {
        loc = {};
        for(j in i)
        {
            loc[j] = j;
        }
        return = loc;
    }
    a={3,4,5};
    t = test(a);
    r = {t[0][3], t[1][4], t[2][5]};
    return = r;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] a = new Object[] { 3, 4, 5 };
            Object[] r = new Object[] { 3, 4, 5 };
            thisTest.Verify("a", a);
            thisTest.Verify("r", r);
        }

        [Test]
        [Category("SmokeTest")]
        public void T29_DynamicArray_Using_Out_Of_Bound_Indices()
        {
            string code = @"
   
    basePoint = {  };
    
    basePoint [ 4 ] =3;
    test = basePoint;
    
    a = basePoint[0] + 1;
    b = basePoint[ 4] + 1;
    c = basePoint [ 8 ] + 1;
    
    d = { 0,1 };
    e1 = d [ 8] + 1;
    
    x = { };
    y = { };    
    t = [Imperative]
    {
        k = { };
	for ( i in 0..1 )
	{
	    x[i] = i;
	}
	k[0] = 0;
	for ( i in x )
	{
	    y[i] = x[i] + x[i+1];
	    k[i+1] = x[i] + x[i+1];
	
	}
	return = k;
    }
    z = y;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 0, 1, null };
            Object v2 = null;
            Object[] v3 = new Object[] { 0, 1 };
            Object[] v4 = new Object[] { 1, null };
            thisTest.Verify("a", v2);
            thisTest.Verify("b", 4);
            thisTest.Verify("c", v2);
            thisTest.Verify("d", v3);
            thisTest.Verify("t", v1);
            thisTest.Verify("z", v4);

        }

        [Test]
        [Category("SmokeTest")]
        public void T40_Index_usingFunction_1467064()
        {
            string code = @"
def foo()
{    
return = 0;
}
x = { 1, 2 };
x[foo()] = 3;
y = x;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] x = new Object[] { 3, 2 };
            Object[] y = new Object[] { 3, 2 };
            thisTest.Verify("x", x);
            thisTest.Verify("y", y);
        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T40_Index_usingFunction_class_1467064_2()
        {
            string code = @"
class test
{
def foo()
{    
	return = 0;
}
}
x = { 1, 2 };
y=test.test();
x[y.foo()] = 3;
a = x;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] x = new Object[] { 3, 2 };
            Object[] a = new Object[] { 3, 2 };
            thisTest.Verify("x", x);
            thisTest.Verify("a", a);
        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T40_Index_byFunction_class_imperative_1467064_3()
        {
            string code = @"
class test
{
def foo()
{    
	return = 2;
}
}
x;y;
[Imperative]
{
x = { 1, 2 };
y=test.test();
x[y.foo()] = 3;
y = x;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] x = new Object[] { 1, 2, 3 };
            Object[] y = new Object[] { 1, 2, 3 };
            thisTest.Verify("x", x);
            thisTest.Verify("y", y);
        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T40_Index_byFunction_argument_1467064_4()
        {
            string code = @"
class test
{
def foo(y:int)
{    
	return = y;
}
}
x;y;
[Imperative]
{
x = { 1, 2 };
y=test.test();
x[y.foo(1)] = 3;
y = x;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] x = new Object[] { 1, 3 };
            Object[] y = new Object[] { 1, 3 };
            thisTest.Verify("x", x);
            thisTest.Verify("y", y);
        }
        /* 
[Test]
         public void T40_Index_DynamicArray_1467064_5()
         {
             Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
             {
                 ExecutionMirror mirror = thisTest.RunScript(filePath, "T40_Index_DynamicArray_byarray_1467064_5.ds");
             });
         }
         
[Test]
         public void T40_Index_DynamicArray_1467064_6()
         {
             Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
             {
                 ExecutionMirror mirror = thisTest.RunScript(filePath, "T40_Index_DynamicArray_byarray_1467064_6.ds");
             });
         }*/

        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T41_Accessing_Non_Existent_Properties_From_Array_Elements()
        {
            string code = @"
class A 
{
    x : var;
    constructor A ( y : var )
    {
        x = y;
    }
}
class B 
{
    x2 : var;
    constructor B ( y : var )
    {
        x2 = y;
    }
}
c = { A.A(0), B.B(1) };
d = c[1].x; 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            // Assert.Fail("1467083 - Sprint23 : rev 2681 : error expected when accessing nonexistent properties of array elements!");
            Object v1 = null;
            thisTest.Verify("d", v1);

        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T41_Accessing_Non_Existent_Property_FromArray_1467083()
        {
            string code = @"
class A
{
x : var;
constructor A ( y : var )
{
x = y;
}
}
class B
{
x2 : var;
constructor B ( y : var )
{
x2 = y;
}
}
c = { A.A(0), B.B(1) };
c0 = c[0].x;//0
d = c[1].x;//null
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object a = null;
            TestFrameWork.Verify(mirror, "c0", 0);
            TestFrameWork.Verify(mirror, "d", a);
        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T41_Accessing_Non_Existent_Property_FromArray_1467083_2()
        {
            string code = @"
class A
{
x : var;
constructor A ( y : var )
{
x = y;
}
}
class B
{
x2 : var;
constructor B ( y : var )
{
x2 = y;
}
}
c = { A.A(0), B.B(1),1 };
e = c[2].x;
e1 = c[2].x2;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object a = null;
            TestFrameWork.Verify(mirror, "e", a);
            TestFrameWork.Verify(mirror, "e1", a);
        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T41_Accessing_Non_Existent_Property_FromArray_1467083_3()
        {
            string code = @"
class A
{
x : var;
constructor A ( y : var )
{
x = y;
}
}
class B
{
x2 : var;
constructor B ( y : var )
{
x2 = y;
}
}
c = null;
d={A.A(0),null};
e = c[0].x; // null
e1 = c[1].x2;//null
f = d[0].x;//0
f1 = d[1].x2;//null
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object a = null;
            TestFrameWork.Verify(mirror, "e", a);
            TestFrameWork.Verify(mirror, "e1", a);
            TestFrameWork.Verify(mirror, "f", 0);
            TestFrameWork.Verify(mirror, "f1", a);
        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("Variable resolution")]
        public void T61_Accessing_Non_Existent_Array_Properties_1467082()
        {
            //Assert.Fail("1467094 - Sprint 24 : Rev 2748 : in some cases if try to access nonexisting item in an array it does not return null ) ");
            string code = @"
class A  
{
    x : var;
    constructor A ( y : var )
    {
        x = y;
    }
}
c = { A.A(0), A.A(1) };
p = {};
d=p[1];
d = [Imperative]
{
    if(c[0].x == 0 )
    {
        c[0] = 0;
	p[0] = 0;
    }
    if(c[0].x == 0 )
    {
        p[1] = 1;
    }
    return = 0;
}
t1 = c[0];
t2 = c[1].x;
t3=p[0];
t4=p[1];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification 
            thisTest.Verify("t1", 0);
            thisTest.Verify("t2", 1);
            thisTest.Verify("t3", 0);

            Object a = null;
            thisTest.Verify("t4", a);
        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T61_Accessing_Non_Existent_Array_Properties_1467082_2()
        {
            // Assert.Fail("");
            string code = @"
class A  
{
  x : var;
    constructor A ( y : var )
    {
        x = y;
    }
}
c = { A.A(0), A.A(1) };
p = {};
q=p[0].X;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification 
            object a = null;
            thisTest.Verify("q", a);
        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T61_Accessing_Non_Existent_Array_Properties_1467082_3()
        {
            // Assert.Fail("");
            string code = @"
class A  
{
    x : var;
    constructor A ( y : var )
    {
        x = y;
    }
}
c = { A.A(0), A.A(1) };
p = {};
q=p[0].X;
c[0]=0;
p=c[0].X; // access as if its propoerty of the class, but thevalue is not class 
r=c[0][0].X;// non existing index 
s=c[0].X[0];// access non array variable as if its array ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification 
            object a = null;
            thisTest.Verify("p", a);
            thisTest.Verify("q", a);
            thisTest.Verify("r", a);
            thisTest.Verify("s", a);
        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T61_Assign_Non_Existent_Array_Properties_1467082_4()
        {


            //thisTest.VerifyWarning(ProtoCore.BuildData.WarningID.kIdUnboundIdentifier);    
            string errmsg = "1467235 - Sprint25: rev 3411 : When class property is a collection and a single value is passed to it, it should be coerced to a collection";
            string code = @"class A  
{
   x : var [];
    constructor A ( y : var )
    {
        x = y;
    }
}
c = { A.A({0,1}), A.A({2}) };
d = { A.A({0,1}), A.A({2}) };
e = { A.A({0,1}), A.A({2}) };
f = { A.A({0,1}), A.A({2}) };
c[1].x=5;// wrong index 
d[1].x={0,1}; // entire row 
e[1][1].x=5;// non existing index 
f[1][0].x=5;// correct one 
p = c[1].x; 
q = d[1].x;
r = e[1][1].x;
s = f[1][0].x;
";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);

            //Verification 
            object a = null;
            thisTest.Verify("p", new Object[] { new Object[] { 5 } });
            thisTest.Verify("q", new Object[] { new Object[] { 0, 1 } });
            thisTest.Verify("r", a);
            thisTest.Verify("s", new Object[] { 5 });
        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T61_Assign_Non_Existent_Array_Properties_1467094()
        {
            // Assert.Fail("");
            string code = @"
class A
{
x : var;
constructor A ( y : var )
{
x = y;
}
}
c = { A.A(0), A.A(1) };
p = {};
d = [Imperative]
{
if(c[0].x == 0 )
{
c[0] = 0;
p[0] = 0;
}
if(c[0].x == 0 )
{
p[1] = 1;
}
return = 0;
}
t1 = c[0];
t2 = c[1].x;
t3=p[0];
t4=p[1];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //thisTest.VerifyWarning(ProtoCore.BuildData.WarningID.kIdUnboundIdentifier);    
            //Verification 
            object a = null;
            thisTest.Verify("t4", a);

        }

        [Test]
        [Category("SmokeTest")]
        public void T62_Create_Dynamic_Array_OnTheFly()
        {
            string code = @"
i;
b;
z=[Imperative]
{
for (i in 0..7)
{
b[i] = i;
}
return=b;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("i", 7);
        }

        [Test]
        [Category("SmokeTest")]
        public void T62_Create_Dynamic_Array_OnTheFly_AsFunctionArgument()
        {
            string code = @"
def func(a:int)
{
a=5;
return = a;
}
b= func(c[0]);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object a = null;
            thisTest.Verify("c", a);
            thisTest.Verify("b", 5);

        }

        [Test]
        [Category("SmokeTest")]
        public void T62_Change_Avariable_To_Dynamic_Array_OnTheFly()
        {
            string code = @"
def func(a:int)
{
a=5;
return = a;
}
c=1;
b= func(c[0]);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", 1);
            thisTest.Verify("b", 5);

        }

        [Test]
        [Category("SmokeTest")]
        public void T63_Create_MultiDimension_Dynamic_Array_OnTheFly()
        {
            string code = @"
def func(a:int[]..[])
{
a[0][1]=5;
a[2][3]=6;
return = a;
}
c=1;
b= func(c);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", 1);
            thisTest.Verify("b", new Object[][] { new Object[] { 1, 5 }, null, new Object[] { null, null, null, 6 } });
        }

        [Test]
        [Category("SmokeTest")]
        public void T63_Dynamic_array_onthefly_1467066()
        {
            string code = @"
b;
z=[Imperative]
{
b[5]=0;
for (i in 0..7)
{
b[i] = i;
}
return=b;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("z", new Object[] { 0, 1, 2, 3, 4, 5, 6, 7 });
            thisTest.Verify("b", new Object[] { 0, 1, 2, 3, 4, 5, 6, 7 });

        }

        [Test]
        [Category("SmokeTest")]
        public void T63_Dynamic_array_onthefly_aliastoanother()
        {
            string code = @"
a=5;
b=a;
a[2]=3;
b[2]=-5;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new Object[] { 5, null, 3 });
            thisTest.Verify("b", new Object[] { 5, null, -5 });

        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T62_Create_Dynamic_Array_OnTheFly_inaClass()
        {
            string code = @"
class test
{
b:int=10;
	constructor test(a:int)
	{
		a=b;
	
	}
}
d=5;
a=test.test(d[0]);
c= a.b;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("d", 5);
            thisTest.Verify("c", 10);

        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T62_Create_Dynamic_Array_OnTheFly_passargument()
        {
            // Assert.Fail("1467139-Sprint 24 - Rev 2958 - an array created dynamically on the fly and passed as an arguemnt to method it gets garbage collected ");
            string code = @"
class test
{
b:int=10;
	
  constructor test(a:int[])
	{
	b=1;
	
	}
}
d[0]=5;
a=test.test(d);
c= a.b;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("d", new Object[] { 5 });
            thisTest.Verify("c", 1);

        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T62_Create_Dynamic_Array_OnTheFly_inaClass_methodoverload()
        {
            // Assert.Fail("1467139-Sprint 24 - Rev 2958 - an array created dynamically on the fly and passed as an arguemnt to method it gets garbage collected ");
            string code = @"
class test
{
b:int=10;
	
  constructor test(a:int)
	{
	b=0;
	
	}
 constructor test(a:int[])
	{
	b=1;
	
	}
}
d[0]=5;
a=test.test(d);
c= a.b;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("d", new Object[] { 5 });
            thisTest.Verify("c", 1);

        }

        [Test]
        [Category("SmokeTest")]
        public void T62_Create_Dynamic_Array_OnTheFly_passargument_function()
        {

            string code = @"
	
  def test(a:int[])
	{
	b=1;
	return=b;
	}
d[0]=5;
a=test(d);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("d", new Object[] { 5 });
            thisTest.Verify("a", 1);
        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("SmokeTest")]
        public void T63_Dynamic_array_onthefly_argument_class__1467139()
        {
            string code = @"
class test
{
b:int=10;
        
  constructor test(a:int[])
        {
        b=1;
        
        }
}
d[0]=5;
a=test.test(d);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("d", new Object[] { 5 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T63_Dynamic_array_onthefly_function_1467139()
        {
            string code = @"
def foo(a:int[])
{
}
x[0]=5;
a = foo(x);
c = {100};
t = x;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t", new Object[] { 5 });
        }

        [Test]
        public void T63_Dynamic_array_onthefly_update()
        {
            string code = @"
z=true;
b=z;
z[0]={1};
z=5;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
        }

        [Test]
        public void T64_Modify_itemInAnArray_1467093()
        {
            string code = @"
a = {1, 2, 3};
a[1] = a; 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new Object[] { 1, new Object[] { 1, 2, 3 }, 3 });
        }

        [Test]
        public void T64_Modify_itemInAnArray_1467093_2()
        {
            string code = @"
a;b;c;
[Imperative]
{
a = {};
b = a;
a[0] = b;
//hangs here
c = a;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new Object[] { new Object[] { } });
            thisTest.Verify("b", new Object[] { });
            thisTest.Verify("c", new Object[] { new Object[] { } });
        }

        [Test]
        public void T65_Array_Alias_ByVal_1467165()
        {
            string code = @"
a = {0,1,2,3};
b=a;
a[0]=9;
b[0]=10;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new Object[] { 9, 1, 2, 3 });
            thisTest.Verify("b", new Object[] { 10, 1, 2, 3 });
        }

        [Test]
        [Category("DSDefinedClass")]
        public void T65_Array_Alias_ByVal_1467165_2()
        {
            string code = @"
class A
{
        id:int;
}
class B
{
        id:int;
}
a = {A.A(),B.B()};
b=a;
a[0].id = 100;
b[0].id = 200;
c=a[0].id;
d=b[0].id;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", 200);
            thisTest.Verify("d", 200);
        }

        [Test]
        public void T65_Array_Alias_ByVal_1467165_3()
        {
            string code = @"
a = {0,1,2,3};
b=a;
a[0]=9;
b[0]=false;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new Object[] { 9, 1, 2, 3 });
            thisTest.Verify("b", new Object[] { false, 1, 2, 3 });

        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("Design Issue")]
        public void T65_Array_Alias_ByVal_1467165_4()
        {
            //Assert.Fail("1467182 - Sprint 25 - [Design Decision] Rev 3163 - method resolution or type conversion is expected in following cases ");
            string code = @"
class A
{
        id:int;
}
class B
{
        id:int;
}
a = {A.A(),B.B()};
b=a;
a[0].id = 100;
b[0].id = ""false"";
c=a[0].id;
d=b[0].id;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);


        }

        [Test]
        [Category("DSDefinedClass")]
        public void T65_Array_Alias_ByVal_1467165_5()
        {
            string code = @"
class A
{
        id:int;
}
class B
{
        id:int;
}
a = {A.A(),B.B()};
b=a;
a[0].id = 100;
b[0].id = null;
c=a[0].id;
d=b[0].id;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", null);
            thisTest.Verify("d", null);
        }

        [Test]
        public void T65_Array_Alias_ByVal_1467165_6()
        {
            string code = @"
a = {0,1,2,3};
b=a;
a[0]=null;
b[0]=false;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new Object[] { null, 1, 2, 3 });
            thisTest.Verify("b", new Object[] { false, 1, 2, 3 });
        }

        [Test]
        [Category("Replication")]
        [Category("Feature")]
        public void T66_Array_CannotBeUsedToIndex1467069()
        {
            string code = @"
x;
[Imperative]
{
    a = {3,1,2}; 
    x = {10,11,12,13,14,15}; 
    x[a] = 2;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", new Object[] { 10, 2, 2, 2, 14, 15 });
        }

        [Test]
        [Category("Replication")]
        [Category("Feature")]
        public void T66_Array_CannotBeUsedToIndex1467069_2()
        {
            string code = @"
x;
[Imperative]
{
    a = {3,1,2}; 
    x = {10,11,12,13,14,15}; 
    x[a] = 2;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", new Object[] { 10, 2, 2, 2, 14, 15 });
        }

        [Test]
        public void T67_Array_Remove_Item()
        {
            string code = @"
a={1,2,3,4,5,6,7};
a=Remove(a,0);// expected :{2,3,4,5,6,7}
a=Remove(a,4);//expected {1,2,3,4,6,7}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new Object[] { 2, 3, 4, 5, 7 });
        }

        [Test]
        public void T67_Array_Remove_Item_2()
        {
            string code = @"
a={1,2,3,4,5,6,7};
a=Remove(a,0);// expected :{2,3,4,5,6,7}
a=Insert(a,4,6);//expected {1,2,3,4,6,7}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new Object[] { 2, 3, 4, 5, 6, 7, 4 });
        }

        [Test]
        public void T68_copy_byreference_1467105()
        {
            string code = @"
test;
[Associative]
{
a = { 1, 2, 3};
b = a;
b[0] = 10;
test = a[0]; //= 10� i.e. a change in �b� causes a change to �a�
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("test", 1);
        }

        [Test]
        [Category("Update")]
        public void T43_Defect_1467234_negative_indexing()
        {
            String code =
@"a = { };
a[-1] = 1;
b = a;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new Object[] { 1 });
        }

        [Test]
        [Category("DSDefinedClass")]
        [Category("Update")]
        public void T44_Defect_1467264()
        {
            String code =
@"class A
{
    X : var[];
    constructor  A ( t1 : var[] )
    {
        X = t1;
    }
}
a1 = { A.A(1..2), A.A(2..3) };
a = a1[0].X[0][1];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object n1 = null;
            thisTest.Verify("a", n1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T45_Defect_1467393()
        {
            String code =
@"arr = { { } };
[Imperative]
{
    numRings = 3;    
    for(i in(0..(numRings-1)))
    {
        [Associative]
        {
            x = 0..i..#numRings;
            arr[i] = x;
        }       
    }
}
test = arr;
a= 1;
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] v1 = new Object[] { new Object[] { 0, 0, 0 }, new Object[] { 0.0, 0.5, 1.0 }, new Object[] { 0, 1, 2 } }
;
            thisTest.Verify("test", v1);

        }

        [Test]
        [Category("SmokeTest")]
        public void T45_Defect_1467393_2()
        {
            String code =
@"arr = { { } };
[Imperative]
{
    numRings = 3;    
    for(i in(0..(numRings-1)))
    {
        s = Print(i);
        [Associative]
        {
            x = 0..i;
            arr[i] = x;
        }       
    }
}
test = arr;
a= 1;
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] v1 = new Object[] { new Object[] { 0 }, new Object[] { 0, 1 }, new Object[] { 0, 1, 2 } }
;
            thisTest.Verify("test", v1);
        }

        [Test]
        public void T45_Defect_1467393_3()
        {
            String code =
@"
def foo ()
{
    arr = { { } };
    [Imperative]
    {
        numRings = 3;    
        for(i in(0..(numRings-1)))
        {
            s = Print(i);
            [Associative]
            {
                x = 0..i;
                arr[i] = x;
            }       
        }
    }
    return = arr;
}
test = foo();
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] v1 = new Object[] { new Object[] { 0 }, new Object[] { 0, 1 }, new Object[] { 0, 1, 2 } }
;
            thisTest.Verify("test", v1);
        }

        [Test]
        public void T45_Defect_1467393_4()
        {
            String code =
@"
def foo ()
{
    arr = { { } };
    [Imperative]
    {
        x = {0, 1};
        for(i in x)
        {
            [Associative]
            {
                arr[i] = 0..i;
            }       
        }
    }
    return = arr;
}
test = foo();
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] v1 = new Object[] { new Object[] { 0 }, new Object[] { 0, 1 } }
;
            thisTest.Verify("test", v1);
        }

        [Test]
        public void T46_Defect_1467502_1()
        {
            String code =
@"
//arr; //  declaring arr here works
test = [Imperative]
{
    arr; //  declaring arr here does not work.
    //arr = { }; //  declaring arr with = {} works
    
    // create arr
    for(i in (0..1))
    {
        arr[i] = i;
    }
    return = arr;
}
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] v1 = new Object[] { 0, 1 }
;
            thisTest.Verify("test", v1);
        }

        [Test]
        public void T46_Defect_1467502_2()
        {
            String code =
@"
arr; //  declaring arr here works
test = [Imperative]
{
    // create arr
    for(i in (0..1))
    {
        arr[i] = i;
    }   
}
test = arr;
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] v1 = new Object[] { 0, 1 }
;
            thisTest.Verify("test", v1);
        }

        [Test]
        public void T46_Defect_1467502_3()
        {
            String code =
@"
arr = { }; 
test = [Imperative]
{
    // create arr
    for(i in (0..1))
    {
        arr[i] = i;
    }   
}
test = arr;
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] v1 = new Object[] { 0, 1 }
;
            thisTest.Verify("test", v1);
        }

        [Test]
        public void T46_Defect_1467502_4()
        {
            String code =
@"
test = [Imperative]
{
    arr = { } ;
    // create arr
    for(i in (0..1))
    {
        arr[i] = i;
    }  
    return= arr; 
}
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object[] v1 = new Object[] { 0, 1 }
;
            thisTest.Verify("test", v1);
        }

        [Test]
        public void T46_Defect_1467502_5()
        {
            String code =
@"
test = [Imperative]
{
    arr = { } ;    
    for(i in (0..1))
    {
        arr[i][i] = i;
    }
    return = arr;   
}
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            Object[] v1 = new Object[] { new Object[] { 0 }, new Object[] { n1, 1 } };
            ;
            thisTest.Verify("test", v1);
        }

        [Test]
        public void T46_Defect_1467502_6()
        {
            String code =
@"
test = [Imperative]
{
    arr = null ;    
    for(i in (0..1))
    {
        arr[i][i] = i;
    }
    return = arr;   
}
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            Object[] v1 = new Object[] { new Object[] { 0 }, new Object[] { n1, 1 } };
            ;
            thisTest.Verify("test", v1);
        }

        [Test]
        public void T46_Defect_1467502_7()
        {
            String code =
@"
arr;
[Imperative]
{
    //arr = null ;    
    for(i in (0..1))
    {
        arr[i][i] = i;
    }
    //return = arr;   
}
test = arr;
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            Object[] v1 = new Object[] { new Object[] { 0 }, new Object[] { n1, 1 } };
            ;
            thisTest.Verify("test", v1);
        }

        [Test]
        public void T46_Defect_1467502_8()
        {
            String code =
@"
arr = { };
test = [Imperative]
{
    //arr = null ;    
    for(i in (0..1))
    {
        arr[i][i] = i;
    }
    return = arr;   
}
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            Object[] v1 = new Object[] { new Object[] { 0 }, new Object[] { n1, 1 } };
            ;
            thisTest.Verify("test", v1);
        }

        [Test]
        public void T46_Defect_1467502_9()
        {
            String code =
@"
arr = { {}, {}};
[Imperative]
{
    //arr = null ;    
    for(i in (0..1))
    {
        arr[i][i] = i;
    }
    return = arr;   
}
test = arr;
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            Object[] v1 = new Object[] { new Object[] { 0 }, new Object[] { n1, 1 } };
            ;
            thisTest.Verify("test", v1);
        }

        [Test]
        public void T46_Defect_1467502_9_1()
        {
            String code =
@"
//arr = { {}, {}};
def foo ()
{
    t = [Imperative]
    {
        arr = null ;    
        for(i in (0..1))
        {
            arr[i][i] = i;
        }
        return = arr;   
    }
    return = t;
}
test = foo();
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            Object[] v1 = new Object[] { new Object[] { 0 }, new Object[] { n1, 1 } };
            ;
            thisTest.Verify("test", v1);
        }

        [Test]
        [Category("DSDefinedClass")]
        public void T46_Defect_1467502_9_2()
        {
            String code =
@"
class A
{
    static def foo ()
    {
        t = [Imperative]
        {
            arr  ;    
            for(i in (0..1))
            {
                arr[i][i] = i;
            }
            return = arr;   
        }
        return = t;
    }
}
test = A.foo();
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            Object[] v1 = new Object[] { new Object[] { 0 }, new Object[] { n1, 1 } };
            ;
            thisTest.Verify("test", v1);
        }

        [Test]
        public void T46_Defect_1467502_9_3()
        {
            String code =
@"
arr = { {}, {}};
def foo ()
{
    t = [Imperative]
    {
        //arr = null ;    
        for(i in (0..1))
        {
            arr[i][i] = i;
        }
        return = arr;   
    }
    return = t;
}
test = foo();
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            Object[] v1 = new Object[] { new Object[] { 0 }, new Object[] { n1, 1 } };
            ;
            thisTest.Verify("test", v1);
        }

        [Test]
        [Category("DSDefinedClass")]
        public void T46_Defect_1467502_9_4()
        {
            String code =
@"
arr;
class A
{
    static def foo ()
    {
        t = [Imperative]
        {
            //arr  ;    
            for(i in (0..1))
            {
                arr[i][i] = i;
            }
            return = arr;   
        }
        return = t;
    }
}
test = A.foo();
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            Object[] v1 = new Object[] { new Object[] { 0 }, new Object[] { n1, 1 } };
            ;
            thisTest.Verify("test", v1);
        }

        [Test]
        [Category("DSDefinedClass")]
        public void T46_Defect_1467502_9_5()
        {
            String code =
@"
class B
{
    x : int = 0;
}
arr = { {}, {}};
def foo ()
{
    t = [Imperative]
    {
        //arr = null ;    
        for(i in (0..1))
        {
            arr[i][i] = B.B();
        }
        return = arr;   
    }
    return = t;
}
test = foo().x;
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            Object[] v1 = new Object[] { new Object[] { 0 }, new Object[] { n1, 0 } };
            ;
            thisTest.Verify("test", v1);
        }

        [Test]
        [Category("DSDefinedClass")]
        public void T46_Defect_1467502_9_6()
        {
            String code =
@"
//arr;
class B
{
    x : int = 0;
}
class A
{
    static def foo ()
    {
        t = [Imperative]
        {
            arr  ;    
            for(i in (0..1))
            {
                arr[i][i] = B.B();
            }
            return = arr;   
        }
        return = t;
    }
}
test = A.foo().x;
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            Object[] v1 = new Object[] { new Object[] { 0 }, new Object[] { n1, 0 } };

            thisTest.Verify("test", v1);
        }

        [Test]
        [Category("DSDefinedClass")]
        public void T47_Defect_1467561_1()
        {
            String code =
@"
//arr;
class B
{
    x : int = 0;
}
class A
{
    static def foo ()
    {
        t = [Imperative]
        {
            arr  ;    
            for(i in (0..1))
            {
                if ( i < 3 )
                {
                    arr[i][i] = B.B();
                }
            }
            return = arr;   
        }
        return = t;
    }
}
test = A.foo().x;
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            Object[] v1 = new Object[] { new Object[] { 0 }, new Object[] { n1, 0 } };
            thisTest.Verify("test", v1);
        }

        [Test]
        [Category("DSDefinedClass")]
        public void T47_Defect_1467561_2()
        {
            String code =
@"
class B
{
    x : int = 0;
}
class A
{
    def foo ()
    {
        t = [Imperative]
        {
            arr = {} ;    
            for(i in (0..1))
            {
                if ( i < 3 )
                {
                    arr[i][i] = B.B();
                }
            }
            return = arr;   
        }
        return = t;
    }
}
aa = A.A();
test = aa.foo().x;
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            Object[] v1 = new Object[] { new Object[] { 0 }, new Object[] { n1, 0 } };
            thisTest.Verify("test", v1);
        }

        [Test]
        [Category("DSDefinedClass")]
        public void T47_Defect_1467561_3()
        {
            String code =
@"
arr;
class B
{
    x : int = 0;
}
class A
{
    static def foo ()
    {
        t = [Imperative]
        {
            for(i in (0..1))
            {
                if ( i < 3 )
                {
                    arr[i][i] = B.B();
                }
            }
            return = arr;   
        }
        return = t;
    }
}
test = A.foo().x;
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            Object[] v1 = new Object[] { new Object[] { 0 }, new Object[] { n1, 0 } };
            thisTest.Verify("test", v1);
        }
        // For Unit tests of Dictionary pls refere Microfeaturetests.cs
        [Test]
        public void T48_Dictionaryvariableaskey()
        {
            String code =
            @"
                a = {1, 2, 3};
                b=""x"";
                a[b] = 4;
                r = a [b];
            ";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 4);
        }
        [Test]
        public void T49_DictionaryvariableArray()
        {
            String code =
            @"
                a = {1, 2, 3};
                b={""x"",""y""};
                a[b] = {4,7};
                r = a [b[0]];
                r1 = a [b];
            ";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 4);
            thisTest.Verify("r1", new object[] { 4, 7 });
        }
        [Test]
        public void T50_DictionaryInline()
        {
            String code =
            @"
                a = {1, 2, 3};
                b=""x"";
                a[b] =(b!=null)?4:-4;
                r = a [b];
            ";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 4);
        }
        [Test]
        public void T51_DictionaryInline_2()
        {
            String code =
            @"
                a = {1, 2, 3};
                b={1,-1};
                a[b] =(b>0)?4:-4;
                r = a [b];
            ";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", new object[] { 4, -4 });
        }
        [Test]
        public void T52_DictionaryKeynull()
        {
            // as per spec this is null as key is supported
            String code =
            @"
                a = {1, 2, 3};
                b=null;
                a[b] =4;
                r = a [b];
            ";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 4);
        }
        [Test]
        public void T53_DictionaryKeyUpdate()
        {

            String code =
            @"
                a = {1, 2, 3};
                 b=""x"";
                 a[b] =4;
                r = a[b];
                b = ""y"";
                a[b] = 10;
            ";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 10);
        }
        [Test]
        public void T54_DictionaryKeyUpdate_2()
        {

            String code =
            @"
               a = {1, 2, 3};
                 b=true;
                 a[b] =4;
                r = a[b];
                b = 1;
                a[b] = 10;
            ";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 10);
        }

        [Test]
        [Category("DSDefinedClass")]
        public void T55_DictionaryKeyinClass()
        {

            String code =
            @"
               
            class test
                {
                    a = { 1, 2, 3 };
                    b = ""x"";
                    def foo(c:int)
                    {
                        a[b] = c;
                        return =a;
                    }
                }

            z = test.test();
            y = z.foo(5);
            r=y[""x""];
            ";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 5);

        }
        [Test]
        [Category("DSDefinedClass")]
        public void T56_DictionaryKeyinClass_2()
        {

            String code =
            @"
               
          class test
            {
                a = { 1, 2, 3 };
                b = ""x"";
                def foo(c:int)
                {
                    a[b] = c;
                    return =a;
                }
            }

            z = test.test();
            y = z.foo(5);
            x = y[z.b];
            ";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 5);

        }

        [Test]
        [Category("DSDefinedClass")]
        public void T57_DictionaryKeyinClass_inheritance()
        {

            String code =
            @"
               
          
            class test
                {
                    a = { 1, 2, 3 };
                    b = ""x"";
                    def foo(c:int)
                    {
                        a[b] = c;
                        return =a;
                    }
                }
            class mytest extends test
            {
                def foo1(d : int)
                {
                    a[b] = d;

                    return = this.a;
                }
            }
            z = mytest.mytest();
            y = z.foo1(5);

            x = y[z.b];
            ";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 5);

        }
        [Test]
        [Category("DSDefinedClass")]
        public void T58_DictionaryKeyinClass_inheritance2()
        {

            String code =
            @"
               
          class test
                {
                    a = { 1, 2, 3 };
                    b = ""x"";
                    def foo(c:int)
                    {
                        a[b] = c;
                        return =a;
                    }
                }
            class mytest extends test
            {
                e = ""y"";
                def foo1(d : int)
                {
                    a[b] = d;
                    a[e] = d + 1;
                    return = this.a;
                }
            }
            z = mytest.mytest();
            y = z.foo1(5);

            x1 = y[z.b];
            x2 = y[z.e];



            ";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x1", 5);
            thisTest.Verify("x2", 6);

        }
        [Test]
        [Category("DSDefinedClass")]
        public void T59_DotOperator()
        {

            String code =
            @"
           class test
                {
                    a = { 1, 2, 3 };
                    b = ""x"";
                    constructor test(c:int)
                    {
                        a[b] = c;
                    }
                }
            z = test.test(5);

            r=z.a[""x""];
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 5);
        }
        [Test]
        [Category("DSDefinedClass")]
        public void T60_DictionaryDotOperator()
        {

            String code =
            @"
               
          class test
            {
                a = { 1, 2, 3 };
                b = {""x"",""y""};
                def foo(c:int)
                {
                    a[b[0]] = c;
                    a[b[1]] = c+1;
                    return =a;
                }
            }

            z = test.test();
            y = z.foo(5);
            x = y[z.b];
            ";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", new object[] { 5, 6 });

        }
        [Test]
        public void T69_DictionaryDynamicArray()
        {

            String code =
            @"
               
          
                a = { };
                b = {""x"",""y""};
                a[b[0]] = 5;
                a[b[1]] = 6;
                a[0]=1;
                a[1]=2;
                r = a[b];
            ";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", new object[] { 5, 6 });

        }
        [Test]
        public void T70_DictionaryImeperativeWhile()
        {

            String code =
            @"
               
            a={};                
[Imperative]
{
    i =0;
    while (i<5)
    {
        a["""" + i] = i;
        i = i + 1;
    }
}
r0 = a[""0""];
r1 = a[""1""];
r2 = a[""2""];
r3 = a[""3""];
r4 = a[""4""];
r5 = a[""5""];

                
            ";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r0", 0);
            thisTest.Verify("r1", 1);
            thisTest.Verify("r2", 2);
            thisTest.Verify("r3", 3);
            thisTest.Verify("r4", 4);
            thisTest.Verify("r5", null);

        }
        [Test]
        public void T71_DictionaryImeperativeFor()
        {

            String code =
            @"
               
a={};                
[Imperative]
{
    i = 0;
    b = { ""x"",true,1};
    for(i in b)
    {
        a[i] = i;
    }
}
r0 = a[""x""];
r1 = a[true];
r2 = a[1];
                
            ";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r0", "x");
            thisTest.Verify("r1", true);
            thisTest.Verify("r2", 1);


        }
        /*   [Test]
           public void T60_Dictionarytemp()
           {
                Assert.Throws(typeof(ProtoCore.Exceptions.RuntimeException), () =>
               {
               String code =
               @"
               
             class test
               {
                   static a = { 1, 2, 3 };
                   static b = {""x"",""y""};
                   static def foo(c:int)
                   {
                       a[b[0]] = c;
                       a[b[1]] = c+1;
                       return =a;
                   }
               }
               def foo(z:test)
               {

                   y = z.foo(5);
                   x = y[z.b];
                   return =x;
               }
               z1 = foo(test.test());
                

               ";

               ExecutionMirror mirror = thisTest.RunScriptSource(code);

               thisTest.Verify("z1", new object[] { 5, 6 });
               });
           }*/
        [Test]
        public void T72_Dictionarytypeconversion()
        {

            String code =
            @"
               
            a = { 1, 2, 3 };
            b = {""x"",""y""};
                
            def foo(a1 : var[], b1 : var[])
            {

                a1[b1] = true;
                return =a1;
            }
            z1 = foo(a, b);
            x = z1[b];
            ";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", new object[] { true, true });



        }
        [Test]
        public void T73_Dictionaryrepguideszip()
        {

            String code =
            @"
               
            a = { { 1 }, { 2 }, { 3 } };
            b = {""x"",""y""};
                
                def foo(a1 : var[], b1 : var)
                            {

                                a1[b1] = true;
                                return =a1;
                            }
                            z1 = foo(a<1>, b<1>);
                x = z1[0][b];
                x2 = z1[1][b];
            ";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", new object[] { true, null });
            thisTest.Verify("x2", new object[] { null, true });



        }
        [Test]
        public void T74_Dictionaryrepguidescartesian()
        {

            String code =
            @"
               
            a = { { 1 }, { 2 }, { 3 } };
            b = {""x"",""y""};
                
            def foo(a1 : var[], b1 : var)
                        {

                            a1[b1] = true;
                            return =a1;
                        }
                        z1 = foo(a<1>, b<2>);
            x = z1[0][0][b]; //true,null
            x1 = z1[0][1][b]; //null ,true
            x2 = z1[1][0][b];//true ,null
            x3 = z1[0][1][b];//null,true

            ";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", new object[] { true, null });
            thisTest.Verify("x1", new object[] { null, true });
            thisTest.Verify("x2", new object[] { true, null });
            thisTest.Verify("x3", new object[] { null, true });



        }
    }
}
