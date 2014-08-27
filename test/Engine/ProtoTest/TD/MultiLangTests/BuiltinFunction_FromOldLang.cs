using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTest.TD;
using ProtoCore.Lang.Replication;
using ProtoTestFx.TD;
namespace ProtoTest.TD.MultiLangTests
{
    class BuiltinFunction_FromOldLang
    {
        public TestFrameWork thisTest = new TestFrameWork();
        string testPath = "..\\..\\..\\Scripts\\TD\\MultiLanguage\\BuiltinFunctionFromOldLang\\";
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void T80580_BuiltinFunc_1()
        {
            string code = @"
import(""DSCoreNodes.dll"");
class TestClass
{
	
	constructor TestClass()
	{
	}
        def testFlatten()
	{
		index = Flatten({1..4,0});
		return = index;
	}
	def testCount()
	{
		a = {true,{false,false},true};
		b = Count(a);
		return = b;
	}	
	def testContains()
	{
		a = {true,{false,false},true};
		b = Contains(a,{false,false});
		return = b;
	}
	def testCountFalse()
	{
		a = {true,{false,false},true};
		b = CountFalse(a);
		return = b;
	}
	def testCountTrue()
	{
		a = {true,{false,false},true};
		b = CountTrue(a);
		return = b;
	}
	def testSomeFalse()
	{
		a = {true,{false,false},true};
		b = SomeFalse(a);
		return = b;
	}
	def testSomeTrue()
	{
		a = {true,{false,false},true};
		b = SomeTrue(a);
		return = b;
	}
	def testToString()
	{
		a = {true,{false,false},true};
		b = ToString(a);
		return = b;
	}
	def testTranspose()
	{
		a = {{3,-4},{4,5}};
		b = Transpose(a);
		return = b;
	}
	def testFloor()
	{
		a = 3.5;
		b = Math.Floor(a);
		return = b;
	}
	def testCeil()
	{
		a = 3.5;
		b = Math.Ceiling(a);
		return = b;
	}
	def testLog()
	{
		b = Math.Log(10);
		return = b;
	}
	def testSqrt()
	{
		b = Math.Sqrt(25);
		return = b;
	}
	def testTan()
	{
		a = 45;
		b = Math.Tan(a);
		return = b;
	}
	def testNormalizeDepth()
	{
		index = NormalizeDepth({{1.1},{{2.3,3}},""5"",{{{{true}}}}},2);
		return = index;
	}
}
test = TestClass.TestClass();
t1 = test.testFlatten();
t2 = test.testCount();
t3 = test.testContains();
t4 = test.testCountFalse();
t5 = test.testCountTrue();
t6 = test.testSomeFalse();
t7 = test.testSomeTrue();
t8 = test.testToString();
t9 = test.testTranspose();
t10 = test.testFloor();
t11 = test.testCeil();
t12 = test.testLog();
t13 = test.testSqrt();
t14 = test.testTan();
t15 = test.testNormalizeDepth();
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 1, 2, 3, 4, 0 };
            Object[] v2 = new Object[] { 3, 4 };
            Object[] v3 = new Object[] { -4, 5 };
            Object[] v4 = new Object[] { v2, v3 };
            Object[] v5 = new Object[] { 1.1 };
            Object[] v6 = new Object[] { 2.3, 3 };
            Object[] v7 = new Object[] { "5" };
            Object[] v8 = new Object[] { true };
            Object[] v9 = new Object[] { v5, v6, v7, v8 };
            thisTest.Verify("t1", v1);
            thisTest.Verify("t2", 3);
            thisTest.Verify("t3", true);
            thisTest.Verify("t4", 2);
            thisTest.Verify("t5", 2);
            thisTest.Verify("t6", true);
            thisTest.Verify("t7", true);
            thisTest.Verify("t8", "{true,{false,false},true}");
            thisTest.Verify("t9", v4);
            thisTest.Verify("t10", 3);
            thisTest.Verify("t11", 4);
            thisTest.Verify("t12", 2.3025850929940459);
            thisTest.Verify("t13", 5.0);
            thisTest.Verify("t14", 1.0);
            thisTest.Verify("t15", v9);
        }

        [Test]
        public void T80581_BuiltinFunc_2()
        {
            string code = @"
import(""DSCoreNodes.dll"");
class TestClass
{
	b;
	b1;
	b2;
	b3;
	b4;
	b5;
	b6;
	b7;
	b8;
	b9;
	b10;
	b11;
	b12;
	b13;
	b14;
	constructor TestClass()
	{
		b = Flatten({1..4,0});
	
		a = {true,{false,false},true};
		b1 = Count(a);
	
		b2 = Contains(a,{false,false});
	
		b3 = CountFalse(a);
		
		b4 = CountTrue(a);
		
		b5 = SomeFalse(a);
		
		b6 = SomeTrue(a);
		
		b7 = ToString(a);
		
		a1 = {{3,-4},{4,5}};
		
		b8 = Transpose(a1);
		
		a2 = 3.5;
		
		b9 = Math.Floor(a2);
		
		b10 = Math.Ceiling(a2);
		
		a3 = 10;
		b11 = Math.Log(a3);
		b12 = Math.Sqrt(25);
		b13 = Math.Tan(45);
		
		b14 = NormalizeDepth({{1.1},{{2.3,3}},""5"",{{{{true}}}}},2);
		
	}
}
test = TestClass.TestClass();
t0 = test.b;
t1 = test.b1;
t2 = test.b2;
t3 = test.b3;
t4 = test.b4;
t5 = test.b5;
t6 = test.b6;
t7 = test.b7;
t8 = test.b8;
t9 = test.b9;
t10 = test.b10;
t11 = test.b11;
t12 = test.b12;
t13 = test.b13;
t14 = test.b14;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 1, 2, 3, 4, 0 };
            Object[] v2 = new Object[] { 3, 4 };
            Object[] v3 = new Object[] { -4, 5 };
            Object[] v4 = new Object[] { v2, v3 };
            Object[] v5 = new Object[] { 1.1 };
            Object[] v6 = new Object[] { 2.3, 3 };
            Object[] v7 = new Object[] { "5" };
            Object[] v8 = new Object[] { true };
            Object[] v9 = new Object[] { v5, v6, v7, v8 };
            thisTest.Verify("t0", v1);
            thisTest.Verify("t1", 3);
            thisTest.Verify("t2", true);
            thisTest.Verify("t3", 2);
            thisTest.Verify("t4", 2);
            thisTest.Verify("t5", true);
            thisTest.Verify("t6", true);
            thisTest.Verify("t7", "{true,{false,false},true}");
            thisTest.Verify("t8", v4);
            thisTest.Verify("t9", 3);
            thisTest.Verify("t10", 4);
            thisTest.Verify("t11", 2.3025850929940459);
            thisTest.Verify("t12", 5.0);
            thisTest.Verify("t13", 1.0);
            thisTest.Verify("t14", v9);
        }

        [Test]
        public void T80582_Ceil()
        {
            string code = @"
//testing ceiling()
import(""DSCoreNodes.dll"");
x = 1.5 ; 
y = 0.01 ; 
z = -0.1 ; 
a = Math.Ceiling(x) ;//2
b = Math.Ceiling(y) ;//1 
c = Math.Ceiling(z) ; //0
d = Math.Ceiling(-1.5) ;//-1
e = Math.Ceiling(-2);//-2
g = Math.Ceiling(2.1);//3
h = Math.Ceiling({0.2*0.2 , 1.2 , -1.2 , 3.4 , 3.6 , -3.6,0}); //{1,2,-1,4,4,-3,0}
//w = {{1.2,-2.1},{0.3,(-4*2.3)}};
//k = Math.Ceiling(y<2><1>); //
//negative testing
a1 = ""s"";
b1 = Math.Ceiling(a1); //null
a2 = Point.ByCartesianCoordinates(CoordinateSystem(),0,0,0);
b2 = Math.Ceiling(a2); //null
b3 = Math.Ceiling(null); //null
a4 = {};
b4 = Math.Ceiling(a4); //null
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 1, 2, -1, 4, 4, -3, 0 };
            Object[] v2 = new Object[] { };
            thisTest.Verify("a", 2);
            thisTest.Verify("b", 1);
            thisTest.Verify("c", 0);
            thisTest.Verify("d", -1);
            thisTest.Verify("e", -2);
            thisTest.Verify("g", 3);
            thisTest.Verify("h", v1);
            thisTest.Verify("b1", null);
            thisTest.Verify("b2", null);
            thisTest.Verify("b3", null);
            thisTest.Verify("b4", v2);
        }

    }
}
