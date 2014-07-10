using System;
using System.Collections.Generic;
using System.Text;
using ProtoCore.Lang;
using ProtoCore.Utils;
using ProtoCore.DSASM;

namespace ProtoCore
{
    namespace CompileTime
    {
        public class Context
        {
            public Guid guid { get; set; } 
            public string SourceCode { get; private set; }
            public Dictionary<string, Object> GlobalVarList { get; private set; }
            public Dictionary<string, bool> execFlagList { get; private set; }
            public Dictionary<int, bool> exprExecutionFlags { get; set; }
            public SymbolTable symbolTable { get; set; }

            /// <summary>
            /// This flag controls whether we want a full codeblock to apply SSA Transform.
            /// Currently it is used to prevent SSA on inline conditional bodies. 
            /// This will be resolved when inline replication is fixed
            /// </summary>
            public bool applySSATransform { get; set; }

            public Context()
            {
                SourceCode = String.Empty;
                GlobalVarList = null;
                execFlagList = null;
                symbolTable = null;
                exprExecutionFlags = new Dictionary<int, bool>();
                applySSATransform = true;
            }
            
            public void SetData(string source, Dictionary<string, Object> context, Dictionary<string, bool> flagList)
            {
                SourceCode = source;
                GlobalVarList = context;
                execFlagList = flagList;
                exprExecutionFlags = new Dictionary<int, bool>();
                applySSATransform = true;
            }

            public Context(string source, Dictionary<string, Object> context = null, Dictionary<string, bool> flagList = null)
            {
                GlobalVarList = context;

                Validity.Assert(null != source && String.Empty != source);
                SourceCode = source;

                execFlagList = flagList;
                symbolTable = null;
                exprExecutionFlags = new Dictionary<int, bool>();
                applySSATransform = true;
            }
        }
    }
}