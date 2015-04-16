using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoCore;

namespace ProtoScript.Utils
{
    public static class ProtoScriptUtils
    {


        public static ProtoCore.Core CreateCore(Options coreOptions,ProtoCore.Core runnerCore)
        {
            runnerCore = new ProtoCore.Core(coreOptions);
            runnerCore.Compilers.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Compiler(runnerCore));
            runnerCore.Compilers.Add(ProtoCore.Language.kImperative, new ProtoImperative.Compiler(runnerCore));
            return runnerCore;
       }
    
    }
}
