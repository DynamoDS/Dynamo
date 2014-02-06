using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProtoTestFx.TD;

namespace IntegrationTests
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
            Assert.IsTrue((Int64)mirror.GetFirstValue("aO").Payload == 1);
            Assert.IsTrue((Int64)mirror.GetFirstValue("bO").Payload == 2);

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
