using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using ProtoCore.DSASM.Mirror;
using System.Text.RegularExpressions;
using ProtoCore.Utils;
using System.IO;
using ProtoCore.DSASM;
using ProtoCore.AST.AssociativeAST;
using ProtoCore;

namespace GraphToDSCompiler
{
    public struct kw
    {
        public const string tempPrefix = "temp";
    }

    public struct Constants
    {
        public const uint TempMask = 0x80000000;
        public const uint UIDStart = 10000;
        public const char ReplicationGuideDelimiter = '¡';

        public const string kwTempNull = "temp_NULL";
    }

    public class GraphUtilities
    {
        private static void SetParsingFlagsForCore(ProtoCore.Core core, bool isCodeBlockNode = false, bool isPreloadedAssembly = false)
        {
            // Reuse the core for every succeeding run
            // TODO Jun: Check with UI - what instances need a new core and what needs reuse
            core.ResetForPrecompilation();
            core.IsParsingPreloadedAssembly = isPreloadedAssembly;
            core.IsParsingCodeBlockNode = isCodeBlockNode;
            core.ParsingMode = ProtoCore.ParseMode.AllowNonAssignment;
        }
   }
}
