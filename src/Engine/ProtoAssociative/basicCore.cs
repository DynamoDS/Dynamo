using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoImperative;

namespace ProtoAssociative
{
    public static class BasicCore
    {
        public static ProtoCore.Core CreateCore()
        {
            ProtoCore.Core core = new ProtoCore.Core();
            core.Compilers.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Compiler(core));
            core.Compilers.Add(ProtoCore.Language.kImperative, new ProtoImperative.Compiler(core));
            return core;
        }
    }
}
