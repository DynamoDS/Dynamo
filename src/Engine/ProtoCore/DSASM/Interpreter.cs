using System.Collections.Generic;

namespace ProtoCore.DSASM
{
    public class Interpreter
    {
        public Executive runtime;

        public Interpreter(RuntimeCore runtimeCore, bool isFEP = false)
        {
            runtime = runtimeCore.ExecutiveProvider.CreateExecutive(runtimeCore, isFEP);
        }
        
        public void Push(StackValue val)
        {
            runtime.rmem.Push(val);
        }

        public StackValue Run(int codeblock = Constants.kInvalidIndex, int entry = Constants.kInvalidIndex, Language lang = Language.NotSpecified, List<Instruction> breakpoints = null)
        {
            runtime.RX = StackValue.Null;
            runtime.Execute(codeblock, entry, breakpoints, lang);
            return runtime.RX;
        }
    }
}
