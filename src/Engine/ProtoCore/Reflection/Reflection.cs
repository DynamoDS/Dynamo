using System.Collections.Generic;

namespace ProtoCore
{
    namespace Mirror
    {
        public static class Reflection
        {

            /// <summary>
            ///  Returns a runtime mirror that can be reflected upon
            /// </summary>
            /// <returns></returns>
            public static RuntimeMirror Reflect(string varname, int blockDecl, ProtoCore.RuntimeCore runtimeCore, ProtoCore.Core core)
            {
                return new RuntimeMirror(varname, blockDecl, runtimeCore, core);
            }

            /// <summary>
            ///  Returns a library mirror that can be reflected upon
            ///  The LibraryMirror is  used for static reflection of classes etc.
            /// </summary>
            /// <returns></returns>
            public static LibraryMirror Reflect(string assemblyName, IList<ProtoCore.DSASM.ClassNode> classNodes, ProtoCore.Core core)
            {
                return new LibraryMirror(core, assemblyName, classNodes);
            }
        }
    }
}
