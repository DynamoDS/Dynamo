using DynamoServices;
using FFITarget;
using NUnit.Framework;
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
            TraceUtils.ClearAllKnownTLSKeys();
        }

        [Test]
        [Category("UnitTests")]
        [Category("Trace")]
        public static void ExecTraceClassWithoutSupport()
        {
            MinimalTracedClass mtc = new MinimalTracedClass();
            Assert.IsTrue(!mtc.WasCreatedWithTrace());
        }

        [Test]
        [Category("UnitTests")]
        [Category("Trace")]
        public static void ExecTraceClassWithoutSupport2()
        {
            //Run the same test twice to ensure that the Fx is separating them correctly
            MinimalTracedClass mtc = new MinimalTracedClass();
            Assert.IsTrue(!mtc.WasCreatedWithTrace());
        }

        [Test]
        //[Category("UnitTests")] --uncomment when failure category is removed
        //[Category("Trace")]
        public static void ExecTraceClassVerifyCleanThread()
        {
            MinimalTracedClass mtc = new MinimalTracedClass();
            Assert.IsTrue(!mtc.WasCreatedWithTrace());
        }




        [Test]
        [Category("UnitTests")]
        [Category("Trace")]
        public void SanityCheckVMExecution()
        {
            var mirror = thisTest.RunScriptSource("a = 4 + 5;");
            thisTest.Verify("a", 9);
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
            thisTest.Verify("b", false);
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
            thisTest.Verify("cleanA", false);
            thisTest.Verify("cleanB", false);
        }
    }
}
