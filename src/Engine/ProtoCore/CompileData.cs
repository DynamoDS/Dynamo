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
    /// The CompileCore is an object required by the compiler front-end (Lexer,Parser,Codegen)
    /// It is instantiated prior to compiling a body of code.
    /// The primary purpose of CompileCore is to store compile-time information that is consumed within the code generators
    /// </summary>
    public class CompileData
    {
        public CompileData()
        {
        }
    }
}
