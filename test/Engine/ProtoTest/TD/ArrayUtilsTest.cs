using System;
using System.Linq;
using NUnit.Framework;
using ProtoCore.DSASM;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Utils;
using ProtoFFI;
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
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ProtoCore.RuntimeCore runtimeCore = null;
            ExecutionMirror mirror = fsr.Execute(code, core, out runtimeCore);
            StackValue svA = mirror.GetRawFirstValue("a");
            StackValue svB = mirror.GetRawFirstValue("b");
            Assert.IsTrue(svA.metaData.type != svB.metaData.type);
        }

        [Test]
        public void StackValueDiffTestUserDefined()
        {
            String code =
@"
class A
{
    x : var;
    constructor A()
    {
        x = 20;
    }
}
[Imperative]
{
	a = A.A();
    b = 1.0;
}
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ProtoCore.RuntimeCore runtimeCore = null;
            ExecutionMirror mirror = fsr.Execute(code, core, out runtimeCore);
            StackValue svA = mirror.GetRawFirstValue("a");
            StackValue svB = mirror.GetRawFirstValue("b");
            Assert.IsTrue(svA.metaData.type != svB.metaData.type);
        }

        [Test]
        public void StackValueDiffTestProperty01()
        {
            String code =
@"
class A
{
    x : var;
    constructor A()
    {
        x = 20;
    }
}
[Imperative]
{
	a = A.A();
    b = 1.0;
}
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ProtoCore.RuntimeCore runtimeCore = null;
            ExecutionMirror mirror = fsr.Execute(code, core, out runtimeCore);
            StackValue svA = mirror.GetRawFirstValue("a");
            StackValue svB = mirror.GetRawFirstValue("b");
            Assert.IsTrue(svA.metaData.type != svB.metaData.type);
        }

        [Test]
        public void StackValueDiffTestProperty02()
        {
            String code =
@"
class A
{
    x : var;
    constructor A()
    {
        x = 20;
    }
}
[Imperative]
{
	a = A.A();
    b = a.x;
    c = 1.0;
}
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ProtoCore.RuntimeCore runtimeCore = null;
            ExecutionMirror mirror = fsr.Execute(code, core, out runtimeCore);
            StackValue svA = mirror.GetRawFirstValue("b");
            StackValue svB = mirror.GetRawFirstValue("c");
            Assert.IsTrue(svA.metaData.type != svB.metaData.type);
        }

        [Test]
        public void TestArrayLayerStatsSimple()
        {
            String code =
@"
a;b;c;
[Imperative]
{
	a = {1,2,3};
    b = {1.0, 2.0, 3.0, 3.0};
    c = {1.0, 2.0, 9};
}
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ProtoCore.RuntimeCore runtimeCore = null;
            ExecutionMirror mirror = fsr.Execute(code, core, out runtimeCore);
            StackValue svA = mirror.GetRawFirstValue("a");
            var dict = ProtoCore.Utils.ArrayUtils.GetTypeStatisticsForLayer(svA, runtimeCore);
            Assert.IsTrue(dict[dict.Keys.First()] == 3);
            StackValue svB = mirror.GetRawFirstValue("b");
            var dict2 = ProtoCore.Utils.ArrayUtils.GetTypeStatisticsForLayer(svB, runtimeCore);
            Assert.IsTrue(dict2[dict2.Keys.First()] == 4);
            StackValue svC = mirror.GetRawFirstValue("c");
            var dict3 = ProtoCore.Utils.ArrayUtils.GetTypeStatisticsForLayer(svC, runtimeCore);
            Assert.IsTrue(dict3[dict3.Keys.First()] == 2);
            Assert.IsTrue(dict3[dict3.Keys.Last()] == 1);

            // Assert.IsTrue((Int64)o.Payload == 5);
        }

        [Test]
        public void TestArrayRankSimple()
        {
            String code =
@"a;b;c;d;e;
[Imperative]
{
	a = {1,2,3};
    b = {1.0, 2.0, 3.0, 3.0};
    c = {1.0, 2.0, 9};
    d = {{1}, {1}, {1}};
    e = {{1, 2, 3}, {1, 2, 3}, {1, 2, 3}};
}
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ProtoCore.RuntimeCore runtimeCore = null;
            ExecutionMirror mirror = fsr.Execute(code, core, out runtimeCore);
            StackValue svA = mirror.GetRawFirstValue("a");
            StackValue svB = mirror.GetRawFirstValue("b");
            StackValue svC = mirror.GetRawFirstValue("c");
            StackValue svD = mirror.GetRawFirstValue("d");
            StackValue svE = mirror.GetRawFirstValue("e");
            var a = ProtoCore.Utils.ArrayUtils.GetMaxRankForArray(svA, runtimeCore);
            Assert.IsTrue(a == 1);
            var b = ProtoCore.Utils.ArrayUtils.GetMaxRankForArray(svB, runtimeCore);
            Assert.IsTrue(b == 1);
            var c = ProtoCore.Utils.ArrayUtils.GetMaxRankForArray(svC, runtimeCore);
            Assert.IsTrue(c == 1);
            var d = ProtoCore.Utils.ArrayUtils.GetMaxRankForArray(svD, runtimeCore);
            Assert.IsTrue(d == 2);
            var e = ProtoCore.Utils.ArrayUtils.GetMaxRankForArray(svE, runtimeCore);
            Assert.IsTrue(e == 2);
            // Assert.IsTrue((Int64)o.Payload == 5);
        }

        [Test]
        public void TestArrayRankJagged()
        {
            String code =
@"
a;b;c;d;e;
[Imperative]
{
	a = {1,{2},3};
    b = {1.0, {2.0, 3.0, 3.0}};
    c = {1.0, {2.0, {9}}};
    d = {{1}, {}, {1}};
    e = {{1, 2, 3}, {1, {2}, 3}, {{{1}}, 2, 3}};
}
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ProtoCore.RuntimeCore runtimeCore = null;
            ExecutionMirror mirror = fsr.Execute(code, core, out runtimeCore);
            StackValue svA = mirror.GetRawFirstValue("a");
            StackValue svB = mirror.GetRawFirstValue("b");
            StackValue svC = mirror.GetRawFirstValue("c");
            StackValue svD = mirror.GetRawFirstValue("d");
            StackValue svE = mirror.GetRawFirstValue("e");
            var a = ProtoCore.Utils.ArrayUtils.GetMaxRankForArray(svA, runtimeCore);
            Assert.IsTrue(a == 2);
            var b = ProtoCore.Utils.ArrayUtils.GetMaxRankForArray(svB, runtimeCore);
            Assert.IsTrue(b == 2);
            var c = ProtoCore.Utils.ArrayUtils.GetMaxRankForArray(svC, runtimeCore);
            Assert.IsTrue(c == 3);
            var d = ProtoCore.Utils.ArrayUtils.GetMaxRankForArray(svD, runtimeCore);
            Assert.IsTrue(d == 2);
            var e = ProtoCore.Utils.ArrayUtils.GetMaxRankForArray(svE, runtimeCore);
            Assert.IsTrue(e == 4);
            // Assert.IsTrue((Int64)o.Payload == 5);
        }

        [Test]
        public void TestArrayGetCommonSuperType()
        {
            String code =
@"
class A {}
class B extends A {}
class C extends B {}
tAAA = {A.A(), A.A(), A.A()};
tAAB = {A.A(), A.A(), B.B()};
tAAC = {A.A(), A.A(), C.C()};
tABA = {A.A(), B.B(), A.A()};
tABB = {A.A(), B.B(), B.B()};
tABC = {A.A(), B.B(), C.C()};
tACA = {A.A(), C.C(), A.A()};
tACB = {A.A(), C.C(), B.B()};
tACC = {A.A(), C.C(), C.C()};
//---
tBAA = {B.B(), A.A(), A.A()};
tBAB = {B.B(), A.A(), B.B()};
tBAC = {B.B(), A.A(), C.C()};
tBBA = {B.B(), B.B(), A.A()};
tBBB = {B.B(), B.B(), B.B()};
tBBC = {B.B(), B.B(), C.C()};
tBCA = {B.B(), C.C(), A.A()};
tBCB = {B.B(), C.C(), B.B()};
tBCC = {B.B(), C.C(), C.C()};
//---
tCAA = {C.C(), A.A(), A.A()};
tCAB = {C.C(), A.A(), B.B()};
tCAC = {C.C(), A.A(), C.C()};
tCBA = {C.C(), B.B(), A.A()};
tCBB = {C.C(), B.B(), B.B()};
tCBC = {C.C(), B.B(), C.C()};
tCCA = {C.C(), C.C(), A.A()};
tCCB = {C.C(), C.C(), B.B()};
tCCC = {C.C(), C.C(), C.C()};
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ProtoCore.RuntimeCore runtimeCore = null;
            ExecutionMirror mirror = fsr.Execute(code, core, out runtimeCore);
            StackValue svAAA = mirror.GetRawFirstValue("tAAA");
            ClassNode superAAA = ArrayUtils.GetGreatestCommonSubclassForArray(svAAA, runtimeCore);
            Assert.IsTrue(superAAA.name == "A");
            StackValue svAAB = mirror.GetRawFirstValue("tAAB");
            ClassNode superAAB = ArrayUtils.GetGreatestCommonSubclassForArray(svAAB, runtimeCore);
            Assert.IsTrue(superAAB.name == "A");
            StackValue svAAC = mirror.GetRawFirstValue("tAAC");
            ClassNode superAAC = ArrayUtils.GetGreatestCommonSubclassForArray(svAAC, runtimeCore);
            Assert.IsTrue(superAAC.name == "A");
            StackValue svABA = mirror.GetRawFirstValue("tABA");
            ClassNode superABA = ArrayUtils.GetGreatestCommonSubclassForArray(svABA, runtimeCore);
            Assert.IsTrue(superABA.name == "A");
            StackValue svABB = mirror.GetRawFirstValue("tABB");
            ClassNode superABB = ArrayUtils.GetGreatestCommonSubclassForArray(svABB, runtimeCore);
            Assert.IsTrue(superABB.name == "A");
            StackValue svABC = mirror.GetRawFirstValue("tABC");
            ClassNode superABC = ArrayUtils.GetGreatestCommonSubclassForArray(svABC, runtimeCore);
            Assert.IsTrue(superABC.name == "A");
            StackValue svACA = mirror.GetRawFirstValue("tACA");
            ClassNode superACA = ArrayUtils.GetGreatestCommonSubclassForArray(svACA, runtimeCore);
            Assert.IsTrue(superACA.name == "A");
            StackValue svACB = mirror.GetRawFirstValue("tACB");
            ClassNode superACB = ArrayUtils.GetGreatestCommonSubclassForArray(svACB, runtimeCore);
            Assert.IsTrue(superACB.name == "A");
            StackValue svACC = mirror.GetRawFirstValue("tACC");
            ClassNode superACC = ArrayUtils.GetGreatestCommonSubclassForArray(svACC, runtimeCore);
            Assert.IsTrue(superACC.name == "A");
            //----
            StackValue svBAA = mirror.GetRawFirstValue("tBAA");
            ClassNode superBAA = ArrayUtils.GetGreatestCommonSubclassForArray(svBAA, runtimeCore);
            Assert.IsTrue(superBAA.name == "A");
            StackValue svBAB = mirror.GetRawFirstValue("tBAB");
            ClassNode superBAB = ArrayUtils.GetGreatestCommonSubclassForArray(svBAB, runtimeCore);
            Assert.IsTrue(superBAB.name == "A");
            StackValue svBAC = mirror.GetRawFirstValue("tBAC");
            ClassNode superBAC = ArrayUtils.GetGreatestCommonSubclassForArray(svBAC, runtimeCore);
            Assert.IsTrue(superBAC.name == "A");
            StackValue svBBA = mirror.GetRawFirstValue("tBBA");
            ClassNode superBBA = ArrayUtils.GetGreatestCommonSubclassForArray(svBBA, runtimeCore);
            Assert.IsTrue(superBBA.name == "A");
            StackValue svBBB = mirror.GetRawFirstValue("tBBB");
            ClassNode superBBB = ArrayUtils.GetGreatestCommonSubclassForArray(svBBB, runtimeCore);
            Assert.IsTrue(superBBB.name == "B");
            StackValue svBBC = mirror.GetRawFirstValue("tBBC");
            ClassNode superBBC = ArrayUtils.GetGreatestCommonSubclassForArray(svBBC, runtimeCore);
            Assert.IsTrue(superBBC.name == "B");
            StackValue svBCA = mirror.GetRawFirstValue("tBCA");
            ClassNode superBCA = ArrayUtils.GetGreatestCommonSubclassForArray(svBCA, runtimeCore);
            Assert.IsTrue(superBCA.name == "A");
            StackValue svBCB = mirror.GetRawFirstValue("tBCB");
            ClassNode superBCB = ArrayUtils.GetGreatestCommonSubclassForArray(svBCB, runtimeCore);
            Assert.IsTrue(superBCB.name == "B");
            StackValue svBCC = mirror.GetRawFirstValue("tBCC");
            ClassNode superBCC = ArrayUtils.GetGreatestCommonSubclassForArray(svBCC, runtimeCore);
            Assert.IsTrue(superBCC.name == "B");
            //----
            StackValue svCAA = mirror.GetRawFirstValue("tCAA");
            ClassNode superCAA = ArrayUtils.GetGreatestCommonSubclassForArray(svCAA, runtimeCore);
            Assert.IsTrue(superCAA.name == "A");
            StackValue svCAB = mirror.GetRawFirstValue("tCAB");
            ClassNode superCAB = ArrayUtils.GetGreatestCommonSubclassForArray(svCAB, runtimeCore);
            Assert.IsTrue(superCAB.name == "A");
            StackValue svCAC = mirror.GetRawFirstValue("tCAC");
            ClassNode superCAC = ArrayUtils.GetGreatestCommonSubclassForArray(svCAC, runtimeCore);
            Assert.IsTrue(superCAC.name == "A");
            StackValue svCBA = mirror.GetRawFirstValue("tCBA");
            ClassNode superCBA = ArrayUtils.GetGreatestCommonSubclassForArray(svCBA, runtimeCore);
            Assert.IsTrue(superCBA.name == "A");
            StackValue svCBB = mirror.GetRawFirstValue("tCBB");
            ClassNode superCBB = ArrayUtils.GetGreatestCommonSubclassForArray(svCBB, runtimeCore);
            Assert.IsTrue(superCBB.name == "B");
            StackValue svCBC = mirror.GetRawFirstValue("tCBC");
            ClassNode superCBC = ArrayUtils.GetGreatestCommonSubclassForArray(svCBC, runtimeCore);
            Assert.IsTrue(superCBC.name == "B");
            StackValue svCCA = mirror.GetRawFirstValue("tCCA");
            ClassNode superCCA = ArrayUtils.GetGreatestCommonSubclassForArray(svCCA, runtimeCore);
            Assert.IsTrue(superCCA.name == "A");
            StackValue svCCB = mirror.GetRawFirstValue("tCCB");
            ClassNode superCCB = ArrayUtils.GetGreatestCommonSubclassForArray(svCCB, runtimeCore);
            Assert.IsTrue(superCCB.name == "B");
            StackValue svCCC = mirror.GetRawFirstValue("tCCC");
            ClassNode superCCC = ArrayUtils.GetGreatestCommonSubclassForArray(svCCC, runtimeCore);
            Assert.IsTrue(superCCC.name == "C");
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
a = A.A();
b = B.B();
c = C.C();
d = D.D();
//ba:A = B.B();
//ca:A = C.C();
//dc:C = D.D();
tABC = { a, b, c };
tABD = { a, b, d };
tACD = { a, c, d };
tBCD = { b, c, d };
tAB = { a, b };
tAD = { a, d };
tBC = { b, c };
tBD = { b, d };
tCD = { c, d };
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ProtoCore.RuntimeCore runtimeCore = null;
            ExecutionMirror mirror = fsr.Execute(code, core, out runtimeCore);
            StackValue svABC = mirror.GetRawFirstValue("tABC");

            ClassNode superABC = ArrayUtils.GetGreatestCommonSubclassForArray(svABC, runtimeCore);
            Assert.IsTrue(superABC.name == "A");
            StackValue svABD = mirror.GetRawFirstValue("tABD");
            ClassNode superABD = ArrayUtils.GetGreatestCommonSubclassForArray(svABD, runtimeCore);
            Assert.IsTrue(superABD.name == "A");
            StackValue svACD = mirror.GetRawFirstValue("tACD");
            ClassNode superACD = ArrayUtils.GetGreatestCommonSubclassForArray(svACD, runtimeCore);
            Assert.IsTrue(superABD.name == "A");
            StackValue svBCD = mirror.GetRawFirstValue("tBCD");
            ClassNode superBCD = ArrayUtils.GetGreatestCommonSubclassForArray(svBCD, runtimeCore);
            Assert.IsTrue(superBCD.name == "A");
            StackValue svAB = mirror.GetRawFirstValue("tAB");
            ClassNode superAB = ArrayUtils.GetGreatestCommonSubclassForArray(svAB, runtimeCore);
            Assert.IsTrue(superAB.name == "A");
            StackValue svAD = mirror.GetRawFirstValue("tAD");
            ClassNode superAD = ArrayUtils.GetGreatestCommonSubclassForArray(svAD, runtimeCore);
            Assert.IsTrue(superAD.name == "A");
            StackValue svBC = mirror.GetRawFirstValue("tBC");
            ClassNode superBC = ArrayUtils.GetGreatestCommonSubclassForArray(svBC, runtimeCore);
            Assert.IsTrue(superBC.name == "A");
            StackValue svBD = mirror.GetRawFirstValue("tBD");
            ClassNode superBD = ArrayUtils.GetGreatestCommonSubclassForArray(svBD, runtimeCore);
            Assert.IsTrue(superBD.name == "A");
            StackValue svCD = mirror.GetRawFirstValue("tCD");
            ClassNode superCD = ArrayUtils.GetGreatestCommonSubclassForArray(svCD, runtimeCore);
            Assert.IsTrue(superCD.name == "C");
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
tABC = { a, ba, ca };
tABD = { a, ba, dc };
tACD = { a, ca, dc };
tBCD = { ba, ca, dc };
tDD = {dc, D.D()};
tE = {};//empty array
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ProtoCore.RuntimeCore runtimeCore = null;
            ExecutionMirror mirror = fsr.Execute(code, core, out runtimeCore);
            StackValue svABC = mirror.GetRawFirstValue("tABC");
            ClassNode superABC = ArrayUtils.GetGreatestCommonSubclassForArray(svABC, runtimeCore);
            Assert.IsTrue(superABC.name == "A");
            StackValue svABD = mirror.GetRawFirstValue("tABD");
            ClassNode superABD = ArrayUtils.GetGreatestCommonSubclassForArray(svABD, runtimeCore);
            Assert.IsTrue(superABD.name == "A");
            StackValue svACD = mirror.GetRawFirstValue("tACD");
            ClassNode superACD = ArrayUtils.GetGreatestCommonSubclassForArray(svACD, runtimeCore);
            Assert.IsTrue(superABD.name == "A");
            StackValue svBCD = mirror.GetRawFirstValue("tBCD");
            ClassNode superBCD = ArrayUtils.GetGreatestCommonSubclassForArray(svBCD, runtimeCore);
            Assert.IsTrue(superBCD.name == "A");
            StackValue svDD = mirror.GetRawFirstValue("tDD");
            ClassNode superDD = ArrayUtils.GetGreatestCommonSubclassForArray(svDD, runtimeCore);
            Assert.IsTrue(superDD.name == "D");
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
rABCDEF = {a,b,c,d,e,f};
rBCDEF = {b,c,d,e,f};
rBH = {b,h};
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ProtoCore.RuntimeCore runtimeCore = null;
            ExecutionMirror mirror = fsr.Execute(code, core, out runtimeCore);
            StackValue svABCDEF = mirror.GetRawFirstValue("rABCDEF");
            ClassNode superABCDEF = ArrayUtils.GetGreatestCommonSubclassForArray(svABCDEF, runtimeCore);
            Assert.IsTrue(superABCDEF.name == "A");
            StackValue svBCDEF = mirror.GetRawFirstValue("rBCDEF");
            ClassNode superBCDEF = ArrayUtils.GetGreatestCommonSubclassForArray(svBCDEF, runtimeCore);
            Assert.IsTrue(superBCDEF.name == "A");
            StackValue svBH = mirror.GetRawFirstValue("rBH");
            ClassNode superBH = ArrayUtils.GetGreatestCommonSubclassForArray(svBH, runtimeCore);
            Assert.IsTrue(superBH.name == "var");
        }

        [Test]
        public void IsArrayTest()
        {
            String code =
@"a;b;c;
[Imperative]
{
	a = {1,2,3};
    b = 1;
    c = a;
}
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ProtoCore.RuntimeCore runtimeCore = null;
            ExecutionMirror mirror = fsr.Execute(code, core, out runtimeCore);
            StackValue svA = mirror.GetRawFirstValue("a");
            StackValue svB = mirror.GetRawFirstValue("b");
            StackValue svC = mirror.GetRawFirstValue("c");
            Assert.IsTrue(svA.IsArray);
            Assert.IsTrue(!svB.IsArray);
            Assert.IsTrue(svC.IsArray);
        }

        [Test]
        public void TestDepthCountOnJaggedArray()
        {
            String code =
                @"
a = {1,{{1},{3.1415}},null,1.0,12.3};
b = {1,2,{3}};
x = {{1},{3.1415}};
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ProtoCore.RuntimeCore runtimeCore = null;
            ExecutionMirror mirror = fsr.Execute(code, core, out runtimeCore);
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
        a = {{3.1415}};
r1 = Contains(a, 3.0);
r2 = Contains(a, 3.0);
//t = Contains(a, null);
";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            ProtoCore.RuntimeCore runtimeCore = null;
            ExecutionMirror mirror = fsr.Execute(code, core, out runtimeCore);
            StackValue a = mirror.GetRawFirstValue("a");
            int rankA = ArrayUtils.GetMaxRankForArray(a, runtimeCore);
            Assert.IsTrue(rankA == 2);
        }
    }
}
