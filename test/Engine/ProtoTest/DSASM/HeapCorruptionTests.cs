using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProtoCore.DSASM;
using ProtoTest.TD;
using ProtoTestFx.TD;
using ProtoScript.Runners;
namespace ProtoTest.DSASM
{
    [TestFixture]
    class HeapCorruptionTests : ProtoTestBase
    {
        public override void Setup()
        {
            base.Setup();
            thisTest.SetupTestCore();
        }

        [Test]
        [Category("HeapCorruptionTests")]
        public void TestArray01()
        {
            string code = @"a = {10, 20, 30};";
            thisTest.RunScriptSource(code);

            Assert.IsFalse(thisTest.GetTestRuntimeCore().RuntimeMemory.Heap.IsHeapCyclic());
        }


        [Test]
        [Category("HeapCorruptionTests")]
        public void TestClassArray01()
        {
            string code = @"
class Obj
{
    x : int;
    constructor Obj(i:int)
    {
        x = i;
    }
}

p = {Obj.Obj(1),Obj.Obj(2),Obj.Obj(3)};
";
            thisTest.RunScriptSource(code);

            Assert.IsFalse(thisTest.GetTestRuntimeCore().RuntimeMemory.Heap.IsHeapCyclic());
        }

        [Test]
        public void TestCyclicPointer01()
        {
            var core = thisTest.GetTestCore();
            DebugRunner fsr = new DebugRunner(core);
            // Execute and verify the main script in a debug session
            fsr.PreStart(
            @"
class Obj
{
    x : var;
    constructor Obj()
    {
        x = null;
    }
}

p = Obj.Obj();
p.x = p;        // Assign the member x to its own 'this' pointer. This creates a cycle
m = p.x;

            ");

            DebugRunner.VMState vms = fsr.StepOver();
            vms = fsr.StepOver();
            vms = fsr.StepOver();

            // Test the heap contains a cycle
            Assert.IsTrue(core.Heap.IsHeapCyclic());

        }
    }
}
