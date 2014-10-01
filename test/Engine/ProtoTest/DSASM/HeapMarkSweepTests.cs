using System.Collections.Generic;

using NUnit.Framework;
using ProtoCore;
using ProtoCore.DSASM;
using ProtoTestFx.TD;
using ProtoScript.Runners;
namespace ProtoTest.DSASM
{
    [TestFixture]
    public class HeapMarkAndSweekTests
    {
        private class TestExecutive : ProtoCore.DSASM.Executive
        {
            public TestExecutive(Core core): base(core)
            {
            }
        }

        private readonly TestFrameWork thisTest = new TestFrameWork();
        private Core testCore;
        private TestExecutive testExecutive;

        [SetUp]
        public void SetUp()
        {
            testCore = thisTest.SetupTestCore();
            testExecutive = new TestExecutive(testCore);
        }

        [TearDown]
        public void CleanUp()
        {
            testCore.Cleanup();
        }

        /// <summary>
        /// Test basic mark and sweep for pointer and string.
        /// </summary>
        [Test]
        public void TesBasic()
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

            heap.GCMarkAndSweep(new List<StackValue>() { array, str }, testExecutive);

            var arrayHeapElement = heap.GetHeapElement(array);
            Assert.IsNull(arrayHeapElement);

            var strHeapElement = heap.GetHeapElement(str);
            Assert.IsNull(strHeapElement);
        }

        /// <summary>
        /// Test that only reference type will be released. Garbage collector 
        /// should skip other types.  
        /// </summary>
        [Test]
        public void TestNonPointers()
        {
            var heap = new Heap();
            var values = new StackValue[]
            {
                StackValue.BuildInt(0),
                StackValue.BuildInt(1),
                StackValue.BuildInt(2)
            };

            var array = heap.AllocateArray(values);

            var allTypes = new List<StackValue>();
            var rawPointer = (int)array.RawIntValue;
            for (int i = 0; i < (int)AddressType.ArrayKey; ++i)
            {
                var val = new StackValue()
                {
                    optype = (AddressType)i, 
                    opdata = rawPointer
                };

                if (!val.IsReferenceType)
                {
                    allTypes.Add(val);
                }
            }

            heap.GCMarkAndSweep(allTypes, testExecutive);

            var arrayHeapElement = heap.GetHeapElement(array);
            Assert.IsNotNull(arrayHeapElement);
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
            // Order doesn't matter
            heap.GCMarkAndSweep(new List<StackValue>() { array3 }, testExecutive);

            var array1HeapElement = heap.GetHeapElement(array1);
            Assert.IsNull(array1HeapElement);

            var array2HeapElement = heap.GetHeapElement(array2);
            Assert.IsNull(array2HeapElement);

            var array3HeapElement = heap.GetHeapElement(array3);
            Assert.IsNull(array3HeapElement);
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

            var array = heap.AllocateArray(new StackValue[] { }, dict);

            heap.GCMarkAndSweep(new List<StackValue>() { array }, testExecutive);

            var valHeapElement = heap.GetHeapElement(val);
            Assert.IsNull(valHeapElement);

            var arrayHeapElement = heap.GetHeapElement(array);
            Assert.IsNull(arrayHeapElement);
        }

        /// <summary>
        /// Test self reference array can be released properly.
        /// </summary>
        [Test]
        public void TestSelfReference()
        {
            var heap = new Heap();
            var array = heap.AllocateArray(new StackValue[] { StackValue.Null });
            var arrayHeapElement = heap.GetHeapElement(array);
            // self reference
            arrayHeapElement.Stack[0] = array;

            heap.GCMarkAndSweep(new List<StackValue>() { array }, testExecutive);
            var releasedHeapElement = heap.GetHeapElement(array);
            Assert.IsNull(releasedHeapElement);
        }

        /// <summary>
        /// Test circular reference array can be released properly.
        /// </summary>
        [Test]
        public void TestCircularReference()
        {
            var heap = new Heap();
            var array1 = heap.AllocateArray(new StackValue[] { StackValue.Null });
            var array2 = heap.AllocateArray(new StackValue[] { array1 });
            var array1HeapElement = heap.GetHeapElement(array1);
            // self reference
            array1HeapElement.Stack[0] = array2;

            heap.GCMarkAndSweep(new List<StackValue>() { array1 }, testExecutive);

            var array1Hpe = heap.GetHeapElement(array1);
            Assert.IsNull(array1Hpe);

            var array2Hpe = heap.GetHeapElement(array2);
            Assert.IsNull(array2Hpe);
        }
    }
}
