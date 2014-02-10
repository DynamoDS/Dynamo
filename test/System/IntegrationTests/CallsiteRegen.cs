using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTestFx.TD;

namespace IntegrationTests
{
    public class CallsiteRegen
    {
        public TestFrameWork thisTest = new TestFrameWork();


        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void DifferentGUIDs()
        {

            String code =
                @"import(""FFITarget.dll"");
a = 5;
b = Minimal.Sqrt(a);
a = 9;
";
            ExecutionMirror mirror = thisTest.RunScriptSource(code);

            Assert.IsTrue((Double)mirror.GetValue("b").Payload == 3.0);

            Assert.Inconclusive("Lookat the GUIDs manually to check if this worked");
        }


    }
}
