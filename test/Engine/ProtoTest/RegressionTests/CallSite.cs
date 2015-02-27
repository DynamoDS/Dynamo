
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;

namespace ProtoTest.RegressionTests
{
    [TestFixture]
        public class CallSite
        {
            public ProtoCore.Core core;

            [SetUp]
            public void Setup()
            {
                core = new ProtoCore.Core(new ProtoCore.Options());
                core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
                core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));
            }

            [Test]
            public void ExternCallTest()
            {

               //String code =
/*@"external (""ffi_library"") def sum_all : double (arr : double[], numElems : int);

def sum_all_2 : double (arr : double[], numElems : int)
{ return = 42;

}

arr = {1.0, 2, 3, 4, 5, 6, 7, 8, 9, 10};

sum_1_to_10 = sum_all_2(arr, 10);";
*/

                String code =
@"external (""ffi_library"") def sum_all : double (arr : double[], numElems : int);


arr = {1.0, 2, 3, 4, 5, 6, 7, 8, 9, 10};

sum_1_to_10 = sum_all(arr, 10);";
                


                ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();
                ExecutionMirror mirror = fsr.Execute(code, core);

                Obj o = mirror.GetValue("sum_1_to_10");
                Assert.IsTrue((Int64)o.Payload == 55);
            }

        }
    }
