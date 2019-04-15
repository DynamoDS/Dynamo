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
