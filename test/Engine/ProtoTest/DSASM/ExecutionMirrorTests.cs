using System;
using NUnit.Framework;
namespace ProtoTest.Associative
{
    class ExecutionMirrorTests : ProtoTestBase
    {
        [Test]
        public void LiteralRetrival()
        {
            String code =
@"
foo;
[Associative]
{
	foo = 5;
}
";

            thisTest.RunScriptSource(code);
            thisTest.Verify("foo", 5);
        }

        [Test]
        public void ArrayRetrival1D()
        {
            String code =
@"
foo;
[Associative]
{
	foo = [5];
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("foo", new object[] { 5});
        }

        [Test]
        public void ArrayRetrival2D()
        {
            String code =
@"
foo;
[Associative]
{
	foo = [[5]];
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("foo", new object[] { new object[] { 5 } });
        }

        [Test]
        public void ArrayRetrival2DJagged()
        {
            String code =
@"
foo;
[Associative]
{
	foo = [[5], 6];
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("foo", new object[] { new object[] { 5 }, 6 });
        }

        [Test]
        public void ArrayRetrival2D2b1()
        {
            String code =
@"
foo;
[Associative]
{
	foo = [[5], [6]];
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("foo", new object[] { new object[] { 5 }, new object[] { 6 } });
        }

        [Test]
        public void ArrayRetrival1DEmpty()
        {
            String code =
@"
foo;
[Associative]
{
	foo = [];
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("foo", new object[] { });
        }
    }
}
