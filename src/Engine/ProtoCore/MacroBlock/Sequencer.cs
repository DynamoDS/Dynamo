using System;
using System.Text;
using System.Collections.Generic;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Utils;
using ProtoCore.DSASM;

namespace ProtoCore.Runtime 
{
    public class MacroblockSequencer
    {
        private List<ProtoCore.Runtime.MacroBlock> macroBlockList = null;

        public MacroblockSequencer(List<Runtime.MacroBlock> macroBlocks)
        {
            macroBlockList = macroBlocks;
        }

        private void SetupExecutive(
            ProtoCore.DSASM.Executive executive,
            int exeblock, 
            int entry, 
            StackFrame stackFrame, int locals = 0)
        {
            //executive.SetupBounce(exeblock, entry, stackFrame, locals);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Begin excution of macroblocks
        /// </summary>
        public void Execute(
            ProtoCore.DSASM.Executive executive,
            int exeblock,
            int entry,
            StackFrame stackFrame, 
            int locals = 0
            )
        {
            Validity.Assert(executive != null);
            Validity.Assert(macroBlockList != null);

            executive.SetupBounce(exeblock, entry, stackFrame, locals);
            foreach (ProtoCore.Runtime.MacroBlock macroBlock in macroBlockList)
            {
                executive.Execute(macroBlock);
            }
        }

        /// <summary>
        /// Determines if a block is ready for execution
        /// A block is ready if all its operands have executed
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        private bool IsBlockReady(ProtoCore.Runtime.MacroBlock block)
        {
            Validity.Assert(macroBlockList != null);
            return true;
        }
    }
}

 