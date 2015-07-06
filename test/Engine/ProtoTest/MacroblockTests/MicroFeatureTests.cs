using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
using ProtoTest.TD;
using ProtoTestFx.TD;
namespace ProtoTest.Macroblocks
{
    public class MicroFeatureTests : ProtoTestBase
    {
        [Test]
        public void TestSimpleParallel01()
        {
            const string code = @"
a = 1;
b = 2;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1);
            thisTest.Verify("b", 2);
        }

      
    }
}