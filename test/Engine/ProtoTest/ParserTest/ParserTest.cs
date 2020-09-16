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
            thisTest.RunScriptSource(code);            thisTest.Verify("x", long.MinValue);
        }
    }
}
