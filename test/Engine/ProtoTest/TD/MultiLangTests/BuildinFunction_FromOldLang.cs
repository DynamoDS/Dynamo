using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTest.TD;
using ProtoCore.Lang.Replication;
namespace ProtoTest.TD.MultiLangTests
{
    class BuildinFunction_FromOldLang
    {
        public TestFrameWork thisTest = new TestFrameWork();
        string testPath = "..\\..\\..\\Scripts\\TD\\MultiLanguage\\BuildinFunctionFromOldLang\\";

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void T80580_BuiltinFunc_1()
        {
		string code = @"
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
		b = Floor(a);
		return = b;
	}
	def testCeil()
	{
		a = 3.5;
		b = Ceil(a);
		return = b;
	}
	def testLog()
	{
		b = Log(10);
		return = b;
	}
	def testSqrt()
	{
		b = Sqrt(25);
		return = b;
	}
	def testTan()
	{
		a = 45;
		b = Tan(a);
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


            thisTest.Verify("t1", v1);
            thisTest.Verify("t2",3);
            thisTest.Verify("t3", true);
            thisTest.Verify("t4", 2);
            thisTest.Verify("t5", 2);
            thisTest.Verify("t6", true);
            thisTest.Verify("t7", true);
            thisTest.Verify("t8", "{true,{false,false},true}");
            thisTest.Verify("t9", v4);

            
        }
    }
}

