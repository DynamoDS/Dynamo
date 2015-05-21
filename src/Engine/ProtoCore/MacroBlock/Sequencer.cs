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
        public MacroblockSequencer()
        {
        }

        /// <summary>
        /// Begin excution of macroblocks
        /// </summary>
        public void Execute(ProtoCore.DSASM.Executive executive, List<ProtoCore.Runtime.MacroBlock> macroBlocks)
        {
            Validity.Assert(macroBlocks != null);
            List<ProtoCore.Runtime.MacroBlock> validBlocks = GetExecutingBlocks(macroBlocks);
            foreach (ProtoCore.Runtime.MacroBlock macroBlock in validBlocks)
            {
                // Assert that the executive is setup for execution
                bool isExecutiveSetup = true;
                Validity.Assert(isExecutiveSetup);

                //executive.Execute(macroBlock.BlockID)
            }
        }

        /// <summary>
        /// Get all macroblocks that can be executed
        /// </summary>
        /// <returns></returns>
        private List<ProtoCore.Runtime.MacroBlock> GetExecutingBlocks(List<ProtoCore.Runtime.MacroBlock> macroBlocks)
        {
            Validity.Assert(macroBlocks != null);
            List<ProtoCore.Runtime.MacroBlock> validBlocks = new List<Runtime.MacroBlock>();
            foreach (ProtoCore.Runtime.MacroBlock block in macroBlocks)
            {
                validBlocks.Add(block);
            }
            return validBlocks;
        }

        /// <summary>
        /// Determines if a block is ready for execution
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        private bool IsBlockReady(ProtoCore.Runtime.MacroBlock block)
        {
            return true;
        }
    }
}

 