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

        [Test]
        public void TestSimpleParallel02()
        {
            const string code = @"
a = 1;
b = 2;

x = 3;
y = x;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1);
            thisTest.Verify("b", 2);
            thisTest.Verify("x", 3);
            thisTest.Verify("y", 3);
        }

        [Test]
        public void TestInputblock01()
        {
            const string code = @"
a = 1;
b = 2;
c = a + b;
";
            thisTest.RunScriptSource(code);
            thisTest.Verify("a", 1);
            thisTest.Verify("b", 2);
            thisTest.Verify("c", 3);
        }
 
    }
}