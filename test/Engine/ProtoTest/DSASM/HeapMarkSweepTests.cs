using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;
using ProtoCore;
using ProtoCore.DSASM;
using ProtoTestFx.TD;
using ProtoScript.Runners;
namespace ProtoTest.DSASM
{
    [TestFixture]
    public class HeapMarkAndSweepTests
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

            heap.GCMarkAndSweep(new List<StackValue>(), testExecutive);

            HeapElement arrayHeapElement;
            Assert.IsFalse(heap.TryGetHeapElement(array, out arrayHeapElement));

            HeapElement strHeapElement;
            Assert.IsFalse(heap.TryGetHeapElement(str, out strHeapElement));
        }

        /// <summary>
        /// Test that only reference type will be released. Garbage collector 
        /// should skip other types.  
        /// </summary>
        [Test]
        public void TestNonPointers01()
        {
            var heap = new Heap();
            var values = new StackValue[]
            {
                StackValue.BuildInt(0),
                StackValue.BuildInt(1),
                StackValue.BuildInt(2)
            };

            var array1 = heap.AllocateArray(values);

            var allTypes = new List<StackValue>();
            var rawPointer = (int)array1.RawIntValue;
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
            var array2 = heap.AllocateArray(allTypes.ToArray());

            heap.GCMarkAndSweep(new List<StackValue>() { array1}, testExecutive);

            HeapElement arrayHeapElement;
            Assert.IsTrue(heap.TryGetHeapElement(array1, out arrayHeapElement));

            heap.Free();
        }

        /// <summary>
        /// Test that only non-pointer gc root won't retain any memory.
        /// </summary>
        [Test]
        public void TestNonPointers02()
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

            // non pointer gc root won't retain memory
            heap.GCMarkAndSweep(allTypes, testExecutive);

            HeapElement arrayHeapElement;
            Assert.IsFalse(heap.TryGetHeapElement(array, out arrayHeapElement));

            heap.Free();
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

            heap.GCMarkAndSweep(new List<StackValue>() {}, testExecutive);

            HeapElement array1HeapElement;
            Assert.IsFalse(heap.TryGetHeapElement(array1, out array1HeapElement));

            HeapElement array2HeapElement;
            Assert.IsFalse(heap.TryGetHeapElement(array2, out array2HeapElement));

            HeapElement array3HeapElement;
            Assert.IsFalse(heap.TryGetHeapElement(array3, out array3HeapElement));
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

            heap.GCMarkAndSweep(new List<StackValue>() {}, testExecutive);

            HeapElement valHeapElement;
            Assert.IsFalse(heap.TryGetHeapElement(val, out valHeapElement));

            HeapElement arrayHeapElement;
            Assert.IsFalse(heap.TryGetHeapElement(array, out arrayHeapElement));
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

            heap.GCMarkAndSweep(new List<StackValue>() {}, testExecutive);

            HeapElement releasedHeapElement;
            Assert.IsFalse(heap.TryGetHeapElement(array, out releasedHeapElement));
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

            heap.GCMarkAndSweep(new List<StackValue>() { }, testExecutive);

            HeapElement array1Hpe;
            Assert.IsFalse(heap.TryGetHeapElement(array1, out array1Hpe));

            HeapElement array2Hpe;
            Assert.IsFalse(heap.TryGetHeapElement(array2, out array2Hpe));
        }
    }
}
