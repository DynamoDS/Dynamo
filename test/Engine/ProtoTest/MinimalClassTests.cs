using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
namespace ProtoTest
{
    class MinimalClassTests : ProtoTestBase
    {
        [Test]
        public void TestDS()
        {
            String code =
@"
size=[Imperative]
{
	size = 5;
    return size;
}
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("size", 5);
        }
    }
}
