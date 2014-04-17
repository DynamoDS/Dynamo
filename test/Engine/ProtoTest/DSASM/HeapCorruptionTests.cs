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
    public class HeapCorruptionTests
    {
        public TestFrameWork thisTest = new TestFrameWork();
        ProtoCore.Core core = null;

        [SetUp]
        public void SetUp()
        {
            core = thisTest.SetupTestCore();
        }


        [Test]
        [Category("HeapCorruptionTests")]
        public void TestArray01()
        {
            string code = @"a = {10, 20, 30};";
            thisTest.RunScriptSource(code);

            Assert.IsFalse(ProtoCore.Utils.HeapUtils.IsHeapCyclic(core));
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

            Assert.IsFalse(ProtoCore.Utils.HeapUtils.IsHeapCyclic(core));
        }

        [Test]
        [Category("HeapCorruptionTests")]
        public void TestBuildManualCyclicPointer01()
        {
            int allocationSize = 1;

            // Allocate a pointer
            int ptr = core.Rmem.Heap.Allocate(allocationSize);

            // Pointer data that points to its own heap element
            int pointerData = ptr;

            // Build a pointer whose data points to its own heap element
            StackValue sv = StackValue.BuildPointer(pointerData);
            core.Rmem.Heap.IncRefCount(sv);
            core.Rmem.Heap.Heaplist[ptr].Stack[0] = sv;

            // Verify the heap contains a cycle
            Assert.IsTrue(ProtoCore.Utils.HeapUtils.IsHeapCyclic(core));
        }

        [Test]
        [Category("HeapCorruptionTests")]
        public void TestBuildManualCyclicPointer02()
        {
            int allocationSize = 1;

            // Allocate 2 pointers
            int ptr1 = core.Rmem.Heap.Allocate(allocationSize);
            int ptr2 = core.Rmem.Heap.Allocate(allocationSize);

            // Build a pointer whose data points to its own heap element
            StackValue svPtr1 = StackValue.BuildPointer(ptr2);
            core.Rmem.Heap.IncRefCount(svPtr1);
            core.Rmem.Heap.Heaplist[ptr1].Stack[0] = svPtr1;


            // Build a 2nd pointer that points to the first pointer
            StackValue svPtr2 = StackValue.BuildPointer(ptr1);
            core.Rmem.Heap.IncRefCount(svPtr2);
            core.Rmem.Heap.Heaplist[ptr2].Stack[0] = svPtr2;

            // Verify the heap contains a cycle
            Assert.IsTrue(ProtoCore.Utils.HeapUtils.IsHeapCyclic(core));
        }

        [Test]
        public void TestCyclicPointer01()
        {
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
            Assert.IsTrue(ProtoCore.Utils.HeapUtils.IsHeapCyclic(core));

        }
    }
}
