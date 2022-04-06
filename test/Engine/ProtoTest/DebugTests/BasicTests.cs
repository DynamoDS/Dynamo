using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ProtoCore.DSASM;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
using ProtoFFI;
using ProtoScript.Runners;
using ProtoTestFx.TD;

namespace ProtoTest.DebugTests
{
    [TestFixture, Category("Debugger")]
    class BasicTests : ProtoTestBase
    {
        private DebugRunner fsr;
        private string testPath = @"..\..\..\test\Engine\ProtoTest\ImportFiles\";

        public override void Setup()
        {
            base.Setup();
            core.Options.kDynamicCycleThreshold = 5;

            fsr = new DebugRunner(core);

            DLLFFIHandler.Register(FFILanguage.CSharp, new CSModuleHelper());
            CLRModuleType.ClearTypes();
        }

        public override void TearDown()
        {
            runtimeCore = fsr.runtimeCore;
            base.TearDown();
        }

        private List<Obj> UnpackToList(ExecutionMirror mirror, Obj obj)
        {
            if (obj == null || !obj.DsasmValue.IsArray)
                return null;

            return mirror.MirrorTarget.rmem.Heap.ToHeapObject<DSArray>(obj.DsasmValue).Values.Select(x => mirror.Unpack(x)).ToList();
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

");
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
");

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
        [Category("Failure")]
        [Category("ExpressionInterpreterRunner")]
        public void TestWatchExpression5()
        {
            string sourceCode = @"
a = [ 1, 0, 0.0 ];  // Line 2
b = [ a, 1 ];       // Line 3

[Imperative]        // Line 5
{
    a[1] = 1;       // Line 7
    m = a;          // Line 8
}                   // Line 9
";
            fsr.PreStart(sourceCode);
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
        [Category("Failure")]
        [Category("PopRxOptimization")]
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
");

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
        [Category("Failure")]
        [Category("PopRxOptimization")]
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
");

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

        [Test, Category("Failure")]
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
");

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
                Assert.AreEqual(null, objExecVal.Payload);
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
                Assert.AreEqual(0, (Int64)objExecVal.Payload);
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
");

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

");
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
");

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
");

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
            fsr.PreStart(code);

            DebugRunner.VMState vms = fsr.Step();
            vms = fsr.Step();
            Object o = vms.mirror.GetDebugValue("foo").Payload;// .GetDebugValue("foo").Payload;
            Assert.IsTrue((Int64)o == 5);
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
            fsr.PreStart(code);
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
        [Category("Failure")]
        [Category("PopRxOptimization")]
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
            fsr.PreStart(code);
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
        [Category("Failure")]
        [Category("PopRxOptimization")]
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
            fsr.PreStart(code);
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
        [Category("Failure")]
        [Category("PopRxOptimization")]
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

            fsr.PreStart(sourceCode);

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
            fsr.PreStart(sourceCode);

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
            fsr.PreStart(sourceCode);

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
            fsr.PreStart(sourceCode);

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
        [Category("Failure")]
        [Category("PopRxOptimization")]
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
            fsr.PreStart(sourceCode);

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
        [Category("Failure")]
        [Category("PopRxOptimization")]
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
            fsr.PreStart(sourceCode);

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
        [Category("Failure")]
        [Category("PopRxOptimization")]
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

            fsr.PreStart(sourceCode);

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
        [Category("Failure")]
        [Category("PopRxOptimization")]
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

p = [ 9, 0 ];
";
            fsr.PreStart(code);
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
            List<Obj> lo = UnpackToList(vms.mirror, o);
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

            fsr.PreStart(code);
            //fsr.Step();

            fsr.Run();
            Assert.AreEqual(fsr.isEnded, true);
        }

        [Test]
        [Category("Failure")]
        [Category("PopRxOptimization")]
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
            fsr.PreStart(code);
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
");

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
            fsr.PreStart(code);
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
            fsr.PreStart(code);
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

            fsr.PreStart(code);
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
        [Category("Failure")]
        [Category("PopRxOptimization")]
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
            fsr.PreStart(code);
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
            fsr.PreStart(code);
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

            fsr.PreStart(code);
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
        [Category("Failure")]
        [Category("PopRxOptimization")]
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
            fsr.PreStart(source);

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

            fsr.PreStart(sourceCode);
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

            fsr.PreStart(sourceCode);
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
    boo = [ 1, 2 ];
    for(index in boo) {
        x = x + 1;
    }
}
";

            fsr.PreStart(sourceCode);
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
    for(index in [ 1, 2 ]) {
        x = x + 1;
    }
}
";

            fsr.PreStart(sourceCode);
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
            fsr.PreStart(code);
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
            fsr.PreStart(code);
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
        [Category("Failure")]
        [Category("PopRxOptimization")]
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

            fsr.PreStart(code);
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
            fsr.PreStart(code);
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
            fsr.PreStart(code);

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
            fsr.PreStart(code);
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
            fsr.PreStart(code);
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
        [Category("Failure")]
        [Category("PopRxOptimization")]
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
            fsr.PreStart(code);
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
        [Category("Failure")]
        [Category("PopRxOptimization")]
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
            fsr.PreStart(code);
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
            List<Obj> lo = UnpackToList(vms.mirror, o);
            Assert.IsTrue(type == "array");
            Assert.IsTrue((Int64)lo[0].Payload == 2);
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
            fsr.PreStart(code);
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

            fsr.PreStart(code);

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

            fsr.PreStart(code);

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
        public void MirrorApiTest002()
        {
            string src = @"
                            a = [ 1, 2, 3, [ 4, 5, 6 ], 7, 8 ];
                         ";
            fsr.PreStart(src);
            fsr.Step();
            DebugRunner.VMState vms = fsr.Step();
            Obj o = vms.mirror.GetDebugValue("a");
            string type = vms.mirror.GetType(o);
            List<Obj> os = UnpackToList(vms.mirror, o);

            Assert.IsTrue(type == "array");
            Assert.IsTrue(os.Count == 6);
            Assert.IsTrue((Int64)UnpackToList(vms.mirror, os[3])[1].Payload == 5);
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

            fsr.PreStart(src);
            DebugRunner.VMState vms = fsr.StepOver();
            vms = fsr.StepOver();
            Obj o = vms.mirror.GetDebugValue("b");
            Assert.IsTrue((Int64)o.Payload == 3);
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
            fsr.PreStart(src);
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

            fsr.PreStart(src);
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

            fsr.PreStart(src);
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

                        a = [ 1, 4, -2 ]; // arbitrary collection

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
            fsr.PreStart(src);
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
            fsr.PreStart(src);
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

            fsr.PreStart(src);
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

            fsr.PreStart(code);
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
        [Category("Failure")]
        [Category("Debugger")]
        public void TestFFISetPropertyAssociative()
        {
            String code =
            @"
import(DummyBase from ""FFITarget.dll"");

dummy = DummyBase.Create();
dummy.Value = 868760;
a = dummy.Value;";

            fsr.PreStart(code);
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
        [Category("Failure")]
        [Category("PopRxOptimization")]
        [Category("Debugger")]
        public void ImportTest001()
        {
            string src =
                Path.GetFullPath(string.Format("{0}{1}", testPath, "ImportTest001.ds"));
            string imp1 =
                Path.GetFullPath(string.Format("{0}{1}", testPath, "import001.ds"));
            string imp2 =
                Path.GetFullPath(string.Format("{0}{1}", testPath, "import002.ds"));

            fsr.LoadAndPreStart(src);

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
        [Category("Failure")]
        [Category("PopRxOptimization")]
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

            fsr.PreStart(src);
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
            o = vms.mirror.GetDebugValue("c");
            type = vms.mirror.GetType("c");
            Assert.IsTrue(type == "int");
            Assert.IsTrue((Int64)o.Payload == 30);
        }

        [Test]
        [Category("PopRxOptimization")]
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
            fsr.PreStart(src);
            DebugRunner.VMState vms = fsr.Step();

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
        [Category("Failure")]
        [Category("PopRxOptimization")]
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

            fsr.PreStart(src);

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

            vms = fsr.Step();
            o = vms.mirror.GetDebugValue("a");
            Assert.IsTrue((Int64)o.Payload == 100);
        }


        [Test]
        [Category("Failure")]
        [Category("PopRxOptimization")]
        [Category("Debugger")]
        public void LanguageBlockInsideFunction5()
        {
            string src =
@"gg = 0;

def foo ()
{
    arr = [ [ ] ];

    [Imperative]
    {
       
        for(i in [0, 1])
        {
            [Associative]
            {
                gg = i;
                arr[i] = [1, 2];
            } 
        }
    }
    return = arr;
}
test = foo();";

            fsr.PreStart(src);
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

            List<Obj> ol = UnpackToList(vms.mirror, o);
            Assert.IsTrue(ol.Count == 1);
            List<Obj> ol_1 = UnpackToList(mirror, ol[0]);
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
            ol = UnpackToList(vms.mirror, o);
            //Assert.IsTrue(ol.Count == 2);
            List<Obj> ol_2 = UnpackToList(vms.mirror, ol[1]);
            Assert.IsTrue(ol_2.Count == 2);
            Assert.IsTrue((Int64)ol_2[0].Payload == 1);
            Assert.IsTrue((Int64)ol_2[1].Payload == 2);

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
            ol = UnpackToList(vms.mirror, o);
            Assert.IsTrue(ol.Count == 2);

            ol_1 = UnpackToList(vms.mirror, ol[0]);
            Assert.IsTrue(ol_1.Count == 2);
            Assert.IsTrue((Int64)ol_1[0].Payload == 1);
            Assert.IsTrue((Int64)ol_1[1].Payload == 2);

            ol_2 = UnpackToList(vms.mirror, ol[1]);
            Assert.IsTrue(ol_2.Count == 2);
            Assert.IsTrue((Int64)ol_2[0].Payload == 1);
            Assert.IsTrue((Int64)ol_2[1].Payload == 2);
        }

        [Test]
        [Category("Debugger")]
        public void FunctionPointer1()
        {
            string src =
@"arr = [ 3, 5, 1, 5, 3, 4, 7, true, 5, null, 12];
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
            fsr.PreStart(src);
            fsr.Step();

            DebugRunner.VMState vms = fsr.Step();

            Obj o = vms.mirror.GetDebugValue("arr");
            string type = vms.mirror.GetType("arr");
            List<Obj> lo = UnpackToList(vms.mirror, o);
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
            lo = UnpackToList(vms.mirror, o);
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
        public void HighlightingFunctionsInArrayAssociative1_Defect_IDE_578()
        {
            string src =
        @"
def f(a : int)
{
    return = a;
}

arr = [ f(99), f(87) ];
b = 2;";
            fsr.PreStart(src);
            DebugRunner.VMState vms = fsr.Step();

            Assert.AreEqual(7, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(7, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(24, vms.ExecutionCursor.EndExclusive.CharNo);

            vms = fsr.StepOver();

            Assert.AreEqual(8, vms.ExecutionCursor.StartInclusive.LineNo);
            Assert.AreEqual(1, vms.ExecutionCursor.StartInclusive.CharNo);
            Assert.AreEqual(8, vms.ExecutionCursor.EndExclusive.LineNo);
            Assert.AreEqual(7, vms.ExecutionCursor.EndExclusive.CharNo);
        }

        [Test]
        [Category("Failure")]
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
    arr = [ f(99), f(87) ];
    b = 2;
}";
            fsr.PreStart(src);
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
        [Category("Failure")]
        [Category("PopRxOptimization")]
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

            fsr.PreStart(src);
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
            fsr.PreStart(src);
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
            fsr.PreStart(src);
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
            fsr.PreStart(src);
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
            fsr.PreStart(src);
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
        [Category("Failure")]
        [Category("PopRxOptimization")]
        [Category("Debugger")]
        public void TestStepInSimpleFunction()
        {
            string src =
              @"def foo : int(a : int)
                {
                    return = a;
                }
                c1 = foo(10);";

            fsr.PreStart(src);
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
        [Category("Failure")]
        [Category("PopRxOptimization")]
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
            fsr.PreStart(src);
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
        [Category("Failure")]
        [Category("PopRxOptimization")]
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
            fsr.PreStart(src);
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
        [Category("Failure")]
        [Category("PopRxOptimization")]
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
            fsr.PreStart(src);
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
        [Test, Category("Failure")]
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
            fsr.PreStart(src);
            DebugRunner.VMState vms = fsr.Step();


            fsr.Run();
            Assert.AreEqual(fsr.isEnded, true);
            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"a");
            TestFrameWork.Verify(mirror, "b", new[] {2, 3, 4});
            TestFrameWork.Verify(mirror, "d", new[] { 2, 3, 4 });
            TestFrameWork.Verify(mirror, "c", new[] { 1, 2, 3 });
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
            fsr.PreStart(src);
            DebugRunner.VMState vms = fsr.Step();
            fsr.Run();
            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"a");
            TestFrameWork.Verify(mirror, "b", 1);
            TestFrameWork.Verify(mirror, "c", 2);
            TestFrameWork.Verify(mirror, "d", 3);
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
");

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
        
        [Test, Category("Failure")]
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

            ");
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
                Assert.AreEqual(0, (Int64)objExecVal.Payload);
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

            ");
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
        [Category("Debugger")]
        public void Defect_IDE_543_1()
        {
            fsr.PreStart(
                 @"[Imperative]
{
    a = 1..8..1;
}");
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
}");
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
}");
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
}");
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
}");
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
}");
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
}");
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
}");
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
        public void Defect_IDE_656_4_stepOver()
        {
            fsr.PreStart(
                @"c = [ 1, 2, 20 ];
def f(a)
{
    return = a;
}
x = f(c) > 5 ? 1 : 2;
b = 2;");

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
                @"c = [ 1, 2, 20 ];
def f(a)
{
    return = a;
}
x = f(c) > 5 ? 1 : 2;
b = 2;");

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
        [Category("Failure")]
        public void Defect_IDE_722()
        {
            fsr.PreStart(
                @"def foo : int(a : int, b : int)
{
    return = x = a > b ? a : b;
}
c1 = foo(10, 3);
Print(c1);");

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
}");

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
           
");

            DebugRunner.VMState vms = fsr.Step();

            vms = fsr.StepOver();


            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"a");
            Obj objExecVal = mirror.GetWatchValue();

            TestFrameWork.Verify(mirror, "a", 0);
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();

            TestFrameWork.Verify(mirror, "b", null);

        }

        [Test, Category("Failure")]
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
           
");

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

            TestFrameWork.Verify(mirror, "k", 0);
            TestFrameWork.Verify(mirror, "r", 0);
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();
            TestFrameWork.Verify(mirror, "k", 0);
            TestFrameWork.Verify(mirror, "r", 0);

        }
        [Test, Category("Failure")]
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
           
");

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
        [Test, Category("Failure")]
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
           
");

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
            TestFrameWork.Verify(mirror, "r", 0, 0);

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
           
");

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
        [Category("Failure")]
        [Category("PopRxOptimization")]
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
           
");

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
        [Category("Failure")]
        [Category("PopRxOptimization")]
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
           
");

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
           
");


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
           
");

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
        [Category("Failure")]
        [Category("PopRxOptimization")]
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
           
");

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
           
");

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
        [Category("Failure")]
        [Category("PopRxOptimization")]
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
           
");

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
           
");

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
        [Category("Failure")]
        [Category("PopRxOptimization")]
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
           
");

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

        public void inlineconditional_stepnext_656_12()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
 @"
def GetCoor(type : int)
{
    return = type == 1 ? 10 : 20;
}

list1 = [ 1, 2 ];

list3 = GetCoor(list1);
           
");

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

list1 = [ 1, 2 ];

list3 = GetCoor(list1);
           
");

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
        [Category("Failure")]
        [Category("PopRxOptimization")]
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
");

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
        [Category("Failure")]
        [Category("PopRxOptimization")]
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
");

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
        [Category("Failure")]
        [Category("ExpressionInterpreterRunner")]
        public void IDE_Debugger_698_3()
        {
            fsr.PreStart( // Execute and verify the main script in a debug session
    @"
results = [ [ ] ];

numCycles = 4;
s;
[Imperative]
{
    k = 0;
    while(k < 2)
    {
    for(i in (0..(numCycles)))
        {
        results[i] = [ ];
    
                for(j in(0..(numCycles-1)))
                {
                results[i][j] = i * j;
                }

            c = j;
        }
        k = k + 1;
    }
}
");

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
        [Category("ExpressionInterpreterRunner")]
        public void TestWatchExpressionForFFIProperty()
        {
            string src = @" 
import(Dummy from ""FFITarget.dll"");
a = Dummy.Create(2);
b = 2;
";

            fsr.PreStart(src);
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
    afinal = Dummy.Create(2);
    
    a = afinal;

}
";

            fsr.PreStart(src);
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
    }

    [TestFixture, Category("Debugger")]
    public class BasicUseCaseTests
    {
        public ProtoCore.Core core;
        private DebugRunner fsr;
        private string testPath = @"..\..\..\test\Engine\ProtoTest\ImportFiles\";

        [SetUp]
        public void Setup()
        {
            // Specify some of the requirements of IDE.
            var options = new ProtoCore.Options();
            options.ExecutionMode = ProtoCore.ExecutionMode.Serial;

            core = new ProtoCore.Core(options);
            core.Compilers.Add(ProtoCore.Language.Associative, new ProtoAssociative.Compiler(core));
            core.Compilers.Add(ProtoCore.Language.Imperative, new ProtoImperative.Compiler(core));

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
");

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
        [Category("Failure")]
        [Category("ExpressionInterpreterRunner")]
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
           
");


            DebugRunner.VMState vms = fsr.Step();

            vms = fsr.StepOver();
            vms = fsr.StepOver();

            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"a");

            Obj objExecVal = mirror.GetWatchValue();

            String type = objExecVal.GetType().ToString();

            TestFrameWork.Verify(mirror, "a", new object[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 0);
            vms = fsr.StepOver();
            TestFrameWork.Verify(mirror, "b", new object[] { 0, 1, 100, 3, 4, 5, 6, 7, 8, 9, 10 }, 0);
            vms = fsr.StepOver();
            TestFrameWork.Verify(mirror, "c", new object[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 0);
            vms = fsr.StepOver();
            TestFrameWork.Verify(mirror, "d", new object[] { 0, 100, 4, 6, 8, 10 }, 0);



        }

        [Test]
        [Category("ExpressionInterpreterRunner")]
        public void UseCase_Robert_modifiying_replicated_inline_conditional_1()
        {
            // Execute and verify the main script in a debug session
            fsr.PreStart(
@"

a = 0..10..2;
a = a < 5 ? 3 : 4;
           
");

            DebugRunner.VMState vms = fsr.Step();

            vms = fsr.StepOver();

            ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core, fsr.runtimeCore);
            ExecutionMirror mirror = watchRunner.Execute(@"a");

            TestFrameWork.Verify(mirror, "a", new object[] { 0, 2, 4, 6, 8, 10 }, 0);
            vms = fsr.StepOver();
            TestFrameWork.Verify(mirror, "a", new object[] { 3, 3, 3, 4, 4, 4 }, 0);
        }
    }
}
