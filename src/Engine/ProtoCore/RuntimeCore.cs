using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Linq;
using ProtoCore.AssociativeGraph;
using ProtoCore.AssociativeEngine;
using ProtoCore.AST;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.BuildData;
using ProtoCore.CodeModel;
using ProtoCore.DebugServices;
using ProtoCore.DSASM;
using ProtoCore.Lang;
using ProtoCore.Lang.Replication;
using ProtoCore.Runtime;
using ProtoCore.Utils;
using ProtoFFI;

using StackFrame = ProtoCore.DSASM.StackFrame;

namespace ProtoCore
{
    /// <summary>
    /// The RuntimeCore is an object that contains properties that is consumed only by the runtime VM
    /// It is instantiated prior to execution and is populated with information gathered from the CompileCore
    /// 
    /// The runtime VM is designed to run independently from the front-end (UI, compiler) 
    /// and the only 2 properties it needs are the RuntimeCore and the DSExecutable.
    /// 
    /// The RuntimeCore will also contain properties that are populated at runtime and consumed at runtime.
    /// </summary>
    public class RuntimeCore
    {

        #region COMPILER_GENERATED

        #endregion

        
        #region RUNTIME_GENERATED

        /// <summary>
        ///  These are the list of symbols updated by the VM after an execution cycle
        /// </summary>
        public HashSet<SymbolNode> UpdatedSymbols { get; private set; }

        #endregion

        public RuntimeCore()
        {
            InitializeProperties();
        }

        private void InitializeProperties()
        {
            UpdatedSymbols = new HashSet<SymbolNode>();
        }

    }
}
