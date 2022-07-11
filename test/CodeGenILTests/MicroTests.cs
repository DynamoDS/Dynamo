using NUnit.Framework;
using ProtoCore.Utils;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ProtoCore.AST.AssociativeAST;
using System.Linq;

namespace CodeGenILTests
{
    [TestFixture]
    public class MicroTests
    {
        private EmitMSIL.CodeGenIL codeGen;
        private Dictionary<string, IList> inputs = new Dictionary<string, IList>();

        [SetUp]
        public void Setup()
        {
            var assemblyPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            codeGen = new EmitMSIL.CodeGenIL(inputs, Path.Combine(assemblyPath, "OpCodesTEST.txt"));
        }

        [Test]
        public void RangeTestInts()
        {
            var dscode = @"
import(""DSCoreNodes.dll"");
0..10;";

            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            CollectionAssert.AreEqual(new object[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9,10 }, output.Values.ToList().First()[0] as object[]);
        }
        [Test]
        public void RangeTestDouble()
        {
            var dscode = @"
import(""DSCoreNodes.dll"");
0..1.1..0.1;";

            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
            Assert.IsNotEmpty(output);
            CollectionAssert.AreEqual(new object[] { 0, .1, .2, .3, .4, .5, .6, .7, .8, .9, 1.0,1.1 }, output.Values.ToList().First()[0] as object[]);
        }

    }
}
