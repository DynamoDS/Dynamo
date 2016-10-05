using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ProtoCore.DSASM;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Utils;
using ProtoFFI;
namespace ProtoTest.UtilsTests
{
    [TestFixture]
    class ClassUtilsTest : ProtoTestBase
    {
        [Test]
        public void GetUpcastChainTest()
        {
            String code =
@"class A {}
class B extends A {}
class C extends B {}
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            ProtoCore.RuntimeCore runtimeCore = null;
            runtimeCore = fsr.Execute(code, core); ExecutionMirror mirror = runtimeCore.Mirror;
            int idA = core.ClassTable.IndexOf("A");
            int idB = core.ClassTable.IndexOf("B");
            int idC = core.ClassTable.IndexOf("C");
            ClassNode cnA = core.ClassTable.ClassNodes[idA];
            ClassNode cnB = core.ClassTable.ClassNodes[idB];
            ClassNode cnC = core.ClassTable.ClassNodes[idC];
            List<int> idsA = ClassUtils.GetClassUpcastChain(cnA, runtimeCore);
            List<int> idsB = ClassUtils.GetClassUpcastChain(cnB, runtimeCore);
            List<int> idsC = ClassUtils.GetClassUpcastChain(cnC, runtimeCore);
            Assert.IsTrue(idsA.Count == 2);
            Assert.IsTrue(idsA.Contains(idA));

            Assert.IsTrue(idsB.Count == 3);
            Assert.IsTrue(idsB.Contains(idA));
            Assert.IsTrue(idsB.Contains(idB));
            Assert.IsTrue(idsC.Count == 4);
            Assert.IsTrue(idsC.Contains(idA));
            Assert.IsTrue(idsC.Contains(idB));
            Assert.IsTrue(idsC.Contains(idC));
        }
    }
}
