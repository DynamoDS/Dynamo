using System;
using NUnit.Framework;

namespace ProtoTest.ParserTest
{
    [TestFixture]
    class ParserTest : ProtoTestBase
    {
        [Test]        public void TestEscapeString()        {            String code =@"x = ""hello\\"" + ""\\"" + ""world"";";            thisTest.RunScriptSource(code);            thisTest.Verify("x", "hello\\\\world");        }

        [Test]
        public void TestInvalidEscape1()
        {
            string code = @"x = ""\"";";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                thisTest.RunScriptSource(code);
            });
        }

        [Test]
        public void TestInvalidEscape2()
        {
            string code = @"x = ""\X"";";
            Assert.Throws(typeof(ProtoCore.Exceptions.CompileErrorsOccured), () =>
            {
                thisTest.RunScriptSource(code);
            });
        }

        [Test]
        public void CanParseMinLongValue()
        {
            var code = "x = -9223372036854775808;";
            thisTest.RunScriptSource(code);
            thisTest.Verify("x", long.MinValue);
        }

        [Test]
        public void CanParseTypedFunctionParametersWithNamespace()
        {
            var code = @"
a : var = Builtin.Dictionary.ByKeysValues([""hello""], [""world""]);
def foo(x: Builtin.Dictionary)
{
    return x;
};
b = foo(a);
keys = b.Keys;
values = b.Values;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("keys", new[] { "hello" });
            thisTest.Verify("values", new[] { "world" });
        }

        [Test]
        public void CanParseTypedFunctionParametersWithoutNamespace()
        {
            var code = @"
a = 1;
def foo(x: int)
{
    return x;
};
b = foo(a);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("b", 1);
        }

        [Test]
        public void CanParseUntypedFunctionParameters()
        {
            var code = @"
a = 2;
def foo(x)
{
    return x;
};
b = foo(a);
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("b", 2);
        }
    }
}
