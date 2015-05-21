using System;
using System.Text;
using System.Collections.Generic;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Utils;
using ProtoCore.DSASM;


namespace ProtoCore.CompileTime
{
    public class MacroBlock
    {
    }

}

namespace ProtoCore.Runtime 
{
    public class MacroBlock
    {
        public enum ExecuteState
        {
            Ready,
            Executing,
            Paused
        }

        public ExecuteState State { get; set; }
        InstructionStream IStream { get; set; }
    }
}

 