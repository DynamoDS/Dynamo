using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using FFITarget;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;

namespace IntegrationTests
{
    public class MinimalTraceTests
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
        public static void ExecTraceClassWithoutSupport()
        {
            MinimalTracedClass mtc = new MinimalTracedClass();
            Assert.IsTrue(!mtc.WasCreatedWithTrace());
        }

        [Test]
        [Category("Trace")]
        public static void ExecTraceClassWithoutSupport2()
        {
            //Run the same test twice to ensure that the Fx is separating them correctly
            MinimalTracedClass mtc = new MinimalTracedClass();
            Assert.IsTrue(!mtc.WasCreatedWithTrace());
        }

        [Test]
        [Category("Trace")]
        public static void ExecTraceClassVerifyCleanThread()
        {
            MinimalTracedClass mtc = new MinimalTracedClass();
            Assert.IsTrue(!mtc.WasCreatedWithTrace());
        }




        [Test]
        [Category("Trace")]
        public void SanityCheckVMExecution()
        {
            var mirror = thisTest.RunScriptSource("a = 4 + 5;");
            var a = mirror.GetFirstValue("a").Payload;
            Assert.IsTrue((Int64)a == 9);
        }

        [Test]
        [Category("Trace")]
        public void ExecTraceVMClassVerifyCleanThread()
        {
            var mirror = thisTest.RunScriptSource(
@"import(""FFITarget.dll"");
a = MinimalTracedClass.MinimalTracedClass();
b = a.WasCreatedWithTrace();
"
);
            Assert.IsTrue((bool)mirror.GetFirstValue("b").Payload == false);
        }

        [Test]
        [Category("Trace")]
        public void ExecTraceVMClassVerifyTLSCleared()
        {
            var mirror = thisTest.RunScriptSource(
@"import(""FFITarget.dll"");
mtcA = MinimalTracedClass.MinimalTracedClass();
cleanA = mtcA.WasCreatedWithTrace();

mtcB = MinimalTracedClass.MinimalTracedClass();
cleanB = mtcB.WasCreatedWithTrace();
"
);
            Assert.IsTrue((bool)mirror.GetFirstValue("cleanA").Payload == false);
            Assert.IsTrue((bool)mirror.GetFirstValue("cleanB").Payload == false);

        }


    }
}
