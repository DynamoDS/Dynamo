using System;
using System.Collections.Generic;
using System.Diagnostics;
using ProtoCore.DSASM;
using ProtoCore.Utils;

namespace ProtoCore.Diagnostics
{
    /// <summary>
    /// Implements functionality that peeks into the state of the RuntimeCore 
    /// </summary>
    public class Runtime
    {
        private RuntimeCore runtimeCore = null;

        public Runtime(RuntimeCore rtCore)
        {
            Validity.Assert(rtCore != null);
            this.runtimeCore = rtCore;
        }

        /// <summary>
        /// Gets the number of executable instructions from the instruction streams
        /// the returned instruction count includes all instructions whether they are reachable or not
        /// </summary>
        /// <returns></returns>
        public int GetExecutableInstructionCount(int macroBlockId = 0)
        {
            Validity.Assert(runtimeCore != null);
            Executable exe = runtimeCore.DSExecutable;
            int cnt = 0;
            InstructionStream[] istreamList = exe.GetInstructionStreamList(macroBlockId);
            foreach (InstructionStream istream in istreamList)
            {
                cnt += istream.instrList.Count;
            }
            return cnt;
        }
    }
}
