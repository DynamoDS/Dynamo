using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTest.TD;
using ProtoTestFx.TD;
namespace ProtoTest.Assembler
{
    public class MicroFeatureTests
    {
        public ProtoCore.Core core;
        public TestFrameWork thisTest = new TestFrameWork();
        // private string AsmFilePath = @"..\..\..\Scripts\ASMScript\";
        [SetUp]
        public void Setup()
        {
            Console.WriteLine("Setup");
            core = new ProtoCore.Core(new ProtoCore.Options());
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));
        }
        /*        
[Test]        public void Simple_Push_Pop()        {            ExecutionMirror mirror =                 ProtoAssembler.Runner.TestLoadAndExecute(AsmFilePath + "Simple_Push_Pop.dsASM");            thisTest.Verify(mirror, "x", 10);            thisTest.Verify(mirror, "y", 10);            thisTest.Verify(mirror, "z", 30);        }        
[Test]        public void Simple_Jmp()        {            ExecutionMirror mirror =                ProtoAssembler.Runner.TestLoadAndExecute(AsmFilePath + "Simple_Jmp.dsASM");            thisTest.Verify(mirror, "x", 5);        }        
[Test]        public void Array_Access()        {            ExecutionMirror mirror =                ProtoAssembler.Runner.TestLoadAndExecute(AsmFilePath + "Array_Access.dsASM");            thisTest.Verify(mirror, "a", 4);        }        
[Test]        public void Sort_3_Numbers()        {            ExecutionMirror mirror =                ProtoAssembler.Runner.TestLoadAndExecute(AsmFilePath + "Sort_3_Numbers.dsASM");            thisTest.Verify(mirror, "x", 1);            thisTest.Verify(mirror, "y", 3);            thisTest.Verify(mirror, "z", 5);        }        
[Test]        public void Arithmatic_Logic()        {            ExecutionMirror mirror =                ProtoAssembler.Runner.TestLoadAndExecute(AsmFilePath + "Arithmatic_Logic.dsASM");            thisTest.Verify(mirror, "x", 0);            thisTest.Verify(mirror, "y", 1);            thisTest.Verify(mirror, "z", 10.0);        }         * */
    }
}
