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
        protected override void GetLibrariesToPreload(ref List<string> libraries)
        {
            base.GetLibrariesToPreload(ref libraries);
            libraries.Add("FFITarget.dll");
        }

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
            Assert.AreEqual(4, output.Values.ToList()[0]);
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
            Assert.AreEqual(5, output.Values.ToList()[1]);
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
            Assert.AreEqual(5, output.Values.ToList()[2]);
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
            Assert.AreEqual(5.0, output.Values.ToList()[2]);
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
            Assert.AreEqual(1.1, output.Values.ToList()[2]);
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
            CollectionAssert.AreEqual(new long[] { 0, 5 }, output.Values.ToList()[1] as long[]);
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
            Assert.AreEqual(3, output.Values.ToList()[3]);
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
            Assert.AreEqual(5, output.Values.ToList()[3]);
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
            Assert.AreEqual(8, output.Values.ToList()[2]);
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
            Assert.AreEqual("C", output["b"]);
        }
        [Test]
        public void Simple_StringOutputTests()
        {
            var dscode = @"
a = ""AAA"";";
            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            Assert.AreEqual("AAA", output["a"]);
            Assert.IsInstanceOf<string>(output["a"]);
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
            Assert.AreEqual(2, (output.Values.ToList()[2] as dynamic).X);
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
            Assert.AreEqual(50, output["c"]);
        }
        #endregion
        #region dictionary
        [Test]
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
            Assert.AreEqual("val", output.Values.ToList()[2]);
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
            Assert.AreEqual("val", output["c"]);
        }
        #endregion

        #region replicated_function_call
        //TODO_MSIL - this test fails when enabling direct function call validation in ReplicationLogic - wrong ValueAtIndex overload is checked for replication
        [Test]
        public void IndexIntoReplicatedFunctionCall_IntegerArray1()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
a=[-1,-2,-3];
b = DSCore.Math.Abs(a);
c = b[0];";
            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            Assert.AreEqual(1, output["c"]);
        }

        //TODO_MSIL - this test fails when enabling direct function call validation in ReplicationLogic - wrong ValueAtIndex overload is checked for replication
        [Test]
        public void IndexIntoReplicatedFunctionCall_IntegerArray2()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
a=[-1,-2,-3];
b = DSCore.Math.Abs(a)[0];";
            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            Assert.AreEqual(1, output["b"]);
        }

        //TODO_MSIL - this test fails when enabling direct function call validation in ReplicationLogic - wrong ValueAtIndex overload is checked for replication
        [Test]
        public void IndexIntoReplicatedFunctionCall_IntegerArray3()
        {
            var dscode = @"
import(""DesignScriptBuiltin.dll"");
a = DSCore.Math.Abs([-1,-2,-3])[0];";
            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            Assert.AreEqual(1, output["a"]);
        }
        #endregion
    }
}
