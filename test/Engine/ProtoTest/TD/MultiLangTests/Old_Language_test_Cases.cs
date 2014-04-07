using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoTestFx.TD;
namespace ProtoTest.TD.MultiLangTests
{
    class Old_Language_test_Cases
    {
        readonly TestFrameWork thisTest = new TestFrameWork();
        string testPath = "..\\..\\..\\Scripts\\TD\\MultiLanguage\\Old_Language_test_Cases\\";
        [SetUp]
        public void SetUp()
        {
        }
        public void T0000_sample()
        {
            String code =
@"x = 1;";
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
            String errmsg = "";
            ExecutionMirror mirror = thisTest.VerifyRunScriptSource(code, errmsg);
            thisTest.Verify("x", 1);
        }
    }
}
