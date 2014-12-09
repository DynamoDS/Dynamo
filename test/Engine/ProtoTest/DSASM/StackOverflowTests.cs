using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProtoTest.TD;
using ProtoTestFx.TD;
namespace ProtoTest.DSASM
{
    [TestFixture]
    class StackOverflowTests : ProtoTestBase
    {
        [Test]
        [Category("StackOverflow")]
        public void StackOverflow_DNL_1467365()
        {
            string code =
                @"class test
{
a;
constructor test(a1 : int, b1 : int, c1 : int) {
a = a1; }
}
class Row {
constructor ByPoints(yy:int, xx: int) {
[Imperative]
{
for(j in 0..36) {
    tread = test.test(yy, xx, 3); rise = test.test(tread.a,xx,3);
} } }
}
a = 0..18..1;
b = 0..18..1;
Rows = Row.ByPoints(a, b);";
            thisTest.RunScriptSource(code);
        }

        [Test]
        [Ignore]
        [Category("StackOverflow")]
        [Category("Failure")]
        public void StackOverflow_DNL_1467354()
        {
            // Tracked by: http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4091
            string err = "MAGN-4091 Stack overflow exception with recursive static function";
            string code =
                @"class A {
static def foo() {
return= A.foo(); }
}
Y = A.foo();";
            thisTest.RunScriptSource(code, err);
            thisTest.Verify("Y", null);
        }
    }
}
