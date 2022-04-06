using DynamoServices;
using FFITarget;
using NUnit.Framework;
using ProtoTestFx.TD;

namespace IntegrationTests
{
    public class WrapperCleanp
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
            WrappersTest.Reset();
        }


        [Test]
        [Category("Failure")]
        //[Category("Trace")] // Uncomment this after removing Failure category.
        public void ExecTraceVMClassVerifyTLSCleared()
        {
            var mirror = thisTest.RunScriptSource(
                @"import(""FFITarget.dll"");
x = 1;
wrapper = WrapperObject.WrapperObject(x);

x = 2;
x = 3;
wrapper = null;
");

Assert.IsTrue(WrappersTest.CleanedObjects.Count == 1);


        }


    }

}
