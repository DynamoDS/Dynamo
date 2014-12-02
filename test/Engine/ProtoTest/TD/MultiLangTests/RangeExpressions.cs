using System;
using System.Collections.Generic;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;
namespace ProtoTest.TD.MultiLangTests
{
    class RangeExpressions : ProtoTestBase
    {
        [Test]
        [Category("SmokeTest")]
        public void T01_SimpleRangeExpression()
        {
            string src = @"a;a1;a2;a3;a4;a5;a6;a7;a8;a9;a10;a11;a12;a13;a14;a17;
[Imperative]
{
	a = 1..-6..-2;
	a1 = 2..6..~2.5; 
	a2 = 0.8..1..0.2; 
	a3 = 0.7..1..0.3; 
	a4 = 0.6..1..0.4; 
	a5 = 0.8..1..0.1; 
	a6 = 1..1.1..0.1; 
	a7 = 9..10..1; 
	a8 = 9..10..0.1;
	a9 = 0..1..0.1; 
	a10 = 0.1..1..0.1;
	a11 = 0.5..1..0.1;
	a12 = 0.4..1..0.1;
	a13 = 0.3..1..0.1;
	a14 = 0.2..1..0.1;
	a17 = (0.5)..(0.25)..(-0.25);
	
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            List<Object> result = new List<Object> { 1, -1, -3, -5 };
            Assert.IsTrue(mirror.CompareArrays("a", result, typeof(System.Double)));
            List<Object> result0 = new List<Object> { 2, 4, 6 };
            Assert.IsTrue(mirror.CompareArrays("a1", result0, typeof(System.Double)));
            List<Object> result1 = new List<Object> { 0.8, 1 };
            Assert.IsTrue(mirror.CompareArrays("a2", result1, typeof(System.Double)));
            List<Object> result2 = new List<Object> { 0.7, 1 };
            Assert.IsTrue(mirror.CompareArrays("a3", result2, typeof(System.Double)));
            List<Object> result3 = new List<Object> { 0.6, 1 };
            Assert.IsTrue(mirror.CompareArrays("a4", result3, typeof(System.Double)));
            List<Object> result4 = new List<Object> { 0.8, 0.9, 1 };
            Assert.IsTrue(mirror.CompareArrays("a5", result4, typeof(System.Double)));
            List<Object> result5 = new List<Object> { 1, 1.1 };
            Assert.IsTrue(mirror.CompareArrays("a6", result5, typeof(System.Double)));
            List<Object> result6 = new List<Object> { 9, 10 };
            Assert.IsTrue(mirror.CompareArrays("a7", result6, typeof(System.Double)));
            List<Object> result7 = new List<Object> { 9, 9.1, 9.2, 9.3, 9.4, 9.5, 9.6, 9.7, 9.8, 9.9, 10 };
            Assert.IsTrue(mirror.CompareArrays("a8", result7, typeof(System.Double)));
            List<Object> result8 = new List<Object> { 0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1 };
            Assert.IsTrue(mirror.CompareArrays("a9", result8, typeof(System.Double)));
            List<Object> result9 = new List<Object> { 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1 };
            Assert.IsTrue(mirror.CompareArrays("a10", result9, typeof(System.Double)));
            List<Object> result10 = new List<Object> { 0.5, 0.6, 0.7, 0.8, 0.9, 1 };
            Assert.IsTrue(mirror.CompareArrays("a11", result10, typeof(System.Double)));
            List<Object> result11 = new List<Object> { 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1 };
            Assert.IsTrue(mirror.CompareArrays("a12", result11, typeof(System.Double)));
            List<Object> result12 = new List<Object> { 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1 };
            Assert.IsTrue(mirror.CompareArrays("a13", result12, typeof(System.Double)));
            List<Object> result13 = new List<Object> { 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1 };
            Assert.IsTrue(mirror.CompareArrays("a14", result13, typeof(System.Double)));
            List<Object> result14 = new List<Object> { 0.5, 0.25 };
            Assert.IsTrue(mirror.CompareArrays("a17", result14, typeof(System.Double)));
        }

        [Test]
        [Category("SmokeTest")]
        public void T02_SimpleRangeExpression()
        {
            string src = @"a15;a16;a18;a19;a20;
[Imperative]
{
	a15 = 1/2..1/4..-1/4;
	a16 = (1/2)..(1/4)..(-1/4);
	a18 = 1.0/2.0..1.0/4.0..-1.0/4.0;
	a19 = (1.0/2.0)..(1.0/4.0)..(-1.0/4.0);
	a20 = 1..3*2; 
	//a21 = 1..-6;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            List<Object> result = new List<Object> { 0.5, 0.25 };
            Assert.IsTrue(mirror.CompareArrays("a15", result, typeof(System.Double)));
            List<Object> result1 = new List<Object> { 0.5, 0.25 };
            Assert.IsTrue(mirror.CompareArrays("a16", result1, typeof(System.Double)));
            List<Object> result2 = new List<Object> { 0.5, 0.25 };
            Assert.IsTrue(mirror.CompareArrays("a18", result2, typeof(System.Double)));
            List<Object> result3 = new List<Object> { 0.5, 0.25 };
            Assert.IsTrue(mirror.CompareArrays("a19", result3, typeof(System.Double)));
            List<Object> result4 = new List<Object> { 1, 2, 3, 4, 5, 6 };
            Assert.IsTrue(mirror.CompareArrays("a20", result4, typeof(System.Double)));
        }

        [Test]
        public void T03_SimpleRangeExpressionUsingCollection()
        {
            string src = @"w1;w2;w3;w4;w5;
[Imperative]
{
	a = 3 ;
	b = 2 ;
	c = -1;
	w1 = a..b..-1 ; //correct  
	w2 = a..b..c; //correct 
	e1 = 1..2 ; //correct
	f = 3..4 ; //correct
	w3 = e1..f; //correct
	w4 = (3-2)..(w3[1][1])..(c+2) ; //correct
	w5 = (w3[1][1]-2)..(w3[1][1])..(w3[0][1]-1) ; //correct
}
/* expected results : 
    Updated variable a = 3
    Updated variable b = 2
    Updated variable c = -1
    Updated variable w1 = { 3, 2 }
    Updated variable w2 = { 3, 2 }
    Updated variable e1 = { 1, 2 }
    Updated variable f = { 3, 4 }
    Updated variable w3 = { { 1, 2, 3 }, { 2, 3, 4 } }
    Updated variable w4 = { 1, 2, 3 }
    Updated variable w5 = { 1, 2, 3 }
*/
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("w1", new object[] { 3, 2 });
            thisTest.Verify("w2", new object[] { 3, 2 });
            thisTest.Verify("w3", new object[] { new object[] { 1, 2, 3 }, new object[] { 2, 3, 4 } });
            thisTest.Verify("w4", new object[] { 1, 2, 3 });
            thisTest.Verify("w5", new object[] { 1, 2, 3 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T04_SimpleRangeExpressionUsingFunctions()
        {
            string src = @"z1;z2;z3;z4;z5;z7;
[Imperative]
{
	def twice : double( a : double ) 
	{
		return = 2 * a;
	}
	z1 = 1..twice(4)..twice(1);
	z2 = 1..twice(4)..twice(1)-1;
	z3 = 1..twice(4)..(twice(1)-1);
	z4 = 2*twice(1)..1..-1;
	z5 = (2*twice(1))..1..-1;
	//z6 = z5 - z2 + 0.3;
	z7 = (z3[0]+0.3)..4..#1 ; 
   
}
/*
Succesfully created function 'twice' 
    Updated variable z1 = { 1, 3, 5, 7 }
    Updated variable z2 = { 1, 2, 3, ... , 6, 7, 8 }
    Updated variable z3 = { 1, 2, 3, ... , 6, 7, 8 }
    Updated variable z4 = { 4, 3, 2, 1 }
    Updated variable z5 = { 4, 3, 2, 1 }
    //Updated variable z6 = { 3.3, 1.3, -1.7, -2.7 }
    Updated variable z7 = { 1.3 }
	*/";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            List<Object> result = new List<Object> { 1, 3, 5, 7 };
            Assert.IsTrue(mirror.CompareArrays("z1", result, typeof(System.Double)));
            List<Object> result1 = new List<Object> { 1, 2, 3, 4, 5, 6, 7, 8 };
            Assert.IsTrue(mirror.CompareArrays("z2", result1, typeof(System.Double)));
            List<Object> result2 = new List<Object> { 1, 2, 3, 4, 5, 6, 7, 8 };
            Assert.IsTrue(mirror.CompareArrays("z3", result2, typeof(System.Double)));
            List<Object> result3 = new List<Object> { 4, 3, 2, 1 };
            Assert.IsTrue(mirror.CompareArrays("z4", result3, typeof(System.Double)));
            List<Object> result4 = new List<Object> { 4, 3, 2, 1 };
            Assert.IsTrue(mirror.CompareArrays("z5", result4, typeof(System.Double)));
            List<Object> result8 = new List<Object> { 1.3 };
            Assert.IsTrue(mirror.CompareArrays("z7", result8, typeof(System.Double)));
        }

        [Test]
        [Category("SmokeTest")]
        public void T05_RangeExpressionWithIncrement()
        {
            string src = @"d;e1;f;g;h;i;j;k;l;m;
[Imperative]
{
	d = 0.9..1..0.1;
	e1 = -0.4..-0.5..-0.1;
	f = -0.4..-0.3..0.1;
	g = 0.4..1..0.2;
	h = 0.4..1..0.1;
	i = 0.4..1;
	j = 0.6..1..0.4;
	k = 0.09..0.1..0.01;
	l = 0.2..0.3..0.05;
	m = 0.05..0.1..0.04;
	n = 0.1..0.9..~0.3;
	k = 0.02..0.03..#3;
	l = 0.9..1..#5;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            List<Object> result3 = new List<Object> { 0.9, 1 };
            Assert.IsTrue(mirror.CompareArrays("d", result3, typeof(System.Double)));
            List<Object> result4 = new List<Object> { -0.4, -0.5 };
            Assert.IsTrue(mirror.CompareArrays("e1", result4, typeof(System.Double)));
            List<Object> result5 = new List<Object> { -0.4, -0.3 };
            Assert.IsTrue(mirror.CompareArrays("f", result5, typeof(System.Double)));
            List<Object> result6 = new List<Object> { 0.4, 0.6, 0.8, 1 };
            Assert.IsTrue(mirror.CompareArrays("g", result6, typeof(System.Double)));
            List<Object> result7 = new List<Object> { 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1 };
            Assert.IsTrue(mirror.CompareArrays("h", result7, typeof(System.Double)));
            List<Object> result8 = new List<Object> { 0.4 };
            Assert.IsTrue(mirror.CompareArrays("i", result8, typeof(System.Double)));
            List<Object> result9 = new List<Object> { 0.6, 1 };
            Assert.IsTrue(mirror.CompareArrays("j", result9, typeof(System.Double)));
            List<Object> result10 = new List<Object> { 0.02, 0.025, 0.03 };
            Assert.IsTrue(mirror.CompareArrays("k", result10, typeof(System.Double)));
            List<Object> result11 = new List<Object> { 0.9, 0.925, 0.95, 0.975, 1 };
            Assert.IsTrue(mirror.CompareArrays("l", result11, typeof(System.Double)));
            List<Object> result12 = new List<Object> { 0.05, 0.09 };
            Assert.IsTrue(mirror.CompareArrays("m", result12, typeof(System.Double)));
        }

        [Test]
        [Category("SmokeTest")]
        public void T06_RangeExpressionWithIncrement()
        {
            string src = @"a;b;c;
[Imperative]
{
	a = 0.3..0.1..-0.1;
	b = 0.1..0.3..0.2;
	c = 0.1..0.3..0.1;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            List<Object> result = new List<Object> { 0.3, 0.2, 0.1 };
            Assert.IsTrue(mirror.CompareArrays("a", result, typeof(System.Double)));
            List<Object> result1 = new List<Object> { 0.1, 0.3 };
            Assert.IsTrue(mirror.CompareArrays("b", result1, typeof(System.Double)));
            List<Object> result2 = new List<Object> { 0.1, 0.2, 0.3 };
            Assert.IsTrue(mirror.CompareArrays("c", result2, typeof(System.Double)));
        }

        [Test]
        [Category("SmokeTest")]
        public void T07_RangeExpressionWithIncrementUsingFunctionCall()
        {
            string code = @"
d;f;
[Imperative]
{
	def increment : double[] (x : double[]) 
	{
		j = 0;
		for( i in x )
		{
			x[j] = x[j] + 1 ;
			j = j + 1;
		}
		return = x;
	}
	a = {1,2,3};
	b = {3,4,5} ;
	c = {1.5,2.5,4,3.65};
	f = {7,8*2,9+1,5-3,-1,-0.34};
	//nested collection
	d = {3.5,increment(c)};
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            List<Object> l1 = new List<object> { 3.5, new List<Object> { 2.5, 3.5, 5, 4.65 } };
            List<Object> l2 = new List<object> { 7, 16, 10, 2, -1, -0.34 };
            Assert.IsTrue(mirror.CompareArrays("d", l1, typeof(System.Double)));
            Assert.IsTrue(mirror.CompareArrays("f", l2, typeof(System.Int64)));
        }

        [Test]
        public void T08_RangeExpressionWithIncrementUsingVariables()
        {
            string src = @"h;i;j;k;l;
[Imperative]
{
	def square : double ( x :double ) 
	{
		return = x * x;
	}
	z = square(4);
	x = 1 ;
	y = -2 ;
	a = 1..2 ;
	b = 1..6..3;
	c = 2..3..1;
	d = 2..10..2;
	e1 = 1..3..0.5;
	f = 2..4..0.2 ;
	//using variables
	h = z..3..-4;
	i = 1..z..x;
	j = z..x..y; 
	k = a..b..x ;
	l = a..c..x ;
	//using function call 
	g = 6..9.5..square(-1);
	m = 0.8..square(1)..0.1; 
	n = square(1)..0.8..-0.1;
	o = 0.8..square(0.9)..0.01; 
}
/*
result
z = 16
x = 1
y = -2
a = {1,2}
b = {1,4}
c = {2,3}
d = {2,4,6,8,10}
e1 = {1.000000,1.500000,2.000000,2.500000,3.000000}
f = {2.000000,2.200000,2.400000,2.600000,2.800000,3.000000,3.200000,3.400000,3.600000,3.800000,4.000000}
h = {16,12,8,4}
i = {1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16}
j = {16,14,12,10,8,6,4,2}
k = {{1},{2,3,4}}
l = {{1,2},{2,3}}
g = {6.000000,7.000000,8.000000,9.000000}
m = {0.800000,0.900000,1.000000}
n = {1.000000,0.900000,0.800000}
o = {0.800000,0.810000}
*/";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("h", new object[] { 16.0, 12.0, 8.0, 4.0 });
            thisTest.Verify("i", new object[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0, 11.0, 12.0, 13.0, 14.0, 15.0, 16.0 });
            thisTest.Verify("j", new object[] { 16.0, 14.0, 12.0, 10.0, 8.0, 6.0, 4.0, 2.0 });
            thisTest.Verify("k", new object[] { new object[] { 1 }, new object[] { 2, 3, 4 } });
            thisTest.Verify("l", new object[] { new object[] { 1, 2 }, new object[] { 2, 3 } });
        }

        [Test]
        [Category("SmokeTest")]
        public void T09_RangeExpressionWithApproximateIncrement()
        {
            string src = @"a;b;f;g;h;j;k;l;
[Imperative]
{
	def square : double ( x: double ) 
	{
		return = x * x;
	}
	
	x = 0.1; 
	a = 0..2..~0.5;
	b = 0..0.1..~square(0.1);
	f = 0..0.1..~x;      
	g = 0.2..0.3..~x;    
	h = 0.3..0.2..~-0.1; 
	
	j = 0.8..0.5..~-0.3;
	k = 0.5..0.8..~0.3; 
	l = 0.2..0.3..~0.0;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            List<Object> result3 = new List<Object> { 0.0, 0.5, 1.0, 1.5, 2.0 };
            Assert.IsTrue(mirror.CompareArrays("a", result3, typeof(System.Double)));
            List<Object> result4 = new List<Object> { 0, 0.01, 0.02, 0.03, 0.04, 0.05, 0.06, 0.07, 0.08, 0.09, 0.1, };
            Assert.IsTrue(mirror.CompareArrays("b", result4, typeof(System.Double)));
            List<Object> result5 = new List<Object> { 0, 0.1 };
            Assert.IsTrue(mirror.CompareArrays("f", result5, typeof(System.Double)));
            List<Object> result6 = new List<Object> { 0.2, 0.3 };
            Assert.IsTrue(mirror.CompareArrays("g", result6, typeof(System.Double)));
            List<Object> result7 = new List<Object> { 0.3, 0.2 };
            Assert.IsTrue(mirror.CompareArrays("h", result7, typeof(System.Double)));
            List<Object> result9 = new List<Object> { 0.8, 0.5 };
            Assert.IsTrue(mirror.CompareArrays("j", result9, typeof(System.Double)));
            List<Object> result10 = new List<Object> { 0.5, 0.8 };
            Assert.IsTrue(mirror.CompareArrays("k", result10, typeof(System.Double)));
        }

        [Test]
        [Category("Replication")]
        public void T10_RangeExpressionWithReplication()
        {
            //Assert.Fail("1454507 - Sprint15 : Rev 666 : Nested range expressions are throwing NullReferenceException ");
            //Assert.Fail("Replication guides are not implmented");
            string src = @"a;b;
[Imperative]
{
	//step value greater than the end value
	a = 1..2..3;
	b = 3..4..3;
	c = a..b..a[0]; // {{1,2,3}}
	d = 0.5..0.9..0.1;
	e1 = 0.1..0.2..0.05;
	f = e1[1]..d[2]..0.5;
	g = e1..d..0.2;
	h = e1[2]..d[1]..0.5;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            List<Object> result = new List<Object>() { 1 };
            Assert.IsTrue(mirror.CompareArrays("a", result, typeof(System.Double)));
            List<Object> result1 = new List<Object>() { 3 };
            Assert.IsTrue(mirror.CompareArrays("b", result1, typeof(System.Double)));
            //List<Object> result2 = new List<Object>() { { 1, 2, 3 } };
            //Assert.IsTrue(mirror.CompareArrays("c", result2, typeof(System.Double)));
            // List<Object> result3 = new List<Object>() { { { 0.100 }, { 0.100, 0.600 }, { 0.100, 0.600 }, { 0.100, 0.600 }, { 0.100, 0.600 } }, { { 0.150 }, { 0.150 }, { 0.150, 0.650 }, { 0.150, 0.650 }, { 0.150, 0.650 } }, { { 0.200 }, { 0.200 }, { 0.200, 0.700 }, { 0.200, 0.700 }, { 0.200, 0.700 } } };
            // Assert.IsTrue(mirror.CompareArrays("f", result3, typeof(System.Double)));
            // List<Object> result4 = new List<Object>() { { { 0.100 }, { 0.150 }, { 0.200 } }, { { 0.100, 0.600 }, { 0.150 }, { 0.200 } }, { { 0.100, 0.600 }, { 0.150, 0.650 }, { 0.200, 0.700 } }, { { 0.100, 0.600 }, { 0.150, 0.650 }, { 0.200, 0.700 } }, { { 0.100, 0.600 }, { 0.150, 0.650 }, { 0.200, 0.700 } } };
            // Assert.IsTrue(mirror.CompareArrays("h", result4, typeof(System.Double)));
        }

        [Test]
        [Category("SmokeTest")]
        public void T11_RangeExpressionUsingClasses()
        {
            string src = @"class point
{
		x : var[];
		
		constructor point1(a : int[])
		{
			x = a;
		}
		
		def foo(a : int)
		{
			return = a;
		}
}
def foo1(a : int)
		{
			return = a;
		}
def foo2(a : int[])
		{
			return = a[2];
		}
a1;a2;a3;a4;
[Imperative]
{
	x1 = 1..4;
	//x1 = { 1, 2, 3, 4 };
	a = point.point1(x1);
	a1 = a.x;
	a2 = a.foo(x1);	
	a3 = foo1(x1[0]);
	a4 = foo2(x1);
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue((Int64)mirror.GetValue("a3").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetValue("a4").Payload == 3);
            List<Object> result = new List<Object> { 1, 2, 3, 4 };
            Assert.IsTrue(mirror.CompareArrays("a1", result, typeof(System.Int64)));
            Assert.IsTrue(mirror.CompareArrays("a2", result, typeof(System.Int64)));
        }

        [Test]
        public void T12_RangeExpressionUsingNestedRangeExpressions()
        {
            string src = @"b;c;d;e1;f;g;h;i;j;
[Imperative]
{
	x = 1..5..2; // {1,3,5}
	y = 0..6..2; // {0,2,4,6}
	a = (3..12..3)..(4..16..4); // {3,6,9,12} .. {4..8..12..16}
	b = 3..00.6..#5;      // {3.0,2.4,1.8,1.2,0.6}
	//c = b[0]..7..#1;    //This indexed case works
	c = 5..7..#1;         //Compile error here , 5
	d = 5.5..6..#3;       // {5.5,5.75,6.0}
	e1 = -6..-8..#3;      //{-6,-7,-8}
	f = 1..0.8..#2;       //{1,0.8}
	g = 1..-0.8..#3;      // {1.0,0.1,-0.8}
	h = 2.5..2.75..#4;    //{2.5,2.58,2.67,2.75}
	i = x[0]..y[3]..#10;//1..6..#10
	j = 1..0.9..#4;// {1.0, 0.96,.93,0.9}
	k= 1..3..#0;//null
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("b", new object[] { 3.0, 2.4, 1.8, 1.2, 0.6 });
            thisTest.Verify("c", new object[] { 5 });
            thisTest.Verify("d", new object[] { 5.5, 5.75, 6.0 });
            thisTest.Verify("e1", new object[] { -6, -7, -8 });
            thisTest.Verify("f", new object[] { 1.0, 0.8 });
            thisTest.Verify("g", new object[] { 1.0, 0.1, -0.8 });
            thisTest.Verify("h", new object[] { 2.5, 2.5 + 0.25 / 3.0, 2.5 + 0.5 / 3.0, 2.75 });
            thisTest.Verify("i", new Object[] { 1.000000, 1.555556, 2.111111, 2.666667, 3.222222, 3.777778, 4.333333, 4.888889, 5.444444, 6.000000 });
            thisTest.Verify("j", new object[] { 1.0, 1.0 - 0.1 / 3.0, 1.0 - 0.2 / 3.0, 0.9 });
        }

        [Test]
        [Category("SmokeTest")]
        public void T13_RangeExpressionWithStartEndValuesUsingFunctionCall()
        {
            string src = @"x;b;c;e1;f;g;
[Imperative]
{
	def even : double (a : int) 
	{
		if((a % 2)>0)
		return = (a+(a * 0.5));
		else
		return = (a-(a * 0.5));
	}
	d = 3;
	x = 1..2..#d;
	a = even(2) ;
	b = 1..a;
	c = even(3)..even(5)..#6;
	d = even(5)..even(6)..#4;
	e1 = e..4..#3;  //e takes default value 2.17
	f = even(3)..(even(8)+4*0.5)..#3;
	g = even(2)+1..1..#5;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            List<Object> result3 = new List<Object> { 1, 1.5, 2.0 };
            Assert.IsTrue(mirror.CompareArrays("x", result3, typeof(System.Double)));
            List<Object> result4 = new List<Object> { 1 };
            Assert.IsTrue(mirror.CompareArrays("b", result4, typeof(System.Double)));
            List<Object> result5 = new List<Object> { 4.5, 5.1, 5.6999999999999993, 6.2999999999999989, 6.8999999999999986, 7.4999999999999982 };
            Assert.IsTrue(mirror.CompareArrays("c", result5, typeof(System.Double)));
            Assert.IsTrue(mirror.GetValue("e1").DsasmValue.IsNull);
            List<Object> result9 = new List<Object> { 4.5, 5.25, 6.0 };
            Assert.IsTrue(mirror.CompareArrays("f", result9, typeof(System.Double)));
            List<Object> result10 = new List<Object> { 2.0, 1.75, 1.5, 1.25, 1.0 };
            Assert.IsTrue(mirror.CompareArrays("g", result10, typeof(System.Double)));
        }

        [Test]
        [Category("SmokeTest")]
        public void T14_RangeExpressionUsingClassMethods()
        {
            string src = @"class collection
{
	x : var[];
	constructor Create(a: int)
	{
		x = a..(a+3)..#4;
	}
	
	public def get_x()
	{
		return = x;
	}
}
b;
[Imperative]
{
	a = collection.Create(5);
	
	b = a.get_x();
	
}
     
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            List<Object> result = new List<Object> { 5, 6, 7, 8 };
            Assert.IsTrue(mirror.CompareArrays("b", result, typeof(System.Double)));
        }

        [Test]
        [Category("SmokeTest")]
        public void T15_SimpleRangeExpression_1()
        {
            string src = @"a;b;d;f;g;h;i;l;
[Imperative]
{
	a = 1..2.2..#3;
	b = 0.1..0.2..#4;
	c = 1..3..~0.2;
	d = (a[0]+1)..(c[2]+0.9)..0.1; 
	e1 = 6..0.5..~-0.3;
	f = 0.5..1..~0.3;
	g = 0.5..0.6..0.01;
	h = 0.51..0.52..0.01;
	i = 0.95..1..0.05;
	j = 0.8..0.99..#10;
	//k = 0.9..1..#1;
	l = 0.9..1..0.1;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            List<Object> result = new List<Object> { 1, 1.6, 2.2 };
            Assert.IsTrue(mirror.CompareArrays("a", result, typeof(System.Double)));
            List<Object> result1 = new List<Object> { 0.1, 0.13333333333333333, 0.16666666666666666, 0.2 };
            Assert.IsTrue(mirror.CompareArrays("b", result1, typeof(System.Double)));
            List<Object> result2 = new List<Object> { 2, 2.1, 2.2, 2.3000000000000003 };
            Assert.IsTrue(mirror.CompareArrays("d", result2, typeof(System.Double)));
            List<Object> result3 = new List<Object> { 0.5, 0.75, 1 };
            Assert.IsTrue(mirror.CompareArrays("f", result3, typeof(System.Double)));
            List<Object> result4 = new List<Object> { 0.5, 0.51, 0.52, 0.53, 0.54, 0.55, 0.56, 0.57, 0.58, 0.59, 0.6 };
            Assert.IsTrue(mirror.CompareArrays("g", result4, typeof(System.Double)));
            List<Object> result5 = new List<Object> { 0.51, 0.52 };
            Assert.IsTrue(mirror.CompareArrays("h", result5, typeof(System.Double)));
            List<Object> result6 = new List<Object> { 0.95, 1 };
            Assert.IsTrue(mirror.CompareArrays("i", result6, typeof(System.Double)));
            //List<Object> result7 = new List<Object>() { 0.9 };
            ////Assert.IsTrue(mirror.CompareArrays("k", result7, typeof(System.Double)));
            List<Object> result8 = new List<Object> { 0.9, 1 };
            Assert.IsTrue(mirror.CompareArrays("l", result8, typeof(System.Double)));
        }

        [Test]
        [Category("SmokeTest")]
        public void T16_SimpleRangeExpression_2()
        {
            string src = @"a;b;c;d;e1;f;g;h;
[Imperative]
{
	a = 1.2..1.3..0.1;
	b = 2..3..0.1;
	c = 1.2..1.5..0.1;
	//d = 1.3..1.4..~0.5; //incorrect 
	d = 1.3..1.4..0.5; 
	e1 = 1.5..1.7..~0.2;
	f = 3..3.2..~0.2;
	g = 3.6..3.8..~0.2; 
	h = 3.8..4..~0.2; 
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            List<Object> result = new List<Object> { 1.2, 1.3 };
            Assert.IsTrue(mirror.CompareArrays("a", result, typeof(System.Double)));
            List<Object> result1 = new List<Object> { 2, 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7, 2.8, 2.9, 3 };
            Assert.IsTrue(mirror.CompareArrays("b", result1, typeof(System.Double)));
            List<Object> result2 = new List<Object> { 1.2, 1.3, 1.4, 1.5 };
            Assert.IsTrue(mirror.CompareArrays("c", result2, typeof(System.Double)));
            List<Object> result3 = new List<Object> { 1.3 };
            Assert.IsTrue(mirror.CompareArrays("d", result3, typeof(System.Double)));
            List<Object> result5 = new List<Object> { 1.5, 1.7 };
            Assert.IsTrue(mirror.CompareArrays("e1", result5, typeof(System.Double)));
            List<Object> result6 = new List<Object> { 3.0, 3.2 };
            Assert.IsTrue(mirror.CompareArrays("f", result6, typeof(System.Double)));
            List<Object> result7 = new List<Object> { 3.6, 3.8 };
            Assert.IsTrue(mirror.CompareArrays("g", result7, typeof(System.Double)));
            List<Object> result8 = new List<Object> { 3.8, 4.0 };
            Assert.IsTrue(mirror.CompareArrays("h", result8, typeof(System.Double)));
        }

        [Test]
        [Category("SmokeTest")]
        public void T17_SimpleRangeExpression_3()
        {
            string src = @"a;b;c;d;e1;f;g;h;i;j;k;
[Imperative]
{
	a = 1..2.2..~0.2;
	b = 1..2..#3;
	c = 2.3..2..#3;
	d = 1.2..1.4..~0.2;
	e1 = 0.9..1..0.1;
	f = 0.9..0.99..~0.01;
	g = 0.8..0.9..~0.1;
	h = 0.8..0.9..0.1;
	i = 0.9..1.1..0.1;
	j = 1..0.9..-0.05;
	k = 1.2..1.3..~0.1;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            List<Object> result = new List<Object> { 1, 1.2, 1.4, 1.6, 1.8, 2, 2.2 };
            Assert.IsTrue(mirror.CompareArrays("a", result, typeof(System.Double)));
            List<Object> result1 = new List<Object> { 1, 1.5, 2 };
            Assert.IsTrue(mirror.CompareArrays("b", result1, typeof(System.Double)));
            List<Object> result2 = new List<Object> { 2.3, 2.15, 2 };
            Assert.IsTrue(mirror.CompareArrays("c", result2, typeof(System.Double)));
            List<Object> result3 = new List<Object> { 1.2, 1.4 };
            Assert.IsTrue(mirror.CompareArrays("d", result3, typeof(System.Double)));
            List<Object> result5 = new List<Object> { 0.9, 1 };
            Assert.IsTrue(mirror.CompareArrays("e1", result5, typeof(System.Double)));
            List<Object> result6 = new List<Object> { 0.9, 0.91, 0.92, 0.93, 0.94, 0.95, 0.96, 0.97, 0.98, 0.99 };
            Assert.IsTrue(mirror.CompareArrays("f", result6, typeof(System.Double)));
            List<Object> result7 = new List<Object> { 0.8, 0.9 };
            Assert.IsTrue(mirror.CompareArrays("g", result7, typeof(System.Double)));
            List<Object> result8 = new List<Object> { 0.8, 0.9 };
            Assert.IsTrue(mirror.CompareArrays("h", result8, typeof(System.Double)));
            List<Object> result9 = new List<Object> { 0.9, 1, 1.1 };
            Assert.IsTrue(mirror.CompareArrays("i", result9, typeof(System.Double)));
            List<Object> result10 = new List<Object> { 1, 0.95, 0.9 };
            Assert.IsTrue(mirror.CompareArrays("j", result10, typeof(System.Double)));
            List<Object> result11 = new List<Object> { 1.2, 1.3 };
            Assert.IsTrue(mirror.CompareArrays("k", result11, typeof(System.Double)));
        }

        [Test]
        [Category("SmokeTest")]
        public void T18_SimpleRangeExpression_4()
        {
            string src = @"a;b;c;d;e1;f;g;h;
[Imperative]
{
	a = 2.3..2.6..0.3;
	b = 4.3..4..-0.3;
	c= 3.7..4..0.3;
	d = 4..4.3..0.3;
	e1 = 3.2..3.3..0.3;
	f = 0.4..1..0.1;
	g = 0.4..0.45..0.05;
	h = 0.4..0.45..~0.05; 
	g = 0.4..0.6..~0.05;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            List<Object> result = new List<Object> { 2.3, 2.6 };
            Assert.IsTrue(mirror.CompareArrays("a", result, typeof(System.Double)));
            List<Object> result1 = new List<Object> { 4.3, 4 };
            Assert.IsTrue(mirror.CompareArrays("b", result1, typeof(System.Double)));
            List<Object> result2 = new List<Object> { 3.7, 4 };
            Assert.IsTrue(mirror.CompareArrays("c", result2, typeof(System.Double)));
            List<Object> result3 = new List<Object> { 4, 4.3 };
            Assert.IsTrue(mirror.CompareArrays("d", result3, typeof(System.Double)));
            List<Object> result5 = new List<Object> { 3.2 };
            Assert.IsTrue(mirror.CompareArrays("e1", result5, typeof(System.Double)));
            List<Object> result6 = new List<Object> { 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1 };
            Assert.IsTrue(mirror.CompareArrays("f", result6, typeof(System.Double)));
            List<Object> result7 = new List<Object> { 0.4, 0.45, 0.5, 0.55, 0.6 };
            Assert.IsTrue(mirror.CompareArrays("g", result7, typeof(System.Double)));
            List<Object> result8 = new List<Object> { 0.4, 0.45 };
            Assert.IsTrue(mirror.CompareArrays("h", result8, typeof(System.Double)));
        }

        [Test]
        [Category("SmokeTest")]
        public void T19_SimpleRangeExpression_5()
        {
            string src = @"b;c;d;e1;f;g;h;i;
[Imperative]
{
	//a = 0.1..0.2..#1; //giving error
	b = 0.1..0.2..#2;
	c = 0.1..0.2..#3;
	d = 0.1..0.1..#4;
	e1 = 0.9..1..#5;
	f = 0.8..0.89..#3;
	g = 0.9..0.8..#3;
	h = 0.9..0.7..#5;
	i = 0.6..1..#4;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            //List<Object> result = new List<Object>() { 0.1 };
            //Assert.IsTrue(mirror.CompareArrays("a", result, typeof(System.Double)));
            List<Object> result1 = new List<Object> { 0.1, 0.2 };
            Assert.IsTrue(mirror.CompareArrays("b", result1, typeof(System.Double)));
            List<Object> result2 = new List<Object> { 0.1, 0.15, 0.2 };
            Assert.IsTrue(mirror.CompareArrays("c", result2, typeof(System.Double)));
            List<Object> result3 = new List<Object> { 0.1, 0.1, 0.1, 0.1 };
            Assert.IsTrue(mirror.CompareArrays("d", result3, typeof(System.Double)));
            List<Object> result5 = new List<Object> { 0.9, 0.925, 0.95, 0.975, 1 };
            Assert.IsTrue(mirror.CompareArrays("e1", result5, typeof(System.Double)));
            List<Object> result6 = new List<Object> { 0.8, 0.845, 0.89 };
            Assert.IsTrue(mirror.CompareArrays("f", result6, typeof(System.Double)));
            List<Object> result7 = new List<Object> { 0.9, 0.85, 0.8 };
            Assert.IsTrue(mirror.CompareArrays("g", result7, typeof(System.Double)));
            List<Object> result8 = new List<Object> { 0.9, 0.85, 0.8, 0.75, 0.7 };
            Assert.IsTrue(mirror.CompareArrays("h", result8, typeof(System.Double)));
            List<Object> result9 = new List<Object> { 0.6, 0.73333333333333328, 0.8666666666666667, 1.0 };
            Assert.IsTrue(mirror.CompareArrays("i", result9, typeof(System.Double)));
        }

        [Test]
        [Category("SmokeTest")]
        public void T20_RangeExpressionsUsingPowerOperator()
        {
            string src = @"e1;f;
[Imperative]
{
	def power : double (a:double,b:int) 
	{
		temp = 1;
		while( b > 0 )
		{
			temp = temp * a;
			b = b - 1;
		}
		return = temp;
	}
	a = 3;
	b = 2; 
	c = power(2,3);
	d = b..a;
	e1 = b..c..power(2,1);
	f = power(1.0,1)..power(2,2)..power(0.5,1);   
	/*h = power(0.1,2)..power(0.2,2)..~power(0.1,2);
	i = power(0.1,1)..power(0.2,1)..~power(0.1,1);         has not been implemented yet
	j = power(0.4,1)..power(0.45,1)..~power(0.05,1);
	k = power(1.2,1)..power(1.4,1)..~power(0.2,1);
	l = power(1.2,1)..power(1.3,1)..~power(0.1,1); 
	m = power(0.8,1)..power(0.9,1)..~power(0.1,1);
	n = power(0.08,1)..power(0.3,2)..~power(0.1,2); */
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            List<Object> result = new List<Object> { 2, 4, 6, 8 };
            Assert.IsTrue(mirror.CompareArrays("e1", result, typeof(System.Double)));
            List<Object> result1 = new List<Object> { 1.0, 1.5, 2.0, 2.5, 3.0, 3.5, 4.0 };
            Assert.IsTrue(mirror.CompareArrays("f", result1, typeof(System.Double)));
            /*List<Object> result2 = new List<Object>() { 0.01,0.02,0.03,0.04 };
            Assert.IsTrue(mirror.CompareArrays("h", result2, typeof(System.Double)));
            List<Object> result3 = new List<Object>() { 0.1, 0.2 };
            Assert.IsTrue(mirror.CompareArrays("i", result3, typeof(System.Double)));
            List<Object> result5 = new List<Object>() { 0.4,0.45 };
            Assert.IsTrue(mirror.CompareArrays("j", result5, typeof(System.Double)));
            List<Object> result6 = new List<Object>() { 1.2,1.4 };
            Assert.IsTrue(mirror.CompareArrays("k", result6, typeof(System.Double)));
            List<Object> result7 = new List<Object>() { 1.2,1.3 };
            Assert.IsTrue(mirror.CompareArrays("l", result7, typeof(System.Double)));
            List<Object> result8 = new List<Object>() { 0.8,0.9 };
            Assert.IsTrue(mirror.CompareArrays("m", result8, typeof(System.Double)));
            List<Object> result9 = new List<Object>() { 0.08, 0.09 };
            Assert.IsTrue(mirror.CompareArrays("n", result9, typeof(System.Double)));*/
        }

        [Test]
        [Category("SmokeTest")]
        public void T21_RangeExpressionsUsingEvenFunction()
        {
            string src = @"c;d;e1;f;g;
[Imperative]
{
	def even : int (a : int) 
	{	
		if(( a % 2 ) > 0 )
			return = a + 1;
		
		else 
			return = a;
	}
	x = 1..3..1;
	y = 1..9..2;
	z = 11..19..2;
	c = even(x); // 2,2,4
	d = even(x)..even(c)..(even(0)+0.5); // {2,2,4}
	e1 = even(y)..even(z)..1; // {2,4,6,8,10} .. {12,14,16,18,20}..1
	f = even(e1[0])..even(e1[1]); // even({2,3,4,5,6,7,8,9,10,11,12} ..even({4,5,6,7,8,9,10,11,12,13,14})
   /*  {2,4,4,6,6,8,8,10,10,12,12} .. {4,6,6,8,8,10,10,12,12,14,14}
*/ 
	g = even(y)..even(z)..f[0][1];  // {2,4,6,8,10} .. {12,14,16,18,20} .. 3
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Object[] c = new Object[] { 2, 2, 4 };
            Object[] d = new Object[] { new Object[] { 2.000000 }, new Object[] { 2.000000 }, new Object[] { 4.000000 } };
            Object[][] e1 = new Object[][] { new Object[] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, new Object[] { 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, new Object[] { 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }, new Object[] { 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 }, new Object[] { 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 } };
            Object[][] f = new Object[][] { new Object[] { 2, 3, 4 }, new Object[] { 4, 5, 6 }, new Object[] { 4, 5, 6 }, new Object[] { 6, 7, 8 }, new Object[] { 6, 7, 8 }, new Object[] { 8, 9, 10 }, new Object[] { 8, 9, 10 }, new Object[] { 10, 11, 12 }, new Object[] { 10, 11, 12 }, new Object[] { 12, 13, 14 }, new Object[] { 12, 13, 14 } };
            Object[][] g = { new Object[] { 2, 5, 8, 11 }, new Object[] { 4, 7, 10, 13 }, new Object[] { 6, 9, 12, 15 }, new Object[] { 8, 11, 14, 17 }, new Object[] { 10, 13, 16, 19 } };
            thisTest.Verify("c", c);
            thisTest.Verify("d", d);
            thisTest.Verify("e1", e1);
            thisTest.Verify("f", f);
            thisTest.Verify("g", g);
            //List<Object> result = new List<Object>()  {2,2,4};
            // Assert.IsTrue(mirror.CompareArrays("d", result, typeof(System.Double)));
        }

        [Test]
        [Category("SmokeTest")]
        public void T22_RangeExpressionsUsingClassMethods_2()
        {
            string src = @"class addition
{
	a : var[];
	constructor Create( y : int[] )
	{
		a = y;
	}
	def get_col ( x : int )
	{
		a[0] = x;
		return = a; 
	}
}
d;
[Imperative]
{
	a = 2..10..2;
	c = addition.Create( a );
	d = c.get_col( 5 );
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            List<Object> result = new List<Object> { 5, 4, 6, 8, 10 };
            Assert.IsTrue(mirror.CompareArrays("d", result, typeof(System.Int64)));
        }

        [Test]
        public void T23_RangeExpressionsUsingClassMethods_3()
        {
            //string err = "1467069 - Sprint 23: rev 2634: 328588 An array cannot be used to index into an array, must throw warning";
            string err = "";
            string code = @"class compare
{
	a ;
	b ; 
	constructor Create (x, y)
	{
		a = x ;
		b = y ;
	}
	def get_max ()
	{
		return = (a > b) ? a : b ; 
	}
	def get_min ()
	{
		return = (a < b) ? a : b ; 
	}
}
	a = 1..5..1;
	b = 10..2..-2;
	c = compare.Create(a,b); 
	i = 4;
	d = c[0..i].get_max();
	e1 = c[0..i].get_min();
";
            thisTest.VerifyRunScriptSource(code, err);
            List<Object> result2 = new List<Object>() { 10, 16, 18, 16, 10 };
            // List<Object> result5 = new List<Object>() { { 11, 16, 18, 16, 10 }, { 10, 10, 9, 8, 7 }, { 10, 8, 6, 4, 5 }, { 1, 2, 3, 4, 2 } };
            // Assert.IsTrue(mirror.CompareArrays("j", result5, typeof(System.Double)));
            thisTest.Verify("d", new object[] { 10, 8, 6, 4, 5 });
            thisTest.Verify("e1", new object[] { 1, 2, 3, 4, 2 });
        }

        [Test]
        [Category("SmokeTest")]
        public void TA01_RangeExpressionWithIntegerIncrement()
        {
            string src = @"a1;a2;
[Imperative]
{
	a1 = 1..5..2;
	a2 = 12.5..20..2;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            List<Object> result = new List<Object> { 1, 3, 5 };
            Assert.IsTrue(mirror.CompareArrays("a1", result, typeof(System.Double)));
            List<Object> result1 = new List<Object> { 12.5, 14.5, 16.5, 18.5 };
            Assert.IsTrue(mirror.CompareArrays("a2", result1, typeof(System.Double)));
        }

        [Test]
        [Category("SmokeTest")]
        public void TA02_RangeExpressionWithDecimalIncrement()
        {
            string src = @"a1;a2;
[Imperative]
{
	a1 = 2..9..2.7;
	a2 = 10..11.5..0.3;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            List<Object> result = new List<Object> { 2, 4.7, 7.4 };
            Assert.IsTrue(mirror.CompareArrays("a1", result, typeof(System.Double)));
            List<Object> result1 = new List<Object> { 10, 10.3, 10.6, 10.9, 11.2, 11.5 };
            Assert.IsTrue(mirror.CompareArrays("a2", result1, typeof(System.Double)));
        }

        [Test]
        [Category("SmokeTest")]
        public void TA03_RangeExpressionWithNegativeIncrement()
        {
            string src = @"a;b;c;
[Imperative]
{
	a = 10..-1..-2;
	b = -2..-10..-1;
	c = 10..3..-1.5;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            List<Object> result = new List<Object> { 10, 8, 6, 4, 2, 0 };
            Assert.IsTrue(mirror.CompareArrays("a", result, typeof(System.Double)));
            List<Object> result1 = new List<Object> { -2, -3, -4, -5, -6, -7, -8, -9, -10 };
            Assert.IsTrue(mirror.CompareArrays("b", result1, typeof(System.Double)));
            List<Object> result2 = new List<Object> { 10, 8.5, 7, 5.5, 4 };
            Assert.IsTrue(mirror.CompareArrays("c", result2, typeof(System.Double)));
        }

        [Test]
        [Category("SmokeTest")]
        public void TA04_RangeExpressionWithNullIncrement()
        {
            string src = @"a;b;
[Imperative]
{
	a = 1..5..null;
	b = 0..6..(null);
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("a", null);
            thisTest.Verify("b", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void TA05_RangeExpressionWithBooleanIncrement()
        {
            string src = @"a;b;
[Imperative]
{
	a = 2.5..6..(true);
	b = 3..7..false;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("a", null);
            thisTest.Verify("b", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void TA06_RangeExpressionWithIntegerTildeValue()
        {
            string src = @"a;b;
[Imperative]
{
	a = 1..10..~4;
	b = -2.5..10..~5;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            List<Object> result = new List<Object> { 1, 5.5, 10 };
            Assert.IsTrue(mirror.CompareArrays("a", result, typeof(System.Double)));
            List<Object> result1 = new List<Object> { -2.5, 1.6666666666666666666666666667, 5.8333333333333333333333333334, 10 };
            Assert.IsTrue(mirror.CompareArrays("b", result1, typeof(System.Double)));
        }

        [Test]
        [Category("SmokeTest")]
        public void TA07_RangeExpressionWithDecimalTildeValue()
        {
            string code = @"
a;b;
[Imperative]
{
	a = 0.2..0.3..~0.2; //divide by zero error
	b = 6..13..~1.3;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedResult = { 0.2, 0.3 };
            object[] expectedResult2 = { 6.0, 7.4, 8.8, 10.2, 11.6, 13.0 };
            thisTest.Verify("a", expectedResult, 0);
            thisTest.Verify("b", expectedResult2, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TA08_RangeExpressionWithNegativeTildeValue()
        {
            string src = @"a;b;
[Imperative]
{
	a = 3..1..~-0.5;
	b = 18..13..~-1.3;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            List<Object> result = new List<Object> { 3, 2.5, 2, 1.5, 1 };
            Assert.IsTrue(mirror.CompareArrays("a", result, typeof(System.Double)));
            List<Object> result1 = new List<Object> { 18, 16.75, 15.5, 14.25, 13 };
            Assert.IsTrue(mirror.CompareArrays("b", result1, typeof(System.Double)));
        }

        [Test]
        [Category("SmokeTest")]
        public void TA09_RangeExpressionWithNullTildeValue()
        {
            string src = @"a;b;
[Imperative]
{
	a = 1..5..~null;
	b = 5..2..~(null);
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("a", null);
            thisTest.Verify("b", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void TA10_RangeExpressionWithBooleanTildeValue()
        {
            string src = @"a;b;
[Imperative]
{
	a = 1..3..(true);
	b = 2..2..false;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("a", null);
            thisTest.Verify("b", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void TA11_RangeExpressionWithIntegerHashValue()
        {
            string src = @"a;b;c;
[Imperative]
{
	a = 1..3.3..#5;
	b = 3..3..#3;
	c = 3..3..#1;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            List<Object> result = new List<Object> { 1, 1.575, 2.150, 2.725, 3.3 };
            Assert.IsTrue(mirror.CompareArrays("a", result, typeof(System.Double)));
            List<Object> result2 = new List<Object> { 3, 3, 3 };
            Assert.IsTrue(mirror.CompareArrays("b", result2, typeof(System.Double)));
            List<Object> result1 = new List<Object> { 3 };
            Assert.IsTrue(mirror.CompareArrays("c", result1, typeof(System.Double)));
        }

        [Test]
        [Category("SmokeTest")]
        public void TA12_RangeExpressionWithDecimalHashValue()
        {
            string src = @"a;b;
[Imperative]
{
	a = 1..7..#2.5;
	b = 2..10..#2.4;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.VerifyBuildWarningCount(2);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kInvalidRangeExpression);
        }

        [Test]
        [Category("SmokeTest")]
        public void TA13_RangeExpressionWithNegativeHashValue()
        {
            string src = @"a;
[Imperative]
{
	a = 7.5..-2..#-9;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue(mirror.GetValue("a").DsasmValue.IsNull);
        }

        [Test]
        [Category("SmokeTest")]
        public void TA14_RangeExpressionWithNullHashValue()
        {
            string src = @"a;
[Imperative]
{
	a = 2..10..#null;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("a", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void TA15_RangeExpressionWithBooleanHashValue()
        {
            string src = @"a;b;
[Imperative]
{
	b = 12..12..#false;
	a = 12..12..#(true);
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("a", null);
            thisTest.Verify("b", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void TA16_RangeExpressionWithIncorrectLogic_1()
        {
            string src = @"a;
[Imperative]
{
	a = 5..1..2;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue(mirror.GetValue("a").DsasmValue.IsNull);
        }

        [Test]
        [Category("SmokeTest")]
        public void TA17_RangeExpressionWithIncorrectLogic_2()
        {
            string src = @"a;
[Imperative]
{
	a = 5.5..10.7..-2;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            Assert.IsTrue(mirror.GetValue("a").DsasmValue.IsNull);
        }

        [Test]
        [Category("SmokeTest")]
        public void TA18_RangeExpressionWithIncorrectLogic_3()
        {
            string src = @"a;b;c;
[Imperative]
{
	a = 7..7..5;
	b = 8..8..~3;
	c = 9..9..#1;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            List<Object> result1 = new List<Object> { 7 };
            Assert.IsTrue(mirror.CompareArrays("a", result1, typeof(System.Double)));
            List<Object> result2 = new List<Object> { 8 };
            Assert.IsTrue(mirror.CompareArrays("b", result2, typeof(System.Double)));
            List<Object> result3 = new List<Object> { 9 };
            Assert.IsTrue(mirror.CompareArrays("c", result3, typeof(System.Double)));
        }

        [Test]
        [Category("SmokeTest")]
        public void TA19_RangeExpressionWithIncorrectLogic_4()
        {
            string src = @"a;
[Imperative]
{
	a = null..8..2;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            thisTest.Verify("a", null);
        }

        [Test]
        [Category("SmokeTest")]
        public void TA21_Defect_1454692()
        {
            string code = @"
x;
b;
y;
[Imperative]
{
	x = 0;
	b = 0..3; //{ 0, 1, 2, 3 }
	for( y in b )
	{
		x = y + x;
	}
	
}	";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 6, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TA21_Defect_1454692_2()
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
num = length(arr);
	";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("num", 4, 0);
        }

        [Test]
        [Category("SmokeTest")]
        public void TA21_Defect_1454692_3()
        {
            string code = @"
class A
{
	Pts : var[];
	constructor A ( pts : double[] )
	{
	    Pts = pts;
	}
	def length ()
	{
		numPts = [Imperative]
		{
			counter = 0;
			for(pt in Pts)
			{
				counter = counter + 1;
			}
			
			return = counter;
		}
		return = numPts;
	}
}
    
arr = 0.0..3.0;//{0.0,1.0,2.0,3.0};
a1 = A.A(arr);
num = a1.length();
	";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("num", 4, 0);
        }


        [Test]
        [Category("SmokeTest")]
        public void TA21_Defect_1454692_4()
        {
            string code = @"
def foo(i : int[])
{
    count = 0;
	count = [Imperative]
	{
	    for ( x in i )
		{
		    count = count + 1;
		}
		return = count;
	}
	return = count;
	
}
    
arr = 0.0..3.0;//{0.0,1.0,2.0,3.0};
c;x;
[Imperative]
{
	x = 0;
	b = 0..3; //{ 0, 1, 2, 3 }
	for( y in arr )
	{
		x = y + x;
	}
	x1 = 0..3;
	c = foo(x1);
	
}
	";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", 4);
            thisTest.Verify("x", 3);
        }

        [Test]
        public void T25_RangeExpression_WithDefaultDecrement()
        {
            // 1467121
            string code = @"
a=5..1;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] a = new Object[] { 5, 4, 3, 2, 1 };
            thisTest.Verify("a", a);

        }

        [Test]
        public void T25_RangeExpression_WithDefaultDecrement_1467121()
        {
            // 1467121
            string code = @"
a=5..1;
b=-5..-1;
c=1..0.5;
d=1..-0.5;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] a = new Object[] { 5, 4, 3, 2, 1 };
            thisTest.Verify("a", a);
        }

        [Test]
        public void T25_RangeExpression_WithDefaultDecrement_nested_1467121_2()
        {
            // 1467121
            string code = @"
a=(5..1).. (1..5);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[][] a = new Object[][] { new Object[] { 5, 4, 3, 2, 1 }, new Object[] { 4, 3, 2 }, new Object[] { 3 }, new Object[] { 2, 3, 4 }, new Object[] { 1, 2, 3, 4, 5 } };
            //thisTest.Verify("a", a);
        }


        [Test]
        public void T26_RangeExpression_Function_tilda_1457845()
        {
            // 1467121
            string code = @"
x;a;b;f;g;h;j;k;l;m;
[Imperative]
{
	def square : double ( x: double ) 
	{
		return = x * x;
	}
	
	x = 0.1; 
	a = 0..2..~0.5;
	b = 0..0.1..~square(0.1);
	f = 0..0.1..~x;      
	g = 0.2..0.3..~x;    
	h = 0.3..0.2..~-0.1; 
	
	j = 0.8..0.5..~-0.3;
	k = 0.5..0.8..~0.3; 
	l = 0.2..0.3..~0.0;
	m = 0.2..0.3..~1/2; // division 
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] a = new Object[] { 0.000000, 0.500000, 1.000000, 1.500000, 2.000000 };
            Object[] b = new Object[] { 0.000000, 0.010000, 0.020000, 0.030000, 0.040000, 0.050000, 0.060000, 0.070000, 0.080000, 0.090000, 0.100000 };
            Object[] f = new Object[] { 0.000000, 0.100000 };
            Object[] g = new Object[] { 0.200000, 0.300000 };

            Object[] h = new Object[] { 0.300000, 0.200000 };
            Object[] j = new Object[] { 0.800000, 0.500000 };
            Object[] k = new Object[] { 0.500000, 0.800000 };
            Object l = null;
            Object[] m = new Object[] { 0.200000, 0.300000 };

            thisTest.Verify("x", 0.100000);
            thisTest.Verify("a", a);
            thisTest.Verify("b", b);
            thisTest.Verify("f", f);
            thisTest.Verify("g", g);
            thisTest.Verify("h", h);
            thisTest.Verify("j", j);
            thisTest.Verify("k", k);
            thisTest.Verify("l", l);
            thisTest.Verify("m", m);

        }

        [Test]
        public void T26_RangeExpression_Function_tilda_multilanguage_1457845_2()
        {
            // 1467121
            string code = @"
x;a;b;f;g;h;j;k;l;m;
[Associative]
{
	def square : double ( x: double ) 
	{
		return = x * x;
	}
[Imperative]
{
	x = 0.1; 
	a = 0..2..~0.5;
	b = 0..0.1..~square(0.1);
	f = 0..0.1..~x;      
	g = 0.2..0.3..~x;    
	h = 0.3..0.2..~-0.1; 
	
	j = 0.8..0.5..~-0.3;
	k = 0.5..0.8..~0.3; 
	l = 0.2..0.3..~0.0;
	m = 0.2..0.3..~1/2; // division 
}
	}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] a = new Object[] { 0.000000, 0.500000, 1.000000, 1.500000, 2.000000 };
            Object[] b = new Object[] { 0.000000, 0.010000, 0.020000, 0.030000, 0.040000, 0.050000, 0.060000, 0.070000, 0.080000, 0.090000, 0.100000 };
            Object[] f = new Object[] { 0.000000, 0.100000 };
            Object[] g = new Object[] { 0.200000, 0.300000 };
            Object[] h = new Object[] { 0.300000, 0.200000 };
            Object[] j = new Object[] { 0.800000, 0.500000 };
            Object[] k = new Object[] { 0.500000, 0.800000 };
            Object l = null;
            Object[] m = new Object[] { 0.200000, 0.300000 };
            thisTest.Verify("x", 0.100000);
            thisTest.Verify("a", a);
            thisTest.Verify("b", b);
            thisTest.Verify("f", f);
            thisTest.Verify("g", g);
            thisTest.Verify("h", h);
            thisTest.Verify("j", j);
            thisTest.Verify("k", k);
            thisTest.Verify("l", l);
            thisTest.Verify("m", m);
        }

        [Test]
        public void T26_RangeExpression_Function_tilda_associative_1457845_3()
        {
            // 1467121
            string code = @"
x;a;b;f;g;h;j;k;l;m;
[Associative]
{
	def square : double ( x: double ) 
	{
		return = x * x;
	}
}
[Imperative]
{
	x = 0.1; 
	a = 0..2..~0.5;
	b = 0..0.1..~square(0.1);
	f = 0..0.1..~x;      
	g = 0.2..0.3..~x;    
	h = 0.3..0.2..~-0.1; 
	
	j = 0.8..0.5..~-0.3;
	k = 0.5..0.8..~0.3; 
	l = 0.2..0.3..~0.0;
	m = 0.2..0.3..~1/2; // division 
}
	
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] a = new Object[] { 0.000000, 0.500000, 1.000000, 1.500000, 2.000000 };
            Object[] b = null;
            Object[] f = new Object[] { 0.000000, 0.100000 };
            Object[] g = new Object[] { 0.200000, 0.300000 };
            Object[] h = new Object[] { 0.300000, 0.200000 };
            Object[] j = new Object[] { 0.800000, 0.500000 };
            Object[] k = new Object[] { 0.500000, 0.800000 };
            Object l = null;
            Object[] m = new Object[] { 0.200000, 0.300000 };
            thisTest.Verify("x", 0.100000);
            thisTest.Verify("a", a);
            thisTest.Verify("b", b);
            thisTest.Verify("f", f);
            thisTest.Verify("g", g);
            thisTest.Verify("h", h);
            thisTest.Verify("j", j);
            thisTest.Verify("k", k);
            thisTest.Verify("l", l);
            thisTest.Verify("m", m);
        }

        [Test]
        public void T27_RangeExpression_Function_Associative_1463472()
        {
            string code = @"
z1;
[Associative]
{
	def twice : double( a : double )
	{
		return = 2 * a;
	}
	z1 = 1..twice(4)..twice(1);
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] a = new Object[] { 1.000000, 3.000000, 5.000000, 7.000000 };
            thisTest.Verify("z1", a);
        }

        [Test]
        public void T27_RangeExpression_Function_Associative_1463472_2()
        {

            string code = @"
c;
[Associative]
{
	def twice : int []( a : double )
	{
		c=1..a;
		return = c;
	}
d=1..4;
c=twice(4);
//	z1 = 1..twice(4)..twice(1);
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] c = new Object[] { 1, 2, 3, 4 };
            thisTest.Verify("c", c);
        }

        [Test]
        public void T27_RangeExpression_Function_return_1463472()
        {
            string code = @"
c;
[Associative]
{
	def twice : int []( a : double )
	{
		c=1..a;
		return = c;
	}
d=1..4;
c=twice(4);
//	z1 = 1..twice(4)..twice(1);
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] c = new Object[] { 1, 2, 3, 4 };
            thisTest.Verify("c", c);
        }

        [Test]
        public void T27_RangeExpression_class_return_1463472_2()
        {
            string code = @"
class twice
{
	def twice : int []( a : double )
	{
		c=1..a;
		return = c;
	}
}
d=1..4;
a=twice.twice();
c=a.twice(4);
//	z1 = 1..twice(4)..twice(1);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] c = new Object[] { 1, 2, 3, 4 };
            thisTest.Verify("c", c);
        }


        [Test]
        public void T27_RangeExpression_Function_Associative_replication()
        {
            string code = @"
z1;
[Associative]
{
	def twice : int[]( a : int )
	{
		c=2*(1..a);
		return = c;
	}
    d={1,2,3,4};
	z1=twice(d);
//	z1 = 1..twice(4)..twice(1);
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[][] c = new Object[][] { new Object[] { 2 }, new Object[] { 2, 4 }, new Object[] { 2, 4, 6 }, new Object[] { 2, 4, 6, 8 } };
            thisTest.Verify("z1", c);
        }

        [Test]
        public void Regress_1467127()
        {
            string code = @"
i = 1..6..#10;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            double step = 5.0 / 9.0;
            object[] values = new object[10];
            values[9] = 6.0;
            for (int i = 0; i < 9; ++i)
            {
                values[i] = 1.0 + step * i;
            }
            thisTest.Verify("i", values);
        }

        [Test]
        public void TA22_Range_Expression_floating_point_conversion_1467127()
        {
            string code = @"
a = 1..6..#10;
b = 0.1..0.6..#10;
c = 0.01..-0.6..#10;
d= -0.1..0.06..#10;
	";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] a = new Object[] { 1.000000, 1.555556, 2.111111, 2.666667, 3.222222, 3.777778, 4.333333, 4.888889, 5.444444, 6.000000 };
            Object[] b = new Object[] { 0.100000, 0.155556, 0.211111, 0.266667, 0.322222, 0.377778, 0.433333, 0.488889, 0.544444, 0.600000 };
            Object[] c = new Object[] { 0.010000, -0.057778, -0.125556, -0.193333, -0.261111, -0.328889, -0.396667, -0.464444, -0.532222, -0.600000 };
            Object[] d = new Object[] { -0.100000, -0.082222, -0.064444, -0.046667, -0.028889, -0.011111, 0.006667, 0.024444, 0.042222, 0.060000 };
            thisTest.Verify("a", a);
            thisTest.Verify("b", b);
            thisTest.Verify("c", c);
            thisTest.Verify("d", d);
        }

        [Test]
        public void TA22_Range_Expression_floating_point_conversion_1467127_2()
        {
            string code = @"
a = 0..7..~0.75;
b = 0.1..0.7..~0.075;
c = 0.01..-7..~0.75;
d= -0.1..7..~0.75; 
e = 1..-7..~1;
	";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] a = new Object[] { 0.000000, 0.777778, 1.555556, 2.333333, 3.111111, 3.888889, 4.666667, 5.444444, 6.222222, 7.000000 };
            Object[] b = new Object[] { 0.100000, 0.175000, 0.250000, 0.325000, 0.400000, 0.475000, 0.550000, 0.625000, 0.700000 };
            Object[] c = new Object[] { 0.010000 };
            Object[] d = new Object[] { -0.100000, 0.688889, 1.477778, 2.266667, 3.055556, 3.844444, 4.633333, 5.422222, 6.211111, 7.000000 };
            Object[] e = new Object[] { 1 };
            thisTest.Verify("a", a);
            thisTest.Verify("b", b);
            thisTest.Verify("c", c);
            thisTest.Verify("d", d);
            thisTest.Verify("e", e);
        }

        [Test]
        [Category("SmokeTest")]
        public void TA23_Defect_1466085_Update_In_Range_Expr()
        {
            string code = @"
y = 1;
y1 = 0..y;
y = 2;
z1 = y1; 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v = new Object[] { 0, 1, 2 };
            thisTest.Verify("z1", v);
        }

        [Test]
        [Category("SmokeTest")]
        public void TA23_Defect_1466085_Update_In_Range_Expr_2()
        {
            string code = @"
a = 0;
b = 10;
c = 2;
y1 = a..b..c;
a = 7;
b = 14;
c = 7;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v = new Object[] { 7, 14 };
            thisTest.Verify("y1", v);
        }

        [Test]
        [Category("SmokeTest")]
        public void TA23_Defect_1466085_Update_In_Range_Expr_3()
        {
            string code = @"
def foo ( x : int[] )
{
    return = Count(x);
}
a = 0;
b = 10;
c = 2;
y1 = a..b..c;
z1 = foo ( y1 );
z2 = Count( y1 );
c = 5;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            thisTest.Verify("z1", 3);
            thisTest.Verify("z2", 3);
        }

        [Test]
        [Category("SmokeTest")]
        public void IndexingIntoClassInstanceByRangeExpr()
        {
            string code = @"
class A
{
    x;
    constructor A(i)
    {
        x = i;
    }
}
x = (A.A(1..3))[0];
z = x.x;
";
            thisTest.VerifyRunScriptSource(code, "DNL-1467618 Regression : Use of the array index after replicated constructor yields complier error now");
            thisTest.Verify("z", 1);
        }

        [Test]
        public void TA24_1467454_negative_case()
        {
            string code = @"
b = 10.0;
a = 0.0;
d1 = a..b..c;
d2 = c..b..c;
d3 = a..c..b;
d4 = c..a..c;
d5 = c..2*c..c;
";
            Object n1 = null;
            thisTest.VerifyRunScriptSource(code, "");
            thisTest.Verify("d1", n1);
            thisTest.Verify("d2", n1);
            thisTest.Verify("d3", n1);
            thisTest.Verify("d4", n1);
            thisTest.Verify("d5", n1);
            thisTest.VerifyBuildWarningCount(1);
        }

        [Test]
        public void TA24_1467454_negative_case_2()
        {
            string code = @"
b = 10.0;
a = 0.0;
d1;d2;d3;d4;d5;
[Imperative]
{
    d1 = a..b..c;
    d2 = c..b..c;
    d3 = a..c..b;
    d4 = c..a..c;
    d5 = c..2*c..c;
}
";
            Object n1 = null;
            thisTest.VerifyRunScriptSource(code, "");
            thisTest.Verify("d1", n1);
            thisTest.Verify("d2", n1);
            thisTest.Verify("d3", n1);
            thisTest.Verify("d4", n1);
            thisTest.Verify("d5", n1);
            thisTest.VerifyBuildWarningCount(1);
        }

        [Test]
        public void RegressMagn5111()
        {
            string code = @"x = 0..0..360/0;";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", null);
            thisTest.VerifyRuntimeWarningCount(1);
        }

        [Test]
        public void RegressMagn5111_02()
        {
            string code = @"x = 360/0..0..0;";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", null);
            thisTest.VerifyRuntimeWarningCount(1);
        }

        [Test]
        public void RegressMagn5111_03()
        {
            string code = @"x = 0..360/0..0;";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", null);
            thisTest.VerifyRuntimeWarningCount(1);
        }

        [Test]
        public void RangeExpression_Infinity()
        {
            // Crash when step range expression come to infinity.
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-5111

            string code = @"x = 0..0..360/{0,0};";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", new object[] { null, null });
            thisTest.VerifyRuntimeWarningCount(2);
        }


        [Test]
        [Category("SmokeTest")]
        [Category("Failure")]
        public void TestStepZero()
        {
            string src = @"
a = 0;
b = 0..10..a;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(src);
            TestFrameWork.VerifyRuntimeWarning(ProtoCore.RuntimeData.WarningID.kInvalidArguments);
            thisTest.VerifyRuntimeWarningCount(1);
            thisTest.Verify("b", null);
        }
    }
}
