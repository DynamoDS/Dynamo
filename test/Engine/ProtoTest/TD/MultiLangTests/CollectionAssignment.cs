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
i = 
[Imperative]
{
	a = [ [1,2], [3,4] ];
	
	a[1] = [-1,-2,3];
	
	c = a[1][1];
	
	d = a[0];
	
	b = [ 1, 2 ];
	
	b[0] = [2,2];
	e = b[0];
    return [c, d, e];
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedResult2 = { 1, 2 };
            object[] expectedResult3 = { 2, 2 };
            thisTest.Verify("i", new object[] { -2, expectedResult2, expectedResult3 });
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
	a = [ [1,2], [3,4] ];
	
	a[1] = [-1,-2,3];
	
	c = a[1][1];
	
	d = a[0];
	
	b = [ 1, 2 ];
	
	b[0] = [2,2];
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
x = [Associative]
{
	a = [ [1,2,3],[4,5,6] ];
	
	i = [Imperative]
	{
		c = a[0];
		d = a[1][2];
        return [c, d];
	}
	c = i[0];
    d = i[1];
	e = i[0];
    return [c, d, e];
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedResult2 = { 1, 2, 3 };
            thisTest.Verify("x", new object[] { expectedResult2, 6, expectedResult2 });
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
	a = [ [1,2,3],[4,5,6] ];
	
	b = [ a[0], 4 ];
	
	c = b[0];
	
	d = b[1];
	
	e = [ a[0][0], a[0][1], a[1][0] ];
	
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
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T05_Collection_Assignment_Using_Class()
        {
            string code = @"
import(""FFITarget.dll"");
d = [Imperative]
{
	c1 = ArrayMember.Ctor([ 1,2,3 ]);
	return = c1.X;
}
		
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedResult2 = { 1, 2, 3 };
            thisTest.Verify("d", expectedResult2, 0);
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
	a = [ 1,0,0 ];
	i = [Imperative]
	{
		return collection( a, 2, 3 );
	}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedResult2 = { 1, 2, 3 };
            thisTest.Verify("i", expectedResult2, 0);
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
	a = [ 1, foo( 2 ) , 3 ];
	b = 
	[Imperative]
	{
		return [ foo( 4 ), 5, 6 ];
	}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedResult2 = { 1, 2, 3 };
            object[] expectedResult = { 4, 5, 6 };
            thisTest.Verify("a", expectedResult2);
            thisTest.Verify("b", expectedResult);
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
	b = [ [0,2,3], [4,5,6] ];
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
b = i[0];
c = i[1];
d = i[2];
i = [Imperative]
{
	a = [ [1,2,3], [4], [5,6] ];
	b = a[1];
	a[1] = 2;
	a[1] = a[1] + 1;
	a[2] = [7,8];
	c = a[1];
	d = a[2][1];
    return [b, c, d];
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
x1 = x[0];
x2 = x[1];
x = [Associative]
{
	a = 3;
	
	b = [Imperative]
	{
		c = [ 1,2,3 ];
		if( c[1] <= 3 )
		return= c;
	}
	
	b[2] = 4;
	a = b;
	c1 = a[1];
	c2 = a[2];
    return [c1, c2];
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("x1", 2);
            thisTest.Verify("x2", 4);
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
		c = [ [ 1,2,3 ] , [ 4,5,6 ] ] ;
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
pts = [[0,1,2],[0,1,2]];
x = [1,2];
y = [1,2,3];
i = [Imperative]
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
	return pts;
}
p1 = i[1][1];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("p1", 4, 0);

        }

        [Test]
        [Category("SmokeTest")]
        public void T15_2D_Collection_Assignment_Using_While_Loop()
        {
            string code = @"
p1 = [Imperative]
{
	pts = [[0,1,2],[0,1,2]];
	x = [1,2,3];
	y = [1,2,3];
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
	return pts[1][1];
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("p1", 2, 0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T16_Assigning_Class_Collection_Property()
        {
            string error = "1467321 rev 3878: class property specified as an empty array with no rank is becoming null when assigned a collection to it ";
            string code = @"
import(""FFITarget.dll"");
a = ArrayMember.Ctor([1,2,3]);
val = a.X;
val[0] = 100;
t = a.X[0];         
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
a = [1, 2, 3];
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
val = [1,2,3];
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
val = [1,2,3];
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
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T20_Defect_1458567_2()
        {
            string code = @"
import(""FFITarget.dll"");
startPt = DummyPoint.ByCoordinates(1, 1, 0);
endPt   = DummyPoint.ByCoordinates(1, 5, 0);
line_0  = DummyLine.ByStartPointEndPoint(startPt, endPt); 	
x1 = line_0[10].Start.X;
x2 = line_0[0].Start.X;
x3 = line_0.Start.X;
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
a = i[0];
c = i[1];
i = [Imperative]
{
    b = [ ];
    count = 0;
    a = 1..5..2;
    for ( i in a )
    {
        b[count] = i + 1;
        count = count + 1;
    }
	c = b ;
    return [a, c];
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
test = [Imperative]
{
    d = [[]];
    r = c = 0;
    a = [ 0, 1, 2 ];
	b = [ 3, 4, 5 ];
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
	return d;
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
b = [0, 1];
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
	def CreateArray ( x : var[] , i )
	{
		x[i] = i;
		return = x;
	}
test = [Imperative]
{
	test = [ ];
	test = CreateArray ( test, 0 );
	test = CreateArray ( test, 1 );
	return test;
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
    d = [ [ ] ];
    r = c = 0;
    a = [ 0, 1, 2 ];
    b = [ 3, 4, 5 ];
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
        collection = [];	
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
x = createArray ( [ 1, 2, 3, 4 ] );
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { -1, -2 };
            thisTest.Verify("x", v1);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T24_Dynamic_Array_1()
        {
            string code = @"
def foo ( i : int )
{
    X : var[] = 0..i;
    Y : var[];
    Count1;

	Y = [Imperative]
	{
	    Count1 = 0;	    
	    y = [];
	    for ( i in X ) 
	    {
	        y[Count1] = i * -1;
		    Count1 = Count1+1;
	    }          
        return y;	    
	}
	return Y;
}

p = 3;
b2 = foo(p);
p = 4;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v2 = new Object[] { 0, -1, -2, -3, -4 };

            thisTest.Verify("b2", v2);
        }

        [Test]
        [Category("SmokeTest")]
        public void T24_Dynamic_Array_Inside_Function()
        {
            string code = @"
def foo ( d : var[] )
{
    return [Imperative]
    {
	    r = c = 0;
	    a = [ 0, 1, 2 ];
	    b1 = [ 3, 4, 5 ];
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
        return d;
    }
    
}
b = [];
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
    return [Imperative]
    {
	    r = c = 0;
	    a = [ 0, 1, 2 ];
	b1 = [ 3, 4, 5 ];
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
        return = d;
    }
    
}
b = [ [] ];
b = foo ( b ) ;     
a = b;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { new Object[] { 3, 4, 5 }, new Object[] { 4, 5, 6 }, new Object[] { 5, 6, 7 } };
            thisTest.Verify("a", v1);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T24_Dynamic_Array_Passed_As_Int_Array_To_Class_Method()
        {
            // Assert.Fail("1465802 - Sprint 22: rev 2359 : Dynamic array issue : when a dynamic array is passed as a collection method/function it throws unknown datatype invalid exception");
            string code = @"

def  foo : int(i : int[])
{
    return  = i[0] + i[1];
}

b1;b2;b3;
[Associative]
{
    cy=[];
    cy[0]=10;
    cy[1]=12;
    a=cy;
    d=[cy[0],cy[1]];     
    b1=foo(d);//works
    b2=foo(a); //does not work � error: Unknown Datatype Invalid
    b3=foo(cy); //does not work � error: Unknown Datatype Invalid
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("b1", 22);
            thisTest.Verify("b2", 22);
            thisTest.Verify("b3", 22);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T24_Dynamic_Array_Passed_As_Primitive_Array_To_Function()
        {
            // Assert.Fail("1465802 - Sprint 22: rev 2359 : Dynamic array issue : when a dynamic array is passed as a collection method/function it throws unknown datatype invalid exception");
            string code = @"

def  foo : double(i : var[])
{
    return  = i[0] + i[1];
}
b1;b2;b3;
[Associative]
{
    cy=[];
    cy[0]=1;
    cy[1]=1.5;
    a=cy;
    d=[cy[0],cy[1]];     
    b1= foo(d);//works
    b2= foo(a); //does not work � error: Unknown Datatype Invalid
    b3= foo(cy); //does not work � error: Unknown Datatype Invalid
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
cy=[];
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
cy=[];
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
x = [ [ 0, 0 ] , [ 1, 1 ] ];
x[1][2] = 1;
x[2] = [2,2,2,2];
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
    x[2] = [ 2, 2, 2, 2 ];
    return = x;
}
x = [ [ 0, 0 ] , [ 1, 1 ] ];
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
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T25_Adding_Elements_To_Array_Class()
        {
            // Assert.Fail("1465704 - Sprint 22: rev 2346 : Adding elements to array from inside class methods is throwing System.IndexOutOfRangeException exception ");

            string code = @"


def add ( ) 
{
    x = [ [ 0, 0 ] , [ 1, 1 ] ];
    x[1][2] = 1;
    x[2] = [ 2, 2, 2, 2 ];
    return = x;
}
x = add(); // expected { { 0,0 }, { 1, 1, 1 }, {2, 2, 2, 2} }
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v2 = new Object[] { 0, 0 };
            Object[] v3 = new Object[] { 1, 1, 1 };
            Object[] v4 = new Object[] { 2, 2, 2, 2 };
            Object[] v5 = new Object[] { v2, v3, v4 };
            thisTest.Verify("x", v5);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T25_Adding_elements_tomemberofclass_1465704()
        {

            string code = @"
def add ( )
{
    x = [ [ 0, 0 ] , [ 1, 1 ] ];
    x[1][2] = 1;
    x[2] = [ 2, 2, 2, 2 ];
    return = x;
}
x = add(); //x = {{0,0},{1,1,1},{2,2,2,2}}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] a2 = new Object[] { 0, 0 };
            Object[] a3 = new Object[] { 1, 1, 1 };
            Object[] a4 = new Object[] { 2, 2, 2, 2 };
            Object[] a5 = new Object[] { a2, a3, a4 };
            thisTest.Verify("x", a5);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T25_Adding_elements_tomemberofclass_1465704_3()
        {
            string code = @"
def add ( )
{
    x = [ [ 0, 0 ] , [ 1, 1 ] ];
    x[1][2] = 1;
    x[2] = [ 2, false, [ 2, 2 ] ];
        
    return = x;
} 
x = add(); // expected { { 0,0 }, { 1, 1, 1 }, {2, false, {2, 2}} }
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v2 = new Object[] { 0, 0 };
            Object[] v3 = new Object[] { 1, 1, 1 };
            Object[] v4 = new Object[] { 2, false, new Object[] { 2, 2 } };
            Object[] v5 = new Object[] { v2, v3, v4 };
            thisTest.Verify("x", v5);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T25_Adding_elements_tomemberofclass_1465704_4()
        {
            string code = @"

def add ( )
{
    x = [ [ 0, 0 ] , [ 1, 1 ] ];
    x[1][2] = 1;
    x[2] = [ 2, false,[ 2, 2] ];
    return = x;
}
def test(x:var[]..[])
{
    a = x;
    a[3]=1;
    return = a;
}
x = add(); 
z = test(x);
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
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T25_Adding_elements_tomemberofclass_1465704_5()
        {
            string code = @"

def remove (x:var[]..[])
{
    t = Remove(x,1);
    return = t;
}
def add(x:var[]..[])
{
    x[1] = [4,4];
    return = x;
}
x = remove([ [ 0, 0 ] , [ 1, 1 ] ]); 
z = add(x);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 0, 0 };
            Object[] v2 = new Object[] { 4, 4 };
            Object[] v3 = new Object[] { v1, v2 };
            thisTest.Verify("x", new object[] { v1 });
            thisTest.Verify("z", v3);
        }



        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T25_Adding_elements_1465704_7()
        {
            string code = @"
    def add ( )
    {
        x = [ [ 0, 0 ] , [ 1, 1 ] ];
        x[1][2] = 1;
        x[2] = [ 2, false,[ 2, 2] ];
        return = x;
    }
x = [Imperative]
{
    z = add();
    return = z;
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
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T25_Adding_elements_imperative_1465704_8()
        {
            string code = @"
    def add ( )
    {
return = [Imperative]{
        x = [ [ 0, 0 ] , [ 1, 1 ] ];
        z = 0..5;
        for(i in z)
        {
	        x[i] = 1;
        }
        return = x; 
}
    }
a = [Imperative]
{
    return = add();
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] a = new Object[] { 1, 1, 1, 1, 1, 1 };
            thisTest.Verify("a", a);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T25_Adding_elements_imperative_1465704_9()
        {
            string error = "1467309 rev 3786 : Warning:Couldn't decide which function to execute... coming from valid code ";
            string code = @"
    def add ( )
    {
    return = [Imperative]{
        x = [ [ 0, 0 ] , [ 1, 1 ] ];
        z = 5;
        j = 0;
        while ( j<=z)
        {
	        x[j] = 1;
            j = j + 1;
        }
        return = x; 
    }
    }
a = [Imperative]
{
    y = add();
    return = y;
}
";
            thisTest.VerifyRunScriptSource(code, error);
            Object[] a = new Object[] { 1, 1, 1, 1, 1, 1 };
            thisTest.Verify("a", a);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T25_Adding_elements_imperative_1465704_10()
        {
            string code = @"
    def add ( )
    {
        x = [ [ 0, 0 ] , [ 1, 1 ] ];
        x[1][2] = 1;
        x[2] = [ null, false,[ 2, 2] ];
        return = x;
    }
x = [Imperative]
{
    z = add();
    return = z;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v2 = new Object[] { 0, 0 };
            Object[] v3 = new Object[] { 1, 1, 1 };
            Object[] v4 = new Object[] { null, false, new Object[] { 2, 2 } };
            Object[] v5 = new Object[] { v2, v3, v4 };
            thisTest.Verify("x", v5);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T25_dynamic_imperative_1465637_1()
        {
            string code = @"
def foo ( i : int )
{
    Y = null;
    Count1 : int;
    X = 0..i;
    return [Imperative]
    {
	    Y = [0,0,0,0,0];
	    Count1 = 0; 
	    for ( i in X ) 
        {
		    Y[Count1] = i * -1;
		    Count1 = Count1 + 1;
	    }
        return Y;
    }
    
}
p = 4;
a = foo(p);
b1 = a;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] b1 = new Object[] { 0, -1, -2, -3, -4 };
            thisTest.Verify("b1", b1);
        }


        [Test]
        [Category("SmokeTest")]
        public void T26_Defct_DNL_1459616()
        {
            string code = @"
a=1;
a=[a,2];";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 1, 2 };
            thisTest.Verify("a", v1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T26_Defct_DNL_1459616_2()
        {
            string code = @"
a=[1,2];
b = [Imperative]
{
    return [a,2];
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { new Object[] { 1, 2 }, 2 };
            thisTest.Verify("b", v1);
        }

        [Test]
        [Category("SmokeTest")]
        public void T26_Defct_DNL_1459616_3()
        {
            string code = @"
def foo (b:var[]..[])
{
    b =  [ b[1], b[1] ];
    return = b;
}
c = foo([1, 2]);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 2, 2 };
            thisTest.Verify("c", v1);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T26_Defct_DNL_1459616_4()
        {
            string code = @"
def foo1()
{
    a = [ a, a ];
    return = a;	
}
def foo2()
{
    b = [ b[0], b[0], b ];
	return = b;
}
t1 = foo1();
c = [Imperative]
{
    t2  = foo2();
    return = [ t1, t2 ];
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { null, null };
            Object[] v2 = new Object[] { null, null, null };
            Object[] v3 = new Object[] { v1, v2 };
            thisTest.Verify("c", v3);
        }

        [Test]
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
        a = [ a, a ];
        x = a;	
    }
    def foo ()
    {
        b = [ b[0], b[0], b ];
	return = b;
    }
}
a=[1,2];
x1 = A.A();
c = [Imperative]
{
    b = [ 0, 1 ];
    t1 = x1.x;
    t2  = x1.foo();
    return = [ t1, t2 ];
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
b = [ ]; // Note : b = { 0, 0} works fine
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
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T27_defect_1464429_DynamicArray_inline()
        {
            string code = @"
	def CreateArray ( x : var[] , i )
	{
		smallest1  =  i>1?i*i:i;
		x[i] = smallest1;
		return = x;
	}

b = [ ]; // Note : b = { 0, 0} works fine
count = 0..2;
t2 = CreateArray( b, count );
t1=b;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] t1 = new Object[] { };
            Object n1 = null;
            Object[] t2 = new Object[] { new Object[] { 0 }, new Object[] { n1, 1 }, new Object[] { n1, n1, 4 } };
            thisTest.Verify("t1", t1);
            thisTest.Verify("t2", t2);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T27_defect_1464429_DynamicArray_class()
        {
            string code = @"

	def CreateArray ( x : var[] , i )
	{
		smallest1  =  i>1?i*i:i;
		x[i] = smallest1;
		return = x;
	}
b = [ ]; // Note : b = { 0, 0} works fine
count = 0..2;
t2 = CreateArray( b, count );
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
b = [ ]; // Note : b = { 0, 0} works fine
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
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T27_defect_1464429_DynamicArray_2()
        {
            string code = @"
def CreateArray (  i :int)
{
    y =[];
	y[i] = i;
	return = y;
}
count = 0..2;
t2 = CreateArray(count );
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("t2", new object [] { new object [] { 0 }, new object [] { null, 1 }, new object[] { null, null, 2 } });
            thisTest.Verify("count", new[] { 0, 1, 2 });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T27_DynamicArray_1465802_Argument()//not
        {
            string code = @"

def foo : int(i:int[])
{
	return = i[0] + i[1];
}

b1;b2;b31;
[Associative]
{
cy=[];
cy[0]=10;
cy[1]=12;
a=cy;
d=[cy[0],cy[1]];
b1 = foo(cy);
b2 = foo(d);
b31 = foo(a);
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            int b = 22;

            thisTest.Verify("b1", b);
            thisTest.Verify("b2", b);
            thisTest.Verify("b31", b);
        }

        [Category("DSDefinedClass_Ported")]
        public void T27_DynamicArray_Class_1465802_Argument_2()
        {
            string code = @"

	def foo : int(i:int[])
	{
		return = 1;
	}
[Associative]
{
cy=[];
cy[0]=10;
cy[1]=null;
b1=foo(cy);
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            int b = 1;
            thisTest.Verify("b1", b);
        }


        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T27_DynamicArray_Class_1465802_member()//not
        {
            string code = @"

	def foo : int(i:int[])
	{
		return = i[0] + i[1];
	}

a1;b1;c1;
[Associative]
{
cy=[];
cy[0]=10;
cy[1]=12;
a=cy;
d=[cy[0],cy[1]];
a1 = foo(cy);
b1 = foo(d);
c1 = foo(a);
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
a=[];
b=a[2];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object b = null;
            thisTest.Verify("b", b);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T27_DynamicArray_Invalid_Index_1465614_2()
        {
            string code = @"
import(""FFITarget.dll"");
baseLineCollection = [ ];
basePoint = [ ]; // replace this with ""basePoint = { 0, 0};"", and it works fine
nsides = 2;
a = 0..nsides - 1..1;
basePnt = [Imperative]
{
    for(i in a)
    {
        basePoint[i] = DummyPoint.ByCoordinates(i, i, 0);
    }
    for(i in a)
    {
        baseLineCollection[i] = DummyLine.ByStartPointEndPoint(basePoint[i], basePoint[i+1]);
    }
    return basePoint;
}
x=basePnt[0].X;
y=basePnt[0].Y;
z=basePnt[0].Z;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 0.0);
            thisTest.Verify("y", 0.0);
            thisTest.Verify("z", 0.0);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T27_DynamicArray_Invalid_Index_1467104()
        {
            string code = @"
import(""FFITarget.dll"");
pts = ClassFunctionality.ClassFunctionality( [ 1, 2] );
aa = pts[null].IntVal;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object aa = null;
            thisTest.Verify("aa", aa);

        }

        [Test]
        public void T27_DynamicArray_Invalid_Index_1467104_2()
        {
            string code = @"
class Point
{
x : var[];
constructor Create(xx : double)
{
	x = [xx,1];
}
}
aa;
[Imperative]
{
pts = Point.Create( [ 1, 2] );
aa = pts[null].x[null];
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object aa = null;
            thisTest.Verify("aa", aa);

        }

        [Test]
        public void T27_DynamicArray_Invalid_Index_1467104_3()
        {
            string code = @"
class Point
{
x : var[];
constructor Create(xx : double)
{
	x = [xx,1];
}
}
aa;aa1;
[Imperative]
{
	pts = Point.Create( [ 1, 2] );
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
def CreateArray ( x : var[] , i )
{
x[i] = i;
return = x;
}
test = [Imperative]
{
test = [ ];
test = CreateArray ( test, 0 );
test = CreateArray ( test, 1 );
return test;
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
a = i[0];
r = i[1];
    def test (i:int)
    {
        return [Imperative]
        {
            loc = [];
            for(j in [i])
            {
                loc[j] = j;
            }
            return loc;
        }
    }
i = [Imperative]
{
    a=[3,4,5];
    t = test(a);
    r = [t[0][3], t[1][4], t[2][5]];
    return [a, r];
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] a = new Object[] { 3, 4, 5 };
            thisTest.Verify("a", a);
            thisTest.Verify("r", a);
        }

        [Test]
        [Category("SmokeTest")]
        public void T29_DynamicArray_Using_Out_Of_Bound_Indices()
        {
            string code = @"
   
    basePoint = [  ];
    
    basePoint [ 4 ] =3;
    test = basePoint;
    
    a = basePoint[0] + 1;
    b = basePoint[ 4] + 1;
    c = basePoint [ 8 ] + 1;
    
    d = [ 0,1 ];
    e1 = d [ 8] + 1;
    
    x = [ ];
    y = [ ];        
    i = [Imperative]
    {
        k = [ ];
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
	    return [y, k];
    }
    t = i[1];
    z = i[0];
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
x = [ 1, 2 ];
x[foo()] = 3;
y = x;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] x = new Object[] { 3, 2 };
            Object[] y = new Object[] { 3, 2 };
            thisTest.Verify("x", x);
            thisTest.Verify("y", y);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T40_Index_usingFunction_1467064_2()
        {
            string code = @"

def foo()
{    
	return = 0;
}

x = [ 1, 2 ];
x[foo()] = 3;
a = x;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] x = new Object[] { 3, 2 };
            Object[] a = new Object[] { 3, 2 };
            thisTest.Verify("x", x);
            thisTest.Verify("a", a);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T40_Index_byFunction_imperative_1467064_3()
        {
            string code = @"

def foo()
{    
	return = 2;
}
x1 = [Imperative]
{
x = [ 1, 2 ];
x[foo()] = 3;
return = x;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] x = new Object[] { 1, 2, 3 };
            thisTest.Verify("x1", x);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T40_Index_byFunction_argument_1467064_4()
        {
            string code = @"

def foo(y:int)
{    
	return = y;
}

x1 = [Imperative]
{
x = [ 1, 2 ];
x[foo(1)] = 3;
return = x;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] x = new Object[] { 1, 3 };
            thisTest.Verify("x1", x);
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
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T41_Accessing_Non_Existent_Properties_From_Array_Elements()
        {
            string code = @"
import(""FFITarget.dll"");
c = [ TestObjectA.TestObjectA(0), TestObjectB.TestObjectB(1) ];
d = c[1].a; 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            // Assert.Fail("1467083 - Sprint23 : rev 2681 : error expected when accessing nonexistent properties of array elements!");
            Object v1 = null;
            thisTest.Verify("d", v1);

        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T41_Accessing_Non_Existent_Property_FromArray_1467083()
        {
            string code = @"
import(""FFITarget.dll"");
c = [ TestObjectA.TestObjectA(0), TestObjectB.TestObjectB(1) ];
c0 = c[0].a;//0
d = c[1].a;//null
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object a = null;
            TestFrameWork.Verify(mirror, "c0", 0);
            TestFrameWork.Verify(mirror, "d", a);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T41_Accessing_Non_Existent_Property_FromArray_1467083_2()
        {
            string code = @"
import(""FFITarget.dll"");
c = [ TestObjectA.TestObjectA(0), TestObjectB.TestObjectB(1), 1 ];
e = c[2].IntVal;
e1 = c[2].IntVal;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object a = null;
            TestFrameWork.Verify(mirror, "e", a);
            TestFrameWork.Verify(mirror, "e1", a);
        }

        [Test]
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
d=[A.A(0),null];
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
        [Category("DSDefinedClass_Ported")]
        [Category("Variable resolution"), Category("Failure")]
        public void T61_Accessing_Non_Existent_Array_Properties_1467082()
        {
            //Assert.Fail("1467094 - Sprint 24 : Rev 2748 : in some cases if try to access nonexisting item in an array it does not return null ) ");
            string code = @"
import(""FFITarget.dll"");
c = [ ClassFunctionality.ClassFunctionality(0), ClassFunctionality.ClassFunctionality(1) ];
p = [];
d = p[1];
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
t3 = p[0];
t4 = p[1];
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
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T61_Accessing_Non_Existent_Array_Properties_1467082_2()
        {
            // Assert.Fail("");
            string code = @"
import(""FFITarget.dll"");
c = [ ClassFunctionality.ClassFunctionality(0), ClassFunctionality.ClassFunctionality(1) ];
p = [];
q=p[0].IntVal;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification 
            object a = null;
            thisTest.Verify("q", a);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T61_Accessing_Non_Existent_Array_Properties_1467082_3()
        {
            // Assert.Fail("");
            string code = @"
import(""FFITarget.dll"");
c = [ ClassFunctionality.ClassFunctionality(0), ClassFunctionality.ClassFunctionality(1) ];
p = [];
q=p[0].IntVal;
c[0]=0;
p=c[0].IntVal; // access as if its propoerty of the class, but thevalue is not class 
r=c[0][0].IntVal;// non existing index 
s=c[0].IntVal[0];// access non array variable as if its array ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification 
            object a = null;
            thisTest.Verify("p", a);
            thisTest.Verify("q", a);
            thisTest.Verify("r", a);
            thisTest.Verify("s", a);
        }

        [Test]
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
c = [ A.A([0,1]), A.A([2]) ];
d = [ A.A([0,1]), A.A([2]) ];
e = [ A.A([0,1]), A.A([2]) ];
f = [ A.A([0,1]), A.A([2]) ];
c[1].x=5;// wrong index 
d[1].x=[0,1]; // entire row 
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
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T61_Assign_Non_Existent_Array_Properties_1467094()
        {
            // Assert.Fail("");
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
t4=p[1];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //thisTest.VerifyWarning(ProtoCore.BuildData.WarningID.kIdUnboundIdentifier);    
            //Verification 
            object a = null;
            thisTest.Verify("t4", a);

        }

        [Test]
        [Category("SmokeTest"), Category("Failure")]
        public void T62_Create_Dynamic_Array_OnTheFly01()
        {
            string code = @"
b;
x=[Imperative]
{
    for (i in 0..7)
    {
        b[i] = i;
    }
    return i;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 7);
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
z=[Imperative]
{
    b[5]=0;
    for (i in 0..7)
    {
        b[i] = i;
    }
    return b;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("z", new Object[] { 0, 1, 2, 3, 4, 5, 6, 7 });

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
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T62_Create_Dynamic_Array_OnTheFly02()
        {
            string code = @"
def test(a:int)
{
    b:int=10;
	a=b;
    return = b;
}
d=5;
c=test(d[0]);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("d", 5);
            thisTest.Verify("c", 10);

        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("SmokeTest")]
        public void T62_Create_Dynamic_Array_OnTheFly_passargument()
        {
            // Assert.Fail("1467139-Sprint 24 - Rev 2958 - an array created dynamically on the fly and passed as an arguemnt to method it gets garbage collected ");
            string code = @"
def test(a:int[])
{
    b:int=1;
    return = b;
}
d[0]=5;
c=test(d);
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
        [Category("SmokeTest")]
        public void T63_Dynamic_array_onthefly_function_1467139()
        {
            string code = @"
def foo(a:int[])
{
}
x[0]=5;
a = foo(x);
c = [100];
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
z[0]=[1];
z=5;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
        }

        [Test]
        public void T64_Modify_itemInAnArray_1467093()
        {
            string code = @"
a = [1, 2, 3];
a[1] = a; 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new Object[] { 1, new Object[] { 1, 2, 3 }, 3 });
        }

        [Test]
        public void T64_Modify_itemInAnArray_1467093_2()
        {
            string code = @"
a = i[0];
b = i[1];
c = i[2];
i = [Imperative]
{
    a = [];
    b = a;
    a[0] = b;
    //hangs here
    c = a;
    return [a,b,c];
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
a = [0,1,2,3];
b=a;
a[0]=9;
b[0]=10;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new Object[] { 9, 1, 2, 3 });
            thisTest.Verify("b", new Object[] { 10, 1, 2, 3 });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T65_Array_Alias_ByVal_1467165_2()
        {
            string code = @"
import(""FFITarget.dll"");
a = [TestObjectA.TestObjectA(),TestObjectB.TestObjectB()];
b=a;
a[0].a = 100;
b[0].a= 200;
c=a[0].a;
d=b[0].a;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", 200);
            thisTest.Verify("d", 200);
        }

        [Test]
        public void T65_Array_Alias_ByVal_1467165_3()
        {
            string code = @"
a = [0,1,2,3];
b=a;
a[0]=9;
b[0]=false;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new Object[] { 9, 1, 2, 3 });
            thisTest.Verify("b", new Object[] { false, 1, 2, 3 });

        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Design Issue")]
        public void T65_Array_Alias_ByVal_1467165_4()
        {
            //Assert.Fail("1467182 - Sprint 25 - [Design Decision] Rev 3163 - method resolution or type conversion is expected in following cases ");
            string code = @"
import(""FFITarget.dll"");
a = [TestObjectA.TestObjectA(),TestObjectB.TestObjectB()];
b=a;
a[0].a = 100;
b[0].a = ""false"";
c=a[0].a;
d=b[0].a;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);


        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T65_Array_Alias_ByVal_1467165_5()
        {
            string code = @"
import(""FFITarget.dll"");
a = [TestObjectA.TestObjectA(),TestObjectB.TestObjectB()];
b=a;
a[0].IntVal = 100;
b[0].IntVal = null;
c=a[0].IntVal;
d=b[0].IntVal;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", null);
            thisTest.Verify("d", null);
        }

        [Test]
        public void T65_Array_Alias_ByVal_1467165_6()
        {
            string code = @"
a = [0,1,2,3];
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
        public void T66_Array_CannotBeUsedToIndex1467069()
        {
            string code = @"
x = [Imperative]
{
    a = [3,1,2]; 
    x = [10,11,12,13,14,15]; 
    x[a] = 2;
    return x;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", new Object[] { 10, 2, 2, 2, 14, 15 });
        }

        [Test]
        [Category("Replication")]
        public void T66_Array_CannotBeUsedToIndex1467069_2()
        {
            string code = @"
x=[Imperative]
{
    a = [3,1,2]; 
    x = [10,11,12,13,14,15]; 
    x[a] = 2;
    return x;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", new Object[] { 10, 2, 2, 2, 14, 15 });
        }

        [Test]
        public void T67_Array_Remove_Item()
        {
            string code = @"
a=[1,2,3,4,5,6,7];
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
a=[1,2,3,4,5,6,7];
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
a = [ 1, 2, 3];
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
@"a = [ ];
a[-1] = 1;
b = a;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", new Object[] { 1 });
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        [Category("Update")]
        public void T44_Defect_1467264()
        {
            String code =
@"
import(""FFITarget.dll"");
a1 = [ ArrayMember.Ctor(1..2), ArrayMember.Ctor(2..3) ];
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
@"arr = [ [ ] ];
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
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
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
@"arr = [ [ ] ];
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
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
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
    arr = [ [ ] ];
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
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
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
    arr = [ [ ] ];
    [Imperative]
    {
        x = [0, 1];
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
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
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
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
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
test = [Imperative]
{
    // create arr
    arr = {};
    for(i in (0..1))
    {
        arr[i] = i;
    }   
    return arr;
}
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
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
arr = [ ]; 
test = [Imperative]
{
    // create arr
    for(i in (0..1))
    {
        arr[i] = i;
    }   
    return arr;
}
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
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
    arr = [ ] ;
    // create arr
    for(i in (0..1))
    {
        arr[i] = i;
    }  
    return= arr; 
}
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
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
    arr = [ ] ;    
    for(i in (0..1))
    {
        arr[i][i] = i;
    }
    return = arr;   
}
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
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
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
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
test = [Imperative]
{
    arr = null ;    
    for(i in (0..1))
    {
        arr[i][i] = i;
    }
    return arr;   
}
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
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
arr = [ ];
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
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
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
arr = [ [], []];
test = [Imperative]
{
    //arr = null ;    
    for(i in (0..1))
    {
        arr[i][i] = i;
    }
    return arr;   
}
";
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            Object[] v1 = new Object[] { new Object[] { 0 }, new Object[] { n1, 1 } };
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
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            Object[] v1 = new Object[] { new Object[] { 0 }, new Object[] { n1, 1 } };
            ;
            thisTest.Verify("test", v1);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T46_Defect_1467502_9_2()
        {
            String code =
@"

    def foo ()
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
test = foo();
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
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
def foo ()
{
    arr = [ [], []];
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
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            Object[] v1 = new Object[] { new Object[] { 0 }, new Object[] { n1, 1 } };
            ;
            thisTest.Verify("test", v1);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T46_Defect_1467502_9_4()
        {
            String code =
@"
def foo ()
{
    arr;
    t = [Imperative]
    {
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
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            Object[] v1 = new Object[] { new Object[] { 0 }, new Object[] { n1, 1 } };
            ;
            thisTest.Verify("test", v1);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T46_Defect_1467502_9_5()
        {
            String code =
@"
import(""FFITarget.dll"");
def foo ()
{
    arr = [ [], []];
    t = [Imperative]
    {
        for(i in (0..1))
        {
            arr[i][i] = ClassFunctionality.ClassFunctionality(0);
        }
        return = arr;   
    }
    return = t;
}
test = foo().IntVal;
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            Object[] v1 = new Object[] { new Object[] { 0 }, new Object[] { n1, 0 } };
            ;
            thisTest.Verify("test", v1);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T46_Defect_1467502_9_6()
        {
            String code =
@"
import(""FFITarget.dll"");
def foo ()
{
    t = [Imperative]
    {
        arr  ;    
        for(i in (0..1))
        {
            arr[i][i] = ClassFunctionality.ClassFunctionality(0);
        }
        return = arr;   
    }
    return = t;
}
test = foo().IntVal;
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            Object[] v1 = new Object[] { new Object[] { 0 }, new Object[] { n1, 0 } };

            thisTest.Verify("test", v1);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T47_Defect_1467561_1()
        {
            String code =
@"
import(""FFITarget.dll"");
def foo ()
{
    t = [Imperative]
    {
        arr  ;    
        for(i in (0..1))
        {
            if ( i < 3 )
            {
                arr[i][i] = ClassFunctionality.ClassFunctionality(0);
            }
        }
        return = arr;   
    }
    return = t;
}
test = foo().IntVal;
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            Object[] v1 = new Object[] { new Object[] { 0 }, new Object[] { n1, 0 } };
            thisTest.Verify("test", v1);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T47_Defect_1467561_2()
        {
            String code =
@"
import(""FFITarget.dll"");
def foo ()
{
    t = [Imperative]
    {
        arr = [] ;    
        for(i in (0..1))
        {
            if ( i < 3 )
            {
                arr[i][i] = ClassFunctionality.ClassFunctionality(0);
            }
        }
        return = arr;   
    }
    return = t;
}
test = foo().IntVal;
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            Object n1 = null;
            Object[] v1 = new Object[] { new Object[] { 0 }, new Object[] { n1, 0 } };
            thisTest.Verify("test", v1);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T47_Defect_1467561_3()
        {
            String code =
@"
import(""FFITarget.dll"");
def foo ()
{
    arr;
    t = [Imperative]
    {
        for(i in (0..1))
        {
            if ( i < 3 )
            {
                arr[i][i] = ClassFunctionality.ClassFunctionality(0);
            }
        }
        return = arr;   
    }
    return = t;
}
test = foo().IntVal;
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
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
                a = [1, 2, 3];
                b=""x"";
                a[b] = 4;
                r = a [b];
            ";

            //ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //thisTest.Verify("r", 4);
            thisTest.RunAndVerifyRuntimeWarning(code, ProtoCore.Runtime.WarningID.InvalidArrayIndexType);
        }
        [Test]
        public void T49_DictionaryvariableArray()
        {
            String code =
            @"
                b=[""x"",""y""];
                a = Dictionary.ByKeysValues(b, [4,7]);
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
                b=""x"";
                a = {""x"" : (b!=null)?4:-4};
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
                a = [1, 2, 3];
                b=[1,-1];
                a[b] =(b>0)?4:-4;
                r = a [b];
            ";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", new object[] { 4, -4 });
        }

        [Test]
        public void T53_DictionaryKeyUpdate()
        {

            String code =
            @"
                r = a[b];
                b = ""y"";
                a ={ ""y"": 10};
            ";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 10);
        }

        [Test]
        public void T54_DictionaryKeyUpdate_2()
        {
            String code =
            @"
               a = [1, 2, 3];
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
        [Category("DSDefinedClass_Ported")]
        public void T59_DotOperator()
        {

            String code =
            @"
            def test(c:int)
            {
                a = {""x"" : c};
                return = a;
            }
                
            z = test(5);
            r = z[""x""];
            ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", 5);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void T60_DictionaryDotOperator()
        {
            String code =
            @"
def foo(c:int)
{
    a = { ""x"" : c, ""y"" : c + 1};
    return = a;
}
y = foo(5);
b = [""x"",""y""];
x = y[b];
            ";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", new object[] { 5, 6 });
        }

        [Test]
        public void T69_DictionaryDynamicArray()
        {
            String code =
            @"
                b = [""x"",""y""];
                a = {""x"": 5, ""y"" : 6};
                r = a[b];
            ";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r", new object[] { 5, 6 });

        }
        [Test]
        public void T70_DictionaryImperativeWhile()
        {

            String code =
            @"
               
           a = Dictionary.ByKeysValues([], []);                  
i = [Imperative]
{
    i =0;
    while (i<5)
    {
        a = a.SetValueAtKeys("""" + i, i);
        i = i + 1;
    }
    return a;
}
r0 = i[""0""];
r1 = i[""1""];
r2 = i[""2""];
r3 = i[""3""];
r4 = i[""4""];
r5 = i[""5""];

                
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
               
a=Dictionary.ByKeysValues([], []);                   
i=[Imperative]
{
    i = 0;
    b = [ ""x"",""true"",""1""];
    for(i in b)
    {
        a = a.SetValueAtKeys(i, i);
    }
    return a;
}
r0 = i[""x""];
r1 = i[""true""];
r2 = i[""1""];
                
            ";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("r0", "x");
            thisTest.Verify("r1", "true");
            thisTest.Verify("r2", "1");


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
            // Dictionary expression keys not (yet) supported
            String code =
            @"
               
            b = [""x"",""y""];

            def foo(b1)
            {
                return Dictionary.ByKeysValues(b1, true);
            }
            z1 = foo(b);
            x = z1[b];
            ";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", new object[] { true, true });
        }

        [Test]
        public void T73_Dictionaryrepguideszip()
        {
            // Dictionary expression keys not (yet) supported
            String code =
            @"
               
            b = [""x"",""y""];

            def foo(b1 : var)
            {
                return Dictionary.ByKeysValues(b1, true);
            }
            z1 = foo(b < 1 >);
            x = z1[0][b];
            x2 = z1[1][b];
            ";

            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", new object[] { true, null });
            thisTest.Verify("x2", new object[] { null, true });
        }
        
    }
}
