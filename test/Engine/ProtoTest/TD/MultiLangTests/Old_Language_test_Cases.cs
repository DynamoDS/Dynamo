using System;
using ProtoCore.DSASM.Mirror;
namespace ProtoTest.TD.MultiLangTests
{
    class Old_Language_test_Cases : ProtoTestBase
    {
        public void T0000_sample()
        {
            String code =
@"x = 1;";
            ProtoScript.Runners.ProtoScriptRunner fsr = new ProtoScript.Runners.ProtoScriptRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("x", 1);
        }
    }
}
