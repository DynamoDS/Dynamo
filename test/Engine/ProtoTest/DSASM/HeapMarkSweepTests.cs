using System;
using System.Collections.Generic;

using NUnit.Framework;
using ProtoCore;
using ProtoCore.DSASM;
using ProtoTestFx.TD;
namespace ProtoTest.DSASM
{
    [TestFixture]
    public class HeapMarkAndSweepTests
    {
        private class TestExecutive : ProtoCore.DSASM.Executive
        {
            public TestExecutive(RuntimeCore runtimeCore) : base(runtimeCore)
            {
            }
        }

        private readonly TestFrameWork thisTest = new TestFrameWork();
        private Core testCore;
        private RuntimeCore testRuntimeCore;
        private TestExecutive testExecutive;

        [SetUp]
        public void SetUp()
        {
            testCore = thisTest.SetupTestCore();

            testRuntimeCore = new RuntimeCore(testCore.Heap, testCore.Options);
            testExecutive = new TestExecutive(testRuntimeCore);
        }

        [TearDown]
        public void CleanUp()
        {
            testRuntimeCore.Cleanup();
        }

        /// <summary>
        /// Test basic mark and sweep for pointer and string.
        /// </summary>
        [Test]
        public void TestBasic()
        {
            var heap = new Heap();
            var values = new StackValue[]
            {
                StackValue.BuildInt(0),
                StackValue.BuildInt(1),
                StackValue.BuildInt(2)
            };

            var array = heap.AllocateArray(values);
            var str = heap.AllocateString("hello world");

            heap.FullGC(new List<StackValue>(), testExecutive);
            Assert.IsNull(heap.ToHeapObject<DSArray>(array));
            Assert.IsNull(heap.ToHeapObject<DSArray>(str));
        }

        /// <summary>
        /// Test the GC string management when the stack references the same string multiple times.
        /// </summary>
        [Test]
        public void TestGCStringCleanup()
        {
            // Simulating the following graph
            /*
            def generateString() {
                return 1+"@" +2;
            };

            vv = [Imperative]{
                generateString();
            // "1@2" will be on the heap
            // current stack has no reference type elements
            //  --------------------------------GC kicks in due to mem threshold----------------------------
            // "1@2" on the heap will be marked as white - since no stack elements have a reference to it
            //
            // Async VM execution computes a new generateString() fn Call
                cc = generateString();
            //
            // cc is pushed on the stack and holds a reference to the "1@2" heap element
            //
            // GC starts the sweep process in which it deletes "1@2" heap element
            // ----------------------------------GC is finished---------------------------------------------
                return cc;
            };
            //
            // vv is still on the stack with a reference to a now null heap element
            */
            var heap = new Heap();

            string sseValue = "hello world";

            // Allocate a string on the heap.
            // Similar to what would happen if a string stack element was pushed on the stack and then popped (due to out of scope).
            heap.AllocateString(sseValue);

            StackValue someStackValue = StackValue.BuildNull();

            var notifications = new Dictionary<Heap.GCState, (Action, Action)>() {
                { 
                    Heap.GCState.Sweep, 
                    (() => {
                        // Simulate a new string (with the same value as existing one on heap) stack element being created while GC is propagating or sweeping
                        someStackValue = heap.AllocateString(sseValue);
                    }, 
                    () => { }) 
                }
            };
            // Start GC with a random stack value as gcRoot (not the string stack element, because it was pushed out of the stack)
            heap.FullGCTest(new List<StackValue>() { StackValue.BuildInt(1) }, testExecutive, notifications);

            // The stack element that was pushed after GC start should be valid.
            Assert.IsNotNull(heap.ToHeapObject<DSString>(someStackValue));
        }

        /// <summary>
        /// Test multi dimensional array could be released properly
        /// </summary>
        [Test]
        public void TestMultiDimensionaldArray()
        {
            var heap = new Heap();

            var array1 = heap.AllocateArray(new StackValue[] { StackValue.BuildInt(0) });
            var array2 = heap.AllocateArray(new StackValue[] { array1 });
            var array3 = heap.AllocateArray(new StackValue[] { array2 });

            heap.FullGC(new List<StackValue>() {}, testExecutive);

            Assert.IsNull(heap.ToHeapObject<DSArray>(array1));
            Assert.IsNull(heap.ToHeapObject<DSArray>(array2));
            Assert.IsNull(heap.ToHeapObject<DSArray>(array3));
        }

        /// <summary>
        /// Test a dictionary will be released properly.
        /// </summary>
        [Test]
        public void TestDictionary()
        {
            var heap = new Heap();

            var key = heap.AllocateArray(new StackValue[] { StackValue.BuildInt(42) });
            var val = heap.AllocateString("Hello world");
            var dict = new Dictionary<StackValue, StackValue>();
            dict[key] = val;

            var array = heap.AllocateArray(new StackValue[] { });

            heap.FullGC(new List<StackValue>() {}, testExecutive);

            Assert.IsNull(heap.ToHeapObject<DSArray>(val));
            Assert.IsNull(heap.ToHeapObject<DSArray>(array));
        }

        /// <summary>
        /// Test self reference array can be released properly.
        /// </summary>
        [Test]
        public void TestSelfReference()
        {
            var heap = new Heap();
            var svArray = heap.AllocateArray(new StackValue[] { StackValue.Null });
            var array = heap.ToHeapObject<DSArray>(svArray);
            // self reference
            array.SetValueForIndex(0, svArray, null);

            heap.FullGC(new List<StackValue>() {}, testExecutive);
            Assert.IsNull(heap.ToHeapObject<DSArray>(svArray));
        }

        /// <summary>
        /// Test circular reference array can be released properly.
        /// </summary>
        [Test]
        public void TestCircularReference()
        {
            var heap = new Heap();
            var svArray1 = heap.AllocateArray(new StackValue[] { StackValue.Null });
            var svArray2 = heap.AllocateArray(new StackValue[] { svArray1 });
            var array1 = heap.ToHeapObject<DSArray>(svArray1);
            // self reference
            array1.SetValueForIndex(0, svArray2, null);

            heap.FullGC(new List<StackValue>() { }, testExecutive);
            Assert.IsNull(heap.ToHeapObject<DSArray>(svArray1));
            Assert.IsNull(heap.ToHeapObject<DSArray>(svArray2));
        }
    }
}
