using NUnit.Framework;

namespace ProtoTest.Associative
{
    class AtLevelTest : ProtoTestBase
    {
        [Test]
        public void TestAtLevel01()
        {
            string code = @"
def foo(x:var[]..[])
{
    return = x;
}

xs = 
     {
        {
            {
                {
                    1, 
                    2
                },

                {
                    3,
                    4
                }
            },
            
            {
                {
                    5,
                    6
                },

                {
                    7,
                    8
                }
            }
        }, 

        {
            {
                {
                    9, 
                   10 
                },

                {
                   11,
                   12 
                }
            },
            
            {
                {
                   13,
                   14 
                },

                {
                   15,
                   16 
                }
            }
        } 
    };
    //  -4  -3  -2  -1

r0 = foo(xs@0);
r1 = foo(xs@-1);
r2 = foo(xs@-2);
r3 = foo(xs@-3);
r4 = foo(xs@-4);
r5 = foo(xs@-5);
";
            thisTest.RunScriptSource(code);

            var x000 = new object[] { 1, 2 };
            var x001 = new object[] { 3, 4 };
            var x010 = new object[] { 5, 6 };
            var x011 = new object[] { 7, 8 };
            var x100 = new object[] { 9, 10 };
            var x101 = new object[] { 11, 12 };
            var x110 = new object[] { 13, 14 };
            var x111 = new object[] { 15, 16 };

            var x00 = new object[] { x000, x001 };
            var x01 = new object[] { x010, x011 };
            var x10 = new object[] { x100, x101 };
            var x11 = new object[] { x110, x111 };

            var x0 = new object[] { x00, x01 };
            var x1 = new object[] { x10, x11 };

            var xs = new object[] { x0, x1 };

            thisTest.Verify("r1", new object[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 });
            thisTest.Verify("r2", new object[] { x000, x001, x010, x011, x100, x101, x110, x111 });
            thisTest.Verify("r3", new object[] { x00, x01, x10, x11 });
            thisTest.Verify("r4", new object[] { x0, x1 });
            thisTest.Verify("r0", xs);
            thisTest.Verify("r5", new object[] { xs });
        }

        [Test]
        public void TestDominantListOnConstructor()
        {
            string code = @"
import (AtLevelTestClass from ""FFITarget.dll"");
xs = { { { 1,2}, { 3, 4} } };
as = AtLevelTestClass.AtLevelTestClass(xs@@-1);
r1 = as.V;
            ";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", new object[] { new object[] { new object[] { 1, 2 }, new object[] { 3, 4 } } });
        }

        [Test]
        public void TestDominantList01Case1()
        {
            string code = @"
def foo(xs:var[])
{
    return = Sum(xs);
}

xs = {{{1,2}, {3, 4}}, {{5, 6}, {7, 8}}};
r1 = foo(xs@@-2);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", new object[] { new object[] { 3, 7 }, new object[] { 11, 15 } });
        }

        [Test]
        public void TestDominantList01Case2()
        {
            string code = @"
def foo(xs:var[])
{
    return = Sum(xs);
}

xs = {{{1,2}, {3, 4}}, {{5, 6}, {7, 8}}};
r1 = foo(xs@@-1);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", new object[] { new object[] { new object[] { 36 } } });
        }

        [Test]
        public void TestDominantList01ForStaticMethod()
        {
            string code = @"
import (AtLevelTestClass from ""FFITarget.dll"");
xs = { { { 1,2}, { 3, 4} }, { { 5, 6}, { 7, 8} } };
r1 = AtLevelTestClass.Sum(xs@@-2);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", new object[] { new object[] { 3, 7 }, new object[] { 11, 15 } });
        }

        [Test]
        public void TestDominantList01ForMember()
        {
            string code = @"
import (AtLevelTestClass from ""FFITarget.dll"");
t = AtLevelTestClass();
xs = { { { 1,2}, { 3, 4} }, { { 5, 6}, { 7, 8} } };
r1 = t.sum(xs@@-2);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", new object[] { new object[] { 3, 7 }, new object[] { 11, 15 } });
        }

        [Test]
        public void TestDominantList02()
        {
            string code = @"
def foo(xs)
{
    return = xs + 1;
}

xs = {{{1,2}, {3, 4}}};
r1 = foo(xs@@-2);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", new object[] { new object[] { new object[] { 2, 3 }, new object[] { 4, 5 } } });
        }

        [Test]
        public void TestDominantList02ForStaticMethod()
        {
            string code = @"
import (AtLevelTestClass from ""FFITarget.dll"");
xs = {{{1,2}, {3, 4}}};
r1 = AtLevelTestClass.Inc(xs@@-2);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", new object[] { new object[] { new object[] { 2, 3 }, new object[] { 4, 5 } } });
        }

        [Test]
        public void TestDominantList02ForMethod()
        {
            string code = @"
import (AtLevelTestClass from ""FFITarget.dll"");
t = AtLevelTestClass();
xs = {{{1,2}, {3, 4}}};
r1 = t.inc(xs@@-2);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", new object[] { new object[] { new object[] { 2, 3 }, new object[] { 4, 5 } } });
        }

        [Test]
        public void TestDominantList03()
        {
            string code = @"
def foo(xs:var[], ys)
{
    return = Sum(xs) + ys;
}

xs = {{{1,2}, {3, 4}}, {{5, 6}, {7, 8}}};
ys = {""a"", ""b"", ""c""};
r1 = foo(xs@@-2, ys@-1);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", new object[] { new object[] { "3a", "7b" }, new object[] { "11c" } });
        }

        [Test]
        public void TestDominantList03ForStaticMethod()
        {
            string code = @"
import (AtLevelTestClass from ""FFITarget.dll"");
xs = {{{1,2}, {3, 4}}, {{5, 6}, {7, 8}}};
ys = {""a"", ""b"", ""c""};
r1 = AtLevelTestClass.SumAndConcat(xs@@-2, ys@-1);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", new object[] { new object[] { "3a", "7b" }, new object[] { "11c" } });
        }

        [Test]
        public void TestDominantList03ForMethod()
        {
            string code = @"
import (AtLevelTestClass from ""FFITarget.dll"");
t = AtLevelTestClass();
xs = {{{1,2}, {3, 4}}, {{5, 6}, {7, 8}}};
ys = {""a"", ""b"", ""c""};
r1 = t.SumAndConcat(xs@@-2, ys@-1);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", new object[] { new object[] { "3a", "7b" }, new object[] { "11c" } });
        }

        [Test]
        public void TestDominantList04()
        {
            string code = @"
def foo(xs:var[], ys)
{
    return = Sum(xs) + ys;
}

xs = {{{1,2}, {3, 4}}, {{5, 6}, {7, 8}}};
ys = {""a"", ""b"", ""c""};
r1 = foo(xs@-2, ys@@-1);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", new object[] { "3a", "7b", "11c" });
        }

        [Test]
        public void TestDominantList04ForStaticMethod()
        {
            string code = @"
import (AtLevelTestClass from ""FFITarget.dll"");
xs = {{{1,2}, {3, 4}}, {{5, 6}, {7, 8}}};
ys = {""a"", ""b"", ""c""};
r1 = AtLevelTestClass.SumAndConcat(xs@-2, ys@@-1);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", new object[] { "3a", "7b", "11c" });
        }

        [Test]
        public void TestDominantList04ForMethod()
        {
            string code = @"
import (AtLevelTestClass from ""FFITarget.dll"");
t = AtLevelTestClass();
xs = {{{1,2}, {3, 4}}, {{5, 6}, {7, 8}}};
ys = {""a"", ""b"", ""c""};
r1 = t.sumAndConcat(xs@-2, ys@@-1);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", new object[] { "3a", "7b", "11c" });
        }

        [Test]
        public void TestDominantList05()
        {
            string code = @"
def foo(xs:var[], ys)
{
    return = Sum(xs) + ys;
}

xs = {{{1,2}, {3, 4}}, {{5, 6}, {7, 8}}};
ys = {""a"", ""b"", ""c""};
r1 = foo(xs@-2<1L>, ys@@-1<1L>);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", new object[] { "3a", "7b", "11c", "15c" });
        }

        [Test]
        public void TestDominantList05ForStaticMethod()
        {
            string code = @"
import (AtLevelTestClass from ""FFITarget.dll"");
xs = {{{1,2}, {3, 4}}, {{5, 6}, {7, 8}}};
ys = {""a"", ""b"", ""c""};
r1 = AtLevelTestClass.SumAndConcat(xs@-2<1L>, ys@@-1<1L>);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", new object[] { "3a", "7b", "11c", "15c" });
        }

        [Test]
        public void TestDominantList05ForMethod()
        {
            string code = @"
import (AtLevelTestClass from ""FFITarget.dll"");
t = AtLevelTestClass();
xs = {{{1,2}, {3, 4}}, {{5, 6}, {7, 8}}};
ys = {""a"", ""b"", ""c""};
r1 = t.sumAndConcat(xs@-2<1L>, ys@@-1<1L>);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", new object[] { "3a", "7b", "11c", "15c" });
        }


        [Test]
        public void TestDominantList06()
        {
            string code = @"
def foo(xs:var[], ys)
{
    return = Sum(xs) + ys;
}
xs = {{{1,2}, {3, 4}}, {{5, 6}}};
ys = {""a"", ""b"", ""c"", ""d""};
r1 = foo(xs@-2<1L>, ys@@-1<1L>);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", new object[] { "3a", "7b", "11c", "11d" });
        }

        [Test]
        public void TestDominantList06ForStaticMethod()
        {
            string code = @"
import (AtLevelTestClass from ""FFITarget.dll"");
xs = {{{1,2}, {3, 4}}, {{5, 6}}};
ys = {""a"", ""b"", ""c"", ""d""};
r1 = AtLevelTestClass.SumAndConcat(xs@-2<1L>, ys@@-1<1L>);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", new object[] { "3a", "7b", "11c", "11d" });
        }

        [Test]
        public void TestDominantList06ForMethod()
        {
            string code = @"
import (AtLevelTestClass from ""FFITarget.dll"");
t = AtLevelTestClass();
xs = {{{1,2}, {3, 4}}, {{5, 6}}};
ys = {""a"", ""b"", ""c"", ""d""};
r1 = t.sumAndConcat(xs@-2<1L>, ys@@-1<1L>);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", new object[] { "3a", "7b", "11c", "11d" });
        }

        [Test]
        public void TestDominantList07()
        {
            string code = @"
def foo(xs:var[], ys)
{
    return = Sum(xs) + ys;
}
xs = {{{1,2}, {3, 4}}, {{5, 6}, {7, 8}}};
ys = {""a"", ""b"", ""c""};
r1 = foo(xs@@-2<1L>, ys@-1<1L>);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", new object[] { new object[] { "3a", "7b" }, new object[] { "11c", "15c"} });
        }

        [Test]
        public void TestDominantList07ForStaticMethod()
        {
            string code = @"
import (AtLevelTestClass from ""FFITarget.dll"");
xs = {{{1,2}, {3, 4}}, {{5, 6}, {7, 8}}};
ys = {""a"", ""b"", ""c""};
r1 = AtLevelTestClass.SumAndConcat(xs@@-2<1L>, ys@-1<1L>);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", new object[] { new object[] { "3a", "7b" }, new object[] { "11c", "15c" } });
        }

        [Test]
        public void TestDominantList07ForMethod()
        {
            string code = @"
import (AtLevelTestClass from ""FFITarget.dll"");
t = AtLevelTestClass();
xs = {{{1,2}, {3, 4}}, {{5, 6}, {7, 8}}};
ys = {""a"", ""b"", ""c""};
r1 = t.sumAndConcat(xs@@L2<1L>, ys@L1<1L>);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("r1", new object[] { new object[] { "3a", "7b" }, new object[] { "11c", "15c" } });
        }

        [Test]
        public void TestTwoDominatListsThrowWarning()
        {
            string code = @"
def foo(x, y)
{
    return = x + y;
}

xs = {{1,2,3}};
ys = {{4,5,6}};
r = foo(xs@@L1, ys@@L1);
";
            thisTest.RunAndVerifyRuntimeWarning(code, ProtoCore.Runtime.WarningID.MoreThanOneDominantList);
            thisTest.Verify("r", new object[] { 5, 7, 9 });
        }
    }
}
