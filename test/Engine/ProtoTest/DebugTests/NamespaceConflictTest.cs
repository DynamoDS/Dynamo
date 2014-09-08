using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProtoTestFx.TD;

namespace ProtoTest.DebugTests
{
    class NamespaceConflictTest
    {
        public TestFrameWork thisTest = new TestFrameWork();


        [Test]
        [Category("Trace")]
        [Category("Failure")]
        public void DupImportTest()
        {
            var mirror = thisTest.RunScriptSource(
@"import(""FFITarget.dll"");
a = A.DupTargetTest.DupTargetTest(); 
aO = a.Foo();

b = B.DupTargetTest.DupTargetTest();
bO = b.Foo();
"
);
            // Tracked by http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-1947
            string defectID = "MAGN-1947 IntegrationTests.NamespaceConflictTest.DupImportTest";
            Assert.IsTrue((Int64)mirror.GetFirstValue("aO").Payload == 1, defectID);
            Assert.IsTrue((Int64)mirror.GetFirstValue("bO").Payload == 2, defectID);

        }

        [Test]
        [Category("Trace")]
        public void DupImportTestNeg()
        {
            var mirror = thisTest.RunScriptSource(
@"import(""FFITarget.dll"");
a = DupTargetTest.DupTargetTest(); 
aO = a.Foo();
"
);
            Assert.Throws<NotImplementedException>(() =>
            {
                mirror.GetFirstValue("a0");
            });


        }
    }
}
