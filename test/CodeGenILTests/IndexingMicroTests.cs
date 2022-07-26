using NUnit.Framework;
using ProtoCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenILTests
{
    [TestFixture]
    public class IndexingMicroTests : MicroTests
    {
        [Test]
        public void IndexArrayAllConstants()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
b=[0,1,2,3,4,5][4];";
            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            Assert.AreEqual(4, output.Values.ToList()[0][0]);
        }
        [Test]
        public void IndexIdentArrayWithConstant()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
a = [0,1,2,3,4,5];
b=a[5];";
            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            Assert.AreEqual(5, output.Values.ToList()[1][0]);
        }
        [Test]
        public void IndexIdentArrayWithIdent()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
a = [0,1,2,3,4,5];
b = 5;
c=a[b];";
            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            Assert.AreEqual(5, output.Values.ToList()[1][0]);
        }
        [Test]
        public void IndexArrayWithArrayConstants()
        {
            var dscode = @"
        import(""DesignScriptBuiltin.dll"");
        a = [0,1,2,3,4,5];
        b=a[[0,5]];";
            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            CollectionAssert.AreEqual(new long[] { 0, 5 }, output.Values.ToList()[1][0] as long[]);
        }
    }
}
