using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
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
    def fibonacci_recursive:int(number : int)
    {
return = [Imperative]{
        if( number < 2)
        {
            return = 1;
        }
        return = fibonacci_recursive(number-1) + fibonacci_recursive(number -2);
}
    }
    
    def fibonacci_iterative:int(number : int)
    {
return = [Imperative]{
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
}
    fib10_r = fibonacci_recursive(20);
    fib10_i = fibonacci_iterative(20);
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
    def abs : double( val : double )
    {
return = [Imperative]{
        if( val < 0 )
        {
            return = -1 * val;
        }
        return = val;
}
    }
    
    //    this is famous as the first ever algo to evaluate
    //    square-root - also known as Babylonian algo
    //    developed by Heron and coded by Sarang :)
    //    
    def sqrt_heron : double ( val : double )
    {
return = [Imperative]{
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
    }
    
    def sqrt : double ( val : double )
    {
        return = sqrt_heron( val);
    }
    
    sqrt_10 = sqrt(10.0);
    sqrt_20 = sqrt(20.0);
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
        //This served as a sample test case for a test which produces non-homogenous output and verifies with baseline.
        [Category("SmokeTest")]
        public void TestArray()
        {
            String code =
@"a;
[Associative]
{
	a = [1001.1,1002, true];
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
input1 = [0.0, 10.0, 20.0, 30.0, 50.0];
input2 = [10.0, 20];
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
input1d = [0.0, 10.0, 20.0, 30.0, 50.0];
input2d = [[10, 20],[30,40]];
input2dJagged = [[10.0, 20],[null,40], [true], [0]];
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
            thisTest.Verify("a", 5);
            thisTest.Verify("b", 5);
            mirror.SetValueAndExecute("a", 10);
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
        public void error_LineNumber_2()
        {
            string err = "1467130 - Sprint 24 - Rev 2908 - Missing Line number information while throwing warning ";
            string code = @"a=b;";
            thisTest.VerifyRunScriptSource(code, err);
            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.IdUnboundIdentifier);
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
		array = [ 1, 2 ];
		counter = 0;
		while( counter < col)
		{
			array[counter] = [ 1, 2];
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
        }

        [Test]
        public void TestFrameWork_IntDouble_Array_1467413()
        {
            String code =
            @"
                a = [1.0,2.0];
                b=[1,2];
            ";
            string errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("a", new object[] { 1.0, 2.0 });
            thisTest.Verify("b", new object[] { 1, 2 });
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
        public void functionNotFound_1467444()
        {
            String code =
            @"
                a = foo();
            ";
            string errmsg = "";

            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);

            TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.FunctionNotFound);
        }
    }
}
