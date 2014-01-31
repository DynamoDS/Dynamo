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
import(""Math.dll"");
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
import(""Math.dll"");
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
import(""Math.dll"");
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

        [Test]
        public void T80585_Count()
        {
            string code = @"
// testing Count()
import(""ProtoGeometry.dll"");
def ret_count(ee : int[]) 
{
return =  Count(ee[1..Count(ee)]);
}
a = {1,2,3,0};
b = Count(a); //4
c = {1.1,-1.2,3,4};
d = Count(c) ; //4
e1 = {{1,2},{3,4},{4,5}};
f = Count(e1);//3
g = ret_count(e1); //{2,2,2}
h = Count(e1[1..Count(e1)-1]);//3
i = Count(e1[0]); //2
j = Count({{{{1,3},2},{1}},{2,3}}); //2
//negative testing
a1 = ""s"";
b1 = Count(a1); //0
a2 = Point.ByCartesianCoordinates(CoordinateSystem(),0,0,0);
b2 = Count(a2); //0
b3 = Count(null); //null
a4 = {};
b4 = Count(a4); //0";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Object[] v1 = new Object[] { 2, 2, 2 };
            thisTest.Verify("b", 4);
            thisTest.Verify("d", 4);
            thisTest.Verify("f", 3);
            thisTest.Verify("g", v1);
            thisTest.Verify("i", 2);
            thisTest.Verify("j", 2);
        }

        [Test]
        public void language_functions_test_1()
        {
            string code = @"
import(""ProtoGeometry.dll"");
import(""Math.dll"");
raggedCollection = { 1, { 2, 3 } };
isUniformDepthRagged = IsUniformDepth(raggedCollection);//false
average = Average(raggedCollection);
sum = Sum(raggedCollection); // works (=6), but complains that the ""Variable is over indexed""
ragged0  = raggedCollection[0]; // (=1)
ragged1  = raggedCollection[1]; // (={2,3})
ragged00 = raggedCollection[0][0]; // (=null) this should and does fail
ragged10 = raggedCollection[1][0]; // (=2)
ragged11 = raggedCollection[1][1]; // (=3) 
ragged2  = raggedCollection[2]; // (={null) // but reports ""Variable is over indexed"" for line 18
raggedminus1  = raggedCollection[-1]; // (={2,3})
raggedminus1minus1 = raggedCollection[-1][-1]; // (=3)
rankRagged = Rank(raggedCollection);
indexOf = IndexOf(raggedCollection, 1); // not sure what value should be returned here
transposeRagged = Transpose(raggedCollection); // (={{1,2},{3}} is this expected?
noramlisedDepthCollection = NormalizeDepth(raggedCollection);
isUniformDepthNormalize = IsUniformDepth(noramlisedDepthCollection);
transposeNormalize = Transpose(noramlisedDepthCollection);
noramlised00 = noramlisedDepthCollection[0][0];
rankNoramlised = Rank(noramlisedDepthCollection);
flattenedCollection = Flatten(raggedCollection);
rankFlattened = Rank(flattenedCollection);
reverseCollection = Reverse(flattenedCollection);
count = Count(reverseCollection);
contains = Contains(reverseCollection, 2);
indexOf = IndexOf(reverseCollection, 2);
reordedCollection = Reorder(flattenedCollection, { 2, 0, 1 }); // (={3,1,2}
indexByValue = SortIndexByValue(reordedCollection, true); // (={1,2,0}) not sure thsis is correct
def sorterFunction(a:double, b:double)
{
    return = a < b ? 1 : 0;
}
sort = Sort(sorterFunction, reordedCollection);  // (=null) something wrong here
newArray = noramlisedDepthCollection;
newArray[0][1] = 6; // directly add a member to a 2D array.. good
newArray[2] = { 7, 8, 9 }; // and good
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            string errmsg = "1467445 - Transpose() of ragged array doesn't behave correctly";
            string errmsg2 = "1467446 - Sort(sorterFunction : Function, varsToSort : var[])          doesn't return correct result";
            thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.VerifyRunScriptSource(code, errmsg2);
            Object[] v1 = new Object[] { 2, 3 };
            Object[] v2 = new Object[] { 1 };
            Object[] v3 = new Object[] { v2, v1 };
            Object[] v4 = new Object[] { 2 };
            Object[] v5 = new Object[] { 3 };
            Object[] v6 = new Object[] { v2, v4, v5 };
            Object[] v7 = new Object[] { 1, 2, 3 };
            Object[] v8 = new Object[] { 3, 2, 1 };
            Object[] v9 = new Object[] { 3, 1, 2 };
            Object[] v10 = new Object[] { 1, 2, 0 };
            Object[] v11 = new Object[] { 1, 6 };
            Object[] v12 = new Object[] { 7, 8, 9 };
            Object[] v13 = new Object[] { v11, v1, v12 };
            Object[] v14 = new Object[] { 1, 2 };
            Object[] v15 = new Object[] { v14, v5 };
            Object[] v16 = new Object[] { v1 };
            Object[] v17 = new Object[] { v2, v16 };

            thisTest.Verify("isUniformDepthRagged", false);
            thisTest.Verify("average", 2.0);
            thisTest.Verify("sum", 6);
            thisTest.Verify("ragged0", 1);
            thisTest.Verify("ragged1", v1);
            thisTest.Verify("ragged00", null);
            thisTest.Verify("ragged10", 2);
            thisTest.Verify("ragged11", 3);
            thisTest.Verify("ragged2", null);
            thisTest.Verify("raggedminus1", v1);
            thisTest.Verify("raggedminus1minus1", 3);
            thisTest.Verify("rankRagged", 2);

            thisTest.Verify("transposeRagged", null);//not sure
            //thisTest.Verify("transposeRagged", nu
            thisTest.Verify("noramlisedDepthCollection", v3);
            thisTest.Verify("isUniformDepthNormalize", true);
            thisTest.Verify("transposeNormalize", v15);
            thisTest.Verify("noramlised00", 1);
            thisTest.Verify("rankNoramlised", 2);
            thisTest.Verify("flattenedCollection", v7);
            thisTest.Verify("rankFlattened", 1);
            thisTest.Verify("reverseCollection", v8);
            thisTest.Verify("count", 3);
            thisTest.Verify("contains", true);
            thisTest.Verify("indexOf", 1);
            thisTest.Verify("reordedCollection", v9);
            thisTest.Verify("indexByValue", v10);
            thisTest.Verify("sort", v7);
            thisTest.Verify("newArray", v13);


        }

        [Test]
        public void set_operation_functions_test_1()
        {
            string code = @"
import(""ProtoGeometry.dll"");
import(""Math.dll"");
set = { true, { false, true } };
allFalseSet = AllFalse(set);
someFalseSet = SomeFalse(set);
someTrueSet = SomeTrue(set);
someNullsSet = SomeNulls(set);
setInsert = Insert(set, null, -1);
allFalseSetInsert = AllFalse(setInsert);
someFalseSetInsert = SomeFalse(setInsert);
someTrueSetInsert = SomeTrue(setInsert); // (=true).. which is correct, but gives 'Argument Type Mismatch' error
someNullsSetInsert = SomeNulls(setInsert);
countFalse = CountFalse(setInsert);
countTrue = CountTrue(setInsert);
containsNull = Contains(setInsert, null);
removeSetInsert = Remove(setInsert, 2);
removeNullsSetInsert = RemoveNulls(setInsert);
removeDuplicatesSetInsert = RemoveDuplicates(setInsert);
flattenSetInsert = Flatten(setInsert);
removeDuplicatesSetInsertFalttened = RemoveDuplicates(flattenSetInsert);
removeIfNotSetInsert = RemoveIfNot(flattenSetInsert, ""bool""); // (={})... this looks incorrect
one1Dcollection = { 3, 1 };
other1Dcollection = { 0, 1, 2, 3, 4 };
setDifferenceA = SetDifference(one1Dcollection, other1Dcollection);
setDifferenceB = SetDifference(other1Dcollection, one1Dcollection);
setIntersection = SetIntersection(other1Dcollection, one1Dcollection);
setUnion = SetUnion(other1Dcollection, one1Dcollection); ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            string errmsg1 = "1467448 - built-in function :RemoveIfNot() gives wrong result when null is the only element left after removing the others ";
            thisTest.VerifyRunScriptSource(code, errmsg1);
            string errmsg2 = "1467447 - built-in function : RemoveDuplicates gives wrong result when there is no duplicates";
            thisTest.VerifyRunScriptSource(code, errmsg2);
            Object[] v1 = new Object[] { false, true };
            Object[] v2 = new Object[] { true, null, v1 };
            Object[] v3 = new Object[] { true, null };
            Object[] v4 = new Object[] { true, null, false, true };
            Object[] v5 = new Object[] { true, null, false };
            Object[] v6 = new Object[] { null };
            Object[] v7 = new Object[] { };
            Object[] v8 = new Object[] { 0, 2, 4 };
            Object[] v9 = new Object[] { 1, 3 };
            Object[] v10 = new Object[] { 0, 1, 2, 3, 4 };
            Object[] v11 = new Object[] { true, v1 };
            Object[] v12 = new Object[] { false };
            Object[] v13 = new Object[] { true, null, v12 };

            thisTest.Verify("allFalseSet", false);
            thisTest.Verify("someFalseSet", true);
            thisTest.Verify("someTrueSet", true);
            thisTest.Verify("someNullsSet", false);
            thisTest.Verify("setInsert", v2);
            thisTest.Verify("allFalseSetInsert", false);
            thisTest.Verify("someFalseSetInsert", true);
            thisTest.Verify("someTrueSetInsert", true);
            thisTest.Verify("someNullsSetInsert", true);
            thisTest.Verify("countFalse", 1);
            thisTest.Verify("countTrue", 2);
            thisTest.Verify("containsNull", true);
            thisTest.Verify("removeSetInsert", v3);
            thisTest.Verify("removeNullsSetInsert", v11);
            thisTest.Verify("removeDuplicatesSetInsert", v2);
            thisTest.Verify("flattenSetInsert", v4);
            thisTest.Verify("removeDuplicatesSetInsertFalttened", v5);
            thisTest.Verify("removeIfNotSetInsert", v6);
            thisTest.Verify("setDifferenceA", v7);
            thisTest.Verify("setDifferenceB", v8);
            thisTest.Verify("setIntersection", v9);
            thisTest.Verify("setUnion", v10);
        }
        /* 
[Test]         public void TestFrameWork_IntDouble()         {             String code = @"     a = 1.0; ";             thisTest.RunScriptSource(code);             thisTest.Verify("a", 1);                     }*/
    }
}
