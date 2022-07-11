using NUnit.Framework;
using ProtoCore.Utils;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ProtoCore.AST.AssociativeAST;

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

        //TODO currently this test fails because it relies on functions defined in DSCoreNodes.dll
        //but import nodes are not yet implemeted in the MSIL compiler / runtime.
        [Test]
        [Category("Failure")]
        public void RangeTests()
        {
            var dscode = @"
import(""DSCoreNodes.dll"");
0..10;";

            var ast = ParserUtils.Parse(dscode).Body;
            var output = codeGen.EmitAndExecute(ast);
        } 
       
    }
}
