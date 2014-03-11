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
            var a0 = mirror.GetFirstValue("aO");
            Assert.IsTrue((Int64)a0.Payload == 1);
            var b0 = mirror.GetFirstValue("b0");
            Assert.IsTrue((Int64)b0.Payload == 2);

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
