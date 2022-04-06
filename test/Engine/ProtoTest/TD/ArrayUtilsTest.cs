using System;
using NUnit.Framework;
using ProtoCore.DSASM;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Utils;
namespace ProtoTest.UtilsTests
{
    [TestFixture]
    class ArrayUtilsTest : ProtoTestBase
    {
        [Test]
        public void StackValueDiffTestDefect()
        {
            String code =
@"[Imperative]
{
	a = 1;
    b = 1.0;
}
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            ProtoCore.RuntimeCore runtimeCore = null;
            runtimeCore = fsr.Execute(code, core); ExecutionMirror mirror = runtimeCore.Mirror;
            StackValue svA = mirror.GetRawFirstValue("a");
            StackValue svB = mirror.GetRawFirstValue("b");
            Assert.IsTrue(svA.metaData.type != svB.metaData.type);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void StackValueDiffTestUserDefined()
        {
            String code =
@"
import(""FFITarget.dll"");
[Imperative]
{
	a = ClassFunctionality.ClassFunctionality(20);
    b = 1.0;
}
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            ProtoCore.RuntimeCore runtimeCore = null;
            runtimeCore = fsr.Execute(code, core);
            ExecutionMirror mirror = runtimeCore.Mirror;
            StackValue svA = mirror.GetRawFirstValue("a");
            StackValue svB = mirror.GetRawFirstValue("b");
            Assert.IsTrue(svA.metaData.type != svB.metaData.type);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void StackValueDiffTestProperty01()
        {
            String code =
@"
import(""FFITarget.dll"");
[Imperative]
{
	a = ClassFunctionality.ClassFunctionality(20);
    b = 1.0;
}
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            ProtoCore.RuntimeCore runtimeCore = null;
            runtimeCore = fsr.Execute(code, core); ExecutionMirror mirror = runtimeCore.Mirror;
            StackValue svA = mirror.GetRawFirstValue("a");
            StackValue svB = mirror.GetRawFirstValue("b");
            Assert.IsTrue(svA.metaData.type != svB.metaData.type);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void StackValueDiffTestProperty02()
        {
            String code =
@"
import(""FFITarget.dll"");
[Imperative]
{
	a = ClassFunctionality.ClassFunctionality(20);
    b = a.IntVal;
    c = 1.0;
}
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            ProtoCore.RuntimeCore runtimeCore = null;
            runtimeCore = fsr.Execute(code, core); ExecutionMirror mirror = runtimeCore.Mirror;
            StackValue svA = mirror.GetRawFirstValue("b");
            StackValue svB = mirror.GetRawFirstValue("c");
            Assert.IsTrue(svA.metaData.type != svB.metaData.type);
        }

        [Test]
        public void TestArrayGetCommonSuperType()
        {
            String code =
@"
class A {}
class B extends A {}
class C extends B {}
tAAA = [A.A(), A.A(), A.A()];
tAAB = [A.A(), A.A(), B.B()];
tAAC = [A.A(), A.A(), C.C()];
tABA = [A.A(), B.B(), A.A()];
tABB = [A.A(), B.B(), B.B()];
tABC = [A.A(), B.B(), C.C()];
tACA = [A.A(), C.C(), A.A()];
tACB = [A.A(), C.C(), B.B()];
tACC = [A.A(), C.C(), C.C()];
//---
tBAA = [B.B(), A.A(), A.A()];
tBAB = [B.B(), A.A(), B.B()];
tBAC = [B.B(), A.A(), C.C()];
tBBA = [B.B(), B.B(), A.A()];
tBBB = [B.B(), B.B(), B.B()];
tBBC = [B.B(), B.B(), C.C()];
tBCA = [B.B(), C.C(), A.A()];
tBCB = [B.B(), C.C(), B.B()];
tBCC = [B.B(), C.C(), C.C()];
//---
tCAA = [C.C(), A.A(), A.A()];
tCAB = [C.C(), A.A(), B.B()];
tCAC = [C.C(), A.A(), C.C()];
tCBA = [C.C(), B.B(), A.A()];
tCBB = [C.C(), B.B(), B.B()];
tCBC = [C.C(), B.B(), C.C()];
tCCA = [C.C(), C.C(), A.A()];
tCCB = [C.C(), C.C(), B.B()];
tCCC = [C.C(), C.C(), C.C()];
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            ProtoCore.RuntimeCore runtimeCore = null;
            runtimeCore = fsr.Execute(code, core); ExecutionMirror mirror = runtimeCore.Mirror;
            StackValue svAAA = mirror.GetRawFirstValue("tAAA");
            ClassNode superAAA = ArrayUtils.GetGreatestCommonSubclassForArray(svAAA, runtimeCore);
            Assert.IsTrue(superAAA.Name == "A");
            StackValue svAAB = mirror.GetRawFirstValue("tAAB");
            ClassNode superAAB = ArrayUtils.GetGreatestCommonSubclassForArray(svAAB, runtimeCore);
            Assert.IsTrue(superAAB.Name == "A");
            StackValue svAAC = mirror.GetRawFirstValue("tAAC");
            ClassNode superAAC = ArrayUtils.GetGreatestCommonSubclassForArray(svAAC, runtimeCore);
            Assert.IsTrue(superAAC.Name == "A");
            StackValue svABA = mirror.GetRawFirstValue("tABA");
            ClassNode superABA = ArrayUtils.GetGreatestCommonSubclassForArray(svABA, runtimeCore);
            Assert.IsTrue(superABA.Name == "A");
            StackValue svABB = mirror.GetRawFirstValue("tABB");
            ClassNode superABB = ArrayUtils.GetGreatestCommonSubclassForArray(svABB, runtimeCore);
            Assert.IsTrue(superABB.Name == "A");
            StackValue svABC = mirror.GetRawFirstValue("tABC");
            ClassNode superABC = ArrayUtils.GetGreatestCommonSubclassForArray(svABC, runtimeCore);
            Assert.IsTrue(superABC.Name == "A");
            StackValue svACA = mirror.GetRawFirstValue("tACA");
            ClassNode superACA = ArrayUtils.GetGreatestCommonSubclassForArray(svACA, runtimeCore);
            Assert.IsTrue(superACA.Name == "A");
            StackValue svACB = mirror.GetRawFirstValue("tACB");
            ClassNode superACB = ArrayUtils.GetGreatestCommonSubclassForArray(svACB, runtimeCore);
            Assert.IsTrue(superACB.Name == "A");
            StackValue svACC = mirror.GetRawFirstValue("tACC");
            ClassNode superACC = ArrayUtils.GetGreatestCommonSubclassForArray(svACC, runtimeCore);
            Assert.IsTrue(superACC.Name == "A");
            //----
            StackValue svBAA = mirror.GetRawFirstValue("tBAA");
            ClassNode superBAA = ArrayUtils.GetGreatestCommonSubclassForArray(svBAA, runtimeCore);
            Assert.IsTrue(superBAA.Name == "A");
            StackValue svBAB = mirror.GetRawFirstValue("tBAB");
            ClassNode superBAB = ArrayUtils.GetGreatestCommonSubclassForArray(svBAB, runtimeCore);
            Assert.IsTrue(superBAB.Name == "A");
            StackValue svBAC = mirror.GetRawFirstValue("tBAC");
            ClassNode superBAC = ArrayUtils.GetGreatestCommonSubclassForArray(svBAC, runtimeCore);
            Assert.IsTrue(superBAC.Name == "A");
            StackValue svBBA = mirror.GetRawFirstValue("tBBA");
            ClassNode superBBA = ArrayUtils.GetGreatestCommonSubclassForArray(svBBA, runtimeCore);
            Assert.IsTrue(superBBA.Name == "A");
            StackValue svBBB = mirror.GetRawFirstValue("tBBB");
            ClassNode superBBB = ArrayUtils.GetGreatestCommonSubclassForArray(svBBB, runtimeCore);
            Assert.IsTrue(superBBB.Name == "B");
            StackValue svBBC = mirror.GetRawFirstValue("tBBC");
            ClassNode superBBC = ArrayUtils.GetGreatestCommonSubclassForArray(svBBC, runtimeCore);
            Assert.IsTrue(superBBC.Name == "B");
            StackValue svBCA = mirror.GetRawFirstValue("tBCA");
            ClassNode superBCA = ArrayUtils.GetGreatestCommonSubclassForArray(svBCA, runtimeCore);
            Assert.IsTrue(superBCA.Name == "A");
            StackValue svBCB = mirror.GetRawFirstValue("tBCB");
            ClassNode superBCB = ArrayUtils.GetGreatestCommonSubclassForArray(svBCB, runtimeCore);
            Assert.IsTrue(superBCB.Name == "B");
            StackValue svBCC = mirror.GetRawFirstValue("tBCC");
            ClassNode superBCC = ArrayUtils.GetGreatestCommonSubclassForArray(svBCC, runtimeCore);
            Assert.IsTrue(superBCC.Name == "B");
            //----
            StackValue svCAA = mirror.GetRawFirstValue("tCAA");
            ClassNode superCAA = ArrayUtils.GetGreatestCommonSubclassForArray(svCAA, runtimeCore);
            Assert.IsTrue(superCAA.Name == "A");
            StackValue svCAB = mirror.GetRawFirstValue("tCAB");
            ClassNode superCAB = ArrayUtils.GetGreatestCommonSubclassForArray(svCAB, runtimeCore);
            Assert.IsTrue(superCAB.Name == "A");
            StackValue svCAC = mirror.GetRawFirstValue("tCAC");
            ClassNode superCAC = ArrayUtils.GetGreatestCommonSubclassForArray(svCAC, runtimeCore);
            Assert.IsTrue(superCAC.Name == "A");
            StackValue svCBA = mirror.GetRawFirstValue("tCBA");
            ClassNode superCBA = ArrayUtils.GetGreatestCommonSubclassForArray(svCBA, runtimeCore);
            Assert.IsTrue(superCBA.Name == "A");
            StackValue svCBB = mirror.GetRawFirstValue("tCBB");
            ClassNode superCBB = ArrayUtils.GetGreatestCommonSubclassForArray(svCBB, runtimeCore);
            Assert.IsTrue(superCBB.Name == "B");
            StackValue svCBC = mirror.GetRawFirstValue("tCBC");
            ClassNode superCBC = ArrayUtils.GetGreatestCommonSubclassForArray(svCBC, runtimeCore);
            Assert.IsTrue(superCBC.Name == "B");
            StackValue svCCA = mirror.GetRawFirstValue("tCCA");
            ClassNode superCCA = ArrayUtils.GetGreatestCommonSubclassForArray(svCCA, runtimeCore);
            Assert.IsTrue(superCCA.Name == "A");
            StackValue svCCB = mirror.GetRawFirstValue("tCCB");
            ClassNode superCCB = ArrayUtils.GetGreatestCommonSubclassForArray(svCCB, runtimeCore);
            Assert.IsTrue(superCCB.Name == "B");
            StackValue svCCC = mirror.GetRawFirstValue("tCCC");
            ClassNode superCCC = ArrayUtils.GetGreatestCommonSubclassForArray(svCCC, runtimeCore);
            Assert.IsTrue(superCCC.Name == "C");
        }

        [Test]
        public void Defect_TestArrayGetCommonSuperType()
        {
            String code =
@"
class A{};
class B extends A{};
class C extends A{};
class D extends C{};
a =A.A();
b = B.B();
c = C.C();
d = D.D();
//ba:A = B.B();
//ca:A = C.C();
//dc:C = D.D();
tABC = [ a, b, c ];
tABD = [ a, b, d ];
tACD = [ a, c, d ];
tBCD = [ b, c, d ];
tAB = [ a, b ];
tAD = [ a, d ];
tBC = [ b, c ];
tBD = [ b, d ];
tCD = [ c, d ];
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            ProtoCore.RuntimeCore runtimeCore = null;
            runtimeCore = fsr.Execute(code, core); ExecutionMirror mirror = runtimeCore.Mirror;
            StackValue svABC = mirror.GetRawFirstValue("tABC");

            ClassNode superABC = ArrayUtils.GetGreatestCommonSubclassForArray(svABC, runtimeCore);
            Assert.IsTrue(superABC.Name == "A");
            StackValue svABD = mirror.GetRawFirstValue("tABD");
            ClassNode superABD = ArrayUtils.GetGreatestCommonSubclassForArray(svABD, runtimeCore);
            Assert.IsTrue(superABD.Name == "A");
            StackValue svACD = mirror.GetRawFirstValue("tACD");
            ClassNode superACD = ArrayUtils.GetGreatestCommonSubclassForArray(svACD, runtimeCore);
            Assert.IsTrue(superABD.Name == "A");
            StackValue svBCD = mirror.GetRawFirstValue("tBCD");
            ClassNode superBCD = ArrayUtils.GetGreatestCommonSubclassForArray(svBCD, runtimeCore);
            Assert.IsTrue(superBCD.Name == "A");
            StackValue svAB = mirror.GetRawFirstValue("tAB");
            ClassNode superAB = ArrayUtils.GetGreatestCommonSubclassForArray(svAB, runtimeCore);
            Assert.IsTrue(superAB.Name == "A");
            StackValue svAD = mirror.GetRawFirstValue("tAD");
            ClassNode superAD = ArrayUtils.GetGreatestCommonSubclassForArray(svAD, runtimeCore);
            Assert.IsTrue(superAD.Name == "A");
            StackValue svBC = mirror.GetRawFirstValue("tBC");
            ClassNode superBC = ArrayUtils.GetGreatestCommonSubclassForArray(svBC, runtimeCore);
            Assert.IsTrue(superBC.Name == "A");
            StackValue svBD = mirror.GetRawFirstValue("tBD");
            ClassNode superBD = ArrayUtils.GetGreatestCommonSubclassForArray(svBD, runtimeCore);
            Assert.IsTrue(superBD.Name == "A");
            StackValue svCD = mirror.GetRawFirstValue("tCD");
            ClassNode superCD = ArrayUtils.GetGreatestCommonSubclassForArray(svCD, runtimeCore);
            Assert.IsTrue(superCD.Name == "C");
        }

        [Test]
        [Category("Method Resolution")]
        public void Defect_TestArrayGetCommonSuperType_2_EmptyArray()
        {
            String code =
@"
class A{};
class B extends A{};
class C extends A{};
class D extends C{};
a = A.A();
ba:A = B.B();
ca:A = C.C();
dc:C = D.D();
tABC = [ a, ba, ca ];
tABD = [ a, ba, dc ];
tACD = [ a, ca, dc ];
tBCD = [ ba, ca, dc ];
tDD = [dc, D.D()];
tE = [];//empty array
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            ProtoCore.RuntimeCore runtimeCore = null;
            runtimeCore = fsr.Execute(code, core); ExecutionMirror mirror = runtimeCore.Mirror;
            StackValue svABC = mirror.GetRawFirstValue("tABC");
            ClassNode superABC = ArrayUtils.GetGreatestCommonSubclassForArray(svABC, runtimeCore);
            Assert.IsTrue(superABC.Name == "A");
            StackValue svABD = mirror.GetRawFirstValue("tABD");
            ClassNode superABD = ArrayUtils.GetGreatestCommonSubclassForArray(svABD, runtimeCore);
            Assert.IsTrue(superABD.Name == "A");
            StackValue svACD = mirror.GetRawFirstValue("tACD");
            ClassNode superACD = ArrayUtils.GetGreatestCommonSubclassForArray(svACD, runtimeCore);
            Assert.IsTrue(superABD.Name == "A");
            StackValue svBCD = mirror.GetRawFirstValue("tBCD");
            ClassNode superBCD = ArrayUtils.GetGreatestCommonSubclassForArray(svBCD, runtimeCore);
            Assert.IsTrue(superBCD.Name == "A");
            StackValue svDD = mirror.GetRawFirstValue("tDD");
            ClassNode superDD = ArrayUtils.GetGreatestCommonSubclassForArray(svDD, runtimeCore);
            Assert.IsTrue(superDD.Name == "D");
            StackValue svE = mirror.GetRawFirstValue("tE");
            ClassNode superE = ArrayUtils.GetGreatestCommonSubclassForArray(svE, runtimeCore);
            Assert.IsTrue(superE == null);
            //Assert.IsTrue(superE.name.Equals(""));
        }

        [Test]
        [Category("Method Resolution")]
        public void Defect_TestArrayGetCommonSuperType_3()
        {
            String code =
@"
class A{};
class B extends A{};
class C extends B{};
class D extends C{};
class E extends D{};
class F extends A{};
class G{};
class H extends G{};
a = A.A();
b = B.B();
c = C.C();
d = D.D();
e = E.E();
f = F.F();
g = G.G();
h = H.H();
rABCDEF = [a,b,c,d,e,f];
rBCDEF = [b,c,d,e,f];
rBH = [b,h];
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            ProtoCore.RuntimeCore runtimeCore = null;
            runtimeCore = fsr.Execute(code, core); ExecutionMirror mirror = runtimeCore.Mirror;
            StackValue svABCDEF = mirror.GetRawFirstValue("rABCDEF");
            ClassNode superABCDEF = ArrayUtils.GetGreatestCommonSubclassForArray(svABCDEF, runtimeCore);
            Assert.IsTrue(superABCDEF.Name == "A");
            StackValue svBCDEF = mirror.GetRawFirstValue("rBCDEF");
            ClassNode superBCDEF = ArrayUtils.GetGreatestCommonSubclassForArray(svBCDEF, runtimeCore);
            Assert.IsTrue(superBCDEF.Name == "A");
            StackValue svBH = mirror.GetRawFirstValue("rBH");
            ClassNode superBH = ArrayUtils.GetGreatestCommonSubclassForArray(svBH, runtimeCore);
            Assert.IsTrue(superBH.Name == "var");
        }

        [Test]
        public void IsArrayTest()
        {
            String code =
@"
i=[Imperative]
{
	a = [1,2,3];
    b = 1;
    c = a;
    return [a,b,c];
}
a=i[0];
b=i[1];
c=i[2];
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            ProtoCore.RuntimeCore runtimeCore = null;
            runtimeCore = fsr.Execute(code, core); ExecutionMirror mirror = runtimeCore.Mirror;
            StackValue svA = mirror.GetRawFirstValue("a");
            StackValue svB = mirror.GetRawFirstValue("b");
            StackValue svC = mirror.GetRawFirstValue("c");
            StackValue svI = mirror.GetRawFirstValue("i");
            Assert.IsTrue(!svA.IsArray);
            Assert.IsTrue(!svB.IsArray);
            Assert.IsTrue(!svC.IsArray);
            Assert.IsTrue(svI.IsArray);
        }

        [Test]
        public void TestDepthCountOnJaggedArray()
        {
            String code =
                @"
a = [1,[[1],[3.1415]],null,1.0,12.3];
b = [1,2,[3]];
x = [[1],[3.1415]];
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            ProtoCore.RuntimeCore runtimeCore = null;
            runtimeCore = fsr.Execute(code, core); ExecutionMirror mirror = runtimeCore.Mirror;
            StackValue a = mirror.GetRawFirstValue("a");
            StackValue b = mirror.GetRawFirstValue("b");
            StackValue x = mirror.GetRawFirstValue("x");
            int rankA = ArrayUtils.GetMaxRankForArray(a, runtimeCore);
            Assert.IsTrue(rankA == 3);
            int rankB = ArrayUtils.GetMaxRankForArray(b, runtimeCore);
            Assert.IsTrue(rankB == 2);
            int rankX = ArrayUtils.GetMaxRankForArray(x, runtimeCore);
            Assert.IsTrue(rankX == 2);            /*
                         * 
                         */
        }

        [Test]
        public void Defect_OnDepthCount()
        {
            String code =
                @"
        a = [[3.1415]];
r1 = Contains(a, 3.0);
r2 = Contains(a, 3.0);
//t = Contains(a, null);
";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            ProtoCore.RuntimeCore runtimeCore = null;
            runtimeCore = fsr.Execute(code, core); ExecutionMirror mirror = runtimeCore.Mirror;
            StackValue a = mirror.GetRawFirstValue("a");
            int rankA = ArrayUtils.GetMaxRankForArray(a, runtimeCore);
            Assert.IsTrue(rankA == 2);
        }
    }
}
