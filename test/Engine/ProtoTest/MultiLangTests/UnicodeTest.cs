using NUnit.Framework;
using ProtoCore.DSASM.Mirror;

namespace ProtoTest.MultiLangTests
{
    class UnicodeTest : ProtoTestBase
    {
        [Test]
        public void TestUnicodeIdentifiers()
        {
            string code = @"
中文２１=21;
Dreißig=40;
க = 21;
sum =中文２１+ Dreißig + க; 
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "");
            thisTest.Verify("sum", 82);
        }

        [Test]
        public void TestUnicodeFunctionName()
        {
            string code = @"
def 中文２１(Dreißig, க)
{
    return = Dreißig * க;
}
ひゃく=中文２１(2, 50);
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code, "");
            thisTest.Verify("ひゃく", 100);
        }
    }
}
