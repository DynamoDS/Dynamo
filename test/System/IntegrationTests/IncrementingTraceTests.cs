using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using FFITarget;
using NUnit.Framework;
using ProtoTestFx.TD;

namespace IntegrationTests
{
    class IncrementingTraceTests
    {
        private const string __TEMP_REVIT_TRACE_ID = "{0459D869-0C72-447F-96D8-08A7FB92214B}-REVIT";
        public TestFrameWork thisTest = new TestFrameWork();


        [SetUp]
        public void Setup()
        {
        }

        [TearDown]
        public static void TLSCleanup()
        {
            Thread.FreeNamedDataSlot(__TEMP_REVIT_TRACE_ID);
        }

        [Test]
        [Category("Trace")]
        public void MultipleCallsToFEPFromDifferentCallsites()
        {
            //Verify that multiple calls to the same FEP from different callsites
            //do not over-increment

            var mirror = thisTest.RunScriptSource(
@"import(""FFITarget.dll"");
mtcA = IncrementerTracedClass.IncrementerTracedClass(1);
cleanA = mtcA.WasCreatedWithTrace();

mtcB =  IncrementerTracedClass.IncrementerTracedClass(1);
cleanB = mtcB.WasCreatedWithTrace();

"
);
            Assert.IsTrue((bool)mirror.GetFirstValue("cleanA").Payload == false);
            Assert.IsTrue((bool)mirror.GetFirstValue("cleanB").Payload == false);

        }


        [Test]
        [Category("Trace")]
        public void ReplicatedCallsToFEPFromSameCallsite()
        {
            //Verify that multiple calls to the same FEP from different callsites
            //do not over-increment

            var mirror = thisTest.RunScriptSource(
@"import(""FFITarget.dll"");
mtcA = IncrementerTracedClass.IncrementerTracedClass(0..3);
cleanA = mtcA.WasCreatedWithTrace();
//cleanA == {false, false, false, false}


mtcB =  IncrementerTracedClass.IncrementerTracedClass(0..2);
cleanB = mtcB.WasCreatedWithTrace();
//cleanB == {false, false, false}

"
);

            List<Object> allFalse4 = new List<Object> {false, false, false, false};
            Assert.IsTrue(mirror.CompareArrays("cleanA", allFalse4, typeof(bool)));

            List<Object> allFalse3 = new List<Object> { false, false, false };
            Assert.IsTrue(mirror.CompareArrays("cleanB", allFalse3, typeof(bool)));

        }


        [Test]
        [Category("Trace")]
        public void IncrementerIDTest()
        {
            //Verify that multiple calls to the same FEP from different callsites
            //do not over-increment

            var mirror = thisTest.RunScriptSource(
@"import(""FFITarget.dll"");
mtcA = IncrementerTracedClass.IncrementerTracedClass(0);
mtcAID = mtcA.ID;
mtcAWasTraced = mtcA.WasCreatedWithTrace();

mtcB = IncrementerTracedClass.IncrementerTracedClass(0);
mtcBID = mtcB.ID;
mtcBWasTraced = mtcB.WasCreatedWithTrace();
"
);
            Assert.IsTrue((Int64)mirror.GetFirstValue("mtcAID").Payload == 0);
            Assert.IsTrue((Boolean)mirror.GetFirstValue("mtcAWasTraced").Payload == false);

            Assert.IsTrue((Int64)mirror.GetFirstValue("mtcBID").Payload == 1);
            Assert.IsTrue((Boolean)mirror.GetFirstValue("mtcBWasTraced").Payload == false);
        }


        [Test]
        [Category("Trace")]
        public void IncrementerIDTestUpdate()
        {
            //Verify that multiple calls to the same FEP from different callsites
            //do not over-increment

            var mirror = thisTest.RunScriptSource(
@"import(""FFITarget.dll"");
x = 0;
mtcA = IncrementerTracedClass.IncrementerTracedClass(x);
mtcAID = mtcA.ID;
mtcAWasTraced = mtcA.WasCreatedWithTrace();

x = 1;
"
);
            Assert.IsTrue((Int64)mirror.GetFirstValue("mtcAID").Payload == 0);
            Assert.IsTrue((Boolean)mirror.GetFirstValue("mtcAWasTraced").Payload == true);
        }


    }
}
