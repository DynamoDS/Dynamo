
namespace ProtoCore
{
    namespace Compiler
    {
        public enum AccessSpecifier
        {
            kPublic,
            kProtected,
            kPrivate
        }

        namespace Associative
        {
            public enum CompilePass
            {
                kClassName,
                kClassBaseClass,
                kClassHierarchy,
                kClassMemVar,

                kClassMemFuncSig,
                kGlobalFuncSig,

                kGlobalScope,

                kClassMemFuncBody,
                kGlobalFuncBody,
                kDone
            }

            public enum SubCompilePass
            {
                kNone,
                kUnboundIdentifier,
                kGlobalInstanceFunctionBody,
                kAll
            }
        }

        namespace Imperative
        {
            public enum CompilePass
            {
                kGlobalFuncSig,
                kGlobalScope,
                kGlobalFuncBody,
                kDone
            }
        }
    }
}

