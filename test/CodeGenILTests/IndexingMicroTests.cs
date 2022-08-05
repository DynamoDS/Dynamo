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
        [Category("Failure")]//fails because we need to emit multiple ldelem opcodes per index or fallback to replication.
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
            Assert.AreEqual(5, output.Values.ToList()[3][0]);
        }

        [Test]
        [Category("Failure")]//to accomplish this either need to generate a function call, or emit conditional il.
        public void IndexIntoRange_WithNegativeIndex_Ident()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
a = [0,1,2,3,4,5,6,7,8,9,10,11,12];
b = -5;
c=a[b];";
            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            Assert.AreEqual(8, output.Values.ToList()[2][0]);
        }
        [Test]
        public void IndexIntoArray_StringArray()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
a = [""A"",""B"",""C""];
b=a[2];";
            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            Assert.AreEqual("C", output["b"][0]);
        }
        [Test]
        public void Simple_StringOutputTests()
        {
            var dscode = @"
a = ""AAA"";";
            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            Assert.AreEqual("AAA", output["a"][0]);
            Assert.IsInstanceOf<string>(output["a"][0]);
        }
        [Test]
        public void IndexIntoArray_ObjectArray()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
import(""FFITarget.dll"");

a = [FFITarget.DummyPoint2D.ByCoordinates(0,0),FFITarget.DummyPoint2D.ByCoordinates(1,1),FFITarget.DummyPoint2D.ByCoordinates(2,2)];
b = 2;
c=a[b];";
            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            //TODO unclear why this index result has an extra level of nesting in the output dictionary. Replication maybe?
            Assert.AreEqual(2, (output.Values.ToList()[2][0] as dynamic)[0].X);
        }
        #endregion
        #region IList
        [Test]
        public void IndexIntoRange_WithIdent()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
a = 0..100..10; //a is an ILIST because of replication?
b = 5;
c=a[b];";
            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            Assert.AreEqual(50, output["c"][0]);
        }
        #endregion
        #region dictionary
        [Test]
        [Category("Failure")]//this is failing because key and val are single items and dictionaries are initialized with lists of keys and vals.
        //once that is resolved it will fail because we don't have enough type info (wrapped by function call logic IList)
        public void IndexIntoDict_WithIdent()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
a = {""key"":""val""};
b = ""key"";
c=a[b];";
            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            Assert.AreEqual("val", output.Values.ToList()[2][0]);
        }
        [Test]
        public void IndexIntoDict_WithIdent2()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
a = DesignScript.Builtin.Dictionary.ByKeysValues([""key1"",""key2""],[""val"",""val2""]);
b = ""key1"";
c=a[b];";
            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            Assert.AreEqual("val", output["c"][0]);
        }
        #endregion
    }
}
