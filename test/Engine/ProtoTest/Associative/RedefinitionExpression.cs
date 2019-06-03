using System;
using NUnit.Framework;
namespace ProtoTest.Associative
{
    class RedefinitionExpression : ProtoTestBase
    {
        [Test]
        public void RedefineWithFunctions01()
        {
            String code =
@"
def f(i : int)
{
    return = i + 1;
}
x = 1000;
x = f(x);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 1001);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void RedefineWithConstructor()
        {
            String code =
@"

import(""FFITarget.dll"");
p = 10;
p = DummyPoint.ByCoordinates(11.0, 20.0, 30.0);
x = p.X;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 11);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void RedefineWithFunctions03()
        {
            String code =
@"
def f(a : int)
{
    a = a + 1;
    return = a;
}

x = 10;
x = f(x);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 11);
        }
        //TestCase from Mark//

        [Test]
        public void RedefineWithFunctions04()
        {
            String code =
@"def f1(i : int, k : int)
{
return = i + k;
}
def f2(i : int, k : int)
{
return = i - k;
}
x = 12;
y = 10;
x = f1(x, y) - f2(x, y); 
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 20);
        }

        [Test]
        public void RedefineWithFunctions05()
        {
            String code =
@"
def f(i : int)
{
i = i * i;
return = i;
}
x = 2;
x = f(x + f(x));
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 36);
        }

        [Test]
        public void RedefineWithExpressionLists01()
        {
            String code =
@"
a = 1;
a = [a, 2];
x = a[0];
y = a[1];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 1);
            thisTest.Verify("y", 2);
        }

        [Test]
        public void RedefineWithExpressionLists02()
        {
            String code =
@"
def f(i : int)
{
    return = i + 1;
}
a = 1;
a = [1, f(a)];
x = a[0];
y = a[1];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 1);
            thisTest.Verify("y", 2);
        }
        //Mark TestCases//

        [Test]
        [Category("ToFixJun")]
        public void RedefineWithExpressionLists03()
        {
            String code =
@"
def f(i : int)
{
    list = [1, 2, 3, 4];
    return = i + list[i];
}
a = 1;
a = [f(f(a)), f(a)];
x = a[0];
y = a[1];
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", 7);
            thisTest.Verify("y", 3);
        }

        [Test]
        [Category("DSDefinedClass_Ported")]
        public void RedefineWithExpressionLists04()
        {
            String code =
@"

import(""FFITarget.dll"");
p = 2;
p = DummyPoint.ByCoordinates(1..3, 20, 30);
a = p.X;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", new object[] {1.0, 2.0, 3.0});
        }
    }
}
