using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
namespace ProtoTest.MultiLangTests
{
    class NullInputTest : ProtoTestBase
    {
        public override void Setup()
        {
            base.Setup();

            // TODO: Determine if this is necessary for this test
            FFITarget.DisposeCounter.Reset(0);
        }

        [Test]
        public void DYN_2093_NullInput_Flatten()
        {
            var code =
@"
import(""DSCoreNodes.dll"");
DSCore.List.flatten(null);
  ";
            thisTest.RunScriptSource(code);
        }
    }
}
