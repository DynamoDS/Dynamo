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
            public static RuntimeMirror Reflect(string varname, int blockDecl, ProtoCore.Core core)
            {
                return new RuntimeMirror(varname, blockDecl, core);
            }

            
            /// <summary>
            ///  Returns class mirror that can be reflected upon
            ///  The ClassMirror is intended to be used at statically at compile time
            /// </summary>
            /// <returns></returns>
            public static ClassMirror Reflect(string className, ProtoCore.Core core)
            {
                return new ClassMirror(className, core);
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
