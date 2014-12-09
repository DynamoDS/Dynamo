using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
using ProtoTestFx.TD;
namespace ProtoTest.TD.OtherMiscTests
{
    class MiscTest : ProtoTestBase
    {
        [Test]
        [Category("SmokeTest")]
        public void Fibunacci()
        {
            string code = @"
fib10_r;
fib10_i;
[Imperative]
{
    def fibonacci_recursive:int(number : int)
    {
        if( number < 2)
        {
            return = 1;
        }
        return = fibonacci_recursive(number-1) + fibonacci_recursive(number -2);
    }
    
    def fibonacci_iterative:int(number : int)
    {
        one = 0;
        two = 1;
       counter = 1;
        
        while( counter <= number )
        {
            temp = one + two;
            one = two;
            two = temp;
            
            //    now increment the counter
            counter = counter + 1;
        }
        
        return = two;
    }
    fib10_r = fibonacci_recursive(20);
    fib10_i = fibonacci_iterative(20);
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("fib10_r", 10946);
            thisTest.Verify("fib10_i", 10946);
        }

        [Test]
        [Category("SmokeTest")]
        public void SquareRoot()
        {
            string code = @"
sqrt_10;
sqrt_20;
[Imperative]
{
    def abs : double( val : double )
    {
        if( val < 0 )
        {
            return = -1 * val;
        }
        return = val;
    }
    
    //    this is famous as the first ever algo to evaluate
    //    square-root - also known as Babylonian algo
    //    developed by Heron and coded by Sarang :)
    //    
    def sqrt_heron : double ( val : double )
    {
        counter = 0;
        temp_cur = val / 2.0;
        temp_pre = temp_cur - 1.0;
        abs_diff = abs(temp_cur - temp_pre);
        tolerance = 0.00001;
        max_iterations = 100;
        
        while( abs_diff > tolerance && counter < max_iterations )
        {
            temp_pre = temp_cur;
            temp_cur = 0.5 * (temp_cur + val / temp_cur );
            
            abs_diff = abs(temp_cur - temp_pre);
            counter = counter + 1;
        }
        
        return = temp_cur;
    }
    
    def sqrt : double ( val : double )
    {
        return = sqrt_heron( val);
    }
    
    sqrt_10 = sqrt(10.0);
    sqrt_20 = sqrt(20.0);
 
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("sqrt_10", Math.Sqrt(10.0));
            thisTest.Verify("sqrt_20", Math.Sqrt(20.0));
        }

        [Test]
        [Category("SmokeTest")]
        public void BasicAssign()
        {
            string code =
                @"a = 16;
b = 42;
c = a + b;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("a", 16);
            thisTest.Verify("b", 42);
            thisTest.Verify("c", 58);
        }

        [Test]
        [Category("SmokeTest")]
        public void Demo_SinWave_WithoutGeometry()
        {
            string code = @"
import (""DemoSupportFiles.ds"");
import(""DSCoreNodes.dll"");
// dimensions of the roof in each direction
//
xSize = 2;//10;
ySize = 6;//30;
// number of Waves in each direction
//
xWaves = 1;
yWaves = 1;
// number of points per Wave in each direction\
//
xPointsPerWave = 2;//10;
yPointsPerWave = 2;//10;
// amplitudes of the frequencies (z dimension)
//
lowFrequencyAmpitude  = 1.0; // only ever a single low frequency wave
highFrequencyAmpitude = 0.75; // user controls the number and amplitude of high frequency waves 
// dimensions of the beams
//
radius = 0.1;
roofWallHeight    = 0.3; // not used
roofWallThickness = 0.1; // not used
// calculate how many 180 degree cycles we need for the Waves
//
x180ToUse = xWaves==1?xWaves:(xWaves*2)-1;
y180ToUse = yWaves==1?yWaves:(yWaves*2)-1;
// count of total number of points in each direction
//
xCount = xPointsPerWave*xWaves;
yCount = yPointsPerWave*yWaves;
highX = 180*x180ToUse;
highY = 180*y180ToUse;
def CreateRangeWithDefinedNumber (start : double, end : double, number: int)
{
	stepsize = (end - start)/(number-1);
	return = start..end..stepsize;
}
//highRangeX = 0.0..highX..#xCount;
//lowRangeX = 0.0..180.0..#xCount;
//highRangeY = 0.0..highY..#yCount;
//lowRangeY = 0.0..180.0..#yCount;
highRangeX = CreateRangeWithDefinedNumber(0.0, highX, xCount);
lowRangeX = CreateRangeWithDefinedNumber(0.0, 180.0, xCount);
highRangeY = CreateRangeWithDefinedNumber(0.0, highY, yCount);
lowRangeY = CreateRangeWithDefinedNumber(0.0, 180.0, yCount);
sinHighRangeX = Math.Sin(highRangeX);
sinHighRangeY = Math.Sin(highRangeY);
xHighFrequency = Scale(sinHighRangeX, highFrequencyAmpitude);
yHighFrequency = Scale(sinHighRangeY, highFrequencyAmpitude);
//equivalent to:
//  xLowFrequency  = sin(-5..185..#xCount)*lowFrequencyAmpitude;
//  yLowFrequency  = sin(-5..185..#yCount)*lowFrequencyAmpitude;
//
sinLowRangeX = Math.Sin(lowRangeX);
sinLowRangeY = Math.Sin(lowRangeY);
xLowFrequency = Scale(sinLowRangeX, lowFrequencyAmpitude);
yLowFrequency = Scale(sinLowRangeY, lowFrequencyAmpitude);
// lowAmpitude is the cartesian product of xLowFrequency multiplied by yLowFrequency
//  equivalent to:
//  lowAmplitude = xLowFrequency<1> * yLowFrequency<2>;
//
lowAmplitude = CollectionCartesianProduct(xLowFrequency, yLowFrequency);
// lowAmplitude is the cartesian product of xLowFrequency multiplied by yLowFrequency
//  equivalent to:
//  highAmplitude = xHighFrequency<1> * yHighFrequency<2>;
//
highAmplitude = CollectionCartesianProduct(xHighFrequency, yHighFrequency);
// Ampitude in y is the zipped collection of the high frequency + low frequency
//
//  equivalent to:
//  amplitude = highAmplitude + lowAmplitude;
//
amplitude = CollectionZipAddition(highAmplitude, lowAmplitude);
//  x = 0..xSize..#xCount; --> this evaluates to 10 elements
x = 0.0..xSize..#xCount;
y = 0.0..ySize..#yCount;  //  actually this evalutes to 30 but to keep life simple i am keeping it at 10
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            // to be verified
        }

        [Test]
        //This served as a sample test case for a test which produces non-homogenous output and verifies with baseline.
        [Category("SmokeTest")]
        public void TestArray()
        {
            String code =
@"a;
[Associative]
{
	a = {1001.1,1002, true};
    x = a[0];
    y = a[1];
    z = a[2];
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expectedA = { 1001.1, 1002, true };
            thisTest.Verify("a", expectedA);
        }

        [Test]
        //This served as a sample test case for a test which produces non-homogenous output and verifies with baseline.
        [Category("SmokeTest")]
        public void DefectVerification()
        {
            String code =
@"a =null;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object test = null;
            thisTest.Verify("a", test);
        }

        [Test]
        [Category("SmokeTest")]
        public void DemoTest_Create1DArray()
        {
            String code =
@"def Create1DArray(numberOfItemInArray : int)
    {
        start = 0.0;
        end = 10.0;
        stepSize = 10/(numberOfItemInArray-1);
        return = 0.0..10..stepSize;
    
    
    }
b = 10.0;
a = 0.0;
numberOfItemInArray = 3;
c = b/(numberOfItemInArray-1);
d = a..b..c;
result = Create1DArray (3);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] test = { 0.0, 5.0, 10.0 };
            thisTest.Verify("result", test);
        }

        [Test]
        [Category("SmokeTest")]
        public void DemoTest_Create2DArray()
        {
            String code =
@"def Create1DArray(numberOfItemInArray : int)
    {
        stepSize = 10.0/(numberOfItemInArray-1);
        return = 0.0..10.0..stepSize;
    }
def Create2DArray(rows : int, columns : int)
    
    {
    
    
   result = [Imperative]
       {
       arrayRows = Create1DArray(rows);
       counter = 0;
       while( counter < rows)
       {
           arrayRows[counter] = Create1DArray(columns);
           counter = counter + 1;
       }
       
       return = arrayRows;
       }
    
    return = result;
        
}
OneD = Create1DArray(5);
TwoD = Create2DArray(2,2);
TwoD0 = TwoD[0];
TwoD1 = TwoD[1];
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object[] expected = { 0.0, 10.0 };
            thisTest.Verify("TwoD0", expected);
            thisTest.Verify("TwoD1", expected);
        }

        [Test]
        [Category("SmokeTest")]
        public void DemoTest_Count()
        {
            String code =
@"def Count(inputArray : double[])
{
	numberOfItemsInArray = [Imperative]
	{
		index = 0;
		for (item in inputArray)
		{
			index = index + 1;
		}
		
		return = index;
	}
	
	return = numberOfItemsInArray;
}
input1 = {0.0, 10.0, 20.0, 30.0, 50.0};
input2 = {10.0, 20};
result1 = Count(input1);
result2 = Count(input2);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("result1", 5);
            thisTest.Verify("result2", 2);
        }

        [Test]
        public void TempTest()
        {
            String code =
@"
input1d = {0.0, 10.0, 20.0, 30.0, 50.0};
input2d = {{10, 20},{30,40}};
input2dJagged = {{10.0, 20},{null,40}, {true}, {0}};
inputDouble = 10.0;
inputNull = null;
inputInteger = 1;
inputBool = true;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object inputDouble = 10.0;
            object inputInteger = 1;
            object inputBool = true;
            object[] input1d = { 0.0, 10.0, 20.0, 30.0, 50.0 };
            object[][] input2d = 
            { 
            new object [] { 10, 20 }, 
            new object [] { 30, 40 } 
            };
            object[][] input2dNegative =
            { 
            new object [] { 10, 21 }, 
            new object [] { 30, 40 } 
            };
            object[][] input2dJagged =
            { 
            new object [] { 10.0, 20}, 
            new object [] { null, 40 },
            new object [] { true },
            new object [] { 0}
            };
            object[][] input2dJaggedNegative =
            { 
            new object [] { 10.0}, 
            new object [] { null, 40 },
            new object [] { true },
            new object [] { 0}
            };
            //Positive Test
            thisTest.Verify("input1d", input1d);
            thisTest.Verify("input2d", input2d);
            //thisTest.Verify("input2d", input2dNegative);
            thisTest.Verify("input2dJagged", input2dJagged);
            thisTest.Verify("input2dJagged", input2dJagged);
            //thisTest.NewVerification(mirror, "inputNull", inputNull);
            //thisTest.NewVerification(mirror, "inputInteger", inputInteger);
            //thisTest.NewVerification(mirror, "inputBool", inputBool);
            ////Negative Test with same type
            //thisTest.NewVerification(mirror, "inputDouble", 11.0);
            //thisTest.NewVerification(mirror, "inputInteger", 2);
            //thisTest.NewVerification(mirror, "inputBool", false);
            ////Negative Test with different type
            //thisTest.NewVerification(mirror, "inputDouble", inputNull);
            //thisTest.NewVerification(mirror, "inputDouble", inputInteger);
            //thisTest.NewVerification(mirror, "inputDouble", inputBool);
            //thisTest.NewVerification(mirror, "inputInteger", inputNull);
            //thisTest.NewVerification(mirror, "inputInteger", inputDouble);
            //thisTest.NewVerification(mirror, "inputInteger", inputBool);
            //thisTest.NewVerification(mirror, "inputNull", inputInteger);
            //thisTest.NewVerification(mirror, "inputNull", inputDouble);
            //thisTest.NewVerification(mirror, "inputNull", inputBool);
            //thisTest.NewVerification(mirror, "inputBool", inputInteger);
            //thisTest.NewVerification(mirror, "inputBool", inputDouble);
            //thisTest.NewVerification(mirror, "inputBool", inputNull);
            //TODO: Multi-level verification. 
        }

        [Test]
        [Category("SmokeTest")]
        [Category("Failure")]
        public void DynamicReferenceResolving_Complex_Case()
        {
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4138
            string code = @"
class A
{
}
class B extends A
{
	k : var;
	
	constructor B(){}
	constructor B(p:int)
	{
		k = C.C(p);
	}
	
	def foo1:int(m:int[], t : int = 4)
	{
		return = m[0] + t;
	}
}
class C
{
	m : var;
	constructor C(){}
	constructor C(p:int)
	{
		m = {p*1, p*2, p*3};
	}
}
def foo : A()
{
	return = B.B(1);
}
def foo2 : int(b:A)
{
	return = b.foo1(b.k.m); 	//warning. will try to find foo1 at runtime. If b happends to be of type B, then it will find foo1.
}
t = foo();	// Type is recognized as A, actual type is B
tm = t.k.m;	// k does not exist in A, unbound identifier warning; tm = {1, 2, 3}
testFoo1 = t.foo1(tm); // foo1 does not exist in A, function not found warning; testFoo1 = 5;
b1 = B.B(2);
testInFunction1 = foo2(b1); //testInFunction1 = 6;
b2 = B.B(3);
testInFunction2 = foo2(b2); //testInFunction2 = 7;
[Imperative]
{
	it = foo();	// Type is recognized as A, actual type is B
	itm = it.k.m;	// k does not exist in A, unbound identifier warning; tm = {1, 2, 3}
	itestFoo1 = it.foo1(itm); // foo1 does not exist in A, function not found warning; testFoo1 = 5;
	ib1 = B.B(2);
	itestInFunction1 = foo2(ib1); //testInFunction1 = 6;
	ib2 = B.B(3);
	itestInFunction2 = foo2(ib2); //testInFunction2 = 7;
}";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification
            object[] tm = { 1, 2, 3 };
            object testFoo1 = 5;
            object testInFunction1 = 6;
            object testInFunction2 = 7;
            object[] itm = { 1, 2, 3 };
            object itestFoo1 = 5;
            object itestInFunction1 = 6;
            object itestInFunction2 = 7;
            thisTest.Verify("tm", tm);
            thisTest.Verify("testFoo1", testFoo1);
            thisTest.Verify("testInFunction1", testInFunction1);
            thisTest.Verify("testInFunction2", testInFunction2);
            thisTest.Verify("itm", itm);
            thisTest.Verify("itestFoo1", itestFoo1);
            thisTest.Verify("itestInFunction1", itestInFunction1);
            thisTest.Verify("itestInFunction2", itestInFunction2);
        }

        [Test]
        [Category("SmokeTest")]
        public void DynamicReference_Variable()
        {
            string code = @"
class A
{
}
class B extends A
{	
	k : var;
	constructor B(x : int)
	{
		k = x;
	}
}
def foo : A(x:int)
{
	return = B.B(x);
}
t = foo(3);
kk = t.k;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification
            object kk = 3;
            thisTest.Verify("kk", kk);
        }

        [Test]
        [Category("SmokeTest")]
        public void DynamicReference_FunctionCall()
        {
            string code = @"
class A
{
}
class B extends A
{	
	def foo1:int(t : int)
	{
		return = t;
	}
}
def foo : A()
{
	return = B.B();
}
t = foo();	// Type is recognized as A, actual type is B
testFoo1 = t.foo1(6); // foo1 does not exist in A, function not found warning; testFoo1 = 6;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification
            object testFoo1 = 6;
            thisTest.Verify("testFoo1", testFoo1);
        }

        [Test]
        [Category("SmokeTest")]
        [Category("Failure")]
        public void DynamicReference_FunctionCall_With_Default_Arg()
        {
            string err = "MAGN-4137 Method resolution error with default arguments in member function";
            string code = @"class A
{
}
class B extends A
{	
	def foo:int(t : int = 4)
	{
		return = t;
	}
}
def afoo : A()
{
	return = B.B();
}
t = afoo();	// Type is recognized as A, actual type is B
testFoo1 = t.foo(6); // foo1 does not exist in A, function not found warning; testFoo1 =6;
testFoo2 = t.foo(); // foo1 does not exist in A, function not found warning; testFoo2 =4;";
            thisTest.VerifyRunScriptSource(code, err);
            //Verification
            object testFoo1 = 6;
            object testFoo2 = 4;
            thisTest.Verify("testFoo1", testFoo1);
            thisTest.Verify("testFoo2", testFoo2);
        }

        [Test]
        [Category("SmokeTest")]
        public void FunctionCall_With_Default_Arg()
        {
            string err = "";
            string code = @"def foo:int(t : int = 4)
	{
		return = t;
	}
	// Type is recognized as A, actual type is B
testFoo1 = foo(6); // foo1 does not exist in A, function not found warning; testFoo1 =6;
testFoo2 = foo();";
            thisTest.VerifyRunScriptSource(code, err);
            //Verification
            object testFoo1 = 6;
            object testFoo2 = 4;
            thisTest.Verify("testFoo1", testFoo1);
            thisTest.Verify("testFoo2", testFoo2);
        }

        [Test]
        [Category("Update")]
        public void TestDynamicSetValueAfterExecution()
        {
            //Assert.Fail("1466857 - Sprint 23 : rev 2486 : Dynamic variable update issue"); 
            String code =
                @"
                a = 2;
                b = a;
                a = 5;
                ";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Obj o = mirror.GetValue("a");
            Obj o2 = mirror.GetValue("b");
            thisTest.Verify("a", 5);
            thisTest.Verify("b", 5);
            mirror.SetValueAndExecute("a", 10);
            o = mirror.GetValue("a");
            o2 = mirror.GetValue("b");
            thisTest.Verify("a", 10);
            thisTest.Verify("b", 10);
        }

        [Test]
        [Category("SmokeTest")]
        public void Comments_1467117()
        {
            string code = @"
/*
/*
*/
a=5;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification
            thisTest.Verify("a", 5);
        }

        [Test]
        [Category("SmokeTest")]
        public void Comments_basic()
        {
            string code = @"
/*
WCS=CoordinateSystem.Identity();
p2 = Point.ByCoordinates(0,0,0);
*/
a=5;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            //Verification
            thisTest.Verify("a", 5);
        }

        [Test]
        [Category("SmokeTest")]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void Comments_Nested()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
/*
WCS=CoordinateSystem.Identity();
/*
p2 = Point.ByCoordinates(0,0,0);
*/
*/
import(""ProtoGeometry.dll"");
WCS=CoordinateSystem.Identity();
p2 = Point.ByCoordinates(0,0,0);";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void Comments_Negative()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
/*
WCS=CoordinateSystem.Identity();
p2 = Point.ByCoordinates(0,0,0);
*/
/*
import(""ProtoGeometry.dll"");
WCS=CoordinateSystem.Identity();
p2 = Point.ByCoordinates(0,0,0);";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
            //Verification
        }

        [Test]
        public void error_LineNumber_2()
        {
            string err = "1467130 - Sprint 24 - Rev 2908 - Missing Line number information while throwing warning ";
            string code = @"a=b;";
            thisTest.VerifyRunScriptSource(code, err);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kIdUnboundIdentifier);
            //Verification
        }

        [Test]
        public void Use_Keyword_Array_1463672()
        {
            string code = @"
def Create2DArray( col : int)
{
	result = [Imperative]
    {
		array = { 1, 2 };
		counter = 0;
		while( counter < col)
		{
			array[counter] = { 1, 2};
			counter = counter + 1;
		}
		return = array;
	}
    return = result;
}
x = Create2DArray( 2) ;";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            object a = new Object[] { new Object[] { 1, 2 }, new Object[] { 1, 2 } };
            thisTest.Verify("x", a);
        }

        [Test]
        public void Comments_1467117_1()
        {
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                string code = @"
/*
/*
*/
*/
a=1;
";
                ExecutionMirror mirror = thisTest.RunScriptSource(code);
            });
        }

        [Test]
        public void GarbageCollection_1467148()
        {
            string code = @"
class B
{
    value : int;
    constructor B (b : int)
    {
		 value = b;
    }
}
class A
{
    a1 : var;
    constructor A ( b1 : int)
    {                
         a1 = b1;
    } 
    def foo( arr : B[])  
    {  
         return = arr.value;  
    }
} 
arr = { B.B(1), B.B(2), B.B(3), B.B(4) }; 
q = A.A( {6,7,8,9} );
t = q.foo(arr);
n = Count(arr);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("n", 4);
        }
        [Test]
        [Category("Failure")]
        public void imperative_Replication_1467070()
        {
            // Tracked by: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4092
            string err = "MAGN-4092 Replication should not be supported in Imperative scope";
            string code = @"
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
return = t;
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, err);
            thisTest.Verify("t", new Object[] { null, null, 3, 4, 5 });
        }
        [Test]
        [Category("Failure")]
        public void imperative_Replication_1467070_2()
        {
            // Tracked by: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4092
            string err = "MAGN-4092 Replication should not be supported in Imperative scope";

            string code = @"
[Imperative]
{
        def CreateArray ( x : var[] , i )
        {
        
                x[i] = i;
                return = x;
        }
        test = { };
         z=0..5;
         for (i in z)
         {
                test[i] = CreateArray ( test, z );
            
        }
}
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, err);
            thisTest.Verify("t", new Object[] { null, null, 3, 4, 5 });
        }

        [Test]
        public void TestFrameWork_IntDouble_1467413()
        {
            String code =
            @"
                a = 1.0;
                b=1;
            ";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", 1.0);
            thisTest.Verify("b", 1);
            Assert.IsTrue(mirror.GetValue("a", 0).DsasmValue.IsDouble);
            Assert.IsFalse(mirror.GetValue("a", 0).DsasmValue.IsInteger);
            Assert.IsTrue(mirror.GetValue("b", 0).DsasmValue.IsInteger);
            Assert.IsFalse(mirror.GetValue("b", 0).DsasmValue.IsDouble);

            //thisTest.Verify("b", 1.0);
        }

        [Test]
        public void TestFrameWork_IntDouble_Array_1467413()
        {
            String code =
            @"
                a = {1.0,2.0};
                b={1,2};
            ";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", new object[] { 1.0, 2.0 });
            thisTest.Verify("b", new object[] { 1, 2 });
            Assert.IsFalse(mirror.GetValue("a", 0).DsasmValue.Equals(new object[] { 1, 1 }));
            Assert.IsFalse(mirror.GetValue("b", 0).DsasmValue.Equals(new object[] { 1.0, 1.0 }));

            //thisTest.Verify("b", 1.0);
        }

        [Test]
        public void TestKeyword_reserved_1467551()
        {
            String code =
            @"
                a = 2;
                base=1;
            ";
            string errmsg = "";
            //Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            //{
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.VerifyBuildWarningCount(0);
            // });

        }

        [Test]
        public void TestKeyword_reserved_1467551_2()
        {
            String code =
            @"
                [Associative]
                {
                a = 2;
                base=1;
                }
            ";
            string errmsg = "";
            //Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            //{
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.VerifyBuildWarningCount(0);
            //});
        }

        [Test]
        public void TestKeyword_reserved_1467551_3()
        {
            String code =
            @"
                [Imperative]
                {
                a = 2;
                base=1;
                }
            ";
            string errmsg = "";
            //Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            //{
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.VerifyBuildWarningCount(0);
            //});
        }


        [Test]
        [Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void TestKeyword_reserved_1467551_4()
        {
            String code =
            @"
                import(""ProtoGeometry.dll"");
                wcs = CoordinateSystem.Identity();
                base = Cylinder.ByRadiusHeight(wcs, 10, 5);
            ";
            string errmsg = "";
            //Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            //{
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.VerifyBuildWarningCount(0);
            //});
        }

        [Test]
        public void functionNotFound_1467444()
        {
            String code =
            @"
                a = foo();
            ";
            string errmsg = "";

            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);

            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kFunctionNotFound);
        }

        [Test]
        public void functionNotFound_1467444_2()
        {
            String code =
            @"
               z=[Imperative]
               {
                        def AnotherFunction(test:int)
                        {
                            result = test * test;
                            return = result;    
                        }
                        x = Function(5);
                        return = x;
               }
                                ";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kFunctionNotFound);
        }
    }
}
