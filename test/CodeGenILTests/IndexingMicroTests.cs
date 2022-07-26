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
        #region array indexing
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
            Assert.AreEqual(5, output.Values.ToList()[2][0]);
        }
        [Test]
        public void IndexIdentArrayWithIdent_Doubles()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
a = [0.0,1.1,2.0,3.0,4.0,5.0];
b = 5;
c=a[b];";
            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            Assert.AreEqual(5.0, output.Values.ToList()[2][0]);
        }
        [Test]
        public void IndexIdentArrayWithIdent_MixedElementTypeArray()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
a = [0.0,1.1,2.0,3.0,4.0,5.0,6,7,8,9,10];
b = 1;
c=a[b];";
            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            Assert.AreEqual(1.1, output.Values.ToList()[2][0]);
        }
        [Test]
        [Category("Failure")]
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
        [Test]
        public void IndexNestedArray_WithIdent()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
a = [[0,1,2],[3,4,5]];
b = 1;
c = 0;
d=a[b][c];";
            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            Assert.AreEqual(3, output.Values.ToList()[3][0]);
        }

        [Test]
        [Category("Failure")]//this fails because we can't index into an object
                             //and [0..10] is assumed to be an object[] because it's generated with a function call.
        public void IndexIntoNestedRange_WithIdent()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
a = [0..10]; //a is a []object
b = 0;
c = 5;
d=a[b][c];";
            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            Assert.AreEqual(4, output.Values.ToList()[3][0]);
        }
        [Test]
        public void IndexIntoRange_WithIdent()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
a = 0..10; //a is an ILIST
b = 5;
c=a[b];";
            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            Assert.AreEqual(4, output.Values.ToList()[2][5]);
        }
        #endregion
    }
}
