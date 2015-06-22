using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using ProtoCore.Lang;
using ProtoFFI;
using ProtoScript.Runners;
using ProtoCore.DSASM.Mirror;
using ProtoTest.TD;
using ProtoTestFx.TD;

namespace ProtoTest.DebugTests
{
    [TestFixture]
    class BasicTests : ProtoTestBase
    {
        private DebugRunner fsr;
        private ProtoScript.Config.RunConfiguration runnerConfig;
        private string testPath = @"..\..\..\test\Engine\ProtoTest\ImportFiles\";

        public override void Setup()
        {
            base.Setup();
            core.Options.kDynamicCycleThreshold = 5;

            runnerConfig = new ProtoScript.Config.RunConfiguration();
            runnerConfig.IsParrallel = false;
            fsr = new DebugRunner(core);

            DLLFFIHandler.Register(FFILanguage.CSharp, new CSModuleHelper());
            CLRModuleType.ClearTypes();
        }

        public override void TearDown()
        {
            runtimeCore = fsr.runtimeCore;
            base.TearDown();
        }

        [Test] 
        [Category("ExpressionInterpreterRunner")]
        public void TestWatchExpression1()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
@"

a = 1;
b = 20;

", runnerConfig);
            DebugRunner.VMState vms = fsr.Step();
            vms = fsr.Step();

            // Execute and verify the watch window expression script
            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                watchRunner.Execute(@"%");
            });

            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"a");
            Obj objExecVal = mirror.GetWatchValue();
            Assert.IsTrue((Int64)objExecVal.Payload == 1);


            vms = fsr.Step();

            // Execute and verify the watch window expression script
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                watchRunner.Execute(@"%");
            });

            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"b");
            objExecVal = mirror.GetWatchValue();
            Assert.IsTrue((Int64)objExecVal.Payload == 20);
        }

        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void TestWatchExpression2()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
            @"
class Vector
{
    x : var;
    y : var;
    constructor Vector()
    {
        x = 10;
        y = 20;
    }
}

p = Vector.Vector();
            ", runnerConfig);

            // Tracked by: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3990
            string defectID = "MAGN-3990 Expression interpreter returns null when evaluates expression at end of script";

            // Highlights "p = Vector.Vector()".
            DebugRunner.VMState vms = fsr.Step();
            Assert.AreEqual(13, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(13, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(21, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step(); // Highlight "x = 10;".
            Assert.AreEqual(8, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(8, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(16, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step(); // Highlight "y = 20;".
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(9, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(16, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step(); // Highlight the closing bracket of constructor "}".
            Assert.AreEqual(10, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(10, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(6, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step(); // Highlight the "p = Vector.Vector();".
            Assert.AreEqual(13, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(13, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(21, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step(); // Ends execution.

            Assert.AreEqual(true, vms.isEnded);

            // Execute and verify the watch window expression script
            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute("p.x");
            Obj objExecVal = mirror.GetWatchValue();

            Assert.AreNotEqual(null, objExecVal, defectID);
            Assert.IsTrue((Int64)objExecVal.Payload == 10, defectID);
        }

        [Test]
        [Category("ExpressionInterpreterRunner")]
        [Category("Failure")]
        public void TestWatchExpression3()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
@"
class Vector
{
    x : var;
    y : var;
    constructor Vector()
    {
        x = 10;
        y = 20;
    }
}

p = Vector.Vector();

", runnerConfig);

            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3990
            string defectiD = "MAGN-3990 Expression interpreter returns null when evaluates expression at end of script";

            // Highlights "p = Vector.Vector()".
            DebugRunner.VMState vms = fsr.Step();
            Assert.AreEqual(13, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(13, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(21, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step(); // Highlight "x = 10;".
            Assert.AreEqual(8, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(8, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(16, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step(); // Highlight "y = 20;".
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(9, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(16, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step(); // Highlight the closing bracket of constructor "}".
            Assert.AreEqual(10, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(10, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(6, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step(); // Highlight the "p = Vector.Vector();".
            Assert.AreEqual(13, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(13, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(21, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step(); // Ends execution.
            Assert.AreEqual(true, vms.isEnded);

            // Execute and verify the watch window expression script
            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"p.y + 2");
            Obj objExecVal = mirror.GetWatchValue();

            Assert.AreNotEqual(null, objExecVal, defectiD);
            Assert.IsTrue((Int64)objExecVal.Payload == 22, defectiD);
        }

        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void TestWatchExpression4()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
@"
def f : int(i : int, j : int)
{
    a = 5;
    return = 7;
}

a = f(1, 2);
b = 2;
c = 3;
", runnerConfig);

            // First step should bring the exec cursor to "a = f(1, 2)".
            DebugRunner.VMState vms = fsr.StepOver();

            {
                // This is to simulate the event when "a" is added into the watch window
                // before it gets assigned a value. This should not cause subsequent stepping
                // to go wrong.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();

                // This should not return a value since "a" is unassigned now.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            // Second step should bring it to "b = 2;".
            vms = fsr.StepOver();

            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();

                // Now "a" should be "7" as the result of assignment from return value of "f".
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreNotEqual(null, objExecVal.Payload);
                Assert.AreEqual("7", objExecVal.Payload.ToString());
            }

            // Third step should bring it to "c = 3;".
            vms = fsr.StepOver();

            // Final step ends the debug session.
            vms = fsr.StepOver();
            Assert.AreEqual(true, vms.isEnded);
        }

        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void TestWatchExpression5()
        {
            string sourceCode = @"
a = { 1, 0, 0.0 };  // Line 2
b = { a, 1 };       // Line 3

[Imperative]        // Line 5
{
    a[1] = 1;       // Line 7
    m = a;          // Line 8
}                   // Line 9
";
            fsr.PreStart(sourceCode, runnerConfig);
            DebugRunner.VMState vms = fsr.StepOver();
            Assert.AreEqual(2, vms.ExecutionCursor.StartInclusive.LineNo);
            vms = fsr.StepOver();
            Assert.AreEqual(3, vms.ExecutionCursor.StartInclusive.LineNo);
            vms = fsr.StepOver();
            Assert.AreEqual(7, vms.ExecutionCursor.StartInclusive.LineNo);
            vms = fsr.StepOver();
            Assert.AreEqual(8, vms.ExecutionCursor.StartInclusive.LineNo);
            vms = fsr.StepOver();
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.LineNo);

            // Get the value of "m[0]".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"m[0]");
                Obj objExecVal = mirror.GetWatchValue();

                // It should be "1".
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreNotEqual(null, objExecVal.Payload);
                Assert.AreEqual("1", objExecVal.Payload.ToString());
            }

            vms = fsr.StepOver(); // Causes "b = { a, 1 };" to execute.
            Assert.AreEqual(3, vms.ExecutionCursor.StartInclusive.LineNo);

            // Get the value of "m[0]".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"m[0]");
                Obj objExecVal = mirror.GetWatchValue();

                // It should still be "1".
                Assert.AreNotEqual(null, objExecVal);
                // Assert.AreEqual(null, objExecVal.Payload);
            }
        }

        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void TestWatchExpressionInFunction()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
@"
def foo()
{
    aaa = 4;
    return = null;
}

bbb = foo();
", runnerConfig);

            // First step should bring the exec cursor to "bbb = foo()".
            DebugRunner.VMState vms = fsr.StepOver();
            Assert.AreEqual(8, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(8, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(13, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step(); // Highlight "aaa = 4;"
            Assert.AreEqual(4, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(4, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(13, vms.ExecutionCursor.EndExclusive.CharNo);

            // Get the value of "aaa".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"aaa");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            vms = fsr.StepOver(); // Highlight "return = null;"
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(5, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(19, vms.ExecutionCursor.EndExclusive.CharNo);

            // Get the value of "aaa".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"aaa");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreNotEqual(null, objExecVal.Payload);
                Assert.AreEqual("4", objExecVal.Payload.ToString());
            }
        }

        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void TestWatchExpressionInFunctionNestedInImperativeBlock()
        {
            // This comes from IDE-538
            fsr.PreStart(
@"z=[Imperative] //Line 1
{
    def GetNumberSquare(test:int)
    {
        result = test * test;
        return = result;    
    }
    x = GetNumberSquare(5);
    return = x;
}
", runnerConfig);

            DebugRunner.VMState vms = fsr.StepOver();//Line 8

            vms = fsr.Step();//Line 5

            // Get the value of "result".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"result");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            // Get the value of "test".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"test");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(5, (Int64)objExecVal.Payload);
            }

            vms = fsr.Step();//Line 6

            // Get the value of "result".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"result");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(25, (Int64)objExecVal.Payload);
            }

            // Get the value of "test".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"test");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(5, (Int64)objExecVal.Payload);
            }

            vms = fsr.Step();//Line 7

            // Get the value of "result".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"result");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(25, (Int64)objExecVal.Payload);
            }

            // Get the value of "test".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"test");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(5, (Int64)objExecVal.Payload);
            }

            vms = fsr.Step();//Line 8

            // Get the value of "result".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"result");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            // Get the value of "test".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"test");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }
        }

        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void TestWatchExpressionInFunctionNestedWithImperativeBlock()
        {
            // This comes from IDE-544
            fsr.PreStart(
@"def func(d : int) //Line 1
{
    m : int;
    m = 10;
    temp = 0; // Line #5
    [Imperative]
    {
        temp = m; // Line #8
    }
    return = temp; // Line #10
}

n = func(1);
", runnerConfig);

            DebugRunner.VMState vms = fsr.StepOver();//Line 13

            vms = fsr.Step();//Line 4

            // Get the value of "m".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"m");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            // Get the value of "temp".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"temp");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            vms = fsr.Step();//Line 5

            // Get the value of "m".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"m");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(10, (Int64)objExecVal.Payload);
            }

            // Get the value of "temp".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"temp");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            vms = fsr.Step();//Line 8

            // Get the value of "m".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"m");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(10, (Int64)objExecVal.Payload);
            }

            // Get the value of "temp".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"temp");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(0, (Int64)objExecVal.Payload);
            }

            vms = fsr.Step();//Line 9

            // Get the value of "m".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"m");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(10, (Int64)objExecVal.Payload);
            }

            // Get the value of "temp".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"temp");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(10, (Int64)objExecVal.Payload);
            }

            vms = fsr.Step();//Line 10

            // Get the value of "m".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"m");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(10, (Int64)objExecVal.Payload);
            }

            // Get the value of "temp".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"temp");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(10, (Int64)objExecVal.Payload);
            }
        }

        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void TestWatchExpressionImperative1()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
@"
[Imperative]
{
    first = 1;
    second = 2;
    third = 3;
    fourth = 4;
    fifth = 5;
}
", runnerConfig);

            // First step should bring the exec cursor to "first = 1;".
            DebugRunner.VMState vms = fsr.StepOver();
            Assert.AreEqual(4, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(4, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(15, vms.ExecutionCursor.EndExclusive.CharNo);

            {
                // This is to simulate the event when "first" is added into the watch window
                // before it gets assigned a value. This should not cause subsequent stepping
                // to go wrong.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"first");
                Obj objExecVal = mirror.GetWatchValue();

                // This should not return a valid payload as "first" is unassigned now.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            // Second step should bring it to "second = 2;".
            vms = fsr.StepOver();
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(5, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(16, vms.ExecutionCursor.EndExclusive.CharNo);

            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"first");
                Obj objExecVal = mirror.GetWatchValue();

                // This time "first" gets assigned a value of "1"...
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreNotEqual(null, objExecVal.Payload);
                Assert.AreEqual("1", objExecVal.Payload.ToString());
            }

            // Third step should bring it to "third = 3;".
            vms = fsr.StepOver();
            Assert.AreEqual(6, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(6, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(15, vms.ExecutionCursor.EndExclusive.CharNo);

            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"second");
                Obj objExecVal = mirror.GetWatchValue();

                // This time "second" gets assigned a value of "2"...
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreNotEqual(null, objExecVal.Payload);
                Assert.AreEqual("2", objExecVal.Payload.ToString());
            }

            // Third step should bring it to "fourth = 4;".
            vms = fsr.StepOver();
            Assert.AreEqual(7, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(7, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(16, vms.ExecutionCursor.EndExclusive.CharNo);
        }

        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void TestWatchExpressionImperative2()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
@"
[Imperative]
{
    a = 1;
    b = 20;
}

", runnerConfig);
            DebugRunner.VMState vms = fsr.Step();
            vms = fsr.Step();

            // Execute and verify the watch window expression script
            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"a");
            Obj objExecVal = mirror.GetWatchValue();
            Assert.IsTrue((Int64)objExecVal.Payload == 1);


            vms = fsr.Step();

            // Execute and verify the watch window expression script
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"b");
            objExecVal = mirror.GetWatchValue();
            Assert.IsTrue((Int64)objExecVal.Payload == 20);
        }

        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void TestWatchExpressionImperative3()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
@"
class Vector
{
    x : var;
    y : var;
    constructor Vector()
    {
        x = 10;
        y = 20;
    }
}

[Imperative]
{
    p = Vector.Vector();
    t = 0;
}

", runnerConfig);

            // Highlights "p = Vector.Vector()".
            DebugRunner.VMState vms = fsr.Step();
            Assert.AreEqual(15, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(15, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(25, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step(); // Highlight "x = 10;".
            Assert.AreEqual(8, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(8, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(16, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step(); // Highlight "y = 20;".
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(9, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(16, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step(); // Highlight the closing bracket of constructor "}".
            Assert.AreEqual(10, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(10, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(6, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step(); // Highlight the "p = Vector.Vector();".
            Assert.AreEqual(15, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(15, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(25, vms.ExecutionCursor.EndExclusive.CharNo);


            vms = fsr.Step(); // Highlight the "t = 0;"
            Assert.AreEqual(16, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(16, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(11, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step(); // Highlight "}" of the "Imperative" block
            Assert.AreEqual(17, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(17, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.EndExclusive.CharNo);

            // Execute and verify the watch window expression script
            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"p.x");
            Obj objExecVal = mirror.GetWatchValue();
            Assert.IsTrue((Int64)objExecVal.Payload == 10);
        }

        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void TestWatchExpressionImperative4()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
@"
[Associative]
{
}

[Imperative]
{
    eee = 43420;
}
", runnerConfig);

            // First step should highlight closing bracket of "Associative" block.
            DebugRunner.VMState vms = fsr.StepOver();
            Assert.AreEqual(4, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(4, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver(); // Highlight "eee = 43420;"
            Assert.AreEqual(8, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(8, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(17, vms.ExecutionCursor.EndExclusive.CharNo);

            // Get the value of "eee".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"eee");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            vms = fsr.StepOver(); // Highlight closing bracket of "Imperative" block.
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(9, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.EndExclusive.CharNo);

            // Get the value of "eee".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"eee");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreNotEqual(null, objExecVal.Payload);
                Assert.AreEqual("43420", objExecVal.Payload.ToString());
            }
        }

        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void TestWatchExpressionInClassMember1()
        {
            // Execute and verify the defect IDE-476
            fsr.PreStart(
@"class A //line 1
{
    a : var;
    a2 : var;
    a4 : var;
    
    
    constructor A(x : var)
    {
        a2 = 3;
        a = x;
    }
    def update(x : var)
    {
        a = {
            x => a1;
            a1 > 10 ? true : false => a4;
            
        }
        return = x;
    }
}

a1 = A.A(0);
a1 = A.A();
x = a1.update(1);
y = { a1.a1, a1.a4 };
", runnerConfig);

            DebugRunner.VMState vms = fsr.StepOver();//Line 24

            vms = fsr.Step();//Line 10

            // Get the value of "a2".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a2");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            vms = fsr.Step();//Line 11

            // Get the value of "a2".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a2");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreEqual((Int64)objExecVal.Payload, 3);
            }

            vms = fsr.Step();//Line 12

            // Get the value of "a2".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a2");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreEqual((Int64)objExecVal.Payload, 3);
            }

            vms = fsr.Step();//Line 24

            // Get the value of "a2".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a2");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            vms = fsr.Step();//Line 25

            // Get the value of "a2".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a2");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            vms = fsr.Step();//Line 25

            // Get the value of "a2".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a2");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            vms = fsr.Step();//Line 26

            // Get the value of "a2".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a2");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            vms = fsr.Step();//Line 16

            // Get the value of "a2".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a2");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            vms = fsr.Step();//Line 17

            // Get the value of "a2".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a2");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            vms = fsr.Step();//Line 15

            // Get the value of "a2".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a2");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            vms = fsr.Step();//Line 20

            // Get the value of "a2".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a2");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            vms = fsr.Step();//Line 21

            // Get the value of "a2".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a2");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            vms = fsr.Step();//Line 26

            // Get the value of "a2".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a2");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            vms = fsr.Step();//Line 27

            // Get the value of "a2".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a2");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            vms = fsr.Step();//ended

            // Get the value of "a2".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a2");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }
        }

        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void TestWatchExpressionInClassMember2()
        {
            // Execute and verify the case attached in the defect IDE-487
            fsr.PreStart(
@"class A //Line 1
{
        a : int;
    
        def foo()
        {
        
            a = 10;
            b = 2 * a;
            [Associative]
            {
                a = 1;
            }
            return = b;
        
        }
    
}
    
a = A.A();
b = a.foo();
", runnerConfig);

            DebugRunner.VMState vms = fsr.StepOver();//Line 20

            vms = fsr.Step();//Line 3

            // Get the value of "a".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            // Get the value of "b".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"b");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            vms = fsr.Step();//Line 20

            // Get the value of "a".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            // Get the value of "b".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"b");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            vms = fsr.Step();//Line 21

            // Get the value of "a".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Dictionary<string, Obj> os = vms.mirror.GetProperties(objExecVal);
                Assert.IsTrue(os.Count == 1);
                Assert.IsTrue((Int64)os["a"].Payload == 0);
            }

            // Get the value of "b".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"b");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            vms = fsr.Step();//Line 8

            // Get the value of "a".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(0, (Int64)objExecVal.Payload);
            }

            // Get the value of "b".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"b");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            vms = fsr.Step();//Line 9

            // Get the value of "a".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(10, (Int64)objExecVal.Payload);
            }

            // Get the value of "b".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"b");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            vms = fsr.Step();//Line 12

            // Get the value of "a".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(10, (Int64)objExecVal.Payload);
            }

            // Get the value of "b".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"b");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(20, (Int64)objExecVal.Payload);
            }

            vms = fsr.Step();//Line 13

            // Get the value of "a".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(1, (Int64)objExecVal.Payload);
            }

            // Get the value of "b".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"b");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(20, (Int64)objExecVal.Payload);
            }

            vms = fsr.Step();//Line 9

            // Get the value of "a".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(1, (Int64)objExecVal.Payload);
            }

            // Get the value of "b".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"b");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(20, (Int64)objExecVal.Payload);
            }

            vms = fsr.Step();//Line 14

            // Get the value of "a".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(1, (Int64)objExecVal.Payload);
            }

            // Get the value of "b".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"b");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(2, (Int64)objExecVal.Payload);
            }

            vms = fsr.Step();//Line 16

            // Get the value of "a".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(1, (Int64)objExecVal.Payload);
            }

            // Get the value of "b".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"b");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(2, (Int64)objExecVal.Payload);
            }

            vms = fsr.Step();//Line 21

            // Get the value of "a".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Dictionary<string, Obj> os = vms.mirror.GetProperties(objExecVal);
                Assert.IsTrue(os.Count == 1);
                Assert.IsTrue((Int64)os["a"].Payload == 1);
            }

            // Get the value of "b".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"b");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(2, (Int64)objExecVal.Payload);
            }

            vms = fsr.Step();//Line 22

            // Get the value of "a".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Dictionary<string, Obj> os = vms.mirror.GetProperties(objExecVal);
                Assert.IsTrue(os.Count == 1);
                Assert.IsTrue((Int64)os["a"].Payload == 1);
            }

            // Get the value of "b".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"b");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(2, (Int64)objExecVal.Payload);
            }
        }

        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void TestWatchExpressionForDotProperty()
        {
            // Execute and verify the case attached in the defect IDE-523
            fsr.PreStart(
@"class A //Line 1
{
    x;
}
a = A.A();
x = a.x;
a.x = a;
b = 33;
", runnerConfig);

            DebugRunner.VMState vms = fsr.StepOver();//Line 5

            vms = fsr.Step();//Line 5

            // Get the value of "a.x".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a.x");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            vms = fsr.Step();//Line 6

            // Get the value of "a.x".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a.y");
                Obj objExecVal = mirror.GetWatchValue();
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);

                mirror = watchRunner.Execute(@"a.x");
                objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            vms = fsr.Step();//Line 7

            // Get the value of "a.x".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a.y");
                Obj objExecVal = mirror.GetWatchValue();
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);

                mirror = watchRunner.Execute(@"a.x");
                objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            vms = fsr.Step();//Line 6

            // Get the value of "a.x".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a.y");
                Obj objExecVal = mirror.GetWatchValue();
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);

                mirror = watchRunner.Execute(@"a.x");
                objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreNotEqual(null, objExecVal.Payload);
                Assert.AreEqual(mirror.GetType(objExecVal), "A");
            }

            vms = fsr.Step();//Line 8

            // Get the value of "a.x".
            {
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a.y");
                Obj objExecVal = mirror.GetWatchValue();
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);

                mirror = watchRunner.Execute(@"a.x");
                objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreNotEqual(null, objExecVal.Payload);
                Assert.AreEqual(mirror.GetType(objExecVal), "A");
            }
        }

        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void TestWatchExpressionInNestedBlock1()
        {
            // Execute and verify the defect IDE-487
            fsr.PreStart(
@"class test { a = 1; } //line 1

c1 = [Imperative]
{
    a = test.test();
    b = [Associative]
    {
        return = test.test();
    }
    
    c = a.a + b.a;
    return = c;
}

c2 = [Associative]
{
    a = test.test();
    b = [Imperative]
    {
        return = test.test();
    }
    
    c = a.a + b.a;
    return = c;
}
", runnerConfig);

            DebugRunner.VMState vms = fsr.StepOver();//Line 5

            vms = fsr.Step();//Line 1

            {
                // 1. Get the value of "a".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            {
                // 2. Get the value of "b".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"b");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            {
                // 3.Get the value of "c".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"c");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }


            vms = fsr.Step();//Line 5

            {
                // 1. Get the value of "a".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            {
                // 2. Get the value of "b".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"b");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            {
                // 3.Get the value of "c".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"c");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            vms = fsr.Step();//Line 8

            {
                // 1. Get the value of "a".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Dictionary<string, Obj> os = vms.mirror.GetProperties(objExecVal);
                Assert.IsTrue(os.Count == 1);
                Assert.IsTrue((Int64)os["a"].Payload == 1);
            }

            {
                // 2. Get the value of "b".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"b");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            {
                // 3.Get the value of "c".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"c");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            vms = fsr.Step();//Line 1

            {
                // 1. Get the value of "a".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            {
                // 2. Get the value of "b".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"b");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            {
                // 3.Get the value of "c".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"c");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            vms = fsr.Step();//Line 8

            {
                // 1. Get the value of "a".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Dictionary<string, Obj> os = vms.mirror.GetProperties(objExecVal);
                Assert.IsTrue(os.Count == 1);
                Assert.IsTrue((Int64)os["a"].Payload == 1);
            }

            {
                // 2. Get the value of "b".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"b");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            {
                // 3.Get the value of "c".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"c");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            vms = fsr.Step();//Line 9

            {
                // 1. Get the value of "a".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Dictionary<string, Obj> os = vms.mirror.GetProperties(objExecVal);
                Assert.IsTrue(os.Count == 1);
                Assert.IsTrue((Int64)os["a"].Payload == 1);
            }

            {
                // 2. Get the value of "b".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"b");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            {
                // 3.Get the value of "c".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"c");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            vms = fsr.Step();//Line 6

            {
                // 1. Get the value of "a".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Dictionary<string, Obj> os = vms.mirror.GetProperties(objExecVal);
                Assert.IsTrue(os.Count == 1);
                Assert.IsTrue((Int64)os["a"].Payload == 1);
            }

            {
                // 2. Get the value of "b".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"b");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            {
                // 3.Get the value of "c".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"c");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            vms = fsr.Step();//Line 11

            {
                // 1. Get the value of "a".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Dictionary<string, Obj> os = vms.mirror.GetProperties(objExecVal);
                Assert.IsTrue(os.Count == 1);
                Assert.IsTrue((Int64)os["a"].Payload == 1);
            }

            {
                // 2. Get the value of "b".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"b");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Dictionary<string, Obj> os = vms.mirror.GetProperties(objExecVal);
                Assert.IsTrue(os.Count == 1);
                Assert.IsTrue((Int64)os["a"].Payload == 1);
            }

            {
                // 3.Get the value of "c".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"c");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            vms = fsr.Step();//Line 12

            {
                // 1. Get the value of "a".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Dictionary<string, Obj> os = vms.mirror.GetProperties(objExecVal);
                Assert.IsTrue(os.Count == 1);
                Assert.IsTrue((Int64)os["a"].Payload == 1);
            }

            {
                // 2. Get the value of "b".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"b");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Dictionary<string, Obj> os = vms.mirror.GetProperties(objExecVal);
                Assert.IsTrue(os.Count == 1);
                Assert.IsTrue((Int64)os["a"].Payload == 1);
            }

            {
                // 3.Get the value of "c".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"c");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual((Int64)objExecVal.Payload, 2);
            }

            vms = fsr.Step();//Line 13

            {
                // 1. Get the value of "a".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Dictionary<string, Obj> os = vms.mirror.GetProperties(objExecVal);
                Assert.IsTrue(os.Count == 1);
                Assert.IsTrue((Int64)os["a"].Payload == 1);
            }

            {
                // 2. Get the value of "b".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"b");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Dictionary<string, Obj> os = vms.mirror.GetProperties(objExecVal);
                Assert.IsTrue(os.Count == 1);
                Assert.IsTrue((Int64)os["a"].Payload == 1);
            }

            {
                // 3.Get the value of "c".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"c");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual((Int64)objExecVal.Payload, 2);
            }

            vms = fsr.Step();//Line 3

            {
                // 1. Get the value of "a".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            {
                // 2. Get the value of "b".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"b");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            {
                // 3.Get the value of "c".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"c");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            vms = fsr.Step();//Line 17

            {
                // 1. Get the value of "a".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            {
                // 2. Get the value of "b".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"b");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            {
                // 3.Get the value of "c".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"c");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            vms = fsr.Step();//Line 1

            {
                // 1. Get the value of "a".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            {
                // 2. Get the value of "b".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"b");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            {
                // 3.Get the value of "c".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"c");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }


            vms = fsr.Step();//Line 17

            {
                // 1. Get the value of "a".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            {
                // 2. Get the value of "b".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"b");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            {
                // 3.Get the value of "c".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"c");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            vms = fsr.Step();//Line 20

            {
                // 1. Get the value of "a".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Dictionary<string, Obj> os = vms.mirror.GetProperties(objExecVal);
                Assert.IsTrue(os.Count == 1);
                Assert.IsTrue((Int64)os["a"].Payload == 1);
            }

            {
                // 2. Get the value of "b".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"b");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            {
                // 3.Get the value of "c".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"c");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            vms = fsr.Step();//Line 1

            {
                // 1. Get the value of "a".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            {
                // 2. Get the value of "b".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"b");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            {
                // 3.Get the value of "c".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"c");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            vms = fsr.Step();//Line 20

            {
                // 1. Get the value of "a".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Dictionary<string, Obj> os = vms.mirror.GetProperties(objExecVal);
                Assert.IsTrue(os.Count == 1);
                Assert.IsTrue((Int64)os["a"].Payload == 1);
            }

            {
                // 2. Get the value of "b".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"b");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            {
                // 3.Get the value of "c".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"c");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            vms = fsr.Step();//Line 21

            {
                // 1. Get the value of "a".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Dictionary<string, Obj> os = vms.mirror.GetProperties(objExecVal);
                Assert.IsTrue(os.Count == 1);
                Assert.IsTrue((Int64)os["a"].Payload == 1);
            }

            {
                // 2. Get the value of "b".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"b");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            {
                // 3.Get the value of "c".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"c");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            vms = fsr.Step();//Line 18

            {
                // 1. Get the value of "a".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Dictionary<string, Obj> os = vms.mirror.GetProperties(objExecVal);
                Assert.IsTrue(os.Count == 1);
                Assert.IsTrue((Int64)os["a"].Payload == 1);
            }

            {
                // 2. Get the value of "b".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"b");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            {
                // 3.Get the value of "c".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"c");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            vms = fsr.Step();//Line 23

            {
                // 1. Get the value of "a".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Dictionary<string, Obj> os = vms.mirror.GetProperties(objExecVal);
                Assert.IsTrue(os.Count == 1);
                Assert.IsTrue((Int64)os["a"].Payload == 1);
            }

            {
                // 2. Get the value of "b".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"b");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Dictionary<string, Obj> os = vms.mirror.GetProperties(objExecVal);
                Assert.IsTrue(os.Count == 1);
                Assert.IsTrue((Int64)os["a"].Payload == 1);
            }

            {
                // 3.Get the value of "c".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"c");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            vms = fsr.Step();//Line 24

            {
                // 1. Get the value of "a".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Dictionary<string, Obj> os = vms.mirror.GetProperties(objExecVal);
                Assert.IsTrue(os.Count == 1);
                Assert.IsTrue((Int64)os["a"].Payload == 1);
            }

            {
                // 2. Get the value of "b".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"b");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Dictionary<string, Obj> os = vms.mirror.GetProperties(objExecVal);
                Assert.IsTrue(os.Count == 1);
                Assert.IsTrue((Int64)os["a"].Payload == 1);
            }

            {
                // 3.Get the value of "c".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"c");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual((Int64)objExecVal.Payload, 2);
            }

            vms = fsr.Step();//Line 25

            {
                // 1. Get the value of "a".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Dictionary<string, Obj> os = vms.mirror.GetProperties(objExecVal);
                Assert.IsTrue(os.Count == 1);
                Assert.IsTrue((Int64)os["a"].Payload == 1);
            }

            {
                // 2. Get the value of "b".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"b");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Dictionary<string, Obj> os = vms.mirror.GetProperties(objExecVal);
                Assert.IsTrue(os.Count == 1);
                Assert.IsTrue((Int64)os["a"].Payload == 1);
            }

            {
                // 3.Get the value of "c".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"c");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual((Int64)objExecVal.Payload, 2);
            }

            vms = fsr.Step();//Line 15

            {
                // 1. Get the value of "a".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            {
                // 2. Get the value of "b".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"b");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }

            {
                // 3.Get the value of "c".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"c");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }
        }

        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void TestWatchExpressionInNestedBlock2()
        {
            // Execute and verify the defect IDE-519
            fsr.PreStart(
@"[Associative] // Line 1
{
    [Imperative]
    {
        i = 3;
    } // The value of 'i' cannot be inspected here.
    
    // If this line is removed, then 'i' can be inspected.
    f = i;
}
", runnerConfig);

            DebugRunner.VMState vms = fsr.StepOver();//Line 5

            vms = fsr.Step();//Line 6

            {
                // Get the value of "i".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"i");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(3, (Int64)objExecVal.Payload);
            }

            vms = fsr.Step();//Line 7

            {
                // Get the value of "i".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"i");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(null, objExecVal.Payload);
            }
        }

        [Test]
        [Category("Debugger")]
        public void TestImperativeExec()
        {

            String code =
@"[Imperative]
{
	foo = 5;
    bah = 10;
}
";


            //ProtoScript.Runners.DebugRunner fsr = new ProtoScript.Runners.DebugRunner(null);
            fsr.PreStart(code, runnerConfig);

            DebugRunner.VMState vms = fsr.Step();
            vms = fsr.Step();
            Object o = vms.mirror.GetFirstValue("foo").Payload;// .GetDebugValue("foo").Payload;
            Assert.IsTrue((Int64)o == 5);
        }

        [Test]
        [Category("Debugger")]
        [Category("ModifierBlock")] [Category("Failure")]
        public void TestModifierBlockDebugging1()
        {

            String code =
@"
b1 = 1;

class A
{
    x : int;
    
    constructor A()
    {
        x = 1;
    }
    
	def foo()
	{
	    return = 90;
	}
}

a =
{    
	A.A() => a2;	
    a2.foo();		  
    b1 + 1;
    +2 => a1;
    a1 + b1;
    
}
c = 90;
";
            //Assert.Fail("IDE-433Debugger does not step through few modifier block cases");

            fsr.PreStart(code, runnerConfig);

            DebugRunner.VMState vms = fsr.Step();
            vms = fsr.Step();

            Obj o = vms.mirror.GetDebugValue("b1");
            string type = vms.mirror.GetType("b1");

            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 1);

            vms = fsr.Step();
            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("x");
            type = vms.mirror.GetType("x");

            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 0);

            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("x");
            type = vms.mirror.GetType("x");

            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 1);

            vms = fsr.Step();
            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("a2");
            type = vms.mirror.GetType("a2");

            Dictionary<string, Obj> os = vms.mirror.GetProperties(o);
            Assert.IsTrue(type == "A");
            Assert.IsTrue(os.Count == 1);
            Assert.IsTrue((Int64)os["x"].Payload == 1);

            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();

            // we should now be at "b1 + 1;"
            vms = fsr.Step();

            // we should now be at "+2 => a1;"
            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a1");
            type = vms.mirror.GetType("a1");

            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 4);

            //vms = fsr.Step();
            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a");
            type = vms.mirror.GetType("a");

            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 5);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("c");
            type = vms.mirror.GetType("c");

            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 90);
        }

        [Test]
        [Category("Debugger")]
        [Category("ModifierBlock")] [Category("Failure")]
        public void TestModifierBlockDebugging2()
        {

            String code =
@"
b1 = 1;

class A
{
    x : int;
    
    constructor A()
    {
        x = 1;
    }
}

    def foo()
	{
	    return = 90;
	}

a =
{
    
	A.A() => a2;	
    foo();		  
    b1 + 1;
    +2 => a1;
    a1 + b1;
    
}
c = 90;
";
            fsr.PreStart(code, runnerConfig);

            DebugRunner.VMState vms = fsr.Step();
            vms = fsr.Step();

            Obj o = vms.mirror.GetDebugValue("b1");
            string type = vms.mirror.GetType("b1");

            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 1);

            vms = fsr.Step();
            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("x");
            type = vms.mirror.GetType("x");

            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 0);

            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("x");
            type = vms.mirror.GetType("x");

            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 1);

            vms = fsr.Step();
            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("a2");
            type = vms.mirror.GetType("a2");

            Dictionary<string, Obj> os = vms.mirror.GetProperties(o);
            Assert.IsTrue(type == "A");
            Assert.IsTrue(os.Count == 1);
            Assert.IsTrue((Int64)os["x"].Payload == 1);

            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("a1");
            type = vms.mirror.GetType("a1");

            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 4);

            //vms = fsr.Step();
            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a");
            type = vms.mirror.GetType("a");

            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 5);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("c");
            type = vms.mirror.GetType("c");

            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 90);
        }

        [Test]
        [Category("Debugger")]
        [Category("ModifierBlock")] [Category("Failure")]
        public void TestModifierBlockDebugging3()
        {

            String code =
@"
class B
{
	x : var;
	constructor B ( y )
	{
		x = y;
	}
    
    def foo()
    {
        return = 90;
    }
}

x = 1;
a =
{
	x => a1;
	 - 0.5 => a2;
	a2 * 4 => a3;
	a1 > 10 ? true : false => a4;
	a1..2 => a5;
	{ a3, a3 } => a6;
    B.B(a1) => a7;
    foo();
    B.B(a1).x => a8;    
}
";
            fsr.PreStart(code, runnerConfig);

            DebugRunner.VMState vms = fsr.Step();
            vms = fsr.Step();

            Obj o = vms.mirror.GetDebugValue("x");
            string type = vms.mirror.GetType("x");

            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 1);

            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("a1");
            type = vms.mirror.GetType("a1");

            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 1);

            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("a2");
            type = vms.mirror.GetType("a2");

            Assert.IsTrue(type == "double");
            Assert.IsTrue((Double)o.Payload == 0.5);

            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("a3");
            type = vms.mirror.GetType("a3");

            Assert.IsTrue(type == "double");
            Assert.IsTrue((Double)o.Payload == 2.0);

            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("a4");
            type = vms.mirror.GetType("a4");

            Assert.IsTrue(type == "bool");
            Assert.IsTrue((Boolean)o.Payload == false);

            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("a5");
            type = vms.mirror.GetType("a5");

            Assert.IsTrue(type == "array");
            List<Obj> lo = vms.mirror.GetArrayElements(o);
            Assert.IsTrue((Int64)lo[0].Payload == 1);
            Assert.IsTrue((Int64)lo[1].Payload == 2);

            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("a6");
            type = vms.mirror.GetType("a6");

            Assert.IsTrue(type == "array");
            lo = vms.mirror.GetArrayElements(o);
            Assert.IsTrue((Double)lo[0].Payload == 2.0);
            Assert.IsTrue((Double)lo[1].Payload == 2.0);

            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("x");
            type = vms.mirror.GetType("x");

            Assert.IsTrue(type == "null");
            Assert.IsTrue(o.Payload == null);

            o = vms.mirror.GetDebugValue("y");
            type = vms.mirror.GetType("y");

            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 1);

            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("x");
            type = vms.mirror.GetType("x");

            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 1);

            vms = fsr.Step();
            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("a7");
            type = vms.mirror.GetType("a7");

            Dictionary<string, Obj> os = vms.mirror.GetProperties(o);
            Assert.IsTrue(type == "B");
            Assert.IsTrue(os.Count == 1);
            Assert.IsTrue((Int64)os["x"].Payload == 1);

            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("x");
            type = vms.mirror.GetType("x");

            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 1);

            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("x");
            type = vms.mirror.GetType("x");

            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 1);

            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("y");
            type = vms.mirror.GetType("y");

            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 1);

            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("a8");
            type = vms.mirror.GetType("a8");

            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 1);

            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("a");
            type = vms.mirror.GetType("a");

            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 1);
        }

        [Test]
        [Category("Debugger")]
        public void TestStepNext()
        {
            String code =
@"
	a = 1;
    b = 2.0;
    c = true;
    d = null;
";
            fsr.PreStart(code, runnerConfig);
            DebugRunner.VMState vms = fsr.Step();
            vms = fsr.Step();
            Obj o = vms.mirror.GetDebugValue("a");
            string type = vms.mirror.GetType("a");

            Assert.IsTrue((Int64)o.Payload == 1);
            Assert.IsTrue(type == "int");

            int startLineNo, startCharNo, endLineNo, endCharNo;

            startLineNo = vms.ExecutionCursor.StartInclusive.LineNo;
            startCharNo = vms.ExecutionCursor.StartInclusive.CharNo;

            endLineNo = vms.ExecutionCursor.EndExclusive.LineNo;
            endCharNo = vms.ExecutionCursor.EndExclusive.CharNo;

            Assert.IsTrue((Int64)startLineNo == 3);
            Assert.IsTrue((Int64)startCharNo == 5);

            Assert.IsTrue((Int64)endLineNo == 3);
            Assert.IsTrue((Int64)endCharNo == 13);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("b");
            type = vms.mirror.GetType("b");

            Assert.IsTrue((double)o.Payload == 2.0);
            Assert.IsTrue(type == "double");

            startLineNo = vms.ExecutionCursor.StartInclusive.LineNo;
            startCharNo = vms.ExecutionCursor.StartInclusive.CharNo;

            Assert.IsTrue((Int64)startLineNo == 4);
            Assert.IsTrue((Int64)startCharNo == 5);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("c");
            type = vms.mirror.GetType("c");

            Assert.IsTrue((Boolean)o.Payload);
            Assert.IsTrue(type == "bool");

            startLineNo = vms.ExecutionCursor.StartInclusive.LineNo;
            startCharNo = vms.ExecutionCursor.StartInclusive.CharNo;

            Assert.IsTrue((Int64)startLineNo == 5);
            Assert.IsTrue((Int64)startCharNo == 5);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("d");
            type = vms.mirror.GetType("d");

            Assert.IsTrue(o.Payload == null);
            Assert.IsTrue(type == "null");

            startLineNo = vms.ExecutionCursor.StartInclusive.LineNo;
            startCharNo = vms.ExecutionCursor.StartInclusive.CharNo;

            Assert.IsTrue((Int64)startLineNo == 0);
            Assert.IsTrue((Int64)startCharNo == 0);
        }

        [Test]
        [Category("Debugger")]
        public void TestStepNextClass()
        {
            String code =
@"
class V
{
	x : int;
	y : int;
	constructor V()
	{
		x = 10; 
		y = 20; 
	}
}

p = V.V();
a = p.x;
";
            fsr.PreStart(code, runnerConfig);
            DebugRunner.VMState vms = fsr.Step();
            vms = fsr.StepOver();
            vms = fsr.StepOver();

            // First var
            Obj o = vms.mirror.GetDebugValue("p");
            string type = vms.mirror.GetType("p");

            Assert.IsTrue(type == "V");

            // Second var
            o = vms.mirror.GetDebugValue("a");
            type = vms.mirror.GetType("a");

            Assert.IsTrue((Int64)o.Payload == 10);
            Assert.IsTrue(type == "int");
        }

        [Test]
        [Category("Debugger")]
        public void TestStepIntoSingleFunction()
        {
            String code =
@"
    def f : int(i : int, j : int)
    {
        a = 5;
        return = 7;
    }

    a = f(1, 2);
    b = 2;
    c = 3;
";
            fsr.PreStart(code, runnerConfig);
            fsr.Step(); // "f(1, 2)"
            fsr.Step(); // "a = 5;"
            DebugRunner.VMState vms = fsr.Step(); // "return = 7;"

            Obj o = vms.mirror.GetDebugValue("a");
            string type = vms.mirror.GetType("a");

            Assert.IsTrue((Int64)o.Payload == 5);
            Assert.IsTrue(type == "int");

            // "return = 7;" statement highlighted.
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.CharNo);

            vms = fsr.Step(); // Closing bracket "}" of "f".
            Assert.AreEqual(6, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);

            fsr.Step(); // "a = f(1, 2);"
            vms = fsr.Step(); // "b = 2;"
            o = vms.mirror.GetDebugValue("a");
            Assert.AreEqual(7, (Int64)o.Payload);

            vms = fsr.Step(); // "c = 3;"
            o = vms.mirror.GetDebugValue("b");
            Assert.AreEqual(2, (Int64)o.Payload);

            vms = fsr.Step(); // Ended.
            o = vms.mirror.GetDebugValue("c");
            Assert.AreEqual(3, (Int64)o.Payload);
        }

        [Test]
        [Category("Debugger")]
        public void TestStepIntoFunctionFromLangBlock()
        {
            String code =
                @"
def f (input) 
{
	return = input * 2;
}

val = [Imperative] 
{
	sum = 1;
	sum1 = f(sum);

    return = sum1;
}
";
            fsr.PreStart(code, runnerConfig);
            fsr.Step();

            DebugRunner.VMState vms = fsr.Step();
            Object o = vms.mirror.GetDebugValue("sum").Payload;
            Assert.IsTrue((Int64)o == 1);

            // The second step-in should highlight "sum1 = f(sum)" statement.
            Assert.AreEqual(10, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(10, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(16, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step(); // Third step-in.
            Assert.AreEqual(4, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(4, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(21, vms.ExecutionCursor.EndExclusive.CharNo);
        }

        [Test]
        [Category("Debugger")]
        public void TestStepInOutLanguageBlockImperative()
        {
            string sourceCode = @"
b = 5;

c = [Imperative]
{
    x = 4 + b;
    return = x;
}

d = c;
";

            fsr.PreStart(sourceCode, runnerConfig);

            DebugRunner.VMState vms = fsr.StepOver(); // "b = 5;"
            Assert.AreEqual(2, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(2, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(7, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver(); // "x = 4 + b;"
            Assert.AreEqual(6, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(6, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(15, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver(); // "return = x;"
            Assert.AreEqual(7, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(7, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(16, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver(); // "}" of the "Imperative" block
            Assert.AreEqual(8, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(8, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver(); // The entire "c = ..." assignment block
            Assert.AreEqual(4, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(8, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver(); // "d = c;"
            Assert.AreEqual(10, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(10, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(7, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver();
            Assert.AreEqual(true, vms.isEnded);
        }

        [Test]
        [Category("Debugger")]
        public void TestFunctionCallImperativeBlock_Defect_IDE_603()
        {
            string sourceCode = @"[Imperative]
{
    def f(input)
    {
        return = input * 2;
    }
    
    sum = 0;
    sum = 2 + f(1);
    k = 22;
}
";
            fsr.PreStart(sourceCode, runnerConfig);

            DebugRunner.VMState vms = fsr.Step(); //sum = 0;
            Assert.AreEqual(8, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(8, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(13, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step(); // sum = 2 + f(1);
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(9, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(20, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver(); // k = 22;
            Assert.AreEqual(10, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(10, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(12, vms.ExecutionCursor.EndExclusive.CharNo);
        }

        [Test]
        [Category("Debugger")]
        public void TestFunctionCallImperativeBlock_Defect_IDE_603_1()
        {
            string sourceCode = @"[Imperative]
{
    def f(input)
    {
        return = input * 2;
    }
    
    sum = 0;
    sum = f(1) + 2;
    k = 22;
}
";
            fsr.PreStart(sourceCode, runnerConfig);

            DebugRunner.VMState vms = fsr.Step(); //sum = 0;
            Assert.AreEqual(8, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(8, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(13, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step(); // sum = f(1) + 2;
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(9, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(20, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver(); // k = 22;
            Assert.AreEqual(10, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(10, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(12, vms.ExecutionCursor.EndExclusive.CharNo);
        }

        [Test]
        [Category("Debugger")]
        public void TestInlineConditionImperativeBlock_Defect_IDE_637()
        {
            string sourceCode = @"x = 330;

def foo(y : int)
{
    return = y + 222;
}

[Imperative]
{
    a = x < foo(22) ? 3 : 55;
}";
            fsr.PreStart(sourceCode, runnerConfig);

            DebugRunner.VMState vms = fsr.Step(); //x = 330;
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(1, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step(); //a = x < foo(22) ? 3 : 55;
            Assert.AreEqual(10, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(10, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(30, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver(); // closing brace of Imperative block
            Assert.AreEqual(11, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(11, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.EndExclusive.CharNo);
        }

        [Test]
        [Category("Debugger")]
        public void TestIfStmtImperativeBlock_Defect_IDE_637_0()
        {
            string sourceCode = @"x = 330;

def foo(y : int)
{
    return = y + 222;
}

[Imperative]
{
	if(x > foo(22))
	{
		a = 3;
	}
	else
	{
		a = 55;
	}
}";
            //Assert.Fail("Regression with if statement");
            fsr.PreStart(sourceCode, runnerConfig);

            DebugRunner.VMState vms = fsr.Step(); //x = 330;
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(1, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step(); //if(x < foo(22))
            Assert.AreEqual(10, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(10, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(17, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver(); // a = 3;
            Assert.AreEqual(12, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(3, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(12, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step(); // closing brace of if 
            vms = fsr.Step(); // closing brace of Imperative block
            Assert.AreEqual(18, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(18, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.EndExclusive.CharNo);

        }

        [Test]
        [Category("Debugger")]
        public void TestIfStmtImperativeBlock_Defect_IDE_637_1()
        {
            string sourceCode = @"x = 330;

def foo(y : int)
{
    return = y + 222;
}

[Imperative]
{
	if(x < foo(22))
	{
		a = 3;
	}
	else
	{
		a = 55;
	}
}";
            fsr.PreStart(sourceCode, runnerConfig);

            DebugRunner.VMState vms = fsr.Step(); //x = 330;
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(1, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step(); //if(x < foo(22))
            Assert.AreEqual(10, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(10, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(17, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver(); // a = 55;
            Assert.AreEqual(16, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(3, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(16, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(10, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step(); // closing brace of else 
            vms = fsr.Step(); // closing brace of Imperative block
            Assert.AreEqual(18, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(18, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.EndExclusive.CharNo);
        }

        [Test]
        [Category("Debugger")]
        public void TestInlineConditionImperativeBlock_Defect_IDE_637_2()
        {
            string sourceCode = @"x = 330;

def foo(y : int)
{
    return = y + 222;
}

[Imperative]
{
    a = x < foo(22) ? 3 : 55;
}";
            fsr.PreStart(sourceCode, runnerConfig);

            DebugRunner.VMState vms = fsr.Step(); //x = 330;
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(1, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step(); //a = x < foo(22) ? 3 : 55;
            Assert.AreEqual(10, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(10, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(30, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step(); //return = y + 222;
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(5, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(22, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step(); //closing brace of foo()
            Assert.AreEqual(6, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(6, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step(); //a = x < foo(22) ? 3 : 55;
            Assert.AreEqual(10, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(10, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(30, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step(); // closing brace of Imperative block
            Assert.AreEqual(11, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(11, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.EndExclusive.CharNo);
        }

        [Test]
        [Category("Debugger")]
        public void TestIfStmtImperativeBlock_Defect_IDE_637_3()
        {
            string sourceCode = @"x = 330;

def foo(y : int)
{
    return = y + 222;
}

[Imperative]
{
	if(x < foo(22))
	{
		a = 3;
	}
	else
	{
		a = 55;
	}
}";
            fsr.PreStart(sourceCode, runnerConfig);

            DebugRunner.VMState vms = fsr.Step(); //x = 330;
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(1, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step(); //if(x < foo(22))
            Assert.AreEqual(10, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(10, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(17, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step(); // return = y + 222;
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(5, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(22, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step(); // closing brace of foo()
            Assert.AreEqual(6, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(6, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step(); //if(x < foo(22))
            Assert.AreEqual(10, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(10, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(17, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step(); // a = 55;
            Assert.AreEqual(16, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(3, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(16, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(10, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step(); // closing brace of else statement
            vms = fsr.Step(); // closing brace of Imperative block
            Assert.AreEqual(18, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(18, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.EndExclusive.CharNo);
        }

        [Test]
        [Category("Debugger")]
        public void TestUnaryExpressionWithFunctionCallImperativeBlock()
        {
            string sourceCode = @"[Imperative]
{
    def f(input)
    {
        return = input * 2;
    }
    
    sum = 0;
    sum = !f(1);
    k = 22;
}
";
            fsr.PreStart(sourceCode, runnerConfig);

            DebugRunner.VMState vms = fsr.Step(); //sum = 0;
            Assert.AreEqual(8, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(8, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(13, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step(); // sum = !f(1);
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(9, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(17, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver(); // k = 22;
            Assert.AreEqual(10, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(10, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(12, vms.ExecutionCursor.EndExclusive.CharNo);
        }

        [Test]
        [Category("Debugger")]
        public void TestStepInOutLanguageBlockAssociative()
        {
            string sourceCode = @"
[Imperative]
{
    b = 5;
    
    c = [Associative]
    {
        y = b + 2;
        return = y;
    }
    
    d = c;
}
";

            fsr.PreStart(sourceCode, runnerConfig);

            DebugRunner.VMState vms = fsr.StepOver(); // "b = 5;"
            Assert.AreEqual(4, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(4, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(11, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver(); // "y = b + 2;"
            Assert.AreEqual(8, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(8, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(19, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver(); // "return = y;"
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(9, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(20, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver(); // "}" of the "Associative" block
            Assert.AreEqual(10, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(10, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(6, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver(); // The entire "c = ..." assignment block
            Assert.AreEqual(6, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(10, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(6, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver(); // "d = c;"
            Assert.AreEqual(12, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(12, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(11, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver(); // "}" of the "Imperative" block
            Assert.AreEqual(13, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(13, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver();
            Assert.AreEqual(true, vms.isEnded);
        }

        [Test]
        [Category("Debugger")]
        public void TestStepNestedFunctionsFromLangBlock0()
        {
            String code =
                @"
def f1()
{
    a = 99;
    b = f2();
    return = b;
}

def f2 : int()
{
    return = 2;
}

v = 90;

[Imperative]
{
    c = 4;
    a = f1();
    b = 98;
}

p = { 9, 0 };
";
            fsr.PreStart(code, runnerConfig);
            fsr.Step();

            DebugRunner.VMState vms = fsr.Step();
            Obj o = vms.mirror.GetDebugValue("v");
            string type = vms.mirror.GetType("v");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 90);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("c");
            type = vms.mirror.GetType("c");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 4);

            vms = fsr.Step();
            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("a");
            type = vms.mirror.GetType("a");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 99);

            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("b");
            type = vms.mirror.GetType("b");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 2);

            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("a");
            type = vms.mirror.GetType("a");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 2);

            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("b");
            type = vms.mirror.GetType("b");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 98);

            fsr.Step();
            fsr.Step();

            o = vms.mirror.GetDebugValue("p");
            type = vms.mirror.GetType("p");
            List<Obj> lo = vms.mirror.GetArrayElements(o);
            Assert.IsTrue(type == "array");
            Assert.IsTrue((Int64)lo[0].Payload == 9);
            Assert.IsTrue((Int64)lo[1].Payload == 0);
        }

        [Test]
        [Category("Debugger")]
        public void TestRunDebugManyFunctions()
        {
            String code =
                @"
def fadd : int(a : int)
{
	return = a + 1;
}

x = 1;

a = fadd(x);
a = fadd(x);
a = fadd(x);
a = fadd(x);
a = fadd(x);
a = fadd(x);
a = fadd(x);
a = fadd(x);
a = fadd(x);
a = fadd(x);
a = fadd(x);
a = fadd(x);
a = fadd(x);
a = fadd(x);
a = fadd(x);
a = fadd(x);
a = fadd(x);
a = fadd(x);
";
            //Assert.Fail("IDE-266: Placing breakpoint and Run (Debug) twice crashes DesignScript");

            fsr.PreStart(code, runnerConfig);
            //fsr.Step();

            fsr.Run();
            Assert.AreEqual(fsr.isEnded, true);
        }

        [Test]
        [Category("Debugger")]
        public void TestRunDebugManyFunctions2()
        {
            String code =
                @"
class Line
{
    _x : int;
    _y : int;
    
    constructor Line(x : int, y : int)
    {
        _x = x;
        _y = y;
    }
    
    static def Create(x:int, y:int)
    {
        return = Line.Line(x, y);
    }
}

x = 1;
y = 2;
l = Line.Create(x, y);
l = Line.Create(x, y);
l = Line.Create(x, y);
l = Line.Create(x, y);
l = Line.Create(x, y);
l = Line.Create(x, y);
";
            //Assert.Fail("IDE-266: Placing breakpoint and Run (Debug) twice crashes DesignScript");

            fsr.PreStart(code, runnerConfig);
            fsr.Step();

            fsr.Run();
        }

        [Test]
        [Category("Debugger")]
        public void TestRunDebugManyCtors()
        {
            String code =
                @"
class Line
{
    _x : int;
    _y : int;
    
    constructor Line(x : int, y : int)
    {
        _x = x;
        _y = y;
    }
    
    static def Create(x:int, y:int)
    {
        return = Line.Line(x, y);
    }
}

x = 1;
y = 2;
l = Line.Line(x, y);
l = Line.Line(x, y);
l = Line.Line(x, y);
l = Line.Line(x, y);
l = Line.Line(x, y);
l = Line.Line(x, y);
l = Line.Line(x, y);
l = Line.Line(x, y);
l = Line.Line(x, y);
l = Line.Line(x, y);
l = Line.Line(x, y);
l = Line.Line(x, y);
";
            //Assert.Fail("IDE-266: Placing breakpoint and Run (Debug) twice crashes DesignScript");

            fsr.PreStart(code, runnerConfig);
            fsr.Step();

            fsr.Run();
        }

        [Test]
        [Category("Debugger")]
        public void TestStepIntoManyFunctions()
        {
            String code =
@"
    def g()
    {
        a = 9;
        return = 0;
    }

    def f()
    {
        a = 5;
        return = 7;
    }

    a = f();
    b = 2;
    c = 3;
";
            fsr.PreStart(code, runnerConfig);
            fsr.Step(); // "f()"
            fsr.Step(); // "a = 5;"
            DebugRunner.VMState vms = fsr.Step(); // "return = 7;"
            Obj o = vms.mirror.GetDebugValue("a");
            string type = vms.mirror.GetType("a");

            Assert.IsTrue((Int64)o.Payload == 5);
            Assert.IsTrue(type == "int");

            // Highlight "return = 7;" statement.
            Assert.AreEqual(11, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.CharNo);

            vms = fsr.Step(); // Closing "}" of function "f".
            Assert.AreEqual(12, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
        }

        [Test]
        [Category("Debugger")]
        public void TestStepIntoClassConstructor1()
        {
            String code =
@"
    class A
    {
        p1 : int;
                
	    constructor A ( a : int )
	    {
		    p1 = a;
		    b=6.0;
	    }

	    def add ( )
	    {
		    b=5;
		    return = p1 + 1;
	    }
    }

    a1 = A.A(7);
    b1 = a1.add();
";
            fsr.PreStart(code, runnerConfig);
            fsr.Step();

            fsr.Step();
            fsr.Step();
            DebugRunner.VMState vms = fsr.Step();
            Obj o = vms.mirror.GetDebugValue("p1");
            string type = vms.mirror.GetType("p1");

            Assert.IsTrue((Int64)o.Payload == 7);
            Assert.IsTrue(type == "int");

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("b");
            type = vms.mirror.GetType("b");

            Assert.IsTrue((double)o.Payload == 6.0);
            Assert.IsTrue(type == "double");

            fsr.Step();
            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a1");
            Assert.IsTrue(vms.mirror.GetType(o) == "A");
            Dictionary<string, Obj> os = vms.mirror.GetProperties(o);
            Assert.IsTrue((Int64)os["p1"].Payload == 7);

            /*fsr.Step();
            fsr.Step();
            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("b");
            Assert.IsTrue((double)o.Payload == 5);

            fsr.Step();
            fsr.Step();
            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("b1");
            Assert.IsTrue((Int64)o.Payload == 8);*/
        }

        [Test]
        [Category("Debugger")]
        public void TestStepIntoClassConstructor2()
        {
            String code =
@"
    class Point
	{
        x : int;
        y : int;
        z : int;
                                
        constructor Point(a : int, b : int, c : int)
        {
			x = a;
            y = b;
            z = c;
        }
    }
	
	newPoint = Point.Point(1,2,3);
";
            fsr.PreStart(code, runnerConfig);
            fsr.Step();

            fsr.Step();
            fsr.Step();
            fsr.Step();
            fsr.Step();
            DebugRunner.VMState vms = fsr.Step();
            Obj o = vms.mirror.GetDebugValue("x");
            string type = vms.mirror.GetType("x");

            Assert.IsTrue((Int64)o.Payload == 1);
            Assert.IsTrue(type == "int");

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("y");
            type = vms.mirror.GetType("y");

            Assert.IsTrue((Int64)o.Payload == 2);
            Assert.IsTrue(type == "int");

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("z");
            type = vms.mirror.GetType("z");

            Assert.IsTrue((Int64)o.Payload == 3);
            Assert.IsTrue(type == "int");

            fsr.Step();
            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("newPoint");
            Assert.IsTrue(vms.mirror.GetType(o) == "Point");
            Dictionary<string, Obj> os = vms.mirror.GetProperties(o);

            Assert.IsTrue((Int64)os["x"].Payload == 1);
            Assert.IsTrue((Int64)os["y"].Payload == 2);
            Assert.IsTrue((Int64)os["z"].Payload == 3);
        }

        [Test]
        [Category("Debugger")]
        [Category("Failure")]
        public void TestStepIntoClassConstructor3()
        {
            String code =
@"
class MyClass
{
    mx : int;
    my : int = 3;
    
    constructor Create(x : int, y : int)
    {
        mx = x;
        my = y;
    }
}

godzilla = MyClass.Create(8, 9);
";
            fsr.PreStart(code, runnerConfig);

            // First step over should bring exec cursor to "godzilla = MyClass.Create(8, 9)"
            DebugRunner.VMState vms = fsr.StepOver();
            Assert.AreEqual(14, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(14, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(33, vms.ExecutionCursor.EndExclusive.CharNo);

            // Stepping into the constructor should bring cursor to "mx : int;".
            vms = fsr.Step();
            Assert.AreEqual(4, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(4, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(14, vms.ExecutionCursor.EndExclusive.CharNo);

            // Stepping over should bring execution cursor to "my : int = 3;".
            vms = fsr.StepOver();
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(5, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(18, vms.ExecutionCursor.EndExclusive.CharNo);

            // Stepping over should bring execution cursor to "mx = x;".
            vms = fsr.StepOver();
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(9, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(16, vms.ExecutionCursor.EndExclusive.CharNo);

            // Stepping over should bring execution cursor to "my = y;".
            vms = fsr.StepOver();
            Assert.AreEqual(10, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(10, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(16, vms.ExecutionCursor.EndExclusive.CharNo);

            // Stepping over to the closing bracket of constructor "}".
            vms = fsr.StepOver();
            Assert.AreEqual(11, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(11, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(6, vms.ExecutionCursor.EndExclusive.CharNo);

            // Stepping over should bring execution cursor to "godzilla = MyClass.Create(8, 9);".
            vms = fsr.StepOver();
            Assert.AreEqual(14, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(14, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(33, vms.ExecutionCursor.EndExclusive.CharNo);

            // Final step ends the debug session.
            vms = fsr.StepOver();
            Assert.AreEqual(true, vms.isEnded);
        }

        [Test]
        [Category("Debugger")]
        public void TestStepOverFunction()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
@"
def f : int(i : int, j : int)
{
    a = 5;
    return = 7;
}

a = f(1, 2);
b = 2;
c = 3;
", runnerConfig);

            // First step should bring the exec cursor to "a = f(1, 2)".
            DebugRunner.VMState vms = fsr.StepOver();
            Assert.AreEqual(8, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(8, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(13, vms.ExecutionCursor.EndExclusive.CharNo);

            // Second step should bring it to "b = 2;".
            vms = fsr.StepOver();
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(9, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(7, vms.ExecutionCursor.EndExclusive.CharNo);

            // Third step should bring it to "c = 3;".
            vms = fsr.StepOver();
            Assert.AreEqual(10, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(10, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(7, vms.ExecutionCursor.EndExclusive.CharNo);

            // Final step ends the debug session.
            vms = fsr.StepOver();
            Assert.AreEqual(true, vms.isEnded);
        }


        [Test]
        [Category("Debugger")]
        [Category("Failure")]
        public void PropertyAssignFromBuiltInFunction()
        {
            string sourceCode = @"
class Zee
{
    a : int;
    b : int;
    c : int;
}

z = Zee.Zee();

z.a = 10;
z.b = 2 * 1;
";
            fsr.PreStart(sourceCode, runnerConfig);

            // First step over should bring exec cursor to and highlight "z = Zee.Zee()"
            DebugRunner.VMState vms = fsr.StepOver();
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(9, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(15, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver(); // Highlight "z.a = 10;"
            Assert.AreEqual(11, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(11, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(10, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver(); // Highlight "z.b = 2 * 1;"
            Assert.AreEqual(12, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(12, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(13, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver(); // Ends execution...
            Assert.AreEqual(true, vms.isEnded);
        }

        [Test]
        [Category("Debugger")]
        public void PropertyAssignFromUserFunction()
        {
            string sourceCode = @"
def foo()
{
    return = 2;
}

class Zee
{
    a : int;
}

z = Zee.Zee();
z.a = foo();
";
            fsr.PreStart(sourceCode, runnerConfig);

            // First step over should bring exec cursor to and highlight "z = Zee.Zee()"
            DebugRunner.VMState vms = fsr.StepOver();
            Assert.AreEqual(12, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(12, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(15, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver(); // Highlight "z.a = foo()"
            Assert.AreEqual(13, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(13, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(13, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver(); // Ends execution...
            Assert.AreEqual(true, vms.isEnded);
        }

        [Test]
        [Category("Debugger")]
        public void TestStepAtPropertyInImperativeBlock()
        {
            String code =
@"
class Point
{
    x : int;
    
    constructor Point()
    {
        x = 10;
    }
}

[Imperative]
{
	p = Point.Point();
	p.x = 9;
    b = p.x;
}
";
            fsr.PreStart(code, runnerConfig);
            fsr.Step();

            fsr.Step();

            DebugRunner.VMState vms = fsr.Step();
            Obj o = vms.mirror.GetDebugValue("x");
            string type = vms.mirror.GetType("x");

            Assert.IsTrue((Int64)o.Payload == 0);
            Assert.IsTrue(type == "int");

            fsr.Step();
            fsr.Step();
            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("p");
            type = vms.mirror.GetType("p");

            Assert.IsTrue(type == "Point");
            Dictionary<string, Obj> os = vms.mirror.GetProperties(o);

            Assert.IsTrue((Int64)os["x"].Payload == 10);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("p");
            os = vms.mirror.GetProperties(o);
            Assert.IsTrue((Int64)os["x"].Payload == 9);

            /*o = vms.mirror.GetDebugValue("b");
            type = vms.mirror.GetType("b");

            Assert.IsTrue((Int64)o.Payload == 9);
            Assert.IsTrue(type == "int");*/
        }


        [Test]
        [Category("Debugger")]
        public void TestStepInClassInheritance0()
        {
            String code =
@"class B
{ 
	x3 : int ;
		
	constructor B(a : int) 
	{	
		x3 = a;
	}
}

class A extends B
{ 
	x1 : int ;
	
	constructor A(a1,a3) : base.B(a3)
	{	
		x1 = a1; 		
	}
}
a1 = A.A( 98, 67 );";

            fsr.PreStart(code, runnerConfig);
            fsr.Step();

            // Step into base constructor
            fsr.Step();
            DebugRunner.VMState vms = fsr.Step();
            Obj o = vms.mirror.GetDebugValue("x3");
            Assert.IsTrue((Int64)o.Payload == 0);
            Assert.IsTrue(vms.mirror.GetType("x3") == "int");

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("x3");
            Assert.IsTrue((Int64)o.Payload == 67);
            Assert.IsTrue(vms.mirror.GetType("x3") == "int");

            fsr.Step();
            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("x1");
            Assert.IsTrue((Int64)o.Payload == 0);
            Assert.IsTrue(vms.mirror.GetType("x1") == "int");

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("x1");
            Assert.IsTrue((Int64)o.Payload == 98);

            fsr.Step();
            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a1");
            Assert.IsTrue(vms.mirror.GetType("a1") == "A");

            Dictionary<string, Obj> os = vms.mirror.GetProperties(o);
            Assert.IsTrue((Int64)os["x1"].Payload == 98);
            Assert.IsTrue((Int64)os["x3"].Payload == 67);
        }

        [Test]
        [Category("Debugger")]
        public void TestStepInClassInheritance()
        {
            String code =
@"
class B
{ 
	x3 : int ;
		
	constructor B(a) 
	{	
		x3 = a;
	}
	def foo ( )
	{
		return = x3;
	}
	def foo ( a : int)
	{
		return = x3 + a;
	}
	
	def foo2 ( a : int)
	{
		return = x3 + a;
	}
}

class A extends B
{ 
	x1 : int ;
	x2 : double;
	
	constructor A(a1,a2,a3) : base.B(a3)
	{	
		x1 = a1; 
		x2 = a2;		
	}
	def foo ( )
	{
		return = {x1, x2};
	}
}

a1 = A.A( 1, 1.5, 2 );
b1 = a1.foo();
b2 = a1.foo2(1);
";
            fsr.PreStart(code, runnerConfig);
            fsr.Step();

            // Step into base constructor
            fsr.Step();
            DebugRunner.VMState vms = fsr.Step();
            Obj o = vms.mirror.GetDebugValue("x3");
            Assert.IsTrue((Int64)o.Payload == 0);
            Assert.IsTrue(vms.mirror.GetType("x3") == "int");

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("x3");
            Assert.IsTrue((Int64)o.Payload == 2);
            Assert.IsTrue(vms.mirror.GetType("x3") == "int");

            fsr.Step();
            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("x1");
            Assert.IsTrue((Int64)o.Payload == 0);
            Assert.IsTrue(vms.mirror.GetType("x1") == "int");

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("x2");
            Assert.IsTrue((double)o.Payload == 0.0);
            Assert.IsTrue(vms.mirror.GetType("x2") == "double");

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("x1");
            Assert.IsTrue((Int64)o.Payload == 1);
            Assert.IsTrue(vms.mirror.GetType("x1") == "int");

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("x2");
            Assert.IsTrue((double)o.Payload == 1.5);
            Assert.IsTrue(vms.mirror.GetType("x2") == "double");

            fsr.Step();
            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a1");
            Assert.IsTrue(vms.mirror.GetType("a1") == "A");

            Dictionary<string, Obj> os = vms.mirror.GetProperties(o);
            Assert.IsTrue((Int64)os["x1"].Payload == 1);
            Assert.IsTrue((double)os["x2"].Payload == 1.5);
            Assert.IsTrue((Int64)os["x3"].Payload == 2);


            fsr.Step();
            fsr.Step();
            fsr.Step();
            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("b1");
            Assert.IsTrue(vms.mirror.GetType("b1") == "array");
            List<Obj> ol = vms.mirror.GetArrayElements(o);

            Assert.IsTrue(ol.Count == 2);
            Assert.IsTrue((Int64)ol[0].Payload == 1);
            Assert.IsTrue((double)ol[1].Payload == 1.5);

            // step into foo2()
            fsr.Step();
            o = vms.mirror.GetDebugValue("x3");
            Assert.IsTrue((Int64)o.Payload == 2);
            o = vms.mirror.GetDebugValue("a");
            Assert.IsTrue((Int64)o.Payload == 1);

            fsr.Step();
            fsr.Step();
            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("b2");
            Assert.IsTrue((Int64)o.Payload == 3);
            Assert.IsTrue(vms.mirror.GetType("b2") == "int");
        }

        [Test]
        [Category("Debugger")]
        public void TestStepInOutConstructors()
        {
            String code =
@"class Point_2
{
	z1 : var;
	z2 : var;
	
	constructor Create_2( )
	{
		z1 = 10;
		z2 = 10;
	}
}

class Point_1
{
	x : var;
	y : var[];
	
	constructor Create_1( _x : Point_2, _y : int[])
	{
		x = _x;
		y = _y[];
	}
}

class Complex
{
	
	a : var;
	p : var;
	c : var[]..[];
	
	constructor Create( _a:int, _p : Point_1, _p1 : Point_1 )
	{
		a = _a;
		p = _p;
		c = { 3, {2,1}, _p1 };
	}
}

	p2 = Point_2.Create_2();
	
	p1_1 = Point_1.Create_1( p2, {11,12} );
	
	p1_2 = Point_1.Create_1( p2, {12,13} ); 
	
	test = Complex.Create( 17, p1_1, p1_2 );	
";
            fsr.PreStart(code, runnerConfig);
            fsr.Step();

            fsr.Step();
            DebugRunner.VMState vms = fsr.Step();
            Obj o = vms.mirror.GetDebugValue("z1");

            Assert.IsTrue((Int64)o.Payload == 10);
            Assert.IsTrue(vms.mirror.GetType(o) == "int");

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("z2");

            Assert.IsTrue((Int64)o.Payload == 10);
            Assert.IsTrue(vms.mirror.GetType(o) == "int");

            fsr.Step();
            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("p2");
            Assert.IsTrue(vms.mirror.GetType(o) == "Point_2");
            Dictionary<string, Obj> os = vms.mirror.GetProperties(o);

            Assert.IsTrue((Int64)os["z1"].Payload == 10);
            Assert.IsTrue((Int64)os["z2"].Payload == 10);

            fsr.Step();
            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("x");
            Assert.IsTrue(vms.mirror.GetType(o) == "Point_2");
            os = vms.mirror.GetProperties(o);

            Assert.IsTrue((Int64)os["z1"].Payload == 10);
            Assert.IsTrue((Int64)os["z2"].Payload == 10);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("y");
            List<Obj> ol = vms.mirror.GetArrayElements(o);

            Assert.IsTrue(vms.mirror.GetType(o) == "array");
            Assert.IsTrue(ol.Count == 2);
            Assert.IsTrue((Int64)ol[0].Payload == 11);
            Assert.IsTrue((Int64)ol[1].Payload == 12);

            fsr.Step();
            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("p1_1");
            Assert.IsTrue(vms.mirror.GetType(o) == "Point_1");
            os = vms.mirror.GetProperties(o);

            Obj o1 = os["x"];
            Obj o2 = os["y"];
            Assert.IsTrue(vms.mirror.GetType(o1) == "Point_2");
            Dictionary<string, Obj> os1 = vms.mirror.GetProperties(o1);
            Assert.IsTrue((Int64)os1["z1"].Payload == 10);
            Assert.IsTrue((Int64)os1["z2"].Payload == 10);

            Assert.IsTrue(vms.mirror.GetType(o2) == "array");
            List<Obj> ol2 = vms.mirror.GetArrayElements(o2);
            Assert.IsTrue(ol2.Count == 2);
            Assert.IsTrue((Int64)ol2[0].Payload == 11);
            Assert.IsTrue((Int64)ol2[1].Payload == 12);

            fsr.Step();
            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("x");
            Assert.IsTrue(vms.mirror.GetType(o) == "Point_2");
            os = vms.mirror.GetProperties(o);

            Assert.IsTrue((Int64)os["z1"].Payload == 10);
            Assert.IsTrue((Int64)os["z2"].Payload == 10);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("y");
            ol = vms.mirror.GetArrayElements(o);

            Assert.IsTrue(vms.mirror.GetType(o) == "array");
            Assert.IsTrue(ol.Count == 2);
            Assert.IsTrue((Int64)ol[0].Payload == 12);
            Assert.IsTrue((Int64)ol[1].Payload == 13);

            fsr.Step();
            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("p1_2");
            Assert.IsTrue(vms.mirror.GetType(o) == "Point_1");
            os = vms.mirror.GetProperties(o);

            o1 = os["x"];
            o2 = os["y"];
            Assert.IsTrue(vms.mirror.GetType(o1) == "Point_2");
            os1 = vms.mirror.GetProperties(o1);
            Assert.IsTrue((Int64)os1["z1"].Payload == 10);
            Assert.IsTrue((Int64)os1["z2"].Payload == 10);

            Assert.IsTrue(vms.mirror.GetType(o2) == "array");
            ol2 = vms.mirror.GetArrayElements(o2);
            Assert.IsTrue(ol2.Count == 2);
            Assert.IsTrue((Int64)ol2[0].Payload == 12);
            Assert.IsTrue((Int64)ol2[1].Payload == 13);

            // test = Complex.Create( 17, p1_1, p1_2 );
            fsr.Step();
            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a");
            Assert.IsTrue((Int64)o.Payload == 17);
            Assert.IsTrue(vms.mirror.GetType(o) == "int");

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("p");
            Assert.IsTrue(vms.mirror.GetType(o) == "Point_1");
            os = vms.mirror.GetProperties(o);

            o1 = os["x"];
            o2 = os["y"];
            Assert.IsTrue(vms.mirror.GetType(o1) == "Point_2");
            os1 = vms.mirror.GetProperties(o1);
            Assert.IsTrue((Int64)os1["z1"].Payload == 10);
            Assert.IsTrue((Int64)os1["z2"].Payload == 10);

            Assert.IsTrue(vms.mirror.GetType(o2) == "array");
            ol2 = vms.mirror.GetArrayElements(o2);
            Assert.IsTrue(ol2.Count == 2);
            Assert.IsTrue((Int64)ol2[0].Payload == 11);
            Assert.IsTrue((Int64)ol2[1].Payload == 12);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("c");
            ol = vms.mirror.GetArrayElements(o);

            Assert.IsTrue(vms.mirror.GetType(o) == "array");
            Assert.IsTrue(ol.Count == 3);
            Assert.IsTrue((Int64)ol[0].Payload == 3);

            List<Obj> ol1 = vms.mirror.GetArrayElements(ol[1]);
            Assert.IsTrue(ol1.Count == 2);
            Assert.IsTrue((Int64)ol1[0].Payload == 2);
            Assert.IsTrue((Int64)ol1[1].Payload == 1);

            Assert.IsTrue(vms.mirror.GetType(ol[2]) == "Point_1");
            os = vms.mirror.GetProperties(ol[2]);
            o1 = os["x"];
            o2 = os["y"];
            Assert.IsTrue(vms.mirror.GetType(o1) == "Point_2");
            os1 = vms.mirror.GetProperties(o1);
            Assert.IsTrue((Int64)os1["z1"].Payload == 10);
            Assert.IsTrue((Int64)os1["z2"].Payload == 10);

            Assert.IsTrue(vms.mirror.GetType(o2) == "array");
            ol2 = vms.mirror.GetArrayElements(o2);
            Assert.IsTrue(ol2.Count == 2);
            Assert.IsTrue((Int64)ol2[0].Payload == 12);
            Assert.IsTrue((Int64)ol2[1].Payload == 13);
        }

        [Test]
        [Category("Debugger")]
        public void TestStepInOutConstructorsFromLangBlock()
        {
            String code =
@"class Point_2
{
	z1 : var;
	z2 : var;
	
	constructor Create_2( )
	{
		z1 = 10;
		z2 = 10;
	}
}

class Point_1
{
	x : var;
	y : var[];
	
	constructor Create_1( _x : Point_2, _y : int[])
	{
		x = _x;
		y = _y;
	}
}

class Complex
{
	
	a : var;
	p : var;
	c : var[]..[];
	
	constructor Create( _a:int, _p : Point_1, _p1 : Point_1 )
	{
		a = _a;
		p = _p;
		c = { 3, {2,1}, _p1 };
	}
}

[Imperative]
{
	p2 = Point_2.Create_2();
	
	p1_1 = Point_1.Create_1( p2, {11,12} );
	
	p1_2 = Point_1.Create_1( p2, {12,13} ); 
	
	test = Complex.Create( 17, p1_1, p1_2 );	
}
";
            fsr.PreStart(code, runnerConfig);
            fsr.Step();

            fsr.Step();
            DebugRunner.VMState vms = fsr.Step();
            Obj o = vms.mirror.GetDebugValue("z1");

            Assert.IsTrue((Int64)o.Payload == 10);
            Assert.IsTrue(vms.mirror.GetType(o) == "int");

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("z2");

            Assert.IsTrue((Int64)o.Payload == 10);
            Assert.IsTrue(vms.mirror.GetType(o) == "int");

            fsr.Step();
            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("p2");
            Assert.IsTrue(vms.mirror.GetType(o) == "Point_2");
            Dictionary<string, Obj> os = vms.mirror.GetProperties(o);

            Assert.IsTrue((Int64)os["z1"].Payload == 10);
            Assert.IsTrue((Int64)os["z2"].Payload == 10);

            fsr.Step();
            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("x");
            Assert.IsTrue(vms.mirror.GetType(o) == "Point_2");
            os = vms.mirror.GetProperties(o);

            Assert.IsTrue((Int64)os["z1"].Payload == 10);
            Assert.IsTrue((Int64)os["z2"].Payload == 10);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("y");
            List<Obj> ol = vms.mirror.GetArrayElements(o);

            Assert.IsTrue(vms.mirror.GetType(o) == "array");
            Assert.IsTrue(ol.Count == 2);
            Assert.IsTrue((Int64)ol[0].Payload == 11);
            Assert.IsTrue((Int64)ol[1].Payload == 12);

            fsr.Step();
            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("p1_1");
            Assert.IsTrue(vms.mirror.GetType(o) == "Point_1");
            os = vms.mirror.GetProperties(o);

            Obj o1 = os["x"];
            Obj o2 = os["y"];
            Assert.IsTrue(vms.mirror.GetType(o1) == "Point_2");
            Dictionary<string, Obj> os1 = vms.mirror.GetProperties(o1);
            Assert.IsTrue((Int64)os1["z1"].Payload == 10);
            Assert.IsTrue((Int64)os1["z2"].Payload == 10);

            Assert.IsTrue(vms.mirror.GetType(o2) == "array");
            List<Obj> ol2 = vms.mirror.GetArrayElements(o2);
            Assert.IsTrue(ol2.Count == 2);
            Assert.IsTrue((Int64)ol2[0].Payload == 11);
            Assert.IsTrue((Int64)ol2[1].Payload == 12);

            fsr.Step();
            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("x");
            Assert.IsTrue(vms.mirror.GetType(o) == "Point_2");
            os = vms.mirror.GetProperties(o);

            Assert.IsTrue((Int64)os["z1"].Payload == 10);
            Assert.IsTrue((Int64)os["z2"].Payload == 10);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("y");
            ol = vms.mirror.GetArrayElements(o);

            Assert.IsTrue(vms.mirror.GetType(o) == "array");
            Assert.IsTrue(ol.Count == 2);
            Assert.IsTrue((Int64)ol[0].Payload == 12);
            Assert.IsTrue((Int64)ol[1].Payload == 13);

            fsr.Step();
            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("p1_2");
            Assert.IsTrue(vms.mirror.GetType(o) == "Point_1");
            os = vms.mirror.GetProperties(o);

            o1 = os["x"];
            o2 = os["y"];
            Assert.IsTrue(vms.mirror.GetType(o1) == "Point_2");
            os1 = vms.mirror.GetProperties(o1);
            Assert.IsTrue((Int64)os1["z1"].Payload == 10);
            Assert.IsTrue((Int64)os1["z2"].Payload == 10);

            Assert.IsTrue(vms.mirror.GetType(o2) == "array");
            ol2 = vms.mirror.GetArrayElements(o2);
            Assert.IsTrue(ol2.Count == 2);
            Assert.IsTrue((Int64)ol2[0].Payload == 12);
            Assert.IsTrue((Int64)ol2[1].Payload == 13);

            // test = Complex.Create( 17, p1_1, p1_2 );
            fsr.Step();
            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a");
            Assert.IsTrue((Int64)o.Payload == 17);
            Assert.IsTrue(vms.mirror.GetType(o) == "int");

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("p");
            Assert.IsTrue(vms.mirror.GetType(o) == "Point_1");
            os = vms.mirror.GetProperties(o);

            o1 = os["x"];
            o2 = os["y"];
            Assert.IsTrue(vms.mirror.GetType(o1) == "Point_2");
            os1 = vms.mirror.GetProperties(o1);
            Assert.IsTrue((Int64)os1["z1"].Payload == 10);
            Assert.IsTrue((Int64)os1["z2"].Payload == 10);

            Assert.IsTrue(vms.mirror.GetType(o2) == "array");
            ol2 = vms.mirror.GetArrayElements(o2);
            Assert.IsTrue(ol2.Count == 2);
            Assert.IsTrue((Int64)ol2[0].Payload == 11);
            Assert.IsTrue((Int64)ol2[1].Payload == 12);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("c");
            ol = vms.mirror.GetArrayElements(o);

            Assert.IsTrue(vms.mirror.GetType(o) == "array");
            Assert.IsTrue(ol.Count == 3);
            Assert.IsTrue((Int64)ol[0].Payload == 3);

            List<Obj> ol1 = vms.mirror.GetArrayElements(ol[1]);
            Assert.IsTrue(ol1.Count == 2);
            Assert.IsTrue((Int64)ol1[0].Payload == 2);
            Assert.IsTrue((Int64)ol1[1].Payload == 1);

            Assert.IsTrue(vms.mirror.GetType(ol[2]) == "Point_1");
            os = vms.mirror.GetProperties(ol[2]);
            o1 = os["x"];
            o2 = os["y"];
            Assert.IsTrue(vms.mirror.GetType(o1) == "Point_2");
            os1 = vms.mirror.GetProperties(o1);
            Assert.IsTrue((Int64)os1["z1"].Payload == 10);
            Assert.IsTrue((Int64)os1["z2"].Payload == 10);

            Assert.IsTrue(vms.mirror.GetType(o2) == "array");
            ol2 = vms.mirror.GetArrayElements(o2);
            Assert.IsTrue(ol2.Count == 2);
            Assert.IsTrue((Int64)ol2[0].Payload == 12);
            Assert.IsTrue((Int64)ol2[1].Payload == 13);
        }

        [Test]
        [Category("Debugger")]
        public void TestStepInAssocLangBlock()
        {
            String code =
@"
[Associative]
{
	a = 4;
	b = 10;
	c = a + b;
    d = c;        
}
";
            fsr.PreStart(code, runnerConfig);
            fsr.Step();
            DebugRunner.VMState vms = fsr.Step();
            Obj o = vms.mirror.GetDebugValue("a");
            string type = vms.mirror.GetType("a");

            Assert.IsTrue((Int64)o.Payload == 4);
            Assert.IsTrue(type == "int");

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("b");
            type = vms.mirror.GetType("b");

            Assert.IsTrue((Int64)o.Payload == 10);
            Assert.IsTrue(type == "int");

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("c");
            type = vms.mirror.GetType("c");

            Assert.IsTrue((Int64)o.Payload == 14);
            Assert.IsTrue(type == "int");
        }

        [Test]
        [Category("Debugger")]
        public void TestStepWithUpdate()
        {
            String code =
@"

	a = 4;
	b = a;
	a = 3;        
";
            fsr.PreStart(code, runnerConfig);
            fsr.Step();
            DebugRunner.VMState vms = fsr.Step();
            Obj o = vms.mirror.GetDebugValue("a");
            string type = vms.mirror.GetType("a");

            Assert.IsTrue((Int64)o.Payload == 4);
            Assert.IsTrue(type == "int");

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("b");
            type = vms.mirror.GetType("b");

            Assert.IsTrue((Int64)o.Payload == 4);
            Assert.IsTrue(type == "int");

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a");
            type = vms.mirror.GetType("a");

            Assert.IsTrue((Int64)o.Payload == 3);
            Assert.IsTrue(type == "int");

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("b");
            type = vms.mirror.GetType("b");

            Assert.IsTrue((Int64)o.Payload == 3);
            Assert.IsTrue(type == "int");
        }

        [Test]
        [Category("Debugger")]
        public void TestStepWithUpdate2()
        {
            String code =
@"
x = 1;
a = x + 1;

c = 0;
b = 2;
t = a+c;
    
c = a + b;    
x = 3;";
            //Assert.Fail("DNL-1467484 wrong execution sequence for update");

            fsr.PreStart(code, runnerConfig);
            fsr.Step();

            DebugRunner.VMState vms = fsr.Step();
            Obj o = vms.mirror.GetDebugValue("x");
            Assert.IsTrue((Int64)o.Payload == 1);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a");
            Assert.IsTrue((Int64)o.Payload == 2);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("c");
            Assert.IsTrue((Int64)o.Payload == 0);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("b");
            Assert.IsTrue((Int64)o.Payload == 2);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("t");
            Assert.IsTrue((Int64)o.Payload == 2);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("c");
            Assert.IsTrue((Int64)o.Payload == 4);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("t");
            Assert.IsTrue((Int64)o.Payload == 6);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("x");
            Assert.IsTrue((Int64)o.Payload == 3);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a");
            Assert.IsTrue((Int64)o.Payload == 4);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("t");
            Assert.IsTrue((Int64)o.Payload == 8);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("c");
            Assert.IsTrue((Int64)o.Payload == 6);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("t");
            Assert.IsTrue((Int64)o.Payload == 10);
        }

        [Test]
        [Category("Debugger")]
        public void TestStepWithUpdateUsingFunctions()
        {
            String code =
@"
def fadd : int(a : int)
{
    return = a+1;
}

x = 1;

a = fadd(x);

b = 11;
c = a + b;

x = 10;      
";
            //Assert.Fail("IDE-312: Debugger Regression: Debugger fails with update");
            fsr.PreStart(code, runnerConfig);
            fsr.Step();

            DebugRunner.VMState vms = fsr.Step();
            Obj o = vms.mirror.GetDebugValue("x");
            String type = vms.mirror.GetType("x");

            Assert.IsTrue((Int64)o.Payload == 1);
            Assert.IsTrue(type == "int");

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a");
            type = vms.mirror.GetType("a");

            Assert.IsTrue((Int64)o.Payload == 1);
            Assert.IsTrue(type == "int");

            fsr.Step();
            fsr.Step();

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a");
            type = vms.mirror.GetType("a");

            Assert.IsTrue((Int64)o.Payload == 2);
            Assert.IsTrue(type == "int");

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("b");
            type = vms.mirror.GetType("b");

            Assert.IsTrue((Int64)o.Payload == 11);
            Assert.IsTrue(type == "int");

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("c");
            type = vms.mirror.GetType("c");

            Assert.IsTrue((Int64)o.Payload == 13);
            Assert.IsTrue(type == "int");

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("x");
            type = vms.mirror.GetType("x");

            Assert.IsTrue((Int64)o.Payload == 10);
            Assert.IsTrue(type == "int");

            fsr.Step();
            fsr.Step();
            fsr.Step();

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a");
            type = vms.mirror.GetType("a");

            Assert.IsTrue((Int64)o.Payload == 11);
            Assert.IsTrue(type == "int");

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("c");
            type = vms.mirror.GetType("c");

            Assert.IsTrue((Int64)o.Payload == 22);
            Assert.IsTrue(type == "int");
        }

        [Test]
        [Category("Debugger")]
        public void TestStepWithUpdateUsingImperativeLangBlock()
        {
            String code =
@"
x = 1;
a = x + 1;

c = 0;
b = 2;
t = a+c;
    
[Imperative]
{
    c = a + b;    
    x = 3;
}     
";
            //Assert.Fail("IDE-333 Debugger fails with update using Imperative Language block");

            fsr.PreStart(code, runnerConfig);
            fsr.Step();

            DebugRunner.VMState vms = fsr.Step();
            Obj o = vms.mirror.GetDebugValue("x");
            String type = vms.mirror.GetType("x");

            Assert.IsTrue((Int64)o.Payload == 1);
            Assert.IsTrue(type == "int");

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a");
            Assert.IsTrue((Int64)o.Payload == 2);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("c");
            Assert.IsTrue((Int64)o.Payload == 0);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("b");
            Assert.IsTrue((Int64)o.Payload == 2);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("t");
            Assert.IsTrue((Int64)o.Payload == 2);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("c");
            Assert.IsTrue((Int64)o.Payload == 4);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("x");
            Assert.IsTrue((Int64)o.Payload == 3);

            fsr.Step();
            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a");
            Assert.IsTrue((Int64)o.Payload == 4);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("t");
            Assert.IsTrue((Int64)o.Payload == 8);
        }

        [Test]
        [Category("Debugger")]
        public void TestStepWithUpdateUsingImperativeLangBlock2()
        {
            String code =
@"
y = 0;
t = y * 2;
[Imperative]
{
    c = 0;
	[Associative]
	{
		def fadd : int(a : int)
		{
		    return = a+1;
		}
		
		x = 1;
		
		a = fadd(x);
		
		b = 11;
		c = a + b;
	
	    x = 10;
	}
    y = c + 2;
}";
            //Assert.Fail("IDE-333 Debugger fails with update using Imperative Language block");

            fsr.PreStart(code, runnerConfig);
            fsr.Step();

            DebugRunner.VMState vms = fsr.Step();
            Obj o = vms.mirror.GetDebugValue("y");
            Assert.IsTrue((Int64)o.Payload == 0);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("t");
            Assert.IsTrue((Int64)o.Payload == 0);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("c");
            Assert.IsTrue((Int64)o.Payload == 0);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("x");
            Assert.IsTrue((Int64)o.Payload == 1);

            fsr.StepOver();
            //vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a");
            Assert.IsTrue((Int64)o.Payload == 2);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("b");
            Assert.IsTrue((Int64)o.Payload == 11);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("c");
            Assert.IsTrue((Int64)o.Payload == 13);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("x");
            Assert.IsTrue((Int64)o.Payload == 10);

            fsr.StepOver();
            //vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a");
            Assert.IsTrue((Int64)o.Payload == 11);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("c");
            Assert.IsTrue((Int64)o.Payload == 22);

            fsr.Step();
            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("y");
            Assert.IsTrue((Int64)o.Payload == 24);

            fsr.Step();
            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("t");
            Assert.IsTrue((Int64)o.Payload == 48);
        }

        [Test]
        [Category("Debugger")]
        public void TestStepWithUpdateUsingAssociativeLangBlock()
        {
            String code =
@"
x = 1;
a = x + 1;

c = 0;
b = 2;
t = a+c;
    
[Associative]
{
    c = a + b;    
    x = 3;
}     
";
            //Assert.Fail("IDE-367 Debugger might fail with update using Assoc Language block in global scope");

            fsr.PreStart(code, runnerConfig);
            fsr.Step();

            DebugRunner.VMState vms = fsr.Step();
            Obj o = vms.mirror.GetDebugValue("x");
            Assert.IsTrue((Int64)o.Payload == 1);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a");
            Assert.IsTrue((Int64)o.Payload == 2);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("c");
            Assert.IsTrue((Int64)o.Payload == 0);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("b");
            Assert.IsTrue((Int64)o.Payload == 2);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("t");
            Assert.IsTrue((Int64)o.Payload == 2);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("c");
            Assert.IsTrue((Int64)o.Payload == 4);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("x");
            Assert.IsTrue((Int64)o.Payload == 3);

            fsr.Step();
            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a");
            Assert.IsTrue((Int64)o.Payload == 4);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("t");
            Assert.IsTrue((Int64)o.Payload == 8);
        }

        [Test]
        [Category("Debugger")]
        public void TestStepWithUpdateUsingAssociativeLangBlock2()
        {
            String code =
@"
y = 0;
t = y * 2;
[Associative]
{
    c = 0;
	[Imperative]
	{
		def fadd : int(a : int)
		{
		    return = a+1;
		}
		
		x = 1;
		
		a = fadd(x);
		
		b = 11;
		c = a + b;
	
	    x = 10;
	}
    y = c + 2;
}
";
            fsr.PreStart(code, runnerConfig);
            fsr.Step();

            DebugRunner.VMState vms = fsr.Step();
            Obj o = vms.mirror.GetDebugValue("y");
            Assert.IsTrue((Int64)o.Payload == 0);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("t");
            Assert.IsTrue((Int64)o.Payload == 0);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("c");
            Assert.IsTrue((Int64)o.Payload == 0);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("x");
            Assert.IsTrue((Int64)o.Payload == 1);

            fsr.StepOver();
            //vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a");
            Assert.IsTrue((Int64)o.Payload == 2);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("b");
            Assert.IsTrue((Int64)o.Payload == 11);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("c");
            Assert.IsTrue((Int64)o.Payload == 13);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("x");
            Assert.IsTrue((Int64)o.Payload == 10);

            fsr.Step();
            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("y");
            Assert.IsTrue((Int64)o.Payload == 15);

            fsr.Step();
            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("t");
            Assert.IsTrue((Int64)o.Payload == 30);
        }

        [Test]
        [Category("Debugger")]
        public void TestStepWithImperativeReturnStatements()
        {
            string source = @"
[Imperative]
{
    def HalveIt : double(value : int)
    {
        if (value < 0)
            return = 0.0;

        return = value * 0.5;
    }

    positive = HalveIt(250);
    negative = HalveIt(-250);
}
";
            fsr.PreStart(source, runnerConfig);

            DebugRunner.VMState vms = fsr.StepOver(); // "positive = HalveIt(250)"
            Assert.AreEqual(12, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(12, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(29, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step(); // "if (value < 0)"
            Assert.AreEqual(6, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(6, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(23, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver(); // "return = value * 0.5;"
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(9, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(30, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver(); // "}" of function "HalveIt"
            Assert.AreEqual(10, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(10, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(6, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver(); // "positive = HalveIt(250);"
            Assert.AreEqual(12, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(12, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(29, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver(); // "negative = HalveIt(-250)"
            Assert.AreEqual(13, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(13, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(30, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step(); // "if (value < 0)"
            Assert.AreEqual(6, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(6, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(23, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver(); // "return = 0.0;"
            Assert.AreEqual(7, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(13, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(7, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(26, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver(); // "}" of function "HalveIt"
            Assert.AreEqual(10, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(10, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(6, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver(); // "negative = HalveIt(-250);"
            Assert.AreEqual(13, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(13, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(30, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver(); // "}" of the "Imperative" block
            Assert.AreEqual(14, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(14, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver(); // Script execution ended.
            Assert.AreEqual(true, vms.isEnded);
        }

        [Test]
        [Category("Debugger")]
        public void TestStepWithAssociativeReturnStatements()
        {
            string source = @"
def MakeItDouble : double(value : int)
{
    return = value * 1.0;
}

d = MakeItDouble(250);
";
            fsr.PreStart(source, runnerConfig);

            DebugRunner.VMState vms = fsr.StepOver(); // "d = MakeItDouble(250)"
            Assert.AreEqual(7, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(7, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(23, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step(); // "return = value * 1.0;"
            Assert.AreEqual(4, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(4, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(26, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver(); // "}" of function "MakeItDouble"
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(5, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver(); // "d = MakeItDouble(250);"
            Assert.AreEqual(7, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(7, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(23, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver(); // Script execution ended.
            Assert.AreEqual(true, vms.isEnded);
        }

        [Test]
        [Category("Debugger")]
        public void TestStepForLoopWithRangeExpression()
        {
            string sourceCode = @"
[Imperative]
{
    x = 0;
    for(index in 1..2) {
        x = x + 1;
    }
}
";

            fsr.PreStart(sourceCode, runnerConfig);
            DebugRunner.VMState vms = fsr.StepOver(); // Highlight "x = 0;"
            Assert.AreEqual("4, 5, 4, 11", CodeRangeToString(vms.ExecutionCursor));
            vms = fsr.StepOver(); // Highlight "for"
            Assert.AreEqual("5, 5, 5, 8", CodeRangeToString(vms.ExecutionCursor));
            vms = fsr.StepOver(); // Highlight "1..2"
            Assert.AreEqual("5, 18, 5, 22", CodeRangeToString(vms.ExecutionCursor));
            vms = fsr.StepOver(); // Highlight "in"
            Assert.AreEqual("5, 15, 5, 17", CodeRangeToString(vms.ExecutionCursor));
            vms = fsr.StepOver(); // Highlight "index"
            Assert.AreEqual("5, 9, 5, 14", CodeRangeToString(vms.ExecutionCursor));
            vms = fsr.StepOver(); // Highlight "x = x + 1;"
            Assert.AreEqual("6, 9, 6, 19", CodeRangeToString(vms.ExecutionCursor));
            vms = fsr.StepOver();
            vms = fsr.StepOver(); // Highlight "in"
            Assert.AreEqual("5, 15, 5, 17", CodeRangeToString(vms.ExecutionCursor));
            vms = fsr.StepOver(); // Highlight "index"
            Assert.AreEqual("5, 9, 5, 14", CodeRangeToString(vms.ExecutionCursor));
            vms = fsr.StepOver(); // Highlight "x = x + 1;"
            Assert.AreEqual("6, 9, 6, 19", CodeRangeToString(vms.ExecutionCursor));
            vms = fsr.StepOver();
            vms = fsr.StepOver(); // Highlight "in"
            Assert.AreEqual("5, 15, 5, 17", CodeRangeToString(vms.ExecutionCursor));
            vms = fsr.StepOver(); // Highlight "}" of closing Imperative block.
            Assert.AreEqual("8, 1, 8, 2", CodeRangeToString(vms.ExecutionCursor));

            vms = fsr.StepOver(); // Execution ended.
            Assert.AreEqual(true, vms.isEnded);
        }

        [Test]
        [Category("Debugger")]
        public void TestStepForLoopWithFunctionCall()
        {
            string sourceCode = @"
def foo : int[]() {
    return = 1..2;
}

[Imperative]
{
    x = 0;
    for(index in foo()) {
        x = x + 1;
    }
}
";

            fsr.PreStart(sourceCode, runnerConfig);
            DebugRunner.VMState vms = fsr.StepOver(); // Highlight "x = 0;"
            Assert.AreEqual("8, 5, 8, 11", CodeRangeToString(vms.ExecutionCursor));
            vms = fsr.StepOver(); // Highlight "for"
            Assert.AreEqual("9, 5, 9, 8", CodeRangeToString(vms.ExecutionCursor));
            vms = fsr.StepOver(); // Highlight "foo()"
            Assert.AreEqual("9, 18, 9, 23", CodeRangeToString(vms.ExecutionCursor));
            vms = fsr.StepOver(); // Highlight "in"
            Assert.AreEqual("9, 15, 9, 17", CodeRangeToString(vms.ExecutionCursor));
            vms = fsr.StepOver(); // Highlight "index"
            Assert.AreEqual("9, 9, 9, 14", CodeRangeToString(vms.ExecutionCursor));
            vms = fsr.StepOver(); // Highlight "x = x + 1;"
            Assert.AreEqual("10, 9, 10, 19", CodeRangeToString(vms.ExecutionCursor));
            vms = fsr.StepOver(); // Highlight "}"
            vms = fsr.StepOver(); // Highlight "in"
            Assert.AreEqual("9, 15, 9, 17", CodeRangeToString(vms.ExecutionCursor));
            vms = fsr.StepOver(); // Highlight "index"
            Assert.AreEqual("9, 9, 9, 14", CodeRangeToString(vms.ExecutionCursor));
            vms = fsr.StepOver(); // Highlight "x = x + 1;"
            Assert.AreEqual("10, 9, 10, 19", CodeRangeToString(vms.ExecutionCursor));
            vms = fsr.StepOver(); // Highlight "}"
            vms = fsr.StepOver(); // Highlight "in"
            Assert.AreEqual("9, 15, 9, 17", CodeRangeToString(vms.ExecutionCursor));
            vms = fsr.StepOver(); // Highlight "}" of closing Imperative block.
            Assert.AreEqual("12, 1, 12, 2", CodeRangeToString(vms.ExecutionCursor));

            vms = fsr.StepOver(); // Execution ended.
            Assert.AreEqual(true, vms.isEnded);
        }

        [Test]
        [Category("Debugger")]
        public void TestStepForLoopWithIdentifier()
        {
            string sourceCode = @"
[Imperative]
{
    x = 0;
    boo = { 1, 2 };
    for(index in boo) {
        x = x + 1;
    }
}
";

            fsr.PreStart(sourceCode, runnerConfig);
            DebugRunner.VMState vms = fsr.StepOver(); // Highlight "x = 0;"
            Assert.AreEqual("4, 5, 4, 11", CodeRangeToString(vms.ExecutionCursor));
            vms = fsr.StepOver(); // Highlight "boo = { 1, 2 };"
            Assert.AreEqual("5, 5, 5, 20", CodeRangeToString(vms.ExecutionCursor));
            vms = fsr.StepOver(); // Highlight "for"
            Assert.AreEqual("6, 5, 6, 8", CodeRangeToString(vms.ExecutionCursor));
            vms = fsr.StepOver(); // Highlight "boo"
            Assert.AreEqual("6, 18, 6, 21", CodeRangeToString(vms.ExecutionCursor));
            vms = fsr.StepOver(); // Highlight "in"
            Assert.AreEqual("6, 15, 6, 17", CodeRangeToString(vms.ExecutionCursor));
            vms = fsr.StepOver(); // Highlight "index"
            Assert.AreEqual("6, 9, 6, 14", CodeRangeToString(vms.ExecutionCursor));
            vms = fsr.StepOver(); // Highlight "x = x + 1;"
            Assert.AreEqual("7, 9, 7, 19", CodeRangeToString(vms.ExecutionCursor));
            vms = fsr.StepOver(); // Highlight "}"
            vms = fsr.StepOver(); // Highlight "in"
            Assert.AreEqual("6, 15, 6, 17", CodeRangeToString(vms.ExecutionCursor));
            vms = fsr.StepOver(); // Highlight "index"
            Assert.AreEqual("6, 9, 6, 14", CodeRangeToString(vms.ExecutionCursor));
            vms = fsr.StepOver(); // Highlight "x = x + 1;"
            Assert.AreEqual("7, 9, 7, 19", CodeRangeToString(vms.ExecutionCursor));
            vms = fsr.StepOver(); // Highlight "}"
            vms = fsr.StepOver(); // Highlight "in"
            Assert.AreEqual("6, 15, 6, 17", CodeRangeToString(vms.ExecutionCursor));
            vms = fsr.StepOver(); // Highlight "}" of closing Imperative block.
            Assert.AreEqual("9, 1, 9, 2", CodeRangeToString(vms.ExecutionCursor));

            vms = fsr.StepOver(); // Execution ended.
            Assert.AreEqual(true, vms.isEnded);
        }

        [Test]
        [Category("Debugger")]
        public void TestStepForLoopWithExpressionList()
        {
            string sourceCode = @"
[Imperative]
{
    x = 0;
    for(index in { 1, 2 }) {
        x = x + 1;
    }
}
";

            fsr.PreStart(sourceCode, runnerConfig);
            DebugRunner.VMState vms = fsr.StepOver(); // Highlight "x = 0;"
            Assert.AreEqual("4, 5, 4, 11", CodeRangeToString(vms.ExecutionCursor));
            vms = fsr.StepOver(); // Highlight "for"
            Assert.AreEqual("5, 5, 5, 8", CodeRangeToString(vms.ExecutionCursor));
            vms = fsr.StepOver(); // Highlight "{ 1, 2 }"
            Assert.AreEqual("5, 18, 5, 26", CodeRangeToString(vms.ExecutionCursor));
            vms = fsr.StepOver(); // Highlight "in"
            Assert.AreEqual("5, 15, 5, 17", CodeRangeToString(vms.ExecutionCursor));
            vms = fsr.StepOver(); // Highlight "index"
            Assert.AreEqual("5, 9, 5, 14", CodeRangeToString(vms.ExecutionCursor));
            vms = fsr.StepOver(); // Highlight "x = x + 1;"
            Assert.AreEqual("6, 9, 6, 19", CodeRangeToString(vms.ExecutionCursor));
            vms = fsr.StepOver(); // Highlight "}"
            vms = fsr.StepOver(); // Highlight "in"
            Assert.AreEqual("5, 15, 5, 17", CodeRangeToString(vms.ExecutionCursor));
            vms = fsr.StepOver(); // Highlight "index"
            Assert.AreEqual("5, 9, 5, 14", CodeRangeToString(vms.ExecutionCursor));
            vms = fsr.StepOver(); // Highlight "x = x + 1;"
            Assert.AreEqual("6, 9, 6, 19", CodeRangeToString(vms.ExecutionCursor));
            vms = fsr.StepOver(); // Highlight "}"
            vms = fsr.StepOver(); // Highlight "in"
            Assert.AreEqual("5, 15, 5, 17", CodeRangeToString(vms.ExecutionCursor));
            vms = fsr.StepOver(); // Highlight "}" of closing Imperative block.
            Assert.AreEqual("8, 1, 8, 2", CodeRangeToString(vms.ExecutionCursor));

            vms = fsr.StepOver(); // Execution ended.
            Assert.AreEqual(true, vms.isEnded);
        }

        private string CodeRangeToString(ProtoCore.CodeModel.CodeRange range)
        {
            return string.Format("{0}, {1}, {2}, {3}",
                range.StartInclusive.LineNo,
                range.StartInclusive.CharNo,
                range.EndExclusive.LineNo,
                range.EndExclusive.CharNo);
        }

        [Test]
        [Category("Debugger")]
        public void TestStepInAssocLangBlockWithUpdate()
        {
            String code =
@"
[Associative]
{
    a = 2;
    b = 2 + a;
    a = 4;
    c = 7;
}

";
            fsr.PreStart(code, runnerConfig);
            fsr.Step();

            DebugRunner.VMState vms = fsr.Step();
            Obj o = vms.mirror.GetDebugValue("a");
            string type = vms.mirror.GetType("a");

            Assert.IsTrue((Int64)o.Payload == 2);
            Assert.IsTrue(type == "int");

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("b");
            type = vms.mirror.GetType("b");

            Assert.IsTrue((Int64)o.Payload == 4);
            Assert.IsTrue(type == "int");

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a");
            type = vms.mirror.GetType("a");

            Assert.IsTrue((Int64)o.Payload == 4);
            Assert.IsTrue(type == "int");

            int startLineNo, startCharNo;

            startLineNo = vms.ExecutionCursor.StartInclusive.LineNo;
            startCharNo = vms.ExecutionCursor.StartInclusive.CharNo;

            Assert.IsTrue((Int64)startLineNo == 5);
            Assert.IsTrue((Int64)startCharNo == 5);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("b");
            type = vms.mirror.GetType("b");

            Assert.IsTrue((Int64)o.Payload == 6);
            Assert.IsTrue(type == "int");

            startLineNo = vms.ExecutionCursor.StartInclusive.LineNo;
            startCharNo = vms.ExecutionCursor.StartInclusive.CharNo;

            Assert.IsTrue((Int64)startLineNo == 7);
            Assert.IsTrue((Int64)startCharNo == 5);
        }

        [Test]
        [Category("Debugger")]
        public void TestStepInAssocLangBlockWithUpdate2()
        {
            String code =
@"
[Associative]
{
    a = 2;
    b = 2 * a;
    a = 4;
	c = 7;
}

";
            fsr.PreStart(code, runnerConfig);
            fsr.Step();
            DebugRunner.VMState vms = fsr.Step();
            Obj o = vms.mirror.GetDebugValue("a");
            string type = vms.mirror.GetType("a");

            Assert.IsTrue((Int64)o.Payload == 2);
            Assert.IsTrue(type == "int");

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("b");
            type = vms.mirror.GetType("b");

            Assert.IsTrue((Int64)o.Payload == 4);
            Assert.IsTrue(type == "int");

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a");
            type = vms.mirror.GetType("a");

            Assert.IsTrue((Int64)o.Payload == 4);
            Assert.IsTrue(type == "int");

            int startLineNo, startCharNo;

            startLineNo = vms.ExecutionCursor.StartInclusive.LineNo;
            startCharNo = vms.ExecutionCursor.StartInclusive.CharNo;

            Assert.IsTrue((Int64)startLineNo == 5);
            Assert.IsTrue((Int64)startCharNo == 5);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("b");
            type = vms.mirror.GetType("b");

            Assert.IsTrue((Int64)o.Payload == 8);
            Assert.IsTrue(type == "int");

            startLineNo = vms.ExecutionCursor.StartInclusive.LineNo;
            startCharNo = vms.ExecutionCursor.StartInclusive.CharNo;

            Assert.IsTrue((Int64)startLineNo == 7);
            Assert.IsTrue((Int64)startCharNo == 2);
        }

        [Test]
        [Category("Debugger")]
        public void TestAddIntToDouble()
        {
            String code =
            @"
[Imperative]
{
	 def add:double( n1:int, n2:double )
	 {
		  
		  return = n1 + n2;

	 }

	 test = add(2,2.5);
	 test1 = add(2,1);
}";

            fsr.PreStart(code, runnerConfig);
            fsr.Step();

            DebugRunner.VMState vms = fsr.Step();
            Obj o = vms.mirror.GetDebugValue("n1");
            string type = vms.mirror.GetType("n1");

            Assert.IsTrue((Int64)o.Payload == 2);
            Assert.IsTrue(type == "int");

            o = vms.mirror.GetDebugValue("n2");
            type = vms.mirror.GetType("n2");

            Assert.IsTrue((Double)o.Payload == 2.5);
            Assert.IsTrue(type == "double");

            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("test");
            type = vms.mirror.GetType("test");

            Assert.IsTrue((Double)o.Payload == 4.5);
            Assert.IsTrue(type == "double");

            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("n1");
            type = vms.mirror.GetType("n1");

            Assert.IsTrue((Int64)o.Payload == 2);
            Assert.IsTrue(type == "int");

            o = vms.mirror.GetDebugValue("n2");
            type = vms.mirror.GetType("n2");

            Assert.IsTrue((Double)o.Payload == 1.0);
            Assert.IsTrue(type == "double");

            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("test1");
            type = vms.mirror.GetType("test1");

            Assert.IsTrue((Double)o.Payload == 3.0);
            Assert.IsTrue(type == "double");

        }

        [Test]
        [Category("Debugger")]
        public void TestStepInImpLangBlock()
        {
            String code =
@"
[Imperative]
{
	a = 4;
	b = 10;
	c = a + b; 
    m = c;       
}
";
            fsr.PreStart(code, runnerConfig);
            fsr.Step();
            DebugRunner.VMState vms = fsr.Step();
            Obj o = vms.mirror.GetDebugValue("a");
            string type = vms.mirror.GetType("a");

            Assert.IsTrue((Int64)o.Payload == 4);
            Assert.IsTrue(type == "int");

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("b");
            type = vms.mirror.GetType("b");

            Assert.IsTrue((Int64)o.Payload == 10);
            Assert.IsTrue(type == "int");

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("c");
            type = vms.mirror.GetType("c");

            Assert.IsTrue((Int64)o.Payload == 14);
            Assert.IsTrue(type == "int");
        }

        [Test]
        [Category("Debugger")]
        public void TestStepInMultiLangBlocks()
        {
            String code =
@"
a = 9;
[Associative]
{
	a = 4;
	b = 10;
	c = a + b;    

	[Imperative]
	{
		a = 5.0;
	}
}
";
            fsr.PreStart(code, runnerConfig);

            DebugRunner.VMState vms = fsr.Step();
            fsr.Step();
            Obj o = vms.mirror.GetDebugValue("a");
            string type = vms.mirror.GetType("a");

            Assert.IsTrue((Int64)o.Payload == 9);
            Assert.IsTrue(type == "int");

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a");
            type = vms.mirror.GetType("a");

            Assert.IsTrue((Int64)o.Payload == 4);
            Assert.IsTrue(type == "int");

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("b");
            type = vms.mirror.GetType("b");

            Assert.IsTrue((Int64)o.Payload == 10);
            Assert.IsTrue(type == "int");

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("c");
            type = vms.mirror.GetType("c");

            Assert.IsTrue((Int64)o.Payload == 14);
            Assert.IsTrue(type == "int");

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a");
            type = vms.mirror.GetType("a");

            Assert.IsTrue((double)o.Payload == 5.0);
            Assert.IsTrue(type == "double");
        }

        [Test]
        [Category("Debugger")]
        public void TestSSAassignments0()
        {
            String code =
@"a = 33;
b = a -2;
a = a + 1;
b = b - 1;
";
            fsr.PreStart(code, runnerConfig);
            fsr.Step();

            DebugRunner.VMState vms = fsr.Step();
            Obj o = vms.mirror.GetDebugValue("a");
            Assert.IsTrue((Int64)o.Payload == 33);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("b");
            Assert.IsTrue((Int64)o.Payload == 31);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a");
            Assert.IsTrue((Int64)o.Payload == 34);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("b");
            Assert.IsTrue((Int64)o.Payload == 32);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("b");
            Assert.IsTrue((Int64)o.Payload == 31);
        }

        [Test]
        [Category("Debugger")]
        public void TestUpdateLoopInsideFunction1()
        {
            String code =
@"
class A
{
        a : int;
    
        def foo()
        {
        
            a = 10;
            c = 2 * a;
            a = 1;
            return = c;
        }
}    
    a = A.A();
    b = a.foo();
";
            fsr.PreStart(code, runnerConfig);
            fsr.Step();

            DebugRunner.VMState vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();

            Obj o = vms.mirror.GetDebugValue("a");
            string type = vms.mirror.GetType("a");

            Dictionary<string, Obj> os = vms.mirror.GetProperties(o);
            Assert.IsTrue(type == "A");
            Assert.IsTrue(os.Count == 1);
            Assert.IsTrue((Int64)os["a"].Payload == 0);

            vms = fsr.Step();
            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("a");
            type = vms.mirror.GetType("a");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 10);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a");
            type = vms.mirror.GetType("a");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 10);
            o = vms.mirror.GetDebugValue("c");
            type = vms.mirror.GetType("c");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 20);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a");
            type = vms.mirror.GetType("a");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 1);
            o = vms.mirror.GetDebugValue("c");
            type = vms.mirror.GetType("c");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 20);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a");
            type = vms.mirror.GetType("a");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 1);
            o = vms.mirror.GetDebugValue("c");
            type = vms.mirror.GetType("c");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 2);

            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("a");
            type = vms.mirror.GetType("a");
            os = vms.mirror.GetProperties(o);

            Assert.IsTrue(type == "A");
            Assert.IsTrue(os.Count == 1);
            Assert.IsTrue((Int64)os["a"].Payload == 1);

            o = vms.mirror.GetDebugValue("b");
            type = vms.mirror.GetType("b");
            Assert.IsTrue((Int64)o.Payload == 2);
            Assert.IsTrue(type == "int");
        }

        [Test]
        [Category("Debugger")]
        [Category("Failure")]
        public void TestUpdateLoopInsideFunction2()
        {
            String code =
@"
class A
{
        a : int;
    
        def foo()
        {
        
            a = 10;
            c = 2 * a;
            [Associative]
            {
                a = 1;
            }
            return = c;
        }
}    
    a = A.A();
    b = a.foo();
";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3963

            fsr.PreStart(code, runnerConfig);
            fsr.Step(); // a = A.A();

            DebugRunner.VMState vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();

            Obj o = vms.mirror.GetDebugValue("a");
            string type = vms.mirror.GetType("a");

            Dictionary<string, Obj> os = vms.mirror.GetProperties(o);
            Assert.IsTrue(type == "A");
            Assert.IsTrue(os.Count == 1);
            Assert.IsTrue((Int64)os["a"].Payload == 0);

            vms = fsr.Step();
            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("a");
            type = vms.mirror.GetType("a");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 10);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a");
            type = vms.mirror.GetType("a");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 10);
            o = vms.mirror.GetDebugValue("c");
            type = vms.mirror.GetType("c");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 20);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a");
            type = vms.mirror.GetType("a");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 1);
            o = vms.mirror.GetDebugValue("c");
            type = vms.mirror.GetType("c");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 20);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a");
            type = vms.mirror.GetType("a");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 1);
            o = vms.mirror.GetDebugValue("c");
            type = vms.mirror.GetType("c");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 2);

            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("a");
            type = vms.mirror.GetType("a");
            os = vms.mirror.GetProperties(o);

            Assert.IsTrue(type == "A");
            Assert.IsTrue(os.Count == 1);
            Assert.IsTrue((Int64)os["a"].Payload == 1);

            o = vms.mirror.GetDebugValue("b");
            type = vms.mirror.GetType("b");
            Assert.IsTrue((Int64)o.Payload == 2);
            Assert.IsTrue(type == "int");
        }

        [Test]
        [Category("Debugger")]
        public void TestUpdateLoopInsideFunction3()
        {
            String code =
@"
class A
{
        a : int;
    
        def foo()
        {
        
            a = 10;
            c = 2 * a;
            [Imperative]
            {
                a = 1;
            }
            return = c;
        }
}    
    a = A.A();
    b = a.foo();
";
            fsr.PreStart(code, runnerConfig);
            fsr.Step();

            DebugRunner.VMState vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();

            Obj o = vms.mirror.GetDebugValue("a");
            string type = vms.mirror.GetType("a");

            Dictionary<string, Obj> os = vms.mirror.GetProperties(o);
            Assert.IsTrue(type == "A");
            Assert.IsTrue(os.Count == 1);
            Assert.IsTrue((Int64)os["a"].Payload == 0);

            vms = fsr.Step();
            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("a");
            type = vms.mirror.GetType("a");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 10);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a");
            type = vms.mirror.GetType("a");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 10);
            o = vms.mirror.GetDebugValue("c");
            type = vms.mirror.GetType("c");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 20);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a");
            type = vms.mirror.GetType("a");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 1);
            o = vms.mirror.GetDebugValue("c");
            type = vms.mirror.GetType("c");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 20);

            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("a");
            type = vms.mirror.GetType("a");
            os = vms.mirror.GetProperties(o);

            Assert.IsTrue(type == "A");
            Assert.IsTrue(os.Count == 1);
            Assert.IsTrue((Int64)os["a"].Payload == 1);

            o = vms.mirror.GetDebugValue("b");
            type = vms.mirror.GetType("b");
            Assert.IsTrue((Int64)o.Payload == 20);
            Assert.IsTrue(type == "int");
        }

        [Test]
        [Category("Debugger")]
        public void TestUpdateLoopWithProperties()
        {
            String code = @"
class A

{

    a : int[];            

}

def foo ( x1 : A)

{

    x1.a= -1;

    return = x1;

}

a1 = A.A();

a1.a = {1,2};

b = a1.a;

a1.a = -1;";

            fsr.PreStart(code, runnerConfig);
            fsr.Step(); // a1 = A.A();

            DebugRunner.VMState vms = fsr.Step();   // a : int[];        

            fsr.Step();
            vms = fsr.Step();   // a1 = A.A();
            Obj o = vms.mirror.GetDebugValue("a1");
            string type = vms.mirror.GetType("a1");
            Assert.IsTrue(type == "A");

            Dictionary<string, Obj> os = vms.mirror.GetProperties(o);
            Assert.IsTrue(os.Count == 1);

            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("a1");
            type = vms.mirror.GetType("a1");
            os = vms.mirror.GetProperties(o);
            type = vms.mirror.GetType(os["a"]);
            Assert.IsTrue(type == "array");
            List<Obj> lo = vms.mirror.GetArrayElements(os["a"]);
            Assert.IsTrue((Int64)lo[0].Payload == 1);
            Assert.IsTrue((Int64)lo[1].Payload == 2);

            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("b");
            type = vms.mirror.GetType("b");
            Assert.IsTrue(type == "array");
            lo = vms.mirror.GetArrayElements(o);
            Assert.IsTrue((Int64)lo[0].Payload == 1);
            Assert.IsTrue((Int64)lo[1].Payload == 2);

            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("a1");
            type = vms.mirror.GetType("a1");

            Assert.IsTrue(type == "A");

            os = vms.mirror.GetProperties(o);
            Assert.IsTrue(os.Count == 1);
            type = vms.mirror.GetType(os["a"]);
            Assert.IsTrue(type == "array");

            lo = vms.mirror.GetArrayElements(os["a"]);
            Assert.IsTrue((Int64)lo[0].Payload == -1);

            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("b");
            type = vms.mirror.GetType("b");
            Assert.IsTrue(type == "array");
            lo = vms.mirror.GetArrayElements(o);
            Assert.IsTrue((Int64)lo[0].Payload == -1);
        }

        [Test]
        [Category("Debugger")]
        [Category("Failure")]
        public void TestUpdateLoopWithNestedDifferentBlocks()
        {
            String code = @"
a = 7;
b = 3;
s = Print(""a = "" + a + "" b = "" + b);

[Imperative]
{
    b = 5 * a;
    s = Print(""b = "" + b);
    c = 0;
    d = 0;
    
    [Associative]
    {
        s = Print(""aa = "" + a);
        c = a + 2;
        s = Print(""cc = "" + c);
        a = 10;
        a = 33;        
    }
    
    d = c;
    s = Print(""dd = "" + d);
}
";
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3985
            fsr.PreStart(code, runnerConfig);
            fsr.Step(); // a = 7;

            DebugRunner.VMState vms = fsr.Step();   // b = 3;

            Obj o = vms.mirror.GetDebugValue("a");
            string type = vms.mirror.GetType("a");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 7);

            vms = fsr.Step();   // s = Print(...);

            o = vms.mirror.GetDebugValue("b");
            type = vms.mirror.GetType("b");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 3);

            vms = fsr.StepOver();
            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("b");
            type = vms.mirror.GetType("b");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 35);

            vms = fsr.StepOver();
            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("c");
            type = vms.mirror.GetType("c");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 0);

            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("d");
            type = vms.mirror.GetType("d");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 0);

            vms = fsr.StepOver();
            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("c");
            type = vms.mirror.GetType("c");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 9);

            vms = fsr.StepOver();
            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("a");
            type = vms.mirror.GetType("a");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 10);

            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("c");
            type = vms.mirror.GetType("c");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 12);

            vms = fsr.StepOver();
            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("a");
            type = vms.mirror.GetType("a");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 33);

            vms = fsr.Step();
            vms = fsr.StepOver();

            o = vms.mirror.GetDebugValue("c");
            type = vms.mirror.GetType("c");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 35);

            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.StepOver();

            o = vms.mirror.GetDebugValue("d");
            type = vms.mirror.GetType("d");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 35);

            vms = fsr.Step();
            Assert.AreEqual(4, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(4, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(37, vms.ExecutionCursor.EndExclusive.CharNo);
        }

        [Test]
        [Category("Debugger")]
        public void TestUpdateStaticMemberInClass()
        {
            String code =
@"
class A
{
    static count : var = 0;
    constructor A()
    {
        count = count + 1;
    }   

    
}

class B
{
    static count : var = 0;
    constructor B()
    {
        count = count + 1;
    }

    
}

a1 = A.A();
";
            fsr.PreStart(code, runnerConfig);
            fsr.Step();

            DebugRunner.VMState vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();

            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"a1.count");
            Obj objExecVal = mirror.GetWatchValue();
            Assert.IsTrue((Int64)objExecVal.Payload == 1);

            Obj o = vms.mirror.GetDebugValue("a1");
            string type = vms.mirror.GetType("a1");

            Dictionary<string, Obj> os = vms.mirror.GetProperties(o);
            Assert.IsTrue(type == "A");
            Assert.IsTrue(os.Count == 1);
        }

        [Test]
        [Category("Debugger")]
        public void TestUpdateLoopWithZiggedDifferentBlocks()
        {
            String code = @"
a = [Imperative]
{    
        c = 0;  
        return = c;
}

b = a - 1;

a = [Imperative]
{    
        c = 1;    
        return = c;
}
";
            fsr.PreStart(code, runnerConfig);
            fsr.Step();

            DebugRunner.VMState vms = fsr.Step();

            Obj o = vms.mirror.GetDebugValue("c");
            string type = vms.mirror.GetType("c");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 0);

            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("a");
            type = vms.mirror.GetType("a");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 0);

            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("b");
            type = vms.mirror.GetType("b");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == -1);

            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("c");
            type = vms.mirror.GetType("c");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 1);

            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("a");
            type = vms.mirror.GetType("a");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 1);

            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("b");
            type = vms.mirror.GetType("b");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 0);
        }

        [Test]
        [Category("Debugger")]
        public void TestUpdateLoopInsideFunctionInAssociativeBlock()
        {
            String code = @"
[Associative]
{ 
         def foo3 : int[] ( a : double )
         {
            return = a;
         }
         
        dummyArg = 1.5;
        
        b2 = foo3 ( dummyArg ); 
}

";
            fsr.PreStart(code, runnerConfig);
            fsr.Step();

            DebugRunner.VMState vms = fsr.Step();

            Obj o = vms.mirror.GetDebugValue("dummyArg");
            string type = vms.mirror.GetType("dummyArg");
            Assert.IsTrue(type == "double");
            Assert.IsTrue((Double)o.Payload == 1.5);

            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("a");
            type = vms.mirror.GetType("a");
            Assert.IsTrue(type == "double");
            Assert.IsTrue((Double)o.Payload == 1.5);

            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("b2");
            type = vms.mirror.GetType("b2");
            List<Obj> lo = vms.mirror.GetArrayElements(o);
            Assert.IsTrue(type == "array");
            Assert.IsTrue((Int64)lo[0].Payload == 2);
        }

        [Test]
        [Category("Debugger")]
        public void TestUpdateWithMutatingProperties()
        {
            String code = @"
class complex
{

        mx : var;
        
        constructor complex(px : int)
        {
            mx = px; 
        }

        def scale : int(s : int)
        {
            mx = mx * s; 
            return = 0;
        }
}

p = complex.complex(8);
i = p.mx;

// Calling a member function of class complex that mutates its properties 
k1 = p.scale(2); 
l1 = p.mx;";

            fsr.PreStart(code, runnerConfig);
            fsr.Step();
        }

        [Test]
        [Category("Debugger")]
        [Category("Failure")]
        public void TestSSAassignments1()
        {
            String code =
@"
class A
{
a : int;

  constructor func(x : int)
  { 
    a = x;
  }
  def diff(x: int)
  {
    b = a-x;
    return=A.func(b);
  }
}


x=2;
a1 = A.func(10);
a1=a1.diff(x);
x=3;
x=4;
";
            fsr.PreStart(code, runnerConfig);
            fsr.Step();

            DebugRunner.VMState vms = fsr.Step();
            Obj o = vms.mirror.GetDebugValue("x");
            string type = vms.mirror.GetType("x");

            Assert.IsTrue((Int64)o.Payload == 2);
            Assert.IsTrue(type == "int");

            vms = fsr.StepOver();
            //vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a1");
            type = vms.mirror.GetType("a1");

            Dictionary<string, Obj> os = vms.mirror.GetProperties(o);
            Assert.IsTrue(type == "A");
            Assert.IsTrue(os.Count == 1);
            Assert.IsTrue((Int64)os["a"].Payload == 10);

            vms = fsr.StepOver();
            //vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a1");
            type = vms.mirror.GetType("a1");
            os = vms.mirror.GetProperties(o);

            Assert.IsTrue(type == "A");
            Assert.IsTrue(os.Count == 1);
            Assert.IsTrue((Int64)os["a"].Payload == 8);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("x");
            type = vms.mirror.GetType("x");

            vms = fsr.StepOver();
            //vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a1");
            type = vms.mirror.GetType("a1");
            os = vms.mirror.GetProperties(o);

            Assert.IsTrue(type == "A");
            Assert.IsTrue(os.Count == 1);
            Assert.IsTrue((Int64)os["a"].Payload == 5);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("x");
            type = vms.mirror.GetType("x");

            Assert.IsTrue((Int64)o.Payload == 4);
            Assert.IsTrue(type == "int");

            vms = fsr.StepOver();
            //vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a1");
            type = vms.mirror.GetType("a1");
            os = vms.mirror.GetProperties(o);

            Assert.IsTrue(type == "A");
            Assert.IsTrue(os.Count == 1);
            Assert.IsTrue((Int64)os["a"].Payload == 1);
        }

        [Test]
        [Category("Debugger")]
        public void TestSSAassignments2()
        {
            String code =
        @"
a : int;
b : int;

[Associative]
{
    a = 10;
    b = 2 * a;
    a = a+1;
}";
            fsr.PreStart(code, runnerConfig);
            DebugRunner.VMState vms = fsr.Step();
            Obj o = vms.mirror.GetDebugValue("a");
            Assert.AreNotEqual(null, o);

            o = vms.mirror.GetDebugValue("b");
            Assert.AreNotEqual(null, o);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a");
            Assert.IsTrue((Int64)o.Payload == 10);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("b");
            Assert.IsTrue((Int64)o.Payload == 20);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a");
            Assert.IsTrue((Int64)o.Payload == 11);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("b");
            Assert.IsTrue((Int64)o.Payload == 22);

            vms = fsr.Step();
            Assert.IsTrue(vms.isEnded);
        }

        [Test]
        [Category("Debugger")]
        public void Numeric_Associative()
        {
            String code =
        @"

a : int;
b : int;

[Associative]
{
	a = 10;
	b = 2 * a;
	a = a + 1;
}";

            fsr.PreStart(code, runnerConfig);

            DebugRunner.VMState vms = fsr.Step();   // a = 10;
            Obj o = vms.mirror.GetDebugValue("a");
            Assert.IsTrue(null == o.Payload);

            o = vms.mirror.GetDebugValue("b");
            Assert.IsTrue(null == o.Payload);

            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("a");
            string type = vms.mirror.GetType("a");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 10);

            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("b");
            type = vms.mirror.GetType("b");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 20);

            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("a");
            type = vms.mirror.GetType("a");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 11);

            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("b");
            type = vms.mirror.GetType("b");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 22);
        }

        [Test]
        [Category("Debugger")]
        public void Numeric_Imperative()
        {
            String code =
        @"

a : int;
b : int;

[Imperative]
{
	a = 10;
	b = 2 * a;
	a = a + 1;
}";

            fsr.PreStart(code, runnerConfig);

            DebugRunner.VMState vms = fsr.Step();
            Obj o = vms.mirror.GetDebugValue("a");
            Assert.IsTrue(null == o.Payload);

            o = vms.mirror.GetDebugValue("b");
            Assert.IsTrue(null == o.Payload);

            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("a");
            string type = vms.mirror.GetType("a");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 10);

            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("b");
            type = vms.mirror.GetType("b");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 20);

            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("a");
            type = vms.mirror.GetType("a");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 11);
        }


     
        [Test]
        [Category("Debugger")]
        public void MirrorApiTest001()
        {
            string src = @"
                            class Point
                            {
                                x : int;
                                y : int;
                                z : int;

                                constructor Point(_x : int, _y : int, _z : int)
                                {
                                    x = _x;
                                    y = _y;
                                    z = _z;
                                }
                            }
                            p = Point.Point(1, 2, 3);
                         ";
            fsr.PreStart(src, runnerConfig);
            //fsr.Step(DebugRunner.BreakMode.ExplicitBreak);
            DebugRunner.VMState vms = fsr.Run();
            Obj o = vms.mirror.GetDebugValue("p");
            string type = vms.mirror.GetType(o);
            Dictionary<string, Obj> os = vms.mirror.GetProperties(o);

            Assert.IsTrue(type == "Point");
            Assert.IsTrue((Int64)os["z"].Payload == 3);
        }

        [Test]
        [Category("Debugger")]
        public void MirrorApiTest002()
        {
            string src = @"
                            a = { 1, 2, 3, { 4, 5, 6 }, 7, 8 };
                         ";
            fsr.PreStart(src, runnerConfig);
            fsr.Step();
            DebugRunner.VMState vms = fsr.Step();
            Obj o = vms.mirror.GetDebugValue("a");
            string type = vms.mirror.GetType(o);
            List<Obj> os = vms.mirror.GetArrayElements(o);

            Assert.IsTrue(type == "array");
            Assert.IsTrue(os.Count == 6);
            Assert.IsTrue((Int64)vms.mirror.GetArrayElements(os[3])[1].Payload == 5);
        }

        [Test]
        [Category("Debugger")]
        public void StepOver001()
        {
            string src = @"
                        def foo : int(i : int, j : int)
                        {
                            return = i + j;
                        }
                        b = foo(1, 2);";

            fsr.PreStart(src, runnerConfig);
            DebugRunner.VMState vms = fsr.StepOver();
            vms = fsr.StepOver();
            Obj o = vms.mirror.GetDebugValue("b");
            Assert.IsTrue((Int64)o.Payload == 3);
        }

        [Test]
        [Category("Debugger")]
        public void StepOver002()
        {
            string src = @"
                        class Point
                        {
	                        x : int;
	                        y : int;
	                        z : int;
	
	                        constructor Point(_x : int, _y : int, _z : int)
	                        {
		                        x = _x;
		                        y = _y;
		                        z = _z;
	                        }
                        }

                        def foo : int[] (p : Point)
                        {
	                        a = { p.x, p.y, p.z};
                            return = a;
                        }

                        p = Point.Point(3, 4, 5);
                        a = foo(p);";

            fsr.PreStart(src, runnerConfig);
            DebugRunner.VMState vms = fsr.Step();
            vms = fsr.StepOver();
            vms = fsr.Step();
            Obj o = vms.mirror.GetDebugValue("p");
            string name = vms.mirror.GetType(o);
            Assert.IsTrue(name == "Point");
            Dictionary<string, Obj> os = vms.mirror.GetProperties(o);
            Assert.IsTrue(os.Count == 3);
            Assert.IsTrue((Int64)os["x"].Payload == 3);

            vms = fsr.StepOver();
            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a");
            name = vms.mirror.GetType(o);
            Assert.IsTrue(name == "array");
            List<Obj> lo = vms.mirror.GetArrayElements(o);
            Assert.IsTrue((Int64)lo[2].Payload == 5);
        }

        [Test]
        [Category("Debugger")]
        public void StepOver003()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
@"

class Sample
{
    sample_member : int;
}

sample = Sample.Sample();
irrelevant = 3;

", runnerConfig);

            // First step should bring the exec cursor to "sample = Sample.Sample()".
            DebugRunner.VMState vms = fsr.StepOver();
            Assert.AreEqual(8, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(8, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(26, vms.ExecutionCursor.EndExclusive.CharNo);

            // Third step should bring it to "irrelevant = 3;".
            vms = fsr.StepOver();
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(9, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(16, vms.ExecutionCursor.EndExclusive.CharNo);
        }

        [Test]
        [Category("Debugger")]
        [Category("Failure")]
        public void ToggleBreakPoint001()
        {
            string src = @"
                            def foo : int(i : int, j : int)
                            {
                                return = i + j;
                            }
                            a = 0;
                            a = 1 + 2 + foo(3, 4) + 5 + foo(5, 6);
                            ";

            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3991
            string defectID = "MAGN-3991 Defects with Toggle breakpoint";
            fsr.PreStart(src, runnerConfig);
            ProtoCore.CodeModel.CodePoint cp = new ProtoCore.CodeModel.CodePoint
            {
                LineNo = 7,
                CharNo = 44
            };
            fsr.ToggleBreakpoint(cp);
            cp = new ProtoCore.CodeModel.CodePoint
            {
                LineNo = 7,
                CharNo = 60
            };
            fsr.ToggleBreakpoint(cp);
            DebugRunner.VMState vms = fsr.Run();
            Obj o = vms.mirror.GetDebugValue("a");
            vms = fsr.Run();
            Obj o2 = vms.mirror.GetDebugValue("a");

            Assert.IsTrue((Int64)o.Payload == 0, defectID);
            Assert.IsTrue((Int64)o2.Payload == 26, defectID);
        }
        [Test]
        [Category("Debugger")]
        public void ToggleBreakPoint_Imperative_003()
        {
            string src = @"
a : int = 0;
b : int = 0;

[Imperative]
{
    a = 10;
    b = a * 2;
    a = 15;
}
";

            fsr.PreStart(src, runnerConfig);
            ProtoCore.CodeModel.CodePoint cp = new ProtoCore.CodeModel.CodePoint
            {
                LineNo = 8,
                CharNo = 0
            };
            fsr.ToggleBreakpoint(cp);
            DebugRunner.VMState vms = fsr.Run();
            vms = fsr.Step();
            Obj o2 = vms.mirror.GetDebugValue("a");

            Assert.IsTrue((Int64)o2.Payload == 10);
            vms = fsr.Run();
            Assert.IsTrue(vms.isEnded);
        }
        [Test]
        [Category("Debugger")]
        public void ToggleBreakPoint_Associative_004()
        {
            string src = @"
                        a : int = 0;
                        b : int = 0;

                        [Associative]
                        {
                            a = 10;
                            b = a * 2;
                            a = 15;
                        }
                        ";

            fsr.PreStart(src, runnerConfig);
            ProtoCore.CodeModel.CodePoint cp = new ProtoCore.CodeModel.CodePoint
            {
                LineNo = 8,
                CharNo = 8
            };
            fsr.ToggleBreakpoint(cp);
            fsr.Step();
            DebugRunner.VMState vms = fsr.Run();
            Obj o2 = vms.mirror.GetDebugValue("a");

            Assert.IsTrue((Int64)o2.Payload == 10);
            vms = fsr.Run();
            vms = fsr.Run();
            Assert.IsTrue(vms.isEnded);
        }
        [Test]
        [Category("Debugger")]
        [Category("Failure")]
        public void ToggleBreakPoint005()
        {
            string src = @"
                        a = 10; // single value
                        b = a * 2;

                        a = { 1, 4, -2 }; // arbitrary collection

                        a = 1..10; // range expression... assume 1 as increment

                        a = 1..10..2; // range expression with defined increment

                        a = 1..10..~2; // range expression with 'appropriate' increment

                        a = 1..10..#3; // range expression with specified number of cases

                        a = 10; // back to single values;
                        b = 2;

                        c = a * b; // define an expression;

                        a = 10..12;
                        b = 2..4;

                        c = a<2> * b<1>; // cartesian replication
                        ";

            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3991
            string defectID = "MAGN-3991 Defects with Toggle breakpoint";
            fsr.PreStart(src, runnerConfig);
            ProtoCore.CodeModel.CodePoint cp = new ProtoCore.CodeModel.CodePoint
            {
                LineNo = 18,
                CharNo = 5
            };
            fsr.ToggleBreakpoint(cp);
            DebugRunner.VMState vms = fsr.Run();
            Obj o2 = vms.mirror.GetDebugValue("b");

            Assert.IsTrue((Int64)o2.Payload == 2, defectID);
            fsr.Run();
            Assert.IsTrue(vms.isEnded, defectID);
        }
        [Test]
        [Category("Debugger")]
        public void ToggleBreakPoint006()
        {
            string src = @"
                        a=1;
                        ";
            fsr.PreStart(src, runnerConfig);
            ProtoCore.CodeModel.CodePoint cp = new ProtoCore.CodeModel.CodePoint
            {
                LineNo = 1,
                CharNo = 2
            };
            fsr.ToggleBreakpoint(cp);
            DebugRunner.VMState vms = fsr.Run();
            Obj o2 = vms.mirror.GetDebugValue("a");
            Assert.IsTrue((Int64)o2.Payload == 1);
            Assert.IsTrue(vms.isEnded);
        }

        [Test]
        [Category("Debugger")]
        public void ToggleBreakPointApiTest()
        {
            string src = @"class A
{
    w : int;
}
zz = A.A();
[Imperative]
{
    def g()
    {
        return = 3;
    }
    def f(a : int)
    {
        return = a;
    }
    
    c2 = A.A();

    c1 = c3 =    c2.w = f(g());

    z = 67;
}
                                        ";

            fsr.PreStart(src, runnerConfig);
            ProtoCore.CodeModel.CodePoint cp = new ProtoCore.CodeModel.CodePoint
            {
                LineNo = 19,
                CharNo = 27
            };
            fsr.ToggleBreakpoint(cp);

            DebugRunner.VMState vms = fsr.Run();
            Assert.AreEqual(19, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(19, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(32, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();
            Assert.AreEqual(10, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(10, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(20, vms.ExecutionCursor.EndExclusive.CharNo);
        }

        [Test]
        [Category("Debugger")]
        public void ToggleBreakPoint002()
        {
            string src = @"
                            def foo : int(i : int, j : int)
                            {
                                return = i + j;
                            }
                            a = 0;
                            a = 1 + 2 + foo(3, 4) + 5 + foo(5, 6);
                            ";

            fsr.PreStart(src, runnerConfig);
            ProtoCore.CodeModel.CodePoint cp = new ProtoCore.CodeModel.CodePoint
            {
                LineNo = 7,
                CharNo = 44
            };
            fsr.ToggleBreakpoint(cp);
            cp = new ProtoCore.CodeModel.CodePoint
            {
                LineNo = 7,
                CharNo = 60
            };
            fsr.ToggleBreakpoint(cp);
            fsr.ToggleBreakpoint(cp);
            DebugRunner.VMState vms = fsr.Run();
            Obj o = vms.mirror.GetDebugValue("a");
            vms = fsr.Run();
            Obj o2 = vms.mirror.GetDebugValue("a");

            Assert.IsTrue((Int64)o.Payload == 0);
            Assert.IsTrue((Int64)o2.Payload == 26);
            Assert.IsTrue(vms.isEnded);
        }

        [Test]
        [Category("Debugger"), Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void TestFFIDebugging()
        {
            String code =
            @"import(""ProtoGeometry.dll"");
              import(Dummy from ""FFITarget.dll"");
             [Associative] 
             {
               dummy = Dummy.Dummy();
               success = dummy.CallMethod();
               point = Point.ByCoordinates(1,2,3);
               x = point.X;
                b = 9;
             }
            ";

            fsr.PreStart(code, runnerConfig);
            fsr.Step();

            fsr.Step();
            DebugRunner.VMState vms = fsr.Step();
            Obj o = vms.mirror.GetDebugValue("dummy");
            Assert.AreEqual("FFITarget.Dummy", vms.mirror.GetType(o));

            fsr.Step();
            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("success");
            Assert.AreEqual("bool", vms.mirror.GetType("success"));
            Assert.AreEqual(true, (bool)o.Payload);

            fsr.Step();
            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("point");
            Assert.AreEqual("Autodesk.DesignScript.Geometry.Point", vms.mirror.GetType(o));
            Dictionary<string, Obj> os = vms.mirror.GetProperties(o);
            //Assert.IsTrue((double)os["X"].Payload == 1);
            //Assert.IsTrue((double)os["Y"].Payload == 2);
            //Assert.IsTrue((double)os["Z"].Payload == 3);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("x");
            Assert.AreEqual(1, (double)o.Payload);
        }

        [Test]
        [Category("Debugger"), Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void TestFFIDebugWithUpdate()
        {
            String code =
                @"
import(""ProtoGeometry.dll"");

x = 1;
p1 = Point.ByCoordinates(x, 2, 0);
p2 = Point.ByCoordinates(1, 10, 0);

l1 = Line.ByStartPointEndPoint(p1, p2);

x = 10;

";
            fsr.PreStart(code, runnerConfig);
            fsr.Step();

            DebugRunner.VMState vms = fsr.Step();
            Obj o = vms.mirror.GetDebugValue("x");
            Assert.AreEqual("int", vms.mirror.GetType(o));

            fsr.Step();
            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("p1");
            Assert.AreEqual("Autodesk.DesignScript.Geometry.Point", vms.mirror.GetType(o));

            fsr.Step();
            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("p2");
            Assert.AreEqual("Autodesk.DesignScript.Geometry.Point", vms.mirror.GetType(o));

            fsr.Step();
            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("l1");
            Assert.AreEqual("Autodesk.DesignScript.Geometry.Line", vms.mirror.GetType(o));

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("x");
            Assert.AreEqual("int", vms.mirror.GetType(o));

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("p1");
            Assert.AreEqual("Autodesk.DesignScript.Geometry.Point", vms.mirror.GetType(o));

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("l1");
            Assert.AreEqual("Autodesk.DesignScript.Geometry.Line", vms.mirror.GetType(o));
        }

        [Test]
        [Category("Debugger")]
        public void TestFFISetPropertyImperative()
        {
            String code =
            @"
import(DummyBase from ""FFITarget.dll"");

[Imperative]
{
    dummy = DummyBase.Create();
    dummy.Value = 868760;
    a = dummy.Value;
    b = 9;
}";

            fsr.PreStart(code, runnerConfig);
            fsr.Step();

            fsr.Step();
            DebugRunner.VMState vms = fsr.Step();
            Obj o = vms.mirror.GetDebugValue("dummy");
            //Verify the returned object type name is fully qualified name.
            Assert.IsTrue(vms.mirror.GetType(o) == "FFITarget.DummyBase");

            fsr.Step();
            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a");
            Assert.IsTrue((Int64)o.Payload == 868760);
        }

        [Test]
        [Category("Debugger")]
        public void TestFFISetPropertyAssociative()
        {
            String code =
            @"
import(DummyBase from ""FFITarget.dll"");

dummy = DummyBase.Create();
dummy.Value = 868760;
a = dummy.Value;";

            fsr.PreStart(code, runnerConfig);
            fsr.Step();

            fsr.Step();
            DebugRunner.VMState vms = fsr.Step();
            Obj o = vms.mirror.GetDebugValue("dummy");
            //Verify the returned object type name is fully qualified name.
            Assert.IsTrue(vms.mirror.GetType(o) == "FFITarget.DummyBase");

            fsr.Step();
            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a");
            Assert.IsTrue((Int64)o.Payload == 868760);
        }

        [Test]
        [Category("Debugger")]
        public void ImportTest001()
        {
            string src =
                Path.GetFullPath(string.Format("{0}{1}", testPath, "ImportTest001.ds"));
            string imp1 =
                Path.GetFullPath(string.Format("{0}{1}", testPath, "import001.ds"));
            string imp2 =
                Path.GetFullPath(string.Format("{0}{1}", testPath, "import002.ds"));

            fsr.LoadAndPreStart(src, runnerConfig);

            ProtoCore.CodeModel.CodePoint cp = new ProtoCore.CodeModel.CodePoint
            {
                LineNo = 5,
                CharNo = 3,
                SourceLocation = new ProtoCore.CodeModel.CodeFile
                {
                    FilePath = src
                }
            };
            fsr.ToggleBreakpoint(cp);
            cp = new ProtoCore.CodeModel.CodePoint
            {
                LineNo = 3,
                CharNo = 9,
                SourceLocation = new ProtoCore.CodeModel.CodeFile
                {
                    FilePath = imp1
                }
            };
            fsr.ToggleBreakpoint(cp);
            fsr.ToggleBreakpoint(cp);
            cp = new ProtoCore.CodeModel.CodePoint
            {
                LineNo = 3,
                CharNo = 9,
                SourceLocation = new ProtoCore.CodeModel.CodeFile
                {
                    FilePath = imp2
                }
            };
            fsr.ToggleBreakpoint(cp);

            DebugRunner.VMState vms = fsr.Run();
            Obj o1 = vms.mirror.GetDebugValue("a");
            Assert.IsTrue(vms.ExecutionCursor.StartInclusive.SourceLocation.FilePath == src);
            Assert.IsTrue((Int64)o1.Payload == 10);
            vms = fsr.Run();
            Obj o2 = vms.mirror.GetDebugValue("j");
            Assert.IsTrue((Int64)o2.Payload == 20);
            Assert.IsTrue(vms.ExecutionCursor.StartInclusive.SourceLocation.FilePath == imp2);

        }

        [Test]
        [Category("Debugger")]
        public void LanguageBlockInsideFunction()
        {
            string src = @"
                           def foo : int()
                            {
	                            return = [Imperative]
	                            {
		                            a = 10;
		                            b = 20;
		                            return = a + b;
	                            }
                            }
                            c = foo();
                            ";

            fsr.PreStart(src, runnerConfig);
            fsr.Step();

            DebugRunner.VMState vms = fsr.Step();
            vms = fsr.Step();
            Obj o = vms.mirror.GetDebugValue("a");
            string type = vms.mirror.GetType("a");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 10);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("b");
            type = vms.mirror.GetType("b");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 20);

            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("c");
            type = vms.mirror.GetType("c");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 30);
        }

        [Test]
        [Category("Debugger")]
        public void LanguageBlockInsideFunction1()
        {
            string src = @"
def foo()
{
	[Associative]
	{
	    c = 9;	    
	}
    return = 9;
}

a = foo();
";
            fsr.PreStart(src, runnerConfig);
            DebugRunner.VMState vms = fsr.Step();

            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();

            Obj o = vms.mirror.GetDebugValue("a");
            string type = vms.mirror.GetType("a");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 9);
        }

        [Test]
        [Category("Debugger")]
        public void LanguageBlockInsideFunction2()
        {
            string src = @"
                           def clampRange : int(i : int, rangeMin : int, rangeMax : int)
                            {
                                result = [Imperative]
                                {
	                                clampedValue = i;
	                                if(i < rangeMin) 
	                                {
		                                clampedValue = rangeMin;
	                                }
	                                elseif( i > rangeMax ) 
	                                {
		                                clampedValue = rangeMax; 
	                                } 
                                    return = clampedValue;
                                }
	                            return = result;
                            }
                            a = clampRange(101, 10, 100);
                            ";

            fsr.PreStart(src, runnerConfig);

            fsr.Step();
            DebugRunner.VMState vms = fsr.Step();
            Obj o = vms.mirror.GetDebugValue("i");
            Assert.IsTrue((Int64)o.Payload == 101);
            o = vms.mirror.GetDebugValue("rangeMin");
            Assert.IsTrue((Int64)o.Payload == 10);
            o = vms.mirror.GetDebugValue("rangeMax");
            Assert.IsTrue((Int64)o.Payload == 100);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("clampedValue");
            Assert.IsTrue((Int64)o.Payload == 101);

            int startLineNo, startCharNo;
            vms = fsr.Step();
            // test for line (ie the elseif)   
            startLineNo = vms.ExecutionCursor.StartInclusive.LineNo;
            startCharNo = vms.ExecutionCursor.StartInclusive.CharNo;

            Assert.IsTrue((Int64)startLineNo == 11);
            Assert.IsTrue((Int64)startCharNo == 34);

            vms = fsr.Step();
            // test for line (ie inside elseif)
            startLineNo = vms.ExecutionCursor.StartInclusive.LineNo;
            startCharNo = vms.ExecutionCursor.StartInclusive.CharNo;

            Assert.IsTrue((Int64)startLineNo == 13);
            Assert.IsTrue((Int64)startCharNo == 35);

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("clampedValue");
            Assert.IsTrue((Int64)o.Payload == 100);

            fsr.Step();
            fsr.Step();

            vms = fsr.Step();
            fsr.Step();
            o = vms.mirror.GetDebugValue("result");
            Assert.IsTrue((Int64)o.Payload == 100);

            fsr.Step();
            fsr.Step();

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a");
            Assert.IsTrue((Int64)o.Payload == 100);
        }


        [Test]
        [Category("Debugger")]
        public void LanguageBlockInsideFunction5()
        {
            string src =
@"gg = 0;

def foo ()
{
    arr = { { } };

    [Imperative]
    {
       
        for(i in {0, 1})
        {
            [Associative]
            {
                gg = i;
                arr[i] = {1, 2};
            } 
        }
    }
    return = arr;
}
test = foo();";

            fsr.PreStart(src, runnerConfig);
            fsr.Step(); // gg = 0;

            DebugRunner.VMState vms = fsr.Step();   // test = foo();
            //Obj o = vms.mirror.GetDebugValue("gg");
            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"gg");
            Obj o = mirror.GetWatchValue();
            Assert.IsTrue((Int64)o.Payload == 0);

            fsr.Step(); // arr = { { } };

            vms = fsr.Step();
            //o = vms.mirror.GetDebugValue("arr");
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"arr");
            //Assert.IsTrue(vms.mirror.GetType("arr") == "array");
            Assert.IsTrue(mirror.GetType("arr") == "array");

            fsr.Step();
            fsr.Step();
            fsr.Step();

            vms = fsr.Step();   // gg = i;
            //o = vms.mirror.GetDebugValue("i");
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"i");
            o = mirror.GetWatchValue();
            Assert.IsTrue((Int64)o.Payload == 0);

            vms = fsr.Step();   // arr[i] = {1, 2};
            //o = vms.mirror.GetDebugValue("gg");
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"gg");
            o = mirror.GetWatchValue();
            Assert.IsTrue((Int64)o.Payload == 0);

            vms = fsr.Step();
            //o = vms.mirror.GetDebugValue("arr");
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"arr");
            o = mirror.GetWatchValue();

            List<Obj> ol = mirror.GetArrayElements(o);
            Assert.IsTrue(ol.Count == 1);
            List<Obj> ol_1 = mirror.GetArrayElements(ol[0]);
            Assert.IsTrue(ol_1.Count == 2);
            Assert.IsTrue((Int64)ol_1[0].Payload == 1);
            Assert.IsTrue((Int64)ol_1[1].Payload == 2);

            fsr.Step();
            fsr.Step();

            vms = fsr.Step();
            fsr.Step();     // gg = i;
            //o = vms.mirror.GetDebugValue("i");
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"i");
            o = mirror.GetWatchValue();
            Assert.IsTrue((Int64)o.Payload == 1);

            vms = fsr.Step();   // arr[i] = {1, 2};
            //o = vms.mirror.GetDebugValue("gg");
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"gg");
            o = mirror.GetWatchValue();
            Assert.IsTrue((Int64)o.Payload == 1);

            vms = fsr.Step();
            //o = vms.mirror.GetDebugValue("arr");
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"arr");
            o = mirror.GetWatchValue();
            ol = vms.mirror.GetArrayElements(o);
            //Assert.IsTrue(ol.Count == 2);
            List<Obj> ol_2 = vms.mirror.GetArrayElements(ol[1]);
            Assert.IsTrue(ol_2.Count == 2);
            Assert.IsTrue((Int64)ol_2[0].Payload == 1);
            Assert.IsTrue((Int64)ol_2[1].Payload == 2);

            fsr.Step();
            fsr.Step();
            fsr.Step();
            fsr.Step();
            fsr.Step();

            vms = fsr.Step();
            fsr.Step();
            //o = vms.mirror.GetDebugValue("test");
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"test");
            o = mirror.GetWatchValue();
            ol = vms.mirror.GetArrayElements(o);
            Assert.IsTrue(ol.Count == 2);

            ol_1 = vms.mirror.GetArrayElements(ol[0]);
            Assert.IsTrue(ol_1.Count == 2);
            Assert.IsTrue((Int64)ol_1[0].Payload == 1);
            Assert.IsTrue((Int64)ol_1[1].Payload == 2);

            ol_2 = vms.mirror.GetArrayElements(ol[1]);
            Assert.IsTrue(ol_2.Count == 2);
            Assert.IsTrue((Int64)ol_2[0].Payload == 1);
            Assert.IsTrue((Int64)ol_2[1].Payload == 2);
        }

        [Test]
        [Category("Debugger")]
        public void FunctionPointer1()
        {
            string src =
@"arr = { 3, 5, 1, 5, 3, 4, 7, true, 5, null, 12};
def Compare(x, y)
{
    return = [Imperative]
    {
        if (null == x)
            return = -1;
        if (null == y)
            return = 1;
        
        a : int = x;
        b : int = y;
        if (null == a && null == b)
            return = 0;
        if (null == a)
            return = - 1;
        if (null == b)
            return = 1;
        return = (a - b);
    }
}

sorted = Sort(Compare, arr); //Stepping over this statement throws exception for empty stack.
";
            //Assert.Fail("IDE-390 Stepping into a function throws an index out of range exception (function pointer)");
            fsr.PreStart(src, runnerConfig);
            fsr.Step();

            DebugRunner.VMState vms = fsr.Step();

            Obj o = vms.mirror.GetDebugValue("arr");
            string type = vms.mirror.GetType("arr");
            List<Obj> lo = vms.mirror.GetArrayElements(o);
            Assert.IsTrue(type == "array");
            Assert.IsTrue((Int64)lo[0].Payload == 3);
            Assert.IsTrue((Int64)lo[1].Payload == 5);
            Assert.IsTrue((Int64)lo[2].Payload == 1);
            Assert.IsTrue((Int64)lo[3].Payload == 5);
            Assert.IsTrue((Int64)lo[4].Payload == 3);
            Assert.IsTrue((Int64)lo[5].Payload == 4);
            Assert.IsTrue((Int64)lo[6].Payload == 7);
            Assert.IsTrue((Boolean)lo[7].Payload == true);
            Assert.IsTrue((Int64)lo[8].Payload == 5);
            Assert.AreEqual(null, lo[9].Payload);
            Assert.IsTrue((Int64)lo[10].Payload == 12);

            fsr.Step();
            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("sorted");
            type = vms.mirror.GetType("sorted");
            lo = vms.mirror.GetArrayElements(o);
            Assert.IsTrue(type == "array");
            Assert.AreEqual(null, lo[0].Payload);
            Assert.IsTrue((Boolean)lo[1].Payload == true);
            Assert.IsTrue((Int64)lo[2].Payload == 1);
            Assert.IsTrue((Int64)lo[3].Payload == 3);
            Assert.IsTrue((Int64)lo[4].Payload == 3);
            Assert.IsTrue((Int64)lo[5].Payload == 4);
            Assert.IsTrue((Int64)lo[6].Payload == 5);
            Assert.IsTrue((Int64)lo[7].Payload == 5);
            Assert.IsTrue((Int64)lo[8].Payload == 5);
            Assert.IsTrue((Int64)lo[9].Payload == 7);
            Assert.IsTrue((Int64)lo[10].Payload == 12);
        }

        [Test]
        [Category("Debugger")]
        public void Defect_IDE442()
        {
            string src =
        @"class A 
{
    x : var;    
    constructor A( )
    {
        x = 0;
    }
}
class B extends A 
{
    t : int;
    constructor B( y: int)
    {
        t = y;
    }
}

a1 = B.B( 0..1);

test = a1.t;";

            //Assert.Fail("IDE-442 Debugger failing to break at getting and setting class properties in inheritance hierarchy (happens only with replication)");
            fsr.PreStart(src, runnerConfig);
            fsr.Step();

            DebugRunner.VMState vms = fsr.Step();

            vms = fsr.Step(); //This does not step into the constructor of B, correct or incorrect?

            Obj o = vms.mirror.GetDebugValue("a1");
            string type = vms.mirror.GetType("a1");

            Assert.IsTrue(type == "array");
            List<Obj> lo = vms.mirror.GetArrayElements(o);

            type = vms.mirror.GetType(lo[0]);
            Assert.IsTrue(type == "B");
            type = vms.mirror.GetType(lo[1]);
            Assert.IsTrue(type == "B");
            Dictionary<string, Obj> os_0 = vms.mirror.GetProperties(lo[0]);
            Dictionary<string, Obj> os_1 = vms.mirror.GetProperties(lo[1]);

            Assert.IsTrue((Int64)os_0["x"].Payload == 0);
            Assert.IsTrue((Int64)os_0["t"].Payload == 0);
            Assert.IsTrue((Int64)os_1["x"].Payload == 0);
            Assert.IsTrue((Int64)os_1["t"].Payload == 1);

            vms = fsr.Step();

            o = vms.mirror.GetDebugValue("test");
            type = vms.mirror.GetType("test");

            Assert.IsTrue(type == "array");
            lo = vms.mirror.GetArrayElements(o);
            Assert.IsTrue((Int64)lo[0].Payload == 0);
            Assert.IsTrue((Int64)lo[1].Payload == 1);
        }

        [Test]
        [Category("Debugger"), Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void TestNestedReplication()
        {
            string src =
        @"
import(""ProtoGeometry.dll"");

def makeSurf(p)
{
    pts = p;
    
    return = BSplineSurface.ByPoints(pts);
}
ps = Point.ByCoordinates((-10..10..1.5), -10, 0);
surf = makeSurf(ps);
";
            fsr.PreStart(src, runnerConfig);
            fsr.Step();
        }
        [Test]
        [Category("Debugger")]
        public void Defect_IDE446()
        {
            string src =
        @"a : int;
                b : int;
                [Associative]
                {
                    a = 10;
                    b = 2 * a;
                    a = a+1;
                }";

            //Assert.Fail("IDE-442 Debugger failing to break at getting and setting class properties in inheritance hierarchy (happens only with replication)");
            fsr.PreStart(src, runnerConfig);
            fsr.Step(); // a = 10;

            DebugRunner.VMState vms = fsr.Step();   // b = 2 * a;

            Obj ao = vms.mirror.GetDebugValue("a");
            string typea = vms.mirror.GetType("a");
            Assert.IsTrue((Int64)ao.Payload == 10);

            vms = fsr.Step();   // a = a+1;
            Obj bo = vms.mirror.GetDebugValue("b");
            string typeb = vms.mirror.GetType("b");
            Assert.IsTrue((Int64)bo.Payload == 20);

            vms = fsr.Step();   // b = 2 * a;
            Obj ao7 = vms.mirror.GetDebugValue("a");
            string typea7 = vms.mirror.GetType("a");
            Assert.IsTrue((Int64)ao7.Payload == 11);

            vms = fsr.Step();
            Obj bo6 = vms.mirror.GetDebugValue("b");
            string typeb6 = vms.mirror.GetType("b");
            Assert.IsTrue((Int64)bo6.Payload == 22);

        }

        [Test]
        [Category("Debugger")]
        public void Defect_IDE_464()
        {
            string src =
        @"class A
            {
                a : int[];
            }
            def foo(x1 : A)
            {
                x1.a = -1;
                return = x1;
            }
            a1 = A.A();
            a1.a = { 1, 2 };
            b = a1.a;
            a1.a = -1;";

            fsr.PreStart(src, runnerConfig);
            fsr.Step(); // a1 = A.A();

            DebugRunner.VMState vms = fsr.Step();   // a : int[];
            fsr.Step();
            fsr.Step();
            fsr.Step();
            vms = fsr.Step();   // a1.a = -1;

            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"b[0]");
            Obj o1 = mirror.GetWatchValue();
            Assert.AreEqual(1, (Int64)o1.Payload);

            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"b[1]");
            o1 = mirror.GetWatchValue();
            Assert.AreEqual(2, (Int64)o1.Payload);

            fsr.Step();
            vms = fsr.Step();

            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"b[0]");
            o1 = mirror.GetWatchValue();
            Assert.AreEqual(-1, (Int64)o1.Payload);
        }

        [Test]
        [Category("Debugger")]
        public void Defect_IDE_368()
        {
            string src =
        @"
            class point
            {
                x : int;

            }

            s = point.point();
            f = { 1, 2, 3, 4, s }; 
            f = { 1, 2, 3 }; 

            n = 2;


            ";

            //Assert.Fail("IDE-442 Debugger failing to break at getting and setting class properties in inheritance hierarchy (happens only with replication)");
            fsr.PreStart(src, runnerConfig);
            fsr.Step();
            fsr.Step();
            fsr.Step();
            DebugRunner.VMState vms = fsr.Step();
            Obj o3 = vms.mirror.GetDebugValue("s");
            string type3 = vms.mirror.GetType(o3);
            Dictionary<string, Obj> os = vms.mirror.GetProperties(o3);
            Assert.IsTrue(type3 == "point");
            Assert.IsTrue((Int64)os["x"].Payload == 0);

        }

        [Test]
        [Category("Debugger")]
        [Category("Failure")]
        public void Defect_IDE_442()
        {
            string src =
        @"
            class A 
{
x : int; 
constructor A( y)
{
x = y;
}
}
class B extends A 
{
t : int;
constructor B( y)
{
x = y;
t = x + 1;
}
}
a1 = { B.B(1), { A.A(2), B.B( 0..1) } };
test = a1.x; //expected : { 1, { 2, { 0, 1 } } }
            ";

            //Assert.Fail("IDE-442 Debugger failing to break at getting and setting class properties in inheritance hierarchy (happens only with replication)");

            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1701
            //TODO: Fails in the language with the new stackframe - 24/01/13
            fsr.PreStart(src, runnerConfig);
            fsr.Step();
            fsr.Step();
            fsr.Step();
            fsr.Step();

            DebugRunner.VMState vms = fsr.Step();
            Obj o = vms.mirror.GetDebugValue("x");
            string type = vms.mirror.GetType(o);
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 1);
            vms = fsr.Step();
            Obj o1 = vms.mirror.GetDebugValue("t");
            string type1 = vms.mirror.GetType(o1);
            Assert.IsTrue(type1 == "int");
            Assert.IsTrue((Int64)o1.Payload == 2);
            fsr.Step();
            fsr.Step();
            fsr.Step();
            vms = fsr.Step();
            Obj o2 = vms.mirror.GetDebugValue("x");
            string type2 = vms.mirror.GetType(o2);
            Assert.IsTrue(type2 == "int");
            Assert.IsTrue((Int64)o2.Payload == 2);
            fsr.Step();
            fsr.Step();
            fsr.Step();
            fsr.Step();
            vms = fsr.Step();
            Obj o3 = vms.mirror.GetDebugValue("test");
            List<Obj> ol3 = vms.mirror.GetArrayElements(o3);
            string type3 = vms.mirror.GetType(ol3[0]);
            Assert.IsTrue(type3 == "int");
            Assert.IsTrue((Int64)ol3[0].Payload == 1);
        }

        [Test]
        [Category("Debugger")]
        public void Defect_IDE_434()
        {
            string src =
        @"
           class B
                {
                    x : var;
                    constructor B(y)
                    {
                        x = y;
                    }
    
                    def foo()
                    {
                        return = 90;
                    }
                }

                x = 1;
                a =
                {
                    x => a1;
                    - 0.5 => a2;
                    a2 * 4 => a3;                
                    a1 > 10 ? true : false => a4;
                    a1..2 => a5;          
                    { a3, a3 } => a6;     
                    B.B(a1) => a7;    
                    foo(); 
                    B.B(a1).x => a8;
                }

            ";

            //Assert.Fail("IDE-442 Debugger failing to break at getting and setting class properties in inheritance hierarchy (happens only with replication)");
            fsr.PreStart(src, runnerConfig);
            fsr.Step();
            DebugRunner.VMState vms = fsr.Step();
            Obj o = vms.mirror.GetDebugValue("x");
            string type = vms.mirror.GetType(o);
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 1);
            fsr.Step();
            Obj o1 = vms.mirror.GetDebugValue("a1");
            string type1 = vms.mirror.GetType(o1);
            Assert.IsTrue(type1 == "int");
            Assert.IsTrue((Int64)o1.Payload == 1);
            fsr.Step();
            Obj o2 = vms.mirror.GetDebugValue("a2");
            string type2 = vms.mirror.GetType(o2);
            Assert.IsTrue(type2 == "double");
            Assert.IsTrue((Double)o2.Payload == 0.5);
            fsr.Step();
            Obj o3 = vms.mirror.GetDebugValue("a3");
            string type3 = vms.mirror.GetType(o3);
            Assert.IsTrue(type3 == "double");
            Assert.IsTrue((Double)o3.Payload == 2);
            fsr.Step();
            Obj o4 = vms.mirror.GetDebugValue("a4");
            string type4 = vms.mirror.GetType(o4);
            Assert.IsTrue(type4 == "bool");
            Assert.IsTrue((Boolean)o4.Payload == false);
            fsr.Step();
            Obj o5 = vms.mirror.GetDebugValue("a5");
            List<Obj> ol5 = vms.mirror.GetArrayElements(o5);
            string type5 = vms.mirror.GetType(ol5[0]);
            Assert.IsTrue(type5 == "int");
            Assert.IsTrue((Int64)ol5[0].Payload == 1);
            Assert.IsTrue((Int64)ol5[1].Payload == 2);
            fsr.Step();
            Obj o6 = vms.mirror.GetDebugValue("a6");
            List<Obj> ol6 = vms.mirror.GetArrayElements(o6);
            string type6 = vms.mirror.GetType(ol6[0]);
            Assert.IsTrue(type6 == "double");
            Assert.IsTrue((Double)ol6[0].Payload == 2.0);
            Assert.IsTrue((Double)ol6[1].Payload == 2.0);
            fsr.Step();
            fsr.Step();
            fsr.Step();
            fsr.Step();
            Obj o7 = vms.mirror.GetDebugValue("a7");

            string type7 = vms.mirror.GetType(o7);
            Assert.IsTrue(type7 == "B");
            Dictionary<string, Obj> os7 = vms.mirror.GetProperties(o7);
            Assert.IsTrue((Int64)os7["x"].Payload == 1);

            fsr.Step();
            fsr.Step();
            fsr.Step();
            fsr.Step();
            fsr.Step();
            fsr.Step();
            fsr.Step();
            fsr.Step();
            Obj o8 = vms.mirror.GetDebugValue("a8");
            string type8 = vms.mirror.GetType(o8);
            Assert.IsTrue(type8 == "int");
            Assert.IsTrue((Int64)o8.Payload == 1);


            vms = fsr.Step();
            Obj o9 = vms.mirror.GetDebugValue("a");
            string type9 = vms.mirror.GetType(o9);
            Assert.IsTrue(type9 == "int");
            Assert.IsTrue((Int64)o9.Payload == 1);
        }

        [Test]
        [Category("Debugger")]
        public void HighlightingFunctionsInArrayAssociative1_Defect_IDE_578()
        {
            string src =
        @"
def f(a : int)
{
    return = a;
}

arr = { f(99), f(87) };
b = 2;";
            fsr.PreStart(src, runnerConfig);
            DebugRunner.VMState vms = fsr.Step();

            Assert.AreEqual(7, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(7, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(24, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver();

            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"arr[0]");
            Obj o1 = mirror.GetWatchValue();
            Assert.AreEqual(99, (Int64)o1.Payload);

            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"arr[1]");
            o1 = mirror.GetWatchValue();
            Assert.AreEqual(87, (Int64)o1.Payload);

            Assert.AreEqual(8, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(8, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(7, vms.ExecutionCursor.EndExclusive.CharNo);
        }

        [Test]
        [Category("Debugger")]
        public void HighlightingFunctionsInArrayAssociative2_Defect_IDE_578()
        {
            string src =
        @"class Dummy
{
    value : var;
    constructor Dummy()
    {
        value = 5;
    }
}

def GetValue(d : Dummy)
{
    return = d.value;
}

arr = {Dummy.Dummy(), Dummy.Dummy(), Dummy.Dummy(), Dummy.Dummy()};
val = GetValue(arr);";

            fsr.PreStart(src, runnerConfig);
            DebugRunner.VMState vms = fsr.Step();

            Assert.AreEqual(15, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(15, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(68, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver();

            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"arr[0].value");
            Obj o1 = mirror.GetWatchValue();
            Assert.AreEqual(5, (Int64)o1.Payload);

            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"arr[1].value");
            o1 = mirror.GetWatchValue();
            Assert.AreEqual(5, (Int64)o1.Payload);

            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"arr[2].value");
            o1 = mirror.GetWatchValue();
            Assert.AreEqual(5, (Int64)o1.Payload);

            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"arr[3].value");
            o1 = mirror.GetWatchValue();
            Assert.AreEqual(5, (Int64)o1.Payload);
        }

        [Test]
        [Category("Debugger")]
        public void HighlightingFunctionsInArrayImperative_Defect_IDE_578()
        {
            string src =
        @"
def f(a : int)
{
    return = a;
}

[Imperative]
{
    arr = { f(99), f(87) };
    b = 2;
}";
            fsr.PreStart(src, runnerConfig);
            DebugRunner.VMState vms = fsr.Step();

            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(9, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(28, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver();

            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"arr[0]");
            Obj o1 = mirror.GetWatchValue();
            Assert.AreEqual(99, (Int64)o1.Payload);

            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"arr[1]");
            o1 = mirror.GetWatchValue();
            Assert.AreEqual(87, (Int64)o1.Payload);

            Assert.AreEqual(10, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(10, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(11, vms.ExecutionCursor.EndExclusive.CharNo);
        }

        [Test]
        [Category("Debugger")]
        public void Defect_IDE_602()
        {
            string src =
    @"class A
{ a : var; }

class B
{
   constructor create()
    {
        a1 = A.A();
    }
}

b = B.create();
f = 3;";
            fsr.PreStart(src, runnerConfig);
            DebugRunner.VMState vms = fsr.Step();

            Assert.AreEqual(12, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(12, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(16, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();

            Assert.AreEqual(8, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(8, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(20, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver();

            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(9, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(6, vms.ExecutionCursor.EndExclusive.CharNo);
        }

        [Test]
        [Category("Debugger")]
        public void Defect_IDE_619_1()
        {
            string src =
                @"x = 33;

def foo(y : int)
{
    return = y + 222;
}

a = x < foo(22) ? 3 : 55;
";

            fsr.PreStart(src, runnerConfig);
            DebugRunner.VMState vms = fsr.Step();

            // highlights "x = 33;"
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(1, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(8, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();

            // highlights "a = x < foo(22) ? 3 : 55;"
            Assert.AreEqual(8, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(8, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(26, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();

            // highlights "return = y + 222;"
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(5, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(22, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();

            // highlights "}"
            Assert.AreEqual(6, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(6, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();

            // highlights "a = x < foo(22) ? 3 : 55;"
            Assert.AreEqual(8, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(8, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(26, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();

            // close
            Assert.AreEqual(0, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(0, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(0, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(0, vms.ExecutionCursor.EndExclusive.CharNo);
        }

        [Test]
        [Category("Debugger")]
        public void Defect_IDE_619_2()
        {
            string src =
                @"class test
{
    def foo(y : int[])
    {
        return = y;
    }
}
x = { };
y = test.test();
a = y.foo({ 0, 1 });
x[y.foo({ 0, 1 })] = 3;
z = x;
";
            fsr.PreStart(src, runnerConfig);
            DebugRunner.VMState vms = fsr.Step();

            //"x = { };"
            Assert.AreEqual(8, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(8, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();

            //"y = test.test();"
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(9, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(17, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();
            vms = fsr.Step();
            //Currently two stepping in are required to move to the next line. 
            //The 1st step in checks for constructor defination and the 
            //2nd step takes debugger to the next line.

            //"a = y.foo({ 0, 1 });"
            Assert.AreEqual(10, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(10, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(21, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();

            //"return = y;"
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(5, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(20, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();

            //"}"
            Assert.AreEqual(6, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(6, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(6, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();

            //"a = y.foo({ 0, 1 });"
            Assert.AreEqual(10, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(10, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(21, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();

            //"x[y.foo({ 0, 1 })] = 3;"
            Assert.AreEqual(11, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(11, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(24, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();

            //"return = y;"
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(5, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(20, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();

            //"}"
            Assert.AreEqual(6, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(6, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(6, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();

            //"x[y.foo({ 0, 1 })] = 3;"
            Assert.AreEqual(11, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(11, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(24, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();

            //"z = x;"
            Assert.AreEqual(12, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(12, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(7, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();

            //Stop
            Assert.AreEqual(0, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(0, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(0, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(0, vms.ExecutionCursor.EndExclusive.CharNo);
        }

        [Test]
        [Category("Debugger")]
        public void Defect_IDE_619_3()
        {
            string src =
                @"class test
{
    def foo()
    {
        return = 0;
    }
}
x = { 1, 2 };
y = test.test();
x[y.foo()] = 3;
a = x;
";
            fsr.PreStart(src, runnerConfig);
            DebugRunner.VMState vms = fsr.Step();

            //"x = { 1, 2 };"
            Assert.AreEqual(8, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(8, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(14, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();

            //"y = test.test();"
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(9, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(17, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();
            vms = fsr.Step();
            //Currently two stepping in are required to move to the next line. 
            //The 1st step in checks for constructor defination and the 
            //2nd step takes debugger to the next line.

            //"x[y.foo()] = 3;"
            Assert.AreEqual(10, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(10, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(16, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();

            //"return = 0;"
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(5, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(20, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();

            //"}"
            Assert.AreEqual(6, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(6, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(6, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();

            //"x[y.foo()] = 3;"
            Assert.AreEqual(10, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(10, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(16, vms.ExecutionCursor.EndExclusive.CharNo);


            vms = fsr.Step();

            //"a = x;"
            Assert.AreEqual(11, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(11, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(7, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();

            //Stop
            Assert.AreEqual(0, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(0, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(0, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(0, vms.ExecutionCursor.EndExclusive.CharNo);
        }

        [Test]
        [Category("Debugger")]
        public void SteppingInFunctionCalls()
        {
            string src =
    @"class A
{
    x : double;
    constructor A(a : double)
    {
        x = a;
    }
    
    def f(a : double)
    {
        return = a;
    }
}

def f(a : double)
{
    return = a;
}

def g()
{
    return = 8.956;
}

p = A.A(f(g()));
a = p.f(g());
b = 2;";
            fsr.PreStart(src, runnerConfig);
            DebugRunner.VMState vms = fsr.Step();

            // highlights "p = A.A(f(g()));"
            Assert.AreEqual(25, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(25, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(17, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();

            // highlights "return = 8.956;" in g()
            Assert.AreEqual(22, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(22, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(20, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();

            // highlights closing bracket in g()
            Assert.AreEqual(23, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(23, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();

            // highlights "p = A.A(f(g()));" again
            Assert.AreEqual(25, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(25, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(17, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();

            // highlights "return = a;" in f()
            Assert.AreEqual(17, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(17, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(16, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();

            // highlights closing bracket in f()
            Assert.AreEqual(18, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(18, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();

            // highlights "p = A.A(f(g()));" yet again
            Assert.AreEqual(25, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(25, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(17, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();

            // highlights "x : double;" in A.A()
            Assert.AreEqual(3, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(3, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(16, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();

            // highlights "x = a;" in A.A()
            Assert.AreEqual(6, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(6, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(15, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();

            // highlights closing bracket in A.A()
            Assert.AreEqual(7, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(7, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(6, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();

            // highlights "p = A.A(f(g()));" yet again
            Assert.AreEqual(25, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(25, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(17, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();

            // highlights "a = p.f(g());" 
            Assert.AreEqual(26, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(26, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(14, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();

            // highlights "return = 8.956;" in g()
            Assert.AreEqual(22, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(22, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(20, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();

            // highlights closing bracket in g()
            Assert.AreEqual(23, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(23, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();

            // highlights "a = p.f(g());" again
            Assert.AreEqual(26, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(26, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(14, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();

            // highlights "return = a;" in f() in class A
            Assert.AreEqual(11, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(11, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(20, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();

            // highlights closing bracket in f()
            Assert.AreEqual(12, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(12, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(6, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();

            // highlights "a = p.f(g());" yet again
            Assert.AreEqual(26, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(26, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(14, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();

            // highlights "b = 2" 
            Assert.AreEqual(27, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(27, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(7, vms.ExecutionCursor.EndExclusive.CharNo);
        }

        [Test]
        [Category("Debugger")]
        public void SteppingOverFunctionCalls()
        {
            string src =
    @"class A
{
    x : double;
    constructor A(a : double)
    {
        x = a;
    }
    
    def f(a : double)
    {
        return = a;
    }
}

def f(a : double)
{
    return = a;
}

def g()
{
    return = 8.956;
}

p = A.A(f(g()));
a = p.f(g());
b = 2;";
            fsr.PreStart(src, runnerConfig);
            DebugRunner.VMState vms = fsr.Step();

            // highlights "p = A.A(f(g()));"
            Assert.AreEqual(25, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(25, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(17, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver();

            // highlights "a = p.f(g());" 
            Assert.AreEqual(26, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(26, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(14, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver();

            // highlights "b = 2" 
            Assert.AreEqual(27, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(27, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(7, vms.ExecutionCursor.EndExclusive.CharNo);
        }
        [Test]
        [Category("Debugger")]
        public void SteppingOverinline_Imperative_723()
        {
            string src =
            @"x = 330;

            def foo(y : int)
            {
                return = y + 222;
            }
          
            [Imperative]
            {
                a = x > foo(20) ? 44 : foo(55); //Line 10
                b = 2;
            }";
            fsr.PreStart(src, runnerConfig);
            DebugRunner.VMState vms = fsr.Step();

            vms = fsr.StepOver();
            vms = fsr.StepOver();

            {

                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();
                //TestFrameWork.Verify(mirror, "a", 44, 1);
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(44, (Int64)objExecVal.Payload);

            }
            vms = fsr.StepOver();
            {
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"b");
                Obj objExecVal = mirror.GetWatchValue();
                //TestFrameWork.Verify(mirror, "b", 2, 1);
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(2, (Int64)objExecVal.Payload);
            }

        }
        [Test]
        [Category("Debugger")]
        public void SteppingOverinline_Imperative_723_2()
        {
            string src =
            @"x = 30;

            def foo(y : int)
            {
                return = y + 222;
            }
          
            [Imperative]
            {
                a = x > foo(20) ? 44 : foo(55); //Line 10
                b = 2;
            }";
            fsr.PreStart(src, runnerConfig);
            DebugRunner.VMState vms = fsr.Step();

            vms = fsr.StepOver();
            vms = fsr.StepOver();

            {

                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();
                TestFrameWork.Verify(mirror, "a", 277, 1);

            }
            vms = fsr.StepOver();
            {
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"b");
                Obj objExecVal = mirror.GetWatchValue();
                TestFrameWork.Verify(mirror, "b", 2, 1);
            }

        }
        [Test]
        [Category("Debugger")]
        public void SteppingOverinline_Imperative_723_3()
        {
            string src =
            @"x = 330;

              def foo(y : int)
              {
                  return = y + 222;
              }
              a;
              [Imperative]
              {
                    a = x > 20 ? foo(44) : foo(55); //Line 10
                    b = 2;
              }";
            fsr.PreStart(src, runnerConfig);
            DebugRunner.VMState vms = fsr.Step();

            vms = fsr.StepOver();
            vms = fsr.StepOver();

            {

                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();
                //TestFrameWork.Verify(mirror, "a", 266, 1);
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(266, (Int64)objExecVal.Payload);

            }
            vms = fsr.StepOver();
            {
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"b");
                Obj objExecVal = mirror.GetWatchValue();
                //TestFrameWork.Verify(mirror, "b", 2, 1);
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(2, (Int64)objExecVal.Payload);
            }

        }
        [Test]
        [Category("Debugger")]
        public void SteppingOverinline_Imperative_723_4()
        {
            string src =
            @"x = 10;

              def foo(y : int)
              {
                  return = y + 222;
              }
              a;
              [Imperative]
              {
                    a = x > 20 ? foo(44) : foo(55); //Line 10
                    b = 2;
              }";
            fsr.PreStart(src, runnerConfig);
            DebugRunner.VMState vms = fsr.Step();

            vms = fsr.StepOver();
            vms = fsr.StepOver();

            {

                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();
                //TestFrameWork.Verify(mirror, "a", 277, 1);
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(277, (Int64)objExecVal.Payload);

            }
            vms = fsr.StepOver();
            {
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"b");
                Obj objExecVal = mirror.GetWatchValue();
                //TestFrameWork.Verify(mirror, "b", 2, 1);
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(2, (Int64)objExecVal.Payload);
            }

        }
        [Test]
        [Category("Debugger")]
        public void SteppingOverinline_Imperative_723_5()
        {
            string src =
            @"import(""DSCoreNodes.dll"");
                x = 330;
                [Imperative]
                {
    
                    a = x > 1 ? Math.Cos(60) : Math.Cos(45);
                    b = 22;
                }";
            fsr.PreStart(src, runnerConfig);
            DebugRunner.VMState vms = fsr.Step();

            vms = fsr.StepOver();
            vms = fsr.StepOver();


            {

                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();
                TestFrameWork.Verify(mirror, "a", 0.5, 1);

            }
            vms = fsr.StepOver();
            {
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"b");
                Obj objExecVal = mirror.GetWatchValue();
                TestFrameWork.Verify(mirror, "b", 22, 1);
            }

        }

        [Test]
        [Category("Debugger")]
        public void SteppingOverinline_Imperative_723_6()
        {
            string src =
            @"import(""DSCoreNodes.dll"");
                x = -330;
                [Imperative]
                {
    
                    a = x > 1 ? Math.Cos(60) : Math.Cos(45);
                    b = 22;
                }";
            fsr.PreStart(src, runnerConfig);
            DebugRunner.VMState vms = fsr.Step();

            vms = fsr.StepOver();
            vms = fsr.StepOver();


            {

                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();
                TestFrameWork.Verify(mirror, "a", 0.707106, 1);

            }
            vms = fsr.StepOver();
            {
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"b");
                Obj objExecVal = mirror.GetWatchValue();
                TestFrameWork.Verify(mirror, "b", 22, 1);
            }

        }

        [Test]
        [Category("Debugger")]
        public void TestStepInSimpleFunction()
        {
            string src =
              @"def foo : int(a : int)
                {
                    return = a;
                }
                c1 = foo(10);";

            fsr.PreStart(src, runnerConfig);
            DebugRunner.VMState vms = fsr.Step();   // c1 = foo(10);

            vms = fsr.Step();  // return = a;

            Assert.AreEqual(3, vms.ExecutionCursor.StartInclusive.LineNo);
            {
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(10, (Int64)objExecVal.Payload);

            }
        }

       

        [Test]
        [Category("Debugger")]
        public void StepIn_inlineconditional_Imperative_722()
        {
            string src =
            @"def foo : int(a : int, b : int)
                {
                    return = x = a > b ? a : b;
                }
                c1 = foo(10, 3);
                Print(c1);";
            fsr.PreStart(src, runnerConfig);
            DebugRunner.VMState vms = fsr.Step();   // c1 = foo(10, 3);

            vms = fsr.Step();  // return = x = a > b ? a : b;



            Assert.AreEqual(3, vms.ExecutionCursor.StartInclusive.LineNo);
            {

                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();
                //TestFrameWork.Verify(mirror, "a", 10, 0);
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(10, (Int64)objExecVal.Payload);

            }

            Assert.AreEqual(3, vms.ExecutionCursor.StartInclusive.LineNo);
            {
                ExpressionInterpreterRunner watchRunner2 = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror2 = watchRunner2.Execute(@"b");
                Obj objExecVal2 = mirror2.GetWatchValue();
                //TestFrameWork.Verify(mirror2, "b", 3, 0);
                Assert.AreNotEqual(null, objExecVal2);
                Assert.AreEqual(3, (Int64)objExecVal2.Payload);
            }

            vms = fsr.Step();
            vms = fsr.Step();
            Assert.AreEqual(4, vms.ExecutionCursor.StartInclusive.LineNo);
            {
                ExpressionInterpreterRunner watchRunner3 = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror3 = watchRunner3.Execute(@"x");
                Obj objExecVal3 = mirror3.GetWatchValue();
                //TestFrameWork.Verify(mirror3, "x", 10, 0);
                Assert.AreNotEqual(null, objExecVal3);
                Assert.AreEqual(10, (Int64)objExecVal3.Payload);
            }

            vms = fsr.Step();
            vms = fsr.Step();
            Assert.AreEqual(6, vms.ExecutionCursor.StartInclusive.LineNo);
            {
                ExpressionInterpreterRunner watchRunner4 = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror4 = watchRunner4.Execute(@"c1");
                Obj objExecVal4 = mirror4.GetWatchValue();
                //TestFrameWork.Verify(mirror4, "c1", 10, 0);
                Assert.AreNotEqual(null, objExecVal4);
                Assert.AreEqual(10, (Int64)objExecVal4.Payload);
            }

        }
        [Test]
        [Category("Debugger")]
        public void StepIn_inlineconditional_Imperative_722_2()
        {
            string src =
            @"def foo : int(a : int, b : int)
                {
                    return = a > b ? a : b;
                }

                c1 = foo(10, 3);
                Print(c1);";
            fsr.PreStart(src, runnerConfig);
            DebugRunner.VMState vms = fsr.Step();

            vms = fsr.Step();



            Assert.AreEqual(3, vms.ExecutionCursor.StartInclusive.LineNo);
            {

                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();
                //TestFrameWork.Verify(mirror, "a", 10, 0);
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(10, (Int64)objExecVal.Payload);

            }

            Assert.AreEqual(3, vms.ExecutionCursor.StartInclusive.LineNo);
            {
                ExpressionInterpreterRunner watchRunner2 = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror2 = watchRunner2.Execute(@"b");
                Obj objExecVal2 = mirror2.GetWatchValue();
                //     TestFrameWork.Verify(mirror2, "b", 3, 0);
                Assert.AreNotEqual(null, objExecVal2);
                Assert.AreEqual(3, (Int64)objExecVal2.Payload);
            }



            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();

            {
                ExpressionInterpreterRunner watchRunner4 = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror4 = watchRunner4.Execute(@"c1");
                Obj objExecVal4 = mirror4.GetWatchValue();
                TestFrameWork.Verify(mirror4, "c1", 10, 0);
            }

        }

        [Test]
        [Category("Debugger")]
        public void StepIn_inlineconditional_Imperative_722_3()
        {
            string src =
            @"[Imperative]
                {
                    def foo : int(a : int, b : int)
                    {
                        return = x = a > b ? a : b;
                    }
                    c1 = foo(10, 3);
    
                    Print(c1);
                }";
            fsr.PreStart(src, runnerConfig);
            DebugRunner.VMState vms = fsr.Step();

            vms = fsr.Step();




            {

                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();
                //TestFrameWork.Verify(mirror, "a", 10, 2);
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(10, (Int64)objExecVal.Payload);

            }


            {
                ExpressionInterpreterRunner watchRunner2 = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror2 = watchRunner2.Execute(@"b");
                Obj objExecVal2 = mirror2.GetWatchValue();
                //TestFrameWork.Verify(mirror2, "b", 3, 2);
                Assert.AreNotEqual(null, objExecVal2);
                Assert.AreEqual(3, (Int64)objExecVal2.Payload);
            }

            vms = fsr.Step();
            vms = fsr.Step();
            {
                ExpressionInterpreterRunner watchRunner3 = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror3 = watchRunner3.Execute(@"x");
                Obj objExecVal3 = mirror3.GetWatchValue();
                //TestFrameWork.Verify(mirror3, "x", 10, 2);
                Assert.AreNotEqual(null, objExecVal3);
                Assert.AreEqual(10, (Int64)objExecVal3.Payload);
            }


            vms = fsr.Step();
            vms = fsr.Step();

            {
                ExpressionInterpreterRunner watchRunner4 = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror4 = watchRunner4.Execute(@"c1");
                Obj objExecVal4 = mirror4.GetWatchValue();
                TestFrameWork.Verify(mirror4, "c1", 10, 1);
            }

        }
        [Test]
        [Category("Debugger")]
        public void cyclicdependancy_726()
        {
            string src =
            @"a = 1..3;
                c = a;

                b = [ Imperative ]
                {
                    count = 0;
                        for ( i in a )
                        {
                            if ( i > 0 )
                                {
                                    a[count] = i + 1;
                                }
                                count = count+1;
                        }
                        return = a;
                }

                d = [ Imperative ]
                {
                    count2 = 0;
                        while (count2 <= 2 ) 
                        {
                            if ( a[count2] > 0 )
                                {
                                    a[count2] = a[count2] + 1;
                                }
                                count2 = count2+1;
                        }
                        return = a;
                }

                e = b;";
            fsr.PreStart(src, runnerConfig);
            DebugRunner.VMState vms = fsr.Step();


            fsr.Run();
            Assert.AreEqual(fsr.isEnded, true);
            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"a");
            //TestFrameWork.Verify(mirror, "b", null, 0);
            TestFrameWork.VerifyRuntimeWarning(fsr.runtimeCore, ProtoCore.Runtime.WarningID.kCyclicDependency);

        }
        [Test]
        [Category("Debugger")]
        public void cyclicdependancy_726_2()
        {
            string src =
            @"a = 1;

                b = a;

                c = [ Imperative ]

                {

                    a = 2;

                        return = a;

                }

                d = [ Imperative ]

                {

                    a = 3;

                        return = a;

                }

                e = c;

                f = d;";
            fsr.PreStart(src, runnerConfig);
            DebugRunner.VMState vms = fsr.Step();
            fsr.Run();
            ProtoCore.RuntimeCore runtimeCore = fsr.runtimeCore;
            TestFrameWork.VerifyRuntimeWarning(runtimeCore, ProtoCore.Runtime.WarningID.kCyclicDependency);
        }
        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void TestWatchExpressionInNestedBlock2_519()
        {
            // Execute and verify the defect IDE-519
            fsr.PreStart(
@"[Imperative]
{
        i = 3;

[Associative]
{
    
  
    f = i;
}
    c = f;
} 
", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();//Line 5

            vms = fsr.StepOver();//Line 6

            {
                // Get the value of "i".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"i");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(3, (Int64)objExecVal.Payload);
            }

            vms = fsr.StepOver();//Line 7

            {
                // Get the value of "i".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"i");
                Obj objExecVal = mirror.GetWatchValue();
                // It should not be available.
                object o = vms.mirror.GetDebugValue("f");
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(3, (Int64)objExecVal.Payload);

            }
        }
        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void TestWatchExpressionInNestedBlock2_519_2()
        {
            // Execute and verify the defect IDE-519
            fsr.PreStart(
            @"class test
                {
                    f;
                    constructor test()
                    {
                    [Associative]
                        {
                            [Imperative]
                            {
                                i = 3;
                            }
                    // The value of 'i' cannot be inspected here.
                    // If this line is removed, then 'i' can be inspected.    
                            f = i;
                        }
                    }
                }

                a = test.test();
                b = a.f;

            ", runnerConfig);

            DebugRunner.VMState vms = fsr.StepOver();//Line 5

            vms = fsr.Step();//Line 6
            vms = fsr.Step();//Line 6

            {
                // Get the value of "i".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"i");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(3, (Int64)objExecVal.Payload);
            }
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            //Line 7
            {
                // Get the value of "i".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"b");
                Obj objExecVal = mirror.GetWatchValue();
                // It should not be available.

                Assert.AreEqual(null, objExecVal.Payload);

            }
        }
        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void Testmemberpropertyinwatch_476()
        {

            fsr.PreStart(
            @"class A
                {
                    a : var;
                    a2 : var;
                    a4 : var;
                    constructor A(x : var)
                    {
                        a = x;
                    }
                    def update(x : var)
                    {
                        a = { x => a1;
                        a1 > 10 ? true : false => a4;
                    }
                        return = x;
                    }
                }
                AA = A.A(0);
                AA1 = A.A();
                x = AA1.update(1);
                y = { AA1.a, AA1.a4 };

            ", runnerConfig);

            DebugRunner.VMState vms = fsr.StepOver();//Line 5

            vms = fsr.Step();//Line 6
            vms = fsr.Step();

            {
                // Get the value of "i".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(0, (Int64)objExecVal.Payload);
            }
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            //Line 7
            {
                // Get the value of "i".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"x");
                Obj objExecVal = mirror.GetWatchValue();
                // It should not be available.

                Assert.AreEqual(1, (Int64)objExecVal.Payload);

            }

        }
        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void Testprivatememberpropertyinwatch_476_2()
        {
            // Execute and verify the defect IDE-519
            fsr.PreStart(
            @"class A
                {
                    private a : var;
                    a2 : var;
                    a4 : var;
                    constructor A(x : var)
                    {
                        a = x;
                    }
                    def update(x : var)
                    {
                        a = { x => a1;
                        a1 > 10 ? true : false => a4;
                    }
                        return = x;
                    }
                }
                AA = A.A(0);
                AA1 = A.A();
                x = AA1.update(1);
                y = { AA1.a, AA1.a4 };

            ", runnerConfig);

            DebugRunner.VMState vms = fsr.StepOver();//Line 5

            vms = fsr.Step();//Line 6
            vms = fsr.Step();

            {
                // Get the value of "i".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(0, (Int64)objExecVal.Payload);
            }
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            //Line 7
            {
                // Get the value of "i".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"x");
                Obj objExecVal = mirror.GetWatchValue();
                // It should not be available.

                Assert.AreEqual(1, (Int64)objExecVal.Payload);

            }
        }
        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void Testprivatememberpropertyinwatch_476_3()
        {
            // Execute and verify the defect IDE-519
            fsr.PreStart(
            @"class A
                {
                    private a : var;
                    a2 : var;
                    a4 : var;
                    constructor A(x : var)
                    {
                        a = x;
                    }
                    def update(x : var)
                    {
                        a = { x => a1;
                        a1 > 10 ? true : false => a4;
                    }
                        return = x;
                    }
                }
                AA = A.A(0);
                AA1 = A.A();
                x = AA1.update(1);
                y = { AA1.a, AA1.a4 };

            ", runnerConfig);

            DebugRunner.VMState vms = fsr.StepOver();//Line 5

            vms = fsr.Step();//Line 6
            vms = fsr.Step();

            {
                // Get the value of "i".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreNotEqual(null, objExecVal);
                Assert.AreEqual(0, (Int64)objExecVal.Payload);
            }
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            //Line 7
            {
                // Get the value of "i".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"x");
                Obj objExecVal = mirror.GetWatchValue();
                // It should not be available.

                Assert.AreEqual(1, (Int64)objExecVal.Payload);

            }
        }
        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void Testprivatememberpropertyinwatch_487_1()
        {
            // Execute and verify the defect IDE-519
            fsr.PreStart(
            @"class test { a = 1; }

                c1 = [Imperative]
                {
                    a = test.test();
                    b = [Associative]
                    {
                        return = test.test();
                    }
    
                    c = a.a + b.a;
                    return = c;
                }

                c2 = [Associative]
                {
                    a = test.test();
                    b = [Imperative]
                    {
                        return = test.test();
                    }
    
                    c = a.a + b.a;
                    return = c;
                }

            ", runnerConfig);
            DebugRunner.VMState vms = fsr.Step();
            vms = fsr.Step();//Line 6
            vms = fsr.Step();
            vms = fsr.Step();

            {
                // Get the value of "i".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a.a");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreEqual(1, (Int64)objExecVal.Payload);
            }
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            {
                // Get the value of "i".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"c");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreEqual(2, (Int64)objExecVal.Payload);
            }
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();

            {
                // Get the value of "i".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a.a");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreEqual(1, (Int64)objExecVal.Payload);
            }
        }
        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void Testprivatememberpropertyinwatch_487_2()
        {
            // Execute and verify the defect IDE-519
            fsr.PreStart(
            @"class test { a = 1; }

                c1 = [Imperative]
                {
                    a = test.test();
                    b = [Associative]
                    {
                        return = test.test();
                    }
    
                    c = a.a + b.a;
                    return = c;
                }

                c2 = [Associative]
                {
                    a = test.test();
                    b = [Imperative]
                    {
                        return = test.test();
                    }
    
                    c = a.a + b.a;
                    return = c;
                }

            ", runnerConfig);
            DebugRunner.VMState vms = fsr.Step();
            vms = fsr.Step();//Line 6
            vms = fsr.Step();
            vms = fsr.Step();

            {
                // Get the value of "i".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a.a");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreEqual(1, (Int64)objExecVal.Payload);
            }
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            {
                // Get the value of "i".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"c");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreEqual(2, (Int64)objExecVal.Payload);
            }
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();

            {
                // Get the value of "i".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"a.a");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreEqual(1, (Int64)objExecVal.Payload);
            }
        }

        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void Testprivatememberpropertyinwatch_544()
        {
            // Execute and verify the defect IDE-519
            fsr.PreStart(
            @"def func(d : int)
                {
                    m : int;
                    m = 10;
                    temp = 0;
                    // Line #5    
                    [Imperative]
                    {
                        temp = m;
                        // Line #8  
                    }
                    return = temp; // Line #10
                }
                n = func(1);

            ", runnerConfig);
            DebugRunner.VMState vms = fsr.Step();
            vms = fsr.Step();//Line 6
            vms = fsr.Step();
            vms = fsr.Step();

            {
                // Get the value of "i".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"m");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreEqual(10, (Int64)objExecVal.Payload);
            }
            vms = fsr.Step();
            vms = fsr.Step();

            {
                // Get the value of "i".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"temp");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreEqual(10, (Int64)objExecVal.Payload);
            }

        }

        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void Testprivatememberpropertyinwatch_544_2()
        {
            // Execute and verify the defect IDE-519
            fsr.PreStart(
            @"def func(d : int)
                {
                    m : int;
                    m = 10;
                    temp = 0;
                    // Line #5    
                    [Associative]
                    {
                        temp = m;
                        // Line #8  
                    }
                    return = temp; // Line #10
                }
                n = func(1);

            ", runnerConfig);
            DebugRunner.VMState vms = fsr.Step();
            vms = fsr.Step();//Line 6
            vms = fsr.Step();
            vms = fsr.Step();

            {
                // Get the value of "i".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"m");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreEqual(10, (Int64)objExecVal.Payload);
            }
            vms = fsr.Step();
            vms = fsr.Step();

            {
                // Get the value of "i".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"temp");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreEqual(10, (Int64)objExecVal.Payload);
            }
        }

        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void Testprivatememberpropertyinwatch_544_3()
        {
            // related defect : DNL-1467497 Regression : When a variable is declared and then defined inside a class function, DS is now throwing CompilerInternalException

            // Execute and verify the defect IDE-519
            fsr.PreStart(
                 @"
               class test
            {
                def func(d : int)
                {
                                 m : int;
                                 m = 10;
                                 temp = 0;
                                 // Line #5    
                [Imperative]
                {
                    temp = m;
                    // Line #8  
                }
                return = temp; // Line #10
                }
            }
            n = test.test();
            n1 = n.func(1);
                    ", runnerConfig);
            DebugRunner.VMState vms = fsr.Step();
            vms = fsr.Step();//Line 6
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            //   vms = fsr.Step();
            {
                // Get the value of "i".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"m");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreEqual(10, (Int64)objExecVal.Payload);
            }
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            {
                // Get the value of "i".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"temp");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreEqual(10, (Int64)objExecVal.Payload);
            }

        }

        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void Testprivatememberpropertyinwatch_544_4()
        {
            // related defect : DNL-1467497 Regression : When a variable is declared and then defined inside a class function, DS is now throwing CompilerInternalException

            // Execute and verify the defect IDE-519
            fsr.PreStart(
                 @"
               class test
            {
                def func(d : int)
                {
                                 m : int;
                                 m = 10;
                                 temp = 0;
                                 // Line #5    
                [Associative]
                {
                    temp = m;
                    // Line #8  
                }
                return = temp; // Line #10
                }
            }
        n = test.test();
        n1 = n.func(1);
                    ", runnerConfig);
            DebugRunner.VMState vms = fsr.Step();
            vms = fsr.Step();//Line 6
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            //   vms = fsr.Step();
            {
                // Get the value of "i".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"m");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreEqual(10, (Int64)objExecVal.Payload);
            }
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            {
                // Get the value of "i".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"temp");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreEqual(10, (Int64)objExecVal.Payload);
            }

        }

        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void Testprivatememberpropertyinwatch_538()
        {
            // Execute and verify the defect IDE-519
            fsr.PreStart(
                 @"
              z=[Imperative]
                    {    
                    def GetNumberSquare(test:int) 
                    {      
                    result = test * test; 
                    return = result;  
                    } 
                    x = GetNumberSquare(5); 
                    return = x;
                    }
                    ", runnerConfig);
            DebugRunner.VMState vms = fsr.Step();
            vms = fsr.Step();//Line 6
            vms = fsr.Step();

            //   vms = fsr.Step();
            {
                // Get the value of "i".
                // This is to simulate the refresh of watch window as a result of "Step" button.
                ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
                ExecutionMirror mirror = watchRunner.Execute(@"result");
                Obj objExecVal = mirror.GetWatchValue();

                // It should not be available.
                Assert.AreEqual(25, (Int64)objExecVal.Payload);
            }
        }

        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void Testdotproperty_523()
        {
            // Execute and verify the defect IDE-519
            fsr.PreStart(
                 @"
        class A
        {
            x;
        }
        a = A.A();
        x = a.x;
        a.x = a;
        b = 33;
            ", runnerConfig);
            DebugRunner.VMState vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();


            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"b");
            Obj objExecVal = mirror.GetWatchValue();

            // It should not be available.
            Assert.AreEqual(33, (Int64)objExecVal.Payload);
        }
        [Test]
        [Category("ExpressionInterpreterRunner"), Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void Testdotproperty_523_2()
        {
            // Execute and verify the defect IDE-519
            fsr.PreStart(
                 @"
        import(""ProtoGeometry.dll"");
        origin = Point.ByCoordinates(0,0,0);
        testSolid = Sphere.ByCenterPointRadius(origin, 10.3);
        testPlaneX = Plane.ByOriginNormal(origin, Vector.ByCoordinates(1,0,0));
        intersectCurveX = testSolid.Intersect(testPlaneX);
        testPlaneY = Plane.ByOriginNormal(origin, Vector.ByCoordinates(0,1,0));
        //Returns 2 arcs, expect 1 circle 
        onlyintersectCurveY = testSolid.Intersect(testPlaneY);
        testPlaneZ = Plane.ByOriginNormal(origin, Vector.ByCoordinates(0,0,1)); //Returns 1 circle 
        nowintersectCurveZ = testSolid.Intersect(testPlaneZ);
            ", runnerConfig);
            DebugRunner.VMState vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();



            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"testSolid.Radius");

            Obj objExecVal = mirror.GetWatchValue();
            Assert.AreNotEqual(null, objExecVal);
            // It should not be available.
            Assert.AreEqual(10.3, (Double)objExecVal.Payload);
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            Assert.AreNotEqual(null, objExecVal);
        }

        [Test]
        [Category("ExpressionInterpreterRunner"), Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void Testdotproperty_523_3()
        {
            // Execute and verify the defect IDE-519
            fsr.PreStart(
                 @"
        import(""ProtoGeometry.dll"");
        origin = Point.ByCoordinates(0,0,0);
        testSolid = Sphere.ByCenterPointRadius(origin, 10.3);
        testPlaneX = Plane.ByOriginNormal(origin, Vector.ByCoordinates(1,0,0));
        intersectCurveX = testSolid.Intersect(testPlaneX);
        def foo()
        {
            a = 4;
            b = 5;
            c = 6;
    
            testPlaneY = Plane.ByOriginNormal(origin,  Vector.ByCoordinates(0,1,0));
        //Returns 2 arcs, expect 1 circle 
            onlyintersectCurveY = testSolid.Intersect(testPlaneY);
            testPlaneZ = Plane.ByOriginNormal(origin,  Vector.ByCoordinates(0,0,1)); //Returns 1 circle 
            nowintersectCurveZ = testSolid.Intersect(testPlaneZ);
return = a;
        }

        z = foo();
            ", runnerConfig);
            DebugRunner.VMState vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();


            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"testSolid.Radius");
            Obj objExecVal = mirror.GetWatchValue();


            // It should not be available.
            Assert.AreEqual(10.3, (Double)objExecVal.Payload);
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror11 = watchRunner.Execute(@"testSolid.CenterPoint.X");
            Obj objExecVal11 = mirror11.GetWatchValue();


            // It should not be available.
            Assert.AreEqual(0, (Double)objExecVal11.Payload);
            vms = fsr.StepOver();
            vms = fsr.StepOver();

            vms = fsr.Step();
            vms = fsr.Step();

            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror2 = watchRunner.Execute(@"a");
            Obj objExecVal2 = mirror2.GetWatchValue();

            // It should not be available.
            Assert.AreEqual(4, (Int64)objExecVal2.Payload);
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror3 = watchRunner.Execute(@"z");
            Obj objExecVal3 = mirror3.GetWatchValue();


            // It should not be available.
            Assert.AreEqual(4, (Int64)objExecVal3.Payload);

        }

        [Test]
        [Category("ExpressionInterpreterRunner"), Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void Testdotproperty_523_4()
        {
            // Execute and verify the defect IDE-519
            fsr.PreStart(
                 @"
        import(""ProtoGeometry.dll"");
        [Imperative]
        {

            origin = Point.ByCoordinates(0,0,0);
            testSolid = Sphere.ByCenterPointRadius(origin, 10.3);
            testPlaneX = Plane.ByOriginNormal(origin, Vector.ByCoordinates(1,0,0));
            intersectCurveX = testSolid.Intersect(testPlaneX);
        
            def foo()
            {
                a = 4;
                b = 5;
                c = 6;
    
                testPlaneY = Plane.ByOriginNormal(origin, Vector.ByCoordinates(0,1,0));
                //Returns 2 arcs, expect 1 circle 
                onlyintersectCurveY = testSolid.Intersect(testPlaneY);
                testPlaneZ = Plane.ByOriginNormal(origin, Vector.ByCoordinates(0,0,1)); //Returns 1 circle 
                nowintersectCurveZ = testSolid.Intersect(testPlaneZ);
                return = a;
            }

            z = foo();
}

            ", runnerConfig);
            DebugRunner.VMState vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();

            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"testSolid.Radius");
            Obj objExecVal = mirror.GetWatchValue();


            // It should not be available.
            Assert.AreEqual(10.3, (Double)objExecVal.Payload);
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror11 = watchRunner.Execute(@"testSolid.CenterPoint.X");

            Obj objExecVal11 = mirror11.GetWatchValue();


            // It should not be available.
            Assert.AreEqual(0, (Double)objExecVal11.Payload);
            vms = fsr.StepOver();
            vms = fsr.StepOver();

            vms = fsr.Step();
            vms = fsr.Step();

            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror2 = watchRunner.Execute(@"a");
            Obj objExecVal2 = mirror2.GetWatchValue();
            // It should not be available.
            Assert.AreEqual(4, (Int64)objExecVal2.Payload);
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror3 = watchRunner.Execute(@"z");
            Obj objExecVal3 = mirror3.GetWatchValue();

            // It should not be available.
            Assert.AreEqual(4, (Int64)objExecVal3.Payload);

        }
        [Test]
        [Category("ExpressionInterpreterRunner"), Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void Testdotproperty_523_5()
        {
            // Execute and verify the defect IDE-519
            fsr.PreStart(
                 @"
        import(""ProtoGeometry.dll"");
        [Imperative]
        {

            origin = Point.ByCoordinates(0,0,0);
            testSolid = Sphere.ByCenterPointRadius(origin, 10.3);
            testPlaneX = Plane.ByOriginNormal(origin, Vector.ByCoordintes(1,0,0));
            intersectCurveX = testSolid.Intersect(testPlaneX);
        
            def foo()
            {
                a = 4;
                b = 5;
                c = 6;
    
                testPlaneY = Plane.ByOriginNormal(origin, Vector.ByCoordintes(0,1,0));
                //Returns 2 arcs, expect 1 circle 
                onlyintersectCurveY = testSolid.Intersect(testPlaneY);
                testPlaneZ = Plane.ByOriginNormal(origin, Vector.ByCoordintes(0,0,1)); //Returns 1 circle 
                nowintersectCurveZ = testSolid.Intersect(testPlaneZ);
                return = a;
            }

            z = foo();
}

            ", runnerConfig);
            DebugRunner.VMState vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();

            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"testSolid.Radius");
            Obj objExecVal = mirror.GetWatchValue();


            // It should not be available.
            Assert.AreEqual(10.3, (Double)objExecVal.Payload);
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror11 = watchRunner.Execute(@"testSolid.CenterPoint.X");

            Obj objExecVal11 = mirror11.GetWatchValue();


            // It should not be available.
            Assert.AreEqual(0, (Double)objExecVal11.Payload);
            vms = fsr.StepOver();
            vms = fsr.StepOver();

            vms = fsr.Step();
            vms = fsr.Step();

            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror2 = watchRunner.Execute(@"a");
            Obj objExecVal2 = mirror2.GetWatchValue();
            // It should not be available.
            Assert.AreEqual(4, (Int64)objExecVal2.Payload);
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror3 = watchRunner.Execute(@"z");
            Obj objExecVal3 = mirror3.GetWatchValue();

            // It should not be available.
            Assert.AreEqual(4, (Int64)objExecVal3.Payload);

        }
        [Test]
        [Category("ExpressionInterpreterRunner"), Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void Testdotproperty_607()
        {
            // related defect : DNL-1467498 Regression in geometry due to Cartesian dot operation implementation : Runtime warning : Can't locate Geometry constructor..

            // Execute and verify the defect IDE-519
            fsr.PreStart(
                 @"
       import(""ProtoGeometry.dll"");
class test
{

            def foo()
            {
                
                WCS = CoordinateSystem.Identity();
                testSolid       = Sphere.ByCenterPointRadius(WCS.Origin, 10.3);
                testPlaneX      = Plane.ByOriginNormal(WCS.Origin, WCS.XAxis, 40);
                intersectCurveX = testSolid.Intersect(testPlaneX);                        
                testPlaneY = Plane.ByOriginNormal(WCS.Origin, WCS.YAxis, 40);
                a = 4;
                b = 5;
                c = 6;
                //Returns 2 arcs, expect 1 circle 

                return = a;
            }
}

[Imperative]
{
     y = test.test();
     z = y.foo();
}

            ", runnerConfig);
            DebugRunner.VMState vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.Step();
            vms = fsr.StepOver();
            vms = fsr.StepOver();

            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"testSolid.Radius");
            Obj objExecVal = mirror.GetWatchValue();
            //  Assert.AreNotEqual(null, objExecVal);
            // It should not be available.
            Assert.AreEqual(10.3, (Double)objExecVal.Payload);
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            ExpressionInterpreterRunner watchRunner2 = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror2 = watchRunner2.Execute(@"a");
            Obj objExecVal2 = mirror.GetWatchValue();
            //  Assert.AreNotEqual(null, objExecVal);
            // It should not be available.
            Assert.AreEqual(4, (Int64)objExecVal2.Payload);
        }

        [Test]
        [Category("Debugger")]
        [Category("Failure")]
        public void Defect_IDE_607()
        {
            // Execute and verify the defect IDE-519
            fsr.PreStart(
                 @"class Point
{
    x : int;
    y : int;
    z : int;
    
    constructor Point(_x : int, _y : int, _z : int)
    {
        x = _x;
        y = _y;
        z = _z;
    }
}

p = Point.Point(3, 4, 5);", runnerConfig);
            DebugRunner.VMState vms = fsr.Step();
            // "p = Point.Point(3, 4, 5);"
            Assert.AreEqual(15, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(15, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(26, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();
            // "x : int;"
            Assert.AreEqual(3, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(3, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(13, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver();
            // "y : int;"
            Assert.AreEqual(4, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(4, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(13, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver();
            // "z : int;"
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(5, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(13, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver();
            // "x = _x;"
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(9, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(16, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver();
            // "y = _y;"
            Assert.AreEqual(10, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(10, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(16, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver();
            // "z = _z;"
            Assert.AreEqual(11, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(11, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(16, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver();
            // closing bracket of Point()
            Assert.AreEqual(12, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(12, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(6, vms.ExecutionCursor.EndExclusive.CharNo);
        }

        [Test]
        [Category("Debugger")]
        public void Defect_IDE_543_1()
        {
            fsr.PreStart(
                 @"[Imperative]
{
    a = 1..8..1;
}", runnerConfig);
            DebugRunner.VMState vms = fsr.Step();
            Assert.AreEqual(3, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(3, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(17, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();
            Assert.AreEqual(4, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(4, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.EndExclusive.CharNo);
        }

        [Test]
        [Category("Debugger")]
        public void Defect_IDE_543_2()
        {
            fsr.PreStart(
                 @"[Imperative]
{
    a = 1..8..1;
}", runnerConfig);
            DebugRunner.VMState vms = fsr.Step();
            Assert.AreEqual(3, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(3, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(17, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver();
            Assert.AreEqual(4, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(4, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.EndExclusive.CharNo);
        }

        [Test]
        [Category("Debugger")]
        public void Defect_IDE_543_3()
        {
            fsr.PreStart(
                 @"[Imperative]
{
    a = 0.5..0.25..-0.25;
}", runnerConfig);
            DebugRunner.VMState vms = fsr.Step();
            Assert.AreEqual(3, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(3, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(26, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();
            Assert.AreEqual(4, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(4, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.EndExclusive.CharNo);
        }

        [Test]
        [Category("Debugger")]
        public void Defect_IDE_543_4()
        {
            fsr.PreStart(
                 @"[Imperative]
{
    a = 0.5..0.25..-0.25;
}", runnerConfig);
            DebugRunner.VMState vms = fsr.Step();
            Assert.AreEqual(3, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(3, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(26, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver();
            Assert.AreEqual(4, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(4, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.EndExclusive.CharNo);
        }

        [Test]
        [Category("Debugger")]
        public void Defect_IDE_543_5()
        {
            fsr.PreStart(
                 @"[Associative]
{
    a = 1..8..1;
}", runnerConfig);
            DebugRunner.VMState vms = fsr.Step();
            Assert.AreEqual(3, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(3, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(17, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();
            Assert.AreEqual(4, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(4, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.EndExclusive.CharNo);
        }

        [Test]
        [Category("Debugger")]
        public void Defect_IDE_543_6()
        {
            fsr.PreStart(
                 @"[Associative]
{
    a = 1..8..1;
}", runnerConfig);
            DebugRunner.VMState vms = fsr.Step();
            Assert.AreEqual(3, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(3, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(17, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver();
            Assert.AreEqual(4, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(4, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.EndExclusive.CharNo);
        }

        [Test]
        [Category("Debugger")]
        public void Defect_IDE_543_7()
        {
            fsr.PreStart(
                 @"[Associative]
{
    a = 0.5..0.25..-0.25;
}", runnerConfig);
            DebugRunner.VMState vms = fsr.Step();
            Assert.AreEqual(3, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(3, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(26, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();
            Assert.AreEqual(4, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(4, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.EndExclusive.CharNo);
        }

        [Test]
        [Category("Debugger")]
        public void Defect_IDE_543_8()
        {
            fsr.PreStart(
                 @"[Associative]
{
    a = 0.5..0.25..-0.25;
}", runnerConfig);
            DebugRunner.VMState vms = fsr.Step();
            Assert.AreEqual(3, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(3, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(26, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver();
            Assert.AreEqual(4, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(4, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.EndExclusive.CharNo);
        }

        [Test]
        [Category("Debugger"), Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void Defect_IDE_653_1()
        {
            fsr.PreStart(
                 @"import(""ProtoGeometry.dll"");
    
sphere = Sphere.ByCenterPointRadius(Point.ByCoordinates(0, 0, 0), 1);

sface = sphere.Faces[0];

surfaceGeom = sface.SurfaceGeometry.SetVisibility(true);", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();   // sphere = Sphere.ByCenterPointRadius(Point.ByCoordinates(0, 0, 0), 1);
            Assert.AreEqual(3, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(3, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(70, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver();   // sface = sphere.Faces[0];
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(5, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(25, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver();   // surfaceGeom = sface.SurfaceGeometry.SetVisibility(true);
            Assert.AreEqual(7, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(7, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(57, vms.ExecutionCursor.EndExclusive.CharNo);
        }

        [Test]
        [Category("Debugger"), Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void Defect_IDE_653_2()
        {
            fsr.PreStart(
                 @"import(""ProtoGeometry.dll"");
    
sphere = Sphere.ByCenterPointRadius(Point.ByCoordinates(0, 0, 0), 1);

surfaceGeom = sphere.Faces[0].SurfaceGeometry.SetVisibility(true);", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();   // sphere = Sphere.ByCenterPointRadius(Point.ByCoordinates(0, 0, 0), 1);
            Assert.AreEqual(3, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(3, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(70, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver();   // surfaceGeom = sphere.Faces[0].SurfaceGeometry.SetVisibility(true);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(5, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(67, vms.ExecutionCursor.EndExclusive.CharNo);
        }

        [Test]
        [Category("Debugger")]
        public void Defect_IDE_653_3()
        {
            fsr.PreStart(
                 @"class Surface
{
    x : int;
    constructor Surface(a : int)
    {
        x = a;
    }
    
    def SetVisibility(a : bool)
    {
        return = x;
    }
}

class Face
{
    SurfaceGeometry : var;
    
    constructor Face(a : int)
    {
        SurfaceGeometry = Surface(a);
    }
}

class Sphere
{
    Faces : var[];
    
    constructor ByCenterPointRadius()
    {
        Faces = { Face(1), Face(2), Face(4) };
    }
}

sphere = Sphere.ByCenterPointRadius();
surfaceGeom = sphere.Faces[0].SurfaceGeometry.SetVisibility(true);", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();   // sphere = Sphere.ByCenterPointRadius(Point.ByCoordinates(0, 0, 0), 1);
            Assert.AreEqual(35, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(35, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(39, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver();   // surfaceGeom = sphere.Faces[0].SurfaceGeometry.SetVisibility(true);
            Assert.AreEqual(36, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(36, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(67, vms.ExecutionCursor.EndExclusive.CharNo);
        }

        [Test]
        [Category("Debugger")]
        [Category("Failure")]
        public void Defect_IDE_656_1()
        {
            fsr.PreStart(
                @"b1 = 1;

class A
{
    x : int;
    
    constructor A()
    {
        x = 1;
    }
    
    def foo()
    {
        return = 90; //Line 14
    } //Line 15
}

def foo1()
{
    return = 10;
}

a =
{
    4 => a1;
    A.A() => a2;
    a2.foo();
    a1 > a2.foo() ? 0 : a2.foo() => a3; //Line 28

}
c = 90;", runnerConfig);

            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1568
            DebugRunner.VMState vms = fsr.Step();   // b1 = 1;

            vms = fsr.Step();   // 4 => a1;
            Assert.AreEqual(25, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(25, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(13, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();   // A.A() => a2;
            Assert.AreEqual(26, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(26, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(17, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver();   // a2.foo();
            Assert.AreEqual(27, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(27, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(14, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();   // return = 90;
            Assert.AreEqual(14, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(14, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(21, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();   // closing brace of A::foo()
            Assert.AreEqual(15, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(15, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(6, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();   // a2.foo();
            Assert.AreEqual(27, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(27, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(14, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();   // a1 > a2.foo() ? 0 : a2.foo() => a3;
            Assert.AreEqual(28, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(28, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(40, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();   // return = 90;
            Assert.AreEqual(14, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(14, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(21, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();   // closing brace of A::foo()
            Assert.AreEqual(15, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(15, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(6, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();   // a1 > a2.foo() ? 0 : a2.foo() => a3;
            Assert.AreEqual(28, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(28, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(40, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();   // return = 90;
            Assert.AreEqual(14, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(14, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(21, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();   // closing brace of A::foo()
            Assert.AreEqual(15, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(15, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(6, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();   // a1 > a2.foo() ? 0 : a2.foo() => a3;
            Assert.AreEqual(28, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(28, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(40, vms.ExecutionCursor.EndExclusive.CharNo);


            vms = fsr.Step();   // a = {}
            Assert.AreEqual(23, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(30, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.EndExclusive.CharNo);
        }

        [Test]
        [Category("Debugger")]
        [Category("Failure")]
        public void Defect_IDE_656_2()
        {
            fsr.PreStart(
                @"b1 = 1;

class A
{
    x : int;
    
    constructor A()
    {
        x = 1;
    }
    
    def foo()
    {
        return = 90; //Line 14
    } //Line 15
}

def foo1()
{
    return = 10;
}

a =
{
    4 => a1;
    A.A() => a2;
    a2.foo();
    a1 > a2.foo() ? 0 : a2.foo() => a3; //Line 28

}
c = 90;", runnerConfig);
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1568
            DebugRunner.VMState vms = fsr.Step();   // b1 = 1;

            vms = fsr.Step();   // 4 => a1;
            Assert.AreEqual(25, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(25, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(13, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();   // A.A() => a2;
            Assert.AreEqual(26, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(26, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(17, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver();   // a2.foo();
            Assert.AreEqual(27, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(27, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(14, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();   // return = 90;
            Assert.AreEqual(14, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(14, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(21, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();   // closing brace of A::foo()
            Assert.AreEqual(15, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(15, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(6, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();   // a2.foo();
            Assert.AreEqual(27, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(27, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(14, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();   // a1 > a2.foo() ? 0 : a2.foo() => a3;
            Assert.AreEqual(28, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(28, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(40, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver();   // a = {}
            Assert.AreEqual(23, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(30, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.EndExclusive.CharNo);
        }



        [Test]
        [Category("Debugger")]
        public void Defect_IDE_656_4_stepOver()
        {
            fsr.PreStart(
                @"c = { 1, 2, 20 };
def f(a)
{
    return = a;
}
x = f(c) > 5 ? 1 : 2;
b = 2;", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();   // c = { 1, 2, 20 };
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(1, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(18, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();   // x = f(c) > 5 ? 1 : 2;
            Assert.AreEqual(6, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(6, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(22, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver();   // b = 2;
            Assert.AreEqual(7, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(7, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(7, vms.ExecutionCursor.EndExclusive.CharNo);
        }

        [Test]
        [Category("Debugger")]
        [Category("Failure")]
        public void Defect_IDE_656_4_stepIn()
        {
            fsr.PreStart(
                @"c = { 1, 2, 20 };
def f(a)
{
    return = a;
}
x = f(c) > 5 ? 1 : 2;
b = 2;", runnerConfig);

            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3961
            Assert.Fail("Stepping In external functions and replicated functions requires two 'step in's to move to the next line");

            DebugRunner.VMState vms = fsr.Step();   // c = { 1, 2, 20 };
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(1, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(18, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();   // x = f(c) > 5 ? 1 : 2;
            Assert.AreEqual(6, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(6, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(22, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();   // b = 2;
            Assert.AreEqual(7, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(7, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(7, vms.ExecutionCursor.EndExclusive.CharNo);
        }


        [Test]
        [Category("Debugger")]
        public void Defect_IDE_721()
        {
            fsr.PreStart(
                @"x = {true, 0,{1},false,null,{false}};

m = 2.56;

a = {

CountFalse(x) => a1; //2

CountFalse(x[5]) => a2;//1

CountFalse(x[CountFalse(x)]) => a3;//0 Line-11

m => a4;

CountFalse({a4}) => a5;//0

}

result = {a1,a2,a3,a4,a5};", runnerConfig);

            //Assert.Fail("IDE-721 Only inner function call is highlighted inside the following modifier block");

            //x = {true, 0,{1},false,null,{false}};
            DebugRunner.VMState vms = fsr.Step();
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(1, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(38, vms.ExecutionCursor.EndExclusive.CharNo);

            //m = 2.56;
            vms = fsr.StepOver();
            Assert.AreEqual(3, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(3, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(10, vms.ExecutionCursor.EndExclusive.CharNo);

            //CountFalse(x) => a1;
            vms = fsr.StepOver();
            Assert.AreEqual(7, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(7, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(21, vms.ExecutionCursor.EndExclusive.CharNo);


            //CountFalse(x[5]) => a2;
            vms = fsr.StepOver();
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(9, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(24, vms.ExecutionCursor.EndExclusive.CharNo);


            //CountFalse(x[CountFalse(x)]) => a3;
            vms = fsr.StepOver();
            Assert.AreEqual(11, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(11, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(36, vms.ExecutionCursor.EndExclusive.CharNo);


            //m => a4;
            vms = fsr.StepOver();
            Assert.AreEqual(13, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(13, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.EndExclusive.CharNo);

            //CountFalse({a4}) => a5;
            vms = fsr.StepOver();
            Assert.AreEqual(15, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(15, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(24, vms.ExecutionCursor.EndExclusive.CharNo);

            /*a = {

CountFalse(x) => a1; //2

CountFalse(x[5]) => a2;//1

CountFalse(x[CountFalse(x)]) => a3;//0 Line-11

m => a4;

CountFalse({a4}) => a5;//0

}*/
            vms = fsr.StepOver();
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(17, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.EndExclusive.CharNo);

            //result = {a1,a2,a3,a4,a5};
            vms = fsr.StepOver();
            Assert.AreEqual(19, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(19, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(27, vms.ExecutionCursor.EndExclusive.CharNo);

        }

        [Test]
        [Category("Debugger")]
        [Category("Failure")]
        public void Defect_IDE_722()
        {
            fsr.PreStart(
                @"def foo : int(a : int, b : int)
{
    return = x = a > b ? a : b;
}
c1 = foo(10, 3);
Print(c1);", runnerConfig);

            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3962
            Assert.Fail("Cannot Step In to the return statement of a function if it contains a In Line Condition");

            DebugRunner.VMState vms = fsr.Step();   // c1 = foo(10, 3);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(5, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(17, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();   // return = x = a > b ? a : b;
            Assert.AreEqual(3, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(3, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(32, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();
            Assert.AreEqual(4, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(4, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();   // c1 = foo(10, 3);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(5, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(17, vms.ExecutionCursor.EndExclusive.CharNo);

        }

        [Test]
        [Category("Debugger")]
        [Category("Failure")]
        public void Defect_IDE_722_1()
        {
            fsr.PreStart(
                @"[Imperative]
{
    def foo : int(a : int, b : int)
    {
        return = x = a > b ? a : b;
    }
    c1 = foo(10, 3);
    
    Print(c1);
}", runnerConfig);

            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3962
            Assert.Fail("IDE-722 Cannot Step In to the return statement of a function if it contains a In Line Condition");

            DebugRunner.VMState vms = fsr.Step();   // c1 = foo(10, 3);
            Assert.AreEqual(7, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(7, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(21, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();   // return = x = a > b ? a : b;
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(5, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(36, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();

            vms = fsr.Step();   // c1 = foo(10, 3);
            Assert.AreEqual(7, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(7, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(21, vms.ExecutionCursor.EndExclusive.CharNo);
        }

        [Test]
        [Category("ExpressionInterpreterRunner")]

        public void watchinImperative_542()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
@"
class Sample
{
a : int = 1234;  
def foo()
{
return = a;
}
}
test = null;
[Imperative]
{
test = Sample.Sample();
b = test.a;
c = test.foo();
}
// Line #16// Comment out the following lines fixes the problem.
test = 1;
b = 2;
c = 3;
           
", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();

            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();


            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"b");
            Obj objExecVal = mirror.GetWatchValue();
            TestFrameWork.Verify(mirror, "b", 1234, 1);
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();

            TestFrameWork.Verify(mirror, "b", 1234, 1);
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            TestFrameWork.Verify(mirror, "b", 2, 0);



        }

        [Test]
        [Category("ExpressionInterpreterRunner")]

        public void watchinImperative_542_2()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
@"
class Sample
{
a : int = 1234;  
def foo()
{
return = a;
}
}
b=1;
test = null;
[Imperative]
{
test = Sample.Sample();
b = test.a;
c = test.foo();
}
// Line #16// Comment out the following lines fixes the problem.
test = 1;
b = 2;
c = 3;
           
", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();

            vms = fsr.StepOver();
            vms = fsr.StepOver();

            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"b");
            Obj objExecVal = mirror.GetWatchValue();
            TestFrameWork.Verify(mirror, "b", 1, 0);

            vms = fsr.StepOver();
            vms = fsr.StepOver();

            //TestFrameWork.Verify(mirror, "b", 1234, 1);

            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();

            // TestFrameWork.Verify(mirror, "b", 1234, 1);

            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();

            TestFrameWork.Verify(mirror, "b", 2, 0);

        }
        [Test]
        [Category("ExpressionInterpreterRunner")]

        public void watchinImperative_542_3()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
@"
class Sample
{
a : int = 1234;  
def foo()
{
return = a;
}
}
[Associative]
{
test = Sample.Sample();
b = test.a;
c = test.foo();
}
// Line #16// Comment out the following lines fixes the problem.
test = 1;
b = 2;
c = 3;
           
", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();

            vms = fsr.StepOver();

            vms = fsr.StepOver();


            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"b");
            Obj objExecVal = mirror.GetWatchValue();
            TestFrameWork.Verify(mirror, "b", 1234, 1);
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();

            TestFrameWork.Verify(mirror, "b", 2, 0);

        }
        [Test]
        [Category("ExpressionInterpreterRunner")]

        public void watchinImperative_542_4()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
@"
class Sample
{
a : int = 1234;  
def foo()
{
return = a;
}
}
[Associative]
{
test = Sample.Sample();
b = test.a;
c = test.foo();
}
// Line #16// Comment out the following lines fixes the problem.
test = 1;
b = 2;
c = 3;
           
", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();

            vms = fsr.StepOver();


            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"b");
            Obj objExecVal = mirror.GetWatchValue();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            TestFrameWork.Verify(mirror, "b", 1234, 1);
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();

            TestFrameWork.Verify(mirror, "b", 2, 0);

        }
        [Test]
        [Category("ExpressionInterpreterRunner")]

        public void watchinImperative_542_5()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
 @"
a;b;
[Associative]
{
    a = 0;
	d = a + 1;
    [Imperative]
    {
		b = 2 + a;
		a = 1.5;
		
    }
	c = 2;
}
           
", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();

            vms = fsr.StepOver();


            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"a");
            Obj objExecVal = mirror.GetWatchValue();

            TestFrameWork.Verify(mirror, "a", 0, 0);
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();

            TestFrameWork.Verify(mirror, "b", 2, 0);

        }

        [Test]
        [Category("ExpressionInterpreterRunner")]

        public void watchinImperative_nested_666()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
 @"
r = 0;

[Imperative]
{
    for(i in 0..1)
    {
        for(k in 0..10)
        {
            r = k;
        }
    }
}
           
", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();

            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();

            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"k");
            Obj objExecVal = mirror.GetWatchValue();

            TestFrameWork.Verify(mirror, "k", 0, 0);
            TestFrameWork.Verify(mirror, "r", 0, 0);
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            TestFrameWork.Verify(mirror, "k", 1, 0);
            TestFrameWork.Verify(mirror, "r", 1, 0);

        }
        [Test]
        [Category("ExpressionInterpreterRunner")]

        public void watchinImperative_nested_666_2()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
 @"
r = 0;

[Imperative]
{

    for(i in 0..1)
    {
        for(j in 0..10)
        {
            for(k in 0..10)
            {
                r = k;
            }
        }
    }
}
r = 0;
           
", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();

            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();

            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"k");
            Obj objExecVal = mirror.GetWatchValue();

            TestFrameWork.Verify(mirror, "k", 0, 0);
            TestFrameWork.Verify(mirror, "r", 0, 0);
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            TestFrameWork.Verify(mirror, "k", 1, 0);
            TestFrameWork.Verify(mirror, "r", 1, 0);

        }
        [Test]
        [Category("ExpressionInterpreterRunner")]

        public void watchinImperative_nested_666_3()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
 @"
r = 0;

 def   foo()
    {
        [Imperative]
        {
            for(i in 0..1)
            {
                for(j in 0..10)
                {
                    for(k in 0..10)
                    {
                        r = k;
                    }
                }
            }
            }
    }

        a = foo();
           
", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();

            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();

            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"k");
            Obj objExecVal = mirror.GetWatchValue();

            TestFrameWork.Verify(mirror, "k", 0, 0);
            TestFrameWork.Verify(mirror, "r", 0, 0);
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();

            TestFrameWork.Verify(mirror, "k", 1, 0);
            TestFrameWork.Verify(mirror, "r", 1, 0);

        }
        [Test]
        [Category("ExpressionInterpreterRunner"), Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]

        public void inlineconditional_656_1()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
 @"
import(""ProtoGeometry.dll"");
import(""DSCoreNodes.dll"");

WCS = CoordinateSystem.Identity( );

p = 0..10..#5;
isPass5 = Count ( p ) == 5 ? true : false ; // verification

startPts = Point.ByCartesianCoordinates( WCS, p, 0, 0 );
isPass6 = Count ( startPts ) == 5 ? true : false ; // verification

endPts = Count(p) >= 1 ? Point.ByCartesianCoordinates( WCS, 0, p, 0 ) : Point.ByCartesianCoordinates( WCS, 0, 0, 0 ); 

isPass7 = Count ( endPts ) == 5 ? true : false ; // verification

lines = Line.ByStartPointEndPoint( startPts, endPts );
isPass8 = Count ( lines ) == 5 ? true : false ; // verification

lines = Line.ByStartPointEndPoint( startPts<1>, endPts<2> );
           
", runnerConfig);

            DebugRunner.VMState vms = fsr.Step(); // WCS = CoordinateSystem.Identity();

            vms = fsr.StepOver();    // p = 0..10..#5;
            vms = fsr.StepOver();    // isPass5 = Count ( p ) == 5 ? true : false ;  

            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"p");
            //Obj objExecVal = mirror.GetWatchValue();
            TestFrameWork.Verify(mirror, "p", new object[] { 0.000000, 2.500000, 5.000000, 7.500000, 10.000000 }, 0);

            vms = fsr.StepOver();    // startPts = Point.ByCartesianCoordinates( WCS, p, 0, 0 );
            TestFrameWork.Verify(mirror, "isPass5", true, 0);

            vms = fsr.StepOver();    // isPass6 = Count ( startPts ) == 5 ? true : false ;
            vms = fsr.StepOver();    //endPts = Count(p) >= 1 ? Point.ByCartesianCoordinates( WCS, 0, p, 0 ) : Point.ByCartesianCoordinates( WCS, 0, 0, 0 );
            TestFrameWork.Verify(mirror, "isPass6", true, 0);

            vms = fsr.StepOver();    // isPass7 = Count ( endPts ) == 5 ? true : false ;
            vms = fsr.StepOver();    // lines = Line.ByStartPointEndPoint( startPts, endPts );
            TestFrameWork.Verify(mirror, "isPass7", true, 0);

            vms = fsr.StepOver();    // isPass8 = Count ( lines ) == 5 ? true : false ;

            vms = fsr.StepOver();    // lines = Line.ByStartPointEndPoint( startPts<1>, endPts<2> );
            TestFrameWork.Verify(mirror, "isPass8", true, 0);

            vms = fsr.StepOver();
        }

        [Test]
        [Category("ExpressionInterpreterRunner"), Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]

        public void inlineconditional_stepin_656_1()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
 @"
import(""ProtoGeometry.dll"");

p = 0..10..#5;
isPass5 = Count ( p ) == 5 ? true : false ; // verification

startPts = Point.ByCoordinates(p, 0, 0 );
isPass6 = Count ( startPts ) == 5 ? true : false ; // verification

endPts = Count(p) >= 1 ? Point.ByCoordinates(0, p, 0 ) : Point.ByCoordinates(0, 0, 0 ); // => at this line, the debugging stops !

isPass7 = Count ( endPts ) == 5 ? true : false ; // verification

lines = Line.ByStartPointEndPoint( startPts, endPts );
isPass8 = Count ( lines ) == 5 ? true : false ; // verification

lines = Line.ByStartPointEndPoint( startPts<1>, endPts<2> );
           
", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();

            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();

            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"p");
            //Obj objExecVal = mirror.GetWatchValue();
            TestFrameWork.Verify(mirror, "p", new object[] { 0.000000, 2.500000, 5.000000, 7.500000, 10.000000 }, 0);
            vms = fsr.Step();
            vms = fsr.Step();
            TestFrameWork.Verify(mirror, "isPass5", true, 0);
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            TestFrameWork.Verify(mirror, "isPass6", true, 0);
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            TestFrameWork.Verify(mirror, "isPass7", true, 0);
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            TestFrameWork.Verify(mirror, "isPass8", true, 0);
        }

        [Test]
        [Category("ExpressionInterpreterRunner")]
        [Category("Failure")]
        public void inlineconditional_656_2()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
 @"
import(""DSCoreNodes.dll"");
x = 330;
a;
[Imperative]
{
    
    a = x > 1 ? Math.Cos(60) : Math.Cos(45);
    b = 22;
}
           
", runnerConfig);

            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1568
            Assert.Fail("IDE-656Regression: Debugging stops at inline condition");

            DebugRunner.VMState vms = fsr.Step();    // x = 330;

            vms = fsr.StepOver();    // a = x > 20 ?  foo(44) : 55 ;
            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"x");
            Obj objExecVal = mirror.GetWatchValue();
            TestFrameWork.Verify(mirror, "x", 330, 0);

            vms = fsr.StepOver();   // b = 22;
            TestFrameWork.Verify(mirror, "a", 0.5, 0);

            TestFrameWork.Verify(mirror, "b", 22, 0);
        }

        [Test]
        [Category("ExpressionInterpreterRunner")]
        [Category("Failure")]
        public void inlineconditional_stepin_656_2()
        {
            fsr.PreStart( // Execute and verify the main script in a debug session
    @"
import(""DSCoreNodes.dll"");
x = 330;
a;
[Imperative]
{
    
    a = x > 1 ? Math.Cos(60) : Math.Cos(45);
    b = 22;
}
           
", runnerConfig);

            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1568
            Assert.Fail("IDE-656Regression: Debugging stops at inline condition");
            DebugRunner.VMState vms = fsr.Step();    // x = 330;

            vms = fsr.Step();    // a = x > 20 ?  foo(44) : 55 ;


            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"x");
            Obj objExecVal = mirror.GetWatchValue();
            TestFrameWork.Verify(mirror, "x", 330, 0);
            vms = fsr.Step();
            TestFrameWork.Verify(mirror, "a", null, 0);
            vms = fsr.Step();
            vms = fsr.Step();   // b = 2;
            TestFrameWork.Verify(mirror, "a", 0.5, 0);
            vms = fsr.Step();    // b = 2;
            // b = 2;
            TestFrameWork.Verify(mirror, "b", 22, 0);
            // b = 2;


        }
        [Test]
        [Category("ExpressionInterpreterRunner")]
        [Category("Failure")]
        public void inlineconditional_656_3()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
 @"
x = 330;

def foo(y : int)
{
    return = y + 222;
}
a;
[Imperative]
{
    a = x > foo(20) ? 44 : foo(55); //Line 10
    b = 2;
}
           
", runnerConfig);

            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1568
            Assert.Fail("IDE-656Regression: Debugging stops at inline condition");
            DebugRunner.VMState vms = fsr.Step();    // x = 330;

            vms = fsr.StepOver();    // a = x > 20 ?  foo(44) : 55 ;


            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"x");
            Obj objExecVal = mirror.GetWatchValue();
            TestFrameWork.Verify(mirror, "x", 330, 0);
            vms = fsr.StepOver();
            TestFrameWork.Verify(mirror, "a", null, 0);
            vms = fsr.StepOver();    // b = 2;
            TestFrameWork.Verify(mirror, "a", 44, 0);
            vms = fsr.StepOver();    // b = 2;

            TestFrameWork.Verify(mirror, "b", 22, 0);



        }
        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void inlineconditional_stepin_656_3()
        {
            fsr.PreStart( // Execute and verify the main script in a debug session
    @"
x = 330;

def foo(y : int)
{
    return = y + 222;
}
a;
b;
[Imperative]
{
    a = x > foo(20) ? 44 : foo(55); //Line 10
    b = 2;
}
           
", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();    // x = 330;

            vms = fsr.Step();    // a = x > 20 ?  foo(44) : 55 ;


            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"x");
            //   Obj objExecVal = mirror.GetWatchValue();
            TestFrameWork.Verify(mirror, "x", 330, 0);
            vms = fsr.Step();
            TestFrameWork.Verify(mirror, "a", null, 0);
            vms = fsr.Step();

            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"y");
            Obj objExecVal = mirror.GetWatchValue();
            Assert.AreNotEqual(null, objExecVal);
            Assert.AreEqual(20, objExecVal.Payload);
            vms = fsr.Step();
            vms = fsr.Step();   // b = 2;
            vms = fsr.Step();
            TestFrameWork.Verify(mirror, "a", 44, 0);
            vms = fsr.Step();    // b = 2;
            // b = 2;
            TestFrameWork.Verify(mirror, "b", 2, 0);
            // b = 2;
        }

        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void inlineconditional_stepin_656_4()
        {
            fsr.PreStart( // Execute and verify the main script in a debug session
    @"
x = 330;

def foo(y : int)
{
    return = y + 222;
}
a;
b;
[Imperative]
{
    a = x > 20 ? foo(44) : foo(55); //Line 10
    b = 2;
}
           
", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();    // x = 330;

            vms = fsr.Step();    // a = x > 20 ?  foo(44) : 55 ;


            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"x");
            //   Obj objExecVal = mirror.GetWatchValue();
            TestFrameWork.Verify(mirror, "x", 330, 0);
            vms = fsr.Step();
            TestFrameWork.Verify(mirror, "a", null, 0);
            vms = fsr.Step();

            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"y");
            Obj objExecVal = mirror.GetWatchValue();
            Assert.AreNotEqual(null, objExecVal);
            Assert.AreEqual(44, objExecVal.Payload);
            vms = fsr.Step();
            vms = fsr.Step();   // b = 2;
            vms = fsr.Step();
            TestFrameWork.Verify(mirror, "a", 266, 0);
            vms = fsr.Step();    // b = 2;
            // b = 2;
            TestFrameWork.Verify(mirror, "b", 2, 0);
            // b = 2;


        }
        [Test]
        [Category("ExpressionInterpreterRunner")]
        [Category("Failure")]
        public void inlineconditional_656_4()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
 @"
x = 330;

def foo(y : int)
{
    return = y + 222;
}
a;
[Imperative]
{
    a = x > 20 ? foo(44) : foo(55); //Line 10
    b = 2;
}
           
", runnerConfig);


            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1568
            Assert.Fail("IDE-656Regression: Debugging stops at inline condition");
            DebugRunner.VMState vms = fsr.Step();    // x = 330;

            vms = fsr.StepOver();    // a = x > 20 ?  foo(44) : 55 ;


            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"x");
            Obj objExecVal = mirror.GetWatchValue();
            TestFrameWork.Verify(mirror, "x", 330, 0);
            vms = fsr.StepOver();
            TestFrameWork.Verify(mirror, "a", null, 0);
            vms = fsr.StepOver();    // b = 2;
            TestFrameWork.Verify(mirror, "a", 266, 0);
            vms = fsr.StepOver();    // b = 2;

            TestFrameWork.Verify(mirror, "b", 22, 0);



        }
        [Test]
        [Category("ExpressionInterpreterRunner")]

        public void inlineconditional_656_5()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
 @"
x = 330;

def foo(y : int)
{
    return = y + 222;
}

[Associative]
{
    a = x > 20 ?  foo(44) : 55 ; //Line 10
    b = 2;
}
           
", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();    // x = 330;

            vms = fsr.StepOver();    // a = x > 20 ?  foo(44) : 55 ;


            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"x");
            //Obj objExecVal = mirror.GetWatchValue();
            TestFrameWork.Verify(mirror, "x", 330, 0);

            vms = fsr.StepOver();    // b = 2;

            TestFrameWork.Verify(mirror, "a", 266, 0);
            vms = fsr.StepOver();
            TestFrameWork.Verify(mirror, "x", 330, 0);
            TestFrameWork.Verify(mirror, "b", 2, 0);
            vms = fsr.StepOver();

        }

        [Test]
        [Category("ExpressionInterpreterRunner")]

        public void inlineconditional_stepin_656_5()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
 @"
x = 330;

def foo(y : int)
{
    return = y + 222;
}

[Associative]
{
    a = x > 20 ?  foo(44) : 55 ; //Line 10
    b = 2;
}
           
", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();    // x = 330;

            vms = fsr.Step();    // a = x > 20 ?  foo(44) : 55 ;

            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"x");
            //Obj objExecVal = mirror.GetWatchValue();
            TestFrameWork.Verify(mirror, "x", 330, 0);

            vms = fsr.Step();    // return = y + 222;
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"y");
            Obj objExecVal = mirror.GetWatchValue();

            // It should not be available.
            Assert.AreNotEqual(null, objExecVal);
            Assert.AreEqual(44, objExecVal.Payload);
            //TestFrameWork.Verify(mirror, "y", 44, 0);

            vms = fsr.Step();
            vms = fsr.Step();    // a = x > 20 ?  foo(44) : 55 ;
            vms = fsr.Step();    // b = 2;

            TestFrameWork.Verify(mirror, "a", 266, 0);

            vms = fsr.Step();

            TestFrameWork.Verify(mirror, "x", 330, 0);

            TestFrameWork.Verify(mirror, "a", 266, 0);

            TestFrameWork.Verify(mirror, "b", 2, 0);
        }

        [Test]
        [Category("ExpressionInterpreterRunner")]

        public void inlineconditional_656_6()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
 @"
x = 330;

def foo(y : int)
{
    return = y + 222;
}

[Associative]
{
    a = x > foo(20) ?  foo(44): 33; //Line 10
    b = 2;
}
           
", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();

            vms = fsr.StepOver();


            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"x");
            //Obj objExecVal = mirror.GetWatchValue();
            TestFrameWork.Verify(mirror, "x", 330, 0);
            vms = fsr.StepOver();
            TestFrameWork.Verify(mirror, "a", 266, 0);
            vms = fsr.StepOver();
            TestFrameWork.Verify(mirror, "x", 330, 0);
            TestFrameWork.Verify(mirror, "b", 2, 0);

        }
        [Test]
        [Category("ExpressionInterpreterRunner")]

        public void inlineconditional_stepin_656_6()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
 @"
x = 330;

def foo(y : int)
{
    return = y + 222;
}

[Associative]
{
    a = x > foo(20) ?  foo(44): 33; //Line 10
    b = 2;
}
           
", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();

            vms = fsr.Step();


            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"x");
            //Obj objExecVal = mirror.GetWatchValue();

            TestFrameWork.Verify(mirror, "x", 330, 0);
            vms = fsr.Step();
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"y");
            Obj objExecVal = mirror.GetWatchValue();

            Assert.AreNotEqual(null, objExecVal);
            Assert.AreEqual(20, objExecVal.Payload);
            //TestFrameWork.Verify(mirror, "y", 20, 0);

            vms = fsr.Step();
            vms = fsr.Step();

            vms = fsr.Step();
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"y");
            objExecVal = mirror.GetWatchValue();

            Assert.AreNotEqual(null, objExecVal);
            Assert.AreEqual(44, objExecVal.Payload);
            //TestFrameWork.Verify(mirror, "y", 44, 0);

            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            TestFrameWork.Verify(mirror, "a", 266, 0);
            vms = fsr.Step();
            TestFrameWork.Verify(mirror, "b", 2, 0);



        }
        [Test]
        [Category("ExpressionInterpreterRunner")]

        public void inlineconditional_656_7()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
 @"
x = 330;

def foo(y : int)
{
    return = y + 222;
}

[Associative]
{
    a = x < foo(20) ? 33 :  foo(44); //Line 10
    b = 2;
}
           
", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();

            vms = fsr.StepOver();


            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"x");
            Obj objExecVal = mirror.GetWatchValue();
            TestFrameWork.Verify(mirror, "x", 330, 0);
            vms = fsr.StepOver();
            TestFrameWork.Verify(mirror, "a", 266, 0);
            vms = fsr.StepOver();
            TestFrameWork.Verify(mirror, "x", 330, 0);
            TestFrameWork.Verify(mirror, "b", 2, 0);

        }
        [Test]
        [Category("ExpressionInterpreterRunner")]

        public void inlineconditional_stepin_656_7()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
 @"
x = 330;

def foo(y : int)
{
    return = y + 222;
}

[Associative]
{
    a = x < foo(20) ? 33 :  foo(44); //Line 10
    b = 2;
}
           
", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();

            vms = fsr.Step();


            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"x");
            // Obj objExecVal = mirror.GetWatchValue();

            TestFrameWork.Verify(mirror, "x", 330, 0);
            vms = fsr.Step();

            //TestFrameWork.Verify(mirror, "y", 20, 0);
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"y");
            Obj objExecVal = mirror.GetWatchValue();

            Assert.AreNotEqual(null, objExecVal);
            Assert.AreEqual(20, objExecVal.Payload);

            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            //TestFrameWork.Verify(mirror, "y", 44, 0);
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"y");
            objExecVal = mirror.GetWatchValue();

            Assert.AreNotEqual(null, objExecVal);
            Assert.AreEqual(44, objExecVal.Payload);

            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();

            TestFrameWork.Verify(mirror, "a", 266, 0);
            vms = fsr.Step();
            TestFrameWork.Verify(mirror, "b", 2, 0);
        }

        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void inlineconditional_stepnext_656_9()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
 @"
b1 = 1;

class A
{
    x : int;
    
    constructor A()
    {
        x = 1;
    }
    
    def foo()
    {
        return = 90; //Line 14
    } //Line 15
}


a1 = 4;
a2 = A.A();
a3 = a1 > a2.foo() ? 0 : a2.foo();
c = 90;
           
", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();

            vms = fsr.StepOver();


            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"x");
            // Obj objExecVal = mirror.GetWatchValue();
            TestFrameWork.Verify(mirror, "b1", 1, 0);

            vms = fsr.StepOver();
            TestFrameWork.Verify(mirror, "a1", 4, 0);
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            TestFrameWork.Verify(mirror, "a3", 90, 0);
            vms = fsr.StepOver();
            TestFrameWork.Verify(mirror, "c", 90, 0);


        }
        [Test]
        [Category("ExpressionInterpreterRunner")]

        public void inlineconditional_stepin_656_9()
        {
            // Execute and verify the main script in a debug session
            // Execute and verify the main script in a debug session
            fsr.PreStart(
 @"
b1 = 1;

class A
{
    x : int;
    
    constructor A()
    {
        x = 1;
    }
    
    def foo()
    {
        return = 90; //Line 14
    } //Line 15
}


a1 = 4;
a2 = A.A();
a3 = a1 > a2.foo() ? 0 : a2.foo();
c = 90;
           
", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();    // b1 = 1;

            vms = fsr.Step();    // a1 = 4;
            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"b1");
            TestFrameWork.Verify(mirror, "b1", 1, 0);

            vms = fsr.Step();    // a2 = A.A();
            TestFrameWork.Verify(mirror, "a1", 4, 0);

            vms = fsr.Step();    // x : int;
            vms = fsr.Step();    // x = 1;           
            // Obj objExecVal = mirror.GetWatchValue();
            vms = fsr.Step();
            //TestFrameWork.Verify(mirror, "x", 1, 0);
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"x");
            Obj o = mirror.GetWatchValue();
            Assert.AreNotEqual(null, o);
            Assert.AreEqual(1, o.Payload);

            vms = fsr.Step();    // a2 = A.A();

            vms = fsr.Step();    // a3 = a1 > a2.foo() ? 0 : a2.foo();

            vms = fsr.Step();    //  return = 90;
            vms = fsr.Step();
            vms = fsr.Step();    // a3 = a1 > a2.foo() ? 0 : a2.foo();

            vms = fsr.Step();    //  return = 90;
            vms = fsr.Step();
            vms = fsr.Step();    // a3 = a1 > a2.foo() ? 0 : a2.foo();

            vms = fsr.Step();    // c = 90;
            TestFrameWork.Verify(mirror, "a3", 90, 0);

            vms = fsr.Step();
            TestFrameWork.Verify(mirror, "c", 90, 0);
        }

        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void inlineconditional_stepnext_656_13()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
 @"
class B
{
    x : var;
    constructor B(y)
    {
        x = y;
    }
    
    def foo()
    {
        return = 90;
    }
}

x = 1;
a =
{
    x > 10 ? true : false => a1;
    B.B(a1).x => a2; //Line 19
    4 => a5;
}
           
", runnerConfig);
            
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1568
            DebugRunner.VMState vms = fsr.Step();

            vms = fsr.StepOver();


            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"x");
            // Obj objExecVal = mirror.GetWatchValue();
            TestFrameWork.Verify(mirror, "x", 1, 0);

            vms = fsr.StepOver();
            TestFrameWork.Verify(mirror, "a1", false, 0);
            vms = fsr.StepOver();
            TestFrameWork.Verify(mirror, "a2", false, 0);
            vms = fsr.StepOver();
            TestFrameWork.Verify(mirror, "a5", 4, 0);
            vms = fsr.StepOver();
            TestFrameWork.Verify(mirror, "a", 4, 0);
        }
        [Test]
        [Category("ExpressionInterpreterRunner")]

        public void inlineconditional_stepin_656_13()
        {
            // Execute and verify the main script in a debug session
            // Execute and verify the main script in a debug session
            fsr.PreStart(
 @"
class B
{
    x1 : var;
    constructor B(y)
    {
        x1 = y;
    }
    
    def foo()
    {
        return = 90;
    }
}

x = 1;
a =
{
    x > 10 ? true : false => a1;
    B.B(a1).x1 => a2; //Line 19
    4 => a5;
}
           
", runnerConfig);
            DebugRunner.VMState vms = fsr.Step();    // x = 1;

            vms = fsr.Step();    // x > 10 ? true : false => a1;


            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"x");
            // Obj objExecVal = mirror.GetWatchValue();
            TestFrameWork.Verify(mirror, "x", 1, 0);
            vms = fsr.Step();    // B.B(a1).x1 => a2; 
            TestFrameWork.Verify(mirror, "a1", false, 0);
            vms = fsr.Step();
            vms = fsr.Step();
            //TestFrameWork.Verify(mirror, "x1", false, 0);

            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"x1");
            Obj objExecVal = mirror.GetWatchValue();

            Assert.AreNotEqual(null, objExecVal);
            Assert.AreEqual(false, objExecVal.Payload);

            vms = fsr.StepOver();
            vms = fsr.StepOver();
            TestFrameWork.Verify(mirror, "a2", false, 0);
            vms = fsr.StepOver();
            TestFrameWork.Verify(mirror, "a5", 4, 0);
            vms = fsr.StepOver();
            TestFrameWork.Verify(mirror, "a", 4, 0);
        }

        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void inlineconditional_stepnext_656_10()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
 @"
class A
{
    static def foo(y : int)
    {
        return = y * 2;
    }
    
}

x = 33;

def foo : int(y : int)
{
    return = y + 222;
}

a = x > foo(22) ? foo(1) : A.foo(4);
           
", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();

            vms = fsr.StepOver();


            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"x");
            // Obj objExecVal = mirror.GetWatchValue();
            TestFrameWork.Verify(mirror, "x", 33, 0);

            vms = fsr.StepOver();
            TestFrameWork.Verify(mirror, "a", 8, 0);
        }

        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void inlineconditional_stepin_656_10()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
 @"
class A
{
    static def foo(y : int)
    {
        return = y * 2;
    }
    
}

x = 33;

def foo : int(y : int)
{
    return = y + 222;
}

a = x > foo(22) ? foo(1) : A.foo(4);
           
", runnerConfig);

            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1568
            DebugRunner.VMState vms = fsr.Step();    // x = 33;

            vms = fsr.Step();    // a = x > foo(22) ? foo(1) : A.foo(4);

            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"x");
            TestFrameWork.Verify(mirror, "x", 33, 0);

            vms = fsr.Step();    // return = y + 222;
            //TestFrameWork.Verify(mirror, "y", 22, 0);
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"y");
            Obj objExecVal = mirror.GetWatchValue();

            // It should not be available.
            Assert.AreNotEqual(null, objExecVal);
            Assert.AreEqual(22, objExecVal.Payload);

            vms = fsr.Step();
            vms = fsr.Step();    // a = x > foo(22) ? foo(1) : A.foo(4);
            vms = fsr.Step();    // return = y * 2;
            //TestFrameWork.Verify(mirror, "y", 4, 0);
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"y");
            objExecVal = mirror.GetWatchValue();

            // It should not be available.
            Assert.AreNotEqual(null, objExecVal);
            Assert.AreEqual(1, objExecVal.Payload);

            vms = fsr.Step();
            vms = fsr.Step();    // a = x > foo(22) ? foo(1) : A.foo(4);
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            TestFrameWork.Verify(mirror, "a", 8, 0);
        }
        [Test]
        [Category("ExpressionInterpreterRunner")]

        public void inlineconditional_stepnext_656_12()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
 @"
def GetCoor(type : int)
{
    return = type == 1 ? 10 : 20;
}

list1 = { 1, 2 };

list3 = GetCoor(list1);
           
", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();

            vms = fsr.StepOver();


            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"list1");
            // Obj objExecVal = mirror.GetWatchValue();

            TestFrameWork.Verify(mirror, "list1", new object[] { 1, 2 }, 0);
            vms = fsr.StepOver();

            TestFrameWork.Verify(mirror, "list3", new object[] { 10, 20 }, 0);





        }
        [Test]
        [Category("ExpressionInterpreterRunner")]

        public void inlineconditional_stepin_656_12()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
 @"
def GetCoor(type : int)
{
    return = type == 1 ? 10 : 20;
}

list1 = { 1, 2 };

list3 = GetCoor(list1);
           
", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();

            vms = fsr.Step();


            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"list1");
            // Obj objExecVal = mirror.GetWatchValue();

            TestFrameWork.Verify(mirror, "list1", new object[] { 1, 2 }, 0);
            vms = fsr.Step();
            vms = fsr.Step();
            TestFrameWork.Verify(mirror, "list3", new object[] { 10, 20 }, 0);

        }

        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void inlineconditional_stepnext_656_14()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
 @"
class B
{
    x : var;
    constructor B(y)
    {
        x = y;
    }
    
    def foo()
    {
        return = 90;
    }
}

x = 1;
a =
{
    x > 10 ? true : false => a1;
    B.B(a1).x => a2; //Line 19
    4 => a5;
}           
", runnerConfig);
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1568
            DebugRunner.VMState vms = fsr.Step();    // x = 1;

            vms = fsr.StepOver();    // x > 10 ? true : false => a1;
            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"x");
            // Obj objExecVal = mirror.GetWatchValue();
            TestFrameWork.Verify(mirror, "x", 1, 0);

            vms = fsr.StepOver();    // B.B(a1).x => a2; //Line 19
            TestFrameWork.Verify(mirror, "a1", false, 0);

            vms = fsr.StepOver();    // 4 => a5;
            TestFrameWork.Verify(mirror, "a2", false, 0);

            vms = fsr.StepOver();    // closing brace of a = {}
            TestFrameWork.Verify(mirror, "a5", 4, 0);
        }

        [Test]
        [Category("ExpressionInterpreterRunner")]
        [Category("Failure")]
        public void inlineconditional_stepin_656_14()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
 @"
class A
{
    a : var;
    a2 : var;
    a4 : var;
    
    
    constructor A(x : var)
    {
        a = x;
    }
    def update(x : var)
    {
        a = {
            x => a1;
            a1 > 10 ? true : false => a4;
            
        }
        return = x;
    }
}

class B
{}

a1 = A.A(0);
a1 = A.A();
x = a1.update(1); //line 28


b1 = B.B(); //line 31
n = 22;
", runnerConfig);
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1568
            DebugRunner.VMState vms = fsr.Step();

            vms = fsr.Step();
            vms = fsr.Step();


            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"a");
            // Obj objExecVal = mirror.GetWatchValue();
            /* TestFrameWork.Verify(mirror, "a", 0, 0);*/
            mirror = watchRunner.Execute(@"a");
            Obj objExecVal = mirror.GetWatchValue();

            Assert.AreNotEqual(null, objExecVal);
            Assert.AreEqual(0, objExecVal.Payload);
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            TestFrameWork.Verify(mirror, "a1", 1, 0);
            vms = fsr.Step();

            /*TestFrameWork.Verify(mirror, "a4", false, 0);
            mirror = watchRunner.Execute(@"a4");
            Obj objExecVal2 = mirror.GetWatchValue();

            Assert.AreNotEqual(null, objExecVal2);
            Assert.AreEqual(false, objExecVal2.Payload);*/
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            TestFrameWork.Verify(mirror, "x", 1, 0);
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            TestFrameWork.Verify(mirror, "n", 22, 0);
        }

        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void inlineconditional_stepin_highlighting_657_1()
        {
            fsr.PreStart( // Execute and verify the main script in a debug session
    @"
x = 330;

def foo(y : int)
{
    return = y + 222;
}

[Imperative]
{
    a = x > foo(22) ? foo(1) : 55; //Line 10
}           
", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();    // x = 330;
            Assert.AreEqual(2, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(2, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();    // a = x > foo(22) ? foo(1) : 55;
            Assert.AreEqual(11, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(11, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(35, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();    // return = y + 222;
            Assert.AreEqual(6, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(6, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(22, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();
            Assert.AreEqual(7, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(7, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();    // a = x > foo(22) ? foo(1) : 55;
            Assert.AreEqual(11, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(11, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(35, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();    // return = y + 222;
            Assert.AreEqual(6, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(6, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(22, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();
            Assert.AreEqual(7, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(7, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();    // a = x > foo(22) ? foo(1) : 55;
            Assert.AreEqual(11, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(11, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(35, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();    // closing brace of Imperative block
            Assert.AreEqual(12, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(12, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.EndExclusive.CharNo);

        }

        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void inlineconditional_stepin_highlighting_657_2()
        {
            fsr.PreStart( // Execute and verify the main script in a debug session
    @"
x = 330;

def foo(y : int)
{
    return = y + 222;
}

[Imperative]
{
    a = x < foo(20) ? 44 : foo(55); //Line 10
}        
", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();    // x = 330;
            Assert.AreEqual(2, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(2, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();    // a = x > foo(22) ? foo(1) : 55;
            Assert.AreEqual(11, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(11, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(36, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();    // return = y + 222;
            Assert.AreEqual(6, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(6, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(22, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();
            Assert.AreEqual(7, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(7, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();    // a = x > foo(22) ? foo(1) : 55;
            Assert.AreEqual(11, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(11, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(36, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();    // return = y + 222;
            Assert.AreEqual(6, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(6, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(22, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();
            Assert.AreEqual(7, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(7, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();    // a = x > foo(22) ? foo(1) : 55;
            Assert.AreEqual(11, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(5, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(11, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(36, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.Step();    // closing brace of Imperative block
            Assert.AreEqual(12, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(12, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(2, vms.ExecutionCursor.EndExclusive.CharNo);

        }

        [Test]
        [Category("ExpressionInterpreterRunner"), Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void IDE_Debugger_698()
        {
            fsr.PreStart( // Execute and verify the main script in a debug session
    @"
import(""DSCoreNodes.dll"");
import(""ProtoGeometry.dll"");

results = { { } };

numCycles = 2;
s;
[Imperative]
{
    for(i in (0..(numCycles)))
        {
        results[i] = { };
    
                for(j in(0..(numCycles-1)))
                {
                        for(k in(0..(numCycles-1)))
                        {
                            results[i][j] = i * j;
                            s = Print(results[i][j]);
                        }
                    c=k;
                }
                s = Print(results[i]);
        } 
}

s = Print(results); 
", runnerConfig);

            Assert.Fail("TODO: Update this test case. It works fine in the debugger");
            DebugRunner.VMState vms = fsr.Step();

            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();

            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"k");

            mirror = watchRunner.Execute(@"k");
            Obj objExecVal = mirror.GetWatchValue();
            TestFrameWork.Verify(mirror, "k", 0, 0);
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();



            TestFrameWork.Verify(mirror, "j", 0, 0);
            //TestFrameWork.Verify(mirror, "c", null, 0);
            //   TestFrameWork.VerifyBuildWarning(ProtoCore.BuildData.WarningID.kIdUnboundIdentifier);

        }
        [Test]
        [Category("ExpressionInterpreterRunner"), Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void IDE_Debugger_698_2()
        {
            fsr.PreStart( // Execute and verify the main script in a debug session
    @"
import(""ProtoGeometry.dll"");
i[0] = Point.ByCoordinates(0, 1, 1);
i[1] = Point.ByCoordinates(0, 2, 1);
i[2] = Point.ByCoordinates(0, 3, 1);
i[3] = Point.ByCoordinates(0, 4, 1);
i[4] = Point.ByCoordinates(0, 5, 1);

[Imperative]
{
    for(j in 0..1)
    {
        for(k in i)
        {
            r = k;
        }
        c = k;
    }
}
", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();    // x = 330;

            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();

            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"c");
            mirror = watchRunner.Execute(@"c");
            Obj objExecVal = mirror.GetWatchValue();
            TestFrameWork.Verify(mirror, "c", null, 0);

        }
        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void IDE_Debugger_698_3()
        {
            fsr.PreStart( // Execute and verify the main script in a debug session
    @"
results = { { } };

numCycles = 4;
s;
[Imperative]
{
    k = 0;
    while(k < 2)
    {
    for(i in (0..(numCycles)))
        {
        results[i] = { };
    
                for(j in(0..(numCycles-1)))
                {
                results[i][j] = i * j;
                }

            c = j;
        }
        k = k + 1;
    }
}
", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();    // x = 330;

            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();


            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"c");
            mirror = watchRunner.Execute(@"c");
            Obj objExecVal = mirror.GetWatchValue();
            TestFrameWork.Verify(mirror, "c", null, 0);

        }

        [Test]
        [Category("DebuggerReferenceCount")]
        public void IDE_DebuggerRefCount_LangBlock()
        {
            fsr.PreStart( // Execute and verify the main script in a debug session
    @"
class A
{
    def _Dispose()
    {
        b = -1;
    }
}

b = 0;

[Imperative]
{
    a = A.A();
}

c = 2;
", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();   // b = 0;

            vms = fsr.StepOver();   // a = A.A();
            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"b");
            Obj objExecVal = mirror.GetWatchValue();
            Assert.IsTrue((Int64)objExecVal.Payload == 0);

            vms = fsr.StepOver();
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"a");
            objExecVal = mirror.GetWatchValue();
            Assert.AreNotEqual(null, objExecVal);
            Assert.AreNotEqual(null, objExecVal.Payload);
            Assert.AreEqual(mirror.GetType(objExecVal), "A");

            vms = fsr.StepOver();   // c = 2;
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"b");
            objExecVal = mirror.GetWatchValue();
            Assert.IsTrue((Int64)objExecVal.Payload == -1);

            vms = fsr.StepOver();   // end of script
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"c");
            objExecVal = mirror.GetWatchValue();
            Assert.IsTrue((Int64)objExecVal.Payload == 2);
        }

        [Test]
        [Category("DebuggerReferenceCount")]
        public void IDE_DebuggerRefCount_FunctionCall()
        {
            fsr.PreStart( // Execute and verify the main script in a debug session
    @"
class A
{
    def _Dispose()
    {
        b = -1;
    }
    
    
}

def foo()
{
    a = A.A();
}
   
b = 0;
foo();
c = 2;
", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();   // b = 0;

            vms = fsr.StepOver();   // foo();
            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"b");
            Obj objExecVal = mirror.GetWatchValue();
            Assert.IsTrue((Int64)objExecVal.Payload == 0);

            fsr.Step();   // a = A.A();
            fsr.StepOver();

            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"a");
            objExecVal = mirror.GetWatchValue();
            Assert.AreNotEqual(null, objExecVal);
            Assert.AreNotEqual(null, objExecVal.Payload);
            Assert.AreEqual(mirror.GetType(objExecVal), "A");

            vms = fsr.StepOver();   // c = 2;
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"b");
            objExecVal = mirror.GetWatchValue();
            Assert.IsTrue((Int64)objExecVal.Payload == -1);

            vms = fsr.StepOver();   // end of script
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"c");
            objExecVal = mirror.GetWatchValue();
            Assert.IsTrue((Int64)objExecVal.Payload == 2);
        }

        [Test]
        [Category("DebuggerReferenceCount")]
        public void IDE_DebuggerRefCount_ReplicatedFunctionCall()
        {
            fsr.PreStart( // Execute and verify the main script in a debug session
    @"
class A
{
    def _Dispose()
    {
        b = b -1;
    }    
}

def foo(i : int)
{
    a = A.A();
}
   
b = 0;
g = { 1, 2, 3 };
foo(g);
c = 2;
", runnerConfig);


            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3984
            DebugRunner.VMState vms = fsr.Step();   // b = 0;

            vms = fsr.StepOver();                   // g = { 1, 2, 3 };

            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"b");
            Obj objExecVal = mirror.GetWatchValue();
            Assert.IsTrue((Int64)objExecVal.Payload == 0);

            vms = fsr.StepOver();                         // foo(g);

            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"g");
            objExecVal = mirror.GetWatchValue();
            List<Obj> lo = mirror.GetArrayElements(objExecVal);
            Assert.IsTrue((Int64)lo[0].Payload == 1);
            Assert.IsTrue((Int64)lo[1].Payload == 2);
            Assert.IsTrue((Int64)lo[2].Payload == 3);

            vms = fsr.StepOver();   // c = 2;
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"b");
            objExecVal = mirror.GetWatchValue();
            Assert.IsTrue((Int64)objExecVal.Payload == -3);

            vms = fsr.StepOver();   // end of script
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"c");
            objExecVal = mirror.GetWatchValue();
            Assert.IsTrue((Int64)objExecVal.Payload == 2);

        }

        [Test]
        [Category("DebuggerReferenceCount")]
        public void IDE_DebuggerRefCount_DotCall()
        {
            fsr.PreStart( // Execute and verify the main script in a debug session
    @"
class A
{
    def _Dispose()
    {
        b = -1;
    }
    
    def foo()
    {
    	a = A.A();
    }
}
    
b = 0;
p = A.A();

p.foo();

c = 2;
", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();   // b = 0;

            vms = fsr.StepOver();   // p = A.A();
            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"b");
            Obj objExecVal = mirror.GetWatchValue();
            Assert.IsTrue((Int64)objExecVal.Payload == 0);

            fsr.StepOver(); // p.foo();

            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"p");
            objExecVal = mirror.GetWatchValue();
            Assert.AreNotEqual(null, objExecVal);
            Assert.AreNotEqual(null, objExecVal.Payload);
            Assert.AreEqual(mirror.GetType(objExecVal), "A");

            fsr.Step(); // a = A.A();
            fsr.StepOver();

            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"a");
            objExecVal = mirror.GetWatchValue();
            Assert.AreNotEqual(null, objExecVal);
            Assert.AreNotEqual(null, objExecVal.Payload);
            Assert.AreEqual(mirror.GetType(objExecVal), "A");

            vms = fsr.StepOver();   // c = 2;
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"b");
            objExecVal = mirror.GetWatchValue();
            Assert.IsTrue((Int64)objExecVal.Payload == -1);

            vms = fsr.StepOver();   // end of script
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"c");
            objExecVal = mirror.GetWatchValue();
            Assert.IsTrue((Int64)objExecVal.Payload == 2);
        }

        [Test]
        [Category("DebuggerReferenceCount")]
        public void IDE_DebuggerRefCount_ForLoop()
        {
            fsr.PreStart( // Execute and verify the main script in a debug session
    @"
class A
{
    def _Dispose()
    {
        b = b-1;
    }
}

b = 0;
arr = { 1,2 };

[Imperative]
{
    for(i in arr)
	{
    	a = A.A();
	}
}

c = 2;
", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();   // b = 0;

            vms = fsr.StepOver();   // arr = { 1,2 };
            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"b");
            Obj objExecVal = mirror.GetWatchValue();
            Assert.IsTrue((Int64)objExecVal.Payload == 0);

            vms = fsr.StepOver();   // for(i in arr)
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"arr");
            objExecVal = mirror.GetWatchValue();
            Assert.AreEqual(mirror.GetType(objExecVal), "array");
            List<Obj> lo = mirror.GetArrayElements(objExecVal);
            Assert.AreEqual(lo[0].Payload, 1);
            Assert.AreEqual(lo[1].Payload, 2);

            fsr.StepOver();
            fsr.StepOver();
            fsr.StepOver();
            vms = fsr.StepOver();   // a = A.A();
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"i");
            objExecVal = mirror.GetWatchValue();
            Assert.IsTrue((Int64)objExecVal.Payload == 1);

            fsr.StepOver();
            // check for "a"
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"a");
            objExecVal = mirror.GetWatchValue();
            Assert.AreNotEqual(null, objExecVal);
            Assert.AreNotEqual(null, objExecVal.Payload);
            Assert.AreEqual(mirror.GetType(objExecVal), "A");

            fsr.StepOver();         // for(i in arr)
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"b");
            objExecVal = mirror.GetWatchValue();

            fsr.StepOver();
            vms = fsr.StepOver();   // a = A.A();
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"i");
            objExecVal = mirror.GetWatchValue();
            Assert.IsTrue((Int64)objExecVal.Payload == 2);

            fsr.StepOver();
            // check for "a"
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"a");
            objExecVal = mirror.GetWatchValue();
            Assert.AreNotEqual(null, objExecVal);
            Assert.AreNotEqual(null, objExecVal.Payload);
            Assert.AreEqual(mirror.GetType(objExecVal), "A");

            fsr.StepOver();         // for(i in arr)
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"b");
            objExecVal = mirror.GetWatchValue();

            fsr.StepOver();
            fsr.StepOver();
            fsr.StepOver();
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"c");
            objExecVal = mirror.GetWatchValue();
            Assert.IsTrue((Int64)objExecVal.Payload == 2);
        }

        [Test]
        [Category("DebuggerReferenceCount")]
        public void IDE_DebuggerRefCount_IfStatement()
        {
            fsr.PreStart( // Execute and verify the main script in a debug session
    @"
class A
{
    def _Dispose()
    {
        b = -1;
    }
}

b = 0;

[Imperative]
{
    if(b == 0)
	{
    	a = A.A();
	}
}

c = 2;
", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();   // b = 0;

            vms = fsr.StepOver();   // if(b == 0)
            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"b");
            Obj objExecVal = mirror.GetWatchValue();
            Assert.IsTrue((Int64)objExecVal.Payload == 0);

            vms = fsr.StepOver();   // a = A.A()
            fsr.StepOver();
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"a");
            objExecVal = mirror.GetWatchValue();
            Assert.AreNotEqual(null, objExecVal);
            Assert.AreNotEqual(null, objExecVal.Payload);
            Assert.AreEqual(mirror.GetType(objExecVal), "A");

            fsr.StepOver();
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"b");
            objExecVal = mirror.GetWatchValue();

            vms = fsr.StepOver();   // c = 2;
            vms = fsr.StepOver();   // end of script
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"c");
            objExecVal = mirror.GetWatchValue();
            Assert.IsTrue((Int64)objExecVal.Payload == 2);
        }

        [Test]
        [Category("DebuggerReferenceCount")]
        public void IDE_DebuggerRefCount_ElseStatement()
        {
            fsr.PreStart( // Execute and verify the main script in a debug session
    @"
class A
{
    def _Dispose()
    {
        b = -1;
    }
}

b = 0;

[Imperative]
{
    if(b == 1)
	{
    	a = 0;
	}
    else
    {
        a = A.A();
    }
}

c = 2;
", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();   // b = 0;

            vms = fsr.StepOver();   // if(b == 1)
            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"b");
            Obj objExecVal = mirror.GetWatchValue();
            Assert.IsTrue((Int64)objExecVal.Payload == 0);

            vms = fsr.StepOver();   // a = A.A()
            fsr.StepOver();
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"a");
            objExecVal = mirror.GetWatchValue();
            Assert.AreNotEqual(null, objExecVal);
            Assert.AreNotEqual(null, objExecVal.Payload);
            Assert.AreEqual(mirror.GetType(objExecVal), "A");

            fsr.StepOver();
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"b");
            objExecVal = mirror.GetWatchValue();

            vms = fsr.StepOver();   // c = 2;
            vms = fsr.StepOver();   // end of script
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"c");
            objExecVal = mirror.GetWatchValue();
            Assert.IsTrue((Int64)objExecVal.Payload == 2);
        }


        [Test]
        [Category("DebuggerReferenceCount")]
        public void IDE_DebuggerRefCount_LangBlock_StepIn()
        {
            fsr.PreStart( // Execute and verify the main script in a debug session
    @"
class A
{
    constructor A()
    {
	}
    def _Dispose()
    {
        b = b-1;
    }
}

b = 0;

[Imperative]
{
    a = A.A();	
}

c = 2;
", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();   // b = 0;

            vms = fsr.StepOver();   // a = A.A();
            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"b");
            Obj objExecVal = mirror.GetWatchValue();
            Assert.IsTrue((Int64)objExecVal.Payload == 0);

            vms = fsr.Step();   // closing brace of A's ctor
            fsr.Step();         // a = A.A();
            fsr.Step();
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"a");
            objExecVal = mirror.GetWatchValue();
            Assert.AreNotEqual(null, objExecVal);
            Assert.AreNotEqual(null, objExecVal.Payload);
            Assert.AreEqual(mirror.GetType(objExecVal), "A");

            vms = fsr.StepOver();   // c = 2;
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"b");
            objExecVal = mirror.GetWatchValue();
            Assert.IsTrue((Int64)objExecVal.Payload == -1);

            vms = fsr.StepOver();   // end of script
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"c");
            objExecVal = mirror.GetWatchValue();
            Assert.IsTrue((Int64)objExecVal.Payload == 2);
        }

        [Test]
        [Category("DebuggerReferenceCount")]
        public void IDE_DebuggerRefCount_AnonymousVariable()
        {
            fsr.PreStart( // Execute and verify the main script in a debug session
    @"
class A
{
    constructor A()
	{
	}

    def _Dispose()
    {
        b = -1;
    }
	def foo()
	{
		return = 89;
	}
}

b = 0;

a = A.A().foo();	

c = 2;
", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();   // b = 0;

            vms = fsr.StepOver();   // a = A.A().foo();
            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"b");
            Obj objExecVal = mirror.GetWatchValue();
            Assert.IsTrue((Int64)objExecVal.Payload == 0);

            vms = fsr.StepOver();   // c = 2;
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"a");
            objExecVal = mirror.GetWatchValue();
            Assert.AreEqual(objExecVal.Payload, 89);

            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"b");
            objExecVal = mirror.GetWatchValue();

            vms = fsr.StepOver();   // end of script
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"c");
            objExecVal = mirror.GetWatchValue();
            Assert.IsTrue((Int64)objExecVal.Payload == 2);
        }

        [Test]
        [Category("DebuggerReferenceCount")]
        public void IDE_DebuggerRefCount_AnonymousVariable_StepIn()
        {
            fsr.PreStart( // Execute and verify the main script in a debug session
    @"
class A
{
    def _Dispose()
    {
        b = -1;
    }
	def foo()
	{
		return = 89;
	}
}

b = 0;

a = A.A().foo();	

c = 2;
", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();   // b = 0;

            vms = fsr.StepOver();   // a = A.A().foo();
            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"b");
            Obj objExecVal = mirror.GetWatchValue();
            Assert.IsTrue((Int64)objExecVal.Payload == 0);

            fsr.Step();
            vms = fsr.Step();   // return = 89;
            fsr.Step();
            fsr.Step();         // a = A.A().foo();
            fsr.Step();         // c = 2;

            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"a");
            objExecVal = mirror.GetWatchValue();
            Assert.AreEqual(objExecVal.Payload, 89);

            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"b");
            objExecVal = mirror.GetWatchValue();

            vms = fsr.StepOver();   // end of script
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"c");
            objExecVal = mirror.GetWatchValue();
            Assert.IsTrue((Int64)objExecVal.Payload == 2);
        }

        [Test]
        public void Defect_1467570_Crash_In_Debug_Mode()
        {
            string src = @" 
class Test 
{   

    IntArray : int[]; 
    
    constructor FirstApproach(intArray : int[]) 
    { 
        IntArray = intArray; 
    } 
    
    def Transform(adjust : int) 
    { 
        return = Test.FirstApproach(this.IntArray + adjust); 
    } 
        
} 

myTest = Test.FirstApproach({ 1, 2 }); 

myNeTwst = myTest.Transform(1); 
";

            fsr.PreStart(src, runnerConfig);
            DebugRunner.VMState vms = fsr.Step();   // myTest = Test.FirstApproach({ 1, 2 }); 

            ProtoCore.CodeModel.CodePoint cp = new ProtoCore.CodeModel.CodePoint
            {
                LineNo = 15,
                CharNo = 5
            };

            fsr.ToggleBreakpoint(cp);

            fsr.Run();  // closing brace of Transform()            

            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"this");
            Obj objExecVal = mirror.GetWatchValue();
            Assert.AreNotEqual(null, objExecVal);
            Assert.AreNotEqual(null, objExecVal.Payload);
            Assert.AreEqual(mirror.GetType(objExecVal), "Test");

            vms = fsr.StepOver();

            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"this");
            objExecVal = mirror.GetWatchValue();
            Assert.AreNotEqual(null, objExecVal);
            Assert.AreEqual(-1, (Int64)objExecVal.Payload);
            Assert.AreEqual(mirror.GetType(objExecVal), "null");
        }

        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void TestWatchExpressionForFFIProperty()
        {
            string src = @" 
import(Dummy from ""FFITarget.dll"");
a = Dummy.Create(2);
b = 2;
";

            fsr.PreStart(src, runnerConfig);
            DebugRunner.VMState vms = fsr.Step();
            vms = fsr.StepOver();

            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"a.DummyProperty");
            Obj objExecVal = mirror.GetWatchValue();
            Assert.AreEqual(2, (Int64)objExecVal.Payload);

            vms = fsr.Step();

            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"a.DummyProperty");
            objExecVal = mirror.GetWatchValue();
            Assert.AreEqual(2, (Int64)objExecVal.Payload);

        }

        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void TestWatchExpressionForFFIProperty_1()
        {
            string src = @" 
import(Dummy from ""FFITarget.dll"");


a : Dummy = null;

[Associative]
{
    a@final = Dummy.Create(2);
    
    a = a@final;

}
";

            fsr.PreStart(src, runnerConfig);
            DebugRunner.VMState vms = fsr.Step();   // a : Dummy = null;
            fsr.Step();
            fsr.StepOver();                         // a = a@final;

            /*ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"a@final.DummyProperty");
            Obj objExecVal = mirror.GetWatchValue();
            Assert.AreEqual(2, (Int64)objExecVal.Payload);*/

            vms = fsr.Step();                       // closing brace of assoc block

            /*watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"a.DummyProperty");
            objExecVal = mirror.GetWatchValue();
            Assert.AreEqual(2, (Int64)objExecVal.Payload);*/

            fsr.Step();
            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"a.DummyProperty");
            Obj objExecVal = mirror.GetWatchValue();
            Assert.AreEqual(2, (Int64)objExecVal.Payload);
        }
        [Test]
        [Category("ExpressionInterpreterRunner"), Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void TestDebug_757()
        {
            string src = @" 
                import(""ProtoGeometry.dll"");
                import(""DSCoreNodes.dll"");



                    a : Line = null;
                    b : Line = null;

                    [Associative]
                    {
                        a =
                        {
                            Line.ByStartPointEndPoint(Point.ByCoordinates(10, 0, 0), Point.ByCoordinates(10, 5, 0)) => a@initial;
                            Translate(1, 1, 0) => a@final;     // move the line
                        }
    
                        b = a@initial.Translate(-1, 1, 0); // and use the right assign
                    }
                ";

            fsr.PreStart(src, runnerConfig);
            DebugRunner.VMState vms = fsr.Step();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();

            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"a");
            Obj objExecVal = mirror.GetWatchValue();
            Assert.AreNotEqual(null, objExecVal.Payload);


            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"b");
            objExecVal = mirror.GetWatchValue();
            Assert.AreNotEqual(null, objExecVal.Payload);

        }
        [Test]
        [Category("ExpressionInterpreterRunner"), Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void Defect_StepOver_734()
        {
            fsr.PreStart(
                 @"import(""ProtoGeometry.dll"");
                  [Imperative]
                  {
                        
                        Point.ByCoordinates(0, 0, 0);//line 5    
                        Point.ByCoordinates(10, 10, 10);    
                        Point.ByCoordinates(20, 10, 10);            
                        a : int = 10;//line 9    
                        a = a + 10;//line 10
                  }
                  ", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();   // sphere = Sphere.ByCenterPointRadius(Point.ByCoordinates(0, 0, 0), 1);
            vms = fsr.StepOver();   // surfaceGeom = sphere.Faces[0].SurfaceGeometry.SetVisibility(true);
            vms = fsr.StepOver();
            vms = fsr.StepOver();

            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"a");
            Obj objExecVal = mirror.GetWatchValue();
            Assert.AreNotEqual(10, objExecVal.Payload);
            vms = fsr.StepOver();


            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"a");
            objExecVal = mirror.GetWatchValue();
            Assert.AreNotEqual(20, objExecVal.Payload);
            vms = fsr.StepOver();
        }

        [Test]
        [Category("ExpressionInterpreterRunner"), Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void Defect_StepIn_734_2()
        {
            fsr.PreStart(
                 @"import(""ProtoGeometry.dll"");
                  [Imperative]
                  {
                        
                        Point.ByCoordinates(0, 0, 0);//line 5    
                        Point.ByCoordinates(10, 10, 10);    
                        Point.ByCoordinates(20, 10, 10);            
                        a : int = 10;//line 9    
                        a = a + 10;//line 10
                  }
                  ", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();   // sphere = Sphere.ByCenterPointRadius(Point.ByCoordinates(0, 0, 0), 1);
            vms = fsr.Step();   // surfaceGeom = sphere.Faces[0].SurfaceGeometry.SetVisibility(true);
            vms = fsr.Step();
            vms = fsr.Step();
            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"a");
            Obj objExecVal = mirror.GetWatchValue();
            Assert.AreNotEqual(10, objExecVal.Payload);
            vms = fsr.Step();


            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"a");
            objExecVal = mirror.GetWatchValue();
            Assert.AreNotEqual(20, objExecVal.Payload);
            vms = fsr.Step();

        }
        [Test]
        [Category("ExpressionInterpreterRunner"), Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void Defect_StepIn_734_3()
        {
            fsr.PreStart(
                 @"import(""ProtoGeometry.dll"");

                        pnt1 = Point.ByCoordinates(0, 0, 0);
                        pnt2 = Point.ByCoordinates(0, 0, 0); //Line 4
            
                        [Imperative]
                        {
                            pnt1.Translate(0, 10, 0);//Line 8
                            pnt2.Translate(0, 10, 0);
    
                            a : int = 10;
                            a = a + 10;//Line 12
                        }
                  ", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();   // sphere = Sphere.ByCenterPointRadius(Point.ByCoordinates(0, 0, 0), 1);
            vms = fsr.Step();   // surfaceGeom = sphere.Faces[0].SurfaceGeometry.SetVisibility(true);
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();

            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"a");
            Obj objExecVal = mirror.GetWatchValue();
            Assert.AreNotEqual(10, objExecVal.Payload);
            vms = fsr.Step();

            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"a");
            objExecVal = mirror.GetWatchValue();
            Assert.AreNotEqual(20, objExecVal.Payload);
            vms = fsr.Step();
        }
        [Test]
        [Category("ExpressionInterpreterRunner"), Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void Defect_StepIn_734_4()
        {
            fsr.PreStart(
                 @"import(""ProtoGeometry.dll"");

                        pnt1 = Point.ByCoordinates(0, 0, 0);
                        pnt2 = Point.ByCoordinates(0, 0, 0); //Line 4
            
                        [Imperative]
                        {
                            pnt1.Translate(0, 10, 0);//Line 8
                            pnt2.Translate(0, 10, 0);
    
                            a : int = 10;
                            a = a + 10;//Line 12
                        }
                  ", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();   // sphere = Sphere.ByCenterPointRadius(Point.ByCoordinates(0, 0, 0), 1);
            vms = fsr.Step();   // surfaceGeom = sphere.Faces[0].SurfaceGeometry.SetVisibility(true);
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"a");
            Obj objExecVal = mirror.GetWatchValue();
            Assert.AreNotEqual(10, objExecVal.Payload);
            vms = fsr.Step();

            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"a");
            objExecVal = mirror.GetWatchValue();
            Assert.AreNotEqual(20, objExecVal.Payload);
            vms = fsr.Step();



        }
        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void TestWatchExpressionForFFIProperty_2()
        {
            string src = @" 
class A
{
    a;
    constructor A()
    {
        a = 1;
    }
    
}

z = { A.A(), A.A() };

";

            fsr.PreStart(src, runnerConfig);
            DebugRunner.VMState vms = fsr.Step();   // z = { A.A(), A.A() };
            fsr.StepOver();                         // end of script

            // Watch "z.a"
            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"z.a[0]");
            Obj objExecVal = mirror.GetWatchValue();
            /*List<Obj> lo = mirror.GetArrayElements(objExecVal);
            Assert.AreNotEqual(null, objExecVal);
            Assert.AreEqual(mirror.GetType(lo[0]), "A");
            Assert.AreEqual(mirror.GetType(lo[1]), "A");*/
        }
        [Test]
        [Category("ExpressionInterpreterRunner")]
        [Category("Failure")]
        public void undefinedclass()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
            @"
                
                variableName : Line;", runnerConfig);
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3982
            DebugRunner.VMState vms = fsr.Step();

            vms = fsr.StepOver();
            vms = fsr.StepOver();


            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"variableName");
            Obj objExecVal = mirror.GetWatchValue();
            Assert.AreEqual(null, objExecVal);

        }

        [Test]
        [Category("ExpressionInterpreterRunner"), Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void Tempvariable_Associative_crash()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
            @"
                
                import(""ProtoGeometry.dll"");
                import(""DSCoreNodes.dll"");

                a : Point = null;
                b : Line = null;

                [Associative]
                {
                    a = Point.ByCoordinates(10, 0, 0);
                    b = Line.ByStartPointEndPoint(a, Point.ByCoordinates(10, 5, 0));
                    c=b.StartPoint.X;
                    a = Point.ByCoordinates(15, 0, 0);
                }", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();

            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();


            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"b");
            Obj objExecVal = mirror.GetWatchValue();
            Assert.IsNotNull(objExecVal);
            TestFrameWork.Verify(mirror, "c", 10.00, 0);

        }
        [Test]
        [Category("ExpressionInterpreterRunner"), Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void Tempvariable_Associative_2()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
            @"
                
                import(""ProtoGeometry.dll"");
                import(""DSCoreNodes.dll"");

                a : Point = null;
                b : Line = null;

                [Associative]
                {
                    b = Line.ByStartPointEndPoint(Point.ByCoordinates(10, 0, 0), Point.ByCoordinates(10, 5, 0));
                    c=b.StartPoint.X;
                    a = Point.ByCoordinates(15, 0, 0);
                }", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();


            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();


            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"b");
            Obj objExecVal = mirror.GetWatchValue();
            Assert.IsNotNull(objExecVal);
            TestFrameWork.Verify(mirror, "c", 10.00, 0);

        }
        [Test]
        [Category("ExpressionInterpreterRunner"), Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void Tempvariable_crash_imperative()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
            @"
                
                import(""ProtoGeometry.dll"");
                import(""DSCoreNodes.dll"");

                a : Point = null;
                b : Line = null;

                [Imperative]
                {
                    a = Point.ByCoordinates(10, 0, 0);
                    b = Line.ByStartPointEndPoint(a, Point.ByCoordinates(10, 5, 0));
                    c=b.StartPoint.X;
                    a = Point.ByCoordinates(15, 0, 0);
                }", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();

            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();


            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"b");
            Obj objExecVal = mirror.GetWatchValue();
            Assert.IsNotNull(objExecVal);
            TestFrameWork.Verify(mirror, "c", 10.00, 0);

        }

        [Test]
        [Category("ExpressionInterpreterRunner"), Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        public void Tempvariable_Imperative_2()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
            @"
                
                import(""ProtoGeometry.dll"");
                import(""DSCoreNodes.dll"");

                a : Point = null;
                b : Line = null;

                [Associative]
                {
                    b = Line.ByStartPointEndPoint(Point.ByCoordinates(10, 0, 0), Point.ByCoordinates(10, 5, 0));
                    c=b.StartPoint.X;
                    a = Point.ByCoordinates(15, 0, 0);
                }", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();


            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();


            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"b");
            Obj objExecVal = mirror.GetWatchValue();
            Assert.IsNotNull(objExecVal);
            TestFrameWork.Verify(mirror, "c", 10.00, 0);

        }

        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void ModifyAndReturnClassPropertyInsideFunction()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
            @"
class A
{
    x;

    constructor A ( )
    {
        x = { { 0, 0 } , { 1, 1 } };
    }
    
    def add()
    {
        x = 10;
        return = x;    
    }
}

y = A.A();
x = y.add(); 
", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();


            vms = fsr.StepOver();

            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"y");
            Obj objExecVal = mirror.GetWatchValue();
            Assert.IsNotNull(objExecVal);
            Assert.AreEqual(mirror.GetType(objExecVal), "A");
            //TestFrameWork.Verify(mirror, "y.x", new object[] { { 0, 0 }, { 1, 1 } }, 0);

            vms = fsr.Step();         // x = 10;

            Assert.AreEqual(13, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(13, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(16, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver();         // return = x;

            Assert.AreEqual(14, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(14, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(20, vms.ExecutionCursor.EndExclusive.CharNo);

            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"x");
            objExecVal = mirror.GetWatchValue();
            Assert.IsNotNull(objExecVal);
            Assert.AreEqual(objExecVal.Payload, 10);

            vms = fsr.Step();

            vms = fsr.Step();        // x = y.add();

            vms = fsr.Step();
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"x");
            objExecVal = mirror.GetWatchValue();
            Assert.IsNotNull(objExecVal);
            Assert.AreEqual(objExecVal.Payload, 10);

            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"y.x");
            objExecVal = mirror.GetWatchValue();
            Assert.IsNotNull(objExecVal);
            Assert.AreEqual(objExecVal.Payload, 10);
        }

        [Test]
        [Category("ExpressionInterpreterRunner")]
        [Category("Failure")]
        public void ModifyAndReturnClassPropertyInsideFunction_1()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
            @"
class A
{
    x;

    constructor A ( )
    {
        x = { { 0, 0 } , { 1, 1 } };
    }
    
    def add()
    {
          x[1][2] = 1;    
          return = x;    
    }
}

y = A.A();
x = y.add(); 
", runnerConfig);

            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3963
            Assert.Fail(" Debugger skips statements on stepping over property setters followed by return statements");

            DebugRunner.VMState vms = fsr.Step();


            vms = fsr.StepOver();

            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"y");
            Obj objExecVal = mirror.GetWatchValue();
            Assert.IsNotNull(objExecVal);
            Assert.AreEqual(mirror.GetType(objExecVal), "A");
            //TestFrameWork.Verify(mirror, "y.x", new object[] { { 0, 0 }, { 1, 1 } }, 0);

            vms = fsr.Step();         // x[1][2] = 1;

            Assert.AreEqual(13, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(13, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(16, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver();         // return = x;

            Assert.AreEqual(14, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(14, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(20, vms.ExecutionCursor.EndExclusive.CharNo);

            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"x[1][2]");
            objExecVal = mirror.GetWatchValue();
            Assert.IsNotNull(objExecVal);
            Assert.AreEqual(objExecVal.Payload, 1);

            vms = fsr.Step();

            vms = fsr.Step();        // x = y.add();

            vms = fsr.Step();
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"x");
            objExecVal = mirror.GetWatchValue();
            Assert.IsNotNull(objExecVal);
            Assert.AreEqual(objExecVal.Payload, 1);

            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"y.x[1][2]");
            objExecVal = mirror.GetWatchValue();
            Assert.IsNotNull(objExecVal);
            Assert.AreEqual(objExecVal.Payload, 1);
        }

        [Test]
        [Category("ExpressionInterpreterRunner")]
        [Category("Failure")]
        public void ModifyAndReturnClassPropertyInsideFunction_2()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
            @"
class A
{
    x;

    constructor A ( )
    {
        x = { { 0, 0 } , { 1, 1 } };
    }
    
}

def add(a : A)
{
      a.x[1][2] = 1;
      return = a.x;    
}

y = A.A();
x = add(y); 

", runnerConfig);

            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3963
            Assert.Fail(" Debugger skips statements on stepping over property setters followed by return statements");

            DebugRunner.VMState vms = fsr.Step();

            vms = fsr.StepOver();

            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"y");
            Obj objExecVal = mirror.GetWatchValue();
            Assert.IsNotNull(objExecVal);
            Assert.AreEqual(mirror.GetType(objExecVal), "A");
            //TestFrameWork.Verify(mirror, "y.x", new object[] { { 0, 0 }, { 1, 1 } }, 0);

            vms = fsr.Step();         // x[1][2] = 1;

            Assert.AreEqual(13, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(13, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(16, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver();         // return = x;

            Assert.AreEqual(14, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(14, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(20, vms.ExecutionCursor.EndExclusive.CharNo);

            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"x[1][2]");
            objExecVal = mirror.GetWatchValue();
            Assert.IsNotNull(objExecVal);
            Assert.AreEqual(objExecVal.Payload, 1);

            vms = fsr.Step();

            vms = fsr.Step();        // x = y.add();

            vms = fsr.Step();
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"x");
            objExecVal = mirror.GetWatchValue();
            Assert.IsNotNull(objExecVal);
            Assert.AreEqual(objExecVal.Payload, 1);

            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"y.x[1][2]");
            objExecVal = mirror.GetWatchValue();
            Assert.IsNotNull(objExecVal);
            Assert.AreEqual(objExecVal.Payload, 1);

        }

        [Test]
        [Category("ExpressionInterpreterRunner")]
        [Category("Failure")]
        public void ModifyAndReturnClassPropertyInsideFunction_3()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
            @"
class A
{
    x;

    constructor A ( )
    {
        x = { { 0, 0 } , { 1, 1 } };
    }
    
}

def add(a : A)
{
    a.x = 10;
    return = a.x;    
}

y = A.A();
x = add(y); 


", runnerConfig);

            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3963
            Assert.Fail("Debugger skips statements on stepping over property setters followed by return statements");

            DebugRunner.VMState vms = fsr.Step();


            vms = fsr.StepOver();

            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"y");
            Obj objExecVal = mirror.GetWatchValue();
            Assert.IsNotNull(objExecVal);
            Assert.AreEqual(mirror.GetType(objExecVal), "A");
            //TestFrameWork.Verify(mirror, "y.x", new object[] { { 0, 0 }, { 1, 1 } }, 0);

            vms = fsr.Step();         // x[1][2] = 1;

            Assert.AreEqual(13, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(13, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(16, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver();         // return = x;

            Assert.AreEqual(14, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(9, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(14, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(20, vms.ExecutionCursor.EndExclusive.CharNo);

            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"x[1][2]");
            objExecVal = mirror.GetWatchValue();
            Assert.IsNotNull(objExecVal);
            Assert.AreEqual(objExecVal.Payload, 1);

            vms = fsr.Step();

            vms = fsr.Step();        // x = y.add();

            vms = fsr.Step();
            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"x");
            objExecVal = mirror.GetWatchValue();
            Assert.IsNotNull(objExecVal);
            Assert.AreEqual(objExecVal.Payload, 1);

            watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            mirror = watchRunner.Execute(@"y.x[1][2]");
            objExecVal = mirror.GetWatchValue();
            Assert.IsNotNull(objExecVal);
            Assert.AreEqual(objExecVal.Payload, 1);

        }

        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void watchExpandClassinstance_758()
        {
            fsr.PreStart(
                 @"
                    class A
                        { 
                            a; 
                            constructor A() 
                            { 
                                a = 1;
                            }
                        }
                    z = { A.A() ,A.A()};
                    y = z.a;

                  ", runnerConfig);

            DebugRunner.VMState vms = fsr.StepOver();
            vms = fsr.StepOver();

            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"z");
            Obj objExecVal = mirror.GetWatchValue();
            Assert.AreNotEqual(null, objExecVal.Payload);

            ExpressionInterpreterRunner watchRunner2 = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror2 = watchRunner2.Execute(@"z");
            Obj objExecVal2 = mirror2.GetWatchValue();
            Assert.AreNotEqual(null, objExecVal2.Payload);


            ExpressionInterpreterRunner watchRunner3 = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror3 = watchRunner3.Execute(@"z.a");
            Obj objExecVal3 = mirror3.GetWatchValue();
            Assert.AreNotEqual(null, objExecVal3.Payload);

            ExpressionInterpreterRunner watchRunner4 = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror4 = watchRunner4.Execute(@"z.a");
            Obj objExecVal4 = mirror4.GetWatchValue();
            Assert.AreNotEqual(null, objExecVal3.Payload);




        }
        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void watchExpandClassinstanceImperative_758()
        {
            fsr.PreStart(
                 @"
                    class A
                        { 
                            a; 
                            constructor A() 
                            { 
                                a = 1;
                            }
                        }
                    [Imperative]
                    {
                        z = { A.A(), A.A() };
                        y = { z[0].a, z[1].a };
                    }

                  ", runnerConfig);

            DebugRunner.VMState vms = fsr.StepOver();
            vms = fsr.StepOver();

            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"z");
            Obj objExecVal = mirror.GetWatchValue();
            Assert.AreNotEqual(null, objExecVal.Payload);

            ExpressionInterpreterRunner watchRunner2 = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror2 = watchRunner2.Execute(@"z");
            Obj objExecVal2 = mirror2.GetWatchValue();
            Assert.AreNotEqual(null, objExecVal2.Payload);


            ExpressionInterpreterRunner watchRunner3 = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror3 = watchRunner3.Execute(@"z.a");
            Obj objExecVal3 = mirror3.GetWatchValue();
            Assert.AreNotEqual(null, objExecVal3.Payload);

            ExpressionInterpreterRunner watchRunner4 = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror4 = watchRunner4.Execute(@"z.a");
            Obj objExecVal4 = mirror4.GetWatchValue();
            Assert.AreNotEqual(null, objExecVal3.Payload);




        }
        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void watchExpandClassinstance_StepIn_758()
        {
            fsr.PreStart(
                 @"
                    class A
                        { 
                            a; 
                            constructor A() 
                            { 
                                a = 1;
                            }
                        }
                    z = { A.A() ,A.A()};
                    y = z.a;

                  ", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"z");
            Obj objExecVal = mirror.GetWatchValue();
            Assert.AreNotEqual(null, objExecVal.Payload);

            ExpressionInterpreterRunner watchRunner2 = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror2 = watchRunner2.Execute(@"z");
            Obj objExecVal2 = mirror2.GetWatchValue();
            Assert.AreNotEqual(null, objExecVal2.Payload);


            ExpressionInterpreterRunner watchRunner3 = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror3 = watchRunner3.Execute(@"z.a");
            Obj objExecVal3 = mirror3.GetWatchValue();
            Assert.AreNotEqual(null, objExecVal3.Payload);

            ExpressionInterpreterRunner watchRunner4 = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror4 = watchRunner4.Execute(@"z.a");
            Obj objExecVal4 = mirror4.GetWatchValue();
            Assert.AreNotEqual(null, objExecVal3.Payload);




        }
        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void watchExpandClassinstance_Imperative_StepIn_758()
        {
            fsr.PreStart(
                 @"
                    class A
                        { 
                            a; 
                            constructor A() 
                            { 
                                a = 1;
                            }
                        }
                    [Imperative]
                    {
                        z = { A.A() ,A.A()};
                        y = z.a;
                    }

                  ", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"z");
            Obj objExecVal = mirror.GetWatchValue();
            Assert.AreNotEqual(null, objExecVal.Payload);

            ExpressionInterpreterRunner watchRunner2 = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror2 = watchRunner2.Execute(@"z");
            Obj objExecVal2 = mirror2.GetWatchValue();
            Assert.AreNotEqual(null, objExecVal2.Payload);


            ExpressionInterpreterRunner watchRunner3 = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror3 = watchRunner3.Execute(@"z.a");
            Obj objExecVal3 = mirror3.GetWatchValue();
            Assert.AreNotEqual(null, objExecVal3.Payload);

            ExpressionInterpreterRunner watchRunner4 = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror4 = watchRunner4.Execute(@"z.a");
            Obj objExecVal4 = mirror4.GetWatchValue();
            Assert.AreNotEqual(null, objExecVal3.Payload);

        }
    }

    [TestFixture]
    public class BasicUseCaseTests
    {
        public ProtoCore.Core core;
        private DebugRunner fsr;
        private ProtoScript.Config.RunConfiguration runnerConfig;
        private string testPath = @"..\..\..\test\Engine\ProtoTest\ImportFiles\";

        [SetUp]
        public void Setup()
        {
            // Specify some of the requirements of IDE.
            var options = new ProtoCore.Options();
            options.ExecutionMode = ProtoCore.ExecutionMode.Serial;
            options.SuppressBuildOutput = false;

            core = new ProtoCore.Core(options);
            core.Compilers.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Compiler(core));
            core.Compilers.Add(ProtoCore.Language.kImperative, new ProtoImperative.Compiler(core));

            runnerConfig = new ProtoScript.Config.RunConfiguration();
            runnerConfig.IsParrallel = false;
            fsr = new DebugRunner(core);

            DLLFFIHandler.Register(FFILanguage.CSharp, new CSModuleHelper());
            CLRModuleType.ClearTypes();
        }

        //To test the update order issue in assoc. code, relates to DNL-1467407
        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void UseCaseTesting_Simple_cross_language_update_Imp_in_Assoc_2()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
@"
a = 7;
[Imperative]
{
    c = 0;    
    [Associative]
    {
        s = Print(""aa = "" + a);
        c = a + 2;
        a = 10;    
    }    
}
", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();

            Obj o_c = vms.mirror.GetDebugValue("c");
            Obj o_a = vms.mirror.GetDebugValue("a");
            string type_c = vms.mirror.GetType("c");
            string type_a = vms.mirror.GetType("a");


            Assert.IsTrue(type_c == "int");
            Assert.IsTrue(type_a == "int");
            Assert.IsTrue((Int64)o_c.Payload == 9);
            Assert.IsTrue((Int64)o_a.Payload == 7);

            vms = fsr.Step();
            o_a = vms.mirror.GetDebugValue("a");
            Assert.IsTrue((Int64)o_a.Payload == 10);
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            vms = fsr.Step();
            o_c = vms.mirror.GetDebugValue("c");
            Assert.IsTrue((Int64)o_c.Payload == 12);

        }

        //Investigate the color object properties number, relates to IDE-493
        [Test]
        [Category("ExpressionInterpreterRunner")]
        [Category("ReleaseCriteria")]
        public void UseCase_Robert_simple_copy_and_modiy_collection_1()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
@"
a = 0..10;
b = a;
b[2] = 100; // modifying a member of a  copy of a collections
c = a;
d = b[0..(Count(b) - 1)..2]; // rnage expression used for indexing into a collection
           
", runnerConfig);


            DebugRunner.VMState vms = fsr.Step();

            vms = fsr.StepOver();
            vms = fsr.StepOver();

            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"a");

            Obj objExecVal = mirror.GetWatchValue();

            List<Obj> lo = mirror.GetArrayElements(objExecVal);
            String type = objExecVal.GetType().ToString();

            TestFrameWork.Verify(mirror, "a", new object[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 0);
            TestFrameWork.Verify(mirror, "b", new object[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 0);
            vms = fsr.StepOver();
            TestFrameWork.Verify(mirror, "b", new object[] { 0, 1, 100, 3, 4, 5, 6, 7, 8, 9, 10 }, 0);
            vms = fsr.StepOver();
            TestFrameWork.Verify(mirror, "c", new object[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 0);
            vms = fsr.StepOver();
            TestFrameWork.Verify(mirror, "d", new object[] { 0, 100, 4, 6, 8, 10 }, 0);



        }

        [Test]
        [Category("ExpressionInterpreterRunner")]
        [Category("ReleaseCriteria")]
        public void UseCase_Robert_modifiying_replicated_inline_conditional_1()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
@"

a = 0..10..2;
a = a < 5 ? 3 : 4;
           
", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();

            vms = fsr.StepOver();

            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"a");

            Obj objExecVal = mirror.GetWatchValue();

            List<Obj> lo = mirror.GetArrayElements(objExecVal);
            String type = objExecVal.GetType().ToString();

            TestFrameWork.Verify(mirror, "a", new object[] { 0, 2, 4, 6, 8, 10 }, 0);
            vms = fsr.StepOver();
            TestFrameWork.Verify(mirror, "a", new object[] { 3, 3, 3, 4, 4, 4 }, 0);


        }
        [Test]
        [Category("ExpressionInterpreterRunner"), Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        [Category("ReleaseCriteria")]
        public void UseCase_Robert_simple_numeric_imperative_2()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
@"
import(""ProtoGeometry.dll"");

a : int;
b : int;

[Imperative]
{
	a = 10;
	b = 2 * a;
	a = a + 1;
}
           
", runnerConfig);


            DebugRunner.VMState vms = fsr.Step();
            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"a");
            Obj objExecVal = mirror.GetWatchValue();

            TestFrameWork.Verify(mirror, "a", null, 0);
            TestFrameWork.Verify(mirror, "b", null, 0);

            vms = fsr.StepOver();

            TestFrameWork.Verify(mirror, "a", 10, 0);

            vms = fsr.Step();
            TestFrameWork.Verify(mirror, "b", 20, 0);

            vms = fsr.Step();
            TestFrameWork.Verify(mirror, "a", 11, 0);

        }
        [Test]
        [Category("ExpressionInterpreterRunner"), Category("ProtoGeometry")] [Ignore] [Category("PortToCodeBlocks")]
        [Category("ReleaseCriteria")]
        public void UseCase_Robert_simple_numeric_associative_2()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
@"
import(""ProtoGeometry.dll"");

a : int;
b : int;

[Associative]
{
    a = 10;
    b = 2 * a;
    a = a + 1;
}
           
", runnerConfig);


            DebugRunner.VMState vms = fsr.Step();

            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"a");
            TestFrameWork.Verify(mirror, "a", null, 0);

            vms = fsr.StepOver();

            TestFrameWork.Verify(mirror, "a", 10, 0);

            vms = fsr.Step();
            TestFrameWork.Verify(mirror, "b", 20, 0);

            vms = fsr.Step();
            TestFrameWork.Verify(mirror, "a", 11, 0);

            vms = fsr.Step();
            TestFrameWork.Verify(mirror, "b", 22, 0);

        }
        [Test]
        [Category("ExpressionInterpreterRunner")]
        [Category("ReleaseCriteria")]
        public void Simple_debug()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
@"
class Tuple4

{

  
    

    public def Equals : bool (other : Tuple4)

    {

        return =true;
          

    }

}
                
t1 = Tuple4.Tuple4();
t2 = Tuple4.Tuple4();
b = t1.Equals(t2);
           
", runnerConfig);

            DebugRunner.VMState vms = fsr.Step();

            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();

            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"b");
            Obj objExecVal = mirror.GetWatchValue();
            TestFrameWork.Verify(mirror, "b", true, 0);


        }
        [Test]
        [Category("Debugger")]
        public void breakPoint_Cursor_1471()
        {
            string src = @"
            class Point
            {
    
            }

            p = 0..10..#5;

            def foo()
            {
                return = 5;
            }
            isPass5 = foo() == 5 ? true : false; // verification


            startPts = Point.Point();
            isPass6 = foo()  == 5 ? true : false ; // verification

            endPts = foo()  >= 1 ? Point.Point() : Point.Point();
            isPass7 = foo()  == 5 ? true : false ; // verification";

            fsr.PreStart(src, runnerConfig);
            ProtoCore.CodeModel.CodePoint cp = new ProtoCore.CodeModel.CodePoint
            {
                LineNo = 13,
                CharNo = 10
            };
            fsr.ToggleBreakpoint(cp);

            fsr.Run();
            DebugRunner.VMState vms = fsr.Run();

            Assert.IsTrue(vms.isEnded);


        }
    }
}
